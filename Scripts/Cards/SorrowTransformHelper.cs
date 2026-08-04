using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using RasForSts2.Scripts.Resources;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 「悲伤！！」→「真正的友谊」自动变换逻辑。
/// 本局累计获得的黑暗法咒达到阈值（未升级 99 / 升级 66）后：
/// - 已有的悲伤！！（牌组/手牌/弃牌/抽牌堆）自动变换；
/// - 之后获得的悲伤！！在加入牌堆时也会自动变换。
/// 变换后继承原卡的升级状态（升级版的真友谊获得 99 黑暗法咒）。
/// </summary>
public static class SorrowTransformHelper
{
    public const int ThresholdUnupgraded = 99;
    public const int ThresholdUpgraded = 66;

    // 全局扫描防重入锁
    private static bool _scanning;

    /// <summary>
    /// 判断本局累计获得的黑暗法咒是否达到指定升级状态的变换阈值。
    /// </summary>
    public static bool IsThresholdReached(Player player, bool upgraded)
    {
        return DarkCurseRunTracker.GetTotalGained(player) >= (upgraded ? ThresholdUpgraded : ThresholdUnupgraded);
    }

    /// <summary>
    /// 将单张悲伤！！变换为真正的友谊（继承升级状态）。
    /// 仅在非打出中、非消耗堆生效（打出中的悲伤！！由 Sorrow.OnPlay 处理：自身照常消耗，真友谊入牌组）。
    /// </summary>
    public static async Task Transform(Sorrow sorrow)
    {
        if (sorrow.Pile is not { Type: not PileType.Play and not PileType.Exhaust })
            return;

        var result = await CardCmd.TransformTo<TrueFriendship>(sorrow);
        if (result.HasValue && sorrow.IsUpgraded)
            CardCmd.Upgrade(result.Value.cardAdded);
    }

    /// <summary>
    /// 扫描玩家所有牌堆（永久牌组 + 战斗中的抽牌/手牌/弃牌堆），
    /// 将所有已达阈值的悲伤！！变换为真正的友谊。
    /// </summary>
    public static async Task TransformAllReached(Player player)
    {
        if (_scanning)
            return;
        _scanning = true;
        try
        {
            foreach (PileType pileType in new[] { PileType.Deck, PileType.Draw, PileType.Hand, PileType.Discard })
            {
                var pile = CardPile.Get(pileType, player);
                if (pile == null)
                    continue;

                foreach (var card in pile.Cards.ToList())
                {
                    if (card is not Sorrow sorrow)
                        continue;
                    if (!IsThresholdReached(player, sorrow.IsUpgraded))
                        continue;

                    await Transform(sorrow);
                }
            }
        }
        finally
        {
            _scanning = false;
        }
    }
}
