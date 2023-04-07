using Interfaz.Models.Puppets;

namespace Interfaz.Models.Worlds
{
    public class BaseWorld : World
    {
        public BaseWorld() {
            dic_SpawnList.Add(new Imp(), 1);
            dic_SpawnList.Add(new Pinkie(), 1);
        }
    }

}