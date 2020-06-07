using RimWorld;
using HarmonyLib;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace HDream
{
    public class Wish_WantIngestible : Wish_Thing<ThingDef>
	{
		public new IngestibleWishDef Def => (IngestibleWishDef)def;
		public float AmountNeeded => Def.amountNeeded;

		float amountIngested = 0;

		protected override List<ThingDef> GetThingsFromDef() => Def.Ingestibles;
		protected override ThingDef GetThingDef(ThingDef thing) => thing;
		protected override LookMode ExposeLookModeT() => LookMode.Undefined;

		public virtual bool CkeckResolve(Thing thing, int amount, float nutriment)
		{
			if (!CorrectIngestibleEaten(thing)) return false;
			amountIngested += Def.checkPerNutriment ? nutriment : amount;
			if(amountIngested >= AmountNeeded) return true; 
			ChangeProgress(Mathf.FloorToInt((amountIngested / AmountNeeded) / (Def.progressStep * (progressCount + 1f))));
			return false;
		}

		protected virtual bool CorrectIngestibleEaten(Thing thing)
		{
			return ThingsWanted.Contains(thing.def);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref amountIngested, "amountIngested", 0);
		}


		public override string FormateText(string text)
		{
			text = text.Replace(Def.amount_Key, AmountNeeded.ToString());
			return base.FormateText(text);
		}

		public override string DescriptionToFulfill
		{
			get
			{
				return base.DescriptionToFulfill + " (" + amountIngested.ToString() + "/" + AmountNeeded.ToString() + ")";
			}
		}
	}
}
