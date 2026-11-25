

using Manager;

namespace AI
{
    public class WeakAI : CodeAI
    {
        public override async Task<string> SelectWord(HashSet<string> usedWord, char initial, int shieldTurn)
        {
            string selected = await DatabaseManager.FindAvailableRandomWord(usedWord, initial, shieldTurn);

            return selected;
        }
    }
}
