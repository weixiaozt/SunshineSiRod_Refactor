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

namespace SiliconRoundBarCheck.Pages
{
    public partial class MainResultLineInfoPage : UIPage
    {
        private System.Windows.Forms.Timer _timeRefresh;

        private System.Windows.Forms.Timer _timeRadiusRefresh;

        private System.Windows.Forms.Timer _timeYingLiRefresh;

        private System.Windows.Forms.Timer _timeResivityRefresh;

        private System.Windows.Forms.Timer _timePenetrationRateRefresh;

        private System.Windows.Forms.Timer _timeStatusFormMonitorRefresh;

        private System.Windows.Forms.Timer _tim;

        private System.Windows.Forms.Timer _timeInitMoveController;
        private string _strGetStickLineNum = ""; //晶编  
        public ChartValues<ObservablePoint> RadiusChartValues { get; set; }
        public ChartValues<ObservablePoint> ResivityChartValues { get; set; }
        public ChartValues<ObservablePoint> YingLiChartValues { get; set; }
        public ChartValues<ObservablePoint> PenetrationRateChartValues { get; set; }
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

        private ArrayList _preShowRadius;
        private ArrayList _preShowYingLi;
        private ArrayList _preShowResisvity;
        private ArrayList _preShowPenetrationRate;
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
            EM_BVYINGLI = 3
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

            hv_ResultDictHandle = new HTuple();
            hv_LQueueHandle = new HTuple();
            hv_ReceiveQueueHandle = new HTuple();
            hv_BVQueueHandle = new HTuple();
            ShowRightBtnFunc = new ShowRightButtonDelegate(ShowRightButton);
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
                PenetrationRateChartValues = new ChartValues<ObservablePoint>{
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


            _curStickData = new StickData();
            _preShowRadius = new ArrayList();
            _preShowResisvity = new ArrayList();
            _preShowPenetrationRate = new ArrayList();
            _preShowYingLi = new ArrayList();

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
            _timeRefresh.Start();

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
            

            _timeStatusFormMonitorRefresh = new System.Windows.Forms.Timer
            {
                Interval = 5000,
            };

         

            _timeStatusFormMonitorRefresh.Tick += timeStatusFormMonitorRefresh_Tick; ;
            _timeStatusFormMonitorRefresh.Start();

        }

