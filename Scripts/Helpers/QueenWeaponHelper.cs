using System;
using MegaCrit.Sts2.Core.Entities.Players;

namespace RasForSts2.Scripts.Helpers;

public static class QueenWeaponHelper
{
    /// <summary>
    /// 女王武具切换事件。
    /// 参数：Player player, Type? oldWeaponType, Type? newWeaponType
    /// </summary>
    public static event Action<Player, Type?, Type?>? OnQueenWeaponChanged;

    public static void NotifyWeaponChanged(Player player, Type? oldWeaponType, Type? newWeaponType)
    {
        OnQueenWeaponChanged?.Invoke(player, oldWeaponType, newWeaponType);
    }
}
