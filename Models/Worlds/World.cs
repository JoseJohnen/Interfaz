using Interfaz.Models.Area;
using Interfaz.Models.Puppets;
using Interfaz.Models.Tiles;
using System.Collections.Concurrent;
using System.Numerics;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Interfaz.Models.Auxiliary;
using Interfaz.Models.Comms;
using Interfaz.Utilities;

namespace Interfaz.Models.Worlds
{
    public abstract class World
    {
        public float FrontBack;
        public float WestEast;
        public float Height;
        private string name;

        public ConcurrentDictionary<string, Tile> dic_worldTiles = new ConcurrentDictionary<string, Tile>();
        public Dictionary<Puppet, int> dic_SpawnList = new Dictionary<Puppet, int>();
        public List<GameSocketClient> l_players = new List<GameSocketClient>();
        public List<AreaDefiner> L_centros = new List<AreaDefiner>();

        public virtual System.Numerics.Vector3 Location {
            get
            {
                return location;
            }
            set 
            {
                location = value;
                Area.Name = "Location";
                Area.L_AreaDefiners.Where(c => c.NombreArea == "NW").First().Point.Item2 = new SerializedVector3(location + new Vector3(-0.8f / 2, 0.8f / 2, 0));
                Area.L_AreaDefiners.Where(c => c.NombreArea == "NE").First().Point.Item2 = new SerializedVector3(location + new Vector3(0.8f / 2, 0.8f / 2, 0));
                Area.L_AreaDefiners.Where(c => c.NombreArea == "SW").First().Point.Item2 = new SerializedVector3(location + new Vector3(-0.8f / 2, -0.8f / 2, 0));
                Area.L_AreaDefiners.Where(c => c.NombreArea == "SE").First().Point.Item2 = new SerializedVector3(location + new Vector3(0.8f / 2, -0.8f / 2, 0));
            }
        }

        public virtual string Name { get => name; 
            set 
            { 
                name = value;
                if (Area != null)
                {
                    Area.Name = "Area_" + name;
                }
            } 
        }

        private Vector3 location = new System.Numerics.Vector3(0, 0, 0);

        public Interfaz.Models.Area.Area Area = new Interfaz.Models.Area.Area(new List<AreaDefiner>() {
            new AreaDefiner(new Pares<string, SerializedVector3>("NW",new SerializedVector3(Vector3.Zero)), "NW"),
            new AreaDefiner(new Pares<string, SerializedVector3>("NE",new SerializedVector3(Vector3.Zero)), "NE"),
            new AreaDefiner(new Pares<string, SerializedVector3>("SW",new SerializedVector3(Vector3.Zero)), "SW"),
            new AreaDefiner(new Pares<string, SerializedVector3>("SE",new SerializedVector3(Vector3.Zero)), "SE"),
        }, "AreaWorld");

        public World(int westEast = 3, int height = 1, int frontBack = 3, string name = "")
        {
            WestEast = westEast;
            Height = height;
            FrontBack = frontBack;
            Name = name;
            Area = new Interfaz.Models.Area.Area(new List<AreaDefiner>() {
                new AreaDefiner(new Pares<string,SerializedVector3>("NW",new SerializedVector3(Vector3.Zero)), "NW"),
                new AreaDefiner(new Pares<string, SerializedVector3>("NE",new SerializedVector3(Vector3.Zero)), "NE"),
                new AreaDefiner(new Pares<string, SerializedVector3>("SW",new SerializedVector3(Vector3.Zero)), "SW"),
                new AreaDefiner(new Pares<string, SerializedVector3>("SE",new SerializedVector3(Vector3.Zero)), "SE"),
            }, "AreaWorld");
        }

