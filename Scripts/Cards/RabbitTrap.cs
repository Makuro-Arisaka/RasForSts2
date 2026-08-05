using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RasForSts2.Scripts.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

[RegisterCard(typeof(XilaCardPool))]
public class RabbitTrap : XilaCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<PoisonPower>(6m),
        new PowerVar<WeakPower>(1m)
    ];

    public RabbitTrap() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var enemy in CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<PoisonPower>(choiceContext, enemy, DynamicVars["PoisonPower"].BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, DynamicVars["WeakPower"].BaseValue, Owner.Creature, this);
        }
        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PoisonPower"].UpgradeValueBy(3m);
        DynamicVars["WeakPower"].UpgradeValueBy(1m);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<PoisonPower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];
}
