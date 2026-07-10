using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using SiliconRoundBarCheck.Data;
using SquareSiliconStickCheck.Parameters;
using SquareSiliconStickCheck.Tools;
using Sunny.UI;


namespace SquareSiliconStickCheck.Pages
{
    public partial class InspectSquareResultNewForm : UIForm
    {
        private SquareStickCheckData _curresult;

        public SquareStickCheckData Curresult { get => _curresult; set => _curresult = value; }

        public InspectSquareResultNewForm(SquareStickCheckData result)
        {
            
            _curresult = result;

            List<SquareStickCheckData> listsq = CMySQLTool.Instance().SelectSquareStickResultBySerial(SettingParameter.Instance().StrMySQLDBName, "squarstickresult", result.StrJBSearial);

            if (listsq.Count > 0 )
            {
                _curresult = listsq[0];
            }
            //refreshViewFunc = UpdateView;
            InitializeComponent();
            InitResult();
            Init();

            
            
            //this.panelRadiusView_LJ.Refresh();
            //this.panelRadiusView_YINLIE.Refresh();
            //this.panelRadiusView_YingLi.Refresh();
          

            //Refresh();
            // Invalidate();
            //timeRefresh = new Timer()
            //{
            //    Interval = 1000,
            //};
            //timeRefresh.Tick += TimeRefresh_Tick;
            //timeRefresh.Start();
        }

        private void TimeRefresh_Tick(object sender, EventArgs e)
        {
            
        }

        private void Init()
        {
            try
            {
               


                
            }
            catch(Exception ex)
            {

            }
           
        }

