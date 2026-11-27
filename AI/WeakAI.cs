using Manager.Manager.Database;

namespace AI
{
    public class WeakAI : CodeAI
    {
        public override async Task<string> SelectWord(HashSet<string> usedWord, char initial, int shieldTurn)
        {
            string selected = await AIManager.FindRandomWord(usedWord, initial, shieldTurn);

            return selected;
        }
    }
}
