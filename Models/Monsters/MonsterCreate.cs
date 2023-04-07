using Interfaz.Utilities;

namespace Interfaz.Models.Monsters
{
    public class MonsterCreate
    {
        public string Id { get; set; }
        public string Type { get; set; }

        public MonsterCreate(string id = "", string typ = "")
        {
            Id = id;
            Type = typ;
        }

        public string ToJson()
        {
            try
            {
                return System.Text.Json.JsonSerializer.Serialize(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MonsterCreate) ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public MonsterCreate FromJson(string json)
        {
            try
            {
                string strJson = UtilityAssistant.CleanJSON(json);
                MonsterCreate shot = System.Text.Json.JsonSerializer.Deserialize<MonsterCreate>(strJson);
                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MonsterCreate) FromJson(): " + ex.Message);
                return new MonsterCreate();
            }
        }

        public static MonsterCreate CreateFromJson(string json)
        {
            try
            {
                MonsterCreate shot = new();
                return shot.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MonsterCreate) CreateFromJson(): " + ex.Message);
                return new MonsterCreate();
            }
        }
    }
}
