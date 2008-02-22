using Extensibility;
using EnvDTE;
using EnvDTE80;

namespace Tnrsoft.VUTPP
{
    partial class UnitTestBrowser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UnitTestBrowser));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.Tests = new System.Windows.Forms.TabPage();
            this.progressBar = new VistaStyleProgressBar.ProgressBar();
            this.Stop = new System.Windows.Forms.Button();
            this.RunSelected = new System.Windows.Forms.Button();
            this.RunAll = new System.Windows.Forms.Button();
            this.RefreshTestList = new System.Windows.Forms.Button();
            this.TestList = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.Config = new System.Windows.Forms.TabPage();
            this.ReparseTick = new System.Windows.Forms.NumericUpDown();
            this.WatchCurrentFile = new System.Windows.Forms.CheckBox();
            this.GotoLineSelect = new System.Windows.Forms.CheckBox();
            this.ReduceFilename = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.linkLabel4 = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.Tests.SuspendLayout();
            this.Config.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ReparseTick)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.Tests);
            this.tabControl1.Controls.Add(this.Config);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(375, 545);
            this.tabControl1.TabIndex = 0;
            // 
            // Tests
            // 
            this.Tests.BackColor = System.Drawing.SystemColors.Control;
            this.Tests.Controls.Add(this.progressBar);
            this.Tests.Controls.Add(this.Stop);
            this.Tests.Controls.Add(this.RunSelected);
            this.Tests.Controls.Add(this.RunAll);
            this.Tests.Controls.Add(this.RefreshTestList);
            this.Tests.Controls.Add(this.TestList);
            this.Tests.Location = new System.Drawing.Point(4, 22);
            this.Tests.Name = "Tests";
            this.Tests.Padding = new System.Windows.Forms.Padding(3);
            this.Tests.Size = new System.Drawing.Size(367, 519);
            this.Tests.TabIndex = 0;
            this.Tests.Text = "Tests";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.BackColor = System.Drawing.Color.White;
            this.progressBar.EndColor = System.Drawing.Color.LimeGreen;
            this.progressBar.Location = new System.Drawing.Point(1, 29);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(363, 23);
            this.progressBar.StartColor = System.Drawing.Color.LimeGreen;
            this.progressBar.TabIndex = 5;
            this.progressBar.Value = 50;
            // 
            // Stop
            // 
            this.Stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Stop.Enabled = false;
            this.Stop.Location = new System.Drawing.Point(316, 0);
            this.Stop.Name = "Stop";
            this.Stop.Size = new System.Drawing.Size(48, 23);
            this.Stop.TabIndex = 4;
            this.Stop.Text = "Stop";
            this.Stop.UseVisualStyleBackColor = true;
            this.Stop.Click += new System.EventHandler(this.Stop_Click);
            // 
            // RunSelected
            // 
            this.RunSelected.Location = new System.Drawing.Point(65, 0);
            this.RunSelected.Name = "RunSelected";
            this.RunSelected.Size = new System.Drawing.Size(93, 23);
            this.RunSelected.TabIndex = 3;
            this.RunSelected.Text = "Run Selected";
            this.RunSelected.UseVisualStyleBackColor = true;
            this.RunSelected.Click += new System.EventHandler(this.RunSelected_Click);
            // 
            // RunAll
            // 
            this.RunAll.Location = new System.Drawing.Point(0, 0);
            this.RunAll.Name = "RunAll";
            this.RunAll.Size = new System.Drawing.Size(59, 23);
            this.RunAll.TabIndex = 2;
            this.RunAll.Text = "Run All";
            this.RunAll.UseVisualStyleBackColor = true;
            this.RunAll.Click += new System.EventHandler(this.RunAll_Click);
            // 
            // RefreshTestList
            // 
            this.RefreshTestList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RefreshTestList.Location = new System.Drawing.Point(0, 496);
            this.RefreshTestList.Name = "RefreshTestList";
            this.RefreshTestList.Size = new System.Drawing.Size(134, 23);
            this.RefreshTestList.TabIndex = 1;
            this.RefreshTestList.Text = "Refresh Tests";
            this.RefreshTestList.UseVisualStyleBackColor = true;
            this.RefreshTestList.Click += new System.EventHandler(this.RefreshTestList_Click);
            // 
            // TestList
            // 
            this.TestList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TestList.HotTracking = true;
            this.TestList.ImageIndex = 0;
            this.TestList.ImageList = this.imageList1;
            this.TestList.Location = new System.Drawing.Point(0, 58);
            this.TestList.Name = "TestList";
            this.TestList.SelectedImageIndex = 0;
            this.TestList.Size = new System.Drawing.Size(367, 433);
            this.TestList.TabIndex = 0;
            this.TestList.NodeMouseHover += new System.Windows.Forms.TreeNodeMouseHoverEventHandler(this.TestList_NodeMouseHover);
            this.TestList.DoubleClick += new System.EventHandler(this.TestList_DoubleClick);
            this.TestList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TestList_AfterSelect);
            this.TestList.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.TestList_BeforeSelect);
            this.TestList.MouseLeave += new System.EventHandler(this.TestList_MouseLeave);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "READY");
            this.imageList1.Images.SetKeyName(1, "START");
            this.imageList1.Images.SetKeyName(2, "FAIL");
            this.imageList1.Images.SetKeyName(3, "SUCCESS");
            this.imageList1.Images.SetKeyName(4, "CHECK");
            this.imageList1.Images.SetKeyName(5, "ERROR");
            // 
            // Config
            // 
            this.Config.BackColor = System.Drawing.SystemColors.Control;
            this.Config.Controls.Add(this.ReparseTick);
            this.Config.Controls.Add(this.WatchCurrentFile);
            this.Config.Controls.Add(this.GotoLineSelect);
            this.Config.Controls.Add(this.ReduceFilename);
            this.Config.Controls.Add(this.groupBox1);
            this.Config.Location = new System.Drawing.Point(4, 22);
            this.Config.Name = "Config";
            this.Config.Padding = new System.Windows.Forms.Padding(3);
            this.Config.Size = new System.Drawing.Size(367, 519);
            this.Config.TabIndex = 1;
            this.Config.Text = "Config";
            // 
            // ReparseTick
            // 
            this.ReparseTick.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.ReparseTick.Location = new System.Drawing.Point(127, 48);
            this.ReparseTick.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.ReparseTick.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.ReparseTick.Name = "ReparseTick";
            this.ReparseTick.Size = new System.Drawing.Size(76, 21);
            this.ReparseTick.TabIndex = 3;
            this.ReparseTick.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.ReparseTick.ValueChanged += new System.EventHandler(this.ReparseTick_ValueChanged);
            // 
            // WatchCurrentFile
            // 
            this.WatchCurrentFile.AutoSize = true;
            this.WatchCurrentFile.Location = new System.Drawing.Point(0, 49);
            this.WatchCurrentFile.Name = "WatchCurrentFile";
            this.WatchCurrentFile.Size = new System.Drawing.Size(121, 16);
            this.WatchCurrentFile.TabIndex = 2;
            this.WatchCurrentFile.Text = "Watch current file";
            this.WatchCurrentFile.UseVisualStyleBackColor = true;
            this.WatchCurrentFile.CheckedChanged += new System.EventHandler(this.WatchCurrentFile_CheckedChanged);
            // 
            // GotoLineSelect
            // 
            this.GotoLineSelect.AutoSize = true;
            this.GotoLineSelect.Location = new System.Drawing.Point(0, 26);
            this.GotoLineSelect.Name = "GotoLineSelect";
            this.GotoLineSelect.Size = new System.Drawing.Size(112, 16);
            this.GotoLineSelect.TabIndex = 1;
            this.GotoLineSelect.Text = "Goto line select";
            this.GotoLineSelect.UseVisualStyleBackColor = true;
            this.GotoLineSelect.CheckedChanged += new System.EventHandler(this.GotoLineSelect_CheckedChanged);
            // 
            // ReduceFilename
            // 
            this.ReduceFilename.AutoSize = true;
            this.ReduceFilename.Location = new System.Drawing.Point(0, 4);
            this.ReduceFilename.Name = "ReduceFilename";
            this.ReduceFilename.Size = new System.Drawing.Size(119, 16);
            this.ReduceFilename.TabIndex = 1;
            this.ReduceFilename.Text = "Reduce filename";
            this.ReduceFilename.UseVisualStyleBackColor = true;
            this.ReduceFilename.CheckedChanged += new System.EventHandler(this.ReduceFilename_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.linkLabel4);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.linkLabel3);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.linkLabel2);
            this.groupBox1.Controls.Add(this.linkLabel1);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox1.Location = new System.Drawing.Point(3, 398);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(361, 118);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 100);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "Icon File :";
            // 
            // linkLabel4
            // 
            this.linkLabel4.AutoSize = true;
            this.linkLabel4.Location = new System.Drawing.Point(94, 100);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(233, 12);
            this.linkLabel4.TabIndex = 7;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Text = "http://www.famfamfam.com/lab/icons/";
            this.linkLabel4.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel4_LinkClicked);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 81);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "Embeded :";
            // 
            // linkLabel3
            // 
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.Location = new System.Drawing.Point(94, 81);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(176, 12);
            this.linkLabel3.TabIndex = 5;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "Vista Style Progress Bar in C#";
            this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 60);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "Author Home :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "Project Home :";
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(94, 60);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(142, 12);
            this.linkLabel2.TabIndex = 2;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "http://www.larosel.com";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(94, 38);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(170, 12);
            this.linkLabel1.TabIndex = 1;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "http://vutpp.googlecode.com";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(118, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "VisualUnitTest++ 0.5";
            // 
            // UnitTestBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Name = "UnitTestBrowser";
            this.Size = new System.Drawing.Size(375, 545);
            this.tabControl1.ResumeLayout(false);
            this.Tests.ResumeLayout(false);
            this.Config.ResumeLayout(false);
            this.Config.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ReparseTick)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage Tests;
        private System.Windows.Forms.TabPage Config;
        public System.Windows.Forms.Button RefreshTestList;
        public System.Windows.Forms.TreeView TestList;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Button RunSelected;
        public System.Windows.Forms.Button RunAll;
        public System.Windows.Forms.Button Stop;
        public VistaStyleProgressBar.ProgressBar progressBar;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.LinkLabel linkLabel3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox ReduceFilename;
        private System.Windows.Forms.CheckBox GotoLineSelect;
        private System.Windows.Forms.NumericUpDown ReparseTick;
        private System.Windows.Forms.CheckBox WatchCurrentFile;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.LinkLabel linkLabel4;
        private System.Windows.Forms.ImageList imageList1;

    }
}
