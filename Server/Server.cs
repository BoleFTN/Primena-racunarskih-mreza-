using Biblioteka;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace Server
{
    public class Server
    {
        public static List<ZadatakProjekta> projekti = new List<ZadatakProjekta>();
        public static List<string> menadzeri = new List<string>();
        
        static void Main(string[] args)
        {
            Console.WriteLine("Server se pokreće...");
            
            // UDP socket za početnu autentifikaciju
            Socket UDPserverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint UDPserverEP = new IPEndPoint(IPAddress.Any, 27015);
            UDPserverSocket.Bind(UDPserverEP);
            UDPserverSocket.Blocking = false;

            // TCP socket za glavnu komunikaciju
            Socket TCPserverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint TCPserverEP = new IPEndPoint(IPAddress.Any, 50001);
            TCPserverSocket.Bind(TCPserverEP);
            TCPserverSocket.Blocking = false;
            TCPserverSocket.Listen(5);

            Console.WriteLine($"UDP Server pokrenut na portu {UDPserverEP.Port}");
            Console.WriteLine($"TCP Server pokrenut na portu {TCPserverEP.Port}");

            List<Socket> tcpClients = new List<Socket>();

            // Učitaj postojeće menadžere
            if (File.Exists("Menadzer.txt"))
            {
                try
                {
                    menadzeri = File.ReadAllLines("Menadzer.txt").Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    Console.WriteLine($"Učitano {menadzeri.Count} menadžera iz fajla");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška pri učitavanju menadžera: {ex.Message}");
                    menadzeri = new List<string>();
                }
            }

            while (true)
            {
                List<Socket> checkRead = new List<Socket>();
                List<Socket> checkError = new List<Socket>();

                checkRead.Add(UDPserverSocket);
                checkRead.Add(TCPserverSocket);
                checkError.Add(UDPserverSocket);
                checkError.Add(TCPserverSocket);

                foreach (Socket client in tcpClients.ToList())
                {
                    checkRead.Add(client);
                    checkError.Add(client);
                }

                try
                {
                    Socket.Select(checkRead, null, checkError, 1000);

                    // UDP komunikacija (autentifikacija)
                    if (checkRead.Contains(UDPserverSocket))
                    {
                        try
                        {
                            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                            byte[] buffer = new byte[1024];
                            int bytes = UDPserverSocket.ReceiveFrom(buffer, ref clientEP);
                            string ime = Encoding.UTF8.GetString(buffer, 0, bytes);
                            
                            Console.WriteLine($"UDP: Primljen zahtev za autentifikaciju od '{ime}'");

                            if (!menadzeri.Contains(ime))
                            {
                                menadzeri.Add(ime);
                                try
                                {
                                    File.WriteAllLines("Menadzer.txt", menadzeri);
                                    Console.WriteLine($"Dodat novi menadžer: {ime}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Greška pri čuvanju menadžera: {ex.Message}");
                                }
                            }

                            byte[] response = Encoding.UTF8.GetBytes("50001");
                            UDPserverSocket.SendTo(response, clientEP);
                            Console.WriteLine($"Poslat TCP port 50001 menadžeru {ime}");
                        }
                        catch (SocketException ex)
                        {
                            if (ex.SocketErrorCode != SocketError.WouldBlock)
                            {
                                Console.WriteLine($"UDP greška: {ex.Message}");
                            }
                        }
                    }

                    // TCP nove konekcije
                    if (checkRead.Contains(TCPserverSocket))
                    {
                        try
                        {
                            Socket newClient = TCPserverSocket.Accept();
                            newClient.Blocking = false;
                            tcpClients.Add(newClient);
                            Console.WriteLine($"Nova TCP konekcija: {newClient.RemoteEndPoint}");
                        }
                        catch (SocketException ex)
                        {
                            if (ex.SocketErrorCode != SocketError.WouldBlock)
                            {
                                Console.WriteLine($"TCP Accept greška: {ex.Message}");
                            }
                        }
                    }

                    // TCP poruke od klijenata
                    List<Socket> clientsToRemove = new List<Socket>();
                    foreach (Socket client in tcpClients.ToList())
                    {
                        if (checkRead.Contains(client))
                        {
                            try
                            {
                                byte[] buffer = new byte[1024];
                                int bytes = client.Receive(buffer);
                                
                                if (bytes == 0)
                                {
                                    Console.WriteLine($"Klijent {client.RemoteEndPoint} se diskonektovao");
                                    clientsToRemove.Add(client);
                                    continue;
                                }

                                string message = Encoding.UTF8.GetString(buffer, 0, bytes);
                                Console.WriteLine($"Primljena poruka: '{message}' od {client.RemoteEndPoint}");
                                
                                if (int.TryParse(message, out int opcija))
                                {
                                    Console.WriteLine($"Primljena opcija {opcija} od {client.RemoteEndPoint}");
                                    
                                    if (opcija == 1)
                                    {
                                        Console.WriteLine("Server prima projekat od menadzera");
                                        byte[] bufferProj = new byte[1024];
                                        // primi objekat
                                        int brBajtaObjekat = client.Receive(bufferProj);
                                        using (MemoryStream ms = new MemoryStream(bufferProj, 0, brBajtaObjekat))
                                        {
                                            BinaryFormatter formatter = new BinaryFormatter();
                                            ZadatakProjekta zp = (ZadatakProjekta)formatter.Deserialize(ms);
                                            projekti.Add(zp);
                                        }
                                    }
                                    else if (opcija == 2)
                                    {
                                        Console.WriteLine($"Šaljem listu od {projekti.Count} projekata...");
                                        
                                        try
                                        {
                                            using (MemoryStream ms = new MemoryStream())
                                            {
                                                BinaryFormatter formatter = new BinaryFormatter();
                                                formatter.Serialize(ms, projekti);
                                                byte[] data = ms.ToArray();
                                                
                                                // Prvo pošalji dužinu podataka
                                                byte[] lengthBytes = BitConverter.GetBytes(data.Length);
                                                client.Send(lengthBytes);
                                                
                                                // Zatim pošalji podatke
                                                int totalSent = 0;
                                                while (totalSent < data.Length)
                                                {
                                                    int sent = client.Send(data, totalSent, data.Length - totalSent, SocketFlags.None);
                                                    totalSent += sent;
                                                }
                                                
                                                Console.WriteLine($"✓ Lista projekata poslata ({data.Length} bajtova)");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Greška pri serijalizaciji liste: {ex.Message}");
                                        }
                                    }
                                    else if (opcija == 0)
                                    {
                                        Console.WriteLine($"Klijent {client.RemoteEndPoint} izlazi");
                                        clientsToRemove.Add(client);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Nevalidna opcija primljena: '{message}'");
                                }
                            }
                            catch (SocketException ex)
                            {
                                if (ex.SocketErrorCode == SocketError.WouldBlock)
                                {
                                    continue;
                                }
                                else if (ex.SocketErrorCode == SocketError.ConnectionReset || ex.SocketErrorCode == SocketError.ConnectionAborted)
                                {
                                    Console.WriteLine($"Klijent {client.RemoteEndPoint} je prekinuo konekciju");
                                    clientsToRemove.Add(client);
                                }
                                else
                                {
                                    Console.WriteLine($"TCP greška: {ex.Message}");
                                    clientsToRemove.Add(client);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Opšta greška: {ex.Message}");
                                clientsToRemove.Add(client);
                            }
                        }
                    }

                    // Ukloni diskonektovane klijente
                    foreach (Socket client in clientsToRemove)
                    {
                        tcpClients.Remove(client);
                        try { client.Close(); } catch { }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Glavna greška: {ex.Message}");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
