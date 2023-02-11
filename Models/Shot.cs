using System.Numerics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
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
                        //new Vector3Converter()
                        new ShotConverter()
                    },
                    //Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                };
                //ReadCommentHandling = JsonCommentHandling.Skip,
                //    AllowTrailingCommas = true,

                string serialized = System.Text.Json.JsonSerializer.Serialize(this, serializeOptions);
                return serialized;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Shot) ToJson(): " + ex.Message);
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
                        new Vector3Converter()
                        ,new ShotConverter()
                    },
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                };

                //AllowTrailingCommas = true,
                //ReadCommentHandling = JsonCommentHandling.Skip,
                Shot shot = System.Text.Json.JsonSerializer.Deserialize<Shot>(json, serializeOptions);
                //this = shot;

                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Shot) FromJson(): " + ex.Message);
                return default(Shot);
            }
        }

        public static Shot CreateFromJson(string json)
        {
            try
            {
                Shot shot = new();
                return shot.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Shot) FromJson(): " + ex.Message);
                return default(Shot);
            }
        }
    }

    public class ShotConverter : System.Text.Json.Serialization.JsonConverter<Shot>
    {
        public override Shot Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string strJson = string.Empty;
            try
            {
                //TODO: Corregir, testear y terminar
                JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
                strJson = jsonDoc.RootElement.GetRawText();
                //strJson = reader.GetString();

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new Vector3Converter()
                    }
                };

                Shot shot = new Shot();
                strJson = strJson.Replace("\"", "").Replace(":<", ":\"<").Replace(">}", ">\"}");
                string[] a = strJson.Replace("{","").Replace("}","").Split(",");//UtilityAssistant.CutJson(strJson);

                if (a[0] != null)
                {
                    shot.Id = Convert.ToInt32(a[0].Substring(a[0].IndexOf(":")+1));
                }
                else
                {
                    Console.WriteLine("a[0] es null, strJason es: " + strJson);
                    shot.Id = 0;
                }

                if (a[1] != null)
                {
                    shot.LN = a[1].Substring(a[1].IndexOf(":") + 1);
                }
                else
                {
                    Console.WriteLine("a[1] es null, strJason es: " + strJson);
                    shot.LN = string.Empty;
                }

                if (a[2] != null)
                {
                    shot.Type = a[2].Substring(a[2].IndexOf(":") + 1);
                }
                else
                {
                    Console.WriteLine("a[2] es null, strJason es: " + strJson);
                    shot.Type = string.Empty;
                }

                if (a[3] != null)
                {
                    string fd = a[3].Substring(a[3].IndexOf(":")+1);
                    shot.OrPos = Vector3Converter.Converter(fd);
                    //shot.OrPos = System.Text.Json.JsonSerializer.Deserialize<Vector3>(fd, serializeOptions); //SerializedVector3.FromJson(a[3].Substring(a[3].IndexOf(":") + 1)).ConvertToVector3();
                }
                else
                {
                    Console.WriteLine("a[3] es null, strJason es: " + strJson);
                    shot.OrPos = Vector3.Zero;
                }

                if (a[4] != null)
                {
                    string fd = a[4].Substring(a[4].IndexOf(":") + 1);
                    shot.WPos = Vector3Converter.Converter(fd);//System.Text.Json.JsonSerializer.Deserialize<Vector3>(fd, serializeOptions);
                }
                else
                {
                    Console.WriteLine("a[4] es null, strJason es: " + strJson);
                    shot.WPos = Vector3.Zero;
                }

                if (a[5] != null)
                {
                    string fd = a[5].Substring(a[5].IndexOf(":") + 1);
                    shot.Mdf = Vector3Converter.Converter(fd);//System.Text.Json.JsonSerializer.Deserialize<Vector3>(fd, serializeOptions);
                }
                else
                {
                    Console.WriteLine("a[5] es null, strJason es: " + strJson);
                    shot.Mdf = Vector3.Zero;
                }

                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ShotConverter) Read(): {0} Message: {1}", strJson, ex.Message);
                return default(Shot);
            }
        }

        public override void Write(Utf8JsonWriter writer, Shot shot, JsonSerializerOptions options)
        {
            try
            {
                //TODO: Corregir, testear y terminar
                string Id = shot.Id+""; //"\"" + shot.Id + "\"";
                string LauncherName = string.IsNullOrWhiteSpace(shot.LN)? "null" : shot.LN ; //"\"" + shot.LN + "\"";
                string Type = string.IsNullOrWhiteSpace(shot.Type) ? "null" : shot.Type ; //"\"" + shot.Type + "\"";
                //string LauncherPos = new SerializedVector3(shot.OrPos).ToXML();
                //string WeaponPos = new SerializedVector3(shot.WPos).ToXML();
                //string Moddif = new SerializedVector3(shot.Mdf).ToXML();

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

                string LauncherPos = System.Text.Json.JsonSerializer.Serialize(shot.OrPos, serializeOptions); //new SerializedVector3(shot.OrPos).ToJson();
                string WeaponPos = System.Text.Json.JsonSerializer.Serialize(shot.WPos, serializeOptions);  //new SerializedVector3(shot.WPos).ToJson();
                string Moddif = System.Text.Json.JsonSerializer.Serialize(shot.Mdf, serializeOptions);  //new SerializedVector3(shot.Mdf).ToJson();

                char[] a = { '"' };
                //string wr =


                string wr = @String.Concat("{ ", new string(a), "Id", new string(a), ":", Id, 
                    ", ",new string(a), "LN", new string(a), ":" , new string(a), LauncherName, new string(a),
                    ", " , new string(a), "Type", new string(a), ":", new string(a), Type, new string(a),
                    ", ", new string(a), "OrPos", new string(a), ":", LauncherPos,
                    ", ", new string(a), "WPos", new string(a), ":", WeaponPos,
                    ", ", new string(a), "Mdf", new string(a), ":", Moddif,
                    "}");

                string resultJson = Regex.Replace(wr, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");
                //string resultJson = "{Id:" + Id + ", LN:" + LauncherName + ", Type:" + Type + ", OrPos:" + LauncherPos + ", WPos:" + WeaponPos + ", Mdf:" + Moddif + "}";
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
                //string LauncherPos = new SerializedVector3(shot.OrPos).ToXML();
                //string WeaponPos = new SerializedVector3(shot.WPos).ToXML();
                //string Moddif = new SerializedVector3(shot.Mdf).ToXML();

                string LauncherPos = new SerializedVector3(shot.OrPos).ToJson(SerializedVector3.TextOrNewtonsoft.Newtonsoft);
                string WeaponPos = new SerializedVector3(shot.WPos).ToJson(SerializedVector3.TextOrNewtonsoft.Newtonsoft);
                string Moddif = new SerializedVector3(shot.Mdf).ToJson(SerializedVector3.TextOrNewtonsoft.Newtonsoft);

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
            string strJson = string.Empty;
            try
            {
                strJson = (string)reader.Value;
                //TODO: Corregir, testear y terminar

                Shot shot = new Shot();
                string[] a = UtilityAssistant.CutJson(strJson);

                shot.Id = Convert.ToInt32(a[0]);
                shot.LN = a[1];
                shot.Type = a[2];

                //shot.OrPos = UtilityAssistant.XmlToClass<SerializedVector3>(a[3]).ConvertToVector3();
                //shot.WPos = UtilityAssistant.XmlToClass<SerializedVector3>(a[4]).ConvertToVector3();
                //shot.Mdf = UtilityAssistant.XmlToClass<SerializedVector3>(a[5]).ConvertToVector3();

                shot.OrPos = JsonConvert.DeserializeObject<SerializedVector3>(a[3]).ConvertToVector3();
                shot.WPos = JsonConvert.DeserializeObject<SerializedVector3>(a[4]).ConvertToVector3();
                shot.Mdf = JsonConvert.DeserializeObject<SerializedVector3>(a[5]).ConvertToVector3();

                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ShotConverterJSON) ReadJson(): {0} Message: {1}", strJson, ex.Message);
                return default(Shot);
            }
        }
    }
}
