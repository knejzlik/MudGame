using MoznyZaklad.Svet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Prikazy
{
    public class InventarPrikaz : IPrikaz
    {
        public string Nazev => "inventar";
        public string Napoveda => "inventar - vypise obsah a kapacitu tveho batohu";

        public string Proved(Hrac hrac, string[] argumenty)
        {
            if (hrac.Inventar.Count == 0)
            {
                return $"V inventari mas (0/{hrac.MaxKapacita}): prazdno";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"V inventari mas ({hrac.Inventar.Count}/{hrac.MaxKapacita}):");

            // Projdeme inventář a přidáme index 
            for (int i = 0; i < hrac.Inventar.Count; i++)
            {
                var predmet = hrac.Inventar[i];
                sb.AppendLine($" [{i + 1}] {predmet.Nazev}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}