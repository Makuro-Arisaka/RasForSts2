using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

[RegisterPower]
public sealed class FarrahHellAshPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://RasForSts2/images/powers/FarrahHellAshPower.png",
        BigIconPath: $"res://RasForSts2/images/powers/FarrahHellAshPower.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Models.CardModel card, bool _)
    {
        if (card.Owner.Creature == base.Owner)
        {
            await PowerCmd.Apply<GuardPower>(null, base.Owner, base.Amount, base.Owner, null);
        }
    }
}