        private void InitResult()
        {

               
            try
            {
                string strLengthInfo = "";


                strLengthInfo = _curresult.FLength.ToString("0.00") + ",";

                labelLength.Text = "棒长: " + strLengthInfo;

                string strLTLength = "";

                foreach (var item in _curresult.ListLTLength)
                {
                    strLTLength += ((float)item).ToString("0.00") + ",";
                }
                labelLTLength.Text = "A面边长：" + strLTLength;

                string strRTLength = "";
                foreach (var item in _curresult.ListRTLength)
                {
                    strRTLength += ((float)item).ToString("0.00") + ",";
                }
                labelRTLength.Text = "B面边长：" + strRTLength;


                string strLDLength = "";
                foreach (var item in _curresult.ListLDLength)
                {
                    strLDLength += ((float)item).ToString("0.00") + ",";
                }
                labelLDLength.Text = "C面边长：" + strLDLength;

                string strRDLength = "";
                foreach (var item in _curresult.ListRDLength)
                {
                    strRDLength += ((float)item).ToString("0.00") + ",";
                }
                labelRDLength.Text = "D面边长：" + strRDLength;


                string strTDLength = "";
                foreach (var item in _curresult.ListTDLength)
                {
                    strTDLength += ((float)item).ToString("0.00") + ",";
                }
                labelTDLength.Text = "上下对角线：" + strTDLength;

                string strLRLength = "";
                foreach (var item in _curresult.ListLRLength)
                {
                    strLRLength += ((float)item).ToString("0.00") + ",";
                }
                labelLRLength.Text = "左右对角线：" + strLRLength;


                string strTDiagLength = "";
                foreach (var item in _curresult.ListTopDiagLength)
                {
                    strTDiagLength += ((float)item).ToString("0.00") + ",";
                }
                labelTopDiag.Text = "上侧弧长：" + strTDiagLength;


                string strTLDiagLength = "";
                foreach (var item in _curresult.ListTopLeftDiagLength)
                {
                    strTLDiagLength += ((float)item).ToString("0.00") + ",";
                }
                labelTopLeftDiag.Text = "上侧弧左弧长投影：" + strTLDiagLength;

                string strTRDiagLength = "";
                foreach (var item in _curresult.ListTopRightDiagLength)
                {
                    strTRDiagLength += ((float)item).ToString("0.00") + ",";
                }
                labelTopRightDiag.Text = "上侧弧右弧长投影：" + strTDiagLength;

                string strLDiagLength = "";
                foreach (var item in _curresult.ListLeftDiagLength)
                {
                    strLDiagLength += ((float)item).ToString("0.00") + ",";
                }
                labelLeftDiag.Text = "左侧弧长：" + strLDiagLength;


                string strLLDiagLength = "";
                foreach (var item in _curresult.ListLeftLeftDiagLength)
                {
                    strLLDiagLength += ((float)item).ToString("0.00") + ",";
                }
                labelLeftLeftDiag.Text = "左侧弧左弧长投影：" + strLLDiagLength;

                string strLRDiagLength = "";
                foreach (var item in _curresult.ListLeftRightDiagLength)
                {
                    strLRDiagLength += ((float)item).ToString("0.00") + ",";
                }
                labelLeftRightDiag.Text = "左侧弧右弧长投影：" + strLRDiagLength;

                string strRDiagLength = "";
                foreach (var item in _curresult.ListRightDiagLength)
                {
                    strRDiagLength += ((float)item).ToString("0.00") + ",";
                }
                labelRightDiag.Text = "右侧弧长：" + strRDiagLength;


                string strRLDiagLength = "";
                foreach (var item in _curresult.ListRightLeftDiagLength)
                {
                    strRLDiagLength += ((float)item).ToString("0.00") + ",";
                }
                labelRightLeftDiag.Text = "左侧弧左弧长投影：" + strRLDiagLength;

                string strRRDiagLength = "";
                foreach (var item in _curresult.ListRightRightDiagLength)
                {
                    strRRDiagLength += ((float)item).ToString("0.00") + ",";
                }
                labelRightRightDiag.Text = "左侧弧右弧长投影：" + strRRDiagLength;

                string strDDiagLength = "";
                foreach (var item in _curresult.ListDownDiagLength)
                {
                    strDDiagLength += ((float)item).ToString("0.00") + ",";
                }
                labelDownDiag.Text = "下侧弧长：" + strDDiagLength;


                string strDLDiagLength = "";
                foreach (var item in _curresult.ListDownLeftDiagLength)
                {
                    strDLDiagLength += ((float)item).ToString("0.00") + ",";
                }
                labelDownLeftDiag.Text = "左侧弧左弧长投影：" + strDLDiagLength;

                string strDRDiagLength = "";
                foreach (var item in _curresult.ListDownRightDiagLength)
                {
                    strDRDiagLength += ((float)item).ToString("0.00") + ",";
                }
                labelDownRightDiag.Text = "左侧弧右弧长投影：" + strDRDiagLength;

                string strTopAngle = "";
                foreach (var item in _curresult.ListLeftAngle)
                {
                    strTopAngle += ((float)item).ToString("0.000") + ",";
                }
                labelTopAngle.Text = "上侧直边角度：" + strTopAngle;


                string strLeftAngle = "";
                foreach (var item in _curresult.ListLeftAngle)
                {
                    strLeftAngle += ((float)item).ToString("0.000") + ",";
                }
                labelLeftAngle.Text = "左侧直边角度：" + strLeftAngle;

                string strRightAngle = "";
                foreach (var item in _curresult.ListRightAngle)
                {
                    strRightAngle += ((float)item).ToString("0.000") + ",";
                }
                labelRightAngle.Text = "右侧直边角度：" + strRightAngle;

                string strDownAngle = "";
                foreach (var item in _curresult.ListDownAngle)
                {
                    strDownAngle += ((float)item).ToString("0.000") + ",";
                }
                labelDownAngle.Text = "下侧直边角度：" + strDownAngle;

            }
            catch (Exception e)
            {
                LogHelper.Info("Silicon","InitResult exception " + e.Message);
            }
            


         
        }



       


      
      
    }
}
