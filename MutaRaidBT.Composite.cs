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
        private Composite BuildNoneCombatBehavior()
        {
            return new Action(ret => Logging.Write(Color.Orange, "Low level combat is unsupported."));
        }

        private Composite BuildAssassinationCombatBehavior()
        {
            return new Decorator(ret => !Me.Mounted && Me.CurrentTarget != null,
                new PrioritySelector(
                    new Action(delegate {
                        UpdateEnergy();
                        SetFocus();
                        Logging.WriteDebug(Color.Orange, IsTargetBoss() + " " + Me.CurrentTarget.Name);
                        return RunStatus.Failure; }),

                    AutoAttack(),

                    CastSpellOnFocus("Tricks of the Trade"),

                    CastSpell("Redirect", ret => Me.ComboPoints < Me.RawComboPoints),
                    CastSpell("Fan of Knives", ret => EnemyUnits.Count() >= 4 && ShouldWeAoe() && !IsTargetBoss()),
                    CastSpell("Slice and Dice", ret => !PlayerHasBuff("Slice and Dice") &&
                                                       Me.ComboPoints >= 1),
                    CastSpell("Rupture", ret => (TargetDebuffTimeLeft("Rupture") <= 1 && Me.ComboPoints >= 4) ||
                                                       (TargetDebuffTimeLeft("Rupture") == 0 && Me.ComboPoints >= 1)),

                    new Decorator(ret => IsTargetBoss(),
                        new PrioritySelector(
                            CastCooldown("Vendetta", ret => TargetDebuffTimeLeft("Rupture") > 0 && PlayerHasBuff("Slice and Dice")),

                            new Decorator(ret => CanCastSpell("Vanish") && !PlayerHasBuff("Overkill") && CurrentEnergy > 60,
                                new Sequence(
                                    CastCooldown("Vanish"),
                                    new WaitContinue(1, ret => false, new ActionAlwaysSucceed()),
                                    CastSpell("Garrote")
                                )
                            ),

                            CastCooldown("Cold Blood", ret => Me.ComboPoints == 5 && CurrentEnergy >= 70 && CurrentEnergy < 90)
                        )
                    ),

                    new Decorator(ret => Me.CurrentTarget.HealthPercent >= 35 ||
                                         (Me.CurrentTarget.HealthPercent < 35 && !BehindTarget()),
                        new PrioritySelector(
                            CastSpell("Envenom", ret => ((CurrentEnergy > 90 && Me.ComboPoints >= 4) ||
                                                         (PlayerBuffTimeLeft("Slice and Dice") < 3 && Me.ComboPoints >= 1)) &&
                                                         (!PlayerHasBuff("Envenom") || CurrentEnergy > 100)),
                            CastSpell("Mutilate", ret => Me.ComboPoints < 4 &&
                                                         (CurrentEnergy > 90 || PlayerHasBuff("Envenom")))
                        )
                    ),

                    new Decorator(ret => Me.CurrentTarget.HealthPercent < 35 && BehindTarget(),
                        new PrioritySelector(
                            CastSpell("Envenom", ret => ((CurrentEnergy > 90 && Me.ComboPoints == 5) ||
                                                        (PlayerBuffTimeLeft("Slice and Dice") < 3 && Me.ComboPoints >= 1)) &&
                                                        (!PlayerHasBuff("Envenom") || CurrentEnergy > 100)),
                            CastSpell("Backstab", ret => Me.ComboPoints != 5 &&
                                                            (CurrentEnergy > 90 || PlayerHasBuff("Envenom")))
                        )
                    )
                )
            );
        }

        private Composite BuildCombatCombatBehavior()
        {
            return new Action(ret => Logging.Write(Color.Orange, "Combat behavior has not yet been implemented."));
        }

        private Composite BuildSubtletyCombatBehavior()
        {
            return new Action(ret => Logging.Write(Color.Orange, "Subtlety combat is unsupported."));
        }

    }
}
