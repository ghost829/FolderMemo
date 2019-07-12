using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

//Comment1 : Resize시 깜빡임현상 존재함. LockWindowUpdate로 미해결. -130913-

namespace CustomControlLibrary
{
    public partial class CustomForm : Form
    {
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool LockWindowUpdate(IntPtr hWndLock);
        

        #region ## VARIABLES

        //This gives us the ability to drag the borderless form to a new location
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public const int WM_SYSCOMMAND = 0x0112;

        public const int WM_MOUSEMOVE = 0x0200;

        
        private Point startPos = Point.Empty; //폼이 이동하기 전에 클릭한 마우스위치
        private bool isFormDraging = false; //폼이 이동중일때 true
        private Rectangle sizeBorder; // 커서 이미지 변경할(Sizable Cursor) 범위사각형 지정
        private int addVal = 7; //폼 두께 범위


        private int m_topBorderLine; // Border부 Bottom Line
        private int m_radius; // 폼의 꼭짓점 뭉툭함 지정
        
        private PointF m_titleLocation; // 제목의 위치 지정. Default값 Constructor에 정의
        private Point m_iconLocation;// 아이콘 위치값
        private bool m_showIconInTitleBox; // 제목표시줄에 아이콘 표시 여부
        private bool m_titleHidden; // 제목을 제목표시줄에서 숨길것인지 여부
        private Color m_titleboxColor; //제목표시줄 뒷배경
        private Font m_titleFont; // 제목 폰트 지정
        private StringFormat m_titleStringFormat; // 제목 정렬 지정
        
        private RectangleCorners m_vertexDirection; // 꼭짓점 뭉툭할 방향지정
        private Color m_titleColor; // 제목 색상
        private bool m_formSizable; // 폼 사이즈 변경 가능 여부



        private Rectangle closeButtonRect; //닫기버튼 위치
        private bool isCloseFormButtonMouseDown = false; //닫기버튼 눌렀을때 true
        private Size m_btnsSize; // 닫기 버튼 크기
        private Image m_closeBtnNormalImage; //닫기 버튼 노멀 이미지
        private Image m_closeBtnHighLightImage; //닫기 버튼 하이라이트 이미지
        private Image m_closeBtnMouseOverImage; //닫기 버튼에 마우스 오버시 이미지
        private bool  m_closeBtnVisible; // 닫기버튼 Visible여부

        private Rectangle minimizeButtonRect; //최소화버튼 위치
        private bool isMinimizeFormButtonMouseDown = false; //최소화버튼 눌렀을때 true
        private bool m_minimizeButtonVisible; // 최소화 버튼 Visible여부
        private Image m_minimizeBtnNormalImage; // 최소화 버튼 노멀 이미지
        private Image m_minimizeBtnHighLightImage; //최소화 버튼 하이라이트 이미지
        private Image m_minimizeBtnMouseOverImage; //최소화 버튼 마우스 오버시 이미지

        private Rectangle maximizeButtonRect; //최대화버튼 위치
        private bool isMaximizeFormButtonMouseDown = false; //최대화버튼 눌렀을때 true
        private bool m_maximizeButtonVisible; // 최대화 버튼 Visible여부
        private Image m_maximizeBtnNormalImage; // 최대화 버튼 노멀 이미지
        private Image m_maximizeBtnHighLightImage; // 최대화 버튼 하이라이트 이미지
        private Image m_maximizeBtnMouseOverImage; // 최대화 버튼 마우스 오버시 이미지
        private Image m_restoreBtnNormalImage; // 최대화 상태일때 복구 버튼 노멀 이미지
        private Image m_restoreBtnHighLightImage; // 최대화 상태일때 복구 버튼 하이라이트 이미지
        private Image m_restoreBtnMouseOverImage; // 최대화 상태일때 복구 버튼 마우스 오버시 이미지

        private bool isActivated; //현재 폼이 활성화 상태일때 True
        private bool isViewBorder; //현재 폼이 Border를 보이게 해놨을 경우
        private Pen m_formActivateBorderPen;
        private Pen m_formDeactivateBorderPen;
        private int m_formBorderWidth; //폼의 테두리 두께
        private FormStartPosition m_startPosition; // 폼의 시작위치

        private CustomForm parentForm; //부모 폼
        private bool m_btnsAutoLocation = true; //화면 우측 위
        private Point m_btnsLocation; //화면 우측 위 버튼의 우측상단기준으로 위치값
        private bool m_titleBoxGradient = false; //폼 제목표시줄 배경색 그라이데션 효과
        private bool m_formClosedSmooth = false; // 폼 닫힐때 부드럽게 닫히기
        private bool m_formRectangleSave = false; // 폼 닫을때 위치값 사이즈값 저장 및 폼 열을때 해당 위치값 사이즈값 으로 지정
        public string customFormRectangleKeyName = "CustomFormRectangle"; //변경해서 사용할것!!

        #region CustomWindowState
        /* FormBorder가 None 일 경우 WindowState = Maximum 시 윈도우 시작표시줄(task bar)까지 덮는 문제 존재
         * 따라서 Custom으로 WindowState 변경 효과
        */
        private enum CustomWindowState
        {
            Normal,
            Maximized,
            Minimized
        }
        private Rectangle previousMaximizeScreen; //최대화 하기 이전의 Form Rectangle
        private CustomWindowState m_thisWindowState = CustomWindowState.Normal;
        private CustomWindowState thisWindowState
        {
            get
            {
                return m_thisWindowState;
            }
            set
            {
                m_thisWindowState = value;
                if (m_thisWindowState == CustomWindowState.Normal)
                {
                    this.Size = this.previousMaximizeScreen.Size;
                    this.Location = this.previousMaximizeScreen.Location;
                }
                else if (m_thisWindowState == CustomWindowState.Maximized)
                {
                    this.previousMaximizeScreen = new Rectangle(this.Location, this.Size);
                    Screen ParentScreen = Screen.FromControl(this);
                    this.Size = ParentScreen.WorkingArea.Size;
                    this.Location = ParentScreen.WorkingArea.Location;
                }
            }
        }
        #endregion

        public enum RectangleCorners
        {
            None = 0, TopLeft = 1, TopRight = 2, BottomLeft = 4, BottomRight = 8,
            All = TopLeft | TopRight | BottomLeft | BottomRight,
            Top = TopLeft | TopRight, Bottom = BottomLeft | BottomRight
        }

        #endregion

