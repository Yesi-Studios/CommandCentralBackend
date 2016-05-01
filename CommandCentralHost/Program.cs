using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CDBServiceHost
{
    class Program
    {

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("It's awfully lonely in here now...");

                var session = CommandCentral.DataAccess.SessionProvider.CreateSession();

                Console.ReadKey();
            }
            catch
            {
                
                throw;
            }
        }

    }
}
