using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts;
using OpenCvSharp;
using SiliconRoundBarCheck.Parameters;
using SiliconRoundBarCheck.Tools;
using Sunny.UI;
using SiliconRoundBarCheck.Data;
using System.Windows.Media;
using System.Diagnostics;
using YouEyEE.Untils.Log;

namespace SiliconRoundBarCheck.Pages
{
    public partial class InspectResultLineNewForm : UIForm
    {
        private InspectResultLineInfo _curresult;

        public InspectResultLineInfo Curresult { get => _curresult; set => _curresult = value; }

        private ArrayList _resultRadius;
        private ArrayList _resultYingLi;
        private ArrayList _resultResisvity ;
        private ArrayList _resultPenetration;

        private ArrayList _preShowRadius;
        private ArrayList _preShowResisvity;
        private ArrayList _preShowPenetrationRate;
        private ArrayList _preShowYingLi;

        private AxisSection[] _axisRadiusBeginsection;
        private AxisSection[] _axisRadiusEndssection;
        private AxisSection[] _axisRadiusAreassection;
        private AxisSection[] _axisYingLiBeginsection;
        private AxisSection[] _axisYingLiEndssection;
        private AxisSection[] _axisYingLiAreassection;
        private StickData _curStickData;
        private Timer _tim;
        private Timer _timeRefresh;
        private Timer _timeRadiusRefresh;
        private Timer _timeYingLiRefresh;
        private Timer _timeResivityRefresh;
        private Timer _timePenetrationRateRefresh;

        private DateTime _curDate;
        public ChartValues<ObservablePoint> RadiusChartValues { get; set; }
        public ChartValues<ObservablePoint> ResivityChartValues { get; set; }
        public ChartValues<ObservablePoint> YingLiChartValues { get; set; }
        public ChartValues<ObservablePoint> PenetrationRateChartValues { get; set; }
        public DateTime CurDate { get => _curDate; set => _curDate = value; }

        private float _fMaxRadius = 0;
        private float _fMinRadius = 0;
        private float _fLength = 0;
        private float _fValidLength = 0;
        private int _preIndexOfRadius = 0;
        private int _preIndexOfYingLi = 0;
        private int _preIndexOfPenetrationRate = 0;
        private int _preIndexOfResisvity = 0;
        private float _preMinValueOfRadius = 65535;
        private float _preMaxValueOfRadius = -1;
        private float _preMinValueOfYingLi = 65535;
        private float _preMaxValueOfYingLi = -1;
        private float _preMinValueOfResisvity = 65535;
        private float _preMaxValueOfResisvity = -1;
        private float _preMinValueOfPenetrationRate = 65535;
        private float _preMaxValueOfPenetrationRate = -1;
        private const int _showCountOfPoints = 300;
        private ArrayList _arrAbnormalArea;
        public InspectResultLineNewForm(InspectResultLineInfo result, DateTime curdate)
        {
            _curDate = curdate;
            _curresult = result;
            InitializeComponent();
            InitResult();
            Init();

            this._tabMainLineControl.SelectedIndex = 0;

            _preIndexOfPenetrationRate = 0;
            _preIndexOfRadius = 0;
            _preIndexOfYingLi = 0;
            _preIndexOfResisvity = 0;

            _preMinValueOfRadius = 65535;
            _preMaxValueOfRadius = -1;
            _preMinValueOfYingLi = 65535;
            _preMaxValueOfYingLi = -1;
            _preMinValueOfResisvity = 65535;
            _preMaxValueOfResisvity = -1;
            _preMinValueOfPenetrationRate = 65535;
            _preMaxValueOfPenetrationRate = -1;
      
            _tim = new System.Windows.Forms.Timer
            {
                Interval = 200,
            };

            _tim.Tick += timeRefreshTabPage;
            _tim.Start();

            _timeRadiusRefresh = new System.Windows.Forms.Timer
            {
                Interval = 500,
            };
            _timeRadiusRefresh.Tick += timeRadiusRefresh_Tick;

            _timeYingLiRefresh = new System.Windows.Forms.Timer
            {
                Interval = 500,
            };
            _timeYingLiRefresh.Tick += timeYingLiRefresh_Tick;

            _timeResivityRefresh = new System.Windows.Forms.Timer
            {
                Interval = 500,
            };
            _timeResivityRefresh.Tick += timeResisvityRefresh_Tick;

            _timePenetrationRateRefresh = new System.Windows.Forms.Timer
            {
                Interval = 500,
            };
            _timePenetrationRateRefresh.Tick += timePenetrationRateRefresh_Tick;

            _timeRadiusRefresh.Start();
            _timeResivityRefresh.Start();
            _timeYingLiRefresh.Start();
            _timePenetrationRateRefresh.Start();
        }

