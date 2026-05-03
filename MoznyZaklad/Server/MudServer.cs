using MoznyZaklad.Data;
using MoznyZaklad.Prikazy;
using MoznyZaklad.Server;
using MoznyZaklad.Svet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Server
{
    public class MudServer
    {
        private readonly TcpListener myServer;
        private bool isRunning;
        private readonly HerniSvet herniSvet;
        private readonly PrikazovyProcesor procesor;

        public static readonly List<Hrac> OnlineHraci = new();
        public MudServer(int port)
        {
            herniSvet = new HerniSvet();

            procesor = new PrikazovyProcesor();

            procesor.Zaregistruj(new InventarPrikaz());
            procesor.Zaregistruj(new JdiPrikaz());
            procesor.Zaregistruj(new KrikPrikaz());
            procesor.Zaregistruj(new MluvPrikaz());
            procesor.Zaregistruj(new PolozPrikaz());
            procesor.Zaregistruj(new ProzkoumejPrikaz());
            procesor.Zaregistruj(new RekniPrikaz());
            procesor.Zaregistruj(new VezmiPrikaz());

            procesor.Zaregistruj(new ZautocPrikaz());
            procesor.Zaregistruj(new StatusPrikaz());
            procesor.Zaregistruj(new ProdejPrikaz());
            procesor.Zaregistruj(new KupPrikaz());
            procesor.Zaregistruj(new PouzijPrikaz());
            procesor.Zaregistruj(new NabidkaPrikaz());



            procesor.Zaregistruj(new PomocPrikaz(procesor.VsechnyPrikazy));

            myServer = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            myServer.Start();
            isRunning = true;

            Console.WriteLine("MUD server byl spusten.");
            _ = ServerLoop();
        }

        public void Stop()
        {
            isRunning = false;
            myServer.Stop();
            Console.WriteLine("MUD server byl ukoncen.");
        }

        private async Task ServerLoop()
        {
            while (isRunning)
            {
                try
                {
                    TcpClient client = await myServer.AcceptTcpClientAsync();
                    _ = Task.Run(() => ClientLoop(client));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Chyba serveru: {ex.Message}");
                }
            }
        }
        private async Task ClientLoop(TcpClient client)
        {
            Console.WriteLine("Klient se pripojil na server.");
            Hrac? hrac = null;

            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    UcetDto? nactenaData = null;

                    // --- 1. MENU: PŘIHLÁŠENÍ / REGISTRACE ---
                    while (nactenaData == null)
                    {
                        await writer.WriteLineAsync("\n--- MUD: KRAJOVÁ BRÁNA ---");
                        await writer.WriteLineAsync("[1] Přihlásit se");
                        await writer.WriteLineAsync("[2] Registrovat nového hrdinu");
                        await writer.WriteAsync("Vaše volba: ");
                        await writer.FlushAsync();

                        string? volba = (await reader.ReadLineAsync())?.Trim();
                        if (volba != "1" && volba != "2") continue;

                        await writer.WriteAsync("Jméno: "); await writer.FlushAsync();
                        string? jmeno = (await reader.ReadLineAsync())?.Trim();

                        await writer.WriteAsync("Heslo: "); await writer.FlushAsync();
                        string? heslo = (await reader.ReadLineAsync())?.Trim();

                        if (string.IsNullOrWhiteSpace(jmeno) || string.IsNullOrWhiteSpace(heslo))
                        {
                            await writer.WriteLineAsync("Jméno i heslo musí být vyplněno!");
                            continue;
                        }

                        if (volba == "2") // Registrace
                        {
                            if (SpravceUctu.Registrovat(jmeno, heslo))
                            {
                                await writer.WriteLineAsync("Registrace úspěšná! Nyní se přihlas.");
                                Logger.Log($"Nový uživatel registrován: {jmeno}");
                            }
                            else await writer.WriteLineAsync("Chyba: Jméno je již obsazené.");
                        }
                        else // Přihlášení
                        {
                            nactenaData = SpravceUctu.Prihlasit(jmeno, heslo);
                            if (nactenaData == null)
                            {
                                await writer.WriteLineAsync("Neplatné jméno nebo heslo.");
                                Logger.Log($"Neúspěšný pokus o login: {jmeno}");
                            }
                        }
                    }

                    // --- KONTROLA DUPLICITNÍHO PŘIHLÁŠENÍ---

                    bool uzJeOnline = false;
                    lock (OnlineHraci)
                    {
                        uzJeOnline = OnlineHraci.Any(h => h.Jmeno.Equals(nactenaData.Jmeno, StringComparison.OrdinalIgnoreCase));
                    }

                    if (uzJeOnline)
                    {
                        await writer.WriteLineAsync("Chyba: Tento ucet je jiz prihlasen z jineho mista!");
                        Logger.Log($"Odmitnuto duplicitni prihlaseni: {nactenaData.Jmeno}");
                        return;
                    }

                    // --- OŽIVENÍ HRÁČE ---
                    Mistnost mistnost = herniSvet.NajdiMistnost(nactenaData.AktualniMistnostId);

                    hrac = new Hrac(mistnost, writer, herniSvet)
                    {
                        Jmeno = nactenaData.Jmeno,
                        Zivoty = nactenaData.Zivoty,
                        MaxZivoty = nactenaData.MaxZivoty, 
                        Utok = nactenaData.Utok,
                        Obrana = nactenaData.Obrana,
                        Penize = nactenaData.Penize
                    };
                    hrac.NaplnInventar(nactenaData.InventarIds, herniSvet.VratVsechnyPredmety());

                    // Registrace do globálního seznamu
                    lock (OnlineHraci)
                    {
                        OnlineHraci.Add(hrac);
                    }

                    // Oznámení ostatním v místnosti o vstupu
                    lock (hrac.AktualniMistnost.Hraci)
                    {
                        foreach (var jinyHrac in hrac.AktualniMistnost.Hraci)
                        {
                            _ = jinyHrac.PosliZpravu($"[System]: {hrac.Jmeno} se prave pripojil do hry.");
                        }
                        hrac.AktualniMistnost.Hraci.Add(hrac);
                    }

                    Logger.Log($"Hrac vstoupil do hry: {hrac.Jmeno}");
                    await writer.WriteLineAsync($"\nAhoj, {hrac.Jmeno}. Prave jsi vstoupil do sveta.");
                    await writer.WriteLineAsync(hrac.AktualniMistnost.VratPopisMistnosti(hrac));

                    // --- HLAVNÍ SMYČKA PŘÍKAZŮ ---
                    bool clientConnected = true;
                    while (clientConnected)
                    {
                        await writer.WriteAsync("\n> ");
                        await writer.FlushAsync();

                        string? vstup = await reader.ReadLineAsync();
                        if (vstup == null) break;
                        vstup = vstup.Trim();
                        if (string.IsNullOrWhiteSpace(vstup)) continue;

                        // 1. KONTROLA: Je hráč v souboji?
                        bool vSouboji = ZautocPrikaz.AktivniSouboje.ContainsKey(hrac);

                        // 2. PŘESMĚROVÁNÍ (pro tvá čísla 1, 2, 3 a výběr předmětu)
                        if (vSouboji && int.TryParse(vstup, out _))
                        {
                            vstup = "utoc " + vstup;
                        }

                        // 3. BLOKOVÁNÍ OSTATNÍCH PŘÍKAZŮ
                        // Pokud hráč bojuje, dovolíme mu POUZE příkaz "utoc"
                        if (vSouboji && !vstup.StartsWith("utoc", StringComparison.OrdinalIgnoreCase))
                        {
                            await writer.WriteLineAsync("V boji nemuzes delat nic jineho! Soustred se na protivnika.\n" +
                                                       "Akce: [1] Utok | [2] Batoh | [3] Utek");
                            continue; // Přeskočí zpracování procesorem a začne novou smyčku
                        }

                        Logger.Log($"[{hrac.Jmeno}] Prikaz: {vstup}");

                        if (vstup.ToLower() == "konec")
                        {
                            await writer.WriteLineAsync("Ukladam stav a nashledanou...");
                            break;
                        }

                        // Provedení příkazu (sem se teď dostane buď "utoc", nebo cokoliv mimo boj)
                        string odpoved = procesor.Zpracuj(hrac, vstup);
                        if (!string.IsNullOrWhiteSpace(odpoved)) await writer.WriteLineAsync(odpoved);
                    }
                }
            }
            catch (IOException) { /* Klient se odpojil */ }
            catch (Exception ex)
            {
                Logger.Log($"Kriticka chyba u hrace {hrac?.Jmeno ?? "Neznamy"}: {ex.Message}");
            }
            finally
            {
                if (hrac != null)
                {
                    // Oznámení ostatním o odchodu
                    lock (hrac.AktualniMistnost.Hraci)
                    {
                        hrac.AktualniMistnost.Hraci.Remove(hrac);
                        foreach (var jinyHrac in hrac.AktualniMistnost.Hraci)
                        {
                            _ = jinyHrac.PosliZpravu($"[System]: {hrac.Jmeno} opustil tento svet.");
                        }
                    }

                    // Odebrání z globálního seznamu
                    lock (OnlineHraci)
                    {
                        OnlineHraci.Remove(hrac);
                    }

                    SpravceUctu.UlozStavHrace(hrac);
                    Logger.Log($"Hrac {hrac.Jmeno} se odpojil. Stav ulozen.");
                }
            }
        }
    }
}