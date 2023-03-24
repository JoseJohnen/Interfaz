using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Text.Json;

namespace Interfaz.Models
{
    public abstract partial class Puppet
    {
        public virtual float HP { get; set; }
        public virtual float VelocityModifier { get; set; }
        public virtual float MPKillBox { get; set; }
        public virtual string Name { get; set; }
        public virtual bool IsFlyer { get; set; }
        public virtual Vector3 Position { get; set; }
        //public virtual AnimacionSprite AnimSprite { get; set; }
        public virtual Vector3 Sprite { get; set; }
        public virtual Vector3 Body { get; set; }
        public virtual Vector3 Weapon { get; set; }
        public virtual Vector3 Leftarm { get; set; }
        public virtual Vector3 Rightarm { get; set; }
        public virtual Quaternion Rotation { get; set; }

        public virtual TcpClient TcpClient { get; set; } = null;


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

        public virtual string ToJson()
        {
            try
            {
                string strResult = JsonSerializer.Serialize(this);
                return strResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) String ToJson(): " + ex.Message);
                return string.Empty;
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

        public abstract void RunIA();
    }

}
