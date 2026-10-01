using UnityEngine;

namespace MedievalRTS.Visuals
{
    public static class ToonyRtsVisualApplier
    {
        public static GameObject Attach(GameObject root, GameObject visualPrefab, Vector3 localPosition, Vector3 localScale, Quaternion localRotation)
        {
            if (root == null || visualPrefab == null) return null;

            var visual = Object.Instantiate(visualPrefab, root.transform);
            visual.name = "ToonyVisual";
            visual.transform.localPosition = Divide(localPosition, root.transform.localScale);
            visual.transform.localRotation = localRotation;
            visual.transform.localScale = Divide(localScale, root.transform.lossyScale);

            StripGameplayComponents(visual);
            return visual;
        }

        public static void FitFootprintToWorldSize(GameObject visual, Vector2 targetFootprint, float minScaleMultiplier = 0.35f, float maxScaleMultiplier = 4f)
        {
            if (visual == null || targetFootprint.x <= 0f || targetFootprint.y <= 0f) return;
            if (!TryGetRendererBounds(visual, out var bounds)) return;

            float sizeX = Mathf.Max(bounds.size.x, 0.001f);
            float sizeZ = Mathf.Max(bounds.size.z, 0.001f);
            float multiplier = Mathf.Min(targetFootprint.x / sizeX, targetFootprint.y / sizeZ);
            multiplier = Mathf.Clamp(multiplier, minScaleMultiplier, maxScaleMultiplier);
            visual.transform.localScale *= multiplier;
        }

        public static void AlignBottomToWorldY(GameObject visual, float worldY)
        {
            if (visual == null) return;
            if (!TryGetRendererBounds(visual, out var bounds)) return;

            var position = visual.transform.position;
            position.y += worldY - bounds.min.y;
            visual.transform.position = position;
        }

        public static void HideRootRenderers(GameObject root)
        {
            if (root == null) return;
            var toonyRoot = root.transform.Find("ToonyVisual");

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (toonyRoot != null && renderer.transform.IsChildOf(toonyRoot))
                    continue;
                renderer.enabled = false;
            }
        }

        private static bool TryGetRendererBounds(GameObject root, out Bounds bounds)
        {
            bounds = default;
            if (root == null) return false;

            bool hasBounds = false;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds;
        }

        private static void StripGameplayComponents(GameObject visual)
        {
            foreach (var collider in visual.GetComponentsInChildren<Collider>(true))
                DestroyComponent(collider);

            foreach (var rigidbody in visual.GetComponentsInChildren<Rigidbody>(true))
                DestroyComponent(rigidbody);
        }

        private static void DestroyComponent(Object component)
        {
            if (Application.isPlaying)
                Object.Destroy(component);
            else
                Object.DestroyImmediate(component);
        }

        private static Vector3 Divide(Vector3 value, Vector3 divisor)
        {
            return new Vector3(
                divisor.x == 0f ? value.x : value.x / divisor.x,
                divisor.y == 0f ? value.y : value.y / divisor.y,
                divisor.z == 0f ? value.z : value.z / divisor.z);
        }
    }
}
