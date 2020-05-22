using RimWorld;
using HarmonyLib;
using Verse;

namespace HDream
{
    public class DrugWishDef : IngestibleWishDef
    {

        public DrugCategory drugCategory = DrugCategory.Any;

        protected override bool IsValidIngestible(ThingDef ingestible)
        {
            return base.IsValidIngestible(ingestible)
                && ingestible.IsDrug
                && (drugCategory == DrugCategory.Any || drugCategory == ingestible.ingestible.drugCategory);
        }
        protected override bool IsMatchingOneParameter(ThingDef ingestible)
        {
            return base.IsMatchingOneParameter(ingestible) || (includeJoy && ingestible.ingestible.JoyKind == HDJoyKindDefOf.Chemical);
        }
    }
}
