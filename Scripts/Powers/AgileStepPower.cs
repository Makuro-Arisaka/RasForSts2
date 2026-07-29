using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

[RegisterPower]
public sealed class AgileStepPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(IconPath: "res://RasForSts2/images/powers/AgileStepPower.png", BigIconPath: "res://RasForSts2/images/powers/AgileStepPower.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), Owner, 4m, applier, cardSource);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            if (Amount > 1)
            {
                await PowerCmd.Apply<AgileStepPower>(choiceContext, Owner, -1m, Owner, null);
            }
            else
            {
                await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, -4m, Owner, null);
                await PowerCmd.Remove(this);
            }
        }
    }
}
