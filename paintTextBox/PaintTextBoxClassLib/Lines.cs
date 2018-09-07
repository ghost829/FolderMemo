using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Text.RegularExpressions;

namespace PaintTextBoxClassLib
{
    public class Lines
    {
        public enum TextType
        {
            PlainText,
            Comment,
            StringText,
            keyword1,
            keyword2,
            keyword3,
        }

        /// <summary>
        /// Regex및 Brush 복사
        /// </summary>
        /// <param name="source">Source Line</param>
        /// <param name="target">Target Line</param>
        public static void LinesCopyInfo(Lines source, Lines target)
        {
            target.reg1 = source.reg1;
            target.reg2 = source.reg2;
            target.reg3 = source.reg3;
            target.commentStr = source.commentStr;
        }

        string m_text = string.Empty;
        //private Brush[] m_textcolorBrush; // regular Express로 인해 변경하는 색
        public TextType[] m_textType; // 텍스트 타입
        
        public Regex reg1;
        public Regex reg2;
        public Regex reg3;

        public string commentStr = "//";

        /// <summary>
        /// 라인 생성
        /// </summary>
        public Lines()
        {
            
        }

        /// <summary>
        /// 라인의 텍스트 - 하이라이트 자동 측정
        /// </summary>
        public string Text
        {
            get
            {
                return this.m_text;
            }
            set
            {
                this.m_text = value;
                SyntaxHighLightUpdate();
            }
        }

        /// <summary>
        /// 특정 Index에 텍스트를 추가한다.
        /// </summary>
        /// <param name="AppendTxT">추가할 텍스트</param>
        /// <param name="index">추가할 위치</param>
        public void textInsertAt(string InsertTxT, int index)
        {
            int appendIndex = Math.Min(this.m_text.Length, index); //에러 방지를 위한 Index지정, 인덱스가 텍스트 길이보다 클 경우 텍스트 끝에 붙임
            string prevtText = this.m_text.Substring(0, appendIndex);
            string nextText = this.m_text.Substring(appendIndex);
            this.Text = prevtText + InsertTxT + nextText;
        }

        /// <summary>
        /// 특정 Index부터 특정길이만큼 텍스트를 제거한다.
        /// </summary>
        /// <param name="removeStartIndex">제거를 시작할 인덱스</param>
        /// <param name="removeLength">제거할 길이</param>
        public void textRemoveAt(int removeStartIndex, int removeLength)
        {
            string prevText = this.m_text.Substring(0, removeStartIndex);
            string nextText = this.m_text.Substring(removeStartIndex + removeLength);
            this.Text = prevText + nextText;
        }

        /// <summary>
        /// 텍스트 변경시 Regex 자동 재설정
        /// </summary>
        public void SyntaxHighLightUpdate()
        {
            
            int textlength = this.m_text.Length;
            m_textType = new TextType[textlength];

            for (int i = 0; i < textlength; i++)
            {
                m_textType[i] = TextType.PlainText;
            }

            //processRegex(reg1, (SolidBrush)brush1);
            //processRegex(reg2, (SolidBrush)brush2);
            //processRegex(reg3, (SolidBrush)brush3);
            if(reg1 != null)
            processRegex(reg1, TextType.keyword1);
            if (reg2 != null)
            processRegex(reg2, TextType.keyword2);
            if (reg3 != null)
            processRegex(reg3, TextType.keyword3);

            //processRegexForComment(this.commentReg, (SolidBrush)this.commentBrush);
            processStringTextSyntax();
            processSyntaxForComment();
        }

        private void processRegex(Regex regex, TextType textType)
        {
            Match regMatch;
            try
            {
                for (regMatch = regex.Match(m_text); regMatch.Success; regMatch = regMatch.NextMatch())
                {
                    for (int i = regMatch.Index; i < regMatch.Index+regMatch.Length; i++)
                    {
                        m_textType[i] = textType;
                    }
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 주석 처리 regex
        /// </summary>
        /// <param name="commentBrush"></param>
        private void processSyntaxForComment()
        {
            if (this.m_text.IndexOf("//") > -1)
            {
                Regex regex_comment = new Regex("//");
                MatchCollection matchs_comment;
                matchs_comment = regex_comment.Matches(m_text);
                Match match_comment;

                for (int i = 0; i < matchs_comment.Count; i++)
                {
                    match_comment = matchs_comment[i];
                    if (this.m_textType[match_comment.Index].Equals(TextType.PlainText)) //해당 인덱스에 있는 문자가 보통문자열일때 (String문자열[" "]에 속해있지 않을때)
                    {
                        for (int tmp_i = match_comment.Index; tmp_i < m_text.Length; tmp_i++)
                        {
                            m_textType[tmp_i] = TextType.Comment;
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// " " (double quotes) 처리 regex
        /// </summary>
        private void processStringTextSyntax()
        {
            Regex regex_stringText = new Regex("\"");
            MatchCollection matchs_stringText;
            matchs_stringText = regex_stringText.Matches(m_text);

            for (int i = 0; i < matchs_stringText.Count; i++)
            {
                Match match = matchs_stringText[i];
                if (i != matchs_stringText.Count - 1)
                {
                    for (int changeColorIndex = match.Index; changeColorIndex < matchs_stringText[i + 1].Index + 1; changeColorIndex++)
                    {
                        m_textType[changeColorIndex] = TextType.StringText;
                    }
                    i++;
                }
                else
                {
                    if (matchs_stringText.Count % 2 == 1)
                        for (int changeColorIndex = match.Index; changeColorIndex < m_text.Length; changeColorIndex++)
                        {
                            m_textType[changeColorIndex] = TextType.StringText;
                        }
                }
            }
        }

    }
}
