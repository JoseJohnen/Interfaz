using Interfaz.Models.Auxiliary;
using Interfaz.Models.Monsters;
using Interfaz.Models.Shots;
using System.Collections.Concurrent;
using System.Numerics;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Interfaz.Utilities;

namespace Interfaz.Models.Puppets
{
    public abstract class Puppet
    {
        private Vector3 position;

        public virtual float HP { get; set; }
        public virtual float VelocityModifier { get; set; }
        public virtual float MPKillBox { get; set; }
        public virtual string Name { get; set; }
        public virtual bool IsFlyer { get; set; }
        public virtual Vector3 Position { get => position; set => position = value; }
        //public virtual AnimacionSprite AnimSprite { get; set; }

        //Pensandolo bien, estos datos son redundantes en esta versión del Puppet porque los cálculos se hacen a partir del medio, no del puppet en sí
        //y en el peor de los casos son calculables desde "position" por el lado del cliente
        /*public virtual Vector3 Sprite { get; set; }
        public virtual Vector3 Body { get; set; }
        public virtual Vector3 Weapon { get; set; }
        public virtual Vector3 Leftarm { get; set; }
        public virtual Vector3 Rightarm { get; set; }
        public virtual Quaternion Rotation { get; set; }*/

        //public virtual TcpClient TcpClient { get; set; } = null;

        public virtual IA_Instructions IA_Instructions { get; set; }

        public static ConcurrentQueue<Shot> q_newShots { get; set; } = new ConcurrentQueue<Shot>();

        #region Constructores
        //TODO: Did puppets have Modes? or Should? Como para atacar/idle/escapar y definirlo por objeto, asignar un valor que defina comportamiento??.
        protected Puppet(Vector3 position, float hp = 10, float velocityModifier = 0.05f, float mpKillBox = 0.08f, bool isFlyer = true)
        {
            Position = position;
            HP = hp;
            VelocityModifier = velocityModifier;
            IsFlyer = isFlyer;
            MPKillBox = mpKillBox;
        }

        protected Puppet(float hp = 10, float velocityModifier = 0.05f, float mpKillBox = 0.08f, bool isFlyer = true)
        {
            HP = hp;
            VelocityModifier = velocityModifier;
            IsFlyer = isFlyer;
            MPKillBox = mpKillBox;
        }

        protected Puppet()
        {

        }

        public static Puppet CreatePuppetFromClassName(string ClassName, Vector3 Pos = new Vector3())
        {
            try
            {
                Type typ = Puppet.TypesOfMonsters().Where(c => c.Name == ClassName).FirstOrDefault();
                if (typ == null)
                {
                    typ = Puppet.TypesOfMonsters().Where(c => c.FullName == ClassName).FirstOrDefault();
                }

                object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.
                                                                  //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list

                Puppet prgObj = ((Puppet)obtOfType);
                if(Pos != new Vector3())
                {
                    prgObj.position = Pos;
                }
                return prgObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (Puppet)  CreatePuppetFromClassName(string, Vector3): "+ ex.Message);
                return default;
            }
        }
        #endregion

        #region Auxiliares
        public virtual string ToJson()
        {
            try
            {
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new PuppetConverter(),
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

        public virtual Puppet FromJson(string Text)
        {
            string txt = Text;
            try
            {
                txt = UtilityAssistant.CleanJSON(txt.Replace("\u002B", "+"));

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new PuppetConverter(),
                    }
                };

                Puppet strResult = JsonSerializer.Deserialize<Puppet>(txt, serializeOptions);

                //TODO: VER QUE EL OBJETO AL HACER TO JSON SALVE EL NOMBRE DE LA CLASE TAMBIÉN
                //TODO2: RECUERDA QUE DEBES EXTRAER EL OBJETO

                if (strResult != null)
                {
                    this.Name = strResult.Name;
                    this.Position = strResult.Position;
                }
                return strResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) FromJson: " + ex.Message + " Text: " + txt);
                return null;
            }
        }

