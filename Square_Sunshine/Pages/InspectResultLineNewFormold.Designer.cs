using Sunny.UI;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using static SiliconRoundBarCheck.Pages.MainResultLineInfoPage;

namespace SiliconRoundBarCheck.Pages
{
    partial class InspectResultLineNewForm
    {
        //移动鼠标 
        const int MOUSEEVENTF_MOVE = 0x0001;      
        //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;

       


        private LiveCharts.WinForms.CartesianChart _cartesianRadiusChart;
        private LiveCharts.WinForms.CartesianChart _cartesianYingLiChart;
        private LiveCharts.WinForms.CartesianChart _cartesianResisvityChart;
        private LiveCharts.WinForms.CartesianChart _cartesianPenetrationRateResultView;

        private UITabControl _tabMainLineControl;
        private System.Windows.Forms.TabPage _tabPageRadius;   //直径
        private System.Windows.Forms.TabPage _tabPageYingLi;  //应力
        private System.Windows.Forms.TabPage _tabPageResisvity;  //电阻率
        private System.Windows.Forms.TabPage _tabPagepenetrationRate;  //透过率
        private System.Windows.Forms.TabPage _tabPageResultPicture; //结果图片
        private InteractivePictureBox.PictureBoxEx pictureBoxResult;

        private Sunny.UI.UILabel _labelMinRadius;
        private Sunny.UI.UILabel _labelMaxRadius;
        private Sunny.UI.UILabel _labelLength;
        private Sunny.UI.UILabel _labelValidLength;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InspectResultLineNewForm));
            this._tabMainLineControl = new Sunny.UI.UITabControl();
            this._tabPageRadius = new System.Windows.Forms.TabPage();
            this._cartesianRadiusChart = new LiveCharts.WinForms.CartesianChart();
            this._labelMinRadius = new Sunny.UI.UILabel();
            this._labelMaxRadius = new Sunny.UI.UILabel();
            this._labelLength = new Sunny.UI.UILabel();
            this._labelValidLength = new Sunny.UI.UILabel();
            this._tabPageYingLi = new System.Windows.Forms.TabPage();
            this._cartesianYingLiChart = new LiveCharts.WinForms.CartesianChart();
            this._tabPageResisvity = new System.Windows.Forms.TabPage();
            this._cartesianResisvityChart = new LiveCharts.WinForms.CartesianChart();
            this._tabPagepenetrationRate = new System.Windows.Forms.TabPage();
            this._cartesianPenetrationRateResultView = new LiveCharts.WinForms.CartesianChart();
            this._tabPageResultPicture = new System.Windows.Forms.TabPage();
            this.pictureBoxResult = new InteractivePictureBox.PictureBoxEx();
            this._tabMainLineControl.SuspendLayout();
            this._tabPageRadius.SuspendLayout();
            this._tabPageYingLi.SuspendLayout();
            this._tabPageResisvity.SuspendLayout();
            this._tabPagepenetrationRate.SuspendLayout();
            this._tabPageResultPicture.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxResult)).BeginInit();
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
            this._tabMainLineControl.Location = new System.Drawing.Point(10, 35);
            this._tabMainLineControl.MainPage = "主页";
            this._tabMainLineControl.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            this._tabMainLineControl.Name = "_tabMainLineControl";
            this._tabMainLineControl.SelectedIndex = 0;
            this._tabMainLineControl.Size = new System.Drawing.Size(1880, 960);
            this._tabMainLineControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this._tabMainLineControl.TabIndex = 22;
            this._tabMainLineControl.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            // 
            // _tabPageRadius
            // 
            this._tabPageRadius.Controls.Add(this._cartesianRadiusChart);
            this._tabPageRadius.Controls.Add(this._labelMinRadius);
            this._tabPageRadius.Controls.Add(this._labelMaxRadius);
            this._tabPageRadius.Controls.Add(this._labelLength);
            this._tabPageRadius.Controls.Add(this._labelValidLength);
            this._tabPageRadius.Location = new System.Drawing.Point(0, 40);
            this._tabPageRadius.Name = "_tabPageRadius";
            this._tabPageRadius.Size = new System.Drawing.Size(1880, 920);
            this._tabPageRadius.TabIndex = 0;
            this._tabPageRadius.Text = "直径";
            // 
            // _cartesianRadiusChart
            // 
            this._cartesianRadiusChart.Location = new System.Drawing.Point(0, 0);
            this._cartesianRadiusChart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cartesianRadiusChart.Name = "_cartesianRadiusChart";
            this._cartesianRadiusChart.Size = new System.Drawing.Size(1880, 920);
            this._cartesianRadiusChart.TabIndex = 3;
            this._cartesianRadiusChart.Text = "cartesianRadiusChart1";
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
            this._labelMaxRadius.Location = new System.Drawing.Point(177, 5);
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
            this._labelLength.Location = new System.Drawing.Point(327, 5);
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
            this._labelValidLength.Location = new System.Drawing.Point(477, 5);
            this._labelValidLength.Name = "_labelValidLength";
            this._labelValidLength.Size = new System.Drawing.Size(90, 21);
            this._labelValidLength.TabIndex = 0;
            this._labelValidLength.Text = "有效长度：";
            this._labelValidLength.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this._cartesianPenetrationRateResultView.Text = "_cartesianPenetrationRateResultView";
            // 
            // _tabPageResultPicture
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
            // InspectResultLineNewForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1900, 1000);
            this.Controls.Add(this._tabMainLineControl);
            this.MaximumSize = new System.Drawing.Size(1920, 1080);
            this.Name = "InspectResultLineNewForm";
            this.ShowFullScreen = true;
            this.Text = "结果展示";
            this.ZoomScaleRect = new System.Drawing.Rectangle(19, 19, 1655, 988);
            this._tabMainLineControl.ResumeLayout(false);
            this._tabPageRadius.ResumeLayout(false);
            this._tabPageRadius.PerformLayout();
            this._tabPageYingLi.ResumeLayout(false);
            this._tabPageResisvity.ResumeLayout(false);
            this._tabPagepenetrationRate.ResumeLayout(false);
            this._tabPageResultPicture.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxResult)).EndInit();
            this.ResumeLayout(false);

        }

        private void PictureBoxResult_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
            
        }




        #endregion




    }
}