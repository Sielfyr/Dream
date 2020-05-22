using RimWorld;


namespace HDream
{
	[DefOf]
	public static class WishDefOf
	{

		public static FoodWishDef WantGoodFood;

		static WishDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(WishDefOf));
		}
	}
}
