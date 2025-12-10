using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private TimeSettings setTimeSettings;
    [SerializeField] private Light globalLight;

    private int minutes;
    public int Minutes {  get { return minutes; } set { minutes = value; OnMinutesChange(value); } }

    private int hours;
    public int Hours { get { return hours; } set { hours = value; OnHoursChange(value); } }

    private int days;
    public int Days { get { return days; } set { days = value; /*OnDaysChange(value);*/ } }

    private float tempSecond;
    private Texture2D currentSkybox;
    private Gradient currentGradient;

    public DayPartInfo CurrentDayPart { get; private set; }

    public int MinutesPerHour => setTimeSettings.GetMinutesPerHour();
    public int HoursPerDay => setTimeSettings.GetHoursPerDay();
    public int TotalMinutesInDay => MinutesPerHour * HoursPerDay;
    public int CurrentMinuteOfDay => Hours * MinutesPerHour + Minutes;

    public string CurrentTimeString => $"Day {Days + 1} | {Hours:00}:{Minutes:00}";

    public TimeSettings Settings => setTimeSettings;

    private void Start()
    {
        currentSkybox = setTimeSettings.GetStartingSkybox();
        currentGradient = setTimeSettings.GetStartingGradient();

        StartCoroutine(LerpSkybox(currentSkybox, currentSkybox, 10f));
        StartCoroutine(LerpLight(CopyLastColorToFirst(currentGradient, currentGradient), 10f));

        CurrentDayPart = FindCurrentDayPart(Hours);
    }
    void Update()
    {
        TimeChange();
    }

    private void TimeChange()
    {
        tempSecond += Time.deltaTime * setTimeSettings.GetTimeMultiplier();

        if (tempSecond >= 1)
        {
            Minutes += 1;
            tempSecond = 0;
        }
    }
    private void ConsoleLogTime()
    {
        Debug.Log("Minutes:" + minutes + ".  Hours: " + hours + ".  Days: " + days + ".");
    }

    private void OnMinutesChange(int value)
    {
        ConsoleLogTime();
        globalLight.transform.Rotate(Vector3.up, (1f / ((float)setTimeSettings.GetMinutesPerHour() * (float)setTimeSettings.GetHoursPerDay())) * 360f, Space.World);//standard is to do 1 / 1440, this is the total minutes in a normal day
        if (value >= setTimeSettings.GetMinutesPerHour())
        {
            Hours++;
            minutes = 0;
        }
        if (Hours >= setTimeSettings.GetHoursPerDay())
        {
            Hours = 0;
            Days++;
        }
    }
    private void OnHoursChange(int value)
    {
        foreach (DayPartInfo daypart in setTimeSettings.GetDayParts())
        {
            if (daypart.DayPartStart == value)
            {
                Debug.Log(daypart.DayPartName);
                StartCoroutine(LerpSkybox(currentSkybox, daypart.DayPartSkybox, daypart.DaypartTransitionTime));
                StartCoroutine(LerpLight(CopyLastColorToFirst(daypart.DayPartGradient, currentGradient), daypart.DaypartTransitionTime));

                currentSkybox = daypart.DayPartSkybox;
                currentGradient = daypart.DayPartGradient;

                CurrentDayPart = daypart;
                break;
            }
        }
    }
    private void OnDaysChange(int value)
    {
        throw new NotImplementedException();
    }

    private IEnumerator LerpSkybox(Texture2D a, Texture2D b, float time)
    {
        RenderSettings.skybox.SetTexture("_Texture1", a);
        RenderSettings.skybox.SetTexture("_Texture2", b);
        RenderSettings.skybox.SetFloat("_Blend", 0);
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            RenderSettings.skybox.SetFloat("_Blend", i / time);
            yield return null;
        }
        RenderSettings.skybox.SetTexture("_Texture1", b);
    }

    private IEnumerator LerpLight(Gradient lightGradient, float time)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            globalLight.color = lightGradient.Evaluate(i / time);
            yield return null;
        }
    }

    public Gradient CopyLastColorToFirst(Gradient newGradient, Gradient oldGradient)
    {
        if (oldGradient == null) return newGradient;
        var colorKeys = oldGradient.colorKeys;

        if (colorKeys.Length < 2) return newGradient;

        Color lastColor = colorKeys[colorKeys.Length - 1].color;

        newGradient.colorKeys[0].color = lastColor;

        return newGradient;
    }

    private DayPartInfo FindCurrentDayPart(int hour)
    {
        foreach (DayPartInfo daypart in setTimeSettings.GetDayParts())
        {
            if (daypart.DayPartStart == hour)
                return daypart;
        }
        return setTimeSettings.GetDayParts().Count > 0 ? setTimeSettings.GetDayParts()[0] : default;
    }

    // ###########################
    // ---------- Debug ----------
    // ###########################
    public void DebugSetTime(int day, int hour, int minute)
    {
        day = Mathf.Max(0, day);
        hour = Mathf.Clamp(hour, 0, HoursPerDay - 1);
        minute = Mathf.Clamp(minute, 0, MinutesPerHour - 1);

        days = day;
        hours = hour;
        minutes = minute;

        ConsoleLogTime();

        float totalMinutes = TotalMinutesInDay;
        float currentMinutes = CurrentMinuteOfDay;
        float t = totalMinutes > 0f ? currentMinutes / totalMinutes : 0f;

        globalLight.transform.rotation = Quaternion.Euler(0f, t * 360f, 0f);

        OnHoursChange(hours);
    }

    public void DebugJumpToDayPart(DayPartInfo dp)
    {
        if (dp == null) return;

        DebugSetTime(Days, dp.DayPartStart, 0);
    }
}