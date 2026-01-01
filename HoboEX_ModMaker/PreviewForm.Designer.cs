namespace HoboEX_ModMaker
{
    partial class PreviewForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblNpcName;
        private System.Windows.Forms.Label lblNpcText;
        private System.Windows.Forms.FlowLayoutPanel flowOptions;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblNpcName = new System.Windows.Forms.Label();
            this.lblNpcText = new System.Windows.Forms.Label();
            this.flowOptions = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // lblNpcName
            // 
            this.lblNpcName.AutoSize = true;
            this.lblNpcName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblNpcName.Location = new System.Drawing.Point(30, 30);
            this.lblNpcName.Name = "lblNpcName";
            this.lblNpcName.Size = new System.Drawing.Size(95, 21);
            this.lblNpcName.TabIndex = 0;
            this.lblNpcName.Text = "NPC NAME";
            // 
            // lblNpcText
            // 
            this.lblNpcText.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblNpcText.Location = new System.Drawing.Point(30, 65);
            this.lblNpcText.Name = "lblNpcText";
            this.lblNpcText.Size = new System.Drawing.Size(540, 80);
            this.lblNpcText.TabIndex = 1;
            this.lblNpcText.Text = "Sample Dialogue Text Content...";
            // 
            // flowOptions
            // 
            this.flowOptions.AutoScroll = true;
            this.flowOptions.Location = new System.Drawing.Point(30, 160);
            this.flowOptions.Name = "flowOptions";
            this.flowOptions.Size = new System.Drawing.Size(540, 220);
            this.flowOptions.TabIndex = 2;
            // 
            // PreviewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 420);
            this.Controls.Add(this.flowOptions);
            this.Controls.Add(this.lblNpcText);
            this.Controls.Add(this.lblNpcName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "PreviewForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dialogue Preview";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
