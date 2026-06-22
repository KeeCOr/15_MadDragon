using System;
using UnityEngine;
using UnityEngine.UI;

namespace MedievalRTS.UI
{
    public class MobileBattleHud
    {
        public enum CommandKind
        {
            Rally,
            Attack,
            Hold,
            Spells
        }

        private readonly GameObject _root;
        private readonly Font _font;
        private Text _topStatus;
        private Text _quickStatus;
        private Action<CommandKind> _commandHandler;

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

        public void SetCommandHandler(Action<CommandKind> handler)
        {
            _commandHandler = handler;
        }

        public void SetVisible(bool visible)
        {
            _root.SetActive(visible);
        }

        public void Refresh(int secondsRemaining, int targetHp, int earnedGold, int earnedHonor)
        {
            _topStatus.text = $"{secondsRemaining}s   HP {targetHp}   +{earnedGold}G   +{earnedHonor} Honor";
        }

        private void Build()
        {
            var top = MobileUiFactory.CreatePanel(_root, "TopStatus", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(8f, -58f), new Vector2(-8f, -8f), MobileHudTheme.PanelStrong);
            _topStatus = MobileUiFactory.CreateLabel(top, "Label", _font, "", MobileHudTheme.TopBarFont, Color.white, TextAnchor.MiddleCenter);

            var bottom = MobileUiFactory.CreatePanel(_root, "QuickBar", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(8f, 8f), new Vector2(-8f, 96f), MobileHudTheme.Panel);
            CreateCommandButton(bottom, "Rally", "Rally", CommandKind.Rally, 0);
            CreateCommandButton(bottom, "Attack", "Attack", CommandKind.Attack, 1);
            CreateCommandButton(bottom, "Hold", "Hold", CommandKind.Hold, 2);
            CreateCommandButton(bottom, "Spells", "Spells", CommandKind.Spells, 3);
            _quickStatus = MobileUiFactory.CreateLabel(bottom, "Status", _font, "Ready", MobileHudTheme.BodyFont, MobileHudTheme.Honor, TextAnchor.MiddleCenter);
            MobileUiFactory.SetRect(_quickStatus.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(-20f, 24f));
        }

        private void CreateCommandButton(GameObject parent, string name, string label, CommandKind command, int index)
        {
            var button = MobileUiFactory.CreateButton(parent, name, _font, label, index == 0 ? MobileHudTheme.PrimaryButton : MobileHudTheme.SecondaryButton, () => Activate(command));
            var rt = button.GetComponent<RectTransform>();
            const float width = 132f;
            const float height = 58f;
            const float gap = 10f;
            var startX = -((width * 4f) + (gap * 3f)) * 0.5f + (width * 0.5f);
            MobileUiFactory.SetRect(rt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(startX + index * (width + gap), 14f), new Vector2(width, height));
        }

        private void Activate(CommandKind command)
        {
            _quickStatus.text = command switch
            {
                CommandKind.Rally => "Rally point ready",
                CommandKind.Attack => "Attack command armed",
                CommandKind.Hold => "Hold position",
                CommandKind.Spells => "Spell bar focused",
                _ => "Ready"
            };
            _commandHandler?.Invoke(command);
        }
    }
}
