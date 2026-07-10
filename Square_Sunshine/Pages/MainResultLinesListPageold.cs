using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts;
using Sunny.UI;
using System.Windows.Media;
using System.Diagnostics;
using System.Collections.Generic;
using SiliconRoundBarCheck.Parameters;
using SiliconRoundBarCheck.Tools;
using MySqlX.XDevAPI.Common;
using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace SiliconRoundBarCheck.Pages
{
    public partial class MainResultLinesListInfoPage : UIPage
    {
        List<Data> datas = new List<Data>();
        List<InspectResultLineInfo> _inspectResults = new List<InspectResultLineInfo>();
        private bool _bFirstRun = true;

        public delegate void ShowRightButtonDelegate(bool bShow);

        public ShowRightButtonDelegate ShowRightBtnFunc;


        public static MainResultLinesListInfoPage instance;
        public MainResultLinesListInfoPage()
        {
            instance = this;
            InitializeComponent();

            ShowRightBtnFunc = new ShowRightButtonDelegate(ShowRightButton);
            DateTime datebegin = DateTime.Now;
            string strTextDate = datebegin.Year.ToString() + "-" + datebegin.Month.ToString() + "-"+ datebegin.Day.ToString();
            uiDatePickerBegin.Text = strTextDate;
            uiDatePickerEnd.Text = strTextDate;
            _inspectResults =  CMySQLTool.Instance().SearchAllResultLinesByCur(datebegin);
            uiDataGridView1.AddColumn("晶编", "ColumnSiliconStickLineNum");
            uiDataGridView1.AddColumn("外观长度", "ColumnApprearancelength");
            uiDataGridView1.AddColumn("外观有效长度", "ColumnApprearanceValidlength");
            uiDataGridView1.AddColumn("最大半径", "ColumnApprearanceMaxRadius");
            uiDataGridView1.AddColumn("最小半径", "ColumnApprearanceMinRadius");
            uiDataGridView1.AddColumn("晶线数", "ColumnSiliconLineNum");
            uiDataGridView1.AddColumn("异常区域_1", "ColumnAbnormalAreaFir");
            uiDataGridView1.AddColumn("异常区域_2", "ColumnAbnormalAreaSec");
            uiDataGridView1.AddColumn("异常区域_3", "ColumnAbnormalAreaThr");
            uiDataGridView1.AddColumn("异常区域_4", "ColumnAbnormalAreaFour");
            uiDataGridView1.AddColumn("画线数据", "ColumnDrawLines");
            


            uiDataGridView1.Init();
            for (int i = 0; i < _inspectResults.Count; i++)
            {
                Data data = new Data();
                data.ColumnSiliconStickLineNum = _inspectResults[i].StrSiliconStickNum;
                data.ColumnApprearancelength = _inspectResults[i].FApperanceLength.ToString("0.00");
                data.ColumnApprearanceValidlength = _inspectResults[i].FApperanceLength.ToString("0.00");
                data.ColumnApprearanceMaxRadius = _inspectResults[i].FAppearanceMaxRadius.ToString("0.00");
                data.ColumnApprearanceMinRadius = _inspectResults[i].FAppearanceMinRadius.ToString("0.00");
                data.ColumnSiliconLineNum = _inspectResults[i].NSiliconLineNum.ToString();
                data.ColumnAbnormalAreaFir = _inspectResults[i].StrAbnormalFir;
                data.ColumnAbnormalAreaSec = _inspectResults[i].StrAbnormalSec;
                data.ColumnAbnormalAreaThr = _inspectResults[i].StrAbnormalThr;
                data.ColumnAbnormalAreaFour = _inspectResults[i].StrAbnormalFour;
                data.ColumnDrawLines = _inspectResults[i].StrDrawLineInfo;
                data.CurDate = _inspectResults[i].CurDate;
                data.ColumnResultPath = _inspectResults[i].StrResultPath;
                datas.Add(data);
            }

            //设置分页控件总数
            uiPagination1.TotalCount = datas.Count;
            uiDataGridView1.DataSource = datas;
            //设置分页控件每页数量
            //uiPagination1.PageSize = 10;

            
            uiDataGridView1.SelectIndexChange += uiDataGridView1_SelectIndexChange;

            uiPagination1.ActivePage = 1;
            
            uiPagination1.Refresh();
            //设置统计绑定的表格
            //uiDataGridViewFooter1.DataGridView = uiDataGridView1;
        }


        public void ShowRightButton(bool bShow)
        {
            if (bShow)
            {
                this._rightBtn.Visible = true;
            }
            else 
            { 
                this._rightBtn.Visible = false;
            }
        }
        public class Data
        {
            public string ColumnJingBian { get; set; }

            public string ColumnSiliconStickLineNum { get; set; }

            public string ColumnApprearancelength { get; set; }

            public string ColumnApprearanceValidlength { get; set; }

            public string ColumnApprearanceMaxRadius { get; set; }

            public string ColumnApprearanceMinRadius { get; set; }

            public string ColumnSiliconLineNum { get; set; }

            public string ColumnAbnormalAreaFir { get; set; }

            public string ColumnAbnormalAreaSec { get; set; }

            public string ColumnAbnormalAreaThr { get; set; }

            public string ColumnAbnormalAreaFour { get; set; }

            public string ColumnDrawLines { get; set; }

            public string ColumnResultPath { get; set; }

            public DateTime CurDate { get; set; }
           
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

            uiDataGridView1.DataSource = data;
            
        }

        private void uiDataGridView1_SelectIndexChange(object sender, int index)
        {
            //if (index == 0 && _bFirstRun == true)
            //{
            //    _bFirstRun = false;
            //    return;
            //}
            //index.WriteConsole("SelectedIndex");
            try
            {
                Data data = datas[index];

                InspectResultLineInfo result = new InspectResultLineInfo();

                result.StrSiliconStickNum = data.ColumnSiliconStickLineNum;
                if (data.ColumnApprearancelength != null)
                {
                    result.FApperanceLength = float.Parse(data.ColumnApprearancelength);
                }
                result.FAppearanceValidLength = float.Parse(data.ColumnApprearanceValidlength);
                result.FAppearanceMaxRadius = float.Parse(data.ColumnApprearanceMaxRadius);
                result.FAppearanceMinRadius = float.Parse(data.ColumnApprearanceMinRadius);
                result.NSiliconLineNum = int.Parse(data.ColumnSiliconLineNum);
                result.StrAbnormalFir = data.ColumnAbnormalAreaFir;
                result.StrAbnormalSec = data.ColumnAbnormalAreaSec;
                result.StrAbnormalThr = data.ColumnAbnormalAreaThr;
                result.StrAbnormalFour = data.ColumnAbnormalAreaFour;
                result.StrDrawLineInfo = data.ColumnDrawLines;
                result.StrResultPath = data.ColumnResultPath;

                InspectResultLineNewForm form = new InspectResultLineNewForm(result, data.CurDate);
                form.ShowDialog();
            }
            catch(Exception ex)
            {

            }
            



        }

        private void buttonSearch_Click(object sender, System.EventArgs e)
        {

            if (uiDatePickerBegin.Text.Length <= 0 || uiDatePickerEnd.Text.Length <= 0)
            {
                MessageBox.Show("请设置开始与结束日期");
                return;
            }

            string[] strInfos = uiDatePickerBegin.Text.Split('-');
            int nYear = int.Parse(strInfos[0]);
            int nMonth = int.Parse(strInfos[1]);
            int nDay = int.Parse(strInfos[2]);

            DateTime beginDate = new DateTime(nYear, nMonth, nDay);

            strInfos = uiDatePickerEnd.Text.Split("-"); 
            nYear = int.Parse(strInfos[0]);
            nMonth = int.Parse(strInfos[1]);
            nDay = int.Parse(strInfos[2]);

            DateTime endDate = new DateTime(nYear, nMonth, nDay);


            List<InspectResultLineInfo> results = CMySQLTool.Instance().SearchResultByDate(beginDate, endDate);

            if (results.Count > 0)
            {
                datas.Clear();

                for (int i = 0; i < results.Count; i++)
                {
                    Data data = new Data();

                    data.ColumnSiliconStickLineNum = results[i].StrSiliconStickNum;
                    data.ColumnApprearancelength = results[i].FApperanceLength.ToString("0.00");
                    data.ColumnApprearanceValidlength = results[i].FAppearanceValidLength.ToString("0.00");
                    data.ColumnApprearanceMaxRadius = results[i].FAppearanceMaxRadius.ToString("0.00");
                    data.ColumnApprearanceMinRadius = results[i].FAppearanceMinRadius.ToString("0.00");
                    data.ColumnSiliconLineNum = results[i].NSiliconLineNum.ToString();
                    data.ColumnAbnormalAreaFir = results[i].StrAbnormalFir;
                    data.ColumnAbnormalAreaSec = results[i].StrAbnormalSec;
                    data.ColumnAbnormalAreaThr = results[i].StrAbnormalThr;
                    data.ColumnAbnormalAreaFour = results[i].StrAbnormalFour;
                    data.ColumnDrawLines =  results[i].StrDrawLineInfo;
                    data.CurDate = results[i].CurDate;
                    datas.Add(data);
                }

                uiDataGridView1.DataSource = datas;
                uiDataGridView1.Refresh();
            }
        }
    }
}