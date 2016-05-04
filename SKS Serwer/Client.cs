
namespace SKS_Serwer
{
    public class Client
    {
        public string IP { get; }
        public string AdminPort { get; set; } // port klienta z którym łączy się admin
        public string GroupID { get; }

        public Client(string groupID, string IP)
        {
            this.IP = IP;
            AdminPort = "9000";
            GroupID = groupID;
        }
    }
}
