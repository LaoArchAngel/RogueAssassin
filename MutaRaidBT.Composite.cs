// MutaRaid by fiftypence
// Now rewritten using BTs!
//
// SVN: http://fiftypence.googlecode.com/svn/trunk/
// 
// As always, if you wish to reuse code, please seek permission first 
// and provide credit where appropriate

using System.Linq;
using System.Drawing;
using Styx.Helpers;
using CommonBehaviors.Actions;
using TreeSharp;
using Action = TreeSharp.Action;

namespace MutaRaidBT
{
    public partial class MutaRaidBt
    {
        #region Constants

        private const int AOE_MIN_TARGETS = 4;

        #region Spellnames

        /// <summary>
        /// Backstab
        /// </summary>
        private const string BS = "Backstab";

        /// <summary>
        /// Cold Blood
        /// </summary>
        private const string CB = "Cold Blood";

        private const string ENVENOM = "Envenom";

        /// <summary>
        /// Fan of Knives
        /// </summary>
        private const string FOK = "Fan of Knives";

        private const string GARROTE = "Garrote";
        private const string MUTILATE = "Mutilate";
        private const string OVERKILL = "Overkill";
        private const string REDIRECT = "Redirect";
        private const string RUPTURE = "Rupture";

        /// <summary>
        /// Slice and Dice
        /// </summary>
        private const string SND = "Slice and Dice";

        /// <summary>
        /// Tricks of the Trade
        /// </summary>
        private const string TOTT = "Tricks of the Trade";

        private const string VANISH = "Vanish";
        private const string VENDETTA = "Vendetta";

        #endregion

        #region Energy Costs

        private const int EC_BS = 30;
        private const int EC_MUTILATE = 55;
        private const int EC_RUPTURE = 25;
        private const int EC_ENVENOM = 35;

        #endregion

        #endregion

        private static Composite BuildNoneCombatBehavior()
        {
            return new Action(ret => Logging.Write(Color.Orange, "Low level combat is unsupported."));
        }

        private Composite BuildAssassinationCombatBehavior()
        {
            return new Decorator(ret => !Me.Mounted && Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange,
                                 new PrioritySelector(
                                     new Action(delegate
                                                    {
                                                        ResetVariables();
                                                        UpdateEnergy();
                                                        SetFocus();
                                                        return RunStatus.Failure;
                                                    }),
                                     CastSpell(GARROTE, ret => Me.IsStealthed),
                                     CastSpell(REDIRECT, ret => Me.ComboPoints < Me.RawComboPoints),
                                     CastSpell(FOK, ret => EnemyUnits.Count() >= AOE_MIN_TARGETS && ShouldWeAoe() && !IsTargetBoss()),
                                     CastSpell(SND, ret => !HasSnd && Me.ComboPoints > 0),
                                     /* Never let SND down.  SND > Rupture.  CB has its own validation, so if CB
									 * validation passed, we know this should be the next move.
									 * Finally, we should never have more than 105 energy (pooling ends @ 90 for all
									 * cp-gen moves). If we've hit 105, we run the risk of capping, which is one of
									 * the worst DPS hits we can take (~9k/s).  Also, do not use cp-gens
									 * (ie, waste cps) as an energy-dump.  It's better to clip a second of Envenom.
									 * http://elitistjerks.com/f78/t110134-assassination_guide_cata_06_28_2011_a/#Combat_and_Rotation
									 */
                                     CastSpell(ENVENOM,
                                               ret => Me.ComboPoints > 0 && (SndBuff < 2 || Me.GetAuraByName(CB) != null)),
                                     /* Only reason Rupture should fall is because SnD was critical.
									 * Leaving Rupture down for 1s = ~3.3k dps loss.
									 * Using 1 cp in Rupture vs 5 = ~1.8k dps loss.
									 * Pound for pound, if Rupture has 1s left, refresh regardless of CP.
									 */
                                     CastSpell(RUPTURE,
                                               ret =>
                                               Me.ComboPoints > 0 &&
                                               (RuptureDebuff == 0 || (PoolRupture() && RuptureDebuff < 2))),
                                     CastSpellOnFocus(TOTT, ret => FocusReadyForTricks()),
                                     new Decorator(ret => IsTargetBoss(), BossFight()),
                                     new Decorator(ret => Me.CurrentTarget.HealthPercent < 35 && BehindTarget(),
                                                   BackstabRotation()),
                                     MutilateRotation()));
        }

        private static Composite BuildCombatCombatBehavior()
        {
            return new Action(ret => Logging.Write(Color.Orange, "Combat behavior has not yet been implemented."));
        }

        private static Composite BuildSubtletyCombatBehavior()
        {
            return new Action(ret => Logging.Write(Color.Orange, "Subtlety combat is unsupported."));
        }

        #region Specialized

        private PrioritySelector BackstabRotation()
        {
            return new PrioritySelector(
                CastSpell(ENVENOM,
                          ret => Me.ComboPoints == 5 && ((!HasEnvenom && _currentEnergy >= 90) || _currentEnergy > 105)
                                 && !PoolRupture()),
                CastSpell(BS, ret => Me.ComboPoints != 5 && PooledFinisher(EC_BS))
                );
        }

        private PrioritySelector BossFight()
        {
            return new PrioritySelector(
                CastCooldown(VENDETTA, ret => RuptureDebuff > 0 && HasSnd),
                new Decorator(
                    ret => CanCastSpell(VANISH) && Me.GetAuraByName(OVERKILL) == null && !HasEnvenom && _currentEnergy > 45,
                    new Sequence(CastCooldown(VANISH), new WaitContinue(1, ret => false, new ActionAlwaysSucceed()),
                                 CastSpell(GARROTE))),
                CastCooldown(CB, ret => Me.ComboPoints == 5 && _currentEnergy >= 65 && _currentEnergy < 95));
        }

        private PrioritySelector MutilateRotation()
        {
            return new PrioritySelector(
                CastSpell(ENVENOM,
                          ret => Me.ComboPoints >= 4 && ((!HasEnvenom && _currentEnergy >= 90) || _currentEnergy > 105)
                                 && !PoolRupture()),
                CastSpell(MUTILATE, ret => Me.ComboPoints < 4 && PooledFinisher(EC_MUTILATE))
                );
        }

        #endregion
    }
}