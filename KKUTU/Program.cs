namespace KKUTU
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using Mutex mutex = new(true, "KKUTU", out bool isNew);

            if (!isNew)
            {
                MessageBox.Show(
                    "이미 게임이 실행 중입니다.",
                    "그두",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new Main());
        }
    }
}