using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace HDream
{
    public class Wish_ImproveRoom : WishWithComp
    {
        public RoomWishDef Def => (RoomWishDef)def;

        protected List<Room> AllRooms => pawn.Map.regionGrid.allRooms;

        private int doAtTick = 0;
        public const int waitForTick = 200;

        protected bool foundRoom = false;
        protected bool reached = false;
        protected bool unreach = false;

        protected float scoreToReach;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref doAtTick, "lastTick", 0);
            Scribe_Values.Look(ref scoreToReach, "scoreToReach", 0);
            Scribe_Values.Look(ref foundRoom, "foundRoom", false);
            Scribe_Values.Look(ref reached, "reached", false);
            Scribe_Values.Look(ref unreach, "unreach", false);
        }

        protected bool roomMatch(Room room)
        {
            return (Def.roomRole == null || room.Role == Def.roomRole) && (!Def.shoulBeOwner || room.Owners.Contains(pawn));
        }
        public override void PostMake()
        {
            base.PostMake();
            for (int i = 0; i < AllRooms.Count; i++)
            {
                if (roomMatch(AllRooms[i]))
                {
                    foundRoom = true;
                    break;
                }
            }
            scoreToReach = Def.GetScoreStage()?.minScore ?? -1;
        }
        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame < doAtTick) return;
            doAtTick = Find.TickManager.TicksGame + waitForTick;
            CheckProgress();
        }

        protected void CheckProgress()
        {
            List<Room> rooms = new List<Room>();
            for (int i = 0; i < AllRooms.Count; i++)
            {
                if (roomMatch(AllRooms[i])) rooms.Add(AllRooms[i]);
            }
            for (int i = 0; i < rooms.Count; i++)
            {
                if (Def.relatedStats == null || rooms[i].GetStatScoreStage(Def.relatedStats).minScore >= scoreToReach)
                {
                    reached = true;
                    if (unreach)
                    {
                        ChangeProgress(1);
                        unreach = false;
                    }
                    OnFulfill();
                    return;
                }
            }
            if (reached && !unreach)
            {
                ChangeProgress(-1);
                unreach = true;
            }
            if (!foundRoom && rooms.Count > 0)
            {
                foundRoom = true;
                ChangeProgress(1);
            }
            else if (foundRoom && rooms.Count == 0)
            {
                foundRoom = false;
                ChangeProgress(-1);
            }
        }
        public override string FormateText(string text)
        {
            if (scoreToReach >= 0) text = text.Replace(Def.stage_Key, Def.GetScoreStage().label);
            if (Def.roomRole != null)
            {
                text = text.Replace(Def.role_Key, Def.roomRole.label);
                text = text.Replace(Def.covetedObjects_Key, Def.roomRole.label);
            }
            return base.FormateText(text);
        }


    }
}
