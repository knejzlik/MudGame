using MoznyZaklad.Server;

namespace MoznyZaklad
{
    public class Program
    {
        static void Main(string[] args)
        {
            int port = 65525;

            if (args.Length > 0 && int.TryParse(args[0], out int parsedPort))
            {
                port = parsedPort;
            }

            MudServer server = new MudServer(port);
            server.Start();

            Console.WriteLine($"Server nasloucha na portu {port}.");
            Console.WriteLine("Stiskni Enter pro ukonceni serveru.");
            Console.ReadLine();

            server.Stop();
        }
    }
}
