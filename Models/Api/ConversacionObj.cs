using Interfaz.Models.Shots;
using Interfaz.Auxiliary;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Text.Json;

namespace Interfaz.Models.Api
{
    public class ConversacionObj
    {
        public List<string> l_bullets_to_create = new List<string>();
        public List<string> l_bullets_to_update = new List<string>();
        public List<string> l_bullets_to_change_state = new List<string>();

        [JsonIgnore]
        public List<Shot> L_Bullets_to_create = new List<Shot>();
        [JsonIgnore]
        public List<ShotPosUpdate> L_Bullets_to_update = new List<ShotPosUpdate>();
        [JsonIgnore]
        public List<ShotState> L_Bullets_to_change_state = new List<ShotState>();

        #region Métodos JSON
        public string ToJson()
        {
            try
            {

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new ConversacionObjConverter()
                    },
                };
                //ReadCommentHandling = JsonCommentHandling.Skip,
                //    AllowTrailingCommas = true,

                return System.Text.Json.JsonSerializer.Serialize(this, serializeOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (ConversacionObj) ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public ConversacionObj FromJson(string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new ConversacionObj();
                }

                json = UtilityAssistant.CleanJSON(json);

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new ConversacionObjConverter()
                    },
                };

                //AllowTrailingCommas = true,
                //ReadCommentHandling = JsonCommentHandling.Skip,
                json = UtilityAssistant.CleanJSON(json);
                ConversacionObj conObj = System.Text.Json.JsonSerializer.Deserialize<ConversacionObj>(json, serializeOptions);//, serializeOptions);
                //this = prgObj;

                if (conObj == null)
                {
                    return new ConversacionObj();
                }