        public virtual bool IsInsideAreaWorld(Vector3 position)
        {
            try
            {
                if (this.Area.Count > 0)
                {
                    AreaDefiner NW = this.Area.Where(c => c.Point.Item1 == "NW").First();
                    AreaDefiner NE = this.Area.Where(c => c.Point.Item1 == "NE").First();
                    AreaDefiner SW = this.Area.Where(c => c.Point.Item1 == "SW").First();
                    AreaDefiner SE = this.Area.Where(c => c.Point.Item1 == "SE").First();

                    if ((NW.Point.Item2.Z <= position.Z) && (SE.Point.Item2.Z >= position.Z))
                    {
                        if ((NW.Point.Item2.X <= position.X) && (SE.Point.Item2.X >= position.X))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error bool IsInsideAreaWorld(Vector3): " + ex.Message);
                return false;
            }
        }

        public virtual bool IsInsideRadiusWorld(Vector3 position, double distance = 15)
        {
            try
            {
                Vector3 rtVect3 = Vector3.Zero;
                foreach (AreaDefiner item  in L_centros)
                {
                    rtVect3 = UtilityAssistant.DistanceComparitorVector3(item.Point.Item2.ConvertToVector3(),position,(Single)distance);
                    //Si al menos uno de ellos esta en rango, es verdad
                    if (
                        rtVect3.X < distance / 2 &&
                        rtVect3.Y < distance / 2 &&
                        rtVect3.Z < distance / 2
                    )
                    {
                        return true;
                    }
                }
                //Si ninguno de ellos lo esta, es falso
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error bool IsInsideAreaWorld(Vector3): " + ex.Message);
                return false;
            }
        }

        public virtual World RegisterWorld(string nameOfTheWorld = "")
        {
            //    try
            //    {
            //        string name = "World_" + WorldController.dic_worlds.Count;
            //        if (nameOfTheWorld != "")
            //        {
            //            name = nameOfTheWorld;
            //        }
            //        WorldController.dic_worlds.TryAdd(name, this);
            return this;
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine("Error RegisterWorld(string): " + ex.Message);
            //        return null;
            //    }
        }

        public virtual World FillWorld(string TileClass = "")
        {
            try
            {
                float x = 0, y = 0, z = 0;
                do
                {
                    //world.worldTiles[x, y, z].Entity.Transform.parent = world.Instance.Transform;
                    //worldTiles[x, y, z] = new Tile_Primus("Tile_"+this.Name+"_"+x+"_"+y+"_"+z);
                    dic_worldTiles.TryAdd("Tile_" + x + "_" + y + "_" + z, new Grass("Tile_" + x + "_" + y + "_" + z));
                    if (x == WestEast - 1)
                    {
                        x = 0;
                        y++;

                        if (y == Height)
                        {
                            y = 0;
                            z++;

                            if (z == FrontBack)
                            {
                                break;
                            }
                        }
                    }
                    else if (x < WestEast)
                    {
                        x++;
                    }
                }
                while (x <= WestEast - 1 && y <= Height - 1 && z <= FrontBack - 1);
                return this;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error FillWorld: " + ex.Message);
                return null;
            }
        }

        public virtual bool EncontrarCentro()
        {
            try
            {
                if(dic_worldTiles.Count == 0)
                {
                    return false;
                }

                Tile primerTileComoEjemplo = dic_worldTiles.Values.FirstOrDefault();
                Vector2 nwVct2 = primerTileComoEjemplo.spriteSize;
                float profCentro = MathF.Floor(nwVct2.Y * this.FrontBack)/2;
                float largCentro = MathF.Floor(nwVct2.X * this.WestEast)/2;

                //Determinar si los valores son pares o impares para poner modificación acorde
                if(WestEast % 2 != 0)
                {
                    profCentro += 1;//(profundidadDelTile / 2);
                }

                if (FrontBack % 2 != 0)
                {
                    largCentro += 1;//(largoDelTile / 2);
                }

                L_centros.Add(new AreaDefiner()
                {
                    NombreArea = "centro_1",
                    Point = new Pares<string, SerializedVector3>()
                    {
                        Item1 = "centro",
                        Item2 = new SerializedVector3(new Vector3(largCentro,primerTileComoEjemplo.Position.Y,profCentro)),
                    }
                });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (World) bool EncontrarCentro(): " + ex.Message);
                return false;
            }
        }

        public static List<Type> TypesOfWorlds()
        {
            List<Type> myTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(World)) && !type.IsAbstract).ToList();
            return myTypes;
        }

