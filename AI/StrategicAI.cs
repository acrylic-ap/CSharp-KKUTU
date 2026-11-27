using Manager.Manager.Database;

namespace AI
{
    public class StrategicAI : CodeAI
    {
        public int? LastScore { get; private set; }
        public int LastDepth { get; private set; }

        public override async Task<string> SelectWord(HashSet<string> usedWord, char initial, int shieldTurn)
        {
            LastScore = null;
            LastDepth = 0;
            var result = await AIManager.FindStrategicWord(usedWord, initial, shieldTurn);
            LastScore = result.Score;
            LastDepth = result.Depth;
            return result.Word;
        }
    }
}