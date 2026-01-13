using UnityEditor;
using UnityEngine;

namespace JottoWorol.BuildNumberSync.Editor
{
    /// <summary>
    /// Build number provider implementation for local storage mode.
    /// Increments the current build number from PlayerSettings locally.
    /// Does not use any external storage - reads from and writes to PlayerSettings directly.
    /// </summary>
    internal class LocalBuildNumberProvider : IBuildNumberProvider
    {
        public bool TryGetNext(string bundleId, BuildTarget buildTarget, out int buildNumber)
        {
            buildNumber = -1;
            if (string.IsNullOrWhiteSpace(bundleId)) return false;

            try
            {
                var current = BuildNumberHelper.GetCurrentBuildNumber(buildTarget);
                buildNumber = current + 1;
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"{Logging.TAG} LocalBuildNumberProvider.TryGetNext error: {ex}");
                return false;
            }
        }
    }
}

