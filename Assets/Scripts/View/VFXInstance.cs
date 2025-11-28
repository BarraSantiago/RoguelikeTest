using System.Collections;
using UnityEngine;

namespace View
{
    public class VFXInstance : MonoBehaviour
    {
        public SpriteRenderer Renderer => _renderer;
        private SpriteRenderer _renderer;
        private float _duration;
        private Color _baseColor;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(Sprite sprite, Color color, float duration)
        {
            _renderer.sprite = sprite;
            _baseColor = color;
            _duration = duration;
            _renderer.color = color;
            StartCoroutine(FadeOutRoutine());
        }

        private IEnumerator FadeOutRoutine()
        {
            float elapsed = 0f;
            Vector3 initialScale = transform.localScale;

            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _duration;

                // Fade out
                float alpha = Mathf.Lerp(_baseColor.a, 0f, progress);
                _renderer.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);

                // Scale up slightly
                transform.localScale = initialScale * (1f + progress * 0.2f);

                yield return null;
            }

            VFXPool.Instance.Return(this);
        }
    }
}