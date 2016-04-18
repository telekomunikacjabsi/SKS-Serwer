using System;

namespace SKS_Serwer
{
    class Program
    {
        static void Main(string[] args)
        {
            Worker worker;
            try
            {
                worker = new Worker();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }
    }
}
