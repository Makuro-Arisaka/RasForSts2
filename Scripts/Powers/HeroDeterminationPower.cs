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
        public bool isUpgraded;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(IconPath: "res://RasForSts2/images/powers/HeroDeterminationPower.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        GetInternalData<Data>().isUpgraded = cardSource != null && cardSource.IsUpgraded;
    }

    // spec: 每回合开始时,获得10(15)点护卫
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await base.AfterPlayerTurnStart(choiceContext, player);

        if (player != Owner.Player)
        {
            return;
        }

        decimal guardAmount = GetInternalData<Data>().isUpgraded ? 15m : 10m;
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

            if (bonusDamage > 0 && cardPlay.Target != null)
            {
                await DamageCmd.Attack(bonusDamage)
                    .FromCard(cardPlay.Card, cardPlay)
                    .Targeting(cardPlay.Target)
                    .Execute(choiceContext);
            }
        }
    }
}
