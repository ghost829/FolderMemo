using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintTextBoxClassLib
{
    /// <summary>
    ///  자동완성 기능 사용시 ListBox를 띄울 부모 Control
    /// </summary>
    class PopupWindow : System.Windows.Forms.ToolStripDropDown
    {
        private System.Windows.Forms.Control _content;
        private System.Windows.Forms.ToolStripControlHost _host;

        /// <summary>
        /// 특정 컨트롤을 팝업 형식으로 화면에 띄울 수 있게 타겟을 지정한다.
        /// </summary>
        /// <param name="content"></param>
        public PopupWindow(System.Windows.Forms.Control content)
        {
            //Basic setup...
            this.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);

            //this.Renderer = new System.Windows.Forms.ToolStripProfessionalRenderer();

            this.AutoSize = false;
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this._content = content;
            this._host = new System.Windows.Forms.ToolStripControlHost(content);

            //Positioning and Sizing
            //this.MinimumSize = content.MinimumSize;
            this.MinimumSize = new System.Drawing.Size(0, 0);
            //this.MaximumSize = content.Size;
            this.MaximumSize = new System.Drawing.Size(0, 0);

            this.Size = new System.Drawing.Size(content.Size.Width, content.Size.Height);
            content.Location = System.Drawing.Point.Empty;


            //Add the host to the list
            this.Items.Add(this._host);
            this.SetTopLevel(true);
        }
    }
}
