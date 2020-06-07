﻿using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace HDream
{
    public abstract class Wish_ThingOnMap<T> : Wish_Thing<T>
    {
        protected int thingCount = 0;
        protected int baseCount = 0;


        private int doAtTick = 0;
        public const int waitForTick = 300;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref thingCount, "itemCount", 0);
            Scribe_Values.Look(ref baseCount, "baseCount", 0);
            Scribe_Values.Look(ref doAtTick, "doAtTick", 0);
        }

        public override void PostMake()
        {
            base.PostMake();

            if (!Def.countPreWishProgress) baseCount = CountMatch();
        }

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame < doAtTick) return;
            doAtTick = Find.TickManager.TicksGame + waitForTick;
            CheckResolve();
        }

        protected abstract int ThingMatching(IEnumerable<Thing> things, T match);
        protected abstract int CountMatch();

        protected virtual void CheckResolve()
        {
            int count = CountMatch() - baseCount;
            if (count >= def.amountNeeded)
            {
                OnFulfill();
                return;
            }
            if (count != thingCount)
            {
                ChangeProgress(Mathf.FloorToInt(((float)count / def.amountNeeded) / (Def.progressStep * (progressCount + 1f))));
                thingCount = count;
            }
        }
        protected virtual int AdjustForSpecifiedCount(int count, int specificTotal)
        {
            if (Def.countAmountPerInfo)
            {
                if (count < specificTotal) return 0;
                return 1;
            }
            return count;
        }


        public override string DescriptionToFulfill
        {
            get
            {
                return base.DescriptionToFulfill + (Def.amountNeeded > 1 ? " (" + thingCount.ToString() + "/" + Def.amountNeeded.ToString() + ")" : "");
            }
        }
    }
}