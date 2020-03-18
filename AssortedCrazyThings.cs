using AssortedCrazyThings.Base;
using AssortedCrazyThings.Buffs;
using AssortedCrazyThings.Items;
using AssortedCrazyThings.Items.PetAccessories;
using AssortedCrazyThings.Items.Pets.CuteSlimes;
using AssortedCrazyThings.Items.Placeable;
using AssortedCrazyThings.Items.Tools;
using AssortedCrazyThings.Items.Weapons;
using AssortedCrazyThings.NPCs.DungeonBird;
using AssortedCrazyThings.Projectiles.Minions;
using AssortedCrazyThings.Projectiles.Minions.CompanionDungeonSouls;
using AssortedCrazyThings.Projectiles.Pets;
using AssortedCrazyThings.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaOverhaul.Core.Systems.Tags.Groups;
using TerrariaOverhaul.Utilities.Extensions;

namespace AssortedCrazyThings
{
    class AssortedCrazyThings : Mod
    {
        //Sun pet textures
        public static Texture2D[] sunPetTextures;

        //Soul item animated textures
        public static Texture2D[] animatedSoulTextures;

        /// <summary>
        /// Soul NPC spawn blacklist
        /// </summary>
        public static int[] soulBuffBlacklist;

        /// <summary>
        /// Zoom level, (for UIs). 0f == fully zoomed out, 1f == fully zoomed in
        /// </summary>
        public static Vector2 ZoomFactor;

        //Loaded Mods
        public static bool BossAssistLoadedWithRadar;

        //UI stuff
        internal static UserInterface CircleUIInterface;
        internal static CircleUI CircleUI;

        internal static UserInterface HoverNPCUIInterface;
        internal static HoverNPCUI HoverNPCUI;

        internal static UserInterface HarvesterEdgeUIInterface;
        internal static HarvesterEdgeUI HarvesterEdgeUI;

        internal static UserInterface EnhancedHunterUIInterface;
        internal static EnhancedHunterUI EnhancedHunterUI;

        internal static UserInterface PetVanityUIInterface;
        internal static PetVanityUI PetVanityUI;

        //Mod Helpers compat
        public static string GithubUserName { get { return "Werebearguy"; } }
        public static string GithubProjectName { get { return "AssortedCrazyThings"; } }

        private void LoadSoulBuffBlacklist()
        {
            List<int> tempList = new List<int>
            {
                NPCID.Bee,
                NPCID.BeeSmall,
                NPCID.BlueSlime,
                NPCID.BlazingWheel,
                NPCID.EaterofWorldsHead,
                NPCID.EaterofWorldsBody,
                NPCID.EaterofWorldsTail,
                NPCID.Creeper,
                NPCID.GolemFistLeft,
                NPCID.GolemFistRight,
                NPCID.PlanterasHook,
                NPCID.PlanterasTentacle,
                NPCID.Probe,
                NPCID.ServantofCthulhu,
                NPCID.SlimeSpiked,
                NPCID.SpikeBall,
                NPCID.TheHungry,
                NPCID.TheHungryII,
            };

            soulBuffBlacklist = tempList.ToArray();
        }

        /// <summary>
        /// Assuming this is called after InitSoulBuffBlacklist.
        /// Adds NPC types to soulBuffBlacklist manually
        /// </summary>
        private void AddToSoulBuffBlacklist()
        {
            //assuming this is called after InitSoulBuffBlacklist
            List<int> tempList = new List<int>(soulBuffBlacklist)
            {
                ModContent.NPCType<DungeonSoul>(),
                ModContent.NPCType<DungeonSoulFreed>()
            };

            soulBuffBlacklist = tempList.ToArray();
            Array.Sort(soulBuffBlacklist);
        }

        /// <summary>
        /// Fills isModdedWormBodyOrTail with types of modded NPCs which names are ending with Body or Tail (indicating they are part of something)
        /// </summary>
        private void LoadWormList()
        {
            List<int> tempList = new List<int>();

            for (int i = Main.maxNPCTypes; i < NPCLoader.NPCCount; i++)
            {
                ModNPC modNPC = NPCLoader.GetNPC(i);
                if (modNPC != null && (modNPC.GetType().Name.EndsWith("Body") || modNPC.GetType().Name.EndsWith("Tail")))
                {
                    tempList.Add(modNPC.npc.type);
                }
            }

            AssUtils.isModdedWormBodyOrTail = tempList.ToArray();
            Array.Sort(AssUtils.isModdedWormBodyOrTail);
        }

