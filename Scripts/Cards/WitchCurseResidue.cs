using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using RasForSts2.Scripts.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 魔女的邪咒残余
/// spec: 0cost [稀有能力牌]
/// 抽牌直至7(9)张牌。回复能量直至上限(上限的两倍)。获得13层灾厄、1层易伤、1层虚弱、1层脆弱。
/// </summary>
[RegisterCard(typeof(XilaCardPool))]
public class WitchCurseResidue : XilaCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    // 升级前后的目标手牌数
    private const int targetHandSize = 7;
    private const int targetHandSizeUpgraded = 9;
    // 灾厄层数
    private const int doomAmount = 13;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<DoomPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<FrailPower>(),
    ];

    public WitchCurseResidue() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 抽牌直至目标手牌数
        int target = IsUpgraded ? targetHandSizeUpgraded : targetHandSize;
        int currentHandCount = PileType.Hand.GetPile(Owner).Cards.Count;
        int cardsToDraw = target - currentHandCount;
        if (cardsToDraw > 0)
        {
            await CardPileCmd.Draw(choiceContext, cardsToDraw, Owner);
        }

        // 回复能量直至上限（升级后为上限的两倍）
        int maxEnergy = Owner.PlayerCombatState.MaxEnergy;
        int energyTarget = IsUpgraded ? maxEnergy * 2 : maxEnergy;
        int currentEnergy = Owner.PlayerCombatState.Energy;
        int energyToGain = energyTarget - currentEnergy;
        if (energyToGain > 0)
        {
            await PlayerCmd.GainEnergy(energyToGain, Owner);
        }

        // 获得13层灾厄
        await PowerCmd.Apply<DoomPower>(choiceContext, Owner.Creature, doomAmount, Owner.Creature, this);
        // 获得1层易伤、1层虚弱、1层脆弱
        await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await PowerCmd.Apply<WeakPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await PowerCmd.Apply<FrailPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级：抽牌 7→9，能量回复 上限→上限的两倍
        // 效果在 OnPlay 中根据 IsUpgraded 处理
    }
}
