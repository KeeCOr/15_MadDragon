// Assets/_Game/Scripts/Base/BaseBuilderUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MedievalRTS.Core;
using MedievalRTS.Data;

namespace MedievalRTS.Base
{
    public class BaseBuilderUI : MonoBehaviour
    {
        [SerializeField] private BuildingPlacer placer;
        [SerializeField] private BaseBuilderManager manager;
        [SerializeField] private Button saveButton;
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private GameObject buildingButtonPrefab;
        [SerializeField] private BuildingData[] catalog;

        private void Start()
        {
            saveButton.onClick.AddListener(OnSave);
            foreach (var b in catalog)
                CreateBuildingButton(b);
        }

        private void CreateBuildingButton(BuildingData data)
        {
            var go = Instantiate(buildingButtonPrefab, buttonContainer);
            go.GetComponentInChildren<TMP_Text>().text = data.buildingName;
            go.GetComponent<Image>().sprite = data.icon;
            go.GetComponent<Button>().onClick.AddListener(() => placer.SelectBuilding(data));
        }

        private void OnSave()
        {
            manager.SaveLayout();
            GameManager.Instance.ChangeState(GameState.MainMenu);
        }
    }
}
