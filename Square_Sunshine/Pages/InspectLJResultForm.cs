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
using SiliconRoundBarCheck.Parameters;
using Sunny.UI;

namespace SiliconRoundBarCheck.Pages
{
    public partial class InspectLJResultForm : UIForm
    {
        private ArrayList[] _subLengthInfo = new ArrayList[4];
        private ArrayList[] _subPictureIndexInfo = new ArrayList[4];
        private InspectResult _curresult;

        public InspectResult Curresult { get => _curresult; set => _curresult = value; }

        public InspectLJResultForm(InspectResult result)
        {
            _curresult = result;
            InitializeComponent();
            Init();


        }

        private void Init()
        {
            try
            {
                if (File.Exists(_curresult.StrFileLJ_1))
                {
                    Bitmap bmpLJ_1 = new Bitmap(_curresult.StrFileLJ_1);
                    pictureBoxLJ_1.Image = bmpLJ_1;
                }

                if (File.Exists(_curresult.StrFileLJ_2))
                {
                    Bitmap bmpLJ_2 = new Bitmap(_curresult.StrFileLJ_2);
                    pictureBoxLJ_2.Image = bmpLJ_2;
                }

                if (File.Exists(_curresult.StrFileLJ_3))
                {
                    Bitmap bmpLJ_3 = new Bitmap(_curresult.StrFileLJ_3);
                    pictureBoxLJ_3.Image = bmpLJ_3;

                }

                if (File.Exists(_curresult.StrFileLJ_4))
                {
                    Bitmap bmpLJ_4 = new Bitmap(_curresult.StrFileLJ_4);
                    pictureBoxLJ_4.Image = bmpLJ_4;
                }

                string strNGSubLength = "";
                for (int i = 0; i < 4; i++)
                {
                    int nPictureWidth = 0;
                    int nImageWidth = 0;
                    switch (i)
                    {
                        case 0:
                            {
                                strNGSubLength = _curresult.StrNGSubLJLength_1;
                                nPictureWidth = pictureBoxLJ_1.Width;
                                nImageWidth = pictureBoxLJ_1.Image.Width;
                                break;
                            }
                        case 1:
                            {
                                strNGSubLength = _curresult.StrNGSubLJLength_2;
                                nPictureWidth = pictureBoxLJ_2.Width;
                                nImageWidth = pictureBoxLJ_2.Image.Width;
                                break;
                            }
                        case 2:
                            {
                                strNGSubLength = _curresult.StrNGSubLJLength_3;
                                nPictureWidth = pictureBoxLJ_3.Width;
                                nImageWidth = pictureBoxLJ_3.Image.Width;
                                break;
                            }
                        case 3:
                            {
                                strNGSubLength = _curresult.StrNGSubLJLength_4;
                                nPictureWidth = pictureBoxLJ_4.Width;
                                nImageWidth = pictureBoxLJ_4.Image.Width;
                                break;
                            }
                    }

                    string[] strLengths = strNGSubLength.Split(';');

                    for (int j = 0; j < strLengths.Length - 1; j++)
                    {
                        string[] strLengthInfo = strLengths[i].Split(',');
                        float[] fnSubLength = new float[2];
                        int[] nSubLength = new int[2];
                        fnSubLength[0] = float.Parse(strLengthInfo[0]);
                        fnSubLength[1] = float.Parse(strLengthInfo[1]);

                        nSubLength[0] = (int)(fnSubLength[0] * nPictureWidth / (float)nImageWidth);
                        nSubLength[1] = (int)(fnSubLength[1] * nPictureWidth / (float)nImageWidth);


                      

                        _subLengthInfo[i].Add(fnSubLength);
                        
                        _subPictureIndexInfo[i].Add(nSubLength);
                    }
                }
            }
            catch (Exception ex)
            {

            }
           
        }


        private void pictureBoxLJ_1_Click(object sender, EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxLJ_1);
            pictureForm.ShowDialog();
        }

        private void PictureBoxLJ_2_Click(object sender, System.EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxLJ_2);
            pictureForm.ShowDialog();
        }

        private void PictureBoxLJ_3_Click(object sender, System.EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxLJ_3);
            pictureForm.ShowDialog();
        }

        private void PictureBoxLJ_4_Click(object sender, System.EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxLJ_4);
            pictureForm.ShowDialog();
        }



    }
}
