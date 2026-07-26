using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Cards;
using RasForSts2.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Relics;

[RegisterRelic(typeof(XilaRelicPool))]
[RegisterCharacterStarterRelic(typeof(XilaCharacter))]
public class XilaStarterRelic : ModRelicTemplate
{
	private bool _hasPlayedQueenWeaponThisTurn;

	private static readonly HashSet<string> QueenWeaponCardIds = new()
	{
		"RAS_FOR_STS2_CARD_MOONLIGHT_GREATSWORD",
		"RAS_FOR_STS2_CARD_MOONLIGHT_SHIELD",
		"RAS_FOR_STS2_CARD_MOONLIGHT_STAFF",
		"RAS_FOR_STS2_CARD_MOONLIGHT_BLADES"
	};

	public override RelicRarity Rarity => RelicRarity.Starter;

	public override RelicAssetProfile AssetProfile => new(
		IconPath: $"res://RasForSts2/images/relics/{GetType().Name}.png",
		IconOutlinePath: $"res://RasForSts2/images/relics/{GetType().Name}.png",
		BigIconPath: $"res://RasForSts2/images/relics/{GetType().Name}.png"
	);

	public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
	{
		_hasPlayedQueenWeaponThisTurn = false;
		Log.Info($"[XilaStarterRelic] AfterPlayerTurnStartEarly for {player.Creature.Name}, round={player.Creature.CombatState?.RoundNumber ?? 0}");
		
		var guardPower = player.Creature.GetPower<GuardPower>();
		if (guardPower == null)
		{
			Log.Info($"[XilaStarterRelic] GuardPower not found, applying 1");
			await PowerCmd.Apply<GuardPower>(choiceContext, player.Creature, 1m, player.Creature, null);
		}
		else
		{
			Log.Info($"[XilaStarterRelic] GuardPower exists with amount={guardPower.GuardAmount}, adding 1");
			await PowerCmd.ModifyAmount(null, guardPower, 1m, null, null);
		}
		
		guardPower = player.Creature.GetPower<GuardPower>();
		Log.Info($"[XilaStarterRelic] GuardPower after operation: {guardPower?.GuardAmount ?? 0}");
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (_hasPlayedQueenWeaponThisTurn)
		{
			return;
		}

		string cardId = cardPlay.Card.Id.Entry;
		if (QueenWeaponCardIds.Contains(cardId))
		{
			_hasPlayedQueenWeaponThisTurn = true;
			await PlayerCmd.GainEnergy(1m, cardPlay.Card.Owner);
		}
	}
}