using System.Text.Json;

namespace Interfaz.Models
{
    public class StateMessage
    {
        public uint idRef = 0;
        public StatusMessage Status = StatusMessage.NonRelevantUsage;
        public StateMessage()
        {
        }

        public StateMessage(uint id, StatusMessage status)
        {
            idRef = id;
            Status = status;
        }

        public string ToJson()
        {
            try
            {
                return JsonSerializer.Serialize(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (ShotState) ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public StateMessage FromJson(string Text)
        {
            try
            {
                return JsonSerializer.Deserialize<StateMessage>(Text);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (ShotState) ToJson(): " + ex.Message);
                return new StateMessage();
            }
        }

        public static StateMessage CreateFromJson(string json)
        {
            try
            {
                StateMessage msg = new StateMessage();
                return msg.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (ShotState) ToJson(): " + ex.Message);
                return new StateMessage();
            }
        }
    }
}
