using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Rewards;

namespace RasForSts2.Scripts.Patches;

[HarmonyPatch(typeof(RewardsSet), "GenerateWithoutOffering")]
public static class GoldTouchRewardPatch
{
    private static readonly HashSet<Player> _goldTouchActivePlayers = new();

    public static void MarkPlayerForBonus(Player player)
    {
        _goldTouchActivePlayers.Add(player);
        Log.Info($"[GoldTouch] 标记玩家 {player.Creature.LogName} 获得金币奖励加成（当前待处理玩家数: {_goldTouchActivePlayers.Count}）");
    }

    public static bool HasBonus(Player player)
    {
        return _goldTouchActivePlayers.Contains(player);
    }

    private static void Postfix(RewardsSet __instance)
    {
        var player = __instance.Player;
        if (!_goldTouchActivePlayers.Contains(player))
        {
            return;
        }

        _goldTouchActivePlayers.Remove(player);

        var rewards = __instance.Rewards;
        Log.Info($"[GoldTouch] 进入奖励生成后处理: 玩家={player.Creature.LogName}, 奖励总数={rewards.Count}");

        if (rewards.Count == 0)
        {
            Log.Info($"[GoldTouch] 奖励列表为空，跳过额外金币发放");
            return;
        }

        int totalGold = 0;
        int goldRewardCount = 0;
        foreach (Reward reward in rewards)
        {
            if (reward is GoldReward goldReward)
            {
                int amount = goldReward.Amount;
                Log.Info($"[GoldTouch]   发现金币奖励: Amount={amount}, IsPopulated={goldReward.IsPopulated}");
                if (amount > 0)
                {
                    totalGold += amount;
                    goldRewardCount++;
                }
                else
                {
                    Log.Warn($"[GoldTouch]   金币奖励 Amount 非正数({amount})，已跳过");
                }
            }
        }

        Log.Info($"[GoldTouch] 统计完成: 金币奖励条目数={goldRewardCount}, 金币总额={totalGold}");

        if (totalGold > 0)
        {
            var bonusReward = new GoldReward(totalGold, player);
            rewards.Add(bonusReward);
            Log.Info($"[GoldTouch] 追加额外金币奖励: 数量={totalGold}, 追加后奖励总数={rewards.Count}");
        }
        else
        {
            Log.Info($"[GoldTouch] 金币总额为 0，未追加额外金币奖励");
        }
    }
}
