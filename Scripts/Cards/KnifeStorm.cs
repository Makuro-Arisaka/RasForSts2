using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.ValueProps;
using RasForSts2.Scripts.Characters;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

[RegisterCard(typeof(XilaCardPool))]
public class KnifeStorm : XilaCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(3)
    ];

    public KnifeStorm() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatState = (CombatState)CombatState;
        var adroitEnchantment = ModelDb.Enchantment<Adroit>();

        List<MegaCrit.Sts2.Core.Models.CardModel> shivs = [];
        for (int i = 0; i < 3; i++)
        {
            var shiv = combatState.CreateCard<Shiv>(Owner);
            CardCmd.Enchant(adroitEnchantment.ToMutable(), shiv, 5m);
            if (IsUpgraded) CardCmd.Upgrade(shiv);
            shivs.Add(shiv);
        }

        await CardPileCmd.AddGeneratedCardsToCombat(shivs, PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromCard<Shiv>(IsUpgraded),
        ..HoverTipFactory.FromEnchantment<Adroit>(),
    ];
}
