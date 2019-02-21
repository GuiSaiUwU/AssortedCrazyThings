using Terraria;

namespace AssortedCrazyThings.Projectiles.Pets
{
    //check this file for more info vvvvvvvv
    public class IlluminantSlimeProj : BabySlimeBase
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Illuminant Slime");
            Main.projFrames[projectile.type] = 6;
            Main.projPet[projectile.type] = true;
            drawOffsetX = 0;
            drawOriginOffsetY = 4;
        }

        public override void MoreSetDefaults()
        {
            //used to set dimensions and damage (if there is, defaults to 0)
            projectile.width = 52;
            projectile.height = 38;
			projectile.alpha = 0;

            Damage = 0;
        }

        public override bool PreAI()
        {
            PetPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<PetPlayer>(mod);
            if (Main.player[projectile.owner].dead)
            {
                modPlayer.IlluminantSlimePet = false;
            }
            if (modPlayer.IlluminantSlimePet)
            {
                projectile.timeLeft = 2;
            }
            return true;
        }
    }
}