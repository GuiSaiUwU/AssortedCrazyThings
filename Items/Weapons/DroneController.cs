using AssortedCrazyThings.Base;
using AssortedCrazyThings.Buffs;
using AssortedCrazyThings.Projectiles.Minions.Drones;
using AssortedCrazyThings.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace AssortedCrazyThings.Items.Weapons
{
    public class DroneController : MinionItemBase
    {
        #region Static Methods
        /// <summary>
        /// Checks if drones are unlocked 
        /// </summary>
        public static bool AllUnlocked(DroneType unlocked)
        {
            foreach (DroneType type in Enum.GetValues(typeof(DroneType)))
            {
                if (type != DroneType.None)
                {
                    if (!unlocked.HasFlag(type))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if all drones are unlocked for the player
        /// </summary>
        public static bool AllUnlocked(Player player)
        {
            return AllUnlocked(player.GetModPlayer<AssPlayer>().droneControllerUnlocked);
        }

        /// <summary>
        /// Returns the custom MinionPos
        /// </summary>
        public static int GetSlotOfNextDrone(Projectile self)
        {
            int slot = 0;
            int min = Main.maxProjectiles;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active)
                {
                    if (proj.owner == self.owner && proj.identity != self.identity)
                    {
                        if (proj.ModProjectile is DroneBase drone && drone.IsCombatDrone)
                        {
                            int minionPos = drone.MinionPos;
                            min = Math.Min(min, minionPos);
                            if (minionPos > slot) slot = minionPos;
                        }
                    }
                }
            }
            if (min > 0) return 0;

            return slot + 1;
        }

        /// <summary>
        /// Determines position for the current projectile based on minionPos
        /// </summary>
        public static Vector2 GetPosition(Projectile projectile, int minionPos)
        {
            int initialVecticalOffset = -70;
            int initialHorizontalOffset = 0;
            int verticalOffset = -(int)(projectile.height * 1.2f);
            int horizontalOffset = (int)(projectile.width * 1.75f);
            int tempPos = minionPos % 6;
            int side = minionPos % 2; //1 means left, 0 means right
            int columnOffset = minionPos / 6 + 1; //1 for <6, 2 for <12, etc

            int rowOffset = tempPos / 2; //0, 1 or 2

            int middleOffset = 0;
            if (rowOffset == 1) middleOffset = horizontalOffset / 2; //the middle one

            Vector2 offset = new Vector2
                (
                initialHorizontalOffset + columnOffset * horizontalOffset + middleOffset,
                initialVecticalOffset + rowOffset * verticalOffset
                );
            offset.X *= side == 1 ? -1 : 1;
            return offset;
        }

        public static int SumOfSummonedDrones(Player player)
        {
            int sum = 0;
            for (int i = 0; i < DataList.Length; i++)
            {
                sum += player.ownedProjectileCounts[DataList[i].ProjType];
            }
            return sum;
        }

        public static bool CanSpawn(Player player, DroneType selected, out bool blocked)
        {
            bool canSpawn = true;
            blocked = false;
            if (selected == DroneType.Healing || selected == DroneType.Shield)
            {
                canSpawn = player.ownedProjectileCounts[GetDroneData(selected).ProjType] == 0;
                if (!canSpawn) blocked = true;
            }
            canSpawn &= player.GetModPlayer<AssPlayer>().droneControllerUnlocked.HasFlag(selected);
            return canSpawn;
        }

        private static void PreSync(Projectile proj)
        {
            if (proj.ModProjectile is DroneBase drone && drone.IsCombatDrone)
            {
                drone.MinionPos = GetSlotOfNextDrone(proj);
            }
        }

        /// <summary>
        /// Sets the data up for a DroneType
        /// </summary>
        public static DroneData SetDroneData(DroneType selected)
        {
            switch (selected)
            {
                case DroneType.BasicLaser:
                    return new DroneData
                        (
                        projType: ModContent.ProjectileType<BasicLaserDrone>(),
                        name: "Basic Laser Drone",
                        desc: "Rapidly fires lasers",
                        firerate: "High"
                        );
                case DroneType.HeavyLaser:
                    return new DroneData
                        (
                        projType: ModContent.ProjectileType<HeavyLaserDrone>(),
                        name: "Heavy Laser Drone",
                        desc: "Fires a penetrating laser after a long delay",
                        firerate: "Extremely slow",
                        dmgModifier: 8.091f,
                        kBModifier: 4f
                        );
                case DroneType.Missile:
                    return new DroneData
                        (
                        projType: ModContent.ProjectileType<MissileDrone>(),
                        name: "Missile Drone",
                        desc: "Fires a salvo of missiles after a long delay",
                        firerate: "Very slow",
                        dmgModifier: 2.19f,
                        kBModifier: 1.2f
                        );
                case DroneType.Healing:
                    return new DroneData
                        (
                        projType: ModContent.ProjectileType<HealingDrone>(),
                        name: "Healing Drone",
                        desc: "Heals you when hurt",
                        misc: "Only one can be summoned",
                        combat: false
                        );
                case DroneType.Shield:
                    return new DroneData
                        (
                        projType: ModContent.ProjectileType<ShieldDrone>(),
                        name: "Shield Drone",
                        desc: "Creates a damage reducing shield over time",
                        misc: "Only one can be summoned\nShield resets if drone despawns",
                        combat: false
                        );
                default:
                    throw new Exception("No DroneType specified");
            }
        }

        /// <summary>
        /// Holds data about each DroneType
        /// </summary>
        public static DroneData[] DataList;

        /// <summary>
        /// Used to access a particular DroneTypes data
        /// </summary>
        public static DroneData GetDroneData(DroneType selected)
        {
            return DataList[(int)Math.Log((int)selected, 2)];
        }

        public static CircleUIConf GetUIConf()
        {
            AssPlayer mPlayer = Main.LocalPlayer.GetModPlayer<AssPlayer>();
            List<string> tooltips = new List<string>();
            List<string> toUnlock = new List<string>();
            List<Asset<Texture2D>> assets = new List<Asset<Texture2D>>();
            List<bool> unlocked = new List<bool>();

            foreach (DroneType type in Enum.GetValues(typeof(DroneType)))
            {
                if (type != DroneType.None)
                {
                    DroneData data = GetDroneData(type);
                    assets.Add(AssUtils.Instance.Assets.Request<Texture2D>(data.PreviewTextureName));
                    unlocked.Add(mPlayer.droneControllerUnlocked.HasFlag(type));
                    tooltips.Add(data.UITooltip);
                    toUnlock.Add("Craft and use a '" + data.Name + " Components' Item");
                }
            }

            return new CircleUIConf(0, -1, assets, unlocked, tooltips, toUnlock);
        }

        /// <summary>
        /// Called in Mod.Load
        /// </summary>
        public static void DoLoad()
        {
            if (!ContentConfig.Instance.Weapons)
            {
                return;
            }

            Array a = Enum.GetValues(typeof(DroneType));
            DataList = new DroneData[a.Length - 1]; //without None
            int i = 0;
            foreach (DroneType type in a)
            {
                if (type != DroneType.None)
                {
                    DataList[i++] = SetDroneData(type);
                }
            }
        }

        public override void Unload()
        {
            DataList = null;
        }
        #endregion

        public override void SafeSetStaticDefaults()
        {
            DisplayName.SetDefault("Drone Controller");
            Tooltip.SetDefault("Summons a friendly Drone to support or fight for you"
                + "\nRight click to pick from available drones"
                + "\nHolding the item improves the Drones' supportive and offensive abilities");
        }

        public const int BaseDmg = 22;
        public const float BaseKB = 2.5f;

        public override void SetDefaults()
        {
            Item.damage = BaseDmg;
            Item.knockBack = BaseKB;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;
            Item.width = 28;
            Item.height = 30;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.value = Item.sellPrice(0, 0, 75, 0);
            Item.rare = -11;
            Item.UseSound = SoundID.Item44;
            Item.shoot = ModContent.ProjectileType<BasicLaserDrone>();
            Item.shootSpeed = 10f;
            Item.buffType = ModContent.BuffType<DroneControllerBuff>();
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage, ref float flat)
        {
            AssPlayer mPlayer = player.GetModPlayer<AssPlayer>();

            DroneType selected = mPlayer.selectedDroneControllerMinionType;
            damage += GetDroneData(selected).DmgModifier;
        }

        public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback, ref float flat)
        {
            AssPlayer mPlayer = player.GetModPlayer<AssPlayer>();

            DroneType selected = mPlayer.selectedDroneControllerMinionType;
            knockback *= GetDroneData(selected).KBModifier;
        }

        public override bool CanUseItem(Player player)
        {
            if (Main.netMode != NetmodeID.Server && player.whoAmI == Main.myPlayer)
            {
                AssPlayer mPlayer = player.GetModPlayer<AssPlayer>();
                DroneType selected = mPlayer.selectedDroneControllerMinionType;

                if (!CanSpawn(player, selected, out _))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool SafeShoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            AssPlayer mPlayer = player.GetModPlayer<AssPlayer>();
            DroneType selected = mPlayer.selectedDroneControllerMinionType;
            DroneData droneData = GetDroneData(selected);
            type = droneData.ProjType;

            int index = AssUtils.NewProjectile(source, player.Center.X, player.Center.Y, 0f, player.velocity.Y - 6f, type, damage, knockback, preSync: PreSync);
            Main.projectile[index].originalDamage = (int)(Item.damage * (1f + droneData.DmgModifier));
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1).AddIngredient(ItemID.HallowedBar, 1).AddIngredient(ItemID.Switch, 2).AddIngredient(ItemID.Wire, 10).AddTile(TileID.MythrilAnvil).Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            AssPlayer mPlayer = Main.LocalPlayer.GetModPlayer<AssPlayer>();
            DroneType selected = mPlayer.selectedDroneControllerMinionType;

            DroneData data = GetDroneData(selected);

            int damageIndex = -1;
            int knockbackIndex = -1;

            bool hasController = Main.LocalPlayer.HasItem(Item.type);

            for (int i = 0; i < tooltips.Count; i++)
            {
                TooltipLine line = tooltips[i];
                if (hasController)
                {
                    if (line.mod == "Terraria" && line.Name == "ItemName")
                    {
                        line.text += " (" + data.Name + ")";
                    }
                }

                if (line.mod == "Terraria" && line.Name == "Damage")
                {
                    damageIndex = i;
                }

                if (line.mod == "Terraria" && line.Name == "Knockback")
                {
                    knockbackIndex = i;
                }
            }

            if (damageIndex > -1)
            {
                if (!data.Combat)
                {
                    tooltips.RemoveAt(damageIndex);
                }
            }

            if (knockbackIndex != -1)
            {
                if (data.Combat)
                {
                    if (data.Firerate != "") tooltips.Insert(knockbackIndex, new TooltipLine(Mod, "Firerate", data.Firerate + " firerate"));
                }
                else
                {
                    //here damageIndex one is removed, so find knockbackindex again
                    knockbackIndex = tooltips.FindIndex(line => line.Name == "Knockback");
                    if (knockbackIndex > -1) tooltips.RemoveAt(knockbackIndex);
                }
            }

            bool allUnlocked = AllUnlocked(mPlayer.droneControllerUnlocked);

            if (!(allUnlocked && hasController))
            {
                tooltips.Add(new TooltipLine(Mod, "Destroyer", "Defeat The Destroyer to unlock more drones"));
            }

            CanSpawn(Main.LocalPlayer, selected, out bool blocked);
            if (hasController && blocked)
            {
                tooltips.Add(new TooltipLine(Mod, "CanSpawn", "Only one " + data.Name + " can be out at once"));
            }
        }
    }

    /// <summary>
    /// Holds data about a DroneType
    /// </summary>
    public struct DroneData
    {
        public readonly int ProjType;
        public readonly string Name;
        public readonly float DmgModifier;
        public readonly float KBModifier;
        public readonly string UITooltip;
        public readonly string Firerate;
        public readonly bool Combat;

        public string PreviewTextureName
        {
            get
            {
                return "Projectiles/Minions/Drones/" + Name.Replace(" ", "") + "Preview";
            }
        }

        public DroneData(int projType, string name, string desc, string misc = "", string firerate = "", float dmgModifier = 0f, float kBModifier = 1f, bool combat = true)
        {
            ProjType = projType;
            Name = name;
            DmgModifier = dmgModifier;
            KBModifier = kBModifier;
            Firerate = firerate;
            string stats = combat ? ("\nBase Damage: " + (int)(DroneController.BaseDmg * (DmgModifier + 1f))
             + "\nBase Knockback: " + Math.Round(DroneController.BaseKB * KBModifier, 1)) : "";
            UITooltip = Name + stats + "\n" + desc + "\n" + misc;
            Combat = combat;
        }
    }



    /// <summary>
    /// The type of drone enumerated, so you can check against it via .HasFlag(DroneType.SomeType)
    /// </summary>
    [Flags]
    public enum DroneType : byte
    {
        None = 0,
        BasicLaser = 1,
        HeavyLaser = 2,
        Missile = 4,
        Healing = 8,
        Shield = 16
    }
}
