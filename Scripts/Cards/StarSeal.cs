using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Powers;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 星辰封印
/// spec: 1cost [罕见能力牌]
/// 回合开始时，获得1点能量并获得1（2）点活力。清空你的护卫层数。
/// </summary>
[RegisterCard(typeof(XilaCardPool))]
public class StarSeal : XilaCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),
        new DynamicVar("Vigor", 1m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<GuardPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
        EnergyHoverTip,
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    public StarSeal() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 打出时立即清空护卫层数
        GuardPower? guardPower = Owner.Creature.GetPower<GuardPower>();
        if (guardPower != null)
        {
            int previousGuard = guardPower.GuardAmount;
            guardPower.SetAmount(0);
            Log.Info($"[StarSeal] Cleared Guard stacks on play: {previousGuard} -> 0");
        }

        // DynamicVar BaseValue 已被 OnUpgrade.UpgradeValueBy 修改过
        // 直接读 BaseValue 就是当前活力值（1未升级 / 2升级）
        decimal vigorAmount = DynamicVars["Vigor"].BaseValue;
        await PowerCmd.Apply<StarSealPower>(choiceContext, Owner.Creature, vigorAmount, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级：活力 1 → 2
        DynamicVars["Vigor"].UpgradeValueBy(1m);
    }
}
