using UnityEngine;
using UnityEngine.UI;

namespace MedievalRTS.UI
{
    public class MobileBattleHud
    {
        private readonly GameObject _root;
        private readonly Font _font;
        private Text _topStatus;
        private Text _quickStatus;

        public GameObject Root => _root;

        public MobileBattleHud(GameObject canvas, Font font)
        {
            _font = font;
            _root = new GameObject("MobileBattleHud");
            _root.transform.SetParent(canvas.transform, false);
            var rt = _root.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Build();
        }

        public void SetVisible(bool visible)
        {
            _root.SetActive(visible);
        }

        public void Refresh(int secondsRemaining, int targetHp, int earnedGold, int earnedHonor)
        {
            _topStatus.text = $"{secondsRemaining}s     Target HP {targetHp}     +{earnedGold}G +{earnedHonor} Honor";
            _quickStatus.text = "Rally   Attack   Hold   Spells";
        }

        private void Build()
        {
            var top = MobileUiFactory.CreatePanel(_root, "TopStatus", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(8f, -58f), new Vector2(-8f, -8f), MobileHudTheme.Panel);
            _topStatus = MobileUiFactory.CreateLabel(top, "Label", _font, "", MobileHudTheme.TopBarFont, Color.white, TextAnchor.MiddleCenter);

            var bottom = MobileUiFactory.CreatePanel(_root, "QuickBar", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(8f, 8f), new Vector2(-8f, 90f), MobileHudTheme.Panel);
            _quickStatus = MobileUiFactory.CreateLabel(bottom, "Label", _font, "", MobileHudTheme.ButtonFont, Color.white, TextAnchor.MiddleCenter);
        }
    }
}
