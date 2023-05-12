using System.Text.Json;
//using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Interfaz.Models.Shots
{
    public struct ShotTotalState
    {
        public List<ShotPosUpdate> l_shotsPosUpdates = new List<ShotPosUpdate>();

        public ShotTotalState()
        {
            l_shotsPosUpdates = new List<ShotPosUpdate>();
        }

        public string ToJson()
        {
            try
            {
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new ShotTotalStateConverter(),
                    },
                };

                string json = JsonSerializer.Serialize(this, serializeOptions);
                //ReadCommentHandling = JsonCommentHandling.Skip,
                //    AllowTrailingCommas = true,

                /*string json = "{";
                json += "[";
                for (int i = 0; i < l_shotsCreated.Count; i++)
                {
                    json += l_shotsCreated[i].ToJson();
                    if (i < (l_shotsCreated.Count - 1))
                    {
                        json += ",";
                    }
                }
                json += "],";
                json += "[";
                for (int i = 0; i < l_shotsPosUpdates.Count; i++)
                {
                    json += l_shotsPosUpdates[i].ToJson();
                    if (i < (l_shotsPosUpdates.Count - 1))
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
                /*if (Regex.Matches(json, "l_shotsPosUpdates").Count != 1 ||
                    (Regex.Matches(json, "{").Count != Regex.Matches(json, "}").Count) ||
                    (Regex.Matches(json, @"\[").Count != Regex.Matches(json, "]").Count) 
                    )
                {
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("¡Error prevented! (ShotTotalState) FromJson(): Defective Json: "+json);
                    Console.ResetColor();
                    return default(ShotTotalState);
                }*/

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new ShotTotalStateConverter(),
                    },
                };

                ShotTotalState shot = JsonSerializer.Deserialize<ShotTotalState>(json, serializeOptions);

                return shot;
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Error ShotTotalState FromJson(): json: " + json + " Message: " + ex.Message);
                Console.ResetColor();
                return default;
            }
        }

        public static ShotTotalState[] CreateFromJson(string json)
        {
            try
            {
                ShotTotalState[] shotTotalStates = new ShotTotalState[1];
                if (Regex.Matches(json, "SM:").Count >= 1)
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    string[] jsonCuts = json.Split("SM:", StringSplitOptions.RemoveEmptyEntries);
                    shotTotalStates = new ShotTotalState[jsonCuts.Length];
                    int i = 0;
                    foreach (string jsonCut in jsonCuts.Where(f => f[1] == '{' && f[f.Length - 2] == '}').ToList())
                    {
                        ShotTotalState shot = new();
                        shotTotalStates[i] = shot.FromJson(jsonCut);
                        i++;
                        Console.WriteLine("From FromJSON: " + jsonCut);
                        Console.WriteLine("FirstCharacter is {0}, because it is {1}", jsonCut[1] == '{', jsonCut[1]);
                        Console.WriteLine("LastCharacter is {0}, because it is {1}", jsonCut[jsonCut.Length - 2] == '}', jsonCut[jsonCut.Length - 2]);
                    }
                    Console.ResetColor();
                }
                else if (Regex.Matches(json, "SM:").Count == 0)
                {
                    ShotTotalState shot = new();
                    shotTotalStates[0] = shot.FromJson(json);
                }

                return shotTotalStates;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error CreateFromJson(): " + ex.Message);
                return new ShotTotalState[0];
            }
        }
    }

    public class ShotTotalStateConverter : System.Text.Json.Serialization.JsonConverter<ShotTotalState>
    {
        public override ShotTotalState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string strEntity = string.Empty;
            try
            {
                //TODO: Corregir, testear y terminar
                JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
                strEntity = jsonDoc.RootElement.GetRawText();
                //strEntity = reader.GetString();

                JsonSerializerOptions jsonOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new ShotConverter(),
                        new ShotUpdateConverter()
                    },
                };

                ShotTotalState resultJson = JsonSerializer.Deserialize<ShotTotalState>(strEntity, jsonOptions);
                return resultJson;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ShotTotalStateConverter) ReadJson(): " + ex.Message + " strEntity: " + strEntity);
                return default;
            }
        }

        public override void Write(Utf8JsonWriter writer, ShotTotalState value, JsonSerializerOptions options)
        {
            try
            {
                JsonSerializerOptions jsonOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new ShotConverter(),
                        new ShotUpdateConverter()
                    },
                };

                string resultJson = JsonSerializer.Serialize(value, jsonOptions);
                writer.WriteStringValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ShotTotalStateConverter) WriteJson(): " + ex.Message);
            }
        }
    }

    /*public class ShotTotalStateConverterJSON : JsonConverter<ShotTotalState>
    {
        public override void WriteJson(JsonWriter writer, ShotTotalState sts, JsonSerializer serializer)
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

        public override ShotTotalState ReadJson(JsonReader reader, Type objectType, ShotTotalState existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string strEntity = string.Empty;
            try
            {
                strEntity = (string)reader.Value;
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
                Console.WriteLine("Error: (ShotTotalStateConverterJSON) ReadJson(): " + ex.Message + " strEntity: " + strEntity);
                return default;
            }
        }
    }*/
}
