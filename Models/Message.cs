using Interfaz.Utilities;
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Interfaz.Models
{
    public enum StatusMessage { Error = -2, NonRelevantUsage = -1, ReadyToSend = 0, Delivered = 1, Executed = 2 }

    public class Message
    {
        public static ConcurrentDictionary<uint, Message> dic_ActiveMessages = new ConcurrentDictionary<uint, Message>();

        private string text = string.Empty;

        private bool IsBlockMultiMessage = false;

        public string Text
        {
            get
            {
                //Console.WriteLine("b: "+b);
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine("\n\nSize of the message is: " + text.Length + " total");
                Console.ResetColor();
                return text;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    text = value;
                    return;
                }
                string tempText = value;
                this.Length = (uint)tempText.Length;
                int remanent = 150;
                if (tempText.Length > remanent && !this.IsBlockMultiMessage)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.WriteLine("this.Length: "+ this.Length);
                    Console.ResetColor();

                    //int single = Convert.ToInt32(MathF.Ceiling(tempText.Length / 150));
                    string bufferTotal = tempText;
                    //string bufferTemporal = string.Empty;
                    //List<string> pieces = new List<string>();

                    uint idUsedRef = 0;
                    if(dic_ActiveMessages.Count >= 1)
                    {
                        this.IdMsg = 1;
                        foreach (Message msgItem in dic_ActiveMessages.Values)
                        {
                            idUsedRef = msgItem.idRef;
                        }
                    }
                    idUsedRef++;
                    this.idRef = idUsedRef;

                    //Preparar mensaje que indica total
                    //Message msg = Message.CreateMessage();
                    text = UtilityAssistant.Base64Encode("LG:" + this.Length);
                    dic_ActiveMessages.TryAdd(this.IdMsg, this);

                    uint i = 2;
                    while (!string.IsNullOrWhiteSpace(bufferTotal))
                    {
                        if(bufferTotal.Length < remanent)
                        {
                            remanent = bufferTotal.Length;
                        }
                        Message mgs = Message.CreateMessage(bufferTotal.Substring(0, remanent), true);
                        mgs.IdMsg = i;
                        mgs.idRef = idUsedRef;
                        dic_ActiveMessages.TryAdd(mgs.IdMsg, mgs);
                        bufferTotal = bufferTotal.Replace(bufferTotal.Substring(0, remanent), "");
                        i++;
                    }
                }
                else
                {
                    text = Utilities.UtilityAssistant.Base64Encode(tempText);
                }
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n\nSize of the message is: " + text.Length + " total");
                Console.ResetColor();
            }
        }

        public string TextOriginal
        {
            get
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
                string a = text;
                bool ifSeveral = false;
                if (a.Contains("="))
                {
                    while (a.Contains("==") || (a.LastIndexOf("=") == (a.Length - 1)))
                    {
                        ifSeveral = true;
                        a = Utilities.UtilityAssistant.Base64Decode(a);
                        //Console.WriteLine("a: "+a);
                    }
                }

                string b = a;//Utilities.UtilityAssistant.Base64Decode(text);
                if (!ifSeveral)
                {
                    b = Utilities.UtilityAssistant.Base64Decode(text);
                }
                //Console.WriteLine("b: "+b);
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine("\n\nSize of the message is: " + b.Length + " total");
                Console.ResetColor();
                return b;
            }
        }

        private uint idMsg = 0;

        private uint length = 0;

        private uint idRef = 0;

        public uint IdRef
        {
            get
            {
                return idRef;
            }
            set
            {
                idRef = value;
                if (idRef != 0)
                {
                    Status = StatusMessage.ReadyToSend;
                }
            }
        }

        public uint Length { get => length; internal set => length = value; }
        public uint IdMsg { get => idMsg; set => idMsg = value; }

        public StatusMessage Status = StatusMessage.NonRelevantUsage;

        public Message()
        {
            IdMsg = 0;
            Text = string.Empty;
            idRef = 0;
            Status = StatusMessage.NonRelevantUsage;
        }

        private Message(string text)
        {
            IdMsg = 0;
            Text = Utilities.UtilityAssistant.Base64Encode(text);
            idRef = 0;
            Status = StatusMessage.NonRelevantUsage;
        }

        private Message(uint idForRef, string text)
        {
            IdMsg = 0;
            Text = Utilities.UtilityAssistant.Base64Encode(text);
            Status = StatusMessage.NonRelevantUsage;
            IdRef = idForRef;
        }

        public static Message CreateMessage(string text, bool BlockChainOfMessages = false, uint idForRef = 0)
        {
            try
            {
                Message msg = new Message();
                if(BlockChainOfMessages)
                {
                    msg.IsBlockMultiMessage = true; //it start false, therefore is unnecesarrely to change it if somebody add it as a false
                }
                msg.Text = UtilityAssistant.CleanJSON(text);
                if (idForRef != 0)
                {
                    msg.idRef = idForRef;
                }
                return msg;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Message CreateMessage: " + ex.Message);
                return new Message();
            }
        }

        public string ToJson()
        {
            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    IgnoreReadOnlyProperties = true
                };
                string result = System.Text.Json.JsonSerializer.Serialize(this, options);
                /*length = (uint)this.ToJson().Length;
                int lngCount = length.ToString().Length;
                length = length + (uint)lngCount;*/
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Message) ToJson: " + ex.Message);
                return string.Empty;
            }
        }

        public Message FromJson(string Text)
        {
            string txt = Text;
            try
            {
                txt = UtilityAssistant.CleanJSON(txt);
                JsonSerializerOptions options= new JsonSerializerOptions()
                {
                    IgnoreReadOnlyProperties = true
                };
                return System.Text.Json.JsonSerializer.Deserialize<Message>(txt, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Message) FromJson: " + ex.Message + " Text: " + txt);
                return new Message();
            }
        }

        public static Message CreateFromJson(string json)
        {
            try
            {
                Message msg = new Message();
                return msg.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Message) CreateFromJson: " + ex.Message);
                return new Message();
            }
        }

        public static bool AddToList(string textInMessage, List<string> l_values)
        {
            try
            {
                uint index = 0;
                uint lastindex = 0;
                foreach (GameSocketClient item in GameSocketClient.L_GameSocketClients)
                {
                    foreach (string itm in l_values)
                    {
                        bool isAlreadyAdded = false;
                        do
                        {
                            if (item.dic_RegisterMessages.Count > 0)
                            {
                                index = (uint)item.dic_RegisterMessages.Count;
                            }

                            Message msg = new Message(index, textInMessage + ":" + itm);
                            msg.IdRef = lastindex;
                            isAlreadyAdded = item.dic_RegisterMessages.TryAdd(index, msg);

                            if (lastindex == 0)
                            {
                                if (msg.Text.Contains("MV:")) //MV: Should never enter here, probably, But Just In Case
                                {
                                    item.l_SendQueueMessages.Enqueue("MS:" + msg.ToJson());
                                }
                                else
                                {
                                    item.l_SendBigMessages.Enqueue("MS:" + msg.ToJson());
                                }
                            }

                            if (item.dic_RegisterMessages.Count > 0)
                            {
                                lastindex = index;
                            }
                        }
                        while (isAlreadyAdded == false);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (bool) AddToList(string, List<string>): " + ex.Message);
                return false;
            }
        }

        public static Message AddToList(string textInMessage, int? PosicionGameSocketClient = null)
        {
            try
            {
                Message msg = new Message();
                bool isAlreadyAdded = false;
                uint index = 0;
                int i = 0;

                if (PosicionGameSocketClient != null)
                {
                    do
                    {
                        i = PosicionGameSocketClient.Value;
                        index = (uint)GameSocketClient.L_GameSocketClients[i].dic_RegisterMessages.Count;
                        msg = new Message(index, textInMessage);
                        isAlreadyAdded = GameSocketClient.L_GameSocketClients[i].dic_RegisterMessages.TryAdd(index, msg);
                    }
                    while (isAlreadyAdded == false);
                    return msg;
                }
                else
                {
                    foreach (GameSocketClient item in GameSocketClient.L_GameSocketClients)
                    {
                        do
                        {
                            index = (uint)item.dic_RegisterMessages.Count;
                            msg = new Message(index, textInMessage);
                            isAlreadyAdded = item.dic_RegisterMessages.TryAdd(index, msg);
                            i++;
                        }
                        while (isAlreadyAdded == false);
                    }
                    return msg;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Message) AddToList(string, int?): " + ex.Message);
                return new Message();
            }
        }

        public static bool RemoveToList(StateMessage stMessage, int PosicionGameSocketClient, out Message outMessage)
        {
            Message msg = new Message();
            outMessage = msg;
            try
            {
                if (stMessage == null)
                {
                    return false;
                }

                if (stMessage.Status == StatusMessage.Executed)
                {
                    if (Message.dic_ActiveMessages.Remove(stMessage.idRef, out msg))
                    {
                        outMessage = msg;
                        return true;
                    }
                    return true;
                }
                else
                {
                    if (Message.dic_ActiveMessages.TryGetValue(stMessage.idRef, out msg))
                    {
                        if (msg != null)
                        {
                            msg.Status = StatusMessage.ReadyToSend;
                            if (Message.dic_ActiveMessages.TryUpdate(stMessage.idRef, msg, msg))
                            {
                                string typeOf = msg.Text.Substring(0, 2);
                                if (typeOf.Contains("MV") || typeOf.Contains("ST") || typeOf.Contains("SS"))
                                {
                                    GameSocketClient.L_GameSocketClients[PosicionGameSocketClient].l_SendQueueMessages.Enqueue("MS:" + msg.ToJson());
                                    return false;
                                }
                                GameSocketClient.L_GameSocketClients[PosicionGameSocketClient].l_SendBigMessages.Enqueue("MS:" + msg.ToJson());
                                return false;
                            }
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Message) RemoveToList: " + ex.Message);
                return false;
            }
        }

    }

    public class DistinctMessageComparer : IEqualityComparer<Message>
    {

        public bool Equals(Message x, Message y)
        {
            return x.IdMsg == y.IdMsg &&
                x.Text == y.Text &&
                x.IdRef == y.IdRef;
        }

        public int GetHashCode(Message obj)
        {
            return obj.IdMsg.GetHashCode() ^
                obj.Text.GetHashCode() ^
                obj.IdRef.GetHashCode();
        }
    }

}