using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using NPOI.Util;
using OpenCvSharp;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using SiliconRoundBarCheck.Data;
using SquareSiliconStickCheck.Data;
using SquareSiliconStickCheck.Parameters;
using Sunny.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace SquareSiliconStickCheck.Tools
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
                    result.StrFileYinLie = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINLIE);
                    result.StrFileYingLi = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINGLI);
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
                   
                    result.StrFileYinLie = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINLIE);
                  
                    result.StrFileYingLi = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINGLI);
                   
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
                  
                    result.StrFileYinLie = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINLIE);
                  
                    result.StrFileYingLi = mySqlDataReader.GetString((int)InspectResult.emDataType.EM_FILEYINGLI);
                  
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

        public void DeleteResultInfo()
        {
            string strSql = @"SET SQL_SAFE_UPDATES = 0;
delete from b_xmartsql.squarstickresult where checkTime like '2024-03-27%';";

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

        public List<SquareStickCheckData> SelectSquareStickResultBySerial(string strDBName, string strTableName, string strSerial)
        {
            try
            {
                string sql = "SELECT * FROM `" + strDBName + "`.`" + strTableName + "` where SerialNum = \"" + strSerial + "\";";
                MySqlCommand cmd = new MySqlCommand(sql, _connection);

                MySqlDataReader mySqlDataReader = cmd.ExecuteReader();
                List<SquareStickCheckData> _list = new List<SquareStickCheckData>();

                while (mySqlDataReader.Read())
                {

                    SquareStickCheckData result = new SquareStickCheckData();

                    result.StrJBSearial = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_SerialNum);


                    string strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_LTLength);
                    string[] infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListLTLength.Add(fValue);
                    }


                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_RTLength);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListRTLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_LDLength);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListLDLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_RDLength);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListRDLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_TDLength);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListTDLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_LRLength);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListLRLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_TopDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListTopDiagLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_LeftDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListLeftDiagLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_RightDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListRightDiagLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_DownDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListDownDiagLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_TopLeftDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListTopLeftDiagLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_TopRightDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListTopRightDiagLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_LeftLeftDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListLeftLeftDiagLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_LeftRightDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListLeftRightDiagLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_RightLeftDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListRightLeftDiagLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_RightRightDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListRightRightDiagLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_DownLeftDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListDownLeftDiagLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_DownRightDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        float.TryParse(infos[i], out float fValue);
                        result.ListDownRightDiagLength.Add(fValue);
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_TopAngle);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListTopAngle.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_LeftAngle);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListLeftAngle.Add(fValue);

                        }

                    }


                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_RightAngle);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListRightAngle.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_DownAngle);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListDownAngle.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_length);
                    float.TryParse(strInfo, out float flength);
                    result.FLength = flength;


                    strInfo = mySqlDataReader.GetString(25);
                    result.SVer = float.Parse(strInfo);

                    strInfo = mySqlDataReader.GetString(26);
                    result.EVer = float.Parse(strInfo);

                    strInfo = mySqlDataReader.GetString(33);
                    result.Mnum = int.Parse(strInfo);

                    strInfo = mySqlDataReader.GetString(32);
                    result.NSquareType = int.Parse(strInfo);


                    _list.Add(result);
                }

                mySqlDataReader.Close();
                return _list;
            }
            catch (Exception ex)
            {
                LogHelper.Info("Silicon", "Mysql Select Exception " + ex.Message);
            }

            return null;
        }
        public List<SquareStickCheckData> SelectSquareStickResult(string strDBName, string strTableName)
        {
            string sql = "SELECT * FROM `" + strDBName + "`.`" + strTableName + "` order by checktime desc;";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);

            MySqlDataReader mySqlDataReader = cmd.ExecuteReader();

            try
            {
                
                List<SquareStickCheckData> _list = new List<SquareStickCheckData>();

                while (mySqlDataReader.Read())
                {

                    SquareStickCheckData result = new SquareStickCheckData();

                    result.StrJBSearial = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_SerialNum);


                    string strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_LTLength);
                    string[] infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListLTLength.Add(fValue);
                        }
                    }


                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_RTLength);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListRTLength.Add(fValue);
                        }
                        
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_LDLength);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListLDLength.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_RDLength);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListRDLength.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_TDLength);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListTDLength.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_LRLength);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true ==  float.TryParse(infos[i], out float fValue))
                        {
                            result.ListLRLength.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_TopDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListTopDiagLength.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_LeftDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true ==  float.TryParse(infos[i], out float fValue))
                        {
                            result.ListLeftDiagLength.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_RightDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListRightDiagLength.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_DownDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListDownDiagLength.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_TopLeftDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true ==  float.TryParse(infos[i], out float fValue))
                        {
                            result.ListTopLeftDiagLength.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_TopRightDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListTopRightDiagLength.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_LeftLeftDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListLeftLeftDiagLength.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_LeftRightDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListLeftRightDiagLength.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_RightLeftDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListRightLeftDiagLength.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_RightRightDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListRightRightDiagLength.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_DownLeftDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListDownLeftDiagLength.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_DownRightDiag);
                    infos = strInfo.Split(',');
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (true == float.TryParse(infos[i], out float fValue))
                        {
                            result.ListDownRightDiagLength.Add(fValue);
                        }
                    }

                    strInfo = mySqlDataReader.GetString((int)SquareStickCheckData.emDataType.EM_length);
                    float.TryParse(strInfo, out float flength);
                    result.FLength = flength;
                   
                    _list.Add(result);
                }

                mySqlDataReader.Close();
                return _list;
            }
            catch (Exception ex)
            {
                mySqlDataReader.Close();
                LogHelper.Info("Silicon", "Mysql Select Exception " + ex.Message);
            }

            return null;
           

        }

        public void InsertSquareStickCheckResultInfo(string strDBName, string strTableName, string strSerialnum, string strLTLength, string strRTLength, string strLDLength, string strRDLength, string strTDLength, string strLRLength, string strTopDiag, string strLeftDiag, string strRightDiag, string strDownDiag, string strTopLeftDiag, string strTopRightDiag, string strLeftLeftDiag, string strLeftRightDiag, string strRightLeftDiag, string strRightRightDiag, string strDownLeftDiag, string strDownRightDiag, string strTopAngle , string strLeftAngle , string strRightAngle , string strDownAngle, string strResult, float fLength, string strDateTime = "",string SVer="", string EVer="",int Ser=0,int Mnum=0)
        {
            try
            { 
                string sql = "INSERT INTO `" + strDBName + "`.`" + strTableName + "` (`SerialNum`,`LTLength`,`RTLength`,`LDLength`,`RDLength`,`TDLength`,`LRLength`,`TopDiag`,`LeftDiag`,`RightDiag`,`DownDiag`,`TopLeftDiag`, `TopRightDiag`, `LeftLeftDiag`,`LeftRightDiag`,`RightLeftDiag`,`RightRightDiag`,`DownLeftDiag`,`DownRightDiag`,`TopAngle`,`LeftAngle`,`RightAngle`,`DownAngle`,`Result`,`length`,`checktime`,`SVer`,`EVer`,`Ser`,`Mnum`) VALUES(\"" + strSerialnum + "\", \"" + strLTLength + "\", \"" + strRTLength + "\", \"" + strLDLength + "\", \"" + strRDLength + "\", \"" + strTDLength + "\", \"" + strLRLength + "\", \"" + strTopDiag + "\", \"" + strLeftDiag + "\", \"" + strRightDiag + "\", \"" + strDownDiag + "\", \"" + strTopLeftDiag + "\", \"" + strTopRightDiag + "\", \"" + strLeftLeftDiag + "\", \"" + strLeftRightDiag + "\", \"" + strRightLeftDiag + "\", \"" + strRightRightDiag + "\", \"" + strDownLeftDiag + "\", \"" + strDownRightDiag + "\", \"" + strTopAngle + "\", \"" + strLeftAngle + "\", \"" + strRightAngle +"\", \"" + strDownAngle + "\", " + strResult + "," + fLength + ",\"" + strDateTime + "\",\"" + SVer + "\",\"" + EVer + "\",\""+Ser+"\",+\""+Mnum+"\"); ";
                MySqlCommand cmd = new MySqlCommand(sql, _connection);

                  cmd.ExecuteNonQuery();
                string [] N=new string[strSerialnum.Length];

                System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();

                #region 晶编写入寄存器
                //try
                //{

                //    for (int i = 0; i < strSerialnum.Length; i++)
                //    {
                //        string vstr = strSerialnum.Substring(i, 1);
                //        N[i]= vstr;
                //        int intAsciiCode = (int)asciiEncoding.GetBytes(vstr)[0];
                //        CMoveControllerModbusTool.Instance().WriteSingleRegister(7001+i, intAsciiCode);
                //    }
                //    for (int i = strSerialnum.Length; i < 16; i++)
                //    {
                //        CMoveControllerModbusTool.Instance().WriteSingleRegister(7001 + i, 0);
                //    }

                //}
                //catch (Exception)
                //{


                //}
                #endregion
                int ER = 0;
                if (SettingParameter.Instance().NDaemon != 1)

                {
                    if (int.Parse(strResult)==1)
                    {
                        CMoveControllerModbusTool.Instance().WriteSingleCoil(2221, true);
                    }
                    CMoveControllerModbusTool.Instance().WriteSingleCoil(2220, true);

                   // CMoveControllerModbusTool.Instance().WriteSingleRegister(7000, 1);//检测完成
                  
                    GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "检测数据上传mes";
                    int o = 0;
                    //while (true)
                    //{

                   
                    //Thread.Sleep(1000);
                    //CMoveControllerModbusTool.Instance().ReadSingleRegister(7500, ref ER);
                    //if (ER == 1)
                    //{
                    //    CMoveControllerModbusTool.Instance().WriteSingleRegister(7000, 0);//检测完成
                    //    GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "收到Mes回复,接收数据成功";
                    //        break;
                    //    }
                    //if (ER == 2)
                    //{
                    //    CMoveControllerModbusTool.Instance().WriteSingleRegister(7000, 0);//检测完成
                    //    GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "收到Mes回复,接收数据失败";
                    //        break;
                    //}
                    //    o = o + 1;
                    //    if (o>5)
                    //    {
                    //        CMoveControllerModbusTool.Instance().WriteSingleRegister(7000, 0);//检测完成
                    //        GlobalDatastate.Instance().stateUpdate = GlobalDatastate.Instance().stateUpdate + "\r\n" + "未收到Mes回复！！！";
                    //        break;
                    //    }
                    //}
                   
                }


            }
            catch(Exception ex)
            {
                LogHelper.Info("Silicon", "Mysql Insert Exception " + ex.Message);
            }

            LogHelper.Info("Silicon", "Mysql Insert OK " + strSerialnum);
        }
        public void InsertResultInfo(string strDBName, string strTableName, string strSiliconStickNum, float fAppearanceLength, float fAppearanceValidLength, float fAppearanceMaxRadius, float fAppearanceMinRadius, int nSiliconLineNum, string strAbnormalAreaFir, string strAbnormalAreaSec, string strAbnormalAreaThr, string strAbnormalAreaFourth, float fDrawLinePos1, float fDrawLinePos2, float fDrawLinePos3, float fDrawLinePos4, float fDrawLinePos5, float fDrawLinePos6, float fDrawLinePos7, float fDrawLinePos8, float fDrawLinePos9, float fDrawLinePos10, float fDrawLinePos11, float fDrawLinePos12)
        {
            try
            { 
                string sql = "INSERT INTO `" + strDBName + "`.`" + strTableName +"`(`SiliconStickNum`,`AppearanceLength`,`AppearanceValidLength`,`AppearanceMaxRadius`,`AppearanceMinRadius`,`SiliconLineNum`,`AbnormalAreaFir`,`AbnormalAreaSec`,`AbnormalAreaThr`,`AbnormalAreaFourth`,`DrawLinePos1`,`DrawLinePos2`,`DrawLinePos3`,`DrawLinePos4`,`DrawLinePos5`,`DrawLinePos6`,`DrawLinePos7`,`DrawLinePos8`,`DrawLinePos9`,`DrawLinePos10`,`DrawLinePos11`,`DrawLinePos12`) VALUES ( \"" + strSiliconStickNum +  "\"" + "," + fAppearanceLength.ToString("0.00000") + "," + fAppearanceValidLength.ToString("0.00000") +", " + fAppearanceMaxRadius.ToString("0.00000") +"," + fAppearanceMinRadius.ToString("0.00000") + ", " + nSiliconLineNum.ToString() + " , \"" + strAbnormalAreaFir  + "\", \"" +  strAbnormalAreaSec + "\", \"" + strAbnormalAreaThr   + "\", \"" + strAbnormalAreaFourth  + "\", " + fDrawLinePos1.ToString("0.00000") + " , " + fDrawLinePos2.ToString("0.00000") + ", " + fDrawLinePos3.ToString("0.00000") + ", " + fDrawLinePos4.ToString("0.00000") + ", " + fDrawLinePos5.ToString("0.00000") + "," + fDrawLinePos6.ToString("0.00000") + ", " + fDrawLinePos7.ToString("0.00000") + "," + fDrawLinePos8.ToString("0.00000") + "," + fDrawLinePos9.ToString("0.00000")  + ", " + fDrawLinePos10.ToString("0.00000") + ", " + fDrawLinePos11.ToString("0.000000")  + " ," + fDrawLinePos12.ToString("0.00000") +");";
                //创建命令对象
                MySqlCommand cmd = new MySqlCommand(sql, _connection);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                 LogHelper.Info("RedisLog", "Mysql Connect exception " + ex.Message.ToString());
            }
        }

        public void InsertResult(int nResult, string strYinLie,  string strYINGLI)
        {
            try
            {
                //写入sql语句
                string sql = "INSERT INTO `stickresult`(`result`,`YinLieFile`,`YingLiFile`,`checktime`) VALUES( " + nResult.ToString() + " , \"" + strYinLie + "\" , \""  + strYINGLI + "\", \"" + DateTime.Now.ToString()+ "\"); ";
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
