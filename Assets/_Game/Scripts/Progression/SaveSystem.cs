// Assets/_Game/Scripts/Progression/SaveSystem.cs
using System.IO;
using System.Collections.Generic;
using MedievalRTS.Economy;
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
            try
            {
                var data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(path))
                           ?? new SaveData();
                Normalize(data);
                return data;
            }
            catch (JsonException ex)
            {
                Debug.LogError($"Failed to load save data: {ex.Message}");
                return new SaveData();
            }
        }

        public static void Delete(string path = null)
        {
            path ??= DefaultPath;
            if (File.Exists(path)) File.Delete(path);
        }

        private static void Normalize(SaveData data)
        {
            data.StageStars ??= new Dictionary<int, int>();
            data.UnitLevels ??= new Dictionary<string, int>();
            data.OwnedResources ??= new ResourceWallet();
            data.StoredResources ??= new ResourceWallet();
            NormalizeWallet(data.OwnedResources);
            NormalizeWallet(data.StoredResources);
            if (data.HeadquartersLevel <= 0) data.HeadquartersLevel = 1;
        }

        private static void NormalizeWallet(ResourceWallet wallet)
        {
            wallet.Amounts ??= new Dictionary<ResourceType, int>();
        }
    }
}
