using UnityEditor;
using UnityEngine;

namespace JottoWorol.BuildNumberSync.Editor
{
    /// <summary>
    /// ScriptableObject that holds Build Number Sync settings.
    /// This asset should be created via the menu and committed to version control
    /// so the whole team shares the same configuration.
    /// </summary>
    public class BuildNumberSyncSettings : ScriptableObject
    {
        private const string SETTINGS_ASSET_PATH = "Assets/BuildNumberSyncSettings.asset";
        private const string SETTINGS_ASSET_GUID_KEY = "BuildNumberSyncSettingsAssetGuid";

        [SerializeField]
        [Tooltip("The base URL used for the build number API. Leave blank to use the default URL.")]
        private string apiBaseUrl = string.Empty;

        public string ApiBaseUrl
        {
            get => string.IsNullOrWhiteSpace(apiBaseUrl) ? null : apiBaseUrl.Trim();
            set => apiBaseUrl = value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Creates a new settings asset at the default location and returns it.
        /// </summary>
        private static BuildNumberSyncSettings CreateSettingsAsset()
        {
            // Ensure directory exists
            var directoryPath = System.IO.Path.GetDirectoryName(SETTINGS_ASSET_PATH);
            if (!string.IsNullOrEmpty(directoryPath) && !AssetDatabase.IsValidFolder(directoryPath))
            {
                AssetDatabase.CreateFolder("Assets", "BuildNumberSync");
            }

            // Create the asset
            var settings = CreateInstance<BuildNumberSyncSettings>();
            AssetDatabase.CreateAsset(settings, SETTINGS_ASSET_PATH);
            AssetDatabase.SaveAssets();

            // Store the GUID for future reference
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(settings));
            EditorPrefs.SetString(SETTINGS_ASSET_GUID_KEY, guid);

            return settings;
        }

        /// <summary>
        /// Gets or loads the settings asset. Returns null if no asset exists.
        /// </summary>
        public static BuildNumberSyncSettings GetOrLoadSettings()
        {
            // Try to load from GUID stored in EditorPrefs
            if (EditorPrefs.HasKey(SETTINGS_ASSET_GUID_KEY))
            {
                var guid = EditorPrefs.GetString(SETTINGS_ASSET_GUID_KEY);
                if (!string.IsNullOrEmpty(guid))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrEmpty(path))
                    {
                        return AssetDatabase.LoadAssetAtPath<BuildNumberSyncSettings>(path);
                    }
                }
            }

            // Try to load from default path
            return AssetDatabase.LoadAssetAtPath<BuildNumberSyncSettings>(SETTINGS_ASSET_PATH);
        }

        /// <summary>
        /// Opens the settings asset for editing in the inspector.
        /// </summary>
        public static void OpenSettingsInInspector()
        {
            var settings = GetOrLoadSettings();
            if (settings == null)
            {
                settings = CreateSettingsAsset();
            }

            SelectObject(settings);
        }

        [MenuItem("Tools/Build Number Sync/Create Settings Asset", priority = 2001)]
        public static void CreateSettingsAssetMenu()
        {
            var settings = CreateSettingsAsset();
            Debug.Log($"{Logging.TAG} Created Build Number Sync settings asset at: Assets/BuildNumberSyncSettings.asset");
            SelectObject(settings);
        }

        [MenuItem("Tools/Build Number Sync/Open Settings", priority = 2002)]
        public static void OpenSettingsMenu()
        {
            OpenSettingsInInspector();
        }
        
        private static void SelectObject(Object unityObject)
        {
            Selection.activeObject = unityObject;
            EditorGUIUtility.PingObject(unityObject);
        }
    }
}