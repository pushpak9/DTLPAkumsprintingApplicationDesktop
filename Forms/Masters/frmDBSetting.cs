using AkumsPrintingApplication.BusinessLayer;
using DTPLLogs;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace AkumsPrintingApplication
{
    public partial class frmDBSetting : Form
    {
        public frmDBSetting()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtServerName.Text.Trim() == "")
                {
                    blCommon.ShowMessage("Please enter server name.", 1);
                    txtServerName.Focus();
                }
                else if (txtUserID.Text.Trim() == "")
                {
                    blCommon.ShowMessage("Please enter user id.", 1);
                    txtUserID.Focus();
                }
                else if (txtPassword.Text.Trim() == "")
                {
                    blCommon.ShowMessage("Please enter password.", 1);
                    txtPassword.Focus();
                }
                else if (cmbDatabaseName.SelectedIndex == 0)
                {
                    blCommon.ShowMessage("Please select database name.", 1);
                    cmbDatabaseName.Focus();
                }
                else if (cmbDatabaseName.Text.Length == 0)
                {
                    blCommon.ShowMessage("Please select database name.", 1);
                    cmbDatabaseName.Focus();
                }
                else
                {
                    string file_name = "\\DBSetting.ini";
                    StreamWriter wr = new StreamWriter(Application.StartupPath + file_name);
                    wr.WriteLine("Data Source" + "~" + txtServerName.Text + ";" + " Initial Catalog" + "~" + cmbDatabaseName.Text.Trim() + ";" + "UID" + "~" + EncryptDecrptClass.Encrypt_data(txtUserID.Text.Trim()) + ";" + "Password" + "~" + EncryptDecrptClass.Encrypt_data(txtPassword.Text.Trim()) + ";");
                    wr.Close();
                    blCommon.ShowMessage("Database setting has been saved, Please Login again.", 1);
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 2);
            }
        }


        private void frmDBSetting_Load(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 2);
            }
        }
        private void cmbDatabaseName_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtServerName.Text == string.Empty)
                {
                    blCommon.ShowMessage("Please enter server name", 1);
                    txtServerName.Focus();
                    return;
                }
                if (txtUserID.Text == string.Empty)
                {
                    blCommon.ShowMessage("Please enter user id", 1);
                    txtUserID.Focus();
                    return;
                }
                if (txtPassword.Text == string.Empty)
                {
                    blCommon.ShowMessage("Please enter password", 1);
                    txtPassword.Focus();
                    return;
                }
                string connection = "Data Source=" + txtServerName.Text + " ; User ID=" + txtUserID.Text + "; Password= " + txtPassword.Text;
                using (var con = new SqlConnection(connection))
                {
                    con.Open();
                    Cursor = Cursors.WaitCursor;
                    DataTable databases = con.GetSchema("Databases");
                    cmbDatabaseName.DataSource = databases;
                    cmbDatabaseName.DisplayMember = "database_name";
                    cmbDatabaseName.ValueMember = "database_name";
                    Cursor = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage("Server connection failed. Please enter correct login id and password", 3);
                txtPassword.Text = string.Empty;
                txtPassword.Focus();
            }
        }

        private void tpDB_Click(object sender, EventArgs e)
        {

        }
    }
}
