using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.SS.UserModel;
using SquareSiliconStickCheck.Tools;
using Sunny.UI;
using Sunny.UI.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SquareSiliconStickCheck.Pages.MainSquareCheckResultListPage;
using HalconDotNet;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Diagnostics.Eventing.Reader;

namespace SquareSiliconStickCheck.Pages
{
    public partial class Parameter : UIPage
    {
        public Parameter()
        {
            InitializeComponent();
          

        }
        private bool Frm_OnLogin(string userName, string password)
        {
            return userName == "njsc" && password == "123456";
        }
       
  
        private void Page_Click(object sender, EventArgs e)
        {
           
            UILoginForm frm = new UILoginForm();
            frm.ShowInTaskbar = true;
            frm.Text = "Login";
            frm.Title = "参数修正";
            frm.SubText = "";
            frm.OnLogin += Frm_OnLogin;
            frm.LoginImage = UILoginForm.UILoginImage.Login2;
            frm.ShowDialog();
            if (frm.IsLogin)
            {
                
                UIMessageTip.ShowOk("登录成功");
            }
            else
            {
                this.Parent = null;
            }

            frm.Dispose();
        }
    

        private void uiIntegerUpDown1_ValueChanged(object sender, int value)
        {
            if (uiIntegerUpDown1.Value<0)
            {
                uiIntegerUpDown1.Value = 0;
            }
            if (uiIntegerUpDown1.Value> 50)
            {
                uiIntegerUpDown1.Value = 50;

            }
            string jsonFilePath = "规格.json";
            using (StreamReader sr = new StreamReader(jsonFilePath))
            {
                string json = sr.ReadToEnd();
                string type = "规格" + uiIntegerUpDown1.Value;
                // 解析 JSON 字符串
                dynamic data = JsonConvert.DeserializeObject(json);
                dynamic yt = data[type];
                yt["切片模板"] = yt["切片模板"];
                uiTextBox1.Text = yt["A_边长_标准值"];
                uiTextBox24.Text = yt["A_边长_正容差"];
                uiTextBox32.Text = yt["A_边长_负容差"];
                uiTextBox16.Text = yt["A_边长_偏量修正"];
               
                uiTextBox9.Text = yt["B_边长_标准值"];
                uiTextBox21.Text = yt["B_边长_正容差"];
                uiTextBox29.Text = yt["B_边长_负容差"];
               uiTextBox13.Text= yt["B_边长_偏量修正"] ;

                uiTextBox57.Text=yt["C_边长_标准值"] ;
                uiTextBox53.Text=yt["C_边长_正容差"];
                uiTextBox51.Text=yt["C_边长_负容差"] ;
                uiTextBox55.Text=yt["C_边长_偏量修正"] ;

                uiTextBox56.Text=yt["D_边长_标准值"];
                uiTextBox52.Text=yt["D_边长_正容差"];
                uiTextBox50.Text=yt["D_边长_负容差"];
                uiTextBox54.Text=yt["D_边长_偏量修正"];

                uiTextBox20.Text = yt["对角线2_正容差"];
                uiTextBox28.Text = yt["对角线2_负容差"];
                uiTextBox5.Text= yt["对角线2_标准值"];
                uiTextBox12.Text=yt["对角线2_偏量修正"];

                uiTextBox11.Text=yt["对角线1_正容差"];
                uiTextBox8.Text=yt["对角线1_负容差"];
                uiTextBox27.Text=yt["对角线1_标准值"];
                uiTextBox19.Text=yt["对角线1_偏量修正"];

                uiTextBox18.Text = yt["上弧长_正容差"];
                uiTextBox26.Text = yt["上弧长_负容差"];
                uiTextBox4.Text = yt["上弧长_标准值"];
                uiTextBox10.Text=yt["上弧长_偏量修正"];

                uiTextBox22.Text = yt["下弧长_正容差"];
                uiTextBox30.Text = yt["下弧长_负容差"];
                uiTextBox3.Text  = yt["下弧长_标准值"];
                uiTextBox14.Text= yt["下弧长_偏量修正"];
               
                uiTextBox23.Text = yt["左弧长_正容差"];
                uiTextBox31.Text = yt["左弧长_负容差"];
                uiTextBox2.Text = yt["左弧长_标准值"] ;
                uiTextBox15.Text=yt["左弧长_偏量修正"];

                uiTextBox17.Text = yt["右弧长_正容差"];
                uiTextBox25.Text = yt["右弧长_负容差"];
                uiTextBox6.Text  = yt["右弧长_标准值"];
                uiTextBox7.Text = yt["右弧长_偏量修正"];

                uiTextBox87.Text = yt["上侧边垂直度_标准值"];
                uiTextBox79.Text = yt["上侧边垂直度_正容差"] ;
                uiTextBox75.Text = yt["上侧边垂直度_负容差"];
                uiTextBox83.Text = yt["上侧边垂直度_偏量修正"] ;

                uiTextBox88.Text = yt["下侧边垂直度_标准值"] ;
                uiTextBox80.Text = yt["下侧边垂直度_正容差"];
                uiTextBox76.Text = yt["下侧边垂直度_负容差"];
                uiTextBox84.Text = yt["下侧边垂直度_偏量修正"];

                uiTextBox89.Text = yt["左侧边垂直度_标准值"];
                uiTextBox81.Text = yt["左侧边垂直度_正容差"];
                uiTextBox77.Text = yt["左侧边垂直度_负容差"];
                uiTextBox85.Text = yt["左侧边垂直度_偏量修正"];

                uiTextBox86.Text = yt["右侧边垂直度_标准值"];
                uiTextBox78.Text = yt["右侧边垂直度_正容差"];
                uiTextBox74.Text = yt["右侧边垂直度_负容差"];
                uiTextBox82.Text = yt["右侧边垂直度_偏量修正"];

                uiTextBox91.Text = yt["端面垂直度_单面上限"];
                uiTextBox90.Text = yt["端面垂直度_双面上限"] ;
                uiTextBox93.Text = yt["端面垂直度_头部修正"] ;
                uiTextBox92.Text = yt["端面垂直度_尾部修正"];

                uiTextBox47.Text = yt["上弧长左投影_标准值"];
                uiTextBox43.Text = yt["上弧长左投影_偏量修正"];
                uiTextBox39.Text =yt["上弧长左投影_正容差"];
                uiTextBox35.Text = yt["上弧长左投影_负容差"];
                uiTextBox48.Text = yt["上弧长右投影_标准值"];
                uiTextBox44.Text = yt["上弧长右投影_偏量修正"];
                uiTextBox40.Text = yt["上弧长右投影_正容差"];
                uiTextBox36.Text = yt["上弧长右投影_负容差"];

                 uiTextBox49.Text = yt["下弧长左投影_标准值"];
                 uiTextBox45.Text = yt["下弧长左投影_偏量修正"];
                 uiTextBox41.Text = yt["下弧长左投影_正容差"];
                 uiTextBox37.Text = yt["下弧长左投影_负容差"];
                 uiTextBox46.Text = yt["下弧长右投影_标准值"];
                 uiTextBox42.Text = yt["下弧长右投影_偏量修正"];
                 uiTextBox38.Text = yt["下弧长右投影_正容差"];
                 uiTextBox34.Text = yt["下弧长右投影_负容差"];

                 uiTextBox71.Text = yt["左弧长左投影_标准值"];
                 uiTextBox67.Text = yt["左弧长左投影_偏量修正"];
                 uiTextBox63.Text = yt["左弧长左投影_正容差"];
                 uiTextBox59.Text = yt["左弧长左投影_负容差"];
                 uiTextBox72.Text = yt["左弧长右投影_标准值"];
                 uiTextBox68.Text = yt["左弧长右投影_偏量修正"];
                 uiTextBox64.Text = yt["左弧长右投影_正容差"];
                 uiTextBox60.Text = yt["左弧长右投影_负容差"];

                 uiTextBox73.Text = yt["右弧长左投影_标准值"] ;
                 uiTextBox69.Text = yt["右弧长左投影_偏量修正"];
                 uiTextBox65.Text = yt["右弧长左投影_正容差"];
                 uiTextBox61.Text = yt["右弧长左投影_负容差"];
                 uiTextBox70.Text = yt["右弧长右投影_标准值"];
                 uiTextBox66.Text = yt["右弧长右投影_偏量修正"];
                 uiTextBox62.Text = yt["右弧长右投影_正容差"];
                 uiTextBox58.Text = yt["右弧长右投影_负容差"];


            }

        }



