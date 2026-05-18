using MedievalRTS.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace MedievalRTS.UI
{
    public class CampaignHubScreen
    {
        private readonly GameObject _root;
        private readonly Font _font;
        private readonly System.Action _openAttackPrep;
        private readonly System.Action _openBase;
        private Text _summary;

        public GameObject Root => _root;

        public CampaignHubScreen(GameObject canvas, Font font, System.Action openAttackPrep, System.Action openBase)
        {
            _font = font;
            _openAttackPrep = openAttackPrep;
            _openBase = openBase;
            _root = MobileUiFactory.CreatePanel(canvas, "CampaignHubScreen", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, MobileHudTheme.PanelStrong);
            Build();
        }

        public void SetVisible(bool visible)
        {
            _root.SetActive(visible);
        }

        public void Refresh(ResourceWallet owned, ResourceWallet stored, RaidForecast forecast)
        {
            int storedGold = stored.Get(ResourceType.Gold);
            int ownedGold = owned.Get(ResourceType.Gold);
            int storedLoss = forecast?.StoredLoss.Get(ResourceType.Gold) ?? 0;
            int ownedLoss = forecast?.OwnedLoss.Get(ResourceType.Gold) ?? 0;

            _summary.text =
                $"Gold {ownedGold:n0}   Honor {owned.Get(ResourceType.Honor):n0}   Stars {owned.Get(ResourceType.Stars):n0}\n" +
                $"Stored Gold {storedGold:n0}\n" +
                $"Raid Risk - stored {storedLoss:n0}G / wallet {ownedLoss:n0}G";
        }

        private void Build()
        {
            _summary = MobileUiFactory.CreateLabel(_root, "Summary", _font, "", 22, Color.white, TextAnchor.MiddleCenter);
            MobileUiFactory.SetRect(_summary.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(720f, 190f));

            var attack = MobileUiFactory.CreateButton(_root, "Attack", _font, "Attack", MobileHudTheme.PrimaryButton, _openAttackPrep);
            MobileUiFactory.SetRect(attack.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-110f, 80f), MobileHudTheme.LargeButtonSize);

            var baseButton = MobileUiFactory.CreateButton(_root, "Base", _font, "Base", MobileHudTheme.SecondaryButton, _openBase);
            MobileUiFactory.SetRect(baseButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(110f, 80f), MobileHudTheme.LargeButtonSize);
        }
    }
}
