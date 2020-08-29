using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System;
using System.Linq;

namespace HDream
{
    public class WishDef : Def
    {

        public List<string> descriptions;

        public WishTierDef tier;

        public Type wishClass = typeof(Wish);

        public WishCategory category;

        public List<WishCompProperties> comps;

        public ThoughtDef fulfillTought;

        public float upsetPerDay;
        public bool progressAddThought = true;
        public int progressRemovePending = 0;

        public List<WishTraitFactor> traitFactor;
        public List<WishPassionFactor> passionFactor;
        public List<WishIncapableFactor> incapableFactor;

        public SimpleCurve endChancePerHour;

        protected float baseChance;

        public int minimunAge = -1;

        public int maxCount = 1;
        public float countChanceFactor = 1;

        public bool wantSpecific = false;

        public bool tryPreventSimilare = true;

        public bool countPreWishProgress = true;

        public float amountNeeded = 1;

        public float progressStep = 1f;

        public string amount_Key = "{Amount}";
        public string covetedObjects_Key = "{Objects}";
        public string listing_Separator = ", ";
        public bool IsPermanent()
        {
            return endChancePerHour == null;
        }
        protected virtual float GetChance(Pawn pawn, float chance)
        {
            int count = pawn.wishes().wishes.Where(wish => wish.def == this).Count();
            if (count >= maxCount || pawn.ageTracker.AgeBiologicalYears < minimunAge) return 0;
            for (int i = 0; i < count; i++) chance *= countChanceFactor;
            
            if (!traitFactor.NullOrEmpty())
            {
                for (int i = 0; i < traitFactor.Count; i++)
                {

                    if (pawn.story.traits.HasTrait(traitFactor[i].trait) && (!traitFactor[i].needDegree || traitFactor[i].degree == pawn.story.traits.DegreeOfTrait(traitFactor[i].trait)))
                        chance *= traitFactor[i].factor;
                }
            }
            if (!incapableFactor.NullOrEmpty())
            {
                for (int i = 0; i < incapableFactor.Count; i++)
                {
                    if(pawn.WorkTypeIsDisabled(incapableFactor[i].workType))
                        chance *= incapableFactor[i].factor;
                }
            }
            if (!passionFactor.NullOrEmpty())
                {
                for (int i = 0; i < passionFactor.Count; i++)
                {
                    switch (pawn.skills.GetSkill(passionFactor[i].skill).passion)
                    {
                        case Passion.Minor:
                            chance *= passionFactor[i].minorPassionFactor;
                            break;
                        case Passion.Major:
                            chance *= passionFactor[i].majorPassionFactor;
                            break;
                    }
                }
            }

            chance *= tier.GetExpectationFactor(ExpectationsUtility.CurrentExpectationFor(pawn));

            return chance;
        }
        public float GetChance(Pawn pawn)
        {
            return GetChance(pawn, baseChance);
        }
    }
}
