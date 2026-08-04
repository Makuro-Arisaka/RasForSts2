using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Powers;

/// <summary>
/// 坚守阵地
/// 回合开始时，将弃牌堆的一张牌放入手牌并给予保留。
/// </summary>
[RegisterPower]
public sealed class HoldThePositionPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://RasForSts2/images/powers/HoldThePositionPower.png",
        BigIconPath: "res://RasForSts2/images/powers/HoldThePositionPower.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await base.AfterPlayerTurnStart(choiceContext, player);

        if (player != Owner.Player)
        {
            return;
        }

        // 手牌已满则跳过（手牌上限10张）
        int handCount = PileType.Hand.GetPile(Owner.Player).Cards.Count;
        if (handCount >= 10)
        {
            Log.Debug("[HoldThePosition] Hand is full, skipping.");
            return;
        }

        // 获取弃牌堆卡牌 (CardPile.Cards = IReadOnlyList<CardModel (Models.CardModel)>)
        IReadOnlyList<CardModel> discardCards = PileType.Discard.GetPile(Owner.Player).Cards;
        if (discardCards.Count == 0)
        {
            Log.Debug("[HoldThePosition] Discard pile is empty, skipping.");
            return;
        }

        Flash();

        // 让玩家从弃牌堆选择1张牌送入手牌
        CardSelectorPrefs prefs = new CardSelectorPrefs(
            new LocString("card_selection", "TO_SEND_TO_HAND"), 1);

        IEnumerable<CardModel> selected = await CardSelectCmd.FromSimpleGrid(
            choiceContext, discardCards, Owner.Player, prefs);

        CardModel? chosen = selected.FirstOrDefault();

        if (chosen == null)
        {
            Log.Debug("[HoldThePosition] No card selected.");
            return;
        }

        // 将选中的卡牌从弃牌堆移动到手牌
        await CardPileCmd.Add(chosen, PileType.Hand);

        // 给予保留关键字（检查避免重复添加）
        if (!chosen.Keywords.Contains(CardKeyword.Retain))
        {
            CardCmd.ApplyKeyword(chosen, [CardKeyword.Retain]);
            Log.Info($"[HoldThePosition] Moved '{chosen.Id.Entry}' from discard to hand with Retain.");
        }
        else
        {
            Log.Info($"[HoldThePosition] Moved '{chosen.Id.Entry}' from discard to hand (already had Retain).");
        }
    }
}
