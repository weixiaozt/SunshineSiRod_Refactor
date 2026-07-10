using SiliconRoundBarCheck.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YouEyEE.Untils.Log;

namespace SiliconRoundBarCheck.Tools
{
    internal class CMoveController
    {
        private static CMoveController _instance;

        public static CMoveController Instance()
        {
            if(_instance == null)
            {
                _instance = new CMoveController();
            }

            return _instance;
        }

        public bool ConnectServer()
        {
            if (false == MoveControllerModbusTool.Instance().ConnectServer())
            {
                return false;
            }
            GoToBeginPosition();
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
           

        }

        /*
         * 计算直径
         */
        public bool GoToTailPosition()
        {
            FlashLogger.Info("Silicon","GoToTailPosition Begin Stick Tail " + SettingParameter.Instance().NStickLength.ToString("0.00"));
            MoveControllerModbusTool.Instance().WriteSingleCoil(122, false); //位置1  晶棒尾部
            MoveControllerModbusTool.Instance().WriteTwoRegister(122, SettingParameter.Instance().NStickLength);
            MoveControllerModbusTool.Instance().WriteSingleCoil(122, true);
            bool bStatus = false;
            while (true)
            {
                Thread.Sleep(2000);
                MoveControllerModbusTool.Instance().ReadSingleCoil(122, ref bStatus);
               
                if (bStatus == false)
                {
                    return true;
                }
                else
                {
                    FlashLogger.Info("Silicon","GoToTailPosition Waited For End Position");
                }
            }
            
        }

        public void GotoDestPosition(float fDestValue, int nStartIndex = 120)
        {
            
            bool bStatus = true;
            float fDestination = fDestValue * (float)0.91;
            FlashLogger.Info("Silicon","GotoDestPosition Begin nStartIndex " + nStartIndex.ToString() + " fDestValue " + fDestValue.ToString("0.00") + " fDestination " + fDestination.ToString("0.00"));
            MoveControllerModbusTool.Instance().WriteSingleCoil(nStartIndex, true);
            MoveControllerModbusTool.Instance().WriteTwoRegister(nStartIndex, fDestination); //位置2
          
            while (true)
            {
                Thread.Sleep(1000);
                MoveControllerModbusTool.Instance().ReadSingleCoil(nStartIndex, ref bStatus);
                if (bStatus == false)
                {
                    break;
                }
                else
                {
                    FlashLogger.Info("Silicon","GotoDestPosition Waited For Begin Position");
                }
            }

            return;
        }

        public void WaitStartSignal()
        {
            bool bStatus = false;
            FlashLogger.Info("Silicon","WaitStartSignal Begin");
           
            while (true)
            {
                Thread.Sleep(1000);
                MoveControllerModbusTool.Instance().ReadSingleCoil(1000, ref bStatus);
                if (bStatus == true)
                {
                    break;
                }
                else
                {
                    FlashLogger.Info("Silicon","WaitStartSignal Continue......");
                }
            }

            return;
        }

        public void GoToBeginPosition()
        {
            bool bStatus = false;
            FlashLogger.Info("Silicon","GoToBeginPosition Begin");
            MoveControllerModbusTool.Instance().WriteSingleCoil(120, false); //位置1
            MoveControllerModbusTool.Instance().WriteSingleRegister(120, 0);
            MoveControllerModbusTool.Instance().WriteSingleRegister(121, 0);
            MoveControllerModbusTool.Instance().WriteSingleCoil(120, true);
            while(true)
            {
                Thread.Sleep(1000);
                MoveControllerModbusTool.Instance().ReadSingleCoil(120, ref bStatus);
                if (bStatus == false)
                {
                    break;
                }
                else
                {
                    FlashLogger.Info("Silicon","GoToTailPosition Waited For Begin Position");
                }
            }

            return ;
        }

        public void Light(bool bRun = true)
        {
            if (bRun)
            {
                MoveControllerModbusTool.Instance().WriteSingleCoil(177, true);
            }
            else
            {
                MoveControllerModbusTool.Instance().WriteSingleCoil(177, false);
            }
        }

        public void Rotate(bool bRun = true)
        {
            if ( true == bRun)
            {
                MoveControllerModbusTool.Instance().WriteSingleCoil(212, true);
            }
            else
            {
                MoveControllerModbusTool.Instance().WriteSingleCoil(212, false);
            }
        }

       
    }
}
