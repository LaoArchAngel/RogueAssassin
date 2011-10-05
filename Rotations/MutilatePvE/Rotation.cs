﻿using System.Linq;
using CommonBehaviors.Actions;
using Styx;
using TreeSharp;

namespace RogueAssassin.Rotations.MutilatePvE
{
    /// <summary>
    /// Defines the rotation for MutilatePvE.
    /// </summary>
    internal class Rotation
    {
        private const int EC_BS = 30;
        private const int EC_MUTILATE = 55;
        private const int EC_RUPTURE = 25;
        private const int EC_ENVENOM = 35;

        public Composite Build()
        {
            return
                new Decorator(ret => !StyxWoW.Me.Mounted && StyxWoW.Me.CurrentTarget != null,
                              new PrioritySelector(ret => StyxWoW.Me.CurrentTarget.IsWithinMeleeRange,
                                                   new Action(delegate { return Helpers.ResetFail(); }),
                                                   AutoAttack(),
                                                   Spells.Cast(Spells.GARROTE, ret => StyxWoW.Me.IsStealthed),
                                                   Spells.Cast(Spells.REDIRECT,
                                                               ret => StyxWoW.Me.ComboPoints < StyxWoW.Me.RawComboPoints),
                                                   Spells.Cast(Spells.FAN_OF_KNIVES,
                                                               ret =>
                                                               RogueAssassin.Settings.FOK && Helpers.AOEIsSafe
                                                               && !Helpers.TargetIsBoss
                                                               &&
                                                               Helpers.NearbyEnemies.Count()
                                                               >= RogueAssassin.Settings.FOKMinTargets),
                                                   Spells.Cast(Spells.SLICE_AND_DICE,
                                                               ret =>
                                                               StyxWoW.Me.ComboPoints > 0 && Auras.SliceAndDice == null),
                                                   Spells.Cast(Spells.ENVENOM,
                                                               ret =>
                                                               StyxWoW.Me.ComboPoints > 0 && Auras.SliceAndDice != null
                                                               &&
                                                               (Auras.SliceAndDice.TimeLeft.Seconds < 2
                                                                ||
                                                                StyxWoW.Me.GetAllAuras().Find(
                                                                    a => a.SpellId == Spells.COLD_BLOOD)
                                                                != null)),
                                                   Rupture(),
                                                   Spells.CastFocus(Spells.TRICKS_OF_THE_TRADE,
                                                                    ret => Helpers.FocusReadyForTricks),
                                                   CoolDowns(),
                                                   ComboPoint(),
                                                   Spells.CastStatus(Spells.ENVENOM,
                                                                     ret =>
                                                                     StyxWoW.Me.ComboPoints > 3
                                                                     &&
                                                                     ((Auras.Envenom == null
                                                                       && Helpers.CurrentEnergy >= 90)
                                                                      || Helpers.CurrentEnergy > 105))));
        }

        private static Composite AutoAttack()
        {
            return new Decorator(ret => !(StyxWoW.Me.IsAutoAttacking ^ StyxWoW.Me.IsStealthed),
                                 new Action(delegate { StyxWoW.Me.ToggleAttack(); }));
        }

        private static Composite ComboPoint()
        {
            return new PrioritySelector(
                new Decorator(ret => StyxWoW.Me.CurrentTarget.HealthPercent < 35
                                     && !StyxWoW.Me.CurrentTarget.IsFacing(StyxWoW.Me)
                                     && StyxWoW.Me.ComboPoints < 5
                                     && PooledFinisher(EC_BS),
                              Spells.Cast(Spells.BACKSTAB)),
                new Decorator(ret => StyxWoW.Me.ComboPoints < 4 && PooledFinisher(EC_MUTILATE),
                              Spells.Cast(Spells.MUTILATE)));
        }

        private static Composite CoolDowns()
        {
            return new PrioritySelector(
                new Decorator(ret => !RogueAssassin.Settings.VendettaBossOnly || Helpers.TargetIsBoss,
                              Spells.Cast(Spells.VENDETTA)),
                new Decorator(ret => StyxWoW.Me.ComboPoints == 5
                                     && Helpers.CurrentEnergy >= 65
                                     && Helpers.CurrentEnergy < 95
                                     && (!RogueAssassin.Settings.ColdBloodBossOnly || Helpers.TargetIsBoss),
                              Spells.Cast(Spells.COLD_BLOOD)),
                new Decorator(ret => RogueAssassin.Settings.Vanish
                                     && (!RogueAssassin.Settings.VanishBossOnly || Helpers.TargetIsBoss)
                                     && StyxWoW.Me.GetAllAuras().Find(a => a.SpellId == Spells.OVERKILL) == null
                                     && Auras.Envenom == null,
                              Vanish())
                );
        }

        /// <summary>
        /// Determines if we have pooled enough energy to cast a Combo Point Generator and a Finisher.
        /// This is important because we do not want to cast a generator if we won't have enough energy to cast a
        /// finisher before a buff runs out.
        /// </summary>
        /// <param name="cost">Cost of our generator.</param>
        /// <returns>True if we've pooled enough energy, or else there's another reason we should not pool.</returns>
        private static bool PooledFinisher(int cost)
        {
            // Do not pool if we have no combo points.
            if (StyxWoW.Me.ComboPoints == 0) return true;
            // Never done pooling if one of our buffs is missing.
            if (Auras.SliceAndDice == null || Auras.Rupture == null) return false;

            // Determine if rupture is our next finisher.  We determine that by ensuring that we have enough time to
            // cast at least rupture, mutilate, and envenom with our current energy pool before SnD goes out.  Also, 
            // rupture must have less time than SnD.
            bool ruptureNext = Auras.SliceAndDice.TimeLeft > Auras.Rupture.TimeLeft
                               &&
                               (EC_RUPTURE + EC_MUTILATE + EC_ENVENOM - Helpers.CurrentEnergy)/10d
                               > Auras.SliceAndDice.TimeLeft.Seconds;

            if (ruptureNext)
            {
                if (EC_RUPTURE + cost < Helpers.CurrentEnergy) return true;
                if (Auras.Envenom.TimeLeft.Seconds + (EC_RUPTURE/10d) < Auras.Rupture.TimeLeft.Seconds) return true;

                // EC_RUPTURE + cost - _currentEnergy = Deficit from casting both a combo point and finisher.
                // ((RuptureDebuff - 2) * 10) = energy that can be pooled before Rupture is 2 secs from falling off.
                return (EC_RUPTURE + cost - Helpers.CurrentEnergy - ((Auras.Rupture.TimeLeft.Seconds - 2)*10) <= 0);
            }

            return EC_ENVENOM + cost <= Helpers.CurrentEnergy;
        }

        private static Composite Rupture()
        {
            return
                new Decorator(ret => StyxWoW.Me.ComboPoints > 0
                                     && (Auras.Rupture == null
                                         || (Auras.Rupture.TimeLeft < Auras.SliceAndDice.TimeLeft
                                             && Auras.Rupture.TimeLeft.Seconds < 2)),
                              new PrioritySelector(Spells.CastStatus(Spells.RUPTURE), ComboPoint(),
                                                   new ActionAlwaysSucceed()));
        }

        private static Composite Vanish()
        {
            return new Sequence(Spells.Cast(Spells.VANISH), new WaitContinue(1, ret => false, new ActionAlwaysSucceed()));
        }
    }
}