using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

[RegisterPower]
public sealed class QueenHarpPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(IconPath: "res://RasForSts2/images/powers/QueenHarpPower.png", BigIconPath: "res://RasForSts2/images/powers/QueenHarpPower.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await base.AfterPlayerTurnStart(choiceContext, player);

        if (player != Owner.Player)
        {
            return;
        }

        decimal stacks = Amount;

        foreach (Creature enemy in Owner.CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, stacks, Owner, null);
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, stacks, Owner, null);
        }

        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, stacks, Owner, null);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, stacks, Owner, null);
    }
}