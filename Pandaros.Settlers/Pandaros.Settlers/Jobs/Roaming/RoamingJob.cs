﻿using Jobs;
using NPC;
using Pipliz;
using Shared;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Jobs.Roaming
{
    [ModLoader.ModManager]
    public static class RoamingJobRegister
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCDied, GameLoader.NAMESPACE + ".Jobs.Roaming.RoamingJobRegister.OnDeath")]
        public static void OnDeath(NPCBase nPC)
        {
            if (nPC.Job != null && nPC.Job.GetType() == typeof(RoamingJob) && ((RoamingJob)nPC.Job).TargetObjective != null)
                ((RoamingJob)nPC.Job).TargetObjective.JobRef = null;
        }
    }

    public abstract class RoamingJob : BlockJobInstance
    {
        protected float cooldown = 2f;
        private const float COOLDOWN = 3f;
        private int _stuckCount;
        

        public RoamingJob(IBlockJobSettings settings, Vector3Int position, ItemTypes.ItemType type, ByteReader reader) :
            base(settings, position, type, reader)
        {
            OriginalPosition = position;
        }

        public RoamingJob(IBlockJobSettings settings, Vector3Int position, ItemTypes.ItemType type, Colony colony) :
            base(settings, position, type, colony)
        {
            OriginalPosition = position;
        }

        public virtual List<uint> OkStatus { get; } = new List<uint>();
        public RoamingJobState TargetObjective { get; set; }
        public RoamingJobState PreviousObjective { get; set; }
        public Vector3Int OriginalPosition { get; private set; }
        public virtual string JobItemKey => null;
        public virtual List<string> ObjectiveCategories => new List<string>();
        public virtual int ActionsPreformed { get; set; }

        public override Vector3Int GetJobLocation()
        {
            var pos = OriginalPosition;

            if (TargetObjective == null)
            {
                if (RoamingJobManager.Objectives.TryGetValue(Owner, out var roamingJobObjectiveDic))
                    foreach (var objective in roamingJobObjectiveDic.Values.Where(m => m.CanBeWorked(ObjectiveCategories)))
                        if (objective != PreviousObjective && objective.PositionIsValid())
                        {
                            var dis = UnityEngine.Vector3.Distance(objective.Position.Vector, pos.Vector);

                            if (dis <= 21)
                            {
                                var action = objective.ActionEnergy.FirstOrDefault(a => a.Value < .5f);
                                if (action.Key != null)
                                {
                                    TargetObjective = objective;
                                    TargetObjective.JobRef = this;

                                    pos = TargetObjective.Position.GetClosestPositionWithinY(NPC.Position, 5);
                                    break;
                                }
                            }
                        }
            }
            else
            {
                pos = TargetObjective.Position.GetClosestPositionWithinY(NPC.Position, 5);
            }

            return pos;
        }

        public override void OnNPCAtJob(ref NPCBase.NPCState state)
        {
            var status        = GameLoader.Waiting_Icon;
            var cooldown      = COOLDOWN;
            var allActionsComplete = false;

            if (TargetObjective != null && NPC != null)
            {
                NPC.LookAt(TargetObjective.Position.Vector);
                bool actionFound = false;

                foreach (var action in new Dictionary<string, float>(TargetObjective.ActionEnergy))
                    if (action.Value < .5f)
                    {
                        status   = TargetObjective.RoamingJobSettings.ActionCallbacks[action.Key].PreformAction(Owner, TargetObjective);
                        cooldown = TargetObjective.RoamingJobSettings.ActionCallbacks[action.Key].TimeToPreformAction;
                        AudioManager.SendAudio(TargetObjective.Position.Vector, TargetObjective.RoamingJobSettings.ActionCallbacks[action.Key].AudoKey);
                        actionFound = true;
                    }

                if (!actionFound)
                {
                    PreviousObjective = null;
                    TargetObjective.JobRef = null;
                    TargetObjective = null;
                    allActionsComplete = true;
                }
            }

            // if the objective is gone, Abort.
            CheckIfValidObjective();

            if (OkStatus.Contains(status))
            {
                ActionsPreformed++;
                _stuckCount = 0;
            }
            else if (status != 0)
                _stuckCount++;

            if (_stuckCount > 5 || TargetObjective == null)
            {
                state.JobIsDone = true;
                status          = GameLoader.Waiting_Icon;

                if (allActionsComplete)
                    cooldown = 0.5f;

                if (_stuckCount > 5)
                {
                    PreviousObjective         = TargetObjective;
                    TargetObjective.JobRef    = null;
                    TargetObjective           = null;
                }
            }

            if (OkStatus.Contains(status))
                state.SetIndicator(new IndicatorState(cooldown, status));
            else if (status != 0)
                state.SetIndicator(new IndicatorState(cooldown, status, true));
            else
                state.SetIndicator(new IndicatorState(cooldown, ColonyBuiltIn.ItemTypes.MISSINGERROR.Name));

            state.SetCooldown(cooldown);
        }

        private void CheckIfValidObjective()
        {
            if (TargetObjective != null && !TargetObjective.PositionIsValid())
            {
                TargetObjective.JobRef = null;
                TargetObjective           = null;
            }
        }

        public override NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            return CalculateGoal(ref state, true);
        }

        public override void OnNPCAtStockpile(ref NPCBase.NPCState state)
        {
            ActionsPreformed = 0;
            base.OnNPCAtStockpile(ref state);
        }

        public NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state, bool sleepAtNight)
        {
            var nPCGoal = NPCBase.NPCGoal.Job;

            if (ActionsPreformed > 6)
                nPCGoal = NPCBase.NPCGoal.Stockpile;
            else if (sleepAtNight && !TimeCycle.IsDay)
                nPCGoal = NPCBase.NPCGoal.Bed;
            else if (TimeCycle.IsDay && !sleepAtNight)
                nPCGoal = NPCBase.NPCGoal.Bed;

            if (nPCGoal != LastNPCGoal)
            {
                Settings.OnGoalChanged(this, LastNPCGoal, nPCGoal);
                LastNPCGoal = nPCGoal;
            }

            return nPCGoal;
        }
       
    }
}