using System.Collections;
using TMPro;
using UnityEngine;

namespace View.UI.Combat
{
    public class DamagePopup : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Animation Settings")]
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float fadeSpeed = 1.5f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
        
        private Transform _transform;
        private Vector3 _moveDirection;
        private float _timer;
        private DamagePopupPool _pool;

        private void Awake()
        {
            _transform = transform;
            if (!damageText) damageText = GetComponentInChildren<TextMeshProUGUI>();
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Initialize(DamagePopupPool pool, Vector3 position, float damage, bool isCritical)
        {
            _pool = pool;
            _transform.position = position;
            _timer = 0f;
            
            // Random horizontal spread
            _moveDirection = new Vector3(Random.Range(-0.5f, 0.5f), 1f, 0f).normalized;
            
            // Set damage text
            damageText.text = Mathf.RoundToInt(damage).ToString();
            
            // Set color and size based on damage type
            if (isCritical)
            {
                damageText.color = new Color(1f, 0.5f, 0f); // Orange for crits
                damageText.fontSize = 24f;
                damageText.fontStyle = FontStyles.Bold;
            }
            else
            {
                damageText.color = Color.white;
                damageText.fontSize = 18f;
                damageText.fontStyle = FontStyles.Normal;
            }
            
            canvasGroup.alpha = 1f;
            _transform.localScale = Vector3.zero;
            
            StartCoroutine(AnimatePopup());
        }

        private IEnumerator AnimatePopup()
        {
            while (_timer < lifetime)
            {
                _timer += Time.deltaTime;
                float progress = _timer / lifetime;
                
                // Move upward with slight curve
                _transform.position += _moveDirection * (moveSpeed * Time.deltaTime);
                
                // Scale animation
                float scale = scaleCurve.Evaluate(Mathf.Min(progress * 2f, 1f));
                _transform.localScale = Vector3.one * scale;
                
                // Fade out in the last portion
                if (progress > 0.6f)
                {
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, (progress - 0.6f) / 0.4f);
                }
                
                yield return null;
            }
            
            _pool.ReturnToPool(this);
        }
    }
}