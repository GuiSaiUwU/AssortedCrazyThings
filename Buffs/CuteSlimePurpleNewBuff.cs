using AssortedCrazyThings.Projectiles.Pets;

namespace AssortedCrazyThings.Buffs
{
    public class CuteSlimePurpleNewBuff : CuteSlimeBaseBuff
    {
        protected override void MoreSetDefaults()
        {
            DisplayName.SetDefault("Cute Purple Slime");
            Description.SetDefault("A cute purple slime girl is following you");
        }

        protected override void MoreUpdate(PetPlayer mPlayer)
        {
            mPlayer.CuteSlimePurpleNew = true;
            projType = mod.ProjectileType<CuteSlimePurpleNewPet>();
            //this is for the altTexture thing where you can specify which slime has which texture
            color = (byte)CuteSlimeBasePet.PetColor.Purple;
        }
    }
}
