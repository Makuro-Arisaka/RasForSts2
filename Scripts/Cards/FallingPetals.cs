using System;
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
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 落花缤纷
/// spec: 1cost [普通攻击卡]
/// 造成8（10）点伤害，获得2（3）点活力。
/// </summary>
[RegisterCard(typeof(XilaCardPool))]
public class FallingPetals : XilaCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8m, ValueProp.Move),
        new DynamicVar("Vigor", 2m),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    public FallingPetals() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        // 造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 获得 2(3) 点活力
        decimal vigorAmount = DynamicVars["Vigor"].BaseValue;
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, vigorAmount, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级：伤害 8→10 (+2)，活力 2→3 (+1)
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["Vigor"].UpgradeValueBy(1m);
    }
}
