using Interfaz.Utilities;
using System.Numerics;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Interfaz.Models.Monsters
{
    public class MonsterCreate
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public Vector3 Pos { get; set; }

        public MonsterCreate(string id = "", string typ = "")
        {
            Id = id;
            Type = typ;
        }

        public string ToJson()
        {
            try
            {
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new Vector3Converter()
                        ,new NullConverter()
                    },
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                    IgnoreNullValues = true
                };

                return System.Text.Json.JsonSerializer.Serialize(this, serializeOptions);
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

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new Vector3Converter()
                        ,new NullConverter()
                    },
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                    IgnoreNullValues = true
                };

                MonsterCreate shot = System.Text.Json.JsonSerializer.Deserialize<MonsterCreate>(strJson, serializeOptions);
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
