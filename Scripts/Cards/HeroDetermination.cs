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
using RasForSts2.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

[RegisterCard(typeof(TokenCardPool))]
public class HeroDetermination : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://RasForSts2/images/cards/{GetType().Name}.png"
    );

    private const int energyCost = 0;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    public HeroDetermination() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 如果 Power 已存在，手动叠加效果（PowerStackType.Single 时再次 Apply 不会调 AfterApplied）
        HeroDeterminationPower? existing = Owner.Creature.GetPower<HeroDeterminationPower>();
        if (existing != null)
        {
            // 再次打出时：按升级状态累加护卫值（10 或 15）
            int addValue = IsUpgraded ? 15 : 10;
            existing.AddGuardAmount(addValue);
        }
        else
        {
            await PowerCmd.Apply<HeroDeterminationPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
    }

    public override async Task OnEnqueuePlayVfx(Creature? target)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
    }

    protected override void OnUpgrade() { }

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    public override IEnumerable<CardKeyword> CanonicalKeywords => Array.Empty<CardKeyword>();

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<GuardPower>(),
    ];
}