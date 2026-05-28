using System.Collections.Generic;
using UnityEngine;

namespace MedievalRTS.Visuals
{
    public static class GeneratedArtLibrary
    {
        private const string Root = "MadDragonArt/";
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite LoadSprite(string key, float pixelsPerUnit = 160f)
        {
            if (string.IsNullOrEmpty(key)) return null;

            string cacheKey = $"{key}:{pixelsPerUnit}";
            if (Cache.TryGetValue(cacheKey, out var cached) && cached != null)
                return cached;

            var texture = Resources.Load<Texture2D>(Root + key);
            if (texture == null) return null;

            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit);
            Cache[cacheKey] = sprite;
            return sprite;
        }

        public static bool HasSprite(string key)
        {
            return !string.IsNullOrEmpty(key) && Resources.Load<Texture2D>(Root + key) != null;
        }
    }
}
