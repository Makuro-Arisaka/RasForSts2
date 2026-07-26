using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
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
        // spec: 获得999层 灾厄（DoomPower，达到血量时直接死亡）
        await PowerCmd.Apply<DoomPower>(choiceContext, Owner.Creature, 999m, Owner.Creature, this);

        // spec: 将你的cost恢复至上限
        Owner.PlayerCombatState?.ResetEnergy();

        // spec: 获得: 攻击牌伤害翻倍, 技能牌格挡翻倍, 虚弱/易伤效果翻倍, 每打出1张牌抽1(2)张
        // 通过 CurseRevealPower 统一承载所有正面效果（不包含月光武具的负面效果）
        await PowerCmd.Apply<CurseRevealPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade() { }

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<DoomPower>(),
        HoverTipFactory.FromPower<CurseRevealPower>(),
    ];
}
