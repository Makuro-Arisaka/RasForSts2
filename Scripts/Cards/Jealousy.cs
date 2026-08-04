using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Resources;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 嫉妒！！
/// spec: 1cost + 2点黑暗法咒 [罕见技能卡]
/// 指定一名敌人，如果敌人意图为攻击，抽2（3）张牌。否则获得1（2）点力量。
/// </summary>
[RegisterCard(typeof(XilaCardPool))]
public class Jealousy : XilaCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DrawCount", 2m),
        new PowerVar<StrengthPower>(1m),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    public Jealousy() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        // 附加 2 点黑暗法咒固定费用
        SecondaryResourceCardExtensions.SecondaryCosts(this).Set(DarkCurseResource.ResourceId, new SecondaryResourceCost(2));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 如果敌人意图为攻击，抽2(3)张牌；否则获得1(2)点力量
        if (cardPlay.Target?.Monster is { } monster && monster.IntendsToAttack)
        {
            int drawCount = (int)DynamicVars["DrawCount"].BaseValue;
            await CardPileCmd.Draw(choiceContext, drawCount, Owner);
        }
        else
        {
            decimal strength = DynamicVars["StrengthPower"].BaseValue;
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, strength, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级：抽牌 2→3 (+1)，力量 1→2 (+1)
        DynamicVars["DrawCount"].UpgradeValueBy(1m);
        DynamicVars["StrengthPower"].UpgradeValueBy(1m);
    }
}
