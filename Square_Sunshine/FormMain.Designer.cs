using Sunny.UI;

namespace SquareSiliconStickCheck
{
    partial class FormMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonLeft = new UIButton();
           
            this.SuspendLayout();
            // 
            // Aside
            // 
            this.Aside.AllowDrop = true;
            this.Aside.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.Aside.FillColor = System.Drawing.SystemColors.MenuHighlight;
            this.Aside.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.Aside.LineColor = System.Drawing.Color.White;
            this.Aside.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            this.Aside.ScrollFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.Aside.SelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.Aside.SelectedForeColor = System.Drawing.Color.White;
            this.Aside.Size = new System.Drawing.Size(226, 995);
            this.Aside.Style = Sunny.UI.UIStyle.Custom;
            // 
            // buttonLeft
            // 
            this.buttonLeft.Location = new System.Drawing.Point(207, 362);
            this.buttonLeft.Name = "buttonLeft";
            this.buttonLeft.Size = new System.Drawing.Size(19, 196);
            this.buttonLeft.TabIndex = 2;
            this.buttonLeft.Text = "<";
            this.buttonLeft.Click += new System.EventHandler(this.buttonLeft_Click);
          
            // 
            // FormMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1920, 1030);
           
            this.Controls.Add(this.buttonLeft);
            this.Name = "FormMain";
            this.Text = "籽晶检测系统";
            this.ZoomScaleRect = new System.Drawing.Rectangle(22, 22, 1345, 731);
            this.Controls.SetChildIndex(this.Aside, 0);
            this.Controls.SetChildIndex(this.buttonLeft, 0);
         
            this.ResumeLayout(false);

            this.FormClosing += FormMain_FormClosing;

        }

        private void FormMain_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (_threadCloseWindows != null && _threadCloseWindows.IsAlive == true)
            {
                _threadCloseWindows.Abort();
            }


            
        }

        #endregion

        private UIButton buttonLeft;
      
    }
}

