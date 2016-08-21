using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

namespace MapEditor
{
	public class LoadFilePanel : MonoBehaviour {

		public Transform FileListParent;
		public GameObject FileButton;
		public InputField inputField;
		public Button LoadButton;
		public Button SaveButton;
		public Button cancelButton;

		/// <summary>
		/// NeedSaveFile为True时，展示保存界面
		/// NeedSaveFile为False时，展示加载界面
		/// </summary>
		private bool needSaveFile;
		public bool NeedSaveFile
		{
			get { return needSaveFile; }
			set { needSaveFile = value; }
		}

		private static string lastLoadedFileName;

		void Start () {
			// add listener for Load File Button
			LoadButton.onClick.AddListener (() => {
				string fileName = inputField.text;
				if (!string.IsNullOrEmpty(fileName))
				{
					MapEditorMain.Instance.LoadMapFromJson(fileName);
					lastLoadedFileName = fileName;
					cancelButton.onClick.Invoke();
				}
			});

			SaveButton.onClick.AddListener (() => {
				string fileName = inputField.text;
				if (!string.IsNullOrEmpty(fileName))
				{
					MapEditorMain.Instance.SaveMapToJson(fileName);
					cancelButton.onClick.Invoke();
				}
			});
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		void OnEnable()
		{
			if (gameObject.activeSelf) 
			{
				// dynamically create a file list
				DirectoryInfo folder = new DirectoryInfo (MapEditorMain.Instance.FileDirectory);
				foreach (FileInfo file in folder.GetFiles("*.json")) 
				{
					GameObject fileButton = (GameObject)Instantiate (FileButton);
					string fileShortName = Path.GetFileNameWithoutExtension (file.FullName);
					fileButton.GetComponentInChildren<Text> ().text = fileShortName;
					fileButton.transform.SetParent (FileListParent);

					if (!needSaveFile) 
					{
						Button button = fileButton.GetComponent<Button> ();
						button.onClick.AddListener (() => OnFileButtonClick(button));
					}
				}

				LoadButton.gameObject.SetActive (!needSaveFile);
				SaveButton.gameObject.SetActive (needSaveFile);
				inputField.placeholder.GetComponent<Text> ().text = needSaveFile ? "Input file name..." : "Select A File...";
				if (needSaveFile && !string.IsNullOrEmpty (lastLoadedFileName)) 
				{
					inputField.placeholder.GetComponent<Text> ().text = lastLoadedFileName;
				}
				inputField.interactable = needSaveFile;
			}
		}

		void OnDisable()
		{
			if (!gameObject.activeSelf) 
			{
				// clear the file list
				for (int i = FileListParent.childCount - 1; i >= 0; i--) 
				{
					Destroy (FileListParent.GetChild (i).gameObject);
				}
			}
		}

		void OnFileButtonClick(Button button)
		{
			inputField.text = button.GetComponentInChildren<Text> ().text;
		}
	}
}