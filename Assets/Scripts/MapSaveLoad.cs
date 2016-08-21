using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

namespace MapEditor
{
	public static class MapSaveLoad
	{
		public static void SaveMapToJson(this MapGrid mapGrid, string fileName)
		{
			Deployable[] walkablTtiles = mapGrid.GetAllChildren (Deployable.DeployLayer._WalkableLayer);
			Deployable[] buildableTtiles = mapGrid.GetAllChildren (Deployable.DeployLayer._BuildableLayer);
			Deployable[] gameObjectTtiles = mapGrid.GetAllChildren (Deployable.DeployLayer._GameObjectLayer);

			List<Deployable> monsterStartList = new List<Deployable> ();
			List<Deployable> playerStartList = new List<Deployable> ();
			foreach (Deployable tile in gameObjectTtiles)
			{
				if (tile.GetDeployableType () == Deployable.DeployableType._Monster) 
				{
					monsterStartList.Add (tile);
				} 
				else if (tile.GetDeployableType () == Deployable.DeployableType._Player) 
				{
					playerStartList.Add (tile);
				}
			}

			string json = "{";

			// pathfinding data
			json += "\"map\":[";
			for (int i = 0; i < mapGrid.Rows; i++) 
			{
				if (i != 0) json += ",";
				json += "[";
				for (int j = 0; j < mapGrid.Columns; j++) 
				{
					if (j != 0) json += ",";
					string result = mapGrid.WalkableCells [mapGrid.CalculateIndex (i, j)].IsEmpty ? "0" : "1";
					json += result;
				}
				json += "]";
			}
			json += "]";

			// walkable point
			json += ",\"walkable\":[";
			for (int i = 0; i < walkablTtiles.Length; i++) 
			{
				if (i != 0) json += ",";
				json += "[";
				json += walkablTtiles [i].GridIndex.X;
				json += ",";
				json += walkablTtiles [i].GridIndex.Y;
				json += "]";
			}
			json += "]";

			// buildable point
			json += ",\"buildable\":[";
			for (int i = 0; i < buildableTtiles.Length; i++) 
			{
				if (i != 0) json += ",";
				json += "[";
				json += buildableTtiles [i].GridIndex.X;
				json += ",";
				json += buildableTtiles [i].GridIndex.Y;
				json += "]";
			}
			json += "]";

			// monster start point
			json += ",\"monster\":[";
			for (int i = 0; i < monsterStartList.Count; i++) 
			{
				if (i != 0) json += ",";
				json += "[";
				json += monsterStartList [i].GridIndex.X;
				json += ",";
				json += monsterStartList [i].GridIndex.Y;
				json += "]";
			}
			json += "]";

			// player start point
			json += ",\"player\":[";
			for (int i = 0; i < playerStartList.Count; i++) 
			{
				if (i != 0) json += ",";
				json += "[";
				json += playerStartList [i].GridIndex.X;
				json += ",";
				json += playerStartList [i].GridIndex.Y;
				json += "]";
			}
			json += "]";

			json += "}";

			String path = MapEditorMain.Instance.FileDirectory + fileName + ".json";
			using (StreamWriter sw = new StreamWriter (path, false)) 
			{
				sw.Write (json);
			}
			Debug.Log ("Map Saved:" + path);
		}

		public static void LoadMapFromJson(this MapGrid mapGrid, string fileName, Dictionary<Deployable.DeployableType, Deployable> deployableDictionary)
		{
			String path = MapEditorMain.Instance.FileDirectory + fileName + ".json";
			if (!File.Exists (path)) 
			{
				Debug.LogError ("File Not Exist:" + path);
				return;
			}
			
			mapGrid.ClearEntireGrid();

			string jsonString;
			using (StreamReader sr = new StreamReader (path)) 
			{
				jsonString = sr.ReadToEnd ();
			}
			Dictionary<string,object> json = MiniJSON.Json.Deserialize (jsonString) as Dictionary<string,object>;

			// walkable data
			List<object> walkable = (List<object>)json ["walkable"];
			for (int i = 0; i < walkable.Count; i++)
			{
				List<object> point = (List<object>)walkable [i];
				IntVector2 index = new IntVector2 (Convert.ToInt32 (point [0]), Convert.ToInt32 (point [1]));
				mapGrid.DeployIfPossible(index, deployableDictionary[Deployable.DeployableType._Walkable]);
			}

			// buildable data
			List<object> buildable = (List<object>)json ["buildable"];
			for (int i = 0; i < buildable.Count; i++)
			{
				List<object> point = (List<object>)buildable [i];
				IntVector2 index = new IntVector2 (Convert.ToInt32 (point [0]), Convert.ToInt32 (point [1]));
				mapGrid.DeployIfPossible(index, deployableDictionary[Deployable.DeployableType._Buildable]);
			}

			// monster start point
			List<object> monster = (List<object>)json ["monster"];
			for (int i = 0; i < monster.Count; i++)
			{
				List<object> point = (List<object>)monster [i];
				IntVector2 index = new IntVector2 (Convert.ToInt32 (point [0]), Convert.ToInt32 (point [1]));
				mapGrid.DeployIfPossible(index, deployableDictionary[Deployable.DeployableType._Monster]);
			}

			// player start point
			List<object> player = (List<object>)json ["player"];
			for (int i = 0; i < player.Count; i++)
			{
				List<object> point = (List<object>)player [i];
				IntVector2 index = new IntVector2 (Convert.ToInt32 (point [0]), Convert.ToInt32 (point [1]));
				mapGrid.DeployIfPossible(index, deployableDictionary[Deployable.DeployableType._Player]);
			}
		}
	}
}