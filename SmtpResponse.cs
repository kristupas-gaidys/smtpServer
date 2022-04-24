namespace smtpServer
{
    public static class SmtpResponse
    {
        public static string Bye = "221 Bye";
        public static string Ok = "250 OK";
        public static string BadCommand = "500 Syntax error, command unrecognized";
        public static string BadParams = "501 Syntax error in parameters or arguments";
        public static string BadSequence = "503 Bad sequence of commands";
        public static string BadUser = "550 User not available";
    }
}