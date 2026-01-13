using UnityEditor;

namespace JottoWorol.BuildNumberSync.Editor
{
    /// <summary>
    /// Provides access to Build Number Sync settings with the following priority:
    /// 1. Settings asset (if exists)
    /// 2. Default values
    /// </summary>
    internal static class BuildNumberSyncSettingsProvider
    {
        private const string DEFAULT_API_BASE_URL = "https://build-number-sync.jottoworol.top";

        /// <summary>
        /// Gets the current API base URL from settings asset, or returns the default URL.
        /// </summary>
        public static string GetApiBaseUrl()
        {
            if (TryGetSettings(out var settings) && !string.IsNullOrEmpty(settings.ApiBaseUrl))
            {
                return settings.ApiBaseUrl;
            }

            return DEFAULT_API_BASE_URL;
        }

        /// <summary>
        /// Gets whether to use local storage mode instead of remote storage.
        /// Returns false if no settings asset exists (defaults to remote storage mode).
        /// </summary>
        public static bool GetUseLocalProvider()
        {
            if (TryGetSettings(out var settings))
            {
                return settings.UseLocalOnly;
            }

            return false;
        }

        /// <summary>
        /// Gets whether to use local storage as fallback when remote storage mode fails during build preprocess.
        /// Returns true if no settings asset exists (defaults to fallback enabled).
        /// </summary>
        public static bool GetUseLocalAsFallback()
        {
            if (TryGetSettings(out var settings))
            {
                return settings.UseLocalAsFallback;
            }

            return true;
        }
        
        /// <summary>
        /// Attempts to get settings from the settings asset.
        /// </summary>
        private static bool TryGetSettings(out BuildNumberSyncSettings settings)
        {
            settings = BuildNumberSyncSettings.GetOrLoadSettings();
            return settings != null;
        }
    }
}