        private void paly_Click(object sender, EventArgs e)
        {
            string jsonFilePath = "规格.json";
            using (StreamReader sr = new StreamReader(jsonFilePath))
            {
                try
                {
                    string json = sr.ReadToEnd();
                    string type = "规格" + uiIntegerUpDown1.Value;
                    // 解析 JSON 字符串
                    dynamic data = JsonConvert.DeserializeObject(json);
                    dynamic yt = data[type];
                    yt["切片模板"] = yt["切片模板"];
                    yt["A_边长"] = yt["A_边长"];
                    yt["B_边长"] = yt["B_边长"];
                    yt["对角1"] = yt["对角1"];
                    yt["对角2"] = yt["对角2"];
                    yt["A_边长_标准值"] = uiTextBox1.Text;
                    yt["A_边长_正容差"] = uiTextBox24.Text;
                    yt["A_边长_负容差"] = uiTextBox32.Text;
                    yt["A_边长_偏量修正"] = uiTextBox16.Text;

                    yt["B_边长_标准值"] = uiTextBox9.Text;
                    yt["B_边长_正容差"] = uiTextBox21.Text;
                    yt["B_边长_负容差"] = uiTextBox29.Text;
                    yt["B_边长_偏量修正"] = uiTextBox13.Text;

                    yt["C_边长_标准值"] = uiTextBox57.Text;
                    yt["C_边长_正容差"] = uiTextBox53.Text;
                    yt["C_边长_负容差"] = uiTextBox51.Text;
                    yt["C_边长_偏量修正"] = uiTextBox55.Text;

                    yt["D_边长_标准值"] = uiTextBox56.Text;
                    yt["D_边长_正容差"] = uiTextBox52.Text;
                    yt["D_边长_负容差"] = uiTextBox50.Text;
                    yt["D_边长_偏量修正"] = uiTextBox54.Text;

                    yt["对角线2_正容差"] = uiTextBox20.Text;
                    yt["对角线2_负容差"] = uiTextBox28.Text;
                    yt["对角线2_标准值"] = uiTextBox5.Text;
                    yt["对角线2_偏量修正"] = uiTextBox12.Text;

                    yt["对角线1_正容差"] = uiTextBox11.Text;
                    yt["对角线1_负容差"] = uiTextBox8.Text;
                    yt["对角线1_标准值"] = uiTextBox27.Text;
                    yt["对角线1_偏量修正"] = uiTextBox19.Text;

                    yt["上弧长_正容差"] = uiTextBox18.Text;
                    yt["上弧长_负容差"] = uiTextBox26.Text;
                    yt["上弧长_标准值"] = uiTextBox4.Text;
                    yt["上弧长_偏量修正"] = uiTextBox10.Text;

                    yt["下弧长_正容差"] = uiTextBox22.Text;
                    yt["下弧长_负容差"] = uiTextBox30.Text;
                    yt["下弧长_标准值"] = uiTextBox3.Text;
                    yt["下弧长_偏量修正"] = uiTextBox14.Text;

                    yt["左弧长_正容差"] = uiTextBox23.Text;
                    yt["左弧长_负容差"] = uiTextBox31.Text;
                    yt["左弧长_标准值"] = uiTextBox2.Text;
                    yt["左弧长_偏量修正"] = uiTextBox15.Text;

                    yt["右弧长_正容差"] = uiTextBox17.Text;
                    yt["右弧长_负容差"] = uiTextBox25.Text;
                    yt["右弧长_标准值"] = uiTextBox6.Text;
                    yt["右弧长_偏量修正"] = uiTextBox7.Text;

                    yt["右弧长_正容差"] = uiTextBox17.Text;
                    yt["右弧长_负容差"] = uiTextBox25.Text;
                    yt["右弧长_标准值"] = uiTextBox6.Text;
                    yt["右弧长_偏量修正"] = uiTextBox7.Text;

                    yt["上侧边垂直度_标准值"] = uiTextBox87.Text;
                    yt["上侧边垂直度_正容差"] = uiTextBox79.Text;
                    yt["上侧边垂直度_负容差"] = uiTextBox75.Text;
                    yt["上侧边垂直度_偏量修正"] = uiTextBox83.Text;

                    yt["下侧边垂直度_标准值"] = uiTextBox88.Text;
                    yt["下侧边垂直度_正容差"] = uiTextBox80.Text;
                    yt["下侧边垂直度_负容差"] = uiTextBox76.Text;
                    yt["下侧边垂直度_偏量修正"] = uiTextBox84.Text;

                    yt["左侧边垂直度_标准值"] = uiTextBox89.Text;
                    yt["左侧边垂直度_正容差"] = uiTextBox81.Text;
                    yt["左侧边垂直度_负容差"] = uiTextBox77.Text;
                    yt["左侧边垂直度_偏量修正"] = uiTextBox85.Text;

                    yt["右侧边垂直度_标准值"] = uiTextBox86.Text;
                    yt["右侧边垂直度_正容差"] = uiTextBox78.Text;
                    yt["右侧边垂直度_负容差"] = uiTextBox74.Text;
                    yt["右侧边垂直度_偏量修正"] = uiTextBox82.Text;

                    yt["端面垂直度_单面上限"] = uiTextBox91.Text;
                    yt["端面垂直度_双面上限"] = uiTextBox90.Text;
                    yt["端面垂直度_头部修正"] = uiTextBox93.Text;
                    yt["端面垂直度_尾部修正"] = uiTextBox92.Text;


                    yt["上弧长左投影_标准值"] = uiTextBox47.Text;
                    yt["上弧长左投影_偏量修正"] = uiTextBox43.Text;
                    yt["上弧长左投影_正容差"] = uiTextBox39.Text;
                    yt["上弧长左投影_负容差"] = uiTextBox35.Text;
                    yt["上弧长右投影_标准值"] = uiTextBox48.Text;
                    yt["上弧长右投影_偏量修正"] = uiTextBox44.Text;
                    yt["上弧长右投影_正容差"] = uiTextBox40.Text;
                    yt["上弧长右投影_负容差"] = uiTextBox36.Text;

                    yt["下弧长左投影_标准值"] = uiTextBox49.Text;
                    yt["下弧长左投影_偏量修正"] = uiTextBox45.Text;
                    yt["下弧长左投影_正容差"] = uiTextBox41.Text;
                    yt["下弧长左投影_负容差"] = uiTextBox37.Text;
                    yt["下弧长右投影_标准值"] = uiTextBox46.Text;
                    yt["下弧长右投影_偏量修正"] = uiTextBox42.Text;
                    yt["下弧长右投影_正容差"] = uiTextBox38.Text;
                    yt["下弧长右投影_负容差"] = uiTextBox34.Text;

                    yt["左弧长左投影_标准值"] = uiTextBox71.Text;
                    yt["左弧长左投影_偏量修正"] = uiTextBox67.Text;
                    yt["左弧长左投影_正容差"] = uiTextBox63.Text;
                    yt["左弧长左投影_负容差"] = uiTextBox59.Text;
                    yt["左弧长右投影_标准值"] = uiTextBox72.Text;
                    yt["左弧长右投影_偏量修正"] = uiTextBox68.Text;
                    yt["左弧长右投影_正容差"] = uiTextBox64.Text;
                    yt["左弧长右投影_负容差"] = uiTextBox60.Text;

                    yt["右弧长左投影_标准值"] = uiTextBox73.Text;
                    yt["右弧长左投影_偏量修正"] = uiTextBox69.Text;
                    yt["右弧长左投影_正容差"] = uiTextBox65.Text;
                    yt["右弧长左投影_负容差"] = uiTextBox61.Text;
                    yt["右弧长右投影_标准值"] = uiTextBox70.Text;
                    yt["右弧长右投影_偏量修正"] = uiTextBox66.Text;
                    yt["右弧长右投影_正容差"] = uiTextBox62.Text;
                    yt["右弧长右投影_负容差"] = uiTextBox58.Text;

                    string jsontext = JsonConvert.SerializeObject(data);
                    sr.Close();
                    WriteJsonFile(jsonFilePath, jsontext);
                    UIMessageTip.ShowOk("参数导入成功！");
                }
                catch (Exception)
                {
                    UIMessageTip.ShowError("参数导入失败！");  
                   
                }

               
            }
          



        }
        private void WriteJsonFile(string path, string jsonConents)
        {
            File.WriteAllText(path, jsonConents, System.Text.Encoding.UTF8);
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            try
            {
                //HDevelopExport.DisInstance();
                
                //HDevelopExport.Instance();
                UIMessageTip.ShowOk("算法加载成功成功！");
            }
            catch (Exception)
            {
                UIMessageTip.ShowError("算法加载失败！");
            }

           
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            if (uiComboBox1.Text == "admin" && uiTextBox33.Text == "123456")
            {
                uiIntegerUpDown1.Enabled = true;
                paly.Enabled = true;
                uiButton1.Enabled = true;
                UIMessageTip.ShowOk("登录成功！");

            }
            else 
            {
                UIMessageTip.ShowError("登录失败！请输入正确的密码！");
            }
        }
    }
    }
