using RimWorld;
using HarmonyLib;
using Verse;

namespace HDream
{
    public class Thought_Wish : Thought_Memory
    {

        public override bool GroupsWith(Thought other)
        {
            if (!base.GroupsWith(other)) return false;
            return CurStageIndex == other.CurStageIndex;
        }

        public override string LabelCap
        {
            get
            {
                return WishUtility.FormateTierKeyword(base.LabelCap, CurStageIndex);
            }
        }

        public override string Description
        {
            get
            {
                return WishUtility.FormateTierKeyword(base.Description, CurStageIndex);
            }
        }
    }
}
