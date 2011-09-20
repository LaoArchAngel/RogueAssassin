// MutaRaid by fiftypence
// Now rewritten using BTs!
//
// SVN: http://fiftypence.googlecode.com/svn/trunk/
// 
// As always, if you wish to reuse code, please seek permission first 
// and provide credit where appropriate

using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using Styx;
using Styx.Helpers;
using Styx.Logic.Combat;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using TreeSharp;
using Action = TreeSharp.Action;

namespace RogueAssassin
{
    public partial class RogueAssassin
    {
        private const int AOE_RANGE = 12;

        #region Fields

        private uint _currentEnergy;
        private double? _envenomBuff;
        private WoWPlayer _focusTarget;
        private bool? _hasEnvenom;
        private bool? _hasSnd;
        private double? _ruptureDebuff;
        private double? _sndBuff;
        private static IEnumerable<WoWUnit> _nearbyEnemies;

        #endregion

        #region Properties

        private double EnvenomBuff
        {
            get
            {
                if (!_envenomBuff.HasValue)
                {
                    var env = Me.GetAuraByName(ENVENOM);
                    _envenomBuff = env == null ? 0 : env.TimeLeft.TotalSeconds;
                }

                return _envenomBuff.Value;
            }
        }

        private bool HasEnvenom
        {
            get
            {
                if (!_hasEnvenom.HasValue) _hasEnvenom = EnvenomBuff > 0;

                return _hasEnvenom.Value;
            }
        }

        private bool HasSnd
        {
            get
            {
                if (!_hasSnd.HasValue) _hasSnd = SndBuff > 0;

                return _hasSnd.Value;
            }
        }

        private double RuptureDebuff
        {
            get
            {
                if (!_ruptureDebuff.HasValue)
                {
                    var rup = Me.CurrentTarget.GetAuraByName(RUPTURE);
                    _ruptureDebuff = rup == null ? 0 : rup.TimeLeft.TotalSeconds;
                }

                return _ruptureDebuff.Value;
            }
        }

        private double SndBuff
        {
            get
            {
                if (!_sndBuff.HasValue)
                {
                    var snd = Me.GetAuraByName(SND);
                    _sndBuff = snd == null ? 0 : snd.TimeLeft.TotalSeconds;
                }

                return _sndBuff.Value;
            }
        }

        #endregion

        #region Helpers

        //******************************************************************************************
        //* The following helpers may be reused at will, as long as credit is given to fiftypence. *
        //******************************************************************************************

        #region Non-BT Helpers

        /// <summary>
        /// List of nearby enemy units that pass certain criteria, this list should only return units 
        /// in active combat with the player, the player's party, or the player's raid.
        /// </summary>
        private static IEnumerable<WoWUnit> EnemyUnits
        {
            get
            {
                return _nearbyEnemies ?? (_nearbyEnemies =
                                          ObjectManager.GetObjectsOfType<WoWUnit>(true, false).Where(
                                              unit => !unit.IsFriendly
                                                      &&
                                                      (unit.IsTargetingMeOrPet || unit.IsTargetingMyPartyMember ||
                                                       unit.IsTargetingMyRaidMember
                                                       || unit.IsPlayer)
                                                      && !unit.IsNonCombatPet && !unit.IsCritter &&
                                                      unit.Distance2D <= AOE_RANGE).ToList());
            }
        }

        /// <summary>
        /// Uses Lua to update current energy resource of the player. This is used
        /// to fix the slow updating of ObjectManager.Me.CurrentEnergy.
        /// </summary>
        private void UpdateEnergy()
        {
            _currentEnergy = Me.CurrentEnergy;
        }

