using Newtonsoft.Json;
using System;
using System.Security.Cryptography;

namespace DiscordAutismParser
{
    class Program
    {
        public static void Main()
        {
            new WelcomeCommand().Run(null);

            while (true)
            {
                Console.Write(">");
                var input = Console.ReadLine();
                Command.ParseAndRun(input);
            }
        }



        private static void foo()
        { 
            
        }
    }
}