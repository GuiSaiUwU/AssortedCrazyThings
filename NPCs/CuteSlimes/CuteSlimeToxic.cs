using AssortedCrazyThings.Base;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AssortedCrazyThings.NPCs.CuteSlimes
{
    public class CuteSlimeToxic : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cute Toxic Slime");
            Main.npcFrameCount[npc.type] = Main.npcFrameCount[NPCID.ToxicSludge];
        }

        public override void SetDefaults()
        {
            npc.width = 32;
            npc.height = 52;
            //npc.friendly = true;
            npc.chaseable = false;
            npc.damage = 0;
            npc.defense = 2;
            npc.lifeMax = 20;
            npc.rarity = 1;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.value = 25f;
            npc.knockBackResist = 0.25f;
            npc.aiStyle = 1;
            aiType = NPCID.ToxicSludge;
            animationType = NPCID.ToxicSludge;
            npc.alpha = 75;
            Main.npcCatchable[mod.NPCType("CuteSlimeToxic")] = true;
            npc.catchItem = (short)mod.ItemType("CuteSlimeToxicNew");
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return SlimePets.CuteSlimeSpawnChance(spawnInfo, SlimePets.SpawnConditionType.Underground);
        }

        public override void NPCLoot()
        {
            Item.NewItem(npc.getRect(), ItemID.Gel);
        }
    }
}