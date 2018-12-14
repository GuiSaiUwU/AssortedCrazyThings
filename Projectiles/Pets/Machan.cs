using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AssortedCrazyThings.Projectiles.Pets
{
    public class Machan : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("まーちゃん");
            Main.projFrames[projectile.type] = 3;
            Main.projPet[projectile.type] = true;
            drawOffsetX = -15;
            drawOriginOffsetX = 0;
            drawOriginOffsetY = -15;
        }

        public override void SetDefaults()
        {
            projectile.CloneDefaults(ProjectileID.DD2PetDragon);
            aiType = ProjectileID.DD2PetDragon;
            projectile.scale = 0.5f;
        }

        public override bool PreAI()
        {
            Player player = Main.player[projectile.owner];
            player.petFlagDD2Dragon = false; // Relic from aiType
            return true;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            PetPlayer modPlayer = player.GetModPlayer<PetPlayer>(mod);
            if (player.dead)
            {
                modPlayer.Machan = false;
            }
            if (modPlayer.Machan)
            {
                projectile.timeLeft = 2;
            }
        }
    }
}
