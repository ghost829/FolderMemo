using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

#region Plan
// 최종본이 PaintTextBox이고 여기에 LinesTextBox와 자동완성기능(AutoWord?)을 붙인다.
// 131028 - 자동완성기능 추가

#endregion

namespace PaintTextBoxClassLib
{
    public partial class PaintTextBox : UserControl
    {
        /// <summary>
        /// 외부에서 LinesTextBox로 접근하기 위한 프로퍼티
        /// </summary>
        [Browsable(true), Category("TextBox"), Description("텍스트 상자")]
        public LinesTextBox TextBox
        {
            get
            {
                return this.linesTextBox1;
            }
        }

        /// <summary>
        /// 자동완성 Node 정의
        /// </summary>
        public List<frmTreeNode> nodeList = new List<frmTreeNode>();

        public delegate bool throwData(object obj);
        public event throwData TextBoxInfoData;           // 
        public event throwData TextBoxProcessCmdKeyCheck; // lineTextBox가 키입력을 받았을때 실행될 이벤트
        public event throwData TextBoxIsDefaultText;      // lineTextBox의 텍스트가 변할때 실행될 이벤트 - true면 DefaultText이다.
        public event throwData TextBoxIsGotFocus;         // lineTextBox가 포커스 됬을때 실행될 이벤트
        public event throwData TextBoxIsLostFocus;        // lineTextBox가 포커스를 잃었을때 실행될 이벤트

        #region ## Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public PaintTextBox()
        {
            InitializeComponent();
            linesTextBox1.vScrollSetValue += linesTextBox1_vScrollSetValue;
            linesTextBox1.hScrollSetValue += linesTextBox1_hScrollSetValue;
            linesTextBox1.CaretMoved += linesTextBox1_CaretMoved;
            linesTextBox1.processKeyCheck += linesTextBox1_processKeyCheck;
            linesTextBox1.throwIsDefaultText += linesTextBox1_throwIsDefaultText;
            linesTextBox1.LostFocus += linesTextBox1_LostFocus;
            linesTextBox1.GotFocus += linesTextBox1_GotFocus;
            linesTextBox1.MouseWheel += new MouseEventHandler(delegate (object sender, MouseEventArgs e) {
                if (e.Delta < 0) // WheelDown
                {
                    if (IntellisenseVisible)
                    {
                        IntellisenseBox.firstViewLineAdjust++;
                    }
                }
                else // WheelUp
                {
                    if (IntellisenseVisible)
                    {
                        IntellisenseBox.firstViewLineAdjust--;
                    }
                }
        });
        }
        #endregion

        #region ## Consturctor Support Method
        /// <summary>
        /// 캐럿이 움직였을 경우 해당 메서드 실행됨 - 현재 selectionLineNumber, selectionStartIndex 정보를 넘겨줌
        /// </summary>
        void linesTextBox1_CaretMoved()
        {
            if (TextBoxInfoData != null)
            {
                object obj = new int[] { this.linesTextBox1.selectionLineNumber, this.linesTextBox1.selectionStartIndex };
                TextBoxInfoData(obj);
            }

            if (IntellisensePopup != null)
                IntellisesneBoxSetHide();
        }

        /// <summary>
        /// linetextBox에서 processCmdkey를 전달받아서 체크
        /// </summary>
        /// <param name="processCmdKey">lineTextbox에서 실행된 processCmdkey의 파라미터값 </param>
        /// <returns>return값이 true일경우 linetextbox에서 key이벤트 발생시키지 않음</returns>
        bool linesTextBox1_processKeyCheck(Keys processCmdKey)
        {
            // 인텔리센스 박스가 보이는 상태일 경우
            if (IntellisenseVisible)
            {
                #region ** IntellisenseBox 컨트롤
                Keys keyData = processCmdKey;

                // base.ProcessCmdkey실행시 방향키 입력할때마다 HScroll과 VScroll의 값이 변경되므로 막아줌.
                if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.Home || keyData == Keys.End)
                {
                    if (keyData == Keys.Up)
                    {
                        IntellisenseBox.selectUpperRow();
                    }
                    else if (keyData == Keys.Down)
                    {
                        IntellisenseBox.selectBelowRow();
                    }
                    return true;
                }
                else if (keyData == Keys.PageDown)
                {
                    if (IntellisenseBox.selectedLine == -1)
                        IntellisenseBox.selectedLine = 0;
                    IntellisenseBox.pageDown();
                    return true;
                }
                else if (keyData == Keys.PageUp)
                {
                    if (IntellisenseBox.selectedLine == -1)
                        IntellisenseBox.selectedLine = 0;
                    IntellisenseBox.pageUp();
                    return true;
                }
                else if (keyData == (Keys.Up | Keys.Shift) || keyData == (Keys.Down | Keys.Shift) ||
                         keyData == (Keys.Left | Keys.Shift) || keyData == (Keys.Right | Keys.Shift) ||
                         keyData == (Keys.Home) || keyData == (Keys.End))
                {
                    return true;
                }
                else if (keyData == Keys.Enter) //Enter키 누르면 인텔리센스박스에서 현재 선택된 라인의 단어 선택
                {
                    this.tmpBox_selectWord(IntellisenseBox.getSelectedWord());
                    if (IntellisensePopup != null)
                        IntellisesneBoxSetHide();
                    return true;
                }
                #endregion
            }

