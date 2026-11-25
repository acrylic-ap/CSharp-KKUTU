using MultiPlay.Manager;
using UI.Forms;

namespace MultiPlay
{
    public partial class MultiGame : GameForm
    {
        private readonly ClientManager clientManager;
        private readonly int sessionId;

        private char startChar;
        private int playerIndex;
        private int currentTurn;

        public MultiGame(ClientManager clientManager, int sessionId)
        {
            Text = "멀티플레이";

            this.clientManager = clientManager;
            this.sessionId = sessionId;

            input.KeyDown += Input_KeyDown;
            Shown += MultiGame_Shown;

            clientManager.GameStart += StartGame;
            clientManager.WordError += ShowWordError;
            clientManager.WordAccepted += ReceiveWord;
            clientManager.GameOver += FinishGame;
        }

        public void Initialize()
        {
            DisableInput();

            turn.Text = "게임 시작 대기 중...";
            initial.Text = "";
        }

        private void StartGame(char startChar, int playerIndex)
        {
            BeginInvoke(() =>
            {
                this.startChar = startChar;
                this.playerIndex = playerIndex;

                currentTurn = 0;

                ShowInitial(startChar);
                UpdateTurn();
            });
        }

        private void UpdateTurn()
        {
            if (currentTurn == playerIndex)
            {
                turn.Text = "내 차례";
                EnableInput();
                return;
            }

            turn.Text = "상대 차례";
            DisableInput();
        }

        public void ShowWord(string word)
        {
            base.ShowWord(word, startChar);
        }

        private async Task PlayerTurn(string word)
        {
            await clientManager.Send($"[Word]\n{sessionId}\n{word}");
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

                await PlayerTurn(word);
            }
            catch
            {
                ShowError();
            }
        }

        private async void MultiGame_Shown(object? sender, EventArgs e)
        {
            Initialize();

            try
            {
                await clientManager.Send($"[Ready]\n{sessionId}");
            }
            catch
            {
                ShowError();
            }
        }

        // -- Server Response ---------------------------------

        private void ShowWordError(string message)
        {
            BeginInvoke(() =>
            {
                MessageBox.Show(message, "그두");
                UpdateTurn();
            });
        }

        private void ReceiveWord(
            string word, char nextChar, int nextTurn, bool isFinished)
        {
            BeginInvoke(() =>
            {
                startChar = nextChar;
                currentTurn = nextTurn;

                ShowWord(word);

                if (isFinished)
                {
                    DisableInput();
                    return;
                }

                UpdateTurn();
            });
        }

        private void FinishGame(int loserIndex)
        {
            BeginInvoke(() =>
            {
                DisableInput();

                MessageBox.Show(
                    loserIndex == playerIndex
                        ? "패배하였습니다."
                        : "승리하였습니다.",
                    "그두"
                );

                DialogResult = DialogResult.OK;
            });
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            clientManager.GameStart -= StartGame;
            clientManager.WordError -= ShowWordError;
            clientManager.WordAccepted -= ReceiveWord;
            clientManager.GameOver -= FinishGame;

            base.OnFormClosed(e);
        }
    }
}