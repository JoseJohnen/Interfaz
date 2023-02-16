using Interfaz.Code.Models;
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
        public static ConcurrentDictionary<string, Pares<DateTime, Message>> dic_ActiveMessages = new ConcurrentDictionary<string, Pares<DateTime, Message>>();

        public static ConcurrentDictionary<string, Pares<DateTime, Message>> dic_BackUpMessages = new ConcurrentDictionary<string, Pares<DateTime, Message>>();

        [JsonInclude]
        public string text = string.Empty;

        private bool IsBlockMultiMessage = false;


        [JsonIgnore]
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    text = value;
                    return;
                }
                string tempText = Utilities.UtilityAssistant.Base64Encode(value);
                this.Length = (uint)tempText.Length;
                int remanent = 150;
                if (tempText.Length > remanent && !this.IsBlockMultiMessage)
                {
                    ConsolidateMessage csMsg = ConsolidateMessage.CreateConsolidateMessage(tempText);
                    if(csMsg != null)
                    {
                        text = Utilities.UtilityAssistant.Base64Encode("CM:" + csMsg.ToJson()); //No se pone TAG porque funciona antes del sistema de TAG
                    }
                    else
                    {
                        text = "ERROR_:_CONSOLIDATE_MESSAGE_CREATE_ON_MESSAGE_TEXT";
                    }
                    /*ConcurrentDictionary<string, Message> dic_newMessages = new ConcurrentDictionary<string, Message>();

                    uint idUsedRef = 0;
                    if (dic_ActiveMessages.Count >= 1)
                    {
                        foreach (Pares<DateTime, Message> msgItem in dic_ActiveMessages.Values)
                        {
                            idUsedRef = msgItem.Item2.idRef;
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
                    this.text = i + "||" + textFirstMessage.Length + "||" + textFirstMessage;
                    dic_newMessages.TryAdd(string.Concat(this.IdMsg, "|", this.IdRef), this);

                    foreach (KeyValuePair<string, Message> item in dic_newMessages.OrderBy(c => c.Key))
                    {
                        dic_ActiveMessages.TryAdd(item.Key, new Pares<DateTime, Message>(DateTime.Now, item.Value));
                        dic_BackUpMessages.TryAdd(item.Key, new Pares<DateTime, Message>(DateTime.Now, item.Value));
                    }*/
                }
                else
                {
                    text = tempText; //Utilities.UtilityAssistant.Base64Encode(tempText);
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
            IdRef = 0;
            Status = StatusMessage.NonRelevantUsage;
        }

        private Message(string text)
        {
            IdMsg = 0;
            Text = text;
            IdRef = 0;
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
                        marker = text.Substring(0, text.IndexOf(":") + 1);
                        text = text.ReplaceFirst(marker, "");
                    }
                }
                string jsonAProc = UtilityAssistant.CleanJSON(text);
                if (!string.IsNullOrWhiteSpace(marker))
                {
                    jsonAProc = marker + jsonAProc;
                }
                msg.Text = jsonAProc;//UtilityAssistant.Base64Encode(jsonAProc);
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
                msg.length = (uint)text.Length;
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

        /*
        public static Message ConsolidateMessages(Message nwMsg)
        {
            try
            {
                Message.dic_ActiveMessages.TryAdd(string.Concat(nwMsg.IdMsg, "|", nwMsg.IdRef), new Pares<DateTime, Message>(DateTime.Now, nwMsg));
                List<Pares<DateTime, Message>> l_dtmessages = Message.dic_ActiveMessages.Where(c => c.Value.Item2.IdRef == nwMsg.IdRef && nwMsg.idRef > 0).Select(c => c.Value).ToList();

                //Si no hay ni se espera ningún otro mensaje de acompañamiento, y el idRef dice que no debería esperar ningún otro
                //en ese caso es un mensaje único y no necesita consolidación
                if (l_dtmessages.Count == 0)
                {
                    return nwMsg;
                }

                Message frsMsg = l_dtmessages.Where(c => c.Item2.IdMsg == 1).Select(c => c.Item2).FirstOrDefault();

                if (frsMsg != null)
                {
                    int tamanoTotal = Convert.ToInt32(frsMsg.length);
                    List<Message> l_messages = l_dtmessages.Select(c => c.Item2).ToList();

                    l_messages = l_messages.Distinct(new DistinctMessageComparer()).ToList();
                    //Obtener total de mensajes relevantes
                    l_messages = l_messages.Where(c => c.IdRef == nwMsg.IdRef).ToList();

                    //Sumar total Length
                    int sumTotal = (int)l_messages.Where(c => c.idMsg != 1).Sum(c => c.Length);
                    int messageTotalAmmount = 0;
                    int firstMessageRealLength = (int)frsMsg.ObtainActualLengthAndMessageAmmount(out messageTotalAmmount);
                    //END sumart total length

                    if ((messageTotalAmmount > 0 || l_messages.Exists(c => c.idMsg == messageTotalAmmount)) &&
                        l_messages.Count < messageTotalAmmount)
                    {
                        //Then you need to request again some messages than went missing for one reason or another
                        bool isTimeForRequestedRemainingMsgAlready = false;
                        foreach (Pares<DateTime, Message> item in l_dtmessages)
                        {
                            //timer to determine if we should check that or not
                            if ((DateTime.Now - item.Item1 > new TimeSpan(0, 0, 1)) && !isTimeForRequestedRemainingMsgAlready)
                            {
                                isTimeForRequestedRemainingMsgAlready = true;
                            }
                        }

                        List<string> l_missingMessages = new List<string>();
                        if (isTimeForRequestedRemainingMsgAlready)
                        {
                            for (int i = 1; i <= messageTotalAmmount; i++)
                            {
                                foreach (Pares<DateTime, Message> item in l_dtmessages.OrderBy(c => c.Item2.idMsg))
                                {
                                    if(item.Item2.idMsg == i)
                                    {
                                        i++;
                                        continue;
                                    }
                                    l_missingMessages.Add(item.Item2.idMsg + "|"+item.Item2.idRef);
                                }

                            }
                            //Request messages
                            MissingMessages.q_MissingMessages.Enqueue(new MissingMessages(l_missingMessages));
                        }

                        return null;
                    }

                    //It can never been 0, so it safe
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

                        Pares<DateTime, Message> msgEmpty = new Pares<DateTime, Message>(DateTime.Now, new Message());
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
                if (this.Text.Contains("||"))
                {
                    string[] parts = this.Text.Split("||");
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
                return (uint)this.Text.Length;
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
        */

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
                                if (msg.TextOriginal.Contains("MV:")) //MV: Should never enter here, probably, But Just In Case
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
                x.text == y.text &&
                x.IdRef == y.IdRef;
        }

        public int GetHashCode(Message obj)
        {
            return obj.IdMsg.GetHashCode() ^
                obj.text.GetHashCode() ^
                obj.IdRef.GetHashCode();
        }
    }

}