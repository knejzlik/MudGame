using MoznyZaklad.Svet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Prikazy
{
    public class NabidkaPrikaz : IPrikaz
    {
        public string Nazev => "nabidka";
        public string Napoveda => "nabidka - zobrazi seznam zbozi u mistniho obchodnika";

        public string Proved(Hrac hrac, string[] argumenty)
        {
            // 1. Najdeme obchodníka v aktuální místnosti
            var obchodnik = hrac.AktualniMistnost.NpcPostavy.OfType<ObchodnikNPC>().FirstOrDefault();

            if (obchodnik == null)
            {
                return "Nenachazi se tu zadny obchodnik.";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"--- Nabidka obchodnika: {obchodnik.Jmeno} ---");
            sb.AppendLine($"\"{obchodnik.UvitaciZprava}\"");
            sb.AppendLine(new string('-', 45));
            sb.AppendLine($"{"ID",-4} | {"Predmet",-20} | {"Cena",-10}");
            sb.AppendLine(new string('-', 45));

            // 2. Procházíme seznam zboží obchodníka pomocí indexu
            for (int i = 0; i < obchodnik.ZboziIds.Count; i++)
            {
                string idPredmetu = obchodnik.ZboziIds[i];

                // Najdeme data o předmětu v katalogu světa
                var item = hrac.Svet.VratVsechnyPredmety().FirstOrDefault(p => p.Id == idPredmetu);

                if (item != null)
                {
                    // Vypíšeme [index + 1], název a cenu
                    sb.AppendLine($"[{i + 1,2}] | {item.Nazev,-20} | {item.Cena,7} zl.");
                }
            }

            sb.AppendLine(new string('-', 45));
            sb.AppendLine($"Tve penize: {hrac.Penize} zlataku.");
            sb.AppendLine("Pro nakup pouzij prikaz: kup <cislo>");

            return sb.ToString();
        }
    }
}