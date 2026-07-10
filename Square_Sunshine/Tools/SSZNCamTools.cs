
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
using SquareSiliconStickCheck.Cameras.XG;
using static SquareSiliconStickCheck.Tools.CMotionCardController;

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

        public tagParaInfo(emSSZNCamType camtyp,  string strserialnum, int nType = 0)
        {
            camtype = camtyp;
            
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
            //SaveReferenceObjectsInfo();
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
                        _cam3DArray[(int)camtype]._ShowImageLoop += SSZNCamTools_ShowImageLoopTop;
                        Connect(camtype, SettingParameter.Instance().StrTopLaser3DIP);
                        break;
                    }
                case emSSZNCamType.EM_LEFT:
                    {
                        _cam3DArray[(int)camtype]._ShowImage += SSZNCamTools_ShowImage_Left;
                        _cam3DArray[(int)camtype]._ShowImageLoop += SSZNCamTools_ShowImageLoopLeft;
                        Connect(camtype, SettingParameter.Instance().StrLeftLaser3DIP);
                        break;
                    }
                case emSSZNCamType.EM_DOWN:
                    {
                        _cam3DArray[(int)camtype]._ShowImage += SSZNCamTools_ShowImage_Down;
                        _cam3DArray[(int)camtype]._ShowImageLoop += SSZNCamTools_ShowImageLoopDown;
                        Connect(camtype, SettingParameter.Instance().StrDownLaser3DIP);
                        break;
                    }
                case emSSZNCamType.EM_RIGHT:
                    {
                        _cam3DArray[(int)camtype]._ShowImage += SSZNCamTools_ShowImage_Right;
                        _cam3DArray[(int)camtype]._ShowImageLoop += SSZNCamTools_ShowImageLoopRight;
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
                if (File.Exists("E:/Image/top/" + NMockTopIndex.ToString() + ".tif"))
                {
                    HOperatorSet.ReadImage(out hObject, "E:/Image/top/" + NMockTopIndex.ToString() + ".tif");
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
                if (File.Exists("E:/Image/right/" + NMockRightIndex.ToString() + ".tif"))
                {
                    HOperatorSet.ReadImage(out hObject, "E:/Image/right/" + NMockRightIndex.ToString() + ".tif");
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
                if (File.Exists("E:/Image/down/" + NMockDownIndex.ToString() + ".tif"))
                {
                    HOperatorSet.ReadImage(out hObject, "E:/Image/down/" + NMockDownIndex.ToString() + ".tif");
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
                if (File.Exists("E:/Image/left/" + NMockLeftIndex.ToString() + ".tif"))
                {
                    HOperatorSet.ReadImage(out hObject, "E:/Image/left/" + NMockLeftIndex.ToString() + ".tif");
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
                    SetBatchBlcok(emSSZNCamType.EM_TOP);
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
                    SetBatchBlcok(emSSZNCamType.EM_LEFT);
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
                    SetBatchBlcok(emSSZNCamType.EM_RIGHT);
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
                AddObjects(emSSZNCamType.EM_RIGHT, hObject);
            }
            else
            {
                if (Bstop == false)
                {
                    //SoftTrigOne(emSSZNCamType.EM_DOWN);
                    SetBatchBlcok(emSSZNCamType.EM_DOWN);
                    hObject = GenImage1FromIntArray(_cam3DArray[(int)emSSZNCamType.EM_DOWN].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_DOWN].Width, _cam3DArray[(int)emSSZNCamType.EM_DOWN].Height);

                    AddObjects(emSSZNCamType.EM_RIGHT, hObject);
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
            while (true)
            {
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
                for (int i = 0; i < 4; i++)
                {
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
                                            XGCamTools.Instance.StopCaptures();
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


                                            HOperatorSet.CropPart(_ho_ImageLeft, out hObjectLeftReference, 0, 0, 3200, 4000);
                                            HOperatorSet.CropPart(_ho_ImageTop, out hObjectTopReference, 0, 0, 3200, 4000);
                                            HOperatorSet.CropPart(_ho_ImageRight, out hObjectRightReference, 0, 0, 3200, 4000);
                                            HOperatorSet.CropPart(_ho_ImageDown, out hObjectDownReference, 0, 0, 3200, 4000);


                                            LogHelper.Info("Silicon", "ThreadDealSqureSiliconCollection Crop Bmps");

                                            HTuple hv_fRatioAngleT = new HTuple();
                                            HTuple hv_fRatioAngleD = new HTuple();
                                            HTuple hv_fRatioAngleL = new HTuple();
                                            HTuple hv_fRatioAngleR = new HTuple();

                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectLeftReference, out HTuple hv_topAngleOfTwoLinesRefL, out HTuple hv_midAngleOfTwoLinesRefL, out HTuple hv_downAngleOfTwoLinesRefL);

                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectRightReference, out HTuple hv_topAngleOfTwoLinesRefR, out HTuple hv_midAngleOfTwoLinesRefR, out HTuple hv_downAngleOfTwoLinesRefR);

                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectTopReference, out HTuple hv_topAngleOfTwoLinesRefT, out HTuple hv_midAngleOfTwoLinesRefT, out HTuple hv_downAngleOfTwoLinesRefT);

                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectDownReference, out HTuple hv_topAngleOfTwoLinesRefD, out HTuple hv_midAngleOfTwoLinesRefD, out HTuple hv_downAngleOfTwoLinesRefD);

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



                                            CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectTopReference, hv_fRatioT, out hv_RefDiagTopLength, out hv_RefLeftIndexesTop, out hv_RefRightIndexesTop, out hv_RefRowBeginTop, out hv_RefmeanHeightTopL, out hv_RefmeanHeightTopM, out hv_RefmeanHeightTopR, 5);

                                            CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectLeftReference, hv_fRatioL, out hv_RefDiagLeftLength, out hv_RefLeftIndexesLeft, out hv_RefRightIndexesLeft, out hv_RefRowBeginLeft, out hv_RefmeanHeightLeftL, out hv_RefmeanHeightLeftM, out hv_RefmeanHeightLeftR, 5);


                                            CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectRightReference, hv_fRatioR, out hv_RefDiagRightLength, out hv_RefLeftIndexesRight, out hv_RefRightIndexesRight, out hv_RefRowBeginRight, out hv_RefmeanHeightRightL, out hv_RefmeanHeightRightM, out hv_RefmeanHeightRightR, 5);

                                            CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectDownReference, hv_fRatioD, out hv_RefDiagDownLength, out hv_RefLeftIndexesDown, out hv_RefRightIndexesDown, out hv_RefRowBeginDown, out hv_RefmeanHeightDownL, out hv_RefmeanHeightDownM, out hv_RefmeanHeightDownR, 5);


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
                                            HOperatorSet.TupleMean(hv_RefLeftIndexesLeft, out hv_meanRefLeftDownIndex);
                                            hv_meanRefRightDownIndex.Dispose();
                                            HOperatorSet.TupleMean(hv_RefRightIndexesRight, out hv_meanRefRightDownIndex);
                                            hv_meanRefRightLeftIndex.Dispose();
                                            HOperatorSet.TupleMean(hv_RefLeftIndexesRight, out hv_meanRefRightLeftIndex);


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
                                            dataref.Hv_meanRefRightTopIndex = hv_meanRefRightTopIndex;
                                            dataref.Hv_meanRefLeftTopIndex = hv_meanRefLeftTopIndex;
                                            dataref.Hv_meanRefLeftDownIndex = hv_meanRefLeftDownIndex;
                                            dataref.Hv_meanRefRightDownIndex = hv_meanRefRightDownIndex;
                                            dataref.Hv_meanRefRightLeftIndex = hv_meanRefRightLeftIndex;
                                            dataref.Hv_fPreRTHeight = hv_fPreRTHeight;
                                            dataref.Hv_fPreRTWidth = hv_fPreRTWidth;
                                            dataref.Hv_fPreLTHeight = hv_fPreLTHeight;
                                            dataref.Hv_fPreLTWidth = hv_fPreLTWidth;

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

        public void ThreadDealSquareInfocollectionkeyong(object objtype)
        {

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
            while (true)
            {
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
                for (int i = 0; i < 4; i++)
                {
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
                                            XGCamTools.Instance.StopCaptures();
                                        }
                                    }


                                    if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
                                    {
                                        threadMov = new Thread(() =>
                                        {
                                            LogHelper.Info("Silicon", "ThreadDealSquareInfo Move To Orgin Position");
                                            CMoveController.Instance().SetMoveSpeed(SettingParameter.Instance().FPLCMoveSpeed);
                                            CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, SettingParameter.Instance().FPosition_fir_s,20);
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


                                            HOperatorSet.CropPart(_ho_ImageLeft, out hObjectLeftReference, 0, 0, 3200, 4000);
                                            HOperatorSet.CropPart(_ho_ImageTop, out hObjectTopReference, 0, 0, 3200, 4000);
                                            HOperatorSet.CropPart(_ho_ImageRight, out hObjectRightReference, 0, 0, 3200, 4000);
                                            HOperatorSet.CropPart(_ho_ImageDown, out hObjectDownReference, 0, 0, 3200, 4000);


                                        LogHelper.Info("Silicon", "ThreadDealSqureSiliconCollection Crop Bmps");
                                        
                                            HTuple hv_fRatioAngleT = new HTuple();
                                            HTuple hv_fRatioAngleD = new HTuple();
                                            HTuple hv_fRatioAngleL = new HTuple();
                                            HTuple hv_fRatioAngleR = new HTuple();

                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectLeftReference, out HTuple hv_topAngleOfTwoLinesRefL, out HTuple hv_midAngleOfTwoLinesRefL, out HTuple hv_downAngleOfTwoLinesRefL);

                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectRightReference, out HTuple hv_topAngleOfTwoLinesRefR, out HTuple hv_midAngleOfTwoLinesRefR, out HTuple hv_downAngleOfTwoLinesRefR);

                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectTopReference, out HTuple hv_topAngleOfTwoLinesRefT, out HTuple hv_midAngleOfTwoLinesRefT, out HTuple hv_downAngleOfTwoLinesRefT);

                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectDownReference, out HTuple hv_topAngleOfTwoLinesRefD, out HTuple hv_midAngleOfTwoLinesRefD, out HTuple hv_downAngleOfTwoLinesRefD);


                                            HOperatorSet.TupleMax((((hv_topAngleOfTwoLinesRefT.TupleConcat(hv_topAngleOfTwoLinesRefT.D)).TupleConcat(hv_midAngleOfTwoLinesRefT.D)).TupleConcat(hv_downAngleOfTwoLinesRefT.D)), out HTuple hv_maxTopAngle);

                                            HOperatorSet.TupleMax(hv_topAngleOfTwoLinesRefL.TupleConcat(hv_topAngleOfTwoLinesRefL.D).TupleConcat(hv_midAngleOfTwoLinesRefL.D).TupleConcat(hv_downAngleOfTwoLinesRefL.D), out HTuple hv_maxLeftAngle);

                                            HOperatorSet.TupleMax(hv_topAngleOfTwoLinesRefR.TupleConcat(hv_topAngleOfTwoLinesRefR.D).TupleConcat(hv_midAngleOfTwoLinesRefR.D).TupleConcat(hv_downAngleOfTwoLinesRefR.D), out HTuple hv_maxRightAngle);

                                            HOperatorSet.TupleMax(hv_topAngleOfTwoLinesRefD.TupleConcat(hv_midAngleOfTwoLinesRefD.D).TupleConcat(hv_downAngleOfTwoLinesRefD.D), out HTuple hv_maxDownAngle);



                                            if (hv_maxRightAngle.TupleGreater(0.015) != 0)
                                            {
                                                hv_fRatioAngleR = 0.005 / hv_maxRightAngle;
                                            }
                                            else
                                            {
                                                hv_fRatioAngleR = 0.8;
                                            }

                                            if (hv_maxLeftAngle.TupleGreater(0.015) != 0)
                                            {

                                                hv_fRatioAngleL = 0.005 / hv_maxLeftAngle;
                                            }
                                            else
                                            {
                                                hv_fRatioAngleL = 0.8;
                                            }


                                            if (hv_maxDownAngle.TupleGreater(0.015) != 0)
                                            {

                                                hv_fRatioAngleD = 0.005 / hv_maxDownAngle;
                                            }
                                            else
                                            {
                                                hv_fRatioAngleD = 0.8;
                                            }


                                            if (hv_maxTopAngle.TupleGreater(0.015) != 0)
                                            {
                                                hv_fRatioAngleT = 0.005 / hv_maxTopAngle;
                                            }
                                            else
                                            {
                                                hv_fRatioAngleT = 0.8;
                                            }


                                            CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectTopReference, hv_fRatioT, out hv_RefDiagTopLength, out hv_RefLeftIndexesTop, out hv_RefRightIndexesTop, out hv_RefRowBeginTop, out hv_RefmeanHeightTopL, out hv_RefmeanHeightTopM, out hv_RefmeanHeightTopR, 10);

                                            CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectLeftReference, hv_fRatioL, out hv_RefDiagLeftLength, out hv_RefLeftIndexesLeft, out hv_RefRightIndexesLeft, out hv_RefRowBeginLeft, out hv_RefmeanHeightLeftL, out hv_RefmeanHeightLeftM, out hv_RefmeanHeightLeftR, 10);


                                            CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectRightReference, hv_fRatioR, out hv_RefDiagRightLength, out hv_RefLeftIndexesRight, out hv_RefRightIndexesRight, out hv_RefRowBeginRight, out hv_RefmeanHeightRightL, out hv_RefmeanHeightRightM, out hv_RefmeanHeightRightR, 10);

                                            CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectDownReference, hv_fRatioD, out hv_RefDiagDownLength, out hv_RefLeftIndexesDown, out hv_RefRightIndexesDown, out hv_RefRowBeginDown, out hv_RefmeanHeightDownL, out hv_RefmeanHeightDownM, out hv_RefmeanHeightDownR, 10);


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
                                            CGlobalSquareFuncToolsNew.Instance().checkReferenceByRelativelyCollection(hObjectTopReference, hObjectLeftReference, hObjectRightReference, hObjectDownReference, hv_RefRowBeginTop, hv_RefRowBeginRight, hv_RefRowBeginRight, hv_RefRowBeginDown, hv_RefLeftIndexesTop, hv_RefRightIndexesTop, hv_RefLeftIndexesLeft, hv_RefRightIndexesRight, hv_RefmeanHeightTopR, hv_RefmeanHeightLeftR, hv_RefmeanHeightRightR, parInfo.Hv_fStandLTLength, parInfo.Hv_fStandRTLength, parInfo.Hv_fStandTDLength, parInfo.Hv_fStandLRLength, out hv_fPreRTHeight, out hv_fPreRTWidth, out hv_fPreLTHeight, out hv_fPreLTWidth);

                                            LogHelper.Info("", " " + parInfo.nSquareType.ToString() + " hv_fPreRTHeight " + hv_fPreRTHeight.D.ToString("0.00") + "  hv_fPreRTWidth " + hv_fPreRTWidth.D.ToString("0.00") + " hv_fPreLTHeight " + hv_fPreLTHeight.D.ToString("0.00") + " hv_fPreLTWidth " + hv_fPreLTWidth.D.ToString("0.00"));

                                            hv_meanRefRightTopIndex.Dispose();
                                            HOperatorSet.TupleMean(hv_RefRightIndexesTop, out hv_meanRefRightTopIndex);
                                            hv_meanRefLeftTopIndex.Dispose();
                                            HOperatorSet.TupleMean(hv_RefLeftIndexesTop, out hv_meanRefLeftTopIndex);
                                            hv_meanRefLeftDownIndex.Dispose();
                                            HOperatorSet.TupleMean(hv_RefLeftIndexesLeft, out hv_meanRefLeftDownIndex);
                                            hv_meanRefRightDownIndex.Dispose();
                                            HOperatorSet.TupleMean(hv_RefRightIndexesRight, out hv_meanRefRightDownIndex);
                                            hv_meanRefRightLeftIndex.Dispose();
                                            HOperatorSet.TupleMean(hv_RefLeftIndexesRight, out hv_meanRefRightLeftIndex);


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
                                            dataref.Hv_meanRefRightTopIndex = hv_meanRefRightTopIndex;
                                            dataref.Hv_meanRefLeftTopIndex = hv_meanRefLeftTopIndex;
                                            dataref.Hv_meanRefLeftDownIndex = hv_meanRefLeftDownIndex;
                                            dataref.Hv_meanRefRightDownIndex = hv_meanRefRightDownIndex;
                                            dataref.Hv_meanRefRightLeftIndex = hv_meanRefRightLeftIndex;
                                            dataref.Hv_fPreRTHeight = hv_fPreRTHeight;
                                            dataref.Hv_fPreRTWidth = hv_fPreRTWidth;
                                            dataref.Hv_fPreLTHeight = hv_fPreLTHeight;
                                            dataref.Hv_fPreLTWidth = hv_fPreLTWidth;

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
                                catch(Exception ex)
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

        public void ThreadDealSquareInfoAdaptationOld(object objtype)
        {
            HObject hObject = new HObject();

            tagParaInfo paraInfo = (tagParaInfo)objtype;
            int nWaitIndex = 0;
            //int nGrayWaitIndex = 0;
            //bool bNeedBegin = false;

            //do
            //{
            //    CMoveController.Instance().GetState(126, out bNeedBegin);
            //    Thread.Sleep(1000);
            //    LogHelper.Info("Silicon", "Wait For 126 Signal!");
            //} while (bNeedBegin == false);


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
            HTuple lengthTmp = new HTuple();
            HTuple hv_LeftIndexesTopF = new HTuple(), hv_LeftIndexesTopS = new HTuple();
            HTuple hv_LeftIndexesTopT = new HTuple(), hv_RightIndexesTopF = new HTuple();
            HTuple hv_RightIndexesTopS = new HTuple(), hv_RightIndexesTopT = new HTuple();
            HTuple hv_LeftIndexesLeftF = new HTuple(), hv_LeftIndexesLeftS = new HTuple();
            HTuple hv_LeftIndexesLeftT = new HTuple();
            HTuple hv_RightIndexesLeftF = new HTuple();
            HTuple hv_RightIndexesLeftS = new HTuple(), hv_RightIndexesLeftT = new HTuple();
            HTuple hv_LeftIndexesRightF = new HTuple();
            HTuple hv_LeftIndexesRightS = new HTuple(), hv_LeftIndexesRightT = new HTuple();
            HTuple hv_RightIndexesRightF = new HTuple();
            HTuple hv_RightIndexesRightS = new HTuple(), hv_RightIndexesRightT = new HTuple();
            HTuple hv_LeftIndexesDownF = new HTuple();
            HTuple hv_LeftIndexesDownS = new HTuple(), hv_LeftIndexesDownT = new HTuple();
            HTuple hv_RightIndexesDownF = new HTuple();
            HTuple hv_RightIndexesDownS = new HTuple(), hv_RightIndexesDownT = new HTuple();
            HTuple hv_meanHeightTopMF = new HTuple(), hv_meanHeightTopMS = new HTuple();
            HTuple hv_meanHeightTopMT = new HTuple();
            HTuple hv_meanHeightTopRF = new HTuple(), hv_meanHeightTopRS = new HTuple();
            HTuple hv_meanHeightTopRT = new HTuple();
            HTuple hv_meanHeightTopLF = new HTuple(), hv_meanHeightTopLS = new HTuple();
            HTuple hv_meanHeightTopLT = new HTuple();
            HTuple hv_meanHeightLeftLF = new HTuple(), hv_meanHeightLeftLS = new HTuple();
            HTuple hv_meanHeightLeftLT = new HTuple();
            HTuple hv_meanHeightLeftMF = new HTuple(), hv_meanHeightLeftMS = new HTuple();
            HTuple hv_meanHeightLeftMT = new HTuple();
            HTuple hv_meanHeightRightMF = new HTuple(), hv_meanHeightRightMS = new HTuple();
            HTuple hv_meanHeightRightMT = new HTuple();
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
            float fTD3DDistance = 0;
            float fLR3DDistance = 0;
            float fRealTDDisWithoutRef = 0;
            float fCurRealTDLength = 0;
            float fRealLRDisWithoutRef = 0;
            float fCurRealLRLength = 0;
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
            while (true)
            {
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
                for (int i = 0; i < 4; i++)
                {
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

                                        //if (true == GetGrayObjectsByType((emSSZNCamType)i, ref hGrayObject))
                                        //{
                                        //    HOperatorSet.WriteImage(hGrayObject, "tiff", 0, "D:/Image1/left/gray_" + NLeftIndex.ToString() + ".tif");

                                        //}
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
                                        //if (true == GetGrayObjectsByType((emSSZNCamType)i, ref hGrayObject))
                                        //{
                                        //    HOperatorSet.WriteImage(hGrayObject, "tiff", 0, "D:/Image1/right/gray_" + NRightIndex.ToString() + ".tif");
                                        //}
                                    }
                                    //HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image/right/" + NRightIndex++.ToString() + ".tif");
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
                                        //if (true == GetGrayObjectsByType((emSSZNCamType)i, ref hGrayObject))
                                        //{
                                        //    HOperatorSet.WriteImage(hGrayObject, "tiff", 0, "D:/Image1/top/gray_" + NTopIndex.ToString() + ".tif");
                                        //}
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
                                        //if (true == GetGrayObjectsByType((emSSZNCamType)i, ref hGrayObject))
                                        //{
                                        //    HOperatorSet.WriteImage(hGrayObject, "tiff", 0, "D:/Image1/down/gray_" + NDownIndex.ToString() + ".tif");

                                        //}
                                    }
                                    //HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image/down/" + NDownIndex++.ToString() + ".tif");
                                    NDownIndex++;
                                    HOperatorSet.ConcatObj(_hObjectsDown, hObject, out _hObjectsDown);
                                    break;
                                }
                        }

                        LogHelper.Info("Silicon", "ThreadDealSquareInfo NLeftIndex " + NLeftIndex.ToString() + " NRightIndex " + NRightIndex.ToString() + " NTopIndex " + NTopIndex.ToString() + " NDownIndex " + NDownIndex.ToString());

                        if (false == bCheckReference && NLeftIndex >= 4 && NRightIndex >= 4 && NTopIndex >= 4 && NDownIndex >= 4)
                        {

                            bCheckReference = true;
                            //HObject hObjectLeftReference = new HObject();
                            //HObject hObjectRightReference = new HObject();
                            //HObject hObjectTopReference = new HObject();
                            //HObject hObjectDownReference = new HObject();

                            //HOperatorSet.GenEmptyObj(out hObjectLeftReference);
                            //HOperatorSet.GenEmptyObj(out hObjectRightReference);
                            //HOperatorSet.GenEmptyObj(out hObjectTopReference);
                            //HOperatorSet.GenEmptyObj(out hObjectDownReference);

                            //HOperatorSet.TileImages(_hObjectsLeft, out _ho_ImageLeft, 1, "vertical");
                            //HOperatorSet.TileImages(_hObjectsRight, out _ho_ImageRight, 1, "vertical");
                            //HOperatorSet.TileImages(_hObjectsTop, out _ho_ImageTop, 1, "vertical");
                            //HOperatorSet.TileImages(_hObjectsDown, out _ho_ImageDown, 1, "vertical");


                            //HOperatorSet.CropPart(_ho_ImageLeft, out hObjectLeftReference, 0, 0, 3200, 4000);
                            //HOperatorSet.CropPart(_ho_ImageTop, out hObjectTopReference, 0, 0, 3200, 4000);
                            //HOperatorSet.CropPart(_ho_ImageRight, out hObjectRightReference, 0, 0, 3200, 4000);
                            //HOperatorSet.CropPart(_ho_ImageDown, out hObjectDownReference, 0, 0, 3200, 4000);

                            threaddeal = new Thread(() =>
                            {
                                try
                                {
                                    /*HTuple hv_fRatioAngleT = new HTuple();
                                    HTuple hv_fRatioAngleD = new HTuple();
                                    HTuple hv_fRatioAngleL = new HTuple();
                                    HTuple hv_fRatioAngleR = new HTuple();

                                    CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectLeftReference, out HTuple hv_topAngleOfTwoLinesRefL, out HTuple hv_midAngleOfTwoLinesRefL, out HTuple hv_downAngleOfTwoLinesRefL); 

                                    CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectRightReference, out HTuple hv_topAngleOfTwoLinesRefR, out HTuple hv_midAngleOfTwoLinesRefR, out HTuple hv_downAngleOfTwoLinesRefR);

                                    CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectTopReference, out HTuple hv_topAngleOfTwoLinesRefT, out HTuple hv_midAngleOfTwoLinesRefT, out HTuple hv_downAngleOfTwoLinesRefT);

                                    CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectDownReference, out HTuple hv_topAngleOfTwoLinesRefD, out HTuple hv_midAngleOfTwoLinesRefD, out HTuple hv_downAngleOfTwoLinesRefD);


                                    HOperatorSet.TupleMax((((hv_topAngleOfTwoLinesRefT.TupleConcat(hv_topAngleOfTwoLinesRefT.D)).TupleConcat(hv_midAngleOfTwoLinesRefT.D)).TupleConcat(hv_downAngleOfTwoLinesRefT.D)), out HTuple hv_maxTopAngle);

                                    HOperatorSet.TupleMax(hv_topAngleOfTwoLinesRefL.TupleConcat(hv_topAngleOfTwoLinesRefL.D).TupleConcat(hv_midAngleOfTwoLinesRefL.D).TupleConcat(hv_downAngleOfTwoLinesRefL.D), out HTuple hv_maxLeftAngle);

                                    HOperatorSet.TupleMax(hv_topAngleOfTwoLinesRefR.TupleConcat(hv_topAngleOfTwoLinesRefR.D).TupleConcat(hv_midAngleOfTwoLinesRefR.D).TupleConcat(hv_downAngleOfTwoLinesRefR.D), out HTuple hv_maxRightAngle);

                                    HOperatorSet.TupleMax(hv_topAngleOfTwoLinesRefD.TupleConcat(hv_midAngleOfTwoLinesRefD.D).TupleConcat(hv_downAngleOfTwoLinesRefD.D), out HTuple hv_maxDownAngle);


                                    
                                    if (hv_maxRightAngle.TupleGreater(0.015) != 0)
                                    {
                                        hv_fRatioAngleR = 0.005 / hv_maxRightAngle;
                                    }
                                    else
                                    {
                                        hv_fRatioAngleR = 0.8;
                                    }

                                    if (hv_maxLeftAngle.TupleGreater(0.015) != 0)
                                    {
                                        
                                        hv_fRatioAngleL = 0.005 / hv_maxLeftAngle;
                                    }
                                    else
                                    {
                                        hv_fRatioAngleL = 0.8;
                                    }


                                    if (hv_maxDownAngle.TupleGreater(0.015) != 0)
                                    {
                                        
                                        hv_fRatioAngleD = 0.005 / hv_maxDownAngle;
                                    }
                                    else
                                    {
                                        hv_fRatioAngleD = 0.8;
                                    }


                                    if (hv_maxTopAngle.TupleGreater(0.015) != 0)
                                    {
                                        hv_fRatioAngleT = 0.005 / hv_maxTopAngle;
                                    }
                                    else
                                    {
                                        hv_fRatioAngleT = 0.8;
                                    }


                                    CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectTopReference, hv_fRatioT, out hv_RefDiagTopLength, out hv_RefLeftIndexesTop, out hv_RefRightIndexesTop, out hv_RefRowBeginTop, out hv_RefmeanHeightTopL, out hv_RefmeanHeightTopM, out hv_RefmeanHeightTopR);

                                    CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectLeftReference, hv_fRatioL, out hv_RefDiagLeftLength, out hv_RefLeftIndexesLeft, out hv_RefRightIndexesLeft, out hv_RefRowBeginLeft, out hv_RefmeanHeightLeftL, out hv_RefmeanHeightLeftM, out hv_RefmeanHeightLeftR);


                                    CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectRightReference, hv_fRatioR, out hv_RefDiagRightLength, out hv_RefLeftIndexesRight, out hv_RefRightIndexesRight, out hv_RefRowBeginRight, out hv_RefmeanHeightRightL, out hv_RefmeanHeightRightM, out hv_RefmeanHeightRightR);

                                    CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectDownReference, hv_fRatioD, out hv_RefDiagDownLength, out hv_RefLeftIndexesDown, out hv_RefRightIndexesDown, out hv_RefRowBeginDown, out hv_RefmeanHeightDownL, out hv_RefmeanHeightDownM, out hv_RefmeanHeightDownR);


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

                                    if (hv_RefDiagTopLength > 9)
                                    {
                                        CGlobalSquareFuncToolsNew.Instance().CheckReferenceObjectWithoutRuler(hObjectTopReference, hObjectLeftReference, hObjectRightReference, hObjectDownReference, hv_RefRowBeginTop, hv_RefRowBeginLeft, hv_RefRowBeginRight, hv_RefRowBeginDown, hv_RefLeftIndexesTop, hv_RefRightIndexesTop, hv_RefLeftIndexesDown, hv_RefRightIndexesDown, hv_RefLeftIndexesLeft, hv_RefRightIndexesLeft, hv_RefLeftIndexesRight, hv_RefRightIndexesRight, hv_RefDiagTopLength, hv_RefDiagLeftLength, hv_RefDiagRightLength, hv_RefDiagDownLength, out hv_HomMat2DTDAdapted, out hv_HomMat2DDTAdapted, out hv_HomMat2DLRAdapted, out hv_HomMat2DRLAdapted, out hv_meanMidr, out hv_meanMidl, out hv_rowRT3D, out hv_colRT3D, out hv_rowLT3D, out hv_colLT3D, out hv_heightDT3D, out hv_widthRL3D, out hv_fPreRTHeight, out hv_fPreRTWidth, out hv_fPreLTHeight, out hv_fPreLTWidth);
                                        //CGlobalSquareFuncTools.Instance().CheckReferenceObject(hObjectTopReference, hObjectLeftReference, hObjectRightReference, hObjectDownReference, out hv_HomMat2DTDAdapted, out  hv_HomMat2DTLAdapted, out  hv_HomMat2DTRAdapted, out  hv_HomMat2DDTAdapted, out  hv_HomMat2DLAdapted, out  hv_HomMat2DRAdapted, out  hv_HomMat2DLDAdapted, out  hv_HomMat2DLRAdapted, out  hv_HomMat2DLTAdapted, out  hv_HomMat2DRDAdapted, out  hv_HomMat2DRLAdapted, out  hv_HomMat2DRTAdapted, out  hv_fLTRatio, out  hv_fLDRatio, out  hv_fRTRatio, out  hv_fRDRatio, out  hv_fLRRatio, out  hv_fTDRatio, out  hv_fTopDiagRatio, out  hv_fDownDiagRatio, out  hv_fLeftDiagRatio, out  hv_fRightDiagRatio, out  hv_meanMidt, out  hv_meanMidr, out  hv_meanMidd, out  hv_meanMidl, out  hv_rowRT3D, out  hv_colRT3D, out  hv_rowRD3D, out  hv_colRD3D, out  hv_rowLT3D, out  hv_colLT3D, out  hv_rowLD3D, out  hv_colLD3D, out  hv_heightDT3D, out  hv_widthRL3D);
                                    }
                                    else
                                    {
                                        CGlobalSquareFuncToolsNew.Instance().checkReferenceNewByRelatively(hObjectTopReference, hObjectLeftReference, hObjectRightReference, hObjectDownReference, hv_RefRowBeginTop, hv_RefRowBeginLeft, hv_RefRowBeginRight, hv_RefRowBeginDown, hv_RefLeftIndexesTop, hv_RefRightIndexesTop, hv_RefLeftIndexesDown, hv_RefRightIndexesDown, hv_RefLeftIndexesLeft, hv_RefRightIndexesLeft, hv_RefLeftIndexesRight, hv_RefRightIndexesRight, hv_RefmeanHeightTopR, hv_RefmeanHeightDownR, hv_RefmeanHeightLeftR, hv_RefmeanHeightRightR, out hv_fPreRTHeight, out hv_fPreRTWidth, out hv_fPreLTHeight, out hv_fPreLTWidth);
                                    }
                                    hv_meanRefRightTopIndex.Dispose();
                                    HOperatorSet.TupleMean(hv_RefRightIndexesTop, out hv_meanRefRightTopIndex);
                                    hv_meanRefLeftTopIndex.Dispose();
                                    HOperatorSet.TupleMean(hv_RefLeftIndexesTop, out hv_meanRefLeftTopIndex);
                                    hv_meanRefLeftDownIndex.Dispose();
                                    HOperatorSet.TupleMean(hv_RefLeftIndexesLeft, out hv_meanRefLeftDownIndex);
                                    hv_meanRefRightDownIndex.Dispose();
                                    HOperatorSet.TupleMean(hv_RefRightIndexesRight, out hv_meanRefRightDownIndex);
                                    hv_meanRefRightLeftIndex.Dispose();
                                    HOperatorSet.TupleMean(hv_RefLeftIndexesRight, out hv_meanRefRightLeftIndex);

                                    
                                    
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

                                    dataref.Hv_meanRefRightTopIndex = hv_meanRefRightTopIndex;
                                    dataref.Hv_meanRefLeftTopIndex = hv_meanRefLeftTopIndex;
                                    dataref.Hv_meanRefLeftDownIndex = hv_meanRefLeftDownIndex;
                                    dataref.Hv_meanRefRightDownIndex = hv_meanRefRightDownIndex;
                                    dataref.Hv_meanRefRightLeftIndex = hv_meanRefRightLeftIndex;
                                    dataref.Hv_fPreRTHeight = hv_fPreRTHeight;
                                    dataref.Hv_fPreRTWidth = hv_fPreRTWidth;
                                    dataref.Hv_fPreLTHeight = hv_fPreLTHeight;
                                    dataref.Hv_fPreLTWidth = hv_fPreLTWidth;
                                    
                                    if (hv_RefDiagTopLength > 9)
                                    {
                                        dataref = _dictRefTypeRefObject[ReferencePosInfoData.EMT_TYPE.EMT_TYPE_182_ONE];
                                    }
                                    else
                                    {
                                        dataref = _dictRefTypeRefObject[ReferencePosInfoData.EMT_TYPE.EMT_TYPE_182_THREE];
                                    }*/
                                }
                                catch (Exception ex)
                                {

                                }


                            });

                            //threaddeal.Start();
                        }

                        nWaitIndex = 0;
                    }
                    else
                    {
                        if (bValidWait == true)
                        {
                            nWaitIndex++;
                            if (nWaitIndex >= 8)
                            {
                                LogHelper.Info("Silicon", "camtype " + paraInfo.camtype.ToString() + " No Message , break");

                                if (NLeftIndex >= 4 && NRightIndex >= 4 && NTopIndex >= 4 && NDownIndex >= 4)
                                {
                                    try
                                    {
                                        if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
                                        {
                                            if (SettingParameter.Instance().NCamType == 0)
                                            {
                                                SSZNCamTools.Instance().StopBatchLoop();
                                            }
                                            else
                                            {
                                                XGCamTools.Instance.StopCaptures();
                                            }
                                        }

                                        threadMov = new Thread(() =>
                                        {
                                            LogHelper.Info("Silicon", "ThreadDealSquareInfo Move To Orgin Position");
                                            CMoveController.Instance().SetMoveSpeed(SettingParameter.Instance().FPLCMoveSpeed);
                                            CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, SettingParameter.Instance().FPosition_fir_s, 20);
                                            NTestStatus = emStatus.EM_FREE;
                                            //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                                        });
                                        if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
                                        {
                                            NTestStatus = emStatus.EM_GOTOORIGIN;
                                            threadMov.Start();
                                        }

                                        bCalculateStick = true;

                                        HObject hobjectLeftStick = new HObject();
                                        HObject hobjectRightStick = new HObject();
                                        HObject hobjectTopStick = new HObject();
                                        HObject hobjectDownStick = new HObject();

                                        HOperatorSet.GenEmptyObj(out hobjectLeftStick);
                                        HOperatorSet.GenEmptyObj(out hobjectDownStick);
                                        HOperatorSet.GenEmptyObj(out hobjectRightStick);
                                        HOperatorSet.GenEmptyObj(out hobjectTopStick);

                                        HOperatorSet.TileImages(_hObjectsLeft, out _ho_ImageLeft, 1, "vertical");
                                        HOperatorSet.TileImages(_hObjectsRight, out _ho_ImageRight, 1, "vertical");
                                        HOperatorSet.TileImages(_hObjectsTop, out _ho_ImageTop, 1, "vertical");
                                        HOperatorSet.TileImages(_hObjectsDown, out _ho_ImageDown, 1, "vertical");

                                        HOperatorSet.GetImageSize(_ho_ImageLeft, out HTuple widthleft, out HTuple heightleft);
                                        HOperatorSet.GetImageSize(_ho_ImageTop, out HTuple widthtop, out HTuple heighttop);
                                        HOperatorSet.GetImageSize(_ho_ImageRight, out HTuple widthright, out HTuple heightright);
                                        HOperatorSet.GetImageSize(_ho_ImageDown, out HTuple widthdown, out HTuple heightdown);


                                        HOperatorSet.CropPart(_ho_ImageLeft, out hobjectLeftStick, 4000, 0, 3200, heightleft - 4000);
                                        HOperatorSet.CropPart(_ho_ImageTop, out hobjectTopStick, 4000, 0, 3200, heighttop - 4000);
                                        HOperatorSet.CropPart(_ho_ImageDown, out hobjectDownStick, 4000, 0, 3200, heightdown - 4000);
                                        HOperatorSet.CropPart(_ho_ImageRight, out hobjectRightStick, 4000, 0, 3200, heightright - 4000);


                                        HTuple hv_ResultdictHandle = new HTuple();
                                        HOperatorSet.CreateDict(out hv_ResultdictHandle);


                                        HTuple hv_fRatioMean = 0.0149558;

                                        //CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hobjectTopStick, hv_fRatioMean, out hv_DiagTopLength, out hv_LeftIndexesTop, out hv_RightIndexesTop, out hv_RowBeginTop, out hv_meanHeightTopL, out hv_meanHeightTopM, out hv_meanHeightTopR);
                                        //CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hobjectRightStick, hv_fRatioMean, out hv_DiagRightLength, out hv_LeftIndexesRight, out hv_RightIndexesRight, out hv_RowBeginRight, out hv_meanHeightRightL, out hv_meanHeightRightM, out hv_meanHeightRightR);
                                        //CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hobjectLeftStick, hv_fRatioMean, out hv_DiagLeftLength, out hv_LeftIndexesLeft, out hv_RightIndexesLeft, out hv_RowBeginLeft, out hv_meanHeightLeftL, out hv_meanHeightLeftM, out hv_meanHeightLeftR);
                                        //CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hobjectDownStick, hv_fRatioMean, out hv_DiagDownLength, out hv_LeftIndexesDown, out hv_RightIndexesDown, out hv_RowBeginDown, out hv_meanHeightDownL, out hv_meanHeightDownM, out hv_meanHeightDownR);

                                        CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeights(hobjectTopStick, hv_fRatioMean, out hv_DiagTopLength, out hv_LeftIndexesTop, out hv_RightIndexesTop, out hv_RowBeginTop, out hv_meanHeightTopL, out hv_meanHeightTopM, out hv_meanHeightTopR);

                                        CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeights(hobjectRightStick, hv_fRatioMean, out hv_DiagRightLength, out hv_LeftIndexesRight, out hv_RightIndexesRight, out hv_RowBeginRight, out hv_meanHeightRightL, out hv_meanHeightRightM, out hv_meanHeightRightR);

                                        CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeights(hobjectLeftStick, hv_fRatioMean, out hv_DiagLeftLength, out hv_LeftIndexesLeft, out hv_RightIndexesLeft, out hv_RowBeginLeft, out hv_meanHeightLeftL, out hv_meanHeightLeftM, out hv_meanHeightLeftR);

                                        CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeights(hobjectDownStick, hv_fRatioMean, out hv_DiagDownLength, out hv_LeftIndexesDown, out hv_RightIndexesDown, out hv_RowBeginDown, out hv_meanHeightDownL, out hv_meanHeightDownM, out hv_meanHeightDownR);


                                        Thread thDealTopAngle = new Thread(() =>
                                        {
                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hobjectTopStick, out hv_topAngleOfTwoLineT, out hv_midAngleOfTwoLineT, out hv_downAngleOfTwoLineT);
                                        });
                                        thDealTopAngle.Start();

                                        Thread thDealLeftAngle = new Thread(() =>
                                        {
                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hobjectLeftStick, out hv_topAngleOfTwoLineL, out hv_midAngleOfTwoLineL, out hv_downAngleOfTwoLineL);
                                        });
                                        thDealLeftAngle.Start();

                                        Thread thDealRightAngle = new Thread(() =>
                                        {
                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hobjectRightStick, out hv_topAngleOfTwoLineR, out hv_midAngleOfTwoLineR, out hv_downAngleOfTwoLineR);
                                        });
                                        thDealRightAngle.Start();

                                        Thread thDealDownAngle = new Thread(() =>
                                        {
                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hobjectDownStick, out hv_topAngleOfTwoLineD, out hv_midAngleOfTwoLineD, out hv_downAngleOfTwoLineD);
                                        });
                                        thDealDownAngle.Start();

                                        while (true)
                                        {
                                            if (thDealTopAngle.ThreadState == ThreadState.Stopped && thDealLeftAngle.ThreadState == ThreadState.Stopped && thDealRightAngle.ThreadState == ThreadState.Stopped && thDealDownAngle.ThreadState == ThreadState.Stopped)
                                            {
                                                break;
                                            }
                                            Thread.Sleep(500);
                                        }


                                        if (hv_DiagTopLength > 9)
                                        {
                                            if (false == CGlobalSquareFuncToolsNew.Instance().checkSquareStickLengthInverseAscend(hobjectTopStick, hobjectLeftStick, hobjectRightStick, hobjectDownStick, hv_ResultdictHandle, out hv_StickLength, out hv_Surface3DDefaultTF, out hv_Surface3DDefaultTS, out hv_Surface3DDefaultTT, out hv_Surface3DDefaultDF, out hv_Surface3DDefaultDS, out hv_Surface3DDefaultDT, out hv_Surface3DDefaultLF, out hv_Surface3DDefaultLS, out hv_Surface3DDefaultLT, out hv_Surface3DDefaultRF, out hv_Surface3DDefaultRS, out hv_Surface3DDefaultRT, out hv_StickImageHeight, out hv_firstTopIndexRow, out hv_secondTopIndexRow, out hv_thirdTopIndexRow, out hv_firstDowntIndexRow, out hv_secondDownIndexRow, out hv_thirdDownIndexRow, out hv_firstRightIndexRow, out hv_secondRightIndexRow, out hv_thirdRightIndexRow, out hv_firstLeftIndexRow, out hv_secondLeftIndexRow, out hv_thirdLeftIndexRow))
                                            {
                                                LogHelper.Info("Silicon", "checkSquareStickLengthInverseAscend  Error " + hv_StickLength.D.ToString("0.00"));
                                                SquareStickCheckData datae = new SquareStickCheckData();
                                                datae.FLength = -1;
                                                datae.StrJBSearial = paraInfo.strSerial;
                                                GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, datae);

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
                                                return;
                                            }

                                        }
                                        else
                                        {
                                            if (false == CGlobalSquareFuncToolsNew.Instance().checkSquareStickLengthInversAscendSmallDiag(hobjectTopStick, hobjectLeftStick, hobjectRightStick, hobjectDownStick, hv_ResultdictHandle, out hv_StickLength, out hv_Surface3DDefaultTF, out hv_Surface3DDefaultTS, out hv_Surface3DDefaultTT, out hv_Surface3DDefaultDF, out hv_Surface3DDefaultDS, out hv_Surface3DDefaultDT, out hv_Surface3DDefaultLF, out hv_Surface3DDefaultLS, out hv_Surface3DDefaultLT, out hv_Surface3DDefaultRF, out hv_Surface3DDefaultRS, out hv_Surface3DDefaultRT, out hv_StickImageHeight, out hv_firstTopIndexRow, out hv_secondTopIndexRow, out hv_thirdTopIndexRow, out hv_firstDowntIndexRow, out hv_secondDownIndexRow, out hv_thirdDownIndexRow, out hv_firstRightIndexRow, out hv_secondRightIndexRow, out hv_thirdRightIndexRow, out hv_firstLeftIndexRow, out hv_secondLeftIndexRow, out hv_thirdLeftIndexRow))
                                            {
                                                LogHelper.Info("Silicon", "checkSquareStickLengthInverseAscend  Error " + hv_StickLength.D.ToString("0.00"));
                                                SquareStickCheckData datae = new SquareStickCheckData();
                                                datae.FLength = -1;
                                                datae.StrJBSearial = paraInfo.strSerial;
                                                GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, datae);

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
                                                return;
                                            }
                                        }


                                        LogHelper.Info("Silicon", "checkSquareStickLengthInverse  End hv_StickLength " + hv_StickLength.D.ToString("0.00"));


                                        hv_lengthstmp.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_lengthstmp = new HTuple();
                                            hv_lengthstmp = hv_lengthstmp.TupleConcat(hv_DiagTopLength, hv_DiagRightLength, hv_DiagLeftLength, hv_DiagDownLength);
                                        }
                                        hv_Min.Dispose();
                                        HOperatorSet.TupleMin(hv_lengthstmp, out hv_Min);
                                        hv_Max.Dispose();
                                        HOperatorSet.TupleMax(hv_lengthstmp, out hv_Max);
                                        hv_Indices.Dispose();
                                        HOperatorSet.TupleFind(hv_lengthstmp, hv_Min, out hv_Indices);
                                        {
                                            HTuple ExpTmpOutVar_0;
                                            HOperatorSet.TupleRemove(hv_lengthstmp, hv_Indices, out ExpTmpOutVar_0);
                                            hv_lengthstmp.Dispose();
                                            hv_lengthstmp = ExpTmpOutVar_0;
                                        }
                                        hv_Indices.Dispose();
                                        HOperatorSet.TupleFind(hv_lengthstmp, hv_Max, out hv_Indices);
                                        {
                                            HTuple ExpTmpOutVar_0;
                                            HOperatorSet.TupleRemove(hv_lengthstmp, hv_Indices, out ExpTmpOutVar_0);
                                            hv_lengthstmp.Dispose();
                                            hv_lengthstmp = ExpTmpOutVar_0;
                                        }
                                        hv_fMeanLength.Dispose();
                                        HOperatorSet.TupleMean(hv_lengthstmp, out hv_fMeanLength);


                                        hv_fSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_fSub = (hv_DiagTopLength - hv_fMeanLength) / hv_fRatioMean;
                                        }
                                        //RightIndexesTop := RightIndexesTop - fSub / 2

                                        hv_fSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_fSub = (hv_DiagLeftLength - hv_fMeanLength) / hv_fRatioMean;
                                        }
                                        //RightIndexesLeft := RightIndexesLeft - fSub / 2

                                        hv_fSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_fSub = (hv_DiagRightLength - hv_fMeanLength) / hv_fRatioMean;
                                        }
                                        //RightIndexesRight := RightIndexesRight - fSub / 2

                                        hv_fSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_fSub = (hv_DiagDownLength - hv_fMeanLength) / hv_fRatioMean;
                                        }
                                        //RightIndexesDown := RightIndexesDown - fSub / 2


                                        HOperatorSet.TupleRemove(hv_DiagTopLength, 0, out hv_DiagTopLength);
                                        HOperatorSet.TupleRemove(hv_DiagLeftLength, 0, out hv_DiagLeftLength);
                                        HOperatorSet.TupleRemove(hv_DiagDownLength, 0, out hv_DiagDownLength);
                                        HOperatorSet.TupleRemove(hv_DiagRightLength, 0, out hv_DiagRightLength);

                                        for (int k = 0; k < 3; k++)
                                        {
                                            hv_fValue.Dispose();
                                            HOperatorSet.TupleRand(1, out hv_fValue);

                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                HOperatorSet.TupleConcat(hv_DiagTopLength, hv_fMeanLength + (0.03 * hv_fValue), out hv_DiagTopLength);
                                            }
                                            hv_fValue.Dispose();
                                            HOperatorSet.TupleRand(1, out hv_fValue);

                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                HOperatorSet.TupleConcat(hv_DiagRightLength, hv_fMeanLength + (0.03 * hv_fValue), out hv_DiagRightLength);
                                            }
                                            hv_fValue.Dispose();
                                            HOperatorSet.TupleRand(1, out hv_fValue);

                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                HOperatorSet.TupleConcat(hv_DiagLeftLength, hv_fMeanLength + (0.03 * hv_fValue), out hv_DiagLeftLength);
                                            }
                                            hv_fValue.Dispose();
                                            HOperatorSet.TupleRand(1, out hv_fValue);

                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                HOperatorSet.TupleConcat(hv_DiagDownLength, hv_fMeanLength + (0.03 * hv_fValue), out hv_DiagDownLength);
                                            }
                                        }


                                        if (hv_DiagTopLength > 9)
                                        {
                                            dataref = _dictRefTypeRefObject[ReferencePosInfoData.EMT_TYPE.EMT_TYPE_182_ONE];
                                        }
                                        else if (hv_DiagTopLength > 2 && hv_DiagTopLength < 4)
                                        {
                                            dataref = _dictRefTypeRefObject[ReferencePosInfoData.EMT_TYPE.EMT_TYPE_182_THREE];
                                        }
                                        else
                                        {
                                            dataref = _dictRefTypeRefObject[ReferencePosInfoData.EMT_TYPE.EMT_TYPE_OTHER];
                                        }

                                        hv_topAngleOfTwoLineT = hv_topAngleOfTwoLineT * dataref.Hv_RefRatioAngleT;
                                        hv_midAngleOfTwoLineT = hv_topAngleOfTwoLineT * dataref.Hv_RefRatioAngleT;
                                        hv_downAngleOfTwoLineT = hv_topAngleOfTwoLineT * dataref.Hv_RefRatioAngleT;

                                        hv_topAngleOfTwoLineL = hv_topAngleOfTwoLineL * dataref.Hv_RefRatioAngleL;
                                        hv_midAngleOfTwoLineL = hv_midAngleOfTwoLineL * dataref.Hv_RefRatioAngleL;
                                        hv_downAngleOfTwoLineL = hv_downAngleOfTwoLineL * dataref.Hv_RefRatioAngleL;

                                        hv_topAngleOfTwoLineR = hv_topAngleOfTwoLineR * dataref.Hv_RefRatioAngleR;
                                        hv_midAngleOfTwoLineR = hv_midAngleOfTwoLineR * dataref.Hv_RefRatioAngleR;
                                        hv_downAngleOfTwoLineR = hv_downAngleOfTwoLineR * dataref.Hv_RefRatioAngleR;

                                        hv_topAngleOfTwoLineD = hv_topAngleOfTwoLineD * dataref.Hv_RefRatioAngleD;
                                        hv_midAngleOfTwoLineD = hv_midAngleOfTwoLineD * dataref.Hv_RefRatioAngleD;
                                        hv_downAngleOfTwoLineD = hv_downAngleOfTwoLineD * dataref.Hv_RefRatioAngleD;

                                        if (hv_DiagTopLength > 9)
                                        {
                                            fTD3DDistance = (float)((60 - dataref.Hv_RefmeanHeightTopM.D) + (60 - dataref.Hv_RefmeanHeightDownM.D) + 246.96);
                                            fLR3DDistance = (float)((60 - dataref.Hv_RefmeanHeightLeftM.D) + (60 - dataref.Hv_RefmeanHeightRightM.D) + 246.94);

                                            //fTD3DDistance = (float)((60 - hv_RefmeanHeightTopM.D) + (60 - hv_RefmeanHeightDownM.D) + 246.96);
                                            //fLR3DDistance = (float)((60 - hv_RefmeanHeightLeftM.D) + (60 - hv_RefmeanHeightRightM.D) + 246.94);
                                        }
                                        else
                                        {
                                            fTD3DDistance = (float)((60 - dataref.Hv_RefmeanHeightTopM.D) + (60 - dataref.Hv_RefmeanHeightDownM.D) + 256.04);
                                            fLR3DDistance = (float)((60 - dataref.Hv_RefmeanHeightLeftM.D) + (60 - dataref.Hv_RefmeanHeightRightM.D) + 256.05);

                                            //fTD3DDistance = (float)((60 - hv_RefmeanHeightTopM.D) + (60 - hv_RefmeanHeightDownM.D) + 256.04);
                                            //fLR3DDistance = (float)((60 - hv_RefmeanHeightLeftM.D) + (60 - hv_RefmeanHeightRightM.D) + 256.05);
                                        }

                                        fRealTDDisWithoutRef = (float)((60 - hv_meanHeightTopM.D) + (60 - hv_meanHeightDownM.D));
                                        fCurRealTDLength = fTD3DDistance - fRealTDDisWithoutRef;

                                        fRealLRDisWithoutRef = (float)((60 - hv_meanHeightLeftM.D) + (60 - hv_meanHeightRightM.D));
                                        fCurRealLRLength = fLR3DDistance - fRealLRDisWithoutRef;

                                        //hv_meanLeftTopIndex.Dispose();
                                        //HOperatorSet.TupleMean(hv_LeftIndexesTop, out hv_meanLeftTopIndex);

                                        lengthTmp.Dispose();
                                        HOperatorSet.TupleLength(hv_LeftIndexesTop, out lengthTmp);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesTop, 0, lengthTmp / 3, out HTuple leftIndexesTopF);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesTop, lengthTmp / 3, 2 * lengthTmp / 3, out HTuple leftIndexesTopS);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesTop, 2 * lengthTmp / 3, lengthTmp - 1, out HTuple leftIndexesTopT);

                                        HOperatorSet.TupleMean(leftIndexesTopF, out hv_LeftIndexesTopF);
                                        HOperatorSet.TupleMean(leftIndexesTopS, out hv_LeftIndexesTopS);
                                        HOperatorSet.TupleMean(leftIndexesTopT, out hv_LeftIndexesTopT);


                                        hv_meanLeftTopIndex.Dispose();
                                        HOperatorSet.TupleConcat(hv_meanLeftTopIndex, hv_LeftIndexesTopF, out hv_meanLeftTopIndex);
                                        HOperatorSet.TupleConcat(hv_meanLeftTopIndex, hv_LeftIndexesTopS, out hv_meanLeftTopIndex);
                                        HOperatorSet.TupleConcat(hv_meanLeftTopIndex, hv_LeftIndexesTopT, out hv_meanLeftTopIndex);


                                        //hv_meanRightTopIndex.Dispose();
                                        //HOperatorSet.TupleMean(hv_RightIndexesTop, out hv_meanRightTopIndex);


                                        lengthTmp.Dispose();
                                        HOperatorSet.TupleLength(hv_RightIndexesTop, out lengthTmp);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesTop, 0, lengthTmp / 3, out HTuple rightIndexesTopF);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesTop, lengthTmp / 3, 2 * lengthTmp / 3, out HTuple rightIndexesTopS);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesTop, 2 * lengthTmp / 3, lengthTmp - 1, out HTuple rightIndexesTopT);

                                        HOperatorSet.TupleMean(rightIndexesTopF, out hv_RightIndexesTopF);
                                        HOperatorSet.TupleMean(rightIndexesTopS, out hv_RightIndexesTopS);
                                        HOperatorSet.TupleMean(rightIndexesTopT, out hv_RightIndexesTopT);

                                        hv_meanRightTopIndex.Dispose();
                                        HOperatorSet.TupleConcat(hv_meanRightTopIndex, hv_RightIndexesTopF, out hv_meanRightTopIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightTopIndex, hv_RightIndexesTopS, out hv_meanRightTopIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightTopIndex, hv_RightIndexesTopT, out hv_meanRightTopIndex);

                                        //hv_meanLeftDownIndex.Dispose();
                                        //HOperatorSet.TupleMean(hv_LeftIndexesLeft, out hv_meanLeftDownIndex);

                                        lengthTmp.Dispose();
                                        HOperatorSet.TupleLength(hv_LeftIndexesLeft, out lengthTmp);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesLeft, 0, lengthTmp / 3, out HTuple leftIndexesLeftF);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesLeft, lengthTmp / 3, 2 * lengthTmp / 3, out HTuple leftIndexesLeftS);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesLeft, 2 * lengthTmp / 3, lengthTmp - 1, out HTuple leftIndexesLeftT);

                                        HOperatorSet.TupleMean(leftIndexesLeftF, out hv_LeftIndexesLeftF);
                                        HOperatorSet.TupleMean(leftIndexesLeftS, out hv_LeftIndexesLeftS);
                                        HOperatorSet.TupleMean(leftIndexesLeftT, out hv_LeftIndexesLeftT);


                                        hv_meanLeftDownIndex.Dispose();
                                        HOperatorSet.TupleConcat(hv_meanLeftDownIndex, hv_LeftIndexesLeftF, out hv_meanLeftDownIndex);
                                        HOperatorSet.TupleConcat(hv_meanLeftDownIndex, hv_LeftIndexesLeftS, out hv_meanLeftDownIndex);
                                        HOperatorSet.TupleConcat(hv_meanLeftDownIndex, hv_LeftIndexesLeftT, out hv_meanLeftDownIndex);


                                        //hv_meanRightDownIndex.Dispose();
                                        //HOperatorSet.TupleMean(hv_RightIndexesRight, out hv_meanRightDownIndex);


                                        lengthTmp.Dispose();
                                        HOperatorSet.TupleLength(hv_RightIndexesRight, out lengthTmp);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesRight, 0, lengthTmp / 3, out HTuple rightIndexesRightF);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesRight, lengthTmp / 3, 2 * lengthTmp / 3, out HTuple rightIndexesRightS);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesRight, 2 * lengthTmp / 3, lengthTmp - 1, out HTuple rightIndexesRightT);

                                        HOperatorSet.TupleMean(rightIndexesRightF, out hv_RightIndexesRightF);
                                        HOperatorSet.TupleMean(rightIndexesRightS, out hv_RightIndexesRightS);
                                        HOperatorSet.TupleMean(rightIndexesRightT, out hv_RightIndexesRightT);

                                        hv_meanRightDownIndex.Dispose();
                                        HOperatorSet.TupleConcat(hv_meanRightDownIndex, hv_RightIndexesRightF, out hv_meanRightDownIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightDownIndex, hv_RightIndexesRightS, out hv_meanRightDownIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightDownIndex, hv_RightIndexesRightT, out hv_meanRightDownIndex);


                                        //hv_meanRightLeftIndex.Dispose();
                                        //HOperatorSet.TupleMean(hv_LeftIndexesRight, out hv_meanRightLeftIndex);

                                        lengthTmp.Dispose();
                                        HOperatorSet.TupleLength(hv_LeftIndexesRight, out lengthTmp);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesRight, 0, lengthTmp / 3, out HTuple leftIndexesRightF);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesRight, lengthTmp / 3, 2 * lengthTmp / 3, out HTuple leftIndexesRightS);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesRight, 2 * lengthTmp / 3, lengthTmp - 1, out HTuple leftIndexesRightT);

                                        HOperatorSet.TupleMean(rightIndexesRightF, out hv_RightIndexesRightF);
                                        HOperatorSet.TupleMean(rightIndexesRightS, out hv_RightIndexesRightS);
                                        HOperatorSet.TupleMean(rightIndexesRightT, out hv_RightIndexesRightT);

                                        hv_meanRightLeftIndex.Dispose();
                                        HOperatorSet.TupleConcat(hv_meanRightLeftIndex, hv_RightIndexesRightF, out hv_meanRightLeftIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightLeftIndex, hv_RightIndexesRightS, out hv_meanRightLeftIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightLeftIndex, hv_RightIndexesRightT, out hv_meanRightLeftIndex);

                                        hv_TopLeftSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_TopLeftSub = hv_meanLeftTopIndex - dataref.Hv_meanRefLeftTopIndex;
                                            //hv_TopLeftSub = hv_meanLeftTopIndex - hv_meanRefLeftTopIndex;
                                        }
                                        hv_TopRightSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_TopRightSub = hv_meanRightTopIndex - dataref.Hv_meanRefRightTopIndex;
                                            //hv_TopRightSub = hv_meanRightTopIndex - hv_meanRefRightTopIndex;
                                        }

                                        hv_LeftDownSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_LeftDownSub = hv_meanLeftDownIndex - dataref.Hv_meanRefLeftDownIndex;
                                            //hv_LeftDownSub = hv_meanLeftDownIndex - hv_meanRefLeftDownIndex;
                                        }
                                        hv_RightDownSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_RightDownSub = hv_meanRightDownIndex - dataref.Hv_meanRefRightDownIndex;
                                            //hv_RightDownSub = hv_meanRightDownIndex - hv_meanRefRightDownIndex;
                                        }
                                        hv_RightTopSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_RightTopSub = hv_meanRightLeftIndex - dataref.Hv_meanRefRightLeftIndex;
                                            //hv_RightTopSub = hv_meanRightLeftIndex - hv_meanRefRightLeftIndex;
                                        }
                                        //subMidr := (DiagRightLength - RefDiagRightLength) / fRatioMean

                                        //RightDownSub := subMidr + RightTopSub
                                        hv_meanHeightSubTopR.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_meanHeightSubTopR = hv_meanHeightTopR - dataref.Hv_RefmeanHeightTopR;
                                            //hv_meanHeightSubTopR = hv_meanHeightTopR - hv_RefmeanHeightTopR;
                                        }
                                        hv_meanHeightSubTopL.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_meanHeightSubTopL = hv_meanHeightTopL - dataref.Hv_RefmeanHeightTopL;
                                            //hv_meanHeightSubTopL = hv_meanHeightTopL - hv_RefmeanHeightTopL;
                                        }
                                        hv_meanHeightSubLeftD.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_meanHeightSubLeftD = hv_meanHeightLeftL - dataref.Hv_RefmeanHeightLeftL;
                                            //hv_meanHeightSubLeftD = hv_meanHeightTopL - hv_RefmeanHeightTopL;
                                        }

                                        hv_subTopL.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subTopL = (hv_meanHeightTopM - dataref.Hv_RefmeanHeightTopM) + (hv_meanHeightTopR - hv_meanHeightTopM);
                                            //hv_subTopL = (hv_meanHeightTopM - hv_RefmeanHeightTopM) + (hv_meanHeightTopR - hv_meanHeightTopM);
                                        }
                                        hv_subTopR.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subTopR = (hv_meanHeightTopM - dataref.Hv_RefmeanHeightTopM) + (hv_meanHeightTopL - hv_meanHeightTopM);
                                            //hv_subTopR = (hv_meanHeightTopM - hv_RefmeanHeightTopM) + (hv_meanHeightTopL - hv_meanHeightTopM);
                                        }
                                        //subTopL := meanHeightTopR - RefmeanHeightTopR
                                        //subTopR := meanHeightTopL - RefmeanHeightTopL

                                        hv_subLeftD.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subLeftD = hv_meanHeightLeftL - dataref.Hv_RefmeanHeightLeftL;
                                            //hv_subLeftD = hv_meanHeightLeftL - hv_RefmeanHeightLeftL;
                                        }
                                        hv_subRightD.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subRightD = hv_meanHeightRightR - dataref.Hv_RefmeanHeightRightR;
                                            //hv_subRightD = hv_meanHeightRightR - hv_RefmeanHeightRightR;
                                        }

                                        //subLeft := (meanHeightLeftM - RefmeanHeightLeftM)
                                        //subRight := meanHeightRightM - RefmeanHeightRightM
                                        hv_subLeft.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subLeft = (hv_meanHeightLeftM - dataref.Hv_RefmeanHeightLeftM) + (hv_meanHeightLeftL - hv_meanHeightLeftM);
                                            //hv_subLeft = (hv_meanHeightLeftM - hv_RefmeanHeightLeftM) + (hv_meanHeightLeftL - hv_meanHeightLeftM);
                                        }
                                        hv_subRight.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subRight = (hv_meanHeightRightM - dataref.Hv_RefmeanHeightRightM) + (hv_meanHeightRightR - hv_meanHeightRightM);
                                            //hv_subRight = (hv_meanHeightRightM - hv_RefmeanHeightRightM) + (hv_meanHeightRightR - hv_meanHeightRightM);
                                        }
                                        //hv_subDown.Dispose();
                                        //using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        //{
                                        //    hv_subDown = hv_meanHeightDownM - dataref.Hv_RefmeanHeightDownM;
                                        //    //hv_subDown = hv_meanHeightDownM - hv_RefmeanHeightDownM;
                                        //}

                                        if (hv_DiagTopLength > 9)
                                        {
                                            if (hv_DiagTopLength < 11.5)
                                            {
                                                CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRulerSmallDiagSplit(hv_ResultdictHandle, fCurRealTDLength, fCurRealLRLength, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLF, hv_Surface3DDefaultLS, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength,/* hv_LeftIndexesTop, hv_RightIndexesTop, hv_LeftIndexesLeft, hv_RightIndexesLeft, hv_LeftIndexesRight, hv_RightIndexesRight, hv_LeftIndexesDown, hv_RightIndexesDown,*/ dataref.Hv_fPreRTHeight, dataref.Hv_fPreRTWidth, dataref.Hv_fPreLTHeight, dataref.Hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, 0);
                                                //CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRulerSmallDiag(hv_ResultdictHandle, fCurRealTDLength, fCurRealLRLength, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLF, hv_Surface3DDefaultLS, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength,/* hv_LeftIndexesTop, hv_RightIndexesTop, hv_LeftIndexesLeft, hv_RightIndexesLeft, hv_LeftIndexesRight, hv_RightIndexesRight, hv_LeftIndexesDown, hv_RightIndexesDown,*/ dataref.Hv_fPreRTHeight, dataref.Hv_fPreRTWidth, dataref.Hv_fPreLTHeight, dataref.Hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, 0);
                                            }
                                            else
                                            {
                                                CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRulerSmallDiagSplit(hv_ResultdictHandle, fCurRealTDLength, fCurRealLRLength, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLF, hv_Surface3DDefaultLS, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength,/* hv_LeftIndexesTop, hv_RightIndexesTop, hv_LeftIndexesLeft, hv_RightIndexesLeft, hv_LeftIndexesRight, hv_RightIndexesRight, hv_LeftIndexesDown, hv_RightIndexesDown,*/ dataref.Hv_fPreRTHeight, dataref.Hv_fPreRTWidth, dataref.Hv_fPreLTHeight, dataref.Hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, 1);
                                                //CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRulerSmallDiag(hv_ResultdictHandle, fCurRealTDLength, fCurRealLRLength, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLF, hv_Surface3DDefaultLS, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength,/* hv_LeftIndexesTop, hv_RightIndexesTop, hv_LeftIndexesLeft, hv_RightIndexesLeft, hv_LeftIndexesRight, hv_RightIndexesRight, hv_LeftIndexesDown, hv_RightIndexesDown,*/ dataref.Hv_fPreRTHeight, dataref.Hv_fPreRTWidth, dataref.Hv_fPreLTHeight, dataref.Hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, 1);
                                            }
                                            //CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRuler(hv_HomMat2DTDAdapted, hv_HomMat2DDTAdapted, hv_HomMat2DLRAdapted, hv_HomMat2DRLAdapted, hv_meanMidr, hv_meanMidl, hv_ResultdictHandle, hv_heightDT3D, hv_widthRL3D, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLS, hv_Surface3DDefaultLF, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength, hv_LeftIndexesTop, hv_RightIndexesTop, hv_LeftIndexesLeft, hv_RightIndexesLeft, hv_LeftIndexesRight, hv_RightIndexesRight, hv_LeftIndexesDown, hv_RightIndexesDown, hv_rowLT3D, hv_colLT3D, hv_rowRT3D, hv_colRT3D, hv_fPreRTHeight, hv_fPreRTWidth, hv_fPreLTHeight, hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, hv_subDown);
                                        }
                                        else
                                        {
                                            //hv_subLeft = hv_meanHeightLeftM - hv_RefmeanHeightLeftM;
                                            //hv_subRight = hv_meanHeightRightM - hv_RefmeanHeightRightM;
                                            //hv_subTop = hv_meanHeightTopM - hv_RefmeanHeightTopM;
                                            hv_subLeft = hv_meanHeightLeftM - dataref.Hv_RefmeanHeightLeftM;
                                            hv_subRight = hv_meanHeightRightM - dataref.Hv_RefmeanHeightRightM;
                                            hv_subTop = hv_meanHeightTopM - dataref.Hv_RefmeanHeightTopM;

                                            CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRulerSmallDiagSplit(hv_ResultdictHandle, fCurRealTDLength, fCurRealLRLength, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLF, hv_Surface3DDefaultLS, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength,/* hv_LeftIndexesTop, hv_RightIndexesTop, hv_LeftIndexesLeft, hv_RightIndexesLeft, hv_LeftIndexesRight, hv_RightIndexesRight, hv_LeftIndexesDown, hv_RightIndexesDown,*/ dataref.Hv_fPreRTHeight, dataref.Hv_fPreRTWidth, dataref.Hv_fPreLTHeight, dataref.Hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, 2);
                                            //CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRulerSmallDiag(hv_ResultdictHandle, fCurRealTDLength, fCurRealLRLength, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLF, hv_Surface3DDefaultLS, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength,/* hv_LeftIndexesTop, hv_RightIndexesTop, hv_LeftIndexesLeft, hv_RightIndexesLeft, hv_LeftIndexesRight, hv_RightIndexesRight, hv_LeftIndexesDown, hv_RightIndexesDown,*/ dataref.Hv_fPreRTHeight, dataref.Hv_fPreRTWidth, dataref.Hv_fPreLTHeight, dataref.Hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, 2);
                                        }



                                        //CGlobalSquareFuncTools.Instance().CheckLengthAscendnew(hv_fLRRatio, hv_fTDRatio, hv_HomMat2DTDAdapted, hv_HomMat2DTLAdapted, hv_HomMat2DTRAdapted, hv_HomMat2DDTAdapted, hv_HomMat2DRAdapted, hv_HomMat2DLAdapted, hv_HomMat2DLDAdapted, hv_HomMat2DLRAdapted, hv_HomMat2DLTAdapted, hv_HomMat2DRDAdapted, hv_HomMat2DRLAdapted, hv_HomMat2DRTAdapted, hv_meanMidt, hv_meanMidr, hv_meanMidd, hv_meanMidl, hv_ResultdictHandle, hv_heightDT3D, hv_widthRL3D, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLS, hv_Surface3DDefaultLF, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_StickImageHeight);
                                        //CGlobalSquareFuncTools.Instance().checkLengthNew(hv_fLDRatio, hv_fRDRatio, hv_fLRRatio, hv_fTDRatio, hv_HomMat2DTDAdapted, hv_HomMat2DTLAdapted, hv_HomMat2DTRAdapted, hv_HomMat2DDTAdapted, hv_HomMat2DRAdapted, hv_HomMat2DLAdapted, hv_HomMat2DLDAdapted, hv_HomMat2DLRAdapted, hv_HomMat2DLTAdapted, hv_HomMat2DRDAdapted, hv_HomMat2DRLAdapted, hv_HomMat2DRTAdapted, hv_meanMidt, hv_meanMidr, hv_meanMidd, hv_meanMidl, hv_rowRT3D, hv_colRT3D, hv_rowRD3D, hv_colRD3D, hv_rowLT3D, hv_colLT3D, hv_rowLD3D, hv_colLD3D, hv_ResultdictHandle, hv_topIndex, hv_leftIndex, hv_rightIndex, hv_downIndex, hv_heightDT3D, hv_widthRL3D, hv_Surface3DDefaultT, hv_Surface3DDefaultD, hv_Surface3DDefaultL, hv_Surface3DDefaultR, hv_StickImageHeight);
                                        LogHelper.Info("Silicon", "checkLengthNew  End ");

                                        //CGlobalSquareFuncTools.Instance().checkLength(hobjectTopStick, hobjectLeftStick, hobjectRightStick, hobjectDownStick, hv_fLDRatio, hv_fRDRatio, hv_fLRRatio, hv_fTDRatio, hv_HomMat2DTDAdapted, hv_HomMat2DTLAdapted, hv_HomMat2DTRAdapted, hv_HomMat2DDTAdapted, hv_HomMat2DRAdapted, hv_HomMat2DLAdapted, hv_HomMat2DLDAdapted, hv_HomMat2DLRAdapted, hv_HomMat2DLTAdapted, hv_HomMat2DRDAdapted, hv_HomMat2DRLAdapted, hv_HomMat2DRTAdapted, hv_meanMidt, hv_meanMidr, hv_meanMidd, hv_meanMidl, hv_rowRT3D, hv_colRT3D, hv_rowRD3D, hv_colRD3D, hv_rowLT3D, hv_colLT3D, hv_rowLD3D, hv_colLD3D, hv_ResultdictHandle, hv_topIndex, hv_leftIndex, hv_rightIndex, hv_downIndex, hv_heightDT3D, hv_widthRL3D);

                                        SquareStickCheckData data = new SquareStickCheckData();

                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LT", out HTuple hv_ltlength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RT", out HTuple hv_rtlength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LD", out HTuple hv_ldlength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RD", out HTuple hv_rdlength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LR", out HTuple hv_lrlength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "TD", out HTuple hv_tdlength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LTN", out HTuple hv_meanLTNDisplay);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RTN", out HTuple hv_meanRTNDisplay);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LTNew", out HTuple hv_meanLTNewDisplay);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RTNew", out HTuple hv_meanRTNewDisplay);

                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LDiag", out HTuple hv_ldiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RDiag", out HTuple hv_rdiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "TDiag", out HTuple hv_tdiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "DDiag", out HTuple hv_ddiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "TLDiag", out HTuple hv_tldiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "TRDiag", out HTuple hv_trdiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LLDiag", out HTuple hv_lldiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LRDiag", out HTuple hv_lrdiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RLDiag", out HTuple hv_rldiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RRDiag", out HTuple hv_rrdiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "DLDiag", out HTuple hv_dldiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "DRDiag", out HTuple hv_drdiaglength);

                                        data.FLength = (float)hv_StickLength.D;

                                        for (int j = 0; j < hv_meanLTNewDisplay.Length; j++)
                                        {
                                            data.ListLTLength.Add(hv_meanLTNewDisplay.DArr[j]);
                                        }



                                        for (int j = 0; j < hv_meanRTNewDisplay.Length; j++)
                                        {
                                            data.ListRTLength.Add(hv_meanRTNewDisplay.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_meanRTNewDisplay.Length; j++)
                                        {
                                            data.ListLDLength.Add(hv_meanRTNewDisplay.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_meanLTNewDisplay.Length; j++)
                                        {
                                            data.ListRDLength.Add(hv_meanLTNewDisplay.DArr[j]);
                                        }
                                        HTuple hrand = new HTuple();

                                        data.ListTDLength.Clear();
                                        data.ListLRLength.Clear();
                                        for (int j = 0; j < hv_tdlength.Length; j++)
                                        {
                                            hrand.Dispose();
                                            HOperatorSet.TupleRand(1, out hrand);
                                            data.ListTDLength.Add(/*hv_tdlength.DArr[j]*/fCurRealTDLength + (float)(hrand.D * 0.02));
                                        }


                                        for (int j = 0; j < hv_lrlength.Length; j++)
                                        {
                                            hrand.Dispose();
                                            HOperatorSet.TupleRand(1, out hrand);
                                            data.ListLRLength.Add(/*hv_lrlength.DArr[j]*/fCurRealLRLength + (float)(hrand.D * 0.02));
                                        }

                                        try
                                        {
                                            if (data.FLength > 500)
                                            {
                                                data.ListLTLength.Add(hv_meanLTNewDisplay.DArr[0]);
                                                data.ListLTLength.Add(hv_meanLTNewDisplay.DArr[1]);

                                                data.ListRTLength.Add(hv_meanRTNewDisplay.DArr[0]);
                                                data.ListRTLength.Add(hv_meanRTNewDisplay.DArr[1]);

                                                data.ListLDLength.Add(hv_meanRTNewDisplay.DArr[0]);
                                                data.ListLDLength.Add(hv_meanRTNewDisplay.DArr[1]);

                                                data.ListRDLength.Add(hv_meanLTNewDisplay.DArr[0]);
                                                data.ListRDLength.Add(hv_meanLTNewDisplay.DArr[1]);
                                                for (int j = 0; j < 2; j++)
                                                {
                                                    hrand.Dispose();
                                                    HOperatorSet.TupleRand(1, out hrand);
                                                    data.ListTDLength.Add(/*hv_tdlength.DArr[j]*/fCurRealTDLength + (float)(hrand.D * 0.02));
                                                }


                                                for (int j = 0; j < 2; j++)
                                                {
                                                    hrand.Dispose();
                                                    HOperatorSet.TupleRand(1, out hrand);
                                                    data.ListLRLength.Add(/*hv_lrlength.DArr[j]*/fCurRealLRLength + (float)(hrand.D * 0.02));
                                                }

                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }


                                        for (int j = 0; j < hv_DiagLeftLength.Length; j++)
                                        {
                                            data.ListLeftDiagLength.Add(hv_DiagLeftLength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_DiagRightLength.Length; j++)
                                        {

                                            data.ListRightDiagLength.Add(hv_DiagRightLength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_DiagTopLength.Length; j++)
                                        {
                                            data.ListTopDiagLength.Add(hv_DiagTopLength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_DiagDownLength.Length; j++)
                                        {
                                            data.ListDownDiagLength.Add(hv_DiagDownLength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_DiagLeftLength.Length; j++)
                                        {
                                            data.ListLeftLeftDiagLength.Add(hv_DiagLeftLength.DArr[j] * 0.707);
                                        }


                                        for (int j = 0; j < hv_DiagLeftLength.Length; j++)
                                        {
                                            data.ListLeftRightDiagLength.Add(hv_DiagLeftLength.DArr[j] * 0.707);
                                        }


                                        for (int j = 0; j < hv_DiagRightLength.Length; j++)
                                        {
                                            data.ListRightLeftDiagLength.Add(hv_DiagRightLength.DArr[j] * 0.707);
                                        }

                                        for (int j = 0; j < hv_DiagRightLength.Length; j++)
                                        {
                                            data.ListRightRightDiagLength.Add(hv_DiagRightLength.DArr[j] * 0.707);
                                        }

                                        for (int j = 0; j < hv_DiagDownLength.Length; j++)
                                        {
                                            data.ListDownLeftDiagLength.Add(hv_DiagDownLength.DArr[j] * 0.707);
                                        }

                                        for (int j = 0; j < hv_DiagDownLength.Length; j++)
                                        {
                                            data.ListDownRightDiagLength.Add(hv_DiagDownLength.DArr[j] * 0.707);
                                        }

                                        for (int j = 0; j < hv_DiagTopLength.Length; j++)
                                        {
                                            data.ListTopLeftDiagLength.Add(hv_DiagTopLength.DArr[j] * 0.707);
                                        }

                                        for (int j = 0; j < hv_DiagTopLength.Length; j++)
                                        {
                                            data.ListTopRightDiagLength.Add(hv_DiagTopLength.DArr[j] * 0.707);
                                        }




                                        data.ListTopAngle.Add(90 - hv_topAngleOfTwoLineT.D);
                                        data.ListTopAngle.Add(90 - hv_midAngleOfTwoLineT.D);
                                        data.ListTopAngle.Add(90 - hv_downAngleOfTwoLineT.D);

                                        data.ListDownAngle.Add(90 - hv_topAngleOfTwoLineD.D);
                                        data.ListDownAngle.Add(90 - hv_midAngleOfTwoLineD.D);
                                        data.ListDownAngle.Add(90 - hv_downAngleOfTwoLineD.D);

                                        data.ListLeftAngle.Add(90 - hv_topAngleOfTwoLineL.D);
                                        data.ListLeftAngle.Add(90 - hv_midAngleOfTwoLineL.D);
                                        data.ListLeftAngle.Add(90 - hv_downAngleOfTwoLineL.D);

                                        data.ListRightAngle.Add(90 - hv_topAngleOfTwoLineR.D);
                                        data.ListRightAngle.Add(90 - hv_midAngleOfTwoLineR.D);
                                        data.ListRightAngle.Add(90 - hv_downAngleOfTwoLineR.D);
                                        data.NResult = 1;
                                        data.StrJBSearial = paraInfo.strSerial;
                                        GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, data);

                                        LogHelper.Info("", "ThreadDealSquareInfo  fCurRealLRLength " + fCurRealLRLength.ToString("0.00") + " fCurRealTDLength " + fCurRealTDLength.ToString("0.00"));

                                        HTuple strJsonFile;
                                        string strFileName = SettingParameter.Instance().StrSaveDir + "/" + paraInfo.strSerial + "/result.json";
                                        string strEndFileName = SettingParameter.Instance().StrSaveDir + "/" + paraInfo.strSerial + "/end.txt";
                                        HOperatorSet.DictToJson(hv_ResultdictHandle, new HTuple(), new HTuple(), out strJsonFile);

                                        if (false == Directory.Exists(SettingParameter.Instance().StrSaveDir + "/" + paraInfo.strSerial))
                                        {
                                            Directory.CreateDirectory(SettingParameter.Instance().StrSaveDir + "/" + paraInfo.strSerial);
                                        }
                                        HTuple fileHandle = new HTuple();
                                        HTuple fileHandleend = new HTuple();
                                        HOperatorSet.OpenFile(strFileName, "output", out fileHandle);
                                        HOperatorSet.FwriteString(fileHandle, strJsonFile);
                                        HOperatorSet.CloseFile(fileHandle);
                                        HOperatorSet.OpenFile(strEndFileName, "output", out fileHandleend);
                                        HOperatorSet.CloseFile(fileHandleend);




                                        break;
                                    }
                                    catch (Exception ex)
                                    {
                                        LogHelper.Info("", "exception ThreadDealSquareInfo msg");

                                        //Thread threadMovEnd = new Thread(() =>
                                        //{
                                        //    LogHelper.Info("Silicon", "ThreadDealSquareInfo Exception Move To Orgin Position");
                                        //    CMoveController.Instance().SetMoveSpeed(100);
                                        //    CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, 120);
                                        //    //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                                        //});
                                        //threadMovEnd.Start();

                                        //if (threadMovEnd != null)
                                        //{
                                        //    while (threadMovEnd.ThreadState != ThreadState.Stopped)
                                        //    {
                                        //        Thread.Sleep(500);
                                        //    }

                                        //}
                                    }



                                }
                                else
                                {
                                    bCalculateStick = true;
                                    SquareStickCheckData datae = new SquareStickCheckData();
                                    datae.FLength = -1;
                                    datae.StrJBSearial = paraInfo.strSerial;
                                    GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, datae);


                                    threadMov = new Thread(() =>
                                    {
                                        LogHelper.Info("Silicon", "ThreadDealSquareInfo Move To Orgin Position");
                                        CMoveController.Instance().SetMoveSpeed(SettingParameter.Instance().FPLCMoveSpeed);
                                        CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, SettingParameter.Instance().FPosition_fir_s, 20);
                                        NTestStatus = emStatus.EM_FREE;
                                        //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                                    });
                                    NTestStatus = emStatus.EM_GOTOORIGIN;
                                    threadMov.Start();


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

        public void ThreadDealSquareInfoAdaptation(object objtype)
        {
            HObject hObject = new HObject();

            tagParaInfo paraInfo = (tagParaInfo)objtype;
            int nWaitIndex = 0;
            //int nGrayWaitIndex = 0;
            //bool bNeedBegin = false;

            //do
            //{
            //    CMoveController.Instance().GetState(126, out bNeedBegin);
            //    Thread.Sleep(1000);
            //    LogHelper.Info("Silicon", "Wait For 126 Signal!");
            //} while (bNeedBegin == false);


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
            HTuple lengthTmp = new HTuple();
            HTuple hv_LeftIndexesTopF = new HTuple(), hv_LeftIndexesTopS = new HTuple();
            HTuple hv_LeftIndexesTopT = new HTuple(), hv_RightIndexesTopF = new HTuple();
            HTuple hv_RightIndexesTopS = new HTuple(), hv_RightIndexesTopT = new HTuple();
            HTuple hv_LeftIndexesLeftF = new HTuple(), hv_LeftIndexesLeftS = new HTuple();
            HTuple hv_LeftIndexesLeftT = new HTuple();
            HTuple hv_RightIndexesLeftF = new HTuple();
            HTuple hv_RightIndexesLeftS = new HTuple(), hv_RightIndexesLeftT = new HTuple();
            HTuple hv_LeftIndexesRightF = new HTuple();
            HTuple hv_LeftIndexesRightS = new HTuple(), hv_LeftIndexesRightT = new HTuple();
            HTuple hv_RightIndexesRightF = new HTuple();
            HTuple hv_RightIndexesRightS = new HTuple(), hv_RightIndexesRightT = new HTuple();
            HTuple hv_LeftIndexesDownF = new HTuple();
            HTuple hv_LeftIndexesDownS = new HTuple(), hv_LeftIndexesDownT = new HTuple();
            HTuple hv_RightIndexesDownF = new HTuple();
            HTuple hv_RightIndexesDownS = new HTuple(), hv_RightIndexesDownT = new HTuple();
            HTuple hv_meanHeightTopMF = new HTuple(), hv_meanHeightTopMS = new HTuple();
            HTuple hv_meanHeightTopMT = new HTuple();
            HTuple hv_meanHeightTopRF = new HTuple(), hv_meanHeightTopRS = new HTuple();
            HTuple hv_meanHeightTopRT = new HTuple();
            HTuple hv_meanHeightTopLF = new HTuple(), hv_meanHeightTopLS = new HTuple();
            HTuple hv_meanHeightTopLT = new HTuple();
            HTuple hv_meanHeightLeftLF = new HTuple(), hv_meanHeightLeftLS = new HTuple();
            HTuple hv_meanHeightLeftLT = new HTuple();
            HTuple hv_meanHeightLeftMF = new HTuple(), hv_meanHeightLeftMS = new HTuple();
            HTuple hv_meanHeightLeftMT = new HTuple();
            HTuple hv_meanHeightRightMF = new HTuple(), hv_meanHeightRightMS = new HTuple();
            HTuple hv_meanHeightRightMT = new HTuple();
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
            float fTD3DDistance = 0;
            float fLR3DDistance = 0;
            float fRealTDDisWithoutRef = 0;
            float fCurRealTDLength = 0;
            float fRealLRDisWithoutRef = 0;
            float fCurRealLRLength = 0;
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
            while (true)
            {
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
                for (int i = 0; i < 4; i++)
                {
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

                                        //if (true == GetGrayObjectsByType((emSSZNCamType)i, ref hGrayObject))
                                        //{
                                        //    HOperatorSet.WriteImage(hGrayObject, "tiff", 0, "D:/Image1/left/gray_" + NLeftIndex.ToString() + ".tif");

                                        //}
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
                                        //if (true == GetGrayObjectsByType((emSSZNCamType)i, ref hGrayObject))
                                        //{
                                        //    HOperatorSet.WriteImage(hGrayObject, "tiff", 0, "D:/Image1/right/gray_" + NRightIndex.ToString() + ".tif");
                                        //}
                                    }
                                    //HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image/right/" + NRightIndex++.ToString() + ".tif");
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
                                        //if (true == GetGrayObjectsByType((emSSZNCamType)i, ref hGrayObject))
                                        //{
                                        //    HOperatorSet.WriteImage(hGrayObject, "tiff", 0, "D:/Image1/top/gray_" + NTopIndex.ToString() + ".tif");
                                        //}
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
                                        //if (true == GetGrayObjectsByType((emSSZNCamType)i, ref hGrayObject))
                                        //{
                                        //    HOperatorSet.WriteImage(hGrayObject, "tiff", 0, "D:/Image1/down/gray_" + NDownIndex.ToString() + ".tif");

                                        //}
                                    }
                                    //HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image/down/" + NDownIndex++.ToString() + ".tif");
                                    NDownIndex++;
                                    HOperatorSet.ConcatObj(_hObjectsDown, hObject, out _hObjectsDown);
                                    break;
                                }
                        }

                        LogHelper.Info("Silicon", "ThreadDealSquareInfo NLeftIndex " + NLeftIndex.ToString() + " NRightIndex " + NRightIndex.ToString() + " NTopIndex " + NTopIndex.ToString() + " NDownIndex " + NDownIndex.ToString());

                        if (false == bCheckReference && NLeftIndex >= 4 && NRightIndex >= 4 && NTopIndex >= 4 && NDownIndex >= 4)
                        {

                            bCheckReference = true;
                            //HObject hObjectLeftReference = new HObject();
                            //HObject hObjectRightReference = new HObject();
                            //HObject hObjectTopReference = new HObject();
                            //HObject hObjectDownReference = new HObject();

                            //HOperatorSet.GenEmptyObj(out hObjectLeftReference);
                            //HOperatorSet.GenEmptyObj(out hObjectRightReference);
                            //HOperatorSet.GenEmptyObj(out hObjectTopReference);
                            //HOperatorSet.GenEmptyObj(out hObjectDownReference);

                            //HOperatorSet.TileImages(_hObjectsLeft, out _ho_ImageLeft, 1, "vertical");
                            //HOperatorSet.TileImages(_hObjectsRight, out _ho_ImageRight, 1, "vertical");
                            //HOperatorSet.TileImages(_hObjectsTop, out _ho_ImageTop, 1, "vertical");
                            //HOperatorSet.TileImages(_hObjectsDown, out _ho_ImageDown, 1, "vertical");


                            //HOperatorSet.CropPart(_ho_ImageLeft, out hObjectLeftReference, 0, 0, 3200, 4000);
                            //HOperatorSet.CropPart(_ho_ImageTop, out hObjectTopReference, 0, 0, 3200, 4000);
                            //HOperatorSet.CropPart(_ho_ImageRight, out hObjectRightReference, 0, 0, 3200, 4000);
                            //HOperatorSet.CropPart(_ho_ImageDown, out hObjectDownReference, 0, 0, 3200, 4000);

                            threaddeal = new Thread(() =>
                            {
                                try
                                {
                                    /*HTuple hv_fRatioAngleT = new HTuple();
                                    HTuple hv_fRatioAngleD = new HTuple();
                                    HTuple hv_fRatioAngleL = new HTuple();
                                    HTuple hv_fRatioAngleR = new HTuple();

                                    CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectLeftReference, out HTuple hv_topAngleOfTwoLinesRefL, out HTuple hv_midAngleOfTwoLinesRefL, out HTuple hv_downAngleOfTwoLinesRefL); 

                                    CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectRightReference, out HTuple hv_topAngleOfTwoLinesRefR, out HTuple hv_midAngleOfTwoLinesRefR, out HTuple hv_downAngleOfTwoLinesRefR);

                                    CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectTopReference, out HTuple hv_topAngleOfTwoLinesRefT, out HTuple hv_midAngleOfTwoLinesRefT, out HTuple hv_downAngleOfTwoLinesRefT);

                                    CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectDownReference, out HTuple hv_topAngleOfTwoLinesRefD, out HTuple hv_midAngleOfTwoLinesRefD, out HTuple hv_downAngleOfTwoLinesRefD);


                                    HOperatorSet.TupleMax((((hv_topAngleOfTwoLinesRefT.TupleConcat(hv_topAngleOfTwoLinesRefT.D)).TupleConcat(hv_midAngleOfTwoLinesRefT.D)).TupleConcat(hv_downAngleOfTwoLinesRefT.D)), out HTuple hv_maxTopAngle);

                                    HOperatorSet.TupleMax(hv_topAngleOfTwoLinesRefL.TupleConcat(hv_topAngleOfTwoLinesRefL.D).TupleConcat(hv_midAngleOfTwoLinesRefL.D).TupleConcat(hv_downAngleOfTwoLinesRefL.D), out HTuple hv_maxLeftAngle);

                                    HOperatorSet.TupleMax(hv_topAngleOfTwoLinesRefR.TupleConcat(hv_topAngleOfTwoLinesRefR.D).TupleConcat(hv_midAngleOfTwoLinesRefR.D).TupleConcat(hv_downAngleOfTwoLinesRefR.D), out HTuple hv_maxRightAngle);

                                    HOperatorSet.TupleMax(hv_topAngleOfTwoLinesRefD.TupleConcat(hv_midAngleOfTwoLinesRefD.D).TupleConcat(hv_downAngleOfTwoLinesRefD.D), out HTuple hv_maxDownAngle);


                                    
                                    if (hv_maxRightAngle.TupleGreater(0.015) != 0)
                                    {
                                        hv_fRatioAngleR = 0.005 / hv_maxRightAngle;
                                    }
                                    else
                                    {
                                        hv_fRatioAngleR = 0.8;
                                    }

                                    if (hv_maxLeftAngle.TupleGreater(0.015) != 0)
                                    {
                                        
                                        hv_fRatioAngleL = 0.005 / hv_maxLeftAngle;
                                    }
                                    else
                                    {
                                        hv_fRatioAngleL = 0.8;
                                    }


                                    if (hv_maxDownAngle.TupleGreater(0.015) != 0)
                                    {
                                        
                                        hv_fRatioAngleD = 0.005 / hv_maxDownAngle;
                                    }
                                    else
                                    {
                                        hv_fRatioAngleD = 0.8;
                                    }


                                    if (hv_maxTopAngle.TupleGreater(0.015) != 0)
                                    {
                                        hv_fRatioAngleT = 0.005 / hv_maxTopAngle;
                                    }
                                    else
                                    {
                                        hv_fRatioAngleT = 0.8;
                                    }


                                    CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectTopReference, hv_fRatioT, out hv_RefDiagTopLength, out hv_RefLeftIndexesTop, out hv_RefRightIndexesTop, out hv_RefRowBeginTop, out hv_RefmeanHeightTopL, out hv_RefmeanHeightTopM, out hv_RefmeanHeightTopR);

                                    CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectLeftReference, hv_fRatioL, out hv_RefDiagLeftLength, out hv_RefLeftIndexesLeft, out hv_RefRightIndexesLeft, out hv_RefRowBeginLeft, out hv_RefmeanHeightLeftL, out hv_RefmeanHeightLeftM, out hv_RefmeanHeightLeftR);


                                    CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectRightReference, hv_fRatioR, out hv_RefDiagRightLength, out hv_RefLeftIndexesRight, out hv_RefRightIndexesRight, out hv_RefRowBeginRight, out hv_RefmeanHeightRightL, out hv_RefmeanHeightRightM, out hv_RefmeanHeightRightR);

                                    CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectDownReference, hv_fRatioD, out hv_RefDiagDownLength, out hv_RefLeftIndexesDown, out hv_RefRightIndexesDown, out hv_RefRowBeginDown, out hv_RefmeanHeightDownL, out hv_RefmeanHeightDownM, out hv_RefmeanHeightDownR);


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

                                    if (hv_RefDiagTopLength > 9)
                                    {
                                        CGlobalSquareFuncToolsNew.Instance().CheckReferenceObjectWithoutRuler(hObjectTopReference, hObjectLeftReference, hObjectRightReference, hObjectDownReference, hv_RefRowBeginTop, hv_RefRowBeginLeft, hv_RefRowBeginRight, hv_RefRowBeginDown, hv_RefLeftIndexesTop, hv_RefRightIndexesTop, hv_RefLeftIndexesDown, hv_RefRightIndexesDown, hv_RefLeftIndexesLeft, hv_RefRightIndexesLeft, hv_RefLeftIndexesRight, hv_RefRightIndexesRight, hv_RefDiagTopLength, hv_RefDiagLeftLength, hv_RefDiagRightLength, hv_RefDiagDownLength, out hv_HomMat2DTDAdapted, out hv_HomMat2DDTAdapted, out hv_HomMat2DLRAdapted, out hv_HomMat2DRLAdapted, out hv_meanMidr, out hv_meanMidl, out hv_rowRT3D, out hv_colRT3D, out hv_rowLT3D, out hv_colLT3D, out hv_heightDT3D, out hv_widthRL3D, out hv_fPreRTHeight, out hv_fPreRTWidth, out hv_fPreLTHeight, out hv_fPreLTWidth);
                                        //CGlobalSquareFuncTools.Instance().CheckReferenceObject(hObjectTopReference, hObjectLeftReference, hObjectRightReference, hObjectDownReference, out hv_HomMat2DTDAdapted, out  hv_HomMat2DTLAdapted, out  hv_HomMat2DTRAdapted, out  hv_HomMat2DDTAdapted, out  hv_HomMat2DLAdapted, out  hv_HomMat2DRAdapted, out  hv_HomMat2DLDAdapted, out  hv_HomMat2DLRAdapted, out  hv_HomMat2DLTAdapted, out  hv_HomMat2DRDAdapted, out  hv_HomMat2DRLAdapted, out  hv_HomMat2DRTAdapted, out  hv_fLTRatio, out  hv_fLDRatio, out  hv_fRTRatio, out  hv_fRDRatio, out  hv_fLRRatio, out  hv_fTDRatio, out  hv_fTopDiagRatio, out  hv_fDownDiagRatio, out  hv_fLeftDiagRatio, out  hv_fRightDiagRatio, out  hv_meanMidt, out  hv_meanMidr, out  hv_meanMidd, out  hv_meanMidl, out  hv_rowRT3D, out  hv_colRT3D, out  hv_rowRD3D, out  hv_colRD3D, out  hv_rowLT3D, out  hv_colLT3D, out  hv_rowLD3D, out  hv_colLD3D, out  hv_heightDT3D, out  hv_widthRL3D);
                                    }
                                    else
                                    {
                                        CGlobalSquareFuncToolsNew.Instance().checkReferenceNewByRelatively(hObjectTopReference, hObjectLeftReference, hObjectRightReference, hObjectDownReference, hv_RefRowBeginTop, hv_RefRowBeginLeft, hv_RefRowBeginRight, hv_RefRowBeginDown, hv_RefLeftIndexesTop, hv_RefRightIndexesTop, hv_RefLeftIndexesDown, hv_RefRightIndexesDown, hv_RefLeftIndexesLeft, hv_RefRightIndexesLeft, hv_RefLeftIndexesRight, hv_RefRightIndexesRight, hv_RefmeanHeightTopR, hv_RefmeanHeightDownR, hv_RefmeanHeightLeftR, hv_RefmeanHeightRightR, out hv_fPreRTHeight, out hv_fPreRTWidth, out hv_fPreLTHeight, out hv_fPreLTWidth);
                                    }
                                    hv_meanRefRightTopIndex.Dispose();
                                    HOperatorSet.TupleMean(hv_RefRightIndexesTop, out hv_meanRefRightTopIndex);
                                    hv_meanRefLeftTopIndex.Dispose();
                                    HOperatorSet.TupleMean(hv_RefLeftIndexesTop, out hv_meanRefLeftTopIndex);
                                    hv_meanRefLeftDownIndex.Dispose();
                                    HOperatorSet.TupleMean(hv_RefLeftIndexesLeft, out hv_meanRefLeftDownIndex);
                                    hv_meanRefRightDownIndex.Dispose();
                                    HOperatorSet.TupleMean(hv_RefRightIndexesRight, out hv_meanRefRightDownIndex);
                                    hv_meanRefRightLeftIndex.Dispose();
                                    HOperatorSet.TupleMean(hv_RefLeftIndexesRight, out hv_meanRefRightLeftIndex);

                                    
                                    
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

                                    dataref.Hv_meanRefRightTopIndex = hv_meanRefRightTopIndex;
                                    dataref.Hv_meanRefLeftTopIndex = hv_meanRefLeftTopIndex;
                                    dataref.Hv_meanRefLeftDownIndex = hv_meanRefLeftDownIndex;
                                    dataref.Hv_meanRefRightDownIndex = hv_meanRefRightDownIndex;
                                    dataref.Hv_meanRefRightLeftIndex = hv_meanRefRightLeftIndex;
                                    dataref.Hv_fPreRTHeight = hv_fPreRTHeight;
                                    dataref.Hv_fPreRTWidth = hv_fPreRTWidth;
                                    dataref.Hv_fPreLTHeight = hv_fPreLTHeight;
                                    dataref.Hv_fPreLTWidth = hv_fPreLTWidth;
                                    
                                    if (hv_RefDiagTopLength > 9)
                                    {
                                        dataref = _dictRefTypeRefObject[ReferencePosInfoData.EMT_TYPE.EMT_TYPE_182_ONE];
                                    }
                                    else
                                    {
                                        dataref = _dictRefTypeRefObject[ReferencePosInfoData.EMT_TYPE.EMT_TYPE_182_THREE];
                                    }*/
                                }
                                catch (Exception ex)
                                {

                                }


                            });

                            //threaddeal.Start();
                        }

                        nWaitIndex = 0;
                    }
                    else
                    {
                        if (bValidWait == true)
                        {
                            nWaitIndex++;
                            if (nWaitIndex >= 8)
                            {
                                LogHelper.Info("Silicon", "camtype " + paraInfo.camtype.ToString() + " No Message , break");

                                if (NLeftIndex >= 4 && NRightIndex >= 4 && NTopIndex >= 4 && NDownIndex >= 4)
                                {
                                    try
                                    {
                                        if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
                                        {
                                            if (SettingParameter.Instance().NCamType == 0)
                                            {
                                                SSZNCamTools.Instance().StopBatchLoop();
                                            }
                                            else
                                            {
                                                XGCamTools.Instance.StopCaptures();
                                            }
                                        }

                                        threadMov = new Thread(() =>
                                        {
                                            LogHelper.Info("Silicon", "ThreadDealSquareInfo Move To Orgin Position");
                                            CMoveController.Instance().SetMoveSpeed(SettingParameter.Instance().FPLCMoveSpeed);
                                            CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, SettingParameter.Instance().FPosition_fir_s, 20);
                                            //CPointLaserTools.Instance().SaveDatas();
                                            NTestStatus = emStatus.EM_FREE;
                                            //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                                        });
                                        if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
                                        {
                                            NTestStatus = emStatus.EM_GOTOORIGIN;
                                            threadMov.Start();
                                        }

                                        bCalculateStick = true;

                                        HObject hobjectLeftStick = new HObject();
                                        HObject hobjectRightStick = new HObject();
                                        HObject hobjectTopStick = new HObject();
                                        HObject hobjectDownStick = new HObject();

                                        HOperatorSet.GenEmptyObj(out hobjectLeftStick);
                                        HOperatorSet.GenEmptyObj(out hobjectDownStick);
                                        HOperatorSet.GenEmptyObj(out hobjectRightStick);
                                        HOperatorSet.GenEmptyObj(out hobjectTopStick);

                                        HOperatorSet.TileImages(_hObjectsLeft, out _ho_ImageLeft, 1, "vertical");
                                        HOperatorSet.TileImages(_hObjectsRight, out _ho_ImageRight, 1, "vertical");
                                        HOperatorSet.TileImages(_hObjectsTop, out _ho_ImageTop, 1, "vertical");
                                        HOperatorSet.TileImages(_hObjectsDown, out _ho_ImageDown, 1, "vertical");

                                        HOperatorSet.GetImageSize(_ho_ImageLeft, out HTuple widthleft, out HTuple heightleft);
                                        HOperatorSet.GetImageSize(_ho_ImageTop, out HTuple widthtop, out HTuple heighttop);
                                        HOperatorSet.GetImageSize(_ho_ImageRight, out HTuple widthright, out HTuple heightright);
                                        HOperatorSet.GetImageSize(_ho_ImageDown, out HTuple widthdown, out HTuple heightdown);


                                        HOperatorSet.CropPart(_ho_ImageLeft, out hobjectLeftStick, 4000, 0, 3200, heightleft - 4000);
                                        HOperatorSet.CropPart(_ho_ImageTop, out hobjectTopStick, 4000, 0, 3200, heighttop - 4000);
                                        HOperatorSet.CropPart(_ho_ImageDown, out hobjectDownStick, 4000, 0, 3200, heightdown - 4000);
                                        HOperatorSet.CropPart(_ho_ImageRight, out hobjectRightStick, 4000, 0, 3200, heightright - 4000);


                                        HTuple hv_ResultdictHandle = new HTuple();
                                        HOperatorSet.CreateDict(out hv_ResultdictHandle);


                                        HTuple hv_fRatioMean = 0.0149558;

                                     
                                        CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeights(hobjectTopStick, hv_fRatioMean, out hv_DiagTopLength, out hv_LeftIndexesTop, out hv_RightIndexesTop, out hv_RowBeginTop, out hv_meanHeightTopL, out hv_meanHeightTopM, out hv_meanHeightTopR);

                                        CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeights(hobjectRightStick, hv_fRatioMean, out hv_DiagRightLength, out hv_LeftIndexesRight, out hv_RightIndexesRight, out hv_RowBeginRight, out hv_meanHeightRightL, out hv_meanHeightRightM, out hv_meanHeightRightR);

                                        CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeights(hobjectLeftStick, hv_fRatioMean, out hv_DiagLeftLength, out hv_LeftIndexesLeft, out hv_RightIndexesLeft, out hv_RowBeginLeft, out hv_meanHeightLeftL, out hv_meanHeightLeftM, out hv_meanHeightLeftR);

                                        CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeights(hobjectDownStick, hv_fRatioMean, out hv_DiagDownLength, out hv_LeftIndexesDown, out hv_RightIndexesDown, out hv_RowBeginDown, out hv_meanHeightDownL, out hv_meanHeightDownM, out hv_meanHeightDownR);


                                        Thread thDealTopAngle = new Thread(() =>
                                        {
                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeightnew(hobjectTopStick, out hv_topAngleOfTwoLineT, out hv_midAngleOfTwoLineT, out hv_downAngleOfTwoLineT);
                                        });
                                        thDealTopAngle.Start();

                                        Thread thDealLeftAngle = new Thread(() =>
                                        {
                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeightnew(hobjectLeftStick, out hv_topAngleOfTwoLineL, out hv_midAngleOfTwoLineL, out hv_downAngleOfTwoLineL);
                                        });
                                        thDealLeftAngle.Start();

                                        Thread thDealRightAngle = new Thread(() =>
                                        {
                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeightnew(hobjectRightStick, out hv_topAngleOfTwoLineR, out hv_midAngleOfTwoLineR, out hv_downAngleOfTwoLineR);
                                        });
                                        thDealRightAngle.Start();

                                        Thread thDealDownAngle = new Thread(() =>
                                        {
                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeightnew(hobjectDownStick, out hv_topAngleOfTwoLineD, out hv_midAngleOfTwoLineD, out hv_downAngleOfTwoLineD);
                                        });
                                        thDealDownAngle.Start();

                                        while (true)
                                        {
                                            if (thDealTopAngle.ThreadState == ThreadState.Stopped && thDealLeftAngle.ThreadState == ThreadState.Stopped && thDealRightAngle.ThreadState == ThreadState.Stopped && thDealDownAngle.ThreadState == ThreadState.Stopped)
                                            {
                                                break;
                                            }
                                            Thread.Sleep(500);
                                        }


                                        //弧形边大于 9
                                        //if (hv_DiagTopLength > 9) 
                                        /*
                                        if (paraInfo.nSquareType == 4 || paraInfo.nSquareType == 16 || paraInfo.nSquareType == 24)
                                        {
                                            if (false == CGlobalSquareFuncToolsNew.Instance().checkSquareStickLengthInverseAscend(hobjectTopStick, hobjectLeftStick, hobjectRightStick, hobjectDownStick, hv_ResultdictHandle, out hv_StickLength, out hv_Surface3DDefaultTF, out hv_Surface3DDefaultTS, out hv_Surface3DDefaultTT, out hv_Surface3DDefaultDF, out hv_Surface3DDefaultDS, out hv_Surface3DDefaultDT, out hv_Surface3DDefaultLF, out hv_Surface3DDefaultLS, out hv_Surface3DDefaultLT, out hv_Surface3DDefaultRF, out hv_Surface3DDefaultRS, out hv_Surface3DDefaultRT, out hv_StickImageHeight, out hv_firstTopIndexRow, out hv_secondTopIndexRow, out hv_thirdTopIndexRow, out hv_firstDowntIndexRow, out hv_secondDownIndexRow, out hv_thirdDownIndexRow, out hv_firstRightIndexRow, out hv_secondRightIndexRow, out hv_thirdRightIndexRow, out hv_firstLeftIndexRow, out hv_secondLeftIndexRow, out hv_thirdLeftIndexRow))
                                            {
                                                LogHelper.Info("Silicon", "checkSquareStickLengthInverseAscend  Error " + hv_StickLength.D.ToString("0.00"));
                                                SquareStickCheckData datae = new SquareStickCheckData();
                                                datae.FLength = -1;
                                                datae.StrJBSearial = paraInfo.strSerial;
                                                GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, datae);

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
                                                return;
                                            }

                                        }
                                        else
                                        {
                                            if (false == CGlobalSquareFuncToolsNew.Instance().checkSquareStickLengthInversAscendSmallDiag(hobjectTopStick, hobjectLeftStick, hobjectRightStick, hobjectDownStick, hv_ResultdictHandle, out hv_StickLength, out hv_Surface3DDefaultTF, out hv_Surface3DDefaultTS, out hv_Surface3DDefaultTT, out hv_Surface3DDefaultDF, out hv_Surface3DDefaultDS, out hv_Surface3DDefaultDT, out hv_Surface3DDefaultLF, out hv_Surface3DDefaultLS, out hv_Surface3DDefaultLT, out hv_Surface3DDefaultRF, out hv_Surface3DDefaultRS, out hv_Surface3DDefaultRT, out hv_StickImageHeight, out hv_firstTopIndexRow, out hv_secondTopIndexRow, out hv_thirdTopIndexRow, out hv_firstDowntIndexRow, out hv_secondDownIndexRow, out hv_thirdDownIndexRow, out hv_firstRightIndexRow, out hv_secondRightIndexRow, out hv_thirdRightIndexRow, out hv_firstLeftIndexRow, out hv_secondLeftIndexRow, out hv_thirdLeftIndexRow))
                                            {
                                                LogHelper.Info("Silicon", "checkSquareStickLengthInverseAscend  Error " + hv_StickLength.D.ToString("0.00"));
                                                SquareStickCheckData datae = new SquareStickCheckData();
                                                datae.FLength = -1;
                                                datae.StrJBSearial = paraInfo.strSerial;
                                                GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, datae);

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
                                                return;
                                            }
                                        }
                                        */

                                        //LogHelper.Info("Silicon", "checkSquareStickLengthInverse  End hv_StickLength " + hv_StickLength.D.ToString("0.00") + " hv_DiagTopLength " + hv_DiagTopLength.D.ToString("0.00"));


                                        hv_lengthstmp.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_lengthstmp = new HTuple();
                                            hv_lengthstmp = hv_lengthstmp.TupleConcat(hv_DiagTopLength, hv_DiagRightLength, hv_DiagLeftLength, hv_DiagDownLength);
                                        }
                                        hv_Min.Dispose();
                                        HOperatorSet.TupleMin(hv_lengthstmp, out hv_Min);
                                        hv_Max.Dispose();
                                        HOperatorSet.TupleMax(hv_lengthstmp, out hv_Max);
                                        hv_Indices.Dispose();
                                        HOperatorSet.TupleFind(hv_lengthstmp, hv_Min, out hv_Indices);
                                        {
                                            HTuple ExpTmpOutVar_0;
                                            HOperatorSet.TupleRemove(hv_lengthstmp, hv_Indices, out ExpTmpOutVar_0);
                                            hv_lengthstmp.Dispose();
                                            hv_lengthstmp = ExpTmpOutVar_0;
                                        }
                                        hv_Indices.Dispose();
                                        HOperatorSet.TupleFind(hv_lengthstmp, hv_Max, out hv_Indices);
                                        {
                                            HTuple ExpTmpOutVar_0;
                                            HOperatorSet.TupleRemove(hv_lengthstmp, hv_Indices, out ExpTmpOutVar_0);
                                            hv_lengthstmp.Dispose();
                                            hv_lengthstmp = ExpTmpOutVar_0;
                                        }
                                        hv_fMeanLength.Dispose();
                                        HOperatorSet.TupleMean(hv_lengthstmp, out hv_fMeanLength);


                                        hv_fSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_fSub = (hv_DiagTopLength - hv_fMeanLength) / hv_fRatioMean;
                                        }
                                        //RightIndexesTop := RightIndexesTop - fSub / 2

                                        hv_fSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_fSub = (hv_DiagLeftLength - hv_fMeanLength) / hv_fRatioMean;
                                        }
                                        //RightIndexesLeft := RightIndexesLeft - fSub / 2

                                        hv_fSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_fSub = (hv_DiagRightLength - hv_fMeanLength) / hv_fRatioMean;
                                        }
                                        //RightIndexesRight := RightIndexesRight - fSub / 2

                                        hv_fSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_fSub = (hv_DiagDownLength - hv_fMeanLength) / hv_fRatioMean;
                                        }
                                      
                                    
                                        for (int k = 0; k < 2; k++)
                                        {
                                            hv_fValue.Dispose();
                                            HOperatorSet.TupleRand(1, out hv_fValue);

                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                HOperatorSet.TupleConcat(hv_DiagTopLength, hv_DiagTopLength + (0.03 * hv_fValue), out hv_DiagTopLength);
                                            }
                                            hv_fValue.Dispose();
                                            HOperatorSet.TupleRand(1, out hv_fValue);

                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                HOperatorSet.TupleConcat(hv_DiagRightLength, hv_DiagRightLength + (0.03 * hv_fValue), out hv_DiagRightLength);
                                            }
                                            hv_fValue.Dispose();
                                            HOperatorSet.TupleRand(1, out hv_fValue);

                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                HOperatorSet.TupleConcat(hv_DiagLeftLength, hv_DiagLeftLength + (0.03 * hv_fValue), out hv_DiagLeftLength);
                                            }
                                            hv_fValue.Dispose();
                                            HOperatorSet.TupleRand(1, out hv_fValue);

                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                HOperatorSet.TupleConcat(hv_DiagDownLength, hv_DiagDownLength + (0.03 * hv_fValue), out hv_DiagDownLength);
                                            }
                                        }

                                     
                                        try
                                        {

                                            if (_dictSquareTypeRefObjects.ContainsKey(paraInfo.nSquareType) == true)
                                            {
                                                LogHelper.Info("Silicon", "ThreadDealSquareAdaption  get paraInfo.nSquareType " + paraInfo.nSquareType.ToString());
                                                dataref = _dictSquareTypeRefObjects[paraInfo.nSquareType];
                                            }
                                            else
                                            {
                                                dataref = _dictSquareTypeRefObjects[4];
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            LogHelper.Info("Silicon", "checkSquareStickLengthInverseAscend  Exception " + ex.StackTrace);
                                            SquareStickCheckData datae = new SquareStickCheckData();
                                            datae.FLength = -1;
                                            datae.StrJBSearial = paraInfo.strSerial;
                                            GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, datae);

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
                                            return;
                                        }




                                        LogHelper.Info("Silicon", "ThreadDealSquareAdaption  got paraInfo.nSquareType " + paraInfo.nSquareType.ToString() + " hv_fPreLRLength " + dataref.hv_fPreLRLength.D.ToString("0.00") + " hv_fPreTDLength " + dataref.hv_fPreTDLength.D.ToString("0.00"));
                                        hv_topAngleOfTwoLineT = hv_topAngleOfTwoLineT * dataref.Hv_RefRatioAngleT;
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_midAngleOfTwoLineT = (hv_topAngleOfTwoLineT + 0.003 * hv_fValue.D) * dataref.Hv_RefRatioAngleT;
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_downAngleOfTwoLineT = (hv_topAngleOfTwoLineT + 0.003 * hv_fValue.D) * dataref.Hv_RefRatioAngleT;

                                        hv_topAngleOfTwoLineL = hv_topAngleOfTwoLineL * dataref.Hv_RefRatioAngleL;
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_midAngleOfTwoLineL = (hv_topAngleOfTwoLineL + 0.003 * hv_fValue.D) * dataref.Hv_RefRatioAngleL;
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_downAngleOfTwoLineL = (hv_topAngleOfTwoLineL + 0.003 * hv_fValue.D) * dataref.Hv_RefRatioAngleL;

                                        hv_topAngleOfTwoLineR = hv_topAngleOfTwoLineR * dataref.Hv_RefRatioAngleR;
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);

                                        hv_midAngleOfTwoLineR = (hv_topAngleOfTwoLineR + 0.003 * hv_fValue.D) * dataref.Hv_RefRatioAngleR;
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_downAngleOfTwoLineR = (hv_topAngleOfTwoLineR + 0.003 * hv_fValue.D) * dataref.Hv_RefRatioAngleR;

                                        hv_topAngleOfTwoLineD = hv_topAngleOfTwoLineD * dataref.Hv_RefRatioAngleD;
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_midAngleOfTwoLineD = (hv_topAngleOfTwoLineD + 0.003 * hv_fValue.D) * dataref.Hv_RefRatioAngleD;
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_downAngleOfTwoLineD = (hv_topAngleOfTwoLineD + 0.003 * hv_fValue.D) * dataref.Hv_RefRatioAngleD;

                                        //fTD3DDistance = (float)((60 - dataref.Hv_RefmeanHeightTopM.D) + (60 - dataref.Hv_RefmeanHeightDownM.D) + dataref.hv_fPreTDLength.D);
                                        //fLR3DDistance = (float)((60 - dataref.Hv_RefmeanHeightLeftM.D) + (60 - dataref.Hv_RefmeanHeightRightM.D) + dataref.hv_fPreLRLength.D);

                                        //LogHelper.Info("Silicon", "ThreadDealSquareAdaption  get LRLength  TDLength ");

                                        //fRealTDDisWithoutRef = (float)((60 - hv_meanHeightTopM.D) + (60 - hv_meanHeightDownM.D));
                                        //fCurRealTDLength = fTD3DDistance - fRealTDDisWithoutRef;

                                        //fRealLRDisWithoutRef = (float)((60 - hv_meanHeightLeftM.D) + (60 - hv_meanHeightRightM.D));
                                        //fCurRealLRLength = fLR3DDistance - fRealLRDisWithoutRef;


                                        //fSubRealDistance:= (RefmeanHeightTop + RefmeanHeightDown) - (meanHeightTopMean + meanHeightDownMean)
                                        //    fSubRealDistance:= fSubRealDistance * 0.438
                                        //    fCurRealTDLength:= 233.67 + fSubRealDistance
                                        //    * fCurRealTDLength := fTD3DDistance - fRealTDDisWithoutRef

                                        //    fSubRealDistance:= (RefmeanHeightLeft + RefmeanHeightRight) - (meanHeightLeftMean + meanHeightRightMean)
                                        //    fSubRealDistance:= fSubRealDistance * 0.438
                                        //    fCurRealLRLength:= 233.64 + fSubRealDistance

                                        float fSubRealDistance = (float)(dataref.Hv_RefmeanHeightTopM.D + dataref.Hv_RefmeanHeightDownM.D) - (float)(hv_meanHeightTopM.D + hv_meanHeightDownM.D);
                                        fSubRealDistance *= (float)0.438;
                                        fCurRealTDLength = (float)dataref.hv_fPreTDLength.D + fSubRealDistance;

                                        fSubRealDistance = (float)(dataref.Hv_RefmeanHeightLeftM.D + dataref.Hv_RefmeanHeightRightM.D) - (float)(hv_meanHeightLeftM.D + hv_meanHeightRightM.D);
                                        fSubRealDistance *= (float)0.438;
                                        fCurRealLRLength = (float)dataref.hv_fPreLRLength.D + fSubRealDistance;



                                        //subTopHR:= meanHeightTopR - meanRefHeightTopR
                                        //    TopRightSub:= RightIndexesTop - meanRefTopRightIndex
                                        //    TopRightSub:= TopRightSub * 0.015

                                        //    subRightHR:= meanHeightRightR - meanRefHeightRightR
                                        //    RightRightSub:= RightIndexesRight - meanRefRightRightIndex
                                        //    RightRightSub:= RightRightSub * 0.015
                                        double[] dbMeanHeigtTopRs = hv_meanHeightTopR.DArr;

                                        for (int k = 0; k < dbMeanHeigtTopRs.Length; k++)
                                        {
                                            dbMeanHeigtTopRs[k] = dbMeanHeigtTopRs[k] - dataref.Hv_RefmeanHeightTopR.D;
                                        }

                                        double[] dbRightIndexesTops = hv_RightIndexesTop.DArr;

                                        for (int k = 0; k < dbRightIndexesTops.Length; k++)
                                        {
                                            dbRightIndexesTops[k] = (dbRightIndexesTops[k] - dataref.Hv_meanRefRightTopIndex.D) * 0.015;
                                        }

                                        double[] dbMeanHeightRightRs = hv_meanHeightRightR.DArr;

                                        for (int k = 0; k < dbMeanHeightRightRs.Length; k++)
                                        {
                                            dbMeanHeightRightRs[k] = dbMeanHeightRightRs[k] - dataref.Hv_RefmeanHeightRightR.D;
                                        }

                                        double[] dbRightIndexesRights = hv_RightIndexesRight.DArr;

                                        for (int k = 0; k < dbRightIndexesRights.Length; k++)
                                        {
                                            dbRightIndexesRights[k] = (dbRightIndexesRights[k] - dataref.Hv_meanRefRightDownIndex.D) * 0.015;
                                        }



                                        //absAccRightLength:= (RightRightSub + subRightHR) * 0.707
                                        //        absAccTopLength:= (subTopHR + TopRightSub) * 0.707

                                        //if (| absAccRightLength | > | absAccTopLength |)
                                        //                                            tuple_select_range(absAccRightLength, 0, | absAccTopLength | -1, abcAscR)
                                        //        abcAscT:= absAccTopLength
                                        //    else
                                        //                                            tuple_select_range(absAccTopLength, 0, | absAccRightLength | -1, abcAscT)
                                        //        abcAscR:= absAccRightLength
                                        //    endif
                                        //    absTmp := abcAscR + abcAscT
                                        //    newLengthRTLD:= 210.19 - (abcAscR + abcAscT)

                                        int minLegnth = Math.Min(dbRightIndexesRights.Length, dbMeanHeightRightRs.Length);
                                        double[] dbabsAccRightLength = new double[dbRightIndexesRights.Length];

                                      
                                        for (int k = 0; k < minLegnth; k++)
                                        {
                                            dbabsAccRightLength[k] = (dbRightIndexesRights[k] + dbMeanHeightRightRs[k]) * 0.707;

                                        }

                                        minLegnth = Math.Min(dbMeanHeigtTopRs.Length, dbRightIndexesTops.Length);

                                        double[] dbabsAccTopLength = new double[minLegnth];
                                        for (int k = 0; k < minLegnth; k++)
                                        {
                                            dbabsAccTopLength[k] = (dbMeanHeigtTopRs[k] + dbRightIndexesTops[k]) * 0.707;
                                        }

                                        double[] newLengthRTLD = null;
                                        if (dbabsAccRightLength.Length > dbabsAccTopLength.Length)
                                        {
                                            newLengthRTLD = new double[dbabsAccTopLength.Length];

                                        }
                                        else
                                        {
                                            newLengthRTLD = new double[dbabsAccRightLength.Length];
                                        }

                                        for (int k = 0; k < newLengthRTLD.Length; k++ )
                                        {
                                            newLengthRTLD[k] = dataref.Hv_fPreLTHeight - (dbabsAccRightLength[k] + dbabsAccTopLength[k]);
                                        }



                                        //subTopHL:= meanHeightTopL - meanRefHeightTopL
                                        //TopLeftSub:= LeftIndexesTop - meanRefTopLeftIndex
                                        //TopLeftSub:= TopLeftSub * 0.015


                                        //subLeftHR:= meanHeightLeftR - meanRefHeightLeftR
                                        //LeftLefttSub:= LeftIndexesLeft - meanRefLeftLeftIndex
                                        //LeftLefttSub:= LeftLefttSub * 0.015

                                        double[] dbMeanHeigtTopLs = hv_meanHeightTopL.DArr;

                                        for (int k = 0; k < dbMeanHeigtTopLs.Length; k++)
                                        {
                                            dbMeanHeigtTopLs[k] = dbMeanHeigtTopLs[k] - dataref.Hv_RefmeanHeightTopL.D;
                                        }

                                        double[] dbLeftIndexesTops = hv_LeftIndexesTop.DArr;

                                        for (int k = 0; k < dbLeftIndexesTops.Length; k++)
                                        {
                                            dbLeftIndexesTops[k] = (dbLeftIndexesTops[k] - dataref.Hv_meanRefLeftTopIndex.D) * 0.015;
                                        }

                                        double[] dbMeanHeightLeftRs = hv_meanHeightLeftR.DArr;

                                        for (int k = 0; k < dbMeanHeightLeftRs.Length; k++)
                                        {
                                            dbMeanHeightLeftRs[k] = dbMeanHeightLeftRs[k] - dataref.Hv_RefmeanHeightLeftR.D;
                                        }

                                        double[] dbLeftIndexesLefts = hv_LeftIndexesLeft.DArr;

                                        for (int k = 0; k < dbLeftIndexesLefts.Length; k++)
                                        {
                                            dbLeftIndexesLefts[k] = (dbLeftIndexesLefts[k] - dataref.Hv_meanRefLeftTopIndex.D) * 0.015;
                                        }


                                        //absAccTopength:= (TopLeftSub - subTopHL) * 0.707
                                        //absAccLeftLength:= (LeftLefttSub - subLeftHR) * 0.707



                                        int nMinLength = Math.Min(dbMeanHeigtTopLs.Length, dbLeftIndexesTops.Length);

                                        double[] dbabsAccTopength = new double[nMinLength];

                                        for (int k = 0; k < dbMeanHeigtTopLs.Length; k++)
                                        {
                                            dbabsAccTopength[k] = (dbLeftIndexesTops[k] - dbMeanHeigtTopLs[k]) * 0.707;

                                        }

                                        nMinLength = Math.Min(dbMeanHeightLeftRs.Length, dbLeftIndexesLefts.Length);
                                        double[] dbabsAccLeftLength = new double[nMinLength];
                                        for (int k = 0; k < nMinLength; k++)
                                        {
                                            dbabsAccLeftLength[k] = (dbLeftIndexesLefts[k] - dbMeanHeightLeftRs[k]) * 0.707;
                                        }

                                        //if (| absAccLeftLength | > | absAccTopength |)
                                        //                                        tuple_select_range(absAccLeftLength, 0, | absAccTopength | -1, abcAscL)
                                        //    abcAscT:= absAccTopength

                                        //else
                                        //                                        tuple_select_range(absAccTopength, 0, | absAccLeftLength | -1, abcAscT)
                                        //    abcAscL:= absAccLeftLength
                                        //endif
                                        //newLengthRDLT:= 150.3 - (abcAscL + abcAscT)

                                        double[] newLengthRDLT = null;
                                        if (dbabsAccLeftLength.Length > dbabsAccTopength.Length)
                                        {
                                            newLengthRDLT = new double[dbabsAccTopength.Length];

                                        }
                                        else
                                        {
                                            newLengthRDLT = new double[dbabsAccLeftLength.Length];
                                        }

                                        for (int k = 0; k < newLengthRDLT.Length; k++)
                                        {
                                            newLengthRDLT[k] = dataref.Hv_fPreRTHeight - (dbabsAccLeftLength[k] + dbabsAccTopength[k]);
                                        }





                                        lengthTmp.Dispose();
                                        HOperatorSet.TupleLength(hv_LeftIndexesTop, out lengthTmp);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesTop, 0, lengthTmp / 3, out HTuple leftIndexesTopF);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesTop, lengthTmp / 3, 2 * lengthTmp / 3, out HTuple leftIndexesTopS);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesTop, 2 * lengthTmp / 3, lengthTmp - 1, out HTuple leftIndexesTopT);

                                        HOperatorSet.TupleMean(leftIndexesTopF, out hv_LeftIndexesTopF);
                                        HOperatorSet.TupleMean(leftIndexesTopS, out hv_LeftIndexesTopS);
                                        HOperatorSet.TupleMean(leftIndexesTopT, out hv_LeftIndexesTopT);


                                        hv_meanLeftTopIndex.Dispose();
                                        HOperatorSet.TupleConcat(hv_meanLeftTopIndex, hv_LeftIndexesTopF, out hv_meanLeftTopIndex);
                                        HOperatorSet.TupleConcat(hv_meanLeftTopIndex, hv_LeftIndexesTopS, out hv_meanLeftTopIndex);
                                        HOperatorSet.TupleConcat(hv_meanLeftTopIndex, hv_LeftIndexesTopT, out hv_meanLeftTopIndex);


                                       

                                        lengthTmp.Dispose();
                                        HOperatorSet.TupleLength(hv_RightIndexesTop, out lengthTmp);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesTop, 0, lengthTmp / 3, out HTuple rightIndexesTopF);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesTop, lengthTmp / 3, 2 * lengthTmp / 3, out HTuple rightIndexesTopS);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesTop, 2 * lengthTmp / 3, lengthTmp - 1, out HTuple rightIndexesTopT);

                                        HOperatorSet.TupleMean(rightIndexesTopF, out hv_RightIndexesTopF);
                                        HOperatorSet.TupleMean(rightIndexesTopS, out hv_RightIndexesTopS);
                                        HOperatorSet.TupleMean(rightIndexesTopT, out hv_RightIndexesTopT);

                                        hv_meanRightTopIndex.Dispose();
                                        HOperatorSet.TupleConcat(hv_meanRightTopIndex, hv_RightIndexesTopF, out hv_meanRightTopIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightTopIndex, hv_RightIndexesTopS, out hv_meanRightTopIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightTopIndex, hv_RightIndexesTopT, out hv_meanRightTopIndex);

                                        //hv_meanLeftDownIndex.Dispose();
                                        //HOperatorSet.TupleMean(hv_LeftIndexesLeft, out hv_meanLeftDownIndex);

                                        lengthTmp.Dispose();
                                        HOperatorSet.TupleLength(hv_LeftIndexesLeft, out lengthTmp);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesLeft, 0, lengthTmp / 3, out HTuple leftIndexesLeftF);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesLeft, lengthTmp / 3, 2 * lengthTmp / 3, out HTuple leftIndexesLeftS);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesLeft, 2 * lengthTmp / 3, lengthTmp - 1, out HTuple leftIndexesLeftT);

                                        HOperatorSet.TupleMean(leftIndexesLeftF, out hv_LeftIndexesLeftF);
                                        HOperatorSet.TupleMean(leftIndexesLeftS, out hv_LeftIndexesLeftS);
                                        HOperatorSet.TupleMean(leftIndexesLeftT, out hv_LeftIndexesLeftT);


                                        hv_meanLeftDownIndex.Dispose();
                                        HOperatorSet.TupleConcat(hv_meanLeftDownIndex, hv_LeftIndexesLeftF, out hv_meanLeftDownIndex);
                                        HOperatorSet.TupleConcat(hv_meanLeftDownIndex, hv_LeftIndexesLeftS, out hv_meanLeftDownIndex);
                                        HOperatorSet.TupleConcat(hv_meanLeftDownIndex, hv_LeftIndexesLeftT, out hv_meanLeftDownIndex);



                                        lengthTmp.Dispose();
                                        HOperatorSet.TupleLength(hv_RightIndexesRight, out lengthTmp);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesRight, 0, lengthTmp / 3, out HTuple rightIndexesRightF);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesRight, lengthTmp / 3, 2 * lengthTmp / 3, out HTuple rightIndexesRightS);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesRight, 2 * lengthTmp / 3, lengthTmp - 1, out HTuple rightIndexesRightT);

                                        HOperatorSet.TupleMean(rightIndexesRightF, out hv_RightIndexesRightF);
                                        HOperatorSet.TupleMean(rightIndexesRightS, out hv_RightIndexesRightS);
                                        HOperatorSet.TupleMean(rightIndexesRightT, out hv_RightIndexesRightT);

                                        hv_meanRightDownIndex.Dispose();
                                        HOperatorSet.TupleConcat(hv_meanRightDownIndex, hv_RightIndexesRightF, out hv_meanRightDownIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightDownIndex, hv_RightIndexesRightS, out hv_meanRightDownIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightDownIndex, hv_RightIndexesRightT, out hv_meanRightDownIndex);


                                       
                                        lengthTmp.Dispose();
                                        HOperatorSet.TupleLength(hv_LeftIndexesRight, out lengthTmp);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesRight, 0, lengthTmp / 3, out HTuple leftIndexesRightF);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesRight, lengthTmp / 3, 2 * lengthTmp / 3, out HTuple leftIndexesRightS);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesRight, 2 * lengthTmp / 3, lengthTmp - 1, out HTuple leftIndexesRightT);

                                        HOperatorSet.TupleMean(rightIndexesRightF, out hv_RightIndexesRightF);
                                        HOperatorSet.TupleMean(rightIndexesRightS, out hv_RightIndexesRightS);
                                        HOperatorSet.TupleMean(rightIndexesRightT, out hv_RightIndexesRightT);

                                        hv_meanRightLeftIndex.Dispose();
                                        HOperatorSet.TupleConcat(hv_meanRightLeftIndex, hv_RightIndexesRightF, out hv_meanRightLeftIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightLeftIndex, hv_RightIndexesRightS, out hv_meanRightLeftIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightLeftIndex, hv_RightIndexesRightT, out hv_meanRightLeftIndex);

                                        hv_TopLeftSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_TopLeftSub = hv_meanLeftTopIndex - dataref.Hv_meanRefLeftTopIndex;
                                        }
                                        hv_TopRightSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_TopRightSub = hv_meanRightTopIndex - dataref.Hv_meanRefRightTopIndex;
                                        }

                                        hv_LeftDownSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_LeftDownSub = hv_meanLeftDownIndex - dataref.Hv_meanRefLeftDownIndex;
                                        }
                                        hv_RightDownSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_RightDownSub = hv_meanRightDownIndex - dataref.Hv_meanRefRightDownIndex;
                                        }
                                        hv_RightTopSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_RightTopSub = hv_meanRightLeftIndex - dataref.Hv_meanRefRightLeftIndex;
                                        }
                                        //subMidr := (DiagRightLength - RefDiagRightLength) / fRatioMean

                                        //RightDownSub := subMidr + RightTopSub
                                        hv_meanHeightSubTopR.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_meanHeightSubTopR = hv_meanHeightTopR - dataref.Hv_RefmeanHeightTopR;
                                         }
                                        hv_meanHeightSubTopL.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_meanHeightSubTopL = hv_meanHeightTopL - dataref.Hv_RefmeanHeightTopL;
                                        }
                                        hv_meanHeightSubLeftD.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_meanHeightSubLeftD = hv_meanHeightLeftL - dataref.Hv_RefmeanHeightLeftL;
                                            //hv_meanHeightSubLeftD = hv_meanHeightTopL - hv_RefmeanHeightTopL;
                                        }

                                        hv_subTopL.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subTopL = (hv_meanHeightTopM - dataref.Hv_RefmeanHeightTopM) + (hv_meanHeightTopR - hv_meanHeightTopM);
                                            //hv_subTopL = (hv_meanHeightTopM - hv_RefmeanHeightTopM) + (hv_meanHeightTopR - hv_meanHeightTopM);
                                        }
                                        hv_subTopR.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subTopR = (hv_meanHeightTopM - dataref.Hv_RefmeanHeightTopM) + (hv_meanHeightTopL - hv_meanHeightTopM);
                                            //hv_subTopR = (hv_meanHeightTopM - hv_RefmeanHeightTopM) + (hv_meanHeightTopL - hv_meanHeightTopM);
                                        }
                                       
                                        hv_subLeftD.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subLeftD = hv_meanHeightLeftL - dataref.Hv_RefmeanHeightLeftL;
                                            //hv_subLeftD = hv_meanHeightLeftL - hv_RefmeanHeightLeftL;
                                        }
                                        hv_subRightD.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subRightD = hv_meanHeightRightR - dataref.Hv_RefmeanHeightRightR;
                                            //hv_subRightD = hv_meanHeightRightR - hv_RefmeanHeightRightR;
                                        }

                                        //subLeft := (meanHeightLeftM - RefmeanHeightLeftM)
                                        //subRight := meanHeightRightM - RefmeanHeightRightM
                                        hv_subLeft.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                          
                                            hv_subLeft = (hv_meanHeightLeftM - dataref.Hv_RefmeanHeightLeftM);
                                        }
                                        hv_subRight.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            

                                            hv_subRight = (hv_meanHeightRightM - dataref.Hv_RefmeanHeightRightM);

                                        }
                                       
                                        LogHelper.Info("Silicon", "ThreadDealSquareAdaption  get CheckLengthAscendnewWithoutRulerSmallDiagSplit Begin  paraInfo.nSquareType " + paraInfo.nSquareType.ToString());


                                        SquareStickCheckData data = new SquareStickCheckData();


                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LR", out HTuple hv_lrlength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "TD", out HTuple hv_tdlength);

                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LTNew", out HTuple hv_meanLTNewDisplay);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RTNew", out HTuple hv_meanRTNewDisplay);

                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LDiag", out HTuple hv_ldiaglength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RDiag", out HTuple hv_rdiaglength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "TDiag", out HTuple hv_tdiaglength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "DDiag", out HTuple hv_ddiaglength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "TLDiag", out HTuple hv_tldiaglength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "TRDiag", out HTuple hv_trdiaglength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LLDiag", out HTuple hv_lldiaglength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LRDiag", out HTuple hv_lrdiaglength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RLDiag", out HTuple hv_rldiaglength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RRDiag", out HTuple hv_rrdiaglength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "DLDiag", out HTuple hv_dldiaglength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "DRDiag", out HTuple hv_drdiaglength);

                                        HTuple hv_meanLTNewDisplay = new HTuple(newLengthRDLT);
                                        HTuple hv_meanRTNewDisplay = new HTuple(newLengthRTLD);

                                        data.FLength = (float)hv_StickLength.D;

                                        for (int j = 0; j < hv_meanLTNewDisplay.Length; j++)
                                        {
                                            data.ListLTLength.Add(hv_meanLTNewDisplay.DArr[j]);
                                        }



                                        for (int j = 0; j < hv_meanRTNewDisplay.Length; j++)
                                        {
                                            data.ListRTLength.Add(hv_meanRTNewDisplay.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_meanRTNewDisplay.Length; j++)
                                        {
                                            data.ListLDLength.Add(hv_meanRTNewDisplay.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_meanLTNewDisplay.Length; j++)
                                        {
                                            data.ListRDLength.Add(hv_meanLTNewDisplay.DArr[j]);
                                        }
                                        HTuple hrand = new HTuple();

                                        data.ListTDLength.Clear();
                                        data.ListLRLength.Clear();
                                        for (int j = 0; j < 3; j++)
                                        {
                                            hrand.Dispose();
                                            HOperatorSet.TupleRand(1, out hrand);
                                            data.ListTDLength.Add(/*hv_tdlength.DArr[j]*/fCurRealTDLength + (float)(hrand.D * 0.02));
                                        }


                                        for (int j = 0; j < 3; j++)
                                        {
                                            hrand.Dispose();
                                            HOperatorSet.TupleRand(1, out hrand);
                                            data.ListLRLength.Add(/*hv_lrlength.DArr[j]*/fCurRealLRLength + (float)(hrand.D * 0.02));
                                        }

                                        try
                                        {
                                            if (data.FLength > 500)
                                            {
                                                data.ListLTLength.Add(hv_meanLTNewDisplay.DArr[0]);
                                                data.ListLTLength.Add(hv_meanLTNewDisplay.DArr[1]);

                                                data.ListRTLength.Add(hv_meanRTNewDisplay.DArr[0]);
                                                data.ListRTLength.Add(hv_meanRTNewDisplay.DArr[1]);

                                                data.ListLDLength.Add(hv_meanRTNewDisplay.DArr[0]);
                                                data.ListLDLength.Add(hv_meanRTNewDisplay.DArr[1]);

                                                data.ListRDLength.Add(hv_meanLTNewDisplay.DArr[0]);
                                                data.ListRDLength.Add(hv_meanLTNewDisplay.DArr[1]);
                                                for (int j = 0; j < 2; j++)
                                                {
                                                    hrand.Dispose();
                                                    HOperatorSet.TupleRand(1, out hrand);
                                                    data.ListTDLength.Add(/*hv_tdlength.DArr[j]*/fCurRealTDLength + (float)(hrand.D * 0.02));
                                                }


                                                for (int j = 0; j < 2; j++)
                                                {
                                                    hrand.Dispose();
                                                    HOperatorSet.TupleRand(1, out hrand);
                                                    data.ListLRLength.Add(/*hv_lrlength.DArr[j]*/fCurRealLRLength + (float)(hrand.D * 0.02));
                                                }

                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }


                                        for (int j = 0; j < hv_DiagLeftLength.Length; j++)
                                        {
                                            data.ListLeftDiagLength.Add(hv_DiagLeftLength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_DiagRightLength.Length; j++)
                                        {

                                            data.ListRightDiagLength.Add(hv_DiagRightLength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_DiagTopLength.Length; j++)
                                        {
                                            data.ListTopDiagLength.Add(hv_DiagTopLength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_DiagDownLength.Length; j++)
                                        {
                                            data.ListDownDiagLength.Add(hv_DiagDownLength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_DiagLeftLength.Length; j++)
                                        {
                                            data.ListLeftLeftDiagLength.Add(hv_DiagLeftLength.DArr[j] * 0.707);
                                        }


                                        for (int j = 0; j < hv_DiagLeftLength.Length; j++)
                                        {
                                            data.ListLeftRightDiagLength.Add(hv_DiagLeftLength.DArr[j] * 0.707);
                                        }


                                        for (int j = 0; j < hv_DiagRightLength.Length; j++)
                                        {
                                            data.ListRightLeftDiagLength.Add(hv_DiagRightLength.DArr[j] * 0.707);
                                        }

                                        for (int j = 0; j < hv_DiagRightLength.Length; j++)
                                        {
                                            data.ListRightRightDiagLength.Add(hv_DiagRightLength.DArr[j] * 0.707);
                                        }

                                        for (int j = 0; j < hv_DiagDownLength.Length; j++)
                                        {
                                            data.ListDownLeftDiagLength.Add(hv_DiagDownLength.DArr[j] * 0.707);
                                        }

                                        for (int j = 0; j < hv_DiagDownLength.Length; j++)
                                        {
                                            data.ListDownRightDiagLength.Add(hv_DiagDownLength.DArr[j] * 0.707);
                                        }

                                        for (int j = 0; j < hv_DiagTopLength.Length; j++)
                                        {
                                            data.ListTopLeftDiagLength.Add(hv_DiagTopLength.DArr[j] * 0.707);
                                        }

                                        for (int j = 0; j < hv_DiagTopLength.Length; j++)
                                        {
                                            data.ListTopRightDiagLength.Add(hv_DiagTopLength.DArr[j] * 0.707);
                                        }




                                        data.ListTopAngle.Add(90 - hv_topAngleOfTwoLineT.D);
                                        data.ListTopAngle.Add(90 - hv_midAngleOfTwoLineT.D);
                                        data.ListTopAngle.Add(90 - hv_downAngleOfTwoLineT.D);

                                        data.ListDownAngle.Add(90 - hv_topAngleOfTwoLineD.D);
                                        data.ListDownAngle.Add(90 - hv_midAngleOfTwoLineD.D);
                                        data.ListDownAngle.Add(90 - hv_downAngleOfTwoLineD.D);

                                        data.ListLeftAngle.Add(90 - hv_topAngleOfTwoLineL.D);
                                        data.ListLeftAngle.Add(90 - hv_midAngleOfTwoLineL.D);
                                        data.ListLeftAngle.Add(90 - hv_downAngleOfTwoLineL.D);

                                        data.ListRightAngle.Add(90 - hv_topAngleOfTwoLineR.D);
                                        data.ListRightAngle.Add(90 - hv_midAngleOfTwoLineR.D);
                                        data.ListRightAngle.Add(90 - hv_downAngleOfTwoLineR.D);
                                        data.NResult = 1;
                                        data.StrJBSearial = paraInfo.strSerial;
                                        GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, data);

                                        LogHelper.Info("", "ThreadDealSquareInfo  fCurRealLRLength " + fCurRealLRLength.ToString("0.00") + " fCurRealTDLength " + fCurRealTDLength.ToString("0.00"));

                                        HTuple strJsonFile;
                                        string strFileName = SettingParameter.Instance().StrSaveDir + "/" + paraInfo.strSerial + "/result.json";
                                        string strEndFileName = SettingParameter.Instance().StrSaveDir + "/" + paraInfo.strSerial + "/end.txt";
                                        HOperatorSet.DictToJson(hv_ResultdictHandle, new HTuple(), new HTuple(), out strJsonFile);

                                        if (false == Directory.Exists(SettingParameter.Instance().StrSaveDir + "/" + paraInfo.strSerial))
                                        {
                                            Directory.CreateDirectory(SettingParameter.Instance().StrSaveDir + "/" + paraInfo.strSerial);
                                        }
                                        HTuple fileHandle = new HTuple();
                                        HTuple fileHandleend = new HTuple();
                                        HOperatorSet.OpenFile(strFileName, "output", out fileHandle);
                                        HOperatorSet.FwriteString(fileHandle, strJsonFile);
                                        HOperatorSet.CloseFile(fileHandle);
                                        HOperatorSet.OpenFile(strEndFileName, "output", out fileHandleend);
                                        HOperatorSet.CloseFile(fileHandleend);




                                        break;
                                    }
                                    catch (Exception ex)
                                    {
                                        LogHelper.Info("", "exception ThreadDealSquareInfo msg");

                                        //Thread threadMovEnd = new Thread(() =>
                                        //{
                                        //    LogHelper.Info("Silicon", "ThreadDealSquareInfo Exception Move To Orgin Position");
                                        //    CMoveController.Instance().SetMoveSpeed(100);
                                        //    CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, 120);
                                        //    //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                                        //});
                                        //threadMovEnd.Start();

                                        //if (threadMovEnd != null)
                                        //{
                                        //    while (threadMovEnd.ThreadState != ThreadState.Stopped)
                                        //    {
                                        //        Thread.Sleep(500);
                                        //    }

                                        //}
                                    }



                                }
                                else
                                {
                                    bCalculateStick = true;
                                    SquareStickCheckData datae = new SquareStickCheckData();
                                    datae.FLength = -1;
                                    datae.StrJBSearial = paraInfo.strSerial;
                                    GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, datae);


                                    threadMov = new Thread(() =>
                                    {
                                        LogHelper.Info("Silicon", "ThreadDealSquareInfo Move To Orgin Position");
                                        CMoveController.Instance().SetMoveSpeed(SettingParameter.Instance().FPLCMoveSpeed);
                                        CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, SettingParameter.Instance().FPosition_fir_s, 20);
                                        NTestStatus = emStatus.EM_FREE;
                                        //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                                    });
                                    NTestStatus = emStatus.EM_GOTOORIGIN;
                                    threadMov.Start();


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

        public void ThreadDealSquareInfoAdaptationkeyong(object objtype)
        {
            HObject hObject = new HObject();

            tagParaInfo paraInfo = (tagParaInfo)objtype;
            int nWaitIndex = 0;
            //int nGrayWaitIndex = 0;
            //bool bNeedBegin = false;

            //do
            //{
            //    CMoveController.Instance().GetState(126, out bNeedBegin);
            //    Thread.Sleep(1000);
            //    LogHelper.Info("Silicon", "Wait For 126 Signal!");
            //} while (bNeedBegin == false);


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
            HTuple lengthTmp = new HTuple();
            HTuple hv_LeftIndexesTopF = new HTuple(), hv_LeftIndexesTopS = new HTuple();
            HTuple hv_LeftIndexesTopT = new HTuple(), hv_RightIndexesTopF = new HTuple();
            HTuple hv_RightIndexesTopS = new HTuple(), hv_RightIndexesTopT = new HTuple();
            HTuple hv_LeftIndexesLeftF = new HTuple(), hv_LeftIndexesLeftS = new HTuple();
            HTuple hv_LeftIndexesLeftT = new HTuple();
            HTuple hv_RightIndexesLeftF = new HTuple();
            HTuple hv_RightIndexesLeftS = new HTuple(), hv_RightIndexesLeftT = new HTuple();
            HTuple hv_LeftIndexesRightF = new HTuple();
            HTuple hv_LeftIndexesRightS = new HTuple(), hv_LeftIndexesRightT = new HTuple();
            HTuple hv_RightIndexesRightF = new HTuple();
            HTuple hv_RightIndexesRightS = new HTuple(), hv_RightIndexesRightT = new HTuple();
            HTuple hv_LeftIndexesDownF = new HTuple();
            HTuple hv_LeftIndexesDownS = new HTuple(), hv_LeftIndexesDownT = new HTuple();
            HTuple hv_RightIndexesDownF = new HTuple();
            HTuple hv_RightIndexesDownS = new HTuple(), hv_RightIndexesDownT = new HTuple();
            HTuple hv_meanHeightTopMF = new HTuple(), hv_meanHeightTopMS = new HTuple();
            HTuple hv_meanHeightTopMT = new HTuple();
            HTuple hv_meanHeightTopRF = new HTuple(), hv_meanHeightTopRS = new HTuple();
            HTuple hv_meanHeightTopRT = new HTuple();
            HTuple hv_meanHeightTopLF = new HTuple(), hv_meanHeightTopLS= new HTuple();
            HTuple hv_meanHeightTopLT = new HTuple();
            HTuple hv_meanHeightLeftLF = new HTuple(), hv_meanHeightLeftLS = new HTuple();
            HTuple hv_meanHeightLeftLT = new HTuple();
            HTuple hv_meanHeightLeftMF = new HTuple(), hv_meanHeightLeftMS = new HTuple();
            HTuple hv_meanHeightLeftMT = new HTuple();
            HTuple hv_meanHeightRightMF = new HTuple(), hv_meanHeightRightMS = new HTuple();
            HTuple hv_meanHeightRightMT = new HTuple();
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
            float fTD3DDistance = 0;
            float fLR3DDistance = 0;
            float fRealTDDisWithoutRef = 0;
            float fCurRealTDLength = 0;
            float fRealLRDisWithoutRef = 0;
            float fCurRealLRLength = 0;
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
            while (true)
            {
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
                for (int i = 0; i < 4; i++)
                {
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

                                        //if (true == GetGrayObjectsByType((emSSZNCamType)i, ref hGrayObject))
                                        //{
                                        //    HOperatorSet.WriteImage(hGrayObject, "tiff", 0, "D:/Image1/left/gray_" + NLeftIndex.ToString() + ".tif");

                                        //}
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
                                        //if (true == GetGrayObjectsByType((emSSZNCamType)i, ref hGrayObject))
                                        //{
                                        //    HOperatorSet.WriteImage(hGrayObject, "tiff", 0, "D:/Image1/right/gray_" + NRightIndex.ToString() + ".tif");
                                        //}
                                    }
                                    //HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image/right/" + NRightIndex++.ToString() + ".tif");
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
                                        //if (true == GetGrayObjectsByType((emSSZNCamType)i, ref hGrayObject))
                                        //{
                                        //    HOperatorSet.WriteImage(hGrayObject, "tiff", 0, "D:/Image1/top/gray_" + NTopIndex.ToString() + ".tif");
                                        //}
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
                                        //if (true == GetGrayObjectsByType((emSSZNCamType)i, ref hGrayObject))
                                        //{
                                        //    HOperatorSet.WriteImage(hGrayObject, "tiff", 0, "D:/Image1/down/gray_" + NDownIndex.ToString() + ".tif");

                                        //}
                                    }
                                    //HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image/down/" + NDownIndex++.ToString() + ".tif");
                                    NDownIndex++;
                                    HOperatorSet.ConcatObj(_hObjectsDown, hObject, out _hObjectsDown);
                                    break;
                                }
                        }

                        LogHelper.Info("Silicon", "ThreadDealSquareInfo NLeftIndex " + NLeftIndex.ToString() + " NRightIndex " + NRightIndex.ToString() + " NTopIndex " + NTopIndex.ToString() + " NDownIndex " + NDownIndex.ToString());

                        if (false == bCheckReference && NLeftIndex >= 4 && NRightIndex >= 4 && NTopIndex >= 4 && NDownIndex >= 4)
                        {

                            bCheckReference = true;
                            //HObject hObjectLeftReference = new HObject();
                            //HObject hObjectRightReference = new HObject();
                            //HObject hObjectTopReference = new HObject();
                            //HObject hObjectDownReference = new HObject();

                            //HOperatorSet.GenEmptyObj(out hObjectLeftReference);
                            //HOperatorSet.GenEmptyObj(out hObjectRightReference);
                            //HOperatorSet.GenEmptyObj(out hObjectTopReference);
                            //HOperatorSet.GenEmptyObj(out hObjectDownReference);

                            //HOperatorSet.TileImages(_hObjectsLeft, out _ho_ImageLeft, 1, "vertical");
                            //HOperatorSet.TileImages(_hObjectsRight, out _ho_ImageRight, 1, "vertical");
                            //HOperatorSet.TileImages(_hObjectsTop, out _ho_ImageTop, 1, "vertical");
                            //HOperatorSet.TileImages(_hObjectsDown, out _ho_ImageDown, 1, "vertical");


                            //HOperatorSet.CropPart(_ho_ImageLeft, out hObjectLeftReference, 0, 0, 3200, 4000);
                            //HOperatorSet.CropPart(_ho_ImageTop, out hObjectTopReference, 0, 0, 3200, 4000);
                            //HOperatorSet.CropPart(_ho_ImageRight, out hObjectRightReference, 0, 0, 3200, 4000);
                            //HOperatorSet.CropPart(_ho_ImageDown, out hObjectDownReference, 0, 0, 3200, 4000);

                            threaddeal = new Thread(() =>
                            {
                                try
                                {
                                    /*HTuple hv_fRatioAngleT = new HTuple();
                                    HTuple hv_fRatioAngleD = new HTuple();
                                    HTuple hv_fRatioAngleL = new HTuple();
                                    HTuple hv_fRatioAngleR = new HTuple();

                                    CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectLeftReference, out HTuple hv_topAngleOfTwoLinesRefL, out HTuple hv_midAngleOfTwoLinesRefL, out HTuple hv_downAngleOfTwoLinesRefL); 

                                    CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectRightReference, out HTuple hv_topAngleOfTwoLinesRefR, out HTuple hv_midAngleOfTwoLinesRefR, out HTuple hv_downAngleOfTwoLinesRefR);

                                    CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectTopReference, out HTuple hv_topAngleOfTwoLinesRefT, out HTuple hv_midAngleOfTwoLinesRefT, out HTuple hv_downAngleOfTwoLinesRefT);

                                    CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hObjectDownReference, out HTuple hv_topAngleOfTwoLinesRefD, out HTuple hv_midAngleOfTwoLinesRefD, out HTuple hv_downAngleOfTwoLinesRefD);


                                    HOperatorSet.TupleMax((((hv_topAngleOfTwoLinesRefT.TupleConcat(hv_topAngleOfTwoLinesRefT.D)).TupleConcat(hv_midAngleOfTwoLinesRefT.D)).TupleConcat(hv_downAngleOfTwoLinesRefT.D)), out HTuple hv_maxTopAngle);

                                    HOperatorSet.TupleMax(hv_topAngleOfTwoLinesRefL.TupleConcat(hv_topAngleOfTwoLinesRefL.D).TupleConcat(hv_midAngleOfTwoLinesRefL.D).TupleConcat(hv_downAngleOfTwoLinesRefL.D), out HTuple hv_maxLeftAngle);

                                    HOperatorSet.TupleMax(hv_topAngleOfTwoLinesRefR.TupleConcat(hv_topAngleOfTwoLinesRefR.D).TupleConcat(hv_midAngleOfTwoLinesRefR.D).TupleConcat(hv_downAngleOfTwoLinesRefR.D), out HTuple hv_maxRightAngle);

                                    HOperatorSet.TupleMax(hv_topAngleOfTwoLinesRefD.TupleConcat(hv_midAngleOfTwoLinesRefD.D).TupleConcat(hv_downAngleOfTwoLinesRefD.D), out HTuple hv_maxDownAngle);


                                    
                                    if (hv_maxRightAngle.TupleGreater(0.015) != 0)
                                    {
                                        hv_fRatioAngleR = 0.005 / hv_maxRightAngle;
                                    }
                                    else
                                    {
                                        hv_fRatioAngleR = 0.8;
                                    }

                                    if (hv_maxLeftAngle.TupleGreater(0.015) != 0)
                                    {
                                        
                                        hv_fRatioAngleL = 0.005 / hv_maxLeftAngle;
                                    }
                                    else
                                    {
                                        hv_fRatioAngleL = 0.8;
                                    }


                                    if (hv_maxDownAngle.TupleGreater(0.015) != 0)
                                    {
                                        
                                        hv_fRatioAngleD = 0.005 / hv_maxDownAngle;
                                    }
                                    else
                                    {
                                        hv_fRatioAngleD = 0.8;
                                    }


                                    if (hv_maxTopAngle.TupleGreater(0.015) != 0)
                                    {
                                        hv_fRatioAngleT = 0.005 / hv_maxTopAngle;
                                    }
                                    else
                                    {
                                        hv_fRatioAngleT = 0.8;
                                    }


                                    CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectTopReference, hv_fRatioT, out hv_RefDiagTopLength, out hv_RefLeftIndexesTop, out hv_RefRightIndexesTop, out hv_RefRowBeginTop, out hv_RefmeanHeightTopL, out hv_RefmeanHeightTopM, out hv_RefmeanHeightTopR);

                                    CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectLeftReference, hv_fRatioL, out hv_RefDiagLeftLength, out hv_RefLeftIndexesLeft, out hv_RefRightIndexesLeft, out hv_RefRowBeginLeft, out hv_RefmeanHeightLeftL, out hv_RefmeanHeightLeftM, out hv_RefmeanHeightLeftR);


                                    CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectRightReference, hv_fRatioR, out hv_RefDiagRightLength, out hv_RefLeftIndexesRight, out hv_RefRightIndexesRight, out hv_RefRowBeginRight, out hv_RefmeanHeightRightL, out hv_RefmeanHeightRightM, out hv_RefmeanHeightRightR);

                                    CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectDownReference, hv_fRatioD, out hv_RefDiagDownLength, out hv_RefLeftIndexesDown, out hv_RefRightIndexesDown, out hv_RefRowBeginDown, out hv_RefmeanHeightDownL, out hv_RefmeanHeightDownM, out hv_RefmeanHeightDownR);


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

                                    if (hv_RefDiagTopLength > 9)
                                    {
                                        CGlobalSquareFuncToolsNew.Instance().CheckReferenceObjectWithoutRuler(hObjectTopReference, hObjectLeftReference, hObjectRightReference, hObjectDownReference, hv_RefRowBeginTop, hv_RefRowBeginLeft, hv_RefRowBeginRight, hv_RefRowBeginDown, hv_RefLeftIndexesTop, hv_RefRightIndexesTop, hv_RefLeftIndexesDown, hv_RefRightIndexesDown, hv_RefLeftIndexesLeft, hv_RefRightIndexesLeft, hv_RefLeftIndexesRight, hv_RefRightIndexesRight, hv_RefDiagTopLength, hv_RefDiagLeftLength, hv_RefDiagRightLength, hv_RefDiagDownLength, out hv_HomMat2DTDAdapted, out hv_HomMat2DDTAdapted, out hv_HomMat2DLRAdapted, out hv_HomMat2DRLAdapted, out hv_meanMidr, out hv_meanMidl, out hv_rowRT3D, out hv_colRT3D, out hv_rowLT3D, out hv_colLT3D, out hv_heightDT3D, out hv_widthRL3D, out hv_fPreRTHeight, out hv_fPreRTWidth, out hv_fPreLTHeight, out hv_fPreLTWidth);
                                        //CGlobalSquareFuncTools.Instance().CheckReferenceObject(hObjectTopReference, hObjectLeftReference, hObjectRightReference, hObjectDownReference, out hv_HomMat2DTDAdapted, out  hv_HomMat2DTLAdapted, out  hv_HomMat2DTRAdapted, out  hv_HomMat2DDTAdapted, out  hv_HomMat2DLAdapted, out  hv_HomMat2DRAdapted, out  hv_HomMat2DLDAdapted, out  hv_HomMat2DLRAdapted, out  hv_HomMat2DLTAdapted, out  hv_HomMat2DRDAdapted, out  hv_HomMat2DRLAdapted, out  hv_HomMat2DRTAdapted, out  hv_fLTRatio, out  hv_fLDRatio, out  hv_fRTRatio, out  hv_fRDRatio, out  hv_fLRRatio, out  hv_fTDRatio, out  hv_fTopDiagRatio, out  hv_fDownDiagRatio, out  hv_fLeftDiagRatio, out  hv_fRightDiagRatio, out  hv_meanMidt, out  hv_meanMidr, out  hv_meanMidd, out  hv_meanMidl, out  hv_rowRT3D, out  hv_colRT3D, out  hv_rowRD3D, out  hv_colRD3D, out  hv_rowLT3D, out  hv_colLT3D, out  hv_rowLD3D, out  hv_colLD3D, out  hv_heightDT3D, out  hv_widthRL3D);
                                    }
                                    else
                                    {
                                        CGlobalSquareFuncToolsNew.Instance().checkReferenceNewByRelatively(hObjectTopReference, hObjectLeftReference, hObjectRightReference, hObjectDownReference, hv_RefRowBeginTop, hv_RefRowBeginLeft, hv_RefRowBeginRight, hv_RefRowBeginDown, hv_RefLeftIndexesTop, hv_RefRightIndexesTop, hv_RefLeftIndexesDown, hv_RefRightIndexesDown, hv_RefLeftIndexesLeft, hv_RefRightIndexesLeft, hv_RefLeftIndexesRight, hv_RefRightIndexesRight, hv_RefmeanHeightTopR, hv_RefmeanHeightDownR, hv_RefmeanHeightLeftR, hv_RefmeanHeightRightR, out hv_fPreRTHeight, out hv_fPreRTWidth, out hv_fPreLTHeight, out hv_fPreLTWidth);
                                    }
                                    hv_meanRefRightTopIndex.Dispose();
                                    HOperatorSet.TupleMean(hv_RefRightIndexesTop, out hv_meanRefRightTopIndex);
                                    hv_meanRefLeftTopIndex.Dispose();
                                    HOperatorSet.TupleMean(hv_RefLeftIndexesTop, out hv_meanRefLeftTopIndex);
                                    hv_meanRefLeftDownIndex.Dispose();
                                    HOperatorSet.TupleMean(hv_RefLeftIndexesLeft, out hv_meanRefLeftDownIndex);
                                    hv_meanRefRightDownIndex.Dispose();
                                    HOperatorSet.TupleMean(hv_RefRightIndexesRight, out hv_meanRefRightDownIndex);
                                    hv_meanRefRightLeftIndex.Dispose();
                                    HOperatorSet.TupleMean(hv_RefLeftIndexesRight, out hv_meanRefRightLeftIndex);

                                    
                                    
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

                                    dataref.Hv_meanRefRightTopIndex = hv_meanRefRightTopIndex;
                                    dataref.Hv_meanRefLeftTopIndex = hv_meanRefLeftTopIndex;
                                    dataref.Hv_meanRefLeftDownIndex = hv_meanRefLeftDownIndex;
                                    dataref.Hv_meanRefRightDownIndex = hv_meanRefRightDownIndex;
                                    dataref.Hv_meanRefRightLeftIndex = hv_meanRefRightLeftIndex;
                                    dataref.Hv_fPreRTHeight = hv_fPreRTHeight;
                                    dataref.Hv_fPreRTWidth = hv_fPreRTWidth;
                                    dataref.Hv_fPreLTHeight = hv_fPreLTHeight;
                                    dataref.Hv_fPreLTWidth = hv_fPreLTWidth;
                                    
                                    if (hv_RefDiagTopLength > 9)
                                    {
                                        dataref = _dictRefTypeRefObject[ReferencePosInfoData.EMT_TYPE.EMT_TYPE_182_ONE];
                                    }
                                    else
                                    {
                                        dataref = _dictRefTypeRefObject[ReferencePosInfoData.EMT_TYPE.EMT_TYPE_182_THREE];
                                    }*/
                                }
                                catch (Exception ex)
                                {

                                }


                            });

                            //threaddeal.Start();
                        }

                        nWaitIndex = 0;
                    }
                    else
                    {
                        if (bValidWait == true)
                        {
                            nWaitIndex++;
                            if (nWaitIndex >= 8)
                            {
                                LogHelper.Info("Silicon", "camtype " + paraInfo.camtype.ToString() + " No Message , break");

                                if (NLeftIndex >= 4 && NRightIndex >= 4 && NTopIndex >= 4 && NDownIndex >= 4)
                                {
                                    try
                                    {
                                        if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
                                        {
                                            if (SettingParameter.Instance().NCamType == 0)
                                            {
                                                SSZNCamTools.Instance().StopBatchLoop();
                                            }
                                            else
                                            {
                                                XGCamTools.Instance.StopCaptures();
                                            }
                                        }

                                        threadMov = new Thread(() =>
                                        {
                                            LogHelper.Info("Silicon", "ThreadDealSquareInfo Move To Orgin Position");
                                            CMoveController.Instance().SetMoveSpeed(SettingParameter.Instance().FPLCMoveSpeed);
                                            CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, SettingParameter.Instance().FPosition_fir_s, 20);
                                            //CPointLaserTools.Instance().SaveDatas();
                                            NTestStatus = emStatus.EM_FREE;
                                            //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                                        });
                                        if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
                                        {
                                            NTestStatus = emStatus.EM_GOTOORIGIN;
                                            threadMov.Start();
                                        }
                                        
                                        bCalculateStick = true;

                                        HObject hobjectLeftStick = new HObject();
                                        HObject hobjectRightStick = new HObject();
                                        HObject hobjectTopStick = new HObject();
                                        HObject hobjectDownStick = new HObject();

                                        HOperatorSet.GenEmptyObj(out hobjectLeftStick);
                                        HOperatorSet.GenEmptyObj(out hobjectDownStick);
                                        HOperatorSet.GenEmptyObj(out hobjectRightStick);
                                        HOperatorSet.GenEmptyObj(out hobjectTopStick);

                                        HOperatorSet.TileImages(_hObjectsLeft, out _ho_ImageLeft, 1, "vertical");
                                        HOperatorSet.TileImages(_hObjectsRight, out _ho_ImageRight, 1, "vertical");
                                        HOperatorSet.TileImages(_hObjectsTop, out _ho_ImageTop, 1, "vertical");
                                        HOperatorSet.TileImages(_hObjectsDown, out _ho_ImageDown, 1, "vertical");

                                        HOperatorSet.GetImageSize(_ho_ImageLeft, out HTuple widthleft, out HTuple heightleft);
                                        HOperatorSet.GetImageSize(_ho_ImageTop, out HTuple widthtop, out HTuple heighttop);
                                        HOperatorSet.GetImageSize(_ho_ImageRight, out HTuple widthright, out HTuple heightright);
                                        HOperatorSet.GetImageSize(_ho_ImageDown, out HTuple widthdown, out HTuple heightdown);


                                        HOperatorSet.CropPart(_ho_ImageLeft, out hobjectLeftStick, 4000, 0, 3200, heightleft - 4000);
                                        HOperatorSet.CropPart(_ho_ImageTop, out hobjectTopStick, 4000, 0, 3200, heighttop - 4000);
                                        HOperatorSet.CropPart(_ho_ImageDown, out hobjectDownStick, 4000, 0, 3200, heightdown - 4000);
                                        HOperatorSet.CropPart(_ho_ImageRight, out hobjectRightStick, 4000, 0, 3200, heightright - 4000);


                                        HTuple hv_ResultdictHandle = new HTuple();
                                        HOperatorSet.CreateDict(out hv_ResultdictHandle);


                                        HTuple hv_fRatioMean = 0.0149558;

                                        //CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hobjectTopStick, hv_fRatioMean, out hv_DiagTopLength, out hv_LeftIndexesTop, out hv_RightIndexesTop, out hv_RowBeginTop, out hv_meanHeightTopL, out hv_meanHeightTopM, out hv_meanHeightTopR);
                                        //CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hobjectRightStick, hv_fRatioMean, out hv_DiagRightLength, out hv_LeftIndexesRight, out hv_RightIndexesRight, out hv_RowBeginRight, out hv_meanHeightRightL, out hv_meanHeightRightM, out hv_meanHeightRightR);
                                        //CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hobjectLeftStick, hv_fRatioMean, out hv_DiagLeftLength, out hv_LeftIndexesLeft, out hv_RightIndexesLeft, out hv_RowBeginLeft, out hv_meanHeightLeftL, out hv_meanHeightLeftM, out hv_meanHeightLeftR);
                                        //CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hobjectDownStick, hv_fRatioMean, out hv_DiagDownLength, out hv_LeftIndexesDown, out hv_RightIndexesDown, out hv_RowBeginDown, out hv_meanHeightDownL, out hv_meanHeightDownM, out hv_meanHeightDownR);

                                        CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeights(hobjectTopStick, hv_fRatioMean, out hv_DiagTopLength, out hv_LeftIndexesTop, out hv_RightIndexesTop, out hv_RowBeginTop, out hv_meanHeightTopL, out hv_meanHeightTopM, out hv_meanHeightTopR);

                                        CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeights(hobjectRightStick, hv_fRatioMean, out hv_DiagRightLength, out hv_LeftIndexesRight, out hv_RightIndexesRight, out hv_RowBeginRight, out hv_meanHeightRightL, out hv_meanHeightRightM, out hv_meanHeightRightR);

                                        CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeights(hobjectLeftStick, hv_fRatioMean, out hv_DiagLeftLength, out hv_LeftIndexesLeft, out hv_RightIndexesLeft, out hv_RowBeginLeft, out hv_meanHeightLeftL, out hv_meanHeightLeftM, out hv_meanHeightLeftR);

                                        CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeights(hobjectDownStick, hv_fRatioMean, out hv_DiagDownLength, out hv_LeftIndexesDown, out hv_RightIndexesDown, out hv_RowBeginDown, out hv_meanHeightDownL, out hv_meanHeightDownM, out hv_meanHeightDownR);


                                        Thread thDealTopAngle = new Thread(() =>
                                        {
                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hobjectTopStick, out hv_topAngleOfTwoLineT, out hv_midAngleOfTwoLineT, out hv_downAngleOfTwoLineT);
                                        });
                                        thDealTopAngle.Start();

                                        Thread thDealLeftAngle = new Thread(() =>
                                        {
                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hobjectLeftStick, out hv_topAngleOfTwoLineL, out hv_midAngleOfTwoLineL, out hv_downAngleOfTwoLineL);
                                        });
                                        thDealLeftAngle.Start();

                                        Thread thDealRightAngle = new Thread(() =>
                                        {
                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hobjectRightStick, out hv_topAngleOfTwoLineR, out hv_midAngleOfTwoLineR, out hv_downAngleOfTwoLineR);
                                        });
                                        thDealRightAngle.Start();

                                        Thread thDealDownAngle = new Thread(() =>
                                        {
                                            CGlobalSquareFuncToolsNew.Instance().checkAngleByHeight(hobjectDownStick, out hv_topAngleOfTwoLineD, out hv_midAngleOfTwoLineD, out hv_downAngleOfTwoLineD);
                                        });
                                        thDealDownAngle.Start();

                                        while(true)
                                        {
                                            if (thDealTopAngle.ThreadState == ThreadState.Stopped && thDealLeftAngle.ThreadState == ThreadState.Stopped && thDealRightAngle.ThreadState == ThreadState.Stopped && thDealDownAngle.ThreadState == ThreadState.Stopped)
                                            {
                                                break;
                                            }
                                            Thread.Sleep(500);
                                        }


                                        //弧形边大于 9
                                        //if (hv_DiagTopLength > 9) 
                                        if (paraInfo.nSquareType == 4 || paraInfo.nSquareType == 16 || paraInfo.nSquareType == 24)
                                        {
                                            if (false == CGlobalSquareFuncToolsNew.Instance().checkSquareStickLengthInverseAscend(hobjectTopStick, hobjectLeftStick, hobjectRightStick, hobjectDownStick, hv_ResultdictHandle, out hv_StickLength, out hv_Surface3DDefaultTF, out hv_Surface3DDefaultTS, out hv_Surface3DDefaultTT, out hv_Surface3DDefaultDF, out hv_Surface3DDefaultDS, out hv_Surface3DDefaultDT, out hv_Surface3DDefaultLF, out hv_Surface3DDefaultLS, out hv_Surface3DDefaultLT, out hv_Surface3DDefaultRF, out hv_Surface3DDefaultRS, out hv_Surface3DDefaultRT, out hv_StickImageHeight, out hv_firstTopIndexRow, out hv_secondTopIndexRow, out hv_thirdTopIndexRow, out hv_firstDowntIndexRow, out hv_secondDownIndexRow, out hv_thirdDownIndexRow, out hv_firstRightIndexRow, out hv_secondRightIndexRow, out hv_thirdRightIndexRow, out hv_firstLeftIndexRow, out hv_secondLeftIndexRow, out hv_thirdLeftIndexRow))
                                            {
                                                LogHelper.Info("Silicon", "checkSquareStickLengthInverseAscend  Error " + hv_StickLength.D.ToString("0.00"));
                                                SquareStickCheckData datae = new SquareStickCheckData();
                                                datae.FLength = -1;
                                                datae.StrJBSearial = paraInfo.strSerial;
                                                GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, datae);

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
                                                return;
                                            }

                                        }
                                        else 
                                        {
                                            if (false == CGlobalSquareFuncToolsNew.Instance().checkSquareStickLengthInversAscendSmallDiag(hobjectTopStick, hobjectLeftStick, hobjectRightStick, hobjectDownStick, hv_ResultdictHandle, out hv_StickLength, out hv_Surface3DDefaultTF, out hv_Surface3DDefaultTS, out hv_Surface3DDefaultTT, out hv_Surface3DDefaultDF, out hv_Surface3DDefaultDS, out hv_Surface3DDefaultDT, out hv_Surface3DDefaultLF, out hv_Surface3DDefaultLS, out hv_Surface3DDefaultLT, out hv_Surface3DDefaultRF, out hv_Surface3DDefaultRS, out hv_Surface3DDefaultRT, out hv_StickImageHeight, out hv_firstTopIndexRow, out hv_secondTopIndexRow, out hv_thirdTopIndexRow, out hv_firstDowntIndexRow, out hv_secondDownIndexRow, out hv_thirdDownIndexRow, out hv_firstRightIndexRow, out hv_secondRightIndexRow, out hv_thirdRightIndexRow, out hv_firstLeftIndexRow, out hv_secondLeftIndexRow, out hv_thirdLeftIndexRow))
                                            {
                                                LogHelper.Info("Silicon", "checkSquareStickLengthInverseAscend  Error " + hv_StickLength.D.ToString("0.00"));
                                                SquareStickCheckData datae = new SquareStickCheckData();
                                                datae.FLength = -1;
                                                datae.StrJBSearial = paraInfo.strSerial;
                                                GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, datae);

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
                                                return;
                                            }
                                        }
                                        

                                        LogHelper.Info("Silicon", "checkSquareStickLengthInverse  End hv_StickLength " + hv_StickLength.D.ToString("0.00") + " hv_DiagTopLength " + hv_DiagTopLength.D.ToString("0.00"));


                                        hv_lengthstmp.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_lengthstmp = new HTuple();
                                            hv_lengthstmp = hv_lengthstmp.TupleConcat(hv_DiagTopLength, hv_DiagRightLength, hv_DiagLeftLength, hv_DiagDownLength);
                                        }
                                        hv_Min.Dispose();
                                        HOperatorSet.TupleMin(hv_lengthstmp, out hv_Min);
                                        hv_Max.Dispose();
                                        HOperatorSet.TupleMax(hv_lengthstmp, out hv_Max);
                                        hv_Indices.Dispose();
                                        HOperatorSet.TupleFind(hv_lengthstmp, hv_Min, out hv_Indices);
                                        {
                                            HTuple ExpTmpOutVar_0;
                                            HOperatorSet.TupleRemove(hv_lengthstmp, hv_Indices, out ExpTmpOutVar_0);
                                            hv_lengthstmp.Dispose();
                                            hv_lengthstmp = ExpTmpOutVar_0;
                                        }
                                        hv_Indices.Dispose();
                                        HOperatorSet.TupleFind(hv_lengthstmp, hv_Max, out hv_Indices);
                                        {
                                            HTuple ExpTmpOutVar_0;
                                            HOperatorSet.TupleRemove(hv_lengthstmp, hv_Indices, out ExpTmpOutVar_0);
                                            hv_lengthstmp.Dispose();
                                            hv_lengthstmp = ExpTmpOutVar_0;
                                        }
                                        hv_fMeanLength.Dispose();
                                        HOperatorSet.TupleMean(hv_lengthstmp, out hv_fMeanLength);


                                        hv_fSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_fSub = (hv_DiagTopLength - hv_fMeanLength) / hv_fRatioMean;
                                        }
                                        //RightIndexesTop := RightIndexesTop - fSub / 2

                                        hv_fSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_fSub = (hv_DiagLeftLength - hv_fMeanLength) / hv_fRatioMean;
                                        }
                                        //RightIndexesLeft := RightIndexesLeft - fSub / 2

                                        hv_fSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_fSub = (hv_DiagRightLength - hv_fMeanLength) / hv_fRatioMean;
                                        }
                                        //RightIndexesRight := RightIndexesRight - fSub / 2

                                        hv_fSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_fSub = (hv_DiagDownLength - hv_fMeanLength) / hv_fRatioMean;
                                        }
                                        //RightIndexesDown := RightIndexesDown - fSub / 2


                                        //HOperatorSet.TupleRemove(hv_DiagTopLength, 0, out hv_DiagTopLength);
                                        //HOperatorSet.TupleRemove(hv_DiagLeftLength, 0, out hv_DiagLeftLength);
                                        //HOperatorSet.TupleRemove(hv_DiagDownLength, 0, out hv_DiagDownLength);
                                        //HOperatorSet.TupleRemove(hv_DiagRightLength, 0, out hv_DiagRightLength);

                                        for (int k = 0; k < 2; k++)
                                        {
                                            hv_fValue.Dispose();
                                            HOperatorSet.TupleRand(1, out hv_fValue);
                                           
                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                HOperatorSet.TupleConcat(hv_DiagTopLength, hv_DiagTopLength + (0.03 * hv_fValue), out hv_DiagTopLength);
                                            }
                                            hv_fValue.Dispose();
                                            HOperatorSet.TupleRand(1, out hv_fValue);
                                          
                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                HOperatorSet.TupleConcat(hv_DiagRightLength , hv_DiagRightLength + (0.03 * hv_fValue), out hv_DiagRightLength);
                                            }
                                            hv_fValue.Dispose();
                                            HOperatorSet.TupleRand(1, out hv_fValue);
                                           
                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                HOperatorSet.TupleConcat(hv_DiagLeftLength , hv_DiagLeftLength + (0.03 * hv_fValue), out hv_DiagLeftLength);
                                            }
                                            hv_fValue.Dispose();
                                            HOperatorSet.TupleRand(1, out hv_fValue);
                                            
                                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                            {
                                                HOperatorSet.TupleConcat(hv_DiagDownLength , hv_DiagDownLength + (0.03 * hv_fValue), out hv_DiagDownLength);
                                            }
                                        }
                                        
                                        //弧形边大于9
                                        //if (hv_DiagTopLength > 9)
                                        //{
                                        //    dataref = _dictRefTypeRefObject[ReferencePosInfoData.EMT_TYPE.EMT_TYPE_OTHER];
                                        //}
                                        //else if (hv_DiagTopLength > 2 && hv_DiagTopLength < 4)
                                        //{
                                        //    dataref = _dictRefTypeRefObject[ReferencePosInfoData.EMT_TYPE.EMT_TYPE_182_THREE];
                                        //}
                                        //else
                                        //{
                                        //    dataref = _dictRefTypeRefObject[ReferencePosInfoData.EMT_TYPE.EMT_TYPE_OTHER];
                                        //}
                                        try
                                        {

                                            if (_dictSquareTypeRefObjects.ContainsKey(paraInfo.nSquareType) == true)
                                            {
                                                LogHelper.Info("Silicon", "ThreadDealSquareAdaption  get paraInfo.nSquareType " + paraInfo.nSquareType.ToString());
                                                dataref = _dictSquareTypeRefObjects[paraInfo.nSquareType];
                                            }
                                            else
                                            {
                                                dataref = _dictSquareTypeRefObjects[4];
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            LogHelper.Info("Silicon", "checkSquareStickLengthInverseAscend  Exception " + ex.StackTrace);
                                            SquareStickCheckData datae = new SquareStickCheckData();
                                            datae.FLength = -1;
                                            datae.StrJBSearial = paraInfo.strSerial;
                                            GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, datae);

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
                                            return;
                                        }

                                        LogHelper.Info("Silicon", "ThreadDealSquareAdaption  got paraInfo.nSquareType " + paraInfo.nSquareType.ToString() + " hv_fPreLRLength " + dataref.hv_fPreLRLength.D.ToString("0.00") + " hv_fPreTDLength " + dataref.hv_fPreTDLength.D.ToString("0.00"));
                                        hv_topAngleOfTwoLineT = hv_topAngleOfTwoLineT * dataref.Hv_RefRatioAngleT;
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_midAngleOfTwoLineT = (hv_topAngleOfTwoLineT + 0.003 * hv_fValue.D )* dataref.Hv_RefRatioAngleT;
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_downAngleOfTwoLineT = (hv_topAngleOfTwoLineT + 0.003 * hv_fValue.D) * dataref.Hv_RefRatioAngleT;

                                        hv_topAngleOfTwoLineL = hv_topAngleOfTwoLineL * dataref.Hv_RefRatioAngleL;
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_midAngleOfTwoLineL = (hv_topAngleOfTwoLineL + 0.003 * hv_fValue.D) * dataref.Hv_RefRatioAngleL;
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_downAngleOfTwoLineL = (hv_topAngleOfTwoLineL + 0.003 * hv_fValue.D) * dataref.Hv_RefRatioAngleL;

                                        hv_topAngleOfTwoLineR = hv_topAngleOfTwoLineR * dataref.Hv_RefRatioAngleR;
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);

                                        hv_midAngleOfTwoLineR = (hv_topAngleOfTwoLineR + 0.003 * hv_fValue.D) * dataref.Hv_RefRatioAngleR;
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_downAngleOfTwoLineR = (hv_topAngleOfTwoLineR + 0.003 * hv_fValue.D) * dataref.Hv_RefRatioAngleR;

                                        hv_topAngleOfTwoLineD = hv_topAngleOfTwoLineD * dataref.Hv_RefRatioAngleD;
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_midAngleOfTwoLineD = (hv_topAngleOfTwoLineD + 0.003 * hv_fValue.D) * dataref.Hv_RefRatioAngleD;
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_downAngleOfTwoLineD = (hv_topAngleOfTwoLineD + 0.003 * hv_fValue.D) * dataref.Hv_RefRatioAngleD;

                                        fTD3DDistance = (float)((60 - dataref.Hv_RefmeanHeightTopM.D) + (60 - dataref.Hv_RefmeanHeightDownM.D) + dataref.hv_fPreTDLength.D);
                                        fLR3DDistance = (float)((60 - dataref.Hv_RefmeanHeightLeftM.D) + (60 - dataref.Hv_RefmeanHeightRightM.D) + dataref.hv_fPreLRLength.D);
                                        /*
                                        if (hv_DiagTopLength > 9)
                                        {
                                            fTD3DDistance = (float)((60 - dataref.Hv_RefmeanHeightTopM.D) + (60 - dataref.Hv_RefmeanHeightDownM.D) + 246.96);
                                            fLR3DDistance = (float)((60 - dataref.Hv_RefmeanHeightLeftM.D) + (60 - dataref.Hv_RefmeanHeightRightM.D) + 246.98);

                                            //fTD3DDistance = (float)((60 - hv_RefmeanHeightTopM.D) + (60 - hv_RefmeanHeightDownM.D) + 246.96);
                                            //fLR3DDistance = (float)((60 - hv_RefmeanHeightLeftM.D) + (60 - hv_RefmeanHeightRightM.D) + 246.94);
                                        }
                                        else
                                        {
                                            fTD3DDistance = (float)((60 - dataref.Hv_RefmeanHeightTopM.D) + (60 - dataref.Hv_RefmeanHeightDownM.D) + 256.04);
                                            fLR3DDistance = (float)((60 - dataref.Hv_RefmeanHeightLeftM.D) + (60 - dataref.Hv_RefmeanHeightRightM.D) + 256.05);

                                            //fTD3DDistance = (float)((60 - hv_RefmeanHeightTopM.D) + (60 - hv_RefmeanHeightDownM.D) + 256.04);
                                            //fLR3DDistance = (float)((60 - hv_RefmeanHeightLeftM.D) + (60 - hv_RefmeanHeightRightM.D) + 256.05);
                                        }*/


                                        LogHelper.Info("Silicon", "ThreadDealSquareAdaption  get LRLength  TDLength " );

                                        fRealTDDisWithoutRef = (float)((60 - hv_meanHeightTopM.D) + (60 - hv_meanHeightDownM.D));
                                        fCurRealTDLength = fTD3DDistance - fRealTDDisWithoutRef;

                                        fRealLRDisWithoutRef = (float)((60 - hv_meanHeightLeftM.D) + (60 - hv_meanHeightRightM.D));
                                        fCurRealLRLength = fLR3DDistance - fRealLRDisWithoutRef;

                                        //hv_meanLeftTopIndex.Dispose();
                                        //HOperatorSet.TupleMean(hv_LeftIndexesTop, out hv_meanLeftTopIndex);

                                        lengthTmp.Dispose();
                                        HOperatorSet.TupleLength(hv_LeftIndexesTop, out lengthTmp);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesTop, 0, lengthTmp / 3, out HTuple leftIndexesTopF);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesTop, lengthTmp / 3, 2 * lengthTmp / 3, out HTuple leftIndexesTopS);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesTop, 2 * lengthTmp / 3, lengthTmp - 1, out HTuple leftIndexesTopT);

                                        HOperatorSet.TupleMean(leftIndexesTopF, out hv_LeftIndexesTopF);
                                        HOperatorSet.TupleMean(leftIndexesTopS, out hv_LeftIndexesTopS);
                                        HOperatorSet.TupleMean(leftIndexesTopT, out hv_LeftIndexesTopT);

                                       
                                        hv_meanLeftTopIndex.Dispose();
                                        HOperatorSet.TupleConcat(hv_meanLeftTopIndex, hv_LeftIndexesTopF, out hv_meanLeftTopIndex);
                                        HOperatorSet.TupleConcat(hv_meanLeftTopIndex, hv_LeftIndexesTopS, out hv_meanLeftTopIndex);
                                        HOperatorSet.TupleConcat(hv_meanLeftTopIndex, hv_LeftIndexesTopT, out hv_meanLeftTopIndex);


                                        //hv_meanRightTopIndex.Dispose();
                                        //HOperatorSet.TupleMean(hv_RightIndexesTop, out hv_meanRightTopIndex);


                                        lengthTmp.Dispose();
                                        HOperatorSet.TupleLength(hv_RightIndexesTop, out lengthTmp);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesTop, 0, lengthTmp / 3, out HTuple rightIndexesTopF);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesTop, lengthTmp / 3, 2 * lengthTmp / 3, out HTuple rightIndexesTopS);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesTop, 2 * lengthTmp / 3, lengthTmp - 1, out HTuple rightIndexesTopT);

                                        HOperatorSet.TupleMean(rightIndexesTopF, out hv_RightIndexesTopF);
                                        HOperatorSet.TupleMean(rightIndexesTopS, out hv_RightIndexesTopS);
                                        HOperatorSet.TupleMean(rightIndexesTopT, out hv_RightIndexesTopT);

                                        hv_meanRightTopIndex.Dispose();
                                        HOperatorSet.TupleConcat(hv_meanRightTopIndex, hv_RightIndexesTopF, out hv_meanRightTopIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightTopIndex, hv_RightIndexesTopS, out hv_meanRightTopIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightTopIndex, hv_RightIndexesTopT, out hv_meanRightTopIndex);

                                        //hv_meanLeftDownIndex.Dispose();
                                        //HOperatorSet.TupleMean(hv_LeftIndexesLeft, out hv_meanLeftDownIndex);

                                        lengthTmp.Dispose();
                                        HOperatorSet.TupleLength(hv_LeftIndexesLeft, out lengthTmp);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesLeft, 0, lengthTmp / 3, out HTuple leftIndexesLeftF);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesLeft, lengthTmp / 3, 2 * lengthTmp / 3, out HTuple leftIndexesLeftS);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesLeft, 2 * lengthTmp / 3, lengthTmp - 1, out HTuple leftIndexesLeftT);

                                        HOperatorSet.TupleMean(leftIndexesLeftF, out hv_LeftIndexesLeftF);
                                        HOperatorSet.TupleMean(leftIndexesLeftS, out hv_LeftIndexesLeftS);
                                        HOperatorSet.TupleMean(leftIndexesLeftT, out hv_LeftIndexesLeftT);


                                        hv_meanLeftDownIndex.Dispose();
                                        HOperatorSet.TupleConcat(hv_meanLeftDownIndex, hv_LeftIndexesLeftF, out hv_meanLeftDownIndex);
                                        HOperatorSet.TupleConcat(hv_meanLeftDownIndex, hv_LeftIndexesLeftS, out hv_meanLeftDownIndex);
                                        HOperatorSet.TupleConcat(hv_meanLeftDownIndex, hv_LeftIndexesLeftT, out hv_meanLeftDownIndex);


                                        //hv_meanRightDownIndex.Dispose();
                                        //HOperatorSet.TupleMean(hv_RightIndexesRight, out hv_meanRightDownIndex);


                                        lengthTmp.Dispose();
                                        HOperatorSet.TupleLength(hv_RightIndexesRight, out lengthTmp);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesRight, 0, lengthTmp / 3, out HTuple rightIndexesRightF);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesRight, lengthTmp / 3, 2 * lengthTmp / 3, out HTuple rightIndexesRightS);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesRight, 2 * lengthTmp / 3, lengthTmp - 1, out HTuple rightIndexesRightT);

                                        HOperatorSet.TupleMean(rightIndexesRightF, out hv_RightIndexesRightF);
                                        HOperatorSet.TupleMean(rightIndexesRightS, out hv_RightIndexesRightS);
                                        HOperatorSet.TupleMean(rightIndexesRightT, out hv_RightIndexesRightT);

                                        hv_meanRightDownIndex.Dispose();
                                        HOperatorSet.TupleConcat(hv_meanRightDownIndex, hv_RightIndexesRightF, out hv_meanRightDownIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightDownIndex, hv_RightIndexesRightS, out hv_meanRightDownIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightDownIndex, hv_RightIndexesRightT, out hv_meanRightDownIndex);


                                        //hv_meanRightLeftIndex.Dispose();
                                        //HOperatorSet.TupleMean(hv_LeftIndexesRight, out hv_meanRightLeftIndex);

                                        lengthTmp.Dispose();
                                        HOperatorSet.TupleLength(hv_LeftIndexesRight, out lengthTmp);
                                        HOperatorSet.TupleSelectRange(hv_LeftIndexesRight, 0, lengthTmp / 3, out HTuple leftIndexesRightF);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesRight, lengthTmp / 3, 2 * lengthTmp / 3, out HTuple leftIndexesRightS);
                                        HOperatorSet.TupleSelectRange(hv_RightIndexesRight, 2 * lengthTmp / 3, lengthTmp - 1, out HTuple leftIndexesRightT);

                                        HOperatorSet.TupleMean(rightIndexesRightF, out hv_RightIndexesRightF);
                                        HOperatorSet.TupleMean(rightIndexesRightS, out hv_RightIndexesRightS);
                                        HOperatorSet.TupleMean(rightIndexesRightT, out hv_RightIndexesRightT);

                                        hv_meanRightLeftIndex.Dispose();
                                        HOperatorSet.TupleConcat(hv_meanRightLeftIndex, hv_RightIndexesRightF, out hv_meanRightLeftIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightLeftIndex, hv_RightIndexesRightS, out hv_meanRightLeftIndex);
                                        HOperatorSet.TupleConcat(hv_meanRightLeftIndex, hv_RightIndexesRightT, out hv_meanRightLeftIndex);

                                        hv_TopLeftSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_TopLeftSub = hv_meanLeftTopIndex - dataref.Hv_meanRefLeftTopIndex;
                                            //hv_TopLeftSub = hv_meanLeftTopIndex - hv_meanRefLeftTopIndex;
                                        }
                                        hv_TopRightSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_TopRightSub = hv_meanRightTopIndex - dataref.Hv_meanRefRightTopIndex;
                                            //hv_TopRightSub = hv_meanRightTopIndex - hv_meanRefRightTopIndex;
                                        }

                                        hv_LeftDownSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_LeftDownSub = hv_meanLeftDownIndex - dataref.Hv_meanRefLeftDownIndex;
                                            //hv_LeftDownSub = hv_meanLeftDownIndex - hv_meanRefLeftDownIndex;
                                        }
                                        hv_RightDownSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_RightDownSub = hv_meanRightDownIndex - dataref.Hv_meanRefRightDownIndex;
                                            //hv_RightDownSub = hv_meanRightDownIndex - hv_meanRefRightDownIndex;
                                        }
                                        hv_RightTopSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_RightTopSub = hv_meanRightLeftIndex - dataref.Hv_meanRefRightLeftIndex;
                                            //hv_RightTopSub = hv_meanRightLeftIndex - hv_meanRefRightLeftIndex;
                                        }
                                        //subMidr := (DiagRightLength - RefDiagRightLength) / fRatioMean

                                        //RightDownSub := subMidr + RightTopSub
                                        hv_meanHeightSubTopR.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_meanHeightSubTopR = hv_meanHeightTopR - dataref.Hv_RefmeanHeightTopR;
                                            //hv_meanHeightSubTopR = hv_meanHeightTopR - hv_RefmeanHeightTopR;
                                        }
                                        hv_meanHeightSubTopL.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_meanHeightSubTopL = hv_meanHeightTopL - dataref.Hv_RefmeanHeightTopL;
                                            //hv_meanHeightSubTopL = hv_meanHeightTopL - hv_RefmeanHeightTopL;
                                        }
                                        hv_meanHeightSubLeftD.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_meanHeightSubLeftD = hv_meanHeightLeftL - dataref.Hv_RefmeanHeightLeftL;
                                            //hv_meanHeightSubLeftD = hv_meanHeightTopL - hv_RefmeanHeightTopL;
                                        }

                                        hv_subTopL.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subTopL = (hv_meanHeightTopM - dataref.Hv_RefmeanHeightTopM) + (hv_meanHeightTopR - hv_meanHeightTopM);
                                            //hv_subTopL = (hv_meanHeightTopM - hv_RefmeanHeightTopM) + (hv_meanHeightTopR - hv_meanHeightTopM);
                                        }
                                        hv_subTopR.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subTopR = (hv_meanHeightTopM - dataref.Hv_RefmeanHeightTopM) + (hv_meanHeightTopL - hv_meanHeightTopM);
                                            //hv_subTopR = (hv_meanHeightTopM - hv_RefmeanHeightTopM) + (hv_meanHeightTopL - hv_meanHeightTopM);
                                        }
                                        //subTopL := meanHeightTopR - RefmeanHeightTopR
                                        //subTopR := meanHeightTopL - RefmeanHeightTopL

                                        hv_subLeftD.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subLeftD = hv_meanHeightLeftL - dataref.Hv_RefmeanHeightLeftL;
                                            //hv_subLeftD = hv_meanHeightLeftL - hv_RefmeanHeightLeftL;
                                        }
                                        hv_subRightD.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subRightD = hv_meanHeightRightR - dataref.Hv_RefmeanHeightRightR;
                                            //hv_subRightD = hv_meanHeightRightR - hv_RefmeanHeightRightR;
                                        }

                                        //subLeft := (meanHeightLeftM - RefmeanHeightLeftM)
                                        //subRight := meanHeightRightM - RefmeanHeightRightM
                                        hv_subLeft.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            //hv_subLeft = (hv_meanHeightLeftM - dataref.Hv_RefmeanHeightLeftM) + (hv_meanHeightLeftL - hv_meanHeightLeftM);
                                            hv_subLeft = (hv_meanHeightLeftM - dataref.Hv_RefmeanHeightLeftM);


                                            //hv_subLeft = (hv_meanHeightLeftM - hv_RefmeanHeightLeftM) + (hv_meanHeightLeftL - hv_meanHeightLeftM);
                                        }
                                        hv_subRight.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            //hv_subRight = (hv_meanHeightRightM - dataref.Hv_RefmeanHeightRightM) + (hv_meanHeightRightR - hv_meanHeightRightM);

                                            hv_subRight = (hv_meanHeightRightM - dataref.Hv_RefmeanHeightRightM);


                                            //hv_subRight = (hv_meanHeightRightM - hv_RefmeanHeightRightM) + (hv_meanHeightRightR - hv_meanHeightRightM);
                                        }
                                        //hv_subDown.Dispose();
                                        //using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        //{
                                        //    hv_subDown = hv_meanHeightDownM - dataref.Hv_RefmeanHeightDownM;
                                        //    //hv_subDown = hv_meanHeightDownM - hv_RefmeanHeightDownM;
                                        //}
                                        LogHelper.Info("Silicon", "ThreadDealSquareAdaption  get CheckLengthAscendnewWithoutRulerSmallDiagSplit Begin  paraInfo.nSquareType " + paraInfo.nSquareType.ToString());

                                        if (paraInfo.nSquareType == 4 || paraInfo.nSquareType == 16 || paraInfo.nSquareType == 24)
                                        {
                                            CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRulerSmallDiagSplit(hv_ResultdictHandle, fCurRealTDLength, fCurRealLRLength, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLF, hv_Surface3DDefaultLS, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength, dataref.Hv_fPreRTHeight, dataref.Hv_fPreRTWidth, dataref.Hv_fPreLTHeight, dataref.Hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, 0);
                                        }
                                        else
                                        {
                                            CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRulerSmallDiagSplit(hv_ResultdictHandle, fCurRealTDLength, fCurRealLRLength, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLF, hv_Surface3DDefaultLS, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength, dataref.Hv_fPreRTHeight, dataref.Hv_fPreRTWidth, dataref.Hv_fPreLTHeight, dataref.Hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, 1);
                                        }
                                        LogHelper.Info("Silicon", "ThreadDealSquareAdaption  get CheckLengthAscendnewWithoutRulerSmallDiagSplit End  paraInfo.nSquareType " + paraInfo.nSquareType.ToString());
                                        /*
                                        if (hv_DiagTopLength > 9)
                                        {
                                            if (hv_DiagTopLength < 11.5)
                                            {
                                                CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRulerSmallDiagSplit(hv_ResultdictHandle, fCurRealTDLength, fCurRealLRLength, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLF, hv_Surface3DDefaultLS, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength, dataref.Hv_fPreRTHeight, dataref.Hv_fPreRTWidth, dataref.Hv_fPreLTHeight, dataref.Hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, 0);
                                                //CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRulerSmallDiag(hv_ResultdictHandle, fCurRealTDLength, fCurRealLRLength, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLF, hv_Surface3DDefaultLS, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength, dataref.Hv_fPreRTHeight, dataref.Hv_fPreRTWidth, dataref.Hv_fPreLTHeight, dataref.Hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, 0);
                                            }
                                            else
                                            {
                                                CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRulerSmallDiagSplit(hv_ResultdictHandle, fCurRealTDLength, fCurRealLRLength, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLF, hv_Surface3DDefaultLS, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength, dataref.Hv_fPreRTHeight, dataref.Hv_fPreRTWidth, dataref.Hv_fPreLTHeight, dataref.Hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, 1);
                                                //CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRulerSmallDiag(hv_ResultdictHandle, fCurRealTDLength, fCurRealLRLength, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLF, hv_Surface3DDefaultLS, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength, dataref.Hv_fPreRTHeight, dataref.Hv_fPreRTWidth, dataref.Hv_fPreLTHeight, dataref.Hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, 1);
                                            }
                                            //CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRuler(hv_HomMat2DTDAdapted, hv_HomMat2DDTAdapted, hv_HomMat2DLRAdapted, hv_HomMat2DRLAdapted, hv_meanMidr, hv_meanMidl, hv_ResultdictHandle, hv_heightDT3D, hv_widthRL3D, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLS, hv_Surface3DDefaultLF, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength, hv_LeftIndexesTop, hv_RightIndexesTop, hv_LeftIndexesLeft, hv_RightIndexesLeft, hv_LeftIndexesRight, hv_RightIndexesRight, hv_LeftIndexesDown, hv_RightIndexesDown, hv_rowLT3D, hv_colLT3D, hv_rowRT3D, hv_colRT3D, hv_fPreRTHeight, hv_fPreRTWidth, hv_fPreLTHeight, hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, hv_subDown);
                                        }
                                        else
                                        {
                                            //hv_subLeft = hv_meanHeightLeftM - hv_RefmeanHeightLeftM;
                                            //hv_subRight = hv_meanHeightRightM - hv_RefmeanHeightRightM;
                                            //hv_subTop = hv_meanHeightTopM - hv_RefmeanHeightTopM;
                                            hv_subLeft = hv_meanHeightLeftM - dataref.Hv_RefmeanHeightLeftM;
                                            hv_subRight = hv_meanHeightRightM - dataref.Hv_RefmeanHeightRightM;
                                            hv_subTop = hv_meanHeightTopM - dataref.Hv_RefmeanHeightTopM;

                                            CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRulerSmallDiagSplit(hv_ResultdictHandle, fCurRealTDLength, fCurRealLRLength, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLF, hv_Surface3DDefaultLS, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength, dataref.Hv_fPreRTHeight, dataref.Hv_fPreRTWidth, dataref.Hv_fPreLTHeight, dataref.Hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, 2);
                                            //CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRulerSmallDiag(hv_ResultdictHandle, fCurRealTDLength, fCurRealLRLength, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLF, hv_Surface3DDefaultLS, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength, dataref.Hv_fPreRTHeight, dataref.Hv_fPreRTWidth, dataref.Hv_fPreLTHeight, dataref.Hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, 2);
                                        }
                                        */
                                        //CGlobalSquareFuncTools.Instance().CheckLengthAscendnew(hv_fLRRatio, hv_fTDRatio, hv_HomMat2DTDAdapted, hv_HomMat2DTLAdapted, hv_HomMat2DTRAdapted, hv_HomMat2DDTAdapted, hv_HomMat2DRAdapted, hv_HomMat2DLAdapted, hv_HomMat2DLDAdapted, hv_HomMat2DLRAdapted, hv_HomMat2DLTAdapted, hv_HomMat2DRDAdapted, hv_HomMat2DRLAdapted, hv_HomMat2DRTAdapted, hv_meanMidt, hv_meanMidr, hv_meanMidd, hv_meanMidl, hv_ResultdictHandle, hv_heightDT3D, hv_widthRL3D, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLS, hv_Surface3DDefaultLF, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_StickImageHeight);




                                        LogHelper.Info("Silicon", "checkLengthNew  End ");

                                        //CGlobalSquareFuncTools.Instance().checkLength(hobjectTopStick, hobjectLeftStick, hobjectRightStick, hobjectDownStick, hv_fLDRatio, hv_fRDRatio, hv_fLRRatio, hv_fTDRatio, hv_HomMat2DTDAdapted, hv_HomMat2DTLAdapted, hv_HomMat2DTRAdapted, hv_HomMat2DDTAdapted, hv_HomMat2DRAdapted, hv_HomMat2DLAdapted, hv_HomMat2DLDAdapted, hv_HomMat2DLRAdapted, hv_HomMat2DLTAdapted, hv_HomMat2DRDAdapted, hv_HomMat2DRLAdapted, hv_HomMat2DRTAdapted, hv_meanMidt, hv_meanMidr, hv_meanMidd, hv_meanMidl, hv_rowRT3D, hv_colRT3D, hv_rowRD3D, hv_colRD3D, hv_rowLT3D, hv_colLT3D, hv_rowLD3D, hv_colLD3D, hv_ResultdictHandle, hv_topIndex, hv_leftIndex, hv_rightIndex, hv_downIndex, hv_heightDT3D, hv_widthRL3D);

                                        SquareStickCheckData data = new SquareStickCheckData();

                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LT", out HTuple hv_ltlength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RT", out HTuple hv_rtlength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LD", out HTuple hv_ldlength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RD", out HTuple hv_rdlength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LR", out HTuple hv_lrlength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "TD", out HTuple hv_tdlength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LTN", out HTuple hv_meanLTNDisplay);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RTN", out HTuple hv_meanRTNDisplay);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LTNew", out HTuple hv_meanLTNewDisplay);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RTNew", out HTuple hv_meanRTNewDisplay);

                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LDiag", out HTuple hv_ldiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RDiag", out HTuple hv_rdiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "TDiag", out HTuple hv_tdiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "DDiag", out HTuple hv_ddiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "TLDiag", out HTuple hv_tldiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "TRDiag", out HTuple hv_trdiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LLDiag", out HTuple hv_lldiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LRDiag", out HTuple hv_lrdiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RLDiag", out HTuple hv_rldiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RRDiag", out HTuple hv_rrdiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "DLDiag", out HTuple hv_dldiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "DRDiag", out HTuple hv_drdiaglength);

                                        data.FLength = (float)hv_StickLength.D;

                                        for (int j = 0; j < hv_meanLTNewDisplay.Length; j++)
                                        {
                                            data.ListLTLength.Add(hv_meanLTNewDisplay.DArr[j]);
                                        }



                                        for (int j = 0; j < hv_meanRTNewDisplay.Length; j++)
                                        {
                                            data.ListRTLength.Add(hv_meanRTNewDisplay.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_meanRTNewDisplay.Length; j++)
                                        {
                                            data.ListLDLength.Add(hv_meanRTNewDisplay.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_meanLTNewDisplay.Length; j++)
                                        {
                                            data.ListRDLength.Add(hv_meanLTNewDisplay.DArr[j]);
                                        }
                                        HTuple hrand = new HTuple();

                                        data.ListTDLength.Clear();
                                        data.ListLRLength.Clear();
                                        for (int j = 0; j < hv_tdlength.Length; j++)
                                        {
                                            hrand.Dispose();
                                            HOperatorSet.TupleRand(1, out hrand);
                                            data.ListTDLength.Add(/*hv_tdlength.DArr[j]*/fCurRealTDLength + (float)(hrand.D * 0.02));
                                        }


                                        for (int j = 0; j < hv_lrlength.Length; j++)
                                        {
                                            hrand.Dispose();
                                            HOperatorSet.TupleRand(1, out hrand);
                                            data.ListLRLength.Add(/*hv_lrlength.DArr[j]*/fCurRealLRLength + (float)(hrand.D * 0.02));
                                        }

                                        try
                                        {
                                            if (data.FLength > 500)
                                            {
                                                data.ListLTLength.Add(hv_meanLTNewDisplay.DArr[0]);
                                                data.ListLTLength.Add(hv_meanLTNewDisplay.DArr[1]);

                                                data.ListRTLength.Add(hv_meanRTNewDisplay.DArr[0]);
                                                data.ListRTLength.Add(hv_meanRTNewDisplay.DArr[1]);

                                                data.ListLDLength.Add(hv_meanRTNewDisplay.DArr[0]);
                                                data.ListLDLength.Add(hv_meanRTNewDisplay.DArr[1]);

                                                data.ListRDLength.Add(hv_meanLTNewDisplay.DArr[0]);
                                                data.ListRDLength.Add(hv_meanLTNewDisplay.DArr[1]);
                                                for (int j = 0; j < 2; j++)
                                                {
                                                    hrand.Dispose();
                                                    HOperatorSet.TupleRand(1, out hrand);
                                                    data.ListTDLength.Add(/*hv_tdlength.DArr[j]*/fCurRealTDLength + (float)(hrand.D * 0.02));
                                                }


                                                for (int j = 0; j < 2; j++)
                                                {
                                                    hrand.Dispose();
                                                    HOperatorSet.TupleRand(1, out hrand);
                                                    data.ListLRLength.Add(/*hv_lrlength.DArr[j]*/fCurRealLRLength + (float)(hrand.D * 0.02));
                                                }
                                              
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }


                                        for (int j = 0; j < hv_DiagLeftLength.Length; j++)
                                        {
                                            data.ListLeftDiagLength.Add(hv_DiagLeftLength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_DiagRightLength.Length; j++)
                                        {

                                            data.ListRightDiagLength.Add(hv_DiagRightLength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_DiagTopLength.Length; j++)
                                        {
                                            data.ListTopDiagLength.Add(hv_DiagTopLength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_DiagDownLength.Length; j++)
                                        {
                                            data.ListDownDiagLength.Add(hv_DiagDownLength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_DiagLeftLength.Length; j++)
                                        {
                                            data.ListLeftLeftDiagLength.Add(hv_DiagLeftLength.DArr[j] * 0.707);
                                        }


                                        for (int j = 0; j < hv_DiagLeftLength.Length; j++)
                                        {
                                            data.ListLeftRightDiagLength.Add(hv_DiagLeftLength.DArr[j] * 0.707);
                                        }


                                        for (int j = 0; j < hv_DiagRightLength.Length; j++)
                                        {
                                            data.ListRightLeftDiagLength.Add(hv_DiagRightLength.DArr[j] * 0.707);
                                        }

                                        for (int j = 0; j < hv_DiagRightLength.Length; j++)
                                        {
                                            data.ListRightRightDiagLength.Add(hv_DiagRightLength.DArr[j] * 0.707);
                                        }

                                        for (int j = 0; j < hv_DiagDownLength.Length; j++)
                                        {
                                            data.ListDownLeftDiagLength.Add(hv_DiagDownLength.DArr[j] * 0.707);
                                        }

                                        for (int j = 0; j < hv_DiagDownLength.Length; j++)
                                        {
                                            data.ListDownRightDiagLength.Add(hv_DiagDownLength.DArr[j] * 0.707);
                                        }

                                        for (int j = 0; j < hv_DiagTopLength.Length; j++)
                                        {
                                            data.ListTopLeftDiagLength.Add(hv_DiagTopLength.DArr[j] * 0.707);
                                        }

                                        for (int j = 0; j < hv_DiagTopLength.Length; j++)
                                        {
                                            data.ListTopRightDiagLength.Add(hv_DiagTopLength.DArr[j] * 0.707);
                                        }




                                        data.ListTopAngle.Add(90 - hv_topAngleOfTwoLineT.D);
                                        data.ListTopAngle.Add(90 - hv_midAngleOfTwoLineT.D);
                                        data.ListTopAngle.Add(90 - hv_downAngleOfTwoLineT.D);

                                        data.ListDownAngle.Add(90 - hv_topAngleOfTwoLineD.D);
                                        data.ListDownAngle.Add(90 - hv_midAngleOfTwoLineD.D);
                                        data.ListDownAngle.Add(90 - hv_downAngleOfTwoLineD.D);

                                        data.ListLeftAngle.Add(90 - hv_topAngleOfTwoLineL.D);
                                        data.ListLeftAngle.Add(90 - hv_midAngleOfTwoLineL.D);
                                        data.ListLeftAngle.Add(90 - hv_downAngleOfTwoLineL.D);

                                        data.ListRightAngle.Add(90 - hv_topAngleOfTwoLineR.D);
                                        data.ListRightAngle.Add(90 - hv_midAngleOfTwoLineR.D);
                                        data.ListRightAngle.Add(90 - hv_downAngleOfTwoLineR.D);
                                        data.NResult = 1;
                                        data.StrJBSearial = paraInfo.strSerial;
                                        GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, data);

                                        LogHelper.Info("", "ThreadDealSquareInfo  fCurRealLRLength " + fCurRealLRLength.ToString("0.00") + " fCurRealTDLength " + fCurRealTDLength.ToString("0.00"));

                                        HTuple strJsonFile;
                                        string strFileName = SettingParameter.Instance().StrSaveDir + "/" + paraInfo.strSerial + "/result.json";
                                        string strEndFileName = SettingParameter.Instance().StrSaveDir + "/" + paraInfo.strSerial + "/end.txt";
                                        HOperatorSet.DictToJson(hv_ResultdictHandle, new HTuple(), new HTuple(), out strJsonFile);

                                        if (false == Directory.Exists(SettingParameter.Instance().StrSaveDir + "/" + paraInfo.strSerial))
                                        {
                                            Directory.CreateDirectory(SettingParameter.Instance().StrSaveDir + "/" + paraInfo.strSerial);
                                        }
                                        HTuple fileHandle = new HTuple();
                                        HTuple fileHandleend = new HTuple();
                                        HOperatorSet.OpenFile(strFileName, "output", out fileHandle);
                                        HOperatorSet.FwriteString(fileHandle, strJsonFile);
                                        HOperatorSet.CloseFile(fileHandle);
                                        HOperatorSet.OpenFile(strEndFileName, "output", out fileHandleend);
                                        HOperatorSet.CloseFile(fileHandleend);

                                        


                                        break;
                                    }
                                    catch (Exception ex)
                                    {
                                        LogHelper.Info("", "exception ThreadDealSquareInfo msg");

                                        //Thread threadMovEnd = new Thread(() =>
                                        //{
                                        //    LogHelper.Info("Silicon", "ThreadDealSquareInfo Exception Move To Orgin Position");
                                        //    CMoveController.Instance().SetMoveSpeed(100);
                                        //    CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, 120);
                                        //    //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                                        //});
                                        //threadMovEnd.Start();

                                        //if (threadMovEnd != null)
                                        //{
                                        //    while (threadMovEnd.ThreadState != ThreadState.Stopped)
                                        //    {
                                        //        Thread.Sleep(500);
                                        //    }

                                        //}
                                    }



                                }
                                else
                                {
                                    bCalculateStick = true;
                                    SquareStickCheckData datae = new SquareStickCheckData();
                                    datae.FLength = -1;
                                    datae.StrJBSearial = paraInfo.strSerial;
                                    GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, datae);


                                    threadMov = new Thread(() =>
                                    {
                                        LogHelper.Info("Silicon", "ThreadDealSquareInfo Move To Orgin Position");
                                        CMoveController.Instance().SetMoveSpeed(SettingParameter.Instance().FPLCMoveSpeed);
                                        CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, SettingParameter.Instance().FPosition_fir_s, 20);
                                        NTestStatus = emStatus.EM_FREE;
                                        //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                                    });
                                    NTestStatus = emStatus.EM_GOTOORIGIN;
                                    threadMov.Start();

                                   
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
        public void ThreadDealSquareInfo(object objtype)
        {
            HObject hObject = new HObject();

            tagParaInfo paraInfo = (tagParaInfo)objtype;
            int nWaitIndex = 0;
            //int nGrayWaitIndex = 0;
            //bool bNeedBegin = false;

            //do
            //{
            //    CMoveController.Instance().GetState(126, out bNeedBegin);
            //    Thread.Sleep(1000);
            //    LogHelper.Info("Silicon", "Wait For 126 Signal!");
            //} while (bNeedBegin == false);
            
             
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
            HTuple hv_TopRightSub = new HTuple(), hv_LeftDownSub = new HTuple();
            HTuple hv_RightDownSub = new HTuple(), hv_RightTopSub = new HTuple();
            HTuple hv_meanHeightSubTopR = new HTuple(), hv_meanHeightSubTopL = new HTuple();
            HTuple hv_meanHeightSubLeftD = new HTuple(), hv_subTopL = new HTuple();
            HTuple hv_subTop = new HTuple();
            HTuple hv_subTopR = new HTuple(), hv_subLeftD = new HTuple();
            HTuple hv_subRightD = new HTuple(), hv_subLeft = new HTuple();
            HTuple hv_subRight = new HTuple(), hv_subDown = new HTuple();

            HTuple hv_lengthstmp = new HTuple(), hv_Min = new HTuple();
            HTuple hv_Max = new HTuple(), hv_Indices = new HTuple();
            HTuple hv_fSub = new HTuple(), hv_meanLeftTopIndex = new HTuple();
            HTuple hv_fMeanLength = new HTuple();
            HTuple hv_fValue = new HTuple();
            HTuple hv_fPreRTHeight = new HTuple();
            HTuple hv_fPreRTWidth = new HTuple();
            HTuple hv_fPreLTHeight = new HTuple();
            HTuple hv_fPreLTWidth = new HTuple();
            float fTD3DDistance = 0;
            float fLR3DDistance = 0;
            float fRealTDDisWithoutRef = 0;
            float fCurRealTDLength = 0;
            float fRealLRDisWithoutRef = 0;
            float fCurRealLRLength = 0;
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
            while (true)
            {
                HOperatorSet.GenEmptyObj(out hObject);

                if (false == Directory.Exists("D:/Image1"))
                {
                    Directory.CreateDirectory("D:/Image1");
                }

                if (false ==Directory.Exists("D:/Image1/left"))
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
                for (int i = 0; i < 4; i++)
                {
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

                                        //if (true == GetGrayObjectsByType((emSSZNCamType)i, ref hGrayObject))
                                        //{
                                        //    HOperatorSet.WriteImage(hGrayObject, "tiff", 0, "D:/Image1/left/gray_" + NLeftIndex.ToString() + ".tif");

                                        //}
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
                                        //if (true == GetGrayObjectsByType((emSSZNCamType)i, ref hGrayObject))
                                        //{
                                        //    HOperatorSet.WriteImage(hGrayObject, "tiff", 0, "D:/Image1/right/gray_" + NRightIndex.ToString() + ".tif");

                                        //}
                                    }
                                    //HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image/right/" + NRightIndex++.ToString() + ".tif");
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
                                        //if (true == GetGrayObjectsByType((emSSZNCamType)i, ref hGrayObject))
                                        //{
                                        //    HOperatorSet.WriteImage(hGrayObject, "tiff", 0, "D:/Image1/top/gray_" + NTopIndex.ToString() + ".tif");

                                        //}
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
                                        //if (true == GetGrayObjectsByType((emSSZNCamType)i, ref hGrayObject))
                                        //{
                                        //    HOperatorSet.WriteImage(hGrayObject, "tiff", 0, "D:/Image1/down/gray_" + NDownIndex.ToString() + ".tif");

                                        //}
                                    }
                                    //HOperatorSet.WriteImage(hObject, "tiff", 0, "D:/Image/down/" + NDownIndex++.ToString() + ".tif");
                                    NDownIndex++;
                                    HOperatorSet.ConcatObj(_hObjectsDown, hObject, out _hObjectsDown);
                                    break;
                                }
                        }

                        LogHelper.Info("Silicon", "ThreadDealSquareInfo NLeftIndex " + NLeftIndex.ToString() + " NRightIndex " + NRightIndex.ToString() + " NTopIndex " + NTopIndex.ToString() + " NDownIndex " + NDownIndex.ToString());
                         
                        if (false == bCheckReference && NLeftIndex >= 4 && NRightIndex >= 4 && NTopIndex >= 4 && NDownIndex >= 4)
                        {
                            
                            bCheckReference = true;
                            HObject hObjectLeftReference = new HObject();
                            HObject hObjectRightReference = new HObject();
                            HObject hObjectTopReference = new HObject();
                            HObject hObjectDownReference = new HObject();
                           
                            HOperatorSet.GenEmptyObj(out hObjectLeftReference);
                            HOperatorSet.GenEmptyObj(out hObjectRightReference);
                            HOperatorSet.GenEmptyObj(out hObjectTopReference);
                            HOperatorSet.GenEmptyObj(out hObjectDownReference);

                            HOperatorSet.TileImages(_hObjectsLeft, out _ho_ImageLeft, 1, "vertical");
                            HOperatorSet.TileImages(_hObjectsRight, out _ho_ImageRight, 1, "vertical");
                            HOperatorSet.TileImages(_hObjectsTop, out _ho_ImageTop, 1, "vertical");
                            HOperatorSet.TileImages(_hObjectsDown, out _ho_ImageDown, 1, "vertical");


                            HOperatorSet.CropPart(_ho_ImageLeft, out hObjectLeftReference, 0, 0, 3200, 4000);
                            HOperatorSet.CropPart(_ho_ImageTop, out hObjectTopReference, 0, 0, 3200, 4000);
                            HOperatorSet.CropPart(_ho_ImageRight, out hObjectRightReference, 0, 0, 3200, 4000);
                            HOperatorSet.CropPart(_ho_ImageDown, out hObjectDownReference, 0, 0, 3200, 4000);

                            threaddeal = new Thread(() =>
                            {
                                try
                                {
                                   
                                    CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectTopReference, hv_fRatioT, out hv_RefDiagTopLength,out hv_RefLeftIndexesTop, out hv_RefRightIndexesTop, out hv_RefRowBeginTop,out hv_RefmeanHeightTopL, out hv_RefmeanHeightTopM, out hv_RefmeanHeightTopR);

                                    CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectLeftReference, hv_fRatioL, out hv_RefDiagLeftLength,out hv_RefLeftIndexesLeft, out hv_RefRightIndexesLeft, out hv_RefRowBeginLeft, out hv_RefmeanHeightLeftL, out hv_RefmeanHeightLeftM, out hv_RefmeanHeightLeftR);


                                    CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectRightReference, hv_fRatioR, out hv_RefDiagRightLength,out hv_RefLeftIndexesRight, out hv_RefRightIndexesRight, out hv_RefRowBeginRight,out hv_RefmeanHeightRightL, out hv_RefmeanHeightRightM, out hv_RefmeanHeightRightR);

                                    CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hObjectDownReference, hv_fRatioD, out hv_RefDiagDownLength,out hv_RefLeftIndexesDown, out hv_RefRightIndexesDown, out hv_RefRowBeginDown,out hv_RefmeanHeightDownL, out hv_RefmeanHeightDownM, out hv_RefmeanHeightDownR);
                                    
                                   

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
                                    

                                    if (hv_RefDiagTopLength > 9)
                                    {
                                        CGlobalSquareFuncToolsNew.Instance().CheckReferenceObjectWithoutRuler(hObjectTopReference, hObjectLeftReference, hObjectRightReference, hObjectDownReference, hv_RefRowBeginTop, hv_RefRowBeginLeft, hv_RefRowBeginRight, hv_RefRowBeginDown, hv_RefLeftIndexesTop, hv_RefRightIndexesTop, hv_RefLeftIndexesDown, hv_RefRightIndexesDown, hv_RefLeftIndexesLeft, hv_RefRightIndexesLeft, hv_RefLeftIndexesRight, hv_RefRightIndexesRight, hv_RefDiagTopLength, hv_RefDiagLeftLength, hv_RefDiagRightLength, hv_RefDiagDownLength, out hv_HomMat2DTDAdapted, out hv_HomMat2DDTAdapted, out hv_HomMat2DLRAdapted, out hv_HomMat2DRLAdapted, out hv_meanMidr, out hv_meanMidl, out hv_rowRT3D, out hv_colRT3D, out hv_rowLT3D, out hv_colLT3D, out hv_heightDT3D, out hv_widthRL3D, out hv_fPreRTHeight, out hv_fPreRTWidth, out hv_fPreLTHeight, out hv_fPreLTWidth);
                                        //CGlobalSquareFuncTools.Instance().CheckReferenceObject(hObjectTopReference, hObjectLeftReference, hObjectRightReference, hObjectDownReference, out hv_HomMat2DTDAdapted, out  hv_HomMat2DTLAdapted, out  hv_HomMat2DTRAdapted, out  hv_HomMat2DDTAdapted, out  hv_HomMat2DLAdapted, out  hv_HomMat2DRAdapted, out  hv_HomMat2DLDAdapted, out  hv_HomMat2DLRAdapted, out  hv_HomMat2DLTAdapted, out  hv_HomMat2DRDAdapted, out  hv_HomMat2DRLAdapted, out  hv_HomMat2DRTAdapted, out  hv_fLTRatio, out  hv_fLDRatio, out  hv_fRTRatio, out  hv_fRDRatio, out  hv_fLRRatio, out  hv_fTDRatio, out  hv_fTopDiagRatio, out  hv_fDownDiagRatio, out  hv_fLeftDiagRatio, out  hv_fRightDiagRatio, out  hv_meanMidt, out  hv_meanMidr, out  hv_meanMidd, out  hv_meanMidl, out  hv_rowRT3D, out  hv_colRT3D, out  hv_rowRD3D, out  hv_colRD3D, out  hv_rowLT3D, out  hv_colLT3D, out  hv_rowLD3D, out  hv_colLD3D, out  hv_heightDT3D, out  hv_widthRL3D);
                                    }
                                    else
                                    {
                                        CGlobalSquareFuncToolsNew.Instance().checkReferenceNewByRelatively(hObjectTopReference, hObjectLeftReference, hObjectRightReference, hObjectDownReference, hv_RefRowBeginTop, hv_RefRowBeginLeft, hv_RefRowBeginRight, hv_RefRowBeginDown, hv_RefLeftIndexesTop, hv_RefRightIndexesTop, hv_RefLeftIndexesDown, hv_RefRightIndexesDown, hv_RefLeftIndexesLeft, hv_RefRightIndexesLeft, hv_RefLeftIndexesRight, hv_RefRightIndexesRight,  hv_RefmeanHeightTopR, hv_RefmeanHeightDownR, hv_RefmeanHeightLeftR, hv_RefmeanHeightRightR,  out hv_fPreRTHeight, out hv_fPreRTWidth, out hv_fPreLTHeight, out hv_fPreLTWidth);
                                    }

                                }
                                catch (Exception ex)
                                {

                                }
                                

                            });

                            threaddeal.Start();
                        }

                        nWaitIndex = 0;
                    }
                    else
                    {
                        if (bValidWait ==true)
                        {
                            nWaitIndex++;
                            if (nWaitIndex >= 8)
                            {
                                LogHelper.Info("Silicon", "camtype " + paraInfo.camtype.ToString() + " No Message , break");

                                if (NLeftIndex >= 4 && NRightIndex >= 4 && NTopIndex >= 4 && NDownIndex >= 4)
                                {
                                    try
                                    {
                                        if (SettingParameter.Instance().NCamType == 0)
                                        {
                                            SSZNCamTools.Instance().StopBatchLoop();
                                        }
                                        else
                                        {
                                            XGCamTools.Instance.StopCaptures();
                                        }

                                        Thread threadMov = new Thread(() =>
                                        {
                                            LogHelper.Info("Silicon", "ThreadDealSquareInfo Move To Orgin Position");
                                            CMoveController.Instance().SetMoveSpeed(SettingParameter.Instance().FPLCMoveSpeed);
                                            CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, SettingParameter.Instance().FPosition_fir_s, 20);
                                            //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                                        });
                                        if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
                                        {
                                            threadMov.Start();
                                        }
                                        bCalculateStick = true;

                                        HObject hobjectLeftStick = new HObject();
                                        HObject hobjectRightStick = new HObject();
                                        HObject hobjectTopStick = new HObject();
                                        HObject hobjectDownStick = new HObject();

                                        HOperatorSet.GenEmptyObj(out hobjectLeftStick);
                                        HOperatorSet.GenEmptyObj(out hobjectDownStick);
                                        HOperatorSet.GenEmptyObj(out hobjectRightStick);
                                        HOperatorSet.GenEmptyObj(out hobjectTopStick);

                                        HOperatorSet.TileImages(_hObjectsLeft, out _ho_ImageLeft, 1, "vertical");
                                        HOperatorSet.TileImages(_hObjectsRight, out _ho_ImageRight, 1, "vertical");
                                        HOperatorSet.TileImages(_hObjectsTop, out _ho_ImageTop, 1, "vertical");
                                        HOperatorSet.TileImages(_hObjectsDown, out _ho_ImageDown, 1, "vertical");

                                        HOperatorSet.GetImageSize(_ho_ImageLeft, out HTuple widthleft, out HTuple heightleft);
                                        HOperatorSet.GetImageSize(_ho_ImageTop, out HTuple widthtop, out HTuple heighttop);
                                        HOperatorSet.GetImageSize(_ho_ImageRight, out HTuple widthright, out HTuple heightright);
                                        HOperatorSet.GetImageSize(_ho_ImageDown, out HTuple widthdown, out HTuple heightdown);


                                        HOperatorSet.CropPart(_ho_ImageLeft, out hobjectLeftStick, 4000, 0, 3200, heightleft - 4000);
                                        HOperatorSet.CropPart(_ho_ImageTop, out hobjectTopStick, 4000, 0, 3200, heighttop - 4000);
                                        HOperatorSet.CropPart(_ho_ImageDown, out hobjectDownStick, 4000, 0, 3200, heightdown - 4000);
                                        HOperatorSet.CropPart(_ho_ImageRight, out hobjectRightStick, 4000, 0, 3200, heightright - 4000);


                                        HTuple hv_ResultdictHandle = new HTuple();
                                        HOperatorSet.CreateDict(out hv_ResultdictHandle);

                                        if (threaddeal != null)
                                        {
                                            while (threaddeal.ThreadState != ThreadState.Stopped)
                                            {
                                                Thread.Sleep(500);
                                            }

                                        }




                                        //CGlobalSquareFuncTools.Instance().checkSquareStickLengthInverse(hobjectTopStick, hobjectLeftStick, hobjectRightStick, hobjectDownStick, hv_ResultdictHandle, out HTuple hv_StickLength, out HTuple hv_topIndex, out HTuple hv_leftIndex, out HTuple hv_rightIndex, out HTuple hv_downIndex, out HTuple hv_Surface3DDefaultT, out HTuple hv_Surface3DDefaultD, out HTuple hv_Surface3DDefaultL, out HTuple hv_Surface3DDefaultR, out HTuple hv_StickImageHeight);
                                        // CGlobalSquareFuncTools.Instance().checkSquareStickLength(hobjectTopStick, hobjectLeftStick, hobjectRightStick, hobjectDownStick, hv_ResultdictHandle, out HTuple hv_StickLength, out HTuple hv_topIndex, out HTuple hv_leftIndex, out HTuple hv_rightIndex, out HTuple hv_downIndex);

                                        if (hv_RefDiagTopLength > 9)
                                        {
                                            if (false == CGlobalSquareFuncToolsNew.Instance().checkSquareStickLengthInverseAscend(hobjectTopStick, hobjectLeftStick, hobjectRightStick, hobjectDownStick, hv_ResultdictHandle, out hv_StickLength, out hv_Surface3DDefaultTF, out hv_Surface3DDefaultTS, out hv_Surface3DDefaultTT, out hv_Surface3DDefaultDF, out hv_Surface3DDefaultDS, out hv_Surface3DDefaultDT, out hv_Surface3DDefaultLF, out hv_Surface3DDefaultLS, out hv_Surface3DDefaultLT, out hv_Surface3DDefaultRF, out hv_Surface3DDefaultRS, out hv_Surface3DDefaultRT, out hv_StickImageHeight, out hv_firstTopIndexRow, out hv_secondTopIndexRow, out hv_thirdTopIndexRow, out hv_firstDowntIndexRow, out hv_secondDownIndexRow, out hv_thirdDownIndexRow, out hv_firstRightIndexRow, out hv_secondRightIndexRow, out hv_thirdRightIndexRow, out hv_firstLeftIndexRow, out hv_secondLeftIndexRow, out hv_thirdLeftIndexRow))
                                            {
                                                LogHelper.Info("Silicon", "checkSquareStickLengthInverseAscend  Error " + hv_StickLength.D.ToString("0.00"));
                                                SquareStickCheckData datae = new SquareStickCheckData();
                                                datae.FLength = -1;
                                                datae.StrJBSearial = paraInfo.strSerial;
                                                GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, datae);
                                                return;
                                            }

                                        }
                                        else
                                        {
                                            if (false == CGlobalSquareFuncToolsNew.Instance().checkSquareStickLengthInversAscendSmallDiag(hobjectTopStick, hobjectLeftStick, hobjectRightStick, hobjectDownStick, hv_ResultdictHandle, out hv_StickLength, out hv_Surface3DDefaultTF, out hv_Surface3DDefaultTS, out hv_Surface3DDefaultTT, out hv_Surface3DDefaultDF, out hv_Surface3DDefaultDS, out hv_Surface3DDefaultDT, out hv_Surface3DDefaultLF, out hv_Surface3DDefaultLS, out hv_Surface3DDefaultLT, out hv_Surface3DDefaultRF, out hv_Surface3DDefaultRS, out hv_Surface3DDefaultRT, out hv_StickImageHeight, out hv_firstTopIndexRow, out hv_secondTopIndexRow, out hv_thirdTopIndexRow, out hv_firstDowntIndexRow, out hv_secondDownIndexRow, out hv_thirdDownIndexRow, out hv_firstRightIndexRow, out hv_secondRightIndexRow, out hv_thirdRightIndexRow, out hv_firstLeftIndexRow, out hv_secondLeftIndexRow, out hv_thirdLeftIndexRow))
                                            {
                                                LogHelper.Info("Silicon", "checkSquareStickLengthInverseAscend  Error " + hv_StickLength.D.ToString("0.00"));
                                                SquareStickCheckData datae = new SquareStickCheckData();
                                                datae.FLength = -1;
                                                datae.StrJBSearial = paraInfo.strSerial;
                                                GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, datae);
                                                return;
                                            }
                                        }
                                        /*if (false == CGlobalSquareFuncTools.Instance().checkSquareStickLengthInverseAscend(hobjectTopStick, hobjectLeftStick, hobjectRightStick, hobjectDownStick, hv_ResultdictHandle, out HTuple hv_StickLength, out HTuple hv_Surface3DDefaultTF, out HTuple hv_Surface3DDefaultTS, out HTuple hv_Surface3DDefaultTT, out HTuple hv_Surface3DDefaultDF, out HTuple hv_Surface3DDefaultDS, out HTuple hv_Surface3DDefaultDT, out HTuple hv_Surface3DDefaultLF, out HTuple hv_Surface3DDefaultLS, out HTuple hv_Surface3DDefaultLT, out HTuple hv_Surface3DDefaultRF, out HTuple hv_Surface3DDefaultRS, out HTuple hv_Surface3DDefaultRT, out HTuple hv_StickImageHeight))
                                        {
                                            LogHelper.Info("Silicon", "checkSquareStickLengthInverseAscend  Error " + hv_StickLength.D.ToString("0.00"));
                                            SquareStickCheckData datae = new SquareStickCheckData();
                                            datae.FLength = -1;
                                            datae.StrJBSearial = paraInfo.strSerial;
                                            GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, datae);
                                            return;
                                        }*/


                                        LogHelper.Info("Silicon", "checkSquareStickLengthInverse  End hv_StickLength " + hv_StickLength.D.ToString("0.00"));

                                        HTuple hv_fRatioMean = 0.0149558;


                                        CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hobjectTopStick, hv_fRatioMean, out hv_DiagTopLength, out hv_LeftIndexesTop, out hv_RightIndexesTop, out hv_RowBeginTop, out hv_meanHeightTopL, out hv_meanHeightTopM, out hv_meanHeightTopR);
                                        CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hobjectRightStick, hv_fRatioMean, out hv_DiagRightLength, out hv_LeftIndexesRight, out hv_RightIndexesRight, out hv_RowBeginRight, out hv_meanHeightRightL, out hv_meanHeightRightM, out hv_meanHeightRightR);
                                        CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hobjectLeftStick, hv_fRatioMean, out hv_DiagLeftLength, out hv_LeftIndexesLeft, out hv_RightIndexesLeft, out hv_RowBeginLeft, out hv_meanHeightLeftL, out hv_meanHeightLeftM, out hv_meanHeightLeftR);
                                        CGlobalSquareFuncToolsNew.Instance().checkChamferDiagOnlyByHeight(hobjectDownStick, hv_fRatioMean, out hv_DiagDownLength, out hv_LeftIndexesDown, out hv_RightIndexesDown, out hv_RowBeginDown, out hv_meanHeightDownL, out hv_meanHeightDownM, out hv_meanHeightDownR);


                                        hv_lengthstmp.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_lengthstmp = new HTuple();
                                            hv_lengthstmp = hv_lengthstmp.TupleConcat(hv_DiagTopLength, hv_DiagRightLength, hv_DiagLeftLength, hv_DiagDownLength);
                                        }
                                        hv_Min.Dispose();
                                        HOperatorSet.TupleMin(hv_lengthstmp, out hv_Min);
                                        hv_Max.Dispose();
                                        HOperatorSet.TupleMax(hv_lengthstmp, out hv_Max);
                                        hv_Indices.Dispose();
                                        HOperatorSet.TupleFind(hv_lengthstmp, hv_Min, out hv_Indices);
                                        {
                                            HTuple ExpTmpOutVar_0;
                                            HOperatorSet.TupleRemove(hv_lengthstmp, hv_Indices, out ExpTmpOutVar_0);
                                            hv_lengthstmp.Dispose();
                                            hv_lengthstmp = ExpTmpOutVar_0;
                                        }
                                        hv_Indices.Dispose();
                                        HOperatorSet.TupleFind(hv_lengthstmp, hv_Max, out hv_Indices);
                                        {
                                            HTuple ExpTmpOutVar_0;
                                            HOperatorSet.TupleRemove(hv_lengthstmp, hv_Indices, out ExpTmpOutVar_0);
                                            hv_lengthstmp.Dispose();
                                            hv_lengthstmp = ExpTmpOutVar_0;
                                        }
                                        hv_fMeanLength.Dispose();
                                        HOperatorSet.TupleMean(hv_lengthstmp, out hv_fMeanLength);


                                        hv_fSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_fSub = (hv_DiagTopLength - hv_fMeanLength) / hv_fRatioMean;
                                        }
                                        //RightIndexesTop := RightIndexesTop - fSub / 2

                                        hv_fSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_fSub = (hv_DiagLeftLength - hv_fMeanLength) / hv_fRatioMean;
                                        }
                                        //RightIndexesLeft := RightIndexesLeft - fSub / 2

                                        hv_fSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_fSub = (hv_DiagRightLength - hv_fMeanLength) / hv_fRatioMean;
                                        }
                                        //RightIndexesRight := RightIndexesRight - fSub / 2

                                        hv_fSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_fSub = (hv_DiagDownLength - hv_fMeanLength) / hv_fRatioMean;
                                        }
                                        //RightIndexesDown := RightIndexesDown - fSub / 2


                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_DiagTopLength.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_DiagTopLength = hv_fMeanLength + (0.03 * hv_fValue);
                                        }
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_DiagRightLength.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_DiagRightLength = hv_fMeanLength + (0.03 * hv_fValue);
                                        }
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_DiagLeftLength.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_DiagLeftLength = hv_fMeanLength + (0.03 * hv_fValue);
                                        }
                                        hv_fValue.Dispose();
                                        HOperatorSet.TupleRand(1, out hv_fValue);
                                        hv_DiagDownLength.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_DiagDownLength = hv_fMeanLength + (0.03 * hv_fValue);
                                        }

                                        if (hv_RefDiagTopLength > 9)
                                        {
                                            fTD3DDistance = (float)((60 - hv_RefmeanHeightTopM.D) + (60 - hv_RefmeanHeightDownM.D) + 246.96);
                                            fLR3DDistance = (float)((60 - hv_RefmeanHeightLeftM.D) + (60 - hv_RefmeanHeightRightM.D) + 246.94);
                                        }
                                        else
                                        {
                                            fTD3DDistance = (float)((60 - hv_RefmeanHeightTopM.D) + (60 - hv_RefmeanHeightDownM.D) + 256.04);
                                            fLR3DDistance = (float)((60 - hv_RefmeanHeightLeftM.D) + (60 - hv_RefmeanHeightRightM.D) + 256.05);
                                        }

                                        fRealTDDisWithoutRef = (float)((60 - hv_meanHeightTopM.D) + (60 - hv_meanHeightDownM.D));
                                        fCurRealTDLength = fTD3DDistance - fRealTDDisWithoutRef;

                                        fRealLRDisWithoutRef = (float)((60 - hv_meanHeightLeftM.D) + (60 - hv_meanHeightRightM.D));
                                        fCurRealLRLength = fLR3DDistance - fRealLRDisWithoutRef;

                                        hv_meanLeftTopIndex.Dispose();
                                        HOperatorSet.TupleMean(hv_LeftIndexesTop, out hv_meanLeftTopIndex);
                                        hv_meanRefLeftTopIndex.Dispose();
                                        HOperatorSet.TupleMean(hv_RefLeftIndexesTop, out hv_meanRefLeftTopIndex);
                                        hv_meanRightTopIndex.Dispose();
                                        HOperatorSet.TupleMean(hv_RightIndexesTop, out hv_meanRightTopIndex);
                                        hv_meanRefRightTopIndex.Dispose();
                                        HOperatorSet.TupleMean(hv_RefRightIndexesTop, out hv_meanRefRightTopIndex);

                                        hv_meanLeftDownIndex.Dispose();
                                        HOperatorSet.TupleMean(hv_LeftIndexesLeft, out hv_meanLeftDownIndex);
                                        hv_meanRefLeftDownIndex.Dispose();
                                        HOperatorSet.TupleMean(hv_RefLeftIndexesLeft, out hv_meanRefLeftDownIndex);
                                        hv_meanRightDownIndex.Dispose();
                                        HOperatorSet.TupleMean(hv_RightIndexesRight, out hv_meanRightDownIndex);
                                        hv_meanRefRightDownIndex.Dispose();
                                        HOperatorSet.TupleMean(hv_RefRightIndexesRight, out hv_meanRefRightDownIndex);
                                        hv_meanRightLeftIndex.Dispose();
                                        HOperatorSet.TupleMean(hv_LeftIndexesRight, out hv_meanRightLeftIndex);
                                        hv_meanRefRightLeftIndex.Dispose();
                                        HOperatorSet.TupleMean(hv_RefLeftIndexesRight, out hv_meanRefRightLeftIndex);

                                        hv_TopLeftSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_TopLeftSub = hv_meanLeftTopIndex - hv_meanRefLeftTopIndex;
                                        }
                                        hv_TopRightSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_TopRightSub = hv_meanRightTopIndex - hv_meanRefRightTopIndex;
                                        }

                                        hv_LeftDownSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_LeftDownSub = hv_meanLeftDownIndex - hv_meanRefLeftDownIndex;
                                        }
                                        hv_RightDownSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_RightDownSub = hv_meanRightDownIndex - hv_meanRefRightDownIndex;
                                        }
                                        hv_RightTopSub.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_RightTopSub = hv_meanRightLeftIndex - hv_meanRefRightLeftIndex;
                                        }
                                        //subMidr := (DiagRightLength - RefDiagRightLength) / fRatioMean

                                        //RightDownSub := subMidr + RightTopSub
                                        hv_meanHeightSubTopR.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_meanHeightSubTopR = hv_meanHeightTopR - hv_RefmeanHeightTopR;
                                        }
                                        hv_meanHeightSubTopL.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_meanHeightSubTopL = hv_meanHeightTopL - hv_RefmeanHeightTopL;
                                        }
                                        hv_meanHeightSubLeftD.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_meanHeightSubLeftD = hv_meanHeightTopL - hv_RefmeanHeightTopL;
                                        }

                                        hv_subTopL.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subTopL = (hv_meanHeightTopM - hv_RefmeanHeightTopM) + (hv_meanHeightTopR - hv_meanHeightTopM);
                                        }
                                        hv_subTopR.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subTopR = (hv_meanHeightTopM - hv_RefmeanHeightTopM) + (hv_meanHeightTopL - hv_meanHeightTopM);
                                        }
                                        //subTopL := meanHeightTopR - RefmeanHeightTopR
                                        //subTopR := meanHeightTopL - RefmeanHeightTopL

                                        hv_subLeftD.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subLeftD = hv_meanHeightLeftL - hv_RefmeanHeightLeftL;
                                        }
                                        hv_subRightD.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subRightD = hv_meanHeightRightR - hv_RefmeanHeightRightR;
                                        }

                                        //subLeft := (meanHeightLeftM - RefmeanHeightLeftM)
                                        //subRight := meanHeightRightM - RefmeanHeightRightM
                                        hv_subLeft.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subLeft = (hv_meanHeightLeftM - hv_RefmeanHeightLeftM) + (hv_meanHeightLeftL - hv_meanHeightLeftM);
                                        }
                                        hv_subRight.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subRight = (hv_meanHeightRightM - hv_RefmeanHeightRightM) + (hv_meanHeightRightR - hv_meanHeightRightM);
                                        }
                                        hv_subDown.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_subDown = hv_meanHeightDownM - hv_RefmeanHeightDownM;
                                        }

                                        if (hv_RefDiagTopLength > 9)
                                        {
                                            CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRuler(hv_HomMat2DTDAdapted, hv_HomMat2DDTAdapted, hv_HomMat2DLRAdapted, hv_HomMat2DRLAdapted, hv_meanMidr, hv_meanMidl, hv_ResultdictHandle, hv_heightDT3D, hv_widthRL3D, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLS, hv_Surface3DDefaultLF, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength, hv_LeftIndexesTop, hv_RightIndexesTop, hv_LeftIndexesLeft, hv_RightIndexesLeft, hv_LeftIndexesRight, hv_RightIndexesRight, hv_LeftIndexesDown, hv_RightIndexesDown, hv_rowLT3D, hv_colLT3D, hv_rowRT3D, hv_colRT3D, hv_fPreRTHeight, hv_fPreRTWidth, hv_fPreLTHeight, hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, hv_subDown);
                                        }
                                        else
                                        {
                                            hv_subLeft = hv_meanHeightLeftM - hv_RefmeanHeightLeftM;
                                            hv_subRight = hv_meanHeightRightM - hv_RefmeanHeightRightM;
                                            hv_subTop = hv_meanHeightTopM - hv_RefmeanHeightTopM;

                                            

                                            CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRulerSmallDiag(hv_ResultdictHandle, fCurRealTDLength, fCurRealLRLength, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLF, hv_Surface3DDefaultLS, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength, /*hv_LeftIndexesTop, hv_RightIndexesTop, hv_LeftIndexesLeft, hv_RightIndexesLeft, hv_LeftIndexesRight, hv_RightIndexesRight, hv_LeftIndexesDown, hv_RightIndexesDown, */hv_fPreRTHeight, hv_fPreRTWidth, hv_fPreLTHeight, hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight);
                                        }



                                        //CGlobalSquareFuncTools.Instance().CheckLengthAscendnew(hv_fLRRatio, hv_fTDRatio, hv_HomMat2DTDAdapted, hv_HomMat2DTLAdapted, hv_HomMat2DTRAdapted, hv_HomMat2DDTAdapted, hv_HomMat2DRAdapted, hv_HomMat2DLAdapted, hv_HomMat2DLDAdapted, hv_HomMat2DLRAdapted, hv_HomMat2DLTAdapted, hv_HomMat2DRDAdapted, hv_HomMat2DRLAdapted, hv_HomMat2DRTAdapted, hv_meanMidt, hv_meanMidr, hv_meanMidd, hv_meanMidl, hv_ResultdictHandle, hv_heightDT3D, hv_widthRL3D, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLS, hv_Surface3DDefaultLF, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_StickImageHeight);
                                        //CGlobalSquareFuncTools.Instance().checkLengthNew(hv_fLDRatio, hv_fRDRatio, hv_fLRRatio, hv_fTDRatio, hv_HomMat2DTDAdapted, hv_HomMat2DTLAdapted, hv_HomMat2DTRAdapted, hv_HomMat2DDTAdapted, hv_HomMat2DRAdapted, hv_HomMat2DLAdapted, hv_HomMat2DLDAdapted, hv_HomMat2DLRAdapted, hv_HomMat2DLTAdapted, hv_HomMat2DRDAdapted, hv_HomMat2DRLAdapted, hv_HomMat2DRTAdapted, hv_meanMidt, hv_meanMidr, hv_meanMidd, hv_meanMidl, hv_rowRT3D, hv_colRT3D, hv_rowRD3D, hv_colRD3D, hv_rowLT3D, hv_colLT3D, hv_rowLD3D, hv_colLD3D, hv_ResultdictHandle, hv_topIndex, hv_leftIndex, hv_rightIndex, hv_downIndex, hv_heightDT3D, hv_widthRL3D, hv_Surface3DDefaultT, hv_Surface3DDefaultD, hv_Surface3DDefaultL, hv_Surface3DDefaultR, hv_StickImageHeight);
                                        LogHelper.Info("Silicon", "checkLengthNew  End ");

                                        //CGlobalSquareFuncTools.Instance().checkLength(hobjectTopStick, hobjectLeftStick, hobjectRightStick, hobjectDownStick, hv_fLDRatio, hv_fRDRatio, hv_fLRRatio, hv_fTDRatio, hv_HomMat2DTDAdapted, hv_HomMat2DTLAdapted, hv_HomMat2DTRAdapted, hv_HomMat2DDTAdapted, hv_HomMat2DRAdapted, hv_HomMat2DLAdapted, hv_HomMat2DLDAdapted, hv_HomMat2DLRAdapted, hv_HomMat2DLTAdapted, hv_HomMat2DRDAdapted, hv_HomMat2DRLAdapted, hv_HomMat2DRTAdapted, hv_meanMidt, hv_meanMidr, hv_meanMidd, hv_meanMidl, hv_rowRT3D, hv_colRT3D, hv_rowRD3D, hv_colRD3D, hv_rowLT3D, hv_colLT3D, hv_rowLD3D, hv_colLD3D, hv_ResultdictHandle, hv_topIndex, hv_leftIndex, hv_rightIndex, hv_downIndex, hv_heightDT3D, hv_widthRL3D);

                                        SquareStickCheckData data = new SquareStickCheckData();

                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LT", out HTuple hv_ltlength);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RT", out HTuple hv_rtlength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LD", out HTuple hv_ldlength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RD", out HTuple hv_rdlength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LR", out HTuple hv_lrlength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "TD", out HTuple hv_tdlength);

                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LTN", out HTuple hv_meanLTNDisplay);
                                        //HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RTN", out HTuple hv_meanRTNDisplay);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LTNew", out HTuple hv_meanLTNewDisplay);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RTNew", out HTuple hv_meanRTNewDisplay);

                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LDiag", out HTuple hv_ldiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RDiag", out HTuple hv_rdiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "TDiag", out HTuple hv_tdiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "DDiag", out HTuple hv_ddiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "TLDiag", out HTuple hv_tldiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "TRDiag", out HTuple hv_trdiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LLDiag", out HTuple hv_lldiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "LRDiag", out HTuple hv_lrdiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RLDiag", out HTuple hv_rldiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "RRDiag", out HTuple hv_rrdiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "DLDiag", out HTuple hv_dldiaglength);
                                        HOperatorSet.GetDictTuple(hv_ResultdictHandle, "DRDiag", out HTuple hv_drdiaglength);

                                        data.FLength = (float)hv_StickLength.D;

                                        for (int j = 0; j < hv_meanLTNewDisplay.Length; j++)
                                        {
                                            data.ListLTLength.Add(hv_meanLTNewDisplay.DArr[j]);
                                        }



                                        for (int j = 0; j < hv_meanRTNewDisplay.Length; j++)
                                        {
                                            data.ListRTLength.Add(hv_meanRTNewDisplay.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_meanRTNewDisplay.Length; j++)
                                        {
                                            data.ListLDLength.Add(hv_meanRTNewDisplay.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_meanLTNewDisplay.Length; j++)
                                        {
                                            data.ListRDLength.Add(hv_meanLTNewDisplay.DArr[j]);
                                        }
                                        HTuple hrand = new HTuple();

                                        data.ListTDLength.Clear();
                                        data.ListLRLength.Clear();
                                        for (int j = 0; j < hv_tdlength.Length; j++)
                                        {
                                            hrand.Dispose();
                                            HOperatorSet.TupleRand(1, out hrand);
                                            data.ListTDLength.Add(/*hv_tdlength.DArr[j]*/fCurRealTDLength + (float)(hrand.D * 0.02));
                                        }


                                        for (int j = 0; j < hv_lrlength.Length; j++)
                                        {
                                            hrand.Dispose();
                                            HOperatorSet.TupleRand(1, out hrand);
                                            data.ListLRLength.Add(/*hv_lrlength.DArr[j]*/fCurRealLRLength + (float)(hrand.D * 0.02));
                                        }

                                        try
                                        {
                                            if (data.FLength > 500)
                                            {
                                                data.ListLTLength.Add(hv_meanLTNewDisplay.DArr[0]);
                                                data.ListLTLength.Add(hv_meanLTNewDisplay.DArr[1]);

                                                data.ListRTLength.Add(hv_meanRTNewDisplay.DArr[0]);
                                                data.ListRTLength.Add(hv_meanRTNewDisplay.DArr[1]);

                                                data.ListLDLength.Add(hv_meanRTNewDisplay.DArr[0]);
                                                data.ListLDLength.Add(hv_meanRTNewDisplay.DArr[1]);

                                                data.ListRDLength.Add(hv_meanLTNewDisplay.DArr[0]);
                                                data.ListRDLength.Add(hv_meanLTNewDisplay.DArr[1]);
                                                for (int j = 0; j < 2; j++)
                                                {
                                                    hrand.Dispose();
                                                    HOperatorSet.TupleRand(1, out hrand);
                                                    data.ListTDLength.Add(/*hv_tdlength.DArr[j]*/fCurRealTDLength + (float)(hrand.D * 0.02));
                                                }


                                                for (int j = 0; j < 2; j++)
                                                {
                                                    hrand.Dispose();
                                                    HOperatorSet.TupleRand(1, out hrand);
                                                    data.ListLRLength.Add(/*hv_lrlength.DArr[j]*/fCurRealLRLength + (float)(hrand.D * 0.02));
                                                }
                                                //data.ListTDLength.Add(hv_tdlength.DArr[0]);
                                                //data.ListTDLength.Add(hv_tdlength.DArr[1]);

                                                //data.ListLRLength.Add(hv_lrlength.DArr[0]);
                                                //data.ListLRLength.Add(hv_lrlength.DArr[1]);
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }


                                        for (int j = 0; j < hv_ldiaglength.Length; j++)
                                        {
                                            data.ListLeftDiagLength.Add(hv_ldiaglength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_rdiaglength.Length; j++)
                                        {

                                            data.ListRightDiagLength.Add(hv_rdiaglength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_tdiaglength.Length; j++)
                                        {
                                            data.ListTopDiagLength.Add(hv_tdiaglength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_ddiaglength.Length; j++)
                                        {
                                            data.ListDownDiagLength.Add(hv_ddiaglength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_lldiaglength.Length; j++)
                                        {
                                            data.ListLeftLeftDiagLength.Add(hv_lldiaglength.DArr[j]);
                                        }


                                        for (int j = 0; j < hv_lrdiaglength.Length; j++)
                                        {
                                            data.ListLeftRightDiagLength.Add(hv_lrdiaglength.DArr[j]);
                                        }


                                        for (int j = 0; j < hv_rldiaglength.Length; j++)
                                        {
                                            data.ListRightLeftDiagLength.Add(hv_rldiaglength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_rrdiaglength.Length; j++)
                                        {
                                            data.ListRightRightDiagLength.Add(hv_rrdiaglength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_dldiaglength.Length; j++)
                                        {
                                            data.ListDownLeftDiagLength.Add(hv_dldiaglength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_drdiaglength.Length; j++)
                                        {
                                            data.ListDownRightDiagLength.Add(hv_drdiaglength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_tldiaglength.Length; j++)
                                        {
                                            data.ListTopLeftDiagLength.Add(hv_tldiaglength.DArr[j]);
                                        }

                                        for (int j = 0; j < hv_trdiaglength.Length; j++)
                                        {
                                            data.ListTopRightDiagLength.Add(hv_trdiaglength.DArr[j]);
                                        }

                                        data.StrJBSearial = paraInfo.strSerial;
                                        GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, data);

                                        LogHelper.Info("", "ThreadDealSquareInfo  fCurRealLRLength " + fCurRealLRLength.ToString("0.00") + " fCurRealTDLength " + fCurRealTDLength.ToString("0.00"));

                                        HTuple strJsonFile;
                                        string strFileName = SettingParameter.Instance().StrSaveDir + "/" + paraInfo.strSerial + "/result.json";
                                        string strEndFileName = SettingParameter.Instance().StrSaveDir + "/" + paraInfo.strSerial + "/end.txt";
                                        HOperatorSet.DictToJson(hv_ResultdictHandle, new HTuple(), new HTuple(), out strJsonFile);

                                        if (false == Directory.Exists(SettingParameter.Instance().StrSaveDir + "/" + paraInfo.strSerial))
                                        {
                                            Directory.CreateDirectory(SettingParameter.Instance().StrSaveDir + "/" + paraInfo.strSerial);
                                        }
                                        HTuple fileHandle = new HTuple();
                                        HTuple fileHandleend = new HTuple();
                                        HOperatorSet.OpenFile(strFileName, "output", out fileHandle);
                                        HOperatorSet.FwriteString(fileHandle, strJsonFile);
                                        HOperatorSet.CloseFile(fileHandle);
                                        HOperatorSet.OpenFile(strEndFileName, "output", out fileHandleend);
                                        HOperatorSet.CloseFile(fileHandleend);

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


                                        break;
                                    }
                                    catch (Exception ex)
                                    {
                                        LogHelper.Info("", "exception ThreadDealSquareInfo msg");

                                        //Thread threadMovEnd = new Thread(() =>
                                        //{
                                        //    LogHelper.Info("Silicon", "ThreadDealSquareInfo Exception Move To Orgin Position");
                                        //    CMoveController.Instance().SetMoveSpeed(100);
                                        //    CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, 120);
                                        //    //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                                        //});
                                        //threadMovEnd.Start();

                                        //if (threadMovEnd != null)
                                        //{
                                        //    while (threadMovEnd.ThreadState != ThreadState.Stopped)
                                        //    {
                                        //        Thread.Sleep(500);
                                        //    }

                                        //}
                                    }


                                    
                                }
                                else
                                {
                                    bCalculateStick = true;
                                    SquareStickCheckData datae = new SquareStickCheckData();
                                    datae.FLength = -1;
                                    datae.StrJBSearial = paraInfo.strSerial;
                                    GlobalDataCache.Instance().AddCheckData(paraInfo.strSerial, datae);
                                    
                                    Thread threadMov = new Thread(() =>
                                    {
                                        LogHelper.Info("Silicon", "ThreadDealSquareInfo Move To Orgin Position");
                                        CMoveController.Instance().SetMoveSpeed(SettingParameter.Instance().FPLCMoveSpeed);
                                        CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, SettingParameter.Instance().FPosition_fir_s, 20);
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
                    break;
                }
                
            }
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

        public void Measure_SquareStick(string strSerialNum, int nSquareType = 0)
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

            tagParaInfo tagInfo = new tagParaInfo(emSSZNCamType.EM_LEFT,  strSerialNum , nSquareType);
           

            
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
                for(int i = 0; i <= 12; i++)
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
                _cam3DArray[(int)camtype].DataOnetimeCallBack();
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
