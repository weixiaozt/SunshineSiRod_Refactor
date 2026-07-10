using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using OpenCvSharp;
using SiliconRoundBarCheck.Parameters;
using Sunny.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouEyEE.Untils.Log;

namespace SiliconRoundBarCheck.Tools
{
    internal class CMySQLTool
    {
        private static CMySQLTool _instance = null;

        private MySqlConnection _connection;
        public static CMySQLTool Instance()
        {
            if (_instance == null)
            {
                _instance = new CMySQLTool();
            }
            return _instance;
        }


        private CMySQLTool() { }


        public List<InspectResult> SearchResult(DateTime start, DateTime end)
        {
            string sql = "SELECT * FROM stickresult where checktime >= \"" + start.ToString() + "\" and checktime <= \"" + end.ToString() + "\";";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);

            MySqlDataReader mySqlDataReader = cmd.ExecuteReader();
            List<InspectResult> _list = new List<InspectResult>();

            try
            {
                while (mySqlDataReader.Read())
                {

                    InspectResult result = new InspectResult();

                    result.NID = mySqlDataReader.GetInt32((int)InspectResult.emDataType.EM_ID);
                    result.NResult = mySqlDataReader.GetInt32((int)InspectResult.emDataType.EM_RESULT);
                    result.StrFileLJ_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILELJ_1);
                    result.StrFileLJ_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILELJ_2);
                    result.StrFileLJ_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILELJ_3);
                    result.StrFileLJ_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILELJ_4);
                    result.StrFileYinLie_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINLIE_1);
                    result.StrFileYinLie_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINLIE_2);
                    result.StrFileYinLie_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINLIE_3);
                    result.StrFileYinLie_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINLIE_4);
                    result.StrFileYingLi_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINGLI_1);
                    result.StrFileYingLi_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINGLI_2);
                    result.StrFileYingLi_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINGLI_3);
                    result.StrFileYingLi_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINGLI_4);
                    result.StrNGSubLJLength_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBLJLENGTH_1);
                    result.StrNGSubLJLength_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBLJLENGTH_2);
                    result.StrNGSubLJLength_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBLJLENGTH_3);
                    result.StrNGSubLJLength_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBLJLENGTH_4);
                    result.StrNGSubYinLieLength_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINLIELENGTH_1);
                    result.StrNGSubYinLieLength_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINLIELENGTH_2);
                    result.StrNGSubYinLieLength_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINLIELENGTH_3);
                    result.StrNGSubYinLieLength_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINLIELENGTH_4);
                    result.StrNGSubYingLiLength_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINGLILENGTH_1);
                    result.StrNGSubYingLiLength_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINGLILENGTH_2);
                    result.StrNGSubYingLiLength_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINGLILENGTH_3);
                    result.StrNGSubYingLiLength_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINGLILENGTH_4);
                    result.StrNGSubRadiusLength = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBRADIUSLENGTH);
                    result.StrNGSubTotalLength = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBTOTALLENGTH);
                    result.Curcheck = mySqlDataReader.GetDateTime((int)InspectResult.emDataType.EM_CHECKTIME);


                    _list.Add(result);
                }


            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon", "Mysql Select Exception " + ex.Message);
            }

            mySqlDataReader.Close();
            return _list;
        }

        public List<InspectResultLineInfo> SearchAllResultLinesByCur(DateTime cur)
        {
            string sql = "SELECT * FROM stickresultinfo_" + cur.Year.ToString() + cur.Month.ToString() + cur.Day.ToString();
            MySqlCommand cmd = new MySqlCommand(sql, _connection);
            List<InspectResultLineInfo> _list = new List<InspectResultLineInfo>();
            MySqlDataReader mySqlDataReader = null;
            try
            {
                mySqlDataReader = cmd.ExecuteReader();
            
            
                while (mySqlDataReader.Read())
                {

                    InspectResultLineInfo result = new InspectResultLineInfo();

                    result.StrSiliconStickNum = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_SILICONSTICKNUM);
                    result.FApperanceLength = mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_APPEARANCELENGTH);
                    result.FAppearanceValidLength = mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_APPEARANCELENGTH);
                    result.NSiliconLineNum = mySqlDataReader.GetInt32((int)InspectResultLineInfo.emDataType.EM_SILICONLINENUM);
                    result.StrAbnormalFir = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_ABNORMALAREAFIR);
                    result.StrAbnormalSec = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_ABNORMALAREASEC);
                    result.StrAbnormalThr = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_ABNORMALAREATHR);
                    result.StrAbnormalFour = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_ABNORMALAREAFOUR);
                    string strDrawLineInfo = mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_1).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_2).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_3).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_4).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_5).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_6).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_7).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_8).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_9).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_10).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_11).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_12).ToString("0.00");
                     
                    result.StrDrawLineInfo = strDrawLineInfo;
                    result.StrResultPath = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_RESULTPATH);
                    result.CurDate = cur;

                    _list.Add(result);
                }

                mySqlDataReader.Close();
                mySqlDataReader = null;
            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon", "Mysql Select Exception " + ex.Message);
            }

            if (mySqlDataReader != null)
            {
                mySqlDataReader.Close();
            }
           
            return _list;
        }

        public List<InspectResultLineInfo> SearchAllResultLines()
        {
            string sql = "SELECT * FROM stickresultinfo ";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);

            MySqlDataReader mySqlDataReader = cmd.ExecuteReader();
            List<InspectResultLineInfo> _list = new List<InspectResultLineInfo>();

            try
            {
                while (mySqlDataReader.Read())
                {

                    InspectResultLineInfo result = new InspectResultLineInfo();

                    result.StrSiliconStickNum = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_SILICONSTICKNUM);
                    result.FApperanceLength = mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_APPEARANCELENGTH);
                    result.FAppearanceValidLength = mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_APPEARANCELENGTH);
                    result.NSiliconLineNum = mySqlDataReader.GetInt32((int)InspectResultLineInfo.emDataType.EM_SILICONLINENUM);
                    result.StrAbnormalFir = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_ABNORMALAREAFIR);
                    result.StrAbnormalSec = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_ABNORMALAREASEC);
                    result.StrAbnormalThr = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_ABNORMALAREATHR);
                    result.StrAbnormalFour = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_ABNORMALAREAFOUR);
                    string strDrawLineInfo = mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_1).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_2).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_3).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_4).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_5).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_6).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_7).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_8).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_9).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_10).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_11).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_12).ToString("0.00");

                    result.StrDrawLineInfo = strDrawLineInfo;


                    _list.Add(result);
                }


            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon", "Mysql Select Exception " + ex.Message);
            }

            mySqlDataReader.Close();
            return _list;
        }

        public bool IsTableExisted(string strDBName, string strTableName)
        {
            try
            {
                string strsqlInfo = @"SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = '" + strDBName + "' and  table_name ='"+ strTableName + "';";
                MySqlCommand cmd = new MySqlCommand(strsqlInfo, _connection);
                MySqlDataReader mySqlDataReader = cmd.ExecuteReader();

                while (mySqlDataReader.Read())
                {
                    int nCount = mySqlDataReader.GetInt32(0);
                    if (nCount > 0)
                    {
                        mySqlDataReader.Close();
                        return true;
                    }
                    else 
                    {
                        mySqlDataReader.Close();
                        return false; 
                    }

                }
            }
            catch (Exception ex)
            {
               
                return false;
            }
            

            return false;
        }

        public List<InspectResultLineInfo> SearchResultByDate(DateTime beginDate, DateTime endDate)
        {
            TimeSpan timeoneday = new TimeSpan(24, 0, 0);
            DateTime timecur = beginDate;
            List<InspectResultLineInfo> _listFull = new List<InspectResultLineInfo>();
            string strTableName = "stickresultinfo_";


            try
            {
                do
                {
                    bool bTableIsExisted = false;
                    string strCurTable = strTableName + timecur.Year.ToString()+ timecur.Month.ToString() + timecur.Day.ToString();
                    bTableIsExisted = IsTableExisted(SettingParameter.Instance().StrMySQLDBName ,strCurTable);

                    if (false == bTableIsExisted)
                    {
                        timecur = timecur + timeoneday;

                        if (timecur > endDate)
                        {
                            break;
                        }
                        continue;
                    }

                    List<InspectResultLineInfo> _listCur = SearchResultLinesByTableName(strCurTable);

                    for(int i = 0; i < _listCur.Count; i++)
                    {
                        _listCur[i].CurDate = timecur;
                        _listFull.Add(_listCur[i]);
                    }
                    _listCur.Clear();

                    timecur = timecur + timeoneday;

                    if (timecur > endDate)
                    {
                        break;
                    }
                } while (true);

                return _listFull;
            }
            catch (Exception ex)
            {

            }

            return _listFull;

        }

        public List<InspectResultLineInfo> SearchResultLinesByTableName(string strTableName)
        {
            string sql = "SELECT * FROM " + strTableName + ";";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);

            MySqlDataReader mySqlDataReader = cmd.ExecuteReader();
            List<InspectResultLineInfo> _list = new List<InspectResultLineInfo>();

            try
            {
                while (mySqlDataReader.Read())
                {

                    InspectResultLineInfo result = new InspectResultLineInfo();

                    result.StrSiliconStickNum = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_SILICONSTICKNUM);
                    result.FApperanceLength = mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_APPEARANCELENGTH);
                    result.FAppearanceValidLength = mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_APPEARANCELENGTH);
                    result.FAppearanceMaxRadius = mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_APPEARANCEMAXRADIUS);
                    result.FAppearanceMinRadius = mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_APPEARANCEMINRADIUS);
                    result.NSiliconLineNum = mySqlDataReader.GetInt32((int)InspectResultLineInfo.emDataType.EM_SILICONLINENUM);
                    result.StrAbnormalFir = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_ABNORMALAREAFIR);
                    result.StrAbnormalSec = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_ABNORMALAREASEC);
                    result.StrAbnormalThr = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_ABNORMALAREATHR);
                    result.StrAbnormalFour = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_ABNORMALAREAFOUR);
                    string strDrawLineInfo = mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_1).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_2).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_3).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_4).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_5).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_6).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_7).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_8).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_9).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_10).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_11).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_12).ToString("0.00");

                    result.StrDrawLineInfo = strDrawLineInfo;


                    _list.Add(result);
                }


            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon", "Mysql Select Exception " + ex.Message);
            }

            mySqlDataReader.Close();
            return _list;
        }

        public List<InspectResultLineInfo> SearchResultLines(string strDBName, string strTableName, string strSiliconStickNum)
        {
            string sql = "SELECT * FROM `" + strDBName + "`.`" + strTableName + "` where SiliconStickNum = \"" + strSiliconStickNum + "\";";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);

            MySqlDataReader mySqlDataReader = cmd.ExecuteReader();
            List<InspectResultLineInfo> _list = new List<InspectResultLineInfo>();

            try
            {
                while (mySqlDataReader.Read())
                {

                    InspectResultLineInfo result = new InspectResultLineInfo();

                    result.StrSiliconStickNum = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_SILICONSTICKNUM);
                    result.FApperanceLength = mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_APPEARANCELENGTH);
                    result.FAppearanceValidLength = mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_APPEARANCELENGTH);    
                    result.NSiliconLineNum = mySqlDataReader.GetInt32((int)InspectResultLineInfo.emDataType.EM_SILICONLINENUM);
                    result.StrAbnormalFir = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_ABNORMALAREAFIR);
                    result.StrAbnormalSec = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_ABNORMALAREASEC);
                    result.StrAbnormalThr = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_ABNORMALAREATHR);
                    result.StrAbnormalFour = mySqlDataReader.GetString((int)InspectResultLineInfo.emDataType.EM_ABNORMALAREAFOUR);
                    string strDrawLineInfo = mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_1).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_2).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_3).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_4).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_5).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_6).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_7).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_8).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_9).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_10).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_11).ToString("0.00") + "," + mySqlDataReader.GetFloat((int)InspectResultLineInfo.emDataType.EM_DRAWLINE_12).ToString("0.00");

                    result.StrDrawLineInfo = strDrawLineInfo;


                    _list.Add(result);
                }


            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon", "Mysql Select Exception " + ex.Message);
            }

            mySqlDataReader.Close();
            return _list;
        }

        public ArrayList SearchPenetrationrateInfoByStickLineNum(string strDBName, string strTableName, string strSiliconStickNum)
        {
            string sql = "SELECT * FROM `" + strDBName + "`.`" + strTableName + "` where SiliconStickNum = \"" + strSiliconStickNum + "\";";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);
            MySqlDataReader mySqlDataReader = cmd.ExecuteReader();
            ArrayList infoArray = new ArrayList();
            try
            {
                while (mySqlDataReader.Read())
                {

                    string strPenetrationrate = mySqlDataReader.GetString(1);
                    string[] strPenetrationrates = strPenetrationrate.Split(',');
                    float fValue = 0;
                    for (int i = 0; i < strPenetrationrates.Length; i++)
                    {
                        if (strPenetrationrates[i].Length > 0)
                        {
                            if (true == float.TryParse(strPenetrationrates[i], out fValue))
                            {
                                infoArray.Add(float.Parse(strPenetrationrates[i]));
                            }
                        }
                    }
                    mySqlDataReader.Close();
                    return infoArray;
                }

            }
            catch (Exception ex)
            {
                mySqlDataReader.Close();
            }

            return infoArray;
        }


        public ArrayList SearchResisvityInfoByStickLineNum(string strDBName, string strTableName, string strSiliconStickNum)
        {
            string sql = "SELECT * FROM `" + strDBName + "`.`" + strTableName + "` where SiliconStickNum = \"" + strSiliconStickNum + "\";";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);
            MySqlDataReader mySqlDataReader = cmd.ExecuteReader();
            ArrayList infoArray = new ArrayList();
            try
            {
                while (mySqlDataReader.Read())
                {

                    string strResisvityInfo = mySqlDataReader.GetString(1);
                    string[] strResisvity = strResisvityInfo.Split(',');

                    float fValue = 0;
                    for (int i = 0; i < strResisvity.Length; i++)
                    {
                        if (strResisvity[i].Length > 0)
                        {
                            if (true == float.TryParse(strResisvity[i], out fValue))
                            {
                                infoArray.Add(float.Parse(strResisvity[i]));
                            }
                        }
                    }
                    mySqlDataReader.Close();

                    return infoArray;
                }

            }
            catch (Exception ex)
            {
                mySqlDataReader.Close();
            }

            return infoArray;
        }

        public ArrayList SearchRadiusInfoByStickLineNum(string strDBName, string strTableName, string strSiliconStickNum)
        {
            string sql = "SELECT * FROM `" + strDBName + "`.`" + strTableName + "` where SiliconStickNum = \"" + strSiliconStickNum + "\";";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);
            MySqlDataReader mySqlDataReader = cmd.ExecuteReader();
            ArrayList infoArray = new ArrayList();
            try
            {
                while (mySqlDataReader.Read())
                {

                    string strRadiusInfo = mySqlDataReader.GetString(1);
                    string[] strRadiuses = strRadiusInfo.Split(',');

                    float fValue = 0;
                    for (int i = 0; i < strRadiuses.Length; i++)
                    {
                        if (true == float.TryParse(strRadiuses[i], out fValue))
                        {
                            infoArray.Add(float.Parse(strRadiuses[i]));
                        }
                    }
                    mySqlDataReader.Close();

                    return infoArray;
                }

            }
            catch (Exception ex)
            {
                mySqlDataReader.Close();
            }

            return infoArray;
        }

        public ArrayList SearchYingLiInfoByStickLineNum(string strDBName, string strTableName, string strSiliconStickNum)
        {
            string sql = "SELECT * FROM `" + strDBName + "`.`" + strTableName + "` where SiliconStickNum = \"" + strSiliconStickNum + "\";";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);

            MySqlDataReader mySqlDataReader = cmd.ExecuteReader();
            ArrayList infoArray = new ArrayList();
            try
            {
                while (mySqlDataReader.Read())
                {

                    string strYingLiInfo = mySqlDataReader.GetString(1);
                    string[] strYingLis = strYingLiInfo.Split(',');
                    float fValue = 0;

                    for (int i = 0; i < strYingLis.Length; i++)
                    {
                        if (true == float.TryParse(strYingLis[i], out fValue))
                        {
                            infoArray.Add(float.Parse(strYingLis[i]));
                        }
                    }
                    mySqlDataReader.Close();

                    return infoArray;
                }

            }
            catch (Exception ex)
            {
                mySqlDataReader.Close();
            }

            return infoArray;
        }

        public List<InspectResult> SearchResult(int nResult = 0)
        {

            List<InspectResult> _list = new List<InspectResult>();

            return _list;
            string sql = "SELECT * FROM stickresult where result = " + nResult.ToString() + ";";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);

            MySqlDataReader mySqlDataReader = cmd.ExecuteReader();


            try
            {
                while (mySqlDataReader.Read())
                {

                    InspectResult result = new InspectResult();

                    result.NID = mySqlDataReader.GetInt32((int)InspectResult.emDataType.EM_ID);
                    result.NResult = mySqlDataReader.GetInt32((int)InspectResult.emDataType.EM_RESULT);
                    result.StrFileLJ_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILELJ_1);
                    result.StrFileLJ_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILELJ_2);
                    result.StrFileLJ_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILELJ_3);
                    result.StrFileLJ_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILELJ_4);
                    result.StrFileYinLie_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINLIE_1);
                    result.StrFileYinLie_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINLIE_2);
                    result.StrFileYinLie_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINLIE_3);
                    result.StrFileYinLie_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINLIE_4);
                    result.StrFileYingLi_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINGLI_1);
                    result.StrFileYingLi_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINGLI_2);
                    result.StrFileYingLi_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINGLI_3);
                    result.StrFileYingLi_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINGLI_4);
                    result.StrNGSubLJLength_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBLJLENGTH_1);
                    result.StrNGSubLJLength_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBLJLENGTH_2);
                    result.StrNGSubLJLength_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBLJLENGTH_3);
                    result.StrNGSubLJLength_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBLJLENGTH_4);
                    result.StrNGSubYinLieLength_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINLIELENGTH_1);
                    result.StrNGSubYinLieLength_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINLIELENGTH_2);
                    result.StrNGSubYinLieLength_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINLIELENGTH_3);
                    result.StrNGSubYinLieLength_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINLIELENGTH_4);
                    result.StrNGSubYingLiLength_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINGLILENGTH_1);
                    result.StrNGSubYingLiLength_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINGLILENGTH_2);
                    result.StrNGSubYingLiLength_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINGLILENGTH_3);
                    result.StrNGSubYingLiLength_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINGLILENGTH_4);
                    result.StrNGSubRadiusLength = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBRADIUSLENGTH);
                    result.StrNGSubTotalLength = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBTOTALLENGTH);
                    result.Curcheck = mySqlDataReader.GetDateTime((int)InspectResult.emDataType.EM_CHECKTIME);


                    _list.Add(result);
                }


            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon", "Mysql Select Exception " + ex.Message);
            }

            mySqlDataReader.Close();
            return _list;
        }

        public List<InspectResult> SelectResult()
        {
            string sql = "SELECT * FROM siliconstick.stickresult;";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);

            MySqlDataReader mySqlDataReader = cmd.ExecuteReader();
            List<InspectResult> _list = new List<InspectResult>();

            try
            {
                while (mySqlDataReader.Read())
                {

                    InspectResult result = new InspectResult();

                    result.NID = mySqlDataReader.GetInt32((int)InspectResult.emDataType.EM_ID);
                    result.NResult = mySqlDataReader.GetInt32((int)InspectResult.emDataType.EM_RESULT);
                    result.StrFileLJ_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILELJ_1);
                    result.StrFileLJ_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILELJ_2);
                    result.StrFileLJ_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILELJ_3);
                    result.StrFileLJ_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILELJ_4);
                    result.StrFileYinLie_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINLIE_1);
                    result.StrFileYinLie_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINLIE_2);
                    result.StrFileYinLie_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINLIE_3);
                    result.StrFileYinLie_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINLIE_4);
                    result.StrFileYingLi_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINGLI_1);
                    result.StrFileYingLi_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINGLI_2);
                    result.StrFileYingLi_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINGLI_3);
                    result.StrFileYingLi_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINGLI_4);
                    result.StrNGSubLJLength_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBLJLENGTH_1);
                    result.StrNGSubLJLength_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBLJLENGTH_2);
                    result.StrNGSubLJLength_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBLJLENGTH_3);
                    result.StrNGSubLJLength_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBLJLENGTH_4);
                    result.StrNGSubYinLieLength_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINLIELENGTH_1);
                    result.StrNGSubYinLieLength_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINLIELENGTH_2);
                    result.StrNGSubYinLieLength_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINLIELENGTH_3);
                    result.StrNGSubYinLieLength_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINLIELENGTH_4);
                    result.StrNGSubYingLiLength_1 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINGLILENGTH_1);
                    result.StrNGSubYingLiLength_2 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINGLILENGTH_2);
                    result.StrNGSubYingLiLength_3 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINGLILENGTH_3);
                    result.StrNGSubYingLiLength_4 = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBYINGLILENGTH_4);
                    result.StrNGSubRadiusLength = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBRADIUSLENGTH);
                    result.StrNGSubTotalLength = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_NGSUBTOTALLENGTH);
                    result.Curcheck = mySqlDataReader.GetDateTime((int)InspectResult.emDataType.EM_CHECKTIME);


                    _list.Add(result);
                }


            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon", "Mysql Select Exception " + ex.Message);
            }

            mySqlDataReader.Close();
            return _list;

        }

        public void Insertpenetrationrate(string strDBName, string strTableName, string strSiliconStickNum, string strPenetrationRate)
        {
            try
            {
                //透过率
                string sql = "INSERT INTO `" + strDBName + "`.`" + strTableName + "`(`SiliconStickNum`,`PenetrationRate`) VALUES ( \"" + strSiliconStickNum + "\", \"" + strPenetrationRate + "\");";
                //创建命令对象
                MySqlCommand cmd = new MySqlCommand(sql, _connection);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
        }

        public void InsertResistivity(string strDBName, string strTableName, string strSiliconStickNum, string strResistivityNumInfo)
        {
            try
            {
                string sql = "INSERT INTO `" + strDBName + "`.`" + strTableName + "`(`SiliconStickNum`,`Resistivity`) VALUES (\"" + strSiliconStickNum + "\", \"" + strResistivityNumInfo + "\");";
                //创建命令对象
                MySqlCommand cmd = new MySqlCommand(sql, _connection);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
        }

        public void InsertRadius(string strDBName, string strTableName, string strSiliconStickNum, string strRadiusNumArr)
        {
            try
            {
                string sql = "INSERT INTO `" + strDBName + "`.`" + strTableName + "`(`SiliconStickNum`,`RadiusNumArr`) VALUES (\"" + strSiliconStickNum + "\", \"" + strRadiusNumArr + " \");";
                //创建命令对象
                MySqlCommand cmd = new MySqlCommand(sql, _connection);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
        }

        public void InsertYingLi(string strDBName, string strTableName, string strSiliconStickNum, string strYingLiNumArr)
        {

            try
            {

                string sql = "INSERT INTO `" + strDBName + "`.`" + strTableName + "`(`SiliconStickNum`,`YingLiNumArr`) VALUES ( \"" + strSiliconStickNum +
                "\" , \"" + strYingLiNumArr + " \");";

                //创建命令对象
                MySqlCommand cmd = new MySqlCommand(sql, _connection);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
        }
        public void CreateIfNotExistPenetrationrateTable(string strDBName, string strTableName)
        {
            try
            {
                
                string strSqlInfo = @"CREATE TABLE IF NOT EXISTS `" + strDBName + "`.`" + strTableName + @"` (
                  `SiliconStickNum` varchar(255) NOT NULL,
                  `PenetrationRate` longtext,
                  PRIMARY KEY (`SiliconStickNum`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;";
                MySqlCommand cmd = new MySqlCommand(strSqlInfo, _connection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                LogHelper.Info("RedisLog", "Mysql CreateIfNotExistYingLiTable exception " + ex.Message.ToString());
            }
        }

        public void CreateIfNotExistResistivityTable(string strDBName, string strTableName)
        {
            try
            {
                string strSqlInfo = @"CREATE TABLE IF NOT EXISTS `" + strDBName + "`.`" + strTableName + @"` (
                  `SiliconStickNum` varchar(255) NOT NULL,
                  `Resistivity` longtext,
                  PRIMARY KEY (`SiliconStickNum`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;";
                MySqlCommand cmd = new MySqlCommand(strSqlInfo, _connection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                LogHelper.Info("RedisLog", "Mysql CreateIfNotExistYingLiTable exception " + ex.Message.ToString());
            }
        }

        public void CreateIfNotExistRadiusTable(string strDBName, string strTableName)
        {
            try
            {
                string strSqlInfo = @"CREATE TABLE IF NOT EXISTS `" + strDBName + "`.`" + strTableName + @"` (
              `SiliconStickNum` varchar(255) NOT NULL,
              `RadiusNumArr` longtext,
              PRIMARY KEY (`SiliconStickNum`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;";
                MySqlCommand cmd = new MySqlCommand(strSqlInfo, _connection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                LogHelper.Info("RedisLog", "Mysql CreateIfNotExistYingLiTable exception " + ex.Message.ToString());
            }
        }

        public void CreateIfNotExistYingLiTable(string strDBName, string strTableName)
        {
            try
            {
                string strSqlInfo = @"CREATE TABLE IF NOT EXISTS `" + strDBName + "`.`" + strTableName + @"` (
              `SiliconStickNum` VARCHAR(255) NOT NULL,
              `YingLiNumArr` longtext,
              PRIMARY KEY (`SiliconStickNum`)
            ) ENGINE=INNODB DEFAULT CHARSET=utf8mb3;";
                //创建命令对象
                MySqlCommand cmd = new MySqlCommand(strSqlInfo, _connection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                LogHelper.Info("RedisLog", "Mysql CreateIfNotExistYingLiTable exception " + ex.Message.ToString());
            }

        }
        public void CreateIfNotExistResultTable(string strDBName,  string strTableName)
        {
            try
            {
               
                string strSqlInfo = @"CREATE TABLE IF NOT EXISTS `" + strDBName + @"`.
             `" + strTableName + @"` (
              `SiliconStickNum` varchar(255) NOT NULL,
              `AppearanceLength` float DEFAULT NULL,
              `AppearanceValidLength` float DEFAULT NULL,
              `AppearanceMaxRadius` float DEFAULT NULL,
              `AppearanceMinRadius` float DEFAULT NULL,
              `SiliconLineNum` int DEFAULT NULL,
              `AbnormalAreaFir` varchar(255) DEFAULT NULL,
              `AbnormalAreaSec` varchar(255) DEFAULT NULL,
              `AbnormalAreaThr` varchar(255) DEFAULT NULL,
              `AbnormalAreaFourth` varchar(255) DEFAULT NULL,
              `DrawLinePos1` float DEFAULT NULL,
              `DrawLinePos2` float DEFAULT NULL,
              `DrawLinePos3` float DEFAULT NULL,
              `DrawLinePos4` float DEFAULT NULL,
              `DrawLinePos5` float DEFAULT NULL,
              `DrawLinePos6` float DEFAULT NULL,
              `DrawLinePos7` float DEFAULT NULL,
              `DrawLinePos8` float DEFAULT NULL,
              `DrawLinePos9` float DEFAULT NULL,
              `DrawLinePos10` float DEFAULT NULL,
              `DrawLinePos11` float DEFAULT NULL,
              `DrawLinePos12` float DEFAULT NULL,
              `ResultPath` varchar(255) NOT NULL,
              `WCLegnth` float DEFAULT NULL,  
              PRIMARY KEY(`SiliconStickNum`)
            ) ENGINE = InnoDB DEFAULT CHARSET = utf8mb3;";

                //创建命令对象
                MySqlCommand cmd = new MySqlCommand(strSqlInfo, _connection);
                cmd.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                LogHelper.Info("RedisLog", "Mysql CreateIfNotExistTable exception " + ex.Message.ToString());
            }
           
        }

        public void InsertOutResultInfo(string strID, string strWaferID, string strResult, string strBatch, int nJingXianResult, int nJingXianNum, int nJingXianMaxArea, int nJingXianFullArea, int nJingXianMaxLength, double dbJingXianX, double dbJingXianY, double dbJingXianRadius, int nWCResult, int nWCNum, int nWCMaxArea, int nWCFullArea, int nWCMaxLength, double dbWCX, double dbWCY, double dbWCRadius, int nYinLieResult, int nYinLieNum, int nYinLieMaxArea, int nYinLieFullArea, int nYinLieMaxLength, double dbYinLieX, double dbYinLieY, double dbYinLieRadius, int nFlag)
        {
            try
            {
                string strSqlInfo = @"INSERT INTO `a_xmartsql`.`db_20230801a白班`
                (`ID`,
                `WaferID`,
                `结果`,
                `批次号`,
                `晶线质量`,
                `晶线个数`,
                `晶线最大面积`,
                `晶线总面积`,
                `晶线最大长度`,
                `晶线纵坐标`,
                `晶线横坐标`,
                `晶线半径`,
                `位错质量`,
                `位错个数`,
                `位错最大面积`,
                `位错总面积`,
                `位错最大长度`,
                `位错纵坐标`,
                `位错横坐标`,
                `位错半径`,
                `隐裂质量`,
                `隐裂个数`,
                `隐裂最大面积`,
                `隐裂总面积`,
                `隐裂最大长度`,
                `隐裂纵坐标`,
                `隐裂横坐标`,
                `隐裂半径`,
                `标志字段`)
                VALUES
                (" + strID + ",\"" + strWaferID + "\"," + strResult + "\"," + nJingXianResult.ToString() + "," + nJingXianNum.ToString() + "," + 
                 nJingXianMaxArea.ToString() + "," + nJingXianFullArea.ToString() + "," + nJingXianMaxLength.ToString() + "," + dbJingXianY.ToString("0.00") + "," + dbJingXianX.ToString("0.00") + "," +dbJingXianRadius.ToString("0.00") + "," + nWCResult.ToString() + "," + 
                 nWCNum.ToString() + "," + nWCMaxArea.ToString() + "," + nWCFullArea.ToString() + "," + nWCMaxLength.ToString() + "," + dbWCY.ToString("0.00") + "," + dbWCX.ToString("0.00") + "," + dbWCRadius.ToString("0.00") + "," + nYinLieResult.ToString() + "," +
                 nYinLieNum.ToString() + "," + nYinLieMaxArea.ToString() + "," + nYinLieFullArea.ToString() + "," + nYinLieMaxLength.ToString() + "," + dbYinLieY.ToString("0.00") + "," + dbYinLieX.ToString("0.00") + ","  + dbYinLieRadius.ToString("0.00") + "," + nFlag.ToString() + ");";

                //创建命令对象
                MySqlCommand cmd = new MySqlCommand(strSqlInfo, _connection);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                 LogHelper.Info("RedisLog", "Mysql Connect exception " + ex.Message.ToString());
            }
        }
        public void InsertResultInfo(string strDBName, string strTableName, string strSiliconStickNum, float fAppearanceLength, float fAppearanceValidLength, float fAppearanceMaxRadius, float fAppearanceMinRadius, int nSiliconLineNum, string strAbnormalAreaFir, string strAbnormalAreaSec, string strAbnormalAreaThr, string strAbnormalAreaFourth, float fDrawLinePos1, float fDrawLinePos2, float fDrawLinePos3, float fDrawLinePos4, float fDrawLinePos5, float fDrawLinePos6, float fDrawLinePos7, float fDrawLinePos8, float fDrawLinePos9, float fDrawLinePos10, float fDrawLinePos11, float fDrawLinePos12, string strResultPath, float fWCLength = 0)
        {
            try
            { 
                string sql = "INSERT INTO `" + strDBName + "`.`" + strTableName + "`(`SiliconStickNum`,`AppearanceLength`,`AppearanceValidLength`,`AppearanceMaxRadius`,`AppearanceMinRadius`,`SiliconLineNum`,`AbnormalAreaFir`,`AbnormalAreaSec`,`AbnormalAreaThr`,`AbnormalAreaFourth`,`DrawLinePos1`,`DrawLinePos2`,`DrawLinePos3`,`DrawLinePos4`,`DrawLinePos5`,`DrawLinePos6`,`DrawLinePos7`,`DrawLinePos8`,`DrawLinePos9`,`DrawLinePos10`,`DrawLinePos11`,`DrawLinePos12`,`ResultPath`,`WCLegnth`) VALUES ( \"" + strSiliconStickNum +  "\"" + "," + fAppearanceLength.ToString("0.00000") + "," + fAppearanceValidLength.ToString("0.00000") +", " + fAppearanceMaxRadius.ToString("0.00000") +"," + fAppearanceMinRadius.ToString("0.00000") + ", " + nSiliconLineNum.ToString() + " , \"" + strAbnormalAreaFir  + "\", \"" +  strAbnormalAreaSec + "\", \"" + strAbnormalAreaThr   + "\", \"" + strAbnormalAreaFourth  + "\", " + fDrawLinePos1.ToString("0.00000") + " , " + fDrawLinePos2.ToString("0.00000") + ", " + fDrawLinePos3.ToString("0.00000") + ", " + fDrawLinePos4.ToString("0.00000") + ", " + fDrawLinePos5.ToString("0.00000") + "," + fDrawLinePos6.ToString("0.00000") + ", " + fDrawLinePos7.ToString("0.00000") + "," + fDrawLinePos8.ToString("0.00000") + "," + fDrawLinePos9.ToString("0.00000")  + ", " + fDrawLinePos10.ToString("0.00000") + ", " + fDrawLinePos11.ToString("0.000000")  + " ," + fDrawLinePos12.ToString("0.00000") + ", "+ strResultPath + ", " + fWCLength.ToString("0.0") +");";
                //创建命令对象
                MySqlCommand cmd = new MySqlCommand(sql, _connection);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                 LogHelper.Info("RedisLog", "Mysql Connect exception " + ex.Message.ToString());
            }
        }

        public void InsertResult(int nResult, string strLJ_1, string strLJ_2, string strLJ_3, string strLJ_4, string strYinLie_1, string strYinLie_2, string strYinLie_3, string strYinLie_4, string strYINGLI_1, string strYINGLI_2, string strYINGLI_3, string strYINGLI_4, string strNGSubLJLength_1, string strNGSubLJLength_2, string strNGSubLJLength_3, string strNGSubLJLength_4, string strNGSubYinLieLength_1, string strNGSubYinLieLength_2, string strNGSubYinLieLength_3, string strNGSubYinLieLength_4, string strNGSubYingLiLength_1, string strNGSubYingLiLength_2, string strNGSubYingLiLength_3, string strNGSubYingLiLength_4, string strNGRadiusLength,string strNGSubTotalLength)
        {
            try
            {
                //写入sql语句
                string sql = "INSERT INTO `stickresult`(`result`,`LJFile_1`,`LJFile_2`,`LJFile_3`,`LJFile_4`,`YinLieFile_1`,`YinLieFile_2`,`YinLieFile_3`,`YinLieFile_4`,`YingLiFile_1`,`YingLiFile_2`,`YingLiFile_3`,`YingLiFile_4`,`NGSubLJLength_1`,`NGSubLJLength_2`,`NGSubLJLength_3`,`NGSubLJLength_4`,`NGSubYinLieLength_1`,`NGSubYinLieLength_2`,`NGSubYinLieLength_3`,`NGSubYinLieLength_4`,`NGSubYingLiLength_1`,`NGSubYingLiLength_2`,`NGSubYingLiLength_3`,`NGSubYingLiLength_4`,`NGSubRadiusLength`,`NGSubTotalLength`,`checktime`) VALUES( " + nResult.ToString() + " , \"" + strLJ_1 + "\" , \"" + strLJ_2 + "\" , \"" + strLJ_3 + "\" , \"" + strLJ_4 + "\", \"" + strYinLie_1 + "\" , \"" + strYinLie_2 + "\", \"" + strYinLie_3 + "\", \"" + strYinLie_4 + "\", \"" + strYINGLI_1 + "\", \"" + strYINGLI_2 + "\", \"" + strYINGLI_3 + "\", \"" + strYINGLI_4 + "\", \"" + strNGSubLJLength_1 + "\", \"" + strNGSubLJLength_2 + "\", \"" + strNGSubLJLength_3 + "\", \"" + strNGSubLJLength_4 + "\", \"" + strNGSubYinLieLength_1 + "\", \"" + strNGSubYinLieLength_2 + "\", \"" + strNGSubYinLieLength_3 + "\", \"" + strNGSubYinLieLength_4 + "\", \"" + strNGSubYingLiLength_1 + "\", \"" + strNGSubYingLiLength_2 + "\", \"" + strNGSubYingLiLength_3 + "\", \"" + strNGSubYingLiLength_4 + "\", \"" + strNGRadiusLength + "\", \""  + strNGSubTotalLength + "\" , \"" + DateTime.Now.ToString()+ "\"); ";
                //创建命令对象
                MySqlCommand cmd = new MySqlCommand(sql, _connection);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex) 
            {
                 LogHelper.Info("RedisLog", "Mysql Connect exception " + ex.Message.ToString());
            }
            
        }

        public bool Connect(string ip, int nPort, string username , string pwd, string strdbname)
        {
            try
            {
                MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
                //用户名
                builder.UserID = username;  //root
                                            //密码
                builder.Password = pwd;  //123456
                                         //服务器地址
                builder.Server = ip; //localhost

                builder.Port = (uint)nPort;  //3306

                //连接时的数据库
                builder.Database = strdbname;
                //定义与数据连接的链接
                _connection = new MySqlConnection(builder.ConnectionString);
                //打开这个链接
                _connection.Open();

                 LogHelper.Info("RedisLog", "MySQL Connected OK " + username + " " + pwd + " " + ip + " " + nPort.ToString());
                return true;
            }
            catch(Exception ex)
            {
                 LogHelper.Info("RedisLog", "Mysql Connect exception " + ex.Message.ToString());
            }

            return false;
            
        }
    }
}
