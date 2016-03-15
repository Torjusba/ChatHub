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

        //Messages to send
        public object LOCKsMessages = new object();
        public Queue<string> sMessages = new Queue<string>();

        string name = "placeHolderName";

        //Constructor
        public Client (string ip_string, int port_input)
        {
            //Set ip and port
            ip = IPAddress.Parse(ip_string);
            port = port_input;

            //Connect
            tClient.Connect(ip, port);
            Console.WriteLine("Connected");
            
            //Update the network stream with connection
            netStream = tClient.GetStream();
            sReader = new StreamReader(netStream);
            sWriter = new StreamWriter(netStream);
            sWriter.AutoFlush = true;

            //Start threads for sending and receiving
            Thread receiverThread = new Thread(new ThreadStart(receiveLoop));
            activeThreads.Add(receiverThread);
            Thread senderThread = new Thread(new ThreadStart(sendLoop));
            activeThreads.Add(senderThread);

            receiverThread.Start();
            senderThread.Start();

            while (true)
            {
                string input = Console.ReadLine();
                string[] cmd = input.ToLower().Split(' ');
                string msg = "";
                switch (cmd[0])
                {
                    case "#setname":
                        name = cmd[1];
                        break;
                    default:
                        msg = input;
                        break;
                }

                if (msg != "")
                {
                    msg = String.Format("{0}: {1}", name, msg);
                    sendMessage(msg);
                }
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
                handleMessage(sReader.ReadLine());
             }
        }

        //Deal with the message received
        void handleMessage(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
