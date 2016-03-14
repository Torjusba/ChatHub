using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatHub
{
    class Program
    {
        static void Main(string[] mainArgs)
        {
            while (true)
                {
                string command = Console.ReadLine().ToLower();
                string[] args = command.Split(' ');
                switch (args[0])
                {
                    case "#startserver":
                        try
                        {
                            Server server = new Server(Convert.ToInt32(args[1]));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        break;

                    case "#connect":
                        try
                        {
                            Client client = new Client(args[1], Convert.ToInt32(args[2]));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        break;
                }
            }
        }
    }
}
