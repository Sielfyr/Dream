using RimWorld;
using HarmonyLib;
using Verse;

namespace HDream
{
    public class Wish_WantHumanLikeFood : Wish_WantIngestible
    {

        protected override bool CorrectIngestibleEaten(Thing thing)
        {
            if(FoodUtility.IsHumanlikeMeatOrHumanlikeCorpse(thing)) return true;
            CompIngredients compIngredients = thing.TryGetComp<CompIngredients>();
            if (compIngredients != null)
            {
                for (int i = 0; i < compIngredients.ingredients.Count; i++)
                {
                    if (FoodUtility.IsHumanlikeMeat(compIngredients.ingredients[i])) return true;
                }
            }
            return base.CorrectIngestibleEaten(thing);
        }
    }
}
