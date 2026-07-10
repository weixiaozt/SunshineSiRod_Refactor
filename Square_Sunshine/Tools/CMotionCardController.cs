

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SquareSiliconStickCheck.Pages;

namespace SquareSiliconStickCheck.Tools
{
    public class CMotionCardController
    {
        public enum emDI
        {
            EM_SAFEDOOR = 0
        };

        public enum emDO
        {
            EM_LEDLIGHT = 0, //海康白光光源
            EM_LEDSWIR = 1, //红外光源
            EM_LIFT = 2, //举重机
        };
        public enum emAxisName
        {
            emThreeD = 1, //运动
            emRoller = 2, //旋转
            emPowerZ = 3,  //光源侧上下
            emCameraZ = 4, //相机侧上下
            emPowerXY = 5, //光源测前后
            emCameraXY = 6, //相机测前后
            emLIFT = 7, //举重机
        };

        private Thread _safeThread;

        private Object _safeObject;

        private bool _bSafeStatus = true;

        private Object _rotateObject;

        private Thread _mointorRoateThread;

        private float _fSubLength = 0;  //相对零点距离 前后轴

        private float _fZeroPosition = 0;

        private bool _bIsSetZeror = false;

        private bool _bRotateInterupt = false;

        private static CMotionCardController _instance = null;

        struct stCmd
        {
            public short Axis;
            public int Val;
            public int Pos;
            public int V;
            public string Sign;
            public int H;
        };

        private stCmd _preCmd;

        private ProcessManagerPage _curPage = null;

        public ProcessManagerPage CurPage { get => _curPage; set => _curPage = value; }
        public float FZeroPosition { get => _fZeroPosition; set => _fZeroPosition = value; }
        public float FSubLength { get => _fSubLength; set => _fSubLength = value; }
        public bool BIsSetZeror { get => _bIsSetZeror; set => _bIsSetZeror = value; }



        public static CMotionCardController Instance()
        {
            if (_instance == null)
            {
                _instance = new CMotionCardController();
            }
            return _instance;
        }

        private CMotionCardController()
        {
            _safeObject = new Object();
            _rotateObject = new Object();
            _bIsSetZeror = false;
        }

        //更新相对零点的位置
        private void UpdateSubLength(float fValue)
        {
            _fSubLength = fValue;
            //LogHelper.Info("Silicon", "UpdateSubLength " + _fSubLength.ToString("0.0"));
        }

        //设置软零点 
        public void SetZeroPosNoLimit(int nAxis)
        {
            _fSubLength = 0;
            _fZeroPosition = 0;
            _bIsSetZeror = true;
            mc.GTN_ZeroPos(1, (short)nAxis, 1);
           
        }

        //设置软零点 
        public void SetZeroPos(int nAxis)
        {
            _fSubLength = 0;
            _fZeroPosition = 0;
            _bIsSetZeror = true;
            mc.GTN_ZeroPos(1, (short)nAxis, 1);
            mc.GTN_SetSoftLimitMode(1, (short)nAxis, 0);
            mc.GTN_SetSoftLimit(1, (short)nAxis, 1000, -650000000);
        }

        public void StopRotateSiliconStick(bool bNeedCloseMonitor = true)
        {
            try
            {
                mc.GTN_Stop(1, 1 << ((int)emAxisName.emRoller - 1), 0);
                //mc.GTN_AxisOff(1, (int)emAxisName.emRoller);

                if (true == bNeedCloseMonitor)
                {
                    if (_mointorRoateThread != null && _mointorRoateThread.ThreadState != ThreadState.Stopped)
                    {
                        _mointorRoateThread.Abort();
                        _mointorRoateThread = null;
                        lock (_rotateObject)
                        {
                            _bRotateInterupt = false;
                        }
                    }

                }

            }
            catch (Exception ex)
            {

            }
        }

        

