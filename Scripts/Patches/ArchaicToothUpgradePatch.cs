using System;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using RasForSts2.Scripts.Cards;

namespace RasForSts2.Scripts.Patches;

/// <summary>
/// Harmony Patch：先古之牙(ArchaicTooth)支持希拉的初始卡「女王武具·月光大剑」→「女王武具·英雄大剑」变换。
///
/// 原版机制：
///   - ArchaicTooth.TranscendenceUpgrades 是静态硬编码字典，键为原版5个角色的初始卡 Id
///     （Bash→Break 等），GetTranscendenceStarterCard 在牌组中查找初始卡，
///     GetTranscendenceTransformedCard 按字典生成对应的 Ancient 卡。
///   - 月光大剑不在字典中：原版会把希拉的月光大剑变换成 Doubt（疑惑），必须拦截。
///
/// 两个补丁：
///   - GetTranscendenceStarterCard  Postfix：原版找不到初始卡时，在牌组中找月光大剑。
///   - GetTranscendenceTransformedCard Prefix：月光大剑 → 英雄大剑（继承升级/附魔）。
/// </summary>
[HarmonyPatch(typeof(ArchaicTooth), "GetTranscendenceStarterCard")]
public static class ArchaicTooth_FindMoonlightGreatsword_Patch
{
    private static void Postfix(Player player, ref CardModel? __result)
    {
        try
        {
            if (__result != null || player?.Deck == null)
            {
                return;
            }

            var moonlight = player.Deck.Cards.FirstOrDefault(c => c is MoonlightGreatsword);
            if (moonlight != null)
            {
                Log.Info($"[ArchaicTooth] Xila starter card detected: MoonlightGreatsword.");
                __result = moonlight;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[ArchaicTooth_FindMoonlightGreatsword_Patch] Patch failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(ArchaicTooth), "GetTranscendenceTransformedCard")]
public static class ArchaicTooth_MoonlightToHeroGreatsword_Patch
{
    private static bool Prefix(CardModel starterCard, ref CardModel __result)
    {
        try
        {
            if (starterCard is not MoonlightGreatsword)
            {
                return true; // 非月光大剑，走原版逻辑
            }

            CardModel heroGreatsword = starterCard.Owner.RunState.CreateCard<HeroGreatsword>(starterCard.Owner);
            if (starterCard.IsUpgraded)
            {
                CardCmd.Upgrade(heroGreatsword);
            }
            if (starterCard.Enchantment != null)
            {
                EnchantmentModel enchantment = (EnchantmentModel)starterCard.Enchantment.MutableClone();
                CardCmd.Enchant(enchantment, heroGreatsword, enchantment.Amount);
            }

            __result = heroGreatsword;
            Log.Info($"[ArchaicTooth] Transformed MoonlightGreatsword -> HeroGreatsword (upgraded={starterCard.IsUpgraded}).");
            return false; // 跳过原版
        }
        catch (Exception ex)
        {
            Log.Error($"[ArchaicTooth_MoonlightToHeroGreatsword_Patch] Patch failed: {ex}");
            return true;
        }
    }
}
