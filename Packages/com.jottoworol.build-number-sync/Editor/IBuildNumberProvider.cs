using UnityEditor;

namespace JottoWorol.BuildNumberSync.Editor
{
    /// <summary>
    /// Provides build number retrieval and management functionality.
    /// </summary>
    internal interface IBuildNumberProvider
    {
        /// <summary>
        /// Attempts to get the next build number for the specified bundle ID and build target.
        /// </summary>
        /// <param name="bundleId">The application bundle identifier</param>
        /// <param name="buildTarget">The target build platform</param>
        /// <param name="buildNumber">The next build number if successful</param>
        /// <returns>True if successful, false otherwise</returns>
        bool TryGetNext(string bundleId, BuildTarget buildTarget, out int buildNumber);
    }
}

