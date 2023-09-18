using System;
using System.Linq;

namespace Junk.Core.Creation
{
    public static class StringHelper
    {
        /*[Test]
        [TestCase("123 123 1adc \n 222", "1231231adc222")]
        public void RemoveWhiteSpace1(string input, string expected)
        {
            string s = null;
            for (int i = 0; i < 1000000; i++)
            {
                s = input.RemoveWhitespace();
            }
            Assert.AreEqual(expected, s);
        }

        [Test]
        [TestCase("123 123 1adc \n 222", "1231231adc222")]
        public void RemoveWhiteSpace2(string input, string expected)
        {
            string s = null;
            for (int i = 0; i < 1000000; i++)
            {
                s = Regex.Replace(input, @"\s+", "");
            }
            Assert.AreEqual(expected, s);
        }*/

        public static string RemoveWhitespace(this string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }

        public static string RemoveTabs(this string input)
        {
            string line = input.Replace("\t", "");
            return line;
        }
        
        public static string[] RemoveAllWhitespace(this string[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                var currentString = input[i];
                currentString = currentString.RemoveWhitespace();
                input[i] = currentString;
            }

            return input;
        }

        public static string[] RemoveAllTabs(this string[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                var currentString = input[i];
                currentString = currentString.RemoveTabs();
                input[i]      = currentString;
            }

            return input;
        }
        public static string GetUntilOrEmpty(this string text, string stopAt = ",")
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return String.Empty;
        }
    }
}