        private void LoadUI()
        {
            if (!Main.dedServ && Main.netMode != 2)
            {
                CircleUI = new CircleUI();
                CircleUI.Activate();
                CircleUIInterface = new UserInterface();
                CircleUIInterface.SetState(CircleUI);

                HoverNPCUI = new HoverNPCUI();
                HoverNPCUI.Activate();
                HoverNPCUIInterface = new UserInterface();
                HoverNPCUIInterface.SetState(HoverNPCUI);

                HarvesterEdgeUI = new HarvesterEdgeUI();
                HarvesterEdgeUI.Activate();
                HarvesterEdgeUIInterface = new UserInterface();
                HarvesterEdgeUIInterface.SetState(HarvesterEdgeUI);

                EnhancedHunterUI = new EnhancedHunterUI();
                EnhancedHunterUI.Activate();
                EnhancedHunterUIInterface = new UserInterface();
                EnhancedHunterUIInterface.SetState(EnhancedHunterUI);

                PetVanityUI = new PetVanityUI();
                PetVanityUI.Activate();
                PetVanityUIInterface = new UserInterface();
                PetVanityUIInterface.SetState(PetVanityUI);
            }
        }

        private void UnloadUI()
        {
            if (!Main.dedServ && Main.netMode != 2)
            {
                CircleUIInterface = null;
                CircleUI = null;

                HoverNPCUIInterface = null;
                HoverNPCUI = null;

                HarvesterEdgeUIInterface = null;
                HarvesterEdgeUI = null;

                EnhancedHunterUIInterface = null;
                EnhancedHunterUI = null;

                PetVanityUIInterface = null;
                PetVanityUI = null;

                HarvesterEdgeUI.texture = null;
                EnhancedHunterUI.arrowTexture = null;
                PetVanityUI.redCrossTexture = null;
                CircleUI.UIConf = null;
                CircleUIHandler.TriggerListLeft.Clear();
                CircleUIHandler.TriggerListRight.Clear();
            }
        }

        private void LoadPets()
        {
            SlimePets.Load();
            if (!Main.dedServ && Main.netMode != 2)
            {
                PetAccessory.Load();
            }
        }

        private void UnloadPets()
        {
            SlimePets.Unload();
            if (!Main.dedServ && Main.netMode != 2)
            {
                PetAccessory.Unload();
            }
        }

        private void LoadMisc()
        {
            if (!Main.dedServ && Main.netMode != 2)
            {
                animatedSoulTextures = new Texture2D[2];

                animatedSoulTextures[0] = GetTexture("Items/CaughtDungeonSoulAnimated");
                animatedSoulTextures[1] = GetTexture("Items/CaughtDungeonSoulFreedAnimated");

                sunPetTextures = new Texture2D[3];

                for (int i = 0; i < sunPetTextures.Length; i++)
                {
                    sunPetTextures[i] = GetTexture("Projectiles/Pets/PetSunProj_" + i);
                    PremultiplyTexture(sunPetTextures[i]);
                }
            }
        }

        private void UnloadMisc()
        {
            if (!Main.dedServ && Main.netMode != 2)
            {
                animatedSoulTextures = null;

                sunPetTextures = null;

                PetEaterofWorldsBase.wormTypes = null;

                PetDestroyerBase.wormTypes = null;
            }
        }

        public override void Load()
        {
            AssHooks.Load();

            LoadPets();

            LoadSoulBuffBlacklist();

            LoadMisc();
        }

        public override void Unload()
        {
            UnloadPets();

            UnloadUI();

            UnloadMisc();

            GitgudData.Unload();

            DroneController.Unload();

            EverhallowedLantern.Unload();
        }

