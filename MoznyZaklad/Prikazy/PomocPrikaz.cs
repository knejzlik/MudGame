using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Prikazy
{
    using MoznyZaklad.Svet;
    using System.Text;

    public class PomocPrikaz : IPrikaz
    {
        private readonly IEnumerable<IPrikaz> _prikazy;

        public PomocPrikaz(IEnumerable<IPrikaz> prikazy)
        {
            _prikazy = prikazy;
        }

        public string Nazev => "pomoc";
        public string Napoveda => "pomoc - vypise dostupne prikazy";

        public string Proved(Hrac hrac, string[] argumenty)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== Dostupne prikazy ===");

            foreach (var prikaz in _prikazy.OrderBy(p => p.Nazev))
            {
                sb.AppendLine($"- {prikaz.Napoveda}");
            }


            sb.AppendLine("- konec - ukonci spojeni se serverem");

            return sb.ToString();
        }
    }
}