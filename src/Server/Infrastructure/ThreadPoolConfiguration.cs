using ILogger = Serilog.ILogger;

namespace BcGov.Jag.AccountManagement.Server.Infrastructure;

public static class ThreadPoolConfiguration
{
    /// <summary>
    /// Sets the minimum number of threads the thread pool creates on demand.
    /// </summary>
    public static void Configure(ILogger logger)
    {
        int minWorkerThreads; // default: 8
        int minCompletionPortThreads;   // default: 8
        ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);

        logger.Debug("Default ThreadPool settings {MinWorkerThreads} / {MinCompletionPortThreads}", minWorkerThreads, minCompletionPortThreads);

        minWorkerThreads = GetSetting("THREADPOOL__MINWORKERTHREADS", minWorkerThreads, logger);
        minCompletionPortThreads = GetSetting("THREADPOOL__MINIOTHREADS", minCompletionPortThreads, logger);

        ThreadPool.SetMinThreads(minWorkerThreads, minCompletionPortThreads);
        logger.Information("ThreadPool settings {MinWorkerThreads} / {MinCompletionPortThreads}", minWorkerThreads, minCompletionPortThreads);
    }

    private static int GetSetting(string name, int defaultValue, ILogger logger)
    {
        var stringValue = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrEmpty(stringValue) || !int.TryParse(stringValue, out var value))
        {
            logger.Information(
                "Environment variable {EnvironmentVariable} is not set or is not an integer, defaulting to {Value}",
                name, defaultValue);
            return defaultValue;
        }
        else
        {
            logger.Information("Environment variable {EnvironmentVariable} is set to {Value}", name, value);
        }

        return value;
    }
}
