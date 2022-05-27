using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;

namespace MemoryGameServer
{
    public class Client
    {
        // Store list of all clients connecting to the server
        // the list is static so all memebers of the chat will be able to obtain list
        // of current connected client
        public static Hashtable allClients = new Hashtable();

        public static List<Client> clientsList = new List<Client>();

        // information about the client
        private TcpClient _client;
        private string _clientIP;
        private string _ClientNick;

        // used for sending and reciving data
        private byte[] data;

        // the nickname being sent
        private bool ReceiveNick = true;

        private string _clientNick;
        private string _code;
        private RSA rsa;
        private string clientPublicKey;
        private string _connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Tal\Downloads\MemoryGame\MemoryGame\MemoryGameServer\Data\Database.mdf;Integrated Security=True";

        /// <summary>
        /// When the client gets connected to the server the server will create an instance of the ChatClient and pass the TcpClient
        /// </summary>
        /// <param name="client"></param>
        public Client(TcpClient client)
        {
            _client = client;

            // get the ip address of the client to register him with our client list
            _clientIP = client.Client.RemoteEndPoint.ToString();

            this.rsa = new RSA();

            // Add the new client to our clients collection
            allClients.Add(_clientIP, this);
            clientsList.Add(this);
            // Read data from the client async
            data = new byte[_client.ReceiveBufferSize];

            // BeginRead will begin async read from the NetworkStream
            // This allows the server to remain responsive and continue accepting new connections from other clients
            // When reading complete control will be transfered to the ReviveMessage() function.
            _client.GetStream().BeginRead(data,
                                          0,
                                          Convert.ToInt32(_client.ReceiveBufferSize),
                                          ReceiveMessage,
                                          null);
        }

        private object GetUserDetailFromDb(string columnName)
        {
            // connection object
            SqlConnection connection = new SqlConnection(_connectionString);
            // command object
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = connection;
            string sql = $"SELECT * FROM [Users] WHERE [username] = '{_clientNick}'";
            cmd.CommandText = sql;
            connection.Open();
            string valueToreturn;
            SqlDataReader reader = cmd.ExecuteReader();
            return reader[columnName];
        }

        //send code to the mail and return the code in string
        private void SendVerifyCode(string email)
        {
            //make the mail to send
            MailMessage message = new MailMessage()
            {
                From = new MailAddress("MemoryGameNba@gmail.com"),
                Subject = "Verification code",
                Body = "Your verification code is " + this._code,
            };

            //add the receive mail to the message
            message.To.Add(new MailAddress(email));

            // make new smtp client object(the sender/server)
            SmtpClient sender = new SmtpClient()
            {
                Port = 587,
                Host = "smtp.gmail.com",
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("MemoryGameNba@gmail.com", "Memory123456"),
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };

            //the smtp client send the message
            sender.Send(message);
        }

