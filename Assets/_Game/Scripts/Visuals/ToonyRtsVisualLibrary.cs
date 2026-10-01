using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MedievalRTS.Visuals
{
    public static class ToonyRtsVisualLibrary
    {
        public const string AssetRoot = "Assets/ToonyTinyPeople/TT_RTS/TT_RTS_Standard/";

        private static ToonyRtsVisualManifest _manifest;

        public static ToonyRtsVisualManifest Manifest
        {
            get
            {
                if (_manifest == null)
                    _manifest = Resources.Load<ToonyRtsVisualManifest>(ToonyRtsVisualManifest.ResourcesPath);
                return _manifest;
            }
        }

        public static GameObject LoadUnit(string assetName)
        {
            var fromManifest = Manifest != null ? Manifest.FindUnit(assetName) : null;
            if (fromManifest != null) return fromManifest;

            return assetName switch
            {
                "Knight" => LoadEditorOnly("prefabs/TT_Swordman.prefab"),
                "Archer" => LoadEditorOnly("prefabs/TT_Archer.prefab"),
                "Mage" => LoadEditorOnly("prefabs/TT_Mage.prefab"),
                "Scout" => LoadEditorOnly("prefabs/TT_Scout.prefab"),
                "Cavalry" => LoadEditorOnly("prefabs/TT_Light_Cavalry.prefab"),
                "Catapult" => LoadEditorOnly("prefabs/machines/TT_Catapult_lvl1.prefab"),
                _ => null
            };
        }

        public static GameObject LoadBuilding(string key)
        {
            var fromManifest = Manifest != null ? Manifest.FindBuilding(key) : null;
            if (fromManifest != null) return fromManifest;

            return key switch
            {
                "player_castle" => LoadEditorOnly("models/buildings/TownHall.FBX"),
                "enemy_castle" => LoadEditorOnly("models/buildings/Keep.FBX"),
                "wall" => LoadEditorOnly("models/buildings/Wall_A_wall.FBX"),
                "tower" => LoadEditorOnly("models/buildings/Tower_A.FBX"),
                "barracks" => LoadEditorOnly("models/buildings/Stables.FBX"),
                "mage_tower" => LoadEditorOnly("models/buildings/MageTower.FBX"),
                "elixir_well" => LoadEditorOnly("models/buildings/Temple.FBX"),
                "gold_mine" => LoadEditorOnly("models/buildings/Granary.FBX"),
                _ => null
            };
        }

        public static GameObject LoadDecoration(string key)
        {
            var fromManifest = Manifest != null ? Manifest.FindDecoration(key) : null;
            if (fromManifest != null) return fromManifest;

            return key switch
            {
                "blue_banner" => LoadEditorOnly("prefabs/banners/TT_Banner_Blue_A.prefab"),
                "red_banner" => LoadEditorOnly("prefabs/banners/TT_Banner_Red.prefab"),
                _ => null
            };
        }

        public static GameObject LoadEffect(string key)
        {
            var fromManifest = Manifest != null ? Manifest.FindEffect(key) : null;
            if (fromManifest != null) return fromManifest;

            return key switch
            {
                "building_destroyed" => LoadEditorOnly("FX/FX_prefabs/FX_Building_Destroyed_mid.prefab"),
                "building_burning" => LoadEditorOnly("FX/FX_prefabs/FX_Building_burning.prefab"),
                "machine_destroyed" => LoadEditorOnly("FX/FX_prefabs/FX_machine_destroyed.prefab"),
                _ => null
            };
        }

        public static bool HasManifestAsset()
        {
            return Manifest != null;
        }

        private static GameObject LoadEditorOnly(string relativePath)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<GameObject>(AssetRoot + relativePath);
#else
            return null;
#endif
        }
    }
}
