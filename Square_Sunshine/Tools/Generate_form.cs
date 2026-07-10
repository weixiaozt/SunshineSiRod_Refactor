using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using SiliconRoundBarCheck.Data;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Windows.Forms; // 用于.xlsx



namespace SquareSiliconStickCheck.Tools
{
    public class Generate_form
    {
        private static Generate_form _instance;
        //SquareStickCheckData data = new SquareStickCheckData();

        public static Generate_form Instance()
        {
            if (_instance == null)
            {
                _instance = new Generate_form();
            }
            return _instance;
        }


        public void ExportToExcel(SquareStickCheckData data)
        {
            if (!System.IO.Directory.Exists(@"D:\Report"))
            {
                System.IO.Directory.CreateDirectory(@"D:\Report");//不存在就创建文件夹
            }
            string tim= DateTime.Now.ToLongDateString().ToString();
            
            IWorkbook workbook = new XSSFWorkbook();
            try
            {
                using (FileStream file = new FileStream("D:\\Report\\" + tim + ".xlsx", FileMode.Open, FileAccess.ReadWrite))
                {
                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);
                        stream.Position = 0;

                        workbook = new XSSFWorkbook(stream); // 用于.xlsx80
                                                             // 或者 workbook = new HSSFWorkbook(stream); // 用于.xls
                    }

                }

            }
            catch (Exception)
            {
                ISheet excelsheet = workbook.CreateSheet("Sheet1");

                IRow er =excelsheet.CreateRow(0);
                
                for (int j = 0; j < 30; j++)
                {
                    er.CreateCell(j);
                }
                //excelsheet.CreateRow(1);
                IRow Row1 = excelsheet.GetRow(0);
                ICell cell1 = Row1.GetCell(0);
                cell1.SetCellValue("Crystal knitting");

                //IRow Row1 = excelsheet.CreateRow(0);
                ICell cell2 = Row1.GetCell(1);
                cell2.SetCellValue("Edge_A");
                ICell cell3 = Row1.GetCell(2);
                cell3.SetCellValue("Edge_B");
                ICell cell4 = Row1.GetCell(3);
                cell4.SetCellValue("Edge_C");
                ICell cell5 = Row1.GetCell(4);
                cell5.SetCellValue("Edge_D");

                ICell cell6 = Row1.GetCell(5);
                cell6.SetCellValue("Diagonal Length_1");
                ICell cell7 = Row1.GetCell(6);
                cell7.SetCellValue("Diagonal Length_2");
                ICell cell8 = Row1.GetCell(7);
                cell8.SetCellValue("Arc length_1");
                ICell cell9 = Row1.GetCell(8);
                cell9.SetCellValue("Arc length_2");
                ICell cell10 = Row1.GetCell(9);
                cell10.SetCellValue("Arc length_3");
                ICell cell11 = Row1.GetCell(10);
                cell11.SetCellValue("Arc length_4");

                ICell cell12 = Row1.GetCell(11);
                cell12.SetCellValue("1_Projectio1");
                ICell cell13 = Row1.GetCell(12);
                cell13.SetCellValue("1_Projectio2");

                ICell cell14 = Row1.GetCell(13);
                cell14.SetCellValue("2_Projectio1");
                ICell cell15 = Row1.GetCell(14);
                cell15.SetCellValue("2_Projectio2");

                ICell cell16 = Row1.GetCell(15);
                cell16.SetCellValue("3_Projectio1");
                ICell cell17 = Row1.GetCell(16);
                cell17.SetCellValue("3_Projectio2");

                ICell cell18 = Row1.GetCell(17);
                cell18.SetCellValue("4_Projectio1");
                ICell cell19 = Row1.GetCell(18);
                cell19.SetCellValue("4_Projectio2");

                ICell cell20 = Row1.GetCell(19);
                cell20.SetCellValue("Side verticality_1");
                ICell cell21 = Row1.GetCell(20);
                cell21.SetCellValue("Side verticality_4");

                ICell cell22 = Row1.GetCell(21);
                cell22.SetCellValue("Side verticality_2");
                ICell cell23 = Row1.GetCell(22);
                cell23.SetCellValue("Side verticality_3");

                ICell cell24 = Row1.GetCell(23);
                cell24.SetCellValue("Length");
                ICell cell25 = Row1.GetCell(24);
                cell25.SetCellValue("Face verticality_H");
                ICell cell26 = Row1.GetCell(25);
                cell26.SetCellValue("Face verticality_T");
                ICell cell27 = Row1.GetCell(26);
                cell27.SetCellValue("DataTime");
            }
            try
            {

            

            int i = 0;
            ISheet excelsheet1 = workbook.GetSheet("Sheet1");
            while (true)
            {
                IRow rowif = excelsheet1.GetRow(i);
                if (rowif == null)
                {
                    break;
                }
                i++;
            }
            IRow row = excelsheet1.CreateRow(i);
            for (int j = 0; j < 30; j++)
            {
                row.CreateCell(j);
            }
         
            ICell cel1 = row.GetCell(0);
            string jingbian = data.StrJBSearial;
            cel1.SetCellValue(jingbian);//写入晶编
            string strLTLength = "";
            foreach (var item in data.ListLTLength)
            {
                    strLTLength += ((float)item).ToString("0.00");
            }
            cel1 = row.GetCell(1);           
            cel1.SetCellValue(strLTLength);//写入A面


                string strRTLength = "";
                foreach (var item in data.ListRTLength)
                {
                    strRTLength += ((float)item).ToString("0.00");
                }
                cel1 = row.GetCell(2);
            cel1.SetCellValue(strRTLength);//写入B面


            string strRDLength = "";
            foreach (var item in data.ListRDLength)
            {
                    strRDLength += ((float)item).ToString("0.00") ;
            }
            cel1 = row.GetCell(3);
            cel1.SetCellValue(strRDLength);//写入C面
            cel1 = row.GetCell(4);

                string strLDLength = "";
                foreach (var item in data.ListLDLength)
                {
                    strLDLength += ((float)item).ToString("0.00") ;
                }
                cel1.SetCellValue(strLDLength);//写入D面

            string strTDLength = "";
            foreach (var item in data.ListTDLength)
            {
                strTDLength += ((float)item).ToString("0.00");
            }
            cel1 = row.GetCell(5);
            cel1.SetCellValue(strTDLength);//写入对角1

            string strRLLength = "";
            foreach (var item in data.ListLRLength)
            {
                strRLLength += ((float)item).ToString("0.00");
            }
            cel1 = row.GetCell(6);
            cel1.SetCellValue(strRLLength);//写入对角2

            string ListTopDiagLength = "";
            foreach (var item in data.ListTopDiagLength)
            {
                ListTopDiagLength += ((float)item).ToString("0.00");
            }
            cel1 = row.GetCell(7);
            cel1.SetCellValue(ListTopDiagLength);//写入弧长I

            string ListLeftDiagLength = "";
            foreach (var item in data.ListLeftDiagLength)
            {
                ListLeftDiagLength += ((float)item).ToString("0.00");
            }
            cel1 = row.GetCell(8);
            cel1.SetCellValue(ListLeftDiagLength);

            string ListDownDiagLength = "";
            foreach (var item in data.ListDownDiagLength)
            {
                ListDownDiagLength += ((float)item).ToString("0.00") ;
            }
            cel1 = row.GetCell(10);
            cel1.SetCellValue(ListDownDiagLength);

            string ListRightDiagLength = "";
            foreach (var item in data.ListRightDiagLength)
            {
                ListRightDiagLength += ((float)item).ToString("0.00") ;
            }
            cel1 = row.GetCell(9);
            cel1.SetCellValue(ListRightDiagLength);

            string ListTopLeftDiagLength = "";
            foreach (var item in data.ListTopLeftDiagLength)
            {
                ListTopLeftDiagLength += ((float)item).ToString("0.00") ;
            }
            cel1 = row.GetCell(11);
            cel1.SetCellValue(ListTopLeftDiagLength);//写入弧长投影

            string ListTopRightDiagLength = "";
            foreach (var item in data.ListTopRightDiagLength)
            {
                ListTopRightDiagLength += ((float)item).ToString("0.00") ;
            }
            cel1 = row.GetCell(12);
            cel1.SetCellValue(ListTopRightDiagLength);//写入弧长投影

            string ListLeftLeftDiagLength = "";
            foreach (var item in data.ListLeftLeftDiagLength)
            {
                ListLeftLeftDiagLength += ((float)item).ToString("0.00");
            }
            cel1 = row.GetCell(13);
            cel1.SetCellValue(ListLeftLeftDiagLength);//写入弧长投影

            string ListLeftRightDiagLength = "";
            foreach (var item in data.ListLeftRightDiagLength)
            {
                ListLeftRightDiagLength += ((float)item).ToString("0.00");
            }
            cel1 = row.GetCell(14);
            cel1.SetCellValue(ListLeftRightDiagLength);//写入弧长投影



            string ListDownLeftDiagLength = "";
            foreach (var item in data.ListDownLeftDiagLength)
            {
                ListDownLeftDiagLength += ((float)item).ToString("0.00") ;
            }
            cel1 = row.GetCell(17);
            cel1.SetCellValue(ListDownLeftDiagLength);//写入弧长投影

            string ListDownRightDiagLength = "";
            foreach (var item in data.ListDownRightDiagLength)
            {
                ListDownRightDiagLength += ((float)item).ToString("0.00") ;
            }
            cel1 = row.GetCell(18);
            cel1.SetCellValue(ListDownRightDiagLength);//写入弧长投影


            string ListRightLeftDiagLength = "";
            foreach (var item in data.ListRightLeftDiagLength)
            {
                ListRightLeftDiagLength += ((float)item).ToString("0.00");
            }
            cel1 = row.GetCell(15);
            cel1.SetCellValue(ListRightLeftDiagLength);//写入弧长投影

            string ListRightRightDiagLength = "";
            foreach (var item in data.ListRightRightDiagLength)
            {
                ListRightRightDiagLength += ((float)item).ToString("0.00") ;
            }
            cel1 = row.GetCell(16);
            cel1.SetCellValue(ListRightRightDiagLength);//写入弧长投影


            string ListTopAngle = "";
            foreach (var item in data.ListTopAngle)
            {
                ListTopAngle += ((float)item).ToString("0.00") ;
            }
            cel1 = row.GetCell(19);
            cel1.SetCellValue(ListTopAngle);//写入垂直度

            string ListLeftAngle = "";
            foreach (var item in data.ListLeftAngle)
            {
                ListLeftAngle += ((float)item).ToString("0.00") ;
            }
            cel1 = row.GetCell(21);
            cel1.SetCellValue(ListLeftAngle);//写入垂直度

            string ListDownAngle = "";
            foreach (var item in data.ListDownAngle)
            {
                    ListDownAngle += ((float)item).ToString("0.00"); 
            }
            cel1 = row.GetCell(20);
            cel1.SetCellValue(ListDownAngle);//写入垂直度

            string ListRightAngle = "";
            foreach (var item in data.ListRightAngle)
            {
                ListRightAngle += ((float)item).ToString("0.00") ;
            }
            cel1 = row.GetCell(22);
            cel1.SetCellValue(ListRightAngle);//写入垂直度

            string strLengthInfo = "";

            strLengthInfo = data.FLength.ToString("0.00");

            cel1 = row.GetCell(23);
            cel1.SetCellValue(strLengthInfo);//写入棒长

                string strS_VER = "";

                strS_VER = data.SVer.ToString("0.00");

                cel1 = row.GetCell(24);
                cel1.SetCellValue(strS_VER);//写入头端面垂直度

                string strE_VER = "";

                strE_VER = data.EVer.ToString("0.00");

                cel1 = row.GetCell(25);
                cel1.SetCellValue(strE_VER);//写入尾端面垂直度



                DateTime dateTime = DateTime.Now;
                string strDateTime = dateTime.ToString("yyyy-MM-dd HH:mm:ss");

                cel1 = row.GetCell(26);
                cel1.SetCellValue(strDateTime);//写入尾端面垂直度
                using (FileStream file = new FileStream("D:\\Report\\" + tim + ".xlsx", FileMode.Create, FileAccess.Write))
                {
                    ISheet excelsheet = workbook.GetSheet("Sheet1");
                    ICellStyle style = workbook.CreateCellStyle();

                    style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                    style.VerticalAlignment = VerticalAlignment.Center;
                    //for (int k = 0; k < 30; k++)
                    //{
                    //    excelsheet.AutoSizeColumn(k);
                        
                    //}
                    excelsheet.DefaultColumnWidth = 20;
                    workbook.Write(file);
                }

            }
            catch (Exception)
            {


            }






        }



    }
}






