using Sunny.UI;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace SiliconRoundBarCheck.Pages
{
    partial class InspectYinLieResultForm
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
            this.pictureBoxYinLie_1 = new System.Windows.Forms.PictureBox();
            this.pictureBoxYinLie_2 = new System.Windows.Forms.PictureBox();
            this.pictureBoxYinLie_3 = new System.Windows.Forms.PictureBox();
            this.pictureBoxYinLie_4 = new System.Windows.Forms.PictureBox();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYinLie_1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYinLie_2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYinLie_3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYinLie_4)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.pictureBoxYinLie_1);
            this.groupBox2.Controls.Add(this.pictureBoxYinLie_2);
            this.groupBox2.Controls.Add(this.pictureBoxYinLie_3);
            this.groupBox2.Controls.Add(this.pictureBoxYinLie_4);
            this.groupBox2.Location = new System.Drawing.Point(3, 38);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1628, 1153);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "应力";
            // 
            // pictureBoxYingLi_1
            // 
            this.pictureBoxYinLie_1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYinLie_1.Location = new System.Drawing.Point(17, 46);
            this.pictureBoxYinLie_1.Name = "pictureBoxYingLi_1";
            this.pictureBoxYinLie_1.Size = new System.Drawing.Size(1558, 249);
            this.pictureBoxYinLie_1.TabIndex = 4;
            this.pictureBoxYinLie_1.TabStop = false;
            this.pictureBoxYinLie_1.MouseDown += PictureBoxYinLie_1_MouseDown;
            this.pictureBoxYinLie_1.MouseMove += PictureBoxYinLie_1_MouseMove;
            this.pictureBoxYinLie_1.MouseUp += PictureBoxYinLie_1_MouseUp;
            this.pictureBoxYinLie_1.Paint += PictureBoxYinLie_1_Paint;
            this.pictureBoxYinLie_1.Click += new System.EventHandler(this.pictureBoxYinLie_1_Click);
            // 
            // pictureBoxYingLi_2
            // 
            this.pictureBoxYinLie_2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYinLie_2.Location = new System.Drawing.Point(17, 327);
            this.pictureBoxYinLie_2.Name = "pictureBoxYingLi_2";
            this.pictureBoxYinLie_2.Size = new System.Drawing.Size(1558, 249);
            this.pictureBoxYinLie_2.TabIndex = 5;
            this.pictureBoxYinLie_2.TabStop = false;
            this.pictureBoxYinLie_2.MouseDown += PictureBoxYinLie_2_MouseDown;
            this.pictureBoxYinLie_2.MouseMove += PictureBoxYinLie_2_MouseMove;
            this.pictureBoxYinLie_2.MouseUp += PictureBoxYinLie_2_MouseUp;
            this.pictureBoxYinLie_2.Paint += PictureBoxYinLie_2_Paint;
            this.pictureBoxYinLie_2.Click += new System.EventHandler(this.PictureBoxYinLie_2_Click);
            // 
            // pictureBoxYingLi_3
            // 
            this.pictureBoxYinLie_3.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYinLie_3.Location = new System.Drawing.Point(17, 618);
            this.pictureBoxYinLie_3.Name = "pictureBoxYingLi_3";
            this.pictureBoxYinLie_3.Size = new System.Drawing.Size(1558, 249);
            this.pictureBoxYinLie_3.TabIndex = 6;
            this.pictureBoxYinLie_3.TabStop = false;
            this.pictureBoxYinLie_3.MouseDown += PictureBoxYinLie_3_MouseDown;
            this.pictureBoxYinLie_3.MouseMove += PictureBoxYinLie_3_MouseMove;
            this.pictureBoxYinLie_3.MouseUp += PictureBoxYinLie_3_MouseUp;
            this.pictureBoxYinLie_3.Paint += PictureBoxYinLie_3_Paint;
            this.pictureBoxYinLie_3.Click += new System.EventHandler(this.PictureBoxYinLie_3_Click);
            // 
            // pictureBoxYingLi_4
            // 
            this.pictureBoxYinLie_4.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxYinLie_4.Location = new System.Drawing.Point(17, 898);
            this.pictureBoxYinLie_4.Name = "pictureBoxYingLi_4";
            this.pictureBoxYinLie_4.Size = new System.Drawing.Size(1558, 249);
            this.pictureBoxYinLie_4.TabIndex = 7;
            this.pictureBoxYinLie_4.TabStop = false;
            this.pictureBoxYinLie_4.MouseDown += PictureBoxYinLie_4_MouseDown;
            this.pictureBoxYinLie_4.MouseUp += PictureBoxYinLie_4_MouseUp;
            this.pictureBoxYinLie_4.MouseMove += PictureBoxYinLie_4_MouseMove;
            this.pictureBoxYinLie_4.Paint += PictureBoxYinLie_4_Paint;
            this.pictureBoxYinLie_4.Click += new System.EventHandler(this.PictureBoxYinLie_4_Click);
            
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
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYinLie_1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYinLie_2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYinLie_3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxYinLie_4)).EndInit();
            this.ResumeLayout(false);

        }

        private void drawLine(PictureBox ob, PaintEventArgs e, int nIndex)
        {
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

        private void PictureBoxYinLie_1_Paint(object sender, PaintEventArgs e)
        {
            drawLine(pictureBoxYinLie_1, e, 0);
        }

        private void PictureBoxYinLie_4_Paint(object sender, PaintEventArgs e)
        {
            drawLine(pictureBoxYinLie_4, e, 3);

        }

        private void PictureBoxYinLie_3_Paint(object sender, PaintEventArgs e)
        {
            drawLine(pictureBoxYinLie_3, e, 2);

        }

        private void PictureBoxYinLie_2_Paint(object sender, PaintEventArgs e)
        {
            drawLine(pictureBoxYinLie_2, e, 1);

        }




        #endregion
        private GroupBox  groupBox2;
        private PictureBox pictureBoxYinLie_4;      
        private PictureBox pictureBoxYinLie_3;       
        private PictureBox pictureBoxYinLie_2;       
        private PictureBox pictureBoxYinLie_1;
        private bool _bDragging;
        private int _nDragLineIndex = 0;
       
    }
}