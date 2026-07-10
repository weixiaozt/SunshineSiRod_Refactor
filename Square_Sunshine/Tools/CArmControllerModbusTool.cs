using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyModbus;
using SquareSiliconStickCheck.Parameters;


namespace SquareSiliconStickCheck.Tools
{
    internal class CArmControllerModbusTool
    {
        private static CArmControllerModbusTool _instance;
        private EasyModbus.ModbusClient _modbusClient;
        public string receiveData = null;

        public static CArmControllerModbusTool Instance()
        {
            if (null == _instance)
            {
                _instance = new CArmControllerModbusTool();
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
           
       

        private CArmControllerModbusTool()
        {
            _modbusClient = new EasyModbus.ModbusClient();
            _modbusClient.ReceiveDataChanged += new EasyModbus.ModbusClient.ReceiveDataChangedHandler(UpdateReceiveData);
            _modbusClient.SendDataChanged += new EasyModbus.ModbusClient.SendDataChangedHandler(UpdateSendData);
            _modbusClient.ConnectedChanged += new EasyModbus.ModbusClient.ConnectedChangedHandler(UpdateConnectedChanged);
        }

        void UpdateReceiveData(object sender)
        {
            //string receiveData = "Rx: " + BitConverter.ToString(_modbusClient.receiveData).Replace("-", " ") + System.Environment.NewLine;

            // LogHelper.Info("Silicon",receiveData);
            //Thread thread = new Thread(updateReceiveTextBox);
            //thread.Start();
        }

        void UpdateSendData(object sender)
        {
            //string sendData = "Tx: " + BitConverter.ToString(_modbusClient.sendData).Replace("-", " ") + System.Environment.NewLine;
           // Thread thread = new Thread(updateSendTextBox);
           // thread.Start();

        }
        public void ReadTwoRegister(int nStartIndex, out float fRegisterInfo)
        {
            int nRegisterInfoFir = 0;
            int nRegisterInfoSec = 0;

            ReadSingleRegister(nStartIndex, ref nRegisterInfoFir);
            ReadSingleRegister(nStartIndex + 1, ref nRegisterInfoSec);

            byte[] btFir = BitConverter.GetBytes((UInt16)nRegisterInfoFir);
            byte[] btSec = BitConverter.GetBytes((UInt16)nRegisterInfoSec);

            byte[] btInfo = new byte[4];
            btInfo[0] = btFir[0];
            btInfo[1] = btFir[1];
            btInfo[2] = btSec[0];
            btInfo[3] = btSec[1];

            float fValue =  BitConverter.ToSingle(btInfo, 0);
            fValue = Math.Max(1, fValue);
            fRegisterInfo = fValue;
        }

        public bool WriteTwoRegister(int nStartIndex, float fRegisterInfo)
        {
            
            try
            {
                UInt16[] uiRegisterInfo = FloatToTwoUInt16(fRegisterInfo);
                int[] iRegisterInfo = new int[uiRegisterInfo.Length];
                for (int i = 0; i < iRegisterInfo.Length; i++)
                {
                    iRegisterInfo[i] = uiRegisterInfo[i];
                }
                WriteSingleRegister(nStartIndex, iRegisterInfo[1]);
                WriteSingleRegister(nStartIndex + 1, iRegisterInfo[0]);

                return true;
            }
            catch (Exception e)
            {
                Thread.Sleep(500);
                    
                LogHelper.Info("Silicon", "ModBus Connected Error, Reconnec " + e.Message);
                FormMain.formMainF.showMessageDelegate.Invoke("modbus disconnect Reconnect!");
            }

            return false;
        }

        public bool WriteSingleRegister(int nStartIndex, int nRegisterInfo)
        {
           
            bool bReconnect = false;
            do
            {
                try
                {
                    if (bReconnect == true)
                    {
                        _modbusClient.Connect();
                        _modbusClient.WriteSingleRegister(nStartIndex, nRegisterInfo);
                        return true;
                    }
                    _modbusClient.WriteSingleRegister(nStartIndex, nRegisterInfo);
                    return true;
                }
                catch (Exception e)
                {
                    Thread.Sleep(500);
                    bReconnect = true;
                     LogHelper.Info("Silicon","ModBus Connected Error, Reconnec " + e.Message);
                    FormMain.formMainF.showMessageDelegate.Invoke("modbus disconnect Reconnect!");
                }

                Thread.Sleep(1000);
            } while(true);
            
        }

        public bool WriteSingleCoil(int nStartIndex, bool bStatus)
        {
           
            bool bReconnect = false;
            do
            {
                try
                {
                    if (true == bReconnect)
                    {
                        _modbusClient.Connect();
                        _modbusClient.WriteSingleCoil(nStartIndex, bStatus);

                        return true;
                    }

                    _modbusClient.WriteSingleCoil(nStartIndex, bStatus);
                    return true;
                }
                catch (Exception e)
                {
                    Thread.Sleep(500);
                    bReconnect = true;
                     LogHelper.Info("Silicon","ModBus Connected Error, Reconnec " + e.Message);
                    FormMain.formMainF.showMessageDelegate.Invoke("modbus disconnect Reconnect!");
                }

                Thread.Sleep(1000);
            } while (true);

        }


        public bool ReadSingleCoil(int nStartIndex, ref bool bStatus)
        {
            bool bReconnect = false;
            int nRegisterCount = 1;
            do
            {
                try
                {
                    if (true == bReconnect)
                    {
                        _modbusClient.Connect();
                        bool[] serverResponsevalue = _modbusClient.ReadCoils(nStartIndex, nRegisterCount);
                        bStatus = serverResponsevalue[0];
                        return true;
                    }

                    bool[] serverResponse = _modbusClient.ReadCoils(nStartIndex, nRegisterCount);
                    bStatus = serverResponse[0];
                    return true;
                }
                catch (Exception e)
                {
                    Thread.Sleep(500);
                    bReconnect = true;
                     LogHelper.Info("Silicon","ModBus Connected Error, Reconnect " + e.Message);
                    FormMain.formMainF.showMessageDelegate.Invoke("modbus disconnect Reconnect!");

                }

                Thread.Sleep(1000);

            } while (true);
        }

        public bool ReadSingleRegister(int nStartIndex, ref int nRegisterInfo)
        {
            int nRegisterCount = 1;
            bool bReconnect = false;
            do
            {
                try
                {

                    if (true == bReconnect)
                    {
                        _modbusClient.Connect();
                        int[] serverResponsevalue = _modbusClient.ReadHoldingRegisters(nStartIndex, nRegisterCount);
                        nRegisterInfo = serverResponsevalue[0];
                        return true;
                    }

                    int[] serverResponse = _modbusClient.ReadHoldingRegisters(nStartIndex, nRegisterCount);
                    nRegisterInfo = serverResponse[0];
                    return true;
                }
                catch (Exception e)
                {
                    Thread.Sleep(500);
                    bReconnect = true;
                     LogHelper.Info("Silicon","ModBus Connected Error, Reconnect " + e.Message);
                    FormMain.formMainF.showMessageDelegate.Invoke("modbus disconnect Reconnect!");
                    
                }

                Thread.Sleep(1000);

            } while (true);
            

            //return false;
        }

        public bool ConnectServer()
        {
            try
            {
                _modbusClient.IPAddress = SettingParameter.Instance().StrIPArmControl;
                _modbusClient.Port = SettingParameter.Instance().NArmControlPort;
                _modbusClient.Connect();
            }
            catch (Exception e)
            {
                 LogHelper.Info("Silicon","Connect MoveController Server Failed" + _modbusClient.IPAddress + " " + _modbusClient.Port + " Connected " + _modbusClient.Connected.ToString());
                return false;
            }
            

             LogHelper.Info("Silicon","SSZN ConnectServer " + _modbusClient.IPAddress + " " + _modbusClient.Port + " Connected " + _modbusClient.Connected.ToString());
            return true;
        }

       
        private void UpdateConnectedChanged(object sender)
        {
            if (_modbusClient.Connected)
            {
               // FormMain.formMainF.showMessageDelegate.Invoke("modbus Connected");
                 LogHelper.Info("Silicon","Connected to Modbus Server");
            }
            else
            {
                //FormMain.formMainF.showMessageDelegate.Invoke("modbus DisConnected", (int)FormMain.emMSGTYPE.EM_MODBUSDISCONNECT);
                FormMain.formMainF.showMessageDelegate.Invoke("modbus DisConnected to Modbus Server");
                 LogHelper.Info("Silicon","modbus DisConnected  to Modbus Server");

            }
        }

    }
}
