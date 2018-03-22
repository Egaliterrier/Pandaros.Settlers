﻿using BlockTypes.Builtin;
using NPC;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class Void
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Void.Add"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".Void";
            var node = new JSONNode();
            node["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/void.png");
            node["isPlaceable"] = new JSONNode(false);

            JSONNode categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("ingredient"));
            categories.AddToArray(new JSONNode("magic"));
            node.SetAs("categories", categories);

            Item = new ItemTypesServer.ItemTypeRaw(name, node);
            items.Add(name, Item);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCCraftedRecipe, GameLoader.NAMESPACE + ".Items.Void.OnNPCCraftedRecipe")]
        public static void OnNPCCraftedRecipe(IJob job, Recipe recipe, List<InventoryItem> results)
        {
            if (recipe.Name == Elementium.Item.name && job.NPC != null)
            {
                var inv = Entities.SettlerInventory.GetSettlerInventory(job.NPC);
                var chance = 0.03f;

                if (inv.JobSkills.ContainsKey(Jobs.ApothecaryRegister.JOB_NAME))
                    chance += inv.JobSkills[Jobs.ApothecaryRegister.JOB_NAME];

                if (Pipliz.Random.NextFloat() <= chance)
                {
                    results.Add(new InventoryItem(Item.ItemIndex));
                    PandaChat.Send(job.NPC.Colony.Owner, $"{inv.SettlerName} the Apothecary has discovered a Void Stone while crafting Elementium!", ChatColor.orange);
                }
            }
        }
    }
}