using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using System.IO; // DirectoryInfo, FileInfo
using IWshRuntimeLibrary;

namespace FolderMemo
{
    class SystemTray : Form
    {
        private const string m_appName_folderMemo = "FolderMemo.exe";
        private const string m_appName_folderMemoAutoUpdate = "FolderMemoUpdate.exe";
        //private const string m_runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu; //트레이 우클릭 메뉴
        private static Form_Folder mainFolder;
        private static Form_Setting mainSetting;
        private static FolderMemoAboutBox m_form_about; // About폼
        private static XmlDocument configDoc;
        //private List<Form_Memo> memoForms = new List<Form_Memo>();
        private List<Form_Memo_RIchText> memoForms = new List<Form_Memo_RIchText>();
        private string m_default_memo_path = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(Application.ExecutablePath.ToString())
            , System.Windows.Forms.Application.StartupPath + "\\" + DEFINE.MEMO_DATA_FILENAME); // 메모데이터 기본위치값

        private static SystemTray m_instance;

        private SystemTray()
        {

        }

        public static SystemTray getInstance()
        {
            if (m_instance == null)
                m_instance = new SystemTray();
            return m_instance;
        }

        protected override void OnLoad(EventArgs e)
        {
            ///*************** 중복 실행 방지 ********************/
            //bool isNew;
            //System.Threading.Mutex mutex = new System.Threading.Mutex(true, "FolderMemo", out isNew);

            //if (isNew)
            //    mutex.ReleaseMutex();
            //else
            //{
            //    this.Close();
            //    return;
            //}
            ///**************************************************/

            // Create a simple tray menu with only one item.
            // 시스템 트레이 우클릭 메뉴 설정
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("&About", ShowAboutThis);
            trayMenu.MenuItems.Add("&Update", runUpdate);
            trayMenu.MenuItems.Add("-");

            MenuItem menu_startup = new MenuItem("&StartUp_FolderMemo", setup_startup_FolderMemo);
            menu_startup.Tag = m_appName_folderMemo;
            // 윈도우 시작시 자동 실행 설정
            if (this.getStartUpYN_FolderMemo())
                menu_startup.Checked = true;
            trayMenu.MenuItems.Add(menu_startup);
            
            MenuItem menu_startup_autoUpdate = new MenuItem("&StartUp_AutoUpdate", setup_startup_FolderMemoAutoUpdate);
            menu_startup_autoUpdate.Tag = m_appName_folderMemoAutoUpdate;
            // 윈도우 시작시 자동 실행 설정
            if (this.getStartUpYN_FolderMemoAutoUpdate())
                menu_startup_autoUpdate.Checked = true;
            trayMenu.MenuItems.Add(menu_startup_autoUpdate);

            trayMenu.MenuItems.Add("&Setting", ShowSettingForm);
            
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("E&xit", OnExit);
            trayMenu.Popup += trayMenu_Popup;

            // Create a tray icon. In this exaple we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon = new NotifyIcon();
            trayIcon.Text = DEFINE.TRAY_NAME;
            //trayIcon.Icon        = new Icon(SystemIcons.Application, new Size(40,40));
            trayIcon.Icon = new System.Drawing.Icon(Properties.Resources.icon_stick_note_16x, new Size(16, 16));

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            trayIcon.DoubleClick += trayIcon_DoubleClick;
            trayIcon.ShowBalloonTip(3000, DEFINE.TRAY_NAME, DEFINE.TRAY_NAME + " 실행~!", ToolTipIcon.Info);


            Visible = false; // Hide form window.
            Opacity = 0;
            //ShowInTaskbar = false; // Remove from taskbar.

            CheckFile.instance.fileInfoCheck(System.Windows.Forms.Application.StartupPath + "\\" + DEFINE.CONFIG_FILENAME, System.Windows.Forms.Application.StartupPath + "\\" + DEFINE.MEMO_DATA_FILENAME);

            // 환경설정 LOAD
            configDoc = Common.XmlControl.getInstance().xmlLoad(System.Windows.Forms.Application.StartupPath +"\\" + DEFINE.CONFIG_FILENAME);
            XmlNode settingNode = configDoc.SelectSingleNode("//SETTING");
            
            if (settingNode == null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
                sb.Append("<SETTING>\r\n");
                sb.Append("</SETTING>\r\n");
                configDoc.LoadXml(sb.ToString());

                //Default Setting
                settingNode = configDoc.SelectSingleNode("//SETTING");
                List<String[]> tmpList = new List<string[]>();
                string[] tmpStrArr1 = { DEFINE.CONFIG_SETTING_TOPMOST, "FALSE" }; // TOPMOST
                string[] tmpStrArr2 = { DEFINE.CONFIG_SETTING_CLOSERECT, "DEFAULT" };      // Folder Form's Rectangle
                string[] tmpStrArr3 = { DEFINE.CONFIG_SETTING_MEMODATAPATH, m_default_memo_path }; // 메모데이터 기본위치값

                tmpList.Add(tmpStrArr1);
                tmpList.Add(tmpStrArr2);
                tmpList.Add(tmpStrArr3);

                for (int i = 0; i < tmpList.Count; i++)
                {
                    String nodeName = tmpList[i][0];
                    String nodeValue = tmpList[i][1];

                    XmlNode tmpNode = configDoc.CreateNode(XmlNodeType.Element, nodeName, "");
                    tmpNode.InnerText = nodeValue;

                    settingNode.AppendChild(tmpNode);
                }

                if (Common.XmlControl.getInstance().xmlSave(configDoc, System.Windows.Forms.Application.StartupPath +"\\" + DEFINE.CONFIG_FILENAME))
                    configDoc = Common.XmlControl.getInstance().xmlLoad(System.Windows.Forms.Application.StartupPath +"\\" + DEFINE.CONFIG_FILENAME);

                settingNode = configDoc.SelectSingleNode("//SETTING");
            }

            // 메모데이터 경로 Setting
            string memo_path = null;
            XmlNode MemoPathNode = settingNode.SelectSingleNode(String.Format("./{0}", DEFINE.CONFIG_SETTING_MEMODATAPATH));
            if (MemoPathNode != null)
            {
                memo_path = MemoPathNode.InnerText;
            }
            this.loadMemoFile(memo_path);

            this.Icon = Properties.Resources.icon_stick_note_32x;
            this.Text = "FolderMemo";

            base.OnLoad(e);
        }
        
