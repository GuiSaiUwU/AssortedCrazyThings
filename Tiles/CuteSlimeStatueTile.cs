﻿using AssortedCrazyThings.Base;
using AssortedCrazyThings.Items.Placeable;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace AssortedCrazyThings.Tiles
{
    [Content(ContentType.PlaceablesFunctional | ContentType.CuteSlimes)]
    public class CuteSlimeStatueTile : DroppableTile<CuteSlimeStatueItem>
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
            //TileObjectData.newTile.StyleWrapLimit = 2;
            //TileObjectData.newTile.StyleMultiplier = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Origin = new Point16(1, 2);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 18 };
            
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight; // allows to place it facing the same way as the player
            TileObjectData.addAlternate(1); // facing right will use the second texture style
            TileObjectData.addTile(Type);

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Statue");
            AddMapEntry(new Color(144, 148, 144), name);
            DustType = 11;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 48, ItemType);
        }

        private bool MechSpawn(float x, float y, int[] types)
        {
            int total = 0;
            int veryClose = 0;
            int fairlyClose = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && Array.IndexOf(types, npc.type) != -1)
                {
                    total++;
                    float distance = new Vector2(x, y).Length();
                    if (distance < 200)
                    {
                        veryClose++;
                    }
                    if (distance < 600f)
                    {
                        fairlyClose++;
                    }
                }
            }
            if (veryClose < 3 && fairlyClose < 6)
            {
                return total < 10;
            }
            return false;
        }

        private int CheckSpawns(float x, float y)
        {
            int npcType = -1;
            if (MechSpawn(x, y, SlimePets.slimePetRegularNPCs.ToArray()))
            {
                npcType = Main.rand.Next(SlimePets.slimePetRegularNPCs.ToArray());
            }
            return npcType;
        }

        public override void HitWire(int i, int j)
        {
            // Find the coordinates of top left tile square through math
            Tile tile = Main.tile[i, j];
            int x = i - tile.TileFrameX / 18 % 36;
            int y = j - tile.TileFrameY / 18;

            Wiring.SkipWire(x, y);
            Wiring.SkipWire(x, y + 1);
            Wiring.SkipWire(x, y + 2);
            Wiring.SkipWire(x + 1, y);
            Wiring.SkipWire(x + 1, y + 1);
            Wiring.SkipWire(x + 1, y + 2);

            // We add 16 to x to spawn right between the 2 tiles. We also want to right on the ground in the y direction.
            int spawnX = x * 16 + 16;
            int spawnY = (y + 3) * 16;

            int npcIndex = -1;
            int npcType = CheckSpawns(spawnX, spawnY);
            // 30 is the time before it can be used again. 
            if (npcType != -1 && Wiring.CheckMech(x, y, 30))
            {
                npcIndex = NPC.NewNPC(Wiring.GetNPCSource(i, j), spawnX, spawnY - 8, npcType);
            }
            if (npcIndex >= 0)
            {
                NPC npc = Main.npc[npcIndex];
                npc.value = 0f;
                npc.npcSlots = 0f;
                // Prevents Loot if NPCID.Sets.NoEarlymodeLootWhenSpawnedFromStatue and !Main.HardMode or NPCID.Sets.StatueSpawnedDropRarity != -1 and NextFloat() >= NPCID.Sets.StatueSpawnedDropRarity or killed by traps.
                // Prevents CatchNPC
                npc.SpawnedFromStatue = true;
            }
        }
    }
}
