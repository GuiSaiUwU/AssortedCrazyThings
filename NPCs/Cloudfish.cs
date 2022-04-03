using AssortedCrazyThings.Base;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace AssortedCrazyThings.NPCs
{
    [Content(ContentType.FriendlyNPCs)]
    public class Cloudfish : AssNPC
    {
        public float scareRange = 200f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cloudfish");
            Main.npcFrameCount[NPC.type] = Main.npcFrameCount[NPCID.Goldfish];
            Main.npcCatchable[NPC.type] = true;
            
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                IsWet = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset[NPC.type] = value;
        }

        public override void SetDefaults()
        {
            NPC.width = 38;
            NPC.height = 36;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 0f;
            NPC.knockBackResist = 0.25f;
            NPC.aiStyle = -1; //custom
            AIType = NPCID.Goldfish;
            AnimationType = NPCID.Goldfish;
            NPC.noGravity = true;
            NPC.catchItem = ItemID.Cloudfish;
            NPC.buffImmune[BuffID.Confused] = false;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneSkyHeight)
            {
                int x = spawnInfo.SpawnTileX;
                int y = spawnInfo.SpawnTileY;
                if (Main.tile[x, y].LiquidAmount == 0)
                {
                    return 0f;
                }
                else if (
                   !WorldGen.SolidTile(x, y) &&
                   !WorldGen.SolidTile(x, y + 1) &&
                   !WorldGen.SolidTile(x, y + 2))
                {
                    return SpawnCondition.Sky.Chance * 4f; //0.05f before, 100f now because water check
                }
            }
            return 0f;
        }

        //public override int SpawnNPC(int tileX, int tileY)
        //{
        //    if (Main.tile[tileX, tileY].LiquidAmount == 0)
        //    {
        //        return 0;
        //    }
        //    else if (
        //       !WorldGen.SolidTile(tileX, tileY) &&
        //       !WorldGen.SolidTile(tileX, tileY + 1) &&
        //       !WorldGen.SolidTile(tileX, tileY + 2))
        //    {
        //        //actually spawn
        //        return base.SpawnNPC(tileX, tileY);
        //    }
        //    return 0;
        //}

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Cloud, minimumDropped: 10, maximumDropped: 20));
            npcLoot.Add(ItemDropRule.Common(ItemID.RainCloud, chanceDenominator: 10, minimumDropped: 10, maximumDropped: 20));
            npcLoot.Add(ItemDropRule.Common(ItemID.SnowCloudBlock, chanceDenominator: 15, minimumDropped: 10, maximumDropped: 20));
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Sky,
				new FlavorTextBestiaryInfoElement("Is it a cloud? Is it a fish? It's both!")
            });
        }

        public override void AI()
        {
            AssAI.ModifiedGoldfishAI(NPC, 200f);
        }
    }
}
