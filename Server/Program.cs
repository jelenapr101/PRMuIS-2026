using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Domain;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("-->  SERVERSKA APLIKACIJA  <--");
            Console.WriteLine("Izaberite protokol:");
            Console.WriteLine("1. TCP");
            Console.WriteLine("2. UDP");
            Console.Write("Izbor: 1 ili 2: ");

            string izbor = Console.ReadLine();

            if (izbor == "1")
            {
                PokreniTCPServer();
            }
            else if (izbor == "2")
            {
                PokreniUDPServer();
            }
            else
            {
                Console.WriteLine("Nevažeći izbor!");
            }
        }

        static void PokreniTCPServer()
        {
            try
            {
                string ipAdresa = "127.0.0.1";
                int port = 5000;
                IPAddress ip = IPAddress.Parse(ipAdresa);
                IPEndPoint endPoint = new IPEndPoint(ip, port);

                Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(endPoint);
                serverSocket.Listen(5);

                Console.WriteLine($"\n[SERVER] TCP server pokrenut!");
                Console.WriteLine($"[SERVER] IP adresa: {ipAdresa}, Port: {port}");
                Console.WriteLine("[SERVER] Čekam na klijente...\n");

                List<NacinKomunikacije> klijenti = new List<NacinKomunikacije>();

                while (true)
                {
                    try
                    {
                        // Polling - prihvata novog klijenta bez blokiranja!!!
                        if (serverSocket.Poll(100000, SelectMode.SelectRead))
                        {
                            Socket klijentSocket = serverSocket.Accept();
                            Console.WriteLine($"[SERVER] Novi klijent se povezao: {klijentSocket.RemoteEndPoint}");

                            byte[] buffer = new byte[1024];
                            int primljeno = klijentSocket.Receive(buffer);
                            string informacije = Encoding.UTF8.GetString(buffer, 0, primljeno);

                            Console.WriteLine($"[SERVER] Primljene informacije: {informacije}");

                            string[] delovi = informacije.Split('|');
                            string algoritam = delovi[0];
                            string kljuc = delovi.Length > 1 ? delovi[1] : "";

                            NacinKomunikacije nacinKom = new NacinKomunikacije(klijentSocket, algoritam, kljuc);
                            klijenti.Add(nacinKom);

                            Console.WriteLine($"[SERVER] Klijent koristi algoritam: {algoritam}");
                        }

                        // Polling za sve povezane klijente
                        for (int i = klijenti.Count - 1; i >= 0; i--)
                        {
                            Socket klijentSocket = klijenti[i].UticnicaKlijenta;

                            if (klijentSocket.Poll(100000, SelectMode.SelectRead))
                            {
                                try
                                {
                                    byte[] buffer = new byte[2048];
                                    int primljeno = klijentSocket.Receive(buffer);

                                    if (primljeno > 0)
                                    {
                                        string sifrovanaPorukaUlaz = Encoding.UTF8.GetString(buffer, 0, primljeno);
                                        Console.WriteLine($"[SERVER] Primljena šifrovana poruka od {klijentSocket.RemoteEndPoint}: {sifrovanaPorukaUlaz}");

                                        string desifrovana = DesifrujPoruku(sifrovanaPorukaUlaz, klijenti[i]);
                                        Console.WriteLine($"[SERVER] Dešifrovana poruka: {desifrovana}");

                                        string odgovor = "Poruka primljena: " + desifrovana;
                                        string sifrovanOdgovor = SifrujPoruku(odgovor, klijenti[i]);

                                        byte[] bufferOdgovor = Encoding.UTF8.GetBytes(sifrovanOdgovor);
                                        klijentSocket.Send(bufferOdgovor);
                                        Console.WriteLine($"[SERVER] Šifrovani odgovor poslat klijentu: {sifrovanOdgovor}\n");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"[SERVER] Klijent {klijentSocket.RemoteEndPoint} se odspojio");
                                        klijentSocket.Close();
                                        klijenti.RemoveAt(i);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[SERVER] Greška pri komunikaciji sa klijentom: {ex.Message}");
                                    klijentSocket.Close();
                                    klijenti.RemoveAt(i);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SERVER] Greška: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER] Greška pri pokretanju servera: {ex.Message}");
            }
        }

        static void PokreniUDPServer()
        {
            try
            {
                string ipAdresa = "127.0.0.1";
                int port = 5001;
                IPAddress ip = IPAddress.Parse(ipAdresa);
                IPEndPoint endPoint = new IPEndPoint(ip, port);

                Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                udpSocket.Bind(endPoint);

                Console.WriteLine($"\n[SERVER] UDP server pokrenut!");
                Console.WriteLine($"[SERVER] IP adresa: {ipAdresa}, Port: {port}");
                Console.WriteLine("[SERVER] Čekam na datagrame...\n");

                List<NacinKomunikacije> klijenti = new List<NacinKomunikacije>();
                byte[] buffer = new byte[2048];

                while (true)
                {
                    try
                    {
                        if (udpSocket.Poll(100000, SelectMode.SelectRead))
                        {
                            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                            int primljeno = udpSocket.ReceiveFrom(buffer, ref remoteEndPoint);
                            string poruka = Encoding.UTF8.GetString(buffer, 0, primljeno);

                            Console.WriteLine($"[SERVER] Datagram primljen od {remoteEndPoint}: {poruka}");

                            // Proveri da li je ovo inicijalna poruka sa informacijama o algoritmu
                            if (poruka.StartsWith("INIT|"))
                            {
                                string[] delovi = poruka.Substring(5).Split('|');
                                string algoritam = delovi[0];
                                string kljuc = delovi.Length > 1 ? delovi[1] : "";

                                NacinKomunikacije nacinKom = new NacinKomunikacije(remoteEndPoint, algoritam, kljuc);
                                klijenti.Add(nacinKom);

                                Console.WriteLine($"[SERVER] Novi UDP klijent registrovan: {remoteEndPoint}, algoritam: {algoritam}");

                                string potvrda = "OK";
                                byte[] potvrdaBuffer = Encoding.UTF8.GetBytes(potvrda);
                                udpSocket.SendTo(potvrdaBuffer, remoteEndPoint);
                            }
                            else
                            {
                                // Pronađi klijenta
                                NacinKomunikacije nacinKom = null;
                                foreach (var k in klijenti)
                                {
                                    if (k.AdresaKlijenta.ToString() == remoteEndPoint.ToString())
                                    {
                                        nacinKom = k;
                                        break;
                                    }
                                }

                                if (nacinKom != null)
                                {
                                    string desifrovana = DesifrujPoruku(poruka, nacinKom);
                                    Console.WriteLine($"[SERVER] Dešifrovana poruka: {desifrovana}");

                                    string odgovor = "Poruka primljena: " + desifrovana;
                                    string sifrovanOdgovor = SifrujPoruku(odgovor, nacinKom);

                                    byte[] odgovorBuffer = Encoding.UTF8.GetBytes(sifrovanOdgovor);
                                    udpSocket.SendTo(odgovorBuffer, remoteEndPoint);
                                    Console.WriteLine($"[SERVER] Šifrovani odgovor poslat klijentu\n");
                                }
                                else
                                {
                                    string odgovor = "Server primio: " + poruka;
                                    byte[] odgovorBuffer = Encoding.UTF8.GetBytes(odgovor);
                                    udpSocket.SendTo(odgovorBuffer, remoteEndPoint);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SERVER] Greška: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER] Greška pri pokretanju UDP servera: {ex.Message}");
            }
        }

        static string DesifrujPoruku(string sifrovana, NacinKomunikacije nacinKom)
        {
            switch (nacinKom.Algoritam.ToLower())
            {
                case "homofonsko":
                    {
                        HomofonoSifrovanje homo = new HomofonoSifrovanje();
                        homo.Kljuc = nacinKom.Kljuc;
                        return homo.Dekripcija(sifrovana);
                    }
                case "vizner":
                    {
                        ViznerovAlgoritam vizner = new ViznerovAlgoritam();
                        vizner.Kljuc = nacinKom.Kljuc;
                        return vizner.Dekripcija(sifrovana);
                    }
                case "transpozicija":
                    {
                        TranspozicijaMatrica transp = new TranspozicijaMatrica();
                        transp.Kljuc = nacinKom.Kljuc;
                        return transp.Dekripcija(sifrovana);
                    }
                default:
                    return sifrovana;
            }
        }

        static string SifrujPoruku(string poruka, NacinKomunikacije nacinKom)
        {
            switch (nacinKom.Algoritam.ToLower())
            {
                case "homofonsko":
                    {
                        HomofonoSifrovanje homo = new HomofonoSifrovanje();
                        homo.Kljuc = nacinKom.Kljuc;
                        homo.Poruka = poruka;
                        return homo.Enkripcija();
                    }
                case "vizner":
                    {
                        ViznerovAlgoritam vizner = new ViznerovAlgoritam();
                        vizner.Kljuc = nacinKom.Kljuc;
                        vizner.Poruka = poruka;
                        return vizner.Enkripcija();
                    }
                case "transpozicija":
                    {
                        TranspozicijaMatrica transp = new TranspozicijaMatrica();
                        transp.Kljuc = nacinKom.Kljuc;
                        transp.Poruka = poruka;
                        return transp.Enkripcija();
                    }
                default:
                    return poruka;
            }
        }
    }
}
