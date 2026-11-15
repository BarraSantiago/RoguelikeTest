using System;
using UnityEngine;

namespace WeaponsDirectory
{
    /// <summary>
    /// Simple generated shapes so weapons work without any art. Each sprite is 1 world unit wide.
    /// Assign a sprite on the WeaponDefinition (icon) or replace these later with real art.
    /// </summary>
    public static class WeaponVisuals
    {
        private const int TextureSize = 64;

        private static Sprite _circle;
        private static Sprite _aura;
        private static Sprite _blade;
        private static Sprite _square;
        private static Sprite _boomerang;

        private static int _sortingLayerId;
        private static int _baseSortingOrder;

        /// <summary>Solid anti-aliased disc.</summary>
        public static Sprite Circle => _circle ? _circle : (_circle = Create("WeaponCircle", (u, v) =>
            Edge(1f - Mathf.Sqrt(u * u + v * v))));

        /// <summary>Disc that is faint in the middle and bright at the rim.</summary>
        public static Sprite Aura => _aura ? _aura : (_aura = Create("WeaponAura", (u, v) =>
        {
            float d = Mathf.Sqrt(u * u + v * v);
            return Mathf.Lerp(0.25f, 1f, d * d * d) * Edge(1f - d);
        }));

        /// <summary>Long diamond pointing along +X.</summary>
        public static Sprite Blade => _blade ? _blade : (_blade = Create("WeaponBlade", (u, v) =>
            Edge(1f - (Mathf.Abs(u) + Mathf.Abs(v) / 0.3f))));

        /// <summary>V shape, used by the boomerang.</summary>
        public static Sprite Boomerang => _boomerang ? _boomerang : (_boomerang = Create("WeaponBoomerang", (u, v) =>
        {
            // Distance to two arms meeting at the left edge
            float arm1 = DistanceToSegment(u, v, -0.6f, 0f, 0.7f, 0.65f);
            float arm2 = DistanceToSegment(u, v, -0.6f, 0f, 0.7f, -0.65f);
            return Edge(0.2f - Mathf.Min(arm1, arm2));
        }));

        /// <summary>Plain white square, used for stretched lines.</summary>
        public static Sprite Square => _square ? _square : (_square = Create("WeaponSquare", (u, v) => 1f));

        /// <summary>Draw weapons just above the player so they're never hidden behind the background.</summary>
        public static void Configure(SpriteRenderer playerRenderer)
        {
            if (!playerRenderer) return;
            _sortingLayerId = playerRenderer.sortingLayerID;
            _baseSortingOrder = playerRenderer.sortingOrder;
        }

        public static SpriteRenderer CreateRenderer(string objectName, Transform parent, Sprite sprite, Color color,
            int sortingOffset = 1)
        {
            GameObject go = new GameObject(objectName);
            go.transform.SetParent(parent, false);

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingLayerID = _sortingLayerId;
            renderer.sortingOrder = _baseSortingOrder + sortingOffset;
            return renderer;
        }

        /// <summary>Positions a renderer using <see cref="Square"/> as a line between two points.</summary>
        public static void StretchBetween(Transform line, Vector2 from, Vector2 to, float width)
        {
            Vector2 delta = to - from;
            line.position = (from + to) * 0.5f;
            line.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            line.localScale = new Vector3(delta.magnitude, width, 1f);
        }

        private static float Edge(float signedDistance)
        {
            // Roughly one texel of anti-aliasing
            return Mathf.Clamp01(signedDistance * TextureSize * 0.5f);
        }

        private static float DistanceToSegment(float px, float py, float ax, float ay, float bx, float by)
        {
            Vector2 p = new Vector2(px, py);
            Vector2 a = new Vector2(ax, ay);
            Vector2 ab = new Vector2(bx, by) - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }

        /// <param name="alpha">Maps u, v in [-1, 1] to opacity.</param>
        private static Sprite Create(string spriteName, Func<float, float, float> alpha)
        {
            Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                name = spriteName,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color32[] pixels = new Color32[TextureSize * TextureSize];
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float u = (x + 0.5f) / TextureSize * 2f - 1f;
                    float v = (y + 0.5f) / TextureSize * 2f - 1f;
                    byte a = (byte)(Mathf.Clamp01(alpha(u, v)) * 255f);
                    pixels[y * TextureSize + x] = new Color32(255, 255, 255, a);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f),
                TextureSize);
            sprite.name = spriteName;
            return sprite;
        }
    }
}
