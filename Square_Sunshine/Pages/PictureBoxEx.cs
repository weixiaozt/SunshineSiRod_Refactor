using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SquareSiliconStickCheck.Pages
{
    internal class PictureBoxEx
    {
        private static bool isMove = false;
        private static Point mouseDownPoint;
        private static int zoomStep = 10;

        public static int ZoomStep
        {
            get
            {
                return zoomStep;
            }

            set
            {
                zoomStep = value;
            }
        }

        /// <summary>
        /// 加载IMG
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="path"></param>
        public static void loadImg(object sender, Image img)
        {
            PictureBox pbox = sender as PictureBox;


            pbox.MouseDown += new System.Windows.Forms.MouseEventHandler(MouseDown);
            pbox.MouseMove += new System.Windows.Forms.MouseEventHandler(MouseMove);
            pbox.MouseUp += new System.Windows.Forms.MouseEventHandler(MouseUp);
            pbox.MouseWheel += new System.Windows.Forms.MouseEventHandler(MouseWheel);

           
            pbox.Image = img;

        }

        public static void MouseMove(object sender, MouseEventArgs e)
        {
            PictureBox pbox = sender as PictureBox;
            pbox.Focus(); //鼠标在picturebox上时才有焦点，此时可以缩放
            if (isMove)
            {
                int x, y;   //新的pictureBox1.Location(x,y)
                int moveX, moveY; //X方向，Y方向移动大小。
                moveX = Cursor.Position.X - mouseDownPoint.X;
                moveY = Cursor.Position.Y - mouseDownPoint.Y;
                x = pbox.Location.X + moveX;
                y = pbox.Location.Y + moveY;
                pbox.Location = new Point(x, y);
                mouseDownPoint.X = Cursor.Position.X;
                mouseDownPoint.Y = Cursor.Position.Y;
            }

        }

        public static void MouseWheel(object sender, MouseEventArgs e)
        {
            PictureBox pbox = sender as PictureBox;
            int x = e.Location.X;
            int y = e.Location.Y;
            int ow = pbox.Width;
            int oh = pbox.Height;
            int VX, VY;  //因缩放产生的位移矢量

            
            if (e.Delta > 0) //放大
            {
                
                //第①步
                pbox.Width += ZoomStep;
                pbox.Height += ZoomStep;
                //第②步
                PropertyInfo pInfo = pbox.GetType().GetProperty("ImageRectangle", BindingFlags.Instance |
                 BindingFlags.NonPublic);
                Rectangle rect = (Rectangle)pInfo.GetValue(pbox, null);
                //第③步
                pbox.Width = rect.Width;
                pbox.Height = rect.Height;

                
            }
            if (e.Delta < 0) //缩小
            {
                //防止一直缩成负值
                if (pbox.Width < 50)
                    return;

                pbox.Width -= ZoomStep;
                pbox.Height -= ZoomStep;
                PropertyInfo pInfo = pbox.GetType().GetProperty("ImageRectangle", BindingFlags.Instance |
                 BindingFlags.NonPublic);
                Rectangle rect = (Rectangle)pInfo.GetValue(pbox, null);
                pbox.Width = rect.Width;
                pbox.Height = rect.Height;
            }
            //第④步，求因缩放产生的位移，进行补偿，实现锚点缩放的效果
            VX = (int)((double)x * (ow - pbox.Width) / ow);
            VY = (int)((double)y * (oh - pbox.Height) / oh);
            if (pbox.Location.X + VX > 0 && pbox.Location.Y + VY > 0)
            {
                pbox.Location = new Point(pbox.Location.X + VX, pbox.Location.Y + VY);
            }
        }


        public static void MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMove = false;
            }
        }

        public static void MouseDown(object sender, MouseEventArgs e)
        {
            PictureBox pbox = sender as PictureBox;
            if (e.Button == MouseButtons.Left)
            {
                mouseDownPoint.X = Cursor.Position.X; //记录鼠标左键按下时位置
                mouseDownPoint.Y = Cursor.Position.Y;
                isMove = true;
                pbox.Focus(); //鼠标滚轮事件(缩放时)需要picturebox有焦点
            }
        }


    }
}
