using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;
using RasForSts2.Scripts.Cards;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Potions;

[RegisterPotion(typeof(XilaPotionPool))]
public sealed class JadeRabbitMochiPotion : ModPotionTemplate
{
    public override PotionRarity Rarity => PotionRarity.Rare;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    public override PotionAssetProfile AssetProfile => new(
        ImagePath: "res://RasForSts2/images/potions/JadeRabbitMochiPotion.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<JadeRabbitMochiPower>(),
    ];

    // spec: 在本场战斗中，女王武具power可以叠加，卡组中所有女王武具卡牌获得保留。
    // 1. 施加 JadeRabbitMochiPower —— QueenWeaponCmd 检测到此 power 后跳过移除旧武具，允许多武具共存
    // 2. 遍历本场战斗所有卡牌，给女王武具卡牌添加 Retain 关键词
    // 幂等性：重复使用时，Power 已存在则跳过施加；卡牌已有 Retain 则跳过添加
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        // 施加多武具共存标记（已存在则跳过，避免 Single 类型 power 的 amount 被错误叠加）
        if (!Owner.Creature.HasPower<JadeRabbitMochiPower>())
        {
            await PowerCmd.Apply<JadeRabbitMochiPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, null);
            Log.Info($"[JadeRabbitMochi] 施加 JadeRabbitMochiPower");
        }
        else
        {
            Log.Info($"[JadeRabbitMochi] JadeRabbitMochiPower 已存在，跳过施加");
        }

        // 给本场战斗中所有女王武具卡牌添加保留关键词（已有 Retain 的卡牌会被跳过）
        int retainCount = 0;
        int skipCount = 0;
        foreach (var card in Owner.PlayerCombatState.AllCards)
        {
            if (!IsQueenWeaponCard(card)) continue;

            if (card.Keywords.Contains(CardKeyword.Retain))
            {
                skipCount++;
                continue;
            }

            card.AddKeyword(CardKeyword.Retain);
            retainCount++;
            Log.Info($"[JadeRabbitMochi] 给卡牌 {card.Id.Entry} 添加保留关键词");
        }
        Log.Info($"[JadeRabbitMochi] 添加保留: {retainCount} 张, 已有保留跳过: {skipCount} 张");
    }

    private static bool IsQueenWeaponCard(MegaCrit.Sts2.Core.Models.CardModel card)
    {
        return card is MoonlightGreatsword
            || card is MoonlightShield
            || card is MoonlightStaff
            || card is MoonlightBlades;
    }
}
