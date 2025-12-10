using UnityEditor;

namespace JottoWorol.BuildNumberSync.Editor
{
    internal static class BuildNumberSyncEditorPrefs
    {
        public const string ApiBaseUrlKey = "JottoWorol.BuildNumberSync.ApiBaseUrl";
        public const string ConfigWindowSeenKey = "JottoWorol.BuildNumberSync.ConfigWindowSeen";

        public static bool HasApiBaseUrl() => EditorPrefs.HasKey(ApiBaseUrlKey) && !string.IsNullOrWhiteSpace(EditorPrefs.GetString(ApiBaseUrlKey));

        public static string GetApiBaseUrl(string fallback) => EditorPrefs.GetString(ApiBaseUrlKey, fallback ?? string.Empty);

        public static void SetApiBaseUrl(string url) => EditorPrefs.SetString(ApiBaseUrlKey, url ?? string.Empty);

        public static bool HasSeenConfigWindow() => EditorPrefs.GetBool(ConfigWindowSeenKey, false);

        public static void MarkConfigWindowSeen() => EditorPrefs.SetBool(ConfigWindowSeenKey, true);
    }
}
