using Sunny.UI;
using LiveCharts;
namespace SiliconRoundBarCheck.Pages
{
    partial class RadiusPage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private LiveCharts.WinForms.CartesianChart _cartesianChart;
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
            this._cartesianChart = new LiveCharts.WinForms.CartesianChart();
            this.SuspendLayout();
            // 
            // _cartesianChart
            // 
            this._cartesianChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cartesianChart.Location = new System.Drawing.Point(0, 0);
            this._cartesianChart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cartesianChart.Name = "cartesianChart1";
            this._cartesianChart.Size = new System.Drawing.Size(879, 543);
            this._cartesianChart.TabIndex = 3;
            this._cartesianChart.Text = "cartesianChart1";
            // 
            // RadiusPage
            // 
            this.AllowShowTitle = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(2078, 956);
            this.Controls.Add(this._cartesianChart);
            this.Name = "RadiusPage";
            this.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.PageIndex = 1002;
            this.ShowTitle = true;
            this.Text = "直径";
            this.TitleFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.ResumeLayout(false);

        }

        #endregion
    }
}