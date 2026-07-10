using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SquareSiliconStickCheck.Pages;
using SquareSiliconStickCheck.Tools;
using SquareSiliconStickCheck.Parameters;
using Sunny.UI;
using HalconDotNet;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using SiliconRoundBarCheck.Cameras;
//using SquareSiliconStickCheck.Cameras.XG;
using SquareSiliconStickCheck.Data;
using NPOI.XWPF.Usermodel;
using System.Net.Http;
using ICSharpCode.SharpZipLib.Zip;
using System.Text.Json;

namespace SquareSiliconStickCheck
{
    public partial class FormMain : UIAsideMainFrame
    {

        public enum emMSGTYPE
        {
            EM_MODBUSDISCONNECT = 1,
            EM_HIKCAMERAVIDEO_DISCONNECT = 2,
            EM_HIKCAMERALINE_DISCONNECT = 3,
            EM_BVCAMERAYINLIE_DISCONNECT = 4,
            EM_BVCAMERAYINGLI_DISCONNECT = 5,
            EM_SSZNCAMERA_DISCONNECT = 6,
            EM_PROGRAME_RESTART = 7,
            EM_SSZNCAMERA_EXCEPTION = 8,
            EM_ARMMODBUSDISCONNECT = 9
        }
        
        public delegate void MessageInfo(string strMsg, int nMsgType = 0);

        public delegate void ShowAsideDelegate();

        public static FormMain formMainF;

        public MessageInfo showMessageDelegate;

        private Thread _threadCloseWindows;
        private Thread _xt;
        private Thread _MesReciver;

        public ShowAsideDelegate showAsideFunc;


        public void ShowAside()
        {
            this.Aside.Show();
            this.buttonLeft.Show();

            this.Aside.Refresh();
            this.buttonLeft.Refresh();
        }

        public FormMain()
        {
            InitializeComponent();
          

            _threadCloseWindows = new Thread(FindWindowsAndClose);
            _threadCloseWindows.Start();
            //_xt = new Thread(xt);
            //_xt.Start();//心跳线程

            formMainF = this;
            showMessageDelegate = new MessageInfo(MessageBInfo);
            showAsideFunc = new ShowAsideDelegate(ShowAside);
            SettingParameter.Instance().Init();
           HObject obj = new HObject();
            HOperatorSet.GenEmptyObj(out obj);
            Control.CheckForIllegalCrossThreadCalls = false;
            if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
            {
                _MesReciver = new Thread(MesReciver);
                _MesReciver.Start();
                CPointLaserTools.Instance().InitComs(SettingParameter.Instance().NTopCom, SettingParameter.Instance().NLeftCom, SettingParameter.Instance().NRightCom, 115200);

                if (SettingParameter.Instance().NCamType == 0)
                {
                    SSZNCamTools.Instance().InitCamTools();
                    //SSZNCamTools.Instance().InitReferenceObjectInfo();
                }
                else
                {
                    //XGCamTools.Instance.InitCamTools();
                    //XGCamTools.Instance.InitReferenceObjectInfo();
                }


            }
            else
            {
                _MesReciver = new Thread(MesReciver);
                _MesReciver.Start();
                SSZNCamTools.Instance().InitReferenceObjectInfo();
                //XGCamTools.Instance.InitReferenceObjectInfo();
            }
            Initalgorithm();


            LogHelper.Info("RedisLog", "SubScribe SQL Begin");
            CMySQLTool.Instance().Connect(SettingParameter.Instance().StrMySQLIP, SettingParameter.Instance().NMySQLPort, SettingParameter.Instance().StrMySQLUser, SettingParameter.Instance().StrMySQLPwd, SettingParameter.Instance().StrMySQLDBName);
             LogHelper.Info("RedisLog", "SubScribe SQL End");
            Aside.TabControl = MainTabControl;

            AddPage(new ProcessManagerCheckPage(), 1001);
            AddPage(new MainSquareCheckResultListPage(), 1002);
            AddPage(new SettingParamPage(), 1003);
            AddPage(new ProcessSquareAcquisition(), 1004);
            AddPage(new Parameter(), 1005);

            Aside.CreateNode("Process", 1001);
            //Aside.CreateNode("Inquire", 1002);
            //Aside.CreateNode("配置", 1003);
            Aside.CreateNode("Data calibration", 1004);
            Aside.CreateNode("Setting", 1005);
            Aside.SelectFirst();

        }

     


        public const int WM_CLOSE = 0x10;
        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        private void FindWindowsAndClose()
        {
           
            while(true)
            {
                IntPtr hwnd_win;
                hwnd_win = FindWindow(null, "HALCON Error");
                if (hwnd_win != IntPtr.Zero)
                {
                    SendMessage(hwnd_win, WM_CLOSE, 0, 0);
                    break;
                }

                Thread.Sleep(1000);
            }

        }
        private bool ConnectArmModBus()
        {
            if (false == CArmControllerModbusTool.Instance().ConnectServer())
            {
                showMessageDelegate.Invoke("连接运动控制模块失败,请检查网络！", (int)emMSGTYPE.EM_ARMMODBUSDISCONNECT);
                return false;
            }

            return true;
        }
        private bool ConnectModBus()
        {
            if (false == CMoveControllerModbusTool.Instance().ConnectServer())
            {
                showMessageDelegate.Invoke("连接运动控制模块失败,请检查网络！", (int)emMSGTYPE.EM_MODBUSDISCONNECT);
                return false;
            }

            return true;
        }
       
