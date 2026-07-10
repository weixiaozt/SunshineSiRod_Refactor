using SiliconRoundBarCheck.Cameras;
using SiliconRoundBarCheck.Data;
using SquareSiliconStickCheck.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquareSiliconStickCheck.Data
{
    internal class GlobalDataCache
    {

        private static GlobalDataCache _instance;


        private Color btnBackColor;
        private Color btnFillColor;
        private Color btnFillHoverColor;
        private Color btnFillPressColor;
        private Color btnFillSelectedColor;
            
        private static float _fInterval = 0.04f;

        private object _lockleftData = new object();
        private object _lockrightData = new object();
        private object _locktopData = new object();
        private object _lockdownData = new object();
        private object _lockcheckData = new object();

        private Dictionary<string, StickData> _dicGlobaStickDataCache;
        private Dictionary<string, SquareStickCheckData> _dicGlobaSquareStickCheckDataCache;
        private Dictionary<int, SquareStickCheckCompResult> _dictGlobaCheckTypeAndResultCache; // key: checkType
        public Dictionary<string, StickData> DicLeftGlobaStickDataCache { get => _dicGlobaStickDataCache; set => _dicGlobaStickDataCache = value; }
        public Color BtnBackColor { get => btnBackColor; set => btnBackColor = value; }
        public Color BtnFillHoverColor { get => btnFillHoverColor; set => btnFillHoverColor = value; }
        public Color BtnFillPressColor { get => btnFillPressColor; set => btnFillPressColor = value; }
        public Color BtnFillSelectedColor { get => btnFillSelectedColor; set => btnFillSelectedColor = value; }
        public Color BtnFillColor { get => btnFillColor; set => btnFillColor = value; }
        public static float FInterval { get => _fInterval; set => _fInterval = value; }

        private GlobalDataCache()
        {

            BtnBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            BtnFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            BtnFillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(190)))), ((int)(((byte)(138)))));
            BtnFillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(175)))), ((int)(((byte)(36)))));
            BtnFillSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));

            _dicGlobaStickDataCache = new Dictionary<string, StickData>();
            _dicGlobaSquareStickCheckDataCache = new Dictionary<string, SquareStickCheckData>();
            _dictGlobaCheckTypeAndResultCache = new Dictionary<int, SquareStickCheckCompResult>();

            InitCheckCompResult();
            this.BtnFillColor = BtnFillColor;
        }



        //public Dictionary<string, StickData> DicLeftGlobaStickDataCache { get => _dicGlobaStickDataCache; set => _dicGlobaStickDataCache = value; }

        public static GlobalDataCache Instance() 
        {
            if (_instance == null)
            {
                _instance = new GlobalDataCache();
            }
            return _instance;
        }

        

        public void AddCheckData(string strSearialNum, SquareStickCheckData datainfo)
        {
            lock (_lockleftData)
            {
                if (true == _dicGlobaSquareStickCheckDataCache.ContainsKey(strSearialNum))
                {
                    _dicGlobaSquareStickCheckDataCache[strSearialNum] = datainfo;
                }
                else
                {
                    _dicGlobaSquareStickCheckDataCache.Add(strSearialNum, datainfo);
                }

            }
        }

        public void InitCheckCompResult()
        {
            float fLRTDLength = 247.1f;
            float fDiagLength = 10.76f;
            float fDiagShandowLength = 7.51f;
            float fLTLength = 182.1f;
            float fRTLegnth = 182.1f;

            _dictGlobaCheckTypeAndResultCache.Add(1, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength  + 0.25f,  fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 182.2f;
            fLRTDLength = 247f;
            fDiagLength = 10.92f;
            fDiagShandowLength = 7.72f;
            _dictGlobaCheckTypeAndResultCache.Add(2, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182f;
            fRTLegnth = 182f;
            fLRTDLength = 247f;
            fDiagLength = 10.76f;
            fDiagShandowLength = 7.51f;
            _dictGlobaCheckTypeAndResultCache.Add(3, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 182.2f;
            fLRTDLength = 247f;
            fDiagLength = 10.91f;
            fDiagShandowLength = 7.72f;
            _dictGlobaCheckTypeAndResultCache.Add(4, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182f;
            fRTLegnth = 182f;
            fLRTDLength = 247f;
            fDiagLength = 10.76f;
            fDiagShandowLength = 7.51f;
            _dictGlobaCheckTypeAndResultCache.Add(5, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 182.2f;
            fLRTDLength = 247f;
            fDiagLength = 10.62f;
            fDiagShandowLength = 7.51f;
            _dictGlobaCheckTypeAndResultCache.Add(6, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 182.2f;
            fLRTDLength = 247f;
            fDiagLength = 10.62f;
            fDiagShandowLength = 7.51f;
            _dictGlobaCheckTypeAndResultCache.Add(7, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 182.2f;
            fLRTDLength = 247f;
            fDiagLength = 10.62f;
            fDiagShandowLength = 7.51f;
            _dictGlobaCheckTypeAndResultCache.Add(8, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 182.2f;
            fLRTDLength = 247f;
            fDiagLength = 10.62f;
            fDiagShandowLength = 7.51f;
            _dictGlobaCheckTypeAndResultCache.Add(9, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 182.2f;
            fLRTDLength = 247f;
            fDiagLength = 10.62f;
            fDiagShandowLength = 7.51f;
            _dictGlobaCheckTypeAndResultCache.Add(10, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 182.2f;
            fLRTDLength = 247f;
            fDiagLength = 10.62f;
            fDiagShandowLength = 7.51f;
            _dictGlobaCheckTypeAndResultCache.Add(11, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 182.2f;
            fLRTDLength = 247f;
            fDiagLength = 10.62f;
            fDiagShandowLength = 7.51f;
            _dictGlobaCheckTypeAndResultCache.Add(12, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182f;
            fRTLegnth = 182f;
            fLRTDLength = 247f;
            fDiagLength = 10.76f;
            fDiagShandowLength = 7.51f;
            _dictGlobaCheckTypeAndResultCache.Add(13, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 182.2f;
            fLRTDLength = 247f;
            fDiagLength = 10.91f;
            fDiagShandowLength = 7.72f;
            _dictGlobaCheckTypeAndResultCache.Add(14, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182f;
            fRTLegnth = 182f;
            fLRTDLength = 247f;
            fDiagLength = 10.76f;
            fDiagShandowLength = 7.51f;
            _dictGlobaCheckTypeAndResultCache.Add(15, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 182.2f;
            fLRTDLength = 247f;
            fDiagLength = 10.91f;
            fDiagShandowLength = 7.72f;
            _dictGlobaCheckTypeAndResultCache.Add(16, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 182.2f;
            fLRTDLength = 247f;
            fDiagLength = 10.91f;
            fDiagShandowLength = 7.72f;
            _dictGlobaCheckTypeAndResultCache.Add(17, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 183.75f;
            fLRTDLength = 247f;
            fDiagLength = 12.07f;
            fDiagShandowLength = 8.49f;
            _dictGlobaCheckTypeAndResultCache.Add(18, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182f;
            fRTLegnth = 182f;
            fLRTDLength = 247f;
            fDiagLength = 10.76f;
            fDiagShandowLength = 7.51f;
            _dictGlobaCheckTypeAndResultCache.Add(19, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            //12A
            fLTLength = 182.3f;
            fRTLegnth = 183.5f;
            fLRTDLength = 256f;
            fDiagLength = 2.78f;
            fDiagShandowLength = 2.78f;
            _dictGlobaCheckTypeAndResultCache.Add(20, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            //15A
            fLTLength = 182.2f;
            fRTLegnth = 183.75f;
            fLRTDLength = 256f;
            fDiagLength = 2.78f;
            fDiagShandowLength = 1.9f;
            _dictGlobaCheckTypeAndResultCache.Add(21, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 183.9f;
            fLRTDLength = 256f;
            fDiagLength = 2.78f;
            fDiagShandowLength = 1.96f;
            _dictGlobaCheckTypeAndResultCache.Add(22, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 183.9f;
            fLRTDLength = 256f;
            fDiagLength = 2.78f;
            fDiagShandowLength = 1.96f;
            _dictGlobaCheckTypeAndResultCache.Add(23, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 183.9f;
            fLRTDLength = 247f;
            fDiagLength = 12.07f;
            fDiagShandowLength = 8.49f;
            _dictGlobaCheckTypeAndResultCache.Add(24, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));

            fLTLength = 182.2f;
            fRTLegnth = 183.9f;
            fLRTDLength = 256f;
            fDiagLength = 2.78f;
            fDiagShandowLength = 1.96f;
            _dictGlobaCheckTypeAndResultCache.Add(25, new SquareStickCheckCompResult(fLTLength - 0.25f, fLTLength + 0.25f, fRTLegnth - 0.25f, fRTLegnth + 0.25f, fLRTDLength - 0.25f, fLRTDLength + 0.25f, fDiagLength - 0.25f, fDiagLength + 0.25f, fDiagShandowLength - 0.5f, fDiagShandowLength + 0.5f));
        }

        public void AddData( string strSearialNum, StickData datainfo)
        {
           
            {
                lock (_lockleftData)
                {
                    if (true == _dicGlobaStickDataCache.ContainsKey(strSearialNum))
                    {
                        _dicGlobaStickDataCache[strSearialNum] = datainfo;
                    }
                    else
                    {
                        _dicGlobaStickDataCache.Add(strSearialNum, datainfo);
                    }

                }
            }
               
            
        }

        public bool GetCheckData(string strSerialNum, ref SquareStickCheckData datainfo)
        {
            try
            {
                bool bGetData = false;

                lock (_lockcheckData)
                {
                    if (_dicGlobaSquareStickCheckDataCache.ContainsKey(strSerialNum))
                    {
                        datainfo = (SquareStickCheckData)_dicGlobaSquareStickCheckDataCache[strSerialNum];
                        bGetData = true;
                        _dicGlobaSquareStickCheckDataCache.Remove(strSerialNum);
                    }
                }

                return bGetData;
            }
            catch (Exception ex)
            {

            }

            return false;
        }



        public bool CheckResultCompStand(int nCheckType, double fLTLength, double fRTLegnth, double fLRLength, double fTDLength, double fDiagLength, double fDiagShandowLength)
        {

            try
            {
                if (_dictGlobaCheckTypeAndResultCache.Keys.Contains(nCheckType) == true)
                {
                    LogHelper.Info("", "CheckResultCompStand Has Key nCheckType " + nCheckType.ToString());
                    SquareStickCheckCompResult result = (SquareStickCheckCompResult)_dictGlobaCheckTypeAndResultCache[nCheckType];

                    if (((fLTLength >= result.FMinLTLength && fLTLength <= result.FMaxLTLength) ||
                        (fLTLength >= result.FMinRTLength && fLTLength <= result.FMaxRTLength)) &&
                        ((fRTLegnth >= result.FMinLTLength && fRTLegnth <= result.FMaxLTLength) ||
                         (fRTLegnth >= result.FMinRTLength && fRTLegnth <= result.FMaxRTLength)) &&
                        (fLRLength >= result.FMinLRTDLength && fLRLength <= result.FMaxLRTDLength) &&
                        (fTDLength >= result.FMinLRTDLength && fTDLength <= result.FMaxLRTDLength) )
                        //(fDiagLength >= result.FMinDiagLength && fDiagLength <= result.FMaxDiagLength) &&
                        //(fDiagShandowLength >= result.FMinDiagShadowLength && fDiagShandowLength <= result.FMaxDiagShadowLength))
                    {
                        return true;
                    }

                    return false;
                }
            }
            catch(Exception ex)
            {

            }
            


            return true;
        }
        public bool GetData(string strSerialNum, ref  StickData datainfo)
        {
            try
            {
                bool bGetData = false;

                lock (_locktopData)
                {
                    if (_dicGlobaStickDataCache.ContainsKey(strSerialNum))
                    {
                        datainfo = (StickData)_dicGlobaStickDataCache[strSerialNum];
                        bGetData = true;
                        _dicGlobaStickDataCache.Remove(strSerialNum);
                    }
                }
                
                return bGetData;
            }
            catch (Exception ex)
            {

            }

            return false;
        }

    }
}
