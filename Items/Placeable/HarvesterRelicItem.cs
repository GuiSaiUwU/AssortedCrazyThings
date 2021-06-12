using AssortedCrazyThings.Tiles;
using Terraria;
using Terraria.ID;

namespace AssortedCrazyThings.Items.Placeable
{
    public class HarvesterRelicItem : PlaceableItem<HarvesterRelicTile>
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Soul Harvester Relic");
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlacableTile(TileType);
            Item.width = 30;
            Item.height = 40;
            Item.maxStack = 99;
            Item.rare = ItemRarityID.Master;
            Item.master = true;
            Item.value = Item.buyPrice(0, 5);
        }
    }
}