        #region Métodos JSON
        public virtual string ToJson()
        {
            try
            {

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new WorldConverter()
                    },
                };
                //ReadCommentHandling = JsonCommentHandling.Skip,
                //    AllowTrailingCommas = true,

                return JsonSerializer.Serialize(this, serializeOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (World) ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public virtual World FromJson(string json)
        {
            string txt = json;
            try
            {
                txt = UtilityAssistant.CleanJSON(txt.Replace("\u002B", "+"));

                //json = UtilityAssistant.CleanJSON(json);

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new WorldConverter()
                    },
                };

                //AllowTrailingCommas = true,
                //ReadCommentHandling = JsonCommentHandling.Skip,
                //json = UtilityAssistant.CleanJSON(json);
                World wrldObj = JsonSerializer.Deserialize<World>(txt, serializeOptions);//, serializeOptions);
                //this = prgObj;

                if (wrldObj != null)
                {
                    this.WestEast = wrldObj.WestEast;
                    this.Height = wrldObj.Height;
                    this.FrontBack = wrldObj.FrontBack;
                    this.dic_worldTiles = wrldObj.dic_worldTiles;
                }

                return wrldObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\nError (World) FromJson(): " + json + " ex.Message: " + ex.Message);
                return null;
            }
        }

        public static World CreateFromJson(string json)
        {
            try
            {
                string clase = UtilityAssistant.CleanJSON(json);
                clase = UtilityAssistant.ExtractAIInstructionData(clase, "Class").Replace("\"", "");

                Type typ = World.TypesOfWorlds().Where(c => c.Name == clase).FirstOrDefault();
                if (typ == null)
                {
                    typ = World.TypesOfWorlds().Where(c => c.FullName == clase).FirstOrDefault();
                }

                object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.
                                                                  //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list

                World prgObj = ((World)obtOfType);
                return prgObj.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (World) CreateFromJson(): " + ex.Message);
                return null;
            }
        }
        #endregion
    }

    public class WorldConverter : System.Text.Json.Serialization.JsonConverter<World>
    {
        public override World Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string[] strJsonArray = new string[1];
            string[] strStrArr = new string[1];
            //string[] strStrArr2 = new string[1];
            //string[] strStrArr3 = new string[1];
            //string readerReceiver = string.Empty;
            try
            {
                //TODO: Corregir, testear y terminar
                //readerReceiver = reader.GetString();
                JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
                string tempString = jsonDoc.RootElement.GetRawText();

                string clase = UtilityAssistant.CleanJSON(tempString);
                clase = UtilityAssistant.ExtractValue(clase, "Class").Replace("\"", "");

                Type typ = World.TypesOfWorlds().Where(c => c.Name == clase).FirstOrDefault();
                if (typ == null)
                {
                    typ = World.TypesOfWorlds().Where(c => c.FullName == clase).FirstOrDefault();
                }

                object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.
                                                                  //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list

                World wrldObj = ((World)obtOfType);

                string strValue = UtilityAssistant.ExtractValue(tempString, "WestEast");
                wrldObj.WestEast = Convert.ToInt32(strValue);
                strValue = UtilityAssistant.ExtractValue(tempString, "Height");
                wrldObj.Height = Convert.ToInt32(strValue);
                strValue = UtilityAssistant.ExtractValue(tempString, "FrontBack");
                wrldObj.FrontBack = Convert.ToInt32(strValue);
                wrldObj.Name = UtilityAssistant.ExtractValue(tempString, "Name");
                strValue = UtilityAssistant.ExtractValue(tempString, "Location");
                wrldObj.Location = Vector3Converter.Converter(strValue);

                strValue = UtilityAssistant.ExtractValue(tempString, "Area");
                wrldObj.Area = Area.Area.CreateFromJson(strValue);


                /*if (string.IsNullOrWhiteSpace(readerReceiver) || readerReceiver.Equals("\"{\""))
                {
                    return null;
                }
                
                strJsonArray = tempString.Split("],");
                if (strJsonArray.Length > 1)
                {
                    strJsonArray[0] += "]";
                    strJsonArray[1] += "]";
                }*/

                strJsonArray[0] = tempString;

                string strTemp = strJsonArray[0].Substring(strJsonArray[0].IndexOf("dic_worldTiles")).Replace("dic_worldTiles", "");
                Tile tile = null;
                List<string> l_string = new List<string>(strTemp.Split("},{", StringSplitOptions.RemoveEmptyEntries));
                foreach (string item in l_string)
                {
                    //strTemp = UtilityAssistant.ExtractValue(item, "Value");
                    strTemp = item.Substring(item.IndexOf("\"Value\""));
                    strTemp = strTemp.Replace("\"Value\":", "").Replace("}}]}", "}");
                    tile = Tile.CreateFromJson(strTemp);
                    wrldObj.dic_worldTiles.TryAdd(tile.Name, tile);
                }
                //strTemp = strTemp.Substring(4).Replace("[", "").Replace("]", "").Replace("}}", "}");

                /*string str_tiles_to_create = string.Empty;
                if(!strTemp.Equals("}"))
                {
                    str_tiles_to_create = strTemp;
                }

                if (!string.IsNullOrWhiteSpace(str_tiles_to_create))
                {
                    //Array.Clear(strStrArr, 0, strStrArr.Length);
                    str_tiles_to_create = str_tiles_to_create.Replace("},{", "}|°|{");
                    strStrArr = str_tiles_to_create.Split("|°|");
                    foreach (string item1 in strStrArr)
                    {
                        //wwrldObj.dic_worldTiles.TryAdd(item1);
                    }

                    //wrldObj.LoadShots();
                }*/

                return wrldObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (WorldConverter) Read(): {0} Message: {1}", strJsonArray[0], ex.Message);
                return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, World wldObj, JsonSerializerOptions options)
        {
            try
            {
                string strTemp = string.Empty;//"{";
                int i = 0;
                int last = 0;
                strTemp += "\"dic_worldTiles\" : [";
                last = wldObj.dic_worldTiles.Count;
                foreach (KeyValuePair<string, Tile> item in wldObj.dic_worldTiles)
                {
                    strTemp += "{\"Key\":\"" + item.Key + "\",\"Value\":\"" + item.Value.ToJson() + "\"}";
                    if (i < last - 1)
                    {
                        strTemp += ",";
                    }
                    i++;
                }
                strTemp += "]"; //,";
                //strTemp += "}";

                //strTemp = UtilityAssistant.CleanJSON(strTemp);

                while (strTemp.Contains("\"\""))
                {
                    strTemp = strTemp.Replace("\"\"", "\"");
                }

                while (strTemp.Contains("\\"))
                {
                    strTemp = strTemp.Replace("\\", "");
                }

                string WestEast = wldObj.WestEast.ToString();
                string Height = wldObj.Height.ToString();
                string FrontBack = wldObj.FrontBack.ToString();
                string Name = string.IsNullOrWhiteSpace(wldObj.Name) ? "null" : wldObj.Name;

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

                string Location = System.Text.Json.JsonSerializer.Serialize(wldObj.Location, serializeOptions);
                string Area = wldObj.Area.ToJson();
                string Class = wldObj.GetType().Name;

                char[] a = { '"' };

                string wr = string.Concat("{", new string(a), "Name", new string(a), ":", new string(a), Name, new string(a),
                    ", ", new string(a), "Class", new string(a), ":", new string(a), Class, new string(a),
                    ", ", new string(a), "WestEast", new string(a), ":", WestEast,
                    ", ", new string(a), "Height", new string(a), ":", Height,
                    ", ", new string(a), "FrontBack", new string(a), ":", FrontBack,
                    ", ", new string(a), "Location", new string(a), ":", Location,
                    ", ", new string(a), "Area", new string(a), ":", Area,
                    ", ", strTemp,
                    "}");

                string resultJson = Regex.Replace(wr, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");

                writer.WriteStringValue(wr);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (WorldConverter) Write(): " + ex.Message);
            }
        }
    }
}