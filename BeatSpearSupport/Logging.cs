using System;

namespace BeatSpearSupport
{
    public static class Logging
    {
        public static void Info(string message) => Log("INFO", message);
        private static void Log(string level, string message) => Console.WriteLine($"[{Plugin.assemblyName} | {level}] {message}");
    }
}
