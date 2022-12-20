using Interfaz.Utilities;
using Newtonsoft.Json;
using System.Numerics;
using System.Text.Json;

namespace Interfaz.Models
{
    public struct ShotUpdate
    {
        public int Id { get; set; }
        public Vector3 Pos { get; set; }

        public ShotUpdate(int id, Vector3 pos)
        {
            this.Id = id;
            this.Pos = pos;
        }

        public string ToJson()
        {
            try
            {

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                    new ShotUpdateConverter()
                    },
                };
                //ReadCommentHandling = JsonCommentHandling.Skip,
                //    AllowTrailingCommas = true,

                return System.Text.Json.JsonSerializer.Serialize(this, serializeOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public Shot FromJson(string json)
        {
            try
            {
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new ShotUpdateConverter()
                    },
                };

                //AllowTrailingCommas = true,
                //ReadCommentHandling = JsonCommentHandling.Skip,
                Shot shot = System.Text.Json.JsonSerializer.Deserialize<Shot>(json, serializeOptions);
                //this = shot;

                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error FromJson(): " + ex.Message);
                return default(Shot);
            }
        }

        public static Shot CreateFromJson(string json)
        {
            Shot shot = new();
            return shot.FromJson(json);
        }
    }

    public class ShotUpdateConverter : System.Text.Json.Serialization.JsonConverter<ShotUpdate>
    {
        public override ShotUpdate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                //TODO: Corregir, testear y terminar
                string strJson = reader.GetString();

                ShotUpdate shot = new ShotUpdate();
                string[] a = UtilityAssistant.CutJson(strJson);

                shot.Id = Convert.ToInt32(a[0]);
                shot.Pos = UtilityAssistant.XmlToClass<SerializedVector3>(a[1]).ConvertToVector3();

                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ShotUpdateConverter) Read(): " + ex.Message);
                return default(ShotUpdate);
            }
        }

        public override void Write(Utf8JsonWriter writer, ShotUpdate shot, JsonSerializerOptions options)
        {
            try
            {
                //TODO: Corregir, testear y terminar
                string Id = "\"" + shot.Id + "\"";
                string Pos = new SerializedVector3(shot.Pos).ToXML();

                string resultJson = "{Id:" + Id + ", Pos:" + Pos + "}";
                writer.WriteStringValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ShotUpdateConvert) Write(): " + ex.Message);
            }
        }
    }

    public class ShotUpdateConverterJSON : Newtonsoft.Json.JsonConverter<ShotUpdate>
    {
        public override void WriteJson(JsonWriter writer, ShotUpdate shot, Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                //TODO: Corregir, testear y terminar
                string Id = "\"" + shot.Id + "\"";
                string Pos = new SerializedVector3(shot.Pos).ToXML();

                string resultJson = "{Id:" + Id + ", Pos:" + Pos + "}";
                writer.WriteValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ShotUpdateConverterJSON) Write(): " + ex.Message);
            }
        }

        public override ShotUpdate ReadJson(JsonReader reader, Type objectType, ShotUpdate existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                //TODO: Corregir, testear y terminar
                string strJson = (string)reader.Value;

                //Console.BackgroundColor = ConsoleColor.Green;
                //Console.WriteLine("strJson: "+ strJson);
                //Console.ResetColor();

                ShotUpdate shot = new ShotUpdate();
                string[] a = UtilityAssistant.CutJson(strJson);
                shot.Id = Convert.ToInt32(a[0]);
                shot.Pos = UtilityAssistant.XmlToClass<SerializedVector3>(a[1]).ConvertToVector3();

                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ShotUpdateConverterJSON) ReadJson(): " + ex.Message);
                return default(ShotUpdate);
            }
        }
    }
}
