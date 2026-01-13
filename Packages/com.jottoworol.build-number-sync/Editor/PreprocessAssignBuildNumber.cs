using JottoWorol.BuildNumberSync.Runtime;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace JottoWorol.BuildNumberSync.Editor
{
    /// <summary>
    /// Preprocess build step that fetches the next build number based on configured storage mode
    /// and assigns it to the appropriate PlayerSettings for the current build target.
    /// </summary>
    internal class PreprocessAssignBuildNumber : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var bundleId = PlayerSettings.applicationIdentifier;
            var buildTarget = report.summary.platform;

            if (string.IsNullOrWhiteSpace(bundleId))
            {
                HandleFailure($"PlayerSettings.applicationIdentifier is not set for target {buildTarget}. Cannot fetch build number without a valid bundle id.");
            }

            // Determine which provider to use based on storage mode configuration
            var useLocalProvider = BuildNumberSyncSettingsProvider.GetUseLocalProvider();
            var useLocalAsFallback = BuildNumberSyncSettingsProvider.GetUseLocalAsFallback();
            IBuildNumberProvider provider;
            string providerName;

            if (useLocalProvider)
            {
                provider = new LocalBuildNumberProvider();
                providerName = "local";
            }
            else
            {
                provider = new RemoteBuildNumberProvider();
                providerName = "remote";
            }

            // Try to get next build number from the primary provider
            if (!provider.TryGetNext(bundleId, buildTarget, out var buildNumber))
            {
                // If remote storage mode failed and fallback is enabled, use local storage
                if (!useLocalProvider && useLocalAsFallback)
                {
                    Debug.LogWarning($"{Logging.TAG} Failed to obtain build number from remote storage. Falling back to local storage.");
                    provider = new LocalBuildNumberProvider();
                    providerName = "local (fallback)";
                    
                    if (!provider.TryGetNext(bundleId, buildTarget, out buildNumber))
                    {
                        HandleFailure($"Failed to obtain next build number for bundle id '{bundleId}' from both remote and local storage.");
                    }
                }
                else
                {
                    HandleFailure($"Failed to obtain next build number for bundle id '{bundleId}' from {providerName} storage.");
                }
            }

            // assign build number to PlayerSettings according to target platform
            if (!BuildNumberHelper.TryAssignBuildNumber(buildTarget, buildNumber, out var errorMessage))
            {
                HandleFailure($"Failed to assign build number {buildNumber} for bundle id '{bundleId}': {errorMessage}");
            }
            
            // store build number in asset for runtime access
            BuildNumberAssetIO.Write(buildNumber);

            Debug.Log($"{Logging.TAG} Assigned build number {buildNumber} for bundle id {bundleId} (target: {buildTarget}, provider: {providerName}).");
        }

        /// <summary>
        /// Unified failure handling
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <exception cref="BuildFailedException"></exception>
        private static void HandleFailure(string errorMessage)
        {
            if (BuildEnvironment.IsCi())
            {
                throw new BuildFailedException(errorMessage);
            }

            var useCurrent = EditorUtility.DisplayDialog(
                "Build number sync error",
                $"{errorMessage}\nYou can keep the current PlayerSettings and continue the build, or abort the build.",
                "Use current settings and continue",
                "Abort Build");

            if (!useCurrent)
            {
                throw new BuildFailedException($"Build aborted by user: {errorMessage}");
            }
        }
    }
}