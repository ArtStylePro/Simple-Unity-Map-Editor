using UnityEngine;
using System.Collections;

namespace MapEditor
{
	public class MapGridCell 
	{
		public Deployable InCellObject;
		public MapGrid ParentGrid;
		public bool IsEmpty;
	}
}