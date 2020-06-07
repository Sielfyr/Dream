using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace HDream
{
    public abstract class Wish_Thing<T>: WishWithComp
    {
        public ThingWishDef Def => (ThingWishDef)def;

        private List<T> thingsWanted = new List<T>();

        public List<T> ThingsWanted => thingsWanted;

        protected abstract LookMode ExposeLookModeT();
        protected abstract ThingDef GetThingDef(T thing);
        protected abstract List<T> GetThingsFromDef();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref thingsWanted, "thingsWanted", ExposeLookModeT());
        }

        public override void PostMake()
        {
            base.PostMake();
            List<T> items = GetThingsFromDef().ListFullCopyOrNull();

            if (items.NullOrEmpty())
            {
                MakeFailed();
                return;
            }

            if (def.wantSpecific)
            {
                if (def.tryPreventSimilare)
                {
                    List<ThingDef> similareThing = new List<ThingDef>();
                    ThingDef sThing;
                    List<Wish> wishes = pawn.wishes().wishes;
                    if(!wishes.NullOrEmpty())
                    {
                        for (int i = 0; i < wishes.Count; i++)
                        {
                            if (def != wishes[i].def) continue;
                            sThing = GetThingDef((wishes[i] as Wish_Thing<T>).ThingsWanted[0]);
                            if (items.Find(item => GetThingDef(item) == sThing) != null)
                                similareThing.Add(sThing);
                        }
                        if (similareThing.Count < items.Count) for (int i = 0; i < similareThing.Count; i++) items.RemoveAll(info => GetThingDef(info) == similareThing[i]);
                    }
                }
                thingsWanted.Add(items[Mathf.FloorToInt(Rand.Value * items.Count)]);
            }
            else thingsWanted = items;
        }


        public override string FormateText(string text)
        {
            
            // TODO To remove : Temporaire to prevent break save !
            if (thingsWanted.NullOrEmpty()) {
                pawn.wishes().wishes.Remove(this);
                PostRemoved();
                return "";
            }
            text = text.Replace(Def.amount_Key, Def.amountNeeded.ToString());
            text = text.Replace(Def.countRule_Key, (Def.countAmountPerInfo ? Def.perInfoRule.ToString() : Def.perUnitRule.ToString()));
            text = text.Replace(def.covetedObjects_Key, FormateListThing(thingsWanted));
            return base.FormateText(text);
        }


        protected virtual string FormateListThing(List<T> things)
        {
            string listing = "";
            for (int i = 0; i < things.Count; i++)
            {
                listing += GetThingDef(things[i]).label;
                if (i != things.Count - 1) listing += def.listing_Separator;
            }
            return listing;
        }

    }
}
