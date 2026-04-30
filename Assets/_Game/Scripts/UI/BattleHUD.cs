// Assets/_Game/Scripts/UI/BattleHUD.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MedievalRTS.Core;
using MedievalRTS.Data;
using MedievalRTS.Battle;
using MedievalRTS.Campaign;

namespace MedievalRTS.UI
{
    public class BattleHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private Transform unitButtonContainer;
        [SerializeField] private GameObject unitButtonPrefab;
        [SerializeField] private UnitData[] availableUnits;

        private UnitSpawner _spawner;
        private float _elapsed;
        private float _duration;

        private void Start()
        {
            _spawner = FindObjectOfType<UnitSpawner>();
            _duration = CampaignManager.Instance?.SelectedStage?.battleDuration ?? 180f;

            EventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);

            foreach (var unit in availableUnits)
                CreateUnitButton(unit);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float remaining = Mathf.Max(0f, _duration - _elapsed);
            timerText.text = $"{Mathf.CeilToInt(remaining)}s";
        }

        private void OnGoldChanged(GoldChangedEvent evt)
        {
            goldText.text = evt.Amount.ToString();
        }

        private void CreateUnitButton(UnitData data)
        {
            var go = Instantiate(unitButtonPrefab, unitButtonContainer);
            go.GetComponentInChildren<TMP_Text>().text = $"{data.unitName}\n{data.goldCost}G";
            go.GetComponent<Image>().sprite = data.icon;
            go.GetComponent<Button>().onClick.AddListener(() => _spawner.TrySpawnPlayerUnit(data));
        }
    }
}
