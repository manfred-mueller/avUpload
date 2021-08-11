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

namespace avUpload
{
    public partial class Mainform : Form
    {
        public string sourceName = null;
        public string fullSourceName = null;
        public string timeStamp = null;
        public string zipName = null;
        public string zipPath = null;
        public string zipSource = null;
        public string zipUpload = null;
        public char mask = '✲';
        RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Avast\Whitelisting", true);


        public string Encrypt()
        {
            try
            {
                string textToEncrypt = txtPassword.Text;
                string ToReturn = "";
                string publickey = "52830761";
                string secretkey = "Cfg_!7KjH";
                byte[] secretkeyByte = { };
                secretkeyByte = Encoding.UTF8.GetBytes(secretkey);
                byte[] publickeybyte = { };
                publickeybyte = Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = Encoding.UTF8.GetBytes(textToEncrypt);
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateEncryptor(publickeybyte, secretkeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    ToReturn = Convert.ToBase64String(ms.ToArray());
                }
                return ToReturn;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public string Decrypt()
        {
            try
            {
                string textToDecrypt = (string)regKey.GetValue("Password", null);
                string ToReturn = "";
                string publickey = "52830761";
                string privatekey = "Cfg_!7KjH";
                byte[] privatekeyByte = { };
                privatekeyByte = Encoding.UTF8.GetBytes(privatekey);
                byte[] publickeybyte = { };
                publickeybyte = Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = new byte[textToDecrypt.Replace(" ", "+").Length];
                inputbyteArray = Convert.FromBase64String(textToDecrypt.Replace(" ", "+"));
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateDecryptor(publickeybyte, privatekeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    Encoding encoding = Encoding.UTF8;
                    ToReturn = encoding.GetString(ms.ToArray());
                }
                return ToReturn;
            }
            catch (Exception ae)
            {
                throw new Exception(ae.Message, ae.InnerException);
            }
        }
        
        public Mainform()
        {
            InitializeComponent();

            WindowState = FormWindowState.Normal;           
            ShowInTaskbar = false;
            notifyIcon1.Icon = new Icon(Properties.Resources.avUpload, 48, 48);
            notifyIcon1.Visible = true;
            Location = Properties.Settings.Default.Location;
            
            TopMost = true;
            AllowDrop = true;

            if (regKey != null)
            {
                txtUri.Text = (string)regKey.GetValue("Uri", null);
                txtUsername.Text = (string)regKey.GetValue("Username", null);
                txtPassword.Text = Decrypt();
                txtEmail.Text = (string)regKey.GetValue("Email", null);
            }

            btnSave.Enabled = false;

            txtUri.TextChanged += new EventHandler(ChangeHandler);
            txtUsername.TextChanged += new EventHandler(ChangeHandler);
            txtPassword.TextChanged += new EventHandler(ChangeHandler);
            txtEmail.TextChanged += new EventHandler(ChangeHandler);
        }

        // Start with the executable file.
        private void Mainform_Load(object sender, EventArgs e)
        {
            lblStatus.Text = Properties.Resources.Done;
            txtFile.Text = Application.ExecutablePath;
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

        // Let the user pick a file.
        private void btnPickFile_Click(object sender, EventArgs e)
        {
            if (ofdFile.ShowDialog() == DialogResult.OK)
            {
                txtFile.Text = ofdFile.FileName;
                ZipTheFile(ofdFile.FileName);
            }
        }

        // Upload the selected file.
        private void btnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                lblStatus.Text = Properties.Resources.Working;
                Application.DoEvents();

                FtpUploadFile(zipUpload, txtUri.Text,
                    txtUsername.Text, txtPassword.Text);

                lblStatus.Text = Properties.Resources.Done;
            }
            catch (Exception ex)
            {
                lblStatus.Text = Properties.Resources.Error;
                MessageBox.Show(ex.Message);
            }
            finally
            {
                File.Delete(zipUpload);
                txtFile.Text = null;
                Cursor = Cursors.Default;
            }
        }

        // Prepare the ZIP archive for Avast

        private void ZipTheFile(string ZipSource)
        {
            DateTime date = DateTime.Now;
            timeStamp = date.ToString("ddMMyyyy");
            fullSourceName = ZipSource;
            txtFile.Text = fullSourceName;
            sourceName = Path.GetFileName(fullSourceName);
            zipName = Path.GetFileNameWithoutExtension(sourceName);
            zipPath = Path.GetTempPath();
            zipUpload = zipPath + txtEmail.Text + "_" + timeStamp + "_" + zipName + ".zip";
            try
            {
                Cursor = Cursors.WaitCursor;
                lblStatus.Text = Properties.Resources.Working;
                // Create FileStream for output ZIP archive
                using (FileStream zipFile = File.Open(zipPath + txtEmail.Text + "_" + timeStamp + "_" + zipName + ".zip", FileMode.Create))
                // File to be added to archive
                using (ZipArchive arch = new ZipArchive(zipFile, ZipArchiveMode.Create))
                {
                    arch.CreateEntryFromFile(fullSourceName, sourceName);
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
                lblStatus.Text = txtEmail.Text + "_" + timeStamp + "_" + zipName + Properties.Resources.ZipCreated;
            }

        }

        // Use FTP to upload a file.
        private void FtpUploadFile(string filename, string to_uri, string user_name, string password)
        {
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(to_uri);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // Get network credentials.
            request.Credentials = new NetworkCredential(user_name.Normalize(), password.Normalize());
            

            // Read the file's contents into a byte array.
            byte[] bytes = File.ReadAllBytes(filename);

            // Write the bytes into the request stream.
            request.ContentLength = bytes.Length;
            using (Stream request_stream = request.GetRequestStream())
            {
                request_stream.Write(bytes, 0, bytes.Length);
                request_stream.Close();
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
                regKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Avast\\Whitelisting");
            }
            regKey.SetValue("Uri", txtUri.Text);
            regKey.SetValue("Username", txtUsername.Text);
            regKey.SetValue("Password", Encrypt());
            regKey.SetValue("Email", txtEmail.Text);
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
                MessageBox.Show(this, Properties.Resources.Description, Properties.Resources.ProgName, MessageBoxButtons.OK, MessageBoxIcon.None);

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
                    ShowInTaskbar = false;
                }
                else
                {
                    WindowState = FormWindowState.Minimized;
                    ShowInTaskbar = false;
//                    selfMinimized = true;
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
            ShowInTaskbar = false;
        }
        private void minimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            notifyIcon1.Visible = true;
            ShowInTaskbar = false;
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
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Any())
            {
                txtFile.Text = files.First();
                ZipTheFile(files.First());
            }
        }
    }
}
