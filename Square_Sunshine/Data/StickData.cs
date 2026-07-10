using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquareSiliconStickCheck.Data
{
    internal class StickData
    {
        private ArrayList _lefttoplineLengthInfo;
        private ArrayList _leftdownlineLengthInfo;
        private ArrayList _righttoplineLengthInfo;
        private ArrayList _rightdownlineLengthInfo;
        private ArrayList _tophypotenuseLengthInfo;
        private ArrayList _downhypotenuseLengthInfo;
        private ArrayList _lefthypotenuseLengthInfo;
        private ArrayList _righthypotenuseLengthInfo;
        private ArrayList _topfirstAngleInfo;
        private ArrayList _downfirstAngleInfo;
        private ArrayList _leftfirstAngleInfo;
        private ArrayList _rightfirstAngleInfo;
        private ArrayList _topsecondAngleInfo;
        private ArrayList _downsecondAngleInfo;
        private ArrayList _leftsecondAngleInfo;
        private ArrayList _rightsecondAngleInfo;
        private ArrayList _topfirstheightInfo;
        private ArrayList _downfirstheightInfo;
        private ArrayList _leftfirstheightInfo;
        private ArrayList _rightfirstheightInfo;
        private ArrayList _topsecondheightInfo;
        private ArrayList _downsecondheightInfo;
        private ArrayList _leftsecondheightInfo;
        private ArrayList _rightsecondheightInfo;
        private ArrayList _topdownhypotenuseDis;
        private ArrayList _leftrighthypotenuseDis;
        




        private string _strJBSearial;

        public StickData()
        {
          
            StrJBSearial = "";
            _lefttoplineLengthInfo = new ArrayList();
            _leftdownlineLengthInfo = new ArrayList();
            _righttoplineLengthInfo = new ArrayList();
            _rightdownlineLengthInfo = new ArrayList();
            _tophypotenuseLengthInfo = new ArrayList();
            _downhypotenuseLengthInfo = new ArrayList();
            _lefthypotenuseLengthInfo = new ArrayList();
            _righthypotenuseLengthInfo = new ArrayList();
            _topfirstAngleInfo = new ArrayList();
            _downfirstAngleInfo = new ArrayList();
            _leftfirstAngleInfo = new ArrayList();
            _rightfirstAngleInfo = new ArrayList();
            _topsecondAngleInfo = new ArrayList();
            _downsecondAngleInfo = new ArrayList();
            _leftsecondAngleInfo = new ArrayList();
            _rightsecondAngleInfo = new ArrayList();
            _topfirstheightInfo = new ArrayList();
            _downfirstheightInfo = new ArrayList();
            _leftfirstheightInfo = new ArrayList();
            _rightfirstheightInfo = new ArrayList();
            _topsecondheightInfo = new ArrayList();
            _downsecondheightInfo = new ArrayList();
            _leftsecondheightInfo = new ArrayList();
            _rightsecondheightInfo = new ArrayList();
            _topdownhypotenuseDis = new ArrayList();
            _leftrighthypotenuseDis = new ArrayList();  
        }

        public ArrayList LeftTopLineLengthInfo { get => _lefttoplineLengthInfo; set => _lefttoplineLengthInfo = value; }
        public ArrayList TopHypotenuseLengthInfo { get => _tophypotenuseLengthInfo; set => _tophypotenuseLengthInfo = value; }
        public ArrayList TopFirstAngleInfo { get => _topfirstAngleInfo; set => _topfirstAngleInfo = value; }
        public ArrayList TopSecondAngleInfo { get => _topsecondAngleInfo; set => _topsecondAngleInfo = value; }
        public ArrayList LeftdownlineLengthInfo { get => _leftdownlineLengthInfo; set => _leftdownlineLengthInfo = value; }
        public ArrayList RighttoplineLengthInfo { get => _righttoplineLengthInfo; set => _righttoplineLengthInfo = value; }
        public ArrayList RightdownlineLengthInfo { get => _rightdownlineLengthInfo; set => _rightdownlineLengthInfo = value; }
        public ArrayList DownhypotenuseLengthInfo { get => _downhypotenuseLengthInfo; set => _downhypotenuseLengthInfo = value; }
        public ArrayList LefthypotenuseLengthInfo { get => _lefthypotenuseLengthInfo; set => _lefthypotenuseLengthInfo = value; }
        public ArrayList RighthypotenuseLengthInfo { get => _righthypotenuseLengthInfo; set => _righthypotenuseLengthInfo = value; }
        public ArrayList DownfirstAngleInfo { get => _downfirstAngleInfo; set => _downfirstAngleInfo = value; }
        public ArrayList LeftfirstAngleInfo { get => _leftfirstAngleInfo; set => _leftfirstAngleInfo = value; }
        public ArrayList RightfirstAngleInfo { get => _rightfirstAngleInfo; set => _rightfirstAngleInfo = value; }
        public ArrayList DownsecondAngleInfo { get => _downsecondAngleInfo; set => _downsecondAngleInfo = value; }
        public ArrayList LeftsecondAngleInfo { get => _leftsecondAngleInfo; set => _leftsecondAngleInfo = value; }
        public ArrayList RightsecondAngleInfo { get => _rightsecondAngleInfo; set => _rightsecondAngleInfo = value; }
        public ArrayList TopfirstheightInfo { get => _topfirstheightInfo; set => _topfirstheightInfo = value; }
        public ArrayList DownfirstheightInfo { get => _downfirstheightInfo; set => _downfirstheightInfo = value; }
        public ArrayList LeftfirstheightInfo { get => _leftfirstheightInfo; set => _leftfirstheightInfo = value; }
        public ArrayList RightfirstheightInfo { get => _rightfirstheightInfo; set => _rightfirstheightInfo = value; }
        public ArrayList TopsecondheightInfo { get => _topsecondheightInfo; set => _topsecondheightInfo = value; }
        public ArrayList DownsecondheightInfo { get => _downsecondheightInfo; set => _downsecondheightInfo = value; }
        public ArrayList LeftsecondheightInfo { get => _leftsecondheightInfo; set => _leftsecondheightInfo = value; }
        public ArrayList RightsecondheightInfo { get => _rightsecondheightInfo; set => _rightsecondheightInfo = value; }
        public string StrJBSearial { get => _strJBSearial; set => _strJBSearial = value; }
        public ArrayList TopdownhypotenuseDis { get => _topdownhypotenuseDis; set => _topdownhypotenuseDis = value; }
        public ArrayList LeftrighthypotenuseDis { get => _leftrighthypotenuseDis; set => _leftrighthypotenuseDis = value; }
    }
}
