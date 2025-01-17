﻿using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items.Reagents;
using Pandaros.Settlers.Models;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pandaros.Settlers.Items.Armor.ArmorFactory;

namespace Pandaros.Settlers.Items.Armor.Magical
{
    public class BootsOfFallingRecipe : ICSRecipe
    {
        public static string NAME = GameLoader.NAMESPACE + ".BootsOfFallingRecipe";

        public List<RecipeItem> requires { get; set; } = new List<RecipeItem>()
        {
            { new RecipeItem(Adamantine.NAME, 20) },
            { new RecipeItem(Elementium.Item.name, 30) },
            { new RecipeItem(AirStone.Item.name, 30) },
            { new RecipeItem(EarthStone.Item.name, 30) },
            { new RecipeItem(WaterStone.Item.name, 30) },
            { new RecipeItem(Esper.Item.name, 1) },
            { new RecipeItem(Mana.Item.name, 50) }
        };

        public List<RecipeItem> results { get; set; } = new List<RecipeItem>()
        {
            { new RecipeItem(BootsOfFalling.NAME, 1) }
        };

        public CraftPriority defaultPriority { get; set; } = CraftPriority.Medium;

        public bool isOptional { get; set; } = false;

        public int defaultLimit { get; set; } = 1;

        public string Job { get; set; } = Jobs.SorcererRegister.JOB_NAME;

        public string name => NAME;
    }

    public class BootsOfFalling : CSType, IArmor, IPlayerMagicItem
    {
        public static string NAME = GameLoader.NAMESPACE + ".BootsOfFalling";

        public override string name { get; set; } = NAME;
        public override bool? isPlaceable => false;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "armor",
            "MagicItem"
        };

        public override string icon { get; set; } = GameLoader.ICON_PATH + "BootsOfFalling.png";

        

        public float MovementSpeed { get; set; }

        public float JumpPower { get; set; }

        public float FlySpeed { get; set; }

        public float MoveSpeed { get; set; }

        public float LightRange { get; set; }

        public string LightColor { get; set; }

        public float FallDamage { get; set; } = PlayerState.GetPlayerVariables().GetAs<float>("FallDamageBaseDamage") * -1;

        public float FallDamagePerUnit { get; set; } = PlayerState.GetPlayerVariables().GetAs<float>("FallDamagePerUnit") * -1;

        public float BuildDistance { get; set; }

        public bool IsMagical { get; set; } = true;
        public float Skilled { get; set; }

        public float HPTickRegen { get; set; }

        public float MissChance { get; set; }

        public DamageType ElementalArmor { get; set; }

        public Dictionary<DamageType, float> AdditionalResistance { get; set; } = new Dictionary<DamageType, float>();

        public Dictionary<DamageType, float> Damage { get; set; } = new Dictionary<DamageType, float>();

        public float Luck { get; set; }

        public float ArmorRating { get; }

        public int Durability { get; set; } = int.MaxValue;

        public ItemTypesServer.ItemTypeRaw ItemType { get; }

        public ArmorSlot Slot { get; } = ArmorSlot.Boots;

        public void Update()
        {

        }
    }
}
