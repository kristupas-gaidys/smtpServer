using System.Collections.Generic;
using System.Collections;

namespace smtpServer
{
    public struct Email
    {
        public string Sender;
        public string Message;
        public List<string> Recipients;
        public List<string> Cc;
        public List<string> Bcc;

        public Email(string sender, List<string> recipients, string message, List<string> cc, List<string> bcc)
        {
            Sender = sender;
            Recipients = recipients;
            Message = message;
            Cc = cc;
            Bcc = bcc;
        }

        public Email(Email email)
        {
            Sender = email.Sender;
            Recipients = new List<string>();
            Message = email.Message;
            Cc = new List<string>();
            Bcc = new List<string>();
        }

        public List<Recipient> GetRecipientList()
        {
            var list = new List<Recipient>();

            foreach (string recipient in Recipients)
            {
                list.Add(new Recipient(recipient, RecievingType.Regular));
            }
            foreach (string recipient in Cc)
            {
                list.Add(new Recipient(recipient, RecievingType.Cc));
            }
            foreach (string recipient in Bcc)
            {
                list.Add(new Recipient(recipient, RecievingType.Bcc));
            }

            return list;
        }
    }
}