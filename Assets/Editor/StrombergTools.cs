using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace StrombergTools {
    public enum Command {
        None,
        ToggleHide
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

        static void OnSceneGUI(SceneView sceneView) {
            if (Event.current.type == EventType.KeyDown) {
                if (Event.current.modifiers == EventModifiers.Shift) {
                    if (Event.current.keyCode == KeyCode.H) {
                        if (Selection.activeGameObject) {
                            Event.current.Use();
                            if (Selection.activeGameObject.activeSelf) {
                                Selection.activeGameObject.SetActive(false);
                            }
                            else {
                                Selection.activeGameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
            if(Utils.InsideSceneView(sceneView, Event.current.mousePosition)) {
                var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                var obj = HandleUtility.RaySnap(ray);
                if (obj != null) {
                    var hit = (RaycastHit)obj;
                    if(hit.collider.gameObject != Selection.activeGameObject) {
                        Handles.color = Color.yellow;
                        Handles.DrawWireCube(hit.collider.transform.position, hit.collider.bounds.size);
                        sceneView.Repaint();
                    }
                }
            }
        }
    }
}
