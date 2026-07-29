using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Characters;

[RegisterSharedRelicPool]
public class GenericRelicPool : TypeListRelicPoolModel
{
    public override string EnergyColorName => "colorless";
}
