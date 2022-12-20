using System.Numerics;
using System.Text.Json;
using Interfaz.Utilities;
using Newtonsoft.Json;

namespace Interfaz.Models
{
    public struct Shot
    {
        public int Id { get; set; }
        public String LN { get; set; }
        public String Type { get; set; }
        public Vector3 OrPos { get; set; }
        public Vector3 WPos { get; set; }
        public Vector3 Mdf { get; set; }

        public string ToJson()
        {
            try
            {

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                    new ShotConverter()
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
                        new ShotConverter()
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

    public class ShotConverter : System.Text.Json.Serialization.JsonConverter<Shot>
    {
        public override Shot Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                //TODO: Corregir, testear y terminar
                string strJson = reader.GetString();

                Shot shot = new Shot();
                string[] a = UtilityAssistant.CutJson(strJson);

                shot.Id = Convert.ToInt32(a[0]);
                shot.LN = a[1];
                shot.Type = a[2];

                shot.OrPos= UtilityAssistant.XmlToClass<SerializedVector3>(a[3]).ConvertToVector3();
                shot.WPos= UtilityAssistant.XmlToClass<SerializedVector3>(a[4]).ConvertToVector3();
                shot.Mdf= UtilityAssistant.XmlToClass<SerializedVector3>(a[5]).ConvertToVector3();

                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ShotConverter) Read(): " + ex.Message);
                return default(Shot);
            }
        }

        public override void Write(Utf8JsonWriter writer, Shot shot, JsonSerializerOptions options)
        {
            try
            {
                //TODO: Corregir, testear y terminar
                string Id = "\"" + shot.Id + "\"";
                string LauncherName = "\"" + shot.LN + "\"";
                string Type = "\"" + shot.Type + "\"";
                string LauncherPos = new SerializedVector3(shot.OrPos).ToXML();
                string WeaponPos = new SerializedVector3(shot.WPos).ToXML();
                string Moddif = new SerializedVector3(shot.Mdf).ToXML();

                string resultJson = "{Id:" + Id + ", LN:" + LauncherName + ", Type:" + Type + ", OrPos:" + LauncherPos + ", WPos:" + WeaponPos + ", Mdf:" + Moddif + "}";
                writer.WriteStringValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ShotConverter) Write(): " + ex.Message);
            }
        }
    }

    public class ShotConverterJSON : Newtonsoft.Json.JsonConverter<Shot>
    {
        public override void WriteJson(JsonWriter writer, Shot shot, Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                //TODO: Corregir, testear y terminar
                string Id = "\"" + shot.Id + "\"";
                string LauncherName = "\"" + shot.LN + "\"";
                string Type = "\"" + shot.Type + "\"";
                string LauncherPos = new SerializedVector3(shot.OrPos).ToXML();
                string WeaponPos = new SerializedVector3(shot.WPos).ToXML();
                string Moddif = new SerializedVector3(shot.Mdf).ToXML();

                string resultJson = "{Id:" + Id + ", LN:" + LauncherName + ", Type:" + Type + ", OrPos:" + LauncherPos + ", WPos:" + WeaponPos + ", Mdf:" + Moddif + "}";
                writer.WriteValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ShotConverterJSON) WriteJson(): " + ex.Message);
            }
        }

        public override Shot ReadJson(JsonReader reader, Type objectType, Shot existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                //TODO: Corregir, testear y terminar
                string strJson = (string)reader.Value;

                Shot shot = new Shot();
                string[] a = UtilityAssistant.CutJson(strJson);

                shot.Id = Convert.ToInt32(a[0]);
                shot.LN = a[1];
                shot.Type = a[2];

                shot.OrPos = UtilityAssistant.XmlToClass<SerializedVector3>(a[3]).ConvertToVector3();
                shot.WPos = UtilityAssistant.XmlToClass<SerializedVector3>(a[4]).ConvertToVector3();
                shot.Mdf = UtilityAssistant.XmlToClass<SerializedVector3>(a[5]).ConvertToVector3();

                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ShotConverterJSON) ReadJson(): " + ex.Message);
                return default(Shot);
            }
        }
    }
}
