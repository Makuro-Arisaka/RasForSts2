using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Resources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 恐慌！！
/// spec: 2cost [稀有攻击卡]
/// 对所有敌人造成6（9）点伤害，获得造成伤害的黑暗法咒层数。给予所有敌人1层虚弱。消耗。
/// </summary>
[RegisterCard(typeof(XilaCardPool))]
public class Panic : XilaCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6m, ValueProp.Move),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    public Panic() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", Owner.Character.AttackAnimDelay);

        // 对所有敌人造成6(9)点伤害，并统计实际造成的总伤害
        AttackCommand attackCommand = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents((MegaCrit.Sts2.Core.Combat.CombatState)CombatState)
            .Execute(choiceContext);

        // 统计实际造成的伤害（UnblockedDamage，不含被格挡/溢出部分，与原版"造成伤害"口径一致）
        int totalDamage = attackCommand.Results.SelectMany(results => results).Sum(r => r.UnblockedDamage);
        if (totalDamage > 0)
        {
            // 获得造成伤害的黑暗法咒层数
            await DarkCurseResource.Gain(Owner, totalDamage, this);
        }

        // 给予所有敌人1层虚弱
        foreach (var enemy in CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, 1m, Owner.Creature, this);
        }

        // 消耗
        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        // 升级：伤害 6→9 (+3)
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
