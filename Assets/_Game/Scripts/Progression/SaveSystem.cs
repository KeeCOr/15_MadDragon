// Assets/_Game/Scripts/Progression/SaveSystem.cs
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace MedievalRTS.Progression
{
    public static class SaveSystem
    {
        private static string DefaultPath =>
            Path.Combine(Application.persistentDataPath, "save.json");

        public static void Save(SaveData data, string path = null)
        {
            path ??= DefaultPath;
            File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public static SaveData Load(string path = null)
        {
            path ??= DefaultPath;
            if (!File.Exists(path)) return new SaveData();
            return JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(path))
                   ?? new SaveData();
        }

        public static void Delete(string path = null)
        {
            path ??= DefaultPath;
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
