using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SiliconRoundBarCheck.Tools;

namespace SiliconRoundBarCheck.Parameter
{
    internal class SettingParameter
    {
        private string _strIPVideo;
        private string _strIPIR;
        private string _strIPYL;
        private string _strIPLJ;
        private string _strIPRadius;
        private string _strIPMoveControl;

        private string _strIniFilePath;

        private static SettingParameter _instance;
        public string StrIPVideo { get => _strIPVideo; set => _strIPVideo = value; }
        public string StrIPIR { get => _strIPIR; set => _strIPIR = value; }
        public string StrIPYL { get => _strIPYL; set => _strIPYL = value; }
        public string StrIPLJ { get => _strIPLJ; set => _strIPLJ = value; }
        public string StrIPRadius { get => _strIPRadius; set => _strIPRadius = value; }

        public string StrIPMoveControl { get => _strIPMoveControl; set => _strIPMoveControl = value; }
        public string StrIniFile { get => _strIniFilePath; set => _strIniFilePath = value; }

        private SettingParameter()
        {
             _strIniFilePath = Convert.ToString(System.AppDomain.CurrentDomain.BaseDirectory) + "Config.ini";
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
            _strIPVideo = IniTool.Instance().INIRead("Setting", "VideoIP", _strIniFilePath);
            _strIPIR = IniTool.Instance().INIRead("Setting", "IRIP", _strIniFilePath);
            _strIPYL = IniTool.Instance().INIRead("Setting", "YLIP", _strIniFilePath);
            _strIPLJ = IniTool.Instance().INIRead("Setting", "LJIP", _strIniFilePath);
            _strIPRadius = IniTool.Instance().INIRead("Setting", "RadiusIP", _strIniFilePath);
            _strIPMoveControl = IniTool.Instance().INIRead("Setting", "MoveControlIP", _strIniFilePath);
        }

        public void Save()
        {
            IniTool.Instance().INIWrite("Setting", "VideoIP", _strIPVideo, _strIniFilePath);
            IniTool.Instance().INIWrite("Setting", "IRIP", _strIPIR, _strIniFilePath);
            IniTool.Instance().INIWrite("Setting", "YLIP", _strIPYL, _strIniFilePath);
            IniTool.Instance().INIWrite("Setting", "LJIP", _strIPLJ, _strIniFilePath);
            IniTool.Instance().INIWrite("Setting", "RadiusIP", _strIPRadius, _strIniFilePath);
            IniTool.Instance().INIWrite("Setting", "MoveControlIP", _strIPMoveControl, _strIniFilePath);

        }
    }
}