        private void _timeInitMoveController_Tick(object sender, EventArgs e)
        {
            CMoveController.Instance().CurPage = this;

            Task.Run(() =>
            {
                if (SettingParameter.Instance().NDaemon != 0)
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
        }

        private void timeStatusFormMonitorRefresh_Tick(object sender, EventArgs e)
        {
            if (_statusForm.Visible == true)
            {
                if ((_curStickData.RadiusNumInfo.Count > 0 ) &&  ((_preIndexOfRadius == 0 || _preIndexOfRadius >= _curStickData.RadiusNumInfo.Count) && (_preIndexOfYingLi == 0 || _preIndexOfYingLi >= _curStickData.YingliNumInfo.Count) && (_preIndexOfResisvity == 0 || _preIndexOfResisvity >= _curStickData.ResisvityNumInfo.Count) && (_preIndexOfPenetrationRate == 0 || _preIndexOfPenetrationRate >= _curStickData.PenetrationRateNumInfo.Count)))
                {
                    _statusForm.Visible = false;
                    _btnSet.Enabled = true;

                    if ( _curStickData.RadiusNumInfo.Count > 0)
                    {
                        InitRadiusResultSeries();
                        _cartesianRadiusChart.AxisX[0].MinValue = 0;
                        _cartesianRadiusChart.AxisX[0].MaxValue = RadiusChartValues.Count + 10;
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
                    ptValues.Add(new ObservablePoint(_preIndexOfRadius, (float)_curStickData.RadiusNumInfo[_preIndexOfRadius]));
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
                _cartesianRadiusChart.AxisY[0].MaxValue = fMaxValue;

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
                    ptValues.Add(new ObservablePoint(_preIndexOfYingLi, (float)_curStickData.YingliNumInfo[_preIndexOfYingLi]));
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
                    ptValues.Add(new ObservablePoint(_preIndexOfResisvity, (float)_curStickData.ResisvityNumInfo[_preIndexOfResisvity]));
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

        private void timePenetrationRateRefresh_Tick(object sender, System.EventArgs e)
        {
            if (_preIndexOfPenetrationRate > 0)
            {
                float fMinValue = _preMinValueOfPenetrationRate;
                float fMaxValue = _preMaxValueOfPenetrationRate;
                int nShowIndex = 0;
                ChartValues<ObservablePoint> ptValues = new ChartValues<ObservablePoint>();

                for (; _preIndexOfPenetrationRate < _curStickData.PenetrationRateNumInfo.Count; _preIndexOfPenetrationRate++)
                {
                    ptValues.Add(new ObservablePoint(_preIndexOfPenetrationRate, (float)_curStickData.PenetrationRateNumInfo[_preIndexOfPenetrationRate]));
                    nShowIndex++;
                    if ((float)_curStickData.PenetrationRateNumInfo[_preIndexOfPenetrationRate] < fMinValue)
                    {
                        fMinValue = (float)_curStickData.PenetrationRateNumInfo[_preIndexOfPenetrationRate];
                    }

                    if ((float)_curStickData.PenetrationRateNumInfo[_preIndexOfPenetrationRate] > fMaxValue)
                    {
                        fMaxValue = (float)_curStickData.PenetrationRateNumInfo[_preIndexOfPenetrationRate];
                    }
                    if (nShowIndex >= _showCountOfPoints)
                    {
                        break;
                    }
                }

                PenetrationRateChartValues.AddRange(ptValues);
                _preMinValueOfPenetrationRate = fMinValue;
                _preMaxValueOfPenetrationRate = fMaxValue;

                if (_preIndexOfPenetrationRate >= _curStickData.PenetrationRateNumInfo.Count)
                {
                    _preIndexOfPenetrationRate = 0;
                    _timePenetrationRateRefresh.Stop();
                }
                _cartesianPenetrationRateResultView.AxisY[0].MinValue = fMinValue;
                _cartesianPenetrationRateResultView.AxisY[0].MaxValue = fMaxValue;
                _cartesianPenetrationRateResultView.Update();
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

               
                for (int i = 0; i < subNormalRaidus.Count; i++)
                {
                    _cartesianRadiusChart.AxisX[0].Sections.Add(_axisRadiusAreassection[i]);
                    _cartesianRadiusChart.AxisX[0].Sections.Add(_axisRadiusBeginsection[i]);
                    _cartesianRadiusChart.AxisX[0].Sections.Add(_axisRadiusEndssection[i]);
                }

                subNormalRaidus.Clear();
                
                _tabPageRadius.Refresh();
                _labelMinRadius.Text = "最小直径：" + _curStickData.FMaxRadius.ToString("0.00");
                _labelMaxRadius.Text = "最大直径：" + _curStickData.FMinRadius.ToString("0.00");
                _labelLength.Text = "长度: " + _curStickData.FLength.ToString("0.00");
                _labelValidLength.Text = "有效长度: " + _curStickData.FValidLength.ToString("0.00");
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

        private void _timeRefresh_Tick(object sender, System.EventArgs e)
        {
            if (_strGetStickLineNum != "")
            {
                _curStickData = new StickData();

                if (true == GlobalDataCache.Instance().GetData(_strGetStickLineNum, ref _curStickData))
                {
                    try
                    {
                        _preShowRadius.Clear();
                        _preShowResisvity.Clear();
                        _preShowPenetrationRate.Clear();
                        _preShowYingLi.Clear();

                        LogHelper.Info("Silicon", "MainResultLineInfoPage GetInfo _strGetStickLineNum " + _strGetStickLineNum);
                        _strGetStickLineNum = "";

                        float fMinValue = 65535;
                        float fMaxValue = -1;
                        #region 直径数据结果更新
                        _preIndexOfRadius = 0;

                        ChartValues<ObservablePoint> ptChartValues = new ChartValues<ObservablePoint>();

                        foreach (var item in _curStickData.RadiusNumInfo)
                        {
                            ptChartValues.Add(new ObservablePoint(_preIndexOfRadius, (float)item));
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
                        _cartesianRadiusChart.AxisY[0].MaxValue = fMaxValue;

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
                        _preIndexOfYingLi = 0;

                        ptChartValues.Clear();

                        foreach (var item in _curStickData.YingliNumInfo)
                        {
                            ptChartValues.Add(new ObservablePoint(_preIndexOfYingLi, (float)item));
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
                            ptChartValues.Add(new ObservablePoint(_preIndexOfResisvity, (float)item));
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
                        #endregion
                        Bitmap bmpLJ_1 = new Bitmap(_curStickData.StrResultPicPath);
                        pictureBoxResult.Image = bmpLJ_1;
                        pictureBoxResult.PreImage = bmpLJ_1;


                        _preMinValueOfResisvity = fMinValue;
                        _preMaxValueOfResisvity = fMaxValue;
                        _cartesianResisvityChart.AxisY[0].MinValue = fMinValue;
                        _cartesianResisvityChart.AxisY[0].MaxValue = fMaxValue;

                       

                        fMinValue = 65535;
                        fMaxValue = -1;
                        #region 透过率数据结果更新
                        _preIndexOfPenetrationRate = 0;
                        ptChartValues.Clear();
                        foreach (var item in _curStickData.PenetrationRateNumInfo)
                        {
                            ptChartValues.Add(new ObservablePoint(_preIndexOfPenetrationRate, (float)item));

                            //PenetrationRateChartValues.Add(new ObservablePoint(_preIndexOfPenetrationRate, (float)item));
                            _preIndexOfPenetrationRate++;
                            if ((float)item < fMinValue)
                            {
                                fMinValue = (float)item;
                            }

                            if ((float)item > fMaxValue)
                            {
                                fMaxValue = (float)item;
                            }
                            if (_preIndexOfPenetrationRate >= _showCountOfPoints)
                            {
                                break;
                            }
                        }
                        PenetrationRateChartValues.AddRange(ptChartValues);
                        _preMinValueOfPenetrationRate = fMinValue;
                        _preMaxValueOfPenetrationRate = fMaxValue;
                        _cartesianPenetrationRateResultView.AxisY[0].MinValue = fMinValue;
                        _cartesianPenetrationRateResultView.AxisY[0].MaxValue = fMaxValue;

                        #endregion

                        _timeRadiusRefresh.Start();
                        _timePenetrationRateRefresh.Start();
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
        private HTuple hv_LQueueHandle;
        private HTuple hv_BVQueueHandle;
        private HTuple hv_ReceiveQueueHandle;

        private void ThreadScanSiliconStickMotionCardOnceFunc()
        {
            HTuple hv_MessageHandle = new HTuple();
            HTuple hv_Data = new HTuple(), hv_Start = new HTuple();
            HTuple hv_Sublevel = new HTuple(), hv_ZB = new HTuple();
            HObject cbho_Imageconst;
            LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin");

            LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Init Move Position Information");
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
            Thread.Sleep(1000);
            LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Find SiliconLine End ");
            CMoveController.Instance().GoOn(false);

            ProcessManager.Instance().FindSiliconLineNew();
           
            

            LogHelper.Info("Silicon", "ThreadScanSiliconStickPLCFunc Begin to Scan Radius ");
            CMoveController.Instance().GotoOrigin();

            Thread.Sleep(500);
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
            Thread measurethread = new Thread(() =>
            {
                SSZNCameraTools.Instance().Measure_3DNew(out cbho_Imageconst, hv_LQueueHandle, hv_ResultDictHandle);
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
            if (hv_Coordinate.Length <= 0)
            {
                CMoveController.Instance().GotoOrigin();
                return;
            }
            //modbus_write_float (Index, 128, HXZB[0]+offse)
            //modbus_write_bit (Index, 128, 1)
            //wait_arrive (Index, 128, IO_Result_Handle)
            //wait_seconds (3)
            //modbus_write_bit (Index, 232, 1) // 启用模板22
            //modbus_write_bit (Index, 230, 1) // 开始激光打标
            //wait_seconds (3)
            //wait_arrive (Index, 230, IO_Result_Handle)
            /*
            for (hv_Index1 = 0; (int)hv_Index1 <= (int)((new HTuple(hv_Coordinate.TupleLength()
                )) - 1); hv_Index1 = (int)hv_Index1 + 1)
            {
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    MoveControllerModbusTool.Instance().WriteTwoRegister(128, (hv_Coordinate.TupleSelect(((new HTuple(hv_Coordinate.TupleLength()
                        )) - 1) - hv_Index1)) + hv_offse);
                 
                }
                bool bState = false;
                MoveControllerModbusTool.Instance().WriteSingleCoil( 128, true);
                do
                {
                    MoveControllerModbusTool.Instance().ReadSingleCoil(128, ref bState);
                    if (bState == false)
                    {
                        break;
                    }
                    Thread.Sleep(500);
                } while (true);
                //wait_arrive(hv_Index, 128, hv_IO_Result_Handle);
                HOperatorSet.WaitSeconds(3);
                MoveControllerModbusTool.Instance().WriteSingleCoil( 232, true);
                MoveControllerModbusTool.Instance().WriteSingleCoil( 230, true);
                HOperatorSet.WaitSeconds(3);
                //wait_arrive(hv_Index, 230, hv_IO_Result_Handle);
                do
                {
                    MoveControllerModbusTool.Instance().ReadSingleCoil(230, ref bState);
                    if (bState == false)
                    {
                        break;
                    }
                    Thread.Sleep(500);
                } while (true);
            }

            */
            //******画线完成，回原点*********
            CMoveController.Instance().GotoOrigin();
            //MoveControllerModbusTool.Instance().WriteTwoRegister(70, 0);
            //MoveControllerModbusTool.Instance().WriteSingleCoil(120, true);
            //MoveControllerModbusTool.Instance().WriteSingleCoil(176, true);
            //MoveControllerModbusTool.Instance().WriteSingleCoil(175, false);
            //CMoveController.Instance().TurnOffStick();
            bool bRefState = false;
            //do
            //{
            //    MoveControllerModbusTool.Instance().ReadSingleCoil(22, ref bRefState);
            //    if (bRefState == false)
            //    {
            //        break;
            //    }
            //    Thread.Sleep(500);
            //} while (true);

            //do
            //{
            //    MoveControllerModbusTool.Instance().ReadSingleCoil(120, ref bRefState);
            //    if (bRefState == false)
            //    {
            //        break;
            //    }
            //    Thread.Sleep(500);
            //} while (true);
          
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
                Thread measurethread = new Thread(() =>
                {
                    SSZNCameraTools.Instance().Measure_3DNew(out cbho_Imageconst, hv_LQueueHandle, hv_ResultDictHandle);
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
                    if (SettingParameter.Instance().NDaemon != 0)
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
                    if (SettingParameter.Instance().NDaemon != 0)
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
            if (SettingParameter.Instance().NDaemon != 0)
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

            if (_cartesianPenetrationRateResultView.AxisX[0].Sections.Count > 0)
            {
                for (int i = 0; i < _cartesianPenetrationRateResultView.AxisX[0].Sections.Count - 1; i++)
                {
                    _cartesianPenetrationRateResultView.AxisX[0].Sections[i].Value = -1;

                }
            }

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
            PenetrationRateChartValues.Clear();
            _btnSet.Enabled = false;
            _strGetStickLineNum = _siliconStickNumTextbox.Text;
            //RedisTool.Instance().RedisPub("JingBian", _siliconStickNumTextbox.Text);

            HOperatorSet.SetDictTuple(hv_ResultDictHandle, "晶编",  new HTuple(_siliconStickNumTextbox.Text));
            string strSiliconNum = _siliconStickNumTextbox.Text;
            if (strSiliconNum[2] == 'P'|| strSiliconNum[2] == 'p')
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
            else
            {
                m_threadScanStick = new Thread(ThreadScanSiliconStickMockFunc);
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