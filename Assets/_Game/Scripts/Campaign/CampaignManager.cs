// Assets/_Game/Scripts/Campaign/CampaignManager.cs
using System.Linq;
using UnityEngine;
using MedievalRTS.Core;
using MedievalRTS.Data;
using MedievalRTS.Progression;

namespace MedievalRTS.Campaign
{
    public class CampaignManager : MonoBehaviour
    {
        public static CampaignManager Instance { get; private set; }

        [SerializeField] private StageData[] stages;

        public StageData SelectedStage { get; private set; }
        public StageData[] Stages => stages;

        private SaveData _saveData;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _saveData = SaveSystem.Load();
            EventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);
        }

        public void SelectAndStartStage(StageData stage)
        {
            SelectedStage = stage;
            GameManager.Instance.ChangeState(GameState.Battle);
        }

        public int GetStageStars(int stageIndex) =>
            _saveData.StageStars.TryGetValue(stageIndex, out int s) ? s : 0;

        public bool IsStageUnlocked(StageData stage)
        {
            if (stage.stageIndex == 0) return true;
            int totalStars = _saveData.StageStars.Values.Sum();
            return totalStars >= stage.unlockRequirementStars;
        }

        private void OnBattleEnded(BattleEndedEvent evt)
        {
            if (SelectedStage == null || !evt.Victory) return;

            int current = GetStageStars(SelectedStage.stageIndex);
            if (evt.Stars > current)
            {
                _saveData.StageStars[SelectedStage.stageIndex] = evt.Stars;
                SaveSystem.Save(_saveData);
            }
        }
    }
}
