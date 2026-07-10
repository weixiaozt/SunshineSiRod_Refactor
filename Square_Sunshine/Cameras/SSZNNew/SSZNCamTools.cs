
using HalconDotNet;
using SquareSiliconStickCheck.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SquareSiliconStickCheck.Parameters;
using SquareSiliconStickCheck.Cameras;
using System.Collections;
using System.Threading;
using SquareSiliconStickCheck.Data;
using System.IO;
using SiliconRoundBarCheck.Tools;
using SiliconRoundBarCheck.Data;
using OpenCvSharp.Internal.Vectors;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
//using SquareSiliconStickCheck.Cameras.XG;
using static SquareSiliconStickCheck.Tools.CMotionCardController;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Windows.Documents;
using System.Runtime.CompilerServices;
using System.Drawing;
using MathNet.Numerics.Distributions;
using System.Diagnostics.Eventing.Reader;
using HZH_Controls;
using System.Windows.Media.Media3D;





namespace SiliconRoundBarCheck.Cameras
{
    public enum emSSZNCamType
    {
        EM_TOP = 0,
        EM_DOWN = 1,
        EM_LEFT = 2,
        EM_RIGHT = 3,
    };

    public class tagParaInfo
    {
        public emSSZNCamType camtype;
        public HObject objref;
        public string strSerial;
        public int nSquareType;
        public int mnum;

        public tagParaInfo(emSSZNCamType camtyp,  string strserialnum, int nType = 0,int Mnum=0)
        {
            camtype = camtyp;
            mnum = Mnum;
            strSerial = strserialnum;
            nSquareType = nType;
        }
    }

    public class SSZNCamTools
    {

        public enum emStatus
        {
            EM_FREE = 0,  //空闲
            EM_TESTING = 1,  //从原点到终点测试
            EM_GOTOORIGIN = 2 //回原点
        }

        private static SSZNCamTools _instance;
        private SsznCamControl[] _cam3DArray = null;
        private ArrayList[] _cam3DOjbectsArray = null;
        private ArrayList[] _camGrayOjbectsArray = null;
        private bool _bNeedInsert3DObject = false;
        private bool _bstop = false;

        private object _objStatus = new object();
        private emStatus nTestStatus;
        private Thread _threadleft = null;
        private Thread _threadright = null;
        private Thread _threadtop = null;
        private Thread _threaddown = null;


        private object[] _objLock = null;
        private object[] _objGrayLock = null;
        private SSZNCamTools()
        {
            _cam3DArray = new SsznCamControl[4];
            _cam3DOjbectsArray = new ArrayList[4];
            _camGrayOjbectsArray = new ArrayList[4]; 
            _objLock = new object[4];
            _objGrayLock = new object[4];
            _bNeedInsert3DObject = false;
            _dictRefTypeRefObject = new Dictionary<ReferencePosInfoData.EMT_TYPE, ReferencePosInfoData>();
            _dictSquareTypeRefObjects = new Dictionary<int, ReferencePosInfoData>();
            for (int i = 0; i < 4; i++)
            {
                _cam3DArray[i] = new SsznCamControl(i);
                _cam3DOjbectsArray[i] = new ArrayList();
                _camGrayOjbectsArray[i] = new ArrayList();
                _objLock[i] = new object();
                _objGrayLock[i] = new object();
            }
            
            //防止误判
            NTestStatus = emStatus.EM_TESTING;
            
            InitReferenceObjectInfo();
            
            SaveReferenceObjectsInfo();
        }

        public void AddGrrayObjects(emSSZNCamType camType, HObject objImage)
        {
            lock (_objGrayLock[(int)camType])
            {
                _camGrayOjbectsArray[(int)camType].Add(objImage);
            }
        }

