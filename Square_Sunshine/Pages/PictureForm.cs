using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SquareSiliconStickCheck.Pages
{
    public partial class PictureForm : UIForm
    {
        public PictureForm()
        {
            InitializeComponent();
        }

        public PictureForm(PictureBox showPic)
        {
            InitializeComponent();
            if (showPic != null && showPic.Image != null)
            {
                Bitmap map = new Bitmap(showPic.Image);
                this.kpImageViewer1.Image = map;
                
            }

        }
    }
}
