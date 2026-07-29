using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using RasForSts2.Scripts.Patches;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

[RegisterPower]
public sealed class GoldTouchPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://RasForSts2/images/potions/GoldTouchPotion.png",
        BigIconPath: "res://RasForSts2/images/potions/GoldTouchPotion.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    // spec: 本场战斗结束时，额外获得战斗胜利奖励中金币数量的100%金币
    // 使用 AfterCombatEnd 而非 AfterCombatVictory，因为 AfterCombatVictory 在 powers 被清除之后才触发
    // CombatManager 时序: AfterCombatEnd → player.AfterCombatEnd(清除powers) → AfterCombatVictory
    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.Player != null)
        {
            Log.Info($"[GoldTouch] 战斗结束，标记玩家 {Owner.Player.Creature.LogName} 获得金币加成");
            GoldTouchRewardPatch.MarkPlayerForBonus(Owner.Player);
        }
        return Task.CompletedTask;
    }
}