        public void SWIRLight(bool bTurnOn = true)
        {
            TurnOnOrOffIOStatus(0, (int)emDO.EM_LEDSWIR, bTurnOn);
        }
        public void LedLight(bool bTurnOn = true)
        {
            TurnOnOrOffIOStatus(0, (int)emDO.EM_LEDLIGHT, bTurnOn);
        }

        private int GetDIOStatus(short extModuleNo = 0, short nDIIndex = 0)
        {
            byte bStatus = 0;

            glink.GT_GetGLinkDiBit(extModuleNo, nDIIndex, out bStatus);
            return bStatus;
        }

        public void TurnOnOrOffIOStatus(short extModuleNo = 0, short nDOIndex = 0, bool bOn = true)
        {
            byte btStatue = 0;
            if (bOn)
            {
                btStatue = 1;
            }
            else
            {
                btStatue = 0;
            }
            glink.GT_SetGLinkDoBit(extModuleNo, nDOIndex, btStatue);

        }

        //result : true   安全
        //       ：false  不安全
        public void TurnOnStick(bool bTurnOn = true)
        {
            TurnOnOrOffIOStatus(0, (short)emDO.EM_LIFT, bTurnOn);
        }

       
        //public void GotoDesition(int nPos, float nMoveSpeed, float fAccThread = (float)0.5)
        //{
        //    int nStatus = -1;
        //    bool bInterupted = false;
        //    do
        //    {
        //        if (false == bInterupted)
        //        {
        //            nStatus = ThreeD_Scan((int)CMotionCardController.emAxisName.emThreeD, (int)nMoveSpeed, nPos, fAccThread, "MeasureStart");

        //            if (nStatus == 1 || nStatus == -1)
        //            {
        //                break;
        //            }
        //            else if (nStatus == 0)
        //            {
        //                Thread.Sleep(1000);
                        
        //                {
                           
        //                    bInterupted = true;
        //                }
        //                 //LogHelper.Info("MVSLog", "Continue to GotoPosition!");
        //            }
        //        }
        //        else
        //        {
        //            if (CurPage.BContinueState == true)
        //            {
        //                bInterupted = false;
        //                //CurPage.BContinueState = false;
        //            }
        //            else
        //            {
        //                Thread.Sleep(500);
        //            }
        //        }
               


        //    } while (nStatus != 1);
            

        //}

        public bool GetCurAxisPostion(emAxisName nAxisNum, out  double dPosition)
        {
            int nsts = 0;
            uint pClock = 0;
            double pValue = 0;
           // mc.GTN_GetSts(1, (short)nAxisNum, out nsts, 1, out pClock);
            mc.GTN_ClrSts(1, (short)nAxisNum, 8);
            mc.GTN_GetSts(1, (short)nAxisNum, out nsts, 1, out pClock);
            mc.GTN_GetPrfPos(1, (short)nAxisNum, out pValue, 1, out pClock);
            
            dPosition = pValue;

            if (nAxisNum == emAxisName.emThreeD)
            {
                if (true == BIsSetZeror)
                {
                    //SettingParameter.Instance().FSubPosition = (float)dPosition;
                    //SettingParameter.Instance().SaveSubLength();
                }

            }
            return true;
           


        }

        //public int ThreeD_ScanZero(short sAxis, int nValSpeed, double nPreZeroPos, double fBeginPosition,float vAcc)
        //{
            
        //    if (false == _bSafeStatus)
        //    {
        //        //LogHelper.Info("MVSLog", "Safe Door Status!");
        //        return 0;
        //    }

        //    double dbDestPosition = 0/*(float)nPos * 14705.8824*/;
           
        //    double acc = 0;
        //    double dec = 0;
        //    double dbSmoothTime = 0;
        //    mc.TTrapPrm pfm = new mc.TTrapPrm();
        //    mc.GTN_PrfTrap(1, (short)sAxis);
        //    short sResult = mc.GTN_GetTrapPrm(1, (short)sAxis, out pfm);

        //    if (sResult != 0)
        //    {
        //        //LogHelper.Info("Erro", "GTN_GetTrapPrm Error sResult " + sResult.ToString());
        //        return -1;
        //    }

