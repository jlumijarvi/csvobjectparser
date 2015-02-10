using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvToObjectLib
{
    public static class StringExtensions
    {
        public static string[] CsvSplit(this string self, char separator)
        {
            var ret = new List<string>();

            var word = new StringBuilder();
            var escapeChar = false;

            foreach (var ch in self)
            {
                if (ch == '\"')
                {
                    escapeChar = !escapeChar;
                }
                else if (ch == separator && !escapeChar)
                {
                    ret.Add(word.ToString());
                    word.Clear();
                }
                else
                {
                    word.Append(ch);
                }
            }

            if (word.Length > 0) ret.Add(word.ToString());

            return ret.ToArray();
        }
    }
}
