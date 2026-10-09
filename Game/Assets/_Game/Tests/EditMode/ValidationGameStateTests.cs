using NUnit.Framework;

namespace KeyboardWarrior.Validation.Tests
{
    public sealed class ValidationGameStateTests
    {
        private ValidationGameState state;

        [SetUp]
        public void SetUp()
        {
            state = new ValidationGameState();
            state.Reset();
        }

        private void EarnEnergy(int count = 1, int firstAttackId = 100)
        {
            for (int i = 0; i < count; i++)
                state.ReceiveEnemyAttack(firstAttackId + i, .05f, 20);
        }

        private void TypePrompt()
        {
            foreach (char character in state.Prompt)
                state.TypeCharacter(character);
        }

        private void OpenBossGate()
        {
            state.DefeatEnemy(1f);
            state.DefeatEnemy(1f);
            state.SolvePuzzle();
        }

        [Test]
        public void FreshRun_StartsAliveWithoutEnergyAndWithBossGateClosed()
        {
            Assert.That(state.Health, Is.EqualTo(100));
            Assert.That(state.Energy, Is.Zero);
            Assert.That(state.RemainingEnemies, Is.EqualTo(2));
            Assert.That(state.IsDead, Is.False);
            Assert.That(state.HasWon, Is.False);
            Assert.That(state.IsTyping, Is.False);
            Assert.That(state.CanEnterBoss, Is.False);
        }

        [TestCase(-.01f)]
        [TestCase(.19f)]
        [TestCase(2f)]
        public void AttackOutsideParryWindow_DamagesInsteadOfGrantingEnergy(float timeSincePress)
        {
            state.ReceiveEnemyAttack(1, timeSincePress, 20);
            Assert.That(state.Health, Is.EqualTo(80));
            Assert.That(state.Energy, Is.Zero);
        }

        [TestCase(0f)]
        [TestCase(.05f)]
        [TestCase(.17f)]
        public void AttackInsideParryWindow_PreventsDamageAndGrantsOneEnergy(float timeSincePress)
        {
            state.ReceiveEnemyAttack(1, timeSincePress, 20);
            Assert.That(state.Health, Is.EqualTo(100));
            Assert.That(state.Energy, Is.EqualTo(1));
        }

        [Test]
        public void RepeatedAttackId_CannotAwardEnergyOrDamageTwice()
        {
            state.ReceiveEnemyAttack(1, .05f, 20);
            state.ReceiveEnemyAttack(1, .05f, 20);
            state.ReceiveEnemyAttack(1, 2f, 20);
            Assert.That(state.Energy, Is.EqualTo(1));
            Assert.That(state.Health, Is.EqualTo(100));

            state.ReceiveEnemyAttack(2, 2f, 20);
            state.ReceiveEnemyAttack(2, 2f, 20);
            Assert.That(state.Health, Is.EqualTo(80));
        }

        [Test]
        public void DistinctParries_CannotOverfillEnergy()
        {
            EarnEnergy(20);
            Assert.That(state.Energy, Is.EqualTo(3));
        }

        [Test]
        public void CastWithoutEnergy_IsRejectedWithoutEnteringTyping()
        {
            Assert.That(state.TryStartCast(1), Is.False);
            Assert.That(state.IsTyping, Is.False);
            Assert.That(state.Energy, Is.Zero);
        }

        [TestCase(0)]
        [TestCase(3)]
        [TestCase(-1)]
        public void UnknownSkill_IsRejectedWithoutSpendingEnergy(int slot)
        {
            EarnEnergy();
            Assert.That(state.TryStartCast(slot), Is.False);
            Assert.That(state.Energy, Is.EqualTo(1));
            Assert.That(state.IsTyping, Is.False);
        }

        [Test]
        public void CastAttempt_SpendsImmediatelyOnlyOnce_EvenWhenRepeatedOrWrong()
        {
            EarnEnergy(3);
            Assert.That(state.TryStartCast(1), Is.True);
            Assert.That(state.Energy, Is.EqualTo(2), "Energy pays for the attempt, before typing succeeds.");
            Assert.That(state.TryStartCast(1), Is.False);
            Assert.That(state.TryStartCast(2), Is.False);
            state.TypeCharacter('x');
            Assert.That(state.SubmitCast(), Is.False);
            Assert.That(state.IsTyping, Is.True, "A wrong submission remains editable until time expires.");
            Assert.That(state.Energy, Is.EqualTo(2));
        }

