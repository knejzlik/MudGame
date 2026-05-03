using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Klient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "MUD Klient";
            // Použij IP adresu počítače
            string ip = "127.0.0.1";
            int port = 65525; 

            try
            {
                using TcpClient client = new TcpClient();
                Console.WriteLine($"Pripojuji se k serveru {ip}:{port}...");
                await client.ConnectAsync(ip, port);
                Console.Clear();
                Console.WriteLine("--- JSI PRIPOJEN K SERVERU ---");

                using NetworkStream stream = client.GetStream();
                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                _ = Task.Run(async () =>
                {
                    try
                    {
                        char[] buffer = new char[1024];
                        while (true)
                        {
                            int nacteno = await reader.ReadAsync(buffer, 0, buffer.Length);
                            if (nacteno == 0) break;
                            Console.Write(new string(buffer, 0, nacteno));
                        }
                    }
                    catch { Console.WriteLine("\n[System] Spojeni se serverem bylo preruseno."); }
                });

                while (true)
                {
                    string? vstup = Console.ReadLine();
                    if (vstup == null) break;

                    await writer.WriteLineAsync(vstup);
                    await writer.FlushAsync(); 

                    if (vstup.ToLower() == "konec") break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba pripojeni: {ex.Message}");
                Console.WriteLine("Zkontrolujte, zda bezi server.");
                Console.ReadKey();
            }
        }
    }
}