using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Svet
{
    public class Hrac
    {
        public string Jmeno { get; set; } = "";
        public Mistnost AktualniMistnost { get; set; }

        public List<Predmet> Inventar { get; } = new List<Predmet>();
        public int MaxKapacita { get; } = 5;

        public StreamWriter Writer { get; set; }
        public int Zivoty { get; set; } = 100;    
        public int MaxZivoty { get; set; } = 100; 
        public int Utok { get; set; } = 10;
        public int Obrana { get; set; } = 2;
        public HerniSvet Svet { get; set; }
        public int Penize { get; set; } = 0;
        public List<string> DosazeneUspechy { get; set; } = new List<string>();

        public Hrac(Mistnost mistnost, StreamWriter writer, HerniSvet svet)
        {
            AktualniMistnost = mistnost;
            Writer = writer;
            Svet = svet;
        }

        public async Task PosliZpravu(string zprava)
        {
            try
            {
                await Writer.WriteLineAsync(zprava);
                await Writer.FlushAsync();
            }
            catch { /* Ošetření odpojeného hráče */ }
        }
        public void NaplnInventar(List<string> ids, List<Predmet> vsechnyPredmety)
        {
            Inventar.Clear();
            foreach (var id in ids)
            {
                var vzor = vsechnyPredmety.FirstOrDefault(p => p.Id == id);
                if (vzor != null)
                {
                    Inventar.Add(new Predmet(vzor.Id, vzor.Nazev, vzor.Popis, vzor.Cena)
                    {
                        TypUcitku = vzor.TypUcitku, 
                        HodnotaEfektu = vzor.HodnotaEfektu 
                    });
                }
            }
        }
    }
}
