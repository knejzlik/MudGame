using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Svet
{
    public abstract class NPC
    {
        public string Jmeno { get; set; }
        public string Popis { get; set; }

        public NPC(string jmeno, string popis)
        {
            Jmeno = jmeno;
            Popis = popis;
        }
    }

    public class MluviciNPC : NPC
    {
        public string Dialog { get; set; }
        public MluviciNPC(string jmeno, string popis, string dialog) : base(jmeno, popis)
        {
            Dialog = dialog;
        }
    }

    public class BojoveNPC : NPC
    {
        public int Zivoty { get; set; }
        public int Utok { get; set; }
        public int Obrana { get; set; }
        public string OdmenaId { get; set; }
        public int DropChance { get; set; } 

        public BojoveNPC(string jmeno, string popis, int zivoty, int utok, int obrana, int dropChance, string odmenaId = null)
            : base(jmeno, popis)
        {
            Zivoty = zivoty;
            Utok = utok;
            Obrana = obrana;
            DropChance = dropChance; 
            OdmenaId = odmenaId;
        }
    }
    public class ObchodnikNPC : NPC
    {
        public string UvitaciZprava { get; set; }
        public List<string> ZboziIds { get; set; } = new List<string>();

        public ObchodnikNPC(string jmeno, string popis, string uvitani, List<string> zbozi) : base(jmeno, popis)
        {
            UvitaciZprava = uvitani;
            ZboziIds = zbozi ?? new List<string>();
        }
    }
}