        public override void PostSetupContent()
        {
            //for things that have to be called after Load() because of Main.projFrames[projectile.type] calls (and similar)
            LoadUI();

            LoadWormList();

            GitgudData.Load();

            DroneController.Load();

            EverhallowedLantern.Load();

            AddToSoulBuffBlacklist();

            PetEaterofWorldsBase.wormTypes = new int[]
            {
                ModContent.ProjectileType<PetEaterofWorldsHead>(),
                ModContent.ProjectileType<PetEaterofWorldsBody1>(),
                ModContent.ProjectileType<PetEaterofWorldsBody2>(),
                ModContent.ProjectileType<PetEaterofWorldsTail>()
            };

            PetDestroyerBase.wormTypes = new int[]
            {
                ModContent.ProjectileType<PetDestroyerHead>(),
                ModContent.ProjectileType<PetDestroyerBody1>(),
                ModContent.ProjectileType<PetDestroyerBody2>(),
                ModContent.ProjectileType<PetDestroyerTail>()
            };

            //https://forums.terraria.org/index.php?threads/boss-checklist-in-game-progression-checklist.50668/
            Mod bossChecklist = ModLoader.GetMod("BossChecklist");
            if (bossChecklist != null)
            {
                //5.1f means just after skeletron
                bossChecklist.Call("AddMiniBossWithInfo", Harvester.name, 5.1f, (Func<bool>)(() => AssWorld.downedHarvester), "Use a [i:" + ModContent.ItemType<IdolOfDecay>() + "] in the dungeon after Skeletron has been defeated");
            }

            Mod summonersAssociation = ModLoader.GetMod("SummonersAssociation");
            if (summonersAssociation != null && summonersAssociation.Version > new Version(0, 4, 1))
            {
                summonersAssociation.Call("AddMinionInfo", ModContent.ItemType<EverglowLantern>(), ModContent.BuffType<CompanionDungeonSoulMinionBuff>(), new List<int>
                {
                    ModContent.ProjectileType<CompanionDungeonSoulPreWOFMinion>(),
                });
                summonersAssociation.Call("AddMinionInfo", ModContent.ItemType<EverhallowedLantern>(), ModContent.BuffType<CompanionDungeonSoulMinionBuff>(), new List<int>
                {
                    ModContent.ProjectileType<CompanionDungeonSoulPostWOFMinion>(),
                    ModContent.ProjectileType<CompanionDungeonSoulFrightMinion>(),
                    ModContent.ProjectileType<CompanionDungeonSoulMightMinion>(),
                    ModContent.ProjectileType<CompanionDungeonSoulSightMinion>()
                });
                summonersAssociation.Call("AddMinionInfo", ModContent.ItemType<SlimeHandlerKnapsack>(), ModContent.BuffType<SlimePackMinionBuff>(), ModContent.ProjectileType<SlimePackMinion>());
                List<int> drones = new List<int>();
                foreach (var drone in DroneController.DataList)
                {
                    drones.Add(drone.ProjType);
                }
                summonersAssociation.Call("AddMinionInfo", ModContent.ItemType<DroneController>(), ModContent.BuffType<DroneControllerBuff>(), drones);
            }

            Mod bossAssist = ModLoader.GetMod("BossAssist");
            if (bossAssist != null && bossAssist.Version > new Version(0, 2, 2))
            {
                BossAssistLoadedWithRadar = true;
            }

            Mod overhaul = ModLoader.GetMod("TerrariaOverhaul");
            if (overhaul != null)
            {
                OverhaulCompat();
            }
        }

        public static void OverhaulCompat()
        {
            ModItem mItem = ModContent.GetModItem(ModContent.ItemType<ExtendoNetRegular>());
            mItem.SetTag(ItemTags.AllowQuickUse, true);
            mItem = ModContent.GetModItem(ModContent.ItemType<ExtendoNetGolden>());
            mItem.SetTag(ItemTags.AllowQuickUse, true);
        }

        public override void AddRecipeGroups()
        {
            RecipeGroup.RegisterGroup("ACT:RegularCuteSlimes", new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Regular Bottled Slime", new int[]
            {
                ModContent.ItemType<CuteSlimeBlueNew>(),
                ModContent.ItemType<CuteSlimeBlackNew>(),
                ModContent.ItemType<CuteSlimeGreenNew>(),
                ModContent.ItemType<CuteSlimePinkNew>(),
                ModContent.ItemType<CuteSlimePurpleNew>(),
                ModContent.ItemType<CuteSlimeRainbowNew>(),
                ModContent.ItemType<CuteSlimeRedNew>(),
                ModContent.ItemType<CuteSlimeYellowNew>()
            }));

