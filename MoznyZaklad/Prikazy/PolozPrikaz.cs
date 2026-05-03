using MoznyZaklad.Svet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Prikazy
{
    public class PolozPrikaz : IPrikaz
    {
        public string Nazev => "poloz";
        public string Napoveda => "poloz <cislo> - odlozi predmet z inventare (cislo zjistis v 'inventar')";

        public string Proved(Hrac hrac, string[] argumenty)
        {
            if (argumenty.Length < 2) return "Co chces polozit? Zadej cislo z inventare.";

            // 1. Prevod argumentu na index
            if (!int.TryParse(argumenty[1], out int index) || index < 1 || index > hrac.Inventar.Count)
            {
                return "Neplatne cislo predmetu. Podivej se do 'inventar'.";
            }

            var predmet = hrac.Inventar[index - 1];

            // 3. Presun predmetu z inventare do mistnosti
            hrac.Inventar.Remove(predmet);
            hrac.AktualniMistnost.Predmety.Add(predmet);

            return $"Polozil jsi {predmet.Nazev}.";
        }
    }
}