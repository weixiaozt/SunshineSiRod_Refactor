
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlX.XDevAPI;
using Org.BouncyCastle.Crypto.Tls;
using SquareSiliconStickCheck.Tools;

namespace SquareSiliconStickCheck.Parameters
{
    internal class SettingParameter
    {

        enum emCamType
        {
            emCamType_SSZN = 0,
            emCamType_XG = 1
        }

        private string _strTopLaser3DIP = "192.168.0.20";  //实时视频
        private string _strLeftLaser3DIP = "192.168.1.11";
        private string _strRightLaser3DIP = "192.168.2.10";
        private string _strDownLaser3DIP = "192.168.3.10";
        private string _strTopLocalIP = "192.168.0.12";  //本地IP
        private string _strLeftLocalIP = "192.168.1.12";
        private string _strRightLocalIP = "192.168.2.12";
        private string _strDownLocalIP = "192.168.3.12";

        private string _strIPArmControl = "127.0.0.1";
        private string _strIPMoveControl = "127.0.0.1";
        private string _strSaveDir = "D:/stickbmp";
        private string _strMySQLIP = "127.0.0.1";
        private string _strMySQLUser = "root";
        private string _strMySQLPwd = "123456";
        private int _nMoveControlPort = 502;
        private int _nArmControlPort = 502;
        private int _nSectionCount = 2;
        private int _nSquareType = 16;
        private int _nSSZNFrameHeight = 1000;
        private int _nStickLength = 600;
        private int _nMySQLPort = 3308;
        private string _strMySQLDBName = "squaresiliconstick";
        private int _nDaemon = 0; // 1 在线  0 离线
        private int _nSaveBmp = 1; // 0 不保存  1 保存
        private float _fSiliconStickBegin = 0;
        private float _fSiliconStickTail = 0;
        private float _fSiliconStickMinRaidus;
        private float _fSiliconStickMaxRaidus;
        private float _fPosition_fir = 0;
        private float _fPosition_fir_s = 0;

        private float _fPosition_sec = 44500;
        private float _fPosition_sec_s = 44500;

        private float _fPosition_thr = 0;
     
        private float _fIncreaseSpeed = 1000;
        private float _fCameraXYSpeed = 500;
        private float _fCurZeroPosition = 0;
        private float _fMoveSpeed = 100;
        private string _strIniFilePath;
        private string _strImagePath = "D:/image";
        private string _switchMorningWorkTime = "8:0";
        private string _switchNightWorkTime = "20:0";
        private string _strBatch;
        private bool _isZeroPosSet;
        private int _nMoveType;
        private int _nSetZeroType; // 0 未设置  1 设置
        private int _nCurPositionRegister = 2120;
        private int _nMotionCardTailPosition = -1000000000;
        private int _nMotionCardBeginPosition = 0;
        private float _fPLCIncreaseSpeed = 50;
        private float _fPLCRotateIncreaseSpeed = 10;
        private float _fPLCMoveSpeed = 1000;

       
        private float _fCheckMinLength = 0.04f;
        private float _fCheckMinVerticalLength = 0.04f;
        private float _fModLTLength = 0.01f;
        private float _fModRTLength = 0.01f;
        private float _fModLRLength = 0.01f;
        private float _fModTDLength = 0.01f;
        private int _nLeftCom = 3;
        private int _nRightCom = 5;
        private int _nTopCom = 6;
        private int _nCamType = 0;
        private static SettingParameter _instance;
        public string StrLeftLaser3DIP { get => StrLeftLaser3DIP1; set => StrLeftLaser3DIP1 = value; }
        public string StrIniFile { get => _strIniFilePath; set => _strIniFilePath = value; }
       
        public int NSSZNFrameHeight { get => _nSSZNFrameHeight; set => _nSSZNFrameHeight = value; }
        public int NMoveControlPort { get => _nMoveControlPort; set => _nMoveControlPort = value; }
        public int NSectionCount { get => _nSectionCount; set => _nSectionCount = value; }
        public string StrSaveDir { get => _strSaveDir; set => _strSaveDir = value; }
        public string StrMySQLIP { get => _strMySQLIP; set => _strMySQLIP = value; }
        public int NMySQLPort { get => _nMySQLPort; set => _nMySQLPort = value; }
        public string StrMySQLUser { get => _strMySQLUser; set => _strMySQLUser = value; }
        public string StrMySQLPwd { get => _strMySQLPwd; set => _strMySQLPwd = value; }
        public int NStickLength { get => _nStickLength; set => _nStickLength = value; }