        /// <summary>
        /// Uses Lua to find the GUID of the player's focus target then updates the 
        /// global WoWPlayer FocusTarget to the new unit if appropriate. 
        /// </summary>
        private void SetFocus()
        {
            var focusGuid =
                Lua.GetReturnVal<string>(
                    "local GUID = UnitGUID(\"focus\"); if GUID == nil then return 0 else return GUID end", 0);

            if (focusGuid == Convert.ToString(0))
            {
                if (_focusTarget != null)
                {
                    _focusTarget = null;

                    Logging.Write(Color.Orange, "Focus dropped -- clearing focus target.");
                }

                return;
            }

            // Remove the two starting characters (0x) from the GUID returned by lua using substring.
            // This is done so we can convert the string to a ulong.
            // Then we should set the WoWPlayer Focus to the unit which belongs to our formatted GUID.

            var focus =
                ObjectManager.GetAnyObjectByGuid<WoWPlayer>(ulong.Parse(focusGuid.Substring(2),
                                                                        System.Globalization.NumberStyles.
                                                                            AllowHexSpecifier));

            if (_focusTarget != focus)
                Logging.Write(Color.Orange, "Setting " + focus.Name + " as focus target.");

            _focusTarget = focus;
        }

        /// <summary>
        /// Use several 'checks' to determine whether or not we're behind the target, as none of them are too reliable.
        /// Made !IsFacing(Me) first, since I've read that's the "safest";
        /// </summary>
        private static bool BehindTarget()
        {
            return !Me.CurrentTarget.IsFacing(Me) || Me.CurrentTarget.IsPlayerBehind || Me.CurrentTarget.MeIsBehind ||
                   Me.CurrentTarget.MeIsSafelyBehind ||
                   WoWMathHelper.IsBehind(Me.Location, Me.CurrentTarget.Location, Me.CurrentTarget.Rotation);
        }

        /// <summary>
        /// Checks to see if specified target is under the effect of crowd control
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns></returns>
        private static bool IsCrowdControlled(WoWUnit target)
        {
            // Just want to throw a shout-out to Singular for this function.
            return
                target.GetAllAuras().Any(
                    unit =>
                    unit.Spell.Mechanic == WoWSpellMechanic.Banished || unit.Spell.Mechanic == WoWSpellMechanic.Charmed
                    || unit.Spell.Mechanic == WoWSpellMechanic.Horrified ||
                    unit.Spell.Mechanic == WoWSpellMechanic.Incapacitated
                    || unit.Spell.Mechanic == WoWSpellMechanic.Polymorphed ||
                    unit.Spell.Mechanic == WoWSpellMechanic.Sapped
                    || unit.Spell.Mechanic == WoWSpellMechanic.Shackled ||
                    unit.Spell.Mechanic == WoWSpellMechanic.Asleep
                    || unit.Spell.Mechanic == WoWSpellMechanic.Frozen);
        }

        /// <summary>
        /// Checks to see if any nearby units are under breakable crowd control.
        /// </summary>
        private static bool ShouldWeAoe()
        {
            return !EnemyUnits.Any(IsCrowdControlled);
        }

        /// <summary>
        /// Gets the cooldown of specified spell using Lua
        /// </summary>
        /// <param name="spellName">Specified Spell</param>
        /// <returns>Spell cooldown</returns>
        public double SpellCooldown(string spellName)
        {
            return
                double.Parse(
                    Lua.GetReturnValues(
                        string.Format(
                            "local start, duration = GetSpellCooldown(\"{0}\"); " +
                            "return (start+duration) - GetTime()",
                            spellName))[0]);
        }

        /// <summary>
        /// Check if player's target is a boss using Lua.
        /// </summary>
        private static bool IsTargetBoss()
        {
            if (!Me.IsInRaid)
            {
                return (Me.CurrentTarget.CreatureRank == WoWUnitClassificationType.WorldBoss) ||
                       ((Me.CurrentTarget.Level == 87 && Me.CurrentTarget.Elite) || (Me.CurrentTarget.Level > 87));
            }

            return (Me.CurrentTarget.CreatureRank == WoWUnitClassificationType.WorldBoss) ||
                   (Me.CurrentTarget != null && Me.CurrentTarget.Level > 87 && Me.CurrentTarget.Elite);
        }

        /// <summary>
        /// Returns true if a spell is not on cooldown or if
        /// there is 0.3 or less left on GCD, AND the spell is known
        /// </summary>
        private static bool CanCastSpell(string spellName)
        {
            return SpellManager.CanCast(SpellManager.Spells[spellName]);
        }

        #endregion

