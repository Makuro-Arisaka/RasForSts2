using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using RasForSts2.Scripts.Nodes;
using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;

namespace RasForSts2.Scripts.Resources;

/// <summary>
/// 黑暗法咒资源定义
/// </summary>
public static class DarkCurseResource
{
    public const string ResourceId = "RAS_FOR_STS2_SECONDARY_RESOURCE_DARK_CURSE";

    public static SecondaryResourceDefinition Definition { get; private set; } = null!;

    public static void Register()
    {
        var resources = RitsuLibFramework.GetSecondaryResourceRegistry(Entry.ModId);

        Definition = resources.Register("dark_curse", new SecondaryResourceDefinition(
            defaultAmount: 0,
            baseMaxAmount: 10,
            minAmount: 0,
            hardMaxAmount: 999_999_999,
            turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
            persistencePolicy: SecondaryResourcePersistencePolicy.Combat,
            smallIconPath: "res://RasForSts2/images/ui/DarkCurse_small.png",
            largeIconPath: "res://RasForSts2/images/ui/DarkCurse_large.png"
        ));

        Log.Info($"[DarkCurse] Step 1: Resource registered. Id={Definition.Id}");

        // 不使用 AlwaysShowInCombatUi(全局)。
        // 任何角色，只要获得了黑暗法咒 (amount>0) 就在战斗UI中显示；数量为0时不显示。
        // — 与辉星行为一致：获得时显示，用光时消失。
        resources.RegisterCombatUiAlwaysVisibleWhen("dark_curse", ctx => ctx.Amount > 0, order: -500);

        Log.Info($"[DarkCurse] Step 2: RegisterCombatUiAlwaysVisibleWhen registered for 'dark_curse', any character, show when amount>0");

        // 注册UI节点，用于在战斗中显示黑暗法咒大图标计数器
        // 直接挂载到 NCombatUi 根层，在 _Ready 中设置绝对位置到能量附近
        resources.RegisterCombatUi(
            "dark_curse_counter",
            parent =>
            {
                Log.Info("[DarkCurse] UI: Creating DarkCurseCounterRow node...");
                return new DarkCurseCounterRow();
            },
            update: ctx =>
            {
                if (ctx.Node == null) return;
                var visibleCount = ctx.VisibleDefinitions?.Count ?? 0;
                if (visibleCount > 0)
                {
                    Log.Info($"[DarkCurse] UI: Refresh, player={ctx.Player?.Character?.GetType().Name ?? "null"}, visibleDefinitions={visibleCount}, nodeName={ctx.Node.Name}, visible={ctx.Node.Visible}, size={ctx.Node.Size}, pos={ctx.Node.Position}");
                }
                ctx.Node.RefreshDisplay(ctx.Player, ctx.VisibleDefinitions);
            });

        Log.Info($"[DarkCurse] Step 3: RegisterCombatUi 'dark_curse_counter' registered");

        // 注册卡牌费用显示 UI：为有黑暗法咒费用的卡牌（如法咒飞刺的 X 点黑暗法咒），
        // 在卡牌左上角能量图标旁（原版辉星费用槽位置）显示要消耗的黑暗法咒图标与数量。
        resources.RegisterCardUi(
            "dark_curse_card_cost",
            _ =>
            {
                var style = new SecondaryResourceCardCostUiStyle
                {
                    ReserveVanillaStarCostSlot = true,
                    SlotSize = new Vector2(58f, 58f),
                    IconSize = new Vector2(56f, 56f),
                    FontSize = 22,
                    OutlineSize = 12,
                };
                var node = NSecondaryResourceCardCostUi.Create(ResourceId, style);
                // 对齐原版辉星费用槽位置（相对 NCard.Body / CardContainer）
                node.Position = new Vector2(-186f, -189f);
                return node;
            },
            update: ctx => ctx.Node.Refresh(ctx));

        Log.Info($"[DarkCurse] Step 4: RegisterCardUi 'dark_curse_card_cost' registered");

        Log.Info($"[DarkCurse] Registered resource: Id={Definition.Id}");
    }

    /// <summary>
    /// 读取当前黑暗法咒数量。
    /// </summary>
    public static int Get(Player player)
    {
        int current = SecondaryResourceCmd.Get(player, ResourceId);
        Log.Info($"[DarkCurse] Get: player={PlayerTag(player)}, current={current}");
        return current;
    }

    /// <summary>
    /// 读取当前黑暗法咒上限（无上限概念时为 null）。
    /// </summary>
    public static int? GetMax(Player player)
    {
        int? max = SecondaryResourceCmd.GetMax(player, ResourceId);
        Log.Info($"[DarkCurse] GetMax: player={PlayerTag(player)}, max={(max.HasValue ? max.Value.ToString() : "null")}");
        return max;
    }

