﻿using Pandaros.Settlers.Managers;
using Pandaros.Settlers.Monsters.Bosses;
using Pandaros.Settlers.Seasons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class SeasonProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ISeason);
        public Type ClassType => null;

        public void AddItemTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            
        }

        public void AfterItemTypesDefined()
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.Log(ChatColor.lime, "-------------------Seasons Loaded----------------------");

            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is ISeason season &&
                    !string.IsNullOrEmpty(season.Name))
                {
                    SeasonsFactory.AddSeason(season);
                }
            }

            SeasonsFactory.ResortSeasons();
            PandaLogger.Log(ChatColor.lime, "---------------------------------------------------------");
        }

        public void AfterSelectedWorld()
        {
           
        }

        public void AfterWorldLoad()
        {
           
        }

        public void OnAddResearchables()
        {

        }
    }
}