        #region BT Helpers

        #region Cast Spell

        /// <summary>
        /// Checks if specified spell is castable, if so casts it and writes to log.
        /// </summary>
        /// <param name="spellName">Spell name</param>
        /// <returns></returns>
        public Composite CastSpell(string spellName)
        {
            return new Decorator(ret => Me.CurrentTarget != null && CanCastSpell(spellName) && SpellManager.Cast(spellName),
                                 new Action(delegate
                                                {
                                                    Logging.Write(Color.LightBlue,
                                                                  string.Format("[{0}] [{1}] {2}", _currentEnergy,
                                                                                Me.ComboPoints, spellName));
                                                }));
        }

        /// <summary>
        /// Checks if specified spell is castable, if so casts it and writes to log.
        /// Uses specified conditions.
        /// </summary>
        /// <param name="spellName">Spell name</param>
        /// <param name="conditions">Specified conditions</param>
        /// <returns></returns>
        public Composite CastSpell(string spellName, CanRunDecoratorDelegate conditions)
        {
            return new Decorator(ret => conditions(ret) && CanCastSpell(spellName) && SpellManager.Cast(spellName),
                                 new Action(delegate
                                                {
                                                    Logging.Write(Color.LightBlue,
                                                                  string.Format("[{0}] [{1}] {2}", _currentEnergy,
                                                                                Me.ComboPoints, spellName));
                                                }));
        }

        #endregion

        #region Cast Focus

        /// <summary>
        /// Checks if specified spell is castable, if so casts it and writes to log.
        /// Casts on focus
        /// </summary>
        /// <param name="spellName">Spell name</param>
        /// <returns></returns>
        public Composite CastSpellOnFocus(string spellName)
        {
            return new Decorator(ret => _focusTarget != null && SpellManager.CanCast(spellName, _focusTarget) && SpellManager.Cast(spellName, _focusTarget),
                                 new Action(delegate
                                                {
                                                    Logging.Write(Color.Yellow, spellName);
                                                }));
        }

        /// <summary>
        /// Checks if specified spell is castable, if so casts it and writes to log.
        /// Casts on focus, uses specified conditions
        /// </summary>
        /// <param name="spellName">Spell name</param>
        /// <param name="conditions">Specified conditions</param>
        /// <returns></returns>
        public Composite CastSpellOnFocus(string spellName, CanRunDecoratorDelegate conditions)
        {
            return new Decorator(ret => _focusTarget != null && conditions(ret) && SpellManager.CanCast(spellName, _focusTarget) && SpellManager.Cast(spellName, _focusTarget),
                                 new Action(delegate
                                                {
                                                    Logging.Write(Color.Yellow, spellName);
                                                }));
        }

        #endregion

        #region Cast Cooldowns

        /// <summary>
        /// Checks if specified cooldown is castable, if so casts it and writes to log.
        /// Casts with Lua to make use of the ability queue.
        /// </summary>
        /// <param name="cooldownName">Cooldown name</param>
        /// <returns></returns>
        public Composite CastCooldown(string cooldownName)
        {
            return new Decorator(ret => CanCastSpell(cooldownName) && SpellManager.Cast(cooldownName),
                                 new Action(delegate { Logging.Write(Color.Yellow, cooldownName); }));
        }

        /// <summary>
        /// Checks if specified cooldown is castable, if so casts it and writes to log.
        /// Uses specified conditions.
        /// Casts with Lua to make use of the ability queue.
        /// </summary>
        /// <param name="cooldownName">Cooldown name</param>
        /// <param name="conditions">Specified conditions</param>
        /// <returns></returns>
        public Composite CastCooldown(string cooldownName, CanRunDecoratorDelegate conditions)
        {
            return new Decorator(
                ret => conditions(ret) && CanCastSpell(cooldownName) && SpellManager.Cast(cooldownName),
                new Action(delegate { Logging.Write(Color.Yellow, cooldownName); }));
        }

        #endregion

