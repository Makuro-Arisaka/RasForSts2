using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using RasForSts2.Scripts.Characters;
using RasForSts2.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RasForSts2.Scripts.Relics;

[RegisterRelic(typeof(XilaRelicPool))]
public class StoneDragonArmor : ModRelicTemplate
{
    private bool _guardDisappeared;
    private decimal _lastGuardAmount;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://RasForSts2/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"res://RasForSts2/images/relics/{GetType().Name}.png",
        BigIconPath: $"res://RasForSts2/images/relics/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar("Guard", 3m) };

    public override Task BeforeCombatStart()
    {
        _guardDisappeared = false;
        _lastGuardAmount = 0;
        Log.Info("[StoneDragonArmor] BeforeCombatStart: initialized state");
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        Log.Info($"[StoneDragonArmor] AfterPlayerTurnStartEarly: _guardDisappeared={_guardDisappeared}, _lastGuardAmount={_lastGuardAmount}");

        CheckGuardDisappeared();

        if (!_guardDisappeared)
        {
            Log.Info("[StoneDragonArmor] No guard disappeared, skip granting");
            return;
        }

        _guardDisappeared = false;

        Flash();
        decimal guardAmount = base.DynamicVars["Guard"].BaseValue;
        Log.Info($"[StoneDragonArmor] Guard disappeared! Granting {guardAmount} guard. Current guard state: _lastGuardAmount={_lastGuardAmount}");
        await PowerCmd.Apply<GuardPower>(choiceContext, player.Creature, guardAmount, player.Creature, null);
        _lastGuardAmount = guardAmount;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CheckGuardDisappeared();
        await Task.CompletedTask;
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        _guardDisappeared = false;
        _lastGuardAmount = 0;
        Log.Info("[StoneDragonArmor] AfterCombatEnd: reset state");
        await Task.CompletedTask;
    }

    private void CheckGuardDisappeared()
    {
        Creature? creature = base.Owner?.Creature;
        if (creature == null)
        {
            return;
        }

        GuardPower? guardPower = creature.GetPower<GuardPower>();
        decimal currentGuard = guardPower?.Amount ?? 0;

        Log.Info($"[StoneDragonArmor] CheckGuardDisappeared: _lastGuardAmount={_lastGuardAmount}, currentGuard={currentGuard}");

        if (_lastGuardAmount > 0 && currentGuard <= 0)
        {
            _guardDisappeared = true;
            Log.Info("[StoneDragonArmor] Guard disappeared: transitioned from {_lastGuardAmount} to {currentGuard}");
        }

        _lastGuardAmount = currentGuard;
    }
}
