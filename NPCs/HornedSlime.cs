using AssortedCrazyThings.Items.Pets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace AssortedCrazyThings.NPCs
{
    [Content(ContentType.HostileNPCs)]
    public class HornedSlime : AssNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Horned Slime");
            Main.npcFrameCount[NPC.type] = Main.npcFrameCount[NPCID.ToxicSludge];
            Main.npcCatchable[NPC.type] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 36;
            NPC.height = 32;
            NPC.damage = 7;
            NPC.defense = 2;
            NPC.lifeMax = 25;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 25f;
            NPC.knockBackResist = 0.25f;
            NPC.aiStyle = 1;
            AIType = NPCID.ToxicSludge;
            AnimationType = NPCID.ToxicSludge;
            NPC.alpha = 175;
            NPC.color = new Color(240, 54, 115, 100);
            NPC.catchItem = (short)ModContent.ItemType<HornedSlimeItem>();
            NPC.lavaImmune = true;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            Color color = NPC.color;
            if (NPC.life > 0)
            {
                for (int i = 0; i < damage / NPC.lifeMax * 100f; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 4, hitDirection, -1f, NPC.alpha, color);
                }
            }
            else
            {
                for (int i = 0; i < 40; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 4, 2 * hitDirection, -2f, NPC.alpha, color);
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return SpawnCondition.Underworld.Chance * 0.015f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Gel));
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
                new FlavorTextBestiaryInfoElement("The horn isn't just for decoration; it is capable of dispersing heat, allowing the slime to remain gelatinous.")
            });
        }
    }
}
