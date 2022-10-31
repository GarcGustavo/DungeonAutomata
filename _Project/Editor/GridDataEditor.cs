using System;
using System.Linq;
using DungeonAutomata._Project.Scripts._Common;
using DungeonAutomata._Project.Scripts.Data;
using DungeonAutomata._Project.Scripts.Utilities;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace DungeonAutomata._Project.Editor
{
	public class GridDataEditor : OdinMenuEditorWindow
	{
		private static Type[] typesToDisplay = TypeCache.GetTypesWithAttribute<ManageableData>()
			.OrderBy(m => m.Name)
			.ToArray();

		private Type selectedType;

		[MenuItem("Tools/UnitEditor")]
		private static void OpenEditor() => GetWindow<GridDataEditor>();

		protected override void OnGUI()
		{
			//draw menu tree for SOs and other assets
			if (GUIUtils.SelectButtonList(ref selectedType, typesToDisplay))
				this.ForceMenuTreeRebuild();

			base.OnGUI();
		}

		protected override OdinMenuTree BuildMenuTree()
		{
			var tree = new OdinMenuTree();
			if (selectedType != null)
			{
				tree.Add("Create New", new CreateNewScriptableData(selectedType.Name));
				tree.AddAllAssetsAtPath(selectedType.Name, 
					"Assets/", selectedType, true, true)
					.AddThumbnailIcons();
			}
			return tree;
		}
		public class CreateNewScriptableData
		{

			public string ObjectName = "New Object";
			[InlineEditor(Expanded = true)]
			public ScriptableObject data;
			
			public CreateNewScriptableData(string selectedTypeName)
			{
				switch (selectedTypeName)
				{
					case "EnemyData":
						var enemyData = CreateInstance<EnemyData>();
						data = enemyData;
						break;
					case "ItemData":
						var itemData = CreateInstance<ItemData>();
						data = itemData;
						break;
					case "CellData":
						var cellData = CreateInstance<CellData>();
						data = cellData;
						break; 
					case "PlayerData":
						var playerData = CreateInstance<PlayerData>();
						data = playerData;
						break; 
					case "ShapeData":
						var shapeData = CreateInstance<ShapeData>();
						data = shapeData;
						break; 
					default:
						data = null;
						break;
				}
			}

			[Button("Add New Data Object")]
			private void CreateNewData()
			{
				AssetDatabase.CreateAsset(data,
					"Assets/_Project/ScriptableObjects/GridUnits/" + ObjectName + ".asset");
				AssetDatabase.SaveAssets();
			}
		}

		protected override void OnBeginDrawEditors()
		{
			OdinMenuTreeSelection selected = MenuTree.Selection;

			SirenixEditorGUI.BeginHorizontalToolbar();
			{
				GUILayout.FlexibleSpace();

				if (SirenixEditorGUI.ToolbarButton("Delete Current"))
				{
					ScriptableObject asset = selected.SelectedValue as ScriptableObject;
					string path = AssetDatabase.GetAssetPath(asset);
					AssetDatabase.DeleteAsset(path);
					AssetDatabase.SaveAssets();
				}

			}
			SirenixEditorGUI.EndHorizontalToolbar(); 
		}
	}
}