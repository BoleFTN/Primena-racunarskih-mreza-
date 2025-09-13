using Biblioteka;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
namespace Server
{
    public class Server
    {
        //rad sa projektnim zadacima ide preko TCP-a
        //ostalo preko UDP-a
        public static List<ZadatakProjekta> projekti = new List<ZadatakProjekta>();
        public static List<string> menadzeri = null;
        public static Dictionary<string,List<ZadatakProjekta>> projektiZaMenadzera = new Dictionary<string, List<ZadatakProjekta>>();
        static void Main(string[] args)
        {
            Socket UDPserverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 27015);
            UDPserverSocket.Bind(serverEP);
            //UDPserverSocket.Blocking = false;
            /*Socket TCPserverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);
            TCPserverSocket.Bind(serverEP);*/

            EndPoint posiljaocKlijentEP = new IPEndPoint(IPAddress.Any, 0);

            byte[] prijemnik = new byte[2048];
            string ime="";
            try
                {
                    int brBajta = UDPserverSocket.ReceiveFrom(prijemnik, ref posiljaocKlijentEP);
                    Console.WriteLine($"Server prima poruku od {posiljaocKlijentEP}");
                    ime = Encoding.UTF8.GetString(prijemnik, 0, brBajta);
                    //string ime = imePrijem.Split(':')[1];

                    //iscitati sve menadzere iz fajla i potvrditi da menadzer kojeg saljes postoji
                    menadzeri = File.ReadAllLines("Menadzer.txt").ToList();

                    if (menadzeri.Contains(ime))
                    {
                        byte[] enkriptovanaTCPuticnica = Encoding.UTF8.GetBytes("50001");
                        int slanje = UDPserverSocket.SendTo(enkriptovanaTCPuticnica, 0, enkriptovanaTCPuticnica.Length, SocketFlags.None, posiljaocKlijentEP);
                    }
                    else
                    {
                        menadzeri.Add(ime);
                        File.WriteAllLines("Menadzer.txt", menadzeri);
                        byte[] enkriptovanaTCPuticnica = Encoding.UTF8.GetBytes("50001");
                        int slanje = UDPserverSocket.SendTo(enkriptovanaTCPuticnica, 0, enkriptovanaTCPuticnica.Length, SocketFlags.None, posiljaocKlijentEP);
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("recvfrom failed with error: {0}", ex.Message);
                }
            //Uspostavljanje TCP konekcije sa klijentom
            Socket TCPserverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint TCPserverEP = new IPEndPoint(IPAddress.Any, 50001);

            TCPserverSocket.Bind(TCPserverEP);
            //TCPserverSocket.Blocking = false;
            TCPserverSocket.Listen(5);

            Console.WriteLine($"Server je stavljen u stanje osluskivanja i ocekuje komunikaciju na {TCPserverEP}");
            Socket acceptedSocket;

            acceptedSocket = TCPserverSocket.Accept();
            //acceptedSocket.Blocking = false;
            
            IPEndPoint menadzerEP = acceptedSocket.RemoteEndPoint as IPEndPoint;
            Console.WriteLine($"Povezao se novi klijent! Njegova adresa je {menadzerEP}");
            //Treba sad da primi poslat objekat od strane Menadzera
            int opcija;

            projektiZaMenadzera.Add(ime, projekti);  //inicijalizacija recnika

            while (true) {
                if (acceptedSocket != null && acceptedSocket.Poll(1000 * 1000, SelectMode.SelectRead))
                {
                    int brBajta = acceptedSocket.Receive(prijemnik);
                    opcija = int.Parse(Encoding.UTF8.GetString(prijemnik, 0, brBajta));
                }
                else { 
                    opcija = -1;
                }

                if (opcija == 1)
                {
                    int brBajtaObjekat = acceptedSocket.Receive(prijemnik);
                    using (MemoryStream ms = new MemoryStream(prijemnik, 0, brBajtaObjekat))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        ZadatakProjekta zp = (ZadatakProjekta)formatter.Deserialize(ms);
                        projekti.Add(zp);
                    }
                    
                    projektiZaMenadzera[ime] = projekti;
                }
                else if (opcija == 2)
                {
                    using (MemoryStream ms = new MemoryStream()) {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(ms,projekti);
                        byte[] data = ms.ToArray();

                        acceptedSocket.Send(data);
                    }
                }

                else if (opcija == 0)
                {
                    break;
                }
                else {
                    break;
                }
            }

        }
    }
}

