using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TimeEventManager))]
public class TimeEventManagerEditor : Editor
{
    private bool showRuntimeDebug = true;

    public override void OnInspectorGUI()
    {
        // Draw the normal inspector first
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime Debug", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Enter Play Mode to see runtime event state.",
                MessageType.Info
            );
            return;
        }

        showRuntimeDebug = EditorGUILayout.Foldout(showRuntimeDebug, "Show Events Debug");

        if (!showRuntimeDebug)
            return;

        EditorGUILayout.Space();

        var mgr = (TimeEventManager)target;
        var states = mgr.RuntimeStates;
        var timeManager = mgr.TimeManager;

        if (states == null || states.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No runtime states found. Make sure you have TimeEventDefinitions assigned.",
                MessageType.Warning
            );
            return;
        }

        int minutesPerHour = timeManager.MinutesPerHour;
        int currentDay = timeManager.Days;
        int currentMinuteOfDay = timeManager.CurrentMinuteOfDay;

        EditorGUILayout.LabelField(
            $"Current Game Day: {currentDay + 1} (Days={currentDay})  |  Time: {FormatTime(currentMinuteOfDay, minutesPerHour)}",
            EditorStyles.miniLabel
        );

        EditorGUILayout.Space();

        foreach (var state in states)
        {
            if (state.timeEvent == null)
                continue;

            DrawStateCard(state, timeManager, minutesPerHour);
        }
    }

    private void DrawStateCard(TimeEventRuntimeState state, TimeManager timeManager, int minutesPerHour)
    {
        var def = state.timeEvent;

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField(def.EventId, EditorStyles.boldLabel);

        // Active today?
        string activeText = state.IsActiveToday ? "Yes" : "No";
        Color prev = GUI.color;
        GUI.color = state.IsActiveToday ? Color.green : Color.gray;
        EditorGUILayout.LabelField($"Active Today: {activeText}");
        GUI.color = prev;

        // Frequency / start day info
        EditorGUILayout.LabelField(
            "Frequency:",
            $"{def.Frequency} (Start Day ≥ {(def.StartDayNumber > 0 ? def.StartDayNumber.ToString() : "1")})",
            EditorStyles.miniLabel
        );

        // Next trigger time
        if (state.IsActiveToday && state.NextTriggerMinuteOfDay >= 0)
        {
            int minuteOfDay = Mathf.Clamp(state.NextTriggerMinuteOfDay, 0, timeManager.TotalMinutesInDay - 1);
            string timeStr = FormatTime(minuteOfDay, minutesPerHour);
            EditorGUILayout.LabelField($"Next Trigger (today): {timeStr}", EditorStyles.miniLabel);
        }
        else
        {
            EditorGUILayout.LabelField("Next Trigger (today): —", EditorStyles.miniLabel);
        }

        // Occurrences info
        if (def.OccurrenceMode == TimeEventOccurrenceMode.Multiple)
        {
            int remaining = Mathf.Max(0, state.TargetOccurrences - state.OccurrencesTriggered);
            EditorGUILayout.LabelField(
                $"Occurrences Today: {state.OccurrencesTriggered}/{state.TargetOccurrences}  (Remaining: {remaining})",
                EditorStyles.miniLabel
            );
        }
        else
        {
            string done = state.OccurrencesTriggered > 0 ? "Triggered" : "Not triggered yet";
            EditorGUILayout.LabelField($"Single Occurrence Status: {done}", EditorStyles.miniLabel);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private string FormatTime(int minuteOfDay, int minutesPerHour)
    {
        int h = minuteOfDay / minutesPerHour;
        int m = minuteOfDay % minutesPerHour;
        return $"{h:00}:{m:00}";
    }
}
