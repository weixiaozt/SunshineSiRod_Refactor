using HalconDotNet;
using SiliconRoundBarCheck.Parameters;
using SiliconRoundBarCheck.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YouEyEE.Untils;
using YouEyEE.Untils.Log;

namespace SiliconRoundBarCheck.Cameras.SSZN
{
    public class SSZNCameraTools
    {
        private static SSZNCameraTools _instance;

        public static SSZNCameraTools Instance()
        {
            if (_instance == null)
            {
                _instance = new SSZNCameraTools();
            }
            return _instance;
        }
        public void Creat_XYZ_From_sszn(HObject ho_ImageZ, out HObject ho_ImageConvertedX,
      out HObject ho_ImageConvertedY, out HObject ho_ImageReducedZ1, HTuple hv_yInterval,
      out HTuple hv_ObjectModel3D)
        {




            // Local iconic variables 

            HObject ho_ImageX, ho_ImageY, ho_ImageConvertedZ;
            HObject ho_Region, ho_ImageReducedZ;

            // Local control variables 

            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_xInterval = new HTuple(), hv_Min = new HTuple();
            HTuple hv_Max = new HTuple(), hv_Range = new HTuple();
            HTuple hv_ScaleZ = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageConvertedX);
            HOperatorSet.GenEmptyObj(out ho_ImageConvertedY);
            HOperatorSet.GenEmptyObj(out ho_ImageReducedZ1);
            HOperatorSet.GenEmptyObj(out ho_ImageX);
            HOperatorSet.GenEmptyObj(out ho_ImageY);
            HOperatorSet.GenEmptyObj(out ho_ImageConvertedZ);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_ImageReducedZ);
            hv_ObjectModel3D = new HTuple();
            try
            {
                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_ImageZ, out hv_Width, out hv_Height);

                //生成XY图像
                hv_xInterval.Dispose();
                hv_xInterval = 0.09;
                //yInterval := 0.09
                ho_ImageX.Dispose();
                HOperatorSet.GenImageSurfaceFirstOrder(out ho_ImageX, "real", 0, hv_xInterval,
                    0, 0, 0, hv_Width, hv_Height);
                ho_ImageY.Dispose();
                HOperatorSet.GenImageSurfaceFirstOrder(out ho_ImageY, "real", hv_yInterval,
                    0, 0, 0, 0, hv_Width, hv_Height);

                //对于real类型的图像不再需要一下操作
                ho_ImageConvertedX.Dispose();
                HOperatorSet.ConvertImageType(ho_ImageX, out ho_ImageConvertedX, "real");
                ho_ImageConvertedY.Dispose();
                HOperatorSet.ConvertImageType(ho_ImageY, out ho_ImageConvertedY, "real");
                ho_ImageConvertedZ.Dispose();
                HOperatorSet.ConvertImageType(ho_ImageZ, out ho_ImageConvertedZ, "real");

                //计算数据最大最小值
                hv_Min.Dispose(); hv_Max.Dispose(); hv_Range.Dispose();
                HOperatorSet.MinMaxGray(ho_ImageZ, ho_ImageZ, 0, out hv_Min, out hv_Max, out hv_Range);
                ho_Region.Dispose();
                HOperatorSet.Threshold(ho_ImageZ, out ho_Region, -500, hv_Max);
                ho_ImageReducedZ.Dispose();
                HOperatorSet.ReduceDomain(ho_ImageZ, ho_Region, out ho_ImageReducedZ);
                //设置Z方向缩放系数,最终高度 = ScaleZ * GrayValue值，ScaleZ越小高度值范围也越小，点云效果接近平面，反之越明显
                //确保在0值以上，可通过min_max_gray获取最小值然后拉伸到0以上
                hv_ScaleZ.Dispose();
                hv_ScaleZ = 1;
                ho_ImageReducedZ1.Dispose();
                HOperatorSet.ScaleImage(ho_ImageReducedZ, out ho_ImageReducedZ1, hv_ScaleZ,
                    0);
                hv_ObjectModel3D.Dispose();
                HOperatorSet.XyzToObjectModel3d(ho_ImageConvertedX, ho_ImageConvertedY, ho_ImageReducedZ1,
                    out hv_ObjectModel3D);

