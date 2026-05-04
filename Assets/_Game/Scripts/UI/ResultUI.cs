// Assets/_Game/Scripts/UI/ResultUI.cs
using UnityEngine;
using UnityEngine.UI;
using MedievalRTS.Core;

namespace MedievalRTS.UI
{
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private GameObject[] starObjects;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button retryButton;

        private void Start()
        {
            EventBus.Subscribe<BattleEndedEvent>(ShowResult);
            continueButton.onClick.AddListener(() =>
                GameManager.Instance?.ChangeState(GameState.MainMenu));
            retryButton.onClick.AddListener(() =>
            {
                var stage = Campaign.CampaignManager.Instance?.SelectedStage;
                if (stage != null)
                    Campaign.CampaignManager.Instance?.SelectAndStartStage(stage);
            });
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<BattleEndedEvent>(ShowResult);
        }

        private void ShowResult(BattleEndedEvent evt)
        {
            titleText.text = evt.Victory ? "Victory!" : "Defeat";
            for (int i = 0; i < starObjects.Length; i++)
                starObjects[i].SetActive(i < evt.Stars);
            retryButton.gameObject.SetActive(!evt.Victory);
        }
    }
}
