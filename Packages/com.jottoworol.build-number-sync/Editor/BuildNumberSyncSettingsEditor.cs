using UnityEditor;
using UnityEngine;

namespace JottoWorol.BuildNumberSync.Editor
{
    [CustomEditor(typeof(BuildNumberSyncSettings))]
    public class BuildNumberSyncSettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty useLocalOnlyProperty;
        private SerializedProperty useLocalAsFallbackProperty;
        private SerializedProperty apiBaseUrlProperty;
        private bool isTesting = false;
        private string testResult = "";
        private MessageType testResultType = MessageType.None;

        private void OnEnable()
        {
            useLocalOnlyProperty = serializedObject.FindProperty("useLocalOnly");
            useLocalAsFallbackProperty = serializedObject.FindProperty("useLocalAsFallback");
            apiBaseUrlProperty = serializedObject.FindProperty("apiBaseUrl");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            
            // Header
            EditorGUILayout.LabelField("Build Number Sync Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Storage Mode Selection
            EditorGUILayout.LabelField("Storage Mode", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(useLocalOnlyProperty, new GUIContent("Use Local Only"));
            
            if (useLocalOnlyProperty.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "Local Storage: Build numbers are stored on your machine and incremented from current PlayerSettings.\n" +
                    "No remote connection required. Numbers are not shared across team members.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Remote Storage: Build numbers are synchronized via API.\n" +
                    "Numbers are shared across team members.",
                    MessageType.Info);
                
                // Fallback option (only visible in remote storage mode)
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(useLocalAsFallbackProperty, new GUIContent("Use Local as Fallback"));
                EditorGUILayout.HelpBox(
                    "If enabled, build preprocess will automatically use local storage if remote storage fails.\n" +
                    "This prevents build failures due to remote API issues.",
                    MessageType.Info);
            }

            EditorGUILayout.Space();

            // API Base URL field (only visible in remote storage mode)
            if (!useLocalOnlyProperty.boolValue)
            {
                EditorGUILayout.LabelField("Remote Configuration", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(apiBaseUrlProperty, new GUIContent("API Base URL"));
                
                // Help box explaining the field
                if (string.IsNullOrWhiteSpace(apiBaseUrlProperty.stringValue))
                {
                    EditorGUILayout.HelpBox(
                        $"Using default API: {BuildNumberSyncSettingsProvider.GetApiBaseUrl()}\n\n" +
                        "For production, it is strongly recommended to deploy your own custom API instance.\n" +
                        "The free public API is available for testing, but may have rate limits and availability constraints.",
                        MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Using custom API URL. Make sure it implements the required endpoints.",
                        MessageType.Info);
                }

                EditorGUILayout.Space();

                // Test Connection button
                EditorGUI.BeginDisabledGroup(isTesting);
                if (GUILayout.Button(isTesting ? "Testing Connection..." : "Test Connection"))
                {
                    TestConnection();
                }
                EditorGUI.EndDisabledGroup();

                // Show test result
                if (!string.IsNullOrEmpty(testResult))
                {
                    EditorGUILayout.HelpBox(testResult, testResultType);
                }

                EditorGUILayout.Space();
            }

            // Current project info
            EditorGUILayout.LabelField("Current Project Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Bundle ID:", PlayerSettings.applicationIdentifier);
            EditorGUILayout.LabelField("Active Platform:", EditorUserBuildSettings.activeBuildTarget.ToString());
            
            // Try to get current build number
            try
            {
                var currentBuildNumber = BuildNumberHelper.GetCurrentBuildNumber(EditorUserBuildSettings.activeBuildTarget);
                EditorGUILayout.LabelField("Current Build Number:", currentBuildNumber.ToString());
            }
            catch
            {
                EditorGUILayout.LabelField("Current Build Number:", "Not set or invalid");
            }

            EditorGUILayout.Space();

            // Links
            if (GUILayout.Button("Open Deployment Guides"))
            {
                Application.OpenURL("https://github.com/JottoWorol/build-number-sync#deploying-your-own-api");
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void TestConnection()
        {
            isTesting = true;
            testResult = "";
            testResultType = MessageType.None;
            Repaint();

            var networkRequests = new NetworkRequests();

            try
            {
                if (networkRequests.TryPingRemote())
                {
                    testResult = $"✓ Connection successful!\n\nAPI URL: {BuildNumberSyncSettingsProvider.GetApiBaseUrl()}";
                    testResultType = MessageType.Info;
                }
                else
                {
                    testResult = "✗ Connection failed.\n\nCheck your internet connection and API URL configuration.";
                    testResultType = MessageType.Error;
                }
            }
            catch (System.Exception ex)
            {
                testResult = $"✗ Connection error:\n\n{ex.Message}\n\nCheck console for details.";
                testResultType = MessageType.Error;
                Debug.LogException(ex);
            }
            finally
            {
                isTesting = false;
                Repaint();
            }
        }
    }
}

