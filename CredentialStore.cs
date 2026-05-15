using Microsoft.Win32;
using System;
using System.Security.Cryptography;
using System.Text;

namespace avUpload
{
    /// <summary>
    /// Speichert und liest SFTP-Zugangsdaten aus der Windows-Registry.
    /// Verschlüsselung erfolgt per Windows DPAPI (CurrentUser-Scope) –
    /// kein hardcodierter Schlüssel, maschinengebunden pro Benutzerkonto.
    /// </summary>
    internal sealed class CredentialStore : IDisposable
    {
        private const string RegistryPath = @"SOFTWARE\NASS e.K.\Avast-Whitelisting";

        private RegistryKey _key;
        private bool _disposed;

        /// <summary>
        /// Öffnet (oder erstellt) den Registry-Schlüssel.
        /// Wirft InvalidOperationException wenn der Schlüssel weder geöffnet
        /// noch erstellt werden kann.
        /// </summary>
        public CredentialStore()
        {
            _key = Registry.CurrentUser.OpenSubKey(RegistryPath, writable: true)
                ?? Registry.CurrentUser.CreateSubKey(RegistryPath);

            if (_key == null)
                throw new InvalidOperationException(
                    $"Registry-Schlüssel '{RegistryPath}' konnte nicht geöffnet oder erstellt werden.");
        }

        /// <summary>True wenn der Schlüssel existiert und lesbar ist.</summary>
        public bool IsAvailable => _key != null;

        // ------------------------------------------------------------------ //
        //  Public API
        // ------------------------------------------------------------------ //

        public string LoadUri()       => Load("Uri");
        public string LoadUsername()  => Load("Username");
        public string LoadPassword()  => Load("Password");
        public string LoadEmail()     => Load("Email");

        public void Save(string uri, string username, string password, string email)
        {
            Store("Uri",      uri);
            Store("Username", username);
            Store("Password", password);
            Store("Email",    email);
        }

        // ------------------------------------------------------------------ //
        //  Private helpers
        // ------------------------------------------------------------------ //

        private void Store(string valueName, string plainText)
        {
            EnsureNotDisposed();

            if (plainText == null)
                throw new ArgumentNullException(nameof(plainText));

            byte[] plainBytes    = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = ProtectedData.Protect(
                plainBytes,
                optionalEntropy: null,
                scope: DataProtectionScope.CurrentUser);

            _key.SetValue(valueName, Convert.ToBase64String(encryptedBytes),
                          RegistryValueKind.String);
        }

        private string Load(string valueName)
        {
            EnsureNotDisposed();

            string stored = _key.GetValue(valueName) as string;

            // Noch kein Wert gespeichert → leeren String zurückgeben,
            // nicht null, damit TextBoxen nicht abstürzen.
            if (string.IsNullOrEmpty(stored))
                return string.Empty;

            // Rückwärtskompatibilität: alte Werte waren DES-verschlüsselt
            // und sind Base64, aber kürzer als ein DPAPI-Blob.
            // Schlägt die Entschlüsselung fehl, geben wir einen leeren
            // String zurück und lassen den Nutzer neu eingeben + speichern.
            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(stored.Replace(" ", "+"));
                byte[] plainBytes     = ProtectedData.Unprotect(
                    encryptedBytes,
                    optionalEntropy: null,
                    scope: DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (CryptographicException)
            {
                // Alter DES-Wert oder auf anderem Rechner gespeichert →
                // Feld leer lassen, Nutzer muss neu speichern.
                return string.Empty;
            }
            catch (FormatException)
            {
                // Kein gültiges Base64 → ignorieren.
                return string.Empty;
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(CredentialStore));
        }

        // ------------------------------------------------------------------ //
        //  IDisposable
        // ------------------------------------------------------------------ //

        public void Dispose()
        {
            if (!_disposed)
            {
                _key?.Dispose();
                _key = null;
                _disposed = true;
            }
        }
    }
}
