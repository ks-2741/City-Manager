using UnityEngine;
using UnityEditor;

// Editor-only tool - must live in a folder named "Editor" anywhere under Assets.
// Open it via the menu: Tools > Road Placer

public class RoadPlacerTool : EditorWindow
{
    private GameObject _roadPrefab;
    private float _spacing = 2f;
    private LayerMask _groundMask;
    private bool _placingActive;

    private Vector3? _firstPoint;

    [MenuItem("Tools/Road Placer")]
    public static void ShowWindow()
    {
        GetWindow<RoadPlacerTool>("Road Placer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Road Placer", EditorStyles.boldLabel);

        _roadPrefab = (GameObject)EditorGUILayout.ObjectField("Road Prefab", _roadPrefab, typeof(GameObject), false);
        _spacing = EditorGUILayout.FloatField("Spacing (grid size)", _spacing);
        _groundMask = LayerMaskField("Ground Mask", _groundMask);

        EditorGUILayout.Space();

        if (_roadPrefab == null)
        {
            EditorGUILayout.HelpBox("Assign a road prefab to begin.", MessageType.Info);
            return;
        }

        string buttonLabel = _placingActive ? "Stop Placing (Esc)" : "Start Placing";
        if (GUILayout.Button(buttonLabel))
        {
            _placingActive = !_placingActive;
            _firstPoint = null;
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Click a point in the Scene view to start a line.\n" +
            "Click a second point to lay road segments between them.\n" +
            "Click again to start a new line from the last point.\n" +
            "Esc or the button above to stop.",
            MessageType.None);
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!_placingActive) return;

        Event e = Event.current;

        // Block Unity's default click-to-select while placing
        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(controlId);

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            _placingActive = false;
            _firstPoint = null;
            e.Use();
            Repaint();
            return;
        }

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 500f, _groundMask))
            {
                if (_firstPoint == null)
                {
                    _firstPoint = hit.point;
                }
                else
                {
                    PlaceLine(_firstPoint.Value, hit.point);
                    _firstPoint = hit.point; // chain the next line from here
                }
            }

            e.Use();
        }

        // Draw a preview line from the first point to the current mouse position
        if (_firstPoint != null)
        {
            Ray previewRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(previewRay, out RaycastHit previewHit, 500f, _groundMask))
            {
                Handles.color = Color.yellow;
                Handles.DrawLine(_firstPoint.Value, previewHit.point);
                Handles.SphereHandleCap(0, _firstPoint.Value, Quaternion.identity, 0.5f, EventType.Repaint);
            }
        }

        sceneView.Repaint();
    }

    private void PlaceLine(Vector3 start, Vector3 end)
    {
        // Snap both ends to the grid first
        start = SnapToGrid(start);
        end = SnapToGrid(end);

        float distance = Vector3.Distance(start, end);
        int segmentCount = Mathf.Max(1, Mathf.RoundToInt(distance / _spacing));
        Vector3 direction = (end - start).normalized;

        // Face the prefab along the line direction
        Quaternion rotation = direction.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(direction, Vector3.up)
            : Quaternion.identity;

        for (int i = 0; i <= segmentCount; i++)
        {
            Vector3 pos = start + direction * (i * _spacing);
            pos = SnapToGrid(pos);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(_roadPrefab);
            instance.transform.position = pos;
            instance.transform.rotation = rotation;

            Undo.RegisterCreatedObjectUndo(instance, "Place Road Segment");
        }
    }

    private Vector3 SnapToGrid(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x / _spacing) * _spacing,
            pos.y,
            Mathf.Round(pos.z / _spacing) * _spacing
        );
    }

    // Simple LayerMask field helper (Unity doesn't expose this directly in EditorGUILayout)
    private LayerMask LayerMaskField(string label, LayerMask layerMask)
    {
        var layers = UnityEditorInternal.InternalEditorUtility.layers;
        int mask = 0;
        for (int i = 0; i < layers.Length; i++)
        {
            int layerIndex = LayerMask.NameToLayer(layers[i]);
            if ((layerMask & (1 << layerIndex)) != 0)
                mask |= 1 << i;
        }

        mask = EditorGUILayout.MaskField(label, mask, layers);

        int result = 0;
        for (int i = 0; i < layers.Length; i++)
        {
            if ((mask & (1 << i)) != 0)
                result |= 1 << LayerMask.NameToLayer(layers[i]);
        }

        return result;
    }
}