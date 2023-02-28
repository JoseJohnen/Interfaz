using Interfaz.Utilities;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
        //public Thread workerThread = null;
        //public int index = 0;

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

        /*static async Task SendSteamAsync(int position)
        {
            int pos = position;
            try
            {
                string strIp = ((IPEndPoint)GameSocketClient.L_GameSocketClients[position].ListenerSocket.RemoteEndPoint).Address.ToString().Replace("f", "").Replace(":", "");
                if (GameSocketClient.L_GameSocketClients[position] == null)
                {
                    GameSocketClient.L_GameSocketClients[position] = new GameSocketClient();
                    GameSocketClient.L_GameSocketClients[position].index = position;
                }

                if (GameSocketClient.L_GameSocketClients[position] != null)
                {
                    if (GameSocketClient.L_GameSocketClients[position].StreamSocket == null)
                    {
                        bool makeSenderSocket = false;
                        TaskStatus tstatus = TaskStatus.Created;
                        GameSocketClient.L_GameSocketClients[position].StreamSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                        await GameSocketClient.L_GameSocketClients[position].StreamSocket.ConnectAsync(strIp, sendPort);
                    }

                    if (!GameSocketClient.L_GameSocketClients[position].StreamSocket.Connected)
                    {
                        await GameSocketClient.L_GameSocketClients[position].StreamSocket.ConnectAsync(strIp, sendPort);
                    }

                    if (GameSocketClient.L_GameSocketClients[position].StreamNetwork == null)
                    {
                        GameSocketClient.L_GameSocketClients[position].StreamNetwork = new NetworkStream(GameSocketClient.L_GameSocketClients[position].StreamSocket);
                    }
                }

                bool stopUpdateProcessing = false;
                string item = string.Empty;
                while (stopUpdateProcessing == false)
                {
                    if (GameSocketClient.L_GameSocketClients[position] != null)
                    {
                        //if (GameSocketClient.L_GameSocketClients[position].SenderSocket != null)
                        //{
                        if (GameSocketClient.L_GameSocketClients[position].l_SendBigMessages.Count > 0)
                        {
                            while (GameSocketClient.L_GameSocketClients[position].l_SendBigMessages.TryTake(out item))
                            //while (GameSocketClient.L_GameSocketClients[position].l_SendBigMessages.TryDequeue(out item))
                            {
                                if (string.IsNullOrWhiteSpace(item))
                                {
                                    continue;
                                }

                                //Example of "Brute Processing" to close connection with the server
                                if (item.Equals("<EXIT>"))
                                {
                                    stopUpdateProcessing = true;
                                }

                                byte[] requestBytes = Encoding.ASCII.GetBytes(item);

                                await GameSocketClient.L_GameSocketClients[position].StreamNetwork.WriteAsync(requestBytes, 0, requestBytes.Length);

                                Console.WriteLine("\nSending (Stream)..." + item + " count: " + requestBytes.Length);
                                //await Task.Delay(TimeSpan.FromSeconds(1));
                            }
                            //GameSocketClient.L_GameSocketClients[position].l_SendBigMessages.Clear();
                        }
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLineAsync("Error SendSteamAsync: " + ex.Message + " Cant Ops: " + GameSocketClient.L_GameSocketClients[pos].l_SendBigMessages.Count);
            }
            finally
            {
                if (GameSocketClient.L_GameSocketClients[position] != null)
                {
                    if (GameSocketClient.L_GameSocketClients[position].SenderSocket != null)
                    {
                        if (GameSocketClient.L_GameSocketClients[position].SenderSocket.Connected)
                        {
                            GameSocketClient.L_GameSocketClients[position].CloseConnection();
                        }
                    }
                }
            }
        }*/

        /*static async Task ReceiveSteamAsync(int position)
        {
            int pos = position;
            try
            {
                if (GameSocketClient.L_GameSocketClients[position].StreamNetwork == null)
                {
                    GameSocketClient.L_GameSocketClients[position].StreamNetwork = new NetworkStream(GameSocketClient.L_GameSocketClients[position].StreamSocket);
                }

                int baseSize = 1024;
                byte[] responseBytes = new byte[baseSize];
                char[] responseChars = new char[baseSize];

                int size = 1000;

                while (true)
                {
                    if (GameSocketClient.L_GameSocketClients[position].StreamSocket.Available > size)
                    {
                        size = GameSocketClient.L_GameSocketClients[position].StreamSocket.Available;
                        responseBytes = new byte[size];
                        responseChars = new char[size];
                    }

                    List<byte> allData = new List<byte>();
                    int numBytesRead = 0;
                    if (GameSocketClient.L_GameSocketClients[position].StreamNetwork.DataAvailable && GameSocketClient.L_GameSocketClients[position].StreamNetwork.CanRead)
                    {
                        do
                        {
                            numBytesRead = await GameSocketClient.L_GameSocketClients[position].StreamNetwork.ReadAsync(responseBytes, 0, responseBytes.Length);

                            if (numBytesRead == responseBytes.Length)
                            {
                                allData.AddRange(responseBytes);
                            }
                            else if (numBytesRead > 0)
                            {
                                allData.AddRange(responseBytes.Take(numBytesRead));
                            }
                        } while (GameSocketClient.L_GameSocketClients[position].StreamNetwork.DataAvailable);
                    }

                    // Convert byteCount bytes to ASCII characters using the 'responseChars' buffer as destination
                    int charCount = Encoding.ASCII.GetChars(allData.ToArray(), 0, numBytesRead, responseChars, 0);

                    if (charCount == 0) continue;

                    string responseString = new String(responseChars).Replace("\0", "");
                    string first3Char = string.Empty;
                    if (responseString.IndexOf(":") <= 6)
                    {
                        first3Char = responseString.Substring(0, responseString.IndexOf(":") + 1);
                        responseString = UtilityAssistant.CleanJSON(responseString);
                    }

                    PlayerController.TalkTurn(responseString, position);

                    await Console.Out.WriteAsync("\nReceived (StreamReader): first3Char: " + first3Char + "\n" + responseString);

                    responseBytes = new byte[baseSize];
                    responseChars = new char[baseSize];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Received (StreamReader): " + ex.Message + " Cant Ops: " + GameSocketClient.L_GameSocketClients[pos].l_ReceiveBigMessages.Count);
            }
            finally
            {
                Console.WriteLine("Algo paso y murio el receiverStream");
                if (GameSocketClient.L_GameSocketClients[position] != null)
                {
                    if (GameSocketClient.L_GameSocketClients[position].SenderSocket != null)
                    {
                        if (GameSocketClient.L_GameSocketClients[position].SenderSocket.Connected)
                        {
                            GameSocketClient.L_GameSocketClients[position].CloseConnection();
                        }
                    }
                }
            }
        }*/

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
