using Interfaz.Models.Area;
using Interfaz.Models.Auxiliary;
using Interfaz.Auxiliary;
using System.Numerics;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Interfaz.Models
{
    public abstract class Tile
    {
        private string name = string.Empty;
        private Vector3 position = Vector3.Zero;
        private Vector3 inWorldPos = Vector3.Zero;

        public virtual string Name { get => name; set => name = value; }
        public virtual Vector3 Position { get => position; set => position = value; }
        public virtual Vector3 InWorldPos { get => inWorldPos; set => inWorldPos = value; }
        public virtual Vector2 spriteSize { get; set; }
        public virtual Interfaz.Models.Area.Area Area { get => area; set => area = value; } 

        private Interfaz.Models.Area.Area area = new Interfaz.Models.Area.Area(new List<AreaDefiner>() {
            new AreaDefiner(),
            new AreaDefiner(),
            new AreaDefiner(),
            new AreaDefiner(),
        });

        public Tile(string name = "", Vector3 position = new(), Vector3 inworldpos = new())
        {
            Name = name;
            Position = position;
            InWorldPos = inworldpos;
        }

        #region Auxiliares
        public virtual string ToJson()
        {
            try
            {
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new TileConverter(),
                    }
                };

                string strResult = JsonSerializer.Serialize(this, serializeOptions);
                return strResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) String ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public virtual Tile FromJson(string Text)
        {
            string txt = Text;
            try
            {
                txt = Interfaz.Auxiliary.UtilityAssistant.CleanJSON(txt.Replace("\u002B", "+"));

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new TileConverter(),
                    }
                };

                Tile strResult = JsonSerializer.Deserialize<Tile>(txt, serializeOptions);

                //TODO: VER QUE EL OBJETO AL HACER TO JSON SALVE EL NOMBRE DE LA CLASE TAMBIÉN
                //TODO2: RECUERDA QUE DEBES EXTRAER EL OBJETO

                if (strResult != null)
                {
                    this.Name = strResult.Name;
                    this.Position = strResult.Position;
                    this.InWorldPos = strResult.InWorldPos;
                }
                return strResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Tile_Primus) FromJson: " + ex.Message + " Text: " + txt);
                return null;
            }
        }

        public static Tile CreateFromJson(string json)
        {
            try
            {
                string clase = UtilityAssistant.CleanJSON(json);
                clase = UtilityAssistant.ExtractAIInstructionData(clase, "Class").Replace("\"", "");

                Type typ = Tile.TypesOfTiles().Where(c => c.Name == clase).FirstOrDefault();
                if (typ == null)
                {
                    typ = Tile.TypesOfTiles().Where(c => c.FullName == clase).FirstOrDefault();
                }

                object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.
                                                                  //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list

                Tile prgObj = ((Tile)obtOfType);
                return prgObj.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Tile_Primus) CreateFromJson(): " + ex.Message);
                return null;
            }
        }

        public static List<Type> TypesOfTiles()
        {
            List<Type> myTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(Tile)) && !type.IsAbstract).ToList();
            return myTypes;
        }

        public virtual void InstanceTile(string name = "", Vector3 position = default(Vector3), Vector3 inworldpos = default(Vector3))
        {
            try
            {
                Vector3 Pos = Vector3.Zero;
                if (position != default(Vector3))
                {
                    Pos = position;
                }

                if (inworldpos != default(Vector3))
                {
                    InWorldPos = inworldpos;
                }

                if (!string.IsNullOrEmpty(name))
                {
                    this.Name = name;
                }

                    this.Position = Pos;

                    // Get the size of the sprite
                    spriteSize = new Vector2(0.8f, 0.8f);

                    // Calculate the corners of the sprite
                    Vector3 topLeft = this.position + new Vector3(-spriteSize.X / 2, spriteSize.Y / 2, 0);
                    Vector3 topRight = this.position + new Vector3(spriteSize.X / 2, spriteSize.Y / 2, 0);
                    Vector3 bottomLeft = this.position + new Vector3(-spriteSize.X / 2, -spriteSize.Y / 2, 0);
                    Vector3 bottomRight = this.position + new Vector3(spriteSize.X / 2, -spriteSize.Y / 2, 0);
                    this.Area.L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>() { Item1 = "NW", Item2 = new SerializedVector3(topLeft) };
                    this.Area.L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>() { Item1 = "NE", Item2 = new SerializedVector3(topRight) };
                    this.Area.L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>() { Item1 = "SW", Item2 = new SerializedVector3(bottomLeft) };
                    this.Area.L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>() { Item1 = "SE", Item2 = new SerializedVector3(bottomRight) };

                    //SceneInstance sceneInstance = WorldController.game.SceneSystem.SceneInstance;
                    //this.entity = sceneInstance.RootScene.Entities.Where(c => c.Name == base.Name).FirstOrDefault();

                    //Correct system rotation
                    //Entity.Transform.Rotation *= Quaternion.RotationX(Convert.ToSingle(MMO_Client.Code.Assistants.UtilityAssistant.ConvertDegreesToRadiants(90)));

                    //More precise rotation
                    //entity.Transform.Rotation *= MMO_Client.Code.Assistants.UtilityAssistant.ConvertSystemNumericsToStrideQuaternion(System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 2));

                    //Entity.Transform.Position = Code.Assistants.UtilityAssistant.ConvertVector3NumericToStride(Pos);
                    return;
                
                Console.WriteLine("Error (MMO_Client.Models.TilesModels.Tile) InstanceTile: SPRITE NO ENCONTRADO PARA CLASE " + this.GetType().FullName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MMO_Client.Models.TilesModels.Tile) InstanceTile: " + ex.Message);
            }
        }
        #endregion
    }

    public class TileConverter : System.Text.Json.Serialization.JsonConverter<Tile>
    {
        public override Tile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string strJson = string.Empty;
            try
            {
                //TODO: Corregir, testear y terminar
                JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
                strJson = jsonDoc.RootElement.GetRawText();
                //strJson = reader.GetString();

                string clase = UtilityAssistant.CleanJSON(strJson);
                clase = UtilityAssistant.ExtractValue(clase, "Class").Replace("\"", "");

                Type typ = Tile.TypesOfTiles().Where(c => c.Name == clase).FirstOrDefault();
                if (typ == null)
                {
                    typ = Tile.TypesOfTiles().Where(c => c.FullName == clase).FirstOrDefault();
                }

                object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.
                                                                  //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list

                Tile prgObj = ((Tile)obtOfType);

                string pst = UtilityAssistant.ExtractValue(strJson, "Position");
                prgObj.Position = UtilityAssistant.Vector3Deserializer(pst);
                pst = UtilityAssistant.ExtractValue(strJson, "InWorldPos");
                prgObj.InWorldPos = UtilityAssistant.Vector3Deserializer(pst);
                prgObj.Name = UtilityAssistant.ExtractValue(strJson, "Name");

                return prgObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (TileConverter) Read(): {0} Message: {1}", strJson, ex.Message);
                return default;
            }
        }

        public override void Write(Utf8JsonWriter writer, Tile tle, JsonSerializerOptions options)
        {
            try
            {
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


                //Para deserealizar los vector3 serializados: UtilityAssistant.Vector3Deserializer(tle);

                //TODO: Corregir, testear y terminar
                string Name = string.IsNullOrWhiteSpace(tle.Name) ? "null" : tle.Name;
                string Position = System.Text.Json.JsonSerializer.Serialize(tle.Position, serializeOptions);
                string InWorldPos = System.Text.Json.JsonSerializer.Serialize(tle.InWorldPos, serializeOptions);
                string Class = tle.GetType().Name;

                char[] a = { '"' };

                string wr = string.Concat("{ ", new string(a), "Name", new string(a), ":", new string(a), Name, new string(a),
                    ", ", new string(a), "Class", new string(a), ":", new string(a), Class, new string(a),
                    ", ", new string(a), "Position", new string(a), ":", Position,
                    ", ", new string(a), "InWorldPos", new string(a), ":", InWorldPos,
                    "}");

                string resultJson = Regex.Replace(wr, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");
                //string resultJson = "{Id:" + Id + ", LN:" + LauncherName + ", Type:" + Type + ", OrPos:" + LauncherPos + ", WPos:" + WeaponPos + ", Mdf:" + Moddif + "}";
                writer.WriteStringValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (TileConverter) Write(): " + ex.Message);
            }
        }
    }

}