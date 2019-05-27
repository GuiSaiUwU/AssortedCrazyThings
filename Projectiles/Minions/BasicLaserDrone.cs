﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using AssortedCrazyThings.Base;

namespace AssortedCrazyThings.Projectiles.Minions
{
    /// <summary>
    /// Fires a weak laser rapidly
    /// </summary>
    public class BasicLaserDrone : DroneBase
    {
        public override string Texture
        {
            get
            {
                return "AssortedCrazyThings/Projectiles/Minions/HealingDrone";
            }
        }

        private static readonly string nameGlow = "Projectiles/Minions/" + "HealingDrone_Glowmask";
        private static readonly string nameLower = "Projectiles/Minions/" + "HealingDrone_Lower";
        private static readonly string nameLowerGlow = "Projectiles/Minions/" + "HealingDrone_Lower_Glowmask";

        private const int AttackDelay = 30; //actually 20 but because incremented by 1.5f

        private const byte STATE_IDLE = 0;
        private const byte STATE_TARGET_FOUND = 1;
        private const byte STATE_TARGET_FIRE = 2;

        private byte AI_STATE = 0;
        private int Direction = -1;
        private float addRotation; //same
        private NPC Target;

        public int Counter
        {
            get
            {
                return (int)projectile.ai[0];
            }
            set
            {
                projectile.ai[0] = value;
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Basic Laser Drone");
            Main.projFrames[projectile.type] = 6;
            Main.projPet[projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
        }

        public override void SetDefaults()
        {
            projectile.CloneDefaults(ProjectileID.DD2PetGhost);
            projectile.aiStyle = -1;
            projectile.width = 38;
            projectile.height = 30;
            projectile.alpha = 0;
            projectile.minion = true;
            projectile.minionSlots = 1f;
        }

        protected override void CustomFrame(int frameCounterMaxFar = 4, int frameCounterMaxClose = 8)
        {
            //frame 0, 1: above two thirds health
            //frame 2, 3: above half health, below two thirds health
            //frame 4, 5: below half health, healing
            Player player = Main.player[projectile.owner];

            int frameOffset = 0; //frame 0, 1

            if (AI_STATE == STATE_TARGET_FIRE) //frame 4, 5
            {
                frameOffset = 4;
            }
            else if (AI_STATE == STATE_TARGET_FOUND) //frame 2, 3
            {
                frameOffset = 2;
            }
            else
            {
                //frameoffset 0
            }
            
            if (projectile.frame < frameOffset) projectile.frame = frameOffset;

            if (projectile.velocity.Length() > 6f)
            {
                if (++projectile.frameCounter >= frameCounterMaxFar)
                {
                    projectile.frameCounter = 0;
                    if (++projectile.frame >= 2 + frameOffset)
                    {
                        projectile.frame = frameOffset;
                    }
                }
            }
            else
            {
                if (++projectile.frameCounter >= frameCounterMaxClose)
                {
                    projectile.frameCounter = 0;
                    if (++projectile.frame >= 2 + frameOffset)
                    {
                        projectile.frame = frameOffset;
                    }
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D image = Main.projectileTexture[projectile.type];
            Rectangle bounds = new Rectangle();
            bounds.X = 0;
            bounds.Width = image.Bounds.Width;
            bounds.Height = image.Bounds.Height / Main.projFrames[projectile.type];
            bounds.Y = projectile.frame * bounds.Height;

            SpriteEffects effects = projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Vector2 stupidOffset = new Vector2(projectile.width / 2, (projectile.height - 8f) + sinY);
            Vector2 drawPos = projectile.position - Main.screenPosition + stupidOffset;
            Vector2 drawOrigin = bounds.Size() / 2;

            spriteBatch.Draw(image, drawPos, bounds, lightColor, projectile.rotation, drawOrigin, 1f, effects, 0f);

            image = mod.GetTexture(nameGlow);
            spriteBatch.Draw(image, drawPos, bounds, Color.White, projectile.rotation, drawOrigin, 1f, effects, 0f);

            Vector2 rotationOffset = new Vector2(0f, -4f); //-2f
            drawPos += rotationOffset;
            drawOrigin += rotationOffset;

            //AssUtils.ShowDustAtPos(135, projectile.position + stupidOffset);

            //AssUtils.ShowDustAtPos(136, projectile.position + stupidOffset - drawOrigin);

            //rotation origin is (projectile.position + stupidOffset) - drawOrigin; //not including Main.screenPosition
            image = mod.GetTexture(nameLower);
            spriteBatch.Draw(image, drawPos, bounds, lightColor, addRotation, drawOrigin, 1f, effects, 0f);

            image = mod.GetTexture(nameLowerGlow);
            spriteBatch.Draw(image, drawPos, bounds, Color.White, addRotation, drawOrigin, 1f, effects, 0f);

            return false;
        }

        protected override bool ModifyDefaultAI(ref bool staticDirection, ref bool reverseSide, ref float veloXToRotationFactor, ref float veloSpeed, ref float offsetX, ref float offsetY)
        {
            if (AI_STATE == STATE_TARGET_FIRE)
            {
                Vector2 between = Target.Center - projectile.Center;
                //between.Length(): 100 is "close", 1000 is "edge of map"
                //15.6f = 1000f / 64f
                float magnitude = Utils.Clamp(between.Length() / 15.6f, 6f, 64f);
                between.Normalize();
                Vector2 offset = between * magnitude;
                offset.Y *= 0.5f;
                offsetX += offset.X;
                offsetY += (offset.Y > 0) ? -(32 - offset.Y) : 0;
            }
            return true;
        }

        protected override void CustomAI()
        {
            Player player = Main.player[projectile.owner];
            //Main.NewText("State: " + AI_STATE);
            //Main.NewText("frame: " + projectile.frame);
            //Main.NewText("Counter: " + Counter);

            #region Handle State
            int targetIndex = AssAI.FindTarget(projectile, projectile.Center, 1300);
            if (targetIndex != -1)
            {
                AI_STATE = STATE_TARGET_FOUND;

                targetIndex = AssAI.FindTarget(projectile, projectile.Center, 900);
                if (targetIndex != -1)
                {
                    AI_STATE = STATE_TARGET_FIRE;
                }
                else
                {
                    AI_STATE = STATE_IDLE;
                }
            }
            else
            {
                AI_STATE = STATE_IDLE;
            }

            if (AI_STATE != STATE_TARGET_FIRE)
            {
                Direction = player.direction;
            }
            else
            {
                Target = Main.npc[targetIndex];
                Direction = (Target.Center.X - projectile.Center.X > 0f).ToDirectionInt();
            }
            #endregion

            projectile.spriteDirection = projectile.direction = -Direction;

            Counter += Main.rand.Next(1, 3);

            if (AI_STATE == STATE_TARGET_FIRE)
            {
                Vector2 shootOffset = new Vector2(projectile.width / 2, (projectile.height - 2f) + sinY);
                Vector2 shootOrigin = projectile.position + shootOffset;
                Vector2 target = Target.Center + new Vector2(0f, -5f);

                Vector2 between = target - shootOrigin;
                shootOrigin += Vector2.Normalize(between) * 16f; //roughly tip of turret

                addRotation = between.ToRotation();

                if (projectile.spriteDirection == 1) //adjust rotation based on direction
                {
                    addRotation -= (float)Math.PI;
                    if (addRotation > 2 * Math.PI)
                    {
                        addRotation = -addRotation;
                    }
                }

                bool canShoot = shootOrigin.Y < target.Y + Target.height / 2;

                if (projectile.spriteDirection == -1) //reset canShoot properly if rotation is too much (aka target is too fast for the drone to catch up)
                {
                    if (addRotation <= projectile.rotation)
                    {
                        canShoot = false;
                        addRotation = projectile.rotation;
                    }
                }
                else
                {
                    if (addRotation <= projectile.rotation - Math.PI)
                    {
                        canShoot = false;
                        addRotation = projectile.rotation;
                    }
                }

                if (canShoot) //when target below drone
                {
                    if (Counter > AttackDelay)
                    {
                        Counter = 0;
                        if (RealOwner)
                        {
                            if (targetIndex != -1 && !Collision.SolidCollision(shootOrigin, 1, 1))
                            {
                                Vector2 position = shootOrigin;
                                between = target + Target.velocity * 6f - shootOrigin;
                                between.Normalize();
                                between *= 6f;
                                Projectile.NewProjectile(position, between, mod.ProjectileType<PetDestroyerDroneLaser>(), projectile.damage, projectile.knockBack, Main.myPlayer, 0f, 0f);

                                //projectile.netUpdate = true;
                            }
                        }
                    }
                }
                else
                {
                    AI_STATE = STATE_IDLE;
                }
            }
            else //if no target, addRotation should go down to projectile.rotation
            {
                //if addRotation is bigger than projectile.rotation by a small margin, reduce it down to projectile.rotation slowly
                if (Math.Abs(addRotation) > Math.Abs(projectile.rotation) + 0.006f)
                {
                    float rotDiff = projectile.rotation - addRotation;
                    if (Math.Abs(rotDiff) < 0.005f)
                    {
                        addRotation = projectile.rotation;
                    }
                    else
                    {
                        addRotation += addRotation * -0.15f;
                    }
                }
                else
                {
                    //fix rotation so it doesn't get adjusted anymore
                    addRotation = projectile.rotation;
                }
            }
        }
    }
}