        public int NSquareType { get => _nSquareType; set => _nSquareType = value; }
        public float FModLTLength { get => _fModLTLength; set => _fModLTLength = value; }
        public float FModRTLength { get => _fModRTLength; set => _fModRTLength = value; }
        public float FModLRLength { get => _fModLRLength; set => _fModLRLength = value; }
        public float FModTDLength { get => _fModTDLength; set => _fModTDLength = value; }
        public float FCheckMinLength { get => _fCheckMinLength; set => _fCheckMinLength = value; }
        public float FCheckMinVerticalLength { get => _fCheckMinVerticalLength; set => _fCheckMinVerticalLength = value; }
        public float FSiliconStickBegin { get => _fSiliconStickBegin; set => _fSiliconStickBegin = value; }
        public float FSiliconStickTail { get => _fSiliconStickTail; set => _fSiliconStickTail = value; }
       public float FSiliconStickMinRaidus { get => _fSiliconStickMinRaidus; set => _fSiliconStickMinRaidus = value; }
        public float FSiliconStickMaxRaidus { get => _fSiliconStickMaxRaidus; set => _fSiliconStickMaxRaidus = value; }
        public int NDaemon { get => _nDaemon; set => _nDaemon = value; }
        public string StrMySQLDBName { get => _strMySQLDBName; set => _strMySQLDBName = value; }
      
        public string StrImagePath { get => _strImagePath; set => _strImagePath = value; }
        public string SwitchMorningWorkTime { get => _switchMorningWorkTime; set => _switchMorningWorkTime = value; }
        public string SwitchNightWorkTime { get => _switchNightWorkTime; set => _switchNightWorkTime = value; }
        public string StrBatch { get => _strBatch; set => _strBatch = value; }
        public float FPosition_fir { get => _fPosition_fir; set => _fPosition_fir = value; }
        public float FPosition_sec { get => _fPosition_sec; set => _fPosition_sec = value; }
        public float FPosition_thr { get => _fPosition_thr; set => _fPosition_thr = value; }
       
        public float FMoveIncreaseSpeed { get => _fIncreaseSpeed; set => _fIncreaseSpeed = value; }
       
        public int NMoveType { get => _nMoveType; set => _nMoveType = value; }
        public int NCurPositionRegister { get => _nCurPositionRegister; set => _nCurPositionRegister = value; }
       
        public float FCameraXYSpeed { get => _fCameraXYSpeed; set => _fCameraXYSpeed = value; }
       
        public float FCurZeroPosition { get => _fCurZeroPosition; set => _fCurZeroPosition = value; }
        public bool IsZeroPosSet { get => _isZeroPosSet; set => _isZeroPosSet = value; }
        public int NSetZeroType { get => _nSetZeroType; set => _nSetZeroType = value; }
        public int NMotionCardTailPosition { get => _nMotionCardTailPosition; set => _nMotionCardTailPosition = value; }
        public int NMotionCardBeginPosition { get => _nMotionCardBeginPosition; set => _nMotionCardBeginPosition = value; }
       
        public float FPLCIncreaseSpeed { get => _fPLCIncreaseSpeed; set => _fPLCIncreaseSpeed = value; }
        public float FPLCRotateIncreaseSpeed { get => _fPLCRotateIncreaseSpeed; set => _fPLCRotateIncreaseSpeed = value; }
        public float FPLCMoveSpeed { get => _fPLCMoveSpeed; set => _fPLCMoveSpeed = value; }
        public string StrTopLaser3DIP { get => _strTopLaser3DIP; set => _strTopLaser3DIP = value; }
        public string StrLeftLaser3DIP1 { get => _strLeftLaser3DIP; set => _strLeftLaser3DIP = value; }
        public string StrRightLaser3DIP { get => _strRightLaser3DIP; set => _strRightLaser3DIP = value; }
        public string StrDownLaser3DIP { get => _strDownLaser3DIP; set => _strDownLaser3DIP = value; }
        public float FMoveSpeed { get => _fMoveSpeed; set => _fMoveSpeed = value; }
        public string StrIPMoveControl { get => _strIPMoveControl; set => _strIPMoveControl = value; }
        public string StrIPArmControl { get => _strIPArmControl; set => _strIPArmControl = value; }
        public int NArmControlPort { get => _nArmControlPort; set => _nArmControlPort = value; }
        public int NSaveBmp { get => _nSaveBmp; set => _nSaveBmp = value; }
        public string StrTopLocalIP { get => _strTopLocalIP; set => _strTopLocalIP = value; }
        public string StrLeftLocalIP { get => _strLeftLocalIP; set => _strLeftLocalIP = value; }
        public string StrRightLocalIP { get => _strRightLocalIP; set => _strRightLocalIP = value; }
        public string StrDownLocalIP { get => _strDownLocalIP; set => _strDownLocalIP = value; }
    
