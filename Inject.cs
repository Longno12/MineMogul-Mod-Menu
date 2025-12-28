using System;
using UnityEngine;

namespace Loading
{
    public class Loader
    {
        private static GameObject _loaderObject;

        public static void Load()
        {
            if (_loaderObject != null) return;
            _loaderObject = new GameObject("MineMoguul");
            _loaderObject.AddComponent<MineMoguul_Mod.ModMenu>();
            UnityEngine.Object.DontDestroyOnLoad(_loaderObject);
        }

        public static void UnloadCheat()
        {
            if (_loaderObject == null) return;
            UnityEngine.Object.Destroy(_loaderObject);
            _loaderObject = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}