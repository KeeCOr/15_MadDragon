using UnityEngine;

namespace MedievalRTS.Visuals
{
    public static class MobileVisualStyle
    {
        public static readonly Color FriendlyBlue = new(0.20f, 0.45f, 0.95f);
        public static readonly Color EnemyRed = new(0.85f, 0.16f, 0.12f);
        public static readonly Color GrassBase = new(0.30f, 0.58f, 0.24f);
        public static readonly Color StoneWarm = new(0.55f, 0.52f, 0.47f);
        public static readonly Color GoldAccent = new(1.00f, 0.72f, 0.20f);
        public static readonly Color MageViolet = new(0.55f, 0.20f, 0.85f);
        public static readonly Color Sky = new(0.50f, 0.70f, 0.95f);

        public static void ApplyCamera(Camera camera, bool defenseSide)
        {
            if (camera == null) return;
            camera.orthographic = false;
            camera.fieldOfView = 42f;
            camera.transform.SetPositionAndRotation(
                defenseSide ? new Vector3(-12f, 30f, -24f) : new Vector3(4f, 32f, -28f),
                Quaternion.Euler(52f, 0f, 0f));
            camera.backgroundColor = Sky;
            camera.clearFlags = CameraClearFlags.SolidColor;
        }

        public static void ApplyLight(Light light)
        {
            if (light == null) return;
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1f, 0.92f, 0.82f);
            light.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        }
    }
}
