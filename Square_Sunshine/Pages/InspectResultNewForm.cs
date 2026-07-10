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
using SquareSiliconStickCheck.Parameters;
using SquareSiliconStickCheck.Tools;
using Sunny.UI;


namespace SquareSiliconStickCheck.Pages
{
    public partial class InspectResultNewForm : UIForm
    {
        private InspectResult _curresult;

        public InspectResult Curresult { get => _curresult; set => _curresult = value; }

        public InspectResultNewForm(InspectResult result)
        {
            _nViewBeginY = 50;
            _nSplitViewWidth = 10;
            _nRadiusViewPictureboxWidth = 150;
            _nRadiusViewPictureboxHeight = 850;
            _nNoCollapsedRadiusViewHeight = 950;
            _nNoCollapsedRadiusViewWidth = 650;
            _nCollapsedRadiusViewWidth = 50;
            _nRaidusViewHeight = 950;
            _curresult = result;
            refreshViewFunc = UpdateView;
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
               

                if (File.Exists(_curresult.StrFileYinLie))
                {
                    Mat matYL = Cv2.ImRead(_curresult.StrFileYinLie);
                    Mat matFinal = new Mat();

                    Cv2.Rotate(matYL, matFinal, RotateFlags.Rotate90Clockwise);
                    Bitmap bmpYinLie_1 = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(matFinal);
                    //Bitmap bmpYinLie_1 = new Bitmap(_curresult.StrFileYinLie_1);
                    pictureBoxYINLIE_1.Image = bmpYinLie_1;
                    pictureBoxYINLIE_1.PreImage = bmpYinLie_1;
                }

                

                if (File.Exists(_curresult.StrFileYingLi))
                {
                    Mat matYingLi = Cv2.ImRead(_curresult.StrFileYingLi);
                    Mat matFinal = new Mat();

                    Cv2.Rotate(matYingLi, matFinal, RotateFlags.Rotate90Clockwise);
                    Bitmap bmpYingLi_1 = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(matFinal);


                    //Bitmap bmpYingLi_1 = new Bitmap(_curresult.StrFileYingLi_1);
                    pictureBoxYingLi_1.Image = bmpYingLi_1;
                    pictureBoxYingLi_1.PreImage = bmpYingLi_1;

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
            ArrayList[] subLJLengthInfo = new ArrayList[4];
            ArrayList subTotalRadiusLengthInfo = new ArrayList();
            ArrayList subTotalLJLengthInfo = new ArrayList();
            ArrayList subTotalYinLieLengthInfo = new ArrayList();
            ArrayList subTotalYingLiLengthInfo = new ArrayList();

           
           
            for (int i = 0; i < 4; i++)
            {
                subYingLiLengthInfo[i] = new ArrayList();
                subYinLieLengthInfo[i] = new ArrayList();
                subLJLengthInfo[i] = new ArrayList();
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
                                   
                                    pictureBoxYingLi_1.AddYingLiSubArea(fnSubLength);
                                    break;
                                }
                            case 1:
                                {
                                   
                                    pictureBoxYingLi_2.AddYingLiSubArea(fnSubLength);
                                    break;
                                }
                            case 2:
                                {
                                   
                                    pictureBoxYingLi_3.AddYingLiSubArea(fnSubLength);
                                    break;
                                }
                            case 3:
                                {
                                  
                                    pictureBoxYingLi_4.AddYingLiSubArea(fnSubLength);
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
                                    pictureBoxYINLIE_1.AddYinLieSubArea(fnSubLength);
                                    break;
                                }
                            case 1:
                                {
                                   
                                    pictureBoxYINLIE_2.AddYinLieSubArea(fnSubLength);
                                    break;
                                }
                            case 2:
                                {
                                  
                                    pictureBoxYINLIE_3.AddYinLieSubArea(fnSubLength);
                                    break;
                                }
                            case 3:
                                {
                                   
                                    pictureBoxYINLIE_4.AddYinLieSubArea(fnSubLength);
                                    break;
                                }
                        }
                    }

                    string[] strLJLengths = strNGLJSubLength.Split(';');

                    for (int j = 0; j < strLJLengths.Length - 1; j++)
                    {
                        string[] strLengthInfo = strLJLengths[j].Split(',');
                        float[] fnSubLength = new float[2];
                        int[] nSubLength = new int[2];
                        fnSubLength[0] = float.Parse(strLengthInfo[0]);
                        fnSubLength[1] = float.Parse(strLengthInfo[1]);

                        subLJLengthInfo[i].Add(fnSubLength);
                        switch(i)
                        {
                            case 0:
                                {
                                    pictureBoxLJ_1.AddLJSubArea(fnSubLength);
                                    break;
                                }
                            case 1:
                                {
                                    pictureBoxLJ_2.AddLJSubArea(fnSubLength);
                                    break;
                                }
                            case 2:
                                {
                                    pictureBoxLJ_3.AddLJSubArea(fnSubLength);
                                    break;
                                }
                            case 3:
                                {
                                    pictureBoxLJ_4.AddLJSubArea(fnSubLength);
                                    break;
                                }

                        }

                    }

                   
                }
                catch (Exception e)
                {
                     LogHelper.Info("Silicon","InitResult exception " + e.Message);
                }
            }



         
        }

        private void pictureBoxYINLIE_1_Click(object sender, EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxYINLIE_1);
            pictureForm.ShowDialog();
        }

        private void pictureBoxYINLIE_2_Click(object sender, EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxYINLIE_2);
            pictureForm.ShowDialog();
        }

        private void PictureBoxYINLIE_3_Click(object sender, System.EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxYINLIE_3);
            pictureForm.ShowDialog();
        }

        private void PictureBoxYINLIE_4_Click(object sender, System.EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxYINLIE_4);
            pictureForm.ShowDialog();
        }

        private void pictureBoxYingLi_1_Click(object sender, EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxYingLi_1);
            pictureForm.ShowDialog();
        }



        private void PictureBoxYingLi_2_Click(object sender, System.EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxYingLi_2);
            pictureForm.ShowDialog();
        }

        private void PictureBoxYingLi_3_Click(object sender, System.EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxYingLi_3);
            pictureForm.ShowDialog();
        }

        private void PictureBoxYingLi_4_Click(object sender, System.EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxYingLi_4);
            pictureForm.ShowDialog();
        }





        private void pictureBoxLJ_1_Click(object sender, EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxLJ_1);
            pictureForm.ShowDialog();
        }

      
      
    }
}
