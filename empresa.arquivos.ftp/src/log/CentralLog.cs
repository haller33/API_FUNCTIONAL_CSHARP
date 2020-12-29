using System;
using NLog;

namespace Empresa.integracao.ftp.log
{
    public static class CentralLog
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        public static void LogInfo(string message)
        {
            Logger.Info(message);
        }

        public static void LogError(Exception exception, string message)
        {
            Logger.Error(exception, message);
        }

    }
}
