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

        public static readonly Dictionary<Hrac, BojoveNPC> AktivniSouboje = new Dictionary<Hrac, BojoveNPC>();
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
                        string vysledekEfektu = PouzijPrikaz.AplikujEfekt(hrac, vybranyPredmet);
                        sb.AppendLine(vysledekEfektu);
                        VybirajiPredmet.Remove(hrac);

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
                    case "2":
                        var pouzitelneVeci = hrac.Inventar.Where(p => !string.IsNullOrEmpty(p.TypUcitku)).ToList();
                        if (pouzitelneVeci.Count == 0) return "Nemáš u sebe žádné použitelné lektvary.\n" + VratStavAMenu(hrac, protivnik);
                        VybirajiPredmet.Add(hrac);
                        sb.AppendLine("\n--- MOŽNÉ PŘEDMĚTY ---");
                        for (int i = 0; i < pouzitelneVeci.Count; i++) sb.AppendLine($"[{i + 1}] {pouzitelneVeci[i].Nazev}");
                        sb.AppendLine("-----------------------");
                        return sb.ToString();
                    case "3":
                        if (rng.Next(1, 101) <= 40) { AktivniSouboje.Remove(hrac); return "[UTĚK] Utekl jsi."; }
                        sb.AppendLine("Nepovedlo se ti utéct!");
                        break;
                    default: return VratStavAMenu(hrac, protivnik);
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
                    // --- VÍTĚZSTVÍ ---
                    sb.AppendLine($"\n[VÍTĚZSTVÍ] {protivnik.Jmeno} padl mrtev k zemi!");
                    Logger.Log($"Hrac {hrac.Jmeno} porazil {protivnik.Jmeno}.");

                    // 1. Logika pro Draka
                    if (protivnik.Jmeno.Equals("Drak", StringComparison.OrdinalIgnoreCase))
                    {
                        // VOLÁME RESPAWN PRO DRAKA (300s = 5 minut)
                        MudServer.NaplanujRespawn(hrac.AktualniMistnost, protivnik, 300);

                        AktivniSouboje.Remove(hrac);
                        hrac.AktualniMistnost.NpcPostavy.Remove(protivnik);
                        return "VYHRAL_JSI_KONEC";
                    }

                    // 2. Logika pro ostatní (Přízrak atd.)
                    // VOLÁME RESPAWN (60s = 1 minuta)
                    MudServer.NaplanujRespawn(hrac.AktualniMistnost, protivnik, 60);

                    // Drop předmětu (tvůj původní kód)
                    if (!string.IsNullOrEmpty(protivnik.OdmenaId) && rng.Next(1, 101) <= protivnik.DropChance)
                    {
                        var predloha = hrac.Svet.VratVsechnyPredmety().FirstOrDefault(p => p.Id == protivnik.OdmenaId);
                        if (predloha != null)
                        {
                            Predmet novyPredmet = new Predmet(predloha.Id, predloha.Nazev, predloha.Popis, predloha.Cena) { TypUcitku = predloha.TypUcitku, HodnotaEfektu = predloha.HodnotaEfektu };
                            if (hrac.Inventar.Count < hrac.MaxKapacita) { hrac.Inventar.Add(novyPredmet); sb.AppendLine($"Našel jsi : **{novyPredmet.Nazev}**."); }
                            else { hrac.AktualniMistnost.Predmety.Add(novyPredmet); sb.AppendLine($"Batoh je plný! {novyPredmet.Nazev} leží na zemi."); }
                        }
                    }

                    hrac.AktualniMistnost.NpcPostavy.Remove(protivnik);
                    AktivniSouboje.Remove(hrac);
                }

                return sb.ToString();
            }
        }
        // ... (metody VratStavAMenu a UkonciPorazkou zůstávají stejné)
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