using System.Collections;
using UnityEngine;

namespace ClassesDirectory.WarriorClass
{
    public class MultiAttackController : MonoBehaviour
    {
        private bool _isAttacking;
        private int _currentAttackIndex;

        public bool CanAttack() => !_isAttacking;

        public IEnumerator ExecuteMultiAttack(System.Action<int, float> attackAction, int attackCount,
            float intervalBetweenAttacks)
        {
            _isAttacking = true;
            _currentAttackIndex = 0;

            for (int i = 0; i < attackCount; i++)
            {
                // Determine angle offset (-1 for left, 1 for right, alternating)
                float angleOffset = (i % 2 == 0) ? -1f : 1f;

                // Add slight spread increase for subsequent attacks
                float spreadMultiplier = 1f + (i * 0.15f);

                attackAction.Invoke(i, angleOffset * spreadMultiplier);
                _currentAttackIndex++;

                if (i < attackCount - 1)
                {
                    yield return new WaitForSeconds(intervalBetweenAttacks);
                }
            }

            _isAttacking = false;
        }
    }
}