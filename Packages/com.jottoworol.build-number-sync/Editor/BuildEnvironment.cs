using System;
using UnityEngine;

namespace JottoWorol.BuildNumberSync.Editor
{
    internal static class BuildEnvironment
    {
        /// <summary>
        /// Determines if the current environment is a Continuous Integration (CI) environment.
        /// </summary>
        public static bool IsCi()
        {
            if (Application.isBatchMode) return true;

            var ci = Environment.GetEnvironmentVariable("CI");
            if (!string.IsNullOrEmpty(ci) && (ci.Equals("true", StringComparison.OrdinalIgnoreCase) || ci == "1"))
                return true;

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")))
                return true;

            return false;
        }
    }
}