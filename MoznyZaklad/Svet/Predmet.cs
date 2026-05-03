using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Svet
{
    public class Predmet
    {
        public string Id { get; set; }
        public string Nazev { get; set; }
        public string Popis { get; set; }
        public string TypUcitku { get; set; } 
        public int HodnotaEfektu { get; set; } 
        public int Cena { get; set; }

        public Predmet(string id, string nazev, string popis, int cena)
        {
            Id = id;
            Nazev = nazev;
            Popis = popis;
            Cena = cena;
        }

        public override string ToString()
        {
            return Nazev;
        }
    }
}
