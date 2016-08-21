using UnityEngine;
using System.Collections;

namespace MapEditor
{
	public abstract class Deployable : MonoBehaviour 
	{
		private IntVector2 gridIndex;
		public IntVector2 GridIndex
		{
			get { return gridIndex; }
			set { gridIndex = value; }
		}

		public MapGridCell ParentMapGridCell;

		[SerializeField]
		public TileMap TileMap;

		public abstract DeployableType GetDeployableType();

		public enum DeployableType
		{
			_Walkable,
			_Buildable,
			_Monster,
			_Player,
		}

		public  DeployLayer GetLayer()
		{
			if (GetDeployableType () == DeployableType._Walkable) 
			{
				return DeployLayer._WalkableLayer;
			}
			else if (GetDeployableType () == DeployableType._Buildable) 
			{
				return DeployLayer._BuildableLayer;
			}
			else
			{
				return DeployLayer._GameObjectLayer;
			}
		}

		public enum DeployLayer
		{
			_Null,
			_WalkableLayer = 28,
			_BuildableLayer = 29,
			_GameObjectLayer = 30,
		}
	}
}