            // 부모 컨트롤이 ProcessCmdKey값을 체크할때 Key 전달
            if (TextBoxProcessCmdKeyCheck != null)
                return TextBoxProcessCmdKeyCheck(processCmdKey);
            else // linetextbox에서 processCmdkey 처리하도록 false 리턴
                return false;
        }


        /// <summary>
        /// lineTextBox 수정여부 전달 Event
        /// </summary>
        /// <param name="boolValue"></param>
        void linesTextBox1_throwIsDefaultText(bool boolValue)
        {
            if (TextBoxIsDefaultText != null)
                TextBoxIsDefaultText(boolValue);
            //throw new NotImplementedException();
        }

        #endregion


        /// <summary>
        /// ROLE -
        /// 1. 자동완성 기능 사용 중일 때 키보드 입력 시 기능을 인텔리센스박스로 돌려줌
        /// 2. 자동완성 기능을 사용하지 않더라도 방향키나 Home, END키 입력 시 PaintTextBox의 Scroll이 움직이므로 이를 방지
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // 자동완성(Intellisense) 기능 사용시
            if (AutoWordCompleteMode)
            {
                #region ** IntellisenseBox OutPut
                if (keyData == (Keys.Control | Keys.Space))
                {
                    if (IntellisensePopup != null)
                        IntellisesneBoxSetHide();
                    showIntellisenseBox();
                    return true;
                }
                //인텔리센스박스가 보일경우
                if (IntellisenseVisible)
                {
                    if (keyData == Keys.Escape) //ESC키 누르면 인텔리센스박스 안보이게
                    {
                        IntellisesneBoxSetHide();
                        return true;
                    }
                }
                else
                {
                    if (keyData == Keys.Escape) //ESC키 누르면 설명박스 안보이게
                    {
                        descriptionBoxSetHide();
                        return true;
                    }
                }
                #endregion
            }

            // base.ProcessCmdkey실행시 방향키 입력할때마다 HScroll과 VScroll의 값이 변경되므로 막아줌.
            if (keyData == (Keys.Up | Keys.Shift) || keyData == (Keys.Down | Keys.Shift) ||
                keyData == (Keys.Left | Keys.Shift) || keyData == (Keys.Right | Keys.Shift) ||
                keyData == (Keys.Home) || keyData == (Keys.End))
            {
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }


        #region ** Scroll Event
        public void setScrollBarHidden(bool hiddenY)
        {
            if (hiddenY)
            {
                this.hScroll.Visible = false;
                this.vScroll.Visible = false;
            }
            else
            {
                this.hScroll.Visible = true;
                this.vScroll.Visible = true;
            }
        }

        /// <summary>
        /// 텍스트박스에서 가로스크롤 값 변경 요청시 실행되는 이벤트
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maximum"></param>
        void linesTextBox1_hScrollSetValue(int value, int maximum)
        {
            this.hScroll.Maximum = maximum;
            this.hScroll.Value = value;
        }

        /// <summary>
        /// 텍스트박스에서 세로스크롤 값 변경 요청시 실행되는 이벤트
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maximum"></param>
        void linesTextBox1_vScrollSetValue(int value, int maximum)
        {
            this.vScroll.Maximum = maximum;
            this.vScroll.Value = value;
        }

