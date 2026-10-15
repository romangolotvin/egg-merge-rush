using System;
using System.IO;
using UnityEngine;

namespace EggMerge.Core
{
    public static class SaveSystem
    {
        static string Path => System.IO.Path.Combine(Application.persistentDataPath, "save.json");

        public static SaveData Load()
        {
            try
            {
                if (!File.Exists(Path))
                    return new SaveData();

                var json = File.ReadAllText(Path);
                var data = JsonUtility.FromJson<SaveData>(json);
                return data ?? new SaveData();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Save load failed: {e.Message}");
                return new SaveData();
            }
        }

        public static void Save(SaveData data)
        {
            try
            {
                var json = JsonUtility.ToJson(data, true);
                File.WriteAllText(Path, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Save write failed: {e.Message}");
            }
        }
    }
}
