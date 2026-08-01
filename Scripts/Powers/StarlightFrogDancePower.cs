using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

/// <summary>
/// 星光蛙的舞乐
/// 当你给予未曾给予过的负面效果时，获得1点力量和1点敏捷。
/// </summary>
[RegisterPower]
public sealed class StarlightFrogDancePower : ModPowerTemplate
{
    private class Data
    {
        public HashSet<string> appliedDebuffIds = new();
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://RasForSts2/images/powers/StarlightFrogDancePower.png",
        BigIconPath: "res://RasForSts2/images/powers/StarlightFrogDancePower.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override object InitInternalData()
    {
        return new Data();
    }

    /// <summary>
    /// 当任何 Power 的 amount 变化时触发。
    /// 检查是否为玩家施加的、未曾给予过的负面效果，如果是则获得1点力量和1点敏捷。
    /// </summary>
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        // 跳过 amount <= 0 的情况
        if (amount <= 0m)
        {
            return;
        }

        // 检查是否为 Debuff 类型
        if (power.GetTypeForAmount(amount) != PowerType.Debuff)
        {
            return;
        }

        // 跳过临时 Power
        if (power is ITemporaryPower)
        {
            return;
        }

        // 检查施加者是否为本 Power 的 Owner
        if (applier != Owner)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        string debuffId = power.Id.Entry;

        // 检查是否为未曾给予过的负面效果
        if (data.appliedDebuffIds.Contains(debuffId))
        {
            return;
        }

        // 记录已给予的负面效果
        data.appliedDebuffIds.Add(debuffId);

        Log.Info($"[StarlightFrogDance] 首次施加负面效果: {debuffId}, 获得1点力量和1点敏捷。");

        // 获得1点力量和1点敏捷
        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, 1m, Owner, null);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, 1m, Owner, null);
    }
}