        /// <summary>
        /// 세로스크롤 이동시 발생되는 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void vScroll_Scroll(object sender, ScrollEventArgs e)
        {
            // 스크롤 이동시 이벤트 두번 발생됨.
            // 첫번째 이벤트는 e.OldValue와 e.NewValue가 다를때, 즉 값 변경 실행 전
            // 두번째 이벤트는 e.OldValue와 e.Newvalue가 같고 해당 스크롤의 Value가 같을때, 즉 값 변경 후
            //Console.WriteLine(e.OldValue + ":" + e.NewValue + " = " + vScroll.Value);

            if (e.OldValue > e.NewValue || e.OldValue < e.NewValue) //이전값이 바뀔값보다 클때, 즉 ScollUp, //이전값이 바뀔값보다 작을때, 즉 ScrollDown
            {
                this.linesTextBox1.firstViewLine = e.NewValue;
                this.linesTextBox1.Invalidate();
                this.linesTextBox1.Update();
            }
        }

        /// <summary>
        /// 가로스크롤 이동시 발생되는 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hScroll_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.OldValue > e.NewValue || e.OldValue < e.NewValue)
            {
                this.linesTextBox1.firstViewIndex = e.NewValue;
                this.linesTextBox1.Invalidate();
                this.linesTextBox1.Update();
            }
        }

        #endregion

        #region ** Properties - LineTextBox Properties

        #region Extends LinesTextBox Properties

        /// <summary>
        /// 폰트 - 수정시 텍스트박스 Invalidate 반영
        /// </summary>
        [Browsable(true), Category("PaintTextBox"), Description("텍스트 박스 폰트 지정")]
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                this.linesTextBox1.Font = value;
            }
        }

        [Browsable(true), Category("PaintTextBox"), Description("왼쪽여백 지정")]
        public int IndentValue
        {
            get
            {
                return this.linesTextBox1.indentValue;
            }
            set
            {
                this.linesTextBox1.indentValue = value;
            }
        }

        [Browsable(true), Category("PaintTextBox"), Description("키워드1번 문자열을 지정")] //키워드 지정
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        //public string[] keyword1
        //{
        //    get
        //    {
        //        return linesTextBox1.keyword1.ToArray();
        //    }
        //    set
        //    {
        //        linesTextBox1.keyword1 = new List<string>(value);
        //    }
        //}
        public List<string> keyword1
        {
            get
            {
                return this.TextBox.keyword1;
            }
            set
            {
                this.TextBox.keyword1 = value;
            }
        }

        [Browsable(true), Category("PaintTextBox"), Description("키워드1번에 등록된 문자열의 색상을 지정")] //키워드 Color 지정
        public Color keyword1_Color
        {
            get
            {
                return this.TextBox.keyword1_Color;
            }
            set
            {
                this.TextBox.keyword1_Color = value;
            }
        }

        [Browsable(true), Category("PaintTextBox"), Description("키워드2번 문자열을 지정")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        //public string[] keyword2
        //{
        //    get
        //    {
        //        return linesTextBox1.keyword2.ToArray();
        //    }
        //    set
        //    {
        //        linesTextBox1.keyword2 = new List<string>(value);
        //    }
        //}
        public List<string> keyword2
        {
            get
            {
                return this.TextBox.keyword2;
            }
            set
            {
                this.TextBox.keyword2 = value;
            }
        }

        [Browsable(true), Category("PaintTextBox"), Description("키워드2번에 등록된 문자열의 색상을 지정")] //키워드 Color 지정
        public Color keyword2_Color
        {
            get
            {
                return this.TextBox.keyword2_Color;
            }
            set
            {
                this.TextBox.keyword2_Color = value;
            }
        }

        [Browsable(true), Category("PaintTextBox"), Description("키워드3번 문자열을 지정")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        //public string[] keyword3
        //{
        //    get
        //    {
        //        return linesTextBox1.keyword3.ToArray();
        //    }
        //    set
        //    {
        //        linesTextBox1.keyword3 = new List<string>(value);
        //    }
        //}

        public List<string> keyword3
        {
            get
            {
                return this.TextBox.keyword3;
            }
            set
            {
                this.TextBox.keyword3 = value;
            }
        }

        [Browsable(true), Category("PaintTextBox"), Description("키워드3번에 등록된 문자열의 색상을 지정")] //키워드 Color 지정
        public Color keyword3_Color
        {
            get
            {
                return this.TextBox.keyword3_Color;
            }
            set
            {
                this.TextBox.keyword3_Color = value;
            }
        }

        //[Browsable(true), Category("PaintTextBox"), Description("키워드4(1)번 문자열을 지정")] //키워드 지정
        //[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        //public string[] keyword4
        //{
        //    get
        //    {
        //        return linesTextBox1.keyword1.ToArray();
        //    }
        //    set
        //    {
        //        linesTextBox1.keyword1 = new List<string>(value);
        //    }
        //}

        [Browsable(true), Category("PaintTextBox"), Description("주석처리 문자열(고정)")]
        public string comment
        {
            get { return "//"; }
        }

        [Browsable(true), Category("PaintTextBox"), Description("주석 색상 지정")]
        public Color comment_Color
        {
            get { return this.TextBox.comment_Color; }
            set
            {
                this.TextBox.comment_Color = value;
            }
        }

        [Browsable(true), Category("PaintTextBox"), Description("라인넘버 Visible여부")]
        public bool LineNumberVisible
        {
            get
            {
                return this.linesTextBox1.LineNumberVisible;
            }
            set
            {
                linesTextBox1.LineNumberVisible = value;
            }
        }

        [Browsable(true), Category("PaintTextBox"), Description("라인넘버 Color지정")]
        public Color LineNumberColor
        {
            get
            {
                return linesTextBox1.LineNumberColor;
            }
            set
            {
                linesTextBox1.LineNumberColor = value;
            }
        }

        [Browsable(true), Category("PaintTextBox"), Description("현재 선택된 단어와 같은 단어 표시 기능")]
        public bool MarkWordTheSameAsSelectionWord
        {
            get
            {
                return this.linesTextBox1.MarkWordTheSameAsSelectionWord;
            }
            set
            {
                this.linesTextBox1.MarkWordTheSameAsSelectionWord = value;
            }
        }

        [Browsable(true), Category("PaintTextBox"), Description("현재 선택된 단어와 같은 단어의 색상 지정")]
        public Color MarkWordColor
        {
            get
            {
                return this.linesTextBox1.MarkWordColor;
            }
            set
            {
                this.linesTextBox1.MarkWordColor = value;
            }
        }

        [Browsable(true), Category("PaintTextBox"), Description("\"\"(큰따옴표)에 감싸있는 텍스트 색상 지정")]
        public Color DoubleQuotesColor
        {
            get
            {
                return this.linesTextBox1.DoubleQuotesColor;
            }
            set
            {
                this.linesTextBox1.DoubleQuotesColor = value;
            }
        }

        [Browsable(true), Category("PaintTextBox"), Description("괄호옆에 캐럿 존재시 괄호에 강조될 색상 지정")]
        public Color BracketBackgroundBrush
        {
            get
            {
                return this.linesTextBox1.BracketBackgroundBrush;
            }
            set
            {
                this.linesTextBox1.BracketBackgroundBrush = value;
            }
        }

        [Browsable(true), Category("PaintTextBox"), Description("전경색 지정")]
        public Color TextForeColor
        {
            get
            {
                return this.linesTextBox1.ForeColor;
            }
            set
            {
                this.linesTextBox1.ForeColor = value;
            }
        }

        
        [Browsable(true), Category("PaintTextBox"), Description("배경색 지정")]
        public Color TextBackColor
        {
            get
            {
                return this.linesTextBox1.BackColor;
            }
            set
            {
                this.linesTextBox1.BackColor = value;
            }
        }

        [Browsable(true), Category("PaintTextBox"), Description("탭 허용")]
        public bool isAllowTab
        {
            get
            {
                return this.linesTextBox1.isAllowTab;
            }
            set
            {
                this.linesTextBox1.isAllowTab = value;
            }
        }

        #endregion

        [Browsable(true), Category("PaintTextBox"), Description("현재 선택된 라인의 테두리 표시 색")]
        public Color selectionBorderLinePen
        {
            get
            {
                return this.linesTextBox1.selectionBorderLinePen;
            }
            set
            {
                this.linesTextBox1.selectionBorderLinePen = value;
            }
        }

        [Browsable(true), Category("PaintTextBox"), Description("현재 선택된 문자열의 뒷배경색")]
        public Color selectionTextBackgroundBrush
        {
            get
            {
                return this.linesTextBox1.selectionTextBackgroundBrush;
            }
            set
            {
                this.linesTextBox1.selectionTextBackgroundBrush = value;
            }
        }

        private bool m_autowordCompleteMode = false; // 자동완성기능 사용여부
        [Browsable(true), Category("PaintTextBox"), Description("자동완성기능 사용여부")]
        public bool AutoWordCompleteMode
        {
            get
            {
                return m_autowordCompleteMode;
            }
            set
            {
                m_autowordCompleteMode = value;
                if (m_autowordCompleteMode)
                {
                    linesTextBox1.NormalTextChanged += linesTextBox1_NormalTextChanged;
                    //linesTextBox1.LostFocus += linesTextBox1_LostFocus;
                    //linesTextBox1.GotFocus += linesTextBox1_GotFocus;
                    linesTextBox1.descriptionVisible += linesTextBox1_descriptionVisible;

                    descriptionTooltip = new ToolTip();
                    descriptionTooltip.UseFading = false;
                }
                else
                {
                    linesTextBox1.NormalTextChanged -= linesTextBox1_NormalTextChanged;
                    //linesTextBox1.LostFocus -= linesTextBox1_LostFocus;
                    //linesTextBox1.GotFocus -= linesTextBox1_GotFocus;
                    linesTextBox1.descriptionVisible -= linesTextBox1_descriptionVisible;
                }
            }
        }

        #endregion


        //#region ** WndProc - MouseWheel Event(자동완성기능 true일때 사용)
        //private const int WM_MOUSEWHEEL = 0x020A;
        //protected override void WndProc(ref Message m)
        //{
        //    switch (m.Msg)
        //    {
        //        case WM_MOUSEWHEEL:
        //            if (m.WParam.ToInt32() < 0) // WheelDown
        //            {
        //                if (IntellisenseVisible)
        //                {
        //                    IntellisenseBox.firstViewLineAdjust++;
        //                }
        //            }
        //            else // WheelUp
        //            {
        //                if (IntellisenseVisible)
        //                {
        //                    IntellisenseBox.firstViewLineAdjust--;
        //                }
        //            }
        //            break;
        //        default:
        //            base.WndProc(ref m);
        //            break;
        //    }
        //}
        //#endregion

        #region ** IntellisenseBox Function

        private const int     SW_SHOWNA = 8;        // Window API - ShowWindow 옵션, 포커스 없이 윈도우를 출력
        private PopupWindow   IntellisensePopup;    // 인텔리센스 박스의 부모컨트롤 윈도우 (ListBox 혼자 윈도우로 띄울수는 없으므로 required)
        private CustomListBox IntellisenseBox;      // 인텔리센스 박스
        private bool m_IntellisenseVisible = false; // 인텔리센스 보여짐 여부
        int[] m_intellisenseEditIndex = new int[2]; // 인텔리센스 호출 후 텍스트 선택시 수정할 Index 위치 기록

        [System.Runtime.InteropServices.DllImport("user32", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private extern static int ShowWindow(IntPtr hWnd, int nCmdShow); // Window API - 윈도우 출력

        /// <summary>
        /// Focus Steal 없이 윈도우 출력
        /// </summary>
        /// <param name="hWnd"></param>
        private void ShowWindow(IntPtr hWnd)
        {
            ShowWindow(hWnd, SW_SHOWNA);
        }

        /// <summary>
        /// 현재 캐럿이 존재하는 부분의 근처의 텍스트를 읽어서 관련 Method명 출력
        /// </summary>
        private void showIntellisenseBox()
        {
            IntellisenseBox = new CustomListBox();

            m_intellisenseEditIndex = this.linesTextBox1.getCaretNearStringStartEndIndex();

            string inputText = this.linesTextBox1.getCaretNearStringIncludeDot();
            
            // . 이 있는 경우 - Method
            if (inputText.IndexOf('.') > -1)
            {
                string[] inputTextArr = inputText.Split('.');
                foreach (frmTreeNode depth1Node in nodeList)
                {
                    if (depth1Node.Name.Equals(inputTextArr[0]))
                    {
                        foreach (frmTreeNode depth2Node in depth1Node.Nodes)
                        {
                            if (depth2Node.Name.IndexOf(inputTextArr[1]) > -1)
                                IntellisenseBox.InsertRow(depth2Node.Name, (string)depth2Node.Text, (string)depth2Node.Tag);
                        }
                    }
                }
            }
            else
            {
                foreach (frmTreeNode depth1Node in nodeList)
                {
                    if (depth1Node.Name.IndexOf(inputText) > -1)
                    {
                        IntellisenseBox.InsertRow(depth1Node.Name, (string)depth1Node.Text, (string)depth1Node.Tag);
                    }
                }
            }

            if (IntellisenseBox.lineInfo.Count == 0)
                return;

            if (descriptionFinding)
                descriptionBoxSetHide();

            IntellisenseBox.autoSetSizeByViewLineCount();
            IntellisensePopup = new PopupWindow(IntellisenseBox);

            Point loc = getSelectionStartPointLocationByClient();
            ShowWindow(IntellisensePopup.Handle);
            
            IntellisensePopup.SetBounds(loc.X, loc.Y, IntellisensePopup.Bounds.Width, IntellisensePopup.Bounds.Height);
            IntellisenseBox.selectWord += tmpBox_selectWord;
            IntellisenseBox.req_viewDescription += IntellisenseBox_req_viewDescription;
            IntellisenseVisible = true;
        }


        /// <summary>
        /// 인텔리센스에서 단어 선택시 발생되는 이벤트
        /// </summary>
        /// <param name="str"></param>
        private void tmpBox_selectWord(string str)
        {
            this.linesTextBox1.Focus();
            this.linesTextBox1.replaceText(this.linesTextBox1.selectionLineNumber, m_intellisenseEditIndex[0], m_intellisenseEditIndex[1], str);
            IntellisenseVisible = false;
            this.linesTextBox1.Invalidate();
            this.linesTextBox1.Update();
        }


        /// <summary>
        /// 인텔리센스가 현재 Focus를 갖고있지 않을 시 PaintTextBox에서 대신 Tooptip에 Description을 뿌려준다.
        /// </summary>
        /// <param name="str"></param>
        void IntellisenseBox_req_viewDescription(string name, string description)
        {
            if (!string.IsNullOrWhiteSpace(description))
            {
                intellisenseToolTip.ToolTipTitle = Convert.ToString(name);

                Point caretPoint = this.linesTextBox1.getDrawPoint(this.linesTextBox1.selectionLineNumber, this.linesTextBox1.selectionStartIndex);
                caretPoint.Y += (int)(this.linesTextBox1.Font.Height * 1.2);
                //Point loc = getSelectionStartPointLocationByClient();
                //ShowWindow(IntellisensePopup.Handle);
                //IntellisensePopup.SetBounds(loc.X, loc.Y, loc.X, IntellisensePopup.Bounds.Height);
                intellisenseToolTip.Show(description, this, caretPoint.X + IntellisenseBox.Width, caretPoint.Y);
            }
            else
            {
                intellisenseToolTip.Hide(this);
            }
            //throw new NotImplementedException();
        }

        
        /// <summary>
        /// IntellisesnseBox Visible여부
        /// </summary>
        bool IntellisenseVisible
        {
            get
            {
                return this.m_IntellisenseVisible;
            }
            set
            {
                this.m_IntellisenseVisible = value;
                this.linesTextBox1.isProcessCmdkeySkip = value;
            }
        }


        /// <summary>
        /// Intellisense박스 숨기기
        /// </summary>
        private void IntellisesneBoxSetHide()
        {
            IntellisenseBox.selectWord -= tmpBox_selectWord;
            IntellisenseBox.req_viewDescription -= IntellisenseBox_req_viewDescription;
            intellisenseToolTip.Hide(this);
            IntellisensePopup.Dispose();
            IntellisenseBox.Dispose();
            IntellisenseBox = null;
            IntellisensePopup = null;
            IntellisenseVisible = false;
        }


        /// <summary>
        /// 텍스트 박스에 문자열 입력 발생시 실행될 메서드 - 인텔리센스박스 보이기
        /// </summary>
        private void linesTextBox1_NormalTextChanged()
        {
            if (IntellisensePopup != null)
            {
                IntellisesneBoxSetHide();
            }

            //공백이 아닐경우 현재 입력된 텍스트를 검색해서 관련 Method출력
            if (!String.IsNullOrWhiteSpace(this.linesTextBox1.getCaretNearStringIncludeDot()))
            {
                if(this.TextBox.selectionStartIndex -1 > 0)
                {
                    if (this.TextBox.getLineInfo[this.TextBox.selectionLineNumber].m_textType[this.TextBox.selectionStartIndex - 1] != Lines.TextType.PlainText)
                        return;
                }
                
                if(!descriptionFinding)
                    showIntellisenseBox();
            }
        }


        /// <summary>
        /// 인텔리센스 박스가 보이는 상태에서 lineTextBox클릭시 인텔리센스박스 제거
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void linesTextBox1_GotFocus(object sender, EventArgs e)
        {
            if (IntellisenseVisible) //인텔리센스 박스가 보이는 상태에서 lineTextBox클릭시
            {
                IntellisesneBoxSetHide();
            }

            if (TextBoxIsGotFocus != null)
                TextBoxIsGotFocus(e);
        }


        /// <summary>
        /// 현재 텍스트 박스에 포커스를 잃었을때 IntellisenseBox가 존재할 경우 제거
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void linesTextBox1_LostFocus(object sender, EventArgs e)
        {
            if (IntellisenseVisible)
            {
                Timer fakeTech1 = new Timer();
                fakeTech1.Interval = 50;
                fakeTech1.Tick += new EventHandler(delegate
                {
                    if (IntellisensePopup != null ? !IntellisensePopup.Focused && !IntellisenseBox.Focused : false)
                    {
                        IntellisesneBoxSetHide();
                    }
                    fakeTech1.Stop();
                });
                fakeTech1.Start();
            }
            //IntellisesneBoxSetHide();
            descriptionBoxSetHide();

            if (TextBoxIsLostFocus != null)
                TextBoxIsLostFocus(e);
        }


        /// <summary>
        /// 텍스트 박스에서 현재 Caret이 존재하는 부분의 화면좌표기준 Point값을 가져온다.
        /// </summary>
        /// <returns></returns>
        private Point getSelectionStartPointLocationByClient()
        {
            Rectangle screenRectangle = RectangleToScreen(this.ClientRectangle);
            int titleHeight = screenRectangle.Top - this.Parent.Top;
            int formBorderWidth = screenRectangle.Left - this.Parent.Left;

            Point caretPoint = this.linesTextBox1.getDrawPoint(this.linesTextBox1.selectionLineNumber, this.linesTextBox1.selectionStartIndex);
            caretPoint.Y += (int)(this.linesTextBox1.Font.Height * 1.2);
            return PointToScreen(caretPoint);
        }

        #endregion

        #region ** descriptionToolTipManagement
        private ToolTip descriptionTooltip; //설명창
        bool descriptionFinding = false; //설명창에 표시할 Method 발견시 true

        /// <summary>
        /// 설명창 출력
        /// </summary>
        private void showDescriptionToolTip()
        {
            object[] wordInfo = this.linesTextBox1.getMethodByOpenBracketStartEndIndex();
            string txt = (string)wordInfo[1];
            
            if (!string.IsNullOrWhiteSpace(txt))
            {
                if (txt.IndexOf('.') > -1)
                {
                    string descriptionTxT = string.Empty;
                    string[] splitTextArr = txt.Split('.');
                    foreach (frmTreeNode depth1Node in nodeList)
                    {
                        if (depth1Node.Name.IndexOf(splitTextArr[0]) > -1)
                        {
                            foreach (frmTreeNode depth2Node in depth1Node.Nodes)
                            {
                                if (depth2Node.Name.Equals(splitTextArr[1]))
                                    descriptionTxT = depth2Node.Text;
                            }
                        }
                    }
                    if (descriptionTxT != string.Empty)
                    {
                        int[] startEndIndex = (int[])wordInfo[0];
                        Point txtPos = this.linesTextBox1.getDrawPoint(this.linesTextBox1.selectionLineNumber, startEndIndex[0]);
                        if(txtPos.X == -1)
                            txtPos = this.linesTextBox1.getDrawPoint(this.linesTextBox1.selectionLineNumber, this.linesTextBox1.selectionStartIndex);
                        txtPos.Y = txtPos.Y + (int)(this.linesTextBox1.Font.Height * 1.2);

                        descriptionTooltip.ToolTipTitle = txt;
                        descriptionTooltip.Show(descriptionTxT, this, txtPos);
                        descriptionFinding = true;
                        return;
                    }
                }
            }
            else
                this.descriptionBoxSetHide();

            descriptionFinding = false;
        }

        /// <summary>
        /// 설명창 안보이게
        /// </summary>
        private void descriptionBoxSetHide()
        {
            if (descriptionTooltip != null)
            {
                descriptionTooltip.Hide(this);
                descriptionFinding = false;
            }
        }

        /// <summary>
        /// 설명창 보이기 여부
        /// </summary>
        /// <param name="visible"></param>
        void linesTextBox1_descriptionVisible(bool visible)
        {
            //throw new NotImplementedException();
            if (visible)
                this.showDescriptionToolTip();
            else
                this.descriptionBoxSetHide();
        }
        #endregion

        #region ** TAG Function (TAG와 같이 특정값 저장)

        List<object[]> dataCollection = new List<object[]>(); // 기타 저장 공간

        /// <summary>
        /// 특정 키값으로 데이터 저장
        /// </summary>
        /// <param name="keyName">키이름</param>
        /// <param name="data">데이터</param>
        public void setData(string keyName, object data)
        {
            foreach (object[] tmp in dataCollection)
            {
                if ((string)tmp[0] == keyName)
                {
                    tmp[1] = data;
                    return;
                }
            }
            dataCollection.Add(new object[] { keyName, data });
        }

        /// <summary>
        /// 특정 키값의 데이터 추출
        /// </summary>
        /// <param name="keyName">키이름</param>
        /// <returns></returns>
        public object getData(string keyName)
        {
            foreach (object[] tmp in dataCollection)
            {
                if ((string)tmp[0] == keyName)
                {
                    return tmp[1];
                }
            }
            return new object[] { };
        }
        #endregion


        /// <summary>
        /// [TEST] 자동완성기능 테스트 데이터
        /// </summary>
        private void setTestAutoword()
        {
            frmTreeNode node = new frmTreeNode("container");
            node.Name = "container";
            node.Text = "container description";
            node.Tag = "class";
            frmTreeNode node_1 = new frmTreeNode("popscreen");
            node_1.Name = "popscreen";
            node_1.Text = "Description : container popscreen method";
            node_1.Tag = "method";
            frmTreeNode node_2 = new frmTreeNode("popscreen1");
            node_2.Name = "popscreen1";
            node_2.Text = "Description : container popscreen method";
            node_2.Tag = "method";
            frmTreeNode node_3 = new frmTreeNode("popscreen2");
            node_3.Name = "popscreen3";
            node_3.Text = "Description : container popscreen3 method";
            node_3.Tag = "method";
            frmTreeNode node_4 = new frmTreeNode("popscreen4");
            node_4.Name = "popscreen4";
            node_4.Text = "Description : container popscreen method";
            node_4.Tag = "method";
            frmTreeNode node_5 = new frmTreeNode("popscreen5");
            node_5.Name = "popscreen5";
            node_5.Text = "Description : container popscreen method";
            node_5.Tag = "method";
            frmTreeNode node_6 = new frmTreeNode("popscreen6");
            node_6.Name = "popscreen6";
            node_6.Text = "Description : container popscreen method";
            node_6.Tag = "method";
            frmTreeNode node_7 = new frmTreeNode("popscreen7");
            node_7.Name = "popscreen7";
            node_7.Text = "Description : container popscreen method";
            node_7.Tag = "method";
            node.Nodes.Add(node_1);
            node.Nodes.Add(node_2);
            node.Nodes.Add(node_3);
            node.Nodes.Add(node_4);
            node.Nodes.Add(node_5);
            node.Nodes.Add(node_6);
            node.Nodes.Add(node_7);
            nodeList.Add(node);
        }

        /// <summary>
        /// [TEST] Property 테스트 데이터
        /// </summary>
        private void setTestDefaultValue()
        {
            this.TextBackColor = Color.White;
            this.TextForeColor = Color.Black;
            this.BracketBackgroundBrush = Color.LightGray;
            this.comment_Color = Color.Green;
            this.DoubleQuotesColor = Color.Red;
            this.Font = new Font("돋움체", 14);
            this.IndentValue = 30;
            this.keyword1 = new List<string>(new string[] { "function", "switch", "case", "break", "for" });
            //this.keyword1 = new string[] { "function", "switch", "case", "break", "for" };
            this.keyword1_Color = Color.Blue;
            this.keyword2 = new List<string>(new string[] { "var" });
            //this.keyword2 = new string[] { "var" };
            this.keyword2_Color = Color.Red;
            this.keyword3 = new List<string>(new string[] { "default" });
            //this.keyword3 = new string[] { "default" };
            this.keyword3_Color = Color.Orange;
            this.LineNumberColor = Color.Silver;
            this.LineNumberVisible = true;
            this.MarkWordColor = Color.LightGray;
            this.MarkWordTheSameAsSelectionWord = true;
            this.selectionBorderLinePen = Color.LightSteelBlue;
            this.selectionTextBackgroundBrush = Color.LightSkyBlue;
        }


    }
}
