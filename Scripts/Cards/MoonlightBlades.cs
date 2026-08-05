using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Commands;
using RasForSts2.Scripts.Helpers;
using RasForSts2.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

[RegisterCard(typeof(XilaCardPool))]
public class MoonlightBlades : XilaCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public MoonlightBlades() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal guardAmount = DynamicVars["Guard"].BaseValue;
        await PowerCmd.Apply<GuardPower>(choiceContext, Owner.Creature, guardAmount, Owner.Creature, this);

        if (!QueenWeaponCmd.CanSwitchWeapon(Owner))
        {
            return;
        }

        await QueenWeaponCmd.SwitchWeapon<MoonlightBladesPower>(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        DynamicVars["Guard"].UpgradeValueBy(2m);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("Guard", 2m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        QueenWeaponHoverTip.Create(),
        HoverTipFactory.FromPower<GuardPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
    ];
}
