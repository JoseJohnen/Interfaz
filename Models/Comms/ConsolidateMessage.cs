using Interfaz.Models.Auxiliary;
using Interfaz.Utilities;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace Interfaz.Models.Comms
{
    public class ConsolidateMessage
    {
        private static ConcurrentDictionary<string, ConsolidateMessage> dic_ActiveConsolidateMessages = new ConcurrentDictionary<string, ConsolidateMessage>();

        private static ConcurrentDictionary<string, Pares<DateTime, List<Message>>> dic_WarehouseMessages = new ConcurrentDictionary<string, Pares<DateTime, List<Message>>>();

        private static DateTime LastIteration = DateTime.Now;

        private string text = string.Empty;
        private uint parts = 0;
        private int length = 0;
        private uint idRef = 0;
        private string key = string.Empty;
        public StatusMessage Status = StatusMessage.NonRelevantUsage;

        [JsonIgnore]
        public string Text
        {
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
                tempText = tempText.Replace(firstMessageText, "");
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
                    ConsolidateMessage CnsMsg = new ConsolidateMessage(idRefMsg, (uint)l_messages.Count, text.Length);
                    CnsMsg.Text = text;
                    CnsMsg.GenerateKey();
                    CnsMsg.SelfRegister();
                    foreach (Message mgs in l_messages)
                    {
                        //MakeMessageConsolidated.dic_WarehouseMessages.TryAdd(CnsMsg.Key, new Pares<DateTime, Message>(DateTime.Now, mgs));
                        CnsMsg.TryAddMessageToWarehouse(mgs);
                    }

                    return CnsMsg;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error MakeMessageConsolidated CreateConsolidateMessage(string): " + ex.Message);
                return new ConsolidateMessage();
            }
        }

        public static List<Message> CheckMissingMessages()
        {
            try
            {
                List<Message> l_result_messages = new List<Message>();
                if (DateTime.Now - LastIteration >= new TimeSpan(0, 0, 0, 0, 25))
                {
                    if (dic_ActiveConsolidateMessages.Count > 0)
                    {
                        foreach (ConsolidateMessage cnMsg in dic_ActiveConsolidateMessages.Values)
                        {
                            List<uint> l_uints = new List<uint>();
                            MissingMessages mmMsg = new MissingMessages();
                            //List<Message> l_WarehouseMessage = MakeMessageConsolidated.dic_WarehouseMessages.Where(c => c.Key == cnMsg.key).Select(c => c.Value.Item2).FirstOrDefault();
                            Pares<DateTime, List<Message>> pares_datetime_l_messages = new(DateTime.Now, new List<Message>());
                            if (TryGetMessageFromWarehouse_ThroughKey(cnMsg.Key, out pares_datetime_l_messages))
                            {
                                if (DateTime.Now - pares_datetime_l_messages.Item1 >= new TimeSpan(0, 0, 0, 0, 50))
                                {
                                    foreach (Message item in pares_datetime_l_messages.Item2)
                                    {
                                        if (item != null)
                                        {
                                            l_uints.Add(item.IdMsg);
                                        }
                                    }

                                    for (uint i = 1; i <= cnMsg.parts; i++)
                                    {
                                        if (!l_uints.Contains(i))
                                        {
                                            mmMsg.l_missingMessages.Add(i.ToString());
                                        }
                                        mmMsg.extraValue = cnMsg.Key;
                                    }

                                    if (mmMsg.l_missingMessages.Count > 0)
                                    {
                                        MissingMessages.q_MissingMessages.Enqueue(mmMsg);
                                    }
                                    else
                                    {
                                        Message consolidatedMessage = cnMsg.MakeMessageConsolidated();
                                        if (consolidatedMessage != null)
                                        {
                                            l_result_messages.Add(consolidatedMessage);
                                            cnMsg.CleanCloseConsolidateMessage();
                                        }

                                    }

                                    //TODO: Check if this work AND update the data in the dic_Warehouse
                                    pares_datetime_l_messages.Item1 = DateTime.Now;
                                    LastIteration = DateTime.Now;
                                    continue;
                                }
                            }
                            else
                            {
                                for (uint i = 1; i <= cnMsg.parts; i++)
                                {
                                    mmMsg.l_missingMessages.Add(i.ToString());
                                }
                                mmMsg.extraValue = cnMsg.Key;

                                if (mmMsg.l_missingMessages.Count > 0)
                                {
                                    MissingMessages.q_MissingMessages.Enqueue(mmMsg);
                                }
                                else
                                {
                                    Message consolidatedMessage = cnMsg.MakeMessageConsolidated();
                                    if (consolidatedMessage != null)
                                    {
                                        l_result_messages.Add(consolidatedMessage);
                                        cnMsg.CleanCloseConsolidateMessage();
                                    }
                                }
                                continue;
                            }
                        }
                    }
                }
                return l_result_messages;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error static List<Message> CheckMissingMessages(): " + ex.Message);
                return new List<Message>();
            }
        }

        private Message MakeMessageConsolidated()
        {
            try
            {
                Pares<DateTime, List<Message>> pares = null;
                Message nwMsg = new Message();
                if (TryGetMessageFromWarehouse_ThroughKey(Key, out pares))
                {
                    List<Message> l_collectedAlready = pares.Item2.Distinct(new DistinctMessageComparer()).ToList();
                    int totalParts = l_collectedAlready.Count;
                    if (parts == totalParts)
                    {
                        foreach (Message item in l_collectedAlready.OrderBy(c => c.IdMsg))
                        {
                            nwMsg.text += item.text;
                        }
                    }
                }

                nwMsg.text = nwMsg.text.Replace("\u002B", "+").Replace("u002B", "+");

                return nwMsg;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Message MakeMessageConsolidated(): " + ex.Message);
                return null;
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

        private bool CleanCloseConsolidateMessage()
        {
            try
            {
                Pares<DateTime, List<Message>> par = null;
                TryRemoveMessageFromWarehouse_ThroughKey(Key, out par);
                UnRegister();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error bool CleanCloseConsolidateMessage(): " + ex.Message);
                return false;
            }
        }

        private static bool CleanCloseConsolidateMessage(ConsolidateMessage consolidateMessage)
        {
            try
            {
                Pares<DateTime, List<Message>> par = null;
                TryRemoveMessageFromWarehouse_ThroughKey(consolidateMessage.Key, out par);
                consolidateMessage.UnRegister();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error static bool CleanCloseConsolidateMessage(): " + ex.Message);
                return false;
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
                if (dic_WarehouseMessages.TryGetValue(key, out Pair_Dt_LsMsg))
                {
                    Old_Pair_Dt_LsMsg = Pair_Dt_LsMsg;
                    if (Pair_Dt_LsMsg != null)
                    {
                        if (Pair_Dt_LsMsg.Item2 != null)
                        {
                            if (Pair_Dt_LsMsg.Item2.Contains(message) == false)
                            {
                                Pair_Dt_LsMsg.Item2.Add(message);
                                if (dic_WarehouseMessages.TryUpdate(key, Pair_Dt_LsMsg, Old_Pair_Dt_LsMsg))
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
                    if (dic_WarehouseMessages.TryAdd(key, pair_dt_l_msg))
                    {
                        result = true;
                    }
                }

                if (dic_WarehouseMessages.TryGetValue(key, out Pair_Dt_LsMsg))
                {
                    if (parts == Pair_Dt_LsMsg.Item2.Count)
                    {
                        //Here you consolidate
                        Message messageResult = new Message();
                        foreach (Message item in Pair_Dt_LsMsg.Item2.OrderBy(c => c.IdMsg))
                        {
                            messageResult.text += item.text.Replace("u002B", "+");
                        }
                        //Here you return the consolidate, it will be null if it's not yet consolidated
                        messageConsolidate = messageResult;

                        //if consolidation was successfull, then, eliminate the consolidate object and the list from the warehouse
                        string tstString = string.Empty;
                        if (UtilityAssistant.TryBase64Decode(messageResult.text, out tstString))
                        {
                            Pares<DateTime, List<Message>> par = null;
                            TryRemoveMessageFromWarehouse_ThroughKey(key, out par);
                            UnRegister();
                        }
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
                if (dic_WarehouseMessages.TryGetValue(key, out Pair_Dt_LsMsg))
                {
                    Old_Pair_Dt_LsMsg = Pair_Dt_LsMsg;
                    if (Pair_Dt_LsMsg != null)
                    {
                        if (Pair_Dt_LsMsg.Item2 != null)
                        {
                            if (Pair_Dt_LsMsg.Item2.Contains(message) == false)
                            {
                                Pair_Dt_LsMsg.Item2.Add(message);
                                if (dic_WarehouseMessages.TryUpdate(key, Pair_Dt_LsMsg, Old_Pair_Dt_LsMsg))
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
                    if (dic_WarehouseMessages.TryAdd(key, pair_dt_l_msg))
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

        public static bool TryGetMessageFromWarehouse_ThroughKey(string key, out Pares<DateTime, List<Message>> par)
        {
            try
            {
                par = null;
                if (string.IsNullOrWhiteSpace(key))
                {
                    Console.WriteLine("Error static bool TryGetMessageFromWarehouse_ThroughKey(string, out pares<DateTime, List<message>>): Key Missing! ");
                    return false;
                }

                if (dic_WarehouseMessages.TryGetValue(key, out par))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error static bool TryGetMessageFromWarehouse_ThroughKey(string, out pares<DateTime, List<message>>): " + ex.Message);
                par = null;
                return false;
            }
        }

        public static bool TryGetSpecificMessageFromWarehouse(string IdMsg, string key, out Message message)
        {
            try
            {
                message = null;
                uint ui = 0;
                if (!uint.TryParse(IdMsg, out ui))
                {
                    Console.WriteLine("Error bool TryGetSpecificMessageFromWarehouse: Trying to parse " + IdMsg + " to uint, parsing failed");
                    return false;
                }

                foreach (KeyValuePair<string, Pares<DateTime, List<Message>>> item in dic_WarehouseMessages)
                {
                    if (item.Key == key)
                    {
                        foreach (Message msg in item.Value.Item2)
                        {
                            if (msg.IdMsg == ui)
                            {
                                message = msg;
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error static bool TryGetSpecificMessageFromWarehouse(string, string, out message): " + ex.Message);
                message = null;
                return false;
            }
        }

        public static bool TryRemoveMessageFromWarehouse_ThroughKey(string key, out Pares<DateTime, List<Message>> par)
        {
            try
            {
                par = null;
                if (string.IsNullOrWhiteSpace(key))
                {
                    Console.WriteLine("Error static bool TryRemoveMessageFromWarehouse_ThroughKey(out pares<DateTime, List<message>>): Key Missing! ");
                    return false;
                }

                if (dic_WarehouseMessages.TryRemove(key, out par))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error bool TryRemoveMessageFromWarehouse_ThroughKey(out Pares<DateTime, List<Message>>): " + ex.Message);
                par = null;
                return false;
            }
        }

        public static bool TryRemoveSpecificMessageFromWarehouse(Message msg)
        {
            try
            {
                foreach (Pares<DateTime, List<Message>> item in dic_WarehouseMessages.Values)
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

        public bool TryRemoveMessageFromWarehouse_ThroughKey(out Pares<DateTime, List<Message>> par)
        {
            try
            {
                par = null;
                if (string.IsNullOrWhiteSpace(Key))
                {
                    Console.WriteLine("Error static bool TryRemoveMessageFromWarehouse_ThroughKey(out pares<DateTime, List<message>>): Key Missing! ");
                    return false;
                }

                if (dic_WarehouseMessages.TryRemove(Key, out par))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error bool TryRemoveMessageFromWarehouse_ThroughKey(out Pares<DateTime, List<Message>>): " + ex.Message);
                par = null;
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
                if (!dic_ActiveConsolidateMessages.TryGetValue(key, out cnMsg))
                {
                    if (!Equals(cnMsg))
                    {
                        if (dic_ActiveConsolidateMessages.TryAdd(key, this))
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
                if (!dic_ActiveConsolidateMessages.TryRemove(key, out cnMsg))
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

                if (string.IsNullOrWhiteSpace(specificRelevantInstruction))
                {
                    return false;
                }

                //Some cleaning, if required, because clean for Json and cleaning for extranction ARE DIFFERENT
                //And can generate problems when in this method
                if (!specificRelevantInstruction.Contains("{"))
                {
                    specificRelevantInstruction = specificRelevantInstruction.Replace("\"}", "");
                }

                List<string> l_strings = new List<string>();
                if (specificRelevantInstruction.Contains("MS:"))
                {
                    //Entro aqui? entonces quiere decir que quedan mas MS: aparte del inicial, estos ya solo deberían ser limpiados
                    specificRelevantInstruction = specificRelevantInstruction.Replace("MS:", "");
                }
                if (specificRelevantInstruction.Contains("}{"))
                {
                    specificRelevantInstruction = specificRelevantInstruction.Replace("}{", "}|°|{");
                }
                string[] strArray = specificRelevantInstruction.Split("|°|", StringSplitOptions.RemoveEmptyEntries);
                l_strings.AddRange(strArray);
                l_strings = l_strings.Distinct().ToList();
                //END Special Cleaning

                foreach (string item in l_strings)
                {
                    Message nwMsg = Message.CreateFromJson(item);
                    if (nwMsg.TextOriginal.Contains("CM:"))
                    {
                        string tempString = UtilityAssistant.ExtractValues(nwMsg.TextOriginal, "CM");
                        if (!string.IsNullOrWhiteSpace(tempString))
                        {
                            ConsolidateMessage cnMsg = new ConsolidateMessage();
                            if (TryCreateFromJson(tempString, out cnMsg))
                            {
                                cnMsg.SelfRegister();
                                //ConsolidateMessage.dic_ActiveConsolidateMessages.TryAdd(cnMsg.key, cnMsg);
                                return true; //Porque es un mensaje de consolidación, no un mensaje que pudiera ser colocado
                                             //junto a otros para armar un mensaje masivo, por tanto, al resolver el tema de agregarlo a la 
                                             //lista para poder tenerlo vigilado y que empieze la espera por los mensaje que componen el mensaje
                                             //compuesto que él debe armar, se resuelve el asunto de si es o no un mensaje que debería ir o no en
                                             //una lista, pues no debería, ya que es un CM, el administra dicha lista pero no participa de ella
                            }
                        }
                    }
                    return CheckMessageIfMatch(nwMsg, out msgResult);
                }
                //Probablemente es un mensaje de otro tipo, así que false
                return false;
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

                if (string.IsNullOrWhiteSpace(specificRelevantInstruction))
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
                    if (TryCreateFromJson(nwMsg.TextOriginal, out cnMsg))
                    {
                        cnMsg.SelfRegister();
                        //ConsolidateMessage.dic_ActiveConsolidateMessages.TryAdd(cnMsg.key, cnMsg);
                        return true; //Porque es un mensaje de consolidación, no un mensaje que pudiera ser colocado
                                     //junto a otros para armar un mensaje masivo, por tanto, al resolver el tema de agregarlo a la 
                                     //lista para poder tenerlo vigilado y que empieze la espera por los mensaje que componen el mensaje
                                     //compuesto que él debe armar, se resuelve el asunto de si es o no un mensaje que debería ir o no en
                                     //una lista, pues no debería, ya que es un CM, el administra dicha lista pero no participa de ella
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

                if (dic_ActiveConsolidateMessages.Count > 0)
                {
                    foreach (ConsolidateMessage item in dic_ActiveConsolidateMessages.Values.ToList())
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
                else
                {
                    if (!string.IsNullOrWhiteSpace(message.TextOriginal))
                    {
                        ConsolidateMessage cnMsg = new ConsolidateMessage();
                        if (TryCreateFromJson(message.TextOriginal, out cnMsg))
                        {
                            cnMsg.SelfRegister();
                            //ConsolidateMessage.dic_ActiveConsolidateMessages.TryAdd(cnMsg.key, cnMsg);
                        }

                        foreach (ConsolidateMessage item in dic_ActiveConsolidateMessages.Values.ToList())
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

                if (dic_ActiveConsolidateMessages.Count > 0)
                {
                    foreach (ConsolidateMessage item in dic_ActiveConsolidateMessages.Values.ToList())
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
                        if (TryCreateFromJson(message.TextOriginal, out cnMsg))
                        {
                            cnMsg.SelfRegister();
                            //ConsolidateMessage.dic_ActiveConsolidateMessages.TryAdd(cnMsg.key, cnMsg);
                        }

                        foreach (ConsolidateMessage item in dic_ActiveConsolidateMessages.Values.ToList())
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
                Console.WriteLine("Error (MakeMessageConsolidated) ToJson: " + ex.Message);
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

                parts = cdMsg.parts;
                text = cdMsg.text;
                length = cdMsg.length;
                idRef = cdMsg.idRef;
                key = cdMsg.key;

                return cdMsg;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MakeMessageConsolidated) FromJson: " + ex.Message + " Text: " + txt);
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

                parts = cdMsg.parts;
                text = cdMsg.text;
                length = cdMsg.length;
                idRef = cdMsg.idRef;
                Key = cdMsg.Key;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MakeMessageConsolidated) TryFromJson: " + ex.Message + " Text: " + txt);
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
                Console.WriteLine("Error (MakeMessageConsolidated) CreateFromJson: " + ex.Message);
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
                Console.WriteLine("Error (MakeMessageConsolidated) TryCreateFromJson: " + ex.Message);
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