using MultiPlay.Manager;
using UI.Forms;

namespace MultiPlay
{
    public partial class MultiGame : GameForm, IGame
    {
        private readonly MultiPlayManager gameManager = new();

        public MultiGame()
        {
            InitializeComponent();
        }
        public void Initialize()
        {
            gameManager.Initialize();
            ShowInitial(gameManager.StartChar);
        }

        public Task<bool> CompleteTurn(string word)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsGameOver()
        {
            return !await gameManager.IsExistInitial();
        }

        public void ShowWord(string word)
        {
            base.ShowWord(word, gameManager.StartChar);
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

        private void MultiGame_Shown(object? sender, EventArgs e)
        {
            Initialize();
        }
    }
}
