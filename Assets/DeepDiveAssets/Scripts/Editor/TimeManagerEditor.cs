using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TimeManager))]
public class TimeManagerEditor : Editor
{
    private bool showDayPartButtons = true;

    private void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
    }

    private void EditorUpdate()
    {
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime Info", EditorStyles.boldLabel);

        var tm = (TimeManager)target;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Enter Play Mode to see live time updates and debug controls.",
                MessageType.Info
            );
            return;
        }

        EditorGUILayout.LabelField("Current Time", tm.CurrentTimeString);

        int totalMinutes = tm.TotalMinutesInDay;
        int minuteOfDay = tm.CurrentMinuteOfDay;

        float t = totalMinutes > 0 ? (float)minuteOfDay / totalMinutes : 0f;
        EditorGUILayout.Slider("Progress Through Day", t, 0f, 1f);
        EditorGUILayout.LabelField(
            $"Minute of Day: {minuteOfDay}/{totalMinutes}",
            EditorStyles.miniLabel
        );

        EditorGUILayout.Space();
        DrawDayPartJumpSection(tm);
    }

    private void DrawDayPartJumpSection(TimeManager tm)
    {
        var settings = tm.Settings;
        if (settings == null)
        {
            EditorGUILayout.HelpBox(
                "No TimeSettings assigned on TimeManager.",
                MessageType.Warning
            );
            return;
        }

        var dayParts = settings.GetDayParts();
        if (dayParts == null || dayParts.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No DayParts configured in TimeSettings.",
                MessageType.Info
            );
            return;
        }

        showDayPartButtons = EditorGUILayout.Foldout(showDayPartButtons, "Debug: Jump to DayPart");

        if (!showDayPartButtons)
            return;

        EditorGUILayout.HelpBox(
            "Click a button to instantly jump the in-game time to the start of that DayPart.\n" +
            "This will recompute time and visuals (skybox/light).",
            MessageType.None
        );

        foreach (var dp in dayParts)
        {
            if (dp == null) continue;

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField($"{dp.DayPartName} (start: {dp.DayPartStart}h)");

            if (GUILayout.Button("Jump", GUILayout.Width(80)))
            {
                tm.DebugJumpToDayPart(dp);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
