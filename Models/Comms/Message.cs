using Interfaz.Models.Auxiliary;
using Interfaz.Utilities;
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Interfaz.Models.Comms
{
    public enum StatusMessage { Error = -2, NonRelevantUsage = -1, ReadyToSend = 0, Delivered = 1, Executed = 2 }

    public class Message
    {
        public static ConcurrentDictionary<string, Pares<DateTime, Message>> dic_ActiveMessages = new ConcurrentDictionary<string, Pares<DateTime, Message>>();

        public static ConcurrentDictionary<string, Pares<DateTime, Message>> dic_BackUpMessages = new ConcurrentDictionary<string, Pares<DateTime, Message>>();

        [JsonInclude]
        public string text = string.Empty;
        private uint idMsg = 0;
        private uint length = 0;
        private uint idRef = 0;

        private bool IsBlockMultiMessage = false;
        public StatusMessage Status = StatusMessage.NonRelevantUsage;


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
                string tempText = UtilityAssistant.Base64Encode(value);
                int remanent = 150;
                if (tempText.Length > remanent && !IsBlockMultiMessage)
                {
                    ConsolidateMessage csMsg = ConsolidateMessage.CreateConsolidateMessage(tempText);
                    if (csMsg != null)
                    {
                        text = UtilityAssistant.Base64Encode("CM:" + csMsg.ToJson()); //No se pone TAG porque funciona antes del sistema de TAG
                    }
                    else
                    {
                        text = "ERROR_:_CONSOLIDATE_MESSAGE_CREATE_ON_MESSAGE_TEXT";
                    }
                }
                else
                {
                    text = tempText;
                }
                Length = (uint)text.Length;
                /*Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n\nSize of the message is: " + text.Length + " total");
                Console.ResetColor();*/
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
        public uint Length { get => length; private set => length = value; }
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
        public uint IdMsg { get => idMsg; set => idMsg = value; }

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
                msg.Text = jsonAProc;
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
                    msg.IdRef = idForRef;
                }
                if (idMessage != 0)
                {
                    msg.IdMsg = idMessage;
                }
                return msg;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Message CreatePartMessage: " + ex.Message);
                return new Message();
            }
        }

        public string ToJson()
        {
            try
            {
                string result = JsonSerializer.Serialize(this);
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
                txt = UtilityAssistant.CleanJSON(txt.Replace("\u002B", "+"));
                Message nwMsg = JsonSerializer.Deserialize<Message>(txt);
                if (nwMsg != null)
                {
                    text = nwMsg.text;
                    length = nwMsg.length;
                    IdRef = nwMsg.IdRef;
                    IdMsg = nwMsg.IdMsg;
                }
                return nwMsg;
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

        public static bool ValidTextFromJsonMsg(string jsonText)
        {
            try
            {
                if (jsonText.Contains("text"))
                {
                    string base64text = jsonText.Substring(jsonText.IndexOf("text"));
                    base64text = base64text.Substring(base64text.IndexOf(":") + 1);
                    base64text = base64text.Substring(0, base64text.LastIndexOf("\"")).Replace("\"", "");
                    string result = string.Empty;
                    return UtilityAssistant.TryBase64Decode(base64text, out result);
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error static bool ValidTextFromJsonMsg: " + ex.Message);
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