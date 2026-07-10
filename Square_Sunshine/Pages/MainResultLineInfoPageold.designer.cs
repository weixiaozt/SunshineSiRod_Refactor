using Sunny.UI;
using LiveCharts;
using System.Reflection.Emit;
using static SiliconRoundBarCheck.Pages.ProcessManagerPage;
using System.Runtime.InteropServices;
using System;
using SiliconRoundBarCheck.Parameters;

namespace SiliconRoundBarCheck.Pages
{
    partial class MainResultLineInfoPage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private LiveCharts.WinForms.CartesianChart _cartesianRadiusChart;
        private LiveCharts.WinForms.CartesianChart _cartesianYingLiChart;
        private LiveCharts.WinForms.CartesianChart _cartesianResisvityChart;
        private LiveCharts.WinForms.CartesianChart _cartesianPenetrationRateResultView;


        public delegate void SwitchStartBtnState(bool bEnable);

        private UITabControl _tabMainLineControl;
        private System.Windows.Forms.TabPage _tabPageRadius;   //直径
        private System.Windows.Forms.TabPage _tabPageYingLi;  //应力
        private System.Windows.Forms.TabPage _tabPageResisvity;  //电阻率
        private System.Windows.Forms.TabPage _tabPagepenetrationRate;  //透过率
        private System.Windows.Forms.TabPage _tabPageResultPicture; //结果图片
        public SwitchStartBtnState stateFunc;
        private InteractivePictureBox.PictureBoxEx pictureBoxResult;
        private bool _bContinueState = false;
        private object _continuestatelock = new object();

        private UILabel _label;
        private UILabel _labelMinRadius;
        private UILabel _labelMaxRadius;
        private UILabel _labelLength;
        private UILabel _labelValidLength;
        private UITextBox _siliconStickNumTextbox;
        private UIButton _btnSet;
        private UIButton _btnActivate;
        private UIButton _btnStart;
        private UIButton _rightBtn;

        public bool BContinueState
        {
            get
            {
                bool bState = false;
                lock (_continuestatelock)
                {
                    bState = _bContinueState;
                }
                return bState;
            }
            set 
            {
                lock (_continuestatelock)
                {
                    _bContinueState = value;
                }
            }
        
        }

