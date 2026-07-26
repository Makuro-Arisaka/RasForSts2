using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

/// <summary>
/// 邪咒再显：获得女王武具的所有正面效果（无负面）
/// 1. 你打出的攻击牌造成的伤害翻倍
/// 2. 你打出的技能牌获得的格挡翻倍
/// 3. 所有敌人身上的虚弱和易伤的效果翻倍
/// 4. 你每打出1张牌时,抽1(2)张牌
/// </summary>
[RegisterPower]
public sealed class CurseRevealPower : ModPowerTemplate
{
    private class Data
    {
        public int drawAmount;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(IconPath: "res://RasForSts2/images/powers/CurseRevealPower.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        // spec: 抽1(2)张牌（升级后抽2）
        GetInternalData<Data>().drawAmount = cardSource != null && cardSource.IsUpgraded ? 2 : 1;
    }

    // spec: 你每打出1张牌时,抽1(2)张牌
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.AfterCardPlayed(choiceContext, cardPlay);

        if (cardPlay.Card.Owner.Creature == base.Owner)
        {
            int drawAmount = GetInternalData<Data>().drawAmount;
            await CardPileCmd.Draw(choiceContext, drawAmount, base.Owner.Player);
        }
    }
}

/// <summary>
/// 邪咒再显: 你打出的攻击牌造成的伤害翻倍
/// </summary>
[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.ModifyDamageMultiplicative))]
public static class CurseRevealAttackDamagePatch
{
    public static void Postfix(AbstractModel __instance, ref decimal __result, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (__instance is not CurseRevealPower power)
        {
            return;
        }

        if (dealer != power.Owner)
        {
            return;
        }

        if (!props.IsPoweredAttack())
        {
            return;
        }

        __result *= 2m;
    }
}

/// <summary>
/// 邪咒再显: 你打出的技能牌获得的格挡翻倍
/// </summary>
[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.ModifyBlockMultiplicative))]
public static class CurseRevealSkillBlockPatch
{
    public static void Postfix(AbstractModel __instance, ref decimal __result, Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (__instance is not CurseRevealPower power)
        {
            return;
        }

        if (power.Owner != target)
        {
            return;
        }

        if (!props.IsPoweredCardOrMonsterMoveBlock())
        {
            return;
        }

        // 限定: 只对玩家打出的技能牌生效 (spec: "你打出的技能牌获得的格挡翻倍")
        // monster move 没有 cardSource, 自动被过滤
        if (cardSource == null || cardSource.Type != CardType.Skill)
        {
            return;
        }

        __result *= 2m;
    }
}

/// <summary>
/// 邪咒再显: 所有敌人身上的虚弱效果翻倍
/// WeakPower 默认使造成伤害 ×0.75 (减伤25%); 翻倍后 ×0.5 (减伤50%)
/// 公式: new = 2 * old - 1
/// 仅当玩家(拥有邪咒再显)被敌人(带有虚弱)攻击时生效
/// </summary>
[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.ModifyDamageMultiplicative))]
public static class CurseRevealWeakPatch
{
    public static void Postfix(AbstractModel __instance, ref decimal __result, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (__instance is not WeakPower weakPower)
        {
            return;
        }

        if (!props.IsPoweredAttack())
        {
            return;
        }

        // WeakPower 仅在 dealer == Owner 时返回非 1.0 的乘数
        if (dealer != weakPower.Owner)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        // 只有玩家拥有 CurseRevealPower 时, 敌人身上的虚弱才翻倍
        if (target.GetPower<CurseRevealPower>() == null)
        {
            return;
        }

        // 翻倍虚弱效果: 0.75 -> 0.5
        __result = 2m * __result - 1m;
    }
}

/// <summary>
/// 邪咒再显: 所有敌人身上的易伤效果翻倍
/// VulnerablePower 默认使受到伤害 ×1.5 (增伤50%); 翻倍后 ×2.0 (增伤100%)
/// 公式: new = 2 * old - 1
/// 仅当玩家(拥有邪咒再显)攻击敌人(带有易伤)时生效
/// </summary>
[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.ModifyDamageMultiplicative))]
public static class CurseRevealVulnerablePatch
{
    public static void Postfix(AbstractModel __instance, ref decimal __result, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (__instance is not VulnerablePower vulnerablePower)
        {
            return;
        }

        if (!props.IsPoweredAttack())
        {
            return;
        }

        // VulnerablePower 仅在 target == Owner 时返回非 1.0 的乘数
        if (target != vulnerablePower.Owner)
        {
            return;
        }

        if (dealer == null)
        {
            return;
        }

        // 只有玩家拥有 CurseRevealPower 时, 敌人身上的易伤才翻倍
        if (dealer.GetPower<CurseRevealPower>() == null)
        {
            return;
        }

        // 翻倍易伤效果: 1.5 -> 2.0
        __result = 2m * __result - 1m;
    }
}
