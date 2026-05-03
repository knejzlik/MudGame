using MoznyZaklad.Svet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Prikazy
{
    public class PrikazovyProcesor
    {
        private readonly Dictionary<string, IPrikaz> _prikazy = new();

        public void Zaregistruj(IPrikaz prikaz)
        {
            _prikazy[prikaz.Nazev] = prikaz;
        }

        public IEnumerable<IPrikaz> VsechnyPrikazy => _prikazy.Values;

        public string Zpracuj(Hrac hrac, string vstup)
        {
            if (string.IsNullOrWhiteSpace(vstup))
                return "";

            string[] casti = vstup.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string nazevPrikazu = casti[0].ToLower();

            if (_prikazy.ContainsKey(nazevPrikazu))
            {
                return _prikazy[nazevPrikazu].Proved(hrac, casti);
            }

            return "Neznamy prikaz. Zadej 'pomoc'.";
        }
    }
}
