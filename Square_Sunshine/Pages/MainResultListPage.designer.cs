using Sunny.UI;

namespace SquareSiliconStickCheck.Pages
{
    partial class MainResultListPage
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this._rightBtn = new Sunny.UI.UIButton();
            this.uiDataGridViewFooter1 = new Sunny.UI.UIDataGridViewFooter();
            this.uiDataGridView1 = new Sunny.UI.UIDataGridView();
            this.uiPagination1 = new Sunny.UI.UIPagination();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.dateTimePickerbegin = new Sunny.UI.UIDatetimePicker();
            this.label1 = new Sunny.UI.UILabel();
            this.label2 = new Sunny.UI.UILabel();
            this.dateTimePickerEnd = new Sunny.UI.UIDatetimePicker();
            this.buttonSearch = new Sunny.UI.UIButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.uiDataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rightBtn
            // 
            this._rightBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this._rightBtn.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._rightBtn.Location = new System.Drawing.Point(0, 362);
            this._rightBtn.MinimumSize = new System.Drawing.Size(1, 1);
            this._rightBtn.Name = "_rightBtn";
            this._rightBtn.Size = new System.Drawing.Size(20, 196);
            this._rightBtn.TabIndex = 3;
            this._rightBtn.Text = ">";
            this._rightBtn.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._rightBtn.Visible = false;
            // 
            // uiDataGridViewFooter1
            // 
           
            this.uiDataGridViewFooter1.DataGridView = this.uiDataGridView1;
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
            // uiDataGridView1
            // 
            this.uiDataGridView1.AllowUserToAddRows = false;
            this.uiDataGridView1.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.uiDataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.uiDataGridView1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.uiDataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.uiDataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.uiDataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.uiDataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.uiDataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(236)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.uiDataGridView1.DefaultCellStyle = dataGridViewCellStyle3;
            this.uiDataGridView1.EnableHeadersVisualStyles = false;
            this.uiDataGridView1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiDataGridView1.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(104)))), ((int)(((byte)(173)))), ((int)(((byte)(255)))));
            this.uiDataGridView1.Location = new System.Drawing.Point(15, 33);
            this.uiDataGridView1.Name = "uiDataGridView1";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.uiDataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.uiDataGridView1.RowHeadersVisible = false;
            this.uiDataGridView1.RowHeadersWidth = 51;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(236)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.uiDataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.uiDataGridView1.RowTemplate.Height = 23;
            this.uiDataGridView1.ScrollBarHandleWidth = 40;
            this.uiDataGridView1.ScrollBarRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.uiDataGridView1.SelectedIndex = -1;
            this.uiDataGridView1.Size = new System.Drawing.Size(1700, 800);
            this.uiDataGridView1.Style = Sunny.UI.UIStyle.Custom;
            this.uiDataGridView1.TabIndex = 6;
            // 
            // uiPagination1
            // 
            this.uiPagination1.ActivePage = 20;
            this.uiPagination1.CausesValidation = false;
            this.uiPagination1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.uiPagination1.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.uiPagination1.Location = new System.Drawing.Point(3, 924);
            this.uiPagination1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiPagination1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPagination1.Name = "uiPagination1";
            this.uiPagination1.PagerCount = 10;
            this.uiPagination1.PageSize = 5;
            this.uiPagination1.RadiusSides = Sunny.UI.UICornerRadiusSides.None;
            this.uiPagination1.RectSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.None;
            this.uiPagination1.ShowText = false;
            this.uiPagination1.Size = new System.Drawing.Size(1756, 35);
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
            this.dateTimePickerbegin.Location = new System.Drawing.Point(97, 33);
            this.dateTimePickerbegin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dateTimePickerbegin.MaxLength = 19;
            this.dateTimePickerbegin.MinimumSize = new System.Drawing.Size(63, 0);
            this.dateTimePickerbegin.Name = "dateTimePickerbegin";
            this.dateTimePickerbegin.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.dateTimePickerbegin.Size = new System.Drawing.Size(214, 34);
            this.dateTimePickerbegin.SymbolDropDown = 61555;
            this.dateTimePickerbegin.SymbolNormal = 61555;
            this.dateTimePickerbegin.TabIndex = 7;
            this.dateTimePickerbegin.Text = "2023-07-12 11:59:30";
            this.dateTimePickerbegin.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.dateTimePickerbegin.Value = new System.DateTime(2023, 7, 12, 11, 59, 30, 651);
            this.dateTimePickerbegin.Watermark = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(20, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 27);
            this.label1.TabIndex = 8;
            this.label1.Text = "开始";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(369, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 27);
            this.label2.TabIndex = 10;
            this.label2.Text = "结束";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dateTimePickerEnd
            // 
            this.dateTimePickerEnd.FillColor = System.Drawing.Color.White;
            this.dateTimePickerEnd.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dateTimePickerEnd.Location = new System.Drawing.Point(446, 33);
            this.dateTimePickerEnd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dateTimePickerEnd.MaxLength = 19;
            this.dateTimePickerEnd.MinimumSize = new System.Drawing.Size(63, 0);
            this.dateTimePickerEnd.Name = "dateTimePickerEnd";
            this.dateTimePickerEnd.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.dateTimePickerEnd.Size = new System.Drawing.Size(214, 34);
            this.dateTimePickerEnd.SymbolDropDown = 61555;
            this.dateTimePickerEnd.SymbolNormal = 61555;
            this.dateTimePickerEnd.TabIndex = 9;
            this.dateTimePickerEnd.Text = "2023-07-12 11:59:30";
            this.dateTimePickerEnd.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.dateTimePickerEnd.Value = new System.DateTime(2023, 7, 12, 11, 59, 30, 658);
            this.dateTimePickerEnd.Watermark = "";
            // 
            // buttonSearch
            // 
            this.buttonSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonSearch.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonSearch.Location = new System.Drawing.Point(752, 33);
            this.buttonSearch.MinimumSize = new System.Drawing.Size(1, 1);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(103, 34);
            this.buttonSearch.TabIndex = 11;
            this.buttonSearch.Text = "查询";
            this.buttonSearch.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.uiDataGridView1);
            this.groupBox1.Controls.Add(this.uiPagination1);
            this.groupBox1.Location = new System.Drawing.Point(28, 110);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1762, 962);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "结果展示";
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.groupBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
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
            // MainResultListPage
            // 
            this.AllowShowTitle = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.ClientSize = new System.Drawing.Size(1889, 1117);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this._rightBtn);
            this.Name = "MainResultListPage";
            this.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.PageIndex = 1003;
            this.ShowTitle = true;
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "结果";
            this.TitleFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            ((System.ComponentModel.ISupportInitialize)(this.uiDataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        private void _rightBtn_Click(object sender, System.EventArgs e)
        {
            FormMain.formMainF.showAsideFunc();
            this._rightBtn.Visible = false;
            SettingParamPage.instance.ShowRightBtnFunc(false);
            
            ProcessManagerPage.instance.ShowRightBtnFunc(false);    


        }

        #endregion

        private UIDataGridViewFooter uiDataGridViewFooter1;
        private UIPagination uiPagination1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private UIDataGridView uiDataGridView1;
        private UIDatetimePicker dateTimePickerbegin;
        private UILabel label1;
        private UILabel label2;
        private UIDatetimePicker dateTimePickerEnd;
        private UIButton buttonSearch;
        private UIButton _rightBtn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}