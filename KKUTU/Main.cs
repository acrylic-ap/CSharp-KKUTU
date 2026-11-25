using Manager;
using MultiPlay.Manager;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;

namespace KKUTU
{
    public partial class Main : Form
    {
        private readonly ClientManager clientManager = new();

        public Main()
        {
            InitializeComponent();
        }

        private void disableButton()
        {
            aiPlayButton.Enabled = false;
            multiPlayButton.Enabled = false;
        }

        private void enableButton()
        {
            aiPlayButton.Enabled = true;
            multiPlayButton.Enabled = true;
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
                    await DatabaseManager.StoreStartChars();

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

        private async void multiPlayButton_Click(object sender, EventArgs e)
        {
            try
            {
                disableButton();

                await clientManager.Matching();
            }
            catch
            {
                MessageBox.Show("서버 연결에 실패했습니다. 인터넷 연결을 확인해 주세요.\n" +
                    "연결에 이상이 없다면 서버에 문제가 있을 수 있습니다.", "그두", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                enableButton();
            }
        }

        private void aiPlayButton_Click(object sender, EventArgs e)
        {
            BotGame botGame = new BotGame();

            this.Hide();
            botGame.ShowDialog();
            this.Show();
        }

        private void loading_Click(object sender, EventArgs e)
        {
            this.loading.Width = this.Width;
            this.loading.Height = this.Height;
        }
    }
}