        /// <summary
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
            this._tabMainLineControl = new Sunny.UI.UITabControl();
            this._tabPageRadius = new System.Windows.Forms.TabPage();
            this._cartesianRadiusChart = new LiveCharts.WinForms.CartesianChart();
            this._labelMinRadius = new Sunny.UI.UILabel();
            this._labelMaxRadius = new Sunny.UI.UILabel();
            this._labelLength = new Sunny.UI.UILabel();
            this.pictureBoxResult = new InteractivePictureBox.PictureBoxEx();
            this._labelValidLength = new Sunny.UI.UILabel();
            this._tabPageYingLi = new System.Windows.Forms.TabPage();
            this._cartesianYingLiChart = new LiveCharts.WinForms.CartesianChart();
            this._tabPageResisvity = new System.Windows.Forms.TabPage();
            this._cartesianResisvityChart = new LiveCharts.WinForms.CartesianChart();
            this._tabPagepenetrationRate = new System.Windows.Forms.TabPage();
            this._tabPageResultPicture = new System.Windows.Forms.TabPage();
            this._cartesianPenetrationRateResultView = new LiveCharts.WinForms.CartesianChart();
            this._label = new Sunny.UI.UILabel();
            this._btnSet = new Sunny.UI.UIButton();
            this._btnActivate = new Sunny.UI.UIButton();
            this._btnStart = new Sunny.UI.UIButton();
            this._rightBtn = new Sunny.UI.UIButton();
            this._siliconStickNumTextbox = new Sunny.UI.UITextBox();
            this._tabMainLineControl.SuspendLayout();
            this._tabPageRadius.SuspendLayout();
            this._tabPageYingLi.SuspendLayout();
            this._tabPageResisvity.SuspendLayout();
            this._tabPageResultPicture.SuspendLayout();
            this._tabPagepenetrationRate.SuspendLayout();
            this.SuspendLayout();
            // 
            // _tabMainLineControl
            // 
            this._tabMainLineControl.Controls.Add(this._tabPageRadius);
            this._tabMainLineControl.Controls.Add(this._tabPageYingLi);
            this._tabMainLineControl.Controls.Add(this._tabPageResisvity);
            this._tabMainLineControl.Controls.Add(this._tabPagepenetrationRate);
            this._tabMainLineControl.Controls.Add(this._tabPageResultPicture);
            this._tabMainLineControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this._tabMainLineControl.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._tabMainLineControl.ItemSize = new System.Drawing.Size(150, 40);
            this._tabMainLineControl.Location = new System.Drawing.Point(25, 85);
            this._tabMainLineControl.MainPage = "主页";
            this._tabMainLineControl.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            this._tabMainLineControl.Name = "_tabMainLineControl";
            this._tabMainLineControl.SelectedIndex = 0;
            this._tabMainLineControl.Size = new System.Drawing.Size(1780, 865);
            this._tabMainLineControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this._tabMainLineControl.TabIndex = 22;
            this._tabMainLineControl.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            
            // 
            // _cartesianRadiusChart
            // 
            this._cartesianRadiusChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cartesianRadiusChart.Location = new System.Drawing.Point(0, 35);
            this._cartesianRadiusChart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cartesianRadiusChart.Name = "_cartesianRadiusChart";
            this._cartesianRadiusChart.Size = new System.Drawing.Size(1780, 825);
            this._cartesianRadiusChart.TabIndex = 3;
            this._cartesianRadiusChart.Text = "cartesianRadiusChart1";
            // 
            // _labelMinRadius
            // 
            this._labelMinRadius.AutoSize = true;
            this._labelMinRadius.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._labelMinRadius.Location = new System.Drawing.Point(20, 5);
            this._labelMinRadius.Name = "_labelMinRadius";
            this._labelMinRadius.Size = new System.Drawing.Size(100, 25);
            this._labelMinRadius.TabIndex = 0;
            this._labelMinRadius.Text = "最小直径：";
            this._labelMinRadius.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _labelMaxRadius
            // 
            this._labelMaxRadius.AutoSize = true;
            this._labelMaxRadius.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._labelMaxRadius.Location = new System.Drawing.Point(180, 5);
            this._labelMaxRadius.Name = "_labelMaxRadius";
            this._labelMaxRadius.Size = new System.Drawing.Size(100, 25);
            this._labelMaxRadius.TabIndex = 0;
            this._labelMaxRadius.Text = "最大直径：";
            this._labelMaxRadius.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // _labelLength
            // 
            this._labelLength.AutoSize = true;
            this._labelLength.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._labelLength.Location = new System.Drawing.Point(340, 5);
            this._labelLength.Name = "_labelLength";
            this._labelLength.Size = new System.Drawing.Size(100, 25);
            this._labelLength.TabIndex = 0;
            this._labelLength.Text = "长度：";
            this._labelLength.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // _labelValidLength
            // 
            this._labelValidLength.AutoSize = true;
            this._labelValidLength.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._labelValidLength.Location = new System.Drawing.Point(500, 5);
            this._labelValidLength.Name = "_labelValidLength";
            this._labelValidLength.Size = new System.Drawing.Size(100, 25);
            this._labelValidLength.TabIndex = 0;
            this._labelValidLength.Text = "有效长度：";
            this._labelValidLength.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
           
            // 
            // _tabPageRadius
            // 
            this._tabPageRadius.Controls.Add(this._labelMinRadius);
            this._tabPageRadius.Controls.Add(this._labelMaxRadius);
            this._tabPageRadius.Controls.Add(this._labelLength);
            this._tabPageRadius.Controls.Add(this._labelValidLength);
            this._tabPageRadius.Controls.Add(this._cartesianRadiusChart);           
            this._tabPageRadius.Location = new System.Drawing.Point(0, 40);
            this._tabPageRadius.Name = "_tabPageRadius";
            this._tabPageRadius.Size = new System.Drawing.Size(1780, 825);
            this._tabPageRadius.TabIndex = 0;
            this._tabPageRadius.Text = "直径";

