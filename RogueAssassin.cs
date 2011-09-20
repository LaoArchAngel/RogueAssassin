// MutaRaid by fiftypence
// Now rewritten using BTs!
//
// SVN: http://fiftypence.googlecode.com/svn/trunk/
// 
// As always, if you wish to reuse code, please seek permission first 
// and provide credit where appropriate

using System;
using System.Drawing;
using System.Reflection;
using Styx.Combat.CombatRoutine;
using Styx.Helpers;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

using Doctrine.Talents;

using TreeSharp;

namespace RogueAssassin
{
	public partial class RogueAssassin : CombatRoutine
	{
        private static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;
        public override sealed string Name { get { return string.Format("RogueAssassin v{0}", Version.ToString(3)); } }
		public override WoWClass Class { get { return WoWClass.Rogue; } }

		private static LocalPlayer Me { get { return ObjectManager.Me; } }
		private readonly SpecManager _talent = new SpecManager();

		public override void Initialize()
		{
		    Logging.Write(Color.Orange, "RogueAssassin v{0} Loaded.",
		                  Version.ToString(3));
		}

		#region Composite declarations

		private Composite _combatBehavior;

	    public override Composite CombatBehavior
		{
			get
			{
				if (_combatBehavior == null)
				{
					switch (_talent.Spec)
					{
						case SpecManager.SpecList.None:

							_combatBehavior = BuildNoneCombatBehavior();
							Logging.Write(Color.Orange, "Low level combat unsupported");
							break;

						case SpecManager.SpecList.Assassination:

							_combatBehavior = BuildAssassinationCombatBehavior();
							Logging.Write(Color.Orange, "Assassination combat behavior built");
							break;

						case SpecManager.SpecList.Combat:

							_combatBehavior = BuildCombatCombatBehavior();
							Logging.Write(Color.Orange, "Combat combat behavior built (EXPERIMENTAL)");
							break;

						case SpecManager.SpecList.Subtlety:

							_combatBehavior = BuildSubtletyCombatBehavior();
							Logging.Write(Color.Orange, "Subtlety combat unsupported");
							break;
					}
				}

				return _combatBehavior;
			}
		}

		#endregion
	}
}