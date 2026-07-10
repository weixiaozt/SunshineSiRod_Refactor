using Sunny.UI;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace SquareSiliconStickCheck.Pages
{
    partial class InspectSquareResultNewForm
    {
        [DllImport("user32", CharSet = CharSet.Unicode)] 
        private static extern int mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo); 
        //移动鼠标 
        const int MOUSEEVENTF_MOVE = 0x0001;      
        //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;

        private delegate void UpdateViewDele(int nType);


        private int _nSplitViewWidth = 10;
        private int _nRaidusViewHeight = 900;
        private int _nRadiusViewWidth = 600;
        private int _nViewBeginY = 50;
        private int _nRadiusViewPictureboxWidth = 150;
        private int _nRadiusViewPictureboxHeight = 900;
        private int _nNoCollapsedRadiusViewHeight = 900;
        private int _nNoCollapsedRadiusViewWidth = 700;
        private int _nCollapsedRadiusViewWidth = 50;
        private UpdateViewDele refreshViewFunc = null;

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
            this.labelRightAngle = new System.Windows.Forms.Label();
            this.labelLeftAngle = new System.Windows.Forms.Label();
            this.labelDownAngle = new System.Windows.Forms.Label();
            this.labelTopAngle = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelDownLeftDiag
            // 
            this.labelDownLeftDiag.AutoSize = true;
            this.labelDownLeftDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelDownLeftDiag.Location = new System.Drawing.Point(787, 848);
            this.labelDownLeftDiag.Name = "labelDownLeftDiag";
            this.labelDownLeftDiag.Size = new System.Drawing.Size(230, 31);
            this.labelDownLeftDiag.TabIndex = 49;
            this.labelDownLeftDiag.Text = "左侧弧左弧长投影：";
            // 
            // labelDownRightDiag
            // 
            this.labelDownRightDiag.AutoSize = true;
            this.labelDownRightDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelDownRightDiag.Location = new System.Drawing.Point(64, 921);
            this.labelDownRightDiag.Name = "labelDownRightDiag";
            this.labelDownRightDiag.Size = new System.Drawing.Size(230, 31);
            this.labelDownRightDiag.TabIndex = 48;
            this.labelDownRightDiag.Text = "左侧弧右弧长投影：";
            // 
            // labelRightLeftDiag
            // 
            this.labelRightLeftDiag.AutoSize = true;
            this.labelRightLeftDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRightLeftDiag.Location = new System.Drawing.Point(787, 711);
            this.labelRightLeftDiag.Name = "labelRightLeftDiag";
            this.labelRightLeftDiag.Size = new System.Drawing.Size(230, 31);
            this.labelRightLeftDiag.TabIndex = 47;
            this.labelRightLeftDiag.Text = "左侧弧左弧长投影：";
            // 
            // labelRightRightDiag
            // 
            this.labelRightRightDiag.AutoSize = true;
            this.labelRightRightDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRightRightDiag.Location = new System.Drawing.Point(64, 784);
            this.labelRightRightDiag.Name = "labelRightRightDiag";
            this.labelRightRightDiag.Size = new System.Drawing.Size(230, 31);
            this.labelRightRightDiag.TabIndex = 46;
            this.labelRightRightDiag.Text = "左侧弧右弧长投影：";
            // 
            // labelLeftLeftDiag
            // 
            this.labelLeftLeftDiag.AutoSize = true;
            this.labelLeftLeftDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLeftLeftDiag.Location = new System.Drawing.Point(787, 562);
            this.labelLeftLeftDiag.Name = "labelLeftLeftDiag";
            this.labelLeftLeftDiag.Size = new System.Drawing.Size(230, 31);
            this.labelLeftLeftDiag.TabIndex = 45;
            this.labelLeftLeftDiag.Text = "左侧弧左弧长投影：";
            // 
            // labelLeftRightDiag
            // 
            this.labelLeftRightDiag.AutoSize = true;
            this.labelLeftRightDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLeftRightDiag.Location = new System.Drawing.Point(64, 635);
            this.labelLeftRightDiag.Name = "labelLeftRightDiag";
            this.labelLeftRightDiag.Size = new System.Drawing.Size(230, 31);
            this.labelLeftRightDiag.TabIndex = 44;
            this.labelLeftRightDiag.Text = "左侧弧右弧长投影：";
            // 
            // labelTopLeftDiag
            // 
            this.labelTopLeftDiag.AutoSize = true;
            this.labelTopLeftDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTopLeftDiag.Location = new System.Drawing.Point(787, 423);
            this.labelTopLeftDiag.Name = "labelTopLeftDiag";
            this.labelTopLeftDiag.Size = new System.Drawing.Size(230, 31);
            this.labelTopLeftDiag.TabIndex = 43;
            this.labelTopLeftDiag.Text = "上侧弧左弧长投影：";
            // 
            // labelTopRightDiag
            // 
            this.labelTopRightDiag.AutoSize = true;
            this.labelTopRightDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTopRightDiag.Location = new System.Drawing.Point(64, 496);
            this.labelTopRightDiag.Name = "labelTopRightDiag";
            this.labelTopRightDiag.Size = new System.Drawing.Size(230, 31);
            this.labelTopRightDiag.TabIndex = 42;
            this.labelTopRightDiag.Text = "上侧弧右弧长投影：";
            // 
            // labelLength
            // 
            this.labelLength.AutoSize = true;
            this.labelLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLength.Location = new System.Drawing.Point(64, 96);
            this.labelLength.Name = "labelLength";
            this.labelLength.Size = new System.Drawing.Size(86, 31);
            this.labelLength.TabIndex = 41;
            this.labelLength.Text = "棒长：";
            // 
            // labelDownDiag
            // 
            this.labelDownDiag.AutoSize = true;
            this.labelDownDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelDownDiag.Location = new System.Drawing.Point(64, 848);
            this.labelDownDiag.Name = "labelDownDiag";
            this.labelDownDiag.Size = new System.Drawing.Size(134, 31);
            this.labelDownDiag.TabIndex = 40;
            this.labelDownDiag.Text = "下侧弧长：";
            // 
            // labelRightDiag
            // 
            this.labelRightDiag.AutoSize = true;
            this.labelRightDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRightDiag.Location = new System.Drawing.Point(64, 711);
            this.labelRightDiag.Name = "labelRightDiag";
            this.labelRightDiag.Size = new System.Drawing.Size(134, 31);
            this.labelRightDiag.TabIndex = 39;
            this.labelRightDiag.Text = "右侧弧长：";
            // 
            // labelLeftDiag
            // 
            this.labelLeftDiag.AutoSize = true;
            this.labelLeftDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLeftDiag.Location = new System.Drawing.Point(64, 562);
            this.labelLeftDiag.Name = "labelLeftDiag";
            this.labelLeftDiag.Size = new System.Drawing.Size(134, 31);
            this.labelLeftDiag.TabIndex = 38;
            this.labelLeftDiag.Text = "左侧弧长：";
            // 
            // labelTopDiag
            // 
            this.labelTopDiag.AutoSize = true;
            this.labelTopDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTopDiag.Location = new System.Drawing.Point(64, 423);
            this.labelTopDiag.Name = "labelTopDiag";
            this.labelTopDiag.Size = new System.Drawing.Size(134, 31);
            this.labelTopDiag.TabIndex = 37;
            this.labelTopDiag.Text = "上侧弧长：";
            // 
            // labelLRLength
            // 
            this.labelLRLength.AutoSize = true;
            this.labelLRLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLRLength.Location = new System.Drawing.Point(787, 349);
            this.labelLRLength.Name = "labelLRLength";
            this.labelLRLength.Size = new System.Drawing.Size(158, 31);
            this.labelLRLength.TabIndex = 36;
            this.labelLRLength.Text = "左右对角线：";
            // 
            // labelTDLength
            // 
            this.labelTDLength.AutoSize = true;
            this.labelTDLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTDLength.Location = new System.Drawing.Point(64, 349);
            this.labelTDLength.Name = "labelTDLength";
            this.labelTDLength.Size = new System.Drawing.Size(158, 31);
            this.labelTDLength.TabIndex = 35;
            this.labelTDLength.Text = "上下对角线：";
            // 
            // labelRDLength
            // 
            this.labelRDLength.AutoSize = true;
            this.labelRDLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRDLength.Location = new System.Drawing.Point(787, 269);
            this.labelRDLength.Name = "labelRDLength";
            this.labelRDLength.Size = new System.Drawing.Size(128, 31);
            this.labelRDLength.TabIndex = 34;
            this.labelRDLength.Text = "D面边长：";
            // 
            // labelLDLength
            // 
            this.labelLDLength.AutoSize = true;
            this.labelLDLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLDLength.Location = new System.Drawing.Point(64, 259);
            this.labelLDLength.Name = "labelLDLength";
            this.labelLDLength.Size = new System.Drawing.Size(126, 31);
            this.labelLDLength.TabIndex = 33;
            this.labelLDLength.Text = "C面边长：";
            // 
            // labelRTLength
            // 
            this.labelRTLength.AutoSize = true;
            this.labelRTLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRTLength.Location = new System.Drawing.Point(787, 180);
            this.labelRTLength.Name = "labelRTLength";
            this.labelRTLength.Size = new System.Drawing.Size(125, 31);
            this.labelRTLength.TabIndex = 32;
            this.labelRTLength.Text = "B面边长：";
            // 
            // labelLTLength
            // 
            this.labelLTLength.AutoSize = true;
            this.labelLTLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLTLength.Location = new System.Drawing.Point(64, 180);
            this.labelLTLength.Name = "labelLTLength";
            this.labelLTLength.Size = new System.Drawing.Size(127, 31);
            this.labelLTLength.TabIndex = 31;
            this.labelLTLength.Text = "A面边长：";
            // 
            // labelRightAngle
            // 
            this.labelRightAngle.AutoSize = true;
            this.labelRightAngle.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRightAngle.Location = new System.Drawing.Point(1466, 423);
            this.labelRightAngle.Name = "labelRightAngle";
            this.labelRightAngle.Size = new System.Drawing.Size(182, 31);
            this.labelRightAngle.TabIndex = 53;
            this.labelRightAngle.Text = "右侧直边角度：";
            // 
            // labelLeftAngle
            // 
            this.labelLeftAngle.AutoSize = true;
            this.labelLeftAngle.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLeftAngle.Location = new System.Drawing.Point(1466, 349);
            this.labelLeftAngle.Name = "labelLeftAngle";
            this.labelLeftAngle.Size = new System.Drawing.Size(182, 31);
            this.labelLeftAngle.TabIndex = 52;
            this.labelLeftAngle.Text = "左侧直边角度：";
            // 
            // labelDownAngle
            // 
            this.labelDownAngle.AutoSize = true;
            this.labelDownAngle.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelDownAngle.Location = new System.Drawing.Point(1466, 269);
            this.labelDownAngle.Name = "labelDownAngle";
            this.labelDownAngle.Size = new System.Drawing.Size(182, 31);
            this.labelDownAngle.TabIndex = 51;
            this.labelDownAngle.Text = "下侧直边角度：";
            // 
            // labelTopAngle
            // 
            this.labelTopAngle.AutoSize = true;
            this.labelTopAngle.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTopAngle.Location = new System.Drawing.Point(1466, 180);
            this.labelTopAngle.Name = "labelTopAngle";
            this.labelTopAngle.Size = new System.Drawing.Size(182, 31);
            this.labelTopAngle.TabIndex = 50;
            this.labelTopAngle.Text = "上侧直边角度：";
            // 
            // InspectSquareResultNewForm
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.ClientSize = new System.Drawing.Size(2166, 1270);
            this.Controls.Add(this.labelRightAngle);
            this.Controls.Add(this.labelLeftAngle);
            this.Controls.Add(this.labelDownAngle);
            this.Controls.Add(this.labelTopAngle);
            this.Controls.Add(this.labelDownLeftDiag);
            this.Controls.Add(this.labelDownRightDiag);
            this.Controls.Add(this.labelRightLeftDiag);
            this.Controls.Add(this.labelRightRightDiag);
            this.Controls.Add(this.labelLeftLeftDiag);
            this.Controls.Add(this.labelLeftRightDiag);
            this.Controls.Add(this.labelTopLeftDiag);
            this.Controls.Add(this.labelTopRightDiag);
            this.Controls.Add(this.labelLength);
            this.Controls.Add(this.labelDownDiag);
            this.Controls.Add(this.labelRightDiag);
            this.Controls.Add(this.labelLeftDiag);
            this.Controls.Add(this.labelTopDiag);
            this.Controls.Add(this.labelLRLength);
            this.Controls.Add(this.labelTDLength);
            this.Controls.Add(this.labelRDLength);
            this.Controls.Add(this.labelLDLength);
            this.Controls.Add(this.labelRTLength);
            this.Controls.Add(this.labelLTLength);
            this.MaximumSize = new System.Drawing.Size(2560, 1440);
            this.Name = "InspectSquareResultNewForm";
            this.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.ShowFullScreen = true;
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "检测结果";
            this.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.ZoomScaleRect = new System.Drawing.Rectangle(19, 19, 800, 480);
            this.ResumeLayout(false);
            this.PerformLayout();

        }







        #endregion

        private Label labelDownLeftDiag;
        private Label labelDownRightDiag;
        private Label labelRightLeftDiag;
        private Label labelRightRightDiag;
        private Label labelLeftLeftDiag;
        private Label labelLeftRightDiag;
        private Label labelTopLeftDiag;
        private Label labelTopRightDiag;
        private Label labelLength;
        private Label labelDownDiag;
        private Label labelRightDiag;
        private Label labelLeftDiag;
        private Label labelTopDiag;
        private Label labelLRLength;
        private Label labelTDLength;
        private Label labelRDLength;
        private Label labelLDLength;
        private Label labelRTLength;
        private Label labelLTLength;
        private Label labelRightAngle;
        private Label labelLeftAngle;
        private Label labelDownAngle;
        private Label labelTopAngle;
    }
}