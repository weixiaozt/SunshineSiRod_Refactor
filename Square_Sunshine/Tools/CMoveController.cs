using HalconDotNet;
using OpenCvSharp.Flann;
using SquareSiliconStickCheck.Pages;
using SquareSiliconStickCheck.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SquareSiliconStickCheck.FormMain;

namespace SquareSiliconStickCheck.Tools
{
    public enum emMoveType
    {
        EM_PLCTYPE = 0,
        EM_MOTIONCARD = 1
    };
    internal class CMoveController
    {

        private static CMoveController _instance;
        private float _fPosition_First = 1;   //机器原点附近位置
        private float _fPosition_Second = 5400;  //棒子尾部扫描信息    带尖的部位为棒子头部
        private float _fPosition_Third = 0;    //棒子头部扫描位置信息
        private float _fPosition_Fourth = 4414;  //棒子晶线扫描位置
        private float _fPosition_Fifth = 0;
        private float _fSpeed = 200;
        private emMoveType _moveType;

      
        public float FPosition_First { get => _fPosition_First; set => _fPosition_First = value; }
        public float FPosition_Second { get => _fPosition_Second; set => _fPosition_Second = value; }
        public float FPosition_Third { get => _fPosition_Third; set => _fPosition_Third = value; }
        public float FPosition_Fourth { get => _fPosition_Fourth; set => _fPosition_Fourth = value; }
        public float FPosition_Fifth { get => _fPosition_Fifth; set => _fPosition_Fifth = value; }
        public emMoveType MoveType { get => _moveType; set => _moveType = value; }

        public static CMoveController Instance()
        {
            if(_instance == null)
            {
                _instance = new CMoveController();
            }

            return _instance;
        }
        public static UInt16[] FloatToTwoUInt16(float f)
        {
            byte[] bs = BitConverter.GetBytes(f);

            UInt16[] uInfo = new UInt16[2];

            UInt16 low = BitConverter.ToUInt16(bs, 0);
            UInt16 high = BitConverter.ToUInt16(bs, 2);



            return new UInt16[] { high, low };

        }

        public void Init()
        {
            if (MoveType == emMoveType.EM_PLCTYPE)
            {
                if (false == CMoveControllerModbusTool.Instance().ConnectServer())
                {
                    FormMain.formMainF.showMessageDelegate("连接运动控制模块失败,请检查网络！", (int)emMSGTYPE.EM_MODBUSDISCONNECT);

                }

                //if (false == CArmControllerModbusTool.Instance().ConnectServer())
                //{
                //    FormMain.formMainF.showMessageDelegate("连接手臂运动控制模块失败,请检查网络！", (int)emMSGTYPE.EM_ARMMODBUSDISCONNECT);

                //}

            }
            else
            {
                CMotionCardController.Instance().Init();
            }
        }

        public void InitPositionInfo()
        {
            
            if (MoveType == emMoveType.EM_PLCTYPE)
            {
                _fPosition_First = SettingParameter.Instance().FPosition_fir;   //0 机器原点附近位置
                _fPosition_Second = SettingParameter.Instance().FPosition_sec; //35000 棒子尾部扫描信息    带尖的部位为棒子头部
                _fPosition_Third = SettingParameter.Instance().FPosition_thr;    // 0 棒子头部扫描位置信息
               
                _fSpeed = SettingParameter.Instance().FPLCMoveSpeed;

                //CMoveControllerModbusTool.Instance().WriteTwoRegister(70, SettingParameter.Instance().FPosition_fir);
                //CMoveControllerModbusTool.Instance().WriteTwoRegister(72, SettingParameter.Instance().FPosition_sec);
                //CMoveControllerModbusTool.Instance().WriteTwoRegister(124, SettingParameter.Instance().FPosition_thr);
                CMoveControllerModbusTool.Instance().WriteTwoRegister(1000, 880);
                CMoveControllerModbusTool.Instance().WriteTwoRegister(1006, _fSpeed);

                LogHelper.Info("", "_fSpeed " + _fSpeed.ToString("0.00"));

            }
            else if (MoveType == emMoveType.EM_MOTIONCARD)
            {
                FPosition_First = SettingParameter.Instance().NMotionCardBeginPosition;
                _fPosition_Second = SettingParameter.Instance().NMotionCardTailPosition;
               
            }
        }

