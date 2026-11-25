namespace Utils
{
    public static class HangulUtils
    {
        public static char GetDoumChar(char syllable)
        {
            if (syllable < '가' || syllable > '힣')
                return syllable;

            int code = syllable - '가';
            int onset = code / 588;
            int nucleus = code / 28 % 21;

            if (onset == 5)
            {
                if (nucleus is 0 or 1 or 8 or 11 or 13 or 18)
                    return (char)(syllable - 1764);

                if (nucleus is 2 or 6 or 7 or 12 or 17 or 20)
                    return (char)(syllable + 3528);
            }
            else if (onset == 2 && nucleus is 6 or 12 or 17 or 20)
            {
                return (char)(syllable + 5292);
            }

            return syllable;
        }

        public static bool IsHangulWord(string word)
        {
            return !string.IsNullOrEmpty(word)
                && word.All(c => c >= '가' && c <= '힣');
        }
    }
}