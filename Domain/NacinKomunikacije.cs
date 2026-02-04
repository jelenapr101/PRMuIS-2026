using System;
using System.Net;
using System.Net.Sockets;

namespace Domain
{
    public class NacinKomunikacije
    {
        public Socket UticnicaKlijenta { get; set; }

        public EndPoint AdresaKlijenta { get; set; }

        public string Algoritam { get; set; }

        public string Kljuc { get; set; }

        public NacinKomunikacije(Socket uticnica, string algoritam, string kljuc)
        {
            UticnicaKlijenta = uticnica;
            Algoritam = algoritam;
            Kljuc = kljuc;
        }

        public NacinKomunikacije(EndPoint adresa, string algoritam, string kljuc)
        {
            AdresaKlijenta = adresa;
            Algoritam = algoritam;
            Kljuc = kljuc;
        }

        public NacinKomunikacije()
        {
        }
    }
}
