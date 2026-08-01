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
/// 钢牙狼一闪之力
/// 下一张攻击卡造成三倍伤害，打出（除来源卡自身外的）攻击卡后移除
/// </summary>
[RegisterPower]
public sealed class SteelFangWolfFlashPower : ModPowerTemplate
{
    private class Data
    {
        /// <summary>施加这个 buff 的那张来源卡（钢牙狼一闪自身）的 Entry，用于在 AfterCardPlayed 中跳过自身</summary>
        public string? AppliedByCardEntry;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://RasForSts2/images/powers/SteelFangWolfFlashPower.png",
        BigIconPath: "res://RasForSts2/images/powers/SteelFangWolfFlashPower.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override object InitInternalData() => new Data();

    public override Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (cardSource != null)
        {
            GetInternalData<Data>().AppliedByCardEntry = cardSource.Id.Entry;
            Log.Info($"[SteelFangWolfFlash] Power applied by card: {cardSource.Id.Entry}, will skip removal for this card.");
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || cardPlay.Card.Type != CardType.Attack)
            return;

        string entry = cardPlay.Card.Id.Entry;
        string? appliedBy = GetInternalData<Data>().AppliedByCardEntry;

        // 跳过施加 buff 自身的那张卡（钢牙狼一闪自己），等"下一张攻击卡"时再生效并移除
        if (appliedBy != null && entry == appliedBy)
        {
            Log.Info($"[SteelFangWolfFlash] Skipped self-card: {entry}, buff remains for next attack.");
            return;
        }

        Log.Info($"[SteelFangWolfFlash] Next attack card played: {entry}, removing self.");
        await PowerCmd.Remove(this);
    }
}

[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.ModifyDamageMultiplicative))]
public static class SteelFangWolfFlashDamagePatch
{
    public static void Postfix(AbstractModel __instance, ref decimal __result, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (__instance is not SteelFangWolfFlashPower power)
        {
            return;
        }

        if (dealer != power.Owner && !power.Owner.Pets.Contains<Creature>(dealer))
        {
            Log.Debug($"[SteelFangWolfFlash] Dealer not Owner, skip. dealer={dealer?.Name} Owner={power.Owner?.Name}");
            return;
        }
        if (!props.IsPoweredAttack())
        {
            Log.Debug($"[SteelFangWolfFlash] Not a powered attack, skip.");
            return;
        }
        if (cardSource == null)
        {
            Log.Debug($"[SteelFangWolfFlash] cardSource is null, skip.");
            return;
        }
        if (cardSource.Type != CardType.Attack)
        {
            Log.Debug($"[SteelFangWolfFlash] cardSource Type={cardSource.Type}, not Attack, skip.");
            return;
        }

        decimal before = __result;
        __result *= 3m;
        Log.Info($"[SteelFangWolfFlash] Triple damage! Dealer={dealer?.Name} Target={target?.Name} Card={cardSource?.Id.Entry} Multiplier: {before} -> {__result}");
    }
}
