// Assets/_Game/Scripts/Base/BuildingPlacer.cs
using UnityEngine;
using MedievalRTS.Data;

namespace MedievalRTS.Base
{
    public class BuildingPlacer : MonoBehaviour
    {
        [SerializeField] private BaseBuilderManager _manager;
        [SerializeField] private Camera _cam;
        [SerializeField] private LayerMask _groundLayer;

        private BuildingData _selected;
        private GameObject _ghost;

        public void SelectBuilding(BuildingData data)
        {
            CancelPlacement();
            _selected = data;
            _ghost = Instantiate(data.prefab);
            SetGhostColor(Color.white, 0.5f);
        }

        private void Update()
        {
            if (_selected == null) return;

            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 100f, _groundLayer)) return;

            var gridPos = _manager.WorldToGrid(hit.point);
            _ghost.transform.position = _manager.GridToWorld(gridPos.x, gridPos.y);

            bool canPlace = _manager.GridSystem.CanPlace(gridPos.x, gridPos.y, _selected.gridSize);
            SetGhostColor(canPlace ? Color.green : Color.red, 0.5f);

            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                _manager.TryPlaceBuilding(_selected, gridPos.x, gridPos.y);
                CancelPlacement();
            }

            if (Input.GetMouseButtonDown(1)) CancelPlacement();
        }

        private void CancelPlacement()
        {
            if (_ghost != null) Destroy(_ghost);
            _ghost = null;
            _selected = null;
        }

        private void SetGhostColor(Color color, float alpha)
        {
            if (_ghost == null) return;
            foreach (var r in _ghost.GetComponentsInChildren<Renderer>())
                r.material.color = new Color(color.r, color.g, color.b, alpha);
        }
    }
}
