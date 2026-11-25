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
            components = new System.ComponentModel.Container();
            label1 = new Label();
            multiPlayButton = new Button();
            aiPlayButton = new Button();
            loading = new Label();
            matchingTimer = new System.Windows.Forms.Timer(components);
            matchingTime = new Label();
            matchingCancelButton = new Button();
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
            multiPlayButton.Text = "멀티 플레이";
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
            loading.Location = new Point(892, 488);
            loading.Name = "loading";
            loading.Size = new Size(354, 202);
            loading.TabIndex = 5;
            loading.Text = "데이터를 불러오는 중입니다...";
            loading.TextAlign = ContentAlignment.MiddleCenter;
            loading.Click += loading_Click;
            // 
            // matchingTimer
            // 
            matchingTimer.Tick += matchingTimer_Tick;
            // 
            // matchingTime
            // 
            matchingTime.AutoSize = true;
            matchingTime.Font = new Font("Pretendard", 13F);
            matchingTime.Location = new Point(22, 25);
            matchingTime.Name = "matchingTime";
            matchingTime.Size = new Size(138, 52);
            matchingTime.TabIndex = 6;
            matchingTime.Text = "매칭 대기 시간\r\n[00:00]";
            matchingTime.TextAlign = ContentAlignment.MiddleCenter;
            matchingTime.Visible = false;
            // 
            // matchingCancelButton
            // 
            matchingCancelButton.Location = new Point(42, 92);
            matchingCancelButton.Name = "matchingCancelButton";
            matchingCancelButton.Size = new Size(94, 29);
            matchingCancelButton.TabIndex = 7;
            matchingCancelButton.Text = "매칭 취소";
            matchingCancelButton.UseVisualStyleBackColor = true;
            matchingCancelButton.Visible = false;
            matchingCancelButton.Click += matchingCancelButton_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(960, 514);
            Controls.Add(matchingCancelButton);
            Controls.Add(matchingTime);
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
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Button multiPlayButton;
        private Button aiPlayButton;
        private Label loading;
        private System.Windows.Forms.Timer matchingTimer;
        private Label matchingTime;
        private Button matchingCancelButton;
    }
}
