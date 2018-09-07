namespace PaintTextBoxClassLib
{
    partial class CustomListBox
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.VScrollBar = new System.Windows.Forms.VScrollBar();
            this.intellisenseToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // VScrollBar
            // 
            this.VScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.VScrollBar.LargeChange = 1;
            this.VScrollBar.Location = new System.Drawing.Point(256, 0);
            this.VScrollBar.Name = "VScrollBar";
            this.VScrollBar.Size = new System.Drawing.Size(16, 115);
            this.VScrollBar.TabIndex = 99;
            this.VScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.VScrollBar_Scroll);
            // 
            // intellisenseToolTip
            // 
            this.intellisenseToolTip.AutomaticDelay = 100;
            this.intellisenseToolTip.AutoPopDelay = 0;
            this.intellisenseToolTip.InitialDelay = 100;
            this.intellisenseToolTip.ReshowDelay = 20;
            this.intellisenseToolTip.ShowAlways = true;
            // 
            // CustomListBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.VScrollBar);
            this.Name = "CustomListBox";
            this.Size = new System.Drawing.Size(272, 115);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.VScrollBar VScrollBar;
        private System.Windows.Forms.ToolTip intellisenseToolTip;
    }
}
