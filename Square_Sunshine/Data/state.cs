using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquareSiliconStickCheck.Data
{
    internal class GlobalDatastate
    {
        private static GlobalDatastate _instance;
        public static GlobalDatastate Instance()
        {
            if (_instance == null)
            {
                _instance = new GlobalDatastate();
            }
            return _instance;
        }

        private string _stateUpdate;

        private string _sernum;
        private bool _timstart=false;


        public bool timstart { get => _timstart; set => _timstart = value; }

        public string sernum { get => _sernum; set => _sernum = value; }
        public string stateUpdate { get => _stateUpdate; set => _stateUpdate = value; }

       
       
     

    }
}
