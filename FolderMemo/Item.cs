using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace FolderMemo
{
    public partial class Item
    {
        public Item(string title, DEFINE.FILETYPE type)
        {
            this.TITLE = title;
            this.TYPE = type;
        }

        public Item(string title, string txt, DEFINE.FILETYPE type)
        {
            this.TITLE = title;
            this.TEXT = txt;
            this.TYPE = type;
        }

        public DEFINE.FILETYPE TYPE{ get; set; }
        public string TITLE { get; set; }
        public string TEXT  { get; set; }
        public string PATH { get; set; }
        public object TAG { get; set; }

        public Rectangle RECT { get; set;}
        public bool EMPTY = true;

        public void setItemType(DEFINE.FILETYPE type)
        {
            this.TYPE = type;
        }
        public void setItemText(string txt)
        {
            this.TEXT = txt;
        }
        public void setItemTitle(string title)
        {
            this.TITLE = title;
        }
        public void setItemPath(string path)
        {
            this.PATH = path;
        }

        public bool isEmpty()
        {
            return EMPTY;
        }

    }
}
