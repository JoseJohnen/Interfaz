using Interfaz.Auxiliary;

namespace Interfaz.Models.Monsters
{
    public enum StateOfTheMonster { JustCreated = -1, None = 0, Destroyed = 1, Attacking = 2 }
    public class MonsterState
    {
        public string Id { get; set; }
        public StateOfTheMonster State { get; set; }

        public MonsterState(string id = "", StateOfTheMonster state = StateOfTheMonster.JustCreated)
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

        public MonsterState FromJson(string json)
        {
            try
            {
                string strJson = UtilityAssistant.CleanJSON(json);
                MonsterState shot = System.Text.Json.JsonSerializer.Deserialize<MonsterState>(strJson);
                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MonsterState) FromJson(): " + ex.Message);
                return new MonsterState();
            }
        }

        public static MonsterState CreateFromJson(string json)
        {
            try
            {
                MonsterState shot = new();
                return shot.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MonsterState) CreateFromJson(): " + ex.Message);
                return new MonsterState();
            }
        }
    }
}
