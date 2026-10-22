using AkumsPrintingApplication.BusinessLayer;
using AkumsPrintingApplication.DataLayer;
using AkumsPrintingApplication.PL;
using BNPMATS.Class;
using DTPLLogs;
using iTextSharp.text.pdf.fonts.cmaps;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AkumsPrintingApplication.Forms.Transaction
{
    public partial class MasterCartonLabelPrinting : Form
    {
        #region Variable Declaration
        ClsSocket.ClientSocket_MultiSocket objBin = new ClsSocket.ClientSocket_MultiSocket();
        bool bWeightCaptured = false;
        System.Windows.Forms.Timer PingTimer = new System.Windows.Forms.Timer();
        public int iScannerDisconnctedd = 0;

        static string sPlantname = string.Empty;
        static string ManualTare = string.Empty;
        static string AutoTare = string.Empty;
        static string GenericName = string.Empty;
        string SResult1 = string.Empty;
        string SResult2 = string.Empty;
        static string SuperviserName = string.Empty;
        static string sPlantAddress1 = string.Empty;
        static string sPlantAddress2 = string.Empty;
        static string sPlantAddress3 = string.Empty;
        string Medicine = string.Empty;
        bool bTFWeight = false;
        SerialPort serialPort1 = new SerialPort();
        static string WeighingConnectionThroughIP = "1";


        #endregion

        #region Form Event
        public MasterCartonLabelPrinting()
        {
            InitializeComponent();
            this.panelPrint.Visible = false;
            // pnlReprintConfirmation.Show();

            try
            {
                WindowState = FormWindowState.Maximized;
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }

        }
        private void MasterCartonLabelPrinting_Load(object sender, EventArgs e)
        {
            //this.MinimumSize = new Size(1024, 768);

            //// ✅ Maximum size
            //this.MaximumSize = new Size(1920, 1080);
            //this.WindowState = FormWindowState.Maximized;
            //this.AutoScaleMode = AutoScaleMode.Font;




            try
            {
                txtmrp.Text = "0";
                this.Text = "Weighing Label Priniting-Line Code : " + PCommon.LineCode;
                Process[] arrProcess = Process.GetProcessesByName("AkumsPrintingApplication");
                Process p1 = Process.GetCurrentProcess();
                int iNoOfCount = 0;
                if (arrProcess.Length <= 6)
                {
                    for (int i = 0; i < arrProcess.Length; i++)
                    {
                        if (arrProcess[i].MainWindowTitle == this.Text)
                        {
                            iNoOfCount++;
                        }
                    }
                    if (iNoOfCount > 1)
                    {
                        blCommon.ShowMessage("Selected line is already running in the back ground, Please close the previous line or login with different line", 3);
                        System.Diagnostics.Process.GetCurrentProcess().Kill();
                        return;
                    }
                }
               

                WeighingConnectionThroughIP = ConfigurationManager.AppSettings["WeighingConnectionThroughIP"].ToString();
                System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
                lblLineCode.Text = PCommon.LineCode;
                lblSiteCode.Text = PCommon.SiteCode;
                lblUserID.Text = PCommon.UserID;
                lblLoginTime.Text = DateTime.Now.ToString();
                BindCustomerCode();
                GetPrinterIP();
                BindCondition();
                BindLicence();
                BindTareType();
                BindCustomerLicence();
                BindCountry();
                if (SResult2 == "True")
                {
                    txtTareWt.ReadOnly = true;
                }

                BindSuperviserName();
                BindMedicine();




                pnlReprintConfirmation.Hide();
                panelwithoutRewing.Hide();
                PnlEditValidation.Hide();
                PnlLooseWeight.Hide();



                cmbCustomer.Enabled = true;
                cmbCondition.Enabled = true;
                cmbLicence.Enabled = true;
                btnReprint.Enabled = false;
                Edit.Enabled = true;

                DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                DataTable dtPrinterDetails = obj.GetPlantDetails();
                if (dtPrinterDetails.Rows.Count > 0)
                {
                    sPlantname = dtPrinterDetails.Rows[0][0].ToString();
                    sPlantAddress1 = dtPrinterDetails.Rows[0][1].ToString();
                    sPlantAddress2 = dtPrinterDetails.Rows[0][2].ToString();
                    sPlantAddress3 = dtPrinterDetails.Rows[0][3].ToString();
                }


            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }
        private void MasterCartonLabelPrinting_FormClosed(object sender, FormClosedEventArgs e)
        {
            //try
            //{
            //    if (serialPort1 != null)
            //    {
            //        serialPort1.Close();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
            //}
        }

        #endregion

        #region Bind Method

        public void GetPrinterIP()
        {
            try
            {
                lblPrinterIP.Text = "****";
                lblPrinterPort.Text = "****";
                lblWeighingIP.Text = "****";
                lblWeighingPort.Text = "****";
                DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                DataTable dtPrinterDetails = obj.GetPrinterIP();
                if (dtPrinterDetails.Rows.Count == 0)
                {
                    lblFMSMessge.Text = "Printer details not found, Please contact admin";
                    txtProcssOrder.Text = string.Empty;
                    txtProcssOrder.Focus();
                    return;
                }
                else
                {
                    lblPrinterIP.Text = dtPrinterDetails.Rows[0][0].ToString();
                    lblPrinterPort.Text = dtPrinterDetails.Rows[0][1].ToString();
                    if (WeighingConnectionThroughIP == "1")
                    {
                        lblWeighingIP.Text = dtPrinterDetails.Rows[0][2].ToString();
                        lblWeighingPort.Text = dtPrinterDetails.Rows[0][3].ToString();
                    }
                    else
                    {
                        lblWeighingIP.Text = ConfigurationManager.AppSettings["PortName"].ToString();
                        lblWeighingPort.Text = "00";
                    }
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }
        public void BindCustomerCode()
        {
            try
            {
                PL_CartonLabelPrinting plobj = new PL_CartonLabelPrinting();
                DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                DataTable dtCustomerCode = obj.BindCustomerCode();
                if (dtCustomerCode.Rows.Count == 0)
                {
                    lblFMSMessge.Text = "Customer Details not found, Please contact admin";
                    txtProcssOrder.Text = string.Empty;
                    txtProcssOrder.Focus();
                    return;
                }
                else
                {
                    blCommon.FillComboBox(cmbCustomer, dtCustomerCode, true);
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        public void BindSuperviserName()
        {
            try
            {
                DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                DataTable dtSuperviserName = obj.BindSuperviserName();



                if (dtSuperviserName.Rows.Count == 0)
                {
                    lblFMSMessge.Text = "Superviser Details  not found, Please contact admin";
                    txtProcssOrder.Text = string.Empty;
                    txtProcssOrder.Focus();
                    return;
                }
                else
                {

                    blCommon.FillComboBox(SuperviserCMb, dtSuperviserName, true);
                    dtSuperviserName.Rows.Add("<NO DATA>");
                    dtSuperviserName.Rows.Add("Supervisor Code : <BLANK>");
                    dtSuperviserName.Rows.Add("Supervisor Name : <BLANK>");
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }

        }

        public void BindLicence()
        {
            try
            {
                DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                DataTable dtCustomerCode = obj.BindLicence();
                if (dtCustomerCode.Rows.Count == 0)
                {
                    lblFMSMessge.Text = "Licence Details not found, Please contact admin";
                    txtProcssOrder.Text = string.Empty;
                    txtProcssOrder.Focus();
                    return;
                }
                else
                {
                    blCommon.FillComboBox(cmbLicence, dtCustomerCode, true);
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        public void BindCustomerLicence()
        {
            try
            {
                DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                DataTable dtCustomerCode = obj.BindCustomerLicence();
                blCommon.FillComboBox(CmbCustLisce, dtCustomerCode, true);

            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        public void BindMedicine()
        {
            try
            {
                DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                DataTable dtCustomerCode = obj.BindMedicine();
                if (dtCustomerCode.Rows.Count == 0)
                {
                    lblFMSMessge.Text = "Medicine Details not found, Please contact admin";
                    txtProcssOrder.Text = string.Empty;
                    txtProcssOrder.Focus();
                    return;
                }
                else
                {
                    blCommon.FillComboBox(Mdicine, dtCustomerCode, true);
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }
        public void BindCondition()
        {
            try
            {
                DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                DataTable dtCustomerCode = obj.BindCondition();
                if (dtCustomerCode.Rows.Count == 0)
                {
                    lblFMSMessge.Text = "Licence Details not found, Please contact admin";
                    txtProcssOrder.Text = string.Empty;
                    txtProcssOrder.Focus();
                    return;
                }
                else
                {
                    blCommon.FillComboBox(cmbCondition, dtCustomerCode, true);
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }


        public void BindLicense()
        {
            try
            {
                DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                DataTable dtCustomerCode = obj.BindCondition();
                if (dtCustomerCode.Rows.Count == 0)
                {
                    lblFMSMessge.Text = "Licence Details not found, Please contact admin";
                    txtProcssOrder.Text = string.Empty;
                    txtProcssOrder.Focus();
                    return;
                }
                else
                {
                    blCommon.FillComboBox(cmbCondition, dtCustomerCode, true);
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        public void BindCountry()
        {
            try
            {
                DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                DataTable dtCustomerCode = obj.BindCountry();
                if (dtCustomerCode.Rows.Count > 0)
                {
                    blCommon.FillComboBox(CmdCountry, dtCustomerCode, true);
                  
                }
   
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        public void BindTareType()
        {
            try
            {
                DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                DataTable dt = obj.BindTareType();
                if (dt.Rows.Count > 0)
                {
                    ManualTare = Convert.ToString(dt.Rows[0][0]);
                    AutoTare = Convert.ToString(dt.Rows[0][2]);
                    SResult1 = ManualTare;
                    SResult2 = AutoTare;

                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }

        }

        public bool HardwareTesting(string sIP)
        {
            bool bConnected = true;
            try
            {
                Ping ping = new Ping();
                PingReply pingresult = ping.Send(sIP);
                if (pingresult.Status.ToString() == "Success")
                {
                    bConnected = true;
                }
                else
                {
                    bConnected = false;
                }
            }
            catch (Exception)
            {
                bConnected = false;
            }
            return bConnected;
        }

        private void PingTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                PingTimer.Enabled = false;
                bool bIsConnected = true;
                string sClientIP = string.Empty;
                if (objBin.IPAddress.Length > 0)
                {
                    bIsConnected = HardwareTesting(objBin.IPAddress);
                    sClientIP = objBin.IPAddress;
                    if (bIsConnected == false)
                    {
                        PCommon.mAppLog.WriteLog(" End Time : " + DateTime.Now
                                                       + ", IP : " + sClientIP
                                                       + ", State : " + bIsConnected,
                                                       DTPLLogsWrite.LogType.Message, MethodBase.GetCurrentMethod()
                                                       );
                        lblWeighingIP.BackColor = Color.Red;
                        iScannerDisconnctedd = 1;
                        System.Media.SoundPlayer player = new System.Media.SoundPlayer(System.Windows.Forms.Application.StartupPath + "\\Error.wav");
                        player.Play();
                        lblHardwareFailMessage.Visible = true;
                        //if (objBin.IsConnected)
                        //{
                        //    objBin.Poll = false;
                        //}
                        //objBin.DataArrived -= ObjBin1_DataArrived;
                    }
                    if (lblHardwareFailMessage.Visible == true && bIsConnected == true && iScannerDisconnctedd == 1)
                    {
                        PCommon.mAppLog.WriteLog(" End Time : " + DateTime.Now
                                                      + ", IP : " + sClientIP
                                                      + ", State : " + bIsConnected,
                                                      DTPLLogsWrite.LogType.Message, MethodBase.GetCurrentMethod()
                                                      );

                        objBin.IPAddress = lblWeighingIP.Text;
                        objBin.Port = Convert.ToInt32(lblWeighingPort.Text);
                        //if (!objBin.IsConnected)
                        //{
                        //    objBin.Poll = true;
                        //    objBin.Open();
                        //}
                        //if (objBin.IsConnected)
                        //{
                        //    lblFMSMessge.Text = "CONNECTED";
                        //    lblFMSMessge.BackColor = Color.Green;
                        //    lblWeighingIP.BackColor = Color.Green;
                        //    lblHardwareFailMessage.Visible = false;
                        //    objBin.DataArrived += ObjBin1_DataArrived;
                        //}
                        objBin.ReConnect();
                        ObjBin_OnConnect(0);
                    }
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
            }
            finally
            {
                PingTimer.Enabled = true;
            }
        }

        public void GetLastPrintedData()
        {
            try
            {
                DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                DataTable dtCustomerCode = obj.GetLastPrintedData(txtProcssOrder.Text);
                if (dtCustomerCode.Rows.Count > 0)
                {
                    txtTargetShipper.Text = dtCustomerCode.Rows[0]["TargetShipper"].ToString();
                    txtRemarks.Text = dtCustomerCode.Rows[0]["Remarks"].ToString();
                    cmbCondition.Text = dtCustomerCode.Rows[0]["Condition"].ToString();
                    cmbLicence.Text = dtCustomerCode.Rows[0]["Licence"].ToString();

                    if (dtCustomerCode.Rows[0]["isPhysicianSample"].ToString() == "True")
                    {
                        chkSample.Checked = true;
                    }
                    if (dtCustomerCode.Rows[0]["isNotForSale"].ToString() == "True")
                    {
                        chkNotForSale.Checked = true;
                    }
                    if (dtCustomerCode.Rows[0]["isNrx"].ToString() == "True")
                    {
                        chkNRxProduct.Checked = true;
                    }
                    if (dtCustomerCode.Rows[0]["PrintedExpiryLabelFormat"].ToString() == "Best Before")
                    {
                        rdbtnBestBefore.Checked = true;
                    }
                    else if (dtCustomerCode.Rows[0]["PrintedExpiryLabelFormat"].ToString() == "Use Before")
                    {
                        rdbtnUseBefore.Checked = true;
                    }
                    else if (dtCustomerCode.Rows[0]["PrintedExpiryLabelFormat"].ToString() == "Exp Date")
                    {
                        rdbtnExpDate.Checked = true;
                    }
                    cmbCustomer.Text = dtCustomerCode.Rows[0]["CustomerCode"].ToString();
                    txtCustomerName.Text = dtCustomerCode.Rows[0]["CustomerName"].ToString();
                    txtQuantity.Text = dtCustomerCode.Rows[0]["Quantity"].ToString();
                    txtMaxWT.Text = dtCustomerCode.Rows[0]["MAXWT"].ToString();
                    txtMinWt.Text = dtCustomerCode.Rows[0]["MINWT"].ToString();

                    //Value
                    txtTareWt.Text = dtCustomerCode.Rows[0]["TAREWT"].ToString();
                    txtmrp.Text = dtCustomerCode.Rows[0]["mrp"].ToString();
                    txtGenricName.Text = dtCustomerCode.Rows[0]["GenericField"].ToString();
                    Mdicine.Text = dtCustomerCode.Rows[0]["Medicine"].ToString();
                    SuperviserCMb.Text = dtCustomerCode.Rows[0]["SupervisorField"].ToString();
                    MediceCheck.Text = dtCustomerCode.Rows[0]["MediceCheck"].ToString();
                    CmbCustLisce.Text = dtCustomerCode.Rows[0]["CustomerLicence"].ToString();
                    CmdCountry.Text = dtCustomerCode.Rows[0]["Country"].ToString();
                    txtGenricField.Text = dtCustomerCode.Rows[0]["ProductDescription"].ToString();


                    if (dtCustomerCode.Rows[0]["YearFourDigit"].ToString() == "True")
                    {
                        RbdDDMMMYYYY.Checked = true;
                    }




                    if (dtCustomerCode.Rows[0]["MonthInChar"].ToString() == "True")
                    {
                        rdbtnMonthInChar.Checked = true;
                    }
                    if (dtCustomerCode.Rows[0]["MonthInNum"].ToString() == "True")
                    {
                        rdbtnMonthInnum.Checked = true;
                    }
                    if (dtCustomerCode.Rows[0]["YearInTwoDigit"].ToString() == "True")
                    {
                        rdbtnYearInLastTwoDigit.Checked = true;
                    }

                    if (dtCustomerCode.Rows[0]["MonthInCharYear"].ToString() == "True")
                    {
                        rbtnCHAR.Checked = true;
                    }


                    if (dtCustomerCode.Rows[0]["LL"].ToString() == "True")
                    {
                        isLLcheckbox.Checked = true;
                    }

                    if (dtCustomerCode.Rows[0]["checkBoxMedicine"].ToString() == "True")
                    {
                        checkBoxMedicine.Checked = true;
                    }


                    //if (dtCustomerCode.Rows[0]["TM"].ToString() == "True" || dtCustomerCode.Rows[0]["TM"].ToString() == "1")
                    //{
                    //    chkTM1.Checked = true;
                    //}
                    //if (dtCustomerCode.Rows[0]["R"].ToString() == "True" || dtCustomerCode.Rows[0]["R"].ToString() == "1")
                    //{
                    //    chkR1.Checked = true;
                    //}

                    if (dtCustomerCode.Rows[0]["Subsidiary"].ToString() == "True" || dtCustomerCode.Rows[0]["Subsidiary"].ToString() == "1")
                    {
                        chkSubsidiary.Checked = true;
                    }
                    if (dtCustomerCode.Rows[0]["PrintRequired"].ToString() == "True")
                    {
                        chkPrintRequired.Checked = true;
                    }




                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        public void GetBatchPrintingData()
        {
            try
            {
                DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                DataSet dtCustomerCode = obj.GetShipperPrintingData(txtProcssOrder.Text);
                if (dtCustomerCode.Tables.Count > 0)
                {
                    dgvData.DataSource = dtCustomerCode.Tables[0];
                    btnReprint.Enabled = true;
                    lblCount.Text = dtCustomerCode.Tables[1].Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        #endregion
        private delegate void SetTextDeleg(string text);
        private void ObjBin_OnConnect(int err)
        {
            try
            {
                if (objBin.IsConnected == false)
                {
                    lblHardwareFailMessage.Text = "ALL HARDWARE ARE NOT CONNECTED";
                    lblHardwareFailMessage.Visible = true;
                    lblWeighingIP.BackColor = Color.Red;
                    iScannerDisconnctedd = 1;
                }
                else
                {
                    objBin.IPAddress = lblWeighingIP.Text;
                    objBin.Port = Convert.ToInt32(lblWeighingPort.Text);
                    if (!objBin.IsConnected)
                    {
                        objBin.Poll = true;
                        objBin.Open();
                    }
                    if (objBin.IsConnected)
                    {
                        objBin.ReConnect();
                    }
                    lblFMSMessge.Text = "CONNECTED";
                    lblFMSMessge.BackColor = Color.Green;
                    lblWeighingIP.BackColor = Color.Green;
                    lblHardwareFailMessage.Visible = false;
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
            }
        }
        private void ObjBin1_DataArrived(string ClientIP, string Message)
        {

            try
            {
                Regex reg = new Regex("[*'\",_&#^@]");
                Message = Regex.Replace(Message, @"[^\u0009\u000A\u000D\u0020-\u007E]", "").Trim();
                PCommon.mAppLog.WriteLog("ObjBin_OnRecieved : IP : " + ClientIP +
                   "Client DLL Called" + "  ::  Data :: " + Message + ", Length : " + Message.Length, DTPLLogsWrite.LogType.Data, MethodBase.GetCurrentMethod());
                if (Message.ToUpper().Contains("NETWORK") || Message.Trim().ToUpper().Contains("CONNECTED"))
                {

                }
                else if (Message.Trim().ToUpper().Contains("NOREAD"))
                {
                    lblFMSMessge.Text = "FAIL-NO READ";
                    lblFMSMessge.BackColor = Color.Yellow;
                    lblFMSMessge.ForeColor = Color.Black;
                    return;
                }
                else
                {
                    Message = reg.Replace(Message, string.Empty);
                    WEIGHT(Message);
                    //ProcessWeightData(Message);


                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
            }
        }


        #region Data through Serial Port
        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(500);
                int bufferSize = 2000;
                byte[] buffer = new byte[bufferSize];
                int bytesRead = serialPort1.Read(buffer, 0, bufferSize);
                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                try
                {
                    this.BeginInvoke(new SetTextDeleg(si_DataReceived), new object[] { receivedData });
                }
                catch (Exception ex)
                {
                    PCommon.mAppLog.WriteLog("issue in Received Data" + " " + ex.Message, DTPLLogsWrite.LogType.Information, MethodBase.GetCurrentMethod());
                    Console.WriteLine($"Error: {ex.Message}");
                }

            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog("issue in Received Data" + " " + ex.Message, DTPLLogsWrite.LogType.Information, MethodBase.GetCurrentMethod());
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        private void si_DataReceived(string Message)
        {
            try
            {
                if (Message.Trim().Length == 0)
                {
                    PCommon.mAppLog.WriteLog("No Weight Found" + " " + Message, DTPLLogsWrite.LogType.Data, MethodBase.GetCurrentMethod());
                    return;
                }
                PCommon.mAppLog.WriteLog("Weight Machine data" + " " + Message, DTPLLogsWrite.LogType.Information, MethodBase.GetCurrentMethod());
                Regex reg = new Regex("[*'\",_&#^@]");
                Message = Regex.Replace(Message, @"[^\u0009\u000A\u000D\u0020-\u007E]", "").Trim();
                if (Message.ToUpper().Contains("NETWORK") || Message.Trim().ToUpper().Contains("CONNECTED"))
                {

                }
                else if (Message.Trim().ToUpper().Contains("NOREAD"))
                {
                    lblFMSMessge.Text = "FAIL-NO READ";
                    lblFMSMessge.BackColor = Color.Yellow;
                    lblFMSMessge.ForeColor = Color.Black;
                    return;
                }
                else
                {
                    Message = reg.Replace(Message, string.Empty);
                    WEIGHT(Message);
                }

            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
            }

        }
        #endregion


        private delegate void DelWEIGHT(string sWeight);
        private void WEIGHT(string sBarcode)
        {
            while (panelPrint.Visible)
            {

                //System.Windows.Forms.Application.DoEvents();
                return;
            }

            //while (PnlLooseWeight.Visible)
            //{

            //    //System.Windows.Forms.Application.DoEvents();
            //    return;
            //}            

            try
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new SetTextDeleg(WEIGHT), new object[] { sBarcode });
                    Console.WriteLine(sBarcode);
                }
                else
                {
                    string[] lines = sBarcode.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    if (SResult1 == "True")
                    {
                        if (lines.Length == 1)
                        {
                            lines[0] = lblMcWeight.Text;
                            lblMcWeight.Text = sBarcode;
                            // Parsing the weights to numbers
                            if (!string.IsNullOrEmpty(txtTareWt.Text))
                            {
                                if (double.TryParse(lblMcWeight.Text, out double mcWeight) && double.TryParse(txtTareWt.Text, out double tareWeight))
                                {
                                    lblNETWeight.Text = (mcWeight - tareWeight).ToString("F3");
                                }

                                if (!string.IsNullOrEmpty(lblNETWeight.Text) && double.TryParse(lblNETWeight.Text, out double netWeight) && netWeight < 0)
                                {
                                    lblNETWeight.Text = "0";
                                    //lblFMSMessge.BackColor = Color.Red;
                                    return;
                                }

                            }
                            if (bTFWeight == false)
                            {
                                if (Convert.ToDecimal(lblNETWeight.Text) >= Convert.ToDecimal(txtMaxWT.Text)
                                               || Convert.ToDecimal(lblNETWeight.Text) <=
                                               Convert.ToDecimal(txtMinWt.Text)
                                               )
                                {
                                    ErrorLogModel objModel = new ErrorLogModel();
                                    objModel.Message = "Weight Failed On Print with Process Code " + txtProcssOrder.Text + ", Weighing Failed : Weight Found " + lblNETWeight.Text;
                                    objModel.ModuleName = "Label Printing";
                                    objModel.LogType = "Data";
                                    objModel.Severity = "2";
                                    objModel.BatchNo = txtBatchNo.Text;
                                    DL_UserLogin dL_UserLogin = new DL_UserLogin();
                                    dL_UserLogin.WriteErrorLog(objModel);
                                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(System.Windows.Forms.Application.StartupPath + "\\Error.wav");
                                    player.Play();
                                    bWeightCaptured = false;
                                    lblFMSMessge.Text = "Item Weight can not be exceed/less then Net weight,Weight is :" + lblNETWeight.Text;


                                    if (PnlLooseWeight.Visible == true)
                                    {
                                        panelPrint.Visible = false;
                                    }

                                    else
                                    {
                                        panelPrint.Visible = true;
                                    }
                                    // Panel ko visible karna


                                    // Clear karna text box
                                    RTxt.Text = string.Empty;

                                    // Background color change karna
                                    lblFMSMessge.BackColor = Color.Red;

                                    // Weight flag set karna
                                    bTFWeight = true;
                                    // Return statement
                                    return;
                                }

                            }
                            if (Convert.ToInt32(txtTargetShipper.Text) <= Convert.ToInt32(lblCount.Text))
                            {
                                lblFMSMessge.Text = "Reached the max limit of printing against the enter process order no";
                                lblMcWeight.Text = "0.00";
                                lblFMSMessge.BackColor = Color.Red;
                                return;
                            }
                            PL_CartonLabelPrinting plobj = new PL_CartonLabelPrinting();
                            DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                            plobj.ProcessOrder = txtProcssOrder.Text.Trim();
                            plobj.BatchNo = txtBatchNo.Text;
                            plobj.Condition = cmbCondition.Text;
                            plobj.TargetShipper = Convert.ToInt32(txtTargetShipper.Text);
                            plobj.Licence = cmbLicence.Text;
                            plobj.Quantity = txtQuantity.Text;
                            plobj.GrossWeight = Decimal.Parse(lblMcWeight.Text);
                            plobj.Remarks = txtRemarks.Text;
                            plobj.Mrp = txtmrp.Text;
                            plobj.TargetShipper = Convert.ToInt32(txtTargetShipper.Text);
                            plobj.Medicine = Mdicine.Text;
                            plobj.SupervisorField = SuperviserCMb.Text;
                            plobj.MediceCheck = MediceCheck.Text;

                            if (checkBoxMedicine.Checked == true)
                            {
                                plobj.checkBoxMedicine = "1";
                            }

                            if (rdbtnBestBefore.Checked == true)
                            {
                                plobj.PrintedExpiryLabelFormat = "Best Before";
                            }
                            else if (rdbtnUseBefore.Checked == true)
                            {
                                plobj.PrintedExpiryLabelFormat = "Use Before";
                            }
                            else if (rdbtnExpDate.Checked == true)
                            {
                                plobj.PrintedExpiryLabelFormat = "Exp Date";
                            }
                            plobj.isNotForSale = "0";
                            plobj.isNrx = "0";
                            plobj.isLl = "0";
                            plobj.isR = "0";
                            plobj.isTm = "0";
                            plobj.isSubsidary = "0";
                            plobj.isPrintReqiured = "0";

                            if (isLLcheckbox.Checked == true)
                            {
                                plobj.isLl = "1";
                            }

                            if (chkTM1.Checked == true)
                            {
                                plobj.isTm = "1";
                            }

                            if (chkR1.Checked == true)
                            {
                                plobj.isR = "1";
                            }
                            if (chkSubsidiary.Checked == true)
                            {
                                plobj.isSubsidary = "1";
                            }
                            if (rdbtnMonthInChar.Checked == true)
                            {
                                plobj.MonthInChar = "1";
                            }
                            if (chkPrintRequired.Checked == true)
                            {
                                plobj.isPrintReqiured = "1";
                            }
                            if (rdbtnYearInLastTwoDigit.Checked == true)
                            {
                                plobj.YearInTwoDigit = "1";
                            }
                            if (rbtnCHAR.Checked == true)
                            {
                                plobj.isPrintReqiured = "1";
                            }

                            if (RbdDDMMMYYYY.Checked == true)
                            {
                                plobj.YearFourDigit = "1";
                            }


                            plobj.MaxWt = Convert.ToDecimal(txtMaxWT.Text);
                            plobj.MinWt = Convert.ToDecimal(txtMinWt.Text);
                            plobj.CustomerCode = cmbCustomer.SelectedValue.ToString();
                            plobj.TareWt = Convert.ToDecimal(txtTareWt.Text);
                            plobj.NetWeight = Decimal.Parse(lblNETWeight.Text);
                            plobj.PrinterIP = lblPrinterIP.Text;
                            plobj.ScaleIP = lblWeighingIP.Text;
                            if (CmdCountry.Text != "--Select--")
                            {
                                plobj.Country = CmdCountry.Text;
                            }



                            // change by pushpak for printer ping break condition
                            if (HardwareTesting(lblPrinterIP.Text) == false)
                            {
                                lblFMSMessge.BackColor = Color.Red;
                                lblFMSMessge.Text = "Printing IP Not Connected, Please try again";
                                return;
                            }
                            if (!string.IsNullOrWhiteSpace(RTxt.Text))
                            {
                                plobj.RejectionRemarks = RTxt.Text;
                            }
                            else if (!string.IsNullOrWhiteSpace(Remrk.Text))
                            {
                                plobj.RejectionRemarks = Remrk.Text;
                            }
                            plobj.Customer = txtCustomerName.Text;
                            plobj.ProductName = txtGenricField.Text;
                            plobj.GenericField = txtGenricName.Text;
                            if (CmbCustLisce.Text != "--Select--")
                            {
                                plobj.CustomerLicence = CmbCustLisce.Text;
                            }

                            while (pnlReprintConfirmation.Visible)
                            {
                                return;
                            }

                            while (PnlLooseWeight.Visible)
                            {

                                //System.Windows.Forms.Application.DoEvents();
                                return;
                            }
                            DataTable dt = obj.SavePrintingData(plobj);
                            if (dt.Rows.Count > 0)
                            {
                                string sResult = dt.Rows[0][0].ToString();
                                if (sResult.StartsWith("SUCCESS~"))
                                {

                                    lblFMSMessge.BackColor = Color.Green;
                                    lblFMSMessge.Text = "SUCCESS";
                                    string sSerialNo = sResult.Split('~')[2].ToString();
                                    plobj.ShipperSerial = sSerialNo;

                                    ErrorLogModel objModel = new ErrorLogModel();
                                    objModel.Message = "Label successfully printed with Process Code " + txtProcssOrder.Text + ", Weighing Success : Weight Found " + lblMcWeight.Text + ", Serial No :" + sSerialNo;
                                    objModel.ModuleName = "Label Printing";
                                    objModel.LogType = "Data";
                                    objModel.Severity = "2";
                                    objModel.BatchNo = txtBatchNo.Text;
                                    DL_UserLogin dL_UserLogin = new DL_UserLogin();
                                    dL_UserLogin.WriteErrorLog(objModel);
                                    PCommon.mAppLog.WriteLog("Print Label Method Called For Barcode :" + plobj.ShipperSerial, DTPLLogsWrite.LogType.Data, MethodBase.GetCurrentMethod());
                                    PrintLabel(plobj, "OK");
                                    bTFWeight = false;
                                }
                                else
                                {
                                    bTFWeight = false;
                                    PCommon.mAppLog.WriteLog("Failed Print Label Method Called For Barcode :" + sResult, DTPLLogsWrite.LogType.Data, MethodBase.GetCurrentMethod());
                                    lblFMSMessge.BackColor = Color.Red;
                                    lblFMSMessge.Text = sResult;
                                }

                            }
                            else
                            {
                                bTFWeight = false;
                                PCommon.mAppLog.WriteLog("No Result Found from database :" + txtBatchNo.Text, DTPLLogsWrite.LogType.Data, MethodBase.GetCurrentMethod());

                            }

                            objBin.ReConnect();
                            ObjBin_OnConnect(0);

                        }

                    }
                    else if (SResult2 == "True")
                    {

                        lblMcWeight.Text = lines[0].ToString().Replace("kg", "").Replace("G", "").Replace(" ", "");


                        if (lines.Length > 1 && !string.IsNullOrWhiteSpace(lines[1]))
                        {
                            txtTareWt.Text = lines[1].ToString().Replace("kg", "").Replace("T", "").Replace(" ", "");
                        }

                        if (!string.IsNullOrEmpty(txtTareWt.Text))
                        {
                            if (double.TryParse(lblMcWeight.Text, out double mcWeight) && double.TryParse(txtTareWt.Text, out double tareWeight))
                            {
                                lblNETWeight.Text = (mcWeight - tareWeight).ToString("F3");
                            }

                            if (!string.IsNullOrEmpty(lblNETWeight.Text) && double.TryParse(lblNETWeight.Text, out double netWeight) && netWeight < 0)
                            {
                                lblNETWeight.Text = "0";
                                //lblFMSMessge.BackColor = Color.Red;
                                return;
                            }
                            else
                            {
                            }
                        }
                        if (bTFWeight == false)
                        {
                            if (Convert.ToDecimal(lblNETWeight.Text) >= Convert.ToDecimal(txtMaxWT.Text)
                                           || Convert.ToDecimal(lblNETWeight.Text) <=
                                           Convert.ToDecimal(txtMinWt.Text)
                                           )
                            {
                                ErrorLogModel objModel = new ErrorLogModel();
                                objModel.Message = "Weight Failed On Print with Process Code " + txtProcssOrder.Text + ", Weighing Failed : Weight Found " + lblNETWeight.Text;
                                objModel.ModuleName = "Label Printing";
                                objModel.LogType = "Data";
                                objModel.Severity = "2";
                                objModel.BatchNo = txtBatchNo.Text;
                                DL_UserLogin dL_UserLogin = new DL_UserLogin();
                                dL_UserLogin.WriteErrorLog(objModel);
                                System.Media.SoundPlayer player = new System.Media.SoundPlayer(System.Windows.Forms.Application.StartupPath + "\\Error.wav");
                                player.Play();
                                bWeightCaptured = false;
                                lblFMSMessge.Text = "Item Weight can not be exceed/less then Net weight,Weight is :" + lblNETWeight.Text;


                                if (PnlLooseWeight.Visible == true)
                                {
                                    panelPrint.Visible = false;
                                }

                                else
                                {
                                    panelPrint.Visible = true;
                                }

                                // Clear karna text box
                                RTxt.Text = string.Empty;

                                // Background color change karna
                                lblFMSMessge.BackColor = Color.Red;

                                // Weight flag set karna
                                bTFWeight = true;
                                // Return statement
                                return;
                            }

                        }
                        if (Convert.ToInt32(txtTargetShipper.Text) <= Convert.ToInt32(lblCount.Text))
                        {
                            lblFMSMessge.Text = "Reached the max limit of printing against the enter process order no";
                            lblMcWeight.Text = "0.00";
                            lblFMSMessge.BackColor = Color.Red;
                            return;
                        }
                        PL_CartonLabelPrinting plobj = new PL_CartonLabelPrinting();
                        DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                        plobj.ProcessOrder = txtProcssOrder.Text.Trim();
                        plobj.BatchNo = txtBatchNo.Text;
                        plobj.Condition = cmbCondition.Text;
                        plobj.TargetShipper = Convert.ToInt32(txtTargetShipper.Text);
                        plobj.Licence = cmbLicence.Text;
                        plobj.Quantity = txtQuantity.Text;
                        plobj.GrossWeight = Decimal.Parse(lblMcWeight.Text);
                        plobj.Remarks = txtRemarks.Text;
                        plobj.Mrp = txtmrp.Text;
                        plobj.TargetShipper = Convert.ToInt32(txtTargetShipper.Text);
                        if (rdbtnBestBefore.Checked == true)
                        {
                            plobj.PrintedExpiryLabelFormat = "Best Before";
                        }
                        else if (rdbtnUseBefore.Checked == true)
                        {
                            plobj.PrintedExpiryLabelFormat = "Use Before";
                        }
                        else if (rdbtnExpDate.Checked == true)
                        {
                            plobj.PrintedExpiryLabelFormat = "Exp Date";
                        }
                        plobj.isNotForSale = "0";
                        plobj.isNrx = "0";
                        plobj.isLl = "0";
                        plobj.isR = "0";
                        plobj.isTm = "0";
                        plobj.isSubsidary = "0";
                        plobj.isPrintReqiured = "0";

                        if (isLLcheckbox.Checked == true)
                        {
                            plobj.isLl = "1";
                        }

                        if (chkTM1.Checked == true)
                        {
                            plobj.isTm = "1";
                        }

                        if (chkR1.Checked == true)
                        {
                            plobj.isR = "1";
                        }
                        if (chkSubsidiary.Checked == true)
                        {
                            plobj.isSubsidary = "1";
                        }
                        if (rdbtnMonthInChar.Checked == true)
                        {
                            plobj.MonthInChar = "1";
                        }
                        if (chkPrintRequired.Checked == true)
                        {
                            plobj.isPrintReqiured = "1";
                        }
                        if (rdbtnYearInLastTwoDigit.Checked == true)
                        {
                            plobj.YearInTwoDigit = "1";
                        }
                        if (rbtnCHAR.Checked == true)
                        {
                            plobj.isPrintReqiured = "1";
                        }

                        if (checkBoxMedicine.Checked == true)
                        {
                            plobj.checkBoxMedicine = "1";
                        }


                        plobj.MaxWt = Convert.ToDecimal(txtMaxWT.Text);
                        plobj.MinWt = Convert.ToDecimal(txtMinWt.Text);
                        plobj.CustomerCode = cmbCustomer.SelectedValue.ToString();
                        plobj.TareWt = Convert.ToDecimal(txtTareWt.Text);
                        plobj.NetWeight = Decimal.Parse(lblNETWeight.Text);

                        // change by pushpak for printer ping break condition
                        if (HardwareTesting(lblPrinterIP.Text) == false)
                        {
                            lblFMSMessge.BackColor = Color.Red;
                            lblFMSMessge.Text = "Printing IP Not Connected, Please try again";
                            return;
                        }
                        if (!string.IsNullOrWhiteSpace(RTxt.Text))
                        {
                            plobj.RejectionRemarks = RTxt.Text;
                        }
                        else if (!string.IsNullOrWhiteSpace(Remrk.Text))
                        {
                            plobj.RejectionRemarks = Remrk.Text;
                        }
                        plobj.Customer = txtCustomerName.Text;
                        plobj.ProductName = txtGenricField.Text;
                        plobj.MaxWt = Convert.ToDecimal(txtMaxWT.Text);

                        plobj.Medicine = Mdicine.Text;
                        plobj.SupervisorField = SuperviserCMb.Text;
                        plobj.MediceCheck = MediceCheck.Text;
                        plobj.PrinterIP = lblPrinterIP.Text;
                        plobj.ScaleIP = lblWeighingIP.Text;
                        plobj.GenericField = txtGenricName.Text;
                        if (CmdCountry.Text != "--Select--")
                        {
                            plobj.Country = CmdCountry.Text;
                        }
                        if (CmbCustLisce.Text != "--Select--")
                        {
                            plobj.CustomerLicence = CmbCustLisce.Text;
                        }



                        while (pnlReprintConfirmation.Visible)
                        {
                            return;
                        }

                        while (PnlLooseWeight.Visible)
                        {

                          
                            return;
                        }
                        DataTable dt = obj.SavePrintingData(plobj);
                        if (dt.Rows.Count > 0)
                        {
                            string sResult = dt.Rows[0][0].ToString();
                            if (sResult.StartsWith("SUCCESS~"))
                            {
                                lblFMSMessge.BackColor = Color.Green;
                                lblFMSMessge.Text = "SUCCESS";
                                string sSerialNo = sResult.Split('~')[2].ToString();
                                plobj.ShipperSerial = sSerialNo;

                                ErrorLogModel objModel = new ErrorLogModel();
                                objModel.Message = "Label successfully printed with Process Code " + txtProcssOrder.Text + ", Weighing Success : Weight Found " + lblMcWeight.Text + ", Serial No :" + sSerialNo;
                                objModel.ModuleName = "Label Printing";
                                objModel.LogType = "Data";
                                objModel.Severity = "2";
                                objModel.BatchNo = txtBatchNo.Text;
                                DL_UserLogin dL_UserLogin = new DL_UserLogin();
                                dL_UserLogin.WriteErrorLog(objModel);
                                PCommon.mAppLog.WriteLog("Print Label Method Called For Barcode :" + plobj.ShipperSerial, DTPLLogsWrite.LogType.Data, MethodBase.GetCurrentMethod());
                                PrintLabel(plobj, "OK");
                                bTFWeight = false;
                            }
                            else
                            {
                                bTFWeight = false;
                                PCommon.mAppLog.WriteLog("Failed Print Label Method Called For Barcode :" + sResult, DTPLLogsWrite.LogType.Data, MethodBase.GetCurrentMethod());
                                lblFMSMessge.BackColor = Color.Red;
                                lblFMSMessge.Text = sResult;
                            }

                        }
                        else
                        {
                            bTFWeight = false;
                            PCommon.mAppLog.WriteLog("No Result Found from database :" + txtBatchNo.Text, DTPLLogsWrite.LogType.Data, MethodBase.GetCurrentMethod());

                        }

                        objBin.ReConnect();
                        ObjBin_OnConnect(0);


                    }

                    else
                    {
                        lblFMSMessge.BackColor = Color.Red;
                        lblFMSMessge.Text = "please seclect manual tare or  auto tare weight first";
                        return;
                    }


                }
            }
            catch (Exception ex)
            {
                bTFWeight = false;
                MessageBox.Show("An error occurred: " + ex.Message);
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                ErrorLogModel objModel = new ErrorLogModel();
                objModel.Message = "Error while getting the weight data on Process Order" + txtProcssOrder.Text + " and Error : " + ex.Message;
                objModel.ModuleName = "Label Printing";
                objModel.LogType = "Error";
                objModel.Severity = "1";
                objModel.BatchNo = txtBatchNo.Text;
                DL_UserLogin dL_UserLogin = new DL_UserLogin();
                dL_UserLogin.WriteErrorLog(objModel);
            }
        }

        public void PrintLabel(PL_CartonLabelPrinting request, string sLabelType)
        {


            try
            {
                StreamReader FSOP;
                string FileTextLine = string.Empty;
                string TempFileTextLine;
                if (File.Exists((System.Windows.Forms.Application.StartupPath + "\\LabelDesign" + "\\" + PCommon.sPrnName)))
                {
                    FSOP = new StreamReader((System.Windows.Forms.Application.StartupPath + "\\LabelDesign" + "\\" + PCommon.sPrnName));
                    FileTextLine = FSOP.ReadToEnd();
                    FSOP.Close();
                }
                else
                {
                    lblFMSMessge.BackColor = Color.Red;
                    lblFMSMessge.Text = "Prn Not Found For printing";
                    return;
                }
                DL_CartonLabelPrinting objCartonPrinting = new DL_CartonLabelPrinting();
                DataTable dtShipperData = objCartonPrinting.GetPrintingData(request.ShipperSerial);
                if (dtShipperData.Rows.Count > 0)
                {
                    Medicine = Mdicine.Text;
                    GenericName = txtGenricName.Text;
                    TempFileTextLine = FileTextLine;

                    if (checkBoxMedicine.Checked == true)
                    {
                        TempFileTextLine = TempFileTextLine.Replace("#VarSampleCondition#", MediceCheck.Text);
                    }

                    TempFileTextLine = TempFileTextLine.Replace("#Product type#", Mdicine.Text);

                    if(CmbCustLisce.Text == "--Select--")
                    {
                        TempFileTextLine = TempFileTextLine.Replace("#cusLiceNo#", "");
                        TempFileTextLine = TempFileTextLine.Replace("cus.Lic.No", "");
                        
                    }

                    TempFileTextLine = TempFileTextLine.Replace("#cusLiceNo#", CmbCustLisce.Text);

                    TempFileTextLine = TempFileTextLine.Replace("#SFGCODE#", txtProductCode.Text);

                    if (chkTM1.Checked == true)
                    {
                        TempFileTextLine = TempFileTextLine.Replace("#VarProductValue1#", "");
                        TempFileTextLine = TempFileTextLine.Replace("#VarProductValue0#", txtGenricField.Text + "_99");
                    }


                    else if (chkR1.Checked == true)
                    {
                        TempFileTextLine = TempFileTextLine.Replace("#VarProductValue0#", "");
                        TempFileTextLine = TempFileTextLine.Replace("#VarProductValue1#", txtGenricField.Text + "_A9");
                    }
                    else
                    {
                        TempFileTextLine = TempFileTextLine.Replace("#VarProductValue0#", "");
                        TempFileTextLine = TempFileTextLine.Replace("#VarProductValue1#", txtGenricField.Text);
                    }

                    if(CmdCountry.Text == "--Select--")
                    {
                        TempFileTextLine = TempFileTextLine.Replace("Country", "");
                        TempFileTextLine = TempFileTextLine.Replace("#Country#", "");
                    }

                   
                    else {
                        TempFileTextLine = TempFileTextLine.Replace("#Country#", CmdCountry.Text);
                    }

                        TempFileTextLine = TempFileTextLine.Replace("#GenricField#", GenericName);
                    TempFileTextLine = TempFileTextLine.Replace("#VarBatchNo#", dtShipperData.Rows[0]["BatchNo"].ToString());
                    string expDate = string.Empty;
                    string MFGDate = string.Empty;
                    DateTime dtTime = Convert.ToDateTime(dtShipperData.Rows[0]["PrintedExpDate"].ToString());
                    DateTime dtMFGTime = Convert.ToDateTime(dtShipperData.Rows[0]["PrintedMFGDate"].ToString());
                    // CHANGE BY PUSHPAK COMPLETE YEAR
                    if (rdbtnMonthInnum.Checked == true)
                    {
                        expDate = dtTime.ToString("MM") + "/" + dtTime.ToString("yyyy");
                        MFGDate = dtMFGTime.ToString("MM") + "/" + dtMFGTime.ToString("yyyy");
                    }

                    if (RbdDDMMMYYYY.Checked == true)
                    {
                        if (RbdDDMMMYYYY.Checked)
                        {
                            expDate = dtTime.ToString("dd/MMM/yyyy");
                            MFGDate = dtMFGTime.ToString("dd/MMM/yyyy");
                        }
                    }

                    if (rdbtnMonthInChar.Checked == true)
                    {
                        string month = dtTime.ToString("MMM");
                        if (month.ToLower().Equals("May".ToLower()))
                        {
                            expDate = month.ToUpper() + " " + dtTime.ToString("yyyy");
                        }
                        else
                        {
                            expDate = month.ToUpper() + "." + dtTime.ToString("yyyy");
                        }
                        string monthmfg = dtMFGTime.ToString("MMM");
                        if (monthmfg.ToLower().Equals("May".ToLower()))
                        {
                            MFGDate = monthmfg.ToUpper() + " " + dtMFGTime.ToString("yyyy");
                        }
                        else
                        {
                            MFGDate = monthmfg.ToUpper() + "." + dtMFGTime.ToString("yyyy");
                        }
                    }
                    // change by pushpak year in two digit and month in char
                    if (rbtnCHAR.Checked == true)
                    {
                        string month = dtTime.ToString("MMM");
                        if (month.ToLower().Equals("May".ToLower()))
                        {
                            expDate = month.ToUpper() + " " + dtTime.ToString("yy");
                        }
                        else
                        {
                            expDate = month.ToUpper() + "." + dtTime.ToString("yy");
                        }
                        string monthmfg = dtMFGTime.ToString("MMM");
                        if (monthmfg.ToLower().Equals("May".ToLower()))
                        {
                            MFGDate = monthmfg.ToUpper() + " " + dtMFGTime.ToString("yy");
                        }
                        else
                        {
                            MFGDate = monthmfg.ToUpper() + "." + dtMFGTime.ToString("yy");
                        }
                    }
                    if (expDate.Length == 0)
                    {
                        expDate = dtTime.ToString("MM") + "/" + dtTime.ToString("yy");
                    }

                    if (MFGDate.Length == 0)
                    {
                        MFGDate = dtMFGTime.ToString("MM") + "/" + dtMFGTime.ToString("yy");
                    }
                    string sBarcode = string.Empty;




                    if (sLabelType == "TEST")
                    {
                        sBarcode = "TEST";
                    }
                    else
                    {
                        // change for special chaactor
                        string rawBarcode = txtProductCode.Text + "," + txtGenricField.Text + "," +
                    dtShipperData.Rows[0]["BatchNo"].ToString() + "," +
                    MFGDate + "," + expDate + "," +
                    dtShipperData.Rows[0]["ShipperSerial"].ToString();

                        sBarcode = rawBarcode.Replace(" ", "")
                                             .Replace("\t", "")
                                             .Replace("\r", "")
                                             .Replace("\n", "");
                        //sBarcode = txtProductCode.Text + "," + txtGenricField.Text + "," + dtShipperData.Rows[0]["BatchNo"].ToString() + "," + MFGDate + "," + expDate + "," + dtShipperData.Rows[0]["ShipperSerial"].ToString();
                    }

                    TempFileTextLine = TempFileTextLine.Replace("#VarStorageConditionValue0#", cmbCondition.Text);
                    TempFileTextLine = TempFileTextLine.Replace("#VarExpiryDate#", expDate);
                    TempFileTextLine = TempFileTextLine.Replace("#VarMfgDate#", MFGDate);
                    TempFileTextLine = TempFileTextLine.Replace("#VarMfgLicNo#", dtShipperData.Rows[0]["licence"].ToString());
                    TempFileTextLine = TempFileTextLine.Replace("#VarGrossWeight#", dtShipperData.Rows[0]["GrossWeight"].ToString() + " kg.");
                    TempFileTextLine = TempFileTextLine.Replace("#VarQuantity#", dtShipperData.Rows[0]["Quantity"].ToString());
                    TempFileTextLine = TempFileTextLine.Replace("#Varboxno#", dtShipperData.Rows[0]["SeqNo"].ToString());
                    TempFileTextLine = TempFileTextLine.Replace("#VarBarcode#", sBarcode);
                    if (SuperviserCMb.Text == "<NO DATA>")
                    {
                        TempFileTextLine = TempFileTextLine.Replace("Supervisor Name", "");
                    }

                    if (SuperviserCMb.Text == "Supervisor Code : <BLANK>")
                    {
                        TempFileTextLine = TempFileTextLine.Replace("Supervisor Name:", "Supervisor Code:");
                    }

                    if (SuperviserCMb.Text == "Supervisor Name : <BLANK>")
                    {
                        TempFileTextLine = TempFileTextLine.Replace("Supervisor Name:", "Supervisor Name:");
                    }

                    else
                    {

                        TempFileTextLine = TempFileTextLine.Replace("Supervisor Name:", SuperviserCMb.Text);
                    }

                      
                    

                    TempFileTextLine = TempFileTextLine.Replace("#VarMRP#", dtShipperData.Rows[0]["MRP"].ToString());


                    //Customer Fields
                    TempFileTextLine = TempFileTextLine.Replace("#VarMktCustomerName#", txtCustomerName.Text);
                    string sCustomerProfiele = string.Empty;
                    sCustomerProfiele = txtAddress1.Text + " " + txtAddress2.Text + " " + txtAddress3.Text + ", " + txtCity.Text + " " + txtPostalCode.Text;
                    TempFileTextLine = TempFileTextLine.Replace("#VarMktAddress0#", sCustomerProfiele);

                    //Manufacturer Fields
                    TempFileTextLine = TempFileTextLine.Replace("#VarManufacturedBy#", sPlantname);

                    ArrayList manufacturerFieldsList = new ArrayList();
                    if (chkSubsidiary.Checked == true)
                    {
                        string subsidiaryText = "(A Subsidiary of Akums Drugs and Pharmaceuticals Ltd.)";
                        manufacturerFieldsList.Add(subsidiaryText);
                    }

                    if (isLLcheckbox.Checked == true)
                    {
                        manufacturerFieldsList.Add("At: " + sPlantAddress1);
                    }
                    else
                    {
                        manufacturerFieldsList.Add(sPlantAddress1);
                    }
                    manufacturerFieldsList.Add(sPlantAddress2);
                    manufacturerFieldsList.Add(sPlantAddress3);
                    string ManufactureAddress = string.Empty;
                    for (int i = 0; i < manufacturerFieldsList.Count; i++)
                    {
                        ManufactureAddress = ManufactureAddress + manufacturerFieldsList[i].ToString();
                    }
                    TempFileTextLine = TempFileTextLine.Replace("#VarAddress0#", ManufactureAddress);
                    TempFileTextLine = TempFileTextLine.Replace("#VarFormatNo#", "HPK-034/F02-00");

                    string sExpDateLabel = string.Empty;
                    string sConditionLabel = string.Empty;
                    if (rdbtnBestBefore.Checked == true)
                    {
                        sExpDateLabel = "Best Before:";
                    }
                    else if (rdbtnUseBefore.Checked == true)
                    {
                        sExpDateLabel = "Use Before:";
                    }
                    else if (rdbtnExpDate.Checked == true)
                    {
                        sExpDateLabel = "Expiry Date:";
                    }
                    // change by pushpak scondition value change


                    if (chkSample.Checked == true)
                    {
                        sConditionLabel = "PHYSICIAN'S SAMPLE";
                    }
                    if (chkNotForSale.Checked == true)
                    {
                        if (sConditionLabel != "")
                        {
                            sConditionLabel += " ";
                        }
                        sConditionLabel += "NOT TO BE SOLD";
                    }
                    if (chkNRxProduct.Checked == true)
                    {
                        if (sConditionLabel != "")
                        {
                            sConditionLabel += " ";
                        }
                        sConditionLabel += "NRx Product";
                    }
                    TempFileTextLine = TempFileTextLine.Replace("#VarSampleCondition#", sConditionLabel);


                    TempFileTextLine = TempFileTextLine.Replace("#VarExpiryDateLabel#", sExpDateLabel);

                    if (chkPrintRequired.Checked == true)
                    {
                        PCommon.mAppLog.WriteLog("Prn Send :" + TempFileTextLine, DTPLLogsWrite.LogType.Data, MethodBase.GetCurrentMethod());
                        blCommon.SendCommandToPrinter(lblPrinterIP.Text, Convert.ToInt32(lblPrinterPort.Text), TempFileTextLine);
                    }
                    else
                    {
                        PCommon.mAppLog.WriteLog("Prn Created but not sent due to checkbox :" + TempFileTextLine, DTPLLogsWrite.LogType.Data, MethodBase.GetCurrentMethod());
                    }

                    objBin.ReConnect();
                    ObjBin_OnConnect(0);

                }
                else
                {
                    PCommon.mAppLog.WriteLog("No data found for printing For Barcode :" + request.ShipperSerial, DTPLLogsWrite.LogType.Data, MethodBase.GetCurrentMethod());

                }
                bWeightCaptured = false;
                lblFMSMessge.Text = "SUCCESS";
                lblFMSMessge.BackColor = Color.Green;
                lblCount.Text = Convert.ToString(Convert.ToInt32(lblCount.Text) + 1);
                GetBatchPrintingData();
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                lblFMSMessge.Text = ex.Message;
                lblFMSMessge.BackColor = Color.Red;
            }
        }


        // Change by sanchit by creating new method with the same name and change the running method by adding Old
        public void PrintLabelOld(PL_CartonLabelPrinting request, string sLabelType)
        {
            try
            {
                StreamReader FSOP;
                string FileTextLine = string.Empty;
                string TempFileTextLine;
                if (File.Exists((System.Windows.Forms.Application.StartupPath + "\\LabelDesign" + "\\" + PCommon.sPrnName)))
                {
                    FSOP = new StreamReader((System.Windows.Forms.Application.StartupPath + "\\LabelDesign" + "\\" + PCommon.sPrnName));
                    FileTextLine = FSOP.ReadToEnd();
                    FSOP.Close();
                }
                else
                {
                    lblFMSMessge.BackColor = Color.Red;
                    lblFMSMessge.Text = "Prn Not Found For printing";
                    return;
                }
                DL_CartonLabelPrinting objCartonPrinting = new DL_CartonLabelPrinting();
                DataTable dtShipperData = objCartonPrinting.GetPrintingData(request.ShipperSerial);
                if (dtShipperData.Rows.Count > 0)
                {


                    TempFileTextLine = FileTextLine;
                    TempFileTextLine = TempFileTextLine.Replace("#VarMedicine#", "Handle With Care");
                    //Product Secti1on If size of Product Desc Inc.

                    // Get the product description
                    string productDescription = txtGenricField.Text; //dtShipperData.Rows[0]["ProductDescription"].ToString();

                    // Initialize line variables
                    string ProductLine1 = string.Empty;
                    string ProductLine2 = string.Empty;

                    // Change by sanchit on 21 Jun 2022 make product desc in multi line             //Commented By dashmesh       
                    //DataTable dtNew = dtNewLine(productDescription,20);

                    //if (dtNew.Rows.Count > 0)
                    //{
                    //    for (int i = 0; i < dtNew.Rows.Count; i++)
                    //    {
                    //        TempFileTextLine = TempFileTextLine.Replace("#VarProductValue" + i + "#", dtNew.Rows[i][0].ToString());
                    //    }
                    //}
                    //PCommon.mAppLog.WriteLog("Data Output  : " + dtNew.Rows.Count, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());


                    ArrayList productNameSplitParts = splitStringInSpaces(productDescription, 25);
                    for (int i = 0; i < productNameSplitParts.Count; i++)
                    {
                        string thisline = productNameSplitParts[i].ToString();

                        if (i == productNameSplitParts.Count - 1)
                        {
                            // Last Line 
                            if (chkTM1.Checked)
                            {
                                thisline = thisline + "\u2122";

                            }
                            else if (chkR1.Checked)
                            {
                                thisline = thisline + "\u00AE";

                            }
                        }

                        TempFileTextLine = TempFileTextLine.Replace("#VarProductValue" + i + "#", thisline);
                    }


                    //to replace leftover string variables with empty.
                    TempFileTextLine = TempFileTextLine.Replace("#VarProductValue0#", "");
                    TempFileTextLine = TempFileTextLine.Replace("#VarProductValue1#", "");
                    TempFileTextLine = TempFileTextLine.Replace("#VarProductValue2#", "");

                    //Product Desc Section End here

                    TempFileTextLine = TempFileTextLine.Replace("#VarBatchNo#", dtShipperData.Rows[0]["BatchNo"].ToString());

                    string expDate = string.Empty;
                    string MFGDate = string.Empty;
                    DateTime dtTime = Convert.ToDateTime(dtShipperData.Rows[0]["PrintedExpDate"].ToString());
                    DateTime dtMFGTime = Convert.ToDateTime(dtShipperData.Rows[0]["PrintedMFGDate"].ToString());
                    // CHANGE BY PUSHPAK COMPLETE YEAR
                    if (rdbtnMonthInnum.Checked == true)
                    {
                        expDate = dtTime.ToString("MM") + "/" + dtTime.ToString("yyyy");

                        MFGDate = dtMFGTime.ToString("MM") + "/" + dtMFGTime.ToString("yyyy");

                    }
                    if (rdbtnMonthInChar.Checked == true)
                    {
                        string month = dtTime.ToString("MMM");
                        if (month.ToLower().Equals("May".ToLower()))
                        {
                            expDate = month.ToUpper() + " " + dtTime.ToString("yyyy");
                        }
                        else
                        {
                            expDate = month.ToUpper() + "." + dtTime.ToString("yyyy");
                        }


                        string monthmfg = dtMFGTime.ToString("MMM");
                        if (monthmfg.ToLower().Equals("May".ToLower()))
                        {
                            MFGDate = monthmfg.ToUpper() + " " + dtMFGTime.ToString("yyyy");
                        }
                        else
                        {
                            MFGDate = monthmfg.ToUpper() + "." + dtMFGTime.ToString("yyyy");
                        }
                    }

                    // change by pushpak year in two digit and month in char

                    if (rbtnCHAR.Checked == true)
                    {
                        string month = dtTime.ToString("MMM");
                        if (month.ToLower().Equals("May".ToLower()))
                        {
                            expDate = month.ToUpper() + " " + dtTime.ToString("yy");
                        }
                        else
                        {
                            expDate = month.ToUpper() + "." + dtTime.ToString("yy");
                        }


                        string monthmfg = dtMFGTime.ToString("MMM");
                        if (monthmfg.ToLower().Equals("May".ToLower()))
                        {
                            MFGDate = monthmfg.ToUpper() + " " + dtMFGTime.ToString("yy");
                        }
                        else
                        {
                            MFGDate = monthmfg.ToUpper() + "." + dtMFGTime.ToString("yy");
                        }
                    }

                    if (expDate.Length == 0)
                    {
                        expDate = dtTime.ToString("MM") + "/" + dtTime.ToString("yy");
                    }

                    if (MFGDate.Length == 0)
                    {
                        MFGDate = dtMFGTime.ToString("MM") + "/" + dtMFGTime.ToString("yy");
                    }


                    string sBarcode = string.Empty;
                    if (sLabelType == "TEST")
                    {
                        sBarcode = "TEST";
                    }
                    else
                    {
                        //sBarcode = dtShipperData.Rows[0]["ProductCode"].ToString() + "," + dtShipperData.Rows[0]["ProductDescription"].ToString() + "," + dtShipperData.Rows[0]["BatchNo"].ToString() + "," + MFGDate + "," + expDate + "," + dtShipperData.Rows[0]["ShipperSerial"].ToString();
                        sBarcode = txtProductCode.Text + "," + txtGenricName.Text + "," + dtShipperData.Rows[0]["BatchNo"].ToString() + "," + MFGDate + "," + expDate + "," + dtShipperData.Rows[0]["ShipperSerial"].ToString();
                    }


                    //Added by dashmesh 240614 - storage condition line break.
                    string conditionline1 = "";
                    string conditionline2 = "";

                    string[] lines = cmbCondition.Text.Split(new string[] { "\r\n", }, StringSplitOptions.None);

                    if (lines.Length >= 2)
                    {
                        conditionline1 = lines[0];
                        conditionline2 = lines[1];
                    }
                    else
                    {
                        conditionline1 = lines[0];
                        conditionline2 = " ";
                    }


                    TempFileTextLine = TempFileTextLine.Replace("#VarStorageConditionValue0#", conditionline1);
                    TempFileTextLine = TempFileTextLine.Replace("#VarStorageConditionValue1#", conditionline2);



                    TempFileTextLine = TempFileTextLine.Replace("#VarExpiryDate#", expDate);
                    TempFileTextLine = TempFileTextLine.Replace("#VarMfgDate#", MFGDate);
                    TempFileTextLine = TempFileTextLine.Replace("#VarMfgLicNo#", dtShipperData.Rows[0]["licence"].ToString());
                    TempFileTextLine = TempFileTextLine.Replace("#VarGrossWeight#", dtShipperData.Rows[0]["GrossWeight"].ToString() + " kg.");
                    TempFileTextLine = TempFileTextLine.Replace("#VarQuantity#", dtShipperData.Rows[0]["Quantity"].ToString());
                    TempFileTextLine = TempFileTextLine.Replace("#Varboxno#", dtShipperData.Rows[0]["SeqNo"].ToString());
                    TempFileTextLine = TempFileTextLine.Replace("#VarBarcode#", sBarcode);
                    TempFileTextLine = TempFileTextLine.Replace("#VarMRP#", dtShipperData.Rows[0]["MRP"].ToString());


                    //Customer Fields
                    TempFileTextLine = TempFileTextLine.Replace("#VarMktCustomerName#", txtCustomerName.Text);
                    string sCustomerProfiele = string.Empty;
                    sCustomerProfiele = txtAddress1.Text + ", " + txtAddress2.Text + ", " + txtAddress3.Text + ", " + txtCity.Text + ", " + txtPostalCode.Text;

                    ArrayList customerNameSplitParts = splitStringInSpaces(sCustomerProfiele, 50);
                    for (int i = 0; i < customerNameSplitParts.Count; i++)
                    {
                        string thisline = customerNameSplitParts[i].ToString();
                        TempFileTextLine = TempFileTextLine.Replace("#VarMktAddress" + i + "#", thisline);
                    }


                    TempFileTextLine = TempFileTextLine.Replace("#VarMktAddress0#", "");
                    TempFileTextLine = TempFileTextLine.Replace("#VarMktAddress1#", "");
                    TempFileTextLine = TempFileTextLine.Replace("#VarMktAddress2#", "");
                    TempFileTextLine = TempFileTextLine.Replace("#VarMktAddress3#", "");
                    TempFileTextLine = TempFileTextLine.Replace("#VarMktAddress4#", "");


                    //Manufacturer Fields
                    TempFileTextLine = TempFileTextLine.Replace("#VarManufacturedBy#", sPlantname);

                    ArrayList manufacturerFieldsList = new ArrayList();
                    if (chkSubsidiary.Checked == true)
                    {
                        string subsidiaryText = "(A Subsidiary of Akums Drugs and Pharmaceuticals Ltd.)";
                        manufacturerFieldsList.Add(subsidiaryText);
                    }

                    if (isLLcheckbox.Checked == true)
                    {
                        manufacturerFieldsList.Add("At: " + sPlantAddress1);
                    }
                    else
                    {
                        manufacturerFieldsList.Add(sPlantAddress1);
                    }
                    manufacturerFieldsList.Add(sPlantAddress2);
                    manufacturerFieldsList.Add(sPlantAddress3);

                    for (int i = 0; i < manufacturerFieldsList.Count; i++)
                    {
                        string thisline = manufacturerFieldsList[i].ToString();
                        TempFileTextLine = TempFileTextLine.Replace("#VarAddress" + i + "#", thisline);
                    }

                    TempFileTextLine = TempFileTextLine.Replace("#VarAddress0#", "");
                    TempFileTextLine = TempFileTextLine.Replace("#VarAddress1#", "");
                    TempFileTextLine = TempFileTextLine.Replace("#VarAddress2#", "");
                    TempFileTextLine = TempFileTextLine.Replace("#VarAddress3#", "");
                    TempFileTextLine = TempFileTextLine.Replace("#VarAddress4#", "");
                    TempFileTextLine = TempFileTextLine.Replace("#VarAddress5#", "");

                    TempFileTextLine = TempFileTextLine.Replace("#VarFormatNo#", "HPK-034/F02-00");

                    string sExpDateLabel = string.Empty;
                    string sConditionLabel = string.Empty;
                    if (rdbtnBestBefore.Checked == true)
                    {
                        sExpDateLabel = "Best Before:";
                    }
                    else if (rdbtnUseBefore.Checked == true)
                    {
                        sExpDateLabel = "Use Before:";
                    }
                    else if (rdbtnExpDate.Checked == true)
                    {
                        sExpDateLabel = "Expiry Date:";
                    }
                    // change by pushpak scondition value change


                    if (chkSample.Checked == true)
                    {
                        sConditionLabel = "PHYSICIAN'S SAMPLE";
                    }
                    if (chkNotForSale.Checked == true)
                    {
                        if (sConditionLabel != "")
                        {
                            sConditionLabel += " ";
                        }
                        sConditionLabel += "NOT TO BE SOLD";
                    }
                    if (chkNRxProduct.Checked == true)
                    {
                        if (sConditionLabel != "")
                        {
                            sConditionLabel += " ";
                        }
                        sConditionLabel += "NRx Product";
                    }
                    TempFileTextLine = TempFileTextLine.Replace("#VarSampleCondition#", sConditionLabel);
                    TempFileTextLine = TempFileTextLine.Replace("#VarExpiryDateLabel#", sExpDateLabel);

                    if (chkPrintRequired.Checked == true)
                    {
                        PCommon.mAppLog.WriteLog("Prn Send :" + TempFileTextLine, DTPLLogsWrite.LogType.Data, MethodBase.GetCurrentMethod());
                        blCommon.SendCommandToPrinter(lblPrinterIP.Text, Convert.ToInt32(lblPrinterPort.Text), TempFileTextLine);
                    }
                    else
                    {
                        PCommon.mAppLog.WriteLog("Prn Created but not sent due to checkbox :" + TempFileTextLine, DTPLLogsWrite.LogType.Data, MethodBase.GetCurrentMethod());
                    }

                    objBin.ReConnect();
                    ObjBin_OnConnect(0);

                }
                else
                {
                    PCommon.mAppLog.WriteLog("No data found for printing For Barcode :" + request.ShipperSerial, DTPLLogsWrite.LogType.Data, MethodBase.GetCurrentMethod());

                }
                bWeightCaptured = false;
                lblFMSMessge.Text = "SUCCESS";
                lblFMSMessge.BackColor = Color.Green;
                lblCount.Text = Convert.ToString(Convert.ToInt32(lblCount.Text) + 1);
                GetBatchPrintingData();
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                lblFMSMessge.Text = ex.Message;
                lblFMSMessge.BackColor = Color.Red;
            }
        }

        //private DataTable dtNewLine(string sAddress, int iNoChar)
        //{
        //    string sSecondValue = string.Empty;
        //    string s = sAddress;
        //    string[] s1 = s.Split(' ');
        //    string value1 = string.Empty;
        //    DataTable dataTable = new DataTable();
        //    dataTable.Columns.Add("Print");
        //    PCommon.mAppLog.WriteLog("Data Comeing for multirow  : " + sAddress + ", Total Array length :" + s1.Length.ToString(), DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
        //    for (int i = 0; i < s1.Length; i++)
        //    {
        //        if (i == 0)
        //        {
        //            if (s1[i].Length > 0)
        //            {
        //                value1 = s1[i].ToString();
        //                PCommon.mAppLog.WriteLog("Data Write : " + value1 + ", Array Length " + i.ToString(), DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
        //                if (i + 1 == s1.Length)
        //                {
        //                    DataRow dr = dataTable.NewRow();
        //                    dr[0] = value1;
        //                    dataTable.Rows.Add(dr);
        //                    value1 = string.Empty;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (s1[i].Length > 0)
        //            {
        //                sSecondValue = string.Empty;
        //                sSecondValue = value1 + " " + s1[i].ToString();
        //                PCommon.mAppLog.WriteLog("Data Write : " + sSecondValue + ", Array Length " + i.ToString(), DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
        //                if (sSecondValue.Length > iNoChar)
        //                {
        //                    DataRow dr = dataTable.NewRow();
        //                    dr[0] = value1;
        //                    dataTable.Rows.Add(dr);
        //                    value1 = string.Empty;
        //                    value1 = s1[i].ToString();
        //                }
        //                else
        //                {
        //                    value1 = value1 + " " + s1[i].ToString();
        //                    PCommon.mAppLog.WriteLog("Data Write : " + value1 + ", Array Length " + i.ToString(), DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
        //                    if (i + 1 == s1.Length)
        //                    {
        //                        DataRow dr = dataTable.NewRow();
        //                        dr[0] = value1;
        //                        dataTable.Rows.Add(dr);
        //                        value1 = string.Empty;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return dataTable;
        //}

        //Added by Dashmesh for address and product name split
        private ArrayList splitStringInSpaces(string totalString, int iNoChar)
        {
            ArrayList res = new ArrayList();
            string[] words = totalString.Split(' ');

            string currentline = string.Empty; //will bild this string one by one
            for (int i = 0; i < words.Length; i++)
            {
                string thisword = words[i];
                bool isCommaOnly = thisword.Trim().Equals(",");
                if (thisword.Length > 0 && !isCommaOnly)
                {
                    if (currentline.Length + thisword.Length <= iNoChar)
                    {
                        //space is available, just add word to current line
                        currentline = currentline + thisword + " ";
                    }
                    else
                    {
                        string toinsert = currentline.Replace(",,", ",");
                        toinsert = toinsert.Trim();
                        toinsert = toinsert.TrimStart(',');
                        //sentence is exceeding max line length, make a new line now
                        res.Add(toinsert); //trim to remove last extra space.
                        currentline = string.Empty; //reset variable and move to next line
                        currentline = currentline + thisword + " ";
                    }
                }
            }

            //after all words are done, append the last line
            string trimmedString = currentline.Replace(",,", ",");
            trimmedString = trimmedString.Trim();
            trimmedString = trimmedString.Trim(',');
            res.Add(trimmedString);

            return res;
        }

        #region Form Controll event Changed
        private void txtProcssOrder_KeyPress(object sender, KeyPressEventArgs e)

        
        {
            try
            {
                if (e.KeyChar == 13)
                {
                    txtBatchNo.Text = string.Empty;
                    txtGenricName.Text = string.Empty;
                    txtProductCode.Text = string.Empty;
                    txtMFGDate.Text = string.Empty;
                    txtExpDate.Text = string.Empty;
                    txtAddress1.Text = string.Empty;
                    txtAddress2.Text = string.Empty;
                    txtAddress3.Text = string.Empty;
                    txtCity.Text = string.Empty;
                    cmbCustomer.SelectedIndex = 0;
                    txtCustomerName.Text = string.Empty;
                    txtPostalCode.Text = string.Empty;
                    txtMaxWT.Text = "0";
                    txtMinWt.Text = "0";
                    cmbCondition.SelectedIndex = 0;
                    cmbLicence.SelectedIndex = 0;
                    txtRemarks.Text = string.Empty;
                    txtQuantity.Text = string.Empty;
                   // btnStart.Enabled = false;
                    btnTestPrint.Enabled = true;
                    dgvData.DataSource = null;
                    lblMcWeight.Text = "0.00";
                    lblCount.Text = "0";
                    txtTargetShipper.Text = "0";

                    DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                    DataTable dtProcessData = obj.ScanProcessCode(txtProcssOrder.Text);
                    if (dtProcessData.Rows.Count > 0)
                    {
                        string sRsult = dtProcessData.Rows[0][0].ToString();
                        if (sRsult.StartsWith("ERROR~"))
                        {
                            lblFMSMessge.Text = sRsult.Split('~')[1];
                            txtProcssOrder.Text = string.Empty;
                            txtProcssOrder.Focus();
                            return;
                        }
                        else
                        {
                            txtProductCode.Text = dtProcessData.Rows[0][0].ToString();
                            txtGenricField.Text = dtProcessData.Rows[0][1].ToString();
                            txtBatchNo.Text = dtProcessData.Rows[0][2].ToString();
                            txtMFGDate.Text = dtProcessData.Rows[0][3].ToString().Trim().Replace("00:00:00", "");
                            txtExpDate.Text = dtProcessData.Rows[0][4].ToString().Trim().Replace("00:00:00", "");
                            txtGenricName.Text = dtProcessData.Rows[0][6].ToString();
                            Mdicine.Text = dtProcessData.Rows[0][7].ToString();
                            SuperviserCMb.Text = dtProcessData.Rows[0][8].ToString();
                            MediceCheck.Text = dtProcessData.Rows[0][9].ToString();
                            CmbCustLisce.Text = dtProcessData.Rows[0][10].ToString();
                            CmdCountry.Text = dtProcessData.Rows[0][11].ToString();

                            if (dtProcessData.Rows[0]["CustomerCode"].ToString().Length > 0)
                            {
                                cmbCustomer.Text = dtProcessData.Rows[0]["CustomerCode"].ToString();
                            }
                            GetLastPrintedData();
                            GetBatchPrintingData();

                            DataSet dtCustomerCode = obj.GetShipperPrintingData(txtProcssOrder.Text);
                            lblCount.Text = dtCustomerCode.Tables[1].Rows[0][0].ToString();

                            if (lblCount.Text != "0")
                            {
                                SuperviserCMb.Enabled = false;
                                btnTestPrint.Enabled = true;
                                EditBtn.Enabled = true;
                               // btnStart.Enabled = false;
                                cmbCustomer.Enabled = false;
                                cmbCondition.Enabled = false;
                                cmbLicence.Enabled = false;
                                txtProcssOrder.Enabled = false;
                                txtProductCode.Enabled = false;
                                txtGenricName.Enabled = false;
                                txtCustomerName.Enabled = false;
                                txtMaxWT.Enabled = false;
                                txtMinWt.Enabled = false;
                                txtTareWt.Enabled = false;
                                txtQuantity.Enabled = false;
                                txtTargetShipper.Enabled = false;
                                txtRemarks.Enabled = false;
                                txtQuantity.Enabled = false;
                                chkNotForSale.Enabled = false;
                                chkNRxProduct.Enabled = false;
                                chkSample.Enabled = false;
                                rdbtnMonthInChar.Enabled = false;
                                rdbtnMonthInnum.Enabled = false;
                                rdbtnUseBefore.Enabled = false;
                                rdbtnExpDate.Enabled = false;
                                rdbtnBestBefore.Enabled = false;
                                rdbtnYearInLastTwoDigit.Enabled = false;
                                rbtnCHAR.Enabled = false;
                                txtmrp.Enabled = false;

                                isLLcheckbox.Enabled = false;
                                chkR1.Enabled = false;
                                chkTM1.Enabled = false;
                                chkSubsidiary.Enabled = false;
                                chkPrintRequired.Enabled = false;
                                txtGenricField.Enabled = false;
                                Mdicine.Enabled = false;
                                checkBoxMedicine.Enabled = false;
                                MediceCheck.Enabled = false;
                                CmbCustLisce.Enabled = false;
                                RbdDDMMMYYYY.Enabled = false;
                                btnTestPrint.Enabled = true;
                                CmdCountry.Enabled = false;

                            }
                        }
                    }
                    else
                    {
                        lblFMSMessge.Text = "Invalid Process Code,Please scan new one";
                        txtProcssOrder.Text = string.Empty;
                        txtProcssOrder.Focus();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        private void txtMinWt_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                blCommon.DecimalValidation(e, sender);
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        private void cmbCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbCustomer.SelectedIndex > 0)
                {
                    txtAddress1.Text = string.Empty;
                    txtAddress2.Text = string.Empty;
                    txtAddress3.Text = string.Empty;
                    txtCity.Text = string.Empty;
                    txtCustomerName.Text = string.Empty;
                    txtPostalCode.Text = string.Empty;
                    DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                    DataTable dt = obj.GetCustomerDetails(cmbCustomer.SelectedValue.ToString());
                    if (dt.Rows.Count > 0)
                    {
                        txtCustomerName.Text = dt.Rows[0][0].ToString();
                        txtAddress1.Text = dt.Rows[0][1].ToString();
                        txtAddress2.Text = dt.Rows[0][2].ToString();
                        txtAddress3.Text = dt.Rows[0][3].ToString();
                        txtCity.Text = dt.Rows[0][4].ToString();
                        txtPostalCode.Text = dt.Rows[0][5].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        #endregion

        #region Button Event
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = MessageBox.Show("Are you sure to start the process", blCommon.sMessageBox, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.No)
                {
                    return;
                }
                if (string.IsNullOrEmpty(txtProcssOrder.Text.Trim()))
                {
                    blCommon.ShowMessage("Please enter the process order", 3);
                    txtProcssOrder.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(txtCustomerName.Text.Trim()))
                {
                    blCommon.ShowMessage("Please select customer", 3);
                    cmbCustomer.Focus();
                    return;
                }
                if (cmbLicence.Items.Count == 0)
                {
                    blCommon.ShowMessage("There is no licence details found, Please contact admin", 3);
                    cmbLicence.Focus();
                    return;
                }
                if (cmbLicence.SelectedIndex == 0)
                {
                    blCommon.ShowMessage("Please select Licenece", 3);
                    cmbLicence.Focus();
                    return;
                }
                if (cmbCondition.Items.Count == 0)
                {
                    blCommon.ShowMessage("There is no conditon details found, Please contact admin", 3);
                    cmbCondition.Focus();
                    return;
                }
                if (cmbCondition.SelectedIndex == 0)
                {
                    blCommon.ShowMessage("Please select condition", 3);
                    cmbCondition.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(txtQuantity.Text.Trim()))
                {
                    blCommon.ShowMessage("Please enter quantity", 3);
                    txtQuantity.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(txtMaxWT.Text.Trim()))
                {
                    blCommon.ShowMessage("Please enter Max Wt", 3);
                    txtMaxWT.Focus();
                    return;
                }
                if (Convert.ToDecimal(txtMaxWT.Text) == 0)
                {
                    blCommon.ShowMessage("Please enter valid Max Wt", 3);
                    txtMaxWT.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(txtMinWt.Text.Trim()))
                {
                    blCommon.ShowMessage("Please enter Min Wt", 3);
                    txtMinWt.Focus();
                    return;
                }
                if (Convert.ToDecimal(txtMinWt.Text) == 0)
                {

                    blCommon.ShowMessage("Please enter valid Min Wt", 3);
                    txtMinWt.Focus();
                    return;
                }
                if (Convert.ToDecimal(txtMaxWT.Text) - Convert.ToDecimal(txtMinWt.Text) == 0)
                {
                    blCommon.ShowMessage("Max Wt and Min Wt can not be same", 3);
                    txtMinWt.Focus();
                    return;
                }
                if (lblWeighingIP.Text == "")
                {
                    blCommon.ShowMessage("Weighing IP Details not found,Please contact admin", 3);
                    txtMinWt.Focus();
                    return;
                }
                if (lblWeighingPort.Text == "")
                {
                    blCommon.ShowMessage("Weighing Port Details not found,Please contact admin", 3);
                    txtMinWt.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(txtTargetShipper.Text))
                {
                    blCommon.ShowMessage("Please enter target shipper", 3);
                    txtTargetShipper.Focus();
                    return;
                }

                if (chkSample.Checked && chkNotForSale.Checked && chkNRxProduct.Checked && checkBoxMedicine.Checked)
                {
                    blCommon.ShowMessage("Please select checkBoxMedicine, or  the only  rest of the fields.", 3);

                    return;
                }

                if (checkBoxMedicine.Checked && chkNotForSale.Checked)
                {
                    blCommon.ShowMessage("Please select only required field.", 3);

                    return;
                }

                if (checkBoxMedicine.Checked && chkNRxProduct.Checked)
                {
                    blCommon.ShowMessage("Please select only required field.", 3);

                    return;
                }



                if (string.IsNullOrEmpty(SuperviserCMb.Text))
                {
                    blCommon.ShowMessage("Please enter supervior Field", 3);
                    txtmrp.Focus();
                    return;
                }

                if (checkBoxMedicine.Checked)
                {
                    if (string.IsNullOrWhiteSpace(MediceCheck.Text))
                    {
                        blCommon.ShowMessage("Please enter medicine Field", 3);
                        MediceCheck.Focus();
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(MediceCheck.Text))
                {
                    if (checkBoxMedicine.Checked == false)
                    {
                        blCommon.ShowMessage("Please enter Required field", 3);
                        MediceCheck.Focus();
                        return;
                    }
                }

                if (checkBoxMedicine.Checked)
                {
                    if (string.IsNullOrWhiteSpace(MediceCheck.Text))
                    {
                        blCommon.ShowMessage("Please enter medicine Field", 3);
                        MediceCheck.Focus();
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(Mdicine.Text))
                    {
                        blCommon.ShowMessage("Please enter medicine remark field", 3);
                        Mdicine.Focus();
                        return;
                    }

                }



                //if (string.IsNullOrEmpty(txtmrp.Text))
                //{
                //    blCommon.ShowMessage("Please enter MRP", 3);
                //    txtmrp.Focus();
                //    return;
                //}

                EditBtn.Enabled = false;
                cmbCustomer.Enabled = false;
                cmbCondition.Enabled = false;
                cmbLicence.Enabled = false;
                txtProcssOrder.Enabled = false;
                txtProductCode.Enabled = false;
                txtGenricName.Enabled = false;
                txtCustomerName.Enabled = false;
                txtMaxWT.Enabled = false;
                txtMinWt.Enabled = false;
                txtTareWt.Enabled = false;
                txtQuantity.Enabled = false;
                txtTargetShipper.Enabled = false;
                btnTestPrint.Enabled = false;
                SuperviserCMb.Enabled = false;
                txtRemarks.Enabled = false;
                txtQuantity.Enabled = false;
                chkNotForSale.Enabled = false;
                chkNRxProduct.Enabled = false;
                RbdDDMMMYYYY.Enabled = false;
                chkSample.Enabled = false;
                rdbtnMonthInChar.Enabled = false;
                rdbtnMonthInnum.Enabled = false;
                rdbtnUseBefore.Enabled = false;
                rdbtnExpDate.Enabled = false;
                rdbtnBestBefore.Enabled = false;
                rdbtnYearInLastTwoDigit.Enabled = false;
                rbtnCHAR.Enabled = false;
                txtmrp.Enabled = false;

                isLLcheckbox.Enabled = false;
                chkR1.Enabled = false;
                chkTM1.Enabled = false;
                chkSubsidiary.Enabled = false;
                chkPrintRequired.Enabled = false;
                checkBoxMedicine.Enabled = false;
                MediceCheck.Enabled = false;
                Mdicine.Enabled = false;
                CmbCustLisce.Enabled = false;
                RbdDDMMMYYYY.Enabled = false;


                if (WeighingConnectionThroughIP == "1")
                {
                    objBin.IPAddress = lblWeighingIP.Text;
                    objBin.Port = Convert.ToInt32(lblWeighingPort.Text);

                    if (!objBin.IsConnected)
                    {
                        objBin.Poll = true;
                        objBin.Open();
                    }
                    if (objBin.IsConnected)
                    {
                        lblFMSMessge.Text = "SUCCESS";
                        lblFMSMessge.BackColor = Color.Green;
                        objBin.DataArrived += ObjBin1_DataArrived;
                    }
                    if (objBin.IsConnected == false)
                    {
                        lblHardwareFailMessage.Visible = true;
                        lblHardwareFailMessage.Text = "ALL HARDWARE ARE NOT CONNECTED";
                    }
                    else
                    {
                        lblHardwareFailMessage.Visible = false;
                    }
                    PingTimer.Interval = 2000;
                    PingTimer.Enabled = true;
                    PingTimer.Start();
                    this.PingTimer.Tick += PingTimer_Tick;
                }
                else
                {
                    serialPort1.PortName = ConfigurationManager.AppSettings["PortName"].ToString();
                    serialPort1.BaudRate = Convert.ToInt32(ConfigurationManager.AppSettings["BaudRate"]);
                    serialPort1.DataBits = Convert.ToInt32(ConfigurationManager.AppSettings["DataBits"]);
                    serialPort1.StopBits = StopBits.One;
                    serialPort1.Parity = Parity.None;
                    serialPort1.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
                    serialPort1.Open();
                    lblHardwareFailMessage.Visible = false;
                }

                btnStart.Enabled = false;
                Edit.Enabled = true;


                //ErrorLogModel objModel = new ErrorLogModel();
                //objModel.Message = "Start Button Press On Label Print with Process Order" + txtProcssOrder.Text + "";
                //objModel.ModuleName = "Label Printing";
                //objModel.LogType = "Message";
                //objModel.Severity = "2";
                //  objModel.BatchNo = txtBatchNo.Text;
                //DL_UserLogin dL_UserLogin = new DL_UserLogin();
                //dL_UserLogin.WriteErrorLog(objModel);

                //objBin.IPAddress = lblWeighingIP.Text;
                //objBin.Port = Convert.ToInt32(lblWeighingPort.Text);

                //if (!objBin.IsConnected)
                //{
                //    objBin.Poll = true;
                //    objBin.Open();
                //}
                //if (objBin.IsConnected)
                //{
                //    objBin.DataArrived += ObjBin1_DataArrived;
                //}
                //if (objBin.IsConnected == false)
                //{
                //    lblHardwareFailMessage.Visible = true;
                //    lblHardwareFailMessage.Text = "ALL HARDWARE ARE NOT CONNECTED";
                //}
                //else
                //{
                //    lblHardwareFailMessage.Visible = false;
                //}
                //PingTimer.Interval = 2000;
                //PingTimer.Enabled = true;
                //PingTimer.Start();
                //this.PingTimer.Tick += PingTimer_Tick;
                //btnStart.Enabled = false;
                //Edit.Enabled = true;
            }
            catch (Exception ex)
            {
                ErrorLogModel objModel = new ErrorLogModel();
                objModel.Message = "Start Button Press On Label Print with Process Order" + txtProcssOrder.Text + " and Error : " + ex.Message;
                objModel.ModuleName = "Label Printing";
                objModel.LogType = "Error";
                objModel.Severity = "1";
                objModel.BatchNo = txtBatchNo.Text;
                DL_UserLogin dL_UserLogin = new DL_UserLogin();
                dL_UserLogin.WriteErrorLog(objModel);
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                if (objBin != null)
                {
                    if (objBin.IsConnected)
                    {
                        objBin.Poll = false;
                    }
                }
                this.PingTimer.Tick -= PingTimer_Tick;
                PingTimer.Enabled = false;
                PingTimer.Stop();
                this.Close();
                System.Windows.Forms.Application.Exit();
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        private void btnTestPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtProcssOrder.Text.Trim()))
                {
                    blCommon.ShowMessage("Please enter the process order", 3);
                    txtProcssOrder.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(txtCustomerName.Text.Trim()))
                {
                    blCommon.ShowMessage("Please select customer", 3);
                    cmbCustomer.Focus();
                    return;
                }
                if (cmbLicence.Items.Count == 0)
                {
                    blCommon.ShowMessage("There is no licence details found, Please contact admin", 3);
                    cmbLicence.Focus();
                    return;
                }
                if (cmbLicence.SelectedIndex == 0)
                {
                    blCommon.ShowMessage("Please select Licenece", 3);
                    cmbLicence.Focus();
                    return;
                }
                if (cmbCondition.Items.Count == 0)
                {
                    blCommon.ShowMessage("There is no conditon details found, Please contact admin", 3);
                    cmbCondition.Focus();
                    return;
                }
                if (cmbCondition.SelectedIndex == 0)
                {
                    blCommon.ShowMessage("Please select condition", 3);
                    cmbCondition.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(txtQuantity.Text.Trim()))
                {
                    blCommon.ShowMessage("Please enter quantity", 3);
                    txtQuantity.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(txtMaxWT.Text.Trim()))
                {
                    blCommon.ShowMessage("Please enter Max Wt", 3);
                    txtMaxWT.Focus();
                    return;
                }
                if (Convert.ToDecimal(txtMaxWT.Text) == 0)
                {
                    blCommon.ShowMessage("Please enter valid Max Wt", 3);
                    txtMaxWT.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(txtMinWt.Text.Trim()))
                {
                    blCommon.ShowMessage("Please enter Min Wt", 3);
                    txtMinWt.Focus();
                    return;
                }
                if (Convert.ToDecimal(txtMinWt.Text) == 0)
                {
                    blCommon.ShowMessage("Please enter valid Min Wt", 3);
                    txtMinWt.Focus();
                    return;
                }
                if (Convert.ToDecimal(txtMaxWT.Text) - Convert.ToDecimal(txtMinWt.Text) == 0)
                {
                    blCommon.ShowMessage("Max Wt and Min Wt can not be same", 3);
                    txtMinWt.Focus();
                    return;
                }
                if (lblWeighingIP.Text == "")
                {
                    blCommon.ShowMessage("Weighing IP Details not found,Please contact admin", 3);
                    txtMinWt.Focus();
                    return;
                }
                if (lblWeighingPort.Text == "")
                {
                    blCommon.ShowMessage("Weighing Port Details not found,Please contact admin", 3);
                    txtMinWt.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(txtTargetShipper.Text))
                {
                    blCommon.ShowMessage("Please enter target shipper", 3);
                    txtTargetShipper.Focus();
                    return;
                }

                if (chkSample.Checked && chkNotForSale.Checked && chkNRxProduct.Checked && checkBoxMedicine.Checked)
                {
                    blCommon.ShowMessage("Please select checkBoxMedicine, or  the only  rest of the fields.", 3);

                    return;
                }

                if (checkBoxMedicine.Checked && chkNotForSale.Checked)
                {
                    blCommon.ShowMessage("Please select only required field.", 3);

                    return;
                }

                if (checkBoxMedicine.Checked && chkNRxProduct.Checked)
                {
                    blCommon.ShowMessage("Please select only required field.", 3);

                    return;
                }



                if (string.IsNullOrEmpty(SuperviserCMb.Text))
                {
                    blCommon.ShowMessage("Please enter supervior Field", 3);
                    txtmrp.Focus();
                    return;
                }

                if (checkBoxMedicine.Checked)
                {
                    if (string.IsNullOrWhiteSpace(MediceCheck.Text))
                    {
                        blCommon.ShowMessage("Please enter medicine Field", 3);
                        MediceCheck.Focus();
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(Mdicine.Text))
                    {
                        blCommon.ShowMessage("Please enter medicine remark field", 3);
                        Mdicine.Focus();
                        return;
                    }

                }

                // MediceCheck.Text
                lblMcWeight.Text = "0.00";
                PL_CartonLabelPrinting plobj = new PL_CartonLabelPrinting();
                DL_CartonLabelPrinting obj = new DL_CartonLabelPrinting();
                plobj.ProcessOrder = txtProcssOrder.Text.Trim();
                plobj.BatchNo = txtBatchNo.Text;
                plobj.Condition = cmbCondition.Text;
                plobj.TargetShipper = Convert.ToInt32(txtTargetShipper.Text);
                plobj.Licence = cmbLicence.Text;
                plobj.Quantity = txtQuantity.Text;
                plobj.GrossWeight = Decimal.Parse(lblMcWeight.Text);
                plobj.Remarks = txtRemarks.Text;
                plobj.TargetShipper = Convert.ToInt32(txtTargetShipper.Text);
                plobj.Mrp = txtmrp.Text;
                plobj.Medicine = Mdicine.Text;
                plobj.SupervisorField = SuperviserCMb.Text;
                plobj.MediceCheck = MediceCheck.Text;
               // plobj.CustomerLicence = CmbCustLisce.Text;

                if (CmbCustLisce.Text != "--Select--")
                {
                    plobj.CustomerLicence = CmbCustLisce.Text;
                }

                if (checkBoxMedicine.Checked == true)
                {
                    plobj.checkBoxMedicine = "1";
                }

                if (checkBoxMedicine.Checked == true)
                {

                    chkSample.Enabled = false;
                    chkNotForSale.Enabled = false;
                    chkNRxProduct.Enabled = false;
                }

                if (checkBoxMedicine.Checked == false)
                {

                    chkSample.Enabled = true;
                    chkNotForSale.Enabled = true;
                    chkNRxProduct.Enabled = true;
                }

                if (RbdDDMMMYYYY.Checked == true)

                {
                    plobj.YearFourDigit = "1";
                }



                if (rdbtnBestBefore.Checked == true)
                {
                    plobj.PrintedExpiryLabelFormat = "Best Before";
                }
                else if (rdbtnUseBefore.Checked == true)
                {
                    plobj.PrintedExpiryLabelFormat = "Use Before";
                }
                else if (rdbtnExpDate.Checked == true)
                {
                    plobj.PrintedExpiryLabelFormat = "Exp Date";
                }
                plobj.isNotForSale = "0";
                plobj.isNrx = "0";
                plobj.isPhysicianSample = "0";
                plobj.isLl = "0";
                plobj.isR = "0";
                plobj.isTm = "0";
                plobj.isSubsidary = "0";
                plobj.isPrintReqiured = "0";

                if (isLLcheckbox.Checked == true)
                {
                    plobj.isLl = "1";
                }
                if (chkTM1.Checked == true)
                {
                    plobj.isTm = "1";
                }
                if (chkR1.Checked == true)
                {
                    plobj.isR = "1";
                }
                if (chkSubsidiary.Checked == true)
                {
                    plobj.isSubsidary = "1";
                }
                if (chkPrintRequired.Checked == true)
                {
                    plobj.isPrintReqiured = "1";
                }
                if (chkNotForSale.Checked == true)
                {
                    plobj.isNotForSale = "1";
                }
                if (chkSample.Checked == true)
                {
                    plobj.isPhysicianSample = "1";
                }
                if (chkNRxProduct.Checked == true)
                {
                    plobj.isNrx = "1";
                }
                if (rdbtnMonthInChar.Checked == true)
                {
                    plobj.MonthInChar = "1";
                }
                if (rdbtnMonthInnum.Checked == true)
                {
                    plobj.MonthInName = "1";
                }
                if (rdbtnYearInLastTwoDigit.Checked == true)
                {
                    plobj.YearInTwoDigit = "1";
                }

                if (rbtnCHAR.Checked == true)
                {
                    plobj.MonthInCharYear = "1";
                }
                plobj.MaxWt = Convert.ToDecimal(txtMaxWT.Text);
                plobj.MinWt = Convert.ToDecimal(txtMinWt.Text);
                plobj.TareWt = Convert.ToDecimal(txtTareWt.Text);
                plobj.GenericField = txtGenricName.Text;
                plobj.MediceCheck = MediceCheck.Text;
                plobj.PrinterIP = lblPrinterIP.Text;
                plobj.ScaleIP = lblWeighingIP.Text;
                if (CmdCountry.Text != "--Select--")
                {
                    plobj.Country = CmdCountry.Text;
                }

                plobj.NetWeight = Decimal.Parse(lblNETWeight.Text);

                plobj.CustomerCode = cmbCustomer.SelectedValue.ToString();
                plobj.ProductName = txtGenricField.Text;

                //if (HardwareTesting(lblPrinterIP.Text) == false)
                //{
                //    lblFMSMessge.BackColor = Color.Red;
                //    lblFMSMessge.Text = "Printing IP Not Connected, Please try again";
                //    return;
                //}
                DataTable dt = obj.SaveTestPrintingData(plobj);
                if (dt.Rows.Count > 0)
                {
                    string sResult = dt.Rows[0][0].ToString();
                    if (sResult.StartsWith("SUCCESS~"))
                    {
                        lblFMSMessge.BackColor = Color.Green;
                        lblFMSMessge.Text = "SUCCESS";
                        string sSerialNo = sResult.Split('~')[2].ToString();
                        plobj.ShipperSerial = sSerialNo;

                        ErrorLogModel objModel = new ErrorLogModel();
                        objModel.Message = "TEST Label Print with Process Order" + txtProcssOrder.Text + "";
                        objModel.ModuleName = "Label Printing";
                        objModel.LogType = "Data";
                        objModel.Severity = "3";
                        objModel.BatchNo = txtBatchNo.Text;
                        DL_UserLogin dL_UserLogin = new DL_UserLogin();
                        dL_UserLogin.WriteErrorLog(objModel);
                        PrintLabel(plobj, "TEST");
                        btnStart.Enabled = true;
                    }
                    else
                    {
                        lblFMSMessge.BackColor = Color.Red;
                        lblFMSMessge.Text = sResult;
                    }
                }
            }

            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }

        #endregion

        private void txtTargetShipper_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                blCommon.OnlyNumeric(e);
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
                        
               try
            {

                if (lblMcWeight.Text == "0")
                {
                    blCommon.ShowMessage("Please weight again", 3);
                }
                if (string.IsNullOrEmpty(txtUserID.Text))
                {
                    blCommon.ShowMessage("Please enter user ID", 3);
                    txtUserID.Focus();
                    return;
                }
                else if (string.IsNullOrEmpty(txtPassword.Text.Trim()))
                {
                    blCommon.ShowMessage("Please enter Password", 3);
                    txtPassword.Focus();
                    return;
                }

                DialogResult dr = MessageBox.Show("Label reprinting done", blCommon.sMessageBox, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (lblMcWeight.Text == "0.00")
                {
                    lblFMSMessge.Text = "Item Weight can not be 0 please Reweight";
                    return;
                }

                if (dr == DialogResult.Yes)
                {
                    DL_CartonLabelPrinting objCartonPrinting = new DL_CartonLabelPrinting();

                    DataTable dt = objCartonPrinting.ValidateShipperData(txtUserID.Text, clsEncryptDecrypt.encrypt(txtPassword.Text), "Label_ReprintingWithRewing");
                    if (dt.Rows.Count > 0)
                    {
                        string sMessage = dt.Rows[0][0].ToString();
                        if (sMessage.StartsWith("ERROR~"))
                        {
                            ErrorLogModel objModel = new ErrorLogModel();
                            objModel.Message = "Verification Failed On Button Press For Re-Label Print with Barcode " + txtBarcode.Text + " and Supervisior ID : " + txtUserID.Text;
                            objModel.ModuleName = "Label Printing";
                            objModel.LogType = "Data";
                            objModel.Severity = "3";
                            objModel.BatchNo = txtBatchNo.Text;
                            DL_UserLogin dL_UserLogin = new DL_UserLogin();
                            dL_UserLogin.WriteErrorLog(objModel);
                            blCommon.ShowMessage(sMessage, 3);
                            txtUserID.Text = string.Empty;
                            txtPassword.Text = string.Empty;
                            txtUserID.Focus();
                            return;
                        }
                        else
                        {
                            ErrorLogModel objModel = new ErrorLogModel();
                            objModel.Message = "Validate Button Press For Re-Label Print with Barcode " + txtBarcode.Text + " and Supervisior ID : " + txtUserID.Text;
                            objModel.ModuleName = "Label Printing";
                            objModel.LogType = "Data";
                            objModel.Severity = "3";
                            objModel.BatchNo = txtBatchNo.Text;
                            DL_UserLogin dL_UserLogin = new DL_UserLogin();
                            dL_UserLogin.WriteErrorLog(objModel);
                            PL_CartonLabelPrinting objPl = new PL_CartonLabelPrinting();

                            string weight = lblMcWeight.Text;
                            string netweight = lblNETWeight.Text;



                            DataTable dtReprint = objCartonPrinting.SaveRePrintingData(txtBarcode.Text, weight, netweight);
                            if (dtReprint.Rows.Count > 0)
                            {
                                if (dtReprint.Rows[0][0].ToString().StartsWith("ERROR"))
                                {
                                    lblFMSMessge.Text = dtReprint.Rows[0][0].ToString();
                                    lblFMSMessge.BackColor = Color.Red;
                                    return;
                                }
                                objPl.ShipperSerial = dtReprint.Rows[0][0].ToString().Split('~')[2].ToString();
                            }
                            txtUserID.Text = string.Empty;
                            txtPassword.Text = string.Empty;
                            pnlReprintConfirmation.Hide();
                            PrintLabel(objPl, "OK");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                lblFMSMessge.Text = ex.Message;
            }
        }

        private void btnPanelClose_Click(object sender, EventArgs e)
        {
            txtUserID.Text = string.Empty;
            txtPassword.Text = string.Empty;
            pnlReprintConfirmation.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(Txt2User.Text))
                {
                    blCommon.ShowMessage("Please enter user ID", 3);
                    Txt2User.Focus();
                    return;
                }
                else if (string.IsNullOrEmpty(Txt3Pass.Text.Trim()))
                {
                    blCommon.ShowMessage("Please enter Password", 3);
                    Txt3Pass.Focus();
                    return;
                }

                else if (string.IsNullOrEmpty(RTxt.Text))
                {
                    blCommon.ShowMessage("Please enter remarks", 3);
                    RTxt.Focus();

                    return;
                }

                if (RTxt.Text.StartsWith("L"))
                {
                    blCommon.ShowMessage("Please enter correct remarks not Loose", 3);
                    RTxt.Focus();

                    return;
                }

                DL_CartonLabelPrinting objCartonPrinting = new DL_CartonLabelPrinting();
                DataTable dt = objCartonPrinting.ValidateShipperData(Txt2User.Text, clsEncryptDecrypt.encrypt(Txt3Pass.Text), "Shipper Over/Underweight Approval");
                if (dt.Rows.Count > 0)
                {

                    string sResult = dt.Rows[0][0].ToString();
                    if (sResult.StartsWith("ERROR~"))
                    {
                        ErrorLogModel objModel = new ErrorLogModel();
                        objModel.Message = "Verification Failed On Button Press For Max Threashold Print with Barcode " + txtBarcode.Text + " and Supervisior ID : " + txtUserID.Text;
                        objModel.ModuleName = "Label Printing";
                        objModel.LogType = "Data";
                        objModel.Severity = "3";
                        objModel.BatchNo = txtBatchNo.Text;
                        DL_UserLogin dL_UserLogin = new DL_UserLogin();
                        dL_UserLogin.WriteErrorLog(objModel);
                        blCommon.ShowMessage(sResult, 3);
                        Txt2User.Clear();
                        Txt3Pass.Clear();
                        RTxt.Focus(); return;
                    }
                    else
                    {
                        ErrorLogModel objModel = new ErrorLogModel();
                        objModel.Message = "Validate Button Press For Threashold Print with Barcode " + txtBarcode.Text + " and Supervisior ID : " + txtUserID.Text;
                        objModel.ModuleName = "Label Printing";
                        objModel.LogType = "Data";
                        objModel.Severity = "3";
                        objModel.BatchNo = txtBatchNo.Text;
                        DL_UserLogin dL_UserLogin = new DL_UserLogin();
                        dL_UserLogin.WriteErrorLog(objModel);
                        panelPrint.Hide();
                        Txt2User.Clear();
                        Txt3Pass.Clear();
                        WEIGHT(lblMcWeight.Text);
                        bTFWeight = false;
                    }
                }
                else
                {
                    lblFMSMessge.Text = "NOT FOUND";
                    lblFMSMessge.BackColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblFMSMessge.Text = "An error occurred: " + ex.Message;
            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            bTFWeight = false;
            panelPrint.Hide();
        }

        private void close()
        {

            bTFWeight = false;
            panelPrint.Hide();
        }


        private void btnReprint_Click(object sender, EventArgs e)
        {

            try
            {
                lblMcWeight.Text = "0.00";
                if (dgvData.Rows.Count > 0)
                {
                    string sSerialNo = dgvData.SelectedCells[1].Value.ToString();


                    if (sSerialNo.Length > 0)
                    {
                        DialogResult dr = MessageBox.Show("Rewing of the Shipper?", blCommon.sMessageBox, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

 
                        if (dr == DialogResult.Yes)
                        {
                            pnlReprintConfirmation.Visible = true;
                            txtBarcode.Text = sSerialNo;
                            txtBarcode.Enabled = true;
                            txtUserID.Text = string.Empty;
                            txtPassword.Text = string.Empty;
                            txtUserID.Focus();
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {

                        lblFMSMessge.Text = "No data found for reprinting";
                    }
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                lblFMSMessge.Text = ex.Message;
            }
        }

        private void Edit_Click(object sender, EventArgs e)
        {
            try
            {
                if (Edit.Text == "Edit")
                {
                    if (string.IsNullOrEmpty(txtProcssOrder.Text))
                    {
                        blCommon.ShowMessage("No data found for edit", 3);
                        txtProcssOrder.Focus();
                        return;
                    }
                    DialogResult dr = MessageBox.Show("Are you sure to Edit the data", blCommon.sMessageBox, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        cmbCustomer.Enabled = true;
                        cmbCondition.Enabled = true;
                        cmbLicence.Enabled = true;
                        txtProcssOrder.Enabled = true;
                        txtMaxWT.Enabled = true;
                        txtMinWt.Enabled = true;
                        txtQuantity.Enabled = true;
                        txtTargetShipper.Enabled = true;
                        Edit.Text = "Update";
                    }
                }
                else if (Edit.Text == "Update")
                {
                    if (string.IsNullOrEmpty(txtProcssOrder.Text.Trim()))
                    {
                        blCommon.ShowMessage("Please enter the process order", 3);
                        txtProcssOrder.Focus();
                        return;
                    }
                    if (string.IsNullOrEmpty(txtCustomerName.Text.Trim()))
                    {
                        blCommon.ShowMessage("Please select customer", 3);
                        cmbCustomer.Focus();
                        return;
                    }
                    if (cmbLicence.Items.Count == 0)
                    {
                        blCommon.ShowMessage("There is no licence details found, Please contact admin", 3);
                        cmbLicence.Focus();
                        return;
                    }
                    if (cmbLicence.SelectedIndex == 0)
                    {
                        blCommon.ShowMessage("Please select Licenece", 3);
                        cmbLicence.Focus();
                        return;
                    }
                    if (cmbCondition.Items.Count == 0)
                    {
                        blCommon.ShowMessage("There is no conditon details found, Please contact admin", 3);
                        cmbCondition.Focus();
                        return;
                    }
                    if (cmbCondition.SelectedIndex == 0)
                    {
                        blCommon.ShowMessage("Please select condition", 3);
                        cmbCondition.Focus();
                        return;
                    }
                    if (string.IsNullOrEmpty(txtQuantity.Text.Trim()))
                    {
                        blCommon.ShowMessage("Please enter quantity", 3);
                        txtQuantity.Focus();
                        return;
                    }
                    if (string.IsNullOrEmpty(txtMaxWT.Text.Trim()))
                    {
                        blCommon.ShowMessage("Please enter Max Wt", 3);
                        txtMaxWT.Focus();
                        return;
                    }
                    if (Convert.ToDecimal(txtMaxWT.Text) == 0)
                    {
                        blCommon.ShowMessage("Please enter valid Max Wt", 3);
                        txtMaxWT.Focus();
                        return;
                    }
                    if (string.IsNullOrEmpty(txtMinWt.Text.Trim()))
                    {
                        blCommon.ShowMessage("Please enter Min Wt", 3);
                        txtMinWt.Focus();
                        return;
                    }
                    if (Convert.ToDecimal(txtMinWt.Text) == 0)
                    {
                        blCommon.ShowMessage("Please enter valid Min Wt", 3);
                        txtMinWt.Focus();
                        return;
                    }
                    if (Convert.ToDecimal(txtMaxWT.Text) - Convert.ToDecimal(txtMinWt.Text) == 0)
                    {
                        blCommon.ShowMessage("Max Wt and Min Wt can not be same", 3);
                        txtMinWt.Focus();
                        return;
                    }
                    if (lblWeighingIP.Text == "")
                    {
                        blCommon.ShowMessage("Weighing IP Details not found,Please contact admin", 3);
                        txtMinWt.Focus();
                        return;
                    }
                    if (lblWeighingPort.Text == "")
                    {
                        blCommon.ShowMessage("Weighing Port Details not found,Please contact admin", 3);
                        txtMinWt.Focus();
                        return;
                    }
                    if (HardwareTesting(lblPrinterIP.Text) == false)
                    {
                        blCommon.ShowMessage("Unable to ping Printer machine IP,Please contact admin", 3);
                        lblPrinterIP.Focus();
                        return;
                    }
                    if (HardwareTesting(lblWeighingIP.Text) == false)
                    {
                        blCommon.ShowMessage("Unable to ping weighing machine IP,Please contact admin", 3);
                        lblWeighingIP.Focus();
                        return;
                    }
                    if (string.IsNullOrEmpty(txtTargetShipper.Text))
                    {
                        blCommon.ShowMessage("Please enter target shipper", 3);
                        txtTargetShipper.Focus();
                        return;
                    }
                    ErrorLogModel objModel = new ErrorLogModel();
                    objModel.Message = "Edit Button Press On Label Print with Process Order" + txtProcssOrder.Text;
                    objModel.ModuleName = "Label Printing";
                    objModel.LogType = "Data";
                    objModel.Severity = "1";
                    objModel.BatchNo = txtBatchNo.Text;
                    DL_UserLogin dL_UserLogin = new DL_UserLogin();
                    dL_UserLogin.WriteErrorLog(objModel);
                    cmbCustomer.Enabled = false;
                    cmbCondition.Enabled = false;
                    cmbLicence.Enabled = false;
                    txtProcssOrder.Enabled = false;
                    txtMaxWT.Enabled = false;
                    txtMinWt.Enabled = false;
                    txtQuantity.Enabled = false;
                    txtTargetShipper.Enabled = false;
                    Edit.Text = "Edit";
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                lblFMSMessge.Text = ex.Message;
            }
        }

        private void MasterCartonLabelPrinting_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                e.Cancel = true;
                this.Hide();
                this.Parent = null;
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                blCommon.ShowMessage(ex.Message, 3);
            }
        }


        #region GTN Format code section


        /// <summary>
        /// By : Kunal Verma
        /// This function is use in case of Weight Get in GTN format from machine 
        /// </summary>
        /// <param name="message"></param>
        /// 
        string grossWeight = string.Empty;
        string tareWeight = string.Empty;
        private void ProcessWeightData(string message)
        {
            try
            {
                // Example format: "G           2.080 kg\nT           0.312 kg\nN           1.768 kg"

                string netWeight = string.Empty;

                // Split the message into lines
                string[] lines = message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);


                grossWeight = lines[0].ToString().Replace("kg", "").Replace("G", "").Replace(" ", "");

                txtTareWt.Text = lines[1].ToString().Replace("kg", "").Replace("T", "").Replace(" ", "");

                netWeight = lines[2].ToString().Replace("kg", "").Replace("N", "").Replace(" ", "");



                if (!string.IsNullOrEmpty(grossWeight) && !string.IsNullOrEmpty(tareWeight))
                {
                    // Remove "kg" from weights if present
                    grossWeight = grossWeight.Replace("kg", "").Trim();
                    tareWeight = tareWeight.Replace("kg", "").Trim();


                }
                else
                {
                    lblFMSMessge.Text = "FAIL-INVALID DATA";
                    lblFMSMessge.BackColor = Color.Red;
                    lblFMSMessge.ForeColor = Color.White;
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
            }
        }

        #endregion GTN Section End 

        private void txtmrp_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblHardwareFailMessage_Click(object sender, EventArgs e)
        {

        }

        private void dgvData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure to Edit  the process", blCommon.sMessageBox, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                PnlEditValidation.Visible = true;

            }
        }

        private void button1_Click_3(object sender, EventArgs e)
        {
            PnlEditValidation.Hide();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UserID.Text))
            {
                blCommon.ShowMessage("Please enter user ID", 3);
                Txt2User.Focus();
                return;
            }
            else if (string.IsNullOrEmpty(Password.Text.Trim()))
            {
                blCommon.ShowMessage("Please enter Password", 3);
                Password.Focus();
                return;
            }
            DL_CartonLabelPrinting objCartonPrinting = new DL_CartonLabelPrinting();
            DataTable dt = objCartonPrinting.ValidateShipperData(UserID.Text, clsEncryptDecrypt.encrypt(Password.Text), "Edit Shiper Data");
            if (dt.Rows.Count > 0)
            {

                string sResult = dt.Rows[0][0].ToString();
                if (sResult.StartsWith("ERROR~"))
                {
                    ErrorLogModel objModel = new ErrorLogModel();
                    objModel.Message = "Verification Failed On Button Press For Edit Shipper Data  " + txtBarcode.Text + " and Supervisior ID : " + txtUserID.Text;
                    objModel.ModuleName = "Label Printing";
                    objModel.LogType = "Data";
                    objModel.Severity = "3";
                    objModel.BatchNo = txtBatchNo.Text;
                    DL_UserLogin dL_UserLogin = new DL_UserLogin();
                    dL_UserLogin.WriteErrorLog(objModel);
                    blCommon.ShowMessage(sResult, 3);
                    UserID.Clear();
                    Password.Clear();
                    return;
                }
                else
                {
                    ErrorLogModel objModel = new ErrorLogModel();
                    objModel.LogType = "Data";
                    objModel.Severity = "3";
                    objModel.BatchNo = txtBatchNo.Text;
                    DL_UserLogin dL_UserLogin = new DL_UserLogin();
                    dL_UserLogin.WriteErrorLog(objModel);
                    cmbCustomer.Enabled = true;
                    cmbCondition.Enabled = true;
                    cmbLicence.Enabled = true;
                    txtProcssOrder.Enabled = true;
                    txtProductCode.Enabled = true;
                    txtGenricName.Enabled = true;
                    txtCustomerName.Enabled = true;
                    txtMaxWT.Enabled = true;
                    txtMinWt.Enabled = true;
                    txtTareWt.Enabled = true;
                    txtQuantity.Enabled = true;
                    txtTargetShipper.Enabled = true;
                    btnTestPrint.Enabled = true;
                    txtRemarks.Enabled = true;
                    txtQuantity.Enabled = true;
                    chkNotForSale.Enabled = true;
                    chkNRxProduct.Enabled = true;
                    chkSample.Enabled = true;
                    rdbtnMonthInChar.Enabled = true;
                    rdbtnMonthInnum.Enabled = true;
                    rdbtnUseBefore.Enabled = true;
                    rdbtnExpDate.Enabled = true;
                    rdbtnBestBefore.Enabled = true;
                    rdbtnYearInLastTwoDigit.Enabled = true;
                    rbtnCHAR.Enabled = true;
                    txtmrp.Enabled = true;
                    isLLcheckbox.Enabled = true;
                    chkR1.Enabled = true;
                    chkTM1.Enabled = true;
                    chkSubsidiary.Enabled = true;
                    chkPrintRequired.Enabled = true;
                    txtGenricField.Enabled = true;
                    SuperviserCMb.Enabled = true;
                    Mdicine.Enabled = true;
                    checkBoxMedicine.Enabled = true;
                    MediceCheck.Enabled = true;
                    CmbCustLisce.Enabled = true;
                    RbdDDMMMYYYY.Enabled = true;
                    CmdCountry.Enabled = true;
                    Txt2User.Clear();
                    Password.Clear();
                    PnlEditValidation.Hide();

                }
            }





        }

        private void txtProcssOrder_TextChanged(object sender, EventArgs e)
        {

        }

        private void rdbtnYearInLastTwoDigit_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void isLLcheckbox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            PnlLooseWeight.Visible = true;
        }

        private void panel20_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtuserId4.Text))
                {
                    blCommon.ShowMessage("Please enter user ID", 3);
                    Txt2User.Focus();
                    return;
                }
                else if (string.IsNullOrEmpty(txtpass6.Text.Trim()))
                {
                    blCommon.ShowMessage("Please enter Password", 3);
                    Txt3Pass.Focus();
                    return;
                }

                else if (string.IsNullOrEmpty(Remrk.Text))
                {
                    blCommon.ShowMessage("Please enter remarks", 3);
                    Remrk.Focus();

                    return;
                }

                DL_CartonLabelPrinting objCartonPrinting = new DL_CartonLabelPrinting();
                DataTable dt = objCartonPrinting.ValidateShipperData(TxtuserId4.Text, clsEncryptDecrypt.encrypt(txtpass6.Text), "ShipperLoose");
                if (dt.Rows.Count > 0)
                {

                    string sResult = dt.Rows[0][0].ToString();
                    if (sResult.StartsWith("ERROR~"))
                    {
                        ErrorLogModel objModel = new ErrorLogModel();
                        objModel.Message = "Verification Failed On Button Press For Max Threashold Print with Barcode " + txtBarcode.Text + " and Supervisior ID : " + txtUserID.Text;
                        objModel.ModuleName = "Label Printing";
                        objModel.LogType = "Data";
                        objModel.Severity = "3";
                        objModel.BatchNo = txtBatchNo.Text;
                        DL_UserLogin dL_UserLogin = new DL_UserLogin();
                        dL_UserLogin.WriteErrorLog(objModel);
                        blCommon.ShowMessage(sResult, 3);
                        TxtuserId4.Clear();
                        txtpass6.Clear();
                        Remrk.Clear();
                        Remrk.Focus(); return;
                    }
                    else
                    {
                        ErrorLogModel objModel = new ErrorLogModel();
                        objModel.Message = "Validate Button Press For Threashold Print with Barcode " + txtBarcode.Text + " and Supervisior ID : " + txtUserID.Text;
                        objModel.ModuleName = "Label Printing";
                        objModel.LogType = "Data";
                        objModel.Severity = "3";
                        objModel.BatchNo = txtBatchNo.Text;
                        DL_UserLogin dL_UserLogin = new DL_UserLogin();
                        dL_UserLogin.WriteErrorLog(objModel);
                        PnlLooseWeight.Hide();
                        TxtuserId4.Clear();
                        txtpass6.Clear();
                       
                        bTFWeight = true;
                        
                        WEIGHT(lblMcWeight.Text);
                        Remrk.Clear();
                        bTFWeight = false;
                        panelPrint.Visible = false;
                    }
                }
                else
                {
                    lblFMSMessge.Text = "NOT FOUND";
                    lblFMSMessge.BackColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblFMSMessge.Text = "An error occurred: " + ex.Message;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            bTFWeight = false;
            PnlLooseWeight.Hide();
        }

        private void txtPostalCode_TextChanged(object sender, EventArgs e)
        {

        }

        private void RerintWIthoutRewing_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvData.Rows.Count > 0)
                {
                    string sSerialNo = dgvData.SelectedCells[1].Value.ToString();


                    if (sSerialNo.Length > 0)
                    {
                        DialogResult dr = MessageBox.Show("Reprint of the Shipper?", blCommon.sMessageBox, MessageBoxButtons.YesNo, MessageBoxIcon.Question);


                        if (dr == DialogResult.Yes)
                        {
                            panelwithoutRewing.Visible = true;
                            txBarcode.Text = sSerialNo;
                            txBarcode.Enabled = true;
                            TxUserID.Text = string.Empty;
                            TxPassword.Text = string.Empty;
                            TxUserID.Focus();
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {

                        lblFMSMessge.Text = "No data found for reprinting";
                    }
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                lblFMSMessge.Text = ex.Message;
            }
        }

        private void ReprintBtn_Click(object sender, EventArgs e)
        {

            try
            {
                if (string.IsNullOrEmpty(TxUserID.Text))
                {
                    blCommon.ShowMessage("Please enter user ID", 3);
                    txtUserID.Focus();
                    return;
                }
                else if (string.IsNullOrEmpty(TxPassword.Text.Trim()))
                {
                    blCommon.ShowMessage("Please enter Password", 3);
                    txtPassword.Focus();
                    return;
                }

                DL_CartonLabelPrinting objCartonPrinting = new DL_CartonLabelPrinting();

                DataTable dt = objCartonPrinting.ValidateShipperData(TxUserID.Text, clsEncryptDecrypt.encrypt(TxPassword.Text), "Label Reprinting");
                if (dt.Rows.Count > 0)
                {
                    string sMessage = dt.Rows[0][0].ToString();
                    if (sMessage.StartsWith("ERROR~"))
                    {
                        ErrorLogModel objModel = new ErrorLogModel();
                        objModel.Message = "Verification Failed On Button Press For Re-Label Print with Barcode " + txtBarcode.Text + " and Supervisior ID : " + txtUserID.Text;
                        objModel.ModuleName = "Label Printing";
                        objModel.LogType = "Data";
                        objModel.Severity = "3";
                        objModel.BatchNo = txtProcssOrder.Text;
                        DL_UserLogin dL_UserLogin = new DL_UserLogin();
                        dL_UserLogin.WriteErrorLog(objModel);
                        blCommon.ShowMessage(sMessage, 3);
                        txtUserID.Text = string.Empty;
                        txtPassword.Text = string.Empty;
                        txtUserID.Focus();
                        return;
                    }
                    else
                    {
                        ErrorLogModel objModel = new ErrorLogModel();
                        objModel.Message = "Validate Button Press For Re-Label Print with Barcode " + txtBarcode.Text + " and Supervisior ID : " + txtUserID.Text;
                        objModel.ModuleName = "Label Printing";
                        objModel.LogType = "Data";
                        objModel.Severity = "3";
                        objModel.BatchNo = txtProcssOrder.Text;
                        DL_UserLogin dL_UserLogin = new DL_UserLogin();
                        dL_UserLogin.WriteErrorLog(objModel);
                        PL_CartonLabelPrinting objPl = new PL_CartonLabelPrinting();
                        objPl.ShipperSerial = txtBarcode.Text;
                        DataTable dtReprint = objCartonPrinting.SaveRePrintingDataOnlyReprint(txBarcode.Text);
                        if (dtReprint.Rows.Count > 0)
                        {
                            if (dtReprint.Rows[0][0].ToString().StartsWith("ERROR"))
                            {
                                lblFMSMessge.Text = dtReprint.Rows[0][0].ToString();
                                lblFMSMessge.BackColor = Color.Red;
                                return;
                            }
                            objPl.ShipperSerial = dtReprint.Rows[0][0].ToString().Split('~')[2].ToString();
                        }
                        TxUserID.Text = string.Empty;
                        TxPassword.Text = string.Empty;
                        panelwithoutRewing.Hide();
                        PrintLabel(objPl, "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                PCommon.mAppLog.WriteLog(ex.Message, DTPLLogsWrite.LogType.Error, MethodBase.GetCurrentMethod());
                lblFMSMessge.Text = ex.Message;
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            txtUserID.Text = string.Empty;
            txtPassword.Text = string.Empty;
            panelwithoutRewing.Hide();

        }

        private void txtCustomerName_TextChanged(object sender, EventArgs e)
        {

        }
    }
}