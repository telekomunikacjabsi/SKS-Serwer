
namespace SKS_Serwer
{
    public static class ThreadLocker
    {
        public static object Lock { get; }

        static ThreadLocker()
        {
            Lock = new object();
        }
    }
}
