using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconRoundBarCheck.Data
{
    [Serializable]
    public class ReferencePosInfoData
    {
        
        public HTuple Hv_fPreRTHeight { get; set; }
        public HTuple Hv_fPreRTWidth { get; set; }
        public HTuple Hv_fPreLTHeight { get; set; }
        public HTuple Hv_fPreLTWidth { get; set; }
        public HTuple Hv_meanRefLeftTopIndex { get; set; }
        public HTuple Hv_meanRefRightTopIndex { get; set; }
        public HTuple Hv_meanRefLeftDownIndex { get; set; }
        public HTuple Hv_meanRefRightLeftIndex { get; set; }

        public HTuple Hv_meanRefRightDownIndex { get; set; }
        public HTuple Hv_meanRefLeftLeftIndex { get; set; }
        public HTuple Hv_meanRefRightRightIndex { get; set; }
        public HTuple Hv_meanRefLeftRightIndex { get; set; }
       




        public EMT_TYPE eMT_TYPE { get; set; }
        public HTuple Hv_RefmeanHeightTopM { get; set; }

        public HTuple Hv_RefmeanHeightTopR { get; set; }

        public HTuple Hv_RefmeanHeightTopL { get; set; }
     
        public HTuple Hv_RefmeanHeightLeftM { get; set; }
        public HTuple Hv_RefmeanHeightLeftR { get; set; }

        public HTuple Hv_RefmeanHeightLeftL { get; set; }
        public HTuple Hv_RefmeanHeightRightM { get; set; }

        public HTuple Hv_RefmeanHeightRightL { get; set; }
        
        public HTuple Hv_RefmeanHeightRightR {  get; set; }
        public HTuple Hv_RefmeanHeightDownR {  get; set; }


  
        public HTuple Hv_RefmeanHeightDownL { get; set; }
        public HTuple Hv_RefmeanHeightDownM { get; set; }

        public HTuple Hv_RefRatioAngleT { get; set; }

        public HTuple Hv_RefRatioAngleD { get; set; }

        public HTuple Hv_RefRatioAngleL { get; set; }

        public HTuple Hv_RefRatioAngleR { get; set; }

        public HTuple hv_fPreLRLength { get; set; }

        public HTuple hv_fPreTDLength { get; set; }
        public int nSquareSiliconType { get; set; } // 加工型号

        public enum EMT_TYPE
        {
            EMT_TYPE_182_ONE = 0, //两边182，对角线 247   12.07mm 
            EMT_TYPE_182_TWO = 1, //一边182， 一边 183，对角线247   10.88mm
            EMT_TYPE_182_THREE = 2, //一边182， 一边 183，对角线256 2.76mm
            EMT_TYPE_OTHER = 3, //其他
        }
    }
}
