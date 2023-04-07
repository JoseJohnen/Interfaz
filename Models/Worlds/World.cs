using Interfaz.Models.Area;
using Interfaz.Models.Puppets;
using Interfaz.Models.Tiles;
using Interfaz.Utilities;
using System.Collections.Concurrent;
using System.Numerics;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Interfaz.Models.Worlds
{
    public abstract class World
    {
        public float FrontBack;
        public float WestEast;
        public float Height;
        public string Name;
        //public Tile_Primus[,,] worldTiles;
        public ConcurrentDictionary<string, Tile> dic_worldTiles = new ConcurrentDictionary<string, Tile>();
        public Dictionary<Puppet,int> dic_SpawnList = new Dictionary<Puppet, int>(); 
        public System.Numerics.Vector3 Location { get; set; } = new System.Numerics.Vector3(0, 0, 0);


        public Interfaz.Models.Area.Area Area = new Interfaz.Models.Area.Area(new List<AreaDefiner>() {
            new AreaDefiner(),
            new AreaDefiner(),
            new AreaDefiner(),
            new AreaDefiner(),
        });

        //public List<Tile_Primus> l_FloorTiles = new List<Tile_Primus>();
        
        public World(int westEast = 3, int height = 1, int frontBack = 3, string name = "")
        {
            WestEast = westEast;
            Height = height;
            FrontBack = frontBack;
            Name = name;
        }

        public virtual bool IsInsideAreaWorld(Vector3 position)
        {
            try
            {
                AreaDefiner NW = this.Area.Where(c => c.Point.Item1 == "NW").First();
                AreaDefiner NE = this.Area.Where(c => c.Point.Item1 == "NE").First();
                AreaDefiner SW = this.Area.Where(c => c.Point.Item1 == "SW").First();
                AreaDefiner SE = this.Area.Where(c => c.Point.Item1 == "SE").First();

                if((NW.Point.Item2.Z <= position.Z) && (SE.Point.Item2.Z >= position.Z))
                {
                    if((NW.Point.Item2.X <= position.X) && (SE.Point.Item2.X >= position.X))
                    {
                        return true;
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
                txt = Interfaz.Utilities.UtilityAssistant.CleanJSON(txt.Replace("\u002B", "+"));

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

        #region Constructores y métodos de creación de mundos
        //public static World CreateWorld(int westEast = 20, int height = 2, int frontBack = 20)
        //{
        //    World world = new World(westEast, height, frontBack);
        //    world.FrontBack = frontBack;
        //    world.WestEast = westEast;
        //    world.Height = height;
        //    int x = 0, y = 0, z = 0;
        //    do
        //    {
        //        //world.worldTiles[x, y, z].Entity.Transform.parent = world.Instance.Transform;
        //        if (x == world.WestEast - 1)
        //        {
        //            x = 0;
        //            y++;

        //            if (y == world.Height)
        //            {
        //                y = 0;
        //                z++;

        //                if (z == world.FrontBack)
        //                {
        //                    break;
        //                }
        //            }
        //        }
        //        else if (x < world.WestEast)
        //        {
        //            x++;
        //        }
        //    }
        //    while (x <= world.WestEast - 1 && y <= world.Height - 1 && z <= world.FrontBack - 1);
        //    return world;
        //}

        /*public World(Prefab prefab, int westEast = 20, int height = 1, int frontBack = 20)
        {
            try
            {
                this.Prefab = prefab;
                this.FrontBack = frontBack;
                this.WestEast = westEast;
                this.Height = height;

                this.Instance = new Entity("World_" + WorldController.dic_worlds.Count());
                this.Instance.Transform.Parent = WorldController.Instance.Entity.Transform;

                worldTiles = new Entity[WestEast + 1, Height + 1, FrontBack + 1];
                float xScale = prefab.Transform.lossyScale.X;
                float yScale = prefab.Transform.lossyScale.Y;
                float zScale = prefab.Transform.lossyScale.Z;
                int x = 0, y = 0, z = 0;
                do
                {
                    var a = prefab.Instantiate();
                    Entity.Scene.Entities.AddRange(a);
                    Entity EntityReference = a[0];

                    if (x == 0 && y == 0 && z == 0)
                    {
                        xScale = 0;
                        yScale = 0;
                        zScale = 0;
                    }

                    worldTiles[x, y, z] = new Entity(EntityReference.Name + "_" + x + "_" + y + "_" + z);
                    //worldTiles[x, y, z].Transform.parent = this.Entity.Transform;
                    worldTiles[x, y, z].Transform.Position = new Vector3(x + xScale, y + yScale, z + zScale); //Modificador X Tamaño

                   
                    EntityReference.Transform.Position = worldTiles[x, y, z].Transform.Position;
                    EntityReference.Transform.Parent = worldTiles[x, y, z].Transform;
                    worldTiles[x, y, z].Transform.Parent = this.Instance.Transform;
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
            }
            catch (Exception ex)
            {
                //Debug.Log("World() (Builder) Error: " + ex.ToString());
            }
            ////Debug.Log("World Created with " + (FrontBack * WestEast * Height) + " tiles (Entitys).");
        }
        #endregion

        #region Regiones Suplementarias Creacion de Mundo
        public bool AddToFloorTiles(Entity go)
        {
            if (!l_FloorTiles.Any(r => r.Name == go.Name))
            {
                l_FloorTiles.Add(go);
                return true;
            }
            return false;
        }

        public bool CheckFloorTiles(Entity go)
        {
            if (!l_FloorTiles.Any(r => r.Name == go.Name))
            {
                return true;
            }
            return false;
        }

        public Entity NameToTile(string name)
        {
            try
            {
                string strTile = name;
                strTile = strTile.Substring(strTile.IndexOf('_') + 1);
                string sX = strTile.Substring(0, strTile.IndexOf('_'));

                strTile = strTile.Substring(strTile.IndexOf('_') + 1);
                string sY = strTile.Substring(0, strTile.IndexOf('_'));

                string sZ = strTile.Substring(strTile.IndexOf('_') + 1);

                int X = Convert.ToInt32(sX);
                int Y = Convert.ToInt32(sY);
                int Z = Convert.ToInt32(sZ);

                return this.worldTiles[X, Y, Z];
            }
            catch (Exception ex)
            {
                //Debug.Log("Tile_Primus NameToTile(string name) ERROR: " + ex.ToString());
                return null;
            }
        }

        public bool Load()
        {
            string wrld = File.ReadAllText(Application.dataPath + "/savedWorldExample.sav");
            int x = 0;
            int y = 0;
            int z = 0;
            int i = 0;
            float xScale = this.Prefab.Transform.lossyScale.X;
            float yScale = this.Prefab.Transform.lossyScale.Y;
            float zScale = this.Prefab.Transform.lossyScale.Z;
            this.FrontBack = Convert.ToInt32(wrld.ObtainValueFromString("FrontBack"));
            this.WestEast = Convert.ToInt32(wrld.ObtainValueFromString("WestEast"));
            this.Height = Convert.ToInt32(wrld.ObtainValueFromString("Height"));
            string[] arrTiles = wrld.ObtainValueFromString("worldTiles").Split(',');
            do
            {
                this.worldTiles[x, y, z] = null;
                if (arrTiles[i] != " 0 ")
                {
                    this.worldTiles[x, y, z] = new Entity(this.Prefab.Name + "_" + x + "_" + y + "_" + z);
                    this.worldTiles[x, y, z].Transform.Position = new Vector3(x + xScale, y + yScale, z + zScale); //Modificador X Tamaño

                    Entity EntityReference = Instantiate(this.Prefab) as Entity;
                    EntityReference.Transform.Position = worldTiles[x, y, z].Transform.Position;
                    EntityReference.Transform.parent = worldTiles[x, y, z].Transform;
                    this.worldTiles[x, y, z].Entity.Transform.parent = WorldController.Instance.Transform;
                }
                if (x == this.WestEast - 1)
                {
                    x = 0;
                    y++;

                    if (y == this.Height)
                    {
                        y = 0;
                        z++;

                        if (z == this.FrontBack)
                        {
                            break;
                        }
                    }
                }
                else if (x < this.WestEast)
                {
                    x++;
                }
                i++;
            }
            while (x <= this.WestEast - 1 && y <= this.Height - 1 && z <= this.FrontBack - 1);
            return true;
        }

        public bool Save()
        {
            string wrld = "{ \"FrontBack\":" + FrontBack + ", \"WestEast\":" + WestEast + ", \"Height\":" + Height + ", \"worldTiles\":[";
            int x = 0;
            int y = 0;
            int z = 0;
            string tmpString = string.Empty;
            do
            {
                tmpString += this.worldTiles[x, y, z] != null ? 1 : 0;
                tmpString += " , ";
                if (x == this.WestEast - 1)
                {
                    x = 0;
                    y++;

                    if (y == this.Height)
                    {
                        y = 0;
                        z++;

                        if (z == this.FrontBack)
                        {
                            break;
                        }
                    }
                }
                else if (x < this.WestEast)
                {
                    x++;
                }
            }
            while (x <= this.WestEast - 1 && y <= this.Height - 1 && z <= this.FrontBack - 1);
            wrld += ExtensionMethods.ReplaceLastOccurrence(tmpString, ", ", "") + "]}";
            //Debug.Log(wrld);
            File.WriteAllText((Application.dataPath + "/savedWorldExample.sav"), wrld);
            //Debug.Log(Application.dataPath);
            return true;
        }

        public void Empty()
        {
            int x = 0;
            int y = 0;
            int z = 0;
            do
            {
                Destroy(this.worldTiles[x, y, z].Entity);
                this.worldTiles[x, y, z] = null;
                if (x == this.WestEast - 1)
                {
                    x = 0;
                    y++;

                    if (y == this.Height)
                    {
                        y = 0;
                        z++;

                        if (z == this.FrontBack)
                        {
                            break;
                        }
                    }
                }
                else if (x < this.WestEast)
                {
                    x++;
                }
            }
            while (x <= this.WestEast - 1 && y <= this.Height - 1 && z <= this.FrontBack - 1);
        }

        public override void Update()
        {
        }*/

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