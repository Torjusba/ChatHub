using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatHub
{
    class Client
    {
        //Main TCP Client
        TcpClient tClient = new TcpClient();

        //Main network streams
        NetworkStream netStream;
        StreamReader sReader;
        StreamWriter sWriter;

        //Active threads
        List<Thread> activeThreads = new List<Thread>();

        //IP address and port information
        IPAddress ip;
        int port;

        //Triggered when a message is received
        AutoResetEvent rMessagesRE = new AutoResetEvent(false);
        public AutoResetEvent sMessagesRE = new AutoResetEvent(false);

        //Received messages and messages to send
        public object LOCKrMessages = new object();
        public Queue<string> rMessages = new Queue<string>();
        public object LOCKsMessages = new object();
        public Queue<string> sMessages = new Queue<string>();



        //Constructor
        public Client (string ip_string, int port_input)
        {
            //Set ip and port
            ip = IPAddress.Parse(ip_string);
            port = port_input;

            //Connect
            tClient.Connect(ip, port);
            Console.WriteLine("Connected");

            Thread.Sleep(1000);

            Console.WriteLine("done");

            //Update the network stream with connection
            netStream = tClient.GetStream();
            sReader = new StreamReader(netStream);
            sWriter = new StreamWriter(netStream);

            //Start threads for sending and receiving
            Thread receiverThread = new Thread(new ThreadStart(receiveLoop));
            activeThreads.Add(receiverThread);
            Thread senderThread = new Thread(new ThreadStart(sendLoop));
            activeThreads.Add(senderThread);

            receiverThread.Start();
            senderThread.Start();

            while (true)
            {
                Console.Write("msg: ");
                sendMessage(Console.ReadLine());
            }

        }

        //Pass a message to the sendLoop
        public void sendMessage(string msg)
        {
            lock (LOCKsMessages)
            {
                sMessages.Enqueue(msg);
            }

            //Tell the loop that a new message is added
            sMessagesRE.Set();
        }

        //Sends all messages
        void sendLoop()
        {
            while (true)
            {
                //wait if the queue is empty
                if (sMessages.Count == 0)
                {
                    sMessagesRE.WaitOne();
                }

                //lock the queue to avoid simultaneous editing
                lock (LOCKsMessages)
                {
                    //Send all the queued messages
                    foreach (string msg in sMessages)
                    {
                        Console.WriteLine("Sent {0}", msg);
                        sWriter.WriteLine(msg);
                    }
                    sMessages.Clear();
                }
            }
        }

        //Receive messages and add to queue
        void receiveLoop()
        {
            while (true)
            {
                //Add message to queue
                rMessages.Enqueue(sReader.ReadLine());

                //Trigger the AutoResetEvent for received messages
                rMessagesRE.Set();
            }
        }
    }
}
