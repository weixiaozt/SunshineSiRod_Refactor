using HalconDotNet;
using SquareSiliconStickCheck.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.MonthCalendar;
using OpenCvSharp;
using System.Drawing;
using System.Windows.Forms;
using SquareSiliconStickCheck.Pages;
using Sunny.UI;
using SiliconRoundBarCheck.Cameras;
//using SquareSiliconStickCheck.Cameras.XG;
using SquareSiliconStickCheck.Data;

namespace SquareSiliconStickCheck.Tools
{
    public class ProcessManager
    {
        private static ProcessManager _instance;

        private Mat[] _preFullLJMat = new Mat[4];
        private Mat[] _preFullLJMatTmp = new Mat[4];

        private Mat[] _preFullYinLieMat = new Mat[4];
        private Mat[] _preFullYinLieMatTmp = new Mat[4];
        private Mat[] _preFullYingLiMat = new Mat[4];
        private Mat[] _preFullYingLiMatTmp = new Mat[4];

        private Thread m_threadRefreshViewYinLie;
        private Thread m_threadRefreshViewYingLi;
        private Thread m_threadTrigger;
        private Thread m_findSiliconThread;
        

        private ProcessManager() 
        {
            for (int i = 0; i < 4; i++)
            {
                _preFullYingLiMat[i] = new Mat();
                _preFullYingLiMatTmp[i] = new Mat();
                _preFullYinLieMat[i] = new Mat();
                _preFullYinLieMatTmp[i] = new Mat();
                _preFullLJMat[i] = new Mat();
                _preFullLJMatTmp[i] = new Mat(); 

            }
        }
        public static ProcessManager Instance()
        {
            if (_instance == null)
            {
                _instance = new ProcessManager();
            }

            return _instance;
        }
        public void Init()
        {
          
             LogHelper.Info("Silicon", "Init");
        }
       


        public void InitMatResource()
        {
            
        }



       
       
        private int _nScanIndex = 0;
        private object _lockScanIndex = new object();
        public int NScanIndex
        {
            get 
            {
                int nValue = 0;
                lock(_lockScanIndex)
                {
                    nValue = _nScanIndex;
                }
                return nValue; 
            }
            set 
            {
                lock(_lockScanIndex)
                {
                    _nScanIndex = value;
                }
                
            }
        }

        public void ScanSquareSiliconStickRoundMock(string strSerialNum, int nSquareType = 0)
        {
            try
            {
                LogHelper.Info("Silicon", "ScanSquareSiliconStickRoundMock Begin");
                if (SettingParameter.Instance().NCamType == 0)
                {
                    SSZNCamTools.Instance().NLeftIndex = 0;
                    SSZNCamTools.Instance().NRightIndex = 0;
                    SSZNCamTools.Instance().NTopIndex = 0;
                    SSZNCamTools.Instance().NDownIndex = 0;
                    SSZNCamTools.Instance().NMockLeftIndex = 0;
                    SSZNCamTools.Instance().NMockRightIndex = 0;
                    SSZNCamTools.Instance().NMockTopIndex = 0;
                    SSZNCamTools.Instance().NMockDownIndex = 0;
                    //SSZNCamTools.Instance().setBatchLoopMock();
                    SSZNCamTools.Instance().SoftTrig();
                    //SSZNCamTools.Instance().SnapData();
                    SSZNCamTools.Instance().Measure_SquareStick(strSerialNum, nSquareType);

                }
                else
                {

                }


                LogHelper.Info("Silicon", "ScanSquareSiliconStickRoundMock End");
            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon", " ScanSquareSiliconStickRoundMock exception " + ex.Message);
            }
        }

        public void ScanSquareSiliconStickCollectionMockTread(float fStandLTRDLength, float fStandRTLDLength, float fStandTDLength, float fStandLRLength, int nSquareType  )
        {
            try
            {
                LogHelper.Info("Silicon", "ScanSquareSiliconStickCollectionMockTread Begin");

                if (SettingParameter.Instance().NCamType == 0)
                {
                    SSZNCamTools.Instance().NLeftIndex = 0;
                    SSZNCamTools.Instance().NRightIndex = 0;
                    SSZNCamTools.Instance().NTopIndex = 0;
                    SSZNCamTools.Instance().NDownIndex = 0;
                    SSZNCamTools.Instance().NMockLeftIndex = 0;
                    SSZNCamTools.Instance().NMockRightIndex = 0;
                    SSZNCamTools.Instance().NMockTopIndex = 0;
                    SSZNCamTools.Instance().NMockDownIndex = 0;
                    SSZNCamTools.Instance().setBatchLoopMock();
                    SSZNCamTools.Instance().Measure_CollectionSquareStick(fStandLTRDLength, fStandRTLDLength, fStandTDLength, fStandLRLength, nSquareType);

                }
                else
                {
                    

                }

            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon", " ScanSquareSiliconStickCollectionTread exception " + ex.Message);
            }

        }

