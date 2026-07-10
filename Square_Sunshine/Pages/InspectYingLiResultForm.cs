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
    public partial class InspectYingLiResultForm : UIForm
    {
        private ArrayList[] _subLengthInfo = new ArrayList[4];
        private ArrayList[] _subPictureIndexInfo = new ArrayList[4];
        private InspectResult _curresult;

        public InspectResult Curresult { get => _curresult; set => _curresult = value; }

        public InspectYingLiResultForm(InspectResult result)
        {
            _curresult = result;
            InitializeComponent();
            Init();


        }

        private void Init()
        {
            try
            {
                if (File.Exists(_curresult.StrFileYingLi))
                {
                    Bitmap bmpYingLi_1 = new Bitmap(_curresult.StrFileYingLi);
                    pictureBoxYingLi_1.Image = bmpYingLi_1;
                }

              


            }
            catch (Exception ex)
            {

            }
           
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



    }
}
