using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquareSiliconStickCheck.Tools
{
    public class CMathTool
    {
        private static CMathTool _instance;


        public static CMathTool Instance()
        {
            if (_instance == null)
            {
                _instance = new CMathTool();
            }
            return _instance;
        }


        private CMathTool() { }

        public void GetDiameter(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double a = x1 - x2;
            double b = y1 - y2;
            double c = x1 - x3;
            double d = y1 - y3;
            double e = ((x1 * x1 - x2 * x2) - (y2 * y2 - y1 * y1)) / 2;
            double f = ((x1 * x1 - x3 * x3) - (y3 * y3 - y1 * y1)) / 2;

            double x = (e * d - b * f) / (a * d - b * c);
            double y = (a * f - e * c) / (a * d - b * c);

        }

        struct CircleData
        {
            public OpenCvSharp.Point2f center;
            public int radius;
        };
 
        CircleData findCircle(OpenCvSharp.Point2f pt1, OpenCvSharp.Point2f pt2, OpenCvSharp.Point2f pt3)
        {
            OpenCvSharp.Point2f midPt1, midPt2;
            midPt1.X = (pt2.X + pt1.X) / 2;
            midPt1.Y = (pt2.Y + pt1.Y) / 2;

            midPt2.X = (pt3.X + pt1.X) / 2;
            midPt2.Y = (pt3.Y + pt1.Y) / 2;

            float k1 = -(pt2.X - pt1.X) / (pt2.Y - pt1.Y);
            float k2 = -(pt3.X - pt1.X) / (pt3.Y - pt1.Y);

            CircleData CD = new CircleData();
            CD.center.X = (midPt2.Y - midPt1.Y - k2 * midPt2.X + k1 * midPt1.X) / (k1 - k2);
            CD.center.Y = midPt1.Y + k1 * (midPt2.Y - midPt1.Y - k2 * midPt2.X + k2 * midPt1.X) / (k1 - k2);
            CD.radius =  (int)Math.Sqrt((CD.center.X - pt1.X) * (CD.center.X - pt1.X) + (CD.center.Y - pt1.Y) * (CD.center.Y - pt1.Y));
            return CD;
        }
        
    }
}
