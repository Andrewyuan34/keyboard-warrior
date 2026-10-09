using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace KeyboardWarrior.Validation
{
    /// <summary>Opt-in standalone integration runner. Uses device events and the real player/physics loop.</summary>
    public sealed class ValidationSmokeRunner : MonoBehaviour
    {
        [Serializable]
        private sealed class CaseResult
        {
            public string name;
            public bool passed;
            public string evidence;
            public int frame;
            public float elapsedSeconds;
        }

        [Serializable]
        private sealed class Report
        {
            public string unityVersion;
            public string platform;
            public string graphicsDevice;
            public string graphicsApi;
            public int targetFps;
            public int renderedFrames;
            public float elapsedSeconds;
            public float meanObservedFps;
            public string scope;
            public bool passed;
            public string screenshot;
            public string typingScreenshot;
            public List<CaseResult> cases = new List<CaseResult>();
        }

        private ValidationWorld world;
        private Keyboard virtualKeyboard;
        private Keyboard previousKeyboard;
        private string reportPath;
        private Report report;
        private double started;
        private int startedFrame;
        private bool finished;

        public void Begin(ValidationWorld owner, string output, string[] arguments)
        {
            world = owner;
            reportPath = Path.GetFullPath(output);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            var fps = 60;
            for (var i = 0; i + 1 < arguments.Length; i++)
                if (arguments[i] == "-validationFps" && int.TryParse(arguments[i + 1], out var requested))
                    fps = Mathf.Clamp(requested, 15, 240);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = fps;
            Application.runInBackground = true;
            started = Time.realtimeSinceStartupAsDouble;
            startedFrame = Time.frameCount;
            previousKeyboard = Keyboard.current;
            virtualKeyboard = InputSystem.AddDevice<Keyboard>("Standalone validation keyboard");
            virtualKeyboard.MakeCurrent();
            report = new Report
            {
                unityVersion = Application.unityVersion,
                platform = Application.platform.ToString(),
                graphicsDevice = SystemInfo.graphicsDeviceName,
                graphicsApi = SystemInfo.graphicsDeviceType.ToString(),
                targetFps = fps,
                scope = "Actual standalone Update/FixedUpdate, Input System key/text events, 2D physics, rendering and audio. " +
                    "Controlled arrangements reset levels, teleport the player near fixtures, and award isolated cast-test energy through state. " +
                    "Parry, melee, typing, rune interaction, pickup and retry assertions use runtime paths. This is integration evidence, not an unaided human playthrough."
            };
            Application.logMessageReceived += OnLog;
            world.SetFocused(true);
            StartCoroutine(Run());
        }

        private void Update()
        {
            if (report != null && !finished && Time.realtimeSinceStartupAsDouble - started > 150)
            {
                Record("watchdog", false, "Standalone smoke exceeded 150 seconds.");
                Finish();
            }
        }

        private void OnLog(string message, string stack, LogType type)
        {
            if (finished || (type != LogType.Exception && type != LogType.Error && type != LogType.Assert)) return;
            Record("runtime log", false, message + "\n" + stack);
            Finish();
        }

        private void Record(string name, bool passed, string evidence)
        {
            report.cases.Add(new CaseResult
            {
                name = name,
                passed = passed,
                evidence = evidence,
                frame = Time.frameCount - startedFrame,
                elapsedSeconds = (float)(Time.realtimeSinceStartupAsDouble - started)
            });
        }

        private void Keys(params Key[] keys)
        {
            virtualKeyboard.MakeCurrent();
            InputSystem.QueueStateEvent(virtualKeyboard, new KeyboardState(keys));
        }

        private IEnumerator Frames(int count)
        {
            for (var i = 0; i < count; i++) yield return null;
        }

        private IEnumerator Press(params Key[] keys)
        {
            Keys(keys);
            yield return Frames(2);
            Keys();
            yield return Frames(2);
        }

        private IEnumerator Reset()
        {
            Keys();
            world.ResetLevel();
            world.SetFocused(true);
            yield return new WaitForSecondsRealtime(.35f);
        }

        private void ArrangePlayer(Vector2 position)
        {
            world.PlayerBody.position = position;
            // A fixture teleport must update both views immediately: interpolation can otherwise
            // leave the Transform at its previous rendered pose until the next physics frame.
            world.PlayerTransform.position = new Vector3(position.x, position.y, world.PlayerTransform.position.z);
            world.PlayerBody.linearVelocity = Vector2.zero;
            Physics2D.SyncTransforms();
            if (Vector2.Distance(world.PlayerBody.position, position) > .001f ||
                Vector2.Distance(world.PlayerTransform.position, position) > .001f)
                throw new InvalidOperationException("Validation fixture failed to place both player Rigidbody2D and Transform.");
        }

        private void ArrangeEnergy(int count)
        {
            for (var i = 0; i < count; i++) world.State.ReceiveEnemyAttack(10000 + i, .05f, 20);
        }

        private void Text(string value)
        {
            foreach (var character in value) InputSystem.QueueTextEvent(virtualKeyboard, character);
        }

        private void CaptureRenderedFrame(string path, string label)
        {
            // Called after WaitForEndOfFrame. Read the actual rendered framebuffer rather than
            // relying on CaptureScreenshot's asynchronous platform file-writing path.
            var capture = ScreenCapture.CaptureScreenshotAsTexture();
            if (capture == null)
            {
                Record(label, false, "The actual rendered framebuffer returned no texture.");
                return;
            }
            try
            {
                var pixels = capture.GetPixels32();
                var minBrightness = 765;
                var maxBrightness = 0;
                var nonzeroSamples = 0;
                var stride = Mathf.Max(1, pixels.Length / 10000);
                for (var i = 0; i < pixels.Length; i += stride)
                {
                    var brightness = pixels[i].r + pixels[i].g + pixels[i].b;
                    minBrightness = Mathf.Min(minBrightness, brightness);
                    maxBrightness = Mathf.Max(maxBrightness, brightness);
                    if (brightness > 20) nonzeroSamples++;
                }
                // Keep even a failed capture as diagnostic evidence; never substitute generated art.
                File.WriteAllBytes(path, capture.EncodeToPNG());
                var valid = capture.width >= 320 && capture.height >= 180 && nonzeroSamples > 100 &&
                    maxBrightness - minBrightness > 40;
                Record(label, valid,
                    $"Actual framebuffer {capture.width}x{capture.height}; nonblack sampled pixels={nonzeroSamples}; " +
                    $"brightness span={maxBrightness - minBrightness}; synchronous PNG={path}. Visual inspection remains required.");
            }
            finally
            {
                Destroy(capture);
            }
        }

        private IEnumerator Run()
        {
            yield return Frames(10);
            Keys();
            AudioSource music = null;
            foreach (var audioSource in world.GetComponentsInChildren<AudioSource>())
                if (audioSource.clip != null && audioSource.loop) music = audioSource;
            var sampleBefore = music != null ? music.timeSamples : -1;
            var dspBefore = AudioSettings.dspTime;
            yield return new WaitForSecondsRealtime(.5f);
            // Sample this source's DSP output, never the OS microphone/loopback or unrelated applications.
            // Several windows avoid mistaking a note's envelope boundary for silent output.
            var audioWindow = new float[1024];
            var maximumRms = 0.0;
            for (var window = 0; window < 8; window++)
            {
                if (music != null)
                {
                    music.GetOutputData(audioWindow, 0);
                    var sumSquares = 0.0;
                    foreach (var sample in audioWindow) sumSquares += (double)sample * sample;
                    maximumRms = Math.Max(maximumRms, Math.Sqrt(sumSquares / audioWindow.Length));
                }
                yield return new WaitForSecondsRealtime(.05f);
            }
            var audioAdvanced = music != null && music.isPlaying && music.timeSamples != sampleBefore;
            var dspAdvanced = AudioSettings.dspTime > dspBefore;
            Record("generated audio DSP output", audioAdvanced && dspAdvanced && maximumRms > .00001,
                $"Own music source playing/sample advancement={audioAdvanced}; DSP clock advanced={dspAdvanced}; " +
                $"maximum RMS across eight 1024-sample output windows={maximumRms:G6} (minimum 0.00001). " +
                "Verifies nonzero Unity source DSP output only; does not prove speaker/headphone audibility or human listening quality.");
            var x = world.PlayerBody.position.x;
            Keys(Key.D);
            yield return new WaitForSecondsRealtime(.2f);
            Keys();
            yield return Frames(2);
            var movementPassed = world.PlayerBody.position.x > x + .4f;
            var floorPassed = world.PlayerBody.GetComponent<Collider2D>().bounds.min.y > -.08f;
            var groundedY = world.PlayerBody.position.y;
            yield return Press(Key.Space);
            yield return new WaitForSecondsRealtime(.15f);
            var jumpPassed = world.PlayerBody.position.y > groundedY + .2f;
            yield return new WaitForSecondsRealtime(1);
            Record("keyboard movement, jump and floor", movementPassed && floorPassed && jumpPassed && world.IsGrounded,
                $"A/D input moved player={movementPassed}; floor collision={floorPassed}; jump={jumpPassed}; landed={world.IsGrounded}.");

            yield return Reset();
            var enemy = world.Enemies[0];
            ArrangePlayer(new Vector2(enemy.transform.position.x - 1, .65f));
            var deadline = Time.realtimeSinceStartupAsDouble + 4;
            while ((!enemy.IsWindingUp || enemy.TimeUntilImpact > .1f) && Time.realtimeSinceStartupAsDouble < deadline)
                yield return null;
            Keys(Key.K);
            while (enemy.IsWindingUp && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            var realParry = world.State.Energy == 1 && world.State.Health == 100;
            var hp = world.State.Health;
            yield return new WaitForSecondsRealtime(2.1f);
            var heldFailed = world.State.Health < hp && world.State.Energy == 1;
            Keys();
            Record("real telegraphed attack and held-key rejection", realParry && heldFailed,
                $"Player arranged beside enemy; actual K event parried={realParry}; holding K failed next attack={heldFailed}.");

            yield return Reset();
            enemy = world.Enemies[0];
            ArrangePlayer(new Vector2(enemy.transform.position.x - .9f, .65f));
            var enemyHp = enemy.Health;
            Keys(Key.J);
            yield return new WaitForSecondsRealtime(.45f);
            Keys();
            Record("one melee hit per key press", enemy.Health == enemyHp - 20,
                $"Held J for .45s; enemy health {enemyHp} -> {enemy.Health}; expected one 20-point hit.");

            yield return Reset();
            var lightningTarget = world.Enemies[0];
            ArrangePlayer(new Vector2(lightningTarget.transform.position.x - .9f, .65f));
            world.State.DamagePlayer(35);
            var lightningHealthBefore = lightningTarget.Health;
            ArrangeEnergy(3);
            yield return Press(Key.Digit1);
            var castingStart = world.PlayerBody.position;
            var combatClock = world.CombatTime;
            Keys(Key.A, Key.D, Key.J, Key.K, Key.E, Key.Space, Key.Digit2);
            yield return Frames(4);
            Keys();
            Text("x");
            yield return Frames(3);
            yield return Press(Key.Enter);
            var wrongEditable = world.State.IsTyping && world.State.TypedText == "x";
            yield return Press(Key.Backspace);
            var backspacePassed = world.State.TypedText == string.Empty;
            var frozen = Vector2.Distance(world.PlayerBody.position, castingStart) < .01f && Mathf.Abs(world.CombatTime - combatClock) < .001f;
            Text(world.State.Prompt);
            yield return Frames(3);
            report.typingScreenshot = Path.ChangeExtension(reportPath, ".typing.png");
            yield return new WaitForEndOfFrame();
            CaptureRenderedFrame(report.typingScreenshot, "rendered typing frame");
            yield return Press(Key.Enter);
            Record("QTE text, mistakes, correction and input isolation",
                frozen && wrongEditable && backspacePassed && world.State.LastCastOutcome == CastOutcome.Success && world.State.Energy == 2 &&
                lightningTarget.Health == lightningHealthBefore - 55 && world.State.Health == 65,
                $"Real onTextInput, wrong Enter, Backspace and correct sentence. Frozen={frozen}; wrong editable={wrongEditable}; corrected={backspacePassed}; energy={world.State.Energy}; " +
                $"nearby lightning target health {lightningHealthBefore}->{lightningTarget.Health} (expected55 damage); player health={world.State.Health} (expected65, no lightning heal).");

            var shockwaveTarget = world.Enemies[1];
            ArrangePlayer(new Vector2(shockwaveTarget.transform.position.x - .9f, .65f));
            var shockwaveHealthBefore = shockwaveTarget.Health;
            yield return Press(Key.Digit2);
            var beforePause = world.State.TimeRemaining;
            yield return Press(Key.Escape);
            var pausedAt = world.State.TimeRemaining;
            yield return new WaitForSecondsRealtime(.3f);
            var manuallyFrozen = world.State.IsPaused && Mathf.Abs(pausedAt - world.State.TimeRemaining) < .001f;
            yield return Press(Key.Escape);
            world.SetFocused(false);
            var unfocusedAt = world.State.TimeRemaining;
            Text("blocked");
            yield return new WaitForSecondsRealtime(.3f);
            var focusFrozen = Mathf.Abs(unfocusedAt - world.State.TimeRemaining) < .001f && world.State.TypedText.Length == 0;
            world.SetFocused(true);
            Text(world.State.Prompt);
            yield return Frames(3);
            yield return Press(Key.Enter);
            var shockwaveDamaged = shockwaveTarget.Health == shockwaveHealthBefore - 25;
            var shockwaveHealed = world.State.Health == 85;
            Record("pause, focus loss and second Smite", manuallyFrozen && focusFrozen && world.State.LastCastOutcome == CastOutcome.Success && world.State.SelectedSmite == 2 && shockwaveDamaged && shockwaveHealed,
                $"Escape froze QTE={manuallyFrozen}; focus callback arrangement froze QTE/text={focusFrozen}; slot2 successful={world.State.LastCastOutcome == CastOutcome.Success}; initial time={beforePause}; " +
                $"nearby shockwave target health {shockwaveHealthBefore}->{shockwaveTarget.Health} (expected25 damage); player health={world.State.Health} (expected65+20=85).");

            yield return new WaitForSecondsRealtime(1.35f);
            var stunned = !shockwaveTarget.IsWindingUp && world.State.Health == 85;
            var stunDeadline = Time.realtimeSinceStartupAsDouble + 3;
            while (!shockwaveTarget.IsWindingUp && Time.realtimeSinceStartupAsDouble < stunDeadline) yield return null;
            var recoveredFromStun = shockwaveTarget.IsWindingUp;
            Record("shockwave stun delays then restores enemy attack", stunned && recoveredFromStun,
                $"Nearby enemy could not wind up or damage player during first1.35s after cast={stunned}; windup resumed after stun={recoveredFromStun}.");

            // Arrange one extra charge to verify actual healing caps in a second successful shockwave.
            world.State.ReceiveEnemyAttack(11000, .05f, 20);
            yield return Press(Key.Digit2);
            Text(world.State.Prompt);
            yield return Frames(3);
            yield return Press(Key.Enter);
            Record("shockwave healing cap", world.State.LastCastOutcome == CastOutcome.Success && world.State.Health == 100,
                $"Second real slot2 cast started with85 health and ended at{world.State.Health}; expected cap100, not105.");

            yield return Press(Key.Digit1);
            var energyBeforeTimeout = world.State.Energy;
            Keys(Key.Digit1);
            yield return new WaitForSecondsRealtime(12.25f);
            Keys();
            Record("real-time QTE timeout", !world.State.IsTyping && world.State.LastCastOutcome == CastOutcome.Timeout && world.State.Energy == energyBeforeTimeout && world.PlayerBody.simulated,
                "Waited 12.25 real seconds while holding skill key; no auto-recast/refund; physics resumed.");

            yield return Reset();
            world.State.DamagePlayer(35);
            var pickup = world.SpawnHealthPickup(new Vector2(1, .65f));
            ArrangePlayer(new Vector2(1, .65f));
            yield return new WaitForSecondsRealtime(.15f);
            var collectedOnce = world.State.Health == 85 && (pickup == null || !pickup.activeSelf);
            yield return new WaitForSecondsRealtime(.15f);
            var stillOnce = world.State.Health == 85;
            world.SpawnHealthPickup(world.PlayerBody.position);
            yield return new WaitForSecondsRealtime(.15f);
            Record("actual health pickup trigger", collectedOnce && stillOnce && world.State.Health == 100,
                $"Damage arranged to65; actual trigger healed once to85={collectedOnce && stillOnce}; second capped at100={world.State.Health == 100}.");

            yield return Reset();
            ArrangePlayer(new Vector2(13, .65f));
            Keys(Key.D);
            yield return new WaitForSecondsRealtime(.45f);
            Keys();
            var gateBlocked = world.PlayerBody.position.x < world.GateCollider.bounds.min.x;
            ArrangePlayer(new Vector2(10, .65f));
            yield return Press(Key.E);
            var wrongRune = !world.State.PuzzleSolved;
            ArrangePlayer(new Vector2(7.5f, .65f));
            yield return Press(Key.E);
            ArrangePlayer(new Vector2(10, .65f));
            yield return Press(Key.E);
            var puzzleSolved = world.State.PuzzleSolved && world.GateCollider.enabled;
            // Killing via actual runtime damage path is an explicit arrangement for the progression test.
            world.Enemies[0].TakeDamage(1000);
            world.Enemies[1].TakeDamage(1000);
            yield return Frames(3);
            var gateOpen = !world.GateCollider.enabled && world.State.CanEnterBoss;
            Record("rune order, collision gate and progression", gateBlocked && wrongRune && puzzleSolved && gateOpen,
                $"Gate physically blocked={gateBlocked}; wrong-first rejected={wrongRune}; E left/right solved while gate stayed closed={puzzleSolved}; arranged enemy defeats opened gate={gateOpen}.");

            var boss = world.Enemies[2];
            ArrangePlayer(new Vector2(boss.transform.position.x - 1, .95f));
            var initialBossHealth = world.State.BossHealth;
            yield return Press(Key.J);
            var bossAttack = world.State.BossHealth == initialBossHealth - 20;
            boss.TakeDamage(1000);
            yield return Frames(3);
            var victory = world.State.HasWon;
            report.screenshot = Path.ChangeExtension(reportPath, ".png");
            yield return new WaitForEndOfFrame();
            CaptureRenderedFrame(report.screenshot, "rendered victory frame");
            yield return Frames(5);
            yield return Press(Key.R);
            var victoryReset = world.State.Health == 100 && world.State.BossHealth == 120 && !world.State.HasWon && world.State.RemainingEnemies == 2;
            Record("boss damage, victory and keyboard retry", bossAttack && victory && victoryReset,
                $"Actual J damaged boss={bossAttack}; final damage explicitly arranged; victory={victory}; R reset world={victoryReset}.");

            world.State.DamagePlayer(1000);
            yield return Frames(2);
            var death = world.State.IsDead && !world.PlayerBody.simulated;
            yield return Press(Key.R);
            Record("death and keyboard retry", death && !world.State.IsDead && world.State.Health == 100 && world.State.Energy == 0 && !world.State.PuzzleSolved,
                $"Fatal damage explicitly arranged; death froze physics={death}; R restored fresh state.");
            Record("rendered screenshots", File.Exists(report.screenshot) && File.Exists(report.typingScreenshot),
                "Captured actual rendered standalone victory and typing screens after EndOfFrame.");
            Finish();
        }

        private void Finish()
        {
            if (finished) return;
            finished = true;
            Application.logMessageReceived -= OnLog;
            StopAllCoroutines();
            report.renderedFrames = Time.frameCount - startedFrame;
            report.elapsedSeconds = (float)(Time.realtimeSinceStartupAsDouble - started);
            report.meanObservedFps = report.renderedFrames / Mathf.Max(.001f, report.elapsedSeconds);
            report.passed = report.cases.Count > 0 && report.cases.TrueForAll(item => item.passed);
            if (virtualKeyboard != null && virtualKeyboard.added) InputSystem.RemoveDevice(virtualKeyboard);
            if (previousKeyboard != null && previousKeyboard.added) previousKeyboard.MakeCurrent();
            File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
            Debug.Log("VALIDATION_SMOKE " + (report.passed ? "PASS " : "FAIL ") + reportPath);
            Application.Quit(report.passed ? 0 : 1);
        }
    }
}
