// Assets/_Game/Scripts/Battle/BattleManager.cs
using UnityEngine;
using MedievalRTS.Core;
using MedievalRTS.Data;
using MedievalRTS.Campaign;

namespace MedievalRTS.Battle
{
    public class BattleManager : MonoBehaviour
    {
        private StageData _stage;
        private float _elapsed;
        private int _playerUnitsLost;
        private bool _active;
        private AIWaveSpawner _waveSpawner;

        private void Start()
        {
            _waveSpawner = FindObjectOfType<AIWaveSpawner>();
            EventBus.Subscribe<UnitDiedEvent>(OnUnitDied);
            EventBus.Subscribe<BuildingDestroyedEvent>(OnBuildingDestroyed);

            _stage = CampaignManager.Instance.SelectedStage;
            if (_stage != null) StartBattle(_stage);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<UnitDiedEvent>(OnUnitDied);
            EventBus.Unsubscribe<BuildingDestroyedEvent>(OnBuildingDestroyed);
        }

        private void StartBattle(StageData stage)
        {
            _elapsed = 0f;
            _playerUnitsLost = 0;
            _active = true;
            _waveSpawner.BeginWaves(stage);
            SpawnEnemyBase(stage);
        }

        private void SpawnEnemyBase(StageData stage)
        {
            foreach (var placement in stage.enemyBase)
            {
                var pos = new Vector3(placement.gridPosition.x, 0, placement.gridPosition.y);
                var go = Instantiate(placement.data.prefab, pos, Quaternion.identity);
                go.GetComponent<Buildings.Building>().Initialize(placement.data, isPlayerBuilding: false);
            }
        }

        private void Update()
        {
            if (!_active) return;
            _elapsed += Time.deltaTime;
            if (_elapsed >= _stage.battleDuration) EndBattle(false);
        }

        private void OnUnitDied(UnitDiedEvent evt)
        {
            if (evt.Unit.IsPlayerUnit) _playerUnitsLost++;
        }

        private void OnBuildingDestroyed(BuildingDestroyedEvent evt)
        {
            if (!evt.Building.IsPlayerBuilding &&
                evt.Building.Data.buildingType == BuildingType.Castle)
                EndBattle(true);

            if (evt.Building.IsPlayerBuilding &&
                evt.Building.Data.buildingType == BuildingType.Castle)
                EndBattle(false);
        }

        private void EndBattle(bool victory)
        {
            if (!_active) return;
            _active = false;
            var stats = new BattleStats
            {
                Victory = victory,
                UnitsLost = _playerUnitsLost,
                TimeElapsed = _elapsed
            };
            int stars = StarRatingSystem.Calculate(stats, _stage);
            EventBus.Publish(new BattleEndedEvent(victory, stars, stats));
            GameManager.Instance.ChangeState(GameState.Result);
        }
    }
}
