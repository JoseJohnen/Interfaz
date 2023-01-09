using Interfaz.Utilities;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Interfaz.Models
{
    public struct ShotTotalState
    {

        public List<Shot> l_shotsCreated = new List<Shot>();
        public List<ShotPosUpdate> l_shotsPosUpdates = new List<ShotPosUpdate>();
        public List<ShotState> l_shotsStates = new List<ShotState>();

        public ShotTotalState()
        {
            l_shotsCreated = new List<Shot>();
            l_shotsPosUpdates = new List<ShotPosUpdate>();
            l_shotsStates = new List<ShotState>();
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
                if (Regex.Matches(json, "l_shotsCreated").Count != 1 ||
                    Regex.Matches(json, "l_shotsPosUpdates").Count != 1 ||
                    Regex.Matches(json, "l_shotsStates").Count != 1 ||
                    (Regex.Matches(json, "{").Count != Regex.Matches(json, "}").Count) ||
                    (Regex.Matches(json, @"\[").Count != Regex.Matches(json, "]").Count) 
                    )
                {
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("¡Error prevented! (ShotTotalState) FromJson(): Defective Json: "+json);
                    Console.ResetColor();
                    return default(ShotTotalState);
                }

                JsonSerializerSettings serializeOptions = new JsonSerializerSettings
                {
                    Converters =
                    {
                        new ShotTotalStateConverterJSON(),
                    },
                };

                ShotTotalState shot = JsonConvert.DeserializeObject<ShotTotalState>(json, serializeOptions);

                return shot;
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red; 
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Error ShotTotalState FromJson(): json: "  + json + " Message: "+ ex.Message);
                Console.ResetColor();
                return default(ShotTotalState);
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
                    foreach (string jsonCut in jsonCuts.Where(f => (f[1] == '{') && (f[(f.Length - 2)] == '}')).ToList())
                    {
                        ShotTotalState shot = new();
                        shotTotalStates[i] = shot.FromJson(jsonCut);
                        i++;
                        Console.WriteLine("From FromJSON: " + jsonCut);
                        Console.WriteLine("FirstCharacter is {0}, because it is {1}", (jsonCut[1] == '{'), jsonCut[1]);
                        Console.WriteLine("LastCharacter is {0}, because it is {1}", (jsonCut[(jsonCut.Length - 2)] == '}'), jsonCut[(jsonCut.Length - 2)]);
                    }
                    Console.ResetColor();
                }
                else if(Regex.Matches(json, "SM:").Count == 0)
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
                Console.WriteLine("Error: (ShotTotalStateConverterJSON) ReadJson(): " + ex.Message + " strEntity: "+ strEntity);
                return default(ShotTotalState);
            }
        }
    }
}
