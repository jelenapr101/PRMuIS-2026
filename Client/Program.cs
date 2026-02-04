using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Domain;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("-->  KLIJENTSKA APLIKACIJA  <--");
            Console.WriteLine("Izaberite protokol:");
            Console.WriteLine("1. TCP");
            Console.WriteLine("2. UDP");
            Console.Write("Izbor: 1 ili 2: ");

            string izbor = Console.ReadLine();

            if (izbor == "1")
            {
                PokreniTCPKlijent();
            }
            else if (izbor == "2")
            {
                PokreniUDPKlijent();
            }
            else
            {
                Console.WriteLine("Nevažeći izbor!");
            }
        }

        static string IzaberiAlgoritam(out string algoritam, out string kljuc)
        {
            Console.WriteLine("\nIzaberite algoritam šifrovanja:");
            Console.WriteLine("1. Homofonsko");
            Console.WriteLine("2. Vizner");
            Console.WriteLine("3. Transpozicija matrica");
            Console.Write("Izbor: 1, 2 ili 3: ");

            string izbor = Console.ReadLine();
            algoritam = "";
            kljuc = "";

            switch (izbor)
            {
                case "1":
                    algoritam = "Homofonsko";
                    HomofonoSifrovanje homo = new HomofonoSifrovanje("TEST");
                    kljuc = homo.Kljuc;
                    break;
                case "2":
                    algoritam = "Vizner";
                    Console.Write("Unesite ključ za Vižnerov algoritam (npr. KLJUC): ");
                    kljuc = Console.ReadLine().ToUpper();
                    break;
                case "3":
                    algoritam = "Transpozicija";
                    TranspozicijaMatrica transp = new TranspozicijaMatrica("TEST", 3, 5);
                    kljuc = transp.Kljuc;
                    break;
                default:
                    return null;
            }
            return izbor;
        }

        static void PokreniTCPKlijent()
        {
            try
            {
                string algoritam, kljuc;
                string izbor = IzaberiAlgoritam(out algoritam, out kljuc);
                if (izbor == null)
                {
                    Console.WriteLine("Nevažeći izbor!");
                    return;
                }

                Socket klijentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Parse("127.0.0.1");
                IPEndPoint endPoint = new IPEndPoint(ip, 5000);

                Console.WriteLine("\n[KLIJENT] Povezujem se na server...");
                klijentSocket.Connect(endPoint);
                Console.WriteLine("[KLIJENT] Povezan na server!");

                string informacije = $"{algoritam}|{kljuc}";
                byte[] bufferInfo = Encoding.UTF8.GetBytes(informacije);
                klijentSocket.Send(bufferInfo);
                Console.WriteLine($"[KLIJENT] Poslane informacije o algoritmu: {algoritam}\n");

                while (true)
                {
                    Console.Write("[KLIJENT] Unesi poruku (ili 'izlaz' za izlaz): ");
                    string poruka = Console.ReadLine();

                    if (poruka.ToLower() == "izlaz")
                        break;

                    string sifrovanaPorukaIzlaz = SifrujPoruku(poruka, algoritam, kljuc);
                    Console.WriteLine($"[KLIJENT] Šifrovana poruka: {sifrovanaPorukaIzlaz}");

                    byte[] bufferPoruka = Encoding.UTF8.GetBytes(sifrovanaPorukaIzlaz);
                    klijentSocket.Send(bufferPoruka);

                    byte[] bufferOdgovor = new byte[2048];
                    int primljeno = klijentSocket.Receive(bufferOdgovor);
                    string sifrovanOdgovor = Encoding.UTF8.GetString(bufferOdgovor, 0, primljeno);
                    Console.WriteLine($"[KLIJENT] Primljen šifrovani odgovor: {sifrovanOdgovor}");

                    string desifrovaniOdgovor = DesifrujPoruku(sifrovanOdgovor, algoritam, kljuc);
                    Console.WriteLine($"[KLIJENT] Dešifrovani odgovor: {desifrovaniOdgovor}\n");
                }

                klijentSocket.Close();
                Console.WriteLine("[KLIJENT] Veza sa serverom zatvorena.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KLIJENT] Greška: {ex.Message}");
            }
        }

        static void PokreniUDPKlijent()
        {
            try
            {
                string algoritam, kljuc;
                string izbor = IzaberiAlgoritam(out algoritam, out kljuc);
                if (izbor == null)
                {
                    Console.WriteLine("Nevažeći izbor!");
                    return;
                }

                Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPAddress ip = IPAddress.Parse("127.0.0.1");
                IPEndPoint serverEndPoint = new IPEndPoint(ip, 5001);

                Console.WriteLine("[KLIJENT] UDP klijent pokrenut!");

                // Posalji inicijalne informacije o algoritmu
                string initPoruka = $"INIT|{algoritam}|{kljuc}";
                byte[] initBuffer = Encoding.UTF8.GetBytes(initPoruka);
                udpSocket.SendTo(initBuffer, serverEndPoint);

                // Primi potvrdu
                byte[] potvrdaBuffer = new byte[1024];
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                int primljeno = udpSocket.ReceiveFrom(potvrdaBuffer, ref remoteEP);
                string potvrda = Encoding.UTF8.GetString(potvrdaBuffer, 0, primljeno);
                Console.WriteLine($"[KLIJENT] Server potvrda: {potvrda}\n");

                while (true)
                {
                    Console.Write("[KLIJENT] Unesi poruku (ili 'izlaz' za izlaz): ");
                    string poruka = Console.ReadLine();

                    if (poruka.ToLower() == "izlaz")
                        break;

                    string sifrovana = SifrujPoruku(poruka, algoritam, kljuc);
                    Console.WriteLine($"[KLIJENT] Šifrovana poruka: {sifrovana}");

                    byte[] buffer = Encoding.UTF8.GetBytes(sifrovana);
                    udpSocket.SendTo(buffer, serverEndPoint);

                    byte[] bufferOdgovor = new byte[2048];
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    primljeno = udpSocket.ReceiveFrom(bufferOdgovor, ref remoteEndPoint);
                    string sifrovanOdgovor = Encoding.UTF8.GetString(bufferOdgovor, 0, primljeno);
                    Console.WriteLine($"[KLIJENT] Primljen šifrovani odgovor: {sifrovanOdgovor}");

                    string desifrovaniOdgovor = DesifrujPoruku(sifrovanOdgovor, algoritam, kljuc);
                    Console.WriteLine($"[KLIJENT] Dešifrovani odgovor: {desifrovaniOdgovor}\n");
                }

                udpSocket.Close();
                Console.WriteLine("[KLIJENT] UDP klijent zatvoren.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KLIJENT] Greška: {ex.Message}");
            }
        }

        static string SifrujPoruku(string poruka, string algoritam, string kljuc)
        {
            switch (algoritam.ToLower())
            {
                case "homofonsko":
                    {
                        HomofonoSifrovanje homo = new HomofonoSifrovanje();
                        homo.Kljuc = kljuc;
                        homo.Poruka = poruka;
                        return homo.Enkripcija();
                    }
                case "vizner":
                    {
                        ViznerovAlgoritam vizner = new ViznerovAlgoritam();
                        vizner.Kljuc = kljuc;
                        vizner.Poruka = poruka;
                        return vizner.Enkripcija();
                    }
                case "transpozicija":
                    {
                        TranspozicijaMatrica transp = new TranspozicijaMatrica();
                        transp.Kljuc = kljuc;
                        transp.Poruka = poruka;
                        return transp.Enkripcija();
                    }
                default:
                    return poruka;
            }
        }

        static string DesifrujPoruku(string sifrovana, string algoritam, string kljuc)
        {
            switch (algoritam.ToLower())
            {
                case "homofonsko":
                    {
                        HomofonoSifrovanje homo = new HomofonoSifrovanje();
                        homo.Kljuc = kljuc;
                        return homo.Dekripcija(sifrovana);
                    }
                case "vizner":
                    {
                        ViznerovAlgoritam vizner = new ViznerovAlgoritam();
                        vizner.Kljuc = kljuc;
                        return vizner.Dekripcija(sifrovana);
                    }
                case "transpozicija":
                    {
                        TranspozicijaMatrica transp = new TranspozicijaMatrica();
                        transp.Kljuc = kljuc;
                        return transp.Dekripcija(sifrovana);
                    }
                default:
                    return sifrovana;
            }
        }
    }
}
