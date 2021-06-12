using Terraria;

namespace AssortedCrazyThings.Projectiles.Pets.CuteSlimes
{
    public class CuteSlimeRedProj : CuteSlimeBaseProj
    {
        public override ref bool PetBool(Player player) => ref player.GetModPlayer<PetPlayer>().CuteSlimeRed;

        public override void SafeSetStaticDefaults()
        {
            DisplayName.SetDefault("Cute Red Slime");
        }

        public override void SafeSetDefaults()
        {
            Projectile.alpha = 75;
        }
    }
}