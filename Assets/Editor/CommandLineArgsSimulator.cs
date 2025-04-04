#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class CommandLineArgsSimulator : EditorWindow {
    private const string PrefsKey_SimulatedArgs = "SimulatedCommandLineArguments"; // Key for EditorPrefs
    public static string SimulatedArguments = "";

    [MenuItem("Tools/Simulate Command Line Args")]
    public static void ShowWindow() {
        CommandLineArgsSimulator window = GetWindow<CommandLineArgsSimulator>("Cmd Args Sim");
        window.LoadArgumentsFromPrefs(); // Load arguments when window opens
    }

    private void OnEnable() {
        LoadArgumentsFromPrefs(); // Load arguments when script is enabled (window opens) - redundant with ShowWindow but good practice
    }

    private void LoadArgumentsFromPrefs() {
        SimulatedArguments = EditorPrefs.GetString(PrefsKey_SimulatedArgs, ""); // Load from prefs, default to empty string
    }

    private void SaveArgumentsToPrefs() {
        EditorPrefs.SetString(PrefsKey_SimulatedArgs, SimulatedArguments); // Save to prefs
    }

    void OnGUI() {
        GUILayout.Label("Simulated Command Line Arguments:", EditorStyles.boldLabel);
        SimulatedArguments = EditorGUILayout.TextArea(SimulatedArguments);

        if (GUILayout.Button("Apply")) {
            SaveArgumentsToPrefs(); // Save to EditorPrefs when Apply is pressed
            Debug.Log("Simulated Arguments Applied and Saved: " + SimulatedArguments);
        }
    }
}
#endif