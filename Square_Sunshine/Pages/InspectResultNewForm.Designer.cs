using Sunny.UI;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace SquareSiliconStickCheck.Pages
{
    partial class InspectResultNewForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InspectResultNewForm));
            this.panelRadiusView_YINLIE = new Sunny.UI.UITitleTypePanel();
            this.pictureBoxYINLIE_4 = new InteractivePictureBox.PictureBoxEx();
            this.pictureBoxYINLIE_3 = new InteractivePictureBox.PictureBoxEx();
            this.pictureBoxYINLIE_2 = new InteractivePictureBox.PictureBoxEx();
            this.pictureBoxYINLIE_1 = new InteractivePictureBox.PictureBoxEx();
            this.panelRadiusView_LJ = new Sunny.UI.UITitleTypePanel();
            this.pictureBoxLJ_1 = new InteractivePictureBox.PictureBoxEx();
            this.pictureBoxLJ_2 = new InteractivePictureBox.PictureBoxEx();
            this.pictureBoxLJ_3 = new InteractivePictureBox.PictureBoxEx();
            this.pictureBoxLJ_4 = new InteractivePictureBox.PictureBoxEx();
            this.panelRadiusView_YingLi = new Sunny.UI.UITitleTypePanel();
            this.pictureBoxYingLi_4 = new InteractivePictureBox.PictureBoxEx();
            this.pictureBoxYingLi_3 = new InteractivePictureBox.PictureBoxEx();
            this.pictureBoxYingLi_2 = new InteractivePictureBox.PictureBoxEx();
            this.pictureBoxYingLi_1 = new InteractivePictureBox.PictureBoxEx();
           
           
            this.panelRadiusView_YINLIE.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYINLIE_4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYINLIE_3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYINLIE_2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYINLIE_1)).BeginInit();
            this.panelRadiusView_LJ.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLJ_1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLJ_2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLJ_3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLJ_4)).BeginInit();

            this.panelRadiusView_YingLi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi_4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi_3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi_2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi_1)).BeginInit();
         
            
            this.SuspendLayout();
            // 
            // panelRadiusView_YINLIE
            // 
            this.panelRadiusView_YINLIE.Controls.Add(this.pictureBoxYINLIE_1);
            this.panelRadiusView_YINLIE.Controls.Add(this.pictureBoxYINLIE_2);
            this.panelRadiusView_YINLIE.Controls.Add(this.pictureBoxYINLIE_3);
            this.panelRadiusView_YINLIE.Controls.Add(this.pictureBoxYINLIE_4);
           
            // 
            // pictureBoxYINLIE_4
            // 
            
            this.pictureBoxYINLIE_4.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYINLIE_4.IsMoving = false;
            this.pictureBoxYINLIE_4.Location = new System.Drawing.Point(20 + 3 * (_nRadiusViewPictureboxWidth + _nSplitViewWidth), 40);
            this.pictureBoxYINLIE_4.Name = "pictureBoxYINLIE_4";
            this.pictureBoxYINLIE_4.NSelectLineIndex = -1;
            this.pictureBoxYINLIE_4.PreImage = null;
            this.pictureBoxYINLIE_4.Size = new System.Drawing.Size(_nRadiusViewPictureboxWidth, _nRadiusViewPictureboxHeight);
            this.pictureBoxYINLIE_4.TabIndex = 3;
            this.pictureBoxYINLIE_4.TabStop = false;
            this.pictureBoxYINLIE_4.BRotate90 = true;
            this.pictureBoxYINLIE_4.UseInteract = true;
            this.pictureBoxYINLIE_4.OnMouseInteractive += PictureBoxYINLIE_4_OnMouseInteractive;
            // 
            // pictureBoxYINLIE_3
            // 
           
            this.pictureBoxYINLIE_3.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYINLIE_3.IsMoving = false;
            this.pictureBoxYINLIE_3.Location = new System.Drawing.Point(20 + 2 * (_nRadiusViewPictureboxWidth + _nSplitViewWidth), 40);
            this.pictureBoxYINLIE_3.Name = "pictureBoxYINLIE_3";
            this.pictureBoxYINLIE_3.NSelectLineIndex = -1;
            this.pictureBoxYINLIE_3.PreImage = null;
            this.pictureBoxYINLIE_3.Size = new System.Drawing.Size(_nRadiusViewPictureboxWidth, _nRadiusViewPictureboxHeight);
            this.pictureBoxYINLIE_3.TabIndex = 2;
            this.pictureBoxYINLIE_3.TabStop = false;
            this.pictureBoxYINLIE_3.BRotate90 = true;
            this.pictureBoxYINLIE_3.UseInteract = true;
            this.pictureBoxYINLIE_3.OnMouseInteractive += PictureBoxYINLIE_3_OnMouseInteractive;
            // 
            // pictureBoxYINLIE_2
            // 
            
            this.pictureBoxYINLIE_2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYINLIE_2.IsMoving = false;
            this.pictureBoxYINLIE_2.Location = new System.Drawing.Point(20 + _nRadiusViewPictureboxWidth + _nSplitViewWidth, 40);
            this.pictureBoxYINLIE_2.Name = "pictureBoxYINLIE_2";
            this.pictureBoxYINLIE_2.NSelectLineIndex = -1;
            this.pictureBoxYINLIE_2.PreImage = null;
            this.pictureBoxYINLIE_2.Size = new System.Drawing.Size(_nRadiusViewPictureboxWidth, _nRadiusViewPictureboxHeight);
            this.pictureBoxYINLIE_2.TabIndex = 1;
            this.pictureBoxYINLIE_2.TabStop = false;
            this.pictureBoxYINLIE_2.BRotate90 = true;
            this.pictureBoxYINLIE_2.UseInteract = true;
            this.pictureBoxYINLIE_2.OnMouseInteractive += PictureBoxYINLIE_2_OnMouseInteractive;
            // 
            // pictureBoxYINLIE_1
            // 
            
            this.pictureBoxYINLIE_1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYINLIE_1.IsMoving = false;
            this.pictureBoxYINLIE_1.Location = new System.Drawing.Point(20, 40);
            this.pictureBoxYINLIE_1.Name = "pictureBoxYINLIE_1";
            this.pictureBoxYINLIE_1.NSelectLineIndex = -1;
            this.pictureBoxYINLIE_1.PreImage = null;
            this.pictureBoxYINLIE_1.Size = new System.Drawing.Size(_nRadiusViewPictureboxWidth, _nRadiusViewPictureboxHeight);
            this.pictureBoxYINLIE_1.TabIndex = 0;
            this.pictureBoxYINLIE_1.TabStop = false;
            this.pictureBoxYINLIE_1.SizeMode = PictureBoxSizeMode.Normal;
            this.pictureBoxYINLIE_1.BRotate90 = true;
            this.pictureBoxYINLIE_1.UseInteract = true;
            this.pictureBoxYINLIE_1.OnMouseInteractive += PictureBoxYINLIE_1_OnMouseInteractive;

            // 
            // pictureBoxLJ_1
            // 

            this.pictureBoxLJ_4.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxLJ_4.IsMoving = false;
            this.pictureBoxLJ_4.Location = new System.Drawing.Point(20 + 3 * (_nRadiusViewPictureboxWidth + _nSplitViewWidth), 40);
            this.pictureBoxLJ_4.Name = "pictureBoxLJ_4";
            this.pictureBoxLJ_4.NSelectLineIndex = -1;
            this.pictureBoxLJ_4.PreImage = null;
            this.pictureBoxLJ_4.Size = new System.Drawing.Size(_nRadiusViewPictureboxWidth, _nRadiusViewPictureboxHeight);
            this.pictureBoxLJ_4.TabIndex = 8;
            this.pictureBoxLJ_4.TabStop = false;
            this.pictureBoxLJ_4.BRotate90 = true;
            this.pictureBoxLJ_4.UseInteract = true;
            this.pictureBoxLJ_4.OnMouseInteractive += PictureBoxLJ_4_OnMouseInteractive; 

            // 
            // pictureBoxLJ_1
            // 

            this.pictureBoxLJ_3.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxLJ_3.IsMoving = false;
            this.pictureBoxLJ_3.Location = new System.Drawing.Point(20 + 2 * (_nRadiusViewPictureboxWidth + _nSplitViewWidth), 40);
            this.pictureBoxLJ_3.Name = "pictureBoxLJ_3";
            this.pictureBoxLJ_3.NSelectLineIndex = -1;
            this.pictureBoxLJ_3.PreImage = null;
            this.pictureBoxLJ_3.Size = new System.Drawing.Size(_nRadiusViewPictureboxWidth, _nRadiusViewPictureboxHeight);
            this.pictureBoxLJ_3.TabIndex = 8;
            this.pictureBoxLJ_3.TabStop = false;
            this.pictureBoxLJ_3.BRotate90 = true;
            this.pictureBoxLJ_3.UseInteract = true;
            this.pictureBoxLJ_3.OnMouseInteractive += PictureBoxLJ_3_OnMouseInteractive; 

            // 
            // pictureBoxLJ_2
            // 

            this.pictureBoxLJ_2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxLJ_2.IsMoving = false;
            this.pictureBoxLJ_2.Location = new System.Drawing.Point(20 + _nRadiusViewPictureboxWidth + _nSplitViewWidth, 40);
            this.pictureBoxLJ_2.Name = "pictureBoxLJ_2";
            this.pictureBoxLJ_2.NSelectLineIndex = -1;
            this.pictureBoxLJ_2.PreImage = null;
            this.pictureBoxLJ_2.Size = new System.Drawing.Size(_nRadiusViewPictureboxWidth, _nRadiusViewPictureboxHeight);
            this.pictureBoxLJ_2.TabIndex = 8;
            this.pictureBoxLJ_2.TabStop = false;
            this.pictureBoxLJ_2.BRotate90 = true;
            this.pictureBoxLJ_2.UseInteract = true;
            this.pictureBoxLJ_2.OnMouseInteractive += PictureBoxLJ_2_OnMouseInteractive; 
            // 
            // pictureBoxLJ_1
            // 

            this.pictureBoxLJ_1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxLJ_1.IsMoving = false;
            this.pictureBoxLJ_1.Location = new System.Drawing.Point(20, 40);
            this.pictureBoxLJ_1.Name = "pictureBoxLJ_1";
            this.pictureBoxLJ_1.NSelectLineIndex = -1;
            this.pictureBoxLJ_1.PreImage = null;
            this.pictureBoxLJ_1.Size = new System.Drawing.Size(_nRadiusViewPictureboxWidth, _nRadiusViewPictureboxHeight);
            this.pictureBoxLJ_1.TabIndex = 8;
            this.pictureBoxLJ_1.TabStop = false;
            this.pictureBoxLJ_1.BRotate90 = true;
            this.pictureBoxLJ_1.UseInteract = true;
            this.pictureBoxLJ_1.OnMouseInteractive += PictureBoxLJ_1_OnMouseInteractive;


            // 
            // pictureBoxYingLi_4
            // 

            this.pictureBoxYingLi_4.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYingLi_4.IsMoving = false;
            this.pictureBoxYingLi_4.Location = new System.Drawing.Point(20 + 3 * (_nRadiusViewPictureboxWidth + _nSplitViewWidth), 40);
            this.pictureBoxYingLi_4.Name = "pictureBoxYingLi_4";
            this.pictureBoxYingLi_4.NSelectLineIndex = -1;
            this.pictureBoxYingLi_4.PreImage = null;
            this.pictureBoxYingLi_4.Size = new System.Drawing.Size(_nRadiusViewPictureboxWidth, _nRadiusViewPictureboxHeight);
            this.pictureBoxYingLi_4.TabIndex = 7;
            this.pictureBoxYingLi_4.TabStop = false;
            this.pictureBoxYingLi_4.UseInteract = true;
            this.pictureBoxYingLi_4.BRotate90 = true;
            this.pictureBoxYingLi_4.OnMouseInteractive += PictureBoxYingLi_4_OnMouseInteractive;
            // 
            // pictureBoxYingLi_3
            // 

            this.pictureBoxYingLi_3.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYingLi_3.IsMoving = false;
            this.pictureBoxYingLi_3.Location = new System.Drawing.Point(20 + 2 * (_nRadiusViewPictureboxWidth + _nSplitViewWidth), 40);
            this.pictureBoxYingLi_3.Name = "pictureBoxYingLi_3";
            this.pictureBoxYingLi_3.NSelectLineIndex = -1;
            this.pictureBoxYingLi_3.PreImage = null;
            this.pictureBoxYingLi_3.Size = new System.Drawing.Size(_nRadiusViewPictureboxWidth, _nRadiusViewPictureboxHeight);
            this.pictureBoxYingLi_3.TabIndex = 6;
            this.pictureBoxYingLi_3.TabStop = false;
            this.pictureBoxYingLi_3.UseInteract = true;
            this.pictureBoxYingLi_3.BRotate90 = true;
            this.pictureBoxYingLi_3.OnMouseInteractive += PictureBoxYingLi_3_OnMouseInteractive;
            // 
            // pictureBoxYingLi_2
            // 

            this.pictureBoxYingLi_2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYingLi_2.IsMoving = false;
            this.pictureBoxYingLi_2.Location = new System.Drawing.Point(20 + _nRadiusViewPictureboxWidth + _nSplitViewWidth, 40);
            this.pictureBoxYingLi_2.Name = "pictureBoxYingLi_2";
            this.pictureBoxYingLi_2.NSelectLineIndex = -1;
            this.pictureBoxYingLi_2.PreImage = null;
            this.pictureBoxYingLi_2.Size = new System.Drawing.Size(_nRadiusViewPictureboxWidth, _nRadiusViewPictureboxHeight);
            this.pictureBoxYingLi_2.TabIndex = 5;
            this.pictureBoxYingLi_2.TabStop = false;
            this.pictureBoxYingLi_2.UseInteract = true;
            this.pictureBoxYingLi_2.BRotate90 = true;
            this.pictureBoxYingLi_2.OnMouseInteractive += PictureBoxYingLi_2_OnMouseInteractive;
            // 
            // pictureBoxYingLi_1
            // 

            this.pictureBoxYingLi_1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYingLi_1.IsMoving = false;
            this.pictureBoxYingLi_1.Location = new System.Drawing.Point(20, 40);
            this.pictureBoxYingLi_1.Name = "pictureBoxYingLi_1";
            this.pictureBoxYingLi_1.NSelectLineIndex = -1;
            this.pictureBoxYingLi_1.PreImage = null;
            this.pictureBoxYingLi_1.Size = new System.Drawing.Size(_nRadiusViewPictureboxWidth, _nRadiusViewPictureboxHeight);
            this.pictureBoxYingLi_1.TabIndex = 4;
            this.pictureBoxYingLi_1.TabStop = false;
            this.pictureBoxYingLi_1.UseInteract = true;
            this.pictureBoxYingLi_1.BRotate90 = true;
            this.pictureBoxYingLi_1.OnMouseInteractive += PictureBoxYingLi_1_OnMouseInteractive;

            


            // 
            // panelRadiusView_LJ
            // 
            this.panelRadiusView_LJ.Controls.Add(this.pictureBoxLJ_1);
            this.panelRadiusView_LJ.Controls.Add(this.pictureBoxLJ_2);
            this.panelRadiusView_LJ.Controls.Add(this.pictureBoxLJ_3);
            this.panelRadiusView_LJ.Controls.Add(this.pictureBoxLJ_4);

            this.panelRadiusView_LJ.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.panelRadiusView_LJ.Location = new System.Drawing.Point(10, _nViewBeginY);
            this.panelRadiusView_LJ.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelRadiusView_LJ.MinimumSize = new System.Drawing.Size(1, 1);
            this.panelRadiusView_LJ.Name = "panelRadiusView_LJ";
            //this.panelRadiusView_LJ.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.panelRadiusView_LJ.ShowCollapse = true;
          
            this.panelRadiusView_LJ.Size = new System.Drawing.Size(_nNoCollapsedRadiusViewWidth, _nNoCollapsedRadiusViewHeight);
            this.panelRadiusView_LJ.TabIndex = 0;
            this.panelRadiusView_LJ.Text = "孪晶";
            this.panelRadiusView_LJ.TitleType = 1;
            this.panelRadiusView_LJ.Collapsed = false;
            this.panelRadiusView_LJ.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.panelRadiusView_LJ.SizeChanged += PanelRadiusView_LJ_SizeChanged;

            // 
            // panelRadiusView_YINLIE
            // 
            this.panelRadiusView_YINLIE.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.panelRadiusView_YINLIE.Location = new System.Drawing.Point(10 + _nNoCollapsedRadiusViewWidth + _nSplitViewWidth, _nViewBeginY);
            this.panelRadiusView_YINLIE.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelRadiusView_YINLIE.MinimumSize = new System.Drawing.Size(1, 1);
            this.panelRadiusView_YINLIE.Name = "panelRadiusView_YINLIE";
            //this.panelRadiusView_YINLIE.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.panelRadiusView_YINLIE.ShowCollapse = true;
            this.panelRadiusView_YINLIE.Collapsed = false;
            
            this.panelRadiusView_YINLIE.Size = new System.Drawing.Size(_nNoCollapsedRadiusViewWidth, _nNoCollapsedRadiusViewHeight);
            this.panelRadiusView_YINLIE.TabIndex = 9;
            this.panelRadiusView_YINLIE.Text = "隐裂";
            this.panelRadiusView_YINLIE.TitleType = 1;
            this.panelRadiusView_YINLIE.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.panelRadiusView_YINLIE.SizeChanged += PanelRadiusView_YINLIE_SizeChanged;

            // 
            // panelRadiusView_YingLi
            // 
            this.panelRadiusView_YingLi.Controls.Add(this.pictureBoxYingLi_1);
            this.panelRadiusView_YingLi.Controls.Add(this.pictureBoxYingLi_2);
            this.panelRadiusView_YingLi.Controls.Add(this.pictureBoxYingLi_3);
            this.panelRadiusView_YingLi.Controls.Add(this.pictureBoxYingLi_4);
            this.panelRadiusView_YingLi.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.panelRadiusView_YingLi.Location = new System.Drawing.Point(10 + 2 * (_nNoCollapsedRadiusViewWidth + _nSplitViewWidth), _nViewBeginY);
            this.panelRadiusView_YingLi.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelRadiusView_YingLi.MinimumSize = new System.Drawing.Size(1, 1);
            this.panelRadiusView_YingLi.Name = "panelRadiusView_YingLi";
            //this.panelRadiusView_YingLi.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.panelRadiusView_YingLi.ShowCollapse = true;
            this.panelRadiusView_YingLi.Collapsed = false;
            this.panelRadiusView_YingLi.Size = new System.Drawing.Size(_nNoCollapsedRadiusViewWidth, _nNoCollapsedRadiusViewHeight);
            this.panelRadiusView_YingLi.TabIndex = 0;
            this.panelRadiusView_YingLi.Text = "应力";
            this.panelRadiusView_YingLi.TitleType = 1;
            this.panelRadiusView_YingLi.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.panelRadiusView_YingLi.SizeChanged += PanelRadiusView_YingLi_SizeChanged;
            

            // 
            // InspectResultNewForm
            //

            this.Controls.Add(this.panelRadiusView_LJ);
            this.Controls.Add(this.panelRadiusView_YingLi);
            this.Controls.Add(this.panelRadiusView_YINLIE);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1900, 980);
            this.Name = "InspectResultNewForm";
            this.Text = "结果展示";
            this.ZoomScaleRect = new System.Drawing.Rectangle(19, 19, 1655, 988);
            this.panelRadiusView_YINLIE.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYINLIE_4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYINLIE_3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYINLIE_2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYINLIE_1)).EndInit();
            this.panelRadiusView_LJ.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLJ_1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLJ_2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLJ_3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLJ_4)).EndInit();
            this.panelRadiusView_YingLi.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi_4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi_3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi_2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi_1)).EndInit();
           
            
            this.ResumeLayout(false);

            this.ShowFullScreen = true;
            this.Shown += InspectResultForm_Shown;
        }

        private void PanelRadiusView_LJ_SizeChanged(object sender, System.EventArgs e)
        {
            if (this.panelRadiusView_LJ.Width <= 40)
            {   
                this.panelRadiusView_YINLIE.Left = 10 + this.panelRadiusView_LJ.Width + _nSplitViewWidth;
                this.panelRadiusView_YingLi.Left = 10 + this.panelRadiusView_LJ.Width + _nSplitViewWidth + this.panelRadiusView_YINLIE.Width + _nSplitViewWidth;
            }
            else
            {
                this.panelRadiusView_YINLIE.Left = 10 + this.panelRadiusView_LJ.Width + _nSplitViewWidth;
                this.panelRadiusView_YingLi.Left = 10 + this.panelRadiusView_LJ.Width + _nSplitViewWidth + this.panelRadiusView_YINLIE.Width + _nSplitViewWidth;

            }
            this.panelRadiusView_LJ.Refresh();
            this.panelRadiusView_YINLIE.Refresh();
            this.panelRadiusView_YingLi.Refresh();
            this.BeginInvoke(refreshViewFunc, 0);

        }

        private void PanelRadiusView_YINLIE_SizeChanged(object sender, System.EventArgs e)
        {
            if (this.panelRadiusView_YINLIE.Width <= 40)
            {
                this.panelRadiusView_YingLi.Left = 10 + this.panelRadiusView_LJ.Width + _nSplitViewWidth + this.panelRadiusView_YINLIE.Width + _nSplitViewWidth;

            }
            else
            {
                this.panelRadiusView_YingLi.Left = 10 + this.panelRadiusView_LJ.Width + _nSplitViewWidth + this.panelRadiusView_YINLIE.Width + _nSplitViewWidth;

            }

            this.panelRadiusView_LJ.Refresh();
            this.panelRadiusView_YINLIE.Refresh();
            this.panelRadiusView_YingLi.Refresh();
            this.BeginInvoke(refreshViewFunc, 1);

        }

        private void PanelRadiusView_YingLi_SizeChanged(object sender, System.EventArgs e)
        {
            this.panelRadiusView_YingLi.Left = 10 + this.panelRadiusView_LJ.Width + _nSplitViewWidth + this.panelRadiusView_YINLIE.Width + _nSplitViewWidth;

            this.panelRadiusView_LJ.Refresh();
            this.panelRadiusView_YINLIE.Refresh();
            this.panelRadiusView_YingLi.Refresh();

            this.BeginInvoke(refreshViewFunc, 2);
        }


        private void UpdateView(int nType)
        {
            switch (nType)
            {
                case 0:
                    {
                        this.panelRadiusView_LJ.Refresh();
                        break;
                    }
                case 1:
                    {
                        this.panelRadiusView_YINLIE.Refresh();
                        break;
                    }
                case 2:
                    {
                        this.panelRadiusView_YingLi.Refresh();
                        break;
                    }
            }
        }


        private void PictureBoxLJ_4_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
            this.panelRadiusView_LJ.Refresh();
        }

        private void PictureBoxLJ_2_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
            this.panelRadiusView_LJ.Refresh();
        }

        private void PictureBoxLJ_3_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
            this.panelRadiusView_LJ.Refresh();
        }

        private void InspectResultForm_Shown(object sender, System.EventArgs e)
        {
            if (this.panelRadiusView_YINLIE.Size.Height > 40)
            {
                this.panelRadiusView_YINLIE.Refresh();
            }

            if (panelRadiusView_YingLi.Size.Height > 40)
            {
                this.panelRadiusView_YingLi.Refresh();
            }

            if (panelRadiusView_LJ.Size.Height > 40)
            {
                this.panelRadiusView_LJ.Refresh();
            }


        }

        private void InspectResultForm_Load(object sender, System.EventArgs e)
        {
            this.panelRadiusView_YINLIE.Collapsed = true;
            this.panelRadiusView_LJ.Collapsed = false;

            
            Thread.Sleep(500);
            this.panelRadiusView_LJ.Collapsed = true;
            this.panelRadiusView_YINLIE.Collapsed = false;
            
            //System.Drawing.Point pt = new System.Drawing.Point();
            //pt.X = this.pictureBoxYINLIE_2.Left;
            //pt.Y = this.pictureBoxYINLIE_2.Top;

            //System.Drawing.Point ptNew  = this.pictureBoxYINLIE_2.PointToScreen(pt);
           

            //mouse_event(MOUSEEVENTF_LEFTDOWN , ptNew.X + 20, ptNew.Y + 20, 0, 0);
        }

       

        private void PictureBoxYINLIE_4_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
            this.panelRadiusView_YINLIE.Refresh();
        }

        private void PictureBoxYingLi_4_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
            this.panelRadiusView_YingLi.Refresh();
        }

       
        private void PictureBoxYingLi_3_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
            this.panelRadiusView_YingLi.Refresh();
        }

        private void PictureBoxYINLIE_3_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
            this.panelRadiusView_YINLIE.Refresh();
        }

        

        private void PictureBoxYINLIE_2_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
            this.panelRadiusView_YINLIE.Refresh();
        }

        private void PictureBoxYingLi_2_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
            this.panelRadiusView_YingLi.Refresh();
        }

        

        private void PictureBoxLJ_1_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
            this.panelRadiusView_LJ.Refresh();

        }

        private void PictureBoxYINLIE_1_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
            this.panelRadiusView_YINLIE.Refresh();
        }

        private void PictureBoxYingLi_1_OnMouseInteractive(object sender, InteractivePictureBox.PanZoomImageEventArgs e)
        {
            this.panelRadiusView_YingLi.Refresh();
        }


       

        private void PanelRadiusView_2_SizeChanged(object sender, System.EventArgs e)
        {
            if (this.panelRadiusView_LJ.Size.Height > 40)
            {
                this.panelRadiusView_YINLIE.Collapsed = true;
                this.panelRadiusView_YingLi.Collapsed = true;
                
                //this.panelRadiusView_YINLIE.Top = _nBeginRaidusViewY;
                //this.panelRadiusView_LJ.Top = this.panelRadiusView_YINLIE.Top + _nCollapsedRadiusViewHeight + _nSplitViewHeight;
                //this.panelRadiusView_YingLi.Top = this.panelRadiusView_LJ.Top + _nNoCollapsedRadiusViewHeight + _nSplitViewHeight;
                //this.panelRadiusView_Radius.Top = this.panelRadiusView_YingLi.Top + _nCollapsedRadiusViewHeight + _nSplitViewHeight;
                panelRadiusView_LJ.Refresh();


            }
            else
            {
                this.panelRadiusView_YingLi.Top = this.panelRadiusView_LJ.Top + this.panelRadiusView_LJ.Height + _nSplitViewWidth;
               
            }
        }

        private void PanelRadiusView_3_SizeChanged(object sender, System.EventArgs e)
        {
            if (this.panelRadiusView_YingLi.Size.Height > 40)
            {
                this.panelRadiusView_YINLIE.Collapsed = true;
                this.panelRadiusView_LJ.Collapsed = true;
                //this.panelRadiusView_Radius.Collapsed = true;

                //this.panelRadiusView_YINLIE.Top = _nBeginRaidusViewY;
                //this.panelRadiusView_LJ.Top = this.panelRadiusView_YINLIE.Top + _nCollapsedRadiusViewHeight + _nSplitViewHeight;
                //this.panelRadiusView_YingLi.Top = this.panelRadiusView_LJ.Top + _nCollapsedRadiusViewHeight + _nSplitViewHeight;
                //this.panelRadiusView_Radius.Top = this.panelRadiusView_YingLi.Top + _nNoCollapsedRadiusViewHeight + _nSplitViewHeight;
                panelRadiusView_YingLi.Refresh();
                
            }
            else
            {
                
               
            }
        }

     

        #endregion
       
        private UITitleTypePanel panelRadiusView_YINLIE;
        private InteractivePictureBox.PictureBoxEx pictureBoxYINLIE_1;
        private InteractivePictureBox.PictureBoxEx pictureBoxYINLIE_2;
        private InteractivePictureBox.PictureBoxEx pictureBoxYINLIE_3;
        private InteractivePictureBox.PictureBoxEx pictureBoxYINLIE_4;
        private UITitleTypePanel panelRadiusView_LJ;
        private InteractivePictureBox.PictureBoxEx pictureBoxLJ_1;
        private InteractivePictureBox.PictureBoxEx pictureBoxLJ_2;
        private InteractivePictureBox.PictureBoxEx pictureBoxLJ_3;
        private InteractivePictureBox.PictureBoxEx pictureBoxLJ_4;

        private UITitleTypePanel panelRadiusView_YingLi;
        private InteractivePictureBox.PictureBoxEx pictureBoxYingLi_1;
        private InteractivePictureBox.PictureBoxEx pictureBoxYingLi_2;
        private InteractivePictureBox.PictureBoxEx pictureBoxYingLi_3;
        private InteractivePictureBox.PictureBoxEx pictureBoxYingLi_4;
        
        
    }
}