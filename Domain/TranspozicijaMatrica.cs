using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    /// Sifrovanje transpozicijom matrica

    public class TranspozicijaMatrica
    {
        private string poruka;
        private string kljuc; 
        private int redova;
        private int kolona;
        private List<int> redosledTranspozicije;


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
                {
                    ParsirajKljuc(value);
                }
            }
        }


        public TranspozicijaMatrica(string poruka, int redova, int kolona)
        {
            this.poruka = poruka;
            this.redova = redova;
            this.kolona = kolona;
            this.redosledTranspozicije = new List<int>();
            GenerisiKljuc();
        }


        public TranspozicijaMatrica()
        {
            this.redosledTranspozicije = new List<int>();
        }

     
        /// Generise kljuc - redosled transpozicije
       
        private void GenerisiKljuc()
        {
            redosledTranspozicije.Clear();
            Random random = new Random();

            // Kreiranje liste brojeva od 0 do kolona-1
            List<int> redosled = new List<int>();
            for (int i = 0; i < kolona; i++)
            {
                redosled.Add(i);
            }

            // Nasumicno mijesanje
            for (int i = redosled.Count - 1; i > 0; i--)
            {
                int randomIndex = random.Next(i + 1);
                int temp = redosled[i];
                redosled[i] = redosled[randomIndex];
                redosled[randomIndex] = temp;
            }

            redosledTranspozicije = redosled;

            // Kreiranje kljuca kao string
            StringBuilder sb = new StringBuilder();
            sb.Append($"Redova:{redova},Kolona:{kolona},Redosled:");
            foreach (int i in redosledTranspozicije)
            {
                sb.Append(i + ",");
            }
            kljuc = sb.ToString().TrimEnd(',');
        }

 
        /// Parsiranj kljuca iz string formata
        
        private void ParsirajKljuc(string kljucString)
        {
            // Format: "Redova:3,Kolona:5,Redosled:2,4,0,3,1"
            string[] delovi = kljucString.Split(',');

            foreach (string deo in delovi)
            {
                if (deo.StartsWith("Redova:"))
                {
                    redova = int.Parse(deo.Substring(7));
                }
                else if (deo.StartsWith("Kolona:"))
                {
                    kolona = int.Parse(deo.Substring(7));
                }
                else if (deo.StartsWith("Redosled:"))
                {
                    string redosledStr = kljucString.Substring(kljucString.IndexOf("Redosled:") + 9);
                    string[] redosledDelovi = redosledStr.Split(',');
                    redosledTranspozicije.Clear();
                    foreach (string r in redosledDelovi)
                    {
                        if (!string.IsNullOrEmpty(r))
                        {
                            redosledTranspozicije.Add(int.Parse(r));
                        }
                    }
                    break;
                }
            }
        }


        /// Enkripcija poruke

        public string Enkripcija()
        {
            if (string.IsNullOrEmpty(poruka))
                return "";

            // Pripremi poruku (dodaj razmake ako je potrebno)
            string pripremljena = poruka.ToUpper();
            while (pripremljena.Length < redova * kolona)
            {
                pripremljena += " ";
            }

            // Kreiraj matricu
            char[,] matrica = new char[redova, kolona];
            int index = 0;
            for (int i = 0; i < redova; i++)
            {
                for (int j = 0; j < kolona; j++)
                {
                    matrica[i, j] = pripremljena[index++];
                }
            }

            // Citaj kolone u redosledu transpozicije
            StringBuilder rezultat = new StringBuilder();
            foreach (int kolona in redosledTranspozicije)
            {
                for (int red = 0; red < redova; red++)
                {
                    rezultat.Append(matrica[red, kolona]);
                }
            }

            return rezultat.ToString();
        }


        /// Dekripcija poruke

        public string Dekripcija(string sifrovana)
        {
            if (string.IsNullOrEmpty(sifrovana))
                return "";

            // Kreiraj matricu za dekriptovanje
            char[,] matrica = new char[redova, kolona];

            // Rekonstruisi originalnu poziciju
            List<int> obrnutiRedosled = new List<int>();
            for (int i = 0; i < kolona; i++)
            {
                obrnutiRedosled.Add(0);
            }

            for (int i = 0; i < redosledTranspozicije.Count; i++)
            {
                obrnutiRedosled[redosledTranspozicije[i]] = i;
            }

            // Popuni matricu prema transpoziciji
            int pozicija = 0;
            for (int k = 0; k < kolona; k++)
            {
                int originalnaKolona = redosledTranspozicije[k];
                for (int red = 0; red < redova; red++)
                {
                    matrica[red, originalnaKolona] = sifrovana[pozicija++];
                }
            }

            // Citaj matricu redoslijed redova i kolona
            StringBuilder rezultat = new StringBuilder();
            for (int i = 0; i < redova; i++)
            {
                for (int j = 0; j < kolona; j++)
                {
                    rezultat.Append(matrica[i, j]);
                }
            }

            return rezultat.ToString();
        }


        /// Postavlja redosled transpozicije

        public void PostaviRedosledTranspozicije(List<int> redosled)
        {
            redosledTranspozicije = new List<int>(redosled);
            StringBuilder sb = new StringBuilder();
            sb.Append($"Redova:{redova},Kolona:{kolona},Redosled:");
            foreach (int i in redosledTranspozicije)
            {
                sb.Append(i + ",");
            }
            kljuc = sb.ToString().TrimEnd(',');
        }
    }
}