        private void timeRadiusRefresh_Tick(object sender, System.EventArgs e)
        {
            try
            {
                float fMinValue = _preMinValueOfRadius;
                float fMaxValue = _preMaxValueOfRadius;
                int nShowIndex = 0;
                ChartValues<ObservablePoint> ptValues = new ChartValues<ObservablePoint>();

                for (; _preIndexOfRadius < _resultRadius.Count; _preIndexOfRadius++)
                {
                    ptValues.Add(new ObservablePoint(_preIndexOfRadius, (float)_resultRadius[_preIndexOfRadius]));
                    nShowIndex++;
                    if ((float)_resultRadius[_preIndexOfRadius] < fMinValue)
                    {
                        fMinValue = (float)_resultRadius[_preIndexOfRadius];
                    }

                    if ((float)_resultRadius[_preIndexOfRadius] > fMaxValue)
                    {
                        fMaxValue = (float)_resultRadius[_preIndexOfRadius];
                    }
                    if (nShowIndex >= _showCountOfPoints)
                    {
                        break;
                    }

                }

                RadiusChartValues.AddRange(ptValues);
                _preMinValueOfRadius = Math.Max(0, fMinValue - 1);
                _preMaxValueOfRadius = fMaxValue + 1;

                if (_preIndexOfRadius >= _resultRadius.Count)
                {
                    _preIndexOfRadius = 0;
                    _timeRadiusRefresh.Stop();

                    InitRadiusResultSeries();
                }

                _cartesianRadiusChart.AxisY[0].MinValue = fMinValue;
                _cartesianRadiusChart.AxisY[0].MaxValue = fMaxValue;
                _cartesianRadiusChart.Update();
            }
            catch (Exception ex)
            {
                 LogHelper.Info("Silicon","ex" + ex.Message);
            }
        }

        private void timeYingLiRefresh_Tick(object sender, System.EventArgs e)
        {
            try
            {
                float fMinValue = _preMinValueOfYingLi;
                float fMaxValue = _preMaxValueOfYingLi;
                int nShowIndex = 0;
                ChartValues<ObservablePoint> ptValues = new ChartValues<ObservablePoint>();

                for (; _preIndexOfYingLi < _resultYingLi.Count; _preIndexOfYingLi++)
                {
                    ptValues.Add(new ObservablePoint(_preIndexOfYingLi, (float)_resultYingLi[_preIndexOfYingLi]));
                    nShowIndex++;
                    if ((float)_resultYingLi[_preIndexOfYingLi] < fMinValue)
                    {
                        fMinValue = (float)_resultYingLi[_preIndexOfYingLi];
                    }

                    if ((float)_resultYingLi[_preIndexOfYingLi] > fMaxValue)
                    {
                        fMaxValue = (float)_resultYingLi[_preIndexOfYingLi];
                    }
                    if (nShowIndex >= _showCountOfPoints)
                    {
                        break;
                    }
                }

                YingLiChartValues.AddRange(ptValues);
                _preMinValueOfYingLi = fMinValue;
                _preMaxValueOfYingLi = fMaxValue;

                if (_preIndexOfYingLi >= _resultYingLi.Count)
                {
                    _preIndexOfYingLi = 0;
                    _timeYingLiRefresh.Stop();
                }
                _cartesianYingLiChart.AxisY[0].MinValue = fMinValue;
                _cartesianYingLiChart.AxisY[0].MaxValue = fMaxValue;
                _cartesianYingLiChart.Update();
            }
            catch(Exception ex)
            {
                 LogHelper.Info("Silicon","ex" + ex.Message);
            }
            

        }

