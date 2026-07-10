using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquareSiliconStickCheck.Data
{
    internal class ResouceData
    {
        private float _Length;
        private float _A_Length;
        private float _B_Length;
        private float _Diagonal_TD;
        private float _Diagonal_RL;
        private float _A_Length_Sub;
        private float _B_Length_Sub;


        public float A_Length { get => _A_Length; set => _A_Length = value; }
        public float B_Length { get => _B_Length; set => _B_Length = value; }
        public float A_Length_Sub { get => _A_Length_Sub; set => _A_Length_Sub = value; }
        public float B_Length_Sub { get => _B_Length_Sub; set => _B_Length_Sub = value; }
       
    }
}
