using System.Collections.Generic;
using UnityEngine;

public class TimeEventManager : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private List<TimeEvent> events;

    [SerializeField, HideInInspector]
    private List<TimeEventRuntimeState> _runtimeStates = new List<TimeEventRuntimeState>();

    public IReadOnlyList<TimeEventRuntimeState> RuntimeStates => _runtimeStates;

    public TimeManager TimeManager => timeManager;

    private void Awake()
    {
        _runtimeStates.Clear();
        foreach (var te in events)
        {
            _runtimeStates.Add(new TimeEventRuntimeState
            {
                timeEvent = te,
                LastScheduledDay = -1,
                IsActiveToday = false
            });
        }
    }

    private void Update()
    {
        foreach (var state in _runtimeStates)
        {
            UpdateEventState(state);
        }
    }

    private void UpdateEventState(TimeEventRuntimeState state)
    {
        var def = state.timeEvent;

        float currentTimeMinutes = timeManager.CurrentMinuteOfDay;
        int currentDay = timeManager.Days;
        int totalMinutesInDay = timeManager.TotalMinutesInDay;
        DayPartInfo currentPart = timeManager.CurrentDayPart;

        // Daypart check
        if (def.AllowedDayParts != null && def.AllowedDayParts.Count > 0)
        {
            if (!def.AllowedDayParts.Contains(currentPart))
                return;
        }

        // New day: plan again
        if (state.LastScheduledDay != currentDay)
        {
            state.IsActiveToday = false;

            state.LastScheduledDay = currentDay;

            if (TryScheduleForNewDay(state, totalMinutesInDay))
            {
                state.IsActiveToday = true;
            }
        }
        
        if (!state.IsActiveToday)
            return;

        if (def.OccurrenceMode == TimeEventOccurrenceMode.Single)
        {
            HandleSingleOccurrence(state, (int)currentTimeMinutes);
        }
        else
        {
            HandleMultipleOccurrences(state, (int)currentTimeMinutes);
        }
    }

    private bool TryScheduleForNewDay(TimeEventRuntimeState state, int totalMinutesInDay)
    {
        var def = state.timeEvent;

        int currentDayNumber = timeManager.Days + 1; // Day 1 = Days == 0

        if (def.StartDayNumber > 0 && currentDayNumber < def.StartDayNumber)
        {
            return false;
        }

        //frequency check
        switch (def.Frequency)
        {
            case TimeEventFrequency.OnceEver:
                if (state.HasTriggeredOnceEver)
                    return false;

                if (def.StartDayNumber > 0 && currentDayNumber != def.StartDayNumber)
                    return false;
                break;

            case TimeEventFrequency.EveryXDays:
                if (def.EveryXDays <= 0)
                    def.EveryXDays = 1;

                if (currentDayNumber % def.EveryXDays != 0)
                    return false;
                break;

            case TimeEventFrequency.ChancePerDay:
                if (Random.value > def.DailyChance)
                    return false;
                break;

            case TimeEventFrequency.EveryDay:
            default:

                break;
        }

        //als de code dit punt bereikt mag het event uitgevoerd worden op de dag
        state.OccurrencesTriggered = 0;

        if (def.OccurrenceMode == TimeEventOccurrenceMode.Multiple)
        {
            state.TargetOccurrences = Random.Range(def.MinOccurrences, def.MaxOccurrences + 1);
        }
        else
        {
            state.TargetOccurrences = 1;
        }

        int windowStartMin = HourToMinutes(def.WindowStartHour);
        int windowEndMin = HourToMinutes(def.WindowEndHour);

        switch (def.TriggerMode)
        {
            case TimeEventTriggerMode.AtFixedTime:
                state.NextTriggerMinuteOfDay = windowStartMin;
                break;

            case TimeEventTriggerMode.RandomInWindow:
                state.NextTriggerMinuteOfDay = Random.Range(windowStartMin, windowEndMin + 1);
                break;

            case TimeEventTriggerMode.AfterPeriod:
                float baseMinutes = windowStartMin + def.PeriodDurationHours * 60f;
                if (def.RandomAfterPeriod)
                {
                    float extra = Random.Range(def.RandomExtraMinHours, def.RandomExtraMaxHours) * 60f;
                    baseMinutes += extra;
                }
                state.NextTriggerMinuteOfDay = Mathf.RoundToInt(baseMinutes) % totalMinutesInDay;
                break;
        }
        return true;
    }

    private void HandleSingleOccurrence(TimeEventRuntimeState state, int currentMinuteOfDay)
    {
        var def = state.timeEvent;

        int windowStartMin = HourToMinutes(def.WindowStartHour);
        int windowEndMin = HourToMinutes(def.WindowEndHour);

        if (currentMinuteOfDay < windowStartMin || currentMinuteOfDay > windowEndMin)
            return;

        if (state.OccurrencesTriggered >= 1)
            return;

        if (currentMinuteOfDay >= state.NextTriggerMinuteOfDay)
        {
            TriggerEvent(state);
            state.OccurrencesTriggered = 1;
        }
    }

    private void HandleMultipleOccurrences(TimeEventRuntimeState state, int currentMinuteOfDay)
    {
        var def = state.timeEvent;

        int windowStartMin = HourToMinutes(def.WindowStartHour);
        int windowEndMin = HourToMinutes(def.WindowEndHour);

        if (currentMinuteOfDay < windowStartMin || currentMinuteOfDay > windowEndMin)
            return;

        if (state.OccurrencesTriggered >= state.TargetOccurrences)
            return;

        if (currentMinuteOfDay >= state.NextTriggerMinuteOfDay)
        {
            TriggerEvent(state);
            state.OccurrencesTriggered++;

            if (state.OccurrencesTriggered < state.TargetOccurrences)
            {
                float interval = Random.Range(
                    def.MinIntervalBetweenOccurrencesMinutes,
                    def.MaxIntervalBetweenOccurrencesMinutes
                );
                state.NextTriggerMinuteOfDay = currentMinuteOfDay + Mathf.RoundToInt(interval);
            }
        }
    }

    private void TriggerEvent(TimeEventRuntimeState state)
    {
        var def = state.timeEvent;
        Debug.Log($"TimeEvent triggered: {def.EventId}");

        if (def.EventChannel != null)
            def.EventChannel.Raise();

        if (def.Frequency == TimeEventFrequency.OnceEver)
            state.HasTriggeredOnceEver = true;
    }

    private int HourToMinutes(float hourValue)
    {
        return Mathf.RoundToInt(hourValue * timeManager.MinutesPerHour);
    }
}

[System.Serializable]
public class TimeEventRuntimeState
{
    public TimeEvent timeEvent;
    public int LastScheduledDay = -1;

    public int TargetOccurrences;
    public int OccurrencesTriggered;

    public int NextTriggerMinuteOfDay;

    public bool HasTriggeredOnceEver;

    public bool IsActiveToday;
}

