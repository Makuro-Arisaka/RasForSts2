using System.Threading.Tasks;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using RasForSts2.Scripts.Resources;

namespace RasForSts2.Scripts.Commands;

/// <summary>
/// 开发控制台命令：darkcurse &lt;amount:int&gt;
/// 获得指定数量的黑暗法咒。例如 "darkcurse 9" 获得 9 点黑暗法咒。
/// 命令通过反射被 DevConsole 自动发现并注册（继承 AbstractConsoleCmd 即可）。
/// </summary>
public sealed class DarkCurseConsoleCmd : AbstractConsoleCmd
{
    public override string CmdName => "darkcurse";

    public override string Args => "<amount:int>";

    public override string Description => "Gain the given amount of Dark Curse.";

    public override bool IsNetworked => true;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (issuingPlayer?.PlayerCombatState == null)
        {
            return new CmdResult(false, "This command only works in combat.");
        }

        if (args.Length < 1)
        {
            return new CmdResult(false, "Usage: darkcurse <amount:int>");
        }

        if (!int.TryParse(args[0], out var amount))
        {
            return new CmdResult(false, $"Amount must be an int, got '{args[0]}'.");
        }

        if (amount <= 0)
        {
            return new CmdResult(false, "Amount must be positive.");
        }

        Task<int> task = DarkCurseResource.Gain(issuingPlayer, amount);
        return new CmdResult(task, true, $"Gaining {amount} Dark Curse.");
    }
}
