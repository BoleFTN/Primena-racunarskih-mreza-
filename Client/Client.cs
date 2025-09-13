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

        static void Main(string[] args)
        {
            Console.WriteLine("Zaposleni krece sa radom...");
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

            }


        }
    }
}
