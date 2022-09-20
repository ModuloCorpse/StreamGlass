namespace StreamGlass
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.TwitchBotSecret = new System.Windows.Forms.Panel();
            this.TwitchBotSecretVisibility = new System.Windows.Forms.Button();
            this.TwitchBotSecretTextBox = new System.Windows.Forms.TextBox();
            this.TwitchBotSecretLabel = new System.Windows.Forms.Label();
            this.TwitchBotPublic = new System.Windows.Forms.Panel();
            this.TwitchBotPublicTextBox = new System.Windows.Forms.TextBox();
            this.TwitchBotPublicLabel = new System.Windows.Forms.Label();
            this.TwitchBotChannel = new System.Windows.Forms.Panel();
            this.TwitchChannelTextBox = new System.Windows.Forms.TextBox();
            this.TwitchChannelLabel = new System.Windows.Forms.Label();
            this.OptionPanel = new System.Windows.Forms.Panel();
            this.SaveButton = new System.Windows.Forms.Button();
            this.SystemPanel = new System.Windows.Forms.Panel();
            this.SystemBrowserFileDialog = new System.Windows.Forms.Button();
            this.SystemBrowserTextBox = new System.Windows.Forms.TextBox();
            this.SystemBrowserLabel = new System.Windows.Forms.Label();
            this.SettingsTabs = new System.Windows.Forms.TabControl();
            this.GeneralTabPage = new System.Windows.Forms.TabPage();
            this.TwitchTabPage = new System.Windows.Forms.TabPage();
            this.TwitchConnectButton = new System.Windows.Forms.Button();
            this.TabIconImageList = new System.Windows.Forms.ImageList(this.components);
            this.TwitchBotSecret.SuspendLayout();
            this.TwitchBotPublic.SuspendLayout();
            this.TwitchBotChannel.SuspendLayout();
            this.OptionPanel.SuspendLayout();
            this.SystemPanel.SuspendLayout();
            this.SettingsTabs.SuspendLayout();
            this.GeneralTabPage.SuspendLayout();
            this.TwitchTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // TwitchBotSecret
            // 
            this.TwitchBotSecret.Controls.Add(this.TwitchBotSecretVisibility);
            this.TwitchBotSecret.Controls.Add(this.TwitchBotSecretTextBox);
            this.TwitchBotSecret.Controls.Add(this.TwitchBotSecretLabel);
            this.TwitchBotSecret.Dock = System.Windows.Forms.DockStyle.Top;
            this.TwitchBotSecret.Location = new System.Drawing.Point(3, 84);
            this.TwitchBotSecret.Name = "TwitchBotSecret";
            this.TwitchBotSecret.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.TwitchBotSecret.Size = new System.Drawing.Size(425, 29);
            this.TwitchBotSecret.TabIndex = 2;
            // 
            // TwitchBotSecretVisibility
            // 
            this.TwitchBotSecretVisibility.BackColor = System.Drawing.Color.Transparent;
            this.TwitchBotSecretVisibility.BackgroundImage = global::StreamGlass.Properties.Resources.sight_disabled;
            this.TwitchBotSecretVisibility.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.TwitchBotSecretVisibility.Dock = System.Windows.Forms.DockStyle.Left;
            this.TwitchBotSecretVisibility.FlatAppearance.BorderSize = 0;
            this.TwitchBotSecretVisibility.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TwitchBotSecretVisibility.Location = new System.Drawing.Point(360, 3);
            this.TwitchBotSecretVisibility.Margin = new System.Windows.Forms.Padding(0);
            this.TwitchBotSecretVisibility.Name = "TwitchBotSecretVisibility";
            this.TwitchBotSecretVisibility.Size = new System.Drawing.Size(23, 23);
            this.TwitchBotSecretVisibility.TabIndex = 3;
            this.TwitchBotSecretVisibility.UseVisualStyleBackColor = false;
            this.TwitchBotSecretVisibility.Click += new System.EventHandler(this.TwitchBotSecretVisibility_Click);
            // 
            // TwitchBotSecretTextBox
            // 
            this.TwitchBotSecretTextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.TwitchBotSecretTextBox.Location = new System.Drawing.Point(110, 3);
            this.TwitchBotSecretTextBox.Name = "TwitchBotSecretTextBox";
            this.TwitchBotSecretTextBox.PasswordChar = '*';
            this.TwitchBotSecretTextBox.Size = new System.Drawing.Size(250, 23);
            this.TwitchBotSecretTextBox.TabIndex = 1;
            // 
            // TwitchBotSecretLabel
            // 
            this.TwitchBotSecretLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.TwitchBotSecretLabel.Location = new System.Drawing.Point(0, 3);
            this.TwitchBotSecretLabel.Name = "TwitchBotSecretLabel";
            this.TwitchBotSecretLabel.Size = new System.Drawing.Size(110, 23);
            this.TwitchBotSecretLabel.TabIndex = 0;
            this.TwitchBotSecretLabel.Text = "Bot Secret:";
            this.TwitchBotSecretLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TwitchBotPublic
            // 
            this.TwitchBotPublic.Controls.Add(this.TwitchBotPublicTextBox);
            this.TwitchBotPublic.Controls.Add(this.TwitchBotPublicLabel);
            this.TwitchBotPublic.Dock = System.Windows.Forms.DockStyle.Top;
            this.TwitchBotPublic.Location = new System.Drawing.Point(3, 55);
            this.TwitchBotPublic.Name = "TwitchBotPublic";
            this.TwitchBotPublic.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.TwitchBotPublic.Size = new System.Drawing.Size(425, 29);
            this.TwitchBotPublic.TabIndex = 4;
            // 
            // TwitchBotPublicTextBox
            // 
            this.TwitchBotPublicTextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.TwitchBotPublicTextBox.Location = new System.Drawing.Point(110, 3);
            this.TwitchBotPublicTextBox.Name = "TwitchBotPublicTextBox";
            this.TwitchBotPublicTextBox.Size = new System.Drawing.Size(250, 23);
            this.TwitchBotPublicTextBox.TabIndex = 1;
            // 
            // TwitchBotPublicLabel
            // 
            this.TwitchBotPublicLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.TwitchBotPublicLabel.Location = new System.Drawing.Point(0, 3);
            this.TwitchBotPublicLabel.Name = "TwitchBotPublicLabel";
            this.TwitchBotPublicLabel.Size = new System.Drawing.Size(110, 23);
            this.TwitchBotPublicLabel.TabIndex = 0;
            this.TwitchBotPublicLabel.Text = "Bot Public:";
            this.TwitchBotPublicLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TwitchBotChannel
            // 
            this.TwitchBotChannel.Controls.Add(this.TwitchChannelTextBox);
            this.TwitchBotChannel.Controls.Add(this.TwitchChannelLabel);
            this.TwitchBotChannel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TwitchBotChannel.Location = new System.Drawing.Point(3, 26);
            this.TwitchBotChannel.Name = "TwitchBotChannel";
            this.TwitchBotChannel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.TwitchBotChannel.Size = new System.Drawing.Size(425, 29);
            this.TwitchBotChannel.TabIndex = 3;
            // 
            // TwitchChannelTextBox
            // 
            this.TwitchChannelTextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.TwitchChannelTextBox.Location = new System.Drawing.Point(110, 3);
            this.TwitchChannelTextBox.Name = "TwitchChannelTextBox";
            this.TwitchChannelTextBox.Size = new System.Drawing.Size(250, 23);
            this.TwitchChannelTextBox.TabIndex = 1;
            // 
            // TwitchChannelLabel
            // 
            this.TwitchChannelLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.TwitchChannelLabel.Location = new System.Drawing.Point(0, 3);
            this.TwitchChannelLabel.Name = "TwitchChannelLabel";
            this.TwitchChannelLabel.Size = new System.Drawing.Size(110, 23);
            this.TwitchChannelLabel.TabIndex = 0;
            this.TwitchChannelLabel.Text = "Channel Name:";
            this.TwitchChannelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // OptionPanel
            // 
            this.OptionPanel.Controls.Add(this.SaveButton);
            this.OptionPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.OptionPanel.Location = new System.Drawing.Point(0, 226);
            this.OptionPanel.Name = "OptionPanel";
            this.OptionPanel.Padding = new System.Windows.Forms.Padding(5);
            this.OptionPanel.Size = new System.Drawing.Size(484, 35);
            this.OptionPanel.TabIndex = 2;
            // 
            // SaveButton
            // 
            this.SaveButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.SaveButton.Location = new System.Drawing.Point(404, 5);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 25);
            this.SaveButton.TabIndex = 0;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // SystemPanel
            // 
            this.SystemPanel.Controls.Add(this.SystemBrowserFileDialog);
            this.SystemPanel.Controls.Add(this.SystemBrowserTextBox);
            this.SystemPanel.Controls.Add(this.SystemBrowserLabel);
            this.SystemPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.SystemPanel.Location = new System.Drawing.Point(3, 3);
            this.SystemPanel.Name = "SystemPanel";
            this.SystemPanel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.SystemPanel.Size = new System.Drawing.Size(425, 29);
            this.SystemPanel.TabIndex = 3;
            // 
            // SystemBrowserFileDialog
            // 
            this.SystemBrowserFileDialog.BackgroundImage = global::StreamGlass.Properties.Resources.magnifying_glass;
            this.SystemBrowserFileDialog.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SystemBrowserFileDialog.Dock = System.Windows.Forms.DockStyle.Left;
            this.SystemBrowserFileDialog.FlatAppearance.BorderSize = 0;
            this.SystemBrowserFileDialog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SystemBrowserFileDialog.Location = new System.Drawing.Point(360, 3);
            this.SystemBrowserFileDialog.Margin = new System.Windows.Forms.Padding(0);
            this.SystemBrowserFileDialog.Name = "SystemBrowserFileDialog";
            this.SystemBrowserFileDialog.Size = new System.Drawing.Size(23, 23);
            this.SystemBrowserFileDialog.TabIndex = 2;
            this.SystemBrowserFileDialog.UseVisualStyleBackColor = true;
            this.SystemBrowserFileDialog.Click += new System.EventHandler(this.SystemBrowserFileDialog_Click);
            // 
            // SystemBrowserTextBox
            // 
            this.SystemBrowserTextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.SystemBrowserTextBox.Location = new System.Drawing.Point(110, 3);
            this.SystemBrowserTextBox.Name = "SystemBrowserTextBox";
            this.SystemBrowserTextBox.Size = new System.Drawing.Size(250, 23);
            this.SystemBrowserTextBox.TabIndex = 1;
            // 
            // SystemBrowserLabel
            // 
            this.SystemBrowserLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.SystemBrowserLabel.Location = new System.Drawing.Point(0, 3);
            this.SystemBrowserLabel.Name = "SystemBrowserLabel";
            this.SystemBrowserLabel.Size = new System.Drawing.Size(110, 23);
            this.SystemBrowserLabel.TabIndex = 0;
            this.SystemBrowserLabel.Text = "Browser:";
            this.SystemBrowserLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsTabs
            // 
            this.SettingsTabs.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.SettingsTabs.Controls.Add(this.GeneralTabPage);
            this.SettingsTabs.Controls.Add(this.TwitchTabPage);
            this.SettingsTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SettingsTabs.ImageList = this.TabIconImageList;
            this.SettingsTabs.ItemSize = new System.Drawing.Size(45, 45);
            this.SettingsTabs.Location = new System.Drawing.Point(0, 0);
            this.SettingsTabs.Margin = new System.Windows.Forms.Padding(0);
            this.SettingsTabs.Multiline = true;
            this.SettingsTabs.Name = "SettingsTabs";
            this.SettingsTabs.SelectedIndex = 0;
            this.SettingsTabs.Size = new System.Drawing.Size(484, 226);
            this.SettingsTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.SettingsTabs.TabIndex = 4;
            // 
            // GeneralTabPage
            // 
            this.GeneralTabPage.AutoScroll = true;
            this.GeneralTabPage.Controls.Add(this.SystemPanel);
            this.GeneralTabPage.ImageIndex = 0;
            this.GeneralTabPage.Location = new System.Drawing.Point(49, 4);
            this.GeneralTabPage.Name = "GeneralTabPage";
            this.GeneralTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.GeneralTabPage.Size = new System.Drawing.Size(431, 218);
            this.GeneralTabPage.TabIndex = 0;
            this.GeneralTabPage.UseVisualStyleBackColor = true;
            // 
            // TwitchTabPage
            // 
            this.TwitchTabPage.AutoScroll = true;
            this.TwitchTabPage.Controls.Add(this.TwitchBotSecret);
            this.TwitchTabPage.Controls.Add(this.TwitchBotPublic);
            this.TwitchTabPage.Controls.Add(this.TwitchBotChannel);
            this.TwitchTabPage.Controls.Add(this.TwitchConnectButton);
            this.TwitchTabPage.ImageIndex = 1;
            this.TwitchTabPage.Location = new System.Drawing.Point(49, 4);
            this.TwitchTabPage.Name = "TwitchTabPage";
            this.TwitchTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.TwitchTabPage.Size = new System.Drawing.Size(431, 218);
            this.TwitchTabPage.TabIndex = 1;
            this.TwitchTabPage.UseVisualStyleBackColor = true;
            // 
            // TwitchConnectButton
            // 
            this.TwitchConnectButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.TwitchConnectButton.Location = new System.Drawing.Point(3, 3);
            this.TwitchConnectButton.Name = "TwitchConnectButton";
            this.TwitchConnectButton.Size = new System.Drawing.Size(425, 23);
            this.TwitchConnectButton.TabIndex = 5;
            this.TwitchConnectButton.Text = "Connect";
            this.TwitchConnectButton.UseVisualStyleBackColor = true;
            this.TwitchConnectButton.Click += new System.EventHandler(this.TwitchConnectButton_Click);
            // 
            // TabIconImageList
            // 
            this.TabIconImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.TabIconImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("TabIconImageList.ImageStream")));
            this.TabIconImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.TabIconImageList.Images.SetKeyName(0, "tinker.png");
            this.TabIconImageList.Images.SetKeyName(1, "580b57fcd9996e24bc43c540.png");
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 261);
            this.Controls.Add(this.SettingsTabs);
            this.Controls.Add(this.OptionPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.TwitchBotSecret.ResumeLayout(false);
            this.TwitchBotSecret.PerformLayout();
            this.TwitchBotPublic.ResumeLayout(false);
            this.TwitchBotPublic.PerformLayout();
            this.TwitchBotChannel.ResumeLayout(false);
            this.TwitchBotChannel.PerformLayout();
            this.OptionPanel.ResumeLayout(false);
            this.SystemPanel.ResumeLayout(false);
            this.SystemPanel.PerformLayout();
            this.SettingsTabs.ResumeLayout(false);
            this.GeneralTabPage.ResumeLayout(false);
            this.TwitchTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Panel OptionPanel;
        private Button SaveButton;
        private Panel TwitchBotSecret;
        private Label TwitchBotSecretLabel;
        private TextBox TwitchBotSecretTextBox;
        private Panel SystemPanel;
        private TextBox SystemBrowserTextBox;
        private Label SystemBrowserLabel;
        private Button SystemBrowserFileDialog;
        private Button TwitchBotSecretVisibility;
        private Panel TwitchBotPublic;
        private TextBox TwitchBotPublicTextBox;
        private Label TwitchBotPublicLabel;
        private Panel TwitchBotChannel;
        private TextBox TwitchChannelTextBox;
        private Label TwitchChannelLabel;
        private TabControl SettingsTabs;
        private TabPage GeneralTabPage;
        private TabPage TwitchTabPage;
        private ImageList TabIconImageList;
        private Button TwitchConnectButton;
    }
}