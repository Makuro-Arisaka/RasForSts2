using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using RasForSts2.Scripts.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace RasForSts2.Scripts.Powers;

[RegisterPower]
public class ShadowWavePower : TemporaryStrengthPower, IModPowerAssetOverrides
{
    public override AbstractModel OriginModel => ModelDb.Card<ShadowWave>();

    protected override bool IsPositive => false;

    public PowerAssetProfile AssetProfile => new(
        IconPath: "res://RasForSts2/images/powers/ShadowWavePower.png",
        BigIconPath: "res://RasForSts2/images/powers/ShadowWavePower.png"
    );

    public string? CustomIconPath => AssetProfile.IconPath;

    public string? CustomBigIconPath => AssetProfile.BigIconPath;
}
