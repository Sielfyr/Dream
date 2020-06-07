using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HDream
{
    public class Wish_Item : Wish_ThingOnMap<ItemWishInfo>
    {
        public new ItemWishDef Def => (ItemWishDef)def;
        protected override List<ItemWishInfo> GetThingsFromDef() => Def.Items;
        protected override ThingDef GetThingDef(ItemWishInfo thing) => thing.def;
        protected override LookMode ExposeLookModeT() => LookMode.Deep;

        protected override int CountMatch()
        {
            int count = 0;
            if (Def.roomRole != null)
            {
                List<Room> rooms = pawn.Map.regionGrid.allRooms.FindAll(room => room.Role == Def.roomRole && (!Def.shoulBeRoomOwner || room.Owners.Contains(pawn)));
                for (int i = 0; i < rooms.Count; i++)
                {
                    for (int j = 0; j < ThingsWanted.Count; j++)
                    {
                        count += AdjustForSpecifiedCount(ThingMatching(rooms[i].ContainedThings(ThingsWanted[j].def).ToList(), ThingsWanted[j]), ThingsWanted[j].needAmount);
                    }
                }
                return count;
            }
            else
            {
                ListerThings lister = pawn.Map.listerThings;
                for (int i = 0; i < ThingsWanted.Count; i++)
                {
                    count += AdjustForSpecifiedCount(ThingMatching(lister.ThingsOfDef(GetThingDef(ThingsWanted[i])), ThingsWanted[i]), ThingsWanted[i].needAmount);
                }
            }
            return count;
        }
        protected override int ThingMatching(IEnumerable<Thing> things, ItemWishInfo match)
        {
            int count = 0;
            if (things.EnumerableNullOrEmpty() || (Def.countAmountPerInfo && match.needAmount > things.Count())) return count;
            bool shouldContinue;
            for (int i = 0; i < things.Count(); i++)
            {
                if (match.fromRessource != null
                        && (things.ElementAt(i).Stuff == null || !match.fromRessource.Contains(things.ElementAt(i).Stuff))) continue;
                if (match.neededComp != null
                    && ((match.neededComp.Contains(typeof(CompQuality))
                        && (things.ElementAt(i).TryGetComp<CompQuality>() == null || things.ElementAt(i).TryGetComp<CompQuality>().Quality < match.minQuality))
                    || (match.neededComp.Contains(typeof(CompArt))
                        && (things.ElementAt(i).TryGetComp<CompArt>() == null || !things.ElementAt(i).TryGetComp<CompArt>().Active)))) continue;
                shouldContinue = false;
                if (match.neededStats != null) for(int j = 0; j < match.neededStats.Count; j++)
                    {
                        if (things.ElementAt(i).GetStatValue(match.neededStats[j].def) < match.neededStats[j].minValue) 
                        {
                            shouldContinue = true;
                            break;
                        }
                    }
                if (shouldContinue) continue;
                count++;
                if (count >= match.needAmount) return count;
            }
            return count;
        }

        public override string FormateText(string text)
        {
            if (Def.roomRole != null) text = text.Replace(Def.role_Key, Def.roomRole.label);
            return base.FormateText(text);
        }
        protected override string FormateListThing(List<ItemWishInfo> things)
        {
            if (things.NullOrEmpty()) return base.FormateListThing(things);
            string listingDefaultParam = "";
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
                || info.needAmount != Def.specificAmount
                || info.minQuality != Def.minQuality
                ) return false;
            return true;
        }

    }
}
