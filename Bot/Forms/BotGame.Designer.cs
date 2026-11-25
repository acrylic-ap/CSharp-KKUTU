namespace Manager
{
    partial class BotGame
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
            usedWordList = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // turn
            // 
            turn.Location = new Point(20, 95);
            // 
            // initial
            // 
            initial.Location = new Point(20, 153);
            initial.Click += initial_Click;
            // 
            // space
            // 
            space.Location = new Point(20, 119);
            // 
            // usedWordList
            // 
            usedWordList.AutoScroll = true;
            usedWordList.Dock = DockStyle.Top;
            usedWordList.Location = new Point(20, 256);
            usedWordList.Name = "usedWordList";
            usedWordList.Size = new Size(760, 75);
            usedWordList.TabIndex = 3;
            usedWordList.WrapContents = false;
            // 
            // BotGame
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(usedWordList);
            Name = "BotGame";
            Text = "봇 게임";
            Shown += BotGame_Shown;
            Controls.SetChildIndex(shield, 0);
            Controls.SetChildIndex(turn, 0);
            Controls.SetChildIndex(space, 0);
            Controls.SetChildIndex(input, 0);
            Controls.SetChildIndex(initial, 0);
            Controls.SetChildIndex(usedWordList, 0);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlowLayoutPanel usedWordList;
    }
}
