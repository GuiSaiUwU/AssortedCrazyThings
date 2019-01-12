﻿using AssortedCrazyThings.Items.PetAccessories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace AssortedCrazyThings.Projectiles.Pets
{
    public abstract class CuteSlimeBasePet : ModProjectile
    {
        public const int Projwidth = 28;
        public const int Projheight = 32;
        public const int Texwidth = 28;
        public const int Texheight = 52;

        public override bool PreAI()
        {
            Player player = Main.player[projectile.owner];
            player.lizard = false;
            return true;
        }

        public override void PostAI()
        {
            if(projectile.velocity.Y != 0.1f) projectile.rotation = projectile.velocity.X * 0.01f;
        }

        public virtual bool MorePreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            DrawAccessories(spriteBatch, drawColor, true);
            return MorePreDraw(spriteBatch, drawColor);
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            DrawAccessories(spriteBatch, drawColor);
        }

        private void DrawAccessories(SpriteBatch spriteBatch, Color drawColor, bool preDraw = false)
        {
            PetAccessoryProj gProjectile = projectile.GetGlobalProjectile<PetAccessoryProj>(mod);
            for (byte slotNumber = 1; slotNumber < 5; slotNumber++) //0 is None, reserved
            {
                uint slimeAccessory = gProjectile.GetAccessory(slotNumber);
                if ((preDraw || !PetAccessories.PreDraw[slimeAccessory]) && slimeAccessory != 0)
                {
                    Texture2D texture = PetAccessories.Texture[slimeAccessory];
                    Rectangle frameLocal = new Rectangle(0, projectile.frame * Texheight, texture.Width, texture.Height / 10);
                    SpriteEffects effect = projectile.spriteDirection != 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    Vector2 drawOrigin = new Vector2(Texwidth * 0.5f, Texheight * 0.5f);
                    Vector2 stupidOffset = PetAccessories.Offset[slimeAccessory] + new Vector2(0f, projectile.gfxOffY);

                    //fix for legacy slimes
                    if (Array.IndexOf(AssortedCrazyThings.slimePetLegacy, projectile.type) != -1)
                    {
                        if (!PetAccessories.AllowLegacy[slimeAccessory]) continue;

                        if (slotNumber == (byte)SlotType.Carried)
                        {
                            //stupidOffset.X += -2f;
                            if (projectile.spriteDirection == 1 && projectile.scale <= 1f)
                            {
                                stupidOffset.X += -2f;
                            }

                            stupidOffset.Y += -2f;
                            if (projectile.frame > 2 && projectile.frame < 6)
                            {
                                stupidOffset += new Vector2(2f, -2f);
                            }
                        }

                        if (slotNumber == (byte)SlotType.Body)
                        {
                            if (projectile.frame > 2 && projectile.frame < 6)
                            {
                                stupidOffset += new Vector2(-4f * projectile.spriteDirection, 0f);
                            }
                            else
                            {
                                if (projectile.spriteDirection == 1)
                                {
                                    stupidOffset += new Vector2(-4f, 0f);
                                }
                            }
                        }
                        if (slotNumber == (byte)SlotType.Hat)
                        {
                            if (projectile.spriteDirection == 1)
                            {
                                stupidOffset += new Vector2(-4f, 0f);
                            }

                            if (projectile.type == AssortedCrazyThings.slimePetLegacy[5]) //rainbow slime fix
                            {
                                if (projectile.spriteDirection == 1)
                                {
                                    stupidOffset += new Vector2(4f, 0f);
                                }
                            }

                            if (projectile.frame < 6)
                            {

                                stupidOffset += new Vector2(-2f * projectile.spriteDirection, 2f);

                                if (projectile.frame > 2)
                                {
                                    stupidOffset += new Vector2(-1f * projectile.spriteDirection, 0f);
                                }
                            }
                            else
                            {
                                stupidOffset += new Vector2(-4f * projectile.spriteDirection, 2f);
                            }
                        }
                    }

                    if (slotNumber == (byte)SlotType.Carried)
                    {
                        float handsOffsetX = -22f * projectile.scale + 22f;
                        float handsOffsetY = (projectile.scale < 1) ? 2.5f * projectile.scale - 2.5f : 10f * projectile.scale - 10f;
                        stupidOffset.X += handsOffsetX;
                        stupidOffset.Y += handsOffsetY;
                        //if (projectile.frame > 2 && projectile.frame < 6) //hands "bounce"
                        //{
                        //    // += -4f
                        //    stupidOffset.X += -4f + handsOffsetX / 4f;
                        //}

                        if (projectile.spriteDirection == -1)
                        {
                            stupidOffset.X -= 2 * stupidOffset.X;
                        }
                    }

                    if (slotNumber == (byte)SlotType.Hat)
                    {
                        stupidOffset.Y += (1f - projectile.scale) * 16f;
                    }

                    //(-7.5f * projectile.scale + 7.5f))
                    stupidOffset += new Vector2(0f, drawOriginOffsetY + (-7.5f * projectile.scale + 7.5f));
                    Vector2 drawPos = projectile.position - Main.screenPosition + drawOrigin + stupidOffset;
                    drawColor.A = (byte)(255 - PetAccessories.Alpha[slimeAccessory]);
                    spriteBatch.Draw(texture, drawPos, new Rectangle?(frameLocal), drawColor, projectile.rotation, frameLocal.Size() / 2, projectile.scale, effect, 0f);
                }
            }
        }
    }
}
