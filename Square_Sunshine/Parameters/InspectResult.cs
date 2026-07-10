using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquareSiliconStickCheck.Parameters
{
    public class InspectResult
    {
        private int _nID;
        private int _nResult;
        private DateTime _curcheck;
       
        private string _strFileYinLie;
       
        private string _strFileYingLi;
    

        public InspectResult() 
        {  
           
           
        }


        public enum emDataType
        {
            EM_ID = 0,
            EM_RESULT = 1,
            EM_FILEYINLIE = 2,
            EM_FILEYINGLI = 3,
            EM_CHECKTIME = 4
        }

        public int NResult { get => _nResult; set => _nResult = value; }
        public DateTime Curcheck { get => _curcheck; set => _curcheck = value; }
      
        public string StrFileYinLie { get => _strFileYinLie; set => _strFileYinLie = value; }
      
        public string StrFileYingLi { get => _strFileYingLi; set => _strFileYingLi = value; }
       
        public int NID { get => _nID; set => _nID = value; }
       
    }
}
