using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Characters;

public class XilaPotionPool : TypeListPotionPoolModel
{
    public override string? TextEnergyIconPath => "res://RasForSts2/images/energy_xila.png";
    public override string? BigEnergyIconPath => "res://RasForSts2/images/energy_xila_big.png";

    public override string EnergyColorName => "xila";
}
