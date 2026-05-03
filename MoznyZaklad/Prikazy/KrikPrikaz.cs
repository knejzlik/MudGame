using MoznyZaklad.Server;
using MoznyZaklad.Svet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Prikazy
{
    public class KrikPrikaz : IPrikaz
    {
        public string Nazev => "krik";
        public string Napoveda => "krik <text> - posle zpravu uplne vsem na serveru";

        public string Proved(Hrac hrac, string[] argumenty)
        {
            if (argumenty.Length < 2) return "Co chces vykriknout?";

            string zprava = string.Join(" ", argumenty.Skip(1));
            string finalni = $"{hrac.Jmeno} krici: {zprava} ";

            lock (MudServer.OnlineHraci)
            {
                foreach (var prijemce in MudServer.OnlineHraci)
                {
                    //všem kromě odesílatele
                    if (prijemce != hrac)
                    {
                        // Používáme discard _, protože PosliZpravu je async Task
                        _ = prijemce.PosliZpravu(finalni);
                    }
                }
            }

            return $"Vykrikl jsi: {zprava}";
        }
    }
}
