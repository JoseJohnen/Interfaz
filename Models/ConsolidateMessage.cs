using Interfaz.Code.Models;
using Interfaz.Utilities;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Linq;

namespace Interfaz.Models
{
    public class ConsolidateMessage
    {
        public static ConcurrentDictionary<string, ConsolidateMessage> dic_ActiveConsolidateMessages = new ConcurrentDictionary<string, ConsolidateMessage>();

        private static ConcurrentDictionary<string, Pares<DateTime, List<Message>>> dic_WarehouseMessages = new ConcurrentDictionary<string, Pares<DateTime, List<Message>>>();

        private string text = string.Empty;
        private uint parts = 0;
        private int length = 0;
        private uint idRef = 0;
        private string key = string.Empty;
        public StatusMessage Status = StatusMessage.NonRelevantUsage;

        public string Text { 
            get => text; 
            set 
            { 
                text = value;
                length = text.Length;
            } 
        }
        public uint Parts { get => parts; set => parts = value; }
        public int Length { get => length; private set => length = value; }
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
        public string Key { get => key; set => key = value; }

        #region Constructors
        public ConsolidateMessage()
        {
            Key = string.Empty;
            idRef = 0;
            Parts = 0;
            Length = 0;
            Status = StatusMessage.NonRelevantUsage;
        }

        private ConsolidateMessage(string stkey)
        {
            Key = stkey;
            idRef = 0;
            Parts = 0;
            Length = 0;
            Status = StatusMessage.NonRelevantUsage;
        }

        private ConsolidateMessage(uint idForRef, uint parts, int lngth, string stkey = "")
        {
            if (!string.IsNullOrWhiteSpace(stkey))
            {
                Key = stkey;
            }
            Status = StatusMessage.NonRelevantUsage;
            IdRef = idForRef;
            Parts = parts;
            Length = lngth;
        }
        #endregion

