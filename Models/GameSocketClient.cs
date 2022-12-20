using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Interfaz.Models
{
    public class GameSocketClient
    {
        public static List<GameSocketClient> L_GameSocketClients = new List<GameSocketClient>();

        private bool clientConstructionReady = false;
        public string Email { get; set; }
        public Socket ListenerSocket { get; set; }
        public Socket SenderSocket { get; set; }
        public ConcurrentQueue<string> l_SendQueueMessages { get; set; }
        public ConcurrentQueue<string> l_ReceiveQueueMessages { get; set; }
        public bool ClientConstructionReady { get => clientConstructionReady; set => clientConstructionReady = value; }
        public NetworkStream StreamSocket { get; set; }

        public GameSocketClient(Socket ListenerSocket)
        {
            this.Email = string.Empty;
            this.ListenerSocket = ListenerSocket;
            this.SenderSocket = null;
            this.l_SendQueueMessages = new ConcurrentQueue<string>();
            this.l_ReceiveQueueMessages = new ConcurrentQueue<string>();
            this.ClientConstructionReady = false;
        }

        public GameSocketClient()
        {
            this.Email = string.Empty;
            this.ListenerSocket = null;
            this.SenderSocket = null;
            this.l_SendQueueMessages = new ConcurrentQueue<string>();
            this.l_ReceiveQueueMessages = new ConcurrentQueue<string>();
            this.ClientConstructionReady = false;
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

                if (this.StreamSocket != null)
                {
                    this.StreamSocket.Close();
                    this.StreamSocket.Dispose();
                }

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
