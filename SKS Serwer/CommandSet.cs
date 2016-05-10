
namespace SKS_Serwer
{
    public static class CommandSet
    {
        // liczby oznaczają liczbę argumentów danej komendy
        public static readonly Command Auth = new Command("AUTH", 1);
        public static readonly Command AdminConnect = new Command("CONNECT", 3);
        public static readonly Command ClientConnect = new Command("CONNECT", 4);
        public static readonly Command Disconnect = new Command("DISCONNECT");
        public static readonly Command VerifyList = new Command("VERIFYLIST", 2);
        public static readonly Command OK = new Command("OK");
        public static readonly Command List = new Command("LIST", 2);
        public static readonly Command Users = new Command("USERS");
        public static readonly Command LongMessage = new Command("LONG_MSG", 1);
    }
}
