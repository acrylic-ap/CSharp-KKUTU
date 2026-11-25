namespace UI.Forms
{
    partial class GameForm
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
            turn = new Label();
            initial = new Label();
            input = new TextBox();
            space = new Panel();
            usedWordList = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // turn
            // 
            turn.AutoSize = true;
            turn.Dock = DockStyle.Top;
            turn.Font = new Font("Pretendard", 12F);
            turn.Location = new Point(20, 20);
            turn.Name = "turn";
            turn.Size = new Size(66, 24);
            turn.TabIndex = 1;
            turn.Text = "내 차례";
            // 
            // initial
            // 
            initial.BorderStyle = BorderStyle.FixedSingle;
            initial.Dock = DockStyle.Top;
            initial.Font = new Font("Pretendard", 14F);
            initial.Location = new Point(20, 78);
            initial.Margin = new Padding(0);
            initial.Name = "initial";
            initial.Size = new Size(760, 103);
            initial.TabIndex = 0;
            initial.Text = "게임을 준비하고 있습니다...";
            initial.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // input
            // 
            input.Dock = DockStyle.Bottom;
            input.Font = new Font("Pretendard", 13F);
            input.Location = new Point(20, 357);
            input.Name = "input";
            input.PlaceholderText = "단어를 입력해 주세요";
            input.ShortcutsEnabled = false;
            input.Size = new Size(760, 33);
            input.TabIndex = 1;
            // 
            // space
            // 
            space.Dock = DockStyle.Top;
            space.Location = new Point(20, 44);
            space.Name = "space";
            space.Size = new Size(760, 34);
            space.TabIndex = 2;
            // 
            // usedWordList
            // 
            usedWordList.AutoScroll = true;
            usedWordList.Dock = DockStyle.Top;
            usedWordList.Location = new Point(20, 181);
            usedWordList.Name = "usedWordList";
            usedWordList.Size = new Size(760, 75);
            usedWordList.TabIndex = 3;
            usedWordList.WrapContents = false;
            // 
            // GameForm
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(usedWordList);
            Controls.Add(initial);
            Controls.Add(input);
            Controls.Add(space);
            Controls.Add(turn);
            Name = "GameForm";
            Padding = new Padding(20, 20, 20, 60);
            Text = "그두";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        protected Label turn;
        protected Label initial;
        protected TextBox input;
        protected Panel space;
        protected FlowLayoutPanel usedWordList;
    }
}