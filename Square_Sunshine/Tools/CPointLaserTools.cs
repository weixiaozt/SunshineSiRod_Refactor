using HalconDotNet;
using STTech.BytesIO.Serial;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SiliconRoundBarCheck.Cameras.SSZNCamTools;

namespace SquareSiliconStickCheck.Tools
{
    public class CPointLaserTools
    {
        private static CPointLaserTools _instance = null;

        private SerialClient _serialClientTop = null;
        private SerialClient _serialClientLeft = null;
        private SerialClient _serialClientRight = null;
        private SerialClient _serialClientDown = null;

        private ArrayList[] _arrLegnthComs = null;
        private object[] _objLengths = null;

        public enum emType
        {
            EM_LEFT = 0,
            EM_RIGHT = 1,
            EM_UP = 2,
            EM_DOWN = 3
        }

        private class tagParamInfo
        {
           
          
            public string strSerial;
            public int nSquareType;

            public tagParamInfo( string strserialnum, int nType = 0)
            {
                nSquareType = nType;
                strSerial = strserialnum;
            }
        }

        public static CPointLaserTools Instance()
        {
            if (_instance == null)
            {
                _instance = new CPointLaserTools();
            }

            return _instance;
        }


        private CPointLaserTools()
        {
            _serialClientTop = new SerialClient();
            //_serialClientDown= new SerialClient();
            _serialClientLeft = new SerialClient();
            _serialClientRight = new SerialClient();
            _arrLegnthComs = new ArrayList[4];
            _objLengths = new object[4];
            for (int i = 0; i < 4; i++)
            {
                _objLengths[i] = new object();
                _arrLegnthComs[i] = new ArrayList();
            }
        }

        Thread _threadScan = null;
        public bool _bNeedStopped = false;
        public void InitComs(int nTopCom, int  nLeftCom, int nRightCom, /*int nDownCom,*/ int nBaud)
        {
            try
            {
                _serialClientLeft.OnConnectedSuccessfully += _serialClientLeft_OnConnectedSuccessfully;
                _serialClientLeft.OnConnectionFailed += _serialClientLeft_OnConnectionFailed;
                _serialClientLeft.OnDataReceived += _serialClientLeft_OnDataReceived;
                _serialClientLeft.OnDisconnected += _serialClientLeft_OnDisconnected;

                _serialClientRight.OnConnectedSuccessfully += _serialClientRight_OnConnectedSuccessfully;
                _serialClientRight.OnConnectionFailed += _serialClientRight_OnConnectionFailed;
                _serialClientRight.OnDataReceived += _serialClientRight_OnDataReceived;
                _serialClientRight.OnDisconnected += _serialClientRight_OnDisconnected;

                _serialClientTop.OnConnectedSuccessfully += _serialClientTop_OnConnectedSuccessfully;
                _serialClientTop.OnConnectionFailed += _serialClientTop_OnConnectionFailed;
                _serialClientTop.OnDataReceived += _serialClientTop_OnDataReceived;
                _serialClientTop.OnDisconnected += _serialClientTop_OnDisconnected;

                //_serialClientDown.OnConnectionFailed += _serialClientDown_OnConnectionFailed;
                ///_serialClientDown.OnConnectedSuccessfully += _serialClientDown_OnConnectedSuccessfully;
                //_serialClientDown.OnDataReceived += _serialClientDown_OnDataReceived;
                //_serialClientDown.OnDisconnected += _serialClientDown_OnDisconnected;

                _serialClientLeft.BaudRate = 115200;
                _serialClientLeft.PortName = "COM" + nLeftCom.ToString();
                _serialClientLeft.Connect();

                _serialClientRight.BaudRate = 115200;
                _serialClientRight.PortName = "COM" + nRightCom.ToString();
                _serialClientRight.Connect();

                _serialClientTop.BaudRate = 115200;
                _serialClientTop.PortName = "COM" + nTopCom.ToString();
                _serialClientTop.Connect();

                //_serialClientDown.BaudRate = 115200;
                //_serialClientDown.PortName = "COM" + nDownCom.ToString();
                //_serialClientDown.Connect();



            }
            catch (Exception e)
            {
                LogHelper.Info("", "Exception Init Coms");
            }
           


        }
        public void Measure_SquareStick(string strSerialNum, int nSquareType = 0)
        {
            tagParamInfo taginfo = new tagParamInfo(strSerialNum, nSquareType);

            _bNeedStopped = true;

            _threadScan = new Thread(new ParameterizedThreadStart(threadScanSquareStick));
            _threadScan.Start(taginfo);
        }