        //    acc = pfm.acc;
        //    dec = pfm.dec;
        //    dbSmoothTime = pfm.smoothTime;

        //    mc.TTrapPrm newpfm = new mc.TTrapPrm();

        //    newpfm.acc = vAcc;
        //    newpfm.dec = vAcc;
        //    newpfm.smoothTime = 100;

        //    uint pCLock = 0;
        //    double pValue = 0;
        //    int nsts = 0;
        //    float fPreSubLength = SettingParameter.Instance().FSubPosition;

        //    double dbCurPosition = 0;
        //    mc.GTN_SetTrapPrm(1, sAxis, ref newpfm);
        //    //mc.GTN_ZeroPos(1, sAxis, 1);
        //    mc.GTN_GetPrfPos(1, sAxis, out dbCurPosition, 1, out pCLock);
        //    mc.GTN_SetPos(1, sAxis, (int)nPreZeroPos);
        //    mc.GTN_SetVel(1, sAxis, nValSpeed);
        //    mc.GTN_Update(1, 1 << (sAxis - 1));

        //    double fSubLength = 0;

        //    while (true)
        //    {
        //        mc.GTN_ClrSts(1, sAxis, 8);
        //        mc.GTN_GetSts(1, sAxis, out nsts, 1, out pCLock);
        //        mc.GTN_GetPrfPos(1, sAxis, out pValue, 1, out pCLock);

        //        if (true == BIsSetZeror)
        //        {
        //            fSubLength = pValue - fBeginPosition;
        //            //SettingParameter.Instance().FSubPosition = fPreSubLength + (float)fSubLength;
        //            //SettingParameter.Instance().SaveSubLength();
        //            //LogHelper.Info("MVSLog", "Get Move Status FSubPosition !" + SettingParameter.Instance().FSubPosition.ToString("0.0") + " " + fSubLength.ToString("0.0") + " dbDestPosition " + dbDestPosition.ToString("0.0") + " Value " + pValue.ToString("0.0") + " BeginPosition " + fBeginPosition.ToString("0.0"));
        //        }
        //        //LogHelper.Info("MVSLog", "Get Move Status !" + pValue.ToString("0.00") + " " + dbDestPosition.ToString("0.00"));
        //        lock (_safeObject)
        //        {
        //            if (false == _bSafeStatus)
        //            {
        //                //LogHelper.Info("MVSLog", "Safe Door Status! Stop Axis " + sAxis.ToString() + "!");
        //                mc.GTN_Stop(1, 1 << (sAxis - 1), 0);   // 平滑停止
        //                return 0;
        //            }

        //        }

        //        if (nPreZeroPos == pValue || nsts == 512)
        //        {
        //            fSubLength = pValue - fBeginPosition;
        //            //SettingParameter.Instance().FSubPosition = fPreSubLength + (float)fSubLength;
        //            //SettingParameter.Instance().SaveSubLength();
        //            return 1;
        //        }
        //        else if (nsts == 544)
        //        {
        //            newpfm.acc = vAcc;
        //            newpfm.dec = vAcc;
        //            newpfm.smoothTime = (short)dbSmoothTime;
        //            mc.GTN_SetTrapPrm(1, sAxis, ref newpfm);
        //            mc.GTN_SetPos(1, sAxis, (int)dbCurPosition);
        //            mc.GTN_SetVel(1, sAxis, nValSpeed);
        //            mc.GTN_Update(1, 1 << (sAxis - 1));
        //            Thread.Sleep(3000);
        //            while (true)
        //            {
        //                Thread.Sleep(500);
        //                mc.GTN_ClrSts(1, sAxis, 8);
        //                mc.GTN_GetSts(1, sAxis, out nsts, 1, out pCLock);
        //                mc.GTN_GetPrfPos(1, sAxis, out pValue, 1, out pCLock);
        //                if (nsts == 512)
        //                {
        //                    break;
        //                }

        //            }

