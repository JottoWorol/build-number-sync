using UnityEditor;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace JottoWorol.BuildNumberSync.Editor
{
    [Serializable]
    internal class BuildNumberResponse
    {
        public int buildNumber;
    }

    internal class NetworkRequests
    {
        private const string GET_BUILD_NUMBER_ENDPOINT = "getNextBuildNumber?bundleId={0}&platform={1}";
        private const string SET_BUILD_NUMBER_ENDPOINT = "setBuildNumber?bundleId={0}&platform={1}&buildNumber={2}";
        private const string DELETE_BUNDLE_ID_ENDPOINT = "deleteBundleId?bundleId={0}&platform={1}";
        private const string EXPECTED_FORMAT = "JSON must contain an integer `buildNumber` field (e.g. { \"buildNumber\": 123 }).";

        private readonly string baseUrl;

        public NetworkRequests()
        {
            baseUrl = BuildNumberSyncSettingsProvider.GetApiBaseUrl();
        }

        /// <summary>
        /// Uses getNextBuildNumber endpoint to obtain the next build number for the given bundle id and platform.
        /// </summary>
        /// <param name="bundleId"></param>
        /// <param name="platform"></param>
        /// <param name="buildNumber"></param>
        public bool TryGetNextBuildNumber(string bundleId, string platform, out int buildNumber)
        {
            buildNumber = -1;
            if (string.IsNullOrWhiteSpace(bundleId)) return false;

            try
            {
                buildNumber = GetNextBuildNumber(bundleId, platform);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{Logging.TAG} TryGetNextBuildNumber error: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Uses setBuildNumber endpoint to set the build number in cloud for the given bundle id and platform.
        /// </summary>
        /// <param name="bundleId"></param>
        /// <param name="platform"></param>
        /// <param name="buildNumber"></param>
        public bool TrySetBuildNumber(string bundleId, string platform, int buildNumber)
        {
            if (string.IsNullOrWhiteSpace(bundleId)) return false;
            if (buildNumber < 0) return false;

            try
            {
                var ok = SetBuildNumber(bundleId, platform, buildNumber);
                return ok;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{Logging.TAG} TrySetBuildNumber error: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Uses deleteBundleId endpoint to delete the build number data for the given bundle id and platform.
        /// </summary>
        /// <param name="bundleId"></param>
        /// <param name="platform"></param>
        public bool TryDeleteBundleId(string bundleId, string platform)
        {
            if (string.IsNullOrWhiteSpace(bundleId)) return false;

            try
            {
                var ok = DeleteBundleId(bundleId, platform);
                return ok;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{Logging.TAG} TryDeleteBundleId error: {ex}");
                return false;
            }
        }
        
        public bool TryPingAPI()
        {
            if (string.IsNullOrWhiteSpace(baseUrl)) return false;

            var url = baseUrl.TrimEnd('/');

            var (responseCode, content, error, result) = SendGet(url);

            if (result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"{Logging.TAG} PingAPI error: Server returned {responseCode}. Error: {error}. Url: {url}");
                return false;
            }

            return true;
        }

        private int GetNextBuildNumber(string bundleId, string platform)
        {
            if (string.IsNullOrWhiteSpace(bundleId)) throw new ArgumentException("bundleId is required", nameof(bundleId));
            if (string.IsNullOrWhiteSpace(baseUrl)) throw new InvalidOperationException("API base URL is not configured.");

            var trimmedBase = baseUrl.TrimEnd('/');
            var url = trimmedBase + "/" + string.Format(GET_BUILD_NUMBER_ENDPOINT,
                Uri.EscapeDataString(bundleId),
                Uri.EscapeDataString(platform ?? string.Empty));

            var (responseCode, content, error, result) = SendGet(url);

            if (result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException($"Server returned {responseCode}. Error: {error}. Url: {url}");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new FormatException("Empty response");
            }

            try
            {
                var parsed = JsonUtility.FromJson<BuildNumberResponse>(content);
                if (parsed == null)
                    throw new FormatException("Unable to parse JSON response: " + EXPECTED_FORMAT);

                return parsed.buildNumber;
            }
            catch (Exception jsonEx) when (!(jsonEx is FormatException))
            {
                throw new FormatException("Invalid response format: " + EXPECTED_FORMAT, jsonEx);
            }
        }

        private bool SetBuildNumber(string bundleId, string platform, int buildNumber)
        {
            if (string.IsNullOrWhiteSpace(bundleId)) throw new ArgumentException("bundleId is required", nameof(bundleId));
            if (string.IsNullOrWhiteSpace(baseUrl)) throw new InvalidOperationException("API base URL is not configured.");

            var trimmedBase = baseUrl.TrimEnd('/');
            var url = trimmedBase + "/" + string.Format(SET_BUILD_NUMBER_ENDPOINT,
                Uri.EscapeDataString(bundleId),
                Uri.EscapeDataString(platform ?? string.Empty),
                Uri.EscapeDataString(buildNumber.ToString()));

            var (responseCode, content, error, result) = SendPost(url);

            if (result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException($"Server returned {responseCode}. Error: {error}. Body: {content}");
            }

            return true;
        }

        private bool DeleteBundleId(string bundleId, string platform)
        {
            if (string.IsNullOrWhiteSpace(bundleId)) throw new ArgumentException("bundleId is required", nameof(bundleId));
            if (string.IsNullOrWhiteSpace(baseUrl)) throw new InvalidOperationException("API base URL is not configured.");

            var trimmedBase = baseUrl.TrimEnd('/');
            var url = trimmedBase + "/" + string.Format(DELETE_BUNDLE_ID_ENDPOINT,
                Uri.EscapeDataString(bundleId),
                Uri.EscapeDataString(platform ?? string.Empty));

            var (responseCode, content, error, result) = SendDelete(url);

            if (result == UnityWebRequest.Result.Success)
            {
                return true;
            }

            // 404 is acceptable - bundle doesn't exist
            if (responseCode == 404)
            {
                Debug.LogWarning($"{Logging.TAG} Bundle ID '{bundleId}' for platform '{platform}' not found on server (already deleted or never existed).");
                return true;
            }

            throw new InvalidOperationException($"Server returned {responseCode}. Error: {error}. Body: {content}");
        }

        private static (long responseCode, string content, string error, UnityWebRequest.Result result) SendGet(string url)
        {
            using var request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 10; // seconds
            var operation = request.SendWebRequest();

            while (!operation.isDone) // TODO: improve?
            {
            }
            
            var content = request.downloadHandler?.text;
            return (request.responseCode, content, request.error, request.result);
        }
        
        private static (long responseCode, string content, string error, UnityWebRequest.Result result) SendPost(string url)
        {
            using var request = UnityWebRequest.PostWwwForm(url, "");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 10; // seconds
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
            }
            
            var content = request.downloadHandler?.text;
            return (request.responseCode, content, request.error, request.result);
        }

        private static (long responseCode, string content, string error, UnityWebRequest.Result result) SendDelete(string url)
        {
            using var request = UnityWebRequest.Delete(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 10; // seconds
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
            }
            
            var content = request.downloadHandler?.text;
            return (request.responseCode, content, request.error, request.result);
        }
    }
}
