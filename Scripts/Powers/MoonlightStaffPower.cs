using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

[RegisterPower]
public sealed class MoonlightStaffPower : ModPowerTemplate
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override PowerAssetProfile AssetProfile => new(IconPath: "res://RasForSts2/images/powers/MoonlightStaffPower.png");

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();
}

/// <summary>
/// 月光法杖: 敌人身上的虚弱效果翻倍
/// WeakPower 默认使造成伤害 ×0.75 (减伤25%); 翻倍后 ×0.5 (减伤50%)
/// 公式: new = 2 * old - 1
/// 仅当玩家(拥有月光法杖)被敌人(带有虚弱)攻击时生效
/// </summary>
[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.ModifyDamageMultiplicative))]
public static class MoonlightStaffWeakPatch
{
	public static void Postfix(AbstractModel __instance, ref decimal __result, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (__instance is not WeakPower weakPower)
		{
			return;
		}

		if (!props.IsPoweredAttack())
		{
			return;
		}

		// WeakPower 仅在 dealer == Owner 时返回非 1.0 的乘数
		if (dealer != weakPower.Owner)
		{
			return;
		}

		if (target == null)
		{
			return;
		}

		// 只有玩家拥有月光法杖时, 敌人身上的虚弱才翻倍
		if (target.GetPower<MoonlightStaffPower>() == null)
		{
			return;
		}

		// 翻倍虚弱效果: 0.75 -> 0.5
		__result = 2m * __result - 1m;
	}
}

/// <summary>
/// 月光法杖: 敌人身上的易伤效果翻倍
/// VulnerablePower 默认使受到伤害 ×1.5 (增伤50%); 翻倍后 ×2.0 (增伤100%)
/// 公式: new = 2 * old - 1
/// 仅当玩家(拥有月光法杖)攻击敌人(带有易伤)时生效
/// </summary>
[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.ModifyDamageMultiplicative))]
public static class MoonlightStaffVulnerablePatch
{
	public static void Postfix(AbstractModel __instance, ref decimal __result, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (__instance is not VulnerablePower vulnerablePower)
		{
			return;
		}

		if (!props.IsPoweredAttack())
		{
			return;
		}

		// VulnerablePower 仅在 target == Owner 时返回非 1.0 的乘数
		if (target != vulnerablePower.Owner)
		{
			return;
		}

		if (dealer == null)
		{
			return;
		}

		// 只有玩家拥有月光法杖时, 敌人身上的易伤才翻倍
		if (dealer.GetPower<MoonlightStaffPower>() == null)
		{
			return;
		}

		// 翻倍易伤效果: 1.5 -> 2.0
		__result = 2m * __result - 1m;
	}
}
