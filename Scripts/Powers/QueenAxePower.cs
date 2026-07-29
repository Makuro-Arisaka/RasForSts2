using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

[RegisterPower]
public sealed class QueenAxePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(IconPath: "res://RasForSts2/images/powers/QueenAxePower.png", BigIconPath: "res://RasForSts2/images/powers/QueenAxePower.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        CombatManager.Instance.TurnEnded += OnTurnEnded;
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
        CombatManager.Instance.TurnEnded -= OnTurnEnded;
    }

    /// <summary>
    /// 文档要求"在本回合内你造成的伤害翻倍"，因此玩家回合结束时移除自身。
    /// 参考 GuardPower 的事件订阅方式。
    /// </summary>
    private async void OnTurnEnded(CombatState state)
    {
        // 防止旧实例（前一场战斗残留）触发
        Creature? owner = base.Owner;
        if (owner == null || owner.GetPower<QueenAxePower>() != this)
        {
            CombatManager.Instance.TurnEnded -= OnTurnEnded;
            return;
        }

        if (state.CurrentSide == CombatSide.Player)
        {
            Flash();
            await PowerCmd.Remove(this);
        }
    }
}

/// <summary>
/// 伤害翻倍逻辑参考月光大剑（MoonlightGreatswordPower）的 Harmony Patch 实现。
/// 仅翻倍玩家作为攻击者造成的攻击伤害（dealer == power.Owner）。
/// </summary>
[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.ModifyDamageMultiplicative))]
public static class QueenAxeDamagePatch
{
    public static void Postfix(AbstractModel __instance, ref decimal __result, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (__instance is not QueenAxePower power)
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
