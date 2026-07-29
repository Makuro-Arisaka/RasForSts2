using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Saves;
using RasForSts2.Scripts.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Relics;

[RegisterRelic(typeof(GenericRelicPool))]
public class RainbowCloak : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://RasForSts2/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"res://RasForSts2/images/relics/{GetType().Name}.png",
        BigIconPath: $"res://RasForSts2/images/relics/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

    // spec: 选择已有的遗物，再次获得一个。
    // 获得时立即触发：弹出选择界面，从已有遗物中选一个，获得其克隆实例。
    // 排除项：
    //   - RainbowCloak 自身（避免无限自我复制）
    //   - Girya（复制后休息点会显示两个"举重"选项，UI 重复）
    public override async Task AfterObtained()
    {
        var player = base.Owner;

        // 构建候选列表：排除自身和 Girya
        var candidates = player.Relics
            .Where(r => r is not RainbowCloak && r is not Girya)
            .ToList();

        Log.Info($"[RainbowCloak] 候选遗物数: {candidates.Count}（已排除 RainbowCloak 自身和 Girya）");

        if (candidates.Count == 0)
        {
            Log.Info($"[RainbowCloak] 没有可复制的遗物，跳过");
            return;
        }

        // 弹出遗物选择界面
        RelicModel? selected = await RelicSelectCmd.FromChooseARelicScreen(player, candidates);

        if (selected == null)
        {
            Log.Info($"[RainbowCloak] 玩家跳过了选择");
            return;
        }

        Log.Info($"[RainbowCloak] 玩家选择了: {selected.Id.Entry}，开始克隆并添加");

        // 克隆选中遗物（通过 ID 获取规范实例的可变副本）
        RelicModel clone = SaveUtil.RelicOrDeprecated(selected.Id).ToMutable();
        await RelicCmd.Obtain(clone, player);

        Log.Info($"[RainbowCloak] 成功添加 {selected.Id.Entry} 的克隆实例");
    }
}
