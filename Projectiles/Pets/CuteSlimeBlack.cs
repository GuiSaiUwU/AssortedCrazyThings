using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AssortedCrazyThings.Projectiles.Pets
{
    public class CuteSlimeBlack : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cute Black Slime");
            Main.projFrames[projectile.type] = 10;
            Main.projPet[projectile.type] = true;
            drawOffsetX = -20;
            drawOriginOffsetX = 0;
            drawOriginOffsetY = -22;
        }

        public override void SetDefaults()
        {
            projectile.CloneDefaults(ProjectileID.PetLizard);
            aiType = ProjectileID.PetLizard;
            projectile.scale = 0.9f;
            projectile.alpha = 75;
        }

        public override bool PreAI()
        {
            Player player = Main.player[projectile.owner];
            return true;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            MyPlayer modPlayer = player.GetModPlayer<MyPlayer>(mod);
            if (player.dead)
            {
                modPlayer.CuteSlimeBlack = false;
            }
            if (modPlayer.CuteSlimeBlack)
            {
                projectile.timeLeft = 2;
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            //drawColor.R = 255;
            //// both these do the same in this situation, using these methods is useful.
            //drawColor.G = Utils.Clamp<byte>(drawColor.G, 175, 255);
            //drawColor.B = Math.Min(drawColor.B, (byte)75);
            //drawColor.A = 255;
            drawColor.R = drawColor.R;
            drawColor.G = drawColor.G;
            drawColor.B = drawColor.B;
            drawColor.A = 175;
            return drawColor;
        }
    }
}