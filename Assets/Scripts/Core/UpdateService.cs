using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace EggMerge.Core
{
    [Serializable]
    public class UpdateManifest
    {
        public string version;
        public string notes;
        public string android;
        public string windows;
    }

    public class UpdateService : MonoBehaviour
    {
        public static UpdateService Instance { get; private set; }

        public UpdateManifest Remote { get; private set; }
        public bool UpdateAvailable { get; private set; }
        public string Status { get; private set; } = "";

        public event Action StateChanged;

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

        public void CheckNow()
        {
            StopAllCoroutines();
            StartCoroutine(CheckRoutine());
        }

        IEnumerator CheckRoutine()
        {
            Status = "Проверка обновлений…";
            UpdateAvailable = false;
            StateChanged?.Invoke();

            using var req = UnityWebRequest.Get(UpdateConfig.ManifestUrl);
            req.timeout = 12;
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Status = "Не удалось проверить обновление";
                StateChanged?.Invoke();
                yield break;
            }

            try
            {
                Remote = JsonUtility.FromJson<UpdateManifest>(req.downloadHandler.text);
            }
            catch (Exception e)
            {
                Status = $"Плохой манифест: {e.Message}";
                StateChanged?.Invoke();
                yield break;
            }

            if (Remote == null || string.IsNullOrEmpty(Remote.version))
            {
                Status = "Манифест пустой";
                StateChanged?.Invoke();
                yield break;
            }

            UpdateAvailable = IsNewer(Remote.version, UpdateConfig.AppVersion);
            Status = UpdateAvailable
                ? $"Доступна {Remote.version}: {Remote.notes}"
                : $"Актуальная версия {UpdateConfig.AppVersion}";
            StateChanged?.Invoke();
        }

        public void OpenDownload()
        {
            if (Remote == null || string.IsNullOrEmpty(Remote.android))
                return;
            Application.OpenURL(Remote.android);
        }

        static bool IsNewer(string remote, string local)
        {
            try
            {
                var a = new Version(Normalize(remote));
                var b = new Version(Normalize(local));
                return a > b;
            }
            catch
            {
                return !string.Equals(remote, local, StringComparison.OrdinalIgnoreCase);
            }
        }

        static string Normalize(string v)
        {
            var parts = v.Trim().TrimStart('v', 'V').Split('.');
            if (parts.Length == 2) return $"{parts[0]}.{parts[1]}.0";
            if (parts.Length == 1) return $"{parts[0]}.0.0";
            return string.Join(".", parts);
        }
    }
}
