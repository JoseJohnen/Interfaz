using Interfaz.Models.Auxiliary;
using Interfaz.Models.Monsters;
using System.Numerics;

namespace Interfaz.Models.Puppets
{
    public class Pinkie : Puppet
    {
        public override IA_Instructions IA_Instructions { get; set; } = new IA_Instructions()
        {
            evaluadores = new List<string>()
             {
                "{\"type_AI_Message\":1,\"Value1\":2,\"Value2\":0}",  //Si sobre 2 de distancia, moverse
                "{\"type_AI_Message\":0,\"Value1\":2,\"Value2\":0}" //Si bajo o igual 2 de distancia, golpear
             }
        };

        public override List<string> RunIAServer(string instruciones, Vector3 targetPosition)
        {
            try
            {
                List<string> result = new();
                MonsterPosUpdate msUpd = new MonsterPosUpdate();
                foreach (char item in instruciones.ToList())
                {
                    switch (item.ToString())
                    {
                        case "0":
                            msUpd.Id = this.Name;
                            msUpd.Pos = MoveTo(targetPosition);
                            result.Add("MU:"+msUpd.ToJson());
                            break;
                        case "1":
                            //TODO: ATAQUE MELEE;
                            result.Add("MA:"+MeleeTo(targetPosition));
                            break;
                        default:
                            break;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Pinkie) RunIAServer(string, Vector3): " + ex.Message);
                return new List<string>();
            }
        }
    }
}
