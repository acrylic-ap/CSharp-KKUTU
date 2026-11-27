using Manager.Manager.Database;

namespace AI
{
    public class Preferred : CodeAI
    {
        public int? LastScore { get; private set; }
        public int LastDepth { get; private set; }

        public override async Task<string> SelectWord(HashSet<string> usedWord, char initial, int shieldTurn)
        {
            string selected = await AIManager.FindPreferredWord(usedWord, initial, shieldTurn);

            return selected;
        }
    }
}