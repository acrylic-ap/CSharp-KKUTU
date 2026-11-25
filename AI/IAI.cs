using System;
using System.Collections.Generic;
using System.Text;

namespace AI
{
    public interface IAI
    {
        Task<string> SelectWord(HashSet<string> usedWord, char initial, int shieldTurn);
    }
}
