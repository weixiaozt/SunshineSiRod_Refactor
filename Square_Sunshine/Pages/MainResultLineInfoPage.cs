using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts;
using Sunny.UI;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Media;
using System.Threading;
using SiliconRoundBarCheck.Data;
using SiliconRoundBarCheck.Tools;
using System;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Collections;
using System.Security.Cryptography;
using System.Windows.Forms;
using YouEyEE.Untils.Log;
using SiliconRoundBarCheck.Parameters;
using SiliconRoundBarCheck.Cameras.SSZN;
using static SiliconRoundBarCheck.Pages.ProcessManagerPage;
using System.Threading.Tasks;
using HalconDotNet;
using SiliconRoundBarCheck.Cameras;
using System.Diagnostics.Eventing.Reader;

namespace SiliconRoundBarCheck.Pages
{
    public partial class MainResultLineInfoPage : UIPage
    {
        private System.Windows.Forms.Timer _timeRefresh;

        private System.Windows.Forms.Timer _timeRefreshFull;

        private System.Windows.Forms.Timer _timeRadiusRefresh;

        private System.Windows.Forms.Timer _timeYingLiRefresh;

        private System.Windows.Forms.Timer _timeResivityRefresh;



        private System.Windows.Forms.Timer _timeRadiusViewRefresh;
        private System.Windows.Forms.Timer _timeYingLiViewRefresh;
        private System.Windows.Forms.Timer _timeResivityViewRefresh;
        private System.Windows.Forms.Timer _timeStatusFormMonitorViewRefresh;

        private System.Windows.Forms.Timer _timeStatusFormMonitorRefresh;
        private System.Windows.Forms.Timer _timeRefreshResultFull;

        private System.Windows.Forms.Timer _tim;

        private System.Windows.Forms.Timer _timeInitMoveController;
        private string _strGetStickLineNum = ""; //晶编  
        public ChartValues<ObservablePoint> RadiusChartValues { get; set; }
        public ChartValues<ObservablePoint> ResivityChartValues { get; set; }
        public ChartValues<ObservablePoint> YingLiChartValues { get; set; }
        public ChartValues<ObservablePoint> PenetrationRateChartValues { get; set; }

        public ChartValues<ObservablePoint> RadiusChartViewValues { get; set; }
        public ChartValues<ObservablePoint> ResivityChartViewValues { get; set; }
        public ChartValues<ObservablePoint> YingLiChartViewValues { get; set; }

        public delegate void ShowRightButtonDelegate(bool bShow);
        public StopThradDelegate stopThradFunc;
        public static MainResultLineInfoPage instance;
        public ShowRightButtonDelegate ShowRightBtnFunc;
        private AxisSection[] _axisRadiusBeginsection;
        private AxisSection[] _axisRadiusEndssection;
        private AxisSection[] _axisRadiusAreassection;
        private AxisSection[] _axisYingLiBeginsection;
        private AxisSection[] _axisYingLiEndssection;
        private AxisSection[] _axisYingLiAreassection;

        private StickData _curStickData;

      
        private int _preIndexOfRadius = 0;
        private int _preIndexOfYingLi = 0;
        private int _preIndexOfResisvity = 0;

        private int _preIndexOfRadiusView = 0;
        private int _preIndexOfYingliView = 0;
        private int _preIndexOfResisvityView = 0;

        private float _preMinValueOfRadius = 65535;
        private float _preMaxValueOfRadius = -1;
        private float _preMinValueOfYingLi = 65535;
        private float _preMaxValueOfYingLi = -1;
        private float _preMinValueOfResisvity = 65535;
        private float _preMaxValueOfResisvity = -1;
        private float _preMinValueOfRadiusView = 65535;
        private float _preMaxValueOfRadiusView = -1;
        private float _preMinValueOfYingLiView = 65535;
        private float _preMaxValueOfYingLiView = -1;
        private float _preMinValueOfResisvityView = 65535;
        private float _preMaxValueOfResisvityView = -1;

        private HTuple hv_OutResultListHandle;
        private const int _showCountOfPoints = 1000;
        private UIWaitForm _statusForm = null;
        public InitDevDelegate initDevFunc;
        Thread m_threadScanRadius = null;
        private int _nSwidthIndex = 0;
        public int NSwidthIndex { get => _nSwidthIndex; set => _nSwidthIndex = value; }

        public enum emCameraType
        {
            EM_HIKVIDEO = 0,
            EM_HIKLJ = 1,
            EM_BVYINLIE = 2,
            EM_BVYINGLI = 3,
        };

        private void InitDevFunction(int nType)
        {
            switch (nType)
            {
                case (int)emCameraType.EM_HIKVIDEO:
                    {
                        CHikCameraTools.Instance().InitDev(SettingParameter.Instance().StrIPVideo, (int)CHikCameraTools.emHikCameraType.emVideo, null);
                        break;
                    }
                case (int)emCameraType.EM_HIKLJ:
                    {
                        CHikCameraTools.Instance().InitDev(SettingParameter.Instance().StrIPLJ, (int)CHikCameraTools.emHikCameraType.emLJ, null);
                        break;
                    }
                case (int)emCameraType.EM_BVYINLIE:
                    {
                        CBVCameraTools.Instance().InitBVCameraByUID(SettingParameter.Instance().StrYinLieUID, (int)CBVCameraTools.emBVCameraType.emBV_YL, null);
                        break;
                    }
                case (int)emCameraType.EM_BVYINGLI:
                    {
                        CBVCameraTools.Instance().InitBVCameraByUID(SettingParameter.Instance().StrYingLiUID, (int)CBVCameraTools.emBVCameraType.emBV_YingLi, null);
                        break;
                    }
            }
        }

        public void Stop()
        {

            if (m_threadScanStick != null && m_threadScanStick.ThreadState != System.Threading.ThreadState.Stopped)
            {
                m_threadScanStick.Abort();
                m_threadScanStick = null;
            }

            if (m_threadScanRadius != null && m_threadScanRadius.ThreadState != System.Threading.ThreadState.Stopped)
            {
                m_threadScanRadius.Abort();
                m_threadScanRadius = null;
            }

            
        }

        public MainResultLineInfoPage()
        {
            InitializeComponent();

            CMoveController.Instance().CurPage = this;
            this.stateFunc = new SwitchStartBtnState(SetStartButtonState);
            instance = this;
            initDevFunc = new InitDevDelegate(InitDevFunction);
            stopThradFunc = new StopThradDelegate(Stop);
            LogHelper.Info("RedisLog", "MainResultLineInfoPage Begin");
            string strInfo = "系统正在处理中，请稍候...";
            _statusForm = new UIWaitForm(strInfo);
            _statusForm.TopMost = true;
            _statusForm.ShowInTaskbar = false;
            _statusForm.Render();

            itemFunc = new SwidthItem(SwitchItemIndex);

            hv_BVDictHandle = new HTuple();
            hv_SSDictHandle = new HTuple();
            hv_ResultDictHandle = new HTuple();
            hv_LQueueHandle = new HTuple();
            hv_ReceiveQueueHandle = new HTuple();
            hv_BVQueueHandle = new HTuple();
            ShowRightBtnFunc = new ShowRightButtonDelegate(ShowRightButton);
            hv_OutResultListHandle = new HTuple();
            HOperatorSet.CreateDict(out hv_BVDictHandle);
            HOperatorSet.CreateDict(out hv_SSDictHandle);

            #region 直径初始化
            {
                RadiusChartValues = new ChartValues<ObservablePoint>{
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

                if (_cartesianRadiusChart.AxisY.Count > 0)
                {
                    _cartesianRadiusChart.AxisY.RemoveAt(0);
                }
                
                if (_cartesianRadiusChart.AxisX.Count > 0)
                {
                    _cartesianRadiusChart.AxisX.RemoveAt(0);
                }
                

                _cartesianRadiusChart.AxisY.Add(new Axis
                {
                    Title = "直径(mm)",
                    FontSize = 14,
                });
                _cartesianRadiusChart.AxisX.Add(new Axis
                {
                    Title = "长度(mm)",
                    FontSize = 14,
                }
                );
                _cartesianRadiusChart.DisableAnimations = true;
                _cartesianRadiusChart.DataTooltip = null;
                _cartesianRadiusChart.Hoverable = false;
                _cartesianRadiusChart.AxisX[0].MinValue = 0;
                _cartesianRadiusChart.AxisX[0].MaxValue = 100;
             
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

                if (_cartesianYingLiChart.AxisY.Count > 0)
                {
                    _cartesianYingLiChart.AxisY.RemoveAt(0);
                }

                if (_cartesianYingLiChart.AxisX.Count > 0)
                {
                    _cartesianYingLiChart.AxisX.RemoveAt(0);
                }

                _cartesianYingLiChart.AxisY.Add(new Axis
                {
                    Title = "灰度",
                    FontSize = 14,

                });
                _cartesianYingLiChart.AxisX.Add(new Axis
                {
                    Title = "长度(mm)",
                    FontSize = 14,

                }
                );
                _cartesianYingLiChart.DisableAnimations = true;
                _cartesianYingLiChart.DataTooltip = null;
                _cartesianYingLiChart.Hoverable = false;
                _cartesianYingLiChart.Zoom = ZoomingOptions.Y;

                
            }
            #endregion

            #region 电阻率初始化
            {
                ResivityChartValues = new ChartValues<ObservablePoint>{
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

                if (_cartesianResisvityChart.AxisY.Count > 0)
                {
                    _cartesianResisvityChart.AxisY.RemoveAt(0);
                }

                if (_cartesianResisvityChart.AxisX.Count > 0)
                {
                    _cartesianResisvityChart.AxisX.RemoveAt(0);
                }
                _cartesianResisvityChart.AxisY.Add(new Axis
                {
                    Title = "电阻率",
                    FontSize = 14,

                });
                _cartesianResisvityChart.AxisX.Add(new Axis
                {
                    Title = "长度(mm)",
                    FontSize = 14,
                });
                _cartesianResisvityChart.DisableAnimations = true;
                _cartesianResisvityChart.DataTooltip = null;
                _cartesianResisvityChart.Hoverable = false;
                _cartesianResisvityChart.Zoom = ZoomingOptions.Y;

                
            }
            #endregion

            #region 全图初始化
            {

                #region 直径初始化
                RadiusChartViewValues = new ChartValues<ObservablePoint>{
                    new ObservablePoint(0, 0)
                };


                _cartesianRadiusChartResultView.Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Values = RadiusChartViewValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                };

                if (_cartesianRadiusChartResultView.AxisY.Count > 0)
                {
                    _cartesianRadiusChartResultView.AxisY[0].Title = "直径";
                }
                else
                {
                    _cartesianRadiusChartResultView.AxisY.Add(new Axis()
                    {
                        Title = "直径"
                     });

                }
                
                if (_cartesianRadiusChartResultView.AxisX.Count > 0)
                {
                    _cartesianRadiusChartResultView.AxisX[0].Title = "长度(mm)";
                }
                else
                {
                    _cartesianRadiusChartResultView.AxisX.Add(new Axis()
                    {
                        Title = "长度(mm)"
                    });
                }
                

                _cartesianRadiusChartResultView.DisableAnimations = true;
                _cartesianRadiusChartResultView.DataTooltip = null;
                _cartesianRadiusChartResultView.Hoverable = false;
                _cartesianRadiusChartResultView.Zoom = ZoomingOptions.Y;
                _cartesianRadiusChartResultView.AxisY[0].MinValue = 0;
                _cartesianRadiusChartResultView.AxisY[0].MaxValue = 10;
                #endregion

                #region  电阻率初始化
                ResivityChartViewValues = new ChartValues<ObservablePoint>{
                    new ObservablePoint(0, 0)
                };


                _cartesianResisvityChartResultView.Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Values = ResivityChartViewValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                };

                if (_cartesianResisvityChartResultView.AxisY.Count > 0)
                {
                    _cartesianResisvityChartResultView.AxisY[0].Title = "电阻率";
                }
                else
                {
                    _cartesianResisvityChartResultView.AxisY.Add(new Axis()
                    {
                        Title = "电阻率"
                    });

                }


                if (_cartesianResisvityChartResultView.AxisX.Count > 0)
                {
                    _cartesianResisvityChartResultView.AxisX[0].Title = "长度(mm)";
                }
                else
                {
                    _cartesianResisvityChartResultView.AxisX.Add(new Axis()
                    {
                        Title = "长度(mm)"
                    });
                }


                _cartesianResisvityChartResultView.DisableAnimations = true;
                _cartesianResisvityChartResultView.DataTooltip = null;
                _cartesianResisvityChartResultView.Hoverable = false;
                _cartesianResisvityChartResultView.Zoom = ZoomingOptions.Y;
                _cartesianResisvityChartResultView.AxisY[0].MinValue = 0;
                _cartesianResisvityChartResultView.AxisY[0].MaxValue = 10;
                #endregion

                #region  应力初始化
                YingLiChartViewValues = new ChartValues<ObservablePoint>{
                    new ObservablePoint(0, 0)
                };


                _cartesianYingLiChartResultView.Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Values = YingLiChartViewValues,
                        PointGeometry = null,
                        DataLabels = false
                    },
                };
                if (_cartesianYingLiChartResultView.AxisY.Count > 0)
                {
                    _cartesianYingLiChartResultView.AxisY[0].Title = "灰度";
                }
                else
                {
                    _cartesianYingLiChartResultView.AxisY.Add(new Axis()
                    {
                        Title = "灰度"
                    });

                }


                if (_cartesianYingLiChartResultView.AxisX.Count > 0)
                {
                    _cartesianYingLiChartResultView.AxisX[0].Title = "长度(mm)";
                }
                else
                {
                    _cartesianYingLiChartResultView.AxisX.Add(new Axis()
                    {
                        Title = "长度(mm)"
                    });
                }


