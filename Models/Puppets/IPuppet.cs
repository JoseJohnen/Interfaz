using Interfaz.Models.Auxiliary;
using System.Numerics;

namespace Interfaz.Models.Puppets
{
    public interface IPuppet
    {
        float HP { get; set; }
        IA_Instructions IA_Instructions { get; set; }
        bool IsFlyer { get; set; }
        float MPKillBox { get; set; }
        string Name { get; set; }
        Vector3 Position { get; set; }
        float VelocityModifier { get; set; }

        bool DetectEntityInRange(Vector3 positionEntity, float DetectionArea = 15);
        Puppet FromJson(string Text);
        Vector3 GetAwayFrom(Vector3 target, bool transition = false);
        string MeleeTo(Vector3 target);
        Vector3 MoveTo(Vector3 targetPosition, bool transition = false);
        List<string> RunIAServer(string instruciones, Vector3 target);
        string ShootingTo(Vector3 target);
        string ToJson();
    }
}