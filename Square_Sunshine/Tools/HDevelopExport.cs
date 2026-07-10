using HalconDotNet;
using SiliconRoundBarCheck.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SquareSiliconStickCheck.Tools
{
    public class HDevelopExport
    {
        
        private static HDevelopExport _instance;

        public static HDevelopExport Instance()
        {
            if (_instance == null)
            {
                _instance = new HDevelopExport();
            }

            return _instance;
        }

        #region 算法
        static HDevProgram Program = new HDevProgram(".\\Square sticks_Measure.hdev");

        static HDevProcedure DetectionInit = new HDevProcedure(Program, "Detection_init");
        static HDevProcedureCall InitCall = new HDevProcedureCall(DetectionInit);
        public void Detection_init(HTuple ParmDict, out HTuple ResourceDictHandle, out HTuple JSONData, out HTuple Ex)
        {
            InitCall.SetInputCtrlParamTuple("ParmDict", ParmDict);
            InitCall.Execute();
            ResourceDictHandle = InitCall.GetOutputCtrlParamTuple("ResourceDictHandle");
            JSONData = InitCall.GetOutputCtrlParamTuple("JSONData");
            Ex = InitCall.GetOutputCtrlParamTuple("Ex");
        }

        static HDevProcedure DetectionProcess = new HDevProcedure(Program, "Detection_process");
        static HDevProcedureCall ProcessCall = new HDevProcedureCall(DetectionProcess);

        public void Detection_process(HObject Image, out HObject SaveImage, out HObject DispImage,
            HTuple IDTime, HTuple ParmDict, HTuple ResourceDictHandle, out HTuple AllQuality,
            out HTuple OutParmDict, out HTuple Ex)
        {

            ProcessCall.SetInputCtrlParamTuple("ParmDict", ParmDict);
            ProcessCall.SetInputCtrlParamTuple("ResourceDictHandle", ResourceDictHandle);
            ProcessCall.SetInputCtrlParamTuple("IDTime", IDTime);
            ProcessCall.SetInputIconicParamObject("Image", Image);
            ProcessCall.Execute();
            SaveImage = ProcessCall.GetOutputIconicParamImage("SaveImage");
            AllQuality = ProcessCall.GetOutputCtrlParamTuple("AllQuality");


            DispImage = ProcessCall.GetOutputIconicParamImage("DispImage");
            OutParmDict = ProcessCall.GetOutputCtrlParamTuple("OutParmDict");
            Ex = ProcessCall.GetOutputCtrlParamTuple("Ex");
        }

        //static HDevProcedure _test = new HDevProcedure(Program, "test");
        //static HDevProcedureCall testCall = new HDevProcedureCall(_test);
        //public void test( out HTuple test)
        //{
        //    testCall.Execute();
        //    test = testCall.GetOutputCtrlParamTuple("test");
        
        //}

        public void HTupToFt(HTuple htuple, out float foalt)
        {

            Double val = htuple.D;
            foalt = (float)val;
        }
        public void HTupToDb(HTuple htuple, out double foalt)
        {

            Double val = htuple.D;
            foalt = (double)val;
        }
        #endregion


        #region 标定
        static HDevProgram Program_bd = new HDevProgram(".\\Calibration.hdev");
        static HDevProcedure b_d = new HDevProcedure(Program_bd, "BD");
        static HDevProcedureCall BDCall = new HDevProcedureCall(b_d);
        public void Calibration(HObject Image, out HTuple DictHandle2)
        {


            BDCall.SetInputIconicParamObject("Image", Image);
            BDCall.Execute();
  
            DictHandle2 = BDCall.GetOutputCtrlParamTuple("DictHandle2");
   
        }


        #endregion
    }


}
