using Biblioteka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Server
    {
        public Dictionary<string, List<ZadatakProjekta>> projekti = new Dictionary<string, List<ZadatakProjekta>>();
        static void Main(string[] args)
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);
            //IPEndPoint destinationEP = new IPEndPoint(IPAddress.Parse(""), );

        }
    }
}

