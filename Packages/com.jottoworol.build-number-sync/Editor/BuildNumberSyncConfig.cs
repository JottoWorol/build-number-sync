using System;
using UnityEngine;

namespace JottoWorol.BuildNumberSync.Editor
{
    /// <summary>
    /// Optional configuration asset for Build Number Sync settings.
    /// Allows customization of the API base URL used for network requests.
    /// </summary>
    [CreateAssetMenu(
        fileName = "BuildNumberSyncConfig",
        menuName = "Build Number Sync/Build Number Sync Config",
        order = 0)]
    public class BuildNumberSyncConfig: ScriptableObject
    {
        [field: SerializeField]
        public string CustomApiBaseUrl { get; private set; } = NetworkRequests.DEFAULT_API_BASE_URL;
        
        private void OnValidate()
        {
            CustomApiBaseUrl = CustomApiBaseUrl?.Trim() ?? string.Empty;
            
            if (!TryValidate(out var errorMessage))
            {
                Debug.LogError($"BuildNumberSyncConfig validation error: {errorMessage} Reverting to default API base URL.");
                CustomApiBaseUrl = NetworkRequests.DEFAULT_API_BASE_URL;
            }
        }
        
        public bool TryValidate(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(CustomApiBaseUrl))
            {
                errorMessage = "CustomApiBaseUrl cannot be empty.";
                return false;
            }
            
            if (!Uri.IsWellFormedUriString(CustomApiBaseUrl, UriKind.Absolute))
            {
                errorMessage = $"CustomApiBaseUrl '{CustomApiBaseUrl}' is not a valid URL.";
                return false;
            }

            return true;
        }
    }
}