using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconRoundBarCheck.Parameters
{
    public class InspectResultLineInfo
    {
        private string _strSiliconStickNum;
        private float _fApperanceLength;
        private float _fAppearanceValidLength;
        private float _fAppearanceMaxRadius;
        private float _fAppearanceMinRadius;
        private string _strResultPath = "";
        private int _nSiliconLineNum;
        private string _strAbnormalFir;
        private string _strAbnormalSec;
        private string _strAbnormalThr;
        private string _strAbnormalFour;
        private string _strDrawLineInfo;
        private DateTime _curDate;
        public enum emDataType
        {
            EM_SILICONSTICKNUM = 0,
            EM_APPEARANCELENGTH = 1,
            EM_APPEARANCEVALIDLENGTH = 2,
            EM_APPEARANCEMAXRADIUS = 3,
            EM_APPEARANCEMINRADIUS = 4,
            EM_SILICONLINENUM = 5,
            EM_ABNORMALAREAFIR = 6,
            EM_ABNORMALAREASEC = 7,
            EM_ABNORMALAREATHR = 8,
            EM_ABNORMALAREAFOUR = 9,
            EM_DRAWLINE_1= 10,
            EM_DRAWLINE_2 = 11,
            EM_DRAWLINE_3 = 12,
            EM_DRAWLINE_4 = 13,
            EM_DRAWLINE_5 = 14,
            EM_DRAWLINE_6 = 15,
            EM_DRAWLINE_7 = 16,
            EM_DRAWLINE_8 = 17,
            EM_DRAWLINE_9 = 18,
            EM_DRAWLINE_10 = 19,
            EM_DRAWLINE_11 = 20,
            EM_DRAWLINE_12 = 21,
            EM_RESULTPATH = 22,

        }


        public string StrSiliconStickNum { get => _strSiliconStickNum; set => _strSiliconStickNum = value; }
        public float FApperanceLength { get => _fApperanceLength; set => _fApperanceLength = value; }
        public float FAppearanceValidLength { get => _fAppearanceValidLength; set => _fAppearanceValidLength = value; }
        public float FAppearanceMaxRadius { get => _fAppearanceMaxRadius; set => _fAppearanceMaxRadius = value; }
        public int NSiliconLineNum { get => _nSiliconLineNum; set => _nSiliconLineNum = value; }
        public string StrAbnormalFir { get => _strAbnormalFir; set => _strAbnormalFir = value; }
        public string StrAbnormalSec { get => _strAbnormalSec; set => _strAbnormalSec = value; }
        public string StrAbnormalThr { get => _strAbnormalThr; set => _strAbnormalThr = value; }
        public string StrAbnormalFour { get => _strAbnormalFour; set => _strAbnormalFour = value; }
        public string StrDrawLineInfo { get => _strDrawLineInfo; set => _strDrawLineInfo = value; }
        public float FAppearanceMinRadius { get => _fAppearanceMinRadius; set => _fAppearanceMinRadius = value; }
        public DateTime CurDate { get => _curDate; set => _curDate = value; }
        public string StrResultPath { get => _strResultPath; set => _strResultPath = value; }
    }
}
