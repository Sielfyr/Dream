using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace HDream
{
    public abstract class ThingWishDef : WishDef
    {

        public List<ThingDef> excludedThing;

        public bool findPossibleWant = false;



    }
}
