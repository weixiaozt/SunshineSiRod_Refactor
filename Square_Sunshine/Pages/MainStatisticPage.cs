using Sunny.UI;
using SiliconRoundBarCheck.Cameras;
using SiliconRoundBarCheck.Tools;
using SiliconRoundBarCheck.Parameters;
using LiveCharts.Wpf;
using LiveCharts;
using System.Windows.Documents;
using System.Collections.Generic;

namespace SiliconRoundBarCheck.Pages
{
    public partial class MainStatisticPage : UIPage
    {
        public delegate void ShowRightButtonDelegate(bool bShow);

        public ShowRightButtonDelegate ShowRightBtnFunc;

        public static MainStatisticPage instance;

        private ChartValues<double> chartValueInfo;
        public MainStatisticPage()
        {

            instance = this;

            InitializeComponent();
            ShowRightBtnFunc = new ShowRightButtonDelegate(ShowRightButton);

            chartValueInfo = new ChartValues<double>();
            List<InspectResult> okResult = CMySQLTool.Instance().SearchResult(1);
            List<InspectResult> ngResult = CMySQLTool.Instance().SearchResult(0);

            
            chartValueInfo.Add(okResult.Count);
            chartValueInfo.Add(ngResult.Count);
            var barSeries = new ColumnSeries
            {
                Values = chartValueInfo
            };


            _cartesianChart.Series.Add(barSeries);
            _cartesianChart.AxisX.Add(new Axis
            {
                Labels = new[] { "OK", "NG"}
            });
        }

        public void ShowRightButton(bool bShow)
        {
            if ( bShow )
            {
                this._rightBtn.Visible = true;
            }
            else
            {
                this._rightBtn.Visible = false;
            }
        }
        private void InitTools()
        {
           
           
        }

    }
}