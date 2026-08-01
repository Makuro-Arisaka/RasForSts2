using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using RasForSts2.Scripts.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 毕业鸦的通晓
/// spec: 99 cost [稀有技能牌]
/// 选择任意一张(升级的)卡牌，加入手牌。在这场战斗中免费打出。消耗。
/// </summary>
[RegisterCard(typeof(XilaCardPool))]
public class GraduationCrow : XilaCardModel
{
    private const int energyCost = 99;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public GraduationCrow() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获取所有可选卡牌：包括所有角色的牌、无色牌与先古牌
        // 排除派生牌(Token)、诅咒牌(Curse)、状态牌(Status)、事件牌(Event)、任务牌(Quest)
        // 排除自身以防止无限循环（即使升级后有消耗，也不允许选自己生成自己）
        List<CardModel> allCards = ModelDb.AllCards
            .Where(c => (c.Rarity == CardRarity.Basic
                     || c.Rarity == CardRarity.Common
                     || c.Rarity == CardRarity.Uncommon
                     || c.Rarity == CardRarity.Rare
                     || c.Rarity == CardRarity.Ancient)
                     && c.Id != Id)
            .ToList();

        // 转换为 mutable 实例供选择界面使用
        List<CardModel> options = allCards
            .Select(c => Owner.Creature.CombatState.CreateCard(c, Owner))
            .ToList();

        // 使用 FromSimpleGrid 支持超过3张卡牌的网格浏览
        CardSelectorPrefs prefs = new CardSelectorPrefs(
            CardSelectorPrefs.TransformSelectionPrompt, 1);

        IEnumerable<CardModel> selected = await CardSelectCmd.FromSimpleGrid(
            choiceContext, options, Owner, prefs);

        CardModel? chosen = selected.FirstOrDefault();

        if (chosen != null)
        {
            // 如果升级，升级选中的卡牌
            if (IsUpgraded && chosen.IsUpgradable)
            {
                CardCmd.Upgrade(chosen);
            }
            // 本场战斗免费打出
            chosen.SetToFreeThisCombat();
            // 加入手牌
            await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级后仍然消耗，但选中的卡牌会升级
    }
}
