using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconRoundBarCheck.Data
{
    public class SquareStickCheckCompResult
    {
        //最小左上边长
        public float FMinLTLength    { get; set; }
        //最大左上边长
        public float FMaxLTLength    { get; set; }
        //最小右上边长
        public float FMinRTLength    { get; set; }
        //最大右上边长
        public float FMaxRTLength    { get; set; }
        //最小对角线边长
        public float FMinLRTDLength    { get; set; }
        //最大对角线边长
        public float FMaxLRTDLength { get; set; }
      
        //最小弧长
        public float FMinDiagLength  { get; set; }
        //最大弧长
        public float FMaxDiagLength  { get; set; }
        //最小弧长投影
        public float FMinDiagShadowLength { get; set; }
        public float FMaxDiagShadowLength { get; set; }
       

        public SquareStickCheckCompResult(float fMinLTLength, float fMaxLTLength, float fMinRTLength, float fMaxRTLength, float fMinLRTDLength, float fMaxLRTDLength, float fMinDiagLength, float fMaxDiagLength, float fMinDiagShadowLength, float fMaxDiagShadowLength)
        {
            FMinLTLength = fMinLTLength;
            FMaxLTLength = fMaxLTLength;
            FMinRTLength = fMinRTLength;  
            FMaxRTLength = fMaxRTLength;
            FMinLRTDLength = fMinLRTDLength;
            FMaxLRTDLength = fMaxLRTDLength;
            FMinDiagLength = fMinDiagLength;
            FMaxDiagLength = fMaxDiagLength;
            FMinDiagShadowLength = fMinDiagShadowLength;
            FMaxDiagShadowLength = fMaxDiagShadowLength;    
        }
    }
}
