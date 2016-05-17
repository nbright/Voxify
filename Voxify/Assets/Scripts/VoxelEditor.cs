using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using SimpleJSON;

public enum EditorMode {
	BRUSH = 0,
	SELECTOR = 1,
	SEGMENT = 2,
	ANIMATE = 3
}

public class VoxelEditor : MonoBehaviour {

	public static Dictionary<string, Group> Groups = new Dictionary<string, Group>();
	
	public GameObject GroupPrefab;
	public Camera MainCamera;

	public KeyData PlaceVoxelKey = new KeyData(KeyCode.A, KeyState.DOWN);
	public KeyData DeleteVoxelKey = new KeyData(KeyCode.S, KeyState.DOWN);

	public KeyData FillSelectionKey = new KeyData(KeyCode.A);
	public KeyData EmptySelectionKey = new KeyData(KeyCode.S);
	public KeyData ClearSelectionKey = new KeyData(KeyCode.D);
	public KeyData DeselectMod = new KeyData(KeyCode.LeftShift);

	public KeyData SelectKey = new KeyData(KeyCode.Mouse0);
	public KeyData OrbitKey = new KeyData(KeyCode.Mouse1);
	public KeyData RotateKey = new KeyData(KeyCode.Mouse1, KeyState.HELD, KeyCode.LeftAlt);
	public KeyData PanKey = new KeyData(KeyCode.Mouse2);
	public KeyData CursorDistanceMod = new KeyData(KeyCode.LeftAlt);
	
	public KeyData BrushKey = new KeyData(KeyCode.Q, KeyState.DOWN);
	public KeyData SelectorKey = new KeyData(KeyCode.W, KeyState.DOWN);
	public KeyData SegmentKey = new KeyData(KeyCode.E, KeyState.DOWN);
	public KeyData AnimateKey = new KeyData(KeyCode.R, KeyState.DOWN);

	public KeyData NewModelKey = new KeyData(KeyCode.N, KeyState.DOWN, KeyCode.LeftControl);
	public KeyData SaveModelKey = new KeyData(KeyCode.S, KeyState.DOWN, KeyCode.LeftControl);
	public KeyData SaveModelAsKey = new KeyData(KeyCode.S, KeyState.DOWN, KeyCode.LeftControl, KeyCode.LeftShift);
	public KeyData OpenModelKey = new KeyData(KeyCode.O, KeyState.DOWN, KeyCode.LeftControl);
	public KeyData ExportModelKey = new KeyData(KeyCode.E, KeyState.DOWN, KeyCode.LeftControl);

	public KeyData UndoKey = new KeyData(KeyCode.Z, KeyState.DOWN, KeyCode.LeftControl);
	public KeyData RedoKey = new KeyData(KeyCode.Y, KeyState.DOWN, KeyCode.LeftControl);

	public GameObject CursorObject;
	public Vector3 VoxelCursorPosition;
	public float VoxelCursorDistance;
	public float VoxelCursorZoomSpeed;
	public float VoxelCursorDistanceMin;
	public float VoxelCursorDistanceMax;

	public RaycastHit CursorHit;

	internal List<WorldPos> SelectedPositions = new List<WorldPos>();
	public Color SelectedColor = Color.blue;
	internal float SelectionPingPong = 0f;
	public float SelectionPingPongLength = 1f;

	internal Transform TargetTransform;
	internal Vector3 LastMousePosition;
	public float PanSpeed = .5f;

	public float RotateSpeedX = 1f;
	public float RotateSpeedY = 1f;
	internal float RotationX = 0f;
	internal float RotationY = 0f;

	internal Group WorkingGroup;
	internal int VoxelCount = -1;
	internal bool ModelChangedSinceSave = true;

	internal EditorMode CurrentMode = EditorMode.BRUSH;

	internal bool ShowToolbars = true;

	public GUIStyle VersionTextStyle;
	public GUIStyle GeneralStyle;

	public Rect MenuRect;
	public Rect ToolRect;

