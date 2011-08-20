using System;
using System.Collections.Generic;
using Styx.WoWInternals;

namespace Doctrine.Talents
{
    class SpecManager
    {
        public enum SpecList
        {
            None = 0,
            Assassination,
            Combat,
            Subtlety
        }

        private int CurrentSpecGroup()
        {
            return Lua.GetReturnVal<int>("return GetActiveTalentGroup(false, false)", 0);
        }

        private int CurrentSpecValue()
        {
            int tab;
            int group = CurrentSpecGroup();

            int[] pointsSpent = new int[3];
           
            for (tab = 1; tab <= 3; tab++)
            {
                List<string> TalentTabInfo = Lua.GetReturnValues("return GetTalentTabInfo(" + tab + ", false, false, " + group + ")");
                pointsSpent[tab - 1] = Convert.ToInt32(TalentTabInfo[4]);
            }

            if (pointsSpent[0] > (pointsSpent[1] + pointsSpent[2]))
            {
                return 1;
            }
            else if (pointsSpent[1] > (pointsSpent[0] + pointsSpent[2])) 
            {
                return 2;
            }
            else if (pointsSpent[2] > (pointsSpent[0] + pointsSpent[1]))
            {
                return 3;
            }
            else
            {
                return 0;
            }
        }

        public SpecList Spec
        {
            get
            {
                int currentSpec = CurrentSpecValue();
                return (SpecList)currentSpec;
            }
        }
    }
}
