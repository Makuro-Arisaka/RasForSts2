using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Resources;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 法咒吟唱
/// spec: 1cost [普通技能卡]
/// 获得2（3）点黑暗法咒。抽2张牌。
/// </summary>
[RegisterCard(typeof(XilaCardPool))]
public class SpellChant : XilaCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        SecondaryResourceVars.ForLocal("DarkCurse", Entry.ModId, "dark_curse", 2m),
        new DynamicVar("DrawCount", 2m),
    ];

    public SpellChant() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 1. 获得 2(3) 黑暗法咒
        int curseAmount = (int)DynamicVars["DarkCurse"].BaseValue;
        await DarkCurseResource.Gain(Owner, curseAmount, this);

        // 2. 抽 2 张牌
        int drawCount = (int)DynamicVars["DrawCount"].BaseValue;
        await CardPileCmd.Draw(choiceContext, drawCount, Owner);
    }

    protected override void OnUpgrade()
    {
        // 升级：黑暗法咒 2→3 (+1)
        DynamicVars["DarkCurse"].UpgradeValueBy(1m);
    }
}