                _cartesianYingLiChartResultView.DisableAnimations = true;
                _cartesianYingLiChartResultView.DataTooltip = null;
                _cartesianYingLiChartResultView.Hoverable = false;
                _cartesianYingLiChartResultView.Zoom = ZoomingOptions.Y;
                _cartesianYingLiChartResultView.AxisY[0].MinValue = 0;
                _cartesianYingLiChartResultView.AxisY[0].MaxValue = 10;

                #endregion

            }
            #endregion


            _curStickData = new StickData();
          

            this._tabMainLineControl.SelectedIndex = 0;

            if (SettingParameter.Instance().NMoveType == (int)emMoveType.EM_MOTIONCARD)
            {
                _timeInitMoveController = new System.Windows.Forms.Timer()
                {
                    Interval = 500,
                };
                _timeInitMoveController.Tick += _timeInitMoveController_Tick;
                _timeInitMoveController.Start();

            }
            _timeRefresh = new System.Windows.Forms.Timer
            {
                Interval = 1000,
            };
            _timeRefresh.Tick += _timeRefresh_Tick;
            //_timeRefresh.Start();


            _timeRefreshFull = new System.Windows.Forms.Timer
            {
                Interval = 1000,
            };
            _timeRefreshFull.Tick += _timeRefreshFullView_Tick;
            _timeRefreshFull.Start();


            hv_ResultDictHandle.Dispose();
            HOperatorSet.ReadDict("./Result", new HTuple(), new HTuple(), out hv_ResultDictHandle);

           
            hv_ReceiveQueueHandle.Dispose();
            HOperatorSet.CreateMessageQueue(out hv_ReceiveQueueHandle);

            hv_BVQueueHandle.Dispose();
            HOperatorSet.CreateMessageQueue(out hv_BVQueueHandle);

            hv_LQueueHandle.Dispose();
            HOperatorSet.CreateMessageQueue(out hv_LQueueHandle);

            _tim = new System.Windows.Forms.Timer
            {
                Interval = 200,
            };

            _tim.Tick += timeRefreshTabPage;
            _tim.Start();

            _timeRadiusRefresh = new System.Windows.Forms.Timer
            {
                Interval = 100,
            };
            _timeRadiusRefresh.Tick += timeRadiusRefresh_Tick;

            _timeYingLiRefresh = new System.Windows.Forms.Timer
            {
                Interval = 100,
            };
            _timeYingLiRefresh.Tick += timeYingLiRefresh_Tick;

            _timeResivityRefresh = new System.Windows.Forms.Timer
            {
                Interval = 100,
            };
            _timeResivityRefresh.Tick += timeResisvityRefresh_Tick;

            _timeRadiusViewRefresh = new System.Windows.Forms.Timer
            {
                Interval = 100,
            };
            _timeRadiusViewRefresh.Tick += _timeRadiusViewRefresh_Tick; 

            _timeYingLiViewRefresh = new System.Windows.Forms.Timer
            {
                Interval = 100,
            };
            _timeYingLiViewRefresh.Tick += _timeYingLiViewRefresh_Tick; 

            _timeResivityViewRefresh = new System.Windows.Forms.Timer
            {
                Interval = 100,
            };
            _timeResivityViewRefresh.Tick += _timeResivityViewRefresh_Tick;

            _timeStatusFormMonitorRefresh = new System.Windows.Forms.Timer
            {
                Interval = 5000,
            };
            _timeStatusFormMonitorRefresh.Tick += timeStatusFormMonitorRefresh_Tick;
            //_timeStatusFormMonitorRefresh.Start();


            _timeRefreshResultFull = new System.Windows.Forms.Timer { Interval = 1000 };
            _timeRefreshResultFull.Tick += _timeRefreshResultFull_Tick;

            _timeStatusFormMonitorViewRefresh = new System.Windows.Forms.Timer
            {
                Interval = 5000,
            };


