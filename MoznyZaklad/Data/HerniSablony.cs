using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Data
{
    public class MistnostDto
    {
        public string Id { get; set; }
        public string Nazev { get; set; }
        public string Popis { get; set; }
        public List<string> Vychody { get; set; }
        public List<string> PredmetIds { get; set; }
        public List<string> NpcIds { get; set; }
    }

    public class PredmetDto
    {
        public string Id { get; set; }
        public string Nazev { get; set; }
        public string Popis { get; set; }
        public string TypUcitku { get; set; }
        public int HodnotaEfektu { get; set; }
        public int Cena { get; set; }
    }

    public class NpcDto
    {
        public string Id { get; set; }
        public string Typ { get; set; }
        public string Jmeno { get; set; }
        public string Popis { get; set; }
        // Jen pro obchodníka
        public string Uvitani { get; set; }
        public List<string> ZboziIds { get; set; } = new List<string>();
        // pro MluviciNPC
        public string Text { get; set; }


        // pro BojoveNPC
        public int Zivoty { get; set; }
        public int Utok { get; set; }
        public int Obrana { get; set; }
        public int DropChance { get; set; }
        public string OdmenaId { get; set; }
    }


    public class UcetDto
    {
        public string Jmeno { get; set; }
        public string HesloHash { get; set; }
        public string AktualniMistnostId { get; set; }

        // Statistiky
        public int Zivoty { get; set; }
        public int MaxZivoty { get; set; } 
        public int Utok { get; set; }
        public int Obrana { get; set; }
        public int Penize { get; set; }

        // Seznam předmětů v inventáři
        public List<string> InventarIds { get; set; } = new List<string>();
    }

}