        public void ReceiveMessage(IAsyncResult ar)
        {
            int bytesRead;
            try
            {
                lock (_client.GetStream())
                {
                    // call EndRead to handle the end of an async read.
                    bytesRead = _client.GetStream().EndRead(ar);
                }
                // if bytesread<1 -> the client disconnected
                if (bytesRead < 1)
                {
                    // remove the client from out list of clients
                    allClients.Remove(_clientIP);

                    return;
                }
                else // client still connected
                {
                    string messageReceived = System.Text.Encoding.ASCII.GetString(data, 0, bytesRead);

                    if (messageReceived.StartsWith("client public key:")) //get the client public key
                    {
                        this.clientPublicKey = messageReceived.Remove(0, 18); //remove the header from the client message
                        string serverpublickey = rsa.GetPublicKey(); //get the server public key
                        SendMessage("server public key:" + serverpublickey); //send the server public key to client
                    }
                    // if the client is sending send me datatable
                    if (messageReceived.StartsWith("%%%regist%%%"))
                    {
                        string details = messageReceived.Remove(0, 12);
                        /*
                        //if regist ok
                        SendMessage("regist OK");

                        //if regist not ok
                        SendMessage("regist NOT OK"); */
                        if (InsertUser(details) > 0)
                            SendMessage("regist OK");
                        else
                            SendMessage("regist NOT OK");
                    }
                    else if (messageReceived.StartsWith("%%%login%%%"))
                    {

                        string userDetails = messageReceived.Remove(0, 11);
                        userDetails = rsa.Decrypt(userDetails);

                        //if login ok;
                        if (IsUserExist(userDetails))
                        {
                            _clientNick = userDetails.Split("'".ToCharArray()[0])[1];
                            this._code = ((new Random()).Next(1000, 10000)).ToString(); // make new 4 digit code
                            string email = GetUserDetailFromDb("email").ToString();
                            SendVerifyCode(email);
                            SendMessage("code sent");
                        }
                        else
                            // SendMessage("login NOT OK");
                            SendMessage("##print##" + "Wrong username or password");
                    }

                }

                lock (_client.GetStream())
                {
                    // continue reading form the client
                    _client.GetStream().BeginRead(data, 0, System.Convert.ToInt32(_client.ReceiveBufferSize), ReceiveMessage, null);
                    SendMessage(System.Text.Encoding.ASCII.GetString(data, 0, bytesRead) + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                allClients.Remove(_clientIP);
                //Broadcast(_ClientNick + " has left the chat.");
            }
        }//end ReceiveMessage

        /// <summary>
        /// allow the server to send message to the client.
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {
            try
            {
                NetworkStream ns;

                // we use lock to present multiple threads from using the networkstream object
                // this is likely to occur when the server is connected to multiple clients all of 
                // them trying to access to the networkstram at the same time.
                lock (_client.GetStream())
                {
                    ns = _client.GetStream();
                }

                // Send data to the client
                byte[] bytesToSend = System.Text.Encoding.ASCII.GetBytes(message);
                ns.Write(bytesToSend, 0, bytesToSend.Length);
                ns.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }//end SendMessage

        private int InsertUser(string userdetails)
        {
            try
            {
                string[] data = userdetails.Split(',');
                //connect sql to the server
                // string connectionstring = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\user1\Desktop\register\register\App_Data\Database1.mdf; Integrated Security = True";

                
                //connection object           
                SqlConnection connection = new SqlConnection(_connectionString);
                //command object
                SqlCommand cmd = new SqlCommand();
                //match command to connection
                cmd.Connection = connection;
                connection.Open();
                //parse userdetails
                cmd.CommandText = "INSERT INTO Table VALUES('" + data[0] + "','" + data[1] + "','" + data[2] + "','" + data[3] + "','" + data[4] + "','"
                                                                        + data[5] + "','" + data[6];
                int x = cmd.ExecuteNonQuery();
                System.Threading.Thread.Sleep(100);
                //int x = (int)cmd.ExecuteScalar();
                connection.Close();
                return x;
            }
            catch (Exception)
            {
                return 0;

            }
        }

        /*{
        string connectionString = @"";
        // connection object
        SqlConnection connection = new SqlConnection(connectionString);
        // command object
        SqlCommand cmd = new SqlCommand();

        //parse  userdetails

        string sql = "INSERT INTO UsersDetails VALUES ";
        connection.Open();
        cmd.ExecuteScalar();

        connection.Close();
    }*/

        private bool IsUserExist(string userDetails)
        {
            try
            {
                string[] data = userDetails.Split(',');
                string userName = data[0];
                string password = data[1];

                string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\user\Documents\MemoryGameAll\MemoryGame\MemoryGameServer\Data\Database.mdf"";Integrated Security=True";
                // string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=Database1.mdf;Integrated Security=True";
                // connection object
                SqlConnection connection = new SqlConnection(connectionString);
                // command object
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = connection;
                connection.Open();
                cmd.CommandText = $"SELECT COUNT(*) FROM [Users] WHERE [username] = '{userName}' AND [password] = '{password}';";
                int c = (int)cmd.ExecuteScalar();
                connection.Close();

                if (c > 0)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
