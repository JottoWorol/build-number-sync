using UnityEditor;
using UnityEngine;

namespace JottoWorol.BuildNumberSync.Editor
{
    public static class EditorCommands
    {
        /// <summary>
        /// Pulls the next build number from the server and assigns it to PlayerSettings.
        /// </summary>
        [MenuItem("Tools/Build Number Sync/Pull & Assign Build Number")]
        public static void AssignBuildNumber()
        {
            var networkRequests = new NetworkRequests();
            var platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            
            if (networkRequests.TryGetNextBuildNumber(PlayerSettings.applicationIdentifier, platform, out var buildNumber))
            {
                var currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;
                if (!BuildNumberHelper.TryAssignBuildNumber(currentBuildTarget, buildNumber, out var errorMessage))
                {
                    Debug.LogError($"{Logging.TAG} Failed to assign build number: {errorMessage}");
                }
                else
                {
                    Debug.Log($"{Logging.TAG} Assigned build number {buildNumber} to PlayerSettings for {platform}.");
                }
            }
        }

        /// <summary>
        /// Pushes the current build number from PlayerSettings to the server, overwriting the server value.
        /// </summary>
        [MenuItem("Tools/Build Number Sync/Push Build Number to Server")]
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
                Debug.Log($"{Logging.TAG} Aborted pushing build number to server.");
                return;
            }
            
            Debug.Log($"{Logging.TAG} Pushing current build number {buildNumber} for platform '{platform}' to server...");
            
            if (networkRequests.TrySetBuildNumber(bundleId, platform, buildNumber))
            {
                Debug.Log($"{Logging.TAG} Successfully set build number {buildNumber} for '{platform}'.");
            }
            else
            {
                Debug.LogError($"{Logging.TAG} Failed to set build number {buildNumber} for '{platform}'.");
            }
        }

        /// <summary>
        /// Deletes the build number data for the current bundle ID and platform from the server.
        /// WARNING: This action cannot be undone. The next build will start from build number 1.
        /// </summary>
        [MenuItem("Tools/Build Number Sync/Delete data from Server")]
        public static void DeleteBundleIdFromServer()
        {
            var networkRequests = new NetworkRequests();
            var bundleId = PlayerSettings.applicationIdentifier;
            var platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            
            var result = EditorUtility.DisplayDialog(
                "Delete Build Number Data",
                $"WARNING: This will permanently delete the build number data from the server for:\n\n" +
                $"Bundle ID: {bundleId}\n" +
                $"Platform: {platform}\n\n" +
                $"This action cannot be undone.\n" +
                $"The next build will start from build number 1.\n\n" +
                $"Are you sure you want to continue?",
                "Delete", 
                "Cancel");
            
            if (!result)
            {
                Debug.Log($"{Logging.TAG} Aborted deleting bundle data from server.");
                return;
            }
            
            Debug.Log($"{Logging.TAG} Deleting build number data for '{bundleId}' platform '{platform}' from server...");
            
            if (networkRequests.TryDeleteBundleId(bundleId, platform))
            {
                Debug.Log($"{Logging.TAG} Successfully deleted build number data for '{bundleId}' platform '{platform}'.");
            }
            else
            {
                Debug.LogError($"{Logging.TAG} Failed to delete build number data. Check console for details.");
            }
        }
    }
}