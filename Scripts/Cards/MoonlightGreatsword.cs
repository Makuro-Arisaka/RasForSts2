using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Commands;
using RasForSts2.Scripts.Helpers;
using RasForSts2.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

[RegisterCharacterStarterCard(typeof(XilaCharacter), 1, Order = 3)]
public class MoonlightGreatsword : XilaCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Basic;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	public MoonlightGreatsword() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得护卫（升级不增加护卫值，只降费用）
        decimal guardAmount = 3m;
        await PowerCmd.Apply<GuardPower>(choiceContext, Owner.Creature, guardAmount, Owner.Creature, this);

        // 若玩家身上有 QueenHarpPower，则不能切换女王武具，仅获得护卫
        if (!QueenWeaponCmd.CanSwitchWeapon(Owner))
        {
            return;
        }

        // 切换女王武具：移除旧Power + 应用新Power + 通知变更 + 回手疾风连拳
        await QueenWeaponCmd.SwitchWeapon<MoonlightGreatswordPower>(choiceContext, Owner, this);
    }

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	public override IEnumerable<CardKeyword> CanonicalKeywords => Array.Empty<CardKeyword>();

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
		QueenWeaponHoverTip.Create(),
		HoverTipFactory.FromPower<GuardPower>(),
	];
}