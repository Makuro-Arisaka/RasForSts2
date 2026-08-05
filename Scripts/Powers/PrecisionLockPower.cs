using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

/// <summary>
/// 精准锁定
/// 下回合打出的第1张攻击牌伤害 +50%(75%)，打出攻击牌后移除。
/// Amount 存提升百分比（50/75）。
/// </summary>
[RegisterPower]
public sealed class PrecisionLockPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://RasForSts2/images/powers/placeholder.png",
        BigIconPath: "res://RasForSts2/images/powers/placeholder.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || cardPlay.Card.Type != CardType.Attack)
        {
            return;
        }

        Log.Info($"[PrecisionLock] Next attack card played: {cardPlay.Card.Id.Entry}, removing self.");
        await PowerCmd.Remove(this);
    }
}

[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.ModifyDamageMultiplicative))]
public static class PrecisionLockDamagePatch
{
    public static void Postfix(AbstractModel __instance, ref decimal __result, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (__instance is not PrecisionLockPower power)
        {
            return;
        }

        if (dealer != power.Owner && !power.Owner.Pets.Contains<Creature>(dealer))
        {
            Log.Debug($"[PrecisionLock] Dealer not Owner, skip. dealer={dealer?.Name} Owner={power.Owner?.Name}");
            return;
        }
        if (!props.IsPoweredAttack())
        {
            Log.Debug("[PrecisionLock] Not a powered attack, skip.");
            return;
        }
        if (cardSource == null || cardSource.Type != CardType.Attack)
        {
            Log.Debug("[PrecisionLock] cardSource is null or not Attack, skip.");
            return;
        }

        decimal before = __result;
        __result *= 1m + power.Amount / 100m;
        Log.Info($"[PrecisionLock] Boosted damage! Card={cardSource.Id.Entry} +{power.Amount}% : {before} -> {__result}");
    }
}
