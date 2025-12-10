using UnityEditor;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[CustomEditor(typeof(TimeEvent))]
public class TimeEventEditor : Editor
{
    SerializedProperty eventIdProp;
    SerializedProperty allowedDayPartsProp;

    SerializedProperty windowStartHourProp;
    SerializedProperty windowEndHourProp;

    SerializedProperty triggerModeProp;
    SerializedProperty periodDurationHoursProp;
    SerializedProperty randomAfterPeriodProp;
    SerializedProperty randomExtraMinHoursProp;
    SerializedProperty randomExtraMaxHoursProp;

    SerializedProperty occurrenceModeProp;
    SerializedProperty minOccurrencesProp;
    SerializedProperty maxOccurrencesProp;
    SerializedProperty minIntervalBetweenOccurrencesProp;
    SerializedProperty maxIntervalBetweenOccurrencesProp;

    SerializedProperty frequencyProp;
    SerializedProperty startDayNumberProp;
    SerializedProperty everyXDaysProp;
    SerializedProperty dailyChanceProp;

    SerializedProperty eventChannelProp;

    private void OnEnable()
    {
        eventIdProp = serializedObject.FindProperty("EventId");
        allowedDayPartsProp = serializedObject.FindProperty("AllowedDayParts");

        windowStartHourProp = serializedObject.FindProperty("WindowStartHour");
        windowEndHourProp = serializedObject.FindProperty("WindowEndHour");

        triggerModeProp = serializedObject.FindProperty("TriggerMode");
        periodDurationHoursProp = serializedObject.FindProperty("PeriodDurationHours");
        randomAfterPeriodProp = serializedObject.FindProperty("RandomAfterPeriod");
        randomExtraMinHoursProp = serializedObject.FindProperty("RandomExtraMinHours");
        randomExtraMaxHoursProp = serializedObject.FindProperty("RandomExtraMaxHours");

        occurrenceModeProp = serializedObject.FindProperty("OccurrenceMode");
        minOccurrencesProp = serializedObject.FindProperty("MinOccurrences");
        maxOccurrencesProp = serializedObject.FindProperty("MaxOccurrences");
        minIntervalBetweenOccurrencesProp = serializedObject.FindProperty("MinIntervalBetweenOccurrencesMinutes");
        maxIntervalBetweenOccurrencesProp = serializedObject.FindProperty("MaxIntervalBetweenOccurrencesMinutes");

        frequencyProp = serializedObject.FindProperty("Frequency");
        startDayNumberProp = serializedObject.FindProperty("StartDayNumber");
        everyXDaysProp = serializedObject.FindProperty("EveryXDays");
        dailyChanceProp = serializedObject.FindProperty("DailyChance");

        eventChannelProp = serializedObject.FindProperty("EventChannel");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Basic", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(eventIdProp);
        EditorGUILayout.PropertyField(allowedDayPartsProp, true);

        EditorGUILayout.Space();
        DrawTimeWindowSection();

        EditorGUILayout.Space();
        DrawTriggerModeSection();

        EditorGUILayout.Space();
        DrawOccurrenceSection();

        EditorGUILayout.Space();
        DrawFrequencySection();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Event Channel", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(eventChannelProp);

        EditorGUILayout.Space();
        DrawPreview();

        EditorGUILayout.Space();
        DrawDebugDection();

        serializedObject.ApplyModifiedProperties();
    }

    // #################################
    // ---------- TIME WINDOW ----------
    // #################################
    private void DrawTimeWindowSection()
    {
        EditorGUILayout.LabelField("Time Window", EditorStyles.boldLabel);

        var triggerMode = (TimeEventTriggerMode)triggerModeProp.enumValueIndex;
        
        EditorGUILayout.PropertyField(windowStartHourProp, new GUIContent("Start Hour"));
        if (triggerMode != TimeEventTriggerMode.AtFixedTime)
        {
            EditorGUILayout.PropertyField(windowEndHourProp, new GUIContent("End Hour"));
        }
        else
        {
            EditorGUILayout.HelpBox("At Fixed Time uses only the Start Hour as the exact trigger moment.", MessageType.None);
        }

        // --- Validation ---
        // EndHour should be > StartHour when used
        if (triggerMode != TimeEventTriggerMode.AtFixedTime)
        {
            if (windowEndHourProp.floatValue <= windowStartHourProp.floatValue)
            {
                EditorGUILayout.HelpBox(
                    "End Hour should be greater than Start Hour for a valid time window.",
                    MessageType.Warning
                );
            }
        }
    }

