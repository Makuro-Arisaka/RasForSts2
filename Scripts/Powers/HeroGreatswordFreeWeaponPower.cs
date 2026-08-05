using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using RasForSts2.Scripts.Commands;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

/// <summary>
/// 英雄大剑：下一张女王武器牌可以免费打出（0费），打出后移除。
/// 参考原版 FreeAttackPower（TryModifyEnergyCostInCombat + BeforeCardPlayed 消耗）。
/// </summary>
[RegisterPower]
public sealed class HeroGreatswordFreeWeaponPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://RasForSts2/images/powers/placeholder.png",
        BigIconPath: "res://RasForSts2/images/powers/placeholder.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        if (card.Owner.Creature != Owner)
        {
            return false;
        }
        if (!QueenWeaponCmd.IsQueenWeaponCard(card))
        {
            return false;
        }

        bool inHandOrPlay;
        switch (card.Pile?.Type)
        {
            case PileType.Hand:
            case PileType.Play:
                inHandOrPlay = true;
                break;
            default:
                inHandOrPlay = false;
                break;
        }
        if (!inHandOrPlay)
        {
            return false;
        }

        modifiedCost = 0m;
        return true;
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner.Player && QueenWeaponCmd.IsQueenWeaponCard(cardPlay.Card))
        {
            Log.Info($"[HeroGreatswordFreeWeapon] Free queen weapon played: {cardPlay.Card.Id.Entry}, removing self.");
            await PowerCmd.Remove(this);
        }
    }
}
