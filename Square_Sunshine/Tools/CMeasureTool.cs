using HalconDotNet;
using SiliconRoundBarCheck.Cameras.SSZN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.ModelBinding;
using YouEyEE.Untils.Log;

namespace SiliconRoundBarCheck.Tools
{
    internal class CMeasureTool
    {
        private static CMeasureTool _instance;



        private CMeasureTool() { }


        public static CMeasureTool Instance()
        {
            if (_instance == null)
            {
                _instance = new CMeasureTool();
            }
            return _instance;
        }

        public void StopMeasure()
        {
            SSZNCamera.Instance().StopMeasure();
        }

        public void StartMeasure()
        {
            SSZNCamera.Instance().StartMeasure();
        }

        public double GetStickLength()
        {
            return SSZNCamera.Instance().StickLength;
        }

        public void CalcRadius(HObject ho_ImageHeight, out HTuple hv_Radius)
        {

            // Local iconic variables 

            HObject ho_Contour = null, ho_ContCircle = null;

            // Local control variables 

            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_Id = new HTuple(), hv_Zero = new HTuple(), hv_i = new HTuple();
            HTuple hv_Rows = new HTuple(), hv_Cols = new HTuple();
            HTuple hv_Y = new HTuple(), hv_Indices = new HTuple();
            HTuple hv_ZeroRatio = new HTuple(), hv_X = new HTuple();
            HTuple hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_R = new HTuple(), hv_StartPhi = new HTuple();
            HTuple hv_EndPhi = new HTuple(), hv_PointOrder = new HTuple();
            HTuple hv_DistanceMin = new HTuple(), hv_DistanceMax = new HTuple();
            HTuple hv_One = new HTuple(), hv_Greater = new HTuple();
            HTuple hv_Indices1 = new HTuple(), hv_Function = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Contour);
            HOperatorSet.GenEmptyObj(out ho_ContCircle);
            hv_Radius = new HTuple();

            hv_Width.Dispose(); hv_Height.Dispose();
            HOperatorSet.GetImageSize(ho_ImageHeight, out hv_Width, out hv_Height);
            hv_Radius.Dispose();
            hv_Radius = new HTuple();
            hv_Id.Dispose();
            hv_Id = new HTuple();
            hv_Zero.Dispose();
            hv_Zero = new HTuple();
            dev_update_off();
            //* for i := Height/2-150 to Height/2+150 by 1
            //for i := 0 to Height by 1
            HTuple end_val8 = hv_Height - 250;
            HTuple step_val8 = 4;
            for (hv_i = 250; hv_i.Continue(end_val8, step_val8); hv_i = hv_i.TupleAdd(step_val8))
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
                if ((int)(new HTuple(hv_ZeroRatio.TupleGreater(0.02))) != 0)
                {
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
                HOperatorSet.FitCircleContourXld(ho_Contour, "geohuber", -1, 0, 0, 3, 2,
                    out hv_Row, out hv_Column, out hv_R, out hv_StartPhi, out hv_EndPhi,
                    out hv_PointOrder);
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
            //endfor

            hv_Function.Dispose();
            //HOperatorSet.CreateFunct1dArray(hv_Radius, out hv_Function);



            ho_Contour.Dispose();
            ho_ContCircle.Dispose();

            hv_Width.Dispose();
            hv_Height.Dispose();
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
            hv_Function.Dispose();

            return;
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
            HOperatorSet.Threshold(ho_ImageZ, out ho_Region, -500, hv_Max);
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

        // Chapter: Develop
        // Short Description: Switch dev_update_pc, dev_update_var and dev_update_window to 'off'. 
        public void dev_update_off()
        {

            // Initialize local and output iconic variables 
            //This procedure sets different update settings to 'off'.
            //This is useful to get the best performance and reduce overhead.
            //
            // dev_update_pc(...); only in hdevelop
            // dev_update_var(...); only in hdevelop
            // dev_update_window(...); only in hdevelop

            return;
        }

        public void Length_measurebyGrayImage(HObject ho_GImage, out HTuple hv_length)
        {
            // Local iconic variables 

            HObject  ho_Region, ho_ConnectedRegions;
            HObject ho_SelectedRegions, ho_SelectedRegion = null;

            // Local control variables 

            HTuple hv_NumRegions = new HTuple();
            HTuple hv_i = new HTuple(), hv_Rows = new HTuple(), hv_Columns = new HTuple();
            HTuple hv_Max = new HTuple(), hv_Min = new HTuple(), hv_c = new HTuple();
            HTuple hv_sum = new HTuple();
            hv_length = new HTuple();

            // Initialize local and output iconic variables 
            
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegion);
           
            ho_Region.Dispose();
            HOperatorSet.Threshold(ho_GImage, out ho_Region, 5, 255);
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_Region, out ho_ConnectedRegions);
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, "area",
                "and", 10000, 99999999);
            
