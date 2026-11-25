using Utils;

namespace Manager
{
    public abstract class GameManager
    {
        protected HashSet<string> usedWord = [];

        public char StartChar { get; protected set; }

        public virtual void Initialize()
        {
            usedWord.Clear();
            StartChar = DatabaseManager.GetStartChar();
        }

        private bool IsValidInitial(string word)
        {
            if (!HangulUtils.IsHangulWord(word))
                return false;

            char initial = word[0];

            return initial == StartChar ||
                   initial == HangulUtils.GetDoumChar(StartChar);
        }

        public async Task<string?> CheckWord(string word)
        {
            if (!IsValidInitial(word))
                return "무효";

            if (usedWord.Contains(word))
                return "이미 쓰인 단어입니다.";

            if (await DatabaseManager.IsExistWord(word) == false)
                return "무효";

            usedWord.Add(word);
            StartChar = word[^1];

            return null;
        }

        public async Task<bool> IsExistInitial()
        {
            return await DatabaseManager.IsExistInitial(
                usedWord, StartChar
            );
        }

        public abstract void ChangeTurn();
    }
}