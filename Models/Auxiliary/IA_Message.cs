using System.Text.Json;

namespace Interfaz.Models.Auxiliary
{
    public enum Type_AI_Message { VAL = 0, OVR = 1, TOP_BOT = 2, END = 3 }

    public class IA_Message
    {
        public Type_AI_Message type_AI_Message { get; set; } = 0;
        public float Value1 { get; set; }
        public float Value2 { get; set; }

        public string ToJson()
        {
            try
            {
                string serialized = JsonSerializer.Serialize(this);
                return serialized;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (IA_Message) ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public IA_Message FromJson(string json)
        {
            try
            {
                IA_Message iaMessage = JsonSerializer.Deserialize<IA_Message>(json);
                return iaMessage;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (IA_Message) FromJson(): " + ex.Message);
                return default;
            }
        }

        public static IA_Message CreateFromJson(string json)
        {
            try
            {
                IA_Message iaMessage = new();
                return iaMessage.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (IA_Message) FromJson(): " + ex.Message);
                return default;
            }
        }
    }
}
