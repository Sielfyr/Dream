using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System;

namespace HDream
{
    public class Pawn_WishTracker : IExposable
    {

        public Pawn pawn;

        public List<Wish> wishes = new List<Wish>();

        private DefMap<WishDef, float> wishChances = new DefMap<WishDef, float>();

        public int tickWithoutWish = 0;

        private int depressionTick = 0;


        public Pawn_WishTracker(Pawn pawn)
        {
            this.pawn = pawn;
        }
        public void ExposeData()
        {

            Scribe_Collections.Look(ref wishes, "wishes", LookMode.Deep);
            Scribe_Values.Look(ref tickWithoutWish, "tickWithoutWish", 0);
            Scribe_Values.Look(ref depressionTick, "depressionTick", 0);
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                for (int i = 0; i < wishes.Count; i++)
                {
                    wishes[i].pawn = pawn;
                }
            }
        }
        public void Clear()
        {
            wishes.Clear();
        }

        public void WishesTick()
        {
            if(!WishUtility.CanHaveWish(pawn)) return;
            for (int i = wishes.Count - 1; i >= 0; i--)
            {
                Wish wish = wishes[i];
                wish.Tick();
                wish.PostTick();
            }
            if (pawn.IsHashIntervalTick(GenDate.TicksPerHour))
            {
                if (Rand.Value <= WishUtility.ChancePerHourToGetNewWish(pawn))
                {
                    Wish wish = TryGiveWish();
                    if (wish != null)
                    {
                        wish.PostAdd();
                        if (wishes.Contains(wish) && depressionTick > 0)
                        {
                            pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(WishUtility.Def.noWishDepression);
                            depressionTick = 0;
                        }
                    }
                }
            }
            NoWishTime();
        }

        public void NoWishTime()
        {
            if (wishes.Count == 0) tickWithoutWish++;
            else tickWithoutWish = 0;
            if(tickWithoutWish >= GenDate.TicksPerDay * ( WishUtility.Def.dayToGetNoWishDepression + depressionTick * WishUtility.Def.dayToUpDepression))
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(WishUtility.Def.noWishDepression, 0));
                depressionTick++;
            }
        }

        public void TryResolveIngestible(Thing thing, int amount, float nutriment)
        {
            for (int i = 0; i < wishes.Count; i++)
            {
                if (wishes[i] is Wish_WantIngestible && (wishes[i] as Wish_WantIngestible).CkeckResolve(thing, amount, nutriment))
                {
                    FulfilledWish(wishes[i]);
                }
            }
        }
        public void FulfilledWish(Wish wish)
        {
            if (!wishes.Contains(wish))
            {
                Log.Error("Try to resolve wish " + wish.ToString() + " that don't exist for " + pawn.ToString());
                return;
            }
            wish.OnFulfill();
        }
        public Wish TryGiveWish()
        {
            if (!WishUtility.CanHaveWish(pawn)) return null;
            List<WishDef> allDefsListForReading = DefDatabase<WishDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                WishDef wishDef = allDefsListForReading[i];
                wishChances[wishDef] = wishDef.GetChance(pawn);

            }

            for (int j = 0; j < wishChances.Count; j++)
            {
                if (!allDefsListForReading.TryRandomElementByWeight((WishDef d) => wishChances[d], out WishDef result))
                {
                    break;
                }
                Wish wish = TryGiveWishFromWishDefDirect(result, pawn);
                if (wish != null) return wish;
                wishChances[result] = 0f;
            }
            return null;
        }
        private Wish TryGiveWishFromWishDefDirect(WishDef def, Pawn pawn)
        {
            Wish wish = (Wish)Activator.CreateInstance(def.wishClass);
            wish.def = def;
            wish.pawn = pawn;
            wishes.Add(wish);
            wish.PostMake();
            return wishes.Contains(wish) ? wish : null;
        }
    }
}
