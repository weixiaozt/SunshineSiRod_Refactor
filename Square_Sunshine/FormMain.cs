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
            EM_SSZNCAMERA_EXCEPTION = 8
        }

        public delegate void MessageInfo(string strMsg, int nMsgType = 0);

        public delegate void ShowAsideDelegate();

        public static FormMain formMainF;

        public MessageInfo showMessageDelegate;

        private Thread _threadCloseWindows;

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
            formMainF = this;
            showMessageDelegate = new MessageInfo(MessageBInfo);
            showAsideFunc = new ShowAsideDelegate(ShowAside);
            SettingParameter.Instance().Init();



            
            if (SettingParameter.Instance().NDaemon == 1)
            {
                if (false == CMoveControllerModbusTool.Instance().ConnectServer())
                {
                    showMessageDelegate.Invoke("连接运动控制模块失败,请检查网络！", (int)emMSGTYPE.EM_MODBUSDISCONNECT);
                }

                   
               
            }
            else if (SettingParameter.Instance().NDaemon == 3)
            {
               
            }
            else if (SettingParameter.Instance().NDaemon == 2)
            {
               
            }
            else if (SettingParameter.Instance().NDaemon == 4)
            {
              
            }

             LogHelper.Info("RedisLog", "SubScribe SQL Begin");
            CMySQLTool.Instance().Connect(SettingParameter.Instance().StrMySQLIP, SettingParameter.Instance().NMySQLPort, SettingParameter.Instance().StrMySQLUser, SettingParameter.Instance().StrMySQLPwd, SettingParameter.Instance().StrMySQLDBName);
             LogHelper.Info("RedisLog", "SubScribe SQL End");
            Aside.TabControl = MainTabControl;

            AddPage(new ProcessManagerPage(), 1001);
           
            AddPage(new MainResultListPage(), 1002);
            AddPage(new SettingParamPage(), 1003);

            Aside.CreateNode("测试流程", 1001);
            Aside.CreateNode("结果查询", 1002);
            Aside.CreateNode("配置", 1003);

          
            
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
            if (nMsgType == 0)
            {
                MessageBox.Show(strMsg);
            }
            else
            {
                if (DialogResult.Yes == MessageBox.Show(strMsg, "提示信息", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification))
                {
                    switch (nMsgType)
                    {
                        case (int)emMSGTYPE.EM_MODBUSDISCONNECT:
                            {
                                ConnectModBus();
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
                }

            }

        }

        private IntPtr ArrToPtr(byte[] array)
        {
            return System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
        }

        private void Test()
        {
            int nTestID = 1;
           
            string strSaveDir = SettingParameter.Instance().StrSaveDir;

            strSaveDir += "/" + nTestID.ToString();

            if (false == Directory.Exists(strSaveDir))
            {
                Directory.CreateDirectory(strSaveDir);
            }

            string strFileLJ_1 = "";
            string strFileLJ_2 = "";
            string strFileLJ_3 = "";
            string strFileLJ_4 = "";
            string strFileYinLie_1 = "";
            string strFileYinLie_2 = "";
            string strFileYinLie_3 = "";
            string strFileYinLie_4 = "";
            string strFileYingLi_1 = "";
            string strFileYingLi_2 = "";
            string strFileYingLi_3 = "";
            string strFileYingLi_4 = "";

            strFileLJ_1 = strSaveDir + "/LJ_1.bmp";
               
            strFileLJ_2 = strSaveDir + "/LJ_2.bmp";
               
            strFileLJ_3 = strSaveDir + "/LJ_3.bmp";
               
            strFileLJ_4 = strSaveDir + "/LJ_4.bmp";
               
            strFileYinLie_1 = strSaveDir + "/YinLie_1.bmp";
               
            strFileYinLie_2 = strSaveDir + "/YinLie_2.bmp";
                
            strFileYinLie_3 = strSaveDir + "/YinLie_3.bmp";
                
            strFileYinLie_4 = strSaveDir + "/YinLie_4.bmp";
               
            strFileYingLi_1 = strSaveDir + "/YingLi_1.bmp";
               
            strFileYingLi_2 = strSaveDir + "/YingLi_2.bmp";
                
            strFileYingLi_3 = strSaveDir + "/YingLi_3.bmp";
                
            strFileYingLi_4 = strSaveDir + "/YingLi_4.bmp";

            string strInfo = "2,10;";

            CMySQLTool.Instance().InsertResult(0,  strFileYinLie_1,  strFileYingLi_1);
            
        }
        private void InitTools()
        {
            
            //CHikCameraTools.Instance().Init();
           

           
             LogHelper.Info("Silicon","InitTools Success!");

            

        }

        private void buttonLeft_Click(object sender, EventArgs e)
        {
            this.Aside.Hide();
            this.buttonLeft.Visible = false;

            ProcessManagerPage.instance.ShowRightBtnFunc(true);
            SettingParamPage.instance.ShowRightBtnFunc(true);
        }

      
    }
}
