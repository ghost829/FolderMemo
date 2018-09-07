using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Text.RegularExpressions;

namespace PaintTextBoxClassLib
{
    class RegexData
    {
        private static RegexData m_instance;
        public static RegexData Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new RegexData();

                }
                return m_instance;
            }
        }

        private RegexData()
        {

        }
        
        /// <summary>
        /// 키워드를 전부 검색하는 Regex 조건식을 생성 후 Return합니다.
        /// </summary>
        /// <param name="keywordCollection"></param>
        /// <returns></returns>
        public string buildKeyword(List<string> keywordCollection)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < keywordCollection.Count; i++)
            {
                if(i == keywordCollection.Count-1)
                    sb.Append("\\b"+keywordCollection[i]+"\\b");
                else
                    sb.Append("\\b"+keywordCollection[i]+"\\b|");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 문자열에 입력한 Index를 기준으로 단어의 시작index와 끝index를 추출한다.
        /// </summary>
        /// <param name="lineText"></param>
        /// <param name="caretIndex"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public int[] getWordStartEndIndex(string lineText, int caretIndex, string pattern)
        {
            int wordStart = 0, wordEnd = lineText.Length;
            string prevText = lineText.Substring(0, caretIndex);
            string nextText = lineText.Substring(caretIndex);
            Regex regex = new Regex(pattern);
            Match regMatch;

            for (regMatch = regex.Match(prevText); regMatch.Success; regMatch = regMatch.NextMatch())
            {
                wordStart = regMatch.Index + regMatch.Length;
            }

            for (regMatch = regex.Match(nextText); regMatch.Success; )
            {
                wordEnd = prevText.Length + regMatch.Index;
                break;
            }
            //regex.RightToLeft = true;
            

            return new int[] { wordStart, wordEnd };
        }

        public int[] getWordStartEndIndex2(string lineText, int caretIndex, string pattern)
        {
            int wordStart = 0, wordEnd = lineText.Length;
            string prevText = lineText.Substring(0, caretIndex);
            string nextText = lineText.Substring(caretIndex);
            Regex regex = new Regex(pattern);
            Match regMatch;

            for (regMatch = regex.Match(prevText); regMatch.Success; regMatch = regMatch.NextMatch())
            {
                wordStart = regMatch.Index + regMatch.Length;
                Console.WriteLine("1. "+wordStart);
            }

            for (regMatch = regex.Match(nextText); regMatch.Success; )
            {
                wordEnd = prevText.Length + regMatch.Index;
                Console.WriteLine("2. "+wordEnd);
                break;
            }
            //regex.RightToLeft = true;


            return new int[] { wordStart, wordEnd };
        }


        /// <summary>
        /// 입력한 문자열에서 특수문자 발견 시 이스케이프문자열을 추가한 후 return한다.
        /// //특수문자 처리 - Regex사용 때문
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public string ConvertRegexWordToPlainText(string plainText)
        {
            string regexWord = string.Empty;

            StringBuilder tmpSB = new StringBuilder();
            for (int tmpI = 0; tmpI < plainText.Length; tmpI++)
            {
                Char tmpChar = plainText[tmpI];

                tmpSB.Append(ConvertRegexWordToPlainText(tmpChar));
            }
            regexWord = tmpSB.ToString();
            return regexWord;
        }

        /// <summary>
        /// 특수문자는 이스케이프문자열을 추가한 후 return한다.
        /// </summary>
        /// <param name="tmpChar"></param>
        /// <returns></returns>
        private string ConvertRegexWordToPlainText(char tmpChar)
        {
            bool isRegexChar = false;
            if (!Char.IsLetterOrDigit(tmpChar))
            {
                switch (tmpChar)
                {
                    case '_':
                    case '-':
                        //isRegexChar = false;
                        break;
                    default:
                        isRegexChar = true;
                        break;
                }
            }
            if (isRegexChar)
                return "\\" + tmpChar;
            else
                return tmpChar + "";
        }
    }
}
