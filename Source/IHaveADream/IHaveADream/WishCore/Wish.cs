﻿using RimWorld;
using HarmonyLib;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

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
		public int upsetTicks;

		public int pendingCount;
		public int progressCount;
		public int regressCount;

		private string cachedLabel = null;
		private string cachedDesc = null;
		private string cachedDescFulfill = null;

		protected int startDayEndChance = -1;


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
				return cachedDescFulfill + "\n" + DescriptionTime;
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

		public virtual string DescriptionTime
		{
			get
			{
				int ageDay = ageTicks / GenDate.TicksPerDay;
				return "\n" + "This wish has been expressed " + ageDay.ToString() + " day(s) ago."
					+ (startDayEndChance != -1 ?
						(ageDay < startDayEndChance ?
							"\n" + "This wish can't end before " + (startDayEndChance - ageDay).ToString() + " day(s)." :
							"\n" + "This wish can potentially end.") 
						: "");
			}
		}

		public Wish() {}

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
			//Scribe_Values.Look(ref cachedLabel, "cachedLabel", null);
			//Scribe_Values.Look(ref cachedDesc, "cachedDesc", null);
			//Scribe_Values.Look(ref cachedDescFulfill, "cachedDescFulfill", null);
			Scribe_Values.Look(ref pendingTicks, "pendingTicks", 0);
			Scribe_Values.Look(ref ageTicks, "ageTicks", 0);
			Scribe_Values.Look(ref upsetTicks, "upsetTicks", 0);
			Scribe_Values.Look(ref pendingCount, "pendingCount", 0);
			Scribe_Values.Look(ref progressCount, "progressCount", 0);
			Scribe_Values.Look(ref regressCount, "regressCount", 0);
			Scribe_Values.Look(ref regressCount, "regressCount", 0);
			Scribe_Values.Look(ref startDayEndChance, "startDayEndChance", -1);

			// Todo : to remove it init startDayEndChance for previous save that didn't had it
			if (startDayEndChance == -1 && !def.endChancePerHour.EnumerableNullOrEmpty())
			{
				def.endChancePerHour.SortPoints();
				for (int i = 0; i < def.endChancePerHour.Count(); i++)
				{
					if (def.endChancePerHour[i].y == 0 && def.endChancePerHour[i + 1].y > 0)
						startDayEndChance = Mathf.FloorToInt(def.endChancePerHour[i].x / GenDate.HoursPerDay);
				}
			}
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
			if (pendingTicks >= (GenDate.TicksPerDay / def.upsetPerDay) * (upsetTicks + 1))
			{
				// Todo : remove the if part and keep the else without condition
				// it fix a bug from older version with keeping save fine
				if(pendingTicks >= (GenDate.TicksPerDay / def.upsetPerDay) * (upsetTicks + 2))
				{
					while (pendingTicks >= (GenDate.TicksPerDay / def.upsetPerDay) * (upsetTicks + 1))
					{
						upsetTicks++;
						RemoveOneMemoryOfDef(HDThoughtDefOf.WishPending, ref pendingCount);
					}
				}
				else
				{
					// part to keep, move it outside the else when do the TODO
					Memories.TryGainMemory(ThoughtMaker.MakeThought(HDThoughtDefOf.WishPending, TierIndex));
					pendingCount++;
					upsetTicks++;
					//-//
				}
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
			Messages.Message("New " + WishUtility.Def.tierSingular[TierIndex] + "! " + pawn.LabelShort + " " + def.label.Substring(0, 1) + LabelCap.Substring(1) + ".", new LookTargets(pawn), MessageTypeDefOf.CautionInput);
			if (!def.endChancePerHour.EnumerableNullOrEmpty())
			{
				def.endChancePerHour.SortPoints();
				for (int i = 0; i < def.endChancePerHour.Count(); i++)
				{
					if (def.endChancePerHour[i].y == 0 && def.endChancePerHour[i + 1].y > 0)
						startDayEndChance = Mathf.FloorToInt(def.endChancePerHour[i].x / GenDate.HoursPerDay);
				}
			}
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
			if(pawn.wishes().wishes.Contains(this)) pawn.wishes().wishes.Remove(this);
			PostRemoved();
		}
	}
}
