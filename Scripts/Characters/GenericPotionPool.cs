using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Characters;

[RegisterSharedPotionPool]
public class GenericPotionPool : TypeListPotionPoolModel
{
    public override string EnergyColorName => "colorless";
}