            // 
            // _tabPageYingLi
            // 
            this._tabPageYingLi.Controls.Add(this._cartesianYingLiChart);
            this._tabPageYingLi.Location = new System.Drawing.Point(0, 40);
            this._tabPageYingLi.Name = "_tabPageYingLi";
            this._tabPageYingLi.Size = new System.Drawing.Size(1780, 825);
            this._tabPageYingLi.TabIndex = 1;
            this._tabPageYingLi.Text = "应力";
            // 
            // _cartesianYingLiChart
            // 
            this._cartesianYingLiChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cartesianYingLiChart.Location = new System.Drawing.Point(0, 0);
            this._cartesianYingLiChart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cartesianYingLiChart.Name = "_cartesianYingLiChart";
            this._cartesianYingLiChart.Size = new System.Drawing.Size(1780, 825);
            this._cartesianYingLiChart.TabIndex = 3;
            this._cartesianYingLiChart.Text = "_cartesianYingLiChart";
            // 
            // _tabPageResisvity
            // 
            this._tabPageResisvity.Controls.Add(this._cartesianResisvityChart);
            this._tabPageResisvity.Location = new System.Drawing.Point(0, 40);
            this._tabPageResisvity.Name = "_tabPageResisvity";
            this._tabPageResisvity.Size = new System.Drawing.Size(200, 60);
            this._tabPageResisvity.TabIndex = 2;
            this._tabPageResisvity.Text = "电阻率";
            // 
            // _cartesianResisvityChart
            // 
            this._cartesianResisvityChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cartesianResisvityChart.Location = new System.Drawing.Point(0, 0);
            this._cartesianResisvityChart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cartesianResisvityChart.Name = "_cartesianResisvityChart";
            this._cartesianResisvityChart.Size = new System.Drawing.Size(200, 60);
            this._cartesianResisvityChart.TabIndex = 3;
            this._cartesianResisvityChart.Text = "cartesianResisvityChart";
            // 
            // _tabPagepenetrationRate
            // 
            this._tabPagepenetrationRate.Controls.Add(this._cartesianPenetrationRateResultView);
            this._tabPagepenetrationRate.Location = new System.Drawing.Point(0, 40);
            this._tabPagepenetrationRate.Name = "_tabPagepenetrationRate";
            this._tabPagepenetrationRate.Size = new System.Drawing.Size(200, 60);
            this._tabPagepenetrationRate.TabIndex = 4;
            this._tabPagepenetrationRate.Text = "透过率";
            this._tabPagepenetrationRate.UseVisualStyleBackColor = true;
            // 
            // _cartesianPenetrationRateResultView
            // 
            this._cartesianPenetrationRateResultView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cartesianPenetrationRateResultView.Location = new System.Drawing.Point(0, 0);
            this._cartesianPenetrationRateResultView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cartesianPenetrationRateResultView.Name = "_cartesianPenetrationRateResultView";
            this._cartesianPenetrationRateResultView.Size = new System.Drawing.Size(200, 60);
            this._cartesianPenetrationRateResultView.TabIndex = 3;
            this._cartesianPenetrationRateResultView.Text = "cartesianRadiusChart1";

            // 
            // _tabPagepenetrationRate
            // 
           
            this._tabPageResultPicture.Controls.Add(this.pictureBoxResult);
            this._tabPageResultPicture.Location = new System.Drawing.Point(0, 40);
            this._tabPageResultPicture.Name = "_tabPageResultPicture";
            this._tabPageResultPicture.Size = new System.Drawing.Size(200, 60);
            this._tabPageResultPicture.TabIndex = 4;
            this._tabPageResultPicture.Text = "结果图片";
            this._tabPageResultPicture.UseVisualStyleBackColor = true;


            // 
            // pictureBoxResult
            // 

            this.pictureBoxResult.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxResult.IsMoving = false;
            this.pictureBoxResult.Location = new System.Drawing.Point(20,40);
            this.pictureBoxResult.Name = "pictureBoxResult";
            this.pictureBoxResult.NSelectLineIndex = -1;
            this.pictureBoxResult.PreImage = null;
            this.pictureBoxResult.Size = new System.Drawing.Size(1800, 600);
            this.pictureBoxResult.TabIndex = 8;
            this.pictureBoxResult.TabStop = false;
            this.pictureBoxResult.BRotate90 = true;
            this.pictureBoxResult.UseInteract = true;
            this.pictureBoxResult.OnMouseInteractive += PictureBoxResult_OnMouseInteractive;
            // 
            // _label
            // 
            this._label.AutoSize = true;
            this._label.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._label.Location = new System.Drawing.Point(25, 40);
            this._label.Name = "_label";
            this._label.Size = new System.Drawing.Size(58, 21);
            this._label.TabIndex = 0;
            this._label.Text = "晶编：";
            this._label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _btnSet
            // 
            this._btnSet.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnSet.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._btnSet.Location = new System.Drawing.Point(305, 40);
            this._btnSet.MinimumSize = new System.Drawing.Size(1, 1);
            this._btnSet.Name = "_btnSet";
            this._btnSet.Size = new System.Drawing.Size(100, 30);
            this._btnSet.TabIndex = 0;
            this._btnSet.Text = "设置";
            this._btnSet.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._btnSet.Click += new System.EventHandler(this._btnSet_Click_1);
            // 
            // _btnActivate
            // 
            this._btnActivate.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnActivate.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._btnActivate.Location = new System.Drawing.Point(420, 40);
            this._btnActivate.MinimumSize = new System.Drawing.Size(1, 1);
            this._btnActivate.Name = "_btnActivate";
            this._btnActivate.Size = new System.Drawing.Size(100, 30);
            this._btnActivate.TabIndex = 2;
            this._btnActivate.Text = "激活";
            this._btnActivate.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._btnActivate.Click += _btnActivate_Click1;

