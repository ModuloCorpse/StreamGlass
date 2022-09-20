namespace StreamGlass
{
    partial class ChatMessage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SenderLabel = new System.Windows.Forms.Label();
            this.MessagePanel = new System.Windows.Forms.Panel();
            this.MessageView = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.MessagePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageView)).BeginInit();
            this.SuspendLayout();
            // 
            // SenderLabel
            // 
            this.SenderLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.SenderLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.SenderLabel.Location = new System.Drawing.Point(0, 0);
            this.SenderLabel.MaximumSize = new System.Drawing.Size(0, 25);
            this.SenderLabel.MinimumSize = new System.Drawing.Size(0, 25);
            this.SenderLabel.Name = "SenderLabel";
            this.SenderLabel.Size = new System.Drawing.Size(110, 25);
            this.SenderLabel.TabIndex = 0;
            this.SenderLabel.Text = "StreamGlass :";
            this.SenderLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // MessagePanel
            // 
            this.MessagePanel.AutoSize = true;
            this.MessagePanel.Controls.Add(this.MessageView);
            this.MessagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessagePanel.Location = new System.Drawing.Point(110, 0);
            this.MessagePanel.MaximumSize = new System.Drawing.Size(240, 0);
            this.MessagePanel.MinimumSize = new System.Drawing.Size(240, 25);
            this.MessagePanel.Name = "MessagePanel";
            this.MessagePanel.Size = new System.Drawing.Size(240, 63);
            this.MessagePanel.TabIndex = 2;
            // 
            // MessageView
            // 
            this.MessageView.AllowExternalDrop = true;
            this.MessageView.CreationProperties = null;
            this.MessageView.DefaultBackgroundColor = System.Drawing.Color.White;
            this.MessageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageView.Location = new System.Drawing.Point(0, 0);
            this.MessageView.MaximumSize = new System.Drawing.Size(240, 0);
            this.MessageView.MinimumSize = new System.Drawing.Size(240, 0);
            this.MessageView.Name = "MessageView";
            this.MessageView.Size = new System.Drawing.Size(240, 63);
            this.MessageView.TabIndex = 0;
            this.MessageView.ZoomFactor = 1D;
            // 
            // ChatMessage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Gray;
            this.Controls.Add(this.MessagePanel);
            this.Controls.Add(this.SenderLabel);
            this.MaximumSize = new System.Drawing.Size(350, 0);
            this.MinimumSize = new System.Drawing.Size(350, 25);
            this.Name = "ChatMessage";
            this.Size = new System.Drawing.Size(350, 63);
            this.MessagePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MessageView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label SenderLabel;
        private Panel MessagePanel;
        private Microsoft.Web.WebView2.WinForms.WebView2 MessageView;
    }
}
