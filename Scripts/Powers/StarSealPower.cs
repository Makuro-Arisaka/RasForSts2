using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

/// <summary>
/// 星辰封印
/// 每回合最大能量 +1。
/// 回合开始时，获得Amount点活力（Amount=1未升级/2升级）。清空你的护卫层数。
/// </summary>
[RegisterPower]
public sealed class StarSealPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    // Counter: Amount 存储活力层数（卡牌施加时：未升级1，升级2）
    // 注意：最大能量+1固定生效，不依赖 Amount（类似 PyrePower 用 ModifyMaxEnergy 钩子）
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://RasForSts2/images/powers/StarSealPower.png",
        BigIconPath: "res://RasForSts2/images/powers/StarSealPower.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    // 基类 ModPowerTemplate.ExtraHoverTips 是 sealed，不能 override，用 new 隐藏（同 PyrePower）
    public new IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.ForEnergy(this),
    ];

    // ========== PYRE 模式：修改最大能量 ==========
    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (player != Owner.Player)
        {
            return amount;
        }
        return amount + 1m;
    }

    // ========== 回合开始：获得活力 ==========
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await base.AfterPlayerTurnStart(choiceContext, player);

        if (player != Owner.Player)
        {
            return;
        }

        Flash();

        // 获得 Amount 点活力（未升级1 / 升级2）
        decimal vigorAmount = Amount;
        if (vigorAmount > 0m)
        {
            Log.Info($"[StarSeal] Applying VigorPower: amount={vigorAmount}");
            await PowerCmd.Apply<VigorPower>(choiceContext, Owner, vigorAmount, Owner, null);
        }
    }
}
