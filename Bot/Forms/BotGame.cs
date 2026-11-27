using BotPlay.Manager;
using AI;
using UI.Forms;

namespace Manager
{
    public partial class BotGame : GameForm
    {
        private readonly BotPlayManager gameManager = new();
        private bool gameFinished;
        private readonly List<(Turn Player, string Word)> gameRecord = new();
        private char startingSyllable;
        private Turn startingPlayer;
        private readonly Label aiAssessment = new() { Dock = DockStyle.Right, AutoSize = true, TextAlign = ContentAlignment.MiddleRight, Text = "AI 판단: 대기 중" };

        public BotGame()
        {
            Text = "봇 게임";

            space.Controls.Add(aiAssessment);
            input.KeyDown += Input_KeyDown;
            Shown += BotGame_Shown;
        }

        public void Initialize()
        {
            gameFinished = false;
            gameManager.Initialize();
            gameRecord.Clear();
            startingSyllable = gameManager.StartChar;
            startingPlayer = gameManager.CurrentTurn;
            aiAssessment.Text = "AI 판단: 대기 중";
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
            gameRecord.Add((gameManager.CurrentTurn, word));
            ShowWord(word);
            gameManager.ChangeTurn();

            shield.Text = gameManager.ShieldTurn.ToString();

            if (await IsGameOver())
            {
                FinishGame(gameManager.CurrentTurn == Turn.AI ? Turn.Player : Turn.AI, false);
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

        private async void BotGame_Shown(object? sender, EventArgs e)
        {
            Initialize();

            if (gameManager.CurrentTurn != Turn.AI)
                return;

            DisableInput();
            try
            {
                await AITurn();
            }
            finally
            {
                EnableInput();
            }
        }

        // -- AI -------------------------------------------

        private string PlayerName(Turn player) => player == Turn.Player ? "플레이어" : "AI";

        private void FinishGame(Turn winner, bool resigned)
        {
            if (gameFinished || IsDisposed) return;
            gameFinished = true;
            string winnerText = PlayerName(winner);
            string reason = resigned ? $"{PlayerName(gameManager.CurrentTurn)}가 단어를 선택하지 못했습니다." : "이어갈 단어가 없습니다.";
            string turnText = PlayerName(startingPlayer);
            string moves = string.Join(Environment.NewLine, gameRecord.Select((move, index) =>
                $"{index + 1}. {PlayerName(move.Player)}: {move.Word}"));
            string result = $"승자: {winnerText}{Environment.NewLine}종료 사유: {reason}{Environment.NewLine}" +
                $"시작 음절: {startingSyllable}{Environment.NewLine}선공: {turnText}{Environment.NewLine}" +
                $"남은 보호막: {gameManager.ShieldTurn}{Environment.NewLine}{Environment.NewLine}대국 기록{Environment.NewLine}" +
                (gameRecord.Count == 0 ? "(없음)" : moves);

            while (MessageBox.Show($"{winnerText} 승리!\n{reason}\n\n결과를 복사할까요?", "게임 종료", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                try
                {
                    Clipboard.SetText(result);
                    break;
                }
                catch (System.Runtime.InteropServices.ExternalException)
                {
                    if (MessageBox.Show("클립보드에 복사하지 못했습니다. 다시 시도할까요?", "복사 실패", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        break;
                }
            }

            DialogResult = DialogResult.OK;
        }

        private async Task<bool> AITurn()
        {
            try
            {
                aiAssessment.Text = "AI 판단: 생각 중";
                IAI currentAI = gameManager.usingAI;
                string selectedWord = await gameManager.PlayAI();
                if (IsDisposed || DialogResult != DialogResult.None) return false;

                if (selectedWord == "")
                {
                    FinishGame(gameManager.CurrentTurn == Turn.Player ? Turn.AI : Turn.Player, true);
                    return false;
                }

                if (currentAI is StrategicAI strategic && strategic.LastScore is int score)
                {
                    string judgment = score >= 999_872 ? "필승 경로 확인" : score <= -999_872 ? "필패 경로 확인" : score > 0 ? "유리 추정" : score < 0 ? "불리 추정" : "판단 보류";
                    aiAssessment.Text = $"AI 판단: {judgment} (점수 {score:+#;-#;0}, 깊이 {strategic.LastDepth})";
                }
                else
                    aiAssessment.Text = "AI 판단: 확인 불가";

                return !await CompleteTurn(selectedWord);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                if (!IsDisposed) ShowError();
                return false;
            }
        }

        private void initial_Click(object sender, EventArgs e)
        {

        }

        private void BotGame_Load(object sender, EventArgs e)
        {

        }
    }
}