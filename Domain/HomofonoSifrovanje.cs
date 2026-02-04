using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Domain
{
    // Homofono sifrovanje 

    public class HomofonoSifrovanje
    {
        private Dictionary<char, List<int>> slovoUBrojeve;
        private Dictionary<int, char> brojUSlovo;
        private Dictionary<char, int> indeksSamoglasnika;
        private Random random;

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
            set
            {
                kljuc = value;
                if (!string.IsNullOrEmpty(value))
                    ParsirajKljuc(value);
            }
        }

        public HomofonoSifrovanje()
        {
            random = new Random();
            slovoUBrojeve = new Dictionary<char, List<int>>();
            brojUSlovo = new Dictionary<int, char>();
            indeksSamoglasnika = new Dictionary<char, int>();

            GenerisiKljuc();
        }

        public HomofonoSifrovanje(string poruka) : this()
        {
            this.poruka = poruka;
        }


        /// Generisanje kljuca

        private void GenerisiKljuc()
        {
            slovoUBrojeve.Clear();
            brojUSlovo.Clear();
            indeksSamoglasnika.Clear();

            string alfabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string samoglasnici = "AEIOU";

            List<int> slobodniBrojevi = Enumerable.Range(10, 90).OrderBy(x => random.Next()).ToList();

            foreach (char slovo in alfabet)
            {
                List<int> brojevi = new List<int>();

                // Svako slovo dobija bar jedan broj
                int b1 = slobodniBrojevi[0];
                slobodniBrojevi.RemoveAt(0);
                brojevi.Add(b1);
                brojUSlovo[b1] = slovo;

                // Samoglasnici dobijaju jos jedan broj
                if (samoglasnici.Contains(slovo))
                {
                    int b2 = slobodniBrojevi[0];
                    slobodniBrojevi.RemoveAt(0);
                    brojevi.Add(b2);
                    brojUSlovo[b2] = slovo;
                    indeksSamoglasnika[slovo] = 0;
                }

                slovoUBrojeve[slovo] = brojevi;
            }

            // ključ kao string (zbog slanja serveru)
            StringBuilder sb = new StringBuilder();
            foreach (var par in slovoUBrojeve)
            {
                sb.Append(par.Key + ":");
                sb.Append(string.Join(",", par.Value));
                sb.Append(";");
            }
            kljuc = sb.ToString();
        }


        /// Parsiranje kljuca

        private void ParsirajKljuc(string key)
        {
            slovoUBrojeve.Clear();
            brojUSlovo.Clear();
            indeksSamoglasnika.Clear();

            string[] parovi = key.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string p in parovi)
            {
                string[] delovi = p.Split(':');
                char slovo = delovi[0][0];
                List<int> brojevi = delovi[1].Split(',').Select(int.Parse).ToList();

                slovoUBrojeve[slovo] = brojevi;
                foreach (int b in brojevi)
                    brojUSlovo[b] = slovo;

                if (brojevi.Count == 2)
                    indeksSamoglasnika[slovo] = 0;
            }
        }


        /// Enkripcija

        public string Enkripcija()
        {
            if (string.IsNullOrEmpty(poruka))
                return "";

            StringBuilder rezultat = new StringBuilder();

            foreach (char c in poruka.ToUpper())
            {
                if (char.IsLetter(c))
                {
                    List<int> brojevi = slovoUBrojeve[c];

                    int izabrani;
                    if (brojevi.Count == 2)
                    {
                        int i = indeksSamoglasnika[c];
                        izabrani = brojevi[i];
                        indeksSamoglasnika[c] = (i + 1) % 2;
                    }
                    else
                    {
                        izabrani = brojevi[0];
                    }

                    rezultat.Append(izabrani + " ");
                }
                else if (char.IsWhiteSpace(c))
                {
                    rezultat.Append("/ ");
                }
            }

            return rezultat.ToString().Trim();
        }

 
        /// Dekripcija
  
        public string Dekripcija(string sifrovana)
        {
            if (string.IsNullOrEmpty(sifrovana))
                return "";

            StringBuilder rezultat = new StringBuilder();
            string[] delovi = sifrovana.Split(' ');

            foreach (string d in delovi)
            {
                if (d == "/")
                    rezultat.Append(" ");
                else
                {
                    int broj = int.Parse(d);
                    rezultat.Append(brojUSlovo[broj]);
                }
            }

            return rezultat.ToString();
        }
    }
}
