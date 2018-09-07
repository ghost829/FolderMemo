namespace FolderMemo
{
    partial class Form_Memo_RIchText
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Memo_RIchText));
            this.lbl_path = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.txt_search = new System.Windows.Forms.TextBox();
            this.txt_search_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
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
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(180)))));
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Font = new System.Drawing.Font("GulimChe", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.richTextBox1.Location = new System.Drawing.Point(12, 71);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(255, 184);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            this.richTextBox1.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextBox1_LinkClicked);
            this.richTextBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.richTextBox1_KeyDown);
            this.richTextBox1.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.richTextBox1_PreviewKeyDown);
            // 
            // txt_search
            // 
            this.txt_search.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_search.Location = new System.Drawing.Point(12, 42);
            this.txt_search.Name = "txt_search";
            this.txt_search.Size = new System.Drawing.Size(175, 21);
            this.txt_search.TabIndex = 3;
            this.txt_search.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_search_KeyDown);
            // 
            // txt_search_button
            // 
            this.txt_search_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_search_button.Location = new System.Drawing.Point(193, 42);
            this.txt_search_button.Name = "txt_search_button";
            this.txt_search_button.Size = new System.Drawing.Size(74, 23);
            this.txt_search_button.TabIndex = 4;
            this.txt_search_button.Text = "찾기(F3)";
            this.txt_search_button.UseVisualStyleBackColor = true;
            this.txt_search_button.Click += new System.EventHandler(this.txt_search_button_Click);
            // 
            // Form_Memo_RIchText
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(180)))));
            this.ClientSize = new System.Drawing.Size(280, 280);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.txt_search_button);
            this.Controls.Add(this.txt_search);
            this.Controls.Add(this.lbl_path);
            this.FormBorder_ActivateColor = System.Drawing.Color.Transparent;
            this.FormBorder_DeactivateColor = System.Drawing.Color.Transparent;
            this.FormBorder_IsView = true;
            this.Location = new System.Drawing.Point(0, 0);
            this.MinimumSize = new System.Drawing.Size(280, 280);
            this.Name = "Form_Memo_RIchText";
            this.Text = "Form_Memo";
            this.TitleBoxColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(120)))));
            this.TitleBoxGradient = true;
            this.TitleColor = System.Drawing.Color.DimGray;
            this.TitleLocation = new System.Drawing.Point(15, 0);
            this.TopRight_BtnsSize = new System.Drawing.Size(20, 20);
            this.TopRight_CloseBtnHighLightImage = global::FolderMemo.Properties.Resources.ssw_close_down2;
            this.TopRight_CloseBtnMouseOverImage = global::FolderMemo.Properties.Resources.ssw_close_over1;
            this.TopRight_CloseBtnNormalImage = global::FolderMemo.Properties.Resources.ssw_close1;
            this.TopRight_MaximizeBtnHighLightImage = global::FolderMemo.Properties.Resources.ssw_expand_down2;
            this.TopRight_MaximizeBtnMouseOverImage = ((System.Drawing.Image)(resources.GetObject("$this.TopRight_MaximizeBtnMouseOverImage")));
            this.TopRight_MaximizeBtnNormalImage = ((System.Drawing.Image)(resources.GetObject("$this.TopRight_MaximizeBtnNormalImage")));
            this.TopRight_MaximizeRestoreBtnHighLightImage = global::FolderMemo.Properties.Resources.ssw_restore_down2;
            this.TopRight_MaximizeRestoreBtnMouseOverImage = ((System.Drawing.Image)(resources.GetObject("$this.TopRight_MaximizeRestoreBtnMouseOverImage")));
            this.TopRight_MaximizeRestoreBtnNormalImage = ((System.Drawing.Image)(resources.GetObject("$this.TopRight_MaximizeRestoreBtnNormalImage")));
            this.TopRight_MinimizeButtonVisible = false;
            this.VertexDirection = CustomControlLibrary.CustomForm.RectangleCorners.None;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Memo_FormClosing);
            this.Load += new System.EventHandler(this.Form_Memo_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form_Memo_RIchText_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_path;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TextBox txt_search;
        private System.Windows.Forms.Button txt_search_button;
    }
}