	internal int SelectedMenuItem = -1;
	internal int SelectedToolItem = 0;

	public const string MENU_NEW = "New";
	public const string MENU_SAVE = "Save";
	public const string MENU_SAVE_AS = "Save As";
	public const string MENU_OPEN = "Open";
	public const string MENU_EXPORT = "Export";

	public const string TOOL_BRUSH = "Brush";
	public const string TOOL_SELECTOR = "Selector";
	public const string TOOL_SEGMENT = "Segment";
	public const string TOOL_ANIMATE = "Animate";

	internal string[] m_MenuItems = new string[] { MENU_NEW, MENU_SAVE, MENU_SAVE_AS, MENU_OPEN, MENU_EXPORT };
	internal string[] m_ToolItems = new string[] { TOOL_BRUSH, TOOL_SELECTOR, TOOL_SEGMENT, TOOL_ANIMATE };

	void Start() {
		MainCamera = GetComponent<Camera>();

		TargetTransform = GetComponent<UltimateOrbitCamera>().target;

		NewStage();
	}

	#region Update

	void Update() {

		UpdateCamera();

		UpdateModals();

		UpdateTools();

		switch (CurrentMode) {
			case EditorMode.ANIMATE:

				// TODO: Handle animation

				break;
			case EditorMode.BRUSH:

				UpdateBrushCursor();

				UpdateBrush();

				break;
			case EditorMode.SELECTOR:
				
				UpdateSelectorCursor();

				UpdateSelector();

				break;
			case EditorMode.SEGMENT:

				// TODO: Handle segment translation, rotation, and scaling

				break;
		}

		UpdateUI();
	}

	void UpdateCamera() {
		if (!SelectKey) {

			if (PanKey) {
				Vector3 deltaMousePos = Util.SubtractVector3(Input.mousePosition, LastMousePosition);
				deltaMousePos.y *= -1;

				TargetTransform.LookAt(transform);
				TargetTransform.Translate(deltaMousePos * PanSpeed, Space.Self);
			}
			else if (RotateKey) {
				// TODO: Rotate CameraTarget around camera.
			}
		}
	}

	void UpdateModals() {
		if (ShowToolbars) {

			if (NewModelKey) NewModelPressed();
			else if (SaveModelKey) SaveModelPressed();
			else if (SaveModelAsKey) SaveModelAsPressed();
			else if (OpenModelKey) OpenModelPressed();
			else if (ExportModelKey) ExportModelPressed();

		}
	}

	void UpdateTools() {
		if (BrushKey && CurrentMode != EditorMode.BRUSH) {
			BrushToolPressed();
		}
		else if (SelectorKey && CurrentMode != EditorMode.SELECTOR) {
			SelectorToolPressed();
		}
		else if (SegmentKey && CurrentMode != EditorMode.SEGMENT) {
			SegmentToolPressed();
		}
		else if (AnimateKey && CurrentMode != EditorMode.ANIMATE) {
			AnimateToolPressed();
		}
		SelectedToolItem = (int)CurrentMode;
	}

