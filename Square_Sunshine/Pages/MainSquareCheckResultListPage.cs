using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts;
using Sunny.UI;
using System.Windows.Media;
using System.Diagnostics;
using System.Collections.Generic;
using SquareSiliconStickCheck.Parameters;
using SquareSiliconStickCheck.Tools;
using SiliconRoundBarCheck.Data;
using Newtonsoft.Json;
using System.IO;
using System;
using HZH_Controls;
using NPOI.SS.Formula.Functions;

namespace SquareSiliconStickCheck.Pages
{
    public partial class MainSquareCheckResultListPage : UIPage
    {
        List<Data> datas = new List<Data>();
        List<SquareStickCheckData> _inspectResults = new List<SquareStickCheckData>();
        private bool _bFirstRun = true;

        public delegate void ShowRightButtonDelegate(bool bShow);
        public ShowRightButtonDelegate ShowRightBtnFunc;


        public static MainSquareCheckResultListPage instance;
        public MainSquareCheckResultListPage()
        {
            instance = this;
            InitializeComponent();
            ShowRightBtnFunc = new ShowRightButtonDelegate(ShowRightButton);
            //_inspectResults =  CMySQLTool.Instance().SelectSquareStickResult(SettingParameter.Instance().StrMySQLDBName, "squarstickresult");
            //uiDataGridView1.AddColumn("晶编", "ColumnSerial");
            //uiDataGridView1.AddColumn("长度", "ColumnLength");
            //uiDataGridView1.AddColumn("A面边长", "ColumnLTLength");
            //uiDataGridView1.AddColumn("B面边长", "ColumnRTLength");
            //uiDataGridView1.AddColumn("C面边长", "ColumnRDLength");
            //uiDataGridView1.AddColumn("D面边长", "ColumnLDLength");
            //uiDataGridView1.AddColumn("上下对角线", "ColumnTDLength");
            //uiDataGridView1.AddColumn("左右对角线", "ColumnLRLength");
            //uiDataGridView1.AddColumn("上侧弧长", "ColumnTopDiagLength");
            //uiDataGridView1.AddColumn("左侧弧长", "ColumnLeftDiagLength");
            //uiDataGridView1.AddColumn("右侧弧长", "ColumnRightDiagLength");
            //uiDataGridView1.AddColumn("下侧弧长", "ColumnDownDiagLength");


            //uiDataGridView1.Init();
            if (_inspectResults != null)
            {
                for (int i = 0; i < _inspectResults.Count; i++)
                {
                    Data data = new Data();

                    data.ColumnSerial = _inspectResults[i].StrJBSearial;

                    string strInfo = "";
                    foreach (var item in _inspectResults[i].ListLTLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnLTLength = strInfo;


                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListRTLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnRTLength = strInfo;

                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListLDLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnLDLength = strInfo;

                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListRDLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnRDLength = strInfo;


                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListTDLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnTDLength = strInfo;

                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListLRLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnLRLength = strInfo;

                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListTopDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnTopDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListLeftDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnLeftDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListRightDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnRightDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListDownDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnDownDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListTopLeftDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnTopLeftDiagLength = strInfo;


                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListTopRightDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnTopRightDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListLeftLeftDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnLeftLeftDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListLeftRightDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnLeftRightDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListRightLeftDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnRightLeftDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListRightRightDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnRightRightDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListDownLeftDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnDownLeftDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in _inspectResults[i].ListDownRightDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    data.ColumnDownRightDiagLength = strInfo;

                    data.ColumnLength = _inspectResults[i].FLength.ToString("0.00");
                    datas.Add(data);
                }

                //设置分页控件总数
                uiPagination1.TotalCount = datas.Count;
               // uiDataGridView1.DataSource = datas;
                //设置分页控件每页数量
                //uiPagination1.PageSize = 10;

                //uiDataGridView1.SelectIndexChange += uiDataGridView1_SelectIndexChange;

                uiPagination1.ActivePage = 1;
                uiPagination1.Refresh();
            }
           
            //设置统计绑定的表格
            //uiDataGridViewFooter1.DataGridView = uiDataGridView1;
        }


        public void ShowRightButton(bool bShow)
        {
            if (bShow)
            {
                //this._rightBtn.Visible = true;
            }
            else 
            { 
                //this._rightBtn.Visible = false;
            }
        }
        public class Data
        {
            public string ColumnSerial { get; set; }
            public string ColumnLength { get; set; }
            public string ColumnLTLength { get; set; }
            public string ColumnRTLength { get; set; }
            public string ColumnLDLength { get; set; }
            public string ColumnRDLength { get; set; }
            public string ColumnTDLength { get; set; }
            public string ColumnLRLength { get; set; }
            public string ColumnTopDiagLength { get; set; }
            public string ColumnLeftDiagLength { get; set; }
            public string ColumnRightDiagLength { get; set; }
            public string ColumnDownDiagLength { get; set; }
            public string ColumnTopLeftDiagLength { get; set; }
            public string ColumnTopRightDiagLength { get; set; }
            public string ColumnLeftLeftDiagLength { get; set; }
            public string ColumnLeftRightDiagLength { get; set; }
            public string ColumnRightLeftDiagLength { get; set; }
            public string ColumnRightRightDiagLength { get; set; }
            public string ColumnDownLeftDiagLength { get; set; }
            public string ColumnDownRightDiagLength { get; set; }

        }

        /// <summary>
        /// 分页控件页面切换事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pagingSource"></param>
        /// <param name="pageIndex"></param>
        /// <param name="count"></param>
        private void uiPagination1_PageChanged(object sender, object pagingSource, int pageIndex, int count)
        {
            //未连接数据库，通过模拟数据来实现
            //一般通过ORM的分页去取数据来填充
            //pageIndex：第几页，和界面对应，从1开始，取数据可能要用pageIndex - 1
            //count：单页数据量，也就是PageSize值
            List<Data> data = new List<Data>();
            for (int i = (pageIndex - 1) * count; i < (pageIndex - 1) * count + count; i++)
            {
                if (i >= datas.Count) continue;
                data.Add(datas[i]);
            }

            //uiDataGridView1.DataSource = data;
            //uiDataGridViewFooter1.Clear();
            //uiDataGridViewFooter1["Column1"] = "合计：";
            //uiDataGridViewFooter1["Column2"] = "Column2_" + pageIndex;
            //uiDataGridViewFooter1["Column3"] = "Column3_" + pageIndex;
            //uiDataGridViewFooter1["Column4"] = "Column4_" + pageIndex;
        }

        private void uiDataGridView1_SelectIndexChange(object sender, int index)
        {
            if (index == 0 && _bFirstRun == true)
            {
                _bFirstRun = false;
                return;
            }
            //index.WriteConsole("SelectedIndex");
            Data data = datas[index];

            SquareStickCheckData result = new SquareStickCheckData();
            result.StrJBSearial = data.ColumnSerial;
            


            InspectSquareResultNewForm form = new InspectSquareResultNewForm(result);
            form.ShowDialog();
        }

        private void buttonSearch_Click(object sender, System.EventArgs e)
        {
            float[] ResultArry = new float[10];

            string strJBSeiral = textBoxJB.Text;
            List<SquareStickCheckData> results = CMySQLTool.Instance().SelectSquareStickResultBySerial(SettingParameter.Instance().StrMySQLDBName, "squarstickresult", strJBSeiral);

            if (results != null && results.Count > 0)
            {
                datas.Clear();

                for (int i = 0; i < results.Count; i++)
                {
                    Data data = new Data();
                    data.ColumnSerial = results[i].StrJBSearial;

                    string strInfo = "";
                    foreach(var item in results[i].ListLTLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelLTLength.Text = "A面边长："+strInfo;
                    data.ColumnLTLength = strInfo;
                    string[] strings = strInfo.Split(",");
                    ResultArry[0] = float.Parse(strings[0]);//A边长


                    strInfo = "";
                    foreach (var item in results[i].ListRTLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                  
                    labelRTLength.Text="B面边长：" + strInfo;
                    data.ColumnRTLength = strInfo;
                    strings = strInfo.Split(",");
                    ResultArry[1] = float.Parse(strings[0]);//B边长
                    strInfo = "";
                    foreach (var item in results[i].ListRDLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelLDLength.Text = "C面边长：" + strInfo;

                    data.ColumnLDLength = strInfo;
                    strings = strInfo.Split(",");
                    ResultArry[4] = float.Parse(strings[0]);//C边长
                 

                    strInfo = "";
                    foreach (var item in results[i].ListLDLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelRDLength.Text = "D面边长：" + strInfo;
                    data.ColumnRDLength = strInfo;
                    strings = strInfo.Split(",");
                    ResultArry[5] = float.Parse(strings[0]);///D边长

                    strInfo = "";
                    foreach (var item in results[i].ListTDLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelTDLength.Text = "对角线1：" + strInfo;
                    data.ColumnTDLength = strInfo;
                    strings = strInfo.Split(",");
                    ResultArry[2] = float.Parse(strings[0]);//对角1
                    strInfo = "";
                    foreach (var item in results[i].ListLRLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelLRLength.Text = "对角线2：" + strInfo;
                    data.ColumnLRLength = strInfo;
                    strings = strInfo.Split(",");
                    ResultArry[3] = float.Parse(strings[0]);//对角2
                    strInfo = "";
                    foreach (var item in results[i].ListTopDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelTopDiag.Text = "上侧弧长：" + strInfo;
                    data.ColumnTopDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in results[i].ListLeftDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelLeftDiag.Text = "左侧弧长：" + strInfo;
                    data.ColumnLeftDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in results[i].ListRightDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelRightDiag.Text = "右侧弧长：" + strInfo;
                    data.ColumnRightDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in results[i].ListDownDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelDownDiag.Text = "下侧弧长：" + strInfo;
                    data.ColumnDownDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in results[i].ListTopLeftDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelTopLeftDiag.Text = "上侧左弧长投影：" + strInfo;
                    data.ColumnTopLeftDiagLength = strInfo;


                    strInfo = "";
                    foreach (var item in results[i].ListTopRightDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelTopRightDiag.Text = "上侧右弧长投影：" + strInfo;
                    data.ColumnTopRightDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in results[i].ListLeftLeftDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelLeftLeftDiag.Text = "左侧左弧长投影：" + strInfo;
                    data.ColumnLeftLeftDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in results[i].ListLeftRightDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelLeftRightDiag.Text = "左侧右弧长投影：" + strInfo;
                    data.ColumnLeftRightDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in results[i].ListRightLeftDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelRightLeftDiag.Text = "右侧左弧长投影：" + strInfo;
                    data.ColumnRightLeftDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in results[i].ListRightRightDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelRightRightDiag.Text = "右侧右弧长投影：" + strInfo;
                    data.ColumnRightRightDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in results[i].ListDownLeftDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelDownLeftDiag.Text = "下侧左弧长投影：" + strInfo;
                    data.ColumnDownLeftDiagLength = strInfo;

                    strInfo = "";
                    foreach (var item in results[i].ListDownRightDiagLength)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelDownRightDiag.Text = "下侧右弧长投影：" + strInfo;

                    strInfo = "";
                    foreach (var item in results[i].ListTopAngle)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelTopAngle.Text = "上侧直边角度：" + strInfo;

                    strInfo = "";
                    foreach (var item in results[i].ListDownAngle)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelDownAngle.Text = "下侧直边角度：" + strInfo;

                    strInfo = "";
                    foreach (var item in results[i].ListLeftAngle)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelLeftAngle.Text = "左侧直边角度：" + strInfo;

                    strInfo = "";
                    foreach (var item in results[i].ListRightAngle)
                    {
                        strInfo += ((float)item).ToString("0.00") + ",";
                    }
                    labelRightAngle.Text = "右侧直边角度：" + strInfo;

                    data.ColumnDownRightDiagLength = strInfo;
                    label4.Text = "规格："+results[i].NSquareType;
                    labelLength.Text = "棒长：" + results[i].FLength;
                    label5.Text = "机台号：" + results[i].Mnum;
                    label3.Text = "尾端面垂直度：" + results[i].EVer;
                    ResultArry[7] = results[i].EVer;//尾端面垂直度
                    label6.Text="头端面垂直度：" + results[i].SVer;
                    ResultArry[6] = results[i].SVer;//头端面垂直度
                    int ser = int.Parse(results[i].NSquareType.ToString());               
                    NG_Juge(ser, ResultArry, out int Result, out string Result_State);
                    label8.Text = "结果：" + Result_State;
                    data.ColumnLength = results[i].FLength.ToString("0.00");
                    datas.Add(data);
                }
               
                //uiDataGridView1.DataSource = datas;
            }
        }
        private void NG_Juge(int SER, Array ResultArry, out int Result, out string Result_State)
        {
            Result = 0;
            Result_State = "";

            string jsonFilePath = "规格.json";
            //StreamReader sr = new StreamReader(jsonFilePath);
            using (StreamReader sr = new StreamReader(jsonFilePath))
            {
                string json = sr.ReadToEnd();
                string type = "规格" + SER;
                // 解析 JSON 字符串
                dynamic data = JsonConvert.DeserializeObject(json);
                dynamic yt = data[type];


                #region 标准值
                string y = yt["A_边长_标准值"];
                float A_BZ = float.Parse(y);
                y = yt["B_边长_标准值"];
                float B_BZ = float.Parse(y);
                y = yt["C_边长_标准值"];
                float C_BZ = float.Parse(y);
                y = yt["D_边长_标准值"];
                float D_BZ = float.Parse(y);



                y = yt["对角线1_标准值"];
                float DJ1_BZ = float.Parse(y);
                y = yt["对角线2_标准值"];
                float DJ2_BZ = float.Parse(y);

                #endregion

                #region 正负容差

                y = yt["A_边长_正容差"];
                float A_ZRC = float.Parse(y);
                y = yt["B_边长_正容差"];
                float B_ZRC = float.Parse(y);
                y = yt["C_边长_正容差"];
                float C_ZRC = float.Parse(y);
                y = yt["D_边长_正容差"];
                float D_ZRC = float.Parse(y);


                y = yt["对角线1_正容差"];
                float DJ1_ZRC = float.Parse(y);
                y = yt["对角线2_正容差"];
                float DJ2_ZRC = float.Parse(y);


                y = yt["A_边长_负容差"];
                float A_FRC = float.Parse(y);
                y = yt["B_边长_负容差"];
                float B_FRC = float.Parse(y);

                y = yt["C_边长_负容差"];
                float C_FRC = float.Parse(y);
                y = yt["D_边长_负容差"];
                float D_FRC = float.Parse(y);

                y = yt["对角线1_负容差"];
                float DJ1_FRC = float.Parse(y);
                y = yt["对角线2_负容差"];
                float DJ2_FRC = float.Parse(y);




                #endregion
                #region 端面垂直度
                y = yt["端面垂直度_单面上限"];
                float D_SX = float.Parse(y);
                y = yt["端面垂直度_双面上限"];
                float S_SX = float.Parse(y);
                #endregion
                #region 判断是否NG
                double A = ResultArry.GetValue(0).ToDouble();
                if (A > (A_BZ + A_ZRC) || A < (A_BZ - A_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "A面边长不合格";
                }
                double B = ResultArry.GetValue(1).ToDouble();
                if (B > (B_BZ + B_ZRC) || B < (B_BZ - B_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "B面边长不合格";
                }
                double C = ResultArry.GetValue(4).ToDouble();
                if (C > (C_BZ + C_ZRC) || C < (C_BZ - C_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "C面边长不合格";
                }
                double D = ResultArry.GetValue(5).ToDouble();
                if (D > (D_BZ + D_ZRC) || D < (D_BZ - D_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "D面边长不合格";
                }
                double S_DM = ResultArry.GetValue(6).ToDouble();
                double T_DM = ResultArry.GetValue(7).ToDouble();
                if (S_DM > D_SX || T_DM > D_SX || (S_DM + T_DM) > S_SX)
                {
                    Result = 1;
                    Result_State = Result_State + "端面垂直不合格";
                }

                double DJ1 = ResultArry.GetValue(2).ToDouble();
                if (DJ1 > (DJ1_BZ + DJ1_ZRC) || DJ1 < (DJ1_BZ - DJ1_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "对角线1不合格";
                }
                double DJ2 = ResultArry.GetValue(3).ToDouble();
                if (DJ2 > (DJ2_BZ + DJ2_ZRC) || DJ2 < (DJ2_BZ - DJ2_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "对角线2不合格";
                }
                if (Result == 0)
                {
                    Result_State = Result_State + "合格";
                }


                #endregion
            }

        }
    }
}