using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TimeSettings))]
public class TimeSettingsEditor : Editor
{
    SerializedProperty dayPartsProp;
    SerializedProperty startingSkyboxProp;
    SerializedProperty startingGradientProp;
    SerializedProperty minutesPerHourProp;
    SerializedProperty hoursPerDayProp;
    SerializedProperty timeMultiplierProp;

    void OnEnable()
    {
        dayPartsProp = serializedObject.FindProperty("dayParts");
        startingSkyboxProp = serializedObject.FindProperty("startingSkybox");
        startingGradientProp = serializedObject.FindProperty("startingGradient");
        minutesPerHourProp = serializedObject.FindProperty("MinutesPerHour");
        hoursPerDayProp = serializedObject.FindProperty("HoursPerDay");
        timeMultiplierProp = serializedObject.FindProperty("timeMultiplier");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Time Scale", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(minutesPerHourProp);
        EditorGUILayout.PropertyField(hoursPerDayProp);
        EditorGUILayout.PropertyField(timeMultiplierProp);

        DrawTimeInfoBox();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(startingSkyboxProp);
        EditorGUILayout.PropertyField(startingGradientProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Day Parts", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(dayPartsProp, new GUIContent("Day Parts"), true);

        DrawDayPartValidationAndTimeline();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTimeInfoBox()
    {
        int minutesPerHour = minutesPerHourProp.intValue;
        int hoursPerDay = hoursPerDayProp.intValue;
        float multiplier = timeMultiplierProp.floatValue;

        int totalMinutes = minutesPerHour * hoursPerDay;
        float realSecondsPerDay = multiplier > 0f ? totalMinutes / multiplier : 0f;

        int realMinutes = (int)(realSecondsPerDay / 60f);
        int realSeconds = (int)(realSecondsPerDay % 60f);

        string text = $"Total in-game minutes per day: {totalMinutes}\n";

        if (multiplier <= 0f)
        {
            text += "Time multiplier is 0 → time will not advance.";
        }
        else
        {
            text += $"1 in-game day ≈ {realSecondsPerDay:0.0} real seconds (~{realMinutes}m {realSeconds}s).\n" +
                    $"1 real second = {multiplier:0.##} in-game minute(s).";
        }

        EditorGUILayout.HelpBox(text, MessageType.Info);
    }

    private void DrawDayPartValidationAndTimeline()
    {
        int hoursPerDay = hoursPerDayProp.intValue;
        var usedStartHours = new HashSet<int>();
        bool hasError = false;

        var timeSettings = (TimeSettings)target;
        var list = timeSettings.GetDayParts();

        if (list == null || list.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No DayParts defined. You should create at least one DayPartInfo asset and assign it here.",
                MessageType.Warning
            );
            return;
        }

        // Validation checks
        foreach (var dp in list)
        {
            if (dp == null) continue;

            if (dp.DayPartStart < 0 || dp.DayPartStart >= hoursPerDay)
            {
                EditorGUILayout.HelpBox(
                    $"DayPart '{dp.DayPartName}' has a start hour ({dp.DayPartStart}) outside the range [0, {hoursPerDay - 1}].",
                    MessageType.Error
                );
                hasError = true;
            }

            if (!usedStartHours.Add(dp.DayPartStart))
            {
                EditorGUILayout.HelpBox(
                    $"Multiple DayParts share the same start hour ({dp.DayPartStart}). " +
                    $"This can cause ambiguous transitions.",
                    MessageType.Warning
                );
            }
        }

        // Timeline preview
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Day Part Timeline", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Rough visual representation of your day parts over a single in-game day.\n" +
            "Segments are based on DayPartStart and HoursPerDay.",
            MessageType.None
        );

        Rect r = GUILayoutUtility.GetRect(10, 30);
        DrawDayPartTimeline(r, list, hoursPerDay);

        if (!hasError && list.Count > 0)
        {
            EditorGUILayout.HelpBox(
                "Tip: Make sure the DayPartStart values cover the whole day in a logical order.\n" +
                "You can manually sort the list by start hour to keep things readable.",
                MessageType.Info
            );
        }
    }

    private void DrawDayPartTimeline(Rect rect, List<DayPartInfo> parts, int hoursPerDay)
    {
        if (Event.current.type != EventType.Repaint || parts == null || parts.Count == 0)
            return;

        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));

        // sort by start hour for drawing
        var sorted = new List<DayPartInfo>(parts);
        sorted.Sort((a, b) => a.DayPartStart.CompareTo(b.DayPartStart));

        for (int i = 0; i < sorted.Count; i++)
        {
            var dp = sorted[i];
            if (dp == null) continue;

            int startHour = Mathf.Clamp(dp.DayPartStart, 0, hoursPerDay);
            int endHour;
            if (i < sorted.Count - 1)
                endHour = Mathf.Clamp(sorted[i + 1].DayPartStart, 0, hoursPerDay);
            else
                endHour = hoursPerDay;

            float startT = (float)startHour / hoursPerDay;
            float endT = (float)endHour / hoursPerDay;

            float x = rect.x + rect.width * startT;
            float width = rect.width * (endT - startT);

            if (width <= 1f) width = 1f;

            Color c = Color.HSVToRGB((float)i / sorted.Count, 0.6f, 0.9f);
            Rect seg = new Rect(x, rect.y, width, rect.height);
            EditorGUI.DrawRect(seg, c);

            var labelRect = new Rect(seg.x + 2, seg.y + 7, seg.width - 4, seg.height - 14);
            GUIStyle style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.black }
            };
            GUI.Label(labelRect, $"{dp.DayPartName} ({dp.DayPartStart}h)", style);
        }

        // border
        Handles.color = Color.black;
        Handles.DrawAAPolyLine(2f,
            new Vector3(rect.x, rect.y),
            new Vector3(rect.x + rect.width, rect.y),
            new Vector3(rect.x + rect.width, rect.y + rect.height),
            new Vector3(rect.x, rect.y + rect.height),
            new Vector3(rect.x, rect.y)
        );
    }
}
