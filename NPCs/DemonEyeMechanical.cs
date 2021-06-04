using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;

namespace AssortedCrazyThings.NPCs
{
    public class DemonEyeMechanical : DemonEyeRecolorBase
    {
        public override int TotalNumberOfThese => 3;

        /*MG = 0
        * MP = 1
        * MR = 2
        */
        public override string Texture
        {
            get
            {
                return "AssortedCrazyThings/NPCs/DemonEyeMechanical_0"; //use fixed texture
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return !NPC.downedMechBoss2 ? 0f : SpawnCondition.OverworldNightMonster.Chance * 0.025f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                new FlavorTextBestiaryInfoElement("Text here.")
            });
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0)
            {
                switch ((int)AiTexture)
                {
                    case 0:
                        Gore.NewGore(NPC.position, NPC.velocity, Mod.Find<ModGore>("DemonEyeGreenGore_0").Type, 1f);
                        break;
                    case 1:
                        Gore.NewGore(NPC.position, NPC.velocity, Mod.Find<ModGore>("DemonEyePurpleGore_0").Type, 1f);
                        break;
                    case 2:
                        Gore.NewGore(NPC.position, NPC.velocity, Mod.Find<ModGore>("DemonEyeRedGore_0").Type, 1f);
                        break;
                    default:
                        break;
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D texture = Mod.GetTexture("NPCs/DemonEyeMechanical_" + AiTexture).Value;
            Vector2 stupidOffset = new Vector2(0f, 0f); //gfxoffY is for when the npc is on a slope or half brick
            SpriteEffects effect = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 drawOrigin = new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);
            Vector2 drawPos = NPC.position - Main.screenPosition + drawOrigin + stupidOffset;
            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effect, 0f);
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D texture = Mod.GetTexture("NPCs/DemonEyeMechanical_Glowmask").Value;
            SpriteEffects effect = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 drawOrigin = new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);
            Vector2 drawPos = NPC.position - Main.screenPosition + drawOrigin + new Vector2(0f, 0f);
            spriteBatch.Draw(texture, drawPos, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effect, 0f);
        }
    }
}
