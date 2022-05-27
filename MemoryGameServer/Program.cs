using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MemoryGameServer
{
    public class Program
    {
        private const int portNo = 1500;
        private const string ipAddress = "127.0.0.1";

        public static void Main(string[] args)
        {

            IPAddress localAdd = IPAddress.Parse(ipAddress);

            TcpListener listener = new TcpListener(localAdd, portNo);

            Console.WriteLine("Simple TCP Server");
            Console.WriteLine("Listening to ip {0} port: {1}", ipAddress, portNo);
            Console.WriteLine("Server is ready.");

            // Start listen to incoming connection requests
            listener.Start();

            // infinit loop.
            while (true)
            {
                // AcceptTcpClient - Blocking call
                // Execute will not continue until a connection is established

                // We create an instance of ChatClient so the server will be able to 
                // server multiple client at the same time.
                TcpClient tcp = listener.AcceptTcpClient();
                if (tcp != null)
                {
                    Thread t = new Thread(() => StartClient(tcp));
                    // Thread t = new Thread(new ThreadStart(StartClient),tcp);
                    t.Start();
                }

            }
        }
        public static void StartClient(TcpClient tcp)
        {
            Client user = new Client(tcp);
        }
    }
}
