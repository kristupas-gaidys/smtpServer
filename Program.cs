namespace smtpServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string Address = "127.0.0.1";
            string Port = "8000";
            string Domain = "test";

            // Server starts upon class init
            ServerSocket Server = new ServerSocket(Address, Port);
            SmtpProtocol Protocol = new SmtpProtocol(Server, Domain);

            Protocol.TestAddressSplitting("MAIL FROM:<kristupas@test.com>", new AddressString("MAIL FROM:<", ">", "kristupas@test.com"));

            Protocol.HandleClient();
        }
    }
}