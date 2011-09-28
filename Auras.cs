using Styx;
using Styx.Logic.Combat;

namespace RogueAssassin
{
	static class Auras
    {
        #region Fields

        private static WoWAura _envenom;
		private static WoWAura _rupture;
		private static WoWAura _sliceAndDice;

		#endregion

		#region Properties

		public static WoWAura Envenom
		{
			get { return _envenom ?? (_envenom = StyxWoW.Me.GetAllAuras().Find(a => a.SpellId == Spells.ENVENOM)); }
		}

		public static WoWAura Rupture
		{
            get
            {
                if (!StyxWoW.Me.GotTarget) return null;
                return _rupture ?? (_rupture = StyxWoW.Me.CurrentTarget.GetAllAuras().Find(a => a.SpellId == Spells.RUPTURE));
            }
		}

		public static WoWAura SliceAndDice
		{
            get { return _sliceAndDice ?? (_sliceAndDice = StyxWoW.Me.GetAllAuras().Find(a => a.SpellId == Spells.SLICE_AND_DICE)); }
		}

		#endregion

		#region Methods

		public static void Reset()
		{
			_envenom = null;
			_rupture = null;
			_sliceAndDice = null;
		}

		#endregion
	}
}
