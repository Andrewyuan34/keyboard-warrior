using UnityEngine;

namespace KeyboardWarrior.Validation
{
    public sealed class ValidationEnemy : MonoBehaviour
    {
        private ValidationWorld world;
        private SpriteRenderer appearance;
        private SpriteRenderer warning;
        private Collider2D hitbox;
        private Color normalColor;
        private Vector3 baseScale;
        private float attackTimer;
        private float cooldown = .8f;
        private float stun;
        private int attackId;
        private bool windingUp;
        private bool dead;
        public bool IsBoss { get; private set; }
        public bool IsAlive => !dead;
        public bool IsWindingUp => windingUp;
        public float TimeUntilImpact => windingUp ? attackTimer : float.PositiveInfinity;
        public int Health { get; private set; }

        internal void Initialize(ValidationWorld owner, bool boss, SpriteRenderer renderer, Collider2D collider, SpriteRenderer warningSprite)
        {
            world = owner;
            IsBoss = boss;
            appearance = renderer;
            hitbox = collider;
            warning = warningSprite;
            normalColor = boss ? new Color(.85f, .28f, .5f) : new Color(.9f, .46f, .25f);
            appearance.color = normalColor;
            Health = boss ? ValidationGameState.MaxBossHealth : 60;
            baseScale = transform.localScale;
            warning.enabled = false;
        }

        internal void Step(float delta)
        {
            if (dead || !world.State.IsCombatActive || (IsBoss && !world.State.CanEnterBoss)) return;
            if (stun > 0)
            {
                stun -= delta;
                appearance.color = Color.cyan;
                warning.enabled = false;
                return;
            }
            var offset = world.PlayerTransform.position - transform.position;
            var distance = Mathf.Abs(offset.x);
            if (windingUp)
            {
                attackTimer -= delta;
                warning.enabled = true;
                warning.color = attackTimer <= ValidationGameState.ParryWindow ? Color.white : Color.yellow;
                appearance.color = Color.Lerp(normalColor, Color.white, .4f + .2f * Mathf.Sin(attackTimer * 25));
                if (attackTimer > 0) return;
                windingUp = false;
                warning.enabled = false;
                cooldown = IsBoss ? .65f : .9f;
                if (distance < (IsBoss ? 2f : 1.65f) && Mathf.Abs(offset.y) < 1.8f)
                    world.ResolveEnemyAttack(attackId, IsBoss ? 24 : 12);
                return;
            }
            appearance.color = normalColor;
            cooldown -= delta;
            if (distance < 8f && distance > (IsBoss ? 1.5f : 1.15f))
            {
                var speed = IsBoss ? 1.4f : 1.1f;
                transform.position += Vector3.right * (Mathf.Sign(offset.x) * speed * delta);
            }
            if (cooldown <= 0 && distance < (IsBoss ? 1.9f : 1.5f))
            {
                windingUp = true;
                attackTimer = IsBoss ? .65f : .85f;
                attackId = world.NextAttackId();
            }
            var scale = baseScale;
            scale.y *= 1 + .035f * Mathf.Sin(world.CombatTime * 7);
            transform.localScale = scale;
        }

        public void TakeDamage(int damage, float stunSeconds = 0)
        {
            if (dead || damage <= 0 || !world.State.IsCombatActive) return;
            if (IsBoss)
            {
                if (!world.State.DamageBoss(damage)) return;
                Health = world.State.BossHealth;
            }
            else Health = Mathf.Max(0, Health - damage);
            stun = Mathf.Max(stun, stunSeconds);
            if (stunSeconds > 0)
            {
                windingUp = false;
                warning.enabled = false;
                cooldown = .6f;
            }
            world.HitFeedback(transform.position);
            if (Health > 0) return;
            dead = true;
            appearance.enabled = false;
            warning.enabled = false;
            hitbox.enabled = false;
            if (!IsBoss && world.State.DefeatEnemy(Random.value))
                world.CreateHealthPickup(transform.position);
        }
    }
}