        [TestCase(1)]
        [TestCase(2)]
        public void ExactPromptIncludingPunctuation_CanBeSubmittedOnce(int slot)
        {
            EarnEnergy(2);
            Assert.That(state.TryStartCast(slot), Is.True);
            TypePrompt();
            Assert.That(state.SubmitCast(), Is.True);
            Assert.That(state.IsTyping, Is.False);
            Assert.That(state.LastCastOutcome.ToString(), Is.EqualTo("Success"));
            Assert.That(state.Energy, Is.EqualTo(1));
            Assert.That(state.SubmitCast(), Is.False, "Enter repeat must not produce another successful cast.");
        }

        [Test]
        public void MistakeCanBeCorrectedWithBackspace_WithoutExtraEnergyCost()
        {
            EarnEnergy();
            state.TryStartCast(1);
            state.Backspace();
            Assert.That(state.TypedText, Is.Empty);
            state.TypeCharacter('x');
            state.Backspace();
            TypePrompt();
            Assert.That(state.SubmitCast(), Is.True);
            Assert.That(state.Energy, Is.Zero);
        }

        [Test]
        public void TimeoutConsumesAttemptAndCannotBeTurnedIntoLateSuccess()
        {
            EarnEnergy(2);
            state.TryStartCast(1);
            TypePrompt();
            state.Tick(12.1f);
            Assert.That(state.IsTyping, Is.False);
            Assert.That(state.LastCastOutcome.ToString(), Is.EqualTo("Timeout"));
            Assert.That(state.Energy, Is.EqualTo(1));
            Assert.That(state.SubmitCast(), Is.False);
            Assert.That(state.TryStartCast(2), Is.True);
            Assert.That(state.TypedText, Is.Empty, "The next attempt must not reuse an old answer.");
        }

        [Test]
        public void PauseFreezesCastDeadlineAndBlocksInputAndAttacks()
        {
            EarnEnergy();
            state.TryStartCast(1);
            float deadline = state.TimeRemaining;
            state.SetPaused(true);
            state.Tick(100f);
            state.TypeCharacter('x');
            state.ReceiveEnemyAttack(500, 2f, 30);
            Assert.That(state.TimeRemaining, Is.EqualTo(deadline));
            Assert.That(state.TypedText, Is.Empty);
            Assert.That(state.Health, Is.EqualTo(100));
            Assert.That(state.IsCombatActive, Is.False);
            state.SetPaused(false);
            state.Tick(1f);
            Assert.That(state.TimeRemaining, Is.EqualTo(deadline - 1f).Within(.001f));
        }

        [Test]
        public void FocusLossFreezesDeadlineUntilFocusReturns()
        {
            EarnEnergy();
            state.TryStartCast(1);
            float deadline = state.TimeRemaining;
            state.SetFocused(false);
            state.Tick(100f);
            state.TypeCharacter('x');
            Assert.That(state.TimeRemaining, Is.EqualTo(deadline));
            Assert.That(state.TypedText, Is.Empty);
            state.SetFocused(true);
            state.Tick(1f);
            Assert.That(state.TimeRemaining, Is.EqualTo(deadline - 1f).Within(.001f));
        }

        [Test]
        public void TypingIsIsolatedFromCombatAndCannotGainMoreEnergy()
        {
            EarnEnergy();
            state.TryStartCast(1);
            state.ReceiveEnemyAttack(500, .05f, 30);
            state.ReceiveEnemyAttack(501, 2f, 30);
            Assert.That(state.Health, Is.EqualTo(100));
            Assert.That(state.Energy, Is.Zero);
            Assert.That(state.IsCombatActive, Is.False);
        }

        [Test]
        public void HealthAndEnergyRemainBounded_DeadRunCannotCast()
        {
            state.DamagePlayer(20);
            state.Heal(1000);
            Assert.That(state.Health, Is.EqualTo(100));
            EarnEnergy(5);
            state.DamagePlayer(1000);
            Assert.That(state.Health, Is.Zero);
            Assert.That(state.IsDead, Is.True);
            Assert.That(state.TryStartCast(1), Is.False);
            Assert.That(state.Energy, Is.InRange(0, 3));
        }