        private void timeResisvityRefresh_Tick(object sender, System.EventArgs e)
        {
            
            try
            {

                float fMinValue = _preMinValueOfResisvity;
                float fMaxValue = _preMaxValueOfResisvity;
                int nShowIndex = 0;
                ChartValues<ObservablePoint> ptValues = new ChartValues<ObservablePoint>();

                for (; _preIndexOfResisvity < _resultResisvity.Count; _preIndexOfResisvity++)
                {
                    ptValues.Add(new ObservablePoint(_preIndexOfResisvity, (float)_resultResisvity[_preIndexOfResisvity]));
                    nShowIndex++;
                    if ((float)_resultResisvity[_preIndexOfResisvity] < fMinValue)
                    {
                        fMinValue = (float)_resultResisvity[_preIndexOfResisvity];
                    }

                    if ((float)_resultResisvity[_preIndexOfResisvity] > fMaxValue)
                    {
                        fMaxValue = (float)_resultResisvity[_preIndexOfResisvity];
                    }
                    if (nShowIndex >= _showCountOfPoints)
                    {
                        break;
                    }
                }

                ResivityChartValues.AddRange(ptValues);
                _preMinValueOfResisvity = fMinValue;
                _preMaxValueOfResisvity = fMaxValue;

                if (_preIndexOfResisvity >= _resultResisvity.Count)
                {
                    _preIndexOfResisvity = 0;
                    _timeResivityRefresh.Stop();
                }

                _cartesianResisvityChart.AxisY[0].MinValue = fMinValue;
                _cartesianResisvityChart.AxisY[0].MaxValue = fMaxValue;
                _cartesianResisvityChart.Update();
            }
            catch (Exception ex)
            {
                 LogHelper.Info("Silicon","ex" + ex.Message);
            }
        }

        private void timePenetrationRateRefresh_Tick(object sender, System.EventArgs e)
        {
            try
            {
                float fMinValue = _preMinValueOfPenetrationRate;
                float fMaxValue = _preMaxValueOfPenetrationRate;
                int nShowIndex = 0;
                ChartValues<ObservablePoint> ptValues = new ChartValues<ObservablePoint>();

                for (; _preIndexOfPenetrationRate < _resultPenetration.Count; _preIndexOfPenetrationRate++)
                {
                    ptValues.Add(new ObservablePoint(_preIndexOfPenetrationRate, (float)_resultPenetration[_preIndexOfPenetrationRate]));
                    nShowIndex++;
                    if ((float)_resultPenetration[_preIndexOfPenetrationRate] < fMinValue)
                    {
                        fMinValue = (float)_resultPenetration[_preIndexOfPenetrationRate];
                    }

                    if ((float)_resultPenetration[_preIndexOfPenetrationRate] > fMaxValue)
                    {
                        fMaxValue = (float)_resultPenetration[_preIndexOfPenetrationRate];
                    }
                    if (nShowIndex >= _showCountOfPoints)
                    {
                        break;
                    }
                }

                PenetrationRateChartValues.AddRange(ptValues);
                _preMinValueOfPenetrationRate = fMinValue;
                _preMaxValueOfPenetrationRate = fMaxValue;

                if (_preIndexOfPenetrationRate >= _resultPenetration.Count)
                {
                    _preIndexOfPenetrationRate = 0;
                    _timePenetrationRateRefresh.Stop();
                }
                _cartesianPenetrationRateResultView.AxisY[0].MinValue = fMinValue;
                _cartesianPenetrationRateResultView.AxisY[0].MaxValue = fMaxValue;
                _cartesianPenetrationRateResultView.Update();
            }
            catch (Exception ex)
            {
                 LogHelper.Info("Silicon","ex" + ex.Message);
            }
        }

        private void timeRefreshTabPage(object sender, EventArgs e)
        {
            _tim.Stop();
            this.BeginInvoke(new Action(() =>
            {
                this._tabMainLineControl.SelectedIndex = 1;

            }));


            this.BeginInvoke(new Action(() =>
            {
                this._tabMainLineControl.SelectedIndex = 2;

            }));


            this.BeginInvoke(new Action(() =>
            {
                this._tabMainLineControl.SelectedIndex = 3;

            }));

            this.BeginInvoke(new Action(() =>
            {
                this._tabMainLineControl.SelectedIndex = 0;

            }));
        }


        private void Init()
        {
            try
            {
                #region 直径初始化
                {
                    RadiusChartValues = new ChartValues<ObservablePoint>
                    {
                        new ObservablePoint(0, 0)
                    };

                    _cartesianRadiusChart.Series = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Values = RadiusChartValues,
                            PointGeometry = null,
                            DataLabels = false,

                        },

                    };
                    _cartesianRadiusChart.AxisY.Add(new Axis
                    {
                        Title = "直径(mm)",

                    });
                    _cartesianRadiusChart.AxisX.Add(new Axis
                    {
                        Title = "长度(mm)",
                    }
                    );



                    _cartesianRadiusChart.DisableAnimations = true;
                    _cartesianRadiusChart.DataTooltip = null;
                    _cartesianRadiusChart.Hoverable = false;
                    _cartesianRadiusChart.Zoom = ZoomingOptions.Y;

                     

                   

                }
                #endregion

