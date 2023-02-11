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
        public static ConcurrentDictionary<string, Message> dic_ActiveMessages = new ConcurrentDictionary<string, Message>();

        [JsonInclude]
        public string text = string.Empty;

        private bool IsBlockMultiMessage = false;


        [JsonIgnore]
        public string Text
        {
            get
            {
                //Console.WriteLine("b: "+b);
                /*Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine("\n\nSize of the message is: " + text.Length + " total");
                Console.ResetColor();*/
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
                    ConcurrentDictionary<string, Message> dic_newMessages = new ConcurrentDictionary<string, Message>();

                    uint idUsedRef = 0;
                    if (dic_ActiveMessages.Count >= 1)
                    {
                        foreach (Message msgItem in dic_ActiveMessages.Values)
                        {
                            idUsedRef = msgItem.idRef;
                        }
                    }
                    idUsedRef++;

                    //Preparar mensaje que indica total
                    string textFirstMessage = tempText.Substring(0, remanent);

                    //Creación de mensaje 2 en adelante
                    uint i = 1;
                    tempText = tempText.Replace(tempText.Substring(0, remanent), "");
                    while (!string.IsNullOrWhiteSpace(tempText))
                    {
                        i++;
                        if (tempText.Length < remanent)
                        {
                            remanent = tempText.Length;
                        }
                        Message mgs = Message.CreatePartMessage(tempText.Substring(0, remanent), idUsedRef, i);
                        dic_newMessages.TryAdd(string.Concat(mgs.IdMsg, "|", mgs.IdRef), mgs);
                        tempText = tempText.Replace(tempText.Substring(0, remanent), "");
                    }

                    //Creación de mensaje 1
                    this.idRef = idUsedRef;
                    this.IdMsg = 1;
                    //Este text manda: cantidad total de mensajes || length del primer mensaje || primera pieza del mensaje
                    this.text = i + "||" + textFirstMessage.Length + "||" + UtilityAssistant.Base64Encode(textFirstMessage);
                    dic_newMessages.TryAdd(string.Concat(this.IdMsg, "|", this.IdRef), this);

                    foreach (KeyValuePair<string, Message> item in dic_newMessages)
                    {
                        dic_ActiveMessages.TryAdd(item.Key, item.Value);
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

        [JsonIgnore]
        public string TextOriginal
        {
            get
            {
                string a = text;
                try
                {
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }

                    if (a.Contains("||"))
                    {
                        string[] idkfa = a.Split("||");
                        if (idkfa.Length > 2)
                        {
                            a = idkfa[2];
                        }
                    }

                    string resultTryDecode = string.Empty;
                    if (UtilityAssistant.TryBase64Decode(a, out resultTryDecode))
                    {
                        a = resultTryDecode;
                    }

                    return a;
                }
                catch (Exception ex)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine("\n\nError Property TextOriginal Message: " + ex.Message);
                    Console.ResetColor();
                    return a;
                }
            }
            private set
            {
                text = value;
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

        public uint Length { get => length; private set => length = value; }
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
            Text = text;
            idRef = 0;
            Status = StatusMessage.NonRelevantUsage;
        }

        private Message(uint idForRef, string text)
        {
            IdMsg = 0;
            Text = text;
            Status = StatusMessage.NonRelevantUsage;
            IdRef = idForRef;
        }

        public static Message CreateMessage(string text, bool BlockChainOfMessages = false, uint idForRef = 0)
        {
            try
            {
                Message msg = new Message();
                if (BlockChainOfMessages)
                {
                    msg.IsBlockMultiMessage = true; //it start false, therefore is unnecesarrely to change it if somebody add it as a false
                }
                string marker = string.Empty;
                if (text.Length >= 4)
                {
                    if (text.Substring(0, 4).Contains(":"))
                    {
                        marker = text.Substring(0,text.IndexOf(":")+1);
                        text = text.ReplaceFirst(marker,"");
                    }
                }
                string jsonAProc = UtilityAssistant.CleanJSON(text);
                if(!string.IsNullOrWhiteSpace(marker))
                {
                    jsonAProc = marker + jsonAProc;
                }
                msg.text = UtilityAssistant.Base64Encode(jsonAProc);
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

        public static Message CreatePartMessage(string text, uint idForRef = 0, uint idMessage = 0)
        {
            try
            {
                Message msg = new Message();
                msg.text = text;
                if (idForRef != 0)
                {
                    msg.idRef = idForRef;
                }
                if (idMessage != 0)
                {
                    msg.idMsg = idMessage;
                }
                return msg;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Message CreatePartMessage: " + ex.Message);
                return new Message();
            }
        }

        public static Message ConsolidateMessages(Message nwMsg)
        {
            try
            {
                Message.dic_ActiveMessages.TryAdd(string.Concat(nwMsg.IdMsg, "|", nwMsg.IdRef), nwMsg);
                List<Message> l_messages = Message.dic_ActiveMessages.Where(c => c.Value.IdRef == nwMsg.IdRef).Select(c => c.Value).ToList();
                Message frsMsg = l_messages.Where(c => c.IdMsg == 1).FirstOrDefault();

                if (frsMsg != null)
                {
                    int tamanoTotal = Convert.ToInt32(frsMsg.length);
                    l_messages = l_messages.Distinct(new DistinctMessageComparer()).ToList();
                    //Obtener total de mensajes relevantes
                    l_messages = l_messages.Where(c => c.IdRef == nwMsg.IdRef).ToList();

                    //Sumar total Length
                    int sumTotal = (int)l_messages.Where(c => c.idMsg != 1).Sum(c => c.Length);
                    int messageTotalAmmount = 0;
                    int firstMessageRealLength = (int)frsMsg.ObtainActualLengthAndMessageAmmount(out messageTotalAmmount);
                    //END sumart total length

                    if (messageTotalAmmount == l_messages.Count)
                    {
                        string textRecopilado = string.Empty;
                        int sumTotalPlusFirst = 0;

                        foreach (Message m in l_messages.OrderBy(c => c.IdMsg))
                        {
                            if (string.Concat(m.IdMsg, "|", m.IdRef) == string.Concat(1, "|", m.IdRef))
                            {
                                string stringRemovedLength = frsMsg.RemoveLengthFromText();
                                textRecopilado = stringRemovedLength;
                                sumTotalPlusFirst += firstMessageRealLength;
                                continue;
                            }
                            textRecopilado += m.TextOriginal;
                            sumTotalPlusFirst += m.TextOriginal.Length;
                        }

                        Message msgEmpty = new Message();
                        foreach (Message item in l_messages)
                        {
                            Message.dic_ActiveMessages.Remove(string.Concat(item.IdMsg, "|", item.IdRef), out msgEmpty);
                        }
                        return Message.CreateMessage(textRecopilado, true);

                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Message ConsolidateMessages: " + ex.Message);
                return new Message();
            }
        }

        public uint ObtainActualLengthAndMessageAmmount(out int messageTotalAmmount)
        {
            messageTotalAmmount = 0;
            try
            {
                if (this.TextOriginal.Contains("||"))
                {
                    string[] parts = this.TextOriginal.Split("||");
                    int actualLength = 0;
                    //Not actual length BUT using the same memory slot for passing the data, to save a little ram
                    if (int.TryParse(parts[0], out actualLength))
                    {
                        messageTotalAmmount = actualLength;
                    }
                    if (int.TryParse(parts[1], out actualLength))
                    {
                        this.IsBlockMultiMessage = true;
                        string specificString = parts[2];
                        string DecodedNewBaseText = string.Empty;
                        string[] strTemp = new string[1];
                        if (UtilityAssistant.TryBase64Decode(specificString, out DecodedNewBaseText))
                        {
                            strTemp = DecodedNewBaseText.Split("||");
                        }
                        else
                        {
                            strTemp[0] = specificString;
                        }
                        //byte[] base64EncodedBytes = System.Convert.FromBase64String(specificString);
                        //string DecodedNewBaseText = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                        //base64EncodedBytes = System.Convert.FromBase64String(strTemp[1]);
                        if (strTemp.Length == 1)
                        {
                            this.text = strTemp[0];//System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                        }
                        else
                        {
                            this.text = strTemp[2];//System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                        }
                        return (uint)actualLength;
                    }
                }
                return this.length;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uint ObtainActualLengthAndMessageAmmount(out int messageTotalAmmount): " + ex.Message);
                return this.length;
            }
        }

        public string RemoveLengthFromText()
        {
            string[] parts = this.TextOriginal.Split("||");
            try
            {
                if (parts.Length == 2)
                {
                    this.IsBlockMultiMessage = true;
                    this.text = parts[1];
                    return this.text;
                }
                return parts[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string RemoveLengthFromText: " + ex.Message);
                return parts[0];
            }
        }

        public string ToJson()
        {
            try
            {
                /*JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    IgnoreReadOnlyProperties = true,
                };*/
                string result = System.Text.Json.JsonSerializer.Serialize(this);//, options);
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
                /*JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    IgnoreReadOnlyProperties = true
                };*/
                return System.Text.Json.JsonSerializer.Deserialize<Message>(txt);//, options);
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

                return false;
                //TODO: Este método esta en desuso y deshabilitado porque: 1) ya no se trabajan los mensajes así, se cambió la forma y el método
                //se volvió redundante
                //2) después de ese cambio, mas adelante durante el desarrollo, se cambió la naturaleza del key, así que ya no sirve y corregir el 
                //método de momento no se ve relevante, por eso se comento todo el interior
                /*if (stMessage.Status == StatusMessage.Executed)
                {
                    if (Message.dic_ActiveMessages.Remove(string.Concat(stMessage.IdMsg, "|", stMessage.IdRef), out msg))
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
                }*/
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