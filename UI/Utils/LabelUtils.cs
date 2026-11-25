using Utils;
using System.Windows.Forms;

namespace UI.Utils
{
    public static class LabelUtils
    {
        public static string ShowInitial(char initial)
        {
            char doumInitial = HangulUtils.GetDoumChar(initial);

            if (doumInitial == initial)
                return initial.ToString();
            else
                return $"{initial}({doumInitial})";
        }

        public static void AddUsedWord(FlowLayoutPanel usedWordList, string word)
        {
            Label label = new()
            {
                Text = word,
                Font = new Font("Pretendard", 14),
                AutoSize = true,
                Margin = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle
            };

            usedWordList.Controls.Add(label);
            usedWordList.Controls.SetChildIndex(label, 0);
        }
    }
}
