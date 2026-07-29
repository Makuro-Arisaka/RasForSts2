using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using RasForSts2.Scripts.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Potions;

[RegisterPotion(typeof(GenericPotionPool))]
public sealed class QueenCursePotion : ModPotionTemplate
{
    public override PotionRarity Rarity => PotionRarity.Common;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.AnyEnemy;

    public override PotionAssetProfile AssetProfile => new(
        ImagePath: "res://RasForSts2/images/potions/QueenCursePotion.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<ArtifactPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<PoisonPower>(),
        HoverTipFactory.FromPower<DoomPower>(),
    ];

    // spec: 去除一名敌人的人工制品，并给予一层易伤，一层虚弱，一层中毒，一层灾厄
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);

        if (target.HasPower<ArtifactPower>())
        {
            await PowerCmd.Remove<ArtifactPower>(target);
        }

        await PowerCmd.Apply<VulnerablePower>(choiceContext, target, 1m, Owner.Creature, null);
        await PowerCmd.Apply<WeakPower>(choiceContext, target, 1m, Owner.Creature, null);
        await PowerCmd.Apply<PoisonPower>(choiceContext, target, 1m, Owner.Creature, null);
        await PowerCmd.Apply<DoomPower>(choiceContext, target, 1m, Owner.Creature, null);
    }
}
