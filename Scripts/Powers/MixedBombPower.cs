using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
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

    // spec: 回合开始时对所有敌人造成5（8）点伤害
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

    // spec: 每当你给予一个敌人负面状态时，使其受到5（8）点伤害
    // 参考原版 SleightOfFleshPower 的实现，使用原生 AfterPowerAmountChanged 方法
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        var target = power.Owner;
        Log.Info($"[MixedBomb] 触发检查: power={power.GetType().Name}, amount={amount}, " +
                 $"target={target.LogName}(CombatId={target.CombatId}), " +
                 $"applier={(applier == null ? "null" : applier.LogName)}, " +
                 $"owner={Owner.LogName}, cardSource={(cardSource == null ? "null" : cardSource.Id)}");

        if (amount == 0m)
        {
            Log.Debug($"[MixedBomb] 跳过: amount == 0 (target={target.LogName})");
            return;
        }
        if (power.GetTypeForAmount(amount) != PowerType.Debuff)
        {
            Log.Debug($"[MixedBomb] 跳过: power 类型不是 Debuff " +
                      $"(power={power.GetType().Name}, actualType={power.GetTypeForAmount(amount)}, target={target.LogName})");
            return;
        }
        if (!target.IsEnemy)
        {
            Log.Debug($"[MixedBomb] 跳过: target 不是敌人 (target={target.LogName})");
            return;
        }
        if (applier != Owner)
        {
            Log.Debug($"[MixedBomb] 跳过: applier != Owner " +
                      $"(applier={(applier == null ? "null" : applier.LogName)}, owner={Owner.LogName})");
            return;
        }
        if (power is ITemporaryPower)
        {
            Log.Debug($"[MixedBomb] 跳过: power 是 ITemporaryPower (power={power.GetType().Name}, target={target.LogName})");
            return;
        }

        Log.Info($"[MixedBomb] 命中: 对 {target.LogName}(CombatId={target.CombatId}) " +
                 $"造成 {Amount} 点伤害 (power={power.GetType().Name}, amount={amount})");
        Flash();
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, Amount, ValueProp.Unpowered, Owner);
    }
}
