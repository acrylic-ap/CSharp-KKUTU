using Manager;
using Server.Models;

namespace Server.Manager
{
    internal class MultiPlayManager : GameManager
    {
        private readonly ClientConnection[] players;

        public int CurrentTurn { get; private set; } = 0;

        public MultiPlayManager(ClientConnection[] players)
        {
            this.players = players;
        }

        public override void Initialize()
        {
            base.Initialize();

            Random.Shared.Shuffle(players);

            CurrentTurn = 0;
        }

        public override void ChangeTurn()
        {
            base.ChangeTurn();

            CurrentTurn = (CurrentTurn + 1) % players.Length;
        }
    }
}