            hv_NumRegions.Dispose();
            HOperatorSet.CountObj(ho_SelectedRegions, out hv_NumRegions);
            HTuple end_val6 = hv_NumRegions;
            HTuple step_val6 = 1;
            bool bFirst = true;
            for (hv_i = 1; hv_i.Continue(end_val6, step_val6); hv_i = hv_i.TupleAdd(step_val6))
            {
                ho_SelectedRegion.Dispose();
                HOperatorSet.SelectObj(ho_SelectedRegions, out ho_SelectedRegion, hv_i);
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_SelectedRegion, out hv_Rows, out hv_Columns);
                hv_Max.Dispose();
                HOperatorSet.TupleMax(hv_Rows, out hv_Max);
                hv_Min.Dispose();
                HOperatorSet.TupleMin(hv_Rows, out hv_Min);
                hv_c.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_c = hv_Max - hv_Min;

                     LogHelper.Info("Silicon","Length_measurebyGrayImage length " + hv_c.D.ToString("0.00"));
                    if (bFirst == true)
                    {
                        hv_sum = hv_c;
                        bFirst = false;
                    }
                    else
                    {
                        hv_sum += hv_c;
                    }
                }

            }


            hv_length = hv_sum;

           

            
            ho_Region.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
            ho_SelectedRegion.Dispose();

            
            hv_NumRegions.Dispose();
            hv_i.Dispose();
            hv_Rows.Dispose();
            hv_Columns.Dispose();
            hv_Max.Dispose();
            hv_Min.Dispose();
            hv_sum.Dispose();
            hv_c.Dispose();
            
        }
        public void length_measure(HObject ho_GImage, out HTuple hv_length)
        {

            // Local iconic variables 

            HObject ho_Region = null, ho_ROI_0 = null, ho_RegionIntersection = null;
            HObject ho_RegionDilation = null, ho_RegionErosion = null, ho_RegionErosion1 = null;
            HObject ho_RegionDilation1 = null, ho_ConnectedRegions = null;
            HObject ho_SelectedRegions = null, ho_Contours = null;

            // Local control variables 

            HTuple hv_Row = new HTuple(), hv_Col = new HTuple();
            HTuple hv_Max = new HTuple(), hv_Min = new HTuple(), hv_i = new HTuple();
            HTuple hv_j = new HTuple(), hv_Exception = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_ROI_0);
            HOperatorSet.GenEmptyObj(out ho_RegionIntersection);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion1);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation1);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_Contours);
            hv_length = new HTuple();
            try
            {
                ho_Region.Dispose();
                HOperatorSet.Threshold(ho_GImage, out ho_Region, 50, 255);

                ho_ROI_0.Dispose();
                HOperatorSet.GenRectangle1(out ho_ROI_0, 38.9045, 642.123, 14967.1, 2635.57);

                ho_RegionIntersection.Dispose();
                HOperatorSet.Intersection(ho_Region, ho_ROI_0, out ho_RegionIntersection);
                ho_RegionDilation.Dispose();
                HOperatorSet.DilationRectangle1(ho_RegionIntersection, out ho_RegionDilation,
                    300, 300);
                ho_RegionErosion.Dispose();
                HOperatorSet.ErosionRectangle1(ho_RegionDilation, out ho_RegionErosion, 300,
                    300);
                ho_RegionErosion1.Dispose();
                HOperatorSet.ErosionRectangle1(ho_RegionErosion, out ho_RegionErosion1, 300,
                    300);
                ho_RegionDilation1.Dispose();
                HOperatorSet.DilationRectangle1(ho_RegionErosion1, out ho_RegionDilation1,
                    300, 300);
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_RegionDilation1, out ho_ConnectedRegions);
                ho_SelectedRegions.Dispose();
                HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, "area",
                    "and", 1000, 999999999999);
                ho_Contours.Dispose();
                HOperatorSet.GenContourRegionXld(ho_SelectedRegions, out ho_Contours, "border");
                hv_Row.Dispose(); hv_Col.Dispose();
                HOperatorSet.GetContourXld(ho_Contours, out hv_Row, out hv_Col);
                hv_Max.Dispose();
                HOperatorSet.TupleMax(hv_Row, out hv_Max);
                hv_Min.Dispose();
                HOperatorSet.TupleMin(hv_Row, out hv_Min);
                hv_i.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_i = hv_Max - hv_Min;
                }

                hv_j.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_j = (hv_i * 0.07312) * 0.96;
                }
                hv_length.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_length = hv_j - 2;
                }
            }
            // catch (Exception) 
            catch (HalconException HDevExpDefaultException1)
            {
                HDevExpDefaultException1.ToHTuple(out hv_Exception);
                hv_length.Dispose();
                hv_length = 900;
            }
            ho_Region.Dispose();
            ho_ROI_0.Dispose();
            ho_RegionIntersection.Dispose();
            ho_RegionDilation.Dispose();
            ho_RegionErosion.Dispose();
            ho_RegionErosion1.Dispose();
            ho_RegionDilation1.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
            ho_Contours.Dispose();

            hv_Row.Dispose();
            hv_Col.Dispose();
            hv_Max.Dispose();
            hv_Min.Dispose();
            hv_i.Dispose();
            hv_j.Dispose();
            hv_Exception.Dispose();

            return;
        }

        public void L_C_Compute(HObject ho_Grayimage, HObject ho_Heightimage, out HTuple hv_Mean,
       out HTuple hv_length)
        {

            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_Regions = null, ho_RegionOpening = null;
            HObject ho_RegionClosing = null, ho_ConnectedRegions = null;
            HObject ho_SelectedRegions = null, ho_RegionTrans = null, ho_ImageReduced = null;
            HObject ho_ImagePart = null, ho_HeightimageT = null, ho_x = null;
            HObject ho_y = null, ho_z = null;

            // Local control variables 

            HTuple hv_ObjectModel3D = new HTuple(), hv_Exception = new HTuple();
            HTuple hv_Max = new HTuple(), hv_Min = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_RegionClosing);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_RegionTrans);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_ImagePart);
            HOperatorSet.GenEmptyObj(out ho_HeightimageT);
            HOperatorSet.GenEmptyObj(out ho_x);
            HOperatorSet.GenEmptyObj(out ho_y);
            HOperatorSet.GenEmptyObj(out ho_z);
            hv_Mean = new HTuple();
            hv_length = new HTuple();
            try
            {

                ho_Regions.Dispose();
                HOperatorSet.Threshold(ho_Grayimage, out ho_Regions, 3, 255);
                ho_RegionOpening.Dispose();
                HOperatorSet.OpeningRectangle1(ho_Regions, out ho_RegionOpening, 1, 200);
                ho_RegionClosing.Dispose();
                HOperatorSet.ClosingRectangle1(ho_RegionOpening, out ho_RegionClosing, 20,
                    1);
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_RegionClosing, out ho_ConnectedRegions);
                ho_SelectedRegions.Dispose();
                HOperatorSet.SelectShapeStd(ho_ConnectedRegions, out ho_SelectedRegions, "max_area",
                    70);
                ho_RegionTrans.Dispose();
                HOperatorSet.ShapeTrans(ho_SelectedRegions, out ho_RegionTrans, "rectangle1");


                ho_ImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_Heightimage, ho_RegionTrans, out ho_ImageReduced
                    );
                int nCount =  ho_ImageReduced.CountObj();
                if (nCount > 0)
                {
                    ho_ImagePart.Dispose();
                    HOperatorSet.CropDomain(ho_ImageReduced, out ho_ImagePart);
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConvertImageType(ho_ImagePart, out ExpTmpOutVar_0, "real");
                        ho_ImagePart.Dispose();
                        ho_ImagePart = ExpTmpOutVar_0;
                    }
                    ho_HeightimageT.Dispose();
                    HOperatorSet.ScaleImage(ho_ImagePart, out ho_HeightimageT, 0.00001, 0);

                    ho_x.Dispose(); ho_y.Dispose(); ho_z.Dispose(); hv_ObjectModel3D.Dispose();
                    Creat_XYZ_From_sszn(ho_HeightimageT, out ho_x, out ho_y, out ho_z, 0.09, out hv_ObjectModel3D);
                    try
                    {
                        hv_Mean.Dispose();
                        CalcRadius(ho_z, out hv_Mean);
                    }
                    catch (HalconException HDevExpDefaultException2)
                    {
                        HDevExpDefaultException2.ToHTuple(out hv_Exception);
                        hv_Mean.Dispose();
                        hv_Mean = new HTuple();
                        hv_Mean[0] = 150;
                        hv_Mean[1] = 150.78;
                        hv_Mean[2] = 149.6;
                    }
                    {
                        HTuple ExpTmpOutVar_0;
                        HOperatorSet.TupleMean(hv_Mean, out ExpTmpOutVar_0);
                        //hv_Mean.Dispose();
                        //hv_Mean = ExpTmpOutVar_0;
                    }
                    //hv_Max.Dispose();
                    //HOperatorSet.TupleMax(hv_Mean, out hv_Max);
                    //hv_Min.Dispose();
                    //HOperatorSet.TupleMin(hv_Mean, out hv_Min);
                    hv_length.Dispose();

                    Length_measurebyGrayImage(ho_Grayimage, out hv_length);
                    //length_measure(ho_Grayimage, out hv_length);
                }
                

            }
            // catch (Exception) 
            catch (HalconException HDevExpDefaultException1)
            {
                HDevExpDefaultException1.ToHTuple(out hv_Exception);
                hv_Mean.Dispose();
                hv_Mean = 0;
                hv_length.Dispose();
                hv_length = 0;

                 LogHelper.Info("Silicon","SSZN Camera Exception ");
                //FormMain.formMainF.showMessageDelegate.Invoke("SSZN Measure Exception, SSZN Camera 配置错误！请查看是否有灰度图！ ", (int)FormMain.emMSGTYPE.EM_SSZNCAMERA_EXCEPTION);
            }
            ho_Regions.Dispose();
            ho_RegionOpening.Dispose();
            ho_RegionClosing.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
            ho_RegionTrans.Dispose();
            ho_ImageReduced.Dispose();
            ho_ImagePart.Dispose();
            ho_HeightimageT.Dispose();
            ho_x.Dispose();
            ho_y.Dispose();
            ho_z.Dispose();

            hv_ObjectModel3D.Dispose();
            hv_Exception.Dispose();
            hv_Max.Dispose();
            hv_Min.Dispose();

            return;
        }

    }
}
