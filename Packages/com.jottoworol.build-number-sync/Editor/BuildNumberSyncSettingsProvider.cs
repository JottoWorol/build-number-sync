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

        /// <summary>
        /// Gets the current API base URL according to the priority:
        /// 1. Settings asset (if exists)
        /// 2. EditorPrefs (for backwards compatibility)
        /// 3. Default URL constant
        /// </summary>
        public static string GetApiBaseUrl()
        {
            if (TryGetSettings(out var settings) && !string.IsNullOrEmpty(settings.ApiBaseUrl))
            {
                return settings.ApiBaseUrl;
            }

            if (TryGetLegacyApiBaseUrl(out var editorPrefsUrl))
            {
                if (!string.IsNullOrWhiteSpace(editorPrefsUrl))
                {
                    return editorPrefsUrl;
                }
            }

            return DEFAULT_API_BASE_URL;
        }
        
        /// <summary>
        /// Attempts to get settings from the settings asset and cleans up legacy EditorPrefs if settings exist.
        /// </summary>
        private static bool TryGetSettings(out BuildNumberSyncSettings settings)
        {
            settings = BuildNumberSyncSettings.GetOrLoadSettings();
            
            if (settings != null)
            {
                CleanupLegacyEditorPrefs();
            }
            
            return settings != null;
        }
        
        /// <summary>
        /// Attempts to get the API base URL from legacy EditorPrefs for backwards compatibility.
        /// </summary>
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

        /// <summary>
        /// Cleans up legacy EditorPrefs when settings asset is found to avoid using outdated configuration.
        /// </summary>
        private static void CleanupLegacyEditorPrefs()
        {
            if (EditorPrefs.HasKey(LEGACY_API_BASE_URL_KEY))
            {
                EditorPrefs.DeleteKey(LEGACY_API_BASE_URL_KEY);
            }
        }
    }
}