        public void ResetTailPosition()
        {
            if (MoveType == emMoveType.EM_PLCTYPE)
            {
                _fPosition_Second = SettingParameter.Instance().FPosition_sec; //35000 棒子尾部扫描信息    带尖的部位为棒子头部
                //CMoveControllerModbusTool.Instance().WriteTwoRegister(72, _fPosition_Second);
                CMoveControllerModbusTool.Instance().WriteTwoRegister(2122, _fPosition_Second);
              
            }
            else if (MoveType == emMoveType.EM_MOTIONCARD)
            {
                _fPosition_Second = SettingParameter.Instance().NMotionCardTailPosition;
               
            }
        }
        public bool ConnectServer()
        {
            if (_moveType == emMoveType.EM_PLCTYPE)
            {
                if (false == CMoveControllerModbusTool.Instance().ConnectServer())
                {
                    return false;
                }

            }
            //            GoToBeginPosition();
            return true;
            //MesureRadiusStart();
            //IsMesureRadiusBeginEnded();
            //GoToBeginPosition();
            //Rotate(true);
            //Thread.Sleep(10000);
            //Rotate(false);

        }

        private CMoveController() 
        {
            _moveType = emMoveType.EM_PLCTYPE;
        }

        /*
         * 计算直径
         */
        public bool GoToTailPosition()
        {
             LogHelper.Info("Silicon","GoToTailPosition Begin Stick Tail " + SettingParameter.Instance().NStickLength.ToString("0.00"));
            CMoveControllerModbusTool.Instance().WriteSingleCoil(122, false); //位置1  晶棒尾部
            CMoveControllerModbusTool.Instance().WriteTwoRegister(122, SettingParameter.Instance().NStickLength);
            CMoveControllerModbusTool.Instance().WriteSingleCoil(122, true);
            bool bStatus = false;
            while (true)
            {
                Thread.Sleep(2000);
                CMoveControllerModbusTool.Instance().ReadSingleCoil(122, ref bStatus);
               
                if (bStatus == false)
                {
                    return true;
                }
                else
                {
                     LogHelper.Info("Silicon","GoToTailPosition Waited For End Position");
                }
            }
            
        }

        public void GetMoveCurPosition(out float fCurPosition)
        {
            if (emMoveType.EM_MOTIONCARD == _moveType)
            {
                double dbPosition = 0;
                CMotionCardController.Instance().GetCurAxisPostion(CMotionCardController.emAxisName.emThreeD, out dbPosition);
                fCurPosition = (float)dbPosition;
            }
            else if (emMoveType.EM_PLCTYPE == _moveType)
            {
                float fRegisterInfo = 0;
                GetValue(SettingParameter.Instance().NCurPositionRegister, out fRegisterInfo);
                fCurPosition = fRegisterInfo;
            }
            else
            {
                fCurPosition = 0;
            }
        }
       
        public void GetState(int nStartIndex, out bool bState)
        {
            bool bValue = false;
            CMoveControllerModbusTool.Instance().ReadSingleCoil(nStartIndex, ref bValue);
            bState = bValue;
        }

        public void GetValue(int nStartIndex, out float fRegisterInfo)
        {
            float fValue = 0;

            CMoveControllerModbusTool.Instance().ReadTwoRegister(nStartIndex, out fValue);
            fRegisterInfo = fValue;

        }

        public void GotoDestPositonNewby(float fDesValue)
        {
            CMoveControllerModbusTool.Instance().WriteTwoRegister(128, fDesValue);

            CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);

            bool bState = false;
            do
            {
                CMoveControllerModbusTool.Instance().ReadSingleCoil(128, ref bState);

                if (false == bState)
                {
                    break;
                }
                Thread.Sleep(500);


            } while (true);
        }


        public void GotoDestPositonNew(float fDestValue )
        {
            bool bStatus = true;
            float fDestination = fDestValue * (float)0.91;
             LogHelper.Info("Silicon", "GotoDestPosition Begin nStartIndex " + " fDestValue " + fDestValue.ToString("0.00") + " fDestination " + fDestination.ToString("0.00"));

            CMotionCardController.Instance().GotoDesition((int)fDestination, SettingParameter.Instance().FMoveSpeed, SettingParameter.Instance().FMoveIncreaseSpeed);

            
            return;
        }

        public void GotoBegin()
        {
            CMotionCardController.Instance().GotoDesition((int)_fPosition_Third, SettingParameter.Instance().FMoveSpeed, SettingParameter.Instance().FMoveIncreaseSpeed);
            //CMotionCardController.Instance().ThreeD_Scan((int)CMotionCardController.emAxisName.emThreeD, 500, (int)_fPosition_Third, 50000, "MeasureStart", 0);
        }

