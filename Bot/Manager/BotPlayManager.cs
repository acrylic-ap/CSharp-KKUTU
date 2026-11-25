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
        public IAI usingAI = new WeakAI();

        public Turn CurrentTurn { get; private set; } = Turn.Player;

        public override void Initialize()
        {
            base.Initialize();

            CurrentTurn = Turn.Player;
        }

        public override void ChangeTurn()
        {
            CurrentTurn = CurrentTurn == Turn.Player
                ? Turn.AI
                : Turn.Player;
        }

        public async Task<string> PlayAI()
        {
            string selectedWord = await usingAI.SelectWord(
                usedWord, StartChar
            );

            if (await CheckWord(selectedWord) is null)
                return selectedWord;

            return "";
        }
    }
}