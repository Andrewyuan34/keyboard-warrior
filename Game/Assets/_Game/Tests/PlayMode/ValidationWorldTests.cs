using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace KeyboardWarrior.Validation.Tests
{
    // These tests send actual Input System device/text events and run the 2D physics engine.
    // The world Update is disabled only to make elapsed time deterministic during each test.
    public sealed class ValidationWorldTests
    {
        private ValidationWorld world;
        private Keyboard keyboard;
        private Keyboard previousKeyboard;
        private SimulationMode2D previousSimulationMode;
        private InputSettings.UpdateMode previousInputMode;
        private InputSettings.BackgroundBehavior previousBackgroundBehavior;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode previousEditorInputBehavior;
#endif
        private Key[] previousHeldKeys = new Key[0];

        [SetUp]
        public void SetUp()
        {
            previousKeyboard = Keyboard.current;
            previousInputMode = InputSystem.settings.updateMode;
            previousBackgroundBehavior = InputSystem.settings.backgroundBehavior;
            // Batch Test Runner has no focused Game View. Route synthetic events to the
            // player state buffers, as the Input System's own test fixture does.
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            previousEditorInputBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
            keyboard = InputSystem.AddDevice<Keyboard>("Validation test keyboard");
            keyboard.MakeCurrent();
            previousSimulationMode = Physics2D.simulationMode;
            Physics2D.simulationMode = SimulationMode2D.Script;
            if (ValidationWorld.Instance != null)
                Object.DestroyImmediate(ValidationWorld.Instance.gameObject);
            world = ValidationWorld.Create();
            world.enabled = false;
            previousHeldKeys = new Key[0];
            Frame(0f);
        }

        [TearDown]
        public void TearDown()
        {
            if (world != null) Object.DestroyImmediate(world.gameObject);
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            if (previousKeyboard != null && previousKeyboard.added) previousKeyboard.MakeCurrent();
            InputSystem.settings.updateMode = previousInputMode;
            InputSystem.settings.backgroundBehavior = previousBackgroundBehavior;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorInputBehavior;
#endif
            Physics2D.simulationMode = previousSimulationMode;
        }

        private void Frame(float delta, params Key[] heldKeys)
        {
            keyboard.MakeCurrent();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(heldKeys));
            InputSystem.Update();
            foreach (Key key in heldKeys)
            {
                Assert.That(keyboard[key].isPressed, Is.True, "Synthetic keyboard event was not delivered: " + key);
                if (System.Array.IndexOf(previousHeldKeys, key) < 0)
                    Assert.That(keyboard[key].wasPressedThisFrame, Is.True,
                        "Synthetic press was not routed to the player input frame: " + key);
            }
            previousHeldKeys = heldKeys;
            world.Step(delta);
            Physics2D.SyncTransforms();
            if (delta > 0f) Physics2D.Simulate(delta);
        }

        private void Frames(int count, params Key[] heldKeys)
        {
            for (int i = 0; i < count; i++) Frame(.02f, heldKeys);
        }

        private void Text(string value)
        {
            keyboard.MakeCurrent();
            foreach (char character in value) InputSystem.QueueTextEvent(keyboard, character);
            InputSystem.Update();
            world.Step(0f);
        }

        private void EarnEnergy(int count = 1)
        {
            for (int i = 0; i < count; i++)
                world.State.ReceiveEnemyAttack(1000 + i, .05f, 20);
        }

        private void PlacePlayer(Vector2 position)
        {
            // The playable body interpolates its rendered Transform. A test teleport
            // must arrange both poses before asking range-based gameplay to run.
            world.PlayerTransform.position = new Vector3(position.x, position.y, world.PlayerTransform.position.z);
            world.PlayerBody.position = position;
            world.PlayerBody.linearVelocity = Vector2.zero;
            Physics2D.SyncTransforms();
            Assert.That(Vector2.Distance(world.PlayerBody.position, position), Is.LessThan(.001f));
            Assert.That(Vector2.Distance(world.PlayerTransform.position, position), Is.LessThan(.001f));
        }

        [UnityTest]
        public IEnumerator EnabledComponent_ConsumesKeyboardInTheRealPlayerLoop()
        {
            Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
            world.enabled = true;
            // Let Unity dispatch its initial batch-mode focus callback, then arrange the
            // focused gameplay state. Focus-loss behavior has its own explicit test.
            yield return null;
            world.SetFocused(true);
            keyboard.MakeCurrent();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
            InputSystem.Update();
            float initialX = world.PlayerBody.position.x;
            yield return null;
            yield return new WaitForFixedUpdate();
            yield return null;
            yield return new WaitForFixedUpdate();
            Assert.That(world.PlayerBody.linearVelocity.x, Is.GreaterThan(0f),
                "The enabled Update method must consume the real Input System state.");
            Assert.That(world.PlayerBody.position.x, Is.GreaterThan(initialX),
                "The normal player loop must integrate velocity into actual movement.");
        }

        [Test]
        public void VirtualMovementAndJump_WorkAgainstActualFloorPhysics()
        {
            Frames(80);
            Assert.That(world.IsGrounded, Is.True);
            Collider2D playerCollider = world.PlayerBody.GetComponent<Collider2D>();
            Assert.That(playerCollider.bounds.min.y,
                Is.GreaterThanOrEqualTo(world.GroundCollider.bounds.max.y - .08f),
                "The player must rest on the floor instead of falling through it.");
            float startX = world.PlayerBody.position.x;
            Frames(10, Key.D);
            Assert.That(world.PlayerBody.position.x, Is.GreaterThan(startX + .1f));
            Frame(.02f);
            Assert.That(Mathf.Abs(world.PlayerBody.linearVelocity.x), Is.LessThan(.01f));
            float groundedY = world.PlayerBody.position.y;
            Frame(.02f, Key.Space);
            Assert.That(world.PlayerBody.linearVelocity.y, Is.GreaterThan(0f));
            Frames(8);
            Assert.That(world.PlayerBody.position.y, Is.GreaterThan(groundedY + .1f));
            Frames(120);
            Assert.That(world.IsGrounded, Is.True, "The real jump must land again.");
        }

        [Test]
        public void TimedKeyboardParry_BlocksARealEnemyAttack_ButHoldingDoesNotBlockTheNext()
        {
            Frames(80);
            ValidationEnemy enemy = null;
            foreach (ValidationEnemy candidate in world.GetComponentsInChildren<ValidationEnemy>())
            {
                if (!candidate.IsBoss) { enemy = candidate; break; }
            }
            Assert.That(enemy, Is.Not.Null);
            float groundY = world.GroundCollider.bounds.max.y
                + world.PlayerBody.GetComponent<Collider2D>().bounds.extents.y + .03f;
            PlacePlayer(new Vector2(enemy.transform.position.x - 1f, groundY));
            for (int i = 0; i < 150 && !enemy.IsWindingUp; i++) Frame(.02f);
            Assert.That(enemy.IsWindingUp, Is.True, "A nearby enemy should telegraph its real attack.");
            for (int i = 0; i < 100 && enemy.TimeUntilImpact > .10f; i++) Frame(.02f);
            Frame(.02f, Key.K);
            for (int i = 0; i < 20 && enemy.IsWindingUp; i++) Frame(.02f, Key.K);
            Assert.That(world.State.Health, Is.EqualTo(100));
            Assert.That(world.State.Energy, Is.EqualTo(1));
            for (int i = 0; i < 150 && !enemy.IsWindingUp; i++) Frame(.02f, Key.K);
            Assert.That(enemy.IsWindingUp, Is.True);
            for (int i = 0; i < 100 && enemy.IsWindingUp; i++) Frame(.02f, Key.K);
            Assert.That(world.State.Health, Is.LessThan(100), "A held key is no longer a precisely timed press.");
            Assert.That(world.State.Energy, Is.EqualTo(1));
        }

        [Test]
        public void TextEventsAndGameplayKeys_AreSeparatedDuringCast()
        {
            Frames(80);
            ValidationEnemy nearbyEnemy = null;
            foreach (ValidationEnemy candidate in world.GetComponentsInChildren<ValidationEnemy>())
                if (!candidate.IsBoss) { nearbyEnemy = candidate; break; }
            Assert.That(nearbyEnemy, Is.Not.Null);
            PlacePlayer(new Vector2(nearbyEnemy.transform.position.x - .9f, world.PlayerBody.position.y));
            int enemyHealth = nearbyEnemy.Health;
            EarnEnergy(2);
            Frame(.02f, Key.Digit1);
            Assert.That(world.State.IsTyping, Is.True);
            Assert.That(world.State.Energy, Is.EqualTo(1));
            Vector2 position = world.PlayerBody.position;
            Frames(5, Key.A, Key.D, Key.Space, Key.J, Key.K, Key.E, Key.Digit2);
            Assert.That(world.PlayerBody.position, Is.EqualTo(position));
            Assert.That(world.PlayerBody.simulated, Is.False);
            Assert.That(world.State.TypedText, Is.Empty,
                "Key state events are controls; text must come through onTextInput.");
            Assert.That(world.State.Energy, Is.EqualTo(1));
            Assert.That(world.State.PuzzleSolved, Is.False);
            Assert.That(world.State.RemainingEnemies, Is.EqualTo(2));
            Assert.That(nearbyEnemy.Health, Is.EqualTo(enemyHealth),
                "Typing-mode J must not attack an enemy that is within melee reach.");
            Text("x");
            Assert.That(world.State.TypedText, Is.EqualTo("x"));
            Frame(0f, Key.Enter);
            Assert.That(world.State.IsTyping, Is.True);
            Frame(0f);
            Frame(0f, Key.Backspace);
            Assert.That(world.State.TypedText, Is.Empty);
            Frame(0f);
            Text(world.State.Prompt);
            Frame(0f, Key.Enter);
            Assert.That(world.State.IsTyping, Is.False);
            Assert.That(world.State.LastCastOutcome, Is.EqualTo(CastOutcome.Success));
            Assert.That(world.State.CastSuccessVersion, Is.EqualTo(1));
            Assert.That(world.State.Energy, Is.EqualTo(1));
            Frames(3, Key.Enter);
            Assert.That(world.State.CastSuccessVersion, Is.EqualTo(1));
        }

        [Test]
        public void AttackKey_DamagesAnActualNearbyEnemy()
        {
            Frames(80);
            ValidationEnemy enemy = null;
            foreach (ValidationEnemy candidate in world.GetComponentsInChildren<ValidationEnemy>())
                if (!candidate.IsBoss) { enemy = candidate; break; }
            Assert.That(enemy, Is.Not.Null);
            PlacePlayer(new Vector2(enemy.transform.position.x - .9f, world.PlayerBody.position.y));
            Assert.That(enemy.transform.position.x - world.PlayerTransform.position.x, Is.InRange(.8f, 1f),
                "Arrange a target in front of the player and within melee reach before sending J.");
            Assert.That(Mathf.Abs(enemy.transform.position.y - world.PlayerTransform.position.y), Is.LessThan(1.5f));
            int health = enemy.Health;
            Frame(.02f, Key.J);
            Assert.That(enemy.Health, Is.LessThan(health));
            Assert.That(enemy.IsAlive, Is.True);
        }

        [Test]
        public void LightningTextCast_DamagesNearbyEnemyByFiftyFive_ExactlyOnce()
        {
            Frames(80);
            ValidationEnemy enemy = world.Enemies[0];
            ValidationEnemy distantEnemy = world.Enemies[1];
            enemy.transform.position = new Vector3(world.PlayerBody.position.x + 1f, .6f, 0f);
            Physics2D.SyncTransforms();
            world.State.DamagePlayer(35);
            EarnEnergy(2);
            Frame(0f, Key.Digit1);
            Assert.That(enemy.Health, Is.EqualTo(60), "Selecting a skill spends energy but does not apply its effect.");
            Text(world.State.Prompt);
            Assert.That(enemy.Health, Is.EqualTo(60), "A completed sentence still requires submission.");
            Frame(0f, Key.Enter);
            Assert.That(enemy.Health, Is.EqualTo(5), "Lightning must apply 55 damage to the actual nearby actor.");
            Assert.That(distantEnemy.Health, Is.EqualTo(60), "Lightning must not damage a target outside its range.");
            Assert.That(world.State.Health, Is.EqualTo(65), "Lightning is not the healing skill.");
            Assert.That(world.State.Energy, Is.EqualTo(1));
            Frames(5, Key.Enter);
            Assert.That(enemy.Health, Is.EqualTo(5), "Holding Enter must not apply the successful cast again.");
            Assert.That(world.State.CastSuccessVersion, Is.EqualTo(1));
        }

        [TestCase(40, 80)]
        [TestCase(5, 100)]
        public void ShockwaveTextCast_DamagesByTwentyFive_AndHealsTwentyWithCap(int initialDamage, int expectedHealth)
        {
            Frames(80);
            ValidationEnemy enemy = world.Enemies[0];
            ValidationEnemy distantEnemy = world.Enemies[1];
            enemy.transform.position = new Vector3(world.PlayerBody.position.x + 1f, .6f, 0f);
            Physics2D.SyncTransforms();
            world.State.DamagePlayer(initialDamage);
            EarnEnergy(2);
            Frame(0f, Key.Digit2);
            Assert.That(world.State.Health, Is.EqualTo(100 - initialDamage));
            Assert.That(enemy.Health, Is.EqualTo(60));
            Text(world.State.Prompt);
            Frame(0f, Key.Enter);
            Assert.That(enemy.Health, Is.EqualTo(35), "Shockwave must apply 25 damage to the actual nearby actor.");
            Assert.That(distantEnemy.Health, Is.EqualTo(60));
            Assert.That(world.State.Health, Is.EqualTo(expectedHealth));
            Assert.That(world.State.Energy, Is.EqualTo(1));
            Frames(5, Key.Enter);
            Assert.That(enemy.Health, Is.EqualTo(35));
            Assert.That(world.State.Health, Is.EqualTo(expectedHealth), "The heal must also occur exactly once.");
            Assert.That(world.State.CastSuccessVersion, Is.EqualTo(1));
        }

        [Test]
        public void ShockwaveInterruptsAnImminentEnemyAttack_AndEnemyResumesAfterTemporaryStun()
        {
            Frames(80);
            ValidationEnemy enemy = world.Enemies[0];
            enemy.transform.position = new Vector3(world.PlayerBody.position.x + 1f, .6f, 0f);
            Physics2D.SyncTransforms();
            EarnEnergy();
            for (int i = 0; i < 100 && !enemy.IsWindingUp; i++) Frame(.02f);
            Assert.That(enemy.IsWindingUp, Is.True);
            for (int i = 0; i < 100 && enemy.TimeUntilImpact > .06f; i++) Frame(.02f);
            Assert.That(world.State.Health, Is.EqualTo(100));
            Assert.That(enemy.TimeUntilImpact, Is.LessThanOrEqualTo(.06f));
            Frame(0f, Key.Digit2);
            Text(world.State.Prompt);
            Frame(0f, Key.Enter);
            Assert.That(enemy.Health, Is.EqualTo(35));
            Assert.That(enemy.IsWindingUp, Is.False, "Shockwave must cancel the attack already in progress.");
            Frames(90);
            Assert.That(enemy.IsWindingUp, Is.False, "The two-second stun must still suppress attacks at 1.8 seconds.");
            Assert.That(world.State.Health, Is.EqualTo(100));
            for (int i = 0; i < 200 && world.State.Health == 100; i++) Frame(.02f);
            Assert.That(world.State.Health, Is.LessThan(100),
                "The stunned enemy must eventually attack again, rather than remaining disabled permanently.");
            Assert.That(enemy.Health, Is.EqualTo(35));
        }

        [Test]
        public void EscapeAndFocusLoss_FreezeWorldAndDeadlineThroughActualInputPath()
        {
            Frames(80);
            EarnEnergy();
            Frame(0f, Key.Digit1);
            float remaining = world.State.TimeRemaining;
            Vector2 position = world.PlayerBody.position;
            Frame(.02f, Key.Escape);
            Assert.That(world.State.IsPaused, Is.True);
            Frames(100, Key.D);
            Text("blocked");
            Assert.That(world.State.TimeRemaining, Is.EqualTo(remaining));
            Assert.That(world.State.TypedText, Is.Empty);
            Assert.That(world.PlayerBody.position, Is.EqualTo(position));
            Frame(0f);
            Frame(0f, Key.Escape);
            Assert.That(world.State.IsPaused, Is.False);

            world.SendMessage("OnApplicationFocus", false, SendMessageOptions.RequireReceiver);
            Frames(100);
            Text("blocked");
            Assert.That(world.State.TimeRemaining, Is.EqualTo(remaining));
            Assert.That(world.State.TypedText, Is.Empty);
            world.SendMessage("OnApplicationFocus", true, SendMessageOptions.RequireReceiver);
            Frame(1f);
            Assert.That(world.State.TimeRemaining, Is.EqualTo(remaining - 1f).Within(.001f));
        }

        [Test]
        public void CastTimeout_RestoresPhysicsWithoutRepeatingAnAttempt()
        {
            Frames(80);
            EarnEnergy(2);
            Frame(0f, Key.Digit1);
            Assert.That(world.PlayerBody.simulated, Is.False);
            // Each frame advances the same clock used in the playable prototype.
            Frames(605, Key.Digit1);
            Assert.That(world.State.IsTyping, Is.False);
            Assert.That(world.State.LastCastOutcome, Is.EqualTo(CastOutcome.Timeout));
            Assert.That(world.State.Energy, Is.EqualTo(1), "Holding the skill key cannot start another attempt.");
            Assert.That(world.PlayerBody.simulated, Is.True);
        }

        [Test]
        public void ClosedBossGateBlocksRigidbody_AndOpensOnlyAfterBothRequirements()
        {
            Frames(80);
            Collider2D playerCollider = world.PlayerBody.GetComponent<Collider2D>();
            float gateX = world.GateCollider.bounds.min.x;
            float halfWidth = playerCollider.bounds.extents.x;
            float groundedY = world.GroundCollider.bounds.max.y + playerCollider.bounds.extents.y + .03f;
            PlacePlayer(new Vector2(gateX - halfWidth - .4f, groundedY));
            Frames(45, Key.D);
            Assert.That(world.GateCollider.enabled, Is.True);
            Assert.That(world.PlayerBody.position.x, Is.LessThanOrEqualTo(gateX - halfWidth + .1f));
            world.State.DefeatEnemy(1f);
            world.State.DefeatEnemy(1f);
            Frame(.02f, Key.D);
            Assert.That(world.GateCollider.enabled, Is.True, "Enemy clear does not bypass the puzzle.");
            world.State.SolvePuzzle();
            Frames(25, Key.D);
            Assert.That(world.GateCollider.enabled, Is.False);
            Assert.That(world.PlayerBody.position.x, Is.GreaterThan(gateX + .2f));
        }

        [Test]
        public void HealthPickup_UsesRealTriggerContact_AndCannotBeCollectedTwice()
        {
            Frames(80);
            world.State.DamagePlayer(40);
            GameObject pickup = world.SpawnHealthPickup(world.PlayerBody.position + Vector2.left * 2f);
            Frame(.02f);
            Assert.That(world.State.Health, Is.EqualTo(60));
            PlacePlayer(pickup.transform.position);
            Frame(.02f);
            Assert.That(world.State.Health, Is.EqualTo(80));
            Assert.That(pickup.activeSelf, Is.False);
            Frames(5);
            Assert.That(world.State.Health, Is.EqualTo(80));
            GameObject cappedPickup = world.SpawnHealthPickup(world.PlayerBody.position);
            Frame(.02f);
            Assert.That(world.State.Health, Is.EqualTo(100));
            Assert.That(cappedPickup.activeSelf, Is.False);
            world.SpawnHealthPickup(world.PlayerBody.position);
            Frame(.02f);
            Assert.That(world.State.Health, Is.EqualTo(100));
        }

        [Test]
        public void RuneKeys_RequireLeftThenRightAndActuallyUpdateTheGate()
        {
            Frames(80);
            Transform leftRune = null;
            Transform rightRune = null;
            foreach (Transform item in world.GetComponentsInChildren<Transform>())
            {
                if (item.name.StartsWith("Left rune")) leftRune = item;
                if (item.name.StartsWith("Right rune")) rightRune = item;
            }
            Assert.That(leftRune, Is.Not.Null);
            Assert.That(rightRune, Is.Not.Null);
            PlacePlayer(new Vector2(rightRune.position.x, world.PlayerBody.position.y));
            Frame(0f, Key.E);
            Assert.That(world.State.PuzzleSolved, Is.False);
            Frame(0f);
            PlacePlayer(new Vector2(leftRune.position.x, world.PlayerBody.position.y));
            Frame(0f, Key.E);
            Assert.That(world.State.PuzzleSolved, Is.False);
            Frame(0f);
            PlacePlayer(new Vector2(rightRune.position.x, world.PlayerBody.position.y));
            Frame(0f, Key.E);
            Assert.That(world.State.PuzzleSolved, Is.True);
            Assert.That(world.GateCollider.enabled, Is.True);
            foreach (ValidationEnemy enemy in world.Enemies)
                if (!enemy.IsBoss) enemy.TakeDamage(1000);
            Frame(0f);
            Assert.That(world.State.RemainingEnemies, Is.Zero);
            Assert.That(world.GateCollider.enabled, Is.False);
        }

        [Test]
        public void DefeatingActualBossAndRetrying_RestoresBossEnemiesAndClosedGate()
        {
            Frames(80);
            ValidationEnemy boss = null;
            foreach (ValidationEnemy enemy in world.Enemies)
            {
                if (enemy.IsBoss) boss = enemy;
                else enemy.TakeDamage(1000);
            }
            Assert.That(boss, Is.Not.Null);
            world.State.SolvePuzzle();
            boss.TakeDamage(1000);
            Frame(0f);
            Assert.That(world.State.HasWon, Is.True);
            Assert.That(boss.IsAlive, Is.False);
            Assert.That(world.PlayerBody.simulated, Is.False);
            Frame(0f, Key.R);
            Assert.That(world.State.HasWon, Is.False);
            Assert.That(world.State.RemainingEnemies, Is.EqualTo(2));
            Assert.That(world.State.BossHealth, Is.EqualTo(120));
            Assert.That(world.GateCollider.enabled, Is.True);
            int aliveCount = 0;
            foreach (ValidationEnemy enemy in world.Enemies) if (enemy.IsAlive) aliveCount++;
            Assert.That(aliveCount, Is.EqualTo(3));
        }

        [Test]
        public void RetryKeyAfterDeath_ResetsRulesPositionVelocityAndTyping()
        {
            Frames(80);
            Vector2 initialPosition = world.PlayerBody.position;
            Frames(10, Key.D);
            world.State.DamagePlayer(1000);
            Frame(.02f);
            Assert.That(world.State.IsDead, Is.True);
            Frame(0f, Key.R);
            Assert.That(world.State.IsDead, Is.False);
            Assert.That(world.State.Health, Is.EqualTo(100));
            Assert.That(world.State.Energy, Is.Zero);
            Assert.That(world.State.RemainingEnemies, Is.EqualTo(2));
            Assert.That(world.State.PuzzleSolved, Is.False);
            Assert.That(world.State.IsTyping, Is.False);
            Assert.That(Mathf.Abs(world.PlayerBody.position.x - initialPosition.x), Is.LessThan(.1f));
            Assert.That(world.PlayerBody.linearVelocity, Is.EqualTo(Vector2.zero));
        }
    }
}