        public void AddObjects(emSSZNCamType camType, HObject objImage)
        {
            lock (_objLock[(int)camType])
            {
                //if (true == BNeedInsert3DObject)
                //{
                    LogHelper.Info("123", "Add Object camtype " + camType.ToString());
                    _cam3DOjbectsArray[(int)camType].Add(objImage);
                //}
            }
        }
        public bool ClearGrayObjectsByType(emSSZNCamType camType)
        {
            lock (_objGrayLock[(int)camType])
            {
                if (_camGrayOjbectsArray[(int)camType].Count > 0)
                {
                    _camGrayOjbectsArray[(int)camType].Clear();
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public bool ClearObjectsByType(emSSZNCamType camType)
        {
            lock (_objLock[(int)camType])
            {
                if (_cam3DOjbectsArray[(int)camType].Count > 0)
                {
                    _cam3DOjbectsArray[(int)camType].Clear();
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public bool GetGrayObjectsByType(emSSZNCamType camType, ref HObject objImage)
        {
            lock (_objGrayLock[(int)camType])
            {
                if (_camGrayOjbectsArray[(int)camType].Count > 0)
                {
                    objImage = (HObject)_camGrayOjbectsArray[(int)camType][0];
                    _camGrayOjbectsArray[(int)camType].RemoveAt(0);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
        public bool GetObjectsByType(emSSZNCamType camType, ref HObject objImage)
        {
            lock(_objLock[(int)camType])
            {
                if (_cam3DOjbectsArray[(int)camType].Count > 0)
                {
                    objImage = (HObject)_cam3DOjbectsArray[(int)camType][0];
                    _cam3DOjbectsArray[(int)camType].RemoveAt(0);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public static SSZNCamTools Instance()
        {
            if (null == _instance)
            {
                _instance = new SSZNCamTools();
            }
            return _instance;
        }

        public void InitCamTools()
        {
            InitCamTool(emSSZNCamType.EM_TOP);
            InitCamTool(emSSZNCamType.EM_DOWN);
            InitCamTool(emSSZNCamType.EM_LEFT);
            InitCamTool(emSSZNCamType.EM_RIGHT);
            
        }

        public void InitCamTool(emSSZNCamType camtype)
        {
            _cam3DArray[(int)camtype] = new SsznCamControl((int)camtype);
            _cam3DArray[((int)camtype)].ShowInfo1 += SSZNCamTools_ShowInfo1;
            _cam3DArray[(int)camtype].ShowNum1 += SSZNCamTools_ShowNum1;
            
            switch (camtype)
            {
                case emSSZNCamType.EM_TOP:
                    {
                        _cam3DArray[(int)camtype]._ShowImage += SSZNCamTools_ShowImage_Top;
                        //_cam3DArray[(int)camtype]._ShowImageLoop += SSZNCamTools_ShowImageLoopTop;
                        Connect(camtype, SettingParameter.Instance().StrTopLaser3DIP);
                        break;
                    }
                case emSSZNCamType.EM_LEFT:
                    {
                        _cam3DArray[(int)camtype]._ShowImage += SSZNCamTools_ShowImage_Left;
                        //_cam3DArray[(int)camtype]._ShowImageLoop += SSZNCamTools_ShowImageLoopLeft;
                        Connect(camtype, SettingParameter.Instance().StrLeftLaser3DIP);
                        break;
                    }
                case emSSZNCamType.EM_DOWN:
                    {
                        _cam3DArray[(int)camtype]._ShowImage += SSZNCamTools_ShowImage_Down;
                        //_cam3DArray[(int)camtype]._ShowImageLoop += SSZNCamTools_ShowImageLoopDown;
                        Connect(camtype, SettingParameter.Instance().StrDownLaser3DIP);
                        break;
                    }
                case emSSZNCamType.EM_RIGHT:
                    {
                        _cam3DArray[(int)camtype]._ShowImage += SSZNCamTools_ShowImage_Right;
                       // _cam3DArray[(int)camtype]._ShowImageLoop += SSZNCamTools_ShowImageLoopRight;
                        Connect(camtype,SettingParameter.Instance().StrRightLaser3DIP);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
           
        }

        private void SSZNCamTools_ShowImageLoopTop(bool topmost)
        {
            HObject hObject;
            HObject hGrayObject;

            HOperatorSet.GenEmptyObj(out hObject);
            HOperatorSet.GenEmptyObj(out hGrayObject);


            if (SettingParameter.Instance().NDaemon == 0)
            {
                if (File.Exists("D:/Image2/top/" + NMockTopIndex.ToString() + ".tif"))
                {
                    HOperatorSet.ReadImage(out hObject, "D:/Image2/top/" + NMockTopIndex.ToString() + ".tif");
                    NMockTopIndex++;
                }
                else
                {
                    return;
                }

            }
            else
            {
                hObject = GenImage1FromIntArray(_cam3DArray[(int)emSSZNCamType.EM_TOP].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_TOP].Width, _cam3DArray[(int)emSSZNCamType.EM_TOP].Height);
                //if (SettingParameter.Instance().NSaveBmp == 1)
                //{
                //    hGrayObject = GenImage1FromByteArray(_cam3DArray[(int)emSSZNCamType.EM_TOP].GrayData[0], _cam3DArray[(int)emSSZNCamType.EM_TOP].Width, _cam3DArray[(int)emSSZNCamType.EM_TOP].Height);
                //    AddGrrayObjects(emSSZNCamType.EM_TOP, hGrayObject);
                //}
                //


                NMockTopIndex++;
            }

            AddObjects(emSSZNCamType.EM_TOP, hObject);
            
            //AddGrrayObjects(emSSZNCamType.EM_TOP, hGrayObject);
        }

        private void SSZNCamTools_ShowImageLoopRight(bool topmost)
        {
            HObject hObject;
            HObject hGrayObject;

            HOperatorSet.GenEmptyObj(out hObject);
            HOperatorSet.GenEmptyObj(out hGrayObject);

            if (SettingParameter.Instance().NDaemon == 0)
            {
                if (File.Exists("D:/Image2/right/" + NMockRightIndex.ToString() + ".tif"))
                {
                    HOperatorSet.ReadImage(out hObject, "D:/Image2/right/" + NMockRightIndex.ToString() + ".tif");
                    NMockRightIndex++;
                }
                else
                {
                    return;
                }
            }
            else
            {
                hObject = GenImage1FromIntArray(_cam3DArray[(int)emSSZNCamType.EM_RIGHT].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_RIGHT].Width, _cam3DArray[(int)emSSZNCamType.EM_RIGHT].Height);
                //if (SettingParameter.Instance().NSaveBmp == 1)
                //{
                //    hGrayObject = GenImage1FromByteArray(_cam3DArray[(int)emSSZNCamType.EM_RIGHT].GrayData[0], _cam3DArray[(int)emSSZNCamType.EM_RIGHT].Width, _cam3DArray[(int)emSSZNCamType.EM_RIGHT].Height);
                //    AddGrrayObjects(emSSZNCamType.EM_RIGHT, hGrayObject);
                //}
                //
                //
                NMockRightIndex++;
            }

            AddObjects(emSSZNCamType.EM_RIGHT, hObject);
            //AddGrrayObjects(emSSZNCamType.EM_RIGHT, hGrayObject);
           
        }

        private void SSZNCamTools_ShowImageLoopDown(bool topmost)
        {
            HObject hObject;
            HObject hGrayObject;

            HOperatorSet.GenEmptyObj(out hObject);
            HOperatorSet.GenEmptyObj(out hGrayObject);


            if (SettingParameter.Instance().NDaemon == 0)
            {
                if (File.Exists("D:/Image2/down/" + NMockDownIndex.ToString() + ".tif"))
                {
                    HOperatorSet.ReadImage(out hObject, "D:/Image2/down/" + NMockDownIndex.ToString() + ".tif");
                    NMockDownIndex++;
                }
                else
                {
                    return;
                }
            }
            else
            {
                hObject = GenImage1FromIntArray(_cam3DArray[(int)emSSZNCamType.EM_DOWN].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_DOWN].Width, _cam3DArray[(int)emSSZNCamType.EM_DOWN].Height);
                //if (SettingParameter.Instance().NSaveBmp == 1)
                //{
                //    hGrayObject = GenImage1FromByteArray(_cam3DArray[(int)emSSZNCamType.EM_DOWN].GrayData[0], _cam3DArray[(int)emSSZNCamType.EM_DOWN].Width, _cam3DArray[(int)emSSZNCamType.EM_DOWN].Height);
                //    AddGrrayObjects(emSSZNCamType.EM_DOWN, hGrayObject);
                //}
                //
                //
                NMockDownIndex++;
            }

            AddObjects(emSSZNCamType.EM_DOWN, hObject);
            //AddGrrayObjects(emSSZNCamType.EM_DOWN, hGrayObject);
           
        }

        private void SSZNCamTools_ShowImageLoopLeft(bool topmost)
        {
            HObject hObject;
            HObject hGrayObject;

            HOperatorSet.GenEmptyObj(out hObject);
            HOperatorSet.GenEmptyObj(out hGrayObject);

            if (SettingParameter.Instance().NDaemon == 0)
            {
                if (File.Exists("D:/Image2/left/" + NMockLeftIndex.ToString() + ".tif"))
                {
                    HOperatorSet.ReadImage(out hObject, "D:/Image2/left/" + NMockLeftIndex.ToString() + ".tif");
                    NMockLeftIndex++;
                }
                else
                {
                    return;
                }
            }
            else
            {
                hObject = GenImage1FromIntArray(_cam3DArray[(int)emSSZNCamType.EM_LEFT].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_LEFT].Width, _cam3DArray[(int)emSSZNCamType.EM_LEFT].Height);
                //if (SettingParameter.Instance().NSaveBmp == 1)
                //{
                //    hGrayObject = GenImage1FromByteArray(_cam3DArray[(int)emSSZNCamType.EM_LEFT].GrayData[0], _cam3DArray[(int)emSSZNCamType.EM_LEFT].Width, _cam3DArray[(int)emSSZNCamType.EM_LEFT].Height);
                //    AddGrrayObjects(emSSZNCamType.EM_LEFT, hGrayObject);
                //}

                //
                NMockLeftIndex++;
            }

            AddObjects(emSSZNCamType.EM_LEFT, hObject);
            //AddGrrayObjects(emSSZNCamType.EM_LEFT, hGrayObject);
            
        }

        public void IntToHObject(int[] buffer, int width, int height, out HObject image)
        {
            IntPtr ptr = Marshal.AllocHGlobal(buffer.Length * sizeof(int));
            Marshal.Copy(buffer, 0, ptr, buffer.Length);
            HOperatorSet.GenImage1(out image, "uint2", width, height, ptr);
            Marshal.FreeHGlobal(ptr);
        }


        public void CheckSquareInfo(ref HObject hObject)
        {
            HObject ho_imageConvertX, ho_imageConvertY, ho_imageReducedZ;
            HTuple ho_object3D;
            CGlobalFuncTools.Instance().Creat_XYZ_From_sszn_COPY_1(hObject, out ho_imageConvertX, out ho_imageConvertY, out ho_imageReducedZ, new HTuple(0.015), out ho_object3D);

            
            HTuple hv_LengthInfo = new HTuple();
            HTuple hv_angle = new HTuple();
            CGlobalFuncTools.Instance().getSquareInfoByImage(hObject, out hv_LengthInfo, out hv_angle);

        }

        private void SSZNCamTools_ShowImage_Top(bool topmost)
        {

            HObject hObject;

            if (SettingParameter.Instance().NDaemon == 0)
            {
                HOperatorSet.ReadObject(out hObject, "D:/20view.tif");
                AddObjects(emSSZNCamType.EM_LEFT, hObject);
            }
            else
            {
                if (Bstop == false)
                {
                    //SoftTrigOne(emSSZNCamType.EM_TOP);
                    //SetBatchBlcok(emSSZNCamType.EM_TOP);
                    hObject = GenImage1FromIntArray(_cam3DArray[(int)emSSZNCamType.EM_TOP].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_TOP].Width, _cam3DArray[(int)emSSZNCamType.EM_TOP].Height);
                    AddObjects(emSSZNCamType.EM_TOP, hObject);
                }
                
               
                
            }

            
            
            ////获取控制器相机0的高度数据并显示图像  HeightData[0]代表相机0，HeightData[1]代表相机1
            //_cam3DArray[(int)emSSZNCamType.EM_TOP].BatchDataShow(_cam3DArray[(int)emSSZNCamType.EM_TOP].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_TOP].Width, _cam3DArray[(int)emSSZNCamType.EM_TOP].Height, SHOW_COLOR_MAP.SSZN_COLOR, -1000, 1000);


           
          
        }

        private void SSZNCamTools_ShowImage_Left(bool topmost)
        {
            HObject hObject;

            if (SettingParameter.Instance().NDaemon == 0)
            {
                HOperatorSet.ReadObject(out hObject, "D:/20view.tif");
                AddObjects(emSSZNCamType.EM_LEFT, hObject);
            }
            else
            {
                if (Bstop == false)
                {
                    //SoftTrigOne(emSSZNCamType.EM_LEFT);
                    //SetBatchBlcok(emSSZNCamType.EM_LEFT);
                    hObject = GenImage1FromIntArray(_cam3DArray[(int)emSSZNCamType.EM_LEFT].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_LEFT].Width, _cam3DArray[(int)emSSZNCamType.EM_LEFT].Height);
                    AddObjects(emSSZNCamType.EM_LEFT, hObject);
                }
               

            }

           
          
                ////获取控制器相机0的高度数据并显示图像  HeightData[0]代表相机0，HeightData[1]代表相机1
            //_cam3DArray[(int)emSSZNCamType.EM_LEFT].BatchDataShow(_cam3DArray[(int)emSSZNCamType.EM_LEFT].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_LEFT].Width, _cam3DArray[(int)emSSZNCamType.EM_LEFT].Height, SHOW_COLOR_MAP.SSZN_COLOR, -1000, 1000);




        }

        private void SSZNCamTools_ShowImage_Right(bool topmost)
        {

            HObject hObject;

            if (SettingParameter.Instance().NDaemon == 0)
            {
                HOperatorSet.ReadObject(out hObject, "D:/20view.tif");
                AddObjects(emSSZNCamType.EM_RIGHT, hObject);
            }
            else
            {
                if (Bstop == false)
                {
                    //SoftTrigOne(emSSZNCamType.EM_RIGHT);
                    //SetBatchBlcok(emSSZNCamType.EM_RIGHT);
                    hObject = GenImage1FromIntArray(_cam3DArray[(int)emSSZNCamType.EM_RIGHT].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_RIGHT].Width, _cam3DArray[(int)emSSZNCamType.EM_RIGHT].Height);
                    AddObjects(emSSZNCamType.EM_RIGHT, hObject);

                }
               

            }

           
           
            
           
            ////获取控制器相机0的高度数据并显示图像  HeightData[0]代表相机0，HeightData[1]代表相机1
            //_cam3DArray[(int)emSSZNCamType.EM_RIGHT].BatchDataShow(_cam3DArray[(int)emSSZNCamType.EM_RIGHT].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_RIGHT].Width, _cam3DArray[(int)emSSZNCamType.EM_RIGHT].Height, SHOW_COLOR_MAP.SSZN_COLOR, -1000, 1000);

        }

        private void SSZNCamTools_ShowImage_Down(bool topmost)
        {
            HObject hObject;
            if (SettingParameter.Instance().NDaemon == 0)
            {
                HOperatorSet.ReadObject(out hObject, "D:/20view.tif");
                AddObjects(emSSZNCamType.EM_DOWN, hObject);
            }
            else
            {
                if (Bstop == false)
                {
                    //SoftTrigOne(emSSZNCamType.EM_DOWN);
                    //SetBatchBlcok(emSSZNCamType.EM_DOWN);
                    hObject = GenImage1FromIntArray(_cam3DArray[(int)emSSZNCamType.EM_DOWN].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_DOWN].Width, _cam3DArray[(int)emSSZNCamType.EM_DOWN].Height);

                    AddObjects(emSSZNCamType.EM_DOWN, hObject);
                }
                
            }

            


            ////获取控制器相机0的高度数据并显示图像  HeightData[0]代表相机0，HeightData[1]代表相机1
            //_cam3DArray[(int)emSSZNCamType.EM_DOWN].BatchDataShow(_cam3DArray[(int)emSSZNCamType.EM_DOWN].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_DOWN].Width, _cam3DArray[(int)emSSZNCamType.EM_DOWN].Height, SHOW_COLOR_MAP.SSZN_COLOR, -1000, 1000);



        }

        public static HImage GenImage1FromByteArray(byte[] data, int width, int height)
        {
            
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr pointer = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
            HImage image = new HImage();
            image.GenImage1("byte", width, height, pointer);
            handle.Free();
            return image;
        }


        public static HImage GenImage1FromIntArray(int[] data, int width, int height)
        {
            float[] fData = new float[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                fData[i] = (float)data[i] / 100000;
            }

            GCHandle handle = GCHandle.Alloc(fData, GCHandleType.Pinned);
            IntPtr pointer = Marshal.UnsafeAddrOfPinnedArrayElement(fData, 0);
            HImage image = new HImage();
            image.GenImage1("real", width, height, pointer);
            Thread.Sleep(2000);
            handle.Free();
            return image;
        }

        public class tagParaCollectInfo
        {
            public HTuple Hv_fStandLTLength { get; set; }
            public HTuple Hv_fStandRTLength { get; set; }
            public HTuple Hv_fStandLRLength { get; set; }
            public HTuple Hv_fStandTDLength { get; set; }

            public int nSquareType { get; set; }
        }

        

        private int _nLeftIndex = 0;
        private int _nTopIndex = 0;
        private int _nRightIndex = 0;
        private int _nDownIndex = 0;
        private int _nMockLeftIndex = 0;
        private int _nMockTopIndex = 0;
        private int _nMockRightIndex = 0;
        private int _nMockDownIndex = 0;

        private int _nGrayLeftIndex = 0;
        private int _nGrayTopIndex = 0;
        private int _nGrayRightIndex = 0;
        private int _nGrayDownIndex = 0;
        

        private object _objStop = new object();
        public bool Bstop {
        get {
                bool bValue = false;
                lock(_objStop)
                {
                    bValue = _bstop;
                }
                return bValue;
            }
        set {
                lock(_objStop)
                {
                    _bstop = value;
                }
            }
        }



        public int NLeftIndex { get => _nLeftIndex; set => _nLeftIndex = value; }
        public int NTopIndex { get => _nTopIndex; set => _nTopIndex = value; }
        public int NRightIndex { get => _nRightIndex; set => _nRightIndex = value; }
        public int NDownIndex { get => _nDownIndex; set => _nDownIndex = value; }
        public int NGrayLeftIndex { get => _nGrayLeftIndex; set => _nGrayLeftIndex = value; }
        public int NGrayTopIndex { get => _nGrayTopIndex; set => _nGrayTopIndex = value; }
        public int NGrayRightIndex { get => _nGrayRightIndex; set => _nGrayRightIndex = value; }
        public int NGrayDownIndex { get => _nGrayDownIndex; set => _nGrayDownIndex = value; }
        public int NMockLeftIndex { get => _nMockLeftIndex; set => _nMockLeftIndex = value; }
        public int NMockTopIndex { get => _nMockTopIndex; set => _nMockTopIndex = value; }
        public int NMockRightIndex { get => _nMockRightIndex; set => _nMockRightIndex = value; }
        public int NMockDownIndex { get => _nMockDownIndex; set => _nMockDownIndex = value; }
        public bool BNeedInsert3DObject { get => _bNeedInsert3DObject; set => _bNeedInsert3DObject = value; }
        public emStatus NTestStatus 
        { get  
            {
                emStatus emvalue = emStatus.EM_FREE;
                lock(_objStatus)
                {
                    emvalue = nTestStatus;
                }
                return emvalue; 
            }
            set 
            { 
                lock(_objStatus)
                {
                    nTestStatus = value;
                }
            }
        }

        HObject _hObjectsLeft = new HObject();
        HObject _hObjectsRight = new HObject();
        HObject _hObjectsTop = new HObject();
        HObject _hObjectsDown = new HObject();
        HObject _ho_ImageLeft = new HObject();
        HObject _ho_ImageTop = new HObject();
        HObject _ho_ImageRight = new HObject();
        HObject _ho_ImageDown = new HObject();

        Dictionary<ReferencePosInfoData.EMT_TYPE, ReferencePosInfoData> _dictRefTypeRefObject;
        Dictionary<int, ReferencePosInfoData> _dictSquareTypeRefObjects;

        private void SaveReferenceObjectsInfo()
        {
            string strDirctory = System.Windows.Forms.Application.StartupPath;
            string strFile = strDirctory + "/Ref.bin";
            BinaryFormatter formatter = new BinaryFormatter();
            //_dictRefTypeRefObject.Clear();
            try
            {

                List<ReferencePosInfoData> listrefdatas = new List<ReferencePosInfoData>();
                //foreach (KeyValuePair<ReferencePosInfoData.EMT_TYPE, ReferencePosInfoData> kvp in _dictRefTypeRefObject)
                //{
                //    listrefdatas.Add(kvp.Value);
                //}
                foreach (KeyValuePair<int, ReferencePosInfoData> kvp in _dictSquareTypeRefObjects)
                {
                    listrefdatas.Add(kvp.Value);
                }
                

                string strJsonsInfo = JsonConvert.SerializeObject(listrefdatas);

                using (FileStream stream = File.Open(strFile, FileMode.OpenOrCreate))
                {
                    formatter.Serialize(stream, listrefdatas);
                }

            }
            catch (Exception ex)
            {

            }
        }
        public void InitReferenceObjectInfo()
        {
            try
            {
                string strJson = "";
                string strDirctory = System.Windows.Forms.Application.StartupPath;
                string strFile = strDirctory + "/Ref.bin";
                BinaryFormatter formatter = new BinaryFormatter();
                _dictRefTypeRefObject.Clear();
                _dictSquareTypeRefObjects.Clear();
                if (File.Exists(strFile))
                {

                    IList<ReferencePosInfoData> refdatas = new List<ReferencePosInfoData>();

                    using (FileStream stream = File.OpenRead(strFile))
                    {
                        refdatas = (IList<ReferencePosInfoData>)formatter.Deserialize(stream);
                    }

                    foreach (var da in refdatas)
                    {
                        try
                        {
                            _dictSquareTypeRefObjects.Add(da.nSquareSiliconType, da);

                            if (_dictRefTypeRefObject.ContainsKey(da.eMT_TYPE) == false)
                            {
                                _dictRefTypeRefObject.Add(da.eMT_TYPE, da);
                            }
                            if (_dictRefTypeRefObject.ContainsKey(da.eMT_TYPE) == false)
                            {
                                _dictRefTypeRefObject.Add(da.eMT_TYPE, da);
                            }


                            LogHelper.Info("", "Ref Data eMT_TYPE " + da.eMT_TYPE.ToString() + " nSquareSiliconType " + da.nSquareSiliconType.ToString() + " Hv_fPreLTHeight " + da.Hv_fPreLTHeight.D.ToString("0.000") + " Hv_fPreLTWidth " + da.Hv_fPreLTWidth.D.ToString("0.000") + " Hv_fPreRTHeight " + da.Hv_fPreRTHeight.D.ToString("0.000") + " Hv_fPreRTWidth " + da.Hv_fPreRTWidth.D.ToString("0.000") + " Hv_meanRefLeftDownIndex " + da.Hv_meanRefLeftDownIndex.D.ToString() + " Hv_meanRefLeftTopIndex " + da.Hv_meanRefLeftTopIndex.D.ToString() + " Hv_meanRefRightDownIndex " + da.Hv_meanRefRightDownIndex.D.ToString() + " Hv_meanRefRightLeftIndex " + da.Hv_meanRefRightLeftIndex.D.ToString() + " Hv_meanRefRightTopIndex " + da.Hv_meanRefRightTopIndex.D.ToString() + " Hv_RefmeanHeightDownL " + da.Hv_RefmeanHeightDownL.D.ToString("0.000") + " Hv_RefmeanHeightDownM " + da.Hv_RefmeanHeightDownM.D.ToString("0.000") + " Hv_RefmeanHeightDownR " + da.Hv_RefmeanHeightDownR.D.ToString("0.000") + " Hv_RefmeanHeightLeftL " + da.Hv_RefmeanHeightLeftL.D.ToString("0.000") + " Hv_RefmeanHeightLeftM " + da.Hv_RefmeanHeightLeftM.D.ToString("0.000") + " Hv_RefmeanHeightLeftR " + da.Hv_RefmeanHeightLeftR.D.ToString("0.000") + " Hv_RefmeanHeightRightL " + da.Hv_RefmeanHeightRightL.D.ToString("0.000") + " Hv_RefmeanHeightRightM " + da.Hv_RefmeanHeightRightM.D.ToString("0.000") + " Hv_RefmeanHeightRightR " + da.Hv_RefmeanHeightRightR.D.ToString("0.000") + " Hv_RefmeanHeightTopL " + da.Hv_RefmeanHeightTopL.D.ToString("0.000") + " Hv_RefmeanHeightTopM " + da.Hv_RefmeanHeightTopM.D.ToString("0.000") + " Hv_RefmeanHeightTopR " + da.Hv_RefmeanHeightTopR.D.ToString("0.000") + " Hv_RefRatioAngleD " + da.Hv_RefRatioAngleD.D.ToString("0.000") + " Hv_RefRatioAngleL " + da.Hv_RefRatioAngleL.D.ToString("0.000") + " Hv_RefRatioAngleR " + da.Hv_RefRatioAngleR.D.ToString("0.000") + " Hv_RefRatioAngleT " + da.Hv_RefRatioAngleT.D.ToString("0.000") + " hv_fPreTDLength " + da.hv_fPreTDLength.D.ToString("0.00") + " hv_fPreLRLength " + da.hv_fPreLRLength.D.ToString("0.00"));
                        }
                        catch(Exception ex)
                        {
                            LogHelper.Info("", "exception is " + ex.Message.ToString());
                        }

                        
                    }

                   
                }
                else
                {
                    List<ReferencePosInfoData> refdatas = new List<ReferencePosInfoData>();

                    for (int i = 0; i < 4; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                {
                                    {
                                        ReferencePosInfoData referencePosInfoData = new ReferencePosInfoData();

                                        referencePosInfoData.eMT_TYPE = (ReferencePosInfoData.EMT_TYPE)i;
                                        referencePosInfoData.Hv_RefmeanHeightLeftM = 4.92171981463614;
                                        referencePosInfoData.Hv_RefmeanHeightLeftL = 4.8575666579705;
                                        referencePosInfoData.Hv_RefmeanHeightLeftR = 4.89983524796044;
                                        referencePosInfoData.Hv_RefmeanHeightRightL = 3.4752019955321;
                                        referencePosInfoData.Hv_RefmeanHeightRightM = 3.50023046738998;
                                        referencePosInfoData.Hv_RefmeanHeightRightR = 3.44064055599585;
                                        referencePosInfoData.Hv_RefmeanHeightTopL = 5.05774630881414;
                                        referencePosInfoData.Hv_RefmeanHeightTopM = 5.08320848454548;
                                        referencePosInfoData.Hv_RefmeanHeightTopR = 5.02597080237689;
                                        referencePosInfoData.Hv_RefmeanHeightDownL = 3.97112301617326;
                                        referencePosInfoData.Hv_RefmeanHeightDownR = 3.92665833191522;
                                        referencePosInfoData.Hv_RefmeanHeightDownM = 3.99176664508051;

                                        referencePosInfoData.Hv_meanRefRightTopIndex = 1744.59183673469;
                                        referencePosInfoData.Hv_meanRefLeftTopIndex = 1035.78775510204;
                                        referencePosInfoData.Hv_meanRefLeftDownIndex = 1260.26464646465;
                                        referencePosInfoData.Hv_meanRefRightDownIndex = 1723.85360824742;
                                        referencePosInfoData.Hv_meanRefRightLeftIndex = 1016.40412371134;
                                        referencePosInfoData.Hv_meanRefLeftLeftIndex = 1035.78775510204;
                                        referencePosInfoData.Hv_meanRefRightRightIndex = 1744.59183673469;                                       
                                        referencePosInfoData.Hv_meanRefLeftRightIndex = 1260.26464646465;

                                        referencePosInfoData.Hv_fPreRTHeight = 128.764;
                                        referencePosInfoData.Hv_fPreRTWidth = 128.764;
                                        referencePosInfoData.Hv_fPreLTHeight = 128.735;
                                        referencePosInfoData.Hv_fPreLTWidth = 128.735;
                                        referencePosInfoData.hv_fPreLRLength = 246.98;
                                        referencePosInfoData.hv_fPreTDLength = 246.96;


                                        referencePosInfoData.Hv_RefRatioAngleR = 0.187;
                                        referencePosInfoData.Hv_RefRatioAngleL = 0.8;
                                        referencePosInfoData.Hv_RefRatioAngleT = 0.8;
                                        referencePosInfoData.Hv_RefRatioAngleD = 0.8;
                                        referencePosInfoData.nSquareSiliconType = 4;
                                        _dictSquareTypeRefObjects.Add(referencePosInfoData.nSquareSiliconType, referencePosInfoData);
                                        _dictRefTypeRefObject.Add((ReferencePosInfoData.EMT_TYPE)i, referencePosInfoData);
                                        refdatas.Add(referencePosInfoData);
                                        break;
                                    }
                                }
                            case 1:
                                {
                                    ReferencePosInfoData referencePosInfoData = new ReferencePosInfoData();

                                    referencePosInfoData.eMT_TYPE = (ReferencePosInfoData.EMT_TYPE)i;
                                    referencePosInfoData.Hv_RefmeanHeightLeftM = 4.92171981463614;
                                    referencePosInfoData.Hv_RefmeanHeightLeftL = 4.8575666579705;
                                    referencePosInfoData.Hv_RefmeanHeightLeftR = 4.89983524796044;
                                    referencePosInfoData.Hv_RefmeanHeightRightL = 3.4752019955321;
                                    referencePosInfoData.Hv_RefmeanHeightRightM = 3.50023046738998;
                                    referencePosInfoData.Hv_RefmeanHeightRightR = 3.44064055599585;
                                    referencePosInfoData.Hv_RefmeanHeightTopL = 5.05774630881414;
                                    referencePosInfoData.Hv_RefmeanHeightTopM = 5.08320848454548;
                                    referencePosInfoData.Hv_RefmeanHeightTopR = 5.02597080237689;
                                    referencePosInfoData.Hv_RefmeanHeightDownL = 3.97112301617326;
                                    referencePosInfoData.Hv_RefmeanHeightDownR = 3.92665833191522;
                                    referencePosInfoData.Hv_RefmeanHeightDownM = 3.99176664508051;

                                    referencePosInfoData.Hv_meanRefRightTopIndex = 1744.59183673469;
                                    referencePosInfoData.Hv_meanRefLeftTopIndex = 1035.78775510204;
                                    referencePosInfoData.Hv_meanRefLeftDownIndex = 1260.26464646465;
                                    referencePosInfoData.Hv_meanRefRightDownIndex = 1723.85360824742;
                                    referencePosInfoData.Hv_meanRefRightLeftIndex = 1016.40412371134;
                                    referencePosInfoData.Hv_fPreRTHeight = 128.764;
                                    referencePosInfoData.Hv_fPreRTWidth = 128.764;
                                    referencePosInfoData.Hv_fPreLTHeight = 128.735;
                                    referencePosInfoData.Hv_fPreLTWidth = 128.735;
                                    referencePosInfoData.hv_fPreLRLength = 246.98;
                                    referencePosInfoData.hv_fPreTDLength = 246.96;


                                    referencePosInfoData.Hv_RefRatioAngleR = 0.187;
                                    referencePosInfoData.Hv_RefRatioAngleL = 0.8;
                                    referencePosInfoData.Hv_RefRatioAngleT = 0.8;
                                    referencePosInfoData.Hv_RefRatioAngleD = 0.8;
                                    referencePosInfoData.nSquareSiliconType = 16;
                                    _dictSquareTypeRefObjects.Add(referencePosInfoData.nSquareSiliconType, referencePosInfoData);
                                    _dictRefTypeRefObject.Add((ReferencePosInfoData.EMT_TYPE)i, referencePosInfoData);
                                    refdatas.Add(referencePosInfoData);
                                    break;
                                }
                            case 2:
                                {
                                    ReferencePosInfoData referencePosInfoData = new ReferencePosInfoData();
                                    referencePosInfoData.eMT_TYPE = ReferencePosInfoData.EMT_TYPE.EMT_TYPE_182_THREE;
                                    referencePosInfoData.Hv_RefmeanHeightLeftM = 10.1180548334554;
                                    referencePosInfoData.Hv_RefmeanHeightLeftL = 10.1150835879326;
                                    referencePosInfoData.Hv_RefmeanHeightLeftR = 10.1139080531063;
                                    referencePosInfoData.Hv_RefmeanHeightRightL = 7.49116925042761;
                                    referencePosInfoData.Hv_RefmeanHeightRightM = 7.48614258419247;
                                    referencePosInfoData.Hv_RefmeanHeightRightR = 7.47342481260813;
                                    referencePosInfoData.Hv_RefmeanHeightTopL = 10.4344867397464;
                                    referencePosInfoData.Hv_RefmeanHeightTopM = 10.42935849383675;
                                    referencePosInfoData.Hv_RefmeanHeightTopR = 10.4167705003001;
                                    referencePosInfoData.Hv_RefmeanHeightDownL = 7.81628332052244;
                                    referencePosInfoData.Hv_RefmeanHeightDownR = 7.79277722495736;
                                    referencePosInfoData.Hv_RefmeanHeightDownM = 7.80810382201027;
                                    referencePosInfoData.Hv_meanRefRightTopIndex = 1564.67407407407;
                                    referencePosInfoData.Hv_meanRefLeftTopIndex = 1368.17037037037;
                                    referencePosInfoData.Hv_meanRefLeftDownIndex = 1610.37619047619;
                                    referencePosInfoData.Hv_meanRefRightDownIndex = 1454.02619047619;
                                    referencePosInfoData.Hv_meanRefRightLeftIndex = 1258.0380952381;
                                    referencePosInfoData.hv_fPreLRLength = 247.04;
                                    referencePosInfoData.hv_fPreTDLength = 247.05;
                                    referencePosInfoData.Hv_fPreRTHeight = 130.023;
                                    referencePosInfoData.Hv_fPreRTWidth = 130.023;
                                    referencePosInfoData.Hv_fPreLTHeight = 128.926;
                                    referencePosInfoData.Hv_fPreLTWidth = 128.926;
                                    referencePosInfoData.Hv_RefRatioAngleR = 0.187;
                                    referencePosInfoData.Hv_RefRatioAngleL = 0.8;
                                    referencePosInfoData.Hv_RefRatioAngleT = 0.8;
                                    referencePosInfoData.Hv_RefRatioAngleD = 0.8;
                                    referencePosInfoData.nSquareSiliconType = 20;
                                    _dictSquareTypeRefObjects.Add(referencePosInfoData.nSquareSiliconType, referencePosInfoData);
                                    _dictRefTypeRefObject.Add((ReferencePosInfoData.EMT_TYPE)i, referencePosInfoData);

                                    refdatas.Add(referencePosInfoData);

                                    break;
                                }
                            case 3:
                                {
                                    ReferencePosInfoData referencePosInfoData = new ReferencePosInfoData();
                                    referencePosInfoData.eMT_TYPE = ReferencePosInfoData.EMT_TYPE.EMT_TYPE_OTHER;
                                    referencePosInfoData.Hv_RefmeanHeightLeftM = 5.52284020636052;
                                    referencePosInfoData.Hv_RefmeanHeightLeftL = 5.46686349195384;
                                    referencePosInfoData.Hv_RefmeanHeightLeftR = 5.46019796087302;
                                    referencePosInfoData.Hv_RefmeanHeightRightL = 2.86046695295053;
                                    referencePosInfoData.Hv_RefmeanHeightRightM = 2.86323904039311;
                                    referencePosInfoData.Hv_RefmeanHeightRightR = 2.75370440739642;
                                    referencePosInfoData.Hv_RefmeanHeightTopL = 5.92618571237539;
                                    referencePosInfoData.Hv_RefmeanHeightTopM = 5.94946302992291;
                                    referencePosInfoData.Hv_RefmeanHeightTopR = 5.85837719432879;
                                    referencePosInfoData.Hv_RefmeanHeightDownL = 3.09583731224572;
                                    referencePosInfoData.Hv_RefmeanHeightDownR = 3.00684649301803;
                                    referencePosInfoData.Hv_RefmeanHeightDownM = 3.10830661246507;

                                    referencePosInfoData.Hv_meanRefRightTopIndex = 1875.22105263158;
                                    referencePosInfoData.Hv_meanRefLeftTopIndex = 1053.11578947368;
                                    referencePosInfoData.Hv_meanRefLeftDownIndex = 1306.69473684211;
                                    referencePosInfoData.Hv_meanRefRightDownIndex = 1764.96666666667;
                                    referencePosInfoData.Hv_meanRefRightLeftIndex = 954.911111111111;
                                    referencePosInfoData.hv_fPreLRLength = 246.98;
                                    referencePosInfoData.hv_fPreTDLength = 246.98;
                                    referencePosInfoData.Hv_fPreRTHeight = 130.04401071752;
                                    referencePosInfoData.Hv_fPreRTWidth = 130.04401071752;
                                    referencePosInfoData.Hv_fPreLTHeight = 128.933847892053;
                                    referencePosInfoData.Hv_fPreLTWidth = 128.933847892053;
                                    referencePosInfoData.Hv_RefRatioAngleR = 0.187;
                                    referencePosInfoData.Hv_RefRatioAngleL = 0.8;
                                    referencePosInfoData.Hv_RefRatioAngleT = 0.8;
                                    referencePosInfoData.Hv_RefRatioAngleD = 0.8;
                                    referencePosInfoData.nSquareSiliconType = 24;
                                    _dictSquareTypeRefObjects.Add(referencePosInfoData.nSquareSiliconType, referencePosInfoData);
                                    _dictRefTypeRefObject.Add((ReferencePosInfoData.EMT_TYPE)i, referencePosInfoData);

                                    refdatas.Add(referencePosInfoData);
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }

                    try
                    {
                        string strJsonsInfo = JsonConvert.SerializeObject(refdatas);

                        
                        using (FileStream stream = File.Open(strFile, FileMode.OpenOrCreate))
                        {
                            formatter.Serialize(stream, refdatas);
                        }

                    }
                    catch(Exception ex)
                    {

                    }
                    


                }
              



            }
            catch (Exception ex)
            {

            }

           
           
           
        }


        public void ThreadDealSquareInfocollection(object objtype)
        {
            #region 初始化
            tagParaCollectInfo parInfo = (tagParaCollectInfo)objtype;
            int nWaitIndex = 0;
            int nGrayWaitIndex = 0;
            bool bNeedBegin = false;
            HObject hObject = new HObject();

            HOperatorSet.GenEmptyObj(out _ho_ImageLeft);
            HOperatorSet.GenEmptyObj(out _ho_ImageDown);
            HOperatorSet.GenEmptyObj(out _ho_ImageTop);
            HOperatorSet.GenEmptyObj(out _ho_ImageRight);

            HOperatorSet.GenEmptyObj(out _hObjectsLeft);
            HOperatorSet.GenEmptyObj(out _hObjectsRight);
            HOperatorSet.GenEmptyObj(out _hObjectsDown);
            HOperatorSet.GenEmptyObj(out _hObjectsTop);

            HTuple hv_HomMat2DTDAdapted = new HTuple();
            HTuple hv_HomMat2DTLAdapted = new HTuple();
            HTuple hv_HomMat2DTRAdapted = new HTuple();
            HTuple hv_HomMat2DDTAdapted = new HTuple();
            HTuple hv_HomMat2DLAdapted = new HTuple();
            HTuple hv_HomMat2DRAdapted = new HTuple();
            HTuple hv_HomMat2DLDAdapted = new HTuple();
            HTuple hv_HomMat2DLRAdapted = new HTuple();
            HTuple hv_HomMat2DLTAdapted = new HTuple();
            HTuple hv_HomMat2DRDAdapted = new HTuple();
            HTuple hv_HomMat2DRLAdapted = new HTuple();
            HTuple hv_HomMat2DRTAdapted = new HTuple();

            HTuple hv_RefDiagTopLength = new HTuple(), hv_RefLeftIndexesTop = new HTuple();
            HTuple hv_RefRightIndexesTop = new HTuple(), hv_RefRowBeginTop = new HTuple();
            HTuple hv_DiagTopLength = new HTuple(), hv_DiagRightLength = new HTuple();
            HTuple hv_DiagLeftLength = new HTuple(), hv_DiagDownLength = new HTuple();
            HTuple hv_RowBeginTop = new HTuple(), hv_RowBeginLeft = new HTuple();
            HTuple hv_RowBeginRight = new HTuple(), hv_RowBeginDown = new HTuple();
            HTuple hv_LeftIndexesTop = new HTuple(), hv_RightIndexesTop = new HTuple();
            HTuple hv_LeftIndexesLeft = new HTuple(), hv_RightIndexesLeft = new HTuple();
            HTuple hv_LeftIndexesRight = new HTuple(), hv_RightIndexesRight = new HTuple();
            HTuple hv_LeftIndexesDown = new HTuple(), hv_RightIndexesDown = new HTuple();
            HTuple hv_RefmeanHeightTopL = new HTuple(), hv_RefmeanHeightTopM = new HTuple();
            HTuple hv_RefmeanHeightTopR = new HTuple(), hv_RefDiagLeftLength = new HTuple();
            HTuple hv_RefLeftIndexesLeft = new HTuple(), hv_RefRightIndexesLeft = new HTuple();
            HTuple hv_RefRowBeginLeft = new HTuple(), hv_RefmeanHeightLeftL = new HTuple();
            HTuple hv_RefmeanHeightLeftM = new HTuple(), hv_RefmeanHeightLeftR = new HTuple();
            HTuple hv_RefDiagRightLength = new HTuple(), hv_RefLeftIndexesRight = new HTuple();
            HTuple hv_RefRightIndexesRight = new HTuple(), hv_RefRowBeginRight = new HTuple();
            HTuple hv_RefmeanHeightRightL = new HTuple(), hv_RefmeanHeightRightM = new HTuple();
            HTuple hv_RefmeanHeightRightR = new HTuple(), hv_RefDiagDownLength = new HTuple();
            HTuple hv_RefLeftIndexesDown = new HTuple(), hv_RefRightIndexesDown = new HTuple();
            HTuple hv_RefRowBeginDown = new HTuple(), hv_RefmeanHeightDownL = new HTuple();
            HTuple hv_RefmeanHeightDownM = new HTuple(), hv_RefmeanHeightDownR = new HTuple();
            HTuple hv_meanHeightTopL = new HTuple(), hv_meanHeightTopM = new HTuple();
            HTuple hv_meanHeightTopR = new HTuple(), hv_meanHeightRightL = new HTuple();
            HTuple hv_meanHeightRightM = new HTuple(), hv_meanHeightRightR = new HTuple();
            HTuple hv_meanHeightLeftL = new HTuple(), hv_meanHeightLeftM = new HTuple();
            HTuple hv_meanHeightLeftR = new HTuple(), hv_meanHeightDownL = new HTuple();
            HTuple hv_meanHeightDownM = new HTuple(), hv_meanHeightDownR = new HTuple();
            HTuple hv_meanRefLeftTopIndex = new HTuple(), hv_meanRightTopIndex = new HTuple();
            HTuple hv_meanRefRightTopIndex = new HTuple(), hv_meanLeftDownIndex = new HTuple();
            HTuple hv_meanRefLeftDownIndex = new HTuple(), hv_meanRightDownIndex = new HTuple();
            HTuple hv_meanRefRightDownIndex = new HTuple(), hv_meanRightLeftIndex = new HTuple();
            HTuple hv_meanRefRightLeftIndex = new HTuple(), hv_TopLeftSub = new HTuple();

            HTuple hv_meanRefLeftLeftIndex = new HTuple();
            HTuple hv_meanRefRightRightIndex = new HTuple();
            HTuple hv_meanRefLeftRightIndex = new HTuple();




            HTuple hv_TopRightSub = new HTuple(), hv_LeftDownSub = new HTuple();
            HTuple hv_RightDownSub = new HTuple(), hv_RightTopSub = new HTuple();
            HTuple hv_meanHeightSubTopR = new HTuple(), hv_meanHeightSubTopL = new HTuple();
            HTuple hv_meanHeightSubLeftD = new HTuple(), hv_subTopL = new HTuple();
            HTuple hv_subTop = new HTuple();
            HTuple hv_subTopR = new HTuple(), hv_subLeftD = new HTuple();
            HTuple hv_subRightD = new HTuple(), hv_subLeft = new HTuple();
            HTuple hv_subRight = new HTuple(), hv_subDown = new HTuple();
            HTuple hv_topAngleOfTwoLineT = new HTuple(), hv_midAngleOfTwoLineT = new HTuple();
            HTuple hv_downAngleOfTwoLineT = new HTuple();
            HTuple hv_topAngleOfTwoLineL = new HTuple(), hv_midAngleOfTwoLineL = new HTuple();
            HTuple hv_downAngleOfTwoLineL = new HTuple();
            HTuple hv_topAngleOfTwoLineR = new HTuple(), hv_midAngleOfTwoLineR = new HTuple();
            HTuple hv_downAngleOfTwoLineR = new HTuple();
            HTuple hv_topAngleOfTwoLineD = new HTuple(), hv_midAngleOfTwoLineD = new HTuple();
            HTuple hv_downAngleOfTwoLineD = new HTuple();
            HTuple hv_lengthstmp = new HTuple(), hv_Min = new HTuple();
            HTuple hv_Max = new HTuple(), hv_Indices = new HTuple();
            HTuple hv_fSub = new HTuple(), hv_meanLeftTopIndex = new HTuple();
            HTuple hv_fMeanLength = new HTuple();
            HTuple hv_fValue = new HTuple();
            HTuple hv_fPreRTHeight = new HTuple();
            HTuple hv_fPreRTWidth = new HTuple();
            HTuple hv_fPreLTHeight = new HTuple();
            HTuple hv_fPreLTWidth = new HTuple();

            HTuple hv_fLTRatio = new HTuple();
            HTuple hv_fLDRatio = new HTuple();
            HTuple hv_fRTRatio = new HTuple();
            HTuple hv_fRDRatio = new HTuple();
            HTuple hv_fLRRatio = new HTuple();
            HTuple hv_fTDRatio = new HTuple();
            HTuple hv_fTopDiagRatio = new HTuple();
            HTuple hv_fDownDiagRatio = new HTuple();
            HTuple hv_fLeftDiagRatio = new HTuple();
            HTuple hv_fRightDiagRatio = new HTuple();

            HTuple hv_meanMidt = new HTuple();
            HTuple hv_meanMidr = new HTuple();
            HTuple hv_meanMidd = new HTuple();
            HTuple hv_meanMidl = new HTuple();
            HTuple hv_rowRT3D = new HTuple();
            HTuple hv_colRT3D = new HTuple();
            HTuple hv_rowRD3D = new HTuple();
            HTuple hv_colRD3D = new HTuple();
            HTuple hv_rowLT3D = new HTuple();
            HTuple hv_colLT3D = new HTuple();
            HTuple hv_rowLD3D = new HTuple();
            HTuple hv_colLD3D = new HTuple();

            HTuple hv_squareStickLength = new HTuple();
            HTuple hv_Surface3DDefaultTF = new HTuple();

            HTuple hv_Surface3DDefaultTS = new HTuple();
            HTuple hv_Surface3DDefaultTT = new HTuple();
            HTuple hv_Surface3DDefaultDF = new HTuple();
            HTuple hv_Surface3DDefaultDS = new HTuple();
            HTuple hv_Surface3DDefaultDT = new HTuple();
            HTuple hv_Surface3DDefaultLF = new HTuple();
            HTuple hv_Surface3DDefaultLS = new HTuple();
            HTuple hv_Surface3DDefaultLT = new HTuple();
            HTuple hv_Surface3DDefaultRF = new HTuple();
            HTuple hv_Surface3DDefaultRS = new HTuple();
            HTuple hv_Surface3DDefaultRT = new HTuple();
            HTuple hv_StickImageHeight = new HTuple();
            HTuple hv_firstTopIndexRow = new HTuple();

            HTuple hv_secondTopIndexRow = new HTuple();
            HTuple hv_thirdTopIndexRow = new HTuple();
            HTuple hv_firstDowntIndexRow = new HTuple();
            HTuple hv_secondDownIndexRow = new HTuple();
            HTuple hv_thirdDownIndexRow = new HTuple();
            HTuple hv_firstRightIndexRow = new HTuple();
            HTuple hv_secondRightIndexRow = new HTuple();
            HTuple hv_thirdRightIndexRow = new HTuple();
            HTuple hv_firstLeftIndexRow = new HTuple();
            HTuple hv_secondLeftIndexRow = new HTuple();
            HTuple hv_thirdLeftIndexRow = new HTuple();
            HTuple hv_StickLength = new HTuple();


            ReferencePosInfoData dataref = new ReferencePosInfoData();
            HTuple hv_fEdgeLengthDDRHeight = new HTuple();
            HTuple hv_fEdgeLengthDDRWidth = new HTuple();
            HTuple hv_fEdgeLengthDDLHeight = new HTuple();
            HTuple hv_fEdgeLengthDDLWidth = new HTuple();
            HTuple hv_heightDT3D = new HTuple();
            HTuple hv_widthRL3D = new HTuple();
            HTuple hv_fRatioT = 0.01496;
            HTuple hv_fRatioR = 0.01496;
            HTuple hv_fRatioL = 0.01496;
            HTuple hv_fRatioD = 0.01496;
            bool bCheckReference = false;
            Thread threaddeal = null;
            bool bCalculateStick = false;
            bool bValidWait = false;
            Thread threadMov = null;
            NTestStatus = emStatus.EM_TESTING;
            #endregion
            while (true)
            {
                #region 存图路径
                HOperatorSet.GenEmptyObj(out hObject);

                if (false == Directory.Exists("D:/Image1"))
                {
                    Directory.CreateDirectory("D:/Image1");
                }

                if (false == Directory.Exists("D:/Image1/left"))
                {
                    Directory.CreateDirectory("D:/Image1/left");
                }

                if (false == Directory.Exists("D:/Image1/right"))
                {
                    Directory.CreateDirectory("D:/Image1/right");
                }

                if (false == Directory.Exists("D:/Image1/top"))
                {
                    Directory.CreateDirectory("D:/Image1/top");
                }

                if (false == Directory.Exists("D:/Image1/down"))
                {
                    Directory.CreateDirectory("D:/Image1/down");
                }
                #endregion
                for (int i = 0; i < 4; i++)
                {
                    #region 加载图片
                    if (true == GetObjectsByType((emSSZNCamType)i, ref hObject))
                    {
                        //paraInfo.objref = new HObject(hObject);
                        bValidWait = true;
                        switch (i)
                        {
                            case (int)emSSZNCamType.EM_LEFT:
                                {
                                    if (SettingParameter.Instance().NSaveBmp == 1)
                                    {
                                        HObject hGrayObject = new HObject();
                                        HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image1/left/" + NLeftIndex.ToString() + ".tif");
                                        LogHelper.Info("", "Save Bmp Down " + NLeftIndex.ToString());

                                    }
                                    //HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image/left/" + NLeftIndex++.ToString() + ".tif");
                                    NLeftIndex++;
                                    HOperatorSet.ConcatObj(_hObjectsLeft, hObject, out _hObjectsLeft);
                                    break;
                                }
                            case (int)emSSZNCamType.EM_RIGHT:
                                {
                                    if (SettingParameter.Instance().NSaveBmp == 1)
                                    {
                                        HObject hGrayObject = new HObject();
                                        HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image1/right/" + NRightIndex.ToString() + ".tif");
                                        LogHelper.Info("", "Save Bmp Down " + NRightIndex.ToString());

                                    }

                                    NRightIndex++;
                                    HOperatorSet.ConcatObj(_hObjectsRight, hObject, out _hObjectsRight);
                                    break;
                                }
                            case (int)emSSZNCamType.EM_TOP:
                                {
                                    if (SettingParameter.Instance().NSaveBmp == 1)
                                    {
                                        HObject hGrayObject = new HObject();
                                        HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image1/top/" + NTopIndex.ToString() + ".tif");
                                        LogHelper.Info("", "Save Bmp Down " + NTopIndex.ToString());

                                    }
                                    //HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image/top/" + NTopIndex++.ToString() + ".tif");
                                    NTopIndex++;
                                    HOperatorSet.ConcatObj(_hObjectsTop, hObject, out _hObjectsTop);
                                    break;
                                }
                            case (int)emSSZNCamType.EM_DOWN:
                                {
                                    if (SettingParameter.Instance().NSaveBmp == 1)
                                    {
                                        HObject hGrayObject = new HObject();
                                        HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image1/down/" + NDownIndex.ToString() + ".tif");
                                        LogHelper.Info("", "Save Bmp Down " + NDownIndex.ToString());

                                    }
                                    //HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image/down/" + NDownIndex++.ToString() + ".tif");
                                    NDownIndex++;
                                    HOperatorSet.ConcatObj(_hObjectsDown, hObject, out _hObjectsDown);
                                    break;
                                }
                        }

                        LogHelper.Info("Silicon", "ThreadDealSquareInfo NLeftIndex " + NLeftIndex.ToString() + " NRightIndex " + NRightIndex.ToString() + " NTopIndex " + NTopIndex.ToString() + " NDownIndex " + NDownIndex.ToString());

                        nWaitIndex = 0;
                    }
                    #endregion
                    else
                    {
                        if (bValidWait == true)
                        {
                            nWaitIndex++;
                            if (nWaitIndex >= 8)
                            {
                                try
                                {
                                    bCalculateStick = true;

                                    if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
                                    {
                                        if (SettingParameter.Instance().NCamType == 0)
                                        {
                                            SSZNCamTools.Instance().StopBatchLoop();
                                        }
                                        else
                                        {
                                            //XGCamTools.Instance.StopCaptures();
                                        }
                                    }


                                    if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
                                    {
                                        threadMov = new Thread(() =>
                                        {
                                            LogHelper.Info("Silicon", "ThreadDealSquareInfo Move To Orgin Position");
                                            CMoveController.Instance().SetMoveSpeed(SettingParameter.Instance().FPLCMoveSpeed);
                                            CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, SettingParameter.Instance().FPosition_fir_s, 20);
                                            NTestStatus = emStatus.EM_FREE;
                                            //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                                        });

                                        threadMov.Start();
                                    }
                                    NTestStatus = emStatus.EM_GOTOORIGIN;

                                    if (NLeftIndex >= 4 && NRightIndex >= 4 && NTopIndex >= 4 && NDownIndex >= 4)
                                    {
                                        LogHelper.Info("Silicon", "ThreadDealSqureSiliconCollection begin to Deal Bmps");
                                        HObject hObjectLeftReference = new HObject();
                                        HObject hObjectRightReference = new HObject();
                                        HObject hObjectTopReference = new HObject();
                                        HObject hObjectDownReference = new HObject();
                                        try
                                        {
                                            HOperatorSet.GenEmptyObj(out hObjectLeftReference);
                                            HOperatorSet.GenEmptyObj(out hObjectRightReference);
                                            HOperatorSet.GenEmptyObj(out hObjectTopReference);
                                            HOperatorSet.GenEmptyObj(out hObjectDownReference);

                                            HOperatorSet.TileImages(_hObjectsLeft, out _ho_ImageLeft, 1, "vertical");
                                            HOperatorSet.TileImages(_hObjectsRight, out _ho_ImageRight, 1, "vertical");
                                            HOperatorSet.TileImages(_hObjectsTop, out _ho_ImageTop, 1, "vertical");
                                            HOperatorSet.TileImages(_hObjectsDown, out _ho_ImageDown, 1, "vertical");


                                            HOperatorSet.CropPart(_ho_ImageLeft, out hObjectLeftReference, 5000, 0, 3200, 500);
                                            HOperatorSet.CropPart(_ho_ImageTop, out hObjectTopReference, 6300, 0, 3200, 500);
                                            HOperatorSet.CropPart(_ho_ImageRight, out hObjectRightReference, 5500, 0, 3200, 500);
                                            HOperatorSet.CropPart(_ho_ImageDown, out hObjectDownReference, 6300, 0, 3200, 500);

                                            LogHelper.Info("Silicon", "ThreadDealSqureSiliconCollection Crop Bmps");

                                            HTuple hv_fRatioAngleT = new HTuple();
                                            HTuple hv_fRatioAngleD = new HTuple();
                                            HTuple hv_fRatioAngleL = new HTuple();
                                            HTuple hv_fRatioAngleR = new HTuple();

                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeightnew(hObjectLeftReference, out HTuple hv_topAngleOfTwoLinesRefL, out HTuple hv_midAngleOfTwoLinesRefL, out HTuple hv_downAngleOfTwoLinesRefL);

                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeightnew(hObjectRightReference, out HTuple hv_topAngleOfTwoLinesRefR, out HTuple hv_midAngleOfTwoLinesRefR, out HTuple hv_downAngleOfTwoLinesRefR);

                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeightnew(hObjectTopReference, out HTuple hv_topAngleOfTwoLinesRefT, out HTuple hv_midAngleOfTwoLinesRefT, out HTuple hv_downAngleOfTwoLinesRefT);

                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeightnew(hObjectDownReference, out HTuple hv_topAngleOfTwoLinesRefD, out HTuple hv_midAngleOfTwoLinesRefD, out HTuple hv_downAngleOfTwoLinesRefD);

                                            if (hv_topAngleOfTwoLinesRefT.Length > 0 && hv_midAngleOfTwoLinesRefT.Length > 0 && hv_downAngleOfTwoLinesRefT.Length > 0)
                                            {
                                                HOperatorSet.TupleMax((((hv_topAngleOfTwoLinesRefT.TupleConcat(hv_topAngleOfTwoLinesRefT.D)).TupleConcat(hv_midAngleOfTwoLinesRefT.D)).TupleConcat(hv_downAngleOfTwoLinesRefT.D)), out HTuple hv_maxTopAngle);


                                                if (hv_maxTopAngle != null && hv_maxTopAngle.TupleGreater(0.015) != 0)
                                                {
                                                    hv_fRatioAngleT = 0.005 / hv_maxTopAngle;
                                                }
                                                else
                                                {
                                                    hv_fRatioAngleT = 0.8;
                                                }
                                            }

                                            if (hv_topAngleOfTwoLinesRefL.Length > 0 && hv_midAngleOfTwoLinesRefL.Length > 0 && hv_downAngleOfTwoLinesRefL.Length > 0)
                                            {

                                                HOperatorSet.TupleMax(hv_topAngleOfTwoLinesRefL.TupleConcat(hv_topAngleOfTwoLinesRefL.D).TupleConcat(hv_midAngleOfTwoLinesRefL.D).TupleConcat(hv_downAngleOfTwoLinesRefL.D), out HTuple hv_maxLeftAngle);

                                                if (hv_maxLeftAngle != null && hv_maxLeftAngle.TupleGreater(0.015) != 0)
                                                {

                                                    hv_fRatioAngleL = 0.005 / hv_maxLeftAngle;
                                                }
                                                else
                                                {
                                                    hv_fRatioAngleL = 0.8;
                                                }
                                            }

                                            if (hv_topAngleOfTwoLinesRefR.Length > 0 && hv_midAngleOfTwoLinesRefR.Length > 0 && hv_downAngleOfTwoLinesRefR.Length > 0)
                                            {
                                                HOperatorSet.TupleMax(hv_topAngleOfTwoLinesRefR.TupleConcat(hv_topAngleOfTwoLinesRefR.D).TupleConcat(hv_midAngleOfTwoLinesRefR.D).TupleConcat(hv_downAngleOfTwoLinesRefR.D), out HTuple hv_maxRightAngle);

                                                if (hv_maxRightAngle != null && hv_maxRightAngle.TupleGreater(0.015) != 0)
                                                {
                                                    hv_fRatioAngleR = 0.005 / hv_maxRightAngle;
                                                }
                                                else
                                                {
                                                    hv_fRatioAngleR = 0.8;
                                                }
                                            }

                                            if (hv_topAngleOfTwoLinesRefD.Length > 0 && hv_midAngleOfTwoLinesRefD.Length > 0 && hv_downAngleOfTwoLinesRefD.Length > 0)
                                            {
                                                HOperatorSet.TupleMax(hv_topAngleOfTwoLinesRefD.TupleConcat(hv_midAngleOfTwoLinesRefD.D).TupleConcat(hv_downAngleOfTwoLinesRefD.D), out HTuple hv_maxDownAngle);

                                                if (hv_maxDownAngle != null && hv_maxDownAngle.TupleGreater(0.015) != 0)
                                                {

                                                    hv_fRatioAngleD = 0.005 / hv_maxDownAngle;
                                                }
                                                else
                                                {
                                                    hv_fRatioAngleD = 0.8;
                                                }
                                            }



                                            CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeights(hObjectTopReference, hv_fRatioT, out hv_RefDiagTopLength, out hv_RefLeftIndexesTop, out hv_RefRightIndexesTop, out hv_RefRowBeginTop, out hv_RefmeanHeightTopL, out hv_RefmeanHeightTopM, out hv_RefmeanHeightTopR, 2);

                                            CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeights(hObjectLeftReference, hv_fRatioL, out hv_RefDiagLeftLength, out hv_RefLeftIndexesLeft, out hv_RefRightIndexesLeft, out hv_RefRowBeginLeft, out hv_RefmeanHeightLeftL, out hv_RefmeanHeightLeftM, out hv_RefmeanHeightLeftR, 2);


                                            CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeights(hObjectRightReference, hv_fRatioR, out hv_RefDiagRightLength, out hv_RefLeftIndexesRight, out hv_RefRightIndexesRight, out hv_RefRowBeginRight, out hv_RefmeanHeightRightL, out hv_RefmeanHeightRightM, out hv_RefmeanHeightRightR, 2);

                                            CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeights(hObjectDownReference, hv_fRatioD, out hv_RefDiagDownLength, out hv_RefLeftIndexesDown, out hv_RefRightIndexesDown, out hv_RefRowBeginDown, out hv_RefmeanHeightDownL, out hv_RefmeanHeightDownM, out hv_RefmeanHeightDownR, 2);


                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                hv_fMeanLength.Dispose();
                                                HOperatorSet.TupleMean(((((hv_RefDiagTopLength.TupleConcat(hv_RefDiagRightLength))).TupleConcat(
                                                    hv_RefDiagLeftLength))).TupleConcat(hv_RefDiagDownLength), out hv_fMeanLength);
                                            }
                                            hv_fValue.Dispose();
                                            HOperatorSet.TupleRand(1, out hv_fValue);
                                            hv_RefDiagTopLength.Dispose();
                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                hv_RefDiagTopLength = hv_fMeanLength + (0.03 * hv_fValue);
                                            }
                                            hv_fValue.Dispose();
                                            HOperatorSet.TupleRand(1, out hv_fValue);
                                            hv_RefDiagRightLength.Dispose();
                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                hv_RefDiagRightLength = hv_fMeanLength + (0.03 * hv_fValue);
                                            }
                                            hv_fValue.Dispose();
                                            HOperatorSet.TupleRand(1, out hv_fValue);
                                            hv_RefDiagLeftLength.Dispose();
                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                hv_RefDiagLeftLength = hv_fMeanLength + (0.03 * hv_fValue);
                                            }
                                            hv_fValue.Dispose();
                                            HOperatorSet.TupleRand(1, out hv_fValue);
                                            hv_RefDiagDownLength.Dispose();
                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                hv_RefDiagDownLength = hv_fMeanLength + (0.03 * hv_fValue);
                                            }

                                            LogHelper.Info("", " " + parInfo.nSquareType.ToString() + " Hv_fStandLTLength " + parInfo.Hv_fStandLTLength.D.ToString("0.00") + "  Hv_fStandRTLength " + parInfo.Hv_fStandRTLength.D.ToString("0.00") + " Hv_fStandTDLength " + parInfo.Hv_fStandTDLength.D.ToString("0.00") + " Hv_fStandLRLength " + parInfo.Hv_fStandLRLength.D.ToString("0.00"));

                                            hv_meanRefRightTopIndex.Dispose();
                                            HOperatorSet.TupleMean(hv_RefRightIndexesTop, out hv_meanRefRightTopIndex);
                                            hv_meanRefLeftTopIndex.Dispose();
                                            HOperatorSet.TupleMean(hv_RefLeftIndexesTop, out hv_meanRefLeftTopIndex);
                                            hv_meanRefLeftDownIndex.Dispose();
                                            HOperatorSet.TupleMean(hv_RefLeftIndexesDown, out hv_meanRefLeftDownIndex);
                                            hv_meanRefRightDownIndex.Dispose();
                                            HOperatorSet.TupleMean(hv_RefRightIndexesDown, out hv_meanRefRightDownIndex);
                                            hv_meanRefRightLeftIndex.Dispose();
                                            HOperatorSet.TupleMean(hv_RefLeftIndexesLeft, out hv_meanRefRightLeftIndex);
                                            hv_meanRefLeftLeftIndex.Dispose();
                                            HOperatorSet.TupleMean(hv_RefLeftIndexesLeft, out hv_meanRefLeftLeftIndex);
                                            hv_meanRefRightRightIndex.Dispose();
                                            HOperatorSet.TupleMean(hv_RefRightIndexesRight, out hv_meanRefRightRightIndex);
                                            hv_meanRefLeftRightIndex.Dispose();
                                            HOperatorSet.TupleMean(hv_RefLeftIndexesRight, out hv_meanRefLeftRightIndex);

                                            hv_RefmeanHeightLeftM.Dispose();
                                            HOperatorSet.TupleMean(hv_RefmeanHeightLeftM, out hv_RefmeanHeightLeftM);
                                            hv_RefmeanHeightLeftL.Dispose();
                                            HOperatorSet.TupleMean(hv_RefmeanHeightLeftL, out hv_RefmeanHeightLeftL);
                                            hv_RefmeanHeightLeftR.Dispose();
                                            HOperatorSet.TupleMean(hv_RefmeanHeightLeftR, out hv_RefmeanHeightLeftR);

                                            hv_RefmeanHeightRightL.Dispose();
                                            HOperatorSet.TupleMean(hv_RefmeanHeightRightL, out hv_RefmeanHeightRightL);
                                            hv_RefmeanHeightRightM.Dispose();
                                            HOperatorSet.TupleMean(hv_RefmeanHeightRightM, out hv_RefmeanHeightRightM);
                                            hv_RefmeanHeightRightR.Dispose();
                                            HOperatorSet.TupleMean(hv_RefmeanHeightRightR, out hv_RefmeanHeightRightR);

                                            hv_RefmeanHeightTopL.Dispose();
                                            HOperatorSet.TupleMean(hv_RefmeanHeightTopL, out hv_RefmeanHeightTopL);
                                            hv_RefmeanHeightTopM.Dispose();
                                            HOperatorSet.TupleMean(hv_RefmeanHeightTopM, out hv_RefmeanHeightTopM);
                                            hv_RefmeanHeightTopR.Dispose();
                                            HOperatorSet.TupleMean(hv_RefmeanHeightTopR, out hv_RefmeanHeightTopR);

                                            hv_RefmeanHeightDownL.Dispose();
                                            HOperatorSet.TupleMean(hv_RefmeanHeightDownL, out hv_RefmeanHeightDownL);
                                            hv_RefmeanHeightDownM.Dispose();
                                            HOperatorSet.TupleMean(hv_RefmeanHeightDownM, out hv_RefmeanHeightDownM);
                                            hv_RefmeanHeightDownR.Dispose();
                                            HOperatorSet.TupleMean(hv_RefmeanHeightDownR, out hv_RefmeanHeightDownR);

                                            dataref.Hv_RefmeanHeightLeftM = hv_RefmeanHeightLeftM;
                                            dataref.Hv_RefmeanHeightLeftL = hv_RefmeanHeightLeftL;
                                            dataref.Hv_RefmeanHeightLeftR = hv_RefmeanHeightLeftR;
                                            dataref.Hv_RefmeanHeightRightL = hv_RefmeanHeightRightL;
                                            dataref.Hv_RefmeanHeightRightM = hv_RefmeanHeightRightM;
                                            dataref.Hv_RefmeanHeightRightR = hv_RefmeanHeightRightR;
                                            dataref.Hv_RefmeanHeightTopL = hv_RefmeanHeightTopL;
                                            dataref.Hv_RefmeanHeightTopM = hv_RefmeanHeightTopM;
                                            dataref.Hv_RefmeanHeightTopR = hv_RefmeanHeightTopR;
                                            dataref.Hv_RefmeanHeightDownL = hv_RefmeanHeightDownL;
                                            dataref.Hv_RefmeanHeightDownR = hv_RefmeanHeightDownR;
                                            dataref.Hv_RefmeanHeightDownM = hv_RefmeanHeightDownM;
                                            dataref.Hv_meanRefRightTopIndex = hv_meanRefRightTopIndex;//TopLeft
                                            dataref.Hv_meanRefLeftTopIndex = hv_meanRefLeftTopIndex; //TopRight
                                            dataref.Hv_meanRefLeftDownIndex = hv_meanRefLeftDownIndex;  //LeftLeft
                                            dataref.Hv_meanRefRightDownIndex = hv_meanRefRightDownIndex; //RightRight
                                            dataref.Hv_meanRefRightLeftIndex = hv_meanRefRightLeftIndex;
                                            dataref.Hv_meanRefLeftLeftIndex = hv_meanRefLeftLeftIndex;//TopLeft
                                            dataref.Hv_meanRefLeftRightIndex = hv_meanRefLeftRightIndex; //TopRight
                                            dataref.Hv_meanRefRightRightIndex = hv_meanRefRightRightIndex;  //LeftLeft





                                            dataref.Hv_fPreRTHeight = parInfo.Hv_fStandRTLength;
                                            dataref.Hv_fPreRTWidth = parInfo.Hv_fStandRTLength;
                                            dataref.Hv_fPreLTHeight = parInfo.Hv_fStandLTLength;
                                            dataref.Hv_fPreLTWidth = parInfo.Hv_fStandLTLength;

                                            dataref.hv_fPreTDLength = parInfo.Hv_fStandTDLength;
                                            dataref.hv_fPreLRLength = parInfo.Hv_fStandLRLength;
                                            dataref.Hv_RefRatioAngleR = 0.187;
                                            dataref.Hv_RefRatioAngleL = 0.8;
                                            dataref.Hv_RefRatioAngleT = 0.8;
                                            dataref.Hv_RefRatioAngleD = 0.8;
                                            dataref.eMT_TYPE = ReferencePosInfoData.EMT_TYPE.EMT_TYPE_OTHER;
                                            dataref.nSquareSiliconType = parInfo.nSquareType;

                                            if (_dictRefTypeRefObject.ContainsKey(ReferencePosInfoData.EMT_TYPE.EMT_TYPE_OTHER))
                                            {
                                                _dictRefTypeRefObject.Remove(ReferencePosInfoData.EMT_TYPE.EMT_TYPE_OTHER);
                                            }
                                            _dictRefTypeRefObject.Add(ReferencePosInfoData.EMT_TYPE.EMT_TYPE_OTHER, dataref);

                                            LogHelper.Info("", "nSquareType " + parInfo.nSquareType.ToString());
                                            if (_dictSquareTypeRefObjects.ContainsKey(parInfo.nSquareType))
                                            {
                                                _dictSquareTypeRefObjects.Remove(parInfo.nSquareType);
                                            }

                                            _dictSquareTypeRefObjects.Add(parInfo.nSquareType, dataref);
                                            LogHelper.Info("", "_dictSquareTypeRefObjects Add nSquareType " + parInfo.nSquareType.ToString());
                                            SaveReferenceObjectsInfo();


                                        }
                                        catch (Exception ex)
                                        {
                                            LogHelper.Info("", "exception is " + ex.ToString());
                                        }

                                    }
                                    if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
                                    {
                                        if (threadMov != null)
                                        {
                                            while (threadMov.ThreadState != ThreadState.Stopped)
                                            {
                                                Thread.Sleep(500);
                                            }

                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
                                    {
                                        if (threadMov != null)
                                        {
                                            while (threadMov.ThreadState != ThreadState.Stopped)
                                            {
                                                Thread.Sleep(500);
                                            }
                                        }
                                    }
                                }



                                break;

                            }
                        }

                        Thread.Sleep(500);
                    }
                }

                if (true == bCalculateStick)
                {
                    break;
                }

            }

            if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
            {
                threadMov = new Thread(() =>
                {
                    LogHelper.Info("Silicon", "ThreadDealSquareInfo Move To Orgin Position");
                    CMoveController.Instance().SetMoveSpeed(SettingParameter.Instance().FPLCMoveSpeed);
                    CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, SettingParameter.Instance().FPosition_fir_s, 20);
                    NTestStatus = emStatus.EM_FREE;
                    //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                });

                threadMov.Start();

                if (threadMov != null)
                {
                    while (threadMov.ThreadState != ThreadState.Stopped)
                    {
                        Thread.Sleep(500);
                    }
                }
            }


            NTestStatus = emStatus.EM_FREE;
        }

        public void ThreadDealSquareInfoAdaptation(object objtype)
        {
            
           
            //GlobalDatastate.Instance().stateUpdate = "初始化图片...";
            #region 初始化

            HObject hObject = new HObject();
            bool bCheckReference = false;
            tagParaInfo paraInfo = (tagParaInfo)objtype;
            int nWaitIndex = 0;
            HOperatorSet.GenEmptyObj(out _ho_ImageLeft);
            HOperatorSet.GenEmptyObj(out _ho_ImageDown);
            HOperatorSet.GenEmptyObj(out _ho_ImageTop);
            HOperatorSet.GenEmptyObj(out _ho_ImageRight);

            HOperatorSet.GenEmptyObj(out _hObjectsLeft);
            HOperatorSet.GenEmptyObj(out _hObjectsRight);
            HOperatorSet.GenEmptyObj(out _hObjectsDown);
            HOperatorSet.GenEmptyObj(out _hObjectsTop);

           bool bValidWait = new bool ();

           bool bCalculateStick=new bool ();


            Thread threadMov = null;
            NTestStatus = emStatus.EM_TESTING;

             HOperatorSet.GenEmptyObj(out hObject);
                if (false == Directory.Exists("D:/Image1"))
                {
                    Directory.CreateDirectory("D:/Image1");
                }

                if (false == Directory.Exists("D:/Image1/left"))
                {
                    Directory.CreateDirectory("D:/Image1/left");
                }

                if (false == Directory.Exists("D:/Image1/right"))
                {
                    Directory.CreateDirectory("D:/Image1/right");
                }

                if (false == Directory.Exists("D:/Image1/top"))
                {
                    Directory.CreateDirectory("D:/Image1/top");
                }

                if (false == Directory.Exists("D:/Image1/down"))
                {
                    Directory.CreateDirectory("D:/Image1/down");
                }

            #endregion
            //GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "相机开始采图完成";
            
            while (true)
            {
               
                Thread.Sleep (1000);
                for (int i = 0; i < 4; i++)
                {
                   
                    #region 加载图片
                    if (true == GetObjectsByType((emSSZNCamType)i, ref hObject))
                    {                      
                        bValidWait = true;
                        switch (i)
                        {
                            case (int)emSSZNCamType.EM_LEFT:
                                {
                                    if (SettingParameter.Instance().NSaveBmp == 1)
                                    {
                                        HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image1/left/" + NLeftIndex.ToString() + ".tif");                                                                           
                                    }
                              
                                    NLeftIndex++;
                                    HOperatorSet.ConcatObj(_hObjectsLeft, hObject, out _hObjectsLeft);
                                    GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "LEFT.";
                                    break;
                                }
                            case (int)emSSZNCamType.EM_RIGHT:
                                {
                                    if (SettingParameter.Instance().NSaveBmp == 1)
                                    {
                                        HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image1/right/" + NRightIndex.ToString() + ".tif");
                                                                           
                                    }                                
                                    NRightIndex++;
                                    HOperatorSet.ConcatObj(_hObjectsRight, hObject, out _hObjectsRight);
                                    GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "RIGHT.";
                                    break;
                                }
                            case (int)emSSZNCamType.EM_TOP:
                                {
                                    if (SettingParameter.Instance().NSaveBmp == 1)
                                    {
                                        HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image1/top/" + NTopIndex.ToString() + ".tif");
                                    
                                        
                                    }
                                   
                                    NTopIndex++;
                                    HOperatorSet.ConcatObj(_hObjectsTop, hObject, out _hObjectsTop);
                                    GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "TOP..";
                                    break;
                                }
                            case (int)emSSZNCamType.EM_DOWN:
                                {
                                    if (SettingParameter.Instance().NSaveBmp == 1)
                                    {
                                        HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image1/down/" + NDownIndex.ToString() + ".tif");
                               
                                       
                                  
                                    }
                                    
                                    NDownIndex++;
                                    HOperatorSet.ConcatObj(_hObjectsDown, hObject, out _hObjectsDown);
                                    GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "DOWN..";
                                    break;
                                }
                        }
                        LogHelper.Info("Silicon", "ThreadDealSquareInfo NLeftIndex " + NLeftIndex.ToString() + " NRightIndex " + NRightIndex.ToString() + " NTopIndex " + NTopIndex.ToString() + " NDownIndex " + NDownIndex.ToString());

                        if (false == bCheckReference && (NLeftIndex==1 && NRightIndex == 1 && NTopIndex == 1 && NDownIndex==1))
                        {

                            bCheckReference = true;

                        }
                        nWaitIndex = 0;
                    }
                    #endregion

                    else
                    {
                        if (bValidWait == true)
                        {
                            nWaitIndex++;
                            if (bCheckReference ==true)
                            {
                               // GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "初始化图片";
                                //LogHelper.Info("Silicon", "camtype " + paraInfo.camtype.ToString() + " No Message , break");

                                if (NLeftIndex >= 1 && NRightIndex >= 1 && NTopIndex >= 1 && NDownIndex >= 1)
                                {
                                    try
                                    {
                                        if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
                                        {
                                            if (SettingParameter.Instance().NCamType == 0)
                                            {
                                                //SSZNCamTools.Instance().StopBatchLoop();
                                            }
                                            else
                                            {
                                                //XGCamTools.Instance.StopCaptures();
                                            }
                                        }
                                        bCalculateStick = true;
                                        //GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "截取图片有效区域";
                                     
                                        #region 图像截取

                                        HObject hobjectLeftStick = new HObject();
                                        HObject hobjectRightStick = new HObject();
                                        HObject hobjectTopStick = new HObject();
                                        HObject hobjectDownStick = new HObject();
                                        HObject Image = new HObject();
                                        HOperatorSet.GenEmptyObj(out hobjectLeftStick);
                                        HOperatorSet.GenEmptyObj(out hobjectDownStick);
                                        HOperatorSet.GenEmptyObj(out hobjectRightStick);
                                        HOperatorSet.GenEmptyObj(out hobjectTopStick);
                                        HOperatorSet.GenEmptyObj(out Image);


                                        HOperatorSet.TileImages(_hObjectsLeft, out _ho_ImageLeft, 1, "vertical");
                                        HOperatorSet.TileImages(_hObjectsRight, out _ho_ImageRight, 1, "vertical");
                                        HOperatorSet.TileImages(_hObjectsTop, out _ho_ImageTop, 1, "vertical");
                                        HOperatorSet.TileImages(_hObjectsDown, out _ho_ImageDown, 1, "vertical");


                                        HOperatorSet.ConcatObj(Image, _ho_ImageLeft, out Image);
                                        HOperatorSet.ConcatObj(Image, _ho_ImageRight, out Image);
                                        HOperatorSet.ConcatObj(Image, _ho_ImageTop, out Image);
                                        HOperatorSet.ConcatObj(Image, _ho_ImageDown, out Image);
                                        #endregion

                                        GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "截取完成";
                                        GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "开始计算...";
                                        HTuple ParmDict = new HTuple();
                                        HTuple Ex = new HTuple();
                                        HOperatorSet.CreateDict(out ParmDict);
                                        HDevelopExport.Instance().Detection_init(ParmDict, out HTuple ResourceDictHandle, out HTuple JSONData, out Ex);
                                        int ser = 100;
                                        try
                                        {
                                             ser = paraInfo.nSquareType;
                                        }
                                        catch (Exception)
                                        {
                                             ser = 100;

                                        }

                                        //ser = 2;
                                        HOperatorSet.SetDictTuple(ResourceDictHandle, "当前规格号", ser);
                                        HDevelopExport.Instance().Detection_process(Image, out HObject SaveImage, out HObject DispImage,
                                         1, ParmDict, ResourceDictHandle, out HTuple AllQuality, out HTuple OutParmDict, out Ex);
                                        GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "计算完成";


                                        #region 录入结果

                                        HTuple g = new HTuple();
                                        HTuple g1 = new HTuple();
                                        g = null;
                                        g1 = null;
                                        SquareStickCheckData data = new SquareStickCheckData();
                                        Thread thread = new Thread(() =>
                                        {
                                            try
                                            {
                                                saveimage(Image, paraInfo.strSerial, ser);
                                                GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "存图完成";
                                            }
                                            catch (Exception)
                                            {
                                                GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "存图失败！请检查路径或硬盘！";


                                            }

                                        });
                                        thread.Start();
                                        float A_PL = 0;
                                        float B_PL = 0;
                                        float DJ1_PL = 0;
                                        float DJ2_PL = 0;
                                        float A_BZ = 0;
                                        float B_BZ = 0;
                                        float DJ1_BZ = 0;
                                        float DJ2_BZ = 0;
                                        float A_Length = 0;
                                        float B_Length = 0;
                                        float C_Length = 0;
                                        float D_Length = 0;
                                        float Length = 0;
                                        float DJ1 = 0;
                                        float DJ2 = 0;
                                        float Arc_Top = 0;
                                        float Arc_Right = 0;
                                        float Arc_Left = 0;
                                        float Arc_Down = 0;
                                        float Per_Top = 0;
                                        float Per_Right = 0;
                                        float Per_Left = 0;
                                        float Per_Down = 0;
                                        float S_Ver = 0;
                                        float E_Ver = 0;
                                        float Arc_Top_Left = 0;
                                        float Arc_Right_Left = 0;
                                        float Arc_Left_Left = 0;
                                        float Arc_Down_Left = 0;

                                        float Arc_Top_Right = 0;
                                        float Arc_Right_Right = 0;
                                        float Arc_Left_Right = 0;
                                        float Arc_Down_Right = 0;

                                        HOperatorSet.GetDictTuple(ResourceDictHandle, "规格" + ser, out HTuple RSTP);
                                        float[] ResultArry = new float[30];
                                        try
                                        {
                                            //GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "存图完成";
                                            HOperatorSet.JsonToDict(AllQuality, g, g1, out HTuple AllQuality1);
                                            HOperatorSet.GetDictTuple(AllQuality1, "结果", out HTuple result);
                                            HOperatorSet.GetDictTuple(result, "A_边长", out HTuple HA_Length);
                                            HOperatorSet.GetDictTuple(result, "B_边长", out HTuple HB_Length);
                                            HOperatorSet.GetDictTuple(result, "C_边长", out HTuple HC_Length);
                                            HOperatorSet.GetDictTuple(result, "D_边长", out HTuple HD_Length);
                                            HOperatorSet.GetDictTuple(result, "棒长", out HTuple HLength);
                                            HOperatorSet.GetDictTuple(result, "对角1", out HTuple HDJ1);
                                            HOperatorSet.GetDictTuple(result, "对角2", out HTuple HDJ2);
                                            HOperatorSet.GetDictTuple(result, "弧长1-1", out HTuple HArc_Top);
                                            HOperatorSet.GetDictTuple(result, "弧长2-1", out HTuple HArc_Right);
                                            HOperatorSet.GetDictTuple(result, "弧长3-1", out HTuple HArc_Left);
                                            HOperatorSet.GetDictTuple(result, "弧长4-1", out HTuple HArc_Down);
                                            HOperatorSet.GetDictTuple(result, "上_垂直度", out HTuple HPer_Top);
                                            HOperatorSet.GetDictTuple(result, "右_垂直度", out HTuple HPer_Right);
                                            HOperatorSet.GetDictTuple(result, "左_垂直度", out HTuple HPer_Left);
                                            HOperatorSet.GetDictTuple(result, "下_垂直度", out HTuple HPer_Down);
                                            HOperatorSet.GetDictTuple(result, "头端面垂直度", out HTuple HS_Ver);
                                            HOperatorSet.GetDictTuple(result, "尾端面垂直度", out HTuple HE_Ver);


                                            HDevelopExport.Instance().HTupToFt(HA_Length, out A_Length);
                                            HDevelopExport.Instance().HTupToFt(HB_Length, out B_Length);
                                            HDevelopExport.Instance().HTupToFt(HC_Length, out C_Length);
                                            HDevelopExport.Instance().HTupToFt(HD_Length, out D_Length);
                                            HDevelopExport.Instance().HTupToFt(HLength, out Length);
                                            HDevelopExport.Instance().HTupToFt(HDJ1, out DJ1);
                                            HDevelopExport.Instance().HTupToFt(HDJ2, out DJ2);
                                            HDevelopExport.Instance().HTupToFt(HArc_Top, out Arc_Top);
                                            HDevelopExport.Instance().HTupToFt(HArc_Right, out Arc_Right);
                                            HDevelopExport.Instance().HTupToFt(HArc_Left, out Arc_Left);
                                            HDevelopExport.Instance().HTupToFt(HArc_Down, out Arc_Down);
                                            HDevelopExport.Instance().HTupToFt(HPer_Top, out Per_Top);
                                            HDevelopExport.Instance().HTupToFt(HPer_Right, out Per_Right);
                                            HDevelopExport.Instance().HTupToFt(HPer_Left, out Per_Left);
                                            HDevelopExport.Instance().HTupToFt(HPer_Down, out Per_Down);
                                            HDevelopExport.Instance().HTupToFt(HS_Ver, out S_Ver);
                                            HDevelopExport.Instance().HTupToFt(HE_Ver, out E_Ver);



                                        }
                                        catch (Exception)
                                        {
                                            GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "数据异常！";
                                            A_Length = 0;
                                            B_Length = 0;
                                            C_Length = 0;
                                            D_Length = 0;
                                            Length = 0;
                                            DJ1 = 0;
                                            DJ2 = 0;
                                            Arc_Top = 0;
                                            Arc_Right = 0;
                                            Arc_Left = 0;
                                            Arc_Down = 0;
                                            Per_Top = 0;
                                            Per_Right = 0;
                                            Per_Left = 0;
                                            Per_Down = 0;
                                            S_Ver = 0;
                                            E_Ver = 0;
                                            Arc_Top_Left = 0;
                                            Arc_Top_Right = 0;
                                            Arc_Down_Left = 0;
                                            Arc_Down_Right = 0;
                                            Arc_Left_Left = 0;
                                            Arc_Left_Right = 0;
                                            Arc_Down_Left = 0;
                                            Arc_Down_Right = 0;
                                        }
                                        ResultArry[0] = Modify(ser, "A_边长_偏量修正", A_Length);//A边长
                                        ResultArry[1] = Modify(ser, "B_边长_偏量修正", B_Length);//B边长
                                        ResultArry[2] = Modify(ser, "对角线1_偏量修正", DJ1);//对角1
                                        ResultArry[3] = Modify(ser, "对角线2_偏量修正", DJ2);//对角2
                                        ResultArry[4] = Modify(ser, "C_边长_偏量修正", C_Length);//C边长
                                        ResultArry[5] = Modify(ser, "D_边长_偏量修正", D_Length);//D边长
                                        ResultArry[6] = Modify(ser, "端面垂直度_头部修正", S_Ver);//头端面垂直度
                                        ResultArry[7] = Modify(ser, "端面垂直度_尾部修正", E_Ver);//尾端面垂直度
                                        ResultArry[8] = Modify(ser, "上弧长_偏量修正", Arc_Top);//头端面垂直度
                                        ResultArry[9] = Modify(ser, "下弧长_偏量修正", Arc_Down);
                                        ResultArry[10] = Modify(ser, "左弧长_偏量修正", Arc_Left);
                                        ResultArry[11] = Modify(ser, "右弧长_偏量修正", Arc_Right);
                                        ResultArry[12] = Modify(ser, "上侧边垂直度_偏量修正", Per_Top);
                                        ResultArry[13] = Modify(ser, "下侧边垂直度_偏量修正", Per_Down);
                                        ResultArry[14] = Modify(ser, "左侧边垂直度_偏量修正", Per_Left);
                                        ResultArry[15] = Modify(ser, "右侧边垂直度_偏量修正", Per_Right);

                                        float late = 0.707F;
                                        float late2 = 0.707F;
                                        if (ser == 100)
                                        {
                                            late = 0.65F;
                                            late2 = 0.75F;
                                        }
                                         Arc_Top_Left = Arc_Top* late;
                                         Arc_Right_Left = Arc_Right * late;
                                         Arc_Left_Left = Arc_Left * late;
                                         Arc_Down_Left = Arc_Down * late;

                                         Arc_Top_Right = Arc_Top * late2;
                                         Arc_Right_Right = Arc_Right * late2;
                                         
                                        Arc_Left_Right = Arc_Left * late2;
                                         Arc_Down_Right = Arc_Down * late2;
                                        ResultArry[16] = Modify(ser, "上弧长左投影_偏量修正", Arc_Top_Left);
                                        ResultArry[17] = Modify(ser, "上弧长右投影_偏量修正", Arc_Top_Right);
                                        ResultArry[18] = Modify(ser, "下弧长左投影_偏量修正", Arc_Down_Left);
                                        ResultArry[19] = Modify(ser, "下弧长右投影_偏量修正", Arc_Down_Right);
                                        ResultArry[20] = Modify(ser, "左弧长左投影_偏量修正", Arc_Left_Left);
                                        ResultArry[21] = Modify(ser, "左弧长右投影_偏量修正", Arc_Left_Right);
                                        ResultArry[22] = Modify(ser, "右弧长左投影_偏量修正", Arc_Right_Left);
                                        ResultArry[23] = Modify(ser, "右弧长右投影_偏量修正", Arc_Right_Right);

                                        int Result = 1;
                                        //NG_Juge(ser, ResultArry, out int Result, out string Result_State);
                                        if (Result == 1)
                                        {
                                            //GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "NG信息：" + Result_State;
                                        }

                                        try
                                        {
                                            data.ListLTLength.Clear();
                                            data.ListRTLength.Clear();
                                            data.ListLDLength.Clear();
                                            data.ListRDLength.Clear();

                                            data.ListTDLength.Clear();
                                            data.ListLRLength.Clear();
                                            data.ListLeftDiagLength.Clear();
                                            data.ListRightDiagLength.Clear();
                                            data.ListTopDiagLength.Clear();
                                            data.ListDownDiagLength.Clear();
                                            data.ListLeftLeftDiagLength.Clear();
                                            data.ListLeftRightDiagLength.Clear();
                                            data.ListRightLeftDiagLength.Clear();
                                            data.ListRightRightDiagLength.Clear();
                                            data.ListDownLeftDiagLength.Clear();
                                            data.ListDownRightDiagLength.Clear();
                                            data.ListTopLeftDiagLength.Clear();
                                            data.ListTopRightDiagLength.Clear();
                                            data.ListTopAngle.Clear();
                                            data.ListTopAngle.Clear();
                                            data.ListTopAngle.Clear();
                                            data.ListDownAngle.Clear();
                                            data.ListDownAngle.Clear();
                                            data.ListDownAngle.Clear();
                                            data.ListLeftAngle.Clear();
                                            data.ListLeftAngle.Clear();
                                            data.ListLeftAngle.Clear();
                                            data.ListRightAngle.Clear();
                                            data.ListRightAngle.Clear();
                                            data.ListRightAngle.Clear();


                                            data.ListLTLength.Add(Modify(ser, "A_边长_偏量修正", A_Length));
                                            data.ListRTLength.Add(Modify(ser, "B_边长_偏量修正", B_Length));
                                            data.ListLDLength.Add(Modify(ser, "D_边长_偏量修正", D_Length));
                                            data.ListRDLength.Add(Modify(ser, "C_边长_偏量修正", C_Length));
                                            data.FLength = Length;
                                            data.ListTDLength.Add(Modify(ser, "对角线1_偏量修正", DJ1));
                                            data.ListLRLength.Add(Modify(ser, "对角线2_偏量修正", DJ2));
                                            data.ListLeftDiagLength.Add(Modify(ser, "左弧长_偏量修正", Arc_Left));
                                            data.ListRightDiagLength.Add(Modify(ser, "右弧长_偏量修正", Arc_Right));
                                            data.ListTopDiagLength.Add(Modify(ser, "上弧长_偏量修正", Arc_Top));
                                            data.ListDownDiagLength.Add(Modify(ser, "下弧长_偏量修正", Arc_Down));
                            
                                            data.ListLeftLeftDiagLength.Add(Modify(ser, "左弧长左投影_偏量修正", Arc_Left_Left));
                                            data.ListLeftRightDiagLength.Add(Modify(ser, "左弧长右投影_偏量修正", Arc_Left_Right));
                                            data.ListRightLeftDiagLength.Add(Modify(ser, "右弧长左投影_偏量修正", Arc_Right_Left));
                                            data.ListRightRightDiagLength.Add(Modify(ser, "右弧长右投影_偏量修正", Arc_Right_Right));
                                            data.ListDownRightDiagLength.Add(Modify(ser, "下弧长左投影_偏量修正", Arc_Down_Left));
                                            data.ListDownLeftDiagLength.Add(Modify(ser, "下弧长右投影_偏量修正", Arc_Down_Right));
                                            data.ListTopLeftDiagLength.Add(Modify(ser, "上弧长左投影_偏量修正", Arc_Top_Left));
                                            data.ListTopRightDiagLength.Add(Modify(ser, "上弧长右投影_偏量修正", Arc_Top_Right));
                                            data.ListTopAngle.Add(Modify(ser, "上侧边垂直度_偏量修正", Per_Top));
                                           // data.ListTopAngle.Add(Modify(ser, "上侧边垂直度_偏量修正", Per_Top));
                                           // data.ListTopAngle.Add(Modify(ser, "上侧边垂直度_偏量修正", Per_Top));


                                            data.ListDownAngle.Add(Modify(ser, "下侧边垂直度_偏量修正", Per_Down));
                                            //data.ListDownAngle.Add(Modify(ser, "下侧边垂直度_偏量修正", Per_Down));
                                            //data.ListDownAngle.Add(Modify(ser, "下侧边垂直度_偏量修正", Per_Down));

                                            data.ListLeftAngle.Add(Modify(ser, "左侧边垂直度_偏量修正", Per_Left));
                                            //data.ListLeftAngle.Add(Modify(ser, "左侧边垂直度_偏量修正", Per_Left));
                                            //data.ListLeftAngle.Add(Modify(ser, "左侧边垂直度_偏量修正", Per_Left));

                                            data.ListRightAngle.Add(Modify(ser, "右侧边垂直度_偏量修正", Per_Right));
                                            //data.ListRightAngle.Add(Modify(ser, "右侧边垂直度_偏量修正", Per_Right));
                                            //data.ListRightAngle.Add(Modify(ser, "右侧边垂直度_偏量修正", Per_Right));

                                            data.NResult = Result.ToString();
                                            data.SVer = Modify(ser, "端面垂直度_头部修正", S_Ver);
                                            data.EVer = Modify(ser, "端面垂直度_尾部修正", E_Ver);
                                            data.StrJBSearial = paraInfo.strSerial;
                                            //data.StrJBSearial = paraInfo.strSerial;
                                            data.NSquareType = ser;
                                            data.Mnum = paraInfo.mnum;
                                        }
                                        catch (Exception)
                                        {


                                        }
                                        try
                                        {
                                            GlobalDatastate.Instance().sernum = paraInfo.strSerial;

                                            Generate_form.Instance().ExportToExcel(data);
                                             GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "导出表格完成";
                                            GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, data);
                                            GlobalDatastate.Instance().timstart = true;

                                        }
                                        catch (Exception)
                                        {


                                        }

                                        #endregion



                                        break;

                                    }
                                    catch (Exception ex)
                                    {
                                        LogHelper.Info("", "exception ThreadDealSquareInfo msg");

                                    }
                                  


                                }
                                else
                                {
                                   // bCalculateStick = true;
                                    SquareStickCheckData datae = new SquareStickCheckData();
                                    datae.FLength = -1;
                                    datae.StrJBSearial = paraInfo.strSerial;
                                    //GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, datae);

/*
                                    threadMov = new Thread(() =>
                                    {
                                        LogHelper.Info("Silicon", "ThreadDealSquareInfo Move To Orgin Position");
                                        CMoveController.Instance().SetMoveSpeed(SettingParameter.Instance().FPLCMoveSpeed);
                                        CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, SettingParameter.Instance().FPosition_fir_s, 20);
                                        NTestStatus = emStatus.EM_FREE;
                                        //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                                    })
*/
                                    NTestStatus = emStatus.EM_GOTOORIGIN;
                                    //threadMov.Start();


                                    if (threadMov != null)
                                    {
                                        while (threadMov.ThreadState != ThreadState.Stopped)
                                        {
                                            Thread.Sleep(500);
                                        }

                                    }

                                    LogHelper.Info("", "ThreadDealSquareInfo No Data NLeftIndex " + NLeftIndex.ToString() + " NRightIndex " + NRightIndex.ToString() + " NTopIndex " + NTopIndex.ToString() + " NDownIndex " + NDownIndex.ToString());
                                    break;

                                }

                              
                            }
                        }

                        Thread.Sleep(500);
                    }
                }

                if (true == bCalculateStick)
               {
                    GlobalDatastate.Instance().stateUpdate=GlobalDatastate.Instance().stateUpdate + "\r\n" + "检测完成";
                    break;
                }

            }
            if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
            {
                if (threadMov != null)
                {
                    while (threadMov.ThreadState != ThreadState.Stopped)
                    {
                        Thread.Sleep(500);
                    }

                }

            }
            NTestStatus = emStatus.EM_FREE;
        }
        

        private void NG_Juge(int SER, Array ResultArry, out int Result, out string Result_State)
        {
            Result = 0;
            Result_State = "";

            string jsonFilePath = "规格.json";
            //StreamReader sr = new StreamReader(jsonFilePath);
            using (StreamReader sr = new StreamReader(jsonFilePath))
            {
                string json = sr.ReadToEnd();
                string type = "规格" + SER;
                // 解析 JSON 字符串
                dynamic data = JsonConvert.DeserializeObject(json);
                dynamic yt = data[type];


                #region 标准值
                string y = yt["A_边长_标准值"];
                float A_BZ = float.Parse(y);
                y = yt["B_边长_标准值"];
                float B_BZ = float.Parse(y);
                y = yt["C_边长_标准值"];
                float C_BZ = float.Parse(y);
                y = yt["D_边长_标准值"];
                float D_BZ = float.Parse(y);



                y = yt["对角线1_标准值"];
                float DJ1_BZ = float.Parse(y);
                y = yt["对角线2_标准值"];
                float DJ2_BZ = float.Parse(y);

                
                //float R_H_BZ = float.Parse(yt["右弧长_标准值"]);
                //float L_H_BZ = float.Parse(yt["左弧长_标准值"]);
                //float T_H_BZ = float.Parse(yt["下弧长_标准值"]);
                //float D_H_BZ = float.Parse(yt["上弧长_标准值"]);

                float R_Ang_BZ = float.Parse(yt["右侧边垂直度_标准值"]);
                float L_Ang_BZ = float.Parse(yt["左侧边垂直度_标准值"]);
                float T_Ang_BZ = float.Parse(yt["上侧边垂直度_标准值"]);
                float D_Ang_BZ = float.Parse(yt["下侧边垂直度_标准值"]);

                float R_Arc_BZ = float.Parse(yt["右弧长_标准值"]);
                float L_Arc_BZ = float.Parse(yt["左弧长_标准值"]);
                float T_Arc_BZ = float.Parse(yt["上弧长_标准值"]);
                float D_Arc_BZ = float.Parse(yt["下弧长_标准值"]);

                float R_Arc_R_BZ = float.Parse(yt["右弧长右投影_标准值"]);
                float L_Arc_R_BZ = float.Parse(yt["左弧长右投影_标准值"]);
                float T_Arc_R_BZ = float.Parse(yt["上弧长右投影_标准值"]);
                float D_Arc_R_BZ = float.Parse(yt["下弧长右投影_标准值"]);

                float R_Arc_L_BZ = float.Parse(yt["右弧长左投影_标准值"]);
                float L_Arc_L_BZ = float.Parse(yt["左弧长左投影_标准值"]);
                float T_Arc_L_BZ = float.Parse(yt["上弧长左投影_标准值"]);
                float D_Arc_L_BZ = float.Parse(yt["下弧长左投影_标准值"]);

                #endregion

                #region 正负容差

                y = yt["A_边长_正容差"];
                float A_ZRC = float.Parse(y);
                y = yt["B_边长_正容差"];
                float B_ZRC = float.Parse(y);
                y = yt["C_边长_正容差"];
                float C_ZRC = float.Parse(y);
                y = yt["D_边长_正容差"];
                float D_ZRC = float.Parse(y);


                y = yt["对角线1_正容差"];
                float DJ1_ZRC = float.Parse(y);
                y = yt["对角线2_正容差"];
                float DJ2_ZRC = float.Parse(y);


                float R_Ang_ZRC = float.Parse(yt["右侧边垂直度_正容差"]);
                float L_Ang_ZRC = float.Parse(yt["左侧边垂直度_正容差"]);
                float T_Ang_ZRC = float.Parse(yt["上侧边垂直度_正容差"]);
                float D_Ang_ZRC = float.Parse(yt["下侧边垂直度_正容差"]);

                float R_Arc_ZRC = float.Parse(yt["右弧长_正容差"]);
                float L_Arc_ZRC = float.Parse(yt["左弧长_正容差"]);
                float T_Arc_ZRC = float.Parse(yt["上弧长_正容差"]);
                float D_Arc_ZRC = float.Parse(yt["下弧长_正容差"]);

                float R_Arc_R_ZRC = float.Parse(yt["右弧长右投影_正容差"]);
                float L_Arc_R_ZRC = float.Parse(yt["左弧长右投影_正容差"]);
                float T_Arc_R_ZRC = float.Parse(yt["上弧长右投影_正容差"]);
                float D_Arc_R_ZRC = float.Parse(yt["下弧长右投影_正容差"]);

                float R_Arc_L_ZRC = float.Parse(yt["右弧长左投影_正容差"]);
                float L_Arc_L_ZRC = float.Parse(yt["左弧长左投影_正容差"]);
                float T_Arc_L_ZRC = float.Parse(yt["上弧长左投影_正容差"]);
                float D_Arc_L_ZRC = float.Parse(yt["下弧长左投影_正容差"]);




                y = yt["A_边长_负容差"];
                float A_FRC = float.Parse(y);
                y = yt["B_边长_负容差"];
                float B_FRC = float.Parse(y);

                y = yt["C_边长_负容差"];
                float C_FRC = float.Parse(y);
                y = yt["D_边长_负容差"];
                float D_FRC = float.Parse(y);

                y = yt["对角线1_负容差"];
                float DJ1_FRC = float.Parse(y);
                y = yt["对角线2_负容差"];
                float DJ2_FRC = float.Parse(y);

                float R_Ang_FRC = float.Parse(yt["右侧边垂直度_负容差"]);
                float L_Ang_FRC = float.Parse(yt["左侧边垂直度_负容差"]);
                float T_Ang_FRC = float.Parse(yt["上侧边垂直度_负容差"]);
                float D_Ang_FRC = float.Parse(yt["下侧边垂直度_负容差"]);

                float R_Arc_FRC = float.Parse(yt["右弧长_负容差"]);
                float L_Arc_FRC = float.Parse(yt["左弧长_负容差"]);
                float T_Arc_FRC = float.Parse(yt["上弧长_负容差"]);
                float D_Arc_FRC = float.Parse(yt["下弧长_负容差"]);

                float R_Arc_R_FRC = float.Parse(yt["右弧长右投影_负容差"]);
                float L_Arc_R_FRC = float.Parse(yt["左弧长右投影_负容差"]);
                float T_Arc_R_FRC = float.Parse(yt["上弧长右投影_负容差"]);
                float D_Arc_R_FRC = float.Parse(yt["下弧长右投影_负容差"]);

                float R_Arc_L_FRC = float.Parse(yt["右弧长左投影_负容差"]);
                float L_Arc_L_FRC = float.Parse(yt["左弧长左投影_负容差"]);
                float T_Arc_L_FRC = float.Parse(yt["上弧长左投影_负容差"]);
                float D_Arc_L_FRC = float.Parse(yt["下弧长左投影_负容差"]);



                #endregion
                #region 端面垂直度
                y = yt["端面垂直度_单面上限"];
                float D_SX = float.Parse(y);
                y = yt["端面垂直度_双面上限"];
                float S_SX = float.Parse(y);
                #endregion
                #region 判断是否NG
                double A = ResultArry.GetValue(0).ToDouble();
                if (A > (A_BZ + A_ZRC) || A < (A_BZ - A_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "A面边长不合格";
                }
                double B = ResultArry.GetValue(1).ToDouble();
                if (B > (B_BZ + B_ZRC) || B < (B_BZ - B_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "B面边长不合格";
                }
                double C = ResultArry.GetValue(4).ToDouble();
                if (C > (C_BZ + C_ZRC) || C < (C_BZ - C_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "C面边长不合格";
                }
                double D = ResultArry.GetValue(5).ToDouble();
                if (D > (D_BZ + D_ZRC) || D < (D_BZ - D_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "D面边长不合格";
                }
                double S_DM = ResultArry.GetValue(6).ToDouble();
                double T_DM = ResultArry.GetValue(7).ToDouble();
                if (S_DM > D_SX || T_DM > D_SX || (S_DM + T_DM) > S_SX)
                {
                    Result = 1;
                    Result_State = Result_State + "端面垂直不合格";
                }

                double DJ1 = ResultArry.GetValue(2).ToDouble();
                if (DJ1 > (DJ1_BZ + DJ1_ZRC) || DJ1 < (DJ1_BZ - DJ1_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "对角线1不合格";
                }
                double DJ2 = ResultArry.GetValue(3).ToDouble();
                if (DJ2 > (DJ2_BZ + DJ2_ZRC) || DJ2 < (DJ2_BZ - DJ2_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "对角线2不合格";
                }


                double T_Arc = ResultArry.GetValue(8).ToDouble();
                if (T_Arc > (T_Arc_BZ + T_Arc_ZRC) || T_Arc < (T_Arc_BZ - T_Arc_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "上弧长不合格";
                }
                double D_Arc = ResultArry.GetValue(9).ToDouble();
                if (D_Arc > (D_Arc_BZ + D_Arc_ZRC) || D_Arc < (D_Arc_BZ - D_Arc_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "下弧长不合格";
                }
                double L_Arc = ResultArry.GetValue(10).ToDouble();
                if (L_Arc > (L_Arc_BZ + L_Arc_ZRC) || L_Arc < (L_Arc_BZ - L_Arc_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "左弧长不合格";
                }
                double R_Arc = ResultArry.GetValue(11).ToDouble();
                if (R_Arc > (R_Arc_BZ + R_Arc_ZRC) || R_Arc < (R_Arc_BZ - R_Arc_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "右弧长不合格";
                }

                double T_Ang = ResultArry.GetValue(12).ToDouble();
                if (T_Ang > (T_Ang_BZ + T_Ang_ZRC) || T_Ang < (T_Ang_BZ - T_Ang_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "上侧边垂直度不合格";
                }
                double D_Ang = ResultArry.GetValue(13).ToDouble();
                if (D_Ang > (D_Ang_BZ + D_Ang_ZRC) || D_Ang < (D_Ang_BZ - D_Ang_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "下侧边垂直度不合格";
                }
                double L_Ang = ResultArry.GetValue(14).ToDouble();
                if (L_Ang > (L_Ang_BZ + L_Ang_ZRC) || L_Ang < (L_Ang_BZ - L_Ang_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "左侧边垂直度不合格";
                }
                double R_Ang = ResultArry.GetValue(15).ToDouble();
                if (R_Ang > (R_Ang_BZ + R_Ang_ZRC) || R_Ang < (R_Ang_BZ - R_Ang_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "右侧边垂直度不合格";
                }

                double T_Arc_L = ResultArry.GetValue(16).ToDouble();
                if (T_Arc_L > (T_Arc_L_BZ + T_Arc_L_ZRC) || T_Arc_L < (T_Arc_L_BZ - T_Arc_L_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "上弧长左投影不合格";
                }
                double T_Arc_R = ResultArry.GetValue(17).ToDouble();
                if (T_Arc_R > (T_Arc_R_BZ + T_Arc_R_ZRC) || T_Arc_R < (T_Arc_R_BZ - T_Arc_R_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "上弧长右投影不合格";
                }
                double D_Arc_L = ResultArry.GetValue(18).ToDouble();
                if (D_Arc_L > (D_Arc_L_BZ + D_Arc_L_ZRC) || D_Arc_L < (D_Arc_L_BZ - D_Arc_L_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "下弧长左投影不合格";
                }

                double D_Arc_R = ResultArry.GetValue(19).ToDouble();
                if (D_Arc_R > (D_Arc_R_BZ + D_Arc_R_ZRC) || D_Arc_R < (D_Arc_R_BZ - D_Arc_R_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "下弧长右投影不合格";
                }
                double L_Arc_L = ResultArry.GetValue(20).ToDouble();
                if (L_Arc_L > (L_Arc_L + L_Arc_L_ZRC) || L_Arc_L < (L_Arc_L_BZ - L_Arc_L_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "左弧长左投影不合格";
                }
                double L_Arc_R = ResultArry.GetValue(21).ToDouble();
                if (L_Arc_R > (L_Arc_R + L_Arc_R_ZRC) || L_Arc_R < (L_Arc_R_BZ - L_Arc_R_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "左弧长右投影不合格";
                }
                double R_Arc_L = ResultArry.GetValue(22).ToDouble();
                if (R_Arc_L > (R_Arc_L + R_Arc_L_ZRC) || R_Arc_L < (R_Arc_L_BZ - R_Arc_L_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "右弧长左投影不合格";
                }
                double R_Arc_R = ResultArry.GetValue(22).ToDouble();
                if (R_Arc_R > (R_Arc_R + R_Arc_R_ZRC) || R_Arc_R < (R_Arc_R_BZ - R_Arc_R_FRC))
                {
                    Result = 1;
                    Result_State = Result_State + "右弧长右投影不合格";
                }
                if (Result == 0)
                {
                    Result_State = Result_State + "合格";
                }


                #endregion
            }

        }
        private static float Modify(int SER, string modfiy, float Rvalue)
        {
            string jsonFilePath = "规格.json";
            //StreamReader sr = new StreamReader(jsonFilePath);
            using (StreamReader sr = new StreamReader(jsonFilePath))
            {
                string json = sr.ReadToEnd();
                string type = "规格" + SER;
                // 解析 JSON 字符串
                dynamic data = JsonConvert.DeserializeObject(json);
                dynamic yt = data[type];
                string y = yt[modfiy];
                float xz = float.Parse(y);

                float Value = Rvalue + xz;
                return Value;


            }
        }
        public void saveimage(HObject image,string str,int ser)
        {
            string tim = DateTime.Now.ToLongDateString().ToString();
            string tim1 = DateTime.Now.ToShortTimeString().ToString();
            string[] time2 = tim1.Split(':');
            if (!System.IO.Directory.Exists(@"D:\\Image_risen\\" + tim +"\\" + ser+"\\" +str +"_"+ser))
            {
                System.IO.Directory.CreateDirectory(@"D:\\Image_risen\\" + tim +"\\" + ser + "\\" + str + "_" + ser);//不存在就创建文件夹
            }
            HOperatorSet.WriteObject(image, "D:\\Image_risen\\" + tim +"\\" + ser + "\\" + str + "_" + ser + "\\"+ time2[0] + "_" + time2[1]);
         }

        public void  StopBatchLoop()
        {
            StopBatchLoop(emSSZNCamType.EM_TOP);
            StopBatchLoop(emSSZNCamType.EM_DOWN);
            StopBatchLoop(emSSZNCamType.EM_LEFT);
            StopBatchLoop(emSSZNCamType.EM_RIGHT);

        }
        public void BatchLoop()
        {
            SetBatchLoop(emSSZNCamType.EM_TOP);
            SetBatchLoop(emSSZNCamType.EM_DOWN);
            SetBatchLoop(emSSZNCamType.EM_LEFT);
            SetBatchLoop(emSSZNCamType.EM_RIGHT);

        }

        public void BatchBlock()
        {
            SetBatchBlcok(emSSZNCamType.EM_TOP);
            SetBatchBlcok(emSSZNCamType.EM_DOWN);
            SetBatchBlcok(emSSZNCamType.EM_LEFT);
            SetBatchBlcok(emSSZNCamType.EM_RIGHT);
        }

        public void SoftTrig()
        {
            Bstop = false;
            SoftTrigOne(emSSZNCamType.EM_TOP);
            SoftTrigOne(emSSZNCamType.EM_DOWN);
            SoftTrigOne(emSSZNCamType.EM_LEFT);
            SoftTrigOne(emSSZNCamType.EM_RIGHT);
        }

        public void StopTrig()
        {
            StopMessure(emSSZNCamType.EM_TOP);
            StopMessure(emSSZNCamType.EM_DOWN);
            StopMessure(emSSZNCamType.EM_LEFT);
            StopMessure(emSSZNCamType.EM_RIGHT);
        }

        public void Measure_CollectionSquareStick(float fStandLTRDLength, float fStandRTLDLength, float fStandTDLength, float fStandLRLength, int nSquareType )
        {
            HObject objrefleft = new HObject();
            HObject objrefright = new HObject();
            HObject objreftop = new HObject();
            HObject objrefdown = new HObject();


            HOperatorSet.GenEmptyObj(out objrefleft);
            HOperatorSet.GenEmptyObj(out objrefright);
            HOperatorSet.GenEmptyObj(out objreftop);
            HOperatorSet.GenEmptyObj(out objrefdown);


            SSZNCamTools.Instance().ClearObjectsByType(emSSZNCamType.EM_TOP);
            SSZNCamTools.Instance().ClearObjectsByType(emSSZNCamType.EM_RIGHT);
            SSZNCamTools.Instance().ClearObjectsByType(emSSZNCamType.EM_LEFT);
            SSZNCamTools.Instance().ClearObjectsByType(emSSZNCamType.EM_RIGHT);

            SSZNCamTools.Instance().ClearGrayObjectsByType(emSSZNCamType.EM_TOP);
            SSZNCamTools.Instance().ClearGrayObjectsByType(emSSZNCamType.EM_RIGHT);
            SSZNCamTools.Instance().ClearGrayObjectsByType(emSSZNCamType.EM_LEFT);
            SSZNCamTools.Instance().ClearGrayObjectsByType(emSSZNCamType.EM_RIGHT);


            tagParaCollectInfo parInfo = new tagParaCollectInfo();
            parInfo.Hv_fStandLRLength = new HTuple(fStandLRLength);
            parInfo.Hv_fStandLTLength = new HTuple(fStandLTRDLength);
            parInfo.Hv_fStandRTLength = new HTuple(fStandRTLDLength);
            parInfo.Hv_fStandTDLength = new HTuple(fStandTDLength);
            parInfo.nSquareType = nSquareType;


            _threadright = new Thread(new ParameterizedThreadStart(ThreadDealSquareInfocollection));
            _threadright.Start(parInfo);


            while (true)
            {
                if (_threadright.ThreadState != ThreadState.Stopped)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                else
                {
                    break;
                }
            }

        }

        public void Measure_SquareStick(string strSerialNum, int nSquareType = 0,int Mnum=0)
        {
            
            //if (SettingParameter.Instance().NDaemon == 1)
            //{
            //    SoftTrigOne(emSSZNCamType.EM_TOP);
            //    SoftTrigOne(emSSZNCamType.EM_DOWN);
            //    SoftTrigOne(emSSZNCamType.EM_LEFT);
            //    SoftTrigOne(emSSZNCamType.EM_RIGHT);
            //}

            HObject objrefleft = new HObject();
            HObject objrefright = new HObject();
            HObject objreftop = new HObject();
            HObject objrefdown = new HObject();


            HOperatorSet.GenEmptyObj(out objrefleft);
            HOperatorSet.GenEmptyObj(out objrefright);
            HOperatorSet.GenEmptyObj(out objreftop);
            HOperatorSet.GenEmptyObj(out objrefdown);


            SSZNCamTools.Instance().ClearObjectsByType(emSSZNCamType.EM_TOP);
            SSZNCamTools.Instance().ClearObjectsByType(emSSZNCamType.EM_RIGHT);
            SSZNCamTools.Instance().ClearObjectsByType(emSSZNCamType.EM_LEFT);
            SSZNCamTools.Instance().ClearObjectsByType(emSSZNCamType.EM_RIGHT);

            SSZNCamTools.Instance().ClearGrayObjectsByType(emSSZNCamType.EM_TOP);
            SSZNCamTools.Instance().ClearGrayObjectsByType(emSSZNCamType.EM_RIGHT);
            SSZNCamTools.Instance().ClearGrayObjectsByType(emSSZNCamType.EM_LEFT);
            SSZNCamTools.Instance().ClearGrayObjectsByType(emSSZNCamType.EM_RIGHT);

            //SSZNCamTools.Instance().BNeedInsert3DObject = true;

            tagParaInfo tagInfo = new tagParaInfo(emSSZNCamType.EM_LEFT,  strSerialNum , nSquareType,Mnum);


            
            _threadleft = new Thread(new ParameterizedThreadStart(ThreadDealSquareInfoAdaptation));        
            _threadleft.Start(tagInfo);
           

            while(true)
            {
               if (_threadleft.ThreadState != ThreadState.Stopped )
                {
                    Thread.Sleep(1000);
                    continue;
                }
                else
                {
                    break;
                }
            }
         
        }

        public void Connect(emSSZNCamType camtype, string strIP)
        {
            try
            {
                _cam3DArray[(int)camtype].connect(strIP);
                Thread.Sleep(500); 
                _cam3DArray[(int)camtype].DataOnetimeCallBack();

            }
            catch(Exception e)
            {

            }
           
        }
        
        public void Disconnect(emSSZNCamType camtype)
        {
            try
            {
                _cam3DArray[(int)camtype].StopMeasure();
                _cam3DArray[(int)camtype].disconnect();
            }
            catch(Exception e )
            {

            }
        }

        public void StopMessure(emSSZNCamType camtype)
        {
            try
            {

                _cam3DArray[(int)camtype].StopMeasure();
            }
            catch (Exception ex)
            {

            }
        }
        public void SetBatchBlcok(emSSZNCamType camtype)
        {
            try
            {
               
                _cam3DArray[(int)camtype].DataBlockReceived(0);
            }
            catch (Exception ex)
            {

            }
            
        }

        public void setBatchLoopMock()
        {
            Thread thread = new Thread(() =>
            {
                for(int i = 0; i <= 23; i++)
                {
                    Thread.Sleep(500);
                    SSZNCamTools_ShowImageLoopTop(true);
                    SSZNCamTools_ShowImageLoopLeft(true);
                    SSZNCamTools_ShowImageLoopRight(true);
                    SSZNCamTools_ShowImageLoopDown(true);
                }
            });

            thread.Start();
        }

        public void StopBatchLoop(emSSZNCamType camtype)
        {
            try
            {
                _cam3DArray[(int)camtype].StopMeasure();

            }
            catch (Exception ex)
            {

            }
        }

        public void SetBatchLoop(emSSZNCamType camtype)
        {
            try
            {
                _cam3DArray[(int)camtype].BatchRollProfilePoint = ((uint)35000);
                _cam3DArray[(int)camtype].LoopReflushCount = ((uint)1000);
                _cam3DArray[(int)camtype].LoopProfilePoint = ((uint)2000);
                _cam3DArray[(int)camtype].DataLoopReceived();

            }
            catch (Exception ex)
            {

            }

        }
        public void SoftTrigOne(emSSZNCamType camtype)
        {
            try
            {
               // _cam3DArray[(int)camtype].DataOnetimeCallBack();
                _cam3DArray[(int)camtype].softTrigOne();
            }
            catch(Exception ex)
            {
                
            }
        }

        public void SnapData(emSSZNCamType camtype)
        {
            try
            {
                _cam3DArray[(int)camtype].DataOnetimeCallBack();
            }
            catch(Exception ex)
            {

            }
        }

        private void SSZNCamTools_ShowNum1(string info1)
        {
            
        }

        private void SSZNCamTools_ShowInfo1(string info1)
        {
            LogHelper.Info("", info1);
        }
    }
}
