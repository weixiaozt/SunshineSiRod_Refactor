using Newtonsoft.Json.Linq;
using Sunny.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YouEyEE.Untils.Enum;
using YouEyEE.Untils.Log;
using YouEyEE.Untils.PublicVar;
using YouEyEE.Untils.Unilt;
using YouEyEE_Redis.StackExchange;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SiliconRoundBarCheck.Data;
using SiliconRoundBarCheck.Parameters;

namespace SiliconRoundBarCheck.Tools
{
    internal class RedisTool
    {

        private static RedisTool _instance;
        private SERedisOperation redisOperation = null;
        public static RedisTool Instance()
        {
            if (_instance == null)
            {
                _instance = new RedisTool();
            }
            return _instance;
        }

        private SUBChannelType _nSubChannelType;  //Redis交互类型

        private string[] _strSubScribeMessageChannelName; //订阅消息频道

        private string _strSubScribeImageChannelName; //订阅图片频道

        public SUBChannelType NSubChannelType { get => _nSubChannelType; set => _nSubChannelType = value; }
        public string[] StrSubScribeMessageChannelName { get => _strSubScribeMessageChannelName; set => _strSubScribeMessageChannelName = value; }
        public string StrSubScribeImageChannelName { get => _strSubScribeImageChannelName; set => _strSubScribeImageChannelName = value; }

        private RedisTool() 
        {
            _nSubChannelType = SUBChannelType.SUBChannel1;
        }