        public int NCamType { get => _nCamType; set => _nCamType = value; }
        public float FPosition_fir_s { get => _fPosition_fir_s; set => _fPosition_fir_s = value; }
        public float FPosition_sec_s { get => _fPosition_sec_s; set => _fPosition_sec_s = value; }
        public int NLeftCom { get => _nLeftCom; set => _nLeftCom = value; }
        public int NRightCom { get => _nRightCom; set => _nRightCom = value; }
        public int NTopCom { get => _nTopCom; set => _nTopCom = value; }

        private SettingParameter()
        {
            _isZeroPosSet = false;
            _strIniFilePath = Convert.ToString(System.AppDomain.CurrentDomain.BaseDirectory) + "ConfigIni.ini";

            _nCamType = (int)emCamType.emCamType_SSZN;
        }

        public static SettingParameter Instance()
        {
            if (null == _instance) 
            {
                _instance = new SettingParameter();
            }

            return _instance;
        }


        public void Init()
        {
            try
            {
                string strTmp;
               
                _strSaveDir = IniTool.Instance().INIRead("Setting", "SaveDir", _strIniFilePath);
                _strMySQLIP = IniTool.Instance().INIRead("Setting", "MySQLIP", _strIniFilePath);
                strTmp = IniTool.Instance().INIRead("Setting", "MySQLPort", _strIniFilePath);
                _nMySQLPort = int.Parse(strTmp);
                _strMySQLUser = IniTool.Instance().INIRead("Setting", "MySQLUser", _strIniFilePath);
                _strMySQLPwd = IniTool.Instance().INIRead("Setting", "MySQLPwd", _strIniFilePath);
                _strMySQLDBName = IniTool.Instance().INIRead("Setting", "MySQLDB", _strIniFilePath);

                _strLeftLaser3DIP = IniTool.Instance().INIRead("Setting", "LeftLaser3DIP", _strIniFilePath);
                _strRightLaser3DIP = IniTool.Instance().INIRead("Setting", "RightLaser3DIP", _strIniFilePath);
                _strTopLaser3DIP = IniTool.Instance().INIRead("Setting", "TopLaser3DIP", _strIniFilePath);
                _strDownLaser3DIP = IniTool.Instance().INIRead("Setting", "DownLaser3DIP", _strIniFilePath);
                _strTopLocalIP = IniTool.Instance().INIRead("Setting", "TopLocalIP", _strIniFilePath);
                _strLeftLocalIP = IniTool.Instance().INIRead("Setting", "LeftLocalIP", _strIniFilePath);
                _strRightLocalIP = IniTool.Instance().INIRead("Setting", "RightLocalIP", _strIniFilePath);
                _strDownLocalIP = IniTool.Instance().INIRead("Setting", "DownLocalIP", _strIniFilePath);

                StrIPMoveControl = IniTool.Instance().INIRead("Setting", "MoveControlIP", _strIniFilePath);
                strTmp = IniTool.Instance().INIRead("Setting", "MoveControlPort", _strIniFilePath);
                int.TryParse(strTmp, out _nMoveControlPort);
                //StrIPArmControl = IniTool.Instance().INIRead("Setting", "ArmControlIP", _strIniFilePath);
                //strTmp = IniTool.Instance().INIRead("Setting", "ArmControlPort", _strIniFilePath);
                //int.TryParse(strTmp, out _nArmControlPort);
                strTmp = IniTool.Instance().INIRead("Setting", "SaveBmp", _strIniFilePath);
                int.TryParse(strTmp, out _nSaveBmp);

                strTmp = IniTool.Instance().INIRead("Setting", "Daemon", _strIniFilePath);
                int.TryParse(strTmp, out _nDaemon);
                //_nMoveControlPort = int.Parse(strTmp);
                strTmp = IniTool.Instance().INIRead("Setting", "MoveType", _strIniFilePath);
                int.TryParse(strTmp, out _nMoveType);
                strTmp = IniTool.Instance().INIRead("Setting", "PLCMoveSpeed", _strIniFilePath);
                float.TryParse(strTmp, out _fPLCMoveSpeed);

                strTmp = IniTool.Instance().INIRead("Setting", "FirstPosition", _strIniFilePath);
                float.TryParse(strTmp, out _fPosition_fir);

                strTmp = IniTool.Instance().INIRead("Setting", "SecondPosition", _strIniFilePath);
                float.TryParse(strTmp, out _fPosition_sec);

                strTmp = IniTool.Instance().INIRead("Setting", "FirstPosition_s", _strIniFilePath);
                float.TryParse(strTmp, out _fPosition_fir_s);

                strTmp = IniTool.Instance().INIRead("Setting", "SecondPosition_s", _strIniFilePath);
                float.TryParse(strTmp, out _fPosition_sec_s);

                strTmp = IniTool.Instance().INIRead("Setting", "ThirdPosition", _strIniFilePath);
                float.TryParse(strTmp, out _fPosition_thr);

                strTmp = IniTool.Instance().INIRead("Setting", "SquareType", _strIniFilePath);
                int.TryParse(strTmp, out _nSquareType);

                strTmp = IniTool.Instance().INIRead("Setting", "LeftCom", _strIniFilePath);
                int.TryParse(strTmp, out _nLeftCom);

                strTmp = IniTool.Instance().INIRead("Setting", "RightCom", _strIniFilePath);
                int.TryParse(strTmp, out _nRightCom);

                strTmp = IniTool.Instance().INIRead("Setting", "TopCom", _strIniFilePath);
                int.TryParse(strTmp, out _nTopCom);


                strTmp = IniTool.Instance().INIRead("Setting", "SquareType", _strIniFilePath);
                int.TryParse(strTmp, out _nSquareType);

                strTmp = IniTool.Instance().INIRead("Setting", "CamType", _strIniFilePath);
                int.TryParse(strTmp, out _nCamType);


                LogHelper.Info("Silicon","Init  SSZN Left " + _strLeftLaser3DIP + " Right " + _strRightLaser3DIP + " Top " + _strTopLaser3DIP + " Down " + _strDownLaser3DIP + " MoveController " + StrIPMoveControl + " Port " + _nMoveControlPort.ToString() + " ArmController " + StrIPArmControl + " Arm Port " + _nArmControlPort.ToString()  + " SectionCount " + _nSectionCount.ToString() + " SiliconStickBegin " + _fSiliconStickBegin.ToString("0.00") + " SiliconStickTail " + _fSiliconStickTail.ToString("0.00") + " SiliconStickLineBegin " + " SiliconStickMinRaidus " + _fSiliconStickMinRaidus.ToString("0.00") + " SiliconStickMaxRaidus " + _fSiliconStickMaxRaidus.ToString("0.00") + " SSZNFrameHeight " + _nSSZNFrameHeight.ToString() + " StickLength " + _nStickLength.ToString() + " SaveDir " + _strSaveDir + " MySQLIP " + _strMySQLIP + " MySQLPort " + _nMySQLPort.ToString() + " MySQLUser " + _strMySQLUser + " MySQLPwd " + _strMySQLPwd + " MySQLDBName " + _strMySQLDBName + 
                    " nDaemon " + _nDaemon.ToString() + " Position_Fir " + _fPosition_fir.ToString() + " Position_Sec " + _fPosition_sec.ToString() + " Position_Thr " + _fPosition_thr.ToString() +  " IncreaseSpeed " + _fIncreaseSpeed.ToString("0.0") + " DecreaseSpeed " + " move_type " + _nMoveType.ToString() + " CurPositionRegister " + _nCurPositionRegister.ToString() +   " SetZeroType " + _nSetZeroType.ToString() + " MotionCardTailPosition " + _nMotionCardTailPosition.ToString() +
                      " MotionCardBeginPosition " + _nMotionCardBeginPosition.ToString() + " PLCIncreaseSpeed " + _fPLCIncreaseSpeed.ToString("0.0") + " PLCRotateIncreaseSpeed " + _fPLCRotateIncreaseSpeed.ToString("0.0") + " PLCMoveSpeed " + _fPLCMoveSpeed.ToString("0.0") + " SquareType " + _nSquareType.ToString() + " CamType " + NCamType.ToString());
            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon","SettingParameter Init " + ex.Message);
            }
          
        }

