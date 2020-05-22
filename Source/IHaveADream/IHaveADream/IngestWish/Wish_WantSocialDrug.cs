using RimWorld;
using HarmonyLib;
using Verse;

namespace HDream
{
    public class Wish_WantSocialDrug : Wish_WantDrug
    {

        protected override bool IsValidIngestible(ThingDef ingestible)
        {
            return base.IsValidIngestible(ingestible) && ingestible.ingestible.drugCategory == DrugCategory.Social;
        }
    }
}
