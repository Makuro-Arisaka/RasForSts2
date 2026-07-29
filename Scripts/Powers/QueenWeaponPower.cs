using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

[RegisterPower]
public sealed class QueenWeaponPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://RasForSts2/images/powers/QueenWeaponPower.png",
        BigIconPath: "res://RasForSts2/images/powers/QueenWeaponPower.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();
}