            RecipeGroup.RegisterGroup("ACT:GoldPlatinum", new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.GoldBar), new int[]
            {
                ItemID.GoldBar,
                ItemID.PlatinumBar
            }));
        }

        /// <summary>
        /// Creates golden dust particles at the projectiles location with that type and LocalPlayer as owner. (Used for pets)
        /// </summary>
        private void PoofVisual(int projType)
        {
            int projIndex = -1;
            //find first occurence of a player owned projectile
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active)
                {
                    if (Main.projectile[i].owner == Main.myPlayer && Main.projectile[i].type == projType)
                    {
                        projIndex = i;
                        break;
                    }
                }
            }

            if (projIndex != -1)
            {
                Dust dust;
                for (int i = 0; i < 14; i++)
                {
                    dust = Dust.NewDustDirect(Main.projectile[projIndex].position, Main.projectile[projIndex].width, Main.projectile[projIndex].height, 204, Main.projectile[projIndex].velocity.X, Main.projectile[projIndex].velocity.Y, 0, new Color(255, 255, 255), 0.8f);
                    dust.noGravity = true;
                    dust.noLight = true;
                }
            }
        }

        /// <summary>
        /// Called when CircleUI starts
        /// </summary>
        private void CircleUIStart(int triggerType, bool triggerLeft = true, bool fromDresser = false)
        {
            AssPlayer mPlayer = Main.LocalPlayer.GetModPlayer<AssPlayer>();
            PetPlayer pPlayer = Main.LocalPlayer.GetModPlayer<PetPlayer>();

            //combine both lists of the players (split for organization and player load shenanigans)
            List<CircleUIHandler> l = mPlayer.CircleUIList;
            l.AddRange(pPlayer.CircleUIList);

            bool found = false;
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].Condition())
                {
                    if (l[i].TriggerItem == triggerType)
                    {
                        if (l[i].TriggerLeft == triggerLeft)
                        {
                            CircleUI.UIConf = l[i].UIConf();
                            CircleUI.currentSelected = l[i].OnUIStart();
                            found = true;
                            break;
                        }
                    }
                }
            }
            //extra things that happen
            if (!found)
            {
                if (triggerType == ModContent.ItemType<VanitySelector>())
                {
                    AssUtils.UIText("No alt costumes found for" + (triggerLeft ? "" : " light") + " pet", CombatText.DamagedFriendly);
                    return;
                }
            }

            //Spawn UI
            CircleUI.Start(triggerType, triggerLeft, fromDresser);
        }

        /// <summary>
        /// Called when CircleUI ends
        /// </summary>
        private void CircleUIEnd(bool triggerLeft = true)
        {
            AssPlayer mPlayer = Main.LocalPlayer.GetModPlayer<AssPlayer>();
            PetPlayer pPlayer = Main.LocalPlayer.GetModPlayer<PetPlayer>();
            if (CircleUI.returned != CircleUI.NONE && CircleUI.returned != CircleUI.currentSelected)
            {
                //if something returned AND if the returned thing isn't the same as the current one

                try
                {
                    Main.PlaySound(SoundID.Item4.WithVolume(0.6f), Main.LocalPlayer.position);
                }
                catch
                {
                    //No idea why but this threw errors one time
                }

                List<CircleUIHandler> l = mPlayer.CircleUIList;
                for (int i = 0; i < l.Count; i++)
                {
                    if (l[i].Condition())
                    {
                        if (l[i].TriggerItem == CircleUI.triggerItemType)
                        {
                            if (l[i].TriggerLeft == triggerLeft)
                            {
                                l[i].OnUIEnd();
                                break;
                            }
                        }
                    }
                }
                //extra things that happen
                if (CircleUI.triggerItemType == ModContent.ItemType<VanitySelector>())
                {
                    PoofVisual(CircleUI.UIConf.AdditionalInfo);
                    AssUtils.UIText("Selected: " + CircleUI.UIConf.Tooltips[CircleUI.returned], CombatText.HealLife);
                }
            }

            CircleUI.returned = CircleUI.NONE;
            CircleUI.visible = false;
        }

        /// <summary>
        /// Called in UpdateUI
        /// </summary>
        private void UpdateCircleUI()
        {
            Player player = Main.LocalPlayer;
            AssPlayer mPlayer = player.GetModPlayer<AssPlayer>();

            int triggerType = player.HeldItem.type;
            bool openWithDresser = player.showItemIcon2 == ModContent.ItemType<VanityDresserItem>();
            if (openWithDresser)
            {
                triggerType = ModContent.ItemType<VanitySelector>();
            }
            bool? left = null;
            if (mPlayer.LeftClickPressed && (CircleUIHandler.TriggerListLeft.Contains(triggerType) || openWithDresser))
            {
                left = true;
            }
            else if (mPlayer.RightClickPressed && (CircleUIHandler.TriggerListRight.Contains(triggerType) || openWithDresser))
            {
                left = false;
            }

            if (left != null && AllowedToOpenUI(ModContent.ItemType<VanityDresserItem>())) CircleUIStart(triggerType, (bool)left, openWithDresser);

            if (CircleUI.visible)
            {
                left = null;
                if (mPlayer.LeftClickReleased)
                {
                    left = true;
                }
                else if (mPlayer.RightClickReleased)
                {
                    left = false;
                }

                if (left != null && left == CircleUI.openedWithLeft) CircleUIEnd((bool)left);

                if (CircleUI.triggerItemType != triggerType && !CircleUI.triggeredFromDresser) //cancel the UI when you switch items
                {
                    CircleUI.returned = CircleUI.NONE;
                    CircleUI.visible = false;
                }
            }
        }

        /// <summary>
        /// Called in UpdateUI
        /// </summary>
        private void UpdatePetVanityUI()
        {
            AssPlayer mPlayer = Main.LocalPlayer.GetModPlayer<AssPlayer>();
            PetPlayer pPlayer = Main.LocalPlayer.GetModPlayer<PetPlayer>();

            if (mPlayer.LeftClickPressed && AllowedToOpenUI() && PetAccessory.IsItemAPetVanity(Main.LocalPlayer.HeldItem.type, forUI: true))
            {
                PetAccessory petAccessory = PetAccessory.GetAccessoryFromType(Main.LocalPlayer.HeldItem.type);
                if (pPlayer.slimePetIndex != -1 &&
                    Main.projectile[pPlayer.slimePetIndex].active &&
                    Main.projectile[pPlayer.slimePetIndex].owner == Main.myPlayer &&
                    SlimePets.slimePets.Contains(Main.projectile[pPlayer.slimePetIndex].type) &&
                    !SlimePets.GetPet(Main.projectile[pPlayer.slimePetIndex].type).IsSlotTypeBlacklisted[(int)petAccessory.Slot])
                {
                    //Spawn UI
                    PetVanityUI.Start(petAccessory);
                }
            }

            if (PetVanityUI.visible)
            {
                if (mPlayer.LeftClickReleased)
                {
                    if (PetVanityUI.returned > PetVanityUI.NONE)
                    {
                        //if something returned AND if the returned thing isn't the same as the current one

                        try
                        {
                            Main.PlaySound(SoundID.Item1, Main.LocalPlayer.position);
                        }
                        catch
                        {
                            //No idea why but this threw errors one time
                        }
                        //UIText("Selected: " + PetVanityUI.petAccessory.AltTextureSuffixes[PetVanityUI.returned], CombatText.HealLife);

                        PetVanityUI.petAccessory.Color = (byte)PetVanityUI.returned;
                        pPlayer.ToggleAccessory(PetVanityUI.petAccessory);
                    }
                    else if (PetVanityUI.hasEquipped && PetVanityUI.returned == PetVanityUI.NONE)
                    {
                        //hovered over the middle and had something equipped: take accessory away
                        pPlayer.DelAccessory(PetVanityUI.petAccessory);
                    }
                    //else if (returned == PetVanityUI.IGNORE) {nothing happens}

                    PetVanityUI.returned = PetVanityUI.NONE;
                    PetVanityUI.visible = false;
                }

                if (PetVanityUI.petAccessory.Type != Main.LocalPlayer.HeldItem.type) //cancel the UI when you switch items
                {
                    PetVanityUI.returned = PetVanityUI.NONE;
                    PetVanityUI.visible = false;
                }
            }
        }

        private void UpdateHoverNPCUI(GameTime gameTime)
        {
            HoverNPCUI.Update(gameTime);
        }

        private void UpdateEnhancedHunterUI(GameTime gameTime)
        {
            if (Main.LocalPlayer.GetModPlayer<AssPlayer>().enhancedHunterBuff)
            {
                EnhancedHunterUI.visible = true;
            }
            else
            {
                EnhancedHunterUI.visible = false;
            }
            EnhancedHunterUI.Update(gameTime);
        }

        private void UpdateHarvesterEdgeUI(GameTime gameTime)
        {
            HarvesterEdgeUI.Update(gameTime);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            UpdateCircleUI();
            UpdateHoverNPCUI(gameTime);
            UpdateEnhancedHunterUI(gameTime);
            UpdateHarvesterEdgeUI(gameTime);
            UpdatePetVanityUI();
        }

        /// <summary>
        /// Checks if LocalPlayer can open a UI
        /// </summary>
        private bool AllowedToOpenUI(int blacklistTileType = -1)
        {
            return Main.hasFocus &&
                !Main.gamePaused &&
                !Main.LocalPlayer.dead &&
                !Main.LocalPlayer.mouseInterface &&
                !Main.drawingPlayerChat &&
                !Main.editSign &&
                !Main.editChest &&
                !Main.blockInput &&
                !Main.mapFullscreen &&
                !Main.HoveringOverAnNPC &&
                Main.LocalPlayer.showItemIcon2 != -1 &&
                (!Main.LocalPlayer.showItemIcon || Main.LocalPlayer.showItemIcon2 == blacklistTileType) &&
                Main.LocalPlayer.talkNPC == -1 &&
                Main.LocalPlayer.itemTime == 0 && Main.LocalPlayer.itemAnimation == 0 &&
                !(Main.LocalPlayer.frozen || Main.LocalPlayer.webbed || Main.LocalPlayer.stoned);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Hotbar"));
            if (inventoryIndex != -1)
            {
                if (CircleUI.visible)
                {
                    //remove the item icon when using the item while held outside the inventory
                    int mouseItemIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Item / NPC Head"));
                    if (mouseItemIndex != -1) layers.RemoveAt(mouseItemIndex);
                    layers.Insert(++inventoryIndex, new LegacyGameInterfaceLayer
                        (
                        "ACT: Appearance Select",
                        delegate
                        {
                            CircleUIInterface.Draw(Main.spriteBatch, new GameTime());
                            return true;
                        },
                        InterfaceScaleType.UI)
                    );
                }

                if (PetVanityUI.visible)
                {
                    //remove the item icon when using the item while held outside the inventory
                    int mouseItemIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Item / NPC Head"));
                    if (mouseItemIndex != -1) layers.RemoveAt(mouseItemIndex);
                    layers.Insert(++inventoryIndex, new LegacyGameInterfaceLayer
                        (
                        "ACT: Pet Vanity Select",
                        delegate
                        {
                            PetVanityUIInterface.Draw(Main.spriteBatch, new GameTime());
                            return true;
                        },
                        InterfaceScaleType.UI)
                    );
                }
            }

            int mouseOverIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Over"));
            if (mouseOverIndex != -1)
            {
                layers.Insert(++mouseOverIndex, new LegacyGameInterfaceLayer
                    (
                    "ACT: NPC Mouse Over",
                    delegate
                    {
                        HoverNPCUIInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );

                layers.Insert(++mouseOverIndex, new LegacyGameInterfaceLayer
                    (
                    "ACT: Enhanced Hunter",
                    delegate
                    {
                        if (EnhancedHunterUI.visible) EnhancedHunterUIInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );

                layers.Insert(++mouseOverIndex, new LegacyGameInterfaceLayer
                    (
                    "ACT: Harvester Edge",
                    delegate
                    {
                        HarvesterEdgeUIInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)
        {
            ZoomFactor = Transform.Zoom - (Vector2.UnitX + Vector2.UnitY);
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            AssMessageType msgType = (AssMessageType)reader.ReadByte();
            byte playerNumber;
            AssPlayer aPlayer;
            PetPlayer petPlayer;
            byte changes;
            byte index;

            switch (msgType)
            {
                case AssMessageType.SyncPlayerVanity:
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        playerNumber = reader.ReadByte();
                        petPlayer = Main.player[playerNumber].GetModPlayer<PetPlayer>();
                        //no "changes" packet
                        petPlayer.RecvSyncPlayerVanitySub(reader);
                    }
                    break;
                case AssMessageType.ClientChangesVanity:
                    //client and server
                    playerNumber = reader.ReadByte();
                    petPlayer = Main.player[playerNumber].GetModPlayer<PetPlayer>();
                    changes = reader.ReadByte();
                    index = reader.ReadByte();
                    petPlayer.RecvClientChangesPacketSub(reader, changes, index);

                    //server transmits to others
                    if (Main.netMode == NetmodeID.Server)
                    {
                        petPlayer.SendClientChangesPacketSub(changes, index, toClient: -1, ignoreClient: playerNumber);
                    }
                    break;
                case AssMessageType.SyncAssPlayer:
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        playerNumber = reader.ReadByte();
                        aPlayer = Main.player[playerNumber].GetModPlayer<AssPlayer>();
                        aPlayer.shieldDroneReduction = reader.ReadByte();
                    }
                    break;
                case AssMessageType.ClientChangesAssPlayer:
                    //client and server
                    playerNumber = reader.ReadByte();
                    aPlayer = Main.player[playerNumber].GetModPlayer<AssPlayer>();
                    aPlayer.shieldDroneReduction = reader.ReadByte();
                    aPlayer.droneControllerUnlocked = (DroneType)reader.ReadByte();

                    //server transmits to others
                    if (Main.netMode == NetmodeID.Server)
                    {
                        aPlayer.SendClientChangesPacket(toClient: -1, ignoreClient: playerNumber);
                    }
                    break;
                case AssMessageType.ConvertInertSoulsInventory:
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        //convert souls in local inventory
                        aPlayer = Main.LocalPlayer.GetModPlayer<AssPlayer>();
                        aPlayer.ConvertInertSoulsInventory();
                    }
                    break;
                case AssMessageType.GitgudLoadCounters:
                    if (Main.netMode == NetmodeID.Server)
                    {
                        GitgudData.RecvCounters(reader);
                    }
                    break;
                case AssMessageType.GitgudChangeCounters:
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        //GitgudData.RecvReset(Main.myPlayer, reader);
                        GitgudData.RecvChangeCounter(reader);
                    }
                    break;
                case AssMessageType.ResetEmpoweringTimerpvp:
                    //client and server
                    playerNumber = reader.ReadByte();
                    aPlayer = Main.player[playerNumber].GetModPlayer<AssPlayer>();
                    aPlayer.ResetEmpoweringTimer(fromServer: true);

                    //server transmits to others
                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)AssMessageType.ResetEmpoweringTimerpvp);
                        packet.Write((byte)playerNumber);
                        packet.Send(playerNumber); //send to client
                    }
                    break;
                case AssMessageType.WyvernCampfireKill:
                    //reusing playerNumber as the npc.whoami
                    playerNumber = reader.ReadByte();
                    if (Main.npc[playerNumber].type == NPCID.WyvernHead)
                    {
                        DungeonSoulBase.KillInstantly(Main.npc[playerNumber]);
                        if (playerNumber < 200)
                        {
                            NetMessage.SendData(23, -1, -1, null, playerNumber);
                        }
                    }
                    else
                    {
                        for (int k = 0; k < 200; k++)
                        {
                            if (Main.npc[k].active && Main.npc[k].type == NPCID.WyvernHead)
                            {
                                DungeonSoulBase.KillInstantly(Main.npc[k]);
                                if (playerNumber < 200)
                                {
                                    NetMessage.SendData(23, -1, -1, null, k);
                                }
                                break;
                            }
                        }
                    }
                    break;
                default:
                    Logger.Debug("Unknown Message type: " + msgType);
                    break;
            }
        }

        //Credit to jopojelly
        /// <summary>
        /// Makes alpha on .png textures actually properly rendered
        /// </summary>
        public static void PremultiplyTexture(Texture2D texture)
        {
            Color[] buffer = new Color[texture.Width * texture.Height];
            texture.GetData(buffer);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = Color.FromNonPremultiplied(buffer[i].R, buffer[i].G, buffer[i].B, buffer[i].A);
            }
            texture.SetData(buffer);
        }
    }

    public enum AssMessageType : byte
    {
        ClientChangesVanity,
        SyncPlayerVanity,
        ClientChangesAssPlayer,
        SyncAssPlayer,
        ConvertInertSoulsInventory,
        GitgudLoadCounters,
        GitgudChangeCounters,
        ResetEmpoweringTimerpvp,
        WyvernCampfireKill
    }

    public enum PetPlayerChanges : byte
    {
        None,
        All,
        Slots,
        PetTypes
    }
}
