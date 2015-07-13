using SmsClient;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SendTextMessage
{
    public partial class frmSendMessage : Form
    {
        public frmSendMessage()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cboServiceType.DataSource = SmsClient.Service.GetList();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Validation.IsValidMobileNumber(txtMobileNumber.Text))
                    throw new Exception("Mobile number was not in proper format");

                if (cboServiceType.SelectedIndex == -1)
                    throw new Exception("Service type required!");

                Service service = ((Service)cboServiceType.SelectedValue);

                object obj = Activator.CreateInstance(service.ServiceType);

                if (!(obj is SmsClient.ISmsClient))
                    throw new Exception("Invalid Service type");

                using (SmsClient.ISmsClient client = (SmsClient.ISmsClient)obj)
                {
                    client.SetAuthentication(txtUserName.Text, txtPassword.Text);

                    client.SendMessage(txtMobileNumber.Text, txtMessage.Text);

                    client.LogOut();
                }

                MessageBox.Show("Message send successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtMessage_TextChanged(object sender, EventArgs e)
        {
            lblMessageLengthCount.Text = string.Format("{0} / {1}", txtMessage.TextLength, txtMessage.MaxLength);
        }

        private void cboServiceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            int MaxLength = 0;

            if (cboServiceType.SelectedIndex == -1)
                txtMessage.MaxLength = 0;
            else
            {
                Service service = ((Service)cboServiceType.SelectedValue);
                MaxLength = service.MaxLength;
            }

            txtMessage.Text = txtMessage.Text.Substring(0, Math.Min(MaxLength, txtMessage.TextLength));

            txtMessage.MaxLength = MaxLength;

            lblMessageLengthCount.Text = string.Format("{0} / {1}", txtMessage.TextLength, txtMessage.MaxLength);
        }

        private void Form1_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            (new About()).ShowDialog();
        }
    }
}
