using Biblioteka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Server
{
    public class Server
    {
        //rad sa projektnim zadacima ide preko TCP-a
        //ostalo preko UDP-a
        public static Dictionary<string, List<ZadatakProjekta>> projekti = new Dictionary<string, List<ZadatakProjekta>>();
        public static List<string> menadzeri = null;
        static void Main(string[] args)
        {
            Socket UDPserverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50000);
            UDPserverSocket.Bind(serverEP);

            Socket TCPserverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);
            TCPserverSocket.Bind(serverEP);

            EndPoint posiljaocKlijentEP = new IPEndPoint(IPAddress.Any, 50001);

            byte[] prijemnik = new byte[2048];
            try
            {
                int brBajta = UDPserverSocket.ReceiveFrom(prijemnik, ref posiljaocKlijentEP);
                string imePrijem = Encoding.UTF8.GetString(prijemnik, 0, brBajta);
                string ime = imePrijem.Split(':')[1];
             
                    //iscitati sve menadzere iz fajla i potvrditi da menadzer kojeg saljes postoji
                    menadzeri = File.ReadAllLines("menadzeri.txt").ToList();

                    if (menadzeri.Contains(ime))
                    {
                        byte[] enkriptovanaTCPuticnica = Encoding.UTF8.GetBytes("50001");
                        int slanje = UDPserverSocket.SendTo(enkriptovanaTCPuticnica, 0, enkriptovanaTCPuticnica.Length, SocketFlags.None, posiljaocKlijentEP);
                    }
                    else { 
                    menadzeri.Add(ime);
                    File.WriteAllLines("Menadzeri.txt", menadzeri);
                    byte[] enkriptovanaTCPuticnica = Encoding.UTF8.GetBytes("50001");
                    int slanje = UDPserverSocket.SendTo(enkriptovanaTCPuticnica, 0, enkriptovanaTCPuticnica.Length, SocketFlags.None, posiljaocKlijentEP);
                    }
                
            }
            catch
            {

            }

            while (true)
            {
               
            }
        }
    }
}

