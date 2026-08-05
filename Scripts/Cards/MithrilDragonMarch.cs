using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 秘银龙的进军
/// spec: 3cost(2cost) [稀有攻击牌]
/// 消耗所有手牌。每张手牌对所有敌人造成6点伤害。清空你的护卫层数。
/// </summary>
[RegisterCard(typeof(XilaCardPool))]
public class MithrilDragonMarch : XilaCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6m, ValueProp.Move)
    ];

    public MithrilDragonMarch() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获取当前手牌列表（此时本牌已移至 Play pile，不会被消耗）
        List<CardModel> handCards = PileType.Hand.GetPile(Owner).Cards.ToList();
        int cardCount = handCards.Count;

        // 消耗所有手牌
        foreach (CardModel card in handCards)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        // 每张手牌对所有敌人造成6点伤害（使用 WithHitCount 实现多次打击）
        if (cardCount > 0)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .WithHitCount(cardCount)
                .FromCard(this, cardPlay)
                .TargetingAllOpponents((MegaCrit.Sts2.Core.Combat.CombatState)CombatState)
                .Execute(choiceContext);
        }

        // 清空护卫层数
        GuardPower? guardPower = Owner.Creature.GetPower<GuardPower>();
        if (guardPower != null)
        {
            int previousGuard = guardPower.GuardAmount;
            guardPower.SetAmount(0);
            Log.Info($"[MithrilDragonMarch] Cleared Guard stacks: {previousGuard} -> 0");
        }
    }

    protected override void OnUpgrade()
    {
        // 升级：费用 3 → 2
        EnergyCost.UpgradeBy(-1);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<GuardPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];
}
