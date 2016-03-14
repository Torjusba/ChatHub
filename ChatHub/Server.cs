using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatHub
{
    class Server
    {
        //The main TCP Listener and local IP address
        public TcpListener tListener;
        IPAddress localIP = IPAddress.Any;

        //List of active server threads
        List<Thread> activeThreads = new List<Thread>();

        //Main Queue of messages to distribute
        private Queue<string> sendQueue = new Queue<string>();
        object LOCKsendQueue = new object();

        //List of all the client handlers created
        private List<ClientHandler> clientHandlers = new List<ClientHandler>();

        //Are we waiting for new connections?
        private bool waitingForConnections;



        //Constructor
        public Server (int port)
        {
            //Set the port and create a TCP listener
            tListener = new TcpListener(localIP, port);

            //Create a separate thread to accept incoming connection attempts
            Thread connectionThread = new Thread(new ThreadStart(ReceiveConnections));
            activeThreads.Add(connectionThread);
            connectionThread.Start();

            //Create a searate thread for and distributing messages
            Thread msgThread = new Thread(new ThreadStart(msgDistributionLoop));
            activeThreads.Add(msgThread);
            msgThread.Start();


        }

        void msgDistributionLoop()
        {
            while (true)
            {
                ClientHandler.rMessagesRE.WaitOne();
                lock(LOCKsendQueue)
                {
                    foreach (ClientHandler ch in clientHandlers)
                    {
                        lock (ch.LOCKrMessages)
                        {
                            foreach (string msg in ch.rMessages)
                            {
                                sendQueue.Enqueue(msg);
                            }
                            ch.rMessages.Clear();
                        }
                    }
                    foreach(string s in sendQueue)
                    {
                        Console.WriteLine(s);
                    }
                    sendQueue.Clear();
                }
            }
        }


        //Receive a connection and create a new ClientHandler for it
        void ReceiveConnections()
        { 
            //Start the listener and prepare a socket
            tListener.Start();
            Console.WriteLine("Started listening");
            Socket socket;

            waitingForConnections = true;
            //Repeatedly look for connections while the server wants to
            while (waitingForConnections)
            {
                //Receive a connection, create a ClientHandler and add it to the list
                socket = tListener.AcceptSocket();
                clientHandlers.Add(new ClientHandler(socket));
                Console.WriteLine("Connected to {0}", socket.RemoteEndPoint);
            }
        }
    }
}
