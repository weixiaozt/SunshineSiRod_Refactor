using Sunny.UI;

namespace SquareSiliconStickCheck.Pages
{
    partial class MainSquareCheckResultListPage
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
            this.uiDataGridViewFooter1 = new Sunny.UI.UIDataGridViewFooter();
            this.uiPagination1 = new Sunny.UI.UIPagination();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.dateTimePickerbegin = new Sunny.UI.UIDatetimePicker();
            this.label1 = new Sunny.UI.UILabel();
            this.label2 = new Sunny.UI.UILabel();
            this.dateTimePickerEnd = new Sunny.UI.UIDatetimePicker();
            this.buttonSearch = new Sunny.UI.UIButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panelRadiusViewFull_Full = new Sunny.UI.UITitlePanel();
            this.label8 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.labelRightAngle = new System.Windows.Forms.Label();
            this.labelLeftAngle = new System.Windows.Forms.Label();
            this.labelDownAngle = new System.Windows.Forms.Label();
            this.labelTopAngle = new System.Windows.Forms.Label();
            this.labelDownLeftDiag = new System.Windows.Forms.Label();
            this.labelDownRightDiag = new System.Windows.Forms.Label();
            this.labelRightLeftDiag = new System.Windows.Forms.Label();
            this.labelRightRightDiag = new System.Windows.Forms.Label();
            this.labelLeftLeftDiag = new System.Windows.Forms.Label();
            this.labelLeftRightDiag = new System.Windows.Forms.Label();
            this.labelTopLeftDiag = new System.Windows.Forms.Label();
            this.labelTopRightDiag = new System.Windows.Forms.Label();
            this.labelLength = new System.Windows.Forms.Label();
            this.labelDownDiag = new System.Windows.Forms.Label();
            this.labelRightDiag = new System.Windows.Forms.Label();
            this.labelLeftDiag = new System.Windows.Forms.Label();
            this.labelTopDiag = new System.Windows.Forms.Label();
            this.labelLRLength = new System.Windows.Forms.Label();
            this.labelTDLength = new System.Windows.Forms.Label();
            this.labelRDLength = new System.Windows.Forms.Label();
            this.labelLDLength = new System.Windows.Forms.Label();
            this.labelRTLength = new System.Windows.Forms.Label();
            this.labelLTLength = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBoxJB = new System.Windows.Forms.TextBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.groupBox1.SuspendLayout();
            this.panelRadiusViewFull_Full.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiDataGridViewFooter1
            // 
            this.uiDataGridViewFooter1.DataGridView = null;
            this.uiDataGridViewFooter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.uiDataGridViewFooter1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiDataGridViewFooter1.Location = new System.Drawing.Point(0, 1011);
            this.uiDataGridViewFooter1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiDataGridViewFooter1.Name = "uiDataGridViewFooter1";
            this.uiDataGridViewFooter1.RadiusSides = Sunny.UI.UICornerRadiusSides.None;
            this.uiDataGridViewFooter1.RectSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)(((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.uiDataGridViewFooter1.Size = new System.Drawing.Size(1889, 29);
            this.uiDataGridViewFooter1.TabIndex = 5;
            this.uiDataGridViewFooter1.Text = "uiDataGridViewFooter1";
            // 
            // uiPagination1
            // 
            this.uiPagination1.ActivePage = 20;
            this.uiPagination1.CausesValidation = false;
            this.uiPagination1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.uiPagination1.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.uiPagination1.Location = new System.Drawing.Point(3, 1102);
            this.uiPagination1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiPagination1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPagination1.Name = "uiPagination1";
            this.uiPagination1.PagerCount = 9;
            this.uiPagination1.PageSize = 10;
            this.uiPagination1.RadiusSides = Sunny.UI.UICornerRadiusSides.None;
            this.uiPagination1.RectSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.None;
            this.uiPagination1.ShowText = false;
            this.uiPagination1.Size = new System.Drawing.Size(2274, 35);
            this.uiPagination1.Style = Sunny.UI.UIStyle.Custom;
            this.uiPagination1.TabIndex = 4;
            this.uiPagination1.Text = "uiDataGridPage1";
            this.uiPagination1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.uiPagination1.TotalCount = 40000;
            this.uiPagination1.PageChanged += new Sunny.UI.UIPagination.OnPageChangeEventHandler(this.uiPagination1_PageChanged);
            // 
            // dateTimePickerbegin
            // 
            this.dateTimePickerbegin.FillColor = System.Drawing.Color.White;
            this.dateTimePickerbegin.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dateTimePickerbegin.Location = new System.Drawing.Point(404, 33);
            this.dateTimePickerbegin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dateTimePickerbegin.MaxLength = 19;
            this.dateTimePickerbegin.MinimumSize = new System.Drawing.Size(63, 0);
            this.dateTimePickerbegin.Name = "dateTimePickerbegin";
            this.dateTimePickerbegin.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.dateTimePickerbegin.Size = new System.Drawing.Size(214, 34);
            this.dateTimePickerbegin.Style = Sunny.UI.UIStyle.Custom;
            this.dateTimePickerbegin.SymbolDropDown = 61555;
            this.dateTimePickerbegin.SymbolNormal = 61555;
            this.dateTimePickerbegin.TabIndex = 7;
            this.dateTimePickerbegin.Text = "2023-07-12 11:59:30";
            this.dateTimePickerbegin.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.dateTimePickerbegin.Value = new System.DateTime(2023, 7, 12, 11, 59, 30, 651);
            this.dateTimePickerbegin.Visible = false;
            this.dateTimePickerbegin.Watermark = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(327, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 21);
            this.label1.Style = Sunny.UI.UIStyle.Custom;
            this.label1.TabIndex = 8;
            this.label1.Text = "开始";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(676, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 21);
            this.label2.Style = Sunny.UI.UIStyle.Custom;
            this.label2.TabIndex = 10;
            this.label2.Text = "结束";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label2.Visible = false;
            // 
            // dateTimePickerEnd
            // 
            this.dateTimePickerEnd.FillColor = System.Drawing.Color.White;
            this.dateTimePickerEnd.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dateTimePickerEnd.Location = new System.Drawing.Point(753, 33);
            this.dateTimePickerEnd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dateTimePickerEnd.MaxLength = 19;
            this.dateTimePickerEnd.MinimumSize = new System.Drawing.Size(63, 0);
            this.dateTimePickerEnd.Name = "dateTimePickerEnd";
            this.dateTimePickerEnd.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.dateTimePickerEnd.Size = new System.Drawing.Size(214, 34);
            this.dateTimePickerEnd.Style = Sunny.UI.UIStyle.Custom;
            this.dateTimePickerEnd.SymbolDropDown = 61555;
            this.dateTimePickerEnd.SymbolNormal = 61555;
            this.dateTimePickerEnd.TabIndex = 9;
            this.dateTimePickerEnd.Text = "2023-07-12 11:59:30";
            this.dateTimePickerEnd.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.dateTimePickerEnd.Value = new System.DateTime(2023, 7, 12, 11, 59, 30, 658);
            this.dateTimePickerEnd.Visible = false;
            this.dateTimePickerEnd.Watermark = "";
            // 
            // buttonSearch
            // 
            this.buttonSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this.buttonSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonSearch.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this.buttonSearch.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(190)))), ((int)(((byte)(138)))));
            this.buttonSearch.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(175)))), ((int)(((byte)(36)))));
            this.buttonSearch.FillSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this.buttonSearch.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonSearch.Location = new System.Drawing.Point(989, 33);
            this.buttonSearch.MinimumSize = new System.Drawing.Size(1, 1);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(103, 34);
            this.buttonSearch.Style = Sunny.UI.UIStyle.Custom;
            this.buttonSearch.TabIndex = 11;
            this.buttonSearch.Text = "查询";
            this.buttonSearch.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panelRadiusViewFull_Full);
            this.groupBox1.Controls.Add(this.uiPagination1);
            this.groupBox1.Location = new System.Drawing.Point(28, 110);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(2280, 1140);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "结果展示";
            // 
            // panelRadiusViewFull_Full
            // 
            this.panelRadiusViewFull_Full.Controls.Add(this.label8);
            this.panelRadiusViewFull_Full.Controls.Add(this.label5);
            this.panelRadiusViewFull_Full.Controls.Add(this.label4);
            this.panelRadiusViewFull_Full.Controls.Add(this.label3);
            this.panelRadiusViewFull_Full.Controls.Add(this.label6);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelRightAngle);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelLeftAngle);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelDownAngle);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelTopAngle);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelDownLeftDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelDownRightDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelRightLeftDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelRightRightDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelLeftLeftDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelLeftRightDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelTopLeftDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelTopRightDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelLength);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelDownDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelRightDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelLeftDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelTopDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelLRLength);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelTDLength);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelRDLength);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelLDLength);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelRTLength);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelLTLength);
            this.panelRadiusViewFull_Full.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.panelRadiusViewFull_Full.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.panelRadiusViewFull_Full.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.panelRadiusViewFull_Full.Location = new System.Drawing.Point(7, 30);
            this.panelRadiusViewFull_Full.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelRadiusViewFull_Full.MinimumSize = new System.Drawing.Size(1, 1);
            this.panelRadiusViewFull_Full.Name = "panelRadiusViewFull_Full";
            this.panelRadiusViewFull_Full.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.panelRadiusViewFull_Full.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.panelRadiusViewFull_Full.ShowText = false;
            this.panelRadiusViewFull_Full.Size = new System.Drawing.Size(1780, 1050);
            this.panelRadiusViewFull_Full.Style = Sunny.UI.UIStyle.Custom;
            this.panelRadiusViewFull_Full.TabIndex = 5;
            this.panelRadiusViewFull_Full.Text = null;
            this.panelRadiusViewFull_Full.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.panelRadiusViewFull_Full.TitleColor = System.Drawing.Color.Gray;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("宋体", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label8.Location = new System.Drawing.Point(68, 573);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(123, 35);
            this.label8.TabIndex = 43;
            this.label8.Text = "结果：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label5.Location = new System.Drawing.Point(1165, 398);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 21);
            this.label5.TabIndex = 42;
            this.label5.Text = "机台号：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label4.Location = new System.Drawing.Point(1165, 260);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 21);
            this.label4.TabIndex = 40;
            this.label4.Text = "规格：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label3.Location = new System.Drawing.Point(1165, 126);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 21);
            this.label3.TabIndex = 38;
            this.label3.Text = "尾端面垂直度：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label6.Location = new System.Drawing.Point(819, 126);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(122, 21);
            this.label6.TabIndex = 37;
            this.label6.Text = "头端面垂直度：";
            // 
            // labelRightAngle
            // 
            this.labelRightAngle.AutoSize = true;
            this.labelRightAngle.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRightAngle.Location = new System.Drawing.Point(1165, 193);
            this.labelRightAngle.Name = "labelRightAngle";
            this.labelRightAngle.Size = new System.Drawing.Size(122, 21);
            this.labelRightAngle.TabIndex = 34;
            this.labelRightAngle.Text = "右侧直边角度：";
            // 
            // labelLeftAngle
            // 
            this.labelLeftAngle.AutoSize = true;
            this.labelLeftAngle.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLeftAngle.Location = new System.Drawing.Point(819, 193);
            this.labelLeftAngle.Name = "labelLeftAngle";
            this.labelLeftAngle.Size = new System.Drawing.Size(122, 21);
            this.labelLeftAngle.TabIndex = 33;
            this.labelLeftAngle.Text = "左侧直边角度：";
            // 
            // labelDownAngle
            // 
            this.labelDownAngle.AutoSize = true;
            this.labelDownAngle.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelDownAngle.Location = new System.Drawing.Point(427, 193);
            this.labelDownAngle.Name = "labelDownAngle";
            this.labelDownAngle.Size = new System.Drawing.Size(122, 21);
            this.labelDownAngle.TabIndex = 32;
            this.labelDownAngle.Text = "下侧直边角度：";
            // 
            // labelTopAngle
            // 
            this.labelTopAngle.AutoSize = true;
            this.labelTopAngle.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTopAngle.Location = new System.Drawing.Point(69, 193);
            this.labelTopAngle.Name = "labelTopAngle";
            this.labelTopAngle.Size = new System.Drawing.Size(122, 21);
            this.labelTopAngle.TabIndex = 31;
            this.labelTopAngle.Text = "上侧直边角度：";
            // 
            // labelDownLeftDiag
            // 
            this.labelDownLeftDiag.AutoSize = true;
            this.labelDownLeftDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelDownLeftDiag.Location = new System.Drawing.Point(427, 469);
            this.labelDownLeftDiag.Name = "labelDownLeftDiag";
            this.labelDownLeftDiag.Size = new System.Drawing.Size(154, 21);
            this.labelDownLeftDiag.TabIndex = 30;
            this.labelDownLeftDiag.Text = "下侧弧左弧长投影：";
            // 
            // labelDownRightDiag
            // 
            this.labelDownRightDiag.AutoSize = true;
            this.labelDownRightDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelDownRightDiag.Location = new System.Drawing.Point(819, 469);
            this.labelDownRightDiag.Name = "labelDownRightDiag";
            this.labelDownRightDiag.Size = new System.Drawing.Size(154, 21);
            this.labelDownRightDiag.TabIndex = 29;
            this.labelDownRightDiag.Text = "下侧弧右弧长投影：";
            // 
            // labelRightLeftDiag
            // 
            this.labelRightLeftDiag.AutoSize = true;
            this.labelRightLeftDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRightLeftDiag.Location = new System.Drawing.Point(427, 398);
            this.labelRightLeftDiag.Name = "labelRightLeftDiag";
            this.labelRightLeftDiag.Size = new System.Drawing.Size(154, 21);
            this.labelRightLeftDiag.TabIndex = 28;
            this.labelRightLeftDiag.Text = "右侧弧左弧长投影：";
            // 
            // labelRightRightDiag
            // 
            this.labelRightRightDiag.AutoSize = true;
            this.labelRightRightDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRightRightDiag.Location = new System.Drawing.Point(819, 398);
            this.labelRightRightDiag.Name = "labelRightRightDiag";
            this.labelRightRightDiag.Size = new System.Drawing.Size(154, 21);
            this.labelRightRightDiag.TabIndex = 27;
            this.labelRightRightDiag.Text = "右侧弧右弧长投影：";
            // 
            // labelLeftLeftDiag
            // 
            this.labelLeftLeftDiag.AutoSize = true;
            this.labelLeftLeftDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLeftLeftDiag.Location = new System.Drawing.Point(427, 332);
            this.labelLeftLeftDiag.Name = "labelLeftLeftDiag";
            this.labelLeftLeftDiag.Size = new System.Drawing.Size(154, 21);
            this.labelLeftLeftDiag.TabIndex = 26;
            this.labelLeftLeftDiag.Text = "左侧弧左弧长投影：";
            // 
            // labelLeftRightDiag
            // 
            this.labelLeftRightDiag.AutoSize = true;
            this.labelLeftRightDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLeftRightDiag.Location = new System.Drawing.Point(819, 332);
            this.labelLeftRightDiag.Name = "labelLeftRightDiag";
            this.labelLeftRightDiag.Size = new System.Drawing.Size(154, 21);
            this.labelLeftRightDiag.TabIndex = 25;
            this.labelLeftRightDiag.Text = "左侧弧右弧长投影：";
            // 
            // labelTopLeftDiag
            // 
            this.labelTopLeftDiag.AutoSize = true;
            this.labelTopLeftDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTopLeftDiag.Location = new System.Drawing.Point(427, 260);
            this.labelTopLeftDiag.Name = "labelTopLeftDiag";
            this.labelTopLeftDiag.Size = new System.Drawing.Size(154, 21);
            this.labelTopLeftDiag.TabIndex = 24;
            this.labelTopLeftDiag.Text = "上侧弧左弧长投影：";
            // 
            // labelTopRightDiag
            // 
            this.labelTopRightDiag.AutoSize = true;
            this.labelTopRightDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTopRightDiag.Location = new System.Drawing.Point(819, 260);
            this.labelTopRightDiag.Name = "labelTopRightDiag";
            this.labelTopRightDiag.Size = new System.Drawing.Size(154, 21);
            this.labelTopRightDiag.TabIndex = 23;
            this.labelTopRightDiag.Text = "上侧弧右弧长投影：";
            // 
            // labelLength
            // 
            this.labelLength.AutoSize = true;
            this.labelLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLength.Location = new System.Drawing.Point(1165, 332);
            this.labelLength.Name = "labelLength";
            this.labelLength.Size = new System.Drawing.Size(58, 21);
            this.labelLength.TabIndex = 22;
            this.labelLength.Text = "棒长：";
            // 
            // labelDownDiag
            // 
            this.labelDownDiag.AutoSize = true;
            this.labelDownDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelDownDiag.Location = new System.Drawing.Point(70, 469);
            this.labelDownDiag.Name = "labelDownDiag";
            this.labelDownDiag.Size = new System.Drawing.Size(90, 21);
            this.labelDownDiag.TabIndex = 21;
            this.labelDownDiag.Text = "下侧弧长：";
            // 
            // labelRightDiag
            // 
            this.labelRightDiag.AutoSize = true;
            this.labelRightDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRightDiag.Location = new System.Drawing.Point(70, 398);
            this.labelRightDiag.Name = "labelRightDiag";
            this.labelRightDiag.Size = new System.Drawing.Size(90, 21);
            this.labelRightDiag.TabIndex = 20;
            this.labelRightDiag.Text = "右侧弧长：";
            // 
            // labelLeftDiag
            // 
            this.labelLeftDiag.AutoSize = true;
            this.labelLeftDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLeftDiag.Location = new System.Drawing.Point(70, 332);
            this.labelLeftDiag.Name = "labelLeftDiag";
            this.labelLeftDiag.Size = new System.Drawing.Size(90, 21);
            this.labelLeftDiag.TabIndex = 19;
            this.labelLeftDiag.Text = "左侧弧长：";
            // 
            // labelTopDiag
            // 
            this.labelTopDiag.AutoSize = true;
            this.labelTopDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTopDiag.Location = new System.Drawing.Point(70, 260);
            this.labelTopDiag.Name = "labelTopDiag";
            this.labelTopDiag.Size = new System.Drawing.Size(90, 21);
            this.labelTopDiag.TabIndex = 18;
            this.labelTopDiag.Text = "上侧弧长：";
            // 
            // labelLRLength
            // 
            this.labelLRLength.AutoSize = true;
            this.labelLRLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLRLength.Location = new System.Drawing.Point(427, 126);
            this.labelLRLength.Name = "labelLRLength";
            this.labelLRLength.Size = new System.Drawing.Size(106, 21);
            this.labelLRLength.TabIndex = 17;
            this.labelLRLength.Text = "左右对角线：";
            // 
            // labelTDLength
            // 
            this.labelTDLength.AutoSize = true;
            this.labelTDLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTDLength.Location = new System.Drawing.Point(70, 126);
            this.labelTDLength.Name = "labelTDLength";
            this.labelTDLength.Size = new System.Drawing.Size(106, 21);
            this.labelTDLength.TabIndex = 16;
            this.labelTDLength.Text = "上下对角线：";
            // 
            // labelRDLength
            // 
            this.labelRDLength.AutoSize = true;
            this.labelRDLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRDLength.Location = new System.Drawing.Point(1165, 67);
            this.labelRDLength.Name = "labelRDLength";
            this.labelRDLength.Size = new System.Drawing.Size(86, 21);
            this.labelRDLength.TabIndex = 15;
            this.labelRDLength.Text = "D面边长：";
            // 
            // labelLDLength
            // 
            this.labelLDLength.AutoSize = true;
            this.labelLDLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLDLength.Location = new System.Drawing.Point(819, 67);
            this.labelLDLength.Name = "labelLDLength";
            this.labelLDLength.Size = new System.Drawing.Size(85, 21);
            this.labelLDLength.TabIndex = 14;
            this.labelLDLength.Text = "C面边长：";
            // 
            // labelRTLength
            // 
            this.labelRTLength.AutoSize = true;
            this.labelRTLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRTLength.Location = new System.Drawing.Point(427, 67);
            this.labelRTLength.Name = "labelRTLength";
            this.labelRTLength.Size = new System.Drawing.Size(84, 21);
            this.labelRTLength.TabIndex = 13;
            this.labelRTLength.Text = "B面边长：";
            // 
            // labelLTLength
            // 
            this.labelLTLength.AutoSize = true;
            this.labelLTLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLTLength.Location = new System.Drawing.Point(70, 64);
            this.labelLTLength.Name = "labelLTLength";
            this.labelLTLength.Size = new System.Drawing.Size(85, 21);
            this.labelLTLength.TabIndex = 12;
            this.labelLTLength.Text = "A面边长：";
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.groupBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.groupBox2.Controls.Add(this.textBoxJB);
            this.groupBox2.Controls.Add(this.uiLabel1);
            this.groupBox2.Controls.Add(this.buttonSearch);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.dateTimePickerEnd);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.dateTimePickerbegin);
            this.groupBox2.Location = new System.Drawing.Point(29, 35);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1762, 82);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "查询";
            // 
            // textBoxJB
            // 
            this.textBoxJB.Location = new System.Drawing.Point(80, 35);
            this.textBoxJB.Name = "textBoxJB";
            this.textBoxJB.Size = new System.Drawing.Size(188, 29);
            this.textBoxJB.TabIndex = 13;
            // 
            // uiLabel1
            // 
            this.uiLabel1.AutoSize = true;
            this.uiLabel1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel1.Location = new System.Drawing.Point(15, 35);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(61, 21);
            this.uiLabel1.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel1.TabIndex = 12;
            this.uiLabel1.Text = "晶编：";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MainSquareCheckResultListPage
            // 
            this.AllowShowTitle = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.ClientSize = new System.Drawing.Size(1940, 1100);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Name = "MainSquareCheckResultListPage";
            this.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.PageIndex = 1003;
            this.ShowTitle = true;
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "结果";
            this.TitleFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.groupBox1.ResumeLayout(false);
            this.panelRadiusViewFull_Full.ResumeLayout(false);
            this.panelRadiusViewFull_Full.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }



        #endregion

        private UIDataGridViewFooter uiDataGridViewFooter1;
        private UIPagination uiPagination1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private UIDatetimePicker dateTimePickerbegin;
        private UILabel label1;
        private UILabel label2;
        private UIDatetimePicker dateTimePickerEnd;
        private UIButton buttonSearch;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private UILabel uiLabel1;
        private System.Windows.Forms.TextBox textBoxJB;
        private UITitlePanel panelRadiusViewFull_Full;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label labelRightAngle;
        private System.Windows.Forms.Label labelLeftAngle;
        private System.Windows.Forms.Label labelDownAngle;
        private System.Windows.Forms.Label labelTopAngle;
        private System.Windows.Forms.Label labelDownLeftDiag;
        private System.Windows.Forms.Label labelDownRightDiag;
        private System.Windows.Forms.Label labelRightLeftDiag;
        private System.Windows.Forms.Label labelRightRightDiag;
        private System.Windows.Forms.Label labelLeftLeftDiag;
        private System.Windows.Forms.Label labelLeftRightDiag;
        private System.Windows.Forms.Label labelTopLeftDiag;
        private System.Windows.Forms.Label labelTopRightDiag;
        private System.Windows.Forms.Label labelLength;
        private System.Windows.Forms.Label labelDownDiag;
        private System.Windows.Forms.Label labelRightDiag;
        private System.Windows.Forms.Label labelLeftDiag;
        private System.Windows.Forms.Label labelTopDiag;
        private System.Windows.Forms.Label labelLRLength;
        private System.Windows.Forms.Label labelTDLength;
        private System.Windows.Forms.Label labelRDLength;
        private System.Windows.Forms.Label labelLDLength;
        private System.Windows.Forms.Label labelRTLength;
        private System.Windows.Forms.Label labelLTLength;
    }
}