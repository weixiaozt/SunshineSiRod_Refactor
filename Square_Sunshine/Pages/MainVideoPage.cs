using Sunny.UI;
using SiliconRoundBarCheck.Cameras;
using SiliconRoundBarCheck.Tools;
using SiliconRoundBarCheck.Parameters;
using SiliconRoundBarCheck.Cameras.SSZN;

namespace SiliconRoundBarCheck.Pages
{
    public partial class MainVideoPage : UIPage
    {
        public MainVideoPage()
        {
            InitializeComponent();
        }

        private void InitTools()
        {
            CHikCameraTools.Instance().InitDev(SettingParameter.Instance().StrIPVideo, (int)CHikCameraTools.emHikCameraType.emVideo, ref pictureBoxVideo);
            CHikCameraTools.Instance().InitDev(SettingParameter.Instance().StrIPLJ, (int)CHikCameraTools.emHikCameraType.emLJ, ref pictureBoxLJ_1);


            CBVCameraTools.Instance().InitBVCamera(SettingParameter.Instance().StrIPYL, (int)CBVCameraTools.emBVCameraType.emBV_YL, ref pictureBoxYINLIE_1);
            CBVCameraTools.Instance().InitBVCamera(SettingParameter.Instance().StrIPYL_1, (int)CBVCameraTools.emBVCameraType.emBV_YL1, ref pictureBoxYingLi_1);

            SSZNCamera.Instance().Connect(SettingParameter.Instance().StrIPRadius);
            CHikCameraTools.Instance().StartPlay((int)CHikCameraTools.emHikCameraType.emVideo);
           
        }

    }
}