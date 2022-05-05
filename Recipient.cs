namespace smtpServer
{
    public struct Recipient
    {
        public Recipient(string name, RecievingType type)
        {
            Name = name;
            Type = type;
        }

        public string Name;
        public RecievingType Type;
    }
}