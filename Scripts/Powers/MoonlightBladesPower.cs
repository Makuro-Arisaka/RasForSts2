using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

[RegisterPower]
public sealed class MoonlightBladesPower : ModPowerTemplate
{
	private class Data
	{
		public int drawAmount;
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override PowerAssetProfile AssetProfile => new(IconPath: "res://RasForSts2/images/powers/MoonlightBladesPower.png", BigIconPath: "res://RasForSts2/images/powers/MoonlightBladesPower.png");

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new("Draw", GetInternalData<Data>().drawAmount == 0 ? 1 : GetInternalData<Data>().drawAmount)
	];

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		await base.AfterApplied(applier, cardSource);
		GetInternalData<Data>().drawAmount = cardSource != null && cardSource.IsUpgraded ? 2 : 1;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner.Creature == base.Owner)
		{
			int drawAmount = GetInternalData<Data>().drawAmount;
			await CardPileCmd.Draw(choiceContext, drawAmount, base.Owner.Player);
		}
	}
}