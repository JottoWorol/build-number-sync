using System;
using UnityEditor;
using UnityEngine;

namespace JottoWorol.BuildNumberSync.Editor
{
    /// <summary>
    /// Build number provider implementation for remote storage mode.
    /// Syncs build numbers with a remote API for team collaboration.
    /// </summary>
    internal class RemoteBuildNumberProvider : IBuildNumberProvider
    {
        private readonly NetworkRequests networkRequests;

        public RemoteBuildNumberProvider()
        {
            networkRequests = new NetworkRequests();
        }

        public bool TryGetNext(string bundleId, BuildTarget buildTarget, out int buildNumber)
        {
            buildNumber = -1;
            if (string.IsNullOrWhiteSpace(bundleId)) return false;

            var platform = buildTarget.ToString();

            try
            {
                if (!networkRequests.TryGetNextBuildNumber(bundleId, platform, out buildNumber))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{Logging.TAG} RemoteBuildNumberProvider.TryGetNext error: {ex}");
                return false;
            }
        }

        public bool SetRemoteBuildNumber(string bundleId, BuildTarget buildTarget, int buildNumber)
        {
            if (string.IsNullOrWhiteSpace(bundleId)) return false;
            if (buildNumber < 0) return false;

            var platform = buildTarget.ToString();

            try
            {
                return networkRequests.TrySetBuildNumber(bundleId, platform, buildNumber);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{Logging.TAG} RemoteBuildNumberProvider.SetRemoteBuildNumber error: {ex}");
                return false;
            }
        }

        public bool DeleteRemoteBuildNumber(string bundleId, BuildTarget buildTarget)
        {
            if (string.IsNullOrWhiteSpace(bundleId)) return false;

            var platform = buildTarget.ToString();

            try
            {
                return networkRequests.TryDeleteBundleId(bundleId, platform);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{Logging.TAG} RemoteBuildNumberProvider.DeleteRemoteBuildNumber error: {ex}");
                return false;
            }
        }
    }
}

