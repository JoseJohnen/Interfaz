using Interfaz.Utilities;
using System.Text.Json;

namespace Interfaz.Models.Auxiliary
{
    public enum PRIORITY { PRIORITYDAM = 0, PRIORITYDIS = 1 }

    public class IA_Instructions
    {
        public List<string> evaluadores = new List<string>();
        //public List<string> moves = new List<string>();

        public PRIORITY priority { get; set; } = 0;
        public IA_Instructions ()
        {
        }

        public IA_Instructions(List<string> l_evaluadores)//, List<string> l_moves)
        {
            evaluadores = l_evaluadores;
            //moves = l_moves;
        }

        public List<IA_Message> LoadEvaluadores()
        {
            try
            {
                if (evaluadores == null)
                {
                    evaluadores = new List<string>();
                }

                List<IA_Message> iA_Messages = new List<IA_Message>();
                string tmpItem = string.Empty;
                foreach (string item in evaluadores)
                {
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        continue;
                    }
                    tmpItem = UtilityAssistant.CleanJSON(item);
                    iA_Messages.Add(IA_Message.CreateFromJson(tmpItem));
                }
                return iA_Messages;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (IA_Instructions) LoadAttacks(): " + ex.Message);
                return new List<IA_Message>();
            }
        }

        /*public List<IA_Message> LoadMoves()
        {
            try
            {
                if (moves == null)
                {
                    moves = new List<string>();
                }

                List<IA_Message> iA_Messages = new List<IA_Message>();
                string tmpItem = string.Empty;
                foreach (string item in moves)
                {
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        continue;
                    }
                    tmpItem = UtilityAssistant.CleanJSON(item);
                    iA_Messages.Add(IA_Message.CreateFromJson(tmpItem));
                }
                return iA_Messages;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (IA_Instructions) LoadMoves(): " + ex.Message);
                return new List<IA_Message>();
            }
        }*/

        public string ToJson()
        {
            try
            {
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        //new Vector3Converter()
                        new IA_InstructionsConverter()
                    },
                    AllowTrailingCommas = true,
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
                Console.WriteLine("Error (IA_Instructions) ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public IA_Instructions FromJson(string json)
        {
            try
            {
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new IA_InstructionsConverter()
                    },
                    AllowTrailingCommas = true,
                    //Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    //WriteIndented = true
                };

                string strJson = UtilityAssistant.CleanJSON(json);
                //AllowTrailingCommas = true,
                //ReadCommentHandling = JsonCommentHandling.Skip,
                IA_Instructions iAInstr = System.Text.Json.JsonSerializer.Deserialize<IA_Instructions>(strJson, serializeOptions);
                //this = shot;

                return iAInstr;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (IA_Instructions) FromJson(): " + ex.Message);
                return default;
            }
        }

        public static IA_Instructions CreateFromJson(string json)
        {
            try
            {
                IA_Instructions iAInstr = new();
                return iAInstr.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (IA_Instructions) FromJson(): " + ex.Message);
                return default;
            }
        }
    }

    public class IA_InstructionsConverter : System.Text.Json.Serialization.JsonConverter<IA_Instructions>
    {
        public override IA_Instructions Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string strJson = string.Empty;
            string[] strJsonArray = new string[1];
            string[] strStrArr = new string[1];
            string[] strStrArr2 = new string[1];
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
                    return new IA_Instructions();
                }

                IA_Instructions iaInstr = new IA_Instructions();

                string strPriority = readerReceiver.Substring(0, readerReceiver.IndexOf(",") + 1);
                string strPriorityValue = strPriority.Substring(strPriority.IndexOf(":")+1);
                strPriorityValue = strPriorityValue.Replace(",","");
                iaInstr.priority = (PRIORITY)Enum.Parse(typeof(PRIORITY), strPriorityValue);

                readerReceiver = readerReceiver.Replace(strPriority, "");
                strJsonArray = readerReceiver.Split("],");
                if (strJsonArray.Length > 1)
                {
                    strJsonArray[0] += "]";
                    strJsonArray[1] += "]";
                }

                string strTemp = strJsonArray[0].Substring(strJsonArray[0].IndexOf("attacks")).Replace("attacks", "");
                strTemp = strTemp.Substring(4).Replace("[", "").Replace("]", "").Replace("}}", "}");
                string str_bullets_to_create = strTemp;

                if (!string.IsNullOrWhiteSpace(str_bullets_to_create))
                {
                    //Array.Clear(strStrArr, 0, strStrArr.Length);
                    str_bullets_to_create = str_bullets_to_create.Replace("},{", "}|°|{");
                    strStrArr = str_bullets_to_create.Split("|°|");
                    foreach (string item1 in strStrArr)
                    {
                        iaInstr.evaluadores.Add(item1);
                    }

                    //iaInstr.LoadAttacks();
                }

                /*string strTemp2 = strJsonArray[1].Substring(strJsonArray[1].IndexOf("moves")).Replace("moves", "");
                strTemp2 = strTemp2.Substring(4).Replace("[", "").Replace("]", "").Replace("}}", "}");
                string str_bullets_to_update = strTemp2;

                if (!string.IsNullOrWhiteSpace(str_bullets_to_update))
                {
                    str_bullets_to_update = str_bullets_to_update.Replace("},{", "}|°|{");
                    //Array.Clear(strStrArr, 0, strStrArr.Length);
                    strStrArr2 = str_bullets_to_update.Split("|°|");
                    foreach (string item2 in strStrArr2)
                    {
                        iaInstr.moves.Add(item2);
                    }

                    iaInstr.LoadMoves();
                }*/

                return iaInstr;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (IA_InstructionsConverter) Read(): {0} Message: {1}", strJson, ex.Message);
                return default;
            }
        }

        public override void Write(Utf8JsonWriter writer, IA_Instructions iaInstr, JsonSerializerOptions options)
        {
            try
            {
                string strTemp = "{";

                strTemp += "\"PRIORITY\":" + ((int)iaInstr.priority)+",";

                int i = 0;
                int last = 0;
                strTemp += "\"attacks\" : [";
                last = iaInstr.evaluadores.Count;
                foreach (string item in iaInstr.evaluadores)
                {
                    strTemp += "\"" + item + "\"";
                    if (i < last - 1)
                    {
                        strTemp += ",";
                    }
                    i++;
                }
                /*strTemp += "],";
                i = 0;

                strTemp += "\"moves\" : [";
                last = iaInstr.moves.Count;
                foreach (string item in iaInstr.moves)
                {
                    strTemp += "\"" + item + "\"";
                    if (i < last - 1)
                    {
                        strTemp += ",";
                    }
                    i++;
                }*/
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
                Console.WriteLine("Error: (IA_InstructionsConverter) Write(): " + ex.Message);
            }
        }
    }

}
