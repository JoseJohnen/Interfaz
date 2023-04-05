using System.Numerics;

namespace Interfaz.Models.Tiles
{
    public class Grass : Tile
    {
        public Grass(string name = "", Vector3 position = default, Vector3 inworldpos = default) : base(name, position, inworldpos)
        { 
        }

        public Grass() { }
    }
}
