using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Rooms;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Helpers;
using RasForSts2.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Relics;

[RegisterRelic(typeof(XilaRelicPool))]
public class BeatBoots : ModRelicTemplate
{
    private bool _hasTriggeredThisCombat;
    private bool _isSubscribed;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://RasForSts2/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"res://RasForSts2/images/relics/{GetType().Name}.png",
        BigIconPath: $"res://RasForSts2/images/relics/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new CardsVar(1),
        new DynamicVar("Guard", 2m)
    };

    public override Task BeforeCombatStart()
    {
        Log.Info($"[BeatBoots] BeforeCombatStart: resetting state and subscribing");
        _hasTriggeredThisCombat = false;

        if (_isSubscribed)
        {
            QueenWeaponHelper.OnQueenWeaponChanged -= OnQueenWeaponChanged;
            _isSubscribed = false;
        }

        QueenWeaponHelper.OnQueenWeaponChanged += OnQueenWeaponChanged;
        _isSubscribed = true;
        Log.Info($"[BeatBoots] Subscribed to OnQueenWeaponChanged");

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        Log.Info($"[BeatBoots] AfterCombatEnd: unsubscribing");
        Unsubscribe();
        return Task.CompletedTask;
    }

    private void Unsubscribe()
    {
        if (_isSubscribed)
        {
            QueenWeaponHelper.OnQueenWeaponChanged -= OnQueenWeaponChanged;
            _isSubscribed = false;
            Log.Info($"[BeatBoots] Unsubscribed from OnQueenWeaponChanged");
        }
    }

    private async void OnQueenWeaponChanged(Player player, Type? oldWeaponType, Type? newWeaponType)
    {
        try
        {
            if (player == null)
            {
                Log.Info($"[BeatBoots] OnQueenWeaponChanged: player is null, skipping");
                return;
            }

            Player? owner = base.Owner;
            if (owner == null)
            {
                Log.Info($"[BeatBoots] Owner is null, unsubscribing");
                Unsubscribe();
                return;
            }

            if (player != owner)
            {
                return;
            }

            if (owner.GetRelic<BeatBoots>() != this)
            {
                Log.Info($"[BeatBoots] Stale instance detected (Owner={owner.Creature?.Name ?? "null"}), unsubscribing");
                Unsubscribe();
                return;
            }

            if (owner.Creature?.CombatState == null)
            {
                Log.Info($"[BeatBoots] Not in combat, skipping (defense-in-depth)");
                return;
            }

            if (_hasTriggeredThisCombat)
            {
                Log.Info($"[BeatBoots] Already triggered this combat, skipping");
                return;
            }

            _hasTriggeredThisCombat = true;
            Log.Info($"[BeatBoots] === Triggering effect ===");
            Log.Info($"[BeatBoots] Weapon switch: {oldWeaponType?.Name ?? "null"} -> {newWeaponType?.Name ?? "null"}");

            Flash();
            Creature creature = player.Creature;

            int drawCount = base.DynamicVars.Cards.IntValue;
            Log.Info($"[BeatBoots] Drawing {drawCount} card(s)");
            await CardPileCmd.Draw(null, drawCount, player);

            decimal guardAmount = base.DynamicVars["Guard"].BaseValue;
            Log.Info($"[BeatBoots] Applying {guardAmount} Guard");
            await PowerCmd.Apply<GuardPower>(null, creature, guardAmount, creature, null);

            Log.Info($"[BeatBoots] === Effect triggered successfully ===");
        }
        catch (Exception ex)
        {
            Log.Info($"[BeatBoots] ERROR in OnQueenWeaponChanged: {ex}");
        }
    }
}
