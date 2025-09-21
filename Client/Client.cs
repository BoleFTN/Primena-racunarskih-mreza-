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
//using System.Runtime.Serialization;
using Biblioteka;

namespace Client
{
    public class Client
    {
        static List<ZadatakProjekta> projekti;

        static void Main(string[] args)
        {
            /* Console.WriteLine("Zaposleni krece sa radom...");
             Thread.Sleep(3000); //Uspavljujemo zbog nepouzdanosti UDP-a, server mora da pocne da osluskuje pre nego sto Zaposleni nesto posalje

             Socket UDPZaposleniSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
             Socket TCPZaposleniSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
             IPEndPoint UDPdestinationEP = new IPEndPoint(IPAddress.Loopback, 27015);
             EndPoint serverEP = new IPEndPoint(IPAddress.Any, 0);

             string ImeZaposlenog = "";
             if (File.Exists("Zaposleni.txt"))
             {
                 //u Zaposleni.txt se cuva ulogovani korisnik
                 ImeZaposlenog = File.ReadAllText("Zaposleni.txt");
                 if (ImeZaposlenog.Trim().Equals(string.Empty))
                 {
                     Console.WriteLine("Unesite vase korisnicko ime da bi ste dobili uticnicu za rad u formatu ZAPOSLENI:[VASE IME]");
                     ImeZaposlenog = Console.ReadLine();
                 }
                 //sada prosledi ImeZaposlenog serveru i trebalo bi da dobijes koju tcp uticnicu koristis

                 byte[] enkriptovanaPoruka = Encoding.UTF8.GetBytes(ImeZaposlenog);
                 int slanje = UDPZaposleniSocket.SendTo(enkriptovanaPoruka, 0, enkriptovanaPoruka.Length, SocketFlags.None, UDPdestinationEP);

             }
             else
             {
                 Console.WriteLine("Unesite vase korisnicko ime u formatu ZAPOSLENI:[VASE IME]");
                 ImeZaposlenog = Console.ReadLine();
                 File.WriteAllText("Zaposleni.txt", ImeZaposlenog);
                 //poslati serveru username da bi dobili uticnicu
                 byte[] enkriptovanaPoruka = Encoding.UTF8.GetBytes(ImeZaposlenog);
                 int slanje = UDPZaposleniSocket.SendTo(enkriptovanaPoruka, 0, enkriptovanaPoruka.Length, SocketFlags.None, UDPdestinationEP);

             }*/
            Console.WriteLine("Zaposleni kreće sa radom...");
            Thread.Sleep(3000);

            Socket UDPzaposleniSocket = null;
            Socket TCPzaposleniSocket = null;

            try
            {
                UDPzaposleniSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint UDPdestinationEP = new IPEndPoint(IPAddress.Loopback, 27015);
                EndPoint serverEP = new IPEndPoint(IPAddress.Any, 0);

                string ImeZaposlenog = "";

                if (File.Exists("Zaposleni.txt"))
                {
                    ImeZaposlenog = File.ReadAllText("Zaposleni.txt").Trim();
                    if (string.IsNullOrEmpty(ImeZaposlenog))
                    {
                        Console.WriteLine("Unesite vaše korisničko ime u formatu ZAPOSLENI:[VAŠE IME]");
                        ImeZaposlenog = Console.ReadLine();
                        File.WriteAllText("Menadzer.txt", ImeZaposlenog);
                    }
                }
                else
                {
                    Console.WriteLine("Unesite vaše korisničko ime u formatu ZAPOSLENI:[VAŠE IME]");
                    ImeZaposlenog = Console.ReadLine();
                    File.WriteAllText("Menadzer.txt", ImeZaposlenog);
                }

                Console.WriteLine($"Šaljem ime '{ImeZaposlenog}' serveru...");

                // Pošalji ime serveru preko UDP-a
                byte[] enkriptovanaPoruka = Encoding.UTF8.GetBytes(ImeZaposlenog);
                int slanje = UDPzaposleniSocket.SendTo(enkriptovanaPoruka, 0, enkriptovanaPoruka.Length, SocketFlags.None, UDPdestinationEP);
                Console.WriteLine("Poslano ime serveru preko UDP-a");

                // Prima TCP port od servera
                byte[] prijemniBuffer = new byte[1024];
                UDPzaposleniSocket.ReceiveTimeout = 10000; // 10 sekundi timeout
                int brBajta = UDPzaposleniSocket.ReceiveFrom(prijemniBuffer, ref serverEP);
                string poruka_o_tcp_uticnici = Encoding.UTF8.GetString(prijemniBuffer, 0, brBajta);
                Console.WriteLine($"Primljen TCP port: {poruka_o_tcp_uticnici}");

                
                

                // Uspostavi TCP vezu sa serverom
                TCPzaposleniSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint TCPserverEP = new IPEndPoint(IPAddress.Loopback, int.Parse(poruka_o_tcp_uticnici));

                Console.WriteLine("Povezujem se sa TCP serverom...");
                TCPzaposleniSocket.Connect(TCPserverEP);
                Console.WriteLine("Uspešno povezan sa TCP serverom!");

                //int opcija;
                while (true)
                {
                    Console.WriteLine("\n" + new string('=', 40));
                    Console.WriteLine("Izaberite opciju:");
                    Console.WriteLine("0->ZAPOSLENI - Izlaz");
                    Console.WriteLine("1->ZAPOSLENI - Lista projekata");
                    Console.WriteLine("2->ZAPOSLENI - Azuriraj projekat");
                    Console.WriteLine("3->ZAPOSLENI - Zavrsi projekat");
                    Console.WriteLine(new string('=', 40));
                    Console.Write("Vaš izbor: ");

                    /*  if (!int.TryParse(Console.ReadLine(), out opcija))
                      {
                          Console.WriteLine("Nevalidna opcija!");
                          continue;
                      }*/
                    string opcija = Console.ReadLine();
                    // Pošalji opciju serveru
                    byte[] opcijaBinarno = Encoding.UTF8.GetBytes(opcija.ToString());
                    TCPzaposleniSocket.Send(opcijaBinarno);
                    Console.WriteLine($"Poslana opcija {opcija}");

                    if (opcija == "1->ZAPOSLENI")
                    {
                        Console.WriteLine("Zahtevam listu projekata...");

                        try
                        {
                            // Prvo primi dužinu podataka (4 bajta)
                            byte[] lengthBuffer = new byte[4];
                            int lengthReceived = 0;
                            while (lengthReceived < 4)
                            {
                                int received = TCPzaposleniSocket.Receive(lengthBuffer, lengthReceived, 4 - lengthReceived, SocketFlags.None);
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

                                int received = TCPzaposleniSocket.Receive(buffer, 0, toReceive, SocketFlags.None);

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
                            //SORTIRANJE LISTE SELECTION SORT ALGORITMOM
                            int minIndex;
                            ZadatakProjekta tmp;
                            for (int i = 0; i < projekti.Count - 1; i++)
                            {
                                minIndex = i;
                                for (int j = i + 1; j < projekti.Count; j++)
                                {
                                    if (projekti[j].prioritet > projekti[minIndex].prioritet)
                                    {
                                        minIndex = j;
                                    }
                                    if (minIndex != i)
                                    {
                                        tmp = projekti[minIndex];
                                        projekti[minIndex] = projekti[i];
                                        projekti[i] = tmp;
                                    }
                                }
                            }

                            // Prikaži listu
                            Console.WriteLine("\n" + new string('*', 50));
                            Console.WriteLine("            SORTIRANA LISTA PROJEKATA");
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
                    else if (opcija == "2->ZAPOSLENI") { 
                        Console.WriteLine("Unesite ime projekta koji ste zavrsili ili zapoceli");
                        string zavrsenProjekat = Console.ReadLine();
                        byte[] zavrsenProjekatBinarno = Encoding.UTF8.GetBytes(zavrsenProjekat);
                        TCPzaposleniSocket.Send(zavrsenProjekatBinarno);


                        string komentar = "";
                        
                        Console.WriteLine("Zelite li da dodate komentar? \n0 za ne \n1 za da");
                        string unos = Console.ReadLine();
                        if (unos == "1")
                        {
                            Console.WriteLine("Unesite komentar");
                            komentar = Console.ReadLine();
                        }
                        byte[] komentarBinarno = Encoding.UTF8.GetBytes(komentar);
                        TCPzaposleniSocket.Send(komentarBinarno);
                    }
                    else if (opcija == "3->ZAPOSLENI")
                    {
                        Console.WriteLine("Unesite ime projekta koji ste zavrsili");
                        string zavrsenProjekat = Console.ReadLine();
                        byte[] zavrsenProjekatBinarno = Encoding.UTF8.GetBytes(zavrsenProjekat);
                        TCPzaposleniSocket.Send(zavrsenProjekatBinarno);
                    }
                    else if (opcija == "0->ZAPOSLENI")
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
                try { TCPzaposleniSocket?.Close(); } catch { }
                try { UDPzaposleniSocket?.Close(); } catch { }
            }

            Console.WriteLine("\nPritisnite bilo koji taster za izlaz...");
            Console.ReadKey();

        }
    }
}
