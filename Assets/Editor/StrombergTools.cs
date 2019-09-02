using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace StrombergTools {
    public enum Command {
        None,
        ToggleHide,
        GroupSelected
    }

    [System.Serializable]
    public class Shortcut {
        public EventModifiers modifiers = EventModifiers.None;
        public KeyCode key = KeyCode.None;
        public Command command = Command.None;
    }

    class StrombergToolsSettings {
        public bool UseHotkeys;
        public List<Shortcut> shortcuts = new List<Shortcut> {
            new Shortcut(),
            new Shortcut()
        };
    }

    class StrombergToolsWindow : EditorWindow {

    }

    /*
    [CustomEditor(typeof(StrombergTools))]
    public class StrombergToolsEditor : Editor {
        StrombergTools editor;
        public override void OnInspectorGUI() {
            editor = (StrombergTools)target;
            base.OnInspectorGUI();
            foreach (var shortcut in editor.shortcuts) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Modifiers:", GUILayout.MaxWidth(60));
                shortcut.modifiers = (EventModifiers)EditorGUILayout.EnumFlagsField(shortcut.modifiers, GUILayout.MaxWidth(80));
                EditorGUILayout.LabelField("Key:", GUILayout.MaxWidth(30));
                shortcut.key = (KeyCode)EditorGUILayout.EnumPopup(shortcut.key, GUILayout.MaxWidth(100));
                EditorGUILayout.LabelField("Command:", GUILayout.MaxWidth(60));
                shortcut.command = (Command)EditorGUILayout.EnumPopup(shortcut.command);
                EditorGUILayout.EndHorizontal();
            }
        }
    }*/

    static class Utils {
        public static bool InsideSceneView(SceneView sceneView, Vector2 position) {
            if (Event.current.mousePosition.x < 0 || Event.current.mousePosition.y < 0 || Event.current.mousePosition.y > sceneView.position.height - 25 || Event.current.mousePosition.x > sceneView.position.width) {
                return false;
            }
            else {
                return true;
            }
        }
    }

    [InitializeOnLoad]
    public class Commands : Editor {
        static StrombergToolsSettings Settings;
        private static GameObject GameObjectUnderMouse;

        static Commands() {
            var settings = new StrombergToolsSettings();
            settings.shortcuts[1].command = Command.ToggleHide;
            var json = JsonUtility.ToJson(settings);
            Debug.Log(json);
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnDestroy() {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        static Color polygon_color = new Color(0.7f, 0.2f, 0f, 0.3f);
        static Color edge_color = new Color(1f, 0f, 0f, 0.8f);

        struct Triangle {
            public Vector3 v1;
            public Vector3 v2;
            public Vector3 v3;
        }

        static void OnSceneGUI(SceneView sceneView) {
            if (Event.current.type == EventType.KeyDown) {
                if (Event.current.modifiers == EventModifiers.Shift) {
                    if (Event.current.keyCode == KeyCode.H) {
                        Undo.RecordObjects(Selection.gameObjects, "Toggle Active");
                        foreach (var activeGameObject in Selection.gameObjects) {
                            Event.current.Use();
                            if (activeGameObject.activeSelf) {
                                activeGameObject.SetActive(false);
                            }
                            else {
                                activeGameObject.SetActive(true);
                            }
                        }
                    }
                }
                else if (Event.current.modifiers == EventModifiers.Control) {
                    if (Event.current.keyCode == KeyCode.G) {
                        if (Selection.gameObjects.Length > 0) {
                            var game_object = new GameObject();
                            Undo.RegisterCreatedObjectUndo(game_object, "Group Selected");
                            foreach (var active_game_object in Selection.gameObjects) {
                                Undo.SetTransformParent(active_game_object.transform, game_object.transform, "Group Selected");
                            }
                        }
                    }
                }
            }
            if (Event.current.type == EventType.MouseMove) {
                var obj = HandleUtility.PickGameObject(Event.current.mousePosition, true);
                if (obj != null) {
                    if (obj.gameObject != Selection.activeGameObject && obj.gameObject.GetComponentInChildren<MeshRenderer>()) {
                        GameObjectUnderMouse = obj;
                    }
                    else {
                        GameObjectUnderMouse = null;
                    }
                }
                else {
                    GameObjectUnderMouse = null;
                }
            }
            if (Utils.InsideSceneView(sceneView, Event.current.mousePosition)) {
                if (GameObjectUnderMouse != null) {
                    foreach (var mesh_filter in GameObjectUnderMouse.GetComponentsInChildren<MeshFilter>()) {
                        var matrix = mesh_filter.gameObject.GetComponent<MeshRenderer>().localToWorldMatrix;
                        for (var submesh_index = 0; submesh_index < mesh_filter.sharedMesh.subMeshCount; submesh_index++) {
                            var triangles = mesh_filter.sharedMesh.GetTriangles(submesh_index);
                            var current_triangle = new Triangle();
                            var triangle_vertex = 0;
                            var vertices = mesh_filter.sharedMesh.vertices;
                            foreach (var triangle in triangles) {
                                if (triangle_vertex == 0) {
                                    current_triangle.v1 = vertices[triangle];
                                }
                                else if (triangle_vertex == 1) {
                                    current_triangle.v2 = vertices[triangle];
                                }
                                else {
                                    current_triangle.v3 = vertices[triangle];
                                    var vertex_1 = matrix.MultiplyPoint(current_triangle.v1);
                                    var vertex_2 = matrix.MultiplyPoint(current_triangle.v2);
                                    var vertex_3 = matrix.MultiplyPoint(current_triangle.v3);
                                    Handles.color = polygon_color;
                                    Handles.DrawAAConvexPolygon(vertex_1, vertex_2, vertex_3);
                                    Handles.color = edge_color;
                                    Handles.DrawAAPolyLine(vertex_1, vertex_2, vertex_3);
                                }
                                triangle_vertex = (triangle_vertex + 1) % 3;
                            }
                        }
                    }
                    sceneView.Repaint();
                }
            }
        }
    }
}
