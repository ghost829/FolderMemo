namespace FolderMemo
{
    partial class Form_Memo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Memo));
            this.paintTextBox1 = new PaintTextBoxClassLib.PaintTextBox();
            this.lbl_path = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // paintTextBox1
            // 
            this.paintTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.paintTextBox1.AutoWordCompleteMode = false;
            this.paintTextBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.paintTextBox1.BracketBackgroundBrush = System.Drawing.Color.DarkGray;
            this.paintTextBox1.comment_Color = System.Drawing.SystemColors.ControlText;
            this.paintTextBox1.DoubleQuotesColor = System.Drawing.SystemColors.ControlText;
            this.paintTextBox1.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.paintTextBox1.IndentValue = 5;
            this.paintTextBox1.isAllowTab = true;
            this.paintTextBox1.keyword1 = ((System.Collections.Generic.List<string>)(resources.GetObject("paintTextBox1.keyword1")));
            this.paintTextBox1.keyword1_Color = System.Drawing.Color.Blue;
            this.paintTextBox1.keyword2 = ((System.Collections.Generic.List<string>)(resources.GetObject("paintTextBox1.keyword2")));
            this.paintTextBox1.keyword2_Color = System.Drawing.Color.Red;
            this.paintTextBox1.keyword3 = ((System.Collections.Generic.List<string>)(resources.GetObject("paintTextBox1.keyword3")));
            this.paintTextBox1.keyword3_Color = System.Drawing.Color.Orange;
            this.paintTextBox1.LineNumberColor = System.Drawing.Color.Silver;
            this.paintTextBox1.LineNumberVisible = false;
            this.paintTextBox1.Location = new System.Drawing.Point(13, 39);
            this.paintTextBox1.Margin = new System.Windows.Forms.Padding(4);
            this.paintTextBox1.MarkWordColor = System.Drawing.Color.LightGray;
            this.paintTextBox1.MarkWordTheSameAsSelectionWord = true;
            this.paintTextBox1.Name = "paintTextBox1";
            this.paintTextBox1.selectionBorderLinePen = System.Drawing.Color.Transparent;
            this.paintTextBox1.selectionTextBackgroundBrush = System.Drawing.Color.LightSkyBlue;
            this.paintTextBox1.Size = new System.Drawing.Size(254, 214);
            this.paintTextBox1.TabIndex = 0;
            this.paintTextBox1.TextBackColor = System.Drawing.Color.Transparent;
            this.paintTextBox1.TextForeColor = System.Drawing.SystemColors.ControlText;
            // 
            // lbl_path
            // 
            this.lbl_path.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_path.ForeColor = System.Drawing.Color.DarkGray;
            this.lbl_path.Location = new System.Drawing.Point(13, 258);
            this.lbl_path.Name = "lbl_path";
            this.lbl_path.Size = new System.Drawing.Size(254, 13);
            this.lbl_path.TabIndex = 1;
            // 
            // Form_Memo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(180)))));
            this.ClientSize = new System.Drawing.Size(280, 280);
            this.Controls.Add(this.lbl_path);
            this.Controls.Add(this.paintTextBox1);
            this.FormBorder_ActivateColor = System.Drawing.Color.Transparent;
            this.FormBorder_IsView = true;
            this.Location = new System.Drawing.Point(0, 0);
            this.MinimumSize = new System.Drawing.Size(280, 280);
            this.Name = "Form_Memo";
            this.Text = "Form_Memo";
            this.TitleBoxColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(120)))));
            this.TitleBoxGradient = true;
            this.TitleColor = System.Drawing.Color.Gray;
            this.TitleLocation = new System.Drawing.Point(15, 0);
            this.TopRight_MinimizeButtonVisible = false;
            this.VertexDirection = CustomControlLibrary.CustomForm.RectangleCorners.None;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Memo_FormClosing);
            this.Load += new System.EventHandler(this.Form_Memo_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private PaintTextBoxClassLib.PaintTextBox paintTextBox1;
        private System.Windows.Forms.Label lbl_path;



    }
}