    /// <summary>
    /// 增加黑暗法咒。会经过 gain hook 与上限修正。
    /// </summary>
    public static async Task<int> Gain(Player player, int amount, AbstractModel? source = null)
    {
        int before = SecondaryResourceCmd.Get(player, ResourceId);
        Log.Info($"[DarkCurse] Gain.Start: player={PlayerTag(player)}, before={before}, requestAmount={amount}, source={SourceTag(source)}");

        int after = await SecondaryResourceCmd.Gain(player, ResourceId, amount, source);

        // 累计本局游戏获得的黑暗法咒（用于悲伤！！的变化阈值判定）
        int actualDelta = after - before;
        if (actualDelta > 0)
            DarkCurseRunTracker.AddGained(player, actualDelta);

        Log.Info($"[DarkCurse] Gain.End: player={PlayerTag(player)}, before={before}, after={after}, actualDelta={after - before}, source={SourceTag(source)}");
        return after;
    }

    /// <summary>
    /// 失去黑暗法咒。不会低于最小值。
    /// </summary>
    public static async Task<int> Lose(Player player, int amount, AbstractModel? source = null)
    {
        int before = SecondaryResourceCmd.Get(player, ResourceId);
        Log.Info($"[DarkCurse] Lose.Start: player={PlayerTag(player)}, before={before}, requestAmount={amount}, source={SourceTag(source)}");

        int after = await SecondaryResourceCmd.Lose(player, ResourceId, amount, source);

        Log.Info($"[DarkCurse] Lose.End: player={PlayerTag(player)}, before={before}, after={after}, actualDelta={after - before}, source={SourceTag(source)}");
        return after;
    }

    /// <summary>
    /// 设置黑暗法咒为指定数量。
    /// </summary>
    public static async Task<int> Set(Player player, int amount, AbstractModel? source = null)
    {
        int before = SecondaryResourceCmd.Get(player, ResourceId);
        Log.Info($"[DarkCurse] Set.Start: player={PlayerTag(player)}, before={before}, targetAmount={amount}, source={SourceTag(source)}");

        int after = await SecondaryResourceCmd.Set(player, ResourceId, amount, source);

        Log.Info($"[DarkCurse] Set.End: player={PlayerTag(player)}, before={before}, after={after}, actualDelta={after - before}, source={SourceTag(source)}");
        return after;
    }

    /// <summary>
    /// 消耗黑暗法咒。数量不足会返回 false 且不改变数量。
    /// </summary>
    public static async Task<bool> Spend(Player player, int amount, CardModel? card = null, AbstractModel? source = null)
    {
        int before = SecondaryResourceCmd.Get(player, ResourceId);
        bool enough = before >= amount;
        Log.Info($"[DarkCurse] Spend.Start: player={PlayerTag(player)}, before={before}, requestAmount={amount}, enough={enough}, card={CardTag(card)}, source={SourceTag(source)}");

        if (!enough)
        {
            Log.Info($"[DarkCurse] Spend.Skip: insufficient resource, before={before} < requestAmount={amount}, card={CardTag(card)}");
            return false;
        }

        bool success = await SecondaryResourceCmd.Spend(player, ResourceId, amount, card, source);
        int after = SecondaryResourceCmd.Get(player, ResourceId);

        Log.Info($"[DarkCurse] Spend.End: player={PlayerTag(player)}, before={before}, after={after}, actualDelta={after - before}, success={success}, card={CardTag(card)}, source={SourceTag(source)}");
        return success;
    }

    /// <summary>
    /// 重置黑暗法咒（toMax=true 时重置为上限，否则重置为默认值）。
    /// </summary>
    public static async Task<int> Reset(Player player, bool toMax = false, AbstractModel? source = null)
    {
        int before = SecondaryResourceCmd.Get(player, ResourceId);
        Log.Info($"[DarkCurse] Reset.Start: player={PlayerTag(player)}, before={before}, toMax={toMax}, source={SourceTag(source)}");

        int after = await SecondaryResourceCmd.Reset(player, ResourceId, toMax, source);

        Log.Info($"[DarkCurse] Reset.End: player={PlayerTag(player)}, before={before}, after={after}, actualDelta={after - before}, source={SourceTag(source)}");
        return after;
    }

    private static string PlayerTag(Player player)
    {
        // 优先用角色类型名；不可用时退回 NetId
        string? charName = player?.Character?.GetType().Name;
        ulong netId = player?.NetId ?? 0ul;
        return string.IsNullOrEmpty(charName) ? $"NetId={netId}" : $"{charName}(NetId={netId})";
    }

    private static string SourceTag(AbstractModel? source)
    {
        return source == null ? "null" : source.GetType().Name;
    }

    private static string CardTag(CardModel? card)
    {
        return card == null ? "null" : card.GetType().Name;
    }
}
