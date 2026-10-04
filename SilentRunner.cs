using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace avUpload
{
    /// <summary>
    /// Führt ZIP-Erstellung und SFTP-Upload ohne GUI aus.
    /// Fortschritt und Fehler gehen auf stdout/stderr.
    /// Exit-Codes: 0 = OK, 1 = Fehler, 2 = Abgebrochen.
    /// </summary>
    internal static class SilentRunner
    {
        [DllImport("kernel32.dll")] private static extern bool AttachConsole(int dwProcessId);
        [DllImport("kernel32.dll")] private static extern bool AllocConsole();

        public static async Task<int> RunAsync(string[] args)
        {
            // Konsole anhängen (WinExe hat standardmäßig keine)
            if (!AttachConsole(-1))
                AllocConsole();

            // ---------------------------------------------------------------- //
            //  Argumente parsen
            // ---------------------------------------------------------------- //

            string email = null;
            var files = new System.Collections.Generic.List<string>();

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--email" || args[i] == "-e")
                {
                    if (i + 1 < args.Length)
                        email = args[++i];
                }
                else if (!args[i].StartsWith("-"))
                {
                    files.Add(args[i]);
                }
            }

            if (files.Count == 0)
            {
                Console.Error.WriteLine(Properties.Resources.SilentErrorNoFiles);
                Console.Error.WriteLine(Properties.Resources.SilentUsage);
                return 1;
            }

            // Nicht-existierende Dateien früh melden
            foreach (string f in files)
            {
                if (!File.Exists(f))
                {
                    Console.Error.WriteLine(
                        string.Format(Properties.Resources.SilentErrorFileNotFound, f));
                    return 1;
                }
            }

            // ---------------------------------------------------------------- //
            //  Credentials laden
            // ---------------------------------------------------------------- //

            CredentialStore creds;
            try
            {
                creds = new CredentialStore();
            }
            catch (InvalidOperationException ex)
            {
                Console.Error.WriteLine(
                    string.Format(Properties.Resources.SilentErrorRegistry, ex.Message));
                return 1;
            }

            // E-Mail: Argument hat Vorrang, sonst Registry
            if (string.IsNullOrWhiteSpace(email))
                email = creds.LoadEmail();

            if (string.IsNullOrWhiteSpace(email))
            {
                Console.Error.WriteLine(Properties.Resources.SilentErrorNoEmail);
                Console.Error.WriteLine(Properties.Resources.SilentUsage);
                creds.Dispose();
                return 1;
            }

            // ---------------------------------------------------------------- //
            //  ZIP erstellen
            // ---------------------------------------------------------------- //

            string zipPath = null;
            try
            {
                zipPath = CreateZip(email, files);
                Console.WriteLine(
                    string.Format(Properties.Resources.SilentZipCreated,
                                  Path.GetFileName(zipPath)));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    string.Format(Properties.Resources.SilentErrorZip, ex.Message));
                TryDelete(zipPath);
                creds.Dispose();
                return 1;
            }

            // ---------------------------------------------------------------- //
            //  Upload
            // ---------------------------------------------------------------- //

            using (var cts = new CancellationTokenSource())
            {
                // Ctrl+C sauber abfangen
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

                try
                {
                    var progress = new Progress<long>(bytes =>
                    {
                        Console.Write("\r" +
                            string.Format(Properties.Resources.SilentUploading,
                                          bytes / 1024));
                    });

                    await SftpService.UploadAsync(
                        zipPath,
                        creds.LoadUri(),
                        creds.LoadUsername(),
                        creds.LoadPassword(),
                        progress,
                        cts.Token);

                    Console.WriteLine();  // Zeilenumbruch nach Fortschrittszeile
                    Console.WriteLine(Properties.Resources.SilentUploadSuccess);
                    return 0;
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine();
                    Console.Error.WriteLine(Properties.Resources.SilentUploadCancelled);
                    return 2;
                }
                catch (SftpUploadException ex)
                {
                    Console.WriteLine();
                    Console.Error.WriteLine(
                        string.Format(Properties.Resources.SilentErrorSftp, ex.Message));
                    return 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.Error.WriteLine(
                        string.Format(Properties.Resources.SilentErrorUnexpected, ex.Message));
                    return 1;
                }
                finally
                {
                    TryDelete(zipPath);
                    creds.Dispose();
                }
            }
        }

        // -------------------------------------------------------------------- //
        //  Hilfsmethoden
        // -------------------------------------------------------------------- //

        private static string CreateZip(string email, System.Collections.Generic.List<string> files)
        {
            string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string zipPath = Path.Combine(
                Path.GetTempPath(),
                string.Format("{0}_{1}.zip", email, timeStamp));

            using (var zipStream = File.Open(zipPath, FileMode.Create))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                foreach (string source in files)
                    archive.CreateEntryFromFile(source, Path.GetFileName(source));
            }

            return zipPath;
        }

        private static void TryDelete(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            try { File.Delete(path); } catch { /* ignorieren */ }
        }
    }
}