        /// <summary>
        /// Checks if player is auto-attacking, and if not toggles auto-attack.
        /// </summary>
        /// <returns></returns>
        public Composite AutoAttack()
        {
            return new Decorator(ret => !Me.IsAutoAttacking, new Action(delegate
                                                                            {
                                                                                Me.ToggleAttack();
                                                                                Logging.Write(Color.Orange,
                                                                                              "Auto-attack");
                                                                            }));
        }

        /// <summary>
        /// Checks if player is auto-attacking, and if not toggles auto-attack.
        /// Uses specified conditions.
        /// </summary>
        /// <returns></returns>
        public Composite AutoAttack(CanRunDecoratorDelegate conditions)
        {
            return new Decorator(ret => conditions(ret) && !Me.IsAutoAttacking, new Action(delegate
                                                                                               {
                                                                                                   Me.ToggleAttack();
                                                                                                   Logging.Write(
                                                                                                       Color.Orange,
                                                                                                       "Auto-attack");
                                                                                               }));
        }

        #endregion

        #region Rogue Helpers

        /// <summary>
        /// Determines if we have pooled enough energy to cast a Combo Point Generator and a Finisher.
        /// This is important because we do not want to cast a generator if we won't have enough energy to cast a
        /// finisher before a buff runs out.
        /// </summary>
        /// <param name="cost">Cost of our generator.</param>
        /// <returns>True if we've pooled enough energy, or else there's another reason we should not pool.</returns>
        private bool PooledFinisher(int cost)
        {
            // Do not pool if we have no combo points.
            if (Me.ComboPoints == 0) return true;

            // Determine if rupture is our next finisher.  We determine that by ensuring that we have enough time to
            // cast at least rupture, mutilate, and envenom with our current energy pool before SnD goes out.  Also, 
            // rupture must have less time than SnD.
            var ruptureNext = SndBuff > RuptureDebuff &&
                              (EC_RUPTURE + EC_MUTILATE + EC_ENVENOM - _currentEnergy)/10d > SndBuff;

            if (ruptureNext)
            {
                if (EC_RUPTURE + cost < _currentEnergy) return true;
                if (EnvenomBuff + (EC_RUPTURE/10d) < RuptureDebuff) return true;

                // EC_RUPTURE + cost - _currentEnergy = Deficit from casting both a combo point and finisher.
                // ((RuptureDebuff - 2) * 10) = energy that can be pooled before Rupture is 2 secs from falling off.
                return (EC_RUPTURE + cost - _currentEnergy - ((RuptureDebuff - 2)*10) <= 0);
            }

            return EC_ENVENOM + cost <= _currentEnergy;
        }

        /// <summary>
        /// Returns true if we want to pool for rupture.
        /// </summary>
        /// <returns>True if we're pooling for Rupture.</returns>
        private bool PoolRupture()
        {
            if (SndBuff < RuptureDebuff) return false;

            var timeToCap = 12 - (_currentEnergy/10d);

            return timeToCap > (RuptureDebuff - 2);
        }

        /// <summary>
        /// Contains the logic on whether our current target is ready for Tricks of the Trade.
        /// </summary>
        /// <returns><c>true</c> if and only if all conditions are met for the focus target to receive ToT</returns>
        private bool FocusReadyForTricks()
        {
            if (_focusTarget.Distance2D > 100)
            {
                Logging.WriteDebug(Color.Yellow, "Focus too far");
                return false;
            }

            if (!_focusTarget.InLineOfSight)
            {
                Logging.WriteDebug(Color.Yellow, "Focus not in LOS");
                return false;
            }

            if (_focusTarget.Dead)
            {
                Logging.WriteDebug(Color.Yellow, "Focus is dead.");
                return false;
            }

            if (!_focusTarget.IsInMyPartyOrRaid)
            {
                Logging.WriteDebug(Color.Yellow, "Focus not in party or raid");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Resets all variables that must be set every time we run.
        /// These variables are kept for a single execution to ensure that we don't request the same data multiple
        /// time.
        /// </summary>
        private void ResetVariables()
        {
            _envenomBuff = null;
            _hasEnvenom = null;
            _hasSnd = null;
            _nearbyEnemies = null;
            _ruptureDebuff = null;
            _sndBuff = null;
        }

        #endregion

        #endregion
    }
}