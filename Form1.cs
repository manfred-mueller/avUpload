using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace avUpload
{
    public partial class Form1 : Form
    {
        // ------------------------------------------------------------------ //
        //  Felder
        // ------------------------------------------------------------------ //

        private CredentialStore _credentials;
        private string          _zipUpload;         // Pfad zur aktuell erzeugten ZIP
        private CancellationTokenSource _uploadCts; // für Abbruch-Button

        // ------------------------------------------------------------------ //
        //  Konstruktor
        // ------------------------------------------------------------------ //

        public Form1(string[] args)
        {
            InitializeComponent();

            // Versionsnummer im Titel
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            Text = $"{Properties.Resources.ProgName} {ver.Major}.{ver.Minor}.{ver.Build}";

            // Tray-Icon
            notifyIcon1.Icon    = new System.Drawing.Icon(Properties.Resources.avUpload, 48, 48);
            notifyIcon1.Visible = true;

            // Fensterposition aus Settings laden
            Location = Properties.Settings.Default.Location;

            // Immer im Vordergrund – aber nur solange kein Upload läuft
            TopMost   = true;
            AllowDrop = true;

            // Credentials laden
            try
            {
                _credentials     = new CredentialStore();
                txtUri.Text      = _credentials.LoadUri();
                txtUsername.Text = _credentials.LoadUsername();
                txtPassword.Text = _credentials.LoadPassword();
                txtEmail.Text    = _credentials.LoadEmail();
            }
            catch (Exception ex)
            {
                // Registry-Schlüssel nicht verfügbar → Info-Dialog, dann weiter
                MessageBox.Show(
                    ex.Message,
                    Properties.Resources.Error,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                // AboutBox2 zeigt Installations-Hinweise
                using (var about = new AboutBox2())
                    about.ShowDialog();
            }

            // Buttons initial deaktivieren
            btnSave.Enabled   = false;
            btnZip.Enabled    = false;
            btnUpload.Enabled = false;

            // Dateien aus Kommandozeile laden (Start > 1, Index 0 = exe selbst)
            if (args.Length > 1)
            {
                for (int i = 1; i < args.Length; i++)
                    AddFileToList(args[i]);
            }

            // Änderungs-Handler für Einstellungsfelder
            txtUri.TextChanged      += OnSettingsChanged;
            txtUsername.TextChanged += OnSettingsChanged;
            txtPassword.TextChanged += OnSettingsChanged;
            txtEmail.TextChanged    += OnSettingsChanged;

            // Verknüpfungs-Checkboxen prüfen
            string linkPath   = ShortcutPath("Desktop");
            string sendtoPath = ShortcutPath("SendTo");

            if (File.Exists(linkPath))   linkToolStripMenuItem.Checked   = true;
            if (File.Exists(sendtoPath)) sendtoToolStripMenuItem.Checked = true;
        }

        // ------------------------------------------------------------------ //
        //  Form-Events
        // ------------------------------------------------------------------ //

        private void Mainform_Load(object sender, EventArgs e)
        {
            lblStatus.Text = Properties.Resources.Done;
            // Executable vorauswählen als Standarddatei
            txtFile.Items.Add(Application.ExecutablePath);
            btnZip.Enabled = true;
        }

        private void formLoading(object sender, EventArgs e)
        {
            Location = Properties.Settings.Default.Location;
        }

        private void formClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Location = Location;
            Properties.Settings.Default.Save();
            _credentials?.Dispose();
            _uploadCts?.Dispose();
        }

        // ------------------------------------------------------------------ //
        //  Datei-Liste
        // ------------------------------------------------------------------ //

        /// <summary>Fügt eine Datei zur Liste hinzu, wenn sie existiert.</summary>
        public void LoadFile(string file)
        {
            if (File.Exists(file))
                AddFileToList(file);
        }

        private void AddFileToList(string path)
        {
            if (!txtFile.Items.Contains(path))
                txtFile.Items.Add(path);

            btnZip.Enabled = txtFile.Items.Count > 0;
        }

        private void UpdateZipButton()
        {
            btnZip.Enabled = txtFile.Items.Count > 0;
        }

        // Datei-Auswahl per Dialog
        private void btnPickFile_Click(object sender, EventArgs e)
        {
            if (ofdFile.ShowDialog() != DialogResult.OK)
                return;

            // Alle gewählten Dateien hinzufügen – kein unnötiges OpenFile()
            foreach (string file in ofdFile.FileNames)
                AddFileToList(file);
        }

        // Rechtsklick entfernt markierte Einträge
        private void txtFile_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            foreach (var item in txtFile.SelectedItems.OfType<string>().ToList())
                txtFile.Items.Remove(item);

            UpdateZipButton();
        }

        // Drag & Drop
        private void txtFile_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        private void txtFile_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Link
                : DragDropEffects.None;
        }

        private void txtFile_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
                AddFileToList(file);
        }

        // ------------------------------------------------------------------ //
        //  ZIP erstellen
        // ------------------------------------------------------------------ //

        private void btnZip_Click(object sender, EventArgs e)
        {
            if (txtFile.Items.Count == 0)
                return;

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show(
                    Properties.Resources.PleaseEnterEmail,
                    Properties.Resources.MissingInput,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string zipPath   = Path.GetTempPath();
            string newZip    = Path.Combine(zipPath, $"{txtEmail.Text}_{timeStamp}.zip");

            try
            {
                Cursor         = Cursors.WaitCursor;
                lblStatus.Text = Properties.Resources.Working;

                using (var zipStream = File.Open(newZip, FileMode.Create))
                using (var archive  = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {
                    foreach (string source in txtFile.Items.OfType<string>())
                        archive.CreateEntryFromFile(source, Path.GetFileName(source));
                }

                // Alte ZIP ggf. aufräumen
                TryDeleteZip(_zipUpload);
                _zipUpload = newZip;

                btnUpload.Enabled = true;
                lblStatus.Text    = $"{txtEmail.Text}_{timeStamp}{Properties.Resources.ZipCreated}";
            }
            catch (Exception ex)
            {
                lblStatus.Text = Properties.Resources.Error;
                MessageBox.Show(ex.Message, Properties.Resources.Error,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                TryDeleteZip(newZip);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // ------------------------------------------------------------------ //
        //  Upload
        // ------------------------------------------------------------------ //

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            // Guard: ZIP muss existieren
            if (string.IsNullOrEmpty(_zipUpload) || !File.Exists(_zipUpload))
            {
                MessageBox.Show(
                    Properties.Resources.PleaseCreateZipFirst,
                    Properties.Resources.MissingInput,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            SetUploadRunning(true);

            _uploadCts = new CancellationTokenSource();

            try
            {
                long fileSize = new FileInfo(_zipUpload).Length;

                var progressHandler = new Progress<long>(bytesRead =>
                {
                    if (fileSize > 0)
                    {
                        int pct = (int)(bytesRead * 100L / fileSize);
                        lblStatus.Text = $"{Properties.Resources.Working} {pct}%";
                    }
                });

                await SftpService.UploadAsync(
                    _zipUpload,
                    txtUri.Text,
                    txtUsername.Text,
                    txtPassword.Text,
                    progressHandler,
                    _uploadCts.Token);

                // Erfolg: ZIP löschen, Liste leeren
                TryDeleteZip(_zipUpload);
                _zipUpload = null;
                txtFile.Items.Clear();
                btnUpload.Enabled = false;

                lblStatus.Text = Properties.Resources.Done;
                MessageBox.Show(
                    Properties.Resources.FileUploadedSuccessfully,
                    Properties.Resources.UploadSuccess,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = Properties.Resources.UploadCancelled;
                // ZIP bleibt erhalten – Nutzer kann erneut hochladen
            }
            catch (Exception ex)
            {
                lblStatus.Text = Properties.Resources.Error;
                MessageBox.Show(ex.Message, Properties.Resources.UploadError,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                // ZIP bleibt erhalten – Nutzer kann erneut versuchen
            }
            finally
            {
                _uploadCts?.Dispose();
                _uploadCts = null;
                SetUploadRunning(false);
            }
        }

        /// <summary>Aktiviert/deaktiviert Controls während des Uploads.</summary>
        private void SetUploadRunning(bool running)
        {
            btnUpload.Enabled  = !running;
            btnZip.Enabled     = !running;
            btnPickFile.Enabled = !running;
            btnSave.Enabled    = !running;
            TopMost            = !running; // während Upload nicht blockieren
            Cursor             = running ? Cursors.WaitCursor : Cursors.Default;

            if (running)
                lblStatus.Text = Properties.Resources.Working;
        }

        // ------------------------------------------------------------------ //
        //  Einstellungen speichern
        // ------------------------------------------------------------------ //

        private void OnSettingsChanged(object sender, EventArgs e)
        {
            btnSave.Enabled = true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (_credentials == null) _credentials = new CredentialStore();
                _credentials.Save(txtUri.Text, txtUsername.Text, txtPassword.Text, txtEmail.Text);
                btnSave.Enabled = false;
                lblStatus.Text  = Properties.Resources.Done;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.Error,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------------ //
        //  Passwort anzeigen / verbergen
        // ------------------------------------------------------------------ //

        public void toggleButton_Click(object sender, EventArgs e)
        {
            bool show           = txtPassword.PasswordChar != '\0';
            txtPassword.PasswordChar = show ? '\0' : '✲';
            toggleButton.Image  = show
                ? Properties.Resources.hide_password
                : Properties.Resources.show_password;
        }

        // ------------------------------------------------------------------ //
        //  Tray-Icon
        // ------------------------------------------------------------------ //

        private void notifyIcon1_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (WindowState == FormWindowState.Minimized || !Visible)
                {
                    BringToFront();
                    WindowState = FormWindowState.Normal;
                }
                else
                {
                    WindowState = FormWindowState.Minimized;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                openToolStripMenuItem.Enabled    = WindowState == FormWindowState.Minimized;
                minimizeToolStripMenuItem.Enabled = WindowState != FormWindowState.Minimized;
            }
        }

        private void open_Click(object sender, EventArgs e)
        {
            BringToFront();
            WindowState         = FormWindowState.Normal;
            notifyIcon1.Visible = true;
        }

        private void minimize_Click(object sender, EventArgs e)
        {
            WindowState         = FormWindowState.Minimized;
            notifyIcon1.Visible = true;
        }

        private void close_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Location = Location;
            Properties.Settings.Default.Save();
            Application.Exit();
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            using (var about = new AboutBox1())
                about.ShowDialog();
        }

        // ------------------------------------------------------------------ //
        //  Desktop- und SendTo-Verknüpfungen
        // ------------------------------------------------------------------ //

        private void link_Click(object sender, EventArgs e)
        {
            string path = ShortcutPath("Desktop");
            ManageShortcut(linkToolStripMenuItem.Checked, path);
        }

        private void sendto_Click(object sender, EventArgs e)
        {
            string path = ShortcutPath("SendTo");
            ManageShortcut(sendtoToolStripMenuItem.Checked, path);
        }

        private void ManageShortcut(bool create, string path)
        {
            if (create)
            {
                ShellLink.CreateShortcut(
                    path,
                    Application.ExecutablePath,
                    Environment.CurrentDirectory,
                    Application.ProductName);
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        // ------------------------------------------------------------------ //
        //  Hilfsmethoden
        // ------------------------------------------------------------------ //

        private static string ShortcutPath(string folder)
        {
            string progName = Assembly.GetExecutingAssembly()
                                      .GetName().Name;

            return folder == "Desktop"
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                               $"{progName}.lnk")
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                               @"Microsoft\Windows\SendTo",
                               $"{progName}.lnk");
        }

        private static void TryDeleteZip(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            try { File.Delete(path); } catch { /* ignorieren */ }
        }

        // Cleanup erfolgt in formClosing – Dispose wird vom Designer generiert
    }
}
