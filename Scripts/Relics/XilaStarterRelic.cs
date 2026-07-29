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
	private bool _hasPlayedQueenWeaponThisCombat;

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

	protected override IEnumerable<DynamicVar> CanonicalVars => new[]
	{
		new EnergyVar(1),
		new DynamicVar("Guard", 1m)
	};

	public override Task BeforeCombatStart()
	{
		_hasPlayedQueenWeaponThisCombat = false;
		Log.Info($"[XilaStarterRelic] BeforeCombatStart: reset per-combat flag");
		return Task.CompletedTask;
	}

	public override async Task BeforeCombatStartLate()
	{
		Player? player = base.Owner;
		if (player == null)
		{
			Log.Info($"[XilaStarterRelic] BeforeCombatStartLate: player is null, skipping");
			return;
		}

		Log.Info($"[XilaStarterRelic] BeforeCombatStartLate: applying 1 guard");
		await PowerCmd.Apply<GuardPower>(null, player.Creature, 1m, player.Creature, null);
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (_hasPlayedQueenWeaponThisCombat)
		{
			return;
		}

		string cardId = cardPlay.Card.Id.Entry;
		if (QueenWeaponCardIds.Contains(cardId))
		{
			_hasPlayedQueenWeaponThisCombat = true;
			Log.Info($"[XilaStarterRelic] First queen weapon card played this combat: {cardId}, gaining 1 energy");
			await PlayerCmd.GainEnergy(1m, cardPlay.Card.Owner);
		}
	}
}