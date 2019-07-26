namespace FolderMemo
{
    partial class Form_Folder
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Folder));
            this.txt_path = new System.Windows.Forms.TextBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.btn_search = new System.Windows.Forms.Button();
            this.ctxt_form = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ctxt_form_topmost = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxt_form_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxt_listViewItem = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ctxt_lstItem_open = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxt_lstItem_rename = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxt_lstItem_delete = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxt_lstItem_copy = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxt_listView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ctxt_lstView_makeGroup = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxt_lstView_makeMemo = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxt_lstView_paste = new System.Windows.Forms.ToolStripMenuItem();
            this.txt_search = new System.Windows.Forms.TextBox();
            this.lbl_time = new System.Windows.Forms.Label();
            this.btn_upperPath = new System.Windows.Forms.Button();
            this.ctxt_form.SuspendLayout();
            this.ctxt_listViewItem.SuspendLayout();
            this.ctxt_listView.SuspendLayout();
            this.SuspendLayout();
            // 
            // txt_path
            // 
            this.txt_path.Location = new System.Drawing.Point(12, 12);
            this.txt_path.Name = "txt_path";
            this.txt_path.ReadOnly = true;
            this.txt_path.Size = new System.Drawing.Size(217, 21);
            this.txt_path.TabIndex = 3;
            this.txt_path.Text = "/";
            // 
            // listView1
            // 
            this.listView1.AllowDrop = true;
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.HideSelection = false;
            this.listView1.LabelEdit = true;
            this.listView1.LargeImageList = this.imageList1;
            this.listView1.Location = new System.Drawing.Point(10, 79);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(398, 249);
            this.listView1.SmallImageList = this.imageList1;
            this.listView1.TabIndex = 2;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listView1_AfterLabelEdit);
            this.listView1.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listView1_ItemDrag);
            this.listView1.DragDrop += new System.Windows.Forms.DragEventHandler(this.listView1_DragDrop);
            this.listView1.DragEnter += new System.Windows.Forms.DragEventHandler(this.listView1_DragEnter);
            this.listView1.DragOver += new System.Windows.Forms.DragEventHandler(this.listView1_DragOver);
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            this.listView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listView1_KeyDown);
            this.listView1.MouseLeave += new System.EventHandler(this.listView1_MouseLeave);
            this.listView1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseMove);
            this.listView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseUp);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "folder_open.ico");
            this.imageList1.Images.SetKeyName(1, "Folder_stuffed.ico");
            this.imageList1.Images.SetKeyName(2, "stick-note_32x.ico");
            this.imageList1.Images.SetKeyName(3, "stick-note_write_32x.ico");
            this.imageList1.Images.SetKeyName(4, "delete.ico");
            // 
            // btn_search
            // 
            this.btn_search.BackgroundImage = global::FolderMemo.Properties.Resources.search_2;
            this.btn_search.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_search.FlatAppearance.BorderSize = 0;
            this.btn_search.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_search.Location = new System.Drawing.Point(196, 42);
            this.btn_search.Name = "btn_search";
            this.btn_search.Size = new System.Drawing.Size(32, 31);
            this.btn_search.TabIndex = 1;
            this.btn_search.UseVisualStyleBackColor = true;
            this.btn_search.Click += new System.EventHandler(this.btn_search_Click);
            // 
            // ctxt_form
            // 
            this.ctxt_form.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxt_form_topmost,
            this.ctxt_form_exit});
            this.ctxt_form.Name = "ctxt_form";
            this.ctxt_form.Size = new System.Drawing.Size(122, 48);
            // 
            // ctxt_form_topmost
            // 
            this.ctxt_form_topmost.Name = "ctxt_form_topmost";
            this.ctxt_form_topmost.Size = new System.Drawing.Size(121, 22);
            this.ctxt_form_topmost.Text = "&TopMost";
            this.ctxt_form_topmost.Click += new System.EventHandler(this.ctxt_form_topmost_Click);
            // 
            // ctxt_form_exit
            // 
            this.ctxt_form_exit.Name = "ctxt_form_exit";
            this.ctxt_form_exit.Size = new System.Drawing.Size(121, 22);
            this.ctxt_form_exit.Text = "E&xit";
            this.ctxt_form_exit.Click += new System.EventHandler(this.ctxt_form_exit_Click);
            // 
            // ctxt_listViewItem
            // 
            this.ctxt_listViewItem.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxt_lstItem_open,
            this.ctxt_lstItem_rename,
            this.ctxt_lstItem_delete,
            this.ctxt_lstItem_copy});
            this.ctxt_listViewItem.Name = "ctxt_listViewItem";
            this.ctxt_listViewItem.Size = new System.Drawing.Size(142, 92);
            // 
            // ctxt_lstItem_open
            // 
            this.ctxt_lstItem_open.Name = "ctxt_lstItem_open";
            this.ctxt_lstItem_open.Size = new System.Drawing.Size(141, 22);
            this.ctxt_lstItem_open.Text = "열기 (&O)";
            this.ctxt_lstItem_open.Click += new System.EventHandler(this.ctxt_lstItem_open_Click);
            // 
            // ctxt_lstItem_rename
            // 
            this.ctxt_lstItem_rename.Name = "ctxt_lstItem_rename";
            this.ctxt_lstItem_rename.Size = new System.Drawing.Size(141, 22);
            this.ctxt_lstItem_rename.Text = "이름변경 (&R)";
            this.ctxt_lstItem_rename.Click += new System.EventHandler(this.ctxt_lstItem_rename_Click);
            // 
            // ctxt_lstItem_delete
            // 
            this.ctxt_lstItem_delete.Name = "ctxt_lstItem_delete";
            this.ctxt_lstItem_delete.Size = new System.Drawing.Size(141, 22);
            this.ctxt_lstItem_delete.Text = "삭제 (&D)";
            this.ctxt_lstItem_delete.Click += new System.EventHandler(this.ctxt_lstItem_delete_Click);
            // 
            // ctxt_lstItem_copy
            // 
            this.ctxt_lstItem_copy.Name = "ctxt_lstItem_copy";
            this.ctxt_lstItem_copy.Size = new System.Drawing.Size(141, 22);
            this.ctxt_lstItem_copy.Text = "복사 (&C)";
            this.ctxt_lstItem_copy.Click += new System.EventHandler(this.ctxt_lstItem_copy_Click);
            // 
            // ctxt_listView
            // 
            this.ctxt_listView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxt_lstView_makeGroup,
            this.ctxt_lstView_makeMemo,
            this.ctxt_lstView_paste});
            this.ctxt_listView.Name = "ctxt_listView";
            this.ctxt_listView.Size = new System.Drawing.Size(143, 70);
            this.ctxt_listView.Opening += new System.ComponentModel.CancelEventHandler(this.ctxt_listView_Opening);
            // 
            // ctxt_lstView_makeGroup
            // 
            this.ctxt_lstView_makeGroup.Name = "ctxt_lstView_makeGroup";
            this.ctxt_lstView_makeGroup.Size = new System.Drawing.Size(142, 22);
            this.ctxt_lstView_makeGroup.Text = "새 그룹 생성";
            this.ctxt_lstView_makeGroup.Click += new System.EventHandler(this.ctxt_lstView_makeGroup_Click);
            // 
            // ctxt_lstView_makeMemo
            // 
            this.ctxt_lstView_makeMemo.Name = "ctxt_lstView_makeMemo";
            this.ctxt_lstView_makeMemo.Size = new System.Drawing.Size(142, 22);
            this.ctxt_lstView_makeMemo.Text = "새 메모 생성";
            this.ctxt_lstView_makeMemo.Click += new System.EventHandler(this.ctxt_lstView_makeMemo_Click);
            // 
            // ctxt_lstView_paste
            // 
            this.ctxt_lstView_paste.Name = "ctxt_lstView_paste";
            this.ctxt_lstView_paste.Size = new System.Drawing.Size(142, 22);
            this.ctxt_lstView_paste.Text = "붙여넣기 (&P)";
            this.ctxt_lstView_paste.Click += new System.EventHandler(this.ctxt_lstView_paste_Click);
            // 
            // txt_search
            // 
            this.txt_search.Font = new System.Drawing.Font("Gulim", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txt_search.Location = new System.Drawing.Point(12, 44);
            this.txt_search.Name = "txt_search";
            this.txt_search.Size = new System.Drawing.Size(178, 26);
            this.txt_search.TabIndex = 0;
            this.txt_search.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_search_KeyDown);
            // 
            // lbl_time
            // 
            this.lbl_time.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_time.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbl_time.Location = new System.Drawing.Point(235, 46);
            this.lbl_time.Name = "lbl_time";
            this.lbl_time.Size = new System.Drawing.Size(173, 32);
            this.lbl_time.TabIndex = 5;
            this.lbl_time.Text = "           ";
            this.lbl_time.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btn_upperPath
            // 
            this.btn_upperPath.BackgroundImage = global::FolderMemo.Properties.Resources.folder_up;
            this.btn_upperPath.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_upperPath.FlatAppearance.BorderSize = 0;
            this.btn_upperPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_upperPath.Location = new System.Drawing.Point(235, 6);
            this.btn_upperPath.Name = "btn_upperPath";
            this.btn_upperPath.Size = new System.Drawing.Size(32, 31);
            this.btn_upperPath.TabIndex = 6;
            this.btn_upperPath.UseVisualStyleBackColor = true;
            this.btn_upperPath.Click += new System.EventHandler(this.btn_upperPath_Click);
            // 
            // Form_Folder
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(160)))));
            this.ClientSize = new System.Drawing.Size(420, 340);
            this.Controls.Add(this.lbl_time);
            this.Controls.Add(this.btn_upperPath);
            this.Controls.Add(this.txt_search);
            this.Controls.Add(this.btn_search);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.txt_path);
            this.FormBorder_ActivateColor = System.Drawing.Color.Transparent;
            this.FormBorder_IsView = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "Form_Folder";
            this.Text = "FolderMemo";
            this.TitleBoxColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(80)))));
            this.TitleHidden = true;
            this.TopRight_BtnsSize = new System.Drawing.Size(24, 24);
            this.TopRight_CloseBtnHighLightImage = global::FolderMemo.Properties.Resources.ssw_close_down2;
            this.TopRight_CloseBtnMouseOverImage = global::FolderMemo.Properties.Resources.ssw_close_over1;
            this.TopRight_CloseBtnNormalImage = global::FolderMemo.Properties.Resources.ssw_close1;
            this.TopRight_MaximizeButtonVisible = false;
            this.TopRight_MinimizeButtonVisible = false;
            this.VertexDirection = CustomControlLibrary.CustomForm.RectangleCorners.None;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Folder_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Folder_FormClosed);
            this.Load += new System.EventHandler(this.Folder_Load);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Folder_MouseUp);
            this.ctxt_form.ResumeLayout(false);
            this.ctxt_listViewItem.ResumeLayout(false);
            this.ctxt_listView.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_path;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button btn_search;
        private System.Windows.Forms.ContextMenuStrip ctxt_form;
        private System.Windows.Forms.ToolStripMenuItem ctxt_form_exit;
        private System.Windows.Forms.ToolStripMenuItem ctxt_form_topmost;
        private System.Windows.Forms.ContextMenuStrip ctxt_listViewItem;
        private System.Windows.Forms.ToolStripMenuItem ctxt_lstItem_open;
        private System.Windows.Forms.ToolStripMenuItem ctxt_lstItem_rename;
        private System.Windows.Forms.ContextMenuStrip ctxt_listView;
        private System.Windows.Forms.ToolStripMenuItem ctxt_lstView_makeGroup;
        private System.Windows.Forms.ToolStripMenuItem ctxt_lstView_makeMemo;
        private System.Windows.Forms.ToolStripMenuItem ctxt_lstItem_delete;
        private System.Windows.Forms.TextBox txt_search;
        private System.Windows.Forms.ToolStripMenuItem ctxt_lstItem_copy;
        private System.Windows.Forms.ToolStripMenuItem ctxt_lstView_paste;
        private System.Windows.Forms.Label lbl_time;
        private System.Windows.Forms.Button btn_upperPath;
    }
}