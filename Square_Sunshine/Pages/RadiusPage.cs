using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts;
using Sunny.UI;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Media;

namespace SiliconRoundBarCheck.Pages
{
    public partial class RadiusPage : UIPage
    {
        public RadiusPage()
        {
            InitializeComponent();

            _cartesianChart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<ObservablePoint>
                    {
                        new ObservablePoint(0, 10),
                        new ObservablePoint(4, 7),
                        new ObservablePoint(5, 3),
                        new ObservablePoint(7, 6),
                        new ObservablePoint(10, 8)
                    },

                    PointGeometrySize = 15,
                    
                    //Values = new ChartValues<double>
                    //{
                    //    4,
                    //    5,
                    //    7,
                    //    8,
                    //    double.NaN,
                    //    5,
                    //    2,
                    //    8,
                    //    double.NaN,
                    //    6,
                    //    2
                    //}
                },


            };


            AxisSection axissection = new AxisSection
            {
                Stroke = System.Windows.Media.Brushes.Black,//colocr
                StrokeThickness = 5,
                Value = 2,//Modify this.
                Draggable = true,
                AllowDrop = true,
            };

            AxisSection areaSection = new AxisSection
            {
                Label = "Good",
                Value = 2,
                SectionWidth = 4,
                Fill = new SolidColorBrush
                {
                    Color = System.Windows.Media.Color.FromRgb(204, 204, 204),
                    Opacity = .4
                }
            };

            AxisSection axiendssection = new AxisSection
            {
                Stroke = System.Windows.Media.Brushes.Black,//colocr
                StrokeThickness = 5,
                Value = 6,//Modify this.
                Draggable = true,
                AllowDrop = true,

            };

            axiendssection.MouseDownFunc += Axiendssection_MouseDown;
            axiendssection.MouseMoveFunc += Axiendssection_MouseMove;
            axiendssection.MouseUpFunc += Axiendssection_MouseUp;

            _cartesianChart.AxisX.Add(new Axis
            {
                Sections = new SectionsCollection
                {
                    axissection,
                    areaSection,
                    axiendssection
                }
            });
        }

        private void Axiendssection_MouseUp(object sender, System.Windows.Input.MouseEventArgs args)
        {
            Debug.WriteLine("Axiendssection_MouseUp Click");
        }

        private void Axiendssection_MouseMove(object sender, System.Windows.Input.MouseEventArgs args)
        {
            Debug.WriteLine("Axiendssection_MouseMove Click");
        }

        private void Axiendssection_MouseDown(object sender, System.Windows.Input.MouseEventArgs args)
        {
            Debug.WriteLine("Axiendssection_MouseDown Click");
        }

    }
}