        /*
         * SUBConnalIndex 与Redis客户端对接凭证  handle  1 ~ 4
         * subChannel 订阅子频道名称
         * str内容 
         */
        private void RedisOperation_RedisSubMessageEvent(int SUBConnalIndex, string subChannel, string str)
        {
             LogHelper.Info("Silicon","RedisOperation_RedisSubMessageEvent SUBConnalIndex： " + SUBConnalIndex.ToString() + " subChannel： " + subChannel);
            if (NSubChannelType == SUBChannelType.NULL) { return; }
            try
            {
                
                if (SUBConnalIndex == (int)NSubChannelType)
                {
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            bool bFind = false;
                            for (int i = 0; i < StrSubScribeMessageChannelName.Length; i++)
                            {
                                if (subChannel == StrSubScribeMessageChannelName[i])
                                { 
                                    bFind = true; 
                                    break; 
                                }
                                
                            }

                            if (bFind)
                            {
                                JObject objectinfo = JsonUtils.AnalayJsonStringToJObject(str);
                                StickData data = new StickData();

                                string strOutInfo = objectinfo["外观"].ToString();                              
                                JObject objOut = JsonConvert.DeserializeObject<JObject>(strOutInfo);

                                float fAppearanceLength = float.Parse(objOut["长度"].ToString());
                                float fAppearanceValidLength = float.Parse(objOut["有效长度"].ToString());
                                float fAppearanceMaxRadius = float.Parse(objOut["最大直径"].ToString());
                                float fAppearanceMinRadius = float.Parse(objOut["最小直径"].ToString());
                                string strSiliconStickNum = objectinfo["晶编"].ToString();


                                data.FMaxRadius = fAppearanceMaxRadius;
                                data.FMinRadius = fAppearanceMinRadius;
                                data.FLength = fAppearanceLength;
                                data.FValidLength = fAppearanceValidLength;


                                 LogHelper.Info("Silicon","Redis Get Message " + strSiliconStickNum);

                                string strSiliconLine = objectinfo["晶线"].ToString();

                               
                                JObject objSiliconLine = JsonConvert.DeserializeObject<JObject>(strSiliconLine);

                                string strLineNum = objSiliconLine["根数"].ToString();
                                int nSiliconLineNum = int.Parse(strLineNum);

                              

                                string strAbnormalArea_Fir = objSiliconLine["异常区域1"].ToString();
                                List<float> objArrFir = JsonConvert.DeserializeObject<List<float>>(strAbnormalArea_Fir);

                                string strInsertAbnormalArea_Fir = "";
                                foreach (var info in objArrFir)
                                {
                                    strInsertAbnormalArea_Fir += info.ToString("0.00000") + ",";
                                }

                                if ((objArrFir != null && objArrFir.Count == 2) && (objArrFir[0] != 0 && objArrFir[1] != 0))
                                {
                                    data.ArrAbnormalArea.Add(objArrFir);
                                }

                                string strAbnormalArea_Sec = objSiliconLine["异常区域2"].ToString();
                                List<float> objArrSec = JsonConvert.DeserializeObject<List<float>>(strAbnormalArea_Sec);

                                string strInsertAbnormalArea_Sec = "";
                                foreach (var info in objArrSec)
                                {
                                    strInsertAbnormalArea_Sec += info.ToString("0.00000") + ",";
                                }

                                if ((objArrSec != null && objArrSec.Count == 2) && ( objArrSec[0] != 0 && objArrSec[1] != 0))
                                {
                                    data.ArrAbnormalArea.Add(objArrSec);
                                }


                                string strAbnormalArea_Third = objSiliconLine["异常区域3"].ToString();
                                List<float> objArrThr = JsonConvert.DeserializeObject<List<float>>(strAbnormalArea_Third);

                                string strInsertAbnormalArea_Third = "";
                                foreach (var info in objArrThr)
                                {
                                    strInsertAbnormalArea_Third += info.ToString("0.00000") + ",";
                                }

                                if ((objArrThr != null && objArrThr.Count == 2) && (objArrThr[0] != 0 && objArrThr[1] != 0))
                                {
                                    data.ArrAbnormalArea.Add(objArrThr);
                                }

                                string strAbnormalArea_Fourth = objSiliconLine["异常区域4"].ToString();
                                List<float> objArrFourth = JsonConvert.DeserializeObject<List<float>>(strAbnormalArea_Fourth);

                                string strInsertAbnormalArea_Fourth = "";
                                foreach (var info in objArrFourth)
                                {
                                    strInsertAbnormalArea_Fourth += info.ToString("0.00000") + ",";
                                }
                                if ((objArrFourth != null && objArrFourth.Count == 2) && (objArrFourth[0] != 0 && objArrFourth[1] != 0))
                                {
                                    data.ArrAbnormalArea.Add(objArrFourth);
                                }
                                
                                string strDrawLine = objectinfo["画线"].ToString();                              
                                JObject objDrawLines = JsonConvert.DeserializeObject<JObject>(strDrawLine);

                                string strDrawLine_Fir = objDrawLines["画线位置1"].ToString();
                                float fDrawLineFir = float.Parse(strDrawLine_Fir);

                                string strDrawLine_Sec = objDrawLines["画线位置2"].ToString();
                                float fDrawLineSec = float.Parse(strDrawLine_Sec);


                                string strDrawLine_Thr = objDrawLines["画线位置3"].ToString();
                                float fDrawLineThr = float.Parse(strDrawLine_Thr);

                                string strDrawLine_Fourth = objDrawLines["画线位置4"].ToString();
                                float fDrawLineFourth = float.Parse(strDrawLine_Fourth);

                                string strDrawLine_Fifth = objDrawLines["画线位置5"].ToString();
                                float fDrawLineFifth = float.Parse(strDrawLine_Fifth);

                                string strDrawLine_Sixth = objDrawLines["画线位置6"].ToString();
                                float fDrawLineSixth = float.Parse(strDrawLine_Sixth);

                                string strDrawLine_Seventh = objDrawLines["画线位置7"].ToString();
                                float fDrawLineSeventh = float.Parse(strDrawLine_Seventh);

                                string strDrawLine_Eighth = objDrawLines["画线位置8"].ToString();
                                float fDrawLineEighth = float.Parse(strDrawLine_Eighth);

                                string strDrawLine_Nineth = objDrawLines["画线位置9"].ToString();
                                float fDrawLineNineth = float.Parse(strDrawLine_Nineth);

                                string strDrawLine_Tenth = objDrawLines["画线位置10"].ToString();
                                float fDrawLineTenth = float.Parse(strDrawLine_Tenth);

                                string strDrawLine_Eleventh = objDrawLines["画线位置11"].ToString();
                                float fDrawLineEleventh = float.Parse(strDrawLine_Eleventh);

                                string strDrawLine_Twelveth = objDrawLines["画线位置12"].ToString();
                                float fDrawLineTwelveth = float.Parse(strDrawLine_Twelveth);
                                DateTime curtime = DateTime.Now;
                                int nYear = curtime.Year;
                                string strTableNameNew = "stickresultinfo_" + curtime.Year.ToString() + curtime.Month.ToString() + curtime.Day.ToString();


                                CMySQLTool.Instance().InsertResultInfo(SettingParameter.Instance().StrMySQLDBName, strTableNameNew, strSiliconStickNum, fAppearanceLength, fAppearanceValidLength, fAppearanceMaxRadius, fAppearanceMinRadius, nSiliconLineNum, strInsertAbnormalArea_Fir, strInsertAbnormalArea_Sec, strInsertAbnormalArea_Third, strInsertAbnormalArea_Fourth, fDrawLineFir, fDrawLineSec, fDrawLineThr, fDrawLineFourth, fDrawLineFifth, fDrawLineSixth, fDrawLineSeventh, fDrawLineEighth, fDrawLineNineth, fDrawLineTenth, fDrawLineEleventh, fDrawLineTwelveth, "123");


                                


                                string strYingLiInfo = objectinfo["应力"].ToString();
                                {
                                    JObject objYingLi = JsonConvert.DeserializeObject<JObject>(strYingLiInfo);
                                    string strYingLiNumsInfo = objYingLi["应力曲线数组"].ToString();
                                    List<float> lstFYingLiNums = JsonConvert.DeserializeObject<List<float>>(strYingLiNumsInfo);

                                    string strInsertInfo = "";
                                    foreach(var info in lstFYingLiNums)
                                    {
                                        strInsertInfo += info.ToString("0.00000") + ",";
                                        data.YingliNumInfo.Add(info);
                                    }
                                    string strTableName = "yingli_" + curtime.Year.ToString() + curtime.Month.ToString() + curtime.Day.ToString();

                                    CMySQLTool.Instance().CreateIfNotExistYingLiTable(SettingParameter.Instance().StrMySQLDBName, strTableName);

                                    CMySQLTool.Instance().InsertYingLi(SettingParameter.Instance().StrMySQLDBName, strTableName, strSiliconStickNum, strInsertInfo);
                                }

                                string strRadiusInfo = objectinfo["直径"].ToString();
                                {
                                    JObject objRadius = JsonConvert.DeserializeObject<JObject>(strRadiusInfo);
                                    string strRadiusNumsInfo = objRadius["直径曲线数组"].ToString();
                                    List<float> lstFRadiusNumsInfo = JsonConvert.DeserializeObject<List<float>>(strRadiusNumsInfo);

                                    string strInsertInfo = "";
                                    foreach (var info in lstFRadiusNumsInfo)
                                    {
                                        strInsertInfo += info.ToString("0.00000") + ",";
                                        data.RadiusNumInfo.Add(info);
                                    }
                                    string strTableName = "radius_" + curtime.Year.ToString() + curtime.Month.ToString() + curtime.Day.ToString();

                                    CMySQLTool.Instance().CreateIfNotExistRadiusTable(SettingParameter.Instance().StrMySQLDBName, strTableName);

                                    CMySQLTool.Instance().InsertRadius(SettingParameter.Instance().StrMySQLDBName, strTableName, strSiliconStickNum, strInsertInfo);
                                }

                                string strPenetrationRate = objectinfo["透过率"].ToString();
                                {
                                    JObject objPenetrationRate = JsonConvert.DeserializeObject<JObject>(strPenetrationRate);
                                    string strPenetrationRateNumsInfo = objPenetrationRate["透过率曲线数组"].ToString();

                                    List<float> lstPenetrationRateNumsInfo = JsonConvert.DeserializeObject<List<float>>(strPenetrationRateNumsInfo);

                                    string strInsertInfo = "";
                                    foreach (var info in lstPenetrationRateNumsInfo)
                                    {
                                        strInsertInfo += info.ToString("0.00000") + ",";
                                        data.PenetrationRateNumInfo.Add(info);
                                    }
                                    string strTableName = "penetrationrate_" + curtime.Year.ToString() + curtime.Month.ToString() + curtime.Day.ToString();
                                    CMySQLTool.Instance().CreateIfNotExistPenetrationrateTable(SettingParameter.Instance().StrMySQLDBName, strTableName);

                                    CMySQLTool.Instance().Insertpenetrationrate(SettingParameter.Instance().StrMySQLDBName, strTableName, strSiliconStickNum, strInsertInfo);
                                }

                                string strResistivity = objectinfo["电阻率"].ToString();
                                {

                                    JObject objResistivity = JsonConvert.DeserializeObject<JObject>(strResistivity);

                                    string strResistivityNumsInfo = objResistivity["电阻率曲线数组"].ToString();

                                    List<float> lstResistivityNumsInfo = JsonConvert.DeserializeObject<List<float>>(strResistivityNumsInfo);

                                    string strInsertInfo = "";
                                    foreach (var info in lstResistivityNumsInfo)
                                    {
                                        strInsertInfo += info.ToString("0.00000") + ",";
                                        data.ResisvityNumInfo.Add(info);
                                    }
                                    string strTableName = "resistivity_" + curtime.Year.ToString() + curtime.Month.ToString() + curtime.Day.ToString();
                                    CMySQLTool.Instance().CreateIfNotExistResistivityTable(SettingParameter.Instance().StrMySQLDBName, strTableName);

                                    CMySQLTool.Instance().InsertResistivity(SettingParameter.Instance().StrMySQLDBName, strTableName, strSiliconStickNum, strInsertInfo);
                                }

                                GlobalDataCache.Instance().AddData(strSiliconStickNum, data);
                                

                            }
                        }
                        catch 
                        {

                        }

                        
                        
                       
                        Thread.Sleep(55);
                    });
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void RedisOperation_RedisSubImageEvent(int SUBConnalIndex, string subChannel, byte[] bytes)
        {
            
            try
            {
                if (SUBConnalIndex == (int)NSubChannelType)
                {
                    if (subChannel == StrSubScribeImageChannelName)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            //byte[] bt = bytes;
                            //byte[] data = new byte[bt.Length];
                            //Buffer.BlockCopy(bt, 0, data, 0, bt.Length);
                            //UpDataImageInfo(SUBConnalIndex, subChannel, data);
                            //GC.Collect();
                            //GC.WaitForPendingFinalizers();
                            //Application.DoEvents();
                            //Thread.Sleep(55);
                        });
                    }
                    
                }
            }
            catch (Exception ex)
            {
                FlashLogger.Error("CameraLog", ",UCMVSScreen的RedisOperation_RedisSubImageEvent异常：", ex);
                //  LogHelper.WriteLogAndInfo("MVSLog", "UCMVSScreen的RedisOperation_RedisSubImageEvent异常：", ex.Message);
            }
        }

        public void RedisPub(string strChannelName, string strMesage)
        {
            redisOperation.PublishSub(strChannelName, strMesage);

        }
        public void SubScribe(string strRedisIP, int nRedisPort, string[] strSubscribeChannelName, int nRedisDBIndex)
        {
            try
            {
                 LogHelper.Info("Silicon","SubScirbe Begin " + strRedisIP + " " + nRedisPort.ToString() + " " + nRedisDBIndex.ToString());
                redisOperation = new SERedisOperation();
                //订阅Redis
                redisOperation.RedisSubImageEvent -= RedisOperation_RedisSubImageEvent;
                redisOperation.RedisSubImageEvent += RedisOperation_RedisSubImageEvent;
                redisOperation.RedisSubMessageEvent -= RedisOperation_RedisSubMessageEvent;
                redisOperation.RedisSubMessageEvent += RedisOperation_RedisSubMessageEvent;

                StrSubScribeMessageChannelName = strSubscribeChannelName;
                 LogHelper.Info("Silicon","SubScirbe Begin 1 _");


                bool result = redisOperation.OpenRedisSUBSCRIBE((int)_nSubChannelType, nRedisDBIndex, strSubscribeChannelName, strRedisIP, (ushort)nRedisPort);
                if (result)
                {
                    UIMessageBox.Show("Redis 订阅成功！", false, true);
                    // LogHelper.Info("Silicon","订阅成功，频道：" + (int)_nSubChannelType + ",地址：" + strRedisIP + ":" + nRedisPort.ToString());
                    return;
                }
                else
                {
                    UIMessageBox.Show("Redis 订阅失败，请检查Redis服务器！", false,true);
                     LogHelper.Info("Silicon","订阅失败，频道：" + (int)_nSubChannelType + ",地址：" + strRedisIP + ":" + nRedisPort.ToString());
                }
            }
            catch(Exception ex)
            {
                 LogHelper.Info("Silicon","Redis异常 " +  ex.Message);
            }
           
        }
    }
}
