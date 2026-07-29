using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using RasForSts2.Scripts.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

[RegisterCard(typeof(XilaCardPool), Inherit = true)]
public abstract class XilaCardModel : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://RasForSts2/images/cards/{GetType().Name}.png"
    );

    protected XilaCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override async Task OnEnqueuePlayVfx(Creature? target)
    {
        if (Type == CardType.Attack)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", Owner.Character.AttackAnimDelay);
        }
        else if (Type == CardType.Power)
        {
            // 能力卡: 仅施法动画
            // VFX 1,2,3 (NPowerAppliedVfx/NPowerAppliedBuffVfx/sfx) 由引擎在 Power 层数变化时自动触发
            // VFX 5 (NCardFlyPowerVfx) 由引擎对 CardType.Power 自动触发
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        }
        else
        {
            // 技能卡: 仅施法动画 (VFX 1,2,3 由引擎在 Power 层数变化时自动触发)
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        }
    }
}
