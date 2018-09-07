using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FolderMemo
{
    public partial class Form_Memo : CustomControlLibrary.CustomForm
    {
        public delegate bool CommonDelegate(DEFINE.EVENTTYPE type, object obj);
        public event CommonDelegate occurred_event;

        private string m_filePath;
        private string m_fileText;
        private string m_fileTitle;
        public Item g_item;
        
        bool isDefaultText = true;
        public bool closeForce = false; // 폼 강제종료 필요할 때 true

        public Form_Memo()
        {
            InitializeComponent();
            this.paintTextBox1.TextBoxIsDefaultText += paintTextBox1_TextBoxIsDefaultText;
        }

        bool paintTextBox1_TextBoxIsDefaultText(object obj)
        {
            if ((bool)obj == true)
            {
                this.Text = m_fileTitle;
            }
            else
            {
                this.Text = m_fileTitle+" *";
            }
            isDefaultText = (bool)obj;
            //throw new NotImplementedException();
            return true;
        }


        private void Form_Memo_Load(object sender, EventArgs e)
        {
            this.Text = g_item.TITLE;
            this.m_fileTitle = g_item.TITLE;
            this.paintTextBox1.TextBox.Text = g_item.TEXT;
            this.m_fileText = g_item.TEXT;
            this.m_filePath = g_item.PATH;
            this.lbl_path.Text = this.m_filePath;

            this.paintTextBox1.TextBox.isDefaultText = true;
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
                    //this.Location = tmpRect.Location;
                    //this.Size = tmpRect.Size;
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

                    //this.Location = tmpRect.Location;
                    //this.Size = tmpRect.Size;
                    this.DesktopBounds = tmpRect;
                    break;
                }
                else
                {
                    //mainFolder.Size = tmpRect.Size;
                    Rectangle primaryScreen_bounds = Screen.PrimaryScreen.Bounds;
                    Size mainFolder_size = DEFINE.DEFAULT_MEMO_SIZE;
                    this.DesktopBounds = new Rectangle((primaryScreen_bounds.Width - mainFolder_size.Width) - 3, 3, mainFolder_size.Width, mainFolder_size.Height);
                }
            }

            this.textBoxFocus();
            this.paintTextBox1.TextBoxProcessCmdKeyCheck += paintTextBox1_TextBoxProcessCmdKeyCheck;
            this.paintTextBox1.TextBoxIsGotFocus += paintTextBox1_TextBoxIsGotFocus;
            this.paintTextBox1.TextBoxIsLostFocus += paintTextBox1_TextBoxIsLostFocus;
            this.paintTextBox1.setScrollBarHidden(true);
        }

        /// <summary>
        /// g_item 기준으로 표시 데이터 reload
        /// </summary>
        public void refresh_memoInfo()
        {
            this.Text = g_item.TITLE;
            this.m_fileTitle = g_item.TITLE;
            // 텍스트 변경하면 paintTextBox 엑박이 뜸..... 141126
            //this.paintTextBox1.TextBox.Text = g_item.TEXT;
            //this.m_fileText = g_item.TEXT;
            this.m_filePath = g_item.PATH;
            this.lbl_path.Text = this.m_filePath;
        }

        bool paintTextBox1_TextBoxIsLostFocus(object obj)
        {
            this.paintTextBox1.setScrollBarHidden(true);
            return true;
            //throw new NotImplementedException();
        }

        bool paintTextBox1_TextBoxIsGotFocus(object obj)
        {
            this.paintTextBox1.setScrollBarHidden(false);
            return true;
            //throw new NotImplementedException();
        }

        bool paintTextBox1_TextBoxProcessCmdKeyCheck(object obj)
        {
            Keys keyData = (Keys)obj;

            if (keyData == (Keys.Control | Keys.S))
            {
                delegate_saveEvent();
                return true;
            }
            if (keyData == (Keys.Escape))
            {
                this.Close();
            }
            return false;
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
                if(saveYN == "Y")
                    g_item.TEXT = this.paintTextBox1.TextBox.Text;
                object tmp = new object[] { this, g_item , saveYN};
                occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_CLOSEMEMO, tmp);
            }
        }

        private void delegate_saveEvent()
        {
            if (occurred_event != null)
            {
                g_item.RECT = new Rectangle(this.Location, this.Size);
                g_item.TEXT = this.paintTextBox1.TextBox.Text;
                if (occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_SAVEMEMO, g_item))
                {
                    this.Text = m_fileTitle;
                    this.paintTextBox1.TextBox.isDefaultText = true;
                    isDefaultText = true;
                }
            }
        }

        public void textBoxFocus()
        {
            this.paintTextBox1.TextBox.Focus();
            //this.Activate();
            //this.paintTextBox1.Focus();
            //this.paintTextBox1.TextBox.Focus();
        }

        private void Form_Memo_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isDefaultText && !closeForce)
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
                delegate_closeEvent("Y");
        }


        public Rectangle getDefaultMemoRectangle()
        {
            Rectangle primaryScreen_bounds = Screen.PrimaryScreen.Bounds;
            Size mainFolder_size = DEFINE.DEFAULT_MEMO_SIZE;
            
            return new Rectangle((primaryScreen_bounds.Width - mainFolder_size.Width) - 3, 3, mainFolder_size.Width, mainFolder_size.Height);
        }
    }
}
