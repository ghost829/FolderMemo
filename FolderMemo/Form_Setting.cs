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

        private void btn_apply_Click(object sender, EventArgs e)
        {
            string memoDataPath = txt_memoDataPath.Text;
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

        private void Form_Setting_Load(object sender, EventArgs e)
        {
            txt_memoDataPath.Text = DEFINE.MEMO_DATA_PATH;
        }


    }
}
