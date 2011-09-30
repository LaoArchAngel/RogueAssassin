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
using RogueAssassin.Talents;
using Styx.Combat.CombatRoutine;
using Styx.Helpers;
using TreeSharp;
using Action = TreeSharp.Action;

namespace RogueAssassin
{
	public class RogueAssassin : CombatRoutine
	{
        private static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;
        public override sealed string Name { get { return string.Format("RogueAssassin v{0}", Version.ToString(3)); } }
		public override WoWClass Class { get { return WoWClass.Rogue; } }

		internal static RASettings Settings { get { return _raSettings ?? (_raSettings = new RASettings()); } }

		public override void Initialize()
		{
		    Logging.Write(Color.Orange, "RogueAssassin v{0} Loaded.",
		                  Version.ToString(3));
            Helpers.ResetAll += Auras.Reset;
		}

		#region Composite declarations

		private Composite _combatBehavior;
		private static RASettings _raSettings;

		public override Composite CombatBehavior
		{
			get
			{
				if (_combatBehavior == null)
				{
					switch (SpecManager.Spec)
					{
						case SpecManager.SpecList.None:

							_combatBehavior = new Action(ret => Logging.Write(Color.Orange, "Low level combat is unsupported."));
							break;

						case SpecManager.SpecList.Assassination:
							_combatBehavior = new Rotations.MutilatePvE.Rotation().Build();
							//_combatBehavior = new Rotations.Debug.Rotation().Build();
							Logging.Write(Color.Orange, "Assassination combat behavior built");
							break;

						case SpecManager.SpecList.Combat:

							_combatBehavior = new Action(ret => Logging.Write(Color.Orange, "Combat combat behavior built"));
							break;

						case SpecManager.SpecList.Subtlety:

							_combatBehavior = new Action(ret => Logging.Write(Color.Orange, "Subtlety combat unsupported"));
							break;
					}
				}

				return _combatBehavior;
			}
		}

		#endregion
	}
}