        //        }
        //        else if (nsts == 576)
        //        {
        //            newpfm.acc = vAcc;
        //            newpfm.dec = vAcc;
        //            newpfm.smoothTime = (short)dbSmoothTime;

        //            mc.GTN_SetTrapPrm(1, sAxis, ref newpfm);

        //            mc.GTN_SetPos(1, sAxis, (int)dbCurPosition);
        //            mc.GTN_SetVel(1, sAxis, nValSpeed);
        //            mc.GTN_Update(1, 1 << (sAxis - 1));
        //            while (true)
        //            {
        //                Thread.Sleep(500);
        //                mc.GTN_ClrSts(1, sAxis, 8);
        //                mc.GTN_GetSts(1, sAxis, out nsts, 1, out pCLock);
        //                mc.GTN_GetPrfPos(1, sAxis, out pValue, 1, out pCLock);
        //                if (nsts == 512)
        //                {
        //                    break;
        //                }
        //            }

        //            break;
        //        }

        //        Thread.Sleep(1000);
        //    }

        //    return 1;
        //}

        /*
         * -1 线路或者网络问题
         * 0 安全门开了
         * 1 正常返回
         */
        //nPos相对零点的位置
        public int ThreeD_Scan(short sAxis, int nValSpeed, int nPos,  float vAcc , string  strSign)
        {
            _preCmd = new stCmd
            {
                Axis = sAxis,
                Val = nValSpeed,
                Pos = nPos,
                //H = nH,
                Sign = strSign,
                V = (int)vAcc
            };
            

            if (false == _bSafeStatus)
            {
                 //LogHelper.Info("MVSLog", "Safe Door Status!");
                return 0;
            }
            //nPos 相对零点的位置
            //每次启动都会归零
            double dbDestPosition = nPos/*(float)nPos * 14705.8824*/;
            double dbDistance1 = 0/*800 * 14705.8824*/;

            double acc = 0;
            double dec = 0;
            double dbSmoothTime = 0;
            mc.TTrapPrm pfm = new mc.TTrapPrm();
            mc.GTN_PrfTrap(1, (short)sAxis);
            short sResult = mc.GTN_GetTrapPrm(1, (short)sAxis, out pfm);

            if (sResult != 0)
            {
                 //LogHelper.Info("Erro", "GTN_GetTrapPrm Error sResult " + sResult.ToString());
                return -1;
            }

            acc = pfm.acc;
            dec = pfm.dec;
            dbSmoothTime = pfm.smoothTime;

            mc.TTrapPrm newpfm = new mc.TTrapPrm();

            newpfm.acc = vAcc;
            newpfm.dec = vAcc;
            newpfm.smoothTime = 100;

            uint pCLock = 0;
            double pValue = 0;
            int nsts = 0;

            double dbCurPosition = 0;
            mc.GTN_SetTrapPrm(1, sAxis, ref newpfm);
            //mc.GTN_ZeroPos(1, sAxis, 1);
            mc.GTN_GetPrfPos(1, sAxis, out dbCurPosition, 1, out pCLock);
           
            mc.GTN_SetPos(1, sAxis, (int)dbDestPosition);
            mc.GTN_SetVel(1, sAxis, nValSpeed);
            mc.GTN_Update(1, 1 << (sAxis - 1));

           
            
            while (true)
            {
                mc.GTN_ClrSts(1, sAxis, 8);
                mc.GTN_GetSts(1, sAxis, out nsts,  1, out pCLock);
                mc.GTN_GetPrfPos(1, sAxis, out pValue, 1, out pCLock);

                if (true == BIsSetZeror)
                {
                    //SettingParameter.Instance().FSubPosition = (float)pValue;
                    //SettingParameter.Instance().SaveSubLength();
                }
                //LogHelper.Info("MVSLog", "ThreeD_Scan Get Move Status !" + pValue.ToString("0.00") + " " + dbDestPosition.ToString("0.00"));
                lock (_safeObject)
                {
                    if (false == _bSafeStatus)
                    {
                        //LogHelper.Info("MVSLog", "Safe Door Status! Stop Axis " + sAxis.ToString() + "!");
                        mc.GTN_Stop(1, 1 << (sAxis - 1), 0);   // 平滑停止
                        return 0;
                    }

                }

                if ( dbDestPosition == pValue || nsts == 512 )
                {
                    //SettingParameter.Instance().FSubPosition = (float)dbDestPosition;
                    //SettingParameter.Instance().SaveSubLength();
                    return 1;
                }
                else if(nsts == 544)
                {
                    newpfm.acc = vAcc;
                    newpfm.dec = vAcc;
                    newpfm.smoothTime = (short)dbSmoothTime;
                    mc.GTN_SetTrapPrm(1, sAxis, ref newpfm);
                    mc.GTN_SetPos(1, sAxis, (int)dbDistance1);
                    mc.GTN_SetVel(1, sAxis, nValSpeed);
                    mc.GTN_Update(1, 1 << (sAxis - 1));
                    Thread.Sleep(1500);
                    while (true)
                    {
                        Thread.Sleep(500);
                        mc.GTN_ClrSts(1, sAxis, 8);
                        mc.GTN_GetSts(1, sAxis, out nsts, 1, out pCLock);
                        mc.GTN_GetPrfPos(1, sAxis, out pValue, 1, out pCLock);
                        if (nsts == 512)
                        {
                            break;
                        }

                    }
                       
                }
                else if(nsts == 576)
                {
                    newpfm.acc= vAcc;
                    newpfm.dec = vAcc;
                    newpfm.smoothTime = (short)dbSmoothTime;

                    mc.GTN_SetTrapPrm(1, sAxis, ref newpfm);

                    mc.GTN_SetPos(1, sAxis, (int)dbDistance1);
                    mc.GTN_SetVel(1, sAxis, nValSpeed);
                    mc.GTN_Update(1, 1 << (sAxis - 1));
                    while (true)
                    {
                        Thread.Sleep(500);
                        mc.GTN_ClrSts(1, sAxis, 8);
                        mc.GTN_GetSts(1, sAxis, out nsts, 1, out pCLock);
                        mc.GTN_GetPrfPos(1, sAxis, out pValue, 1, out pCLock);
                        if (nsts == 512)
                        {
                            break;
                        }
                    }

                    break;
                }

                Thread.Sleep(100);
            }

            return 1;
        }

