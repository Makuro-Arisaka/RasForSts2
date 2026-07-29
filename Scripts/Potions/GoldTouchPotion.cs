using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Potions;

[RegisterPotion(typeof(GenericPotionPool))]
public sealed class GoldTouchPotion : ModPotionTemplate
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    public override PotionAssetProfile AssetProfile => new(
        ImagePath: "res://RasForSts2/images/potions/GoldTouchPotion.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<GoldTouchPower>(),
    ];

    // spec: 本场战斗结束时，额外获得掉落金币数量的100%金币
    // 使用药水时施加 GoldTouchPower，记录当前金币作为基准，战斗胜利时发放差额作为额外金币
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await PowerCmd.Apply<GoldTouchPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
    }
}
