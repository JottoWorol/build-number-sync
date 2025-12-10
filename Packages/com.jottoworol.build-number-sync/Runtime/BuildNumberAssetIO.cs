using System.IO;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Networking;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JottoWorol.BuildNumberSync.Runtime
{
    public static class BuildNumberAssetIO
    {
        private const string STREAMING_ASSETS_DIR = "Assets/StreamingAssets";
        private const string FILE_NAME = "build_number.json";

#if UNITY_EDITOR
        public static void Write(int buildNumber)
        {
            var buildNumberJson = new BuildNumberRuntimeData(buildNumber).ToJson();
            Directory.CreateDirectory(STREAMING_ASSETS_DIR);
            var path = Path.Combine(STREAMING_ASSETS_DIR, FILE_NAME);
            File.WriteAllText(path, buildNumberJson);
            AssetDatabase.ImportAsset(path);
        }
#endif
        
        internal static bool TryLoadBuildNumberData(out BuildNumberRuntimeData data)
        {
            data = null;
            var path = Path.Combine(Application.streamingAssetsPath, FILE_NAME);

#if UNITY_ANDROID && !UNITY_EDITOR
            using var request = new UnityWebRequest(path);
            request.downloadHandler = new DownloadHandlerBuffer();
            var operation = request.SendWebRequest();
            while (!operation.isDone) { }


            if (request.result == UnityWebRequest.Result.Success
                && BuildNumberRuntimeData.TryCreateFromJson(request.downloadHandler.text, out data))
            {
                return true;
            }
#else
            if (File.Exists(path)
                && BuildNumberRuntimeData.TryCreateFromJson(File.ReadAllText(path).Trim(), out data))
            {
                return true;
            }
#endif
            Debug.LogError($"Failed to load build number: {path}");
            return false;
        }
    }
}
