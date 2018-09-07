using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.ComponentModel;

#region // !Development Note!
// 
// ㅇ 적용된 사항
// - Undo, Redo
//    - Tip! - Stack 사용해서 Push, Pull로 데이터 집어넣고 꺼내는데 집어넣을 데이터는 {Text, firstViewLine,
//             firstViewIndex, SelectionLineNumber, SelectionLineIndex}만 있으면 될듯!!!
//    - Keyword - Memento Pattern -2013.10.24-
// - 문자열 더블클릭시 단어 선택
// - 키보드 입력시 텍스트 추가(한,영,특문)
// - Select 기능
//    - (Shift + 방향키, Home,End,마우스클릭), 마우스 드래그 시 Text Select
//    - (Control + Home, End) 페이지 최상단, 최하단으로 이동
// - 현재 Select된 Word와 같은 단어 표시
// - " " DoubleQuotes안에 StringText 내부에 표시 기능
// - Bracket표시(캐럿옆에 괄호 존재시)
// - Tab, Shift+Tab 들여쓰기, 내어쓰기 (MultiLine Select시)
// - Ctrl+좌우방향키, Shift+Ctrl+좌우방향키 캐럿 단어 단위 이동 기능
// - Ctrl+ / 주석 기능 , Alt + / 주석해제 기능
// - Shift + PageUp,PageDown 시 텍스트 셀렉트

// ㅇ 적용해야될 사항
// - /* */ 생략 기능(TextColor)

// 101023 - 기열 발견
// - IntellisenseBox Visible시 Enter기능 안먹힘 - [수정완료]
// - 많은 텍스트를 Select상태에서 Delete시 Exception Error 발생 - 메모리 문제 [보류]
// - 마우스로 텍스트 셀랙션 하면서 스크롤 이동하려할때 부드럽지 않음... 비쥬얼스튜디오처럼 부드럽게 되길 원함.

#endregion

namespace PaintTextBoxClassLib
{
    public class LinesTextBox : UserControl
    {
        int lineHeight; // 한줄의 높이값

        int marginX = 2, marginY = 3; //캐럿여백, 화면 상단 여백

        private int m_indentValue = 30; // 들여쓰기 여백

        public int firstViewLine = 0; //현재 텍스트에서 최상단에 보이는 텍스트의 실제 줄번호 (Vertical)
        public int firstViewIndex = 0; //현재 보이는 텍스트에서 맨 좌측에 위치한 텍스트의 Index (Horizon)
        public int lastViewLine; //현재 텍스트에서 최하단에 보이는 텍스트의 실제 줄번호 (Vertical)
        public int selectionLineNumber = 0; //캐럿이 위치해있는 곳의 줄번호
        public int selectionStartIndex = 0; //캐럿이 위치해있는 곳의 Index번호
        public int selectionEndIndex = 0; // 선택한 부분의 끝부분 Index번호
        public int selectionEndLineNumber = 0; //선택한 부분의 끝부분 줄번호

        public delegate void ScrollEvent(int value, int maximum);
        public event ScrollEvent vScrollSetValue; //세로스크롤 변경이 필요할때 이벤트 실행 (OnPaint에서 자동 호출하므로 invalidate만 잘쓰면됨)
        public event ScrollEvent hScrollSetValue; //가로스크롤 변경이 필요할때 이벤트 실행 (OnPaint에서 자동 호출하므로 invalidate만 잘쓰면됨)

        private int m_selectionLength = 0; //현재 선택된 문자열의 총 길이

        private Stack<txtStatusData> undoStack = new Stack<txtStatusData>();
        private Stack<txtStatusData> redoStack = new Stack<txtStatusData>();
        private struct txtStatusData
        {
            string text;
            int firstViewLine;
            int firstViewIndex;
            int selectionStartLineNumber;
            int selectionStartIndex;
            int selectionEndLineNumber;
            int selectionEndIndex;

            public txtStatusData(string txt, int vLine, int vIndex, int sSLineNumber, int sSIndex, int sELineNumber, int sEindex)
            {
                this.text = txt;
                this.firstViewLine = vLine;
                this.firstViewIndex = vIndex;
                this.selectionStartLineNumber = sSLineNumber;
                this.selectionStartIndex = sSIndex;
                this.selectionEndLineNumber = sELineNumber;
                this.selectionEndIndex = sEindex;
            }
            public object[] getDataObject()
            {
                return new object[]{this.text, this.firstViewLine, this.firstViewIndex, this.selectionStartLineNumber
                    , this.selectionStartIndex, this.selectionEndLineNumber, this.selectionEndIndex};
            }
        }
        
        private List<Lines> lineInfo = new List<Lines>(); // 라인 정보

        private bool isSelectionLineBorderVisible = true; // 캐럿이 존재하는 줄에 BorderLine표시 여부

        private Pen m_selectionBorderLinePen = Pens.LightSteelBlue; //캐럿이 존재하는 줄의 BorderLine 색
        private Brush m_selectionTextBackgroundBrush = Brushes.LightSkyBlue; // 선택된 텍스트의 뒷배경색 지정

        private bool isDraging = false;  //마우스 드래그 중일때 true
        private Point mouseDownPos; //마우스 Down한 위치
        private int mouseDownSelectionLineNumber = -1; //마우스 Down한 라인번호
        private int mouseDownselectionStartIndex = -1; //마우스 Down한 해당 라인의 Index

        private Timer Timer_OnMouseLeaveDraging = new Timer(); //마우스로 텍스트를 Draging시 마우스가 텍스트박스 밖으로 벗어날 경우 텍스트박스의 스크롤값을 조정하기 위한 timer

        private int fixedSelectionLine = -1; //현재 텍스트를 수정중인 Line

        private bool m_isDefaultText = true; //[멤버] 현재 텍스트박스에 출력된 텍스트가 초기 텍스트일때

        /// <summary>
        /// 현재 텍스트박스에 출력된 텍스트가 초기 텍스트일때
        /// </summary>
        public bool isDefaultText
        {
            get
            {
                return m_isDefaultText;
            }
            set
            {
                m_isDefaultText = value;
                if (throwIsDefaultText != null)
                    throwIsDefaultText(value);
            }
        }

        public delegate void BoolTypeEvent(bool boolValue);
        public event BoolTypeEvent throwIsDefaultText; // 텍스트박스의 텍스트가 수정되었는지에 대한 여부

        private bool isLoaded = false; //텍스트박스의 로드

        private bool m_lineNumberVisible = true; // 컨트롤 좌측에 라인번호 Visible여부
        private SolidBrush m_lineNumberColorBrush = (SolidBrush)Brushes.Silver; //컨트롤 좌측의 라인번호 및 구분선 색상 지정
        private Brush m_bracketBackgroundBrush = Brushes.LightGray; // 캐럿 옆에 존재하는 문자가 괄호일경우 괄호의 뒷배경색
        private SolidBrush m_plainTextBrush = (SolidBrush)Brushes.Black; //기본 텍스트 색

        public delegate void TextChangeEvent();
        /// <summary>
        /// 텍스트에 일반 문자를 입력할때마다 실행
        /// </summary>
        public event TextChangeEvent NormalTextChanged;

        public delegate void VoidEvent();
        /// <summary>
        /// 캐럿 이동될때마다 실행
        /// </summary>
        public event VoidEvent CaretMoved;

        public delegate void descriptionEvent(bool visible);
        /// <summary>
        /// 설명창을 표시해야할때 param = true, 제거해야할 때 param = false
        /// </summary>
        public event descriptionEvent descriptionVisible;

        /// <summary>
        /// ProcessCmdkey에 할당된 이벤트를 무시해야할때 true
        /// </summary>
        public bool isProcessCmdkeySkip = false;
        
        private bool m_markWordTheSameAsSelectionWord = true; //같은 단어 표시 기능

        /// <summary>
        /// 라인정보를 가져온다.(Set불허)
        /// </summary>
        public List<Lines> getLineInfo
        {
            get
            {
                return this.lineInfo;
            }
        }

        public delegate bool keyValidate(Keys processCmdKey);
        /// <summary>
        /// 부모컨트롤에게 해당 key를 써도되는건지 물어본다. true값 받으면 키 적용된거라고 판단, processcmdkey에서 return true
        /// </summary>
        public event keyValidate processKeyCheck;

        /// <summary>
        /// 최근 측정된 Caret위치 - LostFocus후에 GotFocus될때 Caret을 다시 그려주는데 캐럿의 위치값을 최근 보여준 위치로 지정
        /// </summary>
        private Point recentlyCaretPos;

        /// <summary>
        /// 텍스트 상태 영역 복사
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void copyTextBoxStatus(LinesTextBox source, LinesTextBox target)
        {
            target.isAllowTab = source.isAllowTab;
            target.Font = source.Font;
            target.ForeColor = source.ForeColor;
            target.BackColor = source.BackColor;
            target.m_comment_ColorBrush = source.m_comment_ColorBrush;
            target.m_doubleQuotes_ColorBrush = source.m_doubleQuotes_ColorBrush;
            target.m_bracketBackgroundBrush = source.m_bracketBackgroundBrush;
            target.m_keyword1_ColorBrush = source.m_keyword1_ColorBrush;
            target.m_keyword2_ColorBrush = source.m_keyword2_ColorBrush;
            target.m_keyword3_ColorBrush = source.m_keyword3_ColorBrush;
            target.m_lineNumberColorBrush = source.m_lineNumberColorBrush;
            target.m_markword_ColorBrush = source.m_markword_ColorBrush;
            target.m_plainTextBrush = source.m_plainTextBrush;
            target.m_selectionBorderLinePen = source.m_selectionBorderLinePen;
            target.keyword1Regex = source.keyword1Regex;
            target.keyword2Regex = source.keyword2Regex;
            target.keyword3Regex = source.keyword3Regex;
            target.Text = source.Text;
            target.firstViewLine = source.firstViewLine;
            target.firstViewIndex = source.firstViewIndex;
            target.selectionStartIndex = source.selectionStartIndex;
            target.selectionEndIndex = source.selectionEndIndex;
            target.selectionLineNumber = source.selectionLineNumber;
            target.selectionEndLineNumber = source.selectionEndLineNumber;
            target.selectionLengthCalculate();
        }

        /// <summary>
        /// Tab허용여부(false[default]:\t를 '    '로변환, true:\t 허용)
        /// </summary>
        //[System.ComponentModel.DefaultValue(true)]
        public bool isAllowTab{get;set;}

        #region ** Constructor
        public LinesTextBox()
        {
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.InitializeComponent();
            //this.Font = new Font(this.Font.SystemFontName, this.Font.Size, GraphicsUnit.Pixel);
            
            //this.fontWidth = (int)(14 * ((fontSize / 2) / 10));

            // SyntaxHighLight Default Value
            m_keyword1 = new List<string>();
            //m_keyword1.Add("public");
            m_keyword1_ColorBrush = (SolidBrush)Brushes.Blue;
            m_keyword2 = new List<string>();
            //m_keyword2.Add("void");
            m_keyword2_ColorBrush = (SolidBrush)Brushes.Red;
            m_keyword3 = new List<string>();
            m_keyword3_ColorBrush = (SolidBrush)Brushes.Yellow;
            //m_keyword3.Add("test");
            m_comment = new List<string>();
            m_comment_ColorBrush = (SolidBrush)Brushes.Green;
            m_comment.Add("//");
            //ForeColor = Color.Black;

            this.Font = new Font("돋움체", 14, GraphicsUnit.Pixel);

            m_wordSeparator_pattern = @"[^A-Za-z0-9ㄱ-힣_]";
            m_methodSeparator_pattern = @"[^A-Za-z0-9ㄱ-힣\._]";
            m_markword_ColorBrush = (SolidBrush)Brushes.LightGray;
            m_doubleQuotes_ColorBrush = (SolidBrush)Brushes.Red;


            //this.lineHeight = (int)(this.FontHeight * 1.3); // this.FontHeight;
            this.lineHeight = (int)(this.FontHeight * 1.2); // this.FontHeight;
            lastViewLine = getViewLinesCount();

            this.TabStop = false;


            //프로퍼티설정
            //foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this))
            //{
            //    DefaultValueAttribute myAttribute = (DefaultValueAttribute)property.Attributes[typeof(DefaultValueAttribute)];

            //    if (myAttribute != null)
            //    {
            //        property.SetValue(this, myAttribute.Value);
            //    }
            //}
        }

        #endregion

        /// <summary>
        /// 문서를 Load하는 기능 실행시 OnLoad에 Text붙일것. defaultText지정
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            //string tmpText = string.Empty;
            //for (int i = 0; i < 10; i++)
            //{
            //    tmpText += i + System.Environment.NewLine;
            //}
            //this.Text = tmpText;
            if (this.lineInfo.Count == 0)
            {
                //this.lineInfo.Add(new Lines(this.ForeColor));
                //this.settingSyntaxHighLight();
                this.Text = string.Empty;
            }
            //defaultText = string.Empty;

            //캐럿 생성
            //CreateCaret(this.Handle, IntPtr.Zero, 0, lineHeight);

            //IMM 연결
            //ImeHandle = ImmGetContext(IntPtr.Zero);


            Timer_OnMouseLeaveDragingSetting();
            //setCaretWidth(0);
            isLoaded = true;

            base.OnLoad(e);
        }


        #region ** Caret Management
        // ShowCaret및 HideCaret 중복 수행시( ex-ShowCaret을 2번 실행하거나 HideCaret을 2번 실행한 경우)
        // ShowCaret은 Caret이 2개가 생겨버리고 HideCaret은 Caret이 보이지 않게 된다.
        // 따라서 캐럿을 한개로 보이도록 관리
        private bool m_isShowCaret = false;

        /// <summary>
        /// 캐럿을 표시한다.
        /// </summary>
        private void viewCaret()
        {
            if (!m_isShowCaret)
            {
                ShowCaret(this.Handle);
                m_isShowCaret = true;
            }
        }
        /// <summary>
        /// 캐럿을 숨긴다.
        /// </summary>
        private void hideCaret()
        {
            if (m_isShowCaret)
            {
                HideCaret(this.Handle);
                m_isShowCaret = false;
            }
        }

        /// <summary>
        /// 캐럿의 넓이 변경
        /// </summary>
        /// <param name="type"></param>
        private void setCaretWidth(int caretWidth)
        {
            hideCaret();
            DestroyCaret();
            CreateCaret(this.Handle, IntPtr.Zero, caretWidth, lineHeight);
            //Console.WriteLine("MakeCarret2");
            viewCaret();
        }
        protected override void OnLostFocus(EventArgs e)
        {
            hideCaret();
            DestroyCaret();
            base.OnLostFocus(e);
        }
        protected override void OnGotFocus(EventArgs e)
        {
            hideCaret();
            DestroyCaret();
            CreateCaret(this.Handle, IntPtr.Zero, 0, lineHeight);

            if (this.selectionLineNumber >= firstViewLine && this.selectionLineNumber <= lastViewLine)
            {
                if (this.CaretX_IsVisible())
                {
                    viewCaret();
                    SetCaretPos(recentlyCaretPos.X, recentlyCaretPos.Y);
                }
            }
            base.OnGotFocus(e);
        }

        #region ** Win32 API
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetCaretPos(int X, int Y);

        [DllImport("user32.dll")]
        static extern int CreateCaret(IntPtr hwnd, IntPtr hBitmap, int width, int height);

        [DllImport("user32.dll")]
        static extern int DestroyCaret();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowCaret(IntPtr handle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool HideCaret(IntPtr handle);
        #endregion

        #endregion

        #region ** Properties

        public System.Text.RegularExpressions.Regex keyword1Regex;
        public System.Text.RegularExpressions.Regex keyword2Regex;
        public System.Text.RegularExpressions.Regex keyword3Regex;
        private List<string> m_keyword1;
        private List<string> m_keyword2;
        private List<string> m_keyword3;
        private List<string> m_comment;
        private string m_wordSeparator_pattern; // 마우스로 텍스트 더블 클릭시 구분할 Regex
        private string m_methodSeparator_pattern; // 자동완성기능에 쓰일 Regex - LinesTextBox.LineInfo 와 같이 '.'도 포함
        private SolidBrush m_keyword1_ColorBrush;
        private SolidBrush m_keyword2_ColorBrush;
        private SolidBrush m_keyword3_ColorBrush;
        private SolidBrush m_comment_ColorBrush;
        private SolidBrush m_markword_ColorBrush;
        private SolidBrush m_doubleQuotes_ColorBrush; // " "(DoubleQuotes) 색상 지정

        [Browsable(true), Category("LineTextBox"), Description("좌측여백")] //좌측여백
        public int indentValue
        {
            get
            {
                return m_indentValue;
            }
            set
            {
                m_indentValue = value;
                this.Invalidate();
                this.Update();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(true), Category("LineTextBox"), Description("Font")] //폰트
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = new Font(value.Name, value.Size, GraphicsUnit.Pixel);

                this.lineHeight = (int)(this.FontHeight * 1.2);
                lastViewLine = getViewLinesCount() + firstViewLine;

                this.Invalidate();
                this.Update();
            }
        }

        [Browsable(true), Category("LineTextBox"), Description("키워드1 지정")] //키워드 지정
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        public List<string> keyword1
        {
            get { return m_keyword1; }
            set {
                m_keyword1 = value;
                settingSyntaxHighLight();
                this.Invalidate();
                this.Update();
            }
        }

        [Browsable(true), Category("LineTextBox"), Description("키워드1 색상지정")] //키워드 Color 지정
        public Color keyword1_Color
        {
            get { return m_keyword1_ColorBrush.Color; }
            set {
                m_keyword1_ColorBrush = new SolidBrush(value);
                settingSyntaxHighLight();
                this.Invalidate();
                this.Update();
            }
        }

        [Browsable(true), Category("LineTextBox"), Description("키워드2 지정")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        public List<string> keyword2
        {
            get { return m_keyword2; }
            set
            {
                m_keyword2 = value;
                settingSyntaxHighLight();
                this.Invalidate();
                this.Update();
            }
        }

        [Browsable(true), Category("LineTextBox"), Description("키워드2 색상지정")]
        public Color keyword2_Color
        {
            get { return m_keyword2_ColorBrush.Color; }
            set
            {
                m_keyword2_ColorBrush = new SolidBrush(value);
                settingSyntaxHighLight();
                this.Invalidate();
                this.Update();
            }
        }

        [Browsable(true), Category("LineTextBox"), Description("키워드3 지정")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        public List<string> keyword3
        {
            get { return m_keyword3; }
            set
            {
                m_keyword3 = value;
                settingSyntaxHighLight();
                this.Invalidate();
                this.Update();
            }
        }

        [Browsable(true), Category("LineTextBox"), Description("키워드3 색상지정")]
        public Color keyword3_Color
        {
            get { return m_keyword3_ColorBrush.Color; }
            set
            {
                m_keyword3_ColorBrush = new SolidBrush(value);
                settingSyntaxHighLight();
                this.Invalidate();
                this.Update();
            }
        }

        [Browsable(true), Category("LineTextBox"), Description("주석처리 문자열(고정)")]
        public string comment
        {
            get { return "//"; }
        }

        [Browsable(true), Category("LineTextBox"), Description("주석 색상 지정")]
        public Color comment_Color
        {
            get { return m_comment_ColorBrush.Color; }
            set
            {
                m_comment_ColorBrush = new SolidBrush(value);
                settingSyntaxHighLight();
                this.Invalidate();
                this.Update();
            }
        }

        [Browsable(true), Category("LineTextBox"), Description("라인넘버 Visible여부")]
        public bool LineNumberVisible{
            get
            {
                return m_lineNumberVisible;
            }
            set
            {
                m_lineNumberVisible = value;
                this.Invalidate();
                this.Update();
            }
        }

        [Browsable(true), Category("LineTextBox"), Description("라인넘버 Color지정")]
        public Color LineNumberColor
        {
            get
            {
                return m_lineNumberColorBrush.Color;
            }
            set
            {
                m_lineNumberColorBrush = new SolidBrush(value);
                this.Invalidate();
                this.Update();
            }
        }

        [Browsable(true), Category("LineTextBox"), Description("더블클릭시 Select를 구분지을 구분자 지정 (정규식 입력)")]
        public string wordSeperator
        {
            get
            {
                return this.m_wordSeparator_pattern;
            }
            //set
            //{
            //    this.m_wordSeparator_pattern = value;
            //}
        }

        [Browsable(true),Category("LineTextBox"), Description("현재 선택된 단어와 같은 단어 표시 기능")]
        public bool MarkWordTheSameAsSelectionWord
        {
            get
            {
                return m_markWordTheSameAsSelectionWord;
            }
            set
            {
                m_markWordTheSameAsSelectionWord = value;
            }
        }

        [Browsable(true),Category("LineTextBox"), Description("현재 선택된 단어와 같은 단어의 색상 지정")]
        public Color MarkWordColor
        {
            get
            {
                return this.m_markword_ColorBrush.Color;
            }
            set
            {
                this.m_markword_ColorBrush = new SolidBrush(value);
            }
        }

        [Browsable(true), Category("LineTextBox"), Description("\"\"(큰따옴표)에 감싸있는 텍스트 색상 지정")]
        public Color DoubleQuotesColor
        {
            get
            {
                return this.m_doubleQuotes_ColorBrush.Color;
            }
            set
            {
                this.m_doubleQuotes_ColorBrush = new SolidBrush(value);
            }
        }

        [Browsable(true), Category("LineTextBox"), Description("괄호옆에 캐럿 존재시 괄호에 강조될 색상 지정")]
        public Color BracketBackgroundBrush
        {
            get
            {
                return ((SolidBrush)m_bracketBackgroundBrush).Color;
            }
            set
            {
                m_bracketBackgroundBrush = new SolidBrush(value);
            }
        }

        [Browsable(true), Category("LineTextBox"), Description("전경색 지정")]
        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
                m_plainTextBrush = new SolidBrush(value);
            }
        }

