using UnityEditor;
using UnityEngine;

namespace JottoWorol.BuildNumberSync.Editor
{
    internal static class BaseApiUrlUpdater
    {
        private static string cachedApiBaseUrl;

        /// <summary>
        /// Invalidate any cached API base URL so subsequent calls re-read EditorPrefs.
        /// </summary>
        public static void InvalidateCache()
        {
            cachedApiBaseUrl = null;
        }

        /// <summary>
        /// Tries to get the updated API base URL from EditorPrefs. Falls back to a default constant.
        /// </summary>
        /// <param name="apiBaseUrl"></param>
        public static bool TryGetUpdatedApiBaseUrl(out string apiBaseUrl)
        {
            if (!string.IsNullOrEmpty(cachedApiBaseUrl))
            {
                apiBaseUrl = cachedApiBaseUrl;
                return true;
            }

            // Prefer EditorPrefs value if set
            if (BuildNumberSyncEditorPrefs.HasApiBaseUrl())
            {
                var value = BuildNumberSyncEditorPrefs.GetApiBaseUrl(NetworkRequests.DEFAULT_API_BASE_URL)?.Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    cachedApiBaseUrl = value;
                    apiBaseUrl = value;
                    return true;
                }
            }

            // No EditorPrefs set; fallback to default
            apiBaseUrl = NetworkRequests.DEFAULT_API_BASE_URL;
            return false;
        }
    }
}