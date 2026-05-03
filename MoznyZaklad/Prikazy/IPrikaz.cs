using MoznyZaklad.Svet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoznyZaklad.Prikazy
{
    public interface IPrikaz
    {
        string Nazev { get; }
        string Napoveda { get; }
        string Proved(Hrac hrac, string[] argumenty);
    }
}
