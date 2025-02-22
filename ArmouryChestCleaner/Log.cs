using ECommons.DalamudServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmouryChestCleaner
{
    public static class Log
    {

        public static void LogDebug(string message, bool printChat = false)
        {
            Svc.Log.Debug(message);
            if (printChat)
                LogChat(message);
        }

        public static void LogInfo(string message, bool printChat = false)
        {
            Svc.Log.Info(message);
            if (printChat)
                LogChat(message);
        }

        public static void LogChat(string message)
        {
            Svc.Chat.Print(message);
        }
    }
}
