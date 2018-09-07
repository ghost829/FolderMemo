using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaintTextBoxClassLib
{
    public partial class CustomListBox : UserControl
    {
        private const int WM_MOUSEWHEEL = 0x020A;
                                                                                   
        private int m_firstViewLine = 0;
        int lastViewLine = 0;
        int m_selectedLine = -1;

        /// <summary>
        /// 한번에 볼수 있는 라인갯수 지정
        /// </summary>
        private int m_viewLIneCount = 6;
        
        /// <summary>
        /// 리스트박스 넓이
        /// </summary>
        int originalWidth = 250;

        /// <summary>
        /// 선택된 라인 지정
        /// </summary>
        public int selectedLine
        {
            get
            {
                return m_selectedLine;
            }
            set
            {
                if (!(firstViewLine <= value && lastViewLine >= value))
                {
                    if (value < firstViewLine)
                    {
                        firstViewLine = value;
                    }
                    if (value > lastViewLine)
                    {
                        firstViewLine = value-viewLineCount;
                    }
                }
                m_selectedLine = value;
                viewDescription();
            }
        }

        /// <summary>
        /// 현재 보이는 첫번째 라인
        /// </summary>
        public int firstViewLine
        {
            get
            {
                return m_firstViewLine;
            }
            set
            {
                m_firstViewLine = value;
            }
        }
        
        public List<string[]> lineInfo = new List<string[]>();

        public delegate void returnEvent(string str);
        public delegate void returnEvent2(string str1, string str2);

        /// <summary>
        /// 리스트 박스에서 특정 라인 선택시 해당단어를 PaintTextBox로 전달
        /// </summary>
        public event returnEvent selectWord;

        /// <summary>
        /// 현재 인텔리센스 박스에 Focus를 갖고있지 않을 경우 Parent에 Description전달
        /// </summary>
        public event returnEvent2 req_viewDescription;

        public CustomListBox()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            InitializeComponent();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            this.Dispose();
            base.OnLostFocus(e);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_MOUSEWHEEL: //마우스 휠 이벤트 발생시 - firstViewLine 값 조정하고 invalidate
                    #region WM_MOUSEWHEEL
                    if (m.WParam.ToInt32() < 0) // WheelDown
                    {
                        if (firstViewLine+1 <= this.VScrollBar.Maximum)
                        {
                            firstViewLine++;
                            this.Invalidate();
                            this.Update();
                        }
                        //Console.WriteLine("WheelDown");
                    }
                    else // WheelUp
                    {
                        if (firstViewLine != 0)
                        {
                            firstViewLine--;
                            this.Invalidate();
                            this.Update();
                            //Console.WriteLine("WheelUp");
                        }
                    }
                    #endregion
                    break;
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// 조정값에 따라 리스트 박스 스크롤 이동, firstViewLineAdjust++ or firstViewLineAdjust--
        /// </summary>
        public int firstViewLineAdjust
        {
            get
            {
                return this.firstViewLine;
            }

            set
            {
                if (this.firstViewLine > value) // Wheel Up 이벤트
                {
                    if (this.firstViewLine > 0)
                    {
                        this.firstViewLine--;
                        this.Invalidate();
                        this.Update();
                    }
                }
                else // Wheel Down 이벤트
                {
                    if (this.lastViewLine < lineInfo.Count-1)
                    {
                        this.firstViewLine++;
                        this.Invalidate();
                        this.Update();
                    }
                }
            }
        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            int lineCount = 0;
            for(int i = firstViewLine; i <= lastViewLine; i++)
            {
                Rectangle rc = new Rectangle(0,lineCount * this.Font.Height,this.Width - this.VScrollBar.Width, this.Font.Height);

                if (rc.Contains(e.Location))
                {
                    if(i <= this.lineInfo.Count-1)
                    {
                        this.selectedLine = i;
                        this.Invalidate();
                        this.Update();
                    }
                    break;
                }
                lineCount++;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            int lineCount = 0;
            for (int i = firstViewLine; i <= lastViewLine; i++)
            {
                Rectangle rc = new Rectangle(0, lineCount * this.Font.Height, this.Width - this.VScrollBar.Width, this.Font.Height);
                if (rc.Contains(e.Location))
                {
                    if (i <= this.lineInfo.Count - 1)
                    {
                        this.selectedLine = i;
                        if (this.selectWord != null)
                        {
                            this.selectWord(lineInfo[this.selectedLine][0]);
                            this.Dispose();
                        }
                        else
                        {
                            this.Invalidate();
                            this.Update();
                        }
                    }
                    break;
                }
                lineCount++;
            }

            //더블클릭은 선택했다는 의미이므로 창을 닫는다.

            base.OnMouseDoubleClick(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Up)
            {
                selectUpperRow();
                return true;
            }
            else if (keyData == Keys.Down)
            {
                selectBelowRow();
                return true;
            }
            else if (keyData == Keys.Left || keyData == Keys.Right)
            {
                return true;
            }
            else if (keyData == Keys.Enter)
            {
                if (this.selectWord != null)
                {
                    this.selectWord(lineInfo[this.selectedLine][0]);
                    this.Dispose();
                }
                return true;
            }
            else if (keyData == Keys.Escape)
                this.Dispose();
            else if (keyData == Keys.PageUp)
            {
                this.pageUp();
                return true;
            }
            else if (keyData == Keys.PageDown)
            {
                this.pageDown();
                return true;
            }
            //return base.ProcessCmdKey(ref msg, keyData);
            return true;
        }

        private void VScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.OldValue > e.NewValue || e.OldValue < e.NewValue) //이전값이 바뀔값보다 클때, 즉 ScollUp, //이전값이 바뀔값보다 작을때, 즉 ScrollDown
            {
                this.firstViewLine = e.NewValue;
                //Console.WriteLine(this.firstViewLine);
                this.Invalidate();
                this.Update();
            }
        }

        /// <summary>
        /// 현재 선택된 라인의 단어를 return;
        /// </summary>
        /// <returns></returns>
        public string getSelectedWord()
        {
            return lineInfo[this.selectedLine][0];
        }

        /// <summary>
        /// 리스트 목록 한개 추가
        /// </summary>
        /// <param name="title">텍스트</param>
        /// <param name="description">설명</param>
        /// <param name="objectType">타입</param>
        public void InsertRow(string title, string description, string objectType)
        {
            this.lineInfo.Add(new string[] { title, description, objectType });
        }

        /// <summary>
        /// 현재 선택된 Row의 설명창 표시
        /// </summary>
        public void viewDescription()
        {
            if (this.Focused)
            {
                string description = this.lineInfo[m_selectedLine][1];
                if (!string.IsNullOrWhiteSpace(description))
                {
                    intellisenseToolTip.ToolTipTitle = Convert.ToString(this.lineInfo[m_selectedLine][0]);
                    intellisenseToolTip.Show(description, this, this.Width, 0);
                }
                else
                {
                    intellisenseToolTip.Hide(this);
                }
            }
            else
            {
                if (req_viewDescription != null)
                {
                    req_viewDescription(this.lineInfo[m_selectedLine][0],this.lineInfo[m_selectedLine][1]);
                }
            }
        }

        /// <summary>
        /// Row를 한칸 위로
        /// </summary>
        public void selectUpperRow()
        {
            if (this.selectedLine - 1 >= 0)
            {
                this.selectedLine--;

                if (this.firstViewLine > this.selectedLine)
                {
                    this.firstViewLine = this.selectedLine;
                }
                this.Invalidate();
                this.Update();
            }
        }

        /// <summary>
        /// Row를 한칸 아래로
        /// </summary>
        public void selectBelowRow()
        {
            if (this.selectedLine < this.lineInfo.Count - 1)
            {
                this.selectedLine++;
                if (this.lastViewLine < this.selectedLine)
                {
                    this.firstViewLine = this.selectedLine - (this.m_viewLIneCount - 1);
                }
                this.Invalidate();
                this.Update();
            }
        }

        /// <summary>
        /// 페이지 업
        /// </summary>
        public void pageUp()
        {
            if (!this.Focused)
                this.Focus();
            int minusLineCount = viewLineCount;
            for (int i = minusLineCount; i > 0; i--)
            {
                if ((this.selectedLine - i) >= 0)
                {
                    this.selectedLine -= i;
                    //this.firstViewLine = this.selectedLine;
                    this.Invalidate();
                    this.Update();
                    break;
                }
            }
        }

        /// <summary>
        /// 페이지 다운
        /// </summary>
        public void pageDown()
        {
            if (!this.Focused)
                this.Focus();
            int plusLineCount = viewLineCount;
            for (int i = plusLineCount; i > 0; i--)
            {
                if ((this.selectedLine + i) < this.lineInfo.Count)
                {
                    //this.firstViewLine = Math.Min(this.selectedLine, this.lineInfo.Count - viewLineCount - 1);
                    this.selectedLine += i;
                    this.Invalidate();
                    this.Update();
                    break;
                }
            }
        }

        /// <summary>
        /// ViewLineCount값에 의한 컨트롤의 높이 자동 지정
        /// </summary>
        public void autoSetSizeByViewLineCount()
        {
            this.Height = this.Font.Height * (this.m_viewLIneCount+1);
            this.MinimumSize = new Size(originalWidth, this.Height);
            this.MaximumSize = new Size(originalWidth, this.Height);
        }

        [Browsable(true), Description("한번에 보일 Line의 개수를 지정")]
        public int viewLineCount
        {
            get
            {
                return this.m_viewLIneCount - 1;
            }
            set
            {
                this.m_viewLIneCount = value + 1;
            }

        }


        private new void Dispose()
        {
            intellisenseToolTip.Hide(this);
            if(this.Parent != null)
                this.Parent.Dispose();
            base.Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(this.BackColor);
            if (lineInfo.Count > 0)
            {
                int vscrollMax = this.lineInfo.Count - this.m_viewLIneCount;
                if (vscrollMax < 0)
                {
                    vscrollMax = this.lineInfo.Count - 1;
                }
                this.VScrollBar.Maximum = vscrollMax; //스크롤바 조정

                this.VScrollBar.Value = this.firstViewLine;

                lastViewLine = firstViewLine + (this.m_viewLIneCount - 1);

                // 사용자 선택 영역 표시
                if (firstViewLine <= this.selectedLine && this.selectedLine <= lastViewLine)
                    e.Graphics.FillRectangle(Brushes.DodgerBlue, new Rectangle(0, (this.selectedLine - firstViewLine) * this.Font.Height, this.Width - this.VScrollBar.Width, this.Font.Height));

                int lineCount = 0;
                for (int i = firstViewLine; i <= this.lastViewLine; i++)
                {
                    if (this.lineInfo.Count <= i)
                        break;

                    //아이콘 Draw
                    if (lineInfo[i][2] != null)
                    {
                        string objectType = (string)lineInfo[i][2];
                        Image icon_image = null;
                        switch (objectType)
                        {
                            case "class":
                                icon_image = PaintTextBoxClassLib.Properties.Resources._class;
                                break;
                            case "property":
                                icon_image = PaintTextBoxClassLib.Properties.Resources._property;
                                break;
                            case "method":
                                icon_image = PaintTextBoxClassLib.Properties.Resources.method_1;
                                break;
                            case "event":
                                icon_image = PaintTextBoxClassLib.Properties.Resources._event;
                                break;
                            case "control":
                                icon_image = PaintTextBoxClassLib.Properties.Resources.control;
                                break;
                            case "transaction":
                                icon_image = PaintTextBoxClassLib.Properties.Resources.transaction;
                                break;
                        }
                        if (icon_image != null)
                            e.Graphics.DrawImage(icon_image, new Rectangle(0, lineCount * this.Font.Height, this.Font.Height, this.Font.Height));
                    }

                    //글자 Write
                    e.Graphics.DrawString(lineInfo[i][0], this.Font, Brushes.Black, new Rectangle(this.Font.Height, lineCount * this.Font.Height, this.Width - this.VScrollBar.Width, this.Font.Height));

                    if (lineCount == this.m_viewLIneCount - 1)
                    {
                        break;
                    }
                    lineCount++;

                }
                //lastViewLine = firstViewLine + this.m_viewLIneCount - 1;

            }

            base.OnPaint(e);
        }
    }
}
