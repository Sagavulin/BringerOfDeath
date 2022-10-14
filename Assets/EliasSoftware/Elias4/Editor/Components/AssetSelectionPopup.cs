using System.Collections.Generic;
using System.Linq;
using System;

namespace EliasSoftware.Elias4.Editor
{
    using EliasSoftware.Elias4;
	using EliasSoftware.Elias4.Common;
	using UnityEditor;
    using UnityEngine;

	public class AssetSelectionPopup :
		PopupWindowContent
	{
		private float width;
		private const float MaxHeight = 400f;

		private bool focusSearchField;
		private Vector2 scrollPosition;
		private string searchText;
        private EliasID[] assetIds;
		private GUIContent[] assetNames;
		private GUIContent[] filteredAssetNames;
		private int selectionIndex;
		private GUIContent initialSelection;
		private GUIStyle selectionStyle;
		private GUIContent clearLabel;
		public EliasID SelectedId = EliasID.Invalid;
		public bool ClearField = false;

		private static EliasEditor EditorAPI => Elias.API as EliasEditor;

		private const string SearchFieldName = "SearchField";

		private AssetSelectionPopup(float width, GUIContent[] cont, EliasID[] ids, GUIContent selection)
		{
			this.width = width;
			selectionStyle = new GUIStyle(EditorStyles.toolbarButton)
			{
				alignment = TextAnchor.MiddleLeft,
				padding = new RectOffset(10, 10, 0, 0),
			};

			clearLabel = EditorGUIUtility.IconContent("winbtn_win_close");
			clearLabel.text = "Clear";
			assetNames = cont;
			assetIds = ids;
			initialSelection = selection;
		}

		public static AssetSelectionPopup CreatePopup(float width, ObjectType type, EliasID selected) {
			EntryType entryType = EntryType.Patch;

			if( type == ObjectType.Patch ) {
				entryType = EntryType.Patch;
			} else if ( type == ObjectType.Enum ) {
				entryType = EntryType.Enum;
			} else if ( type == ObjectType.IR ) {
				entryType = EntryType.IR;
			} else if ( type == ObjectType.EnumValue ) {
				entryType = EntryType.EnumValue;

			} else {
				Debug.LogError("Unsupported content type");
			}

			var list = EditorAPI.EditorAssets.Enumerate(entryType).ToList();
			int len = list.Count;
			if(len == 0) {
				//Debug.LogError("No content found");
				return null;
			}

            var names = new GUIContent[len];
            var ids = new EliasID[len];
			GUIContent sel = null;

            int cntr = 0;
            foreach(var e in list) {
                names[cntr] = new GUIContent(e.Item1);
                ids[cntr] = (EliasID)e.Item2;
                if(selected.Equals(e.Item2))
                    sel = names[cntr];
                cntr++;
            }

			return new AssetSelectionPopup(width, names, ids, sel);
		}

		public override Vector2 GetWindowSize()
		{
			var itemHeight = EditorGUIUtility.singleLineHeight;
			var lineHeight = itemHeight + EditorGUIUtility.standardVerticalSpacing;
			var staticItemCount = 3; // Clear + search + extra padding at bottom
			var preferedHeight = lineHeight * staticItemCount + itemHeight * assetNames.Length;
			return new Vector2(width, Mathf.Min(MaxHeight, preferedHeight));
		}

		public override void OnOpen()
		{
			focusSearchField = true;
			UpdateFilter();
		}

		private void UpdateFilter()
		{
			if (string.IsNullOrEmpty(searchText))
			{
				filteredAssetNames = assetNames;
			}
			else
			{
				var upperSearch = searchText.ToUpperInvariant();
				filteredAssetNames = assetNames.Where(n => n.text.ToUpperInvariant().Contains(upperSearch)).ToArray();
			}

			SetSelection(ArrayUtility.IndexOf(filteredAssetNames, initialSelection));
		}

		private void SetSelection(int index)
		{
			selectionIndex = Mathf.Clamp(index, 0, filteredAssetNames.Length - 1);
			scrollPosition.y = selectionIndex * EditorGUIUtility.singleLineHeight;
		}
		private void MoveSelection(int delta) => SetSelection(selectionIndex + delta);

		private void ActivateCurrentSelection()
		{
			//GUIContent content = null;
            //EliasID id = EliasID.Invalid;
			if (selectionIndex >= 0) {
				//content = filteredAssetNames[selectionIndex];
                SelectedId = assetIds[selectionIndex];
            }
			editorWindow.Close();
		}

		public override void OnGUI(Rect rect)
		{
			var ev = Event.current;
			if (ev.type == EventType.KeyDown)
			{
				switch (ev.keyCode)
				{
					case KeyCode.UpArrow:
						MoveSelection(-1);
						ev.Use();
						break;
					case KeyCode.DownArrow:
						MoveSelection(1);
						ev.Use();
						break;
					case KeyCode.PageUp:
						MoveSelection(-5);
						ev.Use();
						break;
					case KeyCode.PageDown:
						MoveSelection(5);
						ev.Use();
						break;
					case KeyCode.Escape:
						editorWindow.Close();
						ev.Use();
						break;
					case KeyCode.Return:
						ActivateCurrentSelection();
						ev.Use();
						break;
				}
			}

			using (var change = new EditorGUI.ChangeCheckScope())
			{
				if (GUILayout.Button(clearLabel, selectionStyle))
				{
					selectionIndex = -1;
					ClearField = true;
					ActivateCurrentSelection();
				}

				GUI.SetNextControlName(SearchFieldName);
				searchText = EditorGUILayout.TextField(searchText, EditorStyles.toolbarSearchField);

				if (focusSearchField)
				{
					EditorGUI.FocusTextInControl(SearchFieldName);
					focusSearchField = false;
				}

				if (change.changed)
					UpdateFilter();
			}

			using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition))
			{
				scrollPosition = scroll.scrollPosition;

				using (var change = new EditorGUI.ChangeCheckScope())
				{
					selectionIndex = GUILayout.SelectionGrid(selectionIndex, filteredAssetNames, xCount: 1, selectionStyle);

					if (change.changed)
					{
						ActivateCurrentSelection();
					}
				}

				EditorGUILayout.Space();
			}
		}
	}

}
