using UnityEngine;
using UnityEngine.UI;

namespace MedievalRTS.UI
{
    public class AttackPrepScreen
    {
        private readonly GameObject _root;
        private readonly Font _font;
        private readonly System.Action _startAttack;
        private readonly System.Action _editArmy;
        private readonly System.Action _openBase;
        private readonly System.Action _backToHub;
        private Text _slots;

        public GameObject Root => _root;

        public AttackPrepScreen(GameObject canvas, Font font, System.Action startAttack, System.Action editArmy, System.Action openBase, System.Action backToHub)
        {
            _font = font;
            _startAttack = startAttack;
            _editArmy = editArmy;
            _openBase = openBase;
            _backToHub = backToHub;
            _root = MobileUiFactory.CreatePanel(canvas, "AttackPrepScreen", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, MobileHudTheme.PanelStrong);
            Build();
        }

        public void SetVisible(bool visible)
        {
            _root.SetActive(visible);
        }

        public void Refresh(string squadSummary, string spellSummary, string intelSummary)
        {
            _slots.text = $"Squad\n{squadSummary}\n\nSpells\n{spellSummary}\n\nIntel\n{intelSummary}";
        }

        private void Build()
        {
            _slots = MobileUiFactory.CreateLabel(_root, "Slots", _font, "", 20, Color.white, TextAnchor.MiddleCenter);
            MobileUiFactory.SetRect(_slots.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 105f), new Vector2(720f, 300f));

            var start = MobileUiFactory.CreateButton(_root, "StartAttack", _font, "Start", MobileHudTheme.PrimaryButton, _startAttack);
            MobileUiFactory.SetRect(start.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 80f), MobileHudTheme.LargeButtonSize);

            var army = MobileUiFactory.CreateButton(_root, "Army", _font, "Army", MobileHudTheme.SecondaryButton, _editArmy);
            MobileUiFactory.SetRect(army.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-220f, 80f), MobileHudTheme.QuickButtonSize);

            var baseButton = MobileUiFactory.CreateButton(_root, "Base", _font, "Base", MobileHudTheme.SecondaryButton, _openBase);
            MobileUiFactory.SetRect(baseButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(220f, 80f), MobileHudTheme.QuickButtonSize);

            var hub = MobileUiFactory.CreateButton(_root, "Hub", _font, "Hub", MobileHudTheme.SecondaryButton, _backToHub);
            MobileUiFactory.SetRect(hub.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -16f), MobileHudTheme.QuickButtonSize);
        }
    }
}
