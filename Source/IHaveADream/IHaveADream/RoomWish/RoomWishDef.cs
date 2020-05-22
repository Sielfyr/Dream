using RimWorld;
using HarmonyLib;
using Verse;
using System.Linq;
using UnityEngine;
using System;

namespace HDream
{
    public class RoomWishDef : WishDef
    {
        public RoomRoleDef roomRole;

        public RoomStatDef relatedStats;

        public int toNearestScoreStage;

        public bool shoulBeOwner = false;

        public string stage_Key = "{stage}";
        public string role_Key = "{role}";

        public RoomStatScoreStage GetScoreStage()
        {
            if (relatedStats == null) return null;
            for (int i = 0; i < relatedStats.scoreStages.Count; i++)
            {
                if (relatedStats.scoreStages[i].minScore >= toNearestScoreStage)
                {
                    if (relatedStats.scoreStages[i].minScore == toNearestScoreStage || i == 0) return relatedStats.scoreStages[i];
                    if (relatedStats.scoreStages[i].minScore - toNearestScoreStage < toNearestScoreStage - relatedStats.scoreStages[i - 1].minScore)
                         return relatedStats.scoreStages[i];
                    else return relatedStats.scoreStages[i - 1];
                }
            }
            return relatedStats.scoreStages[relatedStats.scoreStages.Count - 1];
        }
    }
}
