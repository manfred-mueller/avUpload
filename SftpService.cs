using Renci.SshNet;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace avUpload
{
    /// <summary>
    /// Kapselt den SFTP-Upload.
    /// – Kein byte[]-Umweg: direktes Streaming aus dem FileStream
    /// – Fortschritt via IProgress&lt;long&gt; (übertragene Bytes)
    /// – Verbindungs-Timeout konfigurierbar
    /// – Unterstützt Abbruch per CancellationToken
    /// </summary>
    internal static class SftpService
    {
        /// <summary>
        /// Lädt <paramref name="localFilePath"/> auf den SFTP-Server hoch.
        /// </summary>
        /// <param name="localFilePath">Vollständiger Pfad zur lokalen Datei.</param>
        /// <param name="sftpUrl">
        ///   Host (und optionaler Pfad) ohne Protokoll-Präfix,
        ///   z.B. "whitelisting.avast.com:22/incoming" oder "whitelisting.avast.com/data".
        /// </param>
        /// <param name="username">SFTP-Benutzername.</param>
        /// <param name="password">SFTP-Passwort.</param>
        /// <param name="progress">
        ///   Optional: Callback der die bisher übertragenen Bytes meldet.
        /// </param>
        /// <param name="cancellationToken">Abbruch-Token.</param>
        /// <exception cref="OperationCanceledException">
        ///   Wenn der Upload vor dem Verbindungsaufbau abgebrochen wird.
        /// </exception>
        /// <exception cref="SftpUploadException">
        ///   Bei SFTP-Verbindungsfehlern, fehlendem Verzeichnis oder Upload-Fehlern.
        /// </exception>
        public static async Task UploadAsync(
            string localFilePath,
            string sftpUrl,
            string username,
            string password,
            IProgress<long> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(localFilePath))
                throw new ArgumentNullException(nameof(localFilePath));
            if (!File.Exists(localFilePath))
                throw new FileNotFoundException("Lokale Datei nicht gefunden.", localFilePath);
            if (string.IsNullOrWhiteSpace(sftpUrl))
                throw new ArgumentNullException(nameof(sftpUrl));

            // URL parsen (sftp:// voranstellen damit Uri sie versteht)
            Uri uri = ParseSftpUrl(sftpUrl);

            string host            = uri.Host;
            int    port            = uri.Port > 0 ? uri.Port : 22;
            string remoteDirectory = uri.AbsolutePath.TrimStart('/');

            cancellationToken.ThrowIfCancellationRequested();

            var authMethod     = new PasswordAuthenticationMethod(username, password);
            var connectionInfo = new ConnectionInfo(host, port, username, authMethod)
            {
                Timeout = TimeSpan.FromSeconds(30)   // Verbindungs-Timeout
            };

            using (var client = new SftpClient(connectionInfo))
            {
                // Verbindung im Thread-Pool aufbauen (SshNet ist synchron)
                await Task.Run(() => client.Connect(), cancellationToken)
                          .ConfigureAwait(false);

                if (!client.IsConnected)
                    throw new SftpUploadException(
                        Properties.Resources.FailedToConnectToTheSFTPServer);

                cancellationToken.ThrowIfCancellationRequested();

                // Verzeichnis prüfen
                string remoteDir = "/" + remoteDirectory;
                bool dirExists   = await Task.Run(
                    () => client.Exists(remoteDir), cancellationToken)
                    .ConfigureAwait(false);

                if (!dirExists)
                    throw new SftpUploadException(
                        string.Format(
                            Properties.Resources.Directory0DoesNotExistOnTheServer,
                            remoteDirectory));

                string remotePath = remoteDir.TrimEnd('/') + "/" + Path.GetFileName(localFilePath);

                // Datei streamen – kein byte[] im Speicher
                using (var fs = new FileStream(
                    localFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 81920,
                    useAsync: true))
                {
                    var reportingStream = progress != null
                        ? new ProgressStream(fs, progress)
                        : (Stream)fs;

                    await Task.Run(
                        () => client.UploadFile(reportingStream, remotePath, canOverride: true),
                        cancellationToken)
                        .ConfigureAwait(false);
                }

                await Task.Run(() => client.Disconnect(), cancellationToken)
                          .ConfigureAwait(false);
            }
        }

        // ------------------------------------------------------------------ //
        //  Hilfsmethoden
        // ------------------------------------------------------------------ //

        private static Uri ParseSftpUrl(string input)
        {
            // Sicherstellen dass ein Schema vorhanden ist
            string withScheme = input.StartsWith("sftp://", StringComparison.OrdinalIgnoreCase)
                ? input
                : "sftp://" + input;

            if (!Uri.TryCreate(withScheme, UriKind.Absolute, out Uri uri) ||
                string.IsNullOrEmpty(uri.Host))
            {
                throw new SftpUploadException($"Ungültige SFTP-URL: '{input}'");
            }

            return uri;
        }
    }

    // ---------------------------------------------------------------------- //
    //  Hilfsklasse: meldet Fortschritt während des Streamings
    // ---------------------------------------------------------------------- //

    internal sealed class ProgressStream : Stream
    {
        private readonly Stream       _inner;
        private readonly IProgress<long> _progress;
        private long _bytesRead;

        public ProgressStream(Stream inner, IProgress<long> progress)
        {
            _inner    = inner    ?? throw new ArgumentNullException(nameof(inner));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
        }

        public override bool CanRead  => _inner.CanRead;
        public override bool CanSeek  => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length   => _inner.Length;

        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int n = _inner.Read(buffer, offset, count);
            if (n > 0)
            {
                _bytesRead += n;
                _progress.Report(_bytesRead);
            }
            return n;
        }

        public override void Flush()                              => _inner.Flush();
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value)                => _inner.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            if (disposing) _inner.Dispose();
            base.Dispose(disposing);
        }
    }

    // ---------------------------------------------------------------------- //
    //  Eigener Exception-Typ
    // ---------------------------------------------------------------------- //

    [Serializable]
    public class SftpUploadException : Exception
    {
        public SftpUploadException(string message) : base(message) { }
        public SftpUploadException(string message, Exception inner) : base(message, inner) { }
    }
}