	void UpdateBrushCursor() {
		Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 100f)) {
			CursorObject.SetActive(true);
			CursorHit = hit;

			VoxelCursorPosition = GetVoxelPos(hit, true).ToVector3();
			CursorObject.transform.position = VoxelCursorPosition;
		}
		else {
			CursorObject.SetActive(false);
		}
	}

	void UpdateBrush() {
		if (PlaceVoxelKey) {
			if (CursorObject.activeInHierarchy) {
				SetVoxel(CursorHit, new Voxel() {
					VoxelColor = SelectedColor
				}, true);
			}
		}
		else if (DeleteVoxelKey) {
			if (CursorObject.activeInHierarchy) {
				SetVoxel(CursorHit, new VoxelAir(), false);
			}
		}
	}

	void UpdateSelectorCursor() {
		if (CursorDistanceMod) {
			VoxelCursorDistance += (Input.GetAxis("Mouse ScrollWheel") * VoxelCursorZoomSpeed);
		}

		VoxelCursorDistance = Mathf.Clamp(VoxelCursorDistance, VoxelCursorDistanceMin, VoxelCursorDistanceMax);

		VoxelCursorPosition = MainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, VoxelCursorDistance));

		VoxelCursorPosition = Util.RoundVector3(VoxelCursorPosition);

		CursorObject.transform.position = VoxelCursorPosition;
	}

	void UpdateSelector() {
		SelectionPingPong = Mathf.PingPong(Time.time * SelectionPingPongLength, 0.75f);

		if (FillSelectionKey) FillSelection();
		if (EmptySelectionKey) EmptySelection();
		if (ClearSelectionKey) ClearSelection();
		if (SelectKey) {

			if (DeselectMod) {
				DeselectPosition(new WorldPos(VoxelCursorPosition));
			}
			else {
				WorldPos pos = new WorldPos(Mathf.RoundToInt(VoxelCursorPosition.x), Mathf.RoundToInt(VoxelCursorPosition.y), Mathf.RoundToInt(VoxelCursorPosition.z));
				// Only select positions which have no already been selected and are valid positions.
				if (!SelectedPositions.Contains(pos) && WorkingGroup.GetVoxel(pos) != null) {
					WorkingGroup.GetVoxel(pos).Selected = true;
					SelectedPositions.Add(pos);
				}
			}
		}
	}

	void UpdateUI() {
		ToolRect.y = Util.ScreenSize.y - ToolRect.height;

		LastMousePosition = Input.mousePosition;
	}

	#endregion

	#region Stage Management

	public Color GetModifiedColor(Voxel voxel) {
		return Color.Lerp(voxel.VoxelColor, voxel.ModColor, SelectionPingPong);
	}

	public void FillSelection() {
		if (SelectedPositions.Count == 0) {
			WorkingGroup.SetVoxel(Mathf.RoundToInt(VoxelCursorPosition.x),
				Mathf.RoundToInt(VoxelCursorPosition.y),
				Mathf.RoundToInt(VoxelCursorPosition.z),
				new Voxel() {
					VoxelColor = SelectedColor
				});
			return;
		}
		foreach (WorldPos pos in SelectedPositions) {
			WorkingGroup.SetVoxel(pos.X, pos.Y, pos.Z, new Voxel() {
				VoxelColor = SelectedColor,
				Selected = true,
			});
		}
	}

	public void EmptySelection() {
		if (SelectedPositions.Count == 0) {
			WorkingGroup.SetVoxel(Mathf.RoundToInt(VoxelCursorPosition.x),
				Mathf.RoundToInt(VoxelCursorPosition.y),
				Mathf.RoundToInt(VoxelCursorPosition.z),
				new VoxelAir());
			return;
		}
		foreach (WorldPos pos in SelectedPositions) {
			WorkingGroup.SetVoxel(pos.X, pos.Y, pos.Z, new VoxelAir() {
				Selected = true
			});
		}
	}

	public void ClearSelection() {
		if (SelectedPositions.Count > 0) {
			Voxel[] selectedVoxels = new Voxel[SelectedPositions.Count];

			for (int i = 0; i < SelectedPositions.Count; i++) {
				WorldPos pos = SelectedPositions[i];
				selectedVoxels[i] = WorkingGroup.GetVoxel(pos);
			}

			foreach (Voxel v in selectedVoxels) {
				v.Selected = false;
			}

			SelectedPositions.Clear();
		}
	}

	public void DeselectPosition(WorldPos pos) {
		if (!SelectedPositions.Contains(pos))
			return;

		WorkingGroup.GetVoxel(pos).Selected = false;
		SelectedPositions.Remove(pos);
	}

	public void NewModelPressed() {
		ShowToolbars = false;

		MessageBox.Show("Any unsaved changes will be lost!",
			"Create a new model?",
			(result) => {
				if (result == DialogResult.Yes) NewStage();
				ShowToolbars = true;
			},
			MessageBoxButtons.YesNo);
	}

	public void SaveModelPressed() {
		/*

		TODO: If new model, fall through to save as.
		otherwise, save to current save file.

		*/
		File.WriteAllText("test.vpj", GetJSON().ToString());
	}

	public void SaveModelAsPressed() {

		// TODO: Show save file dialog for saving.
	}

	public void OpenModelPressed() {
		ShowToolbars = false;

		MessageBox.Show("Any unsaved changes will be lost!",
						"Open a different model?",
						(result) => {
							// TODO: If result == yes, show open file dialog box.
							ShowToolbars = true;
						},
						MessageBoxButtons.YesNo);
	}

	public void ExportModelPressed() {

		// TODO: Show save file dialog for exporting.
	}

	public void BrushToolPressed() {
		CurrentMode = EditorMode.BRUSH;

		CursorObject.SetActive(false);
		ClearSelection();
	}

	public void SelectorToolPressed() {
		CurrentMode = EditorMode.SELECTOR;

		CursorObject.SetActive(true);
	}

	public void SegmentToolPressed() {
		// TODO: Swap to segment mode
	}

	public void AnimateToolPressed() {
		// TODO: Swap to animation mode
	}

	public void NewStage() {
		foreach (Group g in FindObjectsOfType<Group>()) {
			Destroy(g.gameObject);
		}

		Groups.Clear();

		VoxelCount = 0;

		WorkingGroup = Instantiate(GroupPrefab).GetComponent<Group>();
	}

	public JSONNode GetJSON() {
		JSONNode N = new JSONClass();

		JSONArray groups = new JSONArray();
		foreach(KeyValuePair<string, Group> groupKVP in Groups) {
			Group group = groupKVP.Value;
			JSONNode jsonGroup = new JSONClass();

			jsonGroup["name"] = groupKVP.Key;

			JSONArray chunks = new JSONArray();

			foreach (KeyValuePair<WorldPos, Chunk> chunkKVP in group.Chunks) {
				Chunk chunk = chunkKVP.Value;
				JSONNode jsonChunk = new JSONClass();

				jsonChunk["pos"]["x"] = chunkKVP.Key.X.ToString();
				jsonChunk["pos"]["y"] = chunkKVP.Key.Y.ToString();

				JSONArray voxels = new JSONArray();

				for (int i = 0; i < Chunk.ChunkSize; i++) {
					for (int j = 0; j < Chunk.ChunkSize; j++) {
						for (int k = 0; k < Chunk.ChunkSize; k++) {
							JSONNode voxel = new JSONClass();

							voxel["pos"]["x"] = i.ToString();
							voxel["pos"]["y"] = j.ToString();
							voxel["pos"]["z"] = k.ToString();

							Color vColor = chunk.m_Voxels[i, j, k].VoxelColor;
                            voxel["color"]["r"] = vColor.r.ToString();
							voxel["color"]["g"] = vColor.g.ToString();
							voxel["color"]["b"] = vColor.b.ToString();
							voxel["color"]["a"] = vColor.a.ToString();

							voxels.Add(voxel);
						}
					}
				}

				jsonChunk["voxels"] = voxels;

				chunks.Add(jsonChunk);
			}

			jsonGroup["chunks"] = chunks;

			groups.Add(jsonGroup);
		}
		N["groups"] = groups;

		return N;
	}

	#endregion

	#region GUI

	void OnGUI() {
		if (ShowToolbars) {

			GUIMenuBar();

			GUIToolBar();
			
			GUI.Label(new Rect(Util.ScreenSize, new Vector2(0f, 0f)), "v" + Util.AssemblyVersion, VersionTextStyle);
			
			if (CursorObject.activeInHierarchy) {
				Vector3 rectPos = MainCamera.WorldToScreenPoint(VoxelCursorPosition);
				GUI.Box(new Rect(rectPos.x, Util.ScreenSize.y - rectPos.y, 100f, 25f), VoxelCursorPosition.ToString(), GeneralStyle);
			}
		}
	}

	void GUIMenuBar() {
		SelectedMenuItem = GUI.Toolbar(MenuRect, SelectedMenuItem, m_MenuItems);

		if (SelectedMenuItem >= 0) {
			switch (m_MenuItems[SelectedMenuItem]) {
				case MENU_NEW:
					NewModelPressed();
					break;
				case MENU_SAVE:
					SaveModelPressed();
					break;
				case MENU_SAVE_AS:
					SaveModelAsPressed();
					break;
				case MENU_OPEN:
					OpenModelPressed();
					break;
				case MENU_EXPORT:
					ExportModelPressed();
					break;
			}
		}

		SelectedMenuItem = -1;
	}

	void GUIToolBar() {
		SelectedToolItem = GUI.Toolbar(ToolRect, SelectedToolItem, m_ToolItems);

		switch (m_ToolItems[SelectedToolItem]) {
			case TOOL_BRUSH:
				BrushToolPressed();
				break;
			case TOOL_SELECTOR:
				SelectorToolPressed();
				break;
			case TOOL_ANIMATE:
				AnimateToolPressed();
				break;
			case TOOL_SEGMENT:
				SegmentToolPressed();
				break;
		}
	}

