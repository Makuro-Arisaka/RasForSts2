using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Combat.SecondaryResources;

namespace RasForSts2.Scripts.Nodes;

/// <summary>
/// 黑暗法咒计数器行节点。直接显示在能量附近的大图标。
/// </summary>
public partial class DarkCurseCounterRow : NSecondaryResourceCounterRow
{
    private const float IconDim = 120f;

    public override void _Ready()
    {
        base._Ready();

        // 大图标样式（不用缩略图，禁用悬浮提示）
        // 关键：IconStyle 必须显式设置 Size 和 Stretch/Expand 模式，
        // 否则 ResolveIconStyle() 会用 SecondaryResourceIconStyle.Default 的默认 Size，
        // 导致贴图尺寸不对。
        Configure(new SecondaryResourceCounterStyle
        {
            RowSeparation = 10,
            CounterSize = new Vector2(IconDim, IconDim),
            IconSize = new Vector2(IconDim, IconDim),
            FontSize = 32,
            OutlineSize = 16,
            // 和辉星一样，只显示当前数量，不显示 X/Max
            FormatAmount = static (amount, _) => amount.ToString(),
            IconStyle = SecondaryResourceIconStyle.Default with
            {
                Size = new Vector2(IconDim, IconDim),
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                HoverTip = SecondaryResourceHoverTipStyle.Default with { Enabled = false },
            },
        });

        // 绝对定位：在能量圆盘 (约 X=230, Y=860) 的正上方，绿色方框位置
        Position = new Vector2(60f, 750f);
        Size = new Vector2(IconDim + 30, IconDim + 10);
    }

    /// <summary>
    /// 刷新显示。由 SecondaryResourceUi 框架自动调用。
    /// </summary>
    public void RefreshDisplay(Player? player, IReadOnlyList<SecondaryResourceDefinition> visibleDefinitions)
    {
        Refresh(player, visibleDefinitions);
    }
}
