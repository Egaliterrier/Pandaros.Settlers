﻿using BlockTypes;
using Jobs;
using NPC;
using Pandaros.Settlers.NBT;
using Pipliz;
using Pipliz.Mods.BaseGame.Construction;
using Recipes;
using Shared;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Jobs.Construction
{
    [ModLoader.ModManager]
    public class SchematicBuilder : IConstructionType
    {
        private static List<SchematicIterator> _needsChunkLoaded = new List<SchematicIterator>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnShouldKeepChunkLoaded, GameLoader.NAMESPACE + ".Jobs.Construction.SchematicBuilder.OnShouldKeepChunkLoaded")]
        public static void OnShouldKeepChunkLoaded(ChunkUpdating.KeepChunkLoadedData data)
        {
            foreach (var iterator in _needsChunkLoaded)
            {
                if (iterator.CurrentPosition.IsWithinBounds(data.CheckedChunk.Position, data.CheckedChunk.Bounds))
                    data.Result = true;
            }
        }

        public int OnStockpileNewItemCount => 1;

        public void DoJob(IIterationType iterationType, IAreaJob areaJob, ConstructionJobInstance job, ref NPCBase.NPCState state)
        {
            int i = 0;
            var bpi = iterationType as SchematicIterator;

            if (bpi == null)
            {
                PandaLogger.Log(ChatColor.yellow, "iterationType must be of type SchematicIterator for the SchematicBuilder.");
                state.SetIndicator(new Shared.IndicatorState(5f, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                AreaJobTracker.RemoveJob(areaJob);
                return;
            }

            while (true) // This is to move past air.
            {
                if (i > 4000)
                    break;

                var adjX = iterationType.CurrentPosition.x - bpi.BuilderSchematic.StartPos.x;
                var adjY = iterationType.CurrentPosition.y - bpi.BuilderSchematic.StartPos.y;
                var adjZ = iterationType.CurrentPosition.z - bpi.BuilderSchematic.StartPos.z;
                var block = bpi.BuilderSchematic.GetBlock(adjX, adjY, adjZ);
                var mapped = block.MappedBlock;
                var buildType = ItemTypes.GetType(mapped.CSIndex);

                if (buildType == null)
                {
                    state.SetIndicator(new Shared.IndicatorState(5f, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                    AreaJobTracker.RemoveJob(areaJob);
                    return;
                }

                if (World.TryGetTypeAt(iterationType.CurrentPosition, out ushort foundTypeIndex))
                {
                    i++;

                    if (foundTypeIndex == buildType.ItemIndex) // check if the blocks are the same, if they are, move past. Most of the time this will be air.
                        if (iterationType.MoveNext())
                            continue;
                        else
                        {
                            if (_needsChunkLoaded.Contains(bpi))
                                _needsChunkLoaded.Remove(bpi);

                            state.SetIndicator(new Shared.IndicatorState(5f, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                            AreaJobTracker.RemoveJob(areaJob);
                            return;
                        }

                    Stockpile ownerStockPile = areaJob.Owner.Stockpile;

                    bool ok = buildType.ItemIndex == ColonyBuiltIn.ItemTypes.AIR.Id;

                    if (!ok && ownerStockPile.Contains(buildType.ItemIndex))
                        ok = true;

                    if (!ok && !string.IsNullOrWhiteSpace(buildType.ParentType))
                    {
                       var parentType = ItemTypes.GetType(buildType.ParentType);

                        if (!ok && ownerStockPile.Contains(parentType.ItemIndex))
                            ok = true;
                    }

                    if (ok)
                    {
                        if (foundTypeIndex != ColonyBuiltIn.ItemTypes.AIR.Id && foundTypeIndex != ColonyBuiltIn.ItemTypes.WATER.Id)
                        {
                            var foundItem = ItemTypes.GetType(foundTypeIndex);

                            if (foundItem != null && foundItem.ItemIndex != ColonyBuiltIn.ItemTypes.AIR.Id && foundItem.OnRemoveItems != null && foundItem.OnRemoveItems.Count > 0)
                                ownerStockPile.Add(foundItem.OnRemoveItems.Select(itm => itm.item).ToList());
                        }

                        if (ServerManager.TryChangeBlock(iterationType.CurrentPosition, foundTypeIndex, buildType.ItemIndex, areaJob.Owner, ESetBlockFlags.DefaultAudio) == EServerChangeBlockResult.Success)
                        {
                            if (buildType.ItemIndex != ColonyBuiltIn.ItemTypes.AIR.Id)
                            {
                                if (--job.StoredItemCount <= 0)
                                {
                                    job.ShouldTakeItems = true;
                                    state.JobIsDone = true;
                                }

                                ownerStockPile.TryRemove(buildType.ItemIndex);
                            }
                        }
                        else
                        {
                            if (!_needsChunkLoaded.Contains(bpi))
                                _needsChunkLoaded.Add(bpi);

                            state.SetIndicator(new Shared.IndicatorState(5f, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                            ChunkQueue.QueuePlayerSurrounding(iterationType.CurrentPosition);
                            return;
                        }
                    }
                }
                else
                {
                    if (!_needsChunkLoaded.Contains(bpi))
                        _needsChunkLoaded.Add(bpi);

                    state.SetIndicator(new Shared.IndicatorState(5f, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                    ChunkQueue.QueuePlayerSurrounding(iterationType.CurrentPosition);
                    return;
                }


                if (iterationType.MoveNext())
                {
                    if (buildType.ItemIndex != ColonyBuiltIn.ItemTypes.AIR.Id)
                        state.SetIndicator(new Shared.IndicatorState(GetCooldown(), buildType.ItemIndex));
                    else
                        state.SetIndicator(new Shared.IndicatorState(GetCooldown(), foundTypeIndex));

                    return;
                }
                else
                {
                    if (_needsChunkLoaded.Contains(bpi))
                        _needsChunkLoaded.Remove(bpi);

                    // failed to find next position to do job at, self-destruct
                    state.SetIndicator(new Shared.IndicatorState(5f, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                    AreaJobTracker.RemoveJob(areaJob);
                    return;
                }
            }

            if (iterationType.MoveNext())
            {
                state.SetIndicator(new Shared.IndicatorState(5f, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                return;
            }
            else
            {
                if (_needsChunkLoaded.Contains(bpi))
                    _needsChunkLoaded.Remove(bpi);

                // failed to find next position to do job at, self-destruct
                state.SetIndicator(new Shared.IndicatorState(5f, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                AreaJobTracker.RemoveJob(areaJob);
                PandaLogger.Log(ChatColor.yellow, "Failed to MoveNext after while. Iterator position: {0}.", iterationType.CurrentPosition);
                return;
            }
        }

        public static float GetCooldown()
        {
            return Random.NextFloat(1.5f, 2.5f);
        }
    }
}
