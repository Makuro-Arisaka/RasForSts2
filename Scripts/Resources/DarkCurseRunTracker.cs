using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;
using RasForSts2.Scripts.Cards;
using STS2RitsuLib.RunData;

namespace RasForSts2.Scripts.Resources;

/// <summary>
/// 跨战斗持久化的跑局数据：本局游戏累计获得的黑暗法咒总量。
/// </summary>
public sealed class DarkCurseRunData
{
    public int TotalGained { get; set; }
}

/// <summary>
/// 追踪「本局游戏中获得过多少黑暗法咒」（按玩家分桶，通过 RitsuLib RunSavedData 跨战斗持久化）。
/// </summary>
public static class DarkCurseRunTracker
{
    private const string SaveKey = "dark_curse_total_gained";

    private static readonly PlayerRunSavedData<DarkCurseRunData> SavedData =
        RunSavedDataStore.For(Entry.ModId).RegisterPerPlayer<DarkCurseRunData>(
            SaveKey,
            () => new(),
            new() { WritePolicy = RunSavedDataWritePolicy.WhenNonDefault });

    /// <summary>
    /// 确保槽位在跑局开始前完成注册（在 Mod 初始化时调用）。
    /// </summary>
    public static void Initialize()
    {
        _ = SavedData;
        Log.Info("[DarkCurse] RunTracker.Initialize: run saved data slot registered.");
    }

    /// <summary>
    /// 记录本次实际获得的黑暗法咒数量（仅累计正向获得，非跑局场景安全跳过）。
    /// </summary>
    public static void AddGained(Player player, int amount)
    {
        if (amount <= 0 || player.RunState is not RunState runState)
            return;

        SavedData.Modify(runState, player.NetId, data => data.TotalGained += amount);
        int total = SavedData.Get(runState, player.NetId).TotalGained;
        Log.Info($"[DarkCurse] RunTracker.AddGained: player={player.NetId}, amount={amount}, totalGained={total}");

        // 达到最低阈值（升级版 66）后，自动把已有及之后获得的悲伤！！变换为真正的友谊
        if (total >= SorrowTransformHelper.ThresholdUpgraded)
            _ = TryAutoTransformSorrow(player);
    }

    /// <summary>
    /// 后台触发全局扫描：将所有已达阈值的悲伤！！变换为真正的友谊（吞掉异常防止影响 gain 流程）。
    /// </summary>
    private static async Task TryAutoTransformSorrow(Player player)
    {
        try
        {
            await SorrowTransformHelper.TransformAllReached(player);
        }
        catch (Exception ex)
        {
            Log.Warn($"[DarkCurse] Auto transform Sorrow failed: {ex}");
        }
    }

    /// <summary>
    /// 读取本局游戏累计获得的黑暗法咒总量（不在跑局中时返回 0）。
    /// </summary>
    public static int GetTotalGained(Player player)
    {
        if (player.RunState is not RunState runState)
            return 0;

        return SavedData.TryGet(runState, player.NetId, out var data) ? data.TotalGained : 0;
    }
}
