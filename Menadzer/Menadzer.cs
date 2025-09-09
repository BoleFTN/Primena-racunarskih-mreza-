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
namespace Menadzer
{
    public class Menadzer
    {
        static List<ZadatakProjekta> projekti;
        static void Main(string[] args)
        {
            Console.WriteLine("Menadzer krece sa radom...");
            Thread.Sleep(2000); //Uspavljujemo zbog nepouzdanosti UDP-a, server mora da pocne da osluskuje pre nego sto Menadzer nesto posalje

            Socket UDPmenadzerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint UDPdestinationEP = new IPEndPoint(IPAddress.Loopback, 27015);
            EndPoint serverEP = new IPEndPoint(IPAddress.Any, 0);

                string ImeMenadzera = "";
                if (File.Exists("Menadzer.txt"))
                {
                    //u Menadzer.txt se cuva ulogovani korisnik
                    ImeMenadzera = File.ReadAllText("Menadzer.txt");
                    if (ImeMenadzera.Trim().Equals(string.Empty))
                    {
                        Console.WriteLine("Unesite vase korisnicko ime da bi ste dobili uticnicu za rad u formatu MENADZER:[VASE IME]");
                        ImeMenadzera = Console.ReadLine();
                    }
                    //sada prosledi ImeMenadzera serveru i trebalo bi da dobijes koju tcp uticnicu koristis

                    byte[] enkriptovanaPoruka = Encoding.UTF8.GetBytes(ImeMenadzera);
                    int slanje = UDPmenadzerSocket.SendTo(enkriptovanaPoruka, 0, enkriptovanaPoruka.Length, SocketFlags.None, UDPdestinationEP);

                }
                else
                {
                    Console.WriteLine("Unesite vase korisnicko ime u formatu MENADZER:[VASE IME]");
                    ImeMenadzera = Console.ReadLine();
                    File.WriteAllText("Menadzer.txt", ImeMenadzera);
                    //poslati serveru username da bi dobili uticnicu
                    byte[] enkriptovanaPoruka = Encoding.UTF8.GetBytes(ImeMenadzera);
                    int slanje = UDPmenadzerSocket.SendTo(enkriptovanaPoruka, 0, enkriptovanaPoruka.Length, SocketFlags.None, UDPdestinationEP);

                }
                //Menadzer prima informacije o TCP uticnici koje mu salje server
                byte[] prijemniBuffer = new byte[1024];
                int brBajta = UDPmenadzerSocket.ReceiveFrom(prijemniBuffer, ref serverEP);
                string poruka_o_tcp_uticnici = Encoding.UTF8.GetString(prijemniBuffer, 0, brBajta);
                Console.WriteLine($"Konektovanje sa serverom preko UDP uticnice uspesno, port tcp uticnice koji nam salje server je {poruka_o_tcp_uticnici}");

                //Sada menadzer uspostavlja TCP vezu sa serverom
                Socket TPCmenadzerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint TCPserverEP = new IPEndPoint(IPAddress.Loopback, 50001);
                byte[] buffer = new byte[1024];

            Console.WriteLine("Klijent je spreman za povezivanje sa serverom, kliknite enter");
            Console.ReadKey();
            TPCmenadzerSocket.Connect(TCPserverEP);
            Console.WriteLine("Klijent je uspesno povezan sa serverom!");
            //Sada menadzer salje objekat klase Projekat Serveru
            int opcija;
            while (true)
            {

                Console.WriteLine("Izaberite opciju");
                Console.WriteLine("0-izlaz");
                Console.WriteLine("1-zadajte projekat");
                Console.WriteLine("2-izlistajte projekte");

                opcija = int.Parse(Console.ReadLine());

                byte[] opcijaBinarno = Encoding.UTF8.GetBytes(opcija.ToString());
                TPCmenadzerSocket.Send(opcijaBinarno);
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
                        stanje = StanjeProjekta.naCekanju
                    };
                    using (MemoryStream ms = new MemoryStream())
                    {
                        formatter.Serialize(ms, zp);
                        byte[] data = ms.ToArray();

                        TPCmenadzerSocket.Send(data);
                    }
                }
                else if (opcija == 2) {
                    byte[] listaBuffer = new byte[1024];
                    int velicinaNiza = TPCmenadzerSocket.Receive(listaBuffer);
                    using (MemoryStream ms = new MemoryStream(listaBuffer,0,velicinaNiza)) {
                        BinaryFormatter formatter = new BinaryFormatter();
                        projekti = (List<ZadatakProjekta>)formatter.Deserialize(ms);
                    }
                    foreach (ZadatakProjekta zp in projekti) {
                        Console.WriteLine(zp.NazivProjekta);
                        Console.WriteLine(zp.RokIzrade);
                        Console.WriteLine(zp.Zaposleni);
                        Console.WriteLine(zp.prioritet);
                        Console.WriteLine(zp.stanje);
                    }
                }

                else if (opcija == 0)
                {
                    break;
                }

                else
                {
                    break;
                }

            }//kraj while petlje
        }
    }
}
