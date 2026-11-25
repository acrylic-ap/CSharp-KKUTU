namespace KKUTU
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            multiPlayButton = new Button();
            aiPlayButton = new Button();
            loading = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Dock = DockStyle.Top;
            label1.Font = new Font("Pretendard", 26F);
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(960, 190);
            label1.TabIndex = 0;
            label1.Text = "그두";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label1.Click += label1_Click;
            // 
            // multiPlayButton
            // 
            multiPlayButton.Font = new Font("Pretendard", 14F);
            multiPlayButton.Location = new Point(198, 277);
            multiPlayButton.Name = "multiPlayButton";
            multiPlayButton.Size = new Size(214, 79);
            multiPlayButton.TabIndex = 1;
            multiPlayButton.Text = "2인";
            multiPlayButton.UseVisualStyleBackColor = true;
            multiPlayButton.Click += multiPlayButton_Click;
            // 
            // aiPlayButton
            // 
            aiPlayButton.Font = new Font("Pretendard", 14F);
            aiPlayButton.Location = new Point(554, 277);
            aiPlayButton.Name = "aiPlayButton";
            aiPlayButton.Size = new Size(214, 79);
            aiPlayButton.TabIndex = 2;
            aiPlayButton.Text = "봇";
            aiPlayButton.UseVisualStyleBackColor = true;
            aiPlayButton.Click += aiPlayButton_Click;
            // 
            // loading
            // 
            loading.Font = new Font("Pretendard", 18F);
            loading.Location = new Point(0, 0);
            loading.Name = "loading";
            loading.Size = new Size(960, 517);
            loading.TabIndex = 5;
            loading.Text = "데이터를 불러오는 중입니다...";
            loading.TextAlign = ContentAlignment.MiddleCenter;
            loading.Click += loading_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(960, 514);
            Controls.Add(loading);
            Controls.Add(aiPlayButton);
            Controls.Add(multiPlayButton);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "Main";
            Text = "GGUDU";
            Load += Form1_Load;
            Shown += Form1_Shown;
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private Button multiPlayButton;
        private Button aiPlayButton;
        private Label loading;
    }
}
