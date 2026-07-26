using Godot;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace RasForSts2.Scripts.Characters;

public class XilaCardPool : TypeListCardPoolModel
{
    public override string Title => "xila";
    public override string EnergyColorName => "xila";

    public override string? TextEnergyIconPath => "res://RasForSts2/images/xila_energy_icon.png";
    public override string? BigEnergyIconPath => "res://RasForSts2/images/energy_xila_big.png";

    public override Color DeckEntryCardColor => new(0.4f, 0.5f, 0.9f);
    public override Color EnergyOutlineColor => new(0.4f, 0.5f, 0.9f);

    private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateReplaceHueShaderMaterial(0.4f, 0.5f, 0.9f);
    public override Material? PoolFrameMaterial => _poolFrameMaterial;

    public override bool IsColorless => false;
}
