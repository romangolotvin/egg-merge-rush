using System;
using UnityEngine;

namespace EggMerge.Ads
{
    /// <summary>
    /// Stub ads for Editor / early builds.
    /// Swap body for Unity LevelPlay / AdMob when store keys are ready.
    /// </summary>
    public class AdService : MonoBehaviour
    {
        public static AdService Instance { get; private set; }

        public bool IsRewardedReady => true;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ShowRewarded(string placement, Action<bool> onFinished)
        {
            Debug.Log($"[Ads] Rewarded stub: {placement}");
#if UNITY_EDITOR
            onFinished?.Invoke(true);
#else
            // Simulate short delay like a real ad close.
            StartCoroutine(FakeWatch(onFinished));
#endif
        }

        public void ShowInterstitial(string placement)
        {
            Debug.Log($"[Ads] Interstitial stub: {placement}");
        }

        System.Collections.IEnumerator FakeWatch(Action<bool> onFinished)
        {
            yield return new WaitForSecondsRealtime(0.35f);
            onFinished?.Invoke(true);
        }
    }
}