            // 
            // _btnActivate
            // 
            this._btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnStart.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._btnStart.Location = new System.Drawing.Point(535, 40);
            this._btnStart.MinimumSize = new System.Drawing.Size(1, 1);
            this._btnStart.Name = "_btnStart";
            this._btnStart.Size = new System.Drawing.Size(100, 30);
            this._btnStart.TabIndex = 3;
            this._btnStart.Text = "继续";
            this._btnStart.Enabled = false;
            this._btnStart.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._btnStart.Click += _btnStart_Click;
            // 
            // _rightBtn
            // 
            this._rightBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this._rightBtn.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._rightBtn.Location = new System.Drawing.Point(0, 362);
            this._rightBtn.MinimumSize = new System.Drawing.Size(1, 1);
            this._rightBtn.Name = "_rightBtn";
            this._rightBtn.Size = new System.Drawing.Size(20, 196);
            this._rightBtn.TabIndex = 3;
            this._rightBtn.Text = ">";
            this._rightBtn.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._rightBtn.Visible = false;
            this._rightBtn.Click += _rightBtn_Click;
            // 
            // _siliconStickNumTextbox
            // 
            this._siliconStickNumTextbox.ButtonSymbolOffset = new System.Drawing.Point(0, 0);
            this._siliconStickNumTextbox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this._siliconStickNumTextbox.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._siliconStickNumTextbox.Location = new System.Drawing.Point(112, 40);
            this._siliconStickNumTextbox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this._siliconStickNumTextbox.MinimumSize = new System.Drawing.Size(1, 16);
            this._siliconStickNumTextbox.Name = "_siliconStickNumTextbox";
            this._siliconStickNumTextbox.Padding = new System.Windows.Forms.Padding(5);
            this._siliconStickNumTextbox.ShowText = false;
            this._siliconStickNumTextbox.Size = new System.Drawing.Size(150, 30);
            this._siliconStickNumTextbox.TabIndex = 1;
            this._siliconStickNumTextbox.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this._siliconStickNumTextbox.Watermark = "";
            // 
            // MainResultLineInfoPage
            // 
            this.AllowShowTitle = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1940, 956);
            this.Controls.Add(this._label);
            this.Controls.Add(this._siliconStickNumTextbox);
            this.Controls.Add(this._btnSet);
            this.Controls.Add(this._btnActivate);
            this.Controls.Add(this._btnStart);
            this.Controls.Add(this._tabMainLineControl);
            this.Controls.Add(this._rightBtn);
            this.Name = "MainResultLineInfoPage";
            this.FormClosing += MainResultLineInfoPage_FormClosing;
            this.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.PageIndex = 1002;
            this.ShowTitle = true;
            this.Text = "直径";
            this.TitleFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this._tabMainLineControl.ResumeLayout(false);
            this._tabPageRadius.ResumeLayout(false);
            this._tabPageYingLi.ResumeLayout(false);
            this._tabPageResisvity.ResumeLayout(false);
            this._tabPagepenetrationRate.ResumeLayout(false);
            this._tabPageResultPicture.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void PictureBoxResult_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
          
        }

        private void _btnStart_Click(object sender, System.EventArgs e)
        {
            BContinueState = true;
            this._btnStart.Enabled = false;
        }

        private void _btnActivate_Click1(object sender, System.EventArgs e)
        {
            this._btnSet.Enabled = true;
        }

      
        private void SetStartButtonState(bool bEnable)
        {
            this._btnStart.Enabled = bEnable;
        }




        #endregion
    }
}