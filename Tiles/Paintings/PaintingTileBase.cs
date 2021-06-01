using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace AssortedCrazyThings.Tiles.Paintings
{
	/// <summary>
	/// Base class for all painting tiles
	/// </summary>
	public abstract class PaintingTileBase<T> : DroppableTile<T> where T : ModItem
	{
		//TODO style classes based on dimension
		public override void SetDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
			//TileObjectData.newTile.StyleHorizontal = true;
			//TileObjectData.newTile.StyleWrapLimit = 36;
			TileObjectData.addTile(Type);
			DustType = 7;
			//DisableSmartCursor = true;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(i * 16, j * 16, 32, 32, ItemType);
		}
	}
}
