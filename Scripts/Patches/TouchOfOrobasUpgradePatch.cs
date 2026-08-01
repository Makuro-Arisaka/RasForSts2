using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using RasForSts2.Scripts.Relics;

namespace RasForSts2.Scripts.Patches;

/// <summary>
/// Harmony Patch：将希拉的初始遗物 XilaStarterRelic 注入到 TouchOfOrobas（奥罗巴斯之触）的升级映射。
/// 
/// 原版机制：
///   - TouchOfOrobas.RefinementUpgrades 是一个静态硬编码 Dictionary，
///     键为初始遗物 ModelId，值为其升级后的遗物（如 BurningBlood → BlackBlood）。
///   - TouchOfOrobas.GetUpgradedStarterRelic(starter) 查询此字典；找不到（自定义角色）
///     时会回退到 Circlet（空饰品），导致 Xila 的初始遗物被替换成错的东西。
///   
/// 本补丁对 GetUpgradedStarterRelic 做 Postfix：当输入是 XilaStarterRelic 时，
/// 将返回结果覆盖为 HeroAwakening（英雄的觉悟），使其作为 Xila 的"初始遗物升级版"。
/// 
/// 注意：TouchOfOrobas.AfterObtained 实际使用时只取返回值的 .Id 再 ToMutable，
/// 所以此处返回 canonical 版本（与原版 TryGetValue 分支一致）即可。
/// </summary>
[HarmonyPatch(typeof(TouchOfOrobas), nameof(TouchOfOrobas.GetUpgradedStarterRelic))]
public static class TouchOfOrobas_UpgradeXilaStarter_Patch
{
    private static void Postfix(RelicModel starterRelic, ref RelicModel __result)
    {
        try
        {
            if (starterRelic == null)
            {
                return;
            }

            if (starterRelic is XilaStarterRelic)
            {
                Log.Info($"[TouchOfOrobas] Xila starter relic detected: {starterRelic.Id.Entry}, "
                       + $"replacing fallback result ({__result?.Id.Entry ?? "null"}) with HeroAwakening.");

                // 返回 canonical 版本：原版 RefinementUpgrades 中存的也是 ModelDb.Relic<T>()（canonical）
                __result = ModelDb.Relic<HeroAwakening>();
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[TouchOfOrobas_UpgradeXilaStarter_Patch] Patch failed: {ex}");
        }
    }
}
