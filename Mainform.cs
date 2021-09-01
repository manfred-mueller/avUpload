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

namespace avUpload
{
    public partial class Mainform : Form
    {
        public string timeStamp = null;
        public string zipPath = null;
        public string zipUpload = null;
        public char mask = '✲';
        RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Avast\Whitelisting", true);
        public string linkName = null;
        public string linkPath = null;
        public string sendtoPath = null;

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
        
        public Mainform(string[] args)
        {
            InitializeComponent();
            WindowState = FormWindowState.Normal;           
            notifyIcon1.Icon = new Icon(Properties.Resources.avUpload, 48, 48);
            notifyIcon1.Visible = true;
            Location = Properties.Settings.Default.Location;
            TopMost = true;
            AllowDrop = true;
            if (args.Length > 0)
            {
                foreach (string file in args)
                {
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
                txtUri.Text = (string)regKey.GetValue("Uri", null);
                txtUsername.Text = (string)regKey.GetValue("Username", null);
                txtPassword.Text = Decrypt();
                txtEmail.Text = (string)regKey.GetValue("Email", null);
            }

            btnSave.Enabled = false;
            btnUpload.Enabled = false;

            txtUri.TextChanged += new EventHandler(ChangeHandler);
            txtUsername.TextChanged += new EventHandler(ChangeHandler);
            txtPassword.TextChanged += new EventHandler(ChangeHandler);
            txtEmail.TextChanged += new EventHandler(ChangeHandler);

            linkName = Properties.Resources.ProgName + ".lnk";
            linkPath = (@Environment.GetEnvironmentVariable("USERPROFILE") + "\\Desktop\\" + linkName);
            sendtoPath = (@Environment.GetEnvironmentVariable("USERPROFILE") + "\\AppData\\Roaming\\Microsoft\\Windows\\SendTo\\" + linkName);
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
                foreach (String file in ofdFile.FileNames)
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
//            timeStamp = date.ToString("ddMMyyyy-HHmmssffff");
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

        // Use FTP to upload a file.
        private void FtpUploadFile(string filename, string to_uri, string user_name, string password)
        {
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(to_uri);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // Get network credentials.
            request.Credentials = new NetworkCredential(user_name.Normalize(), password.Normalize());
            

            // Read the file's contents into a byte array.
            byte[] bytes = System.IO.File.ReadAllBytes(filename);

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
            AboutBox1 aboutBox = new AboutBox1();

            aboutBox.ShowDialog();
//            MessageBox.Show(this, Properties.Resources.Description, Properties.Resources.ProgName, MessageBoxButtons.OK, MessageBoxIcon.None);
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
    }
}
