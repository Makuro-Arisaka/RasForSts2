using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using RasForSts2.Scripts.Helpers;
using RasForSts2.Scripts.Powers;

namespace RasForSts2.Scripts.Commands;

public static class QueenWeaponCmd
{
    /// <summary>
    /// 检查玩家是否可以切换女王武具。
    /// Per spec: 打出女王竖琴·苍白的安魂曲后，你不能再切换女王武具。
    /// </summary>
    public static bool CanSwitchWeapon(Player player)
    {
        bool canSwitch = player.Creature.GetPower<QueenHarpPower>() == null;
        if (!canSwitch)
        {
            Log.Info($"[QueenWeaponCmd] Cannot switch Queen Weapon: QueenHarpPower is active");
        }
        return canSwitch;
    }

    /// <summary>
    /// 切换女王武具（不使用召唤系统）。
    /// Per spec (Ras兔兔英雄希拉.txt#L77): 打出女王武具牌时,先移除当前的女王武具Power,再获得打出的女王武具牌对应的女王武具Power
    /// 同时触发武器变更通知和疾风连拳回手逻辑。
    /// </summary>
    public static async Task SwitchWeapon<TPower>(
        PlayerChoiceContext choiceContext,
        Player player,
        CardModel cardSource
    ) where TPower : PowerModel
    {
        Log.Info($"[QueenWeaponCmd] === SwitchWeapon START === Player={player.Creature.Name}, NewPower={typeof(TPower).Name}");

        Type? oldWeaponType = GetCurrentWeaponPowerType(player);
        Log.Info($"[QueenWeaponCmd] OldWeaponType: {oldWeaponType?.Name ?? "null"}");

        await RemoveExistingWeaponPowers(player);
        await PowerCmd.Apply<TPower>(choiceContext, player.Creature, 1m, player.Creature, cardSource);
        Log.Info($"[QueenWeaponCmd] Applied new weapon power: {typeof(TPower).Name}");

        Type? newWeaponType = typeof(TPower);
        Log.Info($"[QueenWeaponCmd] Notifying weapon change: Old={oldWeaponType?.Name ?? "null"}, New={newWeaponType.Name}");
        QueenWeaponHelper.NotifyWeaponChanged(player, oldWeaponType, newWeaponType);
        Log.Info($"[QueenWeaponCmd] Weapon change notification sent");

        Log.Info($"[QueenWeaponCmd] === WindFist Return START ===");
        await ReturnWindFistToHand(player);
        Log.Info($"[QueenWeaponCmd] === WindFist Return END ===");

        Log.Info($"[QueenWeaponCmd] === SwitchWeapon END ===");
    }

    private static Type? GetCurrentWeaponPowerType(Player player)
    {
        Creature creature = player.Creature;
        if (creature.GetPower<MoonlightGreatswordPower>() != null) return typeof(MoonlightGreatswordPower);
        if (creature.GetPower<MoonlightShieldPower>() != null) return typeof(MoonlightShieldPower);
        if (creature.GetPower<MoonlightStaffPower>() != null) return typeof(MoonlightStaffPower);
        if (creature.GetPower<MoonlightBladesPower>() != null) return typeof(MoonlightBladesPower);
        return null;
    }

    /// <summary>
    /// Remove all existing Queen Weapon powers from the player before applying a new one.
    /// Per spec: 打出女王武具牌时,先移除当前的女王武具Power,再获得打出的女王武具牌对应的女王武具Power
    /// 例外：若玩家身上有 JadeRabbitMochiPower（玉兔软年糕），则跳过移除，允许多武具共存。
    /// </summary>
    public static async Task RemoveExistingWeaponPowers(Player player)
    {
        Creature creature = player.Creature;

        // 玉兔软年糕：多武具共存，不移除旧武具
        if (creature.HasPower<JadeRabbitMochiPower>())
        {
            Log.Info($"[QueenWeaponCmd] JadeRabbitMochiPower active — skipping weapon removal (multi-weapon coexistence)");
            return;
        }

        Log.Info($"[QueenWeaponCmd] Removing existing Queen Weapon powers from {creature.Name}");
        await PowerCmd.Remove<MoonlightGreatswordPower>(creature);
        await PowerCmd.Remove<MoonlightShieldPower>(creature);
        await PowerCmd.Remove<MoonlightStaffPower>(creature);
        await PowerCmd.Remove<MoonlightBladesPower>(creature);
        Log.Info($"[QueenWeaponCmd] Removed existing Queen Weapon powers");
    }

    private static async Task ReturnWindFistToHand(Player summoner)
    {
        const string windFistCardId = "RAS_FOR_STS2_CARD_WIND_FIST";

        Log.Info($"[QueenWeaponCmd] ReturnWindFistToHand: Searching for cardId={windFistCardId}");

        var allCards = summoner.PlayerCombatState.AllCards;
        Log.Info($"[QueenWeaponCmd] Total cards in combat: {allCards.Count()}");

        var windFist = allCards.FirstOrDefault(c => c != null && c.Id.Entry == windFistCardId);

        if (windFist == null)
        {
            Log.Info($"[QueenWeaponCmd] WindFist NOT FOUND in any card pile");
            return;
        }

        Log.Info($"[QueenWeaponCmd] WindFist FOUND: Id={windFist.Id.Entry}, IsUpgraded={windFist.IsUpgraded}");

        CardPile handPile = PileType.Hand.GetPile(summoner);
        Log.Info($"[QueenWeaponCmd] Hand pile count: {handPile.Cards.Count()}");

        bool alreadyInHand = handPile.Cards.Contains(windFist);
        Log.Info($"[QueenWeaponCmd] WindFist already in hand: {alreadyInHand}");

        if (alreadyInHand)
        {
            Log.Info($"[QueenWeaponCmd] WindFist already in hand, skipping");
            return;
        }

        await CardPileCmd.Add(windFist, PileType.Hand);
        Log.Info($"[QueenWeaponCmd] SUCCESS: WindFist returned to hand (upgraded: {windFist.IsUpgraded})");
    }
}
