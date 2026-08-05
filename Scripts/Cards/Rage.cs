using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Commands;
using RasForSts2.Scripts.Helpers;
using RasForSts2.Scripts.Powers;
using RasForSts2.Scripts.Resources;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 愤怒！！
/// spec: 0cost + 1点黑暗法咒 [普通技能卡]
/// 获得3（5）点格挡，如果你处于女王武具状态下，再获得5（6）点护卫。
/// </summary>
[RegisterCard(typeof(XilaCardPool))]
public class Rage : XilaCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(3, ValueProp.Move),
        new DynamicVar("Guard", 5m),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        QueenWeaponHoverTip.Create(),
        HoverTipFactory.FromPower<GuardPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
    ];

    public Rage() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        // 附加 1 点黑暗法咒固定费用
        SecondaryResourceCardExtensions.SecondaryCosts(this).Set(DarkCurseResource.ResourceId, new SecondaryResourceCost(1));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 获得3(5)点格挡
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, fast: false);

        // 如果处于女王武具状态下，再获得5(6)点护卫
        if (QueenWeaponCmd.IsInQueenWeaponState(Owner))
        {
            decimal guard = DynamicVars["Guard"].BaseValue;
            await PowerCmd.Apply<GuardPower>(choiceContext, Owner.Creature, guard, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级：格挡 3→5 (+2)，护卫 5→6 (+1)
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars["Guard"].UpgradeValueBy(1m);
    }
}
