namespace PaintTextBoxClassLib
{
    partial class PaintTextBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PaintTextBox));
            this.hScroll = new System.Windows.Forms.HScrollBar();
            this.vScroll = new System.Windows.Forms.VScrollBar();
            this.linesTextBox1 = new PaintTextBoxClassLib.LinesTextBox();
            this.intellisenseToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // hScroll
            // 
            this.hScroll.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hScroll.LargeChange = 2;
            this.hScroll.Location = new System.Drawing.Point(0, 197);
            this.hScroll.Maximum = 10;
            this.hScroll.Name = "hScroll";
            this.hScroll.Size = new System.Drawing.Size(212, 17);
            this.hScroll.SmallChange = 2;
            this.hScroll.TabIndex = 1;
            this.hScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScroll_Scroll);
            // 
            // vScroll
            // 
            this.vScroll.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScroll.LargeChange = 1;
            this.vScroll.Location = new System.Drawing.Point(195, 0);
            this.vScroll.Maximum = 10;
            this.vScroll.Name = "vScroll";
            this.vScroll.Size = new System.Drawing.Size(17, 197);
            this.vScroll.TabIndex = 2;
            this.vScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScroll_Scroll);
            // 
            // linesTextBox1
            // 
            this.linesTextBox1.BackColor = System.Drawing.Color.White;
            this.linesTextBox1.BracketBackgroundBrush = System.Drawing.Color.DarkGray;
            this.linesTextBox1.comment_Color = System.Drawing.Color.Green;
            this.linesTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.linesTextBox1.DoubleQuotesColor = System.Drawing.Color.Red;
            this.linesTextBox1.indentValue = 30;
            this.linesTextBox1.isAllowTab = false;
            this.linesTextBox1.isDefaultText = true;
            this.linesTextBox1.keyword1 = ((System.Collections.Generic.List<string>)(resources.GetObject("linesTextBox1.keyword1")));
            this.linesTextBox1.keyword1_Color = System.Drawing.Color.Blue;
            this.linesTextBox1.keyword2 = ((System.Collections.Generic.List<string>)(resources.GetObject("linesTextBox1.keyword2")));
            this.linesTextBox1.keyword2_Color = System.Drawing.Color.Red;
            this.linesTextBox1.keyword3 = ((System.Collections.Generic.List<string>)(resources.GetObject("linesTextBox1.keyword3")));
            this.linesTextBox1.keyword3_Color = System.Drawing.Color.Orange;
            this.linesTextBox1.LineNumberColor = System.Drawing.Color.Silver;
            this.linesTextBox1.LineNumberVisible = true;
            this.linesTextBox1.Location = new System.Drawing.Point(0, 0);
            this.linesTextBox1.MarkWordColor = System.Drawing.Color.LightGray;
            this.linesTextBox1.MarkWordTheSameAsSelectionWord = true;
            this.linesTextBox1.Name = "linesTextBox1";
            this.linesTextBox1.selectionBorderLinePen = System.Drawing.Color.LightSteelBlue;
            this.linesTextBox1.selectionTextBackgroundBrush = System.Drawing.Color.LightSkyBlue;
            this.linesTextBox1.Size = new System.Drawing.Size(195, 197);
            this.linesTextBox1.TabIndex = 3;
            this.linesTextBox1.TabStop = false;
            // 
            // intellisenseToolTip
            // 
            this.intellisenseToolTip.AutomaticDelay = 100;
            this.intellisenseToolTip.AutoPopDelay = 0;
            this.intellisenseToolTip.InitialDelay = 100;
            this.intellisenseToolTip.ReshowDelay = 20;
            this.intellisenseToolTip.ShowAlways = true;
            // 
            // PaintTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.linesTextBox1);
            this.Controls.Add(this.vScroll);
            this.Controls.Add(this.hScroll);
            this.Name = "PaintTextBox";
            this.Size = new System.Drawing.Size(212, 214);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.HScrollBar hScroll;
        public System.Windows.Forms.VScrollBar vScroll;
        private LinesTextBox linesTextBox1;
        private System.Windows.Forms.ToolTip intellisenseToolTip;
    }
}
