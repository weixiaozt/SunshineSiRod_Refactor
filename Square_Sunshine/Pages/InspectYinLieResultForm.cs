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
using LiveCharts.Wpf;
using SiliconRoundBarCheck.Parameters;
using Sunny.UI;

namespace SiliconRoundBarCheck.Pages
{
    public partial class InspectYinLieResultForm : UIForm
    {
        private ArrayList[] _subLengthInfo = new ArrayList[4];
        private ArrayList[] _subPictureIndexInfo = new ArrayList[4];
        private InspectResult _curresult;

        public InspectResult Curresult { get => _curresult; set => _curresult = value; }

        public InspectYinLieResultForm(InspectResult result)
        {
            _curresult = result;
            InitializeComponent();
            Init();

        }

        private void Init()
        {
            try
            {
                

                if (File.Exists(_curresult.StrFileYinLie))
                {
                    Bitmap bmpYinLie_1 = new Bitmap(_curresult.StrFileYinLie);
                    pictureBoxYinLie_1.Image = bmpYinLie_1;
                }

               

             

            }
            catch(Exception ex)
            {

            }
           
        }

        private void PictureBoxYinLie_2_MouseUp(object sender, MouseEventArgs e)
        {
           if (_bDragging == true)
            {
                _bDragging = false;
            }
        }

        private void PictureBoxYinLie_3_MouseMove(object sender, MouseEventArgs e)
        {
            if (_bDragging)
            {
                int nIndex = _nDragLineIndex / 2;

                ArrayList arrInfo = _subPictureIndexInfo[2];
                int[] nArray = (int[])arrInfo[nIndex];
                if (_nDragLineIndex % 2 == 0)
                {
                    int nSub = nArray[1] - nArray[0];
                    nArray[0] = e.X;
                    nArray[1] = nSub + e.X;
                }
                else
                {
                    nArray[1] = e.X;
                }

                pictureBoxYinLie_3.Invalidate();
            }
        }

        private void PictureBoxYinLie_3_MouseDown(object sender, MouseEventArgs e)
        {
            Image b = pictureBoxYinLie_3.Image;
            int x = b.Width * e.X / pictureBoxYinLie_3.Width;
            int y = b.Height * e.Y / pictureBoxYinLie_3.Height;

            ArrayList arrInfo = _subPictureIndexInfo[1];

            for (int i = 0; i < arrInfo.Count; i++)
            {
                int[] nArray = (int[])arrInfo[i];
                Rectangle rectang = new Rectangle(nArray[0], 0, 5, pictureBoxYinLie_3.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2;
                    break;
                }

                rectang = new Rectangle(nArray[1], 0, 5, pictureBoxYinLie_3.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2 + 1;
                    break;
                }

            }
        }

        private void PictureBoxYinLie_3_MouseUp(object sender, MouseEventArgs e)
        {
            if (true == _bDragging)
            {
                _bDragging = false;
            }
        }

        private void PictureBoxYinLie_2_MouseMove(object sender, MouseEventArgs e)
        {
            if (_bDragging)
            {
                int nIndex = _nDragLineIndex / 2;

                ArrayList arrInfo = _subPictureIndexInfo[1];
                int[] nArray = (int[])arrInfo[nIndex];
                if (_nDragLineIndex % 2 == 0)
                {
                    int nSub = nArray[1] - nArray[0];
                    nArray[0] = e.X;
                    nArray[1] = nSub + e.X;
                }
                else
                {
                    nArray[1] = e.X;
                }

                pictureBoxYinLie_2.Invalidate();
            }
        }

        private void PictureBoxYinLie_2_MouseDown(object sender, MouseEventArgs e)
        {
            Image b = pictureBoxYinLie_2.Image;
            int x = b.Width * e.X / pictureBoxYinLie_2.Width;
            int y = b.Height * e.Y / pictureBoxYinLie_2.Height;

            ArrayList arrInfo = _subPictureIndexInfo[1];

            for (int i = 0; i < arrInfo.Count; i++)
            {
                int[] nArray = (int[])arrInfo[i];
                Rectangle rectang = new Rectangle(nArray[0], 0, 5, pictureBoxYinLie_2.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2;
                    break;
                }

                rectang = new Rectangle(nArray[1], 0, 5, pictureBoxYinLie_2.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2 + 1;
                    break;
                }

            }
        }

