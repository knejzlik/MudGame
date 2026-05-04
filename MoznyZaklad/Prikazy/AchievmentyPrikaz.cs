using MoznyZaklad.Data;
using MoznyZaklad.Svet;
using System.Text;
using System.Linq;

namespace MoznyZaklad.Prikazy
{
    public class AchievmentyPrikaz : IPrikaz
    {
        public string Nazev => "uspechy";
        public string Napoveda => "uspechy - zobrazí tvou sbírku získaných trofejí";

        public string Proved(Hrac hrac, string[] argumenty)
        {
            // 1. Zjistíme počty
            int pocetDosazenych = hrac.DosazeneUspechy?.Count ?? 0;
            int celkovyPocet = SpravceUspechu.DefiniceUspechu.Count;

            // 2. Pokud hráč nic nemá, i tak mu ukážeme progres 0/X
            if (pocetDosazenych == 0)
            {
                return $"\nZatím jsi nic nedosáhl. (Odemčeno: 0/{celkovyPocet})\nVydej se do světa!";
            }

            StringBuilder sb = new StringBuilder();
            // 3. Přidáme řádek s progresem hned do nadpisu
            sb.AppendLine($"\n --- TVÉ DOSAŽENÉ ÚSPĚCHY ({pocetDosazenych}/{celkovyPocet}) ---");

            foreach (var idUspechu in hrac.DosazeneUspechy)
            {
                var definice = SpravceUspechu.DefiniceUspechu.FirstOrDefault(d => d.Id == idUspechu);

                if (definice != null)
                {
                    sb.AppendLine($"- **{definice.Nazev}** (Odměna: {definice.Odmena} zl.)");
                }
                else
                {
                    sb.AppendLine($"- {idUspechu}");
                }
            }

            sb.AppendLine("--------------------------------");
            return sb.ToString();
        }
    }
}