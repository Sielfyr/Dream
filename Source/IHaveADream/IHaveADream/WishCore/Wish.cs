using RimWorld;
using HarmonyLib;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace HDream
{
	public abstract class Wish : IExposable
	{

		public WishDef def;

		public Pawn pawn;

		public MemoryThoughtHandler Memories => pawn.needs.mood.thoughts.memories;
		public int TierIndex => def.tier.scale;

		public int pendingTicks;
		public int ageTicks;

		public int pendingCount;
		public int progressCount;
		public int regressCount;

		private string cachedLabel = null;
		private string cachedDesc = null;
		private string cachedDescFulfill = null;

		public virtual string FormateText(string text)
		{
			return WishUtility.FormateTierKeyword(text, TierIndex).Formatted(pawn.Named("Pawn"));
		}
		public virtual string LabelCap
		{
			get
			{
				if(cachedLabel == null) cachedLabel = FormateText(def.LabelCap);
				return cachedLabel;
			}
		}
		public virtual string DescriptionToFulfill
		{
			get
			{
				if (cachedDescFulfill == null) cachedDescFulfill = FormateText(def.description);
				return cachedDescFulfill;
			}
		}
		public virtual string DescriptionTitle
		{
			get
			{
				if (cachedDesc == null)
				{
					if (def.descriptions.NullOrEmpty()) cachedDesc = "";
					else cachedDesc = FormateText(def.descriptions[Mathf.FloorToInt(Rand.Value * def.descriptions.Count)]);
				}
				return cachedDesc;
			}
		}

		public Wish() { }

		public Wish(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public Wish(Pawn pawn, WishDef def)
		{
			this.pawn = pawn;
			this.def = def;
		}
		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref cachedLabel, "cachedLabel", null);
			Scribe_Values.Look(ref cachedDesc, "cachedDesc", null);
			Scribe_Values.Look(ref cachedDescFulfill, "cachedDescFulfill", null);
			Scribe_Values.Look(ref pendingTicks, "pendingTicks", 0);
			Scribe_Values.Look(ref ageTicks, "ageTicks", 0);
			Scribe_Values.Look(ref pendingCount, "pendingCount", 0);
			Scribe_Values.Look(ref progressCount, "progressCount", 0);
			Scribe_Values.Look(ref regressCount, "regressCount", 0);
		}

		public virtual void OnFulfill()
		{
			DoFulfill();
		}
		public virtual void DoFulfill()
		{
			if (def.fulfillTought != null)
			{
				if (def.fulfillTought.stages.Count > 1) Memories.TryGainMemory(ThoughtMaker.MakeThought(def.fulfillTought, TierIndex));
				else Memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(def.fulfillTought));
			}
			else Log.Warning("Wish " + def.label + " for pawn " + pawn.Label + " miss a fulfillTought in the Def, you should add one");
			pawn.wishes().wishes.Remove(this);
			PostRemoved();
		}

		protected virtual void DoPendingEffect()
		{
			pendingTicks++;
			if (pendingTicks >= (GenDate.TicksPerDay / def.upsetPerDay) * (pendingCount + 1))
			{
				Memories.TryGainMemory(ThoughtMaker.MakeThought(HDThoughtDefOf.WishPending, TierIndex));
				pendingCount++;
			}
			if (!def.IsPermanent() && ageTicks % GenDate.TicksPerHour == 0)
			{
				int ageHour = ageTicks / GenDate.TicksPerHour;
				if (Rand.Value <= def.endChancePerHour.Evaluate(ageHour))
				{
					DoTooOld();
				}
			}
		}
		public virtual void DoTooOld()
		{
			Memories.TryGainMemory(ThoughtMaker.MakeThought(HDThoughtDefOf.WishTimeFail, TierIndex));
			pawn.wishes().wishes.Remove(this);
			PostRemoved();
		}
		public virtual void ChangeProgress(int value)
		{
			if (value > 0)
			{
				for (int i = 0; i < value; i++)
				{
					if (regressCount > 0) RemoveOneMemoryOfDef(HDThoughtDefOf.WishRegressing, ref regressCount);
					else
					{
						progressCount++;
						if (def.progressAddThought) Memories.TryGainMemory(ThoughtMaker.MakeThought(HDThoughtDefOf.WishComingTrue, TierIndex));
						for (int j = 0; j < def.progressRemovePending; j++) RemoveOneMemoryOfDef(HDThoughtDefOf.WishPending, ref pendingCount);
					}
				}
			}
			else if (value < 0)
			{
				for (int i = 0; i > value; i--)
				{
					Memories.TryGainMemory(ThoughtMaker.MakeThought(HDThoughtDefOf.WishRegressing, TierIndex));
					regressCount++;
				}
			}
		}

		public virtual void PostAdd() 
		{
			Messages.Message("New " + WishUtility.Def.tierSingular[TierIndex] + "! " + pawn.LabelShort + " " + LabelCap + ".", MessageTypeDefOf.CautionInput);
		}
		public virtual void PostRemoved()
		{
			while (regressCount > 0) RemoveOneMemoryOfDef(HDThoughtDefOf.WishRegressing, ref regressCount);
			while (pendingCount > 0) RemoveOneMemoryOfDef(HDThoughtDefOf.WishPending, ref pendingCount);
			if (def.progressAddThought) while (progressCount > 0) RemoveOneMemoryOfDef(HDThoughtDefOf.WishComingTrue, ref progressCount);
		}
		public void RemoveOneMemoryOfDef(ThoughtDef thoughtDef, ref int count)
		{
			if (count <= 0)
			{
				//Log.Warning("HDream : try to remove a thougth of def :" + thoughtDef.label + " for wish : " + def.label + " but the count for that thought in that wish is already at 0 (pawn : " + pawn.Label + " )");
				return;
			}
			for (int i = 0; i < Memories.Memories.Count; i++)
			{
				Thought_Memory thought_Memory = Memories.Memories[i];
				if (thought_Memory.def == thoughtDef && thought_Memory.CurStageIndex == TierIndex)
				{
					Memories.RemoveMemory(thought_Memory);
					count--;
					return;
				}
			}
			Log.Warning("HDream : try to remove a thougth of def :" + thoughtDef.label + " for wish : " + def.label + " but no thought of that def was found for pawn : " + pawn.Label + "");
			count = 0;
		}
		public virtual void Tick() 
		{
			ageTicks++;
			DoPendingEffect();
		}
		public virtual void PostTick() { }
		public virtual void PostMake() { }

		protected virtual void MakeFailed()
		{
			Log.Warning("HDream : No possible object found for " + ToString() + " of def " + def.defName + " for pawn " + pawn.Label + ", it should not happen, it's bad, fix that ! Wish will be deleted!");
			pawn.wishes().wishes.Remove(this);
			PostRemoved();
		}
	}
}
