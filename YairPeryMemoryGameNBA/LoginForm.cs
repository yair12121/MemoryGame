using System;
using System.Windows.Forms;
using System.Net.Sockets;

namespace YairPeryMemoryGameNBA
{
    public partial class LoginForm : Form
    {
        private int portNo = 1500;
        private string ipAddress = "127.0.0.1";
        private TcpClient client;
        private byte[] data;
        private CommunicationHelper ch;

        public LoginForm()
        {
            InitializeComponent();
            this.ch = new CommunicationHelper(this);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string userName = this.txtUserName.Text;
            string password = this.txtPassword.Text;

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("User name and / or password can not be empty");
                return;
            }

            try
            {
                // connect to the server
                client = new TcpClient();
                client.Connect(ipAddress, portNo);

                string messageToSend = $"%%%login%%%{userName},{password}";

                // send message to the server
                NetworkStream ns = client.GetStream();
                byte[] data = System.Text.Encoding.ASCII.GetBytes(messageToSend);

                // send the text
                ns.Write(data, 0, data.Length);
                ns.Flush();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnSendVerificationCode_Click(object sender, EventArgs e)
        {
            string userName = this.txtUserName.Text;
            string password = this.txtPassword.Text;

            string userInfo = $"username='{userName}' AND password='{password}'";
            ch.SendMessage("%%%login%%%" + ch.Encrypt(userInfo));
        }
    }
}
