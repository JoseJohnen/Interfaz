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
        public string Email { get; set; }
        public Socket ListenerSocket { get; set; }
        public Socket SenderSocket { get; set; }
        public Socket StreamSocket { get; set; }
        public BlockingCollection<string> l_SendQueueMessages { get; set; }
        public BlockingCollection<string> l_SendBigMessages { get; set; }
        public BlockingCollection<string> l_ReceiveQueueMessages { get; set; }
        public BlockingCollection<string> l_ReceiveBigMessages { get; set; }

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
            l_SendQueueMessages = new BlockingCollection<string>();
            l_ReceiveQueueMessages = new BlockingCollection<string>();
            l_SendBigMessages = new BlockingCollection<string>();
            l_ReceiveBigMessages = new BlockingCollection<string>();
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
            l_SendQueueMessages = new BlockingCollection<string>();
            l_ReceiveQueueMessages = new BlockingCollection<string>();
            l_SendBigMessages = new BlockingCollection<string>();
            l_ReceiveBigMessages = new BlockingCollection<string>();
            dic_RegisterMessages = new ConcurrentDictionary<uint, Message>();
            ClientConstructionReady = false;
            SendAsyncIsConnected = false;
        }

        public bool CloseConnection()
        {
            try
            {
                if (l_SendQueueMessages != null)
                {
                    if (l_SendQueueMessages.Count > 0)
                    {
                        l_SendQueueMessages = null;
                    }
                }

                if (l_ReceiveQueueMessages != null)
                {
                    if (l_ReceiveQueueMessages.Count > 0)
                    {
                        l_ReceiveQueueMessages = null;
                    }
                }

                if (l_SendBigMessages != null)
                {
                    if (l_SendBigMessages.Count > 0)
                    {
                        l_SendBigMessages = null;
                    }
                }

                if (l_ReceiveBigMessages != null)
                {
                    if (l_ReceiveBigMessages.Count > 0)
                    {
                        l_ReceiveBigMessages = null;
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
