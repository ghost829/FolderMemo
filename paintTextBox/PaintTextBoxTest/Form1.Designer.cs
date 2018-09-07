namespace PaintTextBoxTest
{
    partial class Form1
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

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.paintTextBox1 = new PaintTextBoxClassLib.PaintTextBox();
            this.SuspendLayout();
            // 
            // paintTextBox1
            // 
            this.paintTextBox1.AutoWordCompleteMode = false;
            this.paintTextBox1.BracketBackgroundBrush = System.Drawing.Color.LightGray;
            this.paintTextBox1.comment_Color = System.Drawing.Color.Green;
            this.paintTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.paintTextBox1.DoubleQuotesColor = System.Drawing.Color.Red;
            this.paintTextBox1.IndentValue = 30;
            this.paintTextBox1.keyword1 = ((System.Collections.Generic.List<string>)(resources.GetObject("paintTextBox1.keyword1")));
            this.paintTextBox1.keyword1_Color = System.Drawing.Color.Blue;
            this.paintTextBox1.keyword2 = ((System.Collections.Generic.List<string>)(resources.GetObject("paintTextBox1.keyword2")));
            this.paintTextBox1.keyword2_Color = System.Drawing.Color.Red;
            this.paintTextBox1.keyword3 = ((System.Collections.Generic.List<string>)(resources.GetObject("paintTextBox1.keyword3")));
            this.paintTextBox1.keyword3_Color = System.Drawing.Color.Orange;
            this.paintTextBox1.LineNumberColor = System.Drawing.Color.Silver;
            this.paintTextBox1.LineNumberVisible = true;
            this.paintTextBox1.Location = new System.Drawing.Point(0, 0);
            this.paintTextBox1.MarkWordColor = System.Drawing.Color.LightGray;
            this.paintTextBox1.MarkWordTheSameAsSelectionWord = true;
            this.paintTextBox1.Name = "paintTextBox1";
            this.paintTextBox1.selectionBorderLinePen = System.Drawing.Color.LightSteelBlue;
            this.paintTextBox1.setDefaultValue = false;
            this.paintTextBox1.Size = new System.Drawing.Size(284, 262);
            this.paintTextBox1.TabIndex = 0;
            this.paintTextBox1.TextBackColor = System.Drawing.Color.White;
            this.paintTextBox1.TextForeColor = System.Drawing.SystemColors.ControlText;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.paintTextBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private PaintTextBoxClassLib.PaintTextBox paintTextBox1;
    }
}

