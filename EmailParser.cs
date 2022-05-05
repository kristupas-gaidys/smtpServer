namespace smtpServer
{
    public static class EmailParser
    {
        public static EmailHeaders ParseHeaders(string message)
        {
            EmailHeaders headers = new EmailHeaders();

            if (message.Contains("Cc"))
            {
                var index = message.IndexOf("Cc");
                var indexEnd = message.IndexOf("\r\n", index);
                var ccString = message.Substring(index + 3, indexEnd - index - 3);

                headers.Cc.AddRange(ccString.Split(","));
            }

            if (message.Contains("Bcc"))
            {
                var index = message.IndexOf("Bcc");
                var indexEnd = message.IndexOf("\r\n", index);
                var ccString = message.Substring(index + 4, indexEnd - index - 4);

                headers.Bcc.AddRange(ccString.Split(","));
            }

            return headers;
        }

        public static string ParseMessage(string message)
        {
            string returnStr = message;

            if (message.Contains("Cc"))
            {
                var index = returnStr.IndexOf("Cc");
                var indexEnd = returnStr.IndexOf("\r\n", index);
                returnStr = returnStr.Remove(index, indexEnd - index + 2);
            }
            if (message.Contains("Bcc"))
            {
                var index = returnStr.IndexOf("Bcc");
                var indexEnd = returnStr.IndexOf("\r\n", index);
                returnStr = returnStr.Remove(index, indexEnd - index + 2);
            }

            return returnStr;
        }
    }
}