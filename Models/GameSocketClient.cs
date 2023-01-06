using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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
        public ConcurrentQueue<string> l_SendQueueMessages { get; set; }
        //public ConcurrentQueue<string> l_SendBigMessages { get; set; }
        public ConcurrentQueue<string> l_SendBigMessages { get; set; }
        public ConcurrentQueue<string> l_ReceiveQueueMessages { get; set; }
        public ConcurrentQueue<string> l_ReceiveBigMessages { get; set; }
        public NetworkStream StreamNetwork { get; set; }
        public bool ClientConstructionReady { get => clientConstructionReady; set => clientConstructionReady = value; }
        public bool SendAsyncIsConnected { get => sendAsyncIsConnected; set => sendAsyncIsConnected = value; }
        public int ReceiveAccepted { get => receiveAccepted; set => receiveAccepted = value; }

        public GameSocketClient(Socket ListenerSocket)
        {
            this.Email = string.Empty;
            this.ListenerSocket = ListenerSocket;
            this.SenderSocket = null;
            this.l_SendQueueMessages = new ConcurrentQueue<string>();
            this.l_ReceiveQueueMessages = new ConcurrentQueue<string>();
            this.l_SendBigMessages = new ConcurrentQueue<string>();
            this.l_ReceiveBigMessages = new ConcurrentQueue<string>();
            this.ClientConstructionReady = false;
            this.SendAsyncIsConnected = false;
        }

        public GameSocketClient()
        {
            this.Email = string.Empty;
            this.ListenerSocket = null;
            this.SenderSocket = null;
            this.l_SendQueueMessages = new ConcurrentQueue<string>();
            this.l_ReceiveQueueMessages = new ConcurrentQueue<string>();
            this.l_SendBigMessages = new ConcurrentQueue<string>();
            this.l_ReceiveBigMessages = new ConcurrentQueue<string>();
            this.ClientConstructionReady = false;
            this.SendAsyncIsConnected = false;
        }

        public bool CloseConnection()
        {
            try
            {
                if(this.l_SendQueueMessages != null)
                {
                    if(this.l_SendQueueMessages.Count >0)
                    {
                        this.l_SendQueueMessages.Clear();
                    }
                }

                if (this.l_ReceiveQueueMessages != null)
                {
                    if (this.l_ReceiveQueueMessages.Count > 0)
                    {
                        this.l_ReceiveQueueMessages.Clear();
                    }
                }

                if (this.l_SendBigMessages != null)
                {
                    if (this.l_SendBigMessages.Count > 0)
                    {
                        this.l_SendBigMessages.Clear();
                    }
                }

                if (this.l_ReceiveBigMessages != null)
                {
                    if (this.l_ReceiveBigMessages.Count > 0)
                    {
                        this.l_ReceiveBigMessages.Clear();
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
