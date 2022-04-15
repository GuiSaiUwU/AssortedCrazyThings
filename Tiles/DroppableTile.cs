using Terraria.ModLoader;

namespace AssortedCrazyThings.Tiles
{
	/// <summary>
	/// Simple ModTile class tied to a ModItem class, providing the item type
	/// </summary>
	[Content(ContentType.PlaceablesFunctional)]
	public abstract class DroppableTile<T> : AssTile where T : ModItem
	{
		public int ItemType => ModContent.ItemType<T>();
	}
}
