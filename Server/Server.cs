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
        public Dictionary<string, List<ZadatakProjekta>> projekti = new Dictionary<string, List<ZadatakProjekta>>();
        public string[] menadzeri = null;
        static void Main(string[] args)
        {
            Socket UDPserverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);
            IPEndPoint UDPserverEP = new IPEndPoint(IPAddress.Any, 5000);
            UDPserverSocket.Bind(UDPserverEP);

            EndPoint posiljaocEP = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                byte[] prijemnik = new byte[2048];
                try
                {
                    int brBajta = UDPserverSocket.ReceiveFrom(prijemnik,ref posiljaocEP);
                    string ime = Encoding.UTF8.GetString(prijemnik, 0, brBajta);

                    if (File.Exists("Menadzeri.txt"))
                    {
                       //iscitati sve menadzere iz fajla i potvrditi da menadzer kojeg saljes postoji
                    }
                    else
                    {
                        //kreirati Menadzeri.txt i dodati menadzera
                    }
                }
                catch { 

                }
            }
        }
    }
}

