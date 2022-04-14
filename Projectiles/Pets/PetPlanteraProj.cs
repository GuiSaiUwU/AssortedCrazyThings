using AssortedCrazyThings.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AssortedCrazyThings.Projectiles.Pets
{
    //cannot be dyed since it counts as a minion and deals damage
    [Content(ContentType.DroppedPets)]
    public class PetPlanteraProj : SimplePetProjBase
    {
        public const int ContactDamage = 20;
        public const int ImmunityCooldown = 60;

        private const float STATE_IDLE = 0f;
        private const float STATE_ATTACK = 1f;

        public float AI_STATE
        {
            get
            {
                return Projectile.ai[0];
            }
            set
            {
                Projectile.ai[0] = value;
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Plantera Sprout");
            Main.projFrames[Projectile.type] = 2;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.BabyEater);
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.minion = false; //only determines the damage type
            //minion = false to prevent it from being "replaced" after casting other summons and then spawning its tentacles again
            Projectile.minionSlots = 0f;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = ImmunityCooldown;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool MinionContactDamage()
        {
            return true;
        }

        public override void AI()
        {
            Player player = Projectile.GetOwner();
            PetPlayer modPlayer = player.GetModPlayer<PetPlayer>();
            if (player.dead)
            {
                modPlayer.PetPlantera = false;
            }
            if (modPlayer.PetPlantera)
            {
                Projectile.timeLeft = 2;
            }

            #region Handle State
            int targetIndex = AssAI.FindTarget(Projectile, player.Center, 300); //check for player surrounding
            if (targetIndex == -1)
            {
                if (AI_STATE == STATE_ATTACK)
                {
                    targetIndex = AssAI.FindTarget(Projectile, player.Center, 400); //check for player surrounding
                    if (targetIndex == -1)
                    {
                        AI_STATE = STATE_IDLE;
                        Projectile.netUpdate = true;
                    }
                }
                else
                {
                    //keep idling
                }
            }
            else //target found
            {
                if (AI_STATE == STATE_IDLE)
                {
                    AI_STATE = STATE_ATTACK;
                    Projectile.netUpdate = true;
                }
                else
                {
                    //keep attacking
                }
            }
            #endregion

            #region Act Upon State
            if (AI_STATE == STATE_IDLE)
            {
                Projectile.friendly = false;
                AssAI.BabyEaterAI(Projectile, originOffset: new Vector2(0f, -60f));

                AssAI.BabyEaterDraw(Projectile);
                Projectile.rotation += 3.14159f;
            }
            else //STATE_ATTACK
            {
                Projectile.friendly = true;

                if (targetIndex != -1)
                {
                    NPC npc = Main.npc[targetIndex];
                    Vector2 distanceToTargetVector = npc.Center - Projectile.Center;
                    float distanceToTarget = distanceToTargetVector.Length();

                    if (distanceToTarget > 30f)
                    {
                        distanceToTargetVector.Normalize();
                        distanceToTargetVector *= 8f;
                        Projectile.velocity = (Projectile.velocity * (16f - 1) + distanceToTargetVector) / 16f;

                        Projectile.rotation = distanceToTargetVector.ToRotation() + 1.57f;
                    }
                }

                AssAI.BabyEaterDraw(Projectile, 4);
            }
            #endregion
        }

        public override bool PreDraw(ref Color lightColor)
        {
            int tentacleCount = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active && Projectile.owner == projectile.owner && projectile.type == ModContent.ProjectileType<PetPlanteraProjTentacle>())
                {
                    AssUtils.DrawTether("AssortedCrazyThings/Projectiles/Pets/PetPlanteraProj_Chain", projectile.Center, Projectile.Center);
                    tentacleCount++;
                }
                if (tentacleCount >= 4) break;
            }
            AssUtils.DrawTether("AssortedCrazyThings/Projectiles/Pets/PetPlanteraProj_Chain", Projectile.GetOwner().Center, Projectile.Center);
            return true;
        }
    }

    [Content(ContentType.DroppedPets)]
    public class PetPlanteraProjTentacle : SimplePetProjBase
    {
        //since the index might be different between clients, using ai[] for it will break stuff
        public int ParentIndex
        {
            get
            {
                return (int)Projectile.localAI[0] - 1;
            }
            set
            {
                Projectile.localAI[0] = value + 1;
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mean Seed Tentacle");
            Main.projFrames[Projectile.type] = 2;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.ZephyrFish);
            Projectile.aiStyle = -1;
            Projectile.width = 14; //14
            Projectile.height = 19; //19
            //gets set in the buff
            //projectile.damage = 1; //to prevent dyes from working on it
        }

        public override void AI()
        {
            Player player = Projectile.GetOwner();
            PetPlayer modPlayer = player.GetModPlayer<PetPlayer>();
            if (player.dead)
            {
                modPlayer.PetPlantera = false;
            }
            if (modPlayer.PetPlantera)
            {
                Projectile.timeLeft = 2;
            }

            #region Find Parent
            //set parent when spawned
            if (ParentIndex < 0)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<PetPlanteraProj>() && Projectile.owner == Main.projectile[i].owner)
                    {
                        ParentIndex = i;
                        //projectile.netUpdate = true;
                        break;
                    }
                }
            }

            //if something goes wrong, abort mission
            if (ParentIndex < 0 || (ParentIndex > -1 && Main.projectile[ParentIndex].type != ModContent.ProjectileType<PetPlanteraProj>()))
            {
                Projectile.Kill();
                return;
            }
            #endregion

            //offsets so the tentacles are distributed evenly
            float offsetX = 0;
            float offsetY = 0;
            switch (Projectile.whoAmI % 4)
            {
                case 0:
                    offsetX = -120 + Main.rand.Next(20);
                    offsetY = 0;
                    break;
                case 1:
                    offsetX = -120 + Main.rand.Next(20);
                    offsetY = 120;
                    break;
                case 2:
                    offsetX = 0 - Main.rand.Next(20);
                    offsetY = 120;
                    break;
                default: //case 3
                    break;
            }

            Projectile parent = Main.projectile[ParentIndex];
            if (!parent.active)
            {
                Projectile.active = false;
                return;
            }

            //velocityFactor: 1.5f + (projectile.whoAmI % 4) * 0.8f so all tentacles don't share the same movement 
            AssAI.ZephyrfishAI(Projectile, parent: parent, velocityFactor: 1.5f + (Projectile.whoAmI % 4) * 0.8f, random: true, swapSides: 1, offsetX: offsetX, offsetY: offsetY);
            Vector2 between = parent.Center - Projectile.Center;
            Projectile.spriteDirection = 1;
            Projectile.rotation = between.ToRotation();

            AssAI.ZephyrfishDraw(Projectile, 3 + Main.rand.Next(3));
        }
    }
}