            _timeStatusFormMonitorViewRefresh.Tick += timeStatusFormMonitorViewRefresh_Tick;
            _timeStatusFormMonitorViewRefresh.Start();
        }

        private void _timeRefreshResultFull_Tick(object sender, EventArgs e)
        {
            this._panelRadiusResultFull.Refresh();
            this._panelResivityResutlFull.Refresh();
            this._panelYingliResultFull.Refresh();

            this._cartesianRadiusChartResultView.Refresh();
            this._cartesianResisvityChartResultView.Refresh();
            this._cartesianYingLiChartResultView.Refresh();

            _timeRefreshResultFull.Stop();
        }

     

        private void timeStatusFormMonitorViewRefresh_Tick(object sender, EventArgs e)
        {
            if (_statusForm.Visible == true)
            {
                try
                {
                    if ((_curStickData.RadiusNumInfo.Count > 0) && ((_preIndexOfRadiusView == 0 || _preIndexOfRadiusView >= _curStickData.RadiusNumInfo.Count) && (_preIndexOfYingliView == 0 || _preIndexOfYingliView >= _curStickData.YingliNumInfo.Count) && (_preIndexOfResisvityView == 0 || _preIndexOfResisvityView >= _curStickData.ResisvityNumInfo.Count)))
                    {
                        _statusForm.Visible = false;
                        _btnSet.Enabled = true;
                       

                        if (_curStickData.RadiusNumInfo.Count > 0)
                        {
                            InitRadiusResultViewSeries();
                         
                            _cartesianRadiusChartResultView.Update();
                            _cartesianResisvityChartResultView.Update();
                            _cartesianYingLiChartResultView.Update();

                        }


                        //if (_curStickData.StrResultYinliePicPath.Length > 0)
                        //{
                        //    Bitmap bmpYL_1 = new Bitmap(_curStickData.StrResultYinliePicPath);
                        //    pictureBoxFullYinLieResultView.Image = bmpYL_1;
                        //    //pictureBoxFullYinLieResultView.PreImage = bmpYL_1;
                        //}

                        if (_curStickData.StrResultPicPath.Length > 0)
                        {
                            Bitmap bmpLJ_1 = new Bitmap(_curStickData.StrResultPicPath);
                            pictureBoxResult.Image = bmpLJ_1;
                            pictureBoxFullYinLieResultView.Image = bmpLJ_1;
                            //pictureBoxResult.PreImage = bmpLJ_1;
                        }
                    }
                }
                catch(Exception ex)
                {

                }
                
            }
        }

        private void _timeResivityViewRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_preIndexOfResisvityView > 0)
                {
                    float fMinValue = _preMinValueOfResisvityView;
                    float fMaxValue = _preMaxValueOfResisvityView;
                    int nShowIndex = 0;
                    ChartValues<ObservablePoint> ptValues = new ChartValues<ObservablePoint>();

                    for (; _preIndexOfResisvityView < _curStickData.ResisvityNumInfo.Count; _preIndexOfResisvityView++)
                    {
                        ptValues.Add(new ObservablePoint((int)(_preIndexOfResisvityView * 0.23), (float)_curStickData.ResisvityNumInfo[_preIndexOfResisvityView]));
                        nShowIndex++;
                        if ((float)_curStickData.ResisvityNumInfo[_preIndexOfResisvityView] < fMinValue)
                        {
                            fMinValue = (float)_curStickData.ResisvityNumInfo[_preIndexOfResisvityView];
                        }

                        if ((float)_curStickData.ResisvityNumInfo[_preIndexOfResisvityView] > fMaxValue)
                        {
                            fMaxValue = (float)_curStickData.ResisvityNumInfo[_preIndexOfResisvityView];
                        }
                        if (nShowIndex >= _showCountOfPoints)
                        {
                            break;
                        }
                    }

                    ResivityChartViewValues.AddRange(ptValues);
                    _preMinValueOfResisvityView = fMinValue;
                    _preMaxValueOfResisvityView = fMaxValue;

                    if (_preIndexOfResisvityView >= _curStickData.ResisvityNumInfo.Count)
                    {
                        _preIndexOfResisvityView = 0;
                        _timeResivityViewRefresh.Stop();
                    }

                    _cartesianResisvityChartResultView.AxisY[0].MinValue = fMinValue;
                    _cartesianResisvityChartResultView.AxisY[0].MaxValue = fMaxValue;
                    _cartesianResisvityChartResultView.Update();
                }
            }
            catch(Exception ex)
            {

            }
            
        }

        private void _timeYingLiViewRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_preIndexOfYingliView > 0)
                {
                    float fMinValue = _preMinValueOfYingLiView;
                    float fMaxValue = _preMaxValueOfYingLiView;
                    int nShowIndex = 0;
                    ChartValues<ObservablePoint> ptValues = new ChartValues<ObservablePoint>();
                    int _preIndexX = -1;
                    for (; _preIndexOfYingliView < _curStickData.YingliNumInfo.Count; _preIndexOfYingliView++)
                    {
                        if (_preIndexX != -1)
                        {
                            _preIndexX = (int)(_preIndexOfYingliView * 0.46);
                        }
                        else
                        {
                            if (_preIndexX == (int)(_preIndexOfYingliView * 0.46))
                            {
                                continue;
                            }
                            else
                            {
                                _preIndexX = (int)(_preIndexOfYingliView * 0.46);
                            }    
                        }
                        ptValues.Add(new ObservablePoint(_preIndexX, (float)_curStickData.YingliNumInfo[_preIndexOfYingliView]));
                        nShowIndex++;
                        if ((float)_curStickData.YingliNumInfo[_preIndexOfYingliView] < fMinValue)
                        {
                            fMinValue = (float)_curStickData.YingliNumInfo[_preIndexOfYingliView];
                        }

                        if ((float)_curStickData.YingliNumInfo[_preIndexOfYingliView] > fMaxValue)
                        {
                            fMaxValue = (float)_curStickData.YingliNumInfo[_preIndexOfYingliView];
                        }
                        if (nShowIndex >= _showCountOfPoints)
                        {
                            break;
                        }
                    }

                    YingLiChartViewValues.AddRange(ptValues);
                    _preMinValueOfYingLiView = fMinValue;
                    _preMaxValueOfYingLiView = fMaxValue;

                    if (_preIndexOfYingliView >= _curStickData.YingliNumInfo.Count)
                    {
                        _preIndexOfYingliView = 0;
                        _timeYingLiViewRefresh.Stop();
                    }
                    _cartesianYingLiChartResultView.AxisY[0].MinValue = fMinValue;
                    _cartesianYingLiChartResultView.AxisY[0].MaxValue = fMaxValue;
                    _cartesianYingLiChartResultView.Update();
                }
            }
            catch(Exception ex)
            {

            }
            
        }

        private void _timeRadiusViewRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_preIndexOfRadiusView > 0)
                {
                    float fMinValue = _preMinValueOfRadiusView;
                    float fMaxValue = _preMaxValueOfRadiusView;
                    int nShowIndex = 0;
                    ChartValues<ObservablePoint> ptValues = new ChartValues<ObservablePoint>();

                    for (; _preIndexOfRadiusView < _curStickData.RadiusNumInfo.Count; _preIndexOfRadiusView++)
                    {
                        ptValues.Add(new ObservablePoint((int)(_preIndexOfRadiusView * 0.23), (float)_curStickData.RadiusNumInfo[_preIndexOfRadiusView]));
                        nShowIndex++;
                        if ((float)_curStickData.RadiusNumInfo[_preIndexOfRadiusView] < fMinValue)
                        {
                            fMinValue = (float)_curStickData.RadiusNumInfo[_preIndexOfRadiusView];
                        }

                        if ((float)_curStickData.RadiusNumInfo[_preIndexOfRadiusView] > fMaxValue)
                        {
                            fMaxValue = (float)_curStickData.RadiusNumInfo[_preIndexOfRadiusView];
                        }
                        if (nShowIndex >= _showCountOfPoints)
                        {
                            break;
                        }
                    }

                    RadiusChartViewValues.AddRange(ptValues);
                    _preMinValueOfRadiusView = fMinValue;
                    _preMaxValueOfRadiusView = fMaxValue;

                    if (_preIndexOfRadiusView >= _curStickData.RadiusNumInfo.Count)
                    {
                        _preIndexOfRadiusView = 0;
                        _timeRadiusViewRefresh.Stop();
                    }

                    _cartesianRadiusChartResultView.AxisY[0].MinValue = Math.Max(0, fMinValue - 1);
                    _cartesianRadiusChartResultView.AxisY[0].MaxValue = fMaxValue + 30;
                    _cartesianRadiusChartResultView.Update();
                }
            }
            catch(Exception ex)
            {

            }
            
        }

        private void _timeInitMoveController_Tick(object sender, EventArgs e)
        {
            CMoveController.Instance().CurPage = this;

            Task.Run(() =>
            {
                if (SettingParameter.Instance().NDaemon > 0)
                {
                    if (SettingParameter.Instance().NMoveType == (int)emMoveType.EM_MOTIONCARD)
                    {

                        CMotionCardController.Instance().Init();

                        //归零
                        if (SettingParameter.Instance().NSetZeroType == 1)
                        {

                            if (SettingParameter.Instance().FSubPosition != 0)
                            {
                                double dbValue = 0;
                                CMotionCardController.Instance().GetCurAxisPostion(CMotionCardController.emAxisName.emThreeD, out dbValue);
                                double dbDestPosition = dbValue - SettingParameter.Instance().FSubPosition;
                                CMotionCardController.Instance().BIsSetZeror = true;
                                CMotionCardController.Instance().ThreeD_ScanZero((short)CMotionCardController.emAxisName.emThreeD, (int)SettingParameter.Instance().FMoveSpeed, dbDestPosition, dbValue, SettingParameter.Instance().FMoveIncreaseSpeed);
                                CMotionCardController.Instance().SetZeroPosNoLimit((int)CMotionCardController.emAxisName.emThreeD);

                            }
                            else
                            {
                                CMotionCardController.Instance().BIsSetZeror = true;
                            }


                        }
                    }

                }
            }); 
           
            _timeInitMoveController.Stop();
        }
        private void ReleaseCameras()
        {
            LogHelper.Info("Silicon", "ReleaseCameras begin");
            try
            {
                CHikCameraTools.Instance().StopPlay((int)CHikCameraTools.emHikCameraType.emVideo);
                CHikCameraTools.Instance().StopPlay((int)CHikCameraTools.emHikCameraType.emLJ);

                CHikCameraTools.Instance().Logout((int)CHikCameraTools.emHikCameraType.emVideo);
                CHikCameraTools.Instance().Logout((int)CHikCameraTools.emHikCameraType.emLJ);


                CBVCameraTools.Instance().StopPlay((int)CBVCameraTools.emBVCameraType.emBV_YL);
                CBVCameraTools.Instance().StopPlay((int)CBVCameraTools.emBVCameraType.emBV_YingLi);

                CBVCameraTools.Instance().Logout((int)CBVCameraTools.emBVCameraType.emBV_YL);
                CBVCameraTools.Instance().Logout((int)CBVCameraTools.emBVCameraType.emBV_YingLi);


                //SSZNCamera.Instance().DisConnect();
            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon", "ReleaseCameras Exception " + ex.Message);
            }

            LogHelper.Info("Silicon", "ReleaseCameras End");
        }

        private void MainResultLineInfoPage_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            try
            {
                if (m_threadScanStick != null && m_threadScanStick.ThreadState != System.Threading.ThreadState.Stopped)
                {
                    m_threadScanStick.Abort();
                    m_threadScanStick = null;
                }

                if (SettingParameter.Instance().NMoveType == (int)emMoveType.EM_MOTIONCARD)
                {
                    CMotionCardController.Instance().Close();
                }

                ReleaseCameras();


            }
            catch (Exception ex)
            {

            }
        }

        private void SwitchItemIndex()
        {
            _tabMainLineControl.SelectedIndex = _nSwidthIndex;
            //_tabMainLineControl.Update();
            //if (_nSwidthIndex == 5)
            //{
            //    this._cartesianRadiusChartResultView.Refresh();
            //    this._cartesianResisvityChartResultView.Refresh();
            //    this._cartesianYingLiChartResultView.Refresh();
            //}
           
        }

        private void timeStatusFormMonitorRefresh_Tick(object sender, EventArgs e)
        {
            if (_statusForm.Visible == true)
            {
                if ((_curStickData.RadiusNumInfo.Count > 0 ) &&  ((_preIndexOfRadius == 0 || _preIndexOfRadius >= _curStickData.RadiusNumInfo.Count) && (_preIndexOfYingLi == 0 || _preIndexOfYingLi >= _curStickData.YingliNumInfo.Count) && (_preIndexOfResisvity == 0 || _preIndexOfResisvity >= _curStickData.ResisvityNumInfo.Count) ))
                {
                    _statusForm.Visible = false;
                    _btnSet.Enabled = true;

                    if ( _curStickData.RadiusNumInfo.Count > 0)
                    {
                        InitRadiusResultSeries();
                       _cartesianRadiusChart.AxisX[0].MinValue = 0;
                       _cartesianRadiusChart.AxisX[0].MaxValue = RadiusChartValues.Count * 0.23 + 10;
                    }
                    
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


        private void timeRefreshTabPage(object sender, EventArgs e )
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


        private void timeRadiusRefresh_Tick(object sender, System.EventArgs e)
        {
            if (_preIndexOfRadius > 0)
            {
                float fMinValue = _preMinValueOfRadius;
                float fMaxValue = _preMaxValueOfRadius;
                int nShowIndex = 0;
                ChartValues<ObservablePoint> ptValues = new ChartValues<ObservablePoint>();

                for (; _preIndexOfRadius < _curStickData.RadiusNumInfo.Count; _preIndexOfRadius++)
                {
                    ptValues.Add(new ObservablePoint((int)(_preIndexOfRadius * 0.23), (float)_curStickData.RadiusNumInfo[_preIndexOfRadius]));
                    nShowIndex++;
                    if ((float)_curStickData.RadiusNumInfo[_preIndexOfRadius] < fMinValue)
                    {
                        fMinValue = (float)_curStickData.RadiusNumInfo[_preIndexOfRadius];
                    }

                    if ((float)_curStickData.RadiusNumInfo[_preIndexOfRadius] > fMaxValue)
                    {
                        fMaxValue = (float)_curStickData.RadiusNumInfo[_preIndexOfRadius];
                    }
                    if (nShowIndex >= _showCountOfPoints)
                    {
                        break;
                    }
                }

                RadiusChartValues.AddRange(ptValues);
                _preMinValueOfRadius = fMinValue;
                _preMaxValueOfRadius = fMaxValue;

                if (_preIndexOfRadius >= _curStickData.RadiusNumInfo.Count)
                {
                    _preIndexOfRadius = 0;
                    _timeRadiusRefresh.Stop();
                }

                _cartesianRadiusChart.AxisY[0].MinValue = Math.Max(0, fMinValue - 1);
                _cartesianRadiusChart.AxisY[0].MaxValue = fMaxValue + 30;

                _cartesianRadiusChart.Update();
            }
        }

        private void timeYingLiRefresh_Tick(object sender, System.EventArgs e)
        {

            if (_preIndexOfYingLi > 0)
            {
                float fMinValue = _preMinValueOfYingLi;
                float fMaxValue = _preMaxValueOfYingLi;
                int nShowIndex = 0;
                ChartValues<ObservablePoint> ptValues = new ChartValues<ObservablePoint>();

                for (; _preIndexOfYingLi < _curStickData.YingliNumInfo.Count; _preIndexOfYingLi++)
                {
                    ptValues.Add(new ObservablePoint((int)(_preIndexOfYingLi * 0.46), (float)_curStickData.YingliNumInfo[_preIndexOfYingLi]));
                    nShowIndex++;
                    if ((float)_curStickData.YingliNumInfo[_preIndexOfYingLi] < fMinValue)
                    {
                        fMinValue = (float)_curStickData.YingliNumInfo[_preIndexOfYingLi];
                    }

                    if ((float)_curStickData.YingliNumInfo[_preIndexOfYingLi] > fMaxValue)
                    {
                        fMaxValue = (float)_curStickData.YingliNumInfo[_preIndexOfYingLi];
                    }
                    if (nShowIndex >= _showCountOfPoints)
                    {
                        break;
                    }
                }

                YingLiChartValues.AddRange(ptValues);
                _preMinValueOfYingLi = fMinValue;
                _preMaxValueOfYingLi = fMaxValue;
                
                if (_preIndexOfYingLi >= _curStickData.YingliNumInfo.Count)
                {
                    _preIndexOfYingLi = 0;
                    _timeYingLiRefresh.Stop();
                }
                _cartesianYingLiChart.AxisY[0].MinValue = fMinValue;
                _cartesianYingLiChart.AxisY[0].MaxValue = fMaxValue;
                _cartesianYingLiChart.Update();
            }

        }

        private void timeResisvityRefresh_Tick(object sender, System.EventArgs e)
        {
            if (_preIndexOfResisvity > 0)
            {
                float fMinValue = _preMinValueOfResisvity;
                float fMaxValue = _preMaxValueOfResisvity;
                int nShowIndex = 0;
                ChartValues<ObservablePoint> ptValues = new ChartValues<ObservablePoint>();

                for (; _preIndexOfResisvity < _curStickData.ResisvityNumInfo.Count; _preIndexOfResisvity++)
                {
                    ptValues.Add(new ObservablePoint((int)(_preIndexOfResisvity /** 0.23*/), (float)_curStickData.ResisvityNumInfo[_preIndexOfResisvity]));
                    nShowIndex++;
                    if ((float)_curStickData.ResisvityNumInfo[_preIndexOfResisvity] < fMinValue)
                    {
                        fMinValue = (float)_curStickData.ResisvityNumInfo[_preIndexOfResisvity];
                    }

                    if ((float)_curStickData.ResisvityNumInfo[_preIndexOfResisvity] > fMaxValue)
                    {
                        fMaxValue = (float)_curStickData.ResisvityNumInfo[_preIndexOfResisvity];
                    }
                    if (nShowIndex >= _showCountOfPoints)
                    {
                        break;
                    }
                }

                ResivityChartValues.AddRange(ptValues);
                _preMinValueOfResisvity = fMinValue;
                _preMaxValueOfResisvity = fMaxValue;

                if (_preIndexOfResisvity >= _curStickData.ResisvityNumInfo.Count)
                {
                    _preIndexOfResisvity = 0;
                    _timeResivityRefresh.Stop();
                }

                _cartesianResisvityChart.AxisY[0].MinValue = fMinValue;
                _cartesianResisvityChart.AxisY[0].MaxValue = fMaxValue;
                _cartesianResisvityChart.Update();
            }
        }

        private void InitRadiusResultViewSeries()
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

                }


                //for (int i = 0; i < subNormalRaidus.Count; i++)
                //{
                //    _cartesianRadiusChart.AxisX[0].Sections.Add(_axisRadiusAreassection[i]);
                //    _cartesianRadiusChart.AxisX[0].Sections.Add(_axisRadiusBeginsection[i]);
                //    _cartesianRadiusChart.AxisX[0].Sections.Add(_axisRadiusEndssection[i]);
                //}

                subNormalRaidus.Clear();

            }
            catch (Exception ex)
            {

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

                }

               
                //for (int i = 0; i < subNormalRaidus.Count; i++)
                //{
                //    _cartesianRadiusChart.AxisX[0].Sections.Add(_axisRadiusAreassection[i]);
                //    _cartesianRadiusChart.AxisX[0].Sections.Add(_axisRadiusBeginsection[i]);
                //    _cartesianRadiusChart.AxisX[0].Sections.Add(_axisRadiusEndssection[i]);
                //}

                subNormalRaidus.Clear();
                
                _tabPageRadius.Refresh();
                _labelMinRadius.Text = "最小直径：" + _curStickData.FMinRadius.ToString("0.00");
                _labelMaxRadius.Text = "最大直径：" + _curStickData.FMaxRadius.ToString("0.00");
                _labelLength.Text = "长度: " + _curStickData.FLength.ToString("0.00");
                _labelValidLength.Text = "有效长度: " + _curStickData.FValidLength.ToString("0.00");
                _labelGLLength.Text = "鼓棱长度:" + _curStickData.FGLLength.ToString("0.00");
                _labelGLLength.Text = "等径长度:" + _curStickData.FDJLength.ToString("0.00");
                _labelWCLength.Text = "无位错长度:" + _curStickData.FWCLength.ToString("0.00");
                _labelTotalLength.Text = "总长:" + _curStickData.FTotalLength.ToString("0.00");
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

        private void _timeRefreshFullView_Tick(object sender, System.EventArgs e)
        {
            if (_strGetStickLineNum != "")
            {
                _curStickData = new StickData();

                if (true == GlobalDataCache.Instance().GetData(_strGetStickLineNum, ref _curStickData))
                {
                    try
                    {

                       LogHelper.Info("Silicon", "MainResultLineInfoPage GetInfo _strGetStickLineNum " + _strGetStickLineNum);
                        _strGetStickLineNum = "";

                        float fMinValue = 65535;
                        float fMaxValue = -1;
                        #region 直径数据结果更新
                        _preIndexOfRadiusView = 0;
                        ChartValues<ObservablePoint> ptChartValues = new ChartValues<ObservablePoint>();

                        foreach (var item in _curStickData.RadiusNumInfo)
                        {
                            ptChartValues.Add(new ObservablePoint((int)(_preIndexOfRadiusView * 0.23), (float)item));

                            _preIndexOfRadiusView++;
                            if ((float)item < fMinValue)
                            {
                                fMinValue = (float)item;
                            }

                            if ((float)item > fMaxValue)
                            {
                                fMaxValue = (float)item;
                            }
                            if (_preIndexOfRadiusView >= _showCountOfPoints)
                            {
                                break;
                            }
                        }

                        RadiusChartViewValues.AddRange(ptChartValues);

                        _preMinValueOfRadiusView = fMinValue;
                        _preMaxValueOfRadiusView = fMaxValue;
                        _cartesianRadiusChartResultView.AxisY[0].MinValue = Math.Max(0, fMinValue - 1);
                        _cartesianRadiusChartResultView.AxisY[0].MaxValue = fMaxValue + 30;
                        _cartesianRadiusChartResultView.Update();
                        //_axisYingLiBeginsection = new AxisSection[_curStickData.ArrAbnormalArea.Count];
                        //_axisYingLiAreassection = new AxisSection[_curStickData.ArrAbnormalArea.Count];
                        //_axisYingLiEndssection = new AxisSection[_curStickData.ArrAbnormalArea.Count];
                        //for (int i = 0; i < _curStickData.ArrAbnormalArea.Count; i++)
                        //{
                        //    List<float> fngLengths = (List<float>)_curStickData.ArrAbnormalArea[i];
                        //    _axisYingLiBeginsection[i] = new AxisSection
                        //    {
                        //        Stroke = System.Windows.Media.Brushes.Black,//colocr
                        //        StrokeThickness = 5,
                        //        Value = fngLengths[0],//Modify this.
                        //        Draggable = true,
                        //        AllowDrop = true,
                        //        DisableAnimations = true,
                        //        Name = "Begin_" + i.ToString(),
                        //    };
                        //    _axisYingLiAreassection[i] = new AxisSection
                        //    {
                        //        Name = "Area_" + i.ToString(),
                        //        Value = fngLengths[0],
                        //        SectionWidth = (fngLengths[1] - fngLengths[0]),
                        //        Fill = new SolidColorBrush
                        //        {
                        //            Color = System.Windows.Media.Color.FromRgb(204, 204, 204),
                        //            Opacity = .4
                        //        }
                        //    };
                        //    _axisYingLiEndssection[i] = new AxisSection
                        //    {
                        //        Stroke = System.Windows.Media.Brushes.Black,//colocr
                        //        StrokeThickness = 5,
                        //        Value = fngLengths[1],//Modify this.
                        //        Draggable = true,
                        //        AllowDrop = true,
                        //        DisableAnimations = true,
                        //        Name = "End_" + i.ToString(),
                        //    };
                        //    _axisYingLiBeginsection[i].MouseUpFunc += YingLiAxissection_MouseUp;
                        //    _axisYingLiEndssection[i].MouseUpFunc += YingLiAxissection_MouseUp;
                        //}
                        //for (int i = 0; i < _curStickData.ArrAbnormalArea.Count; i++)
                        //{
                        //    _cartesianYingLiChart.AxisX[0].Sections.Add(_axisYingLiAreassection[i]);
                        //    _cartesianYingLiChart.AxisX[0].Sections.Add(_axisYingLiBeginsection[i]);
                        //    _cartesianYingLiChart.AxisX[0].Sections.Add(_axisYingLiEndssection[i]);
                        //}

                        #endregion

                        fMinValue = 65535;
                        fMaxValue = -1;
                        #region 应力数据结果更新
                        _preIndexOfYingliView = 0;

                        ptChartValues.Clear();

                        foreach (var item in _curStickData.YingliNumInfo)
                        {
                            ptChartValues.Add(new ObservablePoint((int)(_preIndexOfYingliView * 0.46), (float)item));
                            _preIndexOfYingliView++;
                            if ((float)item < fMinValue)
                            {
                                fMinValue = (float)item;
                            }

                            if ((float)item > fMaxValue)
                            {
                                fMaxValue = (float)item;
                            }
                            if (_preIndexOfYingliView >= _showCountOfPoints)
                            {
                                break;
                            }
                        }
                        YingLiChartViewValues.AddRange(ptChartValues);
                        _preMinValueOfYingLiView = fMinValue;
                        _preMaxValueOfYingLiView = fMaxValue;
                        _cartesianYingLiChartResultView.AxisY[0].MinValue = fMinValue;
                        _cartesianYingLiChartResultView.AxisY[0].MaxValue = fMaxValue;
                        _cartesianYingLiChartResultView.Update();
                        #endregion

                        fMinValue = 65535;
                        fMaxValue = -1;
                        #region 电阻率数据结果更新
                        _preIndexOfResisvityView = 0;
                        ptChartValues.Clear();
                        foreach (var item in _curStickData.ResisvityNumInfo)
                        {
                            ptChartValues.Add(new ObservablePoint((int)(_preIndexOfResisvityView * 0.23), (float)item));
                            _preIndexOfResisvityView++;
                            if ((float)item < fMinValue)
                            {
                                fMinValue = (float)item;
                            }

                            if ((float)item > fMaxValue)
                            {
                                fMaxValue = (float)item;
                            }
                            if (_preIndexOfResisvityView >= _showCountOfPoints)
                            {
                                break;
                            }
                        }
                        ResivityChartViewValues.AddRange(ptChartValues);


                        _preMinValueOfResisvityView = fMinValue;
                        _preMaxValueOfYingLiView = fMaxValue;
                        _cartesianResisvityChartResultView.AxisY[0].MinValue = fMinValue;
                        _cartesianResisvityChartResultView.AxisY[0].MaxValue = fMaxValue;
                        _cartesianResisvityChartResultView.Update();
                        




                        #endregion



                        _labelMinRadiusFullView.Text = "最小直径：" + _curStickData.FMinRadius.ToString("0.00");
                        _labelMaxRadiusFullView.Text = "最大直径：" + _curStickData.FMaxRadius.ToString("0.00");
                        _labelLengthFullView.Text = "长度: " + _curStickData.FLength.ToString("0.00");
                        _labelValidLengthFullView.Text = "有效长度: " + _curStickData.FValidLength.ToString("0.00");
                        _labelGLLength.Text = "鼓棱长度:" + _curStickData.FGLLength.ToString("0.00");
                        _labelDJLength.Text = "等径长度:" + _curStickData.FDJLength.ToString("0.00");
                        _labelWCLength.Text = "无位错长度:" + _curStickData.FWCLength.ToString("0.00");
                        _labelTotalLength.Text = "总长:" + _curStickData.FTotalLength.ToString("0.00");
                        _timeRadiusViewRefresh.Start();
                        _timeYingLiViewRefresh.Start();
                        _timeResivityViewRefresh.Start();

                        _statusForm.Show();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }



        }
        private void _timeRefresh_Tick(object sender, System.EventArgs e)
        {
            if (_strGetStickLineNum != "")
            {
                _curStickData = new StickData();

                if (true == GlobalDataCache.Instance().GetData(_strGetStickLineNum, ref _curStickData))
                {
                    try
                    {
                       

                        LogHelper.Info("Silicon", "MainResultLineInfoPage GetInfo _strGetStickLineNum " + _strGetStickLineNum);
                        _strGetStickLineNum = "";

                        float fMinValue = 65535;
                        float fMaxValue = -1;
                        #region 直径数据结果更新
                        _preIndexOfRadius = 0;

                        ChartValues<ObservablePoint> ptChartValues = new ChartValues<ObservablePoint>();

                        foreach (var item in _curStickData.RadiusNumInfo)
                        {
                            ptChartValues.Add(new ObservablePoint((int)(_preIndexOfRadius /** 0.23*/), (float)item));
                            //RadiusChartValues.Add(new ObservablePoint(_preIndexOfRadius, (float)item));
                            _preIndexOfRadius++;
                            if ((float)item < fMinValue)
                            {
                                fMinValue = (float)item;
                            }

                            if ((float)item > fMaxValue)
                            {
                                fMaxValue = (float)item;
                            }
                            if (_preIndexOfRadius >= _showCountOfPoints)
                            {
                                break;
                            }
                        }

                        RadiusChartValues.AddRange(ptChartValues);

                        _preMinValueOfRadius = fMinValue;
                        _preMaxValueOfRadius = fMaxValue;

                        _cartesianRadiusChart.AxisY[0].MinValue = Math.Max(0, fMinValue - 1);
                        _cartesianRadiusChart.AxisY[0].MaxValue = fMaxValue + 30;

                        _axisYingLiBeginsection = new AxisSection[_curStickData.ArrAbnormalArea.Count];
                        _axisYingLiAreassection = new AxisSection[_curStickData.ArrAbnormalArea.Count];
                        _axisYingLiEndssection = new AxisSection[_curStickData.ArrAbnormalArea.Count];
                        for (int i = 0; i < _curStickData.ArrAbnormalArea.Count; i++)
                        {
                            List<float> fngLengths = (List<float>)_curStickData.ArrAbnormalArea[i];
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


                        }


                        //for (int i = 0; i < _curStickData.ArrAbnormalArea.Count; i++)
                        //{
                        //    _cartesianYingLiChart.AxisX[0].Sections.Add(_axisYingLiAreassection[i]);
                        //    _cartesianYingLiChart.AxisX[0].Sections.Add(_axisYingLiBeginsection[i]);
                        //    _cartesianYingLiChart.AxisX[0].Sections.Add(_axisYingLiEndssection[i]);
                        //}

                        #endregion

                        fMinValue = 65535;
                        fMaxValue = -1;
                        #region 应力数据结果更新
                        _preIndexOfYingLi = 0;

                        ptChartValues.Clear();

                        foreach (var item in _curStickData.YingliNumInfo)
                        {
                            ptChartValues.Add(new ObservablePoint((int)(_preIndexOfYingLi /** 0.46*/), (float)item));
                            //YingLiChartValues.Add(new ObservablePoint(_preIndexOfYingLi, (float)item));
                            _preIndexOfYingLi++;
                            if ((float)item < fMinValue)
                            {
                                fMinValue = (float)item;
                            }

                            if ((float)item > fMaxValue)
                            {
                                fMaxValue = (float)item;
                            }
                            if (_preIndexOfYingLi >= _showCountOfPoints)
                            {
                                break;
                            }
                        }
                        YingLiChartValues.AddRange(ptChartValues);
                        _preMinValueOfYingLi = fMinValue;
                        _preMaxValueOfYingLi = fMaxValue;
                        _cartesianYingLiChart.AxisY[0].MinValue = fMinValue;
                        _cartesianYingLiChart.AxisY[0].MaxValue = fMaxValue;
                        #endregion

                        fMinValue = 65535;
                        fMaxValue = -1;
                        #region 电阻率数据结果更新
                        _preIndexOfResisvity = 0;
                        ptChartValues.Clear();
                        foreach (var item in _curStickData.ResisvityNumInfo)
                        {
                            ptChartValues.Add(new ObservablePoint((int)(_preIndexOfResisvity/* * 0.23*/), (float)item));
                            //ResivityChartValues.Add(new ObservablePoint(_preIndexOfResisvity, (float)item));
                            _preIndexOfResisvity++;
                            if ((float)item < fMinValue)
                            {
                                fMinValue = (float)item;
                            }

                            if ((float)item > fMaxValue)
                            {
                                fMaxValue = (float)item;
                            }
                            if (_preIndexOfResisvity >= _showCountOfPoints)
                            {
                                break;
                            }
                        }
                        ResivityChartValues.AddRange(ptChartValues);
                        if (_curStickData.StrResultPicPath.Length > 0)
                        {
                            Bitmap bmpLJ_1 = new Bitmap(_curStickData.StrResultPicPath);
                            pictureBoxResult.Image = bmpLJ_1;
                            //pictureBoxResult.PreImage = bmpLJ_1;

                        }
                        if (_curStickData.StrResultYinliePicPath.Length > 0)
                        {
                            Bitmap bmpYL_1 = new Bitmap(_curStickData.StrResultYinliePicPath);
                            pictureBoxYinlie.Image = bmpYL_1;
                            //pictureBoxYinlie.PreImage = bmpYL_1;
                        }
                        


                        _preMinValueOfResisvity = fMinValue;
                        _preMaxValueOfResisvity = fMaxValue;
                        _cartesianResisvityChart.AxisY[0].MinValue = fMinValue;
                        _cartesianResisvityChart.AxisY[0].MaxValue = fMaxValue;

                        #endregion

                       

                        _timeRadiusRefresh.Start();
                        _timeYingLiRefresh.Start();
                        _timeResivityRefresh.Start();

                        _statusForm.Show();
                    }
                    catch(Exception ex)
                    {

                    }
                    
                }

            }
        }

        private void ScanRadiusFunc()
        {
            ProcessManager.Instance().ScanRadius();
        }

        private Thread m_threadScanStick;
        //nL 为开始位置
        //nLength 为棒子扫描长度
        private void DealSiliconStickPosition(int nL, int nLength)
        {
            int nTerminalLength = 0;
            int Length = nLength + 300;
            int nW = 0;
            try
            {
                int D = (nL / 1000);
                int B = ((nL - D * 1000) / 100);
                int S = ((nL - D * 1000 - B * 100) / 10);
                int Head = D * 1000 + B * 100 + S * 10;
                CMoveController.Instance().FPosition_Third = nL - 400;

                int F = (Length / 1000);
                switch (F)
                {
                    case 0:
                        {
                            nTerminalLength = nL + 1000 - 400;
                            nW = 1;
                            break;
                        }
                    case 1:
                        {
                            nTerminalLength = nL + 2000 - 400;
                            nW = 2;
                            break;
                        }
                    case 2:
                        {
                            nTerminalLength = nL + 3000 - 400;
                            nW = 3;
                            break;
                        }
                    case 3:
                        {
                            nTerminalLength = nL + 4000 - 400;
                            nW = 4;
                            break;
                        }
                    case 4:
                        {
                            nTerminalLength = nL + 5000 - 400;
                            nW = 5;
                            break;
                        }
                    case 5:
                        {
                            nTerminalLength = nL + 6000 - 400;
                            nW = 6;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                CMoveController.Instance().FPosition_Second = Math.Min(800, nTerminalLength);

            }
            catch (Exception ex)
            {

            }

        }
        private UIBreadcrumb uiBreadcrumb;
        public SwidthItem itemFunc;
        private HTuple hv_ResultDictHandle;
        private HTuple hv_SSDictHandle;
        private HTuple hv_BVDictHandle;
        private HTuple hv_LQueueHandle;
        private HTuple hv_BVQueueHandle;
        private HTuple hv_ReceiveQueueHandle;


        private void ThreadScanSiliconStickNPTypeVirtalFunc()
        {
            HTuple hv_MessageHandle = new HTuple();
            HTuple hv_Data = new HTuple(), hv_Start = new HTuple();
            HTuple hv_Sublevel = new HTuple(), hv_ZB = new HTuple();
            HObject cbho_Imageconst;
            LogHelper.Info("Silicon", "ThreadScanSiliconStickNPTypeFunc Begin");

            try
            {
                //CMoveController.Instance().InitPositionInfo();
                //CMoveController.Instance().TurnOnStick();


                #region Ready Start
                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Turn On Stick End");
                //CMoveController.Instance().GotoOrigin();  //回原点  离零点很近
                //Thread.Sleep(500);

                HOperatorSet.GenEmptyObj(out cbho_Imageconst);
                NSwidthIndex = 0;
                this.Invoke(itemFunc);

                //CMoveController.Instance().GoOn(true);
                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin to Find SiliconLine ");
                //ProcessManager.Instance().JBANG_Identification();
                //Thread.Sleep(1000);
                //LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Find SiliconLine End ");
                //CMoveController.Instance().GoOn(false);

                //ProcessManager.Instance().FindSiliconLineNew();
                #endregion
                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin to Scan Radius ");
                //CMoveController.Instance().GotoOrigin();

                Thread.Sleep(2000);



                //CMoveController.Instance().LightNew(true);
                HTuple hv_Coordinate = new HTuple();

                HObject ho_BV1TiledImage = new HObject();
                HObject ho_BV2TiledImage = new HObject();
                HTuple hv_Name = new HTuple();
                HTuple hv_HFQ = new HTuple();
                HTuple hv_selected = new HTuple();
                HTuple hv_IDTime = new HTuple();
                HTuple hv_WaferID = new HTuple();

                HTuple hv_BVRate = new HTuple();
                HTuple hv_SSRate = new HTuple();
                hv_SSRate = 0.234900925339;
                hv_BVRate = 0.2324571;
                CGlobalFuncTools.Instance().Name_and_other(hv_ResultDictHandle, out hv_selected, out hv_Name, out hv_HFQ, out hv_IDTime, out hv_WaferID);
                HOperatorSet.SetDictTuple(hv_BVDictHandle, "头部反切长度", hv_HFQ);
                HOperatorSet.SetDictTuple(hv_BVDictHandle, "IDTime", hv_IDTime);
                HOperatorSet.SetDictTuple(hv_SSDictHandle, "头部反切长度", hv_HFQ);
                HOperatorSet.SetDictTuple(hv_SSDictHandle, "IDTime", hv_IDTime);

                //CBVCameraTools.Instance().SetFeatrueValue((int)CBVCameraTools.emBVCameraType.emBV_YL, "Gain", (float)1.0);
                //CBVCameraTools.Instance().SetFeatrueValue((int)CBVCameraTools.emBVCameraType.emBV_YingLi, "Gain", (float)1.0);

                //到位置三， 暂定为起点
                //Thread scanYinLieWC = new Thread(() =>
                //{
                //    CBVCameraTools.Instance().ScanYinLieAndWCNewNP(hv_BVDictHandle, hv_ResultDictHandle, hv_ReceiveQueueHandle, hv_LQueueHandle, hv_Data, out ho_BV1TiledImage, out ho_BV2TiledImage);
                //});
                //scanYinLieWC.Start();
                HTuple hv_SS = new HTuple(0);
                Thread measurethread = new Thread(() =>
                {
                    SSZNCameraTools.Instance().Measure_3DNPType(out cbho_Imageconst, ref hv_ResultDictHandle, hv_SSDictHandle, out hv_OutResultListHandle);
                });
                measurethread.Start();
                //CMoveController.Instance().GotoTerninalPosition();
                Thread.Sleep(1500);
                do
                {
                    if (
                        measurethread.ThreadState != System.Threading.ThreadState.Stopped)
                    {

                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }

                } while (true);


                //CMoveController.Instance().LightNew(false);

                ho_BV2TiledImage.Dispose();
                HOperatorSet.ReadImage(out ho_BV2TiledImage, "E:\\code\\svn\\youeye2.1\\11\\WN3A1D09231N\\隐裂\\20231030093600220_1.tif");

                ho_BV1TiledImage.Dispose();
                HOperatorSet.ReadImage(out ho_BV1TiledImage, "E:\\code\\svn\\youeye2.1\\11\\WN3A1D09231N\\应力\\20231030093600220_1.tif");

                HOperatorSet.SetDictObject(ho_BV2TiledImage, hv_SSDictHandle, "隐裂图1");
                HOperatorSet.SetDictObject(ho_BV1TiledImage, hv_SSDictHandle, "应力图1");

                HOperatorSet.SetDictTuple(hv_SSDictHandle, "Rate", hv_BVRate);
                HTuple hv_o = new HTuple();
                HTuple hv_resultPath = new HTuple();


                HObject ho_BV1TiledImage2 = new HObject();
                HObject ho_BV2TiledImage2 = new HObject();

                if (hv_selected.S == "P")
                {
                    #region Rota 6
                    //MoveControllerModbusTool.Instance().WriteSingleCoil(1002, false);
                    //MoveControllerModbusTool.Instance().WriteSingleCoil(212, true);
                    //Thread.Sleep(6000);
                    //MoveControllerModbusTool.Instance().WriteSingleCoil(212, false);
                    //MoveControllerModbusTool.Instance().WriteSingleCoil(1002, true);
                    #endregion

                    HTuple hv_positionHead = new HTuple();
                    HTuple hv_BVDictHandle = new HTuple();
                    HOperatorSet.CreateDict(out hv_BVDictHandle);

                    CGlobalFuncTools.Instance().Postion_Head(hv_SSDictHandle, out hv_positionHead);

                    //MoveControllerModbusTool.Instance().WriteTwoRegister(72, (float)hv_positionHead.D - 100);

                    //CMoveController.Instance().LightNew(true);
                    //CBVCameraTools.Instance().SetFeatrueValue((int)CBVCameraTools.emBVCameraType.emBV_YL, "Gain", (float)16.0);
                    //CBVCameraTools.Instance().SetFeatrueValue((int)CBVCameraTools.emBVCameraType.emBV_YingLi, "Gain", (float)16.0);

                    //CMoveController.Instance().GotoDestinationAndWait(122);

                    //到位置三， 暂定为起点
                    Thread scanYinLieWC2 = new Thread(() =>
                    {
                        CBVCameraTools.Instance().ScanYinLieAndWCNewNP(hv_ResultDictHandle, hv_ReceiveQueueHandle, hv_LQueueHandle, hv_Data, out ho_BV1TiledImage2, out ho_BV2TiledImage2);
                    });
                    scanYinLieWC2.Start();

                    do
                    {
                        if (scanYinLieWC2.ThreadState != System.Threading.ThreadState.Stopped)
                        {
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            break;
                        }

                    } while (true);

                    //CMoveController.Instance().LightNew(false);
                    HObject ho_SSImage = new HObject();
                    HOperatorSet.GenEmptyObj(out ho_SSImage);

                    HOperatorSet.SetDictObject(ho_BV2TiledImage2, hv_SSDictHandle, "隐裂图2");
                    HOperatorSet.SetDictObject(ho_BV1TiledImage2, hv_SSDictHandle, "应力图2");

                    HOperatorSet.GetDictObject(out ho_SSImage, hv_SSDictHandle, "深视灰度合成图");
                    HOperatorSet.RemoveDictKey(hv_SSDictHandle, "深视灰度合成图");
                    CGlobalFuncTools.Instance().SS_JinXian(cbho_Imageconst, hv_ResultDictHandle, hv_SSDictHandle);
                    CGlobalFuncTools.Instance().Finsh(cbho_Imageconst, hv_ReceiveQueueHandle, hv_ResultDictHandle, hv_SSDictHandle);

                }
                else
                {

                    #region Rota 6
                    //MoveControllerModbusTool.Instance().WriteSingleCoil(1002, false);
                    //MoveControllerModbusTool.Instance().WriteSingleCoil(212, true);
                    //Thread.Sleep(6000);
                    //MoveControllerModbusTool.Instance().WriteSingleCoil(212, false);
                    //MoveControllerModbusTool.Instance().WriteSingleCoil(1002, true);
                    #endregion
                    HTuple hvstr = new HTuple();
                    HOperatorSet.DictToJson(hv_ResultDictHandle, new HTuple(), new HTuple(), out hvstr);
                    HTuple hv_positionHead = new HTuple();
                    CGlobalFuncTools.Instance().Postion_Head(hv_SSDictHandle, out hv_positionHead);
                    //HOperatorSet.WriteImage(ho_BV2TiledImage, "tiff", 0, "D:/123.tif");
                    //MoveControllerModbusTool.Instance().WriteTwoRegister(72, (float)hv_positionHead.D - 100);
                    //CMoveController.Instance().LightNew(true);
                    //CBVCameraTools.Instance().SetFeatrueValue((int)CBVCameraTools.emBVCameraType.emBV_YL, "Gain", (float)2.0);
                    //CBVCameraTools.Instance().SetFeatrueValue((int)CBVCameraTools.emBVCameraType.emBV_YingLi, "Gain", (float)2.0);

                    //到位置三， 暂定为起点
                    //Thread scanYinLieWC2 = new Thread(() =>
                    //{
                    //    CBVCameraTools.Instance().ScanYinLieAndWCNewNP(hv_BVDictHandle, hv_ResultDictHandle, hv_ReceiveQueueHandle, hv_LQueueHandle, hv_Data, out ho_BV1TiledImage2, out ho_BV2TiledImage2);
                    //});
                    //scanYinLieWC2.Start();

                    //CMoveController.Instance().GotoDestinationAndWait(122);

                    //do
                    //{
                    //    if (scanYinLieWC2.ThreadState != System.Threading.ThreadState.Stopped)
                    //    {
                    //        Thread.Sleep(1000);
                    //    }
                    //    else
                    //    {
                    //        break;
                    //    }

                    //} while (true);

                    //CMoveController.Instance().LightNew(false);


                    ho_BV2TiledImage2.Dispose();
                    HOperatorSet.ReadImage(out ho_BV2TiledImage2, "E:\\code\\svn\\youeye2.1\\11\\WN3A1D09231N\\隐裂\\20231030093600220_2.tif");

                    ho_BV1TiledImage2.Dispose();
                    HOperatorSet.ReadImage(out ho_BV1TiledImage2, "E:\\code\\svn\\youeye2.1\\11\\WN3A1D09231N\\应力\\20231030093600220_2.tif");

                    HOperatorSet.SetDictObject(ho_BV2TiledImage2, hv_SSDictHandle, "隐裂图2");
                    HOperatorSet.SetDictObject(ho_BV1TiledImage2, hv_SSDictHandle, "应力图2");


                    //CGlobalFuncTools.Instance().Dectect(out cbho_Imageconst, hv_SSDictHandle, ref hv_ResultDictHandle);
                    CGlobalFuncTools.Instance().Finsh(cbho_Imageconst, hv_ReceiveQueueHandle, hv_ResultDictHandle, hv_SSDictHandle);

                }


                //HTuple tuple = new HTuple();
                ////Laser_HX(0, hv_Coordinate, tuple);
                //Reture_Zero(0, tuple);
                //CMoveController.Instance().GotoOrigin();

                //CMoveController.Instance().TurnOffStick();

            }
            catch (Exception ex)
            {

            }

        }


        private void ThreadScanSiliconStickNPTypeFunc()
        {
            HTuple hv_MessageHandle = new HTuple();
            HTuple hv_Data = new HTuple(), hv_Start = new HTuple();
            HTuple hv_Sublevel = new HTuple(), hv_ZB = new HTuple();
            HObject cbho_Imageconst;
            LogHelper.Info("Silicon", "ThreadScanSiliconStickNPTypeFunc Begin");

            try
            {
                CMoveController.Instance().InitPositionInfo();
                CMoveController.Instance().TurnOnStick();


                #region Ready Start
                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Turn On Stick End");
                CMoveController.Instance().GotoOrigin();  //回原点  离零点很近
                Thread.Sleep(500);

                HOperatorSet.GenEmptyObj(out cbho_Imageconst);
                NSwidthIndex = 0;
                this.Invoke(itemFunc);

                CMoveController.Instance().GoOn(true);
                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin to Find SiliconLine ");
                ProcessManager.Instance().JBANG_Identification();
                //Thread.Sleep(1000);
                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Find SiliconLine End ");
                CMoveController.Instance().GoOn(false);

                ProcessManager.Instance().FindSiliconLineNew();
                #endregion
                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin to Scan Radius ");
                CMoveController.Instance().GotoOrigin();

                Thread.Sleep(2000);

                
                //CMoveController.Instance().GotoOrigin();  //回原点  离零点很近
                //Thread.Sleep(500);


                CMoveController.Instance().LightNew(true);
                HTuple hv_Coordinate = new HTuple();

                HObject ho_BV1TiledImage = new HObject();
                HObject ho_BV2TiledImage = new HObject();

                //到位置三， 暂定为起点
                Thread scanYinLieWC = new Thread(() =>
                {
                    CBVCameraTools.Instance().ScanYinLieAndWCNewNP(hv_ResultDictHandle, hv_ReceiveQueueHandle, hv_LQueueHandle, hv_Data, out ho_BV1TiledImage, out ho_BV2TiledImage);
                });
                scanYinLieWC.Start();
                HTuple hv_SS = new HTuple(0);
                Thread measurethread = new Thread(() =>
                {
                    SSZNCameraTools.Instance().Measure_Two3DNew(out cbho_Imageconst, hv_LQueueHandle, hv_ResultDictHandle, out hv_SS);
                });
                measurethread.Start();
                CMoveController.Instance().GotoTerninalPosition();

                do
                {
                    if (scanYinLieWC.ThreadState != System.Threading.ThreadState.Stopped ||
                        measurethread.ThreadState != System.Threading.ThreadState.Stopped)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }

                } while (true);


                CMoveController.Instance().LightNew(false);

                HTuple hv_Name = new HTuple();
                HTuple hv_HFQ = new HTuple();
                HTuple hv_selected = new HTuple();
                HTuple hv_IDTime = new HTuple();
                HTuple hv_WaferID = new HTuple();

                HTuple hv_BVRate = new HTuple();
                HTuple hv_SSRate = new HTuple();
                hv_SSRate = 0.234900925339;
                hv_BVRate = 0.232352455031599;
                CGlobalFuncTools.Instance().Name_and_other(hv_ResultDictHandle, out hv_selected, out hv_Name, out hv_HFQ, out hv_IDTime, out hv_WaferID);
                HObject ho_BV1TiledImage2 = new HObject();
                HObject ho_BV2TiledImage2 = new HObject();
                HTuple hv_o = new HTuple();
                HTuple hv_resultPath = new HTuple();
                if (hv_selected.S == "P")
                {
                    





                    //CGlobalFuncTools.Instance().SS_JinXian(cbho_Imageconst, ho_BV1TiledImage, ho_BV2TiledImage, hv_BVRate, hv_SSRate, hv_Name, hv_IDTime, hv_SS, out hv_Coordinate, out hv_o, out hv_resultPath);
                }
                else
                {

                    #region Rota 6
                    MoveControllerModbusTool.Instance().WriteSingleCoil(1002, false);
                    MoveControllerModbusTool.Instance().WriteSingleCoil(212, true);

                    Thread.Sleep(6000);
                    MoveControllerModbusTool.Instance().WriteSingleCoil(212, false);
                    MoveControllerModbusTool.Instance().WriteSingleCoil(1002, true);
                    #endregion

                    HOperatorSet.SetDictObject(ho_BV2TiledImage, hv_SSDictHandle, "隐裂图1");
                    HOperatorSet.SetDictObject(ho_BV1TiledImage, hv_SSDictHandle, "应力图1");

                    HTuple hv_positionHead = new HTuple();
                    //CGlobalFuncTools.Instance().Postion_Head(ho_BV2TiledImage, hv_BVRate, out hv_positionHead);
                    CGlobalFuncTools.Instance().Postion_Head(hv_SSDictHandle, out hv_positionHead);
                    MoveControllerModbusTool.Instance().WriteTwoRegister(72, (float)hv_positionHead.D);
                    CMoveController.Instance().LightNew(true);


                    //到位置三， 暂定为起点
                    Thread scanYinLieWC2 = new Thread(() =>
                    {
                        CBVCameraTools.Instance().ScanYinLieAndWCNewNP(hv_ResultDictHandle, hv_ReceiveQueueHandle, hv_LQueueHandle, hv_Data, out ho_BV1TiledImage2, out ho_BV2TiledImage2);
                    });
                    scanYinLieWC2.Start();
                    CMoveController.Instance().GotoOrigin();

                    do
                    {
                        if (scanYinLieWC2.ThreadState != System.Threading.ThreadState.Stopped)
                        {
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            break;
                        }

                    } while (true);

                    CMoveController.Instance().LightNew(false);

                    
                    CGlobalFuncTools.Instance().Dectect(cbho_Imageconst, hv_SS,ho_BV1TiledImage, ho_BV2TiledImage, ho_BV2TiledImage2, out cbho_Imageconst, hv_BVRate, hv_ResultDictHandle, hv_Name, hv_LQueueHandle, hv_IDTime, hv_HFQ, out hv_Coordinate, out hv_resultPath);
                }

               

                //CGlobalFuncTools.Instance().Finsh(ho_BV2TiledImage, ho_BV1TiledImage, ho_BV2TiledImage2, ho_BV2TiledImage2, hv_Coordinate, hv_ReceiveQueueHandle, hv_ResultDictHandle, hv_IDTime, hv_Name, hv_resultPath);
                HTuple tuple = new HTuple();
                Laser_HX(0, hv_Coordinate, tuple);
                Reture_Zero(0, tuple);


                CMoveController.Instance().TurnOffStick();

            }
            catch (Exception ex)
            {

            }

        }

        public void Reture_Zero(HTuple hv_Index, HTuple hv_IO_Result_Handle)
        {
            bool bRefState = false;

            // Initialize local and output iconic variables 
            MoveControllerModbusTool.Instance().WriteTwoRegister(70, 0);
            MoveControllerModbusTool.Instance().WriteSingleCoil(120, true);
            MoveControllerModbusTool.Instance().WriteSingleCoil(176, true);
            MoveControllerModbusTool.Instance().WriteSingleCoil(175, false);
            bRefState = true;
            do
            {
                MoveControllerModbusTool.Instance().ReadSingleCoil(176, ref bRefState);
                if (bRefState == false)
                {
                    break;
                }
                Thread.Sleep(500);
            } while (true);

            bRefState = true;
            do
            {
                MoveControllerModbusTool.Instance().ReadSingleCoil(120, ref bRefState);
                if (bRefState == false)
                {
                    break;
                }
                Thread.Sleep(500);
            } while (true);


            return;
        }
        public void Laser_HX(HTuple hv_Index, HTuple hv_HXZB, HTuple hv_IO_Result_Handle)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_offse = new HTuple(), hv_Index1 = new HTuple();
            HTuple hv_A = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                MoveControllerModbusTool.Instance().WriteTwoRegister(1002, 300);
                HOperatorSet.WaitSeconds(1);
                hv_offse.Dispose();
                hv_offse = 453;
                bool bRefState = false;
                for (hv_Index1 = 0; (int)hv_Index1 <= (int)((new HTuple(hv_HXZB.TupleLength())) - 1); hv_Index1 = (int)hv_Index1 + 1)
                {
                    hv_A.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_A = ((new HTuple(hv_HXZB.TupleLength()
                            )) - 1) - hv_Index1;
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        MoveControllerModbusTool.Instance().WriteTwoRegister( 128, (float)(((hv_HXZB.TupleSelect(hv_A)) + hv_offse).D));
                    }
                    MoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                    do
                    {
                        MoveControllerModbusTool.Instance().ReadSingleCoil(128, ref bRefState);
                        if (bRefState == false)
                        {
                            break;
                        }
                        Thread.Sleep(500);
                    } while (true);
                    HOperatorSet.WaitSeconds(3);
                    MoveControllerModbusTool.Instance().WriteSingleCoil( 232, true);
                    MoveControllerModbusTool.Instance().WriteSingleCoil( 230, true);
                    HOperatorSet.WaitSeconds(3);
                    do
                    {
                        MoveControllerModbusTool.Instance().ReadSingleCoil(230, ref bRefState);
                        if (bRefState == false)
                        {
                            break;
                        }
                        Thread.Sleep(500);
                    } while (true);
                }

                hv_offse.Dispose();
                hv_Index1.Dispose();
                hv_A.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_offse.Dispose();
                hv_Index1.Dispose();
                hv_A.Dispose();

            }
        }
        private void ThreadScanSiliconStickMotionCardVirtualNodeviceFunc()
        {
            HTuple hv_MessageHandle = new HTuple();
            HTuple hv_Data = new HTuple(), hv_Start = new HTuple();
            HTuple hv_Sublevel = new HTuple(), hv_ZB = new HTuple();
            HObject cbho_Imageconst;
            LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin");
            LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Init Move Position Information");
            try
            {
               
                //CMoveController.Instance().TurnOnStick();

                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Turn On Stick End");
                
                Thread.Sleep(500);

                HOperatorSet.GenEmptyObj(out cbho_Imageconst);
                //NSwidthIndex = 0;
                //this.Invoke(itemFunc);


               
                //CMoveController.Instance().GoOn(true);
                //LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin to Find SiliconLine ");
                //ProcessManager.Instance().JBANG_Identification();
                //Thread.Sleep(1000);
                //LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Find SiliconLine End ");
                //CMoveController.Instance().GoOn(false);

               


               
                Thread.Sleep(1000);


                hv_MessageHandle.Dispose();
                HOperatorSet.CreateMessage(out hv_MessageHandle);
                HOperatorSet.SetMessageTuple(hv_MessageHandle, "MeasureStart", 1);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.EnqueueMessage(hv_LQueueHandle, hv_MessageHandle, new HTuple(),
                        new HTuple());
                }
                HOperatorSet.ClearMessage(hv_MessageHandle);

                //hv_MessageHandle.Dispose();
                //HOperatorSet.CreateMessage(out hv_MessageHandle);
                //HOperatorSet.SetMessageTuple(hv_MessageHandle, "Start", 1);
                //using (HDevDisposeHelper dh = new HDevDisposeHelper())
                //{
                //    HOperatorSet.EnqueueMessage(hv_CQ.TupleSelect(2), hv_MessageHandle, new HTuple(),
                //        new HTuple());
                //}
                //HOperatorSet.ClearMessage(hv_MessageHandle);

                hv_MessageHandle.Dispose();
                HOperatorSet.CreateMessage(out hv_MessageHandle);
                HOperatorSet.SetMessageTuple(hv_MessageHandle, "BVStart", 1);
                HOperatorSet.SetMessageTuple(hv_MessageHandle, "Start", 1);
                HOperatorSet.SetMessageTuple(hv_MessageHandle, "Sublevel", 6);
                HOperatorSet.SetMessageTuple(hv_MessageHandle, "ZB", 1);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.EnqueueMessage(hv_BVQueueHandle, hv_MessageHandle, new HTuple(),
                        new HTuple());
                }
                HOperatorSet.ClearMessage(hv_MessageHandle);

                //开灯，开始扫描
                //CMoveController.Instance().LightNew(true);
                HTuple hv_Coordinate = new HTuple();


                //到位置三， 暂定为起点
                Thread scanYinLieWC = new Thread(() =>
                {
                    CBVCameraTools.Instance().ScanYinLieAndWCNew(hv_ResultDictHandle, hv_ReceiveQueueHandle, hv_LQueueHandle, hv_Data, out hv_Coordinate);
                });
                scanYinLieWC.Start();
                HTuple hv_ss = new HTuple();
                Thread measurethread = new Thread(() =>
                {
                    SSZNCameraTools.Instance().Measure_Two3DNew(out cbho_Imageconst, hv_LQueueHandle, hv_ResultDictHandle, out hv_ss);

                });
                measurethread.Start();
                //CMoveController.Instance().GotoTerninalPosition();
                do
                {
                    if (scanYinLieWC.ThreadState != System.Threading.ThreadState.Stopped ||
                        measurethread.ThreadState != System.Threading.ThreadState.Stopped)
                    {

                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }

                } while (true);


                Thread.Sleep(1000);

                //CMoveController.Instance().TurnOffStick();
            
            }
            catch (HalconException HDevExpDefaultException)
            {
                LogHelper.Info("", "Exception Thread Start Scan Silicon Stick");
            }


            //******画线完成，回原点*********
            /*MoveControllerModbusTool.Instance().WriteTwoRegister(70, 0);
            MoveControllerModbusTool.Instance().WriteSingleCoil(120, true);
            MoveControllerModbusTool.Instance().WriteSingleCoil(176, true);
            MoveControllerModbusTool.Instance().WriteSingleCoil(175, false);

            bool bRefState = false;
            do
            {
                MoveControllerModbusTool.Instance().ReadSingleCoil(22, ref bRefState);
                if (bRefState == false)
                {
                    break;
                }
                Thread.Sleep(500);
            } while (true);

            do
            {
                MoveControllerModbusTool.Instance().ReadSingleCoil(120, ref bRefState);
                if (bRefState == false)
                {
                    break;
                }
                Thread.Sleep(500);
            } while (true);
            */
            HOperatorSet.WaitSeconds(1);
        }

        private void ThreadScanSiliconStickMotionCardVirtualOnceFunc()
        {
            HTuple hv_MessageHandle = new HTuple();
            HTuple hv_Data = new HTuple(), hv_Start = new HTuple();
            HTuple hv_Sublevel = new HTuple(), hv_ZB = new HTuple();
            HObject cbho_Imageconst;
            LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin");

            LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Init Move Position Information");
            try
            {
                CMoveController.Instance().InitPositionInfo();
                //CMoveController.Instance().TurnOnStick();

                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Turn On Stick End");
                CMoveController.Instance().GotoOrigin();  //回原点  离零点很近
                Thread.Sleep(500);

                HOperatorSet.GenEmptyObj(out cbho_Imageconst);
                NSwidthIndex = 0;
                this.Invoke(itemFunc);

                CMoveController.Instance().GotoDestinationAndWait(126);
                //CMoveController.Instance().GotoDestPositonNew(SettingParameter.Instance().NMotionCardSiliconLine);
                //CMoveController.Instance().GoOn(true);
                //LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin to Find SiliconLine ");
                //ProcessManager.Instance().JBANG_Identification();
                //Thread.Sleep(1000);
                //LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Find SiliconLine End ");
                //CMoveController.Instance().GoOn(false);

                ProcessManager.Instance().FindSiliconLineNew();



                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin to Scan Radius ");
                CMoveController.Instance().GotoOrigin();

                Thread.Sleep(1000);


                hv_MessageHandle.Dispose();
                HOperatorSet.CreateMessage(out hv_MessageHandle);
                HOperatorSet.SetMessageTuple(hv_MessageHandle, "MeasureStart", 1);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.EnqueueMessage(hv_LQueueHandle, hv_MessageHandle, new HTuple(),
                        new HTuple());
                }
                HOperatorSet.ClearMessage(hv_MessageHandle);

                //hv_MessageHandle.Dispose();
                //HOperatorSet.CreateMessage(out hv_MessageHandle);
                //HOperatorSet.SetMessageTuple(hv_MessageHandle, "Start", 1);
                //using (HDevDisposeHelper dh = new HDevDisposeHelper())
                //{
                //    HOperatorSet.EnqueueMessage(hv_CQ.TupleSelect(2), hv_MessageHandle, new HTuple(),
                //        new HTuple());
                //}
                //HOperatorSet.ClearMessage(hv_MessageHandle);

                hv_MessageHandle.Dispose();
                HOperatorSet.CreateMessage(out hv_MessageHandle);
                HOperatorSet.SetMessageTuple(hv_MessageHandle, "BVStart", 1);
                HOperatorSet.SetMessageTuple(hv_MessageHandle, "Start", 1);
                HOperatorSet.SetMessageTuple(hv_MessageHandle, "Sublevel", 6);
                HOperatorSet.SetMessageTuple(hv_MessageHandle, "ZB", 1);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.EnqueueMessage(hv_BVQueueHandle, hv_MessageHandle, new HTuple(),
                        new HTuple());
                }
                HOperatorSet.ClearMessage(hv_MessageHandle);

                //开灯，开始扫描
                CMoveController.Instance().LightNew(true);
                HTuple hv_Coordinate = new HTuple();


                //到位置三， 暂定为起点
                Thread scanYinLieWC = new Thread(() =>
                {
                    CBVCameraTools.Instance().ScanYinLieAndWCNew(hv_ResultDictHandle, hv_ReceiveQueueHandle, hv_LQueueHandle, hv_Data, out hv_Coordinate);
                });
                scanYinLieWC.Start();
                HTuple hv_ss = new HTuple();
                Thread measurethread = new Thread(() =>
                {
                    SSZNCameraTools.Instance().Measure_Two3DNew(out cbho_Imageconst, hv_LQueueHandle, hv_ResultDictHandle, out hv_ss);
                });
                measurethread.Start();
                CMoveController.Instance().GotoTerninalPosition();
                do
                {
                    if (scanYinLieWC.ThreadState != System.Threading.ThreadState.Stopped ||
                        measurethread.ThreadState != System.Threading.ThreadState.Stopped)
                    {

                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }

                } while (true);


                CMoveController.Instance().LightNew(false);
                //CMoveController.Instance().GotoDestPositonNewby(300);


                CMoveController.Instance().GotoOrigin();
                HTuple hv_offse = new HTuple();
                HTuple hv_Index1 = new HTuple();
                HOperatorSet.WaitSeconds(1);
                hv_offse.Dispose();
                hv_offse = 453;
                //modbus_write_float (Index, 128, HXZB[0]+offse)
                //modbus_write_bit (Index, 128, 1)
                //wait_arrive (Index, 128, IO_Result_Handle)
                //wait_seconds (3)
                //modbus_write_bit (Index, 232, 1) // 启用模板22
                //modbus_write_bit (Index, 230, 1) // 开始激光打标
                //wait_seconds (3)
                //wait_arrive (Index, 230, IO_Result_Handle)
                //const int nWaitCount = 10;
                //int nWaitIndex = 0;

                //LogHelper.Info("", "Laser hv_Coordinate " + hv_Coordinate.TupleLength().ToString());
                //for (hv_Index1 = 0; (int)hv_Index1 <= (int)((new HTuple(hv_Coordinate.TupleLength()
                //)) - 1); hv_Index1 = (int)hv_Index1 + 1)
                //{
                //    LogHelper.Info("", "Laser hv_index1 " + hv_Index1.I.ToString());
                //    nWaitIndex = 0;
                //    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                //    {
                //        MoveControllerModbusTool.Instance().WriteTwoRegister(128, (float)((hv_Coordinate.TupleSelect(((new HTuple(hv_Coordinate.TupleLength()
                //            )) - 1) - hv_Index1)) + hv_offse).D);

                //    }
                //    bool bState = false;
                //    MoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                //    do
                //    {
                //        MoveControllerModbusTool.Instance().ReadSingleCoil(128, ref bState);
                //        if (bState == false)
                //        {
                //            break;
                //        }
                //        Thread.Sleep(1500);
                //        nWaitIndex++;
                //        if (nWaitIndex >= nWaitCount)
                //        {
                //            FormMain.formMainF.showMessageDelegate.Invoke("请检查下激光与PLC的通信", (int)FormMain.emMSGTYPE.EM_LASER_EXCEPTION);
                //            break;
                //        }
                //    } while (true);
                //    //wait_arrive(hv_Index, 128, hv_IO_Result_Handle);
                //    HOperatorSet.WaitSeconds(3);
                //    MoveControllerModbusTool.Instance().WriteSingleCoil(232, true);
                //    MoveControllerModbusTool.Instance().WriteSingleCoil(230, true);
                //    HOperatorSet.WaitSeconds(3);
                //    nWaitIndex = 0;
                //    //wait_arrive(hv_Index, 230, hv_IO_Result_Handle);
                //    do
                //    {
                //        MoveControllerModbusTool.Instance().ReadSingleCoil(230, ref bState);
                //        if (bState == false)
                //        {
                //            break;
                //        }
                //        Thread.Sleep(1500);
                //        nWaitIndex++;
                //        if (nWaitIndex >= nWaitCount)
                //        {
                //            FormMain.formMainF.showMessageDelegate.Invoke("请检查下激光与PLC的通信", (int)FormMain.emMSGTYPE.EM_LASER_EXCEPTION);
                //            break;
                //        }
                //    } while (true);
                //}
            }
            catch (HalconException HDevExpDefaultException)
            {
                LogHelper.Info("", "Exception Thread Start Scan Silicon Stick");
            }


            //******画线完成，回原点*********
            /*MoveControllerModbusTool.Instance().WriteTwoRegister(70, 0);
            MoveControllerModbusTool.Instance().WriteSingleCoil(120, true);
            MoveControllerModbusTool.Instance().WriteSingleCoil(176, true);
            MoveControllerModbusTool.Instance().WriteSingleCoil(175, false);

            bool bRefState = false;
            do
            {
                MoveControllerModbusTool.Instance().ReadSingleCoil(22, ref bRefState);
                if (bRefState == false)
                {
                    break;
                }
                Thread.Sleep(500);
            } while (true);

            do
            {
                MoveControllerModbusTool.Instance().ReadSingleCoil(120, ref bRefState);
                if (bRefState == false)
                {
                    break;
                }
                Thread.Sleep(500);
            } while (true);
            */
            HOperatorSet.WaitSeconds(1);
        }


        private void ThreadScanSiliconStickMotionCardOnceFunc()
        {
            HTuple hv_MessageHandle = new HTuple();
            HTuple hv_Data = new HTuple(), hv_Start = new HTuple();
            HTuple hv_Sublevel = new HTuple(), hv_ZB = new HTuple();
            HObject cbho_Imageconst;
            LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin");

            LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Init Move Position Information");
            try
            {
                CMoveController.Instance().InitPositionInfo();
                CMoveController.Instance().TurnOnStick();

               
                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Turn On Stick End");
                CMoveController.Instance().GotoOrigin();  //回原点  离零点很近
                Thread.Sleep(500);

                HOperatorSet.GenEmptyObj(out cbho_Imageconst);
                NSwidthIndex = 0;
                this.Invoke(itemFunc);

                CMoveController.Instance().GoOn(true);
                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin to Find SiliconLine ");
                ProcessManager.Instance().JBANG_Identification();
                //Thread.Sleep(1000);
                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Find SiliconLine End ");
                CMoveController.Instance().GoOn(false);

                ProcessManager.Instance().FindSiliconLineNew();
           
            

                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin to Scan Radius ");
                CMoveController.Instance().GotoOrigin();

                Thread.Sleep(2000);


                hv_MessageHandle.Dispose();
                HOperatorSet.CreateMessage(out hv_MessageHandle);
                HOperatorSet.SetMessageTuple(hv_MessageHandle, "MeasureStart", 1);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.EnqueueMessage(hv_LQueueHandle, hv_MessageHandle, new HTuple(),
                        new HTuple());
                }
                HOperatorSet.ClearMessage(hv_MessageHandle);

                //hv_MessageHandle.Dispose();
                //HOperatorSet.CreateMessage(out hv_MessageHandle);
                //HOperatorSet.SetMessageTuple(hv_MessageHandle, "Start", 1);
                //using (HDevDisposeHelper dh = new HDevDisposeHelper())
                //{
                //    HOperatorSet.EnqueueMessage(hv_CQ.TupleSelect(2), hv_MessageHandle, new HTuple(),
                //        new HTuple());
                //}
                //HOperatorSet.ClearMessage(hv_MessageHandle);

                hv_MessageHandle.Dispose();
                HOperatorSet.CreateMessage(out hv_MessageHandle);
                HOperatorSet.SetMessageTuple(hv_MessageHandle, "BVStart", 1);
                HOperatorSet.SetMessageTuple(hv_MessageHandle, "Start", 1);
                HOperatorSet.SetMessageTuple(hv_MessageHandle, "Sublevel", 6);
                HOperatorSet.SetMessageTuple(hv_MessageHandle, "ZB", 1);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.EnqueueMessage(hv_BVQueueHandle, hv_MessageHandle, new HTuple(),
                        new HTuple());
                }
                HOperatorSet.ClearMessage(hv_MessageHandle);

                //开灯，开始扫描
                CMoveController.Instance().LightNew(true);     
                HTuple hv_Coordinate = new HTuple();


                //到位置三， 暂定为起点
                Thread scanYinLieWC = new Thread(() =>
                {
                    CBVCameraTools.Instance().ScanYinLieAndWCNew(hv_ResultDictHandle, hv_ReceiveQueueHandle, hv_LQueueHandle,hv_Data, out hv_Coordinate);
                });
                scanYinLieWC.Start();
                HTuple hv_ss = new HTuple();
                Thread measurethread = new Thread(() =>
                {
                    SSZNCameraTools.Instance().Measure_Two3DNew(out cbho_Imageconst, hv_LQueueHandle, hv_ResultDictHandle, out hv_ss);
                });
                measurethread.Start();
                CMoveController.Instance().GotoTerninalPosition();
                do
                {
                    if (scanYinLieWC.ThreadState != System.Threading.ThreadState.Stopped ||
                        measurethread.ThreadState != System.Threading.ThreadState.Stopped)
                    {
                    
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }

                } while (true);


                CMoveController.Instance().LightNew(false);
                //CMoveController.Instance().GotoDestPositonNewby(300);

                HTuple hv_offse = new HTuple();
                HTuple hv_Index1 = new HTuple();
                HOperatorSet.WaitSeconds(1);
                hv_offse.Dispose();
                hv_offse = 453;
                //modbus_write_float (Index, 128, HXZB[0]+offse)
                //modbus_write_bit (Index, 128, 1)
                //wait_arrive (Index, 128, IO_Result_Handle)
                //wait_seconds (3)
                //modbus_write_bit (Index, 232, 1) // 启用模板22
                //modbus_write_bit (Index, 230, 1) // 开始激光打标
                //wait_seconds (3)
                //wait_arrive (Index, 230, IO_Result_Handle)
                const int nWaitCount = 10;
                int nWaitIndex = 0;

                LogHelper.Info("", "Laser hv_Coordinate " + hv_Coordinate.TupleLength().ToString());
                for (hv_Index1 = 0; (int)hv_Index1 <= (int)((new HTuple(hv_Coordinate.TupleLength()
                )) - 1); hv_Index1 = (int)hv_Index1 + 1)
                {
                    LogHelper.Info("", "Laser hv_index1 " + hv_Index1.I.ToString());
                    nWaitIndex = 0;
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        MoveControllerModbusTool.Instance().WriteTwoRegister(128, (float)((hv_Coordinate.TupleSelect(((new HTuple(hv_Coordinate.TupleLength()
                            )) - 1) - hv_Index1)) + hv_offse).D);

                    }
                    bool bState = false;
                    MoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                    do
                    {
                        MoveControllerModbusTool.Instance().ReadSingleCoil(128, ref bState);
                        if (bState == false)
                        {
                            break;
                        }
                        Thread.Sleep(1500);
                        //nWaitIndex++;
                        //if (nWaitIndex >= nWaitCount)
                        //{
                        //    FormMain.formMainF.showMessageDelegate.Invoke("请检查下激光与PLC的通信", (int)FormMain.emMSGTYPE.EM_LASER_EXCEPTION);
                        //    break;
                        //}
                    } while (true);
                    //wait_arrive(hv_Index, 128, hv_IO_Result_Handle);
                    HOperatorSet.WaitSeconds(3);
                    MoveControllerModbusTool.Instance().WriteSingleCoil(232, true);
                    MoveControllerModbusTool.Instance().WriteSingleCoil(230, true);
                    HOperatorSet.WaitSeconds(3);
                    nWaitIndex = 0;
                    //wait_arrive(hv_Index, 230, hv_IO_Result_Handle);
                    do
                    {
                        MoveControllerModbusTool.Instance().ReadSingleCoil(230, ref bState);
                        if (bState == false)
                        {
                            break;
                        }
                        Thread.Sleep(1500);
                        nWaitIndex++;
                        //if (nWaitIndex >= nWaitCount)
                        //{
                        //    FormMain.formMainF.showMessageDelegate.Invoke("请检查下激光与PLC的通信", (int)FormMain.emMSGTYPE.EM_LASER_EXCEPTION);
                        //    break;
                        //}
                    } while (true);
                }
            }
            catch(HalconException HDevExpDefaultException)
            {
                LogHelper.Info("", "Exception Thread Start Scan Silicon Stick");
            }


            //******画线完成，回原点*********
            MoveControllerModbusTool.Instance().WriteTwoRegister(70, 0);
            MoveControllerModbusTool.Instance().WriteSingleCoil(120, true);
            MoveControllerModbusTool.Instance().WriteSingleCoil(176, true);
            MoveControllerModbusTool.Instance().WriteSingleCoil(175, false);

            bool bRefState = false;
            do
            {
                MoveControllerModbusTool.Instance().ReadSingleCoil(22, ref bRefState);
                if (bRefState == false)
                {
                    break;
                }
                Thread.Sleep(500);
            } while (true);

            do
            {
                MoveControllerModbusTool.Instance().ReadSingleCoil(120, ref bRefState);
                if (bRefState == false)
                {
                    break;
                }
                Thread.Sleep(500);
            } while (true);
          
            HOperatorSet.WaitSeconds(1);
        }

        private void ThreadScanSiliconStickMockFunc()
        {
            try
            {

                HTuple hv_MessageHandle = new HTuple();
                HTuple hv_Data = new HTuple(), hv_Start = new HTuple();
                HTuple hv_Sublevel = new HTuple(), hv_ZB = new HTuple();

                
                Thread.Sleep(1000);

                NSwidthIndex = 0;
                this.Invoke(itemFunc);

                
                //3D扫描
                Thread.Sleep(100);
                // Output parameters
                HObject cbho_Imageconst;

                //红外BV,3D一起 一个扫描经线，一个是直径
                // Call Measure_3D
                HTuple hv_ss = new HTuple();
                Thread measurethread = new Thread(() =>
                {
                    SSZNCameraTools.Instance().Measure_Two3DNew(out cbho_Imageconst, hv_LQueueHandle, hv_ResultDictHandle, out hv_ss);
                });
                measurethread.Start();

                Thread scanSiliconLineThread = new Thread(() =>
                {
                    CBVCameraTools.Instance().ScanSiliconLine(hv_LQueueHandle, hv_ResultDictHandle, hv_ReceiveQueueHandle, hv_BVQueueHandle);
                });

                scanSiliconLineThread.Start();
                Thread.Sleep(1000);
               

                //DealSiliconStickPosition(500, 500);
                //重新计算的尾部位置  122 位置二
                do
                {
                    if (measurethread.ThreadState != System.Threading.ThreadState.Stopped || scanSiliconLineThread.ThreadState != System.Threading.ThreadState.Stopped)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }

                } while (true);

                Thread.Sleep(1000);
                do
                {
                    try
                    {
                        hv_MessageHandle.Dispose();

                        HOperatorSet.DequeueMessage(hv_BVQueueHandle, "timeout", 1, out hv_MessageHandle);
                        hv_Data.Dispose();
                        hv_Start.Dispose();
                        hv_Sublevel.Dispose();
                        hv_ZB.Dispose();

                        HOperatorSet.GetMessageTuple(hv_MessageHandle, "BVStart", out hv_Data); // 1
                        HOperatorSet.GetMessageTuple(hv_MessageHandle, "Start", out hv_Start); //data
                        HOperatorSet.GetMessageTuple(hv_MessageHandle, "Sublevel", out hv_Sublevel); //计算位置与当前位置
                        HOperatorSet.GetMessageTuple(hv_MessageHandle, "ZB", out hv_ZB); //corrdinate


                        break;
                    }
                    // catch (Exception) 
                    catch (HalconException HDevExpDefaultException2)
                    {

                    }
                    Thread.Sleep(500);

                } while (true);


               
                Thread.Sleep(1000);
                

                //到位置三， 暂定为起点
                Thread scanYinLieWC = new Thread(() =>
                {
                    CBVCameraTools.Instance().ScanYinLieAndWC(hv_ResultDictHandle, hv_ReceiveQueueHandle, hv_ZB, hv_Data, hv_Start, hv_Sublevel.TupleSelect(0).I);
                });
                scanYinLieWC.Start();


                Thread scanLJ = new Thread(() =>
                {
                    if (SettingParameter.Instance().NDaemon > 0)
                    {
                        CHikCameraTools.Instance().ClearImageBuffer((int)CHikCameraTools.emHikCameraType.emLJ);
                        CHikCameraTools.Instance().StartPlay((int)(CHikCameraTools.emHikCameraType.emLJ));
                        CHikCameraTools.Instance().ScanLJ(hv_ResultDictHandle, hv_ReceiveQueueHandle, hv_Sublevel.I);
                        CHikCameraTools.Instance().StopPlay((int)CHikCameraTools.emHikCameraType.emLJ);

                    }
                    else
                    {

                    }
                });
                scanLJ.Start();



                Thread.Sleep(500);
                //CMoveController.Instance().GotoOrigin();
                

                do
                {
                    if (scanYinLieWC.ThreadState != System.Threading.ThreadState.Stopped ||
                        scanLJ.ThreadState != System.Threading.ThreadState.Stopped)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }

                } while (true);

                NSwidthIndex = 1;
                this.Invoke(itemFunc);

                Thread.Sleep(500);
               
            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon", "ThreadScanSiliconStick exception " + ex.Message);
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
                CMoveController.Instance().TurnOnStick();

                 LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Turn On Stick End");
                CMoveController.Instance().GotoOrigin();  //回原点  离零点很近
                Thread.Sleep(1000);

                NSwidthIndex = 0;
                this.Invoke(itemFunc);

                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin to Find SiliconLine ");
                CMoveController.Instance().GotoSiliconLinePosition();
                LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Find SiliconLine End ");
                ProcessManager.Instance().FindSiliconLineNew();

                 LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin to Scan Radius ");
                //CMoveController.Instance().GotoOrigin();
                CMoveController.Instance().GotoTerninalPosition();

                //3D开
                //RedisTool.Instance().RedisPub("3DInfo", "1");

                //3D扫描
                Thread.Sleep(100);
                // Output parameters
                HObject cbho_Imageconst;

                //红外BV,3D一起 一个扫描经线，一个是直径
                // Call Measure_3D
                Thread measurethread = new Thread(() =>
                {
                    SSZNCameraTools.Instance().Measure_3D(out cbho_Imageconst, hv_LQueueHandle, hv_ResultDictHandle);
                });
                measurethread.Start();

                Thread scanSiliconLineThread = new Thread(() =>
                {
                    CBVCameraTools.Instance().ScanSiliconLine(hv_LQueueHandle, hv_ResultDictHandle, hv_ReceiveQueueHandle, hv_BVQueueHandle);
                });

                scanSiliconLineThread.Start();

                Thread.Sleep(1000);
                //CMoveController.Instance().GotoTerninalPosition();
                CMoveController.Instance().GotoOrigin();

                /*
                if (0 == SSZNCamera.Instance().StickLength)
                {
                    FormMain.formMainF.showMessageDelegate.Invoke("SSZN Measure Exception, SSZN Camera 配置错误！请查看是否有灰度图！ 计算直径错误！ ", (int)FormMain.emMSGTYPE.EM_SSZNCAMERA_EXCEPTION);
                    return;
                }*/

                //DealSiliconStickPosition(500, 500);
                //重新计算的尾部位置  122 位置二
                do
                {
                    if (measurethread.ThreadState != System.Threading.ThreadState.Stopped || scanSiliconLineThread.ThreadState != System.Threading.ThreadState.Stopped)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }

                } while (true);

                Thread.Sleep(1000);
                //CMoveController.Instance().GotoTerninalPosition();
                do
                {
                    try
                    {
                        hv_MessageHandle.Dispose();

                        HOperatorSet.DequeueMessage(hv_BVQueueHandle, "timeout", 1, out hv_MessageHandle);
                        hv_Data.Dispose();
                        hv_Start.Dispose();
                        hv_Sublevel.Dispose();
                        hv_ZB.Dispose();
                       
                        HOperatorSet.GetMessageTuple(hv_MessageHandle, "BVStart", out hv_Data); // 1
                        HOperatorSet.GetMessageTuple(hv_MessageHandle, "Start", out hv_Start); //data
                        HOperatorSet.GetMessageTuple(hv_MessageHandle, "Sublevel", out hv_Sublevel); //计算位置与当前位置
                        HOperatorSet.GetMessageTuple(hv_MessageHandle, "ZB", out hv_ZB); //corrdinate


                        break;
                    }
                    // catch (Exception) 
                    catch (HalconException HDevExpDefaultException2)
                    {
                        
                    }
                    Thread.Sleep(500);

                } while (true);

                
                CMoveController.Instance().GotoOrigin();
                Thread.Sleep(1000);
                //旋转 避开晶线
                CMoveController.Instance().RotateSiliconStick(true);
                Thread.Sleep(4000);
                CMoveController.Instance().RotateSiliconStick(false);


                //到位置三， 暂定为起点
                Thread scanYinLieWC = new Thread(() =>
                {
                    CBVCameraTools.Instance().ScanYinLieAndWC(hv_ResultDictHandle, hv_ReceiveQueueHandle, hv_ZB, hv_Data, hv_Start ,hv_Sublevel.TupleSelect(0).TupleInt());
                });
                scanYinLieWC.Start();

                
                Thread scanLJ = new Thread(() =>
                {
                    if (SettingParameter.Instance().NDaemon > 0)
                    {
                        CHikCameraTools.Instance().ClearImageBuffer((int)CHikCameraTools.emHikCameraType.emLJ);
                        CHikCameraTools.Instance().StartPlay((int)(CHikCameraTools.emHikCameraType.emLJ));
                        CHikCameraTools.Instance().ScanLJ(hv_ResultDictHandle, hv_ReceiveQueueHandle, hv_Sublevel.I);
                        CHikCameraTools.Instance().StopPlay((int)CHikCameraTools.emHikCameraType.emLJ);

                    }
                    else
                    {

                    }
                });
                scanLJ.Start();

                
                Thread.Sleep(500);
                //CMoveController.Instance().GotoOrigin();
                CMoveController.Instance().GotoTerninalPosition();


                do
                {
                    if (scanYinLieWC.ThreadState != System.Threading.ThreadState.Stopped || 
                        scanLJ.ThreadState != System.Threading.ThreadState.Stopped)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }

                } while (true);

                NSwidthIndex = 1;
                this.Invoke(itemFunc);

                Thread.Sleep(500);
                CMoveController.Instance().ResetTailPosition();
                
            }
            catch (Exception ex)
            {
                 LogHelper.Info("Silicon", "ThreadScanSiliconStick exception " + ex.Message);
            }

        }


        private void _btnSet_Click_1(object sender, EventArgs e)
        {
            if (_siliconStickNumTextbox.Text.Length == 0)
            {
                MessageBox.Show("晶编不能为空！");
                return;
            }
            if (_siliconStickNumTextbox.Text.Length <= 3)
            {
                MessageBox.Show("晶编长度大于3！");
                return;
            }

            if (SettingParameter.Instance().NDaemon > 0)
            {
                if (CHikCameraTools.Instance().IsCamerasPrepared() == false)
                {
                    MessageBox.Show("Hik 相机有问题，请检查下相机！");
                    return;
                }

                if (CBVCameraTools.Instance().IsCamerasPrepared() == false)
                {
                    MessageBox.Show("BV 相机有问题，请检查下相机！");
                    return;
                }
            }
            

            if (_cartesianRadiusChart.AxisX[0].Sections.Count > 0)
            {
                for (int i = 0; i < _cartesianRadiusChart.AxisX[0].Sections.Count - 1; i++)
                {
                    _cartesianRadiusChart.AxisX[0].Sections[i].Value = -1;
                }
            }
            if (_cartesianYingLiChart.AxisX[0].Sections.Count > 0)
            {
                for (int i = 0; i < _cartesianYingLiChart.AxisX[0].Sections.Count - 1; i++)
                {
                    _cartesianYingLiChart.AxisX[0].Sections[i].Value = -1;
                }
            }

            //if (_cartesianPenetrationRateResultView.AxisX[0].Sections.Count > 0)
            //{
            //    for (int i = 0; i < _cartesianPenetrationRateResultView.AxisX[0].Sections.Count - 1; i++)
            //    {
            //        _cartesianPenetrationRateResultView.AxisX[0].Sections[i].Value = -1;

            //    }
            //}

            if (_cartesianResisvityChart.AxisX[0].Sections.Count > 0)
            {
                for (int i = 0; i < _cartesianResisvityChart.AxisX[0].Sections.Count - 1; i++)
                {
                    _cartesianResisvityChart.AxisX[0].Sections[i].Value = -1;
                }
            }
            RadiusChartValues.Clear();
            YingLiChartValues.Clear();
            ResivityChartValues.Clear();
            RadiusChartViewValues.Clear();
            YingLiChartViewValues.Clear();
            ResivityChartViewValues.Clear();
            //PenetrationRateChartValues.Clear();
            _btnSet.Enabled = false;
            _strGetStickLineNum = _siliconStickNumTextbox.Text;
            //RedisTool.Instance().RedisPub("JingBian", _siliconStickNumTextbox.Text);

            HOperatorSet.SetDictTuple(hv_ResultDictHandle, "晶编",  new HTuple(_siliconStickNumTextbox.Text));
            string strSiliconNum = _siliconStickNumTextbox.Text;
            if (strSiliconNum[2] == 'P' || strSiliconNum[2] == 'p')
            {
                SettingParameter.Instance().NSiliconType = 2;
                CBVCameraTools.Instance().SetValue((int)CBVCameraTools.emBVCameraType.emBV_YL, "ExposureTime", "100");
                CBVCameraTools.Instance().SetValue((int)CBVCameraTools.emBVCameraType.emBV_YingLi, "ExposureTime", "100");

            }
            else
            {
                SettingParameter.Instance().NSiliconType = 1;
                CBVCameraTools.Instance().SetValue((int)CBVCameraTools.emBVCameraType.emBV_YL, "ExposureTime", "1000");
                CBVCameraTools.Instance().SetValue((int)CBVCameraTools.emBVCameraType.emBV_YingLi, "ExposureTime", "1000");
            }
            if (SettingParameter.Instance().NDaemon == 1)
            {
                m_threadScanStick = new Thread(ThreadScanSiliconStickMotionCardFunc);

            }
            else if (SettingParameter.Instance().NDaemon == 2)
            {
                m_threadScanStick = new Thread(ThreadScanSiliconStickMotionCardOnceFunc);
            }
            else if (SettingParameter.Instance().NDaemon == 3)
            {
                m_threadScanStick = new Thread(ThreadScanSiliconStickNPTypeFunc);
            }
            else if (SettingParameter.Instance().NDaemon == -1)
            {
                m_threadScanStick = new Thread(ThreadScanSiliconStickMotionCardVirtualOnceFunc);
                //m_threadScanStick = new Thread(ThreadScanSiliconStickMotionCardVirtualNodeviceFunc);
            }
            else
            {
                m_threadScanStick = new Thread(ThreadScanSiliconStickMotionCardVirtualNodeviceFunc);
                //m_threadScanStick = new Thread(ThreadScanSiliconStickMockFunc);
            }
            

            m_threadScanStick.Start();

        }

        private void _rightBtn_Click(object sender, System.EventArgs e)
        {

            FormMain.formMainF.showAsideFunc();
            this._rightBtn.Visible = false;
            MovePage.instance.ShowRightBtnFunc(true);
            MainResultLinesListInfoPage.instance.ShowRightBtnFunc(false);
            MainResultLinesListInfoPage.instance.ShowRightBtnFunc(false);
            SettingParamPage.instance.ShowRightBtnFunc(false);
            MainStatisticPage.instance.ShowRightBtnFunc(false);
            
        }
    }
}