        #region Internal Asistencial Methods
        public static ConsolidateMessage CreateConsolidateMessage(string text, int remanent = 150)
        {
            try
            {
                uint idRefMsg = 1;
                while (dic_ActiveConsolidateMessages.Values.Where(c => c.idRef == idRefMsg).ToList().Count >= 1)
                {
                    idRefMsg++;
                }

                //Creación de mensaje 1
                uint i = 1;
                string tempText = text;
                string firstMessageText = tempText.Substring(0, remanent);
                Message frstmgs = Message.CreatePartMessage(firstMessageText, idRefMsg, i);
                List<Message> l_messages = new List<Message>
                {
                    frstmgs
                };

                //Creación de mensaje 2 en adelante
                int remanentInt = remanent;
                if (text.Length > remanentInt)
                {
                    while (!string.IsNullOrWhiteSpace(tempText))
                    {
                        i++;
                        if (tempText.Length < remanentInt)
                        {
                            remanentInt = tempText.Length;
                        }
                        Message mgs = Message.CreatePartMessage(tempText.Substring(0, remanentInt), idRefMsg, i);
                        l_messages.Add(mgs);
                        tempText = tempText.Replace(tempText.Substring(0, remanentInt), "");
                    }

                    //Creación de objeto consolidado (Si correponde)
                    ConsolidateMessage CnsMsg = new ConsolidateMessage(idRefMsg, i, text.Length);
                    CnsMsg.GenerateKey();
                    foreach (Message mgs in l_messages)
                    {
                        //ConsolidateMessage.dic_WarehouseMessages.TryAdd(CnsMsg.Key, new Pares<DateTime, Message>(DateTime.Now, mgs));
                        CnsMsg.TryAddMessageToWarehouse(mgs);
                    }

                    return CnsMsg;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ConsolidateMessage CreateConsolidateMessage(string): " + ex.Message);
                return new ConsolidateMessage();
            }
        }

        private string GenerateKey()
        {
            try
            {
                string a = string.Empty;
                string keyAttempt = string.Empty;
                do
                {
                    a = DateTime.Now.ToString() + (idRef + parts + length).ToString();
                    keyAttempt = UtilityAssistant.Base64Encode(a).Substring(0, 10);
                }
                while (dic_ActiveConsolidateMessages.ContainsKey(keyAttempt) || string.IsNullOrWhiteSpace(keyAttempt));
                key = keyAttempt;
                return key;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string GenerateKey(): " + ex.Message);
                return string.Empty;
            }
        }
        #endregion

        #region Dictionarys and List Administration
        public bool TryAddMessageToWarehouse(Message message, out Message messageConsolidate)
        {
            try
            {
                bool result = false;
                messageConsolidate = null;
                Pares<DateTime, List<Message>> Pair_Dt_LsMsg = null;
                Pares<DateTime, List<Message>> Old_Pair_Dt_LsMsg = null;
                if (ConsolidateMessage.dic_WarehouseMessages.TryGetValue(key, out Pair_Dt_LsMsg))
                {
                    Old_Pair_Dt_LsMsg = Pair_Dt_LsMsg;
                    if (Pair_Dt_LsMsg != null)
                    {
                        if (Pair_Dt_LsMsg.Item2 != null)
                        {
                            if (Pair_Dt_LsMsg.Item2.Contains(message) == false)
                            {
                                Pair_Dt_LsMsg.Item2.Add(message);
                                if (ConsolidateMessage.dic_WarehouseMessages.TryUpdate(key, Pair_Dt_LsMsg, Old_Pair_Dt_LsMsg))
                                {
                                    result = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    List<Message> l_msg = new List<Message>() { message };
                    Pares<DateTime, List<Message>> pair_dt_l_msg = new Pares<DateTime, List<Message>>(DateTime.Now, l_msg);
                    if (ConsolidateMessage.dic_WarehouseMessages.TryAdd(key, pair_dt_l_msg))
                    {
                        result = true;
                    }
                }

                if (ConsolidateMessage.dic_WarehouseMessages.TryGetValue(key, out Pair_Dt_LsMsg))
                {
                    if (parts == Pair_Dt_LsMsg.Item2.Count)
                    {
                        //Here you consolidate
                        Message messageResult = new Message();
                        foreach (Message item in Pair_Dt_LsMsg.Item2.OrderBy(c => c.IdMsg))
                        {
                            messageResult.text += item.text;
                        }
                        //Here you return the consolidate, it will be null if it's not yet consolidated
                        messageConsolidate = messageResult;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error bool TryAddMessageToWarehouse(strMessage, out strMessage): " + ex.Message);
                messageConsolidate = null;
                return false;
            }
        }

        public bool TryAddMessageToWarehouse(Message message)
        {
            try
            {
                Pares<DateTime, List<Message>> Pair_Dt_LsMsg = null;
                Pares<DateTime, List<Message>> Old_Pair_Dt_LsMsg = null;
                if (ConsolidateMessage.dic_WarehouseMessages.TryGetValue(key, out Pair_Dt_LsMsg))
                {
                    Old_Pair_Dt_LsMsg = Pair_Dt_LsMsg;
                    if (Pair_Dt_LsMsg != null)
                    {
                        if (Pair_Dt_LsMsg.Item2 != null)
                        {
                            if (Pair_Dt_LsMsg.Item2.Contains(message) == false)
                            {
                                Pair_Dt_LsMsg.Item2.Add(message);
                                if (ConsolidateMessage.dic_WarehouseMessages.TryUpdate(key, Pair_Dt_LsMsg, Old_Pair_Dt_LsMsg))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    List<Message> l_msg = new List<Message>() { message };
                    Pares<DateTime, List<Message>> pair_dt_l_msg = new Pares<DateTime, List<Message>>(DateTime.Now, l_msg);
                    if (ConsolidateMessage.dic_WarehouseMessages.TryAdd(key, pair_dt_l_msg))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error bool TryAddMessageToWarehouse(strMessage): " + ex.Message);
                return false;
            }
        }

        public bool TryRemoveMessageFromWarehouse_ThroughKey(out Pares<DateTime, List<Message>> par)
        {
            try
            {
                if (ConsolidateMessage.dic_WarehouseMessages.TryRemove(key, out par))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error bool TryRemoveMessageFromWarehouse_ThroughKey(): " + ex.Message);
                par = null;
                return false;
            }
        }

        public bool TryRemoveSpecificMessageFromWarehouse(Message msg)
        {
            try
            {
                foreach (Pares<DateTime, List<Message>> item in ConsolidateMessage.dic_WarehouseMessages.Values)
                {
                    if (item.Item2.Contains(msg))
                    {
                        item.Item2.Remove(msg);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error bool TryRemoveSpecificMessageFromWarehouse(): " + ex.Message);
                return false;
            }
        }
        #endregion

        #region Register Related Methods
        public bool SelfRegister()
        {
            try
            {
                ConsolidateMessage cnMsg = new ConsolidateMessage();
                if (!ConsolidateMessage.dic_ActiveConsolidateMessages.TryGetValue(this.key, out cnMsg))
                {
                    if (!this.Equals(cnMsg))
                    {
                        if (ConsolidateMessage.dic_ActiveConsolidateMessages.TryAdd(this.key, this))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error bool SelfRegister(): " + ex.Message);
                return false;
            }
        }

        public bool UnRegister()
        {
            try
            {
                ConsolidateMessage cnMsg = new ConsolidateMessage();
                if (!ConsolidateMessage.dic_ActiveConsolidateMessages.TryRemove(this.key, out cnMsg))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error bool UnRegister(): " + ex.Message);
                return false;
            }
        }
        #endregion

        #region JSON Related Methods
        public static bool CheckJSONMessageIfMatch(string message, out Message msgResult)
        {
            try
            {
                msgResult = null;
                string strMessage = UtilityAssistant.CleanJSON(message);
                string specificRelevantInstruction = strMessage;
                if (strMessage.Contains("MS:"))
                {
                    specificRelevantInstruction = UtilityAssistant.ExtractValues(strMessage, "MS");
                }

                if (String.IsNullOrWhiteSpace(specificRelevantInstruction))
                {
                    return false;
                }

                //Some cleaning, if required, because clean for Json and cleaning for extranction ARE DIFFERENT
                //And can generate problems when in this method
                if (!specificRelevantInstruction.Contains("{"))
                {
                    specificRelevantInstruction = specificRelevantInstruction.Replace("\"}", "");
                }
                //END Special Cleaning

                Message nwMsg = Message.CreateFromJson(specificRelevantInstruction);
                if (!string.IsNullOrWhiteSpace(nwMsg.TextOriginal))
                {
                    ConsolidateMessage cnMsg = new ConsolidateMessage();
                    if (ConsolidateMessage.TryCreateFromJson(nwMsg.TextOriginal, out cnMsg))
                    {
                        ConsolidateMessage.dic_ActiveConsolidateMessages.TryAdd(cnMsg.key, cnMsg);
                        List<Message> l_WarehouseMessage = ConsolidateMessage.dic_WarehouseMessages.Where(c => c.Key == cnMsg.key).Select(c => c.Value.Item2).FirstOrDefault();
                        if (l_WarehouseMessage != null)
                        {
                            if (l_WarehouseMessage.Count() > 0)
                            {
                                List<uint> l_uints = l_WarehouseMessage.Select(c => c.IdMsg).ToList();
                                MissingMessages mmMsg = new MissingMessages();

                                for (uint i = 1; i <= cnMsg.parts; i++)
                                {
                                    if (!l_uints.Contains(i))
                                    {
                                        mmMsg.l_missingMessages.Add(i.ToString());
                                    }
                                }
                                MissingMessages.q_MissingMessages.Enqueue(mmMsg);
                            }
                        }
                    }
                }

                return CheckMessageIfMatch(nwMsg, out msgResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error static bool CheckJSONMessageIfMatch(string, out strMessage): " + ex.Message);
                msgResult = null;
                return false;
            }
        }

        public static bool CheckJSONMessageIfMatch(string message)
        {
            try
            {
                string strMessage = UtilityAssistant.CleanJSON(message);
                string specificRelevantInstruction = UtilityAssistant.ExtractValues(strMessage, "MS");

                if (String.IsNullOrWhiteSpace(specificRelevantInstruction))
                {
                    return false;
                }

                //Some cleaning, if required, because clean for Json and cleaning for extranction ARE DIFFERENT
                //And can generate problems when in this method
                if (specificRelevantInstruction.Contains("{"))
                {
                    specificRelevantInstruction = specificRelevantInstruction.Replace("\"}", "");
                }
                //END Special Cleaning

                Message nwMsg = Message.CreateFromJson(specificRelevantInstruction);
                if (!string.IsNullOrWhiteSpace(nwMsg.TextOriginal))
                {
                    ConsolidateMessage cnMsg = new ConsolidateMessage();
                    if (ConsolidateMessage.TryCreateFromJson(nwMsg.TextOriginal, out cnMsg))
                    {
                        ConsolidateMessage.dic_ActiveConsolidateMessages.TryAdd(cnMsg.key, cnMsg);
                        List<Message> l_WarehouseMessage = ConsolidateMessage.dic_WarehouseMessages.Where(c => c.Key == cnMsg.key).Select(c => c.Value.Item2).FirstOrDefault();
                        if (l_WarehouseMessage != null)
                        {
                            if (l_WarehouseMessage.Count() > 0)
                            {
                                List<uint> l_uints = l_WarehouseMessage.Select(c => c.IdMsg).ToList();
                                MissingMessages mmMsg = new MissingMessages();

                                for (uint i = 1; i <= cnMsg.parts; i++)
                                {
                                    if (!l_uints.Contains(i))
                                    {
                                        mmMsg.l_missingMessages.Add(i.ToString());
                                    }
                                }
                                MissingMessages.q_MissingMessages.Enqueue(mmMsg);
                            }
                        }
                    }
                }

                return CheckMessageIfMatch(nwMsg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error static bool CheckJSONMessageIfMatch(string): " + ex.Message);
                return false;
            }
        }

        public static bool CheckMessageIfMatch(Message message, out Message msgResult)
        {
            try
            {
                msgResult = null;
                if (message.IdRef == 0)
                {
                    return false;
                }

                if (ConsolidateMessage.dic_ActiveConsolidateMessages.Count > 0)
                {
                    foreach (ConsolidateMessage item in ConsolidateMessage.dic_ActiveConsolidateMessages.Values)
                    {
                        if (item.IdRef == message.IdRef)
                        {
                            if (item.TryAddMessageToWarehouse(message))
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(message.TextOriginal))
                    {
                        ConsolidateMessage cnMsg = new ConsolidateMessage();
                        if (ConsolidateMessage.TryCreateFromJson(message.TextOriginal, out cnMsg))
                        {
                            ConsolidateMessage.dic_ActiveConsolidateMessages.TryAdd(cnMsg.key, cnMsg);
                        }

                        foreach (ConsolidateMessage item in ConsolidateMessage.dic_ActiveConsolidateMessages.Values)
                        {
                            if (item.IdRef == message.IdRef)
                            {
                                if (item.TryAddMessageToWarehouse(message, out msgResult))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error static bool CheckMessageIfMatch(strMessage, out strMessage): " + ex.Message);
                msgResult = null;
                return false;
            }
        }

        public static bool CheckMessageIfMatch(Message message)
        {
            try
            {
                if (message.IdRef == 0)
                {
                    return false;
                }

                if (ConsolidateMessage.dic_ActiveConsolidateMessages.Count > 0)
                {
                    foreach (ConsolidateMessage item in ConsolidateMessage.dic_ActiveConsolidateMessages.Values)
                    {
                        if (item.IdRef == message.IdRef)
                        {
                            if (item.TryAddMessageToWarehouse(message))
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(message.TextOriginal))
                    {
                        ConsolidateMessage cnMsg = new ConsolidateMessage();
                        if (ConsolidateMessage.TryCreateFromJson(message.TextOriginal, out cnMsg))
                        {
                            ConsolidateMessage.dic_ActiveConsolidateMessages.TryAdd(cnMsg.key, cnMsg);
                        }

                        foreach (ConsolidateMessage item in ConsolidateMessage.dic_ActiveConsolidateMessages.Values)
                        {
                            if (item.IdRef == message.IdRef)
                            {
                                if (item.TryAddMessageToWarehouse(message))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error static bool CheckMessageIfMatch(strMessage): " + ex.Message);
                return false;
            }
        }

        public string ToJson()
        {
            try
            {
                string result = System.Text.Json.JsonSerializer.Serialize(this);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (ConsolidateMessage) ToJson: " + ex.Message);
                return string.Empty;
            }
        }

        public ConsolidateMessage FromJson(string Text)
        {
            string txt = Text;
            try
            {
                txt = UtilityAssistant.CleanJSON(txt);
                ConsolidateMessage cdMsg = System.Text.Json.JsonSerializer.Deserialize<ConsolidateMessage>(txt);

                this.parts = cdMsg.parts;
                this.text = cdMsg.text;
                this.length = cdMsg.length;
                this.idRef = cdMsg.idRef;
                this.key = cdMsg.key;

                return cdMsg;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (ConsolidateMessage) FromJson: " + ex.Message + " Text: " + txt);
                return new ConsolidateMessage();
            }
        }

        public bool TryFromJson(string Text)
        {
            string txt = Text;
            try
            {
                txt = UtilityAssistant.CleanJSON(txt);
                ConsolidateMessage cdMsg = System.Text.Json.JsonSerializer.Deserialize<ConsolidateMessage>(txt);

                this.parts = cdMsg.parts;
                this.text = cdMsg.text;
                this.length = cdMsg.length;
                this.idRef = cdMsg.idRef;
                this.Key = cdMsg.Key;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (ConsolidateMessage) TryFromJson: " + ex.Message + " Text: " + txt);
                return false;
            }
        }

        public static ConsolidateMessage CreateFromJson(string json)
        {
            try
            {
                ConsolidateMessage msg = new ConsolidateMessage();
                return msg.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (ConsolidateMessage) CreateFromJson: " + ex.Message);
                return new ConsolidateMessage();
            }
        }

        public static bool TryCreateFromJson(string json, out ConsolidateMessage consolidateMessage)
        {
            try
            {
                ConsolidateMessage msg = new ConsolidateMessage();
                if (msg.TryFromJson(json))
                {
                    consolidateMessage = msg;
                    return true;
                }
                consolidateMessage = null;
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (ConsolidateMessage) TryCreateFromJson: " + ex.Message);
                consolidateMessage = null;
                return false;
            }
        }
        #endregion

    }

    public class DistinctConsolidateMessageComparer : IEqualityComparer<ConsolidateMessage>
    {

        public bool Equals(ConsolidateMessage x, ConsolidateMessage y)
        {
            return x.Key == y.Key &&
                x.Text == y.Text &&
                x.Text == y.Text &&
                x.IdRef == y.IdRef;
        }

        public int GetHashCode(ConsolidateMessage obj)
        {
            return obj.Key.GetHashCode() ^
                obj.Parts.GetHashCode() ^
                obj.Text.GetHashCode() ^
                obj.IdRef.GetHashCode();
        }
    }

}