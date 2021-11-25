namespace FolderMemo
{
    partial class Form_alarm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_alarm));
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.paintTextBox1 = new PaintTextBoxClassLib.PaintTextBox();
            this.SuspendLayout();
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(12, 52);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(179, 21);
            this.dateTimePicker1.TabIndex = 1;
            // 
            // paintTextBox1
            // 
            this.paintTextBox1.AutoWordCompleteMode = false;
            this.paintTextBox1.BracketBackgroundBrush = System.Drawing.Color.DarkGray;
            this.paintTextBox1.comment_Color = System.Drawing.Color.Green;
            this.paintTextBox1.DoubleQuotesColor = System.Drawing.Color.Red;
            this.paintTextBox1.IndentValue = 30;
            this.paintTextBox1.isAllowTab = false;
            this.paintTextBox1.keyword1 = ((System.Collections.Generic.List<string>)(resources.GetObject("paintTextBox1.keyword1")));
            this.paintTextBox1.keyword1_Color = System.Drawing.Color.Blue;
            this.paintTextBox1.keyword2 = ((System.Collections.Generic.List<string>)(resources.GetObject("paintTextBox1.keyword2")));
            this.paintTextBox1.keyword2_Color = System.Drawing.Color.Red;
            this.paintTextBox1.keyword3 = ((System.Collections.Generic.List<string>)(resources.GetObject("paintTextBox1.keyword3")));
            this.paintTextBox1.keyword3_Color = System.Drawing.Color.Orange;
            this.paintTextBox1.LineNumberColor = System.Drawing.Color.Silver;
            this.paintTextBox1.LineNumberVisible = true;
            this.paintTextBox1.Location = new System.Drawing.Point(12, 79);
            this.paintTextBox1.MarkWordColor = System.Drawing.Color.LightGray;
            this.paintTextBox1.MarkWordTheSameAsSelectionWord = true;
            this.paintTextBox1.Name = "paintTextBox1";
            this.paintTextBox1.selectionBorderLinePen = System.Drawing.Color.LightSteelBlue;
            this.paintTextBox1.selectionTextBackgroundBrush = System.Drawing.Color.LightSkyBlue;
            this.paintTextBox1.Size = new System.Drawing.Size(212, 214);
            this.paintTextBox1.TabIndex = 2;
            this.paintTextBox1.TextBackColor = System.Drawing.Color.White;
            this.paintTextBox1.TextForeColor = System.Drawing.SystemColors.ControlText;
            // 
            // Form_alarm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(160)))));
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.paintTextBox1);
            this.Controls.Add(this.dateTimePicker1);
            this.FormBorder_ActivateColor = System.Drawing.Color.Transparent;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "Form_alarm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TitleBoxColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(80)))));
            this.TopRight_BtnsSize = new System.Drawing.Size(24, 24);
            this.TopRight_CloseBtnHighLightImage = global::FolderMemo.Properties.Resources.ssw_close_down2;
            this.TopRight_CloseBtnMouseOverImage = global::FolderMemo.Properties.Resources.ssw_close_over1;
            this.TopRight_CloseBtnNormalImage = global::FolderMemo.Properties.Resources.ssw_close1;
            this.TopRight_MaximizeButtonVisible = false;
            this.TopRight_MinimizeButtonVisible = false;
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private PaintTextBoxClassLib.PaintTextBox paintTextBox1;
    }
}