        public void TurnOffStick()
        {
            //if (MoveType == emMoveType.EM_PLCTYPE)
            //{
            //    CMoveControllerModbusTool.Instance().WriteSingleCoil(175, false);
            //    CMoveControllerModbusTool.Instance().WriteSingleCoil(176, true);
            //}
            Thread.Sleep(2000);
        }
        public void TurnOnStick()
        {
            //代码暂定
            //modbus_write_bit(Index, 175, 1)
            if (MoveType == emMoveType.EM_PLCTYPE)
            {
                CMoveControllerModbusTool.Instance().WriteSingleCoil(176, false);
                CMoveControllerModbusTool.Instance().WriteSingleCoil(175, true);
                //modbus_write_bit(Index, 176, 0)
            }
            else if (MoveType == emMoveType.EM_MOTIONCARD)
            {
                //CMotionCardController.Instance().RotateSiliconStick(10,true);
            }
            Thread.Sleep(5000);

        }


        public void GotoOrigin()
        {
             LogHelper.Info("MVSLog", "GotoOrigin Begin");
            if (MoveType == emMoveType.EM_MOTIONCARD)
            {
                CMotionCardController.Instance().GotoDesition((int)_fPosition_First, SettingParameter.Instance().FMoveSpeed, SettingParameter.Instance().FMoveIncreaseSpeed);
            }
            else if (MoveType == emMoveType.EM_PLCTYPE)
            {
                GotoDestinationAndWait(120);
            }
          
             LogHelper.Info("MVSLog", "GotoOrigin End");
            //CMotionCardController.Instance().ThreeD_Scan((int)CMotionCardController.emAxisName.emThreeD, 500, (int)_fPosition_First, 50000, "MeasureStart", 0);
        }

        public void GoOn(bool bState)
        {
            if (MoveType == emMoveType.EM_PLCTYPE)
            {
                if (true == bState)
                {
                    CMoveControllerModbusTool.Instance().WriteSingleCoil(1002, false); //手动模式
                    CMoveControllerModbusTool.Instance().WriteSingleCoil(103, true);
                }
                else
                {
                    CMoveControllerModbusTool.Instance().WriteSingleCoil(103, false);
                    CMoveControllerModbusTool.Instance().WriteSingleCoil(1002, true); //手动模式
                }
            }
        }

        public void GotoSiliconLinePosition()
        {
             LogHelper.Info("MVSLog", "GotoSiliconLinePosition Begin _fPosition_Fourth " + _fPosition_Fourth.ToString("0.0"));
            if (MoveType == emMoveType.EM_MOTIONCARD)
            {
                CMotionCardController.Instance().GotoDesition((int)_fPosition_Fourth, SettingParameter.Instance().FMoveSpeed, SettingParameter.Instance().FMoveIncreaseSpeed);
            }
            else if (MoveType == emMoveType.EM_PLCTYPE)
            {
                GotoDestinationAndWait(126);
            }
            LogHelper.Info("MVSLog", "GotoSiliconLinePosition End");
            
        }

