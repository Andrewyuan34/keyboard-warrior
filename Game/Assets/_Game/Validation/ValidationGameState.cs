using System;
using System.Collections.Generic;

namespace KeyboardWarrior.Validation
{
    public enum AttackOutcome { Ignored, Parried, Damaged }
    public enum CastOutcome { None, Success, Timeout }

    /// <summary>Deterministic rules shared by the playable validation scene and tests.</summary>
    public sealed class ValidationGameState
    {
        public const int MaxHealth = 100;
        public const int MaxEnergy = 3;
        public const int StartingEnemies = 2;
        public const int MaxBossHealth = 120;
        public const float ParryWindow = .18f;
        public const float CastDuration = 12f;

        private readonly HashSet<int> resolvedAttacks = new HashSet<int>();
        private bool manuallyPaused;
        private bool focused = true;

        public int Health { get; private set; }
        public int Energy { get; private set; }
        public int RemainingEnemies { get; private set; }
        public int BossHealth { get; private set; }
        public bool PuzzleSolved { get; private set; }
        public bool HasWon => BossHealth == 0;
        public bool IsDead => Health == 0;
        public bool IsTyping { get; private set; }
        public bool IsPaused => manuallyPaused || !focused;
        public bool IsCombatActive => !IsPaused && !IsTyping && !IsDead && !HasWon;
        public bool CanEnterBoss => RemainingEnemies == 0 && PuzzleSolved;
        public string TypedText { get; private set; }
        public string Prompt { get; private set; }
        public float TimeRemaining { get; private set; }
        public int SelectedSmite { get; private set; }
        public int CastSuccessVersion { get; private set; }
        public CastOutcome LastCastOutcome { get; private set; }

        public ValidationGameState() { Reset(); }

        public void Reset()
        {
            Health = MaxHealth;
            Energy = 0;
            RemainingEnemies = StartingEnemies;
            BossHealth = MaxBossHealth;
            PuzzleSolved = false;
            manuallyPaused = false;
            focused = true;
            IsTyping = false;
            TypedText = string.Empty;
            Prompt = string.Empty;
            TimeRemaining = 0;
            SelectedSmite = 0;
            CastSuccessVersion = 0;
            LastCastOutcome = CastOutcome.None;
            resolvedAttacks.Clear();
        }

        public AttackOutcome ReceiveEnemyAttack(int attackId, float secondsSinceParryPress, int damage)
        {
            if (!IsCombatActive || !resolvedAttacks.Add(attackId)) return AttackOutcome.Ignored;
            if (secondsSinceParryPress >= 0 && secondsSinceParryPress <= ParryWindow)
            {
                Energy = Math.Min(MaxEnergy, Energy + 1);
                return AttackOutcome.Parried;
            }
            DamagePlayer(damage);
            return AttackOutcome.Damaged;
        }

        public void DamagePlayer(int amount)
        {
            if (!IsCombatActive || amount <= 0) return;
            Health = Math.Max(0, Health - amount);
        }

        public void Heal(int amount)
        {
            if (IsDead || HasWon || amount <= 0) return;
            Health = Math.Min(MaxHealth, Health + amount);
        }

        public bool TryStartCast(int slot)
        {
            if (!IsCombatActive || Energy < 1 || (slot != 1 && slot != 2)) return false;
            Energy--;
            SelectedSmite = slot;
            Prompt = slot == 1 ? "Your reign ends." : "Face my words!";
            TypedText = string.Empty;
            TimeRemaining = CastDuration;
            LastCastOutcome = CastOutcome.None;
            IsTyping = true;
            return true;
        }

        public void TypeCharacter(char character)
        {
            if (!IsTyping || IsPaused || char.IsControl(character) || TypedText.Length >= 100) return;
            TypedText += character;
        }

        public void Backspace()
        {
            if (IsTyping && !IsPaused && TypedText.Length > 0)
                TypedText = TypedText.Substring(0, TypedText.Length - 1);
        }

        public bool SubmitCast()
        {
            if (!IsTyping || IsPaused || !string.Equals(TypedText, Prompt, StringComparison.Ordinal)) return false;
            IsTyping = false;
            TimeRemaining = 0;
            LastCastOutcome = CastOutcome.Success;
            CastSuccessVersion++;
            return true;
        }

        public void Tick(float unscaledDelta)
        {
            if (!IsTyping || IsPaused || unscaledDelta <= 0 || float.IsNaN(unscaledDelta) || float.IsInfinity(unscaledDelta)) return;
            TimeRemaining = Math.Max(0, TimeRemaining - unscaledDelta);
            if (TimeRemaining > 0) return;
            IsTyping = false;
            LastCastOutcome = CastOutcome.Timeout;
        }

        public void SetPaused(bool value) { manuallyPaused = value; }
        public void SetFocused(bool value) { focused = value; }

        /// <summary>Returns whether a health drop is awarded; a deterministic roll makes both paths testable.</summary>
        public bool DefeatEnemy(float dropRoll)
        {
            if (!IsCombatActive || RemainingEnemies <= 0) return false;
            RemainingEnemies--;
            return dropRoll >= 0 && dropRoll < .5f;
        }

        public void SolvePuzzle()
        {
            if (IsCombatActive) PuzzleSolved = true;
        }

        public bool DamageBoss(int amount)
        {
            if (!IsCombatActive || !CanEnterBoss || amount <= 0) return false;
            BossHealth = Math.Max(0, BossHealth - amount);
            return true;
        }
    }
}
