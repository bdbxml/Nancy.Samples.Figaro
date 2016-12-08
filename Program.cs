using System;
using Nancy.Hosting.Self;

namespace Nancy.Demos.Figaro
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri =
                new Uri("http://localhost:3579");
            try
            {
                using (var host = new NancyHost(uri))
                {
                    host.Start();

                    Console.WriteLine("Your application is running on " + uri);
                    Console.WriteLine("Press any [Enter] to close the host.");
                    Console.ReadLine();
                }

                Console.WriteLine("Nancy host session ended.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType()}:{ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
        }
    }
}
