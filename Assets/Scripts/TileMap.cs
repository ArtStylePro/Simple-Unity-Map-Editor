using UnityEngine;
using System.Collections;
using System;

namespace MapEditor
{
	[Serializable]
	public class TileMap
	{
		// only use for Inspector
		public IntVector2 TileSize;
		public IntVector2 TileOffset;
	}

	public static class TileMapUtils
	{
		/// <summary>
		/// TileMap类的扩展方法
		/// </summary>
		public static Vector2 GetWorldTransformOffset(this TileMap tile, float worldCellWidth)
		{
			float x = (tile.TileSize.X/2f*worldCellWidth) - (worldCellWidth/2f);
			float y = tile.TileSize.Y/2f*worldCellWidth - worldCellWidth/2f;

			// Calculating Tile Offset
			x += -tile.TileOffset.X*worldCellWidth;
			y += -tile.TileOffset.Y*worldCellWidth;

			return new Vector2(x, -y);
		}
	}
}