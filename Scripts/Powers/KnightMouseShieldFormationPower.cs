using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

/// <summary>
/// 骑士鼠的盾阵
/// 格挡不在回合开始时消失。护卫层数清空时此效果失效。
/// </summary>
[RegisterPower]
public sealed class KnightMouseShieldFormationPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://RasForSts2/images/powers/KnightMouseShieldFormationPower.png",
        BigIconPath: "res://RasForSts2/images/powers/KnightMouseShieldFormationPower.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    /// <summary>
    /// 阻止格挡在回合开始时被清空（仅对自身 Owner 生效）
    /// </summary>
    public override bool ShouldClearBlock(Creature creature)
    {
        if (Owner == creature)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 回合开始时检查：护卫已被清空（≤0）则本效果失效并移除自身。
    /// 护卫数值在回合结束时由 GuardPower 通过内部 SetAmount 结算（不触发 AfterPowerAmountChanged），
    /// 因此这里是失效检测的主路径。
    /// </summary>
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await base.AfterPlayerTurnStart(choiceContext, player);

        if (player != Owner.Player)
        {
            return;
        }

        var guard = Owner.GetPower<GuardPower>();
        if (guard == null || guard.GuardAmount <= 0)
        {
            Log.Info($"[KnightMouseShieldFormation] Guard is {(guard?.GuardAmount ?? 0)} at turn start, removing self.");
            await PowerCmd.Remove(this);
        }
    }

    /// <summary>
    /// 当护卫层数被清空（变为0或以下）时，移除此 Power
    /// </summary>
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not GuardPower guardPower)
        {
            Log.Debug($"[KnightMouseShieldFormation] AfterPowerAmountChanged: power={power?.GetType().Name} (not GuardPower), skip");
            return;
        }
        if (guardPower.Owner != Owner)
        {
            Log.Debug($"[KnightMouseShieldFormation] AfterPowerAmountChanged: guard owner={guardPower.Owner?.Name}, self owner={Owner?.Name}, skip");
            return;
        }

        Log.Info($"[KnightMouseShieldFormation] AfterPowerAmountChanged: guard={guardPower.GuardAmount}, amountDelta={amount}");
        if (guardPower.GuardAmount <= 0)
        {
            Log.Info($"[KnightMouseShieldFormation] Guard stacks cleared ({guardPower.GuardAmount}), removing self.");
            await PowerCmd.Remove(this);
        }
        else
        {
            Log.Debug($"[KnightMouseShieldFormation] Guard still positive ({guardPower.GuardAmount}), keep effect");
        }
    }
}
