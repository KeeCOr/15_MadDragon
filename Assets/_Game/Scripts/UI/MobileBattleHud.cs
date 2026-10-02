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
        private Text _timeStatus;
        private Text _objectiveStatus;
        private Text _rewardStatus;
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
            var status = BattlePriorityModel.BuildHudStatus(secondsRemaining, targetHp, earnedGold, earnedHonor);
            _timeStatus.text = status.TimeText;
            _objectiveStatus.text = status.ObjectiveText;
            _rewardStatus.text = status.RewardText;
            _timeStatus.color = status.TimeTone switch
            {
                BattleTimeTone.Critical => MobileHudTheme.Danger,
                BattleTimeTone.Warning => MobileHudTheme.Gold,
                _ => Color.white
            };
        }

        private void Build()
        {
            var top = MobileUiFactory.CreatePanel(_root, "TopStatus", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(8f, -58f), new Vector2(-8f, -8f), MobileHudTheme.PanelStrong);
            _timeStatus = MobileUiFactory.CreateLabel(top, "Time", _font, "TIME  --", MobileHudTheme.TopBarFont, Color.white, TextAnchor.MiddleLeft);
            _objectiveStatus = MobileUiFactory.CreateLabel(top, "Objective", _font, "OBJECTIVE  BASE HP  --", MobileHudTheme.TopBarFont, Color.white, TextAnchor.MiddleCenter);
            _rewardStatus = MobileUiFactory.CreateLabel(top, "Reward", _font, "REWARD  +0G  +0 HONOR", MobileHudTheme.TopBarFont, MobileHudTheme.Gold, TextAnchor.MiddleRight);
            SetTopZone(_timeStatus.rectTransform, 0f, 0.25f);
            SetTopZone(_objectiveStatus.rectTransform, 0.25f, 0.67f);
            SetTopZone(_rewardStatus.rectTransform, 0.67f, 1f);

            var bottom = MobileUiFactory.CreatePanel(_root, "QuickBar", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(8f, 8f), new Vector2(-8f, 96f), MobileHudTheme.Panel);
            CreateCommandButton(bottom, "Rally", "Rally", CommandKind.Rally, 0);
            CreateCommandButton(bottom, "Attack", "Attack", CommandKind.Attack, 1);
            CreateCommandButton(bottom, "Hold", "Hold", CommandKind.Hold, 2);
            CreateCommandButton(bottom, "Spells", "Spells", CommandKind.Spells, 3);
            _quickStatus = MobileUiFactory.CreateLabel(bottom, "Status", _font, "Ready", MobileHudTheme.BodyFont, MobileHudTheme.Honor, TextAnchor.MiddleCenter);
            MobileUiFactory.SetRect(_quickStatus.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(-20f, 24f));
        }

        private static void SetTopZone(RectTransform rect, float minX, float maxX)
        {
            rect.anchorMin = new Vector2(minX, 0f);
            rect.anchorMax = new Vector2(maxX, 1f);
            rect.offsetMin = new Vector2(10f, 4f);
            rect.offsetMax = new Vector2(-10f, -4f);
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
