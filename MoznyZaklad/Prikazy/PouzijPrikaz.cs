using MoznyZaklad.Prikazy;
using MoznyZaklad.Svet;

public class PouzijPrikaz : IPrikaz
{
    public string Nazev => "pouzij";
    public string Napoveda => "pouzij <cislo> - pouzije predmet";

    public string Proved(Hrac hrac, string[] argumenty)
    {
        if (argumenty.Length < 2) return "Co chces pouzit?";
        if (!int.TryParse(argumenty[1], out int index) || index < 1 || index > hrac.Inventar.Count)
            return "Neplatne cislo predmetu.";

        var predmet = hrac.Inventar[index - 1];

        return AplikujEfekt(hrac, predmet);
    }

    public static string AplikujEfekt(Hrac hrac, Predmet predmet)
    {
        string typ = predmet.TypUcitku?.ToLower() ?? "";
        if (string.IsNullOrEmpty(typ)) return $"{predmet.Nazev} nema zadny efekt.";

        string vysledek = "";
        switch (typ)
        {
            case "leceni":
                if (hrac.Zivoty >= hrac.MaxZivoty) return "Mas plne zdravi.";
                hrac.Zivoty = Math.Min(hrac.MaxZivoty, hrac.Zivoty + predmet.HodnotaEfektu);
                vysledek = $"Vyleceno {predmet.HodnotaEfektu} HP.";
                break;
            case "max_hp":
                hrac.MaxZivoty += predmet.HodnotaEfektu;
                hrac.Zivoty += predmet.HodnotaEfektu;
                vysledek = $"Maximalni zdravi zvyseno na {hrac.MaxZivoty} HP.";
                break;
            case "utok":
                hrac.Utok += predmet.HodnotaEfektu;
                vysledek = $"Utok zvysen o {predmet.HodnotaEfektu}.";
                break;
            case "obrana":
                hrac.Obrana += predmet.HodnotaEfektu;
                vysledek = $"Obrana zvysena o {predmet.HodnotaEfektu}.";
                break;
            default:
                return "Tento predmet neumi udelat nic uzitecneho.";
        }

        hrac.Inventar.Remove(predmet);
        return $"Pouzil jsi {predmet.Nazev}. {vysledek}";
    }
}