using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "TimeEvent", menuName = "Events/TimeEvent")]
public class TimeEvent : ScriptableObject
{
    public string EventId;

    [Tooltip("In what dayparts is this event allowed")]
    public List<DayPartInfo> AllowedDayParts;

    //Time window
    [Range(0f, 144f)]
    public float WindowStartHour;
    [Range(0f, 144f)]
    public float WindowEndHour;

    //Trigger mode
    public TimeEventTriggerMode TriggerMode;

    [Tooltip("For TriggerMode.AfterPeriod: after how much time")]
    public float PeriodDurationHours = 1f;

    [Tooltip("Extra random delay after the period")]
    public bool RandomAfterPeriod;
    public float RandomExtraMinHours;
    public float RandomExtraMaxHours;

    //Occurences
    public TimeEventOccurrenceMode OccurrenceMode = TimeEventOccurrenceMode.Single;
    public int MinOccurrences = 1;
    public int MaxOccurrences = 1;

    [Tooltip("Time between occurrences (for Multiple)")]
    public float MinIntervalBetweenOccurrencesMinutes = 0.1f;
    public float MaxIntervalBetweenOccurrencesMinutes = 0.5f;

    //Frequency
    public TimeEventFrequency Frequency = TimeEventFrequency.EveryDay;

    [Tooltip("Starting day to start repeating the event, or only trigger it once on the exact day")]
    public int StartDayNumber = 0;

    [Tooltip("Only used if EveryXDays")]
    public int EveryXDays = 1;

    [Tooltip("0–1 chance, only used if ChancePerDay")]
    [Range(0f, 1f)]
    public float DailyChance = 1f;

    //Event channel
    public GameEvent EventChannel;
}

public enum TimeEventTriggerMode
{
    AtFixedTime,
    RandomInWindow,
    AfterPeriod
}

public enum TimeEventOccurrenceMode
{
    Single,
    Multiple
}

public enum TimeEventFrequency
{
    EveryDay,
    OnceEver,
    EveryXDays,
    ChancePerDay
}