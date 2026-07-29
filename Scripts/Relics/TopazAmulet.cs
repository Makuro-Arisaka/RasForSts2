using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using RasForSts2.Scripts.Characters;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Relics;

/// <summary>
/// 黄玉护符（商店遗物）
/// 在战斗结束时，如果你没有受到伤害，则你的卡牌奖励额外包含一张卡牌。并额外掉落5金币。
/// 监听"未受到伤害"参考 LavaLamp，"额外卡牌奖励"参考 LastingCandy，"额外金币"参考 AmethystAubergine。
/// </summary>
[RegisterRelic(typeof(GenericRelicPool))]
public class TopazAmulet : ModRelicTemplate
{
    private bool _tookDamageThisCombat;

    public override RelicRarity Rarity => RelicRarity.Shop;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://RasForSts2/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"res://RasForSts2/images/relics/{GetType().Name}.png",
        BigIconPath: $"res://RasForSts2/images/relics/{GetType().Name}.png"
    );

    // 额外掉落的金币数（文档要求5金币）
    protected override IEnumerable<DynamicVar> CanonicalVars => [new GoldVar(5)];

    [SavedProperty]
    public bool TookDamageThisCombat
    {
        get => _tookDamageThisCombat;
        set
        {
            AssertMutable();
            _tookDamageThisCombat = value;
        }
    }

    /// <summary>
    /// 进入房间时重置伤害标记（参考 LavaLamp）。
    /// </summary>
    public override Task AfterRoomEntered(AbstractRoom room)
    {
        TookDamageThisCombat = false;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 受到伤害时设置标记（参考 LavaLamp）。
    /// 过滤掉非战斗、非玩家、0伤害、不可格挡的情况。
    /// </summary>
    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!(base.Owner.RunState.CurrentRoom is CombatRoom))
        {
            return Task.CompletedTask;
        }
        if (target != base.Owner.Creature)
        {
            return Task.CompletedTask;
        }
        if (result.UnblockedDamage <= 0)
        {
            return Task.CompletedTask;
        }
        if (props.HasFlag(ValueProp.Unblockable))
        {
            return Task.CompletedTask;
        }
        TookDamageThisCombat = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 卡牌奖励额外包含一张卡牌（参考 LastingCandy）。
    /// 不限制卡牌类型，从玩家卡池中随机生成一张。
    /// </summary>
    public override bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> options, CardCreationOptions creationOptions)
    {
        if (base.Owner != player)
        {
            return false;
        }
        if (creationOptions.Source != CardCreationSource.Encounter)
        {
            return false;
        }
        if (TookDamageThisCombat)
        {
            return false;
        }

        // 从玩家卡池中生成一张随机卡牌（不限制类型）
        // 使用 CardPools 而非 GetPossibleCards 以匹配正确的构造函数重载
        CardCreationOptions options2 = new CardCreationOptions(
            creationOptions.CardPools,
            CardCreationSource.Other,
            creationOptions.RarityOdds)
            .WithFlags(CardCreationFlags.NoModifyHooks | CardCreationFlags.NoCardPoolModifications);
        CardModel cardModel = CardFactory.CreateForReward(base.Owner, 1, options2).FirstOrDefault()?.Card;
        if (cardModel != null)
        {
            CardCreationResult cardCreationResult = new CardCreationResult(cardModel);
            cardCreationResult.ModifyCard(cardModel, this);
            options.Add(cardCreationResult);
        }
        return cardModel != null;
    }

    /// <summary>
    /// 额外掉落金币（参考 AmethystAubergine）。
    /// </summary>
    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (player != base.Owner)
        {
            return false;
        }
        if (room == null)
        {
            return false;
        }
        if (!room.RoomType.IsCombatRoom())
        {
            return false;
        }
        if (TookDamageThisCombat)
        {
            return false;
        }
        rewards.Add(new GoldReward(base.DynamicVars.Gold.IntValue, player));
        return true;
    }

    public override Task AfterModifyingRewards()
    {
        Flash();
        return Task.CompletedTask;
    }
}
