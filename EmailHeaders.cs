using System.Collections;

namespace smtpServer
{
    public struct EmailHeaders
    {
        public EmailHeaders()
        {
            Cc = new List<string>();
            Bcc = new List<string>();
        }

        public List<string> Cc;
        public List<string> Bcc;
    }
}