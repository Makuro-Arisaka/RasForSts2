using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

[RegisterPower]
public sealed class GuardPower : ModPowerTemplate
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool AllowNegative => true;

	public override PowerAssetProfile AssetProfile => new(
		IconPath: $"res://RasForSts2/images/powers/GuardPower.png",
		BigIconPath: $"res://RasForSts2/images/powers/GuardPower.png"
	);

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	private int _blockBeforeGuard = 0;
	private int _totalEnemyAttackIntent = 0;
	private bool _isEnemyTurn = false;

	public int GuardAmount => Math.Max(0, base.Amount);

	public override int DisplayAmount => GuardAmount;

	public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
	{
		Log.Info($"[GuardPower] === BeforeApplied START ===");
		Log.Info($"[GuardPower] Target: {target?.Name ?? "null"}, Applier: {applier?.Name ?? "null"}, CardSource: {(cardSource != null ? cardSource.Id.ToString() : "null")}");
		Log.Info($"[GuardPower] Amount to add: {amount}, Current base.Amount: {base.Amount}, Current GuardAmount: {GuardAmount}");

		await base.BeforeApplied(target, amount, applier, cardSource);

		Log.Info($"[GuardPower] After base.BeforeApplied - base.Amount: {base.Amount}, GuardAmount: {GuardAmount}");
		Log.Info($"[GuardPower] === BeforeApplied END ===");
	}

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		Log.Info($"[GuardPower] === AfterApplied START ===");
		Log.Info($"[GuardPower] Owner: {base.Owner?.Name ?? "null"}, Applier: {applier?.Name ?? "null"}");
		Log.Info($"[GuardPower] Current base.Amount: {base.Amount}, GuardAmount: {GuardAmount}, DisplayAmount: {DisplayAmount}");

		await base.AfterApplied(applier, cardSource);

		CombatManager.Instance.TurnStarted += OnTurnStarted;
		CombatManager.Instance.TurnEnded += OnTurnEnded;
		Log.Info($"[GuardPower] Subscribed to TurnStarted and TurnEnded events");
		Log.Info($"[GuardPower] === AfterApplied END ===");
	}

	public override async Task AfterRemoved(Creature oldOwner)
	{
		Log.Info($"[GuardPower] === AfterRemoved START ===");
		Log.Info($"[GuardPower] OldOwner: {oldOwner?.Name ?? "null"}, Current guard: {GuardAmount}");

		await base.AfterRemoved(oldOwner);

		CombatManager.Instance.TurnStarted -= OnTurnStarted;
		CombatManager.Instance.TurnEnded -= OnTurnEnded;
		Log.Info($"[GuardPower] Unsubscribed from TurnStarted and TurnEnded events");
		Log.Info($"[GuardPower] === AfterRemoved END ===");
	}

	private async void OnTurnStarted(CombatState state)
	{
		Log.Info($"[GuardPower] === OnTurnStarted START ===");
		Log.Info($"[GuardPower] CurrentSide: {state.CurrentSide}, Round: {state.RoundNumber}");
		Log.Info($"[GuardPower] Current guard: {GuardAmount}, base.Amount: {base.Amount}, isEnemyTurn: {_isEnemyTurn}");

		// Check if this is the active GuardPower instance on the owner
		// (prevents old instances from previous combats from firing)
		Creature? owner = base.Owner;
		if (owner == null || owner.GetPower<GuardPower>() != this)
		{
			Log.Info($"[GuardPower] Stale instance detected (Owner={owner?.Name ?? "null"}), unsubscribing");
			CombatManager.Instance.TurnStarted -= OnTurnStarted;
			CombatManager.Instance.TurnEnded -= OnTurnEnded;
			Log.Info($"[GuardPower] === OnTurnStarted END (stale instance) ===");
			return;
		}

		if (state.CurrentSide == CombatSide.Enemy)
		{
			Log.Info($"[GuardPower] Enemy turn detected");
			_isEnemyTurn = true;
			_blockBeforeGuard = base.Owner.Block;
			_totalEnemyAttackIntent = CalculateTotalEnemyAttackIntent(state);

			Log.Info($"[GuardPower] Recorded blockBeforeGuard={_blockBeforeGuard} (player's block before guard)");
			Log.Info($"[GuardPower] Calculated totalEnemyAttackIntent={_totalEnemyAttackIntent} (sum of all enemy attack intents)");

			if (GuardAmount > 0)
			{
				Log.Info($"[GuardPower] About to add {GuardAmount} block from guard, current owner block: {base.Owner.Block}");
				await CreatureCmd.GainBlock(base.Owner, new BlockVar(GuardAmount, ValueProp.Unpowered), null);
				Log.Info($"[GuardPower] Block added successfully, total block now: {base.Owner.Block}, guard remains: {GuardAmount}");
			}
			else
			{
				Log.Info($"[GuardPower] GuardAmount <= 0, skipping block addition");
			}
		}
		else if (state.CurrentSide == CombatSide.Player)
		{
			Log.Info($"[GuardPower] Player turn detected, resetting tracking state");
			bool wasEnemyTurn = _isEnemyTurn;
			int previousBlockBeforeGuard = _blockBeforeGuard;
			int previousTotalIntent = _totalEnemyAttackIntent;

			_isEnemyTurn = false;
			_blockBeforeGuard = 0;
			_totalEnemyAttackIntent = 0;

			Log.Info($"[GuardPower] Reset state: isEnemyTurn={_isEnemyTurn} (was {wasEnemyTurn})");
			Log.Info($"[GuardPower] Cleared: blockBeforeGuard={_blockBeforeGuard} (was {previousBlockBeforeGuard}), totalEnemyAttackIntent={_totalEnemyAttackIntent} (was {previousTotalIntent})");
			Log.Info($"[GuardPower] Current guard entering player turn: {GuardAmount}, base.Amount: {base.Amount}");
		}
		else
		{
			Log.Info($"[GuardPower] Unknown turn side: {state.CurrentSide}, no action taken");
		}

		Log.Info($"[GuardPower] === OnTurnStarted END ===");
	}

	private void OnTurnEnded(CombatState state)
	{
		Log.Info($"[GuardPower] === OnTurnEnded START ===");
		Log.Info($"[GuardPower] CurrentSide: {state.CurrentSide}, Round: {state.RoundNumber}");

		// Check if this is the active GuardPower instance on the owner
		Creature? owner = base.Owner;
		if (owner == null || owner.GetPower<GuardPower>() != this)
		{
			Log.Info($"[GuardPower] Stale instance detected (Owner={owner?.Name ?? "null"}), unsubscribing");
			CombatManager.Instance.TurnStarted -= OnTurnStarted;
			CombatManager.Instance.TurnEnded -= OnTurnEnded;
			Log.Info($"[GuardPower] === OnTurnEnded END (stale instance) ===");
			return;
		}

		if (state.CurrentSide == CombatSide.Player)
		{
			Log.Info($"[GuardPower] Enemy turn just ended (switched to Player side)");
			Log.Info($"[GuardPower] Current state: GuardAmount={GuardAmount}, base.Amount={base.Amount}");
			Log.Info($"[GuardPower] Recorded values: blockBeforeGuard={_blockBeforeGuard}, totalEnemyAttackIntent={_totalEnemyAttackIntent}");

			if (GuardAmount <= 0)
			{
				Log.Info($"[GuardPower] GuardAmount <= 0, no guard to reduce");
				Log.Info($"[GuardPower] === OnTurnEnded END (no guard) ===");
				return;
			}

			int guardReduction = Math.Max(0, _totalEnemyAttackIntent - _blockBeforeGuard);
			int actualReduction = Math.Min(guardReduction, GuardAmount);
			int remainingGuard = GuardAmount - actualReduction;

			Log.Info($"[GuardPower] === Guard Reduction Calculation ===");
			Log.Info($"[GuardPower]   totalEnemyAttackIntent: {_totalEnemyAttackIntent}");
			Log.Info($"[GuardPower]   blockBeforeGuard:      {_blockBeforeGuard}");
			Log.Info($"[GuardPower]   guardReduction = max(0, {_totalEnemyAttackIntent} - {_blockBeforeGuard}) = {guardReduction}");
			Log.Info($"[GuardPower]   actualReduction = min({guardReduction}, {GuardAmount}) = {actualReduction}");
			Log.Info($"[GuardPower]   remainingGuard = {GuardAmount} - {actualReduction} = {remainingGuard}");

			if (actualReduction > 0)
			{
				if (remainingGuard > 0)
				{
					base.SetAmount(remainingGuard);
					Log.Info($"[GuardPower] Set guard amount to {remainingGuard} (internal base.Amount: {base.Amount})");
				}
				else
				{
					base.SetAmount(0);
					Log.Info($"[GuardPower] Guard depleted! Setting amount to 0 (AllowNegative=true prevents auto-removal)");
					Log.Info($"[GuardPower] DisplayAmount will show: {Math.Max(0, 0)}");
				}
			}
			else
			{
				Log.Info($"[GuardPower] No guard reduction needed (enemy damage fully absorbed by player's own block)");
			}

			Log.Info($"[GuardPower] Final state after guard reduction:");
			Log.Info($"[GuardPower]   - GuardAmount: {GuardAmount}");
			Log.Info($"[GuardPower]   - base.Amount: {base.Amount}");
			Log.Info($"[GuardPower]   - DisplayAmount: {DisplayAmount}");
		}
		else
		{
			Log.Info($"[GuardPower] Player turn just ended (switched to Enemy side), no action needed");
		}

		Log.Info($"[GuardPower] === OnTurnEnded END ===");
	}

	private int CalculateTotalEnemyAttackIntent(CombatState state)
	{
		int total = 0;
		Log.Info($"[GuardPower] === Calculating Total Enemy Attack Intent ===");

		foreach (Creature enemy in state.Enemies)
		{
			if (enemy.IsDead)
			{
				Log.Info($"[GuardPower]   Enemy {enemy.Name}: dead, skipping");
				continue;
			}

			if (enemy.Monster == null)
			{
				Log.Info($"[GuardPower]   Enemy {enemy.Name}: no monster, skipping");
				continue;
			}

			IReadOnlyList<AbstractIntent> intents = enemy.Monster.NextMove.Intents;
			int enemyTotalDamage = 0;

			foreach (AbstractIntent intent in intents)
			{
				if (intent is AttackIntent attackIntent)
				{
					int damage = attackIntent.GetTotalDamage(state.Allies, enemy);
					enemyTotalDamage += damage;
					Log.Info($"[GuardPower]   Enemy {enemy.Name}: {intent.GetType().Name} damage={damage}");
				}
				else
				{
					Log.Info($"[GuardPower]   Enemy {enemy.Name}: {intent.GetType().Name} (non-attack, skipped)");
				}
			}

			Log.Info($"[GuardPower]   Enemy {enemy.Name} total attack damage: {enemyTotalDamage}");
			total += enemyTotalDamage;
		}

		Log.Info($"[GuardPower] === Total Enemy Attack Intent: {total} ===");
		return total;
	}
}