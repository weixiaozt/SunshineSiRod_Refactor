using Sunny.UI;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace SiliconRoundBarCheck.Pages
{
    partial class InspectLJResultForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.pictureBoxLJ_1 = new System.Windows.Forms.PictureBox();
            this.pictureBoxLJ_2 = new System.Windows.Forms.PictureBox();
            this.pictureBoxLJ_3 = new System.Windows.Forms.PictureBox();
            this.pictureBoxLJ_4 = new System.Windows.Forms.PictureBox();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLJ_1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLJ_2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLJ_3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLJ_4)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.pictureBoxLJ_1);
            this.groupBox2.Controls.Add(this.pictureBoxLJ_2);
            this.groupBox2.Controls.Add(this.pictureBoxLJ_3);
            this.groupBox2.Controls.Add(this.pictureBoxLJ_4);
            this.groupBox2.Location = new System.Drawing.Point(3, 38);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1628, 1153);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "应力";
            // 
            // pictureBoxYingLi_1
            // 
            this.pictureBoxLJ_1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxLJ_1.Location = new System.Drawing.Point(17, 46);
            this.pictureBoxLJ_1.Name = "pictureBoxYingLi_1";
            this.pictureBoxLJ_1.Size = new System.Drawing.Size(1558, 249);
            this.pictureBoxLJ_1.TabIndex = 4;
            this.pictureBoxLJ_1.TabStop = false;
            this.pictureBoxLJ_1.MouseDown += PictureBoxLJ_1_MouseDown;
            this.pictureBoxLJ_1.MouseUp += PictureBoxLJ_1_MouseUp;
            this.pictureBoxLJ_1.MouseMove += PictureBoxLJ_1_MouseMove;
            this.pictureBoxLJ_1.Paint += PictureBoxLJ_1_Paint;
            this.pictureBoxLJ_1.Click += new System.EventHandler(this.pictureBoxLJ_1_Click);
            // 
            // pictureBoxYingLi_2
            // 
            this.pictureBoxLJ_2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxLJ_2.Location = new System.Drawing.Point(17, 327);
            this.pictureBoxLJ_2.Name = "pictureBoxYingLi_2";
            this.pictureBoxLJ_2.Size = new System.Drawing.Size(1558, 249);
            this.pictureBoxLJ_2.TabIndex = 5;
            this.pictureBoxLJ_2.TabStop = false;
            this.pictureBoxLJ_2.MouseDown += PictureBoxLJ_2_MouseDown;
            this.pictureBoxLJ_2.MouseUp += PictureBoxLJ_2_MouseUp;
            this.pictureBoxLJ_2.MouseMove += PictureBoxLJ_2_MouseMove;
            this.pictureBoxLJ_2.Paint += PictureBoxLJ_2_Paint;
            this.pictureBoxLJ_2.Click += new System.EventHandler(this.PictureBoxLJ_2_Click);
            // 
            // pictureBoxYingLi_3
            // 
            this.pictureBoxLJ_3.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxLJ_3.Location = new System.Drawing.Point(17, 618);
            this.pictureBoxLJ_3.Name = "pictureBoxYingLi_3";
            this.pictureBoxLJ_3.Size = new System.Drawing.Size(1558, 249);
            this.pictureBoxLJ_3.TabIndex = 6;
            this.pictureBoxLJ_3.TabStop = false;
            this.pictureBoxLJ_3.MouseDown += PictureBoxLJ_3_MouseDown;
            this.pictureBoxLJ_3.MouseUp += PictureBoxLJ_3_MouseUp;
            this.pictureBoxLJ_3.MouseMove += PictureBoxLJ_3_MouseMove;
            this.pictureBoxLJ_3.Paint += PictureBoxLJ_3_Paint;
            this.pictureBoxLJ_3.Click += new System.EventHandler(this.PictureBoxLJ_3_Click);
            // 
            // pictureBoxYingLi_4
            // 
            this.pictureBoxLJ_4.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxLJ_4.Location = new System.Drawing.Point(17, 898);
            this.pictureBoxLJ_4.Name = "pictureBoxYingLi_4";
            this.pictureBoxLJ_4.Size = new System.Drawing.Size(1558, 249);
            this.pictureBoxLJ_4.TabIndex = 7;
            this.pictureBoxLJ_4.TabStop = false;
            this.pictureBoxLJ_4.MouseDown += PictureBoxLJ_4_MouseDown;
            this.pictureBoxLJ_4.MouseUp += PictureBoxLJ_4_MouseUp;
            this.pictureBoxLJ_4.MouseMove += PictureBoxLJ_4_MouseMove;
            this.pictureBoxLJ_4.Paint += PictureBoxLJ_4_Paint;
            this.pictureBoxLJ_4.Click += new System.EventHandler(this.PictureBoxLJ_4_Click);
            // 
            // InspectYingLiResultForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1655, 1194);
            this.Controls.Add(this.groupBox2);
            this.Name = "InspectYingLiResultForm";
            this.Text = "孪晶结果";
            this.ZoomScaleRect = new System.Drawing.Rectangle(19, 19, 1655, 988);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLJ_1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLJ_2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLJ_3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLJ_4)).EndInit();
            this.ResumeLayout(false);

        }

        private void PictureBoxLJ_3_Paint(object sender, PaintEventArgs e)
        {
            drawLine(pictureBoxLJ_3, e, 2);
        }

        private void drawLine(PictureBox ob, PaintEventArgs e, int nIndex)
        {
            if (ob.Image == null)
            {
                return;
            }

            Graphics g = e.Graphics;
            Pen myPen = new Pen(Color.DeepPink, 5);

            

            int nHeight = ob.Image.Height;
            ArrayList infoArray = _subPictureIndexInfo[nIndex];


            for (int i = 0; i < infoArray.Count; i++)
            {
                int[] nArray = (int[])infoArray[i];

                g.DrawLine(myPen, new Point(nArray[0], 0), new Point(nArray[0], nHeight));
                g.DrawLine(myPen, new Point(nArray[1], 0), new Point(nArray[1], nHeight));

                g.FillRectangle(new SolidBrush(Color.FromArgb(125, Color.Cyan)), new Rectangle(nArray[0], 0, (nArray[1] - nArray[0]), nHeight));

            }


        }


        private void PictureBoxLJ_1_Paint(object sender, PaintEventArgs e)
        {
            drawLine(pictureBoxLJ_1, e, 1);
        }

        private void PictureBoxLJ_1_MouseMove(object sender, MouseEventArgs e)
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

                pictureBoxLJ_1.Invalidate();
            }

        }

        private void PictureBoxLJ_1_MouseUp(object sender, MouseEventArgs e)
        {
            if (_bDragging == true)
            {
                _bDragging = false;
            }

        }

        private void PictureBoxLJ_1_MouseDown(object sender, MouseEventArgs e)
        {
            Image b = pictureBoxLJ_1.Image;
            int x = b.Width * e.X / pictureBoxLJ_1.Width;
            int y = b.Height * e.Y / pictureBoxLJ_1.Height;

            ArrayList arrInfo = _subPictureIndexInfo[0];

            for (int i = 0; i < arrInfo.Count; i++)
            {
                int[] nArray = (int[])arrInfo[i];
                Rectangle rectang = new Rectangle(nArray[0], 0, 5, pictureBoxLJ_1.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2;
                    break;
                }

                rectang = new Rectangle(nArray[1], 0, 5, pictureBoxLJ_1.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2 + 1;
                    break;
                }

            }
        }

        private void PictureBoxLJ_2_Paint(object sender, PaintEventArgs e)
        {
            drawLine(pictureBoxLJ_2, e, 1);
        }

        private void PictureBoxLJ_2_MouseMove(object sender, MouseEventArgs e)
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

                pictureBoxLJ_2.Invalidate();
            }
        }

        private void PictureBoxLJ_2_MouseUp(object sender, MouseEventArgs e)
        {
            if (true == _bDragging)
            {
                _bDragging = false;
            }
        }

        private void PictureBoxLJ_2_MouseDown(object sender, MouseEventArgs e)
        {
            Image b = pictureBoxLJ_2.Image;
            int x = b.Width * e.X / pictureBoxLJ_2.Width;
            int y = b.Height * e.Y / pictureBoxLJ_2.Height;

            ArrayList arrInfo = _subPictureIndexInfo[1];

            for (int i = 0; i < arrInfo.Count; i++)
            {
                int[] nArray = (int[])arrInfo[i];
                Rectangle rectang = new Rectangle(nArray[0], 0, 5, pictureBoxLJ_2.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2;
                    break;
                }

                rectang = new Rectangle(nArray[1], 0, 5, pictureBoxLJ_2.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2 + 1;
                    break;
                }

            }
        }

        private void PictureBoxLJ_3_MouseMove(object sender, MouseEventArgs e)
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

                pictureBoxLJ_3.Invalidate();
            }
        }

        private void PictureBoxLJ_3_MouseUp(object sender, MouseEventArgs e)
        {
            if (true == _bDragging)
            {
                _bDragging = false;
            }
        }

        private void PictureBoxLJ_3_MouseDown(object sender, MouseEventArgs e)
        {
            Image b = pictureBoxLJ_3.Image;
            int x = b.Width * e.X / pictureBoxLJ_3.Width;
            int y = b.Height * e.Y / pictureBoxLJ_3.Height;

            ArrayList arrInfo = _subPictureIndexInfo[1];

            for (int i = 0; i < arrInfo.Count; i++)
            {
                int[] nArray = (int[])arrInfo[i];
                Rectangle rectang = new Rectangle(nArray[0], 0, 5, pictureBoxLJ_3.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2;
                    break;
                }

                rectang = new Rectangle(nArray[1], 0, 5, pictureBoxLJ_3.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2 + 1;
                    break;
                }

            }
        }

        private void PictureBoxLJ_4_Paint(object sender, PaintEventArgs e)
        {
            drawLine(pictureBoxLJ_4, e, 3);
        }

        private void PictureBoxLJ_4_MouseMove(object sender, MouseEventArgs e)
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

                pictureBoxLJ_4.Invalidate();
            }
        }

        private void PictureBoxLJ_4_MouseUp(object sender, MouseEventArgs e)
        {
            if (true == _bDragging)
            {
                _bDragging = false;
            }
        }

        private void PictureBoxLJ_4_MouseDown(object sender, MouseEventArgs e)
        {
            Image b = pictureBoxLJ_4.Image;
            int x = b.Width * e.X / pictureBoxLJ_4.Width;
            int y = b.Height * e.Y / pictureBoxLJ_4.Height;

            ArrayList arrInfo = _subPictureIndexInfo[1];

            for (int i = 0; i < arrInfo.Count; i++)
            {
                int[] nArray = (int[])arrInfo[i];
                Rectangle rectang = new Rectangle(nArray[0], 0, 5, pictureBoxLJ_4.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2;
                    break;
                }

                rectang = new Rectangle(nArray[1], 0, 5, pictureBoxLJ_4.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2 + 1;
                    break;
                }

            }
        }

        #endregion
        private GroupBox  groupBox2;
        private PictureBox pictureBoxLJ_4;      
        private PictureBox pictureBoxLJ_3;       
        private PictureBox pictureBoxLJ_2;       
        private PictureBox pictureBoxLJ_1;
        private bool _bDragging;
        private int _nDragLineIndex;
    }
}