                #region 应力初始化
                {
                    YingLiChartValues = new ChartValues<ObservablePoint>
                    {
                        new ObservablePoint(0, 0)
                    };

                    _cartesianYingLiChart.Series = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Values = YingLiChartValues,
                            PointGeometry = null,
                            DataLabels = false,

                        },
                    };
                    _cartesianYingLiChart.AxisY.Add(new Axis
                    {
                        Title = "直径(mm)",

                    });
                    _cartesianYingLiChart.AxisX.Add(new Axis
                    {
                        Title = "长度(mm)",

                    }
                    );
                    _cartesianYingLiChart.DisableAnimations = true;
                    _cartesianYingLiChart.DataTooltip = null;
                    _cartesianYingLiChart.Hoverable = false;
                    _cartesianYingLiChart.Zoom = ZoomingOptions.Y;


                    _axisYingLiBeginsection = new AxisSection[_arrAbnormalArea.Count];
                    _axisYingLiEndssection = new AxisSection[_arrAbnormalArea.Count];
                    _axisYingLiAreassection = new AxisSection[_arrAbnormalArea.Count];
                    for (int i = 0; i < _arrAbnormalArea.Count; i++)
                    {
                        List<float> fngLengths = (List<float>)_arrAbnormalArea[i];
                        _axisYingLiBeginsection[i] = new AxisSection
                        {
                            Stroke = System.Windows.Media.Brushes.Black,//colocr
                            StrokeThickness = 5,
                            Value = fngLengths[0],//Modify this.
                            Draggable = true,
                            AllowDrop = true,
                            DisableAnimations = true,
                            Name = "Begin_" + i.ToString(),

                        };
                        _axisYingLiAreassection[i] = new AxisSection
                        {
                            Name = "Area_" + i.ToString(),
                            Value = fngLengths[0],
                            SectionWidth = (fngLengths[1] - fngLengths[0]),
                            Fill = new SolidColorBrush
                            {
                                Color = System.Windows.Media.Color.FromRgb(204, 204, 204),
                                Opacity = .4
                            }
                        };
                        _axisYingLiEndssection[i] = new AxisSection
                        {
                            Stroke = System.Windows.Media.Brushes.Black,//colocr
                            StrokeThickness = 5,
                            Value = fngLengths[1],//Modify this.
                            Draggable = true,
                            AllowDrop = true,
                            DisableAnimations = true,
                            Name = "End_" + i.ToString(),
                        };

                        _axisYingLiBeginsection[i].MouseUpFunc += YingLiAxissection_MouseUp;
                        _axisYingLiEndssection[i].MouseUpFunc += YingLiAxissection_MouseUp;
                        _axisYingLiBeginsection[i].MouseDownFunc += YingLiAxissection_MouseDown;
                        _axisYingLiEndssection[i].MouseDownFunc += YingLiAxissection_MouseDown;



                    }