        private void loadMemoFile(string memoFile)
        {
            string tmp_memo_path = memoFile;
            if (!System.IO.File.Exists(memoFile))
            {
                // 메모파일이 존재하지 않음
                tmp_memo_path = m_default_memo_path;
            }

            XmlDocument init_memo_doc = Common.XmlControl.getInstance().xmlLoad(tmp_memo_path);
            if (init_memo_doc.SelectSingleNode("//MEMODATA") == null)
            {
                MessageBox.Show("해당 경로 파일은 메모데이터가 아닙니다.");
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = "데이터 파일 열기";
                dialog.Filter = "Data파일(*.xml)|*.xml";
                dialog.FileOk += openMemoFileDialog_FileOk;
                dialog.ShowDialog();
            }
            else {
                DEFINE.MEMO_DATA_PATH = tmp_memo_path;
                XmlNode settingNode = configDoc.SelectSingleNode("//SETTING");
                XmlNode MemoPathNode = settingNode.SelectSingleNode(String.Format("./{0}", DEFINE.CONFIG_SETTING_MEMODATAPATH));
                MemoPathNode.InnerText = tmp_memo_path;
                if (Common.XmlControl.getInstance().xmlSave(configDoc, System.Windows.Forms.Application.StartupPath + "\\" + DEFINE.CONFIG_FILENAME))
                    configDoc = Common.XmlControl.getInstance().xmlLoad(System.Windows.Forms.Application.StartupPath + "\\" + DEFINE.CONFIG_FILENAME);

                // 최근 Open한 메모데이터 경로 Setting
                reload_recent_memo_data();

                // 메인폼 Show
                showFormFolder();
            }
        }

