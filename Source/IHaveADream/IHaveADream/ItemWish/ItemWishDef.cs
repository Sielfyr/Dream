using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System;
using System.Linq;

namespace HDream
{
    public class ItemWishDef : ThingWishDef
    {
        public List<ItemWishInfo> includedThing;

        public ThingCategoryDef thingCategory;

        public int totalNeededAmount = 1;

        public int specificItemAmount = 1;
        public QualityCategory minQuality = QualityCategory.Awful;
        public List<Type> neededComp;
        public List<ItemNeededStat> neededStats;

        public List<ThingDef> fromRessource;

        public RoomRoleDef roomRole;
        public bool shoulBeRoomOwner = false;
        public bool countAmountPerInfo = false;

        public List<ThingCategoryDef> fromCategory;

        private List<ItemWishInfo> cachedItems = null;

        public string role_Key = "{Role}";
        public string countRule_Key = "{CountRule}";

        public string perUnitRule = "Each item unit count for the total need count, surplus of same item don't count";
        public string perInfoRule = "To complete the total need count, you should meet the specifique amount asked per item.";
        
        public string compText = "Comp : ";
        public string compStats = "Stats : ";
        public string stuffText = "From any :";

        protected virtual List<ThingDef> SearchedDef
        {
            get
            {
                List<ThingDef> thingDefs = new List<ThingDef>();
                List<ThingDef> allThing = DefDatabase<ThingDef>.AllDefsListForReading;

                bool found = false;
                for (int i = 0; i < allThing.Count; i++)
                {
                    if (!FastSearchMatch(allThing[i])) continue;
                    if (!fromCategory.NullOrEmpty())
                    {
                        if (allThing[i].thingCategories.NullOrEmpty()
                            || allThing[i].thingCategories.Find(cat => fromCategory.Contains(cat)) == null) continue;
            }
                    if (!neededComp.NullOrEmpty())
                    {
                        if (allThing[i].comps.NullOrEmpty()) continue;
                        for (int j = 0; j < neededComp.Count; j++)
                        {
                            found = false;
                            for (int k = 0; k < allThing[i].comps.Count; k++)
                            {
                                if (neededComp[j] == allThing[i].comps[k].compClass)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found) break;
                        }
                        if (!found) continue;
                    }
                    if (!neededStats.NullOrEmpty())
                    {
                        if (allThing[i].statBases.NullOrEmpty()) continue;
                        found = true;
                        for (int j = 0; j < neededStats.Count; j++)
                        {
                            if (allThing[i].statBases.StatListContains(neededStats[j].def)) continue;
                            found = false;
                            break;
                        }
                        if (!found) continue;
                    }
                    if (!fromRessource.NullOrEmpty())
                    {
                       found = false;
                        for (int j = 0; j < fromRessource.Count; j++)
                        {
                            if (fromRessource[j].stuffProps.CanMake(allThing[i])) continue;
                            found = true;
                            break;
                        }
                        if (!found) continue;
                    }

                    thingDefs.Add(allThing[i]);
                }
                return thingDefs;
            }
        }

        protected virtual bool FastSearchMatch(ThingDef thing)
        {
            return (excludedThing.NullOrEmpty() || !excludedThing.Contains(thing));
        }

        public List<ItemWishInfo> Items
        {
            get
            {
                if (cachedItems == null)
                {
                    cachedItems = includedThing ?? new List<ItemWishInfo>();
                    if (findPossibleWant) CacheData(SearchedDef);
                }
                return cachedItems;
            }
        }

        protected void CacheData(List<ThingDef> defs)
        {
            if (includedThing != null) for (int i = 0; i < includedThing.Count; i++)
            {
                if (includedThing[i].useDefaultParam)
                {
                    if (includedThing[i].needAmount == -1) includedThing[i].needAmount = specificItemAmount;
                    if (includedThing[i].minQuality == (QualityCategory)100) includedThing[i].minQuality = minQuality;
                    if (includedThing[i].neededComp == null) includedThing[i].neededComp = neededComp;
                    if (includedThing[i].neededStats == null) includedThing[i].neededStats = neededStats;
                    if (includedThing[i].fromRessource == null) includedThing[i].fromRessource = fromRessource;
                    }
            }

            if (defs.NullOrEmpty()) Log.Message("HDream : no def to complete heddifInfos for " + defName);
            for (int i = 0; i < defs.Count; i++)
            {
                if (includedThing != null && includedThing.Find(info => info.def == defs[i]) != null) continue;
                cachedItems.Add(new ItemWishInfo
                {
                    def = defs[i],
                    needAmount = specificItemAmount,
                    minQuality = minQuality,
                    neededComp = neededComp,
                    neededStats = neededStats,
                    fromRessource = fromRessource
                });
            }
        }
        public override void PostLoad()
        {
            base.PostLoad();
            if(neededComp != null)
            {
                neededComp.RemoveAll(delegate(Type type) {
                    if (type != typeof(ThingComp) && typeof(ThingComp).IsAssignableFrom(type)) return false;
                    Log.Error("HDream : wrong type (" + type.ToString() + ") added in neededComp, it was removed. You should only add type inherited from ThingComp");
                    return true;
                });
            }
        }


    }
}
