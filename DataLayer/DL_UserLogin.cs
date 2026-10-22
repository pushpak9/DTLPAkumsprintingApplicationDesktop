using AkumsPrintingApplication.PL;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace AkumsPrintingApplication.DataLayer
{
    public class DL_UserLogin
    {
        Common obj = new Common();
        SqlCommand cmd = null;

        public DataTable dtBindSiteCode()
        {
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand();
                cmd.CommandText = "USP_USERLOGIN";
                cmd.Parameters.AddWithValue("@type", "BINDSITECODE");
                DataSet ds = obj.ExecuteDataset(cmd);
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogs.DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                throw ex;
            }
            return dt;
        }
        public DataTable dtBindManufacture(string sSiteCode)
        {
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand();
                cmd.CommandText = "USP_USERLOGIN";
                cmd.Parameters.AddWithValue("@type", "BINDLINEManufacture");
                cmd.Parameters.AddWithValue("@SITECODE", sSiteCode);
                DataSet ds = obj.ExecuteDataset(cmd);
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogs.DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                throw ex;
            }
            return dt;
        }

        public DataTable dtBindLINECODE(string sSiteCode)
        {
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand();
                cmd.CommandText = "USP_USERLOGIN";
                cmd.Parameters.AddWithValue("@type", "BINDLINECODE");
                cmd.Parameters.AddWithValue("@SITECODE", sSiteCode);
                DataSet ds = obj.ExecuteDataset(cmd);
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogs.DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                throw ex;
            }
            return dt;
        }

        public DataTable dtGetUserLogin(UserMasterModel objUserMasterModel)
        {
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand();
                cmd.CommandText = "USP_USERLOGIN";
                cmd.Parameters.AddWithValue("@type", "LOGIN");
                cmd.Parameters.AddWithValue("@USERID", objUserMasterModel.UserID);
                cmd.Parameters.AddWithValue("@PASSWORD", objUserMasterModel.Password);
                cmd.Parameters.AddWithValue("@SITECODE", objUserMasterModel.SiteCode);
                DataSet ds = obj.ExecuteDataset(cmd);
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogs.DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                throw ex;
            }
            return dt;
        }

        public DataTable dtCheckVersion()
        {
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand();
                cmd.CommandText = "USP_USERLOGIN";
                cmd.Parameters.AddWithValue("@type", "VALIDATEVERSION");
                DataSet ds = obj.ExecuteDataset(cmd);
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogs.DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                throw ex;
            }
            return dt;
        }

        public DataTable ValidateRights(UserMasterModel objUserMasterModel)
        {
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand();
                cmd.CommandText = "USP_USERLOGIN";
                cmd.Parameters.AddWithValue("@type", "VALIDATEMODULE");
                cmd.Parameters.AddWithValue("@USERID", objUserMasterModel.UserID);
                cmd.Parameters.AddWithValue("@SITECODE", objUserMasterModel.SiteCode);
                DataSet ds = obj.ExecuteDataset(cmd);
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogs.DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                throw ex;
            }
            return dt;
        }


        public DataTable WriteErrorLog(ErrorLogModel objErrorLog)
        {
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand();
                cmd.CommandText = "USP_ERRORLOG";
                cmd.Parameters.AddWithValue("@SiteCode", PCommon.SiteCode);
                cmd.Parameters.AddWithValue("@LINECODE", PCommon.LineCode);
                cmd.Parameters.AddWithValue("@USERCODE", PCommon.UserID);
                cmd.Parameters.AddWithValue("@LOGTYPE", objErrorLog.LogType);
                cmd.Parameters.AddWithValue("@MODULENAME", objErrorLog.ModuleName);
                cmd.Parameters.AddWithValue("@MESSAGE", objErrorLog.Message);
                cmd.Parameters.AddWithValue("@Severity", objErrorLog.Severity);
                cmd.Parameters.AddWithValue("@BATCHNO", objErrorLog.BatchNo);
                cmd.Parameters.AddWithValue("@DEVICEIP", PCommon.sIP);
                DataSet ds = obj.ExecuteDataset(cmd);
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogs.DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                throw ex;
            }
            return dt;
        }
    }
}