using System;

namespace SKS_Serwer
{
    class Program
    {
        static void Main(string[] args)
        {
            Worker worker = new Worker();
            try
            {
                worker.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }
    }
}
