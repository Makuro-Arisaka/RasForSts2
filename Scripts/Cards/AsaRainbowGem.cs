using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using RasForSts2.Scripts.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

[RegisterCard(typeof(XilaCardPool))]
public class AsaRainbowGem : XilaCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(3)
    ];

    public AsaRainbowGem() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<MegaCrit.Sts2.Core.Models.CardPoolModel> pools = Owner.UnlockState.CharacterCardPools.ToList();
        if (pools.Count > 1)
        {
            pools.Remove(Owner.Character.CardPool);
        }

        IEnumerable<MegaCrit.Sts2.Core.Models.CardModel> attackCards = from c in pools.SelectMany(pool => pool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint))
            where c.Type == CardType.Attack
            select c;

        List<MegaCrit.Sts2.Core.Models.CardModel> options = CardFactory.GetDistinctForCombat(Owner, attackCards, 3, Owner.RunState.Rng.CombatCardGeneration).ToList();

        if (IsUpgraded)
        {
            foreach (MegaCrit.Sts2.Core.Models.CardModel card in options)
            {
                CardCmd.Upgrade(card);
            }
        }

        MegaCrit.Sts2.Core.Models.CardModel? selectedCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner, canSkip: true);

        if (selectedCard != null)
        {
            selectedCard.SetToFreeThisCombat();
            await CardPileCmd.AddGeneratedCardToCombat(selectedCard, PileType.Hand, Owner);
        }

        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade() { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
}
