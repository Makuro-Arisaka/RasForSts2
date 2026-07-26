using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace RasForSts2.Scripts.Helpers;

/// <summary>
/// 提供"女王武具"悬浮词条的工厂方法。
/// 词条描述：打出女王武具牌时，切换女王武具效果。
/// </summary>
public static class QueenWeaponHoverTip
{
    private static readonly string TitleKey = "QUEEN_WEAPON_HOVER_TIP.title";
    private static readonly string DescKey = "QUEEN_WEAPON_HOVER_TIP.description";

    private static HoverTip? _cached;

    /// <summary>
    /// 创建"女王武具"悬浮词条。
    /// 使用 cards 本地化表中的条目，首次调用后缓存。
    /// </summary>
    public static HoverTip Create()
    {
        if (_cached is { } cached) return cached;

        var title = new LocString("cards", TitleKey);
        var description = new LocString("cards", DescKey);
        var tip = new HoverTip(title, description);
        _cached = tip;
        return tip;
    }
}
