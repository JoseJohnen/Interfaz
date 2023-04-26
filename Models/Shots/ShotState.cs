using Interfaz.Utilities;

namespace Interfaz.Models.Shots
{
    public enum StateOfTheShot { JustCreated = -1, None = 0, Destroyed = 1 }
    public class ShotState
    {
        public string Id { get; set; }
        public StateOfTheShot State { get; set; }

        public ShotState(string id = "", StateOfTheShot state = StateOfTheShot.JustCreated)
        {
            Id = id;
            State = state;
        }

        public string ToJson()
        {
            try
            {
                return System.Text.Json.JsonSerializer.Serialize(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MonsterState) ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public ShotState FromJson(string json)
        {
            try
            {
                string strJson = UtilityAssistant.CleanJSON(json);
                ShotState shot = System.Text.Json.JsonSerializer.Deserialize<ShotState>(strJson);
                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MonsterState) FromJson(): " + ex.Message);
                return new ShotState();
            }
        }

        public static ShotState CreateFromJson(string json)
        {
            try
            {
                ShotState shot = new();
                return shot.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MonsterState) CreateFromJson(): " + ex.Message);
                return new ShotState();
            }
        }
    }
}
