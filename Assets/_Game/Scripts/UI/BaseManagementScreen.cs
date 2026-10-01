using MedievalRTS.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace MedievalRTS.UI
{
    public class BaseManagementScreen
    {
        private readonly GameObject _root;
        private readonly Font _font;
        private readonly System.Action _collectAll;
        private readonly System.Action _backToHub;
        private Text _status;

        public GameObject Root => _root;

        public BaseManagementScreen(GameObject canvas, Font font, System.Action collectAll, System.Action backToHub)
        {
            _font = font;
            _collectAll = collectAll;
            _backToHub = backToHub;
            _root = MobileUiFactory.CreatePanel(canvas, "BaseManagementScreen", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, MobileHudTheme.PanelStrong);
            Build();
        }

        public void SetVisible(bool visible)
        {
            _root.SetActive(visible);
        }

        public void Refresh(ResourceWallet owned, ResourceWallet stored, RaidForecast forecast, int headquartersLevel, int storageCapacity)
        {
            int storedGold = stored.Get(ResourceType.Gold);
            int storedLoss = forecast?.StoredLoss.Get(ResourceType.Gold) ?? 0;
            int ownedLoss = forecast?.OwnedLoss.Get(ResourceType.Gold) ?? 0;

            _status.text =
                $"Headquarters Lv.{headquartersLevel}\n" +
                $"Wallet Gold {owned.Get(ResourceType.Gold):n0}\n" +
                $"Stored Gold {storedGold:n0} / {storageCapacity:n0}\n" +
                $"Expected Raid Loss {storedLoss + ownedLoss:n0}G";
        }

        private void Build()
        {
            _status = MobileUiFactory.CreateLabel(_root, "Status", _font, "", 22, Color.white, TextAnchor.MiddleCenter);
            MobileUiFactory.SetRect(_status.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(720f, 220f));

            var collect = MobileUiFactory.CreateButton(_root, "CollectAll", _font, "Collect", MobileHudTheme.Good, _collectAll);
            MobileUiFactory.SetRect(collect.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-110f, 80f), MobileHudTheme.LargeButtonSize);

            var back = MobileUiFactory.CreateButton(_root, "Back", _font, "Hub", MobileHudTheme.SecondaryButton, _backToHub);
            MobileUiFactory.SetRect(back.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(110f, 80f), MobileHudTheme.LargeButtonSize);
        }
    }
}