        public static Puppet CreateFromJson(string json)
        {
            try
            {
                string clase = UtilityAssistant.CleanJSON(json);
                clase = UtilityAssistant.ExtractAIInstructionData(clase, "Class").Replace("\"", "");

                Type typ = Puppet.TypesOfMonsters().Where(c => c.Name == clase).FirstOrDefault();
                if (typ == null)
                {
                    typ = Puppet.TypesOfMonsters().Where(c => c.FullName == clase).FirstOrDefault();
                }

                object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.
                                                                  //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list

                Puppet prgObj = ((Puppet)obtOfType);
                return prgObj.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) CreateFromJson(): " + ex.Message);
                return null;
            }
        }

        public static List<Type> TypesOfMonsters()
        {
            List<Type> myTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(Puppet)) && !type.IsAbstract).ToList();
            return myTypes;
        }

        protected virtual void Power()
        {
            return;
        }
        #endregion

        #region AI Methods
        public virtual string ShootingTo(Vector3 target)
        {
            try
            {
                Vector3 mod = new Vector3(Position.X, Position.Y, Position.Z);
                //Si el target esta a la derecha de posición
                if (target.X > Position.X)
                {
                    mod.X += 0.5f;
                }
                else if (target.X < Position.X)
                {
                    mod.X -= 0.5f;
                }

                //Si el target esta al norte de posición
                if (target.Y > Position.Y)
                {
                    mod.Y += 0.5f;
                }
                else if (target.Y < Position.Y)
                {
                    mod.Y -= 0.5f;
                }

                //Si el target esta al centro de posición
                if (target.Z > Position.Z)
                {
                    mod.Z += 0.5f;
                }
                else if (target.Y < Position.Y)
                {
                    mod.Z -= 0.5f;
                }

                Shot shot = new Shot();
                shot.LN = this.Name;
                shot.Type = "NB";
                shot.OrPos = this.Position;
                shot.WPos = mod;
                shot.Mdf = mod;

                q_newShots.Enqueue(shot);

                MonsterState msUpdate = new MonsterState();
                msUpdate.Id = this.Name;
                msUpdate.State = StateOfTheMonster.Attacking;
                return msUpdate.ToJson();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) ShootingTo(Vector3): " + ex.Message);
                return string.Empty;
            }
        }

        public virtual string MeleeTo(Vector3 target)
        {
            try
            {
                Vector3 mod = new Vector3(Position.X, Position.Y, Position.Z);
                //Si el target esta a la derecha de posición
                if (target.X > Position.X)
                {
                    mod.X += 1.5f;
                }
                else if (target.X < Position.X)
                {
                    mod.X -= 1.5f;
                }

                //Si el target esta al norte de posición
                if (target.Y > Position.Y)
                {
                    mod.Y += 1.5f;
                }
                else if (target.Y < Position.Y)
                {
                    mod.Y -= 1.5f;
                }

                //Si el target esta al centro de posición
                if (target.Z > Position.Z)
                {
                    mod.Z += 1.5f;
                }
                else if (target.Y < Position.Y)
                {
                    mod.Z -= 1.5f;
                }

                if (this.DetectEntityInRange(target, 1.5f))
                {
                    //TODO: Transfer damage
                }

                MonsterState msStt = new();
                msStt.Id = this.Name;
                msStt.State = StateOfTheMonster.Attacking;
                return msStt.ToJson();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) MeleeTo(Vector3): " + ex.Message);
                return string.Empty;
            }
        }

        public virtual bool DetectEntityInRange(Vector3 positionEntity, float DetectionArea = 15)
        {
            try
            {
                float a = UtilityAssistant.DistanceComparitorByAxis(Position.X, positionEntity.X);
                float b = UtilityAssistant.DistanceComparitorByAxis(Position.Y, positionEntity.Y);
                float c = UtilityAssistant.DistanceComparitorByAxis(Position.Z, positionEntity.Z);

                if (
                    a < DetectionArea / 2 &&
                    b < DetectionArea / 2 &&
                    c < DetectionArea / 2
                    )
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) DetectEntityInRange(Vector3, float): " + ex.Message);
                return false;
            }
        }

        //TODO: TEST AND FINISH
        public virtual Vector3 GetAwayFrom(Vector3 target, bool transition = false)
        {
            try
            {
                Vector3 modiff = new Vector3(Position.X, Position.Y, Position.Z);
                //Si el target esta a la derecha de posición
                if (target.X > Position.X)
                {
                    modiff.X -= VelocityModifier;
                }
                else if (target.X < Position.X)
                {
                    modiff.X += VelocityModifier;
                }

                //Only if fly
                if (this.IsFlyer)
                {
                    //Si el target esta al norte de posición
                    if (target.Y > Position.Y)
                    {
                        modiff.Y -= VelocityModifier;
                    }
                    else if (target.Y < Position.Y)
                    {
                        modiff.Y += VelocityModifier;
                    }
                }

                //Si el target esta al centro de posición
                if (target.Z > Position.Z)
                {
                    modiff.Z -= VelocityModifier;
                }
                else if (target.Y < Position.Y)
                {
                    modiff.Z += VelocityModifier;
                }

                if (!transition)
                {
                    Position = modiff;
                    return Position;
                }

                return Position;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) MoveTo(Vector3, bool): " + ex.Message);
                return Position;
            }

        }

        public virtual Vector3 MoveTo(Vector3 targetPosition, bool transition = false)
        {
            try
            {
                if (!transition)
                {
                    Position = targetPosition;
                    return Position;
                }

                Vector3 a = Vector3.Zero;
                if (Position.X < targetPosition.X)//Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.X)
                {
                    a.X = this.VelocityModifier;

                }
                else if (Position.X > targetPosition.X)//Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.X)
                {
                    a.X = this.VelocityModifier * -1;
                }

                //Only if fly
                if (this.IsFlyer)
                {
                    if (Position.Y < targetPosition.Y)//Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Y)
                    {
                        a.Y = this.VelocityModifier;
                    }
                    else if (Position.Y > targetPosition.Y)//Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Y)
                    {
                        a.Y = this.VelocityModifier * -1;
                    }
                }

                if (Position.Z < targetPosition.Z)//Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Z)
                {
                    a.Z = this.VelocityModifier;
                }
                else if (Position.Z > targetPosition.Z)//Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Z)
                {
                    a.Z = this.VelocityModifier * -1;
                }

                Position += a;
                return Position;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) MoveTo(Vector3, bool): " + ex.Message);
                return Position;
            }

        }

        public abstract List<string> RunIAServer(string instruciones, Vector3 target);
        #endregion
    }

    public class PuppetConverter : System.Text.Json.Serialization.JsonConverter<Puppet>
    {
        public override Puppet Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

                Type typ = Puppet.TypesOfMonsters().Where(c => c.Name == clase).FirstOrDefault();
                if (typ == null)
                {
                    typ = Puppet.TypesOfMonsters().Where(c => c.FullName == clase).FirstOrDefault();
                }

                object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.
                                                                  //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list

                Puppet prgObj = ((Puppet)obtOfType);

                string pst = UtilityAssistant.ExtractValue(strJson, "Position");
                prgObj.Position = UtilityAssistant.Vector3Deserializer(pst);
                prgObj.Name = UtilityAssistant.ExtractValue(strJson, "Name");
                
                return prgObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (PuppetConverter) Read(): {0} Message: {1}", strJson, ex.Message);
                return default;
            }
        }

        public override void Write(Utf8JsonWriter writer, Puppet ppt, JsonSerializerOptions options)
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


                //Para deserealizar los vector3 serializados: UtilityAssistant.Vector3Deserializer(ppt);

                //TODO: Corregir, testear y terminar
                string Name = string.IsNullOrWhiteSpace(ppt.Name) ? "null" : ppt.Name;
                string Position = System.Text.Json.JsonSerializer.Serialize(ppt.Position, serializeOptions);
                string Class = ppt.GetType().Name;

                char[] a = { '"' };
                
                string wr = string.Concat("{ ", new string(a), "Name", new string(a), ":", new string(a), Name, new string(a),
                    ", ", new string(a), "Class", new string(a), ":", new string(a), Class, new string(a),
                    ", ", new string(a), "Position", new string(a), ":", Position,
                    "}");

                string resultJson = Regex.Replace(wr, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");
                //string resultJson = "{Id:" + Id + ", LN:" + LauncherName + ", Type:" + Type + ", OrPos:" + LauncherPos + ", WPos:" + WeaponPos + ", Mdf:" + Moddif + "}";
                writer.WriteStringValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (PuppetConverter) Write(): " + ex.Message);
            }
        }
    }

}
