using MoznyZaklad.Svet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MoznyZaklad.Data
{
    public static class SpravceUctu
    {
        private static readonly string SlozkaUctu = "Ucty";

        static SpravceUctu()
        {
            if (!Directory.Exists(SlozkaUctu)) Directory.CreateDirectory(SlozkaUctu);
        }

        // --- REGISTRACE ---
        public static bool Registrovat(string jmeno, string heslo)
        {
            if (File.Exists(ZiskejCestu(jmeno))) return false;

            var novyUcet = new UcetDto
            {
                Jmeno = jmeno,
                HesloHash = BCrypt.Net.BCrypt.HashPassword(heslo),
                AktualniMistnostId = "namesti",

                Zivoty = 100,
                MaxZivoty = 100,
                Utok = 10,
                Obrana = 2,
                Penize = 0
            };

            Uloz(novyUcet);
            return true;
        }

        // --- PŘIHLÁŠENÍ ---
        public static UcetDto? Prihlasit(string jmeno, string heslo)
        {
            var ucet = Nacti(jmeno);
            if (ucet == null) return null;

            if (BCrypt.Net.BCrypt.Verify(heslo, ucet.HesloHash))
            {
                return ucet;
            }
            return null;
        }

        // --- UKLÁDÁNÍ STAVU (Zavolej při odpojení hráče) ---
        public static void UlozStavHrace(Hrac hrac)
        {
            var ucet = Nacti(hrac.Jmeno);
            if (ucet == null) return;

            // Přepíšeme data aktuálním stavem z herního objektu
            ucet.AktualniMistnostId = hrac.AktualniMistnost.Id;
            ucet.Zivoty = hrac.Zivoty;
            ucet.MaxZivoty = hrac.MaxZivoty; 
            ucet.Utok = hrac.Utok;
            ucet.Obrana = hrac.Obrana;
            ucet.Penize = hrac.Penize;

            // Uložíme pouze ID předmětů (v HerniSvet je pak dohledáme v katalogu)
            ucet.InventarIds = hrac.Inventar.Select(p => p.Id).ToList();

            Uloz(ucet);
            Console.WriteLine($"[DEBUG]: Stav hrace {hrac.Jmeno} byl uspesne ulozen.");
        }

        // --- POMOCNÉ METODY ---
        private static string ZiskejCestu(string jmeno) => Path.Combine(SlozkaUctu, $"{jmeno.ToLower()}.json");

        private static void Uloz(UcetDto ucet)
        {
            string json = JsonSerializer.Serialize(ucet, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ZiskejCestu(ucet.Jmeno), json);
        }

        private static UcetDto? Nacti(string jmeno)
        {
            string cesta = ZiskejCestu(jmeno);
            if (!File.Exists(cesta)) return null;
            try
            {
                return JsonSerializer.Deserialize<UcetDto>(File.ReadAllText(cesta));
            }
            catch { return null; }
        }
    }
}