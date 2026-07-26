using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using RasForSts2.Scripts.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

[RegisterPower]
public class ShadowWavePower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<ShadowWave>();

    protected override bool IsPositive => false;
}
