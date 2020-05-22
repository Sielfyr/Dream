﻿using RimWorld;

namespace HDream
{
	[DefOf]
	public static class HDThoughtDefOf
	{
		public static ThoughtDef WishTimeFail;

		public static ThoughtDef WishPending;

		public static ThoughtDef WishComingTrue;

		public static ThoughtDef WishRegressing;

		public static ThoughtDef WishUnHold;

		public static ThoughtDef WishOnHold;

		static HDThoughtDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HDThoughtDefOf));
		}
	}
}
