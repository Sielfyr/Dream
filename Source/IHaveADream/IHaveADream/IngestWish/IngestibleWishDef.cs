using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace HDream
{
    public class IngestibleWishDef : ThingWishDef
    {
        public List<ThingDef> includedThing;

        public bool checkPerNutriment = false;

        public bool includeJoy = false;
        public float minJoy = 0;

        private List<ThingDef> cachedIngestibles = null;
        public List<ThingDef> Ingestibles
        {
            get
            {
                if (cachedIngestibles == null)
                {
                    cachedIngestibles = includedThing ?? new List<ThingDef>();
                    if (findPossibleWant) CacheData(SearchedDef);
                }
                return cachedIngestibles;
            }
        }
        protected override bool FastSearchMatch(ThingDef thing)
        {
            return base.FastSearchMatch(thing) && IsValidIngestible(thing);
        }

        protected override void CacheData(List<ThingDef> defs)
        {
            cachedIngestibles.AddRange(defs);
        }
        protected virtual bool IsValidIngestible(ThingDef ingestible)
        {
            return ingestible.IsIngestible
                && !cachedIngestibles.Contains(ingestible)
                && IsMatchingOneParameter(ingestible);
        }

        protected virtual bool IsMatchingOneParameter(ThingDef ingestible)
        {
            return NoMatchNeeded(ingestible) || (includeJoy && ingestible.ingestible.joy >= minJoy);
        }
        protected virtual bool NoMatchNeeded(ThingDef ingestible)
        {
            return !includeJoy;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }
            if (!includedThing.NullOrEmpty())
            {
                for (int i = 0; i < includedThing.Count; i++)
                {
                    if (!includedThing[i].IsIngestible) yield return ("HDream : Wrong ThingDef listed in includedThing for IngestibleWishDef " + defName + ". It's not an ingestible thing ! That ThingDef '" + includedThing[i].ToString() + "' was removed from the list");
                }
            }
        }

    }
}