        public void Close()
        {
            try
            {
                mc.GTN_AxisOff(1, (int)emAxisName.emThreeD);
                mc.GTN_AxisOff(1, (int)emAxisName.emRoller);
                mc.GTN_AxisOff(1, (int)emAxisName.emPowerZ);
                mc.GTN_AxisOff(1, (int)emAxisName.emCameraZ);
                mc.GTN_AxisOff(1, (int)emAxisName.emPowerXY);
                mc.GTN_AxisOff(1, (int)emAxisName.emCameraXY);


                mc.GTN_TerminateEcatComm(1);
                mc.GTN_Close();

                _safeThread.Abort();

            }
            catch (Exception ex)
            {

            }


        }

        public void GotoDesition(int nPos, float nMoveSpeed, float fAccThread = (float)0.5)
        {
            int nStatus = -1;
            bool bInterupted = false;
            do
            {
                if (false == bInterupted)
                {
                    nStatus = ThreeD_Scan((int)CMotionCardController.emAxisName.emThreeD, (int)nMoveSpeed, nPos, fAccThread, "MeasureStart");

                    if (nStatus == 1 || nStatus == -1)
                    {
                        break;
                    }
                    else if (nStatus == 0)
                    {
                        Thread.Sleep(1000);
                        if (CurPage != null)
                        {
                            CurPage.BeginInvoke(CurPage.stateFunc, true);
                            bInterupted = true;
                        }
                        LogHelper.Info("MVSLog", "Continue to GotoPosition!");
                    }
                }
                else
                {
                    if (CurPage.BContinueState == true)
                    {
                        bInterupted = false;
                        CurPage.BContinueState = false;
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }



            } while (nStatus != 1);


        }


        public void Init()
        {
            try
            {
                mc.GTN_Open(5, 1);
                mc.GTN_InitEcatComm(1);

                short nStatus = 0;
                do
                {
                    Thread.Sleep(1000);
                    mc.GTN_IsEcatReady(1, out nStatus);
                } while (nStatus == 0);

                mc.GTN_StartEcatComm(1);
                mc.GTN_Reset(1);
                mc.GTN_LoadConfig(1, "Gecat.xml");

                Thread.Sleep(100);
                //glink.GT_GLinkInit(0);
                //_safeThread = new Thread(SafeThreadFunc);
                //_safeThread.Start();

                mc.GTN_ClrSts(1, (int)emAxisName.emThreeD, 8);
                mc.GTN_AxisOn(1, (int)emAxisName.emThreeD);
                mc.GTN_ZeroPos(1, (int)emAxisName.emThreeD, 1);

                //没有限位传感器，归零取消
                /*
                int v = 500000;
                ushort sHomeSts = 0;
                mc.GTN_SetHomingMode(1, (int)emAxisName.emThreeD, 6);
                mc.GTN_SetEcatHomingPrm(1, (int)emAxisName.emThreeD, 4, v, v, v, 0, 0);
                mc.GTN_StartEcatHoming(1, (int)emAxisName.emThreeD);
                mc.GTN_GetEcatHomingStatus(1, (int)emAxisName.emThreeD, out sHomeSts);
                bool bInterupted = false;
                while (true)
                {
                    Thread.Sleep(500);
                    if (false == bInterupted)
                    {
                        lock (_safeObject)
                        {
                            if (_bSafeStatus == false)
                            {
                                mc.GTN_StopEcatHoming(1, (int)emAxisName.emThreeD);
                                mc.GTN_SetHomingMode(1, (int)emAxisName.emThreeD, 8);
                                mc.GTN_ClrSts(1, (int)emAxisName.emThreeD, 8);
                                bInterupted = true;
                                CurPage.BeginInvoke(CurPage.stateFunc, true);
                                continue;
                            }
                        }
                        mc.GTN_GetEcatHomingStatus(1, (int)emAxisName.emThreeD, out sHomeSts);
                        if (sHomeSts == 1 || sHomeSts == 3)
                        {
                            break;
                        }
                    }
                    else
                    {
                        lock (_safeObject)
                        {
                            if (true == CurPage.BContinueState)
                            {
                                mc.GTN_SetHomingMode(1, (int)emAxisName.emThreeD, 6);
                                mc.GTN_SetEcatHomingPrm(1, (int)emAxisName.emThreeD, 4, v, v, v, 0, 0);
                                mc.GTN_StartEcatHoming(1, (int)emAxisName.emThreeD);
                                mc.GTN_GetEcatHomingStatus(1, (int)emAxisName.emThreeD, out sHomeSts);
                                bInterupted = false;
                                CurPage.BContinueState = false;
                            }
                            else
                            {
                                Thread.Sleep(200);
                            }
                        }
                    }



                }
                mc.GTN_StopEcatHoming(1, (int)emAxisName.emThreeD);
                mc.GTN_SetHomingMode(1, (int)emAxisName.emThreeD, 8);
                mc.GTN_ClrSts(1, (int)emAxisName.emThreeD, 8);
                */
            }
            catch (Exception ex)
            {

            }
           
            

        }

        private void SafeThreadFunc()
        {
            while(true)
            {

                if (1 == GetDIOStatus(0, (short)emDI.EM_SAFEDOOR))
                {
                    lock(_safeObject)
                    {
                        _bSafeStatus = false;
                    }
                     //LogHelper.Info("MVSLog", "Safe Door Staus!");
                }
                else
                {
                    lock (_safeObject)
                    {
                        _bSafeStatus = true;
                    }
                }

                Thread.Sleep(1000);
            }
        }

      
    }
}
