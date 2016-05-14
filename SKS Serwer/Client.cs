namespace SKS_Serwer
{
    public class Client
    {
        public string IP { get; set; }
        public string AdminPort { get; set; } // port klienta z którym łączy się admin
        public string GroupID { get; set; }
        public string GroupPassword { get; set; }

        public override string ToString()
        {
            return IP + ":" + AdminPort;
        }
    }
}
