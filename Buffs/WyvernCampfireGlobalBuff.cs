﻿using Terraria;
using Terraria.ID;

namespace AssortedCrazyThings.Buffs
{
    [Content(ContentType.PlaceablesFunctional)]
    public class WyvernCampfireGlobalBuff : AssGlobalBuff
    {
        //Tells you its effects in the regular campfire buff (which the wyvern campfire applies)
        //Logic handled in WyvernCampfireTile.NearbyEffects and AssWorld.ResetNearbyTileEffects
        public override void ModifyBuffTip(int type, ref string tip, ref int rare)
        {
            if (type == BuffID.Campfire && Main.LocalPlayer.GetModPlayer<AssPlayer>().wyvernCampfire)
            {
                tip += "\nWyvern Campfire nearby" +
                    "\nYou are protected from Wyverns" +
                    "\nHarpy feathers don't knock you away";
            }
        }
    }
}
