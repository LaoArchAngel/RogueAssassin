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
using System.Diagnostics;
using System.Threading;
using System.Drawing;

using Styx;
using Styx.Combat.CombatRoutine;
using Styx.Helpers;
using Styx.Logic;
using Styx.Logic.Combat;
using Styx.Logic.Pathing;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

using Doctrine.Talents;

using CommonBehaviors.Actions;
using TreeSharp;
using Action = TreeSharp.Action;

namespace MutaRaidBT
{
    public partial class MutaRaidBT : CombatRoutine
    {
        #region Variables

        private int CurrentEnergy = 0;
        private WoWPlayer FocusTarget = null;

        #endregion

        #region Helpers

        //******************************************************************************************
        //* The following helpers may be reused at will, as long as credit is given to fiftypence. *
        //******************************************************************************************

        #region Non-BT Helpers

        /// <summary>
        ///     List of nearby enemy units that pass certain criteria, this list should only return units 
        ///     in active combat with the player, the player's party, or the player's raid.
        /// </summary>

        private List<WoWUnit> EnemyUnits
        {
            get
            {
                return
                    ObjectManager.GetObjectsOfType<WoWUnit>(true, false)
                    .Where(unit =>
                        !unit.IsFriendly
                        && (unit.IsTargetingMeOrPet
                           || unit.IsTargetingMyPartyMember
                           || unit.IsTargetingMyRaidMember
                           || unit.IsPlayer)
                        && !unit.IsNonCombatPet
                        && !unit.IsCritter
                        && unit.Distance2D
                     <= 12).ToList();
            }
        }

        /// <summary>
        ///     Uses Lua to update current energy resource of the player. This is used
        ///     to fix the slow updating of ObjectManager.Me.CurrentEnergy.
        /// </summary>

        private void UpdateEnergy()
        {
            CurrentEnergy = Lua.GetReturnVal<int>("return UnitMana(\"player\");", 0);
        }

        /// <summary>
        ///     Uses Lua to find the GUID of the player's focus target then updates the 
        ///     global WoWPlayer FocusTarget to the new unit if appropriate. 
        /// </summary>

        private void SetFocus()
        {
            WoWPlayer Focus;
            string FocusGUID = Lua.GetReturnVal<string>("local GUID = UnitGUID(\"focus\"); if GUID == nil then return 0 else return GUID end", 0);

            if (FocusGUID == Convert.ToString(0))
            {
                if (FocusTarget != null)
                {
                    FocusTarget = null;

                    Logging.Write(Color.Orange, "Focus dropped -- clearing focus target.");
                }

                return;
            }

            // Remove the two starting characters (0x) from the GUID returned by lua using substring.
            // This is done so we can convert the string to a ulong.
            // Then we should set the WoWPlayer Focus to the unit which belongs to our formatted GUID.

            Focus = ObjectManager.GetAnyObjectByGuid<WoWPlayer>(ulong.Parse(FocusGUID.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier));

            if (FocusTarget != Focus && Focus.Distance2D < 100 && Focus.InLineOfSight && !Focus.Dead && Focus.IsInMyPartyOrRaid)
            {
                FocusTarget = Focus;

                Logging.Write(Color.Orange, "Setting " + FocusTarget.Name + " as focus target.");
            }
        }

        /// <summary>
        ///     Uses WoWMathHelper to ensure we are behind the target.
        ///     A 2 radians cone is tested against. We must be within 1.34r (behind target) to return true.
        ///     Hopefully this should fix HonorBuddy's IsBehind bug.
        /// </summary>

        private bool BehindTarget()
        {
            if (WoWMathHelper.IsBehind(Me.Location, Me.CurrentTarget.Location, Me.CurrentTarget.Rotation, 2f))
                return true;
            else return false;
        }

        /// <summary>
        ///     Checks to see if specified target is under the effect of crowd control
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns></returns>

        private bool IsCrowdControlled(WoWUnit target)
        {
            // Just want to throw a shout-out to Singular for this function.
            return target.GetAllAuras().Any(
            unit => unit.Spell.Mechanic == WoWSpellMechanic.Banished
                || unit.Spell.Mechanic == WoWSpellMechanic.Charmed
                || unit.Spell.Mechanic == WoWSpellMechanic.Horrified
                || unit.Spell.Mechanic == WoWSpellMechanic.Incapacitated
                || unit.Spell.Mechanic == WoWSpellMechanic.Polymorphed
                || unit.Spell.Mechanic == WoWSpellMechanic.Sapped
                || unit.Spell.Mechanic == WoWSpellMechanic.Shackled
                || unit.Spell.Mechanic == WoWSpellMechanic.Asleep
                || unit.Spell.Mechanic == WoWSpellMechanic.Frozen
            );
        }

