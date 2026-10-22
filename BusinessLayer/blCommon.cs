using DTPLLogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace AkumsPrintingApplication.BusinessLayer
{
    public static class blCommon
    {
        public static bool bPasswordOK = false;
        public static string strLogFile = Application.StartupPath + "\\Log\\";
        public static string sMessageBox = "Akums Weighing Label Printing Application [VER : - " + Application.ProductVersion + " ]";
        public static void FillComboBox(ComboBox cbo, DataTable dt, bool isSelect)
        {
            try
            {
                if (isSelect)
                {
                    DataRow dr = dt.NewRow();
                    dr[0] = "--Select--";
              
                    

                    dt.Rows.InsertAt(dr, 0);
                }
                cbo.DisplayMember = dt.Columns[0].ToString();
                cbo.ValueMember = dt.Columns[1].ToString();
                cbo.DataSource = dt;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static void FillSingleColumnCombo(ComboBox cbo, DataTable dt, bool isSelect)
        {
            try
            {
                if (isSelect)
                {
                    DataRow dr = dt.NewRow();
                    dr[0] = "--Select--";
                    dt.Rows.InsertAt(dr, 0);
                }
                cbo.DisplayMember = dt.Columns[0].ToString();
                cbo.ValueMember = dt.Columns[0].ToString();
                cbo.DataSource = dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void ShowMessage(string sMessage, int iWhich)
        {
            switch (iWhich)
            {
                case 1:
                    MessageBox.Show(sMessage, sMessageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 2:
                    MessageBox.Show(sMessage, sMessageBox, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    break;
                case 3:
                    MessageBox.Show(sMessage, sMessageBox, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case 4:
                    break;
            }
        }
        public static DataTable ToDataTable<T>(this IEnumerable<T> items)
        {
            // Create the result table, and gather all properties of a T        
            DataTable table = new DataTable(typeof(T).Name);
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Add the properties as columns to the datatable
            foreach (var prop in props)
            {
                Type propType = prop.PropertyType;

                // Is it a nullable type? Get the underlying type 
                if (propType.IsGenericType && propType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                    propType = new NullableConverter(propType).UnderlyingType;

                table.Columns.Add(prop.Name, propType);
            }

            // Add the property values per T as rows to the datatable
            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                    values[i] = props[i].GetValue(item, null);

                table.Rows.Add(values);
            }

            return table;
        }

        public static void OnlyNumeric(KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        public static void OnlyChar(KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && e.KeyChar != (char)13 && e.KeyChar != (char)8 && e.KeyChar != (char)Keys.Space)
            {
                e.Handled = true;
            }
        }
        public static void DecimalValidation(KeyPressEventArgs e, object sender)
        {
            // allows 0-9, backspace, and decimal
            if (((e.KeyChar < 48 || e.KeyChar > 57) && e.KeyChar != 8 && e.KeyChar != 46))
            {
                e.Handled = true;
                return;
            }

            // checks to make sure only 1 decimal is allowed
            if (e.KeyChar == 46)
            {
                if ((sender as TextBox).Text.IndexOf(e.KeyChar) != -1)
                    e.Handled = true;
            }
        }
        public static void txtForIP(KeyPressEventArgs e)
        {
            // allows 0-9, backspace, and decimal
            if (((e.KeyChar < 48 || e.KeyChar > 57) && e.KeyChar != 8 && e.KeyChar != 46))
            {
                e.Handled = true;
                return;
            }
        }

        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            StreamReader sr = new StreamReader(strFilePath);
            string[] headers = sr.ReadLine().Split(',');
            DataTable dt = new DataTable();
            foreach (string header in headers)
            {
                dt.Columns.Add(header);
            }
            while (!sr.EndOfStream)
            {
                string str = sr.ReadLine();

                if (str != "")
                {
                    string[] rows = str.Split(',');

                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];

                    }
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }
        public static string SendCommandToPrinter(string sPrinterIP, int iPrinterPort,
            string sPrn
            )
        {
            string sResult = string.Empty;
            TcpClient client = new TcpClient(sPrinterIP, iPrinterPort);
            NetworkStream stream = client.GetStream();
            try
            {

                PCommon.mAppLog.WriteLog("Socket Connect", DTPLLogsWrite.LogType.Information, MethodBase.GetCurrentMethod());
                byte[] sendData = Encoding.ASCII.GetBytes(sPrn);
                PCommon.mAppLog.WriteLog("Command Send", DTPLLogsWrite.LogType.Information, MethodBase.GetCurrentMethod());
                stream.Write(sendData, 0, sendData.Length);
                sResult = "Success";
            }
            catch (SocketException ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                sResult = "ERROR~" + ex.Message;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (client != null)
                {
                    client.Close();
                }
            }
            return sResult;
        }


        public static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }


    }
}
