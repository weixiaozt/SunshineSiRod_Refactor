using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;
using MathNet.Numerics.Random;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using OpenCvSharp;
using SiliconRoundBarCheck.Data;
using SquareSiliconStickCheck.Parameters;
using SquareSiliconStickCheck.Tools;
using Sunny.UI;



namespace SquareSiliconStickCheck.Pages
{
    public partial class ProcessSquareAcquisition :  UIPage
    {
        private SquareStickCheckData _curresult;

        public SquareStickCheckData Curresult { get => _curresult; set => _curresult = value; }
        Thread _threadCollection = null;
        public ProcessSquareAcquisition()
        {
            
           
            InitializeComponent();
          
           
        }


        #region 
        //private void buttonStart_Click(object sender, EventArgs e)
        //{
        //    float fStandLTLength = 0;
        //    float fStandRTLength = 0;
        //    float fStandLDLength = 0;
        //    float fStandRDLength = 0;
        //    float fStandTDLength = 0;
        //    float fStandLRLength = 0;
        //    int nSquareType = 0;
        //    float.TryParse(textBoxLTLength.Text, out fStandLTLength);
        //    float.TryParse(textBoxRTLength.Text, out fStandRTLength);
        //    float.TryParse(textBoxLDLength.Text, out fStandLDLength);
        //    float.TryParse(textBoxRDLength.Text, out fStandRDLength);
        //    float.TryParse(textBoxTDLength.Text, out fStandTDLength);
        //    float.TryParse(textBoxLRLength.Text, out fStandLRLength);
        //    int.TryParse(textBoxSquareType.Text, out nSquareType);

        //    _threadCollection = new Thread(() =>
        //    {
        //        if (SettingParameter.Instance().NDaemon == 0)
        //        {
        //            ProcessManager.Instance().ScanSquareSiliconStickCollectionMockTread(fStandLTLength, fStandRTLength, fStandTDLength, fStandLRLength, nSquareType);
        //        }
        //        else
        //        {
        //            ProcessManager.Instance().ScanSquareSiliconStickCollectionTread(fStandLTLength, fStandRTLength, fStandTDLength, fStandLRLength, nSquareType);
        //        }
        //    });
        //    _threadCollection.Start();
        //}
        #endregion
        private void test_Click(object sender, EventArgs e)
        {

            var f = new OpenFileDialog();
            //f.Multiselect = true; //多选
            if (f.ShowDialog() == DialogResult.OK)
            {
                String filepath = f.FileName;//G:\新建文件夹\新建文本文档.txt
                String filename = f.SafeFileName;//新建文本文档.txt
                this.uiTextBox8.Text = filepath;
            }


        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            try
            {
                HOperatorSet.ReadImage(out HObject image, uiTextBox8.Text);
                HOperatorSet.ReadDict("./规格.json", null, null, out HTuple dictHandle);
                String TYPE = "规格" + uiIntegerUpDown1.Value;
                HDevelopExport.Instance().Calibration(image, out HTuple dictHandle2);
                HOperatorSet.GetDictTuple(dictHandle, TYPE, out HTuple dictHandle0);
                HOperatorSet.SetDictTuple(dictHandle0, "切片模板", dictHandle2);
                HOperatorSet.SetDictTuple(dictHandle0, "A_边长", textBoxLTLength.Text.ToFloat());
                HOperatorSet.SetDictTuple(dictHandle0, "B_边长", textBox1.Text.ToFloat());
                HOperatorSet.SetDictTuple(dictHandle0, "C_边长", textBox4.Text.ToFloat());
                HOperatorSet.SetDictTuple(dictHandle0, "D_边长", textBox2.Text.ToFloat());


                HOperatorSet.SetDictTuple(dictHandle0, "对角1", textBoxTDLength.Text.ToFloat());
                HOperatorSet.SetDictTuple(dictHandle0, "对角2", textBox3.Text.ToFloat());

                
               
                //HOperatorSet.SetDictTuple(dictHandle, TYPE, dictHandle0);
                HOperatorSet.DictToJson(dictHandle, null, null, out HTuple jsonString);
                HOperatorSet.WriteDict(dictHandle, "./规格", "file_type", "json");
                UIMessageTip.ShowOk("标定完成！");
            }
            catch (Exception)
            {
                UIMessageTip.ShowError("标定失败！");

            }
        }

        private void uiIntegerUpDown1_ValueChanged(object sender, int value)
        {
            if (uiIntegerUpDown1.Value < 0)
            {
                uiIntegerUpDown1.Value = 0;
            }
            if (uiIntegerUpDown1.Value > 50)
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
                textBoxLTLength.Text = yt["A_边长"];
                textBox1.Text = yt["B_边长"];
                textBoxTDLength.Text = yt["对角1"];

                textBox4.Text = yt["C_边长"];
                textBox2.Text = yt["D_边长"];
                textBox3.Text = yt["对角2"];
            }  
            }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (uiComboBox1.Text == "admin" && uiTextBox33.Text == "123456")
            {
                uiIntegerUpDown1.Enabled = true;
                uiButton2.Enabled = true;
                uiButton1.Enabled = true;
                UIMessageTip.ShowOk("登录成功！");

            }
            else
            {
                UIMessageTip.ShowError("登录失败！请输入正确的密码！");
            }
        }

        private void ProcessSquareAcquisition_Initialize(object sender, EventArgs e)
        {

        }
    }
               
                     
                   
            

        }


