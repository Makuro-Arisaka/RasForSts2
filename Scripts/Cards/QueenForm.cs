using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RasForSts2.Scripts.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

[RegisterCard(typeof(XilaCardPool))]
public class QueenForm : XilaCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public QueenForm() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // spec: 从 英雄决意（英雄决意+）和 邪咒再显（邪咒再显+）中选择一张加入手牌
        // QueenForm 升级后，两个选项都应为升级版
        List<MegaCrit.Sts2.Core.Models.CardModel> options = new();

        MegaCrit.Sts2.Core.Models.CardModel heroDetermination = Owner.Creature.CombatState.CreateCard(ModelDb.Card<HeroDetermination>(), Owner);
        if (IsUpgraded) CardCmd.Upgrade(heroDetermination);
        options.Add(heroDetermination);

        MegaCrit.Sts2.Core.Models.CardModel curseReveal = Owner.Creature.CombatState.CreateCard(ModelDb.Card<CurseReveal>(), Owner);
        if (IsUpgraded) CardCmd.Upgrade(curseReveal);
        options.Add(curseReveal);

        MegaCrit.Sts2.Core.Models.CardModel selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner, canSkip: false);

        if (selected != null)
        {
            await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade() { }

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromCard<HeroDetermination>(IsUpgraded),
        HoverTipFactory.FromCard<CurseReveal>(IsUpgraded),
    ];
}
