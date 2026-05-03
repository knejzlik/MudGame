using MoznyZaklad.Prikazy;
using MoznyZaklad.Svet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoznyZaklad.Prikazy
{
    public class KupPrikaz : IPrikaz
    {
        public string Nazev => "kup";
        public string Napoveda => "kup <číslo> - koupí předmět z nabídky obchodníka (např. kup 1)";

        public string Proved(Hrac hrac, string[] argumenty)
        {
            if (argumenty.Length < 2) return "Co si chceš koupit? Zadej číslo z nabídky.";

            // Najdeme obchodníka v místnosti
            var obchodnik = hrac.AktualniMistnost.NpcPostavy.OfType<ObchodnikNPC>().FirstOrDefault();
            if (obchodnik == null) return "Není tu nikdo, kdo by prodával.";

            var nabidkaZbozi = hrac.Svet.VratVsechnyPredmety()
                .Where(p => obchodnik.ZboziIds.Contains(p.Id))
                .ToList();

            if (nabidkaZbozi.Count == 0) return "Tento obchodník momentálně nemá nic na skladě.";

            if (!int.TryParse(argumenty[1], out int volba) || volba < 1 || volba > nabidkaZbozi.Count)
            {
                return $"Neplatná volba. Zadej číslo od 1 do {nabidkaZbozi.Count}. (Podívej se na 'nabidka')";
            }

            var predloha = nabidkaZbozi[volba - 1];

            //Kontroly (peníze a kapacita)
            if (hrac.Penize < predloha.Cena)
                return $"Máš málo peněz! {predloha.Nazev} stojí {predloha.Cena} zlaťáků, ale ty máš jen {hrac.Penize}.";

            if (hrac.Inventar.Count >= hrac.MaxKapacita)
                return "Máš plný inventář, už se ti tam nic nevejde.";

            hrac.Penize -= predloha.Cena;

            Predmet novyPredmet = new Predmet(predloha.Id, predloha.Nazev, predloha.Popis, predloha.Cena)
            {
                TypUcitku = predloha.TypUcitku,
                HodnotaEfektu = predloha.HodnotaEfektu
            };

            hrac.Inventar.Add(novyPredmet);

            return $"Koupil jsi {novyPredmet.Nazev} za {predloha.Cena} zlaťáků. Zbývá ti {hrac.Penize}.";
        }
    }
}