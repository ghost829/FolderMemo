using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;

namespace FolderMemo
{
    public partial class Form_Folder : CustomControlLibrary.CustomForm
    {
        private static object syncRoot = new object();

        //private XmlDocument xmlDoc; //메모 데이터 정보가 들어있는 Document

        public delegate void KeyValue(List<String[]> keyValue);
        public event KeyValue throw_Environment_SettingChanged;

        public delegate bool CommonDelegate(DEFINE.EVENTTYPE type, object obj);
        public event CommonDelegate occurred_event;

        private const int SW_SHOWNA = 8;        // Window API - ShowWindow 옵션, 포커스 없이 윈도우를 출력
        private PopupWindow tooltipPopup;
        private CustomTooltip tooltip;
        
        [System.Runtime.InteropServices.DllImport("user32", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private extern static int ShowWindow(IntPtr hWnd, int nCmdShow); // Window API - 윈도우 출력

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_SHOWWINDOW = 0x0040;
        const UInt32 SWP_NOACTIVATE = 0x0010;

        // 현재 보여지고 있는 경로
        private string currentPath;
        // 리스트뷰 실제 데이터
        List<Item> data = new List<Item>();
        int recently_searchType = -1; // 마지막으로 검색 수행한 방식 (0:searchbypath, 1:searchbyword)
        string recently_searchPath = string.Empty; // searchByPath 검색일때 검색경로 저장할 변수
        string recently_searchWord = string.Empty; // searchWord 검색일때 검색어 저장할 변수

        private List<Item> m_copylst = new List<Item>();
        bool m_isSearchResult = false; /* 검색 기능 사용중인지 여부. 검색결과 출력중일때는 Paste불가! */

        private Timer timer_show_dt; /*연월일시 출력 타이머*/

        /// <summary>
        /// Focus Steal 없이 윈도우 출력
        /// </summary>
        /// <param name="hWnd"></param>
        private void ShowWindow(IntPtr hWnd)
        {
            ShowWindow(hWnd, SW_SHOWNA);
            if(this.TopMost)
                SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW | SWP_NOACTIVATE);
        }

        public Form_Folder()
        {
            this.AllowDrop = true;
            InitializeComponent();
        }

        private void Folder_Load(object sender, EventArgs e)
        {
            this.SearchByPath("/");
            this.listView1.MouseWheel += listView1_MouseWheel;

            ListViewHelper.SetExtendedStyle(this.listView1, ListViewExtendedStyles.DoubleBuffer);


            timer_show_dt = new Timer();
            timer_show_dt.Tick += new EventHandler(delegate
            {
                DateTime now = DateTime.Now;
                var str_dt = now.ToString("yyyy년 MM월 dd일");
                str_dt += now.ToString(" dddd");
                str_dt += System.Environment.NewLine;
                str_dt += now.Hour < 13 ? "오전" : "오후";
                str_dt += now.ToString(" hh:mm:ss");

                lbl_time.Text = str_dt;
                //lbl_time.Text = DateTime.Now.ToLongDateString() + System.Environment.NewLine + DateTime.Now.ToLongTimeString();
            });
            timer_show_dt.Interval = 100;
            timer_show_dt.Start();
        }


        private void Form_Folder_FormClosing(object sender, FormClosingEventArgs e)
        {
            ListViewHelper.DisableDoubleBuffer(this.listView1);

            if (timer_show_dt != null)
            {
                timer_show_dt.Stop();
                timer_show_dt = null;
            }
        }

        void listView1_MouseWheel(object sender, MouseEventArgs e)
        {
            hideToolTip();
        }

        private void Folder_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (throw_Environment_SettingChanged != null)
            {
                List<String[]> tmpList = new List<string[]>();
                RectangleConverter rc = new RectangleConverter();
                Rectangle tmpRect = new Rectangle(this.Location, this.Size);
                String[] tmpStrArr = { DEFINE.NODETYPE_STRING, DEFINE.CONFIG_SETTING_CLOSERECT, rc.ConvertToString(tmpRect) };

                tmpList.Add(tmpStrArr);
                throw_Environment_SettingChanged(tmpList);
            }
        }
        
        private void Folder_MouseUp(object sender, MouseEventArgs e)
        {
            if(e.Button == System.Windows.Forms.MouseButtons.Right)
                ctxt_form.Show(PointToScreen(e.Location));
            else if (e.Button == System.Windows.Forms.MouseButtons.XButton1) //backward
            {
                if (this.btn_upperPath.Enabled)
                {
                    btn_upperPath_Click(null, null);
                }
            }
            //else if(e.Button == System.Windows.Forms.MouseButtons.XButton2) //forward
        }



        #region ## Form Context Menu
        private void ctxt_form_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ctxt_form_topmost_Click(object sender, EventArgs e)
        {
            this.TopMost = !this.TopMost;
            
        }
        #endregion;


        [Browsable(true), Category("CustomForm"), Description("윈도우를 맨위로 설정")]
        public new bool TopMost
        {
            get
            {
                return base.TopMost;
            }
            set
            {
                base.TopMost = value;
                ctxt_form_topmost.Checked = value;

                if (throw_Environment_SettingChanged != null)
                {
                    String[] keyValue = { DEFINE.NODETYPE_STRING, DEFINE.CONFIG_SETTING_TOPMOST, ctxt_form_topmost.Checked ? "TRUE" : "FALSE" };
                    List<String[]> list = new List<String[]>();
                    list.Add(keyValue);
                    throw_Environment_SettingChanged(list);
                }
            }
        }

        ListViewItem hoveredItem;
        float timer_time = DEFINE.CUSTOMETOOLTIP_WAITTIME;
        Timer hoverTimer;
        bool tooltip_isVisible = false;
        
        // 툴팁 출력을 위한 MouseMove Event
        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            Point form_mousePos = PointToClient(MousePosition);
            Point listView_mousePos = new Point(form_mousePos.X - listView1.Location.X, form_mousePos.Y - listView1.Location.Y);
            ListViewItem item = listView1.GetItemAt(listView_mousePos.X, listView_mousePos.Y);

