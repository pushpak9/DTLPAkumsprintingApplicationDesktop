using DTPLLogs;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;

namespace AkumsPrintingApplication
{
    public static class GlobalHelper
    {
        public static ComboBox DrpPrn { get; set; }

        //public static string GetSelectedPrnFile()
        //{
        //    if (DrpPrn != null && DrpPrn.SelectedItem != null)
        //    {
        //        return DrpPrn.SelectedItem.ToString();
        //    }
        //    else
        //    {
        //        //MessageBox.Show("Please select a file from the dropdown.");
        //        return string.Empty;
        //    }
        //}
    }
    public class PCommon
    {
        public static string sPrnName = string.Empty;
        public static string strLogFile = Application.StartupPath;
        public static DTPLLogs.DTPLLogsWrite mAppLog = new DTPLLogsWrite();

        #region
        public static string sDBSettingPath { get; set; }
        public static string sDbUserID = string.Empty;
        public static string sDbPassword = string.Empty;
        public static string sDbServerName = string.Empty;
        public static string sDbDataBaseName = string.Empty;

        #endregion
        public static string GroupID = string.Empty;
        public static string UserID = string.Empty;
        public static string SiteCode = string.Empty;
        public static string LineCode = string.Empty;
        public static string UserName = string.Empty;
        public static string Sitename = string.Empty;
        public static string Address1 = string.Empty;
        public static string Address2 = string.Empty;
        public static string Address3 = string.Empty;


        public static string sIPAddress = string.Empty;
        public static string StrCon = string.Empty;
        public static string StrSqlCon = string.Empty;
        public static string sHostName = string.Empty;
        public static string sSystemName = string.Empty;
        public static string iPortNo = string.Empty;
        public static string sIP = string.Empty;


        public static void FillListView(DataTable dt, ListView lv)
        {
            lv.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                ListViewItem item = new ListViewItem(row[0].ToString());

                for (int i = 1; i < dt.Columns.Count; i++)
                {
                    item.SubItems.Add(row[i].ToString());
                }
                lv.Items.Add(item);
            }
        }
        public static void KillUnusedExcelProcess()
        {
            Process[] oXlProcess = Process.GetProcessesByName("EXCEL");
            foreach (Process oXLP in oXlProcess)
            {
                oXLP.Kill();
            }
        }

        //Used to fill the listview
        public static void FillListView(DataTable dt, ListView lv, bool clear)
        {
            if (clear)
                lv.Items.Clear();

            foreach (DataRow row in dt.Rows)
            {
                ListViewItem item = new ListViewItem(row[0].ToString());

                for (int i = 1; i < dt.Columns.Count; i++)
                {
                    item.SubItems.Add(row[i].ToString());
                }
                lv.Items.Add(item);
            }
            // lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }
    }
}
