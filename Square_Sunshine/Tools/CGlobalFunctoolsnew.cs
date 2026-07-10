using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using Newtonsoft.Json;
using SquareSiliconStickCheck.Data;
using SquareSiliconStickCheck.Parameters;

namespace SiliconRoundBarCheck.Tools
{
    public class CGlobalFunctoolsnew
    {

        private static CGlobalFunctoolsnew _instance;

        public static CGlobalFunctoolsnew Instance()
        {
            if (_instance == null)
            {
                _instance = new CGlobalFunctoolsnew();
            }
            return _instance;
        }
        private CGlobalFunctoolsnew() { }


        public void checkLength(HObject ho_ImageTop, HObject ho_ImageLeft, HObject ho_ImageRight,
      HObject ho_ImageDown, HTuple hv_LeftTDistance, HTuple hv_LeftDDistance, HTuple hv_RightTDistance,
      HTuple hv_RightDDistance, HTuple hv_DownDistance, HTuple hv_LeftRightDistance, string strJinbian)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_ImageOutLeft = null, ho_ImageOutRight = null;
            HObject ho_ImageOutTop = null, ho_ImageOutDown = null, ho_ImagePart;
            HObject ho_ImagePartReal, ho_ImagePartLeft, ho_ImagePartLeftNew;
            HObject ho_RegionLeft, ho_ImageReducedLeft, ho_RegionErosion;
            HObject ho_ImagePartRight, ho_ImagePartRightNew, ho_RegionRight;
            HObject ho_ImageReducedRight, ho_ImagePartTop, ho_ImagePartTopNew;
            HObject ho_RegionTop, ho_ImageReducedTop, ho_ImagePartDown;
            HObject ho_ImagePartDownNew, ho_RegionDown, ho_ImageReducedDown;
            HObject ho_Bx, ho_By, ho_Bz, ho_Intersectiont = null, ho_UnionContourst = null;
            HObject ho_ObjectSelectedt = null, ho_ContoursSplitt = null;
            HObject ho_SelectedContourst = null, ho_contournewmid = null;
            HObject ho_Intersectionl = null, ho_UnionContoursl = null, ho_ObjectSelectedl = null;
            HObject ho_ContoursSplitl = null, ho_SelectedContoursl = null;
            HObject ho_Intersectionr = null, ho_UnionContoursr = null, ho_ObjectSelectedr = null;
            HObject ho_ContoursSplitr = null, ho_SelectedContoursr = null;
            HObject ho_Intersectiond = null, ho_UnionContoursd = null, ho_ObjectSelectedd = null;
            HObject ho_ContoursSplitd = null, ho_SelectedContoursd = null;
            HObject ho_contourleft = null, ho_contourmidt = null, ho_contourrigt = null;
            HObject ho_contourlefr = null, ho_contourmidr = null, ho_contourrigr = null;
            HObject ho_contourlefl = null, ho_contourmidl = null, ho_contourrigl = null;
            HObject ho_contourlefd = null, ho_contourmidd = null, ho_contourrigd = null;
            HObject ho_contourtmp = null, ho_SelectedContourslPre = null;
            HObject ho_SelectedContourslfinal = null, ho_SelectedContourslnewpre = null;
            HObject ho_contourtoplnew = null, ho_contourmidlnew = null;
            HObject ho_contourdowlnew = null, ho_SelectedContourslprenew = null;
            HObject ho_SelectedContourslnew = null, ho_SelectedContoursrPre = null;
            HObject ho_SelectedContoursrfinal = null, ho_SelectedContoursrnewpre = null;
            HObject ho_contourtoprnew = null, ho_contourmidrnew = null;
            HObject ho_contourdowrnew = null, ho_SelectedContoursrprenew = null;
            HObject ho_SelectedContoursrnew = null, ho_SelectedContoursdPre = null;
            HObject ho_contourmiddnew = null, ho_SelectedContoursdnewpre = null;
            HObject ho_contourtopdnew = null, ho_contourdowdnew = null;
            HObject ho_SelectedContoursdnewnpre = null, ho_SelectedContoursdprenew = null;
            HObject ho_SelectedContoursdnew = null, ho_SelectedContoursrnewnpre = null;

            // Local control variables 

            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_Instructions = new HTuple(), hv_WindowHandle = new HTuple();
            HTuple hv_Message = new HTuple(), hv_r = new HTuple();
            HTuple hv_ParameterValues = new HTuple(), hv_Status = new HTuple();
            HTuple hv_yInterval = new HTuple(), hv_ObjectModel3DBLeft = new HTuple();
            HTuple hv_ObjectModel3DBLeftConnected = new HTuple(), hv_ObjectModel3DBLeftNew = new HTuple();
            HTuple hv_ObjectModel3DBRight = new HTuple(), hv_ObjectModel3DBRightConnected = new HTuple();
            HTuple hv_ObjectModel3DBRightNew = new HTuple(), hv_ObjectModel3DBTop = new HTuple();
            HTuple hv_ObjectModel3DTopConnected = new HTuple(), hv_ObjectModel3DBTopNew = new HTuple();
            HTuple hv_ObjectModel3DBDown = new HTuple(), hv_ObjectModel3DBDownConnected = new HTuple();
            HTuple hv_ObjectModel3DBDownNew = new HTuple(), hv_PoseOut = new HTuple();
            HTuple hv_SampledObjectModel3DTop = new HTuple(), hv_SampledObjectModel3DDown = new HTuple();
            HTuple hv_SampledObjectModel3DLeft = new HTuple(), hv_SampledObjectModel3DRight = new HTuple();
            HTuple hv_Surface3DDefaultT = new HTuple(), hv_Info = new HTuple();
            HTuple hv_Surface3DDefaultD = new HTuple(), hv_Surface3DDefaultL = new HTuple();
            HTuple hv_Surface3DDefaultR = new HTuple(), hv_CenterPointT = new HTuple();
            HTuple hv_Radius = new HTuple(), hv_CenterPointD = new HTuple();
            HTuple hv_CenterPointL = new HTuple(), hv_CenterPointR = new HTuple();
            HTuple hv_raduis = new HTuple(), hv_ObjectModel3DIntersections = new HTuple();
            HTuple hv_DisparityProfileWidth = new HTuple(), hv_DisparityProfileHeight = new HTuple();
            HTuple hv_WindowEnlargement = new HTuple(), hv_WindowWidth = new HTuple();
            HTuple hv_WindowHeight = new HTuple(), hv_VisualizationPlaneSize = new HTuple();
            HTuple hv_PoseT = new HTuple(), hv_PoseD = new HTuple();
            HTuple hv_PoseL = new HTuple(), hv_PoseR = new HTuple();
            HTuple hv_IntersectionPlane1 = new HTuple(), hv_IntersectionPlane2 = new HTuple();
            HTuple hv_IntersectionPlane3 = new HTuple(), hv_IntersectionPlane4 = new HTuple();
            HTuple hv_IntersectionPlane5 = new HTuple(), hv_LeftIndex = new HTuple();
            HTuple hv_RightIndex = new HTuple(), hv_TopIndex = new HTuple();
            HTuple hv_DownIndex = new HTuple(), hv_resultcon = new HTuple();
            HTuple hv_n = new HTuple(), hv_PoseT1 = new HTuple(), hv_PoseR1 = new HTuple();
            HTuple hv_PoseD1 = new HTuple(), hv_PoseL1 = new HTuple();
            HTuple hv_ObjectModel3DIntersectiont = new HTuple(), hv_numbercount = new HTuple();
            HTuple hv_lengthnum = new HTuple(), hv_Length = new HTuple();
            HTuple hv_Max = new HTuple(), hv_Indices = new HTuple();
            HTuple hv_Numberct = new HTuple(), hv_Row = new HTuple();
            HTuple hv_Col = new HTuple(), hv_Colmin = new HTuple();
            HTuple hv_Colmax = new HTuple(), hv_ObjectModel3DIntersectionl = new HTuple();
            HTuple hv_Lengthl = new HTuple(), hv_Maxl = new HTuple();
            HTuple hv_Numbercl = new HTuple(), hv_ObjectModel3DIntersectionr = new HTuple();
            HTuple hv_Lengthr = new HTuple(), hv_Maxr = new HTuple();
            HTuple hv_Numbercr = new HTuple(), hv_ObjectModel3DIntersectiond = new HTuple();
            HTuple hv_Lengthd = new HTuple(), hv_Maxd = new HTuple();
            HTuple hv_Numbercd = new HTuple(), hv_Left3DX = new HTuple();
            HTuple hv_Right3DX = new HTuple(), hv_Down3DY = new HTuple();
            HTuple hv_DistancesRT = new HTuple(), hv_DistancesLT = new HTuple();
            HTuple hv_DistancesRD = new HTuple(), hv_DistancesLD = new HTuple();
            HTuple hv_DistancesTD = new HTuple(), hv_DistancesLR = new HTuple();
            HTuple hv_DistancesTopDiag = new HTuple(), hv_DistancesTopLeftDiag = new HTuple();
            HTuple hv_DistancesTopRightDiag = new HTuple(), hv_DistancesDownDiag = new HTuple();
            HTuple hv_DistancesDownLeftDiag = new HTuple(), hv_DistancesDownRightDiag = new HTuple();
            HTuple hv_DistancesLeftDiag = new HTuple(), hv_DistancesLeftLeftDiag = new HTuple();
            HTuple hv_DistancesLeftRightDiag = new HTuple(), hv_DistancesRightDiag = new HTuple();
            HTuple hv_DistancesRightLeftDiag = new HTuple(), hv_DistancesRightRightDiag = new HTuple();
            HTuple hv_Y = new HTuple(), hv_MaxY = new HTuple(), hv_MinY = new HTuple();
            HTuple hv_LengthY = new HTuple(), hv_NumLengthY = new HTuple();
            HTuple hv_lengthofY = new HTuple(), hv_wlength = new HTuple();
            HTuple hv_resultcon1 = new HTuple(), hv_resultcon2 = new HTuple();
            HTuple hv_resultcon3 = new HTuple(), hv_resultcon4 = new HTuple();
            HTuple hv_Rowsleft = new HTuple(), hv_Colsleft = new HTuple();
            HTuple hv_Rowsrigt = new HTuple(), hv_Colsrigt = new HTuple();
            HTuple hv_Rowsmidt = new HTuple(), hv_Colsmidt = new HTuple();
            HTuple hv_Minleft = new HTuple(), hv_Maxleft = new HTuple();
            HTuple hv_Minrigt = new HTuple(), hv_Maxrigt = new HTuple();
            HTuple hv_RowBeginleft = new HTuple(), hv_ColBeginleft = new HTuple();
            HTuple hv_RowEndleft = new HTuple(), hv_ColEndleft = new HTuple();
            HTuple hv_Nrleft = new HTuple(), hv_Ncleft = new HTuple();
            HTuple hv_Distleft = new HTuple(), hv_RowBeginrigt = new HTuple();
            HTuple hv_ColBeginrigt = new HTuple(), hv_RowEndrigt = new HTuple();
            HTuple hv_ColEndrigt = new HTuple(), hv_Nrrigt = new HTuple();
            HTuple hv_Ncrigt = new HTuple(), hv_Distrigt = new HTuple();
            HTuple hv_lengthleftcontour = new HTuple(), hv_lengthleflined = new HTuple();
            HTuple hv_Abslengthleftcontour = new HTuple(), hv_Abslengthleflined = new HTuple();
            HTuple hv_countofCols = new HTuple(), hv_Index = new HTuple();
            HTuple hv_Rowsnewleft = new HTuple(), hv_Colsnewleft = new HTuple();
            HTuple hv_Indexx = new HTuple(), hv_Index1 = new HTuple();
            HTuple hv_Rowsnewmidt = new HTuple(), hv_Colsnewmidt = new HTuple();
            HTuple hv_countofRowst = new HTuple(), hv_RowBeginmidt = new HTuple();
            HTuple hv_ColBeginmidt = new HTuple(), hv_RowEndmidt = new HTuple();
            HTuple hv_ColEndmidt = new HTuple(), hv_Nrmidt = new HTuple();
            HTuple hv_Ncmidt = new HTuple(), hv_Distmidt = new HTuple();
            HTuple hv_lengthrigtcontour = new HTuple(), hv_lengthriglined = new HTuple();
            HTuple hv_Abslengthrigtcontour = new HTuple(), hv_Abslengthriglined = new HTuple();
            HTuple hv_Rowsnewrigt = new HTuple(), hv_Colsnewrigt = new HTuple();
            HTuple hv_Rowslefd = new HTuple(), hv_Colslefd = new HTuple();
            HTuple hv_Rowsrigd = new HTuple(), hv_Colsrigd = new HTuple();
            HTuple hv_Rowsmidd = new HTuple(), hv_Colsmidd = new HTuple();
            HTuple hv_Minlefd = new HTuple(), hv_Maxlefd = new HTuple();
            HTuple hv_Minrigd = new HTuple(), hv_Maxrigd = new HTuple();
            HTuple hv_RowBeginlefd = new HTuple(), hv_ColBeginlefd = new HTuple();
            HTuple hv_RowEndlefd = new HTuple(), hv_ColEndlefd = new HTuple();
            HTuple hv_Nrlefd = new HTuple(), hv_Nclefd = new HTuple();
            HTuple hv_Distlefd = new HTuple(), hv_RowBeginrigd = new HTuple();
            HTuple hv_ColBeginrigd = new HTuple(), hv_RowEndrigd = new HTuple();
            HTuple hv_ColEndrigd = new HTuple(), hv_Nrrigd = new HTuple();
            HTuple hv_Ncrigd = new HTuple(), hv_Distrigd = new HTuple();
            HTuple hv_lengthlefdcontour = new HTuple(), hv_Abslengthlefdcontour = new HTuple();
            HTuple hv_Rowsnewlefd = new HTuple(), hv_Colsnewlefd = new HTuple();
            HTuple hv_Rowsnewmidd = new HTuple(), hv_Colsnewmidd = new HTuple();
            HTuple hv_countofRowsd = new HTuple(), hv_RowBeginmidd = new HTuple();
            HTuple hv_ColBeginmidd = new HTuple(), hv_RowEndmidd = new HTuple();
            HTuple hv_ColEndmidd = new HTuple(), hv_Nrmidd = new HTuple();
            HTuple hv_Ncmidd = new HTuple(), hv_Distmidd = new HTuple();
            HTuple hv_lengthrigdcontour = new HTuple(), hv_Abslengthrigdcontour = new HTuple();
            HTuple hv_Rowsnewrigd = new HTuple(), hv_Colsnewrigd = new HTuple();
            HTuple hv_Rowslefl = new HTuple(), hv_Colslefl = new HTuple();
            HTuple hv_Rowsrigl = new HTuple(), hv_Colsrigl = new HTuple();
            HTuple hv_Rowsmidl = new HTuple(), hv_Colsmidl = new HTuple();
            HTuple hv_Minlefl = new HTuple(), hv_Maxlefl = new HTuple();
            HTuple hv_Minrigl = new HTuple(), hv_Maxrigl = new HTuple();
            HTuple hv_RowBeginlefl = new HTuple(), hv_ColBeginlefl = new HTuple();
            HTuple hv_RowEndlefl = new HTuple(), hv_ColEndlefl = new HTuple();
            HTuple hv_Nrlefl = new HTuple(), hv_Nclefl = new HTuple();
            HTuple hv_Distlefl = new HTuple(), hv_RowBeginrigl = new HTuple();
            HTuple hv_ColBeginrigl = new HTuple(), hv_RowEndrigl = new HTuple();
            HTuple hv_ColEndrigl = new HTuple(), hv_Nrrigl = new HTuple();
            HTuple hv_Ncrigl = new HTuple(), hv_Distrigl = new HTuple();
            HTuple hv_lengthleflcontour = new HTuple(), hv_Abslengthleflcontour = new HTuple();
            HTuple hv_Rowsnewlefl = new HTuple(), hv_Colsnewlefl = new HTuple();
            HTuple hv_Rowsnewmidl = new HTuple(), hv_Colsnewmidl = new HTuple();
            HTuple hv_countofRowsl = new HTuple(), hv_Rowsnewrigl = new HTuple();
            HTuple hv_Colsnewrigl = new HTuple(), hv_RowBeginmidl = new HTuple();
            HTuple hv_ColBeginmidl = new HTuple(), hv_RowEndmidl = new HTuple();
            HTuple hv_ColEndmidl = new HTuple(), hv_Nrmidl = new HTuple();
            HTuple hv_Ncmidl = new HTuple(), hv_Distmidl = new HTuple();
            HTuple hv_Rowslefr = new HTuple(), hv_Colslefr = new HTuple();
            HTuple hv_Rowsrigr = new HTuple(), hv_Colsrigr = new HTuple();
            HTuple hv_Rowsmidr = new HTuple(), hv_Colsmidr = new HTuple();
            HTuple hv_Minlefr = new HTuple(), hv_Maxlefr = new HTuple();
            HTuple hv_Minrigr = new HTuple(), hv_Maxrigr = new HTuple();
            HTuple hv_RowBeginlefr = new HTuple(), hv_ColBeginlefr = new HTuple();
            HTuple hv_RowEndlefr = new HTuple(), hv_ColEndlefr = new HTuple();
            HTuple hv_Nrlefr = new HTuple(), hv_Nclefr = new HTuple();
            HTuple hv_Distlefr = new HTuple(), hv_RowBeginrigr = new HTuple();
            HTuple hv_ColBeginrigr = new HTuple(), hv_RowEndrigr = new HTuple();
            HTuple hv_ColEndrigr = new HTuple(), hv_Nrrigr = new HTuple();
            HTuple hv_Ncrigr = new HTuple(), hv_Distrigr = new HTuple();
            HTuple hv_lengthlefrcontour = new HTuple(), hv_Abslengthlefrcontour = new HTuple();
            HTuple hv_Rowsnewlefr = new HTuple(), hv_Colsnewlefr = new HTuple();
            HTuple hv_Rowsnewmidr = new HTuple(), hv_Colsnewmidr = new HTuple();
            HTuple hv_countofRowsr = new HTuple(), hv_RowBeginmidr = new HTuple();
            HTuple hv_ColBeginmidr = new HTuple(), hv_RowEndmidr = new HTuple();
            HTuple hv_ColEndmidr = new HTuple(), hv_Nrmidr = new HTuple();
            HTuple hv_Ncmidr = new HTuple(), hv_Distmidr = new HTuple();
            HTuple hv_lengthrigrcontour = new HTuple(), hv_Abslengthrigrcontour = new HTuple();
            HTuple hv_countofColr = new HTuple(), hv_Rowsnewrigr = new HTuple();
            HTuple hv_Colsnewrigr = new HTuple(), hv_Minl = new HTuple();
            HTuple hv_Minr = new HTuple(), hv_top_diag = new HTuple();
            HTuple hv_down_diag = new HTuple(), hv_left_diag = new HTuple();
            HTuple hv_right_diag = new HTuple(), hv_RowIntersectionlpt = new HTuple();
            HTuple hv_ColIntersectionlpt = new HTuple(), hv_IsOverlapping = new HTuple();
            HTuple hv_RowIntersectiontpt = new HTuple(), hv_ColIntersectiontpt = new HTuple();
            HTuple hv_RowIntersectiondpt = new HTuple(), hv_ColIntersectiondpt = new HTuple();
            HTuple hv_RowIntersectionrpt = new HTuple(), hv_ColIntersectionrpt = new HTuple();
            HTuple hv_dis_topLeftDiag = new HTuple(), hv_dis_topRightDiag = new HTuple();
            HTuple hv_dis_downLeftDiag = new HTuple(), hv_dis_downRightDiag = new HTuple();
            HTuple hv_dis_leftLeftDiag = new HTuple(), hv_dis_leftRightDiag = new HTuple();
            HTuple hv_dis_rightLeftDiag = new HTuple(), hv_dis_rightRightDiag = new HTuple();
            HTuple hv_Angleradttl = new HTuple(), hv_Degtl = new HTuple();
            HTuple hv_DegtlAbs = new HTuple(), hv_newttl = new HTuple();
            HTuple hv_verllength = new HTuple(), hv_distop_verlength = new HTuple();
            HTuple hv_Angleradtr = new HTuple(), hv_Degtr = new HTuple();
            HTuple hv_DegtrAbs = new HTuple(), hv_Angleraddtl = new HTuple();
            HTuple hv_Degdl = new HTuple(), hv_DegdlAbs = new HTuple();
            HTuple hv_verldlength = new HTuple(), hv_disdown_verlength = new HTuple();
            HTuple hv_Angleraddtr = new HTuple(), hv_DegdrAbs = new HTuple();
            HTuple hv_Angleradd = new HTuple(), hv_Degtd = new HTuple();
            HTuple hv_DegtdAbs = new HTuple(), hv_Angleraddltl = new HTuple();
            HTuple hv_Degldl = new HTuple(), hv_DegldlAbs = new HTuple();
            HTuple hv_newltl = new HTuple(), hv_verlllength = new HTuple();
            HTuple hv_disleft_verlength = new HTuple(), hv_Angleraddrtl = new HTuple();
            HTuple hv_Degrdl = new HTuple(), hv_newrtr = new HTuple();
            HTuple hv_verrdlength = new HTuple(), hv_disright_verlength = new HTuple();
            HTuple hv_RowsMidt = new HTuple(), hv_ColsMidt = new HTuple();
            HTuple hv_RowsMidd = new HTuple(), hv_ColsMidd = new HTuple();
            HTuple hv_RowsMidl = new HTuple(), hv_ColsMidl = new HTuple();
            HTuple hv_RowsMidr = new HTuple(), hv_ColsMidr = new HTuple();
            HTuple hv_meanRowst = new HTuple(), hv_meanColst = new HTuple();
            HTuple hv_meanRowsd = new HTuple(), hv_meanColsd = new HTuple();
            HTuple hv_meanRowsl = new HTuple(), hv_meanColsl = new HTuple();
            HTuple hv_meanRowsr = new HTuple(), hv_meanColsr = new HTuple();
            HTuple hv_minColsd = new HTuple(), hv_maxColsd = new HTuple();
            HTuple hv_minColst = new HTuple(), hv_maxColst = new HTuple();
            HTuple hv_minColsl = new HTuple(), hv_maxColsl = new HTuple();
            HTuple hv_minColsr = new HTuple(), hv_maxColsr = new HTuple();
            HTuple hv_HomMat2DIdentity = new HTuple(), hv_Areal = new HTuple();
            HTuple hv_Rowl = new HTuple(), hv_Columnl = new HTuple();
            HTuple hv_PointOrderl = new HTuple(), hv_HomMatTransPreL = new HTuple();
            HTuple hv_RowPrel = new HTuple(), hv_ColumnPrel = new HTuple();
            HTuple hv_HomMat2DLRotate = new HTuple(), hv_HomMat2DScale = new HTuple();
            HTuple hv_meanMidColnew = new HTuple(), hv_RowBeginmidtnew = new HTuple();
            HTuple hv_ColBeginmidtnew = new HTuple(), hv_RowEndmidtnew = new HTuple();
            HTuple hv_ColEndmidtnew = new HTuple(), hv_Rowminmidt = new HTuple();
            HTuple hv_SubRow = new HTuple(), hv_RowsTopl = new HTuple();
            HTuple hv_ColsTopl = new HTuple(), hv_RowsDowl = new HTuple();
            HTuple hv_ColsDowl = new HTuple(), hv_meanTopl = new HTuple();
            HTuple hv_meanDowl = new HTuple(), hv_RowBegintopl = new HTuple();
            HTuple hv_ColBegintopl = new HTuple(), hv_RowEndtopl = new HTuple();
            HTuple hv_ColEndtopl = new HTuple(), hv_angleltx = new HTuple();
            HTuple hv_Degltx = new HTuple(), hv_AbgDegltx = new HTuple();
            HTuple hv_Abgangleltx = new HTuple(), hv_DegSubL = new HTuple();
            HTuple hv_MeanMidlx = new HTuple(), hv_Sintl = new HTuple();
            HTuple hv_Costl = new HTuple(), hv_AbsSintl = new HTuple();
            HTuple hv_AbsCostl = new HTuple(), hv_AbsmeanRowsl = new HTuple();
            HTuple hv_DistanceXPre = new HTuple(), hv_DistanceLPre = new HTuple();
            HTuple hv_RowIntersectiontl = new HTuple(), hv_ColIntersectiontl = new HTuple();
            HTuple hv_RowIntersectiont = new HTuple(), hv_ColIntersectiont = new HTuple();
            HTuple hv_DistanceLT = new HTuple(), hv_RowsMidnewt = new HTuple();
            HTuple hv_ColsMidnewt = new HTuple(), hv_MeanMidnewColt = new HTuple();
            HTuple hv_MeanMidnewRowy = new HTuple(), hv_RowIntersectiontd = new HTuple();
            HTuple hv_ColIntersectiontd = new HTuple(), hv_DistanceDL = new HTuple();
            HTuple hv_Arear = new HTuple(), hv_Rowr = new HTuple();
            HTuple hv_Columnr = new HTuple(), hv_PointOrderr = new HTuple();
            HTuple hv_HomMatTransPreR = new HTuple(), hv_RowPrer = new HTuple();
            HTuple hv_ColumnPrer = new HTuple(), hv_HomMat2DRRotate = new HTuple();
            HTuple hv_RowBeginmidnewr = new HTuple(), hv_ColBeginmidnewr = new HTuple();
            HTuple hv_RowEndmidnewr = new HTuple(), hv_ColEndmidnewr = new HTuple();
            HTuple hv_Rowmidrmin = new HTuple(), hv_Rowsrmidnew = new HTuple();
            HTuple hv_Colsrmidnew = new HTuple(), hv_meanmidColsnew = new HTuple();
            HTuple hv_RowsTopr = new HTuple(), hv_ColsTopr = new HTuple();
            HTuple hv_RowsDowr = new HTuple(), hv_ColsDowr = new HTuple();
            HTuple hv_meanTopr = new HTuple(), hv_meanDowr = new HTuple();
            HTuple hv_RowBegintopr = new HTuple(), hv_ColBegintopr = new HTuple();
            HTuple hv_RowEndtopr = new HTuple(), hv_ColEndtopr = new HTuple();
            HTuple hv_Degrtr = new HTuple(), hv_DegrtrAbs = new HTuple();
            HTuple hv_DegSubR = new HTuple(), hv_MeanMidrx = new HTuple();
            HTuple hv_AngleradtrAbs = new HTuple(), hv_Sintr = new HTuple();
            HTuple hv_Costr = new HTuple(), hv_AbsSintr = new HTuple();
            HTuple hv_AbsCostr = new HTuple(), hv_AbsmeanRowsr = new HTuple();
            HTuple hv_DistanceRPre = new HTuple(), hv_RowIntersectiontr = new HTuple();
            HTuple hv_ColIntersectiontr = new HTuple(), hv_DistanceRT = new HTuple();
            HTuple hv_DistanceRD = new HTuple(), hv_Aread = new HTuple();
            HTuple hv_Rowd = new HTuple(), hv_Columnd = new HTuple();
            HTuple hv_HomMatTransPreD = new HTuple(), hv_RowPred = new HTuple();
            HTuple hv_ColumnPred = new HTuple(), hv_PointOrderd = new HTuple();
            HTuple hv_Rowmiddnew = new HTuple(), hv_Colmiddnew = new HTuple();
            HTuple hv_meannewmid = new HTuple(), hv_RowBegintopd = new HTuple();
            HTuple hv_ColBegintopd = new HTuple(), hv_RowEndtopd = new HTuple();
            HTuple hv_ColEndtopd = new HTuple(), hv_Degltxabs = new HTuple();
            HTuple hv_DegSubD = new HTuple(), hv_RowBeginmiddnew = new HTuple();
            HTuple hv_ColBeginmiddnew = new HTuple(), hv_RowEndmiddnew = new HTuple();
            HTuple hv_ColEndmiddnew = new HTuple(), hv_RowMidDown = new HTuple();
            HTuple hv_ColMidDown = new HTuple(), hv_meanDownRNew = new HTuple();
            HTuple hv_AbsDown3DDistance = new HTuple(), hv_AbsmeanRowst = new HTuple();
            HTuple hv_Distancediagonal = new HTuple(), hv_meanDownCNew = new HTuple();
            HTuple hv_distancediag_td = new HTuple(), hv_meanRC = new HTuple();
            HTuple hv_meanRR = new HTuple(), hv_distancediag_lr = new HTuple();
            HTuple hv_Exception = new HTuple(), hv_meanTopDiag = new HTuple();
            HTuple hv_meanDownDiag = new HTuple(), hv_meanLeftDiag = new HTuple();
            HTuple hv_meanRightDiag = new HTuple(), hv_meanTopLeftDiag = new HTuple();
            HTuple hv_meanTopRightDiag = new HTuple(), hv_meanLeftLeftDiag = new HTuple();
            HTuple hv_meanLeftRightDiag = new HTuple(), hv_meanRightLeftDiag = new HTuple();
            HTuple hv_meanRightRightDiag = new HTuple(), hv_meanDownLeftDiag = new HTuple();
            HTuple hv_meanDownRightDiag = new HTuple(), hv_meanLT = new HTuple();
            HTuple hv_meanRT = new HTuple(), hv_meanRD = new HTuple();
            HTuple hv_meanLD = new HTuple(), hv_meanTD = new HTuple();
            HTuple hv_meanLR = new HTuple(), hv_SubLength = new HTuple();
            HTuple hv_Functiontmp = new HTuple(), hv_Functiontmpnew = new HTuple();
            HTuple hv_xtmp = new HTuple(), hv_SmoothedFunction = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageOutLeft);
            HOperatorSet.GenEmptyObj(out ho_ImageOutRight);
            HOperatorSet.GenEmptyObj(out ho_ImageOutTop);
            HOperatorSet.GenEmptyObj(out ho_ImageOutDown);
            HOperatorSet.GenEmptyObj(out ho_ImagePart);
            HOperatorSet.GenEmptyObj(out ho_ImagePartReal);
            HOperatorSet.GenEmptyObj(out ho_ImagePartLeft);
            HOperatorSet.GenEmptyObj(out ho_ImagePartLeftNew);
            HOperatorSet.GenEmptyObj(out ho_RegionLeft);
            HOperatorSet.GenEmptyObj(out ho_ImageReducedLeft);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_ImagePartRight);
            HOperatorSet.GenEmptyObj(out ho_ImagePartRightNew);
            HOperatorSet.GenEmptyObj(out ho_RegionRight);
            HOperatorSet.GenEmptyObj(out ho_ImageReducedRight);
            HOperatorSet.GenEmptyObj(out ho_ImagePartTop);
            HOperatorSet.GenEmptyObj(out ho_ImagePartTopNew);
            HOperatorSet.GenEmptyObj(out ho_RegionTop);
            HOperatorSet.GenEmptyObj(out ho_ImageReducedTop);
            HOperatorSet.GenEmptyObj(out ho_ImagePartDown);
            HOperatorSet.GenEmptyObj(out ho_ImagePartDownNew);
            HOperatorSet.GenEmptyObj(out ho_RegionDown);
            HOperatorSet.GenEmptyObj(out ho_ImageReducedDown);
            HOperatorSet.GenEmptyObj(out ho_Bx);
            HOperatorSet.GenEmptyObj(out ho_By);
            HOperatorSet.GenEmptyObj(out ho_Bz);
            HOperatorSet.GenEmptyObj(out ho_Intersectiont);
            HOperatorSet.GenEmptyObj(out ho_UnionContourst);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelectedt);
            HOperatorSet.GenEmptyObj(out ho_ContoursSplitt);
            HOperatorSet.GenEmptyObj(out ho_SelectedContourst);
            HOperatorSet.GenEmptyObj(out ho_contournewmid);
            HOperatorSet.GenEmptyObj(out ho_Intersectionl);
            HOperatorSet.GenEmptyObj(out ho_UnionContoursl);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelectedl);
            HOperatorSet.GenEmptyObj(out ho_ContoursSplitl);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursl);
            HOperatorSet.GenEmptyObj(out ho_Intersectionr);
            HOperatorSet.GenEmptyObj(out ho_UnionContoursr);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelectedr);
            HOperatorSet.GenEmptyObj(out ho_ContoursSplitr);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursr);
            HOperatorSet.GenEmptyObj(out ho_Intersectiond);
            HOperatorSet.GenEmptyObj(out ho_UnionContoursd);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelectedd);
            HOperatorSet.GenEmptyObj(out ho_ContoursSplitd);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursd);
            HOperatorSet.GenEmptyObj(out ho_contourleft);
            HOperatorSet.GenEmptyObj(out ho_contourmidt);
            HOperatorSet.GenEmptyObj(out ho_contourrigt);
            HOperatorSet.GenEmptyObj(out ho_contourlefr);
            HOperatorSet.GenEmptyObj(out ho_contourmidr);
            HOperatorSet.GenEmptyObj(out ho_contourrigr);
            HOperatorSet.GenEmptyObj(out ho_contourlefl);
            HOperatorSet.GenEmptyObj(out ho_contourmidl);
            HOperatorSet.GenEmptyObj(out ho_contourrigl);
            HOperatorSet.GenEmptyObj(out ho_contourlefd);
            HOperatorSet.GenEmptyObj(out ho_contourmidd);
            HOperatorSet.GenEmptyObj(out ho_contourrigd);
            HOperatorSet.GenEmptyObj(out ho_contourtmp);
            HOperatorSet.GenEmptyObj(out ho_SelectedContourslPre);
            HOperatorSet.GenEmptyObj(out ho_SelectedContourslfinal);
            HOperatorSet.GenEmptyObj(out ho_SelectedContourslnewpre);
            HOperatorSet.GenEmptyObj(out ho_contourtoplnew);
            HOperatorSet.GenEmptyObj(out ho_contourmidlnew);
            HOperatorSet.GenEmptyObj(out ho_contourdowlnew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContourslprenew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContourslnew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursrPre);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursrfinal);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursrnewpre);
            HOperatorSet.GenEmptyObj(out ho_contourtoprnew);
            HOperatorSet.GenEmptyObj(out ho_contourmidrnew);
            HOperatorSet.GenEmptyObj(out ho_contourdowrnew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursrprenew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursrnew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursdPre);
            HOperatorSet.GenEmptyObj(out ho_contourmiddnew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursdnewpre);
            HOperatorSet.GenEmptyObj(out ho_contourtopdnew);
            HOperatorSet.GenEmptyObj(out ho_contourdowdnew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursdnewnpre);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursdprenew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursdnew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursrnewnpre);
            try
            {

                ho_ImageOutLeft.Dispose();
                ho_ImageOutLeft = new HObject(ho_ImageLeft);
                ho_ImageOutRight.Dispose();
                ho_ImageOutRight = new HObject(ho_ImageRight);
                ho_ImageOutTop.Dispose();
                ho_ImageOutTop = new HObject(ho_ImageTop);
                ho_ImageOutDown.Dispose();
                ho_ImageOutDown = new HObject(ho_ImageDown);


                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_ImageOutLeft, out hv_Width, out hv_Height);
                ho_ImagePart.Dispose();
                HOperatorSet.GenImageProto(ho_ImageOutLeft, out ho_ImagePart, 60);
                ho_ImagePartReal.Dispose();
                HOperatorSet.ConvertImageType(ho_ImagePart, out ho_ImagePartReal, "real");
                ho_ImagePartLeft.Dispose();
                HOperatorSet.SubImage(ho_ImagePartReal, ho_ImageOutLeft, out ho_ImagePartLeft,
                    1, 0);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_ImagePartLeftNew.Dispose();
                    HOperatorSet.CropPart(ho_ImagePartLeft, out ho_ImagePartLeftNew, 0, 0, hv_Width - 600,
                        hv_Height);
                }
                ho_RegionLeft.Dispose();
                HOperatorSet.Threshold(ho_ImagePartLeftNew, out ho_RegionLeft, 50, 60);
                ho_ImageReducedLeft.Dispose();
                HOperatorSet.ReduceDomain(ho_ImagePartLeftNew, ho_RegionLeft, out ho_ImageReducedLeft
                    );

                //erosion_rectangle1 (RegionLeft, RegionErosion, 7, 7)
                //reduce_domain (ImagePartLeftNew, RegionErosion, ImageReducedLeft)
                //gauss_filter (ImageReducedLeft, ImageGaussLeft, 5)

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_ImageOutRight, out hv_Width, out hv_Height);
                ho_ImagePart.Dispose();
                HOperatorSet.GenImageProto(ho_ImageOutRight, out ho_ImagePart, 60);
                ho_ImagePartReal.Dispose();
                HOperatorSet.ConvertImageType(ho_ImagePart, out ho_ImagePartReal, "real");
                ho_ImagePartRight.Dispose();
                HOperatorSet.SubImage(ho_ImagePartReal, ho_ImageOutRight, out ho_ImagePartRight,
                    1, 0);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_ImagePartRightNew.Dispose();
                    HOperatorSet.CropPart(ho_ImagePartRight, out ho_ImagePartRightNew, 0, 400,
                        hv_Width - 800, hv_Height);
                }
                ho_RegionRight.Dispose();
                HOperatorSet.Threshold(ho_ImagePartRightNew, out ho_RegionRight, 50, 60);
                ho_RegionErosion.Dispose();
                HOperatorSet.ErosionRectangle1(ho_RegionRight, out ho_RegionErosion, 7, 7);
                ho_ImageReducedRight.Dispose();
                HOperatorSet.ReduceDomain(ho_ImagePartRightNew, ho_RegionErosion, out ho_ImageReducedRight
                    );
                //gauss_filter (ImageReducedRight, ImageGaussRight, 5)

                //threshold (ImagePartRightNew, RegionRight, 40, 100)
                //erosion_rectangle1 (RegionRight, RegionErosion, 7, 7)
                //reduce_domain (ImagePartRightNew, RegionErosion, ImageReducedRight)
                //gauss_filter (ImageReducedRight, ImageGaussRight, 5)

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_ImageOutTop, out hv_Width, out hv_Height);
                ho_ImagePart.Dispose();
                HOperatorSet.GenImageProto(ho_ImageOutTop, out ho_ImagePart, 60);
                ho_ImagePartReal.Dispose();
                HOperatorSet.ConvertImageType(ho_ImagePart, out ho_ImagePartReal, "real");
                ho_ImagePartTop.Dispose();
                HOperatorSet.SubImage(ho_ImagePartReal, ho_ImageOutTop, out ho_ImagePartTop,
                    1, 0);
                ho_ImagePartTopNew.Dispose();
                HOperatorSet.CropPart(ho_ImagePartTop, out ho_ImagePartTopNew, 0, 0, hv_Width,
                    hv_Height);
                ho_RegionTop.Dispose();
                HOperatorSet.Threshold(ho_ImagePartTopNew, out ho_RegionTop, 50, 60);
                ho_ImageReducedTop.Dispose();
                HOperatorSet.ReduceDomain(ho_ImagePartTopNew, ho_RegionTop, out ho_ImageReducedTop
                    );

                //threshold (ImagePartLeftNew, RegionLeft, 50, 60)
                //reduce_domain (ImagePartLeftNew, RegionLeft, ImageReducedLeft)
                //threshold (ImagePartTopNew, RegionTop, 40, 100)
                //erosion_rectangle1 (RegionTop, RegionErosion, 7, 7)
                //reduce_domain (ImagePartTopNew, RegionErosion, ImageReducedTop)
                //gauss_filter (ImageReducedTop, ImageGaussTop, 5)

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_ImageOutDown, out hv_Width, out hv_Height);
                ho_ImagePart.Dispose();
                HOperatorSet.GenImageProto(ho_ImageOutDown, out ho_ImagePart, 60);
                ho_ImagePartReal.Dispose();
                HOperatorSet.ConvertImageType(ho_ImagePart, out ho_ImagePartReal, "real");
                ho_ImagePartDown.Dispose();
                HOperatorSet.SubImage(ho_ImagePartReal, ho_ImageOutDown, out ho_ImagePartDown,
                    1, 0);
                ho_ImagePartDownNew.Dispose();
                HOperatorSet.CropPart(ho_ImagePartDown, out ho_ImagePartDownNew, 0, 0, hv_Width,
                    hv_Height);
                ho_RegionDown.Dispose();
                HOperatorSet.Threshold(ho_ImagePartDownNew, out ho_RegionDown, 50, 60);
                ho_ImageReducedDown.Dispose();
                HOperatorSet.ReduceDomain(ho_ImagePartDownNew, ho_RegionDown, out ho_ImageReducedDown
                    );

                //threshold (ImagePartDownNew, RegionDown, 40, 100)
                //erosion_rectangle1 (RegionDown, RegionErosion, 7, 7)
                //reduce_domain (ImagePartDownNew, RegionErosion, ImageReducedDown)
                //gauss_filter (ImageReducedDown, ImageGaussDown, 5)
                //convert_image_type (ImageOutLeft, ImagePartLeft, 'real')


                if (hv_Instructions == null)
                    hv_Instructions = new HTuple();
                hv_Instructions[0] = "Rotate: Left button";
                if (hv_Instructions == null)
                    hv_Instructions = new HTuple();
                hv_Instructions[1] = "Zoom:   Shift + left button";
                if (hv_Instructions == null)
                    hv_Instructions = new HTuple();
                hv_Instructions[2] = "Move:   Ctrl  + left button";
                if (HDevWindowStack.IsOpen())
                {
                    hv_WindowHandle = HDevWindowStack.GetActive();
                }
                hv_Message.Dispose();
                hv_Message = "Surface model";
                hv_r.Dispose();
                hv_r = "information";
                hv_ParameterValues.Dispose();
                hv_ParameterValues = "verbose";
                hv_Status.Dispose();
                hv_Status = "Performing triangulation with default settings ...";

                hv_yInterval.Dispose();
                hv_yInterval = 0.015;
                ho_Bx.Dispose(); ho_By.Dispose(); ho_Bz.Dispose(); hv_ObjectModel3DBLeft.Dispose();
                Creat_XYZ_From_sszn_COPY_New(ho_ImageReducedLeft, out ho_Bx, out ho_By, out ho_Bz,
                    0.015, out hv_ObjectModel3DBLeft);
                hv_ObjectModel3DBLeftConnected.Dispose();
                HOperatorSet.ConnectionObjectModel3d(hv_ObjectModel3DBLeft, "distance_3d",
                    0.15, out hv_ObjectModel3DBLeftConnected);
                hv_ObjectModel3DBLeftNew.Dispose();
                HOperatorSet.SelectObjectModel3d(hv_ObjectModel3DBLeftConnected, "num_points",
                    "and", 100000, 1e100, out hv_ObjectModel3DBLeftNew);

                //NumNeighbors := 25
                //get_object_model_3d_params (ObjectModel3DBLeft, 'neighbor_distance ' + NumNeighbors, DistanceDistribution)
                //比例
                //InlierRate := 90
                //Distance := sort(DistanceDistribution)[|DistanceDistribution| * InlierRate / 100]
                //get_object_model_3d_params (ObjectModel3DBLeft, 'num_points', NumPoints)
                //最不超过x的距离内的邻居数
                //select_points_object_model_3d (ObjectModel3DBLeft, 'num_neighbors ' + Distance, NumNeighbors, NumPoints, ObjectModel3DBLeftNew)

                ho_Bx.Dispose(); ho_By.Dispose(); ho_Bz.Dispose(); hv_ObjectModel3DBRight.Dispose();
                Creat_XYZ_From_sszn_COPY_New(ho_ImageReducedRight, out ho_Bx, out ho_By, out ho_Bz,
                    0.015, out hv_ObjectModel3DBRight);
                hv_ObjectModel3DBRightConnected.Dispose();
                HOperatorSet.ConnectionObjectModel3d(hv_ObjectModel3DBRight, "distance_3d",
                    0.15, out hv_ObjectModel3DBRightConnected);
                hv_ObjectModel3DBRightNew.Dispose();
                HOperatorSet.SelectObjectModel3d(hv_ObjectModel3DBRightConnected, "num_points",
                    "and", 100000, 1e100, out hv_ObjectModel3DBRightNew);

                ho_Bx.Dispose(); ho_By.Dispose(); ho_Bz.Dispose(); hv_ObjectModel3DBTop.Dispose();
                Creat_XYZ_From_sszn_COPY_New(ho_ImageReducedTop, out ho_Bx, out ho_By, out ho_Bz,
                    0.015, out hv_ObjectModel3DBTop);
                hv_ObjectModel3DTopConnected.Dispose();
                HOperatorSet.ConnectionObjectModel3d(hv_ObjectModel3DBTop, "distance_3d", 0.15,
                    out hv_ObjectModel3DTopConnected);
                hv_ObjectModel3DBTopNew.Dispose();
                HOperatorSet.SelectObjectModel3d(hv_ObjectModel3DTopConnected, "num_points",
                    "and", 100000, 1e100, out hv_ObjectModel3DBTopNew);

                ho_Bx.Dispose(); ho_By.Dispose(); ho_Bz.Dispose(); hv_ObjectModel3DBDown.Dispose();
                Creat_XYZ_From_sszn_COPY_New(ho_ImageReducedDown, out ho_Bx, out ho_By, out ho_Bz,
                    0.015, out hv_ObjectModel3DBDown);
                hv_ObjectModel3DBDownConnected.Dispose();
                HOperatorSet.ConnectionObjectModel3d(hv_ObjectModel3DBDown, "distance_3d",
                    0.15, out hv_ObjectModel3DBDownConnected);
                hv_ObjectModel3DBDownNew.Dispose();
                HOperatorSet.SelectObjectModel3d(hv_ObjectModel3DBDownConnected, "num_points",
                    "and", 100000, 1e100, out hv_ObjectModel3DBDownNew);



                //create_pose (50, 0, -360, 0, -180, 0, 'Rp+T', 'gba', 'point', PoseTran)
                //rigid_trans_object_model_3d (ObjectModel3DBDown, PoseTran, ObjectModel3DB1rigidDown3dtran)

                //create_pose (-177.5, 0, -180, 0, -90, 0, 'Rp+T', 'gba', 'point', PoseTran)
                //rigid_trans_object_model_3d (ObjectModel3DBLeft, PoseTran, ObjectModel3DB1rigidLeft3dtran)

                //create_pose (177.5, 0, -180, 0, 90, 0, 'Rp+T', 'gba', 'point', PoseTran)
                //rigid_trans_object_model_3d (ObjectModel3DBRight, PoseTran, ObjectModel3DB1rigidRight3dtran)

                //visualize_object_model_3d (WindowHandle, [ObjectModel3DBTop,ObjectModel3DB1rigidDown3dtran,ObjectModel3DB1rigidLeft3dtran,ObjectModel3DB1rigidRight3dtran], [], [], ['color_0','color_1','color_2','color_3' ], ['yellow','blue','cyan','red'], Message, [], Instructions, PoseOut)

                //dev_open_window (0, 0, 512, 512, 'black', WindowHandle1)
                //disp_object_model_3d (WindowHandle1, ObjectModel3DB, [], [], ['color_0' ], ['yellow'])
                //visualize_object_model_3d (WindowHandle, [ObjectModel3DBTop,ObjectModel3DB1rigidDown3dtran,ObjectModel3DB1rigidLeft3dtran,ObjectModel3DB1rigidDown3dtran], [], [], ['color_0','color_1','color_2','color_3' ], ['yellow','blue'], Message, [], Instructions, PoseOut)

                //connection_object_model_3d (ObjectModel3DBLeft, 'distance_3d', 0.15, ObjectModel3DBLeftConnected)
                //select_object_model_3d (ObjectModel3DBLeftConnected, 'num_points', 'and', 100000, 1e100, ObjectModel3DBLeftNew)
                //connection_object_model_3d (ObjectModel3DBTop, 'distance_3d', 0.15, ObjectModel3DBTopConnected)
                //select_object_model_3d (ObjectModel3DBTopConnected, 'num_points', 'and', 100000, 1e100, ObjectModel3DBTopNew)
                //connection_object_model_3d (ObjectModel3DBDown, 'distance_3d', 0.15, ObjectModel3DBDownConnected)
                //select_object_model_3d (ObjectModel3DBDownConnected, 'num_points', 'and', 100000, 1e100, ObjectModel3DBDownNew)
                //connection_object_model_3d (ObjectModel3DBRight, 'distance_3d', 0.15, ObjectModel3DBRightConnected)
                //select_object_model_3d (ObjectModel3DBRightConnected, 'num_points', 'and', 100000, 1e100, ObjectModel3DBRightNew)
                //select_object_model_3d (ObjectModel3DConnected, 'num_points', 'and', 10000, 1e100, ObjectModel3DB)
                //connection_object_model_3d (ObjectModel3DA, 'distance_3d', 0.2, ObjectModel3DConnected)
                //rigid_trans_object_model_3d (ObjectModel3DB, PoseInvert, ObjectModel3DRigidTrans)
                //union_object_model_3d ([ObjectModel3DRigidTrans,ObjectModel3DA], 'points_surface', UnionObjectModel3)
                //connection_object_model_3d (ObjectModel3DB, 'distance_3d', 0.2, ObjectModel3DConnected)
                //select_object_model_3d (ObjectModel3DB, 'num_points', 'and', 100000, 1e100, ObjectModel3DA)

                hv_SampledObjectModel3DTop.Dispose();
                HOperatorSet.SampleObjectModel3d(hv_ObjectModel3DBTop, "fast", 0.1, new HTuple(),
                    new HTuple(), out hv_SampledObjectModel3DTop);
                hv_SampledObjectModel3DDown.Dispose();
                HOperatorSet.SampleObjectModel3d(hv_ObjectModel3DBDown, "fast", 0.1, new HTuple(),
                    new HTuple(), out hv_SampledObjectModel3DDown);
                hv_SampledObjectModel3DLeft.Dispose();
                HOperatorSet.SampleObjectModel3d(hv_ObjectModel3DBLeft, "fast", 0.1, new HTuple(),
                    new HTuple(), out hv_SampledObjectModel3DLeft);
                hv_SampledObjectModel3DRight.Dispose();
                HOperatorSet.SampleObjectModel3d(hv_ObjectModel3DBRight, "fast", 0.1, new HTuple(),
                    new HTuple(), out hv_SampledObjectModel3DRight);


                //visualize_object_model_3d (WindowHandle, [SampledObjectModel3DTop,SampledObjectModel3DDown,SampledObjectModel3DLeft,SampledObjectModel3DRight], [], [], ['color_0','color_1','color_2','color_3' ], ['yellow','blue','cyan','red'], Message, [], Instructions, PoseOut)
                //get_object_model_3d_params (SampledObjectModel3DTop, 'point_coord_x', Xt)
                //get_object_model_3d_params (SampledObjectModel3DTop, 'point_coord_y', Yt)
                //get_object_model_3d_params (SampledObjectModel3DTop, 'point_coord_z', Zt)
                //gen_object_model_3d_from_points (Xt, Yt, Zt, ObjectModel3D1Top)
                //gen_object_model_3d_from_points (Xr, Yr, Zr, ObjectModel3D1Right)
                //visualize_object_model_3d (WindowHandle, [ObjectModel3D1Top,ObjectModel3D1Down,ObjectModel3D1Left,ObjectModel3D1Right], [], [], ['color_0','color_1','color_2','color_3' ], ['yellow','blue','cyan', 'red'], Message, [], Instructions, PoseOut)

                hv_Surface3DDefaultT.Dispose(); hv_Info.Dispose();
                HOperatorSet.TriangulateObjectModel3d(hv_SampledObjectModel3DTop, "greedy",
                    new HTuple(), new HTuple(), out hv_Surface3DDefaultT, out hv_Info);
                hv_Surface3DDefaultD.Dispose(); hv_Info.Dispose();
                HOperatorSet.TriangulateObjectModel3d(hv_SampledObjectModel3DDown, "greedy",
                    new HTuple(), new HTuple(), out hv_Surface3DDefaultD, out hv_Info);
                hv_Surface3DDefaultL.Dispose(); hv_Info.Dispose();
                HOperatorSet.TriangulateObjectModel3d(hv_SampledObjectModel3DLeft, "greedy",
                    new HTuple(), new HTuple(), out hv_Surface3DDefaultL, out hv_Info);
                hv_Surface3DDefaultR.Dispose(); hv_Info.Dispose();
                HOperatorSet.TriangulateObjectModel3d(hv_SampledObjectModel3DRight, "greedy",
                    new HTuple(), new HTuple(), out hv_Surface3DDefaultR, out hv_Info);

                //visualize_object_model_3d (WindowHandle, [Surface3DDefaultT,Surface3DDefaultD,Surface3DDefaultL,Surface3DDefaultR], [], [], ['color_0','color_1','color_2','color_3' ], ['yellow','blue','cyan', 'red'], Message, [], Instructions, PoseOut)
                //union_object_model_3d ([Surface3DDefaultT,Surface3DDefaultD, Surface3DDefaultL,Surface3DDefaultR], 'points_surface', UnionObjectModel3D)
                //***三维计算****
                hv_CenterPointT.Dispose(); hv_Radius.Dispose();
                HOperatorSet.SmallestSphereObjectModel3d(hv_SampledObjectModel3DTop, out hv_CenterPointT,
                    out hv_Radius);
                hv_CenterPointD.Dispose(); hv_Radius.Dispose();
                HOperatorSet.SmallestSphereObjectModel3d(hv_SampledObjectModel3DDown, out hv_CenterPointD,
                    out hv_Radius);
                hv_CenterPointL.Dispose(); hv_Radius.Dispose();
                HOperatorSet.SmallestSphereObjectModel3d(hv_SampledObjectModel3DLeft, out hv_CenterPointL,
                    out hv_Radius);
                hv_CenterPointR.Dispose(); hv_Radius.Dispose();
                HOperatorSet.SmallestSphereObjectModel3d(hv_SampledObjectModel3DRight, out hv_CenterPointR,
                    out hv_Radius);


                hv_raduis.Dispose();
                hv_raduis = new HTuple();
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetPart(HDevWindowStack.GetActive(), -1, -1, 1, 1);
                }
                hv_ObjectModel3DIntersections.Dispose();
                hv_ObjectModel3DIntersections = new HTuple();

                hv_DisparityProfileWidth.Dispose(); hv_DisparityProfileHeight.Dispose();
                HOperatorSet.GetImageSize(ho_ImageTop, out hv_DisparityProfileWidth, out hv_DisparityProfileHeight);
                hv_WindowEnlargement.Dispose();
                hv_WindowEnlargement = 350;
                hv_WindowWidth.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_WindowWidth = (hv_DisparityProfileWidth / 2) + hv_WindowEnlargement;
                }
                hv_WindowHeight.Dispose();
                hv_WindowHeight = 1000;
                //dev_open_window (0, 0, WindowWidth, WindowHeight, 'black', WindowHandle1)
                if (HDevWindowStack.IsOpen())
                {
                    //dev_set_part (0, 0, WindowHeight - 1, WindowWidth - 1)
                }
                if (HDevWindowStack.IsOpen())
                {
                    //dev_clear_window ()
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetPart(HDevWindowStack.GetActive(), 0, 0, 1000, 3100);
                }

                hv_VisualizationPlaneSize.Dispose();
                hv_VisualizationPlaneSize = 150;

                //visualize_object_model_3d (WindowHandle, [UnionObjectModel3D], [], [], ['color_0' ], ['yellow'], Message, [], Instructions, PoseOut)
                //gen_cam_par_area_scan_division (0.01, 0, 6e-6, 6e-6, WindowWidth / 2, WindowHeight / 2, WindowWidth, WindowHeight, VisualizationCamParam)
                //create_pose (CenterPoint[0] - 50, CenterPoint[1], CenterPoint[2] + 800, 0, 0, 0, 'Rp+T', 'gba', 'point', VisualizationPose)
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PoseT.Dispose();
                    HOperatorSet.CreatePose(hv_CenterPointT.TupleSelect(0), 0, 0, 90, 0, 0, "Rp+T",
                        "gba", "point", out hv_PoseT);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PoseD.Dispose();
                    HOperatorSet.CreatePose(hv_CenterPointD.TupleSelect(0), 0, 0, 90, 0, 0, "Rp+T",
                        "gba", "point", out hv_PoseD);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PoseL.Dispose();
                    HOperatorSet.CreatePose(hv_CenterPointL.TupleSelect(0), 0, 0, 90, 0, 0, "Rp+T",
                        "gba", "point", out hv_PoseL);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PoseR.Dispose();
                    HOperatorSet.CreatePose(hv_CenterPointR.TupleSelect(0), 0, 0, 90, 0, 0, "Rp+T",
                        "gba", "point", out hv_PoseR);
                }

                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IntersectionPlane1.Dispose();
                    HOperatorSet.GenPlaneObjectModel3d(hv_PoseT, (((new HTuple(-1)).TupleConcat(
                        -1)).TupleConcat(1)).TupleConcat(1) * hv_VisualizationPlaneSize, (((new HTuple(-1)).TupleConcat(
                        1)).TupleConcat(1)).TupleConcat(-1) * hv_VisualizationPlaneSize, out hv_IntersectionPlane1);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IntersectionPlane2.Dispose();
                    HOperatorSet.GenPlaneObjectModel3d(hv_PoseD, (((new HTuple(-1)).TupleConcat(
                        -1)).TupleConcat(1)).TupleConcat(1) * hv_VisualizationPlaneSize, (((new HTuple(-1)).TupleConcat(
                        1)).TupleConcat(1)).TupleConcat(-1) * hv_VisualizationPlaneSize, out hv_IntersectionPlane2);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IntersectionPlane3.Dispose();
                    HOperatorSet.GenPlaneObjectModel3d(hv_PoseL, (((new HTuple(-1)).TupleConcat(
                        -1)).TupleConcat(1)).TupleConcat(1) * hv_VisualizationPlaneSize, (((new HTuple(-1)).TupleConcat(
                        1)).TupleConcat(1)).TupleConcat(-1) * hv_VisualizationPlaneSize, out hv_IntersectionPlane3);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IntersectionPlane4.Dispose();
                    HOperatorSet.GenPlaneObjectModel3d(hv_PoseR, (((new HTuple(-1)).TupleConcat(
                        -1)).TupleConcat(1)).TupleConcat(1) * hv_VisualizationPlaneSize, (((new HTuple(-1)).TupleConcat(
                        1)).TupleConcat(1)).TupleConcat(-1) * hv_VisualizationPlaneSize, out hv_IntersectionPlane4);
                }

                //PoseOut[2] := 300
                if (hv_PoseT == null)
                    hv_PoseT = new HTuple();
                hv_PoseT[1] = 10;
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IntersectionPlane5.Dispose();
                    HOperatorSet.GenPlaneObjectModel3d(hv_PoseT, (((new HTuple(-1)).TupleConcat(
                        -1)).TupleConcat(1)).TupleConcat(1) * hv_VisualizationPlaneSize, (((new HTuple(-1)).TupleConcat(
                        1)).TupleConcat(1)).TupleConcat(-1) * hv_VisualizationPlaneSize, out hv_IntersectionPlane5);
                }

                //disp_object_model_3d (WindowHandle, [Surface3DDefault,IntersectionPlane1,IntersectionPlane2], VisualizationCamParam, PoseOut, ['color_1', 'color_2','alpha', 'alpha_0'], ['blue','orange',0.75, 1])
                //using (HDevDisposeHelper dh = new HDevDisposeHelper())
                //{
                //    hv_PoseOut.Dispose();
                //    visualize_object_model_3d(hv_WindowHandle, ((((hv_Surface3DDefaultT.TupleSelect(
                //        0))).TupleConcat(hv_IntersectionPlane1))).TupleConcat(hv_IntersectionPlane5),
                //        new HTuple(), new HTuple(), ((((new HTuple("color_0")).TupleConcat("color_1")).TupleConcat(
                //        "color_2")).TupleConcat("alpha")).TupleConcat("alpha_0"), ((((new HTuple("yellow")).TupleConcat(
                //        "cyan")).TupleConcat("blue")).TupleConcat(0.75)).TupleConcat(1), hv_Message,
                //        new HTuple(), hv_Instructions, out hv_PoseOut);
                //}


                hv_LeftIndex.Dispose();
                hv_LeftIndex = -1;
                hv_RightIndex.Dispose();
                hv_RightIndex = -1;
                hv_TopIndex.Dispose();
                hv_TopIndex = -1;
                hv_DownIndex.Dispose();
                hv_DownIndex = -1;
                hv_resultcon.Dispose();
                hv_resultcon = 0;



                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PoseT.Dispose();
                    HOperatorSet.CreatePose(hv_CenterPointT.TupleSelect(0), 0, 0, 90, 0, 0, "Rp+T",
                        "gba", "point", out hv_PoseT);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PoseD.Dispose();
                    HOperatorSet.CreatePose(hv_CenterPointD.TupleSelect(0), 0, 0, 90, 0, 0, "Rp+T",
                        "gba", "point", out hv_PoseD);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PoseL.Dispose();
                    HOperatorSet.CreatePose(hv_CenterPointL.TupleSelect(0), 0, 0, 90, 0, 0, "Rp+T",
                        "gba", "point", out hv_PoseL);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PoseR.Dispose();
                    HOperatorSet.CreatePose(hv_CenterPointR.TupleSelect(0), 0, 0, 90, 0, 0, "Rp+T",
                        "gba", "point", out hv_PoseR);
                }

                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IntersectionPlane1.Dispose();
                    HOperatorSet.GenPlaneObjectModel3d(hv_PoseT, (((new HTuple(-1)).TupleConcat(
                        -1)).TupleConcat(1)).TupleConcat(1) * hv_VisualizationPlaneSize, (((new HTuple(-1)).TupleConcat(
                        1)).TupleConcat(1)).TupleConcat(-1) * hv_VisualizationPlaneSize, out hv_IntersectionPlane1);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IntersectionPlane2.Dispose();
                    HOperatorSet.GenPlaneObjectModel3d(hv_PoseD, (((new HTuple(-1)).TupleConcat(
                        -1)).TupleConcat(1)).TupleConcat(1) * hv_VisualizationPlaneSize, (((new HTuple(-1)).TupleConcat(
                        1)).TupleConcat(1)).TupleConcat(-1) * hv_VisualizationPlaneSize, out hv_IntersectionPlane2);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IntersectionPlane3.Dispose();
                    HOperatorSet.GenPlaneObjectModel3d(hv_PoseL, (((new HTuple(-1)).TupleConcat(
                        -1)).TupleConcat(1)).TupleConcat(1) * hv_VisualizationPlaneSize, (((new HTuple(-1)).TupleConcat(
                        1)).TupleConcat(1)).TupleConcat(-1) * hv_VisualizationPlaneSize, out hv_IntersectionPlane3);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IntersectionPlane4.Dispose();
                    HOperatorSet.GenPlaneObjectModel3d(hv_PoseR, (((new HTuple(-1)).TupleConcat(
                        -1)).TupleConcat(1)).TupleConcat(1) * hv_VisualizationPlaneSize, (((new HTuple(-1)).TupleConcat(
                        1)).TupleConcat(1)).TupleConcat(-1) * hv_VisualizationPlaneSize, out hv_IntersectionPlane4);
                }

                //PoseOut[2] := 300
                if (hv_PoseR == null)
                    hv_PoseR = new HTuple();
                hv_PoseR[1] = 45;
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IntersectionPlane5.Dispose();
                    HOperatorSet.GenPlaneObjectModel3d(hv_PoseR, (((new HTuple(-1)).TupleConcat(
                        -1)).TupleConcat(1)).TupleConcat(1) * hv_VisualizationPlaneSize, (((new HTuple(-1)).TupleConcat(
                        1)).TupleConcat(1)).TupleConcat(-1) * hv_VisualizationPlaneSize, out hv_IntersectionPlane5);
                }

                //disp_object_model_3d (WindowHandle, [Surface3DDefault,IntersectionPlane1,IntersectionPlane2], VisualizationCamParam, PoseOut, ['color_1', 'color_2','alpha', 'alpha_0'], ['blue','orange',0.75, 1])
                //using (HDevDisposeHelper dh = new HDevDisposeHelper())
                //{
                //    hv_PoseOut.Dispose();
                //    visualize_object_model_3d(hv_WindowHandle, ((hv_Surface3DDefaultR.TupleConcat(
                //        hv_IntersectionPlane4))).TupleConcat(hv_IntersectionPlane5), new HTuple(),
                //        new HTuple(), ((((new HTuple("color_0")).TupleConcat("color_1")).TupleConcat(
                //        "color_2")).TupleConcat("alpha")).TupleConcat("alpha_0"), ((((new HTuple("yellow")).TupleConcat(
                //        "cyan")).TupleConcat("red")).TupleConcat(0.75)).TupleConcat(1), hv_Message,
                //        new HTuple(), hv_Instructions, out hv_PoseOut);
                //}


                HTuple end_val221 = hv_Height;
                HTuple step_val221 = 2;
                for (hv_n = 500; hv_n.Continue(end_val221, step_val221); hv_n = hv_n.TupleAdd(step_val221))
                {
                    if (hv_PoseT == null)
                        hv_PoseT = new HTuple();
                    hv_PoseT[1] = hv_n * 0.075;
                    if (hv_PoseL == null)
                        hv_PoseL = new HTuple();
                    hv_PoseL[1] = hv_n * 0.075;
                    if (hv_PoseR == null)
                        hv_PoseR = new HTuple();
                    hv_PoseR[1] = hv_n * 0.075;
                    if (hv_PoseD == null)
                        hv_PoseD = new HTuple();
                    hv_PoseD[1] = hv_n * 0.075;

                    hv_PoseT1.Dispose();
                    hv_PoseT1 = new HTuple(hv_PoseT);
                    hv_PoseR1.Dispose();
                    hv_PoseR1 = new HTuple(hv_PoseR);
                    hv_PoseD1.Dispose();
                    hv_PoseD1 = new HTuple(hv_PoseD);
                    hv_PoseL1.Dispose();
                    hv_PoseL1 = new HTuple(hv_PoseL);

                    if ((int)(new HTuple(hv_TopIndex.TupleEqual(-1))) != 0)
                    {
                        hv_resultcon.Dispose();
                        hv_resultcon = 0;
                        hv_ObjectModel3DIntersectiont.Dispose();
                        HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefaultT, hv_PoseT,
                            out hv_ObjectModel3DIntersectiont);
                        ho_Intersectiont.Dispose(); hv_resultcon.Dispose();
                        project_object_model_3d_lines_to_contour_xld(out ho_Intersectiont, hv_PoseT1,
                            hv_ObjectModel3DIntersectiont, out hv_resultcon);
                        if ((int)(new HTuple(hv_resultcon.TupleEqual(1))) != 0)
                        {
                            hv_numbercount.Dispose();
                            HOperatorSet.ContourPointNumXld(ho_Intersectiont, out hv_numbercount);
                            hv_lengthnum.Dispose();
                            HOperatorSet.TupleLength(hv_numbercount, out hv_lengthnum);
                            if ((int)(new HTuple(hv_lengthnum.TupleGreater(0))) != 0)
                            {
                                ho_UnionContourst.Dispose();
                                HOperatorSet.UnionAdjacentContoursXld(ho_Intersectiont, out ho_UnionContourst,
                                    100, 1, "attr_keep");

                                hv_Length.Dispose();
                                HOperatorSet.LengthXld(ho_UnionContourst, out hv_Length);
                                hv_Max.Dispose();
                                HOperatorSet.TupleMax(hv_Length, out hv_Max);
                                hv_Indices.Dispose();
                                HOperatorSet.TupleFind(hv_Length, hv_Max, out hv_Indices);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_ObjectSelectedt.Dispose();
                                    HOperatorSet.SelectObj(ho_UnionContourst, out ho_ObjectSelectedt, hv_Indices + 1);
                                }

                                ho_ContoursSplitt.Dispose();
                                HOperatorSet.SegmentContoursXld(ho_ObjectSelectedt, out ho_ContoursSplitt,
                                    "lines", 3, 2, 2);
                                ho_SelectedContourst.Dispose();
                                HOperatorSet.SelectContoursXld(ho_ContoursSplitt, out ho_SelectedContourst,
                                    "contour_length", 1, 2999, -0.5, 0.5);

                                hv_Numberct.Dispose();
                                HOperatorSet.CountObj(ho_SelectedContourst, out hv_Numberct);
                                if ((int)(new HTuple(hv_Numberct.TupleEqual(3))) != 0)
                                {
                                    ho_contournewmid.Dispose();
                                    HOperatorSet.SelectObj(ho_SelectedContourst, out ho_contournewmid,
                                        2);
                                    hv_Row.Dispose(); hv_Col.Dispose();
                                    HOperatorSet.GetContourXld(ho_contournewmid, out hv_Row, out hv_Col);
                                    hv_Colmin.Dispose();
                                    HOperatorSet.TupleMin(hv_Col, out hv_Colmin);
                                    hv_Colmax.Dispose();
                                    HOperatorSet.TupleMax(hv_Col, out hv_Colmax);
                                    if ((int)(new HTuple(((hv_Colmax - hv_Colmin)).TupleLessEqual(16.5))) != 0)
                                    {
                                        hv_TopIndex.Dispose();
                                        hv_TopIndex = new HTuple(hv_n);
                                    }
                                }
                            }
                        }

                    }

                    if ((int)(new HTuple(hv_LeftIndex.TupleEqual(-1))) != 0)
                    {
                        hv_resultcon.Dispose();
                        hv_resultcon = 0;
                        hv_ObjectModel3DIntersectionl.Dispose();
                        HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefaultL, hv_PoseL,
                            out hv_ObjectModel3DIntersectionl);

                        ho_Intersectionl.Dispose(); hv_resultcon.Dispose();
                        project_object_model_3d_lines_to_contour_xld(out ho_Intersectionl, hv_PoseL1,
                            hv_ObjectModel3DIntersectionl, out hv_resultcon);
                        if ((int)(new HTuple(hv_resultcon.TupleEqual(1))) != 0)
                        {
                            hv_numbercount.Dispose();
                            HOperatorSet.ContourPointNumXld(ho_Intersectionl, out hv_numbercount);
                            hv_lengthnum.Dispose();
                            HOperatorSet.TupleLength(hv_numbercount, out hv_lengthnum);

                            if ((int)(new HTuple(hv_lengthnum.TupleGreater(0))) != 0)
                            {
                                ho_UnionContoursl.Dispose();
                                HOperatorSet.UnionAdjacentContoursXld(ho_Intersectionl, out ho_UnionContoursl,
                                    100, 1, "attr_keep");

                                hv_Lengthl.Dispose();
                                HOperatorSet.LengthXld(ho_UnionContoursl, out hv_Lengthl);
                                hv_Maxl.Dispose();
                                HOperatorSet.TupleMax(hv_Lengthl, out hv_Maxl);
                                hv_Indices.Dispose();
                                HOperatorSet.TupleFind(hv_Lengthl, hv_Maxl, out hv_Indices);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_ObjectSelectedl.Dispose();
                                    HOperatorSet.SelectObj(ho_UnionContoursl, out ho_ObjectSelectedl, hv_Indices + 1);
                                }
                                ho_ContoursSplitl.Dispose();
                                HOperatorSet.SegmentContoursXld(ho_ObjectSelectedl, out ho_ContoursSplitl,
                                    "lines", 3, 2, 2);
                                ho_SelectedContoursl.Dispose();
                                HOperatorSet.SelectContoursXld(ho_ContoursSplitl, out ho_SelectedContoursl,
                                    "contour_length", 2, 2999, -0.5, 0.5);

                                hv_Numbercl.Dispose();
                                HOperatorSet.CountObj(ho_SelectedContoursl, out hv_Numbercl);

                                if ((int)(new HTuple(hv_Numbercl.TupleGreaterEqual(3))) != 0)
                                {
                                    hv_LeftIndex.Dispose();
                                    hv_LeftIndex = new HTuple(hv_n);
                                }
                            }

                        }


                    }

                    if ((int)(new HTuple(hv_RightIndex.TupleEqual(-1))) != 0)
                    {
                        hv_resultcon.Dispose();
                        hv_resultcon = 0;
                        hv_ObjectModel3DIntersectionr.Dispose();
                        HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefaultR, hv_PoseR,
                            out hv_ObjectModel3DIntersectionr);
                        ho_Intersectionr.Dispose(); hv_resultcon.Dispose();
                        project_object_model_3d_lines_to_contour_xld(out ho_Intersectionr, hv_PoseR1,
                            hv_ObjectModel3DIntersectionr, out hv_resultcon);

                        if ((int)(new HTuple(hv_resultcon.TupleEqual(1))) != 0)
                        {
                            hv_numbercount.Dispose();
                            HOperatorSet.ContourPointNumXld(ho_Intersectionr, out hv_numbercount);
                            hv_lengthnum.Dispose();
                            HOperatorSet.TupleLength(hv_numbercount, out hv_lengthnum);
                            if ((int)(new HTuple(hv_lengthnum.TupleGreater(0))) != 0)
                            {
                                ho_UnionContoursr.Dispose();
                                HOperatorSet.UnionAdjacentContoursXld(ho_Intersectionr, out ho_UnionContoursr,
                                    100, 1, "attr_keep");

                                hv_Lengthr.Dispose();
                                HOperatorSet.LengthXld(ho_UnionContoursr, out hv_Lengthr);
                                hv_Maxr.Dispose();
                                HOperatorSet.TupleMax(hv_Lengthr, out hv_Maxr);
                                hv_Indices.Dispose();
                                HOperatorSet.TupleFind(hv_Lengthr, hv_Maxr, out hv_Indices);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_ObjectSelectedr.Dispose();
                                    HOperatorSet.SelectObj(ho_UnionContoursr, out ho_ObjectSelectedr, hv_Indices + 1);
                                }
                                ho_ContoursSplitr.Dispose();
                                HOperatorSet.SegmentContoursXld(ho_ObjectSelectedr, out ho_ContoursSplitr,
                                    "lines", 3, 2, 2);
                                ho_SelectedContoursr.Dispose();
                                HOperatorSet.SelectContoursXld(ho_ContoursSplitr, out ho_SelectedContoursr,
                                    "contour_length", 2, 2999, -0.5, 0.5);

                                hv_Numbercr.Dispose();
                                HOperatorSet.CountObj(ho_SelectedContoursr, out hv_Numbercr);
                                if ((int)(new HTuple(hv_Numbercr.TupleGreaterEqual(3))) != 0)
                                {
                                    hv_RightIndex.Dispose();
                                    hv_RightIndex = new HTuple(hv_n);
                                }
                            }

                        }

                    }

                    if ((int)(new HTuple(hv_DownIndex.TupleEqual(-1))) != 0)
                    {
                        hv_resultcon.Dispose();
                        hv_resultcon = 0;
                        hv_ObjectModel3DIntersectiond.Dispose();
                        HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefaultD, hv_PoseD,
                            out hv_ObjectModel3DIntersectiond);
                        ho_Intersectiond.Dispose(); hv_resultcon.Dispose();
                        project_object_model_3d_lines_to_contour_xld(out ho_Intersectiond, hv_PoseD1,
                            hv_ObjectModel3DIntersectiond, out hv_resultcon);
                        if ((int)(new HTuple(hv_resultcon.TupleEqual(1))) != 0)
                        {
                            hv_numbercount.Dispose();
                            HOperatorSet.ContourPointNumXld(ho_Intersectiond, out hv_numbercount);
                            hv_lengthnum.Dispose();
                            HOperatorSet.TupleLength(hv_numbercount, out hv_lengthnum);
                            if ((int)(new HTuple(hv_lengthnum.TupleGreater(0))) != 0)
                            {
                                ho_UnionContoursd.Dispose();
                                HOperatorSet.UnionAdjacentContoursXld(ho_Intersectiond, out ho_UnionContoursd,
                                    100, 1, "attr_keep");


                                hv_Lengthd.Dispose();
                                HOperatorSet.LengthXld(ho_UnionContoursd, out hv_Lengthd);
                                hv_Maxd.Dispose();
                                HOperatorSet.TupleMax(hv_Lengthd, out hv_Maxd);
                                hv_Indices.Dispose();
                                HOperatorSet.TupleFind(hv_Lengthd, hv_Maxd, out hv_Indices);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_ObjectSelectedd.Dispose();
                                    HOperatorSet.SelectObj(ho_UnionContoursd, out ho_ObjectSelectedd, hv_Indices + 1);
                                }

                                ho_ContoursSplitd.Dispose();
                                HOperatorSet.SegmentContoursXld(ho_ObjectSelectedd, out ho_ContoursSplitd,
                                    "lines", 3, 2, 2);
                                ho_SelectedContoursd.Dispose();
                                HOperatorSet.SelectContoursXld(ho_ContoursSplitd, out ho_SelectedContoursd,
                                    "contour_length", 2, 2999, -0.5, 0.5);

                                hv_Numbercd.Dispose();
                                HOperatorSet.CountObj(ho_SelectedContoursd, out hv_Numbercd);

                                if ((int)(new HTuple(hv_Numbercd.TupleGreaterEqual(3))) != 0)
                                {
                                    ho_contournewmid.Dispose();
                                    HOperatorSet.SelectObj(ho_SelectedContourst, out ho_contournewmid,
                                        2);
                                    hv_Row.Dispose(); hv_Col.Dispose();
                                    HOperatorSet.GetContourXld(ho_contournewmid, out hv_Row, out hv_Col);
                                    hv_Colmin.Dispose();
                                    HOperatorSet.TupleMin(hv_Col, out hv_Colmin);
                                    hv_Colmax.Dispose();
                                    HOperatorSet.TupleMax(hv_Col, out hv_Colmax);
                                    if ((int)(new HTuple(((hv_Colmax - hv_Colmin)).TupleLessEqual(16.5))) != 0)
                                    {
                                        hv_DownIndex.Dispose();
                                        hv_DownIndex = new HTuple(hv_n);
                                    }

                                }
                            }
                        }

                    }

                    if ((int)((new HTuple((new HTuple((new HTuple(hv_TopIndex.TupleNotEqual(-1))).TupleAnd(
                        new HTuple(hv_DownIndex.TupleNotEqual(-1))))).TupleAnd(new HTuple(hv_LeftIndex.TupleNotEqual(
                        -1))))).TupleAnd(new HTuple(hv_RightIndex.TupleNotEqual(-1)))) != 0)
                    {
                        break;
                    }

                }

                hv_Left3DX.Dispose();
                hv_Left3DX = 0;
                hv_Right3DX.Dispose();
                hv_Right3DX = 0;
                hv_Down3DY.Dispose();
                hv_Down3DY = 0;


                //边长
                hv_DistancesRT.Dispose();
                hv_DistancesRT = new HTuple();
                hv_DistancesLT.Dispose();
                hv_DistancesLT = new HTuple();
                hv_DistancesRD.Dispose();
                hv_DistancesRD = new HTuple();
                hv_DistancesLD.Dispose();
                hv_DistancesLD = new HTuple();
                //对角线
                hv_DistancesTD.Dispose();
                hv_DistancesTD = new HTuple();
                hv_DistancesLR.Dispose();
                hv_DistancesLR = new HTuple();

                //斜边弦长与直角边长
                hv_DistancesTopDiag.Dispose();
                hv_DistancesTopDiag = new HTuple();
                hv_DistancesTopLeftDiag.Dispose();
                hv_DistancesTopLeftDiag = new HTuple();
                hv_DistancesTopRightDiag.Dispose();
                hv_DistancesTopRightDiag = new HTuple();
                hv_DistancesDownDiag.Dispose();
                hv_DistancesDownDiag = new HTuple();
                hv_DistancesDownLeftDiag.Dispose();
                hv_DistancesDownLeftDiag = new HTuple();
                hv_DistancesDownRightDiag.Dispose();
                hv_DistancesDownRightDiag = new HTuple();
                hv_DistancesLeftDiag.Dispose();
                hv_DistancesLeftDiag = new HTuple();
                hv_DistancesLeftLeftDiag.Dispose();
                hv_DistancesLeftLeftDiag = new HTuple();
                hv_DistancesLeftRightDiag.Dispose();
                hv_DistancesLeftRightDiag = new HTuple();
                hv_DistancesRightDiag.Dispose();
                hv_DistancesRightDiag = new HTuple();
                hv_DistancesRightLeftDiag.Dispose();
                hv_DistancesRightLeftDiag = new HTuple();
                hv_DistancesRightRightDiag.Dispose();
                hv_DistancesRightRightDiag = new HTuple();

                //平面度 斜面处理


                //垂直度



                //总长

                hv_Y.Dispose();
                HOperatorSet.GetObjectModel3dParams(hv_Surface3DDefaultT, "point_coord_y",
                    out hv_Y);
                hv_MaxY.Dispose();
                HOperatorSet.TupleMax(hv_Y, out hv_MaxY);
                hv_MinY.Dispose();
                HOperatorSet.TupleMin(hv_Y, out hv_MinY);
                hv_LengthY.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_LengthY = (hv_MaxY - hv_MinY) / 0.075;
                }
                hv_NumLengthY.Dispose();
                HOperatorSet.TupleRound(hv_LengthY, out hv_NumLengthY);
                hv_lengthofY.Dispose();
                HOperatorSet.TupleLength(hv_Y, out hv_lengthofY);
                HTuple end_val409 = hv_lengthofY - 1;
                HTuple step_val409 = 1;
                for (hv_n = 0; hv_n.Continue(end_val409, step_val409); hv_n = hv_n.TupleAdd(step_val409))
                {
                    if ((int)(new HTuple(((hv_Y.TupleSelect(hv_n))).TupleGreater(37.5))) != 0)
                    {
                        break;
                    }
                }

                hv_wlength.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_wlength = hv_MaxY - (hv_Y.TupleSelect(
                        hv_n));
                }


                HTuple end_val418 = hv_NumLengthY - 2;
                HTuple step_val418 = 1;
                for (hv_n = 10; hv_n.Continue(end_val418, step_val418); hv_n = hv_n.TupleAdd(step_val418))
                {
                    try
                    {
                        if ((int)((new HTuple((new HTuple((new HTuple(((hv_n + hv_TopIndex)).TupleGreaterEqual(
                            hv_Height))).TupleOr(new HTuple(((hv_n + hv_LeftIndex)).TupleGreaterEqual(
                            hv_Height))))).TupleOr(new HTuple(((hv_n + hv_RightIndex)).TupleGreater(
                            hv_Height))))).TupleOr(new HTuple(((hv_n + hv_DownIndex)).TupleGreater(
                            hv_Height)))) != 0)
                        {
                            break;
                        }
                        if (hv_PoseT == null)
                            hv_PoseT = new HTuple();
                        hv_PoseT[1] = (hv_n + hv_TopIndex) * 0.075;
                        if (hv_PoseL == null)
                            hv_PoseL = new HTuple();
                        hv_PoseL[1] = (hv_n + hv_LeftIndex) * 0.075;
                        if (hv_PoseR == null)
                            hv_PoseR = new HTuple();
                        hv_PoseR[1] = (hv_n + hv_RightIndex) * 0.075;
                        if (hv_PoseD == null)
                            hv_PoseD = new HTuple();
                        hv_PoseD[1] = (hv_n + hv_DownIndex) * 0.075;

                        hv_ObjectModel3DIntersectiont.Dispose();
                        HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefaultT, hv_PoseT,
                            out hv_ObjectModel3DIntersectiont);
                        hv_ObjectModel3DIntersectionl.Dispose();
                        HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefaultL, hv_PoseL,
                            out hv_ObjectModel3DIntersectionl);
                        hv_ObjectModel3DIntersectionr.Dispose();
                        HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefaultR, hv_PoseR,
                            out hv_ObjectModel3DIntersectionr);
                        hv_ObjectModel3DIntersectiond.Dispose();
                        HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefaultD, hv_PoseD,
                            out hv_ObjectModel3DIntersectiond);

                        hv_PoseT1.Dispose();
                        hv_PoseT1 = new HTuple(hv_PoseT);
                        hv_PoseR1.Dispose();
                        hv_PoseR1 = new HTuple(hv_PoseR);
                        hv_PoseD1.Dispose();
                        hv_PoseD1 = new HTuple(hv_PoseD);
                        hv_PoseL1.Dispose();
                        hv_PoseL1 = new HTuple(hv_PoseL);

                        //gen_plane_object_model_3d (PoseR1, [-1, -1, 1, 1] * VisualizationPlaneSize, [-1, 1, 1, -1] * VisualizationPlaneSize, IntersectionPlane4)
                        //visualize_object_model_3d (WindowHandle, [Surface3DDefaultR,IntersectionPlane4], [], [], ['color_0','color_1'], ['yellow','cyan'], Message, [], Instructions, PoseOut)

                        ho_Intersectiont.Dispose(); hv_resultcon1.Dispose();
                        project_object_model_3d_lines_to_contour_xld(out ho_Intersectiont, hv_PoseT1,
                            hv_ObjectModel3DIntersectiont, out hv_resultcon1);
                        ho_UnionContourst.Dispose();
                        HOperatorSet.UnionAdjacentContoursXld(ho_Intersectiont, out ho_UnionContourst,
                            100, 1, "attr_keep");
                        ho_Intersectionl.Dispose(); hv_resultcon2.Dispose();
                        project_object_model_3d_lines_to_contour_xld(out ho_Intersectionl, hv_PoseL1,
                            hv_ObjectModel3DIntersectionl, out hv_resultcon2);
                        ho_UnionContoursl.Dispose();
                        HOperatorSet.UnionAdjacentContoursXld(ho_Intersectionl, out ho_UnionContoursl,
                            100, 1, "attr_keep");
                        ho_Intersectionr.Dispose(); hv_resultcon3.Dispose();
                        project_object_model_3d_lines_to_contour_xld(out ho_Intersectionr, hv_PoseR1,
                            hv_ObjectModel3DIntersectionr, out hv_resultcon3);
                        ho_UnionContoursr.Dispose();
                        HOperatorSet.UnionAdjacentContoursXld(ho_Intersectionr, out ho_UnionContoursr,
                            100, 1, "attr_keep");
                        ho_Intersectiond.Dispose(); hv_resultcon4.Dispose();
                        project_object_model_3d_lines_to_contour_xld(out ho_Intersectiond, hv_PoseD1,
                            hv_ObjectModel3DIntersectiond, out hv_resultcon4);
                        ho_UnionContoursd.Dispose();
                        HOperatorSet.UnionAdjacentContoursXld(ho_Intersectiond, out ho_UnionContoursd,
                            100, 1, "attr_keep");

                        if ((int)((new HTuple((new HTuple((new HTuple(hv_resultcon1.TupleEqual(
                            0))).TupleOr(new HTuple(hv_resultcon2.TupleEqual(0))))).TupleOr(new HTuple(hv_resultcon3.TupleEqual(
                            0))))).TupleOr(new HTuple(hv_resultcon4.TupleEqual(0)))) != 0)
                        {
                            continue;
                        }
                        hv_Length.Dispose();
                        HOperatorSet.LengthXld(ho_UnionContourst, out hv_Length);
                        hv_Max.Dispose();
                        HOperatorSet.TupleMax(hv_Length, out hv_Max);
                        hv_Indices.Dispose();
                        HOperatorSet.TupleFind(hv_Length, hv_Max, out hv_Indices);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            ho_ObjectSelectedt.Dispose();
                            HOperatorSet.SelectObj(ho_UnionContourst, out ho_ObjectSelectedt, hv_Indices + 1);
                        }

                        hv_Lengthl.Dispose();
                        HOperatorSet.LengthXld(ho_UnionContoursl, out hv_Lengthl);
                        hv_Maxl.Dispose();
                        HOperatorSet.TupleMax(hv_Lengthl, out hv_Maxl);
                        hv_Indices.Dispose();
                        HOperatorSet.TupleFind(hv_Lengthl, hv_Maxl, out hv_Indices);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            ho_ObjectSelectedl.Dispose();
                            HOperatorSet.SelectObj(ho_UnionContoursl, out ho_ObjectSelectedl, hv_Indices + 1);
                        }

                        hv_Lengthr.Dispose();
                        HOperatorSet.LengthXld(ho_UnionContoursr, out hv_Lengthr);
                        hv_Maxr.Dispose();
                        HOperatorSet.TupleMax(hv_Lengthr, out hv_Maxr);
                        hv_Indices.Dispose();
                        HOperatorSet.TupleFind(hv_Lengthr, hv_Maxr, out hv_Indices);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            ho_ObjectSelectedr.Dispose();
                            HOperatorSet.SelectObj(ho_UnionContoursr, out ho_ObjectSelectedr, hv_Indices + 1);
                        }

                        hv_Lengthd.Dispose();
                        HOperatorSet.LengthXld(ho_UnionContoursd, out hv_Lengthd);
                        hv_Maxd.Dispose();
                        HOperatorSet.TupleMax(hv_Lengthd, out hv_Maxd);
                        hv_Indices.Dispose();
                        HOperatorSet.TupleFind(hv_Lengthd, hv_Maxd, out hv_Indices);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            ho_ObjectSelectedd.Dispose();
                            HOperatorSet.SelectObj(ho_UnionContoursd, out ho_ObjectSelectedd, hv_Indices + 1);
                        }


                        ho_ContoursSplitt.Dispose();
                        HOperatorSet.SegmentContoursXld(ho_ObjectSelectedt, out ho_ContoursSplitt,
                            "lines", 3, 2, 2);
                        ho_SelectedContourst.Dispose();
                        HOperatorSet.SelectContoursXld(ho_ContoursSplitt, out ho_SelectedContourst,
                            "contour_length", 0.5, 2999, -0.5, 0.5);

                        ho_ContoursSplitl.Dispose();
                        HOperatorSet.SegmentContoursXld(ho_ObjectSelectedl, out ho_ContoursSplitl,
                            "lines", 3, 2, 2);
                        ho_SelectedContoursl.Dispose();
                        HOperatorSet.SelectContoursXld(ho_ContoursSplitl, out ho_SelectedContoursl,
                            "contour_length", 2, 2999, -0.5, 0.5);

                        ho_ContoursSplitr.Dispose();
                        HOperatorSet.SegmentContoursXld(ho_ObjectSelectedr, out ho_ContoursSplitr,
                            "lines", 3, 2, 2);
                        ho_SelectedContoursr.Dispose();
                        HOperatorSet.SelectContoursXld(ho_ContoursSplitr, out ho_SelectedContoursr,
                            "contour_length", 2, 2999, -0.5, 0.5);


                        ho_ContoursSplitd.Dispose();
                        HOperatorSet.SegmentContoursXld(ho_ObjectSelectedd, out ho_ContoursSplitd,
                            "lines", 2, 2, 2);
                        ho_SelectedContoursd.Dispose();
                        HOperatorSet.SelectContoursXld(ho_ContoursSplitd, out ho_SelectedContoursd,
                            "contour_length", 2, 2999, -0.5, 0.5);

                        hv_Numberct.Dispose();
                        HOperatorSet.CountObj(ho_SelectedContourst, out hv_Numberct);
                        hv_Numbercr.Dispose();
                        HOperatorSet.CountObj(ho_SelectedContoursr, out hv_Numbercr);
                        hv_Numbercl.Dispose();
                        HOperatorSet.CountObj(ho_SelectedContoursl, out hv_Numbercl);
                        hv_Numbercd.Dispose();
                        HOperatorSet.CountObj(ho_SelectedContoursd, out hv_Numbercd);



                        if ((int)((new HTuple((new HTuple((new HTuple(hv_Numbercr.TupleEqual(3))).TupleAnd(
                            new HTuple(hv_Numberct.TupleEqual(3))))).TupleAnd(new HTuple(hv_Numbercd.TupleEqual(
                            3))))).TupleAnd(new HTuple(hv_Numbercl.TupleEqual(3)))) != 0)
                        {
                            ho_contourleft.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContourst, out ho_contourleft, 1);
                            ho_contourmidt.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContourst, out ho_contourmidt, 2);
                            ho_contourrigt.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContourst, out ho_contourrigt, 3);

                            ho_contourlefr.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursr, out ho_contourlefr, 1);
                            ho_contourmidr.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursr, out ho_contourmidr, 2);
                            ho_contourrigr.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursr, out ho_contourrigr, 3);

                            ho_contourlefl.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursl, out ho_contourlefl, 1);
                            ho_contourmidl.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursl, out ho_contourmidl, 2);
                            ho_contourrigl.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursl, out ho_contourrigl, 3);

                            ho_contourlefd.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursd, out ho_contourlefd, 1);
                            ho_contourmidd.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursd, out ho_contourmidd, 2);
                            ho_contourrigd.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursd, out ho_contourrigd, 3);


                            //检查上侧3D采集图像，分割线段是否会出现折线
                            hv_Rowsleft.Dispose(); hv_Colsleft.Dispose();
                            HOperatorSet.GetContourXld(ho_contourleft, out hv_Rowsleft, out hv_Colsleft);
                            hv_Rowsrigt.Dispose(); hv_Colsrigt.Dispose();
                            HOperatorSet.GetContourXld(ho_contourrigt, out hv_Rowsrigt, out hv_Colsrigt);
                            hv_Rowsmidt.Dispose(); hv_Colsmidt.Dispose();
                            HOperatorSet.GetContourXld(ho_contourmidt, out hv_Rowsmidt, out hv_Colsmidt);

                            hv_Minleft.Dispose();
                            HOperatorSet.TupleMin(hv_Colsleft, out hv_Minleft);
                            hv_Maxleft.Dispose();
                            HOperatorSet.TupleMax(hv_Colsleft, out hv_Maxleft);

                            hv_Minrigt.Dispose();
                            HOperatorSet.TupleMin(hv_Colsrigt, out hv_Minrigt);
                            hv_Maxrigt.Dispose();
                            HOperatorSet.TupleMax(hv_Colsrigt, out hv_Maxrigt);

                            hv_RowBeginleft.Dispose(); hv_ColBeginleft.Dispose(); hv_RowEndleft.Dispose(); hv_ColEndleft.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourleft, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginleft, out hv_ColBeginleft, out hv_RowEndleft, out hv_ColEndleft,
                                out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                            hv_RowBeginrigt.Dispose(); hv_ColBeginrigt.Dispose(); hv_RowEndrigt.Dispose(); hv_ColEndrigt.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigt.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourrigt, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginrigt, out hv_ColBeginrigt, out hv_RowEndrigt, out hv_ColEndrigt,
                                out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigt);

                            hv_lengthleftcontour.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthleftcontour = hv_Minleft - hv_Maxleft;
                            }
                            hv_lengthleflined.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthleflined = hv_ColEndleft - hv_ColBeginleft;
                            }
                            hv_Abslengthleftcontour.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthleftcontour, out hv_Abslengthleftcontour);
                            hv_Abslengthleflined.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthleflined, out hv_Abslengthleflined);
                            if ((int)(new HTuple(((hv_Abslengthleftcontour - hv_Abslengthleflined)).TupleGreater(
                                0.3))) != 0)
                            {
                                hv_countofCols.Dispose();
                                HOperatorSet.TupleLength(hv_Colsleft, out hv_countofCols);
                                HTuple end_val532 = 2;
                                HTuple step_val532 = -1;
                                for (hv_Index = hv_countofCols - 1; hv_Index.Continue(end_val532, step_val532); hv_Index = hv_Index.TupleAdd(step_val532))
                                {
                                    if ((int)((new HTuple((((hv_Rowsleft.TupleSelect(hv_Index)) - (hv_Rowsleft.TupleSelect(
                                        hv_Index - 1)))).TupleLess(-0.02))).TupleAnd(new HTuple((((hv_Rowsleft.TupleSelect(
                                        hv_Index - 1)) - (hv_Rowsleft.TupleSelect(hv_Index - 2)))).TupleLess(
                                        -0.02)))) != 0)
                                    {
                                        break;
                                    }
                                }
                                hv_Rowsnewleft.Dispose();
                                hv_Rowsnewleft = new HTuple();
                                hv_Colsnewleft.Dispose();
                                hv_Colsnewleft = new HTuple();
                                if ((int)(new HTuple(hv_Index.TupleGreater(2))) != 0)
                                {
                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val541 = hv_Index;
                                    HTuple step_val541 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val541, step_val541); hv_Index1 = hv_Index1.TupleAdd(step_val541))
                                    {
                                        if (hv_Rowsnewleft == null)
                                            hv_Rowsnewleft = new HTuple();
                                        hv_Rowsnewleft[hv_Indexx] = hv_Rowsleft.TupleSelect(hv_Index1);
                                        if (hv_Colsnewleft == null)
                                            hv_Colsnewleft = new HTuple();
                                        hv_Colsnewleft[hv_Indexx] = hv_Colsleft.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val548 = hv_countofCols - 1;
                                    HTuple step_val548 = 1;
                                    for (hv_Index1 = hv_Index; hv_Index1.Continue(end_val548, step_val548); hv_Index1 = hv_Index1.TupleAdd(step_val548))
                                    {
                                        if (hv_Rowsnewmidt == null)
                                            hv_Rowsnewmidt = new HTuple();
                                        hv_Rowsnewmidt[hv_Indexx] = hv_Rowsleft.TupleSelect(hv_Index1);
                                        if (hv_Colsnewmidt == null)
                                            hv_Colsnewmidt = new HTuple();
                                        hv_Colsnewmidt[hv_Indexx] = hv_Colsleft.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_countofRowst.Dispose();
                                    HOperatorSet.TupleLength(hv_Rowsmidt, out hv_countofRowst);
                                    HTuple end_val555 = hv_countofRowst - 1;
                                    HTuple step_val555 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val555, step_val555); hv_Index1 = hv_Index1.TupleAdd(step_val555))
                                    {
                                        if (hv_Rowsnewmidt == null)
                                            hv_Rowsnewmidt = new HTuple();
                                        hv_Rowsnewmidt[hv_Indexx] = hv_Rowsmidt.TupleSelect(hv_Index1);
                                        if (hv_Colsnewmidt == null)
                                            hv_Colsnewmidt = new HTuple();
                                        hv_Colsnewmidt[hv_Indexx] = hv_Colsmidt.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    ho_contourleft.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourleft, hv_Rowsnewleft,
                                        hv_Colsnewleft);
                                    ho_contourmidt.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourmidt, hv_Rowsnewmidt,
                                        hv_Colsnewmidt);
                                    hv_RowBeginmidt.Dispose(); hv_ColBeginmidt.Dispose(); hv_RowEndmidt.Dispose(); hv_ColEndmidt.Dispose(); hv_Nrmidt.Dispose(); hv_Ncmidt.Dispose(); hv_Distmidt.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourmidt, "drop", -1, 5, 5,
                                        1, out hv_RowBeginmidt, out hv_ColBeginmidt, out hv_RowEndmidt,
                                        out hv_ColEndmidt, out hv_Nrmidt, out hv_Ncmidt, out hv_Distmidt);
                                    hv_RowBeginleft.Dispose(); hv_ColBeginleft.Dispose(); hv_RowEndleft.Dispose(); hv_ColEndleft.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourleft, "drop", -1, 5, 5,
                                        1, out hv_RowBeginleft, out hv_ColBeginleft, out hv_RowEndleft,
                                        out hv_ColEndleft, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);

                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContourst, ho_contourleft, out ExpTmpOutVar_0,
                                            1);
                                        ho_SelectedContourst.Dispose();
                                        ho_SelectedContourst = ExpTmpOutVar_0;
                                    }
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContourst, ho_contourmidt, out ExpTmpOutVar_0,
                                            2);
                                        ho_SelectedContourst.Dispose();
                                        ho_SelectedContourst = ExpTmpOutVar_0;
                                    }

                                }

                            }


                            hv_lengthrigtcontour.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthrigtcontour = hv_Maxrigt - hv_Minrigt;
                            }
                            hv_lengthriglined.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthriglined = hv_ColEndrigt - hv_ColBeginrigt;
                            }
                            hv_Abslengthrigtcontour.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthrigtcontour, out hv_Abslengthrigtcontour);
                            hv_Abslengthriglined.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthriglined, out hv_Abslengthriglined);

                            if ((int)(new HTuple(((hv_Abslengthrigtcontour - hv_Abslengthriglined)).TupleGreater(
                                0.3))) != 0)
                            {
                                hv_countofCols.Dispose();
                                HOperatorSet.TupleLength(hv_Rowsrigt, out hv_countofCols);
                                HTuple end_val581 = hv_countofCols - 1;
                                HTuple step_val581 = 1;
                                for (hv_Index = 1; hv_Index.Continue(end_val581, step_val581); hv_Index = hv_Index.TupleAdd(step_val581))
                                {
                                    if ((int)(new HTuple((((hv_Rowsrigt.TupleSelect(hv_Index)) - (hv_Rowsrigt.TupleSelect(
                                        hv_Index + 1)))).TupleLess(-0.02))) != 0)
                                    {
                                        break;
                                    }
                                }

                                hv_Rowsnewrigt.Dispose();
                                hv_Rowsnewrigt = new HTuple();
                                hv_Colsnewrigt.Dispose();
                                hv_Colsnewrigt = new HTuple();
                                if ((int)(new HTuple(hv_Index.TupleNotEqual(hv_countofCols - 1))) != 0)
                                {
                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val591 = hv_countofCols - 1;
                                    HTuple step_val591 = 1;
                                    for (hv_Index1 = hv_Index; hv_Index1.Continue(end_val591, step_val591); hv_Index1 = hv_Index1.TupleAdd(step_val591))
                                    {
                                        if (hv_Rowsnewrigt == null)
                                            hv_Rowsnewrigt = new HTuple();
                                        hv_Rowsnewrigt[hv_Indexx] = hv_Rowsrigt.TupleSelect(hv_Index1);
                                        if (hv_Colsnewrigt == null)
                                            hv_Colsnewrigt = new HTuple();
                                        hv_Colsnewrigt[hv_Indexx] = hv_Colsrigt.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_countofRowst.Dispose();
                                    HOperatorSet.TupleLength(hv_Rowsmidt, out hv_countofRowst);
                                    hv_Indexx.Dispose();
                                    hv_Indexx = new HTuple(hv_countofRowst);
                                    HTuple end_val599 = hv_Index - 1;
                                    HTuple step_val599 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val599, step_val599); hv_Index1 = hv_Index1.TupleAdd(step_val599))
                                    {
                                        if (hv_Rowsmidt == null)
                                            hv_Rowsmidt = new HTuple();
                                        hv_Rowsmidt[hv_Indexx] = hv_Rowsrigt.TupleSelect(hv_Index1);
                                        if (hv_Colsmidt == null)
                                            hv_Colsmidt = new HTuple();
                                        hv_Colsmidt[hv_Indexx] = hv_Colsrigt.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    ho_contourmidt.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourmidt, hv_Rowsmidt,
                                        hv_Colsmidt);
                                    ho_contourrigt.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourrigt, hv_Rowsnewrigt,
                                        hv_Colsnewrigt);
                                    hv_RowBeginmidt.Dispose(); hv_ColBeginmidt.Dispose(); hv_RowEndmidt.Dispose(); hv_ColEndmidt.Dispose(); hv_Nrmidt.Dispose(); hv_Ncmidt.Dispose(); hv_Distmidt.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourmidt, "drop", -1, 5, 5,
                                        1, out hv_RowBeginmidt, out hv_ColBeginmidt, out hv_RowEndmidt,
                                        out hv_ColEndmidt, out hv_Nrmidt, out hv_Ncmidt, out hv_Distmidt);
                                    hv_RowBeginrigt.Dispose(); hv_ColBeginrigt.Dispose(); hv_RowEndrigt.Dispose(); hv_ColEndrigt.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigt.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourrigt, "drop", -1, 5, 5,
                                        1, out hv_RowBeginrigt, out hv_ColBeginrigt, out hv_RowEndrigt,
                                        out hv_ColEndrigt, out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigt);

                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContourst, ho_contourmidt, out ExpTmpOutVar_0,
                                            2);
                                        ho_SelectedContourst.Dispose();
                                        ho_SelectedContourst = ExpTmpOutVar_0;
                                    }
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContourst, ho_contourrigt, out ExpTmpOutVar_0,
                                            3);
                                        ho_SelectedContourst.Dispose();
                                        ho_SelectedContourst = ExpTmpOutVar_0;
                                    }
                                }
                            }




                            //检查下侧3D采集图像，分割线段是否会出现折线
                            hv_Rowslefd.Dispose(); hv_Colslefd.Dispose();
                            HOperatorSet.GetContourXld(ho_contourlefd, out hv_Rowslefd, out hv_Colslefd);
                            hv_Rowsrigd.Dispose(); hv_Colsrigd.Dispose();
                            HOperatorSet.GetContourXld(ho_contourrigd, out hv_Rowsrigd, out hv_Colsrigd);
                            hv_Rowsmidd.Dispose(); hv_Colsmidd.Dispose();
                            HOperatorSet.GetContourXld(ho_contourmidd, out hv_Rowsmidd, out hv_Colsmidd);


                            hv_Minlefd.Dispose();
                            HOperatorSet.TupleMin(hv_Colslefd, out hv_Minlefd);
                            hv_Maxlefd.Dispose();
                            HOperatorSet.TupleMax(hv_Colslefd, out hv_Maxlefd);

                            hv_Minrigd.Dispose();
                            HOperatorSet.TupleMin(hv_Colsrigd, out hv_Minrigd);
                            hv_Maxrigd.Dispose();
                            HOperatorSet.TupleMax(hv_Colsrigd, out hv_Maxrigd);

                            hv_RowBeginlefd.Dispose(); hv_ColBeginlefd.Dispose(); hv_RowEndlefd.Dispose(); hv_ColEndlefd.Dispose(); hv_Nrlefd.Dispose(); hv_Nclefd.Dispose(); hv_Distlefd.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourlefd, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginlefd, out hv_ColBeginlefd, out hv_RowEndlefd, out hv_ColEndlefd,
                                out hv_Nrlefd, out hv_Nclefd, out hv_Distlefd);
                            hv_RowBeginrigd.Dispose(); hv_ColBeginrigd.Dispose(); hv_RowEndrigd.Dispose(); hv_ColEndrigd.Dispose(); hv_Nrrigd.Dispose(); hv_Ncrigd.Dispose(); hv_Distrigd.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourrigd, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginrigd, out hv_ColBeginrigd, out hv_RowEndrigd, out hv_ColEndrigd,
                                out hv_Nrrigd, out hv_Ncrigd, out hv_Distrigd);


                            hv_lengthlefdcontour.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthlefdcontour = hv_Maxlefd - hv_Minlefd;
                            }
                            hv_lengthleflined.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthleflined = hv_ColEndlefd - hv_ColBeginlefd;
                            }
                            hv_Abslengthlefdcontour.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthlefdcontour, out hv_Abslengthlefdcontour);
                            hv_Abslengthleflined.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthleflined, out hv_Abslengthleflined);
                            if ((int)(new HTuple(((hv_Abslengthlefdcontour - hv_Abslengthleflined)).TupleGreater(
                                0.3))) != 0)
                            {
                                hv_countofCols.Dispose();
                                HOperatorSet.TupleLength(hv_Colslefd, out hv_countofCols);
                                HTuple end_val640 = 2;
                                HTuple step_val640 = -1;
                                for (hv_Index = hv_countofCols - 1; hv_Index.Continue(end_val640, step_val640); hv_Index = hv_Index.TupleAdd(step_val640))
                                {
                                    if ((int)((new HTuple((((hv_Rowslefd.TupleSelect(hv_Index)) - (hv_Rowslefd.TupleSelect(
                                        hv_Index - 1)))).TupleLess(-0.02))).TupleAnd(new HTuple((((hv_Rowslefd.TupleSelect(
                                        hv_Index - 1)) - (hv_Rowslefd.TupleSelect(hv_Index - 2)))).TupleLess(
                                        -0.02)))) != 0)
                                    {
                                        break;
                                    }
                                }

                                hv_Rowsnewlefd.Dispose();
                                hv_Rowsnewlefd = new HTuple();
                                hv_Colsnewlefd.Dispose();
                                hv_Colsnewlefd = new HTuple();
                                if ((int)(new HTuple(hv_Index.TupleGreater(2))) != 0)
                                {
                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val650 = hv_Index;
                                    HTuple step_val650 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val650, step_val650); hv_Index1 = hv_Index1.TupleAdd(step_val650))
                                    {
                                        if (hv_Rowsnewlefd == null)
                                            hv_Rowsnewlefd = new HTuple();
                                        hv_Rowsnewlefd[hv_Indexx] = hv_Rowslefd.TupleSelect(hv_Index1);
                                        if (hv_Colsnewlefd == null)
                                            hv_Colsnewlefd = new HTuple();
                                        hv_Colsnewlefd[hv_Indexx] = hv_Colslefd.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }


                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val658 = hv_countofCols - 1;
                                    HTuple step_val658 = 1;
                                    for (hv_Index1 = hv_Index; hv_Index1.Continue(end_val658, step_val658); hv_Index1 = hv_Index1.TupleAdd(step_val658))
                                    {
                                        if (hv_Rowsnewmidd == null)
                                            hv_Rowsnewmidd = new HTuple();
                                        hv_Rowsnewmidd[hv_Indexx] = hv_Rowslefd.TupleSelect(hv_Index1);
                                        if (hv_Colsnewmidd == null)
                                            hv_Colsnewmidd = new HTuple();
                                        hv_Colsnewmidd[hv_Indexx] = hv_Colslefd.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_countofRowsd.Dispose();
                                    HOperatorSet.TupleLength(hv_Rowsmidd, out hv_countofRowsd);
                                    HTuple end_val665 = hv_countofRowsd - 1;
                                    HTuple step_val665 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val665, step_val665); hv_Index1 = hv_Index1.TupleAdd(step_val665))
                                    {
                                        if (hv_Rowsnewmidd == null)
                                            hv_Rowsnewmidd = new HTuple();
                                        hv_Rowsnewmidd[hv_Indexx] = hv_Rowsmidd.TupleSelect(hv_Index1);
                                        if (hv_Colsnewmidd == null)
                                            hv_Colsnewmidd = new HTuple();
                                        hv_Colsnewmidd[hv_Indexx] = hv_Colsmidd.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    ho_contourlefd.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourlefd, hv_Rowsnewlefd,
                                        hv_Colsnewlefd);
                                    ho_contourmidd.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourmidd, hv_Rowsnewmidd,
                                        hv_Colsnewmidd);
                                    hv_RowBeginmidd.Dispose(); hv_ColBeginmidd.Dispose(); hv_RowEndmidd.Dispose(); hv_ColEndmidd.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourmidd, "drop", -1, 5, 5,
                                        1, out hv_RowBeginmidd, out hv_ColBeginmidd, out hv_RowEndmidd,
                                        out hv_ColEndmidd, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                    hv_RowBeginlefd.Dispose(); hv_ColBeginlefd.Dispose(); hv_RowEndlefd.Dispose(); hv_ColEndlefd.Dispose(); hv_Nrlefd.Dispose(); hv_Nclefd.Dispose(); hv_Distlefd.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourlefd, "drop", -1, 5, 5,
                                        1, out hv_RowBeginlefd, out hv_ColBeginlefd, out hv_RowEndlefd,
                                        out hv_ColEndlefd, out hv_Nrlefd, out hv_Nclefd, out hv_Distlefd);

                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursd, ho_contourlefd, out ExpTmpOutVar_0,
                                            1);
                                        ho_SelectedContoursd.Dispose();
                                        ho_SelectedContoursd = ExpTmpOutVar_0;
                                    }
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursd, ho_contourmidd, out ExpTmpOutVar_0,
                                            2);
                                        ho_SelectedContoursd.Dispose();
                                        ho_SelectedContoursd = ExpTmpOutVar_0;
                                    }

                                }

                            }


                            hv_lengthrigdcontour.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthrigdcontour = hv_Maxrigd - hv_Minrigd;
                            }
                            hv_lengthriglined.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthriglined = hv_ColEndrigd - hv_ColBeginrigd;
                            }
                            hv_Abslengthrigdcontour.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthrigdcontour, out hv_Abslengthrigdcontour);
                            hv_Abslengthriglined.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthriglined, out hv_Abslengthriglined);
                            if ((int)(new HTuple(((hv_Abslengthrigdcontour - hv_Abslengthriglined)).TupleGreater(
                                0.3))) != 0)
                            {
                                hv_countofCols.Dispose();
                                HOperatorSet.TupleLength(hv_Colsrigd, out hv_countofCols);
                                HTuple end_val690 = hv_countofCols - 1;
                                HTuple step_val690 = 1;
                                for (hv_Index = 1; hv_Index.Continue(end_val690, step_val690); hv_Index = hv_Index.TupleAdd(step_val690))
                                {
                                    if ((int)((new HTuple((((hv_Rowsrigd.TupleSelect(hv_Index)) - (hv_Rowsrigd.TupleSelect(
                                        hv_Index + 1)))).TupleLess(-0.02))).TupleAnd(new HTuple((((hv_Rowsrigd.TupleSelect(
                                        hv_Index + 1)) - (hv_Rowsrigd.TupleSelect(hv_Index + 2)))).TupleLess(
                                        -0.02)))) != 0)
                                    {
                                        break;
                                    }
                                }

                                hv_Rowsnewrigd.Dispose();
                                hv_Rowsnewrigd = new HTuple();
                                hv_Colsnewrigd.Dispose();
                                hv_Colsnewrigd = new HTuple();
                                if ((int)(new HTuple(hv_Index.TupleNotEqual(hv_countofCols - 1))) != 0)
                                {
                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val700 = hv_countofCols - 1;
                                    HTuple step_val700 = 1;
                                    for (hv_Index1 = hv_Index; hv_Index1.Continue(end_val700, step_val700); hv_Index1 = hv_Index1.TupleAdd(step_val700))
                                    {
                                        if (hv_Rowsnewrigd == null)
                                            hv_Rowsnewrigd = new HTuple();
                                        hv_Rowsnewrigd[hv_Indexx] = hv_Rowsrigd.TupleSelect(hv_Index1);
                                        if (hv_Colsnewrigd == null)
                                            hv_Colsnewrigd = new HTuple();
                                        hv_Colsnewrigd[hv_Indexx] = hv_Colsrigd.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_countofRowsd.Dispose();
                                    HOperatorSet.TupleLength(hv_Rowsmidd, out hv_countofRowsd);
                                    hv_Indexx.Dispose();
                                    hv_Indexx = new HTuple(hv_countofRowsd);
                                    HTuple end_val708 = hv_Index - 1;
                                    HTuple step_val708 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val708, step_val708); hv_Index1 = hv_Index1.TupleAdd(step_val708))
                                    {
                                        if (hv_Rowsmidd == null)
                                            hv_Rowsmidd = new HTuple();
                                        hv_Rowsmidd[hv_Indexx] = hv_Rowsrigd.TupleSelect(hv_Index1);
                                        if (hv_Colsmidd == null)
                                            hv_Colsmidd = new HTuple();
                                        hv_Colsmidd[hv_Indexx] = hv_Colsrigd.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    ho_contourmidd.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourmidd, hv_Rowsmidd,
                                        hv_Colsmidd);
                                    ho_contourrigd.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourrigd, hv_Rowsnewrigd,
                                        hv_Colsnewrigd);
                                    hv_RowBeginmidd.Dispose(); hv_ColBeginmidd.Dispose(); hv_RowEndmidd.Dispose(); hv_ColEndmidd.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourmidd, "drop", -1, 5, 5,
                                        1, out hv_RowBeginmidd, out hv_ColBeginmidd, out hv_RowEndmidd,
                                        out hv_ColEndmidd, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                    hv_RowBeginrigd.Dispose(); hv_ColBeginrigd.Dispose(); hv_RowEndrigd.Dispose(); hv_ColEndrigd.Dispose(); hv_Nrrigd.Dispose(); hv_Ncrigd.Dispose(); hv_Distrigd.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourrigd, "drop", -1, 5, 5,
                                        1, out hv_RowBeginrigd, out hv_ColBeginrigd, out hv_RowEndrigd,
                                        out hv_ColEndrigd, out hv_Nrrigd, out hv_Ncrigd, out hv_Distrigd);
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursd, ho_contourmidd, out ExpTmpOutVar_0,
                                            2);
                                        ho_SelectedContoursd.Dispose();
                                        ho_SelectedContoursd = ExpTmpOutVar_0;
                                    }
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursd, ho_contourrigd, out ExpTmpOutVar_0,
                                            3);
                                        ho_SelectedContoursd.Dispose();
                                        ho_SelectedContoursd = ExpTmpOutVar_0;
                                    }
                                }

                            }


                            //检查左侧3D采集图像，分割线段是否会出现折线
                            hv_Rowslefl.Dispose(); hv_Colslefl.Dispose();
                            HOperatorSet.GetContourXld(ho_contourlefl, out hv_Rowslefl, out hv_Colslefl);
                            hv_Rowsrigl.Dispose(); hv_Colsrigl.Dispose();
                            HOperatorSet.GetContourXld(ho_contourrigl, out hv_Rowsrigl, out hv_Colsrigl);
                            hv_Rowsmidl.Dispose(); hv_Colsmidl.Dispose();
                            HOperatorSet.GetContourXld(ho_contourmidl, out hv_Rowsmidl, out hv_Colsmidl);

                            hv_Minlefl.Dispose();
                            HOperatorSet.TupleMin(hv_Colslefl, out hv_Minlefl);
                            hv_Maxlefl.Dispose();
                            HOperatorSet.TupleMax(hv_Colslefl, out hv_Maxlefl);

                            hv_Minrigl.Dispose();
                            HOperatorSet.TupleMin(hv_Colsrigl, out hv_Minrigl);
                            hv_Maxrigl.Dispose();
                            HOperatorSet.TupleMax(hv_Colsrigl, out hv_Maxrigl);

                            hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrlefl.Dispose(); hv_Nclefl.Dispose(); hv_Distlefl.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourlefl, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl, out hv_ColEndlefl,
                                out hv_Nrlefl, out hv_Nclefl, out hv_Distlefl);
                            hv_RowBeginrigl.Dispose(); hv_ColBeginrigl.Dispose(); hv_RowEndrigl.Dispose(); hv_ColEndrigl.Dispose(); hv_Nrrigl.Dispose(); hv_Ncrigl.Dispose(); hv_Distrigl.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourrigl, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginrigl, out hv_ColBeginrigl, out hv_RowEndrigl, out hv_ColEndrigl,
                                out hv_Nrrigl, out hv_Ncrigl, out hv_Distrigl);

                            hv_lengthleflcontour.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthleflcontour = hv_Maxlefl - hv_Minlefl;
                            }
                            hv_lengthleflined.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthleflined = hv_ColEndlefl - hv_ColBeginlefl;
                            }
                            hv_Abslengthleflcontour.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthleflcontour, out hv_Abslengthleflcontour);
                            hv_Abslengthleflined.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthleflined, out hv_Abslengthleflined);
                            if ((int)(new HTuple(((hv_Abslengthleflcontour - hv_Abslengthleflined)).TupleGreater(
                                0.3))) != 0)
                            {
                                hv_countofCols.Dispose();
                                HOperatorSet.TupleLength(hv_Colslefl, out hv_countofCols);
                                HTuple end_val745 = 2;
                                HTuple step_val745 = -1;
                                for (hv_Index = hv_countofCols - 1; hv_Index.Continue(end_val745, step_val745); hv_Index = hv_Index.TupleAdd(step_val745))
                                {
                                    if ((int)((new HTuple((((hv_Rowslefl.TupleSelect(hv_Index)) - (hv_Rowslefl.TupleSelect(
                                        hv_Index - 1)))).TupleLess(-0.02))).TupleAnd(new HTuple((((hv_Rowslefl.TupleSelect(
                                        hv_Index - 1)) - (hv_Rowslefl.TupleSelect(hv_Index - 2)))).TupleLess(
                                        -0.02)))) != 0)
                                    {
                                        break;
                                    }
                                }

                                hv_Rowsnewlefl.Dispose();
                                hv_Rowsnewlefl = new HTuple();
                                hv_Colsnewlefl.Dispose();
                                hv_Colsnewlefl = new HTuple();
                                if ((int)(new HTuple(hv_Index.TupleGreater(2))) != 0)
                                {
                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val755 = hv_Index;
                                    HTuple step_val755 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val755, step_val755); hv_Index1 = hv_Index1.TupleAdd(step_val755))
                                    {
                                        if (hv_Rowsnewlefl == null)
                                            hv_Rowsnewlefl = new HTuple();
                                        hv_Rowsnewlefl[hv_Indexx] = hv_Rowslefl.TupleSelect(hv_Index1);
                                        if (hv_Colsnewlefl == null)
                                            hv_Colsnewlefl = new HTuple();
                                        hv_Colsnewlefl[hv_Indexx] = hv_Colslefl.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }



                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val764 = hv_countofCols - 1;
                                    HTuple step_val764 = 1;
                                    for (hv_Index1 = hv_Index; hv_Index1.Continue(end_val764, step_val764); hv_Index1 = hv_Index1.TupleAdd(step_val764))
                                    {
                                        if (hv_Rowsnewmidl == null)
                                            hv_Rowsnewmidl = new HTuple();
                                        hv_Rowsnewmidl[hv_Indexx] = hv_Rowslefl.TupleSelect(hv_Index1);
                                        if (hv_Colsnewmidl == null)
                                            hv_Colsnewmidl = new HTuple();
                                        hv_Colsnewmidl[hv_Indexx] = hv_Colslefl.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_countofRowsl.Dispose();
                                    HOperatorSet.TupleLength(hv_Rowsmidl, out hv_countofRowsl);
                                    HTuple end_val771 = hv_countofRowsl - 1;
                                    HTuple step_val771 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val771, step_val771); hv_Index1 = hv_Index1.TupleAdd(step_val771))
                                    {
                                        if (hv_Rowsnewmidl == null)
                                            hv_Rowsnewmidl = new HTuple();
                                        hv_Rowsnewmidl[hv_Indexx] = hv_Rowsmidl.TupleSelect(hv_Index1);
                                        if (hv_Colsnewmidl == null)
                                            hv_Colsnewmidl = new HTuple();
                                        hv_Colsnewmidl[hv_Indexx] = hv_Colsmidl.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    ho_contourlefl.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourlefl, hv_Rowsnewlefl,
                                        hv_Colsnewlefl);
                                    ho_contourmidl.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourmidl, hv_Rowsnewmidl,
                                        hv_Colsnewmidl);
                                    hv_RowBeginmidd.Dispose(); hv_ColBeginmidd.Dispose(); hv_RowEndmidd.Dispose(); hv_ColEndmidd.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourmidl, "drop", -1, 5, 5,
                                        1, out hv_RowBeginmidd, out hv_ColBeginmidd, out hv_RowEndmidd,
                                        out hv_ColEndmidd, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                    hv_RowBeginlefd.Dispose(); hv_ColBeginlefd.Dispose(); hv_RowEndlefd.Dispose(); hv_ColEndlefd.Dispose(); hv_Nrlefd.Dispose(); hv_Nclefd.Dispose(); hv_Distlefd.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourlefl, "drop", -1, 5, 5,
                                        1, out hv_RowBeginlefd, out hv_ColBeginlefd, out hv_RowEndlefd,
                                        out hv_ColEndlefd, out hv_Nrlefd, out hv_Nclefd, out hv_Distlefd);
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursl, ho_contourlefl, out ExpTmpOutVar_0,
                                            1);
                                        ho_SelectedContoursl.Dispose();
                                        ho_SelectedContoursl = ExpTmpOutVar_0;
                                    }
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursl, ho_contourmidl, out ExpTmpOutVar_0,
                                            2);
                                        ho_SelectedContoursl.Dispose();
                                        ho_SelectedContoursl = ExpTmpOutVar_0;
                                    }
                                }

                            }


                            hv_lengthrigdcontour.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthrigdcontour = hv_Maxrigl - hv_Minrigl;
                            }
                            hv_lengthriglined.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthriglined = hv_ColEndrigl - hv_ColBeginrigl;
                            }
                            hv_Abslengthrigdcontour.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthrigdcontour, out hv_Abslengthrigdcontour);
                            hv_Abslengthriglined.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthriglined, out hv_Abslengthriglined);

                            if ((int)(new HTuple(((hv_Abslengthrigdcontour - hv_Abslengthriglined)).TupleGreater(
                                0.5))) != 0)
                            {
                                hv_countofCols.Dispose();
                                HOperatorSet.TupleLength(hv_Rowsrigl, out hv_countofCols);
                                HTuple end_val795 = hv_countofCols - 1;
                                HTuple step_val795 = 1;
                                for (hv_Index = 1; hv_Index.Continue(end_val795, step_val795); hv_Index = hv_Index.TupleAdd(step_val795))
                                {
                                    if ((int)(new HTuple((((hv_Rowsrigl.TupleSelect(hv_Index)) - (hv_Rowsrigl.TupleSelect(
                                        hv_Index - 1)))).TupleLess(-0.02))) != 0)
                                    {
                                        break;
                                    }
                                }

                                hv_Rowsnewrigl.Dispose();
                                hv_Rowsnewrigl = new HTuple();
                                hv_Colsnewrigl.Dispose();
                                hv_Colsnewrigl = new HTuple();
                                if ((int)(new HTuple(hv_Index.TupleNotEqual(hv_countofCols))) != 0)
                                {
                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val805 = hv_countofCols - 1;
                                    HTuple step_val805 = 1;
                                    for (hv_Index1 = hv_Index; hv_Index1.Continue(end_val805, step_val805); hv_Index1 = hv_Index1.TupleAdd(step_val805))
                                    {
                                        if (hv_Rowsnewrigl == null)
                                            hv_Rowsnewrigl = new HTuple();
                                        hv_Rowsnewrigl[hv_Indexx] = hv_Rowsrigl.TupleSelect(hv_Index1);
                                        if (hv_Colsnewrigl == null)
                                            hv_Colsnewrigl = new HTuple();
                                        hv_Colsnewrigl[hv_Indexx] = hv_Colsrigl.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_countofRowsl.Dispose();
                                    HOperatorSet.TupleLength(hv_Rowsmidl, out hv_countofRowsl);
                                    hv_Indexx.Dispose();
                                    hv_Indexx = new HTuple(hv_countofRowsl);
                                    HTuple end_val813 = hv_Index - 1;
                                    HTuple step_val813 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val813, step_val813); hv_Index1 = hv_Index1.TupleAdd(step_val813))
                                    {
                                        if (hv_Rowsmidl == null)
                                            hv_Rowsmidl = new HTuple();
                                        hv_Rowsmidl[hv_Indexx] = hv_Rowsrigl.TupleSelect(hv_Index1);
                                        if (hv_Colsmidl == null)
                                            hv_Colsmidl = new HTuple();
                                        hv_Colsmidl[hv_Indexx] = hv_Colsrigl.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    ho_contourmidl.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourmidl, hv_Rowsmidl,
                                        hv_Colsmidl);
                                    ho_contourrigl.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourrigl, hv_Rowsnewrigl,
                                        hv_Colsnewrigl);
                                    hv_RowBeginmidl.Dispose(); hv_ColBeginmidl.Dispose(); hv_RowEndmidl.Dispose(); hv_ColEndmidl.Dispose(); hv_Nrmidl.Dispose(); hv_Ncmidl.Dispose(); hv_Distmidl.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourmidl, "drop", -1, 5, 5,
                                        1, out hv_RowBeginmidl, out hv_ColBeginmidl, out hv_RowEndmidl,
                                        out hv_ColEndmidl, out hv_Nrmidl, out hv_Ncmidl, out hv_Distmidl);
                                    hv_RowBeginrigl.Dispose(); hv_ColBeginrigl.Dispose(); hv_RowEndrigl.Dispose(); hv_ColEndrigl.Dispose(); hv_Nrrigl.Dispose(); hv_Ncrigl.Dispose(); hv_Distrigl.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourrigl, "drop", -1, 5, 5,
                                        1, out hv_RowBeginrigl, out hv_ColBeginrigl, out hv_RowEndrigl,
                                        out hv_ColEndrigl, out hv_Nrrigl, out hv_Ncrigl, out hv_Distrigl);
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursl, ho_contourmidl, out ExpTmpOutVar_0,
                                            2);
                                        ho_SelectedContoursl.Dispose();
                                        ho_SelectedContoursl = ExpTmpOutVar_0;
                                    }
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursl, ho_contourrigl, out ExpTmpOutVar_0,
                                            3);
                                        ho_SelectedContoursl.Dispose();
                                        ho_SelectedContoursl = ExpTmpOutVar_0;
                                    }
                                }
                            }


                            //检查右侧3D采集图像，分割线段是否会出现折线
                            hv_Rowslefr.Dispose(); hv_Colslefr.Dispose();
                            HOperatorSet.GetContourXld(ho_contourlefr, out hv_Rowslefr, out hv_Colslefr);
                            hv_Rowsrigr.Dispose(); hv_Colsrigr.Dispose();
                            HOperatorSet.GetContourXld(ho_contourrigr, out hv_Rowsrigr, out hv_Colsrigr);
                            hv_Rowsmidr.Dispose(); hv_Colsmidr.Dispose();
                            HOperatorSet.GetContourXld(ho_contourmidr, out hv_Rowsmidr, out hv_Colsmidr);

                            hv_Minlefr.Dispose();
                            HOperatorSet.TupleMin(hv_Colslefr, out hv_Minlefr);
                            hv_Maxlefr.Dispose();
                            HOperatorSet.TupleMax(hv_Colslefr, out hv_Maxlefr);

                            hv_Minrigr.Dispose();
                            HOperatorSet.TupleMin(hv_Colsrigr, out hv_Minrigr);
                            hv_Maxrigr.Dispose();
                            HOperatorSet.TupleMax(hv_Colsrigr, out hv_Maxrigr);

                            hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrlefr.Dispose(); hv_Nclefr.Dispose(); hv_Distlefr.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourlefr, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr, out hv_ColEndlefr,
                                out hv_Nrlefr, out hv_Nclefr, out hv_Distlefr);
                            hv_RowBeginrigr.Dispose(); hv_ColBeginrigr.Dispose(); hv_RowEndrigr.Dispose(); hv_ColEndrigr.Dispose(); hv_Nrrigr.Dispose(); hv_Ncrigr.Dispose(); hv_Distrigr.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourrigr, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginrigr, out hv_ColBeginrigr, out hv_RowEndrigr, out hv_ColEndrigr,
                                out hv_Nrrigr, out hv_Ncrigr, out hv_Distrigr);

                            hv_lengthlefrcontour.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthlefrcontour = hv_Minlefr - hv_Maxlefr;
                            }
                            hv_lengthleflined.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthleflined = hv_ColEndlefr - hv_ColBeginlefr;
                            }
                            hv_Abslengthlefrcontour.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthlefrcontour, out hv_Abslengthlefrcontour);
                            hv_Abslengthleflined.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthleflined, out hv_Abslengthleflined);
                            if ((int)(new HTuple(((hv_Abslengthlefrcontour - hv_Abslengthleflined)).TupleGreater(
                                0.3))) != 0)
                            {
                                hv_countofCols.Dispose();
                                HOperatorSet.TupleLength(hv_Colslefr, out hv_countofCols);
                                HTuple end_val849 = 2;
                                HTuple step_val849 = -1;
                                for (hv_Index = hv_countofCols - 1; hv_Index.Continue(end_val849, step_val849); hv_Index = hv_Index.TupleAdd(step_val849))
                                {
                                    if ((int)((new HTuple((((hv_Rowslefr.TupleSelect(hv_Index)) - (hv_Rowslefr.TupleSelect(
                                        hv_Index - 1)))).TupleLess(-0.02))).TupleAnd(new HTuple((((hv_Rowslefr.TupleSelect(
                                        hv_Index - 1)) - (hv_Rowslefr.TupleSelect(hv_Index - 2)))).TupleLess(
                                        -0.02)))) != 0)
                                    {
                                        break;
                                    }
                                }

                                hv_Rowsnewlefr.Dispose();
                                hv_Rowsnewlefr = new HTuple();
                                hv_Colsnewlefr.Dispose();
                                hv_Colsnewlefr = new HTuple();
                                if ((int)(new HTuple(hv_Index.TupleGreater(2))) != 0)
                                {
                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val859 = hv_Index;
                                    HTuple step_val859 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val859, step_val859); hv_Index1 = hv_Index1.TupleAdd(step_val859))
                                    {
                                        if (hv_Rowsnewlefr == null)
                                            hv_Rowsnewlefr = new HTuple();
                                        hv_Rowsnewlefr[hv_Indexx] = hv_Rowslefr.TupleSelect(hv_Index1);
                                        if (hv_Colsnewlefr == null)
                                            hv_Colsnewlefr = new HTuple();
                                        hv_Colsnewlefr[hv_Indexx] = hv_Colslefr.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }


                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val867 = hv_countofCols - 1;
                                    HTuple step_val867 = 1;
                                    for (hv_Index1 = hv_Index; hv_Index1.Continue(end_val867, step_val867); hv_Index1 = hv_Index1.TupleAdd(step_val867))
                                    {
                                        if (hv_Rowsnewmidr == null)
                                            hv_Rowsnewmidr = new HTuple();
                                        hv_Rowsnewmidr[hv_Indexx] = hv_Rowslefr.TupleSelect(hv_Index1);
                                        if (hv_Colsnewmidr == null)
                                            hv_Colsnewmidr = new HTuple();
                                        hv_Colsnewmidr[hv_Indexx] = hv_Colslefr.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_countofRowsr.Dispose();
                                    HOperatorSet.TupleLength(hv_Rowsmidr, out hv_countofRowsr);
                                    HTuple end_val874 = hv_countofRowsr - 1;
                                    HTuple step_val874 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val874, step_val874); hv_Index1 = hv_Index1.TupleAdd(step_val874))
                                    {
                                        if (hv_Rowsnewmidr == null)
                                            hv_Rowsnewmidr = new HTuple();
                                        hv_Rowsnewmidr[hv_Indexx] = hv_Rowsmidr.TupleSelect(hv_Index1);
                                        if (hv_Colsnewmidr == null)
                                            hv_Colsnewmidr = new HTuple();
                                        hv_Colsnewmidr[hv_Indexx] = hv_Colsmidr.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    ho_contourlefr.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourlefr, hv_Rowsnewlefr,
                                        hv_Colsnewlefr);
                                    ho_contourmidr.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourmidr, hv_Rowsnewmidr,
                                        hv_Colsnewmidr);
                                    hv_RowBeginmidr.Dispose(); hv_ColBeginmidr.Dispose(); hv_RowEndmidr.Dispose(); hv_ColEndmidr.Dispose(); hv_Nrmidr.Dispose(); hv_Ncmidr.Dispose(); hv_Distmidr.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourmidr, "drop", -1, 5, 5,
                                        1, out hv_RowBeginmidr, out hv_ColBeginmidr, out hv_RowEndmidr,
                                        out hv_ColEndmidr, out hv_Nrmidr, out hv_Ncmidr, out hv_Distmidr);
                                    hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrlefr.Dispose(); hv_Nclefr.Dispose(); hv_Distlefr.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourlefr, "drop", -1, 5, 5,
                                        1, out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr,
                                        out hv_ColEndlefr, out hv_Nrlefr, out hv_Nclefr, out hv_Distlefr);
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursr, ho_contourlefr, out ExpTmpOutVar_0,
                                            1);
                                        ho_SelectedContoursr.Dispose();
                                        ho_SelectedContoursr = ExpTmpOutVar_0;
                                    }
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursr, ho_contourmidr, out ExpTmpOutVar_0,
                                            2);
                                        ho_SelectedContoursr.Dispose();
                                        ho_SelectedContoursr = ExpTmpOutVar_0;
                                    }
                                }

                            }


                            hv_lengthrigrcontour.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthrigrcontour = hv_Maxrigr - hv_Minrigr;
                            }
                            hv_lengthriglined.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthriglined = hv_ColEndrigr - hv_ColBeginrigr;
                            }
                            hv_Abslengthrigrcontour.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthrigrcontour, out hv_Abslengthrigrcontour);
                            hv_Abslengthriglined.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthriglined, out hv_Abslengthriglined);

                            if ((int)(new HTuple(((hv_Abslengthrigrcontour - hv_Abslengthriglined)).TupleGreater(
                                0.5))) != 0)
                            {
                                hv_countofColr.Dispose();
                                HOperatorSet.TupleLength(hv_Rowsrigr, out hv_countofColr);
                                HTuple end_val898 = hv_countofCols - 1;
                                HTuple step_val898 = 1;
                                for (hv_Index = 1; hv_Index.Continue(end_val898, step_val898); hv_Index = hv_Index.TupleAdd(step_val898))
                                {
                                    if ((int)(new HTuple((((hv_Rowsrigr.TupleSelect(hv_Index)) - (hv_Rowsrigr.TupleSelect(
                                        hv_Index + 1)))).TupleLess(-0.02))) != 0)
                                    {
                                        break;
                                    }
                                }

                                hv_Rowsnewrigr.Dispose();
                                hv_Rowsnewrigr = new HTuple();
                                hv_Colsnewrigr.Dispose();
                                hv_Colsnewrigr = new HTuple();
                                if ((int)(new HTuple(hv_Index.TupleNotEqual(hv_countofColr))) != 0)
                                {
                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val908 = hv_countofColr - 1;
                                    HTuple step_val908 = 1;
                                    for (hv_Index1 = hv_Index; hv_Index1.Continue(end_val908, step_val908); hv_Index1 = hv_Index1.TupleAdd(step_val908))
                                    {
                                        if (hv_Rowsnewrigr == null)
                                            hv_Rowsnewrigr = new HTuple();
                                        hv_Rowsnewrigr[hv_Indexx] = hv_Rowsrigr.TupleSelect(hv_Index1);
                                        if (hv_Colsnewrigr == null)
                                            hv_Colsnewrigr = new HTuple();
                                        hv_Colsnewrigr[hv_Indexx] = hv_Colsrigr.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_countofRowsr.Dispose();
                                    HOperatorSet.TupleLength(hv_Rowsmidr, out hv_countofRowsr);
                                    hv_Indexx.Dispose();
                                    hv_Indexx = new HTuple(hv_countofRowsr);
                                    HTuple end_val916 = hv_Index - 1;
                                    HTuple step_val916 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val916, step_val916); hv_Index1 = hv_Index1.TupleAdd(step_val916))
                                    {
                                        if (hv_Rowsmidr == null)
                                            hv_Rowsmidr = new HTuple();
                                        hv_Rowsmidr[hv_Indexx] = hv_Rowsrigr.TupleSelect(hv_Index1);
                                        if (hv_Colsmidr == null)
                                            hv_Colsmidr = new HTuple();
                                        hv_Colsmidr[hv_Indexx] = hv_Colsrigr.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    ho_contourmidr.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourmidr, hv_Rowsmidr,
                                        hv_Colsmidr);
                                    ho_contourrigr.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourrigr, hv_Rowsnewrigr,
                                        hv_Colsnewrigr);
                                    hv_RowBeginmidr.Dispose(); hv_ColBeginmidr.Dispose(); hv_RowEndmidr.Dispose(); hv_ColEndmidr.Dispose(); hv_Nrmidr.Dispose(); hv_Ncmidr.Dispose(); hv_Distmidr.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourmidr, "drop", -1, 5, 5,
                                        1, out hv_RowBeginmidr, out hv_ColBeginmidr, out hv_RowEndmidr,
                                        out hv_ColEndmidr, out hv_Nrmidr, out hv_Ncmidr, out hv_Distmidr);
                                    hv_RowBeginrigr.Dispose(); hv_ColBeginrigr.Dispose(); hv_RowEndrigr.Dispose(); hv_ColEndrigr.Dispose(); hv_Nrrigr.Dispose(); hv_Ncrigr.Dispose(); hv_Distrigr.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourrigr, "drop", -1, 5, 5,
                                        1, out hv_RowBeginrigr, out hv_ColBeginrigr, out hv_RowEndrigr,
                                        out hv_ColEndrigr, out hv_Nrrigr, out hv_Ncrigr, out hv_Distrigr);
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursr, ho_contourmidr, out ExpTmpOutVar_0,
                                            2);
                                        ho_SelectedContoursr.Dispose();
                                        ho_SelectedContoursr = ExpTmpOutVar_0;
                                    }
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursr, ho_contourrigr, out ExpTmpOutVar_0,
                                            3);
                                        ho_SelectedContoursr.Dispose();
                                        ho_SelectedContoursr = ExpTmpOutVar_0;
                                    }
                                }
                            }



                            //说明截断的contourleft， 最大的Column值多了，出现最右边不是斜线，有横线部分，需要截断一部分
                            //contourmidt 需要增加一部分


                            hv_Rowsleft.Dispose(); hv_Colsleft.Dispose();
                            HOperatorSet.GetContourXld(ho_contourleft, out hv_Rowsleft, out hv_Colsleft);
                            hv_Rowsrigt.Dispose(); hv_Colsrigt.Dispose();
                            HOperatorSet.GetContourXld(ho_contourrigt, out hv_Rowsrigt, out hv_Colsrigt);

                            hv_Minl.Dispose();
                            HOperatorSet.TupleMin(hv_Colsleft, out hv_Minl);
                            hv_Minr.Dispose();
                            HOperatorSet.TupleMin(hv_Colsrigt, out hv_Minr);

                            if ((int)(new HTuple(hv_Minl.TupleGreater(hv_Minr))) != 0)
                            {
                                ho_contourtmp.Dispose();
                                ho_contourtmp = new HObject(ho_contourleft);
                                ho_contourleft.Dispose();
                                ho_contourleft = new HObject(ho_contourrigt);
                                ho_contourrigt.Dispose();
                                ho_contourrigt = new HObject(ho_contourtmp);
                            }

                            hv_Rowslefl.Dispose(); hv_Colslefl.Dispose();
                            HOperatorSet.GetContourXld(ho_contourlefd, out hv_Rowslefl, out hv_Colslefl);
                            hv_Rowsrigr.Dispose(); hv_Colsrigr.Dispose();
                            HOperatorSet.GetContourXld(ho_contourrigd, out hv_Rowsrigr, out hv_Colsrigr);

                            hv_Minl.Dispose();
                            HOperatorSet.TupleMin(hv_Colslefl, out hv_Minl);
                            hv_Minr.Dispose();
                            HOperatorSet.TupleMin(hv_Colsrigr, out hv_Minr);
                            if ((int)(new HTuple(hv_Minl.TupleGreater(hv_Minr))) != 0)
                            {
                                ho_contourtmp.Dispose();
                                ho_contourtmp = new HObject(ho_contourlefd);
                                ho_contourlefd.Dispose();
                                ho_contourlefd = new HObject(ho_contourrigd);
                                ho_contourrigd.Dispose();
                                ho_contourrigd = new HObject(ho_contourtmp);
                            }

                            hv_Rowslefl.Dispose(); hv_Colslefl.Dispose();
                            HOperatorSet.GetContourXld(ho_contourlefl, out hv_Rowslefl, out hv_Colslefl);
                            hv_Rowsrigr.Dispose(); hv_Colsrigr.Dispose();
                            HOperatorSet.GetContourXld(ho_contourrigl, out hv_Rowsrigr, out hv_Colsrigr);

                            hv_Minl.Dispose();
                            HOperatorSet.TupleMin(hv_Colslefl, out hv_Minl);
                            hv_Minr.Dispose();
                            HOperatorSet.TupleMin(hv_Colsrigr, out hv_Minr);
                            if ((int)(new HTuple(hv_Minl.TupleGreater(hv_Minr))) != 0)
                            {
                                ho_contourtmp.Dispose();
                                ho_contourtmp = new HObject(ho_contourlefl);
                                ho_contourlefl.Dispose();
                                ho_contourlefl = new HObject(ho_contourrigl);
                                ho_contourrigl.Dispose();
                                ho_contourrigl = new HObject(ho_contourtmp);
                            }

                            hv_Rowslefl.Dispose(); hv_Colslefl.Dispose();
                            HOperatorSet.GetContourXld(ho_contourlefr, out hv_Rowslefl, out hv_Colslefl);
                            hv_Rowsrigr.Dispose(); hv_Colsrigr.Dispose();
                            HOperatorSet.GetContourXld(ho_contourrigr, out hv_Rowsrigr, out hv_Colsrigr);

                            hv_Minl.Dispose();
                            HOperatorSet.TupleMin(hv_Colslefl, out hv_Minl);
                            hv_Minr.Dispose();
                            HOperatorSet.TupleMin(hv_Colsrigr, out hv_Minr);
                            if ((int)(new HTuple(hv_Minl.TupleGreater(hv_Minr))) != 0)
                            {
                                ho_contourtmp.Dispose();
                                ho_contourtmp = new HObject(ho_contourlefr);
                                ho_contourlefr.Dispose();
                                ho_contourlefr = new HObject(ho_contourrigr);
                                ho_contourrigr.Dispose();
                                ho_contourrigr = new HObject(ho_contourtmp);
                            }



                            hv_RowBeginmidt.Dispose(); hv_ColBeginmidt.Dispose(); hv_RowEndmidt.Dispose(); hv_ColEndmidt.Dispose(); hv_Nrmidt.Dispose(); hv_Ncmidt.Dispose(); hv_Distmidt.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourmidt, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginmidt, out hv_ColBeginmidt, out hv_RowEndmidt, out hv_ColEndmidt,
                                out hv_Nrmidt, out hv_Ncmidt, out hv_Distmidt);
                            hv_RowBeginmidd.Dispose(); hv_ColBeginmidd.Dispose(); hv_RowEndmidd.Dispose(); hv_ColEndmidd.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourmidd, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginmidd, out hv_ColBeginmidd, out hv_RowEndmidd, out hv_ColEndmidd,
                                out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                            hv_RowBeginmidl.Dispose(); hv_ColBeginmidl.Dispose(); hv_RowEndmidl.Dispose(); hv_ColEndmidl.Dispose(); hv_Nrmidl.Dispose(); hv_Ncmidl.Dispose(); hv_Distmidl.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourmidl, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginmidl, out hv_ColBeginmidl, out hv_RowEndmidl, out hv_ColEndmidl,
                                out hv_Nrmidl, out hv_Ncmidl, out hv_Distmidl);
                            hv_RowBeginmidr.Dispose(); hv_ColBeginmidr.Dispose(); hv_RowEndmidr.Dispose(); hv_ColEndmidr.Dispose(); hv_Nrmidr.Dispose(); hv_Ncmidr.Dispose(); hv_Distmidr.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourmidr, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginmidr, out hv_ColBeginmidr, out hv_RowEndmidr, out hv_ColEndmidr,
                                out hv_Nrmidr, out hv_Ncmidr, out hv_Distmidr);

                            hv_RowBeginleft.Dispose(); hv_ColBeginleft.Dispose(); hv_RowEndleft.Dispose(); hv_ColEndleft.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourleft, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginleft, out hv_ColBeginleft, out hv_RowEndleft, out hv_ColEndleft,
                                out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                            hv_RowBeginlefd.Dispose(); hv_ColBeginlefd.Dispose(); hv_RowEndlefd.Dispose(); hv_ColEndlefd.Dispose(); hv_Nrlefd.Dispose(); hv_Nclefd.Dispose(); hv_Distlefd.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourlefd, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginlefd, out hv_ColBeginlefd, out hv_RowEndlefd, out hv_ColEndlefd,
                                out hv_Nrlefd, out hv_Nclefd, out hv_Distlefd);
                            hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrlefl.Dispose(); hv_Nclefl.Dispose(); hv_Distlefl.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourlefl, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl, out hv_ColEndlefl,
                                out hv_Nrlefl, out hv_Nclefl, out hv_Distlefl);
                            hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrlefr.Dispose(); hv_Nclefr.Dispose(); hv_Distlefr.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourlefr, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr, out hv_ColEndlefr,
                                out hv_Nrlefr, out hv_Nclefr, out hv_Distlefr);

                            hv_RowBeginrigt.Dispose(); hv_ColBeginrigt.Dispose(); hv_RowEndrigt.Dispose(); hv_ColEndrigt.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigt.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourrigt, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginrigt, out hv_ColBeginrigt, out hv_RowEndrigt, out hv_ColEndrigt,
                                out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigt);
                            hv_RowBeginrigd.Dispose(); hv_ColBeginrigd.Dispose(); hv_RowEndrigd.Dispose(); hv_ColEndrigd.Dispose(); hv_Nrrigd.Dispose(); hv_Ncrigd.Dispose(); hv_Distrigd.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourrigd, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginrigd, out hv_ColBeginrigd, out hv_RowEndrigd, out hv_ColEndrigd,
                                out hv_Nrrigd, out hv_Ncrigd, out hv_Distrigd);
                            hv_RowBeginrigl.Dispose(); hv_ColBeginrigl.Dispose(); hv_RowEndrigl.Dispose(); hv_ColEndrigl.Dispose(); hv_Nrrigl.Dispose(); hv_Ncrigl.Dispose(); hv_Distrigl.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourrigl, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginrigl, out hv_ColBeginrigl, out hv_RowEndrigl, out hv_ColEndrigl,
                                out hv_Nrrigl, out hv_Ncrigl, out hv_Distrigl);
                            hv_RowBeginrigr.Dispose(); hv_ColBeginrigr.Dispose(); hv_RowEndrigr.Dispose(); hv_ColEndrigr.Dispose(); hv_Nrrigr.Dispose(); hv_Ncrigr.Dispose(); hv_Distrigr.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourrigr, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginrigr, out hv_ColBeginrigr, out hv_RowEndrigr, out hv_ColEndrigr,
                                out hv_Nrrigr, out hv_Ncrigr, out hv_Distrigr);


                            hv_top_diag.Dispose();
                            HOperatorSet.DistancePp(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                hv_ColEndmidt, out hv_top_diag);
                            hv_down_diag.Dispose();
                            HOperatorSet.DistancePp(hv_RowBeginmidd, hv_ColBeginmidd, hv_RowEndmidd,
                                hv_ColEndmidd, out hv_down_diag);
                            hv_left_diag.Dispose();
                            HOperatorSet.DistancePp(hv_RowBeginmidl, hv_ColBeginmidl, hv_RowEndmidl,
                                hv_ColEndmidl, out hv_left_diag);
                            hv_right_diag.Dispose();
                            HOperatorSet.DistancePp(hv_RowBeginmidr, hv_ColBeginmidr, hv_RowEndmidr,
                                hv_ColEndmidr, out hv_right_diag);

                            {
                                HTuple ExpTmpOutVar_0;
                                HOperatorSet.TupleConcat(hv_DistancesTopDiag, hv_top_diag, out ExpTmpOutVar_0);
                                hv_DistancesTopDiag.Dispose();
                                hv_DistancesTopDiag = ExpTmpOutVar_0;
                            }
                            {
                                HTuple ExpTmpOutVar_0;
                                HOperatorSet.TupleConcat(hv_DistancesDownDiag, hv_down_diag, out ExpTmpOutVar_0);
                                hv_DistancesDownDiag.Dispose();
                                hv_DistancesDownDiag = ExpTmpOutVar_0;
                            }
                            {
                                HTuple ExpTmpOutVar_0;
                                HOperatorSet.TupleConcat(hv_DistancesLeftDiag, hv_left_diag, out ExpTmpOutVar_0);
                                hv_DistancesLeftDiag.Dispose();
                                hv_DistancesLeftDiag = ExpTmpOutVar_0;
                            }
                            {
                                HTuple ExpTmpOutVar_0;
                                HOperatorSet.TupleConcat(hv_DistancesRightDiag, hv_right_diag, out ExpTmpOutVar_0);
                                hv_DistancesRightDiag.Dispose();
                                hv_DistancesRightDiag = ExpTmpOutVar_0;
                            }

                            hv_RowIntersectionlpt.Dispose(); hv_ColIntersectionlpt.Dispose(); hv_IsOverlapping.Dispose();
                            HOperatorSet.IntersectionLines(hv_RowBeginlefl, hv_ColBeginlefl, hv_RowEndlefl,
                                hv_ColEndlefl, hv_RowBeginrigl, hv_ColBeginrigl, hv_RowEndrigl, hv_ColEndrigl,
                                out hv_RowIntersectionlpt, out hv_ColIntersectionlpt, out hv_IsOverlapping);
                            hv_RowIntersectiontpt.Dispose(); hv_ColIntersectiontpt.Dispose(); hv_IsOverlapping.Dispose();
                            HOperatorSet.IntersectionLines(hv_RowBeginleft, hv_ColBeginleft, hv_RowEndleft,
                                hv_ColEndleft, hv_RowBeginrigt, hv_ColBeginrigt, hv_RowEndrigt, hv_ColEndrigt,
                                out hv_RowIntersectiontpt, out hv_ColIntersectiontpt, out hv_IsOverlapping);
                            hv_RowIntersectiondpt.Dispose(); hv_ColIntersectiondpt.Dispose(); hv_IsOverlapping.Dispose();
                            HOperatorSet.IntersectionLines(hv_RowBeginlefd, hv_ColBeginlefd, hv_RowEndlefd,
                                hv_ColEndlefd, hv_RowBeginrigd, hv_ColBeginrigd, hv_RowEndrigd, hv_ColEndrigd,
                                out hv_RowIntersectiondpt, out hv_ColIntersectiondpt, out hv_IsOverlapping);
                            hv_RowIntersectionrpt.Dispose(); hv_ColIntersectionrpt.Dispose(); hv_IsOverlapping.Dispose();
                            HOperatorSet.IntersectionLines(hv_RowBeginlefr, hv_ColBeginlefr, hv_RowEndlefr,
                                hv_ColEndlefr, hv_RowBeginrigr, hv_ColBeginrigr, hv_RowEndrigr, hv_ColEndrigr,
                                out hv_RowIntersectionrpt, out hv_ColIntersectionrpt, out hv_IsOverlapping);


                            if ((int)(new HTuple(hv_RowBeginleft.TupleGreater(hv_RowEndleft))) != 0)
                            {
                                hv_dis_topLeftDiag.Dispose();
                                HOperatorSet.DistancePp(hv_RowEndleft, hv_ColEndleft, hv_RowIntersectiontpt,
                                    hv_ColIntersectiontpt, out hv_dis_topLeftDiag);
                            }
                            else
                            {
                                hv_dis_topLeftDiag.Dispose();
                                HOperatorSet.DistancePp(hv_RowBeginleft, hv_ColBeginleft, hv_RowIntersectiontpt,
                                    hv_ColIntersectiontpt, out hv_dis_topLeftDiag);
                            }

                            if ((int)(new HTuple(hv_RowBeginrigt.TupleGreater(hv_RowEndrigt))) != 0)
                            {
                                hv_dis_topRightDiag.Dispose();
                                HOperatorSet.DistancePp(hv_RowEndrigt, hv_ColEndrigt, hv_RowIntersectiontpt,
                                    hv_ColIntersectiontpt, out hv_dis_topRightDiag);
                            }
                            else
                            {
                                hv_dis_topRightDiag.Dispose();
                                HOperatorSet.DistancePp(hv_RowBeginrigt, hv_ColBeginrigt, hv_RowIntersectiontpt,
                                    hv_ColIntersectiontpt, out hv_dis_topRightDiag);
                            }

                            {
                                HTuple ExpTmpOutVar_0;
                                HOperatorSet.TupleConcat(hv_dis_topLeftDiag, hv_DistancesTopLeftDiag,
                                    out ExpTmpOutVar_0);
                                hv_DistancesTopLeftDiag.Dispose();
                                hv_DistancesTopLeftDiag = ExpTmpOutVar_0;
                            }
                            {
                                HTuple ExpTmpOutVar_0;
                                HOperatorSet.TupleConcat(hv_dis_topRightDiag, hv_DistancesTopRightDiag,
                                    out ExpTmpOutVar_0);
                                hv_DistancesTopRightDiag.Dispose();
                                hv_DistancesTopRightDiag = ExpTmpOutVar_0;
                            }


                            if ((int)(new HTuple(hv_RowBeginlefd.TupleGreater(hv_RowEndlefd))) != 0)
                            {
                                hv_dis_downLeftDiag.Dispose();
                                HOperatorSet.DistancePp(hv_RowEndlefd, hv_ColEndlefd, hv_RowIntersectiondpt,
                                    hv_ColIntersectiondpt, out hv_dis_downLeftDiag);
                            }
                            else
                            {
                                hv_dis_downLeftDiag.Dispose();
                                HOperatorSet.DistancePp(hv_RowBeginlefd, hv_ColBeginlefd, hv_RowIntersectiondpt,
                                    hv_ColIntersectiondpt, out hv_dis_downLeftDiag);
                            }

                            if ((int)(new HTuple(hv_RowBeginrigd.TupleGreater(hv_RowEndrigd))) != 0)
                            {
                                hv_dis_downRightDiag.Dispose();
                                HOperatorSet.DistancePp(hv_RowEndrigd, hv_ColEndrigd, hv_RowIntersectiondpt,
                                    hv_ColIntersectiondpt, out hv_dis_downRightDiag);
                            }
                            else
                            {
                                hv_dis_downRightDiag.Dispose();
                                HOperatorSet.DistancePp(hv_RowBeginrigd, hv_ColBeginrigd, hv_RowIntersectiondpt,
                                    hv_ColIntersectiondpt, out hv_dis_downRightDiag);
                            }

                            {
                                HTuple ExpTmpOutVar_0;
                                HOperatorSet.TupleConcat(hv_dis_downLeftDiag, hv_DistancesDownLeftDiag,
                                    out ExpTmpOutVar_0);
                                hv_DistancesDownLeftDiag.Dispose();
                                hv_DistancesDownLeftDiag = ExpTmpOutVar_0;
                            }
                            {
                                HTuple ExpTmpOutVar_0;
                                HOperatorSet.TupleConcat(hv_dis_downRightDiag, hv_DistancesDownRightDiag,
                                    out ExpTmpOutVar_0);
                                hv_DistancesDownRightDiag.Dispose();
                                hv_DistancesDownRightDiag = ExpTmpOutVar_0;
                            }

                            if ((int)(new HTuple(hv_RowBeginlefl.TupleGreater(hv_RowEndlefl))) != 0)
                            {
                                hv_dis_leftLeftDiag.Dispose();
                                HOperatorSet.DistancePp(hv_RowEndlefl, hv_ColEndlefl, hv_RowIntersectionlpt,
                                    hv_ColIntersectionlpt, out hv_dis_leftLeftDiag);
                            }
                            else
                            {
                                hv_dis_leftLeftDiag.Dispose();
                                HOperatorSet.DistancePp(hv_RowBeginlefl, hv_ColBeginlefl, hv_RowIntersectionlpt,
                                    hv_ColIntersectionlpt, out hv_dis_leftLeftDiag);
                            }

                            if ((int)(new HTuple(hv_RowBeginrigl.TupleGreater(hv_RowEndrigl))) != 0)
                            {
                                hv_dis_leftRightDiag.Dispose();
                                HOperatorSet.DistancePp(hv_RowEndrigl, hv_ColEndrigl, hv_RowIntersectionlpt,
                                    hv_ColIntersectionlpt, out hv_dis_leftRightDiag);
                            }
                            else
                            {
                                hv_dis_leftRightDiag.Dispose();
                                HOperatorSet.DistancePp(hv_RowBeginrigl, hv_ColBeginrigl, hv_RowIntersectionlpt,
                                    hv_ColIntersectionlpt, out hv_dis_leftRightDiag);
                            }
                            {
                                HTuple ExpTmpOutVar_0;
                                HOperatorSet.TupleConcat(hv_dis_leftLeftDiag, hv_DistancesLeftLeftDiag,
                                    out ExpTmpOutVar_0);
                                hv_DistancesLeftLeftDiag.Dispose();
                                hv_DistancesLeftLeftDiag = ExpTmpOutVar_0;
                            }
                            {
                                HTuple ExpTmpOutVar_0;
                                HOperatorSet.TupleConcat(hv_dis_leftRightDiag, hv_DistancesLeftRightDiag,
                                    out ExpTmpOutVar_0);
                                hv_DistancesLeftRightDiag.Dispose();
                                hv_DistancesLeftRightDiag = ExpTmpOutVar_0;
                            }


                            if ((int)(new HTuple(hv_RowBeginlefr.TupleGreater(hv_RowEndlefr))) != 0)
                            {
                                hv_dis_rightLeftDiag.Dispose();
                                HOperatorSet.DistancePp(hv_RowEndlefr, hv_ColEndlefr, hv_RowIntersectionrpt,
                                    hv_ColIntersectionrpt, out hv_dis_rightLeftDiag);
                            }
                            else
                            {
                                hv_dis_rightLeftDiag.Dispose();
                                HOperatorSet.DistancePp(hv_RowBeginlefr, hv_ColBeginlefr, hv_RowIntersectionrpt,
                                    hv_ColIntersectionrpt, out hv_dis_rightLeftDiag);
                            }

                            if ((int)(new HTuple(hv_RowBeginrigr.TupleGreater(hv_RowEndrigr))) != 0)
                            {
                                hv_dis_rightRightDiag.Dispose();
                                HOperatorSet.DistancePp(hv_RowEndrigr, hv_ColEndrigr, hv_RowIntersectionrpt,
                                    hv_ColIntersectionrpt, out hv_dis_rightRightDiag);
                            }
                            else
                            {
                                hv_dis_rightRightDiag.Dispose();
                                HOperatorSet.DistancePp(hv_RowBeginrigr, hv_ColBeginrigr, hv_RowIntersectionrpt,
                                    hv_ColIntersectionrpt, out hv_dis_rightRightDiag);
                            }
                            {
                                HTuple ExpTmpOutVar_0;
                                HOperatorSet.TupleConcat(hv_dis_rightLeftDiag, hv_DistancesRightLeftDiag,
                                    out ExpTmpOutVar_0);
                                hv_DistancesRightLeftDiag.Dispose();
                                hv_DistancesRightLeftDiag = ExpTmpOutVar_0;
                            }
                            {
                                HTuple ExpTmpOutVar_0;
                                HOperatorSet.TupleConcat(hv_dis_rightRightDiag, hv_DistancesRightRightDiag,
                                    out ExpTmpOutVar_0);
                                hv_DistancesRightRightDiag.Dispose();
                                hv_DistancesRightRightDiag = ExpTmpOutVar_0;
                            }











                            if ((int)(new HTuple(hv_RowBeginleft.TupleGreater(hv_RowEndleft))) != 0)
                            {
                                hv_Angleradttl.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                    hv_ColEndmidt, hv_RowBeginleft, hv_ColBeginleft, hv_RowEndleft,
                                    hv_ColEndleft, out hv_Angleradttl);
                            }
                            else
                            {
                                hv_Angleradttl.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                    hv_ColEndmidt, hv_RowEndleft, hv_ColEndleft, hv_RowBeginleft, hv_ColBeginleft,
                                    out hv_Angleradttl);
                            }
                            hv_Degtl.Dispose();
                            HOperatorSet.TupleDeg(hv_Angleradttl, out hv_Degtl);
                            hv_DegtlAbs.Dispose();
                            HOperatorSet.TupleAbs(hv_Degtl, out hv_DegtlAbs);
                            hv_newttl.Dispose();
                            HOperatorSet.TupleAbs(hv_Angleradttl, out hv_newttl);
                            hv_verllength.Dispose();
                            HOperatorSet.TupleSin(hv_newttl, out hv_verllength);
                            hv_distop_verlength.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_distop_verlength = hv_dis_topLeftDiag * hv_verllength;
                            }


                            {
                                HTuple ExpTmpOutVar_0;
                                HOperatorSet.TupleConcat(hv_dis_topRightDiag, hv_DistancesTopRightDiag,
                                    out ExpTmpOutVar_0);
                                hv_DistancesTopRightDiag.Dispose();
                                hv_DistancesTopRightDiag = ExpTmpOutVar_0;
                            }

                            if ((int)(new HTuple(hv_RowBeginrigt.TupleLess(hv_RowEndrigt))) != 0)
                            {
                                hv_Angleradtr.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                    hv_ColEndmidt, hv_RowBeginrigt, hv_ColBeginrigt, hv_RowEndrigt,
                                    hv_ColEndrigt, out hv_Angleradtr);
                            }
                            else
                            {
                                hv_Angleradtr.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                    hv_ColEndmidt, hv_RowEndrigt, hv_ColEndrigt, hv_RowBeginrigt, hv_ColBeginrigt,
                                    out hv_Angleradtr);
                            }
                            hv_Degtr.Dispose();
                            HOperatorSet.TupleDeg(hv_Angleradtr, out hv_Degtr);
                            hv_DegtrAbs.Dispose();
                            HOperatorSet.TupleAbs(hv_Degtr, out hv_DegtrAbs);

                            if ((int)(new HTuple(hv_RowBeginlefd.TupleGreater(hv_RowEndlefd))) != 0)
                            {
                                hv_Angleraddtl.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidd, hv_ColBeginmidd, hv_RowEndmidd,
                                    hv_ColEndmidd, hv_RowBeginlefd, hv_ColBeginlefd, hv_RowEndlefd,
                                    hv_ColEndlefd, out hv_Angleraddtl);
                                //angle_lx (RowBeginlefd, ColBeginlefd, RowEndlefd, ColEndlefd, Angleraddtl)
                            }
                            else
                            {
                                hv_Angleraddtl.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidd, hv_ColBeginmidd, hv_RowEndmidd,
                                    hv_ColEndmidd, hv_RowEndlefd, hv_ColEndlefd, hv_RowBeginlefd, hv_ColBeginlefd,
                                    out hv_Angleraddtl);
                                //angle_lx (RowEndlefd, ColEndlefd, RowBeginlefd, ColBeginlefd, Angleraddtl)

                            }
                            hv_Degdl.Dispose();
                            HOperatorSet.TupleDeg(hv_Angleraddtl, out hv_Degdl);
                            hv_DegdlAbs.Dispose();
                            HOperatorSet.TupleAbs(hv_Degdl, out hv_DegdlAbs);
                            hv_newttl.Dispose();
                            HOperatorSet.TupleAbs(hv_Angleraddtl, out hv_newttl);
                            hv_verldlength.Dispose();
                            HOperatorSet.TupleSin(hv_newttl, out hv_verldlength);
                            hv_disdown_verlength.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_disdown_verlength = hv_dis_downLeftDiag * hv_verldlength;
                            }


                            if ((int)(new HTuple(hv_RowBeginrigd.TupleLess(hv_RowEndrigd))) != 0)
                            {
                                hv_Angleraddtr.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidd, hv_ColBeginmidd, hv_RowEndmidd,
                                    hv_ColEndmidd, hv_RowBeginrigt, hv_ColBeginrigt, hv_RowEndrigt,
                                    hv_ColEndrigt, out hv_Angleraddtr);
                                //angle_lx (RowBeginrigt, ColBeginrigt, RowEndrigt, ColEndrigt, Angleraddtr)
                            }
                            else
                            {
                                hv_Angleraddtr.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidd, hv_ColBeginmidd, hv_RowEndmidd,
                                    hv_ColEndmidd, hv_RowEndrigt, hv_ColEndrigt, hv_RowBeginrigt, hv_ColBeginrigt,
                                    out hv_Angleraddtr);
                                //angle_lx (RowEndrigt, ColEndrigt, RowBeginrigt, ColBeginrigt, Angleraddtr)
                            }
                            hv_Degtr.Dispose();
                            HOperatorSet.TupleDeg(hv_Angleraddtr, out hv_Degtr);
                            hv_DegdrAbs.Dispose();
                            HOperatorSet.TupleAbs(hv_Degtr, out hv_DegdrAbs);


                            if ((int)(new HTuple(hv_ColBeginleft.TupleLess(hv_ColBeginrigt))) != 0)
                            {
                                hv_Angleradd.Dispose();
                                HOperatorSet.AngleLx(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                    hv_ColEndmidt, out hv_Angleradd);
                            }
                            else
                            {
                                hv_Angleradd.Dispose();
                                HOperatorSet.AngleLx(hv_RowEndmidt, hv_ColEndmidt, hv_RowBeginmidt,
                                    hv_ColBeginmidt, out hv_Angleradd);
                            }
                            hv_Degtd.Dispose();
                            HOperatorSet.TupleDeg(hv_Angleradd, out hv_Degtd);
                            hv_DegtdAbs.Dispose();
                            HOperatorSet.TupleAbs(hv_Degtd, out hv_DegtdAbs);



                            if ((int)(new HTuple(hv_RowBeginlefl.TupleGreater(hv_RowEndlefl))) != 0)
                            {
                                hv_Angleraddltl.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidl, hv_ColBeginmidl, hv_RowEndmidl,
                                    hv_ColEndmidl, hv_RowBeginlefl, hv_ColBeginlefl, hv_RowEndlefl,
                                    hv_ColEndlefl, out hv_Angleraddltl);
                                //angle_lx (RowBeginlefd, ColBeginlefd, RowEndlefd, ColEndlefd, Angleraddtl)
                            }
                            else
                            {
                                hv_Angleraddltl.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidl, hv_ColBeginmidl, hv_RowEndmidl,
                                    hv_ColEndmidl, hv_RowEndlefl, hv_ColEndlefl, hv_RowBeginlefl, hv_ColBeginlefd,
                                    out hv_Angleraddltl);
                                //angle_lx (RowEndlefd, ColEndlefd, RowBeginlefd, ColBeginlefd, Angleraddtl)

                            }
                            hv_Degldl.Dispose();
                            HOperatorSet.TupleDeg(hv_Angleraddltl, out hv_Degldl);
                            hv_DegldlAbs.Dispose();
                            HOperatorSet.TupleAbs(hv_Degldl, out hv_DegldlAbs);
                            hv_newltl.Dispose();
                            HOperatorSet.TupleAbs(hv_Angleraddltl, out hv_newltl);
                            hv_verlllength.Dispose();
                            HOperatorSet.TupleSin(hv_newltl, out hv_verlllength);
                            hv_disleft_verlength.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_disleft_verlength = hv_dis_leftLeftDiag * hv_verlllength;
                            }

                            if ((int)(new HTuple(hv_RowBeginlefr.TupleGreater(hv_RowEndlefr))) != 0)
                            {
                                hv_Angleraddrtl.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidr, hv_ColBeginmidr, hv_RowEndmidr,
                                    hv_ColEndmidr, hv_RowBeginlefr, hv_ColBeginlefr, hv_RowEndlefr,
                                    hv_ColEndlefr, out hv_Angleraddrtl);
                                //angle_lx (RowBeginlefd, ColBeginlefd, RowEndlefd, ColEndlefd, Angleraddtl)
                            }
                            else
                            {
                                hv_Angleraddrtl.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidr, hv_ColBeginmidr, hv_RowEndmidr,
                                    hv_ColEndmidr, hv_RowEndlefr, hv_ColEndlefr, hv_RowBeginlefr, hv_ColBeginlefd,
                                    out hv_Angleraddrtl);
                                //angle_lx (RowEndlefd, ColEndlefd, RowBeginlefd, ColBeginlefd, Angleraddtl)

                            }
                            hv_Degrdl.Dispose();
                            HOperatorSet.TupleDeg(hv_Angleraddrtl, out hv_Degrdl);
                            hv_DegdrAbs.Dispose();
                            HOperatorSet.TupleAbs(hv_Degrdl, out hv_DegdrAbs);
                            hv_newrtr.Dispose();
                            HOperatorSet.TupleAbs(hv_Angleraddrtl, out hv_newrtr);
                            hv_verrdlength.Dispose();
                            HOperatorSet.TupleSin(hv_newrtr, out hv_verrdlength);
                            hv_disright_verlength.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_disright_verlength = hv_dis_rightLeftDiag * hv_verrdlength;
                            }




                            hv_RowsMidt.Dispose(); hv_ColsMidt.Dispose();
                            HOperatorSet.GetContourXld(ho_contourmidt, out hv_RowsMidt, out hv_ColsMidt);
                            hv_RowsMidd.Dispose(); hv_ColsMidd.Dispose();
                            HOperatorSet.GetContourXld(ho_contourmidd, out hv_RowsMidd, out hv_ColsMidd);
                            hv_RowsMidl.Dispose(); hv_ColsMidl.Dispose();
                            HOperatorSet.GetContourXld(ho_contourmidl, out hv_RowsMidl, out hv_ColsMidl);
                            hv_RowsMidr.Dispose(); hv_ColsMidr.Dispose();
                            HOperatorSet.GetContourXld(ho_contourmidr, out hv_RowsMidr, out hv_ColsMidr);

                            hv_meanRowst.Dispose();
                            HOperatorSet.TupleMean(hv_RowsMidt, out hv_meanRowst);
                            hv_meanColst.Dispose();
                            HOperatorSet.TupleMean(hv_ColsMidt, out hv_meanColst);
                            hv_meanRowsd.Dispose();
                            HOperatorSet.TupleMean(hv_RowsMidd, out hv_meanRowsd);
                            hv_meanColsd.Dispose();
                            HOperatorSet.TupleMean(hv_ColsMidd, out hv_meanColsd);
                            hv_meanRowsl.Dispose();
                            HOperatorSet.TupleMean(hv_RowsMidl, out hv_meanRowsl);
                            hv_meanColsl.Dispose();
                            HOperatorSet.TupleMean(hv_ColsMidl, out hv_meanColsl);
                            hv_meanRowsr.Dispose();
                            HOperatorSet.TupleMean(hv_RowsMidr, out hv_meanRowsr);
                            hv_meanColsr.Dispose();
                            HOperatorSet.TupleMean(hv_ColsMidr, out hv_meanColsr);

                            hv_minColsd.Dispose();
                            HOperatorSet.TupleMin(hv_ColsMidd, out hv_minColsd);
                            hv_maxColsd.Dispose();
                            HOperatorSet.TupleMax(hv_ColsMidd, out hv_maxColsd);
                            hv_minColst.Dispose();
                            HOperatorSet.TupleMin(hv_ColsMidt, out hv_minColst);
                            hv_maxColst.Dispose();
                            HOperatorSet.TupleMax(hv_ColsMidt, out hv_maxColst);
                            hv_minColsl.Dispose();
                            HOperatorSet.TupleMin(hv_ColsMidl, out hv_minColsl);
                            hv_maxColsl.Dispose();
                            HOperatorSet.TupleMax(hv_ColsMidl, out hv_maxColsl);
                            hv_minColsr.Dispose();
                            HOperatorSet.TupleMin(hv_ColsMidr, out hv_minColsr);
                            hv_maxColsr.Dispose();
                            HOperatorSet.TupleMax(hv_ColsMidr, out hv_maxColsr);




                            //meanColst := (minColst + maxColst) / 2

                            hv_HomMat2DIdentity.Dispose();
                            HOperatorSet.HomMat2dIdentity(out hv_HomMat2DIdentity);


                            //以上侧3D相机激光线侧的坐标为原点， 根据标定块，进行拟合。求出左侧3D相机的坐标位置
                            //Y轴， 以上侧3D相机激光口作为参考线，激光口为线，水平线。
                            //X轴， 以3D相机采集物体中点作为参考点
                            if ((int)(new HTuple(hv_LeftTDistance.TupleNotEqual(0))) != 0)
                            {
                                //与上3D相机，左侧对齐
                                hv_Areal.Dispose(); hv_Rowl.Dispose(); hv_Columnl.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursl, out hv_Areal, out hv_Rowl,
                                    out hv_Columnl, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowst - hv_meanRowsl,
                                        hv_minColst - hv_minColsl, out hv_HomMatTransPreL);
                                }
                                ho_SelectedContourslPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursl, out ho_SelectedContourslPre,
                                    hv_HomMatTransPreL);

                                //逆向旋转90度， 中间的线的中点为旋转中心
                                hv_Areal.Dispose(); hv_RowPrel.Dispose(); hv_ColumnPrel.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContourslPre, out hv_Areal, out hv_RowPrel,
                                    out hv_ColumnPrel, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DLRotate.Dispose();
                                    HOperatorSet.HomMat2dRotate(hv_HomMat2DIdentity, (new HTuple(-90)).TupleRad()
                                        , hv_RowPrel.TupleSelect(1), hv_ColumnPrel.TupleSelect(1), out hv_HomMat2DLRotate);
                                }
                                ho_SelectedContourslfinal.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslPre, out ho_SelectedContourslfinal,
                                    hv_HomMat2DLRotate);

                                hv_Areal.Dispose(); hv_RowPrel.Dispose(); hv_ColumnPrel.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContourslfinal, out hv_Areal,
                                    out hv_RowPrel, out hv_ColumnPrel, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DScale.Dispose();
                                    HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_RowPrel.TupleSelect(
                                        1), hv_ColumnPrel.TupleSelect(1), out hv_HomMat2DScale);
                                }
                                ho_SelectedContourslnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslfinal, out ho_SelectedContourslnewpre,
                                    hv_HomMat2DScale);

                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourdowlnew,
                                    3);


                                hv_RowsMidl.Dispose(); hv_ColsMidl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidlnew, out hv_RowsMidl, out hv_ColsMidl);
                                hv_meanMidColnew.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidl, out hv_meanMidColnew);

                                hv_RowBeginmidtnew.Dispose(); hv_ColBeginmidtnew.Dispose(); hv_RowEndmidtnew.Dispose(); hv_ColEndmidtnew.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmidlnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmidtnew, out hv_ColBeginmidtnew, out hv_RowEndmidtnew,
                                    out hv_ColEndmidtnew, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_Rowminmidt.Dispose();
                                HOperatorSet.TupleMin2(hv_RowBeginmidtnew, hv_RowEndmidtnew, out hv_Rowminmidt);
                                //meanmidCols := (ColBeginmidtnew + ColEndmidtnew) / 2
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowst - hv_Rowminmidt;
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_maxColst - hv_meanMidColnew,
                                        out hv_HomMatTransPreL);
                                }
                                ho_SelectedContourslprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslnewpre, out ho_SelectedContourslprenew,
                                    hv_HomMatTransPreL);

                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourdowlnew,
                                    3);
                                hv_RowsTopl.Dispose(); hv_ColsTopl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourtoplnew, out hv_RowsTopl, out hv_ColsTopl);
                                hv_RowsDowl.Dispose(); hv_ColsDowl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourdowlnew, out hv_RowsDowl, out hv_ColsDowl);

                                hv_meanTopl.Dispose();
                                HOperatorSet.TupleMean(hv_RowsTopl, out hv_meanTopl);
                                hv_meanDowl.Dispose();
                                HOperatorSet.TupleMean(hv_RowsDowl, out hv_meanDowl);

                                if ((int)(new HTuple(hv_meanTopl.TupleGreater(hv_meanDowl))) != 0)
                                {
                                    ho_contourtmp.Dispose();
                                    ho_contourtmp = new HObject(ho_contourtoplnew);
                                    ho_contourtoplnew.Dispose();
                                    ho_contourtoplnew = new HObject(ho_contourdowlnew);
                                    ho_contourdowlnew.Dispose();
                                    ho_contourdowlnew = new HObject(ho_contourtmp);
                                }
                                hv_RowBegintopl.Dispose(); hv_ColBegintopl.Dispose(); hv_RowEndtopl.Dispose(); hv_ColEndtopl.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoplnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopl, out hv_ColBegintopl, out hv_RowEndtopl,
                                    out hv_ColEndtopl, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);

                                //新的图形SelectedContourslfinal，上侧边的角度要与 上3D相机采集图像右侧边角度对齐
                                if ((int)(new HTuple(hv_RowBegintopl.TupleGreater(hv_RowEndtopl))) != 0)
                                {
                                    hv_angleltx.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowBegintopl, hv_ColBegintopl, hv_RowEndtopl,
                                        hv_ColEndtopl, out hv_angleltx);
                                }
                                else
                                {
                                    hv_angleltx.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowEndtopl, hv_ColEndtopl, hv_RowBegintopl,
                                        hv_ColBegintopl, out hv_angleltx);
                                }

                                hv_Degltx.Dispose();
                                HOperatorSet.TupleDeg(hv_angleltx, out hv_Degltx);
                                hv_AbgDegltx.Dispose();
                                HOperatorSet.TupleAbs(hv_Degltx, out hv_AbgDegltx);
                                hv_Abgangleltx.Dispose();
                                HOperatorSet.TupleAbs(hv_Angleradtr, out hv_Abgangleltx);
                                hv_DegSubL.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubL = hv_DegtrAbs - (180 - hv_AbgDegltx);
                                }
                                //if (DegSubL > 0.1 or DegSubL < -0.1)
                                //area_center_xld (SelectedContourslprenew, Areal, Rowl, Columnl, PointOrderl)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(-DegSubL), Columnl[1], Rowl[1], HomMat2DLRotate)
                                //affine_trans_contour_xld (SelectedContourslprenew, SelectedContourslfinalnew, HomMat2DLRotate)
                                //select_obj (SelectedContourslfinalnew, contourtoplnew, 1)
                                //select_obj (SelectedContourslfinalnew, contourmidlnew, 2)
                                //select_obj (SelectedContourslfinalnew, contourdowlnew, 3)
                                //get_contour_xld (contourtoplnew, RowsTopl, ColsTopl)
                                //get_contour_xld (contourdowlnew, RowsDowl, ColsDowl)

                                //tuple_mean (RowsTopl, meanTopl)
                                //tuple_mean (RowsDowl, meanDowl)

                                //if (meanTopl > meanDowl)
                                //contourtmp := contourtoplnew
                                //contourtoplnew := contourdowlnew
                                //contourdowlnew := contourtmp
                                //endif
                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBegintopl, ColBegintopl, RowEndtopl, ColEndtopl, Nrleft, Ncleft, Distleft)
                                //if (RowBegintopl > RowEndtopl)
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowBegintopl, ColBegintopl, RowEndtopl, ColEndtopl, angleltx)
                                //else
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowEndtopl, ColEndtopl, RowBegintopl, ColBegintopl, angleltx)
                                //endif
                                //tuple_deg (angleltx, Degltx)

                                //DegSubL := DegtrAbs - (180 - Degltx)
                                //SelectedContourslprenew := SelectedContourslfinalnew
                                //endif



                                //Y轴 新的图像，要沿着中间线 contourmidlnew，向下移动到与 上侧相机采集图形 SelectedContourst，中间线contourmidt的高度一致。
                                //X轴 新的图像，要沿着 SelectedContourst 的中间线 contourmidt 向左侧偏移到起点，进行滑行


                                //标定块拟合，需要根据标定块边长计算左移距离，  ( 182 - 12 ) * sin45 = 111.72, 移动后就是左边3D相机的具体位置
                                //( 182 - 12 ) * sin45  + meanRowsl , 下次按照这个思路进行偏移，根据新的方棒左3D相机采集的高度，进行
                                //计算，例如标定快高度为 -13,  新的方棒标定块 -11， 那么左移动就是（-11 + 13） = 2， 左移动 111.72 + 2
                                //则与3D位置相同。要沿着上面3D，左边的角度进行移动。 右边3D处理与此类似。

                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourmidlnew,
                                    2);
                                hv_RowsMidl.Dispose(); hv_ColsMidl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidlnew, out hv_RowsMidl, out hv_ColsMidl);
                                hv_MeanMidlx.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidl, out hv_MeanMidlx);

                                hv_Sintl.Dispose();
                                HOperatorSet.TupleSin(hv_Abgangleltx, out hv_Sintl);
                                hv_Costl.Dispose();
                                HOperatorSet.TupleCos(hv_Abgangleltx, out hv_Costl);
                                hv_AbsSintl.Dispose();
                                HOperatorSet.TupleAbs(hv_Sintl, out hv_AbsSintl);
                                hv_AbsCostl.Dispose();
                                HOperatorSet.TupleAbs(hv_Costl, out hv_AbsCostl);

                                //Left3DX + AbsmeanRowsl - meanColst 为 contourmidrnew 需要移动到的位置 X坐标
                                //Left3DX X轴相对上侧相机采集图像中点的距离
                                //Left3DY Y轴相对上侧相机激光口所在水平线的Y轴相对距离
                                //新的方棒 AbsmeanRowsl 采集图像，到激光口的Y 距离
                                hv_AbsmeanRowsl.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsl, out hv_AbsmeanRowsl);
                                hv_DistanceXPre.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DistanceXPre = (hv_LeftTDistance - hv_AbsmeanRowsl) - ((hv_maxColst - hv_minColst) / 2);
                                }
                                hv_DistanceLPre.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DistanceLPre = hv_DistanceXPre / hv_AbsCostl;
                                }

                                //DistanceLPre := 157.98
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_DistanceLPre * hv_AbsSintl,
                                        hv_DistanceXPre, out hv_HomMatTransPreL);
                                }
                                //hom_mat2d_translate (HomMat2DIdentity, 111.72, -111.72, HomMatTransPreL)
                                ho_SelectedContourslnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslprenew, out ho_SelectedContourslnew,
                                    hv_HomMatTransPreL);
                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourdowlnew,
                                    3);
                                //DistanceLPre 为边长
                                //X轴  先向左 移动  meanColst - meanColsl，
                                //Y轴  先移动 meanRowst - RowBeginmidt ，
                                //逆时针旋转90度， 暂时水平翻转180度。  左侧相机右侧跟上侧相机右侧相对。上侧相机采集图像是否需要以中间线中点纵向翻转？
                                //根据新的图形SelectedContourslnewpre，中间线X值，向 上侧相机中间线左边的点对齐，左移 minColst - meanmidColsnew
                                //然后再向左移动 DistanceLPre * Costl  再向下 移动 DistanceLPre * Sintl
                                //Left3DLineX 为旋转后，左3D相机拍出来的方棒上边缘的X位置
                                //Left3DX 为3D相机线所在位置，这个位置为新的方棒，左侧3D相机移动拟合的位置。

                                //get_contour_xld (contourmidlnew, RowsnewMidl, ColsnewMidl)
                                //tuple_mean (ColsnewMidl, Left3DLineX)
                                //tuple_abs (meanRowsl, AbsLeft3Ddistance)
                                //Left3DX := Left3DLineX - AbsLeft3Ddistance
                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, Nrleft, Ncleft, Distleft)
                                hv_RowBeginleft.Dispose(); hv_ColBeginleft.Dispose(); hv_RowEndleft.Dispose(); hv_ColEndleft.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourleft, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginleft, out hv_ColBeginleft, out hv_RowEndleft,
                                    out hv_ColEndleft, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_RowBeginrigt.Dispose(); hv_ColBeginrigt.Dispose(); hv_RowEndrigt.Dispose(); hv_ColEndrigt.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigt.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourrigt, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginrigt, out hv_ColBeginrigt, out hv_RowEndrigt,
                                    out hv_ColEndrigt, out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigt);
                                hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoplnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl,
                                    out hv_ColEndlefl, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourdowlnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr,
                                    out hv_ColEndlefr, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);

                                hv_RowIntersectiontl.Dispose(); hv_ColIntersectiontl.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefl, hv_ColBeginlefl, hv_RowEndlefl,
                                    hv_ColEndlefl, hv_RowBeginlefr, hv_ColBeginlefr, hv_RowEndlefr,
                                    hv_ColEndlefr, out hv_RowIntersectiontl, out hv_ColIntersectiontl,
                                    out hv_IsOverlapping);
                                hv_RowIntersectiont.Dispose(); hv_ColIntersectiont.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginleft, hv_ColBeginleft, hv_RowEndleft,
                                    hv_ColEndleft, hv_RowBeginrigt, hv_ColBeginrigt, hv_RowEndrigt,
                                    hv_ColEndrigt, out hv_RowIntersectiont, out hv_ColIntersectiont,
                                    out hv_IsOverlapping);
                                hv_DistanceLT.Dispose();
                                HOperatorSet.DistancePp(hv_RowIntersectiontl, hv_ColIntersectiontl,
                                    hv_RowIntersectiont, hv_ColIntersectiont, out hv_DistanceLT);
                                {
                                    HTuple ExpTmpOutVar_0;
                                    HOperatorSet.TupleConcat(hv_DistanceLT, hv_DistancesLT, out ExpTmpOutVar_0);
                                    hv_DistancesLT.Dispose();
                                    hv_DistancesLT = ExpTmpOutVar_0;
                                }
                            }



                            //以上侧3D相机激光线侧的坐标为原点， 根据标定块，进行拟合。求出左侧3D相机的坐标位置
                            //Y轴， 以上侧3D相机激光口作为参考线，激光口为线，水平线。
                            //X轴， 以3D相机采集物体中点作为参考点
                            if ((int)(new HTuple(hv_LeftDDistance.TupleNotEqual(0))) != 0)
                            {
                                //与上3D相机，左侧对齐
                                hv_Areal.Dispose(); hv_Rowl.Dispose(); hv_Columnl.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursl, out hv_Areal, out hv_Rowl,
                                    out hv_Columnl, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowsd - hv_meanRowsl,
                                        hv_minColsd - hv_minColsl, out hv_HomMatTransPreL);
                                }
                                ho_SelectedContourslPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursl, out ho_SelectedContourslPre,
                                    hv_HomMatTransPreL);

                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslPre, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslPre, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslPre, out ho_contourdowlnew,
                                    3);

                                hv_RowsMidnewt.Dispose(); hv_ColsMidnewt.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidlnew, out hv_RowsMidnewt, out hv_ColsMidnewt);

                                hv_MeanMidnewColt.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidnewt, out hv_MeanMidnewColt);
                                hv_MeanMidnewRowy.Dispose();
                                HOperatorSet.TupleMean(hv_RowsMidnewt, out hv_MeanMidnewRowy);
                                //逆向旋转90度， 中间的线的中点为旋转中心
                                hv_Areal.Dispose(); hv_RowPrel.Dispose(); hv_ColumnPrel.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContourslPre, out hv_Areal, out hv_RowPrel,
                                    out hv_ColumnPrel, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DLRotate.Dispose();
                                    HOperatorSet.HomMat2dRotate(hv_HomMat2DIdentity, (new HTuple(90)).TupleRad()
                                        , hv_MeanMidnewRowy, hv_MeanMidnewColt, out hv_HomMat2DLRotate);
                                }
                                ho_SelectedContourslfinal.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslPre, out ho_SelectedContourslfinal,
                                    hv_HomMat2DLRotate);


                                hv_Areal.Dispose(); hv_RowPrel.Dispose(); hv_ColumnPrel.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContourslfinal, out hv_Areal,
                                    out hv_RowPrel, out hv_ColumnPrel, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DScale.Dispose();
                                    HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_RowPrel.TupleSelect(
                                        1), hv_ColumnPrel.TupleSelect(1), out hv_HomMat2DScale);
                                }
                                ho_SelectedContourslnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslfinal, out ho_SelectedContourslnewpre,
                                    hv_HomMat2DScale);

                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourdowlnew,
                                    3);


                                hv_RowsMidl.Dispose(); hv_ColsMidl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidlnew, out hv_RowsMidl, out hv_ColsMidl);
                                hv_meanMidColnew.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidl, out hv_meanMidColnew);

                                hv_RowBeginmidtnew.Dispose(); hv_ColBeginmidtnew.Dispose(); hv_RowEndmidtnew.Dispose(); hv_ColEndmidtnew.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmidlnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmidtnew, out hv_ColBeginmidtnew, out hv_RowEndmidtnew,
                                    out hv_ColEndmidtnew, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_Rowminmidt.Dispose();
                                HOperatorSet.TupleMin2(hv_RowBeginmidtnew, hv_RowEndmidtnew, out hv_Rowminmidt);
                                //meanmidCols := (ColBeginmidtnew + ColEndmidtnew) / 2
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowsd - hv_Rowminmidt;
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_minColsd - hv_meanMidColnew,
                                        out hv_HomMatTransPreL);
                                }
                                ho_SelectedContourslprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslnewpre, out ho_SelectedContourslprenew,
                                    hv_HomMatTransPreL);

                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourdowlnew,
                                    3);
                                hv_RowsTopl.Dispose(); hv_ColsTopl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourtoplnew, out hv_RowsTopl, out hv_ColsTopl);
                                hv_RowsDowl.Dispose(); hv_ColsDowl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourdowlnew, out hv_RowsDowl, out hv_ColsDowl);

                                hv_meanTopl.Dispose();
                                HOperatorSet.TupleMean(hv_RowsTopl, out hv_meanTopl);
                                hv_meanDowl.Dispose();
                                HOperatorSet.TupleMean(hv_RowsDowl, out hv_meanDowl);

                                if ((int)(new HTuple(hv_meanTopl.TupleGreater(hv_meanDowl))) != 0)
                                {
                                    ho_contourtmp.Dispose();
                                    ho_contourtmp = new HObject(ho_contourtoplnew);
                                    ho_contourtoplnew.Dispose();
                                    ho_contourtoplnew = new HObject(ho_contourdowlnew);
                                    ho_contourdowlnew.Dispose();
                                    ho_contourdowlnew = new HObject(ho_contourtmp);
                                }
                                hv_RowBegintopl.Dispose(); hv_ColBegintopl.Dispose(); hv_RowEndtopl.Dispose(); hv_ColEndtopl.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoplnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopl, out hv_ColBegintopl, out hv_RowEndtopl,
                                    out hv_ColEndtopl, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);

                                //新的图形SelectedContourslfinal，上侧边的角度要与 上3D相机采集图像右侧边角度对齐
                                if ((int)(new HTuple(hv_RowBegintopl.TupleGreater(hv_RowEndtopl))) != 0)
                                {
                                    hv_angleltx.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowBegintopl, hv_ColBegintopl, hv_RowEndtopl,
                                        hv_ColEndtopl, out hv_angleltx);
                                }
                                else
                                {
                                    hv_angleltx.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowEndtopl, hv_ColEndtopl, hv_RowBegintopl,
                                        hv_ColBegintopl, out hv_angleltx);
                                }

                                hv_Degltx.Dispose();
                                HOperatorSet.TupleDeg(hv_angleltx, out hv_Degltx);
                                hv_AbgDegltx.Dispose();
                                HOperatorSet.TupleAbs(hv_Degltx, out hv_AbgDegltx);
                                hv_Abgangleltx.Dispose();
                                HOperatorSet.TupleAbs(hv_Angleraddtl, out hv_Abgangleltx);
                                hv_DegSubL.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubL = hv_DegtrAbs - (180 - hv_AbgDegltx);
                                }

                                //if (DegSubL > 0.1 or DegSubL < -0.1)
                                //area_center_xld (SelectedContourslprenew, Areal, Rowl, Columnl, PointOrderl)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(-DegSubL), Columnl[1], Rowl[1], HomMat2DLRotate)
                                //affine_trans_contour_xld (SelectedContourslprenew, SelectedContourslfinalnew, HomMat2DLRotate)
                                //select_obj (SelectedContourslfinalnew, contourtoplnew, 1)
                                //select_obj (SelectedContourslfinalnew, contourmidlnew, 2)
                                //select_obj (SelectedContourslfinalnew, contourdowlnew, 3)
                                //get_contour_xld (contourtoplnew, RowsTopl, ColsTopl)
                                //get_contour_xld (contourdowlnew, RowsDowl, ColsDowl)

                                //tuple_mean (RowsTopl, meanTopl)
                                //tuple_mean (RowsDowl, meanDowl)

                                //if (meanTopl > meanDowl)
                                //contourtmp := contourtoplnew
                                //contourtoplnew := contourdowlnew
                                //contourdowlnew := contourtmp
                                //endif
                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBegintopl, ColBegintopl, RowEndtopl, ColEndtopl, Nrleft, Ncleft, Distleft)
                                //if (RowBegintopl > RowEndtopl)
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowBegintopl, ColBegintopl, RowEndtopl, ColEndtopl, angleltx)
                                //else
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowEndtopl, ColEndtopl, RowBegintopl, ColBegintopl, angleltx)
                                //endif
                                //tuple_deg (angleltx, Degltx)

                                //DegSubL := DegtrAbs - (180 - Degltx)
                                //SelectedContourslprenew := SelectedContourslfinalnew
                                //endif



                                //Y轴 新的图像，要沿着中间线 contourmidlnew，向下移动到与 上侧相机采集图形 SelectedContourst，中间线contourmidt的高度一致。
                                //X轴 新的图像，要沿着 SelectedContourst 的中间线 contourmidt 向左侧偏移到起点，进行滑行


                                //标定块拟合，需要根据标定块边长计算左移距离，  ( 182 - 12 ) * sin45 = 111.72, 移动后就是左边3D相机的具体位置
                                //( 182 - 12 ) * sin45  + meanRowsl , 下次按照这个思路进行偏移，根据新的方棒左3D相机采集的高度，进行
                                //计算，例如标定快高度为 -13,  新的方棒标定块 -11， 那么左移动就是（-11 + 13） = 2， 左移动 111.72 + 2
                                //则与3D位置相同。要沿着上面3D，左边的角度进行移动。 右边3D处理与此类似。

                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourmidlnew,
                                    2);
                                hv_RowsMidl.Dispose(); hv_ColsMidl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidlnew, out hv_RowsMidl, out hv_ColsMidl);
                                hv_MeanMidlx.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidl, out hv_MeanMidlx);

                                //tuple_abs (angleltx, Absangleltx)
                                hv_Sintl.Dispose();
                                HOperatorSet.TupleSin(hv_Abgangleltx, out hv_Sintl);
                                hv_Costl.Dispose();
                                HOperatorSet.TupleCos(hv_Abgangleltx, out hv_Costl);
                                hv_AbsSintl.Dispose();
                                HOperatorSet.TupleAbs(hv_Sintl, out hv_AbsSintl);
                                hv_AbsCostl.Dispose();
                                HOperatorSet.TupleAbs(hv_Costl, out hv_AbsCostl);

                                //Left3DX + AbsmeanRowsl - meanColst 为 contourmidrnew 需要移动到的位置 X坐标
                                //Left3DX X轴相对上侧相机采集图像中点的距离
                                //Left3DY Y轴相对上侧相机激光口所在水平线的Y轴相对距离
                                //新的方棒 AbsmeanRowsl 采集图像，到激光口的Y 距离
                                hv_AbsmeanRowsl.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsl, out hv_AbsmeanRowsl);
                                hv_DistanceXPre.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DistanceXPre = (hv_LeftDDistance - hv_AbsmeanRowsl) - ((hv_maxColst - hv_minColst) / 2);
                                }
                                hv_DistanceLPre.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DistanceLPre = hv_DistanceXPre / hv_AbsCostl;
                                }

                                //DistanceLPre := 157.98
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_DistanceLPre * hv_AbsSintl,
                                        -(hv_DistanceLPre * hv_AbsCostl), out hv_HomMatTransPreL);
                                }
                                //hom_mat2d_translate (HomMat2DIdentity, 111.72, -111.72, HomMatTransPreL)
                                ho_SelectedContourslnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslprenew, out ho_SelectedContourslnew,
                                    hv_HomMatTransPreL);
                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourdowlnew,
                                    3);
                                //DistanceLPre 为边长
                                //X轴  先向左 移动  meanColst - meanColsl，
                                //Y轴  先移动 meanRowst - RowBeginmidt ，
                                //逆时针旋转90度， 暂时水平翻转180度。  左侧相机右侧跟上侧相机右侧相对。上侧相机采集图像是否需要以中间线中点纵向翻转？
                                //根据新的图形SelectedContourslnewpre，中间线X值，向 上侧相机中间线左边的点对齐，左移 minColst - meanmidColsnew
                                //然后再向左移动 DistanceLPre * Costl  再向下 移动 DistanceLPre * Sintl
                                //Left3DLineX 为旋转后，左3D相机拍出来的方棒上边缘的X位置
                                //Left3DX 为3D相机线所在位置，这个位置为新的方棒，左侧3D相机移动拟合的位置。

                                //get_contour_xld (contourmidlnew, RowsnewMidl, ColsnewMidl)
                                //tuple_mean (ColsnewMidl, Left3DLineX)
                                //tuple_abs (meanRowsl, AbsLeft3Ddistance)
                                //Left3DX := Left3DLineX - AbsLeft3Ddistance
                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, Nrleft, Ncleft, Distleft)
                                hv_RowBeginlefd.Dispose(); hv_ColBeginlefd.Dispose(); hv_RowEndlefd.Dispose(); hv_ColEndlefd.Dispose(); hv_Nrlefd.Dispose(); hv_Nclefd.Dispose(); hv_Distlefd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourlefd, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginlefd, out hv_ColBeginlefd, out hv_RowEndlefd,
                                    out hv_ColEndlefd, out hv_Nrlefd, out hv_Nclefd, out hv_Distlefd);
                                hv_RowBeginrigd.Dispose(); hv_ColBeginrigd.Dispose(); hv_RowEndrigd.Dispose(); hv_ColEndrigd.Dispose(); hv_Nrrigd.Dispose(); hv_Ncrigd.Dispose(); hv_Distrigd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourrigd, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginrigd, out hv_ColBeginrigd, out hv_RowEndrigd,
                                    out hv_ColEndrigd, out hv_Nrrigd, out hv_Ncrigd, out hv_Distrigd);
                                hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoplnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl,
                                    out hv_ColEndlefl, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourdowlnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr,
                                    out hv_ColEndlefr, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);

                                hv_RowIntersectiontd.Dispose(); hv_ColIntersectiontd.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefl, hv_ColBeginlefl, hv_RowEndlefl,
                                    hv_ColEndlefl, hv_RowBeginlefr, hv_ColBeginlefr, hv_RowEndlefr,
                                    hv_ColEndlefr, out hv_RowIntersectiontd, out hv_ColIntersectiontd,
                                    out hv_IsOverlapping);
                                hv_RowIntersectiont.Dispose(); hv_ColIntersectiont.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefd, hv_ColBeginlefd, hv_RowEndlefd,
                                    hv_ColEndlefd, hv_RowBeginrigd, hv_ColBeginrigd, hv_RowEndrigd,
                                    hv_ColEndrigd, out hv_RowIntersectiont, out hv_ColIntersectiont,
                                    out hv_IsOverlapping);
                                hv_DistanceDL.Dispose();
                                HOperatorSet.DistancePp(hv_RowIntersectiontd, hv_ColIntersectiontd,
                                    hv_RowIntersectiont, hv_ColIntersectiont, out hv_DistanceDL);
                                {
                                    HTuple ExpTmpOutVar_0;
                                    HOperatorSet.TupleConcat(hv_DistanceDL, hv_DistancesLD, out ExpTmpOutVar_0);
                                    hv_DistancesLD.Dispose();
                                    hv_DistancesLD = ExpTmpOutVar_0;
                                }
                            }



                            //以上侧3D相机激光线侧的坐标为原点， 根据标定块，进行拟合。求出右侧3D相机的坐标位置
                            if ((int)(new HTuple(hv_RightTDistance.TupleNotEqual(0))) != 0)
                            {
                                //与上3D相机，左侧对齐
                                hv_Arear.Dispose(); hv_Rowr.Dispose(); hv_Columnr.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursr, out hv_Arear, out hv_Rowr,
                                    out hv_Columnr, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowst - hv_meanRowsr,
                                        hv_minColst - hv_minColsr, out hv_HomMatTransPreR);
                                }
                                ho_SelectedContoursrPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursr, out ho_SelectedContoursrPre,
                                    hv_HomMatTransPreR);

                                //顺时针旋转90度， 中间的线的中点为旋转中心
                                hv_Arear.Dispose(); hv_RowPrer.Dispose(); hv_ColumnPrer.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursrPre, out hv_Arear, out hv_RowPrer,
                                    out hv_ColumnPrer, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DRRotate.Dispose();
                                    HOperatorSet.HomMat2dRotate(hv_HomMat2DIdentity, (new HTuple(90)).TupleRad()
                                        , hv_RowPrer.TupleSelect(1), hv_ColumnPrer.TupleSelect(1), out hv_HomMat2DRRotate);
                                }
                                ho_SelectedContoursrfinal.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrPre, out ho_SelectedContoursrfinal,
                                    hv_HomMat2DRRotate);

                                //沿着SelectedContourslfinal 中间线左侧点，以X轴为中心，进行水平反转
                                //上相机右侧与左侧相机左侧对应，所以需要反转
                                hv_Arear.Dispose(); hv_RowPrer.Dispose(); hv_ColumnPrer.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursrfinal, out hv_Arear,
                                    out hv_RowPrer, out hv_ColumnPrer, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DScale.Dispose();
                                    HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_RowPrer.TupleSelect(
                                        1), hv_ColumnPrer.TupleSelect(1), out hv_HomMat2DScale);
                                }
                                ho_SelectedContoursrnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrfinal, out ho_SelectedContoursrnewpre,
                                    hv_HomMat2DScale);


                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourdowrnew,
                                    3);



                                hv_RowBeginmidnewr.Dispose(); hv_ColBeginmidnewr.Dispose(); hv_RowEndmidnewr.Dispose(); hv_ColEndmidnewr.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmidrnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmidnewr, out hv_ColBeginmidnewr, out hv_RowEndmidnewr,
                                    out hv_ColEndmidnewr, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_Rowmidrmin.Dispose();
                                HOperatorSet.TupleMin2(hv_RowBeginmidnewr, hv_RowEndmidnewr, out hv_Rowmidrmin);
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowst - hv_Rowmidrmin;
                                }
                                hv_Rowsrmidnew.Dispose(); hv_Colsrmidnew.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidrnew, out hv_Rowsrmidnew, out hv_Colsrmidnew);
                                hv_meanmidColsnew.Dispose();
                                HOperatorSet.TupleMean(hv_Colsrmidnew, out hv_meanmidColsnew);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_minColst - hv_meanmidColsnew,
                                        out hv_HomMatTransPreR);
                                }
                                ho_SelectedContoursrprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrnewpre, out ho_SelectedContoursrprenew,
                                    hv_HomMatTransPreR);

                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourdowrnew,
                                    3);

                                hv_RowsTopr.Dispose(); hv_ColsTopr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourtoprnew, out hv_RowsTopr, out hv_ColsTopr);
                                hv_RowsDowr.Dispose(); hv_ColsDowr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourdowrnew, out hv_RowsDowr, out hv_ColsDowr);

                                hv_meanTopr.Dispose();
                                HOperatorSet.TupleMean(hv_RowsTopr, out hv_meanTopr);
                                hv_meanDowr.Dispose();
                                HOperatorSet.TupleMean(hv_RowsDowr, out hv_meanDowr);

                                if ((int)(new HTuple(hv_meanTopr.TupleGreater(hv_meanDowr))) != 0)
                                {
                                    ho_contourtmp.Dispose();
                                    ho_contourtmp = new HObject(ho_contourtoprnew);
                                    ho_contourtoprnew.Dispose();
                                    ho_contourtoprnew = new HObject(ho_contourdowrnew);
                                    ho_contourdowrnew.Dispose();
                                    ho_contourdowrnew = new HObject(ho_contourtmp);
                                }

                                //新的图形SelectedContoursrfinal，上侧边的角度要与 上3D相机采集图像左侧边角度对齐
                                hv_RowBegintopr.Dispose(); hv_ColBegintopr.Dispose(); hv_RowEndtopr.Dispose(); hv_ColEndtopr.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoprnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopr, out hv_ColBegintopr, out hv_RowEndtopr,
                                    out hv_ColEndtopr, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                if ((int)(new HTuple(hv_RowBegintopr.TupleGreater(hv_RowEndtopr))) != 0)
                                {
                                    hv_Angleradtr.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowBegintopr, hv_ColBegintopr, hv_RowEndtopr,
                                        hv_ColEndtopr, out hv_Angleradtr);
                                }
                                else
                                {
                                    hv_Angleradtr.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowEndtopr, hv_ColEndtopr, hv_RowBegintopr,
                                        hv_ColBegintopr, out hv_Angleradtr);
                                }
                                hv_Degrtr.Dispose();
                                HOperatorSet.TupleDeg(hv_Angleradtr, out hv_Degrtr);
                                hv_DegrtrAbs.Dispose();
                                HOperatorSet.TupleAbs(hv_Degrtr, out hv_DegrtrAbs);
                                hv_DegSubR.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubR = hv_DegtlAbs - hv_DegrtrAbs;
                                }

                                //if (DegSubR > 0.1 or DegSubR < -0.1)
                                //area_center_xld (SelectedContoursrprenew, Arear, Rowr, Columnr, PointOrderr)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(DegSubR), Columnr[1], Rowr[1], HomMat2DRRotate)
                                //affine_trans_contour_xld (SelectedContoursrprenew, SelectedContoursrfinalnew, HomMat2DRRotate)
                                //select_obj (SelectedContoursrfinalnew, contourtoprnew, 1)
                                //select_obj (SelectedContoursrfinalnew, contourmidrnew, 2)
                                //select_obj (SelectedContoursrfinalnew, contourdowrnew, 3)
                                //get_contour_xld (contourtoprnew, RowsTopr, ColsTopr)
                                //get_contour_xld (contourdowrnew, RowsDowr, ColsDowr)
                                //tuple_mean (RowsTopr, meanTopr)
                                //tuple_mean (RowsDowr, meanDowr)
                                //if (meanTopr > meanDowr)
                                //contourtmp := contourtoprnew
                                //contourtoprnew := contourdowrnew
                                //contourdowrnew := contourtmp
                                //endif
                                //fit_line_contour_xld (contourtoprnew, 'tukey', -1, 10, 5, 2, RowBegintopr, ColBegintopr, RowEndtopr, ColEndtopr, Nrleft, Ncleft, Distleft)
                                //if (RowBegintopr > RowEndtopr)
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowBegintopr, ColBegintopr, RowEndtopr, ColEndtopr, Angleradtr)
                                //else
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowEndtopr, ColEndtopr, RowBegintopr, ColBegintopr, Angleradtr)
                                //endif
                                //tuple_deg (Angleradtr, Degrtr)
                                //tuple_abs (Degrtr, DegrtrAbs)
                                //DegSubR := DegtlAbs - DegrtrAbs
                                //SelectedContoursrprenew := SelectedContoursrfinalnew
                                //endif


                                //标定块拟合，需要根据标定块边长计算左移距离，  ( 182 - 12 ) * sin45 = 111.72, 移动后就是左边3D相机的具体位置
                                //( 182 - 12 ) * sin45  + meanRowsl , 下次按照这个思路进行偏移，根据新的方棒左3D相机采集的高度，进行
                                //计算，例如标定快高度为 -13,  新的方棒标定块 -11， 那么左移动就是（-11 + 13） = 2， 左移动 111.72 + 2
                                //则与3D位置相同。要沿着上面3D，左边的角度进行移动。 右边3D处理与此类似。

                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourmidrnew,
                                    2);
                                hv_RowsMidr.Dispose(); hv_ColsMidr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidrnew, out hv_RowsMidr, out hv_ColsMidr);
                                hv_MeanMidrx.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidr, out hv_MeanMidrx);


                                hv_AngleradtrAbs.Dispose();
                                HOperatorSet.TupleAbs(hv_Angleradttl, out hv_AngleradtrAbs);
                                hv_Sintr.Dispose();
                                HOperatorSet.TupleSin(hv_AngleradtrAbs, out hv_Sintr);
                                hv_Costr.Dispose();
                                HOperatorSet.TupleCos(hv_AngleradtrAbs, out hv_Costr);
                                hv_AbsSintr.Dispose();
                                HOperatorSet.TupleAbs(hv_Sintr, out hv_AbsSintr);
                                hv_AbsCostr.Dispose();
                                HOperatorSet.TupleAbs(hv_Costr, out hv_AbsCostr);

                                //Right3DX + meanRowsr 为 contourmidrnew 需要移动到的位置 X坐标
                                //tuple_abs (meanRowsr, AbsRight3DDistance)
                                hv_AbsmeanRowsr.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsr, out hv_AbsmeanRowsr);
                                hv_DistanceXPre.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DistanceXPre = (hv_RightTDistance - hv_AbsmeanRowsr) - ((hv_maxColst - hv_minColst) / 2);
                                }
                                hv_DistanceRPre.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DistanceRPre = hv_DistanceXPre / hv_AbsCostr;
                                }


                                //DistanceRPre := 158.75
                                //hom_mat2d_translate (HomMat2DIdentity, 111.72, 111.72, HomMatTransPreR)

                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_DistanceRPre * hv_AbsSintr,
                                        -(hv_DistanceRPre * hv_AbsCostr), out hv_HomMatTransPreR);
                                }


                                ho_SelectedContoursrnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrprenew, out ho_SelectedContoursrnew,
                                    hv_HomMatTransPreR);
                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourdowrnew,
                                    3);

                                //
                                //DistanceRPre 为边长
                                //X轴  先向左 移动  meanColst - meanColsr
                                //Y轴  先移动 meanRowst -  RowBeginmidr
                                //逆时针旋转90度，   右侧相机左侧跟上侧相机左侧相对。上侧相机采集图像是否需要以中间线中点纵向翻转？
                                //根据新的图形SelectedContoursrfinal，中间线X值，向 上侧相机中间线右边的点对齐，右移 maxColst - meanmidColsnew
                                //然后再向右移动 DistanceRPre * Costl  再向下 移动 DistanceRPre * Sintl
                                //Right3DLineX 为旋转后，左3D相机拍出来的方棒上边缘的X位置
                                //Right3DX 为3D相机线所在位置，这个位置为新的方棒，左侧3D相机移动拟合的位置。
                                //Right3DLineX 为旋转后，右侧3D相机拍出来的方棒右边缘的X位置
                                //Right3DX 为3D相机线所在位置，这个位置为新的方棒，左侧3D相机移动拟合的位置。



                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, Nrleft, Ncleft, Distleft)
                                hv_RowBeginleft.Dispose(); hv_ColBeginleft.Dispose(); hv_RowEndleft.Dispose(); hv_ColEndleft.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourleft, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginleft, out hv_ColBeginleft, out hv_RowEndleft,
                                    out hv_ColEndleft, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_RowBeginrigt.Dispose(); hv_ColBeginrigt.Dispose(); hv_RowEndrigt.Dispose(); hv_ColEndrigt.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigt.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourrigt, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginrigt, out hv_ColBeginrigt, out hv_RowEndrigt,
                                    out hv_ColEndrigt, out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigt);
                                hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoprnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl,
                                    out hv_ColEndlefl, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourdowrnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr,
                                    out hv_ColEndlefr, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);

                                hv_RowIntersectiontr.Dispose(); hv_ColIntersectiontr.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefl, hv_ColBeginlefl, hv_RowEndlefl,
                                    hv_ColEndlefl, hv_RowBeginlefr, hv_ColBeginlefr, hv_RowEndlefr,
                                    hv_ColEndlefr, out hv_RowIntersectiontr, out hv_ColIntersectiontr,
                                    out hv_IsOverlapping);
                                hv_RowIntersectiont.Dispose(); hv_ColIntersectiont.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginleft, hv_ColBeginleft, hv_RowEndleft,
                                    hv_ColEndleft, hv_RowBeginrigt, hv_ColBeginrigt, hv_RowEndrigt,
                                    hv_ColEndrigt, out hv_RowIntersectiont, out hv_ColIntersectiont,
                                    out hv_IsOverlapping);
                                hv_DistanceRT.Dispose();
                                HOperatorSet.DistancePp(hv_RowIntersectiontr, hv_ColIntersectiontr,
                                    hv_RowIntersectiont, hv_ColIntersectiont, out hv_DistanceRT);
                                {
                                    HTuple ExpTmpOutVar_0;
                                    HOperatorSet.TupleConcat(hv_DistanceRT, hv_DistancesRT, out ExpTmpOutVar_0);
                                    hv_DistancesRT.Dispose();
                                    hv_DistancesRT = ExpTmpOutVar_0;
                                }
                            }


                            //以上侧3D相机激光线侧的坐标为原点， 根据标定块，进行拟合。求出右侧3D相机的坐标位置
                            if ((int)(new HTuple(hv_RightDDistance.TupleNotEqual(0))) != 0)
                            {

                                //与上3D相机，左侧对齐
                                hv_Arear.Dispose(); hv_Rowr.Dispose(); hv_Columnr.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursr, out hv_Arear, out hv_Rowr,
                                    out hv_Columnr, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowsd - hv_meanRowsr,
                                        hv_minColsd - hv_minColsr, out hv_HomMatTransPreR);
                                }
                                ho_SelectedContoursrPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursr, out ho_SelectedContoursrPre,
                                    hv_HomMatTransPreR);

                                //顺时针旋转90度， 中间的线的中点为旋转中心
                                hv_Arear.Dispose(); hv_RowPrer.Dispose(); hv_ColumnPrer.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursrPre, out hv_Arear, out hv_RowPrer,
                                    out hv_ColumnPrer, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DRRotate.Dispose();
                                    HOperatorSet.HomMat2dRotate(hv_HomMat2DIdentity, (new HTuple(-90)).TupleRad()
                                        , hv_RowPrer.TupleSelect(1), hv_ColumnPrer.TupleSelect(1), out hv_HomMat2DRRotate);
                                }
                                ho_SelectedContoursrfinal.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrPre, out ho_SelectedContoursrfinal,
                                    hv_HomMat2DRRotate);

                                //沿着SelectedContourslfinal 中间线左侧点，以X轴为中心，进行水平反转
                                //上相机右侧与左侧相机左侧对应，所以需要反转
                                hv_Arear.Dispose(); hv_RowPrer.Dispose(); hv_ColumnPrer.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursrfinal, out hv_Arear,
                                    out hv_RowPrer, out hv_ColumnPrer, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DScale.Dispose();
                                    HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_RowPrer.TupleSelect(
                                        1), hv_ColumnPrer.TupleSelect(1), out hv_HomMat2DScale);
                                }
                                ho_SelectedContoursrnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrfinal, out ho_SelectedContoursrnewpre,
                                    hv_HomMat2DScale);


                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourdowrnew,
                                    3);



                                hv_RowBeginmidnewr.Dispose(); hv_ColBeginmidnewr.Dispose(); hv_RowEndmidnewr.Dispose(); hv_ColEndmidnewr.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmidrnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmidnewr, out hv_ColBeginmidnewr, out hv_RowEndmidnewr,
                                    out hv_ColEndmidnewr, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_Rowmidrmin.Dispose();
                                HOperatorSet.TupleMin2(hv_RowBeginmidnewr, hv_RowEndmidnewr, out hv_Rowmidrmin);
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowsd - hv_Rowmidrmin;
                                }
                                hv_Rowsrmidnew.Dispose(); hv_Colsrmidnew.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidrnew, out hv_Rowsrmidnew, out hv_Colsrmidnew);
                                hv_meanmidColsnew.Dispose();
                                HOperatorSet.TupleMean(hv_Colsrmidnew, out hv_meanmidColsnew);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_maxColsd - hv_meanmidColsnew,
                                        out hv_HomMatTransPreR);
                                }
                                ho_SelectedContoursrprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrnewpre, out ho_SelectedContoursrprenew,
                                    hv_HomMatTransPreR);

                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourdowrnew,
                                    3);

                                hv_RowsTopr.Dispose(); hv_ColsTopr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourtoprnew, out hv_RowsTopr, out hv_ColsTopr);
                                hv_RowsDowr.Dispose(); hv_ColsDowr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourdowrnew, out hv_RowsDowr, out hv_ColsDowr);

                                hv_meanTopr.Dispose();
                                HOperatorSet.TupleMean(hv_RowsTopr, out hv_meanTopr);
                                hv_meanDowr.Dispose();
                                HOperatorSet.TupleMean(hv_RowsDowr, out hv_meanDowr);

                                if ((int)(new HTuple(hv_meanTopr.TupleGreater(hv_meanDowr))) != 0)
                                {
                                    ho_contourtmp.Dispose();
                                    ho_contourtmp = new HObject(ho_contourtoprnew);
                                    ho_contourtoprnew.Dispose();
                                    ho_contourtoprnew = new HObject(ho_contourdowrnew);
                                    ho_contourdowrnew.Dispose();
                                    ho_contourdowrnew = new HObject(ho_contourtmp);
                                }

                                //新的图形SelectedContoursrfinal，上侧边的角度要与 上3D相机采集图像左侧边角度对齐
                                hv_RowBegintopr.Dispose(); hv_ColBegintopr.Dispose(); hv_RowEndtopr.Dispose(); hv_ColEndtopr.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoprnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopr, out hv_ColBegintopr, out hv_RowEndtopr,
                                    out hv_ColEndtopr, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                if ((int)(new HTuple(hv_RowBegintopr.TupleGreater(hv_RowEndtopr))) != 0)
                                {
                                    hv_Angleradtr.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowBegintopr, hv_ColBegintopr, hv_RowEndtopr,
                                        hv_ColEndtopr, out hv_Angleradtr);
                                }
                                else
                                {
                                    hv_Angleradtr.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowEndtopr, hv_ColEndtopr, hv_RowBegintopr,
                                        hv_ColBegintopr, out hv_Angleradtr);
                                }
                                hv_Degrtr.Dispose();
                                HOperatorSet.TupleDeg(hv_Angleradtr, out hv_Degrtr);
                                hv_DegrtrAbs.Dispose();
                                HOperatorSet.TupleAbs(hv_Degrtr, out hv_DegrtrAbs);
                                hv_DegSubR.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubR = hv_DegtlAbs - hv_DegrtrAbs;
                                }

                                //if (DegSubR > 0.1 or DegSubR < -0.1)
                                //area_center_xld (SelectedContoursrprenew, Arear, Rowr, Columnr, PointOrderr)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(DegSubR), Columnr[1], Rowr[1], HomMat2DRRotate)
                                //affine_trans_contour_xld (SelectedContoursrprenew, SelectedContoursrfinalnew, HomMat2DRRotate)
                                //select_obj (SelectedContoursrfinalnew, contourtoprnew, 1)
                                //select_obj (SelectedContoursrfinalnew, contourmidrnew, 2)
                                //select_obj (SelectedContoursrfinalnew, contourdowrnew, 3)
                                //get_contour_xld (contourtoprnew, RowsTopr, ColsTopr)
                                //get_contour_xld (contourdowrnew, RowsDowr, ColsDowr)
                                //tuple_mean (RowsTopr, meanTopr)
                                //tuple_mean (RowsDowr, meanDowr)
                                //if (meanTopr > meanDowr)
                                //contourtmp := contourtoprnew
                                //contourtoprnew := contourdowrnew
                                //contourdowrnew := contourtmp
                                //endif
                                //fit_line_contour_xld (contourtoprnew, 'tukey', -1, 10, 5, 2, RowBegintopr, ColBegintopr, RowEndtopr, ColEndtopr, Nrleft, Ncleft, Distleft)
                                //if (RowBegintopr > RowEndtopr)
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowBegintopr, ColBegintopr, RowEndtopr, ColEndtopr, Angleradtr)
                                //else
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowEndtopr, ColEndtopr, RowBegintopr, ColBegintopr, Angleradtr)
                                //endif
                                //tuple_deg (Angleradtr, Degrtr)
                                //tuple_abs (Degrtr, DegrtrAbs)
                                //DegSubR := DegtlAbs - DegrtrAbs
                                //SelectedContoursrprenew := SelectedContoursrfinalnew
                                //endif


                                //标定块拟合，需要根据标定块边长计算左移距离，  ( 182 - 12 ) * sin45 = 111.72, 移动后就是左边3D相机的具体位置
                                //( 182 - 12 ) * sin45  + meanRowsl , 下次按照这个思路进行偏移，根据新的方棒左3D相机采集的高度，进行
                                //计算，例如标定快高度为 -13,  新的方棒标定块 -11， 那么左移动就是（-11 + 13） = 2， 左移动 111.72 + 2
                                //则与3D位置相同。要沿着上面3D，左边的角度进行移动。 右边3D处理与此类似。

                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourmidrnew,
                                    2);
                                hv_RowsMidr.Dispose(); hv_ColsMidr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidrnew, out hv_RowsMidr, out hv_ColsMidr);
                                hv_MeanMidrx.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidr, out hv_MeanMidrx);


                                hv_AngleradtrAbs.Dispose();
                                HOperatorSet.TupleAbs(hv_Angleraddtr, out hv_AngleradtrAbs);
                                hv_Sintr.Dispose();
                                HOperatorSet.TupleSin(hv_AngleradtrAbs, out hv_Sintr);
                                hv_Costr.Dispose();
                                HOperatorSet.TupleCos(hv_AngleradtrAbs, out hv_Costr);
                                hv_AbsSintr.Dispose();
                                HOperatorSet.TupleAbs(hv_Sintr, out hv_AbsSintr);
                                hv_AbsCostr.Dispose();
                                HOperatorSet.TupleAbs(hv_Costr, out hv_AbsCostr);

                                //Right3DX + meanRowsr 为 contourmidrnew 需要移动到的位置 X坐标
                                //tuple_abs (meanRowsr, AbsRight3DDistance)
                                hv_AbsmeanRowsr.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsr, out hv_AbsmeanRowsr);
                                hv_DistanceXPre.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DistanceXPre = (hv_RightDDistance - hv_AbsmeanRowsr) - ((hv_maxColst - hv_minColst) / 2);
                                }
                                hv_DistanceRPre.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DistanceRPre = hv_DistanceXPre / hv_AbsCostr;
                                }


                                //DistanceRPre := 158.75
                                //hom_mat2d_translate (HomMat2DIdentity, 111.72, 111.72, HomMatTransPreR)

                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_DistanceRPre * hv_AbsSintr,
                                        hv_DistanceRPre * hv_AbsCostr, out hv_HomMatTransPreR);
                                }


                                ho_SelectedContoursrnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrprenew, out ho_SelectedContoursrnew,
                                    hv_HomMatTransPreR);
                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourdowrnew,
                                    3);

                                //
                                //DistanceRPre 为边长
                                //X轴  先向左 移动  meanColst - meanColsr
                                //Y轴  先移动 meanRowst -  RowBeginmidr
                                //逆时针旋转90度，   右侧相机左侧跟上侧相机左侧相对。上侧相机采集图像是否需要以中间线中点纵向翻转？
                                //根据新的图形SelectedContoursrfinal，中间线X值，向 上侧相机中间线右边的点对齐，右移 maxColst - meanmidColsnew
                                //然后再向右移动 DistanceRPre * Costl  再向下 移动 DistanceRPre * Sintl
                                //Right3DLineX 为旋转后，左3D相机拍出来的方棒上边缘的X位置
                                //Right3DX 为3D相机线所在位置，这个位置为新的方棒，左侧3D相机移动拟合的位置。
                                //Right3DLineX 为旋转后，右侧3D相机拍出来的方棒右边缘的X位置
                                //Right3DX 为3D相机线所在位置，这个位置为新的方棒，左侧3D相机移动拟合的位置。



                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, Nrleft, Ncleft, Distleft)
                                hv_RowBeginlefd.Dispose(); hv_ColBeginlefd.Dispose(); hv_RowEndlefd.Dispose(); hv_ColEndlefd.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distlefd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourlefd, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginlefd, out hv_ColBeginlefd, out hv_RowEndlefd,
                                    out hv_ColEndlefd, out hv_Nrleft, out hv_Ncleft, out hv_Distlefd);
                                hv_RowBeginrigd.Dispose(); hv_ColBeginrigd.Dispose(); hv_RowEndrigd.Dispose(); hv_ColEndrigd.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourrigd, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginrigd, out hv_ColBeginrigd, out hv_RowEndrigd,
                                    out hv_ColEndrigd, out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigd);
                                hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoprnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl,
                                    out hv_ColEndlefl, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourdowrnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr,
                                    out hv_ColEndlefr, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);

                                hv_RowIntersectiontr.Dispose(); hv_ColIntersectiontr.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefl, hv_ColBeginlefl, hv_RowEndlefl,
                                    hv_ColEndlefl, hv_RowBeginlefr, hv_ColBeginlefr, hv_RowEndlefr,
                                    hv_ColEndlefr, out hv_RowIntersectiontr, out hv_ColIntersectiontr,
                                    out hv_IsOverlapping);
                                hv_RowIntersectiont.Dispose(); hv_ColIntersectiont.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefd, hv_ColBeginlefd, hv_RowEndlefd,
                                    hv_ColEndlefd, hv_RowBeginrigd, hv_ColBeginrigd, hv_RowEndrigd,
                                    hv_ColEndrigd, out hv_RowIntersectiont, out hv_ColIntersectiont,
                                    out hv_IsOverlapping);
                                hv_DistanceRD.Dispose();
                                HOperatorSet.DistancePp(hv_RowIntersectiontr, hv_ColIntersectiontr,
                                    hv_RowIntersectiont, hv_ColIntersectiont, out hv_DistanceRD);
                                {
                                    HTuple ExpTmpOutVar_0;
                                    HOperatorSet.TupleConcat(hv_DistanceRD, hv_DistancesRD, out ExpTmpOutVar_0);
                                    hv_DistancesRD.Dispose();
                                    hv_DistancesRD = ExpTmpOutVar_0;
                                }
                            }

                            if ((int)(new HTuple(hv_DownDistance.TupleNotEqual(0))) != 0)
                            {
                                //先与上侧相机采集的图像中间线的中点进行对齐

                                hv_Aread.Dispose(); hv_Rowd.Dispose(); hv_Columnd.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursd, out hv_Aread, out hv_Rowd,
                                    out hv_Columnd, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowst - hv_meanRowsd,
                                        hv_minColst - hv_minColsd, out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursdPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursd, out ho_SelectedContoursdPre,
                                    hv_HomMatTransPreD);

                                //以中间线做水平翻转
                                //注释部分继续研究，水平翻转过  沿着中线中点进行水平翻转  180
                                //area_center_xld (SelectedContoursdPre, Aread, Rowd, Columnd, PointOrderl)

                                //select_obj (SelectedContoursdPre, contourmiddnew, 2)
                                //get_contour_xld (contourmiddnew, RowsMidr, ColsMidr)
                                //tuple_mean (ColsMidr, MeanMidrx)
                                //tuple_mean (RowsMidr, MeanMidry)
                                //hom_mat2d_rotate (HomMat2DIdentity, 3.14, 56.2, -1, HomMat2DRotate)
                                //affine_trans_contour_xld (SelectedContoursdPre, SelectedContoursdnewpre, HomMat2DRotate)

                                hv_Aread.Dispose(); hv_RowPred.Dispose(); hv_ColumnPred.Dispose(); hv_PointOrderd.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursdPre, out hv_Aread, out hv_RowPred,
                                    out hv_ColumnPred, out hv_PointOrderd);
                                hv_HomMat2DScale.Dispose();
                                HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_meanRowsd,
                                    0, out hv_HomMat2DScale);
                                ho_SelectedContoursdnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursdPre, out ho_SelectedContoursdnewpre,
                                    hv_HomMat2DScale);

                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewpre, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewpre, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewpre, out ho_contourdowdnew,
                                    3);

                                hv_Rowmiddnew.Dispose(); hv_Colmiddnew.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmiddnew, out hv_Rowmiddnew, out hv_Colmiddnew);
                                hv_meannewmid.Dispose();
                                HOperatorSet.TupleMean(hv_Rowmiddnew, out hv_meannewmid);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowst - hv_meannewmid,
                                        0, out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursdnewnpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursdnewpre, out ho_SelectedContoursdnewnpre,
                                    hv_HomMatTransPreD);

                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewnpre, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewnpre, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewnpre, out ho_contourdowdnew,
                                    3);

                                //新的图形SelectedContoursdnewpre 中间线与上侧相机采集图像的中间线，角度保持一致
                                hv_RowBegintopd.Dispose(); hv_ColBegintopd.Dispose(); hv_RowEndtopd.Dispose(); hv_ColEndtopd.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmiddnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopd, out hv_ColBegintopd, out hv_RowEndtopd,
                                    out hv_ColEndtopd, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_angleltx.Dispose();
                                HOperatorSet.AngleLx(hv_RowBegintopd, hv_ColBegintopd, hv_RowEndtopd,
                                    hv_ColEndtopd, out hv_angleltx);
                                hv_Degltx.Dispose();
                                HOperatorSet.TupleDeg(hv_angleltx, out hv_Degltx);
                                hv_Degltxabs.Dispose();
                                HOperatorSet.TupleAbs(hv_Degltx, out hv_Degltxabs);
                                hv_DegSubD.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubD = hv_DegtdAbs - hv_Degltxabs;
                                }

                                //if (DegSubD > 0.1 or DegSubD < -0.1)
                                //area_center_xld (SelectedContoursdnewpre, Aread, Rowd, Columnd, PointOrderd)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(-DegSubD), Columnd[1], Rowd[1], HomMat2DDRotate)
                                //affine_trans_contour_xld (SelectedContoursdnewpre, SelectedContoursdfinalnew, HomMat2DDRotate)
                                //select_obj (SelectedContoursdfinalnew, contourtopdnew, 1)
                                //select_obj (SelectedContoursdfinalnew, contourmiddnew, 2)
                                //select_obj (SelectedContoursdfinalnew, contourdowdnew, 3)
                                //fit_line_contour_xld (contourmiddnew, 'tukey', -1, 10, 5, 2, RowBegintopd, ColBegintopd, RowEndtopd, ColEndtopd, Nrleft, Ncleft, Distleft)
                                //angle_lx (RowBegintopd, ColBegintopd, RowEndtopd, ColEndtopd, angleltx)
                                //tuple_deg (angleltx, Degltx)
                                //tuple_abs (Degltx, Degltxabs)
                                //DegSubD := DegtdAbs - Degltx
                                //SelectedContoursdnewpre := SelectedContoursdfinalnew
                                //endif

                                hv_RowBeginmiddnew.Dispose(); hv_ColBeginmiddnew.Dispose(); hv_RowEndmiddnew.Dispose(); hv_ColEndmiddnew.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmiddnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmiddnew, out hv_ColBeginmiddnew, out hv_RowEndmiddnew,
                                    out hv_ColEndmiddnew, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowst - hv_RowBeginmiddnew;
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_meanColsd - hv_meanColst,
                                        out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursdprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursdnewnpre, out ho_SelectedContoursdprenew,
                                    hv_HomMatTransPreD);

                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdprenew, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdprenew, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdprenew, out ho_contourdowdnew,
                                    3);
                                hv_RowMidDown.Dispose(); hv_ColMidDown.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmiddnew, out hv_RowMidDown, out hv_ColMidDown);
                                hv_meanDownRNew.Dispose();
                                HOperatorSet.TupleMean(hv_RowMidDown, out hv_meanDownRNew);

                                //Down3DY - AbsDown3DDistance 为 contourmiddnew 需要移动到的位置 Y坐标
                                hv_AbsDown3DDistance.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsd, out hv_AbsDown3DDistance);
                                hv_AbsmeanRowst.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowst, out hv_AbsmeanRowst);
                                hv_Distancediagonal.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_Distancediagonal = (hv_DownDistance - hv_AbsDown3DDistance) - hv_AbsmeanRowst;
                                }

                                //Distancediagonal := 241
                                hv_HomMatTransPreD.Dispose();
                                HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_Distancediagonal,
                                    0, out hv_HomMatTransPreD);
                                ho_SelectedContoursdnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursdprenew, out ho_SelectedContoursdnew,
                                    hv_HomMatTransPreD);
                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnew, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnew, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnew, out ho_contourdowdnew,
                                    3);

                                hv_RowMidDown.Dispose(); hv_ColMidDown.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmiddnew, out hv_RowMidDown, out hv_ColMidDown);
                                hv_meanDownRNew.Dispose();
                                HOperatorSet.TupleMean(hv_RowMidDown, out hv_meanDownRNew);
                                hv_meanDownCNew.Dispose();
                                HOperatorSet.TupleMean(hv_ColMidDown, out hv_meanDownCNew);
                                hv_distancediag_td.Dispose();
                                HOperatorSet.DistancePp(hv_meanDownRNew, hv_meanDownCNew, hv_meanRowst,
                                    hv_meanColst, out hv_distancediag_td);
                                {
                                    HTuple ExpTmpOutVar_0;
                                    HOperatorSet.TupleConcat(hv_distancediag_td, hv_DistancesTD, out ExpTmpOutVar_0);
                                    hv_DistancesTD.Dispose();
                                    hv_DistancesTD = ExpTmpOutVar_0;
                                }
                                //
                                //Distancediagonal 为上下倒角斜边之间的长度
                                //X轴  先向左 移动  meanColst - meanColsd，与上侧相机采集的图像的中间线中点保持一致
                                //以中间线为轴，水平翻转。
                                //Y轴  先移动 meanRowst - RowBeginmidd,与上侧相机采集图像中间线保持高度一致
                                //中间线角度与上侧3D相机采集图像的中间线保持角度一致
                                //然后再向下移动 Distancediagonal

                                //Down3DLineY 为旋转后，下3D相机拍出来的方棒上边缘的Y位置
                                //Down3DY 为3D相机出激光线所在位置，这个位置为新的方棒下侧相机扫描线，反转后向下移动拟合的位置。

                                //Down3DLineY := meanDownRNew
                                //tuple_abs (meanRowsd, AbsDown3DDistance)
                                //Down3DY := Down3DLineY + AbsDown3DDistance

                                //fit_line_contour_xld (contourleft, 'tukey', -1, 10, 5, 2, RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, Nrleft, Ncleft, Distleft)
                                //fit_line_contour_xld (contourrigt, 'tukey', -1, 10, 5, 2, RowBeginrigt, ColBeginrigt, RowEndrigt, ColEndrigt, Nrrigt, Ncrigt, Distrigt)
                                //fit_line_contour_xld (contourtopdnew, 'tukey', -1, 10, 5, 2, RowBeginlefl, ColBeginlefl, RowEndlefl, ColEndlefl, Nrmidd, Ncmidd, Distmidd)
                                //fit_line_contour_xld (contourdowdnew, 'tukey', -1, 10, 5, 2, RowBeginlefr, ColBeginlefr, RowEndlefr, ColEndlefr, Nrmidd, Ncmidd, Distmidd)

                                //intersection_lines (RowBeginlefl, ColBeginlefl, RowEndlefl, ColEndlefl, RowBeginlefr, ColBeginlefr, RowEndlefr, ColEndlefr, RowIntersectiontd, ColIntersectiontd, IsOverlapping)
                                //intersection_lines (RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, RowBeginrigt, ColBeginrigt, RowEndrigt, ColEndrigt, RowIntersectiont, ColIntersectiont, IsOverlapping)
                                //distance_pp (RowIntersectiontd, ColIntersectiontd, RowIntersectiont, ColIntersectiont, DistanceTD)
                                //DistanceLR := DistanceTD - distop_verlength - disdown_verlength
                                //tuple_concat (DistanceTD, DistancesTD, DistancesTD)

                                //distance_pp (RowIntersectiontd, ColIntersectiontd, RowIntersectiontr, ColIntersectiontr, DistanceDR)
                                //distance_pp (RowIntersectiontd, ColIntersectiontd, RowIntersectiontl, ColIntersectiontl, DistanceDL)
                            }

                            if ((int)(new HTuple(hv_LeftRightDistance.TupleNotEqual(0))) != 0)
                            {


                                //以相机采集图像，中间线作为参考线有问题。需要以相机激光口横线为参照物，求出下3D相机相对上3D相机的纵向坐标。
                                //先与上侧相机采集的图像中间线的中点进行对齐

                                hv_Arear.Dispose(); hv_Rowr.Dispose(); hv_Columnr.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursr, out hv_Arear, out hv_Rowr,
                                    out hv_Columnr, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowsl - hv_meanRowsr,
                                        hv_minColsl - hv_minColsr, out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursrPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursr, out ho_SelectedContoursrPre,
                                    hv_HomMatTransPreD);

                                //以中间线做水平翻转
                                //注释部分继续研究，水平翻转过  沿着中线中点进行水平翻转  180

                                hv_Arear.Dispose(); hv_RowPrer.Dispose(); hv_ColumnPrer.Dispose(); hv_PointOrderd.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursrPre, out hv_Arear, out hv_RowPrer,
                                    out hv_ColumnPrer, out hv_PointOrderd);
                                hv_HomMat2DScale.Dispose();
                                HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_meanRowsr,
                                    0, out hv_HomMat2DScale);
                                ho_SelectedContoursrnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrPre, out ho_SelectedContoursrnewpre,
                                    hv_HomMat2DScale);


                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourdowdnew,
                                    3);

                                hv_Rowmiddnew.Dispose(); hv_Colmiddnew.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmiddnew, out hv_Rowmiddnew, out hv_Colmiddnew);
                                hv_meannewmid.Dispose();
                                HOperatorSet.TupleMean(hv_Rowmiddnew, out hv_meannewmid);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowsl - hv_meannewmid,
                                        0, out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursrnewnpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrnewpre, out ho_SelectedContoursrnewnpre,
                                    hv_HomMatTransPreD);

                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewnpre, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewnpre, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewnpre, out ho_contourdowdnew,
                                    3);

                                //新的图形SelectedContoursdnewpre 中间线与上侧相机采集图像的中间线，角度保持一致
                                hv_RowBegintopd.Dispose(); hv_ColBegintopd.Dispose(); hv_RowEndtopd.Dispose(); hv_ColEndtopd.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmiddnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopd, out hv_ColBegintopd, out hv_RowEndtopd,
                                    out hv_ColEndtopd, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_angleltx.Dispose();
                                HOperatorSet.AngleLx(hv_RowBegintopd, hv_ColBegintopd, hv_RowEndtopd,
                                    hv_ColEndtopd, out hv_angleltx);
                                hv_Degltx.Dispose();
                                HOperatorSet.TupleDeg(hv_angleltx, out hv_Degltx);
                                hv_Degltxabs.Dispose();
                                HOperatorSet.TupleAbs(hv_Degltx, out hv_Degltxabs);
                                hv_DegSubD.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubD = hv_DegtdAbs - hv_Degltxabs;
                                }

                                //if (DegSubD > 0.1 or DegSubD < -0.1)
                                //area_center_xld (SelectedContoursdnewpre, Aread, Rowd, Columnd, PointOrderd)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(-DegSubD), Columnd[1], Rowd[1], HomMat2DDRotate)
                                //affine_trans_contour_xld (SelectedContoursdnewpre, SelectedContoursdfinalnew, HomMat2DDRotate)
                                //select_obj (SelectedContoursdfinalnew, contourtopdnew, 1)
                                //select_obj (SelectedContoursdfinalnew, contourmiddnew, 2)
                                //select_obj (SelectedContoursdfinalnew, contourdowdnew, 3)
                                //fit_line_contour_xld (contourmiddnew, 'tukey', -1, 10, 5, 2, RowBegintopd, ColBegintopd, RowEndtopd, ColEndtopd, Nrleft, Ncleft, Distleft)
                                //angle_lx (RowBegintopd, ColBegintopd, RowEndtopd, ColEndtopd, angleltx)
                                //tuple_deg (angleltx, Degltx)
                                //tuple_abs (Degltx, Degltxabs)
                                //DegSubD := DegtdAbs - Degltx
                                //SelectedContoursdnewpre := SelectedContoursdfinalnew
                                //endif

                                hv_RowBeginmiddnew.Dispose(); hv_ColBeginmiddnew.Dispose(); hv_RowEndmiddnew.Dispose(); hv_ColEndmiddnew.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmiddnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmiddnew, out hv_ColBeginmiddnew, out hv_RowEndmiddnew,
                                    out hv_ColEndmiddnew, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowsl - hv_RowBeginmiddnew;
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_meanColsd - hv_meanColst,
                                        out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursrprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrnewnpre, out ho_SelectedContoursrprenew,
                                    hv_HomMatTransPreD);

                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourdowdnew,
                                    3);
                                hv_RowMidDown.Dispose(); hv_ColMidDown.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmiddnew, out hv_RowMidDown, out hv_ColMidDown);
                                hv_meanDownRNew.Dispose();
                                HOperatorSet.TupleMean(hv_RowMidDown, out hv_meanDownRNew);

                                //Down3DY - AbsDown3DDistance 为 contourmiddnew 需要移动到的位置 Y坐标
                                hv_AbsDown3DDistance.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsr, out hv_AbsDown3DDistance);
                                hv_AbsmeanRowst.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsl, out hv_AbsmeanRowst);
                                hv_Distancediagonal.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_Distancediagonal = (hv_LeftRightDistance - hv_AbsDown3DDistance) - hv_AbsmeanRowst;
                                }

                                //Distancediagonal := 241
                                hv_HomMatTransPreD.Dispose();
                                HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_Distancediagonal,
                                    0, out hv_HomMatTransPreD);
                                ho_SelectedContoursrnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrprenew, out ho_SelectedContoursrnew,
                                    hv_HomMatTransPreD);
                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourdowdnew,
                                    3);

                                hv_RowMidDown.Dispose(); hv_ColMidDown.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmiddnew, out hv_RowMidDown, out hv_ColMidDown);

                                hv_meanRC.Dispose();
                                HOperatorSet.TupleMean(hv_ColMidDown, out hv_meanRC);
                                hv_meanRR.Dispose();
                                HOperatorSet.TupleMean(hv_RowMidDown, out hv_meanRR);
                                hv_distancediag_lr.Dispose();
                                HOperatorSet.DistancePp(hv_meanRowsl, hv_meanColsl, hv_meanRR, hv_meanRC,
                                    out hv_distancediag_lr);
                                {
                                    HTuple ExpTmpOutVar_0;
                                    HOperatorSet.TupleConcat(hv_distancediag_lr, hv_DistancesLR, out ExpTmpOutVar_0);
                                    hv_DistancesLR.Dispose();
                                    hv_DistancesLR = ExpTmpOutVar_0;
                                }
                                //
                                //Distancediagonal 为上下倒角斜边之间的长度
                                //X轴  先向左 移动  meanColst - meanColsd，与上侧相机采集的图像的中间线中点保持一致
                                //以中间线为轴，水平翻转。
                                //Y轴  先移动 meanRowst - RowBeginmidd,与上侧相机采集图像中间线保持高度一致
                                //中间线角度与上侧3D相机采集图像的中间线保持角度一致
                                //然后再向下移动 Distancediagonal

                                //Down3DLineY 为旋转后，下3D相机拍出来的方棒上边缘的Y位置
                                //Down3DY 为3D相机出激光线所在位置，这个位置为新的方棒下侧相机扫描线，反转后向下移动拟合的位置。
                                //tuple_mean (RowMidDown, meanDownRNew)
                                //Down3DLineY := meanDownRNew
                                //tuple_abs (meanRowsd, AbsDown3DDistance)
                                //Down3DY := Down3DLineY + AbsDown3DDistance


                                //tuple_concat (DistanceLR, DistancesLR, DistancesLR)
                                //fit_line_contour_xld (contourlefl, 'tukey', -1, 10, 5, 2, RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, Nrleft, Ncleft, Distleft)
                                //fit_line_contour_xld (contourrigl, 'tukey', -1, 10, 5, 2, RowBeginrigt, ColBeginrigt, RowEndrigt, ColEndrigt, Nrrigt, Ncrigt, Distrigt)
                                //fit_line_contour_xld (contourtopdnew, 'tukey', -1, 10, 5, 2, RowBeginlefl, ColBeginlefl, RowEndlefl, ColEndlefl, Nrmidd, Ncmidd, Distmidd)
                                //fit_line_contour_xld (contourdowdnew, 'tukey', -1, 10, 5, 2, RowBeginlefr, ColBeginlefr, RowEndlefr, ColEndlefr, Nrmidd, Ncmidd, Distmidd)

                                //intersection_lines (RowBeginlefl, ColBeginlefl, RowEndlefl, ColEndlefl, RowBeginlefr, ColBeginlefr, RowEndlefr, ColEndlefr, RowIntersectiontd, ColIntersectiontd, IsOverlapping)
                                //intersection_lines (RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, RowBeginrigt, ColBeginrigt, RowEndrigt, ColEndrigt, RowIntersectiont, ColIntersectiont, IsOverlapping)
                                //distance_pp (RowIntersectiontd, ColIntersectiontd, RowIntersectiont, ColIntersectiont, DistanceLR)
                                //DistanceLR := DistanceLR - disleft_verlength - disright_verlength
                                //tuple_concat (DistanceLR, DistancesLR, DistancesLR)
                                //distance_pp (RowIntersectiontd, ColIntersectiontd, RowIntersectiontr, ColIntersectiontr, DistanceDR)
                                //distance_pp (RowIntersectiontd, ColIntersectiontd, RowIntersectiontl, ColIntersectiontl, DistanceDL)
                            }


                        }
                        else
                        {
                            continue;
                        }
                    }
                    // catch (Exception) 
                    catch (HalconException HDevExpDefaultException1)
                    {
                        HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    }



                    //fit_circle_contour_xld (ObjectSelected, 'geohuber', -1, 0, 0, 3, 2, Row, Column, Radius1, StartPhi, EndPhi, PointOrder)
                    //gen_circle_contour_xld (ContCircle, Row, Column, Radius1, 0, 6.28318, 'positive', 1)
                    //raduis := [raduis,Radius1]
                    if (HDevWindowStack.IsOpen())
                    {
                        //dev_display (ObjectSelected)
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        //dev_display (SelectedContours)
                    }
                    //disp_message (3600, '角度：' + Angle1 + '°、 ' + Angle2 + '°、 ' + Angle3 + '°', 'image', 20, 20, 'red', 'true')


                }
                hv_meanTopDiag.Dispose();
                HOperatorSet.TupleMean(hv_DistancesTopDiag, out hv_meanTopDiag);
                hv_meanDownDiag.Dispose();
                HOperatorSet.TupleMean(hv_DistancesDownDiag, out hv_meanDownDiag);
                hv_meanLeftDiag.Dispose();
                HOperatorSet.TupleMean(hv_DistancesLeftDiag, out hv_meanLeftDiag);
                hv_meanRightDiag.Dispose();
                HOperatorSet.TupleMean(hv_DistancesRightDiag, out hv_meanRightDiag);
                hv_meanTopLeftDiag.Dispose();
                HOperatorSet.TupleMean(hv_DistancesTopLeftDiag, out hv_meanTopLeftDiag);
                hv_meanTopRightDiag.Dispose();
                HOperatorSet.TupleMean(hv_DistancesTopRightDiag, out hv_meanTopRightDiag);
                hv_meanLeftLeftDiag.Dispose();
                HOperatorSet.TupleMean(hv_DistancesLeftLeftDiag, out hv_meanLeftLeftDiag);
                hv_meanLeftRightDiag.Dispose();
                HOperatorSet.TupleMean(hv_DistancesLeftRightDiag, out hv_meanLeftRightDiag);
                hv_meanRightLeftDiag.Dispose();
                HOperatorSet.TupleMean(hv_DistancesRightLeftDiag, out hv_meanRightLeftDiag);
                hv_meanRightRightDiag.Dispose();
                HOperatorSet.TupleMean(hv_DistancesRightRightDiag, out hv_meanRightRightDiag);
                hv_meanDownLeftDiag.Dispose();
                HOperatorSet.TupleMean(hv_DistancesDownLeftDiag, out hv_meanDownLeftDiag);
                hv_meanDownRightDiag.Dispose();
                HOperatorSet.TupleMean(hv_DistancesDownRightDiag, out hv_meanDownRightDiag);
                hv_meanLT.Dispose();
                HOperatorSet.TupleMean(hv_DistancesLT, out hv_meanLT);
                hv_meanRT.Dispose();
                HOperatorSet.TupleMean(hv_DistancesRT, out hv_meanRT);
                hv_meanRD.Dispose();
                HOperatorSet.TupleMean(hv_DistancesRD, out hv_meanRD);
                hv_meanLD.Dispose();
                HOperatorSet.TupleMean(hv_DistancesLD, out hv_meanLD);
                hv_meanTD.Dispose();
                HOperatorSet.TupleMean(hv_DistancesTD, out hv_meanTD);
                hv_meanLR.Dispose();
                HOperatorSet.TupleMean(hv_DistancesLR, out hv_meanLR);



                hv_SubLength.Dispose();
                hv_SubLength = 0.5;
                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesTopDiag.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesTopDiag.TupleSelect(hv_Index)) - hv_meanTopDiag)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesTopDiag.TupleSelect(
                        hv_Index)) - hv_meanTopDiag)).TupleLess(-hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HTuple ExpTmpOutVar_0;
                        HOperatorSet.TupleRemove(hv_DistancesTopDiag, hv_Index1.TupleSelect(hv_Index),
                            out ExpTmpOutVar_0);
                        hv_DistancesTopDiag.Dispose();
                        hv_DistancesTopDiag = ExpTmpOutVar_0;
                    }
                }

                hv_Functiontmp.Dispose();
                HOperatorSet.CreateFunct1dArray(hv_DistancesTopDiag, out hv_Functiontmp);
                hv_Functiontmpnew.Dispose();
                HOperatorSet.SmoothFunct1dMean(hv_Functiontmp, 3, 2, out hv_Functiontmpnew);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_xtmp.Dispose();
                    HOperatorSet.TupleGenSequence(0, (new HTuple(hv_Functiontmp.TupleLength())) - 1,
                        1, out hv_xtmp);
                }
                hv_DistancesTopDiag.Dispose();
                HOperatorSet.GetYValueFunct1d(hv_Functiontmpnew, hv_xtmp, "constant", out hv_DistancesTopDiag);


                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesDownDiag.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesDownDiag.TupleSelect(hv_Index)) - hv_meanDownDiag)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesDownDiag.TupleSelect(
                        hv_Index)) - hv_meanDownDiag)).TupleLess(-hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HTuple ExpTmpOutVar_0;
                        HOperatorSet.TupleRemove(hv_DistancesDownDiag, hv_Index1.TupleSelect(hv_Index),
                            out ExpTmpOutVar_0);
                        hv_DistancesDownDiag.Dispose();
                        hv_DistancesDownDiag = ExpTmpOutVar_0;
                    }
                }


                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesLeftDiag.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesLeftDiag.TupleSelect(hv_Index)) - hv_meanLeftDiag)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesLeftDiag.TupleSelect(
                        hv_Index)) - hv_meanLeftDiag)).TupleLess(-hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HTuple ExpTmpOutVar_0;
                        HOperatorSet.TupleRemove(hv_DistancesLeftDiag, hv_Index1.TupleSelect(hv_Index),
                            out ExpTmpOutVar_0);
                        hv_DistancesLeftDiag.Dispose();
                        hv_DistancesLeftDiag = ExpTmpOutVar_0;
                    }
                }


                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesRightDiag.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesRightDiag.TupleSelect(hv_Index)) - hv_meanRightDiag)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesRightDiag.TupleSelect(
                        hv_Index)) - hv_meanRightDiag)).TupleLess(-hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_meanRightDiag.Dispose();
                        HOperatorSet.TupleRemove(hv_DistancesRightDiag, hv_Index1.TupleSelect(hv_Index),
                            out hv_meanRightDiag);
                    }
                }


                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesTopLeftDiag.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesTopLeftDiag.TupleSelect(hv_Index)) - hv_meanTopLeftDiag)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesTopLeftDiag.TupleSelect(
                        hv_Index)) - hv_meanTopLeftDiag)).TupleLess(-hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_meanTopLeftDiag.Dispose();
                        HOperatorSet.TupleRemove(hv_DistancesTopLeftDiag, hv_Index1.TupleSelect(hv_Index),
                            out hv_meanTopLeftDiag);
                    }
                }


                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesTopRightDiag.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesTopRightDiag.TupleSelect(hv_Index)) - hv_meanTopRightDiag)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesTopRightDiag.TupleSelect(
                        hv_Index)) - hv_meanTopRightDiag)).TupleLess(-hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_meanTopLeftDiag.Dispose();
                        HOperatorSet.TupleRemove(hv_DistancesTopRightDiag, hv_Index1.TupleSelect(
                            hv_Index), out hv_meanTopLeftDiag);
                    }
                }

                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesTopRightDiag.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesTopRightDiag.TupleSelect(hv_Index)) - hv_meanTopRightDiag)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesTopRightDiag.TupleSelect(
                        hv_Index)) - hv_meanTopRightDiag)).TupleLess(-hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_meanTopLeftDiag.Dispose();
                        HOperatorSet.TupleRemove(hv_DistancesTopRightDiag, hv_Index1.TupleSelect(
                            hv_Index), out hv_meanTopLeftDiag);
                    }
                }

                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesLeftLeftDiag.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesLeftLeftDiag.TupleSelect(hv_Index)) - hv_meanLeftLeftDiag)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesLeftLeftDiag.TupleSelect(
                        hv_Index)) - hv_meanLeftLeftDiag)).TupleLess(-hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_meanLeftLeftDiag.Dispose();
                        HOperatorSet.TupleRemove(hv_DistancesLeftLeftDiag, hv_Index1.TupleSelect(
                            hv_Index), out hv_meanLeftLeftDiag);
                    }
                }

                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesLeftRightDiag.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesLeftRightDiag.TupleSelect(hv_Index)) - hv_meanLeftRightDiag)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesLeftRightDiag.TupleSelect(
                        hv_Index)) - hv_meanLeftLeftDiag)).TupleLess(-hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_meanLeftRightDiag.Dispose();
                        HOperatorSet.TupleRemove(hv_DistancesLeftRightDiag, hv_Index1.TupleSelect(
                            hv_Index), out hv_meanLeftRightDiag);
                    }
                }


                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesRightLeftDiag.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesRightLeftDiag.TupleSelect(hv_Index)) - hv_meanRightLeftDiag)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesRightLeftDiag.TupleSelect(
                        hv_Index)) - hv_meanRightLeftDiag)).TupleLess(-hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_meanRightLeftDiag.Dispose();
                        HOperatorSet.TupleRemove(hv_DistancesRightLeftDiag, hv_Index1.TupleSelect(
                            hv_Index), out hv_meanRightLeftDiag);
                    }
                }


                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesRightRightDiag.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesRightRightDiag.TupleSelect(hv_Index)) - hv_meanRightRightDiag)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesRightRightDiag.TupleSelect(
                        hv_Index)) - hv_meanRightRightDiag)).TupleLess(-hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_meanRightRightDiag.Dispose();
                        HOperatorSet.TupleRemove(hv_DistancesRightRightDiag, hv_Index1.TupleSelect(
                            hv_Index), out hv_meanRightRightDiag);
                    }
                }

                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesLT.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesLT.TupleSelect(hv_Index)) - hv_meanLT)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesLT.TupleSelect(hv_Index)) - hv_meanLT)).TupleLess(
                        -hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_meanLT.Dispose();
                        HOperatorSet.TupleRemove(hv_DistancesLT, hv_Index1.TupleSelect(hv_Index),
                            out hv_meanLT);
                    }
                }

                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesRT.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesRT.TupleSelect(hv_Index)) - hv_meanRT)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesRT.TupleSelect(hv_Index)) - hv_meanRT)).TupleLess(
                        -hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_meanRT.Dispose();
                        HOperatorSet.TupleRemove(hv_DistancesRT, hv_Index1.TupleSelect(hv_Index),
                            out hv_meanRT);
                    }
                }


                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesLD.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesLD.TupleSelect(hv_Index)) - hv_meanLD)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesLD.TupleSelect(hv_Index)) - hv_meanLD)).TupleLess(
                        -hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_meanLD.Dispose();
                        HOperatorSet.TupleRemove(hv_DistancesLD, hv_Index1.TupleSelect(hv_Index),
                            out hv_meanLD);
                    }
                }

                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesRD.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesRD.TupleSelect(hv_Index)) - hv_meanRD)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesRD.TupleSelect(hv_Index)) - hv_meanRD)).TupleLess(
                        -hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_meanRD.Dispose();
                        HOperatorSet.TupleRemove(hv_DistancesRD, hv_Index1.TupleSelect(hv_Index),
                            out hv_meanRD);
                    }
                }

                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesTD.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesTD.TupleSelect(hv_Index)) - hv_meanTD)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesTD.TupleSelect(hv_Index)) - hv_meanTD)).TupleLess(
                        -hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_meanTD.Dispose();
                        HOperatorSet.TupleRemove(hv_DistancesTD, hv_Index1.TupleSelect(hv_Index),
                            out hv_meanTD);
                    }
                }

                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesLR.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesLR.TupleSelect(hv_Index)) - hv_meanLR)).TupleGreater(
                        hv_SubLength))).TupleOr(new HTuple((((hv_DistancesLR.TupleSelect(hv_Index)) - hv_meanLR)).TupleLess(
                        -hv_SubLength)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_meanLR.Dispose();
                        HOperatorSet.TupleRemove(hv_DistancesLR, hv_Index1.TupleSelect(hv_Index),
                            out hv_meanLR);
                    }
                }



                hv_Functiontmp.Dispose();
                HOperatorSet.CreateFunct1dArray(hv_DistancesDownDiag, out hv_Functiontmp);
                //smooth_funct_1d_mean (Functiontmp, 10, 2, Functiontmpnew)
                hv_SmoothedFunction.Dispose();
                HOperatorSet.SmoothFunct1dGauss(hv_Functiontmp, 10, out hv_SmoothedFunction);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_xtmp.Dispose();
                    HOperatorSet.TupleGenSequence(0, (new HTuple(hv_Functiontmp.TupleLength())) - 1,
                        1, out hv_xtmp);
                }
                hv_DistancesDownDiag.Dispose();
                HOperatorSet.GetYValueFunct1d(hv_SmoothedFunction, hv_xtmp, "constant", out hv_DistancesDownDiag);

                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesLeftDiag.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesLeftDiag.TupleSelect(hv_Index)) - hv_meanLeftDiag)).TupleGreater(
                        1))).TupleOr(new HTuple((((hv_DistancesLeftDiag.TupleSelect(hv_Index)) - hv_meanLeftDiag)).TupleLess(
                        -1)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HTuple ExpTmpOutVar_0;
                        HOperatorSet.TupleRemove(hv_DistancesLeftDiag, hv_Index1.TupleSelect(hv_Index),
                            out ExpTmpOutVar_0);
                        hv_DistancesLeftDiag.Dispose();
                        hv_DistancesLeftDiag = ExpTmpOutVar_0;
                    }
                }
                hv_Functiontmp.Dispose();
                HOperatorSet.CreateFunct1dArray(hv_DistancesLeftDiag, out hv_Functiontmp);
                hv_Functiontmpnew.Dispose();
                HOperatorSet.SmoothFunct1dMean(hv_Functiontmp, 3, 2, out hv_Functiontmpnew);
                //smooth_funct_1d_gauss (Functiontmp, 2, SmoothedFunction)
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_xtmp.Dispose();
                    HOperatorSet.TupleGenSequence(0, (new HTuple(hv_Functiontmp.TupleLength())) - 1,
                        1, out hv_xtmp);
                }
                hv_DistancesLeftDiag.Dispose();
                HOperatorSet.GetYValueFunct1d(hv_Functiontmpnew, hv_xtmp, "constant", out hv_DistancesLeftDiag);

                hv_Index1.Dispose();
                hv_Index1 = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_DistancesRightDiag.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)((new HTuple((((hv_DistancesRightDiag.TupleSelect(hv_Index)) - hv_meanDownDiag)).TupleGreater(
                        1))).TupleOr(new HTuple((((hv_DistancesRightDiag.TupleSelect(hv_Index)) - hv_meanDownDiag)).TupleLess(
                        -1)))) != 0)
                    {
                        {
                            HTuple ExpTmpOutVar_0;
                            HOperatorSet.TupleConcat(hv_Index, hv_Index1, out ExpTmpOutVar_0);
                            hv_Index1.Dispose();
                            hv_Index1 = ExpTmpOutVar_0;
                        }
                    }
                }

                for (hv_Index = (new HTuple(hv_Index1.TupleLength())) - 1; (int)hv_Index >= 0; hv_Index = (int)hv_Index + -1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HTuple ExpTmpOutVar_0;
                        HOperatorSet.TupleRemove(hv_DistancesRightDiag, hv_Index1.TupleSelect(hv_Index),
                            out ExpTmpOutVar_0);
                        hv_DistancesRightDiag.Dispose();
                        hv_DistancesRightDiag = ExpTmpOutVar_0;
                    }
                }
                hv_Functiontmp.Dispose();
                HOperatorSet.CreateFunct1dArray(hv_DistancesRightDiag, out hv_Functiontmp);
                hv_Functiontmpnew.Dispose();
                HOperatorSet.SmoothFunct1dMean(hv_Functiontmp, 3, 2, out hv_Functiontmpnew);
                //smooth_funct_1d_gauss (Functiontmp, 2, SmoothedFunction)
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_xtmp.Dispose();
                    HOperatorSet.TupleGenSequence(0, (new HTuple(hv_Functiontmp.TupleLength())) - 1,
                        1, out hv_xtmp);
                }
                hv_DistancesRightDiag.Dispose();
                HOperatorSet.GetYValueFunct1d(hv_Functiontmpnew, hv_xtmp, "constant", out hv_DistancesRightDiag);


                double[] dbDistanceRightDiag =  hv_DistancesRightDiag.ToDArr();
                double[] dbDistanceLeftDiag = hv_DistancesLeftDiag.ToDArr();
                double[] dbDistanceDownDiag = hv_DistancesDownDiag.ToDArr();
                double[] dbDistanceTopDiag = hv_DistancesTopDiag.ToDArr();
                double[] dbDistanceRD = hv_DistancesRD.ToDArr();
                double[] dbDistanceRT = hv_DistancesRT.ToDArr();
                double[] dbDistanceLD = hv_DistancesLD.ToDArr();
                double[] dbDistanceLT = hv_DistancesLT.ToDArr();
                double[] dbDistanceTD = hv_DistancesTD.ToDArr();
                double[] dbDistanceLR = hv_DistancesLR.ToDArr();
                double[] dbDistancesTopLeftDiag = hv_DistancesTopLeftDiag.ToDArr();
                double[] dbDistancesTopRightDiag = hv_DistancesTopRightDiag.ToDArr();
                double[] dbDistancesDownLeftDiag = hv_DistancesDownLeftDiag.ToDArr();
                double[] dbDistancesDownRightDiag = hv_DistancesDownRightDiag.ToDArr();
                double[] dbDistancesLeftLeftDiag = hv_DistancesLeftLeftDiag.ToDArr();
                double[] dbDistancesLeftRightDiag = hv_DistancesLeftRightDiag.ToDArr();
                double[] dbDistancesRightLeftDiag = hv_DistancesRightLeftDiag.ToDArr();
                double[] dbDistancesRightRightDiag = hv_DistancesRightRightDiag.ToDArr();


                StickData data = new StickData();
                data.DbDistanceRightDiag = dbDistanceRightDiag;
                data.DbDistanceLeftDiag = dbDistanceLeftDiag;
                data.DbDistanceDownDiag = dbDistanceDownDiag;
                data.DbDistanceTopDiag = dbDistanceTopDiag;
                data.DbDistanceTD = dbDistanceTD;
                data.DbDistanceLD = dbDistanceLD;
                data.DbDistanceLT = dbDistanceLT;
                data.DbDistanceRD = dbDistanceRD;
                data.DbDistanceLR = dbDistanceLR;
                data.DbDistanceRT = dbDistanceRT;
                data.DbDistancesTopLeftDiag = dbDistancesTopLeftDiag;
                data.DbDistancesTopRightDiag = dbDistancesTopRightDiag;
                data.DbDistancesLeftLeftDiag = dbDistancesLeftLeftDiag;
                data.DbDistancesLeftRightDiag = dbDistancesLeftRightDiag;
                data.DbDistancesRightLeftDiag = dbDistancesRightLeftDiag;
                data.DbDistancesRightRightDiag = dbDistancesRightRightDiag;
                data.DbDistancesDownLeftDiag = dbDistancesDownLeftDiag;
                data.DbDistancesDownRightDiag = dbDistancesDownRightDiag;
                data.StrJBSearial = strJinbian;
                string strInfo = JsonConvert.SerializeObject(data);
                string path = SettingParameter.Instance().StrSaveDir + "/" + strJinbian + "/info.json";

                if (!File.Exists(path))
                {
                    File.Create(path);
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine(strInfo);
                    }
                }
                GlobalDataCache.Instance().AddData(strJinbian, data);
                ho_ImageOutLeft.Dispose();
                ho_ImageOutRight.Dispose();
                ho_ImageOutTop.Dispose();
                ho_ImageOutDown.Dispose();
                ho_ImagePart.Dispose();
                ho_ImagePartReal.Dispose();
                ho_ImagePartLeft.Dispose();
                ho_ImagePartLeftNew.Dispose();
                ho_RegionLeft.Dispose();
                ho_ImageReducedLeft.Dispose();
                ho_RegionErosion.Dispose();
                ho_ImagePartRight.Dispose();
                ho_ImagePartRightNew.Dispose();
                ho_RegionRight.Dispose();
                ho_ImageReducedRight.Dispose();
                ho_ImagePartTop.Dispose();
                ho_ImagePartTopNew.Dispose();
                ho_RegionTop.Dispose();
                ho_ImageReducedTop.Dispose();
                ho_ImagePartDown.Dispose();
                ho_ImagePartDownNew.Dispose();
                ho_RegionDown.Dispose();
                ho_ImageReducedDown.Dispose();
                ho_Bx.Dispose();
                ho_By.Dispose();
                ho_Bz.Dispose();
                ho_Intersectiont.Dispose();
                ho_UnionContourst.Dispose();
                ho_ObjectSelectedt.Dispose();
                ho_ContoursSplitt.Dispose();
                ho_SelectedContourst.Dispose();
                ho_contournewmid.Dispose();
                ho_Intersectionl.Dispose();
                ho_UnionContoursl.Dispose();
                ho_ObjectSelectedl.Dispose();
                ho_ContoursSplitl.Dispose();
                ho_SelectedContoursl.Dispose();
                ho_Intersectionr.Dispose();
                ho_UnionContoursr.Dispose();
                ho_ObjectSelectedr.Dispose();
                ho_ContoursSplitr.Dispose();
                ho_SelectedContoursr.Dispose();
                ho_Intersectiond.Dispose();
                ho_UnionContoursd.Dispose();
                ho_ObjectSelectedd.Dispose();
                ho_ContoursSplitd.Dispose();
                ho_SelectedContoursd.Dispose();
                ho_contourleft.Dispose();
                ho_contourmidt.Dispose();
                ho_contourrigt.Dispose();
                ho_contourlefr.Dispose();
                ho_contourmidr.Dispose();
                ho_contourrigr.Dispose();
                ho_contourlefl.Dispose();
                ho_contourmidl.Dispose();
                ho_contourrigl.Dispose();
                ho_contourlefd.Dispose();
                ho_contourmidd.Dispose();
                ho_contourrigd.Dispose();
                ho_contourtmp.Dispose();
                ho_SelectedContourslPre.Dispose();
                ho_SelectedContourslfinal.Dispose();
                ho_SelectedContourslnewpre.Dispose();
                ho_contourtoplnew.Dispose();
                ho_contourmidlnew.Dispose();
                ho_contourdowlnew.Dispose();
                ho_SelectedContourslprenew.Dispose();
                ho_SelectedContourslnew.Dispose();
                ho_SelectedContoursrPre.Dispose();
                ho_SelectedContoursrfinal.Dispose();
                ho_SelectedContoursrnewpre.Dispose();
                ho_contourtoprnew.Dispose();
                ho_contourmidrnew.Dispose();
                ho_contourdowrnew.Dispose();
                ho_SelectedContoursrprenew.Dispose();
                ho_SelectedContoursrnew.Dispose();
                ho_SelectedContoursdPre.Dispose();
                ho_contourmiddnew.Dispose();
                ho_SelectedContoursdnewpre.Dispose();
                ho_contourtopdnew.Dispose();
                ho_contourdowdnew.Dispose();
                ho_SelectedContoursdnewnpre.Dispose();
                ho_SelectedContoursdprenew.Dispose();
                ho_SelectedContoursdnew.Dispose();
                ho_SelectedContoursrnewnpre.Dispose();

                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_Instructions.Dispose();
                hv_WindowHandle.Dispose();
                hv_Message.Dispose();
                hv_r.Dispose();
                hv_ParameterValues.Dispose();
                hv_Status.Dispose();
                hv_yInterval.Dispose();
                hv_ObjectModel3DBLeft.Dispose();
                hv_ObjectModel3DBLeftConnected.Dispose();
                hv_ObjectModel3DBLeftNew.Dispose();
                hv_ObjectModel3DBRight.Dispose();
                hv_ObjectModel3DBRightConnected.Dispose();
                hv_ObjectModel3DBRightNew.Dispose();
                hv_ObjectModel3DBTop.Dispose();
                hv_ObjectModel3DTopConnected.Dispose();
                hv_ObjectModel3DBTopNew.Dispose();
                hv_ObjectModel3DBDown.Dispose();
                hv_ObjectModel3DBDownConnected.Dispose();
                hv_ObjectModel3DBDownNew.Dispose();
                hv_PoseOut.Dispose();
                hv_SampledObjectModel3DTop.Dispose();
                hv_SampledObjectModel3DDown.Dispose();
                hv_SampledObjectModel3DLeft.Dispose();
                hv_SampledObjectModel3DRight.Dispose();
                hv_Surface3DDefaultT.Dispose();
                hv_Info.Dispose();
                hv_Surface3DDefaultD.Dispose();
                hv_Surface3DDefaultL.Dispose();
                hv_Surface3DDefaultR.Dispose();
                hv_CenterPointT.Dispose();
                hv_Radius.Dispose();
                hv_CenterPointD.Dispose();
                hv_CenterPointL.Dispose();
                hv_CenterPointR.Dispose();
                hv_raduis.Dispose();
                hv_ObjectModel3DIntersections.Dispose();
                hv_DisparityProfileWidth.Dispose();
                hv_DisparityProfileHeight.Dispose();
                hv_WindowEnlargement.Dispose();
                hv_WindowWidth.Dispose();
                hv_WindowHeight.Dispose();
                hv_VisualizationPlaneSize.Dispose();
                hv_PoseT.Dispose();
                hv_PoseD.Dispose();
                hv_PoseL.Dispose();
                hv_PoseR.Dispose();
                hv_IntersectionPlane1.Dispose();
                hv_IntersectionPlane2.Dispose();
                hv_IntersectionPlane3.Dispose();
                hv_IntersectionPlane4.Dispose();
                hv_IntersectionPlane5.Dispose();
                hv_LeftIndex.Dispose();
                hv_RightIndex.Dispose();
                hv_TopIndex.Dispose();
                hv_DownIndex.Dispose();
                hv_resultcon.Dispose();
                hv_n.Dispose();
                hv_PoseT1.Dispose();
                hv_PoseR1.Dispose();
                hv_PoseD1.Dispose();
                hv_PoseL1.Dispose();
                hv_ObjectModel3DIntersectiont.Dispose();
                hv_numbercount.Dispose();
                hv_lengthnum.Dispose();
                hv_Length.Dispose();
                hv_Max.Dispose();
                hv_Indices.Dispose();
                hv_Numberct.Dispose();
                hv_Row.Dispose();
                hv_Col.Dispose();
                hv_Colmin.Dispose();
                hv_Colmax.Dispose();
                hv_ObjectModel3DIntersectionl.Dispose();
                hv_Lengthl.Dispose();
                hv_Maxl.Dispose();
                hv_Numbercl.Dispose();
                hv_ObjectModel3DIntersectionr.Dispose();
                hv_Lengthr.Dispose();
                hv_Maxr.Dispose();
                hv_Numbercr.Dispose();
                hv_ObjectModel3DIntersectiond.Dispose();
                hv_Lengthd.Dispose();
                hv_Maxd.Dispose();
                hv_Numbercd.Dispose();
                hv_Left3DX.Dispose();
                hv_Right3DX.Dispose();
                hv_Down3DY.Dispose();
                hv_DistancesRT.Dispose();
                hv_DistancesLT.Dispose();
                hv_DistancesRD.Dispose();
                hv_DistancesLD.Dispose();
                hv_DistancesTD.Dispose();
                hv_DistancesLR.Dispose();
                hv_DistancesTopDiag.Dispose();
                hv_DistancesTopLeftDiag.Dispose();
                hv_DistancesTopRightDiag.Dispose();
                hv_DistancesDownDiag.Dispose();
                hv_DistancesDownLeftDiag.Dispose();
                hv_DistancesDownRightDiag.Dispose();
                hv_DistancesLeftDiag.Dispose();
                hv_DistancesLeftLeftDiag.Dispose();
                hv_DistancesLeftRightDiag.Dispose();
                hv_DistancesRightDiag.Dispose();
                hv_DistancesRightLeftDiag.Dispose();
                hv_DistancesRightRightDiag.Dispose();
                hv_Y.Dispose();
                hv_MaxY.Dispose();
                hv_MinY.Dispose();
                hv_LengthY.Dispose();
                hv_NumLengthY.Dispose();
                hv_lengthofY.Dispose();
                hv_wlength.Dispose();
                hv_resultcon1.Dispose();
                hv_resultcon2.Dispose();
                hv_resultcon3.Dispose();
                hv_resultcon4.Dispose();
                hv_Rowsleft.Dispose();
                hv_Colsleft.Dispose();
                hv_Rowsrigt.Dispose();
                hv_Colsrigt.Dispose();
                hv_Rowsmidt.Dispose();
                hv_Colsmidt.Dispose();
                hv_Minleft.Dispose();
                hv_Maxleft.Dispose();
                hv_Minrigt.Dispose();
                hv_Maxrigt.Dispose();
                hv_RowBeginleft.Dispose();
                hv_ColBeginleft.Dispose();
                hv_RowEndleft.Dispose();
                hv_ColEndleft.Dispose();
                hv_Nrleft.Dispose();
                hv_Ncleft.Dispose();
                hv_Distleft.Dispose();
                hv_RowBeginrigt.Dispose();
                hv_ColBeginrigt.Dispose();
                hv_RowEndrigt.Dispose();
                hv_ColEndrigt.Dispose();
                hv_Nrrigt.Dispose();
                hv_Ncrigt.Dispose();
                hv_Distrigt.Dispose();
                hv_lengthleftcontour.Dispose();
                hv_lengthleflined.Dispose();
                hv_Abslengthleftcontour.Dispose();
                hv_Abslengthleflined.Dispose();
                hv_countofCols.Dispose();
                hv_Index.Dispose();
                hv_Rowsnewleft.Dispose();
                hv_Colsnewleft.Dispose();
                hv_Indexx.Dispose();
                hv_Index1.Dispose();
                hv_Rowsnewmidt.Dispose();
                hv_Colsnewmidt.Dispose();
                hv_countofRowst.Dispose();
                hv_RowBeginmidt.Dispose();
                hv_ColBeginmidt.Dispose();
                hv_RowEndmidt.Dispose();
                hv_ColEndmidt.Dispose();
                hv_Nrmidt.Dispose();
                hv_Ncmidt.Dispose();
                hv_Distmidt.Dispose();
                hv_lengthrigtcontour.Dispose();
                hv_lengthriglined.Dispose();
                hv_Abslengthrigtcontour.Dispose();
                hv_Abslengthriglined.Dispose();
                hv_Rowsnewrigt.Dispose();
                hv_Colsnewrigt.Dispose();
                hv_Rowslefd.Dispose();
                hv_Colslefd.Dispose();
                hv_Rowsrigd.Dispose();
                hv_Colsrigd.Dispose();
                hv_Rowsmidd.Dispose();
                hv_Colsmidd.Dispose();
                hv_Minlefd.Dispose();
                hv_Maxlefd.Dispose();
                hv_Minrigd.Dispose();
                hv_Maxrigd.Dispose();
                hv_RowBeginlefd.Dispose();
                hv_ColBeginlefd.Dispose();
                hv_RowEndlefd.Dispose();
                hv_ColEndlefd.Dispose();
                hv_Nrlefd.Dispose();
                hv_Nclefd.Dispose();
                hv_Distlefd.Dispose();
                hv_RowBeginrigd.Dispose();
                hv_ColBeginrigd.Dispose();
                hv_RowEndrigd.Dispose();
                hv_ColEndrigd.Dispose();
                hv_Nrrigd.Dispose();
                hv_Ncrigd.Dispose();
                hv_Distrigd.Dispose();
                hv_lengthlefdcontour.Dispose();
                hv_Abslengthlefdcontour.Dispose();
                hv_Rowsnewlefd.Dispose();
                hv_Colsnewlefd.Dispose();
                hv_Rowsnewmidd.Dispose();
                hv_Colsnewmidd.Dispose();
                hv_countofRowsd.Dispose();
                hv_RowBeginmidd.Dispose();
                hv_ColBeginmidd.Dispose();
                hv_RowEndmidd.Dispose();
                hv_ColEndmidd.Dispose();
                hv_Nrmidd.Dispose();
                hv_Ncmidd.Dispose();
                hv_Distmidd.Dispose();
                hv_lengthrigdcontour.Dispose();
                hv_Abslengthrigdcontour.Dispose();
                hv_Rowsnewrigd.Dispose();
                hv_Colsnewrigd.Dispose();
                hv_Rowslefl.Dispose();
                hv_Colslefl.Dispose();
                hv_Rowsrigl.Dispose();
                hv_Colsrigl.Dispose();
                hv_Rowsmidl.Dispose();
                hv_Colsmidl.Dispose();
                hv_Minlefl.Dispose();
                hv_Maxlefl.Dispose();
                hv_Minrigl.Dispose();
                hv_Maxrigl.Dispose();
                hv_RowBeginlefl.Dispose();
                hv_ColBeginlefl.Dispose();
                hv_RowEndlefl.Dispose();
                hv_ColEndlefl.Dispose();
                hv_Nrlefl.Dispose();
                hv_Nclefl.Dispose();
                hv_Distlefl.Dispose();
                hv_RowBeginrigl.Dispose();
                hv_ColBeginrigl.Dispose();
                hv_RowEndrigl.Dispose();
                hv_ColEndrigl.Dispose();
                hv_Nrrigl.Dispose();
                hv_Ncrigl.Dispose();
                hv_Distrigl.Dispose();
                hv_lengthleflcontour.Dispose();
                hv_Abslengthleflcontour.Dispose();
                hv_Rowsnewlefl.Dispose();
                hv_Colsnewlefl.Dispose();
                hv_Rowsnewmidl.Dispose();
                hv_Colsnewmidl.Dispose();
                hv_countofRowsl.Dispose();
                hv_Rowsnewrigl.Dispose();
                hv_Colsnewrigl.Dispose();
                hv_RowBeginmidl.Dispose();
                hv_ColBeginmidl.Dispose();
                hv_RowEndmidl.Dispose();
                hv_ColEndmidl.Dispose();
                hv_Nrmidl.Dispose();
                hv_Ncmidl.Dispose();
                hv_Distmidl.Dispose();
                hv_Rowslefr.Dispose();
                hv_Colslefr.Dispose();
                hv_Rowsrigr.Dispose();
                hv_Colsrigr.Dispose();
                hv_Rowsmidr.Dispose();
                hv_Colsmidr.Dispose();
                hv_Minlefr.Dispose();
                hv_Maxlefr.Dispose();
                hv_Minrigr.Dispose();
                hv_Maxrigr.Dispose();
                hv_RowBeginlefr.Dispose();
                hv_ColBeginlefr.Dispose();
                hv_RowEndlefr.Dispose();
                hv_ColEndlefr.Dispose();
                hv_Nrlefr.Dispose();
                hv_Nclefr.Dispose();
                hv_Distlefr.Dispose();
                hv_RowBeginrigr.Dispose();
                hv_ColBeginrigr.Dispose();
                hv_RowEndrigr.Dispose();
                hv_ColEndrigr.Dispose();
                hv_Nrrigr.Dispose();
                hv_Ncrigr.Dispose();
                hv_Distrigr.Dispose();
                hv_lengthlefrcontour.Dispose();
                hv_Abslengthlefrcontour.Dispose();
                hv_Rowsnewlefr.Dispose();
                hv_Colsnewlefr.Dispose();
                hv_Rowsnewmidr.Dispose();
                hv_Colsnewmidr.Dispose();
                hv_countofRowsr.Dispose();
                hv_RowBeginmidr.Dispose();
                hv_ColBeginmidr.Dispose();
                hv_RowEndmidr.Dispose();
                hv_ColEndmidr.Dispose();
                hv_Nrmidr.Dispose();
                hv_Ncmidr.Dispose();
                hv_Distmidr.Dispose();
                hv_lengthrigrcontour.Dispose();
                hv_Abslengthrigrcontour.Dispose();
                hv_countofColr.Dispose();
                hv_Rowsnewrigr.Dispose();
                hv_Colsnewrigr.Dispose();
                hv_Minl.Dispose();
                hv_Minr.Dispose();
                hv_top_diag.Dispose();
                hv_down_diag.Dispose();
                hv_left_diag.Dispose();
                hv_right_diag.Dispose();
                hv_RowIntersectionlpt.Dispose();
                hv_ColIntersectionlpt.Dispose();
                hv_IsOverlapping.Dispose();
                hv_RowIntersectiontpt.Dispose();
                hv_ColIntersectiontpt.Dispose();
                hv_RowIntersectiondpt.Dispose();
                hv_ColIntersectiondpt.Dispose();
                hv_RowIntersectionrpt.Dispose();
                hv_ColIntersectionrpt.Dispose();
                hv_dis_topLeftDiag.Dispose();
                hv_dis_topRightDiag.Dispose();
                hv_dis_downLeftDiag.Dispose();
                hv_dis_downRightDiag.Dispose();
                hv_dis_leftLeftDiag.Dispose();
                hv_dis_leftRightDiag.Dispose();
                hv_dis_rightLeftDiag.Dispose();
                hv_dis_rightRightDiag.Dispose();
                hv_Angleradttl.Dispose();
                hv_Degtl.Dispose();
                hv_DegtlAbs.Dispose();
                hv_newttl.Dispose();
                hv_verllength.Dispose();
                hv_distop_verlength.Dispose();
                hv_Angleradtr.Dispose();
                hv_Degtr.Dispose();
                hv_DegtrAbs.Dispose();
                hv_Angleraddtl.Dispose();
                hv_Degdl.Dispose();
                hv_DegdlAbs.Dispose();
                hv_verldlength.Dispose();
                hv_disdown_verlength.Dispose();
                hv_Angleraddtr.Dispose();
                hv_DegdrAbs.Dispose();
                hv_Angleradd.Dispose();
                hv_Degtd.Dispose();
                hv_DegtdAbs.Dispose();
                hv_Angleraddltl.Dispose();
                hv_Degldl.Dispose();
                hv_DegldlAbs.Dispose();
                hv_newltl.Dispose();
                hv_verlllength.Dispose();
                hv_disleft_verlength.Dispose();
                hv_Angleraddrtl.Dispose();
                hv_Degrdl.Dispose();
                hv_newrtr.Dispose();
                hv_verrdlength.Dispose();
                hv_disright_verlength.Dispose();
                hv_RowsMidt.Dispose();
                hv_ColsMidt.Dispose();
                hv_RowsMidd.Dispose();
                hv_ColsMidd.Dispose();
                hv_RowsMidl.Dispose();
                hv_ColsMidl.Dispose();
                hv_RowsMidr.Dispose();
                hv_ColsMidr.Dispose();
                hv_meanRowst.Dispose();
                hv_meanColst.Dispose();
                hv_meanRowsd.Dispose();
                hv_meanColsd.Dispose();
                hv_meanRowsl.Dispose();
                hv_meanColsl.Dispose();
                hv_meanRowsr.Dispose();
                hv_meanColsr.Dispose();
                hv_minColsd.Dispose();
                hv_maxColsd.Dispose();
                hv_minColst.Dispose();
                hv_maxColst.Dispose();
                hv_minColsl.Dispose();
                hv_maxColsl.Dispose();
                hv_minColsr.Dispose();
                hv_maxColsr.Dispose();
                hv_HomMat2DIdentity.Dispose();
                hv_Areal.Dispose();
                hv_Rowl.Dispose();
                hv_Columnl.Dispose();
                hv_PointOrderl.Dispose();
                hv_HomMatTransPreL.Dispose();
                hv_RowPrel.Dispose();
                hv_ColumnPrel.Dispose();
                hv_HomMat2DLRotate.Dispose();
                hv_HomMat2DScale.Dispose();
                hv_meanMidColnew.Dispose();
                hv_RowBeginmidtnew.Dispose();
                hv_ColBeginmidtnew.Dispose();
                hv_RowEndmidtnew.Dispose();
                hv_ColEndmidtnew.Dispose();
                hv_Rowminmidt.Dispose();
                hv_SubRow.Dispose();
                hv_RowsTopl.Dispose();
                hv_ColsTopl.Dispose();
                hv_RowsDowl.Dispose();
                hv_ColsDowl.Dispose();
                hv_meanTopl.Dispose();
                hv_meanDowl.Dispose();
                hv_RowBegintopl.Dispose();
                hv_ColBegintopl.Dispose();
                hv_RowEndtopl.Dispose();
                hv_ColEndtopl.Dispose();
                hv_angleltx.Dispose();
                hv_Degltx.Dispose();
                hv_AbgDegltx.Dispose();
                hv_Abgangleltx.Dispose();
                hv_DegSubL.Dispose();
                hv_MeanMidlx.Dispose();
                hv_Sintl.Dispose();
                hv_Costl.Dispose();
                hv_AbsSintl.Dispose();
                hv_AbsCostl.Dispose();
                hv_AbsmeanRowsl.Dispose();
                hv_DistanceXPre.Dispose();
                hv_DistanceLPre.Dispose();
                hv_RowIntersectiontl.Dispose();
                hv_ColIntersectiontl.Dispose();
                hv_RowIntersectiont.Dispose();
                hv_ColIntersectiont.Dispose();
                hv_DistanceLT.Dispose();
                hv_RowsMidnewt.Dispose();
                hv_ColsMidnewt.Dispose();
                hv_MeanMidnewColt.Dispose();
                hv_MeanMidnewRowy.Dispose();
                hv_RowIntersectiontd.Dispose();
                hv_ColIntersectiontd.Dispose();
                hv_DistanceDL.Dispose();
                hv_Arear.Dispose();
                hv_Rowr.Dispose();
                hv_Columnr.Dispose();
                hv_PointOrderr.Dispose();
                hv_HomMatTransPreR.Dispose();
                hv_RowPrer.Dispose();
                hv_ColumnPrer.Dispose();
                hv_HomMat2DRRotate.Dispose();
                hv_RowBeginmidnewr.Dispose();
                hv_ColBeginmidnewr.Dispose();
                hv_RowEndmidnewr.Dispose();
                hv_ColEndmidnewr.Dispose();
                hv_Rowmidrmin.Dispose();
                hv_Rowsrmidnew.Dispose();
                hv_Colsrmidnew.Dispose();
                hv_meanmidColsnew.Dispose();
                hv_RowsTopr.Dispose();
                hv_ColsTopr.Dispose();
                hv_RowsDowr.Dispose();
                hv_ColsDowr.Dispose();
                hv_meanTopr.Dispose();
                hv_meanDowr.Dispose();
                hv_RowBegintopr.Dispose();
                hv_ColBegintopr.Dispose();
                hv_RowEndtopr.Dispose();
                hv_ColEndtopr.Dispose();
                hv_Degrtr.Dispose();
                hv_DegrtrAbs.Dispose();
                hv_DegSubR.Dispose();
                hv_MeanMidrx.Dispose();
                hv_AngleradtrAbs.Dispose();
                hv_Sintr.Dispose();
                hv_Costr.Dispose();
                hv_AbsSintr.Dispose();
                hv_AbsCostr.Dispose();
                hv_AbsmeanRowsr.Dispose();
                hv_DistanceRPre.Dispose();
                hv_RowIntersectiontr.Dispose();
                hv_ColIntersectiontr.Dispose();
                hv_DistanceRT.Dispose();
                hv_DistanceRD.Dispose();
                hv_Aread.Dispose();
                hv_Rowd.Dispose();
                hv_Columnd.Dispose();
                hv_HomMatTransPreD.Dispose();
                hv_RowPred.Dispose();
                hv_ColumnPred.Dispose();
                hv_PointOrderd.Dispose();
                hv_Rowmiddnew.Dispose();
                hv_Colmiddnew.Dispose();
                hv_meannewmid.Dispose();
                hv_RowBegintopd.Dispose();
                hv_ColBegintopd.Dispose();
                hv_RowEndtopd.Dispose();
                hv_ColEndtopd.Dispose();
                hv_Degltxabs.Dispose();
                hv_DegSubD.Dispose();
                hv_RowBeginmiddnew.Dispose();
                hv_ColBeginmiddnew.Dispose();
                hv_RowEndmiddnew.Dispose();
                hv_ColEndmiddnew.Dispose();
                hv_RowMidDown.Dispose();
                hv_ColMidDown.Dispose();
                hv_meanDownRNew.Dispose();
                hv_AbsDown3DDistance.Dispose();
                hv_AbsmeanRowst.Dispose();
                hv_Distancediagonal.Dispose();
                hv_meanDownCNew.Dispose();
                hv_distancediag_td.Dispose();
                hv_meanRC.Dispose();
                hv_meanRR.Dispose();
                hv_distancediag_lr.Dispose();
                hv_Exception.Dispose();
                hv_meanTopDiag.Dispose();
                hv_meanDownDiag.Dispose();
                hv_meanLeftDiag.Dispose();
                hv_meanRightDiag.Dispose();
                hv_meanTopLeftDiag.Dispose();
                hv_meanTopRightDiag.Dispose();
                hv_meanLeftLeftDiag.Dispose();
                hv_meanLeftRightDiag.Dispose();
                hv_meanRightLeftDiag.Dispose();
                hv_meanRightRightDiag.Dispose();
                hv_meanDownLeftDiag.Dispose();
                hv_meanDownRightDiag.Dispose();
                hv_meanLT.Dispose();
                hv_meanRT.Dispose();
                hv_meanRD.Dispose();
                hv_meanLD.Dispose();
                hv_meanTD.Dispose();
                hv_meanLR.Dispose();
                hv_SubLength.Dispose();
                hv_Functiontmp.Dispose();
                hv_Functiontmpnew.Dispose();
                hv_xtmp.Dispose();
                hv_SmoothedFunction.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_ImageOutLeft.Dispose();
                ho_ImageOutRight.Dispose();
                ho_ImageOutTop.Dispose();
                ho_ImageOutDown.Dispose();
                ho_ImagePart.Dispose();
                ho_ImagePartReal.Dispose();
                ho_ImagePartLeft.Dispose();
                ho_ImagePartLeftNew.Dispose();
                ho_RegionLeft.Dispose();
                ho_ImageReducedLeft.Dispose();
                ho_RegionErosion.Dispose();
                ho_ImagePartRight.Dispose();
                ho_ImagePartRightNew.Dispose();
                ho_RegionRight.Dispose();
                ho_ImageReducedRight.Dispose();
                ho_ImagePartTop.Dispose();
                ho_ImagePartTopNew.Dispose();
                ho_RegionTop.Dispose();
                ho_ImageReducedTop.Dispose();
                ho_ImagePartDown.Dispose();
                ho_ImagePartDownNew.Dispose();
                ho_RegionDown.Dispose();
                ho_ImageReducedDown.Dispose();
                ho_Bx.Dispose();
                ho_By.Dispose();
                ho_Bz.Dispose();
                ho_Intersectiont.Dispose();
                ho_UnionContourst.Dispose();
                ho_ObjectSelectedt.Dispose();
                ho_ContoursSplitt.Dispose();
                ho_SelectedContourst.Dispose();
                ho_contournewmid.Dispose();
                ho_Intersectionl.Dispose();
                ho_UnionContoursl.Dispose();
                ho_ObjectSelectedl.Dispose();
                ho_ContoursSplitl.Dispose();
                ho_SelectedContoursl.Dispose();
                ho_Intersectionr.Dispose();
                ho_UnionContoursr.Dispose();
                ho_ObjectSelectedr.Dispose();
                ho_ContoursSplitr.Dispose();
                ho_SelectedContoursr.Dispose();
                ho_Intersectiond.Dispose();
                ho_UnionContoursd.Dispose();
                ho_ObjectSelectedd.Dispose();
                ho_ContoursSplitd.Dispose();
                ho_SelectedContoursd.Dispose();
                ho_contourleft.Dispose();
                ho_contourmidt.Dispose();
                ho_contourrigt.Dispose();
                ho_contourlefr.Dispose();
                ho_contourmidr.Dispose();
                ho_contourrigr.Dispose();
                ho_contourlefl.Dispose();
                ho_contourmidl.Dispose();
                ho_contourrigl.Dispose();
                ho_contourlefd.Dispose();
                ho_contourmidd.Dispose();
                ho_contourrigd.Dispose();
                ho_contourtmp.Dispose();
                ho_SelectedContourslPre.Dispose();
                ho_SelectedContourslfinal.Dispose();
                ho_SelectedContourslnewpre.Dispose();
                ho_contourtoplnew.Dispose();
                ho_contourmidlnew.Dispose();
                ho_contourdowlnew.Dispose();
                ho_SelectedContourslprenew.Dispose();
                ho_SelectedContourslnew.Dispose();
                ho_SelectedContoursrPre.Dispose();
                ho_SelectedContoursrfinal.Dispose();
                ho_SelectedContoursrnewpre.Dispose();
                ho_contourtoprnew.Dispose();
                ho_contourmidrnew.Dispose();
                ho_contourdowrnew.Dispose();
                ho_SelectedContoursrprenew.Dispose();
                ho_SelectedContoursrnew.Dispose();
                ho_SelectedContoursdPre.Dispose();
                ho_contourmiddnew.Dispose();
                ho_SelectedContoursdnewpre.Dispose();
                ho_contourtopdnew.Dispose();
                ho_contourdowdnew.Dispose();
                ho_SelectedContoursdnewnpre.Dispose();
                ho_SelectedContoursdprenew.Dispose();
                ho_SelectedContoursdnew.Dispose();
                ho_SelectedContoursrnewnpre.Dispose();

                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_Instructions.Dispose();
                hv_WindowHandle.Dispose();
                hv_Message.Dispose();
                hv_r.Dispose();
                hv_ParameterValues.Dispose();
                hv_Status.Dispose();
                hv_yInterval.Dispose();
                hv_ObjectModel3DBLeft.Dispose();
                hv_ObjectModel3DBLeftConnected.Dispose();
                hv_ObjectModel3DBLeftNew.Dispose();
                hv_ObjectModel3DBRight.Dispose();
                hv_ObjectModel3DBRightConnected.Dispose();
                hv_ObjectModel3DBRightNew.Dispose();
                hv_ObjectModel3DBTop.Dispose();
                hv_ObjectModel3DTopConnected.Dispose();
                hv_ObjectModel3DBTopNew.Dispose();
                hv_ObjectModel3DBDown.Dispose();
                hv_ObjectModel3DBDownConnected.Dispose();
                hv_ObjectModel3DBDownNew.Dispose();
                hv_PoseOut.Dispose();
                hv_SampledObjectModel3DTop.Dispose();
                hv_SampledObjectModel3DDown.Dispose();
                hv_SampledObjectModel3DLeft.Dispose();
                hv_SampledObjectModel3DRight.Dispose();
                hv_Surface3DDefaultT.Dispose();
                hv_Info.Dispose();
                hv_Surface3DDefaultD.Dispose();
                hv_Surface3DDefaultL.Dispose();
                hv_Surface3DDefaultR.Dispose();
                hv_CenterPointT.Dispose();
                hv_Radius.Dispose();
                hv_CenterPointD.Dispose();
                hv_CenterPointL.Dispose();
                hv_CenterPointR.Dispose();
                hv_raduis.Dispose();
                hv_ObjectModel3DIntersections.Dispose();
                hv_DisparityProfileWidth.Dispose();
                hv_DisparityProfileHeight.Dispose();
                hv_WindowEnlargement.Dispose();
                hv_WindowWidth.Dispose();
                hv_WindowHeight.Dispose();
                hv_VisualizationPlaneSize.Dispose();
                hv_PoseT.Dispose();
                hv_PoseD.Dispose();
                hv_PoseL.Dispose();
                hv_PoseR.Dispose();
                hv_IntersectionPlane1.Dispose();
                hv_IntersectionPlane2.Dispose();
                hv_IntersectionPlane3.Dispose();
                hv_IntersectionPlane4.Dispose();
                hv_IntersectionPlane5.Dispose();
                hv_LeftIndex.Dispose();
                hv_RightIndex.Dispose();
                hv_TopIndex.Dispose();
                hv_DownIndex.Dispose();
                hv_resultcon.Dispose();
                hv_n.Dispose();
                hv_PoseT1.Dispose();
                hv_PoseR1.Dispose();
                hv_PoseD1.Dispose();
                hv_PoseL1.Dispose();
                hv_ObjectModel3DIntersectiont.Dispose();
                hv_numbercount.Dispose();
                hv_lengthnum.Dispose();
                hv_Length.Dispose();
                hv_Max.Dispose();
                hv_Indices.Dispose();
                hv_Numberct.Dispose();
                hv_Row.Dispose();
                hv_Col.Dispose();
                hv_Colmin.Dispose();
                hv_Colmax.Dispose();
                hv_ObjectModel3DIntersectionl.Dispose();
                hv_Lengthl.Dispose();
                hv_Maxl.Dispose();
                hv_Numbercl.Dispose();
                hv_ObjectModel3DIntersectionr.Dispose();
                hv_Lengthr.Dispose();
                hv_Maxr.Dispose();
                hv_Numbercr.Dispose();
                hv_ObjectModel3DIntersectiond.Dispose();
                hv_Lengthd.Dispose();
                hv_Maxd.Dispose();
                hv_Numbercd.Dispose();
                hv_Left3DX.Dispose();
                hv_Right3DX.Dispose();
                hv_Down3DY.Dispose();
                hv_DistancesRT.Dispose();
                hv_DistancesLT.Dispose();
                hv_DistancesRD.Dispose();
                hv_DistancesLD.Dispose();
                hv_DistancesTD.Dispose();
                hv_DistancesLR.Dispose();
                hv_DistancesTopDiag.Dispose();
                hv_DistancesTopLeftDiag.Dispose();
                hv_DistancesTopRightDiag.Dispose();
                hv_DistancesDownDiag.Dispose();
                hv_DistancesDownLeftDiag.Dispose();
                hv_DistancesDownRightDiag.Dispose();
                hv_DistancesLeftDiag.Dispose();
                hv_DistancesLeftLeftDiag.Dispose();
                hv_DistancesLeftRightDiag.Dispose();
                hv_DistancesRightDiag.Dispose();
                hv_DistancesRightLeftDiag.Dispose();
                hv_DistancesRightRightDiag.Dispose();
                hv_Y.Dispose();
                hv_MaxY.Dispose();
                hv_MinY.Dispose();
                hv_LengthY.Dispose();
                hv_NumLengthY.Dispose();
                hv_lengthofY.Dispose();
                hv_wlength.Dispose();
                hv_resultcon1.Dispose();
                hv_resultcon2.Dispose();
                hv_resultcon3.Dispose();
                hv_resultcon4.Dispose();
                hv_Rowsleft.Dispose();
                hv_Colsleft.Dispose();
                hv_Rowsrigt.Dispose();
                hv_Colsrigt.Dispose();
                hv_Rowsmidt.Dispose();
                hv_Colsmidt.Dispose();
                hv_Minleft.Dispose();
                hv_Maxleft.Dispose();
                hv_Minrigt.Dispose();
                hv_Maxrigt.Dispose();
                hv_RowBeginleft.Dispose();
                hv_ColBeginleft.Dispose();
                hv_RowEndleft.Dispose();
                hv_ColEndleft.Dispose();
                hv_Nrleft.Dispose();
                hv_Ncleft.Dispose();
                hv_Distleft.Dispose();
                hv_RowBeginrigt.Dispose();
                hv_ColBeginrigt.Dispose();
                hv_RowEndrigt.Dispose();
                hv_ColEndrigt.Dispose();
                hv_Nrrigt.Dispose();
                hv_Ncrigt.Dispose();
                hv_Distrigt.Dispose();
                hv_lengthleftcontour.Dispose();
                hv_lengthleflined.Dispose();
                hv_Abslengthleftcontour.Dispose();
                hv_Abslengthleflined.Dispose();
                hv_countofCols.Dispose();
                hv_Index.Dispose();
                hv_Rowsnewleft.Dispose();
                hv_Colsnewleft.Dispose();
                hv_Indexx.Dispose();
                hv_Index1.Dispose();
                hv_Rowsnewmidt.Dispose();
                hv_Colsnewmidt.Dispose();
                hv_countofRowst.Dispose();
                hv_RowBeginmidt.Dispose();
                hv_ColBeginmidt.Dispose();
                hv_RowEndmidt.Dispose();
                hv_ColEndmidt.Dispose();
                hv_Nrmidt.Dispose();
                hv_Ncmidt.Dispose();
                hv_Distmidt.Dispose();
                hv_lengthrigtcontour.Dispose();
                hv_lengthriglined.Dispose();
                hv_Abslengthrigtcontour.Dispose();
                hv_Abslengthriglined.Dispose();
                hv_Rowsnewrigt.Dispose();
                hv_Colsnewrigt.Dispose();
                hv_Rowslefd.Dispose();
                hv_Colslefd.Dispose();
                hv_Rowsrigd.Dispose();
                hv_Colsrigd.Dispose();
                hv_Rowsmidd.Dispose();
                hv_Colsmidd.Dispose();
                hv_Minlefd.Dispose();
                hv_Maxlefd.Dispose();
                hv_Minrigd.Dispose();
                hv_Maxrigd.Dispose();
                hv_RowBeginlefd.Dispose();
                hv_ColBeginlefd.Dispose();
                hv_RowEndlefd.Dispose();
                hv_ColEndlefd.Dispose();
                hv_Nrlefd.Dispose();
                hv_Nclefd.Dispose();
                hv_Distlefd.Dispose();
                hv_RowBeginrigd.Dispose();
                hv_ColBeginrigd.Dispose();
                hv_RowEndrigd.Dispose();
                hv_ColEndrigd.Dispose();
                hv_Nrrigd.Dispose();
                hv_Ncrigd.Dispose();
                hv_Distrigd.Dispose();
                hv_lengthlefdcontour.Dispose();
                hv_Abslengthlefdcontour.Dispose();
                hv_Rowsnewlefd.Dispose();
                hv_Colsnewlefd.Dispose();
                hv_Rowsnewmidd.Dispose();
                hv_Colsnewmidd.Dispose();
                hv_countofRowsd.Dispose();
                hv_RowBeginmidd.Dispose();
                hv_ColBeginmidd.Dispose();
                hv_RowEndmidd.Dispose();
                hv_ColEndmidd.Dispose();
                hv_Nrmidd.Dispose();
                hv_Ncmidd.Dispose();
                hv_Distmidd.Dispose();
                hv_lengthrigdcontour.Dispose();
                hv_Abslengthrigdcontour.Dispose();
                hv_Rowsnewrigd.Dispose();
                hv_Colsnewrigd.Dispose();
                hv_Rowslefl.Dispose();
                hv_Colslefl.Dispose();
                hv_Rowsrigl.Dispose();
                hv_Colsrigl.Dispose();
                hv_Rowsmidl.Dispose();
                hv_Colsmidl.Dispose();
                hv_Minlefl.Dispose();
                hv_Maxlefl.Dispose();
                hv_Minrigl.Dispose();
                hv_Maxrigl.Dispose();
                hv_RowBeginlefl.Dispose();
                hv_ColBeginlefl.Dispose();
                hv_RowEndlefl.Dispose();
                hv_ColEndlefl.Dispose();
                hv_Nrlefl.Dispose();
                hv_Nclefl.Dispose();
                hv_Distlefl.Dispose();
                hv_RowBeginrigl.Dispose();
                hv_ColBeginrigl.Dispose();
                hv_RowEndrigl.Dispose();
                hv_ColEndrigl.Dispose();
                hv_Nrrigl.Dispose();
                hv_Ncrigl.Dispose();
                hv_Distrigl.Dispose();
                hv_lengthleflcontour.Dispose();
                hv_Abslengthleflcontour.Dispose();
                hv_Rowsnewlefl.Dispose();
                hv_Colsnewlefl.Dispose();
                hv_Rowsnewmidl.Dispose();
                hv_Colsnewmidl.Dispose();
                hv_countofRowsl.Dispose();
                hv_Rowsnewrigl.Dispose();
                hv_Colsnewrigl.Dispose();
                hv_RowBeginmidl.Dispose();
                hv_ColBeginmidl.Dispose();
                hv_RowEndmidl.Dispose();
                hv_ColEndmidl.Dispose();
                hv_Nrmidl.Dispose();
                hv_Ncmidl.Dispose();
                hv_Distmidl.Dispose();
                hv_Rowslefr.Dispose();
                hv_Colslefr.Dispose();
                hv_Rowsrigr.Dispose();
                hv_Colsrigr.Dispose();
                hv_Rowsmidr.Dispose();
                hv_Colsmidr.Dispose();
                hv_Minlefr.Dispose();
                hv_Maxlefr.Dispose();
                hv_Minrigr.Dispose();
                hv_Maxrigr.Dispose();
                hv_RowBeginlefr.Dispose();
                hv_ColBeginlefr.Dispose();
                hv_RowEndlefr.Dispose();
                hv_ColEndlefr.Dispose();
                hv_Nrlefr.Dispose();
                hv_Nclefr.Dispose();
                hv_Distlefr.Dispose();
                hv_RowBeginrigr.Dispose();
                hv_ColBeginrigr.Dispose();
                hv_RowEndrigr.Dispose();
                hv_ColEndrigr.Dispose();
                hv_Nrrigr.Dispose();
                hv_Ncrigr.Dispose();
                hv_Distrigr.Dispose();
                hv_lengthlefrcontour.Dispose();
                hv_Abslengthlefrcontour.Dispose();
                hv_Rowsnewlefr.Dispose();
                hv_Colsnewlefr.Dispose();
                hv_Rowsnewmidr.Dispose();
                hv_Colsnewmidr.Dispose();
                hv_countofRowsr.Dispose();
                hv_RowBeginmidr.Dispose();
                hv_ColBeginmidr.Dispose();
                hv_RowEndmidr.Dispose();
                hv_ColEndmidr.Dispose();
                hv_Nrmidr.Dispose();
                hv_Ncmidr.Dispose();
                hv_Distmidr.Dispose();
                hv_lengthrigrcontour.Dispose();
                hv_Abslengthrigrcontour.Dispose();
                hv_countofColr.Dispose();
                hv_Rowsnewrigr.Dispose();
                hv_Colsnewrigr.Dispose();
                hv_Minl.Dispose();
                hv_Minr.Dispose();
                hv_top_diag.Dispose();
                hv_down_diag.Dispose();
                hv_left_diag.Dispose();
                hv_right_diag.Dispose();
                hv_RowIntersectionlpt.Dispose();
                hv_ColIntersectionlpt.Dispose();
                hv_IsOverlapping.Dispose();
                hv_RowIntersectiontpt.Dispose();
                hv_ColIntersectiontpt.Dispose();
                hv_RowIntersectiondpt.Dispose();
                hv_ColIntersectiondpt.Dispose();
                hv_RowIntersectionrpt.Dispose();
                hv_ColIntersectionrpt.Dispose();
                hv_dis_topLeftDiag.Dispose();
                hv_dis_topRightDiag.Dispose();
                hv_dis_downLeftDiag.Dispose();
                hv_dis_downRightDiag.Dispose();
                hv_dis_leftLeftDiag.Dispose();
                hv_dis_leftRightDiag.Dispose();
                hv_dis_rightLeftDiag.Dispose();
                hv_dis_rightRightDiag.Dispose();
                hv_Angleradttl.Dispose();
                hv_Degtl.Dispose();
                hv_DegtlAbs.Dispose();
                hv_newttl.Dispose();
                hv_verllength.Dispose();
                hv_distop_verlength.Dispose();
                hv_Angleradtr.Dispose();
                hv_Degtr.Dispose();
                hv_DegtrAbs.Dispose();
                hv_Angleraddtl.Dispose();
                hv_Degdl.Dispose();
                hv_DegdlAbs.Dispose();
                hv_verldlength.Dispose();
                hv_disdown_verlength.Dispose();
                hv_Angleraddtr.Dispose();
                hv_DegdrAbs.Dispose();
                hv_Angleradd.Dispose();
                hv_Degtd.Dispose();
                hv_DegtdAbs.Dispose();
                hv_Angleraddltl.Dispose();
                hv_Degldl.Dispose();
                hv_DegldlAbs.Dispose();
                hv_newltl.Dispose();
                hv_verlllength.Dispose();
                hv_disleft_verlength.Dispose();
                hv_Angleraddrtl.Dispose();
                hv_Degrdl.Dispose();
                hv_newrtr.Dispose();
                hv_verrdlength.Dispose();
                hv_disright_verlength.Dispose();
                hv_RowsMidt.Dispose();
                hv_ColsMidt.Dispose();
                hv_RowsMidd.Dispose();
                hv_ColsMidd.Dispose();
                hv_RowsMidl.Dispose();
                hv_ColsMidl.Dispose();
                hv_RowsMidr.Dispose();
                hv_ColsMidr.Dispose();
                hv_meanRowst.Dispose();
                hv_meanColst.Dispose();
                hv_meanRowsd.Dispose();
                hv_meanColsd.Dispose();
                hv_meanRowsl.Dispose();
                hv_meanColsl.Dispose();
                hv_meanRowsr.Dispose();
                hv_meanColsr.Dispose();
                hv_minColsd.Dispose();
                hv_maxColsd.Dispose();
                hv_minColst.Dispose();
                hv_maxColst.Dispose();
                hv_minColsl.Dispose();
                hv_maxColsl.Dispose();
                hv_minColsr.Dispose();
                hv_maxColsr.Dispose();
                hv_HomMat2DIdentity.Dispose();
                hv_Areal.Dispose();
                hv_Rowl.Dispose();
                hv_Columnl.Dispose();
                hv_PointOrderl.Dispose();
                hv_HomMatTransPreL.Dispose();
                hv_RowPrel.Dispose();
                hv_ColumnPrel.Dispose();
                hv_HomMat2DLRotate.Dispose();
                hv_HomMat2DScale.Dispose();
                hv_meanMidColnew.Dispose();
                hv_RowBeginmidtnew.Dispose();
                hv_ColBeginmidtnew.Dispose();
                hv_RowEndmidtnew.Dispose();
                hv_ColEndmidtnew.Dispose();
                hv_Rowminmidt.Dispose();
                hv_SubRow.Dispose();
                hv_RowsTopl.Dispose();
                hv_ColsTopl.Dispose();
                hv_RowsDowl.Dispose();
                hv_ColsDowl.Dispose();
                hv_meanTopl.Dispose();
                hv_meanDowl.Dispose();
                hv_RowBegintopl.Dispose();
                hv_ColBegintopl.Dispose();
                hv_RowEndtopl.Dispose();
                hv_ColEndtopl.Dispose();
                hv_angleltx.Dispose();
                hv_Degltx.Dispose();
                hv_AbgDegltx.Dispose();
                hv_Abgangleltx.Dispose();
                hv_DegSubL.Dispose();
                hv_MeanMidlx.Dispose();
                hv_Sintl.Dispose();
                hv_Costl.Dispose();
                hv_AbsSintl.Dispose();
                hv_AbsCostl.Dispose();
                hv_AbsmeanRowsl.Dispose();
                hv_DistanceXPre.Dispose();
                hv_DistanceLPre.Dispose();
                hv_RowIntersectiontl.Dispose();
                hv_ColIntersectiontl.Dispose();
                hv_RowIntersectiont.Dispose();
                hv_ColIntersectiont.Dispose();
                hv_DistanceLT.Dispose();
                hv_RowsMidnewt.Dispose();
                hv_ColsMidnewt.Dispose();
                hv_MeanMidnewColt.Dispose();
                hv_MeanMidnewRowy.Dispose();
                hv_RowIntersectiontd.Dispose();
                hv_ColIntersectiontd.Dispose();
                hv_DistanceDL.Dispose();
                hv_Arear.Dispose();
                hv_Rowr.Dispose();
                hv_Columnr.Dispose();
                hv_PointOrderr.Dispose();
                hv_HomMatTransPreR.Dispose();
                hv_RowPrer.Dispose();
                hv_ColumnPrer.Dispose();
                hv_HomMat2DRRotate.Dispose();
                hv_RowBeginmidnewr.Dispose();
                hv_ColBeginmidnewr.Dispose();
                hv_RowEndmidnewr.Dispose();
                hv_ColEndmidnewr.Dispose();
                hv_Rowmidrmin.Dispose();
                hv_Rowsrmidnew.Dispose();
                hv_Colsrmidnew.Dispose();
                hv_meanmidColsnew.Dispose();
                hv_RowsTopr.Dispose();
                hv_ColsTopr.Dispose();
                hv_RowsDowr.Dispose();
                hv_ColsDowr.Dispose();
                hv_meanTopr.Dispose();
                hv_meanDowr.Dispose();
                hv_RowBegintopr.Dispose();
                hv_ColBegintopr.Dispose();
                hv_RowEndtopr.Dispose();
                hv_ColEndtopr.Dispose();
                hv_Degrtr.Dispose();
                hv_DegrtrAbs.Dispose();
                hv_DegSubR.Dispose();
                hv_MeanMidrx.Dispose();
                hv_AngleradtrAbs.Dispose();
                hv_Sintr.Dispose();
                hv_Costr.Dispose();
                hv_AbsSintr.Dispose();
                hv_AbsCostr.Dispose();
                hv_AbsmeanRowsr.Dispose();
                hv_DistanceRPre.Dispose();
                hv_RowIntersectiontr.Dispose();
                hv_ColIntersectiontr.Dispose();
                hv_DistanceRT.Dispose();
                hv_DistanceRD.Dispose();
                hv_Aread.Dispose();
                hv_Rowd.Dispose();
                hv_Columnd.Dispose();
                hv_HomMatTransPreD.Dispose();
                hv_RowPred.Dispose();
                hv_ColumnPred.Dispose();
                hv_PointOrderd.Dispose();
                hv_Rowmiddnew.Dispose();
                hv_Colmiddnew.Dispose();
                hv_meannewmid.Dispose();
                hv_RowBegintopd.Dispose();
                hv_ColBegintopd.Dispose();
                hv_RowEndtopd.Dispose();
                hv_ColEndtopd.Dispose();
                hv_Degltxabs.Dispose();
                hv_DegSubD.Dispose();
                hv_RowBeginmiddnew.Dispose();
                hv_ColBeginmiddnew.Dispose();
                hv_RowEndmiddnew.Dispose();
                hv_ColEndmiddnew.Dispose();
                hv_RowMidDown.Dispose();
                hv_ColMidDown.Dispose();
                hv_meanDownRNew.Dispose();
                hv_AbsDown3DDistance.Dispose();
                hv_AbsmeanRowst.Dispose();
                hv_Distancediagonal.Dispose();
                hv_meanDownCNew.Dispose();
                hv_distancediag_td.Dispose();
                hv_meanRC.Dispose();
                hv_meanRR.Dispose();
                hv_distancediag_lr.Dispose();
                hv_Exception.Dispose();
                hv_meanTopDiag.Dispose();
                hv_meanDownDiag.Dispose();
                hv_meanLeftDiag.Dispose();
                hv_meanRightDiag.Dispose();
                hv_meanTopLeftDiag.Dispose();
                hv_meanTopRightDiag.Dispose();
                hv_meanLeftLeftDiag.Dispose();
                hv_meanLeftRightDiag.Dispose();
                hv_meanRightLeftDiag.Dispose();
                hv_meanRightRightDiag.Dispose();
                hv_meanDownLeftDiag.Dispose();
                hv_meanDownRightDiag.Dispose();
                hv_meanLT.Dispose();
                hv_meanRT.Dispose();
                hv_meanRD.Dispose();
                hv_meanLD.Dispose();
                hv_meanTD.Dispose();
                hv_meanLR.Dispose();
                hv_SubLength.Dispose();
                hv_Functiontmp.Dispose();
                hv_Functiontmpnew.Dispose();
                hv_xtmp.Dispose();
                hv_SmoothedFunction.Dispose();

            }
        }

        public void CheckReferenceObject(HObject ho_ImageTop, HObject ho_ImageLeft, HObject ho_ImageRight,
      HObject ho_ImageDown, out HTuple hv_LeftTDistance, out HTuple hv_LeftDDistance,
      out HTuple hv_RightTDistance, out HTuple hv_RightDDistance, out HTuple hv_DownDistance,
      out HTuple hv_LeftRightDistance)
        {



            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_ImageOutLeft = null, ho_ImageOutRight = null;
            HObject ho_ImageOutTop = null, ho_ImageOutDown = null, ho_ImagePart;
            HObject ho_ImagePartReal, ho_ImagePartLeft, ho_ImagePartLeftNew;
            HObject ho_RegionLeft, ho_ImageReducedLeft, ho_RegionErosion;
            HObject ho_ImagePartRight, ho_ImagePartRightNew, ho_RegionRight;
            HObject ho_ImageReducedRight, ho_ImagePartTop, ho_ImagePartTopNew;
            HObject ho_RegionTop, ho_ImageReducedTop, ho_ImagePartDown;
            HObject ho_ImagePartDownNew, ho_RegionDown, ho_ImageReducedDown;
            HObject ho_Bx, ho_By, ho_Bz, ho_Intersectiont = null, ho_UnionContourst = null;
            HObject ho_ObjectSelectedt = null, ho_ContoursSplitt = null;
            HObject ho_SelectedContourst = null, ho_contournewmid = null;
            HObject ho_Intersectionl = null, ho_UnionContoursl = null, ho_ObjectSelectedl = null;
            HObject ho_ContoursSplitl = null, ho_SelectedContoursl = null;
            HObject ho_Intersectionr = null, ho_UnionContoursr = null, ho_ObjectSelectedr = null;
            HObject ho_ContoursSplitr = null, ho_SelectedContoursr = null;
            HObject ho_Intersectiond = null, ho_UnionContoursd = null, ho_ObjectSelectedd = null;
            HObject ho_ContoursSplitd = null, ho_SelectedContoursd = null;
            HObject ho_contourleft = null, ho_contourmidt = null, ho_contourrigt = null;
            HObject ho_contourlefr = null, ho_contourmidr = null, ho_contourrigr = null;
            HObject ho_contourlefl = null, ho_contourmidl = null, ho_contourrigl = null;
            HObject ho_contourlefd = null, ho_contourmidd = null, ho_contourrigd = null;
            HObject ho_contourtmp = null, ho_SelectedContourslPre = null;
            HObject ho_contourtoplnew = null, ho_contourmidlnew = null;
            HObject ho_contourdowlnew = null, ho_SelectedContourslfinal = null;
            HObject ho_SelectedContourslnewpre = null, ho_SelectedContourslprenew = null;
            HObject ho_SelectedContourslnew = null, ho_SelectedContoursrPre = null;
            HObject ho_SelectedContoursrfinal = null, ho_SelectedContoursrnewpre = null;
            HObject ho_contourtoprnew = null, ho_contourmidrnew = null;
            HObject ho_contourdowrnew = null, ho_SelectedContoursrprenew = null;
            HObject ho_SelectedContoursrnew = null, ho_SelectedContoursdPre = null;
            HObject ho_contourmiddnew = null, ho_SelectedContoursdnewpre = null;
            HObject ho_contourtopdnew = null, ho_contourdowdnew = null;
            HObject ho_SelectedContoursdnewnpre = null, ho_SelectedContoursdprenew = null;
            HObject ho_SelectedContoursdnew = null, ho_SelectedContoursrnewnpre = null;

            // Local control variables 

            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_Instructions = new HTuple(), hv_WindowHandle = new HTuple();
            HTuple hv_Message = new HTuple(), hv_r = new HTuple();
            HTuple hv_ParameterValues = new HTuple(), hv_Status = new HTuple();
            HTuple hv_yInterval = new HTuple(), hv_ObjectModel3DBLeft = new HTuple();
            HTuple hv_ObjectModel3DBLeftConnected = new HTuple(), hv_ObjectModel3DBLeftNew = new HTuple();
            HTuple hv_ObjectModel3DBRight = new HTuple(), hv_ObjectModel3DBRightConnected = new HTuple();
            HTuple hv_ObjectModel3DBRightNew = new HTuple(), hv_ObjectModel3DBTop = new HTuple();
            HTuple hv_ObjectModel3DTopConnected = new HTuple(), hv_ObjectModel3DBTopNew = new HTuple();
            HTuple hv_ObjectModel3DBDown = new HTuple(), hv_ObjectModel3DBDownConnected = new HTuple();
            HTuple hv_ObjectModel3DBDownNew = new HTuple(), hv_PoseOut = new HTuple();
            HTuple hv_SampledObjectModel3DTop = new HTuple(), hv_SampledObjectModel3DDown = new HTuple();
            HTuple hv_SampledObjectModel3DLeft = new HTuple(), hv_SampledObjectModel3DRight = new HTuple();
            HTuple hv_Surface3DDefaultT = new HTuple(), hv_Info = new HTuple();
            HTuple hv_Surface3DDefaultD = new HTuple(), hv_Surface3DDefaultL = new HTuple();
            HTuple hv_Surface3DDefaultR = new HTuple(), hv_CenterPointT = new HTuple();
            HTuple hv_Radius = new HTuple(), hv_CenterPointD = new HTuple();
            HTuple hv_CenterPointL = new HTuple(), hv_CenterPointR = new HTuple();
            HTuple hv_raduis = new HTuple(), hv_ObjectModel3DIntersections = new HTuple();
            HTuple hv_DisparityProfileWidth = new HTuple(), hv_DisparityProfileHeight = new HTuple();
            HTuple hv_WindowEnlargement = new HTuple(), hv_WindowWidth = new HTuple();
            HTuple hv_WindowHeight = new HTuple(), hv_VisualizationPlaneSize = new HTuple();
            HTuple hv_PoseT = new HTuple(), hv_PoseD = new HTuple();
            HTuple hv_PoseL = new HTuple(), hv_PoseR = new HTuple();
            HTuple hv_IntersectionPlane1 = new HTuple(), hv_IntersectionPlane2 = new HTuple();
            HTuple hv_IntersectionPlane3 = new HTuple(), hv_IntersectionPlane4 = new HTuple();
            HTuple hv_IntersectionPlane5 = new HTuple(), hv_top_Hypotenuse = new HTuple();
            HTuple hv_top_LefttAngle = new HTuple(), hv_top_RightAngle = new HTuple();
            HTuple hv_top_HypotenuseFH = new HTuple(), hv_top_HypotenuseSH = new HTuple();
            HTuple hv_topdown_HypotenuseDis = new HTuple(), hv_leftright_HypotenuseDis = new HTuple();
            HTuple hv_left_HypotenuseFH = new HTuple(), hv_left_HypotenuseSH = new HTuple();
            HTuple hv_lefttop_lineLength = new HTuple(), hv_righttop_lineLength = new HTuple();
            HTuple hv_leftdown_lineLength = new HTuple(), hv_rightdown_linelength = new HTuple();
            HTuple hv_left_Hypotenuse = new HTuple(), hv_left_LefttAngle = new HTuple();
            HTuple hv_left_RightAngle = new HTuple(), hv_right_HypotenuseFH = new HTuple();
            HTuple hv_right_HypotenuseSH = new HTuple(), hv_down_HypotenuseFH = new HTuple();
            HTuple hv_down_HypotenuseSH = new HTuple(), hv_down_Hypotenuse = new HTuple();
            HTuple hv_down_LefttAngle = new HTuple(), hv_down_RightAngle = new HTuple();
            HTuple hv_right_Hypotenuse = new HTuple(), hv_right_LefttAngle = new HTuple();
            HTuple hv_right_RightAngle = new HTuple(), hv_DistanceTopDownX = new HTuple();
            HTuple hv_DistanceLeftRightX = new HTuple(), hv_VisualizationColors = new HTuple();
            HTuple hv_LeftIndex = new HTuple(), hv_RightIndex = new HTuple();
            HTuple hv_TopIndex = new HTuple(), hv_DownIndex = new HTuple();
            HTuple hv_resultcon = new HTuple(), hv_n = new HTuple();
            HTuple hv_PoseT1 = new HTuple(), hv_PoseR1 = new HTuple();
            HTuple hv_PoseD1 = new HTuple(), hv_PoseL1 = new HTuple();
            HTuple hv_ObjectModel3DIntersectiont = new HTuple(), hv_numbercount = new HTuple();
            HTuple hv_lengthnum = new HTuple(), hv_Length = new HTuple();
            HTuple hv_Max = new HTuple(), hv_Indices = new HTuple();
            HTuple hv_Numberct = new HTuple(), hv_Row = new HTuple();
            HTuple hv_Col = new HTuple(), hv_Colmin = new HTuple();
            HTuple hv_Colmax = new HTuple(), hv_ObjectModel3DIntersectionl = new HTuple();
            HTuple hv_Lengthl = new HTuple(), hv_Maxl = new HTuple();
            HTuple hv_Numbercl = new HTuple(), hv_ObjectModel3DIntersectionr = new HTuple();
            HTuple hv_Lengthr = new HTuple(), hv_Maxr = new HTuple();
            HTuple hv_Numbercr = new HTuple(), hv_ObjectModel3DIntersectiond = new HTuple();
            HTuple hv_Lengthd = new HTuple(), hv_Maxd = new HTuple();
            HTuple hv_Numbercd = new HTuple(), hv_Left3DX = new HTuple();
            HTuple hv_Right3DX = new HTuple(), hv_Down3DY = new HTuple();
            HTuple hv_resultcon1 = new HTuple(), hv_resultcon2 = new HTuple();
            HTuple hv_resultcon3 = new HTuple(), hv_resultcon4 = new HTuple();
            HTuple hv_Rowsleft = new HTuple(), hv_Colsleft = new HTuple();
            HTuple hv_Rowsrigt = new HTuple(), hv_Colsrigt = new HTuple();
            HTuple hv_Rowsmidt = new HTuple(), hv_Colsmidt = new HTuple();
            HTuple hv_Minleft = new HTuple(), hv_Maxleft = new HTuple();
            HTuple hv_Minrigt = new HTuple(), hv_Maxrigt = new HTuple();
            HTuple hv_RowBeginleft = new HTuple(), hv_ColBeginleft = new HTuple();
            HTuple hv_RowEndleft = new HTuple(), hv_ColEndleft = new HTuple();
            HTuple hv_Nrleft = new HTuple(), hv_Ncleft = new HTuple();
            HTuple hv_Distleft = new HTuple(), hv_RowBeginrigt = new HTuple();
            HTuple hv_ColBeginrigt = new HTuple(), hv_RowEndrigt = new HTuple();
            HTuple hv_ColEndrigt = new HTuple(), hv_Nrrigt = new HTuple();
            HTuple hv_Ncrigt = new HTuple(), hv_Distrigt = new HTuple();
            HTuple hv_lengthleftcontour = new HTuple(), hv_lengthleflined = new HTuple();
            HTuple hv_Abslengthleftcontour = new HTuple(), hv_Abslengthleflined = new HTuple();
            HTuple hv_countofCols = new HTuple(), hv_Index = new HTuple();
            HTuple hv_Rowsnewleft = new HTuple(), hv_Colsnewleft = new HTuple();
            HTuple hv_Indexx = new HTuple(), hv_Index1 = new HTuple();
            HTuple hv_Rowsnewmidt = new HTuple(), hv_Colsnewmidt = new HTuple();
            HTuple hv_countofRowst = new HTuple(), hv_RowBeginmidt = new HTuple();
            HTuple hv_ColBeginmidt = new HTuple(), hv_RowEndmidt = new HTuple();
            HTuple hv_ColEndmidt = new HTuple(), hv_Nrmidt = new HTuple();
            HTuple hv_Ncmidt = new HTuple(), hv_Distmidt = new HTuple();
            HTuple hv_lengthrigtcontour = new HTuple(), hv_lengthriglined = new HTuple();
            HTuple hv_Abslengthrigtcontour = new HTuple(), hv_Abslengthriglined = new HTuple();
            HTuple hv_Rowsnewrigt = new HTuple(), hv_Colsnewrigt = new HTuple();
            HTuple hv_Rowslefd = new HTuple(), hv_Colslefd = new HTuple();
            HTuple hv_Rowsrigd = new HTuple(), hv_Colsrigd = new HTuple();
            HTuple hv_Rowsmidd = new HTuple(), hv_Colsmidd = new HTuple();
            HTuple hv_Minlefd = new HTuple(), hv_Maxlefd = new HTuple();
            HTuple hv_Minrigd = new HTuple(), hv_Maxrigd = new HTuple();
            HTuple hv_RowBeginlefd = new HTuple(), hv_ColBeginlefd = new HTuple();
            HTuple hv_RowEndlefd = new HTuple(), hv_ColEndlefd = new HTuple();
            HTuple hv_Nrlefd = new HTuple(), hv_Nclefd = new HTuple();
            HTuple hv_Distlefd = new HTuple(), hv_RowBeginrigd = new HTuple();
            HTuple hv_ColBeginrigd = new HTuple(), hv_RowEndrigd = new HTuple();
            HTuple hv_ColEndrigd = new HTuple(), hv_Nrrigd = new HTuple();
            HTuple hv_Ncrigd = new HTuple(), hv_Distrigd = new HTuple();
            HTuple hv_lengthlefdcontour = new HTuple(), hv_Abslengthlefdcontour = new HTuple();
            HTuple hv_Rowsnewlefd = new HTuple(), hv_Colsnewlefd = new HTuple();
            HTuple hv_Rowsnewmidd = new HTuple(), hv_Colsnewmidd = new HTuple();
            HTuple hv_countofRowsd = new HTuple(), hv_RowBeginmidd = new HTuple();
            HTuple hv_ColBeginmidd = new HTuple(), hv_RowEndmidd = new HTuple();
            HTuple hv_ColEndmidd = new HTuple(), hv_Nrmidd = new HTuple();
            HTuple hv_Ncmidd = new HTuple(), hv_Distmidd = new HTuple();
            HTuple hv_lengthrigdcontour = new HTuple(), hv_Abslengthrigdcontour = new HTuple();
            HTuple hv_Rowsnewrigd = new HTuple(), hv_Colsnewrigd = new HTuple();
            HTuple hv_Rowslefl = new HTuple(), hv_Colslefl = new HTuple();
            HTuple hv_Rowsrigl = new HTuple(), hv_Colsrigl = new HTuple();
            HTuple hv_Rowsmidl = new HTuple(), hv_Colsmidl = new HTuple();
            HTuple hv_Minlefl = new HTuple(), hv_Maxlefl = new HTuple();
            HTuple hv_Minrigl = new HTuple(), hv_Maxrigl = new HTuple();
            HTuple hv_RowBeginlefl = new HTuple(), hv_ColBeginlefl = new HTuple();
            HTuple hv_RowEndlefl = new HTuple(), hv_ColEndlefl = new HTuple();
            HTuple hv_Nrlefl = new HTuple(), hv_Nclefl = new HTuple();
            HTuple hv_Distlefl = new HTuple(), hv_RowBeginrigl = new HTuple();
            HTuple hv_ColBeginrigl = new HTuple(), hv_RowEndrigl = new HTuple();
            HTuple hv_ColEndrigl = new HTuple(), hv_Nrrigl = new HTuple();
            HTuple hv_Ncrigl = new HTuple(), hv_Distrigl = new HTuple();
            HTuple hv_lengthleflcontour = new HTuple(), hv_Abslengthleflcontour = new HTuple();
            HTuple hv_Rowsnewlefl = new HTuple(), hv_Colsnewlefl = new HTuple();
            HTuple hv_Rowsnewmidl = new HTuple(), hv_Colsnewmidl = new HTuple();
            HTuple hv_countofRowsl = new HTuple(), hv_Rowsnewrigl = new HTuple();
            HTuple hv_Colsnewrigl = new HTuple(), hv_RowBeginmidl = new HTuple();
            HTuple hv_ColBeginmidl = new HTuple(), hv_RowEndmidl = new HTuple();
            HTuple hv_ColEndmidl = new HTuple(), hv_Nrmidl = new HTuple();
            HTuple hv_Ncmidl = new HTuple(), hv_Distmidl = new HTuple();
            HTuple hv_Rowslefr = new HTuple(), hv_Colslefr = new HTuple();
            HTuple hv_Rowsrigr = new HTuple(), hv_Colsrigr = new HTuple();
            HTuple hv_Rowsmidr = new HTuple(), hv_Colsmidr = new HTuple();
            HTuple hv_Minlefr = new HTuple(), hv_Maxlefr = new HTuple();
            HTuple hv_Minrigr = new HTuple(), hv_Maxrigr = new HTuple();
            HTuple hv_RowBeginlefr = new HTuple(), hv_ColBeginlefr = new HTuple();
            HTuple hv_RowEndlefr = new HTuple(), hv_ColEndlefr = new HTuple();
            HTuple hv_Nrlefr = new HTuple(), hv_Nclefr = new HTuple();
            HTuple hv_Distlefr = new HTuple(), hv_RowBeginrigr = new HTuple();
            HTuple hv_ColBeginrigr = new HTuple(), hv_RowEndrigr = new HTuple();
            HTuple hv_ColEndrigr = new HTuple(), hv_Nrrigr = new HTuple();
            HTuple hv_Ncrigr = new HTuple(), hv_Distrigr = new HTuple();
            HTuple hv_lengthlefrcontour = new HTuple(), hv_Abslengthlefrcontour = new HTuple();
            HTuple hv_Rowsnewlefr = new HTuple(), hv_Colsnewlefr = new HTuple();
            HTuple hv_Rowsnewmidr = new HTuple(), hv_Colsnewmidr = new HTuple();
            HTuple hv_countofRowsr = new HTuple(), hv_RowBeginmidr = new HTuple();
            HTuple hv_ColBeginmidr = new HTuple(), hv_RowEndmidr = new HTuple();
            HTuple hv_ColEndmidr = new HTuple(), hv_Nrmidr = new HTuple();
            HTuple hv_Ncmidr = new HTuple(), hv_Distmidr = new HTuple();
            HTuple hv_lengthrigrcontour = new HTuple(), hv_Abslengthrigrcontour = new HTuple();
            HTuple hv_countofColr = new HTuple(), hv_Rowsnewrigr = new HTuple();
            HTuple hv_Colsnewrigr = new HTuple(), hv_Minl = new HTuple();
            HTuple hv_Minr = new HTuple(), hv_Angleradttl = new HTuple();
            HTuple hv_Degtl = new HTuple(), hv_DegtlAbs = new HTuple();
            HTuple hv_Angleradtr = new HTuple(), hv_Degtr = new HTuple();
            HTuple hv_DegtrAbs = new HTuple(), hv_Angleraddtl = new HTuple();
            HTuple hv_Degdl = new HTuple(), hv_DegdlAbs = new HTuple();
            HTuple hv_Angleraddtr = new HTuple(), hv_DegdrAbs = new HTuple();
            HTuple hv_Angleradd = new HTuple(), hv_Degtd = new HTuple();
            HTuple hv_DegtdAbs = new HTuple(), hv_RowsMidt = new HTuple();
            HTuple hv_ColsMidt = new HTuple(), hv_RowsMidd = new HTuple();
            HTuple hv_ColsMidd = new HTuple(), hv_RowsMidl = new HTuple();
            HTuple hv_ColsMidl = new HTuple(), hv_RowsMidr = new HTuple();
            HTuple hv_ColsMidr = new HTuple(), hv_meanRowst = new HTuple();
            HTuple hv_meanColst = new HTuple(), hv_meanRowsd = new HTuple();
            HTuple hv_meanColsd = new HTuple(), hv_meanRowsl = new HTuple();
            HTuple hv_meanColsl = new HTuple(), hv_meanRowsr = new HTuple();
            HTuple hv_meanColsr = new HTuple(), hv_minColsd = new HTuple();
            HTuple hv_maxColsd = new HTuple(), hv_minColst = new HTuple();
            HTuple hv_maxColst = new HTuple(), hv_minColsl = new HTuple();
            HTuple hv_maxColsl = new HTuple(), hv_minColsr = new HTuple();
            HTuple hv_maxColsr = new HTuple(), hv_HomMat2DIdentity = new HTuple();
            HTuple hv_Areal = new HTuple(), hv_Rowl = new HTuple();
            HTuple hv_Columnl = new HTuple(), hv_PointOrderl = new HTuple();
            HTuple hv_HomMatTransPreL = new HTuple(), hv_RowsMidnewt = new HTuple();
            HTuple hv_ColsMidnewt = new HTuple(), hv_MeanMidnewColt = new HTuple();
            HTuple hv_MeanMidnewRowy = new HTuple(), hv_RowPrel = new HTuple();
            HTuple hv_ColumnPrel = new HTuple(), hv_HomMat2DLRotate = new HTuple();
            HTuple hv_HomMat2DScale = new HTuple(), hv_meanMidColnew = new HTuple();
            HTuple hv_RowBeginmidtnew = new HTuple(), hv_ColBeginmidtnew = new HTuple();
            HTuple hv_RowEndmidtnew = new HTuple(), hv_ColEndmidtnew = new HTuple();
            HTuple hv_Rowminmidt = new HTuple(), hv_SubRow = new HTuple();
            HTuple hv_RowsTopl = new HTuple(), hv_ColsTopl = new HTuple();
            HTuple hv_RowsDowl = new HTuple(), hv_ColsDowl = new HTuple();
            HTuple hv_meanTopl = new HTuple(), hv_meanDowl = new HTuple();
            HTuple hv_RowBegintopl = new HTuple(), hv_ColBegintopl = new HTuple();
            HTuple hv_RowEndtopl = new HTuple(), hv_ColEndtopl = new HTuple();
            HTuple hv_angleltx = new HTuple(), hv_Degltx = new HTuple();
            HTuple hv_AbgDegltx = new HTuple(), hv_Abgangleltx = new HTuple();
            HTuple hv_DegSubL = new HTuple(), hv_meanmidColsnew = new HTuple();
            HTuple hv_Sintl = new HTuple(), hv_Costl = new HTuple();
            HTuple hv_AbsSintl = new HTuple(), hv_AbsCostl = new HTuple();
            HTuple hv_DistanceLPre = new HTuple(), hv_RowsnewMidl = new HTuple();
            HTuple hv_ColsnewMidl = new HTuple(), hv_Left3DLineX = new HTuple();
            HTuple hv_AbsLeft3Ddistance = new HTuple(), hv_AbsLeft3DX = new HTuple();
            HTuple hv_Left3DLineY = new HTuple(), hv_Left3DY = new HTuple();
            HTuple hv_RowIntersectiontl = new HTuple(), hv_ColIntersectiontl = new HTuple();
            HTuple hv_IsOverlapping = new HTuple(), hv_RowIntersectiont = new HTuple();
            HTuple hv_ColIntersectiont = new HTuple(), hv_DistanceLT = new HTuple();
            HTuple hv_MeanMidlx = new HTuple(), hv_AbsmeanRowsl = new HTuple();
            HTuple hv_DistanceXPre = new HTuple(), hv_RowIntersectiontd = new HTuple();
            HTuple hv_ColIntersectiontd = new HTuple(), hv_DistanceDL = new HTuple();
            HTuple hv_Arear = new HTuple(), hv_Rowr = new HTuple();
            HTuple hv_Columnr = new HTuple(), hv_PointOrderr = new HTuple();
            HTuple hv_HomMatTransPreR = new HTuple(), hv_RowPrer = new HTuple();
            HTuple hv_ColumnPrer = new HTuple(), hv_HomMat2DRRotate = new HTuple();
            HTuple hv_RowBeginmidnewr = new HTuple(), hv_ColBeginmidnewr = new HTuple();
            HTuple hv_RowEndmidnewr = new HTuple(), hv_ColEndmidnewr = new HTuple();
            HTuple hv_Rowmidrmin = new HTuple(), hv_Rowsrmidnew = new HTuple();
            HTuple hv_Colsrmidnew = new HTuple(), hv_RowsTopr = new HTuple();
            HTuple hv_ColsTopr = new HTuple(), hv_RowsDowr = new HTuple();
            HTuple hv_ColsDowr = new HTuple(), hv_meanTopr = new HTuple();
            HTuple hv_meanDowr = new HTuple(), hv_RowBegintopr = new HTuple();
            HTuple hv_ColBegintopr = new HTuple(), hv_RowEndtopr = new HTuple();
            HTuple hv_ColEndtopr = new HTuple(), hv_Degrtr = new HTuple();
            HTuple hv_DegrtrAbs = new HTuple(), hv_DegSubR = new HTuple();
            HTuple hv_AngleradtrAbs = new HTuple(), hv_Sintr = new HTuple();
            HTuple hv_Costr = new HTuple(), hv_AbsSintr = new HTuple();
            HTuple hv_AbsCostr = new HTuple(), hv_DistanceRPre = new HTuple();
            HTuple hv_RowsnewMidr = new HTuple(), hv_ColsnewMidr = new HTuple();
            HTuple hv_Right3DLineX = new HTuple(), hv_AbsRight3DDistance = new HTuple();
            HTuple hv_AbsRight3DX = new HTuple(), hv_Right3DLineY = new HTuple();
            HTuple hv_Right3DY = new HTuple(), hv_RowIntersectiontr = new HTuple();
            HTuple hv_ColIntersectiontr = new HTuple(), hv_DistanceRT = new HTuple();
            HTuple hv_MeanMidrx = new HTuple(), hv_AbsmeanRowsr = new HTuple();
            HTuple hv_DistanceRD = new HTuple(), hv_Aread = new HTuple();
            HTuple hv_Rowd = new HTuple(), hv_Columnd = new HTuple();
            HTuple hv_HomMatTransPreD = new HTuple(), hv_RowPred = new HTuple();
            HTuple hv_ColumnPred = new HTuple(), hv_PointOrderd = new HTuple();
            HTuple hv_Rowmiddnew = new HTuple(), hv_Colmiddnew = new HTuple();
            HTuple hv_meannewmid = new HTuple(), hv_RowBegintopd = new HTuple();
            HTuple hv_ColBegintopd = new HTuple(), hv_RowEndtopd = new HTuple();
            HTuple hv_ColEndtopd = new HTuple(), hv_Degltxabs = new HTuple();
            HTuple hv_DegSubD = new HTuple(), hv_RowBeginmiddnew = new HTuple();
            HTuple hv_ColBeginmiddnew = new HTuple(), hv_RowEndmiddnew = new HTuple();
            HTuple hv_ColEndmiddnew = new HTuple(), hv_Distancediagonal = new HTuple();
            HTuple hv_RowMidDown = new HTuple(), hv_ColMidDown = new HTuple();
            HTuple hv_meanDownRNew = new HTuple(), hv_meanDownCNew = new HTuple();
            HTuple hv_distancediag_td = new HTuple(), hv_Down3DLineY = new HTuple();
            HTuple hv_AbsDown3DDistance = new HTuple(), hv_AbsMeanRowst = new HTuple();
            HTuple hv_DistanceTD = new HTuple(), hv_AbsmeanRowst = new HTuple();
            HTuple hv_meanRightC = new HTuple(), hv_meanRightR = new HTuple();
            HTuple hv_distancediag_lr = new HTuple(), hv_Exception = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageOutLeft);
            HOperatorSet.GenEmptyObj(out ho_ImageOutRight);
            HOperatorSet.GenEmptyObj(out ho_ImageOutTop);
            HOperatorSet.GenEmptyObj(out ho_ImageOutDown);
            HOperatorSet.GenEmptyObj(out ho_ImagePart);
            HOperatorSet.GenEmptyObj(out ho_ImagePartReal);
            HOperatorSet.GenEmptyObj(out ho_ImagePartLeft);
            HOperatorSet.GenEmptyObj(out ho_ImagePartLeftNew);
            HOperatorSet.GenEmptyObj(out ho_RegionLeft);
            HOperatorSet.GenEmptyObj(out ho_ImageReducedLeft);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_ImagePartRight);
            HOperatorSet.GenEmptyObj(out ho_ImagePartRightNew);
            HOperatorSet.GenEmptyObj(out ho_RegionRight);
            HOperatorSet.GenEmptyObj(out ho_ImageReducedRight);
            HOperatorSet.GenEmptyObj(out ho_ImagePartTop);
            HOperatorSet.GenEmptyObj(out ho_ImagePartTopNew);
            HOperatorSet.GenEmptyObj(out ho_RegionTop);
            HOperatorSet.GenEmptyObj(out ho_ImageReducedTop);
            HOperatorSet.GenEmptyObj(out ho_ImagePartDown);
            HOperatorSet.GenEmptyObj(out ho_ImagePartDownNew);
            HOperatorSet.GenEmptyObj(out ho_RegionDown);
            HOperatorSet.GenEmptyObj(out ho_ImageReducedDown);
            HOperatorSet.GenEmptyObj(out ho_Bx);
            HOperatorSet.GenEmptyObj(out ho_By);
            HOperatorSet.GenEmptyObj(out ho_Bz);
            HOperatorSet.GenEmptyObj(out ho_Intersectiont);
            HOperatorSet.GenEmptyObj(out ho_UnionContourst);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelectedt);
            HOperatorSet.GenEmptyObj(out ho_ContoursSplitt);
            HOperatorSet.GenEmptyObj(out ho_SelectedContourst);
            HOperatorSet.GenEmptyObj(out ho_contournewmid);
            HOperatorSet.GenEmptyObj(out ho_Intersectionl);
            HOperatorSet.GenEmptyObj(out ho_UnionContoursl);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelectedl);
            HOperatorSet.GenEmptyObj(out ho_ContoursSplitl);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursl);
            HOperatorSet.GenEmptyObj(out ho_Intersectionr);
            HOperatorSet.GenEmptyObj(out ho_UnionContoursr);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelectedr);
            HOperatorSet.GenEmptyObj(out ho_ContoursSplitr);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursr);
            HOperatorSet.GenEmptyObj(out ho_Intersectiond);
            HOperatorSet.GenEmptyObj(out ho_UnionContoursd);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelectedd);
            HOperatorSet.GenEmptyObj(out ho_ContoursSplitd);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursd);
            HOperatorSet.GenEmptyObj(out ho_contourleft);
            HOperatorSet.GenEmptyObj(out ho_contourmidt);
            HOperatorSet.GenEmptyObj(out ho_contourrigt);
            HOperatorSet.GenEmptyObj(out ho_contourlefr);
            HOperatorSet.GenEmptyObj(out ho_contourmidr);
            HOperatorSet.GenEmptyObj(out ho_contourrigr);
            HOperatorSet.GenEmptyObj(out ho_contourlefl);
            HOperatorSet.GenEmptyObj(out ho_contourmidl);
            HOperatorSet.GenEmptyObj(out ho_contourrigl);
            HOperatorSet.GenEmptyObj(out ho_contourlefd);
            HOperatorSet.GenEmptyObj(out ho_contourmidd);
            HOperatorSet.GenEmptyObj(out ho_contourrigd);
            HOperatorSet.GenEmptyObj(out ho_contourtmp);
            HOperatorSet.GenEmptyObj(out ho_SelectedContourslPre);
            HOperatorSet.GenEmptyObj(out ho_contourtoplnew);
            HOperatorSet.GenEmptyObj(out ho_contourmidlnew);
            HOperatorSet.GenEmptyObj(out ho_contourdowlnew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContourslfinal);
            HOperatorSet.GenEmptyObj(out ho_SelectedContourslnewpre);
            HOperatorSet.GenEmptyObj(out ho_SelectedContourslprenew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContourslnew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursrPre);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursrfinal);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursrnewpre);
            HOperatorSet.GenEmptyObj(out ho_contourtoprnew);
            HOperatorSet.GenEmptyObj(out ho_contourmidrnew);
            HOperatorSet.GenEmptyObj(out ho_contourdowrnew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursrprenew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursrnew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursdPre);
            HOperatorSet.GenEmptyObj(out ho_contourmiddnew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursdnewpre);
            HOperatorSet.GenEmptyObj(out ho_contourtopdnew);
            HOperatorSet.GenEmptyObj(out ho_contourdowdnew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursdnewnpre);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursdprenew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursdnew);
            HOperatorSet.GenEmptyObj(out ho_SelectedContoursrnewnpre);
            hv_LeftTDistance = new HTuple();
            hv_LeftDDistance = new HTuple();
            hv_RightTDistance = new HTuple();
            hv_RightDDistance = new HTuple();
            hv_DownDistance = new HTuple();
            hv_LeftRightDistance = new HTuple();
            try
            {
                ho_ImageOutLeft.Dispose();
                ho_ImageOutLeft = new HObject(ho_ImageLeft);
                ho_ImageOutRight.Dispose();
                ho_ImageOutRight = new HObject(ho_ImageRight);
                ho_ImageOutTop.Dispose();
                ho_ImageOutTop = new HObject(ho_ImageTop);
                ho_ImageOutDown.Dispose();
                ho_ImageOutDown = new HObject(ho_ImageDown);


                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_ImageOutLeft, out hv_Width, out hv_Height);
                ho_ImagePart.Dispose();
                HOperatorSet.GenImageProto(ho_ImageOutLeft, out ho_ImagePart, 60);
                ho_ImagePartReal.Dispose();
                HOperatorSet.ConvertImageType(ho_ImagePart, out ho_ImagePartReal, "real");
                ho_ImagePartLeft.Dispose();
                HOperatorSet.SubImage(ho_ImagePartReal, ho_ImageOutLeft, out ho_ImagePartLeft,
                    1, 0);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_ImagePartLeftNew.Dispose();
                    HOperatorSet.CropPart(ho_ImagePartLeft, out ho_ImagePartLeftNew, 0, 0, hv_Width - 600,
                        hv_Height);
                }
                ho_RegionLeft.Dispose();
                HOperatorSet.Threshold(ho_ImagePartLeftNew, out ho_RegionLeft, 50, 60);
                ho_ImageReducedLeft.Dispose();
                HOperatorSet.ReduceDomain(ho_ImagePartLeftNew, ho_RegionLeft, out ho_ImageReducedLeft
                    );

                //erosion_rectangle1 (RegionLeft, RegionErosion, 7, 7)
                //reduce_domain (ImagePartLeftNew, RegionErosion, ImageReducedLeft)
                //gauss_filter (ImageReducedLeft, ImageGaussLeft, 5)

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_ImageOutRight, out hv_Width, out hv_Height);
                ho_ImagePart.Dispose();
                HOperatorSet.GenImageProto(ho_ImageOutRight, out ho_ImagePart, 60);
                ho_ImagePartReal.Dispose();
                HOperatorSet.ConvertImageType(ho_ImagePart, out ho_ImagePartReal, "real");
                ho_ImagePartRight.Dispose();
                HOperatorSet.SubImage(ho_ImagePartReal, ho_ImageOutRight, out ho_ImagePartRight,
                    1, 0);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_ImagePartRightNew.Dispose();
                    HOperatorSet.CropPart(ho_ImagePartRight, out ho_ImagePartRightNew, 0, 400,
                        hv_Width - 800, hv_Height);
                }
                ho_RegionRight.Dispose();
                HOperatorSet.Threshold(ho_ImagePartRightNew, out ho_RegionRight, 50, 60);
                ho_RegionErosion.Dispose();
                HOperatorSet.ErosionRectangle1(ho_RegionRight, out ho_RegionErosion, 7, 7);
                ho_ImageReducedRight.Dispose();
                HOperatorSet.ReduceDomain(ho_ImagePartRightNew, ho_RegionErosion, out ho_ImageReducedRight
                    );
                //gauss_filter (ImageReducedRight, ImageGaussRight, 5)

                //threshold (ImagePartRightNew, RegionRight, 40, 100)
                //erosion_rectangle1 (RegionRight, RegionErosion, 7, 7)
                //reduce_domain (ImagePartRightNew, RegionErosion, ImageReducedRight)
                //gauss_filter (ImageReducedRight, ImageGaussRight, 5)

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_ImageOutTop, out hv_Width, out hv_Height);
                ho_ImagePart.Dispose();
                HOperatorSet.GenImageProto(ho_ImageOutTop, out ho_ImagePart, 60);
                ho_ImagePartReal.Dispose();
                HOperatorSet.ConvertImageType(ho_ImagePart, out ho_ImagePartReal, "real");
                ho_ImagePartTop.Dispose();
                HOperatorSet.SubImage(ho_ImagePartReal, ho_ImageOutTop, out ho_ImagePartTop,
                    1, 0);
                ho_ImagePartTopNew.Dispose();
                HOperatorSet.CropPart(ho_ImagePartTop, out ho_ImagePartTopNew, 0, 0, hv_Width,
                    hv_Height);
                ho_RegionTop.Dispose();
                HOperatorSet.Threshold(ho_ImagePartTopNew, out ho_RegionTop, 50, 60);
                ho_ImageReducedTop.Dispose();
                HOperatorSet.ReduceDomain(ho_ImagePartTopNew, ho_RegionTop, out ho_ImageReducedTop
                    );

                //threshold (ImagePartLeftNew, RegionLeft, 50, 60)
                //reduce_domain (ImagePartLeftNew, RegionLeft, ImageReducedLeft)
                //threshold (ImagePartTopNew, RegionTop, 40, 100)
                //erosion_rectangle1 (RegionTop, RegionErosion, 7, 7)
                //reduce_domain (ImagePartTopNew, RegionErosion, ImageReducedTop)
                //gauss_filter (ImageReducedTop, ImageGaussTop, 5)

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_ImageOutDown, out hv_Width, out hv_Height);
                ho_ImagePart.Dispose();
                HOperatorSet.GenImageProto(ho_ImageOutDown, out ho_ImagePart, 60);
                ho_ImagePartReal.Dispose();
                HOperatorSet.ConvertImageType(ho_ImagePart, out ho_ImagePartReal, "real");
                ho_ImagePartDown.Dispose();
                HOperatorSet.SubImage(ho_ImagePartReal, ho_ImageOutDown, out ho_ImagePartDown,
                    1, 0);
                ho_ImagePartDownNew.Dispose();
                HOperatorSet.CropPart(ho_ImagePartDown, out ho_ImagePartDownNew, 0, 0, hv_Width,
                    hv_Height);
                ho_RegionDown.Dispose();
                HOperatorSet.Threshold(ho_ImagePartDownNew, out ho_RegionDown, 50, 60);
                ho_ImageReducedDown.Dispose();
                HOperatorSet.ReduceDomain(ho_ImagePartDownNew, ho_RegionDown, out ho_ImageReducedDown
                    );

                //threshold (ImagePartDownNew, RegionDown, 40, 100)
                //erosion_rectangle1 (RegionDown, RegionErosion, 7, 7)
                //reduce_domain (ImagePartDownNew, RegionErosion, ImageReducedDown)
                //gauss_filter (ImageReducedDown, ImageGaussDown, 5)
                //convert_image_type (ImageOutLeft, ImagePartLeft, 'real')


                if (hv_Instructions == null)
                    hv_Instructions = new HTuple();
                hv_Instructions[0] = "Rotate: Left button";
                if (hv_Instructions == null)
                    hv_Instructions = new HTuple();
                hv_Instructions[1] = "Zoom:   Shift + left button";
                if (hv_Instructions == null)
                    hv_Instructions = new HTuple();
                hv_Instructions[2] = "Move:   Ctrl  + left button";
                if (HDevWindowStack.IsOpen())
                {
                    hv_WindowHandle = HDevWindowStack.GetActive();
                }
                hv_Message.Dispose();
                hv_Message = "Surface model";
                hv_r.Dispose();
                hv_r = "information";
                hv_ParameterValues.Dispose();
                hv_ParameterValues = "verbose";
                hv_Status.Dispose();
                hv_Status = "Performing triangulation with default settings ...";

                hv_yInterval.Dispose();
                hv_yInterval = 0.015;
                ho_Bx.Dispose(); ho_By.Dispose(); ho_Bz.Dispose(); hv_ObjectModel3DBLeft.Dispose();
                Creat_XYZ_From_sszn_COPY_New(ho_ImageReducedLeft, out ho_Bx, out ho_By, out ho_Bz,
                    0.015, out hv_ObjectModel3DBLeft);
                hv_ObjectModel3DBLeftConnected.Dispose();
                HOperatorSet.ConnectionObjectModel3d(hv_ObjectModel3DBLeft, "distance_3d",
                    0.15, out hv_ObjectModel3DBLeftConnected);
                hv_ObjectModel3DBLeftNew.Dispose();
                HOperatorSet.SelectObjectModel3d(hv_ObjectModel3DBLeftConnected, "num_points",
                    "and", 100000, 1e100, out hv_ObjectModel3DBLeftNew);

                //NumNeighbors := 25
                //get_object_model_3d_params (ObjectModel3DBLeft, 'neighbor_distance ' + NumNeighbors, DistanceDistribution)
                //比例
                //InlierRate := 90
                //Distance := sort(DistanceDistribution)[|DistanceDistribution| * InlierRate / 100]
                //get_object_model_3d_params (ObjectModel3DBLeft, 'num_points', NumPoints)
                //最不超过x的距离内的邻居数
                //select_points_object_model_3d (ObjectModel3DBLeft, 'num_neighbors ' + Distance, NumNeighbors, NumPoints, ObjectModel3DBLeftNew)

                ho_Bx.Dispose(); ho_By.Dispose(); ho_Bz.Dispose(); hv_ObjectModel3DBRight.Dispose();
                Creat_XYZ_From_sszn_COPY_New(ho_ImageReducedRight, out ho_Bx, out ho_By, out ho_Bz,
                    0.015, out hv_ObjectModel3DBRight);
                hv_ObjectModel3DBRightConnected.Dispose();
                HOperatorSet.ConnectionObjectModel3d(hv_ObjectModel3DBRight, "distance_3d",
                    0.15, out hv_ObjectModel3DBRightConnected);
                hv_ObjectModel3DBRightNew.Dispose();
                HOperatorSet.SelectObjectModel3d(hv_ObjectModel3DBRightConnected, "num_points",
                    "and", 100000, 1e100, out hv_ObjectModel3DBRightNew);

                ho_Bx.Dispose(); ho_By.Dispose(); ho_Bz.Dispose(); hv_ObjectModel3DBTop.Dispose();
                Creat_XYZ_From_sszn_COPY_New(ho_ImageReducedTop, out ho_Bx, out ho_By, out ho_Bz,
                    0.015, out hv_ObjectModel3DBTop);
                hv_ObjectModel3DTopConnected.Dispose();
                HOperatorSet.ConnectionObjectModel3d(hv_ObjectModel3DBTop, "distance_3d", 0.15,
                    out hv_ObjectModel3DTopConnected);
                hv_ObjectModel3DBTopNew.Dispose();
                HOperatorSet.SelectObjectModel3d(hv_ObjectModel3DTopConnected, "num_points",
                    "and", 100000, 1e100, out hv_ObjectModel3DBTopNew);

                ho_Bx.Dispose(); ho_By.Dispose(); ho_Bz.Dispose(); hv_ObjectModel3DBDown.Dispose();
                Creat_XYZ_From_sszn_COPY_New(ho_ImageReducedDown, out ho_Bx, out ho_By, out ho_Bz,
                    0.015, out hv_ObjectModel3DBDown);
                hv_ObjectModel3DBDownConnected.Dispose();
                HOperatorSet.ConnectionObjectModel3d(hv_ObjectModel3DBDown, "distance_3d",
                    0.15, out hv_ObjectModel3DBDownConnected);
                hv_ObjectModel3DBDownNew.Dispose();
                HOperatorSet.SelectObjectModel3d(hv_ObjectModel3DBDownConnected, "num_points",
                    "and", 100000, 1e100, out hv_ObjectModel3DBDownNew);



                //create_pose (50, 0, -360, 0, -180, 0, 'Rp+T', 'gba', 'point', PoseTran)
                //rigid_trans_object_model_3d (ObjectModel3DBDown, PoseTran, ObjectModel3DB1rigidDown3dtran)

                //create_pose (-177.5, 0, -180, 0, -90, 0, 'Rp+T', 'gba', 'point', PoseTran)
                //rigid_trans_object_model_3d (ObjectModel3DBLeft, PoseTran, ObjectModel3DB1rigidLeft3dtran)

                //create_pose (177.5, 0, -180, 0, 90, 0, 'Rp+T', 'gba', 'point', PoseTran)
                //rigid_trans_object_model_3d (ObjectModel3DBRight, PoseTran, ObjectModel3DB1rigidRight3dtran)

                //visualize_object_model_3d (WindowHandle, [ObjectModel3DBTop,ObjectModel3DB1rigidDown3dtran,ObjectModel3DB1rigidLeft3dtran,ObjectModel3DB1rigidRight3dtran], [], [], ['color_0','color_1','color_2','color_3' ], ['yellow','blue','cyan','red'], Message, [], Instructions, PoseOut)

                //dev_open_window (0, 0, 512, 512, 'black', WindowHandle1)
                //disp_object_model_3d (WindowHandle1, ObjectModel3DB, [], [], ['color_0' ], ['yellow'])
                //visualize_object_model_3d (WindowHandle, [ObjectModel3DBTop,ObjectModel3DB1rigidDown3dtran,ObjectModel3DB1rigidLeft3dtran,ObjectModel3DB1rigidDown3dtran], [], [], ['color_0','color_1','color_2','color_3' ], ['yellow','blue'], Message, [], Instructions, PoseOut)

                //connection_object_model_3d (ObjectModel3DBLeft, 'distance_3d', 0.15, ObjectModel3DBLeftConnected)
                //select_object_model_3d (ObjectModel3DBLeftConnected, 'num_points', 'and', 100000, 1e100, ObjectModel3DBLeftNew)
                //connection_object_model_3d (ObjectModel3DBTop, 'distance_3d', 0.15, ObjectModel3DBTopConnected)
                //select_object_model_3d (ObjectModel3DBTopConnected, 'num_points', 'and', 100000, 1e100, ObjectModel3DBTopNew)
                //connection_object_model_3d (ObjectModel3DBDown, 'distance_3d', 0.15, ObjectModel3DBDownConnected)
                //select_object_model_3d (ObjectModel3DBDownConnected, 'num_points', 'and', 100000, 1e100, ObjectModel3DBDownNew)
                //connection_object_model_3d (ObjectModel3DBRight, 'distance_3d', 0.15, ObjectModel3DBRightConnected)
                //select_object_model_3d (ObjectModel3DBRightConnected, 'num_points', 'and', 100000, 1e100, ObjectModel3DBRightNew)

                //select_object_model_3d (ObjectModel3DConnected, 'num_points', 'and', 10000, 1e100, ObjectModel3DB)
                //connection_object_model_3d (ObjectModel3DA, 'distance_3d', 0.2, ObjectModel3DConnected)
                //rigid_trans_object_model_3d (ObjectModel3DB, PoseInvert, ObjectModel3DRigidTrans)
                //union_object_model_3d ([ObjectModel3DRigidTrans,ObjectModel3DA], 'points_surface', UnionObjectModel3)
                //connection_object_model_3d (ObjectModel3DB, 'distance_3d', 0.2, ObjectModel3DConnected)
                //select_object_model_3d (ObjectModel3DB, 'num_points', 'and', 100000, 1e100, ObjectModel3DA)

                hv_SampledObjectModel3DTop.Dispose();
                HOperatorSet.SampleObjectModel3d(hv_ObjectModel3DBTop, "fast", 0.1, new HTuple(),
                    new HTuple(), out hv_SampledObjectModel3DTop);
                hv_SampledObjectModel3DDown.Dispose();
                HOperatorSet.SampleObjectModel3d(hv_ObjectModel3DBDown, "fast", 0.1, new HTuple(),
                    new HTuple(), out hv_SampledObjectModel3DDown);
                hv_SampledObjectModel3DLeft.Dispose();
                HOperatorSet.SampleObjectModel3d(hv_ObjectModel3DBLeft, "fast", 0.1, new HTuple(),
                    new HTuple(), out hv_SampledObjectModel3DLeft);
                hv_SampledObjectModel3DRight.Dispose();
                HOperatorSet.SampleObjectModel3d(hv_ObjectModel3DBRight, "fast", 0.1, new HTuple(),
                    new HTuple(), out hv_SampledObjectModel3DRight);


                //visualize_object_model_3d (WindowHandle, [SampledObjectModel3DTop,SampledObjectModel3DDown,SampledObjectModel3DLeft,SampledObjectModel3DRight], [], [], ['color_0','color_1','color_2','color_3' ], ['yellow','blue','cyan','red'], Message, [], Instructions, PoseOut)


                //get_object_model_3d_params (SampledObjectModel3DTop, 'point_coord_x', Xt)
                //get_object_model_3d_params (SampledObjectModel3DTop, 'point_coord_y', Yt)
                //get_object_model_3d_params (SampledObjectModel3DTop, 'point_coord_z', Zt)
                //gen_object_model_3d_from_points (Xt, Yt, Zt, ObjectModel3D1Top)

                //gen_object_model_3d_from_points (Xr, Yr, Zr, ObjectModel3D1Right)
                //visualize_object_model_3d (WindowHandle, [ObjectModel3D1Top,ObjectModel3D1Down,ObjectModel3D1Left,ObjectModel3D1Right], [], [], ['color_0','color_1','color_2','color_3' ], ['yellow','blue','cyan', 'red'], Message, [], Instructions, PoseOut)

                hv_Surface3DDefaultT.Dispose(); hv_Info.Dispose();
                HOperatorSet.TriangulateObjectModel3d(hv_SampledObjectModel3DTop, "greedy",
                    new HTuple(), new HTuple(), out hv_Surface3DDefaultT, out hv_Info);
                hv_Surface3DDefaultD.Dispose(); hv_Info.Dispose();
                HOperatorSet.TriangulateObjectModel3d(hv_SampledObjectModel3DDown, "greedy",
                    new HTuple(), new HTuple(), out hv_Surface3DDefaultD, out hv_Info);
                hv_Surface3DDefaultL.Dispose(); hv_Info.Dispose();
                HOperatorSet.TriangulateObjectModel3d(hv_SampledObjectModel3DLeft, "greedy",
                    new HTuple(), new HTuple(), out hv_Surface3DDefaultL, out hv_Info);
                hv_Surface3DDefaultR.Dispose(); hv_Info.Dispose();
                HOperatorSet.TriangulateObjectModel3d(hv_SampledObjectModel3DRight, "greedy",
                    new HTuple(), new HTuple(), out hv_Surface3DDefaultR, out hv_Info);

                //visualize_object_model_3d (WindowHandle, [Surface3DDefaultT,Surface3DDefaultD,Surface3DDefaultL,Surface3DDefaultR], [], [], ['color_0','color_1','color_2','color_3' ], ['yellow','blue','cyan', 'red'], Message, [], Instructions, PoseOut)
                //union_object_model_3d ([Surface3DDefaultT,Surface3DDefaultD, Surface3DDefaultL,Surface3DDefaultR], 'points_surface', UnionObjectModel3D)
                //***三维计算****
                hv_CenterPointT.Dispose(); hv_Radius.Dispose();
                HOperatorSet.SmallestSphereObjectModel3d(hv_SampledObjectModel3DTop, out hv_CenterPointT,
                    out hv_Radius);
                hv_CenterPointD.Dispose(); hv_Radius.Dispose();
                HOperatorSet.SmallestSphereObjectModel3d(hv_SampledObjectModel3DDown, out hv_CenterPointD,
                    out hv_Radius);
                hv_CenterPointL.Dispose(); hv_Radius.Dispose();
                HOperatorSet.SmallestSphereObjectModel3d(hv_SampledObjectModel3DLeft, out hv_CenterPointL,
                    out hv_Radius);
                hv_CenterPointR.Dispose(); hv_Radius.Dispose();
                HOperatorSet.SmallestSphereObjectModel3d(hv_SampledObjectModel3DRight, out hv_CenterPointR,
                    out hv_Radius);


                hv_raduis.Dispose();
                hv_raduis = new HTuple();
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetPart(HDevWindowStack.GetActive(), -1, -1, 1, 1);
                }
                hv_ObjectModel3DIntersections.Dispose();
                hv_ObjectModel3DIntersections = new HTuple();

                hv_DisparityProfileWidth.Dispose(); hv_DisparityProfileHeight.Dispose();
                HOperatorSet.GetImageSize(ho_ImageTop, out hv_DisparityProfileWidth, out hv_DisparityProfileHeight);
                hv_WindowEnlargement.Dispose();
                hv_WindowEnlargement = 350;
                hv_WindowWidth.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_WindowWidth = (hv_DisparityProfileWidth / 2) + hv_WindowEnlargement;
                }
                hv_WindowHeight.Dispose();
                hv_WindowHeight = 1000;
                //dev_open_window (0, 0, WindowWidth, WindowHeight, 'black', WindowHandle1)
                if (HDevWindowStack.IsOpen())
                {
                    //dev_set_part (0, 0, WindowHeight - 1, WindowWidth - 1)
                }
                if (HDevWindowStack.IsOpen())
                {
                    //dev_clear_window ()
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetPart(HDevWindowStack.GetActive(), 0, 0, 1000, 3100);
                }

                hv_VisualizationPlaneSize.Dispose();
                hv_VisualizationPlaneSize = 150;

                //visualize_object_model_3d (WindowHandle, [UnionObjectModel3D], [], [], ['color_0' ], ['yellow'], Message, [], Instructions, PoseOut)

                //gen_cam_par_area_scan_division (0.01, 0, 6e-6, 6e-6, WindowWidth / 2, WindowHeight / 2, WindowWidth, WindowHeight, VisualizationCamParam)
                //create_pose (CenterPoint[0] - 50, CenterPoint[1], CenterPoint[2] + 800, 0, 0, 0, 'Rp+T', 'gba', 'point', VisualizationPose)
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PoseT.Dispose();
                    HOperatorSet.CreatePose(hv_CenterPointT.TupleSelect(0), 0, 0, 90, 0, 0, "Rp+T",
                        "gba", "point", out hv_PoseT);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PoseD.Dispose();
                    HOperatorSet.CreatePose(hv_CenterPointD.TupleSelect(0), 0, 0, 90, 0, 0, "Rp+T",
                        "gba", "point", out hv_PoseD);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PoseL.Dispose();
                    HOperatorSet.CreatePose(hv_CenterPointL.TupleSelect(0), 0, 0, 90, 0, 0, "Rp+T",
                        "gba", "point", out hv_PoseL);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PoseR.Dispose();
                    HOperatorSet.CreatePose(hv_CenterPointR.TupleSelect(0), 0, 0, 90, 0, 0, "Rp+T",
                        "gba", "point", out hv_PoseR);
                }

                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IntersectionPlane1.Dispose();
                    HOperatorSet.GenPlaneObjectModel3d(hv_PoseT, (((new HTuple(-1)).TupleConcat(
                        -1)).TupleConcat(1)).TupleConcat(1) * hv_VisualizationPlaneSize, (((new HTuple(-1)).TupleConcat(
                        1)).TupleConcat(1)).TupleConcat(-1) * hv_VisualizationPlaneSize, out hv_IntersectionPlane1);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IntersectionPlane2.Dispose();
                    HOperatorSet.GenPlaneObjectModel3d(hv_PoseD, (((new HTuple(-1)).TupleConcat(
                        -1)).TupleConcat(1)).TupleConcat(1) * hv_VisualizationPlaneSize, (((new HTuple(-1)).TupleConcat(
                        1)).TupleConcat(1)).TupleConcat(-1) * hv_VisualizationPlaneSize, out hv_IntersectionPlane2);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IntersectionPlane3.Dispose();
                    HOperatorSet.GenPlaneObjectModel3d(hv_PoseL, (((new HTuple(-1)).TupleConcat(
                        -1)).TupleConcat(1)).TupleConcat(1) * hv_VisualizationPlaneSize, (((new HTuple(-1)).TupleConcat(
                        1)).TupleConcat(1)).TupleConcat(-1) * hv_VisualizationPlaneSize, out hv_IntersectionPlane3);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IntersectionPlane4.Dispose();
                    HOperatorSet.GenPlaneObjectModel3d(hv_PoseR, (((new HTuple(-1)).TupleConcat(
                        -1)).TupleConcat(1)).TupleConcat(1) * hv_VisualizationPlaneSize, (((new HTuple(-1)).TupleConcat(
                        1)).TupleConcat(1)).TupleConcat(-1) * hv_VisualizationPlaneSize, out hv_IntersectionPlane4);
                }

                //PoseOut[2] := 300
                if (hv_PoseT == null)
                    hv_PoseT = new HTuple();
                hv_PoseT[1] = 10;
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IntersectionPlane5.Dispose();
                    HOperatorSet.GenPlaneObjectModel3d(hv_PoseT, (((new HTuple(-1)).TupleConcat(
                        -1)).TupleConcat(1)).TupleConcat(1) * hv_VisualizationPlaneSize, (((new HTuple(-1)).TupleConcat(
                        1)).TupleConcat(1)).TupleConcat(-1) * hv_VisualizationPlaneSize, out hv_IntersectionPlane5);
                }

                //disp_object_model_3d (WindowHandle, [Surface3DDefault,IntersectionPlane1,IntersectionPlane2], VisualizationCamParam, PoseOut, ['color_1', 'color_2','alpha', 'alpha_0'], ['blue','orange',0.75, 1])
                //using (HDevDisposeHelper dh = new HDevDisposeHelper())
                //{
                //    hv_PoseOut.Dispose();
                //    visualize_object_model_3d(hv_WindowHandle, ((((hv_Surface3DDefaultT.TupleSelect(
                //        0))).TupleConcat(hv_IntersectionPlane1))).TupleConcat(hv_IntersectionPlane5),
                //        new HTuple(), new HTuple(), ((((new HTuple("color_0")).TupleConcat("color_1")).TupleConcat(
                //        "color_2")).TupleConcat("alpha")).TupleConcat("alpha_0"), ((((new HTuple("yellow")).TupleConcat(
                //        "cyan")).TupleConcat("blue")).TupleConcat(0.75)).TupleConcat(1), hv_Message,
                //        new HTuple(), hv_Instructions, out hv_PoseOut);
                //}

                hv_top_Hypotenuse.Dispose();
                hv_top_Hypotenuse = new HTuple();
                hv_top_LefttAngle.Dispose();
                hv_top_LefttAngle = new HTuple();
                hv_top_RightAngle.Dispose();
                hv_top_RightAngle = new HTuple();
                hv_top_HypotenuseFH.Dispose();
                hv_top_HypotenuseFH = new HTuple();
                hv_top_HypotenuseSH.Dispose();
                hv_top_HypotenuseSH = new HTuple();
                hv_topdown_HypotenuseDis.Dispose();
                hv_topdown_HypotenuseDis = new HTuple();
                hv_leftright_HypotenuseDis.Dispose();
                hv_leftright_HypotenuseDis = new HTuple();
                hv_left_HypotenuseFH.Dispose();
                hv_left_HypotenuseFH = new HTuple();
                hv_left_HypotenuseSH.Dispose();
                hv_left_HypotenuseSH = new HTuple();
                hv_lefttop_lineLength.Dispose();
                hv_lefttop_lineLength = new HTuple();
                hv_righttop_lineLength.Dispose();
                hv_righttop_lineLength = new HTuple();
                hv_leftdown_lineLength.Dispose();
                hv_leftdown_lineLength = new HTuple();
                hv_rightdown_linelength.Dispose();
                hv_rightdown_linelength = new HTuple();
                hv_left_Hypotenuse.Dispose();
                hv_left_Hypotenuse = new HTuple();
                hv_left_LefttAngle.Dispose();
                hv_left_LefttAngle = new HTuple();
                hv_left_RightAngle.Dispose();
                hv_left_RightAngle = new HTuple();
                hv_right_HypotenuseFH.Dispose();
                hv_right_HypotenuseFH = new HTuple();
                hv_right_HypotenuseSH.Dispose();
                hv_right_HypotenuseSH = new HTuple();
                hv_down_HypotenuseFH.Dispose();
                hv_down_HypotenuseFH = new HTuple();
                hv_down_HypotenuseSH.Dispose();
                hv_down_HypotenuseSH = new HTuple();
                hv_down_Hypotenuse.Dispose();
                hv_down_Hypotenuse = new HTuple();
                hv_down_LefttAngle.Dispose();
                hv_down_LefttAngle = new HTuple();
                hv_down_RightAngle.Dispose();
                hv_down_RightAngle = new HTuple();
                hv_right_Hypotenuse.Dispose();
                hv_right_Hypotenuse = new HTuple();
                hv_right_LefttAngle.Dispose();
                hv_right_LefttAngle = new HTuple();
                hv_right_RightAngle.Dispose();
                hv_right_RightAngle = new HTuple();
                hv_DistanceTopDownX.Dispose();
                hv_DistanceTopDownX = -1;
                hv_DistanceLeftRightX.Dispose();
                hv_DistanceLeftRightX = -1;
                hv_VisualizationColors.Dispose();
                hv_VisualizationColors = new HTuple();
                hv_VisualizationColors[0] = "magenta";
                hv_VisualizationColors[1] = "blue";
                hv_VisualizationColors[2] = "orange";

                hv_LeftIndex.Dispose();
                hv_LeftIndex = -1;
                hv_RightIndex.Dispose();
                hv_RightIndex = -1;
                hv_TopIndex.Dispose();
                hv_TopIndex = -1;
                hv_DownIndex.Dispose();
                hv_DownIndex = -1;
                hv_resultcon.Dispose();
                hv_resultcon = 0;


                HTuple end_val235 = hv_Height - 1;
                HTuple step_val235 = 2;
                for (hv_n = 1; hv_n.Continue(end_val235, step_val235); hv_n = hv_n.TupleAdd(step_val235))
                {
                    try
                    {
                        if (hv_PoseT == null)
                            hv_PoseT = new HTuple();
                        hv_PoseT[1] = hv_n * 0.075;
                        if (hv_PoseL == null)
                            hv_PoseL = new HTuple();
                        hv_PoseL[1] = hv_n * 0.075;
                        if (hv_PoseR == null)
                            hv_PoseR = new HTuple();
                        hv_PoseR[1] = hv_n * 0.075;
                        if (hv_PoseD == null)
                            hv_PoseD = new HTuple();
                        hv_PoseD[1] = hv_n * 0.075;

                        hv_PoseT1.Dispose();
                        hv_PoseT1 = new HTuple(hv_PoseT);
                        hv_PoseR1.Dispose();
                        hv_PoseR1 = new HTuple(hv_PoseR);
                        hv_PoseD1.Dispose();
                        hv_PoseD1 = new HTuple(hv_PoseD);
                        hv_PoseL1.Dispose();
                        hv_PoseL1 = new HTuple(hv_PoseL);

                        if ((int)(new HTuple(hv_TopIndex.TupleEqual(-1))) != 0)
                        {
                            hv_resultcon.Dispose();
                            hv_resultcon = 0;
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ObjectModel3DIntersectiont.Dispose();
                                HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefaultT.TupleSelect(
                                    0), hv_PoseT, out hv_ObjectModel3DIntersectiont);
                            }
                            ho_Intersectiont.Dispose(); hv_resultcon.Dispose();
                            project_object_model_3d_lines_to_contour_xld(out ho_Intersectiont, hv_PoseT1,
                                hv_ObjectModel3DIntersectiont, out hv_resultcon);
                            if ((int)(new HTuple(hv_resultcon.TupleEqual(1))) != 0)
                            {
                                hv_numbercount.Dispose();
                                HOperatorSet.ContourPointNumXld(ho_Intersectiont, out hv_numbercount);
                                hv_lengthnum.Dispose();
                                HOperatorSet.TupleLength(hv_numbercount, out hv_lengthnum);
                                if ((int)(new HTuple(hv_lengthnum.TupleGreater(0))) != 0)
                                {
                                    ho_UnionContourst.Dispose();
                                    HOperatorSet.UnionAdjacentContoursXld(ho_Intersectiont, out ho_UnionContourst,
                                        100, 1, "attr_keep");

                                    hv_Length.Dispose();
                                    HOperatorSet.LengthXld(ho_UnionContourst, out hv_Length);
                                    hv_Max.Dispose();
                                    HOperatorSet.TupleMax(hv_Length, out hv_Max);
                                    hv_Indices.Dispose();
                                    HOperatorSet.TupleFind(hv_Length, hv_Max, out hv_Indices);
                                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                    {
                                        ho_ObjectSelectedt.Dispose();
                                        HOperatorSet.SelectObj(ho_UnionContourst, out ho_ObjectSelectedt, hv_Indices + 1);
                                    }

                                    ho_ContoursSplitt.Dispose();
                                    HOperatorSet.SegmentContoursXld(ho_ObjectSelectedt, out ho_ContoursSplitt,
                                        "lines", 3, 2, 2);
                                    ho_SelectedContourst.Dispose();
                                    HOperatorSet.SelectContoursXld(ho_ContoursSplitt, out ho_SelectedContourst,
                                        "contour_length", 1, 2999, -0.5, 0.5);

                                    hv_Numberct.Dispose();
                                    HOperatorSet.CountObj(ho_SelectedContourst, out hv_Numberct);
                                    if ((int)(new HTuple(hv_Numberct.TupleEqual(3))) != 0)
                                    {
                                        ho_contournewmid.Dispose();
                                        HOperatorSet.SelectObj(ho_SelectedContourst, out ho_contournewmid,
                                            2);
                                        hv_Row.Dispose(); hv_Col.Dispose();
                                        HOperatorSet.GetContourXld(ho_contournewmid, out hv_Row, out hv_Col);
                                        hv_Colmin.Dispose();
                                        HOperatorSet.TupleMin(hv_Col, out hv_Colmin);
                                        hv_Colmax.Dispose();
                                        HOperatorSet.TupleMax(hv_Col, out hv_Colmax);
                                        if ((int)(new HTuple(((hv_Colmax - hv_Colmin)).TupleLessEqual(16.5))) != 0)
                                        {
                                            hv_TopIndex.Dispose();
                                            hv_TopIndex = new HTuple(hv_n);
                                        }
                                    }
                                }
                            }

                        }

                        if ((int)(new HTuple(hv_LeftIndex.TupleEqual(-1))) != 0)
                        {
                            hv_resultcon.Dispose();
                            hv_resultcon = 0;
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ObjectModel3DIntersectionl.Dispose();
                                HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefaultL.TupleSelect(
                                    0), hv_PoseL, out hv_ObjectModel3DIntersectionl);
                            }

                            ho_Intersectionl.Dispose(); hv_resultcon.Dispose();
                            project_object_model_3d_lines_to_contour_xld(out ho_Intersectionl, hv_PoseL1,
                                hv_ObjectModel3DIntersectionl, out hv_resultcon);
                            if ((int)(new HTuple(hv_resultcon.TupleEqual(1))) != 0)
                            {
                                hv_numbercount.Dispose();
                                HOperatorSet.ContourPointNumXld(ho_Intersectionl, out hv_numbercount);
                                hv_lengthnum.Dispose();
                                HOperatorSet.TupleLength(hv_numbercount, out hv_lengthnum);

                                if ((int)(new HTuple(hv_lengthnum.TupleGreater(0))) != 0)
                                {
                                    ho_UnionContoursl.Dispose();
                                    HOperatorSet.UnionAdjacentContoursXld(ho_Intersectionl, out ho_UnionContoursl,
                                        100, 1, "attr_keep");

                                    hv_Lengthl.Dispose();
                                    HOperatorSet.LengthXld(ho_UnionContoursl, out hv_Lengthl);
                                    hv_Maxl.Dispose();
                                    HOperatorSet.TupleMax(hv_Lengthl, out hv_Maxl);
                                    hv_Indices.Dispose();
                                    HOperatorSet.TupleFind(hv_Lengthl, hv_Maxl, out hv_Indices);
                                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                    {
                                        ho_ObjectSelectedl.Dispose();
                                        HOperatorSet.SelectObj(ho_UnionContoursl, out ho_ObjectSelectedl, hv_Indices + 1);
                                    }
                                    ho_ContoursSplitl.Dispose();
                                    HOperatorSet.SegmentContoursXld(ho_ObjectSelectedl, out ho_ContoursSplitl,
                                        "lines", 3, 2, 2);
                                    ho_SelectedContoursl.Dispose();
                                    HOperatorSet.SelectContoursXld(ho_ContoursSplitl, out ho_SelectedContoursl,
                                        "contour_length", 2, 2999, -0.5, 0.5);

                                    hv_Numbercl.Dispose();
                                    HOperatorSet.CountObj(ho_SelectedContoursl, out hv_Numbercl);

                                    if ((int)(new HTuple(hv_Numbercl.TupleEqual(3))) != 0)
                                    {
                                        hv_LeftIndex.Dispose();
                                        hv_LeftIndex = new HTuple(hv_n);

                                    }
                                }

                            }


                        }

                        if ((int)(new HTuple(hv_RightIndex.TupleEqual(-1))) != 0)
                        {
                            hv_resultcon.Dispose();
                            hv_resultcon = 0;
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ObjectModel3DIntersectionr.Dispose();
                                HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefaultR.TupleSelect(
                                    0), hv_PoseR, out hv_ObjectModel3DIntersectionr);
                            }
                            ho_Intersectionr.Dispose(); hv_resultcon.Dispose();
                            project_object_model_3d_lines_to_contour_xld(out ho_Intersectionr, hv_PoseR1,
                                hv_ObjectModel3DIntersectionr, out hv_resultcon);

                            if ((int)(new HTuple(hv_resultcon.TupleEqual(1))) != 0)
                            {
                                hv_numbercount.Dispose();
                                HOperatorSet.ContourPointNumXld(ho_Intersectionr, out hv_numbercount);
                                hv_lengthnum.Dispose();
                                HOperatorSet.TupleLength(hv_numbercount, out hv_lengthnum);
                                if ((int)(new HTuple(hv_lengthnum.TupleGreater(0))) != 0)
                                {
                                    ho_UnionContoursr.Dispose();
                                    HOperatorSet.UnionAdjacentContoursXld(ho_Intersectionr, out ho_UnionContoursr,
                                        100, 1, "attr_keep");

                                    hv_Lengthr.Dispose();
                                    HOperatorSet.LengthXld(ho_UnionContoursr, out hv_Lengthr);
                                    hv_Maxr.Dispose();
                                    HOperatorSet.TupleMax(hv_Lengthr, out hv_Maxr);
                                    hv_Indices.Dispose();
                                    HOperatorSet.TupleFind(hv_Lengthr, hv_Maxr, out hv_Indices);
                                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                    {
                                        ho_ObjectSelectedr.Dispose();
                                        HOperatorSet.SelectObj(ho_UnionContoursr, out ho_ObjectSelectedr, hv_Indices + 1);
                                    }
                                    ho_ContoursSplitr.Dispose();
                                    HOperatorSet.SegmentContoursXld(ho_ObjectSelectedr, out ho_ContoursSplitr,
                                        "lines", 3, 2, 2);
                                    ho_SelectedContoursr.Dispose();
                                    HOperatorSet.SelectContoursXld(ho_ContoursSplitr, out ho_SelectedContoursr,
                                        "contour_length", 2, 2999, -0.5, 0.5);

                                    hv_Numbercr.Dispose();
                                    HOperatorSet.CountObj(ho_SelectedContoursr, out hv_Numbercr);
                                    if ((int)(new HTuple(hv_Numbercr.TupleEqual(3))) != 0)
                                    {
                                        hv_RightIndex.Dispose();
                                        hv_RightIndex = new HTuple(hv_n);
                                    }
                                }

                            }

                        }

                        if ((int)(new HTuple(hv_DownIndex.TupleEqual(-1))) != 0)
                        {
                            hv_resultcon.Dispose();
                            hv_resultcon = 0;
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ObjectModel3DIntersectiond.Dispose();
                                HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefaultD.TupleSelect(
                                    0), hv_PoseD, out hv_ObjectModel3DIntersectiond);
                            }
                            ho_Intersectiond.Dispose(); hv_resultcon.Dispose();
                            project_object_model_3d_lines_to_contour_xld(out ho_Intersectiond, hv_PoseD1,
                                hv_ObjectModel3DIntersectiond, out hv_resultcon);
                            if ((int)(new HTuple(hv_resultcon.TupleEqual(1))) != 0)
                            {
                                hv_numbercount.Dispose();
                                HOperatorSet.ContourPointNumXld(ho_Intersectiond, out hv_numbercount);
                                hv_lengthnum.Dispose();
                                HOperatorSet.TupleLength(hv_numbercount, out hv_lengthnum);
                                if ((int)(new HTuple(hv_lengthnum.TupleGreater(0))) != 0)
                                {
                                    ho_UnionContoursd.Dispose();
                                    HOperatorSet.UnionAdjacentContoursXld(ho_Intersectiond, out ho_UnionContoursd,
                                        100, 1, "attr_keep");


                                    hv_Lengthd.Dispose();
                                    HOperatorSet.LengthXld(ho_UnionContoursd, out hv_Lengthd);
                                    hv_Maxd.Dispose();
                                    HOperatorSet.TupleMax(hv_Lengthd, out hv_Maxd);
                                    hv_Indices.Dispose();
                                    HOperatorSet.TupleFind(hv_Lengthd, hv_Maxd, out hv_Indices);
                                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                    {
                                        ho_ObjectSelectedd.Dispose();
                                        HOperatorSet.SelectObj(ho_UnionContoursd, out ho_ObjectSelectedd, hv_Indices + 1);
                                    }

                                    ho_ContoursSplitd.Dispose();
                                    HOperatorSet.SegmentContoursXld(ho_ObjectSelectedd, out ho_ContoursSplitd,
                                        "lines_circles", 3, 2, 2);
                                    ho_SelectedContoursd.Dispose();
                                    HOperatorSet.SelectContoursXld(ho_ContoursSplitd, out ho_SelectedContoursd,
                                        "contour_length", 2, 2999, -0.5, 0.5);

                                    hv_Numbercd.Dispose();
                                    HOperatorSet.CountObj(ho_SelectedContoursd, out hv_Numbercd);

                                    if ((int)(new HTuple(hv_Numbercd.TupleEqual(3))) != 0)
                                    {
                                        ho_contournewmid.Dispose();
                                        HOperatorSet.SelectObj(ho_SelectedContoursd, out ho_contournewmid,
                                            2);
                                        hv_Row.Dispose(); hv_Col.Dispose();
                                        HOperatorSet.GetContourXld(ho_contournewmid, out hv_Row, out hv_Col);
                                        hv_Colmin.Dispose();
                                        HOperatorSet.TupleMin(hv_Col, out hv_Colmin);
                                        hv_Colmax.Dispose();
                                        HOperatorSet.TupleMax(hv_Col, out hv_Colmax);
                                        if ((int)(new HTuple(((hv_Colmax - hv_Colmin)).TupleLessEqual(16.5))) != 0)
                                        {
                                            hv_DownIndex.Dispose();
                                            hv_DownIndex = new HTuple(hv_n);
                                        }
                                    }
                                }
                            }

                        }

                        if ((int)((new HTuple((new HTuple((new HTuple(hv_TopIndex.TupleNotEqual(-1))).TupleAnd(
                            new HTuple(hv_DownIndex.TupleNotEqual(-1))))).TupleAnd(new HTuple(hv_LeftIndex.TupleNotEqual(
                            -1))))).TupleAnd(new HTuple(hv_RightIndex.TupleNotEqual(-1)))) != 0)
                        {
                            break;
                        }
                    }
                    catch (HalconException HDevExpDefaultException1)
                    {
                        HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    }

                }

                hv_Left3DX.Dispose();
                hv_Left3DX = 0;
                hv_Right3DX.Dispose();
                hv_Right3DX = 0;
                hv_Down3DY.Dispose();
                hv_Down3DY = 0;
                hv_LeftTDistance.Dispose();
                hv_LeftTDistance = 0;
                hv_RightTDistance.Dispose();
                hv_RightTDistance = 0;
                hv_LeftDDistance.Dispose();
                hv_LeftDDistance = 0;
                hv_RightDDistance.Dispose();
                hv_RightDDistance = 0;
                hv_DownDistance.Dispose();
                hv_DownDistance = 0;
                hv_LeftRightDistance.Dispose();
                hv_LeftRightDistance = 0;
                HTuple end_val389 = hv_Height - 1;
                HTuple step_val389 = 1;
                for (hv_n = 10; hv_n.Continue(end_val389, step_val389); hv_n = hv_n.TupleAdd(step_val389))
                {
                    try
                    {
                        if ((int)((new HTuple((new HTuple((new HTuple(((hv_n + hv_TopIndex)).TupleGreaterEqual(
                            hv_Height))).TupleOr(new HTuple(((hv_n + hv_LeftIndex)).TupleGreaterEqual(
                            hv_Height))))).TupleOr(new HTuple(((hv_n + hv_RightIndex)).TupleGreater(
                            hv_Height))))).TupleOr(new HTuple(((hv_n + hv_DownIndex)).TupleGreater(
                            hv_Height)))) != 0)
                        {
                            break;
                        }
                        if (hv_PoseT == null)
                            hv_PoseT = new HTuple();
                        hv_PoseT[1] = (hv_n + hv_TopIndex) * 0.075;
                        if (hv_PoseL == null)
                            hv_PoseL = new HTuple();
                        hv_PoseL[1] = (hv_n + hv_LeftIndex) * 0.075;
                        if (hv_PoseR == null)
                            hv_PoseR = new HTuple();
                        hv_PoseR[1] = (hv_n + hv_RightIndex) * 0.075;
                        if (hv_PoseD == null)
                            hv_PoseD = new HTuple();
                        hv_PoseD[1] = (hv_n + hv_DownIndex) * 0.075;

                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ObjectModel3DIntersectiont.Dispose();
                            HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefaultT.TupleSelect(
                                0), hv_PoseT, out hv_ObjectModel3DIntersectiont);
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ObjectModel3DIntersectionl.Dispose();
                            HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefaultL.TupleSelect(
                                0), hv_PoseL, out hv_ObjectModel3DIntersectionl);
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ObjectModel3DIntersectionr.Dispose();
                            HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefaultR.TupleSelect(
                                0), hv_PoseR, out hv_ObjectModel3DIntersectionr);
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ObjectModel3DIntersectiond.Dispose();
                            HOperatorSet.IntersectPlaneObjectModel3d(hv_Surface3DDefaultD.TupleSelect(
                                0), hv_PoseD, out hv_ObjectModel3DIntersectiond);
                        }

                        hv_PoseT1.Dispose();
                        hv_PoseT1 = new HTuple(hv_PoseT);
                        hv_PoseR1.Dispose();
                        hv_PoseR1 = new HTuple(hv_PoseR);
                        hv_PoseD1.Dispose();
                        hv_PoseD1 = new HTuple(hv_PoseD);
                        hv_PoseL1.Dispose();
                        hv_PoseL1 = new HTuple(hv_PoseL);

                        //gen_plane_object_model_3d (PoseR1, [-1, -1, 1, 1] * VisualizationPlaneSize, [-1, 1, 1, -1] * VisualizationPlaneSize, IntersectionPlane4)
                        //visualize_object_model_3d (WindowHandle, [Surface3DDefaultR,IntersectionPlane4], [], [], ['color_0','color_1'], ['yellow','cyan'], Message, [], Instructions, PoseOut)

                        ho_Intersectiont.Dispose(); hv_resultcon1.Dispose();
                        project_object_model_3d_lines_to_contour_xld(out ho_Intersectiont, hv_PoseT1,
                            hv_ObjectModel3DIntersectiont, out hv_resultcon1);
                        ho_UnionContourst.Dispose();
                        HOperatorSet.UnionAdjacentContoursXld(ho_Intersectiont, out ho_UnionContourst,
                            100, 1, "attr_keep");
                        ho_Intersectionl.Dispose(); hv_resultcon2.Dispose();
                        project_object_model_3d_lines_to_contour_xld(out ho_Intersectionl, hv_PoseL1,
                            hv_ObjectModel3DIntersectionl, out hv_resultcon2);
                        ho_UnionContoursl.Dispose();
                        HOperatorSet.UnionAdjacentContoursXld(ho_Intersectionl, out ho_UnionContoursl,
                            100, 1, "attr_keep");
                        ho_Intersectionr.Dispose(); hv_resultcon3.Dispose();
                        project_object_model_3d_lines_to_contour_xld(out ho_Intersectionr, hv_PoseR1,
                            hv_ObjectModel3DIntersectionr, out hv_resultcon3);
                        ho_UnionContoursr.Dispose();
                        HOperatorSet.UnionAdjacentContoursXld(ho_Intersectionr, out ho_UnionContoursr,
                            100, 1, "attr_keep");
                        ho_Intersectiond.Dispose(); hv_resultcon4.Dispose();
                        project_object_model_3d_lines_to_contour_xld(out ho_Intersectiond, hv_PoseD1,
                            hv_ObjectModel3DIntersectiond, out hv_resultcon4);
                        ho_UnionContoursd.Dispose();
                        HOperatorSet.UnionAdjacentContoursXld(ho_Intersectiond, out ho_UnionContoursd,
                            100, 1, "attr_keep");

                        if ((int)((new HTuple((new HTuple((new HTuple(hv_resultcon1.TupleEqual(
                            0))).TupleOr(new HTuple(hv_resultcon2.TupleEqual(0))))).TupleOr(new HTuple(hv_resultcon3.TupleEqual(
                            0))))).TupleOr(new HTuple(hv_resultcon4.TupleEqual(0)))) != 0)
                        {
                            continue;
                        }
                        hv_Length.Dispose();
                        HOperatorSet.LengthXld(ho_UnionContourst, out hv_Length);
                        hv_Max.Dispose();
                        HOperatorSet.TupleMax(hv_Length, out hv_Max);
                        hv_Indices.Dispose();
                        HOperatorSet.TupleFind(hv_Length, hv_Max, out hv_Indices);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            ho_ObjectSelectedt.Dispose();
                            HOperatorSet.SelectObj(ho_UnionContourst, out ho_ObjectSelectedt, hv_Indices + 1);
                        }

                        hv_Lengthl.Dispose();
                        HOperatorSet.LengthXld(ho_UnionContoursl, out hv_Lengthl);
                        hv_Maxl.Dispose();
                        HOperatorSet.TupleMax(hv_Lengthl, out hv_Maxl);
                        hv_Indices.Dispose();
                        HOperatorSet.TupleFind(hv_Lengthl, hv_Maxl, out hv_Indices);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            ho_ObjectSelectedl.Dispose();
                            HOperatorSet.SelectObj(ho_UnionContoursl, out ho_ObjectSelectedl, hv_Indices + 1);
                        }

                        hv_Lengthr.Dispose();
                        HOperatorSet.LengthXld(ho_UnionContoursr, out hv_Lengthr);
                        hv_Maxr.Dispose();
                        HOperatorSet.TupleMax(hv_Lengthr, out hv_Maxr);
                        hv_Indices.Dispose();
                        HOperatorSet.TupleFind(hv_Lengthr, hv_Maxr, out hv_Indices);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            ho_ObjectSelectedr.Dispose();
                            HOperatorSet.SelectObj(ho_UnionContoursr, out ho_ObjectSelectedr, hv_Indices + 1);
                        }

                        hv_Lengthd.Dispose();
                        HOperatorSet.LengthXld(ho_UnionContoursd, out hv_Lengthd);
                        hv_Maxd.Dispose();
                        HOperatorSet.TupleMax(hv_Lengthd, out hv_Maxd);
                        hv_Indices.Dispose();
                        HOperatorSet.TupleFind(hv_Lengthd, hv_Maxd, out hv_Indices);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            ho_ObjectSelectedd.Dispose();
                            HOperatorSet.SelectObj(ho_UnionContoursd, out ho_ObjectSelectedd, hv_Indices + 1);
                        }


                        ho_ContoursSplitt.Dispose();
                        HOperatorSet.SegmentContoursXld(ho_ObjectSelectedt, out ho_ContoursSplitt,
                            "lines", 3, 2, 2);
                        ho_SelectedContourst.Dispose();
                        HOperatorSet.SelectContoursXld(ho_ContoursSplitt, out ho_SelectedContourst,
                            "contour_length", 0.5, 2999, -0.5, 0.5);

                        ho_ContoursSplitl.Dispose();
                        HOperatorSet.SegmentContoursXld(ho_ObjectSelectedl, out ho_ContoursSplitl,
                            "lines", 3, 2, 2);
                        ho_SelectedContoursl.Dispose();
                        HOperatorSet.SelectContoursXld(ho_ContoursSplitl, out ho_SelectedContoursl,
                            "contour_length", 2, 2999, -0.5, 0.5);

                        ho_ContoursSplitr.Dispose();
                        HOperatorSet.SegmentContoursXld(ho_ObjectSelectedr, out ho_ContoursSplitr,
                            "lines", 3, 2, 2);
                        ho_SelectedContoursr.Dispose();
                        HOperatorSet.SelectContoursXld(ho_ContoursSplitr, out ho_SelectedContoursr,
                            "contour_length", 2, 2999, -0.5, 0.5);


                        ho_ContoursSplitd.Dispose();
                        HOperatorSet.SegmentContoursXld(ho_ObjectSelectedd, out ho_ContoursSplitd,
                            "lines", 3, 2, 2);
                        ho_SelectedContoursd.Dispose();
                        HOperatorSet.SelectContoursXld(ho_ContoursSplitd, out ho_SelectedContoursd,
                            "contour_length", 2, 2999, -0.5, 0.5);

                        hv_Numberct.Dispose();
                        HOperatorSet.CountObj(ho_SelectedContourst, out hv_Numberct);
                        hv_Numbercr.Dispose();
                        HOperatorSet.CountObj(ho_SelectedContoursr, out hv_Numbercr);
                        hv_Numbercl.Dispose();
                        HOperatorSet.CountObj(ho_SelectedContoursl, out hv_Numbercl);
                        hv_Numbercd.Dispose();
                        HOperatorSet.CountObj(ho_SelectedContoursd, out hv_Numbercd);



                        if ((int)((new HTuple((new HTuple((new HTuple(hv_Numbercr.TupleEqual(3))).TupleAnd(
                            new HTuple(hv_Numberct.TupleEqual(3))))).TupleAnd(new HTuple(hv_Numbercd.TupleEqual(
                            3))))).TupleAnd(new HTuple(hv_Numbercl.TupleEqual(3)))) != 0)
                        {
                            ho_contourleft.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContourst, out ho_contourleft, 1);
                            ho_contourmidt.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContourst, out ho_contourmidt, 2);
                            ho_contourrigt.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContourst, out ho_contourrigt, 3);

                            ho_contourlefr.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursr, out ho_contourlefr, 1);
                            ho_contourmidr.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursr, out ho_contourmidr, 2);
                            ho_contourrigr.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursr, out ho_contourrigr, 3);

                            ho_contourlefl.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursl, out ho_contourlefl, 1);
                            ho_contourmidl.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursl, out ho_contourmidl, 2);
                            ho_contourrigl.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursl, out ho_contourrigl, 3);

                            ho_contourlefd.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursd, out ho_contourlefd, 1);
                            ho_contourmidd.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursd, out ho_contourmidd, 2);
                            ho_contourrigd.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContoursd, out ho_contourrigd, 3);




                            //检查上侧3D采集图像，分割线段是否会出现折线
                            hv_Rowsleft.Dispose(); hv_Colsleft.Dispose();
                            HOperatorSet.GetContourXld(ho_contourleft, out hv_Rowsleft, out hv_Colsleft);
                            hv_Rowsrigt.Dispose(); hv_Colsrigt.Dispose();
                            HOperatorSet.GetContourXld(ho_contourrigt, out hv_Rowsrigt, out hv_Colsrigt);
                            hv_Rowsmidt.Dispose(); hv_Colsmidt.Dispose();
                            HOperatorSet.GetContourXld(ho_contourmidt, out hv_Rowsmidt, out hv_Colsmidt);

                            hv_Minleft.Dispose();
                            HOperatorSet.TupleMin(hv_Colsleft, out hv_Minleft);
                            hv_Maxleft.Dispose();
                            HOperatorSet.TupleMax(hv_Colsleft, out hv_Maxleft);

                            hv_Minrigt.Dispose();
                            HOperatorSet.TupleMin(hv_Colsrigt, out hv_Minrigt);
                            hv_Maxrigt.Dispose();
                            HOperatorSet.TupleMax(hv_Colsrigt, out hv_Maxrigt);

                            hv_RowBeginleft.Dispose(); hv_ColBeginleft.Dispose(); hv_RowEndleft.Dispose(); hv_ColEndleft.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourleft, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginleft, out hv_ColBeginleft, out hv_RowEndleft, out hv_ColEndleft,
                                out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                            hv_RowBeginrigt.Dispose(); hv_ColBeginrigt.Dispose(); hv_RowEndrigt.Dispose(); hv_ColEndrigt.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigt.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourrigt, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginrigt, out hv_ColBeginrigt, out hv_RowEndrigt, out hv_ColEndrigt,
                                out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigt);

                            hv_lengthleftcontour.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthleftcontour = hv_Minleft - hv_Maxleft;
                            }
                            hv_lengthleflined.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthleflined = hv_ColEndleft - hv_ColBeginleft;
                            }
                            hv_Abslengthleftcontour.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthleftcontour, out hv_Abslengthleftcontour);
                            hv_Abslengthleflined.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthleflined, out hv_Abslengthleflined);
                            if ((int)(new HTuple(((hv_Abslengthleftcontour - hv_Abslengthleflined)).TupleGreater(
                                0.3))) != 0)
                            {
                                hv_countofCols.Dispose();
                                HOperatorSet.TupleLength(hv_Colsleft, out hv_countofCols);
                                HTuple end_val505 = 2;
                                HTuple step_val505 = -1;
                                for (hv_Index = hv_countofCols - 1; hv_Index.Continue(end_val505, step_val505); hv_Index = hv_Index.TupleAdd(step_val505))
                                {
                                    if ((int)((new HTuple((((hv_Rowsleft.TupleSelect(hv_Index)) - (hv_Rowsleft.TupleSelect(
                                        hv_Index - 1)))).TupleLess(-0.02))).TupleAnd(new HTuple((((hv_Rowsleft.TupleSelect(
                                        hv_Index - 1)) - (hv_Rowsleft.TupleSelect(hv_Index - 2)))).TupleLess(
                                        -0.02)))) != 0)
                                    {
                                        break;
                                    }
                                }
                                hv_Rowsnewleft.Dispose();
                                hv_Rowsnewleft = new HTuple();
                                hv_Colsnewleft.Dispose();
                                hv_Colsnewleft = new HTuple();
                                if ((int)(new HTuple(hv_Index.TupleGreater(2))) != 0)
                                {
                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val514 = hv_Index;
                                    HTuple step_val514 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val514, step_val514); hv_Index1 = hv_Index1.TupleAdd(step_val514))
                                    {
                                        if (hv_Rowsnewleft == null)
                                            hv_Rowsnewleft = new HTuple();
                                        hv_Rowsnewleft[hv_Indexx] = hv_Rowsleft.TupleSelect(hv_Index1);
                                        if (hv_Colsnewleft == null)
                                            hv_Colsnewleft = new HTuple();
                                        hv_Colsnewleft[hv_Indexx] = hv_Colsleft.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val521 = hv_countofCols - 1;
                                    HTuple step_val521 = 1;
                                    for (hv_Index1 = hv_Index; hv_Index1.Continue(end_val521, step_val521); hv_Index1 = hv_Index1.TupleAdd(step_val521))
                                    {
                                        if (hv_Rowsnewmidt == null)
                                            hv_Rowsnewmidt = new HTuple();
                                        hv_Rowsnewmidt[hv_Indexx] = hv_Rowsleft.TupleSelect(hv_Index1);
                                        if (hv_Colsnewmidt == null)
                                            hv_Colsnewmidt = new HTuple();
                                        hv_Colsnewmidt[hv_Indexx] = hv_Colsleft.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_countofRowst.Dispose();
                                    HOperatorSet.TupleLength(hv_Rowsmidt, out hv_countofRowst);
                                    HTuple end_val528 = hv_countofRowst - 1;
                                    HTuple step_val528 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val528, step_val528); hv_Index1 = hv_Index1.TupleAdd(step_val528))
                                    {
                                        if (hv_Rowsnewmidt == null)
                                            hv_Rowsnewmidt = new HTuple();
                                        hv_Rowsnewmidt[hv_Indexx] = hv_Rowsmidt.TupleSelect(hv_Index1);
                                        if (hv_Colsnewmidt == null)
                                            hv_Colsnewmidt = new HTuple();
                                        hv_Colsnewmidt[hv_Indexx] = hv_Colsmidt.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    ho_contourleft.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourleft, hv_Rowsnewleft,
                                        hv_Colsnewleft);
                                    ho_contourmidt.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourmidt, hv_Rowsnewmidt,
                                        hv_Colsnewmidt);
                                    hv_RowBeginmidt.Dispose(); hv_ColBeginmidt.Dispose(); hv_RowEndmidt.Dispose(); hv_ColEndmidt.Dispose(); hv_Nrmidt.Dispose(); hv_Ncmidt.Dispose(); hv_Distmidt.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourmidt, "drop", -1, 5, 5,
                                        1, out hv_RowBeginmidt, out hv_ColBeginmidt, out hv_RowEndmidt,
                                        out hv_ColEndmidt, out hv_Nrmidt, out hv_Ncmidt, out hv_Distmidt);
                                    hv_RowBeginleft.Dispose(); hv_ColBeginleft.Dispose(); hv_RowEndleft.Dispose(); hv_ColEndleft.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourleft, "drop", -1, 5, 5,
                                        1, out hv_RowBeginleft, out hv_ColBeginleft, out hv_RowEndleft,
                                        out hv_ColEndleft, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);

                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContourst, ho_contourleft, out ExpTmpOutVar_0,
                                            1);
                                        ho_SelectedContourst.Dispose();
                                        ho_SelectedContourst = ExpTmpOutVar_0;
                                    }
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContourst, ho_contourmidt, out ExpTmpOutVar_0,
                                            2);
                                        ho_SelectedContourst.Dispose();
                                        ho_SelectedContourst = ExpTmpOutVar_0;
                                    }

                                }

                            }


                            hv_lengthrigtcontour.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthrigtcontour = hv_Maxrigt - hv_Minrigt;
                            }
                            hv_lengthriglined.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthriglined = hv_ColEndrigt - hv_ColBeginrigt;
                            }
                            hv_Abslengthrigtcontour.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthrigtcontour, out hv_Abslengthrigtcontour);
                            hv_Abslengthriglined.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthriglined, out hv_Abslengthriglined);

                            if ((int)(new HTuple(((hv_Abslengthrigtcontour - hv_Abslengthriglined)).TupleGreater(
                                0.1))) != 0)
                            {
                                hv_countofCols.Dispose();
                                HOperatorSet.TupleLength(hv_Rowsrigt, out hv_countofCols);
                                HTuple end_val554 = hv_countofCols - 1;
                                HTuple step_val554 = 1;
                                for (hv_Index = 1; hv_Index.Continue(end_val554, step_val554); hv_Index = hv_Index.TupleAdd(step_val554))
                                {
                                    if ((int)(new HTuple((((hv_Rowsrigt.TupleSelect(hv_Index)) - (hv_Rowsrigt.TupleSelect(
                                        hv_Index + 1)))).TupleLess(-0.02))) != 0)
                                    {
                                        break;
                                    }
                                }

                                hv_Rowsnewrigt.Dispose();
                                hv_Rowsnewrigt = new HTuple();
                                hv_Colsnewrigt.Dispose();
                                hv_Colsnewrigt = new HTuple();
                                if ((int)(new HTuple(hv_Index.TupleNotEqual(hv_countofCols - 1))) != 0)
                                {
                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val564 = hv_countofCols - 1;
                                    HTuple step_val564 = 1;
                                    for (hv_Index1 = hv_Index; hv_Index1.Continue(end_val564, step_val564); hv_Index1 = hv_Index1.TupleAdd(step_val564))
                                    {
                                        if (hv_Rowsnewrigt == null)
                                            hv_Rowsnewrigt = new HTuple();
                                        hv_Rowsnewrigt[hv_Indexx] = hv_Rowsrigt.TupleSelect(hv_Index1);
                                        if (hv_Colsnewrigt == null)
                                            hv_Colsnewrigt = new HTuple();
                                        hv_Colsnewrigt[hv_Indexx] = hv_Colsrigt.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_countofRowst.Dispose();
                                    HOperatorSet.TupleLength(hv_Rowsmidt, out hv_countofRowst);
                                    hv_Indexx.Dispose();
                                    hv_Indexx = new HTuple(hv_countofRowst);
                                    HTuple end_val572 = hv_Index - 1;
                                    HTuple step_val572 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val572, step_val572); hv_Index1 = hv_Index1.TupleAdd(step_val572))
                                    {
                                        if (hv_Rowsmidt == null)
                                            hv_Rowsmidt = new HTuple();
                                        hv_Rowsmidt[hv_Indexx] = hv_Rowsrigt.TupleSelect(hv_Index1);
                                        if (hv_Colsmidt == null)
                                            hv_Colsmidt = new HTuple();
                                        hv_Colsmidt[hv_Indexx] = hv_Colsrigt.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    ho_contourmidt.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourmidt, hv_Rowsmidt,
                                        hv_Colsmidt);
                                    ho_contourrigt.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourrigt, hv_Rowsnewrigt,
                                        hv_Colsnewrigt);
                                    hv_RowBeginmidt.Dispose(); hv_ColBeginmidt.Dispose(); hv_RowEndmidt.Dispose(); hv_ColEndmidt.Dispose(); hv_Nrmidt.Dispose(); hv_Ncmidt.Dispose(); hv_Distmidt.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourmidt, "drop", -1, 5, 5,
                                        1, out hv_RowBeginmidt, out hv_ColBeginmidt, out hv_RowEndmidt,
                                        out hv_ColEndmidt, out hv_Nrmidt, out hv_Ncmidt, out hv_Distmidt);
                                    hv_RowBeginrigt.Dispose(); hv_ColBeginrigt.Dispose(); hv_RowEndrigt.Dispose(); hv_ColEndrigt.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigt.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourrigt, "drop", -1, 5, 5,
                                        1, out hv_RowBeginrigt, out hv_ColBeginrigt, out hv_RowEndrigt,
                                        out hv_ColEndrigt, out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigt);

                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContourst, ho_contourmidt, out ExpTmpOutVar_0,
                                            2);
                                        ho_SelectedContourst.Dispose();
                                        ho_SelectedContourst = ExpTmpOutVar_0;
                                    }
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContourst, ho_contourrigt, out ExpTmpOutVar_0,
                                            3);
                                        ho_SelectedContourst.Dispose();
                                        ho_SelectedContourst = ExpTmpOutVar_0;
                                    }
                                }
                            }




                            //检查下侧3D采集图像，分割线段是否会出现折线
                            hv_Rowslefd.Dispose(); hv_Colslefd.Dispose();
                            HOperatorSet.GetContourXld(ho_contourlefd, out hv_Rowslefd, out hv_Colslefd);
                            hv_Rowsrigd.Dispose(); hv_Colsrigd.Dispose();
                            HOperatorSet.GetContourXld(ho_contourrigd, out hv_Rowsrigd, out hv_Colsrigd);
                            hv_Rowsmidd.Dispose(); hv_Colsmidd.Dispose();
                            HOperatorSet.GetContourXld(ho_contourmidd, out hv_Rowsmidd, out hv_Colsmidd);


                            hv_Minlefd.Dispose();
                            HOperatorSet.TupleMin(hv_Colslefd, out hv_Minlefd);
                            hv_Maxlefd.Dispose();
                            HOperatorSet.TupleMax(hv_Colslefd, out hv_Maxlefd);

                            hv_Minrigd.Dispose();
                            HOperatorSet.TupleMin(hv_Colsrigd, out hv_Minrigd);
                            hv_Maxrigd.Dispose();
                            HOperatorSet.TupleMax(hv_Colsrigd, out hv_Maxrigd);

                            hv_RowBeginlefd.Dispose(); hv_ColBeginlefd.Dispose(); hv_RowEndlefd.Dispose(); hv_ColEndlefd.Dispose(); hv_Nrlefd.Dispose(); hv_Nclefd.Dispose(); hv_Distlefd.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourlefd, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginlefd, out hv_ColBeginlefd, out hv_RowEndlefd, out hv_ColEndlefd,
                                out hv_Nrlefd, out hv_Nclefd, out hv_Distlefd);
                            hv_RowBeginrigd.Dispose(); hv_ColBeginrigd.Dispose(); hv_RowEndrigd.Dispose(); hv_ColEndrigd.Dispose(); hv_Nrrigd.Dispose(); hv_Ncrigd.Dispose(); hv_Distrigd.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourrigd, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginrigd, out hv_ColBeginrigd, out hv_RowEndrigd, out hv_ColEndrigd,
                                out hv_Nrrigd, out hv_Ncrigd, out hv_Distrigd);


                            hv_lengthlefdcontour.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthlefdcontour = hv_Maxlefd - hv_Minlefd;
                            }
                            hv_lengthleflined.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthleflined = hv_ColEndlefd - hv_ColBeginlefd;
                            }
                            hv_Abslengthlefdcontour.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthlefdcontour, out hv_Abslengthlefdcontour);
                            hv_Abslengthleflined.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthleflined, out hv_Abslengthleflined);
                            if ((int)(new HTuple(((hv_Abslengthlefdcontour - hv_Abslengthleflined)).TupleGreater(
                                0.3))) != 0)
                            {
                                hv_countofCols.Dispose();
                                HOperatorSet.TupleLength(hv_Colslefd, out hv_countofCols);
                                HTuple end_val613 = 2;
                                HTuple step_val613 = -1;
                                for (hv_Index = hv_countofCols - 1; hv_Index.Continue(end_val613, step_val613); hv_Index = hv_Index.TupleAdd(step_val613))
                                {
                                    if ((int)((new HTuple((((hv_Rowslefd.TupleSelect(hv_Index)) - (hv_Rowslefd.TupleSelect(
                                        hv_Index - 1)))).TupleLess(-0.02))).TupleAnd(new HTuple((((hv_Rowslefd.TupleSelect(
                                        hv_Index - 1)) - (hv_Rowslefd.TupleSelect(hv_Index - 2)))).TupleLess(
                                        -0.02)))) != 0)
                                    {
                                        break;
                                    }
                                }

                                hv_Rowsnewlefd.Dispose();
                                hv_Rowsnewlefd = new HTuple();
                                hv_Colsnewlefd.Dispose();
                                hv_Colsnewlefd = new HTuple();
                                if ((int)(new HTuple(hv_Index.TupleGreater(2))) != 0)
                                {
                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val623 = hv_Index;
                                    HTuple step_val623 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val623, step_val623); hv_Index1 = hv_Index1.TupleAdd(step_val623))
                                    {
                                        if (hv_Rowsnewlefd == null)
                                            hv_Rowsnewlefd = new HTuple();
                                        hv_Rowsnewlefd[hv_Indexx] = hv_Rowslefd.TupleSelect(hv_Index1);
                                        if (hv_Colsnewlefd == null)
                                            hv_Colsnewlefd = new HTuple();
                                        hv_Colsnewlefd[hv_Indexx] = hv_Colslefd.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }


                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val631 = hv_countofCols - 1;
                                    HTuple step_val631 = 1;
                                    for (hv_Index1 = hv_Index; hv_Index1.Continue(end_val631, step_val631); hv_Index1 = hv_Index1.TupleAdd(step_val631))
                                    {
                                        if (hv_Rowsnewmidd == null)
                                            hv_Rowsnewmidd = new HTuple();
                                        hv_Rowsnewmidd[hv_Indexx] = hv_Rowslefd.TupleSelect(hv_Index1);
                                        if (hv_Colsnewmidd == null)
                                            hv_Colsnewmidd = new HTuple();
                                        hv_Colsnewmidd[hv_Indexx] = hv_Colslefd.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_countofRowsd.Dispose();
                                    HOperatorSet.TupleLength(hv_Rowsmidd, out hv_countofRowsd);
                                    HTuple end_val638 = hv_countofRowsd - 1;
                                    HTuple step_val638 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val638, step_val638); hv_Index1 = hv_Index1.TupleAdd(step_val638))
                                    {
                                        if (hv_Rowsnewmidd == null)
                                            hv_Rowsnewmidd = new HTuple();
                                        hv_Rowsnewmidd[hv_Indexx] = hv_Rowsmidd.TupleSelect(hv_Index1);
                                        if (hv_Colsnewmidd == null)
                                            hv_Colsnewmidd = new HTuple();
                                        hv_Colsnewmidd[hv_Indexx] = hv_Colsmidd.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    ho_contourlefd.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourlefd, hv_Rowsnewlefd,
                                        hv_Colsnewlefd);
                                    ho_contourmidd.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourmidd, hv_Rowsnewmidd,
                                        hv_Colsnewmidd);
                                    hv_RowBeginmidd.Dispose(); hv_ColBeginmidd.Dispose(); hv_RowEndmidd.Dispose(); hv_ColEndmidd.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourmidd, "drop", -1, 5, 5,
                                        1, out hv_RowBeginmidd, out hv_ColBeginmidd, out hv_RowEndmidd,
                                        out hv_ColEndmidd, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                    hv_RowBeginlefd.Dispose(); hv_ColBeginlefd.Dispose(); hv_RowEndlefd.Dispose(); hv_ColEndlefd.Dispose(); hv_Nrlefd.Dispose(); hv_Nclefd.Dispose(); hv_Distlefd.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourlefd, "drop", -1, 5, 5,
                                        1, out hv_RowBeginlefd, out hv_ColBeginlefd, out hv_RowEndlefd,
                                        out hv_ColEndlefd, out hv_Nrlefd, out hv_Nclefd, out hv_Distlefd);

                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursd, ho_contourlefd, out ExpTmpOutVar_0,
                                            1);
                                        ho_SelectedContoursd.Dispose();
                                        ho_SelectedContoursd = ExpTmpOutVar_0;
                                    }
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursd, ho_contourmidd, out ExpTmpOutVar_0,
                                            2);
                                        ho_SelectedContoursd.Dispose();
                                        ho_SelectedContoursd = ExpTmpOutVar_0;
                                    }

                                }

                            }


                            hv_lengthrigdcontour.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthrigdcontour = hv_Maxrigd - hv_Minrigd;
                            }
                            hv_lengthriglined.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthriglined = hv_ColEndrigd - hv_ColBeginrigd;
                            }
                            hv_Abslengthrigdcontour.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthrigdcontour, out hv_Abslengthrigdcontour);
                            hv_Abslengthriglined.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthriglined, out hv_Abslengthriglined);
                            if ((int)(new HTuple(((hv_Abslengthrigdcontour - hv_Abslengthriglined)).TupleGreater(
                                0.3))) != 0)
                            {
                                hv_countofCols.Dispose();
                                HOperatorSet.TupleLength(hv_Colsrigd, out hv_countofCols);
                                HTuple end_val663 = hv_countofCols - 1;
                                HTuple step_val663 = 1;
                                for (hv_Index = 1; hv_Index.Continue(end_val663, step_val663); hv_Index = hv_Index.TupleAdd(step_val663))
                                {
                                    if ((int)((new HTuple((((hv_Rowsrigd.TupleSelect(hv_Index)) - (hv_Rowsrigd.TupleSelect(
                                        hv_Index + 1)))).TupleLess(-0.02))).TupleAnd(new HTuple((((hv_Rowsrigd.TupleSelect(
                                        hv_Index + 1)) - (hv_Rowsrigd.TupleSelect(hv_Index + 2)))).TupleLess(
                                        -0.02)))) != 0)
                                    {
                                        break;
                                    }
                                }

                                hv_Rowsnewrigd.Dispose();
                                hv_Rowsnewrigd = new HTuple();
                                hv_Colsnewrigd.Dispose();
                                hv_Colsnewrigd = new HTuple();
                                if ((int)(new HTuple(hv_Index.TupleNotEqual(hv_countofCols - 1))) != 0)
                                {
                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val673 = hv_countofCols - 1;
                                    HTuple step_val673 = 1;
                                    for (hv_Index1 = hv_Index; hv_Index1.Continue(end_val673, step_val673); hv_Index1 = hv_Index1.TupleAdd(step_val673))
                                    {
                                        if (hv_Rowsnewrigd == null)
                                            hv_Rowsnewrigd = new HTuple();
                                        hv_Rowsnewrigd[hv_Indexx] = hv_Rowsrigd.TupleSelect(hv_Index1);
                                        if (hv_Colsnewrigd == null)
                                            hv_Colsnewrigd = new HTuple();
                                        hv_Colsnewrigd[hv_Indexx] = hv_Colsrigd.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_countofRowsd.Dispose();
                                    HOperatorSet.TupleLength(hv_Rowsmidd, out hv_countofRowsd);
                                    hv_Indexx.Dispose();
                                    hv_Indexx = new HTuple(hv_countofRowsd);
                                    HTuple end_val681 = hv_Index - 1;
                                    HTuple step_val681 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val681, step_val681); hv_Index1 = hv_Index1.TupleAdd(step_val681))
                                    {
                                        if (hv_Rowsmidd == null)
                                            hv_Rowsmidd = new HTuple();
                                        hv_Rowsmidd[hv_Indexx] = hv_Rowsrigd.TupleSelect(hv_Index1);
                                        if (hv_Colsmidd == null)
                                            hv_Colsmidd = new HTuple();
                                        hv_Colsmidd[hv_Indexx] = hv_Colsrigd.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    ho_contourmidd.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourmidd, hv_Rowsmidd,
                                        hv_Colsmidd);
                                    ho_contourrigd.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourrigd, hv_Rowsnewrigd,
                                        hv_Colsnewrigd);
                                    hv_RowBeginmidd.Dispose(); hv_ColBeginmidd.Dispose(); hv_RowEndmidd.Dispose(); hv_ColEndmidd.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourmidd, "drop", -1, 5, 5,
                                        1, out hv_RowBeginmidd, out hv_ColBeginmidd, out hv_RowEndmidd,
                                        out hv_ColEndmidd, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                    hv_RowBeginrigd.Dispose(); hv_ColBeginrigd.Dispose(); hv_RowEndrigd.Dispose(); hv_ColEndrigd.Dispose(); hv_Nrrigd.Dispose(); hv_Ncrigd.Dispose(); hv_Distrigd.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourrigd, "drop", -1, 5, 5,
                                        1, out hv_RowBeginrigd, out hv_ColBeginrigd, out hv_RowEndrigd,
                                        out hv_ColEndrigd, out hv_Nrrigd, out hv_Ncrigd, out hv_Distrigd);
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursd, ho_contourmidd, out ExpTmpOutVar_0,
                                            2);
                                        ho_SelectedContoursd.Dispose();
                                        ho_SelectedContoursd = ExpTmpOutVar_0;
                                    }
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursd, ho_contourrigd, out ExpTmpOutVar_0,
                                            3);
                                        ho_SelectedContoursd.Dispose();
                                        ho_SelectedContoursd = ExpTmpOutVar_0;
                                    }
                                }

                            }


                            //检查左侧3D采集图像，分割线段是否会出现折线
                            hv_Rowslefl.Dispose(); hv_Colslefl.Dispose();
                            HOperatorSet.GetContourXld(ho_contourlefl, out hv_Rowslefl, out hv_Colslefl);
                            hv_Rowsrigl.Dispose(); hv_Colsrigl.Dispose();
                            HOperatorSet.GetContourXld(ho_contourrigl, out hv_Rowsrigl, out hv_Colsrigl);
                            hv_Rowsmidl.Dispose(); hv_Colsmidl.Dispose();
                            HOperatorSet.GetContourXld(ho_contourmidl, out hv_Rowsmidl, out hv_Colsmidl);

                            hv_Minlefl.Dispose();
                            HOperatorSet.TupleMin(hv_Colslefl, out hv_Minlefl);
                            hv_Maxlefl.Dispose();
                            HOperatorSet.TupleMax(hv_Colslefl, out hv_Maxlefl);

                            hv_Minrigl.Dispose();
                            HOperatorSet.TupleMin(hv_Colsrigl, out hv_Minrigl);
                            hv_Maxrigl.Dispose();
                            HOperatorSet.TupleMax(hv_Colsrigl, out hv_Maxrigl);

                            hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrlefl.Dispose(); hv_Nclefl.Dispose(); hv_Distlefl.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourlefl, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl, out hv_ColEndlefl,
                                out hv_Nrlefl, out hv_Nclefl, out hv_Distlefl);
                            hv_RowBeginrigl.Dispose(); hv_ColBeginrigl.Dispose(); hv_RowEndrigl.Dispose(); hv_ColEndrigl.Dispose(); hv_Nrrigl.Dispose(); hv_Ncrigl.Dispose(); hv_Distrigl.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourrigl, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginrigl, out hv_ColBeginrigl, out hv_RowEndrigl, out hv_ColEndrigl,
                                out hv_Nrrigl, out hv_Ncrigl, out hv_Distrigl);

                            hv_lengthleflcontour.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthleflcontour = hv_Maxlefl - hv_Minlefl;
                            }
                            hv_lengthleflined.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthleflined = hv_ColEndlefl - hv_ColBeginlefl;
                            }
                            hv_Abslengthleflcontour.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthleflcontour, out hv_Abslengthleflcontour);
                            hv_Abslengthleflined.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthleflined, out hv_Abslengthleflined);
                            if ((int)(new HTuple(((hv_Abslengthleflcontour - hv_Abslengthleflined)).TupleGreater(
                                0.3))) != 0)
                            {
                                hv_countofCols.Dispose();
                                HOperatorSet.TupleLength(hv_Colslefl, out hv_countofCols);
                                HTuple end_val718 = 2;
                                HTuple step_val718 = -1;
                                for (hv_Index = hv_countofCols - 1; hv_Index.Continue(end_val718, step_val718); hv_Index = hv_Index.TupleAdd(step_val718))
                                {
                                    if ((int)((new HTuple((((hv_Rowslefl.TupleSelect(hv_Index)) - (hv_Rowslefl.TupleSelect(
                                        hv_Index - 1)))).TupleLess(-0.02))).TupleAnd(new HTuple((((hv_Rowslefl.TupleSelect(
                                        hv_Index - 1)) - (hv_Rowslefl.TupleSelect(hv_Index - 2)))).TupleLess(
                                        -0.02)))) != 0)
                                    {
                                        break;
                                    }
                                }

                                hv_Rowsnewlefl.Dispose();
                                hv_Rowsnewlefl = new HTuple();
                                hv_Colsnewlefl.Dispose();
                                hv_Colsnewlefl = new HTuple();
                                if ((int)(new HTuple(hv_Index.TupleGreater(2))) != 0)
                                {
                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val728 = hv_Index;
                                    HTuple step_val728 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val728, step_val728); hv_Index1 = hv_Index1.TupleAdd(step_val728))
                                    {
                                        if (hv_Rowsnewlefl == null)
                                            hv_Rowsnewlefl = new HTuple();
                                        hv_Rowsnewlefl[hv_Indexx] = hv_Rowslefl.TupleSelect(hv_Index1);
                                        if (hv_Colsnewlefl == null)
                                            hv_Colsnewlefl = new HTuple();
                                        hv_Colsnewlefl[hv_Indexx] = hv_Colslefl.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }



                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val737 = hv_countofCols - 1;
                                    HTuple step_val737 = 1;
                                    for (hv_Index1 = hv_Index; hv_Index1.Continue(end_val737, step_val737); hv_Index1 = hv_Index1.TupleAdd(step_val737))
                                    {
                                        if (hv_Rowsnewmidl == null)
                                            hv_Rowsnewmidl = new HTuple();
                                        hv_Rowsnewmidl[hv_Indexx] = hv_Rowslefl.TupleSelect(hv_Index1);
                                        if (hv_Colsnewmidl == null)
                                            hv_Colsnewmidl = new HTuple();
                                        hv_Colsnewmidl[hv_Indexx] = hv_Colslefl.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_countofRowsl.Dispose();
                                    HOperatorSet.TupleLength(hv_Rowsmidl, out hv_countofRowsl);
                                    HTuple end_val744 = hv_countofRowsl - 1;
                                    HTuple step_val744 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val744, step_val744); hv_Index1 = hv_Index1.TupleAdd(step_val744))
                                    {
                                        if (hv_Rowsnewmidl == null)
                                            hv_Rowsnewmidl = new HTuple();
                                        hv_Rowsnewmidl[hv_Indexx] = hv_Rowsmidl.TupleSelect(hv_Index1);
                                        if (hv_Colsnewmidl == null)
                                            hv_Colsnewmidl = new HTuple();
                                        hv_Colsnewmidl[hv_Indexx] = hv_Colsmidl.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    ho_contourlefl.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourlefl, hv_Rowsnewlefl,
                                        hv_Colsnewlefl);
                                    ho_contourmidl.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourmidl, hv_Rowsnewmidl,
                                        hv_Colsnewmidl);
                                    hv_RowBeginmidd.Dispose(); hv_ColBeginmidd.Dispose(); hv_RowEndmidd.Dispose(); hv_ColEndmidd.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourmidl, "drop", -1, 5, 5,
                                        1, out hv_RowBeginmidd, out hv_ColBeginmidd, out hv_RowEndmidd,
                                        out hv_ColEndmidd, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                    hv_RowBeginlefd.Dispose(); hv_ColBeginlefd.Dispose(); hv_RowEndlefd.Dispose(); hv_ColEndlefd.Dispose(); hv_Nrlefd.Dispose(); hv_Nclefd.Dispose(); hv_Distlefd.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourlefl, "drop", -1, 5, 5,
                                        1, out hv_RowBeginlefd, out hv_ColBeginlefd, out hv_RowEndlefd,
                                        out hv_ColEndlefd, out hv_Nrlefd, out hv_Nclefd, out hv_Distlefd);
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursl, ho_contourlefl, out ExpTmpOutVar_0,
                                            1);
                                        ho_SelectedContoursl.Dispose();
                                        ho_SelectedContoursl = ExpTmpOutVar_0;
                                    }
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursl, ho_contourmidl, out ExpTmpOutVar_0,
                                            2);
                                        ho_SelectedContoursl.Dispose();
                                        ho_SelectedContoursl = ExpTmpOutVar_0;
                                    }
                                }

                            }


                            hv_lengthrigdcontour.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthrigdcontour = hv_Maxrigl - hv_Minrigl;
                            }
                            hv_lengthriglined.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthriglined = hv_ColEndrigl - hv_ColBeginrigl;
                            }
                            hv_Abslengthrigdcontour.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthrigdcontour, out hv_Abslengthrigdcontour);
                            hv_Abslengthriglined.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthriglined, out hv_Abslengthriglined);

                            if ((int)(new HTuple(((hv_Abslengthrigdcontour - hv_Abslengthriglined)).TupleGreater(
                                0.5))) != 0)
                            {
                                hv_countofCols.Dispose();
                                HOperatorSet.TupleLength(hv_Rowsrigl, out hv_countofCols);
                                HTuple end_val768 = hv_countofCols - 1;
                                HTuple step_val768 = 1;
                                for (hv_Index = 1; hv_Index.Continue(end_val768, step_val768); hv_Index = hv_Index.TupleAdd(step_val768))
                                {
                                    if ((int)(new HTuple((((hv_Rowsrigl.TupleSelect(hv_Index)) - (hv_Rowsrigl.TupleSelect(
                                        hv_Index - 1)))).TupleLess(-0.02))) != 0)
                                    {
                                        break;
                                    }
                                }

                                hv_Rowsnewrigl.Dispose();
                                hv_Rowsnewrigl = new HTuple();
                                hv_Colsnewrigl.Dispose();
                                hv_Colsnewrigl = new HTuple();
                                if ((int)(new HTuple(hv_Index.TupleNotEqual(hv_countofCols))) != 0)
                                {
                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val778 = hv_countofCols - 1;
                                    HTuple step_val778 = 1;
                                    for (hv_Index1 = hv_Index; hv_Index1.Continue(end_val778, step_val778); hv_Index1 = hv_Index1.TupleAdd(step_val778))
                                    {
                                        if (hv_Rowsnewrigl == null)
                                            hv_Rowsnewrigl = new HTuple();
                                        hv_Rowsnewrigl[hv_Indexx] = hv_Rowsrigl.TupleSelect(hv_Index1);
                                        if (hv_Colsnewrigl == null)
                                            hv_Colsnewrigl = new HTuple();
                                        hv_Colsnewrigl[hv_Indexx] = hv_Colsrigl.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_countofRowsl.Dispose();
                                    HOperatorSet.TupleLength(hv_Rowsmidl, out hv_countofRowsl);
                                    hv_Indexx.Dispose();
                                    hv_Indexx = new HTuple(hv_countofRowsl);
                                    HTuple end_val786 = hv_Index - 1;
                                    HTuple step_val786 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val786, step_val786); hv_Index1 = hv_Index1.TupleAdd(step_val786))
                                    {
                                        if (hv_Rowsmidl == null)
                                            hv_Rowsmidl = new HTuple();
                                        hv_Rowsmidl[hv_Indexx] = hv_Rowsrigl.TupleSelect(hv_Index1);
                                        if (hv_Colsmidl == null)
                                            hv_Colsmidl = new HTuple();
                                        hv_Colsmidl[hv_Indexx] = hv_Colsrigl.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    ho_contourmidl.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourmidl, hv_Rowsmidl,
                                        hv_Colsmidl);
                                    ho_contourrigl.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourrigl, hv_Rowsnewrigl,
                                        hv_Colsnewrigl);
                                    hv_RowBeginmidl.Dispose(); hv_ColBeginmidl.Dispose(); hv_RowEndmidl.Dispose(); hv_ColEndmidl.Dispose(); hv_Nrmidl.Dispose(); hv_Ncmidl.Dispose(); hv_Distmidl.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourmidl, "drop", -1, 5, 5,
                                        1, out hv_RowBeginmidl, out hv_ColBeginmidl, out hv_RowEndmidl,
                                        out hv_ColEndmidl, out hv_Nrmidl, out hv_Ncmidl, out hv_Distmidl);
                                    hv_RowBeginrigl.Dispose(); hv_ColBeginrigl.Dispose(); hv_RowEndrigl.Dispose(); hv_ColEndrigl.Dispose(); hv_Nrrigl.Dispose(); hv_Ncrigl.Dispose(); hv_Distrigl.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourrigl, "drop", -1, 5, 5,
                                        1, out hv_RowBeginrigl, out hv_ColBeginrigl, out hv_RowEndrigl,
                                        out hv_ColEndrigl, out hv_Nrrigl, out hv_Ncrigl, out hv_Distrigl);
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursl, ho_contourmidl, out ExpTmpOutVar_0,
                                            2);
                                        ho_SelectedContoursl.Dispose();
                                        ho_SelectedContoursl = ExpTmpOutVar_0;
                                    }
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursl, ho_contourrigl, out ExpTmpOutVar_0,
                                            3);
                                        ho_SelectedContoursl.Dispose();
                                        ho_SelectedContoursl = ExpTmpOutVar_0;
                                    }
                                }
                            }


                            //检查右侧3D采集图像，分割线段是否会出现折线
                            hv_Rowslefr.Dispose(); hv_Colslefr.Dispose();
                            HOperatorSet.GetContourXld(ho_contourlefr, out hv_Rowslefr, out hv_Colslefr);
                            hv_Rowsrigr.Dispose(); hv_Colsrigr.Dispose();
                            HOperatorSet.GetContourXld(ho_contourrigr, out hv_Rowsrigr, out hv_Colsrigr);
                            hv_Rowsmidr.Dispose(); hv_Colsmidr.Dispose();
                            HOperatorSet.GetContourXld(ho_contourmidr, out hv_Rowsmidr, out hv_Colsmidr);

                            hv_Minlefr.Dispose();
                            HOperatorSet.TupleMin(hv_Colslefr, out hv_Minlefr);
                            hv_Maxlefr.Dispose();
                            HOperatorSet.TupleMax(hv_Colslefr, out hv_Maxlefr);

                            hv_Minrigr.Dispose();
                            HOperatorSet.TupleMin(hv_Colsrigr, out hv_Minrigr);
                            hv_Maxrigr.Dispose();
                            HOperatorSet.TupleMax(hv_Colsrigr, out hv_Maxrigr);

                            hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrlefr.Dispose(); hv_Nclefr.Dispose(); hv_Distlefr.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourlefr, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr, out hv_ColEndlefr,
                                out hv_Nrlefr, out hv_Nclefr, out hv_Distlefr);
                            hv_RowBeginrigr.Dispose(); hv_ColBeginrigr.Dispose(); hv_RowEndrigr.Dispose(); hv_ColEndrigr.Dispose(); hv_Nrrigr.Dispose(); hv_Ncrigr.Dispose(); hv_Distrigr.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourrigr, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginrigr, out hv_ColBeginrigr, out hv_RowEndrigr, out hv_ColEndrigr,
                                out hv_Nrrigr, out hv_Ncrigr, out hv_Distrigr);

                            hv_lengthlefrcontour.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthlefrcontour = hv_Minlefr - hv_Maxlefr;
                            }
                            hv_lengthleflined.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthleflined = hv_ColEndlefr - hv_ColBeginlefr;
                            }
                            hv_Abslengthlefrcontour.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthlefrcontour, out hv_Abslengthlefrcontour);
                            hv_Abslengthleflined.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthleflined, out hv_Abslengthleflined);
                            if ((int)(new HTuple(((hv_Abslengthlefrcontour - hv_Abslengthleflined)).TupleGreater(
                                0.3))) != 0)
                            {
                                hv_countofCols.Dispose();
                                HOperatorSet.TupleLength(hv_Colslefr, out hv_countofCols);
                                HTuple end_val822 = 2;
                                HTuple step_val822 = -1;
                                for (hv_Index = hv_countofCols - 1; hv_Index.Continue(end_val822, step_val822); hv_Index = hv_Index.TupleAdd(step_val822))
                                {
                                    if ((int)((new HTuple((((hv_Rowslefr.TupleSelect(hv_Index)) - (hv_Rowslefr.TupleSelect(
                                        hv_Index - 1)))).TupleLess(-0.02))).TupleAnd(new HTuple((((hv_Rowslefr.TupleSelect(
                                        hv_Index - 1)) - (hv_Rowslefr.TupleSelect(hv_Index - 2)))).TupleLess(
                                        -0.02)))) != 0)
                                    {
                                        break;
                                    }
                                }

                                hv_Rowsnewlefr.Dispose();
                                hv_Rowsnewlefr = new HTuple();
                                hv_Colsnewlefr.Dispose();
                                hv_Colsnewlefr = new HTuple();
                                if ((int)(new HTuple(hv_Index.TupleGreater(2))) != 0)
                                {
                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val832 = hv_Index;
                                    HTuple step_val832 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val832, step_val832); hv_Index1 = hv_Index1.TupleAdd(step_val832))
                                    {
                                        if (hv_Rowsnewlefr == null)
                                            hv_Rowsnewlefr = new HTuple();
                                        hv_Rowsnewlefr[hv_Indexx] = hv_Rowslefr.TupleSelect(hv_Index1);
                                        if (hv_Colsnewlefr == null)
                                            hv_Colsnewlefr = new HTuple();
                                        hv_Colsnewlefr[hv_Indexx] = hv_Colslefr.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }


                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val840 = hv_countofCols - 1;
                                    HTuple step_val840 = 1;
                                    for (hv_Index1 = hv_Index; hv_Index1.Continue(end_val840, step_val840); hv_Index1 = hv_Index1.TupleAdd(step_val840))
                                    {
                                        if (hv_Rowsnewmidr == null)
                                            hv_Rowsnewmidr = new HTuple();
                                        hv_Rowsnewmidr[hv_Indexx] = hv_Rowslefr.TupleSelect(hv_Index1);
                                        if (hv_Colsnewmidr == null)
                                            hv_Colsnewmidr = new HTuple();
                                        hv_Colsnewmidr[hv_Indexx] = hv_Colslefr.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_countofRowsr.Dispose();
                                    HOperatorSet.TupleLength(hv_Rowsmidr, out hv_countofRowsr);
                                    HTuple end_val847 = hv_countofRowsr - 1;
                                    HTuple step_val847 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val847, step_val847); hv_Index1 = hv_Index1.TupleAdd(step_val847))
                                    {
                                        if (hv_Rowsnewmidr == null)
                                            hv_Rowsnewmidr = new HTuple();
                                        hv_Rowsnewmidr[hv_Indexx] = hv_Rowsmidr.TupleSelect(hv_Index1);
                                        if (hv_Colsnewmidr == null)
                                            hv_Colsnewmidr = new HTuple();
                                        hv_Colsnewmidr[hv_Indexx] = hv_Colsmidr.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    ho_contourlefr.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourlefr, hv_Rowsnewlefr,
                                        hv_Colsnewlefr);
                                    ho_contourmidr.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourmidr, hv_Rowsnewmidr,
                                        hv_Colsnewmidr);
                                    hv_RowBeginmidr.Dispose(); hv_ColBeginmidr.Dispose(); hv_RowEndmidr.Dispose(); hv_ColEndmidr.Dispose(); hv_Nrmidr.Dispose(); hv_Ncmidr.Dispose(); hv_Distmidr.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourmidr, "drop", -1, 5, 5,
                                        1, out hv_RowBeginmidr, out hv_ColBeginmidr, out hv_RowEndmidr,
                                        out hv_ColEndmidr, out hv_Nrmidr, out hv_Ncmidr, out hv_Distmidr);
                                    hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrlefr.Dispose(); hv_Nclefr.Dispose(); hv_Distlefr.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourlefr, "drop", -1, 5, 5,
                                        1, out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr,
                                        out hv_ColEndlefr, out hv_Nrlefr, out hv_Nclefr, out hv_Distlefr);
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursr, ho_contourlefr, out ExpTmpOutVar_0,
                                            1);
                                        ho_SelectedContoursr.Dispose();
                                        ho_SelectedContoursr = ExpTmpOutVar_0;
                                    }
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursr, ho_contourmidr, out ExpTmpOutVar_0,
                                            2);
                                        ho_SelectedContoursr.Dispose();
                                        ho_SelectedContoursr = ExpTmpOutVar_0;
                                    }
                                }

                            }


                            hv_lengthrigrcontour.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthrigrcontour = hv_Maxrigr - hv_Minrigr;
                            }
                            hv_lengthriglined.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_lengthriglined = hv_ColEndrigr - hv_ColBeginrigr;
                            }
                            hv_Abslengthrigrcontour.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthrigrcontour, out hv_Abslengthrigrcontour);
                            hv_Abslengthriglined.Dispose();
                            HOperatorSet.TupleAbs(hv_lengthriglined, out hv_Abslengthriglined);

                            if ((int)(new HTuple(((hv_Abslengthrigrcontour - hv_Abslengthriglined)).TupleGreater(
                                0.5))) != 0)
                            {
                                hv_countofColr.Dispose();
                                HOperatorSet.TupleLength(hv_Rowsrigr, out hv_countofColr);
                                HTuple end_val871 = hv_countofColr - 1;
                                HTuple step_val871 = 1;
                                for (hv_Index = 1; hv_Index.Continue(end_val871, step_val871); hv_Index = hv_Index.TupleAdd(step_val871))
                                {
                                    if ((int)(new HTuple((((hv_Rowsrigr.TupleSelect(hv_Index)) - (hv_Rowsrigr.TupleSelect(
                                        hv_Index + 1)))).TupleLess(-0.02))) != 0)
                                    {
                                        break;
                                    }
                                }

                                hv_Rowsnewrigl.Dispose();
                                hv_Rowsnewrigl = new HTuple();
                                hv_Colsnewrigl.Dispose();
                                hv_Colsnewrigl = new HTuple();
                                if ((int)(new HTuple(hv_Index.TupleNotEqual(hv_countofColr))) != 0)
                                {
                                    hv_Indexx.Dispose();
                                    hv_Indexx = 0;
                                    HTuple end_val881 = hv_countofColr - 1;
                                    HTuple step_val881 = 1;
                                    for (hv_Index1 = hv_Index; hv_Index1.Continue(end_val881, step_val881); hv_Index1 = hv_Index1.TupleAdd(step_val881))
                                    {
                                        if (hv_Rowsnewrigr == null)
                                            hv_Rowsnewrigr = new HTuple();
                                        hv_Rowsnewrigr[hv_Indexx] = hv_Rowsrigr.TupleSelect(hv_Index1);
                                        if (hv_Colsnewrigr == null)
                                            hv_Colsnewrigr = new HTuple();
                                        hv_Colsnewrigr[hv_Indexx] = hv_Colsrigr.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    hv_countofRowsr.Dispose();
                                    HOperatorSet.TupleLength(hv_Rowsmidr, out hv_countofRowsr);
                                    hv_Indexx.Dispose();
                                    hv_Indexx = new HTuple(hv_countofRowsr);
                                    HTuple end_val889 = hv_Index - 1;
                                    HTuple step_val889 = 1;
                                    for (hv_Index1 = 0; hv_Index1.Continue(end_val889, step_val889); hv_Index1 = hv_Index1.TupleAdd(step_val889))
                                    {
                                        if (hv_Rowsmidr == null)
                                            hv_Rowsmidr = new HTuple();
                                        hv_Rowsmidr[hv_Indexx] = hv_Rowsrigr.TupleSelect(hv_Index1);
                                        if (hv_Colsmidr == null)
                                            hv_Colsmidr = new HTuple();
                                        hv_Colsmidr[hv_Indexx] = hv_Colsrigr.TupleSelect(hv_Index1);
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            {
                                                HTuple
                                                  ExpTmpLocalVar_Indexx = hv_Indexx + 1;
                                                hv_Indexx.Dispose();
                                                hv_Indexx = ExpTmpLocalVar_Indexx;
                                            }
                                        }
                                    }

                                    ho_contourmidr.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourmidr, hv_Rowsmidr,
                                        hv_Colsmidr);
                                    ho_contourrigr.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_contourrigr, hv_Rowsnewrigr,
                                        hv_Colsnewrigr);
                                    hv_RowBeginmidr.Dispose(); hv_ColBeginmidr.Dispose(); hv_RowEndmidr.Dispose(); hv_ColEndmidr.Dispose(); hv_Nrmidr.Dispose(); hv_Ncmidr.Dispose(); hv_Distmidr.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourmidr, "drop", -1, 5, 5,
                                        1, out hv_RowBeginmidr, out hv_ColBeginmidr, out hv_RowEndmidr,
                                        out hv_ColEndmidr, out hv_Nrmidr, out hv_Ncmidr, out hv_Distmidr);
                                    hv_RowBeginrigr.Dispose(); hv_ColBeginrigr.Dispose(); hv_RowEndrigr.Dispose(); hv_ColEndrigr.Dispose(); hv_Nrrigr.Dispose(); hv_Ncrigr.Dispose(); hv_Distrigr.Dispose();
                                    HOperatorSet.FitLineContourXld(ho_contourrigr, "drop", -1, 5, 5,
                                        1, out hv_RowBeginrigr, out hv_ColBeginrigr, out hv_RowEndrigr,
                                        out hv_ColEndrigr, out hv_Nrrigr, out hv_Ncrigr, out hv_Distrigr);
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursr, ho_contourmidr, out ExpTmpOutVar_0,
                                            2);
                                        ho_SelectedContoursr.Dispose();
                                        ho_SelectedContoursr = ExpTmpOutVar_0;
                                    }
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ReplaceObj(ho_SelectedContoursr, ho_contourrigr, out ExpTmpOutVar_0,
                                            3);
                                        ho_SelectedContoursr.Dispose();
                                        ho_SelectedContoursr = ExpTmpOutVar_0;
                                    }
                                }
                            }

                            //说明截断的contourleft， 最大的Column值多了，出现最右边不是斜线，有横线部分，需要截断一部分
                            //contourmidt 需要增加一部分


                            hv_Rowsleft.Dispose(); hv_Colsleft.Dispose();
                            HOperatorSet.GetContourXld(ho_contourleft, out hv_Rowsleft, out hv_Colsleft);
                            hv_Rowsrigt.Dispose(); hv_Colsrigt.Dispose();
                            HOperatorSet.GetContourXld(ho_contourrigt, out hv_Rowsrigt, out hv_Colsrigt);

                            hv_Minl.Dispose();
                            HOperatorSet.TupleMin(hv_Colsleft, out hv_Minl);
                            hv_Minr.Dispose();
                            HOperatorSet.TupleMin(hv_Colsrigt, out hv_Minr);

                            if ((int)(new HTuple(hv_Minl.TupleGreater(hv_Minr))) != 0)
                            {
                                ho_contourtmp.Dispose();
                                ho_contourtmp = new HObject(ho_contourleft);
                                ho_contourleft.Dispose();
                                ho_contourleft = new HObject(ho_contourrigt);
                                ho_contourrigt.Dispose();
                                ho_contourrigt = new HObject(ho_contourtmp);
                            }

                            hv_Rowslefl.Dispose(); hv_Colslefl.Dispose();
                            HOperatorSet.GetContourXld(ho_contourlefd, out hv_Rowslefl, out hv_Colslefl);
                            hv_Rowsrigr.Dispose(); hv_Colsrigr.Dispose();
                            HOperatorSet.GetContourXld(ho_contourrigd, out hv_Rowsrigr, out hv_Colsrigr);

                            hv_Minl.Dispose();
                            HOperatorSet.TupleMin(hv_Colslefl, out hv_Minl);
                            hv_Minr.Dispose();
                            HOperatorSet.TupleMin(hv_Colsrigr, out hv_Minr);
                            if ((int)(new HTuple(hv_Minl.TupleGreater(hv_Minr))) != 0)
                            {
                                ho_contourtmp.Dispose();
                                ho_contourtmp = new HObject(ho_contourlefd);
                                ho_contourlefd.Dispose();
                                ho_contourlefd = new HObject(ho_contourrigd);
                                ho_contourrigd.Dispose();
                                ho_contourrigd = new HObject(ho_contourtmp);
                            }

                            hv_Rowslefl.Dispose(); hv_Colslefl.Dispose();
                            HOperatorSet.GetContourXld(ho_contourlefl, out hv_Rowslefl, out hv_Colslefl);
                            hv_Rowsrigr.Dispose(); hv_Colsrigr.Dispose();
                            HOperatorSet.GetContourXld(ho_contourrigl, out hv_Rowsrigr, out hv_Colsrigr);

                            hv_Minl.Dispose();
                            HOperatorSet.TupleMin(hv_Colslefl, out hv_Minl);
                            hv_Minr.Dispose();
                            HOperatorSet.TupleMin(hv_Colsrigr, out hv_Minr);
                            if ((int)(new HTuple(hv_Minl.TupleGreater(hv_Minr))) != 0)
                            {
                                ho_contourtmp.Dispose();
                                ho_contourtmp = new HObject(ho_contourlefl);
                                ho_contourlefl.Dispose();
                                ho_contourlefl = new HObject(ho_contourrigl);
                                ho_contourrigl.Dispose();
                                ho_contourrigl = new HObject(ho_contourtmp);
                            }

                            hv_Rowslefl.Dispose(); hv_Colslefl.Dispose();
                            HOperatorSet.GetContourXld(ho_contourlefr, out hv_Rowslefl, out hv_Colslefl);
                            hv_Rowsrigr.Dispose(); hv_Colsrigr.Dispose();
                            HOperatorSet.GetContourXld(ho_contourrigr, out hv_Rowsrigr, out hv_Colsrigr);

                            hv_Minl.Dispose();
                            HOperatorSet.TupleMin(hv_Colslefl, out hv_Minl);
                            hv_Minr.Dispose();
                            HOperatorSet.TupleMin(hv_Colsrigr, out hv_Minr);
                            if ((int)(new HTuple(hv_Minl.TupleGreater(hv_Minr))) != 0)
                            {
                                ho_contourtmp.Dispose();
                                ho_contourtmp = new HObject(ho_contourlefr);
                                ho_contourlefr.Dispose();
                                ho_contourlefr = new HObject(ho_contourrigr);
                                ho_contourrigr.Dispose();
                                ho_contourrigr = new HObject(ho_contourtmp);
                            }



                            hv_RowBeginmidt.Dispose(); hv_ColBeginmidt.Dispose(); hv_RowEndmidt.Dispose(); hv_ColEndmidt.Dispose(); hv_Nrmidt.Dispose(); hv_Ncmidt.Dispose(); hv_Distmidt.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourmidt, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginmidt, out hv_ColBeginmidt, out hv_RowEndmidt, out hv_ColEndmidt,
                                out hv_Nrmidt, out hv_Ncmidt, out hv_Distmidt);
                            hv_RowBeginmidd.Dispose(); hv_ColBeginmidd.Dispose(); hv_RowEndmidd.Dispose(); hv_ColEndmidd.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourmidd, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginmidd, out hv_ColBeginmidd, out hv_RowEndmidd, out hv_ColEndmidd,
                                out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                            hv_RowBeginmidl.Dispose(); hv_ColBeginmidl.Dispose(); hv_RowEndmidl.Dispose(); hv_ColEndmidl.Dispose(); hv_Nrmidl.Dispose(); hv_Ncmidl.Dispose(); hv_Distmidl.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourmidl, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginmidl, out hv_ColBeginmidl, out hv_RowEndmidl, out hv_ColEndmidl,
                                out hv_Nrmidl, out hv_Ncmidl, out hv_Distmidl);
                            hv_RowBeginmidr.Dispose(); hv_ColBeginmidr.Dispose(); hv_RowEndmidr.Dispose(); hv_ColEndmidr.Dispose(); hv_Nrmidr.Dispose(); hv_Ncmidr.Dispose(); hv_Distmidr.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourmidr, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginmidr, out hv_ColBeginmidr, out hv_RowEndmidr, out hv_ColEndmidr,
                                out hv_Nrmidr, out hv_Ncmidr, out hv_Distmidr);

                            hv_RowBeginleft.Dispose(); hv_ColBeginleft.Dispose(); hv_RowEndleft.Dispose(); hv_ColEndleft.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourleft, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginleft, out hv_ColBeginleft, out hv_RowEndleft, out hv_ColEndleft,
                                out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                            hv_RowBeginlefd.Dispose(); hv_ColBeginlefd.Dispose(); hv_RowEndlefd.Dispose(); hv_ColEndlefd.Dispose(); hv_Nrlefd.Dispose(); hv_Nclefd.Dispose(); hv_Distlefd.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourlefd, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginlefd, out hv_ColBeginlefd, out hv_RowEndlefd, out hv_ColEndlefd,
                                out hv_Nrlefd, out hv_Nclefd, out hv_Distlefd);
                            hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrlefl.Dispose(); hv_Nclefl.Dispose(); hv_Distlefl.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourlefl, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl, out hv_ColEndlefl,
                                out hv_Nrlefl, out hv_Nclefl, out hv_Distlefl);
                            hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrlefr.Dispose(); hv_Nclefr.Dispose(); hv_Distlefr.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourlefr, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr, out hv_ColEndlefr,
                                out hv_Nrlefr, out hv_Nclefr, out hv_Distlefr);

                            hv_RowBeginrigt.Dispose(); hv_ColBeginrigt.Dispose(); hv_RowEndrigt.Dispose(); hv_ColEndrigt.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigt.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourrigt, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginrigt, out hv_ColBeginrigt, out hv_RowEndrigt, out hv_ColEndrigt,
                                out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigt);
                            hv_RowBeginrigd.Dispose(); hv_ColBeginrigd.Dispose(); hv_RowEndrigd.Dispose(); hv_ColEndrigd.Dispose(); hv_Nrrigd.Dispose(); hv_Ncrigd.Dispose(); hv_Distrigd.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourrigd, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginrigd, out hv_ColBeginrigd, out hv_RowEndrigd, out hv_ColEndrigd,
                                out hv_Nrrigd, out hv_Ncrigd, out hv_Distrigd);
                            hv_RowBeginrigl.Dispose(); hv_ColBeginrigl.Dispose(); hv_RowEndrigl.Dispose(); hv_ColEndrigl.Dispose(); hv_Nrrigl.Dispose(); hv_Ncrigl.Dispose(); hv_Distrigl.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourrigl, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginrigl, out hv_ColBeginrigl, out hv_RowEndrigl, out hv_ColEndrigl,
                                out hv_Nrrigl, out hv_Ncrigl, out hv_Distrigl);
                            hv_RowBeginrigr.Dispose(); hv_ColBeginrigr.Dispose(); hv_RowEndrigr.Dispose(); hv_ColEndrigr.Dispose(); hv_Nrrigr.Dispose(); hv_Ncrigr.Dispose(); hv_Distrigr.Dispose();
                            HOperatorSet.FitLineContourXld(ho_contourrigr, "tukey", -1, 10, 5, 2,
                                out hv_RowBeginrigr, out hv_ColBeginrigr, out hv_RowEndrigr, out hv_ColEndrigr,
                                out hv_Nrrigr, out hv_Ncrigr, out hv_Distrigr);


                            if ((int)(new HTuple(hv_RowBeginleft.TupleGreater(hv_RowEndleft))) != 0)
                            {
                                hv_Angleradttl.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                    hv_ColEndmidt, hv_RowBeginleft, hv_ColBeginleft, hv_RowEndleft,
                                    hv_ColEndleft, out hv_Angleradttl);
                            }
                            else
                            {
                                hv_Angleradttl.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                    hv_ColEndmidt, hv_RowEndleft, hv_ColEndleft, hv_RowBeginleft, hv_ColBeginleft,
                                    out hv_Angleradttl);
                            }
                            hv_Degtl.Dispose();
                            HOperatorSet.TupleDeg(hv_Angleradttl, out hv_Degtl);
                            hv_DegtlAbs.Dispose();
                            HOperatorSet.TupleAbs(hv_Degtl, out hv_DegtlAbs);

                            if ((int)(new HTuple(hv_RowBeginrigt.TupleLess(hv_RowEndrigt))) != 0)
                            {
                                hv_Angleradtr.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                    hv_ColEndmidt, hv_RowBeginrigt, hv_ColBeginrigt, hv_RowEndrigt,
                                    hv_ColEndrigt, out hv_Angleradtr);
                            }
                            else
                            {
                                hv_Angleradtr.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                    hv_ColEndmidt, hv_RowEndrigt, hv_ColEndrigt, hv_RowBeginrigt, hv_ColBeginrigt,
                                    out hv_Angleradtr);
                            }
                            hv_Degtr.Dispose();
                            HOperatorSet.TupleDeg(hv_Angleradtr, out hv_Degtr);
                            hv_DegtrAbs.Dispose();
                            HOperatorSet.TupleAbs(hv_Degtr, out hv_DegtrAbs);

                            if ((int)(new HTuple(hv_RowBeginlefd.TupleGreater(hv_RowEndlefd))) != 0)
                            {
                                hv_Angleraddtl.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidd, hv_ColBeginmidd, hv_RowEndmidd,
                                    hv_ColEndmidd, hv_RowBeginlefd, hv_ColBeginlefd, hv_RowEndlefd,
                                    hv_ColEndlefd, out hv_Angleraddtl);
                                //angle_lx (RowBeginlefd, ColBeginlefd, RowEndlefd, ColEndlefd, Angleraddtl)
                            }
                            else
                            {
                                hv_Angleraddtl.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidd, hv_ColBeginmidd, hv_RowEndmidd,
                                    hv_ColEndmidd, hv_RowEndlefd, hv_ColEndlefd, hv_RowBeginlefd, hv_ColBeginlefd,
                                    out hv_Angleraddtl);
                                //angle_lx (RowEndlefd, ColEndlefd, RowBeginlefd, ColBeginlefd, Angleraddtl)

                            }
                            hv_Degdl.Dispose();
                            HOperatorSet.TupleDeg(hv_Angleraddtl, out hv_Degdl);
                            hv_DegdlAbs.Dispose();
                            HOperatorSet.TupleAbs(hv_Degdl, out hv_DegdlAbs);

                            if ((int)(new HTuple(hv_RowBeginrigd.TupleLess(hv_RowEndrigd))) != 0)
                            {
                                hv_Angleraddtr.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidd, hv_ColBeginmidd, hv_RowEndmidd,
                                    hv_ColEndmidd, hv_RowBeginrigt, hv_ColBeginrigt, hv_RowEndrigt,
                                    hv_ColEndrigt, out hv_Angleraddtr);
                                //angle_lx (RowBeginrigt, ColBeginrigt, RowEndrigt, ColEndrigt, Angleraddtr)
                            }
                            else
                            {
                                hv_Angleraddtr.Dispose();
                                HOperatorSet.AngleLl(hv_RowBeginmidd, hv_ColBeginmidd, hv_RowEndmidd,
                                    hv_ColEndmidd, hv_RowEndrigt, hv_ColEndrigt, hv_RowBeginrigt, hv_ColBeginrigt,
                                    out hv_Angleraddtr);
                                //angle_lx (RowEndrigt, ColEndrigt, RowBeginrigt, ColBeginrigt, Angleraddtr)
                            }
                            hv_Degtr.Dispose();
                            HOperatorSet.TupleDeg(hv_Angleraddtr, out hv_Degtr);
                            hv_DegdrAbs.Dispose();
                            HOperatorSet.TupleAbs(hv_Degtr, out hv_DegdrAbs);


                            if ((int)(new HTuple(hv_ColBeginleft.TupleLess(hv_ColBeginrigt))) != 0)
                            {
                                hv_Angleradd.Dispose();
                                HOperatorSet.AngleLx(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                    hv_ColEndmidt, out hv_Angleradd);
                            }
                            else
                            {
                                hv_Angleradd.Dispose();
                                HOperatorSet.AngleLx(hv_RowEndmidt, hv_ColEndmidt, hv_RowBeginmidt,
                                    hv_ColBeginmidt, out hv_Angleradd);
                            }
                            hv_Degtd.Dispose();
                            HOperatorSet.TupleDeg(hv_Angleradd, out hv_Degtd);
                            hv_DegtdAbs.Dispose();
                            HOperatorSet.TupleAbs(hv_Degtd, out hv_DegtdAbs);





                            hv_RowsMidt.Dispose(); hv_ColsMidt.Dispose();
                            HOperatorSet.GetContourXld(ho_contourmidt, out hv_RowsMidt, out hv_ColsMidt);
                            hv_RowsMidd.Dispose(); hv_ColsMidd.Dispose();
                            HOperatorSet.GetContourXld(ho_contourmidd, out hv_RowsMidd, out hv_ColsMidd);
                            hv_RowsMidl.Dispose(); hv_ColsMidl.Dispose();
                            HOperatorSet.GetContourXld(ho_contourmidl, out hv_RowsMidl, out hv_ColsMidl);
                            hv_RowsMidr.Dispose(); hv_ColsMidr.Dispose();
                            HOperatorSet.GetContourXld(ho_contourmidr, out hv_RowsMidr, out hv_ColsMidr);

                            hv_meanRowst.Dispose();
                            HOperatorSet.TupleMean(hv_RowsMidt, out hv_meanRowst);
                            hv_meanColst.Dispose();
                            HOperatorSet.TupleMean(hv_ColsMidt, out hv_meanColst);
                            hv_meanRowsd.Dispose();
                            HOperatorSet.TupleMean(hv_RowsMidd, out hv_meanRowsd);
                            hv_meanColsd.Dispose();
                            HOperatorSet.TupleMean(hv_ColsMidd, out hv_meanColsd);
                            hv_meanRowsl.Dispose();
                            HOperatorSet.TupleMean(hv_RowsMidl, out hv_meanRowsl);
                            hv_meanColsl.Dispose();
                            HOperatorSet.TupleMean(hv_ColsMidl, out hv_meanColsl);
                            hv_meanRowsr.Dispose();
                            HOperatorSet.TupleMean(hv_RowsMidr, out hv_meanRowsr);
                            hv_meanColsr.Dispose();
                            HOperatorSet.TupleMean(hv_ColsMidr, out hv_meanColsr);

                            hv_minColsd.Dispose();
                            HOperatorSet.TupleMin(hv_ColsMidd, out hv_minColsd);
                            hv_maxColsd.Dispose();
                            HOperatorSet.TupleMax(hv_ColsMidd, out hv_maxColsd);
                            hv_minColst.Dispose();
                            HOperatorSet.TupleMin(hv_ColsMidt, out hv_minColst);
                            hv_maxColst.Dispose();
                            HOperatorSet.TupleMax(hv_ColsMidt, out hv_maxColst);
                            hv_minColsl.Dispose();
                            HOperatorSet.TupleMin(hv_ColsMidl, out hv_minColsl);
                            hv_maxColsl.Dispose();
                            HOperatorSet.TupleMax(hv_ColsMidl, out hv_maxColsl);
                            hv_minColsr.Dispose();
                            HOperatorSet.TupleMin(hv_ColsMidr, out hv_minColsr);
                            hv_maxColsr.Dispose();
                            HOperatorSet.TupleMax(hv_ColsMidr, out hv_maxColsr);
                            //meanColst := (minColst + maxColst) / 2

                            hv_HomMat2DIdentity.Dispose();
                            HOperatorSet.HomMat2dIdentity(out hv_HomMat2DIdentity);





                            //以上侧3D相机激光线侧的坐标为原点， 根据标定块，进行拟合。求出左侧3D相机的坐标位置
                            //Y轴， 以上侧3D相机激光口作为参考线，激光口为线，水平线。
                            //X轴， 以3D相机采集物体中点作为参考点
                            if ((int)(new HTuple(hv_LeftTDistance.TupleEqual(0))) != 0)
                            {
                                //与上3D相机，左侧对齐
                                hv_Areal.Dispose(); hv_Rowl.Dispose(); hv_Columnl.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursl, out hv_Areal, out hv_Rowl,
                                    out hv_Columnl, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowst - hv_meanRowsl,
                                        hv_minColst - hv_minColsl, out hv_HomMatTransPreL);
                                }
                                ho_SelectedContourslPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursl, out ho_SelectedContourslPre,
                                    hv_HomMatTransPreL);

                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslPre, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslPre, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslPre, out ho_contourdowlnew,
                                    3);

                                hv_RowsMidnewt.Dispose(); hv_ColsMidnewt.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidlnew, out hv_RowsMidnewt, out hv_ColsMidnewt);

                                hv_MeanMidnewColt.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidnewt, out hv_MeanMidnewColt);
                                hv_MeanMidnewRowy.Dispose();
                                HOperatorSet.TupleMean(hv_RowsMidnewt, out hv_MeanMidnewRowy);
                                //逆向旋转90度， 中间的线的中点为旋转中心
                                hv_Areal.Dispose(); hv_RowPrel.Dispose(); hv_ColumnPrel.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContourslPre, out hv_Areal, out hv_RowPrel,
                                    out hv_ColumnPrel, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DLRotate.Dispose();
                                    HOperatorSet.HomMat2dRotate(hv_HomMat2DIdentity, (new HTuple(-90)).TupleRad()
                                        , hv_MeanMidnewRowy, hv_MeanMidnewColt, out hv_HomMat2DLRotate);
                                }
                                ho_SelectedContourslfinal.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslPre, out ho_SelectedContourslfinal,
                                    hv_HomMat2DLRotate);


                                hv_Areal.Dispose(); hv_RowPrel.Dispose(); hv_ColumnPrel.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContourslfinal, out hv_Areal,
                                    out hv_RowPrel, out hv_ColumnPrel, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DScale.Dispose();
                                    HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_RowPrel.TupleSelect(
                                        1), hv_ColumnPrel.TupleSelect(1), out hv_HomMat2DScale);
                                }
                                ho_SelectedContourslnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslfinal, out ho_SelectedContourslnewpre,
                                    hv_HomMat2DScale);

                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourdowlnew,
                                    3);


                                hv_RowsMidl.Dispose(); hv_ColsMidl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidlnew, out hv_RowsMidl, out hv_ColsMidl);
                                hv_meanMidColnew.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidl, out hv_meanMidColnew);

                                hv_RowBeginmidtnew.Dispose(); hv_ColBeginmidtnew.Dispose(); hv_RowEndmidtnew.Dispose(); hv_ColEndmidtnew.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmidlnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmidtnew, out hv_ColBeginmidtnew, out hv_RowEndmidtnew,
                                    out hv_ColEndmidtnew, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_Rowminmidt.Dispose();
                                HOperatorSet.TupleMin2(hv_RowBeginmidtnew, hv_RowEndmidtnew, out hv_Rowminmidt);
                                //meanmidCols := (ColBeginmidtnew + ColEndmidtnew) / 2
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowst - hv_Rowminmidt;
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_maxColst - hv_meanMidColnew,
                                        out hv_HomMatTransPreL);
                                }
                                ho_SelectedContourslprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslnewpre, out ho_SelectedContourslprenew,
                                    hv_HomMatTransPreL);


                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourdowlnew,
                                    3);

                                hv_RowsTopl.Dispose(); hv_ColsTopl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourtoplnew, out hv_RowsTopl, out hv_ColsTopl);
                                hv_RowsDowl.Dispose(); hv_ColsDowl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourdowlnew, out hv_RowsDowl, out hv_ColsDowl);

                                hv_meanTopl.Dispose();
                                HOperatorSet.TupleMean(hv_RowsTopl, out hv_meanTopl);
                                hv_meanDowl.Dispose();
                                HOperatorSet.TupleMean(hv_RowsDowl, out hv_meanDowl);

                                if ((int)(new HTuple(hv_meanTopl.TupleGreater(hv_meanDowl))) != 0)
                                {
                                    ho_contourtmp.Dispose();
                                    ho_contourtmp = new HObject(ho_contourtoplnew);
                                    ho_contourtoplnew.Dispose();
                                    ho_contourtoplnew = new HObject(ho_contourdowlnew);
                                    ho_contourdowlnew.Dispose();
                                    ho_contourdowlnew = new HObject(ho_contourtmp);
                                }

                                hv_RowBegintopl.Dispose(); hv_ColBegintopl.Dispose(); hv_RowEndtopl.Dispose(); hv_ColEndtopl.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoplnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopl, out hv_ColBegintopl, out hv_RowEndtopl,
                                    out hv_ColEndtopl, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);

                                //新的图形SelectedContourslfinal，上侧边的角度要与 上3D相机采集图像右侧边角度对齐
                                if ((int)(new HTuple(hv_RowBegintopl.TupleGreater(hv_RowEndtopl))) != 0)
                                {
                                    hv_angleltx.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowBegintopl, hv_ColBegintopl, hv_RowEndtopl,
                                        hv_ColEndtopl, out hv_angleltx);
                                }
                                else
                                {
                                    hv_angleltx.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowEndtopl, hv_ColEndtopl, hv_RowBegintopl,
                                        hv_ColBegintopl, out hv_angleltx);
                                }

                                hv_Degltx.Dispose();
                                HOperatorSet.TupleDeg(hv_angleltx, out hv_Degltx);
                                hv_AbgDegltx.Dispose();
                                HOperatorSet.TupleAbs(hv_Degltx, out hv_AbgDegltx);
                                hv_Abgangleltx.Dispose();
                                HOperatorSet.TupleAbs(hv_Angleradtr, out hv_Abgangleltx);
                                hv_DegSubL.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubL = hv_DegtrAbs - (180 - hv_AbgDegltx);
                                }

                                //if (DegSubL > 0.1 or DegSubL < -0.1)
                                //area_center_xld (SelectedContourslprenew, Areal, Rowl, Columnl, PointOrderl)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(-DegSubL), Columnl[1], Rowl[1], HomMat2DLRotate)
                                //affine_trans_contour_xld (SelectedContourslprenew, SelectedContourslfinalnew, HomMat2DLRotate)
                                //select_obj (SelectedContourslfinalnew, contourtoplnew, 1)
                                //select_obj (SelectedContourslfinalnew, contourmidlnew, 2)
                                //select_obj (SelectedContourslfinalnew, contourdowlnew, 3)

                                //get_contour_xld (contourtoplnew, RowsTopl, ColsTopl)
                                //get_contour_xld (contourdowlnew, RowsDowl, ColsDowl)

                                //tuple_mean (RowsTopl, meanTopl)
                                //tuple_mean (RowsDowl, meanDowl)

                                //if (meanTopl > meanDowl)
                                //contourtmp := contourtoplnew
                                //contourtoplnew := contourdowlnew
                                //contourdowlnew := contourtmp
                                //endif
                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBegintopl, ColBegintopl, RowEndtopl, ColEndtopl, Nrleft, Ncleft, Distleft)
                                //if (RowBegintopl > RowEndtopl)
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowBegintopl, ColBegintopl, RowEndtopl, ColEndtopl, angleltx)
                                //else
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowEndtopl, ColEndtopl, RowBegintopl, ColBegintopl, angleltx)
                                //endif
                                //tuple_deg (angleltx, Degltx)

                                //DegSubL := DegtrAbs - (180 - Degltx)
                                //SelectedContourslprenew := SelectedContourslfinalnew
                                //endif



                                //get_contour_xld (contourmidlnew, Rowslmidnew, Colslmidnew)
                                //tuple_mean (Colslmidnew, meanmidColsnew)
                                //Y轴 新的图像，要沿着中间线 contourmidlnew，向下移动到与 上侧相机采集图形 SelectedContourst，中间线contourmidt的高度一致。
                                //X轴 新的图像，要沿着 SelectedContourst 的中间线 contourmidt 向左侧偏移到起点，进行滑行

                                //标定块拟合，需要根据标定块边长计算左移距离，  ( 182 - 12 ) * sin45 = 111.72, 移动后就是左边3D相机的具体位置
                                //( 182 - 12 ) * sin45  + meanRowsl , 下次按照这个思路进行偏移，根据新的方棒左3D相机采集的高度，进行
                                //计算，例如标定快高度为 -13,  新的方棒标定块 -11， 那么左移动就是（-11 + 13） = 2， 左移动 111.72 + 2
                                //则与3D位置相同。要沿着上面3D，左边的角度进行移动。 右边3D处理与此类似。



                                hv_Sintl.Dispose();
                                HOperatorSet.TupleSin(hv_Abgangleltx, out hv_Sintl);
                                hv_Costl.Dispose();
                                HOperatorSet.TupleCos(hv_Abgangleltx, out hv_Costl);
                                hv_AbsSintl.Dispose();
                                HOperatorSet.TupleAbs(hv_Sintl, out hv_AbsSintl);
                                hv_AbsCostl.Dispose();
                                HOperatorSet.TupleAbs(hv_Costl, out hv_AbsCostl);


                                //hom_mat2d_translate (HomMat2DIdentity, 111.72, -111.72, HomMatTransPreL)
                                //intersection_lines (RowBeginlefl, ColBeginlefl, RowEndlefl, ColEndlefl, RowBeginrigl, ColBeginrigl, RowEndrigl, ColEndrigl, RowIntersectiontlx, RowIntersectiontly, IsOverlapping1)
                                //distance_pp (RowIntersectiontlx, RowIntersectiontly, RowBeginrigt, ColBeginrigt, Distancetr)

                                hv_DistanceLPre.Dispose();
                                hv_DistanceLPre = 158.26;
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_DistanceLPre * hv_AbsSintl,
                                        hv_DistanceLPre * hv_AbsCostl, out hv_HomMatTransPreL);
                                }
                                //hom_mat2d_translate (HomMat2DIdentity, 111.72, -111.72, HomMatTransPreL)
                                ho_SelectedContourslnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslprenew, out ho_SelectedContourslnew,
                                    hv_HomMatTransPreL);
                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourdowlnew,
                                    3);

                                //DistanceLPre 为边长
                                //X轴  先向左 移动  meanColst - meanColsl，
                                //Y轴  先移动 meanRowst - RowBeginmidt ，
                                //逆时针旋转90度， 暂时水平翻转180度。  左侧相机右侧跟上侧相机右侧相对。上侧相机采集图像是否需要以中间线中点纵向翻转？
                                //根据新的图形SelectedContourslnewpre，中间线X值，向 上侧相机中间线左边的点对齐，左移 minColst - meanmidColsnew
                                //然后再向左移动 DistanceLPre * Costl  再向下 移动 DistanceLPre * Sintl
                                //Left3DLineX 为旋转后，左3D相机拍出来的方棒上边缘的X位置
                                //Left3DX 为3D相机线所在位置，这个位置为新的方棒，左侧3D相机移动拟合的位置。
                                //Left3DX X轴相对上侧相机采集图像中点的距离 meanColst 为 上次相机采集图像中心点X坐标
                                //Left3DY Y轴相对上侧相机激光口所在水平线的Y轴相对距离
                                hv_RowsnewMidl.Dispose(); hv_ColsnewMidl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidlnew, out hv_RowsnewMidl, out hv_ColsnewMidl);
                                hv_Left3DLineX.Dispose();
                                HOperatorSet.TupleMean(hv_ColsnewMidl, out hv_Left3DLineX);
                                hv_AbsLeft3Ddistance.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsl, out hv_AbsLeft3Ddistance);
                                hv_Left3DX.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_Left3DX = (hv_Left3DLineX + hv_AbsLeft3Ddistance) - hv_meanColst;
                                }
                                hv_AbsLeft3DX.Dispose();
                                HOperatorSet.TupleAbs(hv_Left3DX, out hv_AbsLeft3DX);
                                hv_Left3DX.Dispose();
                                hv_Left3DX = new HTuple(hv_AbsLeft3DX);
                                hv_Left3DLineY.Dispose();
                                HOperatorSet.TupleMean(hv_RowsnewMidl, out hv_Left3DLineY);
                                hv_Left3DY.Dispose();
                                hv_Left3DY = new HTuple(hv_Left3DLineY);
                                hv_LeftTDistance.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_LeftTDistance = ((hv_DistanceLPre * hv_AbsCostl) + hv_AbsLeft3Ddistance) + ((hv_maxColst - hv_minColst) / 2);
                                }
                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, Nrleft, Ncleft, Distleft)
                                hv_RowBeginleft.Dispose(); hv_ColBeginleft.Dispose(); hv_RowEndleft.Dispose(); hv_ColEndleft.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourleft, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginleft, out hv_ColBeginleft, out hv_RowEndleft,
                                    out hv_ColEndleft, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_RowBeginrigt.Dispose(); hv_ColBeginrigt.Dispose(); hv_RowEndrigt.Dispose(); hv_ColEndrigt.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigt.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourrigt, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginrigt, out hv_ColBeginrigt, out hv_RowEndrigt,
                                    out hv_ColEndrigt, out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigt);
                                hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoplnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl,
                                    out hv_ColEndlefl, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourdowlnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr,
                                    out hv_ColEndlefr, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);

                                hv_RowIntersectiontl.Dispose(); hv_ColIntersectiontl.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefl, hv_ColBeginlefl, hv_RowEndlefl,
                                    hv_ColEndlefl, hv_RowBeginlefr, hv_ColBeginlefr, hv_RowEndlefr,
                                    hv_ColEndlefr, out hv_RowIntersectiontl, out hv_ColIntersectiontl,
                                    out hv_IsOverlapping);
                                hv_RowIntersectiont.Dispose(); hv_ColIntersectiont.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginleft, hv_ColBeginleft, hv_RowEndleft,
                                    hv_ColEndleft, hv_RowBeginrigt, hv_ColBeginrigt, hv_RowEndrigt,
                                    hv_ColEndrigt, out hv_RowIntersectiont, out hv_ColIntersectiont,
                                    out hv_IsOverlapping);
                                hv_DistanceLT.Dispose();
                                HOperatorSet.DistancePp(hv_RowIntersectiontl, hv_ColIntersectiontl,
                                    hv_RowIntersectiont, hv_ColIntersectiont, out hv_DistanceLT);
                            }
                            else
                            {
                                //与上3D相机，左侧对齐
                                hv_Areal.Dispose(); hv_Rowl.Dispose(); hv_Columnl.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursl, out hv_Areal, out hv_Rowl,
                                    out hv_Columnl, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowst - hv_meanRowsl,
                                        hv_minColst - hv_minColsl, out hv_HomMatTransPreL);
                                }
                                ho_SelectedContourslPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursl, out ho_SelectedContourslPre,
                                    hv_HomMatTransPreL);

                                //逆向旋转90度， 中间的线的中点为旋转中心
                                hv_Areal.Dispose(); hv_RowPrel.Dispose(); hv_ColumnPrel.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContourslPre, out hv_Areal, out hv_RowPrel,
                                    out hv_ColumnPrel, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DLRotate.Dispose();
                                    HOperatorSet.HomMat2dRotate(hv_HomMat2DIdentity, (new HTuple(-90)).TupleRad()
                                        , hv_RowPrel.TupleSelect(1), hv_ColumnPrel.TupleSelect(1), out hv_HomMat2DLRotate);
                                }
                                ho_SelectedContourslfinal.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslPre, out ho_SelectedContourslfinal,
                                    hv_HomMat2DLRotate);

                                hv_Areal.Dispose(); hv_RowPrel.Dispose(); hv_ColumnPrel.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContourslfinal, out hv_Areal,
                                    out hv_RowPrel, out hv_ColumnPrel, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DScale.Dispose();
                                    HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_RowPrel.TupleSelect(
                                        1), hv_ColumnPrel.TupleSelect(1), out hv_HomMat2DScale);
                                }
                                ho_SelectedContourslnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslfinal, out ho_SelectedContourslnewpre,
                                    hv_HomMat2DScale);

                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourdowlnew,
                                    3);


                                hv_RowsMidl.Dispose(); hv_ColsMidl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidlnew, out hv_RowsMidl, out hv_ColsMidl);
                                hv_meanMidColnew.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidl, out hv_meanMidColnew);

                                hv_RowBeginmidtnew.Dispose(); hv_ColBeginmidtnew.Dispose(); hv_RowEndmidtnew.Dispose(); hv_ColEndmidtnew.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmidlnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmidtnew, out hv_ColBeginmidtnew, out hv_RowEndmidtnew,
                                    out hv_ColEndmidtnew, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_Rowminmidt.Dispose();
                                HOperatorSet.TupleMin2(hv_RowBeginmidtnew, hv_RowEndmidtnew, out hv_Rowminmidt);
                                //meanmidCols := (ColBeginmidtnew + ColEndmidtnew) / 2
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowst - hv_Rowminmidt;
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_maxColst - hv_meanMidColnew,
                                        out hv_HomMatTransPreL);
                                }
                                ho_SelectedContourslprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslnewpre, out ho_SelectedContourslprenew,
                                    hv_HomMatTransPreL);

                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourdowlnew,
                                    3);
                                hv_RowsTopl.Dispose(); hv_ColsTopl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourtoplnew, out hv_RowsTopl, out hv_ColsTopl);
                                hv_RowsDowl.Dispose(); hv_ColsDowl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourdowlnew, out hv_RowsDowl, out hv_ColsDowl);

                                hv_meanTopl.Dispose();
                                HOperatorSet.TupleMean(hv_RowsTopl, out hv_meanTopl);
                                hv_meanDowl.Dispose();
                                HOperatorSet.TupleMean(hv_RowsDowl, out hv_meanDowl);

                                if ((int)(new HTuple(hv_meanTopl.TupleGreater(hv_meanDowl))) != 0)
                                {
                                    ho_contourtmp.Dispose();
                                    ho_contourtmp = new HObject(ho_contourtoplnew);
                                    ho_contourtoplnew.Dispose();
                                    ho_contourtoplnew = new HObject(ho_contourdowlnew);
                                    ho_contourdowlnew.Dispose();
                                    ho_contourdowlnew = new HObject(ho_contourtmp);
                                }
                                hv_RowBegintopl.Dispose(); hv_ColBegintopl.Dispose(); hv_RowEndtopl.Dispose(); hv_ColEndtopl.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoplnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopl, out hv_ColBegintopl, out hv_RowEndtopl,
                                    out hv_ColEndtopl, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);

                                //新的图形SelectedContourslfinal，上侧边的角度要与 上3D相机采集图像右侧边角度对齐
                                if ((int)(new HTuple(hv_RowBegintopl.TupleGreater(hv_RowEndtopl))) != 0)
                                {
                                    hv_angleltx.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowBegintopl, hv_ColBegintopl, hv_RowEndtopl,
                                        hv_ColEndtopl, out hv_angleltx);
                                }
                                else
                                {
                                    hv_angleltx.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowEndtopl, hv_ColEndtopl, hv_RowBegintopl,
                                        hv_ColBegintopl, out hv_angleltx);
                                }

                                hv_Degltx.Dispose();
                                HOperatorSet.TupleDeg(hv_angleltx, out hv_Degltx);
                                hv_AbgDegltx.Dispose();
                                HOperatorSet.TupleAbs(hv_Degltx, out hv_AbgDegltx);
                                hv_Abgangleltx.Dispose();
                                HOperatorSet.TupleAbs(hv_Angleradtr, out hv_Abgangleltx);
                                hv_DegSubL.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubL = hv_DegtrAbs - (180 - hv_AbgDegltx);
                                }
                                //if (DegSubL > 0.1 or DegSubL < -0.1)
                                //area_center_xld (SelectedContourslprenew, Areal, Rowl, Columnl, PointOrderl)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(-DegSubL), Columnl[1], Rowl[1], HomMat2DLRotate)
                                //affine_trans_contour_xld (SelectedContourslprenew, SelectedContourslfinalnew, HomMat2DLRotate)
                                //select_obj (SelectedContourslfinalnew, contourtoplnew, 1)
                                //select_obj (SelectedContourslfinalnew, contourmidlnew, 2)
                                //select_obj (SelectedContourslfinalnew, contourdowlnew, 3)
                                //get_contour_xld (contourtoplnew, RowsTopl, ColsTopl)
                                //get_contour_xld (contourdowlnew, RowsDowl, ColsDowl)

                                //tuple_mean (RowsTopl, meanTopl)
                                //tuple_mean (RowsDowl, meanDowl)

                                //if (meanTopl > meanDowl)
                                //contourtmp := contourtoplnew
                                //contourtoplnew := contourdowlnew
                                //contourdowlnew := contourtmp
                                //endif
                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBegintopl, ColBegintopl, RowEndtopl, ColEndtopl, Nrleft, Ncleft, Distleft)
                                //if (RowBegintopl > RowEndtopl)
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowBegintopl, ColBegintopl, RowEndtopl, ColEndtopl, angleltx)
                                //else
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowEndtopl, ColEndtopl, RowBegintopl, ColBegintopl, angleltx)
                                //endif
                                //tuple_deg (angleltx, Degltx)

                                //DegSubL := DegtrAbs - (180 - Degltx)
                                //SelectedContourslprenew := SelectedContourslfinalnew
                                //endif



                                //Y轴 新的图像，要沿着中间线 contourmidlnew，向下移动到与 上侧相机采集图形 SelectedContourst，中间线contourmidt的高度一致。
                                //X轴 新的图像，要沿着 SelectedContourst 的中间线 contourmidt 向左侧偏移到起点，进行滑行


                                //标定块拟合，需要根据标定块边长计算左移距离，  ( 182 - 12 ) * sin45 = 111.72, 移动后就是左边3D相机的具体位置
                                //( 182 - 12 ) * sin45  + meanRowsl , 下次按照这个思路进行偏移，根据新的方棒左3D相机采集的高度，进行
                                //计算，例如标定快高度为 -13,  新的方棒标定块 -11， 那么左移动就是（-11 + 13） = 2， 左移动 111.72 + 2
                                //则与3D位置相同。要沿着上面3D，左边的角度进行移动。 右边3D处理与此类似。

                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourmidlnew,
                                    2);
                                hv_RowsMidl.Dispose(); hv_ColsMidl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidlnew, out hv_RowsMidl, out hv_ColsMidl);
                                hv_MeanMidlx.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidl, out hv_MeanMidlx);

                                hv_Sintl.Dispose();
                                HOperatorSet.TupleSin(hv_Abgangleltx, out hv_Sintl);
                                hv_Costl.Dispose();
                                HOperatorSet.TupleCos(hv_Abgangleltx, out hv_Costl);
                                hv_AbsSintl.Dispose();
                                HOperatorSet.TupleAbs(hv_Sintl, out hv_AbsSintl);
                                hv_AbsCostl.Dispose();
                                HOperatorSet.TupleAbs(hv_Costl, out hv_AbsCostl);

                                //Left3DX + AbsmeanRowsl - meanColst 为 contourmidrnew 需要移动到的位置 X坐标
                                //Left3DX X轴相对上侧相机采集图像中点的距离
                                //Left3DY Y轴相对上侧相机激光口所在水平线的Y轴相对距离
                                //新的方棒 AbsmeanRowsl 采集图像，到激光口的Y 距离
                                hv_AbsmeanRowsl.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsl, out hv_AbsmeanRowsl);
                                hv_DistanceXPre.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DistanceXPre = (hv_LeftTDistance - hv_AbsmeanRowsl) - ((hv_maxColst - hv_minColst) / 2);
                                }
                                hv_DistanceLPre.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DistanceLPre = hv_DistanceXPre / hv_AbsCostl;
                                }

                                //DistanceLPre := 157.98
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_DistanceLPre * hv_AbsSintl,
                                        hv_DistanceXPre, out hv_HomMatTransPreL);
                                }
                                //hom_mat2d_translate (HomMat2DIdentity, 111.72, -111.72, HomMatTransPreL)
                                ho_SelectedContourslnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslprenew, out ho_SelectedContourslnew,
                                    hv_HomMatTransPreL);
                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourdowlnew,
                                    3);
                                //DistanceLPre 为边长
                                //X轴  先向左 移动  meanColst - meanColsl，
                                //Y轴  先移动 meanRowst - RowBeginmidt ，
                                //逆时针旋转90度， 暂时水平翻转180度。  左侧相机右侧跟上侧相机右侧相对。上侧相机采集图像是否需要以中间线中点纵向翻转？
                                //根据新的图形SelectedContourslnewpre，中间线X值，向 上侧相机中间线左边的点对齐，左移 minColst - meanmidColsnew
                                //然后再向左移动 DistanceLPre * Costl  再向下 移动 DistanceLPre * Sintl
                                //Left3DLineX 为旋转后，左3D相机拍出来的方棒上边缘的X位置
                                //Left3DX 为3D相机线所在位置，这个位置为新的方棒，左侧3D相机移动拟合的位置。

                                //get_contour_xld (contourmidlnew, RowsnewMidl, ColsnewMidl)
                                //tuple_mean (ColsnewMidl, Left3DLineX)
                                //tuple_abs (meanRowsl, AbsLeft3Ddistance)
                                //Left3DX := Left3DLineX - AbsLeft3Ddistance
                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, Nrleft, Ncleft, Distleft)
                                hv_RowBeginleft.Dispose(); hv_ColBeginleft.Dispose(); hv_RowEndleft.Dispose(); hv_ColEndleft.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourleft, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginleft, out hv_ColBeginleft, out hv_RowEndleft,
                                    out hv_ColEndleft, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_RowBeginrigt.Dispose(); hv_ColBeginrigt.Dispose(); hv_RowEndrigt.Dispose(); hv_ColEndrigt.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigt.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourrigt, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginrigt, out hv_ColBeginrigt, out hv_RowEndrigt,
                                    out hv_ColEndrigt, out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigt);
                                hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoplnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl,
                                    out hv_ColEndlefl, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourdowlnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr,
                                    out hv_ColEndlefr, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);

                                hv_RowIntersectiontl.Dispose(); hv_ColIntersectiontl.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefl, hv_ColBeginlefl, hv_RowEndlefl,
                                    hv_ColEndlefl, hv_RowBeginlefr, hv_ColBeginlefr, hv_RowEndlefr,
                                    hv_ColEndlefr, out hv_RowIntersectiontl, out hv_ColIntersectiontl,
                                    out hv_IsOverlapping);
                                hv_RowIntersectiont.Dispose(); hv_ColIntersectiont.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginleft, hv_ColBeginleft, hv_RowEndleft,
                                    hv_ColEndleft, hv_RowBeginrigt, hv_ColBeginrigt, hv_RowEndrigt,
                                    hv_ColEndrigt, out hv_RowIntersectiont, out hv_ColIntersectiont,
                                    out hv_IsOverlapping);
                                hv_DistanceLT.Dispose();
                                HOperatorSet.DistancePp(hv_RowIntersectiontl, hv_ColIntersectiontl,
                                    hv_RowIntersectiont, hv_ColIntersectiont, out hv_DistanceLT);
                            }



                            //以上侧3D相机激光线侧的坐标为原点， 根据标定块，进行拟合。求出左侧3D相机的坐标位置
                            //Y轴， 以上侧3D相机激光口作为参考线，激光口为线，水平线。
                            //X轴， 以3D相机采集物体中点作为参考点
                            if ((int)(new HTuple(hv_LeftDDistance.TupleEqual(0))) != 0)
                            {
                                //与上3D相机，左侧对齐
                                hv_Areal.Dispose(); hv_Rowl.Dispose(); hv_Columnl.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursl, out hv_Areal, out hv_Rowl,
                                    out hv_Columnl, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowsd - hv_meanRowsl,
                                        hv_minColsd - hv_minColsl, out hv_HomMatTransPreL);
                                }
                                ho_SelectedContourslPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursl, out ho_SelectedContourslPre,
                                    hv_HomMatTransPreL);

                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslPre, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslPre, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslPre, out ho_contourdowlnew,
                                    3);

                                hv_RowsMidnewt.Dispose(); hv_ColsMidnewt.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidlnew, out hv_RowsMidnewt, out hv_ColsMidnewt);

                                hv_MeanMidnewColt.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidnewt, out hv_MeanMidnewColt);
                                hv_MeanMidnewRowy.Dispose();
                                HOperatorSet.TupleMean(hv_RowsMidnewt, out hv_MeanMidnewRowy);
                                //逆向旋转90度， 中间的线的中点为旋转中心
                                hv_Areal.Dispose(); hv_RowPrel.Dispose(); hv_ColumnPrel.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContourslPre, out hv_Areal, out hv_RowPrel,
                                    out hv_ColumnPrel, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DLRotate.Dispose();
                                    HOperatorSet.HomMat2dRotate(hv_HomMat2DIdentity, (new HTuple(90)).TupleRad()
                                        , hv_MeanMidnewRowy, hv_MeanMidnewColt, out hv_HomMat2DLRotate);
                                }
                                ho_SelectedContourslfinal.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslPre, out ho_SelectedContourslfinal,
                                    hv_HomMat2DLRotate);


                                hv_Areal.Dispose(); hv_RowPrel.Dispose(); hv_ColumnPrel.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContourslfinal, out hv_Areal,
                                    out hv_RowPrel, out hv_ColumnPrel, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DScale.Dispose();
                                    HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_RowPrel.TupleSelect(
                                        1), hv_ColumnPrel.TupleSelect(1), out hv_HomMat2DScale);
                                }
                                ho_SelectedContourslnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslfinal, out ho_SelectedContourslnewpre,
                                    hv_HomMat2DScale);

                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourdowlnew,
                                    3);


                                hv_RowsMidl.Dispose(); hv_ColsMidl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidlnew, out hv_RowsMidl, out hv_ColsMidl);
                                hv_meanMidColnew.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidl, out hv_meanMidColnew);

                                hv_RowBeginmidtnew.Dispose(); hv_ColBeginmidtnew.Dispose(); hv_RowEndmidtnew.Dispose(); hv_ColEndmidtnew.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmidlnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmidtnew, out hv_ColBeginmidtnew, out hv_RowEndmidtnew,
                                    out hv_ColEndmidtnew, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_Rowminmidt.Dispose();
                                HOperatorSet.TupleMin2(hv_RowBeginmidtnew, hv_RowEndmidtnew, out hv_Rowminmidt);
                                //meanmidCols := (ColBeginmidtnew + ColEndmidtnew) / 2
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowsd - hv_Rowminmidt;
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_minColsd - hv_meanMidColnew,
                                        out hv_HomMatTransPreL);
                                }
                                ho_SelectedContourslprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslnewpre, out ho_SelectedContourslprenew,
                                    hv_HomMatTransPreL);


                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourdowlnew,
                                    3);

                                hv_RowsTopl.Dispose(); hv_ColsTopl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourtoplnew, out hv_RowsTopl, out hv_ColsTopl);
                                hv_RowsDowl.Dispose(); hv_ColsDowl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourdowlnew, out hv_RowsDowl, out hv_ColsDowl);

                                hv_meanTopl.Dispose();
                                HOperatorSet.TupleMean(hv_RowsTopl, out hv_meanTopl);
                                hv_meanDowl.Dispose();
                                HOperatorSet.TupleMean(hv_RowsDowl, out hv_meanDowl);

                                if ((int)(new HTuple(hv_meanTopl.TupleGreater(hv_meanDowl))) != 0)
                                {
                                    ho_contourtmp.Dispose();
                                    ho_contourtmp = new HObject(ho_contourtoplnew);
                                    ho_contourtoplnew.Dispose();
                                    ho_contourtoplnew = new HObject(ho_contourdowlnew);
                                    ho_contourdowlnew.Dispose();
                                    ho_contourdowlnew = new HObject(ho_contourtmp);
                                }

                                hv_RowBegintopl.Dispose(); hv_ColBegintopl.Dispose(); hv_RowEndtopl.Dispose(); hv_ColEndtopl.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoplnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopl, out hv_ColBegintopl, out hv_RowEndtopl,
                                    out hv_ColEndtopl, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);

                                //新的图形SelectedContourslfinal，上侧边的角度要与 上3D相机采集图像右侧边角度对齐
                                if ((int)(new HTuple(hv_RowBegintopl.TupleGreater(hv_RowEndtopl))) != 0)
                                {
                                    hv_angleltx.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowBegintopl, hv_ColBegintopl, hv_RowEndtopl,
                                        hv_ColEndtopl, out hv_angleltx);
                                }
                                else
                                {
                                    hv_angleltx.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowEndtopl, hv_ColEndtopl, hv_RowBegintopl,
                                        hv_ColBegintopl, out hv_angleltx);
                                }

                                hv_Degltx.Dispose();
                                HOperatorSet.TupleDeg(hv_angleltx, out hv_Degltx);
                                hv_AbgDegltx.Dispose();
                                HOperatorSet.TupleAbs(hv_Degltx, out hv_AbgDegltx);
                                hv_Abgangleltx.Dispose();
                                HOperatorSet.TupleAbs(hv_Angleraddtl, out hv_Abgangleltx);
                                hv_DegSubL.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubL = hv_DegtrAbs - (180 - hv_AbgDegltx);
                                }

                                //if (DegSubL > 0.1 or DegSubL < -0.1)
                                //area_center_xld (SelectedContourslprenew, Areal, Rowl, Columnl, PointOrderl)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(-DegSubL), Columnl[1], Rowl[1], HomMat2DLRotate)
                                //affine_trans_contour_xld (SelectedContourslprenew, SelectedContourslfinalnew, HomMat2DLRotate)
                                //select_obj (SelectedContourslfinalnew, contourtoplnew, 1)
                                //select_obj (SelectedContourslfinalnew, contourmidlnew, 2)
                                //select_obj (SelectedContourslfinalnew, contourdowlnew, 3)

                                //get_contour_xld (contourtoplnew, RowsTopl, ColsTopl)
                                //get_contour_xld (contourdowlnew, RowsDowl, ColsDowl)

                                //tuple_mean (RowsTopl, meanTopl)
                                //tuple_mean (RowsDowl, meanDowl)

                                //if (meanTopl > meanDowl)
                                //contourtmp := contourtoplnew
                                //contourtoplnew := contourdowlnew
                                //contourdowlnew := contourtmp
                                //endif
                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBegintopl, ColBegintopl, RowEndtopl, ColEndtopl, Nrleft, Ncleft, Distleft)
                                //if (RowBegintopl > RowEndtopl)
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowBegintopl, ColBegintopl, RowEndtopl, ColEndtopl, angleltx)
                                //else
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowEndtopl, ColEndtopl, RowBegintopl, ColBegintopl, angleltx)
                                //endif
                                //tuple_deg (angleltx, Degltx)

                                //DegSubL := DegtrAbs - (180 - Degltx)
                                //SelectedContourslprenew := SelectedContourslfinalnew
                                //endif



                                //get_contour_xld (contourmidlnew, Rowslmidnew, Colslmidnew)
                                //tuple_mean (Colslmidnew, meanmidColsnew)
                                //Y轴 新的图像，要沿着中间线 contourmidlnew，向下移动到与 上侧相机采集图形 SelectedContourst，中间线contourmidt的高度一致。
                                //X轴 新的图像，要沿着 SelectedContourst 的中间线 contourmidt 向左侧偏移到起点，进行滑行

                                //标定块拟合，需要根据标定块边长计算左移距离，  ( 182 - 12 ) * sin45 = 111.72, 移动后就是左边3D相机的具体位置
                                //( 182 - 12 ) * sin45  + meanRowsl , 下次按照这个思路进行偏移，根据新的方棒左3D相机采集的高度，进行
                                //计算，例如标定快高度为 -13,  新的方棒标定块 -11， 那么左移动就是（-11 + 13） = 2， 左移动 111.72 + 2
                                //则与3D位置相同。要沿着上面3D，左边的角度进行移动。 右边3D处理与此类似。


                                hv_Sintl.Dispose();
                                HOperatorSet.TupleSin(hv_Abgangleltx, out hv_Sintl);
                                hv_Costl.Dispose();
                                HOperatorSet.TupleCos(hv_Abgangleltx, out hv_Costl);
                                hv_AbsSintl.Dispose();
                                HOperatorSet.TupleAbs(hv_Sintl, out hv_AbsSintl);
                                hv_AbsCostl.Dispose();
                                HOperatorSet.TupleAbs(hv_Costl, out hv_AbsCostl);


                                hv_DistanceLPre.Dispose();
                                hv_DistanceLPre = 158.6;
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_DistanceLPre * hv_AbsSintl,
                                        -(hv_DistanceLPre * hv_AbsCostl), out hv_HomMatTransPreL);
                                }
                                //hom_mat2d_translate (HomMat2DIdentity, 111.72, -111.72, HomMatTransPreL)
                                ho_SelectedContourslnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslprenew, out ho_SelectedContourslnew,
                                    hv_HomMatTransPreL);
                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourdowlnew,
                                    3);

                                //DistanceLPre 为边长
                                //X轴  先向左 移动  meanColst - meanColsl，
                                //Y轴  先移动 meanRowst - RowBeginmidt ，
                                //逆时针旋转90度， 暂时水平翻转180度。  左侧相机右侧跟上侧相机右侧相对。上侧相机采集图像是否需要以中间线中点纵向翻转？
                                //根据新的图形SelectedContourslnewpre，中间线X值，向 上侧相机中间线左边的点对齐，左移 minColst - meanmidColsnew
                                //然后再向左移动 DistanceLPre * Costl  再向下 移动 DistanceLPre * Sintl
                                //Left3DLineX 为旋转后，左3D相机拍出来的方棒上边缘的X位置
                                //Left3DX 为3D相机线所在位置，这个位置为新的方棒，左侧3D相机移动拟合的位置。
                                //Left3DX X轴相对上侧相机采集图像中点的距离 meanColst 为 上次相机采集图像中心点X坐标
                                //Left3DY Y轴相对上侧相机激光口所在水平线的Y轴相对距离
                                hv_RowsnewMidl.Dispose(); hv_ColsnewMidl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidlnew, out hv_RowsnewMidl, out hv_ColsnewMidl);
                                hv_Left3DLineX.Dispose();
                                HOperatorSet.TupleMean(hv_ColsnewMidl, out hv_Left3DLineX);
                                hv_AbsLeft3Ddistance.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsl, out hv_AbsLeft3Ddistance);
                                hv_Left3DX.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_Left3DX = (hv_Left3DLineX + hv_AbsLeft3Ddistance) - hv_meanColst;
                                }
                                hv_AbsLeft3DX.Dispose();
                                HOperatorSet.TupleAbs(hv_Left3DX, out hv_AbsLeft3DX);
                                hv_Left3DX.Dispose();
                                hv_Left3DX = new HTuple(hv_AbsLeft3DX);
                                hv_Left3DLineY.Dispose();
                                HOperatorSet.TupleMean(hv_RowsnewMidl, out hv_Left3DLineY);
                                hv_Left3DY.Dispose();
                                hv_Left3DY = new HTuple(hv_Left3DLineY);
                                hv_LeftDDistance.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_LeftDDistance = ((hv_DistanceLPre * hv_AbsCostl) + hv_AbsLeft3Ddistance) + ((hv_maxColst - hv_minColst) / 2);
                                }
                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, Nrleft, Ncleft, Distleft)
                                hv_RowBeginlefd.Dispose(); hv_ColBeginlefd.Dispose(); hv_RowEndlefd.Dispose(); hv_ColEndlefd.Dispose(); hv_Nrlefd.Dispose(); hv_Nclefd.Dispose(); hv_Distlefd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourlefd, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginlefd, out hv_ColBeginlefd, out hv_RowEndlefd,
                                    out hv_ColEndlefd, out hv_Nrlefd, out hv_Nclefd, out hv_Distlefd);
                                hv_RowBeginrigd.Dispose(); hv_ColBeginrigd.Dispose(); hv_RowEndrigd.Dispose(); hv_ColEndrigd.Dispose(); hv_Nrrigd.Dispose(); hv_Ncrigd.Dispose(); hv_Distrigd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourrigd, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginrigd, out hv_ColBeginrigd, out hv_RowEndrigd,
                                    out hv_ColEndrigd, out hv_Nrrigd, out hv_Ncrigd, out hv_Distrigd);
                                hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoplnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl,
                                    out hv_ColEndlefl, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourdowlnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr,
                                    out hv_ColEndlefr, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);

                                hv_RowIntersectiontd.Dispose(); hv_ColIntersectiontd.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefl, hv_ColBeginlefl, hv_RowEndlefl,
                                    hv_ColEndlefl, hv_RowBeginlefr, hv_ColBeginlefr, hv_RowEndlefr,
                                    hv_ColEndlefr, out hv_RowIntersectiontd, out hv_ColIntersectiontd,
                                    out hv_IsOverlapping);
                                hv_RowIntersectiont.Dispose(); hv_ColIntersectiont.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefd, hv_ColBeginlefd, hv_RowEndlefd,
                                    hv_ColEndlefd, hv_RowBeginrigd, hv_ColBeginrigd, hv_RowEndrigd,
                                    hv_ColEndrigd, out hv_RowIntersectiont, out hv_ColIntersectiont,
                                    out hv_IsOverlapping);
                                hv_DistanceDL.Dispose();
                                HOperatorSet.DistancePp(hv_RowIntersectiontd, hv_ColIntersectiontd,
                                    hv_RowIntersectiont, hv_ColIntersectiont, out hv_DistanceDL);
                            }
                            else
                            {
                                //与上3D相机，左侧对齐
                                hv_Areal.Dispose(); hv_Rowl.Dispose(); hv_Columnl.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursl, out hv_Areal, out hv_Rowl,
                                    out hv_Columnl, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowsd - hv_meanRowsl,
                                        hv_minColsd - hv_minColsl, out hv_HomMatTransPreL);
                                }
                                ho_SelectedContourslPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursl, out ho_SelectedContourslPre,
                                    hv_HomMatTransPreL);

                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslPre, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslPre, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslPre, out ho_contourdowlnew,
                                    3);

                                hv_RowsMidnewt.Dispose(); hv_ColsMidnewt.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidlnew, out hv_RowsMidnewt, out hv_ColsMidnewt);

                                hv_MeanMidnewColt.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidnewt, out hv_MeanMidnewColt);
                                hv_MeanMidnewRowy.Dispose();
                                HOperatorSet.TupleMean(hv_RowsMidnewt, out hv_MeanMidnewRowy);
                                //逆向旋转90度， 中间的线的中点为旋转中心
                                hv_Areal.Dispose(); hv_RowPrel.Dispose(); hv_ColumnPrel.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContourslPre, out hv_Areal, out hv_RowPrel,
                                    out hv_ColumnPrel, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DLRotate.Dispose();
                                    HOperatorSet.HomMat2dRotate(hv_HomMat2DIdentity, (new HTuple(90)).TupleRad()
                                        , hv_MeanMidnewRowy, hv_MeanMidnewColt, out hv_HomMat2DLRotate);
                                }
                                ho_SelectedContourslfinal.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslPre, out ho_SelectedContourslfinal,
                                    hv_HomMat2DLRotate);


                                hv_Areal.Dispose(); hv_RowPrel.Dispose(); hv_ColumnPrel.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContourslfinal, out hv_Areal,
                                    out hv_RowPrel, out hv_ColumnPrel, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DScale.Dispose();
                                    HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_RowPrel.TupleSelect(
                                        1), hv_ColumnPrel.TupleSelect(1), out hv_HomMat2DScale);
                                }
                                ho_SelectedContourslnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslfinal, out ho_SelectedContourslnewpre,
                                    hv_HomMat2DScale);

                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnewpre, out ho_contourdowlnew,
                                    3);


                                hv_RowsMidl.Dispose(); hv_ColsMidl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidlnew, out hv_RowsMidl, out hv_ColsMidl);
                                hv_meanMidColnew.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidl, out hv_meanMidColnew);

                                hv_RowBeginmidtnew.Dispose(); hv_ColBeginmidtnew.Dispose(); hv_RowEndmidtnew.Dispose(); hv_ColEndmidtnew.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmidlnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmidtnew, out hv_ColBeginmidtnew, out hv_RowEndmidtnew,
                                    out hv_ColEndmidtnew, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_Rowminmidt.Dispose();
                                HOperatorSet.TupleMin2(hv_RowBeginmidtnew, hv_RowEndmidtnew, out hv_Rowminmidt);
                                //meanmidCols := (ColBeginmidtnew + ColEndmidtnew) / 2
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowsd - hv_Rowminmidt;
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_minColsd - hv_meanMidColnew,
                                        out hv_HomMatTransPreL);
                                }
                                ho_SelectedContourslprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslnewpre, out ho_SelectedContourslprenew,
                                    hv_HomMatTransPreL);

                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourdowlnew,
                                    3);
                                hv_RowsTopl.Dispose(); hv_ColsTopl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourtoplnew, out hv_RowsTopl, out hv_ColsTopl);
                                hv_RowsDowl.Dispose(); hv_ColsDowl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourdowlnew, out hv_RowsDowl, out hv_ColsDowl);

                                hv_meanTopl.Dispose();
                                HOperatorSet.TupleMean(hv_RowsTopl, out hv_meanTopl);
                                hv_meanDowl.Dispose();
                                HOperatorSet.TupleMean(hv_RowsDowl, out hv_meanDowl);

                                if ((int)(new HTuple(hv_meanTopl.TupleGreater(hv_meanDowl))) != 0)
                                {
                                    ho_contourtmp.Dispose();
                                    ho_contourtmp = new HObject(ho_contourtoplnew);
                                    ho_contourtoplnew.Dispose();
                                    ho_contourtoplnew = new HObject(ho_contourdowlnew);
                                    ho_contourdowlnew.Dispose();
                                    ho_contourdowlnew = new HObject(ho_contourtmp);
                                }
                                hv_RowBegintopl.Dispose(); hv_ColBegintopl.Dispose(); hv_RowEndtopl.Dispose(); hv_ColEndtopl.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoplnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopl, out hv_ColBegintopl, out hv_RowEndtopl,
                                    out hv_ColEndtopl, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);

                                //新的图形SelectedContourslfinal，上侧边的角度要与 上3D相机采集图像右侧边角度对齐
                                if ((int)(new HTuple(hv_RowBegintopl.TupleGreater(hv_RowEndtopl))) != 0)
                                {
                                    hv_angleltx.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowBegintopl, hv_ColBegintopl, hv_RowEndtopl,
                                        hv_ColEndtopl, out hv_angleltx);
                                }
                                else
                                {
                                    hv_angleltx.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowEndtopl, hv_ColEndtopl, hv_RowBegintopl,
                                        hv_ColBegintopl, out hv_angleltx);
                                }

                                hv_Degltx.Dispose();
                                HOperatorSet.TupleDeg(hv_angleltx, out hv_Degltx);
                                hv_AbgDegltx.Dispose();
                                HOperatorSet.TupleAbs(hv_Degltx, out hv_AbgDegltx);
                                hv_Abgangleltx.Dispose();
                                HOperatorSet.TupleAbs(hv_Angleraddtl, out hv_Abgangleltx);
                                hv_DegSubL.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubL = hv_DegtrAbs - (180 - hv_AbgDegltx);
                                }

                                //if (DegSubL > 0.1 or DegSubL < -0.1)
                                //area_center_xld (SelectedContourslprenew, Areal, Rowl, Columnl, PointOrderl)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(-DegSubL), Columnl[1], Rowl[1], HomMat2DLRotate)
                                //affine_trans_contour_xld (SelectedContourslprenew, SelectedContourslfinalnew, HomMat2DLRotate)
                                //select_obj (SelectedContourslfinalnew, contourtoplnew, 1)
                                //select_obj (SelectedContourslfinalnew, contourmidlnew, 2)
                                //select_obj (SelectedContourslfinalnew, contourdowlnew, 3)
                                //get_contour_xld (contourtoplnew, RowsTopl, ColsTopl)
                                //get_contour_xld (contourdowlnew, RowsDowl, ColsDowl)

                                //tuple_mean (RowsTopl, meanTopl)
                                //tuple_mean (RowsDowl, meanDowl)

                                //if (meanTopl > meanDowl)
                                //contourtmp := contourtoplnew
                                //contourtoplnew := contourdowlnew
                                //contourdowlnew := contourtmp
                                //endif
                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBegintopl, ColBegintopl, RowEndtopl, ColEndtopl, Nrleft, Ncleft, Distleft)
                                //if (RowBegintopl > RowEndtopl)
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowBegintopl, ColBegintopl, RowEndtopl, ColEndtopl, angleltx)
                                //else
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowEndtopl, ColEndtopl, RowBegintopl, ColBegintopl, angleltx)
                                //endif
                                //tuple_deg (angleltx, Degltx)

                                //DegSubL := DegtrAbs - (180 - Degltx)
                                //SelectedContourslprenew := SelectedContourslfinalnew
                                //endif



                                //Y轴 新的图像，要沿着中间线 contourmidlnew，向下移动到与 上侧相机采集图形 SelectedContourst，中间线contourmidt的高度一致。
                                //X轴 新的图像，要沿着 SelectedContourst 的中间线 contourmidt 向左侧偏移到起点，进行滑行


                                //标定块拟合，需要根据标定块边长计算左移距离，  ( 182 - 12 ) * sin45 = 111.72, 移动后就是左边3D相机的具体位置
                                //( 182 - 12 ) * sin45  + meanRowsl , 下次按照这个思路进行偏移，根据新的方棒左3D相机采集的高度，进行
                                //计算，例如标定快高度为 -13,  新的方棒标定块 -11， 那么左移动就是（-11 + 13） = 2， 左移动 111.72 + 2
                                //则与3D位置相同。要沿着上面3D，左边的角度进行移动。 右边3D处理与此类似。

                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslprenew, out ho_contourmidlnew,
                                    2);
                                hv_RowsMidl.Dispose(); hv_ColsMidl.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidlnew, out hv_RowsMidl, out hv_ColsMidl);
                                hv_MeanMidlx.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidl, out hv_MeanMidlx);

                                //tuple_abs (angleltx, Absangleltx)
                                hv_Sintl.Dispose();
                                HOperatorSet.TupleSin(hv_Abgangleltx, out hv_Sintl);
                                hv_Costl.Dispose();
                                HOperatorSet.TupleCos(hv_Abgangleltx, out hv_Costl);
                                hv_AbsSintl.Dispose();
                                HOperatorSet.TupleAbs(hv_Sintl, out hv_AbsSintl);
                                hv_AbsCostl.Dispose();
                                HOperatorSet.TupleAbs(hv_Costl, out hv_AbsCostl);

                                //Left3DX + AbsmeanRowsl - meanColst 为 contourmidrnew 需要移动到的位置 X坐标
                                //Left3DX X轴相对上侧相机采集图像中点的距离
                                //Left3DY Y轴相对上侧相机激光口所在水平线的Y轴相对距离
                                //新的方棒 AbsmeanRowsl 采集图像，到激光口的Y 距离
                                hv_AbsmeanRowsl.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsl, out hv_AbsmeanRowsl);
                                hv_DistanceXPre.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DistanceXPre = (hv_LeftDDistance - hv_AbsmeanRowsl) - ((hv_maxColst - hv_minColst) / 2);
                                }
                                hv_DistanceLPre.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DistanceLPre = hv_DistanceXPre / hv_AbsCostl;
                                }

                                //DistanceLPre := 157.98
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreL.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_DistanceLPre * hv_AbsSintl,
                                        -(hv_DistanceLPre * hv_AbsCostl), out hv_HomMatTransPreL);
                                }
                                //hom_mat2d_translate (HomMat2DIdentity, 111.72, -111.72, HomMatTransPreL)
                                ho_SelectedContourslnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContourslprenew, out ho_SelectedContourslnew,
                                    hv_HomMatTransPreL);
                                ho_contourtoplnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourtoplnew,
                                    1);
                                ho_contourmidlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourmidlnew,
                                    2);
                                ho_contourdowlnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContourslnew, out ho_contourdowlnew,
                                    3);
                                //DistanceLPre 为边长
                                //X轴  先向左 移动  meanColst - meanColsl，
                                //Y轴  先移动 meanRowst - RowBeginmidt ，
                                //逆时针旋转90度， 暂时水平翻转180度。  左侧相机右侧跟上侧相机右侧相对。上侧相机采集图像是否需要以中间线中点纵向翻转？
                                //根据新的图形SelectedContourslnewpre，中间线X值，向 上侧相机中间线左边的点对齐，左移 minColst - meanmidColsnew
                                //然后再向左移动 DistanceLPre * Costl  再向下 移动 DistanceLPre * Sintl
                                //Left3DLineX 为旋转后，左3D相机拍出来的方棒上边缘的X位置
                                //Left3DX 为3D相机线所在位置，这个位置为新的方棒，左侧3D相机移动拟合的位置。

                                //get_contour_xld (contourmidlnew, RowsnewMidl, ColsnewMidl)
                                //tuple_mean (ColsnewMidl, Left3DLineX)
                                //tuple_abs (meanRowsl, AbsLeft3Ddistance)
                                //Left3DX := Left3DLineX - AbsLeft3Ddistance
                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, Nrleft, Ncleft, Distleft)
                                hv_RowBeginlefd.Dispose(); hv_ColBeginlefd.Dispose(); hv_RowEndlefd.Dispose(); hv_ColEndlefd.Dispose(); hv_Nrlefd.Dispose(); hv_Nclefd.Dispose(); hv_Distlefd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourlefd, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginlefd, out hv_ColBeginlefd, out hv_RowEndlefd,
                                    out hv_ColEndlefd, out hv_Nrlefd, out hv_Nclefd, out hv_Distlefd);
                                hv_RowBeginrigd.Dispose(); hv_ColBeginrigd.Dispose(); hv_RowEndrigd.Dispose(); hv_ColEndrigd.Dispose(); hv_Nrrigd.Dispose(); hv_Ncrigd.Dispose(); hv_Distrigd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourrigd, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginrigd, out hv_ColBeginrigd, out hv_RowEndrigd,
                                    out hv_ColEndrigd, out hv_Nrrigd, out hv_Ncrigd, out hv_Distrigd);
                                hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoplnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl,
                                    out hv_ColEndlefl, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourdowlnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr,
                                    out hv_ColEndlefr, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);

                                hv_RowIntersectiontd.Dispose(); hv_ColIntersectiontd.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefl, hv_ColBeginlefl, hv_RowEndlefl,
                                    hv_ColEndlefl, hv_RowBeginlefr, hv_ColBeginlefr, hv_RowEndlefr,
                                    hv_ColEndlefr, out hv_RowIntersectiontd, out hv_ColIntersectiontd,
                                    out hv_IsOverlapping);
                                hv_RowIntersectiont.Dispose(); hv_ColIntersectiont.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefd, hv_ColBeginlefd, hv_RowEndlefd,
                                    hv_ColEndlefd, hv_RowBeginrigd, hv_ColBeginrigd, hv_RowEndrigd,
                                    hv_ColEndrigd, out hv_RowIntersectiont, out hv_ColIntersectiont,
                                    out hv_IsOverlapping);
                                hv_DistanceDL.Dispose();
                                HOperatorSet.DistancePp(hv_RowIntersectiontd, hv_ColIntersectiontd,
                                    hv_RowIntersectiont, hv_ColIntersectiont, out hv_DistanceDL);
                            }


                            //以上侧3D相机激光线侧的坐标为原点， 根据标定块，进行拟合。求出右侧3D相机的坐标位置
                            if ((int)(new HTuple(hv_RightTDistance.TupleEqual(0))) != 0)
                            {
                                //与上3D相机，左侧对齐
                                hv_Arear.Dispose(); hv_Rowr.Dispose(); hv_Columnr.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursr, out hv_Arear, out hv_Rowr,
                                    out hv_Columnr, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowst - hv_meanRowsr,
                                        hv_minColst - hv_minColsr, out hv_HomMatTransPreR);
                                }
                                ho_SelectedContoursrPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursr, out ho_SelectedContoursrPre,
                                    hv_HomMatTransPreR);

                                //顺时针旋转90度， 中间的线的中点为旋转中心
                                hv_Arear.Dispose(); hv_RowPrer.Dispose(); hv_ColumnPrer.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursrPre, out hv_Arear, out hv_RowPrer,
                                    out hv_ColumnPrer, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DRRotate.Dispose();
                                    HOperatorSet.HomMat2dRotate(hv_HomMat2DIdentity, (new HTuple(90)).TupleRad()
                                        , hv_RowPrer.TupleSelect(1), hv_ColumnPrer.TupleSelect(1), out hv_HomMat2DRRotate);
                                }
                                ho_SelectedContoursrfinal.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrPre, out ho_SelectedContoursrfinal,
                                    hv_HomMat2DRRotate);

                                //沿着SelectedContourslfinal 中间线左侧点，以X轴为中心，进行水平反转
                                //上相机右侧与左侧相机左侧对应，所以需要反转
                                hv_Arear.Dispose(); hv_RowPrer.Dispose(); hv_ColumnPrer.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursrfinal, out hv_Arear,
                                    out hv_RowPrer, out hv_ColumnPrer, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DScale.Dispose();
                                    HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_RowPrer.TupleSelect(
                                        1), hv_ColumnPrer.TupleSelect(1), out hv_HomMat2DScale);
                                }
                                ho_SelectedContoursrnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrfinal, out ho_SelectedContoursrnewpre,
                                    hv_HomMat2DScale);


                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourdowrnew,
                                    3);



                                hv_RowBeginmidnewr.Dispose(); hv_ColBeginmidnewr.Dispose(); hv_RowEndmidnewr.Dispose(); hv_ColEndmidnewr.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmidrnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmidnewr, out hv_ColBeginmidnewr, out hv_RowEndmidnewr,
                                    out hv_ColEndmidnewr, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_Rowmidrmin.Dispose();
                                HOperatorSet.TupleMin2(hv_RowBeginmidnewr, hv_RowEndmidnewr, out hv_Rowmidrmin);
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowst - hv_Rowmidrmin;
                                }
                                hv_Rowsrmidnew.Dispose(); hv_Colsrmidnew.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidrnew, out hv_Rowsrmidnew, out hv_Colsrmidnew);
                                hv_meanmidColsnew.Dispose();
                                HOperatorSet.TupleMean(hv_Colsrmidnew, out hv_meanmidColsnew);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_minColst - hv_meanmidColsnew,
                                        out hv_HomMatTransPreR);
                                }
                                ho_SelectedContoursrprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrnewpre, out ho_SelectedContoursrprenew,
                                    hv_HomMatTransPreR);

                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourdowrnew,
                                    3);

                                hv_RowsTopr.Dispose(); hv_ColsTopr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourtoprnew, out hv_RowsTopr, out hv_ColsTopr);
                                hv_RowsDowr.Dispose(); hv_ColsDowr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourdowrnew, out hv_RowsDowr, out hv_ColsDowr);

                                hv_meanTopr.Dispose();
                                HOperatorSet.TupleMean(hv_RowsTopr, out hv_meanTopr);
                                hv_meanDowr.Dispose();
                                HOperatorSet.TupleMean(hv_RowsDowr, out hv_meanDowr);

                                if ((int)(new HTuple(hv_meanTopr.TupleGreater(hv_meanDowr))) != 0)
                                {
                                    ho_contourtmp.Dispose();
                                    ho_contourtmp = new HObject(ho_contourtoprnew);
                                    ho_contourtoprnew.Dispose();
                                    ho_contourtoprnew = new HObject(ho_contourdowrnew);
                                    ho_contourdowrnew.Dispose();
                                    ho_contourdowrnew = new HObject(ho_contourtmp);
                                }

                                //新的图形SelectedContoursrfinal，上侧边的角度要与 上3D相机采集图像左侧边角度对齐
                                hv_RowBegintopr.Dispose(); hv_ColBegintopr.Dispose(); hv_RowEndtopr.Dispose(); hv_ColEndtopr.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoprnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopr, out hv_ColBegintopr, out hv_RowEndtopr,
                                    out hv_ColEndtopr, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                if ((int)(new HTuple(hv_RowBegintopr.TupleGreater(hv_RowEndtopr))) != 0)
                                {
                                    hv_Angleradtr.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowBegintopr, hv_ColBegintopr, hv_RowEndtopr,
                                        hv_ColEndtopr, out hv_Angleradtr);
                                }
                                else
                                {
                                    hv_Angleradtr.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowEndtopr, hv_ColEndtopr, hv_RowBegintopr,
                                        hv_ColBegintopr, out hv_Angleradtr);
                                }
                                hv_Degrtr.Dispose();
                                HOperatorSet.TupleDeg(hv_Angleradtr, out hv_Degrtr);
                                hv_DegrtrAbs.Dispose();
                                HOperatorSet.TupleAbs(hv_Degrtr, out hv_DegrtrAbs);
                                hv_DegSubR.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubR = hv_DegtlAbs - hv_DegrtrAbs;
                                }

                                //if (DegSubR > 0.1 or DegSubR < -0.1)
                                //area_center_xld (SelectedContoursrprenew, Arear, Rowr, Columnr, PointOrderr)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(DegSubR), Columnr[1], Rowr[1], HomMat2DRRotate)
                                //affine_trans_contour_xld (SelectedContoursrprenew, SelectedContoursrfinalnew, HomMat2DRRotate)
                                //select_obj (SelectedContoursrfinalnew, contourtoprnew, 1)
                                //select_obj (SelectedContoursrfinalnew, contourmidrnew, 2)
                                //select_obj (SelectedContoursrfinalnew, contourdowrnew, 3)

                                //get_contour_xld (contourtoprnew, RowsTopr, ColsTopr)
                                //get_contour_xld (contourdowrnew, RowsDowr, ColsDowr)

                                //tuple_mean (RowsTopr, meanTopr)
                                //tuple_mean (RowsDowr, meanDowr)

                                //if (meanTopr > meanDowr)
                                //contourtmp := contourtoprnew
                                //contourtoprnew := contourdowrnew
                                //contourdowrnew := contourtmp
                                //endif

                                //fit_line_contour_xld (contourtoprnew, 'tukey', -1, 10, 5, 2, RowBegintopr, ColBegintopr, RowEndtopr, ColEndtopr, Nrleft, Ncleft, Distleft)

                                //if (RowBegintopr > RowEndtopr)
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowBegintopr, ColBegintopr, RowEndtopr, ColEndtopr, Angleradtr)
                                //else
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowEndtopr, ColEndtopr, RowBegintopr, ColBegintopr, Angleradtr)
                                //endif
                                //tuple_deg (Angleradtr, Degrtr)
                                //tuple_abs (Degrtr, DegrtrAbs)
                                //DegSubR := DegtlAbs -  DegrtrAbs
                                //SelectedContoursrprenew := SelectedContoursrfinalnew

                                //endif


                                //标定块拟合，需要根据标定块边长计算左移距离，  ( 182 - 12 ) * sin45 = 111.72, 移动后就是左边3D相机的具体位置
                                //( 182 - 12 ) * sin45  + meanRowsl , 下次按照这个思路进行偏移，根据新的方棒左3D相机采集的高度，进行
                                //计算，例如标定快高度为 -13,  新的方棒标定块 -11， 那么左移动就是（-11 + 13） = 2， 左移动 111.72 + 2
                                //则与3D位置相同。要沿着上面3D，左边的角度进行移动。 右边3D处理与此类似。

                                hv_AngleradtrAbs.Dispose();
                                HOperatorSet.TupleAbs(hv_Angleradttl, out hv_AngleradtrAbs);
                                hv_Sintr.Dispose();
                                HOperatorSet.TupleSin(hv_AngleradtrAbs, out hv_Sintr);
                                hv_Costr.Dispose();
                                HOperatorSet.TupleCos(hv_AngleradtrAbs, out hv_Costr);
                                hv_AbsSintr.Dispose();
                                HOperatorSet.TupleAbs(hv_Sintr, out hv_AbsSintr);
                                hv_AbsCostr.Dispose();
                                HOperatorSet.TupleAbs(hv_Costr, out hv_AbsCostr);
                                //hom_mat2d_translate (HomMat2DIdentity, 111.72, 111.72, HomMatTransPreR)
                                hv_DistanceRPre.Dispose();
                                hv_DistanceRPre = 158.6;
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_DistanceRPre * hv_AbsSintr,
                                        -(hv_DistanceRPre * hv_AbsCostr), out hv_HomMatTransPreR);
                                }


                                ho_SelectedContoursrnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrprenew, out ho_SelectedContoursrnew,
                                    hv_HomMatTransPreR);
                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourdowrnew,
                                    3);

                                //
                                //DistanceRPre 为边长
                                //X轴  先向左 移动  meanColst - meanColsr
                                //Y轴  先移动 meanRowst -  RowBeginmidr
                                //逆时针旋转90度，   右侧相机左侧跟上侧相机左侧相对。上侧相机采集图像是否需要以中间线中点纵向翻转？
                                //根据新的图形SelectedContoursrfinal，中间线X值，向 上侧相机中间线右边的点对齐，右移 maxColst - meanmidColsnew
                                //然后再向右移动 DistanceRPre * Costl  再向下 移动 DistanceRPre * Sintl
                                //Right3DLineX 为旋转后，左3D相机拍出来的方棒上边缘的X位置
                                //Right3DX 为3D相机线所在位置，这个位置为新的方棒，左侧3D相机移动拟合的位置。
                                //Right3DLineX 为旋转后，右侧3D相机拍出来的方棒右边缘的X位置
                                //Right3DX X轴相对上侧相机采集图像中点的距离 meanColst 为 上次相机采集图像中心点X坐标
                                //Right3DY Y轴相对上侧相机激光口所在水平线的Y轴相对距离

                                hv_RowsnewMidr.Dispose(); hv_ColsnewMidr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidrnew, out hv_RowsnewMidr, out hv_ColsnewMidr);
                                hv_Right3DLineX.Dispose();
                                HOperatorSet.TupleMean(hv_ColsnewMidr, out hv_Right3DLineX);
                                hv_AbsRight3DDistance.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsr, out hv_AbsRight3DDistance);

                                hv_Right3DX.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_Right3DX = hv_meanColst - (hv_Right3DLineX - hv_AbsRight3DDistance);
                                }
                                hv_AbsRight3DX.Dispose();
                                HOperatorSet.TupleAbs(hv_Right3DX, out hv_AbsRight3DX);
                                hv_Right3DX.Dispose();
                                hv_Right3DX = new HTuple(hv_AbsRight3DX);
                                hv_Right3DLineY.Dispose();
                                HOperatorSet.TupleMean(hv_RowsnewMidr, out hv_Right3DLineY);
                                hv_Right3DY.Dispose();
                                hv_Right3DY = new HTuple(hv_Right3DLineY);
                                hv_RightTDistance.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_RightTDistance = ((hv_DistanceRPre * hv_AbsCostr) + hv_AbsRight3DDistance) + ((hv_maxColst - hv_minColst) / 2);
                                }




                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, Nrleft, Ncleft, Distleft)
                                hv_RowBeginleft.Dispose(); hv_ColBeginleft.Dispose(); hv_RowEndleft.Dispose(); hv_ColEndleft.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourleft, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginleft, out hv_ColBeginleft, out hv_RowEndleft,
                                    out hv_ColEndleft, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_RowBeginrigt.Dispose(); hv_ColBeginrigt.Dispose(); hv_RowEndrigt.Dispose(); hv_ColEndrigt.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigt.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourrigt, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginrigt, out hv_ColBeginrigt, out hv_RowEndrigt,
                                    out hv_ColEndrigt, out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigt);
                                hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoprnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl,
                                    out hv_ColEndlefl, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourdowrnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr,
                                    out hv_ColEndlefr, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);

                                hv_RowIntersectiontr.Dispose(); hv_ColIntersectiontr.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefl, hv_ColBeginlefl, hv_RowEndlefl,
                                    hv_ColEndlefl, hv_RowBeginlefr, hv_ColBeginlefr, hv_RowEndlefr,
                                    hv_ColEndlefr, out hv_RowIntersectiontr, out hv_ColIntersectiontr,
                                    out hv_IsOverlapping);
                                hv_RowIntersectiont.Dispose(); hv_ColIntersectiont.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginleft, hv_ColBeginleft, hv_RowEndleft,
                                    hv_ColEndleft, hv_RowBeginrigt, hv_ColBeginrigt, hv_RowEndrigt,
                                    hv_ColEndrigt, out hv_RowIntersectiont, out hv_ColIntersectiont,
                                    out hv_IsOverlapping);
                                hv_DistanceRT.Dispose();
                                HOperatorSet.DistancePp(hv_RowIntersectiontr, hv_ColIntersectiontr,
                                    hv_RowIntersectiont, hv_ColIntersectiont, out hv_DistanceRT);
                            }
                            else
                            {
                                //与上3D相机，左侧对齐
                                hv_Arear.Dispose(); hv_Rowr.Dispose(); hv_Columnr.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursr, out hv_Arear, out hv_Rowr,
                                    out hv_Columnr, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowst - hv_meanRowsr,
                                        hv_minColst - hv_minColsr, out hv_HomMatTransPreR);
                                }
                                ho_SelectedContoursrPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursr, out ho_SelectedContoursrPre,
                                    hv_HomMatTransPreR);

                                //顺时针旋转90度， 中间的线的中点为旋转中心
                                hv_Arear.Dispose(); hv_RowPrer.Dispose(); hv_ColumnPrer.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursrPre, out hv_Arear, out hv_RowPrer,
                                    out hv_ColumnPrer, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DRRotate.Dispose();
                                    HOperatorSet.HomMat2dRotate(hv_HomMat2DIdentity, (new HTuple(90)).TupleRad()
                                        , hv_RowPrer.TupleSelect(1), hv_ColumnPrer.TupleSelect(1), out hv_HomMat2DRRotate);
                                }
                                ho_SelectedContoursrfinal.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrPre, out ho_SelectedContoursrfinal,
                                    hv_HomMat2DRRotate);

                                //沿着SelectedContourslfinal 中间线左侧点，以X轴为中心，进行水平反转
                                //上相机右侧与左侧相机左侧对应，所以需要反转
                                hv_Arear.Dispose(); hv_RowPrer.Dispose(); hv_ColumnPrer.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursrfinal, out hv_Arear,
                                    out hv_RowPrer, out hv_ColumnPrer, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DScale.Dispose();
                                    HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_RowPrer.TupleSelect(
                                        1), hv_ColumnPrer.TupleSelect(1), out hv_HomMat2DScale);
                                }
                                ho_SelectedContoursrnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrfinal, out ho_SelectedContoursrnewpre,
                                    hv_HomMat2DScale);


                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourdowrnew,
                                    3);



                                hv_RowBeginmidnewr.Dispose(); hv_ColBeginmidnewr.Dispose(); hv_RowEndmidnewr.Dispose(); hv_ColEndmidnewr.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmidrnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmidnewr, out hv_ColBeginmidnewr, out hv_RowEndmidnewr,
                                    out hv_ColEndmidnewr, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_Rowmidrmin.Dispose();
                                HOperatorSet.TupleMin2(hv_RowBeginmidnewr, hv_RowEndmidnewr, out hv_Rowmidrmin);
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowst - hv_Rowmidrmin;
                                }
                                hv_Rowsrmidnew.Dispose(); hv_Colsrmidnew.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidrnew, out hv_Rowsrmidnew, out hv_Colsrmidnew);
                                hv_meanmidColsnew.Dispose();
                                HOperatorSet.TupleMean(hv_Colsrmidnew, out hv_meanmidColsnew);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_minColst - hv_meanmidColsnew,
                                        out hv_HomMatTransPreR);
                                }
                                ho_SelectedContoursrprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrnewpre, out ho_SelectedContoursrprenew,
                                    hv_HomMatTransPreR);

                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourdowrnew,
                                    3);

                                hv_RowsTopr.Dispose(); hv_ColsTopr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourtoprnew, out hv_RowsTopr, out hv_ColsTopr);
                                hv_RowsDowr.Dispose(); hv_ColsDowr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourdowrnew, out hv_RowsDowr, out hv_ColsDowr);

                                hv_meanTopr.Dispose();
                                HOperatorSet.TupleMean(hv_RowsTopr, out hv_meanTopr);
                                hv_meanDowr.Dispose();
                                HOperatorSet.TupleMean(hv_RowsDowr, out hv_meanDowr);

                                if ((int)(new HTuple(hv_meanTopr.TupleGreater(hv_meanDowr))) != 0)
                                {
                                    ho_contourtmp.Dispose();
                                    ho_contourtmp = new HObject(ho_contourtoprnew);
                                    ho_contourtoprnew.Dispose();
                                    ho_contourtoprnew = new HObject(ho_contourdowrnew);
                                    ho_contourdowrnew.Dispose();
                                    ho_contourdowrnew = new HObject(ho_contourtmp);
                                }

                                //新的图形SelectedContoursrfinal，上侧边的角度要与 上3D相机采集图像左侧边角度对齐
                                hv_RowBegintopr.Dispose(); hv_ColBegintopr.Dispose(); hv_RowEndtopr.Dispose(); hv_ColEndtopr.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoprnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopr, out hv_ColBegintopr, out hv_RowEndtopr,
                                    out hv_ColEndtopr, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                if ((int)(new HTuple(hv_RowBegintopr.TupleGreater(hv_RowEndtopr))) != 0)
                                {
                                    hv_Angleradtr.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowBegintopr, hv_ColBegintopr, hv_RowEndtopr,
                                        hv_ColEndtopr, out hv_Angleradtr);
                                }
                                else
                                {
                                    hv_Angleradtr.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowEndtopr, hv_ColEndtopr, hv_RowBegintopr,
                                        hv_ColBegintopr, out hv_Angleradtr);
                                }
                                hv_Degrtr.Dispose();
                                HOperatorSet.TupleDeg(hv_Angleradtr, out hv_Degrtr);
                                hv_DegrtrAbs.Dispose();
                                HOperatorSet.TupleAbs(hv_Degrtr, out hv_DegrtrAbs);
                                hv_DegSubR.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubR = hv_DegtlAbs - hv_DegrtrAbs;
                                }

                                //if (DegSubR > 0.1 or DegSubR < -0.1)
                                //area_center_xld (SelectedContoursrprenew, Arear, Rowr, Columnr, PointOrderr)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(DegSubR), Columnr[1], Rowr[1], HomMat2DRRotate)
                                //affine_trans_contour_xld (SelectedContoursrprenew, SelectedContoursrfinalnew, HomMat2DRRotate)
                                //select_obj (SelectedContoursrfinalnew, contourtoprnew, 1)
                                //select_obj (SelectedContoursrfinalnew, contourmidrnew, 2)
                                //select_obj (SelectedContoursrfinalnew, contourdowrnew, 3)
                                //get_contour_xld (contourtoprnew, RowsTopr, ColsTopr)
                                //get_contour_xld (contourdowrnew, RowsDowr, ColsDowr)
                                //tuple_mean (RowsTopr, meanTopr)
                                //tuple_mean (RowsDowr, meanDowr)
                                //if (meanTopr > meanDowr)
                                //contourtmp := contourtoprnew
                                //contourtoprnew := contourdowrnew
                                //contourdowrnew := contourtmp
                                //endif
                                //fit_line_contour_xld (contourtoprnew, 'tukey', -1, 10, 5, 2, RowBegintopr, ColBegintopr, RowEndtopr, ColEndtopr, Nrleft, Ncleft, Distleft)
                                //if (RowBegintopr > RowEndtopr)
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowBegintopr, ColBegintopr, RowEndtopr, ColEndtopr, Angleradtr)
                                //else
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowEndtopr, ColEndtopr, RowBegintopr, ColBegintopr, Angleradtr)
                                //endif
                                //tuple_deg (Angleradtr, Degrtr)
                                //tuple_abs (Degrtr, DegrtrAbs)
                                //DegSubR := DegtlAbs - DegrtrAbs
                                //SelectedContoursrprenew := SelectedContoursrfinalnew
                                //endif


                                //标定块拟合，需要根据标定块边长计算左移距离，  ( 182 - 12 ) * sin45 = 111.72, 移动后就是左边3D相机的具体位置
                                //( 182 - 12 ) * sin45  + meanRowsl , 下次按照这个思路进行偏移，根据新的方棒左3D相机采集的高度，进行
                                //计算，例如标定快高度为 -13,  新的方棒标定块 -11， 那么左移动就是（-11 + 13） = 2， 左移动 111.72 + 2
                                //则与3D位置相同。要沿着上面3D，左边的角度进行移动。 右边3D处理与此类似。

                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourmidrnew,
                                    2);
                                hv_RowsMidr.Dispose(); hv_ColsMidr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidrnew, out hv_RowsMidr, out hv_ColsMidr);
                                hv_MeanMidrx.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidr, out hv_MeanMidrx);



                                hv_AngleradtrAbs.Dispose();
                                HOperatorSet.TupleAbs(hv_Angleradttl, out hv_AngleradtrAbs);
                                hv_Sintr.Dispose();
                                HOperatorSet.TupleSin(hv_AngleradtrAbs, out hv_Sintr);
                                hv_Costr.Dispose();
                                HOperatorSet.TupleCos(hv_AngleradtrAbs, out hv_Costr);
                                hv_AbsSintr.Dispose();
                                HOperatorSet.TupleAbs(hv_Sintr, out hv_AbsSintr);
                                hv_AbsCostr.Dispose();
                                HOperatorSet.TupleAbs(hv_Costr, out hv_AbsCostr);

                                //Right3DX + meanRowsr 为 contourmidrnew 需要移动到的位置 X坐标
                                //tuple_abs (meanRowsr, AbsRight3DDistance)
                                hv_AbsmeanRowsr.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsr, out hv_AbsmeanRowsr);
                                hv_DistanceXPre.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DistanceXPre = (hv_RightTDistance - hv_AbsmeanRowsr) - ((hv_maxColst - hv_minColst) / 2);
                                }
                                hv_DistanceRPre.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DistanceRPre = hv_DistanceXPre / hv_AbsCostr;
                                }


                                //DistanceRPre := 158.75
                                //hom_mat2d_translate (HomMat2DIdentity, 111.72, 111.72, HomMatTransPreR)

                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_DistanceRPre * hv_AbsSintr,
                                        -(hv_DistanceRPre * hv_AbsCostr), out hv_HomMatTransPreR);
                                }


                                ho_SelectedContoursrnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrprenew, out ho_SelectedContoursrnew,
                                    hv_HomMatTransPreR);
                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourdowrnew,
                                    3);

                                //
                                //DistanceRPre 为边长
                                //X轴  先向左 移动  meanColst - meanColsr
                                //Y轴  先移动 meanRowst -  RowBeginmidr
                                //逆时针旋转90度，   右侧相机左侧跟上侧相机左侧相对。上侧相机采集图像是否需要以中间线中点纵向翻转？
                                //根据新的图形SelectedContoursrfinal，中间线X值，向 上侧相机中间线右边的点对齐，右移 maxColst - meanmidColsnew
                                //然后再向右移动 DistanceRPre * Costl  再向下 移动 DistanceRPre * Sintl
                                //Right3DLineX 为旋转后，左3D相机拍出来的方棒上边缘的X位置
                                //Right3DX 为3D相机线所在位置，这个位置为新的方棒，左侧3D相机移动拟合的位置。
                                //Right3DLineX 为旋转后，右侧3D相机拍出来的方棒右边缘的X位置
                                //Right3DX 为3D相机线所在位置，这个位置为新的方棒，左侧3D相机移动拟合的位置。



                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, Nrleft, Ncleft, Distleft)
                                hv_RowBeginleft.Dispose(); hv_ColBeginleft.Dispose(); hv_RowEndleft.Dispose(); hv_ColEndleft.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourleft, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginleft, out hv_ColBeginleft, out hv_RowEndleft,
                                    out hv_ColEndleft, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_RowBeginrigt.Dispose(); hv_ColBeginrigt.Dispose(); hv_RowEndrigt.Dispose(); hv_ColEndrigt.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigt.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourrigt, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginrigt, out hv_ColBeginrigt, out hv_RowEndrigt,
                                    out hv_ColEndrigt, out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigt);
                                hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoprnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl,
                                    out hv_ColEndlefl, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourdowrnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr,
                                    out hv_ColEndlefr, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);

                                hv_RowIntersectiontr.Dispose(); hv_ColIntersectiontr.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefl, hv_ColBeginlefl, hv_RowEndlefl,
                                    hv_ColEndlefl, hv_RowBeginlefr, hv_ColBeginlefr, hv_RowEndlefr,
                                    hv_ColEndlefr, out hv_RowIntersectiontr, out hv_ColIntersectiontr,
                                    out hv_IsOverlapping);
                                hv_RowIntersectiont.Dispose(); hv_ColIntersectiont.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginleft, hv_ColBeginleft, hv_RowEndleft,
                                    hv_ColEndleft, hv_RowBeginrigt, hv_ColBeginrigt, hv_RowEndrigt,
                                    hv_ColEndrigt, out hv_RowIntersectiont, out hv_ColIntersectiont,
                                    out hv_IsOverlapping);
                                hv_DistanceRT.Dispose();
                                HOperatorSet.DistancePp(hv_RowIntersectiontr, hv_ColIntersectiontr,
                                    hv_RowIntersectiont, hv_ColIntersectiont, out hv_DistanceRT);
                            }


                            //以上侧3D相机激光线侧的坐标为原点， 根据标定块，进行拟合。求出右侧3D相机的坐标位置
                            if ((int)(new HTuple(hv_RightDDistance.TupleEqual(0))) != 0)
                            {
                                //与上3D相机，左侧对齐
                                hv_Arear.Dispose(); hv_Rowr.Dispose(); hv_Columnr.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursr, out hv_Arear, out hv_Rowr,
                                    out hv_Columnr, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowsd - hv_meanRowsr,
                                        hv_minColsd - hv_minColsr, out hv_HomMatTransPreR);
                                }
                                ho_SelectedContoursrPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursr, out ho_SelectedContoursrPre,
                                    hv_HomMatTransPreR);

                                //顺时针旋转90度， 中间的线的中点为旋转中心
                                hv_Arear.Dispose(); hv_RowPrer.Dispose(); hv_ColumnPrer.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursrPre, out hv_Arear, out hv_RowPrer,
                                    out hv_ColumnPrer, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DRRotate.Dispose();
                                    HOperatorSet.HomMat2dRotate(hv_HomMat2DIdentity, (new HTuple(-90)).TupleRad()
                                        , hv_RowPrer.TupleSelect(1), hv_ColumnPrer.TupleSelect(1), out hv_HomMat2DRRotate);
                                }
                                ho_SelectedContoursrfinal.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrPre, out ho_SelectedContoursrfinal,
                                    hv_HomMat2DRRotate);

                                //沿着SelectedContourslfinal 中间线左侧点，以X轴为中心，进行水平反转
                                //上相机右侧与左侧相机左侧对应，所以需要反转
                                hv_Arear.Dispose(); hv_RowPrer.Dispose(); hv_ColumnPrer.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursrfinal, out hv_Arear,
                                    out hv_RowPrer, out hv_ColumnPrer, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DScale.Dispose();
                                    HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_RowPrer.TupleSelect(
                                        1), hv_ColumnPrer.TupleSelect(1), out hv_HomMat2DScale);
                                }
                                ho_SelectedContoursrnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrfinal, out ho_SelectedContoursrnewpre,
                                    hv_HomMat2DScale);


                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourdowrnew,
                                    3);



                                hv_RowBeginmidnewr.Dispose(); hv_ColBeginmidnewr.Dispose(); hv_RowEndmidnewr.Dispose(); hv_ColEndmidnewr.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmidrnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmidnewr, out hv_ColBeginmidnewr, out hv_RowEndmidnewr,
                                    out hv_ColEndmidnewr, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_Rowmidrmin.Dispose();
                                HOperatorSet.TupleMin2(hv_RowBeginmidnewr, hv_RowEndmidnewr, out hv_Rowmidrmin);
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowsd - hv_Rowmidrmin;
                                }
                                hv_Rowsrmidnew.Dispose(); hv_Colsrmidnew.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidrnew, out hv_Rowsrmidnew, out hv_Colsrmidnew);
                                hv_meanmidColsnew.Dispose();
                                HOperatorSet.TupleMean(hv_Colsrmidnew, out hv_meanmidColsnew);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_maxColsd - hv_meanmidColsnew,
                                        out hv_HomMatTransPreR);
                                }
                                ho_SelectedContoursrprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrnewpre, out ho_SelectedContoursrprenew,
                                    hv_HomMatTransPreR);

                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourdowrnew,
                                    3);

                                hv_RowsTopr.Dispose(); hv_ColsTopr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourtoprnew, out hv_RowsTopr, out hv_ColsTopr);
                                hv_RowsDowr.Dispose(); hv_ColsDowr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourdowrnew, out hv_RowsDowr, out hv_ColsDowr);

                                hv_meanTopr.Dispose();
                                HOperatorSet.TupleMean(hv_RowsTopr, out hv_meanTopr);
                                hv_meanDowr.Dispose();
                                HOperatorSet.TupleMean(hv_RowsDowr, out hv_meanDowr);

                                if ((int)(new HTuple(hv_meanTopr.TupleGreater(hv_meanDowr))) != 0)
                                {
                                    ho_contourtmp.Dispose();
                                    ho_contourtmp = new HObject(ho_contourtoprnew);
                                    ho_contourtoprnew.Dispose();
                                    ho_contourtoprnew = new HObject(ho_contourdowrnew);
                                    ho_contourdowrnew.Dispose();
                                    ho_contourdowrnew = new HObject(ho_contourtmp);
                                }

                                //新的图形SelectedContoursrfinal，上侧边的角度要与 上3D相机采集图像左侧边角度对齐
                                hv_RowBegintopr.Dispose(); hv_ColBegintopr.Dispose(); hv_RowEndtopr.Dispose(); hv_ColEndtopr.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoprnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopr, out hv_ColBegintopr, out hv_RowEndtopr,
                                    out hv_ColEndtopr, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                if ((int)(new HTuple(hv_RowBegintopr.TupleGreater(hv_RowEndtopr))) != 0)
                                {
                                    hv_Angleradtr.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowBegintopr, hv_ColBegintopr, hv_RowEndtopr,
                                        hv_ColEndtopr, out hv_Angleradtr);
                                }
                                else
                                {
                                    hv_Angleradtr.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowEndtopr, hv_ColEndtopr, hv_RowBegintopr,
                                        hv_ColBegintopr, out hv_Angleradtr);
                                }
                                hv_Degrtr.Dispose();
                                HOperatorSet.TupleDeg(hv_Angleradtr, out hv_Degrtr);
                                hv_DegrtrAbs.Dispose();
                                HOperatorSet.TupleAbs(hv_Degrtr, out hv_DegrtrAbs);
                                hv_DegSubR.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubR = hv_DegtlAbs - hv_DegrtrAbs;
                                }

                                //if (DegSubR > 0.1 or DegSubR < -0.1)
                                //area_center_xld (SelectedContoursrprenew, Arear, Rowr, Columnr, PointOrderr)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(DegSubR), Columnr[1], Rowr[1], HomMat2DRRotate)
                                //affine_trans_contour_xld (SelectedContoursrprenew, SelectedContoursrfinalnew, HomMat2DRRotate)
                                //select_obj (SelectedContoursrfinalnew, contourtoprnew, 1)
                                //select_obj (SelectedContoursrfinalnew, contourmidrnew, 2)
                                //select_obj (SelectedContoursrfinalnew, contourdowrnew, 3)

                                //get_contour_xld (contourtoprnew, RowsTopr, ColsTopr)
                                //get_contour_xld (contourdowrnew, RowsDowr, ColsDowr)

                                //tuple_mean (RowsTopr, meanTopr)
                                //tuple_mean (RowsDowr, meanDowr)

                                //if (meanTopr > meanDowr)
                                //contourtmp := contourtoprnew
                                //contourtoprnew := contourdowrnew
                                //contourdowrnew := contourtmp
                                //endif

                                //fit_line_contour_xld (contourtoprnew, 'tukey', -1, 10, 5, 2, RowBegintopr, ColBegintopr, RowEndtopr, ColEndtopr, Nrleft, Ncleft, Distleft)

                                //if (RowBegintopr > RowEndtopr)
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowBegintopr, ColBegintopr, RowEndtopr, ColEndtopr, Angleradtr)
                                //else
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowEndtopr, ColEndtopr, RowBegintopr, ColBegintopr, Angleradtr)
                                //endif
                                //tuple_deg (Angleradtr, Degrtr)
                                //tuple_abs (Degrtr, DegrtrAbs)
                                //DegSubR := DegtlAbs -  DegrtrAbs
                                //SelectedContoursrprenew := SelectedContoursrfinalnew

                                //endif


                                //标定块拟合，需要根据标定块边长计算左移距离，  ( 182 - 12 ) * sin45 = 111.72, 移动后就是左边3D相机的具体位置
                                //( 182 - 12 ) * sin45  + meanRowsl , 下次按照这个思路进行偏移，根据新的方棒左3D相机采集的高度，进行
                                //计算，例如标定快高度为 -13,  新的方棒标定块 -11， 那么左移动就是（-11 + 13） = 2， 左移动 111.72 + 2
                                //则与3D位置相同。要沿着上面3D，左边的角度进行移动。 右边3D处理与此类似。

                                hv_AngleradtrAbs.Dispose();
                                HOperatorSet.TupleAbs(hv_Angleraddtr, out hv_AngleradtrAbs);
                                hv_Sintr.Dispose();
                                HOperatorSet.TupleSin(hv_AngleradtrAbs, out hv_Sintr);
                                hv_Costr.Dispose();
                                HOperatorSet.TupleCos(hv_AngleradtrAbs, out hv_Costr);
                                hv_AbsSintr.Dispose();
                                HOperatorSet.TupleAbs(hv_Sintr, out hv_AbsSintr);
                                hv_AbsCostr.Dispose();
                                HOperatorSet.TupleAbs(hv_Costr, out hv_AbsCostr);
                                //hom_mat2d_translate (HomMat2DIdentity, 111.72, 111.72, HomMatTransPreR)
                                hv_DistanceRPre.Dispose();
                                hv_DistanceRPre = 158.26;
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_DistanceRPre * hv_AbsSintr,
                                        hv_DistanceRPre * hv_AbsCostr, out hv_HomMatTransPreR);
                                }


                                ho_SelectedContoursrnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrprenew, out ho_SelectedContoursrnew,
                                    hv_HomMatTransPreR);
                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourdowrnew,
                                    3);

                                //
                                //DistanceRPre 为边长
                                //X轴  先向左 移动  meanColst - meanColsr
                                //Y轴  先移动 meanRowst -  RowBeginmidr
                                //逆时针旋转90度，   右侧相机左侧跟上侧相机左侧相对。上侧相机采集图像是否需要以中间线中点纵向翻转？
                                //根据新的图形SelectedContoursrfinal，中间线X值，向 上侧相机中间线右边的点对齐，右移 maxColst - meanmidColsnew
                                //然后再向右移动 DistanceRPre * Costl  再向下 移动 DistanceRPre * Sintl
                                //Right3DLineX 为旋转后，左3D相机拍出来的方棒上边缘的X位置
                                //Right3DX 为3D相机线所在位置，这个位置为新的方棒，左侧3D相机移动拟合的位置。
                                //Right3DLineX 为旋转后，右侧3D相机拍出来的方棒右边缘的X位置
                                //Right3DX X轴相对上侧相机采集图像中点的距离 meanColst 为 上次相机采集图像中心点X坐标
                                //Right3DY Y轴相对上侧相机激光口所在水平线的Y轴相对距离

                                hv_RowsnewMidr.Dispose(); hv_ColsnewMidr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidrnew, out hv_RowsnewMidr, out hv_ColsnewMidr);
                                hv_Right3DLineX.Dispose();
                                HOperatorSet.TupleMean(hv_ColsnewMidr, out hv_Right3DLineX);
                                hv_AbsRight3DDistance.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsr, out hv_AbsRight3DDistance);

                                hv_Right3DX.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_Right3DX = hv_meanColst - (hv_Right3DLineX - hv_AbsRight3DDistance);
                                }
                                hv_AbsRight3DX.Dispose();
                                HOperatorSet.TupleAbs(hv_Right3DX, out hv_AbsRight3DX);
                                hv_Right3DX.Dispose();
                                hv_Right3DX = new HTuple(hv_AbsRight3DX);
                                hv_Right3DLineY.Dispose();
                                HOperatorSet.TupleMean(hv_RowsnewMidr, out hv_Right3DLineY);
                                hv_Right3DY.Dispose();
                                hv_Right3DY = new HTuple(hv_Right3DLineY);
                                hv_RightDDistance.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_RightDDistance = ((hv_DistanceRPre * hv_AbsCostr) + hv_AbsRight3DDistance) + ((hv_maxColst - hv_minColst) / 2);
                                }




                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, Nrleft, Ncleft, Distleft)
                                hv_RowBeginlefd.Dispose(); hv_ColBeginlefd.Dispose(); hv_RowEndlefd.Dispose(); hv_ColEndlefd.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distlefd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourlefd, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginlefd, out hv_ColBeginlefd, out hv_RowEndlefd,
                                    out hv_ColEndlefd, out hv_Nrleft, out hv_Ncleft, out hv_Distlefd);
                                hv_RowBeginrigd.Dispose(); hv_ColBeginrigd.Dispose(); hv_RowEndrigd.Dispose(); hv_ColEndrigd.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourrigd, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginrigd, out hv_ColBeginrigd, out hv_RowEndrigd,
                                    out hv_ColEndrigd, out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigd);
                                hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoprnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl,
                                    out hv_ColEndlefl, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourdowrnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr,
                                    out hv_ColEndlefr, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);

                                hv_RowIntersectiontr.Dispose(); hv_ColIntersectiontr.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefl, hv_ColBeginlefl, hv_RowEndlefl,
                                    hv_ColEndlefl, hv_RowBeginlefr, hv_ColBeginlefr, hv_RowEndlefr,
                                    hv_ColEndlefr, out hv_RowIntersectiontr, out hv_ColIntersectiontr,
                                    out hv_IsOverlapping);
                                hv_RowIntersectiont.Dispose(); hv_ColIntersectiont.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefd, hv_ColBeginlefd, hv_RowEndlefd,
                                    hv_ColEndlefd, hv_RowBeginrigd, hv_ColBeginrigd, hv_RowEndrigd,
                                    hv_ColEndrigd, out hv_RowIntersectiont, out hv_ColIntersectiont,
                                    out hv_IsOverlapping);
                                hv_DistanceRD.Dispose();
                                HOperatorSet.DistancePp(hv_RowIntersectiontr, hv_ColIntersectiontr,
                                    hv_RowIntersectiont, hv_ColIntersectiont, out hv_DistanceRD);
                            }
                            else
                            {
                                //与上3D相机，左侧对齐
                                hv_Arear.Dispose(); hv_Rowr.Dispose(); hv_Columnr.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursr, out hv_Arear, out hv_Rowr,
                                    out hv_Columnr, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowsd - hv_meanRowsr,
                                        hv_minColsd - hv_minColsr, out hv_HomMatTransPreR);
                                }
                                ho_SelectedContoursrPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursr, out ho_SelectedContoursrPre,
                                    hv_HomMatTransPreR);

                                //顺时针旋转90度， 中间的线的中点为旋转中心
                                hv_Arear.Dispose(); hv_RowPrer.Dispose(); hv_ColumnPrer.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursrPre, out hv_Arear, out hv_RowPrer,
                                    out hv_ColumnPrer, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DRRotate.Dispose();
                                    HOperatorSet.HomMat2dRotate(hv_HomMat2DIdentity, (new HTuple(-90)).TupleRad()
                                        , hv_RowPrer.TupleSelect(1), hv_ColumnPrer.TupleSelect(1), out hv_HomMat2DRRotate);
                                }
                                ho_SelectedContoursrfinal.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrPre, out ho_SelectedContoursrfinal,
                                    hv_HomMat2DRRotate);

                                //沿着SelectedContourslfinal 中间线左侧点，以X轴为中心，进行水平反转
                                //上相机右侧与左侧相机左侧对应，所以需要反转
                                hv_Arear.Dispose(); hv_RowPrer.Dispose(); hv_ColumnPrer.Dispose(); hv_PointOrderr.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursrfinal, out hv_Arear,
                                    out hv_RowPrer, out hv_ColumnPrer, out hv_PointOrderr);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMat2DScale.Dispose();
                                    HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_RowPrer.TupleSelect(
                                        1), hv_ColumnPrer.TupleSelect(1), out hv_HomMat2DScale);
                                }
                                ho_SelectedContoursrnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrfinal, out ho_SelectedContoursrnewpre,
                                    hv_HomMat2DScale);


                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourdowrnew,
                                    3);



                                hv_RowBeginmidnewr.Dispose(); hv_ColBeginmidnewr.Dispose(); hv_RowEndmidnewr.Dispose(); hv_ColEndmidnewr.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmidrnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmidnewr, out hv_ColBeginmidnewr, out hv_RowEndmidnewr,
                                    out hv_ColEndmidnewr, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_Rowmidrmin.Dispose();
                                HOperatorSet.TupleMin2(hv_RowBeginmidnewr, hv_RowEndmidnewr, out hv_Rowmidrmin);
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowsd - hv_Rowmidrmin;
                                }
                                hv_Rowsrmidnew.Dispose(); hv_Colsrmidnew.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidrnew, out hv_Rowsrmidnew, out hv_Colsrmidnew);
                                hv_meanmidColsnew.Dispose();
                                HOperatorSet.TupleMean(hv_Colsrmidnew, out hv_meanmidColsnew);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_maxColsd - hv_meanmidColsnew,
                                        out hv_HomMatTransPreR);
                                }
                                ho_SelectedContoursrprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrnewpre, out ho_SelectedContoursrprenew,
                                    hv_HomMatTransPreR);

                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourdowrnew,
                                    3);

                                hv_RowsTopr.Dispose(); hv_ColsTopr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourtoprnew, out hv_RowsTopr, out hv_ColsTopr);
                                hv_RowsDowr.Dispose(); hv_ColsDowr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourdowrnew, out hv_RowsDowr, out hv_ColsDowr);

                                hv_meanTopr.Dispose();
                                HOperatorSet.TupleMean(hv_RowsTopr, out hv_meanTopr);
                                hv_meanDowr.Dispose();
                                HOperatorSet.TupleMean(hv_RowsDowr, out hv_meanDowr);

                                if ((int)(new HTuple(hv_meanTopr.TupleGreater(hv_meanDowr))) != 0)
                                {
                                    ho_contourtmp.Dispose();
                                    ho_contourtmp = new HObject(ho_contourtoprnew);
                                    ho_contourtoprnew.Dispose();
                                    ho_contourtoprnew = new HObject(ho_contourdowrnew);
                                    ho_contourdowrnew.Dispose();
                                    ho_contourdowrnew = new HObject(ho_contourtmp);
                                }

                                //新的图形SelectedContoursrfinal，上侧边的角度要与 上3D相机采集图像左侧边角度对齐
                                hv_RowBegintopr.Dispose(); hv_ColBegintopr.Dispose(); hv_RowEndtopr.Dispose(); hv_ColEndtopr.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoprnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopr, out hv_ColBegintopr, out hv_RowEndtopr,
                                    out hv_ColEndtopr, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                if ((int)(new HTuple(hv_RowBegintopr.TupleGreater(hv_RowEndtopr))) != 0)
                                {
                                    hv_Angleradtr.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowBegintopr, hv_ColBegintopr, hv_RowEndtopr,
                                        hv_ColEndtopr, out hv_Angleradtr);
                                }
                                else
                                {
                                    hv_Angleradtr.Dispose();
                                    HOperatorSet.AngleLl(hv_RowBeginmidt, hv_ColBeginmidt, hv_RowEndmidt,
                                        hv_ColEndmidt, hv_RowEndtopr, hv_ColEndtopr, hv_RowBegintopr,
                                        hv_ColBegintopr, out hv_Angleradtr);
                                }
                                hv_Degrtr.Dispose();
                                HOperatorSet.TupleDeg(hv_Angleradtr, out hv_Degrtr);
                                hv_DegrtrAbs.Dispose();
                                HOperatorSet.TupleAbs(hv_Degrtr, out hv_DegrtrAbs);
                                hv_DegSubR.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubR = hv_DegtlAbs - hv_DegrtrAbs;
                                }

                                //if (DegSubR > 0.1 or DegSubR < -0.1)
                                //area_center_xld (SelectedContoursrprenew, Arear, Rowr, Columnr, PointOrderr)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(DegSubR), Columnr[1], Rowr[1], HomMat2DRRotate)
                                //affine_trans_contour_xld (SelectedContoursrprenew, SelectedContoursrfinalnew, HomMat2DRRotate)
                                //select_obj (SelectedContoursrfinalnew, contourtoprnew, 1)
                                //select_obj (SelectedContoursrfinalnew, contourmidrnew, 2)
                                //select_obj (SelectedContoursrfinalnew, contourdowrnew, 3)
                                //get_contour_xld (contourtoprnew, RowsTopr, ColsTopr)
                                //get_contour_xld (contourdowrnew, RowsDowr, ColsDowr)
                                //tuple_mean (RowsTopr, meanTopr)
                                //tuple_mean (RowsDowr, meanDowr)
                                //if (meanTopr > meanDowr)
                                //contourtmp := contourtoprnew
                                //contourtoprnew := contourdowrnew
                                //contourdowrnew := contourtmp
                                //endif
                                //fit_line_contour_xld (contourtoprnew, 'tukey', -1, 10, 5, 2, RowBegintopr, ColBegintopr, RowEndtopr, ColEndtopr, Nrleft, Ncleft, Distleft)
                                //if (RowBegintopr > RowEndtopr)
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowBegintopr, ColBegintopr, RowEndtopr, ColEndtopr, Angleradtr)
                                //else
                                //angle_ll (RowBeginmidt, ColBeginmidt, RowEndmidt, ColEndmidt, RowEndtopr, ColEndtopr, RowBegintopr, ColBegintopr, Angleradtr)
                                //endif
                                //tuple_deg (Angleradtr, Degrtr)
                                //tuple_abs (Degrtr, DegrtrAbs)
                                //DegSubR := DegtlAbs - DegrtrAbs
                                //SelectedContoursrprenew := SelectedContoursrfinalnew
                                //endif


                                //标定块拟合，需要根据标定块边长计算左移距离，  ( 182 - 12 ) * sin45 = 111.72, 移动后就是左边3D相机的具体位置
                                //( 182 - 12 ) * sin45  + meanRowsl , 下次按照这个思路进行偏移，根据新的方棒左3D相机采集的高度，进行
                                //计算，例如标定快高度为 -13,  新的方棒标定块 -11， 那么左移动就是（-11 + 13） = 2， 左移动 111.72 + 2
                                //则与3D位置相同。要沿着上面3D，左边的角度进行移动。 右边3D处理与此类似。

                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourmidrnew,
                                    2);
                                hv_RowsMidr.Dispose(); hv_ColsMidr.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmidrnew, out hv_RowsMidr, out hv_ColsMidr);
                                hv_MeanMidrx.Dispose();
                                HOperatorSet.TupleMean(hv_ColsMidr, out hv_MeanMidrx);


                                hv_AngleradtrAbs.Dispose();
                                HOperatorSet.TupleAbs(hv_Angleraddtr, out hv_AngleradtrAbs);
                                hv_Sintr.Dispose();
                                HOperatorSet.TupleSin(hv_AngleradtrAbs, out hv_Sintr);
                                hv_Costr.Dispose();
                                HOperatorSet.TupleCos(hv_AngleradtrAbs, out hv_Costr);
                                hv_AbsSintr.Dispose();
                                HOperatorSet.TupleAbs(hv_Sintr, out hv_AbsSintr);
                                hv_AbsCostr.Dispose();
                                HOperatorSet.TupleAbs(hv_Costr, out hv_AbsCostr);

                                //Right3DX + meanRowsr 为 contourmidrnew 需要移动到的位置 X坐标
                                //tuple_abs (meanRowsr, AbsRight3DDistance)
                                hv_AbsmeanRowsr.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsr, out hv_AbsmeanRowsr);
                                hv_DistanceXPre.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DistanceXPre = (hv_RightDDistance - hv_AbsmeanRowsr) - ((hv_maxColst - hv_minColst) / 2);
                                }
                                hv_DistanceRPre.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DistanceRPre = hv_DistanceXPre / hv_AbsCostr;
                                }


                                //DistanceRPre := 158.75
                                //hom_mat2d_translate (HomMat2DIdentity, 111.72, 111.72, HomMatTransPreR)

                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreR.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_DistanceRPre * hv_AbsSintr,
                                        hv_DistanceRPre * hv_AbsCostr, out hv_HomMatTransPreR);
                                }


                                ho_SelectedContoursrnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrprenew, out ho_SelectedContoursrnew,
                                    hv_HomMatTransPreR);
                                ho_contourtoprnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourtoprnew,
                                    1);
                                ho_contourmidrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourmidrnew,
                                    2);
                                ho_contourdowrnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourdowrnew,
                                    3);

                                //
                                //DistanceRPre 为边长
                                //X轴  先向左 移动  meanColst - meanColsr
                                //Y轴  先移动 meanRowst -  RowBeginmidr
                                //逆时针旋转90度，   右侧相机左侧跟上侧相机左侧相对。上侧相机采集图像是否需要以中间线中点纵向翻转？
                                //根据新的图形SelectedContoursrfinal，中间线X值，向 上侧相机中间线右边的点对齐，右移 maxColst - meanmidColsnew
                                //然后再向右移动 DistanceRPre * Costl  再向下 移动 DistanceRPre * Sintl
                                //Right3DLineX 为旋转后，左3D相机拍出来的方棒上边缘的X位置
                                //Right3DX 为3D相机线所在位置，这个位置为新的方棒，左侧3D相机移动拟合的位置。
                                //Right3DLineX 为旋转后，右侧3D相机拍出来的方棒右边缘的X位置
                                //Right3DX 为3D相机线所在位置，这个位置为新的方棒，左侧3D相机移动拟合的位置。



                                //fit_line_contour_xld (contourtoplnew, 'tukey', -1, 10, 5, 2, RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, Nrleft, Ncleft, Distleft)
                                hv_RowBeginlefd.Dispose(); hv_ColBeginlefd.Dispose(); hv_RowEndlefd.Dispose(); hv_ColEndlefd.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distlefd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourlefd, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginlefd, out hv_ColBeginlefd, out hv_RowEndlefd,
                                    out hv_ColEndlefd, out hv_Nrleft, out hv_Ncleft, out hv_Distlefd);
                                hv_RowBeginrigd.Dispose(); hv_ColBeginrigd.Dispose(); hv_RowEndrigd.Dispose(); hv_ColEndrigd.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourrigd, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginrigd, out hv_ColBeginrigd, out hv_RowEndrigd,
                                    out hv_ColEndrigd, out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigd);
                                hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtoprnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl,
                                    out hv_ColEndlefl, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourdowrnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr,
                                    out hv_ColEndlefr, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);

                                hv_RowIntersectiontr.Dispose(); hv_ColIntersectiontr.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefl, hv_ColBeginlefl, hv_RowEndlefl,
                                    hv_ColEndlefl, hv_RowBeginlefr, hv_ColBeginlefr, hv_RowEndlefr,
                                    hv_ColEndlefr, out hv_RowIntersectiontr, out hv_ColIntersectiontr,
                                    out hv_IsOverlapping);
                                hv_RowIntersectiont.Dispose(); hv_ColIntersectiont.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefd, hv_ColBeginlefd, hv_RowEndlefd,
                                    hv_ColEndlefd, hv_RowBeginrigd, hv_ColBeginrigd, hv_RowEndrigd,
                                    hv_ColEndrigd, out hv_RowIntersectiont, out hv_ColIntersectiont,
                                    out hv_IsOverlapping);
                                hv_DistanceRD.Dispose();
                                HOperatorSet.DistancePp(hv_RowIntersectiontr, hv_ColIntersectiontr,
                                    hv_RowIntersectiont, hv_ColIntersectiont, out hv_DistanceRD);
                            }

                            if ((int)(new HTuple(hv_DownDistance.TupleEqual(0))) != 0)
                            {
                                //以相机采集图像，中间线作为参考线有问题。需要以相机激光口横线为参照物，求出下3D相机相对上3D相机的纵向坐标。
                                //先与上侧相机采集的图像中间线的中点进行对齐

                                hv_Aread.Dispose(); hv_Rowd.Dispose(); hv_Columnd.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursd, out hv_Aread, out hv_Rowd,
                                    out hv_Columnd, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowst - hv_meanRowsd,
                                        hv_minColst - hv_minColsd, out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursdPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursd, out ho_SelectedContoursdPre,
                                    hv_HomMatTransPreD);

                                //以中间线做水平翻转
                                //注释部分继续研究，水平翻转过  沿着中线中点进行水平翻转  180
                                //area_center_xld (SelectedContoursdPre, Aread, Rowd, Columnd, PointOrderl)

                                //select_obj (SelectedContoursdPre, contourmiddnew, 2)
                                //get_contour_xld (contourmiddnew, RowsMidr, ColsMidr)
                                //tuple_mean (ColsMidr, MeanMidrx)
                                //tuple_mean (RowsMidr, MeanMidry)
                                //hom_mat2d_rotate (HomMat2DIdentity, 3.14, 56.2, -1, HomMat2DRotate)
                                //affine_trans_contour_xld (SelectedContoursdPre, SelectedContoursdnewpre, HomMat2DRotate)

                                hv_Aread.Dispose(); hv_RowPred.Dispose(); hv_ColumnPred.Dispose(); hv_PointOrderd.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursdPre, out hv_Aread, out hv_RowPred,
                                    out hv_ColumnPred, out hv_PointOrderd);
                                hv_HomMat2DScale.Dispose();
                                HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_meanRowsd,
                                    0, out hv_HomMat2DScale);
                                ho_SelectedContoursdnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursdPre, out ho_SelectedContoursdnewpre,
                                    hv_HomMat2DScale);


                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewpre, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewpre, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewpre, out ho_contourdowdnew,
                                    3);

                                hv_Rowmiddnew.Dispose(); hv_Colmiddnew.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmiddnew, out hv_Rowmiddnew, out hv_Colmiddnew);
                                hv_meannewmid.Dispose();
                                HOperatorSet.TupleMean(hv_Rowmiddnew, out hv_meannewmid);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowst - hv_meannewmid,
                                        0, out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursdnewnpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursdnewpre, out ho_SelectedContoursdnewnpre,
                                    hv_HomMatTransPreD);

                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewnpre, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewnpre, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewnpre, out ho_contourdowdnew,
                                    3);

                                //新的图形SelectedContoursdnewpre 中间线与上侧相机采集图像的中间线，角度保持一致
                                hv_RowBegintopd.Dispose(); hv_ColBegintopd.Dispose(); hv_RowEndtopd.Dispose(); hv_ColEndtopd.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmiddnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopd, out hv_ColBegintopd, out hv_RowEndtopd,
                                    out hv_ColEndtopd, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_angleltx.Dispose();
                                HOperatorSet.AngleLx(hv_RowBegintopd, hv_ColBegintopd, hv_RowEndtopd,
                                    hv_ColEndtopd, out hv_angleltx);
                                hv_Degltx.Dispose();
                                HOperatorSet.TupleDeg(hv_angleltx, out hv_Degltx);
                                hv_Degltxabs.Dispose();
                                HOperatorSet.TupleAbs(hv_Degltx, out hv_Degltxabs);
                                hv_DegSubD.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubD = hv_DegtdAbs - hv_Degltxabs;
                                }

                                //if (DegSubD > 0.1 or DegSubD < -0.1)
                                //area_center_xld (SelectedContoursdnewpre, Aread, Rowd, Columnd, PointOrderd)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(-DegSubD), Columnd[1], Rowd[1], HomMat2DDRotate)
                                //affine_trans_contour_xld (SelectedContoursdnewpre, SelectedContoursdfinalnew, HomMat2DDRotate)
                                //select_obj (SelectedContoursdfinalnew, contourtopdnew, 1)
                                //select_obj (SelectedContoursdfinalnew, contourmiddnew, 2)
                                //select_obj (SelectedContoursdfinalnew, contourdowdnew, 3)
                                //fit_line_contour_xld (contourmiddnew, 'tukey', -1, 10, 5, 2, RowBegintopd, ColBegintopd, RowEndtopd, ColEndtopd, Nrleft, Ncleft, Distleft)
                                //angle_lx (RowBegintopd, ColBegintopd, RowEndtopd, ColEndtopd, angleltx)
                                //tuple_deg (angleltx, Degltx)
                                //tuple_abs (Degltx, Degltxabs)
                                //DegSubD := DegtdAbs - Degltx
                                //SelectedContoursdnewpre := SelectedContoursdfinalnew
                                //endif

                                hv_RowBeginmiddnew.Dispose(); hv_ColBeginmiddnew.Dispose(); hv_RowEndmiddnew.Dispose(); hv_ColEndmiddnew.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmiddnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmiddnew, out hv_ColBeginmiddnew, out hv_RowEndmiddnew,
                                    out hv_ColEndmiddnew, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowst - hv_RowBeginmiddnew;
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_meanColsd - hv_meanColst,
                                        out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursdprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursdnewnpre, out ho_SelectedContoursdprenew,
                                    hv_HomMatTransPreD);
                                hv_Distancediagonal.Dispose();
                                hv_Distancediagonal = 241;
                                hv_HomMatTransPreD.Dispose();
                                HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_Distancediagonal,
                                    0, out hv_HomMatTransPreD);
                                ho_SelectedContoursdnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursdprenew, out ho_SelectedContoursdnew,
                                    hv_HomMatTransPreD);
                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnew, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnew, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnew, out ho_contourdowdnew,
                                    3);

                                hv_RowMidDown.Dispose(); hv_ColMidDown.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmiddnew, out hv_RowMidDown, out hv_ColMidDown);

                                hv_meanDownRNew.Dispose();
                                HOperatorSet.TupleMean(hv_RowMidDown, out hv_meanDownRNew);
                                hv_meanDownCNew.Dispose();
                                HOperatorSet.TupleMean(hv_ColMidDown, out hv_meanDownCNew);
                                hv_distancediag_td.Dispose();
                                HOperatorSet.DistancePp(hv_meanDownRNew, hv_meanDownCNew, hv_meanRowst,
                                    hv_meanColst, out hv_distancediag_td);
                                //
                                //Distancediagonal 为上下倒角斜边之间的长度
                                //X轴  先向左 移动  meanColst - meanColsd，与上侧相机采集的图像的中间线中点保持一致
                                //以中间线为轴，水平翻转。
                                //Y轴  先移动 meanRowst - RowBeginmidd,与上侧相机采集图像中间线保持高度一致
                                //中间线角度与上侧3D相机采集图像的中间线保持角度一致
                                //然后再向下移动 Distancediagonal
                                //下移241后，contourmiddnew 的 Y坐标 meanDownRNew
                                //Down3DLineY 为旋转后，下3D相机拍出来的方棒上边缘的Y位置
                                //Down3DY 为3D相机出激光线所在位置，这个位置为新的方棒下侧相机扫描线，反转后向下移动拟合的位置。
                                //Down3DY 为下侧3D相机激光口与上3D相机激光口侧的纵向坐标差，向下为正
                                hv_meanDownRNew.Dispose();
                                HOperatorSet.TupleMean(hv_RowMidDown, out hv_meanDownRNew);
                                hv_Down3DLineY.Dispose();
                                hv_Down3DLineY = new HTuple(hv_meanDownRNew);
                                hv_AbsDown3DDistance.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsd, out hv_AbsDown3DDistance);
                                hv_AbsMeanRowst.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowst, out hv_AbsMeanRowst);
                                hv_Down3DY.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_Down3DY = (hv_Down3DLineY + hv_AbsDown3DDistance) + hv_AbsMeanRowst;
                                }
                                hv_DownDistance.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DownDistance = (hv_AbsDown3DDistance + hv_AbsMeanRowst) + hv_Distancediagonal;
                                }
                                hv_RowBeginleft.Dispose(); hv_ColBeginleft.Dispose(); hv_RowEndleft.Dispose(); hv_ColEndleft.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourleft, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginleft, out hv_ColBeginleft, out hv_RowEndleft,
                                    out hv_ColEndleft, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_RowBeginrigt.Dispose(); hv_ColBeginrigt.Dispose(); hv_RowEndrigt.Dispose(); hv_ColEndrigt.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigt.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourrigt, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginrigt, out hv_ColBeginrigt, out hv_RowEndrigt,
                                    out hv_ColEndrigt, out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigt);
                                hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtopdnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl,
                                    out hv_ColEndlefl, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourdowdnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr,
                                    out hv_ColEndlefr, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);

                                hv_RowIntersectiontd.Dispose(); hv_ColIntersectiontd.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefl, hv_ColBeginlefl, hv_RowEndlefl,
                                    hv_ColEndlefl, hv_RowBeginlefr, hv_ColBeginlefr, hv_RowEndlefr,
                                    hv_ColEndlefr, out hv_RowIntersectiontd, out hv_ColIntersectiontd,
                                    out hv_IsOverlapping);
                                hv_RowIntersectiont.Dispose(); hv_ColIntersectiont.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginleft, hv_ColBeginleft, hv_RowEndleft,
                                    hv_ColEndleft, hv_RowBeginrigt, hv_ColBeginrigt, hv_RowEndrigt,
                                    hv_ColEndrigt, out hv_RowIntersectiont, out hv_ColIntersectiont,
                                    out hv_IsOverlapping);
                                hv_DistanceTD.Dispose();
                                HOperatorSet.DistancePp(hv_RowIntersectiontd, hv_ColIntersectiontd,
                                    hv_RowIntersectiont, hv_ColIntersectiont, out hv_DistanceTD);

                                //distance_pp (RowIntersectiontd, ColIntersectiontd, RowIntersectiontr, ColIntersectiontr, DistanceDR)
                                //distance_pp (RowIntersectiontd, ColIntersectiontd, RowIntersectiontl, ColIntersectiontl, DistanceDL)
                            }
                            else
                            {
                                //先与上侧相机采集的图像中间线的中点进行对齐

                                hv_Aread.Dispose(); hv_Rowd.Dispose(); hv_Columnd.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursd, out hv_Aread, out hv_Rowd,
                                    out hv_Columnd, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowst - hv_meanRowsd,
                                        hv_minColst - hv_minColsd, out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursdPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursd, out ho_SelectedContoursdPre,
                                    hv_HomMatTransPreD);

                                //以中间线做水平翻转
                                //注释部分继续研究，水平翻转过  沿着中线中点进行水平翻转  180
                                //area_center_xld (SelectedContoursdPre, Aread, Rowd, Columnd, PointOrderl)

                                //select_obj (SelectedContoursdPre, contourmiddnew, 2)
                                //get_contour_xld (contourmiddnew, RowsMidr, ColsMidr)
                                //tuple_mean (ColsMidr, MeanMidrx)
                                //tuple_mean (RowsMidr, MeanMidry)
                                //hom_mat2d_rotate (HomMat2DIdentity, 3.14, 56.2, -1, HomMat2DRotate)
                                //affine_trans_contour_xld (SelectedContoursdPre, SelectedContoursdnewpre, HomMat2DRotate)

                                hv_Aread.Dispose(); hv_RowPred.Dispose(); hv_ColumnPred.Dispose(); hv_PointOrderd.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursdPre, out hv_Aread, out hv_RowPred,
                                    out hv_ColumnPred, out hv_PointOrderd);
                                hv_HomMat2DScale.Dispose();
                                HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_meanRowsd,
                                    0, out hv_HomMat2DScale);
                                ho_SelectedContoursdnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursdPre, out ho_SelectedContoursdnewpre,
                                    hv_HomMat2DScale);


                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewpre, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewpre, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewpre, out ho_contourdowdnew,
                                    3);

                                hv_Rowmiddnew.Dispose(); hv_Colmiddnew.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmiddnew, out hv_Rowmiddnew, out hv_Colmiddnew);
                                hv_meannewmid.Dispose();
                                HOperatorSet.TupleMean(hv_Rowmiddnew, out hv_meannewmid);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowst - hv_meannewmid,
                                        0, out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursdnewnpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursdnewpre, out ho_SelectedContoursdnewnpre,
                                    hv_HomMatTransPreD);

                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewnpre, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewnpre, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnewnpre, out ho_contourdowdnew,
                                    3);

                                //新的图形SelectedContoursdnewpre 中间线与上侧相机采集图像的中间线，角度保持一致
                                hv_RowBegintopd.Dispose(); hv_ColBegintopd.Dispose(); hv_RowEndtopd.Dispose(); hv_ColEndtopd.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmiddnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopd, out hv_ColBegintopd, out hv_RowEndtopd,
                                    out hv_ColEndtopd, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_angleltx.Dispose();
                                HOperatorSet.AngleLx(hv_RowBegintopd, hv_ColBegintopd, hv_RowEndtopd,
                                    hv_ColEndtopd, out hv_angleltx);
                                hv_Degltx.Dispose();
                                HOperatorSet.TupleDeg(hv_angleltx, out hv_Degltx);
                                hv_Degltxabs.Dispose();
                                HOperatorSet.TupleAbs(hv_Degltx, out hv_Degltxabs);
                                hv_DegSubD.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubD = hv_DegtdAbs - hv_Degltxabs;
                                }

                                //if (DegSubD > 0.1 or DegSubD < -0.1)
                                //area_center_xld (SelectedContoursdnewpre, Aread, Rowd, Columnd, PointOrderd)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(-DegSubD), Columnd[1], Rowd[1], HomMat2DDRotate)
                                //affine_trans_contour_xld (SelectedContoursdnewpre, SelectedContoursdfinalnew, HomMat2DDRotate)
                                //select_obj (SelectedContoursdfinalnew, contourtopdnew, 1)
                                //select_obj (SelectedContoursdfinalnew, contourmiddnew, 2)
                                //select_obj (SelectedContoursdfinalnew, contourdowdnew, 3)
                                //fit_line_contour_xld (contourmiddnew, 'tukey', -1, 10, 5, 2, RowBegintopd, ColBegintopd, RowEndtopd, ColEndtopd, Nrleft, Ncleft, Distleft)
                                //angle_lx (RowBegintopd, ColBegintopd, RowEndtopd, ColEndtopd, angleltx)
                                //tuple_deg (angleltx, Degltx)
                                //tuple_abs (Degltx, Degltxabs)
                                //DegSubD := DegtdAbs - Degltx
                                //SelectedContoursdnewpre := SelectedContoursdfinalnew
                                //endif

                                hv_RowBeginmiddnew.Dispose(); hv_ColBeginmiddnew.Dispose(); hv_RowEndmiddnew.Dispose(); hv_ColEndmiddnew.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmiddnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmiddnew, out hv_ColBeginmiddnew, out hv_RowEndmiddnew,
                                    out hv_ColEndmiddnew, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowst - hv_RowBeginmiddnew;
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_meanColsd - hv_meanColst,
                                        out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursdprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursdnewnpre, out ho_SelectedContoursdprenew,
                                    hv_HomMatTransPreD);

                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdprenew, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdprenew, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdprenew, out ho_contourdowdnew,
                                    3);
                                hv_RowMidDown.Dispose(); hv_ColMidDown.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmiddnew, out hv_RowMidDown, out hv_ColMidDown);
                                hv_meanDownRNew.Dispose();
                                HOperatorSet.TupleMean(hv_RowMidDown, out hv_meanDownRNew);

                                //Down3DY - AbsDown3DDistance 为 contourmiddnew 需要移动到的位置 Y坐标
                                hv_AbsDown3DDistance.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsd, out hv_AbsDown3DDistance);
                                hv_AbsmeanRowst.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowst, out hv_AbsmeanRowst);
                                hv_Distancediagonal.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_Distancediagonal = (hv_DownDistance - hv_AbsDown3DDistance) - hv_AbsmeanRowst;
                                }

                                //Distancediagonal := 241
                                hv_HomMatTransPreD.Dispose();
                                HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_Distancediagonal,
                                    0, out hv_HomMatTransPreD);
                                ho_SelectedContoursdnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursdprenew, out ho_SelectedContoursdnew,
                                    hv_HomMatTransPreD);
                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnew, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnew, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursdnew, out ho_contourdowdnew,
                                    3);

                                hv_RowMidDown.Dispose(); hv_ColMidDown.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmiddnew, out hv_RowMidDown, out hv_ColMidDown);
                                hv_meanDownRNew.Dispose();
                                HOperatorSet.TupleMean(hv_RowMidDown, out hv_meanDownRNew);
                                hv_meanDownCNew.Dispose();
                                HOperatorSet.TupleMean(hv_ColMidDown, out hv_meanDownCNew);
                                hv_distancediag_td.Dispose();
                                HOperatorSet.DistancePp(hv_meanDownRNew, hv_meanDownCNew, hv_meanRowst,
                                    hv_meanColst, out hv_distancediag_td);
                                //
                                //Distancediagonal 为上下倒角斜边之间的长度
                                //X轴  先向左 移动  meanColst - meanColsd，与上侧相机采集的图像的中间线中点保持一致
                                //以中间线为轴，水平翻转。
                                //Y轴  先移动 meanRowst - RowBeginmidd,与上侧相机采集图像中间线保持高度一致
                                //中间线角度与上侧3D相机采集图像的中间线保持角度一致
                                //然后再向下移动 Distancediagonal

                                //Down3DLineY 为旋转后，下3D相机拍出来的方棒上边缘的Y位置
                                //Down3DY 为3D相机出激光线所在位置，这个位置为新的方棒下侧相机扫描线，反转后向下移动拟合的位置。
                                //tuple_mean (RowMidDown, meanDownRNew)
                                //Down3DLineY := meanDownRNew
                                //tuple_abs (meanRowsd, AbsDown3DDistance)
                                //Down3DY := Down3DLineY + AbsDown3DDistance

                                hv_RowBeginleft.Dispose(); hv_ColBeginleft.Dispose(); hv_RowEndleft.Dispose(); hv_ColEndleft.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourleft, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginleft, out hv_ColBeginleft, out hv_RowEndleft,
                                    out hv_ColEndleft, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_RowBeginrigt.Dispose(); hv_ColBeginrigt.Dispose(); hv_RowEndrigt.Dispose(); hv_ColEndrigt.Dispose(); hv_Nrrigt.Dispose(); hv_Ncrigt.Dispose(); hv_Distrigt.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourrigt, "tukey", -1, 10, 5,
                                    2, out hv_RowBeginrigt, out hv_ColBeginrigt, out hv_RowEndrigt,
                                    out hv_ColEndrigt, out hv_Nrrigt, out hv_Ncrigt, out hv_Distrigt);
                                hv_RowBeginlefl.Dispose(); hv_ColBeginlefl.Dispose(); hv_RowEndlefl.Dispose(); hv_ColEndlefl.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourtopdnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefl, out hv_ColBeginlefl, out hv_RowEndlefl,
                                    out hv_ColEndlefl, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);
                                hv_RowBeginlefr.Dispose(); hv_ColBeginlefr.Dispose(); hv_RowEndlefr.Dispose(); hv_ColEndlefr.Dispose(); hv_Nrmidd.Dispose(); hv_Ncmidd.Dispose(); hv_Distmidd.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourdowdnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginlefr, out hv_ColBeginlefr, out hv_RowEndlefr,
                                    out hv_ColEndlefr, out hv_Nrmidd, out hv_Ncmidd, out hv_Distmidd);

                                hv_RowIntersectiontd.Dispose(); hv_ColIntersectiontd.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginlefl, hv_ColBeginlefl, hv_RowEndlefl,
                                    hv_ColEndlefl, hv_RowBeginlefr, hv_ColBeginlefr, hv_RowEndlefr,
                                    hv_ColEndlefr, out hv_RowIntersectiontd, out hv_ColIntersectiontd,
                                    out hv_IsOverlapping);
                                hv_RowIntersectiont.Dispose(); hv_ColIntersectiont.Dispose(); hv_IsOverlapping.Dispose();
                                HOperatorSet.IntersectionLines(hv_RowBeginleft, hv_ColBeginleft, hv_RowEndleft,
                                    hv_ColEndleft, hv_RowBeginrigt, hv_ColBeginrigt, hv_RowEndrigt,
                                    hv_ColEndrigt, out hv_RowIntersectiont, out hv_ColIntersectiont,
                                    out hv_IsOverlapping);
                                hv_DistanceTD.Dispose();
                                HOperatorSet.DistancePp(hv_RowIntersectiontd, hv_ColIntersectiontd,
                                    hv_RowIntersectiont, hv_ColIntersectiont, out hv_DistanceTD);

                                //distance_pp (RowIntersectiontd, ColIntersectiontd, RowIntersectiontr, ColIntersectiontr, DistanceDR)
                                //distance_pp (RowIntersectiontd, ColIntersectiontd, RowIntersectiontl, ColIntersectiontl, DistanceDL)
                            }


                            if ((int)(new HTuple(hv_LeftRightDistance.TupleEqual(0))) != 0)
                            {
                                //以相机采集图像，中间线作为参考线有问题。需要以相机激光口横线为参照物，求出下3D相机相对上3D相机的纵向坐标。
                                //先与上侧相机采集的图像中间线的中点进行对齐

                                hv_Arear.Dispose(); hv_Rowr.Dispose(); hv_Columnr.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursr, out hv_Arear, out hv_Rowr,
                                    out hv_Columnr, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowsl - hv_meanRowsr,
                                        hv_minColsl - hv_minColsr, out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursrPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursr, out ho_SelectedContoursrPre,
                                    hv_HomMatTransPreD);

                                //以中间线做水平翻转
                                //注释部分继续研究，水平翻转过  沿着中线中点进行水平翻转  180

                                hv_Arear.Dispose(); hv_RowPrer.Dispose(); hv_ColumnPrer.Dispose(); hv_PointOrderd.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursrPre, out hv_Arear, out hv_RowPrer,
                                    out hv_ColumnPrer, out hv_PointOrderd);
                                hv_HomMat2DScale.Dispose();
                                HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_meanRowsr,
                                    0, out hv_HomMat2DScale);
                                ho_SelectedContoursrnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrPre, out ho_SelectedContoursrnewpre,
                                    hv_HomMat2DScale);


                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourdowdnew,
                                    3);

                                hv_Rowmiddnew.Dispose(); hv_Colmiddnew.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmiddnew, out hv_Rowmiddnew, out hv_Colmiddnew);
                                hv_meannewmid.Dispose();
                                HOperatorSet.TupleMean(hv_Rowmiddnew, out hv_meannewmid);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowsl - hv_meannewmid,
                                        0, out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursrnewnpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrnewpre, out ho_SelectedContoursrnewnpre,
                                    hv_HomMatTransPreD);

                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewnpre, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewnpre, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewnpre, out ho_contourdowdnew,
                                    3);

                                //新的图形SelectedContoursdnewpre 中间线与上侧相机采集图像的中间线，角度保持一致
                                hv_RowBegintopd.Dispose(); hv_ColBegintopd.Dispose(); hv_RowEndtopd.Dispose(); hv_ColEndtopd.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmiddnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopd, out hv_ColBegintopd, out hv_RowEndtopd,
                                    out hv_ColEndtopd, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_angleltx.Dispose();
                                HOperatorSet.AngleLx(hv_RowBegintopd, hv_ColBegintopd, hv_RowEndtopd,
                                    hv_ColEndtopd, out hv_angleltx);
                                hv_Degltx.Dispose();
                                HOperatorSet.TupleDeg(hv_angleltx, out hv_Degltx);
                                hv_Degltxabs.Dispose();
                                HOperatorSet.TupleAbs(hv_Degltx, out hv_Degltxabs);
                                hv_DegSubD.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubD = hv_DegtdAbs - hv_Degltxabs;
                                }

                                //if (DegSubD > 0.1 or DegSubD < -0.1)
                                //area_center_xld (SelectedContoursdnewpre, Aread, Rowd, Columnd, PointOrderd)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(-DegSubD), Columnd[1], Rowd[1], HomMat2DDRotate)
                                //affine_trans_contour_xld (SelectedContoursdnewpre, SelectedContoursdfinalnew, HomMat2DDRotate)
                                //select_obj (SelectedContoursdfinalnew, contourtopdnew, 1)
                                //select_obj (SelectedContoursdfinalnew, contourmiddnew, 2)
                                //select_obj (SelectedContoursdfinalnew, contourdowdnew, 3)
                                //fit_line_contour_xld (contourmiddnew, 'tukey', -1, 10, 5, 2, RowBegintopd, ColBegintopd, RowEndtopd, ColEndtopd, Nrleft, Ncleft, Distleft)
                                //angle_lx (RowBegintopd, ColBegintopd, RowEndtopd, ColEndtopd, angleltx)
                                //tuple_deg (angleltx, Degltx)
                                //tuple_abs (Degltx, Degltxabs)
                                //DegSubD := DegtdAbs - Degltx
                                //SelectedContoursdnewpre := SelectedContoursdfinalnew
                                //endif

                                hv_RowBeginmiddnew.Dispose(); hv_ColBeginmiddnew.Dispose(); hv_RowEndmiddnew.Dispose(); hv_ColEndmiddnew.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmiddnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmiddnew, out hv_ColBeginmiddnew, out hv_RowEndmiddnew,
                                    out hv_ColEndmiddnew, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowsl - hv_RowBeginmiddnew;
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_meanColsd - hv_meanColst,
                                        out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursrprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrnewnpre, out ho_SelectedContoursrprenew,
                                    hv_HomMatTransPreD);
                                hv_Distancediagonal.Dispose();
                                hv_Distancediagonal = 241;
                                hv_HomMatTransPreD.Dispose();
                                HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_Distancediagonal,
                                    0, out hv_HomMatTransPreD);
                                ho_SelectedContoursrnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrprenew, out ho_SelectedContoursrnew,
                                    hv_HomMatTransPreD);
                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourdowdnew,
                                    3);

                                hv_RowMidDown.Dispose(); hv_ColMidDown.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmiddnew, out hv_RowMidDown, out hv_ColMidDown);
                                hv_meanDownRNew.Dispose();
                                HOperatorSet.TupleMean(hv_RowMidDown, out hv_meanDownRNew);

                                hv_meanRightC.Dispose();
                                HOperatorSet.TupleMean(hv_ColMidDown, out hv_meanRightC);
                                hv_meanRightR.Dispose();
                                HOperatorSet.TupleMean(hv_RowMidDown, out hv_meanRightR);
                                hv_distancediag_lr.Dispose();
                                HOperatorSet.DistancePp(hv_meanRowsl, hv_meanColsl, hv_meanRightR,
                                    hv_meanRightC, out hv_distancediag_lr);


                                //
                                //Distancediagonal 为上下倒角斜边之间的长度
                                //X轴  先向左 移动  meanColst - meanColsd，与上侧相机采集的图像的中间线中点保持一致
                                //以中间线为轴，水平翻转。
                                //Y轴  先移动 meanRowst - RowBeginmidd,与上侧相机采集图像中间线保持高度一致
                                //中间线角度与上侧3D相机采集图像的中间线保持角度一致
                                //然后再向下移动 Distancediagonal
                                //下移241后，contourmiddnew 的 Y坐标 meanDownRNew
                                //Down3DLineY 为旋转后，下3D相机拍出来的方棒上边缘的Y位置
                                //Down3DY 为3D相机出激光线所在位置，这个位置为新的方棒下侧相机扫描线，反转后向下移动拟合的位置。
                                //Down3DY 为下侧3D相机激光口与上3D相机激光口侧的纵向坐标差，向下为正
                                hv_Down3DLineY.Dispose();
                                hv_Down3DLineY = new HTuple(hv_meanDownRNew);
                                hv_AbsDown3DDistance.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsr, out hv_AbsDown3DDistance);
                                hv_AbsMeanRowst.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsl, out hv_AbsMeanRowst);
                                hv_Down3DY.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_Down3DY = (hv_Down3DLineY + hv_AbsDown3DDistance) + hv_AbsMeanRowst;
                                }
                                hv_LeftRightDistance.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_LeftRightDistance = (hv_AbsDown3DDistance + hv_AbsMeanRowst) + hv_Distancediagonal;
                                }
                                //fit_line_contour_xld (contourlefl, 'tukey', -1, 10, 5, 2, RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, Nrleft, Ncleft, Distleft)
                                //fit_line_contour_xld (contourrigl, 'tukey', -1, 10, 5, 2, RowBeginrigt, ColBeginrigt, RowEndrigt, ColEndrigt, Nrrigt, Ncrigt, Distrigt)
                                //fit_line_contour_xld (contourtopdnew, 'tukey', -1, 10, 5, 2, RowBeginlefl, ColBeginlefl, RowEndlefl, ColEndlefl, Nrmidd, Ncmidd, Distmidd)
                                //fit_line_contour_xld (contourdowdnew, 'tukey', -1, 10, 5, 2, RowBeginlefr, ColBeginlefr, RowEndlefr, ColEndlefr, Nrmidd, Ncmidd, Distmidd)

                                //intersection_lines (RowBeginlefl, ColBeginlefl, RowEndlefl, ColEndlefl, RowBeginlefr, ColBeginlefr, RowEndlefr, ColEndlefr, RowIntersectiontd, ColIntersectiontd, IsOverlapping)
                                //intersection_lines (RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, RowBeginrigt, ColBeginrigt, RowEndrigt, ColEndrigt, RowIntersectiont, ColIntersectiont, IsOverlapping)
                                //distance_pp (RowIntersectiontd, ColIntersectiontd, RowIntersectiont, ColIntersectiont, DistanceLR)

                                //distance_pp (RowIntersectiontd, ColIntersectiontd, RowIntersectiontr, ColIntersectiontr, DistanceDR)
                                //distance_pp (RowIntersectiontd, ColIntersectiontd, RowIntersectiontl, ColIntersectiontl, DistanceDL)
                            }
                            else
                            {
                                //先与上侧相机采集的图像中间线的中点进行对齐

                                //以相机采集图像，中间线作为参考线有问题。需要以相机激光口横线为参照物，求出下3D相机相对上3D相机的纵向坐标。
                                //先与上侧相机采集的图像中间线的中点进行对齐

                                hv_Arear.Dispose(); hv_Rowr.Dispose(); hv_Columnr.Dispose(); hv_PointOrderl.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursr, out hv_Arear, out hv_Rowr,
                                    out hv_Columnr, out hv_PointOrderl);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowsl - hv_meanRowsr,
                                        hv_minColsl - hv_minColsr, out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursrPre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursr, out ho_SelectedContoursrPre,
                                    hv_HomMatTransPreD);

                                //以中间线做水平翻转
                                //注释部分继续研究，水平翻转过  沿着中线中点进行水平翻转  180

                                hv_Arear.Dispose(); hv_RowPrer.Dispose(); hv_ColumnPrer.Dispose(); hv_PointOrderd.Dispose();
                                HOperatorSet.AreaCenterXld(ho_SelectedContoursrPre, out hv_Arear, out hv_RowPrer,
                                    out hv_ColumnPrer, out hv_PointOrderd);
                                hv_HomMat2DScale.Dispose();
                                HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, -1, 1, hv_meanRowsr,
                                    0, out hv_HomMat2DScale);
                                ho_SelectedContoursrnewpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrPre, out ho_SelectedContoursrnewpre,
                                    hv_HomMat2DScale);


                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewpre, out ho_contourdowdnew,
                                    3);

                                hv_Rowmiddnew.Dispose(); hv_Colmiddnew.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmiddnew, out hv_Rowmiddnew, out hv_Colmiddnew);
                                hv_meannewmid.Dispose();
                                HOperatorSet.TupleMean(hv_Rowmiddnew, out hv_meannewmid);
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_meanRowsl - hv_meannewmid,
                                        0, out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursrnewnpre.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrnewpre, out ho_SelectedContoursrnewnpre,
                                    hv_HomMatTransPreD);

                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewnpre, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewnpre, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnewnpre, out ho_contourdowdnew,
                                    3);

                                //新的图形SelectedContoursdnewpre 中间线与上侧相机采集图像的中间线，角度保持一致
                                hv_RowBegintopd.Dispose(); hv_ColBegintopd.Dispose(); hv_RowEndtopd.Dispose(); hv_ColEndtopd.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmiddnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBegintopd, out hv_ColBegintopd, out hv_RowEndtopd,
                                    out hv_ColEndtopd, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_angleltx.Dispose();
                                HOperatorSet.AngleLx(hv_RowBegintopd, hv_ColBegintopd, hv_RowEndtopd,
                                    hv_ColEndtopd, out hv_angleltx);
                                hv_Degltx.Dispose();
                                HOperatorSet.TupleDeg(hv_angleltx, out hv_Degltx);
                                hv_Degltxabs.Dispose();
                                HOperatorSet.TupleAbs(hv_Degltx, out hv_Degltxabs);
                                hv_DegSubD.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_DegSubD = hv_DegtdAbs - hv_Degltxabs;
                                }

                                //if (DegSubD > 0.1 or DegSubD < -0.1)
                                //area_center_xld (SelectedContoursdnewpre, Aread, Rowd, Columnd, PointOrderd)
                                //hom_mat2d_rotate (HomMat2DIdentity, rad(-DegSubD), Columnd[1], Rowd[1], HomMat2DDRotate)
                                //affine_trans_contour_xld (SelectedContoursdnewpre, SelectedContoursdfinalnew, HomMat2DDRotate)
                                //select_obj (SelectedContoursdfinalnew, contourtopdnew, 1)
                                //select_obj (SelectedContoursdfinalnew, contourmiddnew, 2)
                                //select_obj (SelectedContoursdfinalnew, contourdowdnew, 3)
                                //fit_line_contour_xld (contourmiddnew, 'tukey', -1, 10, 5, 2, RowBegintopd, ColBegintopd, RowEndtopd, ColEndtopd, Nrleft, Ncleft, Distleft)
                                //angle_lx (RowBegintopd, ColBegintopd, RowEndtopd, ColEndtopd, angleltx)
                                //tuple_deg (angleltx, Degltx)
                                //tuple_abs (Degltx, Degltxabs)
                                //DegSubD := DegtdAbs - Degltx
                                //SelectedContoursdnewpre := SelectedContoursdfinalnew
                                //endif

                                hv_RowBeginmiddnew.Dispose(); hv_ColBeginmiddnew.Dispose(); hv_RowEndmiddnew.Dispose(); hv_ColEndmiddnew.Dispose(); hv_Nrleft.Dispose(); hv_Ncleft.Dispose(); hv_Distleft.Dispose();
                                HOperatorSet.FitLineContourXld(ho_contourmiddnew, "tukey", -1, 10,
                                    5, 2, out hv_RowBeginmiddnew, out hv_ColBeginmiddnew, out hv_RowEndmiddnew,
                                    out hv_ColEndmiddnew, out hv_Nrleft, out hv_Ncleft, out hv_Distleft);
                                hv_SubRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_SubRow = hv_meanRowsl - hv_RowBeginmiddnew;
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_HomMatTransPreD.Dispose();
                                    HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_SubRow, hv_meanColsd - hv_meanColst,
                                        out hv_HomMatTransPreD);
                                }
                                ho_SelectedContoursrprenew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrnewnpre, out ho_SelectedContoursrprenew,
                                    hv_HomMatTransPreD);

                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrprenew, out ho_contourdowdnew,
                                    3);
                                hv_RowMidDown.Dispose(); hv_ColMidDown.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmiddnew, out hv_RowMidDown, out hv_ColMidDown);
                                hv_meanDownRNew.Dispose();
                                HOperatorSet.TupleMean(hv_RowMidDown, out hv_meanDownRNew);

                                //Down3DY - AbsDown3DDistance 为 contourmiddnew 需要移动到的位置 Y坐标
                                hv_AbsDown3DDistance.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsr, out hv_AbsDown3DDistance);
                                hv_AbsmeanRowst.Dispose();
                                HOperatorSet.TupleAbs(hv_meanRowsl, out hv_AbsmeanRowst);
                                hv_Distancediagonal.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_Distancediagonal = (hv_LeftRightDistance - hv_AbsDown3DDistance) - hv_AbsmeanRowst;
                                }

                                //Distancediagonal := 241
                                hv_HomMatTransPreD.Dispose();
                                HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_Distancediagonal,
                                    0, out hv_HomMatTransPreD);
                                ho_SelectedContoursrnew.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_SelectedContoursrprenew, out ho_SelectedContoursrnew,
                                    hv_HomMatTransPreD);
                                ho_contourtopdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourtopdnew,
                                    1);
                                ho_contourmiddnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourmiddnew,
                                    2);
                                ho_contourdowdnew.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedContoursrnew, out ho_contourdowdnew,
                                    3);

                                hv_RowMidDown.Dispose(); hv_ColMidDown.Dispose();
                                HOperatorSet.GetContourXld(ho_contourmiddnew, out hv_RowMidDown, out hv_ColMidDown);

                                hv_meanRightC.Dispose();
                                HOperatorSet.TupleMean(hv_ColMidDown, out hv_meanRightC);
                                hv_meanRightR.Dispose();
                                HOperatorSet.TupleMean(hv_RowMidDown, out hv_meanRightR);
                                hv_distancediag_lr.Dispose();
                                HOperatorSet.DistancePp(hv_meanRowsl, hv_meanColsl, hv_meanRightR,
                                    hv_meanRightC, out hv_distancediag_lr);


                                //
                                //Distancediagonal 为上下倒角斜边之间的长度
                                //X轴  先向左 移动  meanColst - meanColsd，与上侧相机采集的图像的中间线中点保持一致
                                //以中间线为轴，水平翻转。
                                //Y轴  先移动 meanRowst - RowBeginmidd,与上侧相机采集图像中间线保持高度一致
                                //中间线角度与上侧3D相机采集图像的中间线保持角度一致
                                //然后再向下移动 Distancediagonal

                                //Down3DLineY 为旋转后，下3D相机拍出来的方棒上边缘的Y位置
                                //Down3DY 为3D相机出激光线所在位置，这个位置为新的方棒下侧相机扫描线，反转后向下移动拟合的位置。
                                //tuple_mean (RowMidDown, meanDownRNew)
                                //Down3DLineY := meanDownRNew
                                //tuple_abs (meanRowsd, AbsDown3DDistance)
                                //Down3DY := Down3DLineY + AbsDown3DDistance

                                //fit_line_contour_xld (contourlefl, 'tukey', -1, 10, 5, 2, RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, Nrleft, Ncleft, Distleft)
                                //fit_line_contour_xld (contourrigl, 'tukey', -1, 10, 5, 2, RowBeginrigt, ColBeginrigt, RowEndrigt, ColEndrigt, Nrrigt, Ncrigt, Distrigt)
                                //fit_line_contour_xld (contourtopdnew, 'tukey', -1, 10, 5, 2, RowBeginlefl, ColBeginlefl, RowEndlefl, ColEndlefl, Nrmidd, Ncmidd, Distmidd)
                                //fit_line_contour_xld (contourdowdnew, 'tukey', -1, 10, 5, 2, RowBeginlefr, ColBeginlefr, RowEndlefr, ColEndlefr, Nrmidd, Ncmidd, Distmidd)

                                //intersection_lines (RowBeginlefl, ColBeginlefl, RowEndlefl, ColEndlefl, RowBeginlefr, ColBeginlefr, RowEndlefr, ColEndlefr, RowIntersectiontd, ColIntersectiontd, IsOverlapping)
                                //intersection_lines (RowBeginleft, ColBeginleft, RowEndleft, ColEndleft, RowBeginrigt, ColBeginrigt, RowEndrigt, ColEndrigt, RowIntersectiont, ColIntersectiont, IsOverlapping)
                                //distance_pp (RowIntersectiontd, ColIntersectiontd, RowIntersectiont, ColIntersectiont, DistanceLR)

                                //distance_pp (RowIntersectiontd, ColIntersectiontd, RowIntersectiontr, ColIntersectiontr, DistanceDR)
                                //distance_pp (RowIntersectiontd, ColIntersectiontd, RowIntersectiontl, ColIntersectiontl, DistanceDL)
                            }

                            if ((int)((new HTuple((new HTuple((new HTuple((new HTuple((new HTuple(hv_LeftTDistance.TupleNotEqual(
                                0))).TupleAnd(new HTuple(hv_LeftDDistance.TupleNotEqual(0))))).TupleAnd(
                                new HTuple(hv_RightTDistance.TupleNotEqual(0))))).TupleAnd(new HTuple(hv_RightDDistance.TupleNotEqual(
                                0))))).TupleAnd(new HTuple(hv_DownDistance.TupleNotEqual(0))))).TupleAnd(
                                new HTuple(hv_LeftRightDistance.TupleNotEqual(0)))) != 0)
                            {
                                break;
                            }
                            //get_contour_xld (contourmidt, Rowsmidt, Colsmidt)
                            //tuple_mean (Rowsmidt, MeanT)
                            //get_contour_xld (contourmidd, Rowsmidd, Colsmidt)
                            //tuple_mean (Rowsmidd, MeanD)
                            //tuple_concat (topdown_HypotenuseDis, 360 - MeanT - MeanD, topdown_HypotenuseDis)
                            //get_contour_xld (contourmidl, Rowsmidl, Colsmidl)
                            //tuple_mean (Rowsmidl, MeanL)
                            //get_contour_xld (contourmidr, Rowsmidr, Colsmidr)
                            //tuple_mean (Rowsmidr, MeanR)
                            //tuple_concat (leftright_HypotenuseDis, 355 - MeanL - MeanR, leftright_HypotenuseDis)
                            //length_xld (contourmidt, Length1)
                            //distance_pp (RowBeginmidt, ColBeginmidt, RowBeginmidl, ColBeginmidl, DistanceLTLineLength)
                            //distance_pp (RowBeginmidt, ColBeginmidt, RowBeginmidr, ColBeginmidr, DistanceRTLineLength)
                            //distance_pp (RowBeginmidd, ColBeginmidd, RowBeginmidl, ColBeginmidl, DistanceLDLineLength)
                            //distance_pp (RowBeginmidd, ColBeginmidd, RowBeginmidr, ColBeginmidr, DistanceRDLineLength)
                            //tuple_concat (lefttop_lineLength, DistanceLTLineLength, lefttop_lineLength)
                            //tuple_concat (leftdown_lineLength, DistanceLDLineLength, leftdown_lineLength)
                            //tuple_concat (righttop_lineLength, DistanceRTLineLength, righttop_lineLength)
                            //tuple_concat (rightdown_linelength, DistanceRDLineLength, rightdown_linelength)
                            //hom_mat2d_identity (HomMat2DIdentity)
                            //hom_mat2d_translate (HomMat2DIdentity, 0, 360, HomMat2dtranslate)
                            //affine_trans_contour_xld (contourmidd, contourmiddnew, HomMat2dtranslate)
                            //distance_cc (contourmidt, contourmidd, 'point_to_point', DistanceMin, DistanceMax)
                            //gen_region_line (RegionLineLT, RowBegint, ColBegint[0], RowBeginl[0], ColBeginl[0])
                            //distance_pp (RowBegint[0], ColBegint[0], RowBeginl[0], ColBeginl[0], distances)
                            //get_region_points (RegionLineLT, Rowslt, Columnslt)
                            //intersection_lines (RowBegint[0], ColBegint[0], RowEndt[0], ColEndt[0], RowBegint[2], ColBegint[2], RowEndt[2], ColEndt[2], Rowt, Columnt, IsOverlapping)
                            //intersection_lines (RowBeginl[0], ColBeginl[0], RowEndl[0], ColEndl[0], RowBeginl[2], ColBeginl[2], RowEndl[2], ColEndl[2], Rowl, Columnl, IsOverlapping)
                            //intersection_lines (RowBeginr[0], ColBeginr[0], RowEndr[0], ColEndr[0], RowBeginr[2], ColBeginr[2], RowEndr[2], ColEndr[2], Rowr, Columnr, IsOverlapping)
                            //intersection_lines (RowBegind[0], ColBegind[0], RowEndd[0], ColEndd[0], RowBegind[2], ColBegind[2], RowEndd[2], ColEndd[2], Rowd, Columnd, IsOverlapping)
                            //distance_pp (Rowt, Columnt, Rowl, Columnl, Distancelt)
                            //distance_pp (Rowt, Columnt, Rowr, Columnr, Distancert)
                            //distance_pp (Rowd, Columnd, Rowl, Columnl, Distanceld)
                            //distance_pp (Rowd, Columnd, Rowr, Columnr, Distancerd)
                            //get_contour_xld (contourmidt, Row, Col)
                            //gen_region_line (RegionLineLT, Rowt, Columnt, Rowl, Columnl)
                            //gen_region_line (RegionLineRT, Rowt, Columnt, Rowr, Columnr)
                            //gen_region_line (RegionLineLD, Rowd, Columnd, Rowl, Columnl)
                            //gen_region_line (RegionLineRD, Rowd, Columnd, Rowr, Columnr)
                            //gen_region_line (RegionLineRT, RowBegint[1], ColBegint[1], RowBeginr[0], ColBeginr[0])
                            //distance_pp (RowBegint[0], ColBegint[0], RowBeginr[0], ColBeginr[0], distancesy)
                            //create_pose (30, 0, -200, 0, -45, 0, 'Rp+T', 'gba', 'point', Posebox)
                            //gen_box_object_model_3d (Posebox, Distancert, Distancelt, Distancert, ObjectModel3D)
                            //visualize_object_model_3d (WindowHandle, [ObjectModel3D,Surface3DDefaultT,Surface3DDefaultL, Surface3DDefaultR,Surface3DDefaultD], [], [], ['color_0','color_1','color_2','color_3', 'color_4', 'alpha', 'alpha_0'], ['magenta','yellow','blue','red','cyan',0.75,1], Message, [], Instructions, PoseOut)
                            //poseOut[2] := -10
                            //disp_object_model_3d (WindowHandle, [Surface3DDefaultT], VisualizationCamParam, PoseOut, ['color_0'], ['magenta'])
                            //disp_object_model_3d (WindowHandle, [ObjectModel3D,Surface3DDefaultT,Surface3DDefaultL, Surface3DDefaultR,Surface3DDefaultD], VisualizationCamParam, PoseOut, ['color_0','color_1','color_2','color_3', 'color_4', 'alpha', 'alpha_0'], ['magenta','yellow','blue','red','cyan',0.75,1])
                            //get_region_points (RegionLineRT, Rowsrt, Columnsrt)
                            //gen_region_line (RegionLineLD, RowBeginl[2], ColBeginl[2], RowBegind[0], ColBegind[0])
                            //get_region_points (RegionLineLD, Rowsld, Columnsld)
                            //gen_region_line (RegionLineRD, RowBegind[1], ColBegind[1], RowBeginr[0], ColBeginr[0])
                            //get_region_points (RegionLineRD, Rowsrd, Columnsrd)
                            //create_pose (0, 0, CenterPoint[2], 0, 0, 0, 'Rp+T', 'gba', 'point', Posebox)
                            //get_region_points (RegionLineLT, Rowslt, Columnslt)
                            //angle_ll (RowBegint[0], ColBegint[0], RowEndt[0], ColEndt[0], RowBegint[0], ColBegint[0], RowEndl[0], ColEndl[0], Angle1)
                            //angle_ll (RowBegint[0], ColBegint[0], RowEndt[0], ColEndt[0], RowBegint[1], ColBegint[1], RowEndt[1], ColEndt[1], Angle1)
                            //angle_ll (RowBegint[0], ColBegint[0], RowEndt[0], ColEndt[0], RowBegint[2], ColBegint[2], RowEndt[2], ColEndt[2], Angle2)
                            //angle_ll (RowBegint[1], ColBegint[1], RowEndt[1], ColEndt[1], RowBegint[2], ColBegint[2], RowEndt[2], ColEndt[2], Angle3)
                            //distance_pp (RowBegint[0], ColBegint[0], RowEndt[0], ColEndt[0], distances)
                            //distance_pp (RowBegint[1], ColBegint[1], RowEndt[1], ColEndt[1], distances1)
                            //distance_pp (RowBegint[2], ColBegint[2], RowEndt[2], ColEndt[2], distances2)
                            //Angle1 := abs(deg(Angle1))
                            //Angle2 := abs(deg(Angle2))
                            //Angle3 := abs(deg(Angle3))
                        }
                        else
                        {
                            continue;
                        }
                    }
                    // catch (Exception) 
                    catch (HalconException HDevExpDefaultException1)
                    {
                        HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    }

                    //fit_circle_contour_xld (ObjectSelected, 'geohuber', -1, 0, 0, 3, 2, Row, Column, Radius1, StartPhi, EndPhi, PointOrder)
                    //gen_circle_contour_xld (ContCircle, Row, Column, Radius1, 0, 6.28318, 'positive', 1)
                    //raduis := [raduis,Radius1]


                }

                ho_ImageOutLeft.Dispose();
                ho_ImageOutRight.Dispose();
                ho_ImageOutTop.Dispose();
                ho_ImageOutDown.Dispose();
                ho_ImagePart.Dispose();
                ho_ImagePartReal.Dispose();
                ho_ImagePartLeft.Dispose();
                ho_ImagePartLeftNew.Dispose();
                ho_RegionLeft.Dispose();
                ho_ImageReducedLeft.Dispose();
                ho_RegionErosion.Dispose();
                ho_ImagePartRight.Dispose();
                ho_ImagePartRightNew.Dispose();
                ho_RegionRight.Dispose();
                ho_ImageReducedRight.Dispose();
                ho_ImagePartTop.Dispose();
                ho_ImagePartTopNew.Dispose();
                ho_RegionTop.Dispose();
                ho_ImageReducedTop.Dispose();
                ho_ImagePartDown.Dispose();
                ho_ImagePartDownNew.Dispose();
                ho_RegionDown.Dispose();
                ho_ImageReducedDown.Dispose();
                ho_Bx.Dispose();
                ho_By.Dispose();
                ho_Bz.Dispose();
                ho_Intersectiont.Dispose();
                ho_UnionContourst.Dispose();
                ho_ObjectSelectedt.Dispose();
                ho_ContoursSplitt.Dispose();
                ho_SelectedContourst.Dispose();
                ho_contournewmid.Dispose();
                ho_Intersectionl.Dispose();
                ho_UnionContoursl.Dispose();
                ho_ObjectSelectedl.Dispose();
                ho_ContoursSplitl.Dispose();
                ho_SelectedContoursl.Dispose();
                ho_Intersectionr.Dispose();
                ho_UnionContoursr.Dispose();
                ho_ObjectSelectedr.Dispose();
                ho_ContoursSplitr.Dispose();
                ho_SelectedContoursr.Dispose();
                ho_Intersectiond.Dispose();
                ho_UnionContoursd.Dispose();
                ho_ObjectSelectedd.Dispose();
                ho_ContoursSplitd.Dispose();
                ho_SelectedContoursd.Dispose();
                ho_contourleft.Dispose();
                ho_contourmidt.Dispose();
                ho_contourrigt.Dispose();
                ho_contourlefr.Dispose();
                ho_contourmidr.Dispose();
                ho_contourrigr.Dispose();
                ho_contourlefl.Dispose();
                ho_contourmidl.Dispose();
                ho_contourrigl.Dispose();
                ho_contourlefd.Dispose();
                ho_contourmidd.Dispose();
                ho_contourrigd.Dispose();
                ho_contourtmp.Dispose();
                ho_SelectedContourslPre.Dispose();
                ho_contourtoplnew.Dispose();
                ho_contourmidlnew.Dispose();
                ho_contourdowlnew.Dispose();
                ho_SelectedContourslfinal.Dispose();
                ho_SelectedContourslnewpre.Dispose();
                ho_SelectedContourslprenew.Dispose();
                ho_SelectedContourslnew.Dispose();
                ho_SelectedContoursrPre.Dispose();
                ho_SelectedContoursrfinal.Dispose();
                ho_SelectedContoursrnewpre.Dispose();
                ho_contourtoprnew.Dispose();
                ho_contourmidrnew.Dispose();
                ho_contourdowrnew.Dispose();
                ho_SelectedContoursrprenew.Dispose();
                ho_SelectedContoursrnew.Dispose();
                ho_SelectedContoursdPre.Dispose();
                ho_contourmiddnew.Dispose();
                ho_SelectedContoursdnewpre.Dispose();
                ho_contourtopdnew.Dispose();
                ho_contourdowdnew.Dispose();
                ho_SelectedContoursdnewnpre.Dispose();
                ho_SelectedContoursdprenew.Dispose();
                ho_SelectedContoursdnew.Dispose();
                ho_SelectedContoursrnewnpre.Dispose();

                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_Instructions.Dispose();
                hv_WindowHandle.Dispose();
                hv_Message.Dispose();
                hv_r.Dispose();
                hv_ParameterValues.Dispose();
                hv_Status.Dispose();
                hv_yInterval.Dispose();
                hv_ObjectModel3DBLeft.Dispose();
                hv_ObjectModel3DBLeftConnected.Dispose();
                hv_ObjectModel3DBLeftNew.Dispose();
                hv_ObjectModel3DBRight.Dispose();
                hv_ObjectModel3DBRightConnected.Dispose();
                hv_ObjectModel3DBRightNew.Dispose();
                hv_ObjectModel3DBTop.Dispose();
                hv_ObjectModel3DTopConnected.Dispose();
                hv_ObjectModel3DBTopNew.Dispose();
                hv_ObjectModel3DBDown.Dispose();
                hv_ObjectModel3DBDownConnected.Dispose();
                hv_ObjectModel3DBDownNew.Dispose();
                hv_PoseOut.Dispose();
                hv_SampledObjectModel3DTop.Dispose();
                hv_SampledObjectModel3DDown.Dispose();
                hv_SampledObjectModel3DLeft.Dispose();
                hv_SampledObjectModel3DRight.Dispose();
                hv_Surface3DDefaultT.Dispose();
                hv_Info.Dispose();
                hv_Surface3DDefaultD.Dispose();
                hv_Surface3DDefaultL.Dispose();
                hv_Surface3DDefaultR.Dispose();
                hv_CenterPointT.Dispose();
                hv_Radius.Dispose();
                hv_CenterPointD.Dispose();
                hv_CenterPointL.Dispose();
                hv_CenterPointR.Dispose();
                hv_raduis.Dispose();
                hv_ObjectModel3DIntersections.Dispose();
                hv_DisparityProfileWidth.Dispose();
                hv_DisparityProfileHeight.Dispose();
                hv_WindowEnlargement.Dispose();
                hv_WindowWidth.Dispose();
                hv_WindowHeight.Dispose();
                hv_VisualizationPlaneSize.Dispose();
                hv_PoseT.Dispose();
                hv_PoseD.Dispose();
                hv_PoseL.Dispose();
                hv_PoseR.Dispose();
                hv_IntersectionPlane1.Dispose();
                hv_IntersectionPlane2.Dispose();
                hv_IntersectionPlane3.Dispose();
                hv_IntersectionPlane4.Dispose();
                hv_IntersectionPlane5.Dispose();
                hv_top_Hypotenuse.Dispose();
                hv_top_LefttAngle.Dispose();
                hv_top_RightAngle.Dispose();
                hv_top_HypotenuseFH.Dispose();
                hv_top_HypotenuseSH.Dispose();
                hv_topdown_HypotenuseDis.Dispose();
                hv_leftright_HypotenuseDis.Dispose();
                hv_left_HypotenuseFH.Dispose();
                hv_left_HypotenuseSH.Dispose();
                hv_lefttop_lineLength.Dispose();
                hv_righttop_lineLength.Dispose();
                hv_leftdown_lineLength.Dispose();
                hv_rightdown_linelength.Dispose();
                hv_left_Hypotenuse.Dispose();
                hv_left_LefttAngle.Dispose();
                hv_left_RightAngle.Dispose();
                hv_right_HypotenuseFH.Dispose();
                hv_right_HypotenuseSH.Dispose();
                hv_down_HypotenuseFH.Dispose();
                hv_down_HypotenuseSH.Dispose();
                hv_down_Hypotenuse.Dispose();
                hv_down_LefttAngle.Dispose();
                hv_down_RightAngle.Dispose();
                hv_right_Hypotenuse.Dispose();
                hv_right_LefttAngle.Dispose();
                hv_right_RightAngle.Dispose();
                hv_DistanceTopDownX.Dispose();
                hv_DistanceLeftRightX.Dispose();
                hv_VisualizationColors.Dispose();
                hv_LeftIndex.Dispose();
                hv_RightIndex.Dispose();
                hv_TopIndex.Dispose();
                hv_DownIndex.Dispose();
                hv_resultcon.Dispose();
                hv_n.Dispose();
                hv_PoseT1.Dispose();
                hv_PoseR1.Dispose();
                hv_PoseD1.Dispose();
                hv_PoseL1.Dispose();
                hv_ObjectModel3DIntersectiont.Dispose();
                hv_numbercount.Dispose();
                hv_lengthnum.Dispose();
                hv_Length.Dispose();
                hv_Max.Dispose();
                hv_Indices.Dispose();
                hv_Numberct.Dispose();
                hv_Row.Dispose();
                hv_Col.Dispose();
                hv_Colmin.Dispose();
                hv_Colmax.Dispose();
                hv_ObjectModel3DIntersectionl.Dispose();
                hv_Lengthl.Dispose();
                hv_Maxl.Dispose();
                hv_Numbercl.Dispose();
                hv_ObjectModel3DIntersectionr.Dispose();
                hv_Lengthr.Dispose();
                hv_Maxr.Dispose();
                hv_Numbercr.Dispose();
                hv_ObjectModel3DIntersectiond.Dispose();
                hv_Lengthd.Dispose();
                hv_Maxd.Dispose();
                hv_Numbercd.Dispose();
                hv_Left3DX.Dispose();
                hv_Right3DX.Dispose();
                hv_Down3DY.Dispose();
                hv_resultcon1.Dispose();
                hv_resultcon2.Dispose();
                hv_resultcon3.Dispose();
                hv_resultcon4.Dispose();
                hv_Rowsleft.Dispose();
                hv_Colsleft.Dispose();
                hv_Rowsrigt.Dispose();
                hv_Colsrigt.Dispose();
                hv_Rowsmidt.Dispose();
                hv_Colsmidt.Dispose();
                hv_Minleft.Dispose();
                hv_Maxleft.Dispose();
                hv_Minrigt.Dispose();
                hv_Maxrigt.Dispose();
                hv_RowBeginleft.Dispose();
                hv_ColBeginleft.Dispose();
                hv_RowEndleft.Dispose();
                hv_ColEndleft.Dispose();
                hv_Nrleft.Dispose();
                hv_Ncleft.Dispose();
                hv_Distleft.Dispose();
                hv_RowBeginrigt.Dispose();
                hv_ColBeginrigt.Dispose();
                hv_RowEndrigt.Dispose();
                hv_ColEndrigt.Dispose();
                hv_Nrrigt.Dispose();
                hv_Ncrigt.Dispose();
                hv_Distrigt.Dispose();
                hv_lengthleftcontour.Dispose();
                hv_lengthleflined.Dispose();
                hv_Abslengthleftcontour.Dispose();
                hv_Abslengthleflined.Dispose();
                hv_countofCols.Dispose();
                hv_Index.Dispose();
                hv_Rowsnewleft.Dispose();
                hv_Colsnewleft.Dispose();
                hv_Indexx.Dispose();
                hv_Index1.Dispose();
                hv_Rowsnewmidt.Dispose();
                hv_Colsnewmidt.Dispose();
                hv_countofRowst.Dispose();
                hv_RowBeginmidt.Dispose();
                hv_ColBeginmidt.Dispose();
                hv_RowEndmidt.Dispose();
                hv_ColEndmidt.Dispose();
                hv_Nrmidt.Dispose();
                hv_Ncmidt.Dispose();
                hv_Distmidt.Dispose();
                hv_lengthrigtcontour.Dispose();
                hv_lengthriglined.Dispose();
                hv_Abslengthrigtcontour.Dispose();
                hv_Abslengthriglined.Dispose();
                hv_Rowsnewrigt.Dispose();
                hv_Colsnewrigt.Dispose();
                hv_Rowslefd.Dispose();
                hv_Colslefd.Dispose();
                hv_Rowsrigd.Dispose();
                hv_Colsrigd.Dispose();
                hv_Rowsmidd.Dispose();
                hv_Colsmidd.Dispose();
                hv_Minlefd.Dispose();
                hv_Maxlefd.Dispose();
                hv_Minrigd.Dispose();
                hv_Maxrigd.Dispose();
                hv_RowBeginlefd.Dispose();
                hv_ColBeginlefd.Dispose();
                hv_RowEndlefd.Dispose();
                hv_ColEndlefd.Dispose();
                hv_Nrlefd.Dispose();
                hv_Nclefd.Dispose();
                hv_Distlefd.Dispose();
                hv_RowBeginrigd.Dispose();
                hv_ColBeginrigd.Dispose();
                hv_RowEndrigd.Dispose();
                hv_ColEndrigd.Dispose();
                hv_Nrrigd.Dispose();
                hv_Ncrigd.Dispose();
                hv_Distrigd.Dispose();
                hv_lengthlefdcontour.Dispose();
                hv_Abslengthlefdcontour.Dispose();
                hv_Rowsnewlefd.Dispose();
                hv_Colsnewlefd.Dispose();
                hv_Rowsnewmidd.Dispose();
                hv_Colsnewmidd.Dispose();
                hv_countofRowsd.Dispose();
                hv_RowBeginmidd.Dispose();
                hv_ColBeginmidd.Dispose();
                hv_RowEndmidd.Dispose();
                hv_ColEndmidd.Dispose();
                hv_Nrmidd.Dispose();
                hv_Ncmidd.Dispose();
                hv_Distmidd.Dispose();
                hv_lengthrigdcontour.Dispose();
                hv_Abslengthrigdcontour.Dispose();
                hv_Rowsnewrigd.Dispose();
                hv_Colsnewrigd.Dispose();
                hv_Rowslefl.Dispose();
                hv_Colslefl.Dispose();
                hv_Rowsrigl.Dispose();
                hv_Colsrigl.Dispose();
                hv_Rowsmidl.Dispose();
                hv_Colsmidl.Dispose();
                hv_Minlefl.Dispose();
                hv_Maxlefl.Dispose();
                hv_Minrigl.Dispose();
                hv_Maxrigl.Dispose();
                hv_RowBeginlefl.Dispose();
                hv_ColBeginlefl.Dispose();
                hv_RowEndlefl.Dispose();
                hv_ColEndlefl.Dispose();
                hv_Nrlefl.Dispose();
                hv_Nclefl.Dispose();
                hv_Distlefl.Dispose();
                hv_RowBeginrigl.Dispose();
                hv_ColBeginrigl.Dispose();
                hv_RowEndrigl.Dispose();
                hv_ColEndrigl.Dispose();
                hv_Nrrigl.Dispose();
                hv_Ncrigl.Dispose();
                hv_Distrigl.Dispose();
                hv_lengthleflcontour.Dispose();
                hv_Abslengthleflcontour.Dispose();
                hv_Rowsnewlefl.Dispose();
                hv_Colsnewlefl.Dispose();
                hv_Rowsnewmidl.Dispose();
                hv_Colsnewmidl.Dispose();
                hv_countofRowsl.Dispose();
                hv_Rowsnewrigl.Dispose();
                hv_Colsnewrigl.Dispose();
                hv_RowBeginmidl.Dispose();
                hv_ColBeginmidl.Dispose();
                hv_RowEndmidl.Dispose();
                hv_ColEndmidl.Dispose();
                hv_Nrmidl.Dispose();
                hv_Ncmidl.Dispose();
                hv_Distmidl.Dispose();
                hv_Rowslefr.Dispose();
                hv_Colslefr.Dispose();
                hv_Rowsrigr.Dispose();
                hv_Colsrigr.Dispose();
                hv_Rowsmidr.Dispose();
                hv_Colsmidr.Dispose();
                hv_Minlefr.Dispose();
                hv_Maxlefr.Dispose();
                hv_Minrigr.Dispose();
                hv_Maxrigr.Dispose();
                hv_RowBeginlefr.Dispose();
                hv_ColBeginlefr.Dispose();
                hv_RowEndlefr.Dispose();
                hv_ColEndlefr.Dispose();
                hv_Nrlefr.Dispose();
                hv_Nclefr.Dispose();
                hv_Distlefr.Dispose();
                hv_RowBeginrigr.Dispose();
                hv_ColBeginrigr.Dispose();
                hv_RowEndrigr.Dispose();
                hv_ColEndrigr.Dispose();
                hv_Nrrigr.Dispose();
                hv_Ncrigr.Dispose();
                hv_Distrigr.Dispose();
                hv_lengthlefrcontour.Dispose();
                hv_Abslengthlefrcontour.Dispose();
                hv_Rowsnewlefr.Dispose();
                hv_Colsnewlefr.Dispose();
                hv_Rowsnewmidr.Dispose();
                hv_Colsnewmidr.Dispose();
                hv_countofRowsr.Dispose();
                hv_RowBeginmidr.Dispose();
                hv_ColBeginmidr.Dispose();
                hv_RowEndmidr.Dispose();
                hv_ColEndmidr.Dispose();
                hv_Nrmidr.Dispose();
                hv_Ncmidr.Dispose();
                hv_Distmidr.Dispose();
                hv_lengthrigrcontour.Dispose();
                hv_Abslengthrigrcontour.Dispose();
                hv_countofColr.Dispose();
                hv_Rowsnewrigr.Dispose();
                hv_Colsnewrigr.Dispose();
                hv_Minl.Dispose();
                hv_Minr.Dispose();
                hv_Angleradttl.Dispose();
                hv_Degtl.Dispose();
                hv_DegtlAbs.Dispose();
                hv_Angleradtr.Dispose();
                hv_Degtr.Dispose();
                hv_DegtrAbs.Dispose();
                hv_Angleraddtl.Dispose();
                hv_Degdl.Dispose();
                hv_DegdlAbs.Dispose();
                hv_Angleraddtr.Dispose();
                hv_DegdrAbs.Dispose();
                hv_Angleradd.Dispose();
                hv_Degtd.Dispose();
                hv_DegtdAbs.Dispose();
                hv_RowsMidt.Dispose();
                hv_ColsMidt.Dispose();
                hv_RowsMidd.Dispose();
                hv_ColsMidd.Dispose();
                hv_RowsMidl.Dispose();
                hv_ColsMidl.Dispose();
                hv_RowsMidr.Dispose();
                hv_ColsMidr.Dispose();
                hv_meanRowst.Dispose();
                hv_meanColst.Dispose();
                hv_meanRowsd.Dispose();
                hv_meanColsd.Dispose();
                hv_meanRowsl.Dispose();
                hv_meanColsl.Dispose();
                hv_meanRowsr.Dispose();
                hv_meanColsr.Dispose();
                hv_minColsd.Dispose();
                hv_maxColsd.Dispose();
                hv_minColst.Dispose();
                hv_maxColst.Dispose();
                hv_minColsl.Dispose();
                hv_maxColsl.Dispose();
                hv_minColsr.Dispose();
                hv_maxColsr.Dispose();
                hv_HomMat2DIdentity.Dispose();
                hv_Areal.Dispose();
                hv_Rowl.Dispose();
                hv_Columnl.Dispose();
                hv_PointOrderl.Dispose();
                hv_HomMatTransPreL.Dispose();
                hv_RowsMidnewt.Dispose();
                hv_ColsMidnewt.Dispose();
                hv_MeanMidnewColt.Dispose();
                hv_MeanMidnewRowy.Dispose();
                hv_RowPrel.Dispose();
                hv_ColumnPrel.Dispose();
                hv_HomMat2DLRotate.Dispose();
                hv_HomMat2DScale.Dispose();
                hv_meanMidColnew.Dispose();
                hv_RowBeginmidtnew.Dispose();
                hv_ColBeginmidtnew.Dispose();
                hv_RowEndmidtnew.Dispose();
                hv_ColEndmidtnew.Dispose();
                hv_Rowminmidt.Dispose();
                hv_SubRow.Dispose();
                hv_RowsTopl.Dispose();
                hv_ColsTopl.Dispose();
                hv_RowsDowl.Dispose();
                hv_ColsDowl.Dispose();
                hv_meanTopl.Dispose();
                hv_meanDowl.Dispose();
                hv_RowBegintopl.Dispose();
                hv_ColBegintopl.Dispose();
                hv_RowEndtopl.Dispose();
                hv_ColEndtopl.Dispose();
                hv_angleltx.Dispose();
                hv_Degltx.Dispose();
                hv_AbgDegltx.Dispose();
                hv_Abgangleltx.Dispose();
                hv_DegSubL.Dispose();
                hv_meanmidColsnew.Dispose();
                hv_Sintl.Dispose();
                hv_Costl.Dispose();
                hv_AbsSintl.Dispose();
                hv_AbsCostl.Dispose();
                hv_DistanceLPre.Dispose();
                hv_RowsnewMidl.Dispose();
                hv_ColsnewMidl.Dispose();
                hv_Left3DLineX.Dispose();
                hv_AbsLeft3Ddistance.Dispose();
                hv_AbsLeft3DX.Dispose();
                hv_Left3DLineY.Dispose();
                hv_Left3DY.Dispose();
                hv_RowIntersectiontl.Dispose();
                hv_ColIntersectiontl.Dispose();
                hv_IsOverlapping.Dispose();
                hv_RowIntersectiont.Dispose();
                hv_ColIntersectiont.Dispose();
                hv_DistanceLT.Dispose();
                hv_MeanMidlx.Dispose();
                hv_AbsmeanRowsl.Dispose();
                hv_DistanceXPre.Dispose();
                hv_RowIntersectiontd.Dispose();
                hv_ColIntersectiontd.Dispose();
                hv_DistanceDL.Dispose();
                hv_Arear.Dispose();
                hv_Rowr.Dispose();
                hv_Columnr.Dispose();
                hv_PointOrderr.Dispose();
                hv_HomMatTransPreR.Dispose();
                hv_RowPrer.Dispose();
                hv_ColumnPrer.Dispose();
                hv_HomMat2DRRotate.Dispose();
                hv_RowBeginmidnewr.Dispose();
                hv_ColBeginmidnewr.Dispose();
                hv_RowEndmidnewr.Dispose();
                hv_ColEndmidnewr.Dispose();
                hv_Rowmidrmin.Dispose();
                hv_Rowsrmidnew.Dispose();
                hv_Colsrmidnew.Dispose();
                hv_RowsTopr.Dispose();
                hv_ColsTopr.Dispose();
                hv_RowsDowr.Dispose();
                hv_ColsDowr.Dispose();
                hv_meanTopr.Dispose();
                hv_meanDowr.Dispose();
                hv_RowBegintopr.Dispose();
                hv_ColBegintopr.Dispose();
                hv_RowEndtopr.Dispose();
                hv_ColEndtopr.Dispose();
                hv_Degrtr.Dispose();
                hv_DegrtrAbs.Dispose();
                hv_DegSubR.Dispose();
                hv_AngleradtrAbs.Dispose();
                hv_Sintr.Dispose();
                hv_Costr.Dispose();
                hv_AbsSintr.Dispose();
                hv_AbsCostr.Dispose();
                hv_DistanceRPre.Dispose();
                hv_RowsnewMidr.Dispose();
                hv_ColsnewMidr.Dispose();
                hv_Right3DLineX.Dispose();
                hv_AbsRight3DDistance.Dispose();
                hv_AbsRight3DX.Dispose();
                hv_Right3DLineY.Dispose();
                hv_Right3DY.Dispose();
                hv_RowIntersectiontr.Dispose();
                hv_ColIntersectiontr.Dispose();
                hv_DistanceRT.Dispose();
                hv_MeanMidrx.Dispose();
                hv_AbsmeanRowsr.Dispose();
                hv_DistanceRD.Dispose();
                hv_Aread.Dispose();
                hv_Rowd.Dispose();
                hv_Columnd.Dispose();
                hv_HomMatTransPreD.Dispose();
                hv_RowPred.Dispose();
                hv_ColumnPred.Dispose();
                hv_PointOrderd.Dispose();
                hv_Rowmiddnew.Dispose();
                hv_Colmiddnew.Dispose();
                hv_meannewmid.Dispose();
                hv_RowBegintopd.Dispose();
                hv_ColBegintopd.Dispose();
                hv_RowEndtopd.Dispose();
                hv_ColEndtopd.Dispose();
                hv_Degltxabs.Dispose();
                hv_DegSubD.Dispose();
                hv_RowBeginmiddnew.Dispose();
                hv_ColBeginmiddnew.Dispose();
                hv_RowEndmiddnew.Dispose();
                hv_ColEndmiddnew.Dispose();
                hv_Distancediagonal.Dispose();
                hv_RowMidDown.Dispose();
                hv_ColMidDown.Dispose();
                hv_meanDownRNew.Dispose();
                hv_meanDownCNew.Dispose();
                hv_distancediag_td.Dispose();
                hv_Down3DLineY.Dispose();
                hv_AbsDown3DDistance.Dispose();
                hv_AbsMeanRowst.Dispose();
                hv_DistanceTD.Dispose();
                hv_AbsmeanRowst.Dispose();
                hv_meanRightC.Dispose();
                hv_meanRightR.Dispose();
                hv_distancediag_lr.Dispose();
                hv_Exception.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_ImageOutLeft.Dispose();
                ho_ImageOutRight.Dispose();
                ho_ImageOutTop.Dispose();
                ho_ImageOutDown.Dispose();
                ho_ImagePart.Dispose();
                ho_ImagePartReal.Dispose();
                ho_ImagePartLeft.Dispose();
                ho_ImagePartLeftNew.Dispose();
                ho_RegionLeft.Dispose();
                ho_ImageReducedLeft.Dispose();
                ho_RegionErosion.Dispose();
                ho_ImagePartRight.Dispose();
                ho_ImagePartRightNew.Dispose();
                ho_RegionRight.Dispose();
                ho_ImageReducedRight.Dispose();
                ho_ImagePartTop.Dispose();
                ho_ImagePartTopNew.Dispose();
                ho_RegionTop.Dispose();
                ho_ImageReducedTop.Dispose();
                ho_ImagePartDown.Dispose();
                ho_ImagePartDownNew.Dispose();
                ho_RegionDown.Dispose();
                ho_ImageReducedDown.Dispose();
                ho_Bx.Dispose();
                ho_By.Dispose();
                ho_Bz.Dispose();
                ho_Intersectiont.Dispose();
                ho_UnionContourst.Dispose();
                ho_ObjectSelectedt.Dispose();
                ho_ContoursSplitt.Dispose();
                ho_SelectedContourst.Dispose();
                ho_contournewmid.Dispose();
                ho_Intersectionl.Dispose();
                ho_UnionContoursl.Dispose();
                ho_ObjectSelectedl.Dispose();
                ho_ContoursSplitl.Dispose();
                ho_SelectedContoursl.Dispose();
                ho_Intersectionr.Dispose();
                ho_UnionContoursr.Dispose();
                ho_ObjectSelectedr.Dispose();
                ho_ContoursSplitr.Dispose();
                ho_SelectedContoursr.Dispose();
                ho_Intersectiond.Dispose();
                ho_UnionContoursd.Dispose();
                ho_ObjectSelectedd.Dispose();
                ho_ContoursSplitd.Dispose();
                ho_SelectedContoursd.Dispose();
                ho_contourleft.Dispose();
                ho_contourmidt.Dispose();
                ho_contourrigt.Dispose();
                ho_contourlefr.Dispose();
                ho_contourmidr.Dispose();
                ho_contourrigr.Dispose();
                ho_contourlefl.Dispose();
                ho_contourmidl.Dispose();
                ho_contourrigl.Dispose();
                ho_contourlefd.Dispose();
                ho_contourmidd.Dispose();
                ho_contourrigd.Dispose();
                ho_contourtmp.Dispose();
                ho_SelectedContourslPre.Dispose();
                ho_contourtoplnew.Dispose();
                ho_contourmidlnew.Dispose();
                ho_contourdowlnew.Dispose();
                ho_SelectedContourslfinal.Dispose();
                ho_SelectedContourslnewpre.Dispose();
                ho_SelectedContourslprenew.Dispose();
                ho_SelectedContourslnew.Dispose();
                ho_SelectedContoursrPre.Dispose();
                ho_SelectedContoursrfinal.Dispose();
                ho_SelectedContoursrnewpre.Dispose();
                ho_contourtoprnew.Dispose();
                ho_contourmidrnew.Dispose();
                ho_contourdowrnew.Dispose();
                ho_SelectedContoursrprenew.Dispose();
                ho_SelectedContoursrnew.Dispose();
                ho_SelectedContoursdPre.Dispose();
                ho_contourmiddnew.Dispose();
                ho_SelectedContoursdnewpre.Dispose();
                ho_contourtopdnew.Dispose();
                ho_contourdowdnew.Dispose();
                ho_SelectedContoursdnewnpre.Dispose();
                ho_SelectedContoursdprenew.Dispose();
                ho_SelectedContoursdnew.Dispose();
                ho_SelectedContoursrnewnpre.Dispose();

                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_Instructions.Dispose();
                hv_WindowHandle.Dispose();
                hv_Message.Dispose();
                hv_r.Dispose();
                hv_ParameterValues.Dispose();
                hv_Status.Dispose();
                hv_yInterval.Dispose();
                hv_ObjectModel3DBLeft.Dispose();
                hv_ObjectModel3DBLeftConnected.Dispose();
                hv_ObjectModel3DBLeftNew.Dispose();
                hv_ObjectModel3DBRight.Dispose();
                hv_ObjectModel3DBRightConnected.Dispose();
                hv_ObjectModel3DBRightNew.Dispose();
                hv_ObjectModel3DBTop.Dispose();
                hv_ObjectModel3DTopConnected.Dispose();
                hv_ObjectModel3DBTopNew.Dispose();
                hv_ObjectModel3DBDown.Dispose();
                hv_ObjectModel3DBDownConnected.Dispose();
                hv_ObjectModel3DBDownNew.Dispose();
                hv_PoseOut.Dispose();
                hv_SampledObjectModel3DTop.Dispose();
                hv_SampledObjectModel3DDown.Dispose();
                hv_SampledObjectModel3DLeft.Dispose();
                hv_SampledObjectModel3DRight.Dispose();
                hv_Surface3DDefaultT.Dispose();
                hv_Info.Dispose();
                hv_Surface3DDefaultD.Dispose();
                hv_Surface3DDefaultL.Dispose();
                hv_Surface3DDefaultR.Dispose();
                hv_CenterPointT.Dispose();
                hv_Radius.Dispose();
                hv_CenterPointD.Dispose();
                hv_CenterPointL.Dispose();
                hv_CenterPointR.Dispose();
                hv_raduis.Dispose();
                hv_ObjectModel3DIntersections.Dispose();
                hv_DisparityProfileWidth.Dispose();
                hv_DisparityProfileHeight.Dispose();
                hv_WindowEnlargement.Dispose();
                hv_WindowWidth.Dispose();
                hv_WindowHeight.Dispose();
                hv_VisualizationPlaneSize.Dispose();
                hv_PoseT.Dispose();
                hv_PoseD.Dispose();
                hv_PoseL.Dispose();
                hv_PoseR.Dispose();
                hv_IntersectionPlane1.Dispose();
                hv_IntersectionPlane2.Dispose();
                hv_IntersectionPlane3.Dispose();
                hv_IntersectionPlane4.Dispose();
                hv_IntersectionPlane5.Dispose();
                hv_top_Hypotenuse.Dispose();
                hv_top_LefttAngle.Dispose();
                hv_top_RightAngle.Dispose();
                hv_top_HypotenuseFH.Dispose();
                hv_top_HypotenuseSH.Dispose();
                hv_topdown_HypotenuseDis.Dispose();
                hv_leftright_HypotenuseDis.Dispose();
                hv_left_HypotenuseFH.Dispose();
                hv_left_HypotenuseSH.Dispose();
                hv_lefttop_lineLength.Dispose();
                hv_righttop_lineLength.Dispose();
                hv_leftdown_lineLength.Dispose();
                hv_rightdown_linelength.Dispose();
                hv_left_Hypotenuse.Dispose();
                hv_left_LefttAngle.Dispose();
                hv_left_RightAngle.Dispose();
                hv_right_HypotenuseFH.Dispose();
                hv_right_HypotenuseSH.Dispose();
                hv_down_HypotenuseFH.Dispose();
                hv_down_HypotenuseSH.Dispose();
                hv_down_Hypotenuse.Dispose();
                hv_down_LefttAngle.Dispose();
                hv_down_RightAngle.Dispose();
                hv_right_Hypotenuse.Dispose();
                hv_right_LefttAngle.Dispose();
                hv_right_RightAngle.Dispose();
                hv_DistanceTopDownX.Dispose();
                hv_DistanceLeftRightX.Dispose();
                hv_VisualizationColors.Dispose();
                hv_LeftIndex.Dispose();
                hv_RightIndex.Dispose();
                hv_TopIndex.Dispose();
                hv_DownIndex.Dispose();
                hv_resultcon.Dispose();
                hv_n.Dispose();
                hv_PoseT1.Dispose();
                hv_PoseR1.Dispose();
                hv_PoseD1.Dispose();
                hv_PoseL1.Dispose();
                hv_ObjectModel3DIntersectiont.Dispose();
                hv_numbercount.Dispose();
                hv_lengthnum.Dispose();
                hv_Length.Dispose();
                hv_Max.Dispose();
                hv_Indices.Dispose();
                hv_Numberct.Dispose();
                hv_Row.Dispose();
                hv_Col.Dispose();
                hv_Colmin.Dispose();
                hv_Colmax.Dispose();
                hv_ObjectModel3DIntersectionl.Dispose();
                hv_Lengthl.Dispose();
                hv_Maxl.Dispose();
                hv_Numbercl.Dispose();
                hv_ObjectModel3DIntersectionr.Dispose();
                hv_Lengthr.Dispose();
                hv_Maxr.Dispose();
                hv_Numbercr.Dispose();
                hv_ObjectModel3DIntersectiond.Dispose();
                hv_Lengthd.Dispose();
                hv_Maxd.Dispose();
                hv_Numbercd.Dispose();
                hv_Left3DX.Dispose();
                hv_Right3DX.Dispose();
                hv_Down3DY.Dispose();
                hv_resultcon1.Dispose();
                hv_resultcon2.Dispose();
                hv_resultcon3.Dispose();
                hv_resultcon4.Dispose();
                hv_Rowsleft.Dispose();
                hv_Colsleft.Dispose();
                hv_Rowsrigt.Dispose();
                hv_Colsrigt.Dispose();
                hv_Rowsmidt.Dispose();
                hv_Colsmidt.Dispose();
                hv_Minleft.Dispose();
                hv_Maxleft.Dispose();
                hv_Minrigt.Dispose();
                hv_Maxrigt.Dispose();
                hv_RowBeginleft.Dispose();
                hv_ColBeginleft.Dispose();
                hv_RowEndleft.Dispose();
                hv_ColEndleft.Dispose();
                hv_Nrleft.Dispose();
                hv_Ncleft.Dispose();
                hv_Distleft.Dispose();
                hv_RowBeginrigt.Dispose();
                hv_ColBeginrigt.Dispose();
                hv_RowEndrigt.Dispose();
                hv_ColEndrigt.Dispose();
                hv_Nrrigt.Dispose();
                hv_Ncrigt.Dispose();
                hv_Distrigt.Dispose();
                hv_lengthleftcontour.Dispose();
                hv_lengthleflined.Dispose();
                hv_Abslengthleftcontour.Dispose();
                hv_Abslengthleflined.Dispose();
                hv_countofCols.Dispose();
                hv_Index.Dispose();
                hv_Rowsnewleft.Dispose();
                hv_Colsnewleft.Dispose();
                hv_Indexx.Dispose();
                hv_Index1.Dispose();
                hv_Rowsnewmidt.Dispose();
                hv_Colsnewmidt.Dispose();
                hv_countofRowst.Dispose();
                hv_RowBeginmidt.Dispose();
                hv_ColBeginmidt.Dispose();
                hv_RowEndmidt.Dispose();
                hv_ColEndmidt.Dispose();
                hv_Nrmidt.Dispose();
                hv_Ncmidt.Dispose();
                hv_Distmidt.Dispose();
                hv_lengthrigtcontour.Dispose();
                hv_lengthriglined.Dispose();
                hv_Abslengthrigtcontour.Dispose();
                hv_Abslengthriglined.Dispose();
                hv_Rowsnewrigt.Dispose();
                hv_Colsnewrigt.Dispose();
                hv_Rowslefd.Dispose();
                hv_Colslefd.Dispose();
                hv_Rowsrigd.Dispose();
                hv_Colsrigd.Dispose();
                hv_Rowsmidd.Dispose();
                hv_Colsmidd.Dispose();
                hv_Minlefd.Dispose();
                hv_Maxlefd.Dispose();
                hv_Minrigd.Dispose();
                hv_Maxrigd.Dispose();
                hv_RowBeginlefd.Dispose();
                hv_ColBeginlefd.Dispose();
                hv_RowEndlefd.Dispose();
                hv_ColEndlefd.Dispose();
                hv_Nrlefd.Dispose();
                hv_Nclefd.Dispose();
                hv_Distlefd.Dispose();
                hv_RowBeginrigd.Dispose();
                hv_ColBeginrigd.Dispose();
                hv_RowEndrigd.Dispose();
                hv_ColEndrigd.Dispose();
                hv_Nrrigd.Dispose();
                hv_Ncrigd.Dispose();
                hv_Distrigd.Dispose();
                hv_lengthlefdcontour.Dispose();
                hv_Abslengthlefdcontour.Dispose();
                hv_Rowsnewlefd.Dispose();
                hv_Colsnewlefd.Dispose();
                hv_Rowsnewmidd.Dispose();
                hv_Colsnewmidd.Dispose();
                hv_countofRowsd.Dispose();
                hv_RowBeginmidd.Dispose();
                hv_ColBeginmidd.Dispose();
                hv_RowEndmidd.Dispose();
                hv_ColEndmidd.Dispose();
                hv_Nrmidd.Dispose();
                hv_Ncmidd.Dispose();
                hv_Distmidd.Dispose();
                hv_lengthrigdcontour.Dispose();
                hv_Abslengthrigdcontour.Dispose();
                hv_Rowsnewrigd.Dispose();
                hv_Colsnewrigd.Dispose();
                hv_Rowslefl.Dispose();
                hv_Colslefl.Dispose();
                hv_Rowsrigl.Dispose();
                hv_Colsrigl.Dispose();
                hv_Rowsmidl.Dispose();
                hv_Colsmidl.Dispose();
                hv_Minlefl.Dispose();
                hv_Maxlefl.Dispose();
                hv_Minrigl.Dispose();
                hv_Maxrigl.Dispose();
                hv_RowBeginlefl.Dispose();
                hv_ColBeginlefl.Dispose();
                hv_RowEndlefl.Dispose();
                hv_ColEndlefl.Dispose();
                hv_Nrlefl.Dispose();
                hv_Nclefl.Dispose();
                hv_Distlefl.Dispose();
                hv_RowBeginrigl.Dispose();
                hv_ColBeginrigl.Dispose();
                hv_RowEndrigl.Dispose();
                hv_ColEndrigl.Dispose();
                hv_Nrrigl.Dispose();
                hv_Ncrigl.Dispose();
                hv_Distrigl.Dispose();
                hv_lengthleflcontour.Dispose();
                hv_Abslengthleflcontour.Dispose();
                hv_Rowsnewlefl.Dispose();
                hv_Colsnewlefl.Dispose();
                hv_Rowsnewmidl.Dispose();
                hv_Colsnewmidl.Dispose();
                hv_countofRowsl.Dispose();
                hv_Rowsnewrigl.Dispose();
                hv_Colsnewrigl.Dispose();
                hv_RowBeginmidl.Dispose();
                hv_ColBeginmidl.Dispose();
                hv_RowEndmidl.Dispose();
                hv_ColEndmidl.Dispose();
                hv_Nrmidl.Dispose();
                hv_Ncmidl.Dispose();
                hv_Distmidl.Dispose();
                hv_Rowslefr.Dispose();
                hv_Colslefr.Dispose();
                hv_Rowsrigr.Dispose();
                hv_Colsrigr.Dispose();
                hv_Rowsmidr.Dispose();
                hv_Colsmidr.Dispose();
                hv_Minlefr.Dispose();
                hv_Maxlefr.Dispose();
                hv_Minrigr.Dispose();
                hv_Maxrigr.Dispose();
                hv_RowBeginlefr.Dispose();
                hv_ColBeginlefr.Dispose();
                hv_RowEndlefr.Dispose();
                hv_ColEndlefr.Dispose();
                hv_Nrlefr.Dispose();
                hv_Nclefr.Dispose();
                hv_Distlefr.Dispose();
                hv_RowBeginrigr.Dispose();
                hv_ColBeginrigr.Dispose();
                hv_RowEndrigr.Dispose();
                hv_ColEndrigr.Dispose();
                hv_Nrrigr.Dispose();
                hv_Ncrigr.Dispose();
                hv_Distrigr.Dispose();
                hv_lengthlefrcontour.Dispose();
                hv_Abslengthlefrcontour.Dispose();
                hv_Rowsnewlefr.Dispose();
                hv_Colsnewlefr.Dispose();
                hv_Rowsnewmidr.Dispose();
                hv_Colsnewmidr.Dispose();
                hv_countofRowsr.Dispose();
                hv_RowBeginmidr.Dispose();
                hv_ColBeginmidr.Dispose();
                hv_RowEndmidr.Dispose();
                hv_ColEndmidr.Dispose();
                hv_Nrmidr.Dispose();
                hv_Ncmidr.Dispose();
                hv_Distmidr.Dispose();
                hv_lengthrigrcontour.Dispose();
                hv_Abslengthrigrcontour.Dispose();
                hv_countofColr.Dispose();
                hv_Rowsnewrigr.Dispose();
                hv_Colsnewrigr.Dispose();
                hv_Minl.Dispose();
                hv_Minr.Dispose();
                hv_Angleradttl.Dispose();
                hv_Degtl.Dispose();
                hv_DegtlAbs.Dispose();
                hv_Angleradtr.Dispose();
                hv_Degtr.Dispose();
                hv_DegtrAbs.Dispose();
                hv_Angleraddtl.Dispose();
                hv_Degdl.Dispose();
                hv_DegdlAbs.Dispose();
                hv_Angleraddtr.Dispose();
                hv_DegdrAbs.Dispose();
                hv_Angleradd.Dispose();
                hv_Degtd.Dispose();
                hv_DegtdAbs.Dispose();
                hv_RowsMidt.Dispose();
                hv_ColsMidt.Dispose();
                hv_RowsMidd.Dispose();
                hv_ColsMidd.Dispose();
                hv_RowsMidl.Dispose();
                hv_ColsMidl.Dispose();
                hv_RowsMidr.Dispose();
                hv_ColsMidr.Dispose();
                hv_meanRowst.Dispose();
                hv_meanColst.Dispose();
                hv_meanRowsd.Dispose();
                hv_meanColsd.Dispose();
                hv_meanRowsl.Dispose();
                hv_meanColsl.Dispose();
                hv_meanRowsr.Dispose();
                hv_meanColsr.Dispose();
                hv_minColsd.Dispose();
                hv_maxColsd.Dispose();
                hv_minColst.Dispose();
                hv_maxColst.Dispose();
                hv_minColsl.Dispose();
                hv_maxColsl.Dispose();
                hv_minColsr.Dispose();
                hv_maxColsr.Dispose();
                hv_HomMat2DIdentity.Dispose();
                hv_Areal.Dispose();
                hv_Rowl.Dispose();
                hv_Columnl.Dispose();
                hv_PointOrderl.Dispose();
                hv_HomMatTransPreL.Dispose();
                hv_RowsMidnewt.Dispose();
                hv_ColsMidnewt.Dispose();
                hv_MeanMidnewColt.Dispose();
                hv_MeanMidnewRowy.Dispose();
                hv_RowPrel.Dispose();
                hv_ColumnPrel.Dispose();
                hv_HomMat2DLRotate.Dispose();
                hv_HomMat2DScale.Dispose();
                hv_meanMidColnew.Dispose();
                hv_RowBeginmidtnew.Dispose();
                hv_ColBeginmidtnew.Dispose();
                hv_RowEndmidtnew.Dispose();
                hv_ColEndmidtnew.Dispose();
                hv_Rowminmidt.Dispose();
                hv_SubRow.Dispose();
                hv_RowsTopl.Dispose();
                hv_ColsTopl.Dispose();
                hv_RowsDowl.Dispose();
                hv_ColsDowl.Dispose();
                hv_meanTopl.Dispose();
                hv_meanDowl.Dispose();
                hv_RowBegintopl.Dispose();
                hv_ColBegintopl.Dispose();
                hv_RowEndtopl.Dispose();
                hv_ColEndtopl.Dispose();
                hv_angleltx.Dispose();
                hv_Degltx.Dispose();
                hv_AbgDegltx.Dispose();
                hv_Abgangleltx.Dispose();
                hv_DegSubL.Dispose();
                hv_meanmidColsnew.Dispose();
                hv_Sintl.Dispose();
                hv_Costl.Dispose();
                hv_AbsSintl.Dispose();
                hv_AbsCostl.Dispose();
                hv_DistanceLPre.Dispose();
                hv_RowsnewMidl.Dispose();
                hv_ColsnewMidl.Dispose();
                hv_Left3DLineX.Dispose();
                hv_AbsLeft3Ddistance.Dispose();
                hv_AbsLeft3DX.Dispose();
                hv_Left3DLineY.Dispose();
                hv_Left3DY.Dispose();
                hv_RowIntersectiontl.Dispose();
                hv_ColIntersectiontl.Dispose();
                hv_IsOverlapping.Dispose();
                hv_RowIntersectiont.Dispose();
                hv_ColIntersectiont.Dispose();
                hv_DistanceLT.Dispose();
                hv_MeanMidlx.Dispose();
                hv_AbsmeanRowsl.Dispose();
                hv_DistanceXPre.Dispose();
                hv_RowIntersectiontd.Dispose();
                hv_ColIntersectiontd.Dispose();
                hv_DistanceDL.Dispose();
                hv_Arear.Dispose();
                hv_Rowr.Dispose();
                hv_Columnr.Dispose();
                hv_PointOrderr.Dispose();
                hv_HomMatTransPreR.Dispose();
                hv_RowPrer.Dispose();
                hv_ColumnPrer.Dispose();
                hv_HomMat2DRRotate.Dispose();
                hv_RowBeginmidnewr.Dispose();
                hv_ColBeginmidnewr.Dispose();
                hv_RowEndmidnewr.Dispose();
                hv_ColEndmidnewr.Dispose();
                hv_Rowmidrmin.Dispose();
                hv_Rowsrmidnew.Dispose();
                hv_Colsrmidnew.Dispose();
                hv_RowsTopr.Dispose();
                hv_ColsTopr.Dispose();
                hv_RowsDowr.Dispose();
                hv_ColsDowr.Dispose();
                hv_meanTopr.Dispose();
                hv_meanDowr.Dispose();
                hv_RowBegintopr.Dispose();
                hv_ColBegintopr.Dispose();
                hv_RowEndtopr.Dispose();
                hv_ColEndtopr.Dispose();
                hv_Degrtr.Dispose();
                hv_DegrtrAbs.Dispose();
                hv_DegSubR.Dispose();
                hv_AngleradtrAbs.Dispose();
                hv_Sintr.Dispose();
                hv_Costr.Dispose();
                hv_AbsSintr.Dispose();
                hv_AbsCostr.Dispose();
                hv_DistanceRPre.Dispose();
                hv_RowsnewMidr.Dispose();
                hv_ColsnewMidr.Dispose();
                hv_Right3DLineX.Dispose();
                hv_AbsRight3DDistance.Dispose();
                hv_AbsRight3DX.Dispose();
                hv_Right3DLineY.Dispose();
                hv_Right3DY.Dispose();
                hv_RowIntersectiontr.Dispose();
                hv_ColIntersectiontr.Dispose();
                hv_DistanceRT.Dispose();
                hv_MeanMidrx.Dispose();
                hv_AbsmeanRowsr.Dispose();
                hv_DistanceRD.Dispose();
                hv_Aread.Dispose();
                hv_Rowd.Dispose();
                hv_Columnd.Dispose();
                hv_HomMatTransPreD.Dispose();
                hv_RowPred.Dispose();
                hv_ColumnPred.Dispose();
                hv_PointOrderd.Dispose();
                hv_Rowmiddnew.Dispose();
                hv_Colmiddnew.Dispose();
                hv_meannewmid.Dispose();
                hv_RowBegintopd.Dispose();
                hv_ColBegintopd.Dispose();
                hv_RowEndtopd.Dispose();
                hv_ColEndtopd.Dispose();
                hv_Degltxabs.Dispose();
                hv_DegSubD.Dispose();
                hv_RowBeginmiddnew.Dispose();
                hv_ColBeginmiddnew.Dispose();
                hv_RowEndmiddnew.Dispose();
                hv_ColEndmiddnew.Dispose();
                hv_Distancediagonal.Dispose();
                hv_RowMidDown.Dispose();
                hv_ColMidDown.Dispose();
                hv_meanDownRNew.Dispose();
                hv_meanDownCNew.Dispose();
                hv_distancediag_td.Dispose();
                hv_Down3DLineY.Dispose();
                hv_AbsDown3DDistance.Dispose();
                hv_AbsMeanRowst.Dispose();
                hv_DistanceTD.Dispose();
                hv_AbsmeanRowst.Dispose();
                hv_meanRightC.Dispose();
                hv_meanRightR.Dispose();
                hv_distancediag_lr.Dispose();
                hv_Exception.Dispose();

            }
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

                //鐢熸垚XY鍥惧儚
                hv_xInterval.Dispose();
                hv_xInterval = 0.015;
                //yInterval := 0.09
                ho_ImageX.Dispose();
                HOperatorSet.GenImageSurfaceFirstOrder(out ho_ImageX, "real", 0, hv_xInterval,
                    0, 0, 0, hv_Width, hv_Height);
                ho_ImageY.Dispose();
                HOperatorSet.GenImageSurfaceFirstOrder(out ho_ImageY, "real", hv_yInterval,
                    0, 0, 0, 0, hv_Width, hv_Height);

                //瀵逛簬real绫诲瀷鐨勫浘鍍忎笉鍐嶉渶瑕佷竴涓嬫搷浣�
                ho_ImageConvertedX.Dispose();
                HOperatorSet.ConvertImageType(ho_ImageX, out ho_ImageConvertedX, "real");
                ho_ImageConvertedY.Dispose();
                HOperatorSet.ConvertImageType(ho_ImageY, out ho_ImageConvertedY, "real");
                ho_ImageConvertedZ.Dispose();
                HOperatorSet.ConvertImageType(ho_ImageZ, out ho_ImageConvertedZ, "real");

                //璁＄畻鏁版嵁鏈€澶ф渶灏忓€�
                hv_Min.Dispose(); hv_Max.Dispose(); hv_Range.Dispose();
                HOperatorSet.MinMaxGray(ho_ImageZ, ho_ImageZ, 0, out hv_Min, out hv_Max, out hv_Range);
                ho_Region.Dispose();
                HOperatorSet.Threshold(ho_ImageZ, out ho_Region, -1500, hv_Max);
                ho_ImageReducedZ.Dispose();
                HOperatorSet.ReduceDomain(ho_ImageZ, ho_Region, out ho_ImageReducedZ);
                //璁剧疆Z鏂瑰悜缂╂斁绯绘暟,鏈€缁堥珮搴� = ScaleZ * GrayValue鍊硷紝ScaleZ瓒婂皬楂樺害鍊艰寖鍥翠篃瓒婂皬锛岀偣浜戞晥鏋滄帴杩戝钩闈紝鍙嶄箣瓒婃槑鏄�
                //纭繚鍦�0鍊间互涓婏紝鍙€氳繃min_max_gray鑾峰彇鏈€灏忓€肩劧鍚庢媺浼稿埌0浠ヤ笂
                hv_ScaleZ.Dispose();
                hv_ScaleZ = 1.0;
                ho_ImageReducedZ1.Dispose();
                HOperatorSet.ScaleImage(ho_ImageReducedZ, out ho_ImageReducedZ1, hv_ScaleZ,
                    0);
                hv_ObjectModel3D.Dispose();
                HOperatorSet.XyzToObjectModel3d(ho_ImageConvertedX, ho_ImageConvertedY, ho_ImageConvertedZ,
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

       

        public void Creat_XYZ_From_sszn_COPY_New(HObject ho_ImageZ, out HObject ho_ImageConvertedX,
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
                hv_xInterval = 0.015;
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
                HOperatorSet.Threshold(ho_ImageZ, out ho_Region, -10, 500);
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

            }
        }


        public void project_object_model_3d_lines_to_contour_xld(out HObject ho_Intersection,
            HTuple hv_PoseIntersectionPlane, HTuple hv_ObjectModel3DIntersection, out HTuple hv_resultcon)
        {


            // Local iconic variables 

            // Local control variables 

            HTuple hv_PoseInvert = new HTuple(), hv_Diameter = new HTuple();
            HTuple hv_Exception = new HTuple(), hv_Scale = new HTuple();
            HTuple hv_CamParam = new HTuple();
            HTuple hv_haspoints = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Intersection);
            hv_resultcon = new HTuple();
            try
            {

                //Determine the intersections and convert them into XLD contours

                //The inverted intersection plane pose is our projection pose
                hv_PoseInvert.Dispose();
                HOperatorSet.PoseInvert(hv_PoseIntersectionPlane, out hv_PoseInvert);
                //Make sure, the projection plane lies in front of the camera
                try
                {
                    hv_haspoints.Dispose();
                    hv_Diameter.Dispose();

                    HOperatorSet.GetObjectModel3dParams(hv_ObjectModel3DIntersection, "has_points", out hv_haspoints);
                    if (hv_haspoints.S.CompareTo("true") == 0)
                    {

                        HOperatorSet.GetObjectModel3dParams(hv_ObjectModel3DIntersection, "diameter_axis_aligned_bounding_box",
                            out hv_Diameter);
                    }
                    else
                    {
                        hv_resultcon = 0;
                        return;
                    }
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    hv_resultcon.Dispose();
                    hv_resultcon = 0;

                    hv_PoseInvert.Dispose();
                    hv_Diameter.Dispose();
                    hv_Exception.Dispose();
                    hv_Scale.Dispose();
                    hv_CamParam.Dispose();

                    return;
                }
                if (hv_PoseInvert == null)
                    hv_PoseInvert = new HTuple();
                hv_PoseInvert[2] = (hv_PoseInvert.TupleSelect(2)) + hv_Diameter;
                //Use a parallel projection to achieve the desired scaling (default 1:1)
                hv_Scale.Dispose();
                hv_Scale = 1;
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_CamParam.Dispose();
                    hv_CamParam = hv_CamParam.TupleConcat(new HTuple[] { "area_scan_telecentric_division", 1.0, 0, 1.0 / hv_Scale, 1.0 / hv_Scale, 0, 0, 512, 512 });
                }

                //using (HDevDisposeHelper dh = new HDevDisposeHelper())
                //{
                //    hv_CamParam.Dispose();
                //    gen_cam_par_area_scan_telecentric_division(1.0, 0, 1.0 / hv_Scale, 1.0 / hv_Scale,
                //        0, 0, 512, 512, out hv_CamParam);
                //}
                ho_Intersection.Dispose();
                HOperatorSet.ProjectObjectModel3d(out ho_Intersection, hv_ObjectModel3DIntersection,
                    hv_CamParam, hv_PoseInvert, "data", "lines");
                hv_resultcon.Dispose();
                hv_resultcon = 1;

                hv_PoseInvert.Dispose();
                hv_Diameter.Dispose();
                hv_Exception.Dispose();
                hv_Scale.Dispose();
                hv_CamParam.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_PoseInvert.Dispose();
                hv_Diameter.Dispose();
                hv_Exception.Dispose();
                hv_Scale.Dispose();
                hv_CamParam.Dispose();

                throw HDevExpDefaultException;
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

    }
}
