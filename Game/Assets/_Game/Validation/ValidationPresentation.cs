using UnityEngine;
using UnityEngine.UI;

namespace KeyboardWarrior.Validation
{
    /// <summary>Original generated greybox visuals/audio; no downloaded assets required.</summary>
    internal sealed class ValidationPresentation
    {
        private readonly Text status;
        private readonly Text instruction;
        private readonly Text overlay;
        private readonly AudioSource effects;
        private readonly AudioSource music;
        private readonly AudioClip hit;
        private readonly AudioClip parry;
        private readonly AudioClip cast;
        private readonly AudioClip pickup;
        private readonly AudioClip soundtrack;

        internal ValidationPresentation(Transform root)
        {
            var canvasObject = new GameObject("Validation HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(root);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = .5f;
            var panel = new GameObject("HUD background", typeof(Image));
            panel.transform.SetParent(canvasObject.transform, false);
            panel.GetComponent<Image>().color = new Color(.035f, .045f, .08f, .94f);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(.5f, 1);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0, 142);
            status = MakeText(canvasObject.transform, "Status", 25, TextAnchor.UpperLeft,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(22, -16), new Vector2(-44, 88));
            instruction = MakeText(canvasObject.transform, "Controls", 18, TextAnchor.UpperLeft,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(22, -100), new Vector2(-44, 46));
            overlay = MakeText(canvasObject.transform, "Casting and outcome", 26, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-90, -230));

            var audioObject = new GameObject("Original generated audio", typeof(AudioSource));
            audioObject.transform.SetParent(root);
            effects = audioObject.GetComponent<AudioSource>();
            effects.volume = .3f;
            music = audioObject.AddComponent<AudioSource>();
            music.volume = .06f;
            hit = Tone("Validation impact", 130, .10f);
            parry = Tone("Validation parry", 880, .16f);
            cast = Tone("Validation smite", 520, .32f);
            pickup = Tone("Validation health pickup", 660, .18f);
            soundtrack = MakeMusic();
            music.clip = soundtrack;
            music.loop = true;
            music.Play();
        }

        private static Text MakeText(Transform parent, string name, int size, TextAnchor alignment,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 dimensions)
        {
            var obj = new GameObject(name, typeof(Text));
            obj.transform.SetParent(parent, false);
            var text = obj.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.color = new Color(.9f, .95f, 1);
            text.supportRichText = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            var rect = text.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin == Vector2.up ? Vector2.up : new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;
            return text;
        }

        internal void Refresh(ValidationGameState state, string message, int puzzleStage)
        {
            status.text = "KEYBOARD WARRIOR | Unity validation scene\n" +
                $"Health {state.Health}/100    Smite {state.Energy}/3    Enemies {state.RemainingEnemies}/2    " +
                $"Runes {(state.PuzzleSolved ? "solved" : puzzleStage + "/2")}    Boss {state.BossHealth}/120\n" + message;
            instruction.text = "A/D or arrows: move   Space: jump   J: attack   K: timed parry   1/2: Smite   E: rune\n" +
                "Escape: pause/resume   R: retry after defeat/victory   F10: exit";

            if (state.IsPaused)
                overlay.text = "PAUSED\nReturn to this window and press Escape if needed.";
            else if (state.IsDead)
                overlay.text = "DEFEATED\nPress R to retry the whole validation level.";
            else if (state.HasWon)
                overlay.text = "BOSS DEFEATED\nValidation level complete. Press R to play again.";
            else if (state.IsTyping)
                overlay.text = $"SMITE {state.SelectedSmite} — {state.TimeRemaining:0.0}s\n" +
                    "Type this exact sentence, then press Enter:\n\n" + state.Prompt + "\n\n" +
                    state.TypedText + "_\n\nBackspace fixes mistakes. The fight is frozen.";
            else overlay.text = string.Empty;
            if (state.IsPaused) music.Pause(); else music.UnPause();
        }

        internal void Play(AttackOutcome outcome)
        {
            if (outcome == AttackOutcome.Parried) effects.PlayOneShot(parry);
            if (outcome == AttackOutcome.Damaged) effects.PlayOneShot(hit);
        }
        internal void PlayCast() { effects.PlayOneShot(cast); }
        internal void PlayHit() { effects.PlayOneShot(hit); }
        internal void PlayPickup() { effects.PlayOneShot(pickup); }

        internal void Dispose()
        {
            Object.Destroy(hit); Object.Destroy(parry); Object.Destroy(cast);
            Object.Destroy(pickup); Object.Destroy(soundtrack);
        }

        private static AudioClip Tone(string name, float frequency, float seconds)
        {
            const int rate = 22050;
            var data = new float[Mathf.CeilToInt(rate * seconds)];
            for (var i = 0; i < data.Length; i++)
            {
                var t = (float)i / rate;
                var envelope = Mathf.Min(1, t * 100) * (1 - (float)i / data.Length);
                data[i] = Mathf.Sin(t * frequency * Mathf.PI * 2) * envelope * .35f;
            }
            var clip = AudioClip.Create(name, data.Length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MakeMusic()
        {
            const int rate = 22050;
            var data = new float[rate * 4];
            var notes = new[] { 130.81f, 164.81f, 196f, 164.81f, 146.83f, 174.61f, 220f, 196f };
            for (var i = 0; i < data.Length; i++)
            {
                var t = (float)i / rate;
                var step = Mathf.FloorToInt(t * 2);
                var local = t * 2 - step;
                data[i] = Mathf.Sin(t * notes[step] * Mathf.PI * 2) * Mathf.Sin(local * Mathf.PI) * .3f;
            }
            var clip = AudioClip.Create("Original validation melody", data.Length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
