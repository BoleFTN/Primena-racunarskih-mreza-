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
            Console.WriteLine("Menadzer krece sa radom...");
            Socket UDPmenadzerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint UDPdestinationEP = new IPEndPoint(IPAddress.Loopback, 27015);
           // EndPoint serverEP = new IPEndPoint(IPAddress.Any, 50000);
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
                Console.WriteLine($"Konektovanje sa serverom uspesno, port tcp uticnice je {poruka_o_tcp_uticnici}");
               
        }
    }
}
