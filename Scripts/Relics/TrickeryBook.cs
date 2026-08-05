using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using RasForSts2.Scripts.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Relics;

[RegisterRelic(typeof(GenericRelicPool))]
public class TrickeryBook : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://RasForSts2/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"res://RasForSts2/images/relics/{GetType().Name}.png",
        BigIconPath: $"res://RasForSts2/images/relics/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new DynamicVar("Doom", 1m),
        new DynamicVar("Poison", 1m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<DoomPower>(),
        HoverTipFactory.FromPower<PoisonPower>(),
    ];

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card.Owner != base.Owner)
        {
            return;
        }

        Creature? creature = base.Owner?.Creature;
        if (creature == null || creature.CombatState == null)
        {
            return;
        }

        List<Creature> enemies = creature.CombatState.GetOpponentsOf(creature)
            .Where(c => c.IsAlive)
            .ToList();

        if (enemies.Count == 0)
        {
            return;
        }

        Flash();

        decimal doomAmount = base.DynamicVars["Doom"].BaseValue;
        decimal poisonAmount = base.DynamicVars["Poison"].BaseValue;

        foreach (Creature enemy in enemies)
        {
            await PowerCmd.Apply<DoomPower>(null, enemy, doomAmount, creature, card);
            await PowerCmd.Apply<PoisonPower>(null, enemy, poisonAmount, creature, card);
        }
    }
}
