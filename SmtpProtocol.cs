using System.Net.Sockets;

namespace smtpServer
{
    public class SmtpProtocol
    {
        private string Bye = "221 Bye";
        private string Ok = "250 OK";
        private string BadCommand = "500 Syntax error, command unrecognized";
        private string BadParams = "501 Syntax error in parameters or arguments";
        private string BadSequence = "503 Bad sequence of commands";
        private string BadUser = "550 User not available";
        private string Help = "250 OK. Supported commands: HELO, EHLO, MAIL, RCPT, DATA, NOOP, RSET, VRFY, QUIT";

        ServerSocket Server;
        string Domain;

        public SmtpProtocol(ServerSocket server, string domain)
        {
            Server = server;
            Domain = domain;
        }

        public void HandleClient()
        {
            var message = Server.GetClientMessage();

            if (message.Contains("noop"))
            {
                Server.SendClientMessage(Ok);
            }
            else if (message.Contains("ehlo"))
            {
                Server.SendClientMessage(Help);
            }
            else if (message.Contains("helo"))
            {
                Server.SendClientMessage(Ok);
            }
            else if (message.Contains("QUIT"))
            {
                Server.SendClientMessage(Bye);
            }
            else if (message.Contains("rset"))
            {
                Server.SendClientMessage(Ok);
            }
            else if (message.Contains("vrfy"))
            {
                Server.SendClientMessage(VerifyEmail(message.Split(" ")[1]));
            }
            else if (message.Contains("mail"))
            {
                Server.SendClientMessage(HandleEmailRequest(message.Split(" ")[1]));
            }
            else
            {
                Server.SendClientMessage(BadCommand);
            }
        }

        private string VerifyEmail(string email)
        {
            var domainEnding = $"@{Domain}.com";

            if (!email.Contains("@"))
            {
                return BadParams;
            }
            else if (email.EndsWith(domainEnding))
            {
                return BadUser;
            }
            return Ok;
        }

        private string HandleEmailRequest(string sender)
        {
            if (!(VerifyEmail(sender) is var code).Equals(Ok))
            {
                return code;
            }

            Server.SendClientMessage(Ok);
            var rcptTo = Server.GetClientMessage();

        }
    }
}