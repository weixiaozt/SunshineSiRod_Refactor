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

        private LiveCharts.WinForms.CartesianChart _cartesianRadiusChartResultView;
        private LiveCharts.WinForms.CartesianChart _cartesianYingLiChartResultView;
        private LiveCharts.WinForms.CartesianChart _cartesianResisvityChartResultView;

        public delegate void SwitchStartBtnState(bool bEnable);

        private UITabControl _tabMainLineControl;
        private System.Windows.Forms.TabPage _tabPageRadius;   //直径
        private System.Windows.Forms.TabPage _tabPageYingLi;  //应力
        private System.Windows.Forms.TabPage _tabPageResisvity;  //电阻率
        private System.Windows.Forms.TabPage _tabPageYinLiePictureView;  //透过率
        private System.Windows.Forms.TabPage _tabPageResultPicture; //结果图片
        private System.Windows.Forms.TabPage _tabPageResultFull;
        private System.Windows.Forms.Panel _panelRadiusResultFull;
        private System.Windows.Forms.Panel _panelResivityResutlFull;
        private System.Windows.Forms.Panel _panelYingliResultFull;
        public SwitchStartBtnState stateFunc;
        private InteractivePictureBox.PictureBoxEx pictureBoxResult;
        private InteractivePictureBox.PictureBoxEx pictureBoxYinlie;

        private InteractivePictureBox.PictureBoxEx pictureBoxFullYinLieResultView;

        private bool _bContinueState = false;
        private object _continuestatelock = new object();

        private UILabel _label;
        private UILabel _labelMinRadius;
        private UILabel _labelMaxRadius;
        private UILabel _labelLength;
        private UILabel _labelValidLength;
        private UILabel _labelGLLength;
        private UILabel _labelDJLength;
        private UILabel _labelWCLength;
        private UILabel _labelTotalLength;

        private UILabel _labelMinRadiusFullView;
        private UILabel _labelMaxRadiusFullView;
        private UILabel _labelLengthFullView;
        private UILabel _labelValidLengthFullView;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainResultLineInfoPage));
            this._tabMainLineControl = new Sunny.UI.UITabControl();
            this._tabPageRadius = new System.Windows.Forms.TabPage();
            this._labelMinRadius = new Sunny.UI.UILabel();
            this._labelMaxRadius = new Sunny.UI.UILabel();
            this._labelLength = new Sunny.UI.UILabel();
            this._labelValidLength = new Sunny.UI.UILabel();
            this._labelMinRadiusFullView = new Sunny.UI.UILabel();
            this._labelMaxRadiusFullView = new Sunny.UI.UILabel();
            this._labelLengthFullView = new Sunny.UI.UILabel();
            this._labelValidLengthFullView = new Sunny.UI.UILabel();
            this._labelGLLength = new UILabel();
            this._labelDJLength = new UILabel();
            this._labelWCLength = new UILabel();
            this._labelTotalLength = new UILabel();
            this._cartesianRadiusChart = new LiveCharts.WinForms.CartesianChart();
            this._tabPageYingLi = new System.Windows.Forms.TabPage();
            this._cartesianYingLiChart = new LiveCharts.WinForms.CartesianChart();
            this._tabPageResisvity = new System.Windows.Forms.TabPage();
            this._cartesianResisvityChart = new LiveCharts.WinForms.CartesianChart();
            this._tabPageYinLiePictureView = new System.Windows.Forms.TabPage();
            this.pictureBoxYinlie = new InteractivePictureBox.PictureBoxEx();
            this._tabPageResultPicture = new System.Windows.Forms.TabPage();
            this._tabPageResultFull = new System.Windows.Forms.TabPage();
            this._panelRadiusResultFull = new System.Windows.Forms.Panel();
            this._panelResivityResutlFull = new System.Windows.Forms.Panel();
            this._panelYingliResultFull = new System.Windows.Forms.Panel();
            this._cartesianRadiusChartResultView = new LiveCharts.WinForms.CartesianChart();
            this._cartesianResisvityChartResultView = new LiveCharts.WinForms.CartesianChart();
            this._cartesianYingLiChartResultView = new LiveCharts.WinForms.CartesianChart();

            this.pictureBoxResult = new InteractivePictureBox.PictureBoxEx();
            this.pictureBoxFullYinLieResultView = new InteractivePictureBox.PictureBoxEx();

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
            this._tabPageYinLiePictureView.SuspendLayout();
            this._tabPageResultFull.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYinlie)).BeginInit();
            this._tabPageResultPicture.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxResult)).BeginInit();
            this.SuspendLayout();
            // 
            // _tabMainLineControl
            // 
            //this._tabMainLineControl.Controls.Add(this._tabPageRadius);
            //this._tabMainLineControl.Controls.Add(this._tabPageYingLi);
            //this._tabMainLineControl.Controls.Add(this._tabPageResisvity);
            //this._tabMainLineControl.Controls.Add(this._tabPageYinLiePictureView);
            this._tabMainLineControl.Controls.Add(this._tabPageResultFull);
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
            // _labelMinRadius
            // 
            this._labelMinRadius.AutoSize = true;
            this._labelMinRadius.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._labelMinRadius.Location = new System.Drawing.Point(20, 5);
            this._labelMinRadius.Name = "_labelMinRadius";
            this._labelMinRadius.Size = new System.Drawing.Size(90, 21);
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
            this._labelMaxRadius.Size = new System.Drawing.Size(90, 21);
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
            this._labelLength.Size = new System.Drawing.Size(58, 21);
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
            this._labelValidLength.Size = new System.Drawing.Size(90, 21);
            this._labelValidLength.TabIndex = 0;
            this._labelValidLength.Text = "有效长度：";
            this._labelValidLength.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;


           
            // 
            // _cartesianRadiusChart
            // 
            this._cartesianRadiusChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cartesianRadiusChart.Location = new System.Drawing.Point(0, 0);
            this._cartesianRadiusChart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cartesianRadiusChart.Name = "_cartesianRadiusChart";
            this._cartesianRadiusChart.Size = new System.Drawing.Size(1780, 825);
            this._cartesianRadiusChart.TabIndex = 3;
            this._cartesianRadiusChart.Text = "cartesianRadiusChart1";
            // 
            // _tabPageYingLi
            // 
            this._tabPageYingLi.Controls.Add(this._cartesianYingLiChart);
            this._tabPageYingLi.Location = new System.Drawing.Point(0, 40);
            this._tabPageYingLi.Name = "_tabPageYingLi";
            this._tabPageYingLi.Size = new System.Drawing.Size(200, 60);
            this._tabPageYingLi.TabIndex = 1;
            this._tabPageYingLi.Text = "应力";
            // 
            // _cartesianYingLiChart
            // 
            this._cartesianYingLiChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cartesianYingLiChart.Location = new System.Drawing.Point(0, 0);
            this._cartesianYingLiChart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cartesianYingLiChart.Name = "_cartesianYingLiChart";
            this._cartesianYingLiChart.Size = new System.Drawing.Size(200, 60);
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
            this._tabPageYinLiePictureView.Controls.Add(this.pictureBoxYinlie);
            this._tabPageYinLiePictureView.Location = new System.Drawing.Point(0, 40);
            this._tabPageYinLiePictureView.Name = "_tabPagepenetrationRate";
            this._tabPageYinLiePictureView.Size = new System.Drawing.Size(200, 60);
            this._tabPageYinLiePictureView.TabIndex = 4;
            this._tabPageYinLiePictureView.Text = "隐裂";
            this._tabPageYinLiePictureView.UseVisualStyleBackColor = true;
            // 
            // pictureBoxYinlie
            // 
            this.pictureBoxYinlie.ArrLJListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxYinlie.ArrLJListArea")));
            this.pictureBoxYinlie.ArrRadiusListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxYinlie.ArrRadiusListArea")));
            this.pictureBoxYinlie.ArrYingLiListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxYinlie.ArrYingLiListArea")));
            this.pictureBoxYinlie.ArrYinLieListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxYinlie.ArrYinLieListArea")));
            this.pictureBoxYinlie.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYinlie.BRotate90 = false;
            this.pictureBoxYinlie.IsMoving = false;
            this.pictureBoxYinlie.Location = new System.Drawing.Point(20, 40);
            this.pictureBoxYinlie.Name = "pictureBoxYinlie";
            this.pictureBoxYinlie.NSelectLineIndex = -1;
            //this.pictureBoxYinlie.PreImage = null;
            this.pictureBoxYinlie.Size = new System.Drawing.Size(1800, 600);
            this.pictureBoxYinlie.TabIndex = 8;
            this.pictureBoxYinlie.TabStop = false;
            this.pictureBoxYinlie.UseInteract = true;
            // 
            // _tabPageResultPicture
            // 
            this._tabPageResultPicture.Controls.Add(this.pictureBoxResult);
            this._tabPageResultPicture.Location = new System.Drawing.Point(0, 40);
            this._tabPageResultPicture.Name = "_tabPageResultPicture";
            this._tabPageResultPicture.Size = new System.Drawing.Size(200, 60);
            this._tabPageResultPicture.TabIndex = 4;
            this._tabPageResultPicture.Text = "画线结果";
            this._tabPageResultPicture.UseVisualStyleBackColor = true;
            // 
            // pictureBoxResult
            // 
            this.pictureBoxResult.ArrLJListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxResult.ArrLJListArea")));
            this.pictureBoxResult.ArrRadiusListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxResult.ArrRadiusListArea")));
            this.pictureBoxResult.ArrYingLiListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxResult.ArrYingLiListArea")));
            this.pictureBoxResult.ArrYinLieListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxResult.ArrYinLieListArea")));
            this.pictureBoxResult.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxResult.BRotate90 = true;
            this.pictureBoxResult.IsMoving = false;
            this.pictureBoxResult.Location = new System.Drawing.Point(20, 40);
            this.pictureBoxResult.Name = "pictureBoxResult";
            this.pictureBoxResult.NSelectLineIndex = -1;
            this.pictureBoxResult.PreImage = null;
            this.pictureBoxResult.Size = new System.Drawing.Size(1800, 600);
            this.pictureBoxResult.TabIndex = 8;
            this.pictureBoxResult.TabStop = false;
            this.pictureBoxResult.UseInteract = true;





            // 
            // _cartesianRadiusChartResultView
            // 
            //this._cartesianRadiusChartResultView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cartesianRadiusChartResultView.Location = new System.Drawing.Point(0, 0);
            this._cartesianRadiusChartResultView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cartesianRadiusChartResultView.Name = "cartesianRadiusChartResultView";
            this._cartesianRadiusChartResultView.Size = new System.Drawing.Size(1780, 200);
            this._cartesianRadiusChartResultView.TabIndex = 3;
            this._cartesianRadiusChartResultView.Text = "cartesianRadiusChartResultView";

            this._panelRadiusResultFull.Controls.Add(_cartesianRadiusChartResultView);
            this._panelRadiusResultFull.Location = new System.Drawing.Point(20, 620);
            this._panelRadiusResultFull.Name = "_panelRadiusResultFull";
            this._panelRadiusResultFull.Size = new System.Drawing.Size(1780, 201);
            this._panelRadiusResultFull.TabIndex = 1;
            this._panelRadiusResultFull.Dock = System.Windows.Forms.DockStyle.None;
            // 
            // _cartesianResisvityChartResultView
            // 
            // this._cartesianResisvityChartResultView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cartesianResisvityChartResultView.Location = new System.Drawing.Point(0, 0);
            this._cartesianResisvityChartResultView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cartesianResisvityChartResultView.Name = "cartesianResisvityChartResultView";
            this._cartesianResisvityChartResultView.Size = new System.Drawing.Size(1780, 200);
            this._cartesianResisvityChartResultView.TabIndex = 3;
            this._cartesianResisvityChartResultView.Text = "cartesianResisvityChartResultView";

            this._panelResivityResutlFull.Controls.Add(_cartesianResisvityChartResultView);
            this._panelResivityResutlFull.Location = new System.Drawing.Point(20, 410);
            this._panelResivityResutlFull.Name = "_panelResivityResutlFull";
            this._panelResivityResutlFull.Size = new System.Drawing.Size(1780, 201);
            this._panelResivityResutlFull.TabIndex = 1;
            this._panelResivityResutlFull.Dock = System.Windows.Forms.DockStyle.None;

            // 
            // _cartesianYingLiChartResultView
            // 
            //this._cartesianYingLiChartResultView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cartesianYingLiChartResultView.Location = new System.Drawing.Point(0, 0);
            this._cartesianYingLiChartResultView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cartesianYingLiChartResultView.Name = "cartesianYingLiChartResultView";
            this._cartesianYingLiChartResultView.Size = new System.Drawing.Size(1780, 200);
            this._cartesianYingLiChartResultView.TabIndex = 3;
            this._cartesianYingLiChartResultView.Text = "cartesianYingLiChartResultView";

            this._panelYingliResultFull.Controls.Add(_cartesianYingLiChartResultView);
            this._panelYingliResultFull.Location = new System.Drawing.Point(20, 200);
            this._panelYingliResultFull.Name = "_panelYingliResultFull";
            this._panelYingliResultFull.Size = new System.Drawing.Size(1780, 201);
            this._panelYingliResultFull.TabIndex = 1;
            this._panelYingliResultFull.Dock = System.Windows.Forms.DockStyle.None;

            // 
            // pictureBoxResult
            // 
            //this.pictureBoxFullYinLieResultView.ArrLJListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxResult.ArrLJListArea")));
            //this.pictureBoxFullYinLieResultView.ArrRadiusListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxResult.ArrRadiusListArea")));
            //this.pictureBoxFullYinLieResultView.ArrYingLiListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxResult.ArrYingLiListArea")));
            //this.pictureBoxFullYinLieResultView.ArrYinLieListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxResult.ArrYinLieListArea")));
            this.pictureBoxFullYinLieResultView.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxFullYinLieResultView.BRotate90 = true;
            this.pictureBoxFullYinLieResultView.IsMoving = false;
            this.pictureBoxFullYinLieResultView.Location = new System.Drawing.Point(20, 40);
            this.pictureBoxFullYinLieResultView.Name = "pictureBoxFullYinLieResultView";
            this.pictureBoxFullYinLieResultView.NSelectLineIndex = -1;
            this.pictureBoxFullYinLieResultView.PreImage = null;
            this.pictureBoxFullYinLieResultView.Size = new System.Drawing.Size(1780, 150);
            this.pictureBoxFullYinLieResultView.TabIndex = 8;
            this.pictureBoxFullYinLieResultView.TabStop = false;
            this.pictureBoxFullYinLieResultView.UseInteract = true;
            this.pictureBoxFullYinLieResultView.OnMouseInteractive += PictureBoxFullYinLieResultView_OnMouseInteractive;

            // 
            // _labelMinRadiusFullView
            // 
            this._labelMinRadiusFullView.AutoSize = true;
            this._labelMinRadiusFullView.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._labelMinRadiusFullView.Location = new System.Drawing.Point(20, 5);
            this._labelMinRadiusFullView.Name = "_labelMinRadiusFullView";
            this._labelMinRadiusFullView.Size = new System.Drawing.Size(150, 21);
            this._labelMinRadiusFullView.TabIndex = 0;
            this._labelMinRadiusFullView.Text = "最小直径：";
            this._labelMinRadiusFullView.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _labelMaxRadiusFullView
            // 
            this._labelMaxRadiusFullView.AutoSize = true;
            this._labelMaxRadiusFullView.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._labelMaxRadiusFullView.Location = new System.Drawing.Point(190, 5);
            this._labelMaxRadiusFullView.Name = "_labelMaxRadiusFullView";
            this._labelMaxRadiusFullView.Size = new System.Drawing.Size(170, 21);
            this._labelMaxRadiusFullView.TabIndex = 0;
            this._labelMaxRadiusFullView.Text = "最大直径：";
            this._labelMaxRadiusFullView.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _labelLengthFullView
            // 
            this._labelLengthFullView.AutoSize = true;
            this._labelLengthFullView.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._labelLengthFullView.Location = new System.Drawing.Point(380, 5);
            this._labelLengthFullView.Name = "_labelLengthFullView";
            this._labelLengthFullView.Size = new System.Drawing.Size(120, 21);
            this._labelLengthFullView.TabIndex = 0;
            this._labelLengthFullView.Text = "长度：";
            this._labelLengthFullView.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _labelValidLengthFullView
            // 
            this._labelValidLengthFullView.AutoSize = true;
            this._labelValidLengthFullView.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._labelValidLengthFullView.Location = new System.Drawing.Point(520, 5);
            this._labelValidLengthFullView.Name = "_labelValidLengthFullView";
            this._labelValidLengthFullView.Size = new System.Drawing.Size(150, 21);
            this._labelValidLengthFullView.TabIndex = 0;
            this._labelValidLengthFullView.Text = "有效长度：";
            this._labelValidLengthFullView.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;



            // 
            // _labelValidLength
            // 
            this._labelGLLength.AutoSize = true;
            this._labelGLLength.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._labelGLLength.Location = new System.Drawing.Point(660, 5);
            this._labelGLLength.Name = "_labelGLLength";
            this._labelGLLength.Size = new System.Drawing.Size(90, 21);
            this._labelGLLength.TabIndex = 0;
            this._labelGLLength.Text = "鼓棱长度：";
            this._labelGLLength.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // _labelValidLength
            // 
            this._labelDJLength.AutoSize = true;
            this._labelDJLength.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._labelDJLength.Location = new System.Drawing.Point(800, 5);
            this._labelDJLength.Name = "_labelDJLength";
            this._labelDJLength.Size = new System.Drawing.Size(90, 21);
            this._labelDJLength.TabIndex = 0;
            this._labelDJLength.Text = "等径长度：";
            this._labelDJLength.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // _labelWCLength
            // 
            this._labelWCLength.AutoSize = true;
            this._labelWCLength.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._labelWCLength.Location = new System.Drawing.Point(940, 5);
            this._labelWCLength.Name = "_labelWCLength";
            this._labelWCLength.Size = new System.Drawing.Size(90, 21);
            this._labelWCLength.TabIndex = 0;
            this._labelWCLength.Text = "无位错长度：";
            this._labelWCLength.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // _labelTotalLength
            // 
            this._labelTotalLength.AutoSize = true;
            this._labelTotalLength.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._labelTotalLength.Location = new System.Drawing.Point(1080, 5);
            this._labelTotalLength.Name = "_labelTotalLength";
            this._labelTotalLength.Size = new System.Drawing.Size(90, 21);
            this._labelTotalLength.TabIndex = 0;
            this._labelTotalLength.Text = "总长：";
            this._labelTotalLength.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // _tabPageResultFull
            // 
            this._tabPageResultFull.Controls.Add(this.pictureBoxFullYinLieResultView);
            this._tabPageResultFull.Controls.Add(this._panelRadiusResultFull);
            this._tabPageResultFull.Controls.Add(this._panelResivityResutlFull);
            this._tabPageResultFull.Controls.Add(this._panelYingliResultFull);
            this._tabPageResultFull.Controls.Add(this._labelLengthFullView);
            this._tabPageResultFull.Controls.Add(this._labelValidLengthFullView);
            this._tabPageResultFull.Controls.Add(this._labelDJLength);
            this._tabPageResultFull.Controls.Add(this._labelGLLength);
            this._tabPageResultFull.Controls.Add(this._labelWCLength);
            this._tabPageResultFull.Controls.Add(this._labelTotalLength);
            this._tabPageResultFull.Controls.Add(this._labelMinRadiusFullView);
            this._tabPageResultFull.Controls.Add(this._labelMaxRadiusFullView);


            this._tabPageResultFull.Location = new System.Drawing.Point(0, 40);
            this._tabPageResultFull.Name = "_tabPageResultFull";
            this._tabPageResultFull.Size = new System.Drawing.Size(1800, 1000);
            this._tabPageResultFull.TabIndex = 4;
            this._tabPageResultFull.Text = "总体";
            this._tabPageResultFull.UseVisualStyleBackColor = true;
            //this._tabPageResultFull.Invalidated += _tabPageResultFull_Invalidated;

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
            // 
            // _btnStart
            // 
            this._btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnStart.Enabled = false;
            this._btnStart.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._btnStart.Location = new System.Drawing.Point(535, 40);
            this._btnStart.MinimumSize = new System.Drawing.Size(1, 1);
            this._btnStart.Name = "_btnStart";
            this._btnStart.Size = new System.Drawing.Size(100, 30);
            this._btnStart.TabIndex = 3;
            this._btnStart.Text = "继续";
            this._btnStart.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
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
            this.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.PageIndex = 1002;
            this.ShowTitle = true;
            this.Text = "";
            this.TitleFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this._tabMainLineControl.ResumeLayout(false);
            this._tabPageRadius.ResumeLayout(false);
            this._tabPageRadius.PerformLayout();
            this._tabPageYingLi.ResumeLayout(false);
            this._tabPageResisvity.ResumeLayout(false);
            this._tabPageResultFull.ResumeLayout(false);
            this._tabPageYinLiePictureView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYinlie)).EndInit();
            this._tabPageResultPicture.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxResult)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void PictureBoxFullYinLieResultView_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
           
        }


        private void PictureBoxYinlie_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
            
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