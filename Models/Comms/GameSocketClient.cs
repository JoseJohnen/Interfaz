using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Interfaz.Models.Comms
{
    public class GameSocketClient
    {
        public static List<GameSocketClient> L_GameSocketClients = new List<GameSocketClient>();

        private bool clientConstructionReady = false;
        private bool sendAsyncIsConnected = false;
        private ConcurrentQueue<string> l_sendBigMessages = new ConcurrentQueue<string>();

        private int receiveAccepted = 0;
        private BlockingCollection<string> l_SendQueueMessages;
        private BlockingCollection<string> l_SendBigMessages;
        private BlockingCollection<string> l_ReceiveQueueMessages;
        private BlockingCollection<string> l_ReceiveBigMessages;

        public string Email { get; set; }
        public Socket ListenerSocket { get; set; }
        public Socket SenderSocket { get; set; }
        public Socket StreamSocket { get; set; }

        public BlockingCollection<string> L_SendQueueMessages { 
            get 
            { 
                if(l_SendQueueMessages == null)
                {
                    l_SendQueueMessages = new BlockingCollection<string>();
                }
                return l_SendQueueMessages;
            }
            set => l_SendQueueMessages = value; 
        }
        public BlockingCollection<string> L_SendBigMessages {
            get
            {
                if (l_SendBigMessages == null)
                {
                    l_SendBigMessages = new BlockingCollection<string>();
                }
                return l_SendBigMessages;
            }
            set => l_SendBigMessages = value; 
        }
        public BlockingCollection<string> L_ReceiveQueueMessages {
            get
            {
                if (l_ReceiveQueueMessages == null)
                {
                    l_ReceiveQueueMessages = new BlockingCollection<string>();
                }
                return l_ReceiveQueueMessages;
            }
            set => l_ReceiveQueueMessages = value; 
        }
        public BlockingCollection<string> L_ReceiveBigMessages {
            get 
            {
                if (l_ReceiveBigMessages == null)
                {
                    l_ReceiveBigMessages = new BlockingCollection<string>();
                }
                return l_ReceiveBigMessages;
            } 
            set => l_ReceiveBigMessages = value; 
        }

        public Player player { get; set; } = new Player();

        public ConcurrentDictionary<uint, Message> dic_RegisterMessages { get; set; }
        public NetworkStream StreamNetwork { get; set; }
        public bool ClientConstructionReady { get => clientConstructionReady; set => clientConstructionReady = value; }
        public bool SendAsyncIsConnected { get => sendAsyncIsConnected; set => sendAsyncIsConnected = value; }
        public int ReceiveAccepted { get => receiveAccepted; set => receiveAccepted = value; }

        public GameSocketClient(Socket ListenerSocket)
        {
            Email = string.Empty;
            this.ListenerSocket = ListenerSocket;
            SenderSocket = null;
            StreamSocket = null;
            L_SendQueueMessages = new BlockingCollection<string>();
            L_ReceiveQueueMessages = new BlockingCollection<string>();
            L_SendBigMessages = new BlockingCollection<string>();
            L_ReceiveBigMessages = new BlockingCollection<string>();
            dic_RegisterMessages = new ConcurrentDictionary<uint, Message>();
            ClientConstructionReady = false;
            SendAsyncIsConnected = false;
        }

        public GameSocketClient()
        {
            Email = string.Empty;
            ListenerSocket = null;
            SenderSocket = null;
            StreamSocket = null;
            L_SendQueueMessages = new BlockingCollection<string>();
            L_ReceiveQueueMessages = new BlockingCollection<string>();
            L_SendBigMessages = new BlockingCollection<string>();
            L_ReceiveBigMessages = new BlockingCollection<string>();
            dic_RegisterMessages = new ConcurrentDictionary<uint, Message>();
            ClientConstructionReady = false;
            SendAsyncIsConnected = false;
        }

        public bool CloseConnection()
        {
            try
            {
                if (L_SendQueueMessages != null)
                {
                    if (L_SendQueueMessages.Count > 0)
                    {
                        L_SendQueueMessages = null;
                    }
                }

                if (L_ReceiveQueueMessages != null)
                {
                    if (L_ReceiveQueueMessages.Count > 0)
                    {
                        L_ReceiveQueueMessages = null;
                    }
                }

                if (L_SendBigMessages != null)
                {
                    if (L_SendBigMessages.Count > 0)
                    {
                        L_SendBigMessages = null;
                    }
                }

                if (L_ReceiveBigMessages != null)
                {
                    if (L_ReceiveBigMessages.Count > 0)
                    {
                        L_ReceiveBigMessages = null;
                    }
                }

                if (SenderSocket != null)
                {
                    SenderSocket.Shutdown(SocketShutdown.Both);
                    SenderSocket.Close();
                    SenderSocket.Dispose();
                }

                if (ListenerSocket != null)
                {
                    ListenerSocket.Shutdown(SocketShutdown.Both);
                    ListenerSocket.Close();
                    ListenerSocket.Dispose();
                }

                if (StreamNetwork != null)
                {
                    StreamNetwork.Close();
                    StreamNetwork.Dispose();
                    StreamSocket.Close();
                    StreamSocket.Dispose();
                }

                receiveAccepted = 0;
                L_GameSocketClients.Remove(this);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error CloseConnection: " + ex.Message);
                return false;
            }
        }

    }
}
