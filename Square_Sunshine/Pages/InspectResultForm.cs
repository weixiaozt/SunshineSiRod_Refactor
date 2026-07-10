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
using SquareSiliconStickCheck.Parameters;
using SquareSiliconStickCheck.Tools;
using Sunny.UI;


namespace SquareSiliconStickCheck.Pages
{
    public partial class InspectResultForm : UIForm
    {
        private InspectResult _curresult;

        public delegate void ShowRightButtonDelegate(bool bShow);

        public ShowRightButtonDelegate ShowRightBtnFunc;

        public static SettingParamPage instance;

        public InspectResult Curresult { get => _curresult; set => _curresult = value; }

        public InspectResultForm(InspectResult result)
        {
            _nNoCollapsedRadiusViewHeight = 850;
            _nCollapsedRadiusViewHeight = 50;
            _nSplitViewHeight = 20;

            _nBeginRaidusViewY = 40;
            _nRadiusViewWidth = 1700;
            _nRadiusViewPictureboxWidth = 1600;
            _nRadiusViewPictureboxHeight = 150;

            _curresult = result;
            InitializeComponent();
            InitResult();
            Init();

       
            this.panelRadiusView_Full.Collapsed = false;
            //this.panelRadiusView_YINLIE.Refresh();
            //this.pictureBoxYINLIE_1.Refresh();
            //this.pictureBoxYINLIE_2.Refresh();
            //this.pictureBoxYINLIE_3.Refresh();
            //this.pictureBoxYINLIE_4.Refresh();

            this.groupBox2.Refresh();

            //Refresh();
            // Invalidate();

            //timeRefresh = new Timer()
            //{
            //    Interval = 1000,
            //};

            //timeRefresh.Tick += TimeRefresh_Tick;
            //timeRefresh.Start();
        }

      

        private void Init()
        {
            try
            {
                
                if (File.Exists(_curresult.StrFileYinLie))
                {
                    Bitmap bmpYinLie_1 = new Bitmap(_curresult.StrFileYinLie);
                    pictureBoxYINLIE.Image = bmpYinLie_1;
                    pictureBoxYINLIE.PreImage = bmpYinLie_1;
                }

               

                if (File.Exists(_curresult.StrFileYingLi))
                {
                    Bitmap bmpYingLi_1 = new Bitmap(_curresult.StrFileYingLi);
                    pictureBoxYingLi.Image = bmpYingLi_1;
                    pictureBoxYingLi.PreImage = bmpYingLi_1;

                }

               
            }
            catch(Exception ex)
            {

            }
           
        }

        private void InitResult()
        {

            string strNGYingLiSubLength = "";
            string strNGYinLieSubLength = "";
            string strNGLJSubLength = "";
            string strNGRadisuSubLength = "";

           
            ArrayList[] subYingLiLengthInfo = new ArrayList[4];
            ArrayList[] subYinLieLengthInfo = new ArrayList[4];
           
            ArrayList subTotalRadiusLengthInfo = new ArrayList();

            ArrayList subTotalLJLengthInfo = new ArrayList();
            ArrayList subTotalYinLieLengthInfo = new ArrayList();
            ArrayList subTotalYingLiLengthInfo = new ArrayList();

           
           
            for (int i = 0; i < 4; i++)
            {
                subYingLiLengthInfo[i] = new ArrayList();
                subYinLieLengthInfo[i] = new ArrayList();
              
                try
                {
                   


                    string[] strYingLiLengths = strNGYingLiSubLength.Split(';');

                    for (int j = 0; j < strYingLiLengths.Length - 1; j++)
                    {
                        string[] strLengthInfo = strYingLiLengths[j].Split(',');
                        float[] fnSubLength = new float[2];
                        fnSubLength[0] = float.Parse(strLengthInfo[0]);
                        fnSubLength[1] = float.Parse(strLengthInfo[1]);

                       

                        subYingLiLengthInfo[i].Add(fnSubLength);
                        switch (i)
                        {
                            case 0:
                                {
                                   
                                    pictureBoxYingLi.AddYingLiSubArea(fnSubLength);
                                    break;
                                }
                            
                        }
                       
                    }

                    string[] strYinLieLengths = strNGYinLieSubLength.Split(';');

                    for (int j = 0; j < strYinLieLengths.Length - 1; j++)
                    {
                        string[] strLengthInfo = strYinLieLengths[j].Split(',');
                        float[] fnSubLength = new float[2];
                        fnSubLength[0] = float.Parse(strLengthInfo[0]);
                        fnSubLength[1] = float.Parse(strLengthInfo[1]);


                        subYinLieLengthInfo[i].Add(fnSubLength);
                        switch (i)
                        {
                            case 0:
                                {
                                 
                                    pictureBoxYINLIE.AddYinLieSubArea(fnSubLength);
                                    break;
                                }
                           
                        }
                    }

                    string[] strLJLengths = strNGLJSubLength.Split(';');

                 
                }
                catch (Exception e)
                {
                     LogHelper.Info("Silicon","InitResult exception " + e.Message);
                }
            }



         
        }

        private void pictureBoxYINLIE_1_Click(object sender, EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxYINLIE);
            pictureForm.ShowDialog();
        }

       
        private void pictureBoxYingLi_1_Click(object sender, EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxYingLi);
            pictureForm.ShowDialog();
        }



       

       
    }
}
