using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace HDream
{
    public class IngestibleWishDef : ThingWishDef
    {
        public List<ThingDef> includedThing;

        public bool checkPerNutriment = false;

        public float progressStep = 1f;

        public bool includeJoy = false;
        public float minJoy = 0;

        private List<ThingDef> cachedIngestibles = null;
        public List<ThingDef> Ingestibles
        {
            get
            {
                if(cachedIngestibles == null) CacheData();
                return cachedIngestibles;
            }
        } 

        protected void CacheData()
        {
            cachedIngestibles = new List<ThingDef>();
            if (!includedThing.NullOrEmpty())
            {
                for (int i = 0; i < includedThing.Count; i++)
                {
                    if (includedThing[i].IsIngestible) cachedIngestibles.Add(includedThing[i]);
                    else Log.Warning("HDream : Wrong ThingDef listed in possibleIngestible for IngestibleWishDef " + defName + ". It's not an ingestible thing ! That ThingDef '" + includedThing[i].ToString() + "' was not added in the ingestible pool");
                }
            }
            if (findPossibleWant)
            {
                List<ThingDef> things = DefDatabase<ThingDef>.AllDefsListForReading;
                for (int j = 0; j < things.Count; j++)
                {
                    if (IsValidIngestible(things[j])) cachedIngestibles.Add(things[j]);
                }
            }
        }

        protected virtual bool IsValidIngestible(ThingDef ingestible)
        {
            return ingestible.IsIngestible
                && !cachedIngestibles.Contains(ingestible)
                && (excludedThing.NullOrEmpty() || !excludedThing.Contains(ingestible))
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
    }
}
