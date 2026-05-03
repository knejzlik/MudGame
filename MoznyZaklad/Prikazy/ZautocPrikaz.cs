using MoznyZaklad.Svet;
using MoznyZaklad.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoznyZaklad.Prikazy
{
    public class ZautocPrikaz : IPrikaz
    {
        public string Nazev => "utoc";
        public string Napoveda => "utoc <jmeno> - zahaji boj | v boji staci psat 1, 2, 3";

        // Statická paměť probíhajících soubojů
        public static readonly Dictionary<Hrac, BojoveNPC> AktivniSouboje = new Dictionary<Hrac, BojoveNPC>();
        // Sleduje, zda hráč právě vybírá předmět z batohu
        private static readonly HashSet<Hrac> VybirajiPredmet = new HashSet<Hrac>();
        private static readonly object ZamekSouboju = new object();

        public string Proved(Hrac hrac, string[] argumenty)
        {
            lock (ZamekSouboju)
            {
                // --- 1. START BOJE ---
                if (!AktivniSouboje.ContainsKey(hrac))
                {
                    if (argumenty.Length < 2) return "Na koho chces zautocit? Zadej jmeno.";

                    string cilJmeno = string.Join(" ", argumenty.Skip(1)).ToLower();
                    var npc = hrac.AktualniMistnost.NpcPostavy
                        .FirstOrDefault(n => n.Jmeno.Equals(cilJmeno, StringComparison.OrdinalIgnoreCase));

                    if (npc is BojoveNPC protivnikStart)
                    {
                        AktivniSouboje[hrac] = protivnikStart;
                        Logger.Log($"Hrac {hrac.Jmeno} zahajil boj s {protivnikStart.Jmeno}.");
                        return $"--- BOJ ZAHÁJEN: {hrac.Jmeno} vs {protivnikStart.Jmeno} ---\n" + VratStavAMenu(hrac, protivnikStart);
                    }
                    return $"'{cilJmeno}' tu neni k boji.";
                }

                // --- 2. PRŮBĚH BOJE ---
                BojoveNPC protivnik = AktivniSouboje[hrac];
                string volba = argumenty.Length > 1 ? argumenty[1] : argumenty[0];
                StringBuilder sb = new StringBuilder();
                Random rng = new Random();

                // A) LOGIKA VÝBĚRU PŘEDMĚTU
                if (VybirajiPredmet.Contains(hrac))
                {
                    var pouzitelne = hrac.Inventar.Where(p => !string.IsNullOrEmpty(p.TypUcitku)).ToList();

                    if (int.TryParse(volba, out int index) && index > 0 && index <= pouzitelne.Count)
                    {
                        var vybranyPredmet = pouzitelne[index - 1];

                        // VOLÁME SPOLEČNOU LOGIKU Z POUZIJPRIKAZ
                        string vysledekEfektu = PouzijPrikaz.AplikujEfekt(hrac, vybranyPredmet);
                        sb.AppendLine(vysledekEfektu);

                        VybirajiPredmet.Remove(hrac);

                        // TREST: Protivník tě praští, protože jsi nedával pozor (pil jsi/jedl jsi)
                        int npcDmg = Math.Max(1, rng.Next(protivnik.Utok - 1, protivnik.Utok + 2) - hrac.Obrana);
                        hrac.Zivoty -= npcDmg;
                        sb.AppendLine($"{protivnik.Jmeno} vyuzil tve nepozornosti a zautocil za {npcDmg} dmg!");

                        if (hrac.Zivoty <= 0) return UkonciPorazkou(hrac, protivnik, sb);
                        return sb.AppendLine("\n" + VratStavAMenu(hrac, protivnik)).ToString();
                    }
                    else
                    {
                        VybirajiPredmet.Remove(hrac);
                        return "Zruseno. Zpet v boji.\n" + VratStavAMenu(hrac, protivnik);
                    }
                }

                // B) STANDARDNÍ VOLBY (1, 2, 3)
                switch (volba)
                {
                    case "1": // ÚTOK HRÁČE
                        int hracDmg = Math.Max(0, rng.Next(hrac.Utok - 2, hrac.Utok + 3) - protivnik.Obrana);
                        protivnik.Zivoty -= hracDmg;
                        sb.AppendLine($"Zasahujes {protivnik.Jmeno} za {hracDmg} dmg.");
                        break;
                    case "2": // BATOH - Čistá verze
                        var pouzitelneVeci = hrac.Inventar.Where(p => !string.IsNullOrEmpty(p.TypUcitku)).ToList();

                        if (pouzitelneVeci.Count == 0)
                        {
                            return "Nemáš u sebe žádné použitelné lektvary ani jídlo.\n" + VratStavAMenu(hrac, protivnik);
                        }

                        VybirajiPredmet.Add(hrac); // Přepneme hráče do režimu výběru

                        sb.AppendLine("\n--- MOŽNÉ PŘEDMĚTY ---");

                        for (int i = 0; i < pouzitelneVeci.Count; i++)
                        {
                            var p = pouzitelneVeci[i];
                            // Určíme popisek bonusu podle TypUcitku
                            string bonusText = p.TypUcitku.ToLower() switch
                            {
                                "leceni" => "HP",
                                "max_hp" => "Max HP",
                                "utok" => "Útok",
                                "obrana" => "Obrana",
                                _ => "Efekt"
                            };

                            // Vypíše např.: [1] Elixir sily (+5 Útok)
                            sb.AppendLine($"[{i + 1}] {p.Nazev} (+{p.HodnotaEfektu} {bonusText})");
                        }
                        sb.AppendLine("-----------------------");
                        sb.AppendLine("Zadej ČÍSLO pro použití, nebo 'X' pro návrat.");

                        return sb.ToString();
                    case "3": // ÚTĚK
                        if (rng.Next(1, 101) <= 40)
                        {
                            AktivniSouboje.Remove(hrac);
                            Logger.Log($"Hrac {hrac.Jmeno} utekl z boje s {protivnik.Jmeno}.");
                            return "[UTĚK] Utekl jsi do bezpeci.";
                        }
                        sb.AppendLine("Nepovedlo se ti utéct!");
                        break;

                    default:
                        return VratStavAMenu(hrac, protivnik);
                }

                // C) ODPOVĚĎ PROTIVNÍKA
                if (protivnik.Zivoty > 0)
                {
                    int dmg = Math.Max(1, rng.Next(protivnik.Utok - 1, protivnik.Utok + 2) - hrac.Obrana);
                    hrac.Zivoty -= dmg;
                    sb.AppendLine($"{protivnik.Jmeno} na tebe zautocil za {dmg} dmg.");

                    if (hrac.Zivoty <= 0) return UkonciPorazkou(hrac, protivnik, sb);
                    sb.AppendLine("\n" + VratStavAMenu(hrac, protivnik));
                }
                else
                {
                    // VÍTĚZSTVÍ
                    sb.AppendLine($"\n[VÍTĚZSTVÍ] {protivnik.Jmeno} padl mrtev k zemi!");
                    Logger.Log($"Hrac {hrac.Jmeno} porazil {protivnik.Jmeno}.");

                    // LOGIKA PRO DROP PŘEDMĚTU
                    if (!string.IsNullOrEmpty(protivnik.OdmenaId))
                    {
                        int hodKouskou = rng.Next(1, 101); 

                        if (hodKouskou <= protivnik.DropChance)
                        {
                            var predloha = hrac.Svet.VratVsechnyPredmety().FirstOrDefault(p => p.Id == protivnik.OdmenaId);

                            if (predloha != null)
                            {
                                Predmet novyPredmet = new Predmet(predloha.Id, predloha.Nazev, predloha.Popis, predloha.Cena)
                                {
                                    TypUcitku = predloha.TypUcitku,
                                    HodnotaEfektu = predloha.HodnotaEfektu
                                };

                                // Pokusíme se dát předmět do inventáře
                                if (hrac.Inventar.Count < hrac.MaxKapacita)
                                {
                                    hrac.Inventar.Add(novyPredmet);
                                    sb.AppendLine($"Našel jsi : **{novyPredmet.Nazev}** (přidáno do batohu).");
                                }
                                else
                                {
                                    // Inventář je plný - předmět spadne na zem do místnosti
                                    hrac.AktualniMistnost.Predmety.Add(novyPredmet);
                                    sb.AppendLine($"Našel jsi {novyPredmet.Nazev}, ale tvůj batoh je plný! Předmět leží na zemi.");
                                }
                            }
                        }
                    }

                    // Odstranění z místnosti a ukončení stavu souboje
                    hrac.AktualniMistnost.NpcPostavy.Remove(protivnik);
                    AktivniSouboje.Remove(hrac);
                }

                return sb.ToString();
            }
        }

        private string VratStavAMenu(Hrac hrac, BojoveNPC npc)
        {
            return $"STAV: {hrac.Jmeno} ({hrac.Zivoty} HP) | {npc.Jmeno} ({Math.Max(0, npc.Zivoty)} HP)\n" +
                   "AKCE: [1] Útok | [2] Batoh | [3] Útěk";
        }

        private string UkonciPorazkou(Hrac hrac, BojoveNPC protivnik, StringBuilder sb)
        {
            AktivniSouboje.Remove(hrac);
            VybirajiPredmet.Remove(hrac);
            hrac.Zivoty = 50; 
            Logger.Log($"Hrac {hrac.Jmeno} byl zabit {protivnik.Jmeno}.");
            return sb.AppendLine("\n[SMRT] Byl jsi poražen!").ToString();
        }
    }
}