using UnityEngine;

namespace KeyboardWarrior.Validation
{
    public sealed class ValidationHealthPickup : MonoBehaviour
    {
        private ValidationWorld world;
        private bool collected;

        internal void Initialize(ValidationWorld owner) { world = owner; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || world == null || !world.State.IsCombatActive || other.attachedRigidbody != world.PlayerBody) return;
            collected = true;
            world.CollectHealth();
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
