using Sunny.UI;

namespace SiliconRoundBarCheck.Pages
{
    partial class MainStatisticPage
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
            this._cartesianChart = new LiveCharts.WinForms.CartesianChart();
            this.SuspendLayout();
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
            // _cartesianChart
            // 
            this._cartesianChart.Location = new System.Drawing.Point(27, 47);
            this._cartesianChart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cartesianChart.Name = "_cartesianChart";
            this._cartesianChart.Size = new System.Drawing.Size(1632, 832);
            this._cartesianChart.TabIndex = 5;
            this._cartesianChart.Text = "cartesianChart1";
            // 
            // MainStatisticPage
            // 
            this.AllowShowTitle = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1686, 882);
            this.Controls.Add(this._cartesianChart);
            this.Controls.Add(this._rightBtn);
            this.Name = "MainStatisticPage";
            this.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.PageIndex = 1001;
            this.ShowTitle = true;
            this.Text = "结果统计";
            this.TitleFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.ResumeLayout(false);

        }

       
        private void _rightBtn_Click(object sender, System.EventArgs e)
        {
            FormMain.formMainF.showAsideFunc();
           
           
            SettingParamPage.instance.ShowRightBtnFunc(false);
            MainStatisticPage.instance.ShowRightBtnFunc(false);
            


        }

        #endregion

        private LiveCharts.WinForms.CartesianChart _cartesianChart;
        private UIButton _rightBtn;
    }
}