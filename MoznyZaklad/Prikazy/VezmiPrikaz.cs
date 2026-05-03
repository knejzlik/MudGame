using MoznyZaklad.Svet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Prikazy
{
    public class VezmiPrikaz : IPrikaz
    {
        public string Nazev => "vezmi";
        public string Napoveda => "vezmi <nazev_predmetu> - sebere predmet z mistnosti podle jeho jmena";

        public string Proved(Hrac hrac, string[] argumenty)
        {
            // 1. Kontrola, zda hráč vůbec něco zadal
            if (argumenty.Length < 2) return "Co mam vzit? Musis zadat nazev predmetu.";

            // 2. Kontrola kapacity inventáře
            if (hrac.Inventar.Count >= hrac.MaxKapacita)
                return $"Mas plny inventar! (Kapacita: {hrac.MaxKapacita})";

            // 3. Spojení všech argumentů do názvu
            string nazevPredmetu = string.Join(" ", argumenty.Skip(1)).ToLower().Trim();

            // 4. Vyhledání předmětu v místnosti pouze podle názvu
            var predmet = hrac.AktualniMistnost.Predmety
                .FirstOrDefault(p => p.Nazev.ToLower() == nazevPredmetu);

            if (predmet == null)
            {
                return $"Predmet '{nazevPredmetu}' tu nevidim. Zkus 'prozkoumej' pro presny seznam věcí na zemi.";
            }

            // 5. Logika přesunu z místnosti k hráči
            hrac.AktualniMistnost.Predmety.Remove(predmet);
            hrac.Inventar.Add(predmet);

            return $"Sebral jsi: {predmet.Nazev}.";
        }
    }
}