        #region ## PROPERTIES

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(true), Category("CustomForm"), Description("폼의 제목영역과 화면영역을 구분할 구분선의 Y값을 지정합니다.")]
        public int TopBorderLine
        {
            get
            {
                return m_topBorderLine;
            }
            set
            {
                m_topBorderLine = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼의 꼭짓점 부분 뭉툭함 지정 (0:기본사각형, 1~:뭉툭함)")]
        public int VertexRadius
        {
            get
            {
                return m_radius;
            }
            set
            {
                m_radius = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼 제목표시줄 위치 지정")]
        public Point TitleLocation
        {
            get
            {
                return new Point(Convert.ToInt32( m_titleLocation.X), Convert.ToInt32(m_titleLocation.Y));
            }
            set
            {
                m_titleLocation = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("제목표시줄에 아이콘 표시 여부")]
        public bool ShowIconInTitleBox
        {
            get
            {
                return m_showIconInTitleBox;
            }
            set
            {
                m_showIconInTitleBox = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("제목표시줄에 아이콘 위치값")]
        public Point IconLocation
        {
            get
            {
                return m_iconLocation;
            }
            set
            {
                m_iconLocation = value;
                this.Invalidate();
            }
        }

        #region 버튼 공통
        [Browsable(true), Category("CustomForm"), Description("화면 우측 상단 (0,0) 기준으로 폼 우측의 버튼들의 위치값 지정[TopRight_BtnsAutoLocation값이 false일 경우 적용]")]
        public Point TopRight_BtnsLocation
        {
            get
            {
                return this.m_btnsLocation;
            }
            set
            {
                this.m_btnsLocation = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼 우측의 버튼들의 위치 자동 지정")]
        public bool TopRight_BtnsAutoLocation
        {
            get
            {
                return this.m_btnsAutoLocation;
            }
            set
            {
                this.m_btnsAutoLocation = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼 우측의 버튼들의 크기 지정")]
        public Size TopRight_BtnsSize
        {
            get
            {
                return m_btnsSize;
            }
            set
            {
                m_btnsSize = value;
                this.Invalidate();
            }
        }

        #endregion

        #region Close Btn (닫기 버튼)
        [Browsable(true), Category("CustomForm"), Description("닫기 버튼 Normal 이미지")]
        public Image TopRight_CloseBtnNormalImage
        {
            get
            {
                return m_closeBtnNormalImage;
            }
            set
            {
                m_closeBtnNormalImage = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("닫기 버튼 HighLight 이미지")]
        public Image TopRight_CloseBtnHighLightImage
        {
            get
            {
                return m_closeBtnHighLightImage;
            }
            set
            {
                m_closeBtnHighLightImage = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("닫기 버튼 MouseOver시 이미지")]
        public Image TopRight_CloseBtnMouseOverImage
        {
            get
            {
                return m_closeBtnMouseOverImage;
            }
            set
            {
                m_closeBtnMouseOverImage = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("닫기 버튼 VISIBLE 여부")]
        public Boolean TopRight_CloseBtnVisible
        {
            get
            {
                return m_closeBtnVisible;
            }
            set
            {
                m_closeBtnVisible = value;
                this.Invalidate();
            }
        }
        #endregion

        #region Minimize Btn (최소화 버튼)
        [Browsable(true), Category("CustomForm"), Description("최소화 버튼 Normal 이미지")]
        public Image TopRight_MinimizeBtnNormalImage
        {
            get
            {
                return m_minimizeBtnNormalImage;
            }
            set
            {
                m_minimizeBtnNormalImage = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("최소화 버튼 HighLight 이미지")]
        public Image TopRight_MinimizeBtnHighLightImage
        {
            get
            {
                return m_minimizeBtnHighLightImage;
            }
            set
            {
                m_minimizeBtnHighLightImage = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("최소화 버튼 MouseOver시 이미지")]
        public Image TopRight_MinimizeBtnMouseOverImage
        {
            get
            {
                return m_minimizeBtnMouseOverImage;
            }
            set
            {
                m_minimizeBtnMouseOverImage = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("최소화 버튼 VISIBLE 여부")]
        public Boolean TopRight_MinimizeButtonVisible
        {
            get
            {
                return m_minimizeButtonVisible;
            }
            set
            {
                m_minimizeButtonVisible = value;
                this.Invalidate();
            }
        }
        #endregion

        #region Maximize Btn (최대화 버튼)
        [Browsable(true), Category("CustomForm"), Description("최대화 버튼 Normal 이미지")]
        public Image TopRight_MaximizeBtnNormalImage
        {
            get
            {
                return m_maximizeBtnNormalImage;
            }
            set
            {
                m_maximizeBtnNormalImage = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("최대화 버튼 HighLight 이미지")]
        public Image TopRight_MaximizeBtnHighLightImage
        {
            get
            {
                return m_maximizeBtnHighLightImage;
            }
            set
            {
                m_maximizeBtnHighLightImage = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("최대화 버튼 MouseOver시 이미지")]
        public Image TopRight_MaximizeBtnMouseOverImage
        {
            get
            {
                return m_maximizeBtnMouseOverImage;
            }
            set
            {
                m_maximizeBtnMouseOverImage = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("최대화 복구 버튼 Normal 이미지")]
        public Image TopRight_MaximizeRestoreBtnNormalImage
        {
            get
            {
                return m_restoreBtnNormalImage;
            }
            set
            {
                m_restoreBtnNormalImage = value;
            }
        }

        [Browsable(true), Category("CustomForm"), Description("최대화 복구 버튼 HighLight 이미지")]
        public Image TopRight_MaximizeRestoreBtnHighLightImage
        {
            get
            {
                return m_restoreBtnHighLightImage;
            }
            set
            {
                m_restoreBtnHighLightImage = value;
            }
        }

        [Browsable(true), Category("CustomForm"), Description("최대화 복구 버튼 MouseOver 이미지")]
        public Image TopRight_MaximizeRestoreBtnMouseOverImage
        {
            get
            {
                return m_restoreBtnMouseOverImage;
            }
            set
            {
                m_restoreBtnMouseOverImage = value;
            }
        }

        [Browsable(true), Category("CustomForm"), Description("최대화 버튼 VISIBLE 여부")]
        public Boolean TopRight_MaximizeButtonVisible
        {
            get
            {
                return m_maximizeButtonVisible;
            }
            set
            {
                m_maximizeButtonVisible = value;
                this.Invalidate();
            }
        }
        #endregion

        [Browsable(true), Category("CustomForm"), Description("폼 제목표시줄 색상 지정")]
        public Color TitleBoxColor
        {
            get
            {
                return m_titleboxColor;
            }
            set
            {
                m_titleboxColor = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼 제목 폰트 지정")]
        public Font TitleFont
        {
            get
            {
                return m_titleFont;
            }
            set
            {
                m_titleFont = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼 제목 가로 정렬")]
        public StringAlignment TitleHorizontalAlgin
        {
            get
            {
                return m_titleStringFormat.Alignment;
            }
            set
            {
                m_titleStringFormat.Alignment = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼 제목 세로 정렬")]
        public StringAlignment TitleVerticalAlgin
        {
            get
            {
                return m_titleStringFormat.LineAlignment;
            }
            set
            {
                m_titleStringFormat.LineAlignment = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼 제목 색상 지정")]
        public Color TitleColor
        {
            get
            {
                return m_titleColor;
            }
            set
            {
                m_titleColor = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼의 꼭짓점 부분 뭉툭해질 영역 지정")]
        public RectangleCorners VertexDirection
        {
            get
            {
                return m_vertexDirection;
            }
            set
            {
                m_vertexDirection = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼이 활성화 됬을때의 테두리 색을 지정합니다")]
        public Color FormBorder_ActivateColor
        {
            get
            {
                return m_formActivateBorderPen.Color;
            }
            set
            {
                m_formActivateBorderPen = new Pen(value, m_formBorderWidth);
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼이 비활성화 됬을때의 테두리 색을 지정합니다")]
        public Color FormBorder_DeactivateColor
        {
            get
            {
                return m_formDeactivateBorderPen.Color;
            }
            set
            {
                m_formDeactivateBorderPen = new Pen(value, m_formBorderWidth);
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼의 테두리를 표시합니다")]
        public Boolean FormBorder_IsView
        {
            get
            {
                return isViewBorder;
            }
            set
            {
                isViewBorder = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼의 테두리 두께를 지정합니다")]
        public int FormBorder_Width
        {
            get
            {
                return m_formBorderWidth;
            }
            set
            {
                m_formBorderWidth = value;
                m_formActivateBorderPen = new Pen(m_formActivateBorderPen.Color, m_formBorderWidth);
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼의 사이즈 변경 여부를 지정합니다.")]
        public bool FormSizable
        {
            get
            {
                return m_formSizable;
            }
            set
            {
                m_formSizable = value;
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼의 시작 위치값을 지정합니다.")]
        public new FormStartPosition StartPosition
        {
            get
            {
                return m_startPosition;
            }
            set
            {
                m_startPosition = value;
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼 제목표시줄 배경색 그라데이션 효과 여부")]
        public bool TitleBoxGradient
        {
            get
            {
                return this.m_titleBoxGradient;
            }
            set
            {
                this.m_titleBoxGradient = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼 닫힐때 천천히 닫히는 효과")]
        public bool FormClosedSmooth
        {
            get
            {
                return this.m_formClosedSmooth;
            }
            set
            {
                this.m_formClosedSmooth = value;
            }
        }

        [Browsable(true), Category("CustomForm"), Description("폼 닫을때 위치값 및 크기값 저장, 이후 폼 실행 시 이전 위치값 및 크기값으로 셋팅")]
        public bool FormRectangleSave
        {
            get
            {
                return this.m_formRectangleSave;
            }
            set
            {
                this.m_formRectangleSave = value;
            }
        }

        [Browsable(true), Category("CustomForm"), Description("제목을 제목표시줄에서 숨길것인지 여부")]
        public bool TitleHidden
        {
            get
            {
                return m_titleHidden;
            }
            set
            {
                this.m_titleHidden = value;
                this.Invalidate();
            }
        }
        #endregion

        #region ## OVERRIDE PROPERTIES

        [Browsable(false)]
        [Bindable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool RightToLeftLayout
        {
            get;
            set;
        }

        [Browsable(false)]
        [Bindable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override RightToLeft RightToLeft
        {
            get;
            set;
        }


        [Browsable(true), Category("CustomForm"), Description("폼의 뒷배경색 지정")]
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

        [Browsable(true), Category("CustomForm"), Description("폼 제목 지정")]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                this.Invalidate();
            }
        }

        #endregion

        #region ## CONSTRUCTOR
        public CustomForm()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            InitializeComponent();

            Graphics tmpG = this.CreateGraphics();
            SizeF fontSize = tmpG.MeasureString(this.Text, this.Font);
            tmpG.Dispose();

            m_topBorderLine = 35; // Border부 Bottom Line
            m_btnsSize = new Size(35, 20);
            m_iconLocation = new Point(3, 3);
            m_titleboxColor = Color.DodgerBlue;
            m_radius = 5;
            m_titleFont = new Font("굴림", 9);
            m_closeBtnVisible = true;
            m_minimizeButtonVisible = true;
            m_maximizeButtonVisible = true;
            m_vertexDirection = RectangleCorners.Top;
            m_titleColor = Color.Black;
            m_showIconInTitleBox = false;

            m_titleLocation = new PointF(50, (m_topBorderLine / 2) - (Convert.ToInt32(fontSize.Height) / 2));
            m_titleStringFormat = new StringFormat(StringFormat.GenericDefault);
            m_titleStringFormat.Alignment = StringAlignment.Near;
            m_titleStringFormat.LineAlignment = StringAlignment.Center;

            m_formBorderWidth = 4;
            m_formActivateBorderPen = new Pen(SystemColors.HotTrack, m_formBorderWidth);
            m_formDeactivateBorderPen = new Pen(Color.FromArgb(75, SystemColors.ControlDark), m_formBorderWidth);
            m_formSizable = true;
            m_startPosition = FormStartPosition.WindowsDefaultLocation;
            this.Load += new EventHandler(custom_OnLoad);
        }

        public CustomForm(CustomForm parentForm)
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            InitializeComponent();

            Graphics tmpG = this.CreateGraphics();
            SizeF fontSize = tmpG.MeasureString(this.Text, this.Font);
            tmpG.Dispose();

            m_topBorderLine = 35; // Border부 Bottom Line
            m_btnsSize = new Size(35, 20);
            m_iconLocation = new Point(3, 3);
            m_titleboxColor = Color.DodgerBlue;
            m_radius = 5;
            m_titleFont = new Font("굴림", 9);
            m_closeBtnVisible = true;
            m_minimizeButtonVisible = true;
            m_maximizeButtonVisible = true;
            m_vertexDirection = RectangleCorners.Top;
            m_titleColor = Color.Black;
            m_showIconInTitleBox = false;

            m_titleLocation = new PointF(50, (m_topBorderLine / 2) - (Convert.ToInt32(fontSize.Height) / 2));
            m_titleStringFormat = new StringFormat(StringFormat.GenericDefault);
            m_titleStringFormat.Alignment = StringAlignment.Near;
            m_titleStringFormat.LineAlignment = StringAlignment.Center;

            m_formBorderWidth = 4;
            m_formActivateBorderPen = new Pen(SystemColors.HotTrack, m_formBorderWidth);
            m_formDeactivateBorderPen = new Pen(Color.FromArgb(75, SystemColors.ControlDark), m_formBorderWidth);
            m_formSizable = true;
            m_startPosition = FormStartPosition.WindowsDefaultLocation;
            this.Load += new EventHandler(custom_OnLoad);

            this.parentForm = parentForm;
        }

        private void custom_OnLoad(object sender, EventArgs e)
        {
            switch (this.StartPosition)
            {
                case FormStartPosition.CenterScreen:
                    this.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (this.Width / 2);
                    this.Top = (Screen.PrimaryScreen.Bounds.Height / 2) - (this.Height / 2);
                    break;
                case FormStartPosition.CenterParent:
                    if (parentForm != null)
                    {
                        this.Left = (parentForm.Location.X + (parentForm.Width / 2)) - (this.Width / 2);
                        this.Top = (parentForm.Location.Y + (parentForm.Height / 2)) - (this.Height / 2);
                        break;
                    }
                    break;
                default:
                    break;
            }
            if (this.FormRectangleSave)
            {
                string tmpStrRect = IsolatedStorageManagement.readIsolated(customFormRectangleKeyName);
                if (!string.IsNullOrEmpty(tmpStrRect))
                {
                    RectangleConverter rc = new RectangleConverter();
                    Rectangle tmpRect = (Rectangle)rc.ConvertFromString(tmpStrRect);

                    Screen[] sc = Screen.AllScreens;
                    for (int i = 0; i < sc.Length; i++)
                    {
                        if (sc[i].WorkingArea.Contains(tmpRect)) //맞는 화면 존재 시
                        {
                            this.Location = tmpRect.Location;
                            this.Size = tmpRect.Size;
                            break;
                        }
                        else if (sc[i].WorkingArea.IntersectsWith(tmpRect)) //교차하는 화면 존재 시
                        {
                            // 넓이, 높이 체크, x,y조정
                            Rectangle scRect = sc[i].Bounds;
                            if (scRect.Width < tmpRect.Width)
                                tmpRect.Width = scRect.Width;
                            if (scRect.Height < tmpRect.Height)
                                tmpRect.Height = scRect.Height;

                            if (scRect.Left > tmpRect.Left)
                                tmpRect.X = scRect.Left;
                            if (scRect.Right < tmpRect.Right)
                                tmpRect.X = scRect.Right - tmpRect.Width;
                            if (scRect.Top > tmpRect.Top)
                                tmpRect.Y = scRect.Top;
                            if (scRect.Bottom < tmpRect.Bottom)
                                tmpRect.Y = scRect.Bottom - tmpRect.Height;

                            this.Location = tmpRect.Location;
                            this.Size = tmpRect.Size;
                            break;
                        }
                        else
                            this.Size = tmpRect.Size;
                    }
                }
            }
        }
        #endregion

        #region ## PROTECTED OVERRIDE

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (isCloseFormButtonMouseDown)
            {
                isCloseFormButtonMouseDown = false;
                this.Invalidate(closeButtonRect);
                this.Update();
                if (closeButtonRect.Contains(e.Location))
                {
                    System.Threading.Thread.Sleep(150);
                    this.Close();
                }
            }
            else if (isMinimizeFormButtonMouseDown)
            {
                isMinimizeFormButtonMouseDown = false;
                this.Invalidate(minimizeButtonRect);
                this.Update();
                if (minimizeButtonRect.Contains(e.Location))
                {
                    //this.WindowState = FormWindowState.Minimized;
                    //this.thisWindowState = CustomWindowState.Minimized;
                    this.WindowState = FormWindowState.Minimized;
                    //ShowWindowAsync(this.Handle, SW_SHOWMINIMIZED);
                    // EFFECT
                }
            }
            else if (isMaximizeFormButtonMouseDown)
            {
                isMaximizeFormButtonMouseDown = false;
                this.Invalidate(maximizeButtonRect);
                this.Update();
                if (maximizeButtonRect.Contains(e.Location))
                {
                    //if (this.WindowState == FormWindowState.Maximized)
                    //    this.WindowState = FormWindowState.Normal;
                    //else
                    //    this.WindowState = FormWindowState.Maximized;
                    //    //ShowWindowAsync(this.Handle, SW_SHOWMAXIMIZED);

                    if (this.thisWindowState == CustomWindowState.Maximized)
                        this.thisWindowState = CustomWindowState.Normal;
                    else
                        this.thisWindowState = CustomWindowState.Maximized;
                }
            }
            isFormDraging = false;
            base.OnMouseUp(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (closeButtonRect.Contains(e.Location))
                {
                    isCloseFormButtonMouseDown = true;
                    this.Invalidate(closeButtonRect);
                    this.Update();
                }
                else if (TopRight_MinimizeButtonVisible && minimizeButtonRect.Contains(e.Location))
                {
                    isMinimizeFormButtonMouseDown = true;
                    this.Invalidate(minimizeButtonRect);
                    this.Update();
                }
                else if (TopRight_MaximizeButtonVisible && maximizeButtonRect.Contains(e.Location))
                {
                    isMaximizeFormButtonMouseDown = true;
                    this.Invalidate(maximizeButtonRect);
                    this.Update();
                }
                //else if (m_formSizable && this.WindowState != FormWindowState.Maximized ? getMouseDirectPos(e.Location) > -1 : false)
                else if (m_formSizable && this.thisWindowState != CustomWindowState.Maximized ? getMouseDirectPos(e.Location) > -1 : false)
                {
                    int tmpPos = getMouseDirectPos(e.Location);
                    ReleaseCapture();
                    //this.Capture = false;

                    //0:TopLeft, 1:TopRight, 2:BottomRight, 3:BottomLeft, 4:Left, 5:Right, 6:Top, 7:Bottom
                    //if (this.Width > this.MinimumSize.Width + 1 && this.Height > this.MinimumSize.Height + 1)
                    int prevWidth = this.Width;
                    int prevHeight = this.Height;

                    switch (tmpPos)
                    {
                        case 0:
                            SendMessage(this.Handle, WM_SYSCOMMAND, 0xF004, 0);
                            break;
                        case 1:
                            SendMessage(this.Handle, WM_SYSCOMMAND, 0xF005, 0);
                            break;
                        case 2:
                            SendMessage(this.Handle, WM_SYSCOMMAND, 0xF008, 0);
                            break;
                        case 3:
                            SendMessage(this.Handle, WM_SYSCOMMAND, 0xF007, 0);
                            break;
                        case 4:
                            SendMessage(this.Handle, WM_SYSCOMMAND, 0xF001, 0);
                            break;
                        case 5:
                            SendMessage(this.Handle, WM_SYSCOMMAND, 0xF002, 0);
                            break;
                        case 6:
                            SendMessage(this.Handle, WM_SYSCOMMAND, 0xF003, 0);
                            break;
                        case 7:
                            SendMessage(this.Handle, WM_SYSCOMMAND, 0xF006, 0);
                            break;
                    }
                    this.Width = Math.Max(this.Width, this.MinimumSize.Width);
                    this.Height = Math.Max(this.Height, this.MinimumSize.Height);

                    //System.Diagnostics.Debug.WriteLine(this.Width+":"+this.MinimumSize.Width);

                    //this.Capture = true;
                }
                else if (e.Location.Y < m_topBorderLine && new Rectangle(this.Location, this.Size).Contains(MousePosition))
                {
                    isFormDraging = true;
                    startPos = e.Location;
                }

                ///컨트롤 누르고 마우스왼쪽버튼 눌렀을때 폼 이동 메시지
                if (e.Button == MouseButtons.Left && Control.ModifierKeys == Keys.Control)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            }
            base.OnMouseDown(e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            Cursor = Cursors.Default;
            isTopRight_closeButtonMouseHover = false;
            isTopRight_minimizeButtonMouseHover = false;
            isTopRight_maximizeButtonMouseHover = false;
            this.Invalidate(new Rectangle(0, 0, this.ClientSize.Width, m_topBorderLine));
            base.OnMouseLeave(e);
        
        }
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            Rectangle topRect = new Rectangle(0, 0, this.ClientSize.Width, m_topBorderLine);
            if (TopRight_MaximizeButtonVisible && topRect.Contains(e.Location))
            {
                this.Invalidate(maximizeButtonRect);
                this.Update();
                //if (this.WindowState == FormWindowState.Maximized)
                //    this.WindowState = FormWindowState.Normal;
                //else
                    //this.WindowState = FormWindowState.Maximized;
                if(this.thisWindowState == CustomWindowState.Maximized)
                    this.thisWindowState = CustomWindowState.Normal;
                else
                    this.thisWindowState = CustomWindowState.Maximized;
            }
            base.OnMouseDoubleClick(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            // 꼭짓점 뭉툭함 지정
            if (m_radius > 0)
            {
                GraphicsPath thisRegion = Create(0, 0, this.Width, this.Height, m_radius, m_vertexDirection);
                this.Region = new Region(thisRegion);
                thisRegion.Dispose();
            }
            else
            {
                this.Region = new Region(this.ClientRectangle);
            }

            // 폼 제목표시줄 구분할 사각형 지정
            GraphicsPath tmpPath = new GraphicsPath();
            Rectangle topRect = new Rectangle(0, 0, this.ClientSize.Width, m_topBorderLine);

            tmpPath.AddRectangle(topRect);
            tmpPath.CloseFigure();

            // 폼 제목표시줄 색상 지정
            PathGradientBrush pthGrBrush = new PathGradientBrush(tmpPath);
            if (TitleBoxGradient) //그라데이션 효과
                pthGrBrush.CenterColor = Color.FromArgb(m_titleboxColor.A - 50, m_titleboxColor);
            else
                pthGrBrush.CenterColor = m_titleboxColor;
            Color[] colors = { m_titleboxColor };
            pthGrBrush.SurroundColors = colors;
            e.Graphics.FillPath(pthGrBrush, tmpPath); // 사각형에 색상 칠하기
            pthGrBrush.Dispose();
            tmpPath.Dispose();

            sizeBorder = this.ClientRectangle;
            sizeBorder.X += addVal;
            sizeBorder.Y += addVal;
            sizeBorder.Width -= addVal * 2;
            sizeBorder.Height -= addVal * 2;

            
            
            if (TopRight_BtnsAutoLocation) // 화면 우측 상단의 버튼들 자동위치 지정
            {
                // 우측 상단 버튼메뉴 Y값 측정
                int y = (m_topBorderLine / 2) - (m_btnsSize.Height / 2);
                int idx = 1;

                // 닫기버튼 Rect지정
                if ( m_closeBtnVisible){
                    closeButtonRect = new Rectangle(new Point(this.ClientSize.Width - y - (m_btnsSize.Width * idx), (m_topBorderLine / 2) - (m_btnsSize.Height / 2)), m_btnsSize);
                    idx += 1;
                }else{
                    closeButtonRect = new Rectangle();
                }

                if (TopRight_MaximizeButtonVisible && TopRight_MinimizeButtonVisible)
                {
                    // 최대화버튼 Rect지정
                    maximizeButtonRect = new Rectangle(new Point(this.ClientSize.Width - y - (m_btnsSize.Width * idx), (m_topBorderLine / 2) - (m_btnsSize.Height / 2)), m_btnsSize);
                    idx += 1;
                    // 최소화버튼 Rect지정
                    minimizeButtonRect = new Rectangle(new Point(this.ClientSize.Width - y - (m_btnsSize.Width * idx), (m_topBorderLine / 2) - (m_btnsSize.Height / 2)), m_btnsSize);
                    idx += 1;
                }
                else if (TopRight_MaximizeButtonVisible)
                {
                    maximizeButtonRect = new Rectangle(new Point(this.ClientSize.Width - y - (m_btnsSize.Width * idx), (m_topBorderLine / 2) - (m_btnsSize.Height / 2)), m_btnsSize);
                    idx += 1;
                }
                else if (TopRight_MinimizeButtonVisible)
                {
                    minimizeButtonRect = new Rectangle(new Point(this.ClientSize.Width - y - (m_btnsSize.Width * idx), (m_topBorderLine / 2) - (m_btnsSize.Height / 2)), m_btnsSize);
                    idx += 1;
                }
            }
            else
            {
                int x = this.m_btnsLocation.X;
                int y = this.m_btnsLocation.Y;
                int idx = 1;

                // 닫기버튼 Rect지정
                if (m_closeBtnVisible)
                {
                    closeButtonRect = new Rectangle(new Point(this.ClientSize.Width - x - (m_btnsSize.Width * idx), y), m_btnsSize);
                }else
                {
                    closeButtonRect = new Rectangle();
                }

                if (TopRight_MaximizeButtonVisible && TopRight_MinimizeButtonVisible)
                {
                    // 최대화버튼 Rect지정
                    maximizeButtonRect = new Rectangle(new Point(this.ClientSize.Width - x - (m_btnsSize.Width * idx), y), m_btnsSize);
                    idx += 1;
                    // 최소화버튼 Rect지정
                    minimizeButtonRect = new Rectangle(new Point(this.ClientSize.Width - x - (m_btnsSize.Width * idx), y), m_btnsSize);
                    idx += 1;
                }
                else if (TopRight_MaximizeButtonVisible)
                {
                    maximizeButtonRect = new Rectangle(new Point(this.ClientSize.Width - x - (m_btnsSize.Width * idx), y), m_btnsSize);
                    idx += 1;
                }
                else if (TopRight_MinimizeButtonVisible)
                {
                    minimizeButtonRect = new Rectangle(new Point(this.ClientSize.Width - x - (m_btnsSize.Width * idx), y), m_btnsSize);
                    idx += 1;
                }
            }


            //닫기 버튼 그리기
            if (isCloseFormButtonMouseDown) //마우스 누른상태일 경우
            {
                if (TopRight_CloseBtnHighLightImage == null)
                        ControlPaint.DrawCaptionButton(e.Graphics, closeButtonRect, CaptionButton.Close, ButtonState.Pushed);
                else
                    e.Graphics.DrawImage(TopRight_CloseBtnHighLightImage, closeButtonRect);
            }
            else
            {
                if (isTopRight_closeButtonMouseHover)
                {
                    if (TopRight_CloseBtnMouseOverImage == null)
                        ControlPaint.DrawCaptionButton(e.Graphics, closeButtonRect, CaptionButton.Close, ButtonState.Flat);
                    else
                        e.Graphics.DrawImage(TopRight_CloseBtnMouseOverImage, closeButtonRect);
                }
                else if ( !closeButtonRect.IsEmpty )
                {
                    if (TopRight_CloseBtnNormalImage == null)
                        ControlPaint.DrawCaptionButton(e.Graphics, closeButtonRect, CaptionButton.Close, ButtonState.Normal);
                    else
                        e.Graphics.DrawImage(TopRight_CloseBtnNormalImage, closeButtonRect);
                }
            }

            if (TopRight_MaximizeButtonVisible && TopRight_MinimizeButtonVisible)
            {
                //최소화버튼
                if (isMinimizeFormButtonMouseDown) //마우스 누른상태일 경우
                {
                    if (TopRight_MinimizeBtnHighLightImage == null)
                        ControlPaint.DrawCaptionButton(e.Graphics, minimizeButtonRect, CaptionButton.Minimize, ButtonState.Pushed);
                    else
                        e.Graphics.DrawImage(TopRight_MinimizeBtnHighLightImage, minimizeButtonRect);
                }
                else
                {
                    if (isTopRight_minimizeButtonMouseHover)
                    {
                        if(TopRight_MinimizeBtnMouseOverImage == null)
                            ControlPaint.DrawCaptionButton(e.Graphics, minimizeButtonRect, CaptionButton.Minimize, ButtonState.Flat);
                        else
                            e.Graphics.DrawImage(TopRight_MinimizeBtnMouseOverImage, minimizeButtonRect);
                    }
                    else
                    {
                        if (TopRight_MinimizeBtnNormalImage == null)
                            ControlPaint.DrawCaptionButton(e.Graphics, minimizeButtonRect, CaptionButton.Minimize, ButtonState.Normal);
                        else
                            e.Graphics.DrawImage(TopRight_MinimizeBtnNormalImage, minimizeButtonRect);
                    }
                }

                //if (this.WindowState == FormWindowState.Normal) //최대화버튼
                if (this.thisWindowState == CustomWindowState.Normal)
                {
                    if (isMaximizeFormButtonMouseDown) //마우스 누른상태일 경우
                    {
                        if (TopRight_MaximizeBtnHighLightImage == null)
                            ControlPaint.DrawCaptionButton(e.Graphics, maximizeButtonRect, CaptionButton.Maximize, ButtonState.Pushed);
                        else
                            e.Graphics.DrawImage(TopRight_MaximizeBtnHighLightImage, maximizeButtonRect);
                    }
                    else
                    {

                        if (isTopRight_maximizeButtonMouseHover)
                        {
                            if(TopRight_MaximizeBtnMouseOverImage == null)
                                ControlPaint.DrawCaptionButton(e.Graphics, maximizeButtonRect, CaptionButton.Maximize, ButtonState.Flat);
                            else
                                e.Graphics.DrawImage(TopRight_MaximizeBtnMouseOverImage, maximizeButtonRect);
                        }
                        else
                        {
                            if (TopRight_MaximizeBtnNormalImage == null)
                                ControlPaint.DrawCaptionButton(e.Graphics, maximizeButtonRect, CaptionButton.Maximize, ButtonState.Normal);
                            else
                                e.Graphics.DrawImage(TopRight_MaximizeBtnNormalImage, maximizeButtonRect);
                        }
                    }
                }
                //else if(this.WindowState == FormWindowState.Maximized) //리스토어버튼
                else if(this.thisWindowState == CustomWindowState.Maximized)
                {
                    if (isMaximizeFormButtonMouseDown) //마우스 누른상태일 경우
                    {
                        if (TopRight_MaximizeRestoreBtnHighLightImage == null)
                            ControlPaint.DrawCaptionButton(e.Graphics, maximizeButtonRect, CaptionButton.Restore, ButtonState.Pushed);
                        else
                            e.Graphics.DrawImage(TopRight_MaximizeRestoreBtnHighLightImage, maximizeButtonRect);
                    }
                    else
                    {
                        if (isTopRight_maximizeButtonMouseHover)
                        {
                            if(TopRight_MaximizeRestoreBtnMouseOverImage == null)
                                ControlPaint.DrawCaptionButton(e.Graphics, maximizeButtonRect, CaptionButton.Restore, ButtonState.Flat);
                            else
                                e.Graphics.DrawImage(TopRight_MaximizeRestoreBtnMouseOverImage, maximizeButtonRect);
                        }
                        else
                        {
                            if (TopRight_MaximizeRestoreBtnNormalImage == null)
                                ControlPaint.DrawCaptionButton(e.Graphics, maximizeButtonRect, CaptionButton.Restore, ButtonState.Normal);
                            else
                                e.Graphics.DrawImage(TopRight_MaximizeRestoreBtnNormalImage, maximizeButtonRect);
                        }
                    }
                }
            }
            else if (TopRight_MinimizeButtonVisible)
            {
                //최소화 버튼만 그리기
                if (isMinimizeFormButtonMouseDown)
                {
                    if (TopRight_MinimizeBtnHighLightImage == null)
                        ControlPaint.DrawCaptionButton(e.Graphics, minimizeButtonRect, CaptionButton.Minimize, ButtonState.Pushed);
                    else
                        e.Graphics.DrawImage(TopRight_MinimizeBtnHighLightImage, minimizeButtonRect);
                }
                else if (isTopRight_minimizeButtonMouseHover)
                {
                    if (TopRight_MinimizeBtnMouseOverImage == null)
                        ControlPaint.DrawCaptionButton(e.Graphics, minimizeButtonRect, CaptionButton.Minimize, ButtonState.Flat);
                    else
                        e.Graphics.DrawImage(TopRight_MinimizeBtnMouseOverImage, minimizeButtonRect);
                }
                else
                {
                    if (TopRight_MinimizeBtnNormalImage == null)
                        ControlPaint.DrawCaptionButton(e.Graphics, minimizeButtonRect, CaptionButton.Minimize, ButtonState.Normal);
                    else
                        e.Graphics.DrawImage(TopRight_MinimizeBtnNormalImage, minimizeButtonRect);
                }
            }
            else if (TopRight_MaximizeButtonVisible)
            {
                //최대화버튼
                //if (this.WindowState == FormWindowState.Normal)
                if(this.thisWindowState == CustomWindowState.Normal)
                {
                    if (isMaximizeFormButtonMouseDown)
                    {
                        if (TopRight_MaximizeBtnHighLightImage == null)
                            ControlPaint.DrawCaptionButton(e.Graphics, maximizeButtonRect, CaptionButton.Maximize, ButtonState.Pushed);
                        else
                            e.Graphics.DrawImage(TopRight_MaximizeBtnHighLightImage, maximizeButtonRect);
                    }
                    else if (isTopRight_maximizeButtonMouseHover)
                    {
                        if (TopRight_MaximizeBtnMouseOverImage == null)
                            ControlPaint.DrawCaptionButton(e.Graphics, maximizeButtonRect, CaptionButton.Maximize, ButtonState.Flat);
                        else
                            e.Graphics.DrawImage(TopRight_MaximizeBtnMouseOverImage, maximizeButtonRect);
                    }
                    else
                    {
                        if (TopRight_MaximizeBtnNormalImage == null)
                            ControlPaint.DrawCaptionButton(e.Graphics, maximizeButtonRect, CaptionButton.Maximize, ButtonState.Normal);
                        else
                            e.Graphics.DrawImage(TopRight_MaximizeBtnNormalImage, maximizeButtonRect);
                    }
                }
                //else if (this.WindowState == FormWindowState.Maximized) //리스토어버튼
                else if (this.thisWindowState == CustomWindowState.Maximized)
                {
                    if (isMaximizeFormButtonMouseDown)
                    {
                        if (TopRight_MaximizeRestoreBtnHighLightImage == null)
                            ControlPaint.DrawCaptionButton(e.Graphics, maximizeButtonRect, CaptionButton.Restore, ButtonState.Pushed);
                        else
                            e.Graphics.DrawImage(TopRight_MaximizeRestoreBtnHighLightImage, maximizeButtonRect);
                    }
                    else if (isTopRight_maximizeButtonMouseHover)
                    {
                        if (TopRight_MaximizeRestoreBtnMouseOverImage == null)
                            ControlPaint.DrawCaptionButton(e.Graphics, maximizeButtonRect, CaptionButton.Restore, ButtonState.Flat);
                        else
                            e.Graphics.DrawImage(TopRight_MaximizeRestoreBtnMouseOverImage, maximizeButtonRect);
                    }
                    else
                    {
                        if (TopRight_MaximizeRestoreBtnNormalImage == null)
                            ControlPaint.DrawCaptionButton(e.Graphics, maximizeButtonRect, CaptionButton.Restore, ButtonState.Normal);
                        else
                            e.Graphics.DrawImage(TopRight_MaximizeRestoreBtnNormalImage, maximizeButtonRect);
                    }
                }
            }

            if (!m_titleHidden)
            {
                // 폼의 제목부 위치 잡고 글자 그리기
                Rectangle titleRect = new Rectangle(new Point(Convert.ToInt32(m_titleLocation.X), Convert.ToInt32(m_titleLocation.Y))
                    , new Size(this.ClientRectangle.Width - Convert.ToInt32(m_titleLocation.X) - (this.ClientRectangle.Width - (TopRight_MinimizeButtonVisible ? minimizeButtonRect.Left : closeButtonRect.Left))
                        , m_topBorderLine - Convert.ToInt32(m_titleLocation.Y)));

                e.Graphics.DrawString(this.Text, m_titleFont, new SolidBrush(m_titleColor), titleRect, m_titleStringFormat);

                if (m_showIconInTitleBox && this.Icon != null)
                {
                    e.Graphics.DrawImage(this.Icon.ToBitmap(), m_iconLocation);
                }
            }


            if (isViewBorder)
            {
                //폼 활성화에 따른 테두리 색 지정
                if (m_radius > 0)
                {
                    // Vertex가 뭉툭할때
                    if (isActivated)
                    {
                        e.Graphics.DrawPath(m_formActivateBorderPen, Create(0, 0, this.Width - 1, this.Height - 1, m_radius + 2, m_vertexDirection));
                    }
                    else
                    {
                        e.Graphics.DrawPath(m_formDeactivateBorderPen, Create(0, 0, this.Width - 1, this.Height - 1, m_radius + 2, m_vertexDirection));
                    }
                }
                else
                {
                    //Vertex가 전부 사각일때
                    if (isActivated)
                        e.Graphics.DrawRectangle(m_formActivateBorderPen, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
                    else
                        e.Graphics.DrawRectangle(m_formDeactivateBorderPen, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
                }
            }
            
            base.OnPaint(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            if (!isActivated)
            {
                isActivated = true;
                this.Invalidate();
            }

            base.OnActivated(e);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            if (isActivated)
            {
                isActivated = false;
                this.Invalidate();
            }
            base.OnDeactivate(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.m_formClosedSmooth)
            {
                for (int i = 9; i > 0; i -= 2)
                {
                    System.Threading.Thread.Sleep(30);
                    this.Opacity = i * 0.1;
                }
            }
            //폼의 ClientRectangle 저장
            if (this.m_formRectangleSave)
            {
                RectangleConverter rc = new RectangleConverter();
                Rectangle tmpRect = new Rectangle(this.Location, this.Size);
                IsolatedStorageManagement.WriteIsolated(customFormRectangleKeyName, rc.ConvertToString(tmpRect));
            }
            base.OnClosing(e);
        }



        private const int CS_DROPSHADOW = 0x00020000;

        protected override CreateParams CreateParams
        {
            get
            {
                // add the drop shadow flag for automatically drawing
                // a drop shadow around the form
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
                //return base.CreateParams;
            }
        }
        #endregion

        #region  ## PUBLIC STATIC
        public static GraphicsPath Create(int x, int y, int width, int height,
                                              int radius, RectangleCorners corners)
        {
            int xw = x + width;
            int yh = y + height;
            int xwr = xw - radius;
            int yhr = yh - radius;
            int xr = x + radius;
            int yr = y + radius;
            int r2 = radius * 2;
            int xwr2 = xw - r2;
            int yhr2 = yh - r2;

            GraphicsPath p = new GraphicsPath();
            p.StartFigure();

            //Top Left Corner
            if ((RectangleCorners.TopLeft & corners) == RectangleCorners.TopLeft)
            {
                p.AddArc(x, y, r2, r2, 180, 90);
            }
            else
            {
                p.AddLine(x, yr, x, y);
                p.AddLine(x, y, xr, y);
            }

            //Top Edge
            p.AddLine(xr, y, xwr, y);

            //Top Right Corner
            if ((RectangleCorners.TopRight & corners) == RectangleCorners.TopRight)
            {
                p.AddArc(xwr2, y, r2, r2, 270, 90);
            }
            else
            {
                p.AddLine(xwr, y, xw, y);
                p.AddLine(xw, y, xw, yr);
            }

            //Right Edge
            p.AddLine(xw, yr, xw, yhr);

            //Bottom Right Corner
            if ((RectangleCorners.BottomRight & corners) == RectangleCorners.BottomRight)
            {
                p.AddArc(xwr2, yhr2, r2, r2, 0, 90);
            }
            else
            {
                p.AddLine(xw, yhr, xw, yh);
                p.AddLine(xw, yh, xwr, yh);
            }

            //Bottom Edge
            p.AddLine(xwr, yh, xr, yh);

            //Bottom Left Corner
            if ((RectangleCorners.BottomLeft & corners) == RectangleCorners.BottomLeft)
            {
                p.AddArc(x, yhr2, r2, r2, 90, 90);
            }
            else
            {
                p.AddLine(xr, yh, x, yh);
                p.AddLine(x, yh, x, yhr);
            }

            //Left Edge
            p.AddLine(x, yhr, x, yr);

            p.CloseFigure();
            return p;
        }
        #endregion

        bool isTopRight_closeButtonMouseHover = false;
        bool isTopRight_minimizeButtonMouseHover = false;
        bool isTopRight_maximizeButtonMouseHover = false;

        private void CustomForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (closeButtonRect.Contains(e.Location) || minimizeButtonRect.Contains(e.Location) || maximizeButtonRect.Contains(e.Location))
            {
                Cursor = Cursors.Default;
                if (closeButtonRect.Contains(e.Location))
                {
                    isTopRight_closeButtonMouseHover = true;
                    isTopRight_minimizeButtonMouseHover = false;
                    isTopRight_maximizeButtonMouseHover = false;
                    this.Invalidate();
                }
                else if (minimizeButtonRect.Contains(e.Location))
                {
                    isTopRight_closeButtonMouseHover = false;
                    isTopRight_minimizeButtonMouseHover = true;
                    isTopRight_maximizeButtonMouseHover = false;
                    this.Invalidate();
                }
                else if (maximizeButtonRect.Contains(e.Location))
                {
                    isTopRight_closeButtonMouseHover = false;
                    isTopRight_minimizeButtonMouseHover = false;
                    isTopRight_maximizeButtonMouseHover = true;
                    this.Invalidate();
                }
            }
            else
            {
                if (isTopRight_closeButtonMouseHover | isTopRight_minimizeButtonMouseHover | isTopRight_maximizeButtonMouseHover)
                {
                    isTopRight_closeButtonMouseHover = false;
                    isTopRight_minimizeButtonMouseHover = false;
                    isTopRight_maximizeButtonMouseHover = false;
                    this.Invalidate();
                }
            }
            //if(m_formSizable && this.WindowState != FormWindowState.Maximized)
            if(m_formSizable && this.thisWindowState != CustomWindowState.Maximized)
                getMouseDirectPos(e.Location);

            if (isFormDraging && this.thisWindowState != CustomWindowState.Maximized)
            {
                //if (this.thisWindowState == CustomWindowState.Maximized)
                //    this.thisWindowState = CustomWindowState.Normal;

                this.DesktopLocation = new Point(MousePosition.X - startPos.X, MousePosition.Y - startPos.Y);

                //위치 따라갈 컨트롤

                //if (!(this.imageViewCtrl == null || this.imageViewCtrl.IsDisposed))
                //{
                //    Point tmpP = getTopLocation(recentlyEmoticonViewOpenButton, recentlyEmoticonViewOpenButton, new Point());
                //    this.imageViewCtrl.DesktopLocation = new Point(this.DesktopLocation.X + tmpP.X, this.DesktopLocation.Y + tmpP.Y);
                //}
            }
        }

        /// <summary>
        /// 마우스의 위치가 폼의 Border부분에 위치해 있을때 마우스 커서의 모양을 변경한다.
        /// </summary>
        /// <param name="p"></param>
        /// <returns>0:TopLeft, 1:TopRight, 2:BottomRight, 3:BottomLeft, 4:Left, 5:Right, 6:Top, 7:Bottom, -1:etc...</returns>
        private int getMouseDirectPos(Point p)
        {
            Rectangle f = new Rectangle(new Point(0, 0), this.Size);

            if (!f.Contains(p))
            {
                Cursor = Cursors.Default;
                return -1;
            }
            if (p.X < addVal && p.Y < addVal)
            {
                Cursor = Cursors.SizeNWSE;
                return 0;
            }
            else if (p.X > sizeBorder.Width && p.Y < addVal)
            {
                Cursor = Cursors.SizeNESW;
                return 1;
            }
            else if (p.X > sizeBorder.Width && p.Y > sizeBorder.Height)
            {
                Cursor = Cursors.SizeNWSE;
                return 2;
            }
            else if (p.X < addVal && p.Y > sizeBorder.Height)
            {
                Cursor = Cursors.SizeNESW;
                return 3;
            }
            else if (p.X < addVal)
            {
                Cursor = Cursors.SizeWE;
                return 4;
            }
            else if (p.X > sizeBorder.Width)
            {
                Cursor = Cursors.SizeWE;
                return 5;
            }
            else if (p.Y < addVal)
            {
                Cursor = Cursors.SizeNS;
                return 6;
            }
            else if (p.Y > sizeBorder.Height)
            {
                Cursor = Cursors.SizeNS;
                return 7;
            }
            else
            {
                Cursor = Cursors.Default;
                return -1;
            }
        }

        /// <summary>
        /// 폼의 FormBorder - ActivateColor,DeactivateColor, IsView, Width 속성을 복사한다.
        /// </summary>
        /// <param name="sourceForm">복사할 속성을 갖고 있는 폼</param>
        /// <param name="targetForm">붙여넣을 폼</param>
        /// <returns></returns>
        public static CustomForm formStatusCopy(CustomForm sourceForm, CustomForm targetForm)
        {
            try
            {
                targetForm.FormBorder_ActivateColor = sourceForm.FormBorder_ActivateColor;
                targetForm.FormBorder_DeactivateColor = sourceForm.FormBorder_DeactivateColor;
                targetForm.FormBorder_IsView = sourceForm.FormBorder_IsView;
                targetForm.FormBorder_Width = sourceForm.FormBorder_Width;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            return targetForm;
        }
    }
}
