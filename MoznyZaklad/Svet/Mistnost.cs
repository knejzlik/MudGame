using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoznyZaklad.Svet
{
    public class Mistnost
    {
        public string Id { get; set; }
        public string Nazev { get; set; }
        public string Popis { get; set; }

        public Dictionary<string, Mistnost> SousedniMistnosti { get; } = new();
        public List<Predmet> Predmety { get; } = new();
        public List<Hrac> Hraci { get; } = new();
        public List<NPC> NpcPostavy { get; } = new();

        public Mistnost(string id, string nazev, string popis)
        {
            Id = id;
            Nazev = nazev;
            Popis = popis;
        }

        public void PridejSousedniMistnost(Mistnost mistnost)
        {
            SousedniMistnosti[mistnost.Nazev.ToLower()] = mistnost;
        }

        public string VratPopisMistnosti(Hrac aktualniHrac)
        {
            StringBuilder sb = new StringBuilder();

            // 1. ZÁHLAVÍ: Název a popis místnosti
            sb.AppendLine($"\n== {Nazev.ToUpper()} ==");
            sb.AppendLine(Popis);
            sb.AppendLine();

            // 2. VÝCHODY: Kam se dá jít
            sb.AppendLine("Dostupne vychody:");
            if (SousedniMistnosti.Count == 0)
                sb.AppendLine("- zadne");
            else
                sb.AppendLine("- " + string.Join(", ", SousedniMistnosti.Keys));
            sb.AppendLine();

            // 3. PŘEDMĚTY: Co leží na zemi 
            sb.AppendLine("Predmety na zemi:");
            if (Predmety.Count == 0)
            {
                sb.AppendLine("- zadne");
            }
            else
            {
                sb.AppendLine("- " + string.Join(", ", Predmety.Select(p => p.Nazev)));
            }
            sb.AppendLine();

            // 4. POSTAVY (NPC): Kdo tu stojí
            sb.AppendLine("Osoby a nestvury:");
            if (NpcPostavy.Count == 0)
            {
                sb.AppendLine("- nikdo");
            }
            else
            {
                foreach (var npc in NpcPostavy)
                {
                    sb.AppendLine($"- {npc.Jmeno} ({npc.Popis})");
                }
            }
            sb.AppendLine();

            // 5. OSTATNÍ HRÁČI: Kdo je tu online s tebou
            sb.AppendLine("Ostatni hraci:");
            var ostatni = Hraci.Where(h => h != aktualniHrac).Select(h => h.Jmeno).ToList();
            sb.AppendLine(ostatni.Count == 0 ? "- nikdo dalsi" : "- " + string.Join(", ", ostatni));

            return sb.ToString();
        }
    }
}