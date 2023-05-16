using Interfaz.Utilities;
//using Newtonsoft.Json;
using System.Numerics;
using System.Text.Json;

namespace Interfaz.Models.Shots
{
    public struct ShotPosUpdate
    {
        public string Id { get; set; }
        public Vector3 Pos { get; set; }

        public ShotPosUpdate(string id, Vector3 pos)
        {
            Id = id;
            Pos = pos;
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
                Console.WriteLine("Error (ShotPosUpdate) ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public ShotPosUpdate FromJson(string json)
        {
            try
            {
                string cleanTemp = json.Replace("\"{", "{").Replace("}\"","}");
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new ShotUpdateConverter()
                    },
                };

                //AllowTrailingCommas = true,
                //ReadCommentHandling = JsonCommentHandling.Skip,
                ShotPosUpdate shot = System.Text.Json.JsonSerializer.Deserialize<ShotPosUpdate>(cleanTemp, serializeOptions);
                //this = shot;

                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (ShotPosUpdate) FromJson(): " + ex.Message);
                return default;
            }
        }

        public static ShotPosUpdate CreateFromJson(string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    new ShotPosUpdate();
                }

                ShotPosUpdate shot = new();
                return shot.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (ShotPosUpdate) CreateFromJson(): " + ex.Message);
                return default;
            }
        }
    }

    public class ShotUpdateConverter : System.Text.Json.Serialization.JsonConverter<ShotPosUpdate>
    {
        public override ShotPosUpdate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string strJson = string.Empty;
            try
            {
                //TODO: Corregir, testear y terminar
                JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
                strJson = jsonDoc.RootElement.GetRawText();

                if (reader.Read())
                {
                    strJson = reader.GetString();
                }
                //strJson = reader.GetString();

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new Vector3Converter()
                    }
                };

                ShotPosUpdate shot = new ShotPosUpdate();
                string[] a = strJson.Split(",", StringSplitOptions.RemoveEmptyEntries);
                //string[] a = UtilityAssistant.CutJson(strJson);
                a[0] = a[0].Substring(a[0].IndexOf(":") + 1).Replace("\"", "");
                a[1] = "\"" + a[1].Substring(a[1].IndexOf(":") + 1).Replace("\"", "").Replace("}", "\""); //it sets the " in the replacement to 
                shot.Id = a[0];
                //shot.Pos = UtilityAssistant.XmlToClass<SerializedVector3>(a[1]).ConvertToVector3();
                string tmpString = "{ \"a\":" + a[1] + "}";
                shot.Pos = System.Text.Json.JsonSerializer.Deserialize<Vector3>(tmpString, serializeOptions); // System.Text.Json.JsonSerializer.Deserialize<SerializedVector3>(a[1]).ConvertToVector3();

                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\nError: (MonsterPosUpodateConverter) Read(): {0} Message: {1}", strJson, ex.Message);
                return default;
            }
        }

        public override void Write(Utf8JsonWriter writer, ShotPosUpdate shot, JsonSerializerOptions options)
        {
            try
            {

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new Vector3Converter()
                    }
                };

                //TODO: Corregir, testear y terminar
                string Id = "\"" + shot.Id + "\"";
                //string Pos = new SerializedVector3(shot.Pos).ToXML();
                string Pos = System.Text.Json.JsonSerializer.Serialize(shot.Pos, serializeOptions); //new SerializedVector3(shot.Pos).ToJson();

                string resultJson = "{\"Id\":" + Id + ", \"Pos\":" + Pos + "}";
                writer.WriteStringValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ShotUpdateConvert) Write(): " + ex.Message);
            }
        }
    }

    /*public class ShotUpdateConverterJSON : JsonConverter<ShotPosUpdate>
    {
        public override void WriteJson(JsonWriter writer, ShotPosUpdate shot, Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                //TODO: Corregir, testear y terminar
                string Id = "\"" + shot.Id + "\"";
                //string Pos = new SerializedVector3(shot.Pos).ToXML();
                string Pos = new SerializedVector3(shot.Pos).ToJson(SerializedVector3.TextOrNewtonsoft.Newtonsoft);

                string resultJson = "{Id:" + Id + ", Pos:" + Pos + "}";
                writer.WriteValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (MonsterPosUpdateConverterJSON) Write(): " + ex.Message);
            }
        }

        public override ShotPosUpdate ReadJson(JsonReader reader, Type objectType, ShotPosUpdate existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            string strJson = string.Empty;
            try
            {
                //TODO: Corregir, testear y terminar
                strJson = (string)reader.Value;

                //Console.BackgroundColor = ConsoleColor.Green;
                //Console.WriteLine("strJson: "+ strJson);
                //Console.ResetColor();

                ShotPosUpdate shot = new ShotPosUpdate();
                string[] a = UtilityAssistant.CutJson(strJson);
                shot.Id = a[0];
                //shot.Pos = UtilityAssistant.XmlToClass<SerializedVector3>(a[1]).ConvertToVector3();
                shot.Pos = JsonConvert.DeserializeObject<SerializedVector3>(a[1]).ConvertToVector3();

                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (MonsterPosUpdateConverterJSON) ReadJson(): {0} Message: {1}", strJson, ex.Message);
                return default;
            }
        }
    }*/
}
