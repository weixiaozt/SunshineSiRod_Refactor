using Sunny.UI;
using SquareSiliconStickCheck.Parameters;
using System;
using SquareSiliconStickCheck.Tools;
using System.Windows.Forms;
using LiveCharts.Wpf;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using static SquareSiliconStickCheck.Tools.CMoveControllerModbusTool;

namespace SquareSiliconStickCheck.Pages
{
    public partial class MovePage : UIPage
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct MOVEPARAMETER
        {
            public double _fMoveSpeed;
            public double _fIncreaseSpeed;
        }

        public delegate void ShowRightButtonDelegate(bool bShow);

        public ShowRightButtonDelegate ShowRightBtnFunc;

        public static MovePage instance;

        private double _dbDestPosition = 0;

        private Timer _time;

        
        public MovePage()
        {
            instance = this;

            InitializeComponent();
            SettingParameter.Instance().Init();

            if (SettingParameter.Instance().NDaemon == 1)
            {
                CMoveControllerModbusTool.Instance().WriteSingleRegister(85, 1);
            }
           
            uiLabelCurPosition.Text = "0";

            comboBoxMoveDirection.SelectedIndex = 0;
            ShowRightBtnFunc = new ShowRightButtonDelegate(ShowRightButton);

            tbRotateDecreaseSpeed.Text = SettingParameter.Instance().FRotateDecreaseSpeed.ToString("0.0");
            tbRotateIncreaseSpeed.Text = SettingParameter.Instance().FRotateIncreaseSpeed.ToString("0.0");
            tbRotateLength.Text = SettingParameter.Instance().FRotateLength.ToString("0.0");
            tbRotateSpeed.Text = SettingParameter.Instance().FRotateSpeed.ToString("0.0");

            _time = new Timer
            {
                Interval = 100,
            };
            _time.Tick += _time_Tick;


        }

        private void _time_Tick(object sender, EventArgs e)
        {
            double dbPosition = 0;

            int nReadPosition = 0;
            if (SettingParameter.Instance().NDaemon == 1)
            {
                CMoveControllerModbusTool.Instance().ReadSingleRegister(8, ref nReadPosition);

            }

            uiLabelCurPosition.Text = nReadPosition.ToString();

            if (tbRotateLength.Text.Length <= 0)
            {
                return;
            }

            if (Math.Abs(_dbDestPosition - dbPosition) < 1)
            {
                btnStop.Enabled = false;
                _time.Stop();
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
        
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (tbRotateSpeed.Text.Length <= 0)
                {
                    UIMessageBox.Show("旋转速度不能为空！");
                    return;
                }

                if (tbRotateDecreaseSpeed.Text.Length <= 0)
                {
                    UIMessageBox.Show("旋转减速度不能为空！");
                    return;
                }

                if (tbRotateIncreaseSpeed.Text.Length <= 0)
                {
                    UIMessageBox.Show("旋转加速度不能为空！");
                    return;
                }

                if (tbRotateLength.Text.Length <= 0)
                {
                    UIMessageBox.Show("旋转加速度不能为空！");
                    return;
                }

                if (comboBoxMoveDirection.SelectedIndex == 0)
                {
                    _dbDestPosition = float.Parse(uiLabelCurPosition.Text) + float.Parse(tbRotateLength.Text);

                }
                else
                {
                    _dbDestPosition = float.Parse(uiLabelCurPosition.Text) - float.Parse(tbRotateLength.Text);

                }

                CMoveControllerModbusTool.Instance().WriteSingleRegister(70, (int)float.Parse(tbRotateIncreaseSpeed.Text));

                CMoveControllerModbusTool.Instance().WriteSingleRegister(71, (int)float.Parse(tbRotateDecreaseSpeed.Text));

                CMoveControllerModbusTool.Instance().WriteSingleRegister(72, (int)float.Parse(tbRotateSpeed.Text));

                int nPosition = (int)float.Parse(tbRotateLength.Text);

                if (nPosition >= 65536)
                {
                    CMoveControllerModbusTool.Instance().WriteSingleRegister(73, nPosition % 65536);
                    CMoveControllerModbusTool.Instance().WriteSingleRegister(74, nPosition / 65536);
                }
                else
                {
                    CMoveControllerModbusTool.Instance().WriteSingleRegister(73, nPosition);
                }


                if (comboBoxMoveDirection.SelectedIndex == 0)
                {
                    CMoveControllerModbusTool.Instance().WriteSingleRegister(18, 1);
                }
                else
                {
                    CMoveControllerModbusTool.Instance().WriteSingleRegister(18, 2);
                }



                btnStop.Enabled = true;
                _time.Start();
            }
            catch(Exception ex)
            {

            }

           

          

            
        }

        
        private void btnStop_Click(object sender, EventArgs e)
        {
            int sAxis = 0;

            CMoveControllerModbusTool.Instance().WriteSingleRegister(18, 5);
            _time.Stop();
        }

       

        private void uiButtonClearPosition_Click(object sender, EventArgs e)
        {
            CMoveControllerModbusTool.Instance().WriteSingleRegister(85, 1);
            uiLabelCurPosition.Text = "0";
        }
    }
}