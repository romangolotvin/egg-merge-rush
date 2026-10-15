using UnityEngine;

namespace EggMerge.Core
{
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            if (Object.FindFirstObjectByType<GameManager>() != null)
                return;

            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }
    }
}