        [Browsable(true), Category("LineTextBox"), Description("배경색 지정")]
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        [Browsable(true), Category("LineTextBox"), Description("현재 선택된 라인의 테두리 표시 색")]
        public Color selectionBorderLinePen
        {
            get
            {
                return m_selectionBorderLinePen.Color;
            }
            set
            {
                this.m_selectionBorderLinePen = new Pen(value);
                this.Invalidate();
                this.Update();
            }
        }

        [Browsable(true), Category("LineTextBox"), Description("현재 선택된 문자열의 뒷배경색")]
        public Color selectionTextBackgroundBrush
        {
            get
            {
                return ((SolidBrush)m_selectionTextBackgroundBrush).Color;
            }
            set
            {
                m_selectionTextBackgroundBrush = new SolidBrush(value);
            }
        }

        #endregion

        #region ** ProcessCmdKey
        /// <summary>
        /// 기능키(방향키 등) 눌렀을때 처리받는 이벤트
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // 효과 적용 여부
            bool isApplyEvent = false;

            #region ** Controlkey + [Alpha] Event
            if (!isApplyEvent)
            {
                isApplyEvent = true;
                if (processKeyCheck != null ? processKeyCheck(keyData) : false)
                {
                    return true;
                    //None
                }
                if (keyData == (Keys.Control | Keys.C)) //복사기능
                {
                    this.Copy();
                }
                else if (keyData == (Keys.Control | Keys.X)) //잘라내기 기능
                {
                    this.Cut();
                }
                else if (keyData == (Keys.Control | Keys.V)) //붙여넣기 기능
                {
                    //Console.WriteLine("[ProccessCmdKey] 붙여넣기 기능");
                    this.Paste();
                }
                else if (keyData == (Keys.Control | Keys.A)) //전체 선택 기능
                {
                    //Console.WriteLine("[ProccessCmdKey] 전체선택 기능");
                    this.SelectAllText();
                }
                else if (keyData == (Keys.Control | Keys.Z)) //Undo
                {
                    //Console.WriteLine("[ProcessCmdKey] Undo 기능");
                    this.Undo();
                }
                else if (keyData == (Keys.Control | Keys.Y)) //Redo
                {
                    //Console.WriteLine("[ProcessCmdKey] Redo 기능");
                    this.Redo();
                }
                else if (keyData == (Keys.Control | Keys.OemQuestion))
                {
                    //Console.WriteLine("[ProcessCmdKey] 주석 기능");
                    this.applyComment();
                }
                else if (keyData == (Keys.Alt | Keys.OemQuestion))
                {
                    //Console.WriteLine("[ProcessCmdKey] 주석해제 기능");
                    this.applyUnComment();
                }
                else
                    isApplyEvent = false;
            }
            #endregion

            if (!isProcessCmdkeySkip)
            {
                #region Up DirectionKey Event
                if (keyData == (Keys.Shift | Keys.Up) || keyData == Keys.Up)
                {
                    if (this.selectionLineNumber != 0) //현재 캐럿이 존재하는 라인이 보여지는 화면의 최상단이 아닐때
                    {
                        #region UpKey EFFECT

                        this.selectionLineNumber--;
                        string lineText = this.lineInfo[this.selectionLineNumber + 1].Text;
                        string upLineText = this.lineInfo[this.selectionLineNumber].Text;

                        //현재 줄의 firstViewIndex~selectionStartIndex의 넓이를 구하고 그 길이만큼의 위치에 존재하는 윗줄의 Index를 구한다.

                        bool selectionStartIndex_find = false;
                        float tmpWidth = 0;
                        float fullWidth1 = indentValue;
                        float fullWidth2 = indentValue;
                        Graphics g = this.CreateGraphics();
                        for (int i = firstViewIndex; i <= this.selectionStartIndex; i++)
                        {
                            tmpWidth = calculateStringWidth(g, this.Font, lineText, i);
                            fullWidth1 += tmpWidth;
                        }
                        for (int i = firstViewIndex; i <= upLineText.Length; i++)
                        {
                            fullWidth2 += tmpWidth;

                            tmpWidth = calculateStringWidth(g, this.Font, upLineText, i);
                            if (fullWidth1 <= fullWidth2)
                            {
                                this.selectionStartIndex = i;
                                selectionStartIndex_find = true;
                                break;
                            }
                        }
                        g.Dispose();

                        if (!selectionStartIndex_find)
                        {
                            this.selectionStartIndex = upLineText.Length; // 이동된 라인의 텍스트의 길이만큼으로 이동
                            CaretIsGoEnd();
                        }

                        if ((this.selectionLineNumber + 1) == firstViewLine) //현재 선택된 라인이 최상단일경우
                            firstViewLine--;

                        #endregion

                        switch (keyData)
                        {
                            case Keys.Shift | Keys.Up:
                                selectionLengthCalculate();
                                break;
                            case Keys.Up:
                                selectionLengthSetZero();
                                break;
                        }
                    }
                    isApplyEvent = true;
                }
                #endregion

                #region Down DirectionKey Event
                else if (keyData == (Keys.Shift | Keys.Down) || keyData == Keys.Down)
                {
                    if (this.selectionLineNumber != this.lineInfo.Count - 1) //현재 캐럿이 존재하는 라인이 보여지는 화면의 최하단이 아닐때
                    {
                        #region DownKey Effect

                        this.selectionLineNumber++;
                        string lineText = this.lineInfo[this.selectionLineNumber - 1].Text;
                        string downLineText = this.lineInfo[this.selectionLineNumber].Text;

                        //현재 줄의 firstViewIndex~selectionStartIndex의 넓이를 구하고 그 길이만큼의 위치에 존재하는 아랫줄의 Index를 구한다.
                        float tmpWidth = 0;
                        float fullWidth1 = indentValue;
                        float fullWidth2 = indentValue;
                        bool selectionStartIndex_find = false;
                        Graphics g = this.CreateGraphics();
                        for (int i = firstViewIndex; i <= this.selectionStartIndex; i++)
                        {
                            tmpWidth = calculateStringWidth(g, this.Font, lineText, i);
                            fullWidth1 += tmpWidth;
                        }
                        for (int i = firstViewIndex; i <= downLineText.Length; i++)
                        {
                            fullWidth2 += tmpWidth;

                            tmpWidth = calculateStringWidth(g, this.Font, downLineText, i);
                            if (fullWidth1 <= fullWidth2)
                            {
                                this.selectionStartIndex = i;
                                selectionStartIndex_find = true;
                                break;
                            }
                        }
                        g.Dispose();
                        if (!selectionStartIndex_find)
                        {
                            this.selectionStartIndex = downLineText.Length;
                            CaretIsGoEnd();
                        }

                        if (Math.Abs((this.selectionLineNumber - 1) - lastViewLine) <= 1) //현재 선택된 라인이 보이는 텍스트에서 최하단일경우
                            firstViewLine++;

                        #endregion

                        switch (keyData)
                        {
                            case Keys.Shift | Keys.Down:
                                selectionLengthCalculate();
                                break;
                            case Keys.Down:
                                selectionLengthSetZero();
                                break;
                        }
                    }
                    isApplyEvent = true;
                }
                #endregion

                #region Left DirectionKey Event
                if ((keyData == (Keys.Control | Keys.Shift | Keys.Left)) || (keyData == (Keys.Control | Keys.Left)))
                {
                    bool requiredInvalidate = false;

                    #region LeftKey Effect
                    if (this.selectionStartIndex != 0) //맨 좌측에 캐럿이 존재하지 않을때, 라인변환 필요 없을때
                    {
                        string lineText = this.lineInfo[this.selectionLineNumber].Text;
                        int[] startEndIndex = RegexData.Instance.getWordStartEndIndex(lineText, this.selectionStartIndex, m_wordSeparator_pattern);
                        int wordStartIndex = getCaretNearStringStartEndIndex()[0];
                        if (this.selectionStartIndex == wordStartIndex)
                        {
                            int whiteSpaceLeafLength = lineText.Substring(0, wordStartIndex).TrimEnd().Length;
                            if (this.selectionStartIndex != whiteSpaceLeafLength)
                                this.selectionStartIndex = whiteSpaceLeafLength;
                            else
                                this.selectionStartIndex -= 1;
                        }
                        else
                        {
                            this.selectionStartIndex = wordStartIndex;
                        }
                        if (!CaretX_IsVisible())
                            this.firstViewIndex = this.selectionStartIndex;
                        requiredInvalidate = true;
                    }
                    else if (this.selectionLineNumber > 0)
                    {
                        if (this.selectionLineNumber != this.firstViewLine) // 라인 한줄 위로 이동
                        {
                            this.selectionLineNumber--;
                            this.selectionStartIndex = this.lineInfo[this.selectionLineNumber].Text.Length;
                            CaretIsGoEnd();
                            requiredInvalidate = true;
                        }
                        else if (this.firstViewLine > 0) //라인 한줄 위로 이동 + firsViewLine 한줄 위로 이동
                        {
                            this.selectionLineNumber--;
                            this.selectionStartIndex = this.lineInfo[this.selectionLineNumber].Text.Length;
                            this.firstViewLine--;
                            CaretIsGoEnd();
                            requiredInvalidate = true;
                        }
                    }
                    #endregion

                    if (requiredInvalidate)
                    {
                        switch (keyData)
                        {
                            case (Keys.Control | Keys.Shift | Keys.Left):
                                selectionLengthCalculate();
                                break;
                            case (Keys.Control | Keys.Left):
                                selectionLengthSetZero();
                                break;
                        }
                    }
                    else
                    {
                        //Caret이 0,0에 있을때
                        //Console.Write("?");
                    }
                    isApplyEvent = true;
                }
                else if (keyData == (Keys.Shift | Keys.Left) || keyData == Keys.Left)
                {
                    bool requiredInvalidate = false;

                    #region LeftKey Effect
                    if (this.selectionStartIndex != 0) //맨 좌측에 캐럿이 존재하지 않을때, 라인변환 필요 없을때
                    {
                        this.selectionStartIndex--;
                        if (!CaretX_IsVisible())
                            this.firstViewIndex = this.selectionStartIndex;
                        //CaretIsGoLeft();
                        CaretIsGoEnd();
                        requiredInvalidate = true;
                    }
                    else if (this.selectionLineNumber != 0)
                    {
                        if (this.selectionLineNumber != this.firstViewLine) // 라인 한줄 위로 이동
                        {
                            this.selectionLineNumber--;
                            this.selectionStartIndex = this.lineInfo[this.selectionLineNumber].Text.Length;
                            CaretIsGoEnd();
                            requiredInvalidate = true;
                        }
                        else if (this.firstViewLine > 0) //라인 한줄 위로 이동 + firsViewLine 한줄 위로 이동
                        {
                            this.selectionLineNumber--;
                            this.selectionStartIndex = this.lineInfo[this.selectionLineNumber].Text.Length;
                            this.firstViewLine--;
                            CaretIsGoEnd();
                            requiredInvalidate = true;
                        }
                    }
                    #endregion

                    if (requiredInvalidate)
                    {
                        switch (keyData)
                        {
                            case Keys.Shift | Keys.Left:
                                selectionLengthCalculate();
                                break;
                            case Keys.Left:
                                selectionLengthSetZero();
                                break;
                        }
                    }
                    else
                    {
                        //Caret이 0,0에 있을때
                        //Console.Write("?");
                    }
                    isApplyEvent = true;
                }
                #endregion

                #region Right DirectionKey Event
                if (keyData == (Keys.Shift | Keys.Control | Keys.Right) || keyData == (Keys.Control | Keys.Right))
                {
                    string lineText = this.lineInfo[this.selectionLineNumber].Text;
                    if (this.selectionStartIndex < lineText.Length)
                    {
                        int wordEndIndex = this.getCaretNearStringStartEndIndex()[1];
                        if (this.selectionStartIndex == wordEndIndex)
                        {
                            int whiteSpaceLeapLength = lineText.Substring(wordEndIndex).Length - lineText.Substring(wordEndIndex).TrimStart().Length;
                            if (whiteSpaceLeapLength != 0)
                                this.selectionStartIndex = wordEndIndex + lineText.Substring(wordEndIndex).Length - lineText.Substring(wordEndIndex).TrimStart().Length;
                            else
                                this.selectionStartIndex++;
                        }
                        else
                        {
                            this.selectionStartIndex = wordEndIndex;
                        }
                        CaretIsGoRight();
                        CaretIsGoEnd();
                    }
                    else
                    {
                        int lastViewLine = this.lastViewLine - 1;

                        //라인 넘어갈때
                        if (this.selectionLineNumber < (this.lineInfo.Count - 1))
                        {
                            this.firstViewIndex = 0;
                            if (this.selectionLineNumber != lastViewLine)
                            {
                                this.selectionLineNumber++;
                                this.selectionStartIndex = 0;
                            }
                            else
                            {
                                this.selectionLineNumber++;
                                this.selectionStartIndex = 0;
                                this.firstViewLine++;
                            }
                        }
                        else
                            CaretIsGoEnd();
                    }
                    switch (keyData)
                    {
                        case Keys.Shift | Keys.Control | Keys.Right:
                            selectionLengthCalculate();
                            break;
                        case Keys.Control | Keys.Right:
                            selectionLengthSetZero();
                            break;
                    }
                    isApplyEvent = true;
                }
                else if (keyData == (Keys.Shift | Keys.Right) || keyData == Keys.Right)
                {
                    #region RightKey Effect
                    if (this.selectionStartIndex != this.lineInfo[this.selectionLineNumber].Text.Length) //라인의 맨 우측에 캐럿이 존재하지 않을때(즉 화면 새로 그릴 필요 없을때)
                    {
                        this.selectionStartIndex++;
                        CaretIsGoRight();
                        CaretIsGoEnd();
                    }
                    else
                    {
                        int lastViewLine = this.lastViewLine - 1;

                        //라인 넘어갈때
                        if (this.selectionLineNumber < (this.lineInfo.Count - 1))
                        {
                            this.firstViewIndex = 0;
                            if (this.selectionLineNumber != lastViewLine)
                            {
                                this.selectionLineNumber++;
                                this.selectionStartIndex = 0;
                            }
                            else
                            {
                                this.selectionLineNumber++;
                                this.selectionStartIndex = 0;
                                this.firstViewLine++;
                            }
                        }
                        else
                            CaretIsGoEnd();
                    }

                    #endregion

                    switch (keyData)
                    {
                        case Keys.Shift | Keys.Right:
                            selectionLengthCalculate();
                            break;
                        case Keys.Right:
                            selectionLengthSetZero();
                            break;
                    }
                    isApplyEvent = true;
                }
                #endregion

                #region ** HomeKey Event
                if (keyData == (Keys.Shift | Keys.Control | Keys.Home) || keyData == (Keys.Control | Keys.Home))
                {
                    CaretY_IsGoCurrentVerticalView(false);
                    this.firstViewLine = 0;
                    this.firstViewIndex = 0;
                    this.selectionStartIndex = 0;
                    this.selectionLineNumber = 0;
                    switch (keyData)
                    {
                        case Keys.Control | Keys.Home:
                            this.selectionLengthSetZero();
                            break;
                        case Keys.Control | Keys.Shift | Keys.Home:
                            this.selectionLengthCalculate();
                            break;
                    }
                    isApplyEvent = true;
                }
                else if (keyData == (Keys.Shift | Keys.Home) || keyData == (Keys.Home))
                {
                    CaretY_IsGoCurrentVerticalView(false);
                    #region HOME키 효과

                    string lineText = this.lineInfo[this.selectionLineNumber].Text;
                    int whiteSpaceIndex = lineText.Length - lineText.TrimStart(new char[]{' ','\t'}).Length;

                    //현재 라인의 첫번째 글자의 Index에 캐럿이 존재할 경우
                    if (this.selectionStartIndex == whiteSpaceIndex)
                    {
                        //현재 라인의 index 0으로 캐럿 이동
                        this.firstViewIndex = 0;
                        this.selectionStartIndex = 0;
                    }
                    else
                    {
                        //현재 라인의 공백을 재외한 첫 문자의 index로 캐럿 이동
                        #region 캐럿 이동

                        this.selectionStartIndex = whiteSpaceIndex;
                        float tmpFullWidth = indentValue;
                        float tmpWidth = 0;
                        if (!CaretX_IsVisible())
                        {
                            Graphics g = this.CreateGraphics();

                            for (int i = 0; i < whiteSpaceIndex; i++)
                            {
                                tmpWidth = this.calculateStringWidth(g, this.Font, lineText, i);
                                tmpFullWidth += tmpWidth;
                            }

                            if ((this.Width / 3) * 2 > tmpFullWidth)
                                this.firstViewIndex = 0;
                            else
                            {
                                if (this.selectionStartIndex >= 3)
                                    this.firstViewIndex = this.selectionStartIndex - 3;
                                else
                                    this.firstViewIndex = 0;
                            }
                            g.Dispose();
                        }
                        #endregion
                    }
                    #endregion

                    switch (keyData)
                    {
                        case Keys.Shift | Keys.Home:
                            this.selectionLengthCalculate();
                            break;
                        case Keys.Home:
                            this.selectionLengthSetZero();
                            break;
                    }
                    isApplyEvent = true;
                }
                #endregion

                #region ** EndKey Event
                if (keyData == (Keys.Shift | Keys.Control | Keys.End) || keyData == (Keys.Control | Keys.End))
                {
                    //firstView이동 메서드 실행하기

                    this.selectionLineNumber = this.lineInfo.Count - 1;
                    this.selectionStartIndex = this.lineInfo[this.lineInfo.Count - 1].Text.Length;

                    CaretIsLocateScreenByFraction(1, 1);
                    CaretIsGoEnd();
                    switch (keyData)
                    {
                        case Keys.Shift | Keys.Control | Keys.End:
                            this.selectionLengthCalculate();
                            break;
                        case Keys.Control | Keys.End:
                            this.selectionLengthSetZero();
                            break;
                    }

                    isApplyEvent = true;
                }
                else if (keyData == (Keys.Shift | Keys.End) || keyData == Keys.End)
                {
                    this.selectionStartIndex = this.lineInfo[selectionLineNumber].Text.Length;
                    CaretIsGoEnd();
                    switch (keyData)
                    {
                        case Keys.Shift | Keys.End:
                            this.selectionLengthCalculate();
                            break;
                        case Keys.End:
                            this.selectionLengthSetZero();
                            break;
                    }
                    isApplyEvent = true;
                }
                #endregion

            }

