using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconRoundBarCheck.Data
{
    public class SquareStickCheckData
    {
      
        public enum emDataType
        {
            EM_SerialNum = 0,
            EM_LTLength = 1,
            EM_RTLength = 2,
            EM_LDLength = 3,
            EM_RDLength = 4,
            EM_TDLength = 5,
            EM_LRLength = 6,
            EM_TopDiag = 7,
            EM_LeftDiag = 8,
            EM_RightDiag = 9,
            EM_DownDiag = 10,
            EM_TopLeftDiag = 11,
            EM_TopRightDiag = 12,
            EM_LeftLeftDiag = 13,
            EM_LeftRightDiag = 14,
            EM_RightLeftDiag = 15,
            EM_RightRightDiag = 16,
            EM_DownLeftDiag = 17,
            EM_DownRightDiag = 18,
            EM_TopAngle = 19,
            EM_LeftAngle = 20,
            EM_RightAngle = 21,
            EM_DownAngle = 22,
            EM_Result = 23,
            EM_length = 24,
            EM_checktime = 25,
        }

        
        private string _strJBSearial;

        private float _fLength;

        private ArrayList _listLTLength;

        private ArrayList _listRTLength;

        private ArrayList _listLDLength;

        private ArrayList _listRDLength;

        private ArrayList _listLRLength;

        private ArrayList _listTDLength;

        private ArrayList _listTopDiagLength;

        private ArrayList _listDownDiagLength;

        private ArrayList _listLeftDiagLength;

        private ArrayList _listRightDiagLength;

        private ArrayList _listTopLeftDiagLength;

        private ArrayList _listTopRightDiagLength;

        private ArrayList _listDownLeftDiagLength;

        private ArrayList _listDownRightDiagLength;

        private ArrayList _listLeftLeftDiagLength;

        private ArrayList _listLeftRightDiagLength;

        private ArrayList _listRightLeftDiagLength;

        private ArrayList _listRightRightDiagLength;

        private ArrayList _listTopAngle;

        private ArrayList _listDownAngle;

        private ArrayList _listLeftAngle;

        private ArrayList _listRightAngle;

        private string _nResult;
        private int _nSquareType;
        private int _mnun;
        private int _mnum;
        private float _SVer;
        private float _EVer;
        public string StrJBSearial { get => _strJBSearial; set => _strJBSearial = value; }

        public ArrayList ListTopAngle { get => _listTopAngle; set => _listTopAngle = value; }
        public ArrayList ListDownAngle { get => _listDownAngle; set => _listDownAngle = value; }
        public ArrayList ListLeftAngle { get => _listLeftAngle; set => _listLeftAngle = value; }
        public ArrayList ListRightAngle { get => _listRightAngle; set => _listRightAngle = value; }
        public ArrayList ListLTLength { get => _listLTLength; set => _listLTLength = value; }
        public ArrayList ListRTLength { get => _listRTLength; set => _listRTLength = value; }
        public ArrayList ListLDLength { get => _listLDLength; set => _listLDLength = value; }
        public ArrayList ListRDLength { get => _listRDLength; set => _listRDLength = value; }
        public ArrayList ListLRLength { get => _listLRLength; set => _listLRLength = value; }
        public ArrayList ListTDLength { get => _listTDLength; set => _listTDLength = value; }
        public ArrayList ListTopDiagLength { get => _listTopDiagLength; set => _listTopDiagLength = value; }
        public ArrayList ListDownDiagLength { get => _listDownDiagLength; set => _listDownDiagLength = value; }
        public ArrayList ListLeftDiagLength { get => _listLeftDiagLength; set => _listLeftDiagLength = value; }
        public ArrayList ListRightDiagLength { get => _listRightDiagLength; set => _listRightDiagLength = value; }
        public ArrayList ListTopLeftDiagLength { get => _listTopLeftDiagLength; set => _listTopLeftDiagLength = value; }
        public ArrayList ListTopRightDiagLength { get => _listTopRightDiagLength; set => _listTopRightDiagLength = value; }
        public ArrayList ListDownLeftDiagLength { get => _listDownLeftDiagLength; set => _listDownLeftDiagLength = value; }
        public ArrayList ListDownRightDiagLength { get => _listDownRightDiagLength; set => _listDownRightDiagLength = value; }
        public ArrayList ListLeftLeftDiagLength { get => _listLeftLeftDiagLength; set => _listLeftLeftDiagLength = value; }
        public ArrayList ListLeftRightDiagLength { get => _listLeftRightDiagLength; set => _listLeftRightDiagLength = value; }
        public ArrayList ListRightLeftDiagLength { get => _listRightLeftDiagLength; set => _listRightLeftDiagLength = value; }
        public ArrayList ListRightRightDiagLength { get => _listRightRightDiagLength; set => _listRightRightDiagLength = value; }
        public float FLength { get => _fLength; set => _fLength = value; }
        public float SVer { get => _SVer; set => _SVer = value; }
        public float EVer { get => _EVer; set => _EVer = value; }
        public string NResult { get => _nResult; set => _nResult = value; }
        public int NSquareType { get => _nSquareType; set=> _nSquareType= value; }
        public int Mnum { get => _mnun; set => _mnun = value; }
        public SquareStickCheckData()
        {
            _listLTLength = new ArrayList();
            _listRTLength = new ArrayList();
            _listLDLength = new ArrayList();
            _listRDLength = new ArrayList();
            _listLRLength = new ArrayList();
            _listTDLength = new ArrayList();
            _listTopDiagLength = new ArrayList();
            _listDownDiagLength = new ArrayList();
            _listLeftDiagLength = new ArrayList();
            _listRightDiagLength = new ArrayList();
            _listTopLeftDiagLength = new ArrayList();
            _listTopRightDiagLength = new ArrayList();
            _listDownLeftDiagLength = new ArrayList();
            _listDownRightDiagLength = new ArrayList();
            _listLeftLeftDiagLength = new ArrayList();
            _listLeftRightDiagLength = new ArrayList();
            _listRightLeftDiagLength = new ArrayList();
            _listRightRightDiagLength = new ArrayList();
            _listTopAngle = new ArrayList();
            _listDownAngle = new ArrayList();
            _listLeftAngle = new ArrayList();
            _listRightAngle = new ArrayList();
            _strJBSearial = "123";
            _nResult = "";
            _nSquareType=0;
            _mnun = 0;
        }

    }
}
