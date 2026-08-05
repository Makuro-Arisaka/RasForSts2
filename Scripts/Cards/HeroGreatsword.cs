using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Commands;
using RasForSts2.Scripts.Helpers;
using RasForSts2.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 女王武具·英雄大剑
/// 通过先古之牙(ArchaicTooth)将女王武具·月光大剑变化而来。
/// 保留。获得3点护卫。下一张女王武器牌可以免费打出。攻击/受击伤害翻倍（复用月光大剑Power）。
/// </summary>
[RegisterCard(typeof(XilaCardPool))]
public class HeroGreatsword : XilaCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public HeroGreatsword() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    // 暂无专属卡图，使用 empty 占位
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://RasForSts2/images/cards/empty.png"
    );

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得护卫（升级不增加护卫值，只降费用）
        decimal guardAmount = 3m;
        await PowerCmd.Apply<GuardPower>(choiceContext, Owner.Creature, guardAmount, Owner.Creature, this);

        // 下一张女王武器牌可以免费打出
        await PowerCmd.Apply<HeroGreatswordFreeWeaponPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

        // 若玩家身上有 QueenHarpPower，则不能切换女王武具，仅获得护卫与免费效果
        if (!QueenWeaponCmd.CanSwitchWeapon(Owner))
        {
            return;
        }

        // 切换女王武具：复用月光大剑Power（攻击伤害翻倍，敌人攻击对你伤害翻倍）
        await QueenWeaponCmd.SwitchWeapon<MoonlightGreatswordPower>(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        QueenWeaponHoverTip.Create(),
        HoverTipFactory.FromPower<GuardPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
    ];
}
