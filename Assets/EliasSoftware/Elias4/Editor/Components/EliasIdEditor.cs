using System.Collections.Generic;
using System.Linq;
using System;

namespace EliasSoftware.Elias4.Editor
{
    using EliasSoftware.Elias4;
	using EliasSoftware.Elias4.Common;
	using UnityEditor;
    using UnityEngine;

	public abstract class EliasIdEditor :
		PropertyDrawer
	{
		protected abstract ObjectType ManagedObjectType { get; }
		protected abstract int NumIDs { get; }
		public EliasID FirstID { get; private set; }
		public EliasID SecondID { get; private set; }
		public string FirstName { get; private set; }
		public string SecondName { get; private set; }
		protected abstract void SerializeIDObject(SerializedProperty property);
		protected virtual void OnShowDropdown(Rect position) {
			popup = AssetSelectionPopup.CreatePopup(position.width,
					ManagedObjectType,
					FirstID);

			if(popup != null)
				PopupWindow.Show(position, popup);
		}

		private bool Clear => popup != null && popup.ClearField;

		protected virtual EliasID SelectedFirstID
		{
			get {
				if(popup != null)
					return popup.SelectedId;
				return EliasID.Invalid;
			}
		}
		protected virtual EliasID SelectedSecondID
		{
			get => EliasID.Invalid;
		}

		protected virtual void ResetSelected() {
			if(popup != null && popup.editorWindow)
				popup.editorWindow.Close();
			popup = null;
		}

		private AssetSelectionPopup popup = null;

		protected static EliasEditor EditorAPI => Elias.API as EliasEditor;

		private string GetFirstNameFromAssets(EliasID first)
		{
			string name = null;

			switch(ManagedObjectType) {
				case ObjectType.Patch:			// Fallthrough
				case ObjectType.PatchParam:
				{
					name = EditorAPI.EditorAssets.GetName((IPatchID)first);
				} break;
				case ObjectType.Enum:			// Fallthrough
				case ObjectType.EnumValue:
				{
					name = EditorAPI.EditorAssets.GetName((IEnumID)first);
				} break;
				case ObjectType.IR: {
					name = EditorAPI.EditorAssets.GetName((IIRID)first);
				} break;
				default: {
					name = null;
				} break;
			}
			return name;
		}

		private string GetSecondNameFromAssets(EliasID firstId, EliasID secondId)
		{
			string name = null;

			switch(ManagedObjectType) {
				case ObjectType.EnumValue: {
					name = EditorAPI.EditorAssets.GetName((IEnumID)firstId, (IEnumValueID)secondId);
				} break;
				case ObjectType.PatchParam: {
					name = EditorAPI.EditorAssets.GetName((IPatchID)firstId, (IParameterID)secondId);
				} break;
				default: {
					name = null;
				} break;
			}
			return name;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			using (var propScope = new EditorGUI.PropertyScope(position, label, property))
			{
				var firstID = EliasSerializableID.FirstIDFromProperty(property);
				var propName = EliasSerializableID.FirstNameFromProperty(property);
				var dropdown = new GUIContent(propName);

				if( Elias.IsInitialized == false ) {
					var prevColor = GUI.color;
					GUI.color = Color.gray;
					dropdown.text = "Elias project unavailable";
					EditorGUI.LabelField(position, propScope.content, dropdown);
					GUI.color = prevColor;
					return;
				}

				if (NumIDs == 1 && Application.isPlaying)
				{
					EditorGUI.LabelField(position, propScope.content, dropdown);
					return;
				}

				if( NumIDs > 1 ) {

					var secondID = EliasSerializableIDPair.SecondIDFromProperty(property);
					var secondName = GetSecondNameFromAssets(firstID, secondID);
					if(secondName == null)
						dropdown.text = " --- ";
					else
						dropdown.text += " : " + secondName;

					if (Application.isPlaying) {
						EditorGUI.LabelField(position, propScope.content, dropdown);
						return;
					}

					var controlPosition = EditorGUI.PrefixLabel(position, propScope.content);
					if ( EditorGUI.DropdownButton(controlPosition, dropdown, FocusType.Keyboard) ) {
						OnShowDropdown(controlPosition);
					}

					if ( SelectedSecondID.IsValid && SelectedFirstID.IsValid )
					{
						FirstID = SelectedFirstID;
						FirstName = GetFirstNameFromAssets(SelectedFirstID);
						SecondID = SelectedSecondID;
						SecondName = GetSecondNameFromAssets(FirstID, SecondID);
						SerializeIDObject(property);
						GUI.changed = true;
						ResetSelected();
					}
				}
				else {
					var firstName = GetFirstNameFromAssets(firstID);
					dropdown.text = firstName == null ? " --- " : firstName;
					var controlPosition = EditorGUI.PrefixLabel(position, propScope.content);
					if ( EditorGUI.DropdownButton(controlPosition, dropdown, FocusType.Keyboard) )
					{
						OnShowDropdown(controlPosition);
					}

					if ( SelectedFirstID.IsValid || Clear )
					{
						FirstID = SelectedFirstID;
						FirstName = GetFirstNameFromAssets(SelectedFirstID);
						popup = null;
						SerializeIDObject(property);
						GUI.changed = true;
						ResetSelected();
					}
					else
					{
						if( firstName != null && firstName != propName )
						{
							FirstName = firstName;
							FirstID = firstID;
							GUI.changed = true;
							SerializeIDObject(property);
						}
					}
				}
			}
		}
	}

