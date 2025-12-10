using UnityEditor;
using UnityEngine;

namespace JottoWorol.BuildNumberSync.Editor
{
    public static class EditorTool
    {
        /// <summary>
        /// Pulls the next build number from the server and assigns it to PlayerSettings.
        /// </summary>
        [MenuItem("Tools/Build Number Sync/Pull & Assign New Build Number (Manual)")]
        public static void AssignBuildNumber()
        {
            var networkRequests = new NetworkRequests();
            var platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            
            if (networkRequests.TryGetNextBuildNumber(PlayerSettings.applicationIdentifier, platform, out var buildNumber))
            {
                var currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;
                if (!BuildNumberHelper.TryAssignBuildNumber(currentBuildTarget, buildNumber, out var errorMessage))
                {
                    Debug.LogError($"Failed to assign build number: {errorMessage}");
                }
                else
                {
                    Debug.Log($"Assigned build number {buildNumber} to PlayerSettings for {platform}.");
                }
            }
        }

        /// <summary>
        /// Pushes the current build number from PlayerSettings to the server, overwriting the server value.
        /// </summary>
        [MenuItem("Tools/Build Number Sync/Push Current Build Number to Server")]
        public static void SetCurrentVersionOnServer()
        {
            var networkRequests = new NetworkRequests();
            var bundleId = PlayerSettings.applicationIdentifier;
            var platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            var buildNumber = BuildNumberHelper.GetCurrentBuildNumber(EditorUserBuildSettings.activeBuildTarget);
            
            var result = EditorUtility.DisplayDialog("Push Build Number",
                $"Are you sure you want to push the current build number {buildNumber} for platform '{platform}' to the server?",
                "Yes", "No");
            
            if (!result)
            {
                Debug.Log("Aborted pushing build number to server.");
                return;
            }
            
            Debug.Log($"Pushing current build number {buildNumber} for platform '{platform}' to server...");
            
            if (networkRequests.TrySetBuildNumber(bundleId, platform, buildNumber))
            {
                Debug.Log($"Successfully set build number {buildNumber} for '{platform}'.");
            }
            else
            {
                Debug.LogError($"Failed to set build number {buildNumber} for '{platform}'.");
            }
        }
    }
}