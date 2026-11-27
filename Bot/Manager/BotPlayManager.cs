using Manager;
using AI;

namespace BotPlay.Manager
{
    public enum Turn
    {
        Player, AI
    }

    public class BotPlayManager : GameManager
    {
        public IAI usingAI = new StrategicAI();

        public Turn CurrentTurn { get; private set; } = Turn.Player;

        public override void Initialize()
        {
            base.Initialize();

            int selectRandom = Random.Shared.Next(2);

            CurrentTurn = selectRandom == 1 ? Turn.AI : Turn.Player;
        }

        public override void ChangeTurn()
        {
            base.ChangeTurn();

            CurrentTurn = CurrentTurn == Turn.Player
                ? Turn.AI
                : Turn.Player;
        }

        public async Task<string> PlayAI()
        {
            string selectedWord = await usingAI.SelectWord(
                usedWord, StartChar, ShieldTurn
            );

            if (await CheckWord(selectedWord) is null)
                return selectedWord;

            return "";
        }
    }
}