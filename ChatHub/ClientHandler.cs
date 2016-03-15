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
    class ClientHandler
    {
        //List of active threads in this class
        List<Thread> activeThreads = new List<Thread>();

        //Network streams
        NetworkStream netStream;
        StreamReader sReader;
        StreamWriter sWriter;

        //Triggered when a message is received
        public static AutoResetEvent rMessagesRE = new AutoResetEvent(false);
        public AutoResetEvent sMessagesRE = new AutoResetEvent(false);

        //Received messages and messages to send
        public object LOCKrMessages = new object();
        public Queue<string> rMessages = new Queue<string>();
        public object LOCKsMessages = new object();
        public Queue<string> sMessages = new Queue<string>();

        //Constructor
        public ClientHandler (TcpClient client)
        {
            Console.WriteLine("ClientHandler created for {0}", client.ToString());


            //Initialize network stream and reader/writer
            netStream = client.GetStream();
            sReader = new StreamReader(netStream);
            sWriter = new StreamWriter(netStream);
            sWriter.AutoFlush = true;

            //Create and add receiver thread to list
            Thread receiveThread = new Thread(new ThreadStart(receiveLoop));
            activeThreads.Add(receiveThread);

            //Create and add sender thread to list
            Thread sendThread = new Thread(new ThreadStart(sendLoop));
            activeThreads.Add(sendThread);

            //Start the two threads
            sendThread.Start();
            receiveThread.Start();
        }

        //Pass a message to the sendLoop
        public void sendMessage(string msg)
        {
            lock(LOCKsMessages)
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
        void receiveLoop ()
        {
            while (true)
            {
                Console.WriteLine("Waiting for message");
                //Add message to queue
                rMessages.Enqueue(sReader.ReadLine());

                //Trigger the AutoResetEvent for received messages
                rMessagesRE.Set();
            }
        }
    }
}
