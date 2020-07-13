using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace HDream
{
    public class Wish_Equipment : Wish_Item
    {
        protected override int CountMatch()
        {
            int count = 0;
            List<ThingWithComps> equipment = pawn.equipment.AllEquipmentListForReading;
            for (int i = 0; i < ThingsWanted.Count; i++)
            {
                count += AdjustForSpecifiedCount(ThingMatching(equipment, ThingsWanted[i]), ThingsWanted[i].needAmount);
            }
            return count;
        }

    }
}
