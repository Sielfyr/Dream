using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HDream
{
    public class Wish_Item : Wish_Thing<ItemWishInfo>
    {
        public ItemWishDef Def => (ItemWishDef)def;

        private int doAtTick = 0;
        public const int waitForTick = 300;
        protected override List<ItemWishInfo> GetThingsFromDef() => Def.Items;
        protected override ThingDef GetThingDef(ItemWishInfo thing) => thing.def;
        protected override LookMode ExposeLookModeT() => LookMode.Deep;

        protected int itemCount = 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref itemCount, "itemCount", 0);
        }
        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame < doAtTick) return;
            doAtTick = Find.TickManager.TicksGame + waitForTick;
            CheckResolve();
        }

        protected void CheckResolve()
        {
            int count = 0;
            if (Def.roomRole != null)
            {
                List<Room> rooms = pawn.Map.regionGrid.allRooms.FindAll(room => room.Role == Def.roomRole && (!Def.shoulBeRoomOwner || room.Owners.Contains(pawn)));
                for (int i = 0; i < rooms.Count; i++)
                {
                    for (int j = 0; j < ThingsWanted.Count; j++)
                    {
                        count += MatchCount(ItemMatching(rooms[i].ContainedThings(ThingsWanted[j].def).ToList(), ThingsWanted[j]), ThingsWanted[j]);
                    }
                }
            }
            else
            {
                ListerThings lister = pawn.Map.listerThings;
                for (int i = 0; i < ThingsWanted.Count; i++)
                {
                    count += MatchCount(ItemMatching(lister.ThingsOfDef(ThingsWanted[i].def), ThingsWanted[i]), ThingsWanted[i]);
                }
            }
            if (count >= Def.totalNeededAmount)
            {
                OnFulfill();
                return;
            }
            if (count != itemCount)
            {
                ChangeProgress(count - itemCount);
                itemCount = count;
            }
        }

        protected virtual int MatchCount(int count, ItemWishInfo info)
        {
            if (Def.countAmountPerInfo)
            {
                if (count < info.needAmount) return 0;
                return 1;
            }
            return count;
        }
        protected virtual int ItemMatching(List<Thing> things, ItemWishInfo item)
        {
            int count = 0;
            if (things.NullOrEmpty() || (Def.countAmountPerInfo && item.needAmount > things.Count)) return count;
            bool shouldContinue;
            for (int i = 0; i < things.Count; i++)
            {
                if (item.fromRessource != null
                        && (things[i].Stuff == null || !item.fromRessource.Contains(things[i].Stuff))) continue;
                if (item.neededComp != null
                    && ((item.neededComp.Contains(typeof(CompQuality))
                        && (things[i].TryGetComp<CompQuality>() == null || things[i].TryGetComp<CompQuality>().Quality < item.minQuality))
                    || (item.neededComp.Contains(typeof(CompArt))
                        && (things[i].TryGetComp<CompArt>() == null || !things[i].TryGetComp<CompArt>().Active)))) continue;
                shouldContinue = false;
                if (item.neededStats != null) for(int j = 0; j < item.neededStats.Count; j++)
                    {
                        if (things[i].GetStatValue(item.neededStats[j].def) < item.neededStats[j].minValue) 
                        {
                            shouldContinue = true;
                            break;
                        }
                    }
                if (shouldContinue) continue;
                count++;
                if (count >= item.needAmount) return count;
            }
            return count;
        }

        public override string FormateText(string text)
        {
            text = text.Replace(Def.amount_Key, Def.totalNeededAmount.ToString());
            text = text.Replace(Def.countRule_Key, (Def.countAmountPerInfo ? Def.perInfoRule.ToString() : Def.perUnitRule.ToString()));
            if (Def.roomRole != null) text = text.Replace(Def.role_Key, Def.roomRole.label);
            return base.FormateText(text);
        }
        protected override string FormateListThing(List<ItemWishInfo> things)
        {
            //return base.FormateListThing(things);
            string listingDefaultParam = "";
            if (things.NullOrEmpty()) return listingDefaultParam;
            string listingOwnParam = "";
            string elemSeparator = ", ";
            string bbSeparator = "\n";

            for (int i = 0; i < things.Count; i++)
            {
                if (SimilareToDefault(things[i]))
                {
                    if(listingDefaultParam == "") listingDefaultParam += ParamFor(things[i], elemSeparator) + " : \n";
                    listingDefaultParam += things[i].def.label + elemSeparator;
                }
                else
                {
                    listingOwnParam += things[i].def.label + ParamFor(things[i], elemSeparator) + bbSeparator;
                }
            }
            if (listingDefaultParam.EndsWith(elemSeparator)) listingDefaultParam = listingDefaultParam.Substring(0, listingDefaultParam.Length - elemSeparator.Length);
            if (listingOwnParam.EndsWith(bbSeparator)) listingOwnParam = listingOwnParam.Substring(0, listingOwnParam.Length - bbSeparator.Length);

            if (listingDefaultParam != "") 
            {
                if (listingOwnParam != "") listingDefaultParam += "\n\n" + listingOwnParam;
            }
            else listingDefaultParam += listingOwnParam;
            return listingDefaultParam;
        }
        private string ParamFor(ItemWishInfo info, string elemSeparator)
        {
            string neededSeparator = " // ";
            string minSymbole = " >= ";

            string paramDesc = "(x" + info.needAmount + ") (";
            if (!info.neededComp.NullOrEmpty()) for (int j = 0; j < info.neededComp.Count; j++)
                {
                    paramDesc += info.neededComp[j].Name.Substring("Comp".Length);
                    if (info.neededComp[j] == typeof(CompQuality)) paramDesc += minSymbole + info.minQuality.GetLabel();
                    paramDesc += (j != info.neededComp.Count - 1) ? elemSeparator : neededSeparator;
                }
            if (!info.neededStats.NullOrEmpty()) for (int j = 0; j < info.neededStats.Count; j++)
                {
                    paramDesc += info.neededStats[j].def.LabelCap + minSymbole + info.neededStats[j].minValue;
                    paramDesc += (j != info.neededStats.Count - 1) ? elemSeparator : neededSeparator;
                }
            if (!info.fromRessource.NullOrEmpty()) for (int j = 0; j < info.fromRessource.Count; j++)
                {
                    paramDesc += info.fromRessource[j].LabelAsStuff;
                    paramDesc += (j != info.fromRessource.Count - 1) ? elemSeparator : neededSeparator;
                }
            if (paramDesc.EndsWith(neededSeparator)) paramDesc = paramDesc.Substring(0, paramDesc.Length - neededSeparator.Length);
            paramDesc += ")";
            return paramDesc;
        }

        protected bool SimilareToDefault(ItemWishInfo info)
        {
            if (info.fromRessource != Def.fromRessource 
                || info.neededComp != Def.neededComp
                || info.neededStats != Def.neededStats
                || info.needAmount != Def.specificItemAmount
                || info.minQuality != Def.minQuality
                ) return false;
            return true;
        }

        public override string DescriptionToFulfill
        {
            get
            {
                return base.DescriptionToFulfill + (Def.totalNeededAmount > 1 ? " (" + itemCount.ToString() + "/" + Def.totalNeededAmount.ToString() + ")" : "");
            }
        }
    }
}