        public void threadScanSquareStick(object objtype)
        {
            tagParamInfo param = (tagParamInfo)objtype;

            ClearDatas();
            float fLeftValue = 0;
            float fRightValue = 0;
            float fUpValue = 0;
            float fDownValue = 0;
            while (true)
            {
                
                if (true == GetData(emType.EM_LEFT, ref fLeftValue))
                {

                }



                if (_bNeedStopped == true)
                {
                    break;
                }

            }


        }
       
        public void ClearDatas()
        {
            for (int i = 0; i < 4; i++)
            {
                ClearData((emType)i);
            }
        }

        public void SaveDatas()
        {
            for(int i = 0; i < 4;i++)
            {
                emType emTyped = (emType)i;
                SaveData(emTyped);
                LogHelper.Info("", "Save Data i " + i.ToString() + " OK!");
            }
            LogHelper.Info("", "Save Datas OK!");
        }

        public void SaveData(emType nType)
        {
            try
            {
                HTuple fileHandle = new HTuple();
                string strFileName = "";
                switch (nType)
                {
                    case emType.EM_LEFT:
                        {
                            strFileName = "D:/Image1/left.txt";
                            break;
                        }
                    case emType.EM_RIGHT:
                        {
                            strFileName = "D:/Image1/right.txt";
                            break;
                        }
                    case emType.EM_UP:
                        {
                            strFileName = "D:/Image1/top.txt";
                            break;
                        }
                    case emType.EM_DOWN:
                        {
                            strFileName = "D:/Image1/down.txt";
                            break;
                        }

                }
                HOperatorSet.OpenFile(strFileName, "output", out fileHandle);
                string strInfo = "";
                lock (_objLengths[(int)nType])
                {
                    for (int i = 0; i < _arrLegnthComs[(int)nType].Count; i++)
                    {
                        strInfo += ((float)_arrLegnthComs[(int)nType][i]).ToString("0.00") + "\r\n";
                    }
                }

                HOperatorSet.FwriteString(fileHandle, strInfo);
                HOperatorSet.CloseFile(fileHandle);
            }
            catch(Exception e)
            {

            }
            
        }

        public void ClearData(emType nType)
        {
            lock (_objLengths[(int)nType])
            {
                _arrLegnthComs[(int)nType].Clear();
            }
        }

        private void _serialClientDown_OnDisconnected(object sender, STTech.BytesIO.Core.DisconnectedEventArgs e)
        {
            LogHelper.Info("", "Down  DisConnected ");
        }

        private void _serialClientDown_OnDataReceived(object sender, STTech.BytesIO.Core.DataReceivedEventArgs e)
        {
            DealData(e.Data, emType.EM_DOWN);
        }

        private void _serialClientDown_OnConnectedSuccessfully(object sender, STTech.BytesIO.Core.ConnectedSuccessfullyEventArgs e)
        {
            LogHelper.Info("", "Down Connected Successed");
        }

        private void _serialClientDown_OnConnectionFailed(object sender, STTech.BytesIO.Core.ConnectionFailedEventArgs e)
        {
            LogHelper.Info("", "Down Connected Failed");
        }

        private void _serialClientTop_OnDisconnected(object sender, STTech.BytesIO.Core.DisconnectedEventArgs e)
        {
            LogHelper.Info("", "Up  DisConnected ");
        }

