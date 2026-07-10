using Sunny.UI;

using SquareSiliconStickCheck.Tools;
using SiliconRoundBarCheck.Tools;
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
            this.buttonLeft = new Sunny.UI.UIButton();
            this.SuspendLayout();
            // 
            // Aside
            // 
            this.Aside.AllowDrop = true;
            this.Aside.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.Aside.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this.Aside.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(190)))), ((int)(((byte)(138)))));
            this.Aside.LineColor = System.Drawing.Color.White;
            this.Aside.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            this.Aside.SelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(175)))), ((int)(((byte)(36)))));
            this.Aside.SelectedForeColor = System.Drawing.Color.White;
            this.Aside.Size = new System.Drawing.Size(226, 995);
            this.Aside.Style = Sunny.UI.UIStyle.Custom;
            // 
            // buttonLeft
            // 
            this.buttonLeft.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonLeft.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this.buttonLeft.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this.buttonLeft.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(190)))), ((int)(((byte)(138)))));
            this.buttonLeft.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this.buttonLeft.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonLeft.ForeDisableColor = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(176)))), ((int)(((byte)(176)))));
            this.buttonLeft.Location = new System.Drawing.Point(207, 362);
            this.buttonLeft.MinimumSize = new System.Drawing.Size(1, 1);
            this.buttonLeft.Name = "buttonLeft";
            this.buttonLeft.Size = new System.Drawing.Size(19, 196);
            this.buttonLeft.Style = Sunny.UI.UIStyle.Custom;
            this.buttonLeft.TabIndex = 2;
            this.buttonLeft.Text = "<";
            this.buttonLeft.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonLeft.Click += new System.EventHandler(this.buttonLeft_Click);
            // 
            // FormMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.ClientSize = new System.Drawing.Size(1920, 1030);
            this.Controls.Add(this.buttonLeft);
            this.Name = "FormMain";
            this.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "Square rod size inspection system";
            this.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.TitleFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ZoomScaleRect = new System.Drawing.Rectangle(22, 22, 1345, 731);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing_1);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Controls.SetChildIndex(this.Aside, 0);
            this.Controls.SetChildIndex(this.buttonLeft, 0);
            this.ResumeLayout(false);

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

