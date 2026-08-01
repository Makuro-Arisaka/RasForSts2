using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
    /// 当护卫层数被清空（变为0或以下）时，移除此 Power
    /// </summary>
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is GuardPower guardPower && guardPower.Owner == Owner)
        {
            if (guardPower.GuardAmount <= 0)
            {
                Log.Info($"[KnightMouseShieldFormation] Guard stacks cleared ({guardPower.GuardAmount}), removing self.");
                await PowerCmd.Remove(this);
            }
        }
    }
}
