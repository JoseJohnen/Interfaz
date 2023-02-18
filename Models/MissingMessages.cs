using Interfaz.Utilities;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace Interfaz.Models
{
    public class MissingMessages
    {
        public List<string> l_missingMessages = new List<string>();

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string extraValue = null;

        public static ConcurrentQueue<MissingMessages> q_MissingMessages = new ConcurrentQueue<MissingMessages>();

        public MissingMessages() { }

        public MissingMessages(string[] message, string extra = null)
        {
            l_missingMessages.AddRange(message);
            if(!string.IsNullOrWhiteSpace(extra))
            {
                extraValue = extra;
            }
        }

        public MissingMessages(List<string> message, string extra = null)
        {
            l_missingMessages.AddRange(message);
            if (!string.IsNullOrWhiteSpace(extra))
            {
                extraValue = extra;
            }
        }

        public string ToJson()
        {
            try
            {
                string result = "{ \"l_missingMessages\": [";

                foreach (string item in l_missingMessages)
                {
                    result += item + ", ";
                }
                result += "]";
                result = result.Replace(",]", "]").Replace(", ]", "]");

                if (!string.IsNullOrWhiteSpace(extraValue))
                {
                    result += " \"extraValue\":" + this.extraValue;
                }

                result += "}";
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MissingMessages) ToJson: " + ex.Message);
                return string.Empty;
            }
        }

        public MissingMessages FromJson(string Text)
        {
            string txt = Text;
            try
            {
                txt = UtilityAssistant.CleanJSON(txt);
                string strProcess = txt.Substring(txt.IndexOf("[") + 1);
                strProcess = strProcess.Substring(0, strProcess.IndexOf("]"));
                string[] arrStrings = strProcess.Split(new char[] { ',' });

                foreach (string item in arrStrings)
                {
                    this.l_missingMessages.Add(item);
                }

                string strProcess2 = txt.Substring(txt.IndexOf("]") + 1);
                if (strProcess2.Contains("extraValue"))
                {
                    strProcess2 = strProcess2.Substring(strProcess2.IndexOf(":") + 1);
                    strProcess2 = strProcess2.Replace("}", "");
                    if (!string.IsNullOrWhiteSpace(strProcess2))
                    {
                        this.extraValue = strProcess2;
                    }
                }

                MissingMessages msgMsg = new MissingMessages(arrStrings, strProcess2);
                return msgMsg;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MissingMessages) FromJson: " + ex.Message + " Text: " + txt);
                return new MissingMessages();
            }
        }

        public static MissingMessages CreateFromJson(string json)
        {
            try
            {
                MissingMessages msg = new MissingMessages();
                return msg.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MissingMessages) CreateFromJson: " + ex.Message);
                return new MissingMessages();
            }
        }

    }
}
