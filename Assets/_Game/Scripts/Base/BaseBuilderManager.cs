// Assets/_Game/Scripts/Base/BaseBuilderManager.cs
using System.Collections.Generic;
using UnityEngine;
using MedievalRTS.Grid;
using MedievalRTS.Data;
using MedievalRTS.Buildings;
using MedievalRTS.Progression;

namespace MedievalRTS.Base
{
    public class BaseBuilderManager : MonoBehaviour
    {
        [SerializeField] private int gridWidth = 20;
        [SerializeField] private int gridHeight = 20;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 gridOrigin;
        [SerializeField] private BuildingData[] buildingCatalog;

        public GridSystem GridSystem { get; private set; }

        private BaseLayoutData _layout;
        private SaveData _saveData;
        private Dictionary<string, BuildingData> _catalog;

        private void Awake()
        {
            GridSystem = new GridSystem(gridWidth, gridHeight);
            _layout = new BaseLayoutData();
            _saveData = SaveSystem.Load();

            _catalog = new Dictionary<string, BuildingData>();
            foreach (var b in buildingCatalog)
                _catalog[b.name] = b;

            if (_saveData.PlayerBase != null)
                RestoreLayout(_saveData.PlayerBase);
        }

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize),
                Mathf.FloorToInt((worldPos.z - gridOrigin.z) / cellSize));
        }

        public Vector3 GridToWorld(int x, int y)
        {
            return gridOrigin + new Vector3(
                x * cellSize + cellSize * 0.5f, 0f,
                y * cellSize + cellSize * 0.5f);
        }

        public bool TryPlaceBuilding(BuildingData data, int x, int y)
        {
            if (!GridSystem.CanPlace(x, y, data.gridSize)) return false;

            GridSystem.Place(x, y, data.gridSize);
            var go = Instantiate(data.prefab, GridToWorld(x, y), Quaternion.identity);
            go.GetComponent<Building>().Initialize(data, isPlayerBuilding: true);

            _layout.Buildings.Add(new PlacedBuilding
            {
                BuildingName = data.name,
                GridX = x,
                GridY = y
            });
            return true;
        }

        public void SaveLayout()
        {
            _saveData.PlayerBase = _layout;
            SaveSystem.Save(_saveData);
        }

        private void RestoreLayout(BaseLayoutData saved)
        {
            foreach (var p in saved.Buildings)
            {
                if (_catalog.TryGetValue(p.BuildingName, out var data))
                    TryPlaceBuilding(data, p.GridX, p.GridY);
            }
        }
    }
}
