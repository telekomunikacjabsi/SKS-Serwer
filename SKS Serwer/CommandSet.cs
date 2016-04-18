
namespace SKS_Serwer
{
    public static class CommandSet
    {
        // liczby oznaczają liczbę argumentów danej komendy
        public static readonly Command AuthFail = new Command("AUTH;FAIL");
        public static readonly Command AuthSuccess = new Command("AUTH;SUCCESS");
        public static readonly Command Connect = new Command("CONNECT", 2);
        public static readonly Command Disconnect = new Command("DISCONNECT");
        public static readonly Command VerifyList = new Command("VERIFYLIST", 2);
        public static readonly Command OK = new Command("OK");
        public static readonly Command List = new Command("LIST", 2);
    }
}
