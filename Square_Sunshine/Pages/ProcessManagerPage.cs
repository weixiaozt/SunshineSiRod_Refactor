using HalconDotNet;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using SiliconRoundBarCheck.Cameras;
using SquareSiliconStickCheck.Data;
using SquareSiliconStickCheck.Parameters;
using SquareSiliconStickCheck.Tools;
using Sunny.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace SquareSiliconStickCheck.Pages
{
    public partial class ProcessManagerPage : UIPage
    {
        public delegate void SwidthItem();

        public delegate void RefreshViewDele(int nScanIndex);

        public delegate void RefreshMainVideoDelegate(int nIndexView);

        public delegate void StopThradDelegate();

        public delegate void ShowRightButtonDelegate(bool bShow);

        public delegate void TriggerBVDelegate(int nID);

        public delegate void InitDevDelegate(int nType);

        public TriggerBVDelegate triggerBVFunc;

        public ShowRightButtonDelegate ShowRightBtnFunc;

        public InitDevDelegate initDevFunc;
        
        public SwidthItem itemFunc;

        public RefreshViewDele refreshFunc;

        public RefreshMainVideoDelegate refreshMainVideoFunc;

        public StopThradDelegate stopThradFunc;

        public delegate void SwitchStartBtnState(bool bEnable);


        public SwitchStartBtnState stateFunc;

        private InspectResult _curresult = new InspectResult(); 

        private int _nSwidthIndex = 0;

        private int _nTestID;

        private object _continuestatelock = new object();

        private bool _bContinueState = true;

        private Thread m_threadScanSiliconStick = null;
        private Thread m_threadRefreshVideoView = null;
        private Thread m_threadScanRadius = null;
        private Thread m_threadWaitStartedSignal = null;


        private StickData _curStickData = null;
        private Dictionary<string, ArrayList> _dicSearialAndStickDataArray = null;

        public ChartValues<ObservablePoint> _leftChartValues { get; set; }
        public ChartValues<ObservablePoint> _rightChartValues { get; set; }
        public ChartValues<ObservablePoint> _topChartValues { get; set; }
        public ChartValues<ObservablePoint> _downChartValues { get; set; }

        public bool BContinueState
        {
            get
            {
                bool bState = false;
                lock (_continuestatelock)
                {
                    bState = _bContinueState;
                }
                return bState;
            }
            set
            {
                lock (_continuestatelock)
                {
                    _bContinueState = value;
                }
            }

        }

        private ArrayList[] _subLJLengthInfo = new ArrayList[4];
        private ArrayList[] _subLJPictureIndexInfo = new ArrayList[4];

        private ArrayList[] _subYinLieLengthInfo = new ArrayList[4];
        private ArrayList[] _subYinLiePictureIndexInfo = new ArrayList[4];

        private ArrayList[] _subYingLiLengthInfo = new ArrayList[4];
        private ArrayList[] _subYingLiPictureIndexInfo = new ArrayList[4];


        private ArrayList[] _subYinLiLengthInfo = new ArrayList[4];
        private ArrayList[] _subYinLiPictureIndexInfo = new ArrayList[4];

        private ArrayList _subTotalLJLengthInfo = new ArrayList();
        private ArrayList _subTotalYinLieLengthInfo = new ArrayList();
        private ArrayList _subTotalYingLiLengthInfo = new ArrayList();
        private ArrayList _subTotalRadiusLengthInfo = new ArrayList();
        private ArrayList _subTotalRadiusIndexInfo = new ArrayList();
        private ArrayList _subTotalFullLengthInfo = new ArrayList();

        private ArrayList _subTotalPictureIndexInfo = new ArrayList();
        private System.Windows.Forms.Timer _timeCheckDataRefresh;
        private StickData _leftStickData;
        private StickData _rightStickData;
        private StickData _topStickData;
        private StickData _downStickData;

        
        private System.Windows.Forms.Timer _timeLeftRefresh;

        private System.Windows.Forms.Timer _timeRightRefresh;

        private System.Windows.Forms.Timer _timeTopRefresh;

        private System.Windows.Forms.Timer _timeDownRefresh;

        private System.Windows.Forms.Timer _timeLeftBeginRefresh;

        private System.Windows.Forms.Timer _timeTopBeginRefresh;

        private System.Windows.Forms.Timer _timeDownBeginRefresh;

        private System.Windows.Forms.Timer _timeRightBeginRefresh;

        private int _preIndexOfLeftLineLengthView = 0;
        private int _preIndexOfRightLineLengthView = 0;
        private int _preIndexOfTopLineLengthView = 0;
        private int _preIndexOfDownLineLengthView = 0;

        private float _preMinValueOfLeftLineLengthView = 0;
        private float _preMaxValueOfLeftLineLengthView = 0;
        private float _preMinValueOfRightLineLengthView = 0;
        private float _preMaxValueOfRightLineLengthView = 0;
        private float _preMinValueOfTopLineLengthView = 0;
        private float _preMaxValueOfTopLineLengthView = 0;
        private float _preMinValueOfDownLineLengthView = 0;
        private float _preMaxValueOfDownLineLengthView = 0;

        private int _preIndexOfLeftAngleView = 0;
        private int _preIndexOfRightAngleView = 0;
        private int _preIndexOfTopAngleView = 0;
        private int _preIndexOfDownAngleView = 0;


        private float _preMinValueOfLeftLineLength = 65535;
        private float _preMaxValueOfLeftLineLength = -1;
        private float _preMinValueOfRightLineLength = 65535;
        private float _preMaxValueOfRightLineLength = -1;
        private float _preMinValueOfTopLineLength = 65535;
        private float _preMaxValueOfTopLineLength = -1;
        private float _preMinValueOfDownLineLength = 65535;
        private float _preMaxValueOfDownLineLength = -1;

        private float _preMinValueOfLeftAngle = 65535;
        private float _preMaxValueOfLeftAngle = -1;
        private float _preMinValueOfRightAngle = 65535;
        private float _preMaxValueOfRightAngle = -1;
        private float _preMinValueOfTopAngle = 65535;
        private float _preMaxValueOfTopAngle = -1;
        private float _preMinValueOfDownAngle = 65535;
        private float _preMaxValueOfDownAngle = -1;

        private string _strSiliconSerialNum = "";
        private bool _bGotLeftSiliconData = false;
        private bool _bGotRightSiliconData = false;
        private bool _bGotTopSiliconData = false;
        private bool _bGotDownSiliconData = false;


        private const int _showCountOfPoints = 300;

        public static ProcessManagerPage instance;


        private AxisSection[] _axisbeginsection;
        private AxisSection[] _areaSection;
        private AxisSection[] _axisendssection;


        public ChartValues<ObservablePoint> LeftChartViewLineLengthValues { get; set; }
        public ChartValues<ObservablePoint> RightChartViewLineLengthValues { get; set; }
        public ChartValues<ObservablePoint> TopChartViewLineLengthValues { get; set; }
        public ChartValues<ObservablePoint> DownChartViewLineLengthValues { get; set; }

        public ChartValues<ObservablePoint> RightChartViewHypotenuseLengthValues { get; set; }

        public ChartValues<ObservablePoint> LeftChartViewHypotenuseLengthValues { get; set; }

        public ChartValues<ObservablePoint> TopChartViewHypotenuseLengthValues { get; set; }

        public ChartValues<ObservablePoint> DownChartViewHypotenuseLengthValues { get; set; }

        public ChartValues<ObservablePoint> LeftChartViewFirstAnglesValues { get; set; }

        public ChartValues<ObservablePoint> LeftChartViewSecondAnglesValues { get; set; }

        public ChartValues<ObservablePoint> RightChartViewFirstAnglesValues { get; set; }

        public ChartValues<ObservablePoint> RightChartViewSecondAnglesValues { get; set; }

        public ChartValues<ObservablePoint> TopChartViewFirstAnglesValues { get; set; }

        public ChartValues<ObservablePoint> TopChartViewSecondAnglesValues { get; set; }

        public ChartValues<ObservablePoint> DownChartViewFirstAnglesValues { get; set; }

        public ChartValues<ObservablePoint> DownChartViewSecondAnglesValues { get; set; }



        public ProcessManagerPage()
        {
            instance = this;
            ShowRightBtnFunc = new ShowRightButtonDelegate(ShowRightButton);
            initDevFunc = new InitDevDelegate(InitDevFunction);
            triggerBVFunc = new TriggerBVDelegate(TriggerBVFunction);
           
            refreshFunc = new RefreshViewDele(RefreshViewFunc);
            refreshMainVideoFunc = new RefreshMainVideoDelegate(RefrshMainVideo);
            stopThradFunc = new StopThradDelegate(Stop);
            _nNoCollapsedRadiusViewHeight = 700;
            _nCollapsedRadiusViewHeight = 35;
            _nSplitViewHeight = 10; 
            _nBeginRaidusViewY = 40;
            _nRadiusViewWidth = 1600;
            _nRadiusViewPictureboxWidth = 1500;
            _nRadiusViewPictureboxHeight = 180;
            _nRadiusViewResultPictureboxHeight = 180;
            _nMainPanelViewWidth = 1650;
            _nMainPanelViewHeight = 900;
            InitializeComponent();

            _dicSearialAndStickDataArray = new Dictionary<string, ArrayList>();

            CheckForIllegalCrossThreadCalls = false;
            if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
            {
                if (false == InitCameras())
                {
                    
                    FormMain.formMainF.showMessageDelegate("相机初始化失败！需重新初始化！");

                    Thread.Sleep(1000);

                    if (false == InitCameras())
                    {
                        FormMain.formMainF.showMessageDelegate("初始化失败，需重启程序!", (int)FormMain.emMSGTYPE.EM_PROGRAME_RESTART);
                        return;
                    }
                }

            }

            #region 全图初始化
            {

                #region 左相机数据初始化
                LeftChartViewLineLengthValues = new ChartValues<ObservablePoint>{
                    new ObservablePoint(0, 0)
                };


                _cartesianChartResultLineLengthView.Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Values = LeftChartViewLineLengthValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                    new LineSeries
                    {
                        Values = RightChartViewLineLengthValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                    new LineSeries
                    {
                        Values = TopChartViewLineLengthValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                    new LineSeries
                    {
                        Values = DownChartViewLineLengthValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                };

                if (_cartesianChartResultLineLengthView.AxisY.Count > 0)
                {
                    _cartesianChartResultLineLengthView.AxisY[0].Title = "边长(mm)";
                }
                else
                {
                    _cartesianChartResultLineLengthView.AxisY.Add(new Axis()
                    {
                        Title = "边长(mm)"
                    });

                }

                if (_cartesianChartResultLineLengthView.AxisX.Count > 0)
                {
                    _cartesianChartResultLineLengthView.AxisX[0].Title = "长度(mm)";
                }
                else
                {
                    _cartesianChartResultLineLengthView.AxisX.Add(new Axis()
                    {
                        Title = "长度(mm)"
                    });
                }


                _cartesianChartResultLineLengthView.DisableAnimations = true;
                _cartesianChartResultLineLengthView.DataTooltip = null;
                _cartesianChartResultLineLengthView.Hoverable = false;
                _cartesianChartResultLineLengthView.Zoom = ZoomingOptions.Y;
                _cartesianChartResultLineLengthView.AxisY[0].MinValue = 0;
                _cartesianChartResultLineLengthView.AxisY[0].MaxValue = 10;
                #endregion

                #region  电阻率初始化
                RightChartViewHypotenuseLengthValues = new ChartValues<ObservablePoint>{
                    new ObservablePoint(0, 0)
                };


                _cartesianChartHypotenuseLengthResultView.Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Values = RightChartViewHypotenuseLengthValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                    new LineSeries
                    {
                        Values = LeftChartViewHypotenuseLengthValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                    new LineSeries
                    {
                        Values = TopChartViewHypotenuseLengthValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                    new LineSeries
                    {
                        Values = DownChartViewHypotenuseLengthValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                };

                if (_cartesianChartHypotenuseLengthResultView.AxisY.Count > 0)
                {
                    _cartesianChartHypotenuseLengthResultView.AxisY[0].Title = "边长(mm)";
                }
                else
                {
                    _cartesianChartHypotenuseLengthResultView.AxisY.Add(new Axis()
                    {
                        Title = "边长(mm)"
                    });

                }


                if (_cartesianChartHypotenuseLengthResultView.AxisX.Count > 0)
                {
                    _cartesianChartHypotenuseLengthResultView.AxisX[0].Title = "长度(mm)";
                }
                else
                {
                    _cartesianChartHypotenuseLengthResultView.AxisX.Add(new Axis()
                    {
                        Title = "长度(mm)"
                    });
                }


                _cartesianChartHypotenuseLengthResultView.DisableAnimations = true;
                _cartesianChartHypotenuseLengthResultView.DataTooltip = null;
                _cartesianChartHypotenuseLengthResultView.Hoverable = false;
                _cartesianChartHypotenuseLengthResultView.Zoom = ZoomingOptions.Y;
                _cartesianChartHypotenuseLengthResultView.AxisY[0].MinValue = 0;
                _cartesianChartHypotenuseLengthResultView.AxisY[0].MaxValue = 10;
                #endregion

                #region  应力初始化
                RightChartViewHypotenuseLengthValues = new ChartValues<ObservablePoint>{
                    new ObservablePoint(0, 0)
                };


                _cartesianFirstAngleChartResultView.Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Values = RightChartViewFirstAnglesValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                    new LineSeries
                    {
                        Values = LeftChartViewFirstAnglesValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                    new LineSeries
                    {
                        Values = TopChartViewFirstAnglesValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                    new LineSeries
                    {
                        Values = DownChartViewFirstAnglesValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                };
                if (_cartesianFirstAngleChartResultView.AxisY.Count > 0)
                {
                    _cartesianFirstAngleChartResultView.AxisY[0].Title = "边长(mm)";
                }
                else
                {
                    _cartesianFirstAngleChartResultView.AxisY.Add(new Axis()
                    {
                        Title = "边长(mm)"
                    });

                }


                if (_cartesianFirstAngleChartResultView.AxisX.Count > 0)
                {
                    _cartesianFirstAngleChartResultView.AxisX[0].Title = "长度(mm)";
                }
                else
                {
                    _cartesianFirstAngleChartResultView.AxisX.Add(new Axis()
                    {
                        Title = "长度(mm)"
                    });
                }


                _cartesianFirstAngleChartResultView.DisableAnimations = true;
                _cartesianFirstAngleChartResultView.DataTooltip = null;
                _cartesianFirstAngleChartResultView.Hoverable = false;
                _cartesianFirstAngleChartResultView.Zoom = ZoomingOptions.Y;
                _cartesianFirstAngleChartResultView.AxisY[0].MinValue = 0;
                _cartesianFirstAngleChartResultView.AxisY[0].MaxValue = 10;

                #endregion


                #region  下面3D相机初始化
                LeftChartViewSecondAnglesValues = new ChartValues<ObservablePoint>{
                    new ObservablePoint(0, 0)
                };


                _cartesianSecondAngleChartResultView.Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Values = LeftChartViewSecondAnglesValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                    new LineSeries
                    {
                        Values = RightChartViewSecondAnglesValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                    new LineSeries
                    {
                        Values = TopChartViewSecondAnglesValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                    new LineSeries
                    {
                        Values = DownChartViewSecondAnglesValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                };
                if (_cartesianSecondAngleChartResultView.AxisY.Count > 0)
                {
                    _cartesianSecondAngleChartResultView.AxisY[0].Title = "边长(mm)";
                }
                else
                {
                    _cartesianSecondAngleChartResultView.AxisY.Add(new Axis()
                    {
                        Title = "边长(mm)"
                    });

                }


                if (_cartesianSecondAngleChartResultView.AxisX.Count > 0)
                {
                    _cartesianSecondAngleChartResultView.AxisX[0].Title = "长度(mm)";
                }
                else
                {
                    _cartesianSecondAngleChartResultView.AxisX.Add(new Axis()
                    {
                        Title = "长度(mm)"
                    });
                }


                _cartesianSecondAngleChartResultView.DisableAnimations = true;
                _cartesianSecondAngleChartResultView.DataTooltip = null;
                _cartesianSecondAngleChartResultView.Hoverable = false;
                _cartesianSecondAngleChartResultView.Zoom = ZoomingOptions.Y;
                _cartesianSecondAngleChartResultView.AxisY[0].MinValue = 0;
                _cartesianSecondAngleChartResultView.AxisY[0].MaxValue = 10;

                #endregion

            }
            #endregion


            _timeLeftBeginRefresh = new System.Windows.Forms.Timer
            {
                Interval = 100,
            };
            _timeLeftBeginRefresh.Tick += _timeLeftBeginRefresh_Tick;

            _timeLeftRefresh  = new System.Windows.Forms.Timer
            {
                Interval = 100,
            };
            _timeLeftRefresh.Tick += _timeLeftRefresh_Tick;


            _timeDownBeginRefresh = new System.Windows.Forms.Timer
            {
                Interval = 100,
            };
            _timeDownBeginRefresh.Tick += _timeDownBeginRefresh_Tick;

            _timeDownRefresh = new System.Windows.Forms.Timer
            {
                Interval = 100,
            };
            _timeDownRefresh.Tick += _timeDownRefresh_Tick;


            _timeTopBeginRefresh = new System.Windows.Forms.Timer
            {
                Interval = 100,
            };
            _timeTopBeginRefresh.Tick += _timeTopBeginRefresh_Tick;

            _timeTopRefresh = new System.Windows.Forms.Timer
            {
                Interval = 100,
            };
            _timeTopRefresh.Tick += _timeTopRefresh_Tick;

            _timeRightBeginRefresh = new System.Windows.Forms.Timer 
            {
                Interval = 100, 
            };

            _timeRightBeginRefresh.Tick += _timeRightBeginRefresh_Tick;

            _timeRightRefresh = new System.Windows.Forms.Timer
            {
                Interval = 100,
            };
            _timeRightRefresh.Tick += _timeRightRefresh_Tick;

           
        }


        private void _timeRightRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_preIndexOfRightLineLengthView > 0)
                {
                    float fMinValue = _preMinValueOfRightLineLength;
                    float fMaxValue = _preMaxValueOfRightLineLength;
                    int nShowIndex = 0;
                    ChartValues<ObservablePoint> ptValues = new ChartValues<ObservablePoint>();

                    for (; _preIndexOfRightLineLengthView < _curStickData.RighthypotenuseLengthInfo.Count; _preIndexOfRightLineLengthView++)
                    {
                        ptValues.Add(new ObservablePoint((int)(_preIndexOfRightLineLengthView /** 0.23*/), (float)_curStickData.RighthypotenuseLengthInfo[_preIndexOfRightLineLengthView]));
                        nShowIndex++;
                        if ((float)_curStickData.RighthypotenuseLengthInfo[_preIndexOfRightLineLengthView] < fMinValue)
                        {
                            fMinValue = (float)_curStickData.RighthypotenuseLengthInfo[_preIndexOfRightLineLengthView];
                        }

                        if ((float)_curStickData.RighthypotenuseLengthInfo[_preIndexOfRightLineLengthView] > fMaxValue)
                        {
                            fMaxValue = (float)_curStickData.RighthypotenuseLengthInfo[_preIndexOfRightLineLengthView];
                        }
                        if (nShowIndex >= _showCountOfPoints)
                        {
                            break;
                        }
                    }

                    RightChartViewHypotenuseLengthValues.AddRange(ptValues);
                    _preMinValueOfRightLineLength = fMinValue;
                    _preMaxValueOfRightLineLength = fMaxValue;

                    if (_preIndexOfRightLineLengthView >= _curStickData.RighthypotenuseLengthInfo.Count)
                    {
                        _preIndexOfRightLineLengthView = 0;
                        _timeRightRefresh.Stop();
                    }

                    _cartesianFirstAngleChartResultView.AxisY[0].MinValue = Math.Max(0, fMinValue - 1);
                    _cartesianFirstAngleChartResultView.AxisY[0].MaxValue = fMaxValue + 3;
                    _cartesianFirstAngleChartResultView.Update();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void _timeTopRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_preIndexOfTopLineLengthView > 0)
                {
                    float fMinValue = _preMinValueOfTopLineLength;
                    float fMaxValue = _preMaxValueOfTopLineLength;
                    int nShowIndex = 0;
                    ChartValues<ObservablePoint> ptValues = new ChartValues<ObservablePoint>();

                    for (; _preIndexOfTopLineLengthView < _curStickData.TopHypotenuseLengthInfo.Count; _preIndexOfTopLineLengthView++)
                    {
                        ptValues.Add(new ObservablePoint((int)(_preIndexOfTopLineLengthView /** 0.23*/), (float)_curStickData.TopHypotenuseLengthInfo[_preIndexOfTopLineLengthView]));
                        nShowIndex++;
                        if ((float)_curStickData.TopHypotenuseLengthInfo[_preIndexOfTopLineLengthView] < fMinValue)
                        {
                            fMinValue = (float)_curStickData.TopHypotenuseLengthInfo[_preIndexOfTopLineLengthView];
                        }

                        if ((float)_curStickData.TopHypotenuseLengthInfo[_preIndexOfTopLineLengthView] > fMaxValue)
                        {
                            fMaxValue = (float)_curStickData.TopHypotenuseLengthInfo[_preIndexOfTopLineLengthView];
                        }
                        if (nShowIndex >= _showCountOfPoints)
                        {
                            break;
                        }
                    }

                    LeftChartViewLineLengthValues.AddRange(ptValues);
                    _preMinValueOfTopLineLength = fMinValue;
                    _preMaxValueOfTopLineLength = fMaxValue;

                    if (_preIndexOfTopLineLengthView >= _curStickData.TopHypotenuseLengthInfo.Count)
                    {
                        _preIndexOfTopLineLengthView = 0;
                        _timeTopRefresh.Stop();
                    }

                    _cartesianChartHypotenuseLengthResultView.AxisY[0].MinValue = Math.Max(0, fMinValue - 1);
                    _cartesianChartHypotenuseLengthResultView.AxisY[0].MaxValue = fMaxValue + 3;
                    _cartesianChartHypotenuseLengthResultView.Update();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void _timeDownRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_preIndexOfDownLineLengthView > 0)
                {
                    float fMinValue = _preMinValueOfDownLineLength;
                    float fMaxValue = _preMaxValueOfDownLineLength;
                    int nShowIndex = 0;
                    ChartValues<ObservablePoint> ptValues = new ChartValues<ObservablePoint>();

                    for (; _preIndexOfDownLineLengthView < _curStickData.DownhypotenuseLengthInfo.Count; _preIndexOfDownLineLengthView++)
                    {
                        ptValues.Add(new ObservablePoint((int)(_preIndexOfDownLineLengthView /** 0.23*/), (float)_curStickData.DownhypotenuseLengthInfo[_preIndexOfDownLineLengthView]));
                        nShowIndex++;
                        if ((float)_curStickData.DownhypotenuseLengthInfo[_preIndexOfDownLineLengthView] < fMinValue)
                        {
                            fMinValue = (float)_curStickData.DownhypotenuseLengthInfo[_preIndexOfDownLineLengthView];
                        }

                        if ((float)_curStickData.DownhypotenuseLengthInfo[_preIndexOfDownLineLengthView] > fMaxValue)
                        {
                            fMaxValue = (float)_curStickData.DownhypotenuseLengthInfo[_preIndexOfDownLineLengthView];
                        }
                        if (nShowIndex >= _showCountOfPoints)
                        {
                            break;
                        }
                    }

                    LeftChartViewSecondAnglesValues.AddRange(ptValues);
                    _preMinValueOfDownLineLength = fMinValue;
                    _preMaxValueOfDownLineLength = fMaxValue;

                    if (_preIndexOfDownLineLengthView >= _curStickData.DownhypotenuseLengthInfo.Count)
                    {
                        _preIndexOfDownLineLengthView = 0;
                        _timeDownRefresh.Stop();
                    }

                    _cartesianSecondAngleChartResultView.AxisY[0].MinValue = Math.Max(0, fMinValue - 1);
                    _cartesianSecondAngleChartResultView.AxisY[0].MaxValue = fMaxValue + 3;
                    _cartesianSecondAngleChartResultView.Update();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void _timeLeftRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_preIndexOfLeftLineLengthView > 0)
                {
                    float fMinValue = _preMinValueOfLeftLineLength;
                    float fMaxValue = _preMaxValueOfLeftLineLength;
                    int nShowIndex = 0;
                    ChartValues<ObservablePoint> ptValues = new ChartValues<ObservablePoint>();

                    for (; _preIndexOfLeftLineLengthView < _curStickData.LeftdownlineLengthInfo.Count; _preIndexOfLeftLineLengthView++)
                    {
                        ptValues.Add(new ObservablePoint((int)(_preIndexOfLeftLineLengthView /** 0.23*/), (float)_curStickData.LeftdownlineLengthInfo[_preIndexOfLeftLineLengthView]));
                        nShowIndex++;
                        if ((float)_curStickData.LeftdownlineLengthInfo[_preIndexOfLeftLineLengthView] < fMinValue)
                        {
                            fMinValue = (float)_curStickData.LeftdownlineLengthInfo[_preIndexOfLeftLineLengthView];
                        }

                        if ((float)_curStickData.LeftdownlineLengthInfo[_preIndexOfLeftLineLengthView] > fMaxValue)
                        {
                            fMaxValue = (float)_curStickData.LeftdownlineLengthInfo[_preIndexOfLeftLineLengthView];
                        }
                        if (nShowIndex >= _showCountOfPoints)
                        {
                            break;
                        }
                    }

                    LeftChartViewLineLengthValues.AddRange(ptValues);
                    _preMinValueOfLeftLineLength = fMinValue;
                    _preMaxValueOfLeftLineLength = fMaxValue;

                    if (_preIndexOfLeftLineLengthView >= _curStickData.LeftdownlineLengthInfo.Count)
                    {
                        _preIndexOfLeftLineLengthView = 0;
                        _timeLeftRefresh.Stop();
                    }

                    _cartesianChartResultLineLengthView.AxisY[0].MinValue = Math.Max(0, fMinValue - 1);
                    _cartesianChartResultLineLengthView.AxisY[0].MaxValue = fMaxValue + 3;
                    _cartesianChartResultLineLengthView.Update();
                }
            }
            catch (Exception ex)
            {

            }
        }

       

        private void _timeRightBeginRefresh_Tick(object sender, EventArgs e)
        {
            if (_strSiliconSerialNum != "" && _bGotRightSiliconData == false)
            {
                StickData data = new StickData();
                _bGotRightSiliconData = true;
                if (true == GlobalDataCache.Instance().GetData( _strSiliconSerialNum, ref data))
                {
                    float fMinValue = 65535;
                    float fMaxValue = -1;

                    _preIndexOfRightLineLengthView = 0;
                    ChartValues<ObservablePoint> ptChartValues = new ChartValues<ObservablePoint>();

                    foreach (var item in _curStickData.RighthypotenuseLengthInfo)
                    {
                        ptChartValues.Add(new ObservablePoint((int)(_preIndexOfRightLineLengthView /** 0.23*/), (float)item));

                        _preIndexOfRightLineLengthView++;
                        if ((float)item < fMinValue)
                        {
                            fMinValue = (float)item;
                        }

                        if ((float)item > fMaxValue)
                        {
                            fMaxValue = (float)item;
                        }
                        if (_preIndexOfRightLineLengthView >= _showCountOfPoints)
                        {
                            break;
                        }
                    }

                    RightChartViewHypotenuseLengthValues.AddRange(ptChartValues);

                    _preMinValueOfRightLineLength = fMinValue;
                    _preMaxValueOfRightLineLength = fMaxValue;
                    _cartesianFirstAngleChartResultView.AxisY[0].MinValue = Math.Max(0, fMinValue - 1);
                    _cartesianFirstAngleChartResultView.AxisY[0].MaxValue = fMaxValue + 3;
                    _cartesianFirstAngleChartResultView.Update();
                    _timeRightBeginRefresh.Stop();
                    _timeRightRefresh.Start();
                }
            }
        }

        private void _timeTopBeginRefresh_Tick(object sender, EventArgs e)
        {
            if (_strSiliconSerialNum != "" && _bGotTopSiliconData == false)
            {
                _curStickData = new StickData();
                _bGotTopSiliconData = true;
                if (true == GlobalDataCache.Instance().GetData( _strSiliconSerialNum, ref _curStickData))
                {
                    float fMinValue = 65535;
                    float fMaxValue = -1;

                    _preIndexOfTopLineLengthView = 0;
                    ChartValues<ObservablePoint> ptChartValues = new ChartValues<ObservablePoint>();

                    foreach (var item in _curStickData.TopHypotenuseLengthInfo)
                    {
                        ptChartValues.Add(new ObservablePoint((int)(_preIndexOfTopLineLengthView /** 0.23*/), (float)item));

                        _preIndexOfTopLineLengthView++;
                        if ((float)item < fMinValue)
                        {
                            fMinValue = (float)item;
                        }

                        if ((float)item > fMaxValue)
                        {
                            fMaxValue = (float)item;
                        }
                        if (_preIndexOfTopLineLengthView >= _showCountOfPoints)
                        {
                            break;
                        }
                    }

                    TopChartViewHypotenuseLengthValues.AddRange(ptChartValues);

                    _preMinValueOfTopLineLength = fMinValue;
                    _preMaxValueOfTopLineLength = fMaxValue;
                    _cartesianChartHypotenuseLengthResultView.AxisY[0].MinValue = Math.Max(0, fMinValue - 1);
                    _cartesianChartHypotenuseLengthResultView.AxisY[0].MaxValue = fMaxValue + 3;
                    _cartesianChartHypotenuseLengthResultView.Update();

                    _timeTopBeginRefresh.Stop();
                    _timeTopRefresh.Stop();
                }
            }
        }

        private void _timeDownBeginRefresh_Tick(object sender, EventArgs e)
        {
            if (_strSiliconSerialNum != "" && _bGotDownSiliconData == false)
            {
                _curStickData = new StickData();
                _bGotDownSiliconData = true;
                if (true == GlobalDataCache.Instance().GetData(_strSiliconSerialNum, ref _curStickData))
                {
                    float fMinValue = 65535;
                    float fMaxValue = -1;

                    _preIndexOfDownLineLengthView = 0;
                    ChartValues<ObservablePoint> ptChartValues = new ChartValues<ObservablePoint>();

                    foreach (var item in _curStickData.DownhypotenuseLengthInfo)
                    {
                        ptChartValues.Add(new ObservablePoint((int)(_preIndexOfDownLineLengthView /** 0.23*/), (float)item));

                        _preIndexOfDownLineLengthView++;
                        if ((float)item < fMinValue)
                        {
                            fMinValue = (float)item;
                        }

                        if ((float)item > fMaxValue)
                        {
                            fMaxValue = (float)item;
                        }
                        if (_preIndexOfDownLineLengthView >= _showCountOfPoints)
                        {
                            break;
                        }
                    }

                    DownChartViewHypotenuseLengthValues.AddRange(ptChartValues);

                    _preMinValueOfDownLineLength = fMinValue;
                    _preMaxValueOfDownLineLength = fMaxValue;
                    _cartesianSecondAngleChartResultView.AxisY[0].MinValue = Math.Max(0, fMinValue - 1);
                    _cartesianSecondAngleChartResultView.AxisY[0].MaxValue = fMaxValue + 3;
                    _cartesianSecondAngleChartResultView.Update();

                    _timeDownBeginRefresh.Stop();
                    _timeDownRefresh.Start();
                }
            }
        }

        private void _timeLeftBeginRefresh_Tick(object sender, EventArgs e)
        {
            if (_strSiliconSerialNum != "" && _bGotLeftSiliconData == false)
            {
                _curStickData = new StickData();
                _bGotLeftSiliconData = true;
                if (true == GlobalDataCache.Instance().GetData( _strSiliconSerialNum, ref _curStickData))
                {
                    float fMinValue = 65535;
                    float fMaxValue = -1;
                   
                    _preIndexOfLeftLineLengthView = 0;
                    ChartValues<ObservablePoint> ptChartValues = new ChartValues<ObservablePoint>();

                    foreach (var item in _curStickData.LefthypotenuseLengthInfo)
                    {
                        ptChartValues.Add(new ObservablePoint((int)(_preIndexOfLeftLineLengthView /** 0.23*/), (float)item));

                        _preIndexOfLeftLineLengthView++;
                        if ((float)item < fMinValue)
                        {
                            fMinValue = (float)item;
                        }

                        if ((float)item > fMaxValue)
                        {
                            fMaxValue = (float)item;
                        }
                        if (_preIndexOfLeftLineLengthView >= _showCountOfPoints)
                        {
                            break;
                        }
                    }

                    LeftChartViewLineLengthValues.AddRange(ptChartValues);

                    _preMinValueOfLeftLineLength = fMinValue;
                    _preMaxValueOfLeftLineLength = fMaxValue;
                    _cartesianChartResultLineLengthView.AxisY[0].MinValue = Math.Max(0, fMinValue - 1);
                    _cartesianChartResultLineLengthView.AxisY[0].MaxValue = fMaxValue + 3;
                    _cartesianChartResultLineLengthView.Update();

                    _timeLeftRefresh.Start();
                    _timeLeftBeginRefresh.Stop();
                }

               
            }
        }

 

        public void ShowRightButton(bool bShow)
        {
            if (bShow) 
            {
                this._rightBtn.Visible = true;
            }
            else
            {
                this._rightBtn.Visible = false;

            }

        }
       

        private void TriggerBVFunction(int nID)
        {
          
        }

        private void UpdateSquareInfoChartView()
        {
           
            while (true )
            {
                int nScanIndex = ProcessManager.Instance().NScanIndex;
                 LogHelper.Info("Silicon","UpdateVideoView Begin " + nScanIndex.ToString());

                try
                {
                    switch (nScanIndex)
                    {
                        case 0:
                            {
                              
                              

                                break;
                            }
                       
                        default:
                            {
                                break;
                            }
                    }

                }
                catch(Exception ex)
                {

                }
               
                Thread.Sleep( 1000 );
            }
            
        }
        

     
       
        public int NSwidthIndex { get => _nSwidthIndex; set => _nSwidthIndex = value; }
        public int NTestID { get => _nTestID; set => _nTestID = value; }


        public void Stop()
        {

            if (m_threadWaitStartedSignal != null && m_threadWaitStartedSignal.ThreadState != System.Threading.ThreadState.Stopped)
            {
                m_threadWaitStartedSignal.Abort();
                m_threadWaitStartedSignal = null;
            }

            if (m_threadScanRadius != null && m_threadScanRadius.ThreadState != System.Threading.ThreadState.Stopped)
            {
                m_threadScanRadius.Abort();
                m_threadScanRadius = null;
            }

            if (m_threadScanSiliconStick != null && m_threadScanSiliconStick.ThreadState != System.Threading.ThreadState.Stopped)
            {
                m_threadScanSiliconStick.Abort();
                m_threadScanSiliconStick = null;
            }

            


            if (m_threadRefreshVideoView != null && m_threadRefreshVideoView.ThreadState != System.Threading.ThreadState.Stopped)
            {
                m_threadRefreshVideoView.Abort();
                m_threadRefreshVideoView = null;
            }

           

            ProcessManager.Instance().ClearThreads();


            StopCameras();
        }



       
        

        private void RefrshMainVideo(int nIndexView)
        {
            switch(nIndexView) 
            {
             
                case 2:
                {
                    this.panelRadiusView_Full.Collapsed = true;
                  
                    break;
                }
               
            }
        }

        private void RefreshViewFunc(int nScanIndex)
        {
            try
            {
                switch (nScanIndex)
                {
                    case 0:
                        {
                           
                           
                           

                            break;
                        }
                    
                    default:
                        {
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                 LogHelper.Info("Silicon","RefreshViewFunc exception " + e.Message);
            }
            
        }
        private void ThreadScanSquareStick()
        {
            try
            {

            }
            catch(Exception ex)
            {

            }
        }
        private void ThreadScanSiliconStickMotionCardFunc()
        {
            try
            {

                HTuple hv_MessageHandle = new HTuple();
                HTuple hv_Data = new HTuple(), hv_Start = new HTuple();
                HTuple hv_Sublevel = new HTuple(), hv_ZB = new HTuple();

                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin");

                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Init Move Position Information");
                CMoveController.Instance().InitPositionInfo();
              
                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Turn On Stick End");
                CMoveController.Instance().GotoOrigin();  //回原点  离零点很近
                Thread.Sleep(1000);

               
                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin to Find SiliconLine ");
                CMoveController.Instance().GotoSiliconLinePosition();
                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Find SiliconLine End ");
               
                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin to Scan Radius ");
                //CMoveController.Instance().GotoOrigin();
                CMoveController.Instance().GotoTerninalPosition();

               

                //3D扫描
                Thread.Sleep(100);
                // Output parameters
                HObject cbho_Imageconst;

                //红外BV,3D一起 一个扫描经线，一个是直径
                // Call Measure_3D
                Thread measurethread = new Thread(() =>
                {
                    SSZNCamTools.Instance().Measure_SquareStick(_strSiliconSerialNum);
                });
                measurethread.Start();

              


                Thread.Sleep(1000);
                CMoveController.Instance().GotoTerninalPosition();

               
             
                //重新计算的尾部位置  122 位置二
                do
                {
                    if (measurethread.ThreadState != System.Threading.ThreadState.Stopped )
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }

                } while (true);

                Thread.Sleep(1000);
              

            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon", "ThreadScanSiliconStick exception " + ex.Message);
            }

        }

        private void ThreadScanSiliconSquareStickMockFunc()
        {
            try
            {
                LogHelper.Info("Silicon", "ThreadScanSiliconSquareStickMockFunc Begin");

                ProcessManager.Instance().NScanIndex = 0;

                
                ProcessManager.Instance().ScanSquareSiliconStickRoundMock(_strSiliconSerialNum);



            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon", "ThreadScanSiliconStick exception " + ex.Message);
            }
        }


        private void ThreadScanSiliconSquareStickFunc()
        {
            try
            {
                LogHelper.Info("Silicon","ThreadScanSiliconStickPLCFunc Begin");
                
                ProcessManager.Instance().NScanIndex = 0;
                
                ProcessManager.Instance().ScanSquareSiliconStickRound(_strSiliconSerialNum);
               
               
                ProcessManagerPage.instance.Invoke(refreshMainVideoFunc, 1);



            }
            catch (Exception ex)
            {
                 LogHelper.Info("Silicon","ThreadScanSiliconStick exception " + ex.Message);
            }


        }

      
        private void buttonStart_Click(object sender, System.EventArgs e)
        {
            ProcessManager.Instance().InitMatResource();

            if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
            {
              
                _strSiliconSerialNum = textBoxJB.Text;

                _timeLeftBeginRefresh.Start();
                _timeRightBeginRefresh.Start();
                _timeTopBeginRefresh.Start();
                _timeDownBeginRefresh.Start();


                if (SettingParameter.Instance().NDaemon == 1)
                {
                    m_threadScanSiliconStick = new Thread(ThreadScanSiliconSquareStickFunc);
                }
                else
                {
                    m_threadScanSiliconStick = new Thread(ThreadScanSiliconSquareStickMockFunc);
                }
                m_threadScanSiliconStick.Start();
            }
            else
            {
                
            }
          
            
            
        }

    
        private void pictureBoxYINLIE_1_Click(object sender, EventArgs e)
        {
           
        }

       

     

      

        private void pictureBoxYingLi_1_Click(object sender, EventArgs e)
        {
           
        }


        private void ProcessManagerPage_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReleaseCameras();
            ProcessManager.Instance().ClearThreads();
        }
    }
}