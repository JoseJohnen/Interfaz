using Interfaz.Models.Auxiliary;
using Interfaz.Models.Monsters;
using System.Numerics;
using Interfaz.Utilities;

namespace Interfaz.Models.Puppets
{
    public class Imp : Puppet
    {
        public override IA_Instructions IA_Instructions { get; set; } = new IA_Instructions()
        {
            evaluadores = new List<string>()
            {
                "{\"type_AI_Message\":0,\"Value1\":15,\"Value2\":0}", //Si bajo o igual 15 de distancia, disparar
                "{\"type_AI_Message\":1,\"Value1\":15,\"Value2\":0}", //Si sobre 15 de distancia, moverse
                "{\"type_AI_Message\":2,\"Value1\":7,\"Value2\":1}" //Si la distancia esta entre 7 y 1, retroceder y disparar
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
                            //ATAQUE DISTANCIA
                            result.Add("MA:"+ShootingTo(targetPosition));
                            break;
                        case "1":
                            msUpd.Id = Name;
                            msUpd.Pos = MoveTo(targetPosition);
                            result.Add("MU:" + msUpd.ToJson());
                            break;
                        case "2":
                            //Obtener el punto opuesto del target para alejarse de él
                            msUpd.Id = Name;
                            msUpd.Pos = GetAwayFrom(targetPosition);
                            result.Add("MU:" + msUpd.ToJson());
                            string rslt = UtilityAssistant.ExtractAIInstructionData(IA_Instructions.evaluadores[2], "Value2");
                            if (!string.IsNullOrWhiteSpace(rslt))
                            {
                                int value2 = 0;
                                if (Int32.TryParse(rslt, out value2))
                                {
                                    if (this.DetectEntityInRange(targetPosition, value2))
                                    {
                                        result.Add("MA:" + ShootingTo(targetPosition));
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Imp) RunIAServer(string, Vector3): " + ex.Message);
                return new List<string>();
            }
        }
    }
}
