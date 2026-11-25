using System;
using System.Collections.Generic;
using System.Text;

namespace AI
{
    public abstract class CodeAI : IAI
    {
        public abstract Task<string> SelectWord(HashSet<string> usedWord, char initial, int shieldTurn);
    }
}