        /// <summary>
        ///     Checks to see if any nearby units are under breakable crowd control.
        /// </summary>

        private bool ShouldWeAoe()
        {
            foreach (WoWUnit unit in EnemyUnits)
            {
                if (IsCrowdControlled(unit))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Check if player has specified active buff using Lua.
        /// </summary>
        /// <param name="BuffName"></param>

        private bool PlayerHasBuff(string BuffName)
        {
            string BuffNameLua = Lua.GetReturnValues(string.Format("local name = UnitBuff(\"player\", \"{0}\"); " +
                                                 "return name", BuffName))[0];

            if (BuffNameLua == BuffName) return true;
            else return false;
        }

        /// <summary>
        ///     Returns the duration of a buff on the player
        /// </summary>
        /// <param name="DebuffName"></param>

        private double PlayerBuffTimeLeft(string BuffName)
        {
            return double.Parse(Lua.GetReturnValues(string.Format("local expirationTime = select(7, UnitBuff(\"player\", \"{0}\", nil, \"player\")); " +
                                     "if expirationTime == nil then return 0 else return expirationTime - GetTime() end", BuffName))[0]);
        }

        /// <summary>
        ///     Check if target has specified debuff using Lua, and returns the time remaining on the debuff.
        /// </summary>
        /// <param name="DebuffName"></param>

        private double TargetDebuffTimeLeft(string DebuffName)
        {
            return double.Parse(Lua.GetReturnValues(string.Format("local expirationTime = select(7, UnitDebuff(\"target\", \"{0}\", nil, \"player\")); " +
                                     "if expirationTime == nil then return 0 else return expirationTime - GetTime() end", DebuffName))[0]);
        }

        /// <summary>
        ///     Gets the cooldown of specified spell using Lua
        /// </summary>
        /// <param name="SpellName">Specified Spell</param>
        /// <returns>Spell cooldown</returns>

        public double SpellCooldown(string SpellName)
        {
            return double.Parse(Lua.GetReturnValues(string.Format("local start, duration = GetSpellCooldown(\"{0}\"); " +
                                               "return (start+duration) - GetTime()", SpellName))[0]);
        }

        /// <summary>
        ///     Check if player's target is a boss using Lua.
        /// </summary>

        private bool IsTargetBoss()
        {
            string UnitClassification = Lua.GetReturnValues("local classification = UnitClassification(\"target\"); return classification")[0];

            if (!Me.IsInRaid)
            {
                if (UnitClassification == "worldboss" ||
                   (Me.CurrentTarget.Level == 87 && Me.CurrentTarget.Elite) ||
                   (Me.CurrentTarget.Level == 88))
                    return true;

                else return false;
            }
            else
            {
                if (UnitClassification == "worldboss" ||
                   (Me.CurrentTarget.Level == 88 && Me.CurrentTarget.Elite))
                    return true;

                else return false;
            }
        }

        /// <summary>
        ///     Returns true if a spell is not on cooldown or if
        ///     there is 0.3 or less left on GCD, AND the spell is known
        /// </summary>

        private bool CanCastSpell(string SpellName)
        {
            return (SpellCooldown(SpellName) <= 0.3 && SpellManager.HasSpell(SpellName));
        }

        #endregion

        #region BT Helpers

        #region Cast Spell

        /// <summary>
        ///     Checks if specified spell is castable, if so casts it and writes to log.
        /// </summary>
        /// <param name="SpellName">Spell name</param>
        /// <returns></returns>

        public Composite CastSpell(string SpellName)
        {
            return new Decorator(ret => Me.CurrentTarget != null && CanCastSpell(SpellName),
                new Action(delegate {
                Lua.DoString(String.Format("CastSpellByName(\"{0}\");", SpellName));
                Logging.Write(Color.LightBlue, "[" + CurrentEnergy + "] [" + Me.ComboPoints + "] " + SpellName); })
                    );
        }

        /// <summary>
        ///     Checks if specified spell is castable, if so casts it and writes to log.
        ///     Uses specified conditions.
        /// </summary>
        /// <param name="SpellName">Spell name</param>
        /// <param name="Conditions">Specified conditions</param>
        /// <returns></returns>

        public Composite CastSpell(string SpellName, CanRunDecoratorDelegate Conditions)
        {
            return new Decorator(ret => Conditions(ret) && CanCastSpell(SpellName),
                new Action(delegate {
                Lua.DoString(String.Format("CastSpellByName(\"{0}\");", SpellName));
                Logging.Write(Color.LightBlue, "[" + CurrentEnergy + "] [" + Me.ComboPoints + "] " + SpellName); })
            );
        }

        /// <summary>
        ///     Checks if specified spell is castable, if so casts it and writes to log.
        ///     Casts on focus
        /// </summary>
        /// <param name="SpellName">Spell name</param>
        /// <returns></returns>

        #endregion

        #region Cast Focus

        public Composite CastSpellOnFocus(string SpellName)
        {
            return new Decorator(ret => FocusTarget != null && SpellManager.CanCast(SpellName, FocusTarget),
                new Action(delegate {
                SpellManager.Cast(SpellName, FocusTarget);
                Logging.Write(Color.Yellow, SpellName); })
            );
        }

        /// <summary>
        ///     Checks if specified spell is castable, if so casts it and writes to log.
        ///     Casts on focus, uses specified conditions
        /// </summary>
        /// <param name="SpellName">Spell name</param>
        /// <param name="Conditions">Specified conditions</param>
        /// <returns></returns>

        public Composite CastSpellOnFocus(string SpellName, CanRunDecoratorDelegate Conditions)
        {
            return new Decorator(ret => Conditions(ret) && FocusTarget != null && SpellManager.CanCast(SpellName, FocusTarget),
                new Action(delegate {
                SpellManager.Cast(SpellName, FocusTarget);
                Logging.Write(Color.Yellow, SpellName); })
            );
        }

        #endregion

        #region Cast Cooldowns

        /// <summary>
        ///     Checks if specified cooldown is castable, if so casts it and writes to log.
        ///     Casts with Lua to make use of the ability queue.
        /// </summary>
        /// <param name="SpellName">Cooldown name</param>
        /// <returns></returns>

        public Composite CastCooldown(string CooldownName)
        {
            return new Decorator(ret => CanCastSpell(CooldownName),
                new Action(delegate {
                Lua.DoString(String.Format("CastSpellByName(\"{0}\");", CooldownName));
                Logging.Write(Color.Yellow, CooldownName); })
            );
        }

        /// <summary>
        ///     Checks if specified cooldown is castable, if so casts it and writes to log.
        ///     Uses specified conditions.
        ///     Casts with Lua to make use of the ability queue.
        /// </summary>
        /// <param name="SpellName">Cooldown name</param>
        /// <param name="Conditions">Specified conditions</param>
        /// <returns></returns>

        public Composite CastCooldown(string CooldownName, CanRunDecoratorDelegate Conditions)
        {
            return new Decorator(ret => Conditions(ret) && CanCastSpell(CooldownName),
                new Action(delegate {
                Lua.DoString(String.Format("CastSpellByName(\"{0}\");", CooldownName));
                Logging.Write(Color.Yellow, CooldownName); })
            );
        }

        #endregion

        /// <summary>
        ///     Checks if player is auto-attacking, and if not toggles auto-attack.
        /// </summary>
        /// <returns></returns>

        public Composite AutoAttack()
        {
            return new Decorator(ret => !Me.IsAutoAttacking,
                new Action(delegate
            {
                Me.ToggleAttack();
                Logging.Write(Color.Orange, "Auto-attack");
            })
            );
        }

        /// <summary>
        ///     Checks if player is auto-attacking, and if not toggles auto-attack.
        ///     Uses specified conditions.
        /// </summary>
        /// <returns></returns>

        public Composite AutoAttack(CanRunDecoratorDelegate Conditions)
        {
            return new Decorator(ret => Conditions(ret) && !Me.IsAutoAttacking,
                new Action(delegate
            {
                Me.ToggleAttack();
                Logging.Write(Color.Orange, "Auto-attack");
            })
            );
        }

        #endregion

        #endregion
    }
}
