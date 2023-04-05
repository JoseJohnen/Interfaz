using Interfaz.Models.Auxiliary;
using Interfaz.Models.Puppets;
using Interfaz.Utilities;
using System.Net.Sockets;
using System.Numerics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Interfaz.Models
{
    public class Player 
    {
        private Vector3 lstPosition = new Vector3();
        private Vector3 lstWeapon = new Vector3(-0.5f, 0f, 0f);
        private Vector3 lstLeftarm = new Vector3(0f, 0f, 0.5f);
        private Vector3 lstRightarm = new Vector3(0f, 0f, -0.5f);

        private Quaternion lstRotation = new Quaternion();
        
        private Vector3 position = new Vector3();
        private Vector3 weapon = new Vector3(0, 0, -0.1f);
        private Vector3 leftarm = new Vector3(-0.1f, 0, 0);
        private Vector3 rightarm = new Vector3(0.1f, 0, 0);
        private Quaternion rotation = Quaternion.Identity;

        public static Player plyr;

        public Vector3 Position
        {
            get => position; set
            {
                lstPosition = position;
                position = value;
            }
        }
        public Vector3 Weapon
        {
            get => weapon; set
            {
                lstWeapon = weapon;
                weapon = value;
            }
        }
        public Vector3 Leftarm
        {
            get => leftarm; set
            {
                lstLeftarm = leftarm;
                leftarm = value;
            }
        }
        public Vector3 Rightarm
        {
            get => rightarm; set
            {
                lstRightarm = rightarm;
                rightarm = value;
            }
        }
        public Quaternion Rotation
        {
            get => rotation; set
            {
                lstRotation = rotation;
                rotation = value;
            }
        }

        public float HP { get; set; } = 50;
        public float VelocityModifier { get; set; } = 1.5f;
        public bool IsFlyer { get; set; } = false;
        public float MPKillBox { get; set; } = 0.5f;

        public Vector3 LstPosition { get => lstPosition; }
        public Vector3 LstWeapon { get => lstWeapon; }
        public Vector3 LstLeftarm { get => lstLeftarm; }
        public Vector3 LstRightarm { get => lstRightarm; }
        public Quaternion LstRotation { get => lstRotation; }
        public string Name { get; set; } = "TODO: PlayerNameExtractedFromDBWhenLogin";

        public static string SetLoad(TcpClient tcpClient, string receivedRelevantInformation)
        {
            try
            {
                if(!receivedRelevantInformation.Contains("PST"))
                {
                    return String.Empty;
                }

                string specificRelevantInstruction = Interfaz.Utilities.UtilityAssistant.ValidateAndExtractInstructions(receivedRelevantInformation, "PST", out receivedRelevantInformation);
                receivedRelevantInformation = receivedRelevantInformation.Replace("PST:", "");
                if (String.IsNullOrWhiteSpace(specificRelevantInstruction))
                {
                    return String.Empty;
                }

                PlayerData pldt = JsonSerializer.Deserialize<PlayerData>(specificRelevantInstruction);

                string tempString = string.Empty;
                plyr = new Player
                {
                    Weapon = new Interfaz.Utilities.SerializedVector3(pldt.WP).ConvertToVector3(),
                    Leftarm = new Interfaz.Utilities.SerializedVector3(pldt.LS).ConvertToVector3(),
                    Rightarm = new Interfaz.Utilities.SerializedVector3(pldt.RS).ConvertToVector3(),
                    Position = new Interfaz.Utilities.SerializedVector3(pldt.PS).ConvertToVector3(),
                    Rotation = Interfaz.Utilities.UtilityAssistant.StringToQuaternion(pldt.RT)
                };

                #region Original SetLoad DataFlow
                //position = new SerializedVector3(UtilityAssistant.ExtractValues(receivedRelevantInformation, "PS")).ConvertToVector3();
                /*if (specificRelevantInstruction.Contains("WP"))
                {
                    tempString = UtilityAssistant.ExtractValues(specificRelevantInstruction, "WP");
                    specificRelevantInstruction = specificRelevantInstruction.Replace(("WP:" + tempString), "");
                    specificRelevantInstruction = specificRelevantInstruction.Trim();
                    if (!String.IsNullOrWhiteSpace(tempString))
                    {
                        plyr.Weapon = new SerializedVector3(tempString).ConvertToVector3();
                    }
                }

                if (specificRelevantInstruction.Contains("LS"))
                {
                    tempString = UtilityAssistant.ExtractValues(specificRelevantInstruction, "LS");
                    specificRelevantInstruction = specificRelevantInstruction.Replace(("LS:" + tempString), "");
                    specificRelevantInstruction = specificRelevantInstruction.Trim();
                    if (!String.IsNullOrWhiteSpace(tempString))
                    {
                        plyr.Leftarm = new SerializedVector3(tempString).ConvertToVector3();
                    }

                }

                if (specificRelevantInstruction.Contains("RS"))
                {
                    tempString = UtilityAssistant.ExtractValues(specificRelevantInstruction, "RS");
                    specificRelevantInstruction = specificRelevantInstruction.Replace(("RS:" + tempString), "");
                    specificRelevantInstruction = specificRelevantInstruction.Trim();
                    if (!String.IsNullOrWhiteSpace(tempString))
                    {
                        plyr.Rightarm = new SerializedVector3(tempString).ConvertToVector3();
                    }
                }

                if (specificRelevantInstruction.Contains("PS"))
                {
                    tempString = UtilityAssistant.ExtractValues(specificRelevantInstruction, "PS");
                    specificRelevantInstruction = specificRelevantInstruction.Replace(("PS:" + tempString), "");
                    specificRelevantInstruction = specificRelevantInstruction.Trim();
                    if (!String.IsNullOrWhiteSpace(tempString))
                    {
                        plyr.Position = new SerializedVector3(tempString).ConvertToVector3();
                    }
                }

                if (specificRelevantInstruction.Contains("RT"))
                {
                    tempString = UtilityAssistant.ExtractValues(specificRelevantInstruction, "RT");
                    specificRelevantInstruction = specificRelevantInstruction.Replace(("RT:" + tempString), "");
                    specificRelevantInstruction = specificRelevantInstruction.Trim();
                    if (!String.IsNullOrWhiteSpace(tempString))
                    {
                        //It shouldn't happend, but if happends, it means than was destined to the other RT, hence, we return the instruction rebuilded
                        if(tempString.Length <= 6)
                        {
                            return "RT:"+tempString;
                        }
                        plyr.Rotation = UtilityAssistant.StringToQuaternion(tempString);
                    }
                }*/
                #endregion

                //plyr.TcpClient = tcpClient;
                return receivedRelevantInformation;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: SetLoad(string): " + ex.Message);
                return string.Empty;
            }
        }

        public Player()
        {
            Weapon = new Vector3(-0.5f, 0f, 0f);
            Position = new Vector3(0f, 0f, 0f);
            Leftarm = new Vector3(0f, 0f, 0.5f);
            Rightarm = new Vector3(0f, 0f, -0.5f);
            Rotation = new Quaternion();
        }
        
        //3U + 2F + 5R
        //This return the modifier
        public Vector3 GetChildPosition(Vector3 childWorldPosition, float yaw, float pitch, float roll)
        {
            Matrix4x4 m = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll);
            Vector3 R = new Vector3(m.M11, m.M12, m.M13);
            Vector3 U = new Vector3(m.M21, m.M22, m.M23);
            Vector3 F = new Vector3(m.M31, m.M32, m.M33);

            Vector3 newWorldPositionOfTheChild = (childWorldPosition.X * R) + (childWorldPosition.Y * U) + (childWorldPosition.Z * F) + this.Position;
            return newWorldPositionOfTheChild;
        }


        //3U + 2F + 5R
        //This return the modifier
        public static Vector3 GetChildPosition(Vector3 childWorldPosition, Vector3 parentWorldPosition, float yaw, float pitch, float roll)
        {
            Matrix4x4 m = Matrix4x4.CreateFromYawPitchRoll(yaw,pitch,roll);
            Vector3 R = new Vector3(m.M11, m.M12, m.M13); 
            Vector3 U = new Vector3(m.M21, m.M22, m.M23); 
            Vector3 F = new Vector3(m.M31, m.M32, m.M33);

            Vector3 newWorldPositionOfTheChild = (childWorldPosition.X * R) + (childWorldPosition.Y * U) + (childWorldPosition.Z * F) + parentWorldPosition;
            return newWorldPositionOfTheChild;
        }

        public void PlayerPosUpdate(Vector3 modifier)
        {
            Position += modifier;
            Leftarm += modifier;
            Rightarm += modifier;
            Weapon += modifier;
        }

        public void PlayerRotUpdate(Vector3 eulerRot)
        {
            Quaternion result = Interfaz.Utilities.UtilityAssistant.ToQuaternion(eulerRot);
            Rotation = Interfaz.Utilities.UtilityAssistant.MultiplyQuaternions(Rotation, result);
        }

        public static float PrepareRotation(float angle)
        {
            //Determina la rotación a partir de un ángulo
            float num = angle * 0.5f;
            return (float)Math.Sin(num);
        }

        public static float PrepareRotationW(float angle)
        {
            //Determina la rotación a partir de un ángulo
            float num = angle * 0.5f;
            return (float)Math.Cos(num);
        }

        public Interfaz.Utilities.SerializedVector3 RotatePlayer(Quaternion quaternion)
        {
            Interfaz.Utilities.SerializedVector3 pos = new Interfaz.Utilities.SerializedVector3(Position);
            Interfaz.Utilities.SerializedVector3 result = quaternion * pos;
            return result;
        }

        public string ToJson()
        {
            try
            {
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new PlayerConverter()
                    }
                };
                string result = System.Text.Json.JsonSerializer.Serialize(this, serializeOptions);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Player) ToJson: " + ex.Message);
                return string.Empty;
            }
        }

        //SIN USO -Y NO FUNCIONA-: Hay que arreglarlo después cuando de verdad sea necesario
        //En cualquier caso tira pinta a que voy a tener que eventualmente rehacer y reordenar esta clase y la del lado del cliente
        //Incluida en esto la clase "PlayerData"
        public Player FromJson(string Text)
        {
            string txt = Text;
            try
            {
                txt = Interfaz.Utilities.UtilityAssistant.CleanJSON(txt.Replace("\u002B", "+"));
                PlayerData plDt = System.Text.Json.JsonSerializer.Deserialize<PlayerData>(txt);
                Player nwMsg = new Player();
                if (plDt != null)
                {
                    nwMsg.Weapon = new Interfaz.Utilities.SerializedVector3(plDt.WP).ConvertToVector3();
                    this.Weapon = nwMsg.Weapon;
                    nwMsg.Leftarm = new Interfaz.Utilities.SerializedVector3(plDt.LS).ConvertToVector3();
                    this.Leftarm = nwMsg.Leftarm;
                    nwMsg.Rightarm = new Interfaz.Utilities.SerializedVector3(plDt.RS).ConvertToVector3();
                    this.Rightarm = nwMsg.Rightarm;
                    nwMsg.Position = new Interfaz.Utilities.SerializedVector3(plDt.PS).ConvertToVector3();
                    this.Position = nwMsg.Position;
                    nwMsg.Rotation = Interfaz.Utilities.UtilityAssistant.StringToQuaternion(plDt.RT);
                    this.Rotation = nwMsg.Rotation;
                }
                return nwMsg;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Player) FromJson: " + ex.Message + " Text: " + txt);
                return new Player();
            }
        }

        public static Player CreateFromJson(string json)
        {
            try
            {
                Player msg = new Player();
                return msg.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Player) CreateFromJson: " + ex.Message);
                return new Player();
            }
        }

     
    }

    public class PlayerConverter : System.Text.Json.Serialization.JsonConverter<Player>
    {
        public override Player Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string strJson = string.Empty;
            try
            {
                //TODO: Corregir, testear y terminar
                JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
                strJson = jsonDoc.RootElement.GetRawText();

                Player shot = new Player();
                strJson = strJson.Replace("\"", "").Replace(":<", ":\"<").Replace(">}", ">\"}").Replace(".�M�", ">");
                string[] a = strJson.Replace("{", "").Replace("}", "").Split(",");//UtilityAssistant.CutJson(strJson);

                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (PlayerConverter) Read(): {0} Message: {1}", strJson, ex.Message);
                return new Player();
            }
        }

        public override void Write(Utf8JsonWriter writer, Player plyr, JsonSerializerOptions options)
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

                string LeftArm = System.Text.Json.JsonSerializer.Serialize(plyr.Leftarm, serializeOptions);
                string Rightarm = System.Text.Json.JsonSerializer.Serialize(plyr.Rightarm, serializeOptions);
                string Weapon = System.Text.Json.JsonSerializer.Serialize(plyr.Weapon, serializeOptions);
                string Position = System.Text.Json.JsonSerializer.Serialize(plyr.Position, serializeOptions);

                string HP = plyr.HP.ToString(); //"\"" + plyr.Id + "\"";
                string IsFlyer = plyr.IsFlyer ? "true" : "false";
                string MPKillBox = plyr.MPKillBox.ToString();
                string Name = string.IsNullOrWhiteSpace(plyr.Name) ? "null" : plyr.Name; //"\"" + plyr.Id + "\"";;

                char[] a = { '"' };

                string wr = @String.Concat("{ ", new string(a), "Name", new string(a), ":", new string(a), Name, new string(a),
                    ", ", new string(a), "MPKillBox", new string(a), ":", MPKillBox,
                    ", ", new string(a), "IsFlyer", new string(a), ":", IsFlyer,
                    ", ", new string(a), "HP", new string(a), ":", HP,
                    ", ", new string(a), "Position", new string(a), ":", new string(a), Position, new string(a),
                    ", ", new string(a), "Weapon", new string(a), ":", new string(a), Weapon, new string(a),
                    ", ", new string(a), "Rightarm", new string(a), ":", new string(a), Rightarm, new string(a),
                    ", ", new string(a), "LeftArm", new string(a), ":", new string(a), LeftArm, new string(a),
                    "}");

                string resultJson = Regex.Replace(wr, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");
                
                writer.WriteStringValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (PlayerConverter) Write(): " + ex.Message);
            }
        }
    }

}
