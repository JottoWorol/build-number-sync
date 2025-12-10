using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JottoWorol.BuildNumberSync.Editor
{
    internal class BuildNumberSyncConfigWindow : EditorWindow
    {
        private TextField apiBaseUrlField;
        private Label statusLabel;

        [MenuItem("Tools/Build Number Sync/Config", priority = 2000)]
        public static void OpenWindow()
        {
            var wnd = GetWindow<BuildNumberSyncConfigWindow>("Build Number Sync Config");
            wnd.minSize = new Vector2(420, 120);
            wnd.Show();

            // Mark seen so it won't auto-open next time
            BuildNumberSyncEditorPrefs.MarkConfigWindowSeen();
        }

        [InitializeOnLoadMethod]
        private static void OnEditorLoad()
        {
            EditorApplication.delayCall += () =>
            {
                if (!BuildNumberSyncEditorPrefs.HasSeenConfigWindow())
                {
                    OpenWindow();
                }
            };
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;
            container.style.paddingTop = 10;
            container.style.paddingBottom = 10;
            root.Add(container);

            var title = new Label("Build Number Sync Configuration");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 6;
            container.Add(title);

            apiBaseUrlField = new TextField("API Base URL");
            apiBaseUrlField.value = BuildNumberSyncEditorPrefs.GetApiBaseUrl(NetworkRequests.DEFAULT_API_BASE_URL);
            apiBaseUrlField.tooltip = "The base URL used for the build number API (leave blank to use package default).";
            container.Add(apiBaseUrlField);

            var buttonRow = new VisualElement();
            buttonRow.style.flexDirection = FlexDirection.Row;
            buttonRow.style.justifyContent = Justify.FlexStart;
            buttonRow.style.marginTop = 8;

            var saveBtn = new Button(Save) { text = "Save" };
            saveBtn.style.marginRight = 6;
            buttonRow.Add(saveBtn);

            var revertBtn = new Button(Revert) { text = "Revert" };
            buttonRow.Add(revertBtn);

            container.Add(buttonRow);

            statusLabel = new Label(string.Empty);
            statusLabel.style.marginTop = 8;
            container.Add(statusLabel);
        }

        private void Save()
        {
            var url = apiBaseUrlField.value?.Trim() ?? string.Empty;
            BuildNumberSyncEditorPrefs.SetApiBaseUrl(url);
            statusLabel.text = "Saved.";

            BaseApiUrlUpdater.InvalidateCache();
        }

        private void Revert()
        {
            apiBaseUrlField.value = BuildNumberSyncEditorPrefs.GetApiBaseUrl(NetworkRequests.DEFAULT_API_BASE_URL);
            statusLabel.text = "Reverted.";
        }
    }
}
