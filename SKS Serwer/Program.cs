using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