        public void SetSiliconStickTailPosition(int nF, HTuple hv_L, out HTuple hv_w, int nSubLength)
        {
            if (MoveType == emMoveType.EM_PLCTYPE)
            {
                switch (nF)
                {
                    case 0:
                        {
                            //CMoveControllerModbusTool.Instance().WriteTwoRegister(72, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            CMoveControllerModbusTool.Instance().WriteTwoRegister(2122, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            hv_w = 1;
                            break;
                        }
                    case 1:
                        {
                            //CMoveControllerModbusTool.Instance().WriteTwoRegister(72, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            CMoveControllerModbusTool.Instance().WriteTwoRegister(2122, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            hv_w = 2;
                            break;
                        }
                    case 2:
                        {
                            //CMoveControllerModbusTool.Instance().WriteTwoRegister(72, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            CMoveControllerModbusTool.Instance().WriteTwoRegister(2122, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            hv_w = 3;
                            break;
                        }
                    case 3:
                        {
                            //CMoveControllerModbusTool.Instance().WriteTwoRegister(72, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            CMoveControllerModbusTool.Instance().WriteTwoRegister(2122, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            hv_w = 4;
                            break;
                        }
                    case 4:
                        {
                            //CMoveControllerModbusTool.Instance().WriteTwoRegister(72, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            CMoveControllerModbusTool.Instance().WriteTwoRegister(2122, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            hv_w = 5;
                            break;
                        }
                    case 5:
                        {
                            //CMoveControllerModbusTool.Instance().WriteTwoRegister(72, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            CMoveControllerModbusTool.Instance().WriteTwoRegister(2122, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            hv_w = 6;
                            break;
                        }
                    case 6:
                        {
                            //CMoveControllerModbusTool.Instance().WriteTwoRegister(72, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            CMoveControllerModbusTool.Instance().WriteTwoRegister(2122, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            hv_w = 7;
                            break;
                        }
                    case 7:
                        {
                            //CMoveControllerModbusTool.Instance().WriteTwoRegister(72, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            CMoveControllerModbusTool.Instance().WriteTwoRegister(2122, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            hv_w = 8;
                            break;
                        }
                    case 8:
                        {
                            //CMoveControllerModbusTool.Instance().WriteTwoRegister(72, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            CMoveControllerModbusTool.Instance().WriteTwoRegister(2122, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            hv_w = 9;
                            break;
                        }
                    case 9:
                        {
                            //CMoveControllerModbusTool.Instance().WriteTwoRegister(72, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            CMoveControllerModbusTool.Instance().WriteTwoRegister(2122, hv_L.TupleInt() + (nF + 1) * nSubLength - 150);
                            hv_w = 10;
                            break;
                        }

                    default:
                        {
                            hv_w = 1;
                            break;
                        }
                }
            }
            else
            {
                //重新设置尾部坐标
                _fPosition_Second = hv_L.TupleInt() + (nF + 1) * nSubLength - 150;

                switch (nF)
                {
                    case 0:
                        {

                            hv_w = 1;
                            break;
                        }
                    case 1:
                        {
                             hv_w = 2;
                            break;
                        }
                    case 2:
                        {
                            hv_w = 3;
                            break;
                        }
                    case 3:
                        {
                            hv_w = 4;
                            break;
                        }
                    case 4:
                        {
                           hv_w = 5;
                            break;
                        }
                    case 5:
                        {
                             hv_w = 6;
                            break;
                        }
                    case 6:
                        {
                            hv_w = 7;
                            break;
                        }
                    case 7:
                        {
                            hv_w = 8;
                            break;
                        }
                    case 8:
                        {
                            hv_w = 9;
                            break;
                        }
                    case 9:
                        {
                            hv_w = 10;
                            break;
                        }

                    default:
                        {
                            hv_w = 1;
                            break;
                        }
                }
                hv_w = nF + 1;
            }
        }

        public void SetMoveSpeed(float fMoveSpeed)
        {
            CMoveControllerModbusTool.Instance().WriteTwoRegister(1006, fMoveSpeed);
            
        }
        public void GotoTerninalPosition()
        {
             LogHelper.Info("MVSLog", "GotoTerninalPosition Begin _fPosition_Second " + _fPosition_Second.ToString("0.0"));

            if (MoveType == emMoveType.EM_MOTIONCARD)
            {
                CMotionCardController.Instance().GotoDesition((int)_fPosition_Second, SettingParameter.Instance().FMoveSpeed);
            }
            else
            {
                GotoDestinationAndWait(122);
            }

            
             LogHelper.Info("MVSLog", "GotoTerninalPosition End");
            //CMotionCardController.Instance().ThreeD_Scan((int)CMotionCardController.emAxisName.emThreeD, 500, (int)_fPosition_Second, 50000, "MeasureStart", 0);
        }

        public void GetRegisterStatus(int nStartIndex, out bool bStatus)
        {
            bool bReadStatus = false;

            bStatus = false;
            try
            {
                CMoveControllerModbusTool.Instance().ReadSingleCoil(nStartIndex, ref bReadStatus);
                bStatus = bReadStatus;
            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon", "GetRegisterStatus Error " + ex.Message);
            }

        }

        public void GotoDestPosition(float fDestValue, float fDestValue_s = -658, int nStartIndex = 20)
        {
            bool bStatus = true;
            float fDestination = fDestValue ;
             LogHelper.Info("Silicon","GotoDestPosition Begin nStartIndex " + nStartIndex.ToString() + " fDestValue " + fDestValue.ToString("0.00") + " fDestination " + fDestination.ToString("0.00"));
            //CMoveControllerModbusTool.Instance().WriteTwoRegister(1008, fDestValue);
            //CMoveControllerModbusTool.Instance().WriteTwoRegister(1108, fDestValue_s);

            //CMoveControllerModbusTool.Instance().WriteTwoRegister(5000, fDestValue);
            CMoveControllerModbusTool.Instance().WriteSingleCoil(nStartIndex, true);
            //CMoveControllerModbusTool.Instance().WriteTwoRegister(nStartIndex, fDestination); //位置2
          
            while (true)
            {
                Thread.Sleep(1000);
                CMoveControllerModbusTool.Instance().ReadSingleCoil(1000, ref bStatus);
                if (bStatus == true)
                {
                    break;
                }
                else
                {
                     LogHelper.Info("Silicon","GotoDestPosition Waited For Begin Position");
                }
            }

            return;
        }
 
