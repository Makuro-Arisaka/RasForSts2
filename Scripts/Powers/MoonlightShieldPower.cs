using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

[RegisterPower]
public sealed class MoonlightShieldPower : ModPowerTemplate
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override PowerAssetProfile AssetProfile => new(IconPath: "res://RasForSts2/images/powers/MoonlightShieldPower.png");

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();
}

[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.ModifyBlockMultiplicative))]
public static class MoonlightShieldBlockPatch
{
	public static void Postfix(AbstractModel __instance, ref decimal __result, Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (__instance is not MoonlightShieldPower power)
		{
			return;
		}
		
		if (power.Owner != target)
		{
			return;
		}
		
		if (!props.IsPoweredCardOrMonsterMoveBlock())
		{
			return;
		}

		// 限定: 只对玩家打出的技能牌生效 (spec: "你打出的技能牌获得的格挡翻倍")
		// monster move 没有 cardSource, 自动被过滤
		if (cardSource == null || cardSource.Type != CardType.Skill)
		{
			return;
		}

		__result *= 2m;
	}
}

[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.ModifyDamageMultiplicative))]
public static class MoonlightShieldDamagePatch
{
	public static void Postfix(AbstractModel __instance, ref decimal __result, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (__instance is not MoonlightShieldPower power)
		{
			return;
		}
		
		if (dealer != power.Owner)
		{
			return;
		}
		
		if (!props.IsPoweredAttack())
		{
			return;
		}
		
		__result *= 0.5m;
	}
}