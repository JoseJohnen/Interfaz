using Interfaz.Models.Tiles;
using Interfaz.Models.Worlds;

namespace Interfaz.Models
{
    public class BaseWorld : World
    {
        public BaseWorld() { 
        }

        public override World FillWorld()
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

        public override BaseWorld RegisterWorld(string nameOfTheWorld = "")
        {
            try
            {
                return this;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RegisterWorld(string): " + ex.Message);
                return null;
            }
        }
    }

}