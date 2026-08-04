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
/// 反转术式
/// spec: 1cost + 4点黑暗法咒 [罕见攻击卡]
/// 对所有敌人施加1（2）层易伤，1（2）层虚弱，获得1（2）点力量。
/// </summary>
[RegisterCard(typeof(XilaCardPool))]
public class ReverseSpell : XilaCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<VulnerablePower>(1m),
        new PowerVar<WeakPower>(1m),
        new PowerVar<StrengthPower>(1m),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    public ReverseSpell() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        // 附加 4 点黑暗法咒固定费用
        SecondaryResourceCardExtensions.SecondaryCosts(this).Set(DarkCurseResource.ResourceId, new SecondaryResourceCost(4));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", Owner.Character.AttackAnimDelay);

        decimal vulnerable = DynamicVars["VulnerablePower"].BaseValue;
        decimal weak = DynamicVars["WeakPower"].BaseValue;
        decimal strength = DynamicVars["StrengthPower"].BaseValue;

        // 对所有敌人施加易伤和虚弱
        foreach (var enemy in CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, vulnerable, Owner.Creature, this);
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, weak, Owner.Creature, this);
        }

        // 获得力量
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, strength, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级：易伤/虚弱/力量 1→2 (+1)
        DynamicVars["VulnerablePower"].UpgradeValueBy(1m);
        DynamicVars["WeakPower"].UpgradeValueBy(1m);
        DynamicVars["StrengthPower"].UpgradeValueBy(1m);
    }
}
