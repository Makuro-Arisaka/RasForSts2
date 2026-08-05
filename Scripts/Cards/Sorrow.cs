using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Resources;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 悲伤！！
/// spec: 0cost [罕见技能卡]
/// 获得3（4）点黑暗法咒。如果你在本局游戏中获得过99点（66点）黑暗法咒，将此牌变化为真正的友谊。消耗。保留。
/// </summary>
[RegisterCard(typeof(XilaCardPool))]
public class Sorrow : XilaCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        SecondaryResourceVars.ForLocal("DarkCurse", Entry.ModId, "dark_curse", 3m),
        // 本局游戏累计获得的黑暗法咒（实时计算，用于描述中的 X 显示）。
        // 注意：卡牌图鉴/预览中的卡是 canonical 模型（无 Owner），访问 Owner 会抛 CanonicalModelException，
        // 因此先通过 IsCanonical 判断，canonical 时返回 0。
        ModCardVars.Computed("RunDarkCurseGained", 0m, card =>
            card is null || card.IsCanonical
                ? 0m
                : card.Owner is { } owner ? DarkCurseRunTracker.GetTotalGained(owner) : 0m),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromCard<TrueFriendship>(IsUpgraded),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    public Sorrow() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    /// <summary>
    /// 悲伤！！被加入任意牌堆时，若本局累计黑暗法咒已达自身阈值，自动变换为真正的友谊。
    /// 覆盖「之后获得的悲伤！！」（加入牌组/手牌时立即变化），且不会递归（变换后原卡已移出牌堆）。
    /// </summary>
    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (card == this && Pile is { Type: not PileType.Play })
        {
            if (SorrowTransformHelper.IsThresholdReached(Owner, IsUpgraded))
                return SorrowTransformHelper.Transform(this);
        }
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 1. 获得 3(4) 点黑暗法咒
        int curseAmount = (int)DynamicVars["DarkCurse"].BaseValue;
        await DarkCurseResource.Gain(Owner, curseAmount, this);

        // 2. 若本局累计获得过 99(66) 点黑暗法咒，获得真正的友谊（永久加入牌组）。
        //    本牌不在此处移除——它带 Exhaust 关键字，OnPlayWrapper 会在打出结束后正常送它进消耗堆，
        //    避免手动 RemoveFromCurrentPile 后 UI 卡牌节点滞留在打出区。
        int threshold = IsUpgraded ? 66 : 99;
        int totalGained = DarkCurseRunTracker.GetTotalGained(Owner);
        if (totalGained >= threshold)
        {
            // 必须用 RunState 作用域创建并登记（CardPileCmd.Add 到牌组要求 RunState.ContainsCard），
            // CombatState.CreateCard 只会登记到战斗作用域，加牌组时会抛异常。
            var trueFriendship = Owner.RunState.CreateCard<TrueFriendship>(Owner);
            if (IsUpgraded)
                CardCmd.Upgrade(trueFriendship);

            await CardPileCmd.Add(trueFriendship, PileType.Deck);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级：黑暗法咒 3→4 (+1)，变化阈值 99→66
        DynamicVars["DarkCurse"].UpgradeValueBy(1m);
    }
}
