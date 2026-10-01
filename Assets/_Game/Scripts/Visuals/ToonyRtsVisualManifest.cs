using System;
using UnityEngine;

namespace MedievalRTS.Visuals
{
    [CreateAssetMenu(menuName = "MadDragon/Toony RTS Visual Manifest")]
    public class ToonyRtsVisualManifest : ScriptableObject
    {
        public const string ResourcesPath = "ToonyRts/ToonyRtsVisualManifest";

        [SerializeField] private VisualEntry[] units = Array.Empty<VisualEntry>();
        [SerializeField] private VisualEntry[] buildings = Array.Empty<VisualEntry>();
        [SerializeField] private VisualEntry[] decorations = Array.Empty<VisualEntry>();
        [SerializeField] private VisualEntry[] effects = Array.Empty<VisualEntry>();

        public VisualEntry[] Units => units;
        public VisualEntry[] Buildings => buildings;
        public VisualEntry[] Decorations => decorations;
        public VisualEntry[] Effects => effects;

        public GameObject FindUnit(string key) => Find(units, key);
        public GameObject FindBuilding(string key) => Find(buildings, key);
        public GameObject FindDecoration(string key) => Find(decorations, key);
        public GameObject FindEffect(string key) => Find(effects, key);

        public void SetEntries(VisualEntry[] unitEntries, VisualEntry[] buildingEntries, VisualEntry[] decorationEntries, VisualEntry[] effectEntries)
        {
            units = unitEntries ?? Array.Empty<VisualEntry>();
            buildings = buildingEntries ?? Array.Empty<VisualEntry>();
            decorations = decorationEntries ?? Array.Empty<VisualEntry>();
            effects = effectEntries ?? Array.Empty<VisualEntry>();
        }

        private static GameObject Find(VisualEntry[] entries, string key)
        {
            if (entries == null || string.IsNullOrEmpty(key)) return null;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].Key == key) return entries[i].Prefab;
            }
            return null;
        }

        [Serializable]
        public struct VisualEntry
        {
            [SerializeField] private string key;
            [SerializeField] private GameObject prefab;

            public string Key => key;
            public GameObject Prefab => prefab;

            public VisualEntry(string key, GameObject prefab)
            {
                this.key = key;
                this.prefab = prefab;
            }
        }
    }
}