        public void ScanSquareSiliconStickCollectionTread(float fStandLTRDLength, float fStandRTLDLength, float fStandTDLength, float fStandLRLength, int nSquareType )
        {
            try
            {
                LogHelper.Info("Silicon", "ScanSquareSiliconStickCollectionTread Begin");

                CMoveController.Instance().SetMoveSpeed(32);
                CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, 120);
                Thread.Sleep(3000);
                SSZNCamTools.Instance().Bstop = false;
                Thread _scratchThread = new Thread(() =>
                {
                    if (SettingParameter.Instance().NCamType == 0)
                    {
                        SSZNCamTools.Instance().NLeftIndex = 0;
                        SSZNCamTools.Instance().NRightIndex = 0;
                        SSZNCamTools.Instance().NTopIndex = 0;
                        SSZNCamTools.Instance().NDownIndex = 0;

                        SSZNCamTools.Instance().NGrayLeftIndex = 0;
                        SSZNCamTools.Instance().NGrayRightIndex = 0;
                        SSZNCamTools.Instance().NGrayTopIndex = 0;
                        SSZNCamTools.Instance().NGrayDownIndex = 0;

                        SSZNCamTools.Instance().NMockLeftIndex = 0;
                        SSZNCamTools.Instance().NMockRightIndex = 0;
                        SSZNCamTools.Instance().NMockDownIndex = 0;
                        SSZNCamTools.Instance().NMockTopIndex = 0;

                        SSZNCamTools.Instance().BatchLoop();
                        SSZNCamTools.Instance().Measure_CollectionSquareStick(fStandLTRDLength, fStandRTLDLength, fStandTDLength, fStandLRLength, nSquareType);
                    }
                    else
                    {
                        
                    }
                   

                });


                _scratchThread.Start();
                Thread.Sleep(1000);
                CMoveController.Instance().SetMoveSpeed(32);
                CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_thr, 120);
                //SSZNCamTools.Instance().StopTrig();
                SSZNCamTools.Instance().Bstop = true;
                while (_scratchThread.ThreadState != ThreadState.Stopped)
                {
                    Thread.Sleep(1000);

                }
              