        public void SaveSubLength()
        {
            try
            {
                IniTool.Instance().INIWrite("Setting", "SetZeroType", _nSetZeroType.ToString(), _strIniFilePath);

                LogHelper.Info("Silicon", "SaveSubLength subPosition " + " SetZeroType " + _nSetZeroType.ToString());
            }
            catch(Exception ex)
            {
                LogHelper.Info("Silicon", "SaveSubLength Init " + ex.Message);
            }
        }

        public void Save()
        {
            try
            {
               
               
                IniTool.Instance().INIWrite("Setting", "MoveControlIP", StrIPMoveControl, _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "MoveControlPort", NMoveControlPort.ToString(), _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "ArmControlIP", StrIPArmControl, _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "ArmControlPort", NArmControlPort.ToString(), _strIniFilePath);

                IniTool.Instance().INIWrite("Setting", "SSZNFrameHeight", _nSSZNFrameHeight.ToString(), _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "SaveDir", _strSaveDir, _strIniFilePath );
                IniTool.Instance().INIWrite("Setting", "MySQLIP", _strMySQLIP, _strIniFilePath);
                IniTool.Instance().INIWrite("Setting",  "MySQLPort", _nMySQLPort.ToString(), _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "MySQLUser", _strMySQLUser, _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "MySQLPwd", _strMySQLPwd, _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "LeftLaser3DIP", _strLeftLaser3DIP, _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "RightLaser3DIP", _strRightLaser3DIP, _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "TopLaser3DIP", _strTopLaser3DIP, _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "DownLaser3DIP", _strDownLaser3DIP, _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "TopLocalIP", _strTopLocalIP, _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "LeftLocalIP", _strLeftLocalIP, _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "RightLocalIP", _strRightLocalIP, _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "DownLocalIP", _strDownLocalIP, _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "SaveBmp", _nSaveBmp.ToString(), _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "Daemon", _nDaemon.ToString(), _strIniFilePath);              
                IniTool.Instance().INIWrite("Setting", "MoveType", _nMoveType.ToString(), _strIniFilePath);                              
                IniTool.Instance().INIWrite("Setting", "PLCMoveSpeed", _fPLCMoveSpeed.ToString("0.0"), _strIniFilePath);
                IniTool.Instance().INIWrite("Setting", "CamType", _nCamType.ToString(), _strIniFilePath);

                LogHelper.Info("Silicon","Save  SSZN Left " + _strLeftLaser3DIP + " Right " + _strRightLaser3DIP + " Top " + _strTopLaser3DIP + " Down " + _strDownLaser3DIP + " MoveController " + StrIPMoveControl + " Port " + _nMoveControlPort.ToString() + " ArmController " + StrIPArmControl + " Arm Port " + _nArmControlPort.ToString() + " SiliconStickBegin " + _fSiliconStickBegin.ToString("0.00") + " fSiliconStickTail " + _fSiliconStickTail.ToString("0.00") + " SiliconStickLineBegin "  + " SSZNFrameHeight " + _nSSZNFrameHeight.ToString() +  " SaveDir " + _strSaveDir + " MySQLIP " + _strMySQLIP + " MySQLPort " + _nMySQLPort.ToString() + " MySQLUser " + _strMySQLUser + " MySQLPwd " + _strMySQLPwd + " nDaemon " + _nDaemon.ToString()  + " Position_Sec " + _fPosition_sec.ToString()  + " IncreaseSpeed " + _fIncreaseSpeed.ToString("0.0") + " DecreaseSpeed " +  " MoveType " + _nMoveType.ToString() + " CurPositionRegister " + _nCurPositionRegister.ToString() +   " SetZeroType " + _nSetZeroType.ToString() + " MotionCardSiliconLine " + " MotionCardTailPosition " + _nMotionCardTailPosition.ToString() + " MotionCardBeginPosition " + _nMotionCardBeginPosition.ToString() + " PLCIncreaseSpeed " + _fPLCIncreaseSpeed.ToString("0.0") + " PLCRotateIncreaseSpeed " + _fPLCRotateIncreaseSpeed.ToString("0.0") + " PLCMoveSpeed " + _fPLCMoveSpeed.ToString("0.0") + " SquareType " + _nSquareType.ToString() + " TopLocalIP " + _strTopLocalIP + " LeftLocalIP " + _strLeftLocalIP + " RightLocalIP  " + _strRightLocalIP + " DownLocalIP " + _strDownLocalIP);
            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon","SettingParameter Init " + ex.Message);
            }

        }
    }
}
