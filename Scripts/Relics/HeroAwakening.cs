using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using RasForSts2.Scripts.Cards;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Helpers;
using RasForSts2.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Relics;

/// <summary>
/// 英雄的觉悟（英雄的证明升级版，先古遗物）
/// spec:
/// - 战斗开始时，从3张随机女王武具中选择1张加入手牌
/// - 第一次切换女王武具时获得1点能量
/// - 获得2点护卫
/// </summary>
[RegisterRelic(typeof(XilaRelicPool))]
public class HeroAwakening : ModRelicTemplate
{
    private bool _hasSwitchedWeaponThisCombat;
    private bool _isSubscribed;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        QueenWeaponHoverTip.Create(),
        HoverTipFactory.ForEnergy(this),
        HoverTipFactory.FromPower<GuardPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
    ];

    private static readonly Type[] QueenWeaponCardTypes = new[]
    {
        typeof(MoonlightGreatsword),
        typeof(MoonlightShield),
        typeof(MoonlightStaff),
        typeof(MoonlightBlades),
    };

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://RasForSts2/images/relics/HeroAwakening.png",
        IconOutlinePath: $"res://RasForSts2/images/relics/HeroAwakening.png",
        BigIconPath: $"res://RasForSts2/images/relics/HeroAwakening.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new EnergyVar(1),
        new DynamicVar("Guard", 2m)
    };

    public override Task BeforeCombatStart()
    {
        _hasSwitchedWeaponThisCombat = false;

        if (_isSubscribed)
        {
            QueenWeaponHelper.OnQueenWeaponChanged -= OnQueenWeaponChanged;
            _isSubscribed = false;
        }

        QueenWeaponHelper.OnQueenWeaponChanged += OnQueenWeaponChanged;
        _isSubscribed = true;
        Log.Info($"[HeroAwakening] BeforeCombatStart: subscribed, reset flags");
        return Task.CompletedTask;
    }

    /// <summary>
    /// 获得2点护卫 - BeforeCombatStartLate（无context也能Apply Power）
    /// </summary>
    public override async Task BeforeCombatStartLate()
    {
        Player? player = base.Owner;
        if (player == null)
        {
            Log.Info($"[HeroAwakening] BeforeCombatStartLate: player is null, skipping");
            return;
        }

        Log.Info($"[HeroAwakening] BeforeCombatStartLate: applying 2 Guard");
        await PowerCmd.Apply<GuardPower>(null, player.Creature, DynamicVars["Guard"].BaseValue, player.Creature, null);
    }

    /// <summary>
    /// 女王武具3选1 - 参考 Toolbox/JeweledMask，使用 BeforeHandDraw（带有效context）+ FromChooseACardScreen
    /// 注意：项目引用的 sts2.dll 中 BeforeHandDraw 第三参数为 ICombatState（非 CombatState）
    /// </summary>
    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != base.Owner || combatState.RoundNumber > 1)
        {
            return;
        }

        // 从4张女王武具中随机洗牌，取3张让玩家选择1张加入手牌（均为升级版）
        List<Type> shuffled = QueenWeaponCardTypes.ToList();
        player.RunState.Rng.CombatCardSelection.Shuffle(shuffled);
        List<Type> candidates = shuffled.Take(3).ToList();

        Log.Info($"[HeroAwakening] Queen weapon candidates: {string.Join(", ", candidates.Select(t => t.Name))}");

        List<CardModel> cards = new();
        foreach (Type cardType in candidates)
        {
            CardModel canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(cardType));
            CardModel mutable = combatState.CreateCard(canonical, player);
            CardCmd.Upgrade(mutable); // 升级的女王武具
            cards.Add(mutable);
        }

        Flash();
        CardModel? selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards, base.Owner);

        if (selected != null)
        {
            // 加入手牌并给予保留（检查避免重复添加）
            if (!selected.Keywords.Contains(CardKeyword.Retain))
            {
                CardCmd.ApplyKeyword(selected, [CardKeyword.Retain]);
            }

            Log.Info($"[HeroAwakening] Selected queen weapon: {selected.Id.Entry}");
            await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Hand, player);
        }
        else
        {
            Log.Warn($"[HeroAwakening] No card selected from queen weapon choice");
        }
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        Unsubscribe();
        return Task.CompletedTask;
    }

    public override Task AfterRemoved()
    {
        Unsubscribe();
        return base.AfterRemoved();
    }

    private void Unsubscribe()
    {
        if (_isSubscribed)
        {
            QueenWeaponHelper.OnQueenWeaponChanged -= OnQueenWeaponChanged;
            _isSubscribed = false;
            Log.Info($"[HeroAwakening] Unsubscribed from OnQueenWeaponChanged");
        }
    }

    private async void OnQueenWeaponChanged(Player player, Type? oldWeaponType, Type? newWeaponType)
    {
        try
        {
            if (player == null)
            {
                return;
            }

            Player? owner = base.Owner;
            if (owner == null)
            {
                Unsubscribe();
                return;
            }

            if (player != owner)
            {
                return;
            }

            if (owner.GetRelic<HeroAwakening>() != this)
            {
                Unsubscribe();
                return;
            }

            if (owner.Creature?.CombatState == null)
            {
                return;
            }

            if (_hasSwitchedWeaponThisCombat)
            {
                return;
            }

            _hasSwitchedWeaponThisCombat = true;
            Log.Info($"[HeroAwakening] First weapon switch detected: {oldWeaponType?.Name ?? "null"} -> {newWeaponType?.Name ?? "null"}");

            Flash();
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, player);
            Log.Info($"[HeroAwakening] Gained {DynamicVars.Energy.BaseValue} energy");
        }
        catch (Exception ex)
        {
            Log.Error($"[HeroAwakening] ERROR in OnQueenWeaponChanged: {ex}");
        }
    }
}
