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
        Version curVersion = new Version(3, 0);

        public override sealed string Name { get { return "MutaRaidBT v" + curVersion; } }
        public override WoWClass Class { get { return WoWClass.Rogue; } }

        private static LocalPlayer Me { get { return ObjectManager.Me; } }
        private readonly SpecManager Talent = new SpecManager();

        public override void Initialize()
        {
            Logging.Write(Color.Orange, "");
            Logging.Write(Color.Orange, "MutaRaidBT v" + curVersion + " is now operational.");
            Logging.Write(Color.Orange, "A BehaviorTree implementation, for maximum speed and performance.");
            Logging.Write(Color.Orange, "For support and feedback please visit the forum thread.");
            Logging.Write(Color.Orange, "");
            Logging.Write(Color.Orange, "Enjoy topping the DPS meters!");
            Logging.Write(Color.Orange, "");
        }

        #region Composite declarations

        private Composite _CombatBehavior;

        public override Composite CombatBehavior 
        { 
            get 
            {
                if (_CombatBehavior == null)
                {
                    switch (Talent.Spec)
                    {
                        case SpecManager.SpecList.None:

                            _CombatBehavior = BuildNoneCombatBehavior();
                            Logging.Write(Color.Orange, "  Low level combat unsupported");
                            break;

                        case SpecManager.SpecList.Assassination:

                            _CombatBehavior = BuildAssassinationCombatBehavior();
                            Logging.Write(Color.Orange, "  Assassination combat behavior built");
                            break;

                        case SpecManager.SpecList.Combat:

                            _CombatBehavior = BuildCombatCombatBehavior();
                            Logging.Write(Color.Orange, "  Combat combat behavior built (EXPERIMENTAL)");
                            break;

                        case SpecManager.SpecList.Subtlety:

                            _CombatBehavior = BuildSubtletyCombatBehavior();
                            Logging.Write(Color.Orange, "  Subtlety combat unsupported");
                            break;
                    }
                }

                return _CombatBehavior; 
            } 
        }

        #endregion
    }
}
