using System;
using UnityEditor;

namespace JottoWorol.BuildNumberSync.Editor
{
    internal static class BuildNumberHelper
    {
        /// <summary>
        /// Assign the build number to PlayerSettings based on the target platform.
        /// Some platforms have specific fields for build numbers, while others
        /// use the bundle version format.
        /// Android, iOS, tvOS, macOS, PS4 have dedicated build number fields.
        /// WebGL uses the last part of PlayerSettings.bundleVersion (x.x.x).
        /// WSA uses the Build part of PlayerSettings.WSA.packageVersion.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="buildNumber"></param>
        /// <param name="errorMessage"></param>
        public static bool TryAssignBuildNumber(BuildTarget target, int buildNumber, out string errorMessage)
        {
            switch(target)
            {
                case BuildTarget.iOS:
                    PlayerSettings.iOS.buildNumber = buildNumber.ToString();
                    break;
                case BuildTarget.Android:
                    PlayerSettings.Android.bundleVersionCode = buildNumber;
                    break;
                case BuildTarget.WebGL:
                    if (!TryAssignWebGLBuildNumber(buildNumber, out errorMessage))
                    {
                        return false;
                    }
                    break;
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneOSX:
                    PlayerSettings.macOS.buildNumber = buildNumber.ToString();
                    break;
                case BuildTarget.PS4:
                    PlayerSettings.PS4.appVersion = buildNumber.ToString();
                    break;
                case BuildTarget.WSAPlayer:
                    if (!TryAssignWSABuildNumber(buildNumber, out errorMessage))
                    {
                        return false;
                    }
                    break;
                case BuildTarget.tvOS:
                    PlayerSettings.tvOS.buildNumber = buildNumber.ToString();
                    break;
                default:
                    errorMessage = $"Unsupported build target: {target}";
                    return false;
            }
            
            errorMessage = null;
            return true;
        }
        
        /// <summary>
        /// Retrieve the current build number from PlayerSettings based on the target platform.
        /// Throws exceptions if the build number cannot be retrieved or parsed.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="FormatException"></exception>
        public static int GetCurrentBuildNumber(BuildTarget target)
        {
            string stringResult;
            
            switch(target)
            {
                case BuildTarget.iOS:
                    stringResult = PlayerSettings.iOS.buildNumber;
                    break;
                case BuildTarget.Android:
                    return PlayerSettings.Android.bundleVersionCode;
                case BuildTarget.WebGL:
                    var currentVersion = PlayerSettings.bundleVersion;
                    
                    if (!ValidateWebGLVersionFormat(currentVersion))
                    {
                        throw new InvalidOperationException(
                            $"Current version '{currentVersion}' is not in expected x.x.x format for WebGL builds.");
                    }
                    
                    var versionParts = currentVersion.Split('.');
                    stringResult = versionParts[^1];
                    break;
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneOSX:
                    stringResult = PlayerSettings.macOS.buildNumber;
                    break;
                case BuildTarget.PS4:
                    stringResult =  PlayerSettings.PS4.appVersion;
                    break;
                case BuildTarget.WSAPlayer:
                    return PlayerSettings.WSA.packageVersion.Build;
                case BuildTarget.tvOS:
                    stringResult =  PlayerSettings.tvOS.buildNumber;
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported build target: {target}");
            }
            
            if (!int.TryParse(stringResult, out var result))
            {
                throw new FormatException($"Current build number '{stringResult}' is not a valid integer for target {target}");
            }
            
            return result;
        }

        /// <summary>
        /// Assign the build number part of PlayerSettings.WSA.packageVersion for WSA builds.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="errorMessage"></param>
        private static bool TryAssignWSABuildNumber(int version, out string errorMessage)
        {
            var currentVersion = PlayerSettings.WSA.packageVersion;
            PlayerSettings.WSA.packageVersion = new Version(
                currentVersion.Major,
                currentVersion.Minor,
                version);
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Assign the build number part of PlayerSettings.bundleVersion for WebGL builds.
        /// Assumes x.x.x format and replaces the last part with the provided version.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="errorMessage"></param>
        private static bool TryAssignWebGLBuildNumber(int version, out string errorMessage)
        {
            var currentVersion = PlayerSettings.bundleVersion;
            
            if (!ValidateWebGLVersionFormat(currentVersion))
            {
                errorMessage = $"Current version '{currentVersion}' is not in expected x.x.x format for WebGL builds.";
                return false;
            }
            
            var versionParts = currentVersion.Split('.');
            versionParts[^1] = version.ToString();
            PlayerSettings.bundleVersion = string.Join(".", versionParts);
            
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Ensure PlayerSettings.bundleVersion for WebGL is in x.x.x format.
        /// </summary>
        private static bool ValidateWebGLVersionFormat(string versionString)
        {
            versionString ??= string.Empty;
            var parts = versionString.Split('.');
            return parts.Length == 3;
        }
    }
}