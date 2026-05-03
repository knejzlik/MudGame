using MoznyZaklad.Svet;
using System.Text;

namespace MoznyZaklad.Prikazy
{
    public class StatusPrikaz : IPrikaz
    {
        public string Nazev => "status";
        public string Napoveda => "status - zobrazi tve aktualni statistiky (zivoty, utok, obranu a penize)";

        public string Proved(Hrac hrac, string[] argumenty)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"--- STATISTIKY HRACE: {hrac.Jmeno} ---");

            sb.AppendLine($"Zivoty:  {hrac.Zivoty} / {hrac.MaxZivoty} HP");
            sb.AppendLine($"Utok:    {hrac.Utok}");
            sb.AppendLine($"Obrana:  {hrac.Obrana}");
            sb.AppendLine($"Penize:  {hrac.Penize} zlataku");
            sb.AppendLine("------------------------------------");
            sb.AppendLine($"Inventar: {hrac.Inventar.Count} / {hrac.MaxKapacita} predmetu.");

            return sb.ToString();
        }
    }
}