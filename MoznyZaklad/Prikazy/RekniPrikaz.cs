using MoznyZaklad.Svet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Prikazy
{
    public class RekniPrikaz : IPrikaz
    {
        public string Nazev => "rekni";
        public string Napoveda => "rekni <zprava> - posle zpravu vsem v mistnosti";

        public string Proved(Hrac hrac, string[] argumenty)
        {
            if (argumenty.Length < 2) return "Co chces rict?";
            string zprava = string.Join(" ", argumenty.Skip(1));

            // kdyby někdo během posílání zprávy z místnosti odešel.
            lock (hrac.AktualniMistnost.Hraci)
            {
                foreach (var prijemce in hrac.AktualniMistnost.Hraci)
                {
                    if (prijemce != hrac)
                    {
                        // Používáme discard, protože odeslání běží na pozadí
                        _ = prijemce.PosliZpravu($"[{hrac.Jmeno} rika]: {zprava}");
                    }
                }
            }

            return $"Rekl jsi: {zprava}";
        }
    }
}