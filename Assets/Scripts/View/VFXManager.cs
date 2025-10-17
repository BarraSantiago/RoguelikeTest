// Create VFXManager.cs

using UnityEngine;
using Utilities;

namespace View
{
    public class VFXManager : MonoBehaviour
    {
        [SerializeField] private GameObject warriorBasicVFX;
        [SerializeField] private GameObject warriorSpecialVFX;
        [SerializeField] private GameObject warriorDefenseVFX;

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
            GameObject vfxPrefab = GetVFXPrefab(type);
            if (!vfxPrefab) return;

            GameObject vfx = Instantiate(vfxPrefab, position, Quaternion.LookRotation(Vector3.forward, direction));
            vfx.transform.localScale = new Vector3(area, range, 1f);
            Destroy(vfx, 2f);
        }

        private void HandleRadialVFX(Vector2 position, float radius, AttackType type)
        {
            GameObject vfxPrefab = GetVFXPrefab(type);
            if (!vfxPrefab) return;

            GameObject vfx = Instantiate(vfxPrefab, position, Quaternion.identity);
            vfx.transform.localScale = Vector3.one * radius;
            Destroy(vfx, 2f);
        }

        private GameObject GetVFXPrefab(AttackType type)
        {
            return type switch
            {
                AttackType.WarriorBasic => warriorBasicVFX,
                AttackType.WarriorSpecial => warriorSpecialVFX,
                AttackType.WarriorDefense => warriorDefenseVFX,
                _ => null
            };
        }
    }
}