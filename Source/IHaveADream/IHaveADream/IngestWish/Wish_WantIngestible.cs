using RimWorld;
using HarmonyLib;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace HDream
{
    public class Wish_WantIngestible : Wish
    {
		public IngestibleWishDef Def => (IngestibleWishDef)def;

		private List<ThingDef> ingestiblesWanted = new List<ThingDef>();

		public List<ThingDef> IngestiblesWanted => ingestiblesWanted;

		public float AmountNeeded => Def.amountNeeded;

		float amountIngested = 0;

		public virtual bool CkeckResolve(Thing thing, int amount, float nutriment)
		{
			if (!CorrectIngestibleEaten(thing)) return false;
			amountIngested += Def.checkPerNutriment ? nutriment : amount;
			if(amountIngested >= AmountNeeded) return true;
			ChangeProgress(Mathf.FloorToInt((amountIngested / AmountNeeded) / (Def.progressStep * progressCount + 1)));
			return false;
		}

		protected virtual bool CorrectIngestibleEaten(Thing thing)
		{
			return ingestiblesWanted.Contains(thing.def);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref amountIngested, "amountIngested", 0);
			Scribe_Collections.Look(ref ingestiblesWanted, "ingestiblesWanted");
		}

		public override void PostMake()
		{
			base.PostMake();
			List<ThingDef> ingestibles = new List<ThingDef>(Def.Ingestibles);

			if (ingestibles.Count == 0)
			{
				MakeFailed();
				return;
			}

			if (Def.wantSpecific)
			{
				if (def.tryPreventSimilare)
				{
					List<ThingDef> similareIngestibles = new List<ThingDef>();
					List<Wish> wishes = pawn.wishes().wishes;
					for (int i = 0; i < wishes.Count; i++)
					{
						if(wishes[i].def == def && ingestibles.Contains((wishes[i] as Wish_WantIngestible).IngestiblesWanted[0])) 
							similareIngestibles.Add((wishes[i] as Wish_WantIngestible).IngestiblesWanted[0]);
					}
					if(similareIngestibles.Count < ingestibles.Count) for (int i = 0; i < similareIngestibles.Count; i++) ingestibles.RemoveAll(thing => thing == similareIngestibles[i]);
				}
				ingestiblesWanted.Add(ingestibles[Mathf.FloorToInt(Rand.Value * ingestibles.Count)]);
			}
			else ingestiblesWanted = ingestibles;
		}

		public override string FormateText(string text)
		{
			text = text.Replace(Def.amount_Key, AmountNeeded.ToString());
			string listing = "";
			for (int i = 0; i < ingestiblesWanted.Count; i++)
			{
				listing += ingestiblesWanted[i].label;
				if (i != ingestiblesWanted.Count - 1) listing += ", ";
			}
			text = text.Replace(Def.covetedObjects_Key, listing);
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
