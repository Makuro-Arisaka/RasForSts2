using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Cards;

// 注册到 TokenCardPool（与原版派生卡 Shiv 一致），防止被发现/药水/熵等局内生成机制选中
[RegisterCard(typeof(TokenCardPool))]
public class Pellet : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3, ValueProp.Move)
    ];

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Shiv];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://RasForSts2/images/cards/Pellet.png"
    );

    public Pellet() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);

        if (IsUpgraded)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, CombatState.HittableEnemies, 1m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
    ];

    public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count, CombatState combatState)
    {
        if (count == 0)
            return [];
        if (CombatManager.Instance.IsOverOrEnding)
            return [];

        List<CardModel> pellets = [];
        for (int i = 0; i < count; i++)
        {
            pellets.Add(combatState.CreateCard<Pellet>(owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(pellets, PileType.Hand, owner);
        return pellets;
    }
}
