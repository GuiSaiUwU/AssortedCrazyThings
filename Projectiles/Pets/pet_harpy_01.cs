using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Harblesnargits_Mod_01.Projectiles.Pets
{
	public class pet_harpy_01 : ModProjectile
		{
			public override void SetStaticDefaults()
				{
					DisplayName.SetDefault("Young Harpy");
					Main.projFrames[projectile.type] = 4;
					Main.projPet[projectile.type] = true;
				}
			public override void SetDefaults()
				{
					projectile.CloneDefaults(ProjectileID.BabyHornet);
					aiType = ProjectileID.BabyHornet;
				}
			public override bool PreAI()
				{
					Player player = Main.player[projectile.owner];
					player.hornet = false; // Relic from aiType
					return true;
				}
			public override void AI()
				{
					Player player = Main.player[projectile.owner];
					MyPlayer modPlayer = player.GetModPlayer<MyPlayer>(mod);
					if (player.dead)
						{
							modPlayer.pet_harpy_01 = false;
						}
					if (modPlayer.pet_harpy_01)
						{
							projectile.timeLeft = 2;
						}
				}
		}
}