        private void openMemoFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is OpenFileDialog)
            {
                OpenFileDialog dialog = (OpenFileDialog)sender;
                string fileName = dialog.FileName;
                loadMemoFile(fileName);
            }
        }


        void trayMenu_Popup(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        // If you doubleclick tray icon, show memoFolder (TrayIcon 더블 클릭 시)
        private void trayIcon_DoubleClick(object sender, EventArgs e)
        {
            showFormFolder();
        }

        /// <summary>
        /// 폴더 폼 생성
        /// </summary>
        private void showFormFolder()
        {
            if (mainFolder == null)
            {
                mainFolder = new Form_Folder();

                // mainFolder 폼이 닫힐때 실행될 이벤트 할당
                mainFolder.FormClosed += delegate(Object tmp_sender, FormClosedEventArgs tmp_e)
                {
                    mainFolder.Dispose();
                    mainFolder = null;
                    trayIcon.Text = DEFINE.TRAY_NAME;
                };

                // mainFolder 폼의 설정값이 변경될때 실행될 이벤트 할당
                mainFolder.throw_Environment_SettingChanged += mainFolder_throw_Environment_SettingChanged;


                // mainFolder TopMost 여부 지정
                mainFolder.TopMost = configDoc.SelectSingleNode("//SETTING/" + DEFINE.CONFIG_SETTING_TOPMOST).InnerText == "TRUE" ? true : false;


                // mainFolder 크기 지정(기존에 저장된 위치값 있는지)
                #region
                String str_mainFolder_rect = configDoc.SelectSingleNode("//SETTING/" + DEFINE.CONFIG_SETTING_CLOSERECT).InnerText;
                if (!String.IsNullOrEmpty(str_mainFolder_rect) && str_mainFolder_rect != "DEFAULT")
                {
                    RectangleConverter rc = new RectangleConverter();
                    Rectangle tmpRect = (Rectangle)rc.ConvertFromString(str_mainFolder_rect);

                    Screen[] sc = Screen.AllScreens;
                    for (int i = 0; i < sc.Length; i++)
                    {
                        if (sc[i].WorkingArea.Contains(tmpRect)) //맞는 화면 존재 시
                        {
                            mainFolder.Location = tmpRect.Location;
                            mainFolder.Size = tmpRect.Size;
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

                            mainFolder.Location = tmpRect.Location;
                            mainFolder.Size = tmpRect.Size;
                            break;
                        }
                        else
                        {
                            //mainFolder.Size = tmpRect.Size;
                            Rectangle primaryScreen_bounds = Screen.PrimaryScreen.Bounds;
                            Size mainFolder_size = DEFINE.DEFAULT_FOLDER_SIZE;
                            mainFolder.DesktopBounds = new Rectangle((primaryScreen_bounds.Width - mainFolder_size.Width) - 3, 3, mainFolder_size.Width, mainFolder_size.Height);
                        }
                    }
                }
                else
                {
                    Rectangle primaryScreen_bounds = Screen.PrimaryScreen.Bounds;
                    Size mainFolder_size = DEFINE.DEFAULT_FOLDER_SIZE;
                    mainFolder.DesktopBounds = new Rectangle((primaryScreen_bounds.Width - mainFolder_size.Width) - 3, 3, mainFolder_size.Width, mainFolder_size.Height);
                }
                mainFolder.MinimumSize = DEFINE.DEFAULT_FOLDER_SIZE;
                #endregion

                mainFolder.occurred_event += mainFolder_occurred_event;

                mainFolder.Text = DEFINE.TRAY_NAME;
                mainFolder.ShowInTaskbar = false;
                mainFolder.Show(this);

                //trayIcon.Text = DEFINE.TRAY_NAME + " (RUNNING...)";
            }
            else
            {
                mainFolder.Activate();
            }
        }

        /// <summary>
        /// 폴더 폼에서 이벤트 발생
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool mainFolder_occurred_event(DEFINE.EVENTTYPE type, object obj)
        {
            // 메모 폼 OPEN EVENT 발생
            if (type == DEFINE.EVENTTYPE.EVENTTYPE_LOADMEMO)
            {
                Item item = (Item)obj;
                // 중복 생성 방지
                for (int i = 0; i < memoForms.Count; i++)
                {
                    Item tmpItem = memoForms[i].g_item;
                    if (tmpItem.PATH == item.PATH && tmpItem.TITLE == item.TITLE)
                    {
                        memoForms[i].textBoxFocus();
                        return true;
                    }
                }

                //Form_Memo memo = new Form_Memo();
                Form_Memo_RIchText memo = new Form_Memo_RIchText();
                memo.g_item = item;
                memo.ShowInTaskbar = false;
                memo.Show(this);
                memo.occurred_event += memo_occurred_event;

                memoForms.Add(memo);
            }
            else if (type == DEFINE.EVENTTYPE.EVENTTYPE_DELETEMEMO)
            {
                Item item = (Item)obj;
                // 메모 폼이 현재 열려있으면 폼을 닫음
                for (int i = memoForms.Count -1; i >= 0 ; i--)
                {
                    Item tmpItem = memoForms[i].g_item;
                    if (tmpItem.PATH == item.PATH && tmpItem.TITLE == item.TITLE)
                    {
                        memoForms[i].closeForce = true;
                        memoForms[i].Close();
                        //memoForms.RemoveAt(i);
                        break;
                    }
                }

                XmlDocument xmlDoc = Common.XmlControl.getInstance().xmlLoad(DEFINE.MEMO_DATA_PATH);
                XmlNode memoNode = xmlDoc.SelectSingleNode("//MEMODATA");
                String path = item.PATH;
                if (path != "/")
                {
                    String[] paths = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < paths.Length; i++)
                    {
                        memoNode = memoNode.SelectSingleNode("./" + DEFINE.NODE_GROUPNAME + "[@name='" + paths[i] + "']");
                    }
                }

                XmlNode targetNode = memoNode.SelectSingleNode("./"+ DEFINE.NODE_MEMONAME + "[@name='" + item.TITLE + "']");
                targetNode.ParentNode.RemoveChild(targetNode);

                Common.XmlControl.getInstance().xmlSave(xmlDoc, DEFINE.MEMO_DATA_PATH);

            }
            else if (type == DEFINE.EVENTTYPE.EVENTTYPE_DELETEGROUP)
            {
                Item item = (Item)obj;

                // 해당 그룹에 속하는 메모 폼이 현재 열려있으면 폼을 닫음
                for (int i = memoForms.Count - 1; i >= 0; i--)
                {
                    Item tmpItem = memoForms[i].g_item;
                    if (tmpItem.PATH != "/")
                    {
                        String[] paths = tmpItem.PATH.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        string groupPath = item.PATH + item.TITLE;
                        String[] paths2 = groupPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        bool isGroupChild = true;
                        for (int j = 0; j < paths2.Length; j++)
                        {
                            if (paths.Length < paths2.Length ? true : paths2[j] != paths[j])
                            {
                                isGroupChild = false;
                                break;
                            }
                        }
                        if (isGroupChild)
                        {
                            //memoForms.Remove(memoForms[i]);
                            memoForms[i].closeForce = true;
                            memoForms[i].Close();
                        }
                    }
                }

                XmlDocument xmlDoc = Common.XmlControl.getInstance().xmlLoad(DEFINE.MEMO_DATA_PATH);
                XmlNode memoNode = xmlDoc.SelectSingleNode("//MEMODATA");
                if (item.PATH != "/")
                {
                    string[] paths = item.PATH.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < paths.Length; i++)
                        memoNode = memoNode.SelectSingleNode("./" + DEFINE.NODE_GROUPNAME + "[@name='" + paths[i] + "']");
                }
                XmlNode targetNode = memoNode.SelectSingleNode("./"+ DEFINE.NODE_GROUPNAME + "[@name='" + item.TITLE + "']");
                targetNode.ParentNode.RemoveChild(targetNode);

                Common.XmlControl.getInstance().xmlSave(xmlDoc, DEFINE.MEMO_DATA_PATH);

            }
            else if (type == DEFINE.EVENTTYPE.EVENTTYPE_RENAME) // 이름 변경 시
            {
                Item item = (Item)(((object[])obj)[0]);
                string str_newName = (string)(((object[])obj)[1]);

                renameItem(item, str_newName);
            }
            return true;
            //throw new NotImplementedException();
        }

        /// <summary>
        /// 폴더 폼 종료 시 Rectangle 저장
        /// </summary>
        /// <param name="keyValue"></param>
        private void mainFolder_throw_Environment_SettingChanged(List<string[]> keyValue)
        {
            for (int i = 0; i < keyValue.Count; i++)
            {
                String[] strArr = keyValue[i];
                String keyType = strArr[0];
                String keyName = strArr[1];
                String Value = strArr[2];

                if (keyType == DEFINE.NODETYPE_STRING)
                {
                    XmlNode tmpNode = configDoc.SelectSingleNode("//SETTING/" + keyName);
                    tmpNode.InnerText = Value;
                }
                configDoc.Save(System.Windows.Forms.Application.StartupPath +"\\" + DEFINE.CONFIG_FILENAME);
            }

            //throw new NotImplementedException();
        }

        /// <summary>
        /// 메모 폼에서 이벤트 발생
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool memo_occurred_event(DEFINE.EVENTTYPE type, object obj)
        {
            // 메모 폼 Close 시
            if (type == DEFINE.EVENTTYPE.EVENTTYPE_CLOSEMEMO)
            {
                object sender = ((object[])obj)[0];
                Item item = (Item)(((object[])obj)[1]);
                bool isSave = (string)((object[])obj)[2] == "Y" ? true : false;

                if(isSave)
                    saveMemo(item);

                //memoForms.Remove((Form_Memo)sender);
                memoForms.Remove((Form_Memo_RIchText)sender);
            }
            else if (type == DEFINE.EVENTTYPE.EVENTTYPE_SAVEMEMO) // 메모 폼 Save 시
            {
                Item item = (Item)obj;
                saveMemo(item);
            }
            return true;
            //throw new NotImplementedException();
        }

        /// <summary>
        /// 메모폼 정보 저장
        /// </summary>
        private void saveMemo(Item item)
        {
            XmlDocument xmlDoc = Common.XmlControl.getInstance().xmlLoad(DEFINE.MEMO_DATA_PATH);
            XmlNode memoNode = xmlDoc.SelectSingleNode("//MEMODATA");
            String path = item.PATH;
            if (path != "/")
            {
                String[] paths = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < paths.Length; i++)
                {
                    memoNode = memoNode.SelectSingleNode("./" + DEFINE.NODE_GROUPNAME + "[@name='"+ paths[i]+"']");
                }
            }

            XmlNode targetNode = memoNode.SelectSingleNode("./" + DEFINE.NODE_MEMONAME + "[@name='" + item.TITLE+"']");
            RectangleConverter rc = new RectangleConverter();

            XmlNode tmpNode = xmlDoc.CreateNode(XmlNodeType.Element, targetNode.Name, "");
            XmlAttribute attr1 = xmlDoc.CreateAttribute("name");
            attr1.Value = item.TITLE;
            XmlAttribute attr2 = xmlDoc.CreateAttribute("text");
            attr2.Value = item.TEXT;
            XmlAttribute attr3 = xmlDoc.CreateAttribute("rect");
            attr3.Value = rc.ConvertToString(item.RECT);
            tmpNode.Attributes.Append(attr1);
            tmpNode.Attributes.Append(attr2);
            tmpNode.Attributes.Append(attr3);

            targetNode.ParentNode.ReplaceChild(tmpNode, targetNode);

            Common.XmlControl.getInstance().xmlSave(xmlDoc, DEFINE.MEMO_DATA_PATH);

            if(mainFolder != null)
                mainFolder.listViewReload();
        }


        /// <summary>
        /// 이름 정보 변경
        /// </summary>
        /// <param name="item"></param>
        private void renameItem(Item item, string newName)
        {
            XmlDocument xmlDoc = Common.XmlControl.getInstance().xmlLoad(DEFINE.MEMO_DATA_PATH);
            XmlNode memoNode = xmlDoc.SelectSingleNode("//MEMODATA");
            String path = item.PATH;
            if (path != "/")
            {
                String[] paths = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < paths.Length; i++)
                {
                    memoNode = memoNode.SelectSingleNode("./" + DEFINE.NODE_GROUPNAME + "[@name='" + paths[i] + "']");
                }
            }

            string targetType = item.TYPE == DEFINE.FILETYPE.FILETYPE_TEXT ? DEFINE.NODE_MEMONAME : DEFINE.NODE_GROUPNAME;
            XmlNode targetNode = memoNode.SelectSingleNode("./" + targetType + "[@name='" + item.TITLE + "']");
            

            XmlNode tmpNode = xmlDoc.CreateNode(XmlNodeType.Element, targetNode.Name, "");
            XmlAttribute attr1 = xmlDoc.CreateAttribute("name");
            attr1.Value = newName;
            tmpNode.Attributes.Append(attr1);

            // 이름 바뀐 항목이 Memo인 경우
            if (targetType == DEFINE.NODE_MEMONAME)
            {
                RectangleConverter rc = new RectangleConverter();

                XmlAttribute attr2 = xmlDoc.CreateAttribute("text");
                attr2.Value = item.TEXT;
                tmpNode.Attributes.Append(attr2);
                XmlAttribute attr3 = xmlDoc.CreateAttribute("rect");
                attr3.Value = rc.ConvertToString(item.RECT);
                tmpNode.Attributes.Append(attr3);



                // 현재 이름 변경하는 메모 폼이 열려있을 경우 TITLE 변경
                for (int i = memoForms.Count - 1; i >= 0; i--)
                {
                    if (memoForms[i].g_item.PATH == item.PATH && memoForms[i].g_item.TITLE == item.TITLE)
                    {
                        memoForms[i].g_item.TITLE = newName;
                        memoForms[i].refresh_memoInfo();
                    }
                }
            }
            else if (targetType == DEFINE.NODE_GROUPNAME) //이름 바뀐 항목이 그룹인 경우
            {
                tmpNode.InnerXml = targetNode.InnerXml;
                string groupPath_old = item.PATH + item.TITLE;

                string[] paths = groupPath_old.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                // 현재 이름 변경하는 그룹에 하위항목에 해당하는 메모 폼이 열려있을 경우
                for (int i = memoForms.Count - 1; i >= 0; i--)
                {
                    string[] paths2 = memoForms[i].g_item.PATH.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    bool isChild = true;
                    for (int j = 0; j < paths2.Length; j++)
                    {
                        if (paths.Length < paths2.Length ? true : paths[j] != paths2[j])
                        {
                            isChild = false;
                            break;
                        }
                    }
                    if (isChild)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append('/');
                        for (int j = 0; j < paths2.Length; j++)
                        {
                            if (j == paths.Length - 1)
                                sb.Append(newName);
                            else
                                sb.Append(paths2[j]);
                            sb.Append('/');
                        }
                        memoForms[i].g_item.PATH = sb.ToString();
                        memoForms[i].refresh_memoInfo();
                    }
                }
            }

            targetNode.ParentNode.ReplaceChild(tmpNode, targetNode);

            Common.XmlControl.getInstance().xmlSave(xmlDoc, DEFINE.MEMO_DATA_PATH);

            if (mainFolder != null)
                mainFolder.listViewReload();
        }

        /// <summary>
        /// 설정 폼에서 이벤트 발생
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool settingForm_occurred_event(DEFINE.EVENTTYPE type, object obj)
        {
            if (type == DEFINE.EVENTTYPE.EVENTTYPE_SAVEMEMODATAPATH) // 메모 Path 지정
            {
                string str_memo_path = (String)obj;
                if (!System.IO.File.Exists(str_memo_path))
                {
                    MessageBox.Show("메모데이터가 해당 경로에 존재하지 않습니다.");
                    return false;
                }

                XmlDocument memoDoc = Common.XmlControl.getInstance().xmlLoad(str_memo_path);
                XmlNode memoNode = memoDoc.SelectSingleNode("//MEMODATA");
                if (memoNode == null)
                {
                    MessageBox.Show("해당 경로 파일은 메모데이터가 아닙니다.");
                    return false;
                }

                // 메모 폼이 현재 열려있으면 폼을 닫음
                for (int i = memoForms.Count - 1; i >= 0; i--)
                {
                    Item tmpItem = memoForms[i].g_item;
                    memoForms[i].closeForce = true;
                    memoForms[i].Close();
                }

                // 폴더 폼 닫음
                if (mainFolder != null) {
                    mainFolder.Close();
                }

                XmlNode settingNode = configDoc.SelectSingleNode("//SETTING");
                XmlNode memoPathNode = settingNode.SelectSingleNode(String.Format("./{0}", DEFINE.CONFIG_SETTING_MEMODATAPATH));

                memoPathNode.InnerText = str_memo_path;
                DEFINE.MEMO_DATA_PATH = str_memo_path;

                if (Common.XmlControl.getInstance().xmlSave(configDoc, System.Windows.Forms.Application.StartupPath + "\\" + DEFINE.CONFIG_FILENAME))
                    configDoc = Common.XmlControl.getInstance().xmlLoad(System.Windows.Forms.Application.StartupPath + "\\" + DEFINE.CONFIG_FILENAME);

                // 최근메모에 존재하는지 검사 후 저장
                settingForm_occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_SAVE_RECENT_MEMODATAPATH, obj);

                showFormFolder();
            }
            else if ( type == DEFINE.EVENTTYPE.EVENTTYPE_SAVE_RECENT_MEMODATAPATH) // 최근 메모 Path 저장
            {
                // 최근 Open한 메모리스트에 존재하는지 검사 후 없으면 추가
                XmlNode settingNode = configDoc.SelectSingleNode("//SETTING");
                bool is_exist = false;
                XmlNodeList recentMemoPathNodes = settingNode.SelectNodes(String.Format("./{0}", DEFINE.CONFIG_SETTING_RECENT_MEMODATAPATH));
                for (int i = 0; i < recentMemoPathNodes.Count; i++)
                {
                    XmlNode item = recentMemoPathNodes.Item(i);
                    if (item.InnerText == DEFINE.MEMO_DATA_PATH)
                    {
                        is_exist = true;
                        break;
                    }
                }
                if (!is_exist)
                {
                    XmlNode tmpNode = configDoc.CreateNode(XmlNodeType.Element, DEFINE.CONFIG_SETTING_RECENT_MEMODATAPATH, "");
                    tmpNode.InnerText = (String)obj;
                    settingNode.AppendChild(tmpNode);
                    if (Common.XmlControl.getInstance().xmlSave(configDoc, System.Windows.Forms.Application.StartupPath + "\\" + DEFINE.CONFIG_FILENAME))
                        configDoc = Common.XmlControl.getInstance().xmlLoad(System.Windows.Forms.Application.StartupPath + "\\" + DEFINE.CONFIG_FILENAME);
                }

                // 데이터 및 Setting Form Relaod
                reload_recent_memo_data();
                if( mainSetting != null)
                {
                    mainSetting.reloadForm();
                }
            }
            else if (type == DEFINE.EVENTTYPE.EVENTTYPE_DELETE_RECENT_MEMODATAPATH) // 최근 메모 Path 삭제
            {
                int idx = (int)obj; // index 순서
                XmlNode settingNode = configDoc.SelectSingleNode("//SETTING");
                XmlNodeList RecentMemoPathNodes = settingNode.SelectNodes(String.Format("./{0}", DEFINE.CONFIG_SETTING_RECENT_MEMODATAPATH));
                if( idx < RecentMemoPathNodes.Count )
                {
                    // 노드 삭제
                    XmlNode tmpNode = RecentMemoPathNodes.Item(idx);
                    tmpNode.ParentNode.RemoveChild(tmpNode);

                    // 저장
                    if (Common.XmlControl.getInstance().xmlSave(configDoc, System.Windows.Forms.Application.StartupPath + "\\" + DEFINE.CONFIG_FILENAME))
                        configDoc = Common.XmlControl.getInstance().xmlLoad(System.Windows.Forms.Application.StartupPath + "\\" + DEFINE.CONFIG_FILENAME);
                }
                // 데이터 및 Setting Form Relaod
                reload_recent_memo_data();
                if (mainSetting != null)
                {
                    mainSetting.reloadForm();
                }
            }

            return false;
        }

        /// <summary>
        /// 최근 Open한 메모데이터 경로를 데이터 DEFINE.RECENT_MEMO_DATA_PATH에 Setting
        /// </summary>
        private void reload_recent_memo_data()
        {
            DEFINE.RECENT_MEMO_DATA_PATH.Clear();

            XmlNode settingNode = configDoc.SelectSingleNode("//SETTING");
            XmlNodeList RecentMemoPathNodes = settingNode.SelectNodes(String.Format("./{0}", DEFINE.CONFIG_SETTING_RECENT_MEMODATAPATH));
            
            if (RecentMemoPathNodes.Count > 0)
            {
                for (int i = 0; i < RecentMemoPathNodes.Count; i++)
                {
                    string str_full_path = RecentMemoPathNodes.Item(i).InnerText;
                    string fileName = System.IO.Path.GetFileName(str_full_path);
                    string filePath = System.IO.Path.GetDirectoryName(str_full_path);
                    bool tmp_is_exist = System.IO.File.Exists(str_full_path);
                    DEFINE.RECENT_MEMO_DATA data = new DEFINE.RECENT_MEMO_DATA();
                    data.str_full_path = str_full_path;
                    data.str_name = fileName;
                    data.str_path = filePath;
                    data.is_exist_local = tmp_is_exist;
                    DEFINE.RECENT_MEMO_DATA_PATH.Add(data);
                }
            }
        }


        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #region AboutForm
        /// <summary>
        /// About Form 표시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowAboutThis(object sender, EventArgs e)
        {
            //MessageBox.Show(DEFINE.MESSAGE_ABOUT);
            if( m_form_about == null)
            {
                m_form_about = new FolderMemoAboutBox();
                m_form_about.FormClosed += new FormClosedEventHandler(AboutFormClosed);
                m_form_about.Text = DEFINE.TRAY_NAME + " About";
                m_form_about.Icon = Properties.Resources.icon_stick_note_32x;
                m_form_about.ShowInTaskbar = false;
                m_form_about.Show();
            }
            else
            {
                m_form_about.Focus();
            }
        }
        void AboutFormClosed(object sender, FormClosedEventArgs e)
        {
            m_form_about = null;
        }
        #endregion

        /// <summary>
        /// App Update 프로그램 실행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void runUpdate(object sender, EventArgs e)
        {
            string fileName = "FolderMemoUpdate.exe";
            if(! System.IO.File.Exists(fileName) )
            {
                MessageBox.Show(this, "업데이트 파일인 FolderMemoUpdate.exe이 없습니다", "파일이 존재하지 않음");
                return;
            }
            try
            {
                System.Diagnostics.Process.Start("FolderMemoUpdate.exe");
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), ex.Message);
            }
        }

        #region ※ FolderMemo 자동실행
        /// <summary>
        /// 시작시 FolderMemo 자동 실행 버튼 터치 Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setup_startup_FolderMemo(object sender, EventArgs e)
        {
            setStartup_FolderMemo(!getStartUpYN_FolderMemo());
            MenuItem item = (MenuItem)sender;
            item.Checked = !item.Checked;
            // FolderMemoStartup 키면 자동업데이트 해제 (중복실행되므로)
            if (item.Checked)
            {
                setStartup_FolderMemoAutoUpdate(false);
                var contextMenu = item.GetContextMenu();
                var menus = contextMenu.MenuItems;
                foreach (MenuItem tmpMenu in menus)
                {
                    if (tmpMenu.Tag != null && tmpMenu.Tag.Equals(m_appName_folderMemoAutoUpdate))
                    {
                        tmpMenu.Checked = false;
                        break;
                    }
                }
            }
        }

        private bool getStartUpYN_FolderMemo()
        {
            return this.getStartupYN(m_appName_folderMemo);
        }
        private void setStartup_FolderMemo(bool enable)
        {
            this.setStartup(m_appName_folderMemo, enable);
        }
        #endregion

        #region ※ FolderMemo Update 자동실행
        /// <summary>
        /// 시작시 FolderMemo 자동 실행 버튼 터치 Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setup_startup_FolderMemoAutoUpdate(object sender, EventArgs e)
        {
            setStartup_FolderMemoAutoUpdate(!getStartUpYN_FolderMemoAutoUpdate());
            MenuItem item = (MenuItem)sender;
            item.Checked = !item.Checked;
            // 자동업데이트 키면 FolderMemoStartup 해제 (중복실행되므로)
            if (item.Checked){
                setStartup_FolderMemo(false);
                var contextMenu = item.GetContextMenu();
                var menus = contextMenu.MenuItems;
                foreach (MenuItem tmpMenu in menus)
                {
                    if( tmpMenu.Tag != null && tmpMenu.Tag.Equals(m_appName_folderMemo) )
                    {
                        tmpMenu.Checked = false;
                        break;
                    }
                }
            }
        }

        private bool getStartUpYN_FolderMemoAutoUpdate()
        {
            return this.getStartupYN(m_appName_folderMemoAutoUpdate);
        }
        private void setStartup_FolderMemoAutoUpdate(bool enable)
        {
            this.setStartup(m_appName_folderMemoAutoUpdate, enable);
        }
        #endregion

        /// <summary>
        /// 설정화면 열기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ShowSettingForm(object sender, EventArgs e)
        {
            if (mainSetting == null)
            {
                mainSetting = new Form_Setting();
                mainSetting.FormClosed += delegate(Object tmp_sender, FormClosedEventArgs tmp_e)
                {
                    mainSetting.Dispose();
                    mainSetting = null;
                };
                mainSetting.occurred_event += settingForm_occurred_event;

                mainSetting.Text = DEFINE.TRAY_NAME + " SETTING";
                mainSetting.Icon = Properties.Resources.icon_stick_note_32x;
                mainSetting.ShowInTaskbar = false;
                mainSetting.Show(this);
            }
        }



        /// <summary>
        /// 시작프로그램 등록여부
        /// </summary>
        /// <returns></returns>
        private bool getStartupYN(string appName)
        {
            return getShortCut(appName) != null;
        }

        /// <summary>
        /// 해당 appName에 맞는 바로가기 link인지 여부 확인
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        private FileInfo getShortCut(string appName)
        {
            DirectoryInfo startup_dir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Startup));

            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder shortcut_folder = shell.NameSpace(startup_dir.FullName);
            foreach (FileInfo file in startup_dir.GetFiles())
            {   
                Shell32.FolderItem folder_item = shortcut_folder.Items().Item(file.Name);
                if (folder_item == null)
                {
                    Console.WriteLine("Cannot find shortcut file '" + file.Name + "'");
                }
                else if (!folder_item.IsLink)
                {
                    Console.WriteLine("File '" + file.Name + "' isn't a shortcut.");
                }
                else
                {
                    try
                    {
                        Shell32.ShellLinkObject lnk =
                            (Shell32.ShellLinkObject)folder_item.GetLink;
                        string original_path = lnk.Path;
                        if (original_path.Length > 0)
                        {
                            FileInfo original_file = new FileInfo(original_path);
                            if (original_file.Name == appName) // 해당 파일이 맞는지 확인
                            {
                                return file;
                            }
                        }
                    }catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            return null;
        }

        ///// <summary>
        ///// 시작프로그램 등록여부
        ///// </summary>
        ///// <returns></returns>
        //private bool getStartupYN(string appName, string key)
        //{
        //    Microsoft.Win32.RegistryKey startupKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key);

        //    if (startupKey.GetValue(appName) == null)
        //        return false;
        //    else
        //    {
        //        startupKey.Close();
        //        return true;
        //    }
        //}

        /// <summary>
        /// startup 디렉토리 밑에 shortcut 추가하기(제대로 동작됨을 확인)
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="enable"></param>
        private void setStartup(string appName, bool enable)
        {
            if (enable)
            {
                // 시작프로그램에 link가 없을경우에만 추가
                if (!this.getStartupYN(appName))
                {
                    string fileName = Path.GetFileName(appName);
                    string ext = Path.GetExtension(appName);
                    fileName = fileName.Substring(0, fileName.Length - ext.Length);
                    
                    string shortcutAddress = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\" + fileName + ".lnk";
                    string targetPath = Path.GetDirectoryName(Application.ExecutablePath.ToString());
                    string targetFullPath = Path.GetDirectoryName(Application.ExecutablePath.ToString()) + @"\" + appName;
                    
                    WshShell wsh = new WshShell();
                    IWshRuntimeLibrary.IWshShortcut shortcut = wsh.CreateShortcut(shortcutAddress) as IWshRuntimeLibrary.IWshShortcut;
                    shortcut.Arguments = "";
                    shortcut.TargetPath = targetFullPath;
                    // not sure about what this is for
                    shortcut.WindowStyle = 1;
                    shortcut.Description = "my shortcut description";
                    shortcut.WorkingDirectory = targetPath;
                    shortcut.IconLocation = targetFullPath;
                    shortcut.Save();
                }
            }
            else
            {
                FileInfo info = this.getShortCut(appName);
                if (info != null) {
                    info.Delete();
                }
            }
        }

        // 레지스트리에 등록하는 방식 - startup시 단일 App만 실행됨
        // startup시 Update프로그램 실행 이후 Process.Start()로 앱 실행하면 제대로 동작 안함
        //private void setStartup(string appName, string key, bool enable)
        //{
        //    //Microsoft.Win32.RegistryKey startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(runKey);
        //    Microsoft.Win32.RegistryKey startupKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key);

        //    if (enable)
        //    {
        //        // 레지스트리에 appName값이 없을경우에만 추가
        //        if (startupKey.GetValue(appName) == null)
        //        {
        //            startupKey.Close();
        //            //startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(runKey, true);
        //            startupKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key, true);
        //            string str_path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.ExecutablePath.ToString()), appName + ".exe");
        //            startupKey.SetValue(appName, str_path);
        //            startupKey.Close();
        //        }
        //    }
        //    else
        //    {
        //        //startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(runKey, true);
        //        startupKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key, true);
        //        startupKey.DeleteValue(appName, false);
        //        startupKey.Close();
        //    }
        //}

        ///// <summary>
        ///// 등록일 : 20150504
        ///// 설명 : schtasks 등록 안됨...
        /////        cmd로 등록해서 실행하더라도 FolderMemo.exe 프로세스에만 뜨고 폼이 안뜸..
        /////        시스템트레이도안뜸...
        ///// </summary>
        //private void schtasksReg()
        //{
        //    System.Diagnostics.ProcessStartInfo proInfo = new System.Diagnostics.ProcessStartInfo();
        //    System.Diagnostics.Process pro = new System.Diagnostics.Process();

        //    proInfo.FileName = "cmd";
        //    proInfo.CreateNoWindow = true;
        //    proInfo.UseShellExecute = false;
        //    proInfo.RedirectStandardOutput = true;
        //    proInfo.RedirectStandardInput = true;
        //    proInfo.RedirectStandardError = true;
        //    proInfo.Verb = "runas";

        //    pro.StartInfo = proInfo;
        //    pro.Start();

        //    string command = "schtasks /create /tn FolderMemoStart /tr \""+Application.ExecutablePath.ToString()+"\" /sc onlogon /ru \"system\" /rl HIGHEST /f";
        //    //command = "schtasks /create /tn FolderMemoStart";

        //    pro.StandardInput.Write(command + System.Environment.NewLine);
        //    pro.StandardInput.Close();

        //    string resultValue = pro.StandardOutput.ReadToEnd();
        //    pro.WaitForExit();
        //    pro.Close();

        //    //Console.WriteLine(resultValue);
        //    System.Diagnostics.Debug.WriteLine(resultValue);
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release the icon resource.
                if(trayIcon != null)
                    trayIcon.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SystemTray
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "SystemTray";
            this.Text = "FolderMemo";
            this.ResumeLayout(false);
        }


        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_RESTORE = 0xF120;

        private const int SW_FORCEMINIMIZE = 11;
        private const int SW_MINIMIZE = 6; // 기본 Minimize

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hwnd, int command);
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 1;

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_SYSCOMMAND:
                    int command = m.WParam.ToInt32() & 0xfff0;
                    
                    if(command == SC_MINIMIZE || command == SC_RESTORE)
                    {
                        if (mainFolder == null)
                        {
                            showFormFolder();
                        }
                        else
                            mainFolder.Close();
                        return;
                    }
                    break;
            }
            base.WndProc(ref m);
        }
    }
}
