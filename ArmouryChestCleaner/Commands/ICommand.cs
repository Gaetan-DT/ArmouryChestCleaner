using Dalamud.Game.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmouryChestCleaner.Commands
{
    public interface ICommand
    {
        public string GetCommandName();
        public CommandInfo CreateCommandInfo();
        public void OnCommand(string command, string args);
    }
}
