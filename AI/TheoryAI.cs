using Manager.Manager.Database;

namespace AI
{
    public class TheoryAI : CodeAI
    {
        public override async Task<string> SelectWord(HashSet<string> usedWord, char initial, int shieldTurn)
        {
            string selected = await AIManager.FindTheoryWord(usedWord, initial, shieldTurn);

            return selected;
        }
    }
}
