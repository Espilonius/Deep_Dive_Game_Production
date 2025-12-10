using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TimeSettings", menuName = "Day Night cycle/TimeSettings")]
public class TimeSettings : ScriptableObject
{    
    [SerializeField] private List<DayPartInfo> dayParts;

    [SerializeField] private Texture2D startingSkybox;
    [SerializeField] private Gradient startingGradient;

    [SerializeField]
    [Range(10, 360)]
    [Tooltip("How many in-game minutes are in one in-game hour.")]
    private int MinutesPerHour = 60;

    [SerializeField]
    [Range(4, 144)]
    [Tooltip("How many hours are in an in-game day.")]
    private int HoursPerDay = 24;

    [SerializeField]
    [Range(0f, 10f)]
    [Tooltip("Time speed: in-game minutes advanced per real-time second.")]
    private float timeMultiplier = 1f;

    public List<DayPartInfo> GetDayParts() => dayParts;
    public Texture2D GetStartingSkybox() => startingSkybox;
    public Gradient GetStartingGradient() => startingGradient;

    public int GetMinutesPerHour() => MinutesPerHour;
    public int GetHoursPerDay() => HoursPerDay;
    public float GetTimeMultiplier() => timeMultiplier;
}
