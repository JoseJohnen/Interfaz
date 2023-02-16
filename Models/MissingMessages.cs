using Interfaz.Utilities;
using System.Collections.Concurrent;

namespace Interfaz.Models
{
    public class MissingMessages
    {
        public List<string> l_missingMessages = new List<string>();

        public static ConcurrentQueue<MissingMessages> q_MissingMessages = new ConcurrentQueue<MissingMessages>();

        public MissingMessages() { }
        public MissingMessages(string[] message) { 
            l_missingMessages.AddRange(message);
        }

        public MissingMessages(List<string> message)
        {
            l_missingMessages.AddRange(message);
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
                return System.Text.Json.JsonSerializer.Deserialize<MissingMessages>(txt);
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