            if (item != null && item.Tag != null)
            {
                if (item.Tag.Equals(DEFINE.FILETYPE.FILETYPE_TEXT))
                {
                    if (tooltip_isVisible == false)
                    {
                        //Console.WriteLine("Visible-true");
                        tooltip_isVisible = true;

                        hoveredItem = item;
                        
                        hoverTimer = new Timer();
                        Timer timer = hoverTimer;
                        timer.Tick += new EventHandler(
                            delegate (object t_sender, EventArgs t_e){
                                timer_time -= (timer.Interval/1000f);
                                //Console.WriteLine(timer_time);
                                if (timer_time <= 0)
                                {
                                    timer_time = DEFINE.CUSTOMETOOLTIP_WAITTIME;
                                    this.Invoke(new MethodInvoker(delegate { this.showToolTip(hoveredItem); }));
                                    timer.Stop();
                                    hoverTimer.Dispose();
                                    hoverTimer = null;
                                }
                        }) ;
                        
                        timer.Interval = 100;
                        timer.Start();
                    }
                }
                else
                {
                    hideToolTip();
                }
            }
            else
            {
                hideToolTip();
            }
        }

        // 툴팁 제거를 위한 MouseLeave Event
        private void listView1_MouseLeave(object sender, EventArgs e)
        {
            hideToolTip();
        }

        /// <summary>
        /// ToolTip 팝업 출력
        /// </summary>
        /// <param name="item"></param>
        private void showToolTip(ListViewItem item)
        {
            tooltip = new CustomTooltip(item.Text, item.SubItems[1].Text);
            tooltipPopup = new PopupWindow(tooltip);
            ShowWindow(tooltipPopup.Handle);

            Point loc = new Point(MousePosition.X + 15, MousePosition.Y + 15);
            tooltipPopup.SetBounds(loc.X, loc.Y, tooltip.Bounds.Width, tooltip.Bounds.Height);
            if (this.TopMost)
            {

            }
        }

        /// <summary>
        /// ToolTip 팝업 하이드
        /// </summary>
        private void hideToolTip()
        {
            if (tooltipPopup != null)
            {
                tooltip.Dispose();
                tooltipPopup.Hide();
                tooltipPopup.Dispose();
                tooltipPopup = null;
                tooltip = null;
                tooltip_isVisible = false;
            }
            if (hoverTimer != null)
            {
                hoverTimer.Stop();
                hoverTimer.Dispose();
                hoverTimer = null;
                tooltip_isVisible = false;
            }
        }

        // 단어 검색 버튼 클릭 시
        private void btn_search_Click(object sender, EventArgs e)
        {
            string word = txt_search.Text;
            this.searchWord(word);
        }

        /// <summary>
        /// 메모 데이터 정보를 갱신합니다.
        /// </summary>
        public void listViewReload()
        {
            if (recently_searchType == 0)
                SearchByPath(recently_searchPath);
            else if (recently_searchType == 1)
                searchWord(recently_searchWord);
            else
                SearchByPath(recently_searchPath);
        }

        /// <summary>
        /// 리스트뷰 다시 그리기
        /// </summary>
        private void refresh_listview()
        {
            listView1.Items.Clear();
            for (int i = 0; i < data.Count; i++)
            {
                Item item = data[i];
                int imgIndex = imageList1.Images.Count - 1;
                switch (item.TYPE)
                {
                    case DEFINE.FILETYPE.FILETYPE_DIRECTORY:
                        if (item.isEmpty())
                            imgIndex = 0;
                        else
                            imgIndex = 1;
                        break;
                    case DEFINE.FILETYPE.FILETYPE_TEXT:
                        if (string.IsNullOrEmpty(item.TEXT))
                            imgIndex = 2;
                        else
                            imgIndex = 3;
                        break;
                    default:
                        break;
                }
                ListViewItem lstItem = new ListViewItem(item.TITLE, imgIndex);
                //lstItem.ToolTipText = item.TEXT;
                lstItem.SubItems.Add(item.TEXT);
                lstItem.SubItems.Add(item.PATH);

                lstItem.Tag = item.TYPE;
                listView1.Items.Add(lstItem);
            }
        }

        /// <summary>
        /// 해당 경로에 존재하는 메모데이터를 출력
        /// </summary>
        /// <param name="path">경로</param>
        private void SearchByPath(String path)
        {
            // 기록 저장
            recently_searchType = 0;
            recently_searchPath = path;

            m_isSearchResult = false;

            txt_path.Text = path;

            if (this.Parent != null)
            {
                string name = this.Parent.Name;
            }
            data.Clear();

            RectangleConverter rc = new RectangleConverter();

            //Console.WriteLine(System.Windows.Forms.Application.StartupPath + DEFINE.MEMO_DATA_FILENAME);

            XmlDocument tmpXmlDoc = Common.XmlControl.getInstance().xmlLoad(DEFINE.MEMO_DATA_PATH);
            XmlNode memoNode = tmpXmlDoc.SelectSingleNode("//MEMODATA");

            if (path != "/")
            {
                //this.btn_upperPath.Image = Properties.Resources.upperArrow_enable;
                this.btn_upperPath.BackgroundImage = Properties.Resources.folder_up;
                this.btn_upperPath.Enabled = true;

                String[] paths = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < paths.Length; i++)
                {
                    memoNode = memoNode.SelectSingleNode("./" + DEFINE.NODE_GROUPNAME + "[@name='" + paths[i] + "']");
                }
            }
            else
            {
                //this.btn_upperPath.Image = Properties.Resources.upperArrow_disable;
                this.btn_upperPath.BackgroundImage = Properties.Resources.folder_up_disable;
                this.btn_upperPath.Enabled = false;
            }

