using System;

namespace smtpServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string Host = "127.0.0.1";
            string Port = "8000";

            // Server starts upon class init
            ServerSocket Server = new ServerSocket(Host, Port);

            string messageFromClient;
            while (true)
            {
                messageFromClient = Server.GetClientMessage();
                Server.SendClientMessage(messageFromClient.ToUpper());
            }
        }
    }
}