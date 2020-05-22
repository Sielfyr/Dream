using RimWorld;
using HarmonyLib;
using Verse;

namespace HDream
{
    public class WishCompProperties_Timed : WishCompProperties
    {
        public float daysToHold;

        public bool resetTimerOnFailHold = false;

        public int removePendingOnHoldOffset = 0;
        public float removePendingOnHoldPercent = 0;

        public float removePendingPerTickFactor = 1;

        public string descMain = "The conditions of this wish must be satisfied during {Time} days in order to be fulfilled.";
        public string time_Key = "{Time}";
        public string descSatisfied = "The conditions are satisfied.";
        public string descNotSatisfied = "The conditions aren't satisfied.";

        public WishCompProperties_Timed()
        {
            compClass = typeof(WishComp_Timed);
        }
    }
}
