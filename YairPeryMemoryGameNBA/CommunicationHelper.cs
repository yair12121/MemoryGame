using System;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace YairPeryMemoryGameNBA
{
    public class CommunicationHelper
    {
        private int portNo = 1500;
        private string ipAddress = "127.0.0.1";
        public TcpClient client;
        public byte[] data;
        private bool needEnemy = false;
        private bool rematch = false;
        private string serverPublicKey = "";
        public LoginForm lf;
        private Damka dm;
        private RegistForm rf;
        private Home h;
        private RSA rsa;
        private string stats;
        public delegate void delClientUpdateHistory();
        public delegate void delChoosePlayer(int n);
        public delegate void delMove(int row, int col, int selcRow, int selcCol, int addToI, int addToJ, int n);
        public delegate void delNext_2tUpdateHistory(string str);

        public CommunicationHelper(LoginForm lf)
        {
            this.lf = lf;

            client = new TcpClient();
            client.Connect(ipAddress, portNo);

            rsa = new RSA();

            SendMessage("client public key:" + rsa.GetPublicKey());

            data = new byte[client.ReceiveBufferSize];
            client.GetStream().BeginRead(data,
                                     0,
                                     System.Convert.ToInt32(client.ReceiveBufferSize),
                                     ReceiveMessage,
                                     null);
        }

        public string Encrypt(string message)
        {
            return rsa.Encrypt(message, this.serverPublicKey);
        }

        public void InitializeNextForm(Home h)
        {
            this.h = h;
        }

        public void InitializeNextForm(Damka dm)
        {
            this.dm = dm;
        }

        public void InitializeNextForm(RegistForm rf)
        {
            this.rf = rf;
        }

        public string GetStats()
        {
            return this.stats;
        }

        public void SendMessage(string message)
        {
            try
            {
                // send message to the server
                NetworkStream ns = client.GetStream();
                byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                // send the text
                ns.Write(data, 0, data.Length);
                ns.Flush();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Asynchronously read data sent from the server in a seperate thread.
        /// Update the txtMessageHistory control using delegate.
        /// 
        /// Windows controls are not thread safed !
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                int bytesRead;
                // read the data from the server
                bytesRead = client.GetStream().EndRead(ar);
                {
                    // invoke the delegate to display the recived data
                    string incommingData = System.Text.Encoding.ASCII.GetString(data, 0, bytesRead);
                    if (incommingData.StartsWith("server public key:"))//get public key fro server
                    {
                        //remove the header
                        serverPublicKey = incommingData.Remove(0, 18);
                    }
                    if (incommingData.StartsWith("##print##"))//show the message on mesage box
                        MessageBox.Show(incommingData.Remove(0, 9));
                    if (incommingData.Equals("code sent"))//inform the client that the code was send to his email
                    {
                        MessageBox.Show("Verification code sent to your email");
                        lf.Invoke(new delClientUpdateHistory(lf.ShowBoxes));
                    }
                    if (incommingData.StartsWith("##Open##"))//called the open home page function in the login form
                    {
                        stats = incommingData.Remove(0, 8);
                        MessageBox.Show("login successfully");
                        lf.Invoke(new delClientUpdateHistory(lf.OpenHome));
                    }
                    //if the client wait for opponent give him the opponent who send this invite
                    if (incommingData.StartsWith("##needEnemy##"))
                    {
                        if (needEnemy)
                        {
                            needEnemy = false;
                            string enemy = incommingData.Remove(0, 13);
                            SendMessage("##addMyEnemy##" + enemy);
                            dm.Invoke(new delClientUpdateHistory(dm.GiveChooseOption));
                        }
                    }
                    //give the information of the opponent who add you(agains you)
                    if (incommingData.StartsWith("##yourEnemy##"))
                    {
                        needEnemy = false;
                        dm.MyTurn();
                        string enemy = incommingData.Remove(0, 13);
                        SendMessage("##addEnemy##" + enemy);

                    }
                    //called the enmey win rate function in the damka form and give her the win rate value
                    if (incommingData.StartsWith("##myWinRate##"))
                    {
                        string wr = incommingData.Remove(0, 13);
                        wr = wr.Split('.')[0];
                        dm.Invoke(new delNext_2tUpdateHistory(dm.EnemyWR), wr);
                    }
                    //called the "my player" function in the damka form and give her int player value that he get from the server
                    if (incommingData.StartsWith("##yourPlayer##"))
                    {
                        int player = int.Parse(incommingData.Remove(0, 14));
                        dm.Invoke(new delChoosePlayer(dm.MyPlayer), player);
                    }
                    //get the information of the move from the server and called the move function in the damka form according ti the informations
                    if (incommingData.StartsWith("##move##"))
                    {
                        string beforeSplit = incommingData.Remove(0, 8);
                        string[] matInString = beforeSplit.Split(',');
                        dm.ChangeTurn();
                        int[] mat = new int[7];
                        for (int i = 0; i < 7; i++)
                            mat[i] = int.Parse(matInString[i]);
                        Thread.Sleep(50);
                        dm.Invoke(new delMove(dm.MoveImage), mat[0], mat[1], mat[2], mat[3], mat[4], mat[5], mat[6]);
                        dm.Invoke(new delClientUpdateHistory(dm.ChangeText));
                    }
                    //called the end game function in the damka and send to the server that he lose
                    if (incommingData.StartsWith("##youLose##"))
                    {
                        dm.Invoke(new delClientUpdateHistory(dm.EndGame));
                        dm.Invoke(new delClientUpdateHistory(dm.Lose));
                        SendMessage("##lose##");
                    }
                    //quit from damka form(whit quit function)
                    if (incommingData.StartsWith("##leaveGame##"))
                    {
                        rematch = false;
                        SendMessage("##quit##");
                        dm.OppOut();
                        dm.Invoke(new delClientUpdateHistory(dm.Quit));
                        Thread.Sleep(100);
                        MessageBox.Show("your opponent leave the game");
                    }
                    //if there is home page in one of the open forms called the stats function in home form and give her the stats(got from te server)
                    if (incommingData.StartsWith("##stats##"))
                    {
                        this.stats = incommingData.Remove(0, 9);
                        FormCollection fc = Application.OpenForms;
                        foreach (Form frm in fc)
                        {
                            if (frm.Name == "Home")
                            {
                                h.Invoke(new delNext_2tUpdateHistory(h.Stats), stats);
                            }
                        }
                    }
                    //inform the client the opponent want rematch, if he already want called rematch function and send to server agree rematch message
                    if (incommingData.StartsWith("##wantRematch##"))
                    {
                        dm.Invoke(new delClientUpdateHistory(dm.RematchInvite));
                        if (rematch)
                        {
                            dm.Invoke(new delClientUpdateHistory(dm.Rematch));
                            SendMessage("##rematchOK##");
                        }
                        rematch = false;
                    }
                    if (incommingData.StartsWith("##rematchAccept##"))//called rematch function on damka form
                    {
                        dm.Invoke(new delClientUpdateHistory(dm.Rematch));
                        rematch = false;
                    }
                }

                // continue reading
                client.GetStream().BeginRead(data,
                                         0,
                                         System.Convert.ToInt32(client.ReceiveBufferSize),
                                         ReceiveMessage,
                                         null);
            }
            catch (Exception ex)
            {
                // ignor the error... fired when the user loggs off
            }
        }// end ReceiveMessage

        public void StartGame()
        {
            needEnemy = true;
            SendMessage("##findEnemy##");
        }

        public void Cancel()
        {
            needEnemy = false;
        }

        public void Rematch()
        {
            rematch = true;
        }
    }
}
