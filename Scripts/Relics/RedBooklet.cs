using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using RasForSts2.Scripts.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Relics;

/// <summary>
/// 赤短册（稀有遗物）
/// 当你的生命值将要降至0或以下时，回复最大生命值的10%，获得200金币。（仅能起效一次）
/// 监听死亡阻止参考 LizardTail，金币奖励参考 AmethystAubergine。
/// </summary>
[RegisterRelic(typeof(XilaRelicPool))]
public class RedBooklet : ModRelicTemplate
{
    private bool _wasUsed;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool IsUsedUp => _wasUsed;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://RasForSts2/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"res://RasForSts2/images/relics/{GetType().Name}.png",
        BigIconPath: $"res://RasForSts2/images/relics/{GetType().Name}.png"
    );

    // 回复最大生命值的10%，获得200金币
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new HealVar(10m),
        new GoldVar(200)
    };

    [SavedProperty]
    public bool WasUsed
    {
        get => _wasUsed;
        set
        {
            AssertMutable();
            _wasUsed = value;
            if (IsUsedUp)
            {
                base.Status = RelicStatus.Disabled;
            }
        }
    }

    /// <summary>
    /// 当玩家生命值将要降至0或以下时，返回false阻止死亡（参考 LizardTail）。
    /// </summary>
    public override bool ShouldDieLate(Creature creature)
    {
        if (creature != base.Owner.Creature)
        {
            return true;
        }
        if (WasUsed)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 在阻止死亡后触发效果（参考 LizardTail）。
    /// 回复最大生命值的10%，获得200金币。
    /// </summary>
    public override async Task AfterPreventingDeath(Creature creature)
    {
        Flash();
        WasUsed = true;

        // 回复最大生命值的10%
        decimal healAmount = Math.Max(1m, creature.MaxHp * (base.DynamicVars.Heal.BaseValue / 100m));
        await CreatureCmd.Heal(creature, healAmount);

        // 获得200金币（参考 AmethystAubergine）
        if (base.Owner is Player player)
        {
            await PlayerCmd.GainGold(base.DynamicVars.Gold.IntValue, player);
        }
    }
}
