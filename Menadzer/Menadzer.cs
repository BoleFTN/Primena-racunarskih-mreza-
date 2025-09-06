using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Menadzer
{
    public class Menadzer
    {
        static void Main(string[] args)
        {
            Socket UDPmenadzerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint UDPdestinationEP = new IPEndPoint(IPAddress.Loopback, 50001);
            EndPoint serverEP = new IPEndPoint(IPAddress.Any, 50000);

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
                int slanje = UDPmenadzerSocket.SendTo(enkriptovanaPoruka,0,enkriptovanaPoruka.Length, SocketFlags.None,serverEP );
            }
            else {
                Console.WriteLine("Unesite vase korisnicko ime u formatu MENADZER:[VASE IME]");
                ImeMenadzera = Console.ReadLine();
                File.WriteAllText("Menadzer.txt", ImeMenadzera);
                //poslati serveru username da bi dobili uticnicu
                byte[] enkriptovanaPoruka = Encoding.UTF8.GetBytes(ImeMenadzera);
                int slanje = UDPmenadzerSocket.SendTo(enkriptovanaPoruka, 0, enkriptovanaPoruka.Length, SocketFlags.None, serverEP);
            }
        }
    }
}