            for (int i = 0; i < memoNode.ChildNodes.Count; i++)
            {
                XmlNode tmpNode = memoNode.ChildNodes[i];
                string type = tmpNode.Name;
                if (type == DEFINE.NODE_MEMONAME)
                {
                    string name = tmpNode.Attributes["name"].Value;
                    string text = tmpNode.Attributes["text"].Value;

                    Item item = new Item(name, text, DEFINE.FILETYPE.FILETYPE_TEXT);
                    item.setItemPath(path);

                    if (tmpNode.Attributes["rect"] != null)
                    {
                        string str_rect = tmpNode.Attributes["rect"].Value;
                        item.RECT = (Rectangle)rc.ConvertFromString(str_rect);
                    }


                    data.Add(item);
                }
                else if (type == DEFINE.NODE_GROUPNAME)
                {
                    string name = tmpNode.Attributes["name"].Value;
                    Item item = new Item(name, "", DEFINE.FILETYPE.FILETYPE_DIRECTORY);
                    item.setItemPath(path);
                    if (tmpNode.ChildNodes.Count > 0)
                    {
                        item.EMPTY = false;
                        item.TAG = tmpNode.InnerXml;
                    }
                    data.Add(item);
                }
            }
            currentPath = path;
            refresh_listview();
        }

        /// <summary>
        /// 메모데이터를 해당 단어로 검색 후 출력
        /// </summary>
        /// <param name="word">검색단어</param>
        private void searchWord(string word)
        {
            // 기록 저장
            recently_searchType = 1;
            recently_searchWord = word;

            m_isSearchResult = true;

            string searchWord = string.Empty; // 검색 할 단어
            bool isRegex = false; //정규식 사용여부

            if (String.IsNullOrEmpty(word))
            {
                // 검색어가 없을 경우 전체 검색
                SearchByPath("/");
                return;
            }
            else if (hasKoreanChar(word))
            {
                //검색어에 한글 존재할 경우

                ////////////////////////////////Regex를 사용하기 위해 필요한 Array /////////////////////////////////////
                List<string> korWord = new List<string>() { "가-깋", "나-닣", "다-맇", "라-맇", "마-밓", "바-빟", "사-싷", "아-잏", "자-짛", "차-칳", "타-팋", "카-킿", "파-핗", "하-힣" };
                List<char> korInitial = new List<char>() { 'ㄱ', 'ㄴ', 'ㄷ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅅ', 'ㅇ', 'ㅈ', 'ㅊ', 'ㅌ', 'ㅋ', 'ㅍ', 'ㅎ' };
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////

                StringBuilder regex_word = new StringBuilder(); // Regex에 사용될 단어
                bool findInitial = false; // 단어에서 한글자씩 체크할 때 해당 글자가 초성인지 확인

                for (int i = 0; i < word.Length; i++)
                {
                    findInitial = false;
                    for (int j = 0; j < korInitial.Count; j++)
                    {
                        if (word[i] == korInitial[j])
                        {
                            // 초성 발견되면 그 초성에 해당하는 Regex수식 regex_word에 작성
                            regex_word.Append("[" + korWord[j] + "]");
                            findInitial = true;
                            break;
                        }
                    }
                    if (findInitial == false)
                    {
                        // 초성이 발견되지 않으면 일반 수식 작성
                        regex_word.Append("[" + word[i] + "]");
                    }
                }
                System.Diagnostics.Debug.WriteLine("FIND by Regex : " + regex_word.ToString());
                searchWord = regex_word.ToString();
                isRegex = true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("FIND :" + word);
                searchWord = word;
            }

            XmlDocument tmpXmlDoc = Common.XmlControl.getInstance().xmlLoad(DEFINE.MEMO_DATA_PATH);
            XmlNode memoNode = tmpXmlDoc.SelectSingleNode("//MEMODATA");
            string path = "/";

            data.Clear();
            this.recursion_addChildMemo(memoNode, path, searchWord, isRegex);
            refresh_listview();
        }

        /// <summary>
        /// 재귀함수-단어검색 후 List형Data변수 채움
        /// </summary>
        /// <param name="pNode">root노드</param>
        /// <param name="path">검색 시작경로</param>
        /// <param name="searchWord">검색 단어</param>
        /// <param name="isRegex">정규식 사용여부</param>
        private void recursion_addChildMemo(XmlNode pNode, string path, string searchWord, bool isRegex)
        {
            RectangleConverter rc = new RectangleConverter();
            System.Text.RegularExpressions.Regex regex = null;
            if (isRegex)
            regex = new System.Text.RegularExpressions.Regex(searchWord, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            for (int i = 0; i < pNode.ChildNodes.Count; i++)
            {
                XmlNode tmpNode = pNode.ChildNodes[i];
                string type = tmpNode.Name;
                if (type == DEFINE.NODE_MEMONAME)
                {
                    string name = tmpNode.Attributes["name"].Value;
                    string text = tmpNode.Attributes["text"].Value;

                    if (searchWord != string.Empty)
                    {
                        //단어 포함되어 있지 않으면 PASS
                        if(isRegex)
                        {
                            if (!regex.IsMatch(name) && !regex.IsMatch(text))
                                continue;
                        }
                        else
                        {
                            if(name.IndexOf(searchWord, StringComparison.CurrentCultureIgnoreCase) == -1 &&
                                text.IndexOf(searchWord, StringComparison.CurrentCultureIgnoreCase) == -1)
                                continue;
                        }
                    }

                    Item item = new Item(name, text, DEFINE.FILETYPE.FILETYPE_TEXT);
                    item.setItemPath(path);

                    if (tmpNode.Attributes["rect"] != null)
                    {
                        string str_rect = tmpNode.Attributes["rect"].Value;
                        item.RECT = (Rectangle)rc.ConvertFromString(str_rect);
                    }
                    data.Add(item);
                }
                else if (type == DEFINE.NODE_GROUPNAME)
                {
                    bool isAdd = true;

                    string name = tmpNode.Attributes["name"].Value;

                    if (searchWord != string.Empty)
                    {
                        //단어 포함되어 있지 않으면 PASS
                        if (isRegex)
                        {
                            if (!regex.IsMatch(name))
                                isAdd = false;
                                //continue;
                        }
                        else
                        {
                            if (name.IndexOf(searchWord, StringComparison.CurrentCultureIgnoreCase) == -1)
                                isAdd = false;
                        }
                    }

                    if (isAdd)
                    {
                        Item item = new Item(name, "", DEFINE.FILETYPE.FILETYPE_DIRECTORY);
                        item.setItemPath(path);
                        if (tmpNode.ChildNodes.Count > 0)
                        {
                            item.EMPTY = false;
                            item.TAG = tmpNode.InnerXml;
                        }
                        data.Add(item);
                    }

                    if (tmpNode.ChildNodes.Count > 0)
                    {
                        string innerPath = path+name+"/";
                        recursion_addChildMemo(tmpNode, innerPath, searchWord, isRegex);
                    }
                }
            }
        }

        /// <summary>
        /// 한글이 포함된 글자일 경우 true
        /// </summary>
        /// <param name="str">판별할 문자열</param>
        /// <returns>한글포함 true, 한글 미포함 false</returns>
        private bool hasKoreanChar(string str)
        {
            bool returnVal = false;

            if (!String.IsNullOrWhiteSpace(str))
            {
                int charCode = 0;
                for (int i = 0; i < str.Length; i++)
                {
                    charCode = str[i];
                    if (charCode > 128)
                    {
                        returnVal = true;
                        break;
                    }
                }
            }

            return returnVal;
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            hideToolTip();
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (this.listView1.SelectedItems.Count > 0)
                    ctxt_listViewItem.Show(MousePosition);
                else
                    ctxt_listView.Show(MousePosition);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.XButton1)
            {
                if (this.btn_upperPath.Enabled)
                {
                    btn_upperPath_Click(null, null);
                }
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            this.openMemo();
        }

        private void ctxt_lstView_makeGroup_Click(object sender, EventArgs e)
        {
            XmlDocument tmpXmlDoc = Common.XmlControl.getInstance().xmlLoad(DEFINE.MEMO_DATA_PATH);
            XmlNode memoNode = tmpXmlDoc.SelectSingleNode("//MEMODATA");

            if (currentPath != "/")
            {
                String[] paths = currentPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < paths.Length; i++)
                {
                    Console.WriteLine("./" + DEFINE.NODE_GROUPNAME + "[@name='" + paths[i] + "']");
                    memoNode = memoNode.SelectSingleNode("./" + DEFINE.NODE_GROUPNAME + "[@name='" + paths[i] + "']");
                }
            }
            string groupName = this.get_defaultName(DEFINE.NODE_GROUPNAME, DEFINE.PREFIX_GROUPNAME);
            XmlNode tmpNode = this.makeGroupNode(tmpXmlDoc, groupName);
            memoNode.AppendChild(tmpNode);
            Common.XmlControl.getInstance().xmlSave(tmpXmlDoc, DEFINE.MEMO_DATA_PATH);

            this.listViewReload();
        }

        private void ctxt_lstView_makeMemo_Click(object sender, EventArgs e)
        {
            XmlDocument tmpXmlDoc = Common.XmlControl.getInstance().xmlLoad(DEFINE.MEMO_DATA_PATH);
            XmlNode memoNode = tmpXmlDoc.SelectSingleNode("//MEMODATA");

            if (currentPath != "/")
            {
                String[] paths = currentPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < paths.Length; i++)
                {
                    memoNode = memoNode.SelectSingleNode("./" + DEFINE.NODE_GROUPNAME + "[@name='" + paths[i] + "']");
                }
            }
            string memoName = get_defaultName(DEFINE.NODE_MEMONAME, DEFINE.PREFIX_MEMONAME);
            XmlNode tmpNode = makeMemoNode(tmpXmlDoc, memoName, "", Rectangle.Empty);
            memoNode.AppendChild(tmpNode);
            Common.XmlControl.getInstance().xmlSave(tmpXmlDoc, DEFINE.MEMO_DATA_PATH);

            this.listViewReload();
        }

        private void ctxt_lstItem_open_Click(object sender, EventArgs e)
        {
            this.openMemo();
        }

        private void ctxt_lstItem_delete_Click(object sender, EventArgs e)
        {
            this.deleteMemo();
        }

        /// <summary>
        /// 중복되지 않는 이름 생성 - currentPath기준
        /// </summary>
        /// <param name="prefix">접두사</param>
        /// <returns>접두사+Num</returns>
        private string get_defaultName(string nodeName, string prefix)
        {
            XmlDocument tmpXmlDoc = Common.XmlControl.getInstance().xmlLoad(DEFINE.MEMO_DATA_PATH);
            XmlNode memoNode = tmpXmlDoc.SelectSingleNode("//MEMODATA");

            if (currentPath != "/")
            {
                String[] paths = currentPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < paths.Length; i++)
                {
                    Console.WriteLine("./" + DEFINE.NODE_GROUPNAME + "[@name='" + paths[i] + "']");
                    memoNode = memoNode.SelectSingleNode("./" + DEFINE.NODE_GROUPNAME + "[@name='" + paths[i] + "']");
                }
            }

            String str = prefix;
            for (int i = 0; i < 10000; i++)
            {
                if (memoNode.SelectNodes("./" + nodeName + "[@name='" + str + i + "']").Count == 0)
                {
                    str += i;
                    break;
                }
            }
            return str;
        }

        /// <summary>
        /// 메모 노드 생성
        /// </summary>
        private XmlNode makeMemoNode(XmlDocument doc, string name, string text, Rectangle rect)
        {
            XmlNode tmpNode = doc.CreateNode(XmlNodeType.Element, DEFINE.NODE_MEMONAME, "");
            XmlAttribute attr1 = doc.CreateAttribute("name");
            attr1.Value = name;
            XmlAttribute attr2 = doc.CreateAttribute("text");
            attr2.Value = text;
            XmlAttribute attr3 = doc.CreateAttribute("rect");
            RectangleConverter rc = new RectangleConverter();
            attr3.Value = rc.ConvertToString(rect);
            tmpNode.Attributes.Append(attr1);
            tmpNode.Attributes.Append(attr2);
            tmpNode.Attributes.Append(attr3);
            return tmpNode;
        }

        /// <summary>
        /// 그룹 노드 생성
        /// </summary>
        private XmlNode makeGroupNode(XmlDocument doc, string name)
        {
            XmlNode tmpNode = doc.CreateNode(XmlNodeType.Element, DEFINE.NODE_GROUPNAME, "");
            XmlAttribute attr1 = doc.CreateAttribute("name");
            attr1.Value = name;
            tmpNode.Attributes.Append(attr1);
            return tmpNode;
        }

        /// <summary>
        /// 상위 디렉터리로 이동 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_upperPath_Click(object sender, EventArgs e)
        {
            string[] paths = currentPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (paths.Length > 1)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("/");
                for (int i = 0; i < paths.Length - 1; i++)
                {
                    sb.Append(paths[i]);
                    sb.Append("/");
                }
                SearchByPath(sb.ToString());
            }
            else
            {
                SearchByPath("/");
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                this.deleteMemo();
            }
            else if (e.KeyCode == Keys.F2 ? listView1.SelectedItems.Count == 1 : false)
            {
                listView1.SelectedItems[0].BeginEdit();
            }
            else if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
            {
                this.copyMemo();
            }
            else if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
            {
                if(!m_isSearchResult)
                    this.pasteMemo();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                this.openMemo();
            }
        }


        /// <summary>
        /// 메모 열기
        /// </summary>
        private void openMemo()
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                ListViewItem lstView_item = this.listView1.SelectedItems[0];
                Item item = data[lstView_item.Index];
                if (item.TYPE == DEFINE.FILETYPE.FILETYPE_DIRECTORY)
                {
                    string path = item.PATH + item.TITLE + "/";
                    SearchByPath(path);
                }
                else if (item.TYPE == DEFINE.FILETYPE.FILETYPE_TEXT)
                {
                    if (occurred_event != null)
                    {
                        occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_LOADMEMO, item);
                    }
                }
            }

        }

        /// <summary>
        /// 메모 삭제
        /// </summary>
        private void deleteMemo()
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                ListViewItem lstView_item;
                List<ListViewItem> delList = new List<ListViewItem>();
                for (int i = 0; i < this.listView1.SelectedItems.Count; i++)
                {
                    delList.Add(this.listView1.SelectedItems[i]);
                }
                for (int i = 0; i < delList.Count; i++)
                {
                    lstView_item = delList[i];
                    Item item = data[lstView_item.Index];
                    if (item.TYPE == DEFINE.FILETYPE.FILETYPE_DIRECTORY)
                    {
                        DialogResult result = MessageBox.Show("그룹 '" + item.TITLE + "'를 삭제하시겠습니까?\r\n* 그룹 삭제 시 하위항목도 같이 제거됩니다.", "", MessageBoxButtons.YesNo);
                        if (result == System.Windows.Forms.DialogResult.Yes)
                        {
                            if (occurred_event != null)
                            {
                                //if (occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_DELETEGROUP, item))
                                //    this.listViewReload();
                                occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_DELETEGROUP, item);
                            }
                        }
                    }
                    else if (item.TYPE == DEFINE.FILETYPE.FILETYPE_TEXT)
                    {
                        DialogResult result = MessageBox.Show("메모 '" + item.TITLE + "'를 삭제하시겠습니까?", "", MessageBoxButtons.YesNo);
                        if (result == System.Windows.Forms.DialogResult.Yes)
                        {
                            if (occurred_event != null)
                            {
                                //if (occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_DELETEMEMO, item))
                                //    this.listViewReload();
                                occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_DELETEMEMO, item);
                            }
                        }
                    }
                }

                //for (int i = this.listView1.SelectedItems.Count - 1; i >= 0; i--)
                //{
                //    lstView_item = this.listView1.SelectedItems[i];
                //    Item item = data[lstView_item.Index];
                //    if (item.TYPE == DEFINE.FILETYPE.FILETYPE_DIRECTORY)
                //    {
                //        DialogResult result = MessageBox.Show("그룹 '" + item.TITLE + "'를 삭제하시겠습니까?\r\n* 그룹 삭제 시 하위항목도 같이 제거됩니다.", "", MessageBoxButtons.YesNo);
                //        if (result == System.Windows.Forms.DialogResult.Yes)
                //        {
                //            if (occurred_event != null)
                //            {
                //                //if (occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_DELETEGROUP, item))
                //                //    this.listViewReload();
                //                occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_DELETEGROUP, item);
                //            }
                //        }
                //    }
                //    else if (item.TYPE == DEFINE.FILETYPE.FILETYPE_TEXT)
                //    {
                //        DialogResult result = MessageBox.Show("메모 '" + item.TITLE + "'를 삭제하시겠습니까?", "", MessageBoxButtons.YesNo);
                //        if (result == System.Windows.Forms.DialogResult.Yes)
                //        {
                //            if (occurred_event != null)
                //            {
                //                //if (occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_DELETEMEMO, item))
                //                //    this.listViewReload();
                //                occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_DELETEMEMO, item);
                //            }
                //        }
                //    }
                //}
                this.listViewReload();
            }
        }

        /// <summary>
        /// 메모 복사
        /// </summary>
        private void copyMemo()
        {
            m_copylst.RemoveRange(0, m_copylst.Count);

            if (this.listView1.SelectedItems.Count > 0)
            {
                for (int i = 0; i < this.listView1.SelectedItems.Count; i++)
                {
                    ListViewItem lstItem = this.listView1.SelectedItems[i];
                    Item item = data[lstItem.Index];
                    m_copylst.Add(item);
                }
            }
        }

        /// <summary>
        /// 메모 붙여넣기
        /// </summary>
        private void pasteMemo()
        {
            for (int idx = 0; idx < this.m_copylst.Count; idx++)
            {
                Item item = this.m_copylst[idx];

                XmlDocument tmpXmlDoc = Common.XmlControl.getInstance().xmlLoad(DEFINE.MEMO_DATA_PATH);
                XmlNode memoNode = tmpXmlDoc.SelectSingleNode("//MEMODATA");

                if (currentPath != "/")
                {
                    String[] paths = currentPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < paths.Length; i++)
                    {
                        memoNode = memoNode.SelectSingleNode("./" + DEFINE.NODE_GROUPNAME + "[@name='" + paths[i] + "']");
                    }
                }

                if (item.TYPE == DEFINE.FILETYPE.FILETYPE_TEXT)
                {
                    string memoName = get_defaultName(DEFINE.NODE_MEMONAME, item.TITLE + "_");
                    XmlNode tmpNode = makeMemoNode(tmpXmlDoc, memoName, item.TEXT, item.RECT);
                    memoNode.AppendChild(tmpNode);
                    Common.XmlControl.getInstance().xmlSave(tmpXmlDoc, DEFINE.MEMO_DATA_PATH);
                }
                else if (item.TYPE == DEFINE.FILETYPE.FILETYPE_DIRECTORY)
                {
                    string groupName = this.get_defaultName(DEFINE.NODE_GROUPNAME, item.TITLE + "_");
                    XmlNode tmpNode = this.makeGroupNode(tmpXmlDoc, groupName);
                    tmpNode.InnerXml = (string)item.TAG;
                    memoNode.AppendChild(tmpNode);
                    Common.XmlControl.getInstance().xmlSave(tmpXmlDoc, DEFINE.MEMO_DATA_PATH);
                }
                
                
            }

            this.listViewReload();
        }

        /// <summary>
        /// 메모 정보를 새로 읽어들임 - 메모데이터 경로 변경 시 사용
        /// </summary>
        public void ReloadMemoData()
        {
            this.SearchByPath("/");

        }

        #region # ListView Double Buffering Class
        public enum ListViewExtendedStyles
        {
            /// <summary>
            /// LVS_EX_GRIDLINES
            /// </summary>
            GridLines = 0x00000001,
            /// <summary>
            /// LVS_EX_SUBITEMIMAGES
            /// </summary>
            SubItemImages = 0x00000002,
            /// <summary>
            /// LVS_EX_CHECKBOXES
            /// </summary>
            CheckBoxes = 0x00000004,
            /// <summary>
            /// LVS_EX_TRACKSELECT
            /// </summary>
            TrackSelect = 0x00000008,
            /// <summary>
            /// LVS_EX_HEADERDRAGDROP
            /// </summary>
            HeaderDragDrop = 0x00000010,
            /// <summary>
            /// LVS_EX_FULLROWSELECT
            /// </summary>
            FullRowSelect = 0x00000020,
            /// <summary>
            /// LVS_EX_ONECLICKACTIVATE
            /// </summary>
            OneClickActivate = 0x00000040,
            /// <summary>
            /// LVS_EX_TWOCLICKACTIVATE
            /// </summary>
            TwoClickActivate = 0x00000080,
            /// <summary>
            /// LVS_EX_FLATSB
            /// </summary>
            FlatsB = 0x00000100,
            /// <summary>
            /// LVS_EX_REGIONAL
            /// </summary>
            Regional = 0x00000200,
            /// <summary>
            /// LVS_EX_INFOTIP
            /// </summary>
            InfoTip = 0x00000400,
            /// <summary>
            /// LVS_EX_UNDERLINEHOT
            /// </summary>
            UnderlineHot = 0x00000800,
            /// <summary>
            /// LVS_EX_UNDERLINECOLD
            /// </summary>
            UnderlineCold = 0x00001000,
            /// <summary>
            /// LVS_EX_MULTIWORKAREAS
            /// </summary>
            MultilWorkAreas = 0x00002000,
            /// <summary>
            /// LVS_EX_LABELTIP
            /// </summary>
            LabelTip = 0x00004000,
            /// <summary>
            /// LVS_EX_BORDERSELECT
            /// </summary>
            BorderSelect = 0x00008000,
            /// <summary>
            /// LVS_EX_DOUBLEBUFFER
            /// </summary>
            DoubleBuffer = 0x00010000,
            /// <summary>
            /// LVS_EX_HIDELABELS
            /// </summary>
            HideLabels = 0x00020000,
            /// <summary>
            /// LVS_EX_SINGLEROW
            /// </summary>
            SingleRow = 0x00040000,
            /// <summary>
            /// LVS_EX_SNAPTOGRID
            /// </summary>
            SnapToGrid = 0x00080000,
            /// <summary>
            /// LVS_EX_SIMPLESELECT
            /// </summary>
            SimpleSelect = 0x00100000
        }

        public enum ListViewMessages
        {
            First = 0x1000,
            SetExtendedStyle = (First + 54),
            GetExtendedStyle = (First + 55),
        }

        /// <summary>
        /// Contains helper methods to change extended styles on ListView, including enabling double buffering.
        /// Based on Giovanni Montrone's article on <see cref="http://www.codeproject.com/KB/list/listviewxp.aspx"/>
        /// </summary>
        public class ListViewHelper
        {
            private ListViewHelper()
            {
            }

            [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            private static extern int SendMessage(IntPtr handle, int messg, int wparam, int lparam);

            public static void SetExtendedStyle(Control control, ListViewExtendedStyles exStyle)
            {
                ListViewExtendedStyles styles;
                styles = (ListViewExtendedStyles)SendMessage(control.Handle, (int)ListViewMessages.GetExtendedStyle, 0, 0);
                styles |= exStyle;

                SendMessage(control.Handle, (int)ListViewMessages.SetExtendedStyle, 0, (int)styles);
            }

            public static void EnableDoubleBuffer(Control control)
            {
                ListViewExtendedStyles styles;
                // read current style
                styles = (ListViewExtendedStyles)SendMessage(control.Handle, (int)ListViewMessages.GetExtendedStyle, 0, 0);
                // enable double buffer and border select
                styles |= ListViewExtendedStyles.DoubleBuffer | ListViewExtendedStyles.BorderSelect;
                // write new style
                SendMessage(control.Handle, (int)ListViewMessages.SetExtendedStyle, 0, (int)styles);
            }
            public static void DisableDoubleBuffer(Control control)
            {
                ListViewExtendedStyles styles;
                // read current style
                styles = (ListViewExtendedStyles)SendMessage(control.Handle, (int)ListViewMessages.GetExtendedStyle, 0, 0);
                // disable double buffer and border select
                styles -= styles & ListViewExtendedStyles.DoubleBuffer;
                styles -= styles & ListViewExtendedStyles.BorderSelect;
                // write new style
                SendMessage(control.Handle, (int)ListViewMessages.SetExtendedStyle, 0, (int)styles);
            }
        }
        #endregion

        private void ctxt_lstItem_rename_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 1)
            {
                this.listView1.SelectedItems[0].BeginEdit();
            }
        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Label))
            {
                // 중복 이름 체크
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    if (listView1.Items[i].Text == e.Label)
                    {
                        MessageBox.Show("같은이름이 존재합니다");
                        e.CancelEdit = true;
                        return;
                    }
                }
                Item item = this.data[this.listView1.SelectedItems[0].Index];
                object tmp = new object[] { item, e.Label };
                occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_RENAME, tmp);
            }
        }

        Timer DragDropTimer = new Timer();
        private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            ListViewItem lstItem = (ListViewItem)e.Item;
            Item item = data[lstItem.Index];

            if (item.TYPE == DEFINE.FILETYPE.FILETYPE_TEXT) // 드래그중인 파일이 메모파일입니다 - 파일 생성
            {
                // 임시폴더 생성
                if (!System.IO.Directory.Exists(System.Windows.Forms.Application.StartupPath+"\\tmp"))
                {
                    System.IO.DirectoryInfo di = System.IO.Directory.CreateDirectory(System.Windows.Forms.Application.StartupPath + "\\tmp");
                    di.Attributes = System.IO.FileAttributes.Directory | System.IO.FileAttributes.Hidden;
                }

                string fileName = item.TITLE;
                string fileContents = item.TEXT;
                fileContents = fileContents.Replace("\n",System.Environment.NewLine);

                string fileFullPath = System.Windows.Forms.Application.StartupPath + "\\tmp\\" + fileName + ".txt";

                if(System.IO.File.Exists(fileFullPath))
                    System.IO.File.Delete(fileFullPath);

                System.IO.FileStream fs = new System.IO.FileStream(fileFullPath, System.IO.FileMode.Create);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, Encoding.UTF8);
                sw.Write(fileContents);
                sw.Close();
                fs.Close();

                // 150506
                // WriteAllText에서 스트림 자동으로 close해준다고 했는데 제대로 안됬음...
                // 그래서 DragDropTimer 작동될때 tmp폴더 delete 수행하는데 스트림을 안닫아놓은 상태라
                // '다른 프로세스가 사용중' 이라는 오류 출력됨

                //System.IO.File.WriteAllText(System.Windows.Forms.Application.StartupPath + "\\tmp\\" + fileName + ".txt", fileContents, Encoding.Unicode);
                Console.WriteLine(System.Windows.Forms.Application.StartupPath);

                
                string[] files = new string[] {fileFullPath};

                this.DoDragDrop(new DataObject(DataFormats.FileDrop, files), DragDropEffects.All);
            }
            else
            {
                DoDragDrop(item, DragDropEffects.All);
            }
            DragDropTimer.Tick += DragDropTimer_Tick;
            DragDropTimer.Interval = 5;
            DragDropTimer.Start();
        }

        void DragDropTimer_Tick(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            if (Control.MouseButtons != System.Windows.Forms.MouseButtons.Left)
            {
                DragDropTimer.Stop();
                DragDropTimer.Tick -= DragDropTimer_Tick;

                if (System.IO.Directory.Exists(System.Windows.Forms.Application.StartupPath + "\\tmp"))
                {
                    System.IO.Directory.Delete(System.Windows.Forms.Application.StartupPath + "\\tmp", true);
                }
            }
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (recently_searchType == 1)
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            object folder = e.Data.GetData("FolderMemo.Item");
            if (fileNames != null || folder != null)
            {
                if (System.IO.Directory.Exists(System.Windows.Forms.Application.StartupPath + "\\tmp") || folder != null)
                    e.Effect = DragDropEffects.Move;
                else
                    e.Effect = DragDropEffects.Copy;
            }
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            object folder = e.Data.GetData("FolderMemo.Item");

            if ((fileNames != null ? fileNames.Length > 1 : true) && folder == null)
                return;

            bool isFolderMemoDragStart = false; // 폴더 폼에서 Drag를 스타트 한 경우 true

            if (fileNames != null)
            {
                if (System.IO.Directory.Exists("tmp"))
                {
                    string[] innerfileList = System.IO.Directory.GetFiles("tmp");
                    for (int i = 0; i < innerfileList.Length; i++)
                    {
                        if (fileNames[0] == (System.Windows.Forms.Application.StartupPath + "\\" + innerfileList[i]))
                        {
                            isFolderMemoDragStart = true;
                            break;
                        }
                    }
                }
            }
            else if (folder != null)
                isFolderMemoDragStart = true;
            
            if (isFolderMemoDragStart) // 폴더 폼에서 드래그를 수행한 경우
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    ListViewItem item1 = listView1.SelectedItems[0];
                    //Returns the location of the mouse pointer in the ListView control.
                    Point cp = listView1.PointToClient(new Point(e.X, e.Y));
                    //Obtain the item that is located at the specified location of the mouse pointer.
                    ListViewItem item2 = listView1.GetItemAt(cp.X, cp.Y);
                    if (item2 != null && (item1 != item2))
                    {
                        //Console.WriteLine(item1.Text + ":" + item2.Text + " replace");
                        this.insertAtFrontItem(item1, item2, item1.Index > item2.Index);
                        //this.replaceItem(item1, item2);
                    }
                    else if((item1 == item2))
                        Console.WriteLine("Item1 == Item2");
                    else
                        Console.WriteLine("item2 is null");
                }
            }
            else // 외부에서 파일을 가져다가 끌어온 경우
            {
                Console.WriteLine("외부파일 입니다 : " + fileNames[0]);

                if(System.IO.Path.GetExtension(fileNames[0]) == ".txt")
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(fileNames[0], Encoding.Default, true);
                    string title = System.IO.Path.GetFileNameWithoutExtension(fileNames[0]);
                    string text = sr.ReadToEnd();

                    XmlDocument tmpXmlDoc = Common.XmlControl.getInstance().xmlLoad(DEFINE.MEMO_DATA_PATH);
                    XmlNode memoNode = tmpXmlDoc.SelectSingleNode("//MEMODATA");

                    if (currentPath != "/")
                    {
                        String[] paths = currentPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < paths.Length; i++)
                        {
                            Console.WriteLine("./" + DEFINE.NODE_GROUPNAME + "[@name='" + paths[i] + "']");
                            memoNode = memoNode.SelectSingleNode("./" + DEFINE.NODE_GROUPNAME + "[@name='" + paths[i] + "']");
                        }
                    }
                    bool isDuplicate = false;
                    // 중복 검사
                    for (int i = 0; i < memoNode.ChildNodes.Count; i++)
                    {
                        XmlNode node = memoNode.ChildNodes[i];
                        if (node.Attributes["name"].Value == title && node.Name == DEFINE.NODE_MEMONAME)
                        {
                            // 중복사항 존재
                            // 덮어씌우겠습니까? 알림 뜨게하기
                            isDuplicate = true;
                            Console.WriteLine("중복사항 존재~!!!!!");
                            break;
                        }
                    }

                    if (!isDuplicate)
                    {
                        XmlNode node = this.makeMemoNode(tmpXmlDoc, title, text, Rectangle.Empty);
                        memoNode.AppendChild(node);
                        Common.XmlControl.getInstance().xmlSave(tmpXmlDoc, DEFINE.MEMO_DATA_PATH);
                    }

                    this.listViewReload();
                }
            }
        }


        /// <summary>
        /// ListView에서 두개의 아이템의 위치를 변경합니다.
        /// </summary>
        /// <param name="lstItem1"></param>
        /// <param name="lstItem2"></param>
        private void replaceItem(ListViewItem lstItem1, ListViewItem lstItem2)
        {
            if (lstItem1 == lstItem2)
                return;

            Item item1 = data[lstItem1.Index];
            Item item2 = data[lstItem2.Index];


            XmlDocument tmpXmlDoc = Common.XmlControl.getInstance().xmlLoad(DEFINE.MEMO_DATA_PATH);
            XmlNode memoNode = tmpXmlDoc.SelectSingleNode("//MEMODATA");
            string item2Path = item2.PATH;
            if (item2Path != "/")
            {
                String[] paths = item2Path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < paths.Length; i++)
                {
                    memoNode = memoNode.SelectSingleNode("./" + DEFINE.NODE_GROUPNAME + "[@name='" + paths[i] + "']");
                }
            }

            XmlNode item1Node = memoNode.SelectSingleNode("./" + (item1.TYPE == DEFINE.FILETYPE.FILETYPE_DIRECTORY ? DEFINE.NODE_GROUPNAME : DEFINE.NODE_MEMONAME) + "[@name='" + item1.TITLE + "']");
            XmlNode item2Node = memoNode.SelectSingleNode("./" + (item2.TYPE == DEFINE.FILETYPE.FILETYPE_DIRECTORY ? DEFINE.NODE_GROUPNAME : DEFINE.NODE_MEMONAME) + "[@name='" + item2.TITLE + "']");

            // item2가 Group일 경우
            if (item2.TYPE == DEFINE.FILETYPE.FILETYPE_DIRECTORY)
            {
                if (MessageBox.Show("디렉토리 안에 넣으실껀가요?", "궁금", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    item2Node.InnerXml += item1Node.OuterXml;
                    item1Node.ParentNode.RemoveChild(item1Node);

                    Common.XmlControl.getInstance().xmlSave(tmpXmlDoc, DEFINE.MEMO_DATA_PATH);
                    this.listViewReload();

                    return;
                }
            }

            // 위치 변경
            XmlNode tmpNode = item2Node.Clone();
            memoNode.ReplaceChild(item1Node.Clone(), item2Node);
            memoNode.ReplaceChild(tmpNode, item1Node);

            Common.XmlControl.getInstance().xmlSave(tmpXmlDoc, DEFINE.MEMO_DATA_PATH);

            this.listViewReload();
        }

        /// <summary>
        /// 1번째 아이템의 위치를 2번째 아이템의 앞 또는 뒤로 이동합니다.
        /// </summary>
        /// <param name="lstItem1"></param>
        /// <param name="lstItem2"></param>
        /// <param name="isFront">TRUE:앞, FALSE:뒤</param>
        private void insertAtFrontItem(ListViewItem lstItem1, ListViewItem lstItem2, bool isFront)
        {
            Console.WriteLine("lstItem1 go " + (isFront ? "Front" : "Back") + " lstItem2");

            if (lstItem1 == lstItem2)
                return;

            Item item1 = data[lstItem1.Index];
            Item item2 = data[lstItem2.Index];


            XmlDocument tmpXmlDoc = Common.XmlControl.getInstance().xmlLoad(DEFINE.MEMO_DATA_PATH);
            XmlNode memoNode = tmpXmlDoc.SelectSingleNode("//MEMODATA");
            string item2Path = item2.PATH;
            if (item2Path != "/")
            {
                String[] paths = item2Path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < paths.Length; i++)
                {
                    memoNode = memoNode.SelectSingleNode("./" + DEFINE.NODE_GROUPNAME + "[@name='" + paths[i] + "']");
                }
            }

            XmlNode item1Node = memoNode.SelectSingleNode("./" + (item1.TYPE == DEFINE.FILETYPE.FILETYPE_DIRECTORY ? DEFINE.NODE_GROUPNAME : DEFINE.NODE_MEMONAME) + "[@name='" + item1.TITLE + "']");
            XmlNode item2Node = memoNode.SelectSingleNode("./" + (item2.TYPE == DEFINE.FILETYPE.FILETYPE_DIRECTORY ? DEFINE.NODE_GROUPNAME : DEFINE.NODE_MEMONAME) + "[@name='" + item2.TITLE + "']");

            // item2가 Group일 경우
            if (item2.TYPE == DEFINE.FILETYPE.FILETYPE_DIRECTORY)
            {
                if (MessageBox.Show("디렉토리 안에 넣으실껀가요?", "궁금", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    item2Node.InnerXml += item1Node.OuterXml;
                    item1Node.ParentNode.RemoveChild(item1Node);

                    Common.XmlControl.getInstance().xmlSave(tmpXmlDoc, DEFINE.MEMO_DATA_PATH);
                    this.listViewReload();

                    return;
                }
            }

            XmlNode tmpNode = item1Node.Clone();
            // 위치 변경
            if (isFront)
            {
                memoNode.InsertBefore(tmpNode, item2Node);

                //Item tmpItem = data[lstItem1.Index];
                //data.RemoveAt(lstItem1.Index);
                //data.Insert(lstItem2.Index, tmpItem);

                //listView1.BeginUpdate();
                //ListViewItem lstItem1Clone = (ListViewItem)lstItem1.Clone();
                //listView1.Items.RemoveAt(lstItem1.Index);
                //listView1.Items.Insert(lstItem2.Index-1, lstItem1Clone);
                ////listView1.EndUpdate();
                //Console.WriteLine(lstItem1Clone.Index);
            }
            else
            {
                memoNode.InsertAfter(tmpNode, item2Node);

                //Item tmpItem = data[lstItem1.Index];
                //data.RemoveAt(lstItem1.Index);
                //data.Insert(lstItem2.Index+1, tmpItem);

                ////listView1.BeginUpdate();
                //ListViewItem lstItem1Clone = (ListViewItem)lstItem1.Clone();
                //listView1.Items.RemoveAt(lstItem1.Index);
                //listView1.Items.Insert(lstItem2.Index+1, lstItem1Clone);
                ////listView1.EndUpdate();
            }
            item1Node.ParentNode.RemoveChild(item1Node);

            Common.XmlControl.getInstance().xmlSave(tmpXmlDoc, DEFINE.MEMO_DATA_PATH);


            this.listViewReload();
        }

        private void listView1_DragOver(object sender, DragEventArgs e)
        {
            /* 한 row의 마지막 column의 우측에 InsertionMark표시 버그(다음row의 첫번째 아이템의 우측을 가리킴) */
            //// Retrieve the client coordinates of the mouse pointer.
            //Point targetPoint =
            //    listView1.PointToClient(new Point(e.X, e.Y));

            //// Retrieve the index of the item closest to the mouse pointer.
            //int targetIndex = listView1.InsertionMark.NearestIndex(targetPoint);

            //// Confirm that the mouse pointer is not over the dragged item.
            //if (targetIndex > -1)
            //{
            //    Console.WriteLine(targetIndex);

            //    // Determine whether the mouse pointer is to the left or
            //    // the right of the midpoint of the closest item and set
            //    // the InsertionMark.AppearsAfterItem property accordingly.
            //    Rectangle itemBounds = listView1.GetItemRect(targetIndex);
            //    if (targetPoint.X > itemBounds.Left + (itemBounds.Width / 2))
            //    {
            //        listView1.InsertionMark.AppearsAfterItem = true;
            //    }
            //    else
            //    {
            //        listView1.InsertionMark.AppearsAfterItem = false;
            //    }
            //}

            //// Set the location of the insertion mark. If the mouse is
            //// over the dragged item, the targetIndex value is -1 and
            //// the insertion mark disappears.
            //listView1.InsertionMark.Index = targetIndex;
        }

        private void txt_search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btn_search_Click(null, null);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void ctxt_lstItem_copy_Click(object sender, EventArgs e)
        {
            this.copyMemo();
        }

        private void ctxt_lstView_paste_Click(object sender, EventArgs e)
        {
            if(!m_isSearchResult)
                this.pasteMemo();
        }

        private void ctxt_listView_Opening(object sender, CancelEventArgs e)
        {
            // 그룹 및 메모 생성 활성화
            this.ctxt_lstView_makeGroup.Enabled = !m_isSearchResult;
            this.ctxt_lstView_makeMemo.Enabled = !m_isSearchResult;
            this.ctxt_lstView_paste.Enabled = !m_isSearchResult && m_copylst.Count > 0;
        }

        private void ctxt_form_setting_Click(object sender, EventArgs e)
        {
            // 설정화면 출력
            SystemTray.getInstance().ShowSettingForm(null, null);
        }
    }
}
