using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Prikazy
{
    using MoznyZaklad.Svet;
    using System.Text;

    public class JdiPrikaz : IPrikaz
    {
        public string Nazev => "jdi";
        public string Napoveda => "jdi <jmeno_mistnosti> - presune hrace do sousedni mistnosti";

        public string Proved(Hrac hrac, string[] argumenty)
        {
            if (argumenty.Length < 2) return "Pouziti: jdi <mistnost>";

            string cilKlic = argumenty[1].ToLower();
            if (!hrac.AktualniMistnost.SousedniMistnosti.ContainsKey(cilKlic))
                return "Tudy cesta nevede.";

            Mistnost puvodniMistnost = hrac.AktualniMistnost;
            Mistnost novaMistnost = puvodniMistnost.SousedniMistnosti[cilKlic];

            // OZNÁMENÍ O ODCHODU 
            lock (puvodniMistnost.Hraci)
            {
                puvodniMistnost.Hraci.Remove(hrac);

                foreach (var jinyHrac in puvodniMistnost.Hraci)
                {
                    // Používáme discard _, protože PosliZpravu je async Task
                    _ = jinyHrac.PosliZpravu($"[System]: {hrac.Jmeno} odesel smerem k {novaMistnost.Nazev}.");
                }
            }

            hrac.AktualniMistnost = novaMistnost;

            // OZNÁMENÍ O PŘÍCHODU 
            lock (novaMistnost.Hraci)
            {
                foreach (var jinyHrac in novaMistnost.Hraci)
                {
                    _ = jinyHrac.PosliZpravu($"[System]: {hrac.Jmeno} prave prisel do mistnosti.");
                }

                novaMistnost.Hraci.Add(hrac);
            }

            return $"Vstoupil jsi do: {novaMistnost.Nazev}\n\n" + novaMistnost.VratPopisMistnosti(hrac);
        }
    }
}