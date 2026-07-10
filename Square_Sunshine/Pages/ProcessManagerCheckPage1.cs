using Google.Protobuf.WellKnownTypes;
using HalconDotNet;
using ICSharpCode.SharpZipLib.Zip;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using SiliconRoundBarCheck.Cameras;
using SiliconRoundBarCheck.Data;
using SquareSiliconStickCheck.Data;
using SquareSiliconStickCheck.Parameters;
using SquareSiliconStickCheck.Tools;
using Sunny.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Forms;

namespace SquareSiliconStickCheck.Pages
{
    public partial class ProcessManagerCheckPage : UIPage
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


        public StopThradDelegate stopThradFunc;

        public delegate void SwitchStartBtnState(bool bEnable);


        public SwitchStartBtnState stateFunc;

        private InspectResult _curresult = new InspectResult(); 

        private int _nSwidthIndex = 0;

        private int _nTestID;

        private object _continuestatelock = new object();

        private bool _bContinueState = true;

        private Thread m_threadScanSiliconStick = null;
        private Thread m_Run = null;
        private Thread m_threadRefreshVideoView = null;
        private Thread m_threadMonitor = null;
        private Thread m_threadTick = null;
        private Thread m_threadScanRadius = null;
        private Thread m_threadWaitStartedSignal = null;
        private Thread clear_text = null;
        

        private SquareStickCheckData _curStickData = null;
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

        private StickData _leftStickData;
        private StickData _rightStickData;
        private StickData _topStickData;
        private StickData _downStickData;




        public System.Windows.Forms.Timer _timeCheckDataRefresh;
        private System.Windows.Forms.Timer _timestateDataRefresh;
        private System.Windows.Forms.Timer _XT;
        private System.Windows.Forms.Timer _timeStart;
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
        private int _nSquareType = 0;
        private bool _bGotLeftSiliconData = false;
        private bool _bGotRightSiliconData = false;
        private bool _bGotTopSiliconData = false;
        private bool _bGotDownSiliconData = false;

        tagParaInfo tagInfo = new tagParaInfo(emSSZNCamType.EM_LEFT,"",0);
        private const int _showCountOfPoints = 300;

        public static ProcessManagerCheckPage instance;


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



        public ProcessManagerCheckPage()
        {
            instance = this;
            ShowRightBtnFunc = new ShowRightButtonDelegate(ShowRightButton);
            initDevFunc = new InitDevDelegate(InitDevFunction);
            triggerBVFunc = new TriggerBVDelegate(TriggerBVFunction);
           
            refreshFunc = new RefreshViewDele(RefreshViewFunc);
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
            GlobalDatastate.Instance().sernum = "";
            _dicSearialAndStickDataArray = new Dictionary<string, ArrayList>();

            CheckForIllegalCrossThreadCalls = false;
            if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2||SettingParameter.Instance().NDaemon == 3)
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

           

            _timeCheckDataRefresh = new System.Windows.Forms.Timer
            {
                Interval = 1000,
            };
            _timeCheckDataRefresh.Tick += _timeCheckDataRefresh_Tick;

            _timestateDataRefresh = new System.Windows.Forms.Timer
            {
                Interval = 100,
            };
            _timestateDataRefresh.Tick += _timestateDataRefresh_Tick;

            _timeStart = new System.Windows.Forms.Timer
            {
                Interval = 200,
            };
            _timeStart.Tick += _timeStart_Tick;

