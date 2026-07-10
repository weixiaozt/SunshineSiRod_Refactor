using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts;
using Sunny.UI;
using System.Windows.Media;
using System.Diagnostics;
using System.Collections.Generic;
using SquareSiliconStickCheck.Parameters;
using SquareSiliconStickCheck.Tools;

namespace SquareSiliconStickCheck.Pages
{
    public partial class MainResultListPage : UIPage
    {
        List<Data> datas = new List<Data>();
        List<InspectResult> _inspectResults = new List<InspectResult>();
        private bool _bFirstRun = true;

        public delegate void ShowRightButtonDelegate(bool bShow);
        public ShowRightButtonDelegate ShowRightBtnFunc;


        public static MainResultListPage instance;
        public MainResultListPage()
        {
            instance = this;
            InitializeComponent();
            ShowRightBtnFunc = new ShowRightButtonDelegate(ShowRightButton);
            //_inspectResults =  CMySQLTool.Instance().SelectResult();
            uiDataGridView1.AddColumn("ID", "ColumnID");
            uiDataGridView1.AddColumn("Result", "ColumnResult");
            uiDataGridView1.AddColumn("隐裂", "ColumnYinLie");
            uiDataGridView1.AddColumn("应力", "ColumnYINGLI");
            uiDataGridView1.AddColumn("时间", "ColumnCHECKTIME");


            uiDataGridView1.Init();
            for (int i = 0; i < _inspectResults.Count; i++)
            {
                Data data = new Data();
                data.ColumnID = _inspectResults[i].NID.ToString();
                data.ColumnResult = _inspectResults[i].NResult.ToString();
                data.ColumnYINLIE = _inspectResults[i].StrFileYinLie;
                data.ColumnYINGLI = _inspectResults[i].StrFileYingLi;
                data.ColumnCHECKTIME = _inspectResults[i].Curcheck.ToString();

                datas.Add(data);
            }

            //设置分页控件总数
            //设置分页控件每页数量
            //uiPagination1.PageSize = 5;
           
            uiPagination1.TotalCount = datas.Count;
            uiDataGridView1.DataSource = datas;
            
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
            public string ColumnID { get; set; }

            public string ColumnResult { get; set; }

            public string ColumnYINLIE { get; set; }

            public string ColumnYINGLI { get; set; }

            public string ColumnCHECKTIME { get; set; }
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

            InspectResult result = new InspectResult();

            result.NID = int.Parse(data.ColumnID);
            result.NResult = int.Parse(data.ColumnResult);
            result.StrFileYinLie = data.ColumnYINLIE;
            result.StrFileYingLi = data.ColumnYINGLI;
            result.Curcheck =  System.DateTime.Parse( data.ColumnCHECKTIME);



            InspectResultNewForm form = new InspectResultNewForm(result);
            form.ShowDialog();
        }

        private void buttonSearch_Click(object sender, System.EventArgs e)
        {
            List<InspectResult> results = CMySQLTool.Instance().SearchResult(dateTimePickerbegin.Value, dateTimePickerEnd.Value);

            if (results.Count > 0)
            {
                datas.Clear();

                for (int i = 0; i < results.Count; i++)
                {
                    Data data = new Data();
                    data.ColumnID = results[i].NID.ToString();
                    data.ColumnResult = results[i].NResult.ToString();
                    data.ColumnYINLIE = results[i].StrFileYinLie;
                    data.ColumnYINGLI = results[i].StrFileYingLi;
                    data.ColumnCHECKTIME = results[i].Curcheck.ToString();

                    datas.Add(data);
                }

                uiDataGridView1.DataSource = datas;
            }
        }
    }
}