#endregion

	#region Construction

	private static VoxelEditor m_Instance = null;
	public static VoxelEditor Instance {
		get {
			return m_Instance;
		}
	}

	public void Awake() {
		if (m_Instance == null) {
			m_Instance = this;
		}
	}

	#endregion

	#region Static Editor Functions

	public static WorldPos GetVoxelPos(Vector3 pos) {
		return new WorldPos(
			Mathf.RoundToInt(pos.x),
			Mathf.RoundToInt(pos.y),
			Mathf.RoundToInt(pos.z));
	}

	public static WorldPos GetVoxelPos(RaycastHit hit, bool adjacent = false) {
		Vector3 pos = new Vector3(
				MoveWithinVoxel(hit.point.x, hit.normal.x, adjacent),
				MoveWithinVoxel(hit.point.y, hit.normal.y, adjacent),
				MoveWithinVoxel(hit.point.z, hit.normal.z, adjacent)
				);

		return GetVoxelPos(pos);
	}

	public static bool SetVoxel(RaycastHit hit, Voxel voxel, bool adjacent = false) {
		Debug.Log(Instance.VoxelCount);
		Chunk chunk = hit.collider.GetComponent<Chunk>();
		if (chunk == null || (voxel.GetType() == typeof(VoxelAir) && Instance.VoxelCount == 1)) {
			return false;
		}

		WorldPos pos = GetVoxelPos(hit, adjacent);

		Voxel v = chunk.Group.GetVoxel(pos.X, pos.Y, pos.Z);

		if ((v.GetType() != typeof(VoxelAir) && voxel.GetType() != typeof(VoxelAir)) ||
			(voxel.GetType() == typeof(VoxelAir) && v.GetType() == typeof(VoxelAir))) {
			return false;
		}

		chunk.Group.SetVoxel(pos.X, pos.Y, pos.Z, voxel);

		return true;
	}

	public static Voxel GetVoxel(RaycastHit hit, bool adjacent = false) {
		Chunk chunk = hit.collider.GetComponent<Chunk>();
		if (chunk == null)
			return null;

		WorldPos pos = GetVoxelPos(hit, adjacent);

		Voxel voxel = chunk.Group.GetVoxel(pos.X, pos.Y, pos.Z);

		return voxel;
	}

	static float MoveWithinVoxel(float pos, float norm, bool adjacent = false) {
		if (pos - (int)pos == 0.5f || pos - (int)pos == -0.5f) {
			if (adjacent) {
				pos += (norm / 2);
			}
			else {
				pos -= (norm / 2);
			}
		}

		return pos;
	}

	#endregion
}