	[CustomPropertyDrawer(typeof(EliasPatch))]
	public class EliasPatchEditor :
		EliasIdEditor
	{
		protected override ObjectType ManagedObjectType => ObjectType.Patch;
		private List<(string, IParameterID, ParamType, string)> paramList;
		protected override int NumIDs => 1;
		protected override void SerializeIDObject(SerializedProperty property)
		{
			paramList = EditorAPI.EditorAssets.EnumeratePatchParameters(FirstID).ToList();
			EliasPatch.Serialize(FirstID, FirstName, paramList, property);
		}
	}

	[CustomPropertyDrawer(typeof(EliasPatchParameter))]
	public class EliasPatchParameterEditor :
		EliasIdEditor
	{
		protected override ObjectType ManagedObjectType => ObjectType.PatchParam;
		protected override int NumIDs => 2;
		protected override EliasID SelectedSecondID => (EliasID)selected.Item2.Item2;
		protected override EliasID SelectedFirstID => (EliasID)selected.Item2.Item1;
		protected override void ResetSelected()
		{
			selected = ((string.Empty,string.Empty), (EliasID.Invalid, EliasID.Invalid));
		}

		protected override void SerializeIDObject(SerializedProperty property)
		{
			var info = EditorAPI.Assets.GetParameterInfo(FirstID, SecondName);
			EliasPatchParameter.Serialize(FirstID, SecondID, FirstName, SecondName, info.Item2, info.Item3, property);
		}

		((string, string), (IEliasID, IEliasID)) selected = ((string.Empty,string.Empty), (EliasID.Invalid, EliasID.Invalid));

		private void HandleSelect(object sel) {
			selected = (((string, string), (IEliasID, IEliasID)))sel;
		}

		private IEnumerable<((string, string), (IEliasID, IEliasID))> Alts() {
			foreach(var patch in EditorAPI.EditorAssets.EnumeratePatches()) {
				foreach(var param in EditorAPI.EditorAssets.EnumeratePatchParameters(patch.Item2))
					yield return ((patch.Item1, param.Item1), (patch.Item2, param.Item2));
			}
		}

		private GenericMenu BuildMenu() {
			var menu = new GenericMenu();
			foreach(var e in Alts())
				AddMenuItem(menu, e);
			return menu;
		}

		private void AddMenuItem(GenericMenu menu, ((string, string), (IEliasID, IEliasID)) pair)
		{
			menu.AddItem(new GUIContent(pair.Item1.Item1 + "/" + pair.Item1.Item2), selected.Item1.Equals(pair.Item1), HandleSelect, pair);
		}

		protected override void OnShowDropdown(Rect position)
		{
			selected = ((FirstName, SecondName),(FirstID, SecondID));
			var menu = BuildMenu();
			menu.DropDown(position);
		}
	}


	[CustomPropertyDrawer(typeof(EliasEnumID))]
	public class EliasEnumEditor :
		EliasIdEditor
	{
		protected override ObjectType ManagedObjectType => ObjectType.Enum;
		protected override int NumIDs => 1;
		protected override void SerializeIDObject(SerializedProperty property)
		{
			var valueDomain = EditorAPI.EditorAssets.EnumerateEnumDomain(FirstID).ToList();
			EliasEnumID.Serialize(FirstID, FirstName, valueDomain, property);
		}
	}

	[CustomPropertyDrawer(typeof(EliasIRID))]
	public class EliasIREditor :
		EliasIdEditor
	{
		protected override ObjectType ManagedObjectType => ObjectType.IR;
		protected override int NumIDs => 1;
		protected override void SerializeIDObject(SerializedProperty property)
		{
			EliasSerializableID.Serialize(ManagedObjectType, FirstID, FirstName, property);
		}
	}


	[CustomPropertyDrawer(typeof(EliasEnumValueID))]
	public class EliasEnumValueIDEditor :
		EliasIdEditor
	{
		protected override ObjectType ManagedObjectType => ObjectType.EnumValue;
		protected override int NumIDs => 2;
		protected override EliasID SelectedSecondID => (EliasID)selected.Item2.Item2;
		protected override EliasID SelectedFirstID => (EliasID)selected.Item2.Item1;

		protected override void ResetSelected()
		{
			selected = ((string.Empty,string.Empty), (EliasID.Invalid, EliasID.Invalid));
		}

		((string, string), (IEliasID, IEliasID)) selected = ((string.Empty,string.Empty), (EliasID.Invalid, EliasID.Invalid));

		void HandleSelect(object sel) {
			selected = (((string, string), (IEliasID, IEliasID)))sel;
		}

		IEnumerable<((string, string), (IEliasID, IEliasID))> Alts() {
			foreach(var _enum in EditorAPI.EditorAssets.EnumerateEnums()) {
				foreach(var val in EditorAPI.EditorAssets.EnumerateEnumDomain(_enum.Item2))
					yield return ((_enum.Item1, val.Item1), (_enum.Item2, val.Item2));
			}
		}

		GenericMenu BuildMenu() {
			var menu = new GenericMenu();
			foreach(var e in Alts())
				AddMenuItem(menu, e);
			return menu;
		}

		void AddMenuItem(GenericMenu menu, ((string, string), (IEliasID, IEliasID)) pair)
		{
			menu.AddItem(new GUIContent(pair.Item1.Item1 + "/" + pair.Item1.Item2), selected.Item1.Equals(pair.Item1), HandleSelect, pair);
		}

		protected override void OnShowDropdown(Rect position)
		{
			selected = ((FirstName, SecondName),(FirstID, SecondID));
			var menu = BuildMenu();
			menu.DropDown(position);
		}
		protected override void SerializeIDObject(SerializedProperty property)
		{
			EliasEnumValueID.Serialize(FirstID, SecondID, FirstName, SecondName, property);
		}
	}

}
