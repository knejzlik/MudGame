using MoznyZaklad.Svet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Prikazy
{
    public class MluvPrikaz : IPrikaz
    {
        public string Nazev => "mluv";
        public string Napoveda => "mluv <jmeno_npc> - zahaji rozhovor s postavou v mistnosti";

        public string Proved(Hrac hrac, string[] argumenty)
        {
            if (argumenty.Length < 2)
            {
                return "S kym chces mluvit? Musis zadat jmeno postavy.";
            }

            string cilJmeno = string.Join(" ", argumenty.Skip(1));

            // Vyhledani NPC v mistnosti
            var npc = hrac.AktualniMistnost.NpcPostavy
                .FirstOrDefault(n => n.Jmeno.Equals(cilJmeno, StringComparison.OrdinalIgnoreCase));

            if (npc == null)
            {
                return $"Nikdo jmenem '{cilJmeno}' tu neni.";
            }

            // Kontrola typu pro mluvící NPC
            if (npc is MluviciNPC mluvici)
            {
                return $"{mluvici.Jmeno} se na tebe podiva a rika: \"{mluvici.Dialog}\"";
            }

            // Pokud hrac zkousi mluvit s Bojovym NPC
            if (npc is BojoveNPC)
            {
                return $"{npc.Jmeno} na tebe jen nebezpecne vrci. Tohle na rozhovor nevypada.";
            }

            return $"{npc.Jmeno} s tebou momentalne nechce mluvit.";
        }
    }
}