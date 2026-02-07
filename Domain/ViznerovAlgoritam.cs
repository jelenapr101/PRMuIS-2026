using System;
using System.Text;

namespace Domain
{
    // Viznerov algoritam

    public class ViznerovAlgoritam
    {
        private string poruka;
        private string kljuc;

        public string Poruka
        {
            get { return poruka; }
            set { poruka = value; }
        }

        public string Kljuc
        {
            get { return kljuc; }
            set { kljuc = value; }
        }

        public ViznerovAlgoritam(string poruka, string kljuc)
        {
            this.poruka = poruka;
            this.kljuc = kljuc;
        }

        public ViznerovAlgoritam()
        {
        }

        // produzava kljuc (duzina rijeci koja se sifruje)
        private string NapraviProduzeniKljuc(int duzina)
        {
            StringBuilder produzeni = new StringBuilder();
            for (int i = 0; i < duzina; i++)
            {
                produzeni.Append(kljuc[i % kljuc.Length]);
            }
            return produzeni.ToString().ToUpper();
        }

        public string Enkripcija()
        {
            if (string.IsNullOrEmpty(poruka) || string.IsNullOrEmpty(kljuc))
                return "";

            string tekst = poruka.ToUpper(); // podrazumijevano velika slova
            StringBuilder rezultat = new StringBuilder();
            int kljucIndex = 0;

            foreach (char c in tekst)
            {
                if (char.IsLetter(c))
                {
                    char kljucSlovo = kljuc.ToUpper()[kljucIndex % kljuc.Length];
                    int pomeraj = kljucSlovo - 'A';
                    char sifrovano = (char)((c - 'A' + pomeraj) % 26 + 'A');
                    rezultat.Append(sifrovano);
                    kljucIndex++;
                }
                else
                {
                    rezultat.Append(c);
                }
            }

            return rezultat.ToString();
        }

        public string Dekripcija(string sifrovana)
        {
            if (string.IsNullOrEmpty(sifrovana) || string.IsNullOrEmpty(kljuc))
                return "";

            StringBuilder rezultat = new StringBuilder();
            int kljucIndex = 0;

            foreach (char c in sifrovana)
            {
                if (char.IsLetter(c))
                {
                    char kljucSlovo = kljuc.ToUpper()[kljucIndex % kljuc.Length];
                    int pomeraj = kljucSlovo - 'A';
                    char desifrovano = (char)((c - 'A' - pomeraj + 26) % 26 + 'A');
                    rezultat.Append(desifrovano);
                    kljucIndex++;
                }
                else
                {
                    rezultat.Append(c);
                }
            }

            return rezultat.ToString();
        }
    }
}
