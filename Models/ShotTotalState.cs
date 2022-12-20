using Newtonsoft.Json;

namespace Interfaz.Models
{
    public struct ShotTotalState
    {

        public List<Shot> l_shots = new List<Shot>();
        public List<ShotUpdate> l_shotsUpdates = new List<ShotUpdate>();

        public ShotTotalState()
        {
            l_shots = new List<Shot>();
            l_shotsUpdates = new List<ShotUpdate>();
        }

        public string ToJson()
        {
            try
            {
                JsonSerializerSettings serializeOptions = new JsonSerializerSettings
                {
                    Converters =
                    {
                        new ShotTotalStateConverterJSON(),
                    },
                };

                string json = JsonConvert.SerializeObject(this, serializeOptions);
                //ReadCommentHandling = JsonCommentHandling.Skip,
                //    AllowTrailingCommas = true,

                /*string json = "{";
                json += "[";
                for (int i = 0; i < l_shots.Count; i++)
                {
                    json += l_shots[i].ToJson();
                    if (i < (l_shots.Count - 1))
                    {
                        json += ",";
                    }
                }
                json += "],";
                json += "[";
                for (int i = 0; i < l_shotsUpdates.Count; i++)
                {
                    json += l_shotsUpdates[i].ToJson();
                    if (i < (l_shotsUpdates.Count - 1))
                    {
                        json += ", ";
                    }
                }
                json += "]";
                json += "}";*/

                return json; //JsonSerializer.Serialize(this, serializeOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public ShotTotalState FromJson(string json)
        {
            try
            {
                JsonSerializerSettings serializeOptions = new JsonSerializerSettings
                {
                    Converters =
                    {
                        new ShotTotalStateConverterJSON(),
                    },
                };

                //Console.BackgroundColor = ConsoleColor.Green;
                //Console.WriteLine("json: " + json);
                //Console.ResetColor();

                //AllowTrailingCommas = true,
                //ReadCommentHandling = JsonCommentHandling.Skip,
                ShotTotalState shot = JsonConvert.DeserializeObject<ShotTotalState>(json, serializeOptions);



                /*string strJson = json;
                string[] a = UtilityAssistant.CutJson(strJson);

                string b = a[0];
                string d = a[1];

                string c = b + d;*/

                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error FromJson(): " + ex.Message);
                return default(ShotTotalState);
            }
        }

        public static ShotTotalState CreateFromJson(string json)
        {
            try
            {
                ShotTotalState shot = new();
                return shot.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error CreateFromJson(): " + ex.Message);
                return default(ShotTotalState);
            }
        }
    }

    public class ShotTotalStateConverterJSON : Newtonsoft.Json.JsonConverter<ShotTotalState>
    {
        public override void WriteJson(JsonWriter writer, ShotTotalState sts, Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                JsonSerializerSettings jsonOptions = new JsonSerializerSettings
                {
                    Converters =
                    {
                        new ShotConverterJSON(),
                        new ShotUpdateConverterJSON()
                    },
                };

                string resultJson = JsonConvert.SerializeObject(sts, jsonOptions);
                writer.WriteValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ShotTotalStateConverterJSON) WriteJson(): " + ex.Message);
            }
        }

        public override ShotTotalState ReadJson(JsonReader reader, Type objectType, ShotTotalState existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                string strEntity = (string)reader.Value;
                JsonSerializerSettings jsonOptions = new JsonSerializerSettings
                {
                    Converters =
                    {
                        new ShotConverterJSON(),
                        new ShotUpdateConverterJSON()
                    },
                };

                ShotTotalState resultJson = JsonConvert.DeserializeObject<ShotTotalState>(strEntity, jsonOptions);
                return resultJson;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ShotTotalStateConverterJSON) ReadJson(): " + ex.Message);
                return default(ShotTotalState);
            }
        }
    }
}
