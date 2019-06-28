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
    public partial class Form_Setting : Form
    {
        public delegate bool CommonDelegate(DEFINE.EVENTTYPE type, object obj);
        public event CommonDelegate occurred_event;

        public Form_Setting()
        {
            InitializeComponent();
        }

        private void Form_Setting_Load(object sender, EventArgs e)
        {
            reloadForm();
        }

        /// <summary>
        /// 폼 Layout 세팅
        /// </summary>
        public void reloadForm()
        {
            setMemoDataPathText(DEFINE.MEMO_DATA_PATH);   

            listView1.Items.Clear();
            for (int i = 0; i < DEFINE.RECENT_MEMO_DATA_PATH.Count; i++)
            {
                DEFINE.RECENT_MEMO_DATA data = DEFINE.RECENT_MEMO_DATA_PATH[i];
                ListViewItem item = new ListViewItem(data.str_name);
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, data.str_full_path == data.str_path || data.str_path.Length == 0 ? "프로그램경로" : data.str_path));
                // Subitem 단독으로 ForeColor 지정 안됨
                //ListViewItem.ListViewSubItem subitem_exist = new ListViewItem.ListViewSubItem(item, data.is_exist_local ? "O" : "X");
                //subitem_exist.ForeColor = data.is_exist_local ? Color.Green : Color.Red;
                //item.SubItems.Add(subitem_exist);
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, data.is_exist_local ? "O" : "X"));
                item.ForeColor = data.is_exist_local ? Color.LightSeaGreen : Color.Red;
                listView1.Items.Add(item);
            }
            for(int i=0; i < listView1.Columns.Count; i++)
            {
                listView1.Columns[i].Width = -2;
            }
        }



        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if( listView1.SelectedItems.Count == 1 )
            {
                int itemIndex = listView1.SelectedItems[0].Index;
                DEFINE.RECENT_MEMO_DATA data = DEFINE.RECENT_MEMO_DATA_PATH[itemIndex];
                setMemoDataPathText(data.str_full_path);
            }
        }


        private void btn_apply_Click(object sender, EventArgs e)
        {
            string memoDataPath = txt_memoDataPath.Text;
            string filePath = System.IO.Path.GetDirectoryName(memoDataPath);
            if(filePath == null || filePath.Length == 0 || filePath == memoDataPath)
            {
                memoDataPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.ExecutablePath.ToString()), memoDataPath);
            }
            if (!System.IO.File.Exists(memoDataPath))
            {
                MessageBox.Show("메모데이터가 해당 경로에 존재하지 않습니다.");
            }
            else if (occurred_event != null)
            {
                occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_SAVEMEMODATAPATH, memoDataPath);
            }
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (sender == openFileDialog1)
            {
                setMemoDataPathText(openFileDialog1.FileName);
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if( e.Button == MouseButtons.Right)
            {
                Point p = e.Location;
                
                ctxt_listview1.Show(listView1.PointToScreen(e.Location));
            }
        }

        #region ## Form Context Menu
        private void ctxt_listview1_delete_Click(object sender, EventArgs e)
        {
            if( listView1.SelectedItems.Count == 1 )
            {
                int itemIndex = listView1.SelectedItems[0].Index;
                occurred_event(DEFINE.EVENTTYPE.EVENTTYPE_DELETE_RECENT_MEMODATAPATH, itemIndex);
            }
        }
        #endregion

        private void btn_search_data_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog(this);
        }

        /// <summary>
        /// 데이터 파일 경로 text 설정
        /// </summary>
        /// <param name="str_path"></param>
        private void setMemoDataPathText(string str_path)
        {
            // 메모파일 경로가 현재 실행된 프로그램의 경로와 같다면 경로 단축
            string str_memo_data_path = str_path;
            if (System.IO.Path.GetDirectoryName(str_memo_data_path) == System.IO.Path.GetDirectoryName(Application.ExecutablePath.ToString()))
            {
                str_memo_data_path = System.IO.Path.GetFileName(str_memo_data_path);
            }
            txt_memoDataPath.Text = str_memo_data_path;
        }
    }
}
