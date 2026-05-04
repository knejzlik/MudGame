using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MoznyZaklad.Svet;

namespace MoznyZaklad.Data
{
    public static class SpravceUspechu
    {
        public static List<UspechDefinice> DefiniceUspechu = new();
        private static readonly string CestaUspechy = "Data/Uspechy.json";

        static SpravceUspechu()
        {
            NactiDefinice();
        }

        public static void NactiDefinice()
        {
            try
            {
                if (File.Exists(CestaUspechy))
                {
                    string json = File.ReadAllText(CestaUspechy);
                    DefiniceUspechu = JsonSerializer.Deserialize<List<UspechDefinice>>(json) ?? new();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při načítání úspěchů: {ex.Message}");
            }
        }

        public static async Task Zkontroluj(Hrac hrac, string eventTyp, string eventData = "")
        {
            // Vybereme jen ty, které hráč ještě nemá
            var kOvereni = DefiniceUspechu.Where(d => !hrac.DosazeneUspechy.Contains(d.Id)).ToList();

            foreach (var def in kOvereni)
            {
                bool splneno = false;

                switch (def.Typ)
                {
                    case "navstiv_mistnost":
                        if (hrac.AktualniMistnost.Id == def.Cil) splneno = true;
                        break;

                    case "ziskej_stats":
                        if (int.TryParse(def.Cil, out int potrebnaSila) && hrac.Utok >= potrebnaSila) splneno = true;
                        break;

                    case "poraz_draka":
                        if (eventTyp == "zabiti_npc" && eventData.Equals(def.Cil, StringComparison.OrdinalIgnoreCase))
                            splneno = true;
                        break;
                }

                if (splneno)
                {
                    hrac.DosazeneUspechy.Add(def.Id);
                    hrac.Penize += def.Odmena;

                    await hrac.PosliZpravu("\n***************************************************");
                    await hrac.PosliZpravu($"🏆 USPECH ODEMČEN: {def.Nazev}");
                    await hrac.PosliZpravu($"💰 ODMĚNA: {def.Odmena} zlatáků");
                    await hrac.PosliZpravu("***************************************************\n");
                    SpravceUctu.UlozStavHrace(hrac);
                }
            }
        }
    }
}