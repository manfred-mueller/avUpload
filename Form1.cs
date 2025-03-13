using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;

using Renci.SshNet;

namespace avUpload
{
    public partial class Form1 : Form
    {
        public string timeStamp = null;
        public string zipPath = null;
        public string zipUpload = null;
        public char mask = '✲';
        RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\NASS e.K.\Avast-Whitelisting", true);
        public string linkPath = null;
        public string sendtoPath = null;
        public string publickey = "52830761";
        public string privatekey = "Cfg_!7KjH";


        public string Encrypt(string textToEncrypt)
        {
            try
            {
                string ToReturn = "";
                byte[] privatekeyByte = Encoding.UTF8.GetBytes(privatekey);
                byte[] publickeybyte = Encoding.UTF8.GetBytes(publickey);

                byte[] inputbyteArray = Encoding.UTF8.GetBytes(textToEncrypt);
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                using (MemoryStream ms = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(publickeybyte, privatekeyByte), CryptoStreamMode.Write))
                {
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    ToReturn = Convert.ToBase64String(ms.ToArray());
                }

                return ToReturn;
            }
            catch (Exception ex)
            {
                throw new Exception(avUpload.Properties.Resources.EncryptionFailed, ex);
            }
        }

        public string Decrypt(string textToDecrypt)
        {
            if (string.IsNullOrEmpty(publickey))
            {
                throw new ArgumentNullException(nameof(publickey), avUpload.Properties.Resources.PublicKeyCannotBeNullOrEmpty);
            }

            if (string.IsNullOrEmpty(privatekey))
            {
                throw new ArgumentNullException(nameof(privatekey), avUpload.Properties.Resources.PrivateKeyCannotBeNullOrEmpty);
            }

            try
            {
                string encryptedText = (string)regKey.GetValue(textToDecrypt, null);
                if (encryptedText == null)
                {
                    throw new Exception(String.Format(avUpload.Properties.Resources.NoValueFoundInTheRegistryForKey0, textToDecrypt));
                }

                byte[] privatekeyByte = Encoding.UTF8.GetBytes(privatekey);
                byte[] publickeybyte = Encoding.UTF8.GetBytes(publickey);

                byte[] inputbyteArray = Convert.FromBase64String(encryptedText.Replace(" ", "+"));

                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                using (MemoryStream ms = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(publickeybyte, privatekeyByte), CryptoStreamMode.Write))
                {
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch (Exception)
            {
                return (string)regKey.GetValue(textToDecrypt, null); ;
            }
        }

        public Form1(string[] args)
        {
            InitializeComponent();
            Version shortVersion = Assembly.GetExecutingAssembly().GetName().Version;
            this.Text = string.Format(avUpload.Properties.Resources.ProgName + $" {shortVersion.Major}.{shortVersion.Minor}.{shortVersion.Build}");
            WindowState = FormWindowState.Normal;           
            notifyIcon1.Icon = new Icon(Properties.Resources.avUpload, 48, 48);
            notifyIcon1.Visible = true;
            Location = Properties.Settings.Default.Location;
            TopMost = true;
            AllowDrop = true;
            if (args.Length > 1) // Check if there's more than one argument
            {
                for (int i = 1; i < args.Length; i++) // Start from index 1 to skip the first file
                {
                    string file = args[i];
                    txtFile.Items.Add(file);
                }
                btnZip.Enabled = true;
            }
            else
            {
                btnZip.Enabled = false;
            }

            if (regKey != null)
            {
                txtUri.Text = Decrypt("Uri");
                txtUsername.Text = Decrypt("Username");
                txtPassword.Text = Decrypt("Password");
                txtEmail.Text = Decrypt("Email");
            }
            else
            {
                AboutBox2 aboutBox = new AboutBox2();

                aboutBox.ShowDialog();
            }

            btnSave.Enabled = false;
            btnUpload.Enabled = false;

            txtUri.TextChanged += new EventHandler(ChangeHandler);
            txtUsername.TextChanged += new EventHandler(ChangeHandler);
            txtPassword.TextChanged += new EventHandler(ChangeHandler);
            txtEmail.TextChanged += new EventHandler(ChangeHandler);

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            linkPath = String.Format("{0}\\Desktop\\{1}.lnk", @Environment.GetEnvironmentVariable("USERPROFILE"), Properties.Resources.ProgName);
            sendtoPath = String.Format("{0}\\AppData\\Roaming\\Microsoft\\Windows\\SendTo\\{1}.lnk", @Environment.GetEnvironmentVariable("USERPROFILE"), fvi.ProductName);
            if (System.IO.File.Exists(linkPath))
            {
                this.linkToolStripMenuItem.Checked = true;
            }
            if (System.IO.File.Exists(sendtoPath))
            {
                this.sendtoToolStripMenuItem.Checked = true;
            }
        }

        // Start with the executable file.
        private void Mainform_Load(object sender, EventArgs e)
        {
            lblStatus.Text = Properties.Resources.Done;
            txtFile.Text = Application.ExecutablePath;
            // Get the path of the currently running executable
            string exePath = Assembly.GetExecutingAssembly().Location;
            string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\\Windows\\Start Menu\\Programs", "MyApp.lnk");

            // Create a shortcut to the application if it doesn't already exist
            if (!System.IO.File.Exists(shortcutPath))
            {
                CreateShortcut(shortcutPath, exePath);
            }

            // Pin the shortcut to the taskbar
            PinToTaskbar(shortcutPath);
        }

        private static void CreateShortcut(string shortcutPath, string targetPath)
        {
            var shell = new IWshRuntimeLibrary.WshShell();
            var shortcut = (IWshRuntimeLibrary.WshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.Save();
        }

        private static void PinToTaskbar(string shortcutPath)
        {
            // PowerShell script to pin the shortcut to the taskbar
            string psScript = $@"
            $shell = New-Object -ComObject Shell.Application
            $folder = $shell.Namespace('{Path.GetDirectoryName(shortcutPath)}')
            $item = $folder.ParseName('{Path.GetFileName(shortcutPath)}')
            $item.InvokeVerb('taskbarpin')
        ";

            Process.Start(new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-Command \"{psScript}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });
        }
        public void toggleButton_Click(object sender, EventArgs e)
        {
            if (mask == '✲')
            {
                txtPassword.PasswordChar = '\0';
                mask = '\0';
                toggleButton.Image = Properties.Resources.hide_password;

            }
            else
            {
                txtPassword.PasswordChar = '✲';
                mask = '✲';
                toggleButton.Image = Properties.Resources.show_password;
            }
        }

        // Let the user pick files.
        private void btnPickFile_Click(object sender, EventArgs e)
        {
            if (ofdFile.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in ofdFile.FileNames)
                {
                    try
                    {
                        Stream myStream;
                        if ((myStream = ofdFile.OpenFile()) != null)
                        {
                            using (myStream)
                            {
                                txtFile.Items.Add(file);
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show(Properties.Resources.CouldNotReadTheFile + ex.Message);
                    }
                }
            }
            if (txtFile.Items.Count > 0)
                btnZip.Enabled = true;
            else
            {
                btnZip.Enabled = false;
            }
        }

        private void txtFile_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ListBox.SelectedObjectCollection selListItems = txtFile.SelectedItems;

                foreach (var item in selListItems.OfType<string>().ToList())
                {
                    txtFile.Items.Remove(item);
                }
                if (txtFile.Items.Count > 0)
                    btnZip.Enabled = true;
                else
                {
                    btnZip.Enabled = false;
                }

            }
        }

        // Upload the selected file.
        private async void btnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                lblStatus.Text = Properties.Resources.Working;
                Application.DoEvents();

                // Call the asynchronous FTP upload function.
                await SftpUploadFileAsync(zipUpload, txtUri.Text, txtUsername.Text, txtPassword.Text);

                lblStatus.Text = Properties.Resources.Done;
            }
            catch (Exception ex)
            {
                lblStatus.Text = Properties.Resources.Error;
                MessageBox.Show(ex.Message);
            }
            finally
            {
                System.IO.File.Delete(zipUpload);
                txtFile.Text = null;
                Cursor = Cursors.Default;
            }
        }

