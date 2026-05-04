using MoznyZaklad.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoznyZaklad.Svet
{
    public class HerniSvet
    {
        private Dictionary<string, Mistnost> vsechnyMistnosti = new();
        public Dictionary<string, Predmet> vsechnyPredmety = new();
        public Mistnost StartovniMistnost { get; private set; } = null!;

        public HerniSvet()
        {
            // 1. Načtení surovin z JSONů
            var mData = JsonLoader.NactiData<List<MistnostDto>>("Data/mistnosti.json") ?? new();
            var pData = JsonLoader.NactiData<List<PredmetDto>>("Data/predmety.json") ?? new();
            var nData = JsonLoader.NactiData<List<NpcDto>>("Data/NPC.json") ?? new();

            // 2. Vytvoření herních objektů
            vsechnyPredmety = pData.ToDictionary(p => p.Id, p => new Predmet(p.Id, p.Nazev, p.Popis, p.Cena)
            {
                TypUcitku = p.TypUcitku,
                HodnotaEfektu = p.HodnotaEfektu
            });

            // Načítání NPC s rozlišením typů
            var dictNpcs = new Dictionary<string, NPC>();
            foreach (var n in nData)
            {
                string typ = n.Typ?.ToLower() ?? "";

                if (typ == "mluvici")
                {
                    dictNpcs[n.Id] = new MluviciNPC(n.Jmeno, n.Popis, n.Text);
                }
                else if (typ == "bojove")
                {
                    dictNpcs[n.Id] = new BojoveNPC(n.Jmeno, n.Popis, n.Zivoty, n.Utok, n.Obrana, n.DropChance, n.OdmenaId);
                }
                else if (typ == "obchodnik")
                {
                    dictNpcs[n.Id] = new ObchodnikNPC(n.Jmeno, n.Popis, n.Uvitani, n.ZboziIds);
                }
            }

            vsechnyMistnosti = mData.ToDictionary(m => m.Id, m => new Mistnost(m.Id, m.Nazev, m.Popis));

            // 3. Propojování objektů
            foreach (var dto in mData)
            {
                if (!vsechnyMistnosti.TryGetValue(dto.Id, out var mistnost)) continue;

                // Propojení východů
                if (dto.Vychody != null)
                {
                    foreach (var vId in dto.Vychody)
                    {
                        if (vsechnyMistnosti.TryGetValue(vId, out var cilova))
                        {
                            mistnost.PridejSousedniMistnost(cilova);
                        }
                    }
                }

                // Přidání předmětů do místností
                if (dto.PredmetIds != null)
                {
                    foreach (var pId in dto.PredmetIds)
                    {
                        if (vsechnyPredmety.TryGetValue(pId, out var predmet))
                            mistnost.Predmety.Add(predmet);
                    }
                }

                // Přidání NPC do místností
                if (dto.NpcIds != null)
                {
                    foreach (var nId in dto.NpcIds)
                    {
                        if (dictNpcs.TryGetValue(nId, out var npc))
                            mistnost.NpcPostavy.Add(npc);
                    }
                }
            }

            // Nastavení startovní místnosti
            StartovniMistnost = vsechnyMistnosti.ContainsKey("namesti")
                ? vsechnyMistnosti["namesti"]
                : vsechnyMistnosti.Values.FirstOrDefault()!;

        }

        public Mistnost NajdiMistnost(string id)
        {
            if (string.IsNullOrEmpty(id) || !vsechnyMistnosti.ContainsKey(id))
            {
                return StartovniMistnost;
            }
            return vsechnyMistnosti[id];
        }

        public List<Predmet> VratVsechnyPredmety() => vsechnyPredmety.Values.ToList();
    }
}