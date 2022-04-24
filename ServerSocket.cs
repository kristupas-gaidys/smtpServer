using System.Net;
using System.Net.Sockets;
using System.Text;

namespace smtpServer
{
    public class ServerSocket
    {
        private string Host;
        private string Port;
        private int Backlog = 1;

        private Socket Listener;

        private Socket Client;

        public ServerSocket(string host, string port)
        {
            Host = host;
            Port = port;
            StartServer();
        }

        public ServerSocket(string host, string port, int backlog)
        {
            Host = host;
            Port = port;
            Backlog = backlog;
            StartServer();
        }

        public void StartServer()
        {
            StartListening();
            AcceptClient();
        }

        public void SendClientMessage(string message)
        {
            Client.Send(Encoding.ASCII.GetBytes(message));
        }

        public string GetClientMessage()
        {
            byte[] bytes = new Byte[1024];
            string data = null;

            while (true)
            {
                int bytesRec = Client.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                if (data.Length != 0)
                {
                    break;
                }
            }

            return data;
        }

        private void AcceptClient()
        {
            Console.WriteLine("Waiting for a connection...");
            Client = Listener.Accept();
            Console.WriteLine("Client successfully accepted");
        }

        private void StartListening()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = IPAddress.Parse(Host);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Int32.Parse(Port));

            Listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Listener.Bind(localEndPoint);
                Listener.Listen(Backlog);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine($"\n Socket is listening on {Port} and {Host}");
        }
    }
}