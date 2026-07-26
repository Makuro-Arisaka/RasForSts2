using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

[RegisterPower]
public sealed class MixedBombPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(IconPath: "res://RasForSts2/images/powers/MixedBombPower.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            foreach (Creature hittableEnemy in Owner.CombatState.HittableEnemies)
            {
                await CreatureCmd.Damage(choiceContext, hittableEnemy, Amount, ValueProp.Unpowered, Owner);
            }
        }
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        MixedBombTracker.TrackPower(this);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
        MixedBombTracker.UntrackPower(this);
    }
}

public static class MixedBombTracker
{
    private static readonly HashSet<MixedBombPower> _activePowers = new();

    public static void TrackPower(MixedBombPower power)
    {
        _activePowers.Add(power);
        EnsureHookInstalled();
    }

    public static void UntrackPower(MixedBombPower power)
    {
        _activePowers.Remove(power);
    }

    private static bool _hookInstalled = false;

    private static void EnsureHookInstalled()
    {
        if (_hookInstalled) return;
        _hookInstalled = true;

        var harmony = new Harmony("RasForSts2.MixedBomb");
        harmony.Patch(
            original: AccessTools.Method(typeof(Hook), nameof(Hook.AfterPowerAmountChanged)),
            postfix: new HarmonyMethod(typeof(MixedBombTracker), nameof(OnPowerAmountChanged))
        );
    }

    public static async void OnPowerAmountChanged(CombatState combatState, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (amount == 0m) return;
        if (power.GetTypeForAmount(amount) != PowerType.Debuff) return;
        if (!power.Owner.IsEnemy) return;
        if (applier == null) return;
        if (power is ITemporaryPower) return;

        foreach (var bombPower in _activePowers)
        {
            if (bombPower.Owner != applier) continue;

            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), power.Owner, bombPower.Amount, ValueProp.Unpowered, applier);
        }
    }
}
