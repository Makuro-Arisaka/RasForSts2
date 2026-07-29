using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

[RegisterPower]
public sealed class HeroDeterminationPower : ModPowerTemplate
{
    private class Data
    {
        public int guardAmount;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(IconPath: "res://RasForSts2/images/powers/HeroDeterminationPower.png", BigIconPath: "res://RasForSts2/images/powers/HeroDeterminationPower.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("Guard", 10m)
    ];

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        // 首次应用：根据卡牌升级状态设置初始护卫值
        int initialAmount = cardSource != null && cardSource.IsUpgraded ? 15 : 10;
        GetInternalData<Data>().guardAmount = initialAmount;
        DynamicVars["Guard"].BaseValue = initialAmount;
    }

    /// <summary>
    /// Power 已存在时手动叠加：再次打出英雄决意时累加护卫值
    /// </summary>
    public void AddGuardAmount(int addValue)
    {
        int newAmount = GetInternalData<Data>().guardAmount + addValue;
        GetInternalData<Data>().guardAmount = newAmount;
        DynamicVars["Guard"].BaseValue = newAmount;
    }

    // spec: 每回合开始时,获得guardAmount点护卫
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await base.AfterPlayerTurnStart(choiceContext, player);

        if (player != Owner.Player)
        {
            return;
        }

        decimal guardAmount = GetInternalData<Data>().guardAmount;
        await PowerCmd.Apply<GuardPower>(choiceContext, Owner, guardAmount, Owner, null);
    }

    // spec: 当你打出攻击牌时，额外造成 当前护卫值 的伤害
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.AfterCardPlayed(choiceContext, cardPlay);

        if (cardPlay.Card.Owner.Creature == Owner && cardPlay.Card.Type == CardType.Attack)
        {
            GuardPower? guardPower = Owner.GetPower<GuardPower>();
            decimal bonusDamage = guardPower?.GuardAmount ?? 0m;

            if (bonusDamage > 0)
            {
                if (cardPlay.Card.TargetType == TargetType.AllEnemies)
                {
                    // 全体攻击：对所有敌人造成额外伤害
                    if (Owner.CombatState != null)
                    {
                        await DamageCmd.Attack(bonusDamage)
                            .FromCard(cardPlay.Card, cardPlay)
                            .TargetingAllOpponents(Owner.CombatState)
                            .Execute(choiceContext);
                    }
                }
                else if (cardPlay.Target != null)
                {
                    // 单体攻击：只对目标造成额外伤害
                    await DamageCmd.Attack(bonusDamage)
                        .FromCard(cardPlay.Card, cardPlay)
                        .Targeting(cardPlay.Target)
                        .Execute(choiceContext);
                }
            }
        }
    }
}
