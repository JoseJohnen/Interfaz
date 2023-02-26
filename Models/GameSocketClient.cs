using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Interfaz.Models
{
    public class GameSocketClient
    {
        public static List<GameSocketClient> L_GameSocketClients = new List<GameSocketClient>();

        private bool clientConstructionReady = false;
        private bool sendAsyncIsConnected = false;
        private ConcurrentQueue<string> l_sendBigMessages = new ConcurrentQueue<string>();

        private int receiveAccepted = 0;
        public string Email { get; set; }
        public Socket ListenerSocket { get; set; }
        public Socket SenderSocket { get; set; }
        public Socket StreamSocket { get; set; }
        public BlockingCollection<string> l_SendQueueMessages { get; set; }
        public BlockingCollection<string> l_SendBigMessages { get; set; }
        public BlockingCollection<string> l_ReceiveQueueMessages { get; set; }
        public BlockingCollection<string> l_ReceiveBigMessages { get; set; }
        public ConcurrentDictionary<uint, Message> dic_RegisterMessages { get; set; }
        public NetworkStream StreamNetwork { get; set; }
        public bool ClientConstructionReady { get => clientConstructionReady; set => clientConstructionReady = value; }
        public bool SendAsyncIsConnected { get => sendAsyncIsConnected; set => sendAsyncIsConnected = value; }
        public int ReceiveAccepted { get => receiveAccepted; set => receiveAccepted = value; }

        public GameSocketClient(Socket ListenerSocket)
        {
            this.Email = string.Empty;
            this.ListenerSocket = ListenerSocket;
            this.SenderSocket = null;
            this.StreamSocket = null;
            this.l_SendQueueMessages = new BlockingCollection<string>();
            this.l_ReceiveQueueMessages = new BlockingCollection<string>();
            this.l_SendBigMessages = new BlockingCollection<string>();
            this.l_ReceiveBigMessages = new BlockingCollection<string>();
            this.dic_RegisterMessages = new ConcurrentDictionary<uint, Message>();
            this.ClientConstructionReady = false;
            this.SendAsyncIsConnected = false;
        }

        public GameSocketClient()
        {
            this.Email = string.Empty;
            this.ListenerSocket = null;
            this.SenderSocket = null;
            this.StreamSocket = null;
            this.l_SendQueueMessages = new BlockingCollection<string>();
            this.l_ReceiveQueueMessages = new BlockingCollection<string>();
            this.l_SendBigMessages = new BlockingCollection<string>();
            this.l_ReceiveBigMessages = new BlockingCollection<string>();
            this.dic_RegisterMessages = new ConcurrentDictionary<uint, Message>();
            this.ClientConstructionReady = false;
            this.SendAsyncIsConnected = false;
        }

        public bool CloseConnection()
        {
            try
            {
                if(this.l_SendQueueMessages != null)
                {
                    if(this.l_SendQueueMessages.Count > 0)
                    {
                        this.l_SendQueueMessages = null;
                    }
                }

                if (this.l_ReceiveQueueMessages != null)
                {
                    if (this.l_ReceiveQueueMessages.Count > 0)
                    {
                        this.l_ReceiveQueueMessages = null;
                    }
                }

                if (this.l_SendBigMessages != null)
                {
                    if (this.l_SendBigMessages.Count > 0)
                    {
                        this.l_SendBigMessages = null;
                    }
                }

                if (this.l_ReceiveBigMessages != null)
                {
                    if (this.l_ReceiveBigMessages.Count > 0)
                    {
                        this.l_ReceiveBigMessages = null;
                    }
                }

                if (this.SenderSocket != null)
                {
                    this.SenderSocket.Shutdown(SocketShutdown.Both);
                    this.SenderSocket.Close();
                    this.SenderSocket.Dispose();
                }

                if (this.ListenerSocket != null)
                {
                    this.ListenerSocket.Shutdown(SocketShutdown.Both);
                    this.ListenerSocket.Close();
                    this.ListenerSocket.Dispose();
                }

                if (this.StreamNetwork != null)
                {
                    this.StreamNetwork.Close();
                    this.StreamNetwork.Dispose();
                    this.StreamSocket.Close();
                    this.StreamSocket.Dispose();
                }

                receiveAccepted = 0;
                GameSocketClient.L_GameSocketClients.Remove(this);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error CloseConnection: "+ex.Message);
                return false;
            }
        }

    }
}
