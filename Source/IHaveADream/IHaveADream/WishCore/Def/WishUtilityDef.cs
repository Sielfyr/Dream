using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace HDream
{
    public class WishUtilityDef : Def
    {

        public float ChancePerHourToGetTimeWish;
        public float factorChancePerOtherTimeWish;

        public ThoughtDef noWishDepression;
        public float dayToGetNoWishDepression;
        public float dayToUpDepression;


        public string tierKeySingular;
        public string tierKeyPlural;

        public List<string> tierSingular;
        public List<string> tierPlural;
    }
}
