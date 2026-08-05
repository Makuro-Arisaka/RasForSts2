using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Resources;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 法咒飞刺
/// spec: 2cost + X点黑暗法咒 [罕见攻击卡]
/// 对所有敌人造成6（9）点伤害X次，获得5（8）点格挡X次。
/// X = 打出时消耗的黑暗法咒数量（RitsuLib X费用会消耗全部可用黑暗法咒）。
/// </summary>
[RegisterCard(typeof(XilaCardPool))]
public class SpellFlyingThorns : XilaCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6m, ValueProp.Move),
        new BlockVar(5, ValueProp.Move),
    ];

    public SpellFlyingThorns() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        // 附加 X 点黑暗法咒费用：打出时消耗全部可用黑暗法咒，X = 实际消耗数量
        SecondaryResourceCardExtensions.SecondaryCosts(this).Set(DarkCurseResource.ResourceId, SecondaryResourceCost.X());
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", Owner.Character.AttackAnimDelay);

        // 从支付 ledger 读取本次实际消耗的黑暗法咒数量（X 值）
        int x = cardPlay.SecondaryResources().Spent(DarkCurseResource.ResourceId);
        if (x <= 0)
            return;

        decimal damage = DynamicVars.Damage.BaseValue;

        for (int i = 0; i < x; i++)
        {
            // 对所有敌人造成6(9)点伤害
            await DamageCmd.Attack(damage)
                .FromCard(this, cardPlay)
                .TargetingAllOpponents((MegaCrit.Sts2.Core.Combat.CombatState)CombatState)
                .Execute(choiceContext);

            // 获得5(8)点格挡
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, fast: false);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级：伤害 6→9 (+3)，格挡 5→8 (+3)
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars.Block.UpgradeValueBy(3);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Block),
    ];
}
