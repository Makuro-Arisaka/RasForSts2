using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Relics;

/// <summary>
/// 石龙链甲（罕见遗物）
/// 当你的护卫层数消失时，在下回合开始时给予3层护卫。
/// 参考 GuardPower 的 TurnEnded/TurnStarted 事件订阅方式。
/// </summary>
[RegisterRelic(typeof(XilaRelicPool))]
public class StoneDragonArmor : ModRelicTemplate
{
    private bool _guardDepletedThisTurn;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://RasForSts2/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"res://RasForSts2/images/relics/{GetType().Name}.png",
        BigIconPath: $"res://RasForSts2/images/relics/{GetType().Name}.png"
    );

    // 护卫回复量
    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar("Guard", 3m) };

    public override Task BeforeCombatStart()
    {
        _guardDepletedThisTurn = false;
        CombatManager.Instance.TurnEnded += OnTurnEnded;
        CombatManager.Instance.TurnStarted += OnTurnStarted;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        CombatManager.Instance.TurnEnded -= OnTurnEnded;
        CombatManager.Instance.TurnStarted -= OnTurnStarted;
        _guardDepletedThisTurn = false;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 玩家回合结束时（护卫减少后），检查护卫是否消失。
    /// </summary>
    private void OnTurnEnded(CombatState state)
    {
        if (state.CurrentSide != CombatSide.Player)
        {
            return;
        }

        Creature? creature = base.Owner?.Creature;
        if (creature == null)
        {
            return;
        }

        GuardPower? guard = creature.GetPower<GuardPower>();
        if (guard == null)
        {
            // 没有 GuardPower，不算"消失"
            return;
        }

        // 护卫层数为 0 表示消失了
        if (guard.GuardAmount <= 0)
        {
            _guardDepletedThisTurn = true;
        }
    }

    /// <summary>
    /// 玩家回合开始时，如果上回合护卫消失，给予3层护卫。
    /// </summary>
    private async void OnTurnStarted(CombatState state)
    {
        if (state.CurrentSide != CombatSide.Player)
        {
            return;
        }

        if (!_guardDepletedThisTurn)
        {
            return;
        }

        _guardDepletedThisTurn = false;

        Creature? creature = base.Owner?.Creature;
        if (creature == null)
        {
            return;
        }

        Flash();
        decimal amount = base.DynamicVars["Guard"].BaseValue;
        await PowerCmd.Apply<GuardPower>(null, creature, amount, creature, null);
    }
}
