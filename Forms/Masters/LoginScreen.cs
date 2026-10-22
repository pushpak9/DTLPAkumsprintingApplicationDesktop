using AkumsPrintingApplication.BusinessLayer;
using AkumsPrintingApplication.DataLayer;
using AkumsPrintingApplication.Forms.Transaction;
using AkumsPrintingApplication.PL;
using BNPMATS.Class;
using DTPLLogs;
using MaterialSkin.Controls;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Windows.Forms;

namespace AkumsPrintingApplication
{
    public partial class LoginScreen : MaterialForm
    {
        DL_UserLogin obj = new DL_UserLogin();
        private string PrinterPort;


        public LoginScreen()
        {

            InitializeComponent();
            //this.Load += new EventHandler(LoginScreen_Load);



        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            userLogin();
        }
        public string ReadConnectionFile(string sPath)
        {
            string _sResult = string.Empty;
            try
            {
                StreamReader sr = new StreamReader(sPath);
                _sResult = sr.ReadLine();
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                _sResult = string.Empty;
            }
            return _sResult;
        }

        private void BindSiteCode()
        {
            try
            {
                DL_UserLogin dlobj = new DL_UserLogin();
                DataTable dt = dlobj.dtBindSiteCode();
                if (dt.Rows.Count > 0)
                {
                    blCommon.FillComboBox(cmbSiteCode, dt, true);
                    if (dt.Rows.Count == 1)
                    {
                        cmbSiteCode.SelectedIndex = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                throw ex;
            }
        }
        private void LoginScreen_Load(object sender, EventArgs e)
        {
            try
            {
                //this.MinimumSize = new Size(1024, 768);


                lblVersion.Text = "Copyright @Dashtech  @ " + Application.ProductVersion;

                #region Log Write
                //----------------------------- //
                DirectoryInfo _dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\" + "Logs");
                if (_dir.Exists == false)
                {
                    _dir.Create();
                }
                PCommon.mAppLog = new DTPLLogsWrite();
                PCommon.mAppLog.iLogDays = 10;
                PCommon.mAppLog.sLogFilePath = AppDomain.CurrentDomain.BaseDirectory;
                PCommon.mAppLog.sLogName = "Dashtech";
                PCommon.mAppLog.WriteLog("Initialize Application", DTPLLogsWrite.LogType.Information, MethodBase.GetCurrentMethod());
                PCommon.mAppLog.WriteLog("Dashtech" + "  ::  Version" + Application.ProductVersion, DTPLLogsWrite.LogType.Information, MethodBase.GetCurrentMethod());

                #endregion

                PCommon.sDBSettingPath = Application.StartupPath + "\\DbSetting.ini";
                PCommon.StrSqlCon = ReadConnectionFile(PCommon.sDBSettingPath);// Get the connection string
                if (!string.IsNullOrEmpty(PCommon.StrSqlCon))
                {
                    PCommon.StrSqlCon = PCommon.StrSqlCon.Replace(PCommon.StrSqlCon.Split(';')[2].Split('~')[1], EncryptDecrptClass.Decrypt_data(PCommon.StrSqlCon.Split(';')[2].Split('~')[1]));
                    PCommon.StrSqlCon = PCommon.StrSqlCon.Replace(PCommon.StrSqlCon.Split(';')[3].Split('~')[1], EncryptDecrptClass.Decrypt_data(PCommon.StrSqlCon.Split(';')[3].Split('~')[1]));
                    PCommon.StrSqlCon = PCommon.StrSqlCon.Replace('~', '=');
                }
                if (PCommon.StrSqlCon == string.Empty)
                {
                    blCommon.ShowMessage("Unable to read DbSetting.ini", 1);
                    Cursor.Current = Cursors.WaitCursor;
                    frmDBSetting objServer = new frmDBSetting();
                    objServer.ShowDialog();
                }
                else
                {
                    try
                    {
                        SqlConnection cn = new SqlConnection(PCommon.StrSqlCon);
                        cn.Open();
                        cn.Close();
                        BindSiteCode();
                        txtUserID.Focus();
                    }
                    catch (Exception)
                    {
                        blCommon.ShowMessage("Unable to connect with last database setting. Please configure correct database setting.", 1);
                        // open config setting file
                        Cursor.Current = Cursors.WaitCursor;
                        frmDBSetting objServer = new frmDBSetting();
                        objServer.ShowDialog();
                    }
                    BindPrnLogic();
                    drpPrn.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                Cursor.Current = Cursors.Default;
                frmDBSetting objServer = new frmDBSetting();
                objServer.ShowDialog();
            }
        }
        public void retriveDatabaseSettingandLogin()
        {
            try
            {
                PCommon.sDbServerName = PCommon.StrSqlCon.Split(';')[0].Split('=')[1];
                PCommon.sDbDataBaseName = PCommon.StrSqlCon.Split(';')[1].Split('=')[1];
                PCommon.sDbUserID = PCommon.StrSqlCon.Split(';')[2].Split('=')[1];
                PCommon.sDbPassword = PCommon.StrSqlCon.Split(';')[3].Split('=')[1];
                PCommon.UserID = txtUserID.Text.Trim();
                PCommon.sHostName = Dns.GetHostName(); // Retrive the Name of HOST
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        public void userLogin()
        {
            try
            {

                string sResult = string.Empty;
                if (cmbSiteCode.SelectedIndex == 0)
                {
                    blCommon.ShowMessage("Please select site code", 1);
                    cmbSiteCode.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(txtUserID.Text.Trim()))
                {
                    blCommon.ShowMessage("Please enter user id", 1);
                    txtUserID.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(txtPassword.Text.Trim()))
                {
                    blCommon.ShowMessage("Please enter password", 1);
                    txtPassword.Focus();
                    return;
                }
                if (cmbmnufactre.Items.Count == 0)
                {
                    blCommon.ShowMessage("No line is mapped with the selected site, Please contact admin", 1);
                    cmbSiteCode.Focus();
                    return;
                }
                if (cmbmnufactre.SelectedIndex == 0)
                {
                    blCommon.ShowMessage("Please select Line code", 1);
                    cmbmnufactre.Focus();
                    return;
                }
                if (drpPrn.SelectedIndex == 0)
                {
                    blCommon.ShowMessage("Please select prn file", 1);
                    drpPrn.Focus();
                    return;
                }
                UserMasterModel plobj = new UserMasterModel();
                #region Check for application version
                DataTable dt = new DataTable();
                obj = new DataLayer.DL_UserLogin();
                plobj.ProductVersion = Application.ProductVersion;
                plobj.SiteCode = PCommon.SiteCode;
                dt = obj.dtCheckVersion();
                if (dt.Rows.Count > 0)
                {
                    sResult = dt.Rows[0][0].ToString();
                    if (sResult != plobj.ProductVersion)
                    {
                        if (sResult != Application.ProductVersion)
                        {
                            ErrorLogModel objModel = new ErrorLogModel();
                            objModel.Message = "Windows Login application with user iD :" + txtUserID.Text + ", and Application Version Changed ";
                            objModel.ModuleName = "Login";
                            objModel.LogType = "Error";
                            objModel.Severity = "2";
                            obj.WriteErrorLog(objModel);
                            blCommon.ShowMessage("Application version Changed, Please wait for downloading of new version", 2);
                            return;
                        }
                    }
                }
                #endregion
                PCommon.sIP = NetworkInterface
    .GetAllNetworkInterfaces()
    .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
    .Select(nic => nic.GetPhysicalAddress().ToString())
    .FirstOrDefault();
                plobj = new UserMasterModel();
                plobj.UserID = txtUserID.Text.Trim();
                plobj.Password = clsEncryptDecrypt.encrypt(txtPassword.Text.Trim());
                plobj.SiteCode = cmbSiteCode.SelectedValue.ToString();
                obj = new DataLayer.DL_UserLogin();
                DataTable dtResult = obj.dtGetUserLogin(plobj);
                if (dtResult.Rows.Count > 0)
                {
                    sResult = dtResult.Rows[0][0].ToString();
                    if (sResult.StartsWith("N~") || sResult.StartsWith("ERROR~"))
                    {
                        blCommon.ShowMessage(sResult.Split('~')[1], 2);
                        txtUserID.Text = string.Empty;
                        txtPassword.Text = string.Empty;
                        txtUserID.Focus();
                        return;
                    }
                    else if (sResult == "0")
                    {
                        blCommon.ShowMessage("Invalid UserID and Password. Please try again", 1);
                        txtUserID.Text = string.Empty;
                        txtPassword.Text = string.Empty;
                        txtUserID.Focus();
                        return;
                    }
                    else if (sResult == string.Empty)
                    {
                        blCommon.ShowMessage("Password is not correct. Please try again", 1);
                        txtPassword.Text = string.Empty;
                        txtPassword.Focus();
                        return;
                    }

                    string sLoginID = txtUserID.Text;
                    PCommon.UserID = sLoginID;
                    PCommon.SiteCode = cmbSiteCode.SelectedValue.ToString();
                    PCommon.UserName = sResult.Split('~')[1];
                    string isActive = sResult.Split('~')[2];
                    PCommon.LineCode = CmbLINECODE.Text;
                    string manufactureradress = cmbmnufactre.Text;
                    string[] manufacturerAddressParts = manufactureradress.Split('_');
                    PCommon.Sitename = manufacturerAddressParts[0];
                    PCommon.Address1 = manufacturerAddressParts[1];
                    PCommon.Address2 = manufacturerAddressParts[2];
                    PCommon.Address3 = manufacturerAddressParts[3];


                    PCommon.sPrnName = drpPrn.Text;
                    if (isActive == "False" || isActive == "0")
                    {
                        blCommon.ShowMessage("Selected user is not active, Please enter another user id", 1);
                        txtUserID.Text = string.Empty;
                        txtPassword.Text = string.Empty;
                        txtUserID.Focus();
                        return;
                    }

                    DataTable dtValidateWeighingLabelPrinting = new DataTable();
                    dtValidateWeighingLabelPrinting = obj.ValidateRights(plobj);
                    if (dtValidateWeighingLabelPrinting.Rows.Count > 0)
                    {
                        DataTable bValidate = dtValidateWeighingLabelPrinting.AsEnumerable().Where(i => i.Field<string>("ModuleName") == "Check Weight Printing").CopyToDataTable();
                        if (bValidate.Rows.Count > 0)
                        {
                            ErrorLogModel objModel = new ErrorLogModel();
                            objModel.Message = "Login into the application with user iD :" + txtUserID.Text;
                            objModel.ModuleName = "Login";
                            objModel.LogType = "Message";
                            objModel.Severity = "2";
                            obj.WriteErrorLog(objModel);

                            PCommon.sSystemName = Environment.MachineName.ToString();
                            retriveDatabaseSettingandLogin();

                            Process[] arrProcess = Process.GetProcessesByName("AkumsPrintingApplication");
                            if (arrProcess.Length > 6)
                            {
                                blCommon.ShowMessage("Max system already running, Please close the previous line", 3);
                                return;
                            }
                            MasterCartonLabelPrinting objmenu = new MasterCartonLabelPrinting();
                            txtUserID.Text = string.Empty;
                            txtPassword.Text = string.Empty;
                            txtUserID.Focus();
                            this.Hide();
                            objmenu.Show();

                        }
                        else
                        {
                            blCommon.ShowMessage("Un-authorized access for weighing printing", 1);
                            txtUserID.Text = string.Empty;
                            txtPassword.Text = string.Empty;
                            txtUserID.Focus();
                            return;
                        }
                    }
                    else
                    {
                        blCommon.ShowMessage("Un-authorized access for weighing printing", 1);
                        txtUserID.Text = string.Empty;
                        txtPassword.Text = string.Empty;
                        txtUserID.Focus();
                        return;
                    }
                    return;
                }
                else
                {
                    blCommon.ShowMessage("UserID and password is not correct. Please try again", 1);
                    txtUserID.Text = string.Empty;
                    txtPassword.Text = string.Empty;
                    txtUserID.Focus();
                }
            }
            catch (Exception ex)
            {
                ErrorLogModel objModel = new ErrorLogModel();
                objModel.Message = "Error into the Windows Login application with user iD :" + txtUserID.Text + ", and Error is " + ex.Message;
                objModel.ModuleName = "Login";
                objModel.LogType = "Error";
                objModel.Severity = "2";
                obj.WriteErrorLog(objModel);
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void txtUserID_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(txtUserID, "Enter User ID");
        }
        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                userLogin();
            }
        }

        private void txtUserID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != (char)13 && e.KeyChar != (char)8)//e.KeyChar!=(char)Keys.Space)
            {
                e.Handled = true;
            }
            if (txtUserID.Text != string.Empty && e.KeyChar == (char)13)
            {
                txtPassword.Focus();
            }
        }

        private void cmbSiteCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {

                cmbmnufactre.DropDownWidth = 100; // Increase as per item length

                cmbmnufactre.DataSource = null;
                cmbmnufactre.Items.Clear();
                if (cmbSiteCode.SelectedIndex > 0)
                {
                    DL_UserLogin obj = new DL_UserLogin();
                    DataTable dt = obj.dtBindManufacture(cmbSiteCode.SelectedValue.ToString());
                    if (dt.Rows.Count > 0)
                    {

                        blCommon.FillComboBox(cmbmnufactre, dt, true);

                    }
                    else
                    {
                        blCommon.ShowMessage("No line is mapped with the selected site, Please contact admin", 3);
                        cmbSiteCode.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {


        }

        private void cmbLineCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {

                CmbLINECODE.DataSource = null;
                CmbLINECODE.Items.Clear();
                if (cmbmnufactre.SelectedIndex > 0)
                {
                    DL_UserLogin obj = new DL_UserLogin();
                    DataTable dt = obj.dtBindLINECODE(cmbSiteCode.SelectedValue.ToString());
                    if (dt.Rows.Count > 0)
                    {
                        blCommon.FillComboBox(CmbLINECODE, dt, true);
                    }
                    else
                    {
                        blCommon.ShowMessage("No line is mapped with the selected site, Please contact admin", 3);
                        cmbSiteCode.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        //private string GetSelectedPrnFile()
        //{
        //    if (drpPrn.SelectedItem != null)
        //    {
        //        return drpPrn.SelectedItem.ToString();
        //    }
        //    else
        //    {
        //        MessageBox.Show("Please select a file from the dropdown.");
        //        return string.Empty;
        //    }
        //}

        public void BindPrnLogic()
        {

            // Specify the folder path
            string folderPath = Application.StartupPath + @"\LabelDesign";
            // Get all text files in the folder
            string[] textFiles = Directory.GetFiles(folderPath, "*.prn");
            DataTable dt = new DataTable();
             dt.Columns.Add("PrnName");
            // Extract the file names from the paths
            for (int i = 0; i < textFiles.Length; i++)
            {
                DataRow dr = dt.NewRow();
                dr[0] = Path.GetFileName(textFiles[i]);
                dt.Rows.Add(dr);
            }
            // Bind the file names to the ComboBox
            blCommon.FillSingleColumnCombo(drpPrn, dt, true);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {

        }
    }
}