            if (SettingParameter.Instance().NSaveBmp == 0)
            {
                this.buttonSave.Text = "Not Save";
            }
            else
            {
                this.buttonSave.Text = "Save";

            }

        }

        private void _timestateDataRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                
             
              uiScrollingText1.Text = GlobalDatastate.Instance().stateUpdate;
            }
            catch (Exception ex)
            {

            }

        }
   
        private void _timeCheckDataRefresh_Tick(object sender, EventArgs e)
        {


            if (GlobalDatastate.Instance().timstart==true)
            {
                _strSiliconSerialNum = GlobalDatastate.Instance().sernum;
                try
                {
                    if (_strSiliconSerialNum != "" && _bGotLeftSiliconData == false)
                    {
                        _curStickData = new SquareStickCheckData();
                        LogHelper.Info("", "Get Data " + _strSiliconSerialNum);
                        if (true == GlobalDataCache.Instance().GetCheckData(_strSiliconSerialNum, ref _curStickData))
                        {
                            LogHelper.Info("", "Got Data " + _strSiliconSerialNum);
                            //_bGotLeftSiliconData = true;
                            textBoxJB.Text = _strSiliconSerialNum;
                            string strLengthInfo = "";


                            strLengthInfo = _curStickData.FLength.ToString("0.00");

                            labelLength.Text = "棒长: " + strLengthInfo;

                            string strLTLength = "";


                            foreach (var item in _curStickData.ListLTLength)
                            {

                                strLTLength += ((float)item).ToString("0.00") + ",";


                            }
                            labelLTLength.Text = "Edge_A：" + strLTLength;

                            string strRTLength = "";

                            
                            foreach (var item in _curStickData.ListRTLength)
                            {
                                strRTLength += ((float)item).ToString("0.00") + ",";
                            }
                            labelRTLength.Text = "Edge_B：" + strRTLength;



                            string strRDLength = "";


                            foreach (var item in _curStickData.ListRDLength)
                            {
                                strRDLength += ((float)item).ToString("0.00") + ",";
                            }

                            labelLDLength.Text = "Edge_C：" + strRDLength;

                            string strLDLength = "";

                            
                            foreach (var item in _curStickData.ListLDLength)
                            {
                                strLDLength += ((float)item).ToString("0.00") + ",";
                            }
                            labelRDLength.Text = "Edge_D：" + strLDLength;


                            string strTDLength = "";


                            foreach (var item in _curStickData.ListTDLength)
                            {
                                strTDLength += ((float)item).ToString("0.00") + ",";
                            }
                            labelTDLength.Text = "Diagonal length 1:：" + strTDLength;


                            string strLRLength = "";


                            foreach (var item in _curStickData.ListLRLength)
                            {
                                strLRLength += ((float)item).ToString("0.00") + ",";
                            }
                            labelLRLength.Text = "Diagonal length 2：" + strLRLength;


                            string strTDiagLength = "";


                            foreach (var item in _curStickData.ListTopDiagLength)
                            {
                                strTDiagLength += ((float)item).ToString("0.00") + ",";
                            }
                            labelTopDiag.Text = "Arc length_1：" + strTDiagLength;


                            string strTLDiagLength = "";



                            foreach (var item in _curStickData.ListTopLeftDiagLength)
                            {
                                strTLDiagLength += ((float)item).ToString("0.00") + ",";
                            }
                            labelTopLeftDiag.Text = "1_Projectio1：" + strTLDiagLength;

                            string strTRDiagLength = "";



                            foreach (var item in _curStickData.ListTopRightDiagLength)
                            {
                                strTRDiagLength += ((float)item).ToString("0.00") + ",";
                            }
                            labelTopRightDiag.Text = "1_Projectio2：：" + strTRDiagLength;

                            string strLDiagLength = "";



                            foreach (var item in _curStickData.ListLeftDiagLength)
                            {
                                strLDiagLength += ((float)item).ToString("0.00") + ",";
                            }
                            labelLeftDiag.Text = "Arc length_2：" + strLDiagLength;


                            string strLLDiagLength = "";



                            foreach (var item in _curStickData.ListLeftLeftDiagLength)
                            {
                                strLLDiagLength += ((float)item).ToString("0.00") + ",";
                            }
                            labelLeftLeftDiag.Text = "Arc length_2：" + strLLDiagLength;

                            string strLRDiagLength = "";


                            foreach (var item in _curStickData.ListLeftRightDiagLength)
                            {
                                strLRDiagLength += ((float)item).ToString("0.00") + ",";
                            }
                            labelLeftRightDiag.Text = "2_Projectio2：" + strLRDiagLength;

                            string strRDiagLength = "";


                            foreach (var item in _curStickData.ListRightDiagLength)
                            {
                                strRDiagLength += ((float)item).ToString("0.00") + ",";
                            }
                            labelRightDiag.Text = "Arc length_3：" + strRDiagLength;


                            string strRLDiagLength = "";
                            foreach (var item in _curStickData.ListRightLeftDiagLength)
                            {
                                strRLDiagLength += ((float)item).ToString("0.00") + ",";
                            }
                            labelRightLeftDiag.Text = "3_Projectio1：" + strRLDiagLength;

                            string strRRDiagLength = "";
                            foreach (var item in _curStickData.ListRightRightDiagLength)
                            {
                                strRRDiagLength += ((float)item).ToString("0.00") + ",";
                            }
                            labelRightRightDiag.Text = "3_Projectio2：" + strRRDiagLength;

                            string strDDiagLength = "";


                            foreach (var item in _curStickData.ListDownDiagLength)
                            {
                                strDDiagLength += ((float)item).ToString("0.00") + ",";
                            }
                            labelDownDiag.Text = "Arc length_4：" + strDDiagLength;


                            string strDLDiagLength = "";

                            foreach (var item in _curStickData.ListDownLeftDiagLength)
                            {
                                strDLDiagLength += ((float)item).ToString("0.00") + ",";
                            }
                            labelDownLeftDiag.Text = "4_Projectio1：" + strDLDiagLength;

                            string strDRDiagLength = "";


                            foreach (var item in _curStickData.ListDownRightDiagLength)
                            {
                                strDRDiagLength += ((float)item).ToString("0.00") + ",";
                            }
                            labelDownRightDiag.Text = "4_Projectio2：" + strDRDiagLength;

                            bool bValid = false;

                            string strTopAngle = "";

                            foreach (var item in _curStickData.ListTopAngle)
                            {
                                strTopAngle += ((float)item).ToString("0.000") + ",";
                                if ((float)item < 89.90)
                                {
                                   // bValid = true;
                                }
                            }

                            labelTopAngle.Text = "Side verticality_1：" + strTopAngle;


                            string strLeftAngle = "";

                            foreach (var item in _curStickData.ListLeftAngle)
                            {
                                strLeftAngle += ((float)item).ToString("0.000") + ",";
                                if ((float)item < 89.90)
                                {
                                   // bValid = true;
                                }
                            }

                            labelLeftAngle.Text = "Side verticality_2：" + strLeftAngle;

                            string strRightAngle = "";

                            foreach (var item in _curStickData.ListRightAngle)
                            {
                                strRightAngle += ((float)item).ToString("0.000") + ",";
                                if ((float)item < 89.90)
                                {
                                   // bValid = true;
                                }
                            }

                            labelRightAngle.Text = "Side verticality_3：" + strRightAngle;

                            string strDownAngle = "";

                            foreach (var item in _curStickData.ListDownAngle)
                            {
                                strDownAngle += ((float)item).ToString("0.000") + ",";
                                if ((float)item < 89.90)
                                {
                                   // bValid = true;
                                }
                            }

                            labelDownAngle.Text = "Side verticality_4：" + strDownAngle;


                            //if (bValid == true)
                            //{
                            //    _curStickData.NResult = "0";

                            //}
                            //else
                            //{
                            //    _curStickData.NResult = "1";
                            //}
                            string S_Ver = "";

                            S_Ver += ((float)_curStickData.SVer).ToString("0.00") + "";

                            label2.Text = "Face verticality_H：" + S_Ver;

                            string E_Ver = "";

                            E_Ver += ((float)_curStickData.EVer).ToString("0.00") + "";

                            label3.Text = "Face verticality_T：" + E_Ver;
                            int Ser = int.Parse(_curStickData.NSquareType.ToString());
                            textBox1.Text = Ser.ToString();                          
                            int Mnum= int.Parse(_curStickData.Mnum.ToString());
                            textBox1.Text = Ser.ToString();
                            textBox2.Text = Mnum.ToString();
                            //CMoveControllerModbusTool.Instance().WriteSingleRegister(7020, 1);//检测结果
                            GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "当前晶编：" + _strSiliconSerialNum;

                            GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "检测结果：" + _curStickData.NResult;
                            DateTime dateTime = DateTime.Now;
                            string strDateTime = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            CMySQLTool.Instance().InsertSquareStickCheckResultInfo(SettingParameter.Instance().StrMySQLDBName, "squarstickresult", _curStickData.StrJBSearial, strLTLength, strRTLength, strLDLength, strRDLength, strTDLength, strLRLength, strTDiagLength, strLDiagLength, strRDiagLength, strDDiagLength, strTLDiagLength, strTRDiagLength, strLLDiagLength, strLRDiagLength, strRLDiagLength, strRRDiagLength, strDLDiagLength, strDRDiagLength, strTopAngle, strLeftAngle, strRightAngle, strDownAngle, _curStickData.NResult.ToString(), _curStickData.FLength, strDateTime, S_Ver, E_Ver,Ser,Mnum);
                            //_timeCheckDataRefresh.Stop();
                            GlobalDatastate.Instance().timstart = false;

                        }


                    }
                    else
                    {
                        //_timeCheckDataRefresh.Stop();
                    }


                }
                catch (Exception ex)
                {
                    int t = 4;
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
                ProcessManager.Instance().ScanSquareSiliconStickRoundMock(_strSiliconSerialNum, _nSquareType);

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
                GlobalDatastate.Instance().sernum = textBoxJB.Text;
                _strSiliconSerialNum = textBoxJB.Text;
                ProcessManager.Instance().ScanSquareSiliconStickRound(_strSiliconSerialNum, _nSquareType);
               
            }
            catch (Exception ex)
            {
                 LogHelper.Info("Silicon","ThreadScanSiliconStick exception " + ex.Message);
            }


        }


        private void ExcuteMethodstop()
        {
            try
            {
                _timeCheckDataRefresh.Stop();
            }
            catch
            {

            }
            
        }
        private void ExcuteMethod()
        {
            try
            {
                textBoxJB.Text = _strSiliconSerialNum;
                _timeCheckDataRefresh.Start();
            }
            catch
            {

            }
            
        }
        #region  文山运动逻辑
        private void CheckArmState()
        {
            bool bState = false;
            bool bNewState = false;
            bool bHaveSquareStick = true;
            CMoveController.Instance().SetMoveSpeed(SettingParameter.Instance().FMoveSpeed);
            CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, 120);
            int nCheckPCResult = 0;
            int nWaitCount = 0;
            bool bHasScratedSquareStick = false;
            //允许从产品线抓棒子并放棒
            CMoveControllerModbusTool.Instance().WriteSingleCoil(130, false);
            //不允许检测平台检测
            //CMoveControllerModbusTool.Instance().WriteSingleCoil(132, false);
            //不允许从检测平台抓棒子
            CMoveControllerModbusTool.Instance().WriteSingleCoil(134, false);
            while (true)
            {
                CMoveControllerModbusTool.Instance().ReadSingleCoil(138, ref bHaveSquareStick);

                if (false == bHaveSquareStick)
                {
                    break;
                }
                else
                {
                    LogHelper.Info("", "Have Square Silicon Stick,call arm to  scratch silicon stick !");

                    //允许从检测平台抓棒子
                    CMoveControllerModbusTool.Instance().WriteSingleCoil(134, true);
                    bNewState = false;

                    do
                    {
                        CMoveControllerModbusTool.Instance().ReadSingleCoil(140, ref bNewState);
                        //true 手臂正在从平台上抓棒子
                        if (bNewState == true)
                        {
                            break;
                        }

                        LogHelper.Info("", "Waite For arm scratching silicon stick !");
                        Thread.Sleep(1500);
                    } while (true);
                }
                LogHelper.Info("", "Check Platform has squarestick!");
                Thread.Sleep(2000);
            }




            while (true)
            {
                bState = false;
                CMoveControllerModbusTool.Instance().WriteSingleRegister(4000, 0);
                do
                {
                    CMoveControllerModbusTool.Instance().ReadSingleCoil(140, ref bNewState);
                    //true 手臂正在从平台上抓棒子
                    if (bNewState == true)
                    {
                        break;
                    }

                    LogHelper.Info("", "Waite For arm ready  !");
                    Thread.Sleep(1500);
                } while (true);
                //允许从产品线抓棒子并放棒
                CMoveControllerModbusTool.Instance().WriteSingleCoil(130, true);
                //不允许检测平台检测
                //CMoveControllerModbusTool.Instance().WriteSingleCoil(132, false);
                //不允许从检测平台抓棒子
                CMoveControllerModbusTool.Instance().WriteSingleCoil(134, false);
                //140 机械手准备好，可以上料
               
                while (true)
                {
                    bState = false;
                    CMoveControllerModbusTool.Instance().ReadSingleCoil(132, ref bState);
                    if (bState)
                    {
                        bHasScratedSquareStick = false;
                        //不允许从产品线抓棒子并放棒
                        CMoveControllerModbusTool.Instance().WriteSingleCoil(130, false);

                        //不允许从检测平台抓棒子
                        CMoveControllerModbusTool.Instance().WriteSingleCoil(134, false);

                        //不允许检测
                        //CMoveControllerModbusTool.Instance().WriteSingleCoil(132, false);

                        

                        int[] nJingBianValues = new int[8];
                        byte[] bJingBianValues = new byte[16];
                        int j = 0;
                        for (int i = 0; i < nJingBianValues.Length; i++)
                        {
                            CMoveControllerModbusTool.Instance().ReadSingleRegister(3028 + i, ref nJingBianValues[i]);
                            byte[] btValue = BitConverter.GetBytes(nJingBianValues[i]);
                            bJingBianValues[j] = btValue[0];
                            bJingBianValues[j + 1] = btValue[1];
                            j += 2;
                        }

                        int nType = 0;
                        CMoveControllerModbusTool.Instance().ReadSingleRegister(3044, ref nType);
                        _nSquareType = nType;

                        LogHelper.Info("", "Squarestick type " + nType.ToString());
                        int nArmPosition = 10000;

                        do
                        {
                            nArmPosition = 10000;
                            CMoveControllerModbusTool.Instance().ReadSingleRegister(3048, ref nArmPosition);
                            LogHelper.Info("", "Arm Position " + nArmPosition.ToString());
                            if (nArmPosition < 10)
                            {
                                break;
                            }
                           
                            Thread.Sleep(1000);
                        } while (true);
                        _strSiliconSerialNum = "";
                        for (int i = 0; i < bJingBianValues.Length; i++)
                        {
                            _strSiliconSerialNum += Convert.ToChar(bJingBianValues[i]);
                        }

                        if (bJingBianValues[0] != 0)
                        {
                            _bGotLeftSiliconData = false;
                            //_strSiliconSerialNum = textBoxJB.Text;
                            if (_strSiliconSerialNum == "")
                            {
                                Random random = new Random();
                                _strSiliconSerialNum = "fff" + random.Next(26) + 'A';
                            }
                            //【01】创建委托、绑定委托
                            Action actions = new Action(ExcuteMethodstop);
                            //【02】调用委托
                            this.Invoke(actions);

                            //【01】创建委托、绑定委托
                            Action action = new Action(ExcuteMethod);
                            //【02】调用委托
                            this.Invoke(action);
                            //try
                            //{
                            //    m_threadScanSiliconStick.Abort();
                            //}
                            //catch (Exception e)
                            //{

                            //}

                            if (SettingParameter.Instance().NDaemon == 1)
                            {
                                m_threadScanSiliconStick = new Thread(ThreadScanSiliconSquareStickFunc);
                            }
                            else
                            {
                                m_threadScanSiliconStick = new Thread(ThreadScanSiliconSquareStickMockFunc);
                            }
                            m_threadScanSiliconStick.Start();

                            Thread.Sleep(5000);

                            while (m_threadScanSiliconStick.ThreadState != ThreadState.Stopped)
                            {
                                Thread.Sleep(1000);
                                LogHelper.Info("", "Waite For m_threadScanSiliconStick run stopped!");
                                if (SSZNCamTools.Instance().NTestStatus == SSZNCamTools.emStatus.EM_FREE)
                                {
                                    LogHelper.Info("", "Square Silicon device check ended!");

                                    Thread threadMovOrigin = new Thread(() =>
                                    {
                                        LogHelper.Info("Silicon", "CheckArmState   Move To Orgin Position");
                                        CMoveController.Instance().SetMoveSpeed(150);
                                        CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, 120);
                                        //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                                    });
                                    threadMovOrigin.Start();

                                    if (threadMovOrigin != null)
                                    {
                                        while (threadMovOrigin.ThreadState != ThreadState.Stopped)
                                        {
                                            Thread.Sleep(500);
                                        }

                                    }

                                    bHaveSquareStick = false;
                                    while (true)
                                    {
                                        //判断是否有棒子
                                        CMoveControllerModbusTool.Instance().ReadSingleCoil(138, ref bHaveSquareStick);

                                        if (true == bHaveSquareStick)
                                        {
                                            break;
                                        }
                                        LogHelper.Info("", "Check Platform has not squarestick!");
                                        Thread.Sleep(500);

                                    }
                                    LogHelper.Info("", "Tell PC squarestick check end!");
                                    CMoveControllerModbusTool.Instance().WriteSingleRegister(4000, 1);
                                    for (int i = 0; i < nJingBianValues.Length; i++)
                                    {
                                        CMoveControllerModbusTool.Instance().WriteSingleRegister(4001 + i, nJingBianValues[i]);
                                    }

                                    //允许从检测平台抓棒子
                                    CMoveControllerModbusTool.Instance().WriteSingleCoil(134, true);
                                    bNewState = false;

                                    do
                                    {
                                        CMoveControllerModbusTool.Instance().ReadSingleCoil(140, ref bNewState);
                                        //true 手臂正在从平台上抓棒子
                                        if (bNewState == true)
                                        {
                                            break;
                                        }

                                        LogHelper.Info("", "Waite For arm scratching silicon stick !");
                                        Thread.Sleep(2000);
                                    } while (true);

                                    bHasScratedSquareStick = true;
                                }
                            }
                            Thread.Sleep(1000);

                        }
                        

                        //false 手臂正在从平台上抓棒子
                        //CMoveControllerModbusTool.Instance().WriteSingleCoil(136, false);

                        
                        Thread threadMovEnd = new Thread(() =>
                        {
                            LogHelper.Info("Silicon", "CheckArmState   Move To Orgin Position");
                            CMoveController.Instance().SetMoveSpeed(150);
                            CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, 120);
                            //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                        });
                        threadMovEnd.Start();

                        if (threadMovEnd != null)
                        {
                            while (threadMovEnd.ThreadState != ThreadState.Stopped)
                            {
                                Thread.Sleep(500);
                            }

                        }
                       
                        //本次测试没有取走棒子，需要取棒子
                        if (false == bHasScratedSquareStick)
                        { 
                            //判断是否有棒子
                            bHaveSquareStick = false;
                            while (true)
                            {

                                CMoveControllerModbusTool.Instance().ReadSingleCoil(138, ref bHaveSquareStick);

                                if (true == bHaveSquareStick)
                                {
                                    break;
                                }
                                LogHelper.Info("", "Check Platform has not squarestick!");
                                Thread.Sleep(500);
                            }

                            LogHelper.Info("", "Tell PC squarestick check end!");
                            CMoveControllerModbusTool.Instance().WriteSingleRegister(4000, 1);
                            for (int i = 0; i < nJingBianValues.Length; i++)
                            {
                                CMoveControllerModbusTool.Instance().WriteSingleRegister(4001 + i, nJingBianValues[i]);
                            }

                            //允许从检测平台抓棒子
                            CMoveControllerModbusTool.Instance().WriteSingleCoil(134, true);
                            bNewState = false;

                            do
                            {
                                CMoveControllerModbusTool.Instance().ReadSingleCoil(140, ref bNewState);
                                //true 手臂正在从平台上抓棒子
                                if (bNewState == true)
                                {
                                    break;
                                }

                                LogHelper.Info("", "Waite For arm scratching silicon stick !");
                                Thread.Sleep(2000);
                            } while (true);

                        }

                        nWaitCount = 0;
                        do
                        {
                            CMoveControllerModbusTool.Instance().ReadSingleRegister(4500, ref nCheckPCResult);

                            LogHelper.Info("", "Wait For Checked PC result " + nCheckPCResult.ToString());
                            if (nCheckPCResult == 1)
                            {
                                CMoveControllerModbusTool.Instance().WriteSingleRegister(4000, 0);
                                break;
                            }
                            nWaitCount++;
                            if (nWaitCount > 3)
                            {
                                break;
                            }
                            Thread.Sleep(1000);
                        } while (true);


                        LogHelper.Info("", "Checked Silicon Stick result End " );
                        //此次测棒结束
                        break;
                    }
                    else
                    {
                        LogHelper.Info("", "Wait For Silicon square stick to check paltform!");
                    }
                    Thread.Sleep(1000);
                }

                // bState true 可以测试方棒
               

                Thread.Sleep(1000);
            }
        }
        #endregion
        private void buttonStop_Click_1(object sender, EventArgs e)
        {
            try


            {
              
                if (m_threadMonitor != null && m_threadMonitor.ThreadState != ThreadState.Stopped)
                {
                    m_threadMonitor.Abort();
                }

                if (m_threadTick != null && m_threadTick.ThreadState != ThreadState.Stopped)
                {
                    m_threadTick.Abort();
                }
            }
            catch 
            { 
            }

            
        }
        private void buttonStart_Click(object sender, System.EventArgs e)
        {
           
            if (SettingParameter.Instance().NDaemon == 1)
            {
                //m_threadTick = new Thread(TickClock);
                //m_threadTick.Start();

                m_threadMonitor = new Thread(CheckArmState);
                m_threadMonitor.Start();
                this.buttonStart.Enabled = false;

            }
            else if (SettingParameter.Instance().NDaemon == 3)
            {

               


            }
            else if (SettingParameter.Instance().NDaemon == 2)
            {                
                

                _bGotLeftSiliconData = false;
                //CMoveControllerModbusTool.Instance().WriteSingleCoil(2202, true);
                _timeCheckDataRefresh.Start();
                _timestateDataRefresh.Start();
               // _XT.Start();
                m_threadScanSiliconStick = new Thread(ThreadScanSiliconSquareStickFunc);
                m_threadScanSiliconStick.Start();
                this.buttonStart.Enabled = false;
            }

            else
            {
                ProcessManager.Instance().InitMatResource();

                _bGotLeftSiliconData = false;
                _strSiliconSerialNum = textBoxJB.Text;
                _nSquareType = SettingParameter.Instance().NSquareType;
                _timeCheckDataRefresh.Start();
                _timestateDataRefresh.Start();
                m_threadScanSiliconStick = new Thread(ThreadScanSiliconSquareStickMockFunc);
                GlobalDatastate.Instance().stateUpdate = "开始扫描...";
                m_threadScanSiliconStick.Start();
                clear_text = new Thread(Clear_Text);
                clear_text.Start();
              
            }


        }
        //根据日期删文件夹
        private void DeleteFile(string fileDirect, int saveDay)
        {
            DateTime nowTime = DateTime.Now;
            DirectoryInfo root = new DirectoryInfo(fileDirect);
            DirectoryInfo[] dics = root.GetDirectories();//获取文件夹

            FileAttributes attr = File.GetAttributes(fileDirect);
            if (attr == FileAttributes.Directory)//判断是不是文件夹
            {
                foreach (DirectoryInfo file in dics)//遍历文件夹
                {
                    TimeSpan t = nowTime - file.CreationTime;  //当前时间  减去 文件创建时间
                    int day = t.Days;
                    if (day > saveDay)   //保存的时间 ；  单位：天
                    {

                        Directory.Delete(file.FullName, true);  //删除超过时间的文件夹
                    }
                }
            }
        }
            private void pictureBoxYingLi_1_Click(object sender, EventArgs e)
        {
           
        }


        private void _timeStart_Tick(object sender, EventArgs e)
        {
            bool bStatus = false;
            CMoveControllerModbusTool.Instance().ReadSingleCoil(2205, ref bStatus);

            if (bStatus == true)
            {
                GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "收到开始信号";
               // Run();
            }
        }
        //private void Run()
        //{
        //    _timeStart.Stop();
        //    bool bStatus = true;
        //    SSZNCamTools.Instance().Bstop = false;
        //    CPointLaserTools.Instance().ClearDatas();
     
        //    int ser = new int();

        //    _nSquareType = SettingParameter.Instance().NSquareType;
        //    _timeCheckDataRefresh.Start();
        //    _timestateDataRefresh.Start();
        //    m_threadScanSiliconStick = new Thread(ThreadScanSiliconSquareStickFunc);
        //    m_threadScanSiliconStick.Start();
        //    GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "开始扫描...";
        //    clear_text = new Thread(Clear_Text);
        //    clear_text.Start();
        //    _strSiliconSerialNum = "";
        //        #region 解析棒子信息
        //        for (int i = 0; i < 10; i++)
        //    {
        //        CMoveControllerModbusTool.Instance().ReadSingleRegister(5510 + i, ref ser);
        //        if (ser == 0)
        //        {
        //            continue;
        //        }
        //        int CrcL = ser % 256;                //低8位，取余
        //        int CRCL = ser & (0XFF);             //取低8位,152

        //        int CrcH = ser / 256;                //高8位，除256
        //        int CRLH = (ser >> 8) & 0XFF;//取高8位,99
        //        char c = (char)CRCL;
        //        _strSiliconSerialNum = _strSiliconSerialNum + c;
        //        if (CRLH == 0)
        //        {
        //            continue;
        //        }
        //        c = (char)CRLH;
        //        _strSiliconSerialNum = _strSiliconSerialNum + c;
        //    }
        //        CMoveControllerModbusTool.Instance().ReadTwoRegister(5502, out float LENGTH);//棒长
        //        CMoveControllerModbusTool.Instance().ReadSingleRegister(5500, ref ser);//规格
        //        #endregion

        //        _bGotLeftSiliconData = false;
        //    //_strSiliconSerialNum = textBoxJB.Text;
        //    textBoxJB.Text = _strSiliconSerialNum;
        //        while (true)
        //        {
        //            Thread.Sleep(100);
        //            CMoveControllerModbusTool.Instance().ReadSingleCoil(2205, ref bStatus);

        //            if (bStatus == false)
        //            {
        //            _timeStart.Start();
        //                break;
        //            }

        //        }
           
        //}
        private void Clear_Text()
        {
        
          
                    LogHelper.Info("", "Clear........... " + _strSiliconSerialNum);

                        labelLength.Text = "棒长: ";

                        labelLTLength.Text = "A面边长：";

                        labelRTLength.Text = "B面边长：";

                        labelLDLength.Text = "C面边长：";
            
                        labelRDLength.Text = "D面边长：" ;

                        labelTDLength.Text = "上下对角线：";
                   
                        labelLRLength.Text = "左右对角线：";
                       
                        labelTopDiag.Text = "上侧弧长：";

                        labelTopLeftDiag.Text = "上侧弧左弧长投影：";
                                          
                        labelTopRightDiag.Text = "上侧弧右弧长投影：";
                      
                        labelLeftDiag.Text = "左侧弧长：";
                      
                        labelLeftLeftDiag.Text = "左侧弧左弧长投影：";
                        
                        labelLeftRightDiag.Text = "左侧弧右弧长投影：";
                      
                        labelRightDiag.Text = "右侧弧长：";
                       
                        labelRightLeftDiag.Text = "右侧弧左弧长投影：";

                        labelRightRightDiag.Text = "右侧弧右弧长投影：";
                        
                        labelDownDiag.Text = "下侧弧长：";
                       
                        labelDownLeftDiag.Text = "下侧弧左弧长投影：";
                      
                        labelDownRightDiag.Text = "下侧弧右弧长投影：";
                       
                        labelTopAngle.Text = "上侧直边角度：";
                      
                        labelLeftAngle.Text = "左侧直边角度：";
                     
                        labelRightAngle.Text = "右侧直边角度：";

                        labelDownAngle.Text = "下侧直边角度：" ;
       
        }

        private void ProcessManagerPage_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReleaseCameras();
            ProcessManager.Instance().ClearThreads();
        }

        private void buttonSave_Click_1(object sender, EventArgs e)
        {
            if (SettingParameter.Instance().NSaveBmp == 1)
            {
                SettingParameter.Instance().NSaveBmp = 0;
                this.buttonSave.Text = "Not Save";
            }
            else
            {
                SettingParameter.Instance().NSaveBmp = 1;
                this.buttonSave.Text = "Save";
            }
        }

        private void labelLength_Click(object sender, EventArgs e)
        {

        }

        private void uiButton2_Click(object sender, EventArgs e)
        {

        }
        HObject image1 = null;
        private void uiButton1_Click(object sender, EventArgs e)
        {
            _timeCheckDataRefresh.Start();
            _timestateDataRefresh.Start();
            var f = new OpenFileDialog();
            String filepath=null;
            String filename = null;
            //f.Multiselect = true; //多选
            if (f.ShowDialog() == DialogResult.OK)
            {
                 filepath = f.FileName;//G:\新建文件夹\新建文本文档.txt
                 filename = f.SafeFileName;//新建文本文档.txt             
            }
            try
            {
                HOperatorSet.ReadImage(out image1, filepath);
                GlobalDatastate.Instance().stateUpdate = "读取图片" + "_" + filename;
                string serial = filename;
                GlobalDatastate.Instance().sernum = serial;
                _strSiliconSerialNum = serial;
                Thread t = new Thread(new ThreadStart(run));
                t.Start();
            }
            catch (Exception)
            {
                GlobalDatastate.Instance().stateUpdate = "读取图片失败" + "_" + filename;

            }
           




        }
        private void run()

        {

            Square_Test(image1, _strSiliconSerialNum, 0);

        }


        private void Square_Test(HObject Image, string strSerial, int ser)
        {


            GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "截取完成";
            GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "开始计算...";
            HTuple ParmDict = new HTuple();
            HTuple Ex = new HTuple();
            HOperatorSet.CreateDict(out ParmDict);
            HTuple ResourceDictHandle=null;
            HDevelopExport.Instance().Detection_init(ParmDict, out  ResourceDictHandle, out HTuple JSONData, out Ex);

            HOperatorSet.SetDictTuple(ResourceDictHandle, "当前规格号", ser);
            HDevelopExport.Instance().Detection_process(Image, out HObject SaveImage, out HObject DispImage,
             1, ParmDict, ResourceDictHandle, out HTuple AllQuality, out HTuple OutParmDict, out Ex);
            GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "计算完成";


            #region 录入结果

            HTuple g = new HTuple();
            HTuple g1 = new HTuple();
            g = null;
            g1 = null;
            SquareStickCheckData data = new SquareStickCheckData();
            //GlobalDatastate.Instance().sernum = paraInfo.strSerial;
            //saveimage(Image, paraInfo.strSerial);
            //GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "存图完成";
            HOperatorSet.JsonToDict(AllQuality, g, g1, out HTuple AllQuality1);
            HOperatorSet.GetDictTuple(AllQuality1, "结果", out HTuple result);
            HOperatorSet.GetDictTuple(result, "B_边长", out HTuple HLT_Length);
            HOperatorSet.GetDictTuple(result, "A_边长", out HTuple HRT_Length);
            HOperatorSet.GetDictTuple(result, "棒长", out HTuple HLength);
            HOperatorSet.GetDictTuple(result, "对角1", out HTuple HDJ1);
            HOperatorSet.GetDictTuple(result, "对角1", out HTuple HDJ2);
            HOperatorSet.GetDictTuple(result, "弧长1", out HTuple HArc_Top);
            HOperatorSet.GetDictTuple(result, "弧长2", out HTuple HArc_Right);
            HOperatorSet.GetDictTuple(result, "弧长3", out HTuple HArc_Left);
            HOperatorSet.GetDictTuple(result, "弧长4", out HTuple HArc_Down);
            HOperatorSet.GetDictTuple(result, "上_垂直度", out HTuple HPer_Top);
            HOperatorSet.GetDictTuple(result, "右_垂直度", out HTuple HPer_Right);
            HOperatorSet.GetDictTuple(result, "左_垂直度", out HTuple HPer_Left);
            HOperatorSet.GetDictTuple(result, "下_垂直度", out HTuple HPer_Down);
            HOperatorSet.GetDictTuple(result, "头端面垂直度", out HTuple HS_Ver);
            HOperatorSet.GetDictTuple(result, "尾端面垂直度", out HTuple HE_Ver);


            HDevelopExport.Instance().HTupToFt(HLT_Length, out float LT_Length);
            HDevelopExport.Instance().HTupToFt(HRT_Length, out float RT_Length);
            HDevelopExport.Instance().HTupToFt(HLength, out float Length);
            HDevelopExport.Instance().HTupToFt(HDJ1, out float DJ1);
            HDevelopExport.Instance().HTupToFt(HDJ2, out float DJ2);
            HDevelopExport.Instance().HTupToFt(HArc_Top, out float Arc_Top);
            HDevelopExport.Instance().HTupToFt(HArc_Right, out float Arc_Right);
            HDevelopExport.Instance().HTupToFt(HArc_Left, out float Arc_Left);
            HDevelopExport.Instance().HTupToFt(HArc_Down, out float Arc_Down);
            HDevelopExport.Instance().HTupToFt(HPer_Top, out float Per_Top);
            HDevelopExport.Instance().HTupToFt(HPer_Right, out float Per_Right);
            HDevelopExport.Instance().HTupToFt(HPer_Left, out float Per_Left);
            HDevelopExport.Instance().HTupToFt(HPer_Down, out float Per_Down);
            HDevelopExport.Instance().HTupToFt(HS_Ver, out float S_Ver);
            HDevelopExport.Instance().HTupToFt(HE_Ver, out float E_Ver);

            float A_PL = 0;
            float B_PL = 0;
            float DJ1_PL = 0;
            float DJ2_PL = 0;
            try
            {
                HOperatorSet.GetDictTuple(ResourceDictHandle, "规格" + ser, out HTuple RSTP);
               
                HOperatorSet.GetDictTuple(RSTP, "A_边长_偏量修正", out HTuple HA_PL);
                HOperatorSet.GetDictTuple(RSTP, "B_边长_偏量修正", out HTuple HB_PL);
                HOperatorSet.GetDictTuple(RSTP, "对角线1_偏量修正", out HTuple HDJ1_PL);
                HOperatorSet.GetDictTuple(RSTP, "对角线2_偏量修正", out HTuple HDJ2_PL);
                HOperatorSet.TupleNumber(HA_PL, out HA_PL);
                HOperatorSet.TupleNumber(HB_PL, out HB_PL);
                HOperatorSet.TupleNumber(HDJ1_PL, out HDJ1_PL);
                HOperatorSet.TupleNumber(HDJ2_PL, out HDJ2_PL);
                HDevelopExport.Instance().HTupToFt(HA_PL, out A_PL);
                HDevelopExport.Instance().HTupToFt(HB_PL, out B_PL);
                HDevelopExport.Instance().HTupToFt(HDJ1_PL, out DJ1_PL);
                HDevelopExport.Instance().HTupToFt(HDJ2_PL, out DJ2_PL);

            }
            catch (Exception)
            {
                A_PL = 0;
                B_PL = 0;
                DJ1_PL = 0;
                DJ2_PL = 0;
            }
            LT_Length = LT_Length + A_PL;
            RT_Length = RT_Length + B_PL;
            DJ1 = DJ1 + DJ1_PL;
            DJ2 = DJ2 + DJ2_PL;
            try
            {
                data.ListLTLength.Clear();
                data.ListRTLength.Clear();
                data.ListLDLength.Clear();
                data.ListRDLength.Clear();

                data.ListTDLength.Clear();
                data.ListLRLength.Clear();
                data.ListLeftDiagLength.Clear();
                data.ListRightDiagLength.Clear();
                data.ListTopDiagLength.Clear();
                data.ListDownDiagLength.Clear();
                data.ListLeftLeftDiagLength.Clear();
                data.ListLeftRightDiagLength.Clear();
                data.ListRightLeftDiagLength.Clear();
                data.ListRightRightDiagLength.Clear();
                data.ListDownLeftDiagLength.Clear();
                data.ListDownRightDiagLength.Clear();
                data.ListTopLeftDiagLength.Clear();
                data.ListTopRightDiagLength.Clear();
                data.ListTopAngle.Clear();
                data.ListTopAngle.Clear();
                data.ListTopAngle.Clear();
                data.ListDownAngle.Clear();
                data.ListDownAngle.Clear();
                data.ListDownAngle.Clear();
                data.ListLeftAngle.Clear();
                data.ListLeftAngle.Clear();
                data.ListLeftAngle.Clear();
                data.ListRightAngle.Clear();
                data.ListRightAngle.Clear();
                data.ListRightAngle.Clear();

                float late = 0.707F;
                //float late1 = 0.707;

                data.ListLTLength.Add(LT_Length);
                data.ListRTLength.Add(RT_Length);
                data.ListLDLength.Add(RT_Length);
                data.ListRDLength.Add(LT_Length);
                data.FLength = Length;
                data.ListTDLength.Add(DJ1);
                data.ListLRLength.Add(DJ2);
                data.ListLeftDiagLength.Add(Arc_Left);
                data.ListRightDiagLength.Add(Arc_Right);
                data.ListTopDiagLength.Add(Arc_Top);
                data.ListDownDiagLength.Add(Arc_Down);
                data.ListLeftLeftDiagLength.Add(Arc_Left * 0.707F);
                data.ListLeftRightDiagLength.Add(Arc_Left * 0.707F);
                data.ListRightLeftDiagLength.Add(Arc_Right * 0.707F);
                data.ListRightRightDiagLength.Add(Arc_Right * 0.707F);
                data.ListDownRightDiagLength.Add(Arc_Down * 0.707F);
                data.ListDownLeftDiagLength.Add(Arc_Down * 0.707F);
                data.ListTopLeftDiagLength.Add(Arc_Top * 0.707F);
                data.ListTopRightDiagLength.Add(Arc_Top * 0.707);
                data.ListTopAngle.Add(Per_Top);
                data.ListTopAngle.Add(Per_Top);
                data.ListTopAngle.Add(Per_Top);


                data.ListDownAngle.Add(Per_Down);
                data.ListDownAngle.Add(Per_Down);
                data.ListDownAngle.Add(Per_Down);

                data.ListLeftAngle.Add(Per_Left);
                data.ListLeftAngle.Add(Per_Left);
                data.ListLeftAngle.Add(Per_Left);

                data.ListRightAngle.Add(Per_Right);
                data.ListRightAngle.Add(Per_Right);
                data.ListRightAngle.Add(Per_Right);
                data.NResult = "";
                data.SVer = S_Ver;
                data.EVer = E_Ver;
                data.StrJBSearial = strSerial;
            }
            catch (Exception)
            {


            }

            GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "存图完成";
            Generate_form.Instance().ExportToExcel(data);
            // GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "导出表格完成";
            GlobalDataCache.Instance().AddCheckData(strSerial, data);
            GlobalDatastate.Instance().timstart = true;
            //bCalculateStick = true;
            GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "导入数据库完成";

            /// if (SettingParameter.Instance().NDaemon == 0)
            // {

            // LogHelper.Info("", "ThreadDealSquareInfo  fCurRealLRLength " + fCurRealLRLength.ToString("0.00") + " fCurRealTDLength " + fCurRealTDLength.ToString("0.00"));

            HTuple strJsonFile;
            string strFileName = SettingParameter.Instance().StrSaveDir + "/" + strSerial + "/result.json";
            string strEndFileName = SettingParameter.Instance().StrSaveDir + "/" + strSerial + "/end.txt";
            // HOperatorSet.DictToJson(hv_ResultdictHandle, new HTuple(), new HTuple(), out strJsonFile);

            if (false == Directory.Exists(SettingParameter.Instance().StrSaveDir + "/" + strSerial))
            {
                Directory.CreateDirectory(SettingParameter.Instance().StrSaveDir + "/" + strSerial);
            }
            HTuple fileHandle = new HTuple();
            HTuple fileHandleend = new HTuple();
            HOperatorSet.OpenFile(strFileName, "output", out fileHandle);
            // HOperatorSet.FwriteString(fileHandle, strJsonFile);
            HOperatorSet.CloseFile(fileHandle);
            HOperatorSet.OpenFile(strEndFileName, "output", out fileHandleend);
            HOperatorSet.CloseFile(fileHandleend);

            #endregion

        }

        private void button1_Click(object sender, EventArgs e)
        {
            LogHelper.Info("123", "111111");
        }


        private void uiButton4_Click(object sender, EventArgs e)
        {
            CMoveControllerModbusTool.Instance().WriteSingleRegister(2100, 100);
        }

        private void textBoxJB_TextChanged(object sender, EventArgs e)
        {
            GlobalDatastate.Instance().sernum= textBoxJB.Text;
        }

        private void uiButton2_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_token))
                {
                    MessageBox.Show("请先获取Token！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

               string url = "http://suzhou.orbitmes.com:18734/api/OrbitWebAPI";

                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent("BrickVisionInspectionDataPilot"), "APIName");
                formData.Add(new StringContent(_token), "Token");

                string userDataJson = JsonSerializer.Serialize(new
                {
                    SerialNum = "T1N12H25090050A1910SL",
                    LTLength = 300,
                    RTLength = 22,
                    LDLength = 33,
                    RDLength = 4,
                    TDLength = 5
                });
                formData.Add(new StringContent(userDataJson), "UserData");

                var response = _httpClient.PostAsync(url, formData).GetAwaiter().GetResult();
                //var response = await _httpClient.PostAsync(url, formData);

                string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("\n\n数据发送成功！");
                }
                else
                {
                    //txtStatus.Text += $"\n\n错误: 请求失败，状态码 {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                //txtStatus.Text = $"发生异常:\n{ex.Message}\n\n堆栈跟踪:\n{ex.StackTrace}";
            }
            finally
            {
            }
        }

        private string _token;
        private HttpClient _httpClient;
        private void ProcessManagerCheckPage_Load(object sender, EventArgs e)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            string url = "http://suzhou.orbitmes.com:18734/api/OrbitWebAPILogin";
            url += "?APIName=BrickVisionInspectionDataPilot";
            url += "&UserName=NDK";
            url += "&Password=123456";

            // 同步调用 HttpClient
            var response = _httpClient.PostAsync(url, null).GetAwaiter().GetResult();

            // 同步获取响应内容
            string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                var jsonDoc = JsonDocument.Parse(responseContent);
                if (jsonDoc.RootElement.TryGetProperty("token", out JsonElement tokenElement))
                {
                    _token = tokenElement.GetString();
                }
            }
            else
            {
                MessageBox.Show($"\n\n错误: 请求失败，状态码 {response.StatusCode}");
            }
        }
    }
}