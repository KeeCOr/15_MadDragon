using UnityEngine;

namespace MedievalRTS.Visuals
{
    public static class MobileVisualStyle
    {
        public static readonly Color FriendlyBlue = new(0.10f, 0.42f, 0.95f);
        public static readonly Color EnemyRed = new(0.92f, 0.18f, 0.12f);
        public static readonly Color GrassBase = new(0.42f, 0.72f, 0.25f);
        public static readonly Color GrassDark = new(0.24f, 0.52f, 0.18f);
        public static readonly Color GrassLight = new(0.58f, 0.84f, 0.32f);
        public static readonly Color GrassPatch = new(0.34f, 0.64f, 0.21f);
        public static readonly Color ForestDeep = new(0.12f, 0.36f, 0.16f);
        public static readonly Color FlowerPink = new(1.00f, 0.42f, 0.58f);
        public static readonly Color FlowerYellow = new(1.00f, 0.86f, 0.18f);
        public static readonly Color WaterBlue = new(0.26f, 0.68f, 0.92f);
        public static readonly Color DirtWarm = new(0.57f, 0.42f, 0.24f);
        public static readonly Color PathStone = new(0.66f, 0.60f, 0.47f);
        public static readonly Color StoneWarm = new(0.68f, 0.65f, 0.58f);
        public static readonly Color StoneShadow = new(0.42f, 0.42f, 0.39f);
        public static readonly Color WoodWarm = new(0.55f, 0.34f, 0.16f);
        public static readonly Color GoldAccent = new(1.00f, 0.78f, 0.20f);
        public static readonly Color MageViolet = new(0.66f, 0.20f, 0.95f);
        public static readonly Color TorchOrange = new(1.00f, 0.42f, 0.08f);
        public static readonly Color Sky = new(0.63f, 0.82f, 1.00f);

        public static void ApplyCamera(Camera camera, bool defenseSide)
        {
            if (camera == null) return;
            camera.orthographic = false;
            camera.fieldOfView = 38f;
            camera.transform.SetPositionAndRotation(
                defenseSide ? new Vector3(-11f, 28f, -23f) : new Vector3(4f, 29f, -25f),
                Quaternion.Euler(54f, 0f, 0f));
            camera.backgroundColor = Sky;
            camera.clearFlags = CameraClearFlags.SolidColor;
        }

        public static void ApplyLight(Light light)
        {
            if (light == null) return;
            light.type = LightType.Directional;
            light.intensity = 1.35f;
            light.color = new Color(1f, 0.96f, 0.86f);
            light.transform.rotation = Quaternion.Euler(50f, -38f, 0f);
        }
    }
}
