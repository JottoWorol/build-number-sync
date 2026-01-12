namespace JottoWorol.BuildNumberSync.Runtime
{
    public static class BuildNumberProvider
    {
        /// <summary>
        /// Tries to get the current build number from the asset managed by BuildNumberAssetIO.
        /// </summary>
        /// <param name="buildNumber"></param>
        /// <returns></returns>
        public static bool TryGetCurrentBuildNumber(out int buildNumber)
        {
            if (BuildNumberAssetIO.TryLoadBuildNumberData(out var buildNumberData))
            {
                buildNumber = buildNumberData.BuildNumber;
                return true;
            }
            
            buildNumber = -1;
            return false;
        }
    }
}