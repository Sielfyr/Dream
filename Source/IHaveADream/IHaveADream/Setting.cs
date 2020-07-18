using RimWorld;
using HarmonyLib;
using Verse;
using UnityEngine;
using System.Collections.Generic;

namespace HDream
{
    public class SettingProperty : ModSettings
    {

        public float wishFrequencyFactor = 1f;
        public float wishPendingDebuffFactor = 1f;
        public float wishPendindStackMultiplierOffset = 0f;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref wishFrequencyFactor, "wishFrequencyFactor", 1f);
            Scribe_Values.Look(ref wishPendingDebuffFactor, "wishPendingFactor", 1f);
            Scribe_Values.Look(ref wishPendindStackMultiplierOffset, "wishPendindStackMultiplierOffset", 0f);
            base.ExposeData();
        }

    }



    public class SettingMenu : Mod
    {
        public static SettingProperty settings;

        public SettingMenu(ModContentPack content) : base(content)
        {
            settings = GetSettings<SettingProperty>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("\n");
            listingStandard.Label("Factor the chance to get a wish per hour and the time to get the no wish debuff");
            settings.wishFrequencyFactor = Mathf.Round(listingStandard.Slider(settings.wishFrequencyFactor, 0.2f, 5f) * 100) / 100;
            listingStandard.Label("Your setting : " + settings.wishFrequencyFactor.ToString() + " ( Default is 1 )");
            listingStandard.Label("\n\n");
            listingStandard.Label("Reducing these values risk to transform this mod in a free mood mod. Try to keep the default setting even if the debuff could look high sometime. Breakdown are part of the game.");
            listingStandard.Label("Factor the wish debuff you get other time.");
            settings.wishPendingDebuffFactor = Mathf.Round(listingStandard.Slider(settings.wishPendingDebuffFactor, 0.2f, 2f) * 100) / 100;
            listingStandard.Label("Your setting : " + settings.wishPendingDebuffFactor.ToString() + " ( Default is 1 )");
            listingStandard.Label("An offset on the pending stack multiplier who default setting is 0.92. Even a sligth chance can have a huge impact on the total mood debuff.");
            settings.wishPendindStackMultiplierOffset = Mathf.Round(listingStandard.Slider(settings.wishPendindStackMultiplierOffset, -0.2f, 0.08f) * 1000) / 1000;
            listingStandard.Label("Your setting : " + settings.wishPendindStackMultiplierOffset.ToString() + " ( Default is 0 )");
            listingStandard.End();
            UpdatePendingDef();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return Main.ModName;
        }


        private static float basePendindStackMultiplier = -1f;
        private static List<float> baseMoodDef = null;
        public static void UpdatePendingDef()
        {
            if (basePendindStackMultiplier == -1) basePendindStackMultiplier = HDThoughtDefOf.WishPending.stackedEffectMultiplier;
            if (baseMoodDef == null)
            {
                baseMoodDef = new List<float>();
                for (int i = 0; i < HDThoughtDefOf.WishPending.stages.Count; i++)
                {
                    baseMoodDef.Add(HDThoughtDefOf.WishPending.stages[i].baseMoodEffect);
                }
            }
            HDThoughtDefOf.WishPending.stackedEffectMultiplier = basePendindStackMultiplier + settings.wishPendindStackMultiplierOffset;
            for (int i = 0; i < baseMoodDef.Count; i++)
            {
                HDThoughtDefOf.WishPending.stages[i].baseMoodEffect = baseMoodDef[i] * settings.wishPendingDebuffFactor;
            }
        }
    }
}
