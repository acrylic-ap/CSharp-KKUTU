using Manager;

namespace MultiPlay.Manager
{
    public class MultiPlayManager : GameManager
    {
        private List<object> players = [];

        public int CurrentTurn { get; private set; } = 0;

        public override void Initialize()
        {
            base.Initialize();

            CurrentTurn = 0;
        }

        public override void ChangeTurn()
        {
            CurrentTurn = (CurrentTurn + 1) % players.Count;
        }
    }
}