            #region DeleteKey Event
            if (keyData == Keys.Delete || keyData == (Keys.Shift | Keys.Delete))
            {
                if (!(this.selectionLineNumber == this.lineInfo.Count - 1 && this.selectionStartIndex == this.lineInfo[this.lineInfo.Count - 1].Text.Length))
                    insertUndoData();
                else if (this.m_selectionLength > 0)
                    insertUndoData();

                #region Delete키 효과
                if (m_selectionLength == 0)
                {
                    string lineText = this.lineInfo[this.selectionLineNumber].Text;
                    if (this.selectionStartIndex == lineText.Length ? this.selectionLineNumber != this.lineInfo.Count - 1 : false)
                    {
                        string nextLineText = this.lineInfo[this.selectionLineNumber + 1].Text;
                        this.lineInfo[this.selectionLineNumber].Text = lineText + nextLineText;
                        this.lineInfo.RemoveAt(this.selectionLineNumber + 1);
                    }
                    else if (this.selectionStartIndex != lineText.Length)
                    {
                        string prevText = lineText.Substring(0, this.selectionStartIndex);
                        string nextText = lineText.Substring(this.selectionStartIndex + 1, (lineText.Length - prevText.Length) - 1);
                        this.lineInfo[this.selectionLineNumber].Text = prevText + nextText;
                    }
                }
                else
                {
                    selectedTextRemove(false);
                }
                #endregion
                isApplyEvent = true;

                call_NormalTextChanged();
            }
            #endregion

            #region TabKey Event
            if (keyData == (Keys.Tab | Keys.Shift))  //내어쓰기
            {
                applyOutdent();
                isApplyEvent = true;
            }
            else if (keyData == Keys.Tab) //들여쓰기
            {
                applyIndent();
                isApplyEvent = true;
            }
            #endregion

            #region Enter Event
            if (keyData == Keys.Enter)
            {
                insertUndoData();
                selectedTextRemove(false);

                #region ** Enter효과
                bool naturalEnter = false; //자연스러운 엔터. 엔터시 firstViewLine을 한칸만 ++
                if (firstViewLine <= this.selectionLineNumber && lastViewLine >= this.selectionLineNumber) // 현재 보이는 Text안에 Caret이 존재할때만
                    naturalEnter = true;

                string lineText = lineInfo[this.selectionLineNumber].Text;
                string prevText = lineText.Substring(0, this.selectionStartIndex);
                string nextText = lineText.Substring(this.selectionStartIndex, lineText.Length - prevText.Length);
                lineInfo[this.selectionLineNumber].Text = prevText;
                //lineInfo[this.selectionLineNumber].SyntaxHighLightUpdate();

                Lines tmpLine = new Lines();
                LineSetRegex(tmpLine);
                tmpLine.Text = nextText;
                lineInfo.Insert(this.selectionLineNumber + 1, tmpLine);
                this.selectionLineNumber++;
                //this.selectionStartIndex = 0; //들여쓰기로 인한 selectionStartIndex 조정


                //현재 캐럿이 보이는 위치가 아니면 시선을 Caret이 존재하는곳으로 이동
                if (!(firstViewLine <= this.selectionLineNumber && lastViewLine - 1 >= this.selectionLineNumber))
                {
                    if (naturalEnter)
                        firstViewLine++;
                    else
                        firstViewLine = this.selectionLineNumber - 1;
                }
                firstViewIndex = 0;

                //들여쓰기
                string prevLineText = this.lineInfo[this.selectionLineNumber - 1].Text;
                int whiteSpaceCount = prevLineText.Length - prevLineText.TrimStart(new char[]{' ','\t'}).Length;
                StringBuilder appendWhiteSpaceText = new StringBuilder();
                for (int i = 0; i < whiteSpaceCount; i++)
                    appendWhiteSpaceText.Append(prevLineText[i]);
                lineInfo[this.selectionLineNumber].Text = appendWhiteSpaceText.ToString() + lineInfo[this.selectionLineNumber].Text;
                this.selectionStartIndex = whiteSpaceCount;

                #endregion

                this.selectionEndLineNumber = this.selectionLineNumber;
                this.selectionEndIndex = this.selectionStartIndex;

                isApplyEvent = true;
                call_descriptionVisible(false);
                //call_NormalTextChanged();
            }
            #endregion

            #region PageUp & PageDown Event
            else if (keyData == (Keys.PageUp | Keys.Shift) || keyData == Keys.PageUp)
            {
                #region PageUp Effect
                int viewLineCount = this.getViewLinesCount();
                viewLineCount--;
                int nextSelectionLineNumber = Math.Max(0, this.selectionLineNumber - viewLineCount); //텍스트박스에 보이는 줄의 개수 -1 이 PageUp시 이동될 줄의 개수

                string lineText = this.lineInfo[this.selectionLineNumber].Text;
                string nextLineText = this.lineInfo[nextSelectionLineNumber].Text;

                #region X값 측정
                bool selectionStartIndex_find = false;
                float tmpWidth = 0;
                float fullWidth1 = indentValue;
                float fullWidth2 = indentValue;
                Graphics g = this.CreateGraphics();
                for (int i = firstViewIndex; i <= this.selectionStartIndex; i++)
                {
                    tmpWidth = calculateStringWidth(g, this.Font, lineText, i);
                    fullWidth1 += tmpWidth;
                }
                for (int i = firstViewIndex; i <= nextLineText.Length; i++)
                {
                    fullWidth2 += tmpWidth;

                    tmpWidth = calculateStringWidth(g, this.Font, nextLineText, i);
                    if (fullWidth1 <= fullWidth2)
                    {
                        this.selectionStartIndex = i;
                        selectionStartIndex_find = true;
                        break;
                    }
                }
                g.Dispose();
                #endregion

                this.selectionLineNumber = nextSelectionLineNumber;
                //this.firstViewLine = this.selectionLineNumber;
                CaretIsLocateScreenByFraction(1, 2);
                // X값 측정시 이벤트 실행전 selectionStartIndex가 이벤트 실행 후 선택될 라인의 텍스트 Legnth보다 큰 관계로 Caret생성할 Index가 존재 하지 않을경우
                if (!selectionStartIndex_find)
                {
                    this.selectionStartIndex = nextLineText.Length; // 이동된 라인의 텍스트의 길이만큼으로 이동
                    CaretIsGoEnd();
                }
                #endregion

                switch (keyData)
                {
                    case Keys.PageUp | Keys.Shift:
                        selectionLengthCalculate();
                        break;
                    case Keys.PageUp:
                        selectionLengthSetZero();
                        break;
                }
                isApplyEvent = true;
            }
            else if (keyData == (Keys.PageDown | Keys.Shift) || keyData == Keys.PageDown)
            {
                #region PageDown Effect
                int viewLineCount = this.getViewLinesCount();
                viewLineCount--;
                int nextSelectionLineNumber = Math.Min(this.lineInfo.Count-1 , this.selectionLineNumber + viewLineCount); //텍스트박스에 보이는 줄의 개수 -1 이 PageUp시 이동될 줄의 개수

                string lineText = this.lineInfo[this.selectionLineNumber].Text;
                string nextLineText = this.lineInfo[nextSelectionLineNumber].Text;

                #region X값 측정
                bool selectionStartIndex_find = false;
                float tmpWidth = 0;
                float fullWidth1 = indentValue;
                float fullWidth2 = indentValue;
                Graphics g = this.CreateGraphics();
                for (int i = firstViewIndex; i <= this.selectionStartIndex; i++)
                {
                    tmpWidth = calculateStringWidth(g, this.Font, lineText, i);
                    fullWidth1 += tmpWidth;
                }
                for (int i = firstViewIndex; i <= nextLineText.Length; i++)
                {
                    fullWidth2 += tmpWidth;

                    tmpWidth = calculateStringWidth(g, this.Font, nextLineText, i);
                    if (fullWidth1 <= fullWidth2)
                    {
                        this.selectionStartIndex = i;
                        selectionStartIndex_find = true;
                        break;
                    }
                }
                g.Dispose();
                #endregion

                this.selectionLineNumber = nextSelectionLineNumber;
                CaretIsLocateScreenByFraction(1, 2); //현재 Caret이 있는 줄이 텍스트의 맨 마지막으로 보이는 줄이 되도록 지정
                // X값 측정시 이벤트 실행전 selectionStartIndex가 이벤트 실행 후 선택될 라인의 텍스트 Legnth보다 큰 관계로 Caret생성할 Index가 존재 하지 않을경우
                if (!selectionStartIndex_find)
                {
                    this.selectionStartIndex = nextLineText.Length; // 이동된 라인의 텍스트의 길이만큼으로 이동
                    CaretIsGoEnd();
                }
                #endregion
                switch (keyData)
                {
                    case Keys.PageDown | Keys.Shift:
                        selectionLengthCalculate();
                        break;
                    case Keys.PageDown:
                        selectionLengthSetZero();
                        break;
                }
                isApplyEvent = true;
            }
            #endregion

            if (keyData == Keys.Insert)
            {
                setScrollByLineAndIndex(this.selectionLineNumber, this.selectionStartIndex);
                this.Invalidate();
                this.Update();
                return true;
            }

            if (isApplyEvent)
            {
                CaretY_IsGoCurrentVerticalView(false);
                this.Invalidate();
                this.Update();
                if (isProcessCmdkeySkip)
                {
                    call_NormalTextChanged();
                    return base.ProcessCmdKey(ref msg, keyData);
                }
                else
                    return true;
            }
            else //위의 이벤트들 중 실행된 이벤트가 없을 경우
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        #endregion

        #region ** WndProc

        private const int WM_KEYDOWN = 0x100;
        private const int WM_MOUSEWHEEL = 0x020A;

        private const int WM_CHAR = 0x102; // 영문입력(1byte) || 한글조합 완성(2byte)시 byte만큼 발생. 한글은 1byte씩 두번 찍히므로 WM_CHAR로 한글을 완성할 수 없다.
        private const int WM_IME_COMPOSITION = 0x10F; //한글 조합중일때 발생
        private const int WM_IME_CHAR = 0x286; //완성된 한글에 대한 ASCII값을 반환한다. (WM_IME_COMPOSITION메시지를 사용하기 때문에 별로 사용하지 않음)
        private const int WM_IME_KEYDOWN = 0x290; // 한글 입력중 한글키가 아닌경우 그 키값을 반환한다. WM_IME_ENDCOMPOSITION 메세지 발생 후에 발생됨.
        private const int WM_IME_NOTIFY = 0x282; // IME 모드 변경시 이벤트 발생
        private const int GCS_RESULTSTR = 0x800;
        private const int GCS_COMPSTR = 0x008;
        const int WM_IME_SETCONTEXT = 0x0281;
        public enum ImmAssociateContextExFlags : uint
        {
            IACE_CHILDREN = 0x0001,
            IACE_DEFUALT = 0x0010,
            IACE_IGNORENOCONTEXT = 0x0020
        }

        bool isComposit = false; //글자 조합중일때 true
        string compositText = string.Empty; //글자 조합중일때 조합중인 텍스트 출력
        int compositTextIndex = -1; //글자 조합중일때 조합 자모음을 지웠다 다시 쓰는경우 index번호 저장해서 해당 index값에 존재하는 character만 변경하기 위한 저장소

        #region ** IME extern Method

        public IntPtr ImeHandle = IntPtr.Zero; //메인 IMM핸들.
        int IME_CMODE_NATIVE = 0x0001;

        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetContext(IntPtr hwnd);

        [DllImport("imm32.dll")]
        private static extern int ImmReleaseContext(IntPtr hwnd, IntPtr himc);

        [DllImport("imm32.dll")]
        private static extern int ImmGetCompositionString(IntPtr himc, int dw, StringBuilder lpv, int dw2);

        [DllImport("imm32.dll")]
        private static extern int ImmGetCompositionString(IntPtr himc, int dw, int lpv, int dw2);

        [DllImport("imm32.dll")]
        private static extern bool ImmAssociateContextEx(IntPtr hWnd, IntPtr hlMC, ImmAssociateContextExFlags dwFlags);

