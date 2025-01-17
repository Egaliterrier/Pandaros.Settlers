﻿using Newtonsoft.Json;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Armor;
using Pandaros.Settlers.Items.Weapons;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class ItemsProvider : IAfterWorldLoad, IAddItemTypes
    {
        StringBuilder _sb = new StringBuilder();

        List<string> _fixRelativePaths = new List<string>()
        {
            "icon",
            "mash"
        };

        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ICSType);
        public Type ClassType => null;

        public void AddItemTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            var i = 0;
            List<ICSType> loadedItems = new List<ICSType>();

            foreach (var item in LoadedAssembalies)
            {
                try
                {
                    if (Activator.CreateInstance(item) is ICSType itemType &&
                        !string.IsNullOrEmpty(itemType.name))
                    {
                        loadedItems.Add(itemType);            
                    }
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
            }

            var settings = GameLoader.GetJSONSettingPaths(GameLoader.NAMESPACE + ".CSItems");
            
            foreach (var modInfo in settings)
            {
                foreach (var path in modInfo.Value)
                {
                    try
                    {
                        var jsonFile = JSON.Deserialize(modInfo.Key + "\\" + path);

                        if (jsonFile.NodeType == NodeType.Object && jsonFile.ChildCount > 0)
                            foreach (var item in jsonFile.LoopObject())
                            {
                                foreach (var property in _fixRelativePaths)
                                if (item.Value.TryGetAs(property, out string propertyPath) && propertyPath.StartsWith("./"))
                                    item.Value[property] = new JSONNode(modInfo.Key + "\\" + propertyPath.Substring(2));

                                if (item.Value.TryGetAs("Durability", out int durability))
                                    loadedItems.Add(item.Value.JsonDeerialize<MagicArmor>());
                                else if (item.Value.TryGetAs("WepDurability", out bool wepDurability))
                                    loadedItems.Add(item.Value.JsonDeerialize<MagicWeapon>());
                                else if (item.Value.TryGetAs("IsMagical", out bool isMagic))
                                    loadedItems.Add(item.Value.JsonDeerialize<PlayerMagicItem>());
                                else
                                    loadedItems.Add(item.Value.JsonDeerialize<CSType>());
                            }
                    }
                    catch (Exception ex)
                    {
                        PandaLogger.LogError(ex);
                    }
                }
            }

            foreach (var itemType in loadedItems)
            {
                var rawItem = new ItemTypesServer.ItemTypeRaw(itemType.name, itemType.JsonSerialize());

                if (itemTypes.ContainsKey(itemType.name))
                {
                    PandaLogger.Log(ChatColor.yellow, "Item {0} already loaded...Overriding item.", itemType.name);
                    itemTypes[itemType.name] = rawItem;
                }
                else
                    itemTypes.Add(itemType.name, rawItem);

                if (itemType.StaticItemSettings != null && !string.IsNullOrWhiteSpace(itemType.StaticItemSettings.Name))
                    StaticItems.List.Add(itemType.StaticItemSettings);

                if (itemType is IPlayerMagicItem pmi)
                    MagicItemsCache.PlayerMagicItems[pmi.name] = pmi;

                if (itemType.OpensMenuSettings != null && !string.IsNullOrEmpty(itemType.OpensMenuSettings.ItemName))
                    Help.UIManager.OpenMenuItems.Add(itemType.OpensMenuSettings);

                _sb.Append($"{itemType.name}, ");
                i++;

                if (i > 5)
                {
                    _sb.Append("</color>");
                    i = 0;
                    _sb.AppendLine();
                    _sb.Append("<color=lime>");
                }
            }

        }

        public void AfterWorldLoad()
        {
            PandaLogger.Log(ChatColor.lime, "-------------------Items Loaded----------------------");
            PandaLogger.Log(ChatColor.lime, _sb.ToString());
            PandaLogger.Log(ChatColor.lime, "------------------------------------------------------"); 
        }
    }
}
