using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Resources;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

/// <summary>
/// 法咒连刺
/// spec: 1cost [普通攻击卡]
/// 造成2次4（5）点伤害，获得1（2）黑暗法咒。
/// </summary>
[RegisterCard(typeof(XilaCardPool))]
public class SpellThrusts : XilaCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4m, ValueProp.Move),
        SecondaryResourceVars.ForLocal("DarkCurse", Entry.ModId, "dark_curse", 1m),
    ];

    public SpellThrusts() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        // 造成2次4(5)点伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(2)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_dramatic_stab")
            .Execute(choiceContext);

        // 获得 1(2) 黑暗法咒
        int curseAmount = (int)DynamicVars["DarkCurse"].BaseValue;
        await DarkCurseResource.Gain(Owner, curseAmount, this);
    }

    protected override void OnUpgrade()
    {
        // 升级：伤害 4→5 (+1)，黑暗法咒 1→2 (+1)
        DynamicVars.Damage.UpgradeValueBy(1m);
        DynamicVars["DarkCurse"].UpgradeValueBy(1m);
    }
}