        [DllImport("imm32.dll")]
        private static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hlMC);

        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetConversionStatus(IntPtr himc, IntPtr dwc, IntPtr dws);

        [DllImport("imm32.dll")]
        private static extern IntPtr ImmSetConversionStatus(IntPtr himc, int dwc, int dws);


        #endregion

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_MOUSEWHEEL: //마우스 휠 이벤트 발생시 - firstViewLine 값 조정하고 invalidate
                    #region WM_MOUSEWHEEL
                    if (!isProcessCmdkeySkip)
                    {
                        if (m.WParam.ToInt64() < 0) // WheelDown
                        {
                            int tmpFirstViewLine = firstViewLine + 3;
                            firstViewLine = Math.Min(tmpFirstViewLine, this.lineInfo.Count - 1);
                            this.Invalidate();
                            //if (firstViewLine != this.lineInfo.Count)
                            //{
                            //    firstViewLine++;
                            //    this.Invalidate();
                            //}
                        }
                        else // WheelUp
                        {
                            if (firstViewLine != 0)
                            {
                                int tmpFirstViewLine = firstViewLine - 3;
                                firstViewLine = Math.Max(tmpFirstViewLine, 0);
                                this.Invalidate();
                            }
                            //if (firstViewLine != 0)
                            //{
                            //    firstViewLine--;
                            //    this.Invalidate();
                            //}
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("LineTextBox - WheelEventSkip");
                        base.WndProc(ref m);
                    }
                    #endregion
                    break;
                case WM_KEYDOWN: //현재 안씀(한글 조합시에도 KeyDown이벤트가 떨어짐. 웬만하면 쓰지 말자)
                    #region WM_KEYDOWN

                        Keys keyCode = (Keys)(int)m.WParam & Keys.KeyCode;
                        Keys modifyKey = (Keys)(int)m.WParam & Keys.Modifiers;

                    #endregion
                    break;
                case WM_CHAR: // 일반 문자 입력(IME 텍스트 조립중일때는 발생하지 않는 이벤트)
                    #region WM_CHAR
                        char c = (char)m.WParam;
                        string appendChar = string.Empty;
                        int tmpInt = m.WParam.ToInt32();

                        //IME 모드일 경우 Backspace는 Text를 한칸 뒤로 지우는것이 아닌
                        //조합중인 텍스트를 한개 지우는 이벤트가 발생되어야 하므로 이쪽에 이벤트 할당
                        // 917505는 Backspace 한번, 1074659329는 Backspace 꾹 누르고있을때 발생됨 - 141126
                        if (tmpInt == 8 && (m.LParam.ToInt32() == 917505 || m.LParam.ToInt32() == 1074659329))
                        {
                            CaretY_IsGoCurrentVerticalView(false);
                            CaretIsGoEnd();

                            if (!(this.selectionLineNumber == 0 && this.selectionStartIndex == 0))
                                insertUndoData();
                            else if (this.m_selectionLength > 0)
                                insertUndoData();

                            #region Backspace 효과
                            if (this.m_selectionLength > 0)
                            {
                                selectedTextRemove(true);
                            }
                            else if (this.selectionStartIndex == 0 && this.selectionLineNumber != 0) //현재 캐럿이 존재하는 index가 0번일경우 \r\n을 제거해야됨
                            {
                                string prevText = lineInfo[this.selectionLineNumber - 1].Text;

                                #region 세로스크롤 위치 지정
                                if (this.firstViewLine != 0 && this.firstViewLine == this.selectionLineNumber)
                                {
                                    this.firstViewLine--;
                                }
                                #endregion

                                #region 가로스크롤 이동할 위치 지정
                                float tmpWidth = 0;
                                float fullWidth = this.indentValue;
                                float screenWidth = this.indentValue;
                                Graphics tmpGraphics = this.CreateGraphics();

                                int tmpScreenMoveIndex = 0;
                                for (int j = firstViewIndex; j <= prevText.Length; j++)
                                {
                                    tmpWidth = calculateStringWidth(tmpGraphics, this.Font, prevText, j);
                                    if (screenWidth >= this.Width)
                                    {
                                        screenWidth = 0;
                                        tmpScreenMoveIndex = j;
                                    }

                                    if (j == prevText.Length)
                                    {
                                        break;
                                    }
                                    screenWidth += tmpWidth;
                                    fullWidth += tmpWidth;
                                }
                                tmpGraphics.Dispose();

                                //가로스크롤 이동될 위치 지정
                                this.firstViewIndex = tmpScreenMoveIndex;
                                #endregion

                                string lineText = lineInfo[this.selectionLineNumber].Text;
                                lineInfo[this.selectionLineNumber - 1].Text += lineText;
                                lineInfo.RemoveAt(this.selectionLineNumber);
                                this.selectionLineNumber--;
                                this.selectionStartIndex = prevText.Length;

                                this.Invalidate();
                                this.Update();

                            }
                            else if (this.selectionStartIndex != 0)
                            {
                                #region ** 케이스2 : 현재 캐럿이 존재하는 Index가 0번이 아닐경우, Line변경 할 필요 없는 경우
                                string lineText = this.lineInfo[this.selectionLineNumber].Text;
                                string prevText = lineText.Substring(0, this.selectionStartIndex - 1);
                                string nextText = lineText.Substring(this.selectionStartIndex, lineText.Length - (prevText.Length + 1));
                                this.lineInfo[this.selectionLineNumber].Text = prevText + nextText;

                                if (firstViewIndex >= selectionStartIndex - 1) // firstViewIndex의 변경이 필요한 경우, 가로스크롤 조정해야되는 경우
                                {
                                    #region ...
                                    float tmpWidth = 0;
                                    float fullWidth = this.indentValue;
                                    Graphics tmpGraphics = this.CreateGraphics();
                                    for (int j = 0; j < lineText.Length; j++)
                                    {
                                        tmpWidth = calculateStringWidth(tmpGraphics, this.Font, lineText, j);

                                        if (j == selectionStartIndex - 1)
                                        {
                                            int scrollIndex = ((int)fullWidth / this.Width);
                                            if (scrollIndex == 0)
                                                firstViewIndex = 0;
                                            else
                                                if (((lineText.Length - j) / 3) > 10)
                                                    firstViewIndex = j - (lineText.Length - j) / 3;
                                                else
                                                    firstViewIndex -= 10;
                                            ;
                                            break;
                                        }
                                        fullWidth += tmpWidth;
                                    }
                                    tmpGraphics.Dispose();
                                    #endregion
                                }
                                selectionStartIndex--;
                                this.Invalidate();
                                this.Update();
                                #endregion
                            }

                            #endregion
                            isComposit = false;

                            this.selectionLengthSetZero();
                            call_descriptionVisible(true);
                            call_NormalTextChanged();
                        }
                        else if (tmpInt >= 32 && tmpInt <= 126)
                        {
                            appendChar = c + "";
                        }

                        if (appendChar != string.Empty)
                        {
                            CaretY_IsGoCurrentVerticalView(false);
                            CaretIsGoEnd();

                            //한줄에서 연속으로 문자를 입력한게 아니라면 UndoStack에 현재 텍스트 상태 저장
                            if (fixedSelectionLine != this.selectionLineNumber)
                            {
                                insertUndoData();
                                fixedSelectionLine = this.selectionLineNumber;
                                redoStack.Clear();
                            }

                            // 현재 select중일 경우 select된 글자들을 모두 지운다.
                            if (this.m_selectionLength != 0)
                                this.selectedTextRemove(false);
                            #region 일반 문자열 생성
                            string lineText = this.lineInfo[selectionLineNumber].Text;
                            string prevText = lineText.Substring(0, selectionStartIndex);
                            string nextText = lineText.Substring(selectionStartIndex, lineText.Length - prevText.Length);
                            this.lineInfo[selectionLineNumber].Text = prevText + c + nextText;
                            this.selectionStartIndex++;

                            CaretIsGoRight();

                            this.Invalidate();
                            this.Update();
                            #endregion


                            if (tmpInt == 41 || tmpInt == 59) // ')' or ';'
                            {
                                call_descriptionVisible(false);
                            }
                            else
                            {
                                call_descriptionVisible(true);
                            }

                            //문자 변경사항 발생시 이벤트 호출
                            call_NormalTextChanged();
                            selectionLengthSetZero();

                        }
                        #endregion
                    break;
                case WM_IME_SETCONTEXT: // 윈도우의 IME와 연결
                    #region WM_IME_SETCONTEXT
                    if (m.WParam.ToInt32() != 0)
                    {
                        //윈도우 IME 읽어오기
                        bool rc = ImmAssociateContextEx(this.Handle, ImeHandle, ImmAssociateContextExFlags.IACE_DEFUALT);
                        if (rc)
                        {
                            DefWndProc(ref m);
                        }
                    }
                    #endregion
                    break;
                case WM_IME_COMPOSITION: //문자열 조합중일때
                    #region WM_IME_COMPOSITION
                    CaretY_IsGoCurrentVerticalView(false);
                    CaretIsGoEnd();

                    if (fixedSelectionLine != this.selectionLineNumber)
                    {
                        insertUndoData();
                        fixedSelectionLine = this.selectionLineNumber;
                        redoStack.Clear();
                    }

                    // 현재 select중일 경우 select된 글자들을 모두 지운다.
                    if (this.m_selectionLength != 0)
                        this.selectedTextRemove(false);
                    selectionLengthSetZero();

                    int comp = m.LParam.ToInt32();
                    int intdwSize = 0;
                    string text = "";
                    if ((comp & GCS_RESULTSTR) > 0)  //완성 한글 받기
                    {
                        IntPtr intICHwnd = IntPtr.Zero;
                        intICHwnd = ImmGetContext(this.Handle);

                        intdwSize = ImmGetCompositionString(intICHwnd, GCS_RESULTSTR, 0, 0);
                        if (intdwSize != 0)
                        {
                            StringBuilder s = new StringBuilder(intdwSize + 1);
                            intdwSize = ImmGetCompositionString(intICHwnd, GCS_RESULTSTR, s, intdwSize);
                            text = s.ToString();
                        }
                        ImmReleaseContext(this.Handle, intICHwnd);

                        isComposit = false;
                        string lineText = this.lineInfo[this.selectionLineNumber].Text;
                        string prevText = lineText.Substring(0, this.compositTextIndex);
                        string nextText = lineText.Substring(this.compositTextIndex+1);
                        this.lineInfo[this.selectionLineNumber].Text = prevText + text[0] + nextText;
                        this.selectionStartIndex++;

                        selectionLengthSetZero();
                        CaretIsGoRight();
                        this.Invalidate();
                        this.Update();
                        setCaretWidth(0);
                    }
                    else if ((comp & GCS_COMPSTR) > 0) //미완성 글자 받기
                    {
                        IntPtr intICHwnd = IntPtr.Zero;
                        intICHwnd = ImmGetContext(this.Handle);
                        intdwSize = ImmGetCompositionString(intICHwnd, GCS_COMPSTR, 0, 0);
                        if (intdwSize != 0)
                        {
                            StringBuilder s = new StringBuilder(intdwSize);
                            intdwSize = ImmGetCompositionString(intICHwnd, GCS_COMPSTR, s, intdwSize);
                            text = s.ToString();
                        }
                        ImmReleaseContext(this.Handle, intICHwnd);
                        if (!isComposit)
                        {
                            this.compositTextIndex = this.selectionStartIndex;
                        }
                        
                            
                        string lineText = this.lineInfo[this.selectionLineNumber].Text;
                        string prevText = lineText.Substring(0, this.compositTextIndex);
                        string nextText = string.Empty;
                        if(isComposit)
                            nextText = lineText.Substring(compositTextIndex+1);
                        else
                            nextText = lineText.Substring(compositTextIndex);

                        isComposit = true;

                        if (!string.IsNullOrEmpty(text))
                        {
                            this.lineInfo[this.selectionLineNumber].Text = prevText + text[0] + nextText;
                            this.compositText = text[0] + "";
                            Graphics g = this.CreateGraphics();
                            setCaretWidth((int)calculateStringWidth(g, this.Font, text, 0));
                            CaretIsGoRight();
                            g.Dispose();
                        }
                        else //조합중이던 글자를 다 없앴을때
                        {
                            this.lineInfo[this.selectionLineNumber].Text = prevText + nextText;
                            compositText = "";
                            isComposit = false;
                            setCaretWidth(0);
                            //compositTextIndex = -1;
                        }
                        
                        //미완성 글자 입력시 조합하는화면 보여줄것
                        this.Invalidate();
                        this.Update();
                    }
                    #endregion
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /// <summary>
        /// 텍스트박스 ime 지정
        /// </summary>
        /// <param name="lang">0:영문, 1:IME_CMODE_NATIVE(한글, 해당PC 기본 ime)</param>
        public void imeSetConversion(int lang)
        {
            ImeHandle = ImmGetContext(this.Handle);
            ImmSetConversionStatus(ImeHandle, lang, 0);
            ImmReleaseContext(this.Handle, ImeHandle);
        }

        #endregion

        #region ** OnPaint
        protected override void OnPaint(PaintEventArgs e)
        {
            int viewLines = getViewLinesCount();
            lastViewLine = firstViewLine + viewLines;

            int count = 0;
            bool isCaretMustView = false;


            int horizonMaximumValue = 0;
            for (int i = 0; i < this.lineInfo.Count; i++)
                horizonMaximumValue = Math.Max(horizonMaximumValue, this.lineInfo[i].Text.Length);

            // 마우스로 드래깅한 부분 표시
            if (this.m_selectionLength > 0)
            {
                #region ** ...
                int minLineNumber = Math.Min(selectionLineNumber, selectionEndLineNumber);
                int maxLineNumber = Math.Max(selectionLineNumber, selectionEndLineNumber);
                int minLineStartIndex = minLineNumber == selectionLineNumber ? selectionStartIndex : selectionEndIndex;
                int maxLineEndIndex = maxLineNumber == selectionLineNumber ? selectionStartIndex : selectionEndIndex;
                string lineText = string.Empty;


                if (minLineNumber != maxLineNumber)
                {
                    for (int line_i = minLineNumber; line_i <= maxLineNumber; line_i++)
                    {
                        if (!(firstViewLine <= line_i && lastViewLine >= line_i))
                            continue;

                        lineText = lineInfo[line_i].Text;
                        float tmpCharWidth = 0;
                        float tmpFullWidth = 0;
                        float fillStartX = indentValue;
                        float fillStartY = (line_i - firstViewLine) * lineHeight;
                        float fillWidth = 0;
                        bool DoDrawing = false;
                        if (line_i == minLineNumber)
                        {
                            for (int index_i = 0; index_i <= lineText.Length; index_i++)
                            {
                                if (index_i < firstViewIndex)
                                    continue;
                                tmpCharWidth = this.calculateStringWidth(e.Graphics, this.Font, lineText, index_i);

                                if (index_i == minLineStartIndex)
                                {
                                    fillStartX += tmpFullWidth;
                                    tmpFullWidth = 0;
                                }
                                if (index_i == lineText.Length)
                                {
                                    fillWidth = tmpFullWidth;
                                }
                                tmpFullWidth += tmpCharWidth;
                                DoDrawing = true;
                            }
                        }
                        else if (line_i == maxLineNumber)
                        {
                            for (int index_i = 0; index_i <= lineText.Length; index_i++)
                            {
                                if (index_i < firstViewIndex)
                                    continue;
                                tmpCharWidth = this.calculateStringWidth(e.Graphics, this.Font, lineText, index_i);

                                if (index_i == maxLineEndIndex)
                                {
                                    fillWidth = tmpFullWidth;
                                    DoDrawing = true;
                                    break;
                                }
                                tmpFullWidth += tmpCharWidth;
                            }
                        }
                        else
                        {
                            for (int index_i = 0; index_i <= lineText.Length; index_i++)
                            {
                                if (index_i < firstViewIndex)
                                    continue;
                                tmpCharWidth = this.calculateStringWidth(e.Graphics, this.Font, lineText, index_i);
                                tmpFullWidth += tmpCharWidth;
                                DoDrawing = true;
                            }
                            fillWidth = tmpFullWidth;
                        }

                        if (DoDrawing)
                        {
                            //Rectangle tmpRect = new Rectangle((int)(fillStartX + marginX), (int)fillStartY, (int)(fillWidth + marginX), (int)this.lineHeight);
                            e.Graphics.FillRectangle(this.m_selectionTextBackgroundBrush, fillStartX + marginX, fillStartY, fillWidth + marginX, this.lineHeight);
                        }
                    }
                }
                else
                {
                    if (firstViewLine <= selectionLineNumber && lastViewLine >= selectionLineNumber)
                    {
                        float tmpCharWidth = 0;
                        float tmpFullWidth = 0;
                        float fillStartX = indentValue;
                        float fillStartY = (selectionLineNumber - firstViewLine) * lineHeight;
                        float fillWidth = 0;
                        lineText = this.lineInfo[selectionLineNumber].Text;
                        minLineStartIndex = Math.Min(selectionStartIndex, selectionEndIndex);
                        maxLineEndIndex = Math.Max(selectionStartIndex, selectionEndIndex);
                        bool DoDrawing = false;
                        for (int index_i = 0; index_i <= lineText.Length; index_i++)
                        {
                            if (index_i < firstViewIndex)
                                continue;

                            tmpCharWidth = this.calculateStringWidth(e.Graphics, this.Font, lineText, index_i);
                            if (index_i == minLineStartIndex)
                            {
                                fillStartX += tmpFullWidth;
                                tmpFullWidth = 0;
                            }
                            if (index_i == maxLineEndIndex)
                            {
                                fillWidth = tmpFullWidth;
                            }
                            tmpFullWidth += tmpCharWidth;
                            DoDrawing = true;
                        }
                        if (DoDrawing)
                        {
                            //Console.WriteLine("[OnPaint]한줄드래그 - " + minLineStartIndex + ":" + maxLineEndIndex);
                            e.Graphics.FillRectangle(this.m_selectionTextBackgroundBrush, fillStartX + marginX, fillStartY, fillWidth + marginX, this.lineHeight);
                        }
                    }
                }

                #endregion
            }
            else // Caret의 옆에 괄호가 존재하고 해당 괄호와 매칭되는 괄호를 찾는다.
            {
                // 괄호 카운트 해야될듯. ex) 1[ 2[ 3]  존재시 2에서 클릭했을때 3을 표시하고 1을 클릭시엔 표시 안하기
                char[] openBracketArray = new char[] { '[', '{', '(' };
                char[] closeBracketArray = new char[] { ']', '}', ')' };

                string lineText = this.lineInfo[this.selectionLineNumber].Text;

                #region ** 캐럿 우측에 OpenBracket 존재 시 대칭 CloseBracket검사
                if (lineText.Length > this.selectionStartIndex) // 0 ~ 끝-1 Index OpenBracket검사 (캐럿이 존재하는 곳의 우측값을 검사하므로 우측에 문자가 없으면 안됨)
                {
                    int bracketFind = 0;
                    //현재 캐럿이 존재하는 부분의 텍스트가 일반텍스트일 경우
                    if (this.lineInfo[this.selectionLineNumber].m_textType[this.selectionStartIndex] == Lines.TextType.PlainText)
                        for (int bracketNum = 0; bracketNum < openBracketArray.Length; bracketNum++)
                        {
                            if (lineText[this.selectionStartIndex].Equals(openBracketArray[bracketNum])) //OpenBracket을 찾은 경우
                            {
                                char openBracket = openBracketArray[bracketNum]; //찾은 OpenBracket
                                char closeBracket = closeBracketArray[bracketNum]; //찾을 CloseBracket
                                float openBracketWidth = calculateStringWidth(e.Graphics, this.Font, openBracket + ""); //찾은 OpenBracketWidth
                                float closeBracketWidth = calculateStringWidth(e.Graphics, this.Font, closeBracket + ""); //찾을 CloseBracketWidth

                                float[] reservedCloseBracketDraw = new float[0]; //
                                float[] reservedOpenBracketDraw = new float[0]; // OpenBracket과 대칭되는 CloseBracket을 찾았을 경우에만 OpenBracket표시(위치값저장)

                                float tmpWidth = 0;

                                bool isExistMachingCaret = false; //대칭 괄호 존재시 true

                                // 캐럿 존재하는 라인부터 마지막 라인까지 Bracket검색
                                for (int line_i = this.selectionLineNumber; line_i <= this.lineInfo.Count - 1; line_i++)
                                {
                                    if (isExistMachingCaret)
                                        break;

                                    string tmpLineText = this.lineInfo[line_i].Text;
                                    for (int line_charIndex = 0; line_charIndex < tmpLineText.Length; line_charIndex++)
                                    {
                                        if (line_i == this.selectionLineNumber ? this.selectionStartIndex > line_charIndex : false)
                                            continue;
                                        if (this.lineInfo[line_i].m_textType[line_charIndex].Equals(Lines.TextType.PlainText)) //텍스트가 평문일 경우에만
                                        {
                                            if (tmpLineText[line_charIndex].Equals(openBracket)) //Case1. OpenBracket 발견 시 Count++
                                            {
                                                if (line_i == this.selectionLineNumber ? this.selectionStartIndex == line_charIndex : false) //시작 OpenBracket 색칠
                                                {
                                                    if (firstViewLine <= line_i && line_i <= lastViewLine) // 현재 보일경우
                                                    {
                                                        float line_bracketX = this.indentValue;
                                                        for (int tmpj = firstViewIndex; tmpj <= this.selectionStartIndex; tmpj++)
                                                        {
                                                            if (tmpj == this.selectionStartIndex) //첫번째 발견한 OpenBracket인 경우, 즉 현재 캐럿 바로 옆에있는 OpenBracket일 경우
                                                            {
                                                                reservedCloseBracketDraw = new float[] { line_bracketX + marginX, (line_i - firstViewLine) * lineHeight, openBracketWidth + marginX, lineHeight };
                                                                //e.Graphics.FillRectangle(m_bracketBackgroundBrush, line_bracketX + marginX, (line_i - firstViewLine) * lineHeight, openBracketWidth + marginX, lineHeight);
                                                                break;
                                                            }
                                                            tmpWidth = this.calculateStringWidth(e.Graphics, this.Font, tmpLineText, tmpj);

                                                            line_bracketX += tmpWidth;
                                                            if (line_bracketX >= this.Width)
                                                                break;
                                                        }
                                                    }
                                                }
                                                bracketFind++;
                                            }
                                            else if (tmpLineText[line_charIndex].Equals(closeBracket)) //Case2. CloseBracket 발견시
                                            {
                                                bracketFind--;
                                                if (bracketFind == 0)
                                                {
                                                    if (line_charIndex >= firstViewIndex)
                                                    {
                                                        float line_bracketX = this.indentValue;
                                                        for (int tmpj = firstViewIndex; tmpj <= line_charIndex; tmpj++)
                                                        {
                                                            if (tmpj == line_charIndex)
                                                            {
                                                                if (firstViewLine <= line_i && line_i <= lastViewLine) //현재 세로기준으로 화면에 보일경우
                                                                {
                                                                    if (line_bracketX < this.Width) //현재 가로기준으로 화면에 보일경우
                                                                    {
                                                                        e.Graphics.FillRectangle(m_bracketBackgroundBrush, line_bracketX + marginX, (line_i - firstViewLine) * lineHeight, closeBracketWidth + marginX, lineHeight);
                                                                    }
                                                                }
                                                                break;
                                                            }
                                                            tmpWidth = this.calculateStringWidth(e.Graphics, this.Font, tmpLineText, tmpj);

                                                            line_bracketX += tmpWidth;
                                                        }
                                                        isExistMachingCaret = true;
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (reservedCloseBracketDraw.Length != 0 && isExistMachingCaret)
                                {
                                    e.Graphics.FillRectangle(m_bracketBackgroundBrush, reservedCloseBracketDraw[0], reservedCloseBracketDraw[1], reservedCloseBracketDraw[2], reservedCloseBracketDraw[3]);
                                }
                                break;
                            }
                        }
                }
                #endregion

                #region ** 캐럿 왼쪽 CloseBracket 존재 시 대칭 OpenBracket 표시
                if (0 < this.selectionStartIndex) // CloseBracket은 Caret의 왼쪽 문자를 검사해야됨
                {
                    int bracketFind = 0;
                    if (this.lineInfo[this.selectionLineNumber].m_textType[this.selectionStartIndex - 1] == Lines.TextType.PlainText)
                        for (int bracketNum = 0; bracketNum < closeBracketArray.Length; bracketNum++)
                        {
                            if (lineText[this.selectionStartIndex - 1].Equals(closeBracketArray[bracketNum])) //CloseBracket을 찾은 경우
                            {
                                char closeBracket = closeBracketArray[bracketNum]; //찾은 CloseBracket
                                char openBracket = openBracketArray[bracketNum]; //찾을 OpenBracket
                                float openBracketWidth = calculateStringWidth(e.Graphics, this.Font, openBracket + ""); //찾은 OpenBracketWidth
                                float closeBracketWidth = calculateStringWidth(e.Graphics, this.Font, closeBracket + ""); //찾을 CloseBracketWidth

                                float bracketWidth = calculateStringWidth(e.Graphics, this.Font, closeBracket + "");
                                float tmpWidth = 0;

                                bool isExistMachingCaret = false; //대칭 괄호 존재시 true

                                float[] reservedCloseBracketDraw = new float[0]; //CloseBracket과 대칭되는 OpenBracket을 찾았을 경우에만 fillRectange 보여주기 위한 데이터 저장소

                                // 캐럿 존재하는 라인부터 첫번째 라인까지 Bracket검색
                                for (int line_i = this.selectionLineNumber; line_i >= 0; line_i--)
                                {
                                    if (isExistMachingCaret)
                                        break;
                                    string tmpLineText = this.lineInfo[line_i].Text;
                                    for (int line_charIndex = tmpLineText.Length - 1; line_charIndex >= 0; line_charIndex--)
                                    {
                                        if (line_i == this.selectionLineNumber ? this.selectionStartIndex <= line_charIndex : false)
                                            continue;
                                        if (this.lineInfo[line_i].m_textType[line_charIndex].Equals(Lines.TextType.PlainText)) //텍스트가 평문일 경우에만
                                        {
                                            if (tmpLineText[line_charIndex].Equals(closeBracket)) // Case1. CloseBracket 발견 시 Count++
                                            {
                                                if (line_i == this.selectionLineNumber ? (this.selectionStartIndex > 0 ? this.selectionStartIndex - 1 == line_charIndex : false) : false) //시작 CloseBracket 색칠
                                                {
                                                    if (firstViewLine <= line_i && line_i <= lastViewLine) //현재 보일경우
                                                    {
                                                        float line_bracketX = this.indentValue;
                                                        for (int tmpj = firstViewIndex; tmpj <= this.selectionStartIndex; tmpj++)
                                                        {
                                                            if (tmpj == this.selectionStartIndex - 1) //첫번째 발견한 CloseBracket인 경우, 즉 현재 캐럿 바로 옆에있는 CloseBracket일 경우
                                                            {
                                                                reservedCloseBracketDraw = new float[] { line_bracketX + marginX, (line_i - firstViewLine) * lineHeight, closeBracketWidth + marginX, lineHeight };
                                                                //e.Graphics.FillRectangle(m_bracketBackgroundBrush, line_bracketX + marginX, (line_i - firstViewLine) * lineHeight, openBracketWidth + marginX, lineHeight);
                                                                break;
                                                            }
                                                            tmpWidth = this.calculateStringWidth(e.Graphics, this.Font, tmpLineText, tmpj);

                                                            line_bracketX += tmpWidth;
                                                            if (line_bracketX >= this.Width)
                                                                break;
                                                        }
                                                    }
                                                }
                                                bracketFind++;
                                            }
                                            else if (tmpLineText[line_charIndex].Equals(openBracket)) // Case2. OpenBracket 발견 시
                                            {
                                                bracketFind--;
                                                if (bracketFind == 0)
                                                {
                                                    float line_bracketX = this.indentValue;
                                                    for (int tmpj = firstViewIndex; tmpj <= line_charIndex; tmpj++)
                                                    {
                                                        if (tmpj == line_charIndex)
                                                        {
                                                            if (firstViewLine <= line_i && line_i <= lastViewLine) // 화면 세로 기준으로 Bracket이 보일 경우
                                                            {
                                                                if (line_bracketX < this.Width) // 화면 가로 기준으로 Bracket이 보일 경우
                                                                    e.Graphics.FillRectangle(m_bracketBackgroundBrush, line_bracketX + marginX, (line_i - firstViewLine) * lineHeight, openBracketWidth + marginX, lineHeight);
                                                            }
                                                            break;
                                                        }
                                                        tmpWidth = this.calculateStringWidth(e.Graphics, this.Font, tmpLineText, tmpj);
                                                        line_bracketX += tmpWidth;
                                                    }
                                                    isExistMachingCaret = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (reservedCloseBracketDraw.Length != 0 && isExistMachingCaret) ////CloseBracket이 보이는 상태이고 CloseBracket과 대칭되는 OpenBracket을 찾았을 경우에만 fillRectange 보여주기
                                {
                                    e.Graphics.FillRectangle(m_bracketBackgroundBrush, reservedCloseBracketDraw[0], reservedCloseBracketDraw[1], reservedCloseBracketDraw[2], reservedCloseBracketDraw[3]);
                                }
                                break;
                            }
                        }
                }
                #endregion
            }

            // 현재 선택된 단어 측정 - MarkWordTheSameAsSelectionWord(같은단어 표시) 기능에 사용
            #region ** ...
            string selectLineText = this.lineInfo[this.selectionLineNumber].Text;
            string selectionWord = string.Empty;
            float selectionWordWidth = 0;
            int originalWordIndex = 0;
            if (this.m_selectionLength > 0 ? this.selectionLineNumber == this.selectionEndLineNumber : false)
            {
                int tmpMax = Math.Max(this.selectionStartIndex, this.selectionEndIndex);
                int tmpMin = Math.Min(this.selectionStartIndex, this.selectionEndIndex);
                originalWordIndex = tmpMin;
                selectionWord = selectLineText.Substring(tmpMin, tmpMax - tmpMin); ; //현재 선택된 단어
                selectionWordWidth = calculateStringWidth(e.Graphics, this.Font, selectionWord);
            }

            //검색 문자열에 공백 존재시 Search 하지않음(Ctrl+A와 같은 전체선택 혹은 긴문자열 선택 시 오류 발생)
            selectionWord = !string.IsNullOrWhiteSpace(selectionWord) && selectionWord.IndexOf(' ') == -1 ?
                RegexData.Instance.ConvertRegexWordToPlainText(selectionWord) : string.Empty;

            #endregion

            //현재 보이는 첫번째 라인부터 보여야할 라인 갯수까지의 Text를 출력한다.
            //맨끝 잘리는 Line도 보이기 위해 +1 지정
            // 131023 - MarkWordTheSameAsSelectionWord 효과 추가
            #region ** ...
            for (int i = firstViewLine; i < firstViewLine + viewLines + 1; i++)
            {
                if (this.lineInfo.Count <= i)
                    continue;
                string lineText = this.lineInfo[i].Text;
                float width = 0;
                float fullWidth = indentValue;

                int caretY = ((selectionLineNumber - firstViewLine) * lineHeight);// +marginY;

                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                // 화면 좌측에 줄번호 표시
                if (LineNumberVisible)
                {
                    e.Graphics.DrawString((i + 1) + "", new Font("굴림", this.Font.Size - 6), m_lineNumberColorBrush, new RectangleF(0, marginY + lineHeight * count, indentValue, lineHeight), sf);
                }

                // 같은단어 표시(mark) 기능 활성화됬을 경우 실행
                if (!string.IsNullOrWhiteSpace(selectionWord) ? this.MarkWordTheSameAsSelectionWord && this.m_selectionLength > 0 : false)
                {
                    // Regex로 같은 단어 찾아서 표시
                    #region ...
                    List<int> markwords = new List<int>();
                    try
                    {
                        System.Text.RegularExpressions.Regex tmp_findWord = new System.Text.RegularExpressions.Regex("\\b" + selectionWord + "\\b");
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                    }
                    System.Text.RegularExpressions.Regex regex_findWord = new System.Text.RegularExpressions.Regex("\\b" + selectionWord + "\\b");
                    System.Text.RegularExpressions.Match match_findWord;
                    for (match_findWord = regex_findWord.Match(lineText); match_findWord.Success; match_findWord = match_findWord.NextMatch())
                    {
                        markwords.Add(match_findWord.Index);
                    }
                    int markwordLineCount = 0;


                    if (markwords.Count > 0)
                    {
                        for (int tmp_wordChar = firstViewIndex; tmp_wordChar <= lineText.Length; tmp_wordChar++)
                        {
                            if (markwordLineCount < markwords.Count ? tmp_wordChar == markwords[markwordLineCount] : false)
                            {
                                //발견된 인덱스가 현재 selection된 부분일 경우 Brush색상을 selectionTextBackgroundBrush로 한다.
                                if (this.selectionLineNumber == i && originalWordIndex == tmp_wordChar)
                                    e.Graphics.FillRectangle(this.m_selectionTextBackgroundBrush, marginX + fullWidth, lineHeight * count, selectionWordWidth + marginX, lineHeight);
                                else if (!string.IsNullOrWhiteSpace(selectionWord))
                                    e.Graphics.FillRectangle(m_markword_ColorBrush, marginX + fullWidth, lineHeight * count, selectionWordWidth + marginX, lineHeight);

                                markwordLineCount++;
                            }
                            width = calculateStringWidth(e.Graphics, this.Font, lineText, tmp_wordChar);

                            if (fullWidth >= this.Width)
                            {
                                break;
                            }
                            fullWidth += width;
                        }
                    }
                    #endregion
                }
                fullWidth = indentValue;
                for (int j = firstViewIndex; j <= lineText.Length; j++)
                {
                    width = calculateStringWidth(e.Graphics, this.Font, lineText, j);

                    // 텍스트 표시
                    if (j < lineText.Length)
                    {
                        e.Graphics.DrawString(lineText[j].ToString(), this.Font, ConvertLineTextTypeToColorBrush(lineInfo[i].m_textType[j]), fullWidth, marginY + lineHeight * count);
                    }

                    // 캐럿 표시
                    if (this.Focused)
                    {
                        if (i == selectionLineNumber)
                            if (j == selectionStartIndex)
                            {
                                float caretX = fullWidth + marginX;
                                this.call_setCaretPos((int)caretX, caretY);
                                isCaretMustView = true;
                            }
                    }
                    fullWidth += width;
                    if (fullWidth >= this.Width)
                    {
                        break;
                    }
                }

                // 현재 선택된 줄의 LineBorder표시
                if (i == selectionLineNumber && isSelectionLineBorderVisible && this.m_selectionLength == 0)
                    e.Graphics.DrawRectangle(m_selectionBorderLinePen, new Rectangle(1, caretY, this.Width - 2, lineHeight));

                count++;
            }
            if (LineNumberVisible) // 줄번호가 visible일때 줄번호와 텍스트 사이의 구분선 표시
                e.Graphics.DrawLine(new Pen(m_lineNumberColorBrush),
                                    new Point(indentValue > 0 ? indentValue - 1 : 0, 0),
                                    new Point(indentValue > 0 ? indentValue - 1 : 0, this.Height));

            #endregion

            //HideCaret여러번 실행시 ShowCaret해도 캐럿이 안보임. 한번만 실행되게 지정
            #region** Caret Show || Hide YN
            if (isCaretMustView)
                viewCaret();
            else
                hideCaret();
            #endregion

            vScrollSet(firstViewLine, this.lineInfo.Count);
            if (firstViewIndex > horizonMaximumValue)
            {
                Console.WriteLine("Critical Bug!!!!!");
                hScrollSet(0, horizonMaximumValue);
            }
            else
            {
                hScrollSet(firstViewIndex, horizonMaximumValue);
            }

            base.OnPaint(e);
        }
        #endregion

        #region ** MouseEvent


        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (isDraging)
            {
                isDraging = false;
                this.Invalidate();
                this.Update();
            }
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            this.Cursor = Cursors.IBeam;

            if (isDraging && e.Button == MouseButtons.Left)
            {
                if (!this.DisplayRectangle.Contains(e.Location))
                {
                    if(!Timer_OnMouseLeaveDraging.Enabled)
                        Timer_OnMouseLeaveDraging.Start();
                }

                //if (Math.Abs(e.Location.X - mouseDownPos.X) > 1) // 마우스 움직이면서 여러곳 클릭시 Draging 되는 현상 방지 위해 X값 차이나는지 확인
                //{
                    #region 마우스 드래깅 효과 // selectionLineNumber, selectionStartIndex만 조정
                    //Console.WriteLine("isDraging");
                    Point mouseMovePos = e.Location; // 움직인마우스 위치
                    this.selectionEndLineNumber = this.mouseDownSelectionLineNumber;
                    this.selectionEndIndex = this.mouseDownselectionStartIndex;

                    int viewLinesCount = getViewLinesCount();


                    //for (int viewLinesCount_i = 1; viewLinesCount_i < lastViewLine + 1; viewLinesCount_i++)
                    for (int viewLinesCount_i = 1; viewLinesCount_i < firstViewLine + viewLinesCount; viewLinesCount_i++)
                    {
                        if (mouseMovePos.Y <= viewLinesCount_i * this.lineHeight)
                        {
                            int mouseLocatedLine = viewLinesCount_i - 1 + firstViewLine; // 현재 마우스가 위치해 있는 Line
                            //Console.WriteLine(mouseLocatedLine);
                            if (mouseLocatedLine < this.mouseDownSelectionLineNumber) //현재 마우스가 위치해 있는 라인이 드래그를 시작한 라인보다 위에있을 경우
                            {
                                float tmpCharWidth = 0;

                                Graphics g = this.CreateGraphics();
                                string lineText = this.lineInfo[mouseLocatedLine].Text;
                                float tmpFullWidth = this.indentValue;

                                this.selectionLineNumber = mouseLocatedLine;
                                bool findSelectionStartIndex = false;

                                for (int char_i = firstViewIndex; char_i <= this.lineInfo[mouseLocatedLine].Text.Length; char_i++)
                                {
                                    tmpCharWidth = calculateStringWidth(g, this.Font, lineText, char_i);

                                    if (tmpFullWidth + (tmpCharWidth / 2) >= mouseMovePos.X)
                                    {
                                        this.selectionStartIndex = char_i;
                                        findSelectionStartIndex = true;
                                        //selectionEndLineNumber = mouseLocatedLine;
                                        break;
                                    }

                                    tmpFullWidth += tmpCharWidth;
                                }
                                if (!findSelectionStartIndex)
                                    this.selectionStartIndex = this.lineInfo[mouseLocatedLine].Text.Length;
                                g.Dispose();
                            }
                            else if (mouseLocatedLine > this.mouseDownSelectionLineNumber) //현재 마우스가 위치해 있는 라인이 드래그를 시작한 라인보다 밑에있을 경우
                            {
                                if (mouseLocatedLine >= this.lineInfo.Count)
                                {
                                    //continue;
                                    mouseLocatedLine = this.lineInfo.Count - 1;
                                }
                                //if (mouseLocatedLine == this.lineInfo.Count)
                                //    mouseLocatedLine = this.lineInfo.Count - 1;

                                float tmpCharWidth = 0;
                                float tmpFullWIdth = this.indentValue;
                                Graphics g = this.CreateGraphics();
                                string lineText = this.lineInfo[mouseLocatedLine].Text;
                                this.selectionLineNumber = mouseLocatedLine;
                                bool findSelectionStartIndex = false;


                                for (int char_i = firstViewIndex; char_i <= this.lineInfo[mouseLocatedLine].Text.Length; char_i++)
                                {
                                    tmpCharWidth = calculateStringWidth(g, this.Font, lineText, char_i);
                                    if (tmpFullWIdth + (tmpCharWidth / 2) >= mouseMovePos.X)
                                    {
                                        this.selectionStartIndex = char_i;
                                        findSelectionStartIndex = true;
                                        break;
                                    }
                                    tmpFullWIdth += tmpCharWidth;
                                }
                                if (!findSelectionStartIndex)
                                    this.selectionStartIndex = this.lineInfo[mouseLocatedLine].Text.Length;
                                g.Dispose();
                            }
                            else
                            {
                                // selectStart 와 selectEnd 부분이 한 라인에 있을때
                                float tmpCharWidth = 0;
                                float tmpFullWidth = this.indentValue;
                                Graphics g = this.CreateGraphics();
                                string lineText = this.lineInfo[mouseLocatedLine].Text;
                                selectionLineNumber = mouseLocatedLine;
                                bool findSelectionStartIndex = false;

                                for (int char_i = firstViewIndex; char_i <= this.lineInfo[mouseLocatedLine].Text.Length; char_i++)
                                {
                                    tmpCharWidth = calculateStringWidth(g, this.Font, lineText, char_i);
                                    if (tmpFullWidth + (tmpCharWidth / 2) >= mouseMovePos.X)
                                    {
                                        this.selectionStartIndex = char_i;
                                        findSelectionStartIndex = true;
                                        break;
                                    }
                                    tmpFullWidth += tmpCharWidth;
                                }
                                if (!findSelectionStartIndex)
                                    this.selectionStartIndex = this.lineInfo[mouseLocatedLine].Text.Length;
                                g.Dispose();
                            }
                            break;
                        }
                    }
                    //CaretIsGoEndEndIndex();

                    selectionLengthCalculate();

                    #endregion

                    
                    //{
                    //    if (this.selectionStartIndex != this.lineInfo[this.selectionLineNumber].Text.Length)
                    //        CaretIsGoRight();
                    //    else

                    //if (!CaretX_IsVisible())
                    //    CaretIsGoEnd();
                if(!this.Timer_OnMouseLeaveDraging.Enabled)
                    if (this.selectionStartIndex == this.lineInfo[this.selectionLineNumber].Text.Length)
                        CaretIsGoEnd();
                    //}
                //}

                this.Invalidate();
                this.Update();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            mouseDownPos = e.Location;

            #region ** 왼쪽버튼 클릭 이벤트
            if (e.Button == MouseButtons.Left)
            {
                //bool requiredInvalidate = false;
                int tmpSelectionStartIndex = 0; // 마우스로 클릭한 index
                int tmpSelectionLineNumber = 0; // 마우스로 클릭한 line

                #region firstViewLine변경 및 selectionLineNumber, selectionStartIndex 측정
                int viewLineCount = getViewLinesCount();

                bool isMiddlePosCaret = false;

                int lineCount = 1;
                for (int i = firstViewLine; i <= firstViewLine + viewLineCount + 1; i++)
                {
                    if (i == lastViewLine ? mouseDownPos.Y < lineCount * lineHeight : false) //잘린 마지막줄 클릭시
                    {
                        if (i > this.lineInfo.Count - 1)
                        {
                            continue;
                        }
                        #region ** X값 계산
                        float tmpWidth = 0;
                        float fullWidth = indentValue;
                        string lineText = lineInfo[i].Text;
                        Graphics tmpGraphics = this.CreateGraphics();
                        for (int j = firstViewIndex; j < lineText.Length; j++)
                        {
                            tmpWidth = calculateStringWidth(tmpGraphics, this.Font, lineText, j);

                            if (fullWidth + (tmpWidth / 2) >= mouseDownPos.X)
                            {
                                tmpSelectionStartIndex = j;
                                isMiddlePosCaret = true;
                                break;
                            }
                            fullWidth += tmpWidth;
                        }
                        if (!isMiddlePosCaret)
                        {
                            tmpSelectionStartIndex = lineText.Length;
                        }
                        tmpGraphics.Dispose();
                        #endregion
                        tmpSelectionLineNumber = i;
                        this.firstViewLine++;
                        break;

                    }
                    else if (this.lineInfo.Count >= firstViewLine + lineCount ? mouseDownPos.Y < lineCount * lineHeight : false) //클릭한 부분이 본문 내에 있을 때
                    {
                        #region ** X값 계산
                        float tmpWidth = 0;
                        float fullWidth = indentValue;
                        string lineText = lineInfo[i].Text;
                        Graphics tmpGraphics = this.CreateGraphics();
                        for (int j = firstViewIndex; j < lineText.Length; j++)
                        {
                            tmpWidth = calculateStringWidth(tmpGraphics, this.Font, lineText, j);

                            if (fullWidth + (tmpWidth / 2) >= mouseDownPos.X)
                            {
                                tmpSelectionStartIndex = j;
                                isMiddlePosCaret = true;
                                break;
                            }
                            fullWidth += tmpWidth;
                        }
                        if (!isMiddlePosCaret)
                        {
                            tmpSelectionStartIndex = lineText.Length;
                        }
                        tmpGraphics.Dispose();
                        #endregion

                        tmpSelectionLineNumber = i;
                        break;
                    }
                    else if (mouseDownPos.Y < lineCount * lineHeight && this.lineInfo.Count <= firstViewLine + lineCount) //클릭한 부분이 본문 밖에 존재할 때
                    {
                        //if (this.firstViewLine >= this.lineInfo.Count)
                        //    this.firstViewLine = this.lineInfo.Count - 1;
                        //tmpSelectionLineNumber = this.lineInfo.Count - 1;
                        //tmpSelectionStartIndex = this.lineInfo[tmpSelectionLineNumber].Text.Length;
                        //this.firstViewIndex = 0;

                        ////isMiddlePosCaret = true;

                        if (this.firstViewLine >= this.lineInfo.Count)
                            this.firstViewLine = this.lineInfo.Count - 1;
                        tmpSelectionLineNumber = this.lineInfo.Count - 1;

                        #region ** X값 계산
                        float tmpWidth = 0;
                        float fullWidth = indentValue;
                        string lineText = lineInfo[tmpSelectionLineNumber].Text;
                        Graphics tmpGraphics = this.CreateGraphics();
                        for (int j = firstViewIndex; j < lineText.Length; j++)
                        {
                            tmpWidth = calculateStringWidth(tmpGraphics, this.Font, lineText, j);

                            if (fullWidth + (tmpWidth / 2) >= mouseDownPos.X)
                            {
                                tmpSelectionStartIndex = j;
                                isMiddlePosCaret = true;
                                break;
                            }
                            fullWidth += tmpWidth;
                        }
                        if (!isMiddlePosCaret)
                        {
                            tmpSelectionStartIndex = lineText.Length;
                        }
                        tmpGraphics.Dispose();
                        #endregion

                        break;
                    }
                    lineCount++;
                }
                #endregion

                isDraging = true;
                if (ModifierKeys == Keys.Shift) // Shift키가 보조키로 입력된 경우
                {
                    // 이전에 클릭한 부분부터 현재 클릭한 부분까지 Selection 효과
                    //this.selectionEndLineNumber = this.selectionLineNumber;
                    //this.selectionEndIndex = this.selectionStartIndex;
                    this.selectionLineNumber = tmpSelectionLineNumber;
                    this.selectionStartIndex = tmpSelectionStartIndex;
                    
                    selectionLengthCalculate();
                }
                else
                {
                    this.selectionLineNumber = tmpSelectionLineNumber;
                    this.selectionStartIndex = tmpSelectionStartIndex;
                    this.selectionEndLineNumber = this.selectionLineNumber;
                    this.selectionEndIndex = this.selectionStartIndex;
                    selectionLengthSetZero();
                }

                if (!isMiddlePosCaret)
                    CaretIsGoEnd();

                mouseDownSelectionLineNumber = this.selectionLineNumber;
                mouseDownselectionStartIndex = this.selectionStartIndex;
                //if (requiredInvalidate)
                //{
                    this.Invalidate();
                    this.Update();
                //}
            }
            #endregion

            base.OnMouseDown(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            //마우스 움직임없이 더블클릭시 더블클릭 이벤트 발생됨.
            //Console.WriteLine("DoubleClick");
            string lineText = this.lineInfo[this.selectionLineNumber].Text;
            int[] wordStartEndIndex = RegexData.Instance.getWordStartEndIndex(lineText, this.selectionStartIndex, m_wordSeparator_pattern);

            this.selectionStartIndex = wordStartEndIndex[1];
            this.selectionEndLineNumber = this.selectionLineNumber;
            this.selectionEndIndex = wordStartEndIndex[0];
            this.selectionLengthCalculate();
            this.CaretIsGoEnd();
            this.Invalidate();
            this.Update();
            
            base.OnMouseDoubleClick(e);
        }

        protected override void OnClick(EventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
                string lineText = this.lineInfo[this.selectionLineNumber].Text;
                int[] wordStartEndIndex = RegexData.Instance.getWordStartEndIndex(lineText, this.selectionStartIndex, m_wordSeparator_pattern);

                this.selectionStartIndex = wordStartEndIndex[1];
                this.selectionEndLineNumber = this.selectionLineNumber;
                this.selectionEndIndex = wordStartEndIndex[0];
                this.selectionLengthCalculate();
                this.CaretIsGoEnd();
                this.Invalidate();
                this.Update();
            }
            base.OnClick(e);
        }

        #endregion

        #region ************************* Private Method ****************************

        /// <summary>
        /// [OnPaint시 사용] 해당 타입을 Brush로 변경
        /// </summary>
        /// <param name="textType"></param>
        /// <returns></returns>
        private SolidBrush ConvertLineTextTypeToColorBrush(Lines.TextType textType)
        {
            switch (textType)
            {
                case Lines.TextType.Comment:
                    return m_comment_ColorBrush;
                case Lines.TextType.keyword1:
                    return m_keyword1_ColorBrush;
                case Lines.TextType.keyword2:
                    return m_keyword2_ColorBrush;
                case Lines.TextType.keyword3:
                    return m_keyword3_ColorBrush;
                case Lines.TextType.PlainText:
                    return m_plainTextBrush;
                case Lines.TextType.StringText:
                    return m_doubleQuotes_ColorBrush;
                default :
                    //Console.WriteLine("NotFound TextColor Brush");
                    return (SolidBrush)Brushes.Black;
            }
        }
        
        /// <summary>
        /// 캐럿 Draw 및 캐럿이 이동되었다는 CaretMoved delegate Event 호출
        /// </summary>
        /// <param name="caretX"></param>
        /// <param name="caretY"></param>
        private void call_setCaretPos(int caretX, int caretY)
        {
            recentlyCaretPos = new Point(caretX, caretY);
            
            SetCaretPos(caretX, caretY);
            if(CaretMoved != null)
                CaretMoved();
        }
        
        #region ** Undo & Redo Method

        /// <summary>
        /// 현재 텍스트박스의 상태를 UndoStack에 저장한다.
        /// 텍스트상태 Insert시 RedoStack 초기화
        /// </summary>
        private void insertUndoData()
        {
            txtStatusData txtData = new txtStatusData(this.Text, this.firstViewLine, this.firstViewIndex
            , this.selectionLineNumber, this.selectionStartIndex, this.selectionEndLineNumber, this.selectionEndIndex);
            this.undoStack.Push(txtData);
            isDefaultText = false;
        }

        /// <summary>
        /// 현재 텍스트박스의 상태를 RedoStack에 저장한다.
        /// </summary>
        private void insertRedoData()
        {
            txtStatusData txtData = new txtStatusData(this.Text, this.firstViewLine, this.firstViewIndex
            , this.selectionLineNumber, this.selectionStartIndex, this.selectionEndLineNumber, this.selectionEndIndex);

            this.redoStack.Push(txtData);
        }

        /// <summary>
        /// 입력받은 텍스트상태데이터를 텍스트박스에 적용한다.
        /// </summary>
        /// <param name="data"></param>
        private void applytxtStatusData(txtStatusData data)
        {
            object[] objs = data.getDataObject();
            this.Text = (string)objs[0];
            this.firstViewLine = (int)objs[1];
            this.firstViewIndex = (int)objs[2];
            this.selectionLineNumber = (int)objs[3];
            this.selectionStartIndex = (int)objs[4];
            this.selectionEndLineNumber = (int)objs[5];
            this.selectionEndIndex = (int)objs[6];
            this.selectionLengthCalculate();

            this.Invalidate();
            this.Update();
        }

        #endregion

        /// <summary>
        /// 텍스트에 일반 문자를 입력했을 경우 이벤트 호출(NormalTextChanged 이벤트 할당여부 checking)
        /// </summary>
        private void call_NormalTextChanged()
        {
             if (NormalTextChanged != null)
                NormalTextChanged();
        }

        /// <summary>
        /// 설명창 표시
        /// </summary>
        /// <param name="visible"></param>
        private void call_descriptionVisible(bool visible)
        {
            if (descriptionVisible != null)
                descriptionVisible(visible);
        }

        /// <summary>
        /// 마우스로 텍스트를 Draging시 마우스가 텍스트박스 밖으로 벗어날 경우 텍스트박스의 스크롤값을 조정하기 위한 timer 이벤트 설정.
        /// </summary>
        private void Timer_OnMouseLeaveDragingSetting()
        {
            Timer_OnMouseLeaveDraging.Interval = 100;

            Timer_OnMouseLeaveDraging.Tick += new EventHandler(delegate
            {
                Point MousePosInnerClient = PointToClient(MousePosition);
                if (!this.isDraging || this.DisplayRectangle.Contains(MousePosInnerClient))
                {
                    Timer_OnMouseLeaveDraging.Stop();
                    //Console.WriteLine("TimerStop");
                }
                else
                {
                    bool occurChanging = false;
                    #region 마우스 드래그 도중 마우스가 X축으로 텍스트박스를 넘어갔을때
                    if (MousePosInnerClient.X < this.DisplayRectangle.Left)
                    {
                        //this.selectionLineNumber 
                        int tmp = 3;
                        while (true)
                        {
                            if (this.firstViewIndex - tmp >= 0)
                            {
                                this.firstViewIndex -= tmp;
                                break;
                            }
                            tmp--;
                        }
                        occurChanging = true;
                    }
                    else if (MousePosInnerClient.X > this.DisplayRectangle.Right)
                    {
                        if (!this.CaretX_IsVisible())
                        {
                            this.firstViewIndex += 3;
                            if (this.selectionStartIndex < this.firstViewIndex)
                            {
                                CaretIsGoEnd();
                            }
                        }
                        occurChanging = true;
                        CaretIsGoRight();
                    }
                    #endregion

                    #region 마우스 드래그 도중 마우스가 Y축으로 텍스트박스를 넘어갔을때
                    if (MousePosInnerClient.Y < this.DisplayRectangle.Top)
                    {
                        int tmp = 2;
                        while (true)
                        {
                            if (this.firstViewLine - tmp >= 0)
                            {
                                this.firstViewLine -= tmp;
                                break;
                            }
                            tmp--;
                        }
                        occurChanging = true;
                    }
                    else if (MousePosInnerClient.Y > this.DisplayRectangle.Bottom)
                    {
                        int tmp = 2;
                        int tmpLastViewLine = this.lastViewLine;
                        if (tmpLastViewLine < this.lineInfo.Count)
                        {
                            firstViewLine += tmp;
                        }
                        occurChanging = true;
                    }

                    if (occurChanging)
                    {
                        //MouseEventArgs ex = new MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 0, MousePosInnerClient.X, MousePosInnerClient.Y, 0);
                        // selection 영역을 재설정하기 위해 Move 이벤트 다시 호출
                        //OnMouseMove(ex);
                        this.Invalidate();
                        this.Update();
                    }

                    #endregion
                }
            });
        }

        /// <summary>
        /// 현재 선택된 라인이 화면에 보이지 않을경우 화면높이의 dividend / divisor 지점에 Caret이 위치하게끔 보여준다.
        /// dividend와 divisor을 나눈값이 1일 경우, 현재 Caret이 있는 줄이 텍스트의 맨 마지막으로 보이는 줄이 되도록 지정
        /// SelectionLineNumber 참조
        /// </summary>
        /// <param name="dividend">분자</param>
        /// <param name="divisor">분모</param>
        private void CaretIsLocateScreenByFraction(int dividend, int divisor) 
        {
            LineLocateScreenByFraction(this.selectionLineNumber, dividend, divisor);
                //int lineCount = 0;
                //int viewLineCount = getViewLinesCount();
                //for (int i = this.lineInfo.Count - 1; i >= 0; i--)
                //{
                //    if (i <= this.selectionLineNumber)
                //    {
                //        lineCount++;
                //        if (i == 0 || lineCount >= (viewLineCount / divisor) * dividend) //화면 기준으로 2/3 지점 밑에 캐럿이 위치하게끔 보여준다.
                //        {
                //            this.firstViewLine = i;
                //            break;
                //        }                                
                //    }
                //}
        }

        /// <summary>
        /// 파라미터에 입력한 라인번호가 세로기준으로 화면의 dividend / divisor 에 위치하도록 firstViewLine 지정
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        private void LineLocateScreenByFraction(int lineNumber, int dividend, int divisor)
        {
            int lineCount = 0;
            int viewLineCount = getViewLinesCount();
            for (int i = this.lineInfo.Count - 1; i >= 0; i--)
            {
                if (i <= lineNumber)
                {
                    lineCount++;
                    if (i == 0 || lineCount >= (viewLineCount / divisor) * dividend) //화면 기준으로 2/3 지점 밑에 캐럿이 위치하게끔 보여준다.
                    {
                        this.firstViewLine = i;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// HorizonScroll을 조정해서 해당 인덱스를 보여준다. [firstViewIndex만 조절]
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <param name="index"></param>
        private void IndexLocateScreenDefault(int lineNumber, int index)
        {
            int dividend = 1;
            int divisor = 3;

            string lineText = this.lineInfo[lineNumber].Text;

            float tmpCharWidth;
            float tmpFullWidth = this.indentValue;
            Graphics g = this.CreateGraphics();

            if(firstViewIndex <= index)
                for (int i = firstViewIndex; i <= index; i++)
                {
                    tmpCharWidth = this.calculateStringWidth(g, this.Font, lineText, i);
                    if (this.Width <= (tmpFullWidth + tmpCharWidth))
                    {
                        dividend = (divisor - dividend);
                        break;
                    }
                    else if (i == index) //보임?
                    {
                        return;
                    }
                    tmpFullWidth += tmpCharWidth;
                }

            tmpFullWidth = this.indentValue;

            bool find = false;
            float widthArea = ((this.Width / divisor) * dividend);
            for (int i = index; i >= 0; i--)
            {
                tmpCharWidth = this.calculateStringWidth(g, this.Font, lineText, i);
                //firstViewIndex 찾기
                if (widthArea <= (tmpFullWidth + tmpCharWidth))
                {
                    find = true;
                    this.firstViewIndex = i;

                    tmpFullWidth = this.indentValue;


                    //// 지정해야될 firstViewIndex가 0~this.width사이에 보일 경우에는 firstViewIndex를 0으로 지정
                    //for (int j = 0; j <= this.firstViewIndex; j++)
                    //{
                    //    tmpCharWidth = this.calculateStringWidth(g, this.Font, lineText, j);
                    //    tmpFullWidth += tmpCharWidth;
                    //}
                    ////Console.WriteLine(this.Width + ":" + this.Height + ":" + tmpFullWidth);
                    //if ((this.Width) >= tmpFullWidth)
                    //{
                    //    find = false;
                    //}
                    
                    break;
                }
                tmpFullWidth += tmpCharWidth;
            }
            g.Dispose();
            if (!find)
            {
                this.firstViewIndex = 0;
            }
            
        }

        /// <summary>
        /// 선택한 영역의 텍스트를 지운다. [param = invalidate 여부]
        /// </summary>
        /// <param name="doInvalidate">invalidate 요청 여부</param>
        private void selectedTextRemove(bool doInvalidate)
        {
            int minLineNumber = Math.Min(selectionLineNumber, selectionEndLineNumber);
            int maxLineNumber = Math.Max(selectionLineNumber, selectionEndLineNumber);
            int minLineStartIndex = minLineNumber == selectionLineNumber ? selectionStartIndex : selectionEndIndex;
            int maxLineEndIndex = maxLineNumber == selectionLineNumber ? selectionStartIndex : selectionEndIndex;

            if (minLineNumber != maxLineNumber)
            {
                string minLineText = this.lineInfo[minLineNumber].Text;
                string minLinePrevText = minLineText.Substring(0, minLineStartIndex); // select를 시작한 라인에서 선택 안한 부분
                string maxLineText = this.lineInfo[maxLineNumber].Text;
                string maxLineNextText = maxLineText == string.Empty ? "" : maxLineText.Substring(maxLineEndIndex); // select가 끝난 라인에서 선택 안한 부분
                this.lineInfo[minLineNumber].Text = minLinePrevText + maxLineNextText;
                for (int i = maxLineNumber; i > minLineNumber; i--)
                    this.lineInfo.RemoveAt(i);
            }
            else
            {
                minLineStartIndex = Math.Min(selectionStartIndex, selectionEndIndex);
                maxLineEndIndex = Math.Max(selectionStartIndex, selectionEndIndex);
                // 가끔 버그발생 - SelectionEndIndex가 Length보다 큼 (141125)
                //maxLineEndIndex = Math.Min(selectionEndIndex, this.lineInfo[minLineNumber].Text.Length - 1);

                string lineText = this.lineInfo[minLineNumber].Text;
                string prevText = lineText.Substring(0, minLineStartIndex);
                string nextText = lineText.Substring(maxLineEndIndex);
                this.lineInfo[minLineNumber].Text = prevText + nextText;

            }
            
            this.selectionLineNumber = minLineNumber;
            this.selectionStartIndex = minLineStartIndex;

            this.setScrollByLineAndIndex(this.selectionLineNumber, this.selectionStartIndex);
            selectionLengthSetZero();
            if (doInvalidate)
            {
                this.Invalidate();
                this.Update();
            }

        }


        //캐럿 이동후 현재 지점의 This.Width * 1/3 영역을 앞으로 당겨서 출력한다?
        private void CaretIsGoLeft()
        {
            if (this.firstViewIndex != 0 ? this.selectionStartIndex <= this.firstViewIndex : false)
            {
                string lineText = lineInfo[this.selectionLineNumber].Text;
                float tmpWidth = 0;
                float fullWidth = this.indentValue;
                float tmpScreenWidth = this.indentValue;
                //int tmpScreenIndex = 0;
                Graphics tmpGraphics = this.CreateGraphics();
                for (int i = this.firstViewIndex; i >= 0; i--)
                {
                    tmpWidth = calculateStringWidth(tmpGraphics, this.Font, lineText, i);
                    tmpScreenWidth += tmpWidth;

                    if (tmpScreenWidth + 5 >= this.Width / 3)
                    {
                        firstViewIndex = i;
                        break;
                    }

                }
                tmpGraphics.Dispose();
                //if (this.firstViewIndex <= 10)
                //    this.firstViewIndex = 0;
                if (tmpScreenWidth <= this.ClientSize.Width/3)
                {
                    this.firstViewIndex = 0;
                }
            }
        }

        /// <summary>
        /// 캐럿이 보이는 상태에서 우측으로 이동할때만 화면 이동되는 함수
        /// </summary>
        /// <returns></returns>
        private bool CaretIsGoRight()
        {
            bool requiredInvalidate = false;
            string lineText = lineInfo[this.selectionLineNumber].Text;
            float tmpWidth = 0;
            float fullWidth = this.indentValue;
            float tmpScreenWidth = this.indentValue;
            //int tmpScreenIndex = 0;
            Graphics tmpGraphics = this.CreateGraphics();
            int lastIndex = lineText.Length;
            for (int i = firstViewIndex; i <= lineText.Length; i++)
            {
                tmpWidth = calculateStringWidth(tmpGraphics, this.Font, lineText, i);
                fullWidth += tmpWidth;

                if (fullWidth >= this.Width - 5)
                {
                    if (this.selectionStartIndex < (i - 1))
                        return false;
                    else
                        lastIndex = i;
                    break;
                }

            }
            fullWidth = 0;
            for (int j = lastIndex; j <= lineText.Length; j++)
            {
                tmpWidth = calculateStringWidth(tmpGraphics, this.Font, lineText, j);

                if (fullWidth >= this.Width / 3)
                {
                    this.firstViewIndex += (j - lastIndex);
                    requiredInvalidate = true;
                    break;
                }
                fullWidth += tmpWidth;
            }
            if (!requiredInvalidate)
            {
                this.firstViewIndex += (lineText.Length - lastIndex) * 2;
                requiredInvalidate = true;
            }

            tmpGraphics.Dispose();

            return requiredInvalidate;
        }

        /// <summary>
        /// 캐럿이 텍스트의 맨 끝으로 이동해야 할때 실행할 함수. 실행 전 CaretX_IsVisible 함수 실행해서 true일때는 실행하지 않음
        /// </summary>
        /// <returns>invalidate 필요 여부</returns>
        private bool CaretIsGoEnd()
        {
            float tmpWidth = 0;
            string lineText = lineInfo[this.selectionLineNumber].Text;
            float fullWidth = indentValue;
            Graphics g = this.CreateGraphics();

            if (CaretX_IsVisible())
            {
                g.Dispose();
                return false;
            }
            else
            {
                bool find = false;
                for (int i = lineText.Length; i >= 0; i--)
                {
                    tmpWidth = calculateStringWidth(g, this.Font, lineText, i);

                    if (fullWidth >= (this.Width / 3))
                    {
                        firstViewIndex = i;
                        find = true;
                        break;
                    }
                    fullWidth += tmpWidth;
                }

                fullWidth = indentValue;
                for (int i = 0; i < lineText.Length; i++)
                {
                    tmpWidth = calculateStringWidth(g, this.Font, lineText, i);
                    fullWidth += tmpWidth;
                }
                if (!find || fullWidth < this.Width)
                    firstViewIndex = 0;
            }
            g.Dispose();
            return true;
        }

        /// <summary>
        /// x축 기준으로 캐럿이 현재 화면에 보이는지 여부. firstViewIndex, selectionStartIndex 참조
        /// </summary>
        /// <returns></returns>
        private bool CaretX_IsVisible()
        {
            float tmpWidth = 0;
            float fullWidth = this.indentValue;
            float screenWidth = this.indentValue;
            //int tmpViewStartIndex = 0;
            string lineText = this.lineInfo[this.selectionLineNumber].Text;
            Graphics tmpGraphics = this.CreateGraphics();
            bool isVisible = true;

            if (this.firstViewIndex > selectionStartIndex)  //선택된 텍스트 Index가 보이는 텍스트의 첫번째 Index보다 작은 경우 //좌측기준으로 출력
            {
                isVisible = false;
            }
            else
            {
                // lastViewIndex < selectionLineIndex 
                for (int lineText_i = firstViewIndex; lineText_i <= lineText.Length; lineText_i++)
                {
                    tmpWidth = calculateStringWidth(tmpGraphics, this.Font, lineText, lineText_i);

                    // 선택된 텍스트 Index가 보이는 텍스트의 마지막 Index보다 큰 경우 // 우측 기준으로
                    if (fullWidth >= this.Width)
                    {
                        if(lineText_i <= selectionStartIndex)
                            isVisible = false;
                        break;
                    }

                    fullWidth += tmpWidth;
                }
            }
            tmpGraphics.Dispose();

            return isVisible;
        }

        /// <summary>
        /// vScroll값 및 Maximum설정. maximum값 -1 지정시 기존값 유지 할 수 있도록 할것
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maximum"></param>
        private void vScrollSet(int value, int maximum)
        {
            if (vScrollSetValue != null)
            {
                vScrollSetValue(value, maximum);
            }
        }

        /// <summary>
        /// hScroll값 및 Maximum설정. maximum값 -1 지정시 기존값 유지 할 수 있도록 할것
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maximum"></param>
        private void hScrollSet(int value, int maximum)
        {
            if (hScrollSetValue != null)
            {
                hScrollSetValue(value, maximum);
            }
        }

        /// <summary>
        /// 문자열에서 해당 인덱스의 문자 길이 계산
        /// </summary>
        /// <param name="g"></param>
        /// <param name="font"></param>
        /// <param name="lineText"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private float calculateStringWidth(Graphics g, Font font, string lineText, int index)
        {
            float tmpWidth = 0;
            if (index >= lineText.Length)
            {

            }
            else if (lineText[index].Equals(' '))
            {
                tmpWidth = getStringWidth(g, this.Font, "|");
            }
            else if (lineText[index].Equals('\t'))
            {
                tmpWidth = getStringWidth(g, this.Font, "||||");
            }
            else
            {
                tmpWidth = getStringWidth(g, this.Font, lineText[index] + "");
            }
            return tmpWidth;
        }

        /// <summary>
        /// 문자열의 길이 계산
        /// </summary>
        /// <param name="g"></param>
        /// <param name="font"></param>
        /// <param name="lineText"></param>
        /// <returns></returns>
        private float calculateStringWidth(Graphics g, Font font, string lineText)
        {
            float tmpWidth = 0;
            string tmpLineText = lineText.Replace(' ', '|').Replace("\t", "||||");
            tmpWidth = getStringWidth(g, this.Font, tmpLineText);
            return tmpWidth;
        }

        #region ** GetStringWidth

        private void getStringWidth(Graphics g, Font font, string str, int[][] arr)
        {
            StringFormat txtStringFormat = new StringFormat(StringFormat.GenericTypographic);
            CharacterRange[] ranges = new CharacterRange[arr.Length];
            for(int i=0; i < arr.Length; i++)
            {
                CharacterRange range = new CharacterRange(arr[i][0], arr[i][1]);
                ranges[i] = range;
            }

            txtStringFormat.SetMeasurableCharacterRanges(ranges);
            Region[] region = g.MeasureCharacterRanges(str, font, new RectangleF(0, 0, this.Width, this.Height), txtStringFormat);
        }
        private float getStringWidth(Graphics g, Font font, string str, int startIndex, int length)
        {
            StringFormat txtStringFormat = new StringFormat(StringFormat.GenericTypographic);
            CharacterRange[] ranges = {new CharacterRange(startIndex, length)};
            txtStringFormat.SetMeasurableCharacterRanges(ranges);
            Region[] region = g.MeasureCharacterRanges(str, font, new RectangleF(0, 0, this.Width, this.Height), txtStringFormat);
            RectangleF rectF = region[0].GetBounds(g);
            return rectF.Width;
        }

        private float getStringWidth(Graphics g, Font font, string str, int startIndex)
        {
            StringFormat txtStringFormat = new StringFormat(StringFormat.GenericTypographic);
            CharacterRange[] ranges = { new CharacterRange(startIndex, 1) };
            txtStringFormat.SetMeasurableCharacterRanges(ranges);
            Region[] region = g.MeasureCharacterRanges(str, font, new RectangleF(0, 0, this.Width, this.Height), txtStringFormat);
            RectangleF rectF = region[0].GetBounds(g);
            return rectF.Width;
        }

        private float getStringWidth(Graphics g, Font font, string str)
        {
            if (str.Length == 0)
                return 0;

            StringFormat txtStringFormat = new StringFormat(StringFormat.GenericTypographic);
            CharacterRange[] ranges = { new CharacterRange(0, str.Length) };
            txtStringFormat.SetMeasurableCharacterRanges(ranges);
            Region[] region = g.MeasureCharacterRanges(str, font, new RectangleF(0, 0, this.Width, this.Height), txtStringFormat);
            RectangleF rectF = region[0].GetBounds(g);

            //g.FillRectangle(Brushes.Aqua, rectF);
            return rectF.Width;
        }
        #endregion

        /// <summary>
        /// 현재 보이는 라인 갯수
        /// </summary>
        /// <returns></returns>
        private int getViewLinesCount()
        {
            return this.Size.Height / lineHeight;
        }

        /// <summary>
        /// 해당 라인의 SyntaxHighLight 규칙을 현재 지정되있는 규칙으로 지정한다.
        /// </summary>
        /// <param name="line"></param>
        private void LineSetRegex(Lines line)
        {
            if (keyword1Regex != null)
            {
                line.reg1 = keyword1Regex;
            }
            if (keyword2Regex != null)
            {
                line.reg2 = keyword2Regex;
            }
            if (keyword3Regex != null)
            {
                line.reg3 = keyword3Regex;
            }

            line.commentStr = comment;

            line.SyntaxHighLightUpdate();
        }
        #endregion

        #region ************************* Public Method ***********************************

        /// <summary>
        /// 해당 라인에서 첫번째 문자열이 존재하는 index를 return한다
        /// </summary>
        /// <param name="lineNum">체킹할 라인번호-1</param>
        /// <returns> 첫번째 문자열 index</returns>
        public int getCharStartIndex(int lineNum)
        {
            string lineText = this.lineInfo[lineNum].Text;
            int whiteSpaceIndex = lineText.Length - lineText.TrimStart(' ').Length;
            return whiteSpaceIndex;
        }

        /// <summary>
        /// SyntaxHighLight 재지정
        /// </summary>
        public void settingSyntaxHighLight()
        {
            if (m_keyword1.Count > 0)
            {
                string keyword1_str;
                keyword1_str = RegexData.Instance.buildKeyword(m_keyword1);
                keyword1Regex = new System.Text.RegularExpressions.Regex(keyword1_str);
            }
            if (m_keyword2.Count > 0)
            {
                string keyword2_str;
                keyword2_str = RegexData.Instance.buildKeyword(m_keyword2);
                keyword2Regex = new System.Text.RegularExpressions.Regex(keyword2_str);
            }
            if (m_keyword3.Count > 0)
            {
                string keyword3_str;
                keyword3_str = RegexData.Instance.buildKeyword(m_keyword3);
                keyword3Regex = new System.Text.RegularExpressions.Regex(keyword3_str);
            }
            foreach (Lines line in lineInfo)
            {
                LineSetRegex(line);
            }
        }

        /// <summary>
        /// 선택한 부분의 길이(Length)를 0로 지정한다.
        /// m_selectionLength = 0, selectionEndIndex = selectionStartIndex, selectionEndLineNumber = selectionLineNumber 지정
        /// </summary>
        public void selectionLengthSetZero()
        {
            m_selectionLength = 0;
            selectionEndIndex = selectionStartIndex;
            selectionEndLineNumber = selectionLineNumber;
        }

        /// <summary>
        /// 선택한 부분의 길이(Length)를 재측정 한다.
        /// selectionLineNumber, selectionEndLineNumber, selectionStartIndex, selectionEndLineNumber 사용
        /// </summary>
        public void selectionLengthCalculate()
        {
            int minLineNumber = Math.Min(this.selectionLineNumber, this.selectionEndLineNumber);
            int maxLineNumber = Math.Max(this.selectionLineNumber, this.selectionEndLineNumber);
            int minLineStartIndex = minLineNumber == this.selectionLineNumber ? this.selectionStartIndex : this.selectionEndIndex;
            int maxLineEndIndex = maxLineNumber == this.selectionLineNumber ? this.selectionStartIndex : this.selectionEndIndex;

            int textLength = 0;

            if (minLineNumber != maxLineNumber)
            {
                for (int i = minLineNumber; i <= maxLineNumber; i++)
                {
                    string lineText = lineInfo[i].Text;
                    if (i == minLineNumber)
                    {
                        textLength += lineText.Substring(minLineStartIndex).Length;
                    }
                    else if (i == maxLineNumber)
                    {
                        textLength += maxLineEndIndex;
                    }
                    else
                    {
                        textLength += lineText.Length;
                    }
                }
                textLength += (maxLineNumber - minLineNumber); // newLine개수 더하기
            }
            else
            {
                minLineStartIndex = Math.Min(this.selectionStartIndex, this.selectionEndIndex);
                maxLineEndIndex = Math.Max(this.selectionStartIndex, this.selectionEndIndex);
                string lineText = lineInfo[minLineNumber].Text;
                textLength += (maxLineEndIndex - minLineStartIndex);
            }
            this.m_selectionLength = textLength;
            //Console.WriteLine("[selectionLengthCalculate]" + "Min:(" + minLineNumber + "," + minLineStartIndex + "), Max:(" + maxLineNumber + "," + maxLineEndIndex + "), SelectionLength : " + textLength);
        }

        /// <summary>
        /// 해당 라인의 해당 인덱스가 보이도록 스크롤을 이동시킨다.
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <param name="index"></param>
        public void setScrollByLineAndIndex(int lineNumber, int index)
        {
            #region 세로 스크롤 조정
            if (firstViewLine > lineNumber)
            {
                LineLocateScreenByFraction(lineNumber, 1, 3);
            }
            else if (lastViewLine <= lineNumber)
            {
                LineLocateScreenByFraction(lineNumber, 2, 3);
            }
            #endregion

            IndexLocateScreenDefault(lineNumber, index);
        }

        /// <summary>
        /// 들여쓰기
        /// </summary>
        public void applyIndent()
        {
            insertUndoData();
            #region 들여쓰기
            if (this.selectionLineNumber == this.selectionEndLineNumber)
            {
                if (this.m_selectionLength > 0)
                {
                    this.selectedTextRemove(false);
                }

                #region 기본 TAB 효과
                string lineText = lineInfo[this.selectionLineNumber].Text;
                string prevString = lineText.Substring(0, this.selectionStartIndex);
                string nextString = lineText.Substring(this.selectionStartIndex, lineText.Length - this.selectionStartIndex);
                string appendText = isAllowTab ? "\t" : "    ";
                lineInfo[this.selectionLineNumber].Text = prevString + appendText + nextString;
                this.selectionStartIndex += appendText.Length;

                this.CaretIsGoRight();
                #endregion
                this.selectionEndLineNumber = this.selectionLineNumber;
                this.selectionEndIndex = this.selectionStartIndex;
            }
            else //여러줄 선택시 들여쓰기
            {
                string appendText = isAllowTab ? "\t" : "    ";
                int minLineNum = Math.Min(this.selectionLineNumber, this.selectionEndLineNumber);
                int maxLineNum = Math.Max(this.selectionLineNumber, this.selectionEndLineNumber);
                for (int lineNum = minLineNum; lineNum <= maxLineNum; lineNum++)
                {
                    this.lineInfo[lineNum].textInsertAt(appendText, 0);
                }
                this.selectionStartIndex += appendText.Length;
                this.selectionEndIndex += appendText.Length;

                this.selectionLengthCalculate();
            }
            #endregion
            CaretIsGoEnd();
        }

        /// <summary>
        /// 내어쓰기
        /// </summary>
        public void applyOutdent()
        {
            insertUndoData();
            #region 내어쓰기
            int removeWhiteSpaceLength = 4; //공백 제거할 Length
            if (this.selectionLineNumber == this.selectionEndLineNumber)
            {
                string lineText = this.lineInfo[this.selectionLineNumber].Text;
                int whiteSpaceIndex = lineText.Length - lineText.TrimStart(new Char[]{'\t',' '}).Length;

                //현재 라인의 첫번째 글자의 Index에 캐럿이 존재할 경우
                if (this.selectionStartIndex == whiteSpaceIndex)
                {
                    //int moc = whiteSpaceIndex / removeWhiteSpaceLength;
                    //int etc = whiteSpaceIndex % removeWhiteSpaceLength;
                    //if (moc != 0 && etc == 0)
                    //    moc--;
                    //StringBuilder tmpSb = new StringBuilder();
                    //for (int i = 0; i < moc * removeWhiteSpaceLength; i++)
                    //{
                    //    tmpSb.Append(" ");
                    //}
                    //this.lineInfo[this.selectionLineNumber].Text = tmpSb.ToString() + lineText.Substring(whiteSpaceIndex);
                    //this.selectionStartIndex = moc * removeWhiteSpaceLength;
                    //this.selectionLengthSetZero();

                    int lineTextLength = lineText.Length;
                    int minusIndexCount = 0;
                    for (int i = 0; i < lineTextLength; i++)
                    {
                        if (i >= removeWhiteSpaceLength)
                            break;
                        if (lineText[0].Equals(' '))
                        {
                            lineText = lineText.Substring(1);
                            minusIndexCount++;
                        }
                        else if (lineText[0].Equals('\t'))
                        {
                            lineText = lineText.Substring(1);
                            minusIndexCount++;
                            break;
                        }
                    }
                    this.lineInfo[this.selectionLineNumber].Text = lineText;
                    this.selectionStartIndex = Math.Max(0, this.selectionStartIndex - minusIndexCount);
                    this.selectionLengthSetZero();
                }
            }
            else //여러줄 선택시
            {
                int minLineNum = Math.Min(this.selectionLineNumber, this.selectionEndLineNumber);
                int maxLineNum = Math.Max(this.selectionLineNumber, this.selectionEndLineNumber);

                for (int lineNum = minLineNum; lineNum <= maxLineNum; lineNum++)
                {
                    string lineText = this.lineInfo[lineNum].Text;
                    int lineTextLength = lineText.Length;
                    int minusIndexCount = 0;
                    for (int i = 0; i < lineTextLength; i++)
                    {
                        if (i >= removeWhiteSpaceLength)
                            break;
                        if (lineText[0].Equals(' '))
                        {
                            lineText = lineText.Substring(1);
                            minusIndexCount++;
                        }
                        else if (lineText[0].Equals('\t'))
                        {
                            lineText = lineText.Substring(1);
                            minusIndexCount++;
                            break;
                        }
                    }
                    this.lineInfo[lineNum].Text = lineText;
                    if (lineNum == this.selectionLineNumber)
                    {
                        this.selectionStartIndex = Math.Max(0, this.selectionStartIndex - minusIndexCount);
                    }
                    else if (lineNum == this.selectionEndLineNumber)
                    {
                        this.selectionEndIndex = Math.Max(0, this.selectionEndIndex - minusIndexCount);
                    }
                }
                this.selectionLengthCalculate();
            }
            #endregion
            //CaretIsGoLeft();
            CaretIsGoEnd();
        }

        /// <summary>
        /// 주석처리
        /// </summary>
        public void applyComment()
        {
            insertUndoData();
            #region 주석처리
            int minLineNum = Math.Min(this.selectionLineNumber, this.selectionEndLineNumber);
            int maxLineNum = Math.Max(this.selectionLineNumber, this.selectionEndLineNumber);

            for (int lineNum = minLineNum; lineNum <= maxLineNum; lineNum++)
            {
                string lineText = this.lineInfo[lineNum].Text;
                int whiteSpaceLength = lineText.Length - lineText.TrimStart().Length;
                this.lineInfo[lineNum].textInsertAt("//", whiteSpaceLength);
            }
            this.selectionLineNumber = maxLineNum;
            this.selectionEndLineNumber = minLineNum;
            this.selectionStartIndex = this.lineInfo[maxLineNum].Text.Length;
            this.selectionEndIndex = 0;
            this.selectionLengthCalculate();
            #endregion
            //CaretIsGoEnd();
            //CaretIsGoRight();
            setScrollByLineAndIndex(this.selectionLineNumber, this.selectionStartIndex);
        }

        /// <summary>
        /// 주석처리 해제
        /// </summary>
        public void applyUnComment()
        {
            insertUndoData();
            #region 주석처리해제
            int minLineNum = Math.Min(this.selectionLineNumber, this.selectionEndLineNumber);
            int maxLineNum = Math.Max(this.selectionLineNumber, this.selectionEndLineNumber);

            for (int lineNum = minLineNum; lineNum <= maxLineNum; lineNum++)
            {
                string lineText = this.lineInfo[lineNum].Text;
                int whiteSpaceLength = lineText.Length - lineText.TrimStart().Length;
                if (lineText.Length - whiteSpaceLength > 1)
                {
                    if (lineText.Substring(whiteSpaceLength, 2).Equals("//"))
                    {
                        this.lineInfo[lineNum].textRemoveAt(whiteSpaceLength, 2);
                    }
                }
            }
            this.selectionLineNumber = maxLineNum;
            this.selectionEndLineNumber = minLineNum;
            this.selectionStartIndex = this.lineInfo[maxLineNum].Text.Length;
            this.selectionEndIndex = 0;
            selectionLengthCalculate();
            #endregion

            //CaretIsGoLeft();
            setScrollByLineAndIndex(this.selectionLineNumber, this.selectionStartIndex);
        }

        /// <summary>
        /// 세로스크롤의 위치값 현재 캐럿이 존재하는 라인으로 지정
        /// </summary>
        /// <param name="invalidate"></param>
        public void CaretY_IsGoCurrentVerticalView(bool invalidate)
        {
            if (this.firstViewLine > this.selectionLineNumber)
                this.firstViewLine = this.selectionLineNumber;
            if (this.selectionLineNumber > this.lastViewLine)
                this.CaretIsLocateScreenByFraction(1, 1);

            if (invalidate)
            {
                this.Invalidate();
                this.Update();
            }
        }

        /// <summary>
        /// 현재 캐럿이 존재하는 부분의 근처에 있는 텍스트를 출력한다.
        /// </summary>
        /// <returns></returns>
        public string getCaretNearString()
        {
            string nearStr = string.Empty;

            string lineText = this.lineInfo[this.selectionLineNumber].Text;
            int[] startEndIndex = RegexData.Instance.getWordStartEndIndex(lineText, this.selectionStartIndex, m_wordSeparator_pattern);
            nearStr = lineText.Substring(startEndIndex[0], startEndIndex[1] - startEndIndex[0]);
            return nearStr;
        }


        /// <summary>
        /// 현재 선택된 단어를 return한다.
        /// </summary>
        /// <returns>여러줄 Select시 return string.empty</returns>
        public string getSelectedWord()
        {
            string selectedWord = string.Empty;

            //int minLineNumber = Math.Min(selectionLineNumber, selectionEndLineNumber);
            //int maxLineNumber = Math.Max(selectionLineNumber, selectionEndLineNumber);
            //int minLineStartIndex = minLineNumber == selectionLineNumber ? selectionStartIndex : selectionEndIndex;
            //int maxLineEndIndex = maxLineNumber == selectionLineNumber ? selectionStartIndex : selectionEndIndex;

            if (selectionLineNumber == selectionEndLineNumber)
            {
                string lineText = this.lineInfo[this.selectionLineNumber].Text;
                int minLineStartIndex = Math.Min(selectionStartIndex, selectionEndIndex);
                int maxLineEndIndex = Math.Max(selectionStartIndex, selectionEndIndex);
                selectedWord = lineText.Substring(minLineStartIndex, maxLineEndIndex - minLineStartIndex);
            }
            return selectedWord;
        }

        /// <summary>
        /// 현재 선택된 단어의 StartIndex와 EndIndex를 retun한다.
        /// </summary>
        /// <returns>여러줄 Select시 return {-1,-1}</returns>
        public int[] getSelectedWordStartEndIndex()
        {
            int[] startEndIndex= new int[]{-1,-1};

            int minLineNumber = Math.Min(selectionLineNumber, selectionEndLineNumber);
            int maxLineNumber = Math.Max(selectionLineNumber, selectionEndLineNumber);
            //int minLineStartIndex = minLineNumber == selectionLineNumber ? selectionStartIndex : selectionEndIndex;
            //int maxLineEndIndex = maxLineNumber == selectionLineNumber ? selectionStartIndex : selectionEndIndex;

            if (selectionLineNumber == selectionEndLineNumber)
            {
                string lineText = this.lineInfo[this.selectionLineNumber].Text;
                int minLineStartIndex = Math.Min(selectionStartIndex, selectionEndIndex);
                int maxLineEndIndex = Math.Max(selectionStartIndex, selectionEndIndex);
                startEndIndex = new int[]{minLineStartIndex, maxLineEndIndex};
            }
            return startEndIndex;
        }

        /// <summary>
        /// 현재 캐럿이 존재하는 부분의 근처에 있는 텍스트를 출력한다. '.' 구분자 텍스트로 인식
        /// </summary>
        /// <returns></returns>
        public string getCaretNearStringIncludeDot()
        {
            string nearStr = string.Empty;

            string lineText = this.lineInfo[this.selectionLineNumber].Text;
            int[] startEndIndex = RegexData.Instance.getWordStartEndIndex(lineText, this.selectionStartIndex, m_methodSeparator_pattern);
            nearStr = lineText.Substring(startEndIndex[0], startEndIndex[1] - startEndIndex[0]);
            return nearStr;
        }

        /// <summary>
        /// "  container.popscreen(" 와 같은 문장 입력시 container.popscreen만 추출한다.
        /// </summary>
        /// <returns>object[0] = int[2] startEndIndex, object[1] = string word</returns>
        public object[] getMethodByOpenBracketStartEndIndex()
        {
            string nearStr = string.Empty;
            int[] startEndIndex = new int[2]{-1,-1};

            string lineText = this.lineInfo[this.selectionLineNumber].Text;
            lineText = lineText.Substring(0, this.selectionStartIndex);
            string pattern = @"[\(\)]";

            int openBracketCount = -1;
            System.Text.RegularExpressions.Match match;
            for (match = System.Text.RegularExpressions.Regex.Match(lineText, pattern, System.Text.RegularExpressions.RegexOptions.RightToLeft); match.Success; match = match.NextMatch())
            {
                if (this.lineInfo[this.selectionLineNumber].m_textType[match.Index] == Lines.TextType.PlainText)
                {
                    if (match.Value.Equals("("))
                        openBracketCount++;
                    else if (match.Value.Equals(")"))
                        openBracketCount--;
                }
                if (openBracketCount == 0)
                {
                    startEndIndex = RegexData.Instance.getWordStartEndIndex(lineText.Substring(0, match.Index), match.Index, @"[^A-Za-z0-9\.]");
                    nearStr = lineText.Substring(startEndIndex[0], startEndIndex[1] - startEndIndex[0]);
                    break;
                }
            }
            return new object[]{startEndIndex, nearStr};
            //return nearStr;

        }

        /// <summary>
        /// 현재 캐럿이 존재하는 부분의 근처에 있는 텍스트의 startIndex밑 EndIndex를 출력한다.
        /// </summary>
        /// <returns></returns>
        public int[] getCaretNearStringStartEndIndex()
        {
            string lineText = this.lineInfo[this.selectionLineNumber].Text;
            int[] startEndIndex = RegexData.Instance.getWordStartEndIndex(lineText, this.selectionStartIndex, m_wordSeparator_pattern);
            return startEndIndex;
        }

        /// <summary>
        /// 현재 캐럿이 존재하는 부분의 근처에 있는 텍스트의 startIndex밑 EndIndex를 출력한다. '.' 구분자 텍스트로 인식
        /// </summary>
        /// <returns></returns>
        public int[] getCaretNearStringStartEndIndexIncludeDot()
        {
            string lineText = this.lineInfo[this.selectionLineNumber].Text;
            int[] startEndIndex = RegexData.Instance.getWordStartEndIndex(lineText, this.selectionStartIndex, m_methodSeparator_pattern);
            return startEndIndex;
        }

        /// <summary>
        /// 해당 라인의 해당 StartIndex~EndIndex 사이의 텍스트를 입력한 텍스트로 변경한다.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        public void replaceText(int lineNumber, int startIndex, int endIndex, string text)
        {
            this.insertUndoData(); //Undo 저장
            string lineText = this.lineInfo[lineNumber].Text;
            string prevText = lineText.Substring(0, startIndex);
            string nextText = lineText.Substring(endIndex);
            this.lineInfo[lineNumber].Text = prevText + text + nextText;
            this.selectionStartIndex = prevText.Length + text.Length;
            this.selectionLengthSetZero();
            //this.CaretY_IsGoCurrentVerticalView(false);
            //this.CaretIsGoEnd();
            setScrollByLineAndIndex(lineNumber, selectionStartIndex);
        }

        
        /// <summary>
        /// 텍스트에 존재하는 모든 findWord를 ReplaceWord로 변경한다.
        /// </summary>
        public void replaceAllWord(string findWord, string replaceWord, int replaceCase)
        {
            this.insertUndoData();
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(RegexData.Instance.ConvertRegexWordToPlainText(findWord));
            System.Text.RegularExpressions.MatchCollection matchs;
            System.Text.RegularExpressions.Match match;
            switch (replaceCase)
            {
                case 0: //모든 라인 검색 후 변경
                    for (int i = 0; i < lineInfo.Count; i++)
                    {
                        string lineText = lineInfo[i].Text;

                        matchs = regex.Matches(lineText);
                        bool find = false;
                        for (int matchIndex = matchs.Count - 1; matchIndex >= 0; matchIndex--)
                        {
                            match = matchs[matchIndex];
                            string prevText = lineText.Substring(0, match.Index);
                            string nextText = lineText.Substring(match.Index + match.Length);
                            lineText = prevText + replaceWord + nextText;
                            find = true;
                        }
                        if (find)
                            lineInfo[i].Text = lineText;
                    }
                    this.selectionStartIndex = 0;
                    this.selectionLengthSetZero();
                    break;
                case 1: //선택 영역만 검색 후 변경
                    int searchStartLine = this.selectionLineNumber;
                    int searchEndLine = this.selectionEndLineNumber;
                    int searchStartIndex;
                    int searchEndIndex;
                    if (searchStartLine == searchEndLine)
                    {
                        searchStartIndex = Math.Min(this.selectionStartIndex, this.selectionEndIndex);
                        searchEndIndex = Math.Max(this.selectionStartIndex, this.selectionEndIndex);
                    }
                    else
                    {
                        int minLine = Math.Min(searchStartLine, searchEndLine);
                        int maxLine = Math.Max(searchStartLine, searchEndLine);
                        searchStartIndex = minLine == searchStartLine ? this.selectionStartIndex : this.selectionEndLineNumber;
                        searchEndIndex = maxLine == searchStartLine ? this.selectionStartIndex : this.selectionEndLineNumber;
                        searchStartLine = minLine;
                        searchEndLine = maxLine;
                    }

                    for (int i = searchStartLine; i <= searchEndLine; i++)
                    {
                        string lineText = lineInfo[i].Text;

                        matchs = regex.Matches(lineText);
                        bool find = false;
                        for (int matchIndex = matchs.Count - 1; matchIndex >= 0; matchIndex--)
                        {
                            match = matchs[matchIndex];
                            if (i == searchStartLine)
                            {
                                if (match.Index < searchStartIndex)
                                    continue;
                            }
                            else if (i == searchEndLine)
                            {
                                if (match.Index >= searchEndIndex)
                                    continue;
                            }
                            
                            string prevText = lineText.Substring(0, match.Index);
                            string nextText = lineText.Substring(match.Index + match.Length);
                            lineText = prevText + replaceWord + nextText;
                            find = true;
                        }
                        if (find)
                            lineInfo[i].Text = lineText;
                    }
                    this.selectionEndLineNumber = searchStartLine;
                    this.selectionLineNumber = searchEndLine;
                    this.selectionEndIndex = 0;
                    this.selectionStartIndex = lineInfo[searchEndLine].Text.Length;
                    break;
            }
            this.Invalidate();
            this.Update();
        }

        /// <summary>
        /// 특정 라인의 특정 Index에 해당 텍스트를 추가한다. 현재 선택된 텍스트가 있다면 자동 제거
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <param name="index"></param>
        /// <param name="text"></param>
        public void InsertText(int lineNumber, int index, string text)
        {
            this.insertUndoData();

            if (this.m_selectionLength != 0)
                selectedTextRemove(false);
            
            this.lineInfo[lineNumber].textInsertAt(text, index);
            this.selectionStartIndex += text.Length;
            this.selectionLengthSetZero();
            CaretIsGoRight();
            this.Invalidate();
            this.Update();
        }

        /// <summary>
        /// 입력받은 텍스트를 라인에 추가한다.
        /// </summary>
        /// <param name="text"></param>
        public void InsertTextToLastLine(string pasteText)
        {
            if (!string.IsNullOrEmpty(pasteText))
                insertUndoData();
            #region 붙여넣기

            if(!isAllowTab)
                pasteText = pasteText.Replace("\t", "    ");

            int insertLineAt = this.lineInfo.Count;
            Lines line =  new Lines();
            LineSetRegex(line);
            this.lineInfo.Add(line);

            StringBuilder tmpSB = new StringBuilder();
            for (int i = 0; i <= pasteText.Length; i++)
            {
                if (i == pasteText.Length) // 붙여넣기 마지막 문자일 경우
                {
                    int endIndex = tmpSB.ToString().Length;
                    line.Text += (tmpSB.ToString());

                    this.selectionStartIndex = endIndex;
                    this.selectionLineNumber = insertLineAt;
                    this.selectionLengthSetZero();
                    setScrollByLineAndIndex(insertLineAt, endIndex);
                    
                    this.Invalidate();
                    this.Update();
                }
                else if (i + 1 != pasteText.Length ? pasteText[i].Equals('\r') && pasteText[i + 1].Equals('\n') : false) // New Line 발생 시
                {
                    i++;
                    insertLineAt++;
                    line.Text += tmpSB.ToString();
                    tmpSB.Clear();
                    line = new Lines();
                    LineSetRegex(line);
                    lineInfo.Insert(insertLineAt, line);
                }
                else
                {
                    //line.Text += pasteText[i];
                    tmpSB.Append(pasteText[i] + "");
                }
            }



            #endregion
        }

        /// <summary>
        /// 해당 문자가 그려지기 시작하는 위치값 Point를 가져온다.
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <param name="indexNumberOfLine"></param>
        /// <returns></returns>
        public Point getDrawPoint(int lineNumber, int indexNumberOfLine)
        {
            string lineText = this.lineInfo[lineNumber].Text;
            Graphics g = this.CreateGraphics();
            float tmpWidth = 0;
            float fullWidth = this.indentValue;
            for (int i = firstViewIndex; i <= lineText.Length; i++)
            {
                if (indexNumberOfLine.Equals(i))
                {
                    g.Dispose();
                    return new Point((int)fullWidth, (lineNumber-firstViewLine) * lineHeight);
                }
                tmpWidth = this.calculateStringWidth(g, this.Font, lineText, i);
                
                fullWidth += tmpWidth;
            }
            g.Dispose();
            return new Point(-1,-1);
        }

        /// <summary>
        /// 현재 선택된 텍스트를 ClipBoard에 복사합니다.
        /// </summary>
        public void Copy()
        {
            #region Copy Effect
            int minLineNumber = Math.Min(selectionLineNumber, selectionEndLineNumber);
            int maxLineNumber = Math.Max(selectionLineNumber, selectionEndLineNumber);
            int minLineStartIndex = minLineNumber == selectionLineNumber ? selectionStartIndex : selectionEndIndex;
            int maxLineEndIndex = maxLineNumber == selectionLineNumber ? selectionStartIndex : selectionEndIndex;


            StringBuilder stringSumLineBuilder = new StringBuilder();
            if (minLineNumber != maxLineNumber)
            {
                string lineText = string.Empty;

                for (int i = minLineNumber; i <= maxLineNumber; i++)
                {
                    if (i == minLineNumber)
                    {
                        lineText = this.lineInfo[i].Text;
                        stringSumLineBuilder.AppendLine(lineText.Substring(minLineStartIndex));
                    }
                    else if (i == maxLineNumber)
                    {
                        lineText = this.lineInfo[i].Text;
                        //if (lineText == string.Empty)
                        //    stringSumLineBuilder.Append(string.Empty);
                        //else
                        if(lineText != string.Empty)
                            stringSumLineBuilder.Append(lineText.Substring(0, maxLineEndIndex));

                    }
                    else
                    {
                        lineText = this.lineInfo[i].Text;
                        stringSumLineBuilder.AppendLine(lineText);
                    }
                }
                if (!string.IsNullOrEmpty(stringSumLineBuilder.ToString()))
                    Clipboard.SetText(stringSumLineBuilder.ToString());
                else
                    Clipboard.SetText(System.Environment.NewLine);
            }
            else
            {
                if (this.m_selectionLength != 0)
                {
                    minLineStartIndex = Math.Min(selectionStartIndex, selectionEndIndex);
                    maxLineEndIndex = Math.Max(selectionStartIndex, selectionEndIndex);

                    string lineText = this.lineInfo[minLineNumber].Text;
                    string copyText = lineText.Substring(minLineStartIndex, maxLineEndIndex - minLineStartIndex);
                    if (!string.IsNullOrEmpty(copyText))
                        Clipboard.SetText(copyText);
                    else
                        Clipboard.SetText(System.Environment.NewLine);
                }
            }
            #endregion
        }

        /// <summary>
        /// 현재 선택된 텍스트를 ClipBoard에 복사 후 제거합니다.
        /// </summary>
        public void Cut()
        {
            this.insertUndoData();
            this.Copy();
            this.selectedTextRemove(true);

            call_NormalTextChanged();
        }

        /// <summary>
        /// 현재 선택된 텍스트에 ClipBoard의 복사된 텍스트를 붙여넣습니다.
        /// </summary>
        public void Paste()
        {
            string pasteText = Clipboard.GetText(TextDataFormat.Text);
            if (!string.IsNullOrEmpty(pasteText))
                insertUndoData();
            if (this.m_selectionLength > 0)
            {
                this.selectedTextRemove(false);
            }

            #region 붙여넣기

            if(!isAllowTab)
                pasteText = pasteText.Replace("\t", "    ");

            string lineText = this.lineInfo[this.selectionLineNumber].Text;
            string prevText = lineText.Substring(0, this.selectionStartIndex);
            string nextText = lineText.Substring(this.selectionStartIndex);

            int insertLineAt = this.selectionLineNumber; //끼워넣기를 할 줄

            Lines line = this.lineInfo[this.selectionLineNumber];
            StringBuilder tmpSB = new StringBuilder();
            for (int i = 0; i <= pasteText.Length; i++)
            {
                if (i == pasteText.Length) // 붙여넣기 마지막 문자일 경우
                {
                    if (insertLineAt == this.selectionLineNumber) //한줄에 붙여넣기 했을 경우
                    {
                        this.selectionStartIndex = prevText.Length + tmpSB.ToString().Length;
                        line.Text = prevText + tmpSB.ToString() + nextText;
                    }
                    else
                    {
                        this.selectionLineNumber = insertLineAt;
                        //this.selectionStartIndex = line.Text.Length;
                        this.selectionStartIndex = tmpSB.ToString().Length;
                        line.Text += (tmpSB.ToString() + nextText);
                    }
                }
                else if (i + 1 != pasteText.Length ? pasteText[i].Equals('\r') && pasteText[i + 1].Equals('\n') : false) // New Line 발생 시
                {
                    i++;
                    insertLineAt++;
                    line.Text += tmpSB.ToString();
                    tmpSB.Clear();
                    line = new Lines();
                    LineSetRegex(line);
                    lineInfo.Insert(insertLineAt, line);
                }
                else
                {
                    //line.Text += pasteText[i];
                    tmpSB.Append(pasteText[i] + "");
                }
            }
            
            

            #endregion

            #region firstViewLine 및 firstViewIndex 값 지정
            if (!(firstViewLine <= this.selectionLineNumber && lastViewLine > this.selectionLineNumber))
            {
                CaretIsLocateScreenByFraction(2, 3);
            }
            //if (CaretIsGoEnd())
            //    CaretIsGoRight();
            CaretIsGoEnd();

            #endregion

            selectionLengthSetZero();
            this.Invalidate();
            this.Update();
        }

        /// <summary>
        /// 전체 텍스트를 선택합니다.
        /// </summary>
        public void SelectAllText()
        {
            #region SelectAll Effect
            #region //selectionLineNumber가 최상단
            //this.firstViewLine = 0;
            //this.firstViewIndex = 0;
            //this.selectionLineNumber = 0;
            //this.selectionStartIndex = 0;
            //this.selectionEndLineNumber = this.lineInfo.Count - 1;
            //this.selectionEndIndex = this.lineInfo[this.selectionEndLineNumber].Text.Length;
            #endregion

            #region selectionLineNumber가 최하단
            this.selectionLineNumber = this.lineInfo.Count - 1;
            this.selectionStartIndex = this.lineInfo[this.lineInfo.Count-1].Text.Length;
            this.selectionEndLineNumber = 0;
            this.selectionEndIndex = 0;
            this.CaretIsLocateScreenByFraction(1, 1);
            #endregion

            this.m_selectionLength = this.Text.Length;

            #endregion

            this.Invalidate();
            this.Update();
        }


        /// <summary>
        /// UndoStack에 쌓여있는 실행가능한 Undo 개수를 Return합니다
        /// </summary>
        public int UndoStackCount
        {
            get
            {
                return this.undoStack.Count;
            }
        }

        /// <summary>
        /// RedoStack에 쌓여있는 실행가능한 Redo 개수를 Return합니다
        /// </summary>
        public int RedoStackCount
        {
            get
            {
                return this.redoStack.Count;
            }
        }

        /// <summary>
        /// UndoStack에서 데이터를 빼와서 Undo
        /// </summary>
        public void Undo()
        {
            if (this.undoStack.Count > 0)
            {
                //undo 실행전 Redo정보 저장
                insertRedoData();

                txtStatusData tmpData = this.undoStack.Pop();
                applytxtStatusData(tmpData);
            }
            if (this.undoStack.Count == 0 ? !isDefaultText : false)
            {
                isDefaultText = true;
                fixedSelectionLine = -1;
                this.Invalidate();
                this.Update();
                //Console.WriteLine("[Undo] 텍스트 초기화");
            }
        }


        /// <summary>
        /// RedoStack에서 데이터를 빼와서 Redo
        /// </summary>
        public void Redo()
        {
            if (this.redoStack.Count > 0)
            {
                insertUndoData();

                txtStatusData tmpData = this.redoStack.Pop();
                applytxtStatusData(tmpData);
            }
        }

        #endregion

        /// <summary>
        /// 전체 텍스트
        /// </summary>
        /// <returns></returns>
        public override string Text
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < this.lineInfo.Count; i++)
                {
                    if (i == this.lineInfo.Count - 1)
                    {
                        sb.Append(this.lineInfo[i].Text);
                    }
                    else
                    {
                        sb.Append(this.lineInfo[i].Text + System.Environment.NewLine);
                    }
                }
                return sb.ToString();
            }
            set
            {
                this.lineInfo.Clear();
                string tmpStr = isAllowTab ? value : value.Replace("\t", "    ");

                Lines line = new Lines();
                StringBuilder tmpSB = new StringBuilder();
                lineInfo.Add(line);
                for (int i = 0; i < tmpStr.Length; i++)
                {
                    if (i+1 != tmpStr.Length ? tmpStr[i].Equals('\r')  && tmpStr[i + 1].Equals('\n'): false)
                    {
                        i++;
                        line.Text = tmpSB.ToString();
                        tmpSB.Clear();
                        line = new Lines();
                        lineInfo.Add(line);
                    }
                    else
                    {
                        tmpSB.Append(tmpStr[i]+"");
                    }
                }
                line.Text += tmpSB.ToString();


                #region //unused
                //byte[] stringBytes = Encoding.UTF8.GetBytes(tmpStr);
                //MemoryStream mStream = new MemoryStream(stringBytes);
                //StreamReader sr = new StreamReader(mStream);

                //int count = 0;
                //Console.Write("111");
                //while (sr.Peek() > -1)
                //{
                //    Lines line = new Lines(ForeColor);
                //    line.Text = sr.ReadLine();
                //    lineInfo.Add(line);
                //    count++;
                //    Console.WriteLine(line.Text);
                //}
                //Console.WriteLine("lineCount:" + count);
                //sr.Close();
                //mStream.Close();

                #endregion

                this.settingSyntaxHighLight();

                if (isLoaded)
                {
                    this.isDefaultText = false;
                    //this.selectionLineNumber = this.lineInfo.Count-1;
                    //this.selectionStartIndex = this.lineInfo[this.lineInfo.Count - 1].Text.Length;
                    //this.selectionLengthSetZero();
                    this.Invalidate();
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // LinesTextBox
            // 
            this.BackColor = System.Drawing.Color.White;
            this.Font = new System.Drawing.Font("굴림", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Name = "LinesTextBox";
            this.ResumeLayout(false);

        }
    }
}
