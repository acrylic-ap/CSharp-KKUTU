using UI.Utils;

namespace UI.Forms
{
    public partial class GameForm : Form
    {
        public GameForm()
        {
            InitializeComponent();
        }

        public void GameOver()
        {
            MessageBox.Show("게임 오버!", "그두");

            DialogResult = DialogResult.OK;
        }

        protected void ShowInitial(char startChar)
        {
            initial.Text = LabelUtils.ShowInitial(startChar);
        }

        protected void ShowWord(string word, char startChar)
        {
            LabelUtils.AddUsedWord(usedWordList, word);
            ShowInitial(startChar);
        }

        protected void ShowError()
        {
            MessageBox.Show(
                "처리 중 오류가 발생했습니다.",
                "그두",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            DialogResult = DialogResult.OK;
        }

        protected void DisableInput()
        {
            input.Enabled = false;
        }

        protected void EnableInput()
        {
            if (!IsDisposed && DialogResult == DialogResult.None)
            {
                input.Enabled = true;
                input.Focus();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (DialogResult == DialogResult.OK)
                return;

            DialogResult result = MessageBox.Show(
                "게임에서 나가시겠습니까?",
                "그두",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.No)
                e.Cancel = true;
        }

        private void GameForm_Load(object sender, EventArgs e)
        {

        }

        private void shield_Click(object sender, EventArgs e)
        {

        }
    }
}