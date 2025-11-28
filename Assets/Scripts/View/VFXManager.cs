using UnityEngine;
using Utilities;

namespace View
{
    public class VFXManager : MonoBehaviour
    {
        [SerializeField] private float vfxDuration = 0.4f;

        private Sprite _coneSprite;
        private Sprite _circleSprite;

        private void Awake()
        {
            _circleSprite = CreateCircleSprite();
        }

        private void OnEnable()
        {
            CombatEvents.OnAttackPerformed += HandleAttackVFX;
            CombatEvents.OnRadialAttackPerformed += HandleRadialVFX;
        }

        private void OnDisable()
        {
            CombatEvents.OnAttackPerformed -= HandleAttackVFX;
            CombatEvents.OnRadialAttackPerformed -= HandleRadialVFX;
        }

        private void HandleAttackVFX(Vector2 position, Vector2 direction, float range, float area, AttackType type)
        {
            CreateConeVFX(position, direction, range, area, type);
        }

        private void HandleRadialVFX(Vector2 position, float radius, AttackType type)
        {
            CreateCircleVFX(position, radius, type);
        }
        
        private void CreateConeVFX(Vector2 position, Vector2 direction, float range, float area, AttackType type)
        {
            VFXInstance vfx = VFXPool.Instance.Get();
            vfx.transform.position = position;
        
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            vfx.transform.rotation = Quaternion.Euler(0, 0, angle);
            vfx.transform.localScale = new Vector3(1f, range, 1f);
        
            vfx.Renderer.sortingOrder = -1;
            vfx.Initialize(CreateConeSprite(area), GetColorForAttackType(type), vfxDuration);
        }

        private void CreateCircleVFX(Vector2 position, float radius, AttackType type)
        {
            VFXInstance vfx = VFXPool.Instance.Get();
            vfx.transform.position = position;
            vfx.transform.rotation = Quaternion.identity;
            vfx.transform.localScale = Vector3.one * (radius * 2f);

            vfx.Renderer.sortingOrder = -1;
            vfx.Initialize(_circleSprite, GetColorForAttackType(type), vfxDuration);
        }

        
        
        private Sprite CreateConeSprite(float coneWidth = 1f)
        {
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
        
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float normalizedY = y / 64f;
                    float centerX = 32f;
                    float maxWidth = normalizedY * 32f * coneWidth;
                    float distFromCenter = Mathf.Abs(x - centerX);
        
                    float alpha = distFromCenter < maxWidth ? 0.5f * (1f - normalizedY * 0.3f) : 0f;
                    pixels[y * 64 + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
        
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0f));
        }

        private Sprite CreateCircleSprite()
        {
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];

            Vector2 center = new Vector2(32f, 32f);
            float radius = 32f;

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float normalizedDist = distance / radius;
                    float alpha = distance < radius ? 0.5f * (1f - normalizedDist * 0.5f) : 0f;
                    pixels[y * 64 + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }

        private Color GetColorForAttackType(AttackType type)
        {
            return type switch
            {
                AttackType.WarriorBasic => new Color(1f, 0.3f, 0.3f, 0.6f),
                AttackType.WarriorSpecial => new Color(1f, 0.5f, 0f, 0.6f),
                AttackType.WarriorDefense => new Color(0.3f, 0.6f, 1f, 0.6f),
                _ => Color.white
            };
        }
    }
}