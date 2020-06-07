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

        public QualityCategory minQuality = QualityCategory.Awful;
        public List<ItemNeededStat> neededStats;

        public List<ThingDef> fromRessource;

        public RoomRoleDef roomRole;
        public bool shoulBeRoomOwner = false;

        public List<ThingCategoryDef> fromCategory;

        private List<ItemWishInfo> cachedItems = null;

        public string role_Key = "{Role}";

        public string compText = "Comp : ";
        public string compStats = "Stats : ";
        public string stuffText = "From any :";


        public List<ItemWishInfo> Items
        {
            get
            {
                if (cachedItems == null)
                {
                    cachedItems = includedThing ?? new List<ItemWishInfo>();
                    CompleteInfo();
                    if (findPossibleWant) CacheData(SearchedDef);
                }
                return cachedItems;
            }
        }

        protected override bool FastSearchMatch(ThingDef thing)
        {
            return base.FastSearchMatch(thing) && (includedThing == null || includedThing.Find(info => info.def == thing) == null);
        }

        protected override bool LongSearchMatch(ThingDef thing)
        {
            if (!base.LongSearchMatch(thing)) return false;
            if (!fromCategory.NullOrEmpty())
            {
                if (thing.thingCategories.NullOrEmpty() || thing.thingCategories.Find(cat => fromCategory.Contains(cat)) == null) 
                    return false;
            }
            if (!neededStats.NullOrEmpty())
            {
                if (thing.statBases.NullOrEmpty() || neededStats.FindAll(stat => thing.statBases.StatListContains(stat.def)).Count < neededStats.Count)
                    return false;
            }
            if (!fromRessource.NullOrEmpty())
            {
                if (fromRessource.Find(ress => ress.stuffProps.CanMake(thing)) == null) return false;
            }
            return true;
        }

        protected virtual void CompleteInfo()
        {
            for (int i = 0; i < cachedItems.Count; i++)
            {
                if (cachedItems[i].useDefaultParam)
                {
                    if (cachedItems[i].needAmount == -1) cachedItems[i].needAmount = specificAmount;
                    if (cachedItems[i].minQuality == (QualityCategory)100) cachedItems[i].minQuality = minQuality;
                    if (cachedItems[i].neededComp == null) cachedItems[i].neededComp = neededComp;
                    if (cachedItems[i].neededStats == null) cachedItems[i].neededStats = neededStats;
                    if (cachedItems[i].fromRessource == null) cachedItems[i].fromRessource = fromRessource;
                }
            }
        }
        protected override void CacheData(List<ThingDef> defs)
        {
            if (defs.NullOrEmpty()) 
            { 
                Log.Message("HDream : no def found to complete infos for " + defName);
                return;
            }
            for (int i = 0; i < defs.Count; i++)
            {
                cachedItems.Add(new ItemWishInfo
                {
                    def = defs[i],
                    needAmount = specificAmount,
                    minQuality = minQuality,
                    neededComp = neededComp,
                    neededStats = neededStats,
                    fromRessource = fromRessource
                });
            }
        }
    }
}
