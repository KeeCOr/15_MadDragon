using UnityEngine;

namespace MedievalRTS.UI
{
    public static class MobileHudTheme
    {
        public static readonly Color Panel = new(0.06f, 0.08f, 0.10f, 0.88f);
        public static readonly Color PanelStrong = new(0.04f, 0.05f, 0.07f, 0.94f);
        public static readonly Color Gold = new(1.00f, 0.78f, 0.24f, 1f);
        public static readonly Color Honor = new(0.55f, 0.85f, 1.00f, 1f);
        public static readonly Color Danger = new(0.95f, 0.25f, 0.20f, 1f);
        public static readonly Color Good = new(0.30f, 0.85f, 0.45f, 1f);
        public static readonly Color PrimaryButton = new(0.12f, 0.36f, 0.62f, 1f);
        public static readonly Color SecondaryButton = new(0.20f, 0.22f, 0.25f, 1f);

        public const int TopBarFont = 18;
        public const int BodyFont = 15;
        public const int ButtonFont = 15;
        public const float BottomSheetHeight = 320f;
        public static readonly Vector2 LargeButtonSize = new(176f, 64f);
        public static readonly Vector2 QuickButtonSize = new(132f, 58f);
    }
}
