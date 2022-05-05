namespace smtpServer
{
    public struct AddressString
    {
        public string Start;
        public string End;
        public string Address;

        public AddressString(string start, string end, string address)
        {
            Start = start;
            End = end;
            Address = address;
        }
    }
}