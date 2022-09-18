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
            this.panel8 = new System.Windows.Forms.Panel();
            this.TwitchBotTokenTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.panel7 = new System.Windows.Forms.Panel();
            this.TwitchBotNameTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.TwitchChannelTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.TwitchSecretVisibilityButton = new System.Windows.Forms.Button();
            this.TwitchSecretTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.TwitchPublicTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.DiscordGroup = new System.Windows.Forms.GroupBox();
            this.panel6 = new System.Windows.Forms.Panel();
            this.DiscordSecretVisibilityButton = new System.Windows.Forms.Button();
            this.DiscordSecretTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.DiscordPublicTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.SaveButton = new System.Windows.Forms.Button();
            this.TwitchTokenVisibilityButton = new System.Windows.Forms.Button();
            this.TwitchGroup.SuspendLayout();
            this.panel8.SuspendLayout();
            this.panel7.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.DiscordGroup.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TwitchGroup
            // 
            this.TwitchGroup.Controls.Add(this.panel8);
            this.TwitchGroup.Controls.Add(this.panel7);
            this.TwitchGroup.Controls.Add(this.panel4);
            this.TwitchGroup.Controls.Add(this.panel3);
            this.TwitchGroup.Controls.Add(this.panel2);
            this.TwitchGroup.Dock = System.Windows.Forms.DockStyle.Top;
            this.TwitchGroup.Location = new System.Drawing.Point(0, 0);
            this.TwitchGroup.Name = "TwitchGroup";
            this.TwitchGroup.Size = new System.Drawing.Size(384, 165);
            this.TwitchGroup.TabIndex = 0;
            this.TwitchGroup.TabStop = false;
            this.TwitchGroup.Text = "Twitch";
            // 
            // panel8
            // 
            this.panel8.Controls.Add(this.TwitchTokenVisibilityButton);
            this.panel8.Controls.Add(this.TwitchBotTokenTextBox);
            this.panel8.Controls.Add(this.label7);
            this.panel8.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel8.Location = new System.Drawing.Point(3, 135);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(378, 23);
            this.panel8.TabIndex = 4;
            // 
            // TwitchBotTokenTextBox
            // 
            this.TwitchBotTokenTextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.TwitchBotTokenTextBox.Location = new System.Drawing.Point(110, 0);
            this.TwitchBotTokenTextBox.Name = "TwitchBotTokenTextBox";
            this.TwitchBotTokenTextBox.PasswordChar = '*';
            this.TwitchBotTokenTextBox.Size = new System.Drawing.Size(245, 23);
            this.TwitchBotTokenTextBox.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.Dock = System.Windows.Forms.DockStyle.Left;
            this.label7.Location = new System.Drawing.Point(0, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(110, 23);
            this.label7.TabIndex = 0;
            this.label7.Text = "Token:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.TwitchBotNameTextBox);
            this.panel7.Controls.Add(this.label6);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel7.Location = new System.Drawing.Point(3, 106);
            this.panel7.Name = "panel7";
            this.panel7.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panel7.Size = new System.Drawing.Size(378, 29);
            this.panel7.TabIndex = 3;
            // 
            // TwitchBotNameTextBox
            // 
            this.TwitchBotNameTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TwitchBotNameTextBox.Location = new System.Drawing.Point(110, 3);
            this.TwitchBotNameTextBox.Name = "TwitchBotNameTextBox";
            this.TwitchBotNameTextBox.Size = new System.Drawing.Size(268, 23);
            this.TwitchBotNameTextBox.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.Dock = System.Windows.Forms.DockStyle.Left;
            this.label6.Location = new System.Drawing.Point(0, 3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(110, 23);
            this.label6.TabIndex = 0;
            this.label6.Text = "Bot Name:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.TwitchChannelTextBox);
            this.panel4.Controls.Add(this.label3);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(3, 77);
            this.panel4.Name = "panel4";
            this.panel4.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panel4.Size = new System.Drawing.Size(378, 29);
            this.panel4.TabIndex = 2;
            // 
            // TwitchChannelTextBox
            // 
            this.TwitchChannelTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TwitchChannelTextBox.Location = new System.Drawing.Point(110, 3);
            this.TwitchChannelTextBox.Name = "TwitchChannelTextBox";
            this.TwitchChannelTextBox.Size = new System.Drawing.Size(268, 23);
            this.TwitchChannelTextBox.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Left;
            this.label3.Location = new System.Drawing.Point(0, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 23);
            this.label3.TabIndex = 0;
            this.label3.Text = "Channel Name:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.TwitchSecretVisibilityButton);
            this.panel3.Controls.Add(this.TwitchSecretTextBox);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(3, 48);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panel3.Size = new System.Drawing.Size(378, 29);
            this.panel3.TabIndex = 1;
            // 
            // TwitchSecretVisibilityButton
            // 
            this.TwitchSecretVisibilityButton.BackgroundImage = global::StreamGlass.Properties.Resources.sight_disabled;
            this.TwitchSecretVisibilityButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.TwitchSecretVisibilityButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.TwitchSecretVisibilityButton.FlatAppearance.BorderSize = 0;
            this.TwitchSecretVisibilityButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TwitchSecretVisibilityButton.Location = new System.Drawing.Point(355, 3);
            this.TwitchSecretVisibilityButton.Name = "TwitchSecretVisibilityButton";
            this.TwitchSecretVisibilityButton.Size = new System.Drawing.Size(23, 23);
            this.TwitchSecretVisibilityButton.TabIndex = 2;
            this.TwitchSecretVisibilityButton.UseVisualStyleBackColor = true;
            this.TwitchSecretVisibilityButton.Click += new System.EventHandler(this.TwitchSecretVisibilityButton_Click);
            // 
            // TwitchSecretTextBox
            // 
            this.TwitchSecretTextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.TwitchSecretTextBox.Location = new System.Drawing.Point(110, 3);
            this.TwitchSecretTextBox.Name = "TwitchSecretTextBox";
            this.TwitchSecretTextBox.PasswordChar = '*';
            this.TwitchSecretTextBox.Size = new System.Drawing.Size(245, 23);
            this.TwitchSecretTextBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Left;
            this.label2.Location = new System.Drawing.Point(0, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 23);
            this.label2.TabIndex = 0;
            this.label2.Text = "Bot Private Token:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.TwitchPublicTextBox);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(3, 19);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panel2.Size = new System.Drawing.Size(378, 29);
            this.panel2.TabIndex = 0;
            // 
            // TwitchPublicTextBox
            // 
            this.TwitchPublicTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TwitchPublicTextBox.Location = new System.Drawing.Point(110, 3);
            this.TwitchPublicTextBox.Name = "TwitchPublicTextBox";
            this.TwitchPublicTextBox.Size = new System.Drawing.Size(268, 23);
            this.TwitchPublicTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(0, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Bot public token:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DiscordGroup
            // 
            this.DiscordGroup.Controls.Add(this.panel6);
            this.DiscordGroup.Controls.Add(this.panel5);
            this.DiscordGroup.Dock = System.Windows.Forms.DockStyle.Top;
            this.DiscordGroup.Location = new System.Drawing.Point(0, 165);
            this.DiscordGroup.Name = "DiscordGroup";
            this.DiscordGroup.Size = new System.Drawing.Size(384, 85);
            this.DiscordGroup.TabIndex = 1;
            this.DiscordGroup.TabStop = false;
            this.DiscordGroup.Text = "Discord";
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.DiscordSecretVisibilityButton);
            this.panel6.Controls.Add(this.DiscordSecretTextBox);
            this.panel6.Controls.Add(this.label5);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(3, 48);
            this.panel6.Name = "panel6";
            this.panel6.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panel6.Size = new System.Drawing.Size(378, 29);
            this.panel6.TabIndex = 1;
            // 
            // DiscordSecretVisibilityButton
            // 
            this.DiscordSecretVisibilityButton.BackgroundImage = global::StreamGlass.Properties.Resources.sight_disabled;
            this.DiscordSecretVisibilityButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.DiscordSecretVisibilityButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.DiscordSecretVisibilityButton.FlatAppearance.BorderSize = 0;
            this.DiscordSecretVisibilityButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DiscordSecretVisibilityButton.Location = new System.Drawing.Point(355, 3);
            this.DiscordSecretVisibilityButton.Name = "DiscordSecretVisibilityButton";
            this.DiscordSecretVisibilityButton.Size = new System.Drawing.Size(23, 23);
            this.DiscordSecretVisibilityButton.TabIndex = 2;
            this.DiscordSecretVisibilityButton.UseVisualStyleBackColor = true;
            this.DiscordSecretVisibilityButton.Click += new System.EventHandler(this.DiscordSecretVisibilityButton_Click);
            // 
            // DiscordSecretTextBox
            // 
            this.DiscordSecretTextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.DiscordSecretTextBox.Location = new System.Drawing.Point(110, 3);
            this.DiscordSecretTextBox.Name = "DiscordSecretTextBox";
            this.DiscordSecretTextBox.PasswordChar = '*';
            this.DiscordSecretTextBox.Size = new System.Drawing.Size(245, 23);
            this.DiscordSecretTextBox.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.Dock = System.Windows.Forms.DockStyle.Left;
            this.label5.Location = new System.Drawing.Point(0, 3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(110, 23);
            this.label5.TabIndex = 0;
            this.label5.Text = "Bot Private Token:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.DiscordPublicTextBox);
            this.panel5.Controls.Add(this.label4);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(3, 19);
            this.panel5.Name = "panel5";
            this.panel5.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panel5.Size = new System.Drawing.Size(378, 29);
            this.panel5.TabIndex = 0;
            // 
            // DiscordPublicTextBox
            // 
            this.DiscordPublicTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiscordPublicTextBox.Location = new System.Drawing.Point(110, 3);
            this.DiscordPublicTextBox.Name = "DiscordPublicTextBox";
            this.DiscordPublicTextBox.Size = new System.Drawing.Size(268, 23);
            this.DiscordPublicTextBox.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Left;
            this.label4.Location = new System.Drawing.Point(0, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 23);
            this.label4.TabIndex = 0;
            this.label4.Text = "Bot public token:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.SaveButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 426);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5);
            this.panel1.Size = new System.Drawing.Size(384, 35);
            this.panel1.TabIndex = 2;
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
            // TwitchTokenVisibilityButton
            // 
            this.TwitchTokenVisibilityButton.BackgroundImage = global::StreamGlass.Properties.Resources.sight_disabled;
            this.TwitchTokenVisibilityButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.TwitchTokenVisibilityButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.TwitchTokenVisibilityButton.FlatAppearance.BorderSize = 0;
            this.TwitchTokenVisibilityButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TwitchTokenVisibilityButton.Location = new System.Drawing.Point(355, 0);
            this.TwitchTokenVisibilityButton.Name = "TwitchTokenVisibilityButton";
            this.TwitchTokenVisibilityButton.Size = new System.Drawing.Size(23, 23);
            this.TwitchTokenVisibilityButton.TabIndex = 3;
            this.TwitchTokenVisibilityButton.UseVisualStyleBackColor = true;
            this.TwitchTokenVisibilityButton.Click += new System.EventHandler(this.TwitchTokenVisibilityButton_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 461);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.DiscordGroup);
            this.Controls.Add(this.TwitchGroup);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.TwitchGroup.ResumeLayout(false);
            this.panel8.ResumeLayout(false);
            this.panel8.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.DiscordGroup.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GroupBox TwitchGroup;
        private GroupBox DiscordGroup;
        private Panel panel1;
        private Button SaveButton;
        private Panel panel2;
        private Label label1;
        private Panel panel4;
        private Panel panel3;
        private Panel panel6;
        private Panel panel5;
        private Label label3;
        private Label label2;
        private Label label5;
        private Label label4;
        private TextBox TwitchChannelTextBox;
        private TextBox TwitchSecretTextBox;
        private TextBox TwitchPublicTextBox;
        private TextBox DiscordSecretTextBox;
        private TextBox DiscordPublicTextBox;
        private Panel panel7;
        private Label label6;
        private TextBox TwitchBotNameTextBox;
        private Button TwitchSecretVisibilityButton;
        private Button DiscordSecretVisibilityButton;
        private Panel panel8;
        private TextBox TwitchBotTokenTextBox;
        private Label label7;
        private Button TwitchTokenVisibilityButton;
    }
}