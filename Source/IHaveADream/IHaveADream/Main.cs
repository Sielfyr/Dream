using RimWorld;
using HarmonyLib;
using Verse;

namespace HDream
{

    [StaticConstructorOnStartup]
    static class Main
    {

        public const string Id = "Sielfyr.IHaveADream";
        public const string ModName = "I Have A Dream";
        public const string Version = "0.2.0";
        static Main()
        {
            var harmony = new Harmony(Id);
            harmony.PatchAll();
            Log.Message("Initialized " + ModName + " v" + Version); 

        }
    }
}
