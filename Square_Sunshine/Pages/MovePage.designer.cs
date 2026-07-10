using Sunny.UI;

namespace SquareSiliconStickCheck.Pages
{
    partial class MovePage
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
            this._rightBtn = new Sunny.UI.UIButton();
            this.uiGroupBox1 = new Sunny.UI.UIGroupBox();
            this.tbRotateLength = new System.Windows.Forms.TextBox();
            this.uiLabel3 = new Sunny.UI.UILabel();
            this.tbRotateDecreaseSpeed = new System.Windows.Forms.TextBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.tbRotateIncreaseSpeed = new System.Windows.Forms.TextBox();
            this.uiLabel42 = new Sunny.UI.UILabel();
            this.tbRotateSpeed = new System.Windows.Forms.TextBox();
            this.uiLabel30 = new Sunny.UI.UILabel();
            this.uiLabelCurPosition = new Sunny.UI.UILabel();
            this.btnStop = new Sunny.UI.UIButton();
            this.btnStart = new Sunny.UI.UIButton();
            this.comboBoxMoveDirection = new Sunny.UI.UIComboBox();
            this.uiLabel4 = new Sunny.UI.UILabel();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.btnSaveParameter = new Sunny.UI.UIButton();
            this.uiButtonClearPosition = new Sunny.UI.UIButton();
            this.uiGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rightBtn
            // 
            this._rightBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this._rightBtn.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._rightBtn.Location = new System.Drawing.Point(3, 343);
            this._rightBtn.MinimumSize = new System.Drawing.Size(1, 1);
            this._rightBtn.Name = "_rightBtn";
            this._rightBtn.Size = new System.Drawing.Size(20, 196);
            this._rightBtn.TabIndex = 3;
            this._rightBtn.Text = ">";
            this._rightBtn.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._rightBtn.Visible = false;
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.uiButtonClearPosition);
            this.uiGroupBox1.Controls.Add(this.tbRotateLength);
            this.uiGroupBox1.Controls.Add(this.uiLabel3);
            this.uiGroupBox1.Controls.Add(this.tbRotateDecreaseSpeed);
            this.uiGroupBox1.Controls.Add(this.uiLabel1);
            this.uiGroupBox1.Controls.Add(this.tbRotateIncreaseSpeed);
            this.uiGroupBox1.Controls.Add(this.uiLabel42);
            this.uiGroupBox1.Controls.Add(this.tbRotateSpeed);
            this.uiGroupBox1.Controls.Add(this.uiLabel30);
            this.uiGroupBox1.Controls.Add(this.uiLabelCurPosition);
            this.uiGroupBox1.Controls.Add(this.btnStop);
            this.uiGroupBox1.Controls.Add(this.btnStart);
            this.uiGroupBox1.Controls.Add(this.comboBoxMoveDirection);
            this.uiGroupBox1.Controls.Add(this.uiLabel4);
            this.uiGroupBox1.Controls.Add(this.uiLabel2);
            this.uiGroupBox1.Controls.Add(this.btnSaveParameter);
            this.uiGroupBox1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiGroupBox1.Location = new System.Drawing.Point(25, 51);
            this.uiGroupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Padding = new System.Windows.Forms.Padding(0, 32, 0, 0);
            this.uiGroupBox1.Size = new System.Drawing.Size(1537, 749);
            this.uiGroupBox1.TabIndex = 16;
            this.uiGroupBox1.TabStop = false;
            this.uiGroupBox1.Text = "运动";
            this.uiGroupBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbRotateLength
            // 
            this.tbRotateLength.Location = new System.Drawing.Point(462, 111);
            this.tbRotateLength.Name = "tbRotateLength";
            this.tbRotateLength.Size = new System.Drawing.Size(177, 29);
            this.tbRotateLength.TabIndex = 54;
            // 
            // uiLabel3
            // 
            this.uiLabel3.AutoSize = true;
            this.uiLabel3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel3.Location = new System.Drawing.Point(353, 111);
            this.uiLabel3.Name = "uiLabel3";
            this.uiLabel3.Size = new System.Drawing.Size(78, 21);
            this.uiLabel3.TabIndex = 53;
            this.uiLabel3.Text = "固定距离:";
            this.uiLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbRotateDecreaseSpeed
            // 
            this.tbRotateDecreaseSpeed.Location = new System.Drawing.Point(143, 111);
            this.tbRotateDecreaseSpeed.Name = "tbRotateDecreaseSpeed";
            this.tbRotateDecreaseSpeed.Size = new System.Drawing.Size(177, 29);
            this.tbRotateDecreaseSpeed.TabIndex = 52;
            // 
            // uiLabel1
            // 
            this.uiLabel1.AutoSize = true;
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel1.Location = new System.Drawing.Point(43, 111);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(94, 21);
            this.uiLabel1.TabIndex = 51;
            this.uiLabel1.Text = "旋转减速度:";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbRotateIncreaseSpeed
            // 
            this.tbRotateIncreaseSpeed.Location = new System.Drawing.Point(462, 50);
            this.tbRotateIncreaseSpeed.Name = "tbRotateIncreaseSpeed";
            this.tbRotateIncreaseSpeed.Size = new System.Drawing.Size(177, 29);
            this.tbRotateIncreaseSpeed.TabIndex = 50;
            // 
            // uiLabel42
            // 
            this.uiLabel42.AutoSize = true;
            this.uiLabel42.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel42.Location = new System.Drawing.Point(353, 50);
            this.uiLabel42.Name = "uiLabel42";
            this.uiLabel42.Size = new System.Drawing.Size(94, 21);
            this.uiLabel42.TabIndex = 49;
            this.uiLabel42.Text = "旋转加速度:";
            this.uiLabel42.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbRotateSpeed
            // 
            this.tbRotateSpeed.Location = new System.Drawing.Point(143, 50);
            this.tbRotateSpeed.Name = "tbRotateSpeed";
            this.tbRotateSpeed.Size = new System.Drawing.Size(177, 29);
            this.tbRotateSpeed.TabIndex = 48;
            // 
            // uiLabel30
            // 
            this.uiLabel30.AutoSize = true;
            this.uiLabel30.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel30.Location = new System.Drawing.Point(43, 53);
            this.uiLabel30.Name = "uiLabel30";
            this.uiLabel30.Size = new System.Drawing.Size(78, 21);
            this.uiLabel30.TabIndex = 47;
            this.uiLabel30.Text = "旋转速度:";
            this.uiLabel30.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabelCurPosition
            // 
            this.uiLabelCurPosition.AutoSize = true;
            this.uiLabelCurPosition.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabelCurPosition.Location = new System.Drawing.Point(458, 187);
            this.uiLabelCurPosition.Name = "uiLabelCurPosition";
            this.uiLabelCurPosition.Size = new System.Drawing.Size(19, 21);
            this.uiLabelCurPosition.TabIndex = 31;
            this.uiLabelCurPosition.Text = "0";
            this.uiLabelCurPosition.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnStop
            // 
            this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStop.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStop.Location = new System.Drawing.Point(251, 384);
            this.btnStop.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(103, 35);
            this.btnStop.TabIndex = 29;
            this.btnStop.Text = "停止";
            this.btnStop.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStart.Location = new System.Drawing.Point(56, 384);
            this.btnStart.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(103, 35);
            this.btnStart.TabIndex = 28;
            this.btnStart.Text = "开始";
            this.btnStart.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // comboBoxMoveDirection
            // 
            this.comboBoxMoveDirection.DataSource = null;
            this.comboBoxMoveDirection.FillColor = System.Drawing.Color.White;
            this.comboBoxMoveDirection.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBoxMoveDirection.FormattingEnabled = true;
            this.comboBoxMoveDirection.Items.AddRange(new object[] {
            "正向",
            "反向"});
            this.comboBoxMoveDirection.Location = new System.Drawing.Point(143, 187);
            this.comboBoxMoveDirection.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBoxMoveDirection.MinimumSize = new System.Drawing.Size(63, 0);
            this.comboBoxMoveDirection.Name = "comboBoxMoveDirection";
            this.comboBoxMoveDirection.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.comboBoxMoveDirection.Size = new System.Drawing.Size(177, 29);
            this.comboBoxMoveDirection.TabIndex = 25;
            this.comboBoxMoveDirection.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.comboBoxMoveDirection.Watermark = "";
            // 
            // uiLabel4
            // 
            this.uiLabel4.AutoSize = true;
            this.uiLabel4.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel4.Location = new System.Drawing.Point(45, 187);
            this.uiLabel4.Name = "uiLabel4";
            this.uiLabel4.Size = new System.Drawing.Size(46, 21);
            this.uiLabel4.TabIndex = 24;
            this.uiLabel4.Text = "方向:";
            this.uiLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel2
            // 
            this.uiLabel2.AutoSize = true;
            this.uiLabel2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel2.Location = new System.Drawing.Point(353, 187);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(78, 21);
            this.uiLabel2.TabIndex = 21;
            this.uiLabel2.Text = "当前位置:";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSaveParameter
            // 
            this.btnSaveParameter.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSaveParameter.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSaveParameter.Location = new System.Drawing.Point(453, 384);
            this.btnSaveParameter.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnSaveParameter.Name = "btnSaveParameter";
            this.btnSaveParameter.Size = new System.Drawing.Size(175, 35);
            this.btnSaveParameter.TabIndex = 30;
            this.btnSaveParameter.Text = "保存";
            this.btnSaveParameter.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            // 
            // uiButtonClearPosition
            // 
            this.uiButtonClearPosition.Cursor = System.Windows.Forms.Cursors.Hand;
            this.uiButtonClearPosition.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiButtonClearPosition.Location = new System.Drawing.Point(561, 187);
            this.uiButtonClearPosition.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiButtonClearPosition.Name = "uiButtonClearPosition";
            this.uiButtonClearPosition.Size = new System.Drawing.Size(103, 35);
            this.uiButtonClearPosition.TabIndex = 55;
            this.uiButtonClearPosition.Text = "清零";
            this.uiButtonClearPosition.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiButtonClearPosition.Click += new System.EventHandler(this.uiButtonClearPosition_Click);
            // 
            // MovePage
            // 
            this.AllowShowTitle = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1581, 805);
            this.Controls.Add(this.uiGroupBox1);
            this.Controls.Add(this._rightBtn);
            this.Name = "MovePage";
            this.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.PageIndex = 1004;
            this.ShowTitle = true;
            this.Text = "配置";
            this.TitleFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.uiGroupBox1.ResumeLayout(false);
            this.uiGroupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

       

        private void _rightBtn_Click(object sender, System.EventArgs e)
        {
            FormMain.formMainF.showAsideFunc();
            this._rightBtn.Visible = false;

            MovePage.instance.ShowRightBtnFunc(true);
            ProcessManagerPage.instance.ShowRightBtnFunc(false);
            SettingParamPage.instance.ShowRightBtnFunc(false);

        }

        #endregion

        private UIGroupBox uiGroupBox1;
        
        private UIButton _rightBtn;
        private UILabel uiLabel4;
        private UILabel uiLabel2;
        private UIComboBox comboBoxMoveDirection;
        private UIButton btnStop;
        private UIButton btnStart;
        private UIButton btnSaveParameter;
        private UILabel uiLabelCurPosition;
        private System.Windows.Forms.TextBox tbRotateLength;
        private UILabel uiLabel3;
        private System.Windows.Forms.TextBox tbRotateDecreaseSpeed;
        private UILabel uiLabel1;
        private System.Windows.Forms.TextBox tbRotateIncreaseSpeed;
        private UILabel uiLabel42;
        private System.Windows.Forms.TextBox tbRotateSpeed;
        private UILabel uiLabel30;
        private UIButton uiButtonClearPosition;
    }
}