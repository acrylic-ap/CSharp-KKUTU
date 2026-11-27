namespace MultiPlay
{
    partial class MultiGame
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // MultiGame
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            ClientSize = new Size(800, 450);
            Name = "MultiGame";
            Text = "멀티플레이";
            Load += MultiGame_Load;
            Shown += MultiGame_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}