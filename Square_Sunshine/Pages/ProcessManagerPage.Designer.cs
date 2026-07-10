using Sunny.UI;
using LiveCharts;
using LiveCharts.Defaults;
using System.Windows.Forms;
using SquareSiliconStickCheck.Tools;
using SquareSiliconStickCheck.Parameters;
using System.Drawing;
using System.Threading;
using System;
using System.Collections;
using SiliconRoundBarCheck.Tools;


namespace SquareSiliconStickCheck.Pages
{
    partial class ProcessManagerPage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
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
            this.panelRadiusView_Full = new Sunny.UI.UITitlePanel();
            this._cartesianChartResultLineLengthView = new LiveCharts.WinForms.CartesianChart();
            this._cartesianChartHypotenuseLengthResultView = new LiveCharts.WinForms.CartesianChart();
            this._cartesianFirstAngleChartResultView = new LiveCharts.WinForms.CartesianChart();
            this._cartesianSecondAngleChartResultView = new LiveCharts.WinForms.CartesianChart();
            this.panelRadiusViewFull_Full = new Sunny.UI.UITitlePanel();
            this.textBoxJB = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSave = new Sunny.UI.UIButton();
            this.buttonStart = new Sunny.UI.UIButton();
            this.buttonStop = new Sunny.UI.UIButton();
            this._rightBtn = new Sunny.UI.UIButton();
            this._tabMainLineControl = new Sunny.UI.UITabControl();
            this._tabPageLegnth = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonStartScan = new Sunny.UI.UIButton();
            this.panelRadiusViewFull_Full.SuspendLayout();
            this._tabMainLineControl.SuspendLayout();
            this._tabPageLegnth.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelRadiusView_Full
            // 
            this.panelRadiusView_Full.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.panelRadiusView_Full.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.panelRadiusView_Full.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.panelRadiusView_Full.Location = new System.Drawing.Point(15, 83);
            this.panelRadiusView_Full.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelRadiusView_Full.MinimumSize = new System.Drawing.Size(1, 1);
            this.panelRadiusView_Full.Name = "panelRadiusView_Full";
            this.panelRadiusView_Full.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.panelRadiusView_Full.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.panelRadiusView_Full.ShowCollapse = true;
            this.panelRadiusView_Full.ShowText = false;
            this.panelRadiusView_Full.Size = new System.Drawing.Size(1760, 1050);
            this.panelRadiusView_Full.Style = Sunny.UI.UIStyle.Custom;
            this.panelRadiusView_Full.TabIndex = 9;
            this.panelRadiusView_Full.Text = "数据";
            this.panelRadiusView_Full.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.panelRadiusView_Full.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            // 
            // _cartesianChartResultLineLengthView
            // 
            this._cartesianChartResultLineLengthView.Location = new System.Drawing.Point(10, 64);
            this._cartesianChartResultLineLengthView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cartesianChartResultLineLengthView.Name = "_cartesianChartResultLineLengthView";
            this._cartesianChartResultLineLengthView.Size = new System.Drawing.Size(1780, 194);
            this._cartesianChartResultLineLengthView.TabIndex = 3;
            this._cartesianChartResultLineLengthView.Text = "左边3D数据";
            // 
            // _cartesianChartHypotenuseLengthResultView
            // 
            this._cartesianChartHypotenuseLengthResultView.Location = new System.Drawing.Point(10, 261);
            this._cartesianChartHypotenuseLengthResultView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cartesianChartHypotenuseLengthResultView.Name = "_cartesianChartHypotenuseLengthResultView";
            this._cartesianChartHypotenuseLengthResultView.Size = new System.Drawing.Size(1780, 184);
            this._cartesianChartHypotenuseLengthResultView.TabIndex = 3;
            this._cartesianChartHypotenuseLengthResultView.Text = "上边3D数据";
            // 
            // _cartesianFirstAngleChartResultView
            // 
            this._cartesianFirstAngleChartResultView.Location = new System.Drawing.Point(10, 442);
            this._cartesianFirstAngleChartResultView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cartesianFirstAngleChartResultView.Name = "_cartesianFirstAngleChartResultView";
            this._cartesianFirstAngleChartResultView.Size = new System.Drawing.Size(1780, 200);
            this._cartesianFirstAngleChartResultView.TabIndex = 3;
            this._cartesianFirstAngleChartResultView.Text = "右边3D数据";
            // 
            // _cartesianSecondAngleChartResultView
            // 
            this._cartesianSecondAngleChartResultView.Location = new System.Drawing.Point(10, 641);
            this._cartesianSecondAngleChartResultView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cartesianSecondAngleChartResultView.Name = "_cartesianSecondAngleChartResultView";
            this._cartesianSecondAngleChartResultView.Size = new System.Drawing.Size(1780, 200);
            this._cartesianSecondAngleChartResultView.TabIndex = 3;
            this._cartesianSecondAngleChartResultView.Text = "下边3D数据";
            // 
            // panelRadiusViewFull_Full
            // 
            this.panelRadiusViewFull_Full.Controls.Add(this._cartesianChartResultLineLengthView);
            this.panelRadiusViewFull_Full.Controls.Add(this._cartesianChartHypotenuseLengthResultView);
            this.panelRadiusViewFull_Full.Controls.Add(this._cartesianFirstAngleChartResultView);
            this.panelRadiusViewFull_Full.Controls.Add(this._cartesianSecondAngleChartResultView);
            this.panelRadiusViewFull_Full.Controls.Add(this.textBoxJB);
            this.panelRadiusViewFull_Full.Controls.Add(this.label1);
            this.panelRadiusViewFull_Full.Controls.Add(this.buttonSave);
            this.panelRadiusViewFull_Full.Controls.Add(this.buttonStart);
            this.panelRadiusViewFull_Full.Controls.Add(this.buttonStop);
            this.panelRadiusViewFull_Full.Controls.Add(this._rightBtn);
            this.panelRadiusViewFull_Full.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.panelRadiusViewFull_Full.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.panelRadiusViewFull_Full.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.panelRadiusViewFull_Full.Location = new System.Drawing.Point(0, 0);
            this.panelRadiusViewFull_Full.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelRadiusViewFull_Full.MinimumSize = new System.Drawing.Size(1, 1);
            this.panelRadiusViewFull_Full.Name = "panelRadiusViewFull_Full";
            this.panelRadiusViewFull_Full.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.panelRadiusViewFull_Full.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.panelRadiusViewFull_Full.ShowText = false;
            this.panelRadiusViewFull_Full.Size = new System.Drawing.Size(1780, 1050);
            this.panelRadiusViewFull_Full.Style = Sunny.UI.UIStyle.Custom;
            this.panelRadiusViewFull_Full.TabIndex = 0;
            this.panelRadiusViewFull_Full.Text = null;
            this.panelRadiusViewFull_Full.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.panelRadiusViewFull_Full.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            // 
            // textBoxJB
            // 
            this.textBoxJB.Location = new System.Drawing.Point(134, 18);
            this.textBoxJB.Name = "textBoxJB";
            this.textBoxJB.Size = new System.Drawing.Size(188, 34);
            this.textBoxJB.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(45, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 27);
            this.label1.TabIndex = 10;
            this.label1.Text = "晶编：";
            // 
            // buttonSave
            // 
            this.buttonSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this.buttonSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonSave.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonSave.Location = new System.Drawing.Point(799, 18);
            this.buttonSave.MinimumSize = new System.Drawing.Size(1, 1);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(90, 30);
            this.buttonSave.Style = Sunny.UI.UIStyle.Custom;
            this.buttonSave.TabIndex = 0;
            this.buttonSave.Text = "保存";
            this.buttonSave.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            // 
            // buttonStart
            // 
            this.buttonStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this.buttonStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonStart.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonStart.Location = new System.Drawing.Point(476, 18);
            this.buttonStart.MinimumSize = new System.Drawing.Size(1, 1);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(90, 30);
            this.buttonStart.Style = Sunny.UI.UIStyle.Custom;
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "开始";
            this.buttonStart.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this.buttonStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonStop.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonStop.Location = new System.Drawing.Point(632, 18);
            this.buttonStop.MinimumSize = new System.Drawing.Size(1, 1);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(90, 30);
            this.buttonStop.Style = Sunny.UI.UIStyle.Custom;
            this.buttonStop.TabIndex = 0;
            this.buttonStop.Text = "停止";
            this.buttonStop.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            // 
            // _rightBtn
            // 
            this._rightBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this._rightBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this._rightBtn.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._rightBtn.Location = new System.Drawing.Point(0, 362);
            this._rightBtn.MinimumSize = new System.Drawing.Size(1, 1);
            this._rightBtn.Name = "_rightBtn";
            this._rightBtn.Size = new System.Drawing.Size(20, 196);
            this._rightBtn.Style = Sunny.UI.UIStyle.Custom;
            this._rightBtn.TabIndex = 3;
            this._rightBtn.Text = ">";
            this._rightBtn.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._rightBtn.Visible = false;
            // 
            // _tabMainLineControl
            // 
            this._tabMainLineControl.Controls.Add(this._tabPageLegnth);
            this._tabMainLineControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this._tabMainLineControl.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._tabMainLineControl.ItemSize = new System.Drawing.Size(150, 40);
            this._tabMainLineControl.Location = new System.Drawing.Point(25, 85);
            this._tabMainLineControl.MainPage = "主页";
            this._tabMainLineControl.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            this._tabMainLineControl.Name = "_tabMainLineControl";
            this._tabMainLineControl.SelectedIndex = 0;
            this._tabMainLineControl.Size = new System.Drawing.Size(1780, 1050);
            this._tabMainLineControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this._tabMainLineControl.Style = Sunny.UI.UIStyle.Custom;
            this._tabMainLineControl.TabIndex = 22;
            this._tabMainLineControl.TabSelectedForeColor = System.Drawing.Color.White;
            this._tabMainLineControl.TabSelectedHighColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this._tabMainLineControl.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            // 
            // _tabPageLegnth
            // 
            this._tabPageLegnth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this._tabPageLegnth.Controls.Add(this.panelRadiusViewFull_Full);
            this._tabPageLegnth.Location = new System.Drawing.Point(0, 40);
            this._tabPageLegnth.Name = "_tabPageLegnth";
            this._tabPageLegnth.Size = new System.Drawing.Size(1780, 1010);
            this._tabPageLegnth.TabIndex = 4;
            this._tabPageLegnth.Text = "测试流程";
            this._tabPageLegnth.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 100);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // buttonStartScan
            // 
            this.buttonStartScan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonStartScan.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonStartScan.Location = new System.Drawing.Point(1425, 687);
            this.buttonStartScan.MinimumSize = new System.Drawing.Size(1, 1);
            this.buttonStartScan.Name = "buttonStartScan";
            this.buttonStartScan.Size = new System.Drawing.Size(90, 30);
            this.buttonStartScan.TabIndex = 0;
            this.buttonStartScan.Text = "开始扫描";
            this.buttonStartScan.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonStartScan.Visible = false;
            this.buttonStartScan.Click += new System.EventHandler(this.ButtonStartScan_Click);
            // 
            // ProcessManagerPage
            // 
            this.AllowShowTitle = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.ClientSize = new System.Drawing.Size(2068, 1103);
            this.Controls.Add(this._tabMainLineControl);
            this.Name = "ProcessManagerPage";
            this.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.PageIndex = 1002;
            this.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.ShowTitle = true;
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "检测流程";
            this.TitleFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProcessManagerPage_FormClosing);
            this.panelRadiusViewFull_Full.ResumeLayout(false);
            this.panelRadiusViewFull_Full.PerformLayout();
            this._tabMainLineControl.ResumeLayout(false);
            this._tabPageLegnth.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void ProcessManagerPage_Shown(object sender, EventArgs e)
        {
            if (this.Visible == true)
            {
                if (true == panelRadiusView_Full.Visible)
                {
                    panelRadiusView_Full.Refresh();
                }
            }
            
        }

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            if (SettingParameter.Instance().NDaemon == 1)
            {
                Stop();
            }
        }

        private void ProcessManagerPage_FormClosing1(object sender, FormClosingEventArgs e)
        {
            if (SettingParameter.Instance().NDaemon == 1)
            {
                Stop();
            }
        }

        private void ProcessManagerPage_SizeChanged(object sender, EventArgs e)
        {
            Refresh();
            Invalidate();
        }

        private void _rightBtn_Click(object sender, EventArgs e)
        {
            FormMain.formMainF.showAsideFunc();
            this._rightBtn.Visible = false;

            SettingParamPage.instance.ShowRightBtnFunc(false);
            //MainResultListPage.instance.ShowRightBtnFunc(false);
            //MainStatisticPage.instance.ShowRightBtnFunc(false);

        }

     

        private void ButtonTotal_Click(object sender, EventArgs e)
        {
            
        }



        public enum emResultType
        {
            EM_RESULTRADIUS = 0,
            EM_RESULTYINGLI = 1,
            EM_RESULTYINLIE = 2,
            EM_RESULTLJ = 3,
            EM_RESULTTOTAL = 4,
        };

        private void drawLine(PictureBox ob, PaintEventArgs e, int nIndex, int nType)
        {
            if (ob.Image == null)
            {
                return;
            }
            Graphics g = e.Graphics;
            Pen myPen = new Pen(Color.DeepPink, 5);
            int nHeight = ob.Image.Height;

            ArrayList infoArray = null;
            switch (nType)
            {
                case (int)emResultType.EM_RESULTYINGLI:
                    {
                        infoArray = _subYingLiPictureIndexInfo[nIndex];
                        break;
                    }
                case (int)emResultType.EM_RESULTYINLIE:
                    {
                        infoArray = _subYinLiePictureIndexInfo[nIndex];
                        break;
                    }
                case (int)emResultType.EM_RESULTLJ:
                    {
                        infoArray = _subLJPictureIndexInfo[nIndex];
                        break;
                    }
                case (int)emResultType.EM_RESULTTOTAL:
                    {
                        infoArray = _subTotalPictureIndexInfo;
                        break;
                    }
                default:
                    {
                        infoArray = _subYingLiPictureIndexInfo[nIndex];
                        break;
                    }

            }
           


            for (int i = 0; i < infoArray.Count; i++)
            {
                int[] nArray = (int[])infoArray[i];

                g.DrawLine(myPen, new Point(nArray[0], 0), new Point(nArray[0], nHeight));
                g.DrawLine(myPen, new Point(nArray[1], 0), new Point(nArray[1], nHeight));
                g.FillRectangle(new SolidBrush(Color.FromArgb(125, Color.Cyan)), new Rectangle(nArray[0], 0, (nArray[1] - nArray[0]), nHeight));

            }


        }




      

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            
        }

        private void ButtonStartScanStick_Click(object sender, System.EventArgs e)
        {
            
            ProcessManager.Instance().ScanSquareSiliconStickRound(_strSiliconSerialNum);


        }
        private void ReleaseCameras()
        {
             LogHelper.Info("Silicon","ReleaseCameras begin");
            try
            {
               

              


            }
            catch (Exception ex)
            {
                 LogHelper.Info("Silicon","ReleaseCameras Exception " + ex.Message);
            }

             LogHelper.Info("Silicon","ReleaseCameras End");
        }

       

       


        public enum emCameraType
        {
            EM_HIKVIDEO = 0,
            EM_HIKLJ = 1,
            EM_BVYINLIE = 2,
            EM_BVYINGLI = 3
        };

        private void InitDevFunction(int nType)
        {
            switch(nType)
            {
               
                case (int)emCameraType.EM_BVYINLIE:
                    {
                       
                        break;
                    }
                case (int)emCameraType.EM_BVYINGLI:
                    {
                        
                        break;
                    }
            }
        }


        private void StopCameras()
        {
            try
            {
                
               
            }
            catch(Exception ex)
            {
                 LogHelper.Info("Silicon","StopCameras exception " + ex.Message);
            }
           
        }

        private bool InitCameras()
        {
             LogHelper.Info("Silicon","InitCameras Begin");


            

            LogHelper.Info("Silicon","InitCameras End");
            return true;
        }

        
        private void ButtonStartScan_Click(object sender, System.EventArgs e)
        {

            ProcessManager.Instance().InitMatResource();
            m_threadRefreshVideoView = new Thread(UpdateSquareInfoChartView);
            m_threadRefreshVideoView.Start();

         

        }

        #endregion

     
       
        private System.Windows.Forms.Timer Timer { get; set; }
       
        private UIButton buttonStart;
        private UIButton buttonStop;
        private UIButton buttonStartScan;
     
        private UIButton _rightBtn;
        private GroupBox groupBox1;
        private GroupBox groupBox2;


        private UITabControl _tabMainLineControl;
        private TabPage _tabPageLegnth;   //直径
        private System.Windows.Forms.TabPage _tabPageAngle;  //应力

        private UITitlePanel panelRadiusView_Full;
        private UITitlePanel panelRadiusViewFull_Full;
        private LiveCharts.WinForms.CartesianChart _cartesianChartResultLineLengthView;
        private LiveCharts.WinForms.CartesianChart _cartesianChartHypotenuseLengthResultView;
        private LiveCharts.WinForms.CartesianChart _cartesianFirstAngleChartResultView;
        private LiveCharts.WinForms.CartesianChart _cartesianSecondAngleChartResultView;


        private UIButton buttonSave;
        private UIButton buttonTotal;

       
        private int _nNoCollapsedRadiusViewHeight;
        private int _nCollapsedRadiusViewHeight;
        private int _nBeginRaidusViewY;
        private int _nRadiusViewWidth;
        private int _nSplitViewHeight;
        private int _nRadiusViewPictureboxWidth;
        private int _nRadiusViewPictureboxHeight;
        private int _nRadiusViewResultPictureboxWidth;
        private int _nRadiusViewResultPictureboxHeight;
        private int _nMainPanelViewWidth;
        private int _nMainPanelViewHeight;





        private bool _bDragging = false;
        private int _nDragLineIndex = 0;
        private Label label1;
        private TextBox textBoxJB;
    }
}