using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace FolderMemo
{
    public partial class Form_Memo_RIchText : CustomControlLibrary.CustomForm
    {
        private const int WM_SETREDRAW = 0x000B;
        private const int WM_USER = 0x400;
        private const int EM_GETEVENTMASK = (WM_USER + 59);
        private const int EM_SETEVENTMASK = (WM_USER + 69);
        [DllImport("user32", CharSet = CharSet.Auto)]
        private extern static IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);
        IntPtr eventMask = IntPtr.Zero;

        public delegate bool CommonDelegate(DEFINE.EVENTTYPE type, object obj);
        public event CommonDelegate occurred_event;

        private string m_filePath;
        private string m_fileText;
        private string m_fileTitle;
        public Item g_item;

        Font defaultFont;
        
        //bool isDefaultText = true;
        public bool closeForce = false; // 폼 강제종료 필요할 때 true

        bool isTextDefaultValue;
        private string m_undoText = "";

        public Form_Memo_RIchText()
        {
            InitializeComponent();
        }

        void richtextBox1_checktext()
        {
            if (this.richTextBox1.CanUndo)
            {
                this.Text = m_fileTitle + " *";
                isTextDefaultValue = false;
            }
            else
            {
                this.Text = m_fileTitle;
                isTextDefaultValue = true;
            }
        }


        private void Form_Memo_Load(object sender, EventArgs e)
        {
            this.Text = g_item.TITLE;
            this.m_fileTitle = g_item.TITLE;
            this.richTextBox1.Text = g_item.TEXT;
            this.m_fileText = g_item.TEXT;
            this.m_filePath = g_item.PATH;
            this.lbl_path.Text = this.m_filePath;
            this.txt_search.Text = "";
            this.txt_search.Visible = false;

            // 기본 사이즈 설정
            richTextBox1.SetBounds(12, 42, 255, 213);
            view_search(false);

            this.richTextBox1.TextChanged += richTextBox1_TextChanged;
            this.richTextBox1.ClearUndo();

            m_undoText = g_item.TEXT;
            isTextDefaultValue = true;
            if(this.g_item.RECT == null || this.g_item.RECT == Rectangle.Empty)
            {
                this.g_item.RECT = this.getDefaultMemoRectangle();
            }

            Screen[] sc = Screen.AllScreens;
            Rectangle tmpRect = this.g_item.RECT;
            for (int i = 0; i < sc.Length; i++)
            {
                if (sc[i].WorkingArea.Contains(tmpRect)) //맞는 화면 존재 시
                {
                    this.DesktopBounds = tmpRect;
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

                    this.DesktopBounds = tmpRect;
                    break;
                }
                else
                {
                    Rectangle primaryScreen_bounds = Screen.PrimaryScreen.Bounds;
                    Size mainFolder_size = DEFINE.DEFAULT_MEMO_SIZE;
                    this.DesktopBounds = new Rectangle((primaryScreen_bounds.Width - mainFolder_size.Width) - 3, 3, mainFolder_size.Width, mainFolder_size.Height);
                }
            }

            this.textBoxFocus();

            defaultFont = this.richTextBox1.Font;
        }

        /// <summary>
        /// g_item 기준으로 표시 데이터 reload
        /// </summary>
        public void refresh_memoInfo()
        {
            this.Text = g_item.TITLE;
            this.m_fileTitle = g_item.TITLE;
            this.m_filePath = g_item.PATH;
            this.lbl_path.Text = this.m_filePath;
        }
        
        /// <summary>
        /// 닫기 delegate 수행
        /// </summary>
        /// <param name="saveYN">저장 여부</param>
        private void delegate_closeEvent(string saveYN)
        {
            if (occurred_event != null)
            {
                g_item.RECT = new Rectangle(this.Location, this.Size);
                if (saveYN == "Y")
                    g_item.TEXT = this.richTextBox1.Text;
                object tmp = new object[] { this, g_item , saveYN};
                occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_CLOSEMEMO, tmp);
            }
        }

        private void delegate_saveEvent()
        {
            if (occurred_event != null)
            {
                g_item.RECT = new Rectangle(this.Location, this.Size);
                g_item.TEXT = this.richTextBox1.Text;
                if (occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_SAVEMEMO, g_item))
                {
                    this.Text = m_fileTitle;

                    this.richTextBox1.ClearUndo();
                    richtextBox1_checktext();
                }
            }
        }

        public void textBoxFocus()
        {
            this.richTextBox1.Focus();
        }

        private void Form_Memo_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isTextDefaultValue && !closeForce)
            {
                DialogResult dialogResult = MessageBox.Show("메모를 저장 후 종료하시겠습니까?", "", MessageBoxButtons.YesNoCancel);
                if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                {
                    delegate_closeEvent("Y");
                }
                else if (dialogResult == System.Windows.Forms.DialogResult.No)
                {
                    delegate_closeEvent("N");
                }
                else if (dialogResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
            else
                delegate_closeEvent("N");
        }


        public Rectangle getDefaultMemoRectangle()
        {
            Rectangle primaryScreen_bounds = Screen.PrimaryScreen.Bounds;
            Size mainFolder_size = DEFINE.DEFAULT_MEMO_SIZE;
            
            return new Rectangle((primaryScreen_bounds.Width - mainFolder_size.Width) - 3, 3, mainFolder_size.Width, mainFolder_size.Height);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            
            richtextBox1_checktext();
            if (defaultFont != null ? this.richTextBox1.SelectionFont != defaultFont : false)
            {
                this.richTextBox1.SelectionFont = defaultFont;
            }
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void richTextBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                this.richTextBox1.SelectedText = new string(' ', 4);
            }
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.S && e.Control)
            {
                delegate_saveEvent();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.V && e.Control)
            {
                ((RichTextBox)sender).Paste(DataFormats.GetFormat("Text"));
                e.Handled = true;
            }
            else if (e.KeyCode == (Keys.Escape))
            {
                if (txt_search.Visible)
                {
                    view_search(false);
                    e.Handled = true;
                }
                else
                {
                    e.Handled = true;
                    this.Close();
                }
            }
            else if (e.KeyCode == Keys.C && e.Control)
            {
                // RichText Format아닌 문자열 그대로 Copy
                Console.WriteLine(richTextBox1.SelectionStart + " "+ richTextBox1.SelectionLength);
                if (richTextBox1.SelectedText.Length > 0)
                {
                    string textforClipboard = richTextBox1.SelectedText.Replace("\n", Environment.NewLine);
                    Clipboard.SetText(textforClipboard);
                }
                else
                {
                    Clipboard.Clear();
                }

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F && e.Control)
            {
                if (txt_search.Visible)
                {
                    txt_search.Focus();
                }
                else
                {
                    this.view_search();
                }
                e.Handled = true;
            }
            else if(e.KeyCode == Keys.F3 )
            {
                if (e.Shift)
                {
                    this.richTextBox1_find_prev();
                }
                else
                {
                    this.richTextBox1_find_next();
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Z && e.Control)
            {
                if (g_item.TEXT == richTextBox1.Text && g_item.TEXT == m_undoText)
                {
                    richTextBox1.ClearUndo();
                    richtextBox1_checktext();
                }
                m_undoText = richTextBox1.Text;
            }
            else if ( !e.Control && e.Shift && e.KeyCode == Keys.End )
            {
                // 기존 shift+END는 끝에 NewLine까지 Select해서 조작 시 라인변경이 됨, 수정

                // 1810125 - Wordwrap 적용됬을때 선택영역이 맞지 않음
                // https://stackoverflow.com/questions/36801006/how-to-get-wrapped-lines-from-multiline-textbox
                // richTextBox1.Lines는 multiline일때 selectionStart와 매칭되지 않음, 라인 측정해서 계산

                bool continueProcess = true;
                int i = 1; //Zero Based So Start from 1
                int j = 0;
                List<string> lines = new List<string>();
                List<int> lineStartIdx = new List<int>();
                lineStartIdx.Add(0);
                while (continueProcess)
                {
                    var index = richTextBox1.GetFirstCharIndexFromLine(i);
                    if (index != -1)
                    {
                        lines.Add(richTextBox1.Text.Substring(j, index - j));
                        j = index;
                        i++;
                        lineStartIdx.Add(index);
                    }
                    else
                    {
                        lines.Add(richTextBox1.Text.Substring(j, richTextBox1.Text.Length - j));
                        continueProcess = false;
                    }
                }

                int selectionStart = richTextBox1.SelectionStart;
                int selectionEnd = selectionStart + richTextBox1.SelectionLength;
                int endLineNum = richTextBox1.GetLineFromCharIndex(selectionEnd);
                int endLineStart = richTextBox1.GetFirstCharIndexFromLine(endLineNum);

                int endLineSelectionLength = selectionEnd - endLineStart;
                int endLineLength = lines[endLineNum].Length;
                //System.Diagnostics.Debug.WriteLine("{0}", lines[endLineNum][lines[endLineNum].Length - 1]);
                char endLinelastChar = lines[endLineNum][lines[endLineNum].Length - 1];
                if (endLinelastChar == '\n')
                {
                    endLineLength = Math.Max(endLineLength - 1, 0);
                }

                int selectionLength = richTextBox1.SelectionLength + (endLineLength - endLineSelectionLength);
                richTextBox1.SelectionLength = selectionLength;

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F5) // 날짜 입력
            {
                richTextBox1.SelectedText = DateTime.Now.ToString("yyMMdd(ddd)");
            }
            else if (e.KeyCode == Keys.F6) // 시간 입력
            {
                richTextBox1.SelectedText = DateTime.Now.ToString("HH:mm");
            }
        }

        private void Form_Memo_RIchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F && e.Control)
            {
                this.view_search();
                e.Handled = true;
            }
        }
        


        /// <summary>
        ///  SearchView visible<->invisible
        /// </summary>
        private void view_search()
        {
            this.view_search(!txt_search.Visible);
        }

        private void view_search(bool visible)
        {
            // Search Layout 설정
            txt_search.Visible = visible;
            txt_search_button.Visible = visible;

            if (visible)
            {
                var diff = 69 - richTextBox1.Bounds.Y;
                richTextBox1.SetBounds(richTextBox1.Bounds.X, 69, richTextBox1.Bounds.Width, richTextBox1.Bounds.Height - diff);
                txt_search.Focus();
            }
            else
            {
                var diff = richTextBox1.Bounds.Y - 42;
                richTextBox1.SetBounds(richTextBox1.Bounds.X, 42, richTextBox1.Bounds.Width, richTextBox1.Bounds.Height + diff);
                richTextBox1.Focus();
            }
            richTextBox1.Invalidate();
        }

        private void txt_search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F && e.Control || e.KeyCode == Keys.Escape)
            {
                this.view_search();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                richTextBox1_find_next();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F3)
            {
                if (e.Shift)
                {
                    this.richTextBox1_find_prev();
                }
                else
                {
                    this.richTextBox1_find_next();
                }
                e.Handled = true;
            }
        }

        private void richTextBox1_find_next()
        {
            var text = txt_search.Text;
            if (text.Length > 0)
            {
                var find_idx = richTextBox1.Find(text, richTextBox1.SelectionStart + richTextBox1.SelectionLength, RichTextBoxFinds.None);

                if (find_idx > -1)
                {
                    //StopRepaint();
                    richTextBox1.Select(find_idx, text.Length);
                    richTextBox1.ScrollToCaret();
                    //StartRepaint();
                }
                richTextBox1.Focus();
                //richTextBox1.SelectionBackColor = richTextBox1.Focused ? richTextBox1.BackColor : Color.Blue;
            }
        }

        private void richTextBox1_find_prev()
        {
            StopRepaint();
            var text = txt_search.Text;
            if (text.Length > 0)
            {
                List<int> find_idxs = new List<int>();
                int last_length = richTextBox1.SelectionStart;
                int last_find_idx = 0;
                while (last_find_idx > -1 && last_find_idx < last_length)
                {
                    last_find_idx = richTextBox1.Find(text, last_find_idx, last_length, RichTextBoxFinds.None);
                    if (last_find_idx != -1)
                    {
                        find_idxs.Add(last_find_idx);
                        last_find_idx += text.Length;
                    }
                }
                if(find_idxs.Count > 0)
                {
                    richTextBox1.Select(find_idxs[find_idxs.Count - 1], text.Length);
                    richTextBox1.ScrollToCaret();
                }
                //richTextBox1.SelectionBackColor = richTextBox1.Focused ? richTextBox1.BackColor : Color.Blue;
            }
            richTextBox1.Focus();
            StartRepaint();
        }

        private void txt_search_button_Click(object sender, EventArgs e)
        {
            richTextBox1_find_next();
        }

        private void StopRepaint()
        {
            // Stop redrawing:
            SendMessage(richTextBox1.Handle, WM_SETREDRAW, 0, IntPtr.Zero);
            // Stop sending of events:
            eventMask = SendMessage(richTextBox1.Handle, EM_GETEVENTMASK, 0, IntPtr.Zero);
        }

        private void StartRepaint()
        {
            // turn on events
            SendMessage(richTextBox1.Handle, EM_SETEVENTMASK, 0, eventMask);
            // turn on redrawing
            SendMessage(richTextBox1.Handle, WM_SETREDRAW, 1, IntPtr.Zero);
            // this forces a repaint, which for some reason is necessary in some cases.
            richTextBox1.Invalidate();
        }
    }
}
