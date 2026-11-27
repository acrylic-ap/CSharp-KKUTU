using Manager;
using Manager.Manager.Database;
using MultiPlay;
using MultiPlay.Manager;
using System.Diagnostics;

namespace KKUTU
{
    public partial class Main : Form
    {
        private readonly ClientManager clientManager = new();
        private readonly Stopwatch matchingWatch = new();
        private CancellationTokenSource? matchingCts;

        private void EnterGame(int sessionId)
        {
            BeginInvoke(() =>
            {
                if (matchingCts is null || matchingCts.IsCancellationRequested)
                    return;

                matchingCancelButton.Visible = false;
                StopTimer();

                using MultiGame multiGame = new(clientManager, sessionId);

                Hide();
                multiGame.ShowDialog();
                Show();

                matchingCts?.Cancel();
            });
        }

        public Main()
        {
            InitializeComponent();

            clientManager.MatchSuccess += EnterGame;

            loading.Dock = DockStyle.None;
            loading.AutoSize = false;
            loading.Bounds = ClientRectangle;
            loading.BringToFront();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {

        }
        private async void Form1_Shown(object sender, EventArgs e)
        {
            bool isSucceed = true;

            do
            {
                isSucceed = true;

                try
                {
                    await InitialManager.StoreStartChars();

                    loading.Visible = false;
                }
                catch
                {
                    isSucceed = false;

                    DialogResult select = MessageBox.Show("접속에 실패하였습니다. 다시 시도하겠습니까?", "그두", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                    if (select == DialogResult.Cancel)
                    {
                        Application.Exit();
                        break;
                    }
                }
            } while (!isSucceed);
        }

        private void label1_Click(object sender, EventArgs e)
        {
            label1.Width = this.Width;
        }

        private void StartTimer()
        {
            matchingTime.Visible = true;
            matchingWatch.Restart();
            matchingTimer.Start();
        }

        private void StopTimer()
        {
            matchingTimer.Stop();
            matchingWatch.Stop();
            matchingTime.Visible = false;
        }

        private void EnableMatching()
        {
            multiPlayButton.Text = "매칭 중...";

            aiPlayButton.Enabled = false;
            multiPlayButton.Enabled = false;

            matchingCancelButton.Visible = true;

            StartTimer();
        }

        private void DisableMatching()
        {
            multiPlayButton.Text = "멀티 플레이";

            aiPlayButton.Enabled = true;
            multiPlayButton.Enabled = true;

            matchingCancelButton.Visible = false;

            StopTimer();
        }

        private async void multiPlayButton_Click(object sender, EventArgs e)
        {
            matchingCts = new();

            try
            {
                EnableMatching();

                await clientManager.Matching(matchingCts.Token);
            }
            catch (OperationCanceledException)
                    when (matchingCts.IsCancellationRequested)
            {

            }
            catch
            {
                MessageBox.Show("서버 연결에 실패했습니다. 인터넷 연결을 확인해 주세요.\n" +
                    "연결에 이상이 없다면 서버에 문제가 있을 수 있습니다.", "그두", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                matchingCts.Dispose();
                matchingCts = null;

                DisableMatching();
            }
        }

        private void aiPlayButton_Click(object sender, EventArgs e)
        {
            using BotGame botGame = new();
            Hide();
            try { botGame.ShowDialog(); }
            finally { Show(); }
        }

        private void matchingTimer_Tick(object? sender, EventArgs e)
        {
            matchingTime.Text =
                $"매칭 대기 시간\n" +
                $"[{matchingWatch.Elapsed:mm\\:ss}]";
        }

        private void loading_Click(object sender, EventArgs e)
        {
            this.loading.Width = this.Width;
            this.loading.Height = this.Height;
        }

        private void matchingCancelButton_Click(object sender, EventArgs e)
        {
            matchingCts?.Cancel();
        }
    }
}
