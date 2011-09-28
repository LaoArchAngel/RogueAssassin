using Styx;
using Styx.Helpers;

namespace RogueAssassin
{
    public class RASettings : Settings
    {
        public RASettings()
            : base(string.Format("{0}\\Settings\\RogueAssassin_{1}.xml", Logging.ApplicationPath, StyxWoW.Me.Name))
        {
            Load();
            
        }

        ~RASettings()
        {
            Save();
        }

        [Setting, DefaultValue(4)]
        public int AOEMinTargets { get; set; }

		[Setting, DefaultValue(true)]
		public bool ColdBloodBossOnly { get; set; }

		[Setting, DefaultValue(12f)]
        public float FOKRange { get; set; }

        [Setting, DefaultValue(true)]
        public bool UseAOE { get; set; }

        [Setting, DefaultValue(true)]
        public bool UseFOK { get; set; }

		[Setting, DefaultValue(false)]
		public bool UseItemsBossOnly { get; set; }

		[Setting, DefaultValue(true)]
		public bool UseVanish { get; set; }

		[Setting, DefaultValue(true)]
		public bool VanishBossOnly { get; set; }

		[Setting, DefaultValue(true)]
		public bool VendettaBossOnly { get; set; }
    }
}