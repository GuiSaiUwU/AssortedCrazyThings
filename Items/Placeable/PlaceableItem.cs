using Terraria.ModLoader;

namespace AssortedCrazyThings.Items.Placeable
{
	/// <summary>
	/// Simple ModItem class tied to a ModTile class, providing the tile type
	/// </summary>
	[Content(ContentType.PlaceablesFunctional)]
	public abstract class PlaceableItem<T> : AssItem where T : ModTile
	{
		public int TileType => ModContent.TileType<T>();
	}
}
