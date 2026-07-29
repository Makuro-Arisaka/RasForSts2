using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RasForSts2.Scripts.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

[RegisterPower]
public sealed class MoonlightGreatswordPower : ModPowerTemplate
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override PowerAssetProfile AssetProfile => new(IconPath: "res://RasForSts2/images/powers/MoonlightGreatswordPower.png", BigIconPath: "res://RasForSts2/images/powers/MoonlightGreatswordPower.png");

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();
}

[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.ModifyDamageMultiplicative))]
public static class MoonlightGreatswordDamagePatch
{
	public static void Postfix(AbstractModel __instance, ref decimal __result, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (__instance is not MoonlightGreatswordPower power)
		{
			return;
		}

		Log.Info($"[MoonlightGreatswordDamage] === Postfix START ===");
		Log.Info($"[MoonlightGreatswordDamage] Power.Owner={power.Owner?.Name ?? "null"}, Dealer={dealer?.Name ?? "null"}, Target={target?.Name ?? "null"}");
		Log.Info($"[MoonlightGreatswordDamage] Original __result={__result}, amount={amount}, props={props}, IsPoweredAttack={props.IsPoweredAttack()}");

		if (!props.IsPoweredAttack())
		{
			Log.Info($"[MoonlightGreatswordDamage] Not a powered attack, skipping");
			Log.Info($"[MoonlightGreatswordDamage] === Postfix END (not powered attack) ===");
			return;
		}

		if (dealer == power.Owner)
		{
			Log.Info($"[MoonlightGreatswordDamage] Branch: dealer == power.Owner (player attacking enemy)");

			// 月光大剑本身：伤害 ×2（+100%）
			decimal beforeMultiply = __result;
			__result *= 2m;
			Log.Info($"[MoonlightGreatswordDamage] Moonlight Greatsword multiply: {beforeMultiply} * 2 = {__result}");

			// 山杖遗物：当女王武具为月光大剑时，打出的攻击牌伤害再额外 +75%（加算）
			// 总倍率：1 * 2 + 0.75 = 2.75（+175%）
			MountainStaff? staff = dealer.Player?.GetRelic<MountainStaff>();
			Log.Info($"[MoonlightGreatswordDamage] MountainStaff relic: {(staff != null ? "FOUND" : "not found")}");
			if (staff != null)
			{
				decimal beforeAdd = __result;
				__result += 0.75m;
				Log.Info($"[MoonlightGreatswordDamage] MountainStaff additive: {beforeAdd} + 0.75 = {__result}");
			}

			Log.Info($"[MoonlightGreatswordDamage] Final __result={__result} (player attack damage)");
			Log.Info($"[MoonlightGreatswordDamage] === Postfix END (player attack) ===");
		}
		else if (target == power.Owner)
		{
			Log.Info($"[MoonlightGreatswordDamage] Branch: target == power.Owner (enemy attacking player)");

			// 月光大剑本身：受到的伤害 ×2（+100%）
			decimal beforeMultiply = __result;
			__result *= 2m;
			Log.Info($"[MoonlightGreatswordDamage] Moonlight Greatsword multiply: {beforeMultiply} * 2 = {__result}");

			// 山杖遗物：敌人攻击对你造成的伤害再额外 +75%（加算）
			// 总倍率：1 * 2 + 0.75 = 2.75（+175%）
			MountainStaff? staff = target.Player?.GetRelic<MountainStaff>();
			Log.Info($"[MoonlightGreatswordDamage] MountainStaff relic: {(staff != null ? "FOUND" : "not found")}");
			if (staff != null)
			{
				decimal beforeAdd = __result;
				__result += 0.75m;
				Log.Info($"[MoonlightGreatswordDamage] MountainStaff additive: {beforeAdd} + 0.75 = {__result}");
			}

			Log.Info($"[MoonlightGreatswordDamage] Final __result={__result} (enemy attack on player)");
			Log.Info($"[MoonlightGreatswordDamage] === Postfix END (enemy attack) ===");
		}
		else
		{
			Log.Info($"[MoonlightGreatswordDamage] Neither dealer nor target is power.Owner, no modification");
			Log.Info($"[MoonlightGreatswordDamage] === Postfix END (no modification) ===");
		}
	}
}