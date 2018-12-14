using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AssortedCrazyThings.Projectiles.Pets
{
	public class TorturedSoul : ModProjectile
		{
			public override void SetStaticDefaults()
				{
					DisplayName.SetDefault("Tortured Soul");
					Main.projFrames[projectile.type] = 4;
					Main.projPet[projectile.type] = true;
				}
			public override void SetDefaults()
				{
					projectile.CloneDefaults(ProjectileID.ZephyrFish);
					aiType = ProjectileID.ZephyrFish;
				}
			public override bool PreAI()
				{
					Player player = Main.player[projectile.owner];
					player.zephyrfish = false; // Relic from aiType
					return true;
				}
			public override void AI()
				{
					Player player = Main.player[projectile.owner];
					MyPlayer modPlayer = player.GetModPlayer<MyPlayer>(mod);
					if (player.dead)
						{
							modPlayer.TorturedSoul = false;
						}
					if (modPlayer.TorturedSoul)
						{
							projectile.timeLeft = 2;
						}
				}
		}
}