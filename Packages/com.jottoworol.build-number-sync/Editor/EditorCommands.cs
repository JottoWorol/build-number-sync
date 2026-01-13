using UnityEditor;
using UnityEngine;

namespace JottoWorol.BuildNumberSync.Editor
{
    public static class EditorCommands
    {
        /// <summary>
        /// Pulls the next build number from the remote API and assigns it to PlayerSettings.
        /// Note: This always uses remote storage, regardless of the configured storage mode.
        /// </summary>
        [MenuItem("Tools/Build Number Sync/Pull Next from Remote")]
        public static void PullNextFromRemote()
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var bundleId = PlayerSettings.applicationIdentifier;
            
            var provider = new RemoteBuildNumberProvider();

            if (!provider.TryGetNext(bundleId, buildTarget, out var buildNumber))
            {
                Debug.LogError($"{Logging.TAG} Failed to obtain build number from remote API.");
                return;
            }

            if (!BuildNumberHelper.TryAssignBuildNumber(buildTarget, buildNumber, out var errorMessage))
            {
                Debug.LogError($"{Logging.TAG} Failed to assign build number: {errorMessage}");
            }
            else
            {
                Debug.Log($"{Logging.TAG} Assigned build number {buildNumber} to PlayerSettings for {buildTarget}.");
            }
        }

        /// <summary>
        /// Pushes the current build number from PlayerSettings to the remote API.
        /// Note: This always uses remote storage, regardless of the configured storage mode.
        /// </summary>
        [MenuItem("Tools/Build Number Sync/Push Current to Remote")]
        public static void PushCurrentToRemote()
        {
            var bundleId = PlayerSettings.applicationIdentifier;
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var buildNumber = BuildNumberHelper.GetCurrentBuildNumber(buildTarget);
            
            var provider = new RemoteBuildNumberProvider();
            
            var result = EditorUtility.DisplayDialog("Push Build Number",
                $"Are you sure you want to push the current build number {buildNumber} for platform '{buildTarget}' to remote API?",
                "Yes", "No");
            
            if (!result)
            {
                Debug.Log($"{Logging.TAG} Aborted pushing build number to remote API.");
                return;
            }
            
            Debug.Log($"{Logging.TAG} Pushing current build number {buildNumber} for platform '{buildTarget}' to remote API...");
            
            if (provider.SetRemoteBuildNumber(bundleId, buildTarget, buildNumber))
            {
                Debug.Log($"{Logging.TAG} Successfully set build number {buildNumber} for '{buildTarget}' on remote API.");
            }
            else
            {
                Debug.LogError($"{Logging.TAG} Failed to set build number {buildNumber} for '{buildTarget}' on remote API.");
            }
        }

        /// <summary>
        /// Deletes the build number data for the current bundle ID and platform from the remote API.
        /// WARNING: This action cannot be undone. The next build will start from build number 1.
        /// Note: This always uses remote storage, regardless of the configured storage mode.
        /// </summary>
        [MenuItem("Tools/Build Number Sync/Delete Remote Data")]
        public static void DeleteRemoteData()
        {
            var bundleId = PlayerSettings.applicationIdentifier;
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            
            var provider = new RemoteBuildNumberProvider();
            
            var result = EditorUtility.DisplayDialog(
                "Delete Build Number Data",
                $"WARNING: This will permanently delete the build number data from remote API for:\n\n" +
                $"Bundle ID: {bundleId}\n" +
                $"Platform: {buildTarget}\n\n" +
                $"This action cannot be undone.\n" +
                $"The next build will start from build number 1.\n\n" +
                $"Are you sure you want to continue?",
                "Delete", 
                "Cancel");
            
            if (!result)
            {
                Debug.Log($"{Logging.TAG} Aborted deleting bundle data from remote API.");
                return;
            }
            
            Debug.Log($"{Logging.TAG} Deleting build number data for '{bundleId}' platform '{buildTarget}' from remote API...");
            
            if (provider.DeleteRemoteBuildNumber(bundleId, buildTarget))
            {
                Debug.Log($"{Logging.TAG} Successfully deleted build number data for '{bundleId}' platform '{buildTarget}' from remote API.");
            }
            else
            {
                Debug.LogError($"{Logging.TAG} Failed to delete build number data. Check console for details.");
            }
        }
    }
}