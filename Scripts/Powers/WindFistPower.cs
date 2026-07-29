using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using RasForSts2.Scripts.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace RasForSts2.Scripts.Powers;

/// <summary>
/// 疾风连拳的临时力量 Power。
/// 继承自 TemporaryStrengthPower，回合结束时自动失去对应层数的力量。
/// </summary>
[RegisterPower]
public class WindFistPower : TemporaryStrengthPower, IModPowerAssetOverrides
{
    public override AbstractModel OriginModel => ModelDb.Card<WindFist>();

    protected override bool IsPositive => true;

    public PowerAssetProfile AssetProfile => new(
        IconPath: "res://RasForSts2/images/powers/WindFistPower.png",
        BigIconPath: "res://RasForSts2/images/powers/WindFistPower.png"
    );

    public string? CustomIconPath => AssetProfile.IconPath;

    public string? CustomBigIconPath => AssetProfile.BigIconPath;
}
