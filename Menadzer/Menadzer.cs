using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using Biblioteka;

namespace Menadzer
{
    public class Menadzer
    {
        static List<ZadatakProjekta> projekti;

        static void Main(string[] args)
        {
            Console.WriteLine("Menadžer kreće sa radom...");
            Thread.Sleep(3000);

            Socket UDPmenadzerSocket = null;
            Socket TCPmenadzerSocket = null;

            try
            {
                UDPmenadzerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint UDPdestinationEP = new IPEndPoint(IPAddress.Loopback, 27015);
                EndPoint serverEP = new IPEndPoint(IPAddress.Any, 0);

                string ImeMenadzera = "";

                if (File.Exists("Menadzer.txt"))
                {
                    ImeMenadzera = File.ReadAllText("Menadzer.txt").Trim();
                    if (string.IsNullOrEmpty(ImeMenadzera))
                    {
                        Console.WriteLine("Unesite vaše korisničko ime u formatu MENADZER:[VAŠE IME]");
                        ImeMenadzera = Console.ReadLine();
                        File.WriteAllText("Menadzer.txt", ImeMenadzera);
                    }
                }
                else
                {
                    Console.WriteLine("Unesite vaše korisničko ime u formatu MENADZER:[VAŠE IME]");
                    ImeMenadzera = Console.ReadLine();
                    File.WriteAllText("Menadzer.txt", ImeMenadzera);
                }

                Console.WriteLine($"Šaljem ime '{ImeMenadzera}' serveru...");

                // Pošalji ime serveru preko UDP-a
                byte[] enkriptovanaPoruka = Encoding.UTF8.GetBytes(ImeMenadzera);
                int slanje = UDPmenadzerSocket.SendTo(enkriptovanaPoruka, UDPdestinationEP);
                Console.WriteLine("Poslano ime serveru preko UDP-a");

                // Prima TCP port od servera
                byte[] prijemniBuffer = new byte[1024];
                int brBajta = UDPmenadzerSocket.ReceiveFrom(prijemniBuffer, ref serverEP);
                string poruka_o_tcp_uticnici = Encoding.UTF8.GetString(prijemniBuffer, 0, brBajta);
                Console.WriteLine($"Primljen TCP port: {poruka_o_tcp_uticnici}");



                // Uspostavi TCP vezu sa serverom
                TCPmenadzerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint TCPserverEP = new IPEndPoint(IPAddress.Loopback, int.Parse(poruka_o_tcp_uticnici));

                Console.WriteLine("Povezujem se sa TCP serverom...");
                TCPmenadzerSocket.Connect(TCPserverEP);
                Console.WriteLine("Uspešno povezan sa TCP serverom!");

                int opcija;
                while (true)
                {
                    Console.WriteLine("\n" + new string('=', 40));
                    Console.WriteLine("Izaberite opciju:");
                    Console.WriteLine("0 - Izlaz");
                    Console.WriteLine("1 - Zadajte projekat");
                    Console.WriteLine("2 - Izlistajte projekte");
                    Console.WriteLine(new string('=', 40));
                    Console.Write("Vaš izbor: ");

                    if (!int.TryParse(Console.ReadLine(), out opcija))
                    {
                        Console.WriteLine("Nevalidna opcija!");
                        continue;
                    }

                    // Pošalji opciju serveru
                    byte[] opcijaBinarno = Encoding.UTF8.GetBytes(opcija.ToString());
                    TCPmenadzerSocket.Send(opcijaBinarno);
                    Console.WriteLine($"Poslana opcija {opcija}");

                    if (opcija == 1)
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        Console.WriteLine("Unesite naziv projekta: ");
                        string nazivProjekta = Console.ReadLine();
                        Console.WriteLine("Unesite ime zaposlenog: ");
                        string imeZaposlenog = Console.ReadLine();
                        Console.WriteLine("Unesite rok izrade: ");
                        string rokIzrade = Console.ReadLine();
                        Console.WriteLine("Unesite prioritet: ");
                        int prioritet = int.Parse(Console.ReadLine());
                        ZadatakProjekta zp = new ZadatakProjekta
                        {
                            NazivProjekta = nazivProjekta,
                            Zaposleni = imeZaposlenog,
                            RokIzrade = rokIzrade,
                            prioritet = prioritet,
                            stanje = StanjeProjekta.naCekanju,
                            Komentar = ""
                        };
                        using (MemoryStream ms = new MemoryStream())
                        {
                            formatter.Serialize(ms, zp);
                            byte[] data = ms.ToArray();

                            TCPmenadzerSocket.Send(data);
                        }
                    }
                    else if (opcija == 2)
                    {
                        // Prima listu projekata
                        Console.WriteLine("Zahtevam listu projekata...");

                        try
                        {
                            // Prvo primi dužinu podataka (4 bajta)
                            byte[] lengthBuffer = new byte[4];
                            int lengthReceived = 0;
                            while (lengthReceived < 4)
                            {
                                int received = TCPmenadzerSocket.Receive(lengthBuffer, lengthReceived, 4 - lengthReceived, SocketFlags.None);
                                lengthReceived += received;
                            }

                            int dataLength = BitConverter.ToInt32(lengthBuffer, 0);
                            Console.WriteLine($"Očekujem {dataLength} bajtova podataka...");

                            // Prima podatke u blokovima
                            List<byte> allData = new List<byte>();
                            byte[] buffer = new byte[1024];

                            while (allData.Count < dataLength)
                            {
                                int remaining = dataLength - allData.Count;
                                int toReceive = Math.Min(buffer.Length, remaining);

                                int received = TCPmenadzerSocket.Receive(buffer, 0, toReceive, SocketFlags.None);

                                for (int i = 0; i < received; i++)
                                {
                                    allData.Add(buffer[i]);
                                }

                                Console.WriteLine($"Primljeno {received} bajtova (ukupno: {allData.Count}/{dataLength})");
                            }

                            // Deserijalizuj listu
                            using (MemoryStream ms = new MemoryStream(allData.ToArray()))
                            {
                                BinaryFormatter formatter = new BinaryFormatter();
                                projekti = (List<ZadatakProjekta>)formatter.Deserialize(ms);
                            }

                            // Prikaži listu
                            Console.WriteLine("\n" + new string('*', 50));
                            Console.WriteLine("             LISTA PROJEKATA");
                            Console.WriteLine(new string('*', 50));

                            if (projekti == null || projekti.Count == 0)
                            {
                                Console.WriteLine("Nema projekata u sistemu.");
                            }
                            else
                            {
                                for (int i = 0; i < projekti.Count; i++)
                                {
                                    var zp = projekti[i];
                                    Console.WriteLine($"\n[PROJEKAT {i + 1}]");
                                    Console.WriteLine($"  Naziv: {zp.NazivProjekta}");
                                    Console.WriteLine($"  Zaposleni: {zp.Zaposleni}");
                                    Console.WriteLine($"  Rok izrade: {zp.RokIzrade}");
                                    Console.WriteLine($"  Prioritet: {zp.prioritet}");
                                    Console.WriteLine($"  Stanje: {zp.stanje}");
                                    if (zp.stanje != StanjeProjekta.uIzradi && zp.Komentar != "") { 
                                    Console.WriteLine($"Zaposleni{zp.Zaposleni} je ostavio komentar {zp.Komentar} na projektu {zp.NazivProjekta}");
                                    }
                                    Console.WriteLine("  " + new string('-', 40));
                                }
                                Console.WriteLine($"\nUkupno projekata: {projekti.Count}");
                            }
                            Console.WriteLine(new string('*', 50));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Greška pri primanju liste: {ex.Message}");
                        }
                    }
                    else if (opcija == 0)
                    {
                        Console.WriteLine("Izlazim iz aplikacije...");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Nevalidna opcija! Molimo unesite 0, 1 ili 2.");
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket greška: {ex.Message}");
                Console.WriteLine($"Error code: {ex.SocketErrorCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Opšta greška: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                // Očisti resurse
                try { TCPmenadzerSocket.Close(); } catch { }
                try { UDPmenadzerSocket.Close(); } catch { }

            }

            Console.WriteLine("\nPritisnite bilo koji taster za izlaz...");
            Console.ReadKey();
        }
    }
}