        private void PictureBoxYinLie_1_MouseUp(object sender, MouseEventArgs e)
        {
            if (_bDragging == true)
            {
                _bDragging = false;
            }
        }

        private void PictureBoxYinLie_1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_bDragging)
            {
                int nIndex = _nDragLineIndex / 2;

                ArrayList arrInfo = _subPictureIndexInfo[0];
                int[] nArray = (int[])arrInfo[nIndex];
                if (_nDragLineIndex % 2 == 0)
                {
                    int nSub = nArray[1] - nArray[0];
                    nArray[0] = e.X;
                    nArray[1] = nSub + e.X;
                }
                else
                {
                    nArray[1] = e.X;
                }

                pictureBoxYinLie_1.Invalidate();
            }
        }

        private void PictureBoxYinLie_1_MouseDown(object sender, MouseEventArgs e)
        {
            
            Image b = pictureBoxYinLie_1.Image;
            int x = b.Width * e.X / pictureBoxYinLie_1.Width;
            int y = b.Height * e.Y / pictureBoxYinLie_1.Height;

            ArrayList arrInfo = _subPictureIndexInfo[0];

            for (int i = 0; i < arrInfo.Count; i++)
            {
                int[] nArray = (int[])arrInfo[i];
                Rectangle rectang = new Rectangle(nArray[0], 0, 5, pictureBoxYinLie_1.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2;
                    break;
                }

                rectang = new Rectangle(nArray[1], 0, 5, pictureBoxYinLie_1.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2 + 1;
                    break;
                }

            }
        }

        private void PictureBoxYinLie_4_MouseUp(object sender, MouseEventArgs e)
        {
            if (true == _bDragging)
            {
                _bDragging = false;
            }
        }

        private void PictureBoxYinLie_4_MouseMove(object sender, MouseEventArgs e)
        {
            if (_bDragging)
            {
                int nIndex = _nDragLineIndex / 2;

                ArrayList arrInfo = _subPictureIndexInfo[3];
                int[] nArray = (int[])arrInfo[nIndex];
                if (_nDragLineIndex % 2 == 0)
                {
                    int nSub = nArray[1] - nArray[0];
                    nArray[0] = e.X;
                    nArray[1] = nSub + e.X;
                }
                else
                {
                    nArray[1] = e.X;
                }

                pictureBoxYinLie_4.Invalidate();
            }
        }

        private void PictureBoxYinLie_4_MouseDown(object sender, MouseEventArgs e)
        {
            Image b = pictureBoxYinLie_4.Image;
            int x = b.Width * e.X / pictureBoxYinLie_4.Width;
            int y = b.Height * e.Y / pictureBoxYinLie_4.Height;

            ArrayList arrInfo = _subPictureIndexInfo[3];

            for (int i = 0; i < arrInfo.Count; i++)
            {
                int[] nArray = (int[])arrInfo[i];
                Rectangle rectang = new Rectangle(nArray[0], 0, 5, pictureBoxYinLie_4.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2;
                    break;
                }

                rectang = new Rectangle(nArray[1], 0, 5, pictureBoxYinLie_4.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2 + 1;
                    break;
                }

            }
        }
        private void pictureBoxYinLie_1_Click(object sender, EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxYinLie_1);
            pictureForm.ShowDialog();
        }



        private void PictureBoxYinLie_2_Click(object sender, System.EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxYinLie_2);
            pictureForm.ShowDialog();
        }

        private void PictureBoxYinLie_3_Click(object sender, System.EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxYinLie_3);
            pictureForm.ShowDialog();
        }

        private void PictureBoxYinLie_4_Click(object sender, System.EventArgs e)
        {
            PictureForm pictureForm = new PictureForm(pictureBoxYinLie_4);
            pictureForm.ShowDialog();
        }





       
    }
}
