using UnityEditor;
using UnityEngine;

namespace JottoWorol.BuildNumberSync.Editor
{
    /// <summary>
    /// Provides access to Build Number Sync API base URL with the following priority:
    /// 1. Settings asset (if exists)
    /// 2. EditorPrefs (for backwards compatibility with key JottoWorol.BuildNumberSync.ApiBaseUrl)
    /// 3. Default URL constant
    /// </summary>
    internal static class BuildNumberSyncSettingsProvider
    {
        private const string DEFAULT_API_BASE_URL = "https://build-number-sync.jottoworol.top";
        private const string LEGACY_API_BASE_URL_KEY = "JottoWorol.BuildNumberSync.ApiBaseUrl";

        private static string cachedApiBaseUrl;

        /// <summary>
        /// Gets the current API base URL according to the priority:
        /// 1. Settings asset (if exists)
        /// 2. EditorPrefs (for backwards compatibility)
        /// 3. Default URL constant
        /// </summary>
        public static string GetApiBaseUrl()
        {
            if (!string.IsNullOrEmpty(cachedApiBaseUrl))
            {
                return cachedApiBaseUrl;
            }

            if (TryGetSettings(out var settings) && !string.IsNullOrEmpty(settings.ApiBaseUrl))
            {
                cachedApiBaseUrl = settings.ApiBaseUrl;
                return cachedApiBaseUrl;
            }

            if (TryGetLegacyApiBaseUrl(out var editorPrefsUrl))
            {
                if (!string.IsNullOrWhiteSpace(editorPrefsUrl))
                {
                    cachedApiBaseUrl = editorPrefsUrl;
                    return cachedApiBaseUrl;
                }
            }

            Debug.LogWarning($"{Logging.TAG} No API base url provided, using default API base url.");
            cachedApiBaseUrl = DEFAULT_API_BASE_URL;
            return cachedApiBaseUrl;
        }
        
        private static bool TryGetSettings(out BuildNumberSyncSettings settings)
        {
            settings = BuildNumberSyncSettings.GetOrLoadSettings();
            
            if (settings != null)
            {
                CleanupLegacyEditorPrefs();
            }
            
            return settings != null;
        }
        
        private static bool TryGetLegacyApiBaseUrl(out string apiBaseUrl)
        {
            if (EditorPrefs.HasKey(LEGACY_API_BASE_URL_KEY))
            {
                apiBaseUrl = EditorPrefs.GetString(LEGACY_API_BASE_URL_KEY, DEFAULT_API_BASE_URL);
                return true;
            }

            apiBaseUrl = null;
            return false;
        }
        
        private static void CleanupLegacyEditorPrefs()
        {
            if (EditorPrefs.HasKey(LEGACY_API_BASE_URL_KEY))
            {
                EditorPrefs.DeleteKey(LEGACY_API_BASE_URL_KEY);
            }
        }
    }
}