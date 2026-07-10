
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

namespace SiliconRoundBarCheck.Cameras
{
    public class SSZNCamTools
    {
        public enum emSSZNCamType
        {
            EM_TOP = 0,
            EM_DOWN = 1,
            EM_LEFT = 2,
            EM_RIGHT = 3,
        };

        private static SSZNCamTools _instance;
        private SsznCamControl[] _cam3DArray = null;
        private ArrayList[] _cam3DOjbectsArray = null;
        private ArrayList[] _camGrayOjbectsArray = null;
        private bool _bNeedInsert3DObject = false;
        private bool _bstop = false;

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

            for (int i = 0; i < 4; i++)
            {
                _cam3DArray[i] = new SsznCamControl(i);
                _cam3DOjbectsArray[i] = new ArrayList();
                _camGrayOjbectsArray[i] = new ArrayList();
                _objLock[i] = new object();
                _objGrayLock[i] = new object();
            }
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
                
                HOperatorSet.ReadImage(out hObject, "E:/Image8/top/" + NMockTopIndex.ToString() + ".tif");
                NMockTopIndex++;
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
                
                HOperatorSet.ReadImage(out hObject, "E:/Image8/right/" + NMockRightIndex.ToString() + ".tif");
                NMockRightIndex++;
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
                HOperatorSet.ReadImage(out hObject, "E:/Image8/down/" + NMockDownIndex.ToString() + ".tif");
                NMockDownIndex++;
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
                HOperatorSet.ReadImage(out hObject, "E:/Image8/left/" + NMockLeftIndex.ToString() + ".tif");
                NMockLeftIndex++;
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
            }
            else
            {
                if (Bstop == false)
                {
                    //SoftTrigOne(emSSZNCamType.EM_TOP);
                    SetBatchBlcok(emSSZNCamType.EM_TOP);
                }
                hObject = GenImage1FromIntArray(_cam3DArray[(int)emSSZNCamType.EM_TOP].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_TOP].Width, _cam3DArray[(int)emSSZNCamType.EM_TOP].Height);
                AddObjects(emSSZNCamType.EM_TOP, hObject);
                
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
            }
            else
            {
                if (Bstop == false)
                {
                    //SoftTrigOne(emSSZNCamType.EM_LEFT);
                    SetBatchBlcok(emSSZNCamType.EM_LEFT);
                }
                hObject = GenImage1FromIntArray(_cam3DArray[(int)emSSZNCamType.EM_LEFT].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_LEFT].Width, _cam3DArray[(int)emSSZNCamType.EM_LEFT].Height);

            }

            AddObjects(emSSZNCamType.EM_LEFT, hObject);
          
                ////获取控制器相机0的高度数据并显示图像  HeightData[0]代表相机0，HeightData[1]代表相机1
            //_cam3DArray[(int)emSSZNCamType.EM_LEFT].BatchDataShow(_cam3DArray[(int)emSSZNCamType.EM_LEFT].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_LEFT].Width, _cam3DArray[(int)emSSZNCamType.EM_LEFT].Height, SHOW_COLOR_MAP.SSZN_COLOR, -1000, 1000);




        }

        private void SSZNCamTools_ShowImage_Right(bool topmost)
        {

            HObject hObject;

            if (SettingParameter.Instance().NDaemon == 0)
            {
                HOperatorSet.ReadObject(out hObject, "D:/20view.tif");
            }
            else
            {
                if (Bstop == false)
                {
                    //SoftTrigOne(emSSZNCamType.EM_RIGHT);
                    SetBatchBlcok(emSSZNCamType.EM_RIGHT);

                }
                hObject = GenImage1FromIntArray(_cam3DArray[(int)emSSZNCamType.EM_RIGHT].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_RIGHT].Width, _cam3DArray[(int)emSSZNCamType.EM_RIGHT].Height);

            }

            AddObjects(emSSZNCamType.EM_RIGHT, hObject);
           
            
           
            ////获取控制器相机0的高度数据并显示图像  HeightData[0]代表相机0，HeightData[1]代表相机1
            //_cam3DArray[(int)emSSZNCamType.EM_RIGHT].BatchDataShow(_cam3DArray[(int)emSSZNCamType.EM_RIGHT].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_RIGHT].Width, _cam3DArray[(int)emSSZNCamType.EM_RIGHT].Height, SHOW_COLOR_MAP.SSZN_COLOR, -1000, 1000);

        }

        private void SSZNCamTools_ShowImage_Down(bool topmost)
        {
            HObject hObject;
            if (SettingParameter.Instance().NDaemon == 0)
            {
                HOperatorSet.ReadObject(out hObject, "D:/20view.tif");
            }
            else
            {
                if (Bstop == false)
                {
                    //SoftTrigOne(emSSZNCamType.EM_DOWN);
                    SetBatchBlcok(emSSZNCamType.EM_DOWN);
                }
                hObject = GenImage1FromIntArray(_cam3DArray[(int)emSSZNCamType.EM_DOWN].HeightData[0], _cam3DArray[(int)emSSZNCamType.EM_DOWN].Width, _cam3DArray[(int)emSSZNCamType.EM_DOWN].Height);
            }

            AddObjects(emSSZNCamType.EM_DOWN, hObject);
           


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

        private class tagParaInfo
        {
            public emSSZNCamType camtype;
            public HObject objref;
            public string strSerial;

            public tagParaInfo(emSSZNCamType camtyp, HObject obj, string strserialnum)
            {
                camtype = camtyp;
                objref = obj;
                strSerial = strserialnum;
            }
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

        HObject _hObjectsLeft = new HObject();
        HObject _hObjectsRight = new HObject();
        HObject _hObjectsTop = new HObject();
        HObject _hObjectsDown = new HObject();
        HObject _ho_ImageLeft = new HObject();
        HObject _ho_ImageTop = new HObject();
        HObject _ho_ImageRight = new HObject();
        HObject _ho_ImageDown = new HObject();

       
        public void ThreadDealSquareInfo(object objtype)
        {
            HObject hObject = new HObject();

            tagParaInfo paraInfo = (tagParaInfo)objtype;
            int nWaitIndex = 0;
            int nGrayWaitIndex = 0;


            bool bNeedBegin = false;

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
                                        CGlobalSquareFuncToolsNew.Instance().checkReferenceNewByRelatively(hObjectTopReference, hObjectLeftReference, hObjectRightReference, hObjectDownReference, hv_RefRowBeginTop, hv_RefRowBeginLeft, hv_RefRowBeginRight, hv_RefRowBeginDown, hv_RefLeftIndexesTop, hv_RefRightIndexesTop, hv_RefLeftIndexesDown, hv_RefRightIndexesDown, hv_RefLeftIndexesLeft, hv_RefRightIndexesLeft, hv_RefLeftIndexesRight, hv_RefRightIndexesRight, hv_RefDiagTopLength, hv_RefDiagLeftLength, hv_RefDiagRightLength, hv_RefDiagDownLength,  hv_RefmeanHeightTopR, hv_RefmeanHeightDownR, hv_RefmeanHeightLeftR, hv_RefmeanHeightRightR,  out hv_fPreRTHeight, out hv_fPreRTWidth, out hv_fPreLTHeight, out hv_fPreLTWidth);
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
                                        SSZNCamTools.Instance().StopBatchLoop();

                                        Thread threadMov = new Thread(() =>
                                        {
                                            LogHelper.Info("Silicon", "ThreadDealSquareInfo Move To Orgin Position");
                                            CMoveController.Instance().SetMoveSpeed(200);
                                            CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, 120);
                                            //CMoveControllerModbusTool.Instance().WriteSingleCoil(128, true);
                                        });
                                        if (SettingParameter.Instance().NDaemon == 1)
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

                                            
                                            

                                            CGlobalSquareFuncToolsNew.Instance().CheckLengthAscendnewWithoutRulerSmallDiag(hv_ResultdictHandle, fCurRealTDLength, fCurRealLRLength, hv_Surface3DDefaultTF, hv_Surface3DDefaultTS, hv_Surface3DDefaultTT, hv_Surface3DDefaultDF, hv_Surface3DDefaultDS, hv_Surface3DDefaultDT, hv_Surface3DDefaultLF, hv_Surface3DDefaultLS, hv_Surface3DDefaultLT, hv_Surface3DDefaultRF, hv_Surface3DDefaultRS, hv_Surface3DDefaultRT, hv_firstTopIndexRow, hv_secondTopIndexRow, hv_thirdTopIndexRow, hv_firstDowntIndexRow, hv_secondDownIndexRow, hv_thirdDownIndexRow, hv_firstRightIndexRow, hv_secondRightIndexRow, hv_thirdRightIndexRow, hv_firstLeftIndexRow, hv_secondLeftIndexRow, hv_thirdLeftIndexRow, hv_DiagTopLength, hv_DiagLeftLength, hv_DiagRightLength, hv_DiagDownLength, hv_LeftIndexesTop, hv_RightIndexesTop, hv_LeftIndexesLeft, hv_RightIndexesLeft, hv_LeftIndexesRight, hv_RightIndexesRight, hv_LeftIndexesDown, hv_RightIndexesDown, hv_fPreRTHeight, hv_fPreRTWidth, hv_fPreLTHeight, hv_fPreLTWidth, hv_TopLeftSub, hv_TopRightSub, hv_LeftDownSub, hv_RightDownSub, hv_subTopL, hv_subTopR, hv_subLeft, hv_subRight, hv_subTop);
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

                                        if (SettingParameter.Instance().NDaemon == 1)
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
                                        CMoveController.Instance().SetMoveSpeed(100);
                                        CMoveController.Instance().GotoDestPosition(SettingParameter.Instance().FPosition_fir, 120);
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
        public void Measure_SquareStick(string strSerialNum)
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

            tagParaInfo tagLeft = new tagParaInfo(emSSZNCamType.EM_LEFT, objrefleft, strSerialNum);
            tagParaInfo tagRight = new tagParaInfo(emSSZNCamType.EM_RIGHT, objrefright, strSerialNum);
            tagParaInfo tagTop = new tagParaInfo(emSSZNCamType.EM_TOP, objreftop, strSerialNum);
            tagParaInfo tagDown = new tagParaInfo(emSSZNCamType.EM_DOWN, objrefdown, strSerialNum);

            

            _threadleft = new Thread(new ParameterizedThreadStart(ThreadDealSquareInfo));
            //_threadright = new Thread(new ParameterizedThreadStart(ThreadDealSquareInfo));
            //_threadtop = new Thread(new ParameterizedThreadStart(ThreadDealSquareInfo));
            //_threaddown = new Thread(new ParameterizedThreadStart(ThreadDealSquareInfo));



            _threadleft.Start(tagLeft);
            //_threadright.Start(tagRight);
            //_threadtop.Start(tagTop);
            //_threaddown.Start(tagDown);

           

            while(true)
            {
               if (_threadleft.ThreadState != ThreadState.Stopped /*||  _threadright.ThreadState != ThreadState.Stopped ||
                    _threadtop.ThreadState != ThreadState.Stopped || _threaddown.ThreadState != ThreadState.Stopped*/)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                else
                {
                    break;
                }
            }
            /*
           HTuple hv_top_Hypotenuse = new HTuple();
           HTuple hv_top_LefttAngle = new HTuple();
           HTuple hv_top_RightAngle = new HTuple();
           HTuple hv_top_HypotenuseFH = new HTuple();
           HTuple hv_top_HypotenuseSH = new HTuple();
           HTuple hv_topdown_HypotenuseDis = new HTuple();
           HTuple hv_leftright_HypotenuseDis = new HTuple();
           HTuple hv_left_HypotenuseFH = new HTuple();
           HTuple hv_left_HypotenuseSH = new HTuple();
           HTuple hv_lefttop_lineLength = new HTuple();
           HTuple hv_righttop_lineLength = new HTuple();
           HTuple hv_leftdown_lineLength = new HTuple();
           HTuple hv_rightdown_linelength = new HTuple();
           HTuple hv_left_Hypotenuse = new HTuple();
           HTuple hv_left_LeftAngle = new HTuple();
           HTuple hv_left_RightAngle = new HTuple();
           HTuple hv_right_HypotenuseFH = new HTuple();
           HTuple hv_right_HypotenuseSH = new HTuple();
           HTuple hv_down_HypotenuseFH = new HTuple();
           HTuple hv_down_HypotenuseSH = new HTuple();
           HTuple hv_down_Hypotenuse = new HTuple();
           HTuple hv_down_LeftAngle = new HTuple();
           HTuple hv_down_RightAngle = new HTuple();
           HTuple hv_right_Hypotenuse = new HTuple();
           HTuple hv_right_LeftAngle = new HTuple();
           HTuple hv_right_RightAngle = new HTuple();

           CGlobalFuncTools.Instance().GetDistorInfoByImage(tagTop.objref, tagDown.objref, tagLeft.objref, tagRight.objref, out hv_top_Hypotenuse, out hv_top_RightAngle, out hv_top_LefttAngle, out hv_top_HypotenuseFH, out hv_top_HypotenuseSH, out hv_left_Hypotenuse, out hv_left_LeftAngle, out hv_left_RightAngle, out hv_left_HypotenuseFH, out hv_left_HypotenuseSH, out hv_right_Hypotenuse, out hv_right_RightAngle, out hv_right_LeftAngle, out hv_right_HypotenuseFH, out hv_right_HypotenuseSH, out hv_down_Hypotenuse, out hv_down_RightAngle, out hv_down_LeftAngle, out hv_down_HypotenuseFH, out hv_down_HypotenuseSH, out hv_topdown_HypotenuseDis, out hv_leftright_HypotenuseDis, out hv_lefttop_lineLength, out hv_righttop_lineLength, out hv_leftdown_lineLength, out hv_rightdown_linelength);


           StickData data = new StickData();

           for (int i = 0; i < hv_top_Hypotenuse.DArr.Length; i++)
           {
               data.TopHypotenuseLengthInfo.Add(hv_top_Hypotenuse.DArr[i]);
           }

           for (int i = 0; i < hv_top_LefttAngle.DArr.Length; i++)
           {
               data.TopFirstAngleInfo.Add(hv_top_LefttAngle.DArr[i]);
           }

           for (int i = 0; i < hv_top_RightAngle.DArr.Length; i++)
           {
               data.TopSecondAngleInfo.Add(hv_top_RightAngle.DArr[i]);
           }

           for (int i = 0; i < hv_top_HypotenuseFH.DArr.Length; i++)
           {
               data.TopfirstheightInfo.Add(hv_top_HypotenuseFH.DArr[i]);
           }

           for (int i = 0; i < hv_top_HypotenuseSH.DArr.Length; i++)
           {
               data.TopsecondheightInfo.Add(hv_top_HypotenuseSH.DArr[i]);
           }


           for (int i = 0; i < hv_left_Hypotenuse.DArr.Length; i++)
           {
               data.LefthypotenuseLengthInfo.Add(hv_left_Hypotenuse.DArr[i]);
           }

           for (int i = 0; i < hv_left_LeftAngle.DArr.Length; i++)
           {
               data.LeftfirstAngleInfo.Add(hv_left_LeftAngle.DArr[i]);
           }

           for (int i = 0; i < hv_left_RightAngle.DArr.Length; i++)
           {
               data.LeftsecondAngleInfo.Add(hv_left_RightAngle.DArr[i]);
           }

           for (int i = 0; i < hv_left_HypotenuseFH.DArr.Length; i++)
           {
               data.LeftfirstheightInfo.Add(hv_left_HypotenuseFH.DArr[i]);
           }

           for (int i = 0; i < hv_left_HypotenuseSH.DArr.Length; i++)
           {
               data.LeftsecondheightInfo.Add(hv_left_HypotenuseSH.DArr[i]);
           }


           for (int i = 0; i < hv_down_Hypotenuse.DArr.Length; i++)
           {
               data.DownhypotenuseLengthInfo.Add(hv_down_Hypotenuse.DArr[i]);
           }

           for (int i = 0; i < hv_down_LeftAngle.DArr.Length; i++)
           {
               data.DownfirstAngleInfo.Add(hv_down_LeftAngle.DArr[i]);
           }

           for (int i = 0; i < hv_down_RightAngle.DArr.Length; i++)
           {
               data.DownsecondAngleInfo.Add(hv_down_RightAngle.DArr[i]);
           }

           for (int i = 0; i < hv_down_HypotenuseFH.DArr.Length; i++)
           {
               data.DownfirstheightInfo.Add(hv_down_HypotenuseFH.DArr[i]);
           }

           for (int i = 0; i < hv_down_HypotenuseSH.DArr.Length; i++)
           {
               data.DownsecondheightInfo.Add(hv_down_HypotenuseSH.DArr[i]);
           }


           for (int i = 0; i < hv_right_Hypotenuse.DArr.Length; i++)
           {
               data.RighthypotenuseLengthInfo.Add(hv_right_Hypotenuse.DArr[i]);
           }

           for (int i = 0; i < hv_right_LeftAngle.DArr.Length; i++)
           {
               data.RightfirstAngleInfo.Add(hv_right_LeftAngle.DArr[i]);
           }

           for (int i = 0; i < hv_right_RightAngle.DArr.Length; i++)
           {
               data.RightsecondAngleInfo.Add(hv_right_RightAngle.DArr[i]);
           }

           for (int i = 0; i < hv_right_HypotenuseFH.DArr.Length; i++)
           {
               data.RightfirstheightInfo.Add(hv_right_HypotenuseFH.DArr[i]);
           }

           for (int i = 0; i < hv_right_HypotenuseSH.DArr.Length; i++)
           {
               data.RightsecondheightInfo.Add(hv_right_HypotenuseSH.DArr[i]);
           }


           for (int i = 0; i < hv_topdown_HypotenuseDis.DArr.Length; i++)
           {
               data.TopdownhypotenuseDis.Add(hv_topdown_HypotenuseDis[i]);
           }

           for (int i = 0; i < hv_leftright_HypotenuseDis.DArr.Length; i++)
           {
               data.LeftrighthypotenuseDis.Add(hv_leftright_HypotenuseDis[i]);
           }

           for (int i = 0; i < hv_lefttop_lineLength.DArr.Length; i++)
           {
               data.LeftTopLineLengthInfo.Add(hv_lefttop_lineLength[i]);
           }

           for (int i = 0; i < hv_leftdown_lineLength.DArr.Length; i++)
           {
               data.LeftdownlineLengthInfo.Add(hv_leftdown_lineLength[i]);
           }

           for (int i = 0; i < hv_righttop_lineLength.DArr.Length; i++)
           {
               data.RighttoplineLengthInfo.Add(hv_righttop_lineLength[i]);
           }

           for (int i = 0; i < hv_rightdown_linelength.DArr.Length; i++)
           {
               data.RightdownlineLengthInfo.Add(hv_rightdown_linelength[i]);
           }

           GlobalDataCache.Instance().AddData(strSerialNum, data);
           */
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
                for(int i = 0; i <= 16; i++)
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