        private void _serialClientTop_OnDataReceived(object sender, STTech.BytesIO.Core.DataReceivedEventArgs e)
        {
            DealData(e.Data, emType.EM_UP);
        }

       


        public bool GetData(emType nType, ref float fData)
        {
            lock (_objLengths[(int)nType])
            {
                if (_arrLegnthComs[(int)nType].Count > 0)
                {
                    fData = (float)_arrLegnthComs[(int)nType][0];
                    _arrLegnthComs[(int)nType].RemoveAt(0);
                    return true;
                }
            }

            return false;
        }

        private void DealData(byte[] data, emType nType) 
        {
            if (data == null)
            {
                return;
            }

            if (data.Length < 16) 
            {
                return;
            }

            int nSearchIndex = 0;
            while (true)
            {
                try
                {
                    do
                    {
                        if (data[nSearchIndex] == 0xFA && data[nSearchIndex + 1] == 0xFB)
                        {
                            break;
                        }

                        nSearchIndex++;

                        if (nSearchIndex >= data.Length - 1)
                        {
                            break;
                        }

                    } while (true);


                    byte tmpf = data[nSearchIndex + 8];
                    byte tmps = data[nSearchIndex + 9];
                    byte tmpt = data[nSearchIndex + 10];
                    byte tmpfo = data[nSearchIndex + 11];

                    int nLength = (tmpfo * 256 + tmpt) * 65536 + (tmps * 256 + tmpf);

                    if (nLength > 0)
                    {
                        float fLength = (float)nLength / 1000;

                        lock (_objLengths[(int)nType])
                        {
                            _arrLegnthComs[(int)nType].Add(fLength);
                        }
                    }
                    else
                    {
                        lock (_objLengths[(int)nType])
                        {
                            _arrLegnthComs[(int)nType].Add((float)-1);
                        }
                    }    
                   
                    nSearchIndex += 16;
                    if (nSearchIndex + 16 >= data.Length)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    break;
                }
                
            }
            
            



        }

        private void _serialClientTop_OnConnectionFailed(object sender, STTech.BytesIO.Core.ConnectionFailedEventArgs e)
        {
            LogHelper.Info("", "Up Connected Failed");
        }

        private void _serialClientTop_OnConnectedSuccessfully(object sender, STTech.BytesIO.Core.ConnectedSuccessfullyEventArgs e)
        {
            LogHelper.Info("", "Up Connected Successed");

        }

        private void _serialClientRight_OnDisconnected(object sender, STTech.BytesIO.Core.DisconnectedEventArgs e)
        {
            LogHelper.Info("", "Right  DisConnected ");
        }

        private void _serialClientRight_OnDataReceived(object sender, STTech.BytesIO.Core.DataReceivedEventArgs e)
        {
            DealData(e.Data, emType.EM_RIGHT);
        }

        private void _serialClientRight_OnConnectionFailed(object sender, STTech.BytesIO.Core.ConnectionFailedEventArgs e)
        {
            LogHelper.Info("", "Right Connected Failed");
        }

        private void _serialClientRight_OnConnectedSuccessfully(object sender, STTech.BytesIO.Core.ConnectedSuccessfullyEventArgs e)
        {
            LogHelper.Info("", "Right Connected Successed");
        }

        private void _serialClientLeft_OnDisconnected(object sender, STTech.BytesIO.Core.DisconnectedEventArgs e)
        {

            LogHelper.Info("", "Left  DisConnected ");
        }

        private void _serialClientLeft_OnDataReceived(object sender, STTech.BytesIO.Core.DataReceivedEventArgs e)
        {
            DealData(e.Data, emType.EM_LEFT);
        }

        private void _serialClientLeft_OnConnectionFailed(object sender, STTech.BytesIO.Core.ConnectionFailedEventArgs e)
        {
            LogHelper.Info("", "Left Connected Failed");
        }

        private void _serialClientLeft_OnConnectedSuccessfully(object sender, STTech.BytesIO.Core.ConnectedSuccessfullyEventArgs e)
        {
            LogHelper.Info("", "Left Connected Successed");
        }
    }
}
