using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 精准锁定
/// spec: 1cost [罕见技能牌]
/// 选择两张牌获得保留。下回合打出的第1张攻击牌增加50%（75%）伤害。结束你的回合。
/// </summary>
[RegisterCard(typeof(XilaCardPool))]
public class PrecisionLock : XilaCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DamageBoost", 50m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
    ];

    public PrecisionLock() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 选择两张手牌获得保留（过滤已含保留的牌；不足两张则尽量选择）
        var retainable = PileType.Hand.GetPile(Owner).Cards
            .Where(c => !c.Keywords.Contains(CardKeyword.Retain))
            .ToList();
        if (retainable.Count > 0)
        {
            int selectCount = Math.Min(2, retainable.Count);
            IEnumerable<CardModel> selected = await CardSelectCmd.FromHand(
                prefs: new CardSelectorPrefs(SelectionScreenPrompt, 0, selectCount),
                context: choiceContext,
                player: Owner,
                filter: (CardModel c) => !c.Keywords.Contains(CardKeyword.Retain),
                source: this);
            foreach (CardModel card in selected)
            {
                CardCmd.ApplyKeyword(card, [CardKeyword.Retain]);
                Log.Info($"[PrecisionLock] Card '{card.Id.Entry}' gained Retain.");
            }
        }
        else
        {
            Log.Info("[PrecisionLock] No retainable cards in hand, skipping selection.");
        }

        // 2. 下回合打出的第1张攻击牌伤害 +50%(75%)
        await PowerCmd.Apply<PrecisionLockPower>(choiceContext, Owner.Creature, IsUpgraded ? 75m : 50m, Owner.Creature, this);

        // 3. 结束你的回合
        PlayerCmd.EndTurn(Owner, canBackOut: false);
    }

    protected override void OnUpgrade()
    {
        // 升级：增伤 50% → 75%
        DynamicVars["DamageBoost"].UpgradeValueBy(25m);
    }
}
