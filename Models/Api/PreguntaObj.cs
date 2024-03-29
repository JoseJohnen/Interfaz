﻿using System.Text.Json;

namespace Interfaz.Models.Api
{
    public class PreguntaObj
    {
        public List<string> l_id_bullets_preguntando = new List<string>();

        public string ToJson()
        {
            try
            {

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                    new PreguntaObjConverter()
                    },
                };
                //ReadCommentHandling = JsonCommentHandling.Skip,
                //    AllowTrailingCommas = true,

                return JsonSerializer.Serialize(this, serializeOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (PreguntaObj) ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public PreguntaObj FromJson(string json)
        {
            try
            {
                string strJson = json;
                string[] strJsonArr = new string[1];
                strJsonArr[0] = strJson;
                if (strJson.Contains("PR:"))
                {
                    strJsonArr = strJson.Split("PR:", StringSplitOptions.RemoveEmptyEntries);
                }

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new PreguntaObjConverter()
                    },
                    AllowTrailingCommas = true,
                };

                //AllowTrailingCommas = true,
                //ReadCommentHandling = JsonCommentHandling.Skip,
                PreguntaObj prgObj = JsonSerializer.Deserialize<PreguntaObj>(strJsonArr[0], serializeOptions);
                //this = prgObj;

                return prgObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (PreguntaObj) FromJson(): " + ex.Message);
                return null;
            }
        }

        public static PreguntaObj CreateFromJson(string json)
        {
            try
            {
                PreguntaObj prgObj = new();
                return prgObj.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (PreguntaObj) CreateFromJson(): " + ex.Message);
                return null;
            }
        }
    }

    public class PreguntaObjConverter : System.Text.Json.Serialization.JsonConverter<PreguntaObj>
    {
        public override PreguntaObj Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string strJson = string.Empty;
            try
            {
                //TODO: Corregir, testear y terminar
                //strJson = reader.GetString();
                JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
                strJson = jsonDoc.RootElement.GetRawText();

                string[] strJsonArr = new string[1];
                strJsonArr[0] = strJson;
                if (strJson.Contains("PR:"))
                {
                    strJsonArr = strJson.Split("PR:", StringSplitOptions.RemoveEmptyEntries);
                }

                string tmpItem = strJsonArr[0];
                string[] tmpArrItm = new string[1];

                tmpItem = tmpItem.Substring(tmpItem.IndexOf("[") + 1);
                tmpItem = tmpItem.Substring(0, tmpItem.IndexOf("]"));
                tmpArrItm = tmpItem.Split(",", StringSplitOptions.RemoveEmptyEntries);
                PreguntaObj preguntaObj = new PreguntaObj();
                if (tmpArrItm.Length > 0)
                {
                    foreach (string it in tmpArrItm)
                    {
                        preguntaObj.l_id_bullets_preguntando.Add(it);
                    }
                }

                return preguntaObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (PreguntaObjConverter) Read(): {0} Message: {1}", strJson, ex.Message);
                return default;
            }
        }

        public override void Write(Utf8JsonWriter writer, PreguntaObj conObj, JsonSerializerOptions options)
        {
            try
            {
                string strTemp = "{";
                int i = 0;
                int last = 0;
                strTemp += "\"l_id_bullets_preguntando\" : [";
                last = conObj.l_id_bullets_preguntando.Count - 1;
                foreach (string item in conObj.l_id_bullets_preguntando)
                {
                    strTemp += item;
                    if (i < last)
                    {
                        strTemp += ",";
                    }
                    i++;
                }
                strTemp += "]";

                strTemp += "}";

                //TODO: Corregir, testear y terminar
                //string Id = "\"" + shot.Id + "\"";
                //string Pos = new SerializedVector3(shot.Pos).ToXML();
                //string Pos = new SerializedVector3(shot.Pos).ToJson();

                //string resultJson = "{\"Id\":" + Id + ", \"Pos\":" + Pos + "}";
                writer.WriteStringValue(strTemp);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (PreguntaObjConverter) Write(): " + ex.Message);
            }
        }
    }

}
