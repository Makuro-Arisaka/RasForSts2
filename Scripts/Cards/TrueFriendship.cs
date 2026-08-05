using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using RasForSts2.Scripts.Powers;
using RasForSts2.Scripts.Resources;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 真正的友谊
/// spec: 0cost [无色派生技能卡]
/// 获得1层无实体，获得50层护卫，获得66(99)点黑暗法咒。
/// 只能通过「悲伤！！」变化获得，不进入卡牌奖励/商店等随机池。
/// </summary>
// 注册到 TokenCardPool（与原版派生卡 Shiv 一致），防止被发现/药水/熵等局内生成机制选中
[RegisterCard(typeof(TokenCardPool))]
public class TrueFriendship : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        SecondaryResourceVars.ForLocal("DarkCurse", Entry.ModId, "dark_curse", 66m),
        new PowerVar<IntangiblePower>(1m),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://RasForSts2/images/cards/empty.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<IntangiblePower>(),
        HoverTipFactory.FromPower<GuardPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
    ];

    public TrueFriendship() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 1. 获得 1 层无实体
        await PowerCmd.Apply<IntangiblePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

        // 2. 获得 50 层护卫
        await PowerCmd.Apply<GuardPower>(choiceContext, Owner.Creature, 50m, Owner.Creature, this);

        // 3. 获得 66(99) 点黑暗法咒
        int curseAmount = (int)DynamicVars["DarkCurse"].BaseValue;
        await DarkCurseResource.Gain(Owner, curseAmount, this);
    }

    protected override void OnUpgrade()
    {
        // 升级：黑暗法咒 66→99 (+33)
        DynamicVars["DarkCurse"].UpgradeValueBy(33m);
    }
}