        public void WaitStartSignal()
        {
            bool bStatus = false;
             LogHelper.Info("Silicon","WaitStartSignal Begin");
           
            while (true)
            {
                Thread.Sleep(1000);
                CMoveControllerModbusTool.Instance().ReadSingleCoil(1000, ref bStatus);
                if (bStatus == true)
                {
                    break;
                }
                else
                {
                     LogHelper.Info("Silicon","WaitStartSignal Continue......");
                }
            }

            return;
        }

        public void GotoDestinationAndWait(int nStartIndex)
        {
            CMoveControllerModbusTool.Instance().WriteSingleCoil(nStartIndex, true); //位置1

            bool bStatus = false;
            while (true)
            {
                Thread.Sleep(1000);

                if (true == CMoveControllerModbusTool.Instance().ReadSingleCoil(nStartIndex, ref bStatus) )
                {
                    if (false == bStatus)
                    {
                        break;
                    }
                }

                 LogHelper.Info("Silicon", "GotoDestinationAndWait Continue ");
            }
        }

        public void GoToBeginPosition()
        {
            bool bStatus = false;
             LogHelper.Info("Silicon","GoToBeginPosition Begin");
            CMoveControllerModbusTool.Instance().WriteSingleCoil(120, false); //位置1
            CMoveControllerModbusTool.Instance().WriteSingleRegister(120, 0);
            CMoveControllerModbusTool.Instance().WriteSingleRegister(121, 0);
            CMoveControllerModbusTool.Instance().WriteSingleCoil(120, true);
            while(true)
            {
                Thread.Sleep(1000);
                CMoveControllerModbusTool.Instance().ReadSingleCoil(120, ref bStatus);
                if (bStatus == false)
                {
                    break;
                }
                else
                {
                     LogHelper.Info("Silicon","GoToTailPosition Waited For Begin Position");
                }
            }

            return ;
        }

        public void LightNew(bool bRun = true)
        {
            if (_moveType == emMoveType.EM_MOTIONCARD)
            {
//                CMotionCardController.Instance().LedLight(bRun);
//                CMotionCardController.Instance().SWIRLight(bRun);
            }
            else if (_moveType == emMoveType.EM_PLCTYPE)
            {
                CMoveControllerModbusTool.Instance().WriteSingleCoil(177, bRun);
                CMoveControllerModbusTool.Instance().WriteSingleCoil(178, bRun);
            }
        }

        public void Light(bool bRun = true)
        {
            if (bRun)
            {
                CMoveControllerModbusTool.Instance().WriteSingleCoil(177, true);
            }
            else
            {
                CMoveControllerModbusTool.Instance().WriteSingleCoil(177, false);
            }
        }

        public void Rotate(bool bRun = true)
        {
            if ( true == bRun)
            {
                CMoveControllerModbusTool.Instance().WriteSingleCoil(212, true);
            }
            else
            {
                CMoveControllerModbusTool.Instance().WriteSingleCoil(212, false);
            }
        }

        //public void RotateSiliconStick(bool bRun = true)
        //{
        //    if (MoveType == emMoveType.EM_MOTIONCARD)
        //    {
        //        if (true == bRun)
        //        {
        //            CMotionCardController.Instance().RotateSiliconStick(SettingParameter.Instance().FRotateSpeed, bRun);
        //        }
        //        else
        //        {
        //            CMotionCardController.Instance().StopRotateSiliconStick();
        //        }
        //    }
        //    else if (MoveType == emMoveType.EM_PLCTYPE)
        //    {
        //        if (true == bRun)
        //        {
        //            CMoveControllerModbusTool.Instance().WriteSingleCoil(1002, false);
        //            CMoveControllerModbusTool.Instance().WriteSingleCoil(213, bRun);
        //        }
        //        else
        //        {
        //            CMoveControllerModbusTool.Instance().WriteSingleCoil(213, bRun);
        //            CMoveControllerModbusTool.Instance().WriteSingleCoil(1002, true);
        //        }


        //    }
            
        //}
       
    }
}