                    for (int i = 0; i < _arrAbnormalArea.Count; i++)
                    {
                        _cartesianYingLiChart.AxisX[0].Sections.Add(_axisYingLiAreassection[i]);
                        _cartesianYingLiChart.AxisX[0].Sections.Add(_axisYingLiBeginsection[i]);
                        _cartesianYingLiChart.AxisX[0].Sections.Add(_axisYingLiEndssection[i]);
                    }
                }
                #endregion

                #region 电阻率初始化
                {
                    ResivityChartValues = new ChartValues<ObservablePoint>
                    {
                        new ObservablePoint(0, 0)
                    };

                    _cartesianResisvityChart.Series = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Values = ResivityChartValues,
                            PointGeometry = null,
                            DataLabels = false
                        },
                    };
                    _cartesianResisvityChart.AxisY.Add(new Axis
                    {
                        Title = "电阻率(%)",

                    });
                    _cartesianResisvityChart.AxisX.Add(new Axis
                    {
                        Title = "长度(mm)",
                      
                    }
                    );
                    _cartesianResisvityChart.DisableAnimations = true;
                    _cartesianResisvityChart.DataTooltip = null;
                    _cartesianResisvityChart.Hoverable = false;
                    _cartesianResisvityChart.Zoom = ZoomingOptions.Y;
                }
                #endregion

                #region 透过率初始化
                {
                    PenetrationRateChartValues = new ChartValues<ObservablePoint>
                    {
                        new ObservablePoint(0, 0)
                    };

                    _cartesianPenetrationRateResultView.Series = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Values = PenetrationRateChartValues,
                             PointGeometry = null,
                            DataLabels = false
                        },
                    };
                    _cartesianPenetrationRateResultView.AxisY.Add(new Axis
                    {
                        Title = "透过率(%)",
                    });
                    _cartesianPenetrationRateResultView.AxisX.Add(new Axis
                    {
                        Title = "长度(mm)",
                    }
                    );
                    _cartesianPenetrationRateResultView.DisableAnimations = true;
                    _cartesianPenetrationRateResultView.DataTooltip = null;
                    _cartesianPenetrationRateResultView.Hoverable = false;
                    _cartesianPenetrationRateResultView.Zoom = ZoomingOptions.Y;
                }
                #endregion
            }
            catch (Exception ex)
            {

            }
           
        }

        private void RadiusAxissection_MouseUp(object sender, System.Windows.Input.MouseEventArgs args)
        {
            try
            {
                string strName = AxisSection.Dragging.Name;
                 LogHelper.Info("Silicon", "RadiusAxissection_MouseUp name " + strName);

                int nIndex = 0;
                string strIndex = "";
                strIndex = strName.Substring(strName.IndexOf('_') + 1);

                nIndex = int.Parse(strIndex);

                if (strName.Contains("Begin"))
                {
                    _axisRadiusAreassection[nIndex].Value = _axisRadiusBeginsection[nIndex].Value;
                    _axisRadiusAreassection[nIndex].SectionWidth = _axisRadiusEndssection[nIndex].Value - _axisRadiusBeginsection[nIndex].Value;
                }
                else if (strName.Contains("End"))
                {
                    _axisRadiusAreassection[nIndex].SectionWidth = _axisRadiusEndssection[nIndex].Value - _axisRadiusBeginsection[nIndex].Value;

                }

            }
            catch (Exception ex)
            {
                 LogHelper.Info("Silicon", "RadiusAxissection_MouseUp exception " + ex.Message);
            }



        }


        private void Axissection_MouseMove(object sender, System.Windows.Input.MouseEventArgs args)
        {
            Debug.WriteLine("Axiendssection_MouseMove Click");
        }

        private void RadiusAxissection_MouseDown(object sender, System.Windows.Input.MouseEventArgs args)
        {
            Debug.WriteLine("Axiendssection_MouseDown Click");
        }
        private void YingLiAxissection_MouseDown(object sender, System.Windows.Input.MouseEventArgs args)
        {
            Debug.WriteLine("Axiendssection_MouseDown Click");
        }


        private void YingLiAxissection_MouseUp(object sender, System.Windows.Input.MouseEventArgs args)
        {
            try
            {
                string strName = AxisSection.Dragging.Name;
                 LogHelper.Info("Silicon", "YingLiAxissection_MouseUp name " + strName);

                int nIndex = 0;
                string strIndex = "";
                strIndex = strName.Substring(strName.IndexOf('_') + 1);

                nIndex = int.Parse(strIndex);

                if (strName.Contains("Begin"))
                {
                    _axisYingLiAreassection[nIndex].Value = _axisYingLiBeginsection[nIndex].Value;
                    _axisYingLiAreassection[nIndex].SectionWidth = _axisYingLiEndssection[nIndex].Value - _axisYingLiBeginsection[nIndex].Value;
                }
                else if (strName.Contains("End"))
                {
                    _axisYingLiAreassection[nIndex].SectionWidth = _axisYingLiEndssection[nIndex].Value - _axisYingLiBeginsection[nIndex].Value;
                }

            }
            catch (Exception ex)
            {
                 LogHelper.Info("Silicon", "YingLiAxissection_MouseUp exception " + ex.Message);
            }



        }

        private void InitRadiusResultSeries()
        {
            try
            {

                float fMaxRadius = SettingParameter.Instance().FSiliconStickMaxRaidus;
                float fMinRadius = SettingParameter.Instance().FSiliconStickMinRaidus;
                double dBeginIndex = -1;
                double dEndIndex = -1;
                ArrayList subNormalRaidus = new ArrayList();

                foreach (ObservablePoint value in RadiusChartValues)
                {
                    if (value.Y < fMinRadius || value.Y > fMaxRadius)
                    {
                        if (dBeginIndex == -1)
                        {
                            dBeginIndex = value.X;
                        }
                    }
                    else if (dBeginIndex != -1)
                    {
                        dEndIndex = value.X;
                        float[] fValue = new float[2];
                        fValue[0] = (float)dBeginIndex;
                        fValue[1] = (float)dEndIndex;
                        subNormalRaidus.Add(fValue);
                        dBeginIndex = -1;
                    }
                }

                if (dBeginIndex != -1)
                {
                    dEndIndex = RadiusChartValues[RadiusChartValues.Count - 1].X;
                    float[] fValue = new float[2];
                    fValue[0] = (float)dBeginIndex;
                    fValue[1] = (float)dEndIndex;

                    subNormalRaidus.Add(fValue);
                }


                _axisRadiusBeginsection = new AxisSection[subNormalRaidus.Count];
                _axisRadiusAreassection = new AxisSection[subNormalRaidus.Count];
                _axisRadiusEndssection = new AxisSection[subNormalRaidus.Count];
                for (int i = 0; i < subNormalRaidus.Count; i++)
                {
                    float[] fngLengths = (float[])subNormalRaidus[i];
                    _axisRadiusBeginsection[i] = new AxisSection
                    {
                        Stroke = System.Windows.Media.Brushes.Black,//colocr
                        StrokeThickness = 5,
                        Value = fngLengths[0],//Modify this.
                        Draggable = true,
                        AllowDrop = true,
                        DisableAnimations = true,
                        Name = "Begin_" + i.ToString(),

                    };
                    _axisRadiusAreassection[i] = new AxisSection
                    {
                        Name = "Area_" + i.ToString(),
                        Value = fngLengths[0],
                        SectionWidth = (fngLengths[1] - fngLengths[0]),
                        Fill = new SolidColorBrush
                        {
                            Color = System.Windows.Media.Color.FromRgb(204, 204, 204),
                            Opacity = .4
                        }
                    };
                    _axisRadiusEndssection[i] = new AxisSection
                    {
                        Stroke = System.Windows.Media.Brushes.Black,//colocr
                        StrokeThickness = 5,
                        Value = fngLengths[1],//Modify this.
                        Draggable = true,
                        AllowDrop = true,
                        DisableAnimations = true,
                        Name = "End_" + i.ToString(),
                    };

                    _axisRadiusBeginsection[i].MouseUpFunc += RadiusAxissection_MouseUp;
                    _axisRadiusEndssection[i].MouseUpFunc += RadiusAxissection_MouseUp;
                    _axisRadiusBeginsection[i].MouseDownFunc += RadiusAxissection_MouseDown;
                    _axisRadiusEndssection[i].MouseDownFunc += RadiusAxissection_MouseDown;


                }


                for (int i = 0; i < subNormalRaidus.Count; i++)
                {
                    _cartesianRadiusChart.AxisX[0].Sections.Add(_axisRadiusAreassection[i]);
                    _cartesianRadiusChart.AxisX[0].Sections.Add(_axisRadiusBeginsection[i]);
                    _cartesianRadiusChart.AxisX[0].Sections.Add(_axisRadiusEndssection[i]);
                }

                subNormalRaidus.Clear();
                _cartesianRadiusChart.Update();

                _labelMinRadius.Text = "最小直径：" + _fMaxRadius.ToString("0.00");
                _labelMaxRadius.Text = "最大直径：" + _fMinRadius.ToString("0.00");
                _labelLength.Text = "长度: " + _fLength.ToString("0.00");
                _labelValidLength.Text = "有效长度: " + _fValidLength.ToString("0.00");
            }
            catch (Exception ex)
            {

            }


        }


        private void InitResult()
        {
            try
            {
                _preShowRadius = new ArrayList();
                _preShowResisvity = new ArrayList();
                _preShowPenetrationRate = new ArrayList();
                _preShowYingLi = new ArrayList();

                string strStickresultTableName = "stickresultinfo_"+_curDate.Year.ToString() + _curDate.Month.ToString() + _curDate.Day.ToString();
                List<InspectResultLineInfo> _curLines = CMySQLTool.Instance().SearchResultLines(SettingParameter.Instance().StrMySQLDBName, strStickresultTableName, _curresult.StrSiliconStickNum);

                if (_curLines.Count > 0 )
                {
                    _fMinRadius = _curLines[0].FAppearanceMinRadius;
                    _fMaxRadius = _curLines[0].FAppearanceMaxRadius;
                    _fLength = _curLines[0].FApperanceLength;
                    _fValidLength = _curLines[0].FAppearanceValidLength;
                }
                string strRadiusTableName = "radius_" + _curDate.Year.ToString() + _curDate.Month.ToString() + _curDate.Day.ToString();
                _resultRadius = CMySQLTool.Instance().SearchRadiusInfoByStickLineNum(SettingParameter.Instance().StrMySQLDBName, strRadiusTableName, _curresult.StrSiliconStickNum);

                string strYingLiTableName = "yingli_" + _curDate.Year.ToString() + _curDate.Month.ToString() + _curDate.Day.ToString();
                _resultYingLi = CMySQLTool.Instance().SearchYingLiInfoByStickLineNum(SettingParameter.Instance().StrMySQLDBName, strYingLiTableName, _curresult.StrSiliconStickNum);
                string strPenetrationrateTableName = "penetrationrate_" + _curDate.Year.ToString() + _curDate.Month.ToString() + _curDate.Day.ToString();
                _resultPenetration = CMySQLTool.Instance().SearchPenetrationrateInfoByStickLineNum(SettingParameter.Instance().StrMySQLDBName, strPenetrationrateTableName, _curresult.StrSiliconStickNum);
                string strresistivityTableName = "resistivity_" + _curDate.Year.ToString() + _curDate.Month.ToString() + _curDate.Day.ToString();
                _resultResisvity = CMySQLTool.Instance().SearchResisvityInfoByStickLineNum(SettingParameter.Instance().StrMySQLDBName, strresistivityTableName, _curresult.StrSiliconStickNum);

                _arrAbnormalArea = new ArrayList();

                string[] strInfo;
                if (_curresult.StrAbnormalFir.Length > 0)
                {
                    strInfo = _curresult.StrAbnormalFir.Split(",");

                    List<float> fInfo = new List<float>();

                    fInfo.Add(float.Parse(strInfo[0]));
                    fInfo.Add(float.Parse(strInfo[1]));

                    if (fInfo[0] != 0 || fInfo[1] != 0)
                    {
                        _arrAbnormalArea.Add(fInfo);
                    }
                }


                if (_curresult.StrAbnormalSec.Length > 0)
                {
                    strInfo = _curresult.StrAbnormalSec.Split(",");

                    List<float> fInfo = new List<float>();

                    fInfo.Add(float.Parse(strInfo[0]));
                    fInfo.Add(float.Parse(strInfo[1]));

                    if (fInfo[0] != 0 || fInfo[1] != 0)
                    {
                        _arrAbnormalArea.Add(fInfo);
                    }
                }

                if (_curresult.StrAbnormalThr.Length > 0)
                {
                    strInfo = _curresult.StrAbnormalThr.Split(",");

                    List<float> fInfo = new List<float>();

                    fInfo.Add(float.Parse(strInfo[0]));
                    fInfo.Add(float.Parse(strInfo[1]));

                    if (fInfo[0] != 0 || fInfo[1] != 0)
                    {
                        _arrAbnormalArea.Add(fInfo);
                    }
                }


                if (_curresult.StrAbnormalFour.Length > 0)
                {
                    strInfo = _curresult.StrAbnormalFour.Split(",");

                    List<float> fInfo = new List<float>();

                    fInfo.Add(float.Parse(strInfo[0]));
                    fInfo.Add(float.Parse(strInfo[1]));

                    if (fInfo[0] != 0 || fInfo[1] != 0)
                    {
                        _arrAbnormalArea.Add(fInfo);
                    }
                }


                if (_curresult.StrResultPath != null && _curresult.StrResultPath.Length > 0)
                {
                    Bitmap _bitResult = new Bitmap(_curresult.StrResultPath);
                    this.pictureBoxResult.Image = _bitResult;
                    this.pictureBoxResult.PreImage = _bitResult;
                }

            }
            catch (Exception ex)
            {
                 LogHelper.Info("Silicon","Init Result " + ex.Message);
            }
          
        }

       
    }
}
