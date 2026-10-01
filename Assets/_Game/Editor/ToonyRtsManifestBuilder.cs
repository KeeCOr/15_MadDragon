using System.IO;
using MedievalRTS.Visuals;
using UnityEditor;
using UnityEngine;

namespace MedievalRTS.Editor
{
    public static class ToonyRtsManifestBuilder
    {
        private const string ManifestDir = "Assets/Resources/ToonyRts";
        private const string ManifestPath = ManifestDir + "/ToonyRtsVisualManifest.asset";

        [MenuItem("MadDragon/Rebuild Toony RTS Visual Manifest")]
        public static void Build()
        {
            if (!Directory.Exists(ManifestDir))
                Directory.CreateDirectory(ManifestDir);

            var manifest = AssetDatabase.LoadAssetAtPath<ToonyRtsVisualManifest>(ManifestPath);
            if (manifest == null)
            {
                manifest = ScriptableObject.CreateInstance<ToonyRtsVisualManifest>();
                AssetDatabase.CreateAsset(manifest, ManifestPath);
            }

            manifest.SetEntries(
                new[]
                {
                    Entry("Knight", "prefabs/TT_Swordman.prefab"),
                    Entry("Archer", "prefabs/TT_Archer.prefab"),
                    Entry("Mage", "prefabs/TT_Mage.prefab"),
                    Entry("Scout", "prefabs/TT_Scout.prefab"),
                    Entry("Cavalry", "prefabs/TT_Light_Cavalry.prefab"),
                    Entry("Catapult", "prefabs/machines/TT_Catapult_lvl1.prefab"),
                },
                new[]
                {
                    Entry("player_castle", "models/buildings/TownHall.FBX"),
                    Entry("enemy_castle", "models/buildings/Keep.FBX"),
                    Entry("wall", "models/buildings/Wall_A_wall.FBX"),
                    Entry("tower", "models/buildings/Tower_A.FBX"),
                    Entry("barracks", "models/buildings/Stables.FBX"),
                    Entry("mage_tower", "models/buildings/MageTower.FBX"),
                    Entry("elixir_well", "models/buildings/Temple.FBX"),
                    Entry("gold_mine", "models/buildings/Granary.FBX"),
                },
                new[]
                {
                    Entry("blue_banner", "prefabs/banners/TT_Banner_Blue_A.prefab"),
                    Entry("red_banner", "prefabs/banners/TT_Banner_Red.prefab"),
                },
                new[]
                {
                    Entry("building_destroyed", "FX/FX_prefabs/FX_Building_Destroyed_mid.prefab"),
                    Entry("building_burning", "FX/FX_prefabs/FX_Building_burning.prefab"),
                    Entry("machine_destroyed", "FX/FX_prefabs/FX_machine_destroyed.prefab"),
                });

            EditorUtility.SetDirty(manifest);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Rebuilt Toony RTS visual manifest at {ManifestPath}");
        }

        private static ToonyRtsVisualManifest.VisualEntry Entry(string key, string relativePath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ToonyRtsVisualLibrary.AssetRoot + relativePath);
            return new ToonyRtsVisualManifest.VisualEntry(key, prefab);
        }
    }
}
