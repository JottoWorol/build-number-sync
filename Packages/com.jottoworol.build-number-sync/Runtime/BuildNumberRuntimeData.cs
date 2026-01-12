using System;
using UnityEngine;

namespace JottoWorol.BuildNumberSync.Runtime
{
    /// <summary>
    /// Runtime data structure for storing the build number.
    /// </summary>
    [Serializable]
    public class BuildNumberRuntimeData
    {
        [SerializeField] private int buildNumber;
        
        public int BuildNumber => buildNumber;

        public BuildNumberRuntimeData(int buildNumber)
        {
            this.buildNumber = buildNumber;
        }

        public static bool TryCreateFromJson(string jsonString, out BuildNumberRuntimeData buildNumberData)
        {
            try
            {
                buildNumberData = JsonUtility.FromJson<BuildNumberRuntimeData>(jsonString);
                return true;
            }
            catch (Exception)
            {
                buildNumberData = null;
                return false;
            }
        }
        
        public string ToJson()
        {
            return JsonUtility.ToJson(this, false);
        }
    }
}