using Sunny.UI;
using SquareSiliconStickCheck.Parameters;
using System;
using SquareSiliconStickCheck.Tools;

namespace SquareSiliconStickCheck.Pages
{
    public partial class SettingParamPage : UIPage
    {
        public delegate void ShowRightButtonDelegate(bool bShow);

        public ShowRightButtonDelegate ShowRightBtnFunc;

        public static SettingParamPage instance;


        public SettingParamPage()
        {
            try
            {
                instance = this;
                InitializeComponent();
                ShowRightBtnFunc = new ShowRightButtonDelegate(ShowRightButton);
                SettingParameter.Instance().Init();
                tb3DLaserLeftIP.Text = SettingParameter.Instance().StrLeftLaser3DIP;
                tb3DLaserRightIP.Text = SettingParameter.Instance().StrRightLaser3DIP;
                tb3DLaserDownIP.Text = SettingParameter.Instance().StrDownLaser3DIP;
                tb3DLaserTopIP.Text = SettingParameter.Instance().StrTopLaser3DIP;

                tbMoveControlIP.Text = SettingParameter.Instance().StrIPMoveControl;
                tbMoveControlPort.Text = SettingParameter.Instance().NMoveControlPort.ToString();

                //tbSaveDir.Text = SettingParameter.Instance().StrSaveDir;
                numericUpDownTDLength.Value = new decimal(SettingParameter.Instance().FModTDLength);
                numericUpDownLRLength.Value = new decimal(SettingParameter.Instance().FModLRLength);
                numericUpDownLTLength.Value = new decimal(SettingParameter.Instance().FModLTLength);
                numericUpDownRTLength.Value = new decimal(SettingParameter.Instance().FModRTLength);
                textBoxCheckMinLength.Text = SettingParameter.Instance().FCheckMinLength.ToString("0.00");
                textBoxCheckMinVerticalLength.Text = SettingParameter.Instance().FCheckMinVerticalLength.ToString("0.00");
            }
            catch(Exception ex)
            {

            }
            

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

        private void button1_Click(object sender, System.EventArgs e)
        {
            try
            {
                SettingParameter.Instance().StrLeftLaser3DIP = tb3DLaserLeftIP.Text;
                SettingParameter.Instance().StrRightLaser3DIP = tb3DLaserRightIP.Text;
                SettingParameter.Instance().StrDownLaser3DIP = tb3DLaserDownIP.Text;
                SettingParameter.Instance().StrTopLaser3DIP = tb3DLaserTopIP.Text;
                //SettingParameter.Instance().NMoveControlPort = int.Parse(tb3DLaserLeftPort.Text);
                //SettingParameter.Instance().StrSaveDir = tbSaveDir.Text;
                SettingParameter.Instance().FModLRLength = (float)numericUpDownLRLength.Value;
                SettingParameter.Instance().FModTDLength = (float)numericUpDownTDLength.Value;
                SettingParameter.Instance().FModLTLength = (float)numericUpDownLTLength.Value;
                SettingParameter.Instance().FModRTLength = (float)numericUpDownRTLength.Value;
                float fValue = 0;
                float.TryParse(textBoxCheckMinLength.Text, out fValue);
                SettingParameter.Instance().FCheckMinLength = fValue;
                float.TryParse(textBoxCheckMinVerticalLength.Text, out fValue);
                SettingParameter.Instance().FCheckMinVerticalLength = fValue;

                SettingParameter.Instance().Save();

            }
            catch(Exception ex)
            {
                LogHelper.Info("Silicon", "Save Parameters " + ex.Message);
            }
        }

        private void SettingParamPage_Initialize(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}