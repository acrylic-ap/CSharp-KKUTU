using BotPlay.Manager;
using UI.Forms;

namespace Manager
{
    public partial class BotGame : GameForm
    {
        private readonly BotPlayManager gameManager = new();

        public BotGame()
        {
            Text = "봇 게임";

            input.KeyDown += Input_KeyDown;
            Shown += BotGame_Shown;
        }

        public void Initialize()
        {
            gameManager.Initialize();
            ShowInitial(gameManager.StartChar);
        }

        public async Task<bool> IsGameOver()
        {
            return !await gameManager.IsExistInitial();
        }

        public void ShowWord(string word)
        {
            base.ShowWord(word, gameManager.StartChar);
        }

        public async Task<bool> CompleteTurn(string word)
        {
            ShowWord(word);
            gameManager.ChangeTurn();

            if (await IsGameOver())
            {
                GameOver();
                return true;
            }

            return false;
        }

        private async Task<bool> PlayerTurn(string word)
        {
            string? err_message = await gameManager.CheckWord(word);

            if (err_message is not null)
            {
                MessageBox.Show(err_message, "그두");
                return false;
            }

            return !await CompleteTurn(word);
        }

        private async void Input_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || !input.Enabled)
                return;

            e.SuppressKeyPress = true;
            DisableInput();

            try
            {
                string word = input.Text;
                input.Clear();

                if (!await PlayerTurn(word))
                    return;

                await AITurn();
            }
            catch
            {
                ShowError();
            }
            finally
            {
                EnableInput();
            }
        }

        private void BotGame_Shown(object? sender, EventArgs e)
        {
            Initialize();
        }

        // -- AI -------------------------------------------

        private void AIResign()
        {
            MessageBox.Show("AI가 게임을 포기하였습니다.", "그두");

            DialogResult = DialogResult.OK;
        }

        private async Task AITurn()
        {
            string selectedWord = await gameManager.PlayAI();

            if (selectedWord == "")
            {
                AIResign();
                return;
            }

            await CompleteTurn(selectedWord);
        }
    }
}