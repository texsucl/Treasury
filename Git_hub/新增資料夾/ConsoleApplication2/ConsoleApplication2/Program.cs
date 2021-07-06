using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            //StartClient($@"REMIT_OMS|A|a|77");
            //for (var i = 0; i < 10; i++)
            //{
            //    try
            //    {
            //        //StartClient(i.ToString());
            //        StartClient($@"REMIT_OMS|A|a|7{i}");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex);
            //    }

            //}
            //StartClient("190");
            //Task.Delay(30000).Wait();
            //StartClient("10");

            //Console.WriteLine(Convert.ToInt32("123345"));
            List<string> a = new List<string>() { "NTD", "EUR", "USD", "AAA", "ZZZ" };
            a.OrderBy(x => x != "NTD").ThenBy(x => x).ToList().ForEach(x => { Console.WriteLine(x); });
           
            Console.ReadLine();
        }

        public static void StartClient(string val)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // This example uses port 11000 on the local computer.  
                //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                //IPAddress ipAddress = ipHostInfo.AddressList[0];
                //IPAddress ipAddress = IPAddress.Parse("10.240.68.38");
                IPAddress ipAddress = IPAddress.Parse("10.240.1.81");
                //IPEndPoint remoteEP = new IPEndPoint(ipAddress, 5000);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 9202);

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);
                    sender.ReceiveTimeout = 3000;
                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    Encoding ascii = Encoding.ASCII;
                    Encoding ebcdic = Encoding.GetEncoding("IBM037");

                    // Encode the data string into a byte array.  
                    //byte[] msg = Encoding.ASCII.GetBytes($@"This is a test<EOF> {val}");



                    byte[] msg = Encoding.ASCII.GetBytes($@"{val}");

                    byte[] ebcdicBytes = Encoding.Convert(ascii, ebcdic, msg);

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(ebcdicBytes);

                    // Receive the response from the remote device.  
                    int bytesRec = sender.Receive(bytes);
                    Console.WriteLine("Echoed test = {0}",
                        Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    // Release the socket.  
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
