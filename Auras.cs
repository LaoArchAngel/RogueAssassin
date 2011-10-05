using Styx;
using Styx.Logic.Combat;

namespace RogueAssassin
{
    /// <summary>
    /// Contains all of the Auras used by rogues.
    /// These auras are loaded on the first call from the BT.
    /// </summary>
    internal static class Auras
    {
        #region Fields

        private static WoWAura _envenom;
        private static WoWAura _rupture;
        private static WoWAura _sliceAndDice;

        #endregion

        #region Properties

        /// <summary>
        /// Envenom buff on the player.
        /// Null if no buff was found.
        /// </summary>
        public static WoWAura Envenom
        {
            get { return _envenom ?? (_envenom = StyxWoW.Me.GetAllAuras().Find(a => a.SpellId == Spells.ENVENOM)); }
        }

        /// <summary>
        /// Rupture debuff on the target.
        /// Null if Rupture is down.
        /// </summary>
        public static WoWAura Rupture
        {
            get
            {
                if (!StyxWoW.Me.GotTarget) return null;
                return _rupture
                       ?? (_rupture = StyxWoW.Me.CurrentTarget.GetAllAuras().Find(a => a.SpellId == Spells.RUPTURE));
            }
        }

        /// <summary>
        /// Slice and Dice buff on the player.
        /// Null if SnD is down.
        /// </summary>
        public static WoWAura SliceAndDice
        {
            get
            {
                return _sliceAndDice
                       ?? (_sliceAndDice = StyxWoW.Me.GetAllAuras().Find(a => a.SpellId == Spells.SLICE_AND_DICE));
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Resets the properties fields used by the properties so that they can be reset.
        /// </summary>
        public static void Reset()
        {
            _envenom = null;
            _rupture = null;
            _sliceAndDice = null;
        }

        #endregion
    }
}