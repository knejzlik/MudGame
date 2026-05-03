using MoznyZaklad.Svet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Prikazy
{
    public class ProzkoumejPrikaz : IPrikaz
    {
        public string Nazev => "prozkoumej";
        public string Napoveda => "prozkoumej [cislo] - vypise popis mistnosti nebo konkretniho predmetu";

        public string Proved(Hrac hrac, string[] argumenty)
        {
            // 1. Pokud hrac nezada zadny argument, vypiseme popis cele mistnosti
            if (argumenty.Length < 2)
            {
                return hrac.AktualniMistnost.VratPopisMistnosti(hrac);
            }

            // 2. Pokud zada cislo, zkusime najst predmet v mistnosti podle indexu
            if (int.TryParse(argumenty[1], out int index))
            {
                // Kontrola v seznamu predmetu v mistnosti
                if (index >= 1 && index <= hrac.AktualniMistnost.Predmety.Count)
                {
                    var predmet = hrac.AktualniMistnost.Predmety[index - 1];
                    return $"Predmet: {predmet.Nazev}\n{predmet.Popis}";
                }

                return "Predmet s timto cislem tu nevidis.";
            }

            return "Neplatny argument. Pouzij 'prozkoumej' pro mistnost nebo 'prozkoumej <cislo>' pro predmet.";
        }
    }
}