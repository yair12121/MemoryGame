using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGameServer
{
    public class Client
    {
        // Store list of all clients connecting to the server
        // the list is static so all memebers of the chat will be able to obtain list
        // of current connected client
        public static Hashtable AllClients = new Hashtable();

        public static List<Client> clientsList = new List<Client>();
        // information about the client
        private TcpClient _client;
        private string _clientIP;
        private string _ClientNick;

        // used for sending and reciving data
        private byte[] data;

        // the nickname being sent
        private bool ReceiveNick = true;

        /// <summary>
        /// When the client gets connected to the server the server will create an instance of the ChatClient and pass the TcpClient
        /// </summary>
        /// <param name="client"></param>
        public Client(TcpClient client)
        {
            _client = client;

            // get the ip address of the client to register him with our client list
            _clientIP = client.Client.RemoteEndPoint.ToString();

            // Add the new client to our clients collection
            AllClients.Add(_clientIP, this);
            clientsList.Add(this);
            // Read data from the client async
            data = new byte[_client.ReceiveBufferSize];

            // BeginRead will begin async read from the NetworkStream
            // This allows the server to remain responsive and continue accepting new connections from other clients
            // When reading complete control will be transfered to the ReviveMessage() function.
            _client.GetStream().BeginRead(data,
                                          0,
                                          System.Convert.ToInt32(_client.ReceiveBufferSize),
                                          ReceiveMessage,
                                          null);
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
                    AllClients.Remove(_clientIP);

                    return;
                }
                else // client still connected
                {
                    string messageReceived = System.Text.Encoding.ASCII.GetString(data, 0, bytesRead);
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

                        string details = messageReceived.Remove(0, 11);
                        //if login ok;
                        if (IsExist(details))
                            SendMessage("login OK");
                        else
                            SendMessage("login NOT OK");
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
                AllClients.Remove(_clientIP);
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
                System.Net.Sockets.NetworkStream ns;

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

                string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\user\Downloads\login server clint Blank (3)\login server clint\DataDemoLogin_server\DataDemoLogin_server\Database1.mdf;Integrated Security=True";
                //connection object           
                SqlConnection connection = new SqlConnection(connectionString);
                //command object
                SqlCommand cmd = new SqlCommand();
                //match command to connection
                cmd.Connection = connection;
                connection.Open();
                //parse userdetails
                cmd.CommandText = "INSERT INTO Table VALUES('" + data[0] + "','" + data[1] + "','" + data[2] + "','" + data[3] + "','" + data[4] + "','"
                                                                        + data[5] + "','" + data[6];
                int x = cmd.ExecuteNonQuery();
                Thread.Sleep(100);
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

        private bool IsExist(string username)
        {
            try
            {
                string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\user\Downloads\login server clint Blank (3)\login server clint\DataDemoLogin_server\DataDemoLogin_server\Database1.mdf"";Integrated Security=True";
                // string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=Database1.mdf;Integrated Security=True";
                // connection object
                SqlConnection connection = new SqlConnection(connectionString);
                // command object
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = connection;
                connection.Open();
                cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE username = '" + username + "';";
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


            /* string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C: \Users\user\Downloads\login server clint Blank(3)\login server clint\DataDemoLogin_server\DataDemoLogin_server\Database1.mdf;Integrated Security=True";
            // connection object
            SqlConnection connection = new SqlConnection(connectionString);
            // command object
            SqlCommand cmd = new SqlCommand();

            string sql = "SELECT COUNT(*) FROM Users" + Users;

            connection.Open();
            int c = (int)cmd.ExecuteScalar();

            connection.Close();

            if (c > 0)
            {
                return true;
            }
            return false; */
        }
    }
}
