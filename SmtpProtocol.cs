using System;
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
        private string InitialMessage = "220 Service ready";
        private string StartInput = "354 Start mail input; end with <CRLF>.";

        private bool InitialMessageSent = false;

        ServerSocket Server;
        string Domain;

        public SmtpProtocol(ServerSocket server, string domain)
        {
            Server = server;
            Domain = domain;
        }

        private AddressString SplitAddress(string message)
        {
            string beginning = message.Substring(0, message.IndexOf("<") + 1);
            string ending = message[message.Length - 1].ToString();
            string address = message.Remove(message.Length - 1, ending.Length).Remove(0, beginning.Length);

            return new AddressString(beginning, ending, address);
        }

        public void TestAddressSplitting(string message, AddressString expected)
        {
            var gotten = SplitAddress(message);
            if (!expected.Equals(gotten))
            {
                System.Console.WriteLine(gotten.Address);
                System.Console.WriteLine(gotten.Start);
                System.Console.WriteLine(gotten.End);
                System.Environment.Exit(1);
            }
            System.Console.WriteLine("Splitting good");
            return;
        }

        public void HandleClient()
        {
            if (!InitialMessageSent)
            {
                InitialMessageSent = true;
                Server.SendClientMessage(InitialMessage);
            }

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
                Server.CloseClient();
                System.Environment.Exit(1);
            }
            else if (message.Contains("rset"))
            {
                Server.SendClientMessage(Ok);
            }
            else if (message.Contains("vrfy"))
            {
                Server.SendClientMessage(VrfyCommand(message));
            }
            else if (message.Contains("mail FROM:<"))
            {
                var email = HandleEmailRequest(message);
                if (email != null)
                {
                    EmailSaver.Save((Email)email);
                }
            }
            else
            {
                Server.SendClientMessage(BadCommand);
            }

            HandleClient();
        }

        private string VerifyMessage(string message, string start, string end)
        {
            AddressString splitMessage = SplitAddress(message);
            if (splitMessage.Start.Equals(start) && splitMessage.End.Equals(end))
            {
                return splitMessage.Address;
            }
            return BadSequence;
        }

        private string VrfyCommand(string email)
        {
            var domainEnding = $"@{Domain}.com";

            if (!(email.Contains("@") && email.Contains(".")))
            {
                return BadParams;
            }
            else if (!email.EndsWith(domainEnding))
            {
                return BadUser;
            }
            return Ok;
        }

        private Email? HandleEmailRequest(string message)
        {
            var sender = VerifyMessage(message, "mail FROM:<", ">");
            if (sender.Equals(BadSequence))
            {
                Server.SendClientMessage(BadSequence);
                return null;
            }
            Server.SendClientMessage(Ok);

            List<string> recipients = new List<string>();
            while (true)
            {
                var rcptMessage = Server.GetClientMessage();
                if (rcptMessage.Contains("data"))
                {
                    if (recipients.Count() == 0)
                    {
                        Server.SendClientMessage(BadSequence);
                        return null;
                    }
                    break;
                }
                var recipient = VerifyMessage(rcptMessage, "rcpt TO:<", ">");
                if (recipient.Equals(BadSequence))
                {
                    Server.SendClientMessage(BadSequence);
                    return null;
                }
                recipients.Add(recipient);
                Server.SendClientMessage(Ok);
            }

            Server.SendClientMessage(StartInput);

            var clientMessage = Server.GetClientMessage();
            Server.SendClientMessage(Ok);

            var headers = EmailParser.ParseHeaders(clientMessage);

            return new Email(sender, recipients, EmailParser.ParseMessage(clientMessage), headers.Cc, headers.Bcc);
        }
    }
}