        [TestCase(-.01f, false)]
        [TestCase(0f, true)]
        [TestCase(.49f, true)]
        [TestCase(.5f, false)]
        [TestCase(1f, false)]
        public void EnemyDropUsesAConsistentHalfChanceAndRequiresCollection(float roll, bool expectedDrop)
        {
            state.DamagePlayer(40);
            Assert.That(state.DefeatEnemy(roll), Is.EqualTo(expectedDrop));
            Assert.That(state.Health, Is.EqualTo(60), "Defeating an enemy awards a pickup, not immediate healing.");
            Assert.That(state.RemainingEnemies, Is.EqualTo(1));
        }

        [Test]
        public void ReturningFocusDoesNotCancelManualPause()
        {
            EarnEnergy();
            state.TryStartCast(1);
            state.SetPaused(true);
            state.SetFocused(false);
            state.SetFocused(true);
            state.Tick(20f);
            Assert.That(state.IsPaused, Is.True);
            Assert.That(state.IsTyping, Is.True);
            Assert.That(state.TimeRemaining, Is.EqualTo(12f));
        }

        [Test]
        public void ExactAnswerRequiresCaseSpacesAndPunctuation()
        {
            EarnEnergy();
            state.TryStartCast(1);
            foreach (char character in state.Prompt.ToLowerInvariant()) state.TypeCharacter(character);
            Assert.That(state.SubmitCast(), Is.False);
            while (state.TypedText.Length > 0) state.Backspace();
            foreach (char character in state.Prompt.TrimEnd('.')) state.TypeCharacter(character);
            Assert.That(state.SubmitCast(), Is.False);
            state.TypeCharacter('.');
            Assert.That(state.SubmitCast(), Is.True);
        }

        [Test]
        public void DefeatedEnemyCannotBeFarmedAfterAllEnemiesAreGone()
        {
            state.DamagePlayer(50);
            state.DefeatEnemy(1f);
            state.DefeatEnemy(1f);
            state.DefeatEnemy(0f);
            Assert.That(state.RemainingEnemies, Is.Zero);
            Assert.That(state.Health, Is.EqualTo(50));
        }

        [Test]
        public void BossRequiresBothPuzzleAndEnemies_AndDiesOnlyOnce()
        {
            int initialBossHealth = state.BossHealth;
            state.DamageBoss(1000);
            Assert.That(state.BossHealth, Is.EqualTo(initialBossHealth));
            state.SolvePuzzle();
            Assert.That(state.CanEnterBoss, Is.False);
            state.DefeatEnemy(1f);
            Assert.That(state.CanEnterBoss, Is.False);
            state.DefeatEnemy(1f);
            Assert.That(state.CanEnterBoss, Is.True);
            state.DamageBoss(1000);
            Assert.That(state.BossHealth, Is.Zero);
            Assert.That(state.HasWon, Is.True);
            state.DamageBoss(1000);
            Assert.That(state.BossHealth, Is.Zero);
        }

        [Test]
        public void EnemyClearAloneDoesNotOpenPuzzleGate()
        {
            state.DefeatEnemy(1f);
            state.DefeatEnemy(1f);
            Assert.That(state.CanEnterBoss, Is.False);
        }

        [Test]
        public void RetryRestoresFreshStateAndOldAttackIdsMayBeUsedInNewRun()
        {
            EarnEnergy();
            OpenBossGate();
            state.DamageBoss(1000);
            state.SetPaused(true);
            state.SetFocused(false);
            state.Reset();
            Assert.That(state.Health, Is.EqualTo(100));
            Assert.That(state.Energy, Is.Zero);
            Assert.That(state.RemainingEnemies, Is.EqualTo(2));
            Assert.That(state.PuzzleSolved, Is.False);
            Assert.That(state.HasWon, Is.False);
            Assert.That(state.IsDead, Is.False);
            Assert.That(state.IsTyping, Is.False);
            Assert.That(state.IsPaused, Is.False);
            Assert.That(state.CanEnterBoss, Is.False);
            Assert.That(state.TypedText, Is.Empty);
            Assert.That(state.BossHealth, Is.GreaterThan(0));
            state.ReceiveEnemyAttack(100, .05f, 20);
            Assert.That(state.Energy, Is.EqualTo(1), "Attack de-duplication must reset with the run.");
        }
    }
}