        // Prepare the ZIP archive for Avast

        private void btnZip_Click(object sender, EventArgs e)
        {
            ListBox.ObjectCollection ListItems = txtFile.Items;
            DateTime date = DateTime.Now;
            timeStamp = date.ToString("ffff_dd-MM-yyyy");
            zipPath = Path.GetTempPath();
            zipUpload = zipPath + txtEmail.Text + "_" + timeStamp + ".zip";
            try
            {
                Cursor = Cursors.WaitCursor;
                lblStatus.Text = Properties.Resources.Working;
                // Create FileStream for output ZIP archive
                using (FileStream zipFile = System.IO.File.Open(zipUpload, FileMode.Create))
                // File to be added to archive
                using (ZipArchive arch = new ZipArchive(zipFile, ZipArchiveMode.Create))
                {
                    foreach (var zipSource in ListItems.OfType<string>().ToList())
                    {
                        arch.CreateEntryFromFile(zipSource, Path.GetFileName(zipSource));
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = Properties.Resources.Error;
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnUpload.Enabled = true;
                lblStatus.Text = txtEmail.Text + "_" + timeStamp + Properties.Resources.ZipCreated;
            }

        }

        private async Task SftpUploadFileAsync(string filename, string sftpUrl, string user_name, string password)
        {
            try
            {
                // Parse the URL to get the host, port, and directory
                Uri uri = new Uri("sftp://" + sftpUrl); // Prepend "sftp://" to make it a valid URI

                string host = uri.Host;  // Host (e.g., whitelisting.avast.com)
                int port = uri.Port;     // Port (e.g., 22)
                string remoteDirectory = uri.AbsolutePath;  // Remote directory (e.g., /data)

                // Ensure the remote directory is correctly formatted (without starting slash)
                remoteDirectory = remoteDirectory.TrimStart('/');

                // Create a connection to the SFTP server
                var connectionInfo = new ConnectionInfo(host, port, user_name, new PasswordAuthenticationMethod(user_name, password));

                using (var sftpClient = new SftpClient(connectionInfo))
                {
                    // Attempt to connect
                    sftpClient.Connect();

                    if (sftpClient.IsConnected)
                    {
                        // Open the file stream asynchronously
                        byte[] bytes;
                        using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                        {
                            bytes = new byte[fs.Length];
                            await fs.ReadAsync(bytes, 0, (int)fs.Length);
                        }

                        using (MemoryStream ms = new MemoryStream(bytes))
                        {
                            // Define the remote path for file upload
                            string remotePath = "/" + remoteDirectory + "/" + Path.GetFileName(filename);

                            // Ensure the directory exists before uploading
                            if (!sftpClient.Exists("/" + remoteDirectory))
                            {
                                // Show a message if the directory does not exist
                                MessageBox.Show(String.Format(avUpload.Properties.Resources.Directory0DoesNotExistOnTheServer, remoteDirectory), avUpload.Properties.Resources.DirectoryError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            // Upload the file to the SFTP server
                            await Task.Run(() => sftpClient.UploadFile(ms, remotePath));

                            // Show message after successful upload
                            MessageBox.Show(avUpload.Properties.Resources.FileUploadedSuccessfully, avUpload.Properties.Resources.UploadSuccess, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        // Disconnect after upload
                        sftpClient.Disconnect();
                    }
                    else
                    {
                        // Show message if connection failed
                        MessageBox.Show(avUpload.Properties.Resources.FailedToConnectToTheSFTPServer, avUpload.Properties.Resources.ConnectionFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // Catch any errors during the connection or upload process and show them
                MessageBox.Show(String.Format(avUpload.Properties.Resources.Error0, ex.Message), avUpload.Properties.Resources.UploadError, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        void ChangeHandler(object sender, EventArgs e)
        {
            btnSave.Enabled = true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (regKey == null)
            {
                regKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\NASS e.K.\\Avast-Whitelisting");
            }
            regKey.SetValue("Uri", Encrypt(txtUri.Text));
            regKey.SetValue("Username", Encrypt(txtUsername.Text));
            regKey.SetValue("Password", Encrypt(txtPassword.Text));
            regKey.SetValue("Email", Encrypt(txtEmail.Text));
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox = new AboutBox1();

            aboutBox.ShowDialog();
        }
        private void notifyIcon1_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (Visible == false)
                {
                    BringToFront();
                }
                if (WindowState == FormWindowState.Minimized || Visible == false)
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
                if (WindowState == FormWindowState.Minimized)
                {
                    openToolStripMenuItem.Enabled = true;
                    minimizeToolStripMenuItem.Enabled = false;
                }
                else
                {
                    openToolStripMenuItem.Enabled = false;
                    minimizeToolStripMenuItem.Enabled = true;
                }
            }
        }

        private void open_Click(object sender, EventArgs e)
        {
            if (Visible == false)
            {
                BringToFront();
            }
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = true;
        }
        private void minimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            notifyIcon1.Visible = true;
        }

        private void link_Click(object sender, EventArgs e)
        {
            if (this.linkToolStripMenuItem.Checked == true)
            {
                var WshShell = new WshShell();
                IWshShortcut MyShortcut;

                MyShortcut = (IWshShortcut)WshShell.CreateShortcut(linkPath);
                MyShortcut.TargetPath = Application.ExecutablePath;
                MyShortcut.WorkingDirectory = Environment.CurrentDirectory;
                MyShortcut.Description = Application.ProductName;
                MyShortcut.Save();
            }
            else
            {
                if (System.IO.File.Exists(linkPath))
                {
                    System.IO.File.Delete(linkPath);
                    this.linkToolStripMenuItem.Checked = false;
                }
            }
        }
        private void sendto_Click(object sender, EventArgs e)
        {
            if (this.sendtoToolStripMenuItem.Checked == true)
            {
                var WshShell = new WshShell();
                IWshShortcut MyShortcut;

                MyShortcut = (IWshShortcut)WshShell.CreateShortcut(sendtoPath);
                MyShortcut.TargetPath = Application.ExecutablePath;
                MyShortcut.WorkingDirectory = Environment.CurrentDirectory;
                MyShortcut.Description = Application.ProductName;
                MyShortcut.Save();
            }
            else
            {
                if (System.IO.File.Exists(sendtoPath))
                {
                    System.IO.File.Delete(sendtoPath);
                    this.sendtoToolStripMenuItem.Checked = false;
                }
            }
        }
        private void close_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Location = Location;
            Properties.Settings.Default.Save();
            Application.Exit();
        }

        private void formClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Location = Location;
            Properties.Settings.Default.Save();
        }

        private void formLoading(object sender, EventArgs e)
        {
            Location = Properties.Settings.Default.Location;
        }
        private void txtFile_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }
        private void txtFile_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                txtFile.Items.Add(file);
            }
            btnZip.Enabled = true;
        }
        private void txtFile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
        public void LoadFile(string file)
        {
            if (System.IO.File.Exists(file))
            {
                txtFile.Items.Add(file);
            }

        }
    }
}
