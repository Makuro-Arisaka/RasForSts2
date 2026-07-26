using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using RasForSts2.Scripts.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

[RegisterCard(typeof(XilaCardPool))]
public class Reload : XilaCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public Reload() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var strikeCards = Owner.PlayerCombatState.AllCards
            .Where(c => c != null && c.Tags.Contains(CardTag.Strike))
            .ToList();

        foreach (var strikeCard in strikeCards)
        {
            await CardPileCmd.Add(strikeCard, PileType.Hand);

            var pellet = ((CombatState)CombatState).CreateCard<Pellet>(Owner);
            if (IsUpgraded)
            {
                CardCmd.Upgrade(pellet);
            }
            await CardCmd.Transform(strikeCard, pellet);
        }
    }

    protected override void OnUpgrade()
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromCard<Pellet>(IsUpgraded),
    ];
}
