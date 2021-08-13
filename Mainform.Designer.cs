
using System.Windows.Forms;

namespace avUpload
{
    partial class Mainform
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Mainform));
            this.btnPickFile = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnUpload = new System.Windows.Forms.Button();
            this.txtFile = new System.Windows.Forms.ListBox();
            this.lblFile = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblUsername = new System.Windows.Forms.Label();
            this.txtUri = new System.Windows.Forms.TextBox();
            this.lblUri = new System.Windows.Forms.Label();
            this.ofdFile = new System.Windows.Forms.OpenFileDialog();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label();
            this.toggleButton = new System.Windows.Forms.Button();
            this.btnAbout = new System.Windows.Forms.Button();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.minimizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayIconContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip3 = new System.Windows.Forms.ToolTip(this.components);
            this.btnZip = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.trayIconContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnPickFile
            // 
            this.btnPickFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPickFile.Location = new System.Drawing.Point(342, 114);
            this.btnPickFile.Name = "btnPickFile";
            this.btnPickFile.Size = new System.Drawing.Size(30, 23);
            this.btnPickFile.TabIndex = 1;
            this.btnPickFile.Text = "...";
            this.toolTip2.SetToolTip(this.btnPickFile, global::avUpload.Properties.Resources.PickFile);
            this.btnPickFile.UseVisualStyleBackColor = true;
            this.btnPickFile.Click += new System.EventHandler(this.btnPickFile_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblStatus.Location = new System.Drawing.Point(15, 293);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(357, 23);
            this.lblStatus.TabIndex = 30;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnUpload
            // 
            this.btnUpload.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnUpload.Image = global::avUpload.Properties.Resources.upload;
            this.btnUpload.Location = new System.Drawing.Point(342, 179);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(30, 35);
            this.btnUpload.TabIndex = 3;
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            this.toolTip1.SetToolTip(this.btnUpload, global::avUpload.Properties.Resources.ToolTipUpload);
            // 
            // txtFile
            // 
            this.txtFile.AllowDrop = true;
            this.txtFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFile.Location = new System.Drawing.Point(82, 116);
            this.txtFile.Name = "txtFile";
            this.txtFile.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.txtFile.Size = new System.Drawing.Size(254, 173);
            this.txtFile.TabIndex = 25;
            this.toolTip3.SetToolTip(this.txtFile, global::avUpload.Properties.Resources.DropFileToUploadHere);
            this.txtFile.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtFile_DragDrop);
            this.txtFile.DragOver += new System.Windows.Forms.DragEventHandler(this.txtFile_DragOver);
            this.txtFile.MouseDown += new System.Windows.Forms.MouseEventHandler(this.txtFile_MouseDown);
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(12, 119);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(37, 13);
            this.lblFile.TabIndex = 29;
            this.lblFile.Text = global::avUpload.Properties.Resources.Files;
            // 
            // txtPassword
            // 
            this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPassword.Location = new System.Drawing.Point(82, 64);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '✲';
            this.txtPassword.Size = new System.Drawing.Size(254, 20);
            this.txtPassword.TabIndex = 4;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(12, 67);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(56, 13);
            this.lblPassword.TabIndex = 28;
            this.lblPassword.Text = global::avUpload.Properties.Resources.Password;
            // 
            // txtUsername
            // 
            this.txtUsername.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUsername.Location = new System.Drawing.Point(82, 38);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(290, 20);
            this.txtUsername.TabIndex = 6;
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new System.Drawing.Point(12, 41);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(58, 13);
            this.lblUsername.TabIndex = 27;
            this.lblUsername.Text = global::avUpload.Properties.Resources.Username;
            // 
            // txtUri
            // 
            this.txtUri.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUri.Location = new System.Drawing.Point(82, 12);
            this.txtUri.Name = "txtUri";
            this.txtUri.Size = new System.Drawing.Size(290, 20);
            this.txtUri.TabIndex = 5;
            // 
            // lblUri
            // 
            this.lblUri.AutoSize = true;
            this.lblUri.Location = new System.Drawing.Point(12, 15);
            this.lblUri.Name = "lblUri";
            this.lblUri.Size = new System.Drawing.Size(32, 13);
            this.lblUri.TabIndex = 22;
            this.lblUri.Text = global::avUpload.Properties.Resources.URL;
            // 
            // ofdFile
            // 
            this.ofdFile.FileName = global::avUpload.Properties.Resources.WhitelistExe;
            this.ofdFile.Multiselect = true;
            this.ofdFile.RestoreDirectory = true;
            this.ofdFile.Title = global::avUpload.Properties.Resources.SelectFile;
            // 
            // txtEmail
            // 
            this.txtEmail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEmail.Location = new System.Drawing.Point(82, 90);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(290, 20);
            this.txtEmail.TabIndex = 6;
            // 
            // lblEmail
            // 
            this.lblEmail.AutoSize = true;
            this.lblEmail.Location = new System.Drawing.Point(12, 93);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(35, 13);
            this.lblEmail.TabIndex = 33;
            this.lblEmail.Text = global::avUpload.Properties.Resources.Email;
            // 
            // toggleButton
            // 
            this.toggleButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toggleButton.BackColor = System.Drawing.SystemColors.Control;
            this.toggleButton.Image = global::avUpload.Properties.Resources.show_password;
            this.toggleButton.Location = new System.Drawing.Point(342, 62);
            this.toggleButton.Name = "toggleButton";
            this.toggleButton.Size = new System.Drawing.Size(30, 23);
            this.toggleButton.TabIndex = 5;
            this.toolTip1.SetToolTip(this.toggleButton, global::avUpload.Properties.Resources.ShowHidePassword);
            this.toggleButton.UseVisualStyleBackColor = false;
            this.toggleButton.Click += new System.EventHandler(this.toggleButton_Click);
            // 
            // btnAbout
            // 
            this.btnAbout.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnAbout.Image = global::avUpload.Properties.Resources.info;
            this.btnAbout.Location = new System.Drawing.Point(342, 255);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(30, 35);
            this.btnAbout.TabIndex = 4;
            this.btnAbout.UseVisualStyleBackColor = true;
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.openToolStripMenuItem.Text = global::avUpload.Properties.Resources.Show;
            this.openToolStripMenuItem.Click += new System.EventHandler(this.open_Click);
            // 
            // minimizeToolStripMenuItem
            // 
            this.minimizeToolStripMenuItem.Image = global::avUpload.Properties.Resources.underline;
            this.minimizeToolStripMenuItem.Name = "minimizeToolStripMenuItem";
            this.minimizeToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.minimizeToolStripMenuItem.Text = global::avUpload.Properties.Resources.Minimize;
            this.minimizeToolStripMenuItem.Click += new System.EventHandler(this.minimize_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Image = global::avUpload.Properties.Resources.exit;
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.closeToolStripMenuItem.Text = global::avUpload.Properties.Resources.Close;
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.close_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.trayIconContextMenuStrip;
            this.notifyIcon1.Text = global::avUpload.Properties.Resources.ProgName;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_Click);
            // 
            // trayIconContextMenuStrip
            // 
            this.trayIconContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.minimizeToolStripMenuItem,
            this.closeToolStripMenuItem});
            this.trayIconContextMenuStrip.Name = "trayIconContextMenuStrip";
            this.trayIconContextMenuStrip.Size = new System.Drawing.Size(124, 70);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 50000;
            this.toolTip1.InitialDelay = 500;
            this.toolTip1.ReshowDelay = 100;
            this.toolTip1.ShowAlways = true;
            // 
            // btnZip
            // 
            this.btnZip.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnZip.Image = global::avUpload.Properties.Resources.compress;
            this.btnZip.Location = new System.Drawing.Point(342, 143);
            this.btnZip.Name = "btnZip";
            this.btnZip.Size = new System.Drawing.Size(30, 35);
            this.btnZip.TabIndex = 2;
            this.btnZip.UseVisualStyleBackColor = true;
            this.toolTip1.SetToolTip(this.btnZip, global::avUpload.Properties.Resources.ToolTipCompress);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnSave.Image = global::avUpload.Properties.Resources.save;
            this.btnSave.Location = new System.Drawing.Point(342, 217);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(30, 35);
            this.btnSave.TabIndex = 7;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.toolTip1.SetToolTip(this.btnSave, avUpload.Properties.Resources.ToolTipSaveSettings);
            // 
            // Mainform
            // 
            this.AcceptButton = this.btnUpload;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 324);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnZip);
            this.Controls.Add(this.btnAbout);
            this.Controls.Add(this.toggleButton);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.lblEmail);
            this.Controls.Add(this.btnPickFile);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnUpload);
            this.Controls.Add(this.txtFile);
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.txtUri);
            this.Controls.Add(this.lblUri);
            this.Icon = global::avUpload.Properties.Resources.avUpload;
            this.Name = "Mainform";
            this.Text = avUpload.Properties.Resources.ProgName;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formClosing);
            this.Load += new System.EventHandler(this.formLoading);
            this.trayIconContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPickFile;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.ListBox txtFile;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtUri;
        private System.Windows.Forms.Label lblUri;
        private System.Windows.Forms.OpenFileDialog ofdFile;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.Button toggleButton;
        private System.Windows.Forms.Button btnAbout;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolTip toolTip2;
        private System.Windows.Forms.ToolTip toolTip3;
        private System.Windows.Forms.ContextMenuStrip trayIconContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem minimizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private Button btnZip;
        private Button btnSave;
    }
}

