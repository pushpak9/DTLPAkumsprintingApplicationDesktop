using AkumsPrintingApplication.BusinessLayer;
using System;
using System.Windows.Forms;

namespace AkumsPrintingApplication.Forms.Masters
{
    public partial class PasswordWindow : Form
    {
        public PasswordWindow()
        {
            InitializeComponent();
            blCommon.bPasswordOK = false;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                blCommon.ShowMessage("Please enter password", 2);
                txtPassword.Focus();
                return;
            }
            if (txtPassword.Text != "Admin@1234")
            {
                blCommon.ShowMessage("Invalid Password,Please try again", 2);
                txtPassword.Text = string.Empty;
                txtPassword.Focus();
                return;
            }
            else
            {
                blCommon.bPasswordOK = true;
                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
            blCommon.bPasswordOK = false;
        }
    }
}
