using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace DungeonAutomata._Project.Editor
{
	public static class SceneSwitcher
	{
		public static bool AutoEnterPlaymode = false;
		public static readonly List<string> ScenePaths = new();
		public static string RootDirectory;

		public static void SwitchScene(string scenePath)
		{
			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
			if (AutoEnterPlaymode) 
				EditorApplication.EnterPlaymode();
		}

		public static void LoadScenes()
		{
			// clear scenes 
			ScenePaths.Clear();
			RootDirectory = "Assets/DungeonAutomata/_Project/_Scenes";

			// find all scenes in the Assets folder
			var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] {RootDirectory});

			foreach (var sceneGuid in sceneGuids)
			{
				var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
				var sceneAsset = AssetDatabase.LoadAssetAtPath(scenePath, typeof(SceneAsset));
				ScenePaths.Add(scenePath);
			}
		}

		public static void SetRootDir(string root_dir)
		{
			RootDirectory = root_dir;
		}
		
	}

	[Icon("d_SceneAsset Icon")]
	[Overlay(typeof(SceneView), OverlayID, "Scene Switcher Overlay")]
	public class SceneSwitcherToolbarOverlay : ToolbarOverlay
	{
		public const string OverlayID = "scene-switcher";

		private SceneSwitcherToolbarOverlay() : base(
			SceneDropdown.ID
			//AutoEnterPlayModeToggle.ID
		)
		{
		}

		public override void OnCreated()
		{
			// load the scenes when the toolbar overlay is initially created
			SceneSwitcher.LoadScenes();
			// subscribe to the event where scene assets were potentially modified
			EditorApplication.projectChanged += OnProjectChanged;
		}

		// Called when an Overlay is about to be destroyed.
		// Usually this corresponds to the EditorWindow in which this Overlay resides closing. (Scene View in this case)
		public override void OnWillBeDestroyed()
		{
			// unsubscribe from the event where scene assets were potentially modified
			EditorApplication.projectChanged -= OnProjectChanged;
		}

		private void OnProjectChanged()
		{
			// reload the scenes whenever scene assets were potentially modified
			SceneSwitcher.LoadScenes();
		}
	}

	[EditorToolbarElement(ID, typeof(SceneView))]
	public class SceneDropdown : EditorToolbarDropdown, ISearchWindowProvider
	{
		public const string ID = SceneSwitcherToolbarOverlay.OverlayID + "/scene-dropdown";

		private const string Tooltip = "Switch scenes";

		public SceneDropdown()
		{
			var content =
				EditorGUIUtility.TrTextContentWithIcon(SceneManager.GetActiveScene().name, Tooltip,
					"d_SceneAsset Icon");
			text = content.text;
			tooltip = content.tooltip;
			icon = content.image as Texture2D;

			ElementAt(1).style.paddingLeft = 5;
			ElementAt(1).style.paddingRight = 5;

			clicked += ToggleDropdown;

			RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
			RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
		}

		protected virtual void OnAttachToPanel(AttachToPanelEvent evt)
		{
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

			EditorApplication.projectChanged += OnProjectChanged;

			EditorSceneManager.sceneOpened += OnSceneOpened;
		}

		protected virtual void OnDetachFromPanel(DetachFromPanelEvent evt)
		{
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

			EditorApplication.projectChanged -= OnProjectChanged;

			EditorSceneManager.sceneOpened -= OnSceneOpened;
		}

		private void OnPlayModeStateChanged(PlayModeStateChange stateChange)
		{
			switch (stateChange)
			{
				case PlayModeStateChange.EnteredEditMode:
					SetEnabled(true);
					break;
				case PlayModeStateChange.EnteredPlayMode:
					SetEnabled(false);
					break;
			}
		}

		private void OnProjectChanged()
		{
			text = SceneManager.GetActiveScene().name;
		}

		private void OnSceneOpened(Scene scene, OpenSceneMode mode)
		{
			text = scene.name;
		}

		private void ToggleDropdown()
		{
			var menu = new GenericMenu();
			foreach (var scenePath in SceneSwitcher.ScenePaths)
			{
				var sceneName = Path.GetFileNameWithoutExtension(scenePath);
				menu.AddItem(new GUIContent(sceneName), text == sceneName,
					() => OnDropdownItemSelected(sceneName, scenePath));
			}

			menu.DropDown(worldBound);
		}

		private void OnDropdownItemSelected(string sceneName, string scenePath)
		{
			text = sceneName;
			SceneSwitcher.SwitchScene(scenePath);
		}

		public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
		{
			List<SearchTreeEntry> search_list = new List<SearchTreeEntry>();
			search_list.Add(new SearchTreeGroupEntry(new GUIContent("Scenes"), 0));
			return search_list;
		}

		public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
		{
			return true;
		}
	}
}