    // ##################################
    // ---------- TRIGGER MODE ----------
    // ##################################
    private void DrawTriggerModeSection()
    {
        EditorGUILayout.LabelField("Trigger Mode", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(triggerModeProp);

        var triggerMode = (TimeEventTriggerMode)triggerModeProp.enumValueIndex;

        EditorGUI.indentLevel++;
        switch (triggerMode)
        {
            case TimeEventTriggerMode.AtFixedTime:
                //The UI change is done in OnInspectorGUI
                EditorGUILayout.HelpBox("Event will trigger at WindowStartHour.", MessageType.Info);
                break;

            case TimeEventTriggerMode.RandomInWindow:
                EditorGUILayout.HelpBox("Event will trigger at a random time between start hour and end hour.", MessageType.Info);
                break;

            case TimeEventTriggerMode.AfterPeriod:
                EditorGUILayout.HelpBox(
                    "The event will trigger AFTER a delay starting from the Start Hour.\n" +
                    "Example: Start Hour = 20, Period = 2 → event triggers at 22.\n" +
                    "You can optionally add a random extra delay on top of this.",
                MessageType.Info
                );

                EditorGUILayout.PropertyField(
                    periodDurationHoursProp,
                    new GUIContent("Period Duration (hours)", "How many in-game hours AFTER the Start Hour this event should trigger.")
                );

                EditorGUILayout.PropertyField(
                    randomAfterPeriodProp,
                    new GUIContent("Random Extra Delay?", "If enabled, adds a random delay on top of the period duration.")
                );

                if (randomAfterPeriodProp.boolValue)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.HelpBox(
                        "A random value between Min and Max will be added ON TOP of the period duration.\n" +
                        "Example:\n" +
                        "Period = 2h\n" +
                        "Random Min = 0.5h\n" +
                        "Random Max = 1.5h\n" +
                        "→ Final trigger time = 2.5h to 3.5h after Start Hour.",
                        MessageType.None
                    );

                    EditorGUILayout.PropertyField(
                        randomExtraMinHoursProp,
                        new GUIContent("Extra Min (hours)", "Minimum random extra delay added after the period.")
                    );
                    EditorGUILayout.PropertyField(
                        randomExtraMaxHoursProp,
                        new GUIContent("Extra Max (hours)", "Maximum random extra delay added after the period.")
                    );

                    // --- Validation ---
                    // Random extra min/max check
                    if (triggerMode == TimeEventTriggerMode.AfterPeriod && randomAfterPeriodProp.boolValue)
                    {
                        if (randomExtraMinHoursProp.floatValue > randomExtraMaxHoursProp.floatValue)
                        {
                            EditorGUILayout.HelpBox(
                                "Random Extra Min is greater than Max. The event will behave unpredictably.\n" +
                                "Make sure Min ≤ Max.",
                                MessageType.Warning
                            );
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                break;
        }
        EditorGUI.indentLevel--;
    }

    // ################################
    // ---------- OCCURRENCE ----------
    // ################################
    private void DrawOccurrenceSection()
    {
        EditorGUILayout.LabelField("Occurrences", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(occurrenceModeProp);

        EditorGUI.indentLevel++;
        if ((TimeEventOccurrenceMode)occurrenceModeProp.enumValueIndex == TimeEventOccurrenceMode.Multiple)
        {
            // --- SECTION 1: Count ---

            EditorGUILayout.LabelField("Occurrences Count", EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "Defines how many times this event will fire within its active window.\n" +
                "Example: Min = 5, Max = 10 → each day this event is active, it will choose a random count in that range.",
                MessageType.None
            );

            EditorGUILayout.PropertyField(minOccurrencesProp, new GUIContent("Min Occurrences", "Minimum times this event fires in one active cycle."));
            EditorGUILayout.PropertyField(maxOccurrencesProp,new GUIContent("Max Occurrences", "Maximum times this event fires in one active cycle."));

            int minOcc = minOccurrencesProp.intValue;
            int maxOcc = maxOccurrencesProp.intValue;

            if (minOcc < 1)
            {
                EditorGUILayout.HelpBox("Min Occurrences should be at least 1.",MessageType.Warning);
            }
            if (maxOcc < 1)
            {
                EditorGUILayout.HelpBox("Max Occurrences should be at least 1.",MessageType.Warning);
            }
            if (maxOcc < minOcc)
            {
                EditorGUILayout.HelpBox("Max Occurrences is smaller than Min Occurrences. The random range will not behave correctly.",MessageType.Warning);
            }

            EditorGUILayout.Space();

            // --- SECTION 2: Interval ---

            EditorGUILayout.LabelField("Interval Between Occurrences", EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "Defines the time spacing between individual triggers inside this event.\n" +
                "Example: Min = 0.2, Max = 0.5 → each next occurrence will happen 0.2–0.5 minutes after the previous one.",
                MessageType.None
            );

            EditorGUILayout.PropertyField(minIntervalBetweenOccurrencesProp,new GUIContent("Min Interval (min)", "Minimum in-game minutes between two occurrences."));
            EditorGUILayout.PropertyField(maxIntervalBetweenOccurrencesProp,new GUIContent("Max Interval (min)", "Maximum in-game minutes between two occurrences."));

            float minInt = minIntervalBetweenOccurrencesProp.floatValue;
            float maxInt = maxIntervalBetweenOccurrencesProp.floatValue;

            if (minInt < 0f || maxInt < 0f)
            {
                EditorGUILayout.HelpBox("Intervals should not be negative.",MessageType.Warning);
            }
            if (maxInt < minInt)
            {
                EditorGUILayout.HelpBox("Max Interval is smaller than Min Interval. The random interval range will not behave correctly.",MessageType.Warning);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Single occurrence. Multiple settings are ignored.", MessageType.None);
        }
        EditorGUI.indentLevel--;
    }

    // ###############################
    // ---------- FREQUENCY ----------
    // ###############################
    private void DrawFrequencySection()
    {
        EditorGUILayout.LabelField("Frequency", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(frequencyProp);

        EditorGUILayout.PropertyField(
            startDayNumberProp,
            new GUIContent(
                "Start Day Number",
                "If > 0, the event will never be active before this in-game day.\n" +
                "Day 1 = first day of the game (TimeManager.Days == 0).\n\n" +
                "For OnceEver: this becomes the exact day the event can occur."
            )
        );

        if (startDayNumberProp.intValue < 0)
        {
            EditorGUILayout.HelpBox(
                "Start Day Number is negative. Use 0 for 'immediately' or a positive value for a specific day.",
                MessageType.Warning
            );
        }

        EditorGUI.indentLevel++;
        switch ((TimeEventFrequency)frequencyProp.enumValueIndex)
        {
            case TimeEventFrequency.EveryDay:
                EditorGUILayout.HelpBox(
                    "This event can be active every in-game day starting from the Start Day Number\n" +
                    "(as long as time window, day parts, etc. also match).",
                    MessageType.None
                );
                break;

            case TimeEventFrequency.OnceEver:
                EditorGUILayout.HelpBox(
                    "This event can only ever occur ONCE in the entire game.\n" +
                    "If Start Day Number > 0, that is the ONLY day it can occur.\n" +
                    "If Start Day Number <= 0, it will occur the first day where all conditions match.",
                    MessageType.Warning
                );

                if (startDayNumberProp.intValue == 0)
                {
                    EditorGUILayout.HelpBox(
                        "OnceEver with Start Day Number = 0: the event will trigger on the first day where all conditions match.\n" +
                        "Set Start Day Number > 0 to force a specific in-game day.",
                        MessageType.Info
                    );
                }
                break;

            case TimeEventFrequency.EveryXDays:
                EditorGUILayout.HelpBox(
                    "This event will only be active on days that are multiples of X,\n" +
                    "starting from the Start Day Number.\n" +
                    "Example: Start Day = 3, X = 2 → days 3, 5, 7, ...",
                    MessageType.None
                );

                EditorGUILayout.PropertyField(
                    everyXDaysProp,
                    new GUIContent(
                        "Every X Days",
                        "Event is only active on days that are multiples of this number (after Start Day Number)."
                    )
                );

                if (everyXDaysProp.intValue <= 0)
                {
                    EditorGUILayout.HelpBox(
                        "Every X Days should be at least 1.",
                        MessageType.Warning
                    );
                }
                break;

            case TimeEventFrequency.ChancePerDay:
                EditorGUILayout.HelpBox(
                    "Each day from Start Day Number onwards, this event has a random chance to be active.\n" +
                    "If it is not active today, it will not be scheduled at all.",
                    MessageType.None
                );

                EditorGUILayout.PropertyField(
                    dailyChanceProp,
                    new GUIContent(
                        "Daily Chance (0–1)",
                        "Probability that the event is active on a given day (0 = never, 1 = always)."
                    )
                );

                if (dailyChanceProp.floatValue <= 0f)
                {
                    EditorGUILayout.HelpBox(
                        "Daily Chance is 0 or less: this event will never be active.",
                        MessageType.Info
                    );
                }
                else if (dailyChanceProp.floatValue >= 1f)
                {
                    EditorGUILayout.HelpBox(
                        "Daily Chance is 1 or more: this is effectively the same as EveryDay (after Start Day Number).",
                        MessageType.Info
                    );
                }
                break;
        }
        EditorGUI.indentLevel--;
    }

    // #############################
    // ---------- PREVIEW ----------
    // #############################
    private void DrawPreview()
    {
        var triggerMode = (TimeEventTriggerMode)triggerModeProp.enumValueIndex;

        float startHour = windowStartHourProp.floatValue;
        float endHour = windowEndHourProp.floatValue;
        float period = periodDurationHoursProp.floatValue;
        bool useRandomExtra = randomAfterPeriodProp.boolValue;
        float extraMin = randomExtraMinHoursProp.floatValue;
        float extraMax = randomExtraMaxHoursProp.floatValue;

        int startDay = startDayNumberProp.intValue;
        var freq = (TimeEventFrequency)frequencyProp.enumValueIndex;
        int everyX = everyXDaysProp.intValue;
        float chance = dailyChanceProp.floatValue;

        // --- PREVIEW ---

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Trigger Preview (Time)", EditorStyles.boldLabel);

        // --- TIME PREVIEW ---

        switch (triggerMode)
        {
            case TimeEventTriggerMode.AtFixedTime:
                EditorGUILayout.HelpBox(
                    $"Event will trigger at: {startHour:0.00} h (Start Hour).",
                    MessageType.Info
                );
                break;

            case TimeEventTriggerMode.RandomInWindow:
                EditorGUILayout.HelpBox(
                    $"Event will trigger at a random time between {startHour:0.00} h and {endHour:0.00} h.",
                    MessageType.Info
                );
                break;

            case TimeEventTriggerMode.AfterPeriod:
                float baseTrigger = startHour + period;

                if (!useRandomExtra)
                {
                    EditorGUILayout.HelpBox(
                        $"Event will trigger at: Start Hour ({startHour:0.00} h) + Period ({period:0.00} h) = {baseTrigger:0.00} h.",
                        MessageType.Info
                    );
                }
                else
                {
                    float minTrigger = baseTrigger + extraMin;
                    float maxTrigger = baseTrigger + extraMax;

                    EditorGUILayout.HelpBox(
                        $"Event will trigger between:\n" +
                        $"Min: Start ({startHour:0.00} h) + Period ({period:0.00} h) + ExtraMin ({extraMin:0.00} h) = {minTrigger:0.00} h\n" +
                        $"Max: Start ({startHour:0.00} h) + Period ({period:0.00} h) + ExtraMax ({extraMax:0.00} h) = {maxTrigger:0.00} h",
                        MessageType.Info
                    );
                }
                break;
        }

        // --- DAY PREVIEW ---

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Trigger Preview (Days)", EditorStyles.boldLabel);

        string dayMessage = "";

        // normalized description of StartDay
        string startDayDesc = startDay <= 0
            ? "Day 1 (no minimum start day is enforced)"
            : $"Day {startDay}";

        switch (freq)
        {
            case TimeEventFrequency.EveryDay:
                if (startDay <= 0)
                {
                    dayMessage =
                        "Event can be active every in-game day, starting from Day 1 (no minimum start day).\n" +
                        "Actual activation still depends on time window, allowed day parts, etc.";
                }
                else
                {
                    dayMessage =
                        $"Event can be active every in-game day, starting from Day {startDay}.\n" +
                        "Before that day, it will never be scheduled.";
                }
                break;

            case TimeEventFrequency.OnceEver:
                if (startDay > 0)
                {
                    dayMessage =
                        $"Event will only ever be allowed to occur ONCE, on exactly Day {startDay}.\n" +
                        "If conditions (time window, day part, etc.) are not met that day, it will never occur.";
                }
                else
                {
                    dayMessage =
                        "Event will occur only once in the entire game, on the FIRST day where all conditions match.\n" +
                        "Set Start Day Number > 0 if you want to lock it to a specific day.";
                }
                break;

            case TimeEventFrequency.EveryXDays:
                if (everyX <= 0)
                {
                    dayMessage =
                        "Every X Days is invalid (≤ 0). Event will not be scheduled correctly.\n" +
                        "Set X to at least 1.";
                }
                else
                {
                    if (startDay <= 0)
                    {
                        dayMessage =
                            $"Event can be active on days where (DayNumber % {everyX}) == 0, starting from Day 1.\n" +
                            $"Example: X = {everyX} → Day {everyX}, {everyX * 2}, {everyX * 3}, ...";
                    }
                    else
                    {
                        dayMessage =
                            $"Event can be active on days >= Day {startDay} where (DayNumber % {everyX}) == 0.\n" +
                            "So it will only appear on specific pattern days after the start day.";
                    }
                }
                break;

            case TimeEventFrequency.ChancePerDay:
                if (chance <= 0f)
                {
                    dayMessage =
                        "Daily Chance is 0: this event will never be active on any day.";
                }
                else if (chance >= 1f)
                {
                    if (startDay <= 0)
                    {
                        dayMessage =
                            "Daily Chance is 1: this behaves like EveryDay starting from Day 1.";
                    }
                    else
                    {
                        dayMessage =
                            $"Daily Chance is 1: this behaves like EveryDay starting from Day {startDay}.";
                    }
                }
                else
                {
                    if (startDay <= 0)
                    {
                        dayMessage =
                            $"From Day 1 onwards, each day has a {chance * 100f:0.#}% chance for this event to be active.\n" +
                            "If it is not active on a given day, it will not be scheduled at all that day.";
                    }
                    else
                    {
                        dayMessage =
                            $"From Day {startDay} onwards, each day has a {chance * 100f:0.#}% chance for this event to be active.\n" +
                            "Before that day, it will never be scheduled.";
                    }
                }
                break;
        }

        if (!string.IsNullOrEmpty(dayMessage))
        {
            EditorGUILayout.HelpBox(dayMessage, MessageType.Info);
        }
    }

    // ###########################
    // ---------- DEBUG ----------
    // ###########################
    private void DrawDebugDection()
    {
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);

        var evtObj = eventChannelProp.objectReferenceValue as GameEvent;

        if (evtObj == null)
        {
            EditorGUILayout.HelpBox(
                "No Event Channel assigned. Assign a GameEvent to test triggering.",
                MessageType.Info
            );
            return;
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Enter Play Mode to test the event. The GameEvent will only notify active listeners in Play Mode.",
                MessageType.None
            );
            return;
        }

        if (GUILayout.Button("Test Trigger Now"))
        {
            evtObj.Raise();
            Debug.Log($"[TimeEventDefinitionEditor] Manually triggered GameEvent from '{evtObj.name}'.");
        }
    }
}
