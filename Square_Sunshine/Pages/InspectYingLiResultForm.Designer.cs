using Sunny.UI;
using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace SiliconRoundBarCheck.Pages
{
    partial class InspectYingLiResultForm
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
            this.pictureBoxYingLi_1 = new System.Windows.Forms.PictureBox();
            this.pictureBoxYingLi_2 = new System.Windows.Forms.PictureBox();
            this.pictureBoxYingLi_3 = new System.Windows.Forms.PictureBox();
            this.pictureBoxYingLi_4 = new System.Windows.Forms.PictureBox();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi_1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi_2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi_3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi_4)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.pictureBoxYingLi_1);
            this.groupBox2.Controls.Add(this.pictureBoxYingLi_2);
            this.groupBox2.Controls.Add(this.pictureBoxYingLi_3);
            this.groupBox2.Controls.Add(this.pictureBoxYingLi_4);
            this.groupBox2.Location = new System.Drawing.Point(3, 38);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1628, 1153);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "应力";
            // 
            // pictureBoxYingLi_1
            // 
            this.pictureBoxYingLi_1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYingLi_1.Location = new System.Drawing.Point(17, 46);
            this.pictureBoxYingLi_1.Name = "pictureBoxYingLi_1";
            this.pictureBoxYingLi_1.Size = new System.Drawing.Size(1558, 249);
            this.pictureBoxYingLi_1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;
            this.pictureBoxYingLi_1.TabIndex = 4;
            this.pictureBoxYingLi_1.TabStop = false;
            this.pictureBoxYingLi_1.MouseDown += PictureBoxYingLi_1_MouseDown;
            this.pictureBoxYingLi_1.MouseMove += PictureBoxYingLi_1_MouseMove;
            this.pictureBoxYingLi_1.MouseUp += PictureBoxYingLi_1_MouseUp;
            this.pictureBoxYingLi_1.Paint += PictureBoxYingLi_1_Paint;
            this.pictureBoxYingLi_1.Click += new System.EventHandler(this.pictureBoxYingLi_1_Click);
            // 
            // pictureBoxYingLi_2
            // 
            this.pictureBoxYingLi_2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYingLi_2.Location = new System.Drawing.Point(17, 327);
            this.pictureBoxYingLi_2.Name = "pictureBoxYingLi_2";
            this.pictureBoxYingLi_2.Size = new System.Drawing.Size(1558, 249);
            this.pictureBoxYingLi_2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;
            this.pictureBoxYingLi_2.TabIndex = 5;
            this.pictureBoxYingLi_2.TabStop = false;
            this.pictureBoxYingLi_2.MouseDown += PictureBoxYingLi_2_MouseDown;
            this.pictureBoxYingLi_2.MouseMove += PictureBoxYingLi_2_MouseMove;
            this.pictureBoxYingLi_2.MouseUp += PictureBoxYingLi_2_MouseUp;
            this.pictureBoxYingLi_2.Paint += PictureBoxYingLi_2_Paint;
            this.pictureBoxYingLi_2.Click += new System.EventHandler(this.PictureBoxYingLi_2_Click);
            // 
            // pictureBoxYingLi_3
            // 
            this.pictureBoxYingLi_3.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYingLi_3.Location = new System.Drawing.Point(17, 618);
            this.pictureBoxYingLi_3.Name = "pictureBoxYingLi_3";
            this.pictureBoxYingLi_3.Size = new System.Drawing.Size(1558, 249);
            this.pictureBoxYingLi_3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;
            this.pictureBoxYingLi_3.TabIndex = 6;
            this.pictureBoxYingLi_3.TabStop = false;
            this.pictureBoxYingLi_3.MouseDown += PictureBoxYingLi_3_MouseDown;
            this.pictureBoxYingLi_3.MouseMove += PictureBoxYingLi_3_MouseMove;
            this.pictureBoxYingLi_3.MouseUp += PictureBoxYingLi_3_MouseUp;
            this.pictureBoxYingLi_3.Paint += PictureBoxYingLi_3_Paint;
            this.pictureBoxYingLi_3.Click += new System.EventHandler(this.PictureBoxYingLi_3_Click);
            // 
            // pictureBoxYingLi_4
            // 
            this.pictureBoxYingLi_4.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYingLi_4.Location = new System.Drawing.Point(17, 898);
            this.pictureBoxYingLi_4.Name = "pictureBoxYingLi_4";
            this.pictureBoxYingLi_4.Size = new System.Drawing.Size(1558, 249);
            this.pictureBoxYingLi_4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;
            this.pictureBoxYingLi_4.TabIndex = 7;
            this.pictureBoxYingLi_4.TabStop = false;
            this.pictureBoxYingLi_4.MouseDown += PictureBoxYingLi_4_MouseDown;
            this.pictureBoxYingLi_4.MouseMove += PictureBoxYingLi_4_MouseMove;
            this.pictureBoxYingLi_4.MouseUp += PictureBoxYingLi_4_MouseUp;
            this.pictureBoxYingLi_4.Paint += PictureBoxYingLi_4_Paint;
            this.pictureBoxYingLi_4.Click += new System.EventHandler(this.PictureBoxYingLi_4_Click);
            // 
            // InspectYingLiResultForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1655, 1194);
            this.Controls.Add(this.groupBox2);
            this.Name = "InspectYingLiResultForm";
            this.Text = "应力结果";
            this.ZoomScaleRect = new System.Drawing.Rectangle(19, 19, 1655, 988);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi_1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi_2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi_3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYingLi_4)).EndInit();
            this.ResumeLayout(false);

        }

      

        private void PictureBoxYingLi_1_MouseUp(object sender, MouseEventArgs e)
        {
            if (true ==_bDragging)
            {
                _bDragging = false;
            }
        }

        private void PictureBoxYingLi_1_MouseMove(object sender, MouseEventArgs e)
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

                pictureBoxYingLi_1.Invalidate();
            }
        }

        private void PictureBoxYingLi_1_MouseDown(object sender, MouseEventArgs e)
        {
            Image b = pictureBoxYingLi_1.Image;
            int x = b.Width * e.X / pictureBoxYingLi_1.Width;
            int y = b.Height * e.Y / pictureBoxYingLi_1.Height;

            ArrayList arrInfo = _subPictureIndexInfo[1];

            for (int i = 0; i < arrInfo.Count; i++)
            {
                int[] nArray = (int[])arrInfo[i];
                Rectangle rectang = new Rectangle(nArray[0], 0, 5, pictureBoxYingLi_1.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2;
                    break;
                }

                rectang = new Rectangle(nArray[1], 0, 5, pictureBoxYingLi_1.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2 + 1;
                    break;
                }

            }

        }

        private void PictureBoxYingLi_4_MouseMove(object sender, MouseEventArgs e)
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

                pictureBoxYingLi_4.Invalidate();
            }
        }

        private void PictureBoxYingLi_4_MouseUp(object sender, MouseEventArgs e)
        {
            if (_bDragging)
            {
                _bDragging = false;
            }
        }

        private void PictureBoxYingLi_4_MouseDown(object sender, MouseEventArgs e)
        {
            Image b = pictureBoxYingLi_4.Image;
            int x = b.Width * e.X / pictureBoxYingLi_4.Width;
            int y = b.Height * e.Y / pictureBoxYingLi_4.Height;

            ArrayList arrInfo = _subPictureIndexInfo[1];

            for (int i = 0; i < arrInfo.Count; i++)
            {
                int[] nArray = (int[])arrInfo[i];
                Rectangle rectang = new Rectangle(nArray[0], 0, 5, pictureBoxYingLi_4.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2;
                    break;
                }

                rectang = new Rectangle(nArray[1], 0, 5, pictureBoxYingLi_4.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2 + 1;
                    break;
                }

            }
        }

        private void PictureBoxYingLi_4_Paint(object sender, PaintEventArgs e)
        {
            drawLine(pictureBoxYingLi_4, e, 3);
        }

        private void PictureBoxYingLi_3_MouseMove(object sender, MouseEventArgs e)
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

                pictureBoxYingLi_3.Invalidate();
            }
        }

        private void PictureBoxYingLi_3_MouseUp(object sender, MouseEventArgs e)
        {
            if (true == _bDragging)
            {
                _bDragging = false;
            }
        }

        private void PictureBoxYingLi_3_MouseDown(object sender, MouseEventArgs e)
        {
            Image b = pictureBoxYingLi_3.Image;
            int x = b.Width * e.X / pictureBoxYingLi_3.Width;
            int y = b.Height * e.Y / pictureBoxYingLi_3.Height;

            ArrayList arrInfo = _subPictureIndexInfo[2];

            for (int i = 0; i < arrInfo.Count; i++)
            {
                int[] nArray = (int[])arrInfo[i];
                Rectangle rectang = new Rectangle(nArray[0], 0, 5, pictureBoxYingLi_3.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2;
                    break;
                }

                rectang = new Rectangle(nArray[1], 0, 5, pictureBoxYingLi_3.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2 + 1;
                    break;
                }

            }
        }

        private void PictureBoxYingLi_3_Paint(object sender, PaintEventArgs e)
        {
            drawLine(pictureBoxYingLi_3, e, 2);
        }

        private void PictureBoxYingLi_2_MouseMove(object sender, MouseEventArgs e)
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

                pictureBoxYingLi_2.Invalidate();
            }
        }

        private void PictureBoxYingLi_2_MouseUp(object sender, MouseEventArgs e)
        {
            if (true == _bDragging)
            {
                _bDragging = false;
            }
        }

        private void PictureBoxYingLi_2_MouseDown(object sender, MouseEventArgs e)
        {
            Image b = pictureBoxYingLi_2.Image;
            int x = b.Width * e.X / pictureBoxYingLi_2.Width;
            int y = b.Height * e.Y / pictureBoxYingLi_2.Height;

            ArrayList arrInfo = _subPictureIndexInfo[1];

            for (int i = 0; i < arrInfo.Count; i++)
            {
                int[] nArray = (int[])arrInfo[i];
                Rectangle rectang = new Rectangle(nArray[0], 0, 5, pictureBoxYingLi_2.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2;
                    break;
                }

                rectang = new Rectangle(nArray[1], 0, 5, pictureBoxYingLi_2.Height);

                if (rectang.Contains(e.X, e.Y))
                {
                    _bDragging = true;
                    _nDragLineIndex = i * 2 + 1;
                    break;
                }

            }
        }

        private void PictureBoxYingLi_2_Paint(object sender, PaintEventArgs e)
        {
            drawLine(pictureBoxYingLi_2, e, 1);
        }

        

       

       

        private int nHeight;
        private int _nSelectLineIndexLine = 0;

        private void drawLine(PictureBox ob, PaintEventArgs e, int nIndex)
        {
            Graphics g = e.Graphics;
            Pen myPen = new Pen(Color.DeepPink, 5);
            nHeight = ob.Image.Height;
            ArrayList infoArray = _subPictureIndexInfo[nIndex];


            for (int i = 0; i < infoArray.Count; i++)
            {
                int[] nArray = (int[])infoArray[i];

                g.DrawLine(myPen, new Point(nArray[0], 0), new Point(nArray[0], nHeight));
                g.DrawLine(myPen, new Point(nArray[1], 0), new Point(nArray[1], nHeight));

                g.FillRectangle(new SolidBrush(Color.FromArgb(125, Color.Cyan)), new Rectangle(nArray[0], 0, (nArray[1] - nArray[0]), nHeight));

            }

           
        }



        private void PictureBoxYingLi_1_Paint(object sender, PaintEventArgs e)
        {
            drawLine(pictureBoxYingLi_1, e, 0);
        }

        #endregion
        private GroupBox  groupBox2;
        private PictureBox pictureBoxYingLi_4;      
        private PictureBox pictureBoxYingLi_3;       
        private PictureBox pictureBoxYingLi_2;       
        private PictureBox pictureBoxYingLi_1;

        private bool _bDragging;
        private int _nDragLineIndex;
    }
}