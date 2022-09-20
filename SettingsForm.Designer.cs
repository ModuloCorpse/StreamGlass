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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.TwitchGroup = new System.Windows.Forms.GroupBox();
            this.TwitchBotSecret = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.TwitchBotSecretTextBox = new System.Windows.Forms.TextBox();
            this.TwitchSecretLabel = new System.Windows.Forms.Label();
            this.TwitchBotPublic = new System.Windows.Forms.Panel();
            this.TwitchBotPublicTextBox = new System.Windows.Forms.TextBox();
            this.TwitchBotPublicLabel = new System.Windows.Forms.Label();
            this.TwitchBotChannel = new System.Windows.Forms.Panel();
            this.TwitchChannelTextBox = new System.Windows.Forms.TextBox();
            this.TwitchChannelLabel = new System.Windows.Forms.Label();
            this.OptionPanel = new System.Windows.Forms.Panel();
            this.SaveButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.SystemBrowserFileDialog = new System.Windows.Forms.Button();
            this.SystemBrowserTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TwitchGroup.SuspendLayout();
            this.TwitchBotSecret.SuspendLayout();
            this.TwitchBotPublic.SuspendLayout();
            this.TwitchBotChannel.SuspendLayout();
            this.OptionPanel.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // TwitchGroup
            // 
            this.TwitchGroup.Controls.Add(this.TwitchBotSecret);
            this.TwitchGroup.Controls.Add(this.TwitchBotPublic);
            this.TwitchGroup.Controls.Add(this.TwitchBotChannel);
            this.TwitchGroup.Dock = System.Windows.Forms.DockStyle.Top;
            this.TwitchGroup.Location = new System.Drawing.Point(0, 29);
            this.TwitchGroup.Name = "TwitchGroup";
            this.TwitchGroup.Size = new System.Drawing.Size(384, 110);
            this.TwitchGroup.TabIndex = 0;
            this.TwitchGroup.TabStop = false;
            this.TwitchGroup.Text = "Twitch";
            // 
            // TwitchBotSecret
            // 
            this.TwitchBotSecret.Controls.Add(this.button1);
            this.TwitchBotSecret.Controls.Add(this.TwitchBotSecretTextBox);
            this.TwitchBotSecret.Controls.Add(this.TwitchSecretLabel);
            this.TwitchBotSecret.Dock = System.Windows.Forms.DockStyle.Top;
            this.TwitchBotSecret.Location = new System.Drawing.Point(3, 77);
            this.TwitchBotSecret.Name = "TwitchBotSecret";
            this.TwitchBotSecret.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.TwitchBotSecret.Size = new System.Drawing.Size(378, 29);
            this.TwitchBotSecret.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Transparent;
            this.button1.BackgroundImage = global::StreamGlass.Properties.Resources.sight_disabled;
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button1.Dock = System.Windows.Forms.DockStyle.Left;
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(355, 3);
            this.button1.Margin = new System.Windows.Forms.Padding(0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(23, 23);
            this.button1.TabIndex = 3;
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // TwitchBotSecretTextBox
            // 
            this.TwitchBotSecretTextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.TwitchBotSecretTextBox.Location = new System.Drawing.Point(110, 3);
            this.TwitchBotSecretTextBox.Name = "TwitchBotSecretTextBox";
            this.TwitchBotSecretTextBox.PasswordChar = '*';
            this.TwitchBotSecretTextBox.Size = new System.Drawing.Size(245, 23);
            this.TwitchBotSecretTextBox.TabIndex = 1;
            // 
            // TwitchSecretLabel
            // 
            this.TwitchSecretLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.TwitchSecretLabel.Location = new System.Drawing.Point(0, 3);
            this.TwitchSecretLabel.Name = "TwitchSecretLabel";
            this.TwitchSecretLabel.Size = new System.Drawing.Size(110, 23);
            this.TwitchSecretLabel.TabIndex = 0;
            this.TwitchSecretLabel.Text = "Bot Secret:";
            this.TwitchSecretLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TwitchBotPublic
            // 
            this.TwitchBotPublic.Controls.Add(this.TwitchBotPublicTextBox);
            this.TwitchBotPublic.Controls.Add(this.TwitchBotPublicLabel);
            this.TwitchBotPublic.Dock = System.Windows.Forms.DockStyle.Top;
            this.TwitchBotPublic.Location = new System.Drawing.Point(3, 48);
            this.TwitchBotPublic.Name = "TwitchBotPublic";
            this.TwitchBotPublic.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.TwitchBotPublic.Size = new System.Drawing.Size(378, 29);
            this.TwitchBotPublic.TabIndex = 4;
            // 
            // TwitchBotPublicTextBox
            // 
            this.TwitchBotPublicTextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.TwitchBotPublicTextBox.Location = new System.Drawing.Point(110, 3);
            this.TwitchBotPublicTextBox.Name = "TwitchBotPublicTextBox";
            this.TwitchBotPublicTextBox.Size = new System.Drawing.Size(268, 23);
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
            this.TwitchBotChannel.Location = new System.Drawing.Point(3, 19);
            this.TwitchBotChannel.Name = "TwitchBotChannel";
            this.TwitchBotChannel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.TwitchBotChannel.Size = new System.Drawing.Size(378, 29);
            this.TwitchBotChannel.TabIndex = 3;
            // 
            // TwitchChannelTextBox
            // 
            this.TwitchChannelTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TwitchChannelTextBox.Location = new System.Drawing.Point(110, 3);
            this.TwitchChannelTextBox.Name = "TwitchChannelTextBox";
            this.TwitchChannelTextBox.Size = new System.Drawing.Size(268, 23);
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
            this.OptionPanel.Location = new System.Drawing.Point(0, 426);
            this.OptionPanel.Name = "OptionPanel";
            this.OptionPanel.Padding = new System.Windows.Forms.Padding(5);
            this.OptionPanel.Size = new System.Drawing.Size(384, 35);
            this.OptionPanel.TabIndex = 2;
            // 
            // SaveButton
            // 
            this.SaveButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.SaveButton.Location = new System.Drawing.Point(304, 5);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 25);
            this.SaveButton.TabIndex = 0;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.SystemBrowserFileDialog);
            this.panel2.Controls.Add(this.SystemBrowserTextBox);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panel2.Size = new System.Drawing.Size(384, 29);
            this.panel2.TabIndex = 3;
            // 
            // SystemBrowserFileDialog
            // 
            this.SystemBrowserFileDialog.Dock = System.Windows.Forms.DockStyle.Left;
            this.SystemBrowserFileDialog.Location = new System.Drawing.Point(360, 3);
            this.SystemBrowserFileDialog.Margin = new System.Windows.Forms.Padding(0);
            this.SystemBrowserFileDialog.Name = "SystemBrowserFileDialog";
            this.SystemBrowserFileDialog.Size = new System.Drawing.Size(23, 23);
            this.SystemBrowserFileDialog.TabIndex = 2;
            this.SystemBrowserFileDialog.Text = "...";
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
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(0, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Browser:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 461);
            this.Controls.Add(this.TwitchGroup);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.OptionPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.TwitchGroup.ResumeLayout(false);
            this.TwitchBotSecret.ResumeLayout(false);
            this.TwitchBotSecret.PerformLayout();
            this.TwitchBotPublic.ResumeLayout(false);
            this.TwitchBotPublic.PerformLayout();
            this.TwitchBotChannel.ResumeLayout(false);
            this.TwitchBotChannel.PerformLayout();
            this.OptionPanel.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GroupBox TwitchGroup;
        private Panel OptionPanel;
        private Button SaveButton;
        private Panel TwitchBotSecret;
        private Label TwitchSecretLabel;
        private TextBox TwitchBotSecretTextBox;
        private Panel panel2;
        private TextBox SystemBrowserTextBox;
        private Label label1;
        private Button SystemBrowserFileDialog;
        private Button button1;
        private Panel TwitchBotPublic;
        private TextBox TwitchBotPublicTextBox;
        private Label TwitchBotPublicLabel;
        private Panel TwitchBotChannel;
        private TextBox TwitchChannelTextBox;
        private Label TwitchChannelLabel;
    }
}