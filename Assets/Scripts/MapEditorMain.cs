using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MapEditor
{
	public class MapEditorMain : MonoBehaviour 
	{
		public MapGrid GameMapGrid;

		private bool isLeftMouseDown;
		private bool isRightMouseDown;
		private bool isScrollWheelDown;

		private Vector2 navigateLastPosition = new Vector2 (0, 0);
		private float MaximumOrthographicSize = 20f;
		private float NavigateMoveSpeed = 0.025f;
		private float NavigateZoomSpeed = 2f;

		public static MapEditorMain Instance;

		/// <summary>
		/// 当前选择的刷子类型
		/// </summary>
		private Deployable objectToDeploy;
		public Deployable ObjectToDeploy
		{
			get { return objectToDeploy; }
			set { objectToDeploy = value; }
		}

		public List<Deployable> DeployableList;
		private Dictionary<Deployable.DeployableType, Deployable> deployableTypeDictionary;

		/// <summary>
		/// 在该层上删除物体
		/// </summary>
		private Deployable.DeployLayer currentLayerToErase = Deployable.DeployLayer._Null;

		/// <summary>
		/// 刷子的Prefab
		/// </summary>
		public GameObject DeployableButton;
		public Transform DeployableButtonParent;

		/// <summary>
		/// 上次选中的刷子按钮
		/// </summary>
		private Button lastDeployableButton;
		private Color lastDeployableButtonColor;

		/// <summary>
		/// 在该层上执行选中物体的操作
		/// </summary>
		private Deployable.DeployLayer currentLayerToSelect;
		private Dictionary<int, Deployable.DeployLayer> layerToSelectDictionary;

		/// <summary>
		/// 地图文件加载和保存的目录
		/// </summary>
		private string fileDirectory;
		public string FileDirectory
		{
			get { return fileDirectory; }
		}

		public GameObject FileBrowserPanel;

		public Toggle GameObjectToggle;
		public Toggle BuildableToggle;
		public Toggle WalkableToggle;

		// Use this for initialization
		void Start () {
			Instance = this;
			fileDirectory = Application.dataPath + "/Data/";

			deployableTypeDictionary = new Dictionary<Deployable.DeployableType, Deployable> ();
			DeployableList.Reverse ();
			foreach (Deployable deployableObject in DeployableList) 
			{
				deployableTypeDictionary.Add (deployableObject.GetDeployableType (), deployableObject);

				// dynamically create deployable buttons
				GameObject newButton = (GameObject) Instantiate(DeployableButton);
				newButton.transform.SetParent (DeployableButtonParent);
				newButton.GetComponentInChildren<Text> ().text = deployableObject.GetDeployableType().ToString();

				Button button = newButton.GetComponent<Button> ();
				// this temp variable is useful in this foreach block
				Deployable deployableTemp = deployableObject;
				button.onClick.AddListener (() => OnDeployableButtonClick(deployableTemp, button));
			}

			layerToSelectDictionary = new Dictionary<int, Deployable.DeployLayer> ();
			layerToSelectDictionary.Add (0, Deployable.DeployLayer._Null);
			layerToSelectDictionary.Add (1, Deployable.DeployLayer._WalkableLayer);
			layerToSelectDictionary.Add (2, Deployable.DeployLayer._BuildableLayer);
			layerToSelectDictionary.Add (3, Deployable.DeployLayer._GameObjectLayer);
		}
		
		// Update is called once per frame
		void Update () {
			CreateUpdate ();
			EraseUpdate ();
			NavigateUpdate ();
		}

		private void CreateUpdate()
		{
			if (objectToDeploy) 
			{
				if (Input.GetMouseButtonDown (0)) 
				{
					isLeftMouseDown = true;
				}

				if (isLeftMouseDown) 
				{
					DragCheck ();
				}

				if (Input.GetMouseButtonUp (0)) 
				{
					isLeftMouseDown = false;
				}
			}
		}

		private void DragCheck()
		{
			// If click over a UI element, return
			if (EventSystem.current.IsPointerOverGameObject ())
				return;

			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			GameMapGrid.DeployIfPossible (ray, objectToDeploy);
		}

		private void EraseUpdate()
		{
			if (Input.GetMouseButtonDown (1)) 
			{
				isRightMouseDown = true;
			}

			if (isRightMouseDown) 
			{
				EraseCheck ();
			}

			if (Input.GetMouseButtonUp (1)) 
			{
				isRightMouseDown = false;
			}
		}

		public void OnLayerToSelectChange(int index)
		{
			currentLayerToSelect = layerToSelectDictionary [index];
		}

		private void EraseCheck()
		{
			// If click over a UI element, return
			if (EventSystem.current.IsPointerOverGameObject ())
				return;

			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (currentLayerToErase != Deployable.DeployLayer._Null) 
			{
				GameMapGrid.EraseDeployableObject (ray, currentLayerToErase);
			}
		}

		private void NavigateUpdate()
		{
			if (!FileBrowserPanel.activeSelf) 
			{
				Camera.main.orthographicSize = Mathf.Clamp (
					Camera.main.orthographicSize + (-1 * NavigateZoomSpeed * Input.GetAxis ("Mouse ScrollWheel")),
					MaximumOrthographicSize / 6, MaximumOrthographicSize);
			}
			
			if (Input.GetMouseButtonDown (2)) 
			{
				isScrollWheelDown = true;
				navigateLastPosition = Input.mousePosition;
			}

			if (isScrollWheelDown) 
			{
				Vector2 mPos = Input.mousePosition;
				Vector2 delta = mPos - navigateLastPosition;
				MoveCamera (delta);
				navigateLastPosition = mPos;
			}

			if (Input.GetMouseButtonUp (2)) 
			{
				isScrollWheelDown = false;
			}
		}

		private void MoveCamera(Vector3 movement)
		{
			movement.z = 0;
			movement *= -NavigateMoveSpeed;
			movement *= Camera.main.orthographicSize / 10;

			// rotate the vector for 45 degree camera
			movement = Quaternion.Euler(0, 0, 45) * movement;

			Vector3 pos = Camera.main.transform.position;
			pos += movement;
			Camera.main.transform.position = pos;
		}

		void OnDeployableButtonClick(Deployable deployable, Button button)
		{
			// check whether layer is active
			Transform layerTransform = GameMapGrid.deployableParentDictionary [deployable.GetLayer ()];
			if (!layerTransform.gameObject.activeSelf) 
			{
				return;
			}

			// when clicking self
			if (button == lastDeployableButton) 
			{
				return;
			}

			objectToDeploy = deployable;
			currentLayerToErase = deployable.GetLayer ();

			if (lastDeployableButton) 
			{
				lastDeployableButton.image.color = button.image.color;
			}
			lastDeployableButtonColor = button.image.color;
			button.image.color = new Color (0.5f, 0.5f, 0.5f, button.image.color.a);
			lastDeployableButton = button;
		}

		private void ResetObjectToDeploy()
		{
			objectToDeploy = null;
			currentLayerToErase = Deployable.DeployLayer._Null;

			if (lastDeployableButton)
			{
				lastDeployableButton.image.color = lastDeployableButtonColor;
				lastDeployableButton = null;
			}
		}

		public void OnGameObjectLayerToggle(bool isActive)
		{
			GameMapGrid.GameObjectTransform.gameObject.SetActive (isActive);
			if (objectToDeploy && GameMapGrid.deployableParentDictionary [objectToDeploy.GetLayer ()] == GameMapGrid.GameObjectTransform) 
			{
				ResetObjectToDeploy ();
			}
		}

		public void OnBuildableLayerToggle(bool isActive)
		{
			GameMapGrid.BuildableTransform.gameObject.SetActive (isActive);
			if (objectToDeploy && GameMapGrid.deployableParentDictionary [objectToDeploy.GetLayer ()] == GameMapGrid.BuildableTransform) 
			{
				ResetObjectToDeploy ();
			}
		}

		public void OnWalkableLayerToggle(bool isActive)
		{
			GameMapGrid.WalkableTransform.gameObject.SetActive (isActive);
			if (objectToDeploy && GameMapGrid.deployableParentDictionary [objectToDeploy.GetLayer ()] == GameMapGrid.WalkableTransform) 
			{
				ResetObjectToDeploy ();
			}
		}

		public void SaveMapToJson(string fileName)
		{
			GameMapGrid.SaveMapToJson (fileName);
		}

		public void LoadMapFromJson(string fileName)
		{
			GameMapGrid.LoadMapFromJson (fileName, deployableTypeDictionary);

			// show all layer after loading map
			if (!GameMapGrid.GameObjectTransform.gameObject.activeSelf)
			{
				GameObjectToggle.isOn = true;
				OnGameObjectLayerToggle (true);
			}
			if (!GameMapGrid.BuildableTransform.gameObject.activeSelf)
			{
				BuildableToggle.isOn = true;
				OnBuildableLayerToggle (true);
			}
			if (!GameMapGrid.WalkableTransform.gameObject.activeSelf)
			{
				WalkableToggle.isOn = true;
				OnWalkableLayerToggle (true);
			}
		}
	}
}