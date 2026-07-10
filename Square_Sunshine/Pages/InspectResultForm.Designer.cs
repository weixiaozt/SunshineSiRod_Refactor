using Sunny.UI;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace SquareSiliconStickCheck.Pages
{
    partial class InspectResultForm
    {
        [DllImport("user32", CharSet = CharSet.Unicode)] 
         private static extern int mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo); 
         //移动鼠标 
         const int MOUSEEVENTF_MOVE = 0x0001;      
         //模拟鼠标左键按下 
         const int MOUSEEVENTF_LEFTDOWN = 0x0002;


        private int _nNoCollapsedRadiusViewHeight = 850;
        private int _nCollapsedRadiusViewHeight = 50;
        private int _nSplitViewHeight = 10;
        private int _nBeginRaidusViewY = 40;
        private int _nRadiusViewWidth = 1700;
        private int _nRadiusViewPictureboxWidth = 1600;
        private int _nRadiusViewPictureboxHeight = 200;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InspectResultForm));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panelRadiusView_Full = new Sunny.UI.UITitlePanel();
            this.pictureBoxYINLIE = new InteractivePictureBox.PictureBoxEx();
            this.pictureBoxYingLi = new InteractivePictureBox.PictureBoxEx();
            this.groupBox2.SuspendLayout();
            this.panelRadiusView_Full.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYINLIE)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.panelRadiusView_Full);
            this.groupBox2.Location = new System.Drawing.Point(3, 38);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1790, 1080);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "隐裂、应力";
            // 
            // panelRadiusView_Full
            // 
            this.panelRadiusView_Full.Controls.Add(this.pictureBoxYINLIE);
            this.panelRadiusView_Full.Controls.Add(this.pictureBoxYingLi);
            this.panelRadiusView_Full.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.panelRadiusView_Full.Location = new System.Drawing.Point(16, 40);
            this.panelRadiusView_Full.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelRadiusView_Full.MinimumSize = new System.Drawing.Size(1, 1);
            this.panelRadiusView_Full.Name = "panelRadiusView_Full";
            this.panelRadiusView_Full.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.panelRadiusView_Full.ShowText = false;
            this.panelRadiusView_Full.Size = new System.Drawing.Size(1700, 850);
            this.panelRadiusView_Full.TabIndex = 9;
            this.panelRadiusView_Full.Text = "隐裂、应力图片";
            this.panelRadiusView_Full.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBoxYINLIE
            // 
            this.pictureBoxYINLIE.ArrLJListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxYINLIE.ArrLJListArea")));
            this.pictureBoxYINLIE.ArrRadiusListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxYINLIE.ArrRadiusListArea")));
            this.pictureBoxYINLIE.ArrYingLiListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxYINLIE.ArrYingLiListArea")));
            this.pictureBoxYINLIE.ArrYinLieListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxYINLIE.ArrYinLieListArea")));
            this.pictureBoxYINLIE.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYINLIE.BRotate90 = false;
            this.pictureBoxYINLIE.IsMoving = false;
            this.pictureBoxYINLIE.Location = new System.Drawing.Point(20, 40);
            this.pictureBoxYINLIE.Name = "pictureBoxYINLIE";
            this.pictureBoxYINLIE.NSelectLineIndex = -1;
            this.pictureBoxYINLIE.PreImage = null;
            this.pictureBoxYINLIE.Size = new System.Drawing.Size(1600, 400);
            this.pictureBoxYINLIE.TabIndex = 0;
            this.pictureBoxYINLIE.TabStop = false;
            this.pictureBoxYINLIE.UseInteract = true;
            // 
            // pictureBoxYingLi
            // 
            this.pictureBoxYingLi.ArrLJListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxYingLi.ArrLJListArea")));
            this.pictureBoxYingLi.ArrRadiusListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxYingLi.ArrRadiusListArea")));
            this.pictureBoxYingLi.ArrYingLiListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxYingLi.ArrYingLiListArea")));
            this.pictureBoxYingLi.ArrYinLieListArea = ((System.Collections.ArrayList)(resources.GetObject("pictureBoxYingLi.ArrYinLieListArea")));
            this.pictureBoxYingLi.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYingLi.BRotate90 = false;
            this.pictureBoxYingLi.IsMoving = false;
            this.pictureBoxYingLi.Location = new System.Drawing.Point(20, 460);
            this.pictureBoxYingLi.Name = "pictureBoxYingLi";
            this.pictureBoxYingLi.NSelectLineIndex = -1;
            this.pictureBoxYingLi.PreImage = null;
            this.pictureBoxYingLi.Size = new System.Drawing.Size(1600, 400);
            this.pictureBoxYingLi.TabIndex = 4;
            this.pictureBoxYingLi.TabStop = false;
            this.pictureBoxYingLi.UseInteract = true;
            // 
            // InspectResultForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1833, 1032);
            this.Controls.Add(this.groupBox2);
            this.Name = "InspectResultForm";
            this.Text = "视频";
            this.ZoomScaleRect = new System.Drawing.Rectangle(19, 19, 1655, 1050);
            this.groupBox2.ResumeLayout(false);
            this.panelRadiusView_Full.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYINLIE)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi)).EndInit();
            this.ResumeLayout(false);

        }

        private void InspectResultForm_Shown(object sender, System.EventArgs e)
        {
            if (this.panelRadiusView_Full.Size.Height > 40)
            {
                this.panelRadiusView_Full.Refresh();
            }
        }


        private void PictureBoxYINLIE_1_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
            this.panelRadiusView_Full.Refresh();
        }

        private void PictureBoxYingLi_1_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
            this.panelRadiusView_Full.Refresh();
        }


        #endregion
        private GroupBox  groupBox2;
       
       
    
        private UITitlePanel panelRadiusView_Full;
        private InteractivePictureBox.PictureBoxEx pictureBoxYINLIE;
        private InteractivePictureBox.PictureBoxEx pictureBoxYingLi;
    }
}