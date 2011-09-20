using Styx;
using Styx.Helpers;

namespace RogueAssassin
{
    public class RogueAssassinSettings : Settings
    {
        public RogueAssassinSettings()
            : base(string.Format("{0}\\Settings\\RogueAssassin_{1}.xml", Logging.ApplicationPath, StyxWoW.Me.Name))
        {
            Load();
            
        }

        ~RogueAssassinSettings()
        {
            Save();
        }

        [Setting, DefaultValue(12f)]
        public float FOKRange { get; set; }

        [Setting, DefaultValue(true)]
        public bool UseAOE { get; set; }

        [Setting, DefaultValue(true)]
        public bool UseFOK { get; set; }
    }
}