                return conObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\nError (ConversacionObj) FromJson(): " + json + " ex.Message: " + ex.Message);
                return new ConversacionObj();
            }
        }

        public static ConversacionObj CreateFromJson(string json)
        {
            try
            {
                ConversacionObj prgObj = new();
                return prgObj.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\nError (ConversacionObj) CreateFromJson(): " + ex.Message);
                return new ConversacionObj();
            }
        }
        #endregion

        #region Load Lists
        public ConversacionObj Load()
        {
            try
            {
                if (L_Bullets_to_create == null)
                {
                    L_Bullets_to_create = new List<Shot>();
                }

                if (L_Bullets_to_update == null)
                {
                    L_Bullets_to_update = new List<ShotPosUpdate>();
                }

                if (L_Bullets_to_change_state == null)
                {
                    L_Bullets_to_change_state = new List<ShotState>();
                }

                LoadShots();
                LoadShotPosUpdates();
                LoadShotStates();
                return this;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ConversacionObj) Load(): " + ex.Message);
                return new ConversacionObj();
            }
        }

        public List<Shot> LoadShots()
        {
            try
            {
                if (L_Bullets_to_create == null)
                {
                    L_Bullets_to_create = new List<Shot>();
                }

                string tmpItem = string.Empty;
                foreach (string item in l_bullets_to_create)
                {
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        continue;
                    }
                    tmpItem = UtilityAssistant.CleanJSON(item);
                    L_Bullets_to_create.Add(Shot.CreateFromJson(tmpItem));
                }
                return L_Bullets_to_create;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ConversacionObj) LoadShots(): " + ex.Message);
                return new List<Shot>();
            }
        }

        public List<ShotPosUpdate> LoadShotPosUpdates()
        {
            try
            {
                if (L_Bullets_to_update == null)
                {
                    L_Bullets_to_update = new List<ShotPosUpdate>();
                }

                string tmpItem = string.Empty;
                foreach (string item in l_bullets_to_update)
                {
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        continue;
                    }
                    tmpItem = UtilityAssistant.CleanJSON(item);
                    L_Bullets_to_update.Add(ShotPosUpdate.CreateFromJson(tmpItem));
                }
                return L_Bullets_to_update;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ConversacionObj) LoadShotPosUpdates(): " + ex.Message);
                return new List<ShotPosUpdate>();
            }
        }

        public List<ShotState> LoadShotStates()
        {
            try
            {
                if (L_Bullets_to_change_state == null)
                {
                    L_Bullets_to_change_state = new List<ShotState>();
                }

                string tmpItem = string.Empty;
                foreach (string item in l_bullets_to_change_state)
                {
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        continue;
                    }
                    tmpItem = UtilityAssistant.CleanJSON(item);
                    L_Bullets_to_change_state.Add(ShotState.CreateFromJson(tmpItem));
                }
                return L_Bullets_to_change_state;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ConversacionObj) LoadShotStates(): " + ex.Message);
                return new List<ShotState>();
            }
        }
        #endregion
    }

    public class ConversacionObjConverter : System.Text.Json.Serialization.JsonConverter<ConversacionObj>
    {
        public override ConversacionObj Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string[] strJsonArray = new string[1];
            string[] strStrArr = new string[1];
            string[] strStrArr2 = new string[1];
            string[] strStrArr3 = new string[1];
            string readerReceiver = string.Empty;
            try
            {
                //TODO: Corregir, testear y terminar
                //readerReceiver = reader.GetString();
                JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
                string tempString = jsonDoc.RootElement.GetRawText();

                readerReceiver = UtilityAssistant.CleanJSON(tempString);
                if (string.IsNullOrWhiteSpace(readerReceiver) || readerReceiver.Equals("\"{\""))
                {
                    return new ConversacionObj();
                }

                strJsonArray = readerReceiver.Split("],");
                if (strJsonArray.Length > 1)
                {
                    strJsonArray[0] += "]";
                    strJsonArray[1] += "]";
                }
                ConversacionObj conObj = new ConversacionObj();

                string strTemp = strJsonArray[0].Substring(strJsonArray[0].IndexOf("l_bullets_to_create")).Replace("l_bullets_to_create", "");
                strTemp = strTemp.Substring(4).Replace("[", "").Replace("]", "").Replace("}}", "}");
                string str_bullets_to_create = strTemp;

                if (!string.IsNullOrWhiteSpace(str_bullets_to_create))
                {
                    //Array.Clear(strStrArr, 0, strStrArr.Length);
                    str_bullets_to_create = str_bullets_to_create.Replace("},{", "}|°|{");
                    strStrArr = str_bullets_to_create.Split("|°|");
                    foreach (string item1 in strStrArr)
                    {
                        conObj.l_bullets_to_create.Add(item1);
                    }

                    conObj.LoadShots();
                }

                string strTemp2 = strJsonArray[1].Substring(strJsonArray[1].IndexOf("l_bullets_to_update")).Replace("l_bullets_to_update", "");
                strTemp2 = strTemp2.Substring(4).Replace("[", "").Replace("]", "").Replace("}}", "}");
                string str_bullets_to_update = strTemp2;

                if (!string.IsNullOrWhiteSpace(str_bullets_to_update))
                {
                    str_bullets_to_update = str_bullets_to_update.Replace("},{", "}|°|{");
                    //Array.Clear(strStrArr, 0, strStrArr.Length);
                    strStrArr2 = str_bullets_to_update.Split("|°|");
                    foreach (string item2 in strStrArr2)
                    {
                        conObj.l_bullets_to_update.Add(item2);
                    }

                    conObj.LoadShotPosUpdates();
                }

                string strTemp3 = strJsonArray[2].Substring(strJsonArray[2].IndexOf("l_bullets_to_change_state")).Replace("l_bullets_to_change_state", "");
                strTemp3 = strTemp3.Substring(4).Replace("[", "").Replace("]}", "");
                string str_bullets_to_change_state = strTemp3;

                if (!string.IsNullOrWhiteSpace(str_bullets_to_change_state))
                {
                    //Array.Clear(strStrArr, 0, strStrArr.Length);
                    //strStrArr3 = str_bullets_to_change_state.Split("\",\"");
                    str_bullets_to_change_state = str_bullets_to_change_state.Replace("},{", "}|°|{");
                    strStrArr3 = str_bullets_to_change_state.Split("|°|");
                    foreach (string item3 in strStrArr3)
                    {
                        conObj.l_bullets_to_change_state.Add(item3);
                    }

                    conObj.LoadShotStates();
                }

                return conObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ConversacionObjConverter) Read(): {0} Message: {1}", strJsonArray[0], ex.Message);
                return new ConversacionObj();
            }
        }

        public override void Write(Utf8JsonWriter writer, ConversacionObj conObj, JsonSerializerOptions options)
        {
            try
            {
                string strTemp = "{";
                int i = 0;
                int last = 0;
                strTemp += "\"l_bullets_to_create\" : [";
                last = conObj.l_bullets_to_create.Count;
                foreach (string item in conObj.l_bullets_to_create)
                {
                    strTemp += "\"" + item + "\"";
                    if (i < last - 1)
                    {
                        strTemp += ",";
                    }
                    i++;
                }
                strTemp += "],";
                i = 0;

                strTemp += "\"l_bullets_to_update\" : [";
                last = conObj.l_bullets_to_update.Count;
                foreach (string item in conObj.l_bullets_to_update)
                {
                    strTemp += "\"" + item + "\"";
                    if (i < last - 1)
                    {
                        strTemp += ",";
                    }
                    i++;
                }
                strTemp += "],";
                i = 0;

                strTemp += "\"l_bullets_to_change_state\" : [";
                last = conObj.l_bullets_to_change_state.Count;
                foreach (string item in conObj.l_bullets_to_change_state)
                {
                    strTemp += "\"" + item + "\"";
                    if (i < last - 1)
                    {
                        strTemp += ",";
                    }
                    i++;
                }
                strTemp += "]";
                strTemp += "}";

                strTemp = UtilityAssistant.CleanJSON(strTemp);

                while (strTemp.Contains("\"\""))
                {
                    strTemp = strTemp.Replace("\"\"", "\"");
                }

                while (strTemp.Contains("\\"))
                {
                    strTemp = strTemp.Replace("\\", "");
                }

                writer.WriteStringValue(strTemp);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ConversacionObjConverter) Write(): " + ex.Message);
            }
        }
    }
}
