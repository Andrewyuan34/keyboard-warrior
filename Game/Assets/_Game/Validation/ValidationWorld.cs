using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace KeyboardWarrior.Validation
{
    /// <summary>A deliberately small, self-contained greybox for verifying the chosen Unity toolchain.</summary>
    public sealed class ValidationWorld : MonoBehaviour
    {
        private readonly List<ValidationEnemy> enemies = new List<ValidationEnemy>();
        private Transform runtimeRoot;
        private ValidationPresentation presentation;
        private SpriteRenderer playerSprite;
        private SpriteRenderer leftRune;
        private SpriteRenderer rightRune;
        private Camera followCamera;
        private Keyboard keyboard;
        private Sprite square;
        private float lastParryPress = float.NegativeInfinity;
        private float lastAttack = -10;
        private float facing = 1;
        private float movement;
        private float noticeTimer;
        private string notice;
        private int nextAttackId;
        private int appliedCastVersion;
        private int puzzleStage;
        private bool bodyFrozen;
        private Vector2 savedVelocity;
        private bool manualPause;

        public static ValidationWorld Instance { get; private set; }
        public ValidationGameState State { get; private set; }
        public Rigidbody2D PlayerBody { get; private set; }
        public Transform PlayerTransform => PlayerBody.transform;
        public Collider2D GroundCollider { get; private set; }
        public Collider2D GateCollider { get; private set; }
        public IReadOnlyList<ValidationEnemy> Enemies => enemies;
        public float CombatTime { get; private set; }
        public bool IsGrounded => PlayerBody != null && Physics2D.OverlapCircle(
            PlayerBody.position + Vector2.down * .67f, .13f, 1 << 8) != null;

        public static ValidationWorld Create()
        {
            return new GameObject("Keyboard Warrior Validation").AddComponent<ValidationWorld>();
        }

        private void Awake()
        {
            Instance = this;
            State = new ValidationGameState();
            square = Resources.Load<Sprite>("Validation/White");
            if (square == null)
                throw new System.InvalidOperationException(
                    "Required validation sprite could not load: Resources/Validation/White.png. " +
                    "Fetch Git LFS assets and confirm Unity imports the fixture as a Sprite before running the scene.");
            BuildWorld();
        }

        private void OnEnable() { ConnectKeyboard(); }

        private void Start()
        {
            var arguments = System.Environment.GetCommandLineArgs();
            for (var i = 0; i + 1 < arguments.Length; i++)
            {
                if (arguments[i] != "-validationSmoke") continue;
                gameObject.AddComponent<ValidationSmokeRunner>().Begin(this, arguments[i + 1], arguments);
                break;
            }
        }

        private void OnDisable()
        {
            if (keyboard != null) keyboard.onTextInput -= HandleText;
            keyboard = null;
        }

        private void OnDestroy()
        {
            if (keyboard != null) keyboard.onTextInput -= HandleText;
            keyboard = null;
            presentation?.Dispose();
            if (Instance == this) Instance = null;
        }

        private void BuildWorld()
        {
            enemies.Clear();
            runtimeRoot = new GameObject("Generated validation level").transform;
            runtimeRoot.SetParent(transform);
            var ground = Rectangle("Ground", new Vector2(11, -.5f), new Vector2(36, 1), new Color(.16f, .22f, .3f));
            ground.gameObject.layer = 8;
            GroundCollider = ground.gameObject.AddComponent<BoxCollider2D>();
            MakeWall("Left boundary", -5);
            MakeWall("Right boundary", 28);
            for (var i = 0; i < 14; i++)
                Rectangle("Background pillar " + i, new Vector2(-4 + i * 2.5f, 2.5f), new Vector2(.3f, 5), new Color(.08f, .11f, .18f), -5);
            var platform = Rectangle("Jump platform", new Vector2(2.5f, .8f), new Vector2(1.5f, .3f), new Color(.25f, .35f, .46f));
            platform.gameObject.layer = 8;
            platform.gameObject.AddComponent<BoxCollider2D>();

            playerSprite = Rectangle("Player", new Vector2(0, .75f), new Vector2(.65f, 1.2f), new Color(.35f, .75f, 1));
            playerSprite.gameObject.AddComponent<BoxCollider2D>();
            PlayerBody = playerSprite.gameObject.AddComponent<Rigidbody2D>();
            PlayerBody.gravityScale = 3;
            PlayerBody.freezeRotation = true;
            PlayerBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            PlayerBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            // Child decoration gives direction and a recognizable keyboard silhouette.
            var weapon = Rectangle("Keyboard weapon", new Vector2(.32f, .62f), new Vector2(.6f, .16f), new Color(.8f, .85f, .95f), 2);
            weapon.transform.SetParent(playerSprite.transform, true);

            SpawnEnemy(new Vector2(5, .6f), false);
            SpawnEnemy(new Vector2(10.5f, .6f), false);
            SpawnEnemy(new Vector2(22, .9f), true);
            leftRune = Rectangle("Left rune - press E first", new Vector2(7.5f, .65f), new Vector2(.4f, 1.3f), new Color(.3f, .35f, .7f));
            rightRune = Rectangle("Right rune - press E second", new Vector2(10, .65f), new Vector2(.4f, 1.3f), new Color(.55f, .3f, .65f));
            var gate = Rectangle("Boss gate", new Vector2(14, 2), new Vector2(.4f, 4), new Color(.45f, .5f, .6f));
            GateCollider = gate.gameObject.AddComponent<BoxCollider2D>();
            Rectangle("Boss arena floor", new Vector2(21, -.04f), new Vector2(12, .08f), new Color(.5f, .2f, .35f), 1);
            var cameraObject = new GameObject("Validation camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(runtimeRoot);
            followCamera = cameraObject.GetComponent<Camera>();
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            followCamera.orthographic = true;
            followCamera.orthographicSize = 5;
            followCamera.clearFlags = CameraClearFlags.SolidColor;
            followCamera.backgroundColor = new Color(.035f, .055f, .1f);
            followCamera.transform.position = new Vector3(4, 3, -10);
            presentation = new ValidationPresentation(runtimeRoot);
            ShowNotice("A keyboard bites back. Parry when the enemy's yellow marker turns WHITE to charge a Smite.", 10);
            presentation.Refresh(State, notice, puzzleStage);
        }

        private void MakeWall(string name, float x)
        {
            var wall = Rectangle(name, new Vector2(x, 3), new Vector2(.4f, 7), new Color(.12f, .2f, .29f));
            wall.gameObject.layer = 8;
            wall.gameObject.AddComponent<BoxCollider2D>();
        }

        private void SpawnEnemy(Vector2 position, bool boss)
        {
            var sprite = Rectangle(boss ? "Boss" : "Enemy", position,
                boss ? new Vector2(1.2f, 1.8f) : new Vector2(.7f, 1.2f), Color.white);
            var collider = sprite.gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            var warning = Rectangle("Parry telegraph", position + Vector2.up * (boss ? 1.3f : 1), new Vector2(.3f, .3f), Color.yellow, 3);
            warning.transform.SetParent(sprite.transform, true);
            var enemy = sprite.gameObject.AddComponent<ValidationEnemy>();
            enemy.Initialize(this, boss, sprite, collider, warning);
            enemies.Add(enemy);
        }

        private SpriteRenderer Rectangle(string name, Vector2 position, Vector2 size, Color color, int order = 0)
        {
            var obj = new GameObject(name, typeof(SpriteRenderer));
            obj.transform.SetParent(runtimeRoot);
            obj.transform.position = position;
            obj.transform.localScale = size;
            var renderer = obj.GetComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }

        private void ConnectKeyboard()
        {
            if (keyboard == Keyboard.current) return;
            if (keyboard != null) keyboard.onTextInput -= HandleText;
            keyboard = Keyboard.current;
            if (keyboard != null) keyboard.onTextInput += HandleText;
        }

        private void Update() { Step(Time.unscaledDeltaTime); }

        /// <summary>Single frame hook; Physics2D still runs through Unity's real physics loop.</summary>
        public void Step(float unscaledDelta)
        {
            ConnectKeyboard();
            PollKeyboard();
            var delta = Mathf.Max(0, unscaledDelta);
            var wasTyping = State.IsTyping;
            State.Tick(delta);
            if (wasTyping && !State.IsTyping && State.LastCastOutcome == CastOutcome.Timeout)
                ShowNotice("Typing timed out. The charge was spent; parry to try again.", 5);
            if (State.CastSuccessVersion > appliedCastVersion)
            {
                appliedCastVersion = State.CastSuccessVersion;
                ApplySuccessfulSmite(State.SelectedSmite);
            }
            UpdatePlayerPhysics();
            if (State.IsCombatActive)
            {
                CombatTime += delta;
                foreach (var enemy in enemies) enemy.Step(delta);
                if (PlayerTransform.position.y < -4) State.DamagePlayer(100);
                noticeTimer -= delta;
            }
            var gateOpen = State.CanEnterBoss;
            GateCollider.enabled = !gateOpen;
            GateCollider.GetComponent<SpriteRenderer>().enabled = !gateOpen;
            leftRune.color = puzzleStage >= 1 ? Color.green : new Color(.3f, .35f, .7f);
            rightRune.color = State.PuzzleSolved ? Color.green : new Color(.55f, .3f, .65f);
            playerSprite.color = CombatTime - lastParryPress < ValidationGameState.ParryWindow
                ? Color.white : new Color(.35f, .75f, 1);
            var cameraTarget = new Vector3(Mathf.Clamp(PlayerTransform.position.x + 2, 3, 23), 3, -10);
            followCamera.transform.position = Vector3.Lerp(followCamera.transform.position, cameraTarget, 1 - Mathf.Exp(-7 * delta));
            presentation.Refresh(State, noticeTimer > 0 ? notice : DefaultHint(), puzzleStage);
        }

        private string DefaultHint()
        {
            if (!State.PuzzleSolved) return "Two runes guard the gate: LEFT, then RIGHT. Stand by each rune and press E.";
            if (State.RemainingEnemies > 0) return "Runes solved. Defeat both orange enemies to open the boss gate.";
            return "Gate open. Defeat the pink boss! Smite 1: lightning. Smite 2: healing shockwave.";
        }

        private void PollKeyboard()
        {
            if (keyboard == null) { movement = 0; return; }
            if (keyboard.f10Key.wasPressedThisFrame) Application.Quit();
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                manualPause = !manualPause;
                State.SetPaused(manualPause);
            }
            if (State.IsPaused) { movement = 0; return; }
            if (State.IsDead || State.HasWon)
            {
                movement = 0;
                if (keyboard.rKey.wasPressedThisFrame) ResetLevel();
                return;
            }
            if (State.IsTyping)
            {
                movement = 0;
                if (keyboard.backspaceKey.wasPressedThisFrame) State.Backspace();
                if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
                {
                    if (!State.SubmitCast()) ShowNotice("The sentence must match exactly. Backspace can fix mistakes.", 3);
                }
                return;
            }
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.digit2Key.wasPressedThisFrame)
            {
                var slot = keyboard.digit1Key.wasPressedThisFrame ? 1 : 2;
                if (State.TryStartCast(slot)) { movement = 0; return; }
                ShowNotice("Parry an enemy attack to earn a Smite charge first.", 3);
            }
            movement = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0) -
                (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0);
            if (movement != 0) facing = movement;
            if (keyboard.spaceKey.wasPressedThisFrame && IsGrounded)
                PlayerBody.linearVelocity = new Vector2(PlayerBody.linearVelocity.x, 9);
            if (keyboard.kKey.wasPressedThisFrame) lastParryPress = CombatTime;
            if (keyboard.jKey.wasPressedThisFrame) PlayerAttack();
            if (keyboard.eKey.wasPressedThisFrame) InteractWithRune();
        }

        private void UpdatePlayerPhysics()
        {
            if (!State.IsCombatActive)
            {
                if (!bodyFrozen) savedVelocity = PlayerBody.linearVelocity;
                bodyFrozen = true;
                PlayerBody.linearVelocity = Vector2.zero;
                PlayerBody.simulated = false;
                return;
            }
            if (bodyFrozen)
            {
                PlayerBody.simulated = true;
                PlayerBody.linearVelocity = savedVelocity;
                bodyFrozen = false;
            }
            PlayerBody.linearVelocity = new Vector2(movement * 5, PlayerBody.linearVelocity.y);
        }

        public void HandleText(char character) { State?.TypeCharacter(character); }

        public void SetFocused(bool focused) { State.SetFocused(focused); }
        private void OnApplicationFocus(bool focused) { State?.SetFocused(focused); }

        private void PlayerAttack()
        {
            if (!State.IsCombatActive || CombatTime - lastAttack < .3f) return;
            lastAttack = CombatTime;
            var hitPosition = PlayerTransform.position + Vector3.right * facing * .85f;
            Flash("Attack arc", hitPosition, new Vector2(1.2f, .25f), Color.white, .12f);
            foreach (var enemy in enemies)
            {
                var delta = enemy.transform.position - PlayerTransform.position;
                if (enemy.IsAlive && Mathf.Abs(delta.x) < 1.7f && Mathf.Abs(delta.y) < 1.5f && delta.x * facing > -.2f)
                    enemy.TakeDamage(20);
            }
        }

        private void InteractWithRune()
        {
            if (State.PuzzleSolved) return;
            var x = PlayerTransform.position.x;
            if (Mathf.Abs(x - 7.5f) < 1.2f)
            {
                puzzleStage = 1;
                ShowNotice("Left rune active. Now activate the right rune.", 4);
            }
            else if (Mathf.Abs(x - 10) < 1.2f)
            {
                if (puzzleStage == 1)
                {
                    puzzleStage = 2;
                    State.SolvePuzzle();
                    presentation.PlayPickup();
                    ShowNotice("Runes solved. Clear the enemies to open the boss gate.", 5);
                }
                else ShowNotice("Wrong order. Activate the LEFT rune first.", 4);
            }
        }

        public int NextAttackId() { return ++nextAttackId; }

        public AttackOutcome ResolveEnemyAttack(int attackId, int damage)
        {
            var result = State.ReceiveEnemyAttack(attackId, CombatTime - lastParryPress, damage);
            presentation.Play(result);
            if (result == AttackOutcome.Parried)
            {
                Flash("Parry burst", PlayerTransform.position, new Vector2(1.2f, 1.5f), new Color(.8f, 1, .2f, .5f), .15f);
                ShowNotice("PARRY! +1 charge. Press 1 for lightning or 2 for healing shockwave.", 5);
            }
            else if (result == AttackOutcome.Damaged) ShowNotice("Hit! Tap K as the telegraph turns WHITE; holding K does not keep parrying.", 5);
            return result;
        }

        private void ApplySuccessfulSmite(int slot)
        {
            presentation.PlayCast();
            var range = slot == 1 ? 4.5f : 6;
            var damage = slot == 1 ? 55 : 25;
            Flash(slot == 1 ? "Lightning" : "Shockwave", PlayerTransform.position,
                slot == 1 ? new Vector2(range * 2, 4) : new Vector2(range * 2, .6f),
                slot == 1 ? new Color(1, .9f, .2f, .6f) : new Color(.2f, 1, .9f, .6f), .4f);
            if (slot == 2) State.Heal(20);
            foreach (var enemy in enemies)
                if (enemy.IsAlive && Vector2.Distance(enemy.transform.position, PlayerTransform.position) < range)
                    enemy.TakeDamage(damage, slot == 2 ? 2 : .4f);
            ShowNotice(slot == 1 ? "LIGHTNING! Heavy nearby damage." : "SHOCKWAVE! Nearby enemies stunned; recover 20 health.", 4);
        }

        internal void HitFeedback(Vector3 position)
        {
            presentation.PlayHit();
            Flash("Hit spark", position, new Vector2(.35f, .35f), Color.yellow, .15f);
        }

        internal void CreateHealthPickup(Vector2 position) { SpawnHealthPickup(position); }

        public GameObject SpawnHealthPickup(Vector2 position)
        {
            var visual = Rectangle("Health drop +20", position, new Vector2(.35f, .35f), Color.green, 2);
            var collider = visual.gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            visual.gameObject.AddComponent<ValidationHealthPickup>().Initialize(this);
            return visual.gameObject;
        }

        internal void CollectHealth()
        {
            State.Heal(20);
            presentation.PlayPickup();
            ShowNotice("Health pickup: +20 (maximum 100).", 3);
        }

        private void Flash(string name, Vector3 position, Vector2 size, Color color, float duration)
        {
            var effect = Rectangle(name, position, size, color, 4).gameObject;
            Destroy(effect, duration);
        }

        private void ShowNotice(string message, float duration)
        {
            notice = message;
            noticeTimer = duration;
        }

        public void ResetLevel()
        {
            presentation.Dispose();
            runtimeRoot.gameObject.SetActive(false);
            Destroy(runtimeRoot.gameObject);
            State.Reset();
            CombatTime = 0;
            lastParryPress = float.NegativeInfinity;
            lastAttack = -10;
            nextAttackId = 0;
            appliedCastVersion = 0;
            puzzleStage = 0;
            movement = 0;
            facing = 1;
            bodyFrozen = false;
            manualPause = false;
            BuildWorld();
        }
    }
}
