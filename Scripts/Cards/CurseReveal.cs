using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using RasForSts2.Scripts.Helpers;
using RasForSts2.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

[RegisterCard(typeof(TokenCardPool))]
public class CurseReveal : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://RasForSts2/images/cards/{GetType().Name}.png"
    );

    private const int energyCost = 0;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    public CurseReveal() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DoomPower>(choiceContext, Owner.Creature, 999m, Owner.Creature, this);

        Owner.PlayerCombatState?.ResetEnergy();

        await PowerCmd.Apply<CurseRevealPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade() { }

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    public override IEnumerable<CardKeyword> CanonicalKeywords => Array.Empty<CardKeyword>();

    public override async Task OnEnqueuePlayVfx(Creature? target)
    {
        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NPowerUpVfx.CreateNormal(Owner.Creature));
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        QueenWeaponHoverTip.Create(),
        HoverTipFactory.FromPower<DoomPower>(),
    ];
}
