using JottoWorol.BuildNumberSync.Runtime;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace JottoWorol.BuildNumberSync.Editor
{
    /// <summary>
    /// Preprocess build step that fetches the next build number from the network and assigns
    /// it to the appropriate PlayerSettings for the current build target.
    /// </summary>
    internal class PreprocessAssignBuildNumber : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var isCi = BuildEnvironment.IsCi();
            var bundleId = PlayerSettings.applicationIdentifier;
            var platform = report.summary.platform.ToString();

            if (string.IsNullOrWhiteSpace(bundleId))
            {
                HandleFailure(isCi,
                    $"PlayerSettings.applicationIdentifier is not set for target {report.summary.platform}. Cannot fetch build number without a valid bundle id.");
            }

            var network = new NetworkRequests();

            // fetch next build number from server
            if (!network.TryGetNextBuildNumber(bundleId, platform, out var buildNumber))
            {
                HandleFailure(isCi, $"Failed to obtain next build number for bundle id '{bundleId}'");
            }

            // assign build number to PlayerSettings according to target platform
            if (!BuildNumberHelper.TryAssignBuildNumber(report.summary.platform, buildNumber, out var errorMessage))
            {
                HandleFailure(isCi,
                    $"Failed to assign build number {buildNumber} for bundle id '{bundleId}': {errorMessage}");
            }
            
            // store build number in asset for runtime access
            BuildNumberAssetIO.Write(buildNumber);

            Debug.Log($"{Logging.TAG} Assigned build number {buildNumber} for bundle id {bundleId} (target: {report.summary.platform}).");
        }

        /// <summary>
        /// Unified failure handling
        /// </summary>
        /// <param name="isCi">If true, avoid any user interaction and just throw exceptions on failure.</param>
        /// <param name="errorMessage"></param>
        /// <exception cref="BuildFailedException"></exception>
        private static void HandleFailure(bool isCi, string errorMessage)
        {
            var useCurrent = false;

            if (isCi)
            {
                throw new BuildFailedException(errorMessage);
            }

            useCurrent = EditorUtility.DisplayDialog(
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