                ho_ImageX.Dispose();
                ho_ImageY.Dispose();
                ho_ImageConvertedZ.Dispose();
                ho_Region.Dispose();
                ho_ImageReducedZ.Dispose();

                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_xInterval.Dispose();
                hv_Min.Dispose();
                hv_Max.Dispose();
                hv_Range.Dispose();
                hv_ScaleZ.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_ImageX.Dispose();
                ho_ImageY.Dispose();
                ho_ImageConvertedZ.Dispose();
                ho_Region.Dispose();
                ho_ImageReducedZ.Dispose();

                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_xInterval.Dispose();
                hv_Min.Dispose();
                hv_Max.Dispose();
                hv_Range.Dispose();
                hv_ScaleZ.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void Fb(HObject ho_ImageHeight, HTuple hv_Height, HTuple hv_Width, HTuple hv_start,
      HTuple hv_end, out HTuple hv_RadiusMean, out HTuple hv_Radius, out HTuple hv_M)
        {
            

            // Local iconic variables 

            HObject ho_Contour = null, ho_ContCircle = null;

            // Local control variables 

            HTuple hv_Id = new HTuple(), hv_Zero = new HTuple();
            HTuple hv_i = new HTuple(), hv_Rows = new HTuple(), hv_Cols = new HTuple();
            HTuple hv_Y = new HTuple(), hv_Indices = new HTuple();
            HTuple hv_ZeroRatio = new HTuple(), hv_X = new HTuple();
            HTuple hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_R = new HTuple(), hv_StartPhi = new HTuple();
            HTuple hv_EndPhi = new HTuple(), hv_PointOrder = new HTuple();
            HTuple hv_DistanceMin = new HTuple(), hv_DistanceMax = new HTuple();
            HTuple hv_One = new HTuple(), hv_Greater = new HTuple();
            HTuple hv_Indices1 = new HTuple(), hv_Indices2 = new HTuple();
            HTuple hv_Reduced = new HTuple(), hv_Exception = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Contour);
            HOperatorSet.GenEmptyObj(out ho_ContCircle);
            hv_RadiusMean = new HTuple();
            hv_Radius = new HTuple();
            hv_M = new HTuple();
            try
            {
                hv_M.Dispose();
                hv_M = 0;
                hv_Radius.Dispose();
                hv_Radius = new HTuple();
                hv_Id.Dispose();
                hv_Id = new HTuple();
                hv_Zero.Dispose();
                hv_Zero = new HTuple();

                HTuple end_val5 = hv_end - 100;
                HTuple step_val5 = 1;
                for (hv_i = hv_start; hv_i.Continue(end_val5, step_val5); hv_i = hv_i.TupleAdd(step_val5))
                {
                    hv_Rows.Dispose();
                    HOperatorSet.TupleGenConst(hv_Width, hv_i, out hv_Rows);
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Cols.Dispose();
                        HOperatorSet.TupleGenSequence(0, hv_Width - 1, 1, out hv_Cols);
                    }
                    hv_Y.Dispose();
                    HOperatorSet.GetGrayval(ho_ImageHeight, hv_Rows, hv_Cols, out hv_Y);
                    hv_Indices.Dispose();
                    HOperatorSet.TupleFind(hv_Y, 0, out hv_Indices);
                    hv_ZeroRatio.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ZeroRatio = ((new HTuple(hv_Indices.TupleLength()
                            )) * 1.0) / hv_Width;
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Zero = hv_Zero.TupleConcat(
                                hv_ZeroRatio);
                            hv_Zero.Dispose();
                            hv_Zero = ExpTmpLocalVar_Zero;
                        }
                    }
                    if ((int)(new HTuple(hv_ZeroRatio.TupleGreater(0.2))) != 0)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_Radius = hv_Radius.TupleConcat(
                                    0);
                                hv_Radius.Dispose();
                                hv_Radius = ExpTmpLocalVar_Radius;
                            }
                        }
                        continue;
                    }
                    hv_X.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_X = hv_Cols * 0.09;
                    }
                    {
                        HTuple ExpTmpOutVar_0;
                        HOperatorSet.TupleRemove(hv_X, hv_Indices, out ExpTmpOutVar_0);
                        hv_X.Dispose();
                        hv_X = ExpTmpOutVar_0;
                    }
                    {
                        HTuple ExpTmpOutVar_0;
                        HOperatorSet.TupleRemove(hv_Y, hv_Indices, out ExpTmpOutVar_0);
                        hv_Y.Dispose();
                        hv_Y = ExpTmpOutVar_0;
                    }

                    ho_Contour.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_Contour, hv_Y, hv_X);
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_R.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_Contour, "atukey", -1, 0, 0, 3, 2, out hv_Row,
                        out hv_Column, out hv_R, out hv_StartPhi, out hv_EndPhi, out hv_PointOrder);
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_R,
                        0, 6.28318, "positive", 1);
                    hv_DistanceMin.Dispose(); hv_DistanceMax.Dispose();
                    HOperatorSet.DistancePc(ho_ContCircle, hv_Y, hv_X, out hv_DistanceMin, out hv_DistanceMax);
                    hv_One.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_One = (hv_DistanceMin * 0) + 1;
                    }
                    hv_Greater.Dispose();
                    HOperatorSet.TupleGreaterElem(hv_DistanceMin, hv_One, out hv_Greater);
                    hv_Indices1.Dispose();
                    HOperatorSet.TupleFind(hv_Greater, 1, out hv_Indices1);
                    if ((int)(new HTuple(hv_R.TupleGreater(126))) != 0)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_Id = hv_Id.TupleConcat(
                                    hv_i);
                                hv_Id.Dispose();
                                hv_Id = ExpTmpLocalVar_Id;
                            }
                        }
                    }

                    if ((int)(new HTuple((new HTuple(hv_Indices1.TupleLength())).TupleGreater(
                        50))) != 0)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_Radius = hv_Radius.TupleConcat(
                                    0);
                                hv_Radius.Dispose();
                                hv_Radius = ExpTmpLocalVar_Radius;
                            }
                        }
                        continue;
                    }

                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Radius = hv_Radius.TupleConcat(
                                hv_R);
                            hv_Radius.Dispose();
                            hv_Radius = ExpTmpLocalVar_Radius;
                        }
                    }
                }
                try
                {
                    hv_Indices2.Dispose();
                    HOperatorSet.TupleFind(hv_Radius, 0, out hv_Indices2);
                    hv_Reduced.Dispose();
                    HOperatorSet.TupleRemove(hv_Radius, hv_Indices2, out hv_Reduced);
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    hv_Reduced.Dispose();
                    hv_Reduced = 0;
                }

                try
                {
                    hv_RadiusMean.Dispose();
                    HOperatorSet.TupleMean(hv_Reduced, out hv_RadiusMean);
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    hv_RadiusMean.Dispose();
                    hv_RadiusMean = 0;
                }
                hv_M.Dispose();
                hv_M = 1;
                ho_Contour.Dispose();
                ho_ContCircle.Dispose();

                hv_Id.Dispose();
                hv_Zero.Dispose();
                hv_i.Dispose();
                hv_Rows.Dispose();
                hv_Cols.Dispose();
                hv_Y.Dispose();
                hv_Indices.Dispose();
                hv_ZeroRatio.Dispose();
                hv_X.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_R.Dispose();
                hv_StartPhi.Dispose();
                hv_EndPhi.Dispose();
                hv_PointOrder.Dispose();
                hv_DistanceMin.Dispose();
                hv_DistanceMax.Dispose();
                hv_One.Dispose();
                hv_Greater.Dispose();
                hv_Indices1.Dispose();
                hv_Indices2.Dispose();
                hv_Reduced.Dispose();
                hv_Exception.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Contour.Dispose();
                ho_ContCircle.Dispose();

                hv_Id.Dispose();
                hv_Zero.Dispose();
                hv_i.Dispose();
                hv_Rows.Dispose();
                hv_Cols.Dispose();
                hv_Y.Dispose();
                hv_Indices.Dispose();
                hv_ZeroRatio.Dispose();
                hv_X.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_R.Dispose();
                hv_StartPhi.Dispose();
                hv_EndPhi.Dispose();
                hv_PointOrder.Dispose();
                hv_DistanceMin.Dispose();
                hv_DistanceMax.Dispose();
                hv_One.Dispose();
                hv_Greater.Dispose();
                hv_Indices1.Dispose();
                hv_Indices2.Dispose();
                hv_Reduced.Dispose();
                hv_Exception.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void CalcRadius(HObject ho_ImageHeight, out HTuple hv_RadiusPlus, out HTuple hv_mean100)
        {

           
            {
                // +++ Threading variables 
                HDevThread devThread;


                // Local iconic variables 

                // Local control variables 

                HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
                HTuple hv_M = new HTuple(), hv_M1 = new HTuple(), hv_M2 = new HTuple();
                HTuple hv_M3 = new HTuple(), hv_M4 = new HTuple(), hv_M5 = new HTuple();
                HTuple hv_M6 = new HTuple(), hv_M7 = new HTuple(), hv_M8 = new HTuple();
                HTuple hv_M9 = new HTuple(), hv_M10 = new HTuple(), hv_M11 = new HTuple();
                HTuple hv_M12 = new HTuple(), hv_M13 = new HTuple(), hv_M14 = new HTuple();
                HTuple hv_M15 = new HTuple(), hv_M16 = new HTuple(), hv_M17 = new HTuple();
                HTuple hv_M18 = new HTuple(), hv_M19 = new HTuple(), hv_M20 = new HTuple();
                HTuple hv_M21 = new HTuple(), hv_M22 = new HTuple(), hv_M23 = new HTuple();
                HTuple hv_RadiusMean = new HTuple(), hv_Radius1 = new HTuple();
                HTuple hv_ThreadID0 = new HTuple(), hv_RadiusMean1 = new HTuple();
                HTuple hv_Radius2 = new HTuple(), hv_ThreadID1 = new HTuple();
                HTuple hv_RadiusMean2 = new HTuple(), hv_Radius3 = new HTuple();
                HTuple hv_ThreadID2 = new HTuple(), hv_RadiusMean3 = new HTuple();
                HTuple hv_Radius4 = new HTuple(), hv_ThreadID3 = new HTuple();
                HTuple hv_RadiusMean4 = new HTuple(), hv_Radius5 = new HTuple();
                HTuple hv_ThreadID4 = new HTuple(), hv_RadiusMean5 = new HTuple();
                HTuple hv_Radius6 = new HTuple(), hv_ThreadID5 = new HTuple();
                HTuple hv_RadiusMean6 = new HTuple(), hv_Radius7 = new HTuple();
                HTuple hv_ThreadID6 = new HTuple(), hv_RadiusMean7 = new HTuple();
                HTuple hv_Radius8 = new HTuple(), hv_ThreadID7 = new HTuple();
                HTuple hv_RadiusMean8 = new HTuple(), hv_Radius9 = new HTuple();
                HTuple hv_ThreadID8 = new HTuple(), hv_RadiusMean9 = new HTuple();
                HTuple hv_Radius10 = new HTuple(), hv_ThreadID9 = new HTuple();
                HTuple hv_RadiusMean10 = new HTuple(), hv_Radius11 = new HTuple();
                HTuple hv_ThreadID10 = new HTuple(), hv_RadiusMean11 = new HTuple();
                HTuple hv_Radius12 = new HTuple(), hv_ThreadID11 = new HTuple();
                HTuple hv_RadiusMean12 = new HTuple(), hv_Radius13 = new HTuple();
                HTuple hv_ThreadID12 = new HTuple(), hv_RadiusMean13 = new HTuple();
                HTuple hv_Radius14 = new HTuple(), hv_ThreadID13 = new HTuple();
                HTuple hv_RadiusMean14 = new HTuple(), hv_Radius15 = new HTuple();
                HTuple hv_ThreadID14 = new HTuple(), hv_RadiusMean15 = new HTuple();
                HTuple hv_Radius16 = new HTuple(), hv_ThreadID15 = new HTuple();
                HTuple hv_RadiusMean16 = new HTuple(), hv_Radius17 = new HTuple();
                HTuple hv_ThreadID16 = new HTuple(), hv_RadiusMean17 = new HTuple();
                HTuple hv_Radius18 = new HTuple(), hv_ThreadID17 = new HTuple();
                HTuple hv_RadiusMean18 = new HTuple(), hv_Radius19 = new HTuple();
                HTuple hv_ThreadID18 = new HTuple(), hv_RadiusMean19 = new HTuple();
                HTuple hv_Radius20 = new HTuple(), hv_MP = new HTuple();
                HTuple hv_Exception = new HTuple();
                // Initialize local and output iconic variables 
                hv_RadiusPlus = new HTuple();
                hv_mean100 = new HTuple();
                try
                {
                    hv_mean100.Dispose();
                    hv_mean100 = new HTuple();

                    hv_Width.Dispose(); hv_Height.Dispose();
                    HOperatorSet.GetImageSize(ho_ImageHeight, out hv_Width, out hv_Height);
                    
                    hv_M.Dispose();
                    hv_M = 0;
                    hv_M1.Dispose();
                    hv_M1 = 0;
                    hv_M2.Dispose();
                    hv_M2 = 0;
                    hv_M3.Dispose();
                    hv_M3 = 0;
                    hv_M4.Dispose();
                    hv_M4 = 0;
                    hv_M5.Dispose();
                    hv_M5 = 0;
                    hv_M6.Dispose();
                    hv_M6 = 0;
                    hv_M7.Dispose();
                    hv_M7 = 0;
                    hv_M8.Dispose();
                    hv_M8 = 0;
                    hv_M9.Dispose();
                    hv_M9 = 0;
                    hv_M10.Dispose();
                    hv_M10 = 0;
                    hv_M11.Dispose();
                    hv_M11 = 0;

                    hv_M12.Dispose();
                    hv_M12 = 0;
                    hv_M13.Dispose();
                    hv_M13 = 0;
                    hv_M14.Dispose();
                    hv_M14 = 0;
                    hv_M15.Dispose();
                    hv_M15 = 0;
                    hv_M16.Dispose();
                    hv_M16 = 0;
                    hv_M17.Dispose();
                    hv_M17 = 0;
                    hv_M18.Dispose();
                    hv_M18 = 0;
                    hv_M19.Dispose();
                    hv_M19 = 0;
                    hv_M20.Dispose();
                    hv_M20 = 0;
                    hv_M21.Dispose();
                    hv_M21 = 0;
                    hv_M22.Dispose();
                    hv_M22 = 0;
                    hv_M23.Dispose();
                    hv_M23 = 0;
                    if (hv_RadiusMean == null)
                        hv_RadiusMean = new HTuple();
                    if (hv_Radius1 == null)
                        hv_Radius1 = new HTuple();
                    if (hv_M == null)
                        hv_M = new HTuple();
                    

                    Thread devThread_Fir = new Thread(() =>
                      {
                          try
                          {
                             

                              // Call Fb
                              Fb(ho_ImageHeight, hv_Height, hv_Width, 0, 99, out hv_RadiusMean,
                                      out hv_Radius1, out hv_M);

                             

                          }
                          catch (HalconException exc)
                          {
                
                          }
                      });
                    devThread_Fir.Start();

                    if (hv_RadiusMean1 == null)
                        hv_RadiusMean1 = new HTuple();
                    if (hv_Radius2 == null)
                        hv_Radius2 = new HTuple();
                    if (hv_M1 == null)
                        hv_M1 = new HTuple();
                    
                    Thread devThread_Sec = new Thread(() =>
                    {
                        try
                        { 
                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 100, 199, out hv_RadiusMean1,
                                        out hv_Radius2, out hv_M1);
                            
                        }
                        catch (HalconException exc)
                        {
               
                        }
                    });



                    // Start proc line in thread
                    devThread_Sec.Start();


                    if (hv_RadiusMean2 == null)
                        hv_RadiusMean2 = new HTuple();
                    
                    if (hv_Radius3 == null)
                        hv_Radius3 = new HTuple();
                    if (hv_M2 == null)
                        hv_M2 = new HTuple();
                    Thread devThread_thr = new Thread(() =>
                    {
                        try
                        {


                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 200, 299, out hv_RadiusMean2,
                                    out hv_Radius3, out hv_M2);


                        }
                        catch (HalconException exc)
                        {

                        }
                    });
                    devThread_thr.Start();

                    if (hv_RadiusMean3 == null)
                        hv_RadiusMean3 = new HTuple();
                    
                    if (hv_Radius4 == null)
                        hv_Radius4 = new HTuple();
                    if (hv_M3 == null)
                        hv_M3 = new HTuple();

                    Thread devThread_Four = new Thread(() =>
                    {
                        try
                        {
                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 300, 399, out hv_RadiusMean3,
                                        out hv_Radius4, out hv_M3);
                            
                        }
                        catch (HalconException exc)
                        {
                
                        }
                    });
                    devThread_Four.Start();

                    if (hv_RadiusMean4 == null)
                        hv_RadiusMean4 = new HTuple();
                   
                    if (hv_Radius5 == null)
                        hv_Radius5 = new HTuple();
                   
                    if (hv_M4 == null)
                        hv_M4 = new HTuple();
                  
                    Thread devThread_Fif = new Thread(() =>
                    {
                        try
                        {
                

                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 400, 499, out hv_RadiusMean4,
                                      out hv_Radius5, out hv_M4);
                            
                          }
                          catch (HalconException exc)
                          {
               
                          }
                      });

                    devThread_Fif.Start();

                    if (hv_RadiusMean5 == null)
                        hv_RadiusMean5 = new HTuple();
                    
                    if (hv_Radius6 == null)
                        hv_Radius6 = new HTuple();
                    if (hv_M5 == null)
                        hv_M5 = new HTuple();

                    Thread devThread_six = new Thread(() =>
                    {
                        try
                        {
                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 500, 599, out hv_RadiusMean5,
                                        out hv_Radius6, out hv_M5);

                        }
                        catch (HalconException exc)
                        {
               
                        }
                    });
                    devThread_six.Start();

                    if (hv_RadiusMean6 == null)
                        hv_RadiusMean6 = new HTuple();
                    if (hv_Radius7 == null)
                        hv_Radius7 = new HTuple();
                  
                    if (hv_M6 == null)
                        hv_M6 = new HTuple();
                 
                    Thread devThread_seven = new Thread(() =>
                    {
                        try
                        {
                

                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 600, 699, out hv_RadiusMean6,
                                        out hv_Radius7, out hv_M6);
                            
                        }
                        catch (HalconException exc)
                        {
              
                        }
                    });
                    devThread_seven.Start();

                    if (hv_RadiusMean7 == null)
                        hv_RadiusMean7 = new HTuple();
                    if (hv_Radius8 == null)
                        hv_Radius8 = new HTuple();
                    if (hv_M7 == null)
                        hv_M7 = new HTuple();
                    Thread devThread_eight = new Thread(() =>
                    {
                        try
                        {
               

                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 700, 799, out hv_RadiusMean7,
                                        out hv_Radius8, out hv_M7);

                        }
                        catch (HalconException exc)
                        {
               
                        }
                    });
                    devThread_eight.Start();


                    if (hv_RadiusMean8 == null)
                        hv_RadiusMean8 = new HTuple();
                    if (hv_Radius9 == null)
                        hv_Radius9 = new HTuple();
                    if (hv_M8 == null)
                        hv_M8 = new HTuple();
                    Thread devThread_nine = new Thread(() =>
                    {
                        try
                        {
               
                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 800, 899, out hv_RadiusMean8,
                                        out hv_Radius9, out hv_M8);

                
                        }
                        catch (HalconException exc)
                        {
               
                        }
                    });
                    devThread_nine.Start();


                    if (hv_RadiusMean9 == null)
                        hv_RadiusMean9 = new HTuple();
                    if (hv_Radius10 == null)
                        hv_Radius10 = new HTuple();
                    if (hv_M9 == null)
                        hv_M9 = new HTuple();
                    Thread devThread_ten = new Thread(() =>
                    {
                        try
                        {
                
                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 900, 999, out hv_RadiusMean9,
                                        out hv_Radius10, out hv_M9);


                        }
                        catch (HalconException exc)
                        {
                
                        }
                    });

                    devThread_ten.Start();


                    if (hv_RadiusMean10 == null)
                        hv_RadiusMean10 = new HTuple();
                    if (hv_Radius11 == null)
                        hv_Radius11 = new HTuple();
                    if (hv_M10 == null)
                        hv_M10 = new HTuple();
                    
                    Thread devThread_eleven = new Thread(() =>
                    {
                        try
                        {
                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 1000, 1099, out hv_RadiusMean10,
                                        out hv_Radius11, out hv_M10);
                            
                        }
                        catch (HalconException exc)
                        {
               
                        }
                    });

                    devThread_eleven.Start();

                    if (hv_RadiusMean11 == null)
                        hv_RadiusMean11 = new HTuple();
                    if (hv_Radius12 == null)
                        hv_Radius12 = new HTuple();
                    if (hv_M11 == null)
                        hv_M11 = new HTuple();
                   
                    Thread devThread_twelve = new Thread(() =>
                    {
                        try
                        {
                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 1100, 1199, out hv_RadiusMean11,
                                        out hv_Radius12, out hv_M11);
                            
                        }
                        catch (HalconException exc)
                        {
                
                        }
                    });
                    devThread_twelve.Start();

                    if (hv_RadiusMean12 == null)
                        hv_RadiusMean12 = new HTuple();
                   
                    if (hv_Radius13 == null)
                        hv_Radius13 = new HTuple();
                    if (hv_M12 == null)
                        hv_M12 = new HTuple();
                    Thread devThread_thirteen = new Thread(() =>
                    {
                        try
                        {
                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 1200, 1299, out hv_RadiusMean12,
                                        out hv_Radius13, out hv_M12);
                            
                        }
                        catch (HalconException exc)
                        {
                
                        }
                    });
                    devThread_thirteen.Start();



                    if (hv_RadiusMean13 == null)
                        hv_RadiusMean13 = new HTuple();
                    if (hv_Radius14 == null)
                        hv_Radius14 = new HTuple();
                    if (hv_M13 == null)
                        hv_M13 = new HTuple();
                  
                    Thread devThread_fourteen = new Thread(() =>
                    {
                        try
                        {
                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 1300, 1399, out hv_RadiusMean13,
                                        out hv_Radius14, out hv_M13);
                            
                        }
                        catch (HalconException exc)
                        {
               
                        }
                    });
                    devThread_fourteen.Start();

                    if (hv_RadiusMean14 == null)
                        hv_RadiusMean14 = new HTuple();
                    if (hv_Radius15 == null)
                        hv_Radius15 = new HTuple();
                    if (hv_M14 == null)
                        hv_M14 = new HTuple();
                   
                    Thread devThread_fifteen = new Thread(() =>
                    {
                        try
                        {
               
                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 1400, 1499, out hv_RadiusMean14,
                                        out hv_Radius15, out hv_M14);
                            
                        }
                        catch (HalconException exc)
                        {
              
                        }
                    });
                    devThread_fifteen.Start();

                    if (hv_RadiusMean15 == null)
                        hv_RadiusMean15 = new HTuple();
                    if (hv_Radius16 == null)
                        hv_Radius16 = new HTuple();
                    if (hv_M15 == null)
                        hv_M15 = new HTuple();
                  
                    Thread devThread_sixteen = new Thread(() =>
                    {
                        try
                        {
                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 1500, 1599, out hv_RadiusMean15,
                                        out hv_Radius16, out hv_M15);
                            
                        }
                        catch (HalconException exc)
                        {
               
                        }
                    });
                    devThread_sixteen.Start();

                    if (hv_RadiusMean16 == null)
                        hv_RadiusMean16 = new HTuple();
                    if (hv_Radius17 == null)
                        hv_Radius17 = new HTuple();
                    if (hv_M16 == null)
                        hv_M16 = new HTuple();
                    Thread devThread_seventeen = new Thread(() =>
                    {
                        try
                        {

                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 1600, 1699, out hv_RadiusMean16,
                                        out hv_Radius17, out hv_M16);



                        }
                        catch (HalconException exc)
                        {
              
                        }
                    });
                    devThread_seventeen.Start();


                    if (hv_RadiusMean17 == null)
                        hv_RadiusMean17 = new HTuple();
                    if (hv_Radius18 == null)
                        hv_Radius18 = new HTuple();
                    if (hv_M17 == null)
                        hv_M17 = new HTuple();
                    Thread devThread_eighteen = new Thread(() =>
                    {
                        try
                        {


                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 1700, 1799, out hv_RadiusMean17,
                                        out hv_Radius18, out hv_M17);



                        }
                        catch (HalconException exc)
                        {
               
                        }
                    });
                    devThread_eighteen.Start();


                    if (hv_RadiusMean18 == null)
                        hv_RadiusMean18 = new HTuple();
                    if (hv_Radius19 == null)
                        hv_Radius19 = new HTuple();
                    if (hv_M18 == null)
                        hv_M18 = new HTuple();
                    Thread devThread_nineteen = new Thread(() =>
                    {
                        try
                        {
                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 1800, 1899, out hv_RadiusMean18,
                                        out hv_Radius19, out hv_M18);
                            
                        }
                        catch (HalconException exc)
                        {
               
                        }
                    });
                    devThread_nineteen.Start();

                    if (hv_RadiusMean19 == null)
                        hv_RadiusMean19 = new HTuple();
                    if (hv_Radius20 == null)
                        hv_Radius20 = new HTuple();
                    if (hv_M19 == null)
                        hv_M19 = new HTuple();
                    Thread devThread_twenty = new Thread(() =>
                    {
                        try
                        {
                            // Call Fb
                            Fb(ho_ImageHeight, hv_Height, hv_Width, 1900, hv_Height - 1, out hv_RadiusMean19,
                                        out hv_Radius20, out hv_M19);

                        }
                        catch (HalconException exc)
                        {

                        }
                    });
                    devThread_twenty.Start();
                    
                    //mean100为每100个点的平均值，RadiusPlus为所有点的集合（用于计算坐标）**
                    while (true)
                    {
                        try
                        {
                            hv_MP.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_MP = ((((((((((((((((((hv_M + hv_M1) + hv_M2) + hv_M3) + hv_M4) + hv_M5) + hv_M6) + hv_M7) + hv_M8) + hv_M9) + hv_M10) + hv_M11) + hv_M12) + hv_M13) + hv_M14) + hv_M15) + hv_M16) + hv_M17) + hv_M18) + hv_M19;
                            }
                            if ((int)(hv_MP.TupleEqual(20)) != 0)
                            {
                                hv_mean100.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_mean100 = new HTuple();
                                    hv_mean100 = hv_mean100.TupleConcat(hv_RadiusMean, hv_RadiusMean1, hv_RadiusMean2, hv_RadiusMean3, hv_RadiusMean4, hv_RadiusMean5, hv_RadiusMean6, hv_RadiusMean7, hv_RadiusMean8, hv_RadiusMean9, hv_RadiusMean10, hv_RadiusMean11, hv_RadiusMean12, hv_RadiusMean13, hv_RadiusMean14, hv_RadiusMean15, hv_RadiusMean16, hv_RadiusMean17, hv_RadiusMean18, hv_RadiusMean19);
                                }
                                hv_RadiusPlus.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_RadiusPlus = new HTuple();
                                    hv_RadiusPlus = hv_RadiusPlus.TupleConcat(hv_Radius1, hv_Radius2, hv_Radius3, hv_Radius4, hv_Radius5, hv_Radius6, hv_Radius7, hv_Radius8, hv_Radius9, hv_Radius10, hv_Radius11, hv_Radius12, hv_Radius13, hv_Radius14, hv_Radius15, hv_Radius16, hv_Radius17, hv_Radius18, hv_Radius19, hv_Radius20);
                                }
                                break;
                            }
                            else
                            {
                                Thread.Sleep(1000);
                            }
                        }
                        // catch (Exception) 
                        catch (HalconException HDevExpDefaultException1)
                        {
                            HDevExpDefaultException1.ToHTuple(out hv_Exception);
                        }
                    }

                    hv_Width.Dispose();
                    hv_Height.Dispose();
                    hv_M.Dispose();
                    hv_M1.Dispose();
                    hv_M2.Dispose();
                    hv_M3.Dispose();
                    hv_M4.Dispose();
                    hv_M5.Dispose();
                    hv_M6.Dispose();
                    hv_M7.Dispose();
                    hv_M8.Dispose();
                    hv_M9.Dispose();
                    hv_M10.Dispose();
                    hv_M11.Dispose();
                    hv_M12.Dispose();
                    hv_M13.Dispose();
                    hv_M14.Dispose();
                    hv_M15.Dispose();
                    hv_M16.Dispose();
                    hv_M17.Dispose();
                    hv_M18.Dispose();
                    hv_M19.Dispose();
                    hv_M20.Dispose();
                    hv_M21.Dispose();
                    hv_M22.Dispose();
                    hv_M23.Dispose();
                    hv_RadiusMean.Dispose();
                    hv_Radius1.Dispose();
                    hv_ThreadID0.Dispose();
                    hv_RadiusMean1.Dispose();
                    hv_Radius2.Dispose();
                    hv_ThreadID1.Dispose();
                    hv_RadiusMean2.Dispose();
                    hv_Radius3.Dispose();
                    hv_ThreadID2.Dispose();
                    hv_RadiusMean3.Dispose();
                    hv_Radius4.Dispose();
                    hv_ThreadID3.Dispose();
                    hv_RadiusMean4.Dispose();
                    hv_Radius5.Dispose();
                    hv_ThreadID4.Dispose();
                    hv_RadiusMean5.Dispose();
                    hv_Radius6.Dispose();
                    hv_ThreadID5.Dispose();
                    hv_RadiusMean6.Dispose();
                    hv_Radius7.Dispose();
                    hv_ThreadID6.Dispose();
                    hv_RadiusMean7.Dispose();
                    hv_Radius8.Dispose();
                    hv_ThreadID7.Dispose();
                    hv_RadiusMean8.Dispose();
                    hv_Radius9.Dispose();
                    hv_ThreadID8.Dispose();
                    hv_RadiusMean9.Dispose();
                    hv_Radius10.Dispose();
                    hv_ThreadID9.Dispose();
                    hv_RadiusMean10.Dispose();
                    hv_Radius11.Dispose();
                    hv_ThreadID10.Dispose();
                    hv_RadiusMean11.Dispose();
                    hv_Radius12.Dispose();
                    hv_ThreadID11.Dispose();
                    hv_RadiusMean12.Dispose();
                    hv_Radius13.Dispose();
                    hv_ThreadID12.Dispose();
                    hv_RadiusMean13.Dispose();
                    hv_Radius14.Dispose();
                    hv_ThreadID13.Dispose();
                    hv_RadiusMean14.Dispose();
                    hv_Radius15.Dispose();
                    hv_ThreadID14.Dispose();
                    hv_RadiusMean15.Dispose();
                    hv_Radius16.Dispose();
                    hv_ThreadID15.Dispose();
                    hv_RadiusMean16.Dispose();
                    hv_Radius17.Dispose();
                    hv_ThreadID16.Dispose();
                    hv_RadiusMean17.Dispose();
                    hv_Radius18.Dispose();
                    hv_ThreadID17.Dispose();
                    hv_RadiusMean18.Dispose();
                    hv_Radius19.Dispose();
                    hv_ThreadID18.Dispose();
                    hv_RadiusMean19.Dispose();
                    hv_Radius20.Dispose();
                    hv_MP.Dispose();
                    hv_Exception.Dispose();

                    return;
                }
                catch (HalconException HDevExpDefaultException)
                {

                    hv_Width.Dispose();
                    hv_Height.Dispose();
                    hv_M.Dispose();
                    hv_M1.Dispose();
                    hv_M2.Dispose();
                    hv_M3.Dispose();
                    hv_M4.Dispose();
                    hv_M5.Dispose();
                    hv_M6.Dispose();
                    hv_M7.Dispose();
                    hv_M8.Dispose();
                    hv_M9.Dispose();
                    hv_M10.Dispose();
                    hv_M11.Dispose();
                    hv_M12.Dispose();
                    hv_M13.Dispose();
                    hv_M14.Dispose();
                    hv_M15.Dispose();
                    hv_M16.Dispose();
                    hv_M17.Dispose();
                    hv_M18.Dispose();
                    hv_M19.Dispose();
                    hv_M20.Dispose();
                    hv_M21.Dispose();
                    hv_M22.Dispose();
                    hv_M23.Dispose();
                    hv_RadiusMean.Dispose();
                    hv_Radius1.Dispose();
                    hv_ThreadID0.Dispose();
                    hv_RadiusMean1.Dispose();
                    hv_Radius2.Dispose();
                    hv_ThreadID1.Dispose();
                    hv_RadiusMean2.Dispose();
                    hv_Radius3.Dispose();
                    hv_ThreadID2.Dispose();
                    hv_RadiusMean3.Dispose();
                    hv_Radius4.Dispose();
                    hv_ThreadID3.Dispose();
                    hv_RadiusMean4.Dispose();
                    hv_Radius5.Dispose();
                    hv_ThreadID4.Dispose();
                    hv_RadiusMean5.Dispose();
                    hv_Radius6.Dispose();
                    hv_ThreadID5.Dispose();
                    hv_RadiusMean6.Dispose();
                    hv_Radius7.Dispose();
                    hv_ThreadID6.Dispose();
                    hv_RadiusMean7.Dispose();
                    hv_Radius8.Dispose();
                    hv_ThreadID7.Dispose();
                    hv_RadiusMean8.Dispose();
                    hv_Radius9.Dispose();
                    hv_ThreadID8.Dispose();
                    hv_RadiusMean9.Dispose();
                    hv_Radius10.Dispose();
                    hv_ThreadID9.Dispose();
                    hv_RadiusMean10.Dispose();
                    hv_Radius11.Dispose();
                    hv_ThreadID10.Dispose();
                    hv_RadiusMean11.Dispose();
                    hv_Radius12.Dispose();
                    hv_ThreadID11.Dispose();
                    hv_RadiusMean12.Dispose();
                    hv_Radius13.Dispose();
                    hv_ThreadID12.Dispose();
                    hv_RadiusMean13.Dispose();
                    hv_Radius14.Dispose();
                    hv_ThreadID13.Dispose();
                    hv_RadiusMean14.Dispose();
                    hv_Radius15.Dispose();
                    hv_ThreadID14.Dispose();
                    hv_RadiusMean15.Dispose();
                    hv_Radius16.Dispose();
                    hv_ThreadID15.Dispose();
                    hv_RadiusMean16.Dispose();
                    hv_Radius17.Dispose();
                    hv_ThreadID16.Dispose();
                    hv_RadiusMean17.Dispose();
                    hv_Radius18.Dispose();
                    hv_ThreadID17.Dispose();
                    hv_RadiusMean18.Dispose();
                    hv_Radius19.Dispose();
                    hv_ThreadID18.Dispose();
                    hv_RadiusMean19.Dispose();
                    hv_Radius20.Dispose();
                    hv_MP.Dispose();
                    hv_Exception.Dispose();

                    throw HDevExpDefaultException;
                }
            }
        }
        // Procedures 
        public void Creat_XYZ_From_sszn_COPY_1(HObject ho_ImageZ, out HObject ho_ImageConvertedX,
            out HObject ho_ImageConvertedY, out HObject ho_ImageReducedZ1, HTuple hv_yInterval,
            out HTuple hv_ObjectModel3D)
        {
            
            // Local iconic variables 

            HObject ho_ImageX, ho_ImageY, ho_ImageConvertedZ;
            HObject ho_Region, ho_ImageReducedZ;

            // Local control variables 

            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_xInterval = new HTuple(), hv_Min = new HTuple();
            HTuple hv_Max = new HTuple(), hv_Range = new HTuple();
            HTuple hv_ScaleZ = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageConvertedX);
            HOperatorSet.GenEmptyObj(out ho_ImageConvertedY);
            HOperatorSet.GenEmptyObj(out ho_ImageReducedZ1);
            HOperatorSet.GenEmptyObj(out ho_ImageX);
            HOperatorSet.GenEmptyObj(out ho_ImageY);
            HOperatorSet.GenEmptyObj(out ho_ImageConvertedZ);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_ImageReducedZ);
            hv_ObjectModel3D = new HTuple();
            hv_Width.Dispose(); hv_Height.Dispose();
            HOperatorSet.GetImageSize(ho_ImageZ, out hv_Width, out hv_Height);

            //生成XY图像
            hv_xInterval.Dispose();
            hv_xInterval = 0.09;
            //yInterval := 0.09
            ho_ImageX.Dispose();
            HOperatorSet.GenImageSurfaceFirstOrder(out ho_ImageX, "real", 0, hv_xInterval,
                0, 0, 0, hv_Width, hv_Height);
            ho_ImageY.Dispose();
            HOperatorSet.GenImageSurfaceFirstOrder(out ho_ImageY, "real", hv_yInterval, 0,
                0, 0, 0, hv_Width, hv_Height);

            //对于real类型的图像不再需要一下操作
            ho_ImageConvertedX.Dispose();
            HOperatorSet.ConvertImageType(ho_ImageX, out ho_ImageConvertedX, "real");
            ho_ImageConvertedY.Dispose();
            HOperatorSet.ConvertImageType(ho_ImageY, out ho_ImageConvertedY, "real");
            ho_ImageConvertedZ.Dispose();
            HOperatorSet.ConvertImageType(ho_ImageZ, out ho_ImageConvertedZ, "real");

            //计算数据最大最小值
            hv_Min.Dispose(); hv_Max.Dispose(); hv_Range.Dispose();
            HOperatorSet.MinMaxGray(ho_ImageZ, ho_ImageZ, 0, out hv_Min, out hv_Max, out hv_Range);
            ho_Region.Dispose();
            HOperatorSet.Threshold(ho_ImageZ, out ho_Region, -1500, hv_Max);
            ho_ImageReducedZ.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageZ, ho_Region, out ho_ImageReducedZ);
            //设置Z方向缩放系数,最终高度 = ScaleZ * GrayValue值，ScaleZ越小高度值范围也越小，点云效果接近平面，反之越明显
            //确保在0值以上，可通过min_max_gray获取最小值然后拉伸到0以上
            hv_ScaleZ.Dispose();
            hv_ScaleZ = 1;
            ho_ImageReducedZ1.Dispose();
            HOperatorSet.ScaleImage(ho_ImageReducedZ, out ho_ImageReducedZ1, hv_ScaleZ, 0);
            hv_ObjectModel3D.Dispose();
            HOperatorSet.XyzToObjectModel3d(ho_ImageConvertedX, ho_ImageConvertedY, ho_ImageReducedZ1,
                out hv_ObjectModel3D);

            ho_ImageX.Dispose();
            ho_ImageY.Dispose();
            ho_ImageConvertedZ.Dispose();
            ho_Region.Dispose();
            ho_ImageReducedZ.Dispose();

            hv_Width.Dispose();
            hv_Height.Dispose();
            hv_xInterval.Dispose();
            hv_Min.Dispose();
            hv_Max.Dispose();
            hv_Range.Dispose();
            hv_ScaleZ.Dispose();

            return;
        }
         public void gen_cam_par_area_scan_telecentric_division (HTuple hv_Magnification, 
      HTuple hv_Kappa, HTuple hv_Sx, HTuple hv_Sy, HTuple hv_Cx, HTuple hv_Cy, HTuple hv_ImageWidth, 
      HTuple hv_ImageHeight, out HTuple hv_CameraParam)
          {

            try
            {
                // Local iconic variables 
                // Initialize local and output iconic variables 
                hv_CameraParam = new HTuple();
                //Generate a camera parameter tuple for an area scan camera
                //with a telecentric lens and with distortions modeled by the
                //division model.
                //
                hv_CameraParam.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_CameraParam = new HTuple();
                    hv_CameraParam[0] = "area_scan_telecentric_division";
                    hv_CameraParam = hv_CameraParam.TupleConcat(hv_Magnification, hv_Kappa, hv_Sx, hv_Sy, hv_Cx, hv_Cy, hv_ImageWidth, hv_ImageHeight);
                }
                return;
            }
            catch (HalconException HDevExpDefaultException1)
            {
                hv_CameraParam = new HTuple();
            }

            

            return;
          }

        public void project_object_model_3d_lines_to_contour_xld(out HObject ho_Intersection,
      HTuple hv_PoseIntersectionPlane, HTuple hv_ObjectModel3DIntersection, out HTuple hGetValue)
        {
            
            // Local iconic variables 

            // Local control variables 

            HTuple hv_PoseInvert = new HTuple(), hv_Diameter = new HTuple();
            HTuple hv_Exception = new HTuple(), hv_Scale = new HTuple();
            HTuple hv_CamParam = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Intersection);

            //Determine the intersections and convert them into XLD contours

            //The inverted intersection plane pose is our projection pose
            hv_PoseInvert.Dispose();
            HOperatorSet.PoseInvert(hv_PoseIntersectionPlane, out hv_PoseInvert);
            //Make sure, the projection plane lies in front of the camera
            try
            {
                hv_Diameter.Dispose();
                HOperatorSet.GetObjectModel3dParams(hv_ObjectModel3DIntersection, "diameter_axis_aligned_bounding_box",
                    out hv_Diameter);
            }
            // catch (Exception) 
            catch (HalconException HDevExpDefaultException1)
            {
                //HDevExpDefaultException1.ToHTuple(out hv_Exception);
                hGetValue = 0;
                hv_PoseInvert.Dispose();
                hv_Diameter.Dispose();
                hv_Exception.Dispose();
                hv_Scale.Dispose();
                hv_CamParam.Dispose();

                return;
            }
            hGetValue = 1;
            if (hv_PoseInvert == null)
                hv_PoseInvert = new HTuple();
            hv_PoseInvert[2] = (hv_PoseInvert.TupleSelect(2)) + hv_Diameter;
            //Use a parallel projection to achieve the desired scaling (default 1:1)
            hv_Scale.Dispose();
            hv_Scale = 1;
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_CamParam.Dispose();
                gen_cam_par_area_scan_telecentric_division(1.0, 0, 1.0 / hv_Scale, 1.0 / hv_Scale,
                    0, 0, 512, 512, out hv_CamParam);
            }
            ho_Intersection.Dispose();
            HOperatorSet.ProjectObjectModel3d(out ho_Intersection, hv_ObjectModel3DIntersection,
                hv_CamParam, hv_PoseInvert, "data", "lines");

            hv_PoseInvert.Dispose();
            hv_Diameter.Dispose();
            hv_Exception.Dispose();
            hv_Scale.Dispose();
            hv_CamParam.Dispose();

            return;
        }

        public void td(HObject ho_Image, HTuple hv_PoseInvert, out HTuple hv_raduis)
        {
            // Local iconic variables 

            HObject ho_ImageOut = null, ho_ImagePart, ho_Rectangle2;
            HObject ho_Rectangle1, ho_ImageReduced, ho_ACameraa = null;
            HObject ho_BCamerab = null, ho_Bx = null, ho_By = null, ho_Bz = null;
            HObject ho_Ax = null, ho_Ay = null, ho_Az = null, ho_Intersection = null;
            HObject ho_UnionContours = null, ho_ObjectSelected = null, ho_ContCircle = null;

            // Local control variables 

            HTuple hv_Exception = new HTuple(), hv_ObjectModel3DB = new HTuple();
            HTuple hv_ObjectModel3DA = new HTuple(), hv_ObjectModel3DConnected = new HTuple();
            HTuple hv_ObjectModel3DRigidTrans = new HTuple(), hv_UnionObjectModel3 = new HTuple();
            HTuple hv_SampledObjectModel3D = new HTuple(), hv_X = new HTuple();
            HTuple hv_Y = new HTuple(), hv_Z = new HTuple(), hv_ObjectModel3D1 = new HTuple();
            HTuple hv_Surface3DDefault = new HTuple(), hv_Info = new HTuple();
            HTuple hv_CenterPoint = new HTuple(), hv_Radius = new HTuple();
            HTuple hv_Pose1 = new HTuple(), hv_ObjectModel3DIntersections = new HTuple();
            HTuple hv_n = new HTuple(), hv_ObjectModel3DIntersection = new HTuple();
            HTuple hv_PoseT = new HTuple(), hv_Length = new HTuple();
            HTuple hv_Max = new HTuple(), hv_Indices = new HTuple();
            HTuple hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_Radius1 = new HTuple(), hv_StartPhi = new HTuple();
            HTuple hv_EndPhi = new HTuple(), hv_PointOrder = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageOut);
            HOperatorSet.GenEmptyObj(out ho_ImagePart);
            HOperatorSet.GenEmptyObj(out ho_Rectangle2);
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_ACameraa);
            HOperatorSet.GenEmptyObj(out ho_BCamerab);
            HOperatorSet.GenEmptyObj(out ho_Bx);
            HOperatorSet.GenEmptyObj(out ho_By);
            HOperatorSet.GenEmptyObj(out ho_Bz);
            HOperatorSet.GenEmptyObj(out ho_Ax);
            HOperatorSet.GenEmptyObj(out ho_Ay);
            HOperatorSet.GenEmptyObj(out ho_Az);
            HOperatorSet.GenEmptyObj(out ho_Intersection);
            HOperatorSet.GenEmptyObj(out ho_UnionContours);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelected);
            HOperatorSet.GenEmptyObj(out ho_ContCircle);
            hv_raduis = new HTuple();
            ho_ImageOut.Dispose();
            ho_ImageOut = new HObject(ho_Image);
            ho_ImagePart.Dispose();
            HOperatorSet.ConvertImageType(ho_ImageOut, out ho_ImagePart, "real");
            ho_ImageOut.Dispose();
            HOperatorSet.ScaleImage(ho_ImagePart, out ho_ImageOut, 0.00001, 0);
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                ho_Rectangle2.Dispose();
                HOperatorSet.GenRectangle1(out ho_Rectangle2, 0, 0, 2000 - 1, 3200 - 1);
            }
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                ho_Rectangle1.Dispose();
                HOperatorSet.GenRectangle1(out ho_Rectangle1, 0, 3200, 2000 - 1, 6400 - 1);
            }
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageOut, ho_Rectangle2, out ho_ImageReduced);
            try
            {

                ho_ACameraa.Dispose();
                HOperatorSet.CropDomain(ho_ImageReduced, out ho_ACameraa);
                ho_ImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_ImageOut, ho_Rectangle1, out ho_ImageReduced);
                ho_BCamerab.Dispose();
                HOperatorSet.CropDomain(ho_ImageReduced, out ho_BCamerab);
                
                ho_Bx.Dispose(); ho_By.Dispose(); ho_Bz.Dispose(); hv_ObjectModel3DB.Dispose();
                Creat_XYZ_From_sszn_COPY_1(ho_BCamerab, out ho_Bx, out ho_By, out ho_Bz, 0.09,
                    out hv_ObjectModel3DB);
                ho_Ax.Dispose(); ho_Ay.Dispose(); ho_Az.Dispose(); hv_ObjectModel3DA.Dispose();
                Creat_XYZ_From_sszn_COPY_1(ho_ACameraa, out ho_Ax, out ho_Ay, out ho_Az, 0.09,
                    out hv_ObjectModel3DA);

                hv_ObjectModel3DConnected.Dispose();
                HOperatorSet.ConnectionObjectModel3d(hv_ObjectModel3DB, "distance_3d", 0.2,
                    out hv_ObjectModel3DConnected);
                hv_ObjectModel3DB.Dispose();
                HOperatorSet.SelectObjectModel3d(hv_ObjectModel3DConnected, "num_points", "and",
                    10000, 1e10, out hv_ObjectModel3DB);
                hv_ObjectModel3DConnected.Dispose();
                HOperatorSet.ConnectionObjectModel3d(hv_ObjectModel3DA, "distance_3d", 0.2,
                    out hv_ObjectModel3DConnected);
                hv_ObjectModel3DA.Dispose();
                HOperatorSet.SelectObjectModel3d(hv_ObjectModel3DConnected, "num_points", "and",
                    100000, 1e10, out hv_ObjectModel3DA);
                hv_ObjectModel3DRigidTrans.Dispose();
                HOperatorSet.RigidTransObjectModel3d(hv_ObjectModel3DB, hv_PoseInvert, out hv_ObjectModel3DRigidTrans);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_UnionObjectModel3.Dispose();
                    HOperatorSet.UnionObjectModel3d(hv_ObjectModel3DRigidTrans.TupleConcat(hv_ObjectModel3DA),
                        "points_surface", out hv_UnionObjectModel3);
                }
                
                hv_ObjectModel3DB.Dispose();

                hv_SampledObjectModel3D.Dispose();
                HOperatorSet.SampleObjectModel3d(hv_ObjectModel3DA, "fast", 1, new HTuple(),
                    new HTuple(), out hv_SampledObjectModel3D);
                hv_X.Dispose();
                HOperatorSet.GetObjectModel3dParams(hv_SampledObjectModel3D, "point_coord_x",
                    out hv_X);
                hv_Y.Dispose();
                HOperatorSet.GetObjectModel3dParams(hv_SampledObjectModel3D, "point_coord_y",
                    out hv_Y);
                hv_Z.Dispose();
                HOperatorSet.GetObjectModel3dParams(hv_SampledObjectModel3D, "point_coord_z",
                    out hv_Z);
                hv_ObjectModel3D1.Dispose();
                HOperatorSet.GenObjectModel3dFromPoints(hv_X, hv_Y, hv_Z, out hv_ObjectModel3D1);
                hv_Surface3DDefault.Dispose(); hv_Info.Dispose();
                HOperatorSet.TriangulateObjectModel3d(hv_ObjectModel3D1, "greedy", new HTuple(),
                    new HTuple(), out hv_Surface3DDefault, out hv_Info);
                //***三维计算****
                hv_CenterPoint.Dispose(); hv_Radius.Dispose();
                HOperatorSet.SmallestSphereObjectModel3d(hv_Surface3DDefault, out hv_CenterPoint,
                    out hv_Radius);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Pose1.Dispose();
                    HOperatorSet.CreatePose(hv_CenterPoint.TupleSelect(0), hv_CenterPoint.TupleSelect(
                        1), hv_CenterPoint.TupleSelect(2), 90, 0, 0, "Rp+T", "gba", "point", out hv_Pose1);
                }
                hv_raduis.Dispose();
                hv_raduis = new HTuple();
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetPart(HDevWindowStack.GetActive(), -1, -1, 1, 1);
                }
                hv_ObjectModel3DIntersections.Dispose();
                hv_ObjectModel3DIntersections = new HTuple();
                HTuple hv_getValue = 1;
                for (hv_n = 1; (int)hv_n <= (int)(2000 - 1); hv_n = hv_n + 1)
                {
                    if (hv_Pose1 == null)
                        hv_Pose1 = new HTuple();
                    hv_Pose1[1] = hv_n * 0.089;
                    hv_ObjectModel3DIntersection.Dispose();
                    HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefault, hv_Pose1, out hv_ObjectModel3DIntersection);
                    hv_PoseT.Dispose();
                    hv_PoseT = new HTuple(hv_Pose1);
                    try
                    {
                        hv_getValue = 1;
                        ho_Intersection.Dispose();
                        project_object_model_3d_lines_to_contour_xld(out ho_Intersection, hv_PoseT,
                            hv_ObjectModel3DIntersection, out hv_getValue);

                        if (hv_getValue == 0)
                        {
                            hv_n += 4;
                            continue;
                        }
                        ho_UnionContours.Dispose();
                        HOperatorSet.UnionAdjacentContoursXld(ho_Intersection, out ho_UnionContours,
                            50, 1, "attr_keep");

                        hv_Length.Dispose();
                        HOperatorSet.LengthXld(ho_UnionContours, out hv_Length);
                        hv_Max.Dispose();
                        HOperatorSet.TupleMax(hv_Length, out hv_Max);
                        hv_Indices.Dispose();
                        HOperatorSet.TupleFind(hv_Length, hv_Max, out hv_Indices);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            ho_ObjectSelected.Dispose();
                            HOperatorSet.SelectObj(ho_UnionContours, out ho_ObjectSelected, hv_Indices + 1);
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_ObjectSelected, HDevWindowStack.GetActive());
                        }


                        hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius1.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                        HOperatorSet.FitCircleContourXld(ho_ObjectSelected, "geohuber", -1, 0,
                            0, 3, 2, out hv_Row, out hv_Column, out hv_Radius1, out hv_StartPhi,
                            out hv_EndPhi, out hv_PointOrder);
                        ho_ContCircle.Dispose();
                        HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column,
                            hv_Radius1, 0, 6.28318, "positive", 1);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_raduis = hv_raduis.TupleConcat(
                                    hv_Radius1);
                                hv_raduis.Dispose();
                                hv_raduis = ExpTmpLocalVar_raduis;
                            }
                        }
                    }
                    // catch (Exception) 
                    catch (HalconException HDevExpDefaultException2)
                    {
                        HDevExpDefaultException2.ToHTuple(out hv_Exception);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_raduis = hv_raduis.TupleConcat(
                                    0);
                                hv_raduis.Dispose();
                                hv_raduis = ExpTmpLocalVar_raduis;
                            }
                        }
                    }

                }

            }
            // catch (Exception) 
            catch (HalconException HDevExpDefaultException1)
            {
                HDevExpDefaultException1.ToHTuple(out hv_Exception);
                if (hv_raduis == null)
                    hv_raduis = new HTuple();
                hv_raduis[1999] = 0;
            }
            ho_ImageOut.Dispose();
            ho_ImagePart.Dispose();
            ho_Rectangle2.Dispose();
            ho_Rectangle1.Dispose();
            ho_ImageReduced.Dispose();
            ho_ACameraa.Dispose();
            ho_BCamerab.Dispose();
            ho_Bx.Dispose();
            ho_By.Dispose();
            ho_Bz.Dispose();
            ho_Ax.Dispose();
            ho_Ay.Dispose();
            ho_Az.Dispose();
            ho_Intersection.Dispose();
            ho_UnionContours.Dispose();
            ho_ObjectSelected.Dispose();
            ho_ContCircle.Dispose();

            hv_Exception.Dispose();
            hv_ObjectModel3DB.Dispose();
            hv_ObjectModel3DA.Dispose();
            hv_ObjectModel3DConnected.Dispose();
            hv_ObjectModel3DRigidTrans.Dispose();
            hv_UnionObjectModel3.Dispose();
            hv_SampledObjectModel3D.Dispose();
            hv_X.Dispose();
            hv_Y.Dispose();
            hv_Z.Dispose();
            hv_ObjectModel3D1.Dispose();
            hv_Surface3DDefault.Dispose();
            hv_Info.Dispose();
            hv_CenterPoint.Dispose();
            hv_Radius.Dispose();
            hv_Pose1.Dispose();
            hv_ObjectModel3DIntersections.Dispose();
            hv_n.Dispose();
            hv_ObjectModel3DIntersection.Dispose();
            hv_PoseT.Dispose();
            hv_Length.Dispose();
            hv_Max.Dispose();
            hv_Indices.Dispose();
            hv_Row.Dispose();
            hv_Column.Dispose();
            hv_Radius1.Dispose();
            hv_StartPhi.Dispose();
            hv_EndPhi.Dispose();
            hv_PointOrder.Dispose();

            return;
        }

        public void tdtmp(HObject ho_Image, HTuple hv_PoseInvert, out HTuple hv_raduis)
        {
            
            // Local iconic variables 

            HObject ho_ImageOut = null, ho_ImagePart, ho_Rectangle2;
            HObject ho_Rectangle1, ho_ImageReduced, ho_ACameraa = null;
            HObject ho_BCamerab = null, ho_Bx = null, ho_By = null, ho_Bz = null;
            HObject ho_Ax = null, ho_Ay = null, ho_Az = null, ho_Intersection = null;
            HObject ho_UnionContours = null, ho_ObjectSelected = null, ho_ContCircle = null;

            // Local control variables 

            HTuple hv_Exception = new HTuple(), hv_ObjectModel3DB = new HTuple();
            HTuple hv_ObjectModel3DA = new HTuple(), hv_ObjectModel3DConnected = new HTuple();
            HTuple hv_ObjectModel3DRigidTrans = new HTuple(), hv_UnionObjectModel3 = new HTuple();
            HTuple hv_SampledObjectModel3D = new HTuple(), hv_X = new HTuple();
            HTuple hv_Y = new HTuple(), hv_Z = new HTuple(), hv_ObjectModel3D1 = new HTuple();
            HTuple hv_Surface3DDefault = new HTuple(), hv_Info = new HTuple();
            HTuple hv_CenterPoint = new HTuple(), hv_Radius = new HTuple();
            HTuple hv_Pose1 = new HTuple(), hv_ObjectModel3DIntersections = new HTuple();
            HTuple hv_n = new HTuple(), hv_ObjectModel3DIntersection = new HTuple();
            HTuple hv_PoseT = new HTuple(), hv_Length = new HTuple();
            HTuple hv_Max = new HTuple(), hv_Indices = new HTuple();
            HTuple hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_Radius1 = new HTuple(), hv_StartPhi = new HTuple();
            HTuple hv_EndPhi = new HTuple(), hv_PointOrder = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageOut);
            HOperatorSet.GenEmptyObj(out ho_ImagePart);
            HOperatorSet.GenEmptyObj(out ho_Rectangle2);
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_ACameraa);
            HOperatorSet.GenEmptyObj(out ho_BCamerab);
            HOperatorSet.GenEmptyObj(out ho_Bx);
            HOperatorSet.GenEmptyObj(out ho_By);
            HOperatorSet.GenEmptyObj(out ho_Bz);
            HOperatorSet.GenEmptyObj(out ho_Ax);
            HOperatorSet.GenEmptyObj(out ho_Ay);
            HOperatorSet.GenEmptyObj(out ho_Az);
            HOperatorSet.GenEmptyObj(out ho_Intersection);
            HOperatorSet.GenEmptyObj(out ho_UnionContours);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelected);
            HOperatorSet.GenEmptyObj(out ho_ContCircle);
            hv_raduis = new HTuple();
            ho_ImageOut.Dispose();
            ho_ImageOut = new HObject(ho_Image);
            ho_ImagePart.Dispose();
            HOperatorSet.ConvertImageType(ho_ImageOut, out ho_ImagePart, "real");
            ho_ImageOut.Dispose();
            HOperatorSet.ScaleImage(ho_ImagePart, out ho_ImageOut, 0.00001, 0);
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                ho_Rectangle2.Dispose();
                HOperatorSet.GenRectangle1(out ho_Rectangle2, 0, 0, 2000 - 1, 3200 - 1);
            }
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                ho_Rectangle1.Dispose();
                HOperatorSet.GenRectangle1(out ho_Rectangle1, 0, 3200, 2000 - 1, 6400 - 1);
            }
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageOut, ho_Rectangle2, out ho_ImageReduced);
            try
            {

                ho_ACameraa.Dispose();
                HOperatorSet.CropDomain(ho_ImageReduced, out ho_ACameraa);
                ho_ImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_ImageOut, ho_Rectangle1, out ho_ImageReduced);
                ho_BCamerab.Dispose();
                HOperatorSet.CropDomain(ho_ImageReduced, out ho_BCamerab);

            }
            // catch (Exception) 
            catch (HalconException HDevExpDefaultException1)
            {
                HDevExpDefaultException1.ToHTuple(out hv_Exception);
            }
            try
            {

                ho_Bx.Dispose(); ho_By.Dispose(); ho_Bz.Dispose(); hv_ObjectModel3DB.Dispose();
                Creat_XYZ_From_sszn_COPY_1(ho_BCamerab, out ho_Bx, out ho_By, out ho_Bz, 0.09,
                    out hv_ObjectModel3DB);
                ho_Ax.Dispose(); ho_Ay.Dispose(); ho_Az.Dispose(); hv_ObjectModel3DA.Dispose();
                Creat_XYZ_From_sszn_COPY_1(ho_ACameraa, out ho_Ax, out ho_Ay, out ho_Az, 0.09,
                    out hv_ObjectModel3DA);

                hv_ObjectModel3DConnected.Dispose();
                HOperatorSet.ConnectionObjectModel3d(hv_ObjectModel3DB, "distance_3d", 0.2,
                    out hv_ObjectModel3DConnected);
                hv_ObjectModel3DB.Dispose();
                HOperatorSet.SelectObjectModel3d(hv_ObjectModel3DConnected, "num_points", "and",
                    10000, 10000000000, out hv_ObjectModel3DB);
                hv_ObjectModel3DConnected.Dispose();
                HOperatorSet.ConnectionObjectModel3d(hv_ObjectModel3DA, "distance_3d", 0.2,
                    out hv_ObjectModel3DConnected);
                hv_ObjectModel3DA.Dispose();
                HOperatorSet.SelectObjectModel3d(hv_ObjectModel3DConnected, "num_points", "and",
                    100000, 10000000000, out hv_ObjectModel3DA);
                hv_ObjectModel3DRigidTrans.Dispose();
                HOperatorSet.RigidTransObjectModel3d(hv_ObjectModel3DB, hv_PoseInvert, out hv_ObjectModel3DRigidTrans);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_UnionObjectModel3.Dispose();
                    HOperatorSet.UnionObjectModel3d(hv_ObjectModel3DRigidTrans.TupleConcat(hv_ObjectModel3DA),
                        "points_surface", out hv_UnionObjectModel3);
                }
                hv_SampledObjectModel3D.Dispose();
                HOperatorSet.SampleObjectModel3d(hv_ObjectModel3DA, "fast", 1, new HTuple(),
                    new HTuple(), out hv_SampledObjectModel3D);

               

                hv_X.Dispose();
                HOperatorSet.GetObjectModel3dParams(hv_SampledObjectModel3D, "point_coord_x",
                    out hv_X);
                hv_Y.Dispose();
                HOperatorSet.GetObjectModel3dParams(hv_SampledObjectModel3D, "point_coord_y",
                    out hv_Y);
                hv_Z.Dispose();
                HOperatorSet.GetObjectModel3dParams(hv_SampledObjectModel3D, "point_coord_z",
                    out hv_Z);
                hv_ObjectModel3D1.Dispose();

                hv_ObjectModel3DB.Dispose();
                hv_ObjectModel3DA.Dispose();
                hv_ObjectModel3DConnected.Dispose();
                HOperatorSet.GenObjectModel3dFromPoints(hv_X, hv_Y, hv_Z, out hv_ObjectModel3D1);
                hv_Surface3DDefault.Dispose(); hv_Info.Dispose();
                HOperatorSet.TriangulateObjectModel3d(hv_ObjectModel3D1, "greedy", new HTuple(),
                    new HTuple(), out hv_Surface3DDefault, out hv_Info);
                //***三维计算****
                hv_CenterPoint.Dispose(); hv_Radius.Dispose();
                HOperatorSet.SmallestSphereObjectModel3d(hv_Surface3DDefault, out hv_CenterPoint,
                    out hv_Radius);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Pose1.Dispose();
                    HOperatorSet.CreatePose(hv_CenterPoint.TupleSelect(0), hv_CenterPoint.TupleSelect(
                        1), hv_CenterPoint.TupleSelect(2), 90, 0, 0, "Rp+T", "gba", "point", out hv_Pose1);
                }
                hv_raduis.Dispose();
                hv_raduis = new HTuple();
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetPart(HDevWindowStack.GetActive(), -1, -1, 1, 1);
                }
                hv_ObjectModel3DIntersections.Dispose();
                hv_ObjectModel3DIntersections = new HTuple();
                HTuple hGetValue = new HTuple();
                if (hv_Pose1 == null)
                    hv_Pose1 = new HTuple();

                float[] fRadis = new float[2000];
                Parallel.For(1, 2000, nIndex =>
                 {
                     HTuple hv_posenew = new HTuple(hv_Pose1);
                     HTuple hv_PoseTnew = new HTuple();
                     HTuple hv_ObjectModel3DIntersectionew = new HTuple();
                     HTuple hv_Lengthnew = new HTuple();
                     HTuple hv_Maxnew = new HTuple(), hv_Indicesnew = new HTuple();
                     HTuple hv_StartPhinew = new HTuple();
                     HTuple hv_EndPhinew = new HTuple(), hv_PointOrdernew = new HTuple();
                     
                     hv_posenew[1] = (float)nIndex * 0.089;
                     hv_ObjectModel3DIntersectionew.Dispose();
                     HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefault, hv_posenew, out hv_ObjectModel3DIntersectionew);
                     hv_PoseTnew.Dispose();
                     hv_PoseTnew = new HTuple(hv_posenew);
                     try
                     {
                        
                         HTuple hv_Rownew = new HTuple(), hv_Columnnew = new HTuple();
                         HTuple hv_Radius1new = new HTuple();
                         HObject ho_Intersectionnew = null;
                         HObject ho_UnionContoursnew = null;
                         HObject ho_ObjectSelectednew = null;
                         HObject ho_ContCirclenew = null; 
                         HOperatorSet.GenEmptyObj(out ho_Intersectionnew);
                         HOperatorSet.GenEmptyObj(out ho_UnionContoursnew);
                         HOperatorSet.GenEmptyObj(out ho_ObjectSelectednew);
                         HOperatorSet.GenEmptyObj(out ho_ContCirclenew);
                         
                         {
                             HTuple hv_PoseInvertnew = new HTuple(), hv_Diameter = new HTuple();
                             HTuple hv_Scale = new HTuple();
                             HTuple hv_CamParam = new HTuple();
                             // Initialize local and output iconic variables                            
                             //Determine the intersections and convert them into XLD contours
                             //The inverted intersection plane pose is our projection pose
                             hv_PoseInvertnew.Dispose();
                             HOperatorSet.PoseInvert(hv_PoseTnew, out hv_PoseInvertnew);
                             //Make sure, the projection plane lies in front of the camera
                             try
                             {
                                 hv_Diameter.Dispose();

                                 HOperatorSet.GetObjectModel3dParams(hv_ObjectModel3DIntersectionew, "diameter_axis_aligned_bounding_box",
                                     out hv_Diameter);
                             }
                             // catch (Exception) 
                             catch (HalconException HDevExpDefaultException1)
                             {

                                 hv_PoseInvert.Dispose();
                                 hv_Diameter.Dispose();
                                 hv_Exception.Dispose();
                                 hv_Scale.Dispose();
                                 hv_CamParam.Dispose();
                                 return;
                             }

                             if (hv_PoseInvertnew == null)
                                 hv_PoseInvertnew = new HTuple();
                             hv_PoseInvertnew[2] = (hv_PoseInvertnew.TupleSelect(2)) + hv_Diameter;
                             //Use a parallel projection to achieve the desired scaling (default 1:1)
                             hv_Scale.Dispose();
                             hv_Scale = 1;
                             using (HDevDisposeHelper dh = new HDevDisposeHelper())
                             {
                                 hv_CamParam.Dispose();

                                 using (HDevDisposeHelper dh1 = new HDevDisposeHelper())
                                 {
                                     hv_CamParam = new HTuple();
                                     hv_CamParam[0] = "area_scan_telecentric_division";
                                     hv_CamParam = hv_CamParam.TupleConcat(1.0, 0, 1.0 / hv_Scale, 1.0 / hv_Scale, 0, 0, 512, 512);
                                 }

                             }
                             ho_Intersectionnew.Dispose();
                             HOperatorSet.ProjectObjectModel3d(out ho_Intersectionnew, hv_ObjectModel3DIntersectionew,
                                 hv_CamParam, hv_PoseInvertnew, "data", "lines");

                             hv_PoseInvertnew.Dispose();
                             hv_Diameter.Dispose();
                             hv_Scale.Dispose();
                             hv_CamParam.Dispose();
                         }



                         //project_object_model_3d_lines_to_contour_xld(out ho_Intersection, hv_PoseT,
                         //    hv_ObjectModel3DIntersection, out hGetValue);
                         //if (hGetValue.TupleEqual(0) != 0)
                         //{
                         //    hv_raduis = hv_raduis.TupleConcat(0);
                         //    continue;
                         //}
                         ho_UnionContoursnew.Dispose();
                         HOperatorSet.UnionAdjacentContoursXld(ho_Intersectionnew, out ho_UnionContoursnew,
                             50, 1, "attr_keep");

                         hv_Lengthnew.Dispose();
                         HOperatorSet.LengthXld(ho_UnionContoursnew, out hv_Lengthnew);
                         hv_Maxnew.Dispose();
                         if (hv_Lengthnew.Length <= 0)
                         {
                             fRadis[(int)nIndex] = 0;
                             //hv_raduis.Dispose();
                             //hv_raduis = ExpTmpLocalVar_raduis;
                             return;
                         }
                         HOperatorSet.TupleMax(hv_Lengthnew, out hv_Maxnew);
                         hv_Indicesnew.Dispose();
                         HOperatorSet.TupleFind(hv_Lengthnew, hv_Maxnew, out hv_Indicesnew);
                         using (HDevDisposeHelper dh = new HDevDisposeHelper())
                         {
                             ho_ObjectSelectednew.Dispose();
                             HOperatorSet.SelectObj(ho_UnionContoursnew, out ho_ObjectSelectednew, hv_Indicesnew + 1);
                         }


                         hv_Rownew.Dispose(); hv_Columnnew.Dispose(); hv_Radius1new.Dispose(); hv_StartPhinew.Dispose(); hv_EndPhinew.Dispose(); hv_PointOrdernew.Dispose();
                         HOperatorSet.FitCircleContourXld(ho_ObjectSelectednew, "geohuber", -1, 0,
                             0, 3, 2, out hv_Rownew, out hv_Columnnew, out hv_Radius1new, out hv_StartPhinew,
                             out hv_EndPhinew, out hv_PointOrdernew);
                         ho_ContCirclenew.Dispose();
                         HOperatorSet.GenCircleContourXld(out ho_ContCirclenew, hv_Rownew, hv_Columnnew,
                             hv_Radius1new, 0, 6.28318, "positive", 1);


                         fRadis[nIndex] = (float)(hv_Radius1);
                         //hv_raduis.Dispose();
                         //hv_raduis = ExpTmpLocalVar_raduis;

                     }
                     // catch (Exception) 
                     catch (HalconException HDevExpDefaultException2)
                     {
                         fRadis[nIndex] = 0;
                     }
                 }) ;


                for(int i = 1; i <= 1999; i++)
                {
                    hv_raduis = hv_raduis.TupleConcat(new HTuple(fRadis[i]));
                }

                /*
                for (hv_n = 1; (int)hv_n <= (int)(2000 - 1); hv_n = (int)hv_n + 1)
                {
                    
                    hv_Pose1[1] = hv_n * 0.089;
                    hv_ObjectModel3DIntersection.Dispose();
                    HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefault, hv_Pose1, out hv_ObjectModel3DIntersection);
                    hv_PoseT.Dispose();
                    hv_PoseT = new HTuple(hv_Pose1);
                    try
                    {
                        hGetValue = 1;
                        ho_Intersection.Dispose();


                        {
                            HTuple hv_PoseInvertnew = new HTuple(), hv_Diameter = new HTuple();
                            HTuple hv_Scale = new HTuple();
                            HTuple hv_CamParam = new HTuple();
                            // Initialize local and output iconic variables                            
                            //Determine the intersections and convert them into XLD contours
                            //The inverted intersection plane pose is our projection pose
                            hv_PoseInvertnew.Dispose();
                            HOperatorSet.PoseInvert(hv_PoseT, out hv_PoseInvertnew);
                            //Make sure, the projection plane lies in front of the camera
                            try
                            {
                                hv_Diameter.Dispose();
                               
                                HOperatorSet.GetObjectModel3dParams(hv_ObjectModel3DIntersection, "diameter_axis_aligned_bounding_box",
                                    out hv_Diameter);
                            }
                            // catch (Exception) 
                            catch (HalconException HDevExpDefaultException1)
                            {
                               
                                hv_PoseInvert.Dispose();
                                hv_Diameter.Dispose();
                                hv_Exception.Dispose();
                                hv_Scale.Dispose();
                                hv_CamParam.Dispose();

                                continue;
                            }
                           
                            if (hv_PoseInvertnew == null)
                                hv_PoseInvertnew = new HTuple();
                            hv_PoseInvertnew[2] = (hv_PoseInvertnew.TupleSelect(2)) + hv_Diameter;
                            //Use a parallel projection to achieve the desired scaling (default 1:1)
                            hv_Scale.Dispose();
                            hv_Scale = 1;
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_CamParam.Dispose();
                             
                                using (HDevDisposeHelper dh1 = new HDevDisposeHelper())
                                {
                                    hv_CamParam = new HTuple();
                                    hv_CamParam[0] = "area_scan_telecentric_division";
                                    hv_CamParam = hv_CamParam.TupleConcat(1.0, 0, 1.0 / hv_Scale, 1.0 / hv_Scale, 0, 0, 512, 512);
                                }

                            }
                            ho_Intersection.Dispose();
                            HOperatorSet.ProjectObjectModel3d(out ho_Intersection, hv_ObjectModel3DIntersection,
                                hv_CamParam, hv_PoseInvert, "data", "lines");

                            hv_PoseInvert.Dispose();
                            hv_Diameter.Dispose();
                            hv_Scale.Dispose();
                            hv_CamParam.Dispose();
                        }



                        //project_object_model_3d_lines_to_contour_xld(out ho_Intersection, hv_PoseT,
                        //    hv_ObjectModel3DIntersection, out hGetValue);
                        //if (hGetValue.TupleEqual(0) != 0)
                        //{
                        //    hv_raduis = hv_raduis.TupleConcat(0);
                        //    continue;
                        //}
                        ho_UnionContours.Dispose();
                        HOperatorSet.UnionAdjacentContoursXld(ho_Intersection, out ho_UnionContours,
                            50, 1, "attr_keep");

                        hv_Length.Dispose();
                        HOperatorSet.LengthXld(ho_UnionContours, out hv_Length);
                        hv_Max.Dispose();
                        if (hv_Length.Length <= 0)
                        {
                            
                            hv_raduis = hv_raduis.TupleConcat(0);
                            //hv_raduis.Dispose();
                            //hv_raduis = ExpTmpLocalVar_raduis;
                           
                            continue;
                        }
                        HOperatorSet.TupleMax(hv_Length, out hv_Max);
                        hv_Indices.Dispose();
                        HOperatorSet.TupleFind(hv_Length, hv_Max, out hv_Indices);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            ho_ObjectSelected.Dispose();
                            HOperatorSet.SelectObj(ho_UnionContours, out ho_ObjectSelected, hv_Indices + 1);
                        }
                      

                        hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius1.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                        HOperatorSet.FitCircleContourXld(ho_ObjectSelected, "geohuber", -1, 0,
                            0, 3, 2, out hv_Row, out hv_Column, out hv_Radius1, out hv_StartPhi,
                            out hv_EndPhi, out hv_PointOrder);
                        ho_ContCircle.Dispose();
                        HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column,
                            hv_Radius1, 0, 6.28318, "positive", 1);
                        

                        hv_raduis = hv_raduis.TupleConcat(hv_Radius1);
                        //hv_raduis.Dispose();
                        //hv_raduis = ExpTmpLocalVar_raduis;
                            
                    }
                    // catch (Exception) 
                    catch (HalconException HDevExpDefaultException2)
                    {

                        hv_raduis = hv_raduis.TupleConcat(0);
                        //hv_raduis.Dispose();
                        //hv_raduis = ExpTmpLocalVar_raduis;
                            
                    }

                }
                */
            }
            // catch (Exception) 
            catch (HalconException HDevExpDefaultException1)
            {
                //HDevExpDefaultException1.ToHTuple(out hv_Exception);
                if (hv_raduis == null)
                {
                    hv_raduis = new HTuple();
                }
                hv_raduis[1999] = 0;
            }

            ho_ImageOut.Dispose();
            ho_ImagePart.Dispose();
            ho_Rectangle2.Dispose();
            ho_Rectangle1.Dispose();
            ho_ImageReduced.Dispose();
            ho_ACameraa.Dispose();
            ho_BCamerab.Dispose();
            ho_Bx.Dispose();
            ho_By.Dispose();
            ho_Bz.Dispose();
            ho_Ax.Dispose();
            ho_Ay.Dispose();
            ho_Az.Dispose();
            ho_Intersection.Dispose();
            ho_UnionContours.Dispose();
            ho_ObjectSelected.Dispose();
            ho_ContCircle.Dispose();

            hv_Exception.Dispose();
            //hv_ObjectModel3DB.Dispose();
            //hv_ObjectModel3DA.Dispose();
            //hv_ObjectModel3DConnected.Dispose();
            hv_ObjectModel3DRigidTrans.Dispose();
            hv_UnionObjectModel3.Dispose();
            hv_SampledObjectModel3D.Dispose();
            hv_X.Dispose();
            hv_Y.Dispose();
            hv_Z.Dispose();
            hv_ObjectModel3D1.Dispose();
            hv_Surface3DDefault.Dispose();
            hv_Info.Dispose();
            hv_CenterPoint.Dispose();
            hv_Radius.Dispose();
            hv_Pose1.Dispose();
            hv_ObjectModel3DIntersections.Dispose();
            hv_n.Dispose();
            hv_ObjectModel3DIntersection.Dispose();
            hv_PoseT.Dispose();
            hv_Length.Dispose();
            hv_Max.Dispose();
            hv_Indices.Dispose();
            hv_Row.Dispose();
            hv_Column.Dispose();
            hv_Radius1.Dispose();
            hv_StartPhi.Dispose();
            hv_EndPhi.Dispose();
            hv_PointOrder.Dispose();

            return;
        }

        public void L_C_Compute(HObject ho_Grayimage, HObject ho_Heightimage, HTuple hv_M,
      HTuple hv_PoseInvert, out HTuple hv_Radius, out HTuple hv_Q, out HTuple hv_Mean10)
        {
            
            // Local iconic variables 

            // Local control variables 

            HTuple hv_Seconds = new HTuple(), hv_raduis = new HTuple();
            HTuple hv_Index = new HTuple(), hv_Selected = new HTuple();
            HTuple hv_Mean = new HTuple(), hv_Seconds1 = new HTuple();
            HTuple hv_l = new HTuple(), hv_g = new HTuple(), hv_Greater = new HTuple();
            HTuple hv_Less = new HTuple(), hv_Indices1 = new HTuple();
            HTuple hv_Indices2 = new HTuple(), hv_k = new HTuple();
            HTuple hv_Reduced = new HTuple(), hv_Function = new HTuple();
            HTuple hv_Exception = new HTuple(), hv_e = new HTuple();
            // Initialize local and output iconic variables 
            hv_Radius = new HTuple();
            hv_Q = new HTuple();
            hv_Mean10 = new HTuple();
            try
            {

                hv_Seconds.Dispose();
                HOperatorSet.CountSeconds(out hv_Seconds);
                hv_raduis.Dispose();
                td(ho_Heightimage, hv_PoseInvert, out hv_raduis);
                hv_Mean10.Dispose();
                hv_Mean10 = new HTuple();
                for (hv_Index = 1; (int)hv_Index <= (int)((new HTuple(hv_raduis.TupleLength())) - 10); hv_Index = (int)hv_Index + 10)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Selected.Dispose();
                        HOperatorSet.TupleSelectRange(hv_raduis, hv_Index - 1, hv_Index + 9, out hv_Selected);
                    }
                    hv_Mean.Dispose();
                    HOperatorSet.TupleMean(hv_Selected, out hv_Mean);
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Mean10 = hv_Mean10.TupleConcat(
                                hv_Mean);
                            hv_Mean10.Dispose();
                            hv_Mean10 = ExpTmpLocalVar_Mean10;
                        }
                    }
                }
                hv_Seconds1.Dispose();
                HOperatorSet.CountSeconds(out hv_Seconds1);
                hv_l.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_l = hv_Seconds1 - hv_Seconds;
                }
                hv_Radius.Dispose();
                hv_Radius = new HTuple(hv_raduis);
                hv_g.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_g = (hv_Mean10 * 0) + 160;
                }
                hv_l.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_l = (hv_Mean10 * 0) + 120;
                }
                hv_Greater.Dispose();
                HOperatorSet.TupleGreaterElem(hv_Mean10, hv_g, out hv_Greater);
                hv_Less.Dispose();
                HOperatorSet.TupleLessElem(hv_Mean10, hv_l, out hv_Less);
                hv_Indices1.Dispose();
                HOperatorSet.TupleFind(hv_Greater, 1, out hv_Indices1);
                hv_Indices2.Dispose();
                HOperatorSet.TupleFind(hv_Less, 1, out hv_Indices2);
                hv_k.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_k = new HTuple();
                    hv_k = hv_k.TupleConcat(hv_Indices1, hv_Indices2);
                }
                hv_Reduced.Dispose();
                HOperatorSet.TupleRemove(hv_Mean10, hv_k, out hv_Reduced);
                try
                {
                    hv_Function.Dispose();
                    HOperatorSet.CreateFunct1dArray(hv_Reduced, out hv_Function);
                    {
                        HTuple ExpTmpOutVar_0;
                        HOperatorSet.SmoothFunct1dMean(hv_Function, 9, 3, out ExpTmpOutVar_0);
                        hv_Function.Dispose();
                        hv_Function = ExpTmpOutVar_0;
                    }
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException2)
                {
                    HDevExpDefaultException2.ToHTuple(out hv_Exception);
                    if (hv_Mean10 == null)
                        hv_Mean10 = new HTuple();
                    hv_Mean10[199] = 0;
                }

            }
            // catch (Exception) 
            catch (HalconException HDevExpDefaultException1)
            {
                HDevExpDefaultException1.ToHTuple(out hv_Exception);
                hv_e.Dispose();
                hv_e = 2;
                hv_Q.Dispose();
                hv_Q = 1;
            }
            hv_e.Dispose();
            hv_e = 2;
            hv_Q.Dispose();
            hv_Q = 1;

        }


        public void HX_BY_JX(HObject ho_Image, out HTuple hv_Coordinate, out HTuple hv_Length,
      out HTuple hv_H, out HTuple hv_T, out HTuple hv_Head, out HTuple hv_Tail)
        {
            
            // Local iconic variables 

            HObject ho_DerivGauss = null, ho_ImageScaleMax = null;
            HObject ho_ImageMedian = null, ho_ImageMean = null, ho_RegionDynThresh = null;
            HObject ho_Region = null, ho_RegionDilation2 = null, ho_RegionErosion2 = null;
            HObject ho_RegionErosion3 = null, ho_RegionDilation3 = null;
            HObject ho_RegionDifference1 = null, ho_RegionOpening = null;
            HObject ho_RegionClosing1 = null, ho_ConnectedRegions = null;
            HObject ho_SelectedRegions2 = null, ho_SelectedRegions1 = null;
            HObject ho_ObjectSelected1 = null, ho_Rectangle = null, ho_Region1 = null;
            HObject ho_RegionDilation = null, ho_RegionErosion1 = null;
            HObject ho_ConnectedRegions1 = null, ho_SelectedRegions3 = null;
            HObject ho_RegionUnion = null;

            // Local control variables 

            HTuple hv_Rate = new HTuple(), hv_Value1 = new HTuple();
            HTuple hv_N = new HTuple(), hv_Index1 = new HTuple(), hv_Rowss = new HTuple();
            HTuple hv_Columnss = new HTuple(), hv_Mins = new HTuple();
            HTuple hv_Maxs = new HTuple(), hv_i = new HTuple(), hv_Abs = new HTuple();
            HTuple hv_Coordinateout = new HTuple(), hv_RowsL = new HTuple();
            HTuple hv_ColumnsL = new HTuple(), hv_MaxL = new HTuple();
            HTuple hv_MinL = new HTuple(), hv_Exception = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_DerivGauss);
            HOperatorSet.GenEmptyObj(out ho_ImageScaleMax);
            HOperatorSet.GenEmptyObj(out ho_ImageMedian);
            HOperatorSet.GenEmptyObj(out ho_ImageMean);
            HOperatorSet.GenEmptyObj(out ho_RegionDynThresh);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation2);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion2);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion3);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation3);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference1);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_RegionClosing1);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions2);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelected1);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_Region1);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion1);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions3);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion);
            hv_Coordinate = new HTuple();
            hv_Length = new HTuple();
            hv_H = new HTuple();
            hv_T = new HTuple();
            hv_Head = new HTuple();
            hv_Tail = new HTuple();
            try
            {
                hv_Rate.Dispose();
                hv_Rate = 0.28316683;
                //Rate := 0.30578512 // 像素精度
                try
                {

                    //晶线识别计算*
                    //threshold (Image, Region2, 5, 255)
                    //erosion_rectangle1 (Region2, RegionErosion, 11, 11)
                    //dilation_rectangle1 (RegionErosion, RegionDilation1, 200, 1)
                    //reduce_domain (Image, RegionDilation1, ImageReduced)
                    //crop_domain (ImageReduced, ImagePart)
                    ho_DerivGauss.Dispose();
                    HOperatorSet.DerivateGauss(ho_Image, out ho_DerivGauss, 3, "xx");
                    ho_ImageScaleMax.Dispose();
                    HOperatorSet.ScaleImageMax(ho_DerivGauss, out ho_ImageScaleMax);
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianRect(ho_ImageScaleMax, out ho_ImageMedian, 1, 2);
                    ho_ImageMean.Dispose();
                    HOperatorSet.MeanImage(ho_ImageMedian, out ho_ImageMean, 20, 1);
                    ho_RegionDynThresh.Dispose();
                    HOperatorSet.DynThreshold(ho_ImageMedian, ho_ImageMean, out ho_RegionDynThresh,
                        20, "dark");
                    ho_Region.Dispose();
                    HOperatorSet.Threshold(ho_Image, out ho_Region, 0, 50);
                    ho_RegionDilation2.Dispose();
                    HOperatorSet.DilationRectangle1(ho_Region, out ho_RegionDilation2, 30, 30);
                    ho_RegionErosion2.Dispose();
                    HOperatorSet.ErosionRectangle1(ho_RegionDilation2, out ho_RegionErosion2,
                        30, 30);
                    ho_RegionErosion3.Dispose();
                    HOperatorSet.ErosionRectangle1(ho_RegionErosion2, out ho_RegionErosion3,
                        50, 50);
                    ho_RegionDilation3.Dispose();
                    HOperatorSet.DilationRectangle1(ho_RegionErosion3, out ho_RegionDilation3,
                        50, 50);


                    ho_RegionDifference1.Dispose();
                    HOperatorSet.Difference(ho_RegionDynThresh, ho_RegionDilation3, out ho_RegionDifference1
                        );

                    ho_RegionOpening.Dispose();
                    HOperatorSet.OpeningRectangle1(ho_RegionDifference1, out ho_RegionOpening,
                        1, 130);
                    ho_RegionClosing1.Dispose();
                    HOperatorSet.ClosingRectangle1(ho_RegionOpening, out ho_RegionClosing1, 3,
                        200);
                    ho_ConnectedRegions.Dispose();
                    HOperatorSet.Connection(ho_RegionClosing1, out ho_ConnectedRegions);
                    //select_shape (ConnectedRegions, SelectedRegions2, 'orientation', 'and', 1.53, 1.63)
                    ho_SelectedRegions2.Dispose();
                    HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions2, "outer_radius",
                        "and", 250, 9999999);

                    //closing_rectangle1 (SelectedRegions2, RegionClosing, 1, 1000)
                    //select_shape (ConnectedRegions, SelectedRegions2, 'column', 'and', 1600, 1690)
                    ho_SelectedRegions1.Dispose();
                    HOperatorSet.SelectGray(ho_SelectedRegions2, ho_Image, out ho_SelectedRegions1,
                        "mean", "and", 30, 255);
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.DispObj(ho_Image, HDevWindowStack.GetActive());
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.DispObj(ho_SelectedRegions1, HDevWindowStack.GetActive());
                    }
                    //根据缺陷分段头尾*
                    hv_Value1.Dispose();
                    HOperatorSet.RegionFeatures(ho_SelectedRegions1, "area", out hv_Value1);
                    hv_N.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_N = new HTuple(hv_Value1.TupleLength()
                            );
                    }
                    hv_Coordinate.Dispose();
                    hv_Coordinate = new HTuple();
                    HTuple end_val39 = hv_N;
                    HTuple step_val39 = 1;
                    for (hv_Index1 = 1; hv_Index1.Continue(end_val39, step_val39); hv_Index1 = hv_Index1.TupleAdd(step_val39))
                    {
                        ho_ObjectSelected1.Dispose();
                        HOperatorSet.SelectObj(ho_SelectedRegions1, out ho_ObjectSelected1, hv_Index1);
                        hv_Rowss.Dispose(); hv_Columnss.Dispose();
                        HOperatorSet.GetRegionPoints(ho_ObjectSelected1, out hv_Rowss, out hv_Columnss);
                        hv_Mins.Dispose();
                        HOperatorSet.TupleMin(hv_Rowss, out hv_Mins);
                        hv_Maxs.Dispose();
                        HOperatorSet.TupleMax(hv_Rowss, out hv_Maxs);
                        if (hv_Head == null)
                            hv_Head = new HTuple();
                        hv_Head[hv_Index1 - 1] = hv_Mins * hv_Rate;
                        ho_Rectangle.Dispose();
                        HOperatorSet.GenRectangle1(out ho_Rectangle, hv_Mins, 0, hv_Mins, 3200);
                        if (hv_Tail == null)
                            hv_Tail = new HTuple();
                        hv_Tail[hv_Index1 - 1] = hv_Maxs * hv_Rate;
                        ho_Rectangle.Dispose();
                        HOperatorSet.GenRectangle1(out ho_Rectangle, hv_Maxs, 0, hv_Maxs, 3200);
                        hv_i.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_i = (hv_Head.TupleSelect(
                                hv_Index1 - 1)) - (hv_Tail.TupleSelect(hv_Index1 - 1));
                        }
                        hv_Abs.Dispose();
                        HOperatorSet.TupleAbs(hv_i, out hv_Abs);
                        //PKSW_Head (Head[Index1-1], Tail[Index1-1], Abs, Coordinateout)
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_Coordinateout.Dispose();
                            CGlobalFuncTools.Instance().PKSW_Tail(hv_Head.TupleSelect(hv_Index1 - 1), hv_Tail.TupleSelect(hv_Index1 - 1),
                                hv_Abs, out hv_Coordinateout);
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_Coordinate = hv_Coordinate.TupleConcat(
                                    hv_Coordinateout);
                                hv_Coordinate.Dispose();
                                hv_Coordinate = ExpTmpLocalVar_Coordinate;
                            }
                        }
                    }

                    //棒子实际长度部分,用于分段扫描*
                    ho_Region1.Dispose();
                    HOperatorSet.Threshold(ho_Image, out ho_Region1, 250, 255);
                    ho_RegionDilation.Dispose();
                    HOperatorSet.DilationRectangle1(ho_Region1, out ho_RegionDilation, 100, 100);
                    ho_RegionErosion1.Dispose();
                    HOperatorSet.ErosionRectangle1(ho_RegionDilation, out ho_RegionErosion1,
                        100, 100);
                    ho_ConnectedRegions1.Dispose();
                    HOperatorSet.Connection(ho_RegionErosion1, out ho_ConnectedRegions1);
                    ho_SelectedRegions3.Dispose();
                    HOperatorSet.SelectShape(ho_ConnectedRegions1, out ho_SelectedRegions3, "area",
                        "and", 10000, 999999999999999999);
                    ho_RegionUnion.Dispose();
                    HOperatorSet.Union1(ho_SelectedRegions3, out ho_RegionUnion);
                    hv_RowsL.Dispose(); hv_ColumnsL.Dispose();
                    HOperatorSet.GetRegionPoints(ho_RegionUnion, out hv_RowsL, out hv_ColumnsL);
                    hv_MaxL.Dispose();
                    HOperatorSet.TupleMax(hv_RowsL, out hv_MaxL);
                    hv_MinL.Dispose();
                    HOperatorSet.TupleMin(hv_RowsL, out hv_MinL);
                    hv_Length.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Length = (hv_MaxL - hv_MinL) * hv_Rate;
                    }
                    hv_H.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_H = hv_MinL * hv_Rate;
                    }
                    hv_T.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_T = hv_MaxL * hv_Rate;
                    }
                    //Coordinate := [MinL*Rate,Length,Head,Tail]
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);

                }
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_DerivGauss.Dispose();
                ho_ImageScaleMax.Dispose();
                ho_ImageMedian.Dispose();
                ho_ImageMean.Dispose();
                ho_RegionDynThresh.Dispose();
                ho_Region.Dispose();
                ho_RegionDilation2.Dispose();
                ho_RegionErosion2.Dispose();
                ho_RegionErosion3.Dispose();
                ho_RegionDilation3.Dispose();
                ho_RegionDifference1.Dispose();
                ho_RegionOpening.Dispose();
                ho_RegionClosing1.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_SelectedRegions2.Dispose();
                ho_SelectedRegions1.Dispose();
                ho_ObjectSelected1.Dispose();
                ho_Rectangle.Dispose();
                ho_Region1.Dispose();
                ho_RegionDilation.Dispose();
                ho_RegionErosion1.Dispose();
                ho_ConnectedRegions1.Dispose();
                ho_SelectedRegions3.Dispose();
                ho_RegionUnion.Dispose();

                hv_Rate.Dispose();
                hv_Value1.Dispose();
                hv_N.Dispose();
                hv_Index1.Dispose();
                hv_Rowss.Dispose();
                hv_Columnss.Dispose();
                hv_Mins.Dispose();
                hv_Maxs.Dispose();
                hv_i.Dispose();
                hv_Abs.Dispose();
                hv_Coordinateout.Dispose();
                hv_RowsL.Dispose();
                hv_ColumnsL.Dispose();
                hv_MaxL.Dispose();
                hv_MinL.Dispose();
                hv_Exception.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void HT(HObject ho_Image1, HObject ho_Image2, HObject ho_Image3, HObject ho_Image4,
      HObject ho_Image5, HObject ho_Image6, HObject ho_Image7, HObject ho_Image8,
      HObject ho_Image9, HObject ho_Image10, out HObject ho_Imageconst)
        {



            // Local iconic variables 

            HObject ho_Rectangle;

            // Local control variables 

            HTuple hv_n = new HTuple(), hv_Width = new HTuple();
            HTuple hv_Height = new HTuple(), hv_Rows = new HTuple();
            HTuple hv_Columns = new HTuple(), hv_Grayval = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Imageconst);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            try
            {
                ho_Imageconst.Dispose();
                HOperatorSet.GenImageConst(out ho_Imageconst, "byte", 6400, 22000);
                hv_n.Dispose();
                hv_n = 0;

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image1, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image1, hv_Rows, hv_Columns, out hv_Grayval);
                HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows, hv_Columns, hv_Grayval);

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image2, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image2, hv_Rows, hv_Columns, out hv_Grayval);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows + hv_n, hv_Columns, hv_Grayval);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image3, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image3, hv_Rows, hv_Columns, out hv_Grayval);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows + hv_n, hv_Columns, hv_Grayval);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image4, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image4, hv_Rows, hv_Columns, out hv_Grayval);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows + hv_n, hv_Columns, hv_Grayval);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }


                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image5, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image5, hv_Rows, hv_Columns, out hv_Grayval);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows + hv_n, hv_Columns, hv_Grayval);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image6, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image6, hv_Rows, hv_Columns, out hv_Grayval);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows + hv_n, hv_Columns, hv_Grayval);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image7, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image7, hv_Rows, hv_Columns, out hv_Grayval);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows + hv_n, hv_Columns, hv_Grayval);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image8, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image8, hv_Rows, hv_Columns, out hv_Grayval);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows + hv_n, hv_Columns, hv_Grayval);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image9, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image9, hv_Rows, hv_Columns, out hv_Grayval);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows + hv_n, hv_Columns, hv_Grayval);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image10, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image10, hv_Rows, hv_Columns, out hv_Grayval);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows + hv_n, hv_Columns, hv_Grayval);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }

                //get_image_size (Image11, Width, Height)
                //gen_rectangle1 (Rectangle, 0, 0, Height-1, Width-1)
                //get_region_points (Rectangle, Rows, Columns)
                //get_grayval (Image11, Rows, Columns, Grayval)
                //set_grayval (Imageconst, Rows+n, Columns, Grayval)
                //n := n+Height
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Rectangle.Dispose();

                hv_n.Dispose();
                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_Rows.Dispose();
                hv_Columns.Dispose();
                hv_Grayval.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void HX_DEMONew(HObject ho_Grayimage1, HObject ho_Grayimage2, HObject ho_Grayimage3,
       HObject ho_Grayimage4, HObject ho_Grayimage5, HObject ho_Grayimage6, HObject ho_Grayimage7,
       HObject ho_Grayimage8, HObject ho_Grayimage9, HObject ho_Grayimage10, HObject ho_Heightimage1,
       HObject ho_Heightimage2, HObject ho_Heightimage3, HObject ho_Heightimage4, HObject ho_Heightimage5,
       HObject ho_Heightimage6, HObject ho_Heightimage7, HObject ho_Heightimage8, HObject ho_Heightimage9,
       HObject ho_Heightimage10, out HObject ho_Imageconst, HTuple hv_ResultDictHandle,
       HTuple hv_Rate)
        {




            // Local iconic variables 

            // Local control variables 

            HTuple hv_Name1 = new HTuple(), hv_Substring = new HTuple();
            HTuple hv_Name = new HTuple(), hv_Exception = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Imageconst);
            try
            {
                ho_Imageconst.Dispose();
                HT(ho_Grayimage1, ho_Grayimage2, ho_Grayimage3, ho_Grayimage4, ho_Grayimage5,
                    ho_Grayimage6, ho_Grayimage7, ho_Grayimage8, ho_Grayimage9, ho_Grayimage10,
                    out ho_Imageconst);
                hv_Name1.Dispose();
                HOperatorSet.GetDictTuple(hv_ResultDictHandle, "晶编", out hv_Name1);
                hv_Substring.Dispose();
                HOperatorSet.TupleSplit(hv_Name1, "-", out hv_Substring);
                hv_Name.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Name = hv_Substring.TupleSelect(
                        0);
                }
                try
                {
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage1, "高度图", "H1", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage2, "高度图", "H2", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage3, "高度图", "H3", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage4, "高度图", "H4", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage5, "高度图", "H5", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage6, "高度图", "H6", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage7, "高度图", "H7", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage8, "高度图", "H8", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage9, "高度图", "H9", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage10, "高度图", "H10", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Imageconst, "灰度图", "合成", hv_Name, "3D", "tiff");
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                }

                hv_Name1.Dispose();
                hv_Substring.Dispose();
                hv_Name.Dispose();
                hv_Exception.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_Name1.Dispose();
                hv_Substring.Dispose();
                hv_Name.Dispose();
                hv_Exception.Dispose();

                throw HDevExpDefaultException;
            }
        }
        public void HX_DEMO(HObject ho_Grayimage1, HObject ho_Grayimage2, HObject ho_Grayimage3,
      HObject ho_Grayimage4, HObject ho_Grayimage5, HObject ho_Grayimage6, HObject ho_Grayimage7,
      HObject ho_Grayimage8, HObject ho_Grayimage9, HObject ho_Grayimage10, HObject ho_Heightimage1,
      HObject ho_Heightimage2, HObject ho_Heightimage3, HObject ho_Heightimage4, HObject ho_Heightimage5,
      HObject ho_Heightimage6, HObject ho_Heightimage7, HObject ho_Heightimage8, HObject ho_Heightimage9,
      HObject ho_Heightimage10, out HObject ho_Imageconst, HTuple hv_ResultDictHandle,
      HTuple hv_Rate, out HTuple hv_Name, out HTuple hv_Coordinate, out HTuple hv_Length,
      out HTuple hv_H, out HTuple hv_T, out HTuple hv_Head, out HTuple hv_Tail, out HTuple hv_CoordinateHT,
      out HTuple hv_Number1, out HTuple hv_L)
        {




            // Local iconic variables 

            HObject ho_Rectangle = null, ho_Rectangle1 = null;
            HObject ho_Rectangle2 = null;

            // Local control variables 

            HTuple hv_Exception = new HTuple(), hv_Index2 = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Imageconst);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            HOperatorSet.GenEmptyObj(out ho_Rectangle2);
            hv_Name = new HTuple();
            hv_Coordinate = new HTuple();
            hv_Length = new HTuple();
            hv_H = new HTuple();
            hv_T = new HTuple();
            hv_Head = new HTuple();
            hv_Tail = new HTuple();
            hv_CoordinateHT = new HTuple();
            hv_Number1 = new HTuple();
            hv_L = new HTuple();
            try
            {
                ho_Imageconst.Dispose();
                HT(ho_Grayimage1, ho_Grayimage2, ho_Grayimage3, ho_Grayimage4, ho_Grayimage5,
                    ho_Grayimage6, ho_Grayimage7, ho_Grayimage8, 
                    out ho_Imageconst);
                hv_Name.Dispose();
                HOperatorSet.GetDictTuple(hv_ResultDictHandle, "晶编", out hv_Name);

                LogHelper.Info("Silicon ", "晶编 " + hv_Name.ToString());
                try
                {
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage1, "高度图", "H1", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage2,  "高度图", "H2",hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage3, "高度图", "H3", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage4, "高度图", "H4", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage5, "高度图", "H5", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage6, "高度图", "H6", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage7, "高度图", "H7", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage8, "高度图", "H8", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage9, "高度图", "H9", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Heightimage10, "高度图", "H10", hv_Name, "3D", "tiff");
                    CGlobalFuncTools.Instance().Save_Image(ho_Imageconst, "灰度图", "合成", hv_Name, "3D", "tiff");
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    LogHelper.Error("HX_DEMO exception " + hv_Exception.ToString());
                }

                try
                {
                    hv_Coordinate.Dispose(); hv_Length.Dispose(); hv_H.Dispose(); hv_T.Dispose(); hv_Head.Dispose(); hv_Tail.Dispose();
                    //ho_Imageconst.Dispose();

                    ////HOperatorSet.ReadImage(out ho_Imageconst, "E:/binnew/11/11/WN1052/3D/灰度图_合成.tif");
                    HX_BY_JX(ho_Imageconst, out hv_Coordinate, out hv_Length, out hv_H, out hv_T,
                        out hv_Head, out hv_Tail);
                    hv_CoordinateHT.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_CoordinateHT = new HTuple();
                        hv_CoordinateHT = hv_CoordinateHT.TupleConcat(hv_Head, hv_Tail);
                    }
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                }
                hv_Number1.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Number1 = new HTuple(hv_Coordinate.TupleLength());
                }
                hv_L.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_L = hv_Length.TupleInt();
                }
                HTuple end_val24 = hv_Number1 - 1;
                HTuple step_val24 = 1;
                for (hv_Index2 = 0; hv_Index2.Continue(end_val24, step_val24); hv_Index2 = hv_Index2.TupleAdd(step_val24))
                {
                    
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, (hv_Coordinate.TupleSelect(hv_Index2)) / hv_Rate,
                        0, (hv_Coordinate.TupleSelect(hv_Index2)) / hv_Rate, 3200);
                    
                    if ((int)(new HTuple(hv_Index2.TupleEqual(0))) != 0)
                    {
                        
                        ho_Rectangle1.Dispose();
                        HOperatorSet.GenRectangle1(out ho_Rectangle1, ((hv_Coordinate.TupleSelect(
                            hv_Index2)) / hv_Rate) + 18, 0, ((hv_Coordinate.TupleSelect(hv_Index2)) / hv_Rate) + 18,
                            3200);
                        
                    }
                    else if ((int)(new HTuple(hv_Index2.TupleEqual(hv_Number1 - 1))) != 0)
                    {
                        
                        ho_Rectangle2.Dispose();
                        HOperatorSet.GenRectangle1(out ho_Rectangle2, ((hv_Coordinate.TupleSelect(
                            hv_Index2)) / hv_Rate) - 18, 0, ((hv_Coordinate.TupleSelect(hv_Index2)) / hv_Rate) - 18,
                            3200);
                        
                    }
                    //if (HDevWindowStack.IsOpen())
                    //{
                    //    HOperatorSet.DispObj(ho_Rectangle, HDevWindowStack.GetActive());
                    //}
                }
                ho_Rectangle.Dispose();
                ho_Rectangle1.Dispose();
                ho_Rectangle2.Dispose();

                hv_Exception.Dispose();
                hv_Index2.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Rectangle.Dispose();
                ho_Rectangle1.Dispose();
                ho_Rectangle2.Dispose();

                hv_Exception.Dispose();
                hv_Index2.Dispose();
                HDevExpDefaultException.ToHTuple(out hv_Exception);

                LogHelper.Error("HX_DEMO " + hv_Exception.ToString());
            }

            return;
        }
        public void HT(HObject ho_Image1, HObject ho_Image2, HObject ho_Image3, HObject ho_Image4,
      HObject ho_Image5, HObject ho_Image6, HObject ho_Image7, HObject ho_Image8,
      out HObject ho_Imageconst)
        {


            try
            {
                // Local iconic variables 

                HObject ho_Rectangle;

                // Local control variables 

                HTuple hv_n = new HTuple(), hv_Width = new HTuple();
                HTuple hv_Height = new HTuple(), hv_Rows = new HTuple();
                HTuple hv_Columns = new HTuple(), hv_Grayval = new HTuple();
                // Initialize local and output iconic variables 
                HOperatorSet.GenEmptyObj(out ho_Imageconst);
                HOperatorSet.GenEmptyObj(out ho_Rectangle);
                ho_Imageconst.Dispose();
                HOperatorSet.GenImageConst(out ho_Imageconst, "byte", 6400, 22000);
                hv_n.Dispose();
                hv_n = 0;

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image1, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image1, hv_Rows, hv_Columns, out hv_Grayval);
                HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows, hv_Columns, hv_Grayval);

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image2, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image2, hv_Rows, hv_Columns, out hv_Grayval);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows + hv_n, hv_Columns, hv_Grayval);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image3, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image3, hv_Rows, hv_Columns, out hv_Grayval);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows + hv_n, hv_Columns, hv_Grayval);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image4, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image4, hv_Rows, hv_Columns, out hv_Grayval);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows + hv_n, hv_Columns, hv_Grayval);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }


                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image5, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image5, hv_Rows, hv_Columns, out hv_Grayval);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows + hv_n, hv_Columns, hv_Grayval);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image6, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image6, hv_Rows, hv_Columns, out hv_Grayval);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows + hv_n, hv_Columns, hv_Grayval);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image7, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image7, hv_Rows, hv_Columns, out hv_Grayval);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows + hv_n, hv_Columns, hv_Grayval);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_n = hv_n + hv_Height;
                        hv_n.Dispose();
                        hv_n = ExpTmpLocalVar_n;
                    }
                }

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image8, out hv_Width, out hv_Height);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, 0, hv_Height - 1, hv_Width - 1);
                }
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Rectangle, out hv_Rows, out hv_Columns);
                hv_Grayval.Dispose();
                HOperatorSet.GetGrayval(ho_Image8, hv_Rows, hv_Columns, out hv_Grayval);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetGrayval(ho_Imageconst, hv_Rows + hv_n, hv_Columns, hv_Grayval);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HTuple  ExpTmpLocalVar_n = hv_n + hv_Height;
                    hv_n.Dispose();
                    hv_n = ExpTmpLocalVar_n;
                }

                
            }
            catch(HalconException HDevExpDefaultException)
            {
                HTuple hv_Exception = new HTuple();
                HOperatorSet.GenEmptyObj(out ho_Imageconst);
                HDevExpDefaultException.ToHTuple(out hv_Exception);
            }

            

           
        }

        
        public void Remove_max_min(HTuple hv_raduis, HTuple hv_n, out HTuple hv_Reduced)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_Index = new HTuple(), hv_Max = new HTuple();
            HTuple hv_Min = new HTuple(), hv_Indices1 = new HTuple();
            HTuple hv_Indices2 = new HTuple(), hv_h = new HTuple();
            HTuple hv_raduis_COPY_INP_TMP = new HTuple(hv_raduis);

            // Initialize local and output iconic variables 
            hv_Reduced = new HTuple();
            try
            {
                HTuple end_val0 = hv_n;
                HTuple step_val0 = 1;
                for (hv_Index = 1; hv_Index.Continue(end_val0, step_val0); hv_Index = hv_Index.TupleAdd(step_val0))
                {

                    hv_Max.Dispose();
                    HOperatorSet.TupleMax(hv_raduis_COPY_INP_TMP, out hv_Max);
                    hv_Min.Dispose();
                    HOperatorSet.TupleMin(hv_raduis_COPY_INP_TMP, out hv_Min);
                    hv_Indices1.Dispose();
                    HOperatorSet.TupleFind(hv_raduis_COPY_INP_TMP, hv_Min, out hv_Indices1);
                    hv_Indices2.Dispose();
                    HOperatorSet.TupleFind(hv_raduis_COPY_INP_TMP, hv_Max, out hv_Indices2);
                    hv_h.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_h = new HTuple();
                        hv_h = hv_h.TupleConcat(hv_Indices1, hv_Indices2);
                    }
                    {
                        HTuple ExpTmpOutVar_0;
                        HOperatorSet.TupleRemove(hv_raduis_COPY_INP_TMP, hv_h, out ExpTmpOutVar_0);
                        hv_raduis_COPY_INP_TMP.Dispose();
                        hv_raduis_COPY_INP_TMP = ExpTmpOutVar_0;
                    }

                }
                hv_Reduced.Dispose();
                hv_Reduced = new HTuple(hv_raduis_COPY_INP_TMP);

                hv_raduis_COPY_INP_TMP.Dispose();
                hv_Index.Dispose();
                hv_Max.Dispose();
                hv_Min.Dispose();
                hv_Indices1.Dispose();
                hv_Indices2.Dispose();
                hv_h.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_raduis_COPY_INP_TMP.Dispose();
                hv_Index.Dispose();
                hv_Max.Dispose();
                hv_Min.Dispose();
                hv_Indices1.Dispose();
                hv_Indices2.Dispose();
                hv_h.Dispose();

                throw HDevExpDefaultException;
            }
        }
        public void tuple_fe(HTuple hv_MeanRadius100, out HTuple hv_first, out HTuple hv_end)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_j = new HTuple(), hv_k = new HTuple();
            HTuple hv_Index = new HTuple(), hv_Index1 = new HTuple();
            // Initialize local and output iconic variables 
            hv_first = new HTuple();
            hv_end = new HTuple();
            try
            {
                hv_j.Dispose();
                hv_j = 0;
                hv_k.Dispose();
                hv_k = 0;
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_MeanRadius100.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {

                    if ((int)(new HTuple(((hv_MeanRadius100.TupleSelect(hv_Index))).TupleGreater(
                        125))) != 0)
                    {
                        if ((int)(new HTuple(((hv_MeanRadius100.TupleSelect(hv_Index))).TupleLess(
                            127))) != 0)
                        {
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_j = hv_j + 1;
                                    hv_j.Dispose();
                                    hv_j = ExpTmpLocalVar_j;
                                }
                            }
                            if ((int)(new HTuple(hv_j.TupleGreater(3))) != 0)
                            {
                                break;
                            }

                        }

                    }
                }
                hv_first.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_first = hv_Index - 2;
                }

                for (hv_Index1 = (new HTuple(hv_MeanRadius100.TupleLength())) - 1; (int)hv_Index1 >= 0; hv_Index1 = (int)hv_Index1 + -1)
                {

                    if ((int)((new HTuple(((hv_MeanRadius100.TupleSelect(hv_Index1))).TupleGreater(
                        125))).TupleAnd(new HTuple(((hv_MeanRadius100.TupleSelect(hv_Index1))).TupleLess(
                        127)))) != 0)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_k = hv_k + 1;
                                hv_k.Dispose();
                                hv_k = ExpTmpLocalVar_k;
                            }
                        }
                        if ((int)(new HTuple(hv_k.TupleGreater(3))) != 0)
                        {
                            break;
                        }
                    }
                }
                hv_end.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_end = hv_Index1 + 2;
                }
                if ((int)(new HTuple(((hv_end - hv_first)).TupleLess(0))) != 0)
                {
                    hv_first.Dispose();
                    hv_first = 0;
                    hv_end.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_end = (new HTuple(hv_MeanRadius100.TupleLength()
                            )) - 1;
                    }
                }



                hv_j.Dispose();
                hv_k.Dispose();
                hv_Index.Dispose();
                hv_Index1.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_j.Dispose();
                hv_k.Dispose();
                hv_Index.Dispose();
                hv_Index1.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void Measure_3DNew(out HObject ho_Imageconst, HTuple hv_LQueueHandle, HTuple hv_ResultDictHandle)
        {

            //using (HDevThreadContext context = new HDevThreadContext())
            {
                // +++ Threading variables 
                Thread devThread = null;
                Thread devThread1 = null;
                Thread devThread2 = null;
                Thread devThread3 = null;
                Thread devThread4 = null;
                Thread devThread5 = null;
                Thread devThread6 = null;
                Thread devThread7 = null;
                Thread devThread8 = null;
                Thread devThread9 = null;



                // Local iconic variables 

                HObject ho_EmptyObject = null, ho_Grayimage1 = null;
                HObject ho_Heightimage1 = null, ho_Grayimage2 = null, ho_Heightimage2 = null;
                HObject ho_Grayimage3 = null, ho_Heightimage3 = null, ho_Grayimage4 = null;
                HObject ho_Heightimage4 = null, ho_Grayimage5 = null, ho_Heightimage5 = null;
                HObject ho_Grayimage6 = null, ho_Heightimage6 = null, ho_Grayimage7 = null;
                HObject ho_Heightimage7 = null, ho_Grayimage8 = null, ho_Heightimage8 = null;
                HObject ho_Grayimage9 = null, ho_Heightimage9 = null, ho_Grayimage10 = null;
                HObject ho_Heightimage10 = null;
                HObject ho_Rectangle = null, ho_Rectangle1 = null;
                HObject ho_Rectangle2 = null;

                // Local control variables 

                HTuple hv_Rate = new HTuple(), hv_Seconds1 = new HTuple();
                HTuple hv_MessageHandle = new HTuple(), hv_Start = new HTuple();
                HTuple hv_Seconds2 = new HTuple(), hv_Name = new HTuple();
                HTuple hv_Q1 = new HTuple(), hv_Q2 = new HTuple();
                HTuple hv_Q3 = new HTuple(), hv_Q4 = new HTuple(), hv_Q5 = new HTuple();
                HTuple hv_Q6 = new HTuple(), hv_Q7 = new HTuple(), hv_Q8 = new HTuple();
                HTuple hv_Q9 = new HTuple(), hv_SRadius1 = new HTuple();
                HTuple hv_ThreadID1 = new HTuple();
                HTuple hv_SRadius2 = new HTuple(), hv_mean1 = new HTuple();
                HTuple hv_ThreadID = new HTuple(), hv_SRadius3 = new HTuple();
                HTuple hv_mean2 = new HTuple(), hv_ThreadID9 = new HTuple();
                HTuple hv_SRadius4 = new HTuple(), hv_mean3 = new HTuple();
                HTuple hv_ThreadID10 = new HTuple(), hv_SRadius5 = new HTuple();
                HTuple hv_mean4 = new HTuple(), hv_ThreadID4 = new HTuple();
                HTuple hv_SRadius6 = new HTuple(), hv_mean5 = new HTuple();
                HTuple hv_ThreadID5 = new HTuple(), hv_SRadius7 = new HTuple();
                HTuple hv_mean6 = new HTuple(), hv_ThreadID6 = new HTuple();
                HTuple hv_SRadius8 = new HTuple(), hv_mean7 = new HTuple();
                HTuple hv_ThreadID7 = new HTuple(), hv_SRadius9 = new HTuple();
                HTuple hv_mean8 = new HTuple(), hv_ThreadID8 = new HTuple();
                HTuple hv_Q10 = new HTuple(), hv_mean10 = new HTuple();
                HTuple hv_SRadius10 = new HTuple(), hv_mean9 = new HTuple();
                HTuple hv_Function = new HTuple(), hv_Derivative = new HTuple();
                HTuple hv_FunctionAbsolute = new HTuple(), hv_k1 = new HTuple();
                HTuple hv_Reduced4 = new HTuple(), hv_Exception = new HTuple();
                HTuple hv_Max = new HTuple(), hv_Min = new HTuple(), hv_Mean = new HTuple();
                HTuple hv_DiameterMax = new HTuple(), hv_DiameterMin = new HTuple();
                HTuple hv_DiameterMean = new HTuple(), hv_f = new HTuple();
                HTuple hv_Index = new HTuple(), hv_Selected1 = new HTuple();

                HTuple hv_QQ = new HTuple();
                HTuple hv_Coordinate = new HTuple(), hv_Length = new HTuple();
                HTuple hv_H = new HTuple(), hv_T = new HTuple(), hv_Head = new HTuple();
                HTuple hv_Tail = new HTuple(), hv_CoordinateHT = new HTuple();
                HTuple hv_Number1 = new HTuple(), hv_L = new HTuple();
                HTuple hv_Index2 = new HTuple(), hv_MeanRadius = new HTuple();
                HTuple hv_Greater = new HTuple();
                HTuple hv_Indices2 = new HTuple(), hv_Reduced = new HTuple();
                HTuple hv_Indices3 = new HTuple(), hv_Indices4 = new HTuple();
                HTuple hv_Indices1 = new HTuple(), hv_Reduced1 = new HTuple();
                HTuple hv_Mean1 = new HTuple();
                HTuple hv_IDTime = new HTuple();
                HTuple hv_k = new HTuple();
                HTuple hv_MeanRadius1 = new HTuple();
                HTuple hv_MeanRadius100 = new HTuple(), hv_first = new HTuple();
                HTuple hv_end = new HTuple(), hv_Selected = new HTuple();
                HTuple hv_Raduiszz = new HTuple(), hv_Raduiszz1 = new HTuple();
                HTuple hv_g = new HTuple(), hv_l = new HTuple();
                HTuple hv_Less = new HTuple();
                HTuple hv_PoseInvert = new HTuple();
                hv_Rate = 0.28316683;
                hv_PoseInvert.Dispose();
                HOperatorSet.ReadTuple("./PoseInvert.tup", out hv_PoseInvert);

                // Initialize local and output iconic variables 
                HOperatorSet.GenEmptyObj(out ho_Imageconst);
                HOperatorSet.GenEmptyObj(out ho_EmptyObject);
                HOperatorSet.GenEmptyObj(out ho_Grayimage1);
                HOperatorSet.GenEmptyObj(out ho_Heightimage1);
                HOperatorSet.GenEmptyObj(out ho_Grayimage2);
                HOperatorSet.GenEmptyObj(out ho_Heightimage2);
                HOperatorSet.GenEmptyObj(out ho_Grayimage3);
                HOperatorSet.GenEmptyObj(out ho_Heightimage3);
                HOperatorSet.GenEmptyObj(out ho_Grayimage4);
                HOperatorSet.GenEmptyObj(out ho_Heightimage4);
                HOperatorSet.GenEmptyObj(out ho_Grayimage5);
                HOperatorSet.GenEmptyObj(out ho_Heightimage5);
                HOperatorSet.GenEmptyObj(out ho_Grayimage6);
                HOperatorSet.GenEmptyObj(out ho_Heightimage6);
                HOperatorSet.GenEmptyObj(out ho_Grayimage7);
                HOperatorSet.GenEmptyObj(out ho_Heightimage7);
                HOperatorSet.GenEmptyObj(out ho_Grayimage8);
                HOperatorSet.GenEmptyObj(out ho_Heightimage8);
                HOperatorSet.GenEmptyObj(out ho_Grayimage9);
                HOperatorSet.GenEmptyObj(out ho_Heightimage9);
                HOperatorSet.GenEmptyObj(out ho_Grayimage10);
                HOperatorSet.GenEmptyObj(out ho_Heightimage10);
                HOperatorSet.GenEmptyObj(out ho_Rectangle);
                HOperatorSet.GenEmptyObj(out ho_Rectangle1);
                HOperatorSet.GenEmptyObj(out ho_Rectangle2);
                try
                {
                    //global tuple QuitTaskFlag

                    try
                    {
                        if (SettingParameter.Instance().NDaemon == 1)
                        {
                            int nIndexOfImage = 0;
                            HDlHalconSSZN.SR7IFEthernetOpen(0, 192, 168, 0, 23);
                            hv_Rate.Dispose();
                            hv_Rate = 0.28316683;
                            hv_Seconds1.Dispose();
                            HOperatorSet.CountSeconds(out hv_Seconds1);

                            //**第一段***
                            HTuple houtwidth = new HTuple();
                            HTuple houtheight = new HTuple();

                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            ho_Grayimage1.Dispose(); ho_Heightimage1.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage1, out ho_Heightimage1,
                                0);
                            HOperatorSet.GetImageSize(ho_Grayimage1, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "First Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            //ho_Grayimage1.Dispose(); ho_Heightimage1.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage1, "E:/binnew/11/11/WN1052/3D/灰度图_1.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage1, "E:/binnew/11/11/WN1052/3D/1_高度图.tif");
                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                //HOperatorSet.WriteImage(ho_Grayimage1, "bmp", 0, "D:/" + nIndexOfImage.ToString() + ".bmp");
                                //HOperatorSet.WriteImage(ho_Heightimage1, "tiff", 0, "D:/" + nIndexOfImage.ToString() + ".tif");

                                if (hv_SRadius1 == null)
                                    hv_SRadius1 = new HTuple();
                                if (hv_Q1 == null)
                                    hv_Q1 = new HTuple();
                                if (hv_mean1 == null)
                                    hv_mean1 = new HTuple();
                                hv_SRadius1.Dispose(); hv_Q1.Dispose(); hv_mean1.Dispose();
                                devThread = new Thread(() =>
                                {
                                    try
                                    {
                                        // Input parameters
                                        L_C_Compute(ho_Grayimage1, ho_Heightimage1, 1, hv_PoseInvert, out hv_SRadius1, out hv_Q1, out hv_mean1);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d " + exc.Message);
                                    }
                                });
                                devThread.Start();
                            }






                            if (hv_SRadius2 == null)
                                hv_SRadius2 = new HTuple();
                            if (hv_Q2 == null)
                                hv_Q2 = new HTuple();
                            if (hv_mean2 == null)
                                hv_mean2 = new HTuple();

                            // Thread.Sleep(3000);
                            //**第二段***
                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);

                            ho_Grayimage2.Dispose(); ho_Heightimage2.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage2, out ho_Heightimage2,
                                0);
                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage2, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Second Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            //ho_Grayimage2.Dispose(); ho_Heightimage2.Dispose();
                            ////HOperatorSet.ReadImage(out ho_Grayimage2, "E:/binnew/11/11/WN1052/3D/灰度图_2.bmp");
                            ////HOperatorSet.ReadImage(out ho_Heightimage2, "E:/binnew/11/11/WN1052/3D/高度图_H3.tif");
                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                //HOperatorSet.WriteImage(ho_Grayimage2, "bmp", 0, "D:/" + nIndexOfImage.ToString() + ".bmp");
                                //HOperatorSet.WriteImage(ho_Heightimage2, "tiff", 0, "D:/" + nIndexOfImage.ToString() + ".tif");

                                hv_SRadius2.Dispose(); hv_Q2.Dispose(); hv_mean2.Dispose();
                                devThread1 = new Thread(() =>
                                {
                                    try
                                    {
                                        // Input parameters


                                        // Call L_C_Compute
                                        L_C_Compute(ho_Grayimage2, ho_Heightimage2, 2, hv_PoseInvert, out hv_SRadius2,
                                                    out hv_Q2, out hv_mean2);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 2 " + exc.Message);
                                    }
                                });
                                devThread1.Start();
                            }


                            //**第三段***
                            //Thread.Sleep(3000);

                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            //Thread.Sleep(3000);
                            ho_Grayimage3.Dispose(); ho_Heightimage3.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage3, out ho_Heightimage3,
                                0);
                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage3, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Third Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            //ho_Grayimage3.Dispose(); ho_Heightimage3.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage3, "E:/binnew/11/11/WN1052/3D/灰度图_3.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage3, "E:/binnew/11/11/WN1052/3D/高度图_H4.tif");
                            if (hv_SRadius3 == null)
                                hv_SRadius3 = new HTuple();
                            if (hv_Q3 == null)
                                hv_Q3 = new HTuple();
                            if (hv_mean3 == null)
                                hv_mean3 = new HTuple();

                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                //HOperatorSet.WriteImage(ho_Grayimage3, "bmp", 0, "D:/" + nIndexOfImage.ToString() + ".bmp");
                                //HOperatorSet.WriteImage(ho_Heightimage3, "tiff", 0, "D:/" + nIndexOfImage.ToString() + ".tif");

                                hv_SRadius3.Dispose(); hv_Q3.Dispose(); hv_mean3.Dispose();
                                devThread2 = new Thread(() =>
                                {
                                    try
                                    {

                                        // Call L_C_Compute
                                        L_C_Compute(ho_Grayimage3, ho_Heightimage3, 3, hv_PoseInvert, out hv_SRadius3,
                                                    out hv_Q3, out hv_mean3);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 2 " + exc.Message);
                                    }
                                });
                                devThread2.Start();
                            }
                            //**第四段***
                            Thread.Sleep(3000);
                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            //Thread.Sleep(3000);
                            ho_Grayimage4.Dispose(); ho_Heightimage4.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage4, out ho_Heightimage4, 0);
                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage4, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Fourth Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            //ho_Grayimage4.Dispose(); ho_Heightimage4.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage4, "E:/binnew/11/11/WN1052/3D/灰度图_4.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage4, "E:/binnew/11/11/WN1052/3D/高度图_H5.tif");

                            if (hv_SRadius4 == null)
                                hv_SRadius4 = new HTuple();
                            if (hv_Q4 == null)
                                hv_Q4 = new HTuple();
                            if (hv_mean4 == null)
                                hv_mean4 = new HTuple();
                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                //HOperatorSet.WriteImage(ho_Grayimage4, "bmp", 0, "D:/" + nIndexOfImage.ToString() + ".bmp");
                                //HOperatorSet.WriteImage(ho_Heightimage4, "tiff", 0, "D:/" + nIndexOfImage.ToString() + ".tif");

                                hv_SRadius4.Dispose(); hv_Q4.Dispose(); hv_mean4.Dispose();
                                devThread3 = new Thread(() =>
                                {
                                    try
                                    {

                                        // Call L_C_Compute
                                        L_C_Compute(ho_Grayimage4, ho_Heightimage4, 4, hv_PoseInvert, out hv_SRadius4,
                                        out hv_Q4, out hv_mean4);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 4 " + exc.Message);
                                    }
                                });
                                devThread3.Start();
                            }


                            //**第五段***
                            //Thread.Sleep(3000);
                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            //Thread.Sleep(3000);
                            ho_Grayimage5.Dispose(); ho_Heightimage5.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage5, out ho_Heightimage5,
                                0);

                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage5, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Fifth Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());

                            //ho_Grayimage5.Dispose(); ho_Heightimage5.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage5, "E:/binnew/11/11/WN1052/3D/灰度图_5.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage5, "E:/binnew/11/11/WN1052/3D/高度图_H6.tif");

                            if (hv_SRadius5 == null)
                                hv_SRadius5 = new HTuple();
                            if (hv_Q5 == null)
                                hv_Q5 = new HTuple();
                            if (hv_mean5 == null)
                                hv_mean5 = new HTuple();
                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                //HOperatorSet.WriteImage(ho_Grayimage5, "bmp", 0, "D:/" + nIndexOfImage.ToString() + ".bmp");
                                //HOperatorSet.WriteImage(ho_Heightimage5, "tiff", 0, "D:/" + nIndexOfImage.ToString() + ".tif");

                                hv_SRadius5.Dispose(); hv_Q5.Dispose(); hv_mean5.Dispose();
                                devThread4 = new Thread(() =>
                                {
                                    try
                                    {
                                        // Call L_C_Compute
                                        L_C_Compute(ho_Grayimage5, ho_Heightimage5, 5, hv_PoseInvert, out hv_SRadius5,
                                        out hv_Q5, out hv_mean5);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 5 " + exc.Message);
                                    }
                                });
                                devThread4.Start();
                            }



                            //**第六段***
                            //Thread.Sleep(3000);
                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            //Thread.Sleep(3000);
                            ho_Grayimage6.Dispose(); ho_Heightimage6.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage6, out ho_Heightimage6,
                                0);
                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage6, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Sixth Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            //ho_Grayimage6.Dispose(); ho_Heightimage6.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage6, "E:/binnew/11/11/WN1052/3D/灰度图_6.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage6, "E:/binnew/11/11/WN1052/3D/高度图_H7.tif");
                            if (hv_SRadius6 == null)
                                hv_SRadius6 = new HTuple();
                            if (hv_Q6 == null)
                                hv_Q6 = new HTuple();
                            if (hv_mean6 == null)
                                hv_mean6 = new HTuple();
                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                hv_SRadius6.Dispose(); hv_Q6.Dispose(); hv_mean6.Dispose();
                                devThread5 = new Thread(() =>
                                {
                                    try
                                    {

                                        // Call L_C_Compute
                                        L_C_Compute(ho_Grayimage6, ho_Heightimage6, 6, hv_PoseInvert, out hv_SRadius6,
                                        out hv_Q6, out hv_mean6);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 6 " + exc.Message);
                                    }
                                });
                                devThread5.Start();
                            }


                            //**第七段***
                            //Thread.Sleep(3000);

                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            //Thread.Sleep(3000);
                            ho_Grayimage7.Dispose(); ho_Heightimage7.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage7, out ho_Heightimage7,
                                0);
                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage7, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Seventh Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            //ho_Grayimage7.Dispose(); ho_Heightimage7.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage7, "E:/binnew/11/11/WN1052/3D/灰度图_7.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage7, "E:/binnew/11/11/WN1052/3D/高度图_H8.tif");
                            if (hv_SRadius7 == null)
                                hv_SRadius7 = new HTuple();
                            if (hv_Q7 == null)
                                hv_Q7 = new HTuple();
                            if (hv_mean7 == null)
                                hv_mean7 = new HTuple();
                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                hv_SRadius7.Dispose(); hv_Q7.Dispose(); hv_mean7.Dispose();
                                devThread6 = new Thread(() =>
                                {
                                    try
                                    {
                                        // Call L_C_Compute
                                        L_C_Compute(ho_Grayimage7, ho_Heightimage7, 7, hv_PoseInvert, out hv_SRadius7,
                                        out hv_Q7, out hv_mean7);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 7 " + exc.Message);
                                    }
                                });
                                devThread6.Start();
                            }

                            //**第八段***
                            //Thread.Sleep(3000);
                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            //Thread.Sleep(3000);
                            ho_Grayimage8.Dispose(); ho_Heightimage8.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage8, out ho_Heightimage8,
                                0);
                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage8, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Eighth Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            //ho_Grayimage8.Dispose(); ho_Heightimage8.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage8, "E:/binnew/11/11/WN1052/3D/灰度图_8.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage8, "E:/binnew/11/11/WN1052/3D/高度图_H9.tif");
                            if (hv_SRadius8 == null)
                                hv_SRadius8 = new HTuple();
                            if (hv_Q8 == null)
                                hv_Q8 = new HTuple();
                            if (hv_mean8 == null)
                                hv_mean8 = new HTuple();
                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                hv_SRadius8.Dispose(); hv_Q8.Dispose(); hv_mean8.Dispose();
                                devThread7 = new Thread(() =>
                                {
                                    try
                                    {
                                        L_C_Compute(ho_Grayimage8, ho_Heightimage8, 8, hv_PoseInvert, out hv_SRadius8,
                                        out hv_Q8, out hv_mean8);
                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 8 " + exc.Message);
                                    }
                                });
                                devThread7.Start();
                            }

                            ////**第九段***
                            //Thread.Sleep(3000);

                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            //Thread.Sleep(3000);
                            ho_Grayimage9.Dispose(); ho_Heightimage9.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage9, out ho_Heightimage9,
                                0);
                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage9, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Eighth Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());

                            //ho_Grayimage9.Dispose(); ho_Heightimage9.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage9, "E:/binnew/11/11/WN1052/3D/灰度图_9.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage9, "E:/binnew/11/11/WN1052/3D/高度图_H10.tif");
                            if (hv_SRadius9 == null)
                                hv_SRadius9 = new HTuple();
                            if (hv_Q9 == null)
                                hv_Q9 = new HTuple();
                            if (hv_mean9 == null)
                                hv_mean9 = new HTuple();
                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                hv_SRadius9.Dispose(); hv_Q9.Dispose(); hv_mean9.Dispose();
                                devThread8 = new Thread(() =>
                                {
                                    try
                                    {

                                        // Call L_C_Compute
                                        L_C_Compute(ho_Grayimage9, ho_Heightimage9, 9, hv_PoseInvert, out hv_SRadius9,
                                        out hv_Q9, out hv_mean9);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 9 " + exc.Message);
                                    }
                                });
                                devThread8.Start();
                            }


                            ////**第十段***
                            //Thread.Sleep(3000);
                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            //Thread.Sleep(3000);
                            ho_Grayimage10.Dispose(); ho_Heightimage10.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage10, out ho_Heightimage10,
                                0);
                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage10, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Nineth Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            //ho_Grayimage10.Dispose(); ho_Heightimage10.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage10, "E:/binnew/11/11/WN1052/3D/灰度图_10.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage10, "E:/binnew/11/11/WN1052/3D/高度图_H11.tif");
                            if (hv_SRadius10 == null)
                                hv_SRadius10 = new HTuple();
                            if (hv_Q10 == null)
                                hv_Q10 = new HTuple();
                            if (hv_mean10 == null)
                                hv_mean10 = new HTuple();

                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                hv_SRadius10.Dispose(); hv_Q10.Dispose(); hv_mean10.Dispose();
                                devThread9 = new Thread(() =>
                                {
                                    try
                                    {
                                        // Call L_C_Compute
                                        L_C_Compute(ho_Grayimage10, ho_Heightimage10, 10, hv_PoseInvert, out hv_SRadius10,
                                        out hv_Q10, out hv_mean10);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 10 " + exc.Message);
                                    }
                                });
                                devThread9.Start();
                            }
                            HDlHalconSSZN.SR7IFCommClose(0);
                        }
                        else
                        {
                            ho_Grayimage1.Dispose(); ho_Heightimage1.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage1, "E:/binnew/11/11/WN1052/3D/灰度图_1.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage1, "E:/binnew/11/11/WN1052/3D/1_高度图.tif");

                            if (hv_SRadius1 == null)
                                hv_SRadius1 = new HTuple();
                            if (hv_Q1 == null)
                                hv_Q1 = new HTuple();
                            if (hv_mean1 == null)
                                hv_mean1 = new HTuple();
                            hv_SRadius1.Dispose(); hv_Q1.Dispose(); hv_mean1.Dispose();
                            devThread = new Thread(() =>
                            {
                                try
                                {
                                    L_C_Compute(ho_Grayimage1, ho_Heightimage1, 1, hv_PoseInvert, out hv_SRadius1, out hv_Q1, out hv_mean1);

                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d " + exc.Message);
                                }
                            });
                            devThread.Start();

                            ho_Grayimage2.Dispose(); ho_Heightimage2.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage2, "E:/binnew/11/11/WN1052/3D/灰度图_2.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage2, "E:/binnew/11/11/WN1052/3D/高度图_H3.tif");
                            hv_SRadius2.Dispose(); hv_Q2.Dispose(); hv_mean2.Dispose();
                            devThread1 = new Thread(() =>
                            {
                                try
                                {

                                    // Call L_C_Compute
                                    L_C_Compute(ho_Grayimage2, ho_Heightimage2, 2, hv_PoseInvert, out hv_SRadius2,
                                                out hv_Q2, out hv_mean2);

                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 2 " + exc.Message);
                                }
                            });
                            devThread1.Start();

                            ho_Grayimage3.Dispose(); ho_Heightimage3.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage3, "E:/binnew/11/11/WN1052/3D/灰度图_3.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage3, "E:/binnew/11/11/WN1052/3D/高度图_H4.tif");
                            if (hv_SRadius3 == null)
                                hv_SRadius3 = new HTuple();
                            if (hv_Q3 == null)
                                hv_Q3 = new HTuple();
                            if (hv_mean3 == null)
                                hv_mean3 = new HTuple();
                            hv_SRadius3.Dispose(); hv_Q3.Dispose(); hv_mean3.Dispose();
                            devThread2 = new Thread(() =>
                            {
                                try
                                {
                                    // Call L_C_Compute
                                    L_C_Compute(ho_Grayimage3, ho_Heightimage3, 3, hv_PoseInvert, out hv_SRadius3,
                                                out hv_Q3, out hv_mean3);
                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 2 " + exc.Message);
                                }
                            });
                            devThread2.Start();

                            ho_Grayimage4.Dispose(); ho_Heightimage4.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage4, "E:/binnew/11/11/WN1052/3D/灰度图_4.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage4, "E:/binnew/11/11/WN1052/3D/高度图_H5.tif");
                            if (hv_SRadius4 == null)
                                hv_SRadius4 = new HTuple();
                            if (hv_Q4 == null)
                                hv_Q4 = new HTuple();
                            if (hv_mean4 == null)
                                hv_mean4 = new HTuple();
                            hv_SRadius4.Dispose(); hv_Q4.Dispose(); hv_mean4.Dispose();
                            devThread3 = new Thread(() =>
                            {
                                try
                                {
                                    // Call L_C_Compute
                                    L_C_Compute(ho_Grayimage4, ho_Heightimage4, 4, hv_PoseInvert, out hv_SRadius4,
                                    out hv_Q4, out hv_mean4);
                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 4 " + exc.Message);
                                }
                            });
                            devThread3.Start();

                            ho_Grayimage5.Dispose(); ho_Heightimage5.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage5, "E:/binnew/11/11/WN1052/3D/灰度图_5.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage5, "E:/binnew/11/11/WN1052/3D/高度图_H6.tif");
                            if (hv_SRadius5 == null)
                                hv_SRadius5 = new HTuple();
                            if (hv_Q5 == null)
                                hv_Q5 = new HTuple();
                            if (hv_mean5 == null)
                                hv_mean5 = new HTuple();
                            hv_SRadius5.Dispose(); hv_Q5.Dispose(); hv_mean5.Dispose();
                            devThread4 = new Thread(() =>
                            {
                                try
                                {
                                    // Call L_C_Compute
                                    L_C_Compute(ho_Grayimage5, ho_Heightimage5, 5, hv_PoseInvert, out hv_SRadius5,
                                    out hv_Q5, out hv_mean5);

                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 5 " + exc.Message);
                                }
                            });
                            devThread4.Start();

                            ho_Grayimage6.Dispose(); ho_Heightimage6.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage6, "E:/binnew/11/11/WN1052/3D/灰度图_6.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage6, "E:/binnew/11/11/WN1052/3D/高度图_H7.tif");
                            if (hv_SRadius6 == null)
                                hv_SRadius6 = new HTuple();
                            if (hv_Q6 == null)
                                hv_Q6 = new HTuple();
                            if (hv_mean6 == null)
                                hv_mean6 = new HTuple();
                            hv_SRadius6.Dispose(); hv_Q6.Dispose(); hv_mean6.Dispose();
                            devThread5 = new Thread(() =>
                            {
                                try
                                {

                                    // Call L_C_Compute
                                    L_C_Compute(ho_Grayimage6, ho_Heightimage6, 6, hv_PoseInvert, out hv_SRadius6,
                                    out hv_Q6, out hv_mean6);

                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 6 " + exc.Message);
                                }
                            });
                            devThread5.Start();


                            ho_Grayimage7.Dispose(); ho_Heightimage7.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage7, "E:/binnew/11/11/WN1052/3D/灰度图_7.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage7, "E:/binnew/11/11/WN1052/3D/高度图_H8.tif");
                            if (hv_SRadius7 == null)
                                hv_SRadius7 = new HTuple();
                            if (hv_Q7 == null)
                                hv_Q7 = new HTuple();
                            if (hv_mean7 == null)
                                hv_mean7 = new HTuple();
                            hv_SRadius7.Dispose(); hv_Q7.Dispose(); hv_mean7.Dispose();
                            devThread6 = new Thread(() =>
                            {
                                try
                                {
                                    // Call L_C_Compute
                                    L_C_Compute(ho_Grayimage7, ho_Heightimage7, 7, hv_PoseInvert, out hv_SRadius7,
                                    out hv_Q7, out hv_mean7);

                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 7 " + exc.Message);
                                }
                            });
                            devThread6.Start();

                            ho_Grayimage8.Dispose(); ho_Heightimage8.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage8, "E:/binnew/11/11/WN1052/3D/灰度图_8.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage8, "E:/binnew/11/11/WN1052/3D/高度图_H9.tif");

                            if (hv_SRadius8 == null)
                                hv_SRadius8 = new HTuple();
                            if (hv_Q8 == null)
                                hv_Q8 = new HTuple();
                            if (hv_mean8 == null)
                                hv_mean8 = new HTuple();
                            hv_SRadius8.Dispose(); hv_Q8.Dispose(); hv_mean8.Dispose();
                            devThread7 = new Thread(() =>
                            {
                                try
                                {
                                    L_C_Compute(ho_Grayimage8, ho_Heightimage8, 8, hv_PoseInvert, out hv_SRadius8,
                                    out hv_Q8, out hv_mean8);
                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 8 " + exc.Message);
                                }
                            });
                            devThread7.Start();


                            ho_Grayimage9.Dispose(); ho_Heightimage9.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage9, "E:/binnew/11/11/WN1052/3D/灰度图_9.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage9, "E:/binnew/11/11/WN1052/3D/高度图_H10.tif");

                            if (hv_SRadius9 == null)
                                hv_SRadius9 = new HTuple();
                            if (hv_Q9 == null)
                                hv_Q9 = new HTuple();
                            if (hv_mean9 == null)
                                hv_mean9 = new HTuple();

                            hv_SRadius9.Dispose(); hv_Q9.Dispose(); hv_mean9.Dispose();
                            devThread8 = new Thread(() =>
                            {
                                try
                                {

                                    // Call L_C_Compute
                                    L_C_Compute(ho_Grayimage9, ho_Heightimage9, 9, hv_PoseInvert, out hv_SRadius9,
                                    out hv_Q9, out hv_mean9);

                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 9 " + exc.Message);
                                }
                            });
                            devThread8.Start();

                            ho_Grayimage10.Dispose(); ho_Heightimage10.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage10, "E:/binnew/11/11/WN1052/3D/灰度图_10.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage10, "E:/binnew/11/11/WN1052/3D/高度图_H11.tif");

                            if (hv_SRadius10 == null)
                                hv_SRadius10 = new HTuple();
                            if (hv_Q10 == null)
                                hv_Q10 = new HTuple();
                            if (hv_mean10 == null)
                                hv_mean10 = new HTuple();

                            hv_SRadius10.Dispose(); hv_Q10.Dispose(); hv_mean10.Dispose();
                            devThread9 = new Thread(() =>
                            {
                                try
                                {
                                    // Call L_C_Compute
                                    L_C_Compute(ho_Grayimage10, ho_Heightimage10, 10, hv_PoseInvert, out hv_SRadius10,
                                    out hv_Q10, out hv_mean10);

                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 10 " + exc.Message);
                                }
                            });
                            devThread9.Start();
                        }

                        while (true)
                        {
                            if ((devThread != null && devThread.ThreadState != ThreadState.Stopped) || (devThread1 != null && devThread1.ThreadState != ThreadState.Stopped) ||
                                (devThread2 != null && devThread2.ThreadState != ThreadState.Stopped) || (devThread3 != null && devThread3.ThreadState != ThreadState.Stopped) ||
                                (devThread4 != null && devThread4.ThreadState != ThreadState.Stopped) || (devThread5 != null && devThread5.ThreadState != ThreadState.Stopped) ||
                                (devThread6 != null && devThread6.ThreadState != ThreadState.Stopped) || (devThread7 != null && devThread7.ThreadState != ThreadState.Stopped) ||
                                (devThread8 != null && devThread8.ThreadState != ThreadState.Stopped) || (devThread9 != null && devThread9.ThreadState != ThreadState.Stopped))
                            {
                                Thread.Sleep(1000);
                                continue;
                            }
                            else
                            {
                                break;
                            }
                            //hv_QQ.Dispose();
                            //using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            //{
                            //    hv_QQ = hv_Q1 + hv_Q2 + hv_Q3 + hv_Q4 + hv_Q5 + hv_Q6 + hv_Q7 + hv_Q8 + hv_Q9 + hv_Q10;
                            //}
                            //if (new HTuple(hv_QQ.TupleEqual(10)) != 0)
                            //{
                            //    break;
                            //}

                        }

                        LogHelper.Info("Silicon", "Measur_3D wait 10 Thread End");


                        //par_join ([ThreadID,ThreadID1,ThreadID2,ThreadID3,ThreadID4,ThreadID5,ThreadID6,ThreadID7,ThreadID8])
                        //**第十一段***
                        //SR7IF_StartMeasure (0, 10000) // 开始批处理
                        //SR7IF_GetBatchRollData (Grayimage11, Heightimage11, 0) // 取图
                        //L_C_Compute (Grayimage11, Heightimage11, 11, SRadius11, Q10, mean10010)
                        ho_Imageconst.Dispose();
                        HX_DEMONew(ho_Grayimage1, ho_Grayimage2, ho_Grayimage3, ho_Grayimage4, ho_Grayimage5,
                            ho_Grayimage6, ho_Grayimage7, ho_Grayimage8, ho_Grayimage9, ho_Grayimage10,
                            ho_Heightimage1, ho_Heightimage2, ho_Heightimage3, ho_Heightimage4,
                            ho_Heightimage5, ho_Heightimage6, ho_Heightimage7, ho_Heightimage8,
                            ho_Heightimage9, ho_Heightimage10, out ho_Imageconst, hv_ResultDictHandle,
                            hv_Rate);
                        hv_Raduiszz1.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_Raduiszz1 = new HTuple();
                            hv_Raduiszz1 = hv_Raduiszz1.TupleConcat(hv_SRadius1, hv_SRadius2, hv_SRadius3, hv_SRadius4, hv_SRadius5, hv_SRadius6, hv_SRadius7, hv_SRadius8, hv_SRadius9, hv_SRadius10);
                        }
                        hv_Raduiszz.Dispose();
                        hv_Raduiszz = new HTuple(hv_Raduiszz1);
                        hv_g.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_g = (hv_Raduiszz * 0) + 160;
                        }
                        hv_l.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_l = (hv_Raduiszz * 0) + 120;
                        }
                        hv_Greater.Dispose();
                        HOperatorSet.TupleGreaterElem(hv_Raduiszz, hv_g, out hv_Greater);
                        hv_Less.Dispose();
                        HOperatorSet.TupleLessElem(hv_Raduiszz, hv_l, out hv_Less);
                        hv_Indices1.Dispose();
                        HOperatorSet.TupleFind(hv_Greater, 1, out hv_Indices1);
                        hv_Indices2.Dispose();
                        HOperatorSet.TupleFind(hv_Less, 1, out hv_Indices2);
                        hv_k.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_k = new HTuple();
                            hv_k = hv_k.TupleConcat(hv_Indices1, hv_Indices2);
                        }
                        hv_Reduced.Dispose();
                        HOperatorSet.TupleRemove(hv_Raduiszz, hv_k, out hv_Reduced);
                        //Remove_max_min (Reduced, 50, MeanRadius1)
                        //MeanRadius100 := MeanRadius1*0.984
                        try
                        {
                            hv_first.Dispose(); hv_end.Dispose();
                            tuple_fe(hv_Reduced, out hv_first, out hv_end);
                            hv_Selected.Dispose();
                            HOperatorSet.TupleSelectRange(hv_Reduced, hv_first, hv_end, out hv_Selected);
                            hv_Reduced.Dispose();
                            Medium_Remove_Catch(hv_Selected, out hv_Reduced);
                            hv_Function.Dispose();
                            HOperatorSet.CreateFunct1dArray(hv_Reduced, out hv_Function);
                            hv_Derivative.Dispose();
                            HOperatorSet.DerivateFunct1d(hv_Function, "second", out hv_Derivative);
                            hv_FunctionAbsolute.Dispose();
                            HOperatorSet.AbsFunct1d(hv_Derivative, out hv_FunctionAbsolute);
                            hv_Greater.Dispose();
                            HOperatorSet.TupleGreaterElem(hv_FunctionAbsolute, 0.0042, out hv_Greater);
                            hv_Indices1.Dispose();
                            HOperatorSet.TupleFind(hv_Greater, 1, out hv_Indices1);
                            hv_k1.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_k1 = hv_Indices1 - 3;
                            }
                            hv_Reduced4.Dispose();
                            HOperatorSet.TupleRemove(hv_Reduced, hv_k1, out hv_Reduced4);
                        }
                        // catch (Exception) 
                        catch (HalconException HDevExpDefaultException2)
                        {
                            HDevExpDefaultException2.ToHTuple(out hv_Exception);
                            hv_Reduced.Dispose();
                            hv_Reduced = 0;
                        }
                        try
                        {
                            hv_Max.Dispose();
                            HOperatorSet.TupleMax(hv_Reduced, out hv_Max);
                            hv_Min.Dispose();
                            HOperatorSet.TupleMin(hv_Reduced, out hv_Min);
                            hv_Mean.Dispose();
                            HOperatorSet.TupleMean(hv_Reduced, out hv_Mean);
                            hv_DiameterMax.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_DiameterMax = 2 * hv_Max;
                            }
                            hv_DiameterMin.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_DiameterMin = 2 * hv_Min;
                            }
                            hv_DiameterMean.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_DiameterMean = 2 * hv_Mean;
                            }
                        }
                        // catch (Exception) 
                        catch (HalconException HDevExpDefaultException2)
                        {
                            HDevExpDefaultException2.ToHTuple(out hv_Exception);
                            hv_DiameterMax.Dispose();
                            hv_DiameterMax = 0;
                            hv_DiameterMin.Dispose();
                            hv_DiameterMin = 0;
                            hv_DiameterMean.Dispose();
                            hv_DiameterMean = 0;
                        }

                        try
                        {
                            hv_MessageHandle.Dispose();
                            HOperatorSet.CreateMessage(out hv_MessageHandle);
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetMessageTuple(hv_MessageHandle, "SS", ((hv_DiameterMax.TupleConcat(
                                    hv_DiameterMin))).TupleConcat(hv_DiameterMean));
                            }
                            HOperatorSet.EnqueueMessage(hv_LQueueHandle, hv_MessageHandle, new HTuple(),
                                new HTuple());
                            HOperatorSet.ClearMessage(hv_MessageHandle);
                        }
                        // catch (Exception) 
                        catch (HalconException HDevExpDefaultException2)
                        {
                            HDevExpDefaultException2.ToHTuple(out hv_Exception);
                        }
                        hv_f.Dispose();
                        hv_f = new HTuple();
                        for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_Selected.TupleLength()
                            )) / 1); hv_Index = (int)hv_Index + 10)
                        {
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_Selected1.Dispose();
                                HOperatorSet.TupleSelectRange(hv_Selected, 10 * hv_Index, (10 * hv_Index) + 10,
                                    out hv_Selected1);
                            }
                            hv_Mean1.Dispose();
                            HOperatorSet.TupleMean(hv_Selected1, out hv_Mean1);
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_f = hv_f.TupleConcat(
                                        hv_Mean1);
                                    hv_f.Dispose();
                                    hv_f = ExpTmpLocalVar_f;
                                }
                            }
                        }
                        CGlobalFuncTools.Instance().Set_Twokey_Tuple(hv_ResultDictHandle, "直径", "直径曲线数组", hv_MeanRadius1);
                        hv_IDTime.Dispose();
                        CGlobalFuncTools.Instance().CreateID(out hv_IDTime);

                    }
                    // catch (Exception) 
                    catch (HalconException HDevExpDefaultException1)
                    {
                        HDevExpDefaultException1.ToHTuple(out hv_Exception);
                        LogHelper.Info("Silicon", "Measur_3D wait 10 Exception " + hv_Exception.ToString());
                    }



                    ho_EmptyObject.Dispose();
                    ho_Grayimage1.Dispose();
                    ho_Heightimage1.Dispose();
                    ho_Grayimage2.Dispose();
                    ho_Heightimage2.Dispose();
                    ho_Grayimage3.Dispose();
                    ho_Heightimage3.Dispose();
                    ho_Grayimage4.Dispose();
                    ho_Heightimage4.Dispose();
                    ho_Grayimage5.Dispose();
                    ho_Heightimage5.Dispose();
                    ho_Grayimage6.Dispose();
                    ho_Heightimage6.Dispose();
                    ho_Grayimage7.Dispose();
                    ho_Heightimage7.Dispose();
                    ho_Grayimage8.Dispose();
                    ho_Heightimage8.Dispose();

                    ho_Rectangle.Dispose();
                    ho_Rectangle1.Dispose();
                    ho_Rectangle2.Dispose();

                    hv_Rate.Dispose();
                    hv_Seconds1.Dispose();
                    hv_MessageHandle.Dispose();
                    hv_Start.Dispose();
                    hv_Seconds2.Dispose();
                    hv_Name.Dispose();

                    hv_Q1.Dispose();
                    hv_Q2.Dispose();
                    hv_Q3.Dispose();
                    hv_Q4.Dispose();
                    hv_Q5.Dispose();
                    hv_Q6.Dispose();
                    hv_Q7.Dispose();
                    hv_Q8.Dispose();
                    hv_Q9.Dispose();
                    hv_SRadius1.Dispose();
                    hv_mean1.Dispose();
                    hv_ThreadID1.Dispose();
                    hv_SRadius2.Dispose();
                    hv_mean2.Dispose();
                    hv_ThreadID.Dispose();
                    hv_SRadius3.Dispose();
                    hv_mean3.Dispose();
                    hv_ThreadID9.Dispose();
                    hv_SRadius4.Dispose();
                    hv_mean4.Dispose();
                    hv_ThreadID10.Dispose();
                    hv_SRadius5.Dispose();
                    hv_mean5.Dispose();
                    hv_ThreadID4.Dispose();
                    hv_SRadius6.Dispose();
                    hv_mean6.Dispose();
                    hv_ThreadID5.Dispose();
                    hv_SRadius7.Dispose();
                    hv_mean7.Dispose();
                    hv_ThreadID6.Dispose();
                    hv_SRadius8.Dispose();
                    hv_mean8.Dispose();
                    hv_ThreadID7.Dispose();
                    hv_SRadius9.Dispose();
                    hv_ThreadID8.Dispose();
                    hv_QQ.Dispose();
                    hv_Exception.Dispose();
                    hv_Coordinate.Dispose();
                    hv_Length.Dispose();
                    hv_H.Dispose();
                    hv_T.Dispose();
                    hv_Head.Dispose();
                    hv_Tail.Dispose();
                    hv_CoordinateHT.Dispose();
                    hv_Number1.Dispose();
                    hv_L.Dispose();
                    hv_Index2.Dispose();
                    hv_MeanRadius.Dispose();
                    hv_Function.Dispose();
                    hv_Greater.Dispose();
                    hv_Indices2.Dispose();
                    hv_Reduced.Dispose();
                    hv_Indices3.Dispose();
                    hv_Indices4.Dispose();
                    hv_Indices1.Dispose();
                    hv_Reduced1.Dispose();
                    hv_Max.Dispose();
                    hv_Min.Dispose();
                    hv_Mean.Dispose();
                    hv_DiameterMax.Dispose();
                    hv_DiameterMin.Dispose();
                    hv_DiameterMean.Dispose();
                    hv_IDTime.Dispose();
                }
                catch (HalconException ex)
                {

                    ho_EmptyObject.Dispose();
                    ho_Grayimage1.Dispose();
                    ho_Heightimage1.Dispose();
                    ho_Grayimage2.Dispose();
                    ho_Heightimage2.Dispose();
                    ho_Grayimage3.Dispose();
                    ho_Heightimage3.Dispose();
                    ho_Grayimage4.Dispose();
                    ho_Heightimage4.Dispose();
                    ho_Grayimage5.Dispose();
                    ho_Heightimage5.Dispose();
                    ho_Grayimage6.Dispose();
                    ho_Heightimage6.Dispose();
                    ho_Grayimage7.Dispose();
                    ho_Heightimage7.Dispose();
                    ho_Grayimage8.Dispose();
                    ho_Heightimage8.Dispose();

                    ho_Rectangle.Dispose();
                    ho_Rectangle1.Dispose();
                    ho_Rectangle2.Dispose();

                    hv_Rate.Dispose();
                    hv_Seconds1.Dispose();
                    hv_MessageHandle.Dispose();
                    hv_Start.Dispose();
                    hv_Seconds2.Dispose();
                    hv_Name.Dispose();

                    hv_Q1.Dispose();
                    hv_Q2.Dispose();
                    hv_Q3.Dispose();
                    hv_Q4.Dispose();
                    hv_Q5.Dispose();
                    hv_Q6.Dispose();
                    hv_Q7.Dispose();
                    hv_Q8.Dispose();
                    hv_Q9.Dispose();
                    hv_SRadius1.Dispose();
                    hv_mean1.Dispose();
                    hv_ThreadID1.Dispose();
                    hv_SRadius2.Dispose();
                    hv_mean2.Dispose();
                    hv_ThreadID.Dispose();
                    hv_SRadius3.Dispose();
                    hv_mean3.Dispose();
                    hv_ThreadID9.Dispose();
                    hv_SRadius4.Dispose();
                    hv_mean4.Dispose();
                    hv_ThreadID10.Dispose();
                    hv_SRadius5.Dispose();
                    hv_mean5.Dispose();
                    hv_ThreadID4.Dispose();
                    hv_SRadius6.Dispose();
                    hv_mean6.Dispose();
                    hv_ThreadID5.Dispose();
                    hv_SRadius7.Dispose();
                    hv_mean7.Dispose();
                    hv_ThreadID6.Dispose();
                    hv_SRadius8.Dispose();
                    hv_mean8.Dispose();
                    hv_ThreadID7.Dispose();
                    hv_SRadius9.Dispose();

                    hv_ThreadID8.Dispose();

                    hv_QQ.Dispose();
                    hv_Exception.Dispose();
                    hv_Coordinate.Dispose();
                    hv_Length.Dispose();
                    hv_H.Dispose();
                    hv_T.Dispose();
                    hv_Head.Dispose();
                    hv_Tail.Dispose();
                    hv_CoordinateHT.Dispose();
                    hv_Number1.Dispose();
                    hv_L.Dispose();
                    hv_Index2.Dispose();
                    hv_MeanRadius.Dispose();
                    hv_Function.Dispose();
                    hv_Greater.Dispose();
                    hv_Indices2.Dispose();
                    hv_Reduced.Dispose();
                    hv_Indices3.Dispose();
                    hv_Indices4.Dispose();
                    hv_Indices1.Dispose();
                    hv_Reduced1.Dispose();
                    hv_Max.Dispose();
                    hv_Min.Dispose();
                    hv_Mean.Dispose();
                    hv_DiameterMax.Dispose();
                    hv_DiameterMin.Dispose();
                    hv_DiameterMean.Dispose();
                    hv_IDTime.Dispose();

                }


                return;
            }
        }

        public void Medium_Remove_Catch(HTuple hv_Selected, out HTuple hv_Reduced)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_g = new HTuple(), hv_l = new HTuple();
            HTuple hv_Median = new HTuple(), hv_Mean1 = new HTuple();
            HTuple hv_Min = new HTuple(), hv_f = new HTuple(), hv_Greater = new HTuple();
            HTuple hv_Less = new HTuple(), hv_Indices1 = new HTuple();
            HTuple hv_Indices2 = new HTuple(), hv_k = new HTuple();
            // Initialize local and output iconic variables 
            hv_Reduced = new HTuple();
            try
            {
                hv_g.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_g = (hv_Selected * 0) + 1;
                }
                hv_l.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_l = (hv_Selected * 0) - 1;
                }
                hv_Median.Dispose();
                HOperatorSet.TupleMedian(hv_Selected, out hv_Median);
                hv_Mean1.Dispose();
                HOperatorSet.TupleMean(hv_Selected, out hv_Mean1);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Min.Dispose();
                    HOperatorSet.TupleMin(hv_Median.TupleConcat(hv_Mean1), out hv_Min);
                }
                hv_f.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_f = hv_Selected - ((hv_Selected * 0) + hv_Min);
                }
                hv_Greater.Dispose();
                HOperatorSet.TupleGreaterElem(hv_f, hv_g, out hv_Greater);
                hv_Less.Dispose();
                HOperatorSet.TupleLessElem(hv_f, hv_l, out hv_Less);
                hv_Indices1.Dispose();
                HOperatorSet.TupleFind(hv_Greater, 1, out hv_Indices1);
                hv_Indices2.Dispose();
                HOperatorSet.TupleFind(hv_Less, 1, out hv_Indices2);
                hv_k.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_k = new HTuple();
                    hv_k = hv_k.TupleConcat(hv_Indices1, hv_Indices2);
                }
                hv_Reduced.Dispose();
                HOperatorSet.TupleRemove(hv_Selected, hv_k, out hv_Reduced);

                hv_g.Dispose();
                hv_l.Dispose();
                hv_Median.Dispose();
                hv_Mean1.Dispose();
                hv_Min.Dispose();
                hv_f.Dispose();
                hv_Greater.Dispose();
                hv_Less.Dispose();
                hv_Indices1.Dispose();
                hv_Indices2.Dispose();
                hv_k.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_g.Dispose();
                hv_l.Dispose();
                hv_Median.Dispose();
                hv_Mean1.Dispose();
                hv_Min.Dispose();
                hv_f.Dispose();
                hv_Greater.Dispose();
                hv_Less.Dispose();
                hv_Indices1.Dispose();
                hv_Indices2.Dispose();
                hv_k.Dispose();

                throw HDevExpDefaultException;
            }
        }
        public void Measure_3D(out HObject ho_Imageconst,  HTuple hv_LQueueHandle, HTuple hv_ResultDictHandle)
        {

            //using (HDevThreadContext context = new HDevThreadContext())
            {
                // +++ Threading variables 
                Thread devThread = null;
                Thread devThread1 = null;
                Thread devThread2 = null;
                Thread devThread3 = null;
                Thread devThread4 = null;
                Thread devThread5 = null;
                Thread devThread6 = null;
                Thread devThread7 = null;
                Thread devThread8 = null;
                Thread devThread9 = null;



                // Local iconic variables 

                HObject ho_EmptyObject = null, ho_Grayimage1 = null;
                HObject ho_Heightimage1 = null, ho_Grayimage2 = null, ho_Heightimage2 = null;
                HObject ho_Grayimage3 = null, ho_Heightimage3 = null, ho_Grayimage4 = null;
                HObject ho_Heightimage4 = null, ho_Grayimage5 = null, ho_Heightimage5 = null;
                HObject ho_Grayimage6 = null, ho_Heightimage6 = null, ho_Grayimage7 = null;
                HObject ho_Heightimage7 = null, ho_Grayimage8 = null, ho_Heightimage8 = null;
                HObject ho_Grayimage9 = null, ho_Heightimage9 = null, ho_Grayimage10 = null;
                HObject ho_Heightimage10 = null;
                HObject  ho_Rectangle = null, ho_Rectangle1 = null;
                HObject ho_Rectangle2 = null;

                // Local control variables 

                HTuple hv_Rate = new HTuple(), hv_Seconds1 = new HTuple();
                HTuple hv_MessageHandle = new HTuple(), hv_Start = new HTuple();
                HTuple hv_Seconds2 = new HTuple(), hv_Name = new HTuple();
                HTuple hv_Q1 = new HTuple(), hv_Q2 = new HTuple();
                HTuple hv_Q3 = new HTuple(), hv_Q4 = new HTuple(), hv_Q5 = new HTuple();
                HTuple hv_Q6 = new HTuple(), hv_Q7 = new HTuple(), hv_Q8 = new HTuple();
                HTuple hv_Q9 = new HTuple(), hv_SRadius1 = new HTuple();
                HTuple hv_ThreadID1 = new HTuple();
                HTuple hv_SRadius2 = new HTuple(), hv_mean1 = new HTuple();
                HTuple hv_ThreadID = new HTuple(), hv_SRadius3 = new HTuple();
                HTuple hv_mean2 = new HTuple(), hv_ThreadID9 = new HTuple();
                HTuple hv_SRadius4 = new HTuple(), hv_mean3 = new HTuple();
                HTuple hv_ThreadID10 = new HTuple(), hv_SRadius5 = new HTuple();
                HTuple hv_mean4 = new HTuple(), hv_ThreadID4 = new HTuple();
                HTuple hv_SRadius6 = new HTuple(), hv_mean5 = new HTuple();
                HTuple hv_ThreadID5 = new HTuple(), hv_SRadius7 = new HTuple();
                HTuple hv_mean6 = new HTuple(), hv_ThreadID6 = new HTuple();
                HTuple hv_SRadius8 = new HTuple(), hv_mean7 = new HTuple();
                HTuple hv_ThreadID7 = new HTuple(), hv_SRadius9 = new HTuple();
                HTuple hv_mean8 = new HTuple(), hv_ThreadID8 = new HTuple();
                HTuple hv_Q10 = new HTuple(), hv_mean10 = new HTuple();
                HTuple hv_SRadius10 = new HTuple(), hv_mean9 = new HTuple();

                HTuple hv_QQ = new HTuple(), hv_Exception = new HTuple();
                HTuple hv_Coordinate = new HTuple(), hv_Length = new HTuple();
                HTuple hv_H = new HTuple(), hv_T = new HTuple(), hv_Head = new HTuple();
                HTuple hv_Tail = new HTuple(), hv_CoordinateHT = new HTuple();
                HTuple hv_Number1 = new HTuple(), hv_L = new HTuple();
                HTuple hv_Index2 = new HTuple(), hv_MeanRadius = new HTuple();
                HTuple hv_Function = new HTuple(), hv_Greater = new HTuple();
                HTuple hv_Indices2 = new HTuple(), hv_Reduced = new HTuple();
                HTuple hv_Indices3 = new HTuple(), hv_Indices4 = new HTuple();
                HTuple hv_Indices1 = new HTuple(), hv_Reduced1 = new HTuple();
                HTuple hv_Max = new HTuple(), hv_Min = new HTuple(), hv_Mean = new HTuple();
                HTuple hv_DiameterMax = new HTuple(), hv_DiameterMin = new HTuple();
                HTuple hv_DiameterMean = new HTuple(), hv_IDTime = new HTuple();
                HTuple hv_k = new HTuple();
                HTuple hv_MeanRadius1 = new HTuple();
                HTuple hv_MeanRadius100 = new HTuple(), hv_first = new HTuple();
                HTuple hv_end = new HTuple(), hv_Selected = new HTuple();
                HTuple hv_Raduiszz = new HTuple();
                HTuple hv_g = new HTuple(), hv_l = new HTuple();
                HTuple hv_Less = new HTuple();
                HTuple hv_PoseInvert = new HTuple();
                hv_Rate = 0.28316683;
                hv_PoseInvert.Dispose();
                HOperatorSet.ReadTuple("./PoseInvert.tup", out hv_PoseInvert);

                // Initialize local and output iconic variables 
                HOperatorSet.GenEmptyObj(out ho_Imageconst);
                HOperatorSet.GenEmptyObj(out ho_EmptyObject);
                HOperatorSet.GenEmptyObj(out ho_Grayimage1);
                HOperatorSet.GenEmptyObj(out ho_Heightimage1);
                HOperatorSet.GenEmptyObj(out ho_Grayimage2);
                HOperatorSet.GenEmptyObj(out ho_Heightimage2);
                HOperatorSet.GenEmptyObj(out ho_Grayimage3);
                HOperatorSet.GenEmptyObj(out ho_Heightimage3);
                HOperatorSet.GenEmptyObj(out ho_Grayimage4);
                HOperatorSet.GenEmptyObj(out ho_Heightimage4);
                HOperatorSet.GenEmptyObj(out ho_Grayimage5);
                HOperatorSet.GenEmptyObj(out ho_Heightimage5);
                HOperatorSet.GenEmptyObj(out ho_Grayimage6);
                HOperatorSet.GenEmptyObj(out ho_Heightimage6);
                HOperatorSet.GenEmptyObj(out ho_Grayimage7);
                HOperatorSet.GenEmptyObj(out ho_Heightimage7);
                HOperatorSet.GenEmptyObj(out ho_Grayimage8);
                HOperatorSet.GenEmptyObj(out ho_Heightimage8);
                HOperatorSet.GenEmptyObj(out ho_Grayimage9);
                HOperatorSet.GenEmptyObj(out ho_Heightimage9);
                HOperatorSet.GenEmptyObj(out ho_Grayimage10);
                HOperatorSet.GenEmptyObj(out ho_Heightimage10);
                HOperatorSet.GenEmptyObj(out ho_Rectangle);
                HOperatorSet.GenEmptyObj(out ho_Rectangle1);
                HOperatorSet.GenEmptyObj(out ho_Rectangle2);
                try
                {
                    //global tuple QuitTaskFlag
                    
                    try
                    {
                        if (SettingParameter.Instance().NDaemon == 1)
                        {
                            int nIndexOfImage = 0;
                            HDlHalconSSZN.SR7IFEthernetOpen(0, 192, 168, 0, 23);
                            hv_Rate.Dispose();
                            hv_Rate = 0.28316683;
                            hv_Seconds1.Dispose();
                            HOperatorSet.CountSeconds(out hv_Seconds1);

                            //**第一段***
                            HTuple houtwidth = new HTuple();
                            HTuple houtheight = new HTuple();

                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            ho_Grayimage1.Dispose(); ho_Heightimage1.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage1, out ho_Heightimage1,
                                0);
                            HOperatorSet.GetImageSize(ho_Grayimage1, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "First Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            //ho_Grayimage1.Dispose(); ho_Heightimage1.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage1, "E:/binnew/11/11/WN1052/3D/灰度图_1.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage1, "E:/binnew/11/11/WN1052/3D/1_高度图.tif");
                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                //HOperatorSet.WriteImage(ho_Grayimage1, "bmp", 0, "D:/" + nIndexOfImage.ToString() + ".bmp");
                                //HOperatorSet.WriteImage(ho_Heightimage1, "tiff", 0, "D:/" + nIndexOfImage.ToString() + ".tif");

                                if (hv_SRadius1 == null)
                                    hv_SRadius1 = new HTuple();
                                if (hv_Q1 == null)
                                    hv_Q1 = new HTuple();
                                if (hv_mean1 == null)
                                    hv_mean1 = new HTuple();
                                hv_SRadius1.Dispose(); hv_Q1.Dispose(); hv_mean1.Dispose();
                                devThread = new Thread(() =>
                                {
                                    try
                                    {
                                        // Input parameters
                                        L_C_Compute(ho_Grayimage1, ho_Heightimage1, 1, hv_PoseInvert, out hv_SRadius1, out hv_Q1, out hv_mean1);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d " + exc.Message);
                                    }
                                });
                                devThread.Start();
                            }
                           
                           

                           


                            if (hv_SRadius2 == null)
                                hv_SRadius2 = new HTuple();
                            if (hv_Q2 == null)
                                hv_Q2 = new HTuple();
                            if (hv_mean2 == null)
                                hv_mean2 = new HTuple();

                            // Thread.Sleep(3000);
                            //**第二段***
                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);

                            ho_Grayimage2.Dispose(); ho_Heightimage2.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage2, out ho_Heightimage2,
                                0);
                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage2, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Second Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            //ho_Grayimage2.Dispose(); ho_Heightimage2.Dispose();
                            ////HOperatorSet.ReadImage(out ho_Grayimage2, "E:/binnew/11/11/WN1052/3D/灰度图_2.bmp");
                            ////HOperatorSet.ReadImage(out ho_Heightimage2, "E:/binnew/11/11/WN1052/3D/高度图_H3.tif");
                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                //HOperatorSet.WriteImage(ho_Grayimage2, "bmp", 0, "D:/" + nIndexOfImage.ToString() + ".bmp");
                                //HOperatorSet.WriteImage(ho_Heightimage2, "tiff", 0, "D:/" + nIndexOfImage.ToString() + ".tif");

                                hv_SRadius2.Dispose(); hv_Q2.Dispose(); hv_mean2.Dispose();
                                devThread1 = new Thread(() =>
                                {
                                    try
                                    {
                                        // Input parameters


                                        // Call L_C_Compute
                                        L_C_Compute(ho_Grayimage2, ho_Heightimage2, 2, hv_PoseInvert, out hv_SRadius2,
                                                    out hv_Q2, out hv_mean2);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 2 " + exc.Message);
                                    }
                                });
                                devThread1.Start();
                            }
                            

                            //**第三段***
                            //Thread.Sleep(3000);

                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            //Thread.Sleep(3000);
                            ho_Grayimage3.Dispose(); ho_Heightimage3.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage3, out ho_Heightimage3,
                                0);
                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage3, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Third Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            //ho_Grayimage3.Dispose(); ho_Heightimage3.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage3, "E:/binnew/11/11/WN1052/3D/灰度图_3.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage3, "E:/binnew/11/11/WN1052/3D/高度图_H4.tif");
                            if (hv_SRadius3 == null)
                                hv_SRadius3 = new HTuple();
                            if (hv_Q3 == null)
                                hv_Q3 = new HTuple();
                            if (hv_mean3 == null)
                                hv_mean3 = new HTuple();

                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                //HOperatorSet.WriteImage(ho_Grayimage3, "bmp", 0, "D:/" + nIndexOfImage.ToString() + ".bmp");
                                //HOperatorSet.WriteImage(ho_Heightimage3, "tiff", 0, "D:/" + nIndexOfImage.ToString() + ".tif");

                                hv_SRadius3.Dispose(); hv_Q3.Dispose(); hv_mean3.Dispose();
                                devThread2 = new Thread(() =>
                                {
                                    try
                                    {

                                        // Call L_C_Compute
                                        L_C_Compute(ho_Grayimage3, ho_Heightimage3, 3, hv_PoseInvert, out hv_SRadius3,
                                                    out hv_Q3, out hv_mean3);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 2 " + exc.Message);
                                    }
                                });
                                devThread2.Start();
                            }
                            //**第四段***
                            Thread.Sleep(3000);
                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            //Thread.Sleep(3000);
                            ho_Grayimage4.Dispose(); ho_Heightimage4.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage4, out ho_Heightimage4, 0);
                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage4, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Fourth Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            //ho_Grayimage4.Dispose(); ho_Heightimage4.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage4, "E:/binnew/11/11/WN1052/3D/灰度图_4.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage4, "E:/binnew/11/11/WN1052/3D/高度图_H5.tif");

                            if (hv_SRadius4 == null)
                                hv_SRadius4 = new HTuple();
                            if (hv_Q4 == null)
                                hv_Q4 = new HTuple();
                            if (hv_mean4 == null)
                                hv_mean4 = new HTuple();
                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                //HOperatorSet.WriteImage(ho_Grayimage4, "bmp", 0, "D:/" + nIndexOfImage.ToString() + ".bmp");
                                //HOperatorSet.WriteImage(ho_Heightimage4, "tiff", 0, "D:/" + nIndexOfImage.ToString() + ".tif");

                                hv_SRadius4.Dispose(); hv_Q4.Dispose(); hv_mean4.Dispose();
                                devThread3 = new Thread(() =>
                                {
                                    try
                                    {

                                        // Call L_C_Compute
                                        L_C_Compute(ho_Grayimage4, ho_Heightimage4, 4, hv_PoseInvert, out hv_SRadius4,
                                        out hv_Q4, out hv_mean4);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 4 " + exc.Message);
                                    }
                                });
                                devThread3.Start();
                            }


                            //**第五段***
                            //Thread.Sleep(3000);
                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            //Thread.Sleep(3000);
                            ho_Grayimage5.Dispose(); ho_Heightimage5.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage5, out ho_Heightimage5,
                                0);

                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage5, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Fifth Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());

                            //ho_Grayimage5.Dispose(); ho_Heightimage5.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage5, "E:/binnew/11/11/WN1052/3D/灰度图_5.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage5, "E:/binnew/11/11/WN1052/3D/高度图_H6.tif");

                            if (hv_SRadius5 == null)
                                hv_SRadius5 = new HTuple();
                            if (hv_Q5 == null)
                                hv_Q5 = new HTuple();
                            if (hv_mean5 == null)
                                hv_mean5 = new HTuple();
                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                //HOperatorSet.WriteImage(ho_Grayimage5, "bmp", 0, "D:/" + nIndexOfImage.ToString() + ".bmp");
                                //HOperatorSet.WriteImage(ho_Heightimage5, "tiff", 0, "D:/" + nIndexOfImage.ToString() + ".tif");

                                hv_SRadius5.Dispose(); hv_Q5.Dispose(); hv_mean5.Dispose();
                                devThread4 = new Thread(() =>
                                {
                                    try
                                    {
                                        // Call L_C_Compute
                                        L_C_Compute(ho_Grayimage5, ho_Heightimage5, 5, hv_PoseInvert, out hv_SRadius5,
                                        out hv_Q5, out hv_mean5);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 5 " + exc.Message);
                                    }
                                });
                                devThread4.Start();
                            }



                            //**第六段***
                            //Thread.Sleep(3000);
                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            //Thread.Sleep(3000);
                            ho_Grayimage6.Dispose(); ho_Heightimage6.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage6, out ho_Heightimage6,
                                0);
                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage6, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Sixth Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            //ho_Grayimage6.Dispose(); ho_Heightimage6.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage6, "E:/binnew/11/11/WN1052/3D/灰度图_6.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage6, "E:/binnew/11/11/WN1052/3D/高度图_H7.tif");
                            if (hv_SRadius6 == null)
                                hv_SRadius6 = new HTuple();
                            if (hv_Q6 == null)
                                hv_Q6 = new HTuple();
                            if (hv_mean6 == null)
                                hv_mean6 = new HTuple();
                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                hv_SRadius6.Dispose(); hv_Q6.Dispose(); hv_mean6.Dispose();
                                devThread5 = new Thread(() =>
                                {
                                    try
                                    {

                                        // Call L_C_Compute
                                        L_C_Compute(ho_Grayimage6, ho_Heightimage6, 6, hv_PoseInvert, out hv_SRadius6,
                                        out hv_Q6, out hv_mean6);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 6 " + exc.Message);
                                    }
                                });
                                devThread5.Start();
                            }


                            //**第七段***
                            //Thread.Sleep(3000);

                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            //Thread.Sleep(3000);
                            ho_Grayimage7.Dispose(); ho_Heightimage7.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage7, out ho_Heightimage7,
                                0);
                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage7, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Seventh Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            //ho_Grayimage7.Dispose(); ho_Heightimage7.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage7, "E:/binnew/11/11/WN1052/3D/灰度图_7.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage7, "E:/binnew/11/11/WN1052/3D/高度图_H8.tif");
                            if (hv_SRadius7 == null)
                                hv_SRadius7 = new HTuple();
                            if (hv_Q7 == null)
                                hv_Q7 = new HTuple();
                            if (hv_mean7 == null)
                                hv_mean7 = new HTuple();
                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                hv_SRadius7.Dispose(); hv_Q7.Dispose(); hv_mean7.Dispose();
                                devThread6 = new Thread(() =>
                                {
                                    try
                                    {
                                        // Call L_C_Compute
                                        L_C_Compute(ho_Grayimage7, ho_Heightimage7, 7, hv_PoseInvert, out hv_SRadius7,
                                        out hv_Q7, out hv_mean7);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 7 " + exc.Message);
                                    }
                                });
                                devThread6.Start();
                            }

                            //**第八段***
                            //Thread.Sleep(3000);
                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            //Thread.Sleep(3000);
                            ho_Grayimage8.Dispose(); ho_Heightimage8.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage8, out ho_Heightimage8,
                                0);
                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage8, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Eighth Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            //ho_Grayimage8.Dispose(); ho_Heightimage8.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage8, "E:/binnew/11/11/WN1052/3D/灰度图_8.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage8, "E:/binnew/11/11/WN1052/3D/高度图_H9.tif");
                            if (hv_SRadius8 == null)
                                hv_SRadius8 = new HTuple();
                            if (hv_Q8 == null)
                                hv_Q8 = new HTuple();
                            if (hv_mean8 == null)
                                hv_mean8 = new HTuple();
                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                hv_SRadius8.Dispose(); hv_Q8.Dispose(); hv_mean8.Dispose();
                                devThread7 = new Thread(() =>
                                {
                                    try
                                    {
                                        L_C_Compute(ho_Grayimage8, ho_Heightimage8, 8, hv_PoseInvert, out hv_SRadius8,
                                        out hv_Q8, out hv_mean8);
                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 8 " + exc.Message);
                                    }
                                });
                                devThread7.Start();
                            }

                            ////**第九段***
                            //Thread.Sleep(3000);

                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            //Thread.Sleep(3000);
                            ho_Grayimage9.Dispose(); ho_Heightimage9.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage9, out ho_Heightimage9,
                                0);
                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage9, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Eighth Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            
                            //ho_Grayimage9.Dispose(); ho_Heightimage9.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage9, "E:/binnew/11/11/WN1052/3D/灰度图_9.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage9, "E:/binnew/11/11/WN1052/3D/高度图_H10.tif");
                            if (hv_SRadius9 == null)
                                hv_SRadius9 = new HTuple();
                            if (hv_Q9 == null)
                                hv_Q9 = new HTuple();
                            if (hv_mean9 == null)
                                hv_mean9 = new HTuple();
                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                hv_SRadius9.Dispose(); hv_Q9.Dispose(); hv_mean9.Dispose();
                                devThread8 = new Thread(() =>
                                {
                                    try
                                    {

                                        // Call L_C_Compute
                                        L_C_Compute(ho_Grayimage9, ho_Heightimage9, 9, hv_PoseInvert, out hv_SRadius9,
                                        out hv_Q9, out hv_mean9);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 9 " + exc.Message);
                                    }
                                });
                                devThread8.Start();
                            }


                            ////**第十段***
                            //Thread.Sleep(3000);
                            HDlHalconSSZN.SR7IFStartMeasure(0, 10000);
                            //Thread.Sleep(3000);
                            ho_Grayimage10.Dispose(); ho_Heightimage10.Dispose();
                            HDlHalconSSZN.SR7IFGetBatchRollData(out ho_Grayimage10, out ho_Heightimage10,
                                0);
                            houtwidth.Dispose(); houtheight.Dispose();
                            HOperatorSet.GetImageSize(ho_Grayimage10, out houtwidth, out houtheight);
                            LogHelper.Info("Silicon", "Nineth Section 3D Measure width " + houtwidth.TupleInt().ToString() + " height " + houtheight.TupleInt().ToString());
                            //ho_Grayimage10.Dispose(); ho_Heightimage10.Dispose();
                            //HOperatorSet.ReadImage(out ho_Grayimage10, "E:/binnew/11/11/WN1052/3D/灰度图_10.bmp");
                            //HOperatorSet.ReadImage(out ho_Heightimage10, "E:/binnew/11/11/WN1052/3D/高度图_H11.tif");
                            if (hv_SRadius10 == null)
                                hv_SRadius10 = new HTuple();
                            if (hv_Q10 == null)
                                hv_Q10 = new HTuple();
                            if (hv_mean10 == null)
                                hv_mean10 = new HTuple();

                            if (houtheight.TupleInt() != 0 && houtwidth.TupleInt() != 0)
                            {
                                hv_SRadius10.Dispose(); hv_Q10.Dispose(); hv_mean10.Dispose();
                                devThread9 = new Thread(() =>
                                {
                                    try
                                    {
                                        // Call L_C_Compute
                                        L_C_Compute(ho_Grayimage10, ho_Heightimage10, 10, hv_PoseInvert, out hv_SRadius10,
                                        out hv_Q10, out hv_mean10);

                                    }
                                    catch (HalconException exc)
                                    {
                                        LogHelper.Error("exception measure 3d 10 " + exc.Message);
                                    }
                                });
                                devThread9.Start();
                            }
                            HDlHalconSSZN.SR7IFCommClose(0);
                        }
                        else
                        {
                            ho_Grayimage1.Dispose(); ho_Heightimage1.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage1, "E:/binnew/11/11/WN1052/3D/灰度图_1.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage1, "E:/binnew/11/11/WN1052/3D/1_高度图.tif");

                            if (hv_SRadius1 == null)
                                hv_SRadius1 = new HTuple();
                            if (hv_Q1 == null)
                                hv_Q1 = new HTuple();
                            if (hv_mean1 == null)
                                hv_mean1 = new HTuple();
                            hv_SRadius1.Dispose(); hv_Q1.Dispose(); hv_mean1.Dispose();
                            devThread = new Thread(() =>
                            {
                                try
                                {  
                                    L_C_Compute(ho_Grayimage1, ho_Heightimage1, 1, hv_PoseInvert, out hv_SRadius1, out hv_Q1, out hv_mean1);

                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d " + exc.Message);
                                }
                            });
                            devThread.Start();

                            ho_Grayimage2.Dispose(); ho_Heightimage2.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage2, "E:/binnew/11/11/WN1052/3D/灰度图_2.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage2, "E:/binnew/11/11/WN1052/3D/高度图_H3.tif");
                            hv_SRadius2.Dispose(); hv_Q2.Dispose(); hv_mean2.Dispose();
                            devThread1 = new Thread(() =>
                            {
                                try
                                {
                                   
                                    // Call L_C_Compute
                                    L_C_Compute(ho_Grayimage2, ho_Heightimage2, 2, hv_PoseInvert, out hv_SRadius2,
                                                out hv_Q2, out hv_mean2);

                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 2 " + exc.Message);
                                }
                            });
                            devThread1.Start();

                            ho_Grayimage3.Dispose(); ho_Heightimage3.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage3, "E:/binnew/11/11/WN1052/3D/灰度图_3.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage3, "E:/binnew/11/11/WN1052/3D/高度图_H4.tif");
                            if (hv_SRadius3 == null)
                                hv_SRadius3 = new HTuple();
                            if (hv_Q3 == null)
                                hv_Q3 = new HTuple();
                            if (hv_mean3 == null)
                                hv_mean3 = new HTuple();
                            hv_SRadius3.Dispose(); hv_Q3.Dispose(); hv_mean3.Dispose();
                            devThread2 = new Thread(() =>
                            {
                                try
                                {
                                    // Call L_C_Compute
                                    L_C_Compute(ho_Grayimage3, ho_Heightimage3, 3, hv_PoseInvert, out hv_SRadius3,
                                                out hv_Q3, out hv_mean3);
                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 2 " + exc.Message);
                                }
                            });
                            devThread2.Start();

                            ho_Grayimage4.Dispose(); ho_Heightimage4.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage4, "E:/binnew/11/11/WN1052/3D/灰度图_4.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage4, "E:/binnew/11/11/WN1052/3D/高度图_H5.tif");
                            if (hv_SRadius4 == null)
                                hv_SRadius4 = new HTuple();
                            if (hv_Q4 == null)
                                hv_Q4 = new HTuple();
                            if (hv_mean4 == null)
                                hv_mean4 = new HTuple();
                            hv_SRadius4.Dispose(); hv_Q4.Dispose(); hv_mean4.Dispose();
                            devThread3 = new Thread(() =>
                            {
                                try
                                {
                                    // Call L_C_Compute
                                    L_C_Compute(ho_Grayimage4, ho_Heightimage4, 4, hv_PoseInvert, out hv_SRadius4,
                                    out hv_Q4, out hv_mean4);
                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 4 " + exc.Message);
                                }
                            });
                            devThread3.Start();

                            ho_Grayimage5.Dispose(); ho_Heightimage5.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage5, "E:/binnew/11/11/WN1052/3D/灰度图_5.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage5, "E:/binnew/11/11/WN1052/3D/高度图_H6.tif");
                            if (hv_SRadius5 == null)
                                hv_SRadius5 = new HTuple();
                            if (hv_Q5 == null)
                                hv_Q5 = new HTuple();
                            if (hv_mean5 == null)
                                hv_mean5 = new HTuple();
                            hv_SRadius5.Dispose(); hv_Q5.Dispose(); hv_mean5.Dispose();
                            devThread4 = new Thread(() =>
                            {
                                try
                                {
                                    // Call L_C_Compute
                                    L_C_Compute(ho_Grayimage5, ho_Heightimage5, 5, hv_PoseInvert, out hv_SRadius5,
                                    out hv_Q5, out hv_mean5);

                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 5 " + exc.Message);
                                }
                            });
                            devThread4.Start();

                            ho_Grayimage6.Dispose(); ho_Heightimage6.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage6, "E:/binnew/11/11/WN1052/3D/灰度图_6.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage6, "E:/binnew/11/11/WN1052/3D/高度图_H7.tif");
                            if (hv_SRadius6 == null)
                                hv_SRadius6 = new HTuple();
                            if (hv_Q6 == null)
                                hv_Q6 = new HTuple();
                            if (hv_mean6 == null)
                                hv_mean6 = new HTuple();
                            hv_SRadius6.Dispose(); hv_Q6.Dispose(); hv_mean6.Dispose();
                            devThread5 = new Thread(() =>
                            {
                                try
                                {

                                    // Call L_C_Compute
                                    L_C_Compute(ho_Grayimage6, ho_Heightimage6, 6, hv_PoseInvert, out hv_SRadius6,
                                    out hv_Q6, out hv_mean6);

                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 6 " + exc.Message);
                                }
                            });
                            devThread5.Start();


                            ho_Grayimage7.Dispose(); ho_Heightimage7.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage7, "E:/binnew/11/11/WN1052/3D/灰度图_7.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage7, "E:/binnew/11/11/WN1052/3D/高度图_H8.tif");
                            if (hv_SRadius7 == null)
                                hv_SRadius7 = new HTuple();
                            if (hv_Q7 == null)
                                hv_Q7 = new HTuple();
                            if (hv_mean7 == null)
                                hv_mean7 = new HTuple();
                            hv_SRadius7.Dispose(); hv_Q7.Dispose(); hv_mean7.Dispose();
                            devThread6 = new Thread(() =>
                            {
                                try
                                {
                                    // Call L_C_Compute
                                    L_C_Compute(ho_Grayimage7, ho_Heightimage7, 7, hv_PoseInvert, out hv_SRadius7,
                                    out hv_Q7, out hv_mean7);

                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 7 " + exc.Message);
                                }
                            });
                            devThread6.Start();

                            ho_Grayimage8.Dispose(); ho_Heightimage8.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage8, "E:/binnew/11/11/WN1052/3D/灰度图_8.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage8, "E:/binnew/11/11/WN1052/3D/高度图_H9.tif");

                            if (hv_SRadius8 == null)
                                hv_SRadius8 = new HTuple();
                            if (hv_Q8 == null)
                                hv_Q8 = new HTuple();
                            if (hv_mean8 == null)
                                hv_mean8 = new HTuple();
                            hv_SRadius8.Dispose(); hv_Q8.Dispose(); hv_mean8.Dispose();
                            devThread7 = new Thread(() =>
                            {
                                try
                                {
                                    L_C_Compute(ho_Grayimage8, ho_Heightimage8, 8, hv_PoseInvert, out hv_SRadius8,
                                    out hv_Q8, out hv_mean8);
                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 8 " + exc.Message);
                                }
                            });
                            devThread7.Start();


                            ho_Grayimage9.Dispose(); ho_Heightimage9.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage9, "E:/binnew/11/11/WN1052/3D/灰度图_9.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage9, "E:/binnew/11/11/WN1052/3D/高度图_H10.tif");

                            if (hv_SRadius9 == null)
                                hv_SRadius9 = new HTuple();
                            if (hv_Q9 == null)
                                hv_Q9 = new HTuple();
                            if (hv_mean9 == null)
                                hv_mean9 = new HTuple();

                            hv_SRadius9.Dispose(); hv_Q9.Dispose(); hv_mean9.Dispose();
                            devThread8 = new Thread(() =>
                            {
                                try
                                {

                                    // Call L_C_Compute
                                    L_C_Compute(ho_Grayimage9, ho_Heightimage9, 9, hv_PoseInvert, out hv_SRadius9,
                                    out hv_Q9, out hv_mean9);

                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 9 " + exc.Message);
                                }
                            });
                            devThread8.Start();

                            ho_Grayimage10.Dispose(); ho_Heightimage10.Dispose();
                            HOperatorSet.ReadImage(out ho_Grayimage10, "E:/binnew/11/11/WN1052/3D/灰度图_10.bmp");
                            HOperatorSet.ReadImage(out ho_Heightimage10, "E:/binnew/11/11/WN1052/3D/高度图_H11.tif");

                            if (hv_SRadius10 == null)
                                hv_SRadius10 = new HTuple();
                            if (hv_Q10 == null)
                                hv_Q10 = new HTuple();
                            if (hv_mean10 == null)
                                hv_mean10 = new HTuple();

                            hv_SRadius10.Dispose(); hv_Q10.Dispose(); hv_mean10.Dispose();
                            devThread9 = new Thread(() =>
                            {
                                try
                                {
                                    // Call L_C_Compute
                                    L_C_Compute(ho_Grayimage10, ho_Heightimage10, 10, hv_PoseInvert, out hv_SRadius10,
                                    out hv_Q10, out hv_mean10);

                                }
                                catch (HalconException exc)
                                {
                                    LogHelper.Error("exception measure 3d 10 " + exc.Message);
                                }
                            });
                            devThread9.Start();
                        }

                        while (true)
                        {
                            if ((devThread != null && devThread.ThreadState != ThreadState.Stopped) || (devThread1 != null && devThread1.ThreadState != ThreadState.Stopped) ||
                                (devThread2 != null && devThread2.ThreadState != ThreadState.Stopped) || (devThread3 != null && devThread3.ThreadState != ThreadState.Stopped )||
                                (devThread4 != null && devThread4.ThreadState != ThreadState.Stopped) || (devThread5 != null && devThread5.ThreadState != ThreadState.Stopped) ||
                                (devThread6 != null && devThread6.ThreadState != ThreadState.Stopped) || (devThread7 != null && devThread7.ThreadState != ThreadState.Stopped) ||
                                (devThread8 != null && devThread8.ThreadState != ThreadState.Stopped) || (devThread9 != null && devThread9.ThreadState != ThreadState.Stopped))
                            {
                                Thread.Sleep(1000);
                                continue;
                            }
                            else
                            {
                                break;
                            }
                            //hv_QQ.Dispose();
                            //using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            //{
                            //    hv_QQ = hv_Q1 + hv_Q2 + hv_Q3 + hv_Q4 + hv_Q5 + hv_Q6 + hv_Q7 + hv_Q8 + hv_Q9 + hv_Q10;
                            //}
                            //if (new HTuple(hv_QQ.TupleEqual(10)) != 0)
                            //{
                            //    break;
                            //}

                        }

                        LogHelper.Info("Silicon", "Measur_3D wait 10 Thread End");

                        
                        //par_join ([ThreadID,ThreadID1,ThreadID2,ThreadID3,ThreadID4,ThreadID5,ThreadID6,ThreadID7,ThreadID8])
                        //**第十一段***
                        //SR7IF_StartMeasure (0, 10000) // 开始批处理
                        //SR7IF_GetBatchRollData (Grayimage11, Heightimage11, 0) // 取图
                        //L_C_Compute (Grayimage11, Heightimage11, 11, SRadius11, Q10, mean10010)
                        ho_Imageconst.Dispose();
                        ho_Imageconst.Dispose(); hv_Name.Dispose(); hv_Coordinate.Dispose(); hv_Length.Dispose(); hv_H.Dispose(); hv_T.Dispose(); hv_Head.Dispose(); hv_Tail.Dispose(); hv_CoordinateHT.Dispose(); hv_Number1.Dispose(); hv_L.Dispose();
                        HX_DEMO(ho_Grayimage1, ho_Grayimage2, ho_Grayimage3, ho_Grayimage4, ho_Grayimage5,
                            ho_Grayimage6, ho_Grayimage7, ho_Grayimage8, ho_Grayimage9, ho_Grayimage10,
                            ho_Heightimage1, ho_Heightimage2, ho_Heightimage3, ho_Heightimage4,
                            ho_Heightimage5, ho_Heightimage6, ho_Heightimage7, ho_Heightimage8,
                            ho_Heightimage9, ho_Heightimage10, out ho_Imageconst, hv_ResultDictHandle,
                            hv_Rate, out hv_Name, out hv_Coordinate, out hv_Length, out hv_H, out hv_T,
                            out hv_Head, out hv_Tail, out hv_CoordinateHT, out hv_Number1, out hv_L);
                        try
                        {
                            hv_Raduiszz.Dispose();
                            hv_Raduiszz = new HTuple();
                            hv_Raduiszz = hv_Raduiszz.TupleConcat(hv_SRadius1, hv_SRadius2, hv_SRadius3, hv_SRadius4, hv_SRadius5, hv_SRadius6, hv_SRadius7, hv_SRadius8, hv_SRadius9, hv_SRadius10);
                        
                            hv_g.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_g = (hv_Raduiszz * 0) + 160;
                            }
                            hv_l.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_l = (hv_Raduiszz * 0) + 120;
                            }
                            hv_Greater.Dispose();
                            HOperatorSet.TupleGreaterElem(hv_Raduiszz, hv_g, out hv_Greater);
                            hv_Less.Dispose();
                            HOperatorSet.TupleLessElem(hv_Raduiszz, hv_l, out hv_Less);
                            hv_Indices1.Dispose();
                            HOperatorSet.TupleFind(hv_Greater, 1, out hv_Indices1);
                            hv_Indices2.Dispose();
                            HOperatorSet.TupleFind(hv_Less, 1, out hv_Indices2);
                            hv_k.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_k = new HTuple();
                                hv_k = hv_k.TupleConcat(hv_Indices1, hv_Indices2);
                            }
                            hv_Reduced.Dispose();
                            HOperatorSet.TupleRemove(hv_Raduiszz, hv_k, out hv_Reduced);
                            hv_MeanRadius1.Dispose();
                            Remove_max_min(hv_Reduced, 50, out hv_MeanRadius1);
                            hv_MeanRadius100.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_MeanRadius100 = hv_MeanRadius1 * 0.984;
                            }
                            hv_first.Dispose(); hv_end.Dispose();
                            tuple_fe(hv_MeanRadius100, out hv_first, out hv_end);
                            hv_Selected.Dispose();
                            HOperatorSet.TupleSelectRange(hv_MeanRadius100, hv_first, hv_end, out hv_Selected);

                       
                            hv_Max.Dispose();
                            HOperatorSet.TupleMax(hv_Selected, out hv_Max);
                            hv_Min.Dispose();
                            HOperatorSet.TupleMin(hv_Selected, out hv_Min);
                            hv_Mean.Dispose();
                            HOperatorSet.TupleMean(hv_Selected, out hv_Mean);
                            hv_DiameterMax.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_DiameterMax = 2 * hv_Max;
                            }
                            hv_DiameterMin.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_DiameterMin = 2 * hv_Min;
                            }
                            hv_DiameterMean.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_DiameterMean = 2 * hv_Mean;
                            }
                        }
                        // catch (Exception) 
                        catch (HalconException HDevExpDefaultException2)
                        {
                            HDevExpDefaultException2.ToHTuple(out hv_Exception);
                            hv_DiameterMax = 0;
                            hv_DiameterMin = 0;
                            hv_DiameterMean = 0;
                            hv_Length = 0;
                        }

                        try
                        {
                            hv_MessageHandle.Dispose();
                            HOperatorSet.CreateMessage(out hv_MessageHandle);
                            HOperatorSet.SetMessageTuple(hv_MessageHandle, "SS", ((((((hv_DiameterMax.TupleConcat(
                                hv_DiameterMin))).TupleConcat(hv_DiameterMean))).TupleConcat(hv_Length))).TupleConcat(
                                hv_H));
                            HOperatorSet.EnqueueMessage(hv_LQueueHandle, hv_MessageHandle, new HTuple(),
                                new HTuple());
                            LogHelper.Info("Silicon", "SSZNTools Enqueue SS Info");
                            HOperatorSet.ClearMessage(hv_MessageHandle);
                        }
                        // catch (Exception) 
                        catch (HalconException HDevExpDefaultException2)
                        {
                            HDevExpDefaultException2.ToHTuple(out hv_Exception);
                        }
                        CGlobalFuncTools.Instance().Set_Twokey_Tuple(hv_ResultDictHandle, "直径", "直径曲线数组", hv_MeanRadius1);
                        hv_IDTime.Dispose();
                        CGlobalFuncTools.Instance().CreateID(out hv_IDTime);
                    }
                    // catch (Exception) 
                    catch (HalconException HDevExpDefaultException1)
                    {
                        HDevExpDefaultException1.ToHTuple(out hv_Exception);
                        LogHelper.Info("Silicon", "Measur_3D wait 10 Exception " + hv_Exception.ToString());
                    }
                


                    ho_EmptyObject.Dispose();
                    ho_Grayimage1.Dispose();
                    ho_Heightimage1.Dispose();
                    ho_Grayimage2.Dispose();
                    ho_Heightimage2.Dispose();
                    ho_Grayimage3.Dispose();
                    ho_Heightimage3.Dispose();
                    ho_Grayimage4.Dispose();
                    ho_Heightimage4.Dispose();
                    ho_Grayimage5.Dispose();
                    ho_Heightimage5.Dispose();
                    ho_Grayimage6.Dispose();
                    ho_Heightimage6.Dispose();
                    ho_Grayimage7.Dispose();
                    ho_Heightimage7.Dispose();
                    ho_Grayimage8.Dispose();
                    ho_Heightimage8.Dispose();
                   
                    ho_Rectangle.Dispose();
                    ho_Rectangle1.Dispose();
                    ho_Rectangle2.Dispose();

                    hv_Rate.Dispose();
                    hv_Seconds1.Dispose();
                    hv_MessageHandle.Dispose();
                    hv_Start.Dispose();
                    hv_Seconds2.Dispose();
                    hv_Name.Dispose();
                    
                    hv_Q1.Dispose();
                    hv_Q2.Dispose();
                    hv_Q3.Dispose();
                    hv_Q4.Dispose();
                    hv_Q5.Dispose();
                    hv_Q6.Dispose();
                    hv_Q7.Dispose();
                    hv_Q8.Dispose();
                    hv_Q9.Dispose();
                    hv_SRadius1.Dispose();
                    hv_mean1.Dispose();
                    hv_ThreadID1.Dispose();
                    hv_SRadius2.Dispose();
                    hv_mean2.Dispose();
                    hv_ThreadID.Dispose();
                    hv_SRadius3.Dispose();
                    hv_mean3.Dispose();
                    hv_ThreadID9.Dispose();
                    hv_SRadius4.Dispose();
                    hv_mean4.Dispose();
                    hv_ThreadID10.Dispose();
                    hv_SRadius5.Dispose();
                    hv_mean5.Dispose();
                    hv_ThreadID4.Dispose();
                    hv_SRadius6.Dispose();
                    hv_mean6.Dispose();
                    hv_ThreadID5.Dispose();
                    hv_SRadius7.Dispose();
                    hv_mean7.Dispose();
                    hv_ThreadID6.Dispose();
                    hv_SRadius8.Dispose();
                    hv_mean8.Dispose();
                    hv_ThreadID7.Dispose();
                    hv_SRadius9.Dispose();
                    hv_ThreadID8.Dispose();
                    hv_QQ.Dispose();
                    hv_Exception.Dispose();
                    hv_Coordinate.Dispose();
                    hv_Length.Dispose();
                    hv_H.Dispose();
                    hv_T.Dispose();
                    hv_Head.Dispose();
                    hv_Tail.Dispose();
                    hv_CoordinateHT.Dispose();
                    hv_Number1.Dispose();
                    hv_L.Dispose();
                    hv_Index2.Dispose();
                    hv_MeanRadius.Dispose();
                    hv_Function.Dispose();
                    hv_Greater.Dispose();
                    hv_Indices2.Dispose();
                    hv_Reduced.Dispose();
                    hv_Indices3.Dispose();
                    hv_Indices4.Dispose();
                    hv_Indices1.Dispose();
                    hv_Reduced1.Dispose();
                    hv_Max.Dispose();
                    hv_Min.Dispose();
                    hv_Mean.Dispose();
                    hv_DiameterMax.Dispose();
                    hv_DiameterMin.Dispose();
                    hv_DiameterMean.Dispose();
                    hv_IDTime.Dispose();
                }
                catch (HalconException ex)
                {

                    ho_EmptyObject.Dispose();
                    ho_Grayimage1.Dispose();
                    ho_Heightimage1.Dispose();
                    ho_Grayimage2.Dispose();
                    ho_Heightimage2.Dispose();
                    ho_Grayimage3.Dispose();
                    ho_Heightimage3.Dispose();
                    ho_Grayimage4.Dispose();
                    ho_Heightimage4.Dispose();
                    ho_Grayimage5.Dispose();
                    ho_Heightimage5.Dispose();
                    ho_Grayimage6.Dispose();
                    ho_Heightimage6.Dispose();
                    ho_Grayimage7.Dispose();
                    ho_Heightimage7.Dispose();
                    ho_Grayimage8.Dispose();
                    ho_Heightimage8.Dispose();
                    
                    ho_Rectangle.Dispose();
                    ho_Rectangle1.Dispose();
                    ho_Rectangle2.Dispose();

                    hv_Rate.Dispose();
                    hv_Seconds1.Dispose();
                    hv_MessageHandle.Dispose();
                    hv_Start.Dispose();
                    hv_Seconds2.Dispose();
                    hv_Name.Dispose();
                   
                    hv_Q1.Dispose();
                    hv_Q2.Dispose();
                    hv_Q3.Dispose();
                    hv_Q4.Dispose();
                    hv_Q5.Dispose();
                    hv_Q6.Dispose();
                    hv_Q7.Dispose();
                    hv_Q8.Dispose();
                    hv_Q9.Dispose();
                    hv_SRadius1.Dispose();
                    hv_mean1.Dispose();
                    hv_ThreadID1.Dispose();
                    hv_SRadius2.Dispose();
                    hv_mean2.Dispose();
                    hv_ThreadID.Dispose();
                    hv_SRadius3.Dispose();
                    hv_mean3.Dispose();
                    hv_ThreadID9.Dispose();
                    hv_SRadius4.Dispose();
                    hv_mean4.Dispose();
                    hv_ThreadID10.Dispose();
                    hv_SRadius5.Dispose();
                    hv_mean5.Dispose();
                    hv_ThreadID4.Dispose();
                    hv_SRadius6.Dispose();
                    hv_mean6.Dispose();
                    hv_ThreadID5.Dispose();
                    hv_SRadius7.Dispose();
                    hv_mean7.Dispose();
                    hv_ThreadID6.Dispose();
                    hv_SRadius8.Dispose();
                    hv_mean8.Dispose();
                    hv_ThreadID7.Dispose();
                    hv_SRadius9.Dispose();
                   
                    hv_ThreadID8.Dispose();
                   
                    hv_QQ.Dispose();
                    hv_Exception.Dispose();
                    hv_Coordinate.Dispose();
                    hv_Length.Dispose();
                    hv_H.Dispose();
                    hv_T.Dispose();
                    hv_Head.Dispose();
                    hv_Tail.Dispose();
                    hv_CoordinateHT.Dispose();
                    hv_Number1.Dispose();
                    hv_L.Dispose();
                    hv_Index2.Dispose();
                    hv_MeanRadius.Dispose();
                    hv_Function.Dispose();
                    hv_Greater.Dispose();
                    hv_Indices2.Dispose();
                    hv_Reduced.Dispose();
                    hv_Indices3.Dispose();
                    hv_Indices4.Dispose();
                    hv_Indices1.Dispose();
                    hv_Reduced1.Dispose();
                    hv_Max.Dispose();
                    hv_Min.Dispose();
                    hv_Mean.Dispose();
                    hv_DiameterMax.Dispose();
                    hv_DiameterMin.Dispose();
                    hv_DiameterMean.Dispose();
                    hv_IDTime.Dispose();

                }


                return;
            }
        }
    }

    
}