                Thread.Sleep(2000);
                LogHelper.Info("Silicon", "ScanSquareSiliconStickCollectionTread End");
            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon", " ScanSquareSiliconStickCollectionTread exception " + ex.Message);
            }

        }



        public void ScanSquareSiliconStickRound(string strSerialNum, int nSquareType = 0,int Mum=0)
        {
      
            int ser = 0;
            int Mnum = 0;
            while (true)
            {
                bool bStatus1 = false;
                bool bStatus2 = false;
                //strSerialNum = "";
                CMoveControllerModbusTool.Instance().WriteSingleCoil(6424, false);
                while (true)
                {
                    Thread.Sleep(500);
                    CMoveControllerModbusTool.Instance().ReadSingleCoil(6425, ref bStatus1);
                    //CMoveControllerModbusTool.Instance().ReadSingleCoil(2200, ref bStatus2);
                    if (bStatus1 == true)
                    {
                        GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate = "收到开始信号" ;
                        #region 解析棒子信息
                        for (int i = 0; i < 10; i++)
                        {
                            CMoveControllerModbusTool.Instance().ReadSingleRegister(5510 + i, ref ser);
                            if (ser == 0)
                            {
                                continue;
                            }
                            int CrcL = ser % 256;                //低8位，取余
                            int CRCL = ser & (0XFF);             //取低8位,152

                            int CrcH = ser / 256;                //高8位，除256
                            int CRLH = (ser >> 8) & 0XFF;//取高8位,99
                            char c = (char)CRCL;
                            strSerialNum = strSerialNum + c;
                            if (CRLH == 0)
                            {
                                continue;
                            }
                            c = (char)CRLH;
                            strSerialNum = strSerialNum + c;
                        }
                        strSerialNum = GlobalDatastate.Instance().sernum;
                        
                        GlobalDatastate.Instance().sernum = strSerialNum;
                        CMoveControllerModbusTool.Instance().ReadTwoRegister(5502, out float LENGTH);//棒长
                        CMoveControllerModbusTool.Instance().ReadSingleRegister(2100, ref ser);//规格
                        CMoveControllerModbusTool.Instance().ReadSingleRegister(5504, ref Mnum);//机台号
                        //ser = 1;
                        #endregion
                        break;
                    }
                }
                GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "收到晶编："+ strSerialNum;
                GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "规格：" + ser+"    "+"机台号："+ Mnum;
                try
            {
                LogHelper.Info("Silicon","ScanFirstQuarterRound Begin");




                Thread _scratchThread = new Thread(() =>
                {
                    if (SettingParameter.Instance().NCamType == 0)
                    {
                        SSZNCamTools.Instance().NLeftIndex = 0;
                        SSZNCamTools.Instance().NRightIndex = 0;
                        SSZNCamTools.Instance().NTopIndex = 0;
                        SSZNCamTools.Instance().NDownIndex = 0;

                        SSZNCamTools.Instance().NGrayLeftIndex = 0;
                        SSZNCamTools.Instance().NGrayRightIndex = 0;
                        SSZNCamTools.Instance().NGrayTopIndex = 0;
                        SSZNCamTools.Instance().NGrayDownIndex = 0;

                        SSZNCamTools.Instance().NMockLeftIndex = 0;
                        SSZNCamTools.Instance().NMockRightIndex = 0;
                        SSZNCamTools.Instance().NMockDownIndex = 0;
                        SSZNCamTools.Instance().NMockTopIndex = 0;
                      
                        //SSZNCamTools.Instance().BatchLoop();
                        SSZNCamTools.Instance().SoftTrig();
                        CMoveControllerModbusTool.Instance().WriteSingleCoil(6424, true);//给PLC发Ready信号
                        GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "发送Ready信号完成";
                        //while (true)
                        //{
                        //    bool Readysatus = false;
                        //    CMoveControllerModbusTool.Instance().ReadSingleCoil(6424, ref Readysatus);
                        //    if (Readysatus)
                        //    {
                        //        GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "PLC收到Ready信号";
                        //        break;
                        //    }
                        //}
                        nSquareType = ser;
                        Mum = Mnum;
                        //strSerialNum = "123";
                        GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "相机开始扫描";
                        SSZNCamTools.Instance().Measure_SquareStick(strSerialNum, nSquareType,Mum);


                    }
                    else
                    {
                    }
                });


                _scratchThread.Start(); 
                Thread.Sleep(1000);


               
                //CMoveController.Instance().SetMoveSpeed(30);
                //CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_sec, SettingParameter.Instance().FPosition_sec_s, 1040);
                while (true)
                {
                    Thread.Sleep(1000);
                    //CMoveControllerModbusTool.Instance().ReadSingleCoil(2201, ref bStatus1);
                    CMoveControllerModbusTool.Instance().ReadSingleCoil(6426, ref bStatus2);
                    if (bStatus2 == true)
                    {
                        break;
                    }                   
                }
                CMoveControllerModbusTool.Instance().WriteSingleCoil(6424, false);//给PLC发end信号
                    GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "扫描完成,清零";
                    //SSZNCamTools.Instance().StopTrig();
                    if (SettingParameter.Instance().NCamType == 0)
                {
                    //SSZNCamTools.Instance().StopBatchLoop();
                    //SSZNCamTools.Instance().StopTrig();
                    }
                else
                {
                   // XGCamTools.Instance.StopCaptures();
                }
                SSZNCamTools.Instance().Bstop = true;
                //CPointLaserTools.Instance().SaveDatas();
                
                while (_scratchThread.ThreadState != ThreadState.Stopped)
                {
                    Thread.Sleep(2000);

                }

                //CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_sec, SettingParameter.Instance().FPosition_sec_s, 3000);

                //Thread.Sleep(5000);
                //SSZNCamTools.Instance().StopTrig();
                //CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, 120);
                Thread.Sleep(1000);
                 LogHelper.Info("Silicon","ScanFirstQuarterRound End");
            }
            catch(Exception ex)
            {
                 LogHelper.Info("Silicon"," ScanFirstQuarterRound exception " + ex.Message);
            }
            }
        }

        public void ClearThreads()
        {
             LogHelper.Info("Silicon","ClearThreads begin");
            try
            {
               
                
               
            }
            catch (Exception ex)
            {
                 LogHelper.Info("Silicon","ClearThreads exception " + ex.Message);
            }

             LogHelper.Info("Silicon","ClearThreads end");


        }

    }
}
