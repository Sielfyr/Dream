using RimWorld;
using HarmonyLib;
using Verse;

namespace HDream
{
    public class FoodWishDef : IngestibleWishDef
    {

        public bool includePreferability = false;
        public FoodPreferability minFoodPreferability = 0;

        protected override bool IsValidIngestible(ThingDef ingestible)
        {
            return base.IsValidIngestible(ingestible)
                && ingestible.ingestible.JoyKind == JoyKindDefOf.Gluttonous;
        }
        protected override bool IsMatchingOneParameter(ThingDef ingestible)
        {
            return base.IsMatchingOneParameter(ingestible) || (includePreferability && ingestible.ingestible.preferability >= minFoodPreferability);
        }
        protected override bool NoMatchNeeded(ThingDef ingestible)
        {
            return base.NoMatchNeeded(ingestible) && !includePreferability;
        }
    }
}
