using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MapEditor
{
	public class MapGrid : MonoBehaviour 
	{
		public Transform WalkableTransform;
		public Transform BuildableTransform;
		public Transform GameObjectTransform;

		public Transform PlaneTransform;
		public Transform GridLineTransform;

		[HideInInspector]
		public int Rows;
		[HideInInspector]
		public int Columns;

		public const float GlobalCellWidth = 1;

		public MapGridCell[] WalkableCells;
		public MapGridCell[] BuildableCells;
		public MapGridCell[] GameObjectCells;

		private Dictionary<Deployable.DeployLayer, MapGridCell[]> deployableCellDictionary;

		public Dictionary<Deployable.DeployLayer, Transform> deployableParentDictionary;

		private float boundX;
		private float boundY;
		private float boundZ;

		private const int gridPlaneLayer = 25;

		/// <summary>
		/// 底板方格左下角位置
		/// </summary>
		private Vector3 planeBottomLeftPosition;
		public Vector3 PlaneBottomLeftPosition
		{
			get { return planeBottomLeftPosition; }
		}

		// Use this for initialization
		void Start ()
		{
			PlaneTransform.gameObject.layer = gridPlaneLayer;
			UpdatePlaneDetails();

			WalkableCells = new MapGridCell[Rows * Columns];
			for (int i = 0; i < Rows * Columns; i++) 
			{
				WalkableCells [i] = new MapGridCell { IsEmpty = true, ParentGrid = this };
			}

			BuildableCells = new MapGridCell[Rows * Columns];
			for (int i = 0; i < Rows * Columns; i++) 
			{
				BuildableCells [i] = new MapGridCell { IsEmpty = true, ParentGrid = this };
			}

			GameObjectCells = new MapGridCell[Rows * Columns];
			for (int i = 0; i < Rows * Columns; i++) 
			{
				GameObjectCells [i] = new MapGridCell { IsEmpty = true, ParentGrid = this };
			}

			deployableCellDictionary = new Dictionary<Deployable.DeployLayer, MapGridCell[]> ();
			deployableCellDictionary.Add (Deployable.DeployLayer._WalkableLayer, WalkableCells);
			deployableCellDictionary.Add (Deployable.DeployLayer._BuildableLayer, BuildableCells);
			deployableCellDictionary.Add (Deployable.DeployLayer._GameObjectLayer, GameObjectCells);

			deployableParentDictionary = new Dictionary<Deployable.DeployLayer, Transform> ();
			deployableParentDictionary.Add (Deployable.DeployLayer._WalkableLayer, WalkableTransform);
			deployableParentDictionary.Add (Deployable.DeployLayer._BuildableLayer, BuildableTransform);
			deployableParentDictionary.Add (Deployable.DeployLayer._GameObjectLayer, GameObjectTransform);
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		private void UpdatePlaneDetails()
		{
			boundX = PlaneTransform.GetComponent<Renderer> ().bounds.size.x;
			boundY = PlaneTransform.GetComponent<Renderer> ().bounds.size.y;
			boundZ = PlaneTransform.GetComponent<Renderer> ().bounds.size.z;

			planeBottomLeftPosition = PlaneTransform.position - new Vector3 (boundX / 2f, boundY / 2f, boundZ / 2f);
		}

		/// <summary>
		/// 检查物体能否放入该格子
		/// </summary>
		public bool IsPlaceableWithOffset(Deployable deployableObject, IntVector2 cellIndex)
		{
			TileMap tile = deployableObject.TileMap;
			for (int i = 0; i < tile.TileSize.X; i++) 
			{
				for (int j = 0; j < tile.TileSize.Y; j++) 
				{
					int posX = cellIndex.X + i - tile.TileOffset.X;
					int posY = cellIndex.Y - j + tile.TileOffset.Y;
					if (posX < Columns && posX >= 0 && posY < Rows && posY >= 0) 
					{
						int index = CalculateIndex (posX, posY);
						MapGridCell[] cells = deployableCellDictionary [deployableObject.GetLayer ()];
						if (!cells [index].IsEmpty) return false;
					}
					else
					{
						return false;
					}
				}
			}
			return true;
		}

		public int CalculateIndex(int x, int y)
		{
			if (x < 0 || y < 0)
				return -1;
			return x + y * Rows;
		}

		public int CalculateIndex(IntVector2 index)
		{
			return CalculateIndex (index.X, index.Y);
		}

		public Vector3 IndexToWorldPosition(IntVector2 index)
		{
			Vector3 pos = new Vector3 (planeBottomLeftPosition.x + index.X * GlobalCellWidth,
				              planeBottomLeftPosition.y + index.Y * GlobalCellWidth, 0);
			Vector3 offset = new Vector3 (GlobalCellWidth / 2f, GlobalCellWidth / 2f, 0);
			return pos + offset;
		}

		/// <summary>
		/// Used for deploying object in Editor
		/// </summary>
		public void DeployIfPossible(Ray ray, Deployable deployableObject)
		{
			RaycastHit hitInfo;
			Physics.Raycast (ray, out hitInfo, 100, 1 << gridPlaneLayer);
			if (hitInfo.collider) 
			{
				Vector3 loc = hitInfo.point - planeBottomLeftPosition;
				IntVector2 index = new IntVector2 (loc.x * Columns / boundX, loc.y * Rows / boundY);

				if (IsPlaceableWithOffset (deployableObject, index)) 
				{
					Vector3 pos = IndexToWorldPosition (index);
					Vector2 wOffset = deployableObject.TileMap.GetWorldTransformOffset (GlobalCellWidth);
					pos.x += wOffset.x;
					pos.y += wOffset.y;

					Deployable newCell = (Deployable) Instantiate (deployableObject, pos, Quaternion.identity);
					newCell.transform.parent = deployableParentDictionary [deployableObject.GetLayer ()];
					MapGridCell[] cells = deployableCellDictionary [deployableObject.GetLayer ()];
					newCell.ParentMapGridCell = cells[CalculateIndex (index)];
					newCell.gameObject.layer = (int)deployableObject.GetLayer();
					newCell.GridIndex = index;

					UpdateTilesWithOffset (newCell, index, false);
				}
			}
		}

		/// <summary>
		/// Used for Loading map file
		/// </summary>
		public Deployable DeployIfPossible(IntVector2 index, Deployable deployableObject)
		{
			if (IsPlaceableWithOffset (deployableObject, index)) 
			{
				Vector3 pos = IndexToWorldPosition (index);
				Vector2 wOffset = deployableObject.TileMap.GetWorldTransformOffset (GlobalCellWidth);
				pos.x += wOffset.x;
				pos.y += wOffset.y;

				Deployable newCell = (Deployable) Instantiate (deployableObject, pos, Quaternion.identity);
				newCell.transform.parent = deployableParentDictionary [deployableObject.GetLayer ()];
				MapGridCell[] cells = deployableCellDictionary [deployableObject.GetLayer ()];
				newCell.ParentMapGridCell = cells[CalculateIndex (index)];
				newCell.gameObject.layer = (int)deployableObject.GetLayer();
				newCell.GridIndex = index;

				UpdateTilesWithOffset (newCell, index, false);

				return newCell;
			}

			return null;
		}

		public void UpdateTilesWithOffset(Deployable deployableObject, IntVector2 cellIndex, bool isEmpty)
		{
			for (int i = 0; i < deployableObject.TileMap.TileSize.X; i++) 
			{
				for (int j = 0; j < deployableObject.TileMap.TileSize.Y; j++) 
				{
					int posX = cellIndex.X + i - deployableObject.TileMap.TileOffset.X;
					int posY = cellIndex.Y - j + deployableObject.TileMap.TileOffset.Y;

					if (posX < Columns && posX >= 0 && posY < Rows && posY >= 0) 
					{
						int index = CalculateIndex (posX, posY);
						MapGridCell[] cells = deployableCellDictionary [deployableObject.GetLayer ()];
						cells [index].IsEmpty = isEmpty;
						if (isEmpty) 
						{
							cells [index].InCellObject = null;
						} 
						else 
						{
							cells [index].InCellObject = deployableObject;
						}
					}
				}
			}
		}

		public void EraseDeployableObject(Ray ray, Deployable.DeployLayer selectedLayer)
		{
			RaycastHit hitInfo;
			Physics.Raycast (ray, out hitInfo, 100, 1 << gridPlaneLayer);
			if (hitInfo.collider) 
			{
				Vector3 loc = hitInfo.point - planeBottomLeftPosition;
				IntVector2 index = new IntVector2 (loc.x * Columns / boundX, loc.y * Rows / boundY);

				MapGridCell[] cells = deployableCellDictionary [selectedLayer];
				Deployable toDeleteObject = cells [CalculateIndex (index)].InCellObject;
				if (toDeleteObject)
				{
					UpdateTilesWithOffset (toDeleteObject, toDeleteObject.GridIndex, true);
					Destroy (toDeleteObject.gameObject);
				}
			}
		}

		public Deployable[] GetAllChildren(Deployable.DeployLayer deployLayer)
		{
			Transform parentTransform = deployableParentDictionary [deployLayer];
			var result = new Deployable[parentTransform.childCount];
			for (int i = 0; i < parentTransform.childCount; i++) 
			{
				result [i] = parentTransform.GetChild (i).GetComponent<Deployable> ();
			}
			return result;
		}

		public void ClearEntireGrid()
		{
			ClearGrid (WalkableCells, WalkableTransform);
			ClearGrid (BuildableCells, BuildableTransform);
			ClearGrid (GameObjectCells, GameObjectTransform);
		}

		private void ClearGrid(MapGridCell[] cells, Transform parentTransform)
		{
			for (int i = 0; i < cells.Length; i++) 
			{
				cells [i].IsEmpty = true;
				cells [i].InCellObject = null;
			}

			for (int i = parentTransform.childCount - 1; i >= 0; i--) 
			{
				Destroy (parentTransform.GetChild (i).gameObject);
			}
		}
	}
}