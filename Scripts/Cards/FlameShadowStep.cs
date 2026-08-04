using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using RasForSts2.Scripts.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 焰影步
/// spec: 0cost [罕见技能卡]
/// 获得2（3）点能量。将2张灼伤放入手牌。
/// </summary>
[RegisterCard(typeof(XilaCardPool))]
public class FlameShadowStep : XilaCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(2),
        new DynamicVar("BurnCount", 2m),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromCard<MegaCrit.Sts2.Core.Models.Cards.Burn>(IsUpgraded),
        EnergyHoverTip,  // 显示能量图标悬浮提示
    ];

    public FlameShadowStep() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 1. 获得 2(3) 点能量（临时，用完就没了）
        decimal energyAmount = DynamicVars.Energy.BaseValue;
        await PlayerCmd.GainEnergy(energyAmount, Owner);

        // 2. 将 2 张灼伤放入手牌
        int burnCount = (int)DynamicVars["BurnCount"].BaseValue;
        for (int i = 0; i < burnCount; i++)
        {
            var burn = Owner.Creature.CombatState.CreateCard<MegaCrit.Sts2.Core.Models.Cards.Burn>(Owner);
            if (burn != null)
            {
                await CardPileCmd.AddGeneratedCardToCombat(burn, PileType.Hand, Owner);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 升级：能量 2→3
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}