        public void MessageBInfo(string strMsg, int nMsgType = 0)
        {
            //if (nMsgType == 1)
            //{
            //    MessageBox.Show(strMsg);
            //}
            //else
            //{
                //if (DialogResult.Yes == MessageBox.Show(strMsg, "提示信息", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification))
                //{
                    switch (nMsgType)
                    {
                        case (int)emMSGTYPE.EM_MODBUSDISCONNECT:
                            {
                                ConnectModBus();
                                break;
                            }
                        case (int)emMSGTYPE.EM_ARMMODBUSDISCONNECT:
                            {
                                ConnectArmModBus();
                                break;
                            }
                        case (int)emMSGTYPE.EM_BVCAMERAYINGLI_DISCONNECT:
                            {
                                 ProcessManagerPage.instance.initDevFunc((int)ProcessManagerPage.emCameraType.EM_BVYINGLI);
                                break;
                            }
                        case (int)emMSGTYPE.EM_BVCAMERAYINLIE_DISCONNECT:
                            {
                                ProcessManagerPage.instance.initDevFunc((int)ProcessManagerPage.emCameraType.EM_BVYINLIE);
                                break;
                            }
                        case (int)emMSGTYPE.EM_PROGRAME_RESTART:
                            {

                                //string strExeFilePath = System.Windows.Forms.Application.ExecutablePath;
                                //System.Diagnostics.Process p = new System.Diagnostics.Process();
                                //p.StartInfo.FileName = strExeFilePath;   //exe程序文件地址
                                //p.StartInfo.UseShellExecute = false;
                                //p.StartInfo.RedirectStandardInput = false;
                                //p.StartInfo.RedirectStandardOutput = false;
                                //p.StartInfo.RedirectStandardError = false;
                                //p.StartInfo.CreateNoWindow = true;                  //不弹出窗口，改为后台运行
                                //p.StartInfo.Arguments = sb.ToString();  //参数 多个参数使用空格分开               
                                //p.Start();

                                Application.Exit();

                                break;
                            }
                        case (int)emMSGTYPE.EM_SSZNCAMERA_EXCEPTION:
                            {
                                this.Invoke(ProcessManagerPage.instance.stopThradFunc);
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                //}

            //}

        }

        private IntPtr ArrToPtr(byte[] array)
        {
            return System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
        }

  
  
        private void Initalgorithm()
        {

            HTuple ParmDict = new HTuple();
            HTuple ResourceDictHandle = new HTuple();
            HTuple JSONData = new HTuple();
            HTuple Ex = new HTuple();
            HTuple la = new HTuple();
            HTuple gg = new HTuple();
            HDevelopExport.Instance().Detection_init(ParmDict, out ResourceDictHandle, out JSONData, out Ex);
            HOperatorSet.GetDictTuple(ResourceDictHandle, "规格0", out gg);
            HOperatorSet.GetDictTuple(gg,"A_边长",out la);
            LogHelper.Info("Silicon", "Initalgorithm Success!");
            ResouceData YU=new ResouceData();
           

        }

        private void buttonLeft_Click(object sender, EventArgs e)
        {
            this.Aside.Hide();
            this.buttonLeft.Visible = false;

            ProcessManagerCheckPage.instance.ShowRightBtnFunc(true);
            SettingParamPage.instance.ShowRightBtnFunc(true);
        }

        private void xt()//心跳
        {
            while (true)
            {
                Thread.Sleep(800);
                try
                {
                    CMoveControllerModbusTool.Instance().WriteSingleCoil(2203, true);//心跳
                    Thread.Sleep(800);
                    CMoveControllerModbusTool.Instance().WriteSingleCoil(2203, false);//心跳
                }
                catch (Exception ex)
                {

                }
            }


        }
        private void MesReciver()//mes
        {
           
        }
        private void Node_Click(object sender, EventArgs e)
        {
            UILoginForm frm = new UILoginForm();
            frm.ShowInTaskbar = true;
            frm.Text = "Login";
            frm.Title = "SunnyUI.Net Login Form";
            frm.SubText = Version;
            frm.OnLogin += Frm_OnLogin;
            frm.LoginImage = UILoginForm.UILoginImage.Login2;
            frm.ShowDialog();
            if (frm.IsLogin)
            {
                UIMessageTip.ShowOk("登录成功");
            }

            frm.Dispose();
        }
        private bool Frm_OnLogin(string userName, string password)
        {
            return userName == "admin" && password == "123456";
        }

        private void FormMain_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(0);
        }


        private string _token;
        private HttpClient _httpClient;
        private void FormMain_Load(object sender, EventArgs e)
        {
            //_httpClient = new HttpClient();
            //_httpClient.Timeout = TimeSpan.FromSeconds(30);

            //string url = "http://suzhou.orbitmes.com:18734/api/OrbitWebAPILogin";
            //url += "?APIName=BrickVisionInspectionDataPilot";
            //url += "&UserName=NDK";
           // url += "&Password=123456";

            // 同步调用 HttpClient
            //var response = _httpClient.PostAsync(url, null).GetAwaiter().GetResult();

            // 同步获取响应内容
            //string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //if (response.IsSuccessStatusCode)
            //{
             //   var jsonDoc = JsonDocument.Parse(responseContent);
             //   if (jsonDoc.RootElement.TryGetProperty("token", out JsonElement tokenElement))
            //    {
            //        _token = tokenElement.GetString();
             //   }
            //}
            //else
            //{
              //  MessageBox.Show($"\n\n错误: 请求失败，状态码 